//////////////////////////////////////////////////////////////////////////
// This shader displays the objects in our fat G-Buffer
//
#include "Inc/Global.fx"

cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
};

cbuffer	cbPrimitive	: register( b11 )
{
	int4		_MatIDs;		// 4 material IDs for each of the 4 layers
	float3		_Thickness;		// 3 thicknesses for the top 3 layers
	// TODO: Add tiling + offset for each layer
};
Texture2DArray	_TexMaterial	: register(t10);	// 4 Slices of diffuse+blend masks + normal map + specular map = 6 textures per primitive
// TODO: Don't use a texture array => Separate textures are cooler so we can easily send 1x1 blank textures for unused layers

// struct	PomParameters
// {
// };
// TextureBuffer<PomParameters>	_TexMaterials : register(t11);

struct	VS_IN
{
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float2	UV			: TEXCOORD0;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
//	float3	BiTangent	: BITANGENT;
	float2	UV			: TEXCOORD0;
};

struct	PS_OUT
{
	float4	NormalTangent		: SV_TARGET0;	// XY=Normal  ZW=TangentXY
	float4	DiffuseAlbedo		: SV_TARGET1;	// XYZ=Diffuse Albedo W=TangentZ
	float4	SpecularAlbedo		: SV_TARGET2;	// XYZ=Specular Albedo
	int4	WeightMatIDs0		: SV_TARGET3;	// 4 couples of [Weight,MatID] each in [0,255]
};

PS_IN	VS( VS_IN _In )
{
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );
	float3	WorldNormal = mul( float4( _In.Normal, 0.0 ), _Local2World ).xyz;
	float3	WorldTangent = mul( float4( _In.Tangent, 0.0 ), _Local2World ).xyz;
// 	float3	WorldBiTangent = cross( WorldNormal, WorldTangent );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Position = WorldPosition.xyz;
	Out.Normal = WorldNormal;
	Out.Tangent = WorldTangent;
// 	Out.BiTangent = WorldBiTangent;
	Out.UV = _In.UV;

	return Out;
}

// Compute rotation matrix to transform source normal into target normal
// (routine from Thomas Moller) (which is actually the same as converting from a quaternion to a matrix)
//
float3x3	ComputeRotation( float3 _NewNormalTS )
{
	float3	BaseNormalTS = float3( 0, 0, 1 );
	float	e = dot( BaseNormalTS, _NewNormalTS );
	vec3	Ortho = cross( BaseNormalTS, _NewNormalTS );	// Rotation axis * sin( angle )
	
	float	h = 1.0 / (1.0 + e);      // Optimization by Gottfried Chen
	
	float3x3	Transform;
	Transform[0].x = e + h * Ortho.x * Ortho.x;
	Transform[0].y = h * Ortho.x * Ortho.y - Ortho.z;
	Transform[0].z = h * Ortho.x * Ortho.z + Ortho.y;

	Transform[1].x = h * Ortho.x * Ortho.y + Ortho.z;
	Transform[1].y = e + h * Ortho.y * Ortho.y;
	Transform[1].z = h * Ortho.y * Ortho.z - Ortho.x;

	Transform[2].x = h * Ortho.x * Ortho.z - Ortho.y;
	Transform[2].y = h * Ortho.y * Ortho.z + Ortho.x;
	Transform[2].z = e + h * Ortho.z * Ortho.z;

	return Transform;
}

int	WriteWeightMatID( float _Weight, int _MatID )
{
	int	Weight = int( _Weight * 255.0 );
	int	Concat = (_MatID << 8) | Weight;	// [0,65535]
	return Concat;
}

