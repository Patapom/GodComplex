#include "../GodComplex.h"
#include "ConstantBuffers.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )	if ( (pMaterial)->HasErrors() ) return ErrorCode;

static Camera*		gs_pCamera = NULL;

// Textures & Render targets
static Texture2D*	gs_pRTHDR = NULL;
//static Texture2D*	gs_pTexTestNoise = NULL;

// Primitives
static Primitive*	gs_pPrimQuad = NULL;		// Screen quad for post-processes
static Primitive*	gs_pPrimSphereInternal;
static Primitive*	gs_pPrimSphereExternal;

// Materials
static Material*	gs_pMatPostFinal = NULL;	// Final post-process rendering to the screen
static Material*	gs_pMatTestDisplay = NULL;	// Some test material for primitive display

// Constant buffers
static CB<CBTest>*		gs_pCB_Test = NULL;
static CB<CBObject>*	gs_pCB_Object = NULL;


//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
/*

//float	CombineDistances( float _pDistances[] )	{ return sqrtf( _pDistances[0] ); }	// Use F1 = closest distance
//float	CombineDistances( float _pDistances[] )	{ return sqrtf( _pDistances[1] ); }	// Use F2 = second closest distance
float	CombineDistances( float _pDistances[] )	{ return sqrtf( _pDistances[1] ) - sqrtf( _pDistances[0] ); }	// Use F2 - F1
//float	CombineDistances( float _pDistances[] )	{ return _pDistances[1] - sqrt(_pDistances[0]); }	// Use F2² - F1 => Alligator scales ! ^^

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

	_Color.Set( C, C, C, 1.0f );
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

void	FillScratch( const DrawUtils::DrawInfos& i, DrawUtils::Pixel& P, float _Distance, float _U )
{
 	Noise&	N = *((Noise*) i.pData);

 	float		Value = 1.0f * N.Perlin( 0.001f * NjFloat2( float(i.x) / i.w, float(i.y) / i.h ) );
 				Value += abs( 4.0f * N.Perlin( 0.005f * (Value + _U) ) );

	NjFloat4	Color( Value, Value, Value, i.Coverage * (1.0f - abs(i.Distance)) );
	P.Blend( Color, Color.w );
}

void	FillSplotch( const DrawUtils::DrawInfos& i, DrawUtils::Pixel& P )
{
 	Noise&	N = *((Noise*) i.pData);

	NjFloat2	UV = i.UV;
	UV.x -= 0.5f;
	UV.y -= 0.5f;

	float	Scale = 1.0f + 1.0f * N.Perlin( NjFloat2( 0.005f * i.x / i.w, 0.005f * i.y / i.h ) );
	UV.x *= Scale;
	UV.y *= Scale;

	float	Distance2Center = UV.Length();

	float	C = 1.2f * (1.0f - 2.0f * Distance2Center);
			C = CLAMP01( C );
	float	A = C * C * i.Coverage;
	NjFloat4	Color( C, C, C, A );

	P.Blend( Color, Color.w );
}
*/

#include "Build2DTextures.cpp"


