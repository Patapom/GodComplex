////////////////////////////////////////////////////////////////////////////////
// Improved AO demo
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "SphericalHarmonics.hlsl"

//#define	GROUND_TRUTH_MAX_BOUNCES	20
#define	GROUND_TRUTH_MAX_BOUNCES	1

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
//Texture2D<float4>	_texGroundTruthIrradiance : register( t4 );
Texture2DArray<float4>	_texGroundTruthIrradiance : register( t3 );

Texture2D<float4>	_texBentCone : register( t4 );

Texture2D<float4>	_texIndirectMap : register( t5 );


struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

float3	GroundTruth( float2 _UV, float3 _rho ) {
	float3	E0 = _texGroundTruthIrradiance.Sample( LinearClamp, float3( _UV, 0 ) ).xyz;			// Direct irradiance E0
	float3	E = E0;
	#if GROUND_TRUTH_MAX_BOUNCES > 1
		for ( uint bounceIndex=1; bounceIndex <= GROUND_TRUTH_MAX_BOUNCES; bounceIndex++ ) {
			E += _texGroundTruthIrradiance.Sample( LinearClamp, float3( _UV, bounceIndex ) ).xyz;	// + Indirect irradiance Ei
		}
	#elif GROUND_TRUTH_MAX_BOUNCES > 0
		E += _texGroundTruthIrradiance.Sample( LinearClamp, float3( _UV, 1 ) ).xyz;	// + Indirect irradiance E1
	#endif

	return (_rho / PI) * E;		// E * Diffuse BRDF = Radiance
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
float3	ComputeAO_TEST( float2 _AO_E0, float _tauFactor, float3 _rho ) {
	const float	A = 27.576937094210384876503293303541;
	const float	B = 3.3364392003423804;

	_AO_E0 = saturate( _AO_E0 / float2( 2.0 * PI, PI ) );
//return _AO;
//_AO *= _AO;

	float	F0 = _AO_E0.x * (1.0 + 0.5 * pow( 1.0 - _AO_E0.x, 0.75 ));						// = \[Alpha]*(1 + 0.5*(1 - \[Alpha])^0.75)


#if GROUND_TRUTH_MAX_BOUNCES == 0
return F0;
#endif


float	tempF1 = _debugValue.x * pow( 1.0 - _AO_E0.y, _debugValue.y );
//float	tempF1 = 0.4444 * pow( 1.0 - _AO_E0.y, 1.037 );	// Cool values
return F0 + tempF1;


	float	F1 = A * _AO_E0.x * pow( 1.0 - _AO_E0.x, 1.5 ) * exp( -B * pow( _AO_E0.x, 0.25 ) );	// = `27.576937094210384876503293303541\)*\[Alpha]*(1 - \[Alpha])^1.5 * Exp[-\!\(TraditionalForm\`3.3364392003423804\)*Sqrt[Sqrt[\[Alpha]]]
	float	tau = 1.0 - F1 / (0.01 + saturate( 1.0 - F0 ));
//	return F0 + _rho / (1.0 - _rho * tau) * F1;
	return F0 + _rho / (1.0 - saturate(_rho * _tauFactor * tau)) * F1;
}

float3	EvaluateSHIrradianceBentCone( float4 _bentCone, float2 _AO_E0, float3 _SH[9] ) {

	// Reduce bent cone angle
	// TODO: Compute it better to avoid using this!!! STOP USING MIN/MAX, THIS IS SHIT!
	_bentCone.w = 0.2 + 0.8 * saturate( _bentCone.w );

	// Estimate reduced irradiance in the bent cone's direction
	float3	bentNormal = normalize( _bentCone.xyz );
	float	cosAlpha = cos( _bentCone.w * 0.5 * PI );
	float3	E0 = EvaluateSHIrradiance( bentNormal, cosAlpha, _SH );

	// Boost resulting irradiance
	float boostFactor = 2.0 * _AO_E0.y / _AO_E0.x;		// Use ratio of unit irradiance over AO to boost result
	E0 *= 0.5 * (1.0 + boostFactor);

	return E0;
}

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _resolution;
	float2	AO_E0 = _texAO.Sample( LinearClamp, UV );
	float3	normal = 2.0 * _texNormal.Sample( LinearClamp, UV ) - 1.0;
			normal.y *= -1.0;	// Reverse Y (done everywhere else at texture creation level, but not here)

	float3	SH[9] = { _SH[0].xyz, _SH[1].xyz, _SH[2].xyz, _SH[3].xyz, _SH[4].xyz, _SH[5].xyz, _SH[6].xyz, _SH[7].xyz, _SH[8].xyz };

	///////////////////////////////////////////////////////////
	// Estimate direct irradiance
	float3	E0 = 0.0;
	float	tauFactor = 1.0;
	if ( (_flags & 0x3U) == 2 ) {
		// Estimate SH in the direction of the bent normal, and reduce the irradiance integration to the aperture of the cone
		float4	bentCone = _texBentCone.Sample( LinearClamp, UV );
				bentCone.xyz = 2.0 * bentCone.xyz - 1.0;
				bentCone.y *= -1.0;	// Reverse Y (done everywhere else at texture creation level, but not here)
//return normal.xyz;
//return bentCone.xyz;

//return _debugValue.x * pow( saturate(1 - AO_E0.y/PI), _debugValue.y );
//return 1 - dot( bentCone.xyz, normal );

#if 1
		E0 = EvaluateSHIrradianceBentCone( bentCone, AO_E0, SH );
#else
//bentCone.w = pow( saturate( bentCone.w ), 2.0 * _debugValue.x );	// Not very manageable
bentCone.w = lerp( saturate( bentCone.w ), 1.0, _debugValue.x );	// _debugValue.x = 0.33333 OR 0.2

//bentCone.w = 0.9999;


		float3	bentNormal = normalize( bentCone.xyz );
		float	cosAlpha = cos( bentCone.w * 0.5 * PI );
//cosAlpha = _debugValue.x;
//cosAlpha = lerp( 0.0, cos( bentCone.w * 0.5 * PI ), _debugValue.x );
		E0 = EvaluateSHIrradiance( bentNormal, cosAlpha, SH );
//		E0 = EvaluateSHIrradiance( bentNormal, SH );

//E0 *= 1.5;
//E0 *= bentCone.w * 1.5;

//AO = AO_E0.y / PI;		// GOOD RESULTS!!
//AO = AO.x * (1.0 + 0.5 * pow( saturate( 1.0 - AO.x ), 0.75 ));
//return AO;

//float	boostFactor = AO.x / (1.0 - cosAlpha);	// Use ratio of solid angles (assume 2PI*AO = solid angle of unoccluded rays, while 2PI(1-cos(theta)) = solid angle covered by bent cone)
//float boostFactor = AO_E0.y / PI / AO.x;		// Use ratio of unit irradiance over AO
float boostFactor = 2.0 * AO_E0.y / AO_E0.x;		// Use ratio of unit irradiance over AO
//float boostFactor = 1.0 + 0.5 * pow( saturate( 1.0 - AO.x ), 0.75 );	// Use F0*AO / AO
//return 0.8 * boostFactor;
//return boostFactor > 1.0 ? float3( boostFactor - 1.0, 0, 0 ) : boostFactor;
//return dot( normal, bentNormal );

E0 *= lerp( 1.0, boostFactor, _debugValue.y );	// _debugValue.y = 0.5185
//E0 *= 0.5 * (1.0 + boostFactor);
#endif

//		tauFactor = 1.0 + 0.5 * dot( bentCone.xyz, normal );
//		tauFactor = 1.0 + (1.0-bentCone.w) * dot( bentCone.xyz, normal );
	} else {
		E0 = EvaluateSHIrradiance( normal, SH );
	}

	///////////////////////////////////////////////////////////
	// Analytical solution
	float3	AO = AO_E0.x / (2.0 * PI);
	if ( (_flags & 0x3U) == 1 || (_flags & 0x3U) == 2 ) {
//		AO = ComputeAO( AO.x, tauFactor, _rho );
		AO = ComputeAO_TEST( AO_E0, tauFactor, _rho );
	}
	float3	resultAnalytical = (_rho / PI) * E0 * AO;

	///////////////////////////////////////////////////////////
	// Ground Truth solution
	float3	resultGroundTruth = GroundTruth( UV, _rho );
//	float3	resultGroundTruth = (dot( GroundTruth( UV, _rho ), LUMINANCE ) - dot( (_rho / PI) * E0 * AO, LUMINANCE ));

	///////////////////////////////////////////////////////////
	// Combine result
	float3	result = resultAnalytical;
	if ( (_flags & 0x3U) == 3 )
		result = resultGroundTruth;

	if ( _flags & 0x10U )
		result = abs( resultAnalytical - GroundTruth( UV, _rho ) );	// Show difference

result = _texIndirectMap.Sample( LinearClamp, UV ).xyz;

	return _exposure * result;
}