PS_OUT	PS( PS_IN _In )
{
	// Compute view vector
	float3	View = normalize( _In.Position - _Camera2World[3].xyz );	// Pointing toward the surface

	// Compute tangent space
	float3	WorldNormal = normalize( _In.Normal );
	float3	WorldTangent = normalize( _In.Tangent );
	float3	WorldBiTangent = normalize( cross( WorldNormal, WorldTangent ) );

	// Compute change in UVs for change in world space
	float3	ViewTS = float3( dot( View, WorldTangent ), dot( View, WorldBiTangent ), dot( View, WorldNormal ) );
//	float2	dUV = ViewTS.xy / ViewTS.z;	// The amount of 
// 	float2	dUVx = float2( ddx( UV.x ), ddy( UV.x ) );
// 	float2	dUVy = float2( ddx( UV.y ), ddy( UV.y ) );
	float2	dUV = 0.0;	// Let's do that later... It's complicated and unnecessary

	// Sample layers with parallax based on thickness of each layer
	float2	UV = _In.UV;
	float4	TexSpecular = _TexObject.Sample( LinearWrap, UV, 5 );							// Specular is sampled first as it's tied to the top layer
	float4	TexLayer3 = _TexObject.Sample( LinearWrap, UV, 3 );	UV += dUV * _Thickness.z;	// Layer 3 is sampled at entry point (top layer)
	float4	TexLayer2 = _TexObject.Sample( LinearWrap, UV, 2 );	UV += dUV * _Thickness.y;	// Layer 2
	float4	TexLayer1 = _TexObject.Sample( LinearWrap, UV, 1 );	UV += dUV * _Thickness.x;	// Layer 1
	float4	TexLayer0 = _TexObject.Sample( LinearWrap, UV, 0 );								// Layer 0
	float4	TexNormal = _TexObject.Sample( LinearWrap, UV, 4 );								// Normal Map is always assigned to bottom layer

	// Transform tangent space & get normal+tangent into camera space
	float3		NormalMap = 2.0 * TexNormal.xyz - 1.0;
	float3x3	Rotation = ComputeRotation( NormalMap );

	float3	NewNormal = mul( WorldNormal, Rotation );
	float3	NewTangent = mul( WorldTangent, Rotation );

	float2	CameraNormal = float2( dot( NewNormal, _World2Camera[0].xyz ), dot( NewNormal, _World2Camera[1].xyz ) );	// We only need XY
	float3	CameraTangent = float3( dot( NewTangent, _World2Camera[0].xyz ), dot( NewTangent, _World2Camera[1].xyz ), dot( NewTangent, _World2Camera[2].xyz ) );

	//////////////////////////////////////////////////////////////////////
	// Apply Pom model
	float3	DiffuseAlbedo = lerp( lerp( lerp( TexLayer0.xyz, TexLayer1.xyz, TexLayer1.w ), TexLayer2.xyz, TexLayer2.w ), TexLayer3.xyz, TexLayer3.w );
	float3	DiffuseWeights = 1.0 - float3( TexLayer1.w, TexLayer2.w, TexLayer3.w );	// The weight of light passing through each layer, individually
			DiffuseWeights.y *= DiffuseWeights.z;									// Cumulated weight of light reaching layer 1
			DiffuseWeights.x *= DiffuseWeights.y;									// Cumulated weight of light reaching layer 0

	// Write final result
	PS_OUT	Out;
	Out.NormalTangent = float4( CameraNormal, CameraTangent.xy );	// XY=Normal  ZW=TangentXY
	Out.DiffuseAlbedo = float4( DiffuseAlbedo, CameraTangent.z );	// XYZ=Diffuse Albedo W=TangentZ
	Out.SpecularAlbedo = TexSpecular;								// XYZ=Specular Albedo
	Out.WeightMatIDs0 = float4(										// 4 couples of [Weight,MatID] each in [0,255]
								WriteWeightMatID( DiffuseWeight.x, _MatIDs.x ),
								WriteWeightMatID( DiffuseWeight.y, _MatIDs.y ),
								WriteWeightMatID( DiffuseWeight.z, _MatIDs.z ),
								WriteWeightMatID( 1.0, _MatIDs.w )
							);

	return Out;
}
