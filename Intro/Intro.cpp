#include "../GodComplex.h"
#include "ConstantBuffers.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )	if ( pMaterial->HasErrors() ) return ErrorCode;

// Textures & Render targets
static Texture2D*	gs_pRTHDR = NULL;
static Texture2D*	gs_pTexTestNoise = NULL;

// Primitives
static Primitive*	gs_pPrimQuad = NULL;		// Screen quad for post-processes

// Materials
static Material*	gs_pMatPostFinal = NULL;	// Final post-process rendering to the screen

// Constant buffers
static CBTest			gs_CBTest;
static ConstantBuffer*	gs_pCB_Test = NULL;

//float	CombineDistances( float _pDistances[] )	{ return sqrtf( _pDistances[0] ); }	// Use F1 = closest distance
//float	CombineDistances( float _pDistances[] )	{ return sqrtf( _pDistances[1] ); }	// Use F2 = second closest distance
//float	CombineDistances( float _pDistances[] )	{ return sqrtf( _pDistances[1] ) - sqrtf( _pDistances[0] ); }	// Use F2 - F1
float	CombineDistances( float _pDistances[] )	{ return _pDistances[1] - sqrt(_pDistances[0]); }	// Use F2² - F1 => Alligator scales ! ^^

float	FBMDelegate( const NjFloat2& _UV, void* _pData )
{
	Noise&	N = *((Noise*) _pData);
//	return 2.0f * abs( N.Perlin( 0.003f * _UV ) );
//	return N.Cellular( 16.0f * _UV, CombineDistances, true );
	return 3.0f * abs(N.Worley( 8.0f * _UV, CombineDistances, true ));	// F2² - F1 => Ugly corruption texture
//	return abs(N.Wavelet( _UV ));
}

float	RMFDelegate( const NjFloat2& _UV, void* _pData )
{
	Noise&	N = *((Noise*) _pData);

//	return 4.0f * N.Perlin( 0.002f * _UV );
//	return 8.0f * N.WrapPerlin( _UV );		// Excellent !
//	return 2.0f * N.Cellular( 16.0f * _UV, CombineDistances, true );
//	return 6.0f * abs(N.Worley( 8.0f * _UV, CombineDistances, true ) - 0.4f);	// Use this with F1 => Corruption texture
	return 6.0f * abs(N.Worley( 8.0f * _UV, CombineDistances, true ) + 0.0f);	// Use this with F2²-F1 => Funny crystaline structure
//	return 3.0f * N.Wavelet( _UV );
}

void	FillNoise( int x, int y, const NjFloat2& _UV, NjFloat4& _Color, void* _pData )
{
 	Noise&	N = *((Noise*) _pData);

//	float	C = abs( N.Perlin( 0.005f * _UV ) );					// Simple test with gradient noise that doesn't loop
// 	float	C = abs( N.WrapPerlin( _UV ) );							// Advanced test with gradient noise that loops !
//	float	C = N.Cellular( 16.0f * _UV, CombineDistances, true );	// Simple cellular (NOT Worley !)
	float	C = N.Worley( 16.0f * _UV, CombineDistances, true );	// Worley noise
//	float	C = abs(N.Wavelet( _UV ));								// Wavelet noise
//	float	C = N.FractionalBrownianMotion( FBMDelegate, _pData, _UV );	// Fractional Brownian Motion
//	float	C = N.RidgedMultiFractal( RMFDelegate, _pData, _UV );	// Ridged Multi Fractal

	_Color.x = _Color.y = _Color.z = C;
	_Color.w = 1.0f;
}

void	FillRectangle( const DrawUtils::DrawInfos& i, DrawUtils::Pixel& P )
{
	float		Alpha = i.Coverage;
	float		Distance = 1.0f - 2.0f * abs(i.Distance);

	NjFloat4	C( Distance, 0, 0, 0 );	// Draw distance to border in red
	if ( Distance < 0.0f )
		Alpha = 0.0f;

	P.Blend( C, Alpha );
}

void	FillEllipse( const DrawUtils::DrawInfos& i, DrawUtils::Pixel& P )
{
	NjFloat4	C = i.Distance < 1.0f ? NjFloat4( 0, 0, i.Distance, i.Distance * i.Coverage ) : NjFloat4( 0, 1, 1, 0.5 );
	P.Blend( C, C.w );
}

void	FillLine( const DrawUtils::DrawInfos& i, DrawUtils::Pixel& P )
{
	float	D = MAX( 0.0f, 1.0f - i.Distance );
	P.Blend( NjFloat4( 0, D, 0, 0 ), D * i.Coverage );
}

