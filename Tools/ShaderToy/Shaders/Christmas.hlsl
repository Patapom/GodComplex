
#define mix	lerp
#define mod	fmod
#define fract frac
#define atan atan2

#define PI	3.1415926535897932384626433832795

cbuffer CB_Main : register(b0) {
	float3	iResolution;	// viewport resolution (in pixels)
	float	iGlobalTime;	// shader playback time (in seconds)

// 	uniform vec3      iResolution;           // viewport resolution (in pixels)
// 	uniform float     iGlobalTime;           // shader playback time (in seconds)
// 	uniform vec3      iChannelResolution[4]; // channel resolution (in pixels)
// 	uniform vec4      iMouse;                // mouse pixel coords. xy: current (if MLB down), zw: click
};

Texture2D< float4 >	_Tex : register(t0);

struct VS_IN {
	float4	__Position : SV_POSITION;
};

SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border


VS_IN	VS( VS_IN _In ) {
	return _In;
}

float3	Fog( float3 _Color, float _Distance ) {
	const float3	FogColor = float3( 1.0, 0.95, 0.9 );
	float	Ext = exp( -0.01 * _Distance );
	return lerp( FogColor, _Color, Ext );
}

bool	IntersectPlane( float3 P, float3 V, out float _Distance ) {
	_Distance = (P.y - 0.0) / -V.y;
	return _Distance > 0.0;
}

float3	GetColorPlane( float3 P, float3 V, float _Distance ) {

	P += _Distance * V;

	const float	TileSize = 1.0;
	float2	xz = 10000.0 + floor( P.xz / TileSize );
	float	CheckColor = fmod( xz.x + xz.y, 2.0 );

	float3	CheckerBoard = lerp( float3( 0.0, 0.0, 0.0 ), float3( 1.0, 1.0, 1.0 ), CheckColor );
	return Fog( CheckerBoard, _Distance );
}

bool	IntersectSky( float3 P, float3 V, out float _Distance ) {
	_Distance = (P.y - 10.0) / -V.y;
	return _Distance > 0.0;
}

float hash( float n ) { return fract(sin(n)*43758.5453123); }

float	CloudDensity( float3 P ) {
	return abs( sin( 10.01 * P.x ) ) * abs( sin( 10.01 * P.z ) );
	return hash( 0.0001 * P.x ) * hash( 0.0001 * P.z );
}

#define STEPS_COUNT 32
float3	Cloud( float3 _SkyColor, float3 P, float3 V ) {
	const float		Thickness = 4.0;
	const float		Sigma_t = 0.04;
	const float3	Sigma_s = float3( 1.0, 0.9, 0.6 ) * Sigma_t;

	float	EndDist = Thickness * (V.y > 1e-3 ? 1.0 / V.y : 10.0);
	float3	EndPos = EndDist * V;
	float4	Step = float4( EndPos - P, EndDist ) / STEPS_COUNT;
	float4	Pos = 0.5 * Step;
	float4	Cloud = float4( 0.0, 0.0, 0.0, 1.0 );
	for ( int i=0; i < STEPS_COUNT; i++ ) {
		float	Density = CloudDensity( Pos.xyz );
		float	Ext = exp( -Sigma_t * Density * Step.w );
		float3	Scat = 0.5 * lerp( float3( 0.6, 0.7, 0.9 ), float3( 0.0, 0.0, 0.0 ), exp( -Sigma_s * Pos.y ) );
		Cloud.xyz += Scat * (1.0 - Ext) / Sigma_s;
		Cloud.w *= Ext;
		Pos += Step;
	}

	return _SkyColor * Cloud.w + Cloud.xyz;
}
float3	GetColorSky( float3 P, float3 V, float _Distance ) {
	float3	Sky = float3( 0.6, 0.8, 1.0 );
	Sky = Cloud( Sky, P + _Distance * V, V );
	return Fog( Sky, _Distance );
}

// bool	IntersectBall( float3 P, float3 V, out float _Distance ) {
// 
// }


float4	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / iResolution.xy;
	float	AspectRatio = iResolution.x / iResolution.y;
	const float	TanHalfFOV = tan( 0.5 * 50.0 * PI / 180.0 );
	float3	View = float3( AspectRatio * TanHalfFOV * (2.0 * UV.x - 1.0), TanHalfFOV * (1.0 - 2.0f * UV.y), 1.0 );
	float	Z2Dist = length( View );
	View /= Z2Dist;

	float3	Pos = float3( 0.5, 1.5, -4.0 * iGlobalTime );
	float3	Target = Pos + float3( 0.0, 0.0, -1.0 );
	const float3	UP = float3( 0.0, 1.0, 0.0 );
	float3	Z = normalize( Target - Pos );
	float3	X = normalize( cross( Z, UP ) );
	float3	Y = cross( X, Z );
	View = View.x * X + View.y * Y + View.z * Z;

	float3	Color = 0.0;
	float	HitDistance;
	if ( IntersectPlane( Pos, View, HitDistance ) ) {
		Color = GetColorPlane( Pos, View, HitDistance );
	} else if ( IntersectSky( Pos, View, HitDistance ) ) {
		Color = GetColorSky( Pos, View, HitDistance );
	}
	return float4( Color, 1.0 );

	return pow( _Tex.Sample( LinearWrap, UV ), 1.0 / 2.2 );
	return float4( UV, iGlobalTime % 1.0, 1 );
	return float4( 1, 1, 0, 1 );
}
