//////////////////////////////////////////////////////////////////////////
// 
//
#include "Inc/Global.hlsl"

static const int	PARTICLES_COUNT = 64;
static const int	PARTICLES_MASK = PARTICLES_COUNT-1;
static const int	PARTICLES_SHIFT = 6;


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
// 	uint	Index	: SV_INSTANCEID;
// };

struct	VS_IN
{
	float4	__Position		: SV_POSITION;
	uint	Index	: SV_VERTEXID;
};

struct	GS_IN
{
	float3	Position		: POSITION;
	float3	Normal			: NORMAL;
	float3	Tangent			: TANGENT;
	float3	BiTangent		: BITANGENT;
	float4	UV				: TEXCOORD0;
	uint	Index			: PARTICLE_INDEX;
	float	Life			: LIFE;
	float	Size			: SIZE;
};

struct	PS_IN
{
	float4	__Position		: SV_POSITION;
	float2	UV				: TEXCOORD0;
	float3	Position		: POSITION;			// World position
	uint	Index			: PARTICLE_INDEX;
	float3	Normal			: NORMAL;			// World normal
	float	Life			: LIFE;
};

GS_IN	VS( VS_IN _In )
{
	// Isolate particle in the texture
	uint	ParticleY = _In.Index >> PARTICLES_SHIFT;
	uint	ParticleX = _In.Index & PARTICLES_MASK;
	float2	UV = float2( ParticleX, ParticleY ) / PARTICLES_COUNT;

	// Retrieve position & normal
	float4	PositionLife = _TexParticlePositions.SampleLevel( PointClamp, UV, 0.0 );
	float4	NormalSize = _TexParticleNormals.SampleLevel( PointClamp, UV, 0.0 );
	float3	Normal = NormalSize.xyz;
	float	Size = NormalSize.w;
	float3	Tangent = _TexParticleTangents.SampleLevel( PointClamp, UV, 0.0 ).xyz;
	float3	BiTangent = normalize( cross( Normal, Tangent ) );

	// We now have all the necessary informations
 	GS_IN	Out;
	Out.Position = PositionLife.xyz;
	Out.Normal = Normal;
	Out.Tangent = Tangent;
	Out.BiTangent = BiTangent;
	Out.UV = _In.__Position;	// Here the position of the vertex actually contains the min/max UV coordinates
	Out.Index = _In.Index;
	Out.Life = PositionLife.w;
	Out.Size = Size;

	return Out;
}

