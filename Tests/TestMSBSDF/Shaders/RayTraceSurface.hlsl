//////////////////////////////////////////////////////////////////////////
// This shader performs a ray-tracing of the surface accounting for multiple scattering
//////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

static const float	MAX_HEIGHT = 8.0;					// Arbitrary top height above which the ray is deemed to escape the surface
static const float	INITIAL_HEIGHT = MAX_HEIGHT - 0.1;	// So we're almost sure to start above the heightfield but below the escape height
static const float	STEP_SIZE = 1.0;					// Ray-tracing step size

static const float	OFF_SURFACE_DISTANCE = 1e-3;

cbuffer CB_Raytrace : register(b10) {
	float3	_direction;			// Incoming ray direction
	float	_roughness;			// Surface roughness in [0,1]
	float2	_offset;			// Horizontal offset in surface in [0,1]
	float	_albedo;			// Surface albedo in [0,1]
	float	_IOR;				// Surface IOR
};

Texture2D< float >			_Tex_HeightField_Height : register( t0 );
Texture2D< float4 >			_Tex_HeightField_Normal : register( t1 );
Texture2DArray< float4 >	_Tex_Random : register( t2 );
RWTexture2DArray< float4 >	_Tex_OutgoingDirections_Reflected : register( u0 );
RWTexture2DArray< float4 >	_Tex_OutgoingDirections_Transmitted : register( u1 );


// Introduces random variations in the initial random number based on the pixel offset
// This is important otherwise a completely flat surface (i.e. roughness == 0) will never
//	output a different result, whatever the iteration number...
float4	JitterRandom( float4 _initialRandom, float2 _pixelOffset ) {

	// Use 4th slice of random, sampled by the jitter offset in [0,1]², as an offset to the existing random
	// Each iteration will yield a different offset and we will sample the random texture at a different spot,
	//	yielding completely different random values at each iteration...
	float4	randomOffset = _Tex_Random.SampleLevel( LinearWrap, float3( _pixelOffset, 3 ), 0.0 );
	return frac( _initialRandom + randomOffset );

// This solution is much too biased! iQ's rand generator is not robust enough for so many rays... Or I'm using it badly...
//	float	offset = 13498.0 * _pixelOffset.x + 91132.0 * _pixelOffset.y;
//	float4	newRandom;
//	newRandom.x = rand( _initialRandom.x + offset );
//	newRandom.y = rand( _initialRandom.y + offset );
//	newRandom.z = rand( _initialRandom.z + offset );
//	newRandom.w = rand( _initialRandom.w + offset );
//
//	return newRandom;
}

// Expects _pos in texels
float	SampleHeight( float2 _pos ) {
	return _Tex_HeightField_Height.SampleLevel( LinearWrap, INV_HEIGHTFIELD_SIZE * _pos, 0.0 );
}
float4	SampleNormalHeight( float2 _pos ) {
	float4	NH = _Tex_HeightField_Normal.SampleLevel( LinearWrap, INV_HEIGHTFIELD_SIZE * _pos, 0.0 );
			NH.xyz = normalize( NH.xyz );
	return NH;
}


