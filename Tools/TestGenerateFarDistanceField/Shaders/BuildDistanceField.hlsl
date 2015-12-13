#include "Global.hlsl"

static const float	DISTANCE_FIELD_FAR = 10.0;	// 64 depth levels get spread over that distance

cbuffer CB_DistanceField : register(b2) {
	float3	_Direction;
};

Texture3D< float4 >		_TexSource : register(t0);
RWTexture3D< float4 >	_TexTarget : register(u0);

//////////////////////////////////////////////////////////////////////////////////////////
// 
//
groupshared float4	gs_csPositions[2*64];
[numthreads( 16, 1, 1 )]
void	CS_X( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint3	PixelPosition = _DispatchThreadID;
	uint	PixelOffset = _GroupThreadID.x;

	uint3	Dimensions;
	_TexSource.GetDimensions( Dimensions.x, Dimensions.y, Dimensions.z );

	// Compute camera space position
	float4	projPosition = float4( 2.0 * (PixelPosition.x + 0.5) / Dimensions.x - 1.0, 1.0 - 2.0 * (PixelPosition.y + 0.5) / Dimensions.y, Zproj, 1.0 );
	float4	csPosition = mul( projPosition, _Proj2Camera );
			csPosition.xyz /= csPosition.w;

	// Sample distances left & right
	float3	UVW0 = (0.5 + float3( PixelPosition - (1+PixelOffset) * _Direction )) / Dimensions;
	float3	UVW1 = (0.5 + float3( PixelPosition + PixelOffset * _Direction )) / Dimensions;

	float	Distance0 = _TexSource.SampleLevel( PointClamp, UVW0 ).w + 1 + PixelOffset;
	float	Distance1 = _TexSource.SampleLevel( PointClamp, UVW1 ).w;
	gs_csPositions[63-PixelOffset] = Distance0*Distance0;
	gs_csPositions[64+PixelOffset] = Distance1*Distance1;

	// 

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

	csAveragePosition = csAveragePosition.w > 0.0 ? csAveragePosition / csAveragePosition.w : float4( 0.0, 0.0, 1.0, 0.0 );
	_TexTarget[uint3( _GroupID.xy, ThreadIndex )] = float4( csAveragePosition.xyz, 0.0 );
}
