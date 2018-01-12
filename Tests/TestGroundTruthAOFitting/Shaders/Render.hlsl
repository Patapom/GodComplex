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
	float	_exposure;

	float4	_debugValue;
}

Texture2D<float>	_texHeight : register( t0 );
Texture2D<float3>	_texNormal : register( t1 );
Texture2D<float2>	_texAO : register( t2 );

//Texture2DArray<float>	_texIrradianceBounces : register( t3 );
//Texture2D<float4>	_texGroundTruth : register( t4 );
Texture2DArray<float4>	_texGroundTruth : register( t3 );

Texture2D<float4>	_texBentCone : register( t4 );

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
	return (_rho / PI) * E * _E0;
*/

//return _texGroundTruth.Sample( LinearClamp, _UV ).xyz;
//return (_rho / PI) * _texGroundTruth.Sample( LinearClamp, float3( _UV, _bounceIndex ) ).xyz;

	float3	L = (_rho / PI) * _texGroundTruth.Sample( LinearClamp, float3( _UV, 0 ) ).xyz;
//	for ( uint bounceIndex=1; bounceIndex <= 20; bounceIndex++ ) {
//		L += (_rho / PI) * _texGroundTruth.Sample( LinearClamp, float3( _UV, bounceIndex ) ).xyz;
//	}
	return L;
}

// Computes an improved AO factor based on original AO and surface reflectance
float3	ComputeAO( float _AO, float _tauFactor, float3 _rho ) {
	const float	A = 27.576937094210384876503293303541;
	const float	B = 3.3364392003423804;

	_AO = saturate( _AO );

	float	F0 = _AO * (1.0 + 0.5 * pow( 1.0 - _AO, 0.75 ));						// = \[Alpha]*(1 + 0.5*(1 - \[Alpha])^0.75)
	float	F1 = A * _AO * pow( 1.0 - _AO, 1.5 ) * exp( -B * pow( _AO, 0.25 ) );	// = `27.576937094210384876503293303541\)*\[Alpha]*(1 - \[Alpha])^1.5 * Exp[-\!\(TraditionalForm\`3.3364392003423804\)*Sqrt[Sqrt[\[Alpha]]]
	float	tau = 1.0 - F1 / (0.01 + saturate( 1.0 - F0 ));
//	return F0 + _rho / (1.0 - _rho * tau) * F1;
	return F0 + _rho / (1.0 - saturate(_rho * _tauFactor * tau)) * F1;
}

// Computes an improved AO factor based on original AO and surface reflectance
float3	ComputeAO_TEST( float _AO, float _tauFactor, float3 _rho ) {
	const float	A = 27.576937094210384876503293303541;
	const float	B = 3.3364392003423804;

	_AO = saturate( _AO );
//return _AO;

//_AO *= _AO;

	float	F0 = _AO * (1.0 + 0.5 * pow( 1.0 - _AO, 0.75 ));						// = \[Alpha]*(1 + 0.5*(1 - \[Alpha])^0.75)


return F0;


	float	F1 = A * _AO * pow( 1.0 - _AO, 1.5 ) * exp( -B * pow( _AO, 0.25 ) );	// = `27.576937094210384876503293303541\)*\[Alpha]*(1 - \[Alpha])^1.5 * Exp[-\!\(TraditionalForm\`3.3364392003423804\)*Sqrt[Sqrt[\[Alpha]]]
	float	tau = 1.0 - F1 / (0.01 + saturate( 1.0 - F0 ));
//	return F0 + _rho / (1.0 - _rho * tau) * F1;
	return F0 + _rho / (1.0 - saturate(_rho * _tauFactor * tau)) * F1;
}

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _resolution;
	float2	AO_E0 = _texAO.Sample( LinearClamp, UV );
	float3	AO = AO_E0.x / (2.0 * PI);
	float3	normal = 2.0 * _texNormal.Sample( LinearClamp, UV ) - 1.0;

	float3	SH[9] = { _SH[0].xyz, _SH[1].xyz, _SH[2].xyz, _SH[3].xyz, _SH[4].xyz, _SH[5].xyz, _SH[6].xyz, _SH[7].xyz, _SH[8].xyz };
	float3	E0 = EvaluateSHIrradiance( normal, SH );

	float	tauFactor = 1.0;
	if ( (_flags & 0x3U) == 2 ) {
		// Estimate SH in the direction of the bent normal, and reduce the irradiance integration to the aperture of the cone
		float4	bentCone = _texBentCone.Sample( LinearClamp, UV );
				bentCone.xyz = 2.0 * bentCone.xyz - 1.0;
//return normal.xyz;
//return bentCone.xyz;

//bentCone.w = pow( saturate( bentCone.w ), 2.0 * _debugValue.x );
bentCone.w = lerp( saturate( bentCone.w ), 1.0, _debugValue.x );

//bentCone.w = 0.9999;


		float3	bentNormal = normalize( bentCone.xyz );
		float	cosTheta = cos( bentCone.w * 0.5 * PI );
//cosTheta = _debugValue.x;
//cosTheta = lerp( 0.0, cos( bentCone.w * 0.5 * PI ), _debugValue.x );
		E0 = EvaluateSHIrradiance( bentNormal, cosTheta, SH );
//		E0 = EvaluateSHIrradiance( bentNormal, SH );

//E0 *= 1.5;
//E0 *= bentCone.w * 1.5;

//AO = AO_E0.y / PI;		// GOOD RESULTS!!
//AO = AO.x * (1.0 + 0.5 * pow( saturate( 1.0 - AO.x ), 0.75 ));
//return AO;

//float	boostFactor = AO.x / (1.0 - cosTheta);	// Use ratio of solid angles (assume 2PI*AO = solid angle of unoccluded rays, while 2PI(1-cos(theta)) = solid angle covered by bent cone)
float boostFactor = AO_E0.y / PI / AO.x;		// Use ratio of unit irradiance over AO
//float boostFactor = 1.0 + 0.5 * pow( saturate( 1.0 - AO.x ), 0.75 );	// Use F0*AO / AO
//return 0.8 * boostFactor;
//return boostFactor > 1.0 ? float3( boostFactor - 1.0, 0, 0 ) : boostFactor;
//return dot( normal, bentNormal );

E0 *= lerp( 1.0, boostFactor, _debugValue.y );

//		tauFactor = 1.0 + 0.5 * dot( bentCone.xyz, normal );
//		tauFactor = 1.0 + (1.0-bentCone.w) * dot( bentCone.xyz, normal );
	}

	switch ( _flags & 0x3U ) {
	case 1:
	case 2:
//		AO = ComputeAO( AO.x, tauFactor, _rho );
		AO = ComputeAO_TEST( AO.x, tauFactor, _rho );
		break;
	case 3:
		return _exposure * GroundTruth( UV, _rho, E0 );
//		return _exposure * (dot( GroundTruth( UV, _rho, E0 ), LUMINANCE ) - dot( (_rho / PI) * E0 * AO, LUMINANCE ));
	}

	return _exposure * (_rho / PI) * E0 * AO;
}