//////////////////////////////////////////////////////////////////////////
// Ray-traces the height field
//	_position, _direction, the ray to trace through the height field
//	_normal, the normal at intersection
//	_aboveSurface, true if offset should be upward
// returns the hit position (xyz) and hit distance (w) from original position (or INFINITY when no hit)
float4	RayTrace( float3 _position, float3 _direction, out float3 _normal, bool _aboveSurface ) {

	// Compute maximum ray distance
	float	maxDistance = min(	0.2 * HEIGHTFIELD_SIZE,					// Anyway, can't ray-trace more than this size
								2.0 * MAX_HEIGHT / abs( _direction.z )	// How long does it take, using the current ray direction, to move from -MAX_HEIGHT to +MAX_HEIGHT?
							);

//	float	maxDistance = 2.0 * MAX_HEIGHT / abs( _direction.z );	// How many steps does it take, using the ray direction, to move from -MAX_HEIGHT to +MAX_HEIGHT ?

	// Build initial direction and position as extended vectors
	float4	dir = float4( _direction, 1.0 );
	float4	pos = float4( _position, 0.0 );
	float4	ppos;

	// Sample initial height and normal
	float4	NH = SampleNormalHeight( pos.xy );	// Normal + Height
	float4	pNH;

	_normal = NH.xyz;

	// Main loop using adaptive step size
	[loop]
	[fastopt]
	while ( abs(pos.z) < MAX_HEIGHT && pos.w < maxDistance ) {	// The ray stops if it either escapes the surface (above or below) or runs for too long without any intersection
		ppos = pos;	// Keep previous ray position
		pNH = NH;	// Keep previous heightfield's height + normal

		// Compute an adaptive step size based on the difference of height with the surface: the closer we get, the smaller the steps
		float	stepSize = STEP_SIZE * lerp( 0.01, 1.0, saturate( 0.5 * abs( pos.z - NH.w ) ) );
		float4	step = stepSize * dir;

		// March one step and sample heightfield again
		pos += step;
		NH = SampleNormalHeight( pos.xy );	// New height field's normal + height

		// Compute possible intersection between the 2 segments (i.e. heightfield segment versus ray segment)
		float	deltaP = step.z;//pos.z - ppos.z;			// Difference in ray height
		float	deltaH = NH.w - pNH.w;						// Difference in height field
		float	t = (ppos.z - pNH.w) / (deltaH - deltaP);
		if ( t >= 0.0 && t <= 1.0 ) {
			// Compute true intersection & interpolate normal
			pos = ppos + t * step;
			_normal = normalize( lerp( pNH.xyz, NH.xyz, t ) );
			return pos;
		}

//		#if 1
//			// Fix any error
//			if ( _aboveSurface && pos.z < NH.w ) {
//				pos.z = NH.w + 1e-2;
//			} else if ( !_aboveSurface && pos.z > NH.w ) {
//				pos.z = NH.w - 1e-2;
//			}	
//		#else
///*			// Report the error as a visible peak
//			if ( _aboveSurface && pos.z < NH.w ) {
//				_normal = float3( 0, 0, 1 );
//				return float4( pos.xy, MAX_HEIGHT, 0.0 );	// Escape upward
//			} else if ( !_aboveSurface && pos.z > NH.w ) {
//				_normal = float3( 0, 0, -1 );
//				return float4( pos.xy, -MAX_HEIGHT, 0.0 );	// Escape downward
//			}
////*/
//		#endif
	}

	return float4( pos.xyz, INFINITY );	// No hit!
}

