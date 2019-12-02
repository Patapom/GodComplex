//////////////////////////////////////////////////////////////////////////
// This shader downsamples the depth-buffer into 1/2, 1/4 and 1/8 resolutions
// Inspired from http://mynameismjp.wordpress.com/2011/08/10/average-luminance-compute-shader/
//
#include "Inc/Global.hlsl"

// cbuffer	cbCompute	: register( b10 )
// {
// 	float2	_UnprojectParms;
// };

struct	CS_IN
{
	uint3	GroupID			: SV_GroupID;			// Defines the group offset within a Dispatch call, per dimension of the dispatch call
	uint3	ThreadID		: SV_DispatchThreadID;	// Defines the global thread offset within the Dispatch call, per dimension of the group
	uint3	GroupThreadID	: SV_GroupThreadID;		// Defines the thread offset within the group, per dimension of the group
	uint	GroupIndex		: SV_GroupIndex;		// Provides a flattened index for a given thread within a given group
};

Texture2D<float>	_TexDepthBuffer : register(t10);
RWTexture2D<float4>	_TexDownsampledDepthMip0 : register(u0);
RWTexture2D<float4>	_TexDownsampledDepthMip1 : register(u1);
RWTexture2D<float4>	_TexDownsampledDepthMip2 : register(u2);


float	ReadDepth( uint2 _PixelIndex )
{
	float	Zproj = _TexDepthBuffer[_PixelIndex];

	float	Q = _CameraData.w / (_CameraData.w - _CameraData.z);	// Zf / (Zf-Zn)
	return (Q * _CameraData.z) / (Q - Zproj);
}

groupshared float4	SamplesMip0[4*4];
groupshared float4	SamplesMip1[2*2];

[numthreads( 4, 4, 1 )]
void	CS( CS_IN _In )
{
	//////////////////////////////////////////////////////////////////////////
	// Perform first downsampling
	{
		uint2	PixelIndex = 2 * (4*_In.GroupID.xy + _In.GroupThreadID.xy);
		float4	Samples = float4(	ReadDepth( PixelIndex ),
									ReadDepth( PixelIndex + uint2( 1, 0 ) ),
									ReadDepth( PixelIndex + uint2( 0, 1 ) ),
									ReadDepth( PixelIndex + uint2( 1, 1 ) )
								);

		float4	ZAvgMinMax = 0.0;
		ZAvgMinMax.x = 0.25 * (Samples.x + Samples.y + Samples.z + Samples.w);
		ZAvgMinMax.y = min( min( min( Samples.x, Samples.y), Samples.z), Samples.w );
		ZAvgMinMax.z = max( max( max( Samples.x, Samples.y), Samples.z), Samples.w );

		_TexDownsampledDepthMip0[PixelIndex/2] = ZAvgMinMax;	// Store to mip 0
		SamplesMip0[4*_In.GroupThreadID.y+_In.GroupThreadID.x] = ZAvgMinMax;			// But also to thread group for further processing
	}

 	GroupMemoryBarrierWithGroupSync();

	//////////////////////////////////////////////////////////////////////////
	// Perform second downsampling
	if ( _In.GroupThreadID.x < 2 && _In.GroupThreadID.y < 2 )
	{
		uint	PixelIndex = 2 * (2 * _In.GroupThreadID.y + _In.GroupThreadID.x);
		float4	ZAvgMinMax00 = SamplesMip0[PixelIndex];
		float4	ZAvgMinMax01 = SamplesMip0[PixelIndex+1];
		float4	ZAvgMinMax10 = SamplesMip0[PixelIndex+4];
		float4	ZAvgMinMax11 = SamplesMip0[PixelIndex+4+1];

		float4	ZAvgMinMax = 0.0;
		ZAvgMinMax.x = 0.25 * (ZAvgMinMax00.x + ZAvgMinMax01.x + ZAvgMinMax10.x + ZAvgMinMax11.x);
		ZAvgMinMax.y = min( min( min( ZAvgMinMax00.y, ZAvgMinMax01.y), ZAvgMinMax10.y), ZAvgMinMax11.y );
		ZAvgMinMax.z = max( max( max( ZAvgMinMax00.z, ZAvgMinMax01.z), ZAvgMinMax10.z), ZAvgMinMax11.z );

		uint2	TargetPixelIndex = 2*_In.GroupID.xy + _In.GroupThreadID.xy;
		_TexDownsampledDepthMip1[TargetPixelIndex] = ZAvgMinMax;	// Store to mip 1
		SamplesMip1[2*_In.GroupThreadID.y+_In.GroupThreadID.x] = ZAvgMinMax;			// But also to thread group for further processing
	}

 	GroupMemoryBarrierWithGroupSync();

	//////////////////////////////////////////////////////////////////////////
	// Perform final downsampling
	if ( _In.GroupThreadID.x == 0 && _In.GroupThreadID.y == 0 )
	{
		float4	ZAvgMinMax00 = SamplesMip1[0];
		float4	ZAvgMinMax01 = SamplesMip1[1];
		float4	ZAvgMinMax10 = SamplesMip1[2];
		float4	ZAvgMinMax11 = SamplesMip1[3];

		float4	ZAvgMinMax = 0.0;
		ZAvgMinMax.x = 0.25 * (ZAvgMinMax00.x + ZAvgMinMax01.x + ZAvgMinMax10.x + ZAvgMinMax11.x);
		ZAvgMinMax.y = min( min( min( ZAvgMinMax00.y, ZAvgMinMax01.y), ZAvgMinMax10.y), ZAvgMinMax11.y );
		ZAvgMinMax.z = max( max( max( ZAvgMinMax00.z, ZAvgMinMax01.z), ZAvgMinMax10.z), ZAvgMinMax11.z );

		uint2	TargetPixelIndex = _In.GroupID.xy;
		_TexDownsampledDepthMip2[TargetPixelIndex] = ZAvgMinMax;	// Store to mip 2
	}
}
