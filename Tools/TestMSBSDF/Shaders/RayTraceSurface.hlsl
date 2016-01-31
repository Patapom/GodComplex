//////////////////////////////////////////////////////////////////////////
// This shader performs a ray-tracing of the surface accounting for multiple scattering
//////////////////////////////////////////////////////////////////////////
//
// @TODO:
//	• Find why we have an energy leak!
//
#include "Global.hlsl"

static const float	MAX_HEIGHT = 8.0;					// Arbitrary top height above which the ray is deemed to escape the surface
static const float	INITIAL_HEIGHT = MAX_HEIGHT - 0.1;	// So we're almost sure to start above the heightfield but below the escape height
static const float	STEP_SIZE = 1.0;					// Ray-tracing step size

static const float	OFF_SURFACE_DISTANCE = 1e-2;

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
float4	RayTrace( float3 _position, float3 _direction, out float3 _normal, bool _aboveSurface ) {
	float4	dir = float4( _direction, 1.0 );
	float4	ppos;
	float4	pos = float4( _position, 0.0 );

	float4	pNH;
	float4	NH = _Tex_HeightField.SampleLevel( LinearWrap, INV_HEIGHTFIELD_SIZE * pos.xy, 0.0 );	// Normal + Height

	_normal = NH.xyz;

//	float	maxStepsCount = min(	0.5 * HEIGHTFIELD_SIZE / STEP_SIZE,		// Anyway, can't ray-trace more than this amount of steps
//									2.0 * MAX_HEIGHT / abs( _direction.z )	// How many steps does it take, using the ray direction, to move from -MAX_HEIGHT to +MAX_HEIGHT ?
//								);

	float	maxStepsCount = 2.0 * MAX_HEIGHT / abs( _direction.z );	// How many steps does it take, using the ray direction, to move from -MAX_HEIGHT to +MAX_HEIGHT ?

	[loop]
	[fastopt]
	while ( abs(pos.z) < MAX_HEIGHT && pos.w < maxStepsCount ) {	// The ray stops if it either escapes the surface (above or below) or runs for too long without any intersection
		ppos = pos;	// Keep previous ray position
		pNH = NH;	// Keep previous heightfield's height + normal

		// Compute an adaptive step size based on the difference of height with the surface: the closer we get, the smaller the steps
		float	stepSize = STEP_SIZE * lerp( 0.01, 1.0, saturate( 0.5 * abs( pos.z - NH.w ) ) );
		float4	step = stepSize * dir;

		pos += step;	// March
		NH =_Tex_HeightField.SampleLevel( LinearWrap, INV_HEIGHTFIELD_SIZE * pos.xy, 0.0 );	// New height field's height + normal

		// Compute possible intersection between the 2 segments (i.e. heightfield segment versus ray segment)
		float	deltaP = pos.z - ppos.z;	// Difference in ray height
		float	deltaH = NH.w - pNH.w;		// Difference in height field
		float	t = (ppos.z - pNH.w) / (deltaH - deltaP);
		if ( t >= 0.0 && t <= 1.0 ) {
			// Compute true intersection & interpolate normal
			pos = ppos + t * step;
			_normal = normalize( lerp( pNH.xyz, NH.xyz, t ) );
			return pos;
		}

//*
		if ( _aboveSurface && pos.z < NH.w ) {
			_normal = float3( 0, 0, 1 );
			return float4( pos.xy, MAX_HEIGHT, 0.0 );	// Escape upward
		} else if ( !_aboveSurface && pos.z > NH.w ) {
			_normal = float3( 0, 0, -1 );
			return float4( pos.xy, -MAX_HEIGHT, 0.0 );	// Escape downward
		}
//*/
	}

	return float4( pos.xyz, INFINITY );	// No hit!
}

// Computes the new position so that we're at least off the surface by the given distance
//	_position is assumed to be on the surface (i.e. hit position)
//	_direction is assumed to either be the reflected or refracted direction, pointing AWAY from the surface
//	_normal is the surface's normal
//	_offDistance is the distance we need to get off the surface
float3	GetOffSurface( float3 _position, float3 _direction, float3 _normal, float _offDistance ) {
	float	d = _offDistance / dot( _direction, _normal );	// Distance we need to walk along direction to reach a plane at _offDistance from surface
	return _position + d * _direction;
}


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
		hitPosition = RayTrace( position, direction, normal, true );
		if ( hitPosition.w > 1e3 )
			break;	// The ray escaped the surface!

		// Walk to hit
		position = hitPosition.xyz;

		// Bounce off the surface
		#if 1
			direction = reflect( direction, normal );	// Simple, don't use inverse CDF!
		#else
			direction = GenerateDirection( normal, _roughness, random );
		#endif

		// Each bump into the surface decreases the weight by the albedo
		// 2 bumps we have albedo², 3 bumps we have albedo^3, etc. That explains the saturation of colors!
		weight *= _albedo;

		// Now, walk a little to avoid hitting the surface again
		position = GetOffSurface( position, direction, normal, OFF_SURFACE_DISTANCE );

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

	[loop]
	[fastopt]
	for ( ; scatteringIndex < 4; scatteringIndex++ ) {
		hitPosition = RayTrace( position, direction, normal, weight >= 0.0 );
		if ( hitPosition.w > 1e3 )
			break;	// The ray escaped the surface!

		// Walk to hit
		position = hitPosition.xyz;

		float3	orientedNormal = weight * normal;							// Either standard normal, or reversed if we're standing below the surface...
		float	cosTheta = abs( dot( direction, orientedNormal ) );	// cos( incidence angle with the surface's normal )

		float	F = FresnelAccurate( IOR, cosTheta );						// 1 for grazing angles or very large IOR, like metals

		// Randomly reflect or refract depending on Fresnel
		if ( random2.x < F ) {
			// Reflect off the surface
			direction = reflect( direction, orientedNormal );
		} else {
			// Refract through the surface
			IOR = 1.0 / IOR;	// Swap above/under surface (we do that BEFORE calling Refract because it expects eta = IOR_above / IOR_below)
			direction = Refract( -direction, orientedNormal, IOR );
			weight *= -1.0;		// Swap above/under surface
			orientedNormal = -orientedNormal;	// For GetOffSurface() to have a correct normal pointing away from surface (since we just passed through, we need to invert it right away)
		}

		// Now, walk a little to avoid hitting the surface again
		position = GetOffSurface( position, direction, orientedNormal, OFF_SURFACE_DISTANCE );

		// Update random seeds
		random.xy = random.zw;
		random.z = rand( random.z );
		random.w = rand( random.w );

		random2.xyz = random2.yzw;
		random2.w = rand( random2.w );
	}

// Actually, don't do that as it changes the lobe a lot!!
// The question is: what to do with those rays as they are quite numerous?
// Are they lost? Although no energy can be lost...
// Where do they come from? Are these all from long rays going toward the surface but never intersecting?
//
//	weight *= direction.z * weight;	// Last "magic trick" where we accumulate to the opposite lobe if ever the direction is wrong (i.e. very long grazing angles)
									// For example, we could have a downward direction in the upper lobe, or an upward direction in the lower lobe...

	// Don't accumulate! This is done by the histogram generation!
	if ( weight >= 0.0 )
		_Tex_OutgoingDirections_Reflected[uint3( pixelPosition, scatteringIndex-1 )] = float4( direction, weight );
	else
		_Tex_OutgoingDirections_Transmitted[uint3( pixelPosition, scatteringIndex-1 )] = float4( direction.xy, -direction.z, -weight );
}
