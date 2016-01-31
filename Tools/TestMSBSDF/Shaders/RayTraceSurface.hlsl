//////////////////////////////////////////////////////////////////////////
// This shader performs a ray-tracing of the surface accounting for multiple scattering
//////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

static const float	MAX_HEIGHT = 6.0;					// Arbitrary top height above which the ray is deemed to escape the surface
static const float	INITIAL_HEIGHT = MAX_HEIGHT - 0.1;	// So we're almost sure to start above the heightfield but below the escape height
static const float	STEP_SIZE = 1.0;					// Ray-tracing step size @TODO: Try smaller step size to see if result is the same (maybe very rough surface require smaller steps to avoid missing peaks?)

cbuffer CB_Raytrace : register(b10) {
	float3	_direction;			// Incoming ray direction
	float	_roughness;			// Surface roughness in [0,1]
	float2	_offset;			// Horizontal offset in surface in [0,1]
	float	_albedo;			// Surface albedo in [0,1]
	float	_IOR;				// Surface IOR
};

Texture2D< float4 >			_Tex_HeightField : register( t0 );
Texture2DArray< float4 >	_Tex_Random : register( t1 );
RWTexture2DArray< float4 >	_Tex_OutgoingDirections_Reflected : register( u0 );
RWTexture2DArray< float4 >	_Tex_OutgoingDirections_Transmitted : register( u1 );

