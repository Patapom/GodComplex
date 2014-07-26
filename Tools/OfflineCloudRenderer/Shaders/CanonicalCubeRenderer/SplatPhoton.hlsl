////////////////////////////////////////////////////////////////////////////////
// Splats the photons into the cube texture
////////////////////////////////////////////////////////////////////////////////
#include "../Global.hlsl"
#include "../Noise.hlsl"

cbuffer	cbRender : register(b8)
{
	float		_SplatSize;
	float		_SplatIntensity;
}

struct PhotonOut
{
	float3	ExitPosition;			// Exit position in [-1,+1]
	float3	ExitDirection;			// Exit direction
	float	MarchedLength;			// Length the photon had to march before exiting (in canonical [-1,+1] units, multiply by 0.5*CubeSize to get length in meters)
	uint	ScatteringEventsCount;	// Amount of scattering events before exiting
};

StructuredBuffer<PhotonOut>	_Photons : register( t0 );

struct VS_IN
{
	float3	Position : POSITION;
	uint	PhotonIndex : SV_INSTANCEID;
};

struct PS_IN
{
	float4	__Position : SV_POSITION;
	uint	CubeFaceIndex	: SV_RENDERTARGETARRAYINDEX;
	float2	UV : TEXCOORD0;
	float4	Data0 : DATA0;
	float4	Data1 : DATA1;
};

struct PS_OUT
{
	float4	Data0 : SV_TARGET0;
	float4	Data1 : SV_TARGET1;
};

VS_IN	VS( VS_IN _In )
{
	return _In;
}

StructuredBuffer<float>	_PhaseQuantiles : register( t1 );

[maxvertexcount( 4 )]
void	GS( point VS_IN _In[1], inout TriangleStream<PS_IN> _OutStream )
{
	PS_IN	Out;

	PhotonOut	Photon = _Photons[_In[0].PhotonIndex];

	Out.Data0 = float4( Photon.ExitPosition, Photon.MarchedLength );
	Out.Data1 = float4( Photon.ExitDirection, Photon.ScatteringEventsCount );

	// Determine where to splat the photon
	const float	eps = 1e-3;

	float4	P = float4( 0, 0, 0, 1 );
	Out.CubeFaceIndex = 0;
	if ( Photon.ExitPosition.x >= 1.0-eps )
	{
		Out.CubeFaceIndex = 0;
		P.xy = float2( -Photon.ExitPosition.z, Photon.ExitPosition.y );
	}
	else if ( Photon.ExitPosition.x <= -1.0+eps )
	{
		Out.CubeFaceIndex = 1;
		P.xy = float2( Photon.ExitPosition.z, Photon.ExitPosition.y );
	}
	else if ( Photon.ExitPosition.y >= 1.0-eps )
	{
		Out.CubeFaceIndex = 2;
		P.xy = float2( Photon.ExitPosition.x, -Photon.ExitPosition.z );
	}
	else if ( Photon.ExitPosition.y <= -1.0+eps )
	{
		Out.CubeFaceIndex = 3;
		P.xy = float2( Photon.ExitPosition.x, Photon.ExitPosition.z );
	}
	else if ( Photon.ExitPosition.z >= 1.0-eps )
	{
		Out.CubeFaceIndex = 4;
		P.xy = float2( Photon.ExitPosition.x, Photon.ExitPosition.y );
	}
	else if ( Photon.ExitPosition.z <= -1.0+eps )
	{
		Out.CubeFaceIndex = 5;
		P.xy = float2( -Photon.ExitPosition.x, Photon.ExitPosition.y );
	}
	else
	{	// Error!
		Out.CubeFaceIndex = 2;
		P.xy = 2*float2( Hash( 0.3789161 * (2*_In[0].PhotonIndex) ), Hash( 0.2194 * (2*_In[0].PhotonIndex+1) ) )-1;

Out.Data0 = float4( 1, 0, 1, 0 );
Out.Data1 = float4( 1, 0, 1, 0 );
	}


// Out.CubeFaceIndex = 6 * Hash( _In[0].PhotonIndex );
// P = float4( 2.0*Hash( 18.091 * _In[0].PhotonIndex )-1.0, 2.0*Hash( 37.351 * _In[0].PhotonIndex )-1.0, 0, 1 );


	// Stream out the 4 vertices for the splat quad
	Out.UV = float2( -1, 1 );
	Out.__Position = P + float4( _SplatSize * Out.UV, 0, 0 );
	_OutStream.Append( Out );

	Out.UV = float2( -1, -1 );
	Out.__Position = P + float4( _SplatSize * Out.UV, 0, 0 );
	_OutStream.Append( Out );

	Out.UV = float2( 1, 1 );
	Out.__Position = P + float4( _SplatSize * Out.UV, 0, 0 );
	_OutStream.Append( Out );

	Out.UV = float2( 1, -1 );
	Out.__Position = P + float4( _SplatSize * Out.UV, 0, 0 );
	_OutStream.Append( Out );
}

PS_OUT	PS( PS_IN _In )
{
	PS_OUT	Out;
	Out.Data0 = _In.Data0;
	Out.Data1 = _In.Data1;
	return Out;
}

float4	PS_Intensity( PS_IN _In ) : SV_TARGET0
{
	return _SplatIntensity * exp( -10.0 * length( _In.UV ) );
}
