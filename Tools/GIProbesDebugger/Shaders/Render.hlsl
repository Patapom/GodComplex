#include "Global.hlsl"

cbuffer CB_Main : register(b0) {
	float4x4	_Local2World;
	float4		_TargetSize;	// XY=Size, ZW=1/XY
	uint		_Type;			// Visualization type
	uint		_Flags;			// 1 = show cube face color, 2 = show distance
};

TextureCubeArray<float4>	_TexCube : register(t0);

struct ProbeSampleInfo {
	uint		ID;
	float3		Position;
	float3		Normal;
	float3		Tangent;
	float3		BiTangent;
	float		Radius;
	float3		Albedo;
	float3		F0;
	uint		PixelsCount;
	float		SH[9];
};
StructuredBuffer< ProbeSampleInfo >	_BufferSamples : register(t1);

struct EmissiveSurfaceInfo {
	uint			ID;
	float		SH[9];
};
StructuredBuffer< EmissiveSurfaceInfo >	_BufferEmissiveSurfaces : register(t2);


struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) {
	return _In;
}

float	Bisou( float x ) {
	return 0.2 + 0.8 * (x > 0);
}
float3	GetCubeFaceColor( float3 _View ) {
	if ( abs(_View.x) > abs(_View.y ) ) {
		if ( abs(_View.x) > abs(_View.z ) ) {
			return float3( Bisou(_View.x), 0, 0 );
		} else {
			return float3( 0, 0, Bisou(_View.z) );
		}
	} else {
		if ( abs(_View.y) > abs(_View.z ) ) {
			return float3( 0, Bisou(_View.y), 0 );
		} else {
			return float3( 0, 0, Bisou(_View.z) );
		}
	}
}

// Our cube map array contains:
//
// #0 => P.Position, P.Distance
// #1 => P.Normal, P.SmoothedDistance 
// #2 => P.Albedo, P.SmoothedInfinity
// #3 => P.StaticLitColor, (float) P.ParentSampleIndex
// #4 => P.SmoothedStaticLitColor, (float) P.Importance
// #5 => P.UsedForSampling ? 1 : 0, P.Infinity ? 1 : 0, (float) P.FaceIndex, 0
// #6 => P.F0, (float) P.NeighborProbeID
// #7 => P.NeighborProbeDistance, 0, 0, 0
//
//
float4	PS( VS_IN _In ) : SV_TARGET0 {

	float2	UV = _In.__Position.xy * _TargetSize.zw;
	float2	TanFOV = float2( _TargetSize.x * _TargetSize.w, 1.0 ) * tan( 0.5 * 120.0 * PI / 180.0 );
	float3	csView = float3( TanFOV.x * (2.0 * UV.x - 1.0), TanFOV.y * (1.0 - 2.0 * UV.y), 1.0 );
	float3	wsView = normalize( mul( float4( csView, 1.0 ), _Local2World ).xyz );
//return float4( wsView, 1 );

	float3	Value = 0.0;
	switch ( _Type ) {
	case 0:	// World-Space Position
		float4	Temp = _TexCube.SampleLevel( LinearWrap, float4( wsView, 0 ), 0.0 );
		Value = Temp.w < 1000.0 ? 0.1 * Temp.xyz : 0.0;
		break;
	case 1:	// World-Space Normal
		Value = 0.5 * (1.0 + _TexCube.SampleLevel( LinearWrap, float4( wsView, 1 ), 0.0 ).xyz);
		break;
	case 2:	// Distance
		Value = 0.05 * _TexCube.SampleLevel( LinearWrap, float4( wsView, 0 ), 0.0 ).w;	// Distance
//		Value = 0.05 * _TexCube.SampleLevel( LinearWrap, float4( wsView, 1 ), 0.0 ).w;	// Smoothed distance
//		Value = _TexCube.SampleLevel( LinearWrap, float4( wsView, 5 ), 0.0 ).z;			// Infinity
//		Value = _TexCube.SampleLevel( LinearWrap, float4( wsView, 2 ), 0.0 ).w;			// Smoothed infinity
		break;
	case 3:	// Albedo
		float	S = lerp( 0.9, 1.0, 0.5 * (1.0 + _TexCube.SampleLevel( LinearWrap, float4( wsView, 1 ), 0.0 ).y) );
		Value = S * _TexCube.SampleLevel( LinearWrap, float4( wsView, 2 ), 0.0 ).xyz;
		break;
	case 4:	// Static lit color
		Value = _TexCube.SampleLevel( LinearWrap, float4( wsView, 3 ), 0.0 ).xyz;
		break;
	case 5:	// Sample index
		{
			uint	SampleIndex = _TexCube.SampleLevel( PointWrap, float4( wsView, 3 ), 0.0 ).w;
			Value = SampleIndex != ~0U ? 1.0/128.0 * SampleIndex : float3( 1, 0, 0 );
			break;
		}
	case 6:	// Distance 2 border
		{
			uint	SurfaceIndex = _TexCube.SampleLevel( PointWrap, float4( wsView, 3 ), 0.0 ).w;
			if ( SurfaceIndex != ~0U ) {
				Value = 0.01 * _TexCube.SampleLevel( PointWrap, float4( wsView, 5 ), 0.0 ).x;
			} else {
				Value = float3( 1, 0, 0 );
			}
			break;
		}
	case 7:	// Face index
		Value = 0.0001 * _TexCube.SampleLevel( PointWrap, float4( wsView, 5 ), 0.0 ).w;
		break;
	}

	if ( _Flags & 1 )
		Value = GetCubeFaceColor( wsView );
	if ( _Flags & 2 )
		Value = 0.05 * _TexCube.SampleLevel( LinearWrap, float4( wsView, 0 ), 0.0 ).w;
	if ( _Flags & 4 ) {
		float4	Temp = _TexCube.SampleLevel( LinearWrap, float4( wsView, 0 ), 0.0 );
		Value = Temp.w < 1000.0 ? 0.1 * Temp.xyz : 0.0;
	}
	if ( _Flags & 8 ) {
		// Show samples
		bool	ShowAllPixels = (_Type & 1) != 0;
		float	Factor = ShowAllPixels || _TexCube.SampleLevel( PointWrap, float4( wsView, 5 ), 0.0 ).x > 0.5 ? 1 : 0;
		uint	Type = _Type >> 1;
		uint	SampleIndex = _TexCube.SampleLevel( PointWrap, float4( wsView, 3 ), 0.0 ).w;

		ProbeSampleInfo	Sample = _BufferSamples[SampleIndex];

		static float3	PipoColors[8] = {
			float3( 1, 0, 0 ),
			float3( 1, 1, 0 ),
			float3( 0, 1, 0 ),
			float3( 0, 1, 1 ),
			float3( 0, 0, 1 ),
			float3( 1, 0, 1 ),
			float3( 1, 0.5, 0.5 ),
			float3( 0.5, 0.5, 1 ),
		};

		switch ( Type ) {
		case 0: Value = PipoColors[SampleIndex%7]; break;
		case 1: {
			float	S = lerp( 0.9, 1.0, 0.5 * (1.0 + Sample.Normal.y) );
			Value = S * Sample.Albedo; break;
				}
		case 2: Value = 0.5 * (1.0 + Sample.Normal); break;
		}
		Value *= Factor;
	}

//	float4	Value = _TexCube.SampleLevel( LinearWrap, float4( wsView, _Type ), 0.0 );
	return float4( Value, 1 );
}
