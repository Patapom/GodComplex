#include "Global.hlsl"
#include "CommonDistanceField.hlsl"

Texture3D< float4 >		_TexSource : register(t0);
RWTexture3D< uint >		_TexTarget0 : register(u0);
RWTexture3D< uint >		_TexTarget1 : register(u1);

//////////////////////////////////////////////////////////////////////////////////////////
// Reprojects previous frame result into current frame's accumulator
//
[numthreads( 4, 4, 4 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	float4	previousFrameVoxelInnerCoordinate = _TexSource[_DispatchThreadID];
	if ( previousFrameVoxelInnerCoordinate.w < 0.5 )
		return;	// Empty voxel

	// Compute camera space position
	float3	oldcsPosition = Voxel2CameraSpace( _DispatchThreadID + previousFrameVoxelInnerCoordinate.xyz );

	// Transform into current camera space
	float3	csPosition = mul( float4( oldcsPosition, 1.0 ), _OldCamera2NewCamera ).xyz;

	// Then back into voxel space
	float3	voxelPosition = CameraSpace2Voxel( csPosition );
	uint3	voxelIndex = floor( voxelPosition );
	float3	voxelInnerCoordinate = voxelPosition - voxelIndex;

	// Compute fixed-point position within voxel
	uint3	voxelFixedPoint = uint3( 256.0 * voxelInnerCoordinate );	// Fixed-point decimal position in [0,256[

	// Pack values as true UINTs
	uint	value0 = (voxelFixedPoint.x << 16) | voxelFixedPoint.y;
	uint	value1 = (voxelFixedPoint.z << 16) | 1U;

	// Accumulate to target
	uint	onSenFout;
	InterlockedAdd( _TexTarget0[voxelIndex], value0, onSenFout );
	InterlockedAdd( _TexTarget1[voxelIndex], value1, onSenFout );
}
