#include "Global.hlsl"

cbuffer CB_Object : register(b2) {
	float4x4	_Local2World;
};

Texture2D< float4 >	_TexAreaLight : register(t0);

struct VS_IN {
	float4	__Position : SV_POSITION;
};


VS_IN	VS( VS_IN _In ) {
	return _In;
}

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
	float	Beta = 0.1;

	float3	Color = I0 * Airlight( Pos, LightPos, HitPos, Beta );

	return float4( pow( Color, 1.0 / 2.2 ), 1 );
}
