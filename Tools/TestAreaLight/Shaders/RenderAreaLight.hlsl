#include "Global.hlsl"
#include "AreaLight3.hlsl"
#include "ParaboloidShadowMap.hlsl"

cbuffer CB_Object : register(b4) {
	float4x4	_Local2World;
	float4x4	_World2Local;
	float3		_DiffuseAlbedo;
	float		_Gloss;
	float3		_SpecularTint;
	float		_Metal;
	uint		_UseTexture;
	uint		_FalseColors;
	float		_FalseColorsMaxRange;
};

struct VS_IN {
	float3	Position : POSITION;
	float3	Normal : NORMAL;
	float3	Tangent : TANGENT;
	float3	BiTangent : BITANGENT;
	float2	UV : TEXCOORD0;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float2	UV : TEXCOORDS0;
};

PS_IN	VS( VS_IN _In ) {

	PS_IN	Out;

	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.UV = _In.UV;

	return Out;
}

#ifdef USE_SAT
float4	SampleSATSinglePixel( float2 _UV ) {
	
	float2	PixelIndex = _UV * _AreaLightTexDimensions.xy;
	float2	NextPixelIndex = PixelIndex + 1;
	float2	UV2 = NextPixelIndex * _AreaLightTexDimensions.zw;

	float3	dUV = float3( _AreaLightTexDimensions.zw, 0.0 );
	float4	C00 = _TexAreaLightSAT.Sample( LinearClamp, _UV );
	float4	C01	= _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.xz );
	float4	C10 = _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.zy );
	float4	C11 = _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.xy );

	return C11 - C10 - C01 + C00;
}
#endif

float4	PS( PS_IN _In ) : SV_TARGET0 {

// float2	BRDF = float4( _TexBRDFIntegral.Sample( LinearClamp, _In.UV ), 0, 1 );
// return float4( BRDF, 0, 1 );

 	float3	StainedGlass = _UseTexture ? _TexAreaLight.SampleLevel( LinearClamp, _In.UV, 0*_AreaLightDiffusion * 7 ).xyz : 1.0;
// 	float4	StainedGlass = _TexAreaLight.SampleLevel( LinearClamp, _In.UV, 9.0 * abs( fmod( 0.25 * iGlobalTime, 2.0 ) - 1.0 ) );
// 	float4	StainedGlass = 0.0001 * _TexAreaLightSAT.Sample( LinearClamp, _In.UV );
#ifdef USE_SAT
	float3	StainedGlass = SampleSATSinglePixel( _In.UV ).xyz;
#else
#endif

// 	StainedGlass = _TexAreaLightMIP.SampleLevel( LinearClamp, float3( _In.UV, 0.5 * (1.0 + sin( iGlobalTime )) ), 0.0 );

	StainedGlass *= _AreaLightIntensity;

// Debug shadow map
//StainedGlass = 1.0 * _TexShadowMap.Sample( LinearClamp, _In.UV );
// StainedGlass = float4( _TexShadowMap.SampleLevel( LinearClamp, _In.UV, 0.0 ).x - _TexShadowSmoothie.Sample( LinearClamp, _In.UV ), 0, 0 ).y;
//StainedGlass = float4( _TexShadowSmoothie.Sample( LinearClamp, _In.UV ), 0, 0 ).x;
//StainedGlass = 20.0 * float4( _TexShadowSmoothie.Sample( LinearClamp, _In.UV ), 0, 0 ).y;


	if ( _FalseColors )
		StainedGlass = _TexFalseColors.SampleLevel( LinearClamp, float2( dot( LUMINANCE, StainedGlass ) / _FalseColorsMaxRange, 0.5 ), 0.0 ).xyz;



// 	// Debug UV clipping
// 	if ( all( abs( 2.0 * _In.UV - 1.0 ) > 0.9 ) )
// 		return float4( _In.UV, 0, 0 );
// 
// 	float3	wsPosition = float3( 0, 0, 0 );
// 	float3	wsNormal = float3( 0, 1, 0 );
// 
// 	float3	wsCenter2Position = wsPosition - _AreaLightT;
// 	float3	lsPosition = float3(	dot( wsCenter2Position, _AreaLightX ),	// Transform world position in local area light space
// 									dot( wsCenter2Position, _AreaLightY ),
// 									dot( wsCenter2Position, _AreaLightZ ) );
// 	lsPosition.xy /= float2( _AreaLightScaleX, _AreaLightScaleY );			// Account for scale
// 	float3	lsNormal = float3(	dot( wsNormal, _AreaLightX ),				// Transform world normal in local area light space
// 								dot( wsNormal, _AreaLightY ),
// 								dot( wsNormal, _AreaLightZ ) );
// 
// 	float4	Debug;
// 	float4	ClippedUVs = ComputeAreaLightClipping( lsPosition, lsNormal );
// 	StainedGlass.xyz = (_In.UV.x < ClippedUVs.x || _In.UV.y < ClippedUVs.y || _In.UV.x > ClippedUVs.z || _In.UV.y > ClippedUVs.w) ? float3( 0.2, 0, 0.2 ) : float3( _In.UV, 0 );

//return Debug;
//return float4( ClippedUVs.zw, 0, 0 );


//	return float4( pow( StainedGlass, 1/2.2 ), 1 );
	return float4( StainedGlass, 1 );
}
