
SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border

cbuffer CB_Main : register(b0) {
	float3	iResolution;	// viewport resolution (in pixels)
	float	iGlobalTime;	// shader playback time (in seconds)

	float2	_OnSenFout0;
	float2	_OnSenFout1;
	float2	_OnSenFout2;
	float2	_OnSenFout3;
	float2	_OnSenFout4;

	uint	_OnSenFout5;
	float	_DebugPlaneHeight;
	uint	_DebugFlags;
	float	_DebugParm;
};

cbuffer CB_Camera : register(b1) {
	float4x4	_Camera2World;
	float4x4	_World2Camera;
	float4x4	_Proj2World;
	float4x4	_World2Proj;
	float4x4	_Camera2Proj;
	float4x4	_Proj2Camera;
};

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) {
	return _In;
}


/////////////////////////////////////////////////////////////////////////////////////////////

#define mix	lerp
#define mod	fmod
#define fract frac

#define PI	3.1415926535897932384626433832795
static const float	INFINITY = 1e6;
static const float	TAN_HALF_FOV = 0.57735026918962576450914878050196;	// tan( 60° / 2 )
static const float	SQRT2 = 1.4142135623730950488016887242097;

// Assuming n1=1 (air) we get:
//	F0 = ((n2 - n1) / (n2 + n1))²
//	=> n2 = (1 + sqrt(F0)) / (1 - sqrt(F0))
//
float3	Fresnel_IORFromF0( float3 _F0 )
{
	float3	SqrtF0 = sqrt( _F0 );
	return (1.0 + SqrtF0) / (1.00001 - SqrtF0);
}

// Full accurate Fresnel computation (from Walter's paper §5.1 => http://www.cs.cornell.edu/~srm/publications/EGSR07-btdf.pdf)
// For dielectrics only but who cares!?
float3	FresnelAccurate( float3 _IOR, float _CosTheta )
{
	float	c = _CosTheta;
	float3	g_squared = max( 0.0, _IOR*_IOR - 1.0 + c*c );
// 	if ( g_squared < 0.0 )
// 		return 1.0;	// Total internal reflection

	float3	g = sqrt( g_squared );

	float3	a = (g - c) / (g + c);
			a *= a;
	float3	b = (c * (g+c) - 1.0) / (c * (g-c) + 1.0);
			b = 1.0 + b*b;

	return 0.5 * a * b;
}

#include "Includes/AreaLight.hlsl"


float	subtract( float a, float b ) { return max( a, -b ); }
float	intersect( float a, float b ) { return max( a, b ); }
float	vmin( float2 v ) { return min( v.x, v.y ); }
float	vmin( float3 v ) { return min( min( v.x, v.y ), v.z ); }
float	vmin( float4 v ) { float2 temp = min( v.xy, v.zw ); return min( temp.x, temp.y ); }
float	vmax( float2 v ) { return max( v.x, v.y ); }
float	vmax( float3 v ) { return max( max( v.x, v.y ), v.z ); }
float	vmax( float4 v ) { float2 temp = max( v.xy, v.zw ); return max( temp.x, temp.y ); }
float	min3( float a, float b, float c ) { return min( min( a, b ), c ); }
float	min4( float a, float b, float c, float d ) { return min( min( min( a, b ), c ), d ); }
float	min5( float a, float b, float c, float d, float e ) { return min( min( min( min( a, b ), c ), d ), e ); }
float	min6( float a, float b, float c, float d, float e, float f ) { return min( min( min( min( min( a, b ), c ), d ), e ), f ); }
float	min7( float a, float b, float c, float d, float e, float f, float g ) { return min( min( min( min( min( min( a, b ), c ), d ), e ), f ), g ); }
float3	scale( float3 p, float3 s ) { return  p * (1.0 / s); }

