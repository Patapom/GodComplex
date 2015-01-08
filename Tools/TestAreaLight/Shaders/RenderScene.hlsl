#include "Global.hlsl"

cbuffer CB_Object : register(b2) {
	float4x4	_Local2World;
	float4x4	_World2Local;
};

cbuffer CB_Material : register(b3) {
	float4x4	_AreaLight2World;
	float4x4	_World2AreaLight;
	float3		_ProjectionDirection;
	float		_Area;
	float		_LightIntensity;
	float		_Gloss;
	float		_Metal;
};

Texture2D< float4 >	_TexAreaLightSAT : register(t0);

struct VS_IN {
	float3	Position : POSITION;
	float3	Normal : NORMAL;
	float3	Tangent : TANGENT;
	float3	BiTangent : BITANGENT;
	float2	UV : TEXCOORD0;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	Position : POSITION;
	float3	Normal : NORMAL;
	float2	UV : TEXCOORDS0;
};

PS_IN	VS( VS_IN _In ) {

	PS_IN	Out;

	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Position = WorldPosition.xyz;
	Out.Normal = mul( float4( _In.Normal, 0.0 ), _Local2World ).xyz;
	Out.UV = _In.UV;

	return Out;
}

static const uint2	TEX_SIZE = uint2( 465, 626 );
static const float3	dUV = float3( 1.0 / TEX_SIZE, 0.0 );

float4	SampleSATSinglePixel( float2 _UV ) {

	float2	PixelIndex = _UV * TEX_SIZE;
	float2	NextPixelIndex = PixelIndex + 1;
	float2	UV2 = NextPixelIndex / TEX_SIZE;

	float4	C00 = _TexAreaLightSAT.Sample( LinearClamp, _UV );
	float4	C01 = _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.xz );
	float4	C10 = _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.zy );
	float4	C11 = _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.xy );

	return C11 - C10 - C01 + C00;
}

// Computes the 2 Uv and the solid angle perceived from a point in world space
bool	ComputeSolidAngleFromPoint( float3 _wsPosition, float3 _wsNormal, out float2 _UV0, out float2 _UV1, out float _ProjectedSolidAngle ) {

	float3	lsPosition = mul( float4( _wsPosition, 1.0 ), _World2AreaLight ).xyz;		// Transform world position in local area light space
	if ( lsPosition.z <= 0.0 ) {
		// Position is behind area light...
		_UV0 = _UV1 = 0.0;
		_ProjectedSolidAngle = 0.0;
		return false;
	}

	// In local area light space, the position is in front of a canonical square:
	//
	//	(-1,+1)					(+1,+1)
	//			o-------------o
	//			|             |
	//			|             |
	//			|             |
	//			|      o      |
	//			|             |
	//			|             |
	//			|             |
	//			o-------------o
	//	(-1,-1)					(+1,-1)
	//
	//
	float3	lsPortal[2] = {
		float3( -1, +1, 0 ),		// Top left
		float3( +1, -1, 0 ),		// Bottom right
	};

	// Compute the UV coordinates of the intersection of the frustum with the portal's plane
	float2	lsIntersection[2] = { 0.0.xx, 0.0.xx };
	for ( uint Corner=0; Corner < 2; Corner++ ) {
		float3	lsVirtualPos = lsPortal[Corner] - _ProjectionDirection;	// This is the position of the corner of the virtual source
		float3	Dir = lsVirtualPos - lsPosition;						// This is the pointing direction, originating from the source _wsPosition
		float	t = -lsPosition.z / Dir.z;								// This is the distance at which we hit the physical portal's plane
		lsIntersection[Corner] = (lsPosition + t * Dir).xy;				// This is the position on the portal's plane
	}

	// Retrieve the UVs
	_UV0 = 0.5 * (1.0 + lsIntersection[0]);
	_UV1 = max( _UV0 + dUV.xy, 0.5 * (1.0 + lsIntersection[1]) );		// Make sure the UVs are at least separated by a single texel before clamping

	// Clamp to [0,1]
	_UV0 = saturate( _UV0 );
	_UV1 = saturate( _UV1 );

	// Compute the solid angle
	float2	DeltaUV = _UV1 - _UV0;
	float	UVArea = DeltaUV.x * DeltaUV.y;	// This is the perceived area in UV space
	float	wsArea = UVArea * _Area;		// This is the perceived area in world space

	float2	lsCenter = 0.5 * (lsIntersection[0] + lsIntersection[1]);
	float3	wsCenter = _AreaLight2World[3].xyz + lsCenter.x * _AreaLight2World[0].xyz + lsCenter.y * _AreaLight2World[1].xyz;	// World space center
	float3	wsPos2Center = normalize( wsCenter - _wsPosition );

	float	SolidAngle = wsArea * -dot( wsPos2Center, _AreaLight2World[2].xyz );	// dWi = Area * cos( theta )

	// Now, we can compute the projected solid angle by dotting with the normal
	_ProjectedSolidAngle = saturate( dot( _wsNormal, wsPos2Center ) ) * SolidAngle;	// (N.Wi) * dWi

	return true;
}

float4	SampleSAT( float2 _UV0, float2 _UV1 ) {
	// Sample SAT
	float4	C00 = _TexAreaLightSAT.Sample( LinearClamp, _UV0 );
	float4	C01 = _TexAreaLightSAT.Sample( LinearClamp, float2( _UV1.x, _UV0.y ) );
	float4	C10 = _TexAreaLightSAT.Sample( LinearClamp, float2( _UV0.x, _UV1.y ) );
	float4	C11 = _TexAreaLightSAT.Sample( LinearClamp, _UV1 );
	float4	C = C11 - C10 - C01 + C00;

	// Compute normalization factor
	float2	DeltaUV = _UV1 - _UV0;
	float	PixelsCount = (DeltaUV.x * TEX_SIZE.x) * (DeltaUV.y * TEX_SIZE.y);

	return C / (1e-6 + PixelsCount);
}


float4	PS( PS_IN _In ) : SV_TARGET0 {
// 	float4	StainedGlass = _TexAreaLight.Sample( LinearClamp, _In.UV );
// 	float4	StainedGlass = 0.0001 * _TexAreaLightSAT.Sample( LinearClamp, _In.UV );
//	float4	StainedGlass = SampleSATSinglePixel( _In.UV );

	float3	wsPosition = _In.Position;
	float3	wsNormal = normalize( _In.Normal );

	const float3	RhoD = 0.5;	// 50% diffuse albedo

	// Compute diffuse lighting
	float3	Ld = 0.0;
	float2	UV0, UV1;
	float	SolidAngle;
	if ( ComputeSolidAngleFromPoint( wsPosition, wsNormal, UV0, UV1, SolidAngle ) ) {
		float3	Irradiance = _LightIntensity * SampleSAT( UV0, UV1 ).xyz;
		Ld = RhoD / PI * Irradiance * SolidAngle;
	}

	return float4( Ld, 1 );
}
