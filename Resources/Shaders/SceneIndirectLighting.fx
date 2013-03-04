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
Texture2D			_TexDepthFrontDownSampled : register( t13 );

Texture2DArray		_TexDiffuseSpecular	: register(t14);	// Diffuse + Specular in 2 slices

Texture2D			_TexEnvMap	: register(t15);	// The spherical projection env map with mips

Texture2DArray		_TexDEBUGMaterial	: register(t16);	// 4 Slices of diffuse+blend masks + normal map + specular map = 6 textures per primitive
Texture2D			_TexDEBUGBackGBuffer: register(t17);


struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )
{
	return _In;
}

WeightMatID	ReadWeightMatID( uint _Packed )
{
	WeightMatID	Out;
	Out.ID = _Packed >> 8;
	Out.Weight = (_Packed & 0xFF) / 255.0f;
	return Out;
}

float3	SampleEnvMap( float3 _Direction, float _MipLevel )
{
	float	EnvMapPhi = 0.0;
	float2	UV = float2( 0.5 * (1.0 + (atan2( _Direction.x, -_Direction.z ) + EnvMapPhi) * INVPI), acos( _Direction.y ) * INVPI );
	return _TexEnvMap.SampleLevel( LinearWrap, UV, _MipLevel ).xyz;
}

bool	Intersect( float3 _Position, float3 _Direction, out float4 _Intersection, out float2 _UV, out float4 _Debug )
{
	const float	StepsCount = 32;
	const float	MaxRadius = 2.0;	// 2 world units max until we drop the computation

_Debug = 0.0;

	_Intersection = float4( _Position, 0.0 );
	float4	Step = (MaxRadius / StepsCount) * float4( _Direction, 1.0 );
//			Step = float4( Step.xy / (_Z * _CameraData.xy), Step.zw );

//Step /= saturate( 1.0 + Step.z );
//_Debug = saturate( 0.2 + Step.z );

	_Intersection += 0.05 * Step;

	float2	PreviousZ, CurrentZ = _Position.zz;
	float	StepIndex = 0;
	for ( ; StepIndex < StepsCount; StepIndex++ )
	{
//		_Intersection += Step;

		// Retrieve UVs from current camera position
		_UV = _Intersection.xy / (_Intersection.z * _CameraData.xy);
		_UV = float2( 0.5 * (1.0 + _UV.x), 0.5 * (1.0 - _UV.y) );
// _Debug = float3( _UV, 0 );
// return true;

		// Sample Z buffer at this position
		PreviousZ = CurrentZ;
		CurrentZ = _TexDepth.SampleLevel( LinearClamp, _UV, 0.0 ).xy;
//		CurrentZ = _TexDepthFrontDownSampled.SampleLevel( LinearClamp, _UV, 0.0 ).xy;

		if ( _Intersection.z > CurrentZ.x )
		{	// Frontface Hit! Interpolate to "exact intersection"...
			float	t = (_Intersection.z - CurrentZ.x) / (PreviousZ.x - CurrentZ.x + Step.z);

_Debug.x = _Intersection.z - CurrentZ.x;
_Debug.y = t;

			_Intersection -= t * Step;
//			StepIndex -= t;

			_UV = _Intersection.xy / (_Intersection.z * _CameraData.xy);
			_UV = float2( 0.5 * (1.0 + _UV.x), 0.5 * (1.0 - _UV.y) );
			return true;
		}

// 		if ( CurrentZ.x > CurrentZ.y && CurrentZ.y > _Intersection.z )
// 		{	// Backface Hit! Interpolate to "exact intersection"...
// 			float	t = (_Intersection.z - CurrentZ.y) / (PreviousZ.y - CurrentZ.y + Step.z);
// 			_Intersection -= t * Step;
// //			StepIndex -= t;
// 			_Debug = 1;
// 
// 			_UV = _Intersection.xy / (_Intersection.z * _CameraData.xy);
// 			_UV = float2( 0.5 * (1.0 + _UV.x), 0.5 * (1.0 - _UV.y) );
// 			return true;
// 		}

		_Intersection += Step;
	}

	return false;
}

float3	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _dUV.xy * _In.__Position.xy;

	float4	Buf0 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );
	float4	Buf1 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );
	float4	Buf2 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 2 ), 0.0 );
	uint4	Buf3 = _TexGBuffer1.Load( _In.__Position.xyz );

	// Prepare necessary informations
	float	Z = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;

//return 0.1 * Z;
//return 0.1 * _TexDepthFrontDownSampled.SampleLevel( LinearClamp, UV, 0.0 ).x;

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

//return 100.0 * abs( Mats[0].Weight - DiffuseAlbedo.x );
//return sqrt(DiffuseAlbedo);
//return Mats[0].Weight;
//return 0.2 * Mats[0].ID;

	// Recompose and unpack tangent
	float3	CameraTangent = 2.0 * float3( Buf0.zw, Buf1.w ) - 1.0;

	// Unpack stereographic normal (from http://aras-p.info/texts/CompactNormalStorage.html#method07stereo)
	// See also http://en.wikipedia.org/wiki/Stereographic_projection
 	Buf0.xy = (1.57 * 2.0) * (Buf0.xy - 0.5);
	float	NormalScale = 2.0 / (1.0 + dot( Buf0.xy, Buf0.xy ) );
	float3	CameraNormal = float3( NormalScale * Buf0.xy, 1.0-NormalScale );

