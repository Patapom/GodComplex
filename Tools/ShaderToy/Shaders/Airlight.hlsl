
#define mix	lerp
#define mod	fmod
#define fract frac
//#define atan atan2

#define PI	3.1415926535897932384626433832795

cbuffer CB_Main : register(b0) {
	float3	iResolution;	// viewport resolution (in pixels)
	float	iGlobalTime;	// shader playback time (in seconds)

	float	_Beta;			// Scattering coefficient
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

// u€[0,10], v€[0,PI/2]
float	F( float _u, float _v ) {
	float	a = 0.00118554 + _v * (0.599188 - 0.012787 * _v);
	float	b = 0.977767 + _v * (-0.748114 + _v* (0.555383 - _v * 0.175846));
	return _v * exp( a * pow( _u, b ) );
}

float	Airlight( float3 _cameraPosition, float3 _lightPosition, float3 _hitPosition, float _beta ) {
	float3	V2S = _lightPosition - _cameraPosition;
	float	Dsv = length( V2S );
			V2S /= Dsv;
	float	Tsv = _beta * Dsv;	// Optical thickness from light source to camera

	float3	V2P = _hitPosition - _cameraPosition;
	float	Dvp = length( V2P );
			V2P /= Dvp;
	float	Tvp = _beta * Dvp;	// Optical thickness from hit position to camera

	float	CosGamma = dot( V2P, V2S );
	float	SinGamma = sqrt( 1.0 - CosGamma*CosGamma );
	float	Gamma = acos( CosGamma );
	float	A1 = Tsv * SinGamma;
	float	A0 = _beta*_beta * exp( -Tsv * CosGamma ) / (2.0 * PI * A1);

	// Estimate integral
	float	v0 = 0.5 * Gamma;
	float	F0 = F( A1, v0 );

	float	v1 = 0.25 * PI + atan( (Tvp - Tsv * CosGamma) / A1 );
	float	F1 = F( A1, v1 );

	return A0 * saturate( F1 - F0 );
}

// =========== SECOND VERSION WITHOUT INVERSE TRIGONOMETRICS ===========
// u€[0,10], cos_v€[0,1]
float	F2( float _u, float _cosv ) {
	float	v = 0.5 * PI * sqrt( 1.0 - _cosv );	// Hack: acos(x) ~= PI/2 * sqrt( 1-x )

	float	a = 0.00118554 + v * (0.599188 - 0.012787 * v);
	float	b = 0.977767 + v * (-0.748114 + v* (0.555383 - v * 0.175846));
	return v * exp( a * pow( _u, b ) );
}

float	Airlight2( float3 _cameraPosition, float3 _lightPosition, float3 _hitPosition, float _beta ) {
	float3	V2S = _lightPosition - _cameraPosition;
	float	Dsv = length( V2S );
			V2S /= Dsv;
	float	Tsv = _beta * Dsv;	// Optical thickness from light source to camera

	float3	V2P = _hitPosition - _cameraPosition;
	float	Dvp = length( V2P );
			V2P /= Dvp;
	float	Tvp = _beta * Dvp;	// Optical thickness from hit position to camera

	float	CosGamma = dot( V2P, V2S );
	float	SinGamma = sqrt( 1.0 - CosGamma*CosGamma );
//	float	Gamma = acos( CosGamma );
	float	A1 = Tsv * SinGamma;
	float	A0 = _beta*_beta * exp( -Tsv * CosGamma ) / (2.0 * PI * A1);

	// Estimate integral
	float	v0 = sqrt( 0.5 * (1.0+CosGamma) );	// cos( Gamma/2 ) = sqrt( (1+cos( Gamma )) / 2 )
	float	F0 = F2( A1, v0 );

	// Here, first we note that atan(a)+atan(b) = atan( (a+b)/(1-ab) )
	// so atan(PI/4) + atan(b) = atan( (1+b)/(1-b) )  with b = (Tvp - Tsv * CosGamma) / A1
	//
	// Then cos( atan( x ) ) = 1 / sqrt( 1 + x² ) so
	//
	//	cos( atan( (1+b) / (1-b) ) )	= 1 / sqrt( 1 + (1+b)² / (1-b)² )
	//									= 1 / sqrt( ((1-b)² + (1+b)²) / (1-b)² )
	//									= (1-b) / sqrt( 2 + b² )
	//
//	float	v1 = cos( 0.25 * PI + atan( (Tvp - Tsv * CosGamma) / A1 ) );

	float	b = (Tvp - Tsv * CosGamma) / A1;
	float	v1 = (1-b) / sqrt( 2 + b*b );
	float	F1 = F2( A1, v1 );

	return A0 * saturate( F1 - F0 );
}

float4	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / iResolution.xy;
	float	AspectRatio = iResolution.x / iResolution.y;
	const float	TanHalfFOV = tan( 0.5 * 50.0 * PI / 180.0 );
	float3	View = float3( AspectRatio * TanHalfFOV * (2.0 * UV.x - 1.0), TanHalfFOV * (1.0 - 2.0f * UV.y), 1.0 );
	float	Z2Dist = length( View );
	View /= Z2Dist;

	float3	Pos = float3( 0.0, 0.0, 0.0 );
	float3	Target = Pos + float3( 0.0, 0.0, -1.0 );
	const float3	UP = float3( 0.0, 1.0, 0.0 );
	float3	Z = normalize( Target - Pos );
	float3	X = normalize( cross( Z, UP ) );
	float3	Y = cross( X, Z );
	View = View.x * X + View.y * Y + View.z * Z;

	float3	I0 = 4.0;	// Light color
	float3	LightPos = float3( 0, 1, -4 );
	float3	HitPos = Pos + 1000.0 * View;
	float	Beta = _Beta;

	float3	Color = I0 * Airlight2( Pos, LightPos, HitPos, Beta );

	return float4( pow( Color, 1.0 / 2.2 ), 1 );
}
