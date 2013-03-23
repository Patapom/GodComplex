#include "../../GodComplex.h"
#include "EffectVolumetric.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

const float	EffectVolumetric::SCREEN_TARGET_RATIO = 1.0f;

EffectVolumetric::EffectVolumetric( Device& _Device, Primitive& _ScreenQuad ) : m_Device( _Device ), m_ScreenQuad( _ScreenQuad ), m_ErrorCode( 0 )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
 	CHECK_MATERIAL( m_pMatDepthWrite = CreateMaterial( IDR_SHADER_VOLUMETRIC_DEPTH_WRITE, VertexFormatP3::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
 	CHECK_MATERIAL( m_pMatComputeTransmittance = CreateMaterial( IDR_SHADER_VOLUMETRIC_COMPUTE_TRANSMITTANCE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 2 );
 	CHECK_MATERIAL( m_pMatDisplay = CreateMaterial( IDR_SHADER_VOLUMETRIC_DISPLAY, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 3 );
 	CHECK_MATERIAL( m_pMatCombine = CreateMaterial( IDR_SHADER_VOLUMETRIC_COMBINE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 4 );

//	const char*	pCSO = LoadCSO( "./Resources/Shaders/CSO/VolumetricCombine.cso" );
//	CHECK_MATERIAL( m_pMatCombine = CreateMaterial( IDR_SHADER_VOLUMETRIC_COMBINE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, pCSO ), 4 );
//	delete[] pCSO;
	

	//////////////////////////////////////////////////////////////////////////
	// Build the box primitive
	{
		m_pPrimBox = new Primitive( m_Device, VertexFormatP3::DESCRIPTOR );
		GeometryBuilder::BuildCube( 1, 1, 1, *m_pPrimBox );
	}

	//////////////////////////////////////////////////////////////////////////
	// Build textures & render targets
	m_pRTTransmittanceZ = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, PixelFormatRG16F::DESCRIPTOR, 1, NULL );
	m_pRTTransmittanceMap = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 2, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );

	int	W = m_Device.DefaultRenderTarget().GetWidth();
	int	H = m_Device.DefaultRenderTarget().GetHeight();
	m_RenderWidth = int( ceilf( W * SCREEN_TARGET_RATIO ) );
	m_RenderHeight = int( ceilf( H * SCREEN_TARGET_RATIO ) );

	m_pRTRenderZ = new Texture2D( m_Device, m_RenderWidth, m_RenderHeight, 1, PixelFormatRG16F::DESCRIPTOR, 1, NULL );
	m_pRTRender = new Texture2D( m_Device, m_RenderWidth, m_RenderHeight, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );

//	m_pTexFractal0 = BuildFractalTexture( true );
	m_pTexFractal1 = BuildFractalTexture( false );
	
	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_Object = new CB<CBObject>( m_Device, 10 );
	m_pCB_Splat = new CB<CBSplat>( m_Device, 10 );
	m_pCB_Shadow = new CB<CBShadow>( m_Device, 11 );


	//////////////////////////////////////////////////////////////////////////
	// Setup our volume & light
	m_Position = NjFloat3( 0.0f, 2.0f, 0.0f );
	m_Rotation = NjFloat4::QuatFromAngleAxis( 0.0f, NjFloat3::UnitY );
	m_Scale = NjFloat3( 1.0f, 2.0f, 1.0f );

	m_LightDirection = NjFloat3( 1, 1, 1 );
	m_LightDirection.Normalize();
}

EffectVolumetric::~EffectVolumetric()
{
	delete m_pCB_Shadow;
	delete m_pCB_Splat;
	delete m_pCB_Object;

	delete m_pTexFractal1;
	delete m_pTexFractal0;
	delete m_pRTRender;
	delete m_pRTRenderZ;
	delete m_pRTTransmittanceMap;
	delete m_pRTTransmittanceZ;

	delete m_pPrimBox;

 	delete m_pMatCombine;
 	delete m_pMatDisplay;
 	delete m_pMatComputeTransmittance;
	delete m_pMatDepthWrite;
}

void	EffectVolumetric::Render( float _Time, float _DeltaTime, Camera& _Camera )
{
// DEBUG
//m_LightDirection.Set( cosf(_Time), 2.0f * sinf( 0.324f * _Time ), sinf( _Time ) );
//m_LightDirection.Set( cosf(_Time), 1.0f, sinf( _Time ) );
m_LightDirection.Set( 0, 1, 0 );
// DEBUG

	D3DPERF_BeginEvent( D3DCOLOR( 0xFF00FF00 ), L"Compute Shadow" );

	if ( m_pTexFractal0 != NULL )
		m_pTexFractal0->SetPS( 16 );
	if ( m_pTexFractal1 != NULL )
		m_pTexFractal1->SetPS( 17 );

	//////////////////////////////////////////////////////////////////////////
	// 1] Compute transforms
m_Position.Set( 0, 1.0f, -20 );
m_Scale.Set( 128.0f, 1.0f, 128.0f );	// 64
	m_Box2World.PRS( m_Position, m_Rotation, m_Scale );

	ComputeShadowTransform();

	m_pCB_Shadow->UpdateData();

	D3DPERF_EndEvent();

	//////////////////////////////////////////////////////////////////////////
	// 2] Compute the transmittance function map

	// 2.1] Render front & back depths
	D3DPERF_BeginEvent( D3DCOLOR( 0xFF000000 ), L"Render TFM Z" );

	m_Device.ClearRenderTarget( *m_pRTTransmittanceZ, NjFloat4( 1e4f, -1e4f, 0.0f, 0.0f ) );

	USING_MATERIAL_START( *m_pMatDepthWrite )

		m_Device.SetRenderTarget( *m_pRTTransmittanceZ );

		m_pCB_Object->m.Local2View = m_Box2World * m_World2Light;
		m_pCB_Object->m.View2Proj = m_Light2ShadowNormalized; // Here we use the alternate projection matrix that actually scales Z in [0,1] for ZBuffer compliance (since original World2Shadow keeps the Z in world units)
		m_pCB_Object->m.dUV = m_pRTTransmittanceZ->GetdUV();
		m_pCB_Object->UpdateData();

		D3DPERF_SetMarker(  D3DCOLOR( 0x00FF00FF ), L"Render Front Faces" );

	 	m_Device.SetStates( m_Device.m_pRS_CullFront, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled_RedOnly );
		m_pPrimBox->Render( M );

		D3DPERF_SetMarker(  D3DCOLOR( 0xFFFF00FF ), L"Render Back Faces" );

	 	m_Device.SetStates( m_Device.m_pRS_CullBack, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled_GreenOnly );
		m_pPrimBox->Render( M );

	USING_MATERIAL_END

	D3DPERF_EndEvent();

	// 2.2] Compute transmittance map
	D3DPERF_BeginEvent( D3DCOLOR( 0xFF400000 ), L"Render TFM" );

	m_Device.ClearRenderTarget( *m_pRTTransmittanceMap, NjFloat4( 0.0f, 0.0f, 0.0f, 0.0f ) );

	ID3D11RenderTargetView*	ppViews[2] = {
		m_pRTTransmittanceMap->GetTargetView( 0, 0, 1 ),
		m_pRTTransmittanceMap->GetTargetView( 0, 1, 1 ),
	};
	m_Device.SetRenderTargets( m_pRTTransmittanceMap->GetWidth(), m_pRTTransmittanceMap->GetHeight(), 2, ppViews );

	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );

	USING_MATERIAL_START( *m_pMatComputeTransmittance )

		m_pRTTransmittanceZ->SetPS( 10 );

		m_pCB_Splat->m.dUV = m_pRTTransmittanceMap->GetdUV();
		m_pCB_Splat->UpdateData();

		m_ScreenQuad.Render( M );

	USING_MATERIAL_END

	// Remove contention on Transmittance Z that we don't need next...
	m_Device.RemoveShaderResources( 10 );

	D3DPERF_EndEvent();

	//////////////////////////////////////////////////////////////////////////
	// 3] Render the actual volume

	// 3.1] Render front & back depths
	D3DPERF_BeginEvent( D3DCOLOR( 0xFF800000 ), L"Render Volume Z" );

	m_Device.ClearRenderTarget( *m_pRTRenderZ, NjFloat4( 0.0f, -1e4f, 0.0f, 0.0f ) );

	USING_MATERIAL_START( *m_pMatDepthWrite )

		m_Device.SetRenderTarget( *m_pRTRenderZ );

		m_pCB_Object->m.Local2View = m_Box2World * _Camera.GetCB().World2Camera;
		m_pCB_Object->m.View2Proj = _Camera.GetCB().Camera2Proj;
		m_pCB_Object->m.dUV = m_pRTRenderZ->GetdUV();
		m_pCB_Object->UpdateData();

		D3DPERF_SetMarker(  D3DCOLOR( 0x00FF00FF ), L"Render Front Faces" );

	 	m_Device.SetStates( m_Device.m_pRS_CullBack, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled_RedOnly );
		m_pPrimBox->Render( M );

		D3DPERF_SetMarker(  D3DCOLOR( 0xFFFF00FF ), L"Render Back Faces" );

	 	m_Device.SetStates( m_Device.m_pRS_CullFront, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled_GreenOnly );
		m_pPrimBox->Render( M );

	USING_MATERIAL_END

	D3DPERF_EndEvent();

	// 3.2] Render the actual volume
	D3DPERF_BeginEvent( D3DCOLOR( 0xFFC00000 ), L"Render Volume" );

	m_Device.ClearRenderTarget( *m_pRTRender, NjFloat4( 0.0f, 0.0f, 0.0f, 1.0f ) );
	m_Device.SetRenderTarget( *m_pRTRender );
	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );

	USING_MATERIAL_START( *m_pMatDisplay )

		m_pRTRenderZ->SetPS( 10 );
		m_pRTTransmittanceMap->SetPS( 11 );

		m_pCB_Splat->m.dUV = m_pRTRender->GetdUV();
		m_pCB_Splat->UpdateData();

		m_ScreenQuad.Render( M );

	USING_MATERIAL_END

	D3DPERF_EndEvent();

	//////////////////////////////////////////////////////////////////////////
	// 4] Combine with screen
	D3DPERF_BeginEvent( D3DCOLOR( 0xFFFF0000 ), L"Render TFM Z" );

	m_Device.SetRenderTarget( m_Device.DefaultRenderTarget(), NULL );
	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );

	USING_MATERIAL_START( *m_pMatCombine )

		m_pCB_Splat->m.dUV = m_Device.DefaultRenderTarget().GetdUV();
		m_pCB_Splat->UpdateData();