float	sphere( float3 p, float r ) {
	return length( p ) - r;
}
float sbox( float3 p, float3 b ) {
	float3	d = abs(p) - b;
	return min( max(d.x,max(d.y,d.z)), length( max( d, 0.0 )) );
}
float box( float3 p, float3 b) {
	float3 d = abs(p) - b;
	return length( max( d, 0.0 ) ) + vmax( min( d, 0.0 ) );
}
float box2( float2 p, float2 b) {
	float2 d = abs(p) - b;
	return length( max(d, 0.0 ) ) + vmax( min( d, 0.0 ) );
}
float	plane( float3 p, float3 n, float d ) {
	return dot( p, n ) - d;
}
float	bowl( float3 p, float3 n, float r_out, float r_in, float h ) {
	float	hollowedSphere = subtract( sphere( p, r_out ), sphere( p, r_in ) );
	return subtract( hollowedSphere, plane( p, n, h ) );
}

float	sphereConvex( float3 p, float r, const float dAngle = PI / 10.0 ) {

	float3	dir = normalize( p );
	float	phi = atan2( dir.z, dir.x );
	float	theta = acos( clamp( dir.y, -1.0, 1.0 ) );

	phi = dAngle * (0.5 + floor( phi / dAngle ) );
	theta = dAngle * (0.5 + floor( theta / dAngle ) );

	float2	scPhi;
	sincos( phi, scPhi.x, scPhi.y );
	float2	scTheta;
	sincos( theta, scTheta.x, scTheta.y );

	float3	planeDir = float3( scTheta.x * scPhi.y, scTheta.y, scTheta.x * scPhi.x );
	float	planeDist = r * cos( 0.5 * dAngle );
	float3	planePos = planeDist * planeDir;
	float	planeHitDistance = dot( p - planePos, planeDir ) / dot( planeDir, dir );
	float3	planeHitPos = p - planeHitDistance * dir;

	return length( p - planeHitPos );
	return subtract( sphere( p, r ), length( p - planeHitPos ) );
}

float	sphereConvex2( float3 p, float r, const float dAngle = PI / 10.0 ) {

p.y *= -1.0;

	float	interval = 0.001 + _DebugParm;

	float	dist = length( p );
	float3	dir = 0.5 * p / dist;
	float2	stereoP = float2( dir.xz ) / (1.0 + dir.y);

#if 1
	// Cartersian repeat
	stereoP = interval * (frac( 0.5 + stereoP / interval ) - 0.5);
#endif

#if 0
	// Polar repeat
	interval = 0.125 * PI;
	float	radius = length( stereoP );
	float	angle = atan2( stereoP.y, stereoP.x );
			angle = fmod( PI + 0.5 * interval + angle, interval ) - 0.5*interval;
	sincos( angle, stereoP.x, stereoP.y );
	stereoP *= radius;
#endif

#if 0
	// Croute repeat
	float	radius = length( stereoP );
	float	newRadius = mod( radius, interval );
	stereoP *= newRadius / radius;
#endif

	// Unproject back to 3D cartesian
	float3	newDir = float3( 2.0 * stereoP.x, 1.0 - dot(stereoP,stereoP), 2.0 * stereoP.y ) / (1.0 + dot(stereoP,stereoP) );
	float3	newP = dist * newDir;

//	return length(newP) - r;
	return newP.y - r;
}

// Repeat around the origin by a fixed angle.
// For easier use, num of repetitions is used to specify the angle.
float pModPolar( inout float2 p, float repetitions ) {
	float	angle = 2.0*PI / repetitions;
	float	halfAngle = 0.5 * angle;
	float	a = halfAngle + atan2( p.y, p.x );
	float	r = length( p );
	float	cell = floor( a / angle );
	a = fmod( 100.0 * angle + a, angle ) - halfAngle;
	p = float2( cos(a), sin(a) ) * r;

	// For an odd number of repetitions, fix cell index of the cell in -x direction
	// (cell index would be e.g. -5 and 5 in the two halves of the cell):
//	if ( abs(cell) >= (repetitions/2) ) cell = abs(cell);

	return cell;
}

float3	pModSpherical( in float3 p, float repetitions ) {
	float	angle = 2.0*PI / repetitions;
	float	halfAngle = 0.5 * angle;

	float	r = length( p );
	float	phi = atan2( p.z, p.x );
			phi = fmod( 100.5 * angle + phi, angle ) - halfAngle;

	float	theta = asin( clamp( p.y / r, -1.0, 1.0 ) );
			angle = PI / repetitions;;
			halfAngle = 0.5 * angle;
			theta = fmod( 100.5 * angle + theta, angle ) - halfAngle;

	float2	scPhi, scTheta;
	sincos( phi, scPhi.x, scPhi.y );
	sincos( theta, scTheta.x, scTheta.y );
	return r * float3( scTheta.y * scPhi.y, scTheta.x, scTheta.y * scPhi.x );
}

