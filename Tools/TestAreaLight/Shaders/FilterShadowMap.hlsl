#include "Global.hlsl"
#include "AreaLight.hlsl"
#include "ParaboloidShadowMap.hlsl"

static const float	INV_TEX_SIZE = 1.0 / 512.0;
//static const float	MAX_DISTANCE = 32.0;

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }


float	GaussWeight( float _X, float _Sigma ) {
	return exp( -_X*_X / (2.0 * _Sigma * _Sigma)) / sqrt( 2.0 * PI * _Sigma * _Sigma );
}

float	SampleShadowMap( float2 _UV ) {
	return _TexShadowMap.SampleLevel( LinearClamp, _UV, 0.0 );
//	return 1.0 - _TexShadowMap.SampleLevel( LinearClamp, _UV, 0.0 );
}

float	GaussianFilter( float2 _UV, float2 _dUV ) {
//	const float	Sigma = sqrt( -MAX_DISTANCE*MAX_DISTANCE / (2.0 * log( 0.01 )) );
	const float	Sigma = sqrt( -_KernelSize*_KernelSize / (2.0 * log( 0.01 )) );

	float	Sum = GaussWeight( 0.0, Sigma ) * SampleShadowMap( _UV );

	float4	UV_left_right = _UV.xyxy;
	for ( float i=1; i < _KernelSize; i++ ) {
		UV_left_right.xy -= _dUV;
		UV_left_right.zw += _dUV;
		Sum += GaussWeight( i, Sigma ) * (SampleShadowMap( UV_left_right.xy ) + SampleShadowMap( UV_left_right.zw ));
	}

	return Sum;
}

float	PS_FilterH( VS_IN _In ) : SV_TARGET0 {
	float2	dUV = float2( INV_TEX_SIZE, 0.0 );
	float2	UV = _In.__Position.xy * INV_TEX_SIZE;
//return _TexShadowMap.SampleLevel( LinearClamp, UV, 0.0 );
	return GaussianFilter( UV, dUV );
}

float	PS_FilterV( VS_IN _In ) : SV_TARGET0 {
	float2	dUV = float2( 0.0, INV_TEX_SIZE );
	float2	UV = _In.__Position.xy * INV_TEX_SIZE;
//return _TexShadowMap.SampleLevel( LinearClamp, UV, 0.0 );
	return GaussianFilter( UV, dUV );
}
