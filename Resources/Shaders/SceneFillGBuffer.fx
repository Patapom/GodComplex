//////////////////////////////////////////////////////////////////////////
// This shader displays the objects in our fat G-Buffer
//
//	TODO: Don't use a texture array => Separate textures are cooler so we can easily send 1x1 blank textures for unused layers
//		  Also useful to have several formats (like diffuse in sRGB and stuff)
//
//	TODO: Use PTMs !!!
//
#include "Inc/Global.fx"

cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
};

cbuffer	cbPrimitive	: register( b11 )
{
	uint4		_MatIDs;		// 4 material IDs for each of the 4 layers
	float4		_Thickness;		// 3 thicknesses for the top 3 layers (X=layer#1, Y=layer#2, Z=layer#3)
	float3		_Extinction;	// 3 extinction coefficients for the top 3 layers (X=layer#1, Y=layer#2, Z=layer#3)
	float3		_IOR;			// 3 indices of refraction for the top 3 layers (X=layer#1, Y=layer#2, Z=layer#3)
	float3		_Frosting;		// 3 "frosting coefficients" for the top 3 layers (X=layer#1, Y=layer#2, Z=layer#3)
	float4		_NoDiffuse;		// 4 no diffuse indices telling if the diffuse texture should be used to tint the specular instead
	// TODO: Add diffusion (i.e. mip bias) for each transmissive layer
	// TODO: Add tiling + offset for each layer
};

Texture2DArray	_TexMaterial	: register(t10);	// 4 Slices of diffuse+blend masks + specular map + normal map = 6 textures per primitive

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
	uint4	WeightMatIDs0		: SV_TARGET3;	// 4 couples of [Weight,MatID] each in [0,255]
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
	float3	Ortho = cross( BaseNormalTS, _NewNormalTS );	// Rotation axis * sin( angle )
	
	float	h = 1.0 / (1.0 + e);      // Optimization by Gottfried Chen
	
	float3x3	Transform;
	Transform[0].x = e + h * Ortho.x * Ortho.x;
	Transform[1].x = h * Ortho.x * Ortho.y - Ortho.z;
	Transform[2].x = h * Ortho.x * Ortho.z + Ortho.y;

	Transform[0].y = h * Ortho.x * Ortho.y + Ortho.z;
	Transform[1].y = e + h * Ortho.y * Ortho.y;
	Transform[2].y = h * Ortho.y * Ortho.z - Ortho.x;

	Transform[0].z = h * Ortho.x * Ortho.z - Ortho.y;
	Transform[1].z = h * Ortho.y * Ortho.z + Ortho.x;
	Transform[2].z = e + h * Ortho.z * Ortho.z;

	return Transform;
}

uint	WriteWeightMatID( float _Weight, uint _MatID )
{
	uint	Weight = int( _Weight * 255.0 );
	uint	Concat = (_MatID << 8) | Weight;	// [0,65535]
	return Concat;
}

