////////////////////////////////////////////////////////////////////////////////
// Splits a full-res texture into 4x4 quarter res textures
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

#define THREADS_X	8
#define THREADS_Y	8

Texture2D< float >			_tex_sourceFloat1 : register(t0);
Texture2D< float3 >			_tex_sourceFloat3 : register(t1);
Texture2DArray< float3 >	_tex_sourceFloat3_Mip : register(t2);
RWTexture2DArray< float >	_tex_targetFloat1 : register(u0);
RWTexture2DArray< float3 >	_tex_targetFloat3 : register(u1);
RWTexture2DArray< float2 >	_tex_targetFloat2 : register(u2);

cbuffer CB_DownSample : register( b3 ) {
	uint2	_targetSize;
	uint	_passIndex;
};


////////// GOD DAMN IT! //////////
// Instead of each thread sampling 16 pixels, I had to dumb down the code to 16 draw calls, each thread sampling only 1 sample
// The fucking compiler can't seem to compile properly!!!
//
[numthreads( THREADS_X, THREADS_Y, 1 )]
void	CS_SplitFloat1( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID, uint3 _dispatchThreadID : SV_dispatchThreadID ) {
	uint3	targetPixelIndex = uint3( _dispatchThreadID.xy, _passIndex );
	uint2	sourcePixelIndex = _dispatchThreadID.xy << 2;
			sourcePixelIndex.x += _passIndex & 3;
			sourcePixelIndex.y += _passIndex >> 2;
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];
}

[numthreads( THREADS_X, THREADS_Y, 1 )]
void	CS_SplitFloat3( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID, uint3 _dispatchThreadID : SV_dispatchThreadID ) {
	uint3	targetPixelIndex = uint3( _dispatchThreadID.xy, _passIndex );
	uint2	sourcePixelIndex = _dispatchThreadID.xy << 2;
			sourcePixelIndex.x += _passIndex & 3;
			sourcePixelIndex.y += _passIndex >> 2;
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];
}

[numthreads( THREADS_X, THREADS_Y, 1 )]
void	CS_SplitNormal( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID, uint3 _dispatchThreadID : SV_dispatchThreadID ) {
	uint3	targetPixelIndex = uint3( _dispatchThreadID.xy, _passIndex );
	uint2	sourcePixelIndex = _dispatchThreadID.xy << 2;
			sourcePixelIndex.x += _passIndex & 3;
			sourcePixelIndex.y += _passIndex >> 2;

//	// Convert world-space normal into local camera-space
//	float3	wsNormal = normalize( _tex_sourceFloat3[sourcePixelIndex] );
//
//	float3	csView = normalize( BuildCameraRay( float2(sourcePixelIndex + 0.5) / _resolution ) );
//	float3	wsView = mul( float4( csView, 0.0 ), _camera2World ).xyz;
//	float3	wsRight = normalize( cross( wsView, _camera2World[1].xyz ) );
//	float3	wsUp = cross( wsRight, wsView );
//	float3	wsAt = -wsView;
//
//	float3	N = float3( dot( wsNormal, wsRight ), dot( wsNormal, wsUp ), dot( wsNormal, wsAt ) );

	// Now stored in camera space
	float3	N = _tex_sourceFloat3[sourcePixelIndex];

	// At this point, we know N.z > 0
	// We simply need to store the XY part into the R8G8_SNORM target...
	_tex_targetFloat2[targetPixelIndex] = N.xy;
}
//
////////// GOD DAMN IT! //////////



////////////////////////////////////////////////////////////////////////////////
// Downsample split radiance
[numthreads( THREADS_X, THREADS_Y, 1 )]
void	CS_DownSample( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID, uint3 _dispatchThreadID : SV_dispatchThreadID ) {
	uint3	targetPixelIndex = uint3( _dispatchThreadID.xy, _passIndex );
	if ( any( targetPixelIndex.xy >= _targetSize ) )
		return;

	float2	UV = float2( targetPixelIndex.xy + 0.25 ) / _targetSize;
	float2	dUV = 0.5 / _targetSize;
	float3	V00 = _tex_sourceFloat3_Mip.SampleLevel( PointClamp, float3( UV, _passIndex ), 0.0 );	UV.x += dUV.x;
	float3	V10 = _tex_sourceFloat3_Mip.SampleLevel( PointClamp, float3( UV, _passIndex ), 0.0 );	UV.y += dUV.y;
	float3	V11 = _tex_sourceFloat3_Mip.SampleLevel( PointClamp, float3( UV, _passIndex ), 0.0 );	UV.x -= dUV.x;
	float3	V01 = _tex_sourceFloat3_Mip.SampleLevel( PointClamp, float3( UV, _passIndex ), 0.0 );

	_tex_targetFloat3[targetPixelIndex] = 0.25 * (V00 + V10 + V01 + V11);
}


