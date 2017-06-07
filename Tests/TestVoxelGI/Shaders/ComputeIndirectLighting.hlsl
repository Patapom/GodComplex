//////////////////////////////////////////////////////////////////////////////////////////////
// Computes the indirect lighting from a voxelized scene
//////////////////////////////////////////////////////////////////////////////////////////////
//
// For eavery non-empty voxel, we read the normal and establish a local vector base from which we'll cast a bunch of cones
//	that will sample the diffuse indirect lighting perceived from the envirnment
//
#include "Global.hlsl"
#include "DistanceField.hlsl"
#include "Voxel.hlsl"

Texture3D< float4 >		_Tex_VoxelScene_Albedo : register(t0);
Texture3D< float4 >		_Tex_VoxelScene_Normal : register(t1);

Texture3D< float4 >		_Tex_VoxelScene_SourceLighting : register(t2);
RWTexture3D< float4 >	_Tex_VoxelScene_TargetLighting : register(u0);

static const float		ALPHA_THRESHOLD = 2.0 / 255.0;

// Traces a cone through the scene
// We're asking for the sine of the half angle because at each step we need the radius of the sphere tangent to the cone and although the cone's radius is R = distance * tan( half angle ), we want r = cos( half angle ) * R = distance * sin( half angle )
float3	ConeTrace( float3 _wsPosition, float3 _wsDirection, float _initialDistance, float _sinHalfAngle, float _solidAngle ) {

//	float3	positionUVW = (_wsPosition - VOXEL_MIN) * INV_VOXEL_VOLUME_SIZE;
//	float3	directionUVW = _wsDirection * INV_VOXEL_VOLUME_SIZE;

	float	distance = _initialDistance;
	float	radius = 0.0;
	for ( uint stepIndex=0; stepIndex < 128; stepIndex++ ) {
		// March and increase cone radius
		radius += distance * _sinHalfAngle;		// This is actually the tangent sphere's radius at current distance
		_wsPosition += distance * _wsDirection;

		// Compute mip level depending on how many voxels the sphere is covering
		float	mipLevel = 2.0 * radius / VOXEL_SIZE;

float3	positionUVW = (_wsPosition - VOXEL_MIN) * INV_VOXEL_VOLUME_SIZE;	// TODO: Optimize by always computing in UVW space

		// Sample albedo at position
		float4	albedo = _Tex_VoxelScene_Albedo.SampleLevel( LienarClamp, positionUVW, mipLevel );
		if ( albedo.w > ALPHA_THRESHOLD ) {
			// We have a non empty voxel, sample lighting here
			float4	irradiance = _Tex_VoxelScene_SourceLighting.SampleLevel( LienarClamp, positionUVW, mipLevel );
			float3	wsNormal = _Tex_VoxelScene_Normal.SampleLevel( LienarClamp, positionUVW, mipLevel ).xyz;
			float	normalLength = length( wsNormal );
					wsNormal *= normalLength > 0.0 ? 1.0 / normalLength : 0.0;
			float	NdotV = -dot( _wsDirection, wsNormal );
//			return albedo.xyz * lighting.xyz * saturate( NdotV ) * _solidAngle;	// TODO: Store incoming irradiance so we can multiply by albedo and solid angle!
return lighting.xyz * saturate( NdotV ) * _solidAngle;
		}
	}

	return 0.0;	// No hit
}

[numthreads( 16, 16, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint3	voxelIndex = _DispatchThreadID;
	float3	wsVoxelCenter = VOXEL_MIN + (voxelIndex + 0.5) * VOXEL_SIZE;

	float4	albedo = _Tex_VoxelScene_Albedo[voxelIndex];
	if ( albedo.w < 1.0 / 255.0 ) {
		// Voxel is empty...
		_Tex_VoxelScene_TargetLighting[voxelIndex] = 0.0;
		return;
	}

	// Read normal & compute local vector base
	float3	N = normalize( _Tex_VoxelScene_Normal[voxelIndex].xyz );
	float3	T, B;
	BuildOrthonormalBasis( N, T, B );

	// Cast 6 cones to cover the top hemisphere
	const uint		CONES_COUNT = 6;
	const float		CONE_APERTURE = 60.0 * PI / 180.0;		// 60° aperture
	const float		solidAngle = 2.0 * PI / CONES_COUNT;	// The sum of all cones must cover the 2PI steradians hemisphere
	const float		sinHalfAngle = 0.5;						// sin( 30 )
	const float3	LS_CONE_DIRECTIONS[] = {

	};

	float3	sumIrradiance = 0.0;
	[unroll]
	for ( uint coneIndex=0; coneIndex < CONES_COUNT; coneIndex++ ) {
		float3	lsConeDirection = LS_CONE_DIRECTIONS[coneIndex];
		float3	wsConeDirection = lsConeDirection.x * T + lsConeDirection.y * B + lsConeDirection.z * N;
		float	initialDistance = 0.5 * VOXEL_SIZE / lsConeDirection.z;	// Offset half a voxel away
		float3	irradiance = ConeTrace( wsVoxelCenter, wsConeDirection, initialDistance, sinHalfAngle, solidAngle );
		sumIrradiance += irradiance * dot( N, wsConeDirection );
	}
	sumIrradiance *= INVPI * albedo.xyz;
	return sumIrradiance;
}