//return float3( 1, 1, -1 ) * CameraNormal;

	// Transform everything into world space
// 	float3	WorldView = CameraView.x * _Camera2World[0].xyz + CameraView.y * _Camera2World[1].xyz + CameraView.z * _Camera2World[2].xyz;
// 	float3	WorldPosition = CameraPosition.x * _Camera2World[0].xyz + CameraPosition.y * _Camera2World[1].xyz + CameraPosition.z * _Camera2World[2].xyz + _Camera2World[3].xyz;
// 	float3	WorldNormal = CameraNormal.x * _Camera2World[0].xyz + CameraNormal.y * _Camera2World[1].xyz - CameraNormal.z * _Camera2World[2].xyz;
// 	float3	WorldTangent = CameraTangent.x * _Camera2World[0].xyz + CameraTangent.y * _Camera2World[1].xyz + CameraTangent.z * _Camera2World[2].xyz;
// 	float3	WorldBiTangent = normalize( cross( WorldNormal, WorldTangent ) );
// 
// 	// Compute light irradiance
// 	float3	ToLight;
// 	float3	LightIrradiance = ComputeLightIrradiance( _In, WorldPosition, WorldNormal, ToLight );
// 
// 	// Compute half vector space data
// 	float3	LightTS = float3( dot( ToLight, WorldTangent ), dot( ToLight, WorldBiTangent ), dot( ToLight, WorldNormal ) );
// 	float3	ViewTS = -float3( dot( WorldView, WorldTangent ), dot( WorldView, WorldBiTangent ), dot( WorldView, WorldNormal ) );
// 	HalfVectorSpaceParams	ViewParams = Tangent2HalfVector( ViewTS, LightTS );
// 
// 	// Retrieve weighted material params
// 	MaterialParams	MatParams = ComputeWeightedMaterialParams( Mats );
// 	MatReflectance	Reflectance = LayeredMatEval( ViewParams, MatParams );

	float3	CameraBiTangent = normalize( cross( CameraTangent, CameraNormal ) );

	// Compute view reflection
	float3	CameraReflect = reflect( CameraView, CameraNormal );

	// Use camera view for empty pixels
	float	InfinityZ = saturate( 10000.0 * (Z - 50.0) );
	CameraReflect = lerp( CameraReflect, CameraView, InfinityZ );

//return float4( SampleEnvMap( CameraReflect.x * _Camera2World[0].xyz + CameraReflect.y * _Camera2World[1].xyz + CameraReflect.z * _Camera2World[2].xyz, 0.0 ), 1 );
//return float4( CameraView, 0 );
//return float4( float3( 1,1,-1 ) * CameraTangent, 0 );
//return float4( float3( 1,1,-1 ) * CameraBiTangent, 0 );
//return float4( float3( 1,1,-1 ) * CameraNormal, 0 );
//return float4( CameraReflect, 0 );


	float3	Reflection = 0.0;
#if 1
	// Proceed with intersection
	float4	SceneReflection = 0.0;
	float4	Intersection, DEBUG;
	float2	IntersectionUV;
	if ( InfinityZ == 0.0 && Intersect( CameraPosition, CameraReflect, Intersection, IntersectionUV, DEBUG ) )
	{
//return DEBUG;
	 	SceneReflection.xyz = _TexDiffuseSpecular.SampleLevel( LinearClamp, float3( IntersectionUV, 0 ), 0.0 ).xyz;

//Reflection = 0.5 * DEBUG.y;
//Reflection = 0.5 * DEBUG.x;

		SceneReflection.w = saturate( 2.0 * (0.5 - DEBUG.x) );	// Blends with envmap

// Darkens reflection
SceneReflection.xyz *= saturate( 1.0 * (0.5 - DEBUG.x) );
SceneReflection.w = 1.0;


// 	 	Reflection = DEBUG.x == 0
// 			? _TexDiffuseSpecular.SampleLevel( LinearClamp, float3( IntersectionUV, 0 ), 0.0 ).xyz
// 			: _TexDEBUGBackGBuffer.SampleLevel( LinearClamp, IntersectionUV, 0.0 ).xyz;
	}

	float3	EnvMapReflection = lerp( 0.2 * INVPI * SpecularAlbedo, 1.0, InfinityZ ) * SampleEnvMap( CameraReflect.x * _Camera2World[0].xyz + CameraReflect.y * _Camera2World[1].xyz + CameraReflect.z * _Camera2World[2].xyz, 0.0 );

	Reflection = lerp( EnvMapReflection, SceneReflection.xyz, SceneReflection.w );

//return float4( -4.0 * Step.yyy, 0 );
//return 10 * StepIndex / StepsCount;
// return Intersection.w;
//return float4( abs(Intersection.xy - UV), 0, 0 );
// return float4( Intersection.xy, 0, 0 );
// return float4( 10.0 * Step.xy, 0, 0 );
#endif


	float3	AccDiffuse = _TexDiffuseSpecular.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 ).xyz;
	float3	AccSpecular = _TexDiffuseSpecular.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 ).xyz;

AccSpecular += Reflection;

//return AccSpecular;
//return AccDiffuse;

//return 0.1 * _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).y;
//return _TexDEBUGBackGBuffer.SampleLevel( LinearClamp, UV, 0.0 );

	return AccDiffuse + AccSpecular;
}