//////////////////////////////////////////////////////////////////////////
// Ray-traces the height field
//	_position, _direction, the ray to trace through the height field
//	_normal, the normal at intersection
// returns the hit position (xyz) and hit distance (w) from original position (or INFINITY when no hit)
float4	RayTrace( float3 _position, float3 _direction, out float3 _normal ) {
	float4	dir = STEP_SIZE * float4( _direction, 1.0 );
	float4	ppos;
	float4	pos = float4( _position, 0.0 );

	float4	pNH;
	float4	NH = _Tex_HeightField.SampleLevel( LinearWrap, INV_HEIGHTFIELD_SIZE * pos.xy, 0.0 );	// Normal + Height

	_normal = NH.xyz;

	float	maxStepsCount = min(	0.5 * HEIGHTFIELD_SIZE,					// Anyway, can't ray-trace more than this amount of steps
									2.0 * MAX_HEIGHT / abs( _direction.z )	// How many steps does it take, using the ray direction, to move from -MAX_HEIGHT to +MAX_HEIGHT ?
								);

	[loop]
	[fastopt]
	while ( abs(pos.z) < MAX_HEIGHT && pos.w < maxStepsCount ) {	// The ray stops if it either escapes the surface (above or below) or runs for too long without any intersection
		ppos = pos;	// Keep previous ray position
		pNH = NH;	// Keep previous heightfield's height + normal

		pos += dir;	// March
		NH =_Tex_HeightField.SampleLevel( LinearWrap, INV_HEIGHTFIELD_SIZE * pos.xy, 0.0 );	// New height field's height + normal

		// Compute possible intersection of the 2 segments (i.e. heightfield segment versus ray segment)
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

#if 0
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
#endif

///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Conductor Ray-Tracing
// We only account for albedo
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
[numthreads( 16, 16, 1 )]
void	CS_Conductor( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint2	pixelPosition = _DispatchThreadID.xy;

	float3	targetPosition = float3( pixelPosition + _offset, 0.0 );					// The target point of our ray is the heightfield's texel
	float3	position = targetPosition + (INITIAL_HEIGHT / _direction.z) * _direction;	// So start from there and go back along the ray direction to reach the start height
	float3	direction = _direction;
	float	weight = 1.0;

	uint	scatteringIndex = 0;	// No scattering yet
	float4	hitPosition = float4( position, 0 );
	float3	normal;

	float4	random = _Tex_Random[uint3(pixelPosition,0)];

	[loop]
	[fastopt]
	for ( ; scatteringIndex < 4; scatteringIndex++ ) {
		hitPosition = RayTrace( position, direction, normal );
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
//		weight = 1.0;	// TODO: Handle absorption/transmission

		weight *= _albedo;	// Each bump into the surface decreases the weight by the albedo, 2 bumps we have albedo², 3 bumps we have albedo^3, etc. That explains the saturation of colors!

		// Update random seed
		random.xy = random.zw;
		random.z = rand( random.z );
		random.w = rand( random.w );
	}

//hitPosition = RayTrace( position, direction, normal );
//normal = normalize( float3( 1, 0, 10 ) );
//weight = scatteringIndex;
//direction = float3( 0, 0, 1 );
//direction = float3( position.xy * INV_HEIGHTFIELD_SIZE, position.z );
//direction = float3( hitPosition.xy * INV_HEIGHTFIELD_SIZE, hitPosition.z );
//weight = hitPosition.z;
//direction = reflect( direction, normal );
//direction = normal;
//direction = hitPosition.xyz;

	_Tex_OutgoingDirections_Reflected[uint3( pixelPosition, scatteringIndex-1 )] = float4( direction, weight );	// Don't accumulate! This is done by the histogram generation!
}


///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dielectric Ray-Tracing
// After each bump, the ray has a non-zero chance to be transmitted below the surface depending on the incidence angle with the surface and the Fresnel reflectance
// The weight is never decreased but its sign may oscillate between +1 (reflected) and -1 (transmitted)
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//

// From Walter 2007
// Expects i pointing away from the surface
// eta = IOR_over / IOR_under
//
float3	Refract( float3 i, float3 n, float eta ) {
	float	c = dot( i, n );
	return (eta * c - sign(c) * sqrt( 1.0 + eta * (c*c - 1.0))) * n - eta * i;

#if 0
	// From http://asawicki.info/news_1301_reflect_and_refract_functions.html
	float	cosTheta = dot( n, i );
	float	k = 1.0 - eta * eta * (1.0 - cosTheta * cosTheta);
	i = eta * i - (eta * cosTheta + sqrt(max( 0.0, k ))) * n;
	return k >= 0.0;
#endif
}

[numthreads( 16, 16, 1 )]
void	CS_Dielectric( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint2	pixelPosition = _DispatchThreadID.xy;

	float3	targetPosition = float3( pixelPosition + _offset, 0.0 );					// The target point of our ray is the heightfield's texel
	float3	position = targetPosition + (INITIAL_HEIGHT / _direction.z) * _direction;	// So start from there and go back along the ray direction to reach the start height
	float3	direction = _direction;
	float	weight = 1.0;

	uint	scatteringIndex = 0;	// No scattering yet
	float4	hitPosition = float4( position, 0 );
	float3	normal;

	float4	random = _Tex_Random[uint3(pixelPosition,0)];
	float4	random2 = _Tex_Random[uint3(pixelPosition,1)];

	float	IOR = _IOR;
//*
	[loop]
	[fastopt]
	for ( ; scatteringIndex < 4; scatteringIndex++ ) {
		hitPosition = RayTrace( position, direction, normal );
		if ( hitPosition.w > 1e3 )
			break;	// The ray escaped the surface!

		float3	orientedNormal = weight * normal;							// Either standard normal, or reversed if we're standing below the surface...
		float	cosTheta = abs( dot( direction, orientedNormal ) );	// cos( incidence angle with the surface's normal )

		float	F = FresnelAccurate( IOR, cosTheta );						// 1 for grazing angles or very large IOR, like metals
																			// 0 for IOR = 1
//F = 0.0;

		// Randomly reflect or refract depending on Fresnel
		if ( random2.x < F ) {
			// Reflect off the surface
			direction = reflect( direction, orientedNormal );
		} else {
			// Refract through the surface
			IOR = 1.0 / IOR;	// Swap above/under surface (we do that BEFORE calling Refract because it expects eta = IOR_above / IOR_below)
			direction = Refract( -direction, -orientedNormal, IOR );
			weight *= -1.0;		// Swap above/under surface

scatteringIndex++;
break;
		}

		// Update random seeds
		random.xy = random.zw;
		random.z = rand( random.z );
		random.w = rand( random.w );

		random2.xyz = random2.yzw;
		random2.w = rand( random2.w );
	}

//	weight *= direction.z * weight;	// Last "magic trick" where we accumulate to the opposite lobe if ever the direction is wrong (i.e. very long grazing angles)
									// For example, we could have a downward direction in the upper lobe, or an upward direction in the lower lobe...

//*/

/*
float	theta = 2.0 * asin( sqrt( 0.5 * (pixelPosition.x + random.x) / 512 ) );
float	phi = 2.0 * PI * (pixelPosition.y + random.y) / 512;
//float	theta = 2.0 * asin( sqrt( 0.5 * random.x ) );
//float	phi = 2.0 * PI * random.y;
direction = float3( sin(theta)*cos(phi), sin(theta)*sin(phi), cos(theta) );
//direction = normalize( direction - _direction );

//IOR = 1.0 / IOR;	// Swap above/under surface (we do that BEFORE calling Refract because it expects eta = IOR_above / IOR_below)
direction = Refract( direction, float3( 0, 0, 1 ), 1.0 / IOR );
//direction = -direction;
weight = -1.0;


scatteringIndex = 2;

//*/

	// Don't accumulate! This is done by the histogram generation!
	if ( weight >= 0.0 )
		_Tex_OutgoingDirections_Reflected[uint3( pixelPosition, scatteringIndex-1 )] = float4( direction, weight );
	else
		_Tex_OutgoingDirections_Transmitted[uint3( pixelPosition, scatteringIndex-1 )] = float4( direction.xy, -direction.z, -weight );
}
