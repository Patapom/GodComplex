/////////////////////////////////////////////////////////////////////////////////////////////////////
// Common values for the distance field voxels
/////////////////////////////////////////////////////////////////////////////////////////////////////
//
static const uint	VOXELS_COUNT = 64;										// 3D Texture dimension
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
