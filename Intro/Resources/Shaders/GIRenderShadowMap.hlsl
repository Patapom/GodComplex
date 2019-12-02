//////////////////////////////////////////////////////////////////////////
// The classical shadow map renderer
//
#include "Inc/Global.hlsl"
#include "Inc/ShadowMap.hlsl"

cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
};

// Scene vertex format
struct	VS_IN
{
	float3	Position	: POSITION;
// 	float3	Normal		: NORMAL;
// 	float3	Tangent		: TANGENT;
// 	float3	BiTangent	: BITANGENT;
// 	float2	UV			: TEXCOORD0;
// 	uint	ProbeID		: INFO;
};

struct	GS_IN
{
	float3	Position	: POSITION;	// "Local position" (i.e. relative to the light's position)
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
};

struct	PS_IN2
{
	float4	__Position		: SV_POSITION;
	uint	CubeFaceIndex	: SV_RENDERTARGETARRAYINDEX;
};


///////////////////////////////////////////////////////////
// Directional Shadow Map rendering
PS_IN	VS( VS_IN _In )
{
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );

	PS_IN	Out;
	Out.__Position = World2ShadowMapProj( WorldPosition.xyz );

	return Out;
}


///////////////////////////////////////////////////////////
// Point-Light Shadow Map rendering
GS_IN	VS2( VS_IN _In )
{
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );

	GS_IN	Out;
	Out.Position = WorldPosition.xyz - _ShadowPointLightPosition;

	return Out;
}

static const float3	Xp = float3( 1, 0, 0 );
static const float3	Xn = float3( -1, 0, 0 );
static const float3	Yp = float3( 0, 1, 0 );
static const float3	Yn = float3( 0, -1, 0 );
static const float3	Zp = float3( 0, 0, 1 );
static const float3	Zn = float3( 0, 0, -1 );

static const float	NearClip = 0.5;
static const float	FarClip = _ShadowPointFarClip;
static const float	Q = FarClip / (FarClip - NearClip);

float4	World2CubeFaceProj( float3 _LocalPosition, float3 _Right, float3 _Up, float3 _At )
{
	float3	CubeFacePos = float3( 
		dot( _LocalPosition, _Right ),
		dot( _LocalPosition, _Up ),
		dot( _LocalPosition, _At ) );

	return float4( CubeFacePos.xy, Q * (CubeFacePos.z - NearClip), CubeFacePos.z );
}

[maxvertexcount( 18 )]
void	GS( triangle GS_IN _In[3], inout TriangleStream<PS_IN2> _OutStream )
{
	PS_IN2	Out;

	// Write +X face
	{
		float3	X = Zp;
		float3	Y = Yp;
		float3	Z = Xp;
		Out.CubeFaceIndex = 0;

		Out.__Position = World2CubeFaceProj( _In[0].Position, X, Y, Z );	_OutStream.Append( Out );
		Out.__Position = World2CubeFaceProj( _In[1].Position, X, Y, Z );	_OutStream.Append( Out );
		Out.__Position = World2CubeFaceProj( _In[2].Position, X, Y, Z );	_OutStream.Append( Out );
		_OutStream.RestartStrip();
	}

	// Write -X face
	{
		float3	X = Zn;
		float3	Y = Yp;
		float3	Z = Xn;
		Out.CubeFaceIndex = 1;

		Out.__Position = World2CubeFaceProj( _In[0].Position, X, Y, Z );	_OutStream.Append( Out );
		Out.__Position = World2CubeFaceProj( _In[1].Position, X, Y, Z );	_OutStream.Append( Out );
		Out.__Position = World2CubeFaceProj( _In[2].Position, X, Y, Z );	_OutStream.Append( Out );
		_OutStream.RestartStrip();
	}

	// Write +Y face
	{
		float3	X = Xn;
		float3	Y = Zn;
		float3	Z = Yp;
		Out.CubeFaceIndex = 2;

		Out.__Position = World2CubeFaceProj( _In[0].Position, X, Y, Z );	_OutStream.Append( Out );
		Out.__Position = World2CubeFaceProj( _In[1].Position, X, Y, Z );	_OutStream.Append( Out );
		Out.__Position = World2CubeFaceProj( _In[2].Position, X, Y, Z );	_OutStream.Append( Out );
		_OutStream.RestartStrip();
	}
	// Write -Y face
	{
		float3	X = Xn;
		float3	Y = Zp;
		float3	Z = Yn;
		Out.CubeFaceIndex = 3;

		Out.__Position = World2CubeFaceProj( _In[0].Position, X, Y, Z );	_OutStream.Append( Out );
		Out.__Position = World2CubeFaceProj( _In[1].Position, X, Y, Z );	_OutStream.Append( Out );
		Out.__Position = World2CubeFaceProj( _In[2].Position, X, Y, Z );	_OutStream.Append( Out );
		_OutStream.RestartStrip();
	}

	// Write +Z face
	{
		float3	X = Xn;
		float3	Y = Yp;
		float3	Z = Zp;
		Out.CubeFaceIndex = 4;

		Out.__Position = World2CubeFaceProj( _In[0].Position, X, Y, Z );	_OutStream.Append( Out );
		Out.__Position = World2CubeFaceProj( _In[1].Position, X, Y, Z );	_OutStream.Append( Out );
		Out.__Position = World2CubeFaceProj( _In[2].Position, X, Y, Z );	_OutStream.Append( Out );
		_OutStream.RestartStrip();
	}
	// Write -Z face
	{
		float3	X = Xp;
		float3	Y = Yp;
		float3	Z = Zn;
		Out.CubeFaceIndex = 5;

		Out.__Position = World2CubeFaceProj( _In[0].Position, X, Y, Z );	_OutStream.Append( Out );
		Out.__Position = World2CubeFaceProj( _In[1].Position, X, Y, Z );	_OutStream.Append( Out );
		Out.__Position = World2CubeFaceProj( _In[2].Position, X, Y, Z );	_OutStream.Append( Out );
		_OutStream.RestartStrip();
	}
}
