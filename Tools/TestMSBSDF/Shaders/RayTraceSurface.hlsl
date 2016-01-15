//////////////////////////////////////////////////////////////////////////
// This shader performs a ray-tracing of the surface accounting for multiple scattering
//////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

static const float	HEIGHTFIELD_SIZE = 256.0;
static const float	INV_HEIGHTFIELD_SIZE = 1.0 / HEIGHTFIELD_SIZE;

static const float	MAX_HEIGHT = 6.0;					// Arbitrary top height
static const float	INITIAL_HEIGHT = MAX_HEIGHT - 0.1;	// So we're almost sure to start above the heightfield
static const float	STEP_SIZE = 1.0;					// Ray-tracing step size

cbuffer CB_Raytrace : register(b10) {
	float3	_direction;					// Incoming ray direction
	float	_roughness;					// Surface roughness in [0,1]
};

Texture2D< float4 >			_Tex_HeightField : register( t0 );
RWTexture2DArray< float4 >	_Tex_OutgoingDirections : register( u0 );	// 

//////////////////////////////////////////////////////////////////////////
// Ray-traces the height field
//	_position, _direction, the ray to trace through the height field
//	_normal, the normal at intersection
// returns the hit position (xyz) and hit distance (w) from original position (or INFINITY when no hit)
float4	rayTrace( float3 _position, float3 _direction, out float3 _normal ) {
	float4	dir = STEP_SIZE * float4( _direction, 1.0 );
	float4	ppos;
	float4	pos = float4( _position, 0.0 );

	float4	pNH;
	float4	NH = _Tex_HeightField.SampleLevel( LinearClamp, INV_HEIGHTFIELD_SIZE * pos.xy, 0.0 );	// Normal + Height

	_normal = NH.xyz;

	[loop]
	[fastopt]
	while ( abs(pos.z) < MAX_HEIGHT ) {
		ppos = pos;
		pNH = NH;

		pos += dir;
		NH =_Tex_HeightField.SampleLevel( LinearClamp, INV_HEIGHTFIELD_SIZE * pos.xy, 0.0 );

		// Compute intersection of the 2 segments
		float	deltaP = pos.z - ppos.z;	// Difference in ray height
		float	deltaH = NH.w - pNH.w;		// Difference in height field
		float	t = (ppos.z - pNH.w) / (deltaH - deltaP);
		if ( t > 0.0 && t <= 1.0 ) {
			// Compute true intersection & interpolate normal
			pos = ppos + t * dir;
			_normal = normalize( lerp( pNH.xyz, NH.xyz, t ) );
			return pos;
		}
	}

	return float4( pos.xyz, INFINITY );	// No hit!
}

// Source: http://blog.tobias-franke.eu/2014/03/30/notes_on_importance_sampling.html
float2	ImportanceSample_GGX( float2 xi, float a ) {
	return float2(	2.0 * PI * xi.x,									// Phi
					sqrt( (1.0f - xi.y) / ((a*a - 1.0) * xi.y + 1.0) )	// cos( Theta )
				 );
}

//////////////////////////////////////////////////////////////////////////
// Generates a random outgoing direction following the NDF of the surface
//	_normal, the normal to the surface 
//	_roughness, the roughness of the surface
//	_U, two random numbers
float3	GenerateDirection( float3 _normal, float _roughness, float2 _U ) {
	float3	tangent, biTangent;
	BuildOrthonormalBasis( _normal, tangent, biTangent );

	float2	phi_cosTheta = ImportanceSample_GGX( _U, _roughness );

	float2	scPhi;
	sincos( phi_cosTheta.x, scPhi.x, scPhi.y );
	float2	scTheta = float2( sqrt( 1.0 - phi_cosTheta.y*phi_cosTheta.y ), phi_cosTheta.y );

	float3	lsDirection = float3( scPhi.x * scTheta.x, scPhi.y * scTheta.x, scTheta.y );	// Local space direction (i.e. microfacet space)

	return lsDirection.x * tangent + lsDirection.y * biTangent + lsDirection.z * _normal;	// World space direction (i.e. surface space)
}


[numthreads( 16, 16, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint2	pixelPosition = _DispatchThreadID.xy;

	float3	targetPosition = float3( 0.5 + pixelPosition, 0.0 );
	float3	position = targetPosition + (INITIAL_HEIGHT / _direction.z) * _direction;
	float3	direction = _direction;
	float	weight = 1.0;

	uint	scatteringIndex = 0;	// No scattering yet
	float4	hitPosition = float4( position, 0 );
	float3	normal;

	float2	random = float2( rand( pixelPosition.x ), rand( pixelPosition.y ) );

/*	[loop]
	[fastopt]
	for ( ; scatteringIndex < 4; scatteringIndex++ ) {
		hitPosition = rayTrace( position, direction, normal );

		if ( hitPosition.w > 1e3 )
			break;	// The ray escaped the surface!

		// Bounce off the surface
		#if 1
			direction = reflect( direction, normal );	// Simple, don't use inverse CDF!
		#else
			position = hitPosition.xyz;
			direction = GenerateDirection( normal, _roughness, random );
		#endif

		// Assume 100% reflective conductor (no Fresnel)
		weight = 1.0;	// TODO: Handle absorption/transmission

		// Update random seed
		random.x = rand( random.x );
		random.y = rand( random.y );
	}
	*/

hitPosition = rayTrace( position, direction, normal );

scatteringIndex = 0;
//direction = float3( 0, 0, 1 );
direction = float3( position.xy * INV_HEIGHTFIELD_SIZE, position.z );
direction = float3( hitPosition.xy * INV_HEIGHTFIELD_SIZE, hitPosition.z );
weight = hitPosition.z;
//direction = -_direction;

	_Tex_OutgoingDirections[uint3( pixelPosition, scatteringIndex )] = float4( direction, weight );
}
