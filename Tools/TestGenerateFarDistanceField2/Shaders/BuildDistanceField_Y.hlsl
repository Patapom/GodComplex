#include "Global.hlsl"
#include "CommonDistanceField.hlsl"

cbuffer CB_DistanceField : register(b2) {
	float3	_Direction;
};

Texture3D< float >		_TexSource : register(t0);
RWTexture3D< float >	_TexTarget : register(u0);

//////////////////////////////////////////////////////////////////////////////////////////
// Expands initial X-spanned distance field to Y
//
[numthreads( 8, 1, 8 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint3	VoxelPosition = uint3( _DispatchThreadID.x, _GroupID.y, _DispatchThreadID.z );

	float	sqDistance_Min = _TexSource[VoxelPosition];
			sqDistance_Min *= sqDistance_Min;

	uint3	VoxelPosition_Left = VoxelPosition;
	uint3	VoxelPosition_Right = VoxelPosition;
	[unroll]
	for ( uint i=1; i <= 32; i++ ) {
		VoxelPosition_Left.y--;
		VoxelPosition_Right.y++;

		float	sqDistance = i*i;
		float	sqDistance_Left = _TexSource[VoxelPosition_Left];
				sqDistance_Left = VoxelPosition_Left.y < VOXELS_COUNT ? sqDistance + sqDistance_Left*sqDistance_Left : 1e12;
		float	sqDistance_Right = _TexSource[VoxelPosition_Right];
				sqDistance_Right = VoxelPosition_Right.y < VOXELS_COUNT ? sqDistance + sqDistance_Right*sqDistance_Right : 1e12;

		sqDistance_Min = min( min( sqDistance_Min, sqDistance_Left ), sqDistance_Right );
	}

	_TexTarget[VoxelPosition] = sqrt( sqDistance_Min );
}
