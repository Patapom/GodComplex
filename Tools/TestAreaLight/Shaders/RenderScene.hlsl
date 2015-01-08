#include "Global.hlsl"

cbuffer CB_Object : register(b2) {
	float4x4	_Local2World;
	float4x4	_World2Local;
};

cbuffer CB_Material : register(b3) {
	float4x4	_AreaLight2World;
	float4x4	_World2AreaLight;
	float3		_ProjectionDirection;
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
	float2	UV : TEXCOORDS0;
};

PS_IN	VS( VS_IN _In ) {

	PS_IN	Out;

	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );
	Out.__Position = mul( WorldPosition, _World2Proj );
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
bool	ComputeSolidAngleFromPoint( float3 _wsPosition, out float2 _UV0, out float2 _UV1, out float _SolidAngle ) {

	float3	lsPosition = mul( float4( _wsPosition, 1.0 ), _World2AreaLight ).xyz;		// Transform world position in local area light space
	if ( lsPosition.z <= 0.0 ) {
		// Position is behind area light...
		_UV0 = _UV1 = 0.0;
		_SolidAngle = 0.0;
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
	float2	UV[2];
	for ( uint Corner=0; Corner < 2; Corner++ ) {
		float3	lsVirtualPos = lsPortal[Corner] - _ProjectionDirection;	// This is the position of the corner of the virtual source
		float3	Dir = lsVirtualPos - lsPosition;						// This is the pointing direction, originating from the source _wsPosition
		float	t = -lsPosition.z / Dir.z;								// This is the distance at which we hit the physical portal's plane
		float3	lsHitPos = lsPosition + t * Dir;						// This is the position on the portal's plane
		UV[Corner] = 0.5 * (1.0 + lsHitPos);							// Retrieve the UVs
	}

	// Make sure the UVs are at least separated by a single texel before clamping
	_UV0 = UV[0];
	_UV1 = max( _UV0 + dUV.xy, UV[1] );

	// Clamp to [0,1]
	_UV0 = saturate( _UV0 );
	_UV1 = saturate( _UV1 );



	return true;
}


float4	PS( PS_IN _In ) : SV_TARGET0 {
// 	float4	StainedGlass = _TexAreaLight.Sample( LinearClamp, _In.UV );
// 	float4	StainedGlass = 0.0001 * _TexAreaLightSAT.Sample( LinearClamp, _In.UV );
	float4	StainedGlass = SampleSATSinglePixel( _In.UV );

	return float4( StainedGlass.xyz, 1 );
}
