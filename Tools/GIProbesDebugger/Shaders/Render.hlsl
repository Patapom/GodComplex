#include "Global.hlsl"

cbuffer CB_Main : register(b0) {
	float4x4	_Local2World;
	float4		_TargetSize;	// XY=Size, ZW=1/XY
	uint		_Type;			// Visualization type
	uint		_Flags;			// 1 = show cube face color, 2 = show distance
	uint		_SampleIndex;
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
	float		SHFactor;
	float		SH[9];
};
StructuredBuffer< ProbeSampleInfo >	_BufferSamples : register(t1);

struct EmissiveSurfaceInfo {
	uint		ID;
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

// Evaluates the SH coefficients in the requested direction
//
float3	EvaluateSH( float3 _Direction, float _SH[9] )
{
	float	f0 = 0.28209479177387814347403972578039;		// 0.5 / sqrt(PI);
	float	f1 = 0.48860251190291992158638462283835;		// 0.5 * sqrt(3.0/PI);
	float	f2 = 1.0925484305920790705433857058027;			// 0.5 * sqrt(15.0/PI);

	float	EvalSH0 = f0;
	float4	EvalSH1234, EvalSH5678;
	EvalSH1234.x = -f1 * _Direction.x;
	EvalSH1234.y = f1 * _Direction.y;
	EvalSH1234.z = -f1 * _Direction.z;
	EvalSH1234.w = f2 * _Direction.x * _Direction.z;
	EvalSH5678.x = -f2 * _Direction.x * _Direction.y;
	EvalSH5678.y = f2 * 0.28867513459481288225457439025097 * (3.0 * _Direction.y*_Direction.y - 1.0);
	EvalSH5678.z = -f2 * _Direction.z * _Direction.y;
	EvalSH5678.w = f2 * 0.5 * (_Direction.z*_Direction.z - _Direction.x*_Direction.x);

	// Dot the SH together
	return max( 0.0,
		   EvalSH0		* _SH[0]
		 + EvalSH1234.x * _SH[1]
		 + EvalSH1234.y * _SH[2]
		 + EvalSH1234.z * _SH[3]
		 + EvalSH1234.w * _SH[4]
		 + EvalSH5678.x * _SH[5]
		 + EvalSH5678.y * _SH[6]
		 + EvalSH5678.z * _SH[7]
		 + EvalSH5678.w * _SH[8] );
}

// Rotates ZH coefficients in the specified direction (from "Stupid SH Tricks")
// Rotating ZH comes to evaluating scaled SH in the given direction.
// The scaling factors for each band are equal to the ZH coefficients multiplied by sqrt( 4PI / (2l+1) )
//
void ZHRotate( const in float3 _Direction, const in float3 _ZHCoeffs, out float _Coeffs[9] )
{
	float	cl0 = 3.5449077018110320545963349666823 * _ZHCoeffs.x;	// sqrt(4PI)
	float	cl1 = 2.0466534158929769769591032497785 * _ZHCoeffs.y;	// sqrt(4PI/3)
	float	cl2 = 1.5853309190424044053380115060481 * _ZHCoeffs.z;	// sqrt(4PI/5)

	float	f0 = cl0 * 0.28209479177387814347403972578039;	// 0.5 / sqrt(PI);
	float	f1 = cl1 * 0.48860251190291992158638462283835;	// 0.5 * sqrt(3.0/PI);
	float	f2 = cl2 * 1.0925484305920790705433857058027;	// 0.5 * sqrt(15.0/PI);
	_Coeffs[0] = f0;
	_Coeffs[1] = -f1 * _Direction.x;
	_Coeffs[2] = f1 * _Direction.y;
	_Coeffs[3] = -f1 * _Direction.z;
	_Coeffs[4] = f2 * _Direction.x * _Direction.z;
	_Coeffs[5] = -f2 * _Direction.x * _Direction.y;
	_Coeffs[6] = f2 * 0.28209479177387814347403972578039 * (3.0 * _Direction.y*_Direction.y - 1.0);
	_Coeffs[7] = -f2 * _Direction.z * _Direction.y;
	_Coeffs[8] = f2 * 0.5 * (_Direction.z*_Direction.z - _Direction.x*_Direction.x);
}

void BuildSHCosineLobe( const in float3 _Direction, out float _Coeffs[9] )
{
	static const float3 ZHCoeffs = float3(
		0.88622692545275801364908374167057,	// sqrt(PI) / 2
		1.0233267079464884884795516248893,	// sqrt(PI / 3)
		0.49541591220075137666812859564002	// sqrt(5PI) / 8
		);
	ZHRotate( _Direction, ZHCoeffs, _Coeffs );
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
				Factor *= _SampleIndex == ~0U || SampleIndex == _SampleIndex ? 1.0 : 0.0;

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
			Value = S * Sample.Albedo;
			break;
			}
		case 2: Value = 0.5 * (1.0 + Sample.Normal); break;
		case 4: {

// 			float	SH[9];
// 			BuildSHCosineLobe( Sample.Normal, SH );

			Value = Sample.SHFactor * Sample.Albedo * EvaluateSH( wsView, Sample.SH );
			break;
			}
		}
		Value *= Factor;
	}
	if ( _Flags & 16 ) {
		// Show neighbor/voronoï
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

		uint	ID = ~0U;
		if ( _Type == 0 )
			ID = _TexCube.SampleLevel( PointWrap, float4( wsView, 6 ), 0.0 ).w;	// Neighbor ID
		else
			ID = _TexCube.SampleLevel( PointWrap, float4( wsView, 5 ), 0.0 ).w;	// Voronoï ID

		Value = ID != ~0U ? PipoColors[ID&7] : 0.0;
	}

//	float4	Value = _TexCube.SampleLevel( LinearWrap, float4( wsView, _Type ), 0.0 );
	return float4( Value, 1 );
}
