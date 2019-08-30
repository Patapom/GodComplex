#include "Global.hlsl"

static const float	NODE_SIZE = 0.005;
static const float	LINK_SIZE = 0.0015;

StructuredBuffer<SB_NodeInfo>	_SB_Nodes : register( t0 );
StructuredBuffer<uint>			_SB_Links : register( t1 );
StructuredBuffer<uint>			_SB_LinkSources : register( t2 );

// Heat simulation
StructuredBuffer<float>			_SB_Heat : register( t3 );
StructuredBuffer<float>			_SB_HeatBarycentrics : register( t4 );

Texture2D<float3>				_tex_FalseColors : register( t5 );

struct VS_IN {
	float4	__Position : SV_POSITION;
	uint	instanceID : SV_INSTANCEID;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	UV : TEXCOORDS0;
	float3	color : COLOR;
};

// Computes the result distance from the isobarycenter
float	ComputeResult( uint _nodeIndex ) {
	const float	barycentricCenter0 = (1.0 - _barycentricBias) / (_sourcesCount - _barycentricBias);	// The ideal center is a vector with all components equal to this value
	const float	barycentricCenter1 = lerp( barycentricCenter0, 1.0, _barycentricBias );

	float	sqDistance = 0.0;
	for ( uint sourceIndex=0; sourceIndex < _sourcesCount; sourceIndex++ ) {
		float	barycentric = _SB_HeatBarycentrics[Local2GlobalIndex( _nodeIndex, sourceIndex )];
		float	delta = barycentric - (sourceIndex == _sourceIndex ? barycentricCenter1 : barycentricCenter0);
		sqDistance += delta * delta;
	}

	return sqrt( sqDistance );
}

float3	ComputeResultColor( uint _nodeIndex ) {
	float	distance2Iso = ComputeResult( _nodeIndex );

	const float	tol = _barycentricDistanceTolerance;

	return distance2Iso < tol ? float3( tol - distance2Iso, 0, 0 ) : 0.5 * float3( 0, 0, 2*tol - distance2Iso ) / tol;
}

// Checks for out of bounds barycentrics
float3	ComputeSumBarycentrics( uint _nodeIndex, inout float _Z ) {
	float	sumBarycentrics = 0.0;
	for ( uint sourceIndex=0; sourceIndex < _sourcesCount; sourceIndex++ ) {
		sumBarycentrics += _SB_HeatBarycentrics[Local2GlobalIndex( _nodeIndex, sourceIndex )];
	}

	const float	MIN = 0.0;
	const float	MAX = 1.0;

#if 1
	if ( sumBarycentrics < MIN ) {
		return float3( 0, 0, MIN - sumBarycentrics );
	} else if ( sumBarycentrics > MAX ) {
		return 0.1 * float3( sumBarycentrics - MAX, 0, 0 );
	} else {
		_Z = 0.0;	// Bring front
		return _tex_FalseColors.SampleLevel( LinearClamp, float2( sumBarycentrics, 0.5 ), 0.0 );
//		return sumBarycentrics;
	}
#else
	return sumBarycentrics < -0.2 ? float3( 0, 0, -sumBarycentrics )
			: sumBarycentrics > 1.2 ? float3( sumBarycentrics - 1.0, 0, 0 )
			:  _tex_FalseColors.SampleLevel( LinearClamp, float2( sumBarycentrics, 0.5 ), 0.0 );
//			: sumBarycentrics;
#endif
}

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

	Out.color = 0.0;
	if ( nodeIndex == _hoveredNodeIndex ) {
		Out.color = float3( 1, 1, 0 );	// Hovered
	} else if ( node.m_flags & 7U ) {
		Out.color = float3( 1, 1, 1 );	// Selected
	} else {
		// Regular
		uint	displayMode = _renderFlags & 0x3U;
		bool	showLog = _renderFlags & 0x10U;

		if ( displayMode != 2U ) {
			float	heat = 0.0;
			if ( displayMode & 1 ) {
				if ( showLog ) {
					Out.color = ComputeSumBarycentrics( nodeIndex, Out.UV.z );
				} else {
					heat = _SB_HeatBarycentrics[Local2GlobalIndex( nodeIndex, _sourceIndex )];
					Out.color = _tex_FalseColors.SampleLevel( LinearClamp, float2( heat, 0.5 ), 0.0 );
				}
			} else {
				heat = _SB_Heat[Local2GlobalIndex( nodeIndex, _sourceIndex )];

				if ( showLog ) {
					// Render Log(Temp)
					float	logTemp = 0.43429448190325182765112891891661 * log( max( 1e-6, heat ) );	// Log10( temp )
					heat = (6.0 + logTemp) / 6.0;
				} else {
					heat = lerp( 0.1, 1.0, heat );
				}

				Out.color = _tex_FalseColors.SampleLevel( LinearClamp, float2( heat, 0.5 ), 0.0 );
			}
		} else {
			Out.color = ComputeResultColor( nodeIndex );
		}
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
	Out.UV.z = 0.99;//saturate( lerp( sourceNode.m_mass, targetNode.m_mass, U ) / _maxMass );

	Out.color = 0.0;
	if ( (sourceNode.m_flags & 7U) && (targetNode.m_flags & 7U) ) {
		Out.__Position.z = 0.0;	// Draw in front

		if ( sourceNode.m_flags & 4U )
			Out.color = 0.9 * float3( 0, 1, 1 );		// Child selection
		else if ( sourceNode.m_flags & 2U )
			Out.color = 0.4 * float3( 1, 1, 1 );		// Hierarchy selection
		else if ( sourceNode.m_flags & 1U )
			Out.color = 0.9 * float3( 1, 1, 1 );		// Actual selection
	} else if ( (_renderFlags & 0x3U) != 2U ) {
		// Draw heat gradient
		uint	displayMode = _renderFlags & 0x3U;
		bool	showLog = _renderFlags & 0x10U;

		uint	nodeIndex = U < 0.5 ? sourceNodeIndex : targetNodeIndex;
		float	heat = 0.0;
		if ( displayMode & 1 ) {
			if ( showLog ) {
				Out.color = ComputeSumBarycentrics( nodeIndex, Out.UV.z );
			} else {
				heat = _SB_HeatBarycentrics[Local2GlobalIndex( nodeIndex, _sourceIndex )];
				heat = lerp( 0.1, 0.9, heat );
				Out.color =_tex_FalseColors.SampleLevel( LinearClamp, float2( heat, 0.5 ), 0.0 );
			}

		} else {
			heat = _SB_Heat[Local2GlobalIndex( nodeIndex, _sourceIndex )];

			if ( showLog ) {
				// Render Log(Temp)
				float	logTemp = 0.43429448190325182765112891891661 * log( max( 1e-6, heat ) );	// Log10( temp )
				heat = (6.0 + logTemp) / 6.0;
			} else {
				heat = lerp( 0.1, 0.9, heat );
			}

			Out.color =_tex_FalseColors.SampleLevel( LinearClamp, float2( heat, 0.5 ), 0.0 );
		}

	} else {
		uint	nodeIndex = U < 0.5 ? sourceNodeIndex : targetNodeIndex;
		Out.color = ComputeResultColor( nodeIndex );
	}

	Out.__Position.z = Out.UV.z;

	return Out;
}

float3	PS2( PS_IN _In ) : SV_TARGET0 {
	return 0.5 * sqrt( 1.0 - pow2( _In.UV.y ) ) * _In.color;
}

