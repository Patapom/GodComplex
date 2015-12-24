/////////////////////////////////////////////////////////////////////////////////////////////////////
// Common values for the distance field voxels
/////////////////////////////////////////////////////////////////////////////////////////////////////
//
static const uint	VOXELS_COUNT = 64;										// 3D Texture dimension
static const float	INV_VOXELS_COUNT = 1.0 / VOXELS_COUNT;
static const float	VOXEL_SIZE = 0.1;										// In meters
static const float	INV_VOXEL_SIZE = 1.0 / VOXEL_SIZE;

static const float3	VOXELS_POSITION_MIN = VOXEL_SIZE * float3( -0.5 * VOXELS_COUNT, -0.5 * VOXELS_COUNT, 0.0 );			// Coordinates of the minimum voxel corner
static const float3	VOXELS_POSITION_MAX = VOXEL_SIZE * float3( 0.5 * VOXELS_COUNT, 0.5 * VOXELS_COUNT, VOXELS_COUNT );	// Coordinates of the maximum voxel corner

// Transforms a camera space position into a voxel index
float3	CameraSpace2Voxel( float3 _csPosition ) {
	return (_csPosition - VOXELS_POSITION_MIN) * INV_VOXEL_SIZE;
}

// Transforms a camera space position into a voxel index
float3	Voxel2CameraSpace( float3 _voxelPosition ) {
	return VOXELS_POSITION_MIN + _voxelPosition * VOXEL_SIZE;
}

float	SampleDistance( Texture3D<float> _DistanceField, float3 _voxelPosition ) {
	return _DistanceField.Sample( LinearClamp, INV_VOXELS_COUNT * _voxelPosition );
}

float	SampleDistanceLevel( Texture3D<float> _DistanceField, float3 _voxelPosition, float _MipLevel ) {
	return _DistanceField.SampleLevel( LinearClamp, INV_VOXELS_COUNT * _voxelPosition, _MipLevel );
}

float3	ComputeNormal( Texture3D<float> _DistanceField, float3 _voxelPosition, const float _epsilon=0.01 ) {
	const float2	eps = float2( _epsilon, 0.0 );

	float3	UVW = INV_VOXELS_COUNT * _voxelPosition;
	return float3(
		_DistanceField.Sample( LinearClamp, UVW - eps.xyy ) - _DistanceField.Sample( LinearClamp, UVW + eps.xyy ),
		_DistanceField.Sample( LinearClamp, UVW + eps.yxy ) - _DistanceField.Sample( LinearClamp, UVW - eps.yxy ),
		_DistanceField.Sample( LinearClamp, UVW - eps.yyx ) - _DistanceField.Sample( LinearClamp, UVW + eps.yyx ) );
}
