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

float4	PS( PS_IN _In ) : COLOR
{
	float3 lkjwejhsdkl_1;
	float4 opahwcte_2;
	float4 Result = 0.0;
	float4 SourceColor = _tex2Dlod(_MainTex, float4( _In.uv, 0, 0.0 ) );

	if ( SourceColor.w == 0.0 ) {
		Result = float4(0.0, 0.0, 0.0, 0.0);
		return Result;
	}

//return float4( 1, 1, 0, 1 );

//*
	float4 tmpvar_5;
	tmpvar_5 = _tex2Dlod(_CameraDepthTexture, float4(_In.uv, 0, 0.0));
	float tmpvar_6;
	tmpvar_6 = tmpvar_5.x;
	float tmpvar_7;
	tmpvar_7 = (1.0/(((_ZBufferParams.x * tmpvar_5.x) + _ZBufferParams.y)));
	if ((tmpvar_7 > _maxDepthCull)) {
		Result = float4(0.0, 0.0, 0.0, 0.0);
	} else {
		float4 acccols_8;
		int s_9;
		float4 uiduefa_10;
		int icoiuf_11;
		bool biifejd_12;
		float4 rensfief_13;
		float lenfaiejd_14;
		int vbdueff_15;
		float3 eiieiaced_16;
		float3 jjdafhue_17;
		float3 hgeiald_18;
		float4 loveeaed_19;
		float4 mcjkfeeieijd_20;
		float3 xvzyufalj_21;
		float4 efljafolclsdf_22;
		int tmpvar_23;
		tmpvar_23 = int(_maxStep);
		efljafolclsdf_22.w = 1.0;
		efljafolclsdf_22.xy = ((_In.uv * 2.0) - 1.0);
		efljafolclsdf_22.z = tmpvar_6;
		float4 tmpvar_24;
		tmpvar_24 = mul(_ProjectionInv, efljafolclsdf_22);
		float4 tmpvar_25;
		tmpvar_25 = (tmpvar_24 / tmpvar_24.w);
		xvzyufalj_21.xy = efljafolclsdf_22.xy;
		xvzyufalj_21.z = tmpvar_6;
		mcjkfeeieijd_20.w = 0.0;
		mcjkfeeieijd_20.xyz = ((_tex2Dlod(_CameraNormalsTexture, float4(_In.uv, 0, 0.0)).xyz * 2.0) - 1.0);
		float3 tmpvar_26;
		tmpvar_26 = normalize(tmpvar_25.xyz);
		float3 tmpvar_27;
		tmpvar_27 = normalize(mul(_ViewMatrix, mcjkfeeieijd_20).xyz);
		float3 tmpvar_28;
		tmpvar_28 = normalize((tmpvar_26 - (2.0 * (dot (tmpvar_27, tmpvar_26) * tmpvar_27))));
		loveeaed_19.w = 1.0;
		loveeaed_19.xyz = (tmpvar_25.xyz + tmpvar_28);
		float4 tmpvar_29;
		tmpvar_29 = mul(_ProjMatrix, loveeaed_19);
		float3 tmpvar_30;
		tmpvar_30 = normalize(((tmpvar_29.xyz / tmpvar_29.w) - xvzyufalj_21));
		lkjwejhsdkl_1.z = tmpvar_30.z;
		lkjwejhsdkl_1.xy = (tmpvar_30.xy * 0.5);
		hgeiald_18.xy = _In.uv;
		hgeiald_18.z = tmpvar_6;
		float tmpvar_31;
		tmpvar_31 = (2.0 / _ScreenParams.x);
		float tmpvar_32;
		tmpvar_32 = sqrt(dot (lkjwejhsdkl_1.xy, lkjwejhsdkl_1.xy));
		float3 tmpvar_33;
		tmpvar_33 = (lkjwejhsdkl_1 * ((tmpvar_31 * _stepGlobalScale) / tmpvar_32));
		jjdafhue_17 = tmpvar_33;
		vbdueff_15 = int(_maxStep);
		lenfaiejd_14 = 0.0;
		biifejd_12 = bool(0);
		eiieiaced_16 = (hgeiald_18 + tmpvar_33);
		icoiuf_11 = 0;
		s_9 = 0;
		for (int s_9 = 0; s_9 < 100; ) {
		if ((icoiuf_11 >= vbdueff_15)) {
			break;
		};
		float tmpvar_34;
		tmpvar_34 = (1.0/(((_ZBufferParams.x * _tex2Dlod(_CameraDepthTexture, float4(eiieiaced_16.xy, 0, 0.0)).x) + _ZBufferParams.y)));
		float tmpvar_35;
		tmpvar_35 = (1.0/(((_ZBufferParams.x * eiieiaced_16.z) + _ZBufferParams.y)));
		if ((tmpvar_34 < (tmpvar_35 - 1e-06))) {
			uiduefa_10.w = 1.0;
			uiduefa_10.xyz = eiieiaced_16;
			rensfief_13 = uiduefa_10;
			biifejd_12 = bool(1);
			break;
		};
		eiieiaced_16 = (eiieiaced_16 + jjdafhue_17);
		lenfaiejd_14 = (lenfaiejd_14 + 1.0);
		icoiuf_11 = (icoiuf_11 + 1);
		s_9 = (s_9 + 1);
		};
		if ((biifejd_12 == bool(0))) {
		float4 vartfie_36;
		vartfie_36.w = 0.0;
		vartfie_36.xyz = eiieiaced_16;
		rensfief_13 = vartfie_36;
		biifejd_12 = bool(1);
		};
		opahwcte_2 = rensfief_13;
		float tmpvar_37;
		tmpvar_37 = abs((rensfief_13.x - 0.5));
		acccols_8 = float4(0.0, 0.0, 0.0, 0.0);
		if ((_SSRRcomposeMode > 0.0)) {
		float4 tmpvar_38;
		tmpvar_38.w = 0.0;
		tmpvar_38.xyz = SourceColor.xyz;
		acccols_8 = tmpvar_38;
		};
		if ((tmpvar_37 > 0.5)) {
		Result = acccols_8;
		} else {
		float tmpvar_39;
		tmpvar_39 = abs((rensfief_13.y - 0.5));
		if ((tmpvar_39 > 0.5)) {
			Result = acccols_8;
		} else {
			if (((1.0/(((_ZBufferParams.x * rensfief_13.z) + _ZBufferParams.y))) > _maxDepthCull)) {
			Result = float4(0.0, 0.0, 0.0, 0.0);
			} else {
			if ((rensfief_13.z < 0.1)) {
				Result = float4(0.0, 0.0, 0.0, 0.0);
			} else {
				if ((rensfief_13.w == 1.0)) {
				int j_40;
				float4 greyfsd_41;
				float3 poffses_42;
				int i_49_43;
				bool fjekfesa_44;
				float4 alsdmes_45;
				int maxfeis_46;
				float3 refDir_44_47;
				float3 oifejef_48;
				float3 tmpvar_49;
				tmpvar_49 = (rensfief_13.xyz - tmpvar_33);
				float3 tmpvar_50;
				tmpvar_50 = (lkjwejhsdkl_1 * (tmpvar_31 / tmpvar_32));
				refDir_44_47 = tmpvar_50;
				maxfeis_46 = int(_maxFineStep);
				fjekfesa_44 = bool(0);
				poffses_42 = tmpvar_49;
				oifejef_48 = (tmpvar_49 + tmpvar_50);
				i_49_43 = 0;
				j_40 = 0;
				for (int j_40 = 0; j_40 < 20; ) {
					if ((i_49_43 >= maxfeis_46)) {
					break;
					};
					float tmpvar_51;
					tmpvar_51 = (1.0/(((_ZBufferParams.x * _tex2Dlod(_CameraDepthTexture, float4(oifejef_48.xy, 0, 0.0)).x) + _ZBufferParams.y)));
					float tmpvar_52;
					tmpvar_52 = (1.0/(((_ZBufferParams.x * oifejef_48.z) + _ZBufferParams.y)));
					if ((tmpvar_51 < tmpvar_52)) {
					if (((tmpvar_52 - tmpvar_51) < _bias)) {
						greyfsd_41.w = 1.0;
						greyfsd_41.xyz = oifejef_48;
						alsdmes_45 = greyfsd_41;
						fjekfesa_44 = bool(1);
						break;
					};
					float3 tmpvar_53;
					tmpvar_53 = (refDir_44_47 * 0.5);
					refDir_44_47 = tmpvar_53;
					oifejef_48 = (poffses_42 + tmpvar_53);
					} else {
					poffses_42 = oifejef_48;
					oifejef_48 = (oifejef_48 + refDir_44_47);
					};
					i_49_43 = (i_49_43 + 1);
					j_40 = (j_40 + 1);
				}

				if ((fjekfesa_44 == bool(0))) {
					float4 tmpvar_55_54;
					tmpvar_55_54.w = 0.0;
					tmpvar_55_54.xyz = oifejef_48;
					alsdmes_45 = tmpvar_55_54;
					fjekfesa_44 = bool(1);
				}

				opahwcte_2 = alsdmes_45;
				}

				if ((opahwcte_2.w < 0.01))
				{
					Result = acccols_8;
				}
				else
				{
					float4 tmpvar_57_55;
					tmpvar_57_55.xyz = _tex2Dlod(_MainTex, float4(opahwcte_2.xy, 0, 0.0)).xyz;
					tmpvar_57_55.w = (((opahwcte_2.w * (1.0 - (tmpvar_7 / _maxDepthCull))) * (1.0 - pow ((lenfaiejd_14 / float(tmpvar_23)), _fadePower))) * pow (clamp (((dot (normalize(tmpvar_28), normalize(tmpvar_25).xyz) + 1.0) + (_fadePower * 0.1)), 0.0, 1.0), _fadePower));
					Result = tmpvar_57_55;
				}
			}
			}
		}
		}
	}
//*/

	return Result;
}
ENDCG

	}	//	Pass
}	//SubShader

Fallback off
}	// Shader