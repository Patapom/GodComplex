//////////////////////////////////////////////////////////////////////////
// 
//
#include "Inc/Global.fx"

static const int	PARTICLES_COUNT = 16;
static const int	PARTICLES_MASK = PARTICLES_COUNT-1;
static const int	PARTICLES_SHIFT = 4;


Texture2D	_TexParticlePositions	: register(t10);
Texture2D	_TexParticleNormals		: register(t11);
Texture2D	_TexParticleTangents	: register(t12);
Texture2D	_TexParticleCells		: register(t13);

//[
cbuffer	cbRender	: register( b10 )
{
};
//]

// struct	VS_IN
// {
// 	float3	Position		: POSITION;
// 	uint	ParticleIndex	: SV_INSTANCEID;
// };

struct	VS_IN
{
	float4	__Position		: SV_POSITION;
	uint	ParticleIndex	: SV_VERTEXID;
};

struct	GS_IN
{
	float3	Position		: POSITION;
	float3	Tangent			: TANGENT;
	float3	BiTangent		: BITANGENT;
	float4	UV				: TEXCOORD0;
	uint	ParticleIndex	: PARTICLE_INDEX;
	float	Life			: LIFE;
};

struct	PS_IN
{
	float4	__Position		: SV_POSITION;
	float2	UV				: TEXCOORD0;
	uint	ParticleIndex	: PARTICLE_INDEX;
	float	Life			: LIFE;
};

GS_IN	VS( VS_IN _In )
{
	// Isolate particle in the texture
	uint	ParticleY = _In.ParticleIndex >> PARTICLES_SHIFT;
	uint	ParticleX = _In.ParticleIndex & PARTICLES_MASK;
	float2	UV = float2( ParticleX, ParticleY ) / PARTICLES_COUNT;

	// Retrieve position & normal
	float4	Position = _TexParticlePositions.SampleLevel( PointClamp, UV, 0.0 );
	float3	Normal = _TexParticleNormals.SampleLevel( PointClamp, UV, 0.0 ).xyz;
	float3	Tangent = _TexParticleTangents.SampleLevel( PointClamp, UV, 0.0 ).xyz;

	// Build remaining tangent space from an arbitrary Up vector
// 	float3	Up = float3( 0, 1, -0.01 );
// 	float3	Tangent = normalize( cross( Up, Normal ) );
	float3	BiTangent = cross( Normal, Tangent );

	// We now have all the necessary informations
 	GS_IN	Out;
	Out.Position = Position.xyz;
	Out.Tangent = Tangent;
	Out.BiTangent = BiTangent;
	Out.UV = _In.__Position;	// Here the position of the vertex actually contains the min/max UV coordinates
	Out.ParticleIndex = _In.ParticleIndex;
	Out.Life = Position.w;

	return Out;
}

[maxvertexcount( 4 )]
void	GS( point GS_IN _In[1], inout TriangleStream<PS_IN> _OutStream )
{
	PS_IN	Out;
	Out.ParticleIndex = _In[0].ParticleIndex;
	Out.Life = _In[0].Life;

//	float	ParticleSize = 0.025;
	float	ParticleSize = 0.1;

	// Expand the 4 vertices of the particle quad from the center position
	float3	WorldPosition = _In[0].Position + ParticleSize * (-_In[0].Tangent + _In[0].BiTangent);
 	Out.__Position = mul( float4( WorldPosition, 1.0 ), _World2Proj );	// Top-left corner
	Out.UV = _In[0].UV.xy;
	_OutStream.Append( Out );

	WorldPosition = _In[0].Position + ParticleSize * (-_In[0].Tangent - _In[0].BiTangent);
 	Out.__Position = mul( float4( WorldPosition, 1.0 ), _World2Proj );	// Bottom-left corner
	Out.UV = _In[0].UV.xw;
	_OutStream.Append( Out );

	WorldPosition = _In[0].Position + ParticleSize * (+_In[0].Tangent + _In[0].BiTangent);
 	Out.__Position = mul( float4( WorldPosition, 1.0 ), _World2Proj );	// Top-right corner
	Out.UV = _In[0].UV.zy;
	_OutStream.Append( Out );

	WorldPosition = _In[0].Position + ParticleSize * (+_In[0].Tangent - _In[0].BiTangent);
 	Out.__Position = mul( float4( WorldPosition, 1.0 ), _World2Proj );	// Bottom-right corner
	Out.UV = _In[0].UV.zw;
	_OutStream.Append( Out );
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
//	return float4( _In.UV, 0, 1 );
	float2	ParticleInfos = _TexParticleCells.SampleLevel( PointWrap, _In.UV, 0.0 ).xy;
	clip( -abs( ParticleInfos.x - _In.ParticleIndex ) );	// Discard pixels not part of the current particle

	float3	Colors[] = {
		float3( 1, 0, 0 ),
		float3( 1, 1, 0 ),
		float3( 1, 0, 1 ),
		float3( 0, 1, 0 ),
		float3( 1, 1, 0 ),
		float3( 0, 1, 1 ),
		float3( 0, 0, 1 ),
		float3( 1, 0, 1 ),
		float3( 0, 1, 1 ),
		float3( 1, 1, 1 ),
	};
	return float4( Colors[ParticleInfos.x%10], 1 );
	return ParticleInfos.y;
}

///////////////////////////////////////////////////////////////////////////////////////////
struct	PS_IN_DEBUG
{
	float4	__Position : SV_POSITION;
	float2	UV : TEXCOORDS0;
};

PS_IN_DEBUG	VS_DEBUG( VS_IN _In )
{
	PS_IN_DEBUG	Out;
	Out.__Position = _In.__Position;
	Out.UV = 0.5 * (1.0 + _In.__Position.xy);
	return Out;
}
float4	PS_DEBUG( PS_IN_DEBUG _In ) : SV_TARGET0
{
	float3	Colors[] = {
		float3( 1, 0, 0 ),
		float3( 1, 1, 0 ),
		float3( 1, 0, 1 ),
		float3( 0, 1, 0 ),
		float3( 1, 1, 0 ),
		float3( 0, 1, 1 ),
		float3( 0, 0, 1 ),
		float3( 1, 0, 1 ),
		float3( 0, 1, 1 ),
		float3( 1, 1, 1 ),
	};
	return float4( Colors[_TexParticlePositions.SampleLevel( PointWrap, _In.UV, 0.0 ).x % 10], 1 );
}
