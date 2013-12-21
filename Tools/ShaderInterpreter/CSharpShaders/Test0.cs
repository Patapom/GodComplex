using System;
using ShaderInterpreter.ShaderMath;
using ShaderInterpreter.Textures;

namespace ShaderInterpreter
{
	public class	Test : Shader
	{
//////////////////////////////////////////////////////////////////////////
// This shader renders a cube map at a specified position
// Each face of the cubemap will be composed of 2 render targets:
//	RT0 = Albedo (RGB) + Empty (A)
//	RT1 = Normal (RGB) + Distance (Z)
//

	#region ======================= START INCLUDE Inc/Global.hlsl =======================
////////////////////////////////////////////////////////////////////////////////////////
// Global stuff
////////////////////////////////////////////////////////////////////////////////////////
//
// #ifndef _GLOBAL_INC_
// #define _GLOBAL_INC_

// WARNING! These must correspond to the resolution set in GodComplex.h !
static double	RESX = 1280.0;
static double	RESY = 720.0;
static double	ASPECT_RATIO = RESX / RESY;
static double	INV_ASPECT_RATIO = RESY / RESX;
static float2	SCREEN_SIZE = _float2( RESX, RESY );
static float2	INV_SCREEN_SIZE = _float2( 1.0/RESX, 1.0/RESY );

static double	PI = 3.1415926535897932384626433832795;			// ...
static double	TWOPI = 6.283185307179586476925286766559;		// 2PI
static double	FOURPI = 12.566370614359172953850573533118;		// 4PI
static double	HALFPI = 1.5707963267948966192313216916398;		// PI/2
static double	INVPI = 0.31830988618379067153776752674503;		// 1/PI
static double	INVHALFPI = 0.63661977236758134307553505349006;	// 1/(PI/2)
static double	INVTWOPI = 0.15915494309189533576888376337251;	// 1/2PI
static double	INVFOURPI = 0.07957747154594766788444188168626;	// 1/4PI

static float3	LUMINANCE = _float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2� observer (cf. http://wiki.nuaj.net/index.php?title=Colorimetry)

static double	INFINITY = 1e6;


////////////////////////////////////////////////////////////////////////////////////////
// Samplers
[Register( "s0" )]
SamplerState LinearClamp	;
[Register( "s1" )]
SamplerState PointClamp		;
[Register( "s2" )]
SamplerState LinearWrap		;
[Register( "s3" )]
SamplerState PointWrap		;
[Register( "s4" )]
SamplerState LinearMirror	;
[Register( "s5" )]
SamplerState PointMirror	;


////////////////////////////////////////////////////////////////////////////////////////
// Constants

//[ // Minifier doesn't'support cbuffers !
//[cbuffer]
//cbuffer	cbCamera	: register( b0 )
{
	float4		_CameraData;		// X=tan(FOV_H/2) Y=tan(FOV_V/2) Z=Near W=Far
	float4x4	_Camera2World;
	float4x4	_World2Camera;
	float4x4	_Camera2Proj;
	float4x4	_Proj2Camera;
	float4x4	_World2Proj;
	float4x4	_Proj2World;
};

[cbuffer]
//cbuffer	cbGlobal	: register( b1 )
{
	float4		_Time;				// X=Time Y=DeltaTime Z=1/Time W=1/DeltaTime
};
//]


[Register( "t0" )]
Texture3D<float4>	_TexNoise3D	;


////////////////////////////////////////////////////////////////////////////////////////
// Distort position with noise
// float3	Distort( float3 _Position, float3 _Normal, float4 _NoiseOffset )
// {
// 	double	Noise = _NoiseOffset.w * (-1.0 + _TexNoise3D.SampleLevel( LinearWrap, 0.2 * (_Position + _NoiseOffset.xyz), 0.0 ).x);
// 	return	_Position + Noise * _Normal;
// }

float3	Distort( float3 _Position, float3 _Normal, float4 _NoiseOffset )
{
	return _Position + _NoiseOffset.w * _TexNoise3D.SampleLevel( LinearWrap, 0.2 * (_Position + _NoiseOffset.xyz), 0.0 ).xyz;
}


////////////////////////////////////////////////////////////////////////////////////////
// Standard bilinear interpolation on a quad
//
//	a ---- d --> U
//	|      |
//	|      |
//	|      |
//	b ---- c
//  :
//  v V
//
// #define BILERP( a, b, c, d, uv )	lerp( lerp( a, d, uv.x ), lerp( b, c, uv.x ), uv.y )


////////////////////////////////////////////////////////////////////////////////////////
// Rotates a vector about an axis
// float3	RotateVector( float3 v, float3 _Axis, double _Angle )
// {
//		_Axis = normalize( _Axis );
//		float3	n = _Axis * dot( _Axis, v );
// 		float2	SC;
// 		sincos( _Angle, SC.x, SC.y );
//		return n + SC.y * (v - n) + SC.x * cross( _Axis, v );
// }

float3	RotateVector( float3 _Vector, float3 _Axis, double _Angle )
{
	float2	SinCos;
	sincos( _Angle, SinCos.x, SinCos.y );

	float3	Result = _Vector * SinCos.y;
	double	temp = dot( _Vector, _Axis );
			temp *= 1.0 - SinCos.y;

	Result += _Axis * temp;

	float3	Ortho = cross( _Axis, _Vector );

	Result += Ortho * SinCos.x;

	return Result;
}

// #endif	// _GLOBAL_INC_
	#endregion // ======================= END INCLUDE Inc/Global.hlsl =======================


//[
[cbuffer]
//cbuffer	cbCubeMapCamera	: register( b9 )
{
	float4x4	_CubeMap2World;
	float4x4	_CubeMapWorld2Proj;
};
//]

//[
[cbuffer]
//cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
};
//]

//[
[cbuffer]
//cbuffer	cbMaterial	: register( b11 )
{
	float3		_DiffuseAlbedo;
	bool		_HasDiffuseTexture;
	float3		_SpecularAlbedo;

	bool		_HasSpecularTexture;
	//test no public!
	double		_SpecularExponent;
};
//]

[Register( "t10" )]
Texture2D<float4>	_TexDiffuseAlbedo ;
[Register( "t11" )]
Texture2D<float4>	_TexSpecularAlbedo ;


struct	VS_IN
{
[Semantic( "POSITION" )]
public 	float3	Position	;
[Semantic( "NORMAL" )]
public 	float3	Normal		;
[Semantic( "TANGENT" )]
public 	float3	Tangent		;
[Semantic( "BITANGENT" )]
public 	float3	BiTangent	;
[Semantic( "TEXCOORD0" )]
public 	float3	UV			;
};

struct	PS_IN
{
[Semantic( "SV_POSITION" )]
public 	float4	__Position	;
[Semantic( "POSITION" )]
public 	float3	Position	;
[Semantic( "NORMAL" )]
public 	float3	Normal		;
[Semantic( "TANGENT" )]
public 	float3	Tangent		;
[Semantic( "BITANGENT" )]
public 	float3	BiTangent	;
[Semantic( "TEXCOORD0" )]
public 	float3	UV			;
};

struct	PS_OUT
{
[Semantic( "SV_TARGET0" )]
public 	float3	DiffuseAlbedo	;
[Semantic( "SV_TARGET1" )]
public 	float4	NormalDistance	;
};

PS_IN	VS( VS_IN _In )
{
	float4	WorldPosition = mul( _float4( _In.Position, 1.0 ), _Local2World );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _CubeMapWorld2Proj );
	Out.Position = WorldPosition.xyz;
	Out.Normal = mul( _float4( _In.Normal, 0.0 ), _Local2World );
	Out.Tangent = mul( _float4( _In.Tangent, 0.0 ), _Local2World );
	Out.BiTangent = mul( _float4( _In.BiTangent, 0.0 ), _Local2World );
	Out.UV = _In.UV;

	return Out;
}

PS_OUT	PS( PS_IN _In )
{
	PS_OUT	Out;
	Out.DiffuseAlbedo = _DiffuseAlbedo;
	if ( _HasDiffuseTexture )
		Out.DiffuseAlbedo = _TexDiffuseAlbedo.Sample( LinearWrap, _In.UV ).xyz;

	Out.NormalDistance = _float4( normalize( _In.Normal ), length( _In.Position - _CubeMap2World[3].xyz ) );	// Store distance
//	Out.NormalDistance = _float4( normalize( _In.Normal ), dot( _In.Position - _CubeMap2World[3].xyz, _CubeMap2World[2].xyz ) );	// Store Z
	
	return Out;
}

	}

}