[maxvertexcount( 4 )]
void	GS( point GS_IN _In[1], inout TriangleStream<PS_IN> _OutStream )
{
	PS_IN	Out;
	Out.Position = _In[0].Position;
	Out.Index = _In[0].Index;
	Out.Normal = _In[0].Normal;
	Out.Life = _In[0].Life;

	float2	DeltaUV = _In[0].UV.zw - _In[0].UV.xy;	// Size in UV space

//float2	CenterUV = _In[0].UV.xy + 0.5 * DeltaUV;
// float2	CenterUV = 0.5 * (_In[0].UV.xy + _In[0].UV.zw);
// _In[0].UV.xy = CenterUV - 0.5 * DeltaUV;
// _In[0].UV.zw = CenterUV + 0.5 * DeltaUV;

	float2	Size = _In[0].Size * DeltaUV;			// Size in WORLD space

	// Expand the 4 vertices of the particle quad from the center position
	float3	WorldPosition = _In[0].Position - Size.x * _In[0].Tangent - Size.y * _In[0].BiTangent;
 	Out.__Position = mul( float4( WorldPosition, 1.0 ), _World2Proj );	// Top-left corner
	Out.UV = _In[0].UV.xy;
	_OutStream.Append( Out );

	WorldPosition = _In[0].Position - Size.x * _In[0].Tangent + Size.y * _In[0].BiTangent;
 	Out.__Position = mul( float4( WorldPosition, 1.0 ), _World2Proj );	// Bottom-left corner
	Out.UV = _In[0].UV.xw;
	_OutStream.Append( Out );

	WorldPosition = _In[0].Position + Size.x * _In[0].Tangent - Size.y * _In[0].BiTangent;
 	Out.__Position = mul( float4( WorldPosition, 1.0 ), _World2Proj );	// Top-right corner
	Out.UV = _In[0].UV.zy;
	_OutStream.Append( Out );

	WorldPosition = _In[0].Position + Size.x * _In[0].Tangent + Size.y * _In[0].BiTangent;
 	Out.__Position = mul( float4( WorldPosition, 1.0 ), _World2Proj );	// Bottom-right corner
	Out.UV = _In[0].UV.zw;
	_OutStream.Append( Out );
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
//	return float4( _In.UV, 0, 1 );
	float2	ParticleInfos = _TexParticleCells.SampleLevel( PointWrap, _In.UV, 0.0 ).xy;
	clip( 0.0 - 1.0 * abs( ParticleInfos.x - _In.Index ) );	// Discard pixels not part of the current particle

// 	if ( abs( ParticleInfos.x - _In.Index ) > 1e-3 )
// 		return 0.0;

// 	float3	Colors[] = {
// 		float3( 1, 0, 0 ),
// 		float3( 1, 1, 0 ),
// 		float3( 1, 0, 1 ),
// 		float3( 0, 1, 0 ),
// 		float3( 1, 1, 0 ),
// 		float3( 0, 1, 1 ),
// 		float3( 0, 0, 1 ),
// 		float3( 1, 0, 1 ),
// 		float3( 0, 1, 1 ),
// 		float3( 1, 1, 1 ),
// 	};
//	return float4( Colors[ParticleInfos.x%10], 1 );
//	return ParticleInfos.y;	// Show distance to cell center

	// We must animate the color of the particle based on distance to cell center and life...
	// We want to simulate the effect of a tiny speck of paper burning.
	// To achieve this, we split the colors into 4 phases:
	//	1) Normal phase => where the particle has a uniform color
	//	2) Burning phase => The particle starts burning so it takes a yellow/orange color at a distance
	//	3) Ash phase => When the particle has combusted for some time, it starts getting black
	//	4) Crumbling phase => The ash crumbles in particles so tiny they can't be seen, the particle disappears
	//
	float3	ToLight = float3( 0, 1, 0 );
	float4	Color = float4( float3( 0.1, 0.1, 0.1 ) + saturate( dot( ToLight, _In.Normal ) ) * float3( 0.7, 0.7, 0.7 ), 1.0 );

	static const float	START_BURNING = -0.1;
	static const float	START_ASH = 3.0;
	static const float	END_ASH = 7.0;

	float	Life = _In.Life + ParticleInfos.y;	// Make the distant parts of the particle evolve faster
	if ( Life > START_ASH )
	{	// Ash phase
		float	t = 1.0 * (Life - START_ASH) / (END_ASH - START_ASH);
		Color = float4( 0, 0, 0, 1.0 - t );	// Slowly become transparent
	}
	else if ( Life > START_BURNING )
	{	// Burn phase
		float	t = 5.0 * (Life - START_BURNING) / (START_ASH - START_BURNING);
		float4	BurnColors[] = {
			float4( 10, 10, 0.5, 2 ),		// Bright yellow (Alpha is > 1 so we blend with current color)
			float4( 8, 5, 0.1, 1 ),			// Orange
			float4( 5, 2, 0.1, 1 ),			// Red
			float4( 1, 0.2, 0.05, 1 ),		// Darker red
			float4( 0.5, 0.1, 0.025, 1 ),	// Even darker red
			float4( 0.1, 0.01, 0.001, 1 ),	// Almost black
		};
		int		BurnColorIndex = int( floor( t ) );
		t -= BurnColorIndex;
		float4	CurrentBurnColor = lerp( BurnColors[BurnColorIndex], BurnColors[BurnColorIndex+1], t );
		Color = lerp( Color, CurrentBurnColor, 2.0 - CurrentBurnColor.w );
	}

	clip( Color.w - 0.1 );	// Make the ash disppear...
	return Color;
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
