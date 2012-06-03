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

	_Color.Set( SumAO, SumAO, SumAO, 1.0f );
}

void Generators::ComputeAO( const TextureBuilder& _Source, TextureBuilder& _Target, float _HeightFactor, int _DirectionsCount, int _SamplesCount, bool _bWriteOnlyAlpha )
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

void	Generators::Dirtyness( TextureBuilder& _Builder, const Noise& _Noise, float _InitialIntensity, float _AverageIntensity, float _DirtNoiseFrequency, float _DirtAmplitude, float _PullBackForce )
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
		Pixel.Set( InitialValue, InitialValue, InitialValue, 0.0f );
	}

	_Builder.Fill( FillDirtyness, &Params );
}

//////////////////////////////////////////////////////////////////////////
// Secret marble recipe
U32	LCGRandom( U32& _LastValue )
{
	return _LastValue = U32( (1103515245u * _LastValue + 12345u) );
}

struct __MarbleStruct
{
	U32		Width;
	float	Min, Factor;
	float*	pBuffer;
};
void	FillMarble( int _X, int _Y, const NjFloat2& _UV, NjFloat4& _Color, void* _pData )
{
	__MarbleStruct&	Params = *((__MarbleStruct*) _pData);

	float	Value = Params.pBuffer[Params.Width*_Y+_X];
	Value = Params.Factor * (Value - Params.Min);
	_Color.Set( Value, Value, Value, 1.0f );
}

void	Generators::Marble( TextureBuilder& _Builder, int _BootSize, float _NoiseAmplitude, float _Weight0, float _Weight1, float _Weight2, float _WeightsNormalizer )
{
	int	W = _Builder.GetWidth();
	int	H = _Builder.GetHeight();
	H += _BootSize;		// Skip the first lines which are sometimes ugly 

	float*		pBuffer = new float[W*H];

	// Fill up the first N lines
	U32		RandomSeed = 1;
	float	Min = FLOAT32_MAX, Max = -FLOAT32_MAX;
	float*	pTarget = pBuffer + 2*W;
	for ( int Y=2; Y < H; Y++ )
	{
		float*	pSource = &pBuffer[W*(Y-1)];
		for ( int X=0; X < W; X++ )
		{
			LCGRandom( RandomSeed );
			float	RandomColor = RandomSeed / 2147483648.0f - 1.0f;

			float	C0 = pSource[(X+W-1) % W];
			float	C1 = pSource[X];
			float	C2 = pSource[(X+1) % W];

			float	C = _WeightsNormalizer * (_Weight0 * C0 + _Weight1 * C1 + _Weight2 * C2);
					C += _NoiseAmplitude * RandomColor;

			C = fmodf( C, 1.0f );
			Min = MIN( Min, C );
			Max = MAX( Max, C );

			*pTarget++ = C;
		}
	}

	// Renormalize
	__MarbleStruct	Params;
	Params.Width = W;
	Params.Min = Min;
	Params.Factor = 1.0f / (Max - Min);
	Params.pBuffer = pBuffer + W * _BootSize;
	_Builder.Fill( FillMarble, &Params );

	delete[] pBuffer;
}

//////////////////////////////////////////////////////////////////////////
// Marble 3D
// struct __Marble3DStruct
// {
// 	U32				Width, Height;
// 	U32				RandomSeed;
// 	float			fNoiseAmplitude;
// 	NjFloat4		Min, Max;
// 	TextureBuilder*	pSource;
// };
// 
// void	FillMarble3D( int _X, int _Y, const NjFloat2& _UV, NjFloat4& _Color, void* _pData )
// {
// 	__Marble3DStruct&	Params = *((__Marble3DStruct*) _pData);
// 
// 	NjFloat4	pC[9];
// 	Params.pSource->SampleWrap( _X-1.0f, _Y-1.0f, pC[0] );
// 	Params.pSource->SampleWrap( _X+0.0f, _Y-1.0f, pC[1] );
// 	Params.pSource->SampleWrap( _X+1.0f, _Y-1.0f, pC[2] );
// 	Params.pSource->SampleWrap( _X-1.0f, _Y+0.0f, pC[3] );
// 	Params.pSource->SampleWrap( _X+0.0f, _Y+0.0f, pC[4] );
// 	Params.pSource->SampleWrap( _X+1.0f, _Y+0.0f, pC[5] );
// 	Params.pSource->SampleWrap( _X-1.0f, _Y+1.0f, pC[6] );
// 	Params.pSource->SampleWrap( _X+0.0f, _Y+1.0f, pC[7] );
// 	Params.pSource->SampleWrap( _X+1.0f, _Y+1.0f, pC[8] );
// 
// //	NjFloat4	C = 0.25f * (pC[1] + pC[3] + pC[5] + pC[7]);
// 	NjFloat4	C = 0.125f * (pC[0] + pC[1] + pC[2] + pC[3] + pC[4] + pC[5] + pC[6] + pC[7] + pC[8]);
// 
// 	float	RandomColor = LCGRandom( Params.RandomSeed ) / 2147483648.0f - 1.0f;
// 	RandomColor *= Params.fNoiseAmplitude;
// 
// 	_Color.x = C.x + RandomColor;
// 	_Color.y = C.y + RandomColor;
// 	_Color.z = C.z + RandomColor;
// 
// 	Params.Min = Params.Min.Min( _Color );
// 	Params.Max = Params.Max.Max( _Color );
// }
// 
// void	FillMarble3DNormalize( int _X, int _Y, const NjFloat2& _UV, NjFloat4& _Color, void* _pData )
// {
// 	__Marble3DStruct&	Params = *((__Marble3DStruct*) _pData);
// 
// 	// Normalize
// 	NjFloat4	Pixel = Params.pSource->GetMips()[0][Params.Width*_Y+_X];
// 	_Color.x = (Pixel.x - Params.Min.x) / (Params.Max.x - Params.Min.x);
// 	_Color.y = (Pixel.y - Params.Min.y) / (Params.Max.y - Params.Min.y);
// 	_Color.z = (Pixel.z - Params.Min.z) / (Params.Max.z - Params.Min.z);
// 	_Color.w = (Pixel.w - Params.Min.w) / (Params.Max.w - Params.Min.w);
// }
// 
// void	Generators::Marble3D( TextureBuilder& _Builder, int _IterationsCount, float _NoiseAmplitude )
// {
// 	int	W = _Builder.GetWidth();
// 	int	H = _Builder.GetHeight();
// 
// 	TextureBuilder		Temp( W, H );
// 	__Marble3DStruct	Params;
// 	Params.Width = W;
// 	Params.Height = H;
// 	Params.RandomSeed = 1;
// 	Params.Min = +FLOAT32_MAX * NjFloat4::One;
// 	Params.Max = -FLOAT32_MAX * NjFloat4::One;
// 	Params.fNoiseAmplitude = _NoiseAmplitude;
// 
// 	TextureBuilder*	pSource = &_Builder;
// 	TextureBuilder*	pTarget = &Temp;
// 	for ( int Iteration=0; Iteration < _IterationsCount; Iteration++ )
// 	{
// 		Params.pSource = pSource;
// 		pTarget->Fill( FillMarble3D, &Params );
// 
// 		TextureBuilder*	pTemp = pSource;
// 		pSource = pTarget;
// 		pTarget = pTemp;
// 	}
// 
// 	// Normalize result
// 	Params.pSource = pSource;
// 	_Builder.Fill( FillMarble3DNormalize, &Params );
// }
