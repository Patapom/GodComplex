#include "Global.hlsl"
#include "CommonDistanceField.hlsl"

cbuffer CB_DistanceField : register(b2) {
	float3	_Direction;
};

Texture3D< float4 >		_TexSource : register(t0);
RWTexture3D< float >	_TexTarget : register(u0);

//////////////////////////////////////////////////////////////////////////////////////////
// Computes initial squared distance field
//
[numthreads( 1, 8, 8 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint3	VoxelPosition = uint3( _GroupID.x, _DispatchThreadID.yz );
	float4	vxPos_Center = _TexSource[VoxelPosition];
	if ( vxPos_Center.w > 0.0 ) {
//		float3	delta = (VoxelPosition.xyz+0.5) - (VoxelPosition.xyz+vxPos_Center.xyz);	// Actual computation
		float3	delta = 0.5 - vxPos_Center.xyz;			// Simplified
		_TexTarget[VoxelPosition] = length( delta );
		return;
	}

	uint3	VoxelPosition_Left = VoxelPosition;
	uint3	VoxelPosition_Right = VoxelPosition;
	[unroll]
	for ( uint i=0; i < 32; i++ ) {
		VoxelPosition_Left.x--;
		VoxelPosition_Right.x++;
		if ( VoxelPosition_Left.x < VOXELS_COUNT ) {
			float4	vxPos = _TexSource[VoxelPosition_Left];
			if ( vxPos.w > 0.0 ) {
				float3	delta = (VoxelPosition+0.5) - (VoxelPosition_Left + vxPos.xyz);
				_TexTarget[VoxelPosition] = length( delta );
				return;
			}
		}
		if ( VoxelPosition_Right.x < VOXELS_COUNT ) {
			float4	vxPos = _TexSource[VoxelPosition_Right];
			if ( vxPos.w > 0.0 ) {
				float3	delta = (VoxelPosition+0.5) - (VoxelPosition_Right + vxPos.xyz);
				_TexTarget[VoxelPosition] = length( delta );
				return;
			}
		}
	}

	_TexTarget[VoxelPosition] = 1e6;
}
