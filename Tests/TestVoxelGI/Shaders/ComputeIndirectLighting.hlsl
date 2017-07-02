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


#define USE_PHASE_FUNCTION 1

cbuffer glou : register(b10) {
	uint	_trucs;
};

float	Phase( float _cosTheta, float _anisotropy ) {
	const float	g = _anisotropy;

// Simplest, solid medium case
//	return saturate( -_cosTheta );

	// Rayleigh
// 	return (3.0 / (16.0 * PI)) * (1.0 + _cosTheta * _cosTheta);

	// Henyey-Greenstein
	return (1.5 / (4.0 * PI)) * (1.0 - g*g) * pow( max( 0.00001, 1.0 + (g*g) - 2.0*g*_cosTheta ), -1.5 ) * (1.0 + _cosTheta * _cosTheta) / (2.0 + g*g);

// 	// Schlick's simplified Henyey-Greenstein
// 	float	den = (1.0 - g * _cosTheta);
// 	return (0.25 / PI) * (1.0 - g*g) / (den * den);

	// Draine: An interesting variation and more general phase function taking 2 parameters (from: http://arxiv.org/pdf/astro-ph/0304060.pdf)
	//	g is the same as usual, i.e. Henyey-Greenstein scattering anisotropy
	//	a is new and interestingly gives:
	//		• a=0, g!=0 reverts back to Henyey-Greenstein
	//		• a=0, g=0 reverts back to Rayleigh scattering
	//		• a=1, g!=0 reverts back to Cornette-Shanks
	//
	const float	a = 1;
	return (1.0 - g*g) * (1.0 + a * _cosTheta*_cosTheta) /
			(4.0 * PI * (1.0 + a * (1.0 + 2.0*g*g)/3.0) * pow( max( 0.00001, 1.0 + (g*g) - 2.0*g*_cosTheta ), 1.5 ));
}

