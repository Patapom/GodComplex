
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
#include "Includes/DistanceFieldHelpers.hlsl"

float	map( float3 p ) {
	float	room = min( -box2( p.xz - float2( 0, 0 ), float2( 4, 4 ) ), plane( p, float3( 0, 1, 0 ), 0.0 ) );
//	float	room = -box2( p.xz - float2( 0, 0 ), float2( 4, 4 ) );	// Without floor
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
