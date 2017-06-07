//
//
#include "Global.hlsl"

cbuffer CB_Filter : register(b10) {
	float	_stride;			// Stride length (1, 2, 4, 8, etc.)
	float	_sigma_Color;		// -1/(sigma_color * sigma_color)
	float	_sigma_Normal;		// -1/(sigma_normal * sigma_normal)
	float	_sigma_Position;	// -1/(sigma_position * sigma_position)
};

Texture2DArray<float4>	_Tex_GBuffer : register(t0);
Texture2D<float4>		_Tex_Scene : register(t1);

// A convolution of 2 B3-spline 1D kernels of weights { 1/16, 1/4, 3/8, 1/4, 1/16 }
static const float	KERNEL[5*5] = {
	  0.00390625,	0.015625,	0.0234375,	0.015625,	0.00390625
	, 0.015625,		0.0625,		0.09375,	0.0625,		0.015625	
	, 0.0234375,	0.09375,	0.140625,	0.09375,	0.0234375	
	, 0.015625,		0.0625,		0.09375,	0.0625,		0.015625	
	, 0.00390625, 0.015625,		0.0234375,	0.015625,	0.00390625
};


// uniform sampler2D colorMap, normalMap, posMap;
// uniform float c_phi, n_phi, p_phi, stepwidth;
// uniform float kernel[25];
// uniform float2 offset[25];
// void main(void) {
//	float4	sumColor = float4(0.0);
//	float2	step = float2(1./512., 1./512.); // resolution
//	float4	cval = texture2D(colorMap, gl_TexCoord[0].st);
//	float4	nval = texture2D(normalMap, gl_TexCoord[0].st);
//	float4	pval = texture2D(posMap, gl_TexCoord[0].st);
//	float	sumWeights = 0.0;
//	for ( int i = 0; i < 25; i++ ) {
//		float2	uv = gl_TexCoord[0].st + offset[i]*step*stepwidth;
//
//		float4	ctmp = texture2D(colorMap, uv);
//		float4	t = cval - ctmp;
//		float	sqDistance = dot(t,t);
//		float	weightColor = exp( -sqDistance / c_phi );
//
//		float4	ntmp = texture2D(normalMap, uv);
//		t = nval - ntmp;
//		sqDistance = dot(t,t) / (stepwidth*stepwidth);
//		float	weightNormal = exp( -sqDistance / n_phi );
//
//		float4	ptmp = texture2D(posMap, uv);
//		t = pval - ptmp;
//		sqDistance = dot(t,t);
//		float	weightPosition = exp( -sqDistance / p_phi );
//
//		float	weight = weightColor * weightNormal * weightPosition;
//		float	influence = weight * kernel[i];
//		sumColor += ctmp * influence;
//		sumWeights += influence;
//	}
//	gl_FragData[0] = sumColor / sumWeights;
//}


float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _resolution;
// 	float	aspectRatio = float( _resolution.x ) / _resolution.y;
// 	float3	csView = float3( TAN_HALF_FOV * aspectRatio * (2.0 * UV.x - 1.0), TAN_HALF_FOV * (1.0 - 2.0 * UV.y), 1.0 );
// 	float	Z2Distance = length( csView );
// 			csView /= Z2Distance;
// 	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;
// 	float3	wsPos = _Camera2World[3].xyz;

	float3	centerColor = _Tex_Scene[uint2(_In.__Position.xy)].xyz;
	float3	centerNormal = _Tex_GBuffer[uint3(_In.__Position.xy,1)].xyz;
	float3	centerPosition = _Tex_GBuffer[uint3(_In.__Position.xy,2)].xyz;

	float	rcpSqStride = 1.0 / (_stride * _stride);	// Normal distance needs to be reduced by square stride

	float3	sumColor = 0.0;
	float	sumWeights = 0.0;
	for ( int Y=-2; Y < 2; Y++ ) {
		for ( int X=-2; X < 2; X++ ) {
			float2	UV = (_In.__Position.xy + _stride * float2( X, Y )) / _resolution;

			float3	neighborColor = _Tex_Scene.SampleLevel( LinearClamp, UV, 0.0 ).xyz;
			float3	neighborNormal = _Tex_GBuffer.SampleLevel( LinearClamp, float3( UV, 1.0 ), 0.0 ).xyz;
			float3	neighborPosition = _Tex_GBuffer.SampleLevel( LinearClamp, float3( UV, 2.0 ), 0.0 ).xyz;

			float3	diffColor = neighborColor - centerColor;
			float	sqDistance = dot( diffColor, diffColor );
			float	weightColor = exp( _sigma_Color * sqDistance );

			float3	diffNormal = neighborNormal - centerNormal;
					sqDistance = dot( diffNormal, diffNormal ) * rcpSqStride;
			float	weightNormal = exp( _sigma_Normal * sqDistance );

			float3	diffPosition = neighborPosition - centerPosition;
					sqDistance = dot( diffPosition, diffPosition );
			float	weightPosition = exp( _sigma_Position * sqDistance );

			float	weight = weightColor * weightNormal * weightPosition;
			float	influence = weight * KERNEL[5*(2+Y)+(2+X)];

			sumColor += influence * neighborColor;
			sumWeights += influence;
		}
	}
	return sumColor / sumWeights;
}