float4	RayTrace_TEMP( float3 _position, float3 _direction, bool _aboveSurface ) {

	// Compute maximum ray distance
	float	maxDistance = 2.0 * MAX_HEIGHT / abs( _direction.z );	// How many steps does it take, using the ray direction, to move from -MAX_HEIGHT to +MAX_HEIGHT?
			maxDistance = min( HEIGHTFIELD_SIZE, maxDistance );		// Anyway, can't ray-trace more than the entire heightfield (if we cross it entirely horizontally without a hit, 
																	//	chances are there is no hit at all because of a very flat surface and it's no use tracing the heightfield again...)

	// Build initial direction and position as extended vectors
	float4	dir = float4( _direction, 1.0 );
	float4	pos = float4( _position, 0.0 );

	// Scale direction to obtain a unit horizontal step (so we march only a single texel in the heightfield)
	float	horizontalStepSize = max( abs( dir.x ), abs( dir.y ) );				// Dividing by the maximum horizontal component guarantees a horizontal step size of 1

			horizontalStepSize = max( horizontalStepSize, 1.0 / maxDistance );	// For nearly vertical rays though, the horizontal step size is mostly 0.
																				// Using the inverse maximum distance will guarantee the step spans the full height of the heightfield 
																				//  and thus the intersection must occur since the Z component of "dir" will necessarily intersect the
																				//  heightfield at some point... (e.g. for a fully vertical ray, horizontalStepSize will be 2*MAX_HEIGHT)
	dir /= horizontalStepSize;

	float2	dXdY = float2( dir.x >= 0.0 ? 1 : -1, dir.y >= 0.0 ? 1 : -1 );


//dir = float4( 1e-12, 1e-12, -16, 16 );

	const float	eps = 0.001;

	// Main loop
	[loop]
	[fastopt]
	while ( abs(pos.z) < MAX_HEIGHT && pos.w < maxDistance ) {	// The ray stops if it either escapes the surface (above or below) or runs for too long without any intersection

		// Sample the 4 heights surrounding our position
		float2	P = floor( pos.xy - 0.5 );
		float2	p = pos.xy - 0.5 - P;

		#if 1
			P = fmod( HEIGHTFIELD_SIZE + P, HEIGHTFIELD_SIZE );
			int2	nP = int2( P );
			float	H00 = _Tex_HeightField_Height[nP];	nP.x += dXdY.x;
			float	H01 = _Tex_HeightField_Height[nP];	nP.y += dXdY.y;
			float	H11 = _Tex_HeightField_Height[nP];	nP.x -= dXdY.x;
			float	H10 = _Tex_HeightField_Height[nP];
		#else
			P = fmod( HEIGHTFIELD_SIZE + P + 0.5, HEIGHTFIELD_SIZE );
			float	H00 = SampleHeight( P );	P.x += dXdY.x;
			float	H01 = SampleHeight( P );	P.y += dXdY.y;
			float	H11 = SampleHeight( P );	P.x -= dXdY.x;
			float	H10 = SampleHeight( P );
		#endif

		// Compute the possible intersection between our ray and a bilinear surface
		// The equation of the bilinear surface is given by:
		//	H(x,y) = A + B.x + C.y + D.x.y
		// With:
		//	• A = H00
		//	• B = H01 - H00
		//	• C = H10 - H00
		//	• D = H11 + H00 - H10 - H01
		//
		// The equation of our ray is given by:
		//	pos(t) = pos + dir.t
		//		Px(t) = Px + Dx.t
		//		Py(t) = Px + Dy.t
		//		Pz(t) = Pz + Dz.t
		//
		// So H(t) is given by:
		//	H(t) = A + B.Px(t) + C.Py(t) + D.Px(t).Py(t)
		//		 = A + B.Px + B.Dx.t + C.Py + C.Dy.t + D.[Px.Py + Px.Dy.t + Py.Dx.t + Dx.Dy.t²]
		//
		// And if we search for the intersection then H(t) = Pz(t) so we simply need to find the roots of the polynomial:
		//	a.t² + b.t + c = 0
		//
		// With:
		//	• a = D.Dx.Dy
		//	• b = [B.Dx + C.Dy + D.Px.Dy + D.Py.Dx] - Dz
		//	• c = [A + B.Px + C.Py + D.Px.Py] - Pz
		//
		float	A = H00;
		float	B = H01 - H00;
		float	C = H10 - H00;
		float	D = H11 + H00 - H01 - H10;
		float	a = D * dir.x * dir.y;
		float	b = (B * dir.x + C * dir.y + D*(p.x*dir.y + p.y*dir.x)) - dir.z;
		float	c = (A + B*p.x + C*p.y + D*p.x*p.y) - pos.z;

		if ( true ) {//abs(a) < 1e-6 ) {
			// Special case where the quadratic part doesn't play any role (i.e. vertical or axis-aligned cases)
			// We only need to solve b.t + c = 0 so t = -c / b
			float	t = abs(b) > 1e-6 ? -c / b : INFINITY;
			if ( t >= -eps && t <= 1.0+eps ) {
				return pos + saturate(t) * dir;	// Found a hit!
			}

		} else {
			// General, quadratic equation
			float	delta = b*b - 4*a*c;
			if ( delta >= 0.0 ) {
				// Maybe we get a hit?
				delta = sqrt( delta );
				float	t0 = (-b - delta) / (2.0 * a);
				float	t1 = (-b + delta) / (2.0 * a);
				float	t = INFINITY;
				if ( t0 >= -eps )// && t0 < t )
					t = t0;	// t0 is closer
				if ( t1 >= -eps && t1 < t )
					t = t1;	// t1 is closer

				if ( t < 1.0+eps ) {
					return pos + saturate(t) * dir;	// Found a hit!
				}
			}
		}

//return pos;

		// March one step
		pos += dir;
	}

	// No hit!
	pos.w = INFINITY;
	return pos;
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
// We only account for albedo, the weight is decreased by the albedo each time (although we should use a complex Fresnel term here!!!!)
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
[numthreads( 16, 16, 1 )]
void	CS_Conductor( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint2	pixelPosition = _DispatchThreadID.xy;

	float3	targetPosition = float3( pixelPosition + _offset, 0.0 );					// The target point of our ray is the heightfield's texel

//targetPosition = float3( 3.5, 0, 0 );
//targetPosition.xy = clamp( targetPosition.xy, 0.0, 5.0 );
//targetPosition.x = clamp( targetPosition.x, 4.0, 5.0 );
//targetPosition.x = 4.5;
//targetPosition.y = 0.0;

//targetPosition = 0;

float4	NH0 = float4( 0.486027956, -0.207504988, 0.8489514, -1.04155791 );
float4	NH1 = float4( -0.280534029, -0.28463, 0.9166714, -1.0612607 );

	float3	position = targetPosition + (INITIAL_HEIGHT / _direction.z) * _direction;	// So start from there and go back along the ray direction to reach the start height
	float3	direction = _direction;														// Points TOWARD the surface (i.e. down)!
	float	weight = 1.0;

	uint	scatteringIndex = 0;	// No scattering yet
	float4	hitPosition = float4( position, 0 );

	float4	random = _Tex_Random[uint3(pixelPosition,0)];
			random = JitterRandom( random, _offset );

	float	IOR = _IOR;
	float	F0 = Fresnel_F0FromIOR( IOR );
	float	F90 = 1.0;		// TODO!!
	bool	error = false;

	[loop]
	[fastopt]
//	for ( ; scatteringIndex <= MAX_SCATTERING_ORDER; scatteringIndex++ ) {
	for ( ; scatteringIndex < 2; scatteringIndex++ ) {
		float3	normal;
//		hitPosition = RayTrace( position, direction, normal, true );
		hitPosition = RayTrace_TEMP( position, direction, true );
		if ( hitPosition.w > 1e3 )
			break;	// The ray escaped the surface!

		// Walk to hit
		position = hitPosition.xyz;

		// Sample normal and height
		float4	NH = SampleNormalHeight( position.xy );	// New height field's normal + height


//NH = lerp( NH0, NH1, 0.5 );
//if ( scatteringIndex == 1 ) {
////	NH.xyz = float3( 0.111414455, -0.266825169, 0.9572832 );
//	NH.xyz = float3( 0, 0, 1 );
////NH.w = 0;
//}


		normal = normalize( NH.xyz );

// No normal are oriented toward the bottom (actually, no Z < 0.4 exist)
//if ( normal.z < 0.0 ) {
//	error = true;
//	break;
//}

position.z = NH.w + 1e-3;

		float	cosTheta = -dot( direction, normal );	// Minus sign because direction is opposite to normal direction
		if ( cosTheta < -0.95 ) {
			error = true;
			break;
		}

		// Bounce off the surface
		direction = reflect( direction, normal );	// Perfect mirror
//direction = normalize( direction + float3( 0, 0, 0.1 ) );
//direction.z = max( direction.z, 0.001 );
//direction = normalize( direction );

//		#if 1
//			// Use dielectric Fresnel to weigh reflection
//			float	F = FresnelAccurate( IOR, saturate( cosTheta ) );
//			weight *= F;
//		#elif 1
//			// Use metal Fersnel to weigh reflection
//			float	F = FresnelMetal( F0, F90, saturate( cosTheta ) ).x;
//			weight *= F;
//		#else
//			// Each bump into the surface decreases the weight by the albedo
//			// 2 bumps we have albedo², 3 bumps we have albedo^3, etc. That explains the saturation of colors!
//			weight *= _albedo;
//		#endif

//		// Now, walk a little to avoid hitting the surface again
//		position = GetOffSurface( position, direction, normal, OFF_SURFACE_DISTANCE );
//
//		#if 1
//			float4	NH = SampleNormalHeight( position.xy );	// Normal + Height
//			if ( position.z < NH.w - 1e-2 ) {
//				position.z = NH.w + 1e-2;			// Ensure we're always ABOVE the surface!
////				direction = float3( 0, 0, -1 );
////				break;
//			}
//		#endif

		// Update random seed
		random.xy = random.zw;
		random.z = rand( random.z );
		random.w = rand( random.w );
	}


	if ( scatteringIndex == 0 )
		return;	// CAN'T HAPPEN! The heightfield is continuous and covers the entire

if ( error ) {
	_Tex_OutgoingDirections_Reflected[uint3( pixelPosition, scatteringIndex-1 )] = float4( float3( normalize( float2( -1, 0 ) ), 1 ), 10000 );
	return;
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


//scatteringIndex = 1;
//direction = normalize( float3( 0, 0, 1 ) );
//weight = 1.0;

	uint	targetScatteringIndex = scatteringIndex <= MAX_SCATTERING_ORDER ? scatteringIndex-1 : MAX_SCATTERING_ORDER;
	_Tex_OutgoingDirections_Reflected[uint3( pixelPosition, targetScatteringIndex )] = float4( direction, weight );		// Don't accumulate! This is done by the histogram generation!
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

	random = JitterRandom( random, _offset );
	random2 = JitterRandom( random2, _offset );

	float	IOR = _IOR;

	[loop]
	[fastopt]
	for ( ; scatteringIndex < 5; scatteringIndex++ ) {
		hitPosition = RayTrace( position, direction, normal, weight >= 0.0 );
		if ( hitPosition.w > 1e3 )
			break;	// The ray escaped the surface!

		// Walk to hit
		position = hitPosition.xyz;

		float3	orientedNormal = weight * normal;							// Either standard normal, or reversed if we're standing below the surface...
		float	cosTheta = abs( dot( direction, orientedNormal ) );			// cos( incidence angle with the surface's normal )

		float	F = FresnelAccurate( IOR, cosTheta );						// 1 for grazing angles or very large IOR, like metals

		// Randomly reflect or refract depending on Fresnel
		// We do that because we can't split the ray and trace the 2 resulting rays...
		if ( random.x < F ) {
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
		random = float4( random.yzw, random2.x );
		random2 = float4( random2.yzw, rand( random2.w ) );
	}

// Actually, don't do that as it changes the lobe a lot!!
// The question is: what to do with those rays as they are quite numerous?
// Are they lost? Although no energy can be lost...
// Where do they come from? Are these all from long rays going toward the surface but never intersecting?
//
//	weight *= direction.z * weight;	// Last "magic trick" where we accumulate to the opposite lobe if ever the direction is wrong (i.e. very long grazing angles)
									// For example, we could have a downward direction in the upper lobe, or an upward direction in the lower lobe...

	// Don't accumulate! This is done by the histogram generation!
	uint	targetScatteringIndex = scatteringIndex <= MAX_SCATTERING_ORDER ? scatteringIndex-1 : MAX_SCATTERING_ORDER;
	if ( weight >= 0.0 )
		_Tex_OutgoingDirections_Reflected[uint3( pixelPosition, targetScatteringIndex )] = float4( direction, weight );
	else
		_Tex_OutgoingDirections_Transmitted[uint3( pixelPosition, targetScatteringIndex )] = float4( direction.xy, -direction.z, -weight );
}


///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Diffuse Ray-Tracing
// We only account for albedo, the weight is decreased by the albedo after each bump and a cosine-weighted random reflected direction is chosen
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
[numthreads( 16, 16, 1 )]
void	CS_Diffuse( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

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

	random = JitterRandom( random, _offset );
	random2 = JitterRandom( random2, _offset );

	[loop]
	[fastopt]
	for ( ; scatteringIndex < 5; scatteringIndex++ ) {
		hitPosition = RayTrace( position, direction, normal, true );
		if ( hitPosition.w > 1e3 )
			break;	// The ray escaped the surface!

		// Walk to hit
		position = hitPosition.xyz;

		// Bounce off the surface using random direction
		float3	tangent, biTangent;
		BuildOrthonormalBasis( normal, tangent, biTangent );
//		biTangent = normalize( cross( normal, float3( 1, 0, 0 ) ) );
//		tangent = cross( biTangent, normal );


//normal = float3( 0, 0, 1 );
//tangent = float3( 1, 0, 0 );
//biTangent = float3( 0, 1, 0 );

		float	theta = acos( random.x );	// Uniform distribution on theta
		float	cosTheta = cos( theta );
		float	sinTheta = sqrt( 1.0 - cosTheta*cosTheta );
		float2	scPhi;
		sincos( 2.0 * PI * random.y, scPhi.x, scPhi.y );

		float3	lsDirection = float3( sinTheta * scPhi.y, sinTheta * scPhi.x, cosTheta );
		direction = lsDirection.x * tangent + lsDirection.y * biTangent + lsDirection.z * normal;

		// Each bump into the surface decreases the weight by the albedo
		// 2 bumps we have albedo², 3 bumps we have albedo^3, etc. That explains the saturation of colors!
		weight *= _albedo;

		// Now, walk a little to avoid hitting the surface again
		position = GetOffSurface( position, direction, normal, OFF_SURFACE_DISTANCE );

		#if 1
			float4	NH = SampleNormalHeight( position.xy );	// Normal + Height
			if ( position.z < NH.w-1e-2 ) {
				position.z = NH.w+1e-2;				// Ensure we're always ABOVE the surface!
			}

		#endif

		// Update random seed
		random = float4( random.zw, random2.xy );
		random2 = float4( random2.zw, rand( random2.z ), rand( random2.w ) );
	}

	uint	targetScatteringIndex = scatteringIndex <= MAX_SCATTERING_ORDER ? scatteringIndex-1 : MAX_SCATTERING_ORDER;
	_Tex_OutgoingDirections_Reflected[uint3( pixelPosition, targetScatteringIndex )] = float4( direction, weight );	// Don't accumulate! This is done by the histogram generation!
}

