//////////////////////////////////////////////////////////////////////////////////////////////
// Depth buffer reduction into a half resolution render target and its first 3 mips, so we have in total the 1/2, 1/4, 1/8 and 1/16 sizes at our disposition...
// We store the average, min and max linear Z in the X, Y, Z components of the target respectively. W is unused.
//
//////////////////////////////////////////////////////////////////////////////////////////////
//
#include "Includes/Global.hlsl"

#define	NUMTHREADX	8	// 8x8 threads per group, but each thread reads 4 pixels
#define	NUMTHREADY	8

cbuffer CB_Downsample : register(b2) {
	uint2	_depthBufferSize;
};

Texture2D<float>	_texDepthBuffer				: register(t0);	// SRV, linear depth
RWTexture2D<float4>	_downsampledDepthBufferMip0	: register(u0);	// UAV, half-res average/min/max depth
RWTexture2D<float4>	_downsampledDepthBufferMip1	: register(u1);	// UAV, quarter-res average/min/max depth
RWTexture2D<float4>	_downsampledDepthBufferMip2	: register(u2);	// UAV, 1/8th average/min/max depth
RWTexture2D<float4>	_downsampledDepthBufferMip3	: register(u3);	// UAV, 1/16th average/min/max depth


float	ComputeLinearZ( float _projZ ) {
	float	temp = _Proj2Camera[2].w * _projZ + _Proj2Camera[3].w;
	return _projZ / temp;
}

//////////////////////////////////////////////////////////////////////////////////////////////
// Actual downsampling
//#define	USE_16_16_BUFFER	// Undefine this to reveal nasty flickering bug!
#ifdef USE_16_16_BUFFER
	groupshared float4	gs_Samples[16 * 16];	// Apparently, storing in too large a shared array fucks everything up!
#else
	groupshared float4	gs_Samples[NUMTHREADX * NUMTHREADY];
#endif

