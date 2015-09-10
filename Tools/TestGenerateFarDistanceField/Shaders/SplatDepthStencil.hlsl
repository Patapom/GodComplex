#include "Global.hlsl"

Texture2D< float >		_TexDepthStencil : register(t0);
RWTexture3D< float4 >	_TexDistance : register(u0);

groupshared float3	gs_csPositions[64];
groupshared float4	gs_sumcsPositions[64];

[numthreads( 8, 8, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	PixelPosition = _DispatchThreadID.xy;

	uint2	Dimensions;
	_TexDepthStencil.GetDimensions( Dimensions.x, Dimensions.y );
	float	Zproj = _TexDepthStencil.Load( uint3( PixelPosition, 0 ) ).x;

	// Unproject Z
	const float2	ZnearFar = float2( 0.01, 100.0 );
	float	Q = ZnearFar.y / (ZnearFar.y - ZnearFar.x);
	float	Z = Q * ZnearFar.x / (Q - Zproj);

	// Compute camera position
	float4	projPosition = float4( 2.0 * (PixelPosition + 0.5) / Dimensions - 1.0, Zproj, 1.0 );
	float4	csPosition = mul( projPosition, _Proj2Camera );
			csPosition.xyz / csPosition.w;
 
// 	gs_csPositions[8*_GroupThreadID.y+_GroupThreadID.x] = csPosition.xyz;
// 	gs_sumcsPositions[8*_GroupThreadID.y+_GroupThreadID.x] = 0.0;
// 
// 	GroupMemoryBarrierWithGroupSync();

	//////////////////////////////////////////////////////////////////////////////////////////
	// Sum contributions to each deep cell
	float	fCellZ = 64.0 * (Z - ZnearFar.x) / (ZnearFar.y - ZnearFar.x);	// 64 cells along depth
	uint	CellZ = uint( floor( fCellZ ) );

	if ( CellZ < 64 )
		_TexDistance[uint3( PixelPosition, CellZ )] = float4( csPosition.xyz, 1.0 );
}