int	IntroInit( IntroProgressDelegate& _Delegate )
{
	//////////////////////////////////////////////////////////////////////////
	// Create our camera
	gs_pCamera = new Camera( gs_Device );

	gs_pCamera->SetPerspective( HALFPI, float(RESX) / RESY, 0.01f, 5000.0f );
	gs_pCamera->LookAt( NjFloat3( 0.0f, 0.0f, 10.0f ), NjFloat3( 0.0f, 0.0f, 0.0f ), NjFloat3::UnitY );


	//////////////////////////////////////////////////////////////////////////
	// Create render targets & textures
	{
		gs_pRTHDR = new Texture2D( gs_Device, RESX, RESY, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );

		Build2DTextures( _Delegate );
/*

		DrawUtils	Draw;
		{
			TextureBuilder	TB( 512, 512 );
			Draw.SetupSurface( 512, 512, TB.GetMips()[0] );	// Let's draw into the first mip level !

/ * General tests for drawing tools and filtering
 			Noise	N( 1 );
			N.Create2DWaveletNoiseTile( 6 );	// If you need to use wavelet noise...
			TB.Fill( FillNoise, &N );

			Draw.DrawLine( 20.0f, 0.0f, 400.0f, 500.0f, 10.0f, FillLine, NULL );

			Draw.SetupContext( 30.0f, 0.0f, 20.0f );
 			Draw.DrawEllipse( 10.0f, 13.4f, 497.39f, 282.78f, 40.0f, 0.0f, FillEllipse, NULL );

			Draw.SetupContext( 250.0f, 0.0f, 30.0f );
 			Draw.DrawRectangle( 10.0f, 13.4f, 197.39f, 382.78f, 40.0f, 0.5f, FillRectangle, NULL );

//			Filters::BlurGaussian( TB, 20.0f, 20.0f, true, 0.5f );
//			Filters::UnsharpMask( TB, 20.0f );
//			Filters::BrightnessContrastGamma( TB, 0.2f, 0.0f, 2.0f );
//			Filters::Emboss( TB, NjFloat2( 1, 1 ), 4.0f );
//			Filters::Erode( TB );
//			Filters::Dilate( TB );

// 			// Test the AO converter
// 			TextureBuilder	PipoTemp( TB.GetWidth(), TB.GetHeight() );
// 			PipoTemp.CopyFrom( TB );
// 			Generators::ComputeAO( PipoTemp, TB, 2.0f );

// * /

// 			// Test the dirtyness generator
//			Generators::Dirtyness( TB, N, 0.5f, 0.0f, 0.1f, 0.3f, 0.01f );

			// Test the secret marble generator
//			Generators::Marble( TB, 30, 0.2f, 0.5f, 1.0f, 0.5f, 0.5f );
//			Generators::Marble3D( TB, 16, 0.2f );


// * Test advanced drawing

 			Noise	N( 1 );
			// Draw scratches
			for ( int i=0; i < 10; i++ )
			{
				NjFloat2	Pos;
				Pos.x = _frand( 0.0f, 512.0f );
				Pos.y = _frand( 0.0f, 512.0f );
				NjFloat2	Dir;
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

				NjFloat2	Pos;
				Pos.x = _frand( -512.0f, 512.0f );
				Pos.y = _frand( -512.0f, 512.0f );

				NjFloat2	Size;
				Size.x = _frand( 40.0f, 50.0f );
				Size.y = _frand( 40.0f, 50.0f );
				float	DeltaSize = _frand( 0.0f, 30.0f );	// Splotches get bigger or smaller with time
						DeltaSize /= Count;

				float	Step = _frand( 0.25f, 1.0f ) * Size.x;	// Splotches interpenetrate

				for ( int X=0; X < Count; X++ )
				{
					Draw.SetupContext( Pos.x, Pos.y, Angle );
					Draw.DrawRectangle( 0.0f, 0.0f, Size.x, Size.y, 0.1f * Size.Min(), 0.0f, FillSplotch, &N );

					Angle += DeltaAngle;
					float	AngleRad = DEG2RAD( Angle );
					Pos = Pos + Step * NjFloat2( cosf(AngleRad), sinf(AngleRad) );
					Size.x += DeltaSize;
				}
			}

// Draw.DrawRectangle( 0.0f, 0.0f, 100.0f, 100.0f, 10.0f, 0.0f, FillSplotch, &N );

//			Filters::BrightnessContrastGamma( TB, 0.1f, 0.7f, 2.0f );
//			Filters::Erode( TB, 3 );
// * /

			gs_pTexTestNoise = new Texture2D( gs_Device, 512, 512, 1, PixelFormatRGBA16F::DESCRIPTOR, 0, TB.Convert( PixelFormatRGBA16F::DESCRIPTOR ) );
		}
*/
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


		{	// Build some spheres
			GeometryBuilder::MapperSpherical	Mapper( 4.0f, 2.0f );

			gs_pPrimSphereInternal = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
			GeometryBuilder::BuildSphere( 20, 10, *gs_pPrimSphereInternal, Mapper );

			gs_pPrimSphereExternal = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
			GeometryBuilder::BuildSphere( 20, 10, *gs_pPrimSphereExternal, Mapper );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Create materials
	{
		CHECK_MATERIAL( gs_pMatPostFinal = CreateMaterial( IDR_SHADER_POST_FINAL, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 1001 );
		CHECK_MATERIAL( gs_pMatTestDisplay = CreateMaterial( IDR_SHADER_TEST_DISPLAY, VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, "PS" ), 1002 );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create constant buffers
	{
		gs_pCB_Test = new CB<CBTest>( gs_Device );
		gs_pCB_Object = new CB<CBObject>( gs_Device );
	}

	return 0;
}

void	IntroExit()
{
	// Release constant buffers
	delete gs_pCB_Object;
	delete gs_pCB_Test;

	// Release materials
 	delete gs_pMatTestDisplay;
 	delete gs_pMatPostFinal;

	// Release primitives
	delete gs_pPrimSphereInternal;
	delete gs_pPrimSphereExternal;
	delete gs_pPrimQuad;

	// Release render targets & textures
	Delete2DTextures();
	delete gs_pRTHDR;

	// Release the camera
	delete gs_pCamera;
}

bool	IntroDo( float _Time, float _DeltaTime )
{
//	gs_Device.ClearRenderTarget( gs_Device.DefaultRenderTarget(), NjFloat4( 0.5f, 0.5f, 0.5f, 1.0f ) );
	gs_Device.ClearRenderTarget( *gs_pRTHDR, NjFloat4( 0.5f, 0.4f, 0.25f, 1.0f ) );
	gs_Device.ClearDepthStencil( gs_Device.DefaultDepthStencil(), 1.0f, 0 );

	//////////////////////////////////////////////////////////////////////////
	// Update the camera settings and upload its data to the shaders

	// TODO: Animate camera...

	gs_pCamera->Upload( 0 );


	//////////////////////////////////////////////////////////////////////////
	// Render some shit to the HDR buffer
	gs_Device.SetRenderTarget( *gs_pRTHDR, &gs_Device.DefaultDepthStencil() );
	USING_MATERIAL_START( *gs_pMatTestDisplay )

		gs_Device.SetStates( *gs_Device.m_pRS_CullNone, *gs_Device.m_pDS_Disabled, *gs_Device.m_pBS_Disabled );
//		gs_Device.SetStates( *gs_Device.m_pRS_CullBack, *gs_Device.m_pDS_ReadWriteLess, *gs_Device.m_pBS_Disabled );

		gs_pCB_Object->m.Local2World = NjFloat4x4::PRS( NjFloat3::Zero, NjFloat4::QuatFromAngleAxis( 0.0f, NjFloat3::UnitY ), NjFloat3::One );
		gs_pCB_Object->UpdateData();
		gs_pCB_Object->Set( 1 );

		gs_pPrimSphereInternal->Render( *gs_pMatTestDisplay );

	USING_MATERIAL_END

	// Setup default states
	gs_Device.SetStates( *gs_Device.m_pRS_CullNone, *gs_Device.m_pDS_Disabled, *gs_Device.m_pBS_Disabled );

	// Render to screen
	USING_MATERIAL_START( *gs_pMatPostFinal )
//		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget(), &gs_Device.DefaultDepthStencil() );
		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget() );

		gs_pCB_Test->m.LOD = 10.0f * (1.0f - fabs( sinf( _Time ) ));
//gs_CBTest.LOD = 0.0f;

		gs_pCB_Test->UpdateData();
		gs_pCB_Test->SetPS( 1 );

		gs_pTexTestNoise->SetPS( 0 );
		gs_pRTHDR->SetPS( 1 );

		gs_pPrimQuad->Render( M );

	USING_MATERIAL_END

	// Present !
	gs_Device.DXSwapChain().Present( 0, 0 );

	return true;	// True means continue !
}