[numthreads( NUMTHREADX, NUMTHREADY, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	maxPos = _depthBufferSize-1;
	uint2	position00 = 2U * _DispatchThreadID.xy;				// Pixel position
			position00 = min( position00, maxPos );				// Clamp to avoid fetching crap
	uint2	position11 = min( position00+1, maxPos );			// Clamp to avoid fetching crap
	uint2	position01 = uint2( position11.x, position00.y );
	uint2	position10 = uint2( position00.x, position11.y );

	// ======================================================================
	// Full-res sampling
//	float4	linearZ = _texDepthBuffer.Gather( PointClamp, 0.0, position00 );		// Original size linear Z
	float4	linearZ = _texDepthBuffer.Gather( PointClamp, float2( position00 ) / _depthBufferSize );		// Original size linear Z

// 	float4	linearZ;
// 	linearZ.x = _texDepthBuffer.Load( uint3( position00, 0 ) );		// Original size linear Z
// 	linearZ.y = _texDepthBuffer.Load( uint3( position01, 0 ) );		// Original size linear Z
// 	linearZ.z = _texDepthBuffer.Load( uint3( position11, 0 ) );		// Original size linear Z
// 	linearZ.w = _texDepthBuffer.Load( uint3( position10, 0 ) );		// Original size linear Z

	// ======================================================================
	// Build thread indices so each thread addresses groups of 2x2 pixels in shared memory
	uint2	threadPos = _GroupThreadID.xy;	// Thread position within group
	uint	threadIndex = NUMTHREADY * threadPos.y + threadPos.x;

	uint	threadIndex00 = threadIndex << 1;
	uint	threadIndex01 = threadIndex00 + 1;			// Right pixel
	uint	threadIndex11 = threadIndex01 + NUMTHREADX;	// Bottom-Right pixel
	uint	threadIndex10 = threadIndex11 - 1;			// Bottom pixel

	#ifdef USE_16_16_BUFFER
		// Write to shared memory for gathering later
		gs_Samples[threadIndex00] = linearZ.w;
		gs_Samples[threadIndex01] = linearZ.z;
		gs_Samples[threadIndex10] = linearZ.x;
		gs_Samples[threadIndex11] = linearZ.y;

		GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available
	#endif

 	// ======================================================================
	// Compute mip 0 (half res of original buffer)
	// This is the first mip for which we can build average, min and max Z values...
	{
		// Threads in range [{0,0},{8,8}[ will each span 4 samples in range [{0,0},{16,16}[
		#ifdef USE_16_16_BUFFER
			float4	ZAvgMinMax00 = gs_Samples[threadIndex00];
			float4	ZAvgMinMax01 = gs_Samples[threadIndex01];
			float4	ZAvgMinMax11 = gs_Samples[threadIndex10];
			float4	ZAvgMinMax10 = gs_Samples[threadIndex11];
		#else
			float4	ZAvgMinMax00 = linearZ.x;
			float4	ZAvgMinMax01 = linearZ.y;
			float4	ZAvgMinMax11 = linearZ.z;
			float4	ZAvgMinMax10 = linearZ.w;
		#endif

		float4	ZAvgMinMax;
		ZAvgMinMax.x = 0.25 * (ZAvgMinMax00.x + ZAvgMinMax01.x + ZAvgMinMax10.x + ZAvgMinMax11.x);
		ZAvgMinMax.y = min( min( min( ZAvgMinMax00.y, ZAvgMinMax01.y ), ZAvgMinMax10.y ), ZAvgMinMax11.y );
		ZAvgMinMax.z = max( max( max( ZAvgMinMax00.z, ZAvgMinMax01.z ), ZAvgMinMax10.z ), ZAvgMinMax11.z );
		ZAvgMinMax.w = 0.0;

		// Store into first mip
		uint2	targetPos = 8U * _GroupID.xy + threadPos;
		_downsampledDepthBufferMip0[targetPos] = ZAvgMinMax;

		// Also store in shared memory for further downsampling
		gs_Samples[threadIndex] = ZAvgMinMax;
	}

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

 	// ======================================================================
	// Compute mip 1 (quarter res of original buffer)
	if ( all( threadPos < 4U ) ) {
		// Threads in range [{0,0},{4,4}[ will each span 4 samples in range [{0,0},{8,8}[
		float4	ZAvgMinMax00 = gs_Samples[threadIndex00];
		float4	ZAvgMinMax01 = gs_Samples[threadIndex01];
		float4	ZAvgMinMax11 = gs_Samples[threadIndex11];
		float4	ZAvgMinMax10 = gs_Samples[threadIndex10];

		float4	ZAvgMinMax;
		ZAvgMinMax.x = 0.25 * (ZAvgMinMax00.x + ZAvgMinMax01.x + ZAvgMinMax10.x + ZAvgMinMax11.x);
		ZAvgMinMax.y = min( min( min( ZAvgMinMax00.y, ZAvgMinMax01.y ), ZAvgMinMax10.y ), ZAvgMinMax11.y );
		ZAvgMinMax.z = max( max( max( ZAvgMinMax00.z, ZAvgMinMax01.z ), ZAvgMinMax10.z ), ZAvgMinMax11.z );
		ZAvgMinMax.w = 0.0;

		// Store into second mip
		uint2	targetPos = 4U * _GroupID.xy + threadPos;
		_downsampledDepthBufferMip1[targetPos] = ZAvgMinMax;

		// Also store in shared memory for further downsampling
		gs_Samples[threadIndex] = ZAvgMinMax;
	}

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

 	// ======================================================================
	// Compute mip 2 (1/8th res of original buffer)
	if ( all( threadPos < 2U ) ) {
		// Threads in range [{0,0},{2,2}[ will each span 4 samples in range [{0,0},{4,4}[
		float4	ZAvgMinMax00 = gs_Samples[threadIndex00];
		float4	ZAvgMinMax01 = gs_Samples[threadIndex01];
		float4	ZAvgMinMax11 = gs_Samples[threadIndex11];
		float4	ZAvgMinMax10 = gs_Samples[threadIndex10];

		float4	ZAvgMinMax;
		ZAvgMinMax.x = 0.25 * (ZAvgMinMax00.x + ZAvgMinMax01.x + ZAvgMinMax10.x + ZAvgMinMax11.x);
		ZAvgMinMax.y = min( min( min( ZAvgMinMax00.y, ZAvgMinMax01.y ), ZAvgMinMax10.y ), ZAvgMinMax11.y );
		ZAvgMinMax.z = max( max( max( ZAvgMinMax00.z, ZAvgMinMax01.z ), ZAvgMinMax10.z ), ZAvgMinMax11.z );
		ZAvgMinMax.w = 0.0;

		// Store into third mip
		uint2	targetPos = 2U * _GroupID.xy + threadPos;
		_downsampledDepthBufferMip2[targetPos] = ZAvgMinMax;

		// Also store in shared memory for further downsampling
		gs_Samples[threadIndex] = ZAvgMinMax;
	}

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

 	// ======================================================================
	// Compute mip 3 (1/16th res of original buffer)
	if ( threadIndex == 0 ) {
		// Thread 0 will each span 4 samples in range [{0,0},{2,2}[
		float4	ZAvgMinMax00 = gs_Samples[threadIndex00];
		float4	ZAvgMinMax01 = gs_Samples[threadIndex01];
		float4	ZAvgMinMax11 = gs_Samples[threadIndex11];
		float4	ZAvgMinMax10 = gs_Samples[threadIndex10];

		float4	ZAvgMinMax;
		ZAvgMinMax.x = 0.25 * (ZAvgMinMax00.x + ZAvgMinMax01.x + ZAvgMinMax10.x + ZAvgMinMax11.x);
		ZAvgMinMax.y = min( min( min( ZAvgMinMax00.y, ZAvgMinMax01.y ), ZAvgMinMax10.y ), ZAvgMinMax11.y );
		ZAvgMinMax.z = max( max( max( ZAvgMinMax00.z, ZAvgMinMax01.z ), ZAvgMinMax10.z ), ZAvgMinMax11.z );
		ZAvgMinMax.w = 0.0;

		// Store into fourth mip
		_downsampledDepthBufferMip3[_GroupID.xy] = ZAvgMinMax;
	}
}
