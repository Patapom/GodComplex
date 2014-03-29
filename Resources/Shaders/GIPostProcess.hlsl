//////////////////////////////////////////////////////////////////////////
// This shader post-processes the Global Illumination test room
//
#include "Inc/Global.hlsl"
#include "Inc/ShadowMap.hlsl"

//[
cbuffer	cbObject	: register( b10 )
{
	float3		_dUV;
};
//]

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )	{ return _In; }


Texture2D<float4>	_TexSourceImage : register( t10 );

// DEBUG!
TextureCubeArray<float4>	_TexCubemapProbe : register( t64 );


float4	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _dUV.xy;

float	Angle = PI;//_Time.x;
float	AspectRatio = 1280.0 / 720.0;
float3	View = float3( AspectRatio * (2.0 * UV.x - 1.0), 1.0 - 2.0 * UV.y, 1.0 );
float	Z2Distance = length( View );
		View /= Z2Distance;

// float3	Up = float3( 0, 1, 0 );
// float3	Right = -float3( cos( Angle ), 0, sin( Angle ) );
// float3	At = float3( -sin( Angle ), 0, cos( Angle ) );
// 		View = View.x * Right + View.y * Up + View.z * At;

View = mul( float4( View, 0.0 ), _Camera2World ).xyz;

//return float4( View, 0 );


// DEBUG NEIGHBOR PROBES IDS
// if ( false )
// {
// 	float3	AbsView = abs( View );
// 	float	MaxComponent = max( max( AbsView.x, AbsView.y ), AbsView.z );
// 
// 	float3	fXYZ = View / MaxComponent;
// 	float2	fXY = 0.0;
// 	int		FaceIndex = 0;
// 	if ( abs( MaxComponent - AbsView.x ) < 1e-5 )
// 	{	// +X or -X
// 		FaceIndex = View.x > 0.0 ? 0 : 1;
// 		fXY = View.x > 0.0 ? float2( -fXYZ.z, fXYZ.y ) : fXYZ.zy;
// 	}
// 	else if ( abs( MaxComponent - AbsView.y ) < 1e-5 )
// 	{	// +Y or -Y
// 		FaceIndex = View.y > 0.0 ? 2 : 3;
// 		fXY = View.y > 0.0 ? float2( fXYZ.x, -fXYZ.z ) : fXYZ.xz;
// 	}
// 	else // if ( abs( MaxComponent - AbsView.z ) < 1e-5 )
// 	{	// +Z or -Z
// 		FaceIndex = View.z > 0.0 ? 4 : 5;
// 		fXY = View.z > 0.0 ? fXYZ.xy : float2( -fXYZ.x, fXYZ.y );
// 	}
// 
// 	fXY.y = -fXY.y;
// 
// //return float4( fXY, 0, 0 );
// //return float4( 0.5 * (1.0 + fXY), 0, 0 );
// //return FaceIndex == 4 ? float4( 1, 0, 0, 0 ) : 0.0;
// 
// 	uint2	XY = uint2( 128 * 0.5 * (1.0 + fXY) );
// 	uint	ProbeID = _TexCubemapProbe[uint3( XY, 2*6 + FaceIndex )].x;
// 
// 	return float4( (ProbeID & 0xFF) / 255.0, ((ProbeID >> 8) & 0xFF) / 255.0, ((ProbeID >> 16) & 0xFF) / 255.0, 0 );
// }
// 
// if ( false )
// {
// 	float3	AbsView = abs( View );
// 	float	MaxComponent = max( max( AbsView.x, AbsView.y ), AbsView.z );
// 
// 	float3	fXYZ = View / MaxComponent;
// 	float2	fXY = 0.0;
// 	int		FaceIndex = 0;
// 	if ( abs( MaxComponent - AbsView.x ) < 1e-5 )
// 	{	// +X or -X
// 		FaceIndex = View.x > 0.0 ? 0 : 1;
// 		fXY = View.x > 0.0 ? float2( -fXYZ.z, fXYZ.y ) : fXYZ.zy;
// 	}
// 	else if ( abs( MaxComponent - AbsView.y ) < 1e-5 )
// 	{	// +Y or -Y
// 		FaceIndex = View.y > 0.0 ? 2 : 3;
// 		fXY = View.y > 0.0 ? float2( fXYZ.x, -fXYZ.z ) : fXYZ.xz;
// 	}
// 	else // if ( abs( MaxComponent - AbsView.z ) < 1e-5 )
// 	{	// +Z or -Z
// 		FaceIndex = View.z > 0.0 ? 4 : 5;
// 		fXY = View.z > 0.0 ? fXYZ.xy : float2( -fXYZ.x, fXYZ.y );
// 	}
// 
// 	fXY.y = -fXY.y;
// 
// 	uint2	XY = uint2( 128 * 0.5 * (1.0 + fXY) );
// 	uint	ProbeID = _TexCubemapProbe[uint3( XY, 2*6 + FaceIndex )].x;
// 
// 	float3	Colors[7] = {
// 		float3( 0, 0, 0 ),
// 		float3( 1, 0, 0 ),
// 		float3( 0.5, 0, 0 ),
// 		float3( 0, 1, 0 ),
// 		float3( 0, 0.5, 0 ),
// 		float3( 0, 0, 1 ),
// 		float3( 0, 0, 0.5 ),
// 	};
// 	return ProbeID == 0 ? 0.0 : float4( Colors[1+((ProbeID-1)%6)], 0 );
// 
// 	return float4( (ProbeID & 0xFF) / 255.0, ((ProbeID >> 8) & 0xFF) / 255.0, 0, 0 );
// 
// 	return float4( 0.5 * (1.0 + fXY), 0, 0 );
// 	return float4( fXY, 0, 0 );
// 	return float4( Colors[1+FaceIndex], 0 );
// 
// 	return 0.2 * ProbeID;
// }
// DEBUG NEIGHBOR PROBES IDS

