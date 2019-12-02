//////////////////////////////////////////////////////////////////////////
// This shader applies indirect lighting and finalizes rendering
//
#include "Inc/Global.hlsl"
#include "Inc/LayeredMaterials.hlsl"

cbuffer	cbRender	: register( b10 )
{
	float3		_dUV;
	float3		_MainLightDirection;	// Main light direction
};

Texture2DArray		_TexGBuffer0 : register( t10 );	// 3 First render targets as RGBA16F
Texture2D<uint4>	_TexGBuffer1 : register( t11 );	// [Weight,MatID] target as RGBA16_UINT
Texture2D			_TexDepth : register( t12 );

Texture2DArray		_TexDiffuseSpecular	: register(t14);	// Diffuse + Specular in 2 slices

Texture2D			_TexEnvMap	: register(t15);	// The spherical projection env map with mips


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

float3	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _dUV.xy * _In.__Position.xy;

	float4	Buf0 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );
	float4	Buf1 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );
	float4	Buf2 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 2 ), 0.0 );
	uint4	Buf3 = _TexGBuffer1.Load( _In.__Position.xyz );

	// Prepare necessary informations
	float	Z = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;
//return 0.1 * _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;;

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
 	Buf0.xy = (1.57 * 2.0) * (Buf0.xy - 0.5);
	float	NormalScale = 2.0 / (1.0 + dot( Buf0.xy, Buf0.xy ) );
	float3	CameraNormal = float3( NormalScale * Buf0.xy, 1.0-NormalScale );


	float3	CameraBiTangent = normalize( cross( CameraTangent, CameraNormal ) );

	// Compute view reflection
	float3	CameraReflect = reflect( CameraView, CameraNormal );

	// Use camera view for empty pixels
	float	InfinityZ = saturate( 10000.0 * (Z - 50.0) );
	CameraReflect = lerp( CameraReflect, CameraView, InfinityZ );

	///////////////////////////////////////////////////////////////////////////////////
	float3	Reflection = lerp( 0.5 * INVPI * lerp( float3( 1, 1, 1 ), SpecularAlbedo, 0.25 ), 1.0, InfinityZ ) * SampleEnvMap( CameraReflect.x * _Camera2World[0].xyz + CameraReflect.y * _Camera2World[1].xyz + CameraReflect.z * _Camera2World[2].xyz, 0.0 );

	float3	AccDiffuse = _TexDiffuseSpecular.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 ).xyz;
	float3	AccSpecular = _TexDiffuseSpecular.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 ).xyz;

	AccSpecular += Reflection;

	return AccDiffuse + AccSpecular;
}


Texture2D			_TexSource	: register(t16);

float3	PS_Finalize( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _dUV.xy * _In.__Position.xy;
	return _TexSource.SampleLevel( LinearClamp, UV, 0.0 ).xyz;// + float3( 0.1, 0, 0 );
	return _TexSource.SampleLevel( LinearClamp, UV, 2.0 ).yzx;
}
