// CANDELA-SSRR SCREEN SPACE RAYTRACED REFLECTIONS
// Copyright 2014 Livenda

Shader "Hidden/dephNormBlurX" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest 
#pragma target 3.0
#pragma glsl
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform float4 _MainTex_TexelSize;
sampler2D _CameraDepthNormalsTexture;
uniform sampler2D _CameraNormalsTexture;
uniform half4 _Sensitivity; 
uniform float _blurSampleRadius;

//--------------------------------------------
sampler2D _CameraDepthTexture;
uniform float _DistanceBlurRadius = 0.0f;
uniform float _DistanceBlurStart  = 3.0f;
uniform float _GrazeBlurPower	  = 0.0f;
float4x4 _ViewProjectInverse;
//--------------------------------------------

struct v2f {
	float4 pos : POSITION;
	float2 uv  : TEXCOORD0;
};

inline half CheckSame (half2 centerNormal, float centerDepth, half4 sample)
{
		// difference in normals
		// do not bother decoding normals - there's no need here
		half2 diff = abs(centerNormal - sample.xy) * _Sensitivity.y;
		half isSameNormal = (diff.x + diff.y) * _Sensitivity.y < 0.1;
		// difference in depth
		//float sampleDepth = DecodeFloatRG (sample.zw);
		float zdiff = abs(centerDepth-DecodeFloatRG (sample.zw));
		// scale the required threshold by the distance
		half isSameDepth = zdiff * _Sensitivity.x < 0.09 * centerDepth;
		// return:
		// 1 - if normals and depth are similar enough
		// 0 - otherwise
		return isSameNormal * isSameDepth;
}



v2f vert( appdata_img v )
{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv  = v.texcoord.xy;
		return o;
}

	

half4 frag (v2f i) : COLOR
{
	if(tex2Dlod(_CameraDepthTexture, float4(i.uv,0,0)).x > 0.99) discard;

	half4 col = tex2Dlod(_MainTex, float4(i.uv,0,0));
	float p2 = col.w;
	int alphaPointsFound = 1;
	
	half4 centerSample = tex2Dlod(_CameraDepthNormalsTexture, float4(i.uv,0,0));
	int numOfPointsFound = 1;
	float2 sampleUV;
	int s;
	half edgeCheck;
	
	
	
	//----------------------------------------------------------------------
	//WS position
	float  grazingAngle = 0;
	float4 worldnorm 	  = tex2Dlod(_CameraNormalsTexture, float4(i.uv,0,0));
	if(_GrazeBlurPower>0)
	{
    float4 posWS = mul(_ViewProjectInverse, float4(i.uv * 2 - 1, tex2Dlod(_CameraDepthTexture, float4(i.uv,0,0)).x, 1));
 	posWS = posWS/posWS.w;
    grazingAngle   = pow(1-saturate(dot(normalize(_WorldSpaceCameraPos-posWS.xyz),worldnorm.xyz*2.0-1.0)),5);
    }
    //----------------------------------------------------------------------
	
	float blurRad =  _blurSampleRadius*saturate((1-worldnorm.w) + grazingAngle*_GrazeBlurPower) + pow((1-col.w),_DistanceBlurStart)*_DistanceBlurRadius;
	
	float alphaBlurRad = 0.75f;
	
	//NEGATIVE TEXEL SIDE
	for(s=1;s<4;s++)
	{	
		sampleUV		= i.uv + (float2(-_MainTex_TexelSize.x*s,0)*blurRad);
		edgeCheck 		= CheckSame(centerSample.xy, DecodeFloatRG(centerSample.zw), tex2Dlod(_CameraDepthNormalsTexture, float4(sampleUV,0,0)));
		
		if(edgeCheck > 0)//SamplePoint Ok..
		{
			col += tex2Dlod(_MainTex, float4(sampleUV,0,0));
			numOfPointsFound++;
		}
		else
		{
			break;
		}
		
	}
	
	//------------------------------------------------------------------------------
	//ALPHA BLUR
	if(worldnorm.w > 0.7)
	{
		for(s=1;s<4;s++)
		{
		 sampleUV		= i.uv + (float2(-_MainTex_TexelSize.x*s,0)*alphaBlurRad);
		 edgeCheck 		= CheckSame(centerSample.xy, DecodeFloatRG(centerSample.zw), tex2Dlod(_CameraDepthNormalsTexture, float4(sampleUV,0,0)));
		 if(edgeCheck > 0)//SamplePoint Ok..
		 {
		  p2 += tex2Dlod(_MainTex, float4(sampleUV,0,0)).w;
		  alphaPointsFound++;
		 }
		 else
		 {
		  break;
		 }
		
		}
	}
	//------------------------------------------------------------------------------
	
	
	//POSITIVE TEXEL SIDE
	for(s=1;s<4;s++)
	{	
		sampleUV		= i.uv + (float2(+_MainTex_TexelSize.x*s,0)*blurRad);
		edgeCheck 		= CheckSame(centerSample.xy, DecodeFloatRG(centerSample.zw), tex2Dlod(_CameraDepthNormalsTexture, float4(sampleUV,0,0)));
		
		if(edgeCheck > 0)//SamplePoint Ok..
		{
			col += tex2Dlod(_MainTex, float4(sampleUV,0,0));
			numOfPointsFound++;
		}
		else
		{
			break;
		}
		
	}
	
	//------------------------------------------------------------------------------
	//ALPHA BLUR
	if(worldnorm.w > 0.7)
	{
		for(s=1;s<4;s++)
		{
		 sampleUV		= i.uv + (float2(+_MainTex_TexelSize.x*s,0)*alphaBlurRad);
		 edgeCheck 		= CheckSame(centerSample.xy, DecodeFloatRG(centerSample.zw), tex2Dlod(_CameraDepthNormalsTexture, float4(sampleUV,0,0)));
		 if(edgeCheck > 0)//SamplePoint Ok..
		 {
		  p2 += tex2Dlod(_MainTex, float4(sampleUV,0,0)).w;
		  alphaPointsFound++;
		 }
		 else
		 {
		  break;
		 }
		
		}
	}
	//------------------------------------------------------------------------------
	
	col /= numOfPointsFound;
	
	if(worldnorm.w > 0.7)
		col.w = p2/alphaPointsFound;
	
	return col;
	
}
ENDCG
	}
	//==============================================================================================
	//Inverted Roughness
	//==============================================================================================
		Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest 