float	sphereConvex3( float3 p, float r, const float dAngle = PI / 10.0 ) {
	pModPolar( p.xz, 18.0 );
	pModPolar( p.xy, 18.0 );
//	pModPolar( p.xz, lerp( 36.0, 36.0, abs( p.y ) ) );	// Funky
//	pModPolar( p.xy, lerp( 18.0, 36.0, abs( p.z + p.y ) ) );
	return p.x - r;
//	return dot( float2( p.x, -p.z ), SQRT2 ) - r;
}

float	sphereConvex4( float3 p, float r, const float dAngle = PI / 10.0 ) {
	p = pModSpherical( p, 18.0 );
//	return dot( p, normalize(
//	return length( p ) - r;
	return p.x - r;
//	return dot( float2( p.x, -p.z ), SQRT2 ) - r;
}

float	map( float3 p ) {
//	float	room = min( -box2( p.xz - float2( 0, 0 ), float2( 4, 4 ) ), plane( p, float3( 0, 1, 0 ), 0.0 ) );
	float	room = -box2( p.xz - float2( 0, 0 ), float2( 4, 4 ) );	// Without floor
	float	obj = sphere( p - float3( 0, 1, 0 ), 1.0 );
//	if ( obj > 2.0 )
//		return room;

	return min( room, sphereConvex2( p - float3( 0, 1, 0 ), 1.0 ) );

//	return sphere( p - float3( 0, 0.0, 0 ), 1.0 );
//	return min3( bowl( p - float3( 0, 1, 0 ), float3( 0, -1, 0 ), 1.0, 0.95, 0.3 ), plane( p, float3( 0, 1, 0 ), 0.0 ), -box2( p.xz - float2( 0, 0 ), float2( 4, 4 ) ) );
//	return min3(
//		sphere( p - float3( 0, 0.6, 0 ), 0.6 )
//		, plane( p, float3( 0, 1, 0 ), 0.0 )
////		, box( scale( p - float3( -1, 0.2, 0 ), float3( 1, 1, 1 ) ), float3( 1, 0.2, 1 ) )
//		, box( p - float3( -1, 0.2, 0 ), float3( 0.5, 0.2, 0.5 ) )
//		);
}

float3	normal( float3 p, const float _eps=0.01 ) {
	const float2	e = float2( _eps, 0.0 );
#if 0
	return float3(
			map( p + e.xyy ) - map( p - e.xyy ),
			map( p + e.yxy ) - map( p - e.yxy ),
			map( p + e.yyx ) - map( p - e.yyx )
		) / (2.0 * _eps);
#else
	return normalize( float3(
			map( p + e.xyy ) - map( p - e.xyy ),
			map( p + e.yxy ) - map( p - e.yxy ),
			map( p + e.yyx ) - map( p - e.yyx )
		) );
#endif
}

float	AO( float3 p, float3 n, float _strength, const float _stepSize=0.1, const uint _stepsCount=5 ) {
	float4	dir = _stepSize * float4( n, 1.0 );
	float4	pos = float4( p, 0.0 );
//			pos += 1.0 * dir;

#if 1	// iQ's method
	float	sumAO = 0.0;
	float	den = 1.0;
	for ( uint i=0; i < _stepsCount; i++ ) {
		float	distance = max( 0.0, map( pos.xyz ) );
		sumAO += (pos.w - distance) * den;
		den *= 0.5;
		pos += dir;
	}

	return 1.0 - saturate( _strength * sumAO / _stepsCount );
#else
	float	sumAO = 0.0;
	float	den = 1.0;
	for ( uint i=0; i < _stepsCount; i++ ) {
		pos += dir;
		float	distance = max( 0.0, map( pos.xyz ) );
		float	ratio = distance / pos.w;
//		sumAO += ratio;
		sumAO += ratio * ratio;
	}
	return sumAO / _stepsCount;
#endif
}

