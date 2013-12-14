//////////////////////////////////////////////////////////////////////////
// This shader renders a cube map at a specified position
// Each face of the cubemap will be composed of 2 render targets:
//	RT0 = Albedo (RGB) + Empty (A)
//	RT1 = Normal (RGB) + Distance (Z)
//

// ======================= START INCLUDE Inc/Global.hlsl =======================
////////////////////////////////////////////////////////////////////////////////////////
// Global stuff
////////////////////////////////////////////////////////////////////////////////////////
//
#ifndef _GLOBAL_INC_
#define _GLOBAL_INC_

// WARNING! These must correspond to the resolution set in GodComplex.h !
static const float	RESX = 1280.0;
static const float	RESY = 720.0;
static const float	ASPECT_RATIO = RESX / RESY;
static const float	INV_ASPECT_RATIO = RESY / RESX;
static const float2	SCREEN_SIZE = float2( RESX, RESY );
static const float2	INV_SCREEN_SIZE = float2( 1.0/RESX, 1.0/RESY );

static const float	PI = 3.1415926535897932384626433832795;			// ...
static const float	TWOPI = 6.283185307179586476925286766559;		// 2PI
static const float	FOURPI = 12.566370614359172953850573533118;		// 4PI
static const float	HALFPI = 1.5707963267948966192313216916398;		// PI/2
static const float	INVPI = 0.31830988618379067153776752674503;		// 1/PI
static const float	INVHALFPI = 0.63661977236758134307553505349006;	// 1/(PI/2)
static const float	INVTWOPI = 0.15915494309189533576888376337251;	// 1/2PI
static const float	INVFOURPI = 0.07957747154594766788444188168626;	// 1/4PI

static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2� observer (cf. http://wiki.nuaj.net/index.php?title=Colorimetry)

static const float	INFINITY = 1e6;


////////////////////////////////////////////////////////////////////////////////////////
// Samplers
SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );


////////////////////////////////////////////////////////////////////////////////////////
// Constants

//[ // Minifier doesn't'support cbuffers !
cbuffer	cbCamera	: register( b0 )
{
	float4		_CameraData;		// X=tan(FOV_H/2) Y=tan(FOV_V/2) Z=Near W=Far
	float4x4	_Camera2World;
	float4x4	_World2Camera;
	float4x4	_Camera2Proj;
	float4x4	_Proj2Camera;
	float4x4	_World2Proj;
	float4x4	_Proj2World;
};

cbuffer	cbGlobal	: register( b1 )
{
	float4		_Time;				// X=Time Y=DeltaTime Z=1/Time W=1/DeltaTime
};
//]


Texture3D	_TexNoise3D	: register(t0);


////////////////////////////////////////////////////////////////////////////////////////
// Distort position with noise
// float3	Distort( float3 _Position, float3 _Normal, float4 _NoiseOffset )
// {
// 	float	Noise = _NoiseOffset.w * (-1.0 + _TexNoise3D.SampleLevel( LinearWrap, 0.2 * (_Position + _NoiseOffset.xyz), 0.0 ).x);
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
#define BILERP( a, b, c, d, uv )	lerp( lerp( a, d, uv.x ), lerp( b, c, uv.x ), uv.y )


////////////////////////////////////////////////////////////////////////////////////////
// Rotates a vector about an axis
// float3	RotateVector( float3 v, float3 _Axis, float _Angle )
// {
//		_Axis = normalize( _Axis );
//		float3	n = _Axis * dot( _Axis, v );
// 		float2	SC;
// 		sincos( _Angle, SC.x, SC.y );
//		return n + SC.y * (v - n) + SC.x * cross( _Axis, v );
// }

float3	RotateVector( float3 _Vector, float3 _Axis, float _Angle )
{
	float2	SinCos;
	sincos( _Angle, SinCos.x, SinCos.y );

	float3	Result = _Vector * SinCos.y;
	float	temp = dot( _Vector, _Axis );
			temp *= 1.0 - SinCos.y;

	Result += _Axis * temp;

	float3	Ortho = cross( _Axis, _Vector );

	Result += Ortho * SinCos.x;

	return Result;
}

#endif	// _GLOBAL_INC_
// ======================= END INCLUDE Inc/Global.hlsl =======================


//[
cbuffer	cbCubeMapCamera	: register( b9 )
{
	float4x4	_CubeMapWorld2Proj;
};
//]

//[
cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
};
//]

//[
cbuffer	cbMaterial	: register( b11 )
{
	float3		_DiffuseAlbedo;
	bool		_HasDiffuseTexture;
	float3		_SpecularAlbedo;
	bool		_HasSpecularTexture;
	float		_SpecularExponent;
};
//]

Texture2D<float4>	_TexDiffuseAlbedo : register( t10 );
Texture2D<float4>	_TexSpecularAlbedo : register( t11 );


struct	VS_IN
{
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float3	BiTangent	: BITANGENT;
	float3	UV			: TEXCOORD0;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float3	BiTangent	: BITANGENT;
	float3	UV			: TEXCOORD0;
};

struct	PS_OUT
{
	float3	DiffuseAlbedo	: SV_TARGET0;
	float4	NormalDistance	: SV_TARGET1;
};

PS_IN	VS( VS_IN _In )
{
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _CubeMapWorld2Proj );
	Out.Position = WorldPosition.xyz;
	Out.Normal = mul( float4( _In.Normal, 0.0 ), _Local2World );
	Out.Tangent = mul( float4( _In.Tangent, 0.0 ), _Local2World );
	Out.BiTangent = mul( float4( _In.BiTangent, 0.0 ), _Local2World );
	Out.UV = _In.UV;

	return Out;
}

PS_OUT	PS( PS_IN _In )
{
	PS_OUT	Out;
	Out.DiffuseAlbedo = _DiffuseAlbedo;
	if ( _HasDiffuseTexture )
		Out.DiffuseAlbedo = _TexDiffuseAlbedo.Sample( LinearWrap, _In.UV ).xyz;

//	Out.NormalDistance = float4( normalize( _In.Normal ), length( _In.Position - _Camera2World[3].xyz ) );	// Store distance
	Out.NormalDistance = float4( normalize( _In.Normal ), dot( _In.Position - _Camera2World[3].xyz, _Camera2World[2].xyz ) );	// Store Z
	
	return Out;
}
