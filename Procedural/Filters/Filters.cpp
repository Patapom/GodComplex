#include "../../GodComplex.h"

//////////////////////////////////////////////////////////////////////////
// Gaussian Blur
struct __BlurStruct
{
	TextureBuilder*	pSource;
	int		W, H;
	int		Size;
	float*	pWeights;
	float	InvSumWeights;
};

// Wrap versions
void	FillBlurGaussianHW( int _X, int _Y, const float2& _UV, Pixel& _Pixel, void* _pData )
{
	__BlurStruct&	Data = *((__BlurStruct*) _pData);

	float	Xl = _X-1.0f, Xr = _X+1.0f;
	float	Y = float(_Y);

	Data.pSource->SampleWrap( float(_X), Y, 0, _Pixel );

	Pixel	Temp;
	for ( int i=0; i < Data.Size; i++ )
	{
		float	Weight = Data.pWeights[i];

		// Accumulate from the left
		Data.pSource->SampleWrap( Xl, Y, 0, Temp );	Xl--;
		_Pixel.RGBA = _Pixel.RGBA + Weight * Temp.RGBA;
		_Pixel.Height += Weight * Temp.Height;
		_Pixel.Roughness += Weight * Temp.Roughness;

		// Accumulate from the right
		Data.pSource->SampleWrap( Xr, Y, 0, Temp );	Xr++;
		_Pixel.RGBA = _Pixel.RGBA + Weight * Temp.RGBA;
		_Pixel.Height += Weight * Temp.Height;
		_Pixel.Roughness += Weight * Temp.Roughness;
	}

	// Normalize result
	_Pixel.RGBA = Data.InvSumWeights * _Pixel.RGBA;
	_Pixel.Roughness *= Data.InvSumWeights;
	_Pixel.Height *= Data.InvSumWeights;
}
void	FillBlurGaussianVW( int _X, int _Y, const float2& _UV, Pixel& _Pixel, void* _pData )
{
	__BlurStruct&	Data = *((__BlurStruct*) _pData);

	float	X = float(_X);
	float	Yl = _Y-1.0f, Yr = _Y+1.0f;

	Data.pSource->SampleWrap( X, float(_Y), 0, _Pixel );

	Pixel	Temp;
	for ( int i=0; i < Data.Size; i++ )
	{
		float	Weight = Data.pWeights[i];

		// Accumulate from the top
		Data.pSource->SampleWrap( X, Yl, 0, Temp );	Yl--;
		_Pixel.RGBA = _Pixel.RGBA + Weight * Temp.RGBA;
		_Pixel.Height += Weight * Temp.Height;
		_Pixel.Roughness += Weight * Temp.Roughness;

		// Accumulate from the bottom
		Data.pSource->SampleWrap( X, Yr, 0, Temp );	Yr++;
		_Pixel.RGBA = _Pixel.RGBA + Weight * Temp.RGBA;
		_Pixel.Height += Weight * Temp.Height;
		_Pixel.Roughness += Weight * Temp.Roughness;
	}

	// Normalize result
	_Pixel.RGBA = Data.InvSumWeights * _Pixel.RGBA;
	_Pixel.Roughness *= Data.InvSumWeights;
	_Pixel.Height *= Data.InvSumWeights;
}
// Clamp versions
void	FillBlurGaussianHC( int _X, int _Y, const float2& _UV, Pixel& _Pixel, void* _pData )
{
	__BlurStruct&	Data = *((__BlurStruct*) _pData);

	float	Xl = _X-1.0f, Xr = _X+1.0f;
	float	Y = float(_Y);

	Data.pSource->SampleClamp( float(_X), Y, 0, _Pixel );

	Pixel	Temp;
	for ( int i=0; i < Data.Size; i++ )
	{
		float	Weight = Data.pWeights[i];

		// Accumulate from the left
		Data.pSource->SampleClamp( Xl, Y, 0, Temp );	Xl--;
		_Pixel.RGBA = _Pixel.RGBA + Weight * Temp.RGBA;
		_Pixel.Height += Weight * Temp.Height;
		_Pixel.Roughness += Weight * Temp.Roughness;

		// Accumulate from the right
		Data.pSource->SampleClamp( Xr, Y, 0, Temp );	Xr++;
		_Pixel.RGBA = _Pixel.RGBA + Weight * Temp.RGBA;
		_Pixel.Height += Weight * Temp.Height;
		_Pixel.Roughness += Weight * Temp.Roughness;
	}

	// Normalize result
	_Pixel.RGBA = Data.InvSumWeights * _Pixel.RGBA;
	_Pixel.Roughness *= Data.InvSumWeights;
	_Pixel.Height *= Data.InvSumWeights;
}
void	FillBlurGaussianVC( int _X, int _Y, const float2& _UV, Pixel& _Pixel, void* _pData )
{
	__BlurStruct&	Data = *((__BlurStruct*) _pData);

	float	X = float(_X);
	float	Yl = _Y-1.0f, Yr = _Y+1.0f;

	Data.pSource->SampleClamp( X, float(_Y), 0, _Pixel );

	Pixel	Temp;
	for ( int i=0; i < Data.Size; i++ )
	{
		float	Weight = Data.pWeights[i];

		// Accumulate from the top
		Data.pSource->SampleClamp( X, Yl, 0, Temp );	Yl--;
		_Pixel.RGBA = _Pixel.RGBA + Weight * Temp.RGBA;
		_Pixel.Height += Weight * Temp.Height;
		_Pixel.Roughness += Weight * Temp.Roughness;

		// Accumulate from the bottom
		Data.pSource->SampleClamp( X, Yr, 0, Temp );	Yr++;
		_Pixel.RGBA = _Pixel.RGBA + Weight * Temp.RGBA;
		_Pixel.Height += Weight * Temp.Height;
		_Pixel.Roughness += Weight * Temp.Roughness;
	}

	// Normalize result
	_Pixel.RGBA = Data.InvSumWeights * _Pixel.RGBA;
	_Pixel.Roughness *= Data.InvSumWeights;
	_Pixel.Height *= Data.InvSumWeights;
}

