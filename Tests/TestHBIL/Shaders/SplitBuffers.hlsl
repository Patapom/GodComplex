////////////////////////////////////////////////////////////////////////////////
// Splits a full-res texture into 4x4 quarter res textures
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

#define THREADS_X	8
#define THREADS_Y	8

Texture2D< float >			_tex_sourceFloat1 : register(t0);
Texture2D< float3 >			_tex_sourceFloat3 : register(t1);
RWTexture2DArray< float >	_tex_targetFloat1 : register(u0);
RWTexture2DArray< float3 >	_tex_targetFloat3 : register(u1);

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
//
////////// GOD DAMN IT! //////////

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
