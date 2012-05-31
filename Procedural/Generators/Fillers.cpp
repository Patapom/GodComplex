#include "../../GodComplex.h"

//////////////////////////////////////////////////////////////////////////
// AO
// The algorithm here is to find the "horizon" of the height field for each of the samplings directions
// We slice the circle of directions about the considered point then for each direction we march forward
//	and analyze the height of the height field at that position.
// The height and the distance we marched give us the slope of the horizon, which we keep if it exceeds the
//	maximum for that direction.
// In the end, we obtain a portion of horizon that is not occluded by the surrounding heights. Summing these
//	portions we get the AO...
//
struct __AOStruct
{
	const TextureBuilder*	pSource;
	float			HeightFactor;
	int				DirectionsCount;
	int				SamplesCount;
	bool			bWriteOnlyAlpha;
};
void	FillAO( int _X, int _Y, const NjFloat2& _UV, NjFloat4& _Color, void* _pData )
{
	__AOStruct&	Params = *((__AOStruct*) _pData);

	float	SumAO = 0.0f;
	for ( int DirectionIndex=0; DirectionIndex < Params.DirectionsCount; DirectionIndex++ )
	{
		float		Angle = TWOPI * DirectionIndex / Params.DirectionsCount;
		NjFloat2	Direction( cosf( Angle ), sinf( Angle ) );

//		NjFloat2	Position( float(_X), float(_Y) );	// For some reason, this doesn't compile !!
		NjFloat2	Position;
		Position.x = float(_X);
		Position.y = float(_Y);

		float	MaxSlope = 0.0f;
		for ( int SampleIndex=0; SampleIndex < Params.SamplesCount; SampleIndex++ )
		{
			Position = Position + Direction;	// March one step

			NjFloat4	C;
			Params.pSource->SampleWrap( Position.x, Position.y, C );
			float		Height = C | LUMINANCE;	// Transform into luminance, which we consider being the height

			float		Slope = Params.HeightFactor * Height / (1.0f + SampleIndex);	// The slope of the horizon
			MaxSlope = MAX( MaxSlope, Slope );
		}

		// Accumulate visibility
		SumAO += HALFPI - atanf( MaxSlope );
	}
	SumAO /= HALFPI * Params.DirectionsCount;	// Normalize

	_Color.w = SumAO;
	if ( Params.bWriteOnlyAlpha )
		return;

	_Color.x = SumAO;
	_Color.y = SumAO;
	_Color.z = SumAO;
}

void Fillers::ComputeAO( const TextureBuilder& _Source, TextureBuilder& _Target, float _HeightFactor, int _DirectionsCount, int _SamplesCount, bool _bWriteOnlyAlpha )
{
	__AOStruct	Params;
	Params.pSource = &_Source;
	Params.HeightFactor = _HeightFactor;
	Params.DirectionsCount = _DirectionsCount;
	Params.SamplesCount = _SamplesCount;
	Params.bWriteOnlyAlpha = _bWriteOnlyAlpha;

	_Target.Fill( FillAO, &Params );
}

//////////////////////////////////////////////////////////////////////////
// Dirtyness
struct __DirtynessStruct
{
	TextureBuilder*	pBuilder;
	const Noise*	pNoise;
	float			DirtNoiseFrequency;
	float			DirtAmplitude;
	float			PullBackForce;
	float			AverageIntensity;
};
void	FillDirtyness( int _X, int _Y, const NjFloat2& _UV, NjFloat4& _Color, void* _pData )
{
	__DirtynessStruct&	Params = *((__DirtynessStruct*) _pData);

	NjFloat4	C[3];
	Params.pBuilder->SampleWrap( _X-1.0f, _Y-1.0f, C[0] );
	Params.pBuilder->SampleWrap( _X+0.0f, _Y-1.0f, C[1] );
	Params.pBuilder->SampleWrap( _X+1.0f, _Y-1.0f, C[2] );

//	NjFloat4	F = C[0] + C[2] - C[1];	// Some sort of average
//	NjFloat4	F = 0.333f * (C[0] + C[2] + C[1]);
	NjFloat4	F = C[1];
	float		fOffset = Params.DirtAmplitude * Params.pNoise->Perlin( Params.DirtNoiseFrequency * _UV ) + F.w;

	F.x += fOffset;
	F.y += fOffset;
	F.z += fOffset;
	F.w = Params.PullBackForce * (Params.AverageIntensity - (F | LUMINANCE));	// Will yield -1 when F reaches the average intensity so the color is always somewhat brought back to average

	_Color = F;
}

void	Fillers::Dirtyness( TextureBuilder& _Builder, const Noise& _Noise, float _InitialIntensity, float _AverageIntensity, float _DirtNoiseFrequency, float _DirtAmplitude, float _PullBackForce )
{
	__DirtynessStruct	Params;
	Params.pBuilder = &_Builder;
	Params.pNoise = &_Noise;
	Params.DirtNoiseFrequency = _DirtNoiseFrequency;
	Params.DirtAmplitude = _DirtAmplitude;
	Params.PullBackForce = _PullBackForce;
	Params.AverageIntensity = _AverageIntensity;

	// Setup last line used as initial seed
	for ( int X=0; X < _Builder.GetWidth(); X++ )
	{
		NjFloat4&	Pixel = _Builder.GetMips()[0][_Builder.GetWidth() * (_Builder.GetHeight()-1) + X];
//		float		InitialValue = _AverageIntensity + abs( _Noise.Perlin( NjFloat2( _InitNoiseFrequency * float(X) / _Builder.GetWidth(), 0.0f ) ) );
		float		InitialValue = _InitialIntensity;
		Pixel.x = Pixel.y = Pixel.z = InitialValue;
		Pixel.w = 0.0f;
	}

	_Builder.Fill( FillDirtyness, &Params );
}
