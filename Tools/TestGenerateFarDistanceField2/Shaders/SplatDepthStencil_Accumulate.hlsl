#include "Global.hlsl"
#include "CommonDistanceField.hlsl"

Texture2D< float >		_TexDepthStencil : register(t0);
RWTexture3D< uint >		_TexTarget0 : register(u0);
RWTexture3D< uint >		_TexTarget1 : register(u1);

//////////////////////////////////////////////////////////////////////////////////////////
// 1st shader clears the accumulator
//
[numthreads( 4, 4, 4 )]
void	CS_Clear( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	_TexTarget0[_DispatchThreadID] = 0U;
	_TexTarget1[_DispatchThreadID] = 0U;
}

//////////////////////////////////////////////////////////////////////////////////////////
// 2nd shader accumulates normalized positions to camera-space voxels
// Because of InterlockedAdd use, the (XYZ+1) float4 is split into 2 UINTs that are accumulated each to their target UAV
//
[numthreads( 8, 8, 1 )]
void	CS_Accumulate( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	PixelPosition = _DispatchThreadID.xy;

	uint2	Dimensions;
	_TexDepthStencil.GetDimensions( Dimensions.x, Dimensions.y );
	if ( any( PixelPosition > Dimensions ) )
		return;

	// Compute camera space position
//	float	Zproj = _TexDepthStencil.SampleLevel( LinearClamp, float2(0.5+PixelPosition) / Dimensions, 0.0 );
	float	Zproj = _TexDepthStencil[PixelPosition];
	if ( Zproj == 0.0 )//< 1e-3 )
		return;

	float4	projPosition = float4( 2.0 * (PixelPosition.x + 0.5) / Dimensions.x - 1.0, 1.0 - 2.0 * (PixelPosition.y + 0.5) / Dimensions.y, Zproj, 1.0 );
	float4	csPosition = mul( projPosition, _Proj2Camera );
			csPosition.xyz /= csPosition.w;

	// Compute voxel position
	float3	voxelPosition = CameraSpace2Voxel( csPosition.xyz );
	uint3	voxelIndex = floor( voxelPosition );
	if ( any(voxelIndex >= VOXELS_COUNT) )
		return;
	float3	voxelInnerCoordinate = voxelPosition - voxelIndex;

	// Compute fixed-point position within voxel
	uint3	voxelFixedPoint = uint3( 256.0 * voxelInnerCoordinate );	// Fixed-point decimal position in [0,256[

	// Pack values as true UINTs
	uint	value0 = (voxelFixedPoint.x << 16) | voxelFixedPoint.y;
	uint	value1 = (voxelFixedPoint.z << 16) | 256U;

	// Accumulate to target
	uint	onSenFout;
	InterlockedAdd( _TexTarget0[voxelIndex], value0, onSenFout );
	InterlockedAdd( _TexTarget1[voxelIndex], value1, onSenFout );
}