void	Filters::BlurGaussian( TextureBuilder& _Builder, float _SizeX, float _SizeY, bool _bWrap, float _MinWeight )
{
	int	W = _Builder.GetWidth(), H = _Builder.GetHeight();

	TextureBuilder	Temp( W, H );

	// Apply horizontal pass
	{
		__BlurStruct	BS;
		BS.pSource = &_Builder;
		BS.W = W;
		BS.H = H;

		BS.Size = ceilf( _SizeX );
		float	k = logf( _MinWeight ) / (_SizeX*_SizeX);

		BS.pWeights = new float[BS.Size];
		BS.InvSumWeights = 1.0f;
		for ( int i=0; i < BS.Size; i++ )
		{
			BS.pWeights[i] = expf( k * (1+i)*(1+i) );
			BS.InvSumWeights += 2.0f * BS.pWeights[i];
		}
		BS.InvSumWeights = 1.0f / BS.InvSumWeights;

		Temp.Fill( _bWrap ? FillBlurGaussianHW : FillBlurGaussianHC, &BS );

		delete[] BS.pWeights;
	}

	// Apply vertical pass
	{
		__BlurStruct	BS;
		BS.pSource = &Temp;
		BS.W = W;
		BS.H = H;

		BS.Size = ceilf( _SizeY );
		float	k = logf( _MinWeight ) / (_SizeY*_SizeY);

		BS.pWeights = new float[BS.Size];
		BS.InvSumWeights = 1.0f;
		for ( int i=0; i < BS.Size; i++ )
		{
			BS.pWeights[i] = expf( k * (1+i)*(1+i) );
			BS.InvSumWeights += 2.0f * BS.pWeights[i];
		}
		BS.InvSumWeights = 1.0f / BS.InvSumWeights;

		_Builder.Fill( _bWrap ? FillBlurGaussianVW : FillBlurGaussianVC, &BS );

		delete[] BS.pWeights;
	}
}

