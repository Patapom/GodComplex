#include "../GodComplex.h"

// Textures
static Texture2D*	gs_pTexTestNoise = NULL;

//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

//float	CombineDistances( float _pDistances[], int _pCellX[], int _pCellY[], int _pCellZ[], void* _pData )	{ return sqrtf( _pDistances[0] ); }	// Use F1 = closest distance
//float	CombineDistances( float _pDistances[], int _pCellX[], int _pCellY[], int _pCellZ[], void* _pData )	{ return sqrtf( _pDistances[1] ); }	// Use F2 = second closest distance
float	CombineDistances( float _pDistances[], int _pCellX[], int _pCellY[], int _pCellZ[], void* _pData )	{ return sqrtf( _pDistances[1] ) - sqrtf( _pDistances[0] ); }	// Use F2 - F1
//float	CombineDistances( float _pDistances[], int _pCellX[], int _pCellY[], int _pCellZ[], void* _pData )	{ return _pDistances[1] - sqrt(_pDistances[0]); }	// Use F2² - F1 => Alligator scales ! ^^

float	FBMDelegate( const float2& _UV, void* _pData )
{
	Noise&	N = *((Noise*) _pData);
//	return 2.0f * abs( N.Perlin( 0.003f * _UV ) );
//	return N.Cellular( 16.0f * _UV, CombineDistances, NULL, true );
	return 3.0f * abs(N.Worley( 8.0f * _UV, CombineDistances, NULL, true ));	// F2² - F1 => Ugly corruption texture
//	return abs(N.Wavelet( _UV ));
}

float	RMFDelegate( const float2& _UV, void* _pData )
{
	Noise&	N = *((Noise*) _pData);

//	return 4.0f * N.Perlin( 0.002f * _UV );
//	return 8.0f * N.WrapPerlin( _UV );		// Excellent !
//	return 2.0f * N.Cellular( 16.0f * _UV, CombineDistances, NULL, true );
//	return 6.0f * abs(N.Worley( 8.0f * _UV, CombineDistances, NULL, true ) - 0.4f);	// Use this with F1 => Corruption texture
	return 6.0f * abs(N.Worley( 8.0f * _UV, CombineDistances, NULL, true ) + 0.0f);	// Use this with F2²-F1 => Funny crystaline structure
//	return 3.0f * N.Wavelet( _UV );
}

void	FillNoise( int x, int y, const float2& _UV, float4& _Color, void* _pData )
{
 	Noise&	N = *((Noise*) _pData);

//	float	C = abs( N.Perlin( 0.005f * _UV ) );					// Simple test with gradient noise that doesn't loop
// 	float	C = abs( N.WrapPerlin( _UV ) );							// Advanced test with gradient noise that loops !
//	float	C = N.Cellular( 16.0f * _UV, CombineDistances, NULL, true );	// Simple cellular (NOT Worley !)
	float	C = N.Worley( 16.0f * _UV, CombineDistances, NULL, true );	// Worley noise
//	float	C = abs(N.Wavelet( _UV ));								// Wavelet noise
//	float	C = N.FractionalBrownianMotion( FBMDelegate, _pData, _UV );	// Fractional Brownian Motion
//	float	C = N.RidgedMultiFractal( RMFDelegate, _pData, _UV );	// Ridged Multi Fractal

	_Color.Set( C, C, C, 1.0f );
}

void	FillRectangle( const DrawUtils::DrawInfos& i, Pixel& P )
{
	float		Alpha = i.Coverage;
	float		Distance = 1.0f - 2.0f * abs(i.Distance);

	Pixel	P2( float4( Distance, 0, 0, 0 ) );	// Draw distance to border in red
	if ( Distance < 0.0f )
		Alpha = 0.0f;

	P.Blend( P2, Alpha );
}

void	FillEllipse( const DrawUtils::DrawInfos& i, Pixel& P )
{
	Pixel	C( i.Distance < 1.0f ? float4( 0, 0, i.Distance, i.Distance * i.Coverage ) : float4( 0, 1, 1, 0.5 ) );
	P.Blend( C, C.RGBA.w );
}

void	FillLine( const DrawUtils::DrawInfos& i, Pixel& P )
{
	float	D = MAX( 0.0f, 1.0f - i.Distance );
	P.Blend( Pixel( float4( 0, D, 0, 0 ) ), D * i.Coverage );
}

void	FillScratch( const DrawUtils::DrawInfos& i, Pixel& P, float _Distance, float _U )
{
 	Noise&	N = *((Noise*) i.pData);

 	float		Value = 1.0f * N.Perlin( 0.001f * float2( float(i.x) / i.w, float(i.y) / i.h ) );
 				Value += abs( 4.0f * N.Perlin( 0.005f * (Value + _U) ) );

	float4	Color( Value, Value, Value, i.Coverage * (1.0f - abs(i.Distance)) );
	P.Blend( Pixel( Color ), Color.w );
}

