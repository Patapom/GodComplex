////////////////////////////////////////////////////////////////////////////////
// Compute indirect map
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "SphericalHarmonics.hlsl"


cbuffer	CBMain : register( b0 ) {
	uint2	_resolution;		// 
	uint	_flags;				// 
	uint	_bounceIndex;

	float3	_rho;
	float	_exposure;

	float4	_debugValue;
}

Texture2D<float>		_texHeight : register( t0 );
Texture2D<float3>		_texNormal : register( t1 );
Texture2D<float2>		_texAO : register( t2 );
Texture2DArray<float4>	_texGroundTruthIrradiance : register( t3 );
Texture2D<float4>		_texBentCone : register( t4 );

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

float4	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _resolution;
	float2	dUV;

	const float	PIXEL_GAP = 2.0;

	float	H0 = _texHeight.SampleLevel( LinearClamp, UV, 0.0 );
	for ( int Dy=-4; Dy <= 4; Dy++ ) {
		dUV.y = PIXEL_GAP * Dy / _resolution.y;
		for ( int Dx=-4; Dx <= 4; Dx++ ) {
			dUV.x = PIXEL_GAP * Dx / _resolution.x;

			float	H = _texHeight.SampleLevel( LinearClamp, UV + dUV, 0.0 );
		}
	}

	float2	AO_E0 = _texAO.Sample( LinearClamp, UV );
	return AO_E0.x / (2.0*PI);
}
