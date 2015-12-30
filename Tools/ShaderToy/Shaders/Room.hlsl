
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

//#include "Includes/AreaLight.hlsl"

float	sphere( float3 p, float3 c, float r ) {
	return length( p - c ) - r;
}
float	plane( float3 p, float3 n, float d ) {
	return dot( p, n ) - d;
}
float	map( float3 p ) {
	return min( sphere( p, float3( 0, 1, 0 ), 1.0 ), plane( p, float3( 0, 1, 0 ), 0.0 ) );
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
		return float4( pos.xyz, INFINITY );

	return candidateHit;
}

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / iResolution.xy;
	float	AspectRatio = iResolution.x / iResolution.y;
	float	pixelRadius = 2.0 * TAN_HALF_FOV / iResolution.y;
	float3	csView = normalize( float3( AspectRatio * TAN_HALF_FOV * (2.0 * UV.x - 1.0), TAN_HALF_FOV * (1.0 - 2.0 * UV.y), 1.0 ) );
//return csView;

	float4	hitPosition = ComputeHit( float3( 0, 1, -2.5 ), csView, 0.1, 40.0, pixelRadius );

return 100.0 * map( hitPosition.xyz );

	return 0.1 * hitPosition.w;
}
