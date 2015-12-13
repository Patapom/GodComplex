#include "Global.hlsl"

static const float	DISTANCE_FIELD_FAR = 10.0;	// 64 depth levels get spread over that distance

Texture3D< float3 >		_TexSource : register(t0);
Texture2D< float >		_TexDepthStencil : register(t1);	// Special case for the first pass: source is the depth stencil buffer!
RWTexture3D< float4 >	_TexTarget : register(u0);

//////////////////////////////////////////////////////////////////////////////////////////
// 1st shader creates 4 depth levels from 2x2 groups of a single depth level (i.e. depth stencil)
//
groupshared float4	gs_csPositions0[4][4];
[numthreads( 2, 2, 1 )]
void	CS0( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	PixelPosition = _DispatchThreadID.xy;
	uint	ThreadIndex = 2*_GroupThreadID.y+_GroupThreadID.x;	// in [0,3]

	uint2	Dimensions;
	_TexDepthStencil.GetDimensions( Dimensions.x, Dimensions.y );
	float	Zproj = 1.0;
	if ( all( PixelPosition < Dimensions ) )
		Zproj = _TexDepthStencil.Load( uint3( PixelPosition, 0 ) ).x;

	// Compute camera position
	float4	projPosition = float4( 2.0 * (PixelPosition.x + 0.5) / Dimensions.x - 1.0, 1.0 - 2.0 * (PixelPosition.y + 0.5) / Dimensions.y, Zproj, 1.0 );
	float4	csPosition = mul( projPosition, _Proj2Camera );
			csPosition.xyz /= csPosition.w;

	csPosition.z /= DISTANCE_FIELD_FAR;	// Normalized

	//////////////////////////////////////////////////////////////////////////////////////////
	// Assign to proper cell
	uint	ThreadOffset = 4 * ThreadIndex;
	[unroll]
	for ( uint i=0; i < 4; i++ )
		gs_csPositions0[ThreadIndex][i] = 0.0;

	uint	CellIndex = floor( 4.0 * csPosition.z );
	if ( CellIndex < 4 )
		gs_csPositions0[ThreadIndex][CellIndex] = float4( csPosition.xyz, 1.0 );	// Each thread/pixel will write one value to one of the 4 available depth cells
 
 	GroupMemoryBarrierWithGroupSync();


	//////////////////////////////////////////////////////////////////////////////////////////
	// Each thread will write to a specific depth cell of the 3D render target
	float4	csAveragePosition	= gs_csPositions0[0][ThreadIndex]
								+ gs_csPositions0[1][ThreadIndex]
								+ gs_csPositions0[2][ThreadIndex]
								+ gs_csPositions0[3][ThreadIndex];

	csAveragePosition = csAveragePosition.w > 0.0 ? csAveragePosition / csAveragePosition.w : float4( 0, 0, 1, 0 );
	_TexTarget[uint3( _GroupID.xy, ThreadIndex )] = float4( csAveragePosition.xyz, 0.0 );
}


