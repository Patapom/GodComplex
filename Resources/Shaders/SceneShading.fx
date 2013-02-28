//////////////////////////////////////////////////////////////////////////
// This shader performs the actual scene lighting
//
#include "Inc/Global.fx"
#include "Inc/LayeredMaterials.fx"

cbuffer	cbRender	: register( b10 )
{
	float3		_dUV;
};

cbuffer	cbLight	: register( b11 )
{
	float3		_LightPosition;
	float3		_LightDirection;
	float3		_LightRadiance;
	float4		_LightData;			// For directionals: X=Hotspot Radius Y=Falloff Radius Z=Length
									// For points: X=Radius
									// For spots: X=Hotspot Angle Y=Falloff Angle Z=Length W=tan(Falloff Angle/2)
};

Texture2DArray		_TexGBuffer0 : register( t10 );	// 3 First render targets as RGBA16F
Texture2D<uint4>	_TexGBuffer1 : register( t11 );	// [Weight,MatID] target as RGBA16_UINT
Texture2D			_TexDepth : register( t12 );

Texture2DArray		_TexMaterial	: register(t13);	// 4 Slices of diffuse+blend masks + normal map + specular map = 6 textures per primitive


struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	LightX		: LIGHTX;
	float3	LightZ		: LIGHTZ;
};

struct	PS_OUT
{
	float3	Diffuse		: SV_TARGET0;
	float3	Specular	: SV_TARGET1;
};


//////////////////////////////////////////////////////////////////////////
// Light-specific functions

#if LIGHT_TYPE == 0
// ======================= DIRECTIONAL LIGHT =======================

PS_IN	VS( VS_IN _In )
{
	PS_IN	Out;
	Out.__Position = _In.__Position;
	Out.LightZ = normalize( cross( _LightDirection, float3( 1, 0, 0 ) ) );	// Won't work if light is aligned with X!
	Out.LightX = cross( Out.LightZ, _LightDirection );

	return Out;
}

float3	ComputeLightIrradiance( PS_IN _In, float3 _Position, float3 _Normal, out float3 _ToLight )
{
	_ToLight = -_LightDirection;

	// Compute radial attenuation
	float3	ToSource = _LightPosition - _Position;
	float2	ToSource2D = float2( dot( ToSource, _In.LightX ), dot( ToSource, _In.LightZ ) );
	float2	Radius = length( ToSource2D );
	float	Attenuation = smoothstep( _LightData.y, _LightData.x, Radius );

	// Compute radiance
	float	NdotL = saturate( dot( _ToLight, _Normal ) );

	return Attenuation * NdotL * _LightRadiance;	// For directionals, this is irradiance
}

#elif LIGHT_TYPE == 1
// ======================= POINT LIGHT =======================

PS_IN	VS( VS_IN _In )
{
	PS_IN	Out;
	Out.__Position = _In.__Position;
	Out.LightZ = Out.LightX = 0.0;

	return Out;
}

float3	ComputeLightIrradiance( PS_IN _In, float3 _Position, float3 _Normal, out float3 _ToLight )
{
	_ToLight = _LightPosition - _Position;

	// Compute radial attenuation
	float	SqDistance = dot( _ToLight, _ToLight );
	float	Attenuation = PI / SqDistance;

	// Compute radiance
	_ToLight /= sqrt( SqDistance );
	float	NdotL = saturate( dot( _ToLight, _Normal ) );

	return Attenuation * NdotL * _LightRadiance;
}

#else
// ======================= SPOT LIGHT =======================

PS_IN	VS( VS_IN _In )
{
	PS_IN	Out;
	Out.__Position = _In.__Position;
	Out.LightZ = normalize( cross( _LightDirection, float3( 1, 0, 0 ) ) );	// Won't work if light is aligned with X!
	Out.LightX = cross( Out.LightZ, _LightDirection );

	return Out;
}

float3	ComputeLightIrradiance( PS_IN _In, float3 _Position, float3 _Normal, out float3 _ToLight )
{
	_ToLight = _LightPosition - _Position;

	// Compute radial attenuation
	float	SqDistance = dot( _ToLight, _ToLight );
	float	Attenuation = PI / SqDistance;

	// Compute angular attenuation
	_ToLight /= sqrt( SqDistance );
	float	Angle = acos( -dot( _ToLight, _LightDirection ) );
	Attenuation *= smoothstep( _LightData.y, _LightData.x, Angle );

	// Compute radiance
	float	NdotL = saturate( dot( _ToLight, _Normal ) );

	return Attenuation * NdotL * _LightRadiance;
}