// DEBUG
m_pRTRender->SetPS( 10 );
//m_pRTTransmittanceZ->SetPS( 11 );
m_pRTRenderZ->SetPS( 11 );
m_pRTTransmittanceMap->SetPS( 12 );
// DEBUG

		m_ScreenQuad.Render( M );

	USING_MATERIAL_END

	D3DPERF_EndEvent();
}

void	EffectVolumetric::ComputeShadowTransform()
{
	// Build basis for directional light
	m_LightDirection.Normalize();
	NjFloat3	X, Y;
	if ( fabs( m_LightDirection.y - 1.0f ) < 1e-3f )
	{	// Special case
		X = NjFloat3::UnitX;
		Y = NjFloat3::UnitZ;
	}
	else
	{
		X = m_LightDirection ^ NjFloat3::UnitY;
		X.Normalize();
		Y = X ^ m_LightDirection;
	}

	// Project the box's corners to the light plane
	NjFloat3	Center = NjFloat3::Zero;
	NjFloat3	CornerLocal, CornerWorld, CornerLight;
	NjFloat3	Min( FLOAT32_MAX, FLOAT32_MAX, FLOAT32_MAX ), Max( -FLOAT32_MAX, -FLOAT32_MAX, -FLOAT32_MAX );
	for ( int CornerIndex=0; CornerIndex < 8; CornerIndex++ )
	{
		CornerLocal.x = 2.0f * (CornerIndex & 1) - 1.0f;
		CornerLocal.y = 2.0f * ((CornerIndex >> 1) & 1) - 1.0f;
		CornerLocal.z = 2.0f * ((CornerIndex >> 2) & 1) - 1.0f;

		CornerWorld = NjFloat4( CornerLocal, 1 ) * m_Box2World;

		Center = Center + CornerWorld;

		CornerLight.x = CornerWorld | X;
		CornerLight.y = CornerWorld | Y;
		CornerLight.z = CornerWorld | m_LightDirection;

		Min = Min.Min( CornerLight );
		Max = Max.Max( CornerLight );
	}
	Center = Center / 8.0f;

	NjFloat3	SizeLight = Max - Min;

	// Offset center to Z start of box
	float	CenterZ = Center | m_LightDirection;	// How much the center is already in light direction?
	Center = Center + (Max.z-CenterZ) * m_LightDirection;

	// Build LIGHT => WORLD transform & inverse
	NjFloat4x4	Light2World;
	Light2World.SetRow( 0, X, 0 );
	Light2World.SetRow( 1, Y, 0 );
	Light2World.SetRow( 2, -m_LightDirection, 0 );
	Light2World.SetRow( 3, Center, 1 );

	m_World2Light = Light2World.Inverse();

	// Build projection transforms
	NjFloat4x4	Shadow2Light;
	Shadow2Light.SetRow( 0, NjFloat4( 0.5f * SizeLight.x, 0, 0, 0 ) );
	Shadow2Light.SetRow( 1, NjFloat4( 0, 0.5f * SizeLight.y, 0, 0 ) );
	Shadow2Light.SetRow( 2, NjFloat4( 0, 0, 1, 0 ) );
//	Shadow2Light.SetRow( 3, NjFloat4( 0, 0, -0.5f * SizeLight.z, 1 ) );
	Shadow2Light.SetRow( 3, NjFloat4( 0, 0, 0, 1 ) );

	NjFloat4x4	Light2Shadow = Shadow2Light.Inverse();

	m_pCB_Shadow->m.LightDirection = NjFloat4( m_LightDirection, 0 );
	m_pCB_Shadow->m.World2Shadow = m_World2Light * Light2Shadow;
	m_pCB_Shadow->m.Shadow2World = Shadow2Light * Light2World;
	m_pCB_Shadow->m.ZMax.Set( SizeLight.z, 1.0f / SizeLight.z );


	// Create an alternate projection matrix that doesn't keep the World Z but instead projects it in [0,1]
	Shadow2Light.SetRow( 2, NjFloat4( 0, 0, SizeLight.z, 0 ) );
//	Shadow2Light.SetRow( 3, NjFloat4( 0, 0, -0.5f * SizeLight.z, 1 ) );
	Shadow2Light.SetRow( 3, NjFloat4( 0, 0, 0, 1 ) );

	m_Light2ShadowNormalized = Shadow2Light.Inverse();


// CHECK
NjFloat3	CheckMin( FLOAT32_MAX, FLOAT32_MAX, FLOAT32_MAX ), CheckMax( -FLOAT32_MAX, -FLOAT32_MAX, -FLOAT32_MAX );
NjFloat3	CornerShadow;
for ( int CornerIndex=0; CornerIndex < 8; CornerIndex++ )
{
	CornerLocal.x = 2.0f * (CornerIndex & 1) - 1.0f;
	CornerLocal.y = 2.0f * ((CornerIndex >> 1) & 1) - 1.0f;
	CornerLocal.z = 2.0f * ((CornerIndex >> 2) & 1) - 1.0f;

	CornerWorld = NjFloat4( CornerLocal, 1 ) * m_Box2World;

 	CornerShadow = NjFloat4( CornerWorld, 1 ) * m_pCB_Shadow->m.World2Shadow;
//	CornerShadow = NjFloat4( CornerWorld, 1 ) * m_World2ShadowNormalized;

	NjFloat4	CornerWorldAgain = NjFloat4( CornerShadow, 1 ) * m_pCB_Shadow->m.Shadow2World;

	CheckMin = CheckMin.Min( CornerShadow );
	CheckMax = CheckMax.Max( CornerShadow );
}
// CHECK

// CHECK
NjFloat3	Test0 = NjFloat4( 0.0f, 0.0f, 0.5f * SizeLight.z, 1.0f ) * m_pCB_Shadow->m.Shadow2World;
NjFloat3	Test1 = NjFloat4( 0.0f, 0.0f, 0.0f, 1.0f ) * m_pCB_Shadow->m.Shadow2World;
NjFloat3	DeltaTest0 = Test1 - Test0;
NjFloat3	Test2 = NjFloat4( 0.0f, 0.0f, SizeLight.z, 1.0f ) * m_pCB_Shadow->m.Shadow2World;
NjFloat3	DeltaTest1 = Test2 - Test0;
// CHECK
}

