#include "Global.hlsl"
#include "AreaLight.hlsl"

cbuffer CB_Object : register(b3) {
	float4x4	_Local2World;
};

Texture2D< float4 >	_TexAreaLight : register(t2);

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

float4	SampleSATSinglePixel( float2 _UV ) {
	
	float2	PixelIndex = _UV * TEX_SIZE;
	float2	NextPixelIndex = PixelIndex + 1;
	float2	UV2 = NextPixelIndex / TEX_SIZE;

	float4	C00 = _TexAreaLightSAT.Sample( LinearClamp, _UV );
	float4	C01	= _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.xz );
	float4	C10 = _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.zy );
	float4	C11 = _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.xy );

	return C11 - C10 - C01 + C00;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
 	float4	StainedGlass = _TexAreaLight.Sample( LinearClamp, _In.UV );
// 	float4	StainedGlass = 0.0001 * _TexAreaLightSAT.Sample( LinearClamp, _In.UV );
//	float4	StainedGlass = SampleSATSinglePixel( _In.UV );
	StainedGlass *= _AreaLightIntensity;

// 	// Debug UV clipping
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
// 	float4	ClippedUVs = ComputeClipping( lsPosition, lsNormal, Debug );
// 	StainedGlass.xyz = (_In.UV.x < ClippedUVs.x || _In.UV.y < ClippedUVs.y || _In.UV.x > ClippedUVs.z || _In.UV.y > ClippedUVs.w) ? float3( 0.2, 0, 0.2 ) : float3( _In.UV, 0 );


	return float4( StainedGlass.xyz, 1 );
}