float3	DebugPlane( float3 _color, float3 pos, float3 dir, float _rayHitDistance ) {
	float	planeHitDistance = -(pos - float3( 0, _DebugPlaneHeight, 0 )).y / dir.y;
	if ( planeHitDistance < 0.0 || planeHitDistance > _rayHitDistance )
		return _color;

	float	d = map( pos + planeHitDistance * dir );
	float	i0 = saturate( d );
	float	i1 = saturate( d - 1.0 );
	float	i2 = saturate( d - 2.0 );
	float	i3 = saturate( d - 3.0 );
	float	i4 = saturate( d - 4.0 );
	float3	gradient[] = {
		float3( 0, 0, 0.25 ),
		float3( 0.25, 0, 0.25 ),
		float3( 0.25, 0, 0 ),
		float3( 0.5, 0.5, 0 ),
		float3( 0.5, 0.5, 0.5 ),
		float3( 1, 1, 1 ),
	};
	_color = lerp( lerp( lerp( lerp( lerp( gradient[0], gradient[1], i0 ), gradient[2], i1 ), gradient[3], i2 ), gradient[4], i3 ), gradient[5], i4 );

	// Add graduations
	float	isGraduation = smoothstep( 0.1, 0.0, abs( frac( 0.5 + 10.0 * d ) - 0.5 ) );
	float	isLargeGraduation = smoothstep( 0.05, 0.0, abs( frac( 0.5 + d ) - 0.5 ) );
	_color = lerp( _color, float3( 0, 0, 0 ), saturate( isGraduation + isLargeGraduation ) );

	return _color;
}

float4	ComputeHit( float3 _origin, float3 _direction, float _initialStepSize, float _maxDistance, const float _pixelRadius, const uint _maxIterations=64 ) {
	float	omega = 1.2;	// Over-relaxation size in [1,2]
	float4	pos = float4( _origin, 0.0 );
	float4	step = float4( _direction, 1.0 );
			pos += _initialStepSize * step;

	float	candidate_error = INFINITY;
	float4	candidateHit = pos;
	float	previousRadius = 0.0;
	float	stepLength = 0.0;
	float	functionSign = map( pos.xyz ) < 0.0 ? -1.0 : 1.0;	// To correct the fact we may start from inside an object
	for ( uint i=0; i < _maxIterations; ++i ) {
		float	signedRadius = functionSign * map( pos.xyz );
		float	radius = abs( signedRadius );
		bool	overRelaxtionFailed = omega > 1.0 && (radius + previousRadius) < stepLength;
		if ( overRelaxtionFailed ) {
			// Failed! Go back to normal sphere tracing with a unit over-relaxation factor...
			stepLength -= omega * stepLength;
			omega = 1.0;
		} else {
			stepLength = signedRadius * omega;	// Use a larger radius than given by the distance field (i.e. over-relaxation)
		}
		previousRadius = radius;

		float error = radius / pos.w;
		if ( !overRelaxtionFailed && error < candidate_error ) {
			// Keep smallest radius for candidate hit
			candidateHit = pos;
			candidate_error = error;
		}

		if ( !overRelaxtionFailed && error < _pixelRadius || pos.w > _maxDistance )
			break;

		pos += stepLength * step;
	}
	if ( (pos.w > _maxDistance || candidate_error > _pixelRadius) )
		return float4( pos.xyz, INFINITY );	// No hit!

	// Finalize hit by computing a proper intersection
#if 0
	for ( uint j=0; j < 5; j++ ) {
		float	signedRadius = functionSign * map( candidateHit.xyz );
		float	err = 0.01 * candidateHit.w * _pixelRadius;
		stepLength = signedRadius - err;
		candidateHit += stepLength * step;
	}
#elif 1
	for ( uint j=0; j < 5; j++ ) {
		float	signedRadius = functionSign * map( candidateHit.xyz );
		float	err = 20.0 * _pixelRadius * candidateHit.w;
		stepLength = signedRadius * pow( 2.0, 1.0 - err * j );
		candidateHit += stepLength * step;
	}
#endif

	return candidateHit;
}