/*
[numthreads( THREADS_X, THREADS_Y, 1 )]
void	CS_SplitFloat1_FUCKING_COMPILER_BUG( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID, uint3 _dispatchThreadID : SV_dispatchThreadID ) {
	uint3	targetPixelIndex = uint3( _dispatchThreadID.xy, 0 );
	uint3	sourcePixelIndex = uint3( _dispatchThreadID.xy << 2, 0 );

#if 0
	// First line
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x-=3; sourcePixelIndex.y++; targetPixelIndex.z++;

	// Second line
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x-=3; sourcePixelIndex.y++; targetPixelIndex.z++;

	// Third line
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x-=3; sourcePixelIndex.y++; targetPixelIndex.z++;

	// Fourth line
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat1[targetPixelIndex] = _tex_sourceFloat1[sourcePixelIndex];	sourcePixelIndex.x-=3; sourcePixelIndex.y++; targetPixelIndex.z++;
#elif 0
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*0+0 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 0, 0 )];
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*0+1 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 1, 0 )];
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*0+2 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 2, 0 )];
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*0+3 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 3, 0 )];
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*1+0 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 0, 1 )];
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*1+1 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 1, 1 )];
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*1+2 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 2, 1 )];
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*1+3 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 3, 1 )];
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*2+0 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 0, 2 )];
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*2+1 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 1, 2 )];
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*2+2 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 2, 2 )];
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*2+3 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 3, 2 )];
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*3+0 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 0, 3 )];
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*3+1 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 1, 3 )];
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*3+2 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 2, 3 )];
	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*3+3 )] = _tex_sourceFloat1[sourcePixelIndex + uint2( 3, 3 )];
#else
	[loop]
	[fastopt]
	[allow_uav_condition]
	for ( uint Y=0; Y < 4; Y++ ) {
		[loop]
		[fastopt]
		[allow_uav_condition]
		for ( uint X=0; X < 4; X++ ) {
			float	V;
			_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*Y+X )] = _tex_sourceFloat1.Load( sourcePixelIndex + uint3( X, Y, 0 ) );
//			_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*Y+X )] = _tex_sourceFloat1[sourcePixelIndex.xy + uint2( X, Y )];
		}
	}
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*0+0 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 0, 0 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*0+1 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 0, 1 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*0+1 )] = _tex_sourceFloat1.Load( sourcePixelIndex + uint3( 0, 0, 0 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*0+2 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 2, 0 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*0+3 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 3, 0 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*1+0 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 0, 1 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*1+1 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 1, 1 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*1+2 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 2, 1 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*1+3 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 3, 1 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*2+0 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 0, 2 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*2+1 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 1, 2 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*2+2 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 2, 2 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*2+3 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 3, 2 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*3+0 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 0, 3 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*3+1 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 1, 3 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*3+2 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 2, 3 ) );
//	_tex_targetFloat1[uint3( targetPixelIndex.xy, 4*3+3 )] = _tex_sourceFloat1.Load( sourcePixelIndex, uint2( 3, 3 ) );
#endif
}

[numthreads( THREADS_X, THREADS_Y, 1 )]
void	CS_SplitFloat3_FUCKING_COMPILER_BUG( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID, uint3 _dispatchThreadID : SV_dispatchThreadID ) {
	uint3	targetPixelIndex = uint3( _dispatchThreadID.xy, 0 );
	uint2	sourcePixelIndex = _dispatchThreadID.xy << 2;

	// First line
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x-=3; sourcePixelIndex.y++; targetPixelIndex.z++;

	// Second line
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x-=3; sourcePixelIndex.y++; targetPixelIndex.z++;

	// Third line
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x-=3; sourcePixelIndex.y++; targetPixelIndex.z++;

	// Fourth line
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x++; targetPixelIndex.z++;
	_tex_targetFloat3[targetPixelIndex] = _tex_sourceFloat3[sourcePixelIndex];	sourcePixelIndex.x-=3; sourcePixelIndex.y++; targetPixelIndex.z++;
}
*/