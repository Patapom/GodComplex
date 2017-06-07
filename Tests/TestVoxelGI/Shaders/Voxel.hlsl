///////////////////////////////////////////////////////////////////////////////////////////////////////
// Voxel Helpers
///////////////////////////////////////////////////////////////////////////////////////////////////////
//
#include "CornellBox.hlsl"

static const float	VOXEL_SIZE = 0.05;											// Size of a single voxel, in meters
static const float	VOXEL_DIAG_SIZE = VOXEL_SIZE * SQRT3;						// Size of the diagonal of a single voxel, in meters
static const float3	VOXELS_COUNT = float3( 128, 128, 128 );						// Amount of voxels in each direction
static const uint3	VOXEL_POTS = uint3( 7, 7, 7 );								// Power Of Twos of voxel count
static const uint3	VOXEL_MASKS = uint3( 0x7F, 0x7F, 0x7F );					// Masks to isolate bits for each dimension
static const float3	INV_VOXELS_COUNT = 1.0 / VOXELS_COUNT;
static const float3	VOXEL_VOLUME_SIZE = VOXEL_SIZE * VOXELS_COUNT;				// Size of the entire volume
static const float3	INV_VOXEL_VOLUME_SIZE = 1.0 / VOXEL_VOLUME_SIZE;

static const float3	VOXEL_VOLUME_CENTER = float3( 0, 0.5 * CORNELL_SIZE.y, 0 );	// Center the volume on the center of the Cornell box
static const float3	VOXEL_MIN = VOXEL_VOLUME_CENTER - 0.5 * VOXEL_VOLUME_SIZE;	// Min corner of the volume
static const float3	VOXEL_MAX = VOXEL_VOLUME_CENTER + 0.5 * VOXEL_VOLUME_SIZE;	// Max corner of the volume

