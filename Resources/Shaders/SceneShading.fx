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

	float4	Buf0 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );
	float4	Buf1 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );
	float4	Buf2 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 2 ), 0.0 );
	uint4	Buf3 = _TexGBuffer1.Load( _In.__Position.xyz );

	// Prepare necessary informations
	float	Z = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;

	float3	DiffuseAlbedo = Buf1.xyz;
	float3	SpecularAlbedo = Buf2.xyz;
	float	Height = Buf2.w;

	WeightMatID		Mats[4] = {
		ReadWeightMatID( Buf3.x ),
		ReadWeightMatID( Buf3.y ),
		ReadWeightMatID( Buf3.z ),
		ReadWeightMatID( Buf3.w ),
	};

	float3	CameraView = float3( (2.0 * UV.x - 1.0) * _CameraData.x, (1.0 - 2.0 * UV.y) * _CameraData.y, 1.0 );
	float3	CameraPosition = Z * CameraView;
			CameraView = normalize( CameraView );

	// Recompose and unpack tangent
	float3	CameraTangent = 2.0 * float3( Buf0.zw, Buf1.w ) - 1.0;

	// Unpack stereographic normal (from http://aras-p.info/texts/CompactNormalStorage.html#method07stereo)
	// See also http://en.wikipedia.org/wiki/Stereographic_projection
 	Buf0.xy = (1.7777 * 2.0) * (Buf0.xy - 0.5);
	float	NormalScale = 2.0 / (1.0 + dot( Buf0.xy, Buf0.xy ) );
	float3	CameraNormal = float3( NormalScale * Buf0.xy, NormalScale-1.0 );

	// Transform everything into world space
	float3	WorldView = CameraView.x * _Camera2World[0].xyz + CameraView.y * _Camera2World[1].xyz + CameraView.z * _Camera2World[2].xyz;
	float3	WorldPosition = CameraPosition.x * _Camera2World[0].xyz + CameraPosition.y * _Camera2World[1].xyz + CameraPosition.z * _Camera2World[2].xyz + _Camera2World[3].xyz;
	float3	WorldNormal = CameraNormal.x * _Camera2World[0].xyz + CameraNormal.y * _Camera2World[1].xyz - CameraNormal.z * _Camera2World[2].xyz;
	float3	WorldTangent = CameraTangent.x * _Camera2World[0].xyz + CameraTangent.y * _Camera2World[1].xyz + CameraTangent.z * _Camera2World[2].xyz;
	float3	WorldBiTangent = normalize( cross( WorldNormal, WorldTangent ) );

	// Compute light irradiance
	float3	ToLight;
	float3	LightIrradiance = ComputeLightIrradiance( _In, WorldPosition, WorldNormal, ToLight );

	// Compute half vector space data
	float3	LightTS = float3( dot( ToLight, WorldTangent ), dot( ToLight, WorldBiTangent ), dot( ToLight, WorldNormal ) );
	float3	ViewTS = -float3( dot( WorldView, WorldTangent ), dot( WorldView, WorldBiTangent ), dot( WorldView, WorldNormal ) );
	HalfVectorSpaceParams	ViewParams = Tangent2HalfVector( ViewTS, LightTS );

	// Retrieve weighted material params
	MaterialParams	MatParams = ComputeWeightedMaterialParams( Mats );
	MatReflectance	Reflectance = LayeredMatEval( ViewParams, MatParams );


// DEBUG Use a simple blinn-phong model with lambert for diffuse
// float	S = 100.0;
// float	Kd = 0.2 * INVPI;
// float	Ks = 0.3 * INVPI;
// 
// float	SpecFactor = (S+2) * (S+4) / (8.0*PI*(exp2(-0.5*S) + S));
// Reflectance.Specular = Ks * SpecFactor * pow( saturate( ViewParams.Half.z ), S );
// Reflectance.Diffuse = Kd * saturate( LightTS.z );
// Reflectance.RetroDiffuse = 0.0;
// DEBUG


	Out.Diffuse = LightIrradiance * (Reflectance.Diffuse + Reflectance.RetroDiffuse) * DiffuseAlbedo;
	Out.Specular = LightIrradiance * Reflectance.Specular * SpecularAlbedo;


// DEBUG => Show the array of material parameters
// uint	MatID = uint( floor( 4*UV.y ) );
// uint	ComponentIndex = uint( floor( 9 * UV.x ) );
// float	Bli = 0.0;
// switch ( ComponentIndex )
// {
// case 0: Bli = 0.1 * _Materials[MatID].AmplitudeFalloff.x; break;
// case 1: Bli = 0.1 * _Materials[MatID].AmplitudeFalloff.y; break;
// case 2: Bli = 1.0 * _Materials[MatID].AmplitudeFalloff.z; break;
// case 3: Bli = 1.0 * _Materials[MatID].AmplitudeFalloff.w; break;
// case 4: Bli = 1.0 * _Materials[MatID].ExponentDiffuse.x; break;
// case 5: Bli = 1.0 * _Materials[MatID].ExponentDiffuse.y; break;
// case 6: Bli = 1.0 * _Materials[MatID].ExponentDiffuse.z; break;
// case 7: Bli = 1.0 * _Materials[MatID].ExponentDiffuse.w; break;
// case 8: Bli = 1.0 * _Materials[MatID].Offset; break;
// }
// Out.Diffuse = Bli;
// DEBUG

	return Out;
}