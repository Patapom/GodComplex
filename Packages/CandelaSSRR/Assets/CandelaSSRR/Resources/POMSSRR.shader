Shader "Hidden/POMSSRR" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
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
//uniform float4		_ScreenParams;	// XY=screen size

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
	float4	SourceColor = _tex2Dlod(_MainTex, float4( _In.uv, 0, 0.0 ) );
	if ( SourceColor.w == 0.0 )
		return 0.0;

	float4	Result = _SSRRcomposeMode > 0.0 ? float4( SourceColor.xyz, 0.0 ) : 0.0;

	float	Zproj = _tex2Dlod( _CameraDepthTexture, float4( _In.uv, 0, 0.0 ) ).x;
	float	Z = UnProject( Zproj );
	if ( Z > _maxDepthCull )
		return 0.0;

//return 80 * Z;

	// Compute position in projected space
	float4	projPosition = float4( 2.0 * _In.uv - 1.0, Zproj, 1.0 );

	// Transform back into camera space
	float4	csPosition = mul( _ProjectionInv, projPosition );
			csPosition = csPosition / csPosition.w;

	// Compute view vector
	float3	csView = normalize( csPosition.xyz );

//return float4( csPosition.xy, -csPosition.z, csPosition.w );

	float4	wsNormal = float4( 2.0 * _tex2Dlod( _CameraNormalsTexture, float4( _In.uv, 0, 0.0 ) ).xyz - 1.0, 0.0 );
	float3	csNormal = normalize( mul( _ViewMatrix, wsNormal ).xyz );

//return float4( csNormal, 0 );

//	float3	csReflectedView = normalize( csView - 2.0 * dot( csNormal, csView) * csNormal );
	float3	csReflectedView = csView - 2.0 * dot( csNormal, csView ) * csNormal;	// No use to normalize this!
//return float4( csReflectedView, 1 );

	float4	projOffsetPosition = mul( _ProjMatrix, float4( csPosition.xyz + csReflectedView, 1.0 ) );	// Position offset by reflected view
			projOffsetPosition /= projOffsetPosition.w;
	float3	projRay = normalize( projOffsetPosition - projPosition.xyz );	// Some kind of target vector in projective space (the ray?)
//return float4( projRay, 1 );


// ScreenWidth = _ScreenParams.xy = (687, 325)
//return 0.5 * _ScreenParams.x / 687.0;
//return 0.5 * _ScreenParams.y / 325.0;

	float3	baseRay = float3( 0.5 * projRay.xy, projRay.z );		// 0.5 because we're tracing in UV space, not in [-1,+1] NDC space
	float	baseRayUVLength = length( baseRay.xy );
			baseRay *= 2.0 / (_ScreenParams.x * baseRayUVLength);	
	float3	globalRay = baseRay * _stepGlobalScale;

	int		MaxGlobalStepsCount = int( _maxStep );
	float	reflectionDistance = 0.0;

	float4	projHitPosition = float4( float3( _In.uv, Zproj ) + globalRay, 0.0 );
	float	ScreenZ, CurrentZ;
	for ( int GlobalStepIndex=0; GlobalStepIndex < MaxGlobalStepsCount; GlobalStepIndex++ )
	{
		ScreenZ = UnProject( _tex2Dlod( _CameraDepthTexture, float4( projHitPosition.xy, 0, 0.0 ) ).x );
		CurrentZ = UnProject( projHitPosition.z );
		if ( ScreenZ < CurrentZ - 1e-06 )
		{	// Got a hit
			projHitPosition.w = 1.0;
			break;
		}

		projHitPosition.xyz += globalRay;
		reflectionDistance += 1.0;
	}

	if ( abs( projHitPosition.x - 0.5 ) > 0.5 || abs( projHitPosition.y - 0.5 ) > 0.5 )
		return Result;	// Out of screen...

	if ( UnProject( projHitPosition.z ) > _maxDepthCull || projHitPosition.z < 0.1 )
		return float4( 0.0, 0.0, 0.0, 0.0 );	// Out of Z range

	if ( projHitPosition.w > 1.0-0.01 )
	{	// Fine step tracing using binary search (i.e. dichotomic interval reduction)
		projHitPosition.w = 0.0;	// Reset intersection blend to "no intersection"

		int		MaxFineStepsCount = int( _maxFineStep );

		projHitPosition.xyz -= globalRay;					// Go back one step, before the rough intersection
		float3	projIntervalPositionStart = projHitPosition.xyz;
		projHitPosition.xyz += baseRay;						// Interval end is one step forward
		float3	fineRay = baseRay;							// Start with full length base ray

		for ( int FineStepIndex=0; FineStepIndex < MaxFineStepsCount; FineStepIndex++ )
		{
			float	FineScreenZ = UnProject( _tex2Dlod( _CameraDepthTexture, float4( projHitPosition.xy, 0, 0.0 ) ).x );
			float	FineCurrentZ = UnProject( projHitPosition.z );
			if ( FineScreenZ < FineCurrentZ )
			{	// Screen Z is in front of current Z
#if 0
				// Reduce interval length and recompute end position (not moving start position since we have a hit in that interval)
				if ( FineCurrentZ - FineScreenZ < _bias )
				{	// If discrepancy is too low then assume a correct hit
					projHitPosition.w = 1.0;
					break;
				}

				fineRay *= 0.5;	// Halve marching step
				projHitPosition.xyz = projIntervalPositionStart + fineRay;	// New end position is at the end of the new interval

#else
				// POM: Compute fine intersection
				float	Z0 = CurrentZ;
				float	DZ0 = FineCurrentZ - CurrentZ;
				float	Z1 = ScreenZ;
				float	DZ1 = FineScreenZ - ScreenZ;
				float	t = (DZ1 - DZ0) / (Z0 - Z1);
				projHitPosition = float4( lerp( projIntervalPositionStart, projHitPosition, t ), 1.0 );
				break;
#endif
			}
			else
			{	// Make the interval march forward
				projIntervalPositionStart = projHitPosition.xyz;
				projHitPosition.xyz += baseRay;

				ScreenZ = FineScreenZ;
				CurrentZ = FineCurrentZ;
			}
		}
	}

	if ( projHitPosition.w < 0.01 )
		return Result;	// Opacity too low, no hit...

	// Retrieve scene's color at new UV
	Result.xyz = _tex2Dlod( _MainTex, float4( projHitPosition.xy, 0, 0.0 ) ).xyz;
	Result.w = projHitPosition.w * (1.0 - (Z / _maxDepthCull)) * (1.0 - pow( reflectionDistance / _maxStep, _fadePower))
			 * pow( saturate( dot( normalize(csReflectedView), normalize(csPosition).xyz ) + 1.0 + 0.1 * _fadePower ), _fadePower );
//*/

	return Result;
}
ENDCG

	}	//	Pass
}	//SubShader

Fallback off
}	// Shader