//////////////////////////////////////////////////////////////////////////
// Unsharp masking
void	FillUnsharpMaskSubtract( int _X, int _Y, const float2& _UV, Pixel& _Pixel, void* _pData )
{
	TextureBuilder&	SourceSmooth = *((TextureBuilder*) _pData);

	Pixel	Smooth;
	SourceSmooth.Get( _X, _Y, 0, Smooth );

	_Pixel.RGBA = 2.0f * _Pixel.RGBA - Smooth.RGBA;
	_Pixel.Height = 2.0f * _Pixel.Height - Smooth.Height;
	_Pixel.Roughness = 2.0f * _Pixel.Roughness - Smooth.Roughness;

	// Clip negatives
	_Pixel.RGBA = _Pixel.RGBA.Max( float4::Zero );
	_Pixel.Height = MAX( 0.0f, _Pixel.Height );
	_Pixel.Roughness = MAX( 0.0f, _Pixel.Roughness );
}

void	Filters::UnsharpMask( TextureBuilder& _Builder, float _Size )
{
	// Blur the source
	TextureBuilder	Temp( _Builder.GetWidth(), _Builder.GetHeight() );
	Temp.CopyFrom( _Builder );
	BlurGaussian( Temp, _Size, _Size );

	// Subtract
	_Builder.Fill( FillUnsharpMaskSubtract, &Temp );
}

//////////////////////////////////////////////////////////////////////////
// Luminance tweaking
struct __BCGStruct
{
	float	B, C, G;
};
void	FillBCG( int _X, int _Y, const float2& _UV, Pixel& _Pixel, void* _pData )
{
	__BCGStruct&	BCG = *((__BCGStruct*) _pData);

	float	Luma = _Pixel.RGBA | LUMINANCE;
//	float	ContrastedLuma = BCG.B + BCG.C * (Luma - 0.5f);
	float	ContrastedLuma = 0.5f + BCG.C * (Luma + BCG.B);
			ContrastedLuma = SATURATE( ContrastedLuma );
	float	NewLuma = powf( ContrastedLuma, BCG.G );

	_Pixel.RGBA = _Pixel.RGBA * (NewLuma / Luma);
}

void	Filters::BrightnessContrastGamma( TextureBuilder& _Builder, float _Brightness, float _Contrast, float _Gamma )
{
	__BCGStruct	BCG;
//	BCG.B = 0.5f + _Brightness;
	BCG.B = _Brightness - 0.5f;
	BCG.C = tanf( HALFPI * 0.5f * (1.0f + _Contrast) );
	BCG.G = _Gamma;

	_Builder.Fill( FillBCG, &BCG );
}

//////////////////////////////////////////////////////////////////////////
// Filters
struct __EmbossStruct
{
	TextureBuilder*	pSource;
	float2		Direction;
	float			Amplitude;
};
void	FillEmboss( int _X, int _Y, const float2& _UV, Pixel& _Pixel, void* _pData )
{
	__EmbossStruct&	Params = *((__EmbossStruct*) _pData);

	Pixel	C0, C1;
	Params.pSource->SampleWrap( _X + Params.Direction.x, _Y + Params.Direction.y, 0, C0 );
	Params.pSource->SampleWrap( _X - Params.Direction.x, _Y - Params.Direction.y, 0, C1 );

	_Pixel.RGBA = 0.5f * float4::One + Params.Amplitude * (C0.RGBA - C1.RGBA);
	_Pixel.Height = 0.5f + Params.Amplitude * (C0.Height - C1.Height);
	_Pixel.Roughness = 0.5f + Params.Amplitude * (C0.Roughness - C1.Roughness);
}