void	FillSplotch( const DrawUtils::DrawInfos& i, Pixel& P )
{
 	Noise&	N = *((Noise*) i.pData);

	float2	UV = i.UV;
	UV.x -= 0.5f;
	UV.y -= 0.5f;

	float	Scale = 1.0f + 1.0f * N.Perlin( float2( 0.005f * i.x / i.w, 0.005f * i.y / i.h ) );
	UV.x *= Scale;
	UV.y *= Scale;

	float	Distance2Center = UV.Length();

	float	C = 1.2f * (1.0f - 2.0f * Distance2Center);
			C = SATURATE( C );
	float	A = C * C * i.Coverage;
	float4	Color( C, C, C, A );

	P.Blend( Pixel( Color ), Color.w );
}

int	Build2DTextures( IntroProgressDelegate& _Delegate )
{
return 0;

	DrawUtils	Draw;
	{
		TextureBuilder	TB( 512, 512 );
		Draw.SetupSurface( 512, 512, TB.GetMips()[0] );	// Let's draw into the first mip level !

/* General tests for drawing tools and filtering
 		Noise	N( 1 );
		N.Create2DWaveletNoiseTile( 6 );	// If you need to use wavelet noise...
		TB.Fill( FillNoise, &N );

		Draw.DrawLine( 20.0f, 0.0f, 400.0f, 500.0f, 10.0f, FillLine, NULL );

		Draw.SetupContext( 30.0f, 0.0f, 20.0f );
 		Draw.DrawEllipse( 10.0f, 13.4f, 497.39f, 282.78f, 40.0f, 0.0f, FillEllipse, NULL );

		Draw.SetupContext( 250.0f, 0.0f, 30.0f );
 		Draw.DrawRectangle( 10.0f, 13.4f, 197.39f, 382.78f, 40.0f, 0.5f, FillRectangle, NULL );

//		Filters::BlurGaussian( TB, 20.0f, 20.0f, true, 0.5f );
//		Filters::UnsharpMask( TB, 20.0f );
//		Filters::BrightnessContrastGamma( TB, 0.2f, 0.0f, 2.0f );
//		Filters::Emboss( TB, NjFloat2( 1, 1 ), 4.0f );
//		Filters::Erode( TB );
//		Filters::Dilate( TB );

// 		// Test the AO converter
// 		TextureBuilder	PipoTemp( TB.GetWidth(), TB.GetHeight() );
// 		PipoTemp.CopyFrom( TB );
// 		Generators::ComputeAO( PipoTemp, TB, 2.0f );

//*/

// 		// Test the dirtyness generator
//		Generators::Dirtyness( TB, N, 0.5f, 0.0f, 0.1f, 0.3f, 0.01f );

	// Test the secret marble generator
//		Generators::Marble( TB, 30, 0.2f, 0.5f, 1.0f, 0.5f, 0.5f );
//		Generators::Marble3D( TB, 16, 0.2f );


//* Test advanced drawing

 		Noise	N( 1 );
		// Draw scratches
		for ( int i=0; i < 10; i++ )
		{
			float2	Pos;
			Pos.x = _frand( 0.0f, 512.0f );
			Pos.y = _frand( 0.0f, 512.0f );
			float2	Dir;
			Dir.x = _frand( -1.0f, +1.0f );
			Dir.y = _frand( -1.0f, +1.0f );

			float	Length = _frand( 100.0f, 500.0f );
			float	Thickness = _frand( 4.0f, 5.0f );
			float	Curve = _frand( -0.05f, 0.05f );

			Draw.DrawScratch( Pos, Dir, Length, Thickness, 0.01f, Curve, 10.0f, FillScratch, &N );
		}

		// Draw splotches
		for ( int Y=0; Y < 20; Y++ )
		{
			int		Count = _rand( 10, 20 );

			float	Angle = _frand( -180.0f, +180.0f );
			float	DeltaAngle = _frand( 0.0f, 40.0f );		// Splotches rotate with time
					DeltaAngle /= Count;

			float2	Pos;
			Pos.x = _frand( -512.0f, 512.0f );
			Pos.y = _frand( -512.0f, 512.0f );

			float2	Size;
			Size.x = _frand( 40.0f, 50.0f );
			Size.y = _frand( 40.0f, 50.0f );
			float	DeltaSize = _frand( 0.0f, 30.0f );	// Splotches get bigger or smaller with time
					DeltaSize /= Count;

			float	Step = _frand( 0.25f, 1.0f ) * Size.x;	// Splotches interpenetrate

			for ( int X=0; X < Count; X++ )
			{
				Draw.SetupTransform( Pos.x, Pos.y, Angle );
				Draw.DrawRectangle( 0.0f, 0.0f, Size.x, Size.y, 0.1f * Size.Min(), 0.0f, FillSplotch, &N );

				Angle += DeltaAngle;
				float	AngleRad = DEG2RAD( Angle );
				Pos = Pos + Step * float2( cosf(AngleRad), sinf(AngleRad) );
				Size.x += DeltaSize;
			}
		}

// Draw.DrawRectangle( 0.0f, 0.0f, 100.0f, 100.0f, 10.0f, 0.0f, FillSplotch, &N );

//			Filters::BrightnessContrastGamma( TB, 0.1f, 0.7f, 2.0f );
//			Filters::Erode( TB, 3 );
//*/

		gs_pTexTestNoise = TB.CreateTexture( PixelFormatRGBA16F::DESCRIPTOR, TextureBuilder::CONV_RGBA_NxNyHR_M );
	}

	return 0;	// OK !
}

void	Delete2DTextures()
{
	delete gs_pTexTestNoise;
}