PS_OUT	PS( PS_IN _In )
{
	// Compute view vector
	float3	View = normalize( _Camera2World[3].xyz - _In.Position );	// Pointing toward the camera

	// Compute tangent space
	float3	WorldNormal = normalize( _In.Normal );
	float3	WorldTangent = normalize( _In.Tangent );
	float3	WorldBiTangent = normalize( cross( WorldNormal, WorldTangent ) );

	// Compute change in UVs for change in world space
	float3	ViewTS = float3( dot( View, WorldTangent ), dot( View, WorldBiTangent ), dot( View, WorldNormal ) );
	float	DistanceFactor = 1.0 / max( 1e-3, ViewTS.z );	// Depends on view angle with surface

//	float2	dUV = ViewTS.xy / ViewTS.z;	// The amount of 
// 	float2	dUVx = float2( ddx( UV.x ), ddy( UV.x ) );
// 	float2	dUVy = float2( ddx( UV.y ), ddy( UV.y ) );
	float2	dUV = 0.0;	// Let's do that later... It's complicated and unnecessary

	// TODO: Account for refraction => 1 dUV per layer

	// Sample layers with parallax based on thickness of each layer
	float2	UV = _In.UV;
	float4	TexSpecular = _TexMaterial.Sample( LinearWrap,	float3( UV, 4 ) );								// Specular is sampled first as it's tied to the top layer
	float4	TexLayer3 = _TexMaterial.Sample( LinearWrap,	float3( UV, 3 ) );	UV += dUV * _Thickness.z;	// Layer 3 is sampled at entry point (top layer)
	float4	TexLayer2 = _TexMaterial.Sample( LinearWrap,	float3( UV, 2 ) );	UV += dUV * _Thickness.y;	// Layer 2
	float4	TexLayer1 = _TexMaterial.Sample( LinearWrap,	float3( UV, 1 ) );	UV += dUV * _Thickness.x;	// Layer 1
	float4	TexLayer0 = _TexMaterial.Sample( LinearWrap,	float3( UV, 0 ) );								// Layer 0
	float4	TexNormal = _TexMaterial.Sample( LinearWrap,	float3( UV, 5 ) );								// Normal Map is always assigned to bottom layer

	// Transform tangent space & get normal+tangent into camera space
	float3		NormalMap = normalize( 2.0 * TexNormal.xyz - 1.0 );
	float3x3	Rotation = ComputeRotation( NormalMap );

// 	WorldNormal = mul( WorldNormal, Rotation );
// 	WorldTangent = mul( WorldTangent, Rotation );

	float3	CameraNormal = float3( dot( WorldNormal, _Camera2World[0].xyz ), dot( WorldNormal, _Camera2World[1].xyz ), -dot( WorldNormal, _Camera2World[2].xyz ) );
	float3	CameraTangent = float3( dot( WorldTangent, _Camera2World[0].xyz ), dot( WorldTangent, _Camera2World[1].xyz ), dot( WorldTangent, _Camera2World[2].xyz ) );
			CameraTangent = normalize( CameraTangent );

	// Pack for storage
	CameraTangent = 0.5 * (1.0 + CameraTangent);

	// Stereographic projection of normal (from http://aras-p.info/texts/CompactNormalStorage.html#method07stereo)
	// See also http://en.wikipedia.org/wiki/Stereographic_projection
	CameraNormal.xy /= 1.0 + CameraNormal.z;	// Gives quite a large value for negative normals
	CameraNormal /= 1.7777;						// So we simply divide by a number larger than 1 to account for "some parts" of the negative normals but we hope there won't be too much negative ones
	CameraNormal = 0.5 * (1.0 + CameraNormal);


	//////////////////////////////////////////////////////////////////////
	// Apply Pom model

	// Compute perceived diffuse
	float3	LayerDistances = DistanceFactor * _Thickness.yzw;
	float3	LayerExtinctions = exp( _Extinction * LayerDistances );				// The extinction of light going through some distance for each layer
			LayerExtinctions.x *= TexLayer1.w;									// Apply masking of each layer as well
			LayerExtinctions.y *= TexLayer2.w;
			LayerExtinctions.z *= TexLayer3.w;

	float3	Layer0 = TexLayer0.xyz;	// Full base layer
	float3	Layer1 = lerp( Layer0, TexLayer1.xyz, LayerExtinctions.x );			// Layer 0 is only visible if extinction from layer 1 is low
	float3	Layer2 = lerp( Layer1, TexLayer2.xyz, LayerExtinctions.y );			// Layer 1 is only visible if extinction from layer 2 is low
	float3	Layer3 = lerp( Layer2, TexLayer3.xyz, LayerExtinctions.z );			// Layer 2 is only visible if extinction from layer 3 is low
	float3	DiffuseAlbedo = Layer3;												// This is the final diffuse color seen through all layers


	// Compute perceived specular
	// We proceed the same as for diffuse above except we also interpolate between specular and diffuse for each layer depending on the layer's "NoDiffuse" parameter
	// The goal is to use the diffuse texture instead of the specified specular as soon as we encounter a NoDiffuse layer (like a metal for example)
	Layer0 = lerp( TexSpecular.xyz, TexLayer0.xyz, _NoDiffuse.x );
	Layer1 = lerp( Layer0, lerp( TexSpecular.xyz, TexLayer1.xyz, _NoDiffuse.y ), LayerExtinctions.x );
	Layer2 = lerp( Layer1, lerp( TexSpecular.xyz, TexLayer2.xyz, _NoDiffuse.z ), LayerExtinctions.y );
	Layer3 = lerp( Layer2, lerp( TexSpecular.xyz, TexLayer3.xyz, _NoDiffuse.w ), LayerExtinctions.z );
	float3	SpecularAlbedo = Layer3;


	// Compute layers' weights
	float3	Transparency = 1.0 - LayerExtinctions;								// Individual transparencies
			Transparency.y *= Transparency.z;									// Cumulated transparency for layer 2 as seen through layer 3
			Transparency.x *= Transparency.y;									// Cumulated transparency for layer 1 as seen through layer 2 and 3
 	float4	LayerWeights = float4(
									Transparency.x * 1.0,						// Weight of layer 0 seen through 1, 2, 3
 									Transparency.y * TexLayer1.w,				// Weight of layer 1 seen through 2, 3
 									Transparency.z * TexLayer2.w,				// Weight of layer 2 seen through 3
 									TexLayer3.w									// Weight of layer 3 seen directly
								);

// I don't think weights should be normalized!
// The idea is rather to apply the materials one after another, light filtering through as "diffuse"
//	reaches the level below and weights should better be there to tell if the model applies or not
// Unfortunately, I don't think it's going to be realistic to apply the model layer by layer as it's
//	quite costly already...
//
	float	SumWeights = dot( LayerWeights, 1.0 );
	LayerWeights /= SumWeights;													// We normalize weights as we can't exceed one!

	// Write final result
	PS_OUT	Out;
	Out.NormalTangent = float4( CameraNormal.xy, CameraTangent.xy );			// XY=Normal  ZW=TangentXY
	Out.DiffuseAlbedo = float4( DiffuseAlbedo, CameraTangent.z );				// XYZ=Diffuse Albedo W=TangentZ
	Out.SpecularAlbedo = float4( SpecularAlbedo, _Thickness.x * TexNormal.w );	// XYZ=Specular Albedo W=Height (in millimeters)
	Out.WeightMatIDs0 = uint4(													// 4 couples of [Weight,MatID] each in [0,255]
								WriteWeightMatID( LayerWeights.x, _MatIDs.x ),
								WriteWeightMatID( LayerWeights.y, _MatIDs.y ),
								WriteWeightMatID( LayerWeights.z, _MatIDs.z ),
								WriteWeightMatID( LayerWeights.w, _MatIDs.w )
							);

	return Out;
}
