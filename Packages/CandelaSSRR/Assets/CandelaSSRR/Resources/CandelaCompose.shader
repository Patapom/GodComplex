// CANDELA-SSRR SCREEN SPACE RAYTRACED REFLECTIONS
// Copyright 2014 Livenda

Shader "Hidden/CandelaCompose" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	
}

SubShader {
	ZTest Always Cull Off ZWrite Off Fog { Mode Off }
	Pass {

CGPROGRAM
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#pragma glsl
#include "UnityCG.cginc"
                
uniform sampler2D _MainTex;
uniform sampler2D _SSRtexture;
uniform float4 _ScreenFadeControls;
uniform float _UseEdgeTexture;
uniform float _IsInForwardRender;
uniform sampler2D _EdgeFadeTexture;
uniform sampler2D _depthTexCustom;
uniform float _SSRRcomposeMode;
uniform float _reflectionMultiply;
uniform sampler2D _CameraDepthTexture;
struct v2f {
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
};

v2f vert( appdata_img v )
{
	v2f o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	o.uv = MultiplyUV( UNITY_MATRIX_TEXTURE0, v.texcoord );

	return o;
}


half4 frag (v2f i) : COLOR
{
	float4 original    = tex2D(_MainTex, i.uv);
	float4 reflections = tex2D(_SSRtexture, i.uv);
	
	float screenFade  = 1.0f;
	
	if(_UseEdgeTexture > 0)
	screenFade = tex2D(_EdgeFadeTexture, i.uv).x;
	else
	screenFade = 1-saturate((pow(length(((i.uv * 2.0) - 1.0)),_ScreenFadeControls.y)-_ScreenFadeControls.z)*_ScreenFadeControls.w);
	
	float4 col = float4(0,0,0,0);
	
	if(_SSRRcomposeMode > 0)//Physically Accurate Mode
	{
	col = reflections*reflections.w*screenFade*saturate(original.w)*_reflectionMultiply+original*(1-reflections.w*screenFade*saturate(original.w)*_reflectionMultiply);
	}
	else
	{//Additive Mode
	if(_IsInForwardRender > 0 && (tex2Dlod(_depthTexCustom, float4(i.uv,0,0)).x < 0.7)) reflections.w = 0;
	else if(_IsInForwardRender < 1 && tex2Dlod(_CameraDepthTexture, float4(i.uv,0,0)).x < 0.7) reflections.w = 0;
	col = reflections*reflections.w*screenFade*saturate(original.w)*_reflectionMultiply+original;
	}
	
	//Debug Display Screen Fade
	if(_ScreenFadeControls.x > 0)
	col = screenFade;
	
	
	return col;
	
}
ENDCG
	}
}

Fallback off

}