//////////////////////////////////////////////////////////////////////////
// Builds a fractal texture compositing several octaves of tiling Perlin noise
// Successive mips don't average previous mips but rather attenuate higher octaves
namespace
{
	float	CombineDistances( float _pSqDistances[], int _pCellX[], int _pCellY[], int _pCellZ[], void* _pData )
	{
		return _pSqDistances[0];
	}
}

#define USE_WIDE_NOISE
#ifdef USE_WIDE_NOISE

// ========================================================================
// Since we're using a very thin slab of volume, vertical precision is not necessary
// The scale of the world=>noise transform is World * 0.05, meaning the noise will tile every 20 world units.
// The cloud slab is 2 world units thick, meaning 10% of its vertical size is used.
// That also means that we can reuse 90% of its volume and convert it into a surface.
//
// The total volume is 128x128x128. The actually used volume is 128*128*13 (13 is 10% of 128).
// Keeping the height at 16 (rounding 13 up to next POT), the available surface is 131072.
//
// This yields a final teture size of 360x360x16
//
Texture3D*	EffectVolumetric::BuildFractalTexture( bool _bBuildFirst )
{
	Noise*	pNoises[FRACTAL_OCTAVES];
	float	NoiseFrequency = 0.0001f;
	float	FrequencyFactor = 2.0f;
	float	AmplitudeFactor = _bBuildFirst ? 0.707f : 0.707f;

	for ( int OctaveIndex=0; OctaveIndex < FRACTAL_OCTAVES; OctaveIndex++ )
	{
		pNoises[OctaveIndex] = new Noise( _bBuildFirst ? 1+OctaveIndex : 37951+OctaveIndex );
		pNoises[OctaveIndex]->SetWrappingParameters( NoiseFrequency, 198746+OctaveIndex );
		NoiseFrequency *= FrequencyFactor;
	}

static const int TEXTURE_SIZE_XY = 360;
static const int TEXTURE_SIZE_Z = 16;
static const int TEXTURE_MIPS = 5;		// Max mips is the lowest dimension's mip

	int		SizeXY = TEXTURE_SIZE_XY;
	int		SizeZ = TEXTURE_SIZE_Z;

	float	Normalizer = 0.0f;
	float	Amplitude = 1.0f;
	for ( int OctaveIndex=1; OctaveIndex < FRACTAL_OCTAVES; OctaveIndex++ )
	{
		Normalizer += Amplitude;
		Amplitude *= AmplitudeFactor;
	}
	Normalizer = 1.0f / Normalizer;

	float**	ppMips = new float*[TEXTURE_MIPS];

	// Build first mip
	ppMips[0] = new float[SizeXY*SizeXY*SizeZ];

#if 0
	NjFloat3	UVW;
	for ( int Z=0; Z < SizeZ; Z++ )
	{
		UVW.z = float( Z ) / SizeXY;	// Here we keep a cubic aspect ratio for voxels so we also divide by the same size as other dimensions: we don't want the noise to quickly loop vertically!
		float*	pSlice = ppMips[0] + SizeXY*SizeXY*Z;
		for ( int Y=0; Y < SizeXY; Y++ )
		{
			UVW.y = float( Y ) / SizeXY;
			float*	pScanline = pSlice + SizeXY * Y;
			for ( int X=0; X < SizeXY; X++ )
			{
				UVW.x = float( X ) / SizeXY;

				float	V = 0.0f;
				float	Amplitude = 1.0f;
				for ( int OctaveIndex=0; OctaveIndex < FRACTAL_OCTAVES; OctaveIndex++ )
				{
					V += Amplitude * pNoises[OctaveIndex]->WrapPerlin( UVW );
					Amplitude *= AmplitudeFactor;
				}
				V *= Normalizer;
				*pScanline++ = V;
			}
		}
	}
	FILE*	pFile = NULL;
	fopen_s( &pFile, _bBuildFirst ? "FractalNoise0.float" : "FractalNoise1.float", "wb" );
	ASSERT( pFile != NULL, "Couldn't write fractal file!" );
	fwrite( ppMips[0], sizeof(float), SizeXY*SizeXY*SizeZ, pFile );
	fclose( pFile );
#else
	FILE*	pFile = NULL;
	fopen_s( &pFile, _bBuildFirst ? "FractalNoise0.float" : "FractalNoise1.float", "rb" );
	ASSERT( pFile != NULL, "Couldn't load fractal file!" );
	fread_s( ppMips[0], SizeXY*SizeXY*SizeZ*sizeof(float), sizeof(float), SizeXY*SizeXY*SizeZ, pFile );
	fclose( pFile );
#endif


	// Build other mips
	for ( int MipIndex=1; MipIndex < TEXTURE_MIPS; MipIndex++ )
	{
		int		SourceSizeXY = SizeXY;
		int		SourceSizeZ = SizeZ;
		SizeXY = MAX( 1, SizeXY >> 1 );
		SizeZ = MAX( 1, SizeZ >> 1 );

		float*	pSource = ppMips[MipIndex-1];
		float*	pTarget = new float[SizeXY*SizeXY*SizeZ];
		ppMips[MipIndex] = pTarget;

		for ( int Z=0; Z < SizeZ; Z++ )
		{
			float*	pSlice0 = pSource + SourceSizeXY*SourceSizeXY*(2*Z);
			float*	pSlice1 = pSource + SourceSizeXY*SourceSizeXY*(2*Z+1);
			float*	pSliceT = pTarget + SizeXY*SizeXY*Z;
			for ( int Y=0; Y < SizeXY; Y++ )
			{
				float*	pScanline00 = pSlice0 + SourceSizeXY * (2*Y);
				float*	pScanline01 = pSlice0 + SourceSizeXY * (2*Y+1);
				float*	pScanline10 = pSlice1 + SourceSizeXY * (2*Y);
				float*	pScanline11 = pSlice1 + SourceSizeXY * (2*Y+1);
				float*	pScanlineT = pSliceT + SizeXY*Y;
				for ( int X=0; X < SizeXY; X++ )
				{
					float	V  = pScanline00[0] + pScanline00[1];	// From slice 0, current line
							V += pScanline01[0] + pScanline01[1];	// From slice 0, next line
							V += pScanline10[0] + pScanline10[1];	// From slice 1, current line
							V += pScanline11[0] + pScanline11[1];	// From slice 1, next line
					V *= 0.125f;

					*pScanlineT++ = V;

					pScanline00 += 2;
					pScanline01 += 2;
					pScanline10 += 2;
					pScanline11 += 2;
				}
			}
		}
	}

	// Build actual texture
	Texture3D*	pResult = new Texture3D( m_Device, TEXTURE_SIZE_XY, TEXTURE_SIZE_XY, TEXTURE_SIZE_Z, PixelFormatR32F::DESCRIPTOR, TEXTURE_MIPS, (void**) ppMips );

	for ( int MipIndex=0; MipIndex < TEXTURE_MIPS; MipIndex++ )
		delete[] ppMips[MipIndex];
	delete[] ppMips;

	for ( int OctaveIndex=0; OctaveIndex < FRACTAL_OCTAVES; OctaveIndex++ )
		delete pNoises[OctaveIndex];

	return pResult;
}