void	Filters::Emboss( TextureBuilder& _Builder, const float2& _Direction, float _Amplitude )
{
	TextureBuilder	Temp( _Builder.GetWidth(), _Builder.GetHeight() );
	Temp.CopyFrom( _Builder );

	__EmbossStruct	Params;
	Params.pSource = &Temp;
	Params.Direction = _Direction;
	Params.Direction.Normalize();
	Params.Amplitude = _Amplitude;

	_Builder.Fill( FillEmboss, &Params );
}


//////////////////////////////////////////////////////////////////////////
// Erosion
struct __ErosionStruct
{
	Pixel*	pSource;
	int		W, H;
	int		Size;
};
void	FillErode( int _X, int _Y, const float2& _UV, Pixel& _Pixel, void* _pData )
{
	__ErosionStruct&	Params = *((__ErosionStruct*) _pData);

	_Pixel.RGBA = FLOAT32_MAX * float4::One;
	_Pixel.Height = FLOAT32_MAX;
	_Pixel.Roughness = FLOAT32_MAX;
	for ( int Y=_Y-Params.Size; Y <= _Y+Params.Size; Y++ )
	{
		int		SampleY = ((Params.H+Y) % Params.H);
		Pixel*	pScanline = &Params.pSource[Params.W * SampleY];
		for ( int X=_X-Params.Size; X <= _X+Params.Size; X++ )
		{
			int	SampleX = (Params.W+X) % Params.W;
			_Pixel.RGBA = _Pixel.RGBA.Min( pScanline[SampleX].RGBA );
			_Pixel.Height = MIN( _Pixel.Height, pScanline[SampleX].Height );
			_Pixel.Roughness = MIN( _Pixel.Roughness, pScanline[SampleX].Roughness );
		}
	}
}

void	Filters::Erode( TextureBuilder& _Builder, int _KernelSize )
{
	TextureBuilder	Temp( _Builder.GetWidth(), _Builder.GetHeight() );
	Temp.CopyFrom( _Builder );

	__ErosionStruct	Params;
	Params.pSource = Temp.GetMips()[0];
	Params.W = Temp.GetWidth();
	Params.H = Temp.GetHeight();
	Params.Size = _KernelSize;

	_Builder.Fill( FillErode, &Params );
}


//////////////////////////////////////////////////////////////////////////
// Dilation
struct __DilationStruct
{
	Pixel*	pSource;
	int		W, H;
	int		Size;
};
void	FillDilate( int _X, int _Y, const float2& _UV, Pixel& _Pixel, void* _pData )
{
	__DilationStruct&	Params = *((__DilationStruct*) _pData);

	_Pixel.RGBA = -FLOAT32_MAX * float4::One;
	_Pixel.Height = -FLOAT32_MAX;
	_Pixel.Roughness = -FLOAT32_MAX;
	for ( int Y=_Y-Params.Size; Y <= _Y+Params.Size; Y++ )
	{
		int		SampleY = ((Params.H+Y) % Params.H);
		Pixel*	pScanline = &Params.pSource[Params.W * SampleY];
		for ( int X=_X-Params.Size; X <= _X+Params.Size; X++ )
		{
			int	SampleX = (Params.W+X) % Params.W;
			_Pixel.RGBA = _Pixel.RGBA.Max( pScanline[SampleX].RGBA );
			_Pixel.Height = MAX( _Pixel.Height, pScanline[SampleX].Height );
			_Pixel.Roughness = MAX( _Pixel.Roughness, pScanline[SampleX].Roughness );
		}
	}
}

void	Filters::Dilate( TextureBuilder& _Builder, int _KernelSize )
{
	TextureBuilder	Temp( _Builder.GetWidth(), _Builder.GetHeight() );
	Temp.CopyFrom( _Builder );

	__DilationStruct	Params;
	Params.pSource = Temp.GetMips()[0];
	Params.W = Temp.GetWidth();
	Params.H = Temp.GetHeight();
	Params.Size = _KernelSize;

	_Builder.Fill( FillDilate, &Params );
}
