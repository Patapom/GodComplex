//////////////////////////////////////////////////////////////////////////
// This shader displays the objects
//
#include "Inc/Global.hlsl"

//[
cbuffer	cbRender	: register( b10 )
{
	float3		_dUV;
	float3		_Ambient;
};

cbuffer	cbLight	: register( b11 )
{
	float3		_LightPosition;
	uint		_LightType;
	float3		_LightDirection;
	float3		_LightColor;
	float4		_LightData;			// For directionals: X=Hotspot Radius Y=Falloff Radius Z=Length
									// For spots: X=Hotspot Angle Y=Falloff Angle Z=Length W=tan(Falloff Angle/2)
									// For points: X=Radius

};
//]

Texture2DArray	_TexGBuffer	: register(t10);
Texture2D		_TexZBuffer	: register(t11);

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

struct	PS_OUT
{
	float3	Diffuse : SV_TARGET0
	float3	Specular : SV_TARGET1
};

VS_IN	VS( VS_IN _In )	{ return _In; }

float3	ComputeLight( float3 _Position, float3 _Normal, out float3 _LightDir )
{
	if ( _LightType == 0 )
	{	// DIRECTIONAL
		_LightDir = _LightDirection;

		float3	X = normalize( cross( _LightDirection, float3( 1, 0, 0 ) ) );
		float3	Y = normalize( cross( _LightDirection, X ) );
		float3	ToPosition = _Position - _LightPosition;
		float2	ToPosition2D = float2( dot( ToPosition, X ), dot( ToPosition, Y ) );
		float	Radius = length( ToPostion2D );
		float	Attenuation = smoothstep( _LightData.y, _LightData.x, Radius );
		return Attenuation * _LightColor;
	}
	else if ( _LightType == 1 )
	{	// SPOT
		_LightDir = _Position - _LightPosition;
	}
	else
	{	// OMNI
		_LightDir = _Position - _LightPosition;
	}
}

PS_OUT	PS( VS_IN _In )
{
	float2	UV = _In.__Position.xy * _dUV.xy;

//	float4	Tex0 = _TexGBuffer.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );	// RGB=Diffuse A=Specular
	float4	Tex1 = _TexGBuffer.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );	// RG=Stereo Normal B=Roughness A=Ambient Occlusion

	// Retrieve unprojected Z
	float	Q = _CameraData.w / (_CameraData.w - _CameraData.z);	// Zf / (Zf-Zn)
	float	Zproj = _TexZBuffer.SampleLevel( LinearClamp, UV, 0.0 ).x;
	float	Z = (Q * _CameraData.z) / (Q - Zproj);

	// Unproject normal
 	Tex1.xy = (1.57 * 2.0) * (Tex1.xy - 0.5);
	float	NormalScale = 2.0 / (1.0 + dot( Tex1.xy, Tex1.xy ) );
	float3	CameraNormal = float3( NormalScale * Tex1.xy, NormalScale-1.0 );

	float3	WorldNormal = normalize( mul( float4( CameraNormal, 0.0 ), _Camera2World ).xyz );

	// Compute view vector
	float3	CameraView = float3( _CameraData.x * (2.0 * UV.x - 1.0), _CameraData.y * (1.0 - 2.0 * UV.y), 1.0 );
	float3	WorldView = normalize( mul( float4( CameraView, 0.0 ), _Camera2World ).xyz );

	// Retrieve world position
	float3	CameraPosition = Z * CameraView;
	float3	WorldPosition = mul( float4( CameraPosition, 1.0 ), _Camera2World ).xyz;

	// Compute light direction & intensity
	float3	WorldLight;
	float3	LightIntensity = ComputeLight( WorldPosition, WorldNormal, WorldLight );

	// Apply Blinn-Phong model
	float3	Half = normalize( WorldLight + WorldView );
	float	N = exp2( 10.0 * (1.0 - Tex1.z) );
	float	Fact = (N+2)*(N+4)/(8*PI*(exp2(-0.5*N)+N));
	float	Specular = Fact * pow( saturate( -dot( Half, WorldNormal ) ), N );

	PS_OUT	Out;
	Out.Diffuse = LightIntensity;
	Out.Specular = Specular * LightIntensity;

	return Out;
}