int	IntroInit( IntroProgressDelegate& _Delegate )
{
	//////////////////////////////////////////////////////////////////////////
	// Create render targets
	{
		gs_pRTHDR = new Texture2D( gs_Device, RESX, RESY, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );

		DrawUtils	Draw;
		{
			TextureBuilder	TB( 512, 512 );

//* General tests for drawing tools and filtering
 			Noise	N( 1 );
			N.Create2DWaveletNoiseTile( 6 );	// If you need to use wavelet noise...
			TB.Fill( FillNoise, &N );

			Draw.SetupSurface( 512, 512, TB.GetMips()[0] );

			Draw.DrawLine( 20.0f, 0.0f, 400.0f, 500.0f, 10.0f, FillLine );

			Draw.SetupContext( 30.0f, 0.0f, 20.0f );
 			Draw.DrawEllipse( 10.0f, 13.4f, 497.39f, 282.78f, 40.0f, 0.0f, FillEllipse );

			Draw.SetupContext( 250.0f, 0.0f, 30.0f );
 			Draw.DrawRectangle( 10.0f, 13.4f, 197.39f, 382.78f, 40.0f, 0.5f, FillRectangle );

//			Filters::BlurGaussian( TB, 20.0f, 20.0f, true, 0.5f );
//			Filters::UnsharpMask( TB, 20.0f );
//			Filters::BrightnessContrastGamma( TB, 0.2f, 0.0f, 2.0f );
//			Filters::Emboss( TB, NjFloat2( 1, 1 ), 4.0f );
//*/

// 			// Test the dirtyness filler
//			Fillers::Dirtyness( TB, N, 0.5f, 0.0f, 0.1f, 0.3f, 0.01f );

			// Test the AO converter
			TextureBuilder	PipoTemp( TB.GetWidth(), TB.GetHeight() );
			PipoTemp.CopyFrom( TB );
			Fillers::ComputeAO( PipoTemp, TB, 2.0f );

			gs_pTexTestNoise = new Texture2D( gs_Device, 512, 512, 1, PixelFormatRGBA16F::DESCRIPTOR, 0, TB.Convert( PixelFormatRGBA16F::DESCRIPTOR ) );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Create primitives
	{
		NjFloat4	pVertices[4] =
		{
			NjFloat4( -1.0f, +1.0f, 0.0f, 0.0f ),
			NjFloat4( -1.0f, -1.0f, 0.0f, 0.0f ),
			NjFloat4( +1.0f, +1.0f, 0.0f, 0.0f ),
			NjFloat4( +1.0f, -1.0f, 0.0f, 0.0f ),
		};
		gs_pPrimQuad = new Primitive( gs_Device, 4, pVertices, 0, NULL, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP, VertexFormatPt4::DESCRIPTOR );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create materials
	{
		gs_pMatPostFinal = CreateMaterial( IDR_SHADER_POST_FINAL, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" );
		CHECK_MATERIAL( gs_pMatPostFinal, 1001 );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create constant buffers
	{
		gs_pCB_Test = new ConstantBuffer( gs_Device, sizeof(gs_CBTest) );
	}

	return 0;
}

void	IntroExit()
{
	// Release constant buffers
	delete gs_pCB_Test;

	// Release materials
 	delete gs_pMatPostFinal;

	// Release primitives
	delete gs_pPrimQuad;

	// Release render targets & textures
	delete gs_pTexTestNoise;
	delete gs_pRTHDR;
}

bool	IntroDo( float _Time, float _DeltaTime )
{
	gs_Device.ClearRenderTarget( gs_Device.DefaultRenderTarget(), NjFloat4( 0.5f, 0.5f, 0.5f, 1.0f ) );

	// Setup default states
	gs_Device.SetStates( *gs_Device.m_pRS_CullNone, *gs_Device.m_pDS_Disabled, *gs_Device.m_pBS_Disabled );

// 	{	// Render some shit to the HDR buffer
// 		gs_Device.SetRenderTarget( *gs_pRTHDR );
// 
// 	}

	// Render to screen
	USING_MATERIAL_START( *gs_pMatPostFinal )
//		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget(), &gs_Device.DefaultDepthStencil() );
		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget() );

		gs_CBTest.LOD = 10.0f * (1.0f - fabs( sinf( _Time ) ));

gs_CBTest.LOD = 0.0f;

		gs_pCB_Test->UpdateData( &gs_CBTest );
		gs_pCB_Test->SetPS( 0 );

		gs_pTexTestNoise->SetPS( 0 );

		gs_pPrimQuad->Render( M );
	USING_MATERIAL_END

	// Present !
	gs_Device.DXSwapChain().Present( 0, 0 );

	return true;	// True means continue !
}
