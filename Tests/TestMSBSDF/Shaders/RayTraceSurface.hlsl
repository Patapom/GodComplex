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
	return _Tex_HeightField_Height.SampleLevel( LinearWrap, INV_HEIGHTFIELD_SIZE * (_pos + 0.5), 0.0 );
}

// Normal sampling is WRONG!
// Bilinear interpolation of precomputed normals + renormalization leads to incorrect normals!
// Use ComputeNormal() instead...
//float4	SampleNormalHeight( float2 _pos ) {
//	float4	NH = _Tex_HeightField_Normal.SampleLevel( LinearWrap, INV_HEIGHTFIELD_SIZE * _pos, 0.0 );
//			NH.xyz = normalize( NH.xyz );
//	return NH;
//}

// Computes the local normal from bilinear 
float3	ComputeNormal( float2 _pos ) {
	const float2 eps = float2( 0.01, 0 );
	float	HXn = SampleHeight( _pos-eps.xy );
	float	HXp = SampleHeight( _pos+eps.xy );
	float	HYn = SampleHeight( _pos-eps.yx );
	float	HYp = SampleHeight( _pos+eps.yx );
	return normalize( float3( HXn - HXp, HYn - HYp, 2.0 * eps.x ) );
}


//////////////////////////////////////////////////////////////////////////
// Ray-traces the height field
//	_position, _direction, the ray to trace through the height field
//	_normal, the normal at intersection
// returns the hit position (xyz) and hit distance (w) from original position (or INFINITY when no hit)
float4	RayTrace( float3 _position, float3 _direction, float _minTraceDistance=1e-3 ) {

	const float	eps = 0.001;	// Epsilon on surface interval [0,1] to have a little additionnal tolerance

	// Compute maximum ray distance
	float	maxDistance = 2.0 * MAX_HEIGHT / abs( _direction.z );	// How many steps does it take, using the ray direction, to move from -MAX_HEIGHT to +MAX_HEIGHT?
			maxDistance = min( HEIGHTFIELD_SIZE, maxDistance );		// Anyway, can't ray-trace more than the entire heightfield (if we cross it entirely horizontally without a hit, 
																	//	chances are there is no hit at all because of a very flat surface and it's no use tracing the heightfield again...)

	// Build initial direction and position as extended vectors
	float4	dir = float4( _direction, 1.0 );
	float4	pos = float4( _position, 0.0 );

	int2	P = int2( floor( pos.xy ) );									// Integer texel position
	float2	rDir = abs( dir.xy ) > 1e-6 ? 1.0 / abs( dir.xy ) : INFINITY;	// Reciprocal horizontal slope
 	int2	I = int2( dir.x >= 0.0 ? 1 : -1, dir.y >= 0.0 ? 1 : -1 );		// Integer increment

	// Main loop
	[loop]
	[fastopt]
	while ( abs(pos.z) < MAX_HEIGHT && pos.w < maxDistance ) {	// The ray stops if it either escapes the surface (above or below) or runs for too long without any intersection

		float2	p = pos.xy - P;																// Sub-texel position, always in [0,1]
		float2	R = float2( dir.x >= 0.0 ? 1 - p.x : p.x, dir.y >= 0.0 ? 1 - p.y : p.y );	// Remaining distance to texel border

		// Compute intersection to the next border of the texel
		float	tx = R.x * rDir.x;	// Intercept distance to horizontal border
		float	ty = R.y * rDir.y;	// Intercept distance to vertical border
		float	t = min( tx, ty );

		// Sample the 4 heights surrounding our position
		float	H00 = _Tex_HeightField_Height[(P + HEIGHTFIELD_SIZE) & (HEIGHTFIELD_SIZE-1)];
		float	H01 = _Tex_HeightField_Height[(P+int2(1,0) + HEIGHTFIELD_SIZE) & (HEIGHTFIELD_SIZE-1)];
		float	H11 = _Tex_HeightField_Height[(P+int2(1,1) + HEIGHTFIELD_SIZE) & (HEIGHTFIELD_SIZE-1)];
		float	H10 = _Tex_HeightField_Height[(P+int2(0,1) + HEIGHTFIELD_SIZE) & (HEIGHTFIELD_SIZE-1)];

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

		if ( abs(a) < 1e-6 ) {
			// Special case where the quadratic part doesn't play any role (i.e. vertical or axis-aligned cases)
			// We only need to solve b.t + c = 0 so t = -c / b
			float	tz = abs(b) > 1e-6 ? -c / b : INFINITY;
			if ( tz >= -eps && tz <= t+eps && pos.w+tz > _minTraceDistance ) {
				return pos + tz * dir;	// Found a hit!
			}
		} else {
			// General, quadratic equation
			float	delta = b*b - 4*a*c;
			if ( delta >= 0.0 ) {
				// Maybe we get a hit?
				delta = sqrt( delta );
				float	t0 = (-b - delta) / (2.0 * a);
				float	t1 = (-b + delta) / (2.0 * a);
				float	tz = INFINITY;
				if ( t0 >= -eps )
					tz = t0;	// t0 is closer
				if ( t1 >= -eps && t1 <= tz )
					tz = t1;	// t1 is closer

				if ( tz <= t+eps && pos.w+tz > _minTraceDistance ) {
					return pos + tz * dir;	// Found a hit!
				}
			}
		}

		// March to the next texel
		if ( tx <= ty ) {
			// March horizontally
			pos += tx * dir;
			P.x += I.x;					// Next horizontal integer texel
		} else {
			// March vertically
			pos += ty * dir;
			P.y += I.y;					// Next vertical integer texel
		}
	}

	// No hit!
	pos.w = INFINITY;
	return pos;
}


