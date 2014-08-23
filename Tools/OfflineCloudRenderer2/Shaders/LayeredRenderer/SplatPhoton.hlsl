////////////////////////////////////////////////////////////////////////////////
// Splats the photons into the 2D texture representing the layer
////////////////////////////////////////////////////////////////////////////////
#include "../Global.hlsl"
#include "../Noise.hlsl"
#include "PhotonStructures.hlsl"

cbuffer	cbRender : register(b8)
{
	float3		_CloudScapeSize;			// Size of the cloud scape covered by the 3D texture of densities
	float		_SplatSize;					// Splat size in NDC space

	float		_SplatIntensity;			// Global intensity multiplier
	uint		_LayerIndex;				// Index of the layer we splat photons to, most significant bit is set to indicate direction (1 is bottom to top, is top to bottom)
}

struct VS_IN
{
	float3	Position : POSITION;
	uint	PhotonIndex : SV_INSTANCEID;
};

struct PS_IN
{
	float4	__Position : SV_POSITION;
	float2	UV : TEXCOORD0;
	float4	Data0 : DATA0;
	float4	Data1 : DATA1;
};

VS_IN	VS( VS_IN _In )
{
	return _In;
}

[maxvertexcount( 4 )]
void	GS( point VS_IN _In[1], inout TriangleStream<PS_IN> _OutStream )
{
	PS_IN	Out;

	uint	LayerIndex = _PhotonLayerIndices[_In.PhotonIndex];
	if ( LayerIndex != _LayerIndex )
		return;	// Photon is not concerned for splatting...

	Photon	Pp = _Photons[_In[0].PhotonIndex];
	PhotonUnpacked	P;
	UnPackPhoton( Pp, P );

	// Determine where to splat the photon
	float3	TopCorner = float3( 0, 0, 0 ) + float3( -0.5 * _CloudScapeSize.x, _CloudScapeSize.y, -0.5 * _CloudScapeSize.z );
	float3	BottomCorner = float3( 0, 0, 0 ) + float3( 0.5 * _CloudScapeSize.x, 0.0, 0.5 * _CloudScapeSize.z );
	float3	UVW = (_Position - TopCorner) / (BottomCorner - TopCorner);

	float4	P = float4( 2.0 * UVW.xy - 1.0, 0, 1 );

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

float4	PS( PS_IN _In ) : SV_TARGET0
{
	return _SplatIntensity * exp( -10.0 * length( _In.UV ) );
}
