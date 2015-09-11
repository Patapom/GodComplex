#include "Global.hlsl"

static const float	DISTANCE_FIELD_FAR = 20.0;	// 64 depth levels get spread over that distance

Texture3D< float >		_TexSource : register(t0);
Texture2D< float >		_TexDepthStencil : register(t1);	// Special case for the first pass: source is the depth stencil buffer!
RWTexture3D< float4 >	_TexTarget : register(u0);

//////////////////////////////////////////////////////////////////////////////////////////
// 1st shader creates 4 depth levels from 2x2 groups of single depth level (i.e. depth stencil)
//
groupshared float3	gs_csPositions0[4];
[numthreads( 2, 2, 1 )]
void	CS0( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	PixelPosition = _DispatchThreadID.xy;

	uint2	Dimensions;
	_TexDepthStencil.GetDimensions( Dimensions.x, Dimensions.y );
	float	Zproj = _TexDepthStencil.Load( uint3( PixelPosition, 0 ) ).x;

	// Unproject Z
	const float2	ZnearFar = float2( 0.01, 10.0 );
// 	float	Q = ZnearFar.y / (ZnearFar.y - ZnearFar.x);
// 	float	Z = Q * ZnearFar.x / (Q - Zproj);

	// Compute camera position
	float4	projPosition = float4( 2.0 * (PixelPosition + 0.5) / Dimensions - 1.0, Zproj, 1.0 );
	float4	csPosition = mul( projPosition, _Proj2Camera );
			csPosition.xyz / csPosition.w;
//	csPosition.w = floor( (csPosition.z - ZnearFar.x) * 64.0 / (ZnearFar.y - ZnearFar.x) );	// Depth cell index

	uint	PixelIndex = 8*_GroupThreadID.y+_GroupThreadID.x;
 	gs_csPositions0[PixelIndex] = csPosition.xyz;
 
 	GroupMemoryBarrierWithGroupSync();


	//////////////////////////////////////////////////////////////////////////////////////////
	// Perform parallel reduction
	if ( PixelIndex < 32 ) { gs_csPositions[PixelIndex] += gs_csPositions[PixelIndex+32]; }
	GroupMemoryBarrierWithGroupSync();

	if ( PixelIndex < 16 ) { gs_csPositions[PixelIndex] += gs_csPositions[PixelIndex+16]; }
	GroupMemoryBarrierWithGroupSync();

	if ( PixelIndex < 8 )  { gs_csPositions[PixelIndex] += gs_csPositions[PixelIndex+8]; }
	GroupMemoryBarrierWithGroupSync();

	if ( PixelIndex < 4 )  { gs_csPositions[PixelIndex] += gs_csPositions[PixelIndex+4]; }
	GroupMemoryBarrierWithGroupSync();


 	//////////////////////////////////////////////////////////////////////////////////////////
	// Finalize
	if ( PixelIndex == 0 ) {
		float3	csAveragePosition = (gs_csPositions[0] + gs_csPositions[1] + gs_csPositions[2] + gs_csPositions[3]) / 64.0;
	 	uint	Pipo = uint( floor( 64.0 * (csAveragePosition.z - ZnearFar.x) / (ZnearFar.y - ZnearFar.x) ) );	// 64 cells along depth
 		if ( Pipo < 64 )
			_TexDistance[uint3( _GroupID.xy, Pipo )] = float4( csAveragePosition, 0.0 );

// 		if ( _GroupID.x < 64 ) _TexDistance[uint3( _GroupID.xy, _GroupID.x )] = 0.0;
	}

// 	//////////////////////////////////////////////////////////////////////////////////////////
// 	// Sum contributions to each deep cell
// 	float	fCellZ = 64.0 * (Z - ZnearFar.x) / (ZnearFar.y - ZnearFar.x);	// 64 cells along depth
// 	uint	CellZ = uint( floor( fCellZ ) );
// 
// 	if ( CellZ < 64 )
// 		_TexDistance[uint3( PixelPosition, CellZ )] = float4( csPosition.xyz, 1.0 );
}
