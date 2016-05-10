//////////////////////////////////////////////////////////////////////////////////////////////
// Depth buffer reduction into a half resolution render target and its first 3 mips, so we have in total the 1/2, 1/4, 1/8 and 1/16 sizes at our disposition...
//
// We store the average, min and max linear Z in the X, Y, Z components of the target respectively. W is unused.
//
//////////////////////////////////////////////////////////////////////////////////////////////
//
#include "Includes/Global.hlsl"

#define	NUMTHREADX	16
#define	NUMTHREADY	16

cbuffer CB_Downsample : register(b2) {
	uint2	_depthBufferSize;
};

Texture2D<float4>	_texDepthBuffer				: register(t0);	// SRV, projected depth
RWTexture2D<float>	_texDepthBufferUAV			: register(u0);	// UAV, full-res linear depth in [ZNear, ZFar]
RWTexture2D<float4>	_downsampledDepthBufferMip0	: register(u1);	// UAV, half-res average/min/max depth
RWTexture2D<float4>	_downsampledDepthBufferMip1	: register(u2);	// UAV, quarter-res average/min/max depth
RWTexture2D<float4>	_downsampledDepthBufferMip2	: register(u3);	// UAV, 1/8th average/min/max depth
RWTexture2D<float4>	_downsampledDepthBufferMip3	: register(u4);	// UAV, 1/16th average/min/max depth

float	ComputeLinearZ( float _projZ ) {
	float	temp = _Proj2Camera[2].w * _projZ + _Proj2Camera[3].w;
	return _projZ / temp;
}

groupshared float4	gs_Samples[NUMTHREADX * NUMTHREADY];

[numthreads( NUMTHREADX, NUMTHREADY, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint2	position = _DispatchThreadID.xy;				// Pixel position
			position = min( position, _depthBufferSize-1 );	// Clamp to avoid fetching crap

	// Linearizes the projected [0,1] Z coordinates to [Znear, Zfar] range
	float	rawZ = _texDepthBuffer.Load( uint3( position, 0 ) ).w;		// Depth is stored in the W component
	float4	linearZ = float4( 0.0, ComputeLinearZ( rawZ ).xx, 0.0 );	// Notice that X component is not available until we make sure we're in image range
	if ( all( _DispatchThreadID.xy < _depthBufferSize ) ) {
		// Only write if we're in range
		linearZ.x = linearZ.y;											// Make X component available for averaging, so out of range values are only accounted for min/max-ing, but not averaging
		_texDepthBufferUAV[position] = linearZ.x;
	}

 	// ======================================================================
	// Build thread indices so each thread addresses groups of 2x2 pixels in shared memory
	uint2	threadPos = _GroupThreadID.xy;	// Thread position within group
	uint	threadIndex = NUMTHREADY * threadPos.y + threadPos.x;

	uint	threadIndex00 = threadIndex << 1;
	uint	threadIndex01 = threadIndex00 + 1;			// Right pixel
	uint	threadIndex11 = threadIndex01 + NUMTHREADX;	// Bottom-Right pixel
	uint	threadIndex10 = threadIndex11 - 1;			// Bottom pixel

	gs_Samples[threadIndex] = linearZ;	// Write to shared memory for gathering later

//gs_Samples[threadIndex] = float4( threadPos, 0, 0 );

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

 	// ======================================================================
	// Compute mip 0 (half res of original buffer)
	// This is the first mip for which we can build average, min and max Z values...
	if ( all( threadPos < 8 ) ) {
		// Threads in range { [0,8[, [0,8[ } will each span 4 samples in range { [0,16[, [0,16[ }
		float4	ZAvgMinMax00 = gs_Samples[threadIndex00];
		float4	ZAvgMinMax01 = gs_Samples[threadIndex01];
		float4	ZAvgMinMax11 = gs_Samples[threadIndex11];
		float4	ZAvgMinMax10 = gs_Samples[threadIndex10];

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
	if ( all( threadPos < 4 ) ) {
		// Threads in range { [0,4[, [0,4[ } will each span 4 samples in range { [0,8[, [0,8[ }
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
	if ( all( threadPos < 2 ) ) {
		// Threads in range { [0,2[, [0,2[ } will each span 4 samples in range { [0,4[, [0,4[ }
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
		// Thread 0 will each span 4 samples in range { [0,2[, [0,2[ }
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
