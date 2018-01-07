////////////////////////////////////////////////////////////////////////////////
// Improved AO demo
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "SphericalHarmonics.hlsl"

cbuffer	CBMain : register( b0 ) {
	uint2	_resolution;		// 
	uint	_flags;				// 
	uint	_bounceIndex;

	float3	_rho;
}

Texture2D<float>	_texHeight : register( t0 );
Texture2D<float3>	_texNormal : register( t1 );
Texture2D<float2>	_texAO : register( t2 );

//Texture2DArray<float>	_texIrradianceBounces : register( t3 );
//Texture2D<float4>	_texGroundTruth : register( t4 );
Texture2DArray<float4>	_texGroundTruth : register( t3 );

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

float3	GroundTruth( float2 _UV, float3 _rho, float3 _E0 ) {
	float3	E = 0.0;//(_rho / PI) * _texIrradianceBounces.Sample( LinearClamp, float3( _UV, 0 ) );
//	for ( uint bounceIndex=20; bounceIndex > 0; bounceIndex-- ) {
//		E *= (_rho / PI);
//		E += _texIrradianceBounces.Sample( LinearClamp, float3( _UV, bounceIndex ) );
//	}

/*
#if 0
	float3	r = saturate( _rho / PI );
//	float3	r = saturate( _rho );
	E += pow( r, 1.0 ) * _texIrradianceBounces.Sample( LinearClamp, float3( _UV, 0 ) );
	E += pow( r, 2.0 ) * _texIrradianceBounces.Sample( LinearClamp, float3( _UV, 1 ) );
	E += pow( r, 3.0 ) * _texIrradianceBounces.Sample( LinearClamp, float3( _UV, 2 ) );
	E += pow( r, 4.0 ) * _texIrradianceBounces.Sample( LinearClamp, float3( _UV, 3 ) );
#else
	float3	r = saturate( _rho / PI );
	for ( uint bounceIndex=0; bounceIndex < 20; bounceIndex++ ) {
		E += pow( r, 1+bounceIndex ) * _texIrradianceBounces.Sample( LinearClamp, float3( _UV, bounceIndex ) );
	}
#endif
*/

//return _texGroundTruth.Sample( LinearClamp, _UV ).xyz;
return (_rho / PI) * _texGroundTruth.Sample( LinearClamp, float3( _UV, _bounceIndex ) ).xyz;


	return (_rho / PI) * E * _E0;
}

// Computes an improved AO factor based on original AO and surface reflectance
float3	ComputeAO( float _AO, float3 _rho ) {
//	float	a = _AO;
//	float	ra = 1.0 - a;

	const float	A = 27.576937094210384876503293303541;
	const float	B = 3.3364392003423804;

	_AO = saturate( _AO );

	float	F0 = _AO * (1.0 + 0.5 * pow( 1.0 - _AO, 0.75 ));						// = \[Alpha]*(1 + 0.5*(1 - \[Alpha])^0.75)
	float	F1 = A * _AO * pow( 1.0 - _AO, 1.5 ) * exp( -B * pow( _AO, 0.25 ) );	// = `27.576937094210384876503293303541\)*\[Alpha]*(1 - \[Alpha])^1.5 * Exp[-\!\(TraditionalForm\`3.3364392003423804\)*Sqrt[Sqrt[\[Alpha]]]
	float	tau = 1.0 - F1 / (0.01 + saturate( 1.0 - F0 ));
//	return F0 + _rho / (1.0 - _rho * tau) * F1;
	return F0 + _rho / (PI - _rho * tau) * F1;
}

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _resolution;
	float3	AO = _texAO.Sample( LinearClamp, UV ).x;
	float3	normal = 2.0 * _texNormal.Sample( LinearClamp, UV ) - 1.0;

	float3	SH[9] = { _SH[0].xyz, _SH[1].xyz, _SH[2].xyz, _SH[3].xyz, _SH[4].xyz, _SH[5].xyz, _SH[6].xyz, _SH[7].xyz, _SH[8].xyz };
	float3	E0 = EvaluateSHIrradiance( normal, SH );

	switch ( _flags & 0x3U ) {
	case 1:
		AO = ComputeAO( AO.x, _rho );
		break;
	case 2:
		return GroundTruth( UV, _rho, E0 );
	}

	return (_rho / PI) * E0 * AO;

//	return _reflectance;
//	return pow( saturate( AO ), 2.2 );
//	return float3( UV, 0 );
}