#pragma target 3.0
#pragma glsl
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform float4 _MainTex_TexelSize;
sampler2D _CameraDepthNormalsTexture;
uniform sampler2D _CameraNormalsTexture;
uniform half4 _Sensitivity; 
uniform float _blurSampleRadius;

//--------------------------------------------
sampler2D _CameraDepthTexture;
uniform float _DistanceBlurRadius = 0.0f;
uniform float _DistanceBlurStart  = 3.0f;
uniform float _GrazeBlurPower	  = 0.0f;
float4x4 _ViewProjectInverse;
//--------------------------------------------

struct v2f {
	float4 pos : POSITION;
	float2 uv  : TEXCOORD0;
};

inline half CheckSame (half2 centerNormal, float centerDepth, half4 sample)
{
		// difference in normals
		// do not bother decoding normals - there's no need here
		half2 diff = abs(centerNormal - sample.xy) * _Sensitivity.y;
		half isSameNormal = (diff.x + diff.y) * _Sensitivity.y < 0.1;
		// difference in depth
		//float sampleDepth = DecodeFloatRG (sample.zw);
		float zdiff = abs(centerDepth-DecodeFloatRG (sample.zw));
		// scale the required threshold by the distance
		half isSameDepth = zdiff * _Sensitivity.x < 0.09 * centerDepth;
		// return:
		// 1 - if normals and depth are similar enough
		// 0 - otherwise
		return isSameNormal * isSameDepth;
}



v2f vert( appdata_img v )
{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv  = v.texcoord.xy;
		return o;
}

	