float3	ComputeLighting( float3 _pos, float3 _normal, float3 _view, float3 _IOR ) {

	SurfaceContext	Surf;
	Surf.wsPosition = _pos;
	Surf.wsNormal = _normal;
	Surf.wsTangent = normalize( cross( float3( 0, 1, 0 ), Surf.wsNormal ) );
	Surf.wsBiTangent = cross( Surf.wsNormal, Surf.wsTangent );
	Surf.IOR = _IOR;
	Surf.diffuseAlbedo = 0.5 / PI;
	Surf.wsView = _view;
	Surf.roughness = 0.1;

	float3	LightPos = float3( 0, 8, 0 );
	float3	LightX = float3( 1, 0, 0 );
	float3	LightZ = float3( 0, -1, 0 );
	float2	LightScale = float2( 1.0, 1.0 );
	float	LightIntensity = 40.0;
	float	Shadow = 1.0;
	float	LightDiffusion = 1.0;
	float3	ProjectionDirection = float3( 0, 1, 0 );
	float	DistanceFalloff = 10.0;
	float	DistanceCutoff = 15.0;

	AreaLightContext		Light = CreateAreaLightContext( Surf, LightPos, LightX, LightZ, LightScale, LightIntensity, Shadow, ProjectionDirection, LightDiffusion, float2( DistanceFalloff, DistanceCutoff ) );

	ComputeLightingResult	Result = (ComputeLightingResult) 0;
	ComputeAreaLightLighting( Result, Surf, Light );

//return Surf.wsBiTangent;

	return Result.accumDiffuse + Result.accumSpecular;
}

float3	Shader( float2 _UV ) {
	float	AspectRatio = iResolution.x / iResolution.y;
	float	pixelRadius = 2.0 * SQRT2 * TAN_HALF_FOV / iResolution.y;
	float3	csView = normalize( float3( AspectRatio * TAN_HALF_FOV * (2.0 * _UV.x - 1.0), TAN_HALF_FOV * (1.0 - 2.0 * _UV.y), 1.0 ) );
	float3	wsPos = _Camera2World[3].xyz;
	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;

	float3	color = 0.0;

//return csView;

	float3	IOR = Fresnel_IORFromF0( 0.1 * float3( 0.9, 0.8, 0.6 ) );	// Standard dielectric
//	float3	IOR = Fresnel_IORFromF0( float3( 0.1, 0.1, 0.1 ) );	// Standard dielectric

	float4	hitPosition = ComputeHit( wsPos, wsView, 0.1, 40.0, pixelRadius );
	float3	N = normal( hitPosition.xyz );
	float3	Lighting0 = ComputeLighting( hitPosition.xyz, N, -wsView, IOR );
			Lighting0 *= AO( hitPosition.xyz, N, 20.0, 0.1 );

	float3	wsReflect = reflect( wsView, N );
	float4	secondHitPosition = ComputeHit( hitPosition.xyz, wsReflect, 0.01, 40.0, pixelRadius );
	float3	N2 = normal( secondHitPosition.xyz );
	float3	Lighting1 = ComputeLighting( secondHitPosition.xyz, N2, -wsReflect, IOR );
			Lighting1 *= AO( secondHitPosition.xyz, N2, 20.0, 0.1 );

	float3	Fresnel = FresnelAccurate( IOR, saturate( dot( -wsView, N ) ) );
	float3	Lighting = lerp( Lighting0, Lighting1, Fresnel );

color = Lighting;
//color = secondHitPosition.w;
//color = 0.1 * hitPosition.w;
//color = N;
//color = 0.5 * (1.0+N);
//color = 0.25 * hitPosition.xyz;
//color = AO( hitPosition.xyz, N, 50.0, 0.1 );
//color = Fresnel;
//if ( hitPosition.w > 100.0 ) return 0.0;
//	return smoothstep( 0, 0.5, abs( fmod( 100.0 + 20.0 * (hitPosition.x + hitPosition.z), 2.0 ) - 1.0 ) );

//return 100.0 * abs( map( hitPosition.xyz ) );

	if ( _DebugFlags & 1 )
		return DebugPlane( color, wsPos, wsView, hitPosition.w );
	else
		return color;
}

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / iResolution.xy;
	float3	Color = Shader( UV );
//if ( any(Color < 0.0) ) Color = float3( 1, 0, 1 );
	
Color = pow( max( 0.0, Color ), 1.0 / 2.2 );	// Gamma-correct

	return Color;
}
