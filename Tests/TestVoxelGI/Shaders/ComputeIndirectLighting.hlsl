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

// Traces a cone through the scene
// We're asking for the sine of the half angle because at each step we need the radius of the sphere tangent to the cone and although the cone's radius is R = distance * tan( half angle ), we want r = cos( half angle ) * R = distance * sin( half angle )
float3	ConeTrace( float3 _wsPosition, float3 _wsDirection, float _initialDistance, float _sinHalfAngle, out float _hitDistance ) {

	const float	ALPHA_THRESHOLD = 16.0 / 255.0;

//	float3	positionUVW = (_wsPosition - VOXEL_MIN) * INV_VOXEL_VOLUME_SIZE;
//	float3	directionUVW = _wsDirection * INV_VOXEL_VOLUME_SIZE;

	_hitDistance = _initialDistance;
	for ( uint stepIndex=0; stepIndex < 128; stepIndex++ ) {
		// March and increase cone radius
//		float	radius = max( 0.5 * VOXEL_SIZE, _hitDistance * _sinHalfAngle );		// This is actually the tangent sphere's radius at current distance
float	radius = max( 0.5 * VOXEL_SIZE, 0.2 * _hitDistance );
		float3	wsPos = _wsPosition + _hitDistance * _wsDirection;

		// Compute mip level depending on how many voxels the sphere is covering
		float	mipLevel = 0.0;// 1.0 + log2( radius / VOXEL_SIZE );

//float3	positionUVW = (wsPos - VOXEL_MIN) * INV_VOXEL_VOLUME_SIZE;	// TODO: Optimize by always computing in UVW space
float3	positionUVW = (wsPos - VOXEL_MIN) * INV_VOXEL_VOLUME_SIZE;	// TODO: Optimize by always computing in UVW space

		// Sample albedo at position
		float4	albedo = _Tex_VoxelScene_Albedo.SampleLevel( LinearClamp, positionUVW, mipLevel );
		if ( albedo.w > ALPHA_THRESHOLD ) {
			// We have a non empty voxel, sample lighting here
			float3	irradiance = _Tex_VoxelScene_SourceLighting.SampleLevel( LinearClamp, positionUVW, mipLevel ).xyz;
			float3	wsNormal = _Tex_VoxelScene_Normal.SampleLevel( LinearClamp, positionUVW, mipLevel ).xyz;
			float	normalLength = length( wsNormal );
					wsNormal *= normalLength > 0.0 ? 1.0 / normalLength : 0.0;
			float	NdotV = -dot( _wsDirection, wsNormal );
//			return albedo.xyz * irradiance * saturate( NdotV );	// TODO: Store incoming irradiance so we can multiply by albedo and solid angle!

return irradiance * saturate( NdotV );
		}

		// Advance by sphere radius
		_hitDistance += 0.125 * radius;
	}

	_hitDistance = INFINITY;
	return 0.0;	// No hit
}

