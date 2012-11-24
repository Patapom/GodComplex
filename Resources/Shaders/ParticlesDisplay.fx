//////////////////////////////////////////////////////////////////////////
// 
//
#include "Inc/Global.fx"

Texture2D	_TexParticlePositions	: register(t10);
Texture2D	_TexParticleRotations	: register(t11);

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
	float3	Position	: POSITION;
	float3	Tangent		: TANGENT;
	float3	BiTangent	: BITANGENT;
	float	Life		: LIFE;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float2	UV			: TEXCOORD0;
	float	Life		: LIFE;
};

GS_IN	VS( VS_IN _In )
{
	uint	ParticleY = _In.ParticleIndex >> 9;
	uint	ParticleX = _In.ParticleIndex & 511;
	float2	UV = float2( ParticleX, ParticleY ) / 512.0;

	float4	Position = _TexParticlePositions.SampleLevel( PointClamp, UV, 0.0 );
	float3	Normal = _TexParticleRotations.SampleLevel( PointClamp, UV, 0.0 ).xyz;
	float3	Up = float3( 0, 1, 0 );
	float3	Tangent = normalize( cross( Up, Normal ) );
	float3	BiTangent = cross( Normal, Tangent );

 	GS_IN	Out;
	Out.Position = Position.xyz;
	Out.Tangent = Tangent;
	Out.BiTangent = BiTangent;
	Out.Life = Position.w;

	return Out;
}

[maxvertexcount( 4 )]
void	GS( point GS_IN _In[1], inout TriangleStream<PS_IN> _OutStream )
{
	PS_IN	Out;
	Out.Life = _In[0].Life;

	float	ParticleSize = 0.025;
	float3	WorldPosition = _In[0].Position + ParticleSize * (-_In[0].Tangent + _In[0].BiTangent);
 	Out.__Position = mul( float4( WorldPosition, 1.0 ), _World2Proj );	// Top-left corner
	Out.UV = float2( 0, 0 );
	_OutStream.Append( Out );

	WorldPosition = _In[0].Position + ParticleSize * (-_In[0].Tangent - _In[0].BiTangent);
 	Out.__Position = mul( float4( WorldPosition, 1.0 ), _World2Proj );	// Bottom-left corner
	Out.UV = float2( 0, 1 );
	_OutStream.Append( Out );

	WorldPosition = _In[0].Position + ParticleSize * (+_In[0].Tangent + _In[0].BiTangent);
 	Out.__Position = mul( float4( WorldPosition, 1.0 ), _World2Proj );	// Top-right corner
	Out.UV = float2( 1, 0 );
	_OutStream.Append( Out );

	WorldPosition = _In[0].Position + ParticleSize * (+_In[0].Tangent - _In[0].BiTangent);
 	Out.__Position = mul( float4( WorldPosition, 1.0 ), _World2Proj );	// Bottom-right corner
	Out.UV = float2( 1, 1 );
	_OutStream.Append( Out );
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
	return float4( _In.UV, 0, 1 );
}
