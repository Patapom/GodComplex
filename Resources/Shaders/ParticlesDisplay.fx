//////////////////////////////////////////////////////////////////////////
// 
//
#include "Inc/Global.fx"

Texture2D	_TexParticles	: register(t10);

//[
cbuffer	cbRender	: register( b10 )
{
};
//]

struct	VS_IN
{
	float3	Position		: POSITION;
	uint	ParticleIndex	: SV_INSTANCEID;
};

struct	GS_IN
{
	float4	__Position	: SV_POSITION;
	float4	Size		: SCREEN_SIZE;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float2	UV			: TEXCOORD0;
};

GS_IN	VS( VS_IN _In )
{
	uint	ParticleY = _In.ParticleIndex >> 9;
	uint	ParticleX = _In.ParticleIndex & 511;
	float2	UV = float2( ParticleX, ParticleY ) / 512.0;

	float3	Position = _TexParticles.SampleLevel( PointClamp, UV, 0.0 ).xyz;

	float3	CameraPosition = _Camera2World[3].xyz;
	float	Distance2Camera = length( Position - CameraPosition );

	GS_IN	Out;
	Out.__Position = mul( float4( Position, 1.0 ), _World2Proj );
	Out.Size.xy = 0.025 / (Distance2Camera * _CameraData.y) * float2( -INV_ASPECT_RATIO, 1.0 );
	Out.Size.zw = 2.0 * float2( -Out.Size.x, Out.Size.y );

	return Out;
}

[maxvertexcount( 4 )]
void	GS( point GS_IN _In[1], inout TriangleStream<PS_IN> _OutStream )
{
	if ( _In[0].__Position.z < 0.0 )
		return;	// Cull if behind camera...

	float4	Size = _In[0].Size;

	PS_IN	Out;
	Out.__Position = _In[0].__Position + float4( Size.xy, 0.0, 0.0 );	// Top-left corner
	Out.UV = float2( 0, 0 );
	_OutStream.Append( Out );

	Out.__Position.y -= Size.w;											// Bottom-left corner
	Out.UV = float2( 0, 1 );
	_OutStream.Append( Out );

	Out.__Position.xy += Size.zw;										// Top-right corner
	Out.UV = float2( 1, 0 );
	_OutStream.Append( Out );

	Out.__Position.y -= Size.w;											// Bottom-right corner
	Out.UV = float2( 1, 1 );
	_OutStream.Append( Out );
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
	return float4( _In.UV, 0, 1 );
}