//////////////////////////////////////////////////////////////////////////////////////////
// 2nd shader creates 16 depth levels from 2x2 groups of 4 depth levels
//
groupshared float4	gs_csPositions1[4][16];
[numthreads( 2, 2, 1 )]
void	CS1( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	PixelPosition = _DispatchThreadID.xy;
	uint	ThreadIndex = 2*_GroupThreadID.y+_GroupThreadID.x;	// in [0,3]

	//////////////////////////////////////////////////////////////////////////////////////////
	// Read the 4 values previously assigned to source
	float3	csPositions[4];
	csPositions[0] = _TexSource.Load( uint4( PixelPosition, 0, 0 ) );
	csPositions[1] = _TexSource.Load( uint4( PixelPosition, 1, 0 ) );
	csPositions[2] = _TexSource.Load( uint4( PixelPosition, 2, 0 ) );
	csPositions[3] = _TexSource.Load( uint4( PixelPosition, 3, 0 ) );


	//////////////////////////////////////////////////////////////////////////////////////////
	// Accumulate to one of the 16 available cells
	{
		[unroll]
		for ( uint i=0; i < 16; i++ )
			gs_csPositions1[ThreadIndex][i] = 0.0;

//		[unroll]
		[fastopt]
		for ( uint j=0; j < 4; j++ ) {
			float3	csPosition = csPositions[j];
			uint	CellIndex = uint( floor( 16.0 * csPosition.z ) );
			if ( CellIndex < 16 )
				gs_csPositions1[ThreadIndex][CellIndex] = float4( csPosition, 1.0 );	// Each thread/pixel will write its 4 values to 4 of the 16 available depth cells
		}
	}

	GroupMemoryBarrierWithGroupSync();


	//////////////////////////////////////////////////////////////////////////////////////////
	// Each thread will write 4 Z cells for a total of 4*4 = 16 Z cells (the depth dimension of our target)
	{
//		[unroll]
		[fastopt]
		for ( uint i=0; i < 4; i++ ) {
			float4	csAveragePosition	= gs_csPositions1[0][4*ThreadIndex+i]
										+ gs_csPositions1[1][4*ThreadIndex+i]
										+ gs_csPositions1[2][4*ThreadIndex+i]
										+ gs_csPositions1[3][4*ThreadIndex+i];

			csAveragePosition = csAveragePosition.w > 0.0 ? csAveragePosition / csAveragePosition.w : float4( 0, 0, 1, 0 );
			_TexTarget[uint3( _GroupID.xy, 4*ThreadIndex+i )] = float4( csAveragePosition.xyz, 0.0 );
		}
	}
}


//////////////////////////////////////////////////////////////////////////////////////////
// 3rd shader creates 64 depth levels from 2x2 groups of 16 depth levels
//
groupshared float4	gs_csPositions2[4][64];
[numthreads( 2, 2, 1 )]
void	CS2( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	PixelPosition = _DispatchThreadID.xy;
	uint	ThreadIndex = 2*_GroupThreadID.y+_GroupThreadID.x;	// in [0,3]

	//////////////////////////////////////////////////////////////////////////////////////////
	// Read the 16 values previously assigned to source
	float3	csPositions[16];
	{
		[unroll]
		for ( uint i=0; i < 16; i++ )
			csPositions[i] = _TexSource.Load( uint4( PixelPosition, i, 0 ) );
	}


	//////////////////////////////////////////////////////////////////////////////////////////
	// Accumulate to one of the 64 available cells
	{
		[unroll]
		for ( uint i=0; i < 64; i++ )
			gs_csPositions2[ThreadIndex][i] = 0.0;

//		[unroll]
		[fastopt]
		for ( uint j=0; j < 16; j++ ) {
			float3	csPosition = csPositions[j];
			uint	CellIndex = uint( floor( 64.0 * csPosition.z ) );
			if ( CellIndex < 64 )
				gs_csPositions2[ThreadIndex][CellIndex] += float4( csPosition, 1.0 );	// Each thread/pixel will write to one of the 64 available depth cells
		}
	}

 	GroupMemoryBarrierWithGroupSync();


	//////////////////////////////////////////////////////////////////////////////////////////
	// Each thread will write 16 Z cells for a total of 4*16 = 64 Z cells (the depth dimension of our target)
	{
//		[unroll]
		[fastopt]
		for ( uint i=0; i < 16; i++ ) {
			float4	csAveragePosition	= gs_csPositions2[0][16*ThreadIndex+i]
										+ gs_csPositions2[1][16*ThreadIndex+i]
										+ gs_csPositions2[2][16*ThreadIndex+i]
										+ gs_csPositions2[3][16*ThreadIndex+i];

			csAveragePosition = csAveragePosition.w > 0.0 ? float4( csAveragePosition.xyz / csAveragePosition.w, 0.0 ) : 1e6;
			_TexTarget[uint3( _GroupID.xy, 16*ThreadIndex+i )] = csAveragePosition;
		}
	}
}

