#include "Global.hlsl"

static const float	NODE_SIZE = 0.005;
static const float	LINK_SIZE = 0.0015;

StructuredBuffer<SB_NodeInfo>	_SB_Nodes : register( t0 );
StructuredBuffer<uint>			_SB_Links : register( t1 );
StructuredBuffer<uint>			_SB_LinkSources : register( t2 );

//Texture2D<float3>	_tex_FalseColors : register( t4 );

struct VS_IN {
	float4	__Position : SV_POSITION;
	uint	instanceID : SV_INSTANCEID;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	UV : TEXCOORDS0;
};

///////////////////////////////////////////////////////////////////////////////////////
// NODE DRAWING
//
PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	uint		nodeIndex = _In.instanceID;
	SB_NodeInfo	node = _SB_Nodes[nodeIndex];

//	const float	SIZE = 0.1;
	const float	SIZE = NODE_SIZE * dot( 0.5, _cameraSize );

	float2	position = node.m_position + SIZE * _In.__Position.xy;

	Out.__Position = TransformPosition( position );

	Out.UV.xy = _In.__Position.xy;
	Out.UV.z = 1.0;

	if ( nodeIndex == _hoveredNodeIndex ) {
		Out.UV.z = -1;	// Hovered
	} else if ( node.m_flags & 7U ) {
		Out.UV.z = -2;	// Selected
	}

	return Out;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {
	float	sqDistance = 1.0 - dot( _In.UV.xy, _In.UV.xy );
	clip( sqDistance );

	if ( _In.UV.z < -1.5 )
		return float3( 1, 1, 1 );
	else if ( _In.UV.z < -0.5 )
		return float3( 1, 1, 0 );

//	return sqrt( sqDistance ) * _tex_FalseColors.SampleLevel( LinearClamp, float2( lerp( 0.2, 1.0, _In.UV.z ), 0.5 ), 0.0 );
	return sqrt( sqDistance );
}


///////////////////////////////////////////////////////////////////////////////////////
// LINK DRAWING
//
PS_IN	VS2( VS_IN _In ) {
	PS_IN	Out;

	uint		linkIndex = _In.instanceID;
	uint		souceIndex = _SB_LinkSources[linkIndex];
	uint		targetIndex = _SB_Links[linkIndex];
	SB_NodeInfo	sourceNode = _SB_Nodes[souceIndex];
	SB_NodeInfo	targetNode = _SB_Nodes[targetIndex];

	const float	SIZE = LINK_SIZE * dot( 0.5, _cameraSize );

	float2	P0 = sourceNode.m_position;
	float2	P1 = targetNode.m_position;

	float2	dir = P1 - P0;
	float2	ortho = SIZE * normalize( float2( -dir.y, dir.x ) );

	float	U = 0.5 * (1.0 + _In.__Position.x);
	float2	finalPos = P0 + U * dir +  _In.__Position.y * ortho;

	Out.__Position = TransformPosition( finalPos );

	Out.UV.xy = _In.__Position.xy;
	Out.UV.z = 1;//saturate( lerp( sourceNode.m_mass, targetNode.m_mass, U ) / _maxMass );

	Out.__Position.z = 1.0 - Out.UV.z;

	if ( (sourceNode.m_flags & 7U) && (targetNode.m_flags & 7U) ) {
		Out.UV.z = (sourceNode.m_flags & 4U) ? -3 : ((targetNode.m_flags & 2U) ? -2 : -1);
		Out.__Position.z = 0.0;
	}

	return Out;
}

float3	PS2( PS_IN _In ) : SV_TARGET0 {
	if ( _In.UV.z < -2.5 )
		return 0.9 * float3( 0, 1, 1 );		// Child selection
	if ( _In.UV.z < -1.5 )
		return 0.4 * float3( 1, 1, 1 );		// Hierarchy selection
	else if ( _In.UV.z < -0.5 )
		return 0.9 * float3( 1, 1, 1 );		// Actual selection

//	return 0.5 * sqrt( 1.0 - pow2( _In.UV.y ) ) * _tex_FalseColors.SampleLevel( LinearClamp, float2( lerp( 0.20, 1.0, _In.UV.z ), 0.5 ), 0.0 );
	return 0.5 * sqrt( 1.0 - pow2( _In.UV.y ) );
}