if ( true )
{
	if ( UV.x < 0.3 && UV.y < 0.3 )
	{
		UV /= 0.3;
//		return _ShadowMap.SampleLevel( LinearClamp, UV, 0.0 ).x;

		UV.x *= 3.0;
		UV.y *= 2.0;
		float	ArrayIndex = 3 * int( UV.y ) + int( UV.x );
		UV = frac( UV );
		float	Zproj = _ShadowMapPoint.SampleLevel( LinearClamp, float3( UV, ArrayIndex ), 0.0 ).x;
return 1.0 * Zproj;

		const float	NearClip = 0.01;
		const float	FarClip = _ShadowPointFarClip;

		float	Z = NearClip * FarClip / (FarClip - Zproj * (FarClip - NearClip));
		return Z;
	}
}

if ( false )
{
	float3	AbsView = abs( View );
	float	MaxComponent = max( max( AbsView.x, AbsView.y ), AbsView.z );

	float3	fXYZ = View / MaxComponent;
	float3	UV = 0.0;
	if ( abs( MaxComponent - AbsView.x ) < 1e-5 )
	{	// +X or -X
		UV.z = View.x > 0.0 ? 0 : 1;
		UV.xy = View.x < 0.0 ? float2( -fXYZ.z, fXYZ.y ) : fXYZ.zy;
	}
	else if ( abs( MaxComponent - AbsView.y ) < 1e-5 )
	{	// +Y or -Y
		UV.z = View.y > 0.0 ? 2 : 3;
		UV.xy = View.y > 0.0 ? float2( -fXYZ.x, -fXYZ.z ) : float2( -fXYZ.x, fXYZ.z );
	}
	else // if ( abs( MaxComponent - AbsView.z ) < 1e-5 )
	{	// +Z or -Z
		UV.z = View.z > 0.0 ? 4 : 5;
		UV.xy = View.z < 0.0 ? fXYZ.xy : float2( -fXYZ.x, fXYZ.y );
	}

	UV.y = -UV.y;
	UV.xy = 0.5 * (1.0 + UV.xy);

	float	Zproj = _ShadowMapPoint.SampleLevel( LinearClamp, UV, 0.0 ).x;

	const float	NearClip = 0.01;
	const float	FarClip = _ShadowPointFarClip;

	float	Z = NearClip * FarClip / (FarClip - Zproj * (FarClip - NearClip));
	return Z;
}

// Test dot products
// float3 TestNormal = _TexCubemapProbe.Sample( LinearClamp, float4( View, 1.0 ) ).xyz;
// if ( dot( TestNormal, View ) > 0.0 )
// 	return float4( 1, 0, 0, 1 );
// return -dot( TestNormal, View );


//return _TexCubemapProbe.Sample( LinearClamp, float4( View, 2.0 ) );
//return asuint( _TexCubemapProbe.Sample( PointClamp, float4( View, 2.0 ) ).w );
//return (asuint( _TexCubemapProbe.Sample( PointClamp, float4( View, 0.0 ) ).w ) & 0xFF) / 255.0;

	return _TexSourceImage.SampleLevel( LinearClamp, UV, 0.0 );
	return float4( _In.__Position.xy * _dUV.xy, 0, 0 );
}
