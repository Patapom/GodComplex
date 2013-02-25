//////////////////////////////////////////////////////////////////////////
// This shader applies indirect lighting and finalizes rendering
//
#include "Inc/Global.fx"
#include "Inc/LayeredMaterials.fx"

cbuffer	cbRender	: register( b10 )
{
	float3		_dUV;
	float3		_MainLightDirection;	// Main light direction
};

Texture2DArray		_TexGBuffer0 : register( t10 );	// 3 First render targets as RGBA16F
Texture2D<uint4>	_TexGBuffer1 : register( t11 );	// [Weight,MatID] target as RGBA16_UINT
Texture2D			_TexDepth : register( t12 );

Texture2DArray		_TexMaterial	: register(t13);	// 4 Slices of diffuse+blend masks + normal map + specular map = 6 textures per primitive


struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )
{
	return _In;
}

struct	WeightMatID
{
	uint	ID;
	float	Weight;
};

WeightMatID	ReadWeightMatID( uint _Packed )
{
	WeightMatID	Out;
	Out.ID = _Packed >> 8;
	Out.Weight = (_Packed & 0xFF) / 255.0f;
	return Out;
}

float4	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _dUV.xy * _In.__Position.xy;

	float4	Buf0 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );
	float4	Buf1 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );
	float4	Buf2 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 2 ), 0.0 );
	uint4	Buf3 = _TexGBuffer1.Load( _In.__Position.xyz );

	float	Z = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;
//return 0.2 * Z;

	WeightMatID		Mats[4] = {
		ReadWeightMatID( Buf3.x ),
		ReadWeightMatID( Buf3.y ),
		ReadWeightMatID( Buf3.z ),
		ReadWeightMatID( Buf3.w ),
	};
// return 0.25 * Mats[0].ID;
// return 1.0 * Mats[0].Weight;

	float3	CameraView = float3( (2.0 * UV.x - 1.0) * _CameraData.x, (1.0 - 2.0 * UV.y) * _CameraData.y, 1.0 );
	float3	CameraPosition = Z * CameraView;
			CameraView = normalize( CameraView );

	float3	CameraNormal = float3( Buf0.xy, sqrt( 1.0 - dot( Buf0.xy, Buf0.xy ) ) );
	float3	CameraTangent = float3( Buf0.zw, Buf1.w );
//return float4( CameraNormal, 1 );
// return float4( CameraTangent, 1 );
// return float4( _World2Camera[2].xyz, 1 );
//
// float3		Bisou = 2.0 * _TexMaterial.SampleLevel( LinearWrap, float3( UV, 5 ), 0.0 ).xyz - 1.0;
// //return float4( 0.5 * length(Bisou).xxx, 1 );
// //return float4( Bisou, 1 );
//
//return _Materials[0].DiffuseRoughness;
//return _Materials[0].DiffuseReflectance;
//return _Materials[0].Offset;
//return _Materials[0].Exponent.y;
//return _Materials[0].Exponent.x;
//return _Materials[0].Falloff.y;
//return _Materials[0].Falloff.x;
//return _Materials[0].Amplitude.y;
//return _Materials[0].Amplitude.x;


	float3	WorldView = CameraView.x * _Camera2World[0].xyz + CameraView.y * _Camera2World[1].xyz + CameraView.z * _Camera2World[2].xyz;
	float3	WorldPosition = CameraPosition.x * _Camera2World[0].xyz + CameraPosition.y * _Camera2World[1].xyz + CameraPosition.z * _Camera2World[2].xyz + _Camera2World[3].xyz;
	float3	WorldNormal = CameraNormal.x * _Camera2World[0].xyz + CameraNormal.y * _Camera2World[1].xyz - CameraNormal.z * _Camera2World[2].xyz;
	float3	WorldTangent = CameraTangent.x * _Camera2World[0].xyz + CameraTangent.y * _Camera2World[1].xyz - CameraTangent.z * _Camera2World[2].xyz;
	float3	WorldBiTangent = normalize( cross( WorldNormal, WorldTangent ) );
//return float4( WorldNormal, 1 );
//return float4( WorldTangent, 1 );
//return float4( WorldBiTangent, 1 );

	float3	Diffuse = Buf1.xyz;
	float3	Specular = Buf2.xyz;
	float	Height = Buf2.w;
// return float4( Diffuse, 1 );
// return Height;

	// Display the half vector space data
	float3	ViewTS = -float3( dot( WorldView, WorldTangent ), dot( WorldView, WorldBiTangent ), dot( WorldView, WorldNormal ) );
	float3	LightTS = float3( dot( _MainLightDirection, WorldTangent ), dot( _MainLightDirection, WorldBiTangent ), dot( _MainLightDirection, WorldNormal ) );
	HalfVectorSpaceParams	ViewParams = Tangent2HalfVector( ViewTS, LightTS );

//return float4( _MainLightDirection, 1 );
//return float4( -WorldView, 1 );
//return saturate( dot( -WorldView, WorldNormal ) );
//return saturate( dot( _MainLightDirection, WorldNormal ) );
// return float4( LightTS, 1 );
// return float4( ViewTS, 1 );
//return saturate( ViewTS.z );
// return saturate( LightTS.z );
// return float4( LightTS, 1 );
//return float4( pow( ViewParams.Half.zzz, 10 ), 1 );
//return float4( ViewParams.UV, 0, 0 );

	MatReflectance	Reflectance = LayeredMatEval( ViewParams, _Materials[1] );
return Reflectance.Specular;
}