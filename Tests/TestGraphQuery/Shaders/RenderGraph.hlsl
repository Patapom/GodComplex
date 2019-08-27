#include "Global.hlsl"

static const float	NODE_SIZE = 0.005;
static const float	LINK_SIZE = 0.0015;

StructuredBuffer<SB_NodeInfo>	_SB_Nodes : register( t0 );
StructuredBuffer<uint>			_SB_Links : register( t1 );
StructuredBuffer<uint>			_SB_LinkSources : register( t2 );

// Heat simulation
StructuredBuffer<float>			_SB_Heat : register( t3 );

Texture2D<float3>	_tex_FalseColors : register( t4 );

struct VS_IN {
	float4	__Position : SV_POSITION;
	uint	instanceID : SV_INSTANCEID;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	UV : TEXCOORDS0;
	float3	color : COLOR;
};

///////////////////////////////////////////////////////////////////////////////////////
// NODE DRAWING
//
PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	uint		nodeIndex = _In.instanceID;
	SB_NodeInfo	node = _SB_Nodes[nodeIndex];

	const float	SIZE = NODE_SIZE * dot( 0.5, _cameraSize );

	float2	position = node.m_position + SIZE * _In.__Position.xy;

	Out.__Position = TransformPosition( position );

	Out.UV.xy = _In.__Position.xy;
	Out.UV.z = 1.0;

	if ( nodeIndex == _hoveredNodeIndex ) {
		Out.color  = float3( 1, 1, 0 );	// Hovered
	} else if ( node.m_flags & 7U ) {
		Out.color  = float3( 1, 1, 1 );	// Selected
	} else {
		float	heat = _SB_Heat[Local2GlobalIndex( nodeIndex, _sourceIndex )];
		Out.color = _tex_FalseColors.SampleLevel( LinearClamp, float2( lerp( 0.2, 1.0, heat ), 0.5 ), 0.0 );
	}

	return Out;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {
	float	sqDistance = 1.0 - dot( _In.UV.xy, _In.UV.xy );
	clip( sqDistance );
	return sqrt( sqDistance ) * _In.color;
}


///////////////////////////////////////////////////////////////////////////////////////
// LINK DRAWING
//
PS_IN	VS2( VS_IN _In ) {
	PS_IN	Out;

	uint		linkIndex = _In.instanceID;
	uint		sourceNodeIndex = _SB_LinkSources[linkIndex];
	uint		targetNodeIndex = _SB_Links[linkIndex];
	SB_NodeInfo	sourceNode = _SB_Nodes[sourceNodeIndex];
	SB_NodeInfo	targetNode = _SB_Nodes[targetNodeIndex];

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

	Out.color = 0.0;
	if ( (sourceNode.m_flags & 7U) && (targetNode.m_flags & 7U) ) {
		Out.__Position.z = 0.0;	// Draw in front

		if ( sourceNode.m_flags & 4U )
			Out.color = 0.9 * float3( 0, 1, 1 );		// Child selection
		else if ( sourceNode.m_flags & 2U )
			Out.color = 0.4 * float3( 1, 1, 1 );		// Hierarchy selection
		else if ( sourceNode.m_flags & 1U )
			Out.color = 0.9 * float3( 1, 1, 1 );		// Actual selection
	} else {
		// Draw heat gradient
		uint	nodeIndex = U < 0.5 ? sourceNodeIndex : targetNodeIndex;
		float	heat = _SB_Heat[Local2GlobalIndex( nodeIndex, _sourceIndex )];
		Out.color =_tex_FalseColors.SampleLevel( LinearClamp, float2( lerp( 0.20, 1.0, heat ), 0.5 ), 0.0 );
	}

	return Out;
}

float3	PS2( PS_IN _In ) : SV_TARGET0 {
	return 0.5 * sqrt( 1.0 - pow2( _In.UV.y ) ) * _In.color;
}

