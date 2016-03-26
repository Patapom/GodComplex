#define PI	3.1415926535897932384626433832795

static const uint		PROBES_COUNT = 5;

cbuffer CB_Main : register(b0) {
	float3	iResolution;	// viewport resolution (in pixels)
	float	iGlobalTime;	// shader playback time (in seconds)

	float2	_MainPos;
	float2	_NeighborPosition0;
	float2	_NeighborPosition1;
	float2	_NeighborPosition2;
	float2	_NeighborPosition3;

	uint	_IsolatedProbeIndex;
	float	_WeightMultiplier;
	uint	_ShowWeights;
};

Texture2D< float4 >	_Tex : register(t0);

struct VS_IN {
	float4	__Position : SV_POSITION;
};

SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border


// static const float2		NEIGHBOR_POSITIONS[PROBES_COUNT] = {
// 	float2( 0.3, 0.2 ),
// 	float2( 0.8, 0.3 ),
// 	float2( 0.4, 0.9 ),
// 	float2( 0.2, 0.57 ),
// };

static const float3		NEIGHBOR_COLORS[PROBES_COUNT] = {
	float3( 0, 0, 0 ),
	float3( 1, 0, 0 ),
	float3( 0, 1, 0 ),
	float3( 0, 0, 1 ),
	float3( 0, 1, 1 ),
};

VS_IN	VS( VS_IN _In ) {
	return _In;
}

float	IsOnProbe( float2 _UV, float _Distance2Probe ) {
	return smoothstep( 0.01, 0.006, _Distance2Probe );
}

float	ComputeInterpolationWeight_TEST( float2 _UV, uint _CurrentCellIndex, float2 _ProbePositions[PROBES_COUNT] ) {
	float2	CenterPosition = _ProbePositions[0];
	float	Distance2Center = length( _UV - CenterPosition );

	if ( _IsolatedProbeIndex == 0 ) {

		float	SumWeights = 1e6;
		for ( uint NeighborIndex=1; NeighborIndex < PROBES_COUNT; NeighborIndex++ ) {
			float2	NeighborPosition = _ProbePositions[NeighborIndex];
			float2	PlanePosition = NeighborPosition;
			float2	PlaneNormal = CenterPosition - NeighborPosition;
			float	DistanceCenter2Plane = length( PlaneNormal );
					PlaneNormal /= DistanceCenter2Plane;

			float	Distance2Neighbor = length( _UV - NeighborPosition );
			float	Distance2Plane = dot( _UV - PlanePosition, PlaneNormal );

			float	Weight = Distance2Plane / DistanceCenter2Plane;
//			float	Weight = smoothstep( 0.0, 1.0, Distance2Plane / DistanceCenter2Plane );
			SumWeights = min( SumWeights, Weight );
		}

return SumWeights;
		return _CurrentCellIndex == _IsolatedProbeIndex ? SumWeights / (PROBES_COUNT-1.0) : 0.0;
	}

	float2	NeighborPosition = _ProbePositions[_IsolatedProbeIndex];

	float2	PlanePosition = 0.5 * (CenterPosition + NeighborPosition);
	float2	PlaneNormal = CenterPosition - NeighborPosition;
	float	DistanceCenter2Plane = length( PlaneNormal );
			PlaneNormal /= DistanceCenter2Plane;

	float	Distance2Neighbor = length( _UV - NeighborPosition );
	float	Distance2Plane = dot( _UV - PlanePosition, PlaneNormal );

	return smoothstep( 0.0, 1.0, 2.0 * Distance2Plane / DistanceCenter2Plane );
	return -Distance2Neighbor / Distance2Plane;
}

// Stolen from http://www.iquilezles.org/www/articles/smin/smin.htm
float	SmoothMin( float a, float b ) {
	if ( _ShowWeights & 2 ) {
#if 1	// Exponential version
		const float	k = -16.0;
		float	res = exp( k*a ) + exp( k*b );
		return log( res ) / k;
#else	// Power version
		const float k = 16.0;
		a = pow( saturate(a), k );
		b = pow( saturate(b), k );
		return pow( (a*b) / (a+b), 1.0/k );
#endif
	}
	return min( a, b );
}