///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Conductor Ray-Tracing
// We only account for F0, the weight is decreased by fresnel each bounce
// @TODO: we should use a complex Fresnel term here!!!!
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
[numthreads( 16, 16, 1 )]
void	CS_Conductor( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint2	pixelPosition = _DispatchThreadID.xy;
	float3	targetPosition = float3( pixelPosition + _offset, 0.0 );					// The target point of our ray is the heightfield's texel
	float3	position = targetPosition + (INITIAL_HEIGHT / _direction.z) * _direction;	// So start from there and go back along the ray direction to reach the start height
	float3	direction = _direction;														// Points TOWARD the surface (i.e. down)!
	float	weight = 1.0;

	uint	scatteringIndex = 0;	// No scattering yet

	float4	random = _Tex_Random[uint3(pixelPosition,0)];
			random = JitterRandom( random, _offset );

	float	IOR = _IOR;
	float	F0 = Fresnel_F0FromIOR( IOR );
	float	F90 = 1.0;		// TODO!!
	bool	error = false;

	[loop]
	[fastopt]
	for ( ; scatteringIndex <= MAX_SCATTERING_ORDER; scatteringIndex++ ) {
//	for ( ; scatteringIndex < 2; scatteringIndex++ ) {
		float4	hitPosition = RayTrace( position, direction );
		if ( hitPosition.w > 1e3 )
			break;	// The ray escaped the surface!

		// Walk to hit
		position = hitPosition.xyz;

		// Compute normal at position
		float3	normal = ComputeNormal( position.xy );

// No normal are oriented toward the bottom (actually, no Z < 0.4 exist)
if ( normal.z < 0.0 ) {
	error = true;
	break;
}

		float	cosTheta = -dot( direction, normal );	// Minus sign because direction is opposite to normal direction
if ( cosTheta < -0.95 ) {
	error = true;
	break;
}

		// Bounce off the surface
		direction = reflect( direction, normal );	// Perfect mirror

		#if 1
			// Use dielectric Fresnel to weigh reflection
			float	F = FresnelAccurate( IOR, saturate( cosTheta ) );
			weight *= F;
		#elif 1
			// Use metal Fersnel to weigh reflection
			float	F = FresnelMetal( F0, F90, saturate( cosTheta ) ).x;
			weight *= F;
		#else
			// Each bump into the surface decreases the weight by the albedo
			// 2 bumps we have albedo², 3 bumps we have albedo^3, etc. That explains the saturation of colors!
			weight *= _albedo;
		#endif

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

	float4	random = _Tex_Random[uint3(pixelPosition,0)];
	float4	random2 = _Tex_Random[uint3(pixelPosition,1)];

	random = JitterRandom( random, _offset );
	random2 = JitterRandom( random2, _offset );

	float	IOR = _IOR;

	[loop]
	[fastopt]
	for ( ; scatteringIndex < 5; scatteringIndex++ ) {
		float4	hitPosition = RayTrace( position, direction );
		if ( hitPosition.w > 1e3 )
			break;	// The ray escaped the surface!

		// Walk to hit
		position = hitPosition.xyz;

		// Compute normal at position
		float3	normal = ComputeNormal( position.xy );

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
		}

		// Update random seeds
		random = float4( random.yzw, random2.x );
		random2 = float4( random2.yzw, rand( random2.w ) );
	}

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

	float4	random = _Tex_Random[uint3(pixelPosition,0)];
	float4	random2 = _Tex_Random[uint3(pixelPosition,1)];

	random = JitterRandom( random, _offset );
	random2 = JitterRandom( random2, _offset );

	[loop]
	[fastopt]
	for ( ; scatteringIndex < 5; scatteringIndex++ ) {
		float4	hitPosition = RayTrace( position, direction );
		if ( hitPosition.w > 1e3 )
			break;	// The ray escaped the surface!

		// Walk to hit
		position = hitPosition.xyz;

		// Compute normal at position
		float3	normal = ComputeNormal( position.xy );

		// Bounce off the surface using random direction
		float3	tangent, biTangent;
		BuildOrthonormalBasis( normal, tangent, biTangent );

		float	cosTheta = random.x;	// Cosine-weighted distribution on theta
		float	sinTheta = sqrt( 1.0 - cosTheta*cosTheta );
		float2	scPhi;
		sincos( 2.0 * PI * random.y, scPhi.x, scPhi.y );

		float3	lsDirection = float3( sinTheta * scPhi.y, sinTheta * scPhi.x, cosTheta );
		direction = lsDirection.x * tangent + lsDirection.y * biTangent + lsDirection.z * normal;

		// Each bump into the surface decreases the weight by the albedo
		// 2 bounces we have albedo², 3 bounces we have albedo^3, etc. That explains the saturation of colors!
		weight *= _albedo;

		// Update random seed
		random = float4( random.zw, random2.xy );
		random2 = float4( random2.zw, rand( random2.z ), rand( random2.w ) );
	}

	uint	targetScatteringIndex = scatteringIndex <= MAX_SCATTERING_ORDER ? scatteringIndex-1 : MAX_SCATTERING_ORDER;
	_Tex_OutgoingDirections_Reflected[uint3( pixelPosition, targetScatteringIndex )] = float4( direction, weight );	// Don't accumulate! This is done by the histogram generation!
}