#else

Texture3D*	EffectVolumetric::BuildFractalTexture( bool _bBuildFirst )
{
	Noise*	pNoises[FRACTAL_OCTAVES];
	float	NoiseFrequency = 0.0001f;
	float	FrequencyFactor = 2.0f;
	float	AmplitudeFactor = _bBuildFirst ? 0.707f : 0.707f;

	for ( int OctaveIndex=0; OctaveIndex < FRACTAL_OCTAVES; OctaveIndex++ )
	{
		pNoises[OctaveIndex] = new Noise( _bBuildFirst ? 1+OctaveIndex : 37951+OctaveIndex );
		pNoises[OctaveIndex]->SetWrappingParameters( NoiseFrequency, 198746+OctaveIndex );

		int	CellsCount = 4 << OctaveIndex;
		pNoises[OctaveIndex]->SetCellularWrappingParameters( CellsCount, CellsCount, CellsCount );
		NoiseFrequency *= FrequencyFactor;
	}

	int		Size = 1 << FRACTAL_TEXTURE_POT;
	int		MipsCount = 1+FRACTAL_TEXTURE_POT;

	float	Normalizer = 0.0f;
	float	Amplitude = 1.0f;
	for ( int OctaveIndex=1; OctaveIndex < FRACTAL_OCTAVES; OctaveIndex++ )
	{
		Normalizer += Amplitude;
		Amplitude *= AmplitudeFactor;
	}
	Normalizer = 1.0f / Normalizer;

	float**	ppMips = new float*[MipsCount];

	// Build first mip
	ppMips[0] = new float[Size*Size*Size];

//#define USE_CELLULAR_NOISE
#if defined(USE_CELLULAR_NOISE)
// ========================================================================
#if 0
	NjFloat3	UVW;
	for ( int Z=0; Z < Size; Z++ )
	{
		UVW.z = float( Z ) / Size;
		float*	pSlice = ppMips[0] + Size*Size*Z;
		for ( int Y=0; Y < Size; Y++ )
		{
			UVW.y = float( Y ) / Size;
			float*	pScanline = pSlice + Size * Y;
			for ( int X=0; X < Size; X++ )
			{
				UVW.x = float( X ) / Size;

				float	V = 0.0f;
				float	Amplitude = 1.0f;
				float	Frequency = 1.0f;
				for ( int OctaveIndex=0; OctaveIndex < FRACTAL_OCTAVES; OctaveIndex++ )
				{
					V += Amplitude * pNoises[OctaveIndex]->Worley( Frequency * UVW, CombineDistances, NULL, true );
					Amplitude *= AmplitudeFactor;
//					Frequency *= FrequencyFactor;
				}
				V *= Normalizer;
				*pScanline++ = V;
			}
		}
	}
	FILE*	pFile = NULL;
	fopen_s( &pFile, "CellularNoise.float", "wb" );
	ASSERT( pFile != NULL, "Couldn't write cellular file!" );
	fwrite( ppMips[0], sizeof(float), Size*Size*Size, pFile );
	fclose( pFile );
#else
	FILE*	pFile = NULL;
	fopen_s( &pFile, "CellularNoise.float", "rb" );
	ASSERT( pFile != NULL, "Couldn't load cellular file!" );
	fread_s( ppMips[0], Size*Size*Size*sizeof(float), sizeof(float), Size*Size*Size, pFile );
	fclose( pFile );
#endif

#else
// ========================================================================
#if 0
	NjFloat3	UVW;
	for ( int Z=0; Z < Size; Z++ )
	{
		UVW.z = float( Z ) / Size;
		float*	pSlice = ppMips[0] + Size*Size*Z;
		for ( int Y=0; Y < Size; Y++ )
		{
			UVW.y = float( Y ) / Size;
			float*	pScanline = pSlice + Size * Y;
			for ( int X=0; X < Size; X++ )
			{
				UVW.x = float( X ) / Size;

				float	V = 0.0f;
				float	Amplitude = 1.0f;
				float	Frequency = 1.0f;
				for ( int OctaveIndex=0; OctaveIndex < FRACTAL_OCTAVES; OctaveIndex++ )
				{
					V += Amplitude * pNoises[OctaveIndex]->WrapPerlin( Frequency * UVW );
					Amplitude *= AmplitudeFactor;
//					Frequency *= FrequencyFactor;
				}
				V *= Normalizer;
				*pScanline++ = V;
			}
		}
	}
	FILE*	pFile = NULL;
	fopen_s( &pFile, _bBuildFirst ? "FractalNoise0.float" : "FractalNoise1.float", "wb" );
	ASSERT( pFile != NULL, "Couldn't write fractal file!" );
	fwrite( ppMips[0], sizeof(float), Size*Size*Size, pFile );
	fclose( pFile );
#else
	FILE*	pFile = NULL;
	fopen_s( &pFile, _bBuildFirst ? "FractalNoise0.float" : "FractalNoise1.float", "rb" );
	ASSERT( pFile != NULL, "Couldn't load fractal file!" );
	fread_s( ppMips[0], Size*Size*Size*sizeof(float), sizeof(float), Size*Size*Size, pFile );
	fclose( pFile );
#endif

#endif

	// Build other mips
	for ( int MipIndex=1; MipIndex < MipsCount; MipIndex++ )
	{
		int		SourceSize = Size;
		Size >>= 1;

		float*	pSource = ppMips[MipIndex-1];
		float*	pTarget = new float[Size*Size*Size];
		ppMips[MipIndex] = pTarget;

		for ( int Z=0; Z < Size; Z++ )
		{
			float*	pSlice0 = pSource + SourceSize*SourceSize*(2*Z);
			float*	pSlice1 = pSource + SourceSize*SourceSize*(2*Z+1);
			float*	pSliceT = pTarget + Size*Size*Z;
			for ( int Y=0; Y < Size; Y++ )
			{
				float*	pScanline00 = pSlice0 + SourceSize * (2*Y);
				float*	pScanline01 = pSlice0 + SourceSize * (2*Y+1);
				float*	pScanline10 = pSlice1 + SourceSize * (2*Y);
				float*	pScanline11 = pSlice1 + SourceSize * (2*Y+1);
				float*	pScanlineT = pSliceT + Size*Y;
				for ( int X=0; X < Size; X++ )
				{
					float	V  = pScanline00[0] + pScanline00[1];	// From slice 0, current line
							V += pScanline01[0] + pScanline01[1];	// From slice 0, next line
							V += pScanline10[0] + pScanline10[1];	// From slice 1, current line
							V += pScanline11[0] + pScanline11[1];	// From slice 1, next line
					V *= 0.125f;

					*pScanlineT++ = V;

					pScanline00 += 2;
					pScanline01 += 2;
					pScanline10 += 2;
					pScanline11 += 2;
				}
			}
		}
	}
	Size = 1 << FRACTAL_TEXTURE_POT;	// Restore size after mip modifications

	// Build actual texture
	Texture3D*	pResult = new Texture3D( m_Device, Size, Size, Size, PixelFormatR32F::DESCRIPTOR, 0, (void**) ppMips );

	for ( int MipIndex=0; MipIndex < MipsCount; MipIndex++ )
		delete[] ppMips[MipIndex];
	delete[] ppMips;

	for ( int OctaveIndex=0; OctaveIndex < FRACTAL_OCTAVES; OctaveIndex++ )
		delete pNoises[OctaveIndex];

	return pResult;
}

#endif