float4	ConeTrace2( float3 _wsPosition, float3 _wsDirection, float _initialDistance, float _sinHalfAngle, out float _hitDistance ) {

	_hitDistance = _initialDistance;
	float	previousDistance = _hitDistance;
	float4	irradiance = float4( 0, 0, 0, 1 );
	for ( uint stepIndex=0; stepIndex < 256; stepIndex++ ) {
		// March and increase cone radius
		float	radius = max( 0.5 * VOXEL_SIZE, _hitDistance * _sinHalfAngle );		// This is actually the tangent sphere's radius at current distance
//float	radius = max( 0.5 * VOXEL_SIZE, 0.5 * _hitDistance );
//float	radius = 0.5 * VOXEL_SIZE;
		float3	wsPos = _wsPosition + _hitDistance * _wsDirection;

		// Compute mip level depending on how many voxels the sphere is covering
		float	mipLevel = 1.0 + log2( radius / VOXEL_SIZE );

//float3	positionUVW = (wsPos - VOXEL_MIN) * INV_VOXEL_VOLUME_SIZE;	// TODO: Optimize by always computing in UVW space
float3	positionUVW = (wsPos - VOXEL_MIN) * INV_VOXEL_VOLUME_SIZE;	// TODO: Optimize by always computing in UVW space
// if ( any(abs(positionUVW-0.5)) > 0.5 )
// 	break;	// Outside of the volume

#if 1
		// Continuous algorithm that keeps accumulating with
		const float	ALPHA_THRESHOLD = 1.0 / 255.0;
		const float	TRANSPARENCY_THRESHOLD = 0.1;
		const float	EXTINCTION_FACTOR = 1.0;

		float4	irradiance_alpha = _Tex_VoxelScene_SourceLighting.SampleLevel( LinearClamp, positionUVW, mipLevel );
		if ( irradiance_alpha.w > ALPHA_THRESHOLD ) {
			float	marchedDistance = _hitDistance - previousDistance;
			float	extinction = exp( -EXTINCTION_FACTOR * marchedDistance * irradiance_alpha.w );	// Use alpha as a measure of density
			irradiance.xyz += irradiance.w * irradiance_alpha.xyz;	// Add voxel's irradiance as perceived through our accumulated extinction
			irradiance.w *= extinction;								// And accumulate extinction
// 			if ( irradiance.w < TRANSPARENCY_THRESHOLD )
// 				break;	// Opaque!
		}
#else
		// Blocker algorithm that stops accumulating after an alpha threshold is met
		const float	ALPHA_THRESHOLD = 16.0 / 255.0;

		float4	albedo = _Tex_VoxelScene_Albedo.SampleLevel( LinearClamp, positionUVW, mipLevel );
		if ( albedo.w > ALPHA_THRESHOLD ) {
			// We have a non empty voxel, sample lighting here
			float3	flux = _Tex_VoxelScene_SourceLighting.SampleLevel( LinearClamp, positionUVW, mipLevel ).xyz;
			float3	wsNormal = _Tex_VoxelScene_Normal.SampleLevel( LinearClamp, positionUVW, mipLevel ).xyz;
			float	normalLength = length( wsNormal );
					wsNormal *= normalLength > 0.0 ? 1.0 / normalLength : 0.0;
			float	NdotV = -dot( _wsDirection, wsNormal );
//			irradiance = albedo.xyz * flux * saturate( NdotV );	// TODO: Store incoming irradiance so we can multiply by albedo and solid angle!
			irradiance = flux * saturate( NdotV );
			break;
		}
#endif

		// Advance by sphere radius
		previousDistance = _hitDistance;
		_hitDistance += 0.125 * radius;
	}

// 	_hitDistance = INFINITY;
	return irradiance;
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
	
float	hitDistance;
float3	wsConeDirection = N;
float4	irradiance = ConeTrace2( wsVoxelCenter, wsConeDirection, VOXEL_SIZE / max( max( abs( wsConeDirection.x ), abs( wsConeDirection.y ) ), abs( wsConeDirection.z ) ), 0.5, hitDistance );
//float3	value = 0.05 * hitDistance;
float3	value = 1.0 * irradiance.xyz;
//float3	value = irradiance.w;
//float3	value = N;
//float3	wsPos = wsVoxelCenter + 0.5 * VOXEL_SIZE * N;
//float3	posUVW = (wsPos - VOXEL_MIN) * INV_VOXEL_VOLUME_SIZE;
//float4	temp = _Tex_VoxelScene_Albedo.SampleLevel( LinearClamp, posUVW, 0.0 );
//float3	value = pow2( temp.xyz / temp.w );
//float3	value = pow2( temp.xyz );
_Tex_VoxelScene_TargetLighting[voxelIndex] = float4( value, 0 );


// 	// Cast 7 cones to cover the top hemisphere
// 	const uint		CONES_COUNT = 7;
// 	const float		CONE_APERTURE = 60.0 * PI / 180.0;		// 60° aperture
// 	const float		SOLID_ANGLE = 2.0 * PI / CONES_COUNT;	// The sum of all cones must cover the 2PI steradians hemisphere
// 	const float		SIN_HALF_ANGLE = 0.5;						// sin( 30 )
// 	const float3	LS_CONE_DIRECTIONS[7] = {	float3( 0, 0, 1 ),
// 												float3( 0.8660254, 0, 0.5 ),
// 												float3( 0.4330127, 0.7500001, 0.5 ),
// 												float3( -0.4330128, 0.75, 0.5 ),
// 												float3( -0.8660254, -7.571035E-08, 0.5 ),
// 												float3( -0.4330126, -0.7500001, 0.5 ),
// 												float3( 0.433013, -0.7499999, 0.5 ),
// 											};
// 
// 	float3	sumIrradiance = 0.0;
// 	[unroll]
// 	for ( uint coneIndex=0; coneIndex < CONES_COUNT; coneIndex++ ) {
// 		float3	lsConeDirection = LS_CONE_DIRECTIONS[coneIndex];
// 		float3	wsConeDirection = lsConeDirection.x * T + lsConeDirection.y * B + lsConeDirection.z * N;
// 		float	initialDistance = 0.5 * VOXEL_SIZE / lsConeDirection.z;	// Offset half a voxel away
// 		float	hitDistance;
// 		float3	irradiance = ConeTrace( wsVoxelCenter, wsConeDirection, initialDistance, SIN_HALF_ANGLE, hitDistance );
// 		sumIrradiance += irradiance * lsConeDirection.z * SOLID_ANGLE;
// 	}
// 
// 	_Tex_VoxelScene_TargetLighting[voxelIndex] = float4( sumIrradiance * INVPI * albedo.xyz, 0.0 );
}