float	ComputeInterpolationWeight( float2 _UV, uint _CurrentCellIndex, float2 _ProbePositions[PROBES_COUNT] ) {

	float	Weights[PROBES_COUNT] = { 1e5, 1e5, 1e5, 1e5, 1e5 };
#if 0
	// Use single links to main probe
	float2	ProbePosition0 = _ProbePositions[0];
	for ( uint Index1=1; Index1 < PROBES_COUNT; Index1++ ) {
		float2	ProbePosition1 = _ProbePositions[Index1];

		float2	Normal = ProbePosition1 - ProbePosition0;
		float	Distance = length( Normal );
				Normal /= Distance;

 		float	Weight0 = dot( ProbePosition1 - _UV, Normal ) / Distance;
 		float	Weight1 = dot( _UV - ProbePosition0, Normal ) / Distance;

		Weights[0] = SmoothMin( Weights[0], Weight0 );
		Weights[Index1] = SmoothMin( Weights[Index1], Weight1 );
	}
#else
	// Use all possible links between probes
	// Gives the correct result but actually we could restrict the amount of links to N (direct neighbors) + K (direct neighbors of each neighbor)
	// Actually, this could end up being 2*N instead of N*(N-1) links!
	//
	for ( uint Index0=0; Index0 < PROBES_COUNT-1; Index0++ ) {
		float2	ProbePosition0 = _ProbePositions[Index0];
		for ( uint Index1=Index0+1; Index1 < PROBES_COUNT; Index1++ ) {
			float2	ProbePosition1 = _ProbePositions[Index1];

			float2	Normal = ProbePosition1 - ProbePosition0;
			float	Distance = length( Normal );
					Normal /= Distance;

 			float	Weight0 = dot( ProbePosition1 - _UV, Normal ) / Distance;
 			float	Weight1 = dot( _UV - ProbePosition0, Normal ) / Distance;

// 			Weight0 = smoothstep( 0.0, 1.0, dot( ProbePosition1 - _UV, Normal ) / Distance );
// 			Weight1 = smoothstep( 0.0, 1.0, dot( _UV - ProbePosition0, Normal ) / Distance );

			Weights[Index0] = SmoothMin( Weights[Index0], Weight0 );
			Weights[Index1] = SmoothMin( Weights[Index1], Weight1 );
		}
	}
#endif

	return Weights[_IsolatedProbeIndex];
}

float4	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = 2.0 * _In.__Position.xy / iResolution.xy - 1.0;
	float	AspectRatio = iResolution.x / iResolution.y;
	UV.x *= AspectRatio;

	// Compute cell distances
	float2	CellPositions[PROBES_COUNT] = { _MainPos, _NeighborPosition0, _NeighborPosition1, _NeighborPosition2, _NeighborPosition3 };

	uint	BestProbeIndex = 0;
	float	BestProbeDistance = 1e6;
	float	RightOnProbe = 0.0;

	[loop]
	[fastopt]
	for ( uint ProbeIndex=0; ProbeIndex < PROBES_COUNT; ProbeIndex++ ) {
		float	ProbeDistance = length( UV - CellPositions[ProbeIndex] );
		RightOnProbe += IsOnProbe( UV, ProbeDistance );
		if ( ProbeDistance < BestProbeDistance ) {
			BestProbeDistance = ProbeDistance;
			BestProbeIndex = ProbeIndex;
		}
	}

	// Apply correct cell color
	float3	CellColor = 0.4 * NEIGHBOR_COLORS[BestProbeIndex];

	// Compute interpolation weight
	float3	InterpolationColor = 0.0;
	if ( _ShowWeights & 1 ) {
		float	InterpolationWeight = ComputeInterpolationWeight( UV, BestProbeIndex, CellPositions );
		float3	SpecialBoundary = abs( InterpolationWeight - 0.5 ) < 0.5e-2 ? float3( 0, 0, 1 ) : 0.0;
				SpecialBoundary += abs( InterpolationWeight - 0.0 ) < 0.5e-2 ? float3( 1, 0, 0 ) : 0.0;
				SpecialBoundary += abs( InterpolationWeight - 0.75 ) < 0.5e-2 ? float3( 0, 1, 0 ) : 0.0;
		InterpolationColor = _WeightMultiplier * saturate( InterpolationWeight ) + SpecialBoundary;
	}

	return float4( lerp( CellColor, float3( 1, 1, 0 ), RightOnProbe ) + InterpolationColor, 1.0 );
}
