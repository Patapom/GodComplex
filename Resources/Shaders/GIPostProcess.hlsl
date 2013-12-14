//////////////////////////////////////////////////////////////////////////
// This shader post-processes the Global Illumination test room
//
#include "Inc/Global.hlsl"

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
TextureCube<float4>		_TexCubemapProbe0 : register( t64 );
TextureCube<float4>		_TexCubemapProbe1 : register( t65 );
Texture2DArray<uint>	_TexCubemapProbe2 : register( t66 );


float4	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _dUV.xy;

float	Angle = PI;//_Time.x;
float	AspectRatio = 1280.0 / 720.0;
float3	View = normalize( float3( AspectRatio * (2.0 * UV.x - 1.0), 1.0 - 2.0 * UV.y, 1.0 ) );
// float3	Up = float3( 0, 1, 0 );
// float3	Right = -float3( cos( Angle ), 0, sin( Angle ) );
// float3	At = float3( -sin( Angle ), 0, cos( Angle ) );
// 		View = View.x * Right + View.y * Up + View.z * At;

View = mul( float4( View, 0.0 ), _Camera2World ).xyz;

//return float4( View, 0 );


// DEBUG NEIGHBOR PROBES IDS
{
	float3	AbsView = abs( View );
	float	MaxComponent = max( max( AbsView.x, AbsView.y ), AbsView.z );

	float3	fXYZ = View / MaxComponent;
	float2	fXY = 0.0;
	int		FaceIndex = 0;
	if ( abs( MaxComponent - AbsView.x ) < 1e-5 )
	{	// +X or -X
		FaceIndex = View.x > 0.0 ? 0 : 1;
		fXY = View.x > 0.0 ? fXYZ.zy : float2( -fXYZ.z, fXYZ.y );
	}
	else if ( abs( MaxComponent - AbsView.y ) < 1e-5 )
	{	// +Y or -Y
		FaceIndex = View.y > 0.0 ? 2 : 3;
		fXY = View.y > 0.0 ? float2( fXYZ.x, -fXYZ.z ) : fXYZ.xz;
	}
	else // if ( abs( MaxComponent - AbsView.z ) < 1e-5 )
	{	// +Z or -Z
		FaceIndex = View.z > 0.0 ? 4 : 5;
		fXY = View.z > 0.0 ? float2( -fXYZ.x, fXYZ.y ) : fXYZ.xy;
	}

	uint2	XY = uint2( 128 * 0.5 * (1.0 + fXY) );
	uint	ProbeID = _TexCubemapProbe2[uint3( XY, FaceIndex )].x;

	float3	Colors[7] = {
		float3( 0, 0, 0 ),
		float3( 1, 0, 0 ),
		float3( 0.5, 0, 0 ),
		float3( 0, 1, 0 ),
		float3( 0, 0.5, 0 ),
		float3( 0, 0, 1 ),
		float3( 0, 0, 0.5 ),
	};
	return float4( Colors[ProbeID], 0 );
	return float4( 0.5 * (1.0 + fXY), 0, 0 );
	return float4( fXY, 0, 0 );
	return float4( Colors[1+FaceIndex], 0 );

	return 0.2 * ProbeID;
}
// DEBUG NEIGHBOR PROBES IDS


return _TexCubemapProbe0.Sample( LinearClamp, View );
//return _TexCubemapProbe1.Sample( LinearClamp, View );
return 0.1 * _TexCubemapProbe1.Sample( LinearClamp, View ).w;

	return _TexSourceImage.SampleLevel( LinearClamp, UV, 0.0 );
	return float4( _In.__Position.xy * _dUV.xy, 0, 0 );
}
