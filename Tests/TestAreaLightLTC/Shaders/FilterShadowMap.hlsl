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
	return exp( -_X*_X / (2.0 * _Sigma * _Sigma));
}

float	SampleShadowMap( float2 _UV ) {
	return _TexShadowMap.SampleLevel( LinearClamp, _UV, 0.0 );
//	return 1.0 - _TexShadowMap.SampleLevel( LinearClamp, _UV, 0.0 );
}

float	GaussianFilter( float2 _UV, float2 _dUV ) {

	float	Zcenter = SampleShadowMap( _UV );
	float	KernelSize = max( 2.0, _KernelSize );// * (1.0-Zcenter);
	float	iKernelSize = 1.0 * ceil( KernelSize );

	const float	Sigma = sqrt( -KernelSize*KernelSize / (2.0 * log( 0.01 )) );
//	const float	Sigma = max( 1e-4, sqrt( -KernelSize*KernelSize / (2.0 * log( 0.1 )) ) );

	float	Sum = GaussWeight( 0.0, Sigma ) * Zcenter;

//_dUV *= 2.0;

	float4	UV_left_right = _UV.xyxy;
	for ( float i=1; i <= iKernelSize; i++ ) {
		UV_left_right.xy -= _dUV;
		UV_left_right.zw += _dUV;
		Sum += GaussWeight( i, Sigma ) * (SampleShadowMap( UV_left_right.xy ) + SampleShadowMap( UV_left_right.zw ));
	}

	return Sum / sqrt( PI * 2.0 * Sigma * Sigma );
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
