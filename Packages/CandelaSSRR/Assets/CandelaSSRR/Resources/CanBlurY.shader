// CANDELA-SSRR SCREEN SPACE RAYTRACED REFLECTIONS
// Copyright 2014 Livenda

Shader "Hidden/CanBlurY" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	
}

SubShader {
ZTest Always Cull Off ZWrite Off Fog { Mode Off }
	Pass {
		
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest 
#pragma target 3.0
#pragma glsl
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform float4 _MainTex_TexelSize;
sampler2D _CameraNormalsTexture;
sampler2D _CameraDepthTexture;
uniform float _BlurRadius = 1.0f;
uniform float _DistanceBlurRadius = 0.0f;
uniform float _DistanceBlurStart  = 3.0f;
uniform float _GrazeBlurPower	  = 0.0f;
float4x4 _ViewProjectInverse;


struct v2f {
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
};

v2f vert( appdata_img v )
{
	v2f o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	o.uv  = MultiplyUV( UNITY_MATRIX_TEXTURE0, v.texcoord );
	return o;
}


half4 frag (v2f i) : COLOR
{
	if(tex2Dlod(_CameraDepthTexture, float4(i.uv,0,0)).x > 0.99) discard;

	half4 p1 =  tex2Dlod( _MainTex, float4(i.uv,0,0));
	float p2 = p1.w;
	
	//WS position
	float  grazingAngle = 0;
	float4 worldnorm 	  = tex2Dlod(_CameraNormalsTexture, float4(i.uv,0,0));
	if(_GrazeBlurPower>0)
	{
    float4 posWS = mul(_ViewProjectInverse, float4(i.uv * 2 - 1, tex2Dlod(_CameraDepthTexture, float4(i.uv,0,0)).x, 1));
 	posWS = posWS/posWS.w;
    grazingAngle   = pow(1-saturate(dot(normalize(_WorldSpaceCameraPos-posWS.xyz),worldnorm.xyz*2.0-1.0)),5);
    }
    
	float blurRad =  _BlurRadius*saturate((1-worldnorm.w) + grazingAngle*_GrazeBlurPower) + pow((1-p1.w),_DistanceBlurStart)*_DistanceBlurRadius;
	
	if(worldnorm.w > 0.7)
	{
	float alphaBlurRad = 0.75f;
	p2 	 	 += tex2Dlod( _MainTex, float4(i.uv + float2(0,   -_MainTex_TexelSize.y)*alphaBlurRad,0,0)).w
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0, -_MainTex_TexelSize.y*2)*alphaBlurRad,0,0)).w
		      + tex2Dlod( _MainTex, float4(i.uv + float2(0, -_MainTex_TexelSize.y*3)*alphaBlurRad,0,0)).w
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0,   +_MainTex_TexelSize.y)*alphaBlurRad,0,0)).w
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0, +_MainTex_TexelSize.y*2)*alphaBlurRad,0,0)).w
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0, +_MainTex_TexelSize.y*3)*alphaBlurRad,0,0)).w;
		   	  
	p2 = p2*0.1428571428571429; 
	}	
		
	//Y-Blur
	
		
	
		  p1 += tex2Dlod( _MainTex, float4(i.uv + float2(0, -_MainTex_TexelSize.y) * blurRad,0,0))
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0, -_MainTex_TexelSize.y*2)*blurRad,0,0))
		      + tex2Dlod( _MainTex, float4(i.uv + float2(0, -_MainTex_TexelSize.y*3)*blurRad,0,0))
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0, +_MainTex_TexelSize.y) * blurRad,0,0))
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0, +_MainTex_TexelSize.y*2)*blurRad,0,0))
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0, +_MainTex_TexelSize.y*3)*blurRad,0,0));
		   	  
	half4 final =  p1*0.1428571428571429;
	
	if(worldnorm.w > 0.7)
	final.w = p2;
	
	return final;
	
}
ENDCG
	}
	
	//==============================================================================================
	//Inverted Roughness
	//==============================================================================================
		Pass {
		
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest 
#pragma target 3.0
#pragma glsl
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform float4 _MainTex_TexelSize;
sampler2D _CameraNormalsTexture;
sampler2D _CameraDepthTexture;
uniform float _BlurRadius = 1.0f;
uniform float _DistanceBlurRadius = 0.0f;
uniform float _DistanceBlurStart  = 3.0f;
uniform float _GrazeBlurPower	  = 0.0f;
float4x4 _ViewProjectInverse;


struct v2f {
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
};

v2f vert( appdata_img v )
{
	v2f o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	o.uv  = MultiplyUV( UNITY_MATRIX_TEXTURE0, v.texcoord );
	return o;
}


half4 frag (v2f i) : COLOR
{
	if(tex2Dlod(_CameraDepthTexture, float4(i.uv,0,0)).x > 0.99) discard;

	half4 p1 =  tex2Dlod( _MainTex, float4(i.uv,0,0));
	float p2 = p1.w;
	
	//WS position
	float  grazingAngle = 0;
	float4 worldnorm 	  = tex2Dlod(_CameraNormalsTexture, float4(i.uv,0,0));
	if(_GrazeBlurPower>0)
	{
    float4 posWS = mul(_ViewProjectInverse, float4(i.uv * 2 - 1, tex2Dlod(_CameraDepthTexture, float4(i.uv,0,0)).x, 1));
 	posWS = posWS/posWS.w;
    grazingAngle   = pow(1-saturate(dot(normalize(_WorldSpaceCameraPos-posWS.xyz),worldnorm.xyz*2.0-1.0)),5);
    }
    
	float blurRad =  _BlurRadius*saturate((worldnorm.w) + grazingAngle*_GrazeBlurPower) + pow((1-p1.w),_DistanceBlurStart)*_DistanceBlurRadius;
	
	if(worldnorm.w < 0.3)
	{
	float alphaBlurRad = 0.75f;
	p2 	 	 += tex2Dlod( _MainTex, float4(i.uv + float2(0,   -_MainTex_TexelSize.y)*alphaBlurRad,0,0)).w
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0, -_MainTex_TexelSize.y*2)*alphaBlurRad,0,0)).w
		      + tex2Dlod( _MainTex, float4(i.uv + float2(0, -_MainTex_TexelSize.y*3)*alphaBlurRad,0,0)).w
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0,   +_MainTex_TexelSize.y)*alphaBlurRad,0,0)).w
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0, +_MainTex_TexelSize.y*2)*alphaBlurRad,0,0)).w
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0, +_MainTex_TexelSize.y*3)*alphaBlurRad,0,0)).w;
		   	  
	p2 = p2*0.1428571428571429; 
	}	
		
	//Y-Blur
	
		
	
		  p1 += tex2Dlod( _MainTex, float4(i.uv + float2(0, -_MainTex_TexelSize.y) * blurRad,0,0))
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0, -_MainTex_TexelSize.y*2)*blurRad,0,0))
		      + tex2Dlod( _MainTex, float4(i.uv + float2(0, -_MainTex_TexelSize.y*3)*blurRad,0,0))
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0, +_MainTex_TexelSize.y) * blurRad,0,0))
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0, +_MainTex_TexelSize.y*2)*blurRad,0,0))
		   	  + tex2Dlod( _MainTex, float4(i.uv + float2(0, +_MainTex_TexelSize.y*3)*blurRad,0,0));
		   	  
	half4 final =  p1*0.1428571428571429;
	
	if(worldnorm.w < 0.3)
	final.w = p2;
	
	return final;
	
}
ENDCG
	}
	//==============================================================================================
	//End Of Inverted Roughness
	//==============================================================================================
}

Fallback off

}