// Traces a cone through the scene
// We're asking for the sine of the half angle because at each step we need the radius of the sphere tangent to the cone and although the cone's radius is R = distance * tan( half angle ), we want r = cos( half angle ) * R = distance * sin( half angle )
float4	ConeTrace( float3 _wsPosition, float3 _wsDirection, float _initialDistance, float _sinHalfAngle, out float _hitDistance ) {

	_hitDistance = _initialDistance;
	float	previousDistance = _hitDistance;
	float4	irradiance = float4( 0, 0, 0, 1 );
	for ( uint stepIndex=0; stepIndex < 64; stepIndex++ ) {
		// March and increase cone radius
		float3	wsPos = _wsPosition + _hitDistance * _wsDirection;
//		float	radius = _hitDistance * _sinHalfAngle;		// This is actually the tangent sphere's radius at current distance
// This is a way of biasing the mip level and smoothing the result but on top of that it also increases the marching speed which is even better
float	radius = 2 * _hitDistance * _sinHalfAngle;		// This is actually the tangent sphere's radius at current distance

		// Compute mip level depending on how many voxels the sphere is covering
		float	mipLevel = 1.0 + log2( radius / VOXEL_SIZE );
// Bias the mip to smooth the result, but doesn't increase marching step size: better to do that by multiplying radius
//float	mipLevel = 3.0 + log2( radius / VOXEL_SIZE );

//float3	positionUVW = (wsPos - VOXEL_MIN) * INV_VOXEL_VOLUME_SIZE;	// TODO: Optimize by always computing in UVW space
float3	positionUVW = (wsPos - VOXEL_MIN) * INV_VOXEL_VOLUME_SIZE;	// TODO: Optimize by always computing in UVW space
// if ( any(abs(positionUVW-0.5)) > 0.5 )
// 	break;	// Outside of the volume

		// Continuous algorithm that keeps accumulating along the ray
		const float	ALPHA_THRESHOLD = 1.0 / 255.0;
		const float	TRANSPARENCY_THRESHOLD = 0.05;
		const float	EXTINCTION_FACTOR = 10.0;
		const float	SCATTERING_ALBEDO = 1.0;	// Scatters half as much as it extincts

		float4	irradiance_alpha = _Tex_VoxelScene_SourceLighting.SampleLevel( LinearClamp, positionUVW, mipLevel );
		if ( irradiance_alpha.w > ALPHA_THRESHOLD ) {

#if USE_PHASE_FUNCTION
// Use a phase function depending on normal? NDF? lobe variance?
float3	wsNormal = _Tex_VoxelScene_Normal.SampleLevel( LinearClamp, positionUVW, mipLevel ).xyz;
		wsNormal *= irradiance_alpha.w > 0.0 ? 1.0 / irradiance_alpha.w : 0.0;
// Test to make sure albedo's alpha and irradiance's alpha are the same
//float4	albedo = _Tex_VoxelScene_Albedo.SampleLevel( LinearClamp, positionUVW, mipLevel );
//		wsNormal *= albedo.w > 0.0 ? 1.0 / albedo.w : 0.0;
float	normalLength = length( wsNormal );
		wsNormal *= normalLength > 0.0 ? 1.0 / normalLength : 0.0;
float	anisotropy = lerp( 0, -0.5, saturate(normalLength) );	// The length of the normal dictates the anisotropy of the voxel
irradiance_alpha.xyz *= Phase( dot( _wsDirection, wsNormal ), anisotropy );
#endif

			float	marchedDistance = _hitDistance - previousDistance;
			float	extinction = exp( -EXTINCTION_FACTOR * marchedDistance * irradiance_alpha.w );	// Use alpha as a measure of density
			#if PREMULTIPLY_LIGHTING
				float3	scattering = SCATTERING_ALBEDO * (1.0 - extinction) * irradiance_alpha.xyz / irradiance_alpha.w;
			#else
				float3	scattering = SCATTERING_ALBEDO * (1.0 - extinction) * irradiance_alpha.xyz;
			#endif
			irradiance.xyz += irradiance.w * scattering;	// Add voxel's irradiance as perceived through our accumulated extinction
			irradiance.w *= extinction;						// And accumulate extinction

			if ( irradiance.w < TRANSPARENCY_THRESHOLD )
				break;	// Opaque!
		}

		// Advance by sphere radius
		previousDistance = _hitDistance;
		_hitDistance += 4 * 0.125 * radius;
	}

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

#if 0	// DEBUG
float	hitDistance;
float3	wsConeDirection = N;
float4	irradiance = ConeTrace( wsVoxelCenter, wsConeDirection, VOXEL_SIZE / max( max( abs( wsConeDirection.x ), abs( wsConeDirection.y ) ), abs( wsConeDirection.z ) ), 0.5, hitDistance );
//float3	value = 0.05 * hitDistance;
float3	value = 2.0 * PI * irradiance.xyz;
//float3	value = irradiance.w;
//float3	value = N;
//float3	wsPos = wsVoxelCenter + 0.5 * VOXEL_SIZE * N;
//float3	posUVW = (wsPos - VOXEL_MIN) * INV_VOXEL_VOLUME_SIZE;
//float4	temp = _Tex_VoxelScene_Albedo.SampleLevel( LinearClamp, posUVW, 0.0 );
//float3	value = pow2( temp.xyz / temp.w );
//float3	value = pow2( temp.xyz );
_Tex_VoxelScene_TargetLighting[voxelIndex] = float4( value, 0 );

#else
	// Cast 7 cones to cover the top hemisphere
	const uint		CONES_COUNT = 7;
	const float		CONE_APERTURE = 60.0 * PI / 180.0;				// 60° aperture
	const float		SOLID_ANGLE = 2.0 * PI / CONES_COUNT;			// The sum of all cones must cover the 2PI steradians hemisphere
	const float		SIN_HALF_ANGLE = sin( 0.5 * CONE_APERTURE );	// sin( 30 )

const float	prout = 0.5;

	const float3	LS_CONE_DIRECTIONS[7] = {	float3( 0, 0, 1 ),
												float3( 0.8660254, 0, prout ),
												float3( 0.4330127, 0.7500001, prout ),
												float3( -0.4330128, 0.75, prout ),
												float3( -0.8660254, -7.571035E-08, prout ),
												float3( -0.4330126, -0.7500001, prout ),
												float3( 0.433013, -0.7499999, prout ),
											};

	float3	sumIrradiance = 0.0;
	[unroll]
	for ( uint coneIndex=0; coneIndex < CONES_COUNT; coneIndex++ ) {
		float3	lsConeDirection = LS_CONE_DIRECTIONS[coneIndex];
		float3	wsConeDirection = lsConeDirection.x * T + lsConeDirection.y * B + lsConeDirection.z * N;
		float	initialDistance = 0.5 * VOXEL_SIZE / lsConeDirection.z;	// Offset half a voxel away
//		float	initialDistance = VOXEL_SIZE;// / max( max( abs( lsConeDirection.x ), abs( lsConeDirection.y ) ), abs( lsConeDirection.z ) );
		float	hitDistance;
		float3	wsStartPosition = wsVoxelCenter;	// + VOXEL_SIZE * N;
		float4	irradiance = ConeTrace( wsStartPosition, wsConeDirection, initialDistance, SIN_HALF_ANGLE, hitDistance );
		sumIrradiance += irradiance.xyz * lsConeDirection.z;
	}
	#if USE_PHASE_FUNCTION
		sumIrradiance *= 4.0 * PI;
	#endif
	sumIrradiance *= 2.0 * PI;

//	_Tex_VoxelScene_TargetLighting[voxelIndex] = float4( sumIrradiance * INVPI * albedo.xyz, albedo.w );
	_Tex_VoxelScene_TargetLighting[voxelIndex] = float4( sumIrradiance * INVPI * albedo.xyz, 1.0 );
#endif
}
