Shader "Hidden/CandelaSSRRv1_POM" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

//================================================================================================================================================
//USING UNITY DEPTH TEXTURE - BEGIN OF PASS 2
//================================================================================================================================================
	
SubShader {
	ZTest Always Cull Off ZWrite Off Fog { Mode Off }
	Pass {

CGPROGRAM
#pragma target 3.0
#pragma vertex VS
#pragma fragment PS
#pragma fragmentoption ARB_precision_hint_fastest
//#extension GL_ARB_shader_texture_lod : enable
#pragma glsl
#include "UnityCG.cginc"

uniform float		_SSRRcomposeMode;
uniform sampler2D	_CameraDepthTexture;
uniform sampler2D	_CameraNormalsTexture;
uniform float4x4	_ViewMatrix;
uniform float4x4	_ProjectionInv;
uniform float4x4	_ProjMatrix;
uniform float		_bias;
uniform float		_stepGlobalScale;
uniform float		_maxStep;
uniform float		_maxFineStep;
uniform float		_maxDepthCull;
uniform float		_fadePower;
uniform sampler2D	_MainTex;
//uniform float4		_ZBufferParams;
//uniform float4		_ScreenParams;

#if TARGET_GLSL
#define	_tex2Dlod( s, uv ) tex2D( s, uv.xy )	// SIMPLE AS THAT???? Ôõ
#else
#define	_tex2Dlod( s, uv ) tex2Dlod( s, uv )
#endif

struct PS_IN {
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
};

PS_IN VS( appdata_img v )
{
	PS_IN o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	o.uv = MultiplyUV( UNITY_MATRIX_TEXTURE0, v.texcoord );

	return o;
}

float	UnProject( float _Zproj )
{
	return 1.0 / (_ZBufferParams.x * _Zproj + _ZBufferParams.y);
}

float4	PS( PS_IN _In ) : COLOR
{
	float4 Result = 0.0;
	float4 SourceColor = _tex2Dlod(_MainTex, float4( _In.uv, 0, 0.0 ) );

	if ( SourceColor.w == 0.0 ) {
		Result = float4(0.0, 0.0, 0.0, 0.0);
		return Result;
	}

//return float4( 1, 1, 0, 1 );

//*
	float	Zproj = _tex2Dlod(_CameraDepthTexture, float4(_In.uv, 0, 0.0)).x;
	float	Z = UnProject( Zproj );
//return 80 * Z;

	if ( Z > _maxDepthCull )
		return 0.0;

	int tmpvar_23 = int(_maxStep);

	// Compute position in projected space
	float4	projPosition = float4( (_In.uv * 2.0) - 1.0, Zproj, 1.0 );

	// Transform back into camera space
	float4	csPosition = mul( _ProjectionInv, projPosition );
			csPosition = csPosition / csPosition.w;

	// Compute view vector
	float3	csView = normalize( csPosition.xyz );

//return float4( csPosition.xy, -csPosition.z, csPosition.w );

	float4 wsNormal;
	wsNormal.w = 0.0;
	wsNormal.xyz = 2.0 * _tex2Dlod( _CameraNormalsTexture, float4(_In.uv, 0, 0.0) ).xyz - 1.0;

	float3 csNormal;
	csNormal = normalize( mul(_ViewMatrix, wsNormal).xyz );

//return float4( csNormal, 0 );

//	float3	csReflectedView = normalize( csView - (2.0 * (dot( csNormal, csView) * csNormal)) );
	float3	csReflectedView = csView - (2.0 * (dot( csNormal, csView) * csNormal));	// No use to normalize this!
//return float4( csReflectedView, 1 );

	float4	projOffsetPosition = mul( _ProjMatrix, float4( csPosition.xyz + csReflectedView, 1.0 ) );	// Position offset by reflected view
			projOffsetPosition /= projOffsetPosition.w;
	float3	projRay = normalize( projOffsetPosition - projPosition.xyz );	// Some kind of target vector in projective space (the ray?)
//return float4( projRay, 1 );


// ScreenWidth = _ScreenParams.xy = (687, 325)
//return 0.5 * _ScreenParams.x / 687.0;
//return 0.5 * _ScreenParams.y / 325.0;

	float3	baseRay = float3( 0.5 * projRay.xy, projRay.z );
	float	baseRayUVLength = length( baseRay.xy );
	float3	globalRay = baseRay * ((2.0 * _stepGlobalScale / _ScreenParams.x) / baseRayUVLength);

	int		MaxGlobalStepsCount = int( _maxStep );
	float	reflectionDistance = 0.0;

	bool	globalHitValid = false;

	float3	projCurrentPos = float3( _In.uv, Zproj ) + globalRay;
	float4	projHitPosition;
	for ( int GlobalStepIndex=0; GlobalStepIndex < MaxGlobalStepsCount; GlobalStepIndex++ )
	{
		float	ScreenZ = UnProject( _tex2Dlod( _CameraDepthTexture, float4( projCurrentPos.xy, 0, 0.0 ) ).x );//1.0 / (_ZBufferParams.x * _tex2Dlod( _CameraDepthTexture, float4( projCurrentPos.xy, 0, 0.0 ) ).x + _ZBufferParams.y);
		float	CurrentZ = UnProject( projCurrentPos.z );//1.0 / (_ZBufferParams.x * projCurrentPos.z + _ZBufferParams.y);
		if ( ScreenZ < CurrentZ - 1e-06 )
		{	// Got a hit
			projHitPosition = float4( projCurrentPos, 1.0 );	// W set to 1
			globalHitValid = true;
			break;
		}

		projCurrentPos += globalRay;
		reflectionDistance += 1.0;
	}

	if ( !globalHitValid )
		projHitPosition = float4( projCurrentPos, 0.0 );	// W set to 0 if no hit

	float4 acccols_8 = _SSRRcomposeMode > 0.0 ? float4( SourceColor.xyz, 0.0 ) : float4( 0.0, 0.0, 0.0, 0.0 );
	if ( abs( projHitPosition.x - 0.5 ) > 0.5 || abs( projHitPosition.y - 0.5 ) > 0.5 )
		return acccols_8;

	if ( UnProject( projHitPosition.z ) > _maxDepthCull || projHitPosition.z < 0.1 )
		return float4( 0.0, 0.0, 0.0, 0.0 );

	float4	opahwcte_2 = projHitPosition;
	if ( projHitPosition.w == 1.0 )
	{	// Fine step tracing using binary search (i.e. dichotomic interval reduction)
		float4	alsdmes_45;
		float3	projStartPosition = projHitPosition.xyz - globalRay;
		float3	originalLengthFineRay = baseRay * ((2.0 / _ScreenParams.x) / baseRayUVLength);
		float3	fineRay = originalLengthFineRay;	// Start with full length fine ray

		int		MaxFineStepsCount = int( _maxFineStep );
		bool	fineHitValid = false;
		float3	projIntervalPositionStart = projStartPosition;
		float3	projIntervalPositionEnd = projStartPosition + fineRay;
		for ( int FineStepIndex=0; FineStepIndex < 20; FineStepIndex++ )
		{
			if ( FineStepIndex >= MaxFineStepsCount )
				break;

			float	ScreenZ = UnProject( _tex2Dlod( _CameraDepthTexture, float4( projIntervalPositionEnd.xy, 0, 0.0 ) ).x );
			float	CurrentZ = UnProject( projIntervalPositionEnd.z );
			if ( ScreenZ < CurrentZ )
			{
				if ( CurrentZ - ScreenZ < _bias )
				{
					alsdmes_45 = float4( projIntervalPositionEnd, 1.0 );
					fineHitValid = true;
					break;
				}

				fineRay *= 0.5;
				projIntervalPositionEnd = projIntervalPositionStart + fineRay;
			}
			else
			{
				projIntervalPositionStart = projIntervalPositionEnd;
				projIntervalPositionEnd = projIntervalPositionEnd + originalLengthFineRay;
			}
		}

		if ( !fineHitValid )
		{
			alsdmes_45 = float4( projIntervalPositionEnd, 0.0 );
			fineHitValid = true;
		}

		opahwcte_2 = alsdmes_45;
	}

	if ( opahwcte_2.w < 0.01 )
		return acccols_8;

	Result.xyz = _tex2Dlod( _MainTex, float4( opahwcte_2.xy, 0, 0.0 ) ).xyz;
	Result.w = (((opahwcte_2.w * (1.0 - (Z / _maxDepthCull))) * (1.0 - pow ( reflectionDistance / float(tmpvar_23), _fadePower))) * pow (clamp (((dot (normalize(csReflectedView), normalize(csPosition).xyz) + 1.0) + (_fadePower * 0.1)), 0.0, 1.0), _fadePower));
//*/

	return Result;
}
ENDCG

	}	//	Pass
}	//SubShader

Fallback off
}	// Shader