half4 frag (v2f i) : COLOR
{
	if(tex2Dlod(_CameraDepthTexture, float4(i.uv,0,0)).x > 0.99) discard;

	half4 col = tex2Dlod(_MainTex, float4(i.uv,0,0));
	float p2 = col.w;
	int alphaPointsFound = 1;
	
	half4 centerSample = tex2Dlod(_CameraDepthNormalsTexture, float4(i.uv,0,0));
	int numOfPointsFound = 1;
	float2 sampleUV;
	int s;
	half edgeCheck;
	
	
	
	//----------------------------------------------------------------------
	//WS position
	float  grazingAngle = 0;
	float4 worldnorm 	  = tex2Dlod(_CameraNormalsTexture, float4(i.uv,0,0));
	if(_GrazeBlurPower>0)
	{
    float4 posWS = mul(_ViewProjectInverse, float4(i.uv * 2 - 1, tex2Dlod(_CameraDepthTexture, float4(i.uv,0,0)).x, 1));
 	posWS = posWS/posWS.w;
    grazingAngle   = pow(1-saturate(dot(normalize(_WorldSpaceCameraPos-posWS.xyz),worldnorm.xyz*2.0-1.0)),5);
    }
    //----------------------------------------------------------------------
	
	float blurRad =  _blurSampleRadius*saturate((worldnorm.w) + grazingAngle*_GrazeBlurPower) + pow((1-col.w),_DistanceBlurStart)*_DistanceBlurRadius;
	
	float alphaBlurRad = 0.75f;
	
	//NEGATIVE TEXEL SIDE
	for(s=1;s<4;s++)
	{	
		sampleUV		= i.uv + (float2(-_MainTex_TexelSize.x*s,0)*blurRad);
		edgeCheck 		= CheckSame(centerSample.xy, DecodeFloatRG(centerSample.zw), tex2Dlod(_CameraDepthNormalsTexture, float4(sampleUV,0,0)));
		
		if(edgeCheck > 0)//SamplePoint Ok..
		{
			col += tex2Dlod(_MainTex, float4(sampleUV,0,0));
			numOfPointsFound++;
		}
		else
		{
			break;
		}
		
	}
	
	//------------------------------------------------------------------------------
	//ALPHA BLUR
	if(worldnorm.w < 0.3)
	{
		for(s=1;s<4;s++)
		{
		 sampleUV		= i.uv + (float2(-_MainTex_TexelSize.x*s,0)*alphaBlurRad);
		 edgeCheck 		= CheckSame(centerSample.xy, DecodeFloatRG(centerSample.zw), tex2Dlod(_CameraDepthNormalsTexture, float4(sampleUV,0,0)));
		 if(edgeCheck > 0)//SamplePoint Ok..
		 {
		  p2 += tex2Dlod(_MainTex, float4(sampleUV,0,0)).w;
		  alphaPointsFound++;
		 }
		 else
		 {
		  break;
		 }
		
		}
	}
	//------------------------------------------------------------------------------
	
	
	//POSITIVE TEXEL SIDE
	for(s=1;s<4;s++)
	{	
		sampleUV		= i.uv + (float2(+_MainTex_TexelSize.x*s,0)*blurRad);
		edgeCheck 		= CheckSame(centerSample.xy, DecodeFloatRG(centerSample.zw), tex2Dlod(_CameraDepthNormalsTexture, float4(sampleUV,0,0)));
		
		if(edgeCheck > 0)//SamplePoint Ok..
		{
			col += tex2Dlod(_MainTex, float4(sampleUV,0,0));
			numOfPointsFound++;
		}
		else
		{
			break;
		}
		
	}
	
	//------------------------------------------------------------------------------
	//ALPHA BLUR
	if(worldnorm.w < 0.3)
	{
		for(s=1;s<4;s++)
		{
		 sampleUV		= i.uv + (float2(+_MainTex_TexelSize.x*s,0)*alphaBlurRad);
		 edgeCheck 		= CheckSame(centerSample.xy, DecodeFloatRG(centerSample.zw), tex2Dlod(_CameraDepthNormalsTexture, float4(sampleUV,0,0)));
		 if(edgeCheck > 0)//SamplePoint Ok..
		 {
		  p2 += tex2Dlod(_MainTex, float4(sampleUV,0,0)).w;
		  alphaPointsFound++;
		 }
		 else
		 {
		  break;
		 }
		
		}
	}
	//------------------------------------------------------------------------------
	
	col /= numOfPointsFound;
	
	if(worldnorm.w < 0.3)
		col.w = p2/alphaPointsFound;
	
	return col;
	
}
ENDCG
	}
	//==============================================================================================
	//End Of Inverted Roughness
	//==============================================================================================
}

Fallback off

}