#endif

//////////////////////////////////////////////////////////////////////////
// Main code
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

PS_OUT	PS( PS_IN _In )
{
	PS_OUT	Out = (PS_OUT) 0;

	float2	UV = _dUV.xy * _In.__Position.xy;
//return float4( 1, 0, 0, 1 );

	float4	Buf0 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );
	float4	Buf1 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );
	float4	Buf2 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 2 ), 0.0 );
	uint4	Buf3 = _TexGBuffer1.Load( _In.__Position.xyz );

//Out.Diffuse = Buf1.xyz;
//Out.Diffuse = CameraTangent;
//return Out;


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

	// Unpack tangent
	float3	CameraTangent = 2.0 * float3( Buf0.zw, Buf1.w ) - 1.0;

	// Unpack stereographic normal (from http://aras-p.info/texts/CompactNormalStorage.html#method07stereo)
	// See also http://en.wikipedia.org/wiki/Stereographic_projection
 	Buf0.xy = (1.7777 * 2.0) * (Buf0.xy - 0.5);
	float	NormalScale = 2.0 / (1.0 + dot( Buf0.xy, Buf0.xy ) );
	float3	CameraNormal = float3( NormalScale * Buf0.xy, NormalScale-1.0 );


// CameraNormal = CameraTangent;


//Out.Diffuse = CameraNormal;
//Out.Diffuse = length( Buf0.xy );
//Out.Diffuse = CameraTangent;
//Out.Diffuse = 0.5 * length( Buf1.xyz );
// Out.Diffuse = 0.5 * length( CameraNormal );
// Out.Diffuse = 100.0 * abs( length( CameraNormal ) - 1.0 );
// Out.Diffuse = 100.0 * abs( length( CameraTangent ) - 1.0 );
// return Out;

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
//Out.Diffuse = 0.5 * WorldPosition;
// Out.Diffuse = 0.5 * length( CameraNormal );
// Out.Diffuse = CameraNormal;
//Out.Diffuse = WorldNormal;
//Out.Diffuse = WorldTangent;
//Out.Diffuse = WorldBiTangent;
//Out.Diffuse = 100.0 * abs( length( WorldNormal ) - 1.0 );
//Out.Diffuse = 100.0 * abs( length( WorldTangent ) - 1.0 );
//return Out;

	float3	Diffuse = Buf1.xyz;
	float3	Specular = Buf2.xyz;
	float	Height = Buf2.w;

	// Compute light irradiance
	float3	ToLight;
	float3	LightIrradiance = ComputeLightIrradiance( _In, WorldPosition, WorldNormal, ToLight );

//LightIrradiance = 1.0;

	float3	LightTS = float3( dot( ToLight, WorldTangent ), dot( ToLight, WorldBiTangent ), dot( ToLight, WorldNormal ) );


	// Display the half vector space data
	float3	ViewTS = -float3( dot( WorldView, WorldTangent ), dot( WorldView, WorldBiTangent ), dot( WorldView, WorldNormal ) );
	HalfVectorSpaceParams	ViewParams = Tangent2HalfVector( ViewTS, LightTS );

//Out.Diffuse = -WorldView;
//Out.Diffuse = saturate( dot( -WorldView, WorldNormal ) );
//Out.Diffuse = saturate( dot( ToLight, WorldNormal ) );
//Out.Diffuse = 100.0 * abs(length( WorldTangent ) - 1.0);
//Out.Diffuse = 100.0 * abs(length( WorldNormal ) - 1.0);
//Out.Diffuse = 0.5 * length( WorldNormal );
//Out.Diffuse = LightTS.z;
//Out.Diffuse = ViewTS;
//Out.Diffuse = saturate( ViewTS.z );
// Out.Diffuse = saturate( LightTS.z );
//Out.Diffuse = LightTS;
//Out.Diffuse = pow( ViewParams.Half.zzz, 10 );
//Out.Diffuse = float3( ViewParams.UV, 0 );
//return Out;


	MatReflectance	Reflectance = LayeredMatEval( ViewParams, _Materials[0] );
//return Reflectance.Specular;


	Out.Diffuse = LightIrradiance * (Reflectance.Diffuse + Reflectance.RetroDiffuse);
	Out.Specular = LightIrradiance * Reflectance.Specular;
	return Out;
}