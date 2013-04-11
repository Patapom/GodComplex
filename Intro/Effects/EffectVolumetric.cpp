#include "../../GodComplex.h"
#include "EffectVolumetric.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

const float	EffectVolumetric::SCREEN_TARGET_RATIO = 0.5f;

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
	// Pre-Compute multiple-scattering tables
	PreComputeSkyTables();

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
	m_pCB_Volume = new CB<CBVolume>( m_Device, 12 );

	m_pCB_Volume->m.Params.Set( 0, 0, 0, 0 );

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
	delete m_pCB_Volume;
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

	FreeSkyTables();

 	delete m_pMatCombine;
 	delete m_pMatDisplay;
 	delete m_pMatComputeTransmittance;
	delete m_pMatDepthWrite;
}

#ifdef _DEBUG
#define PERF_BEGIN_EVENT( Color, Text )	D3DPERF_BeginEvent( Color, Text )
#define PERF_END_EVENT()				D3DPERF_EndEvent()
#define PERF_MARKER( Color, Text )		D3DPERF_SetMarker( Color, Text )
#else
#define PERF_BEGIN_EVENT( Color, Text )
#define PERF_END_EVENT()
#define PERF_MARKER( Color, Text )
#endif

void	EffectVolumetric::Render( float _Time, float _DeltaTime, Camera& _Camera )
{
// DEBUG
float	t = 0;//0.05f * _Time;
//m_LightDirection.Set( 0, 1, -1 );
//m_LightDirection.Set( 1, 2.0, -5 );
//m_LightDirection.Set( cosf(_Time), 2.0f * sinf( 0.324f * _Time ), sinf( _Time ) );
m_LightDirection.Set( sinf(t), 2.0f * sinf( 0.4f + 4.0f * 0.324f * t ), -cosf( t ) );	// Fast vertical change
//m_LightDirection.Set( cosf(_Time), 1.0f, sinf( _Time ) );
// DEBUG

	PERF_BEGIN_EVENT( D3DCOLOR( 0xFF00FF00 ), L"Compute Shadow" );

#ifdef _DEBUG
	if ( gs_WindowInfos.pKeys[VK_NUMPAD1] )
		m_pCB_Volume->m.Params.x -= 0.5f * _DeltaTime;
	if ( gs_WindowInfos.pKeys[VK_NUMPAD7] )
		m_pCB_Volume->m.Params.x += 0.5f * _DeltaTime;

	m_pCB_Volume->m.Params.y = gs_WindowInfos.pKeys[VK_RETURN];
#endif

	m_pCB_Volume->UpdateData();

	if ( m_pTexFractal0 != NULL )
		m_pTexFractal0->SetPS( 16 );
	if ( m_pTexFractal1 != NULL )
		m_pTexFractal1->SetPS( 17 );

	//////////////////////////////////////////////////////////////////////////
	// 1] Compute transforms
m_Position.Set( 0, 4.0f, -20 );
m_Scale.Set( 128.0f, 4.0f, 128.0f );
	m_Box2World.PRS( m_Position, m_Rotation, m_Scale );

	ComputeShadowTransform();

	m_pCB_Shadow->UpdateData();

	PERF_END_EVENT();

	//////////////////////////////////////////////////////////////////////////
	// 2] Compute the transmittance function map

	// 2.1] Render front & back depths
	PERF_BEGIN_EVENT( D3DCOLOR( 0xFF000000 ), L"Render TFM Z" );

	m_Device.ClearRenderTarget( *m_pRTTransmittanceZ, NjFloat4( 1e4f, -1e4f, 0.0f, 0.0f ) );

	USING_MATERIAL_START( *m_pMatDepthWrite )

		m_Device.SetRenderTarget( *m_pRTTransmittanceZ );

		m_pCB_Object->m.Local2View = m_Box2World * m_World2Light;
		m_pCB_Object->m.View2Proj = m_Light2ShadowNormalized; // Here we use the alternate projection matrix that actually scales Z in [0,1] for ZBuffer compliance (since original World2Shadow keeps the Z in world units)
		m_pCB_Object->m.dUV = m_pRTTransmittanceZ->GetdUV();
		m_pCB_Object->UpdateData();

		PERF_MARKER(  D3DCOLOR( 0x00FF00FF ), L"Render Front Faces" );

	 	m_Device.SetStates( m_Device.m_pRS_CullFront, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled_RedOnly );
		m_pPrimBox->Render( M );

		PERF_MARKER(  D3DCOLOR( 0xFFFF00FF ), L"Render Back Faces" );

	 	m_Device.SetStates( m_Device.m_pRS_CullBack, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled_GreenOnly );
		m_pPrimBox->Render( M );

	USING_MATERIAL_END

	PERF_END_EVENT();

	// 2.2] Compute transmittance map
	PERF_BEGIN_EVENT( D3DCOLOR( 0xFF400000 ), L"Render TFM" );

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

	PERF_END_EVENT();

	//////////////////////////////////////////////////////////////////////////
	// 3] Render the actual volume

	// 3.1] Render front & back depths
	PERF_BEGIN_EVENT( D3DCOLOR( 0xFF800000 ), L"Render Volume Z" );

	m_Device.ClearRenderTarget( *m_pRTRenderZ, NjFloat4( 0.0f, -1e4f, 0.0f, 0.0f ) );

	USING_MATERIAL_START( *m_pMatDepthWrite )

		m_Device.SetRenderTarget( *m_pRTRenderZ );

		m_pCB_Object->m.Local2View = m_Box2World * _Camera.GetCB().World2Camera;
		m_pCB_Object->m.View2Proj = _Camera.GetCB().Camera2Proj;
		m_pCB_Object->m.dUV = m_pRTRenderZ->GetdUV();
		m_pCB_Object->UpdateData();

		PERF_MARKER(  D3DCOLOR( 0x00FF00FF ), L"Render Front Faces" );

	 	m_Device.SetStates( m_Device.m_pRS_CullBack, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled_RedOnly );
		m_pPrimBox->Render( M );

		PERF_MARKER(  D3DCOLOR( 0xFFFF00FF ), L"Render Back Faces" );

	 	m_Device.SetStates( m_Device.m_pRS_CullFront, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled_GreenOnly );
		m_pPrimBox->Render( M );

	USING_MATERIAL_END

	PERF_END_EVENT();

	// 3.2] Render the actual volume
	PERF_BEGIN_EVENT( D3DCOLOR( 0xFFC00000 ), L"Render Volume" );

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

	PERF_END_EVENT();

	//////////////////////////////////////////////////////////////////////////
	// 4] Combine with screen
	PERF_BEGIN_EVENT( D3DCOLOR( 0xFFFF0000 ), L"Render TFM Z" );

	m_Device.SetRenderTarget( m_Device.DefaultRenderTarget(), NULL );
	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );

	USING_MATERIAL_START( *m_pMatCombine )

		m_pCB_Splat->m.dUV = m_Device.DefaultRenderTarget().GetdUV();
		m_pCB_Splat->UpdateData();

// DEBUG
m_pRTRender->SetPS( 10 );
m_pRTTransmittanceZ->SetPS( 11 );
//m_pRTRenderZ->SetPS( 11 );
m_pRTTransmittanceMap->SetPS( 12 );
// DEBUG

		m_ScreenQuad.Render( M );

	USING_MATERIAL_END

	PERF_END_EVENT();
}

//#define	SPLAT_TO_BOX
#ifdef	SPLAT_TO_BOX
//////////////////////////////////////////////////////////////////////////
// Here, shadow map is simply made to fit the top/bottom of the box
void	EffectVolumetric::ComputeShadowTransform()
{
	// Build basis for directional light
	m_LightDirection.Normalize();

	bool		Above = m_LightDirection.y > 0.0f;

	NjFloat3	X = NjFloat3::UnitX;
	NjFloat3	Y = NjFloat3::UnitZ;

	NjFloat3	Center = m_Position + NjFloat3( 0, (Above ? +1 : -1) * m_Scale.y, 0 );

	NjFloat3	SizeLight = NjFloat3( 2.0f * m_Scale.x, 2.0f * m_Scale.z, 2.0f * m_Scale.y / fabs(m_LightDirection.y) );

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
	Shadow2Light.SetRow( 3, NjFloat4( 0, 0, 0, 1 ) );

	NjFloat4x4	Light2Shadow = Shadow2Light.Inverse();

	m_pCB_Shadow->m.LightDirection = NjFloat4( m_LightDirection, 0 );
	m_pCB_Shadow->m.World2Shadow = m_World2Light * Light2Shadow;
	m_pCB_Shadow->m.Shadow2World = Shadow2Light * Light2World;
	m_pCB_Shadow->m.ZMax.Set( SizeLight.z, 1.0f / SizeLight.z );


	// Create an alternate projection matrix that doesn't keep the World Z but instead projects it in [0,1]
	Shadow2Light.SetRow( 2, NjFloat4( 0, 0, SizeLight.z, 0 ) );
	Shadow2Light.SetRow( 3, NjFloat4( 0, 0, 0, 1 ) );

	m_Light2ShadowNormalized = Shadow2Light.Inverse();

// CHECK
NjFloat3	CheckMin( FLOAT32_MAX, FLOAT32_MAX, FLOAT32_MAX ), CheckMax( -FLOAT32_MAX, -FLOAT32_MAX, -FLOAT32_MAX );
NjFloat3	CornerLocal, CornerWorld, CornerShadow;
NjFloat4x4	World2ShadowNormalized = m_World2Light * m_Light2ShadowNormalized;
for ( int CornerIndex=0; CornerIndex < 8; CornerIndex++ )
{
	CornerLocal.x = 2.0f * (CornerIndex & 1) - 1.0f;
	CornerLocal.y = 2.0f * ((CornerIndex >> 1) & 1) - 1.0f;
	CornerLocal.z = 2.0f * ((CornerIndex >> 2) & 1) - 1.0f;

	CornerWorld = NjFloat4( CornerLocal, 1 ) * m_Box2World;

// 	CornerShadow = NjFloat4( CornerWorld, 1 ) * m_pCB_Shadow->m.World2Shadow;
	CornerShadow = NjFloat4( CornerWorld, 1 ) * World2ShadowNormalized;

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

#else
//////////////////////////////////////////////////////////////////////////
// Here, shadow is fit to bounding volume as seen from light
void	EffectVolumetric::ComputeShadowTransform()
{
	// Build basis for directional light
	m_LightDirection.Normalize();

	NjFloat3	X, Y;
#if 1
	// Use a ground vector
	if ( fabs( m_LightDirection.x - 1.0f ) < 1e-3f )
	{	// Special case
		X = NjFloat3::UnitZ;
		Y = NjFloat3::UnitY;
	}
	else
	{
		X = NjFloat3::UnitX ^ m_LightDirection;
		X.Normalize();
		Y = X ^ m_LightDirection;
	}
#elif 1
	// Force both X,Y vectors to the ground
	float	fFactor = (m_LightDirection.y > 0.0f ? 1.0f : -1.0f);
	X = fFactor * NjFloat3::UnitX;
	Y = NjFloat3::UnitZ;
#else
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
#endif

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

#endif

//////////////////////////////////////////////////////////////////////////
// Builds a fractal texture compositing several octaves of tiling Perlin noise
// Successive mips don't average previous mips but rather attenuate higher octaves <= WRONG! ^^
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

//static const int TEXTURE_SIZE_XY = 360;	// 280 FPS full res
static const int TEXTURE_SIZE_XY = 180;		// 400 FPS full res
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
	// Build & Save
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
	// Only load
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

#define PACK_R8	// Use R8 instead of R32F
#ifdef PACK_R8

	const float	ScaleMin = -0.15062222f, ScaleMax = 0.16956991f;

	// Convert mips to U8
	U8**	ppMipsU8 = new U8*[TEXTURE_MIPS];

	SizeXY = TEXTURE_SIZE_XY;
	SizeZ = TEXTURE_SIZE_Z;
	for ( int MipIndex=0; MipIndex < TEXTURE_MIPS; MipIndex++ )
	{
		float*	pSource = ppMips[MipIndex];
		U8*		pTarget = new U8[SizeXY*SizeXY*SizeZ];
		ppMipsU8[MipIndex] = pTarget;

		float	Min = +1.0, Max = -1.0f;
		for ( int Z=0; Z < SizeZ; Z++ )
		{
			float*	pSlice = pSource + SizeXY*SizeXY*Z;
			U8*		pSliceT = pTarget + SizeXY*SizeXY*Z;
			for ( int Y=0; Y < SizeXY; Y++ )
			{
				float*	pScanline = pSlice + SizeXY * Y;
				U8*		pScanlineT = pSliceT + SizeXY*Y;
				for ( int X=0; X < SizeXY; X++ )
				{
					float	V = *pScanline++;
							V = (V-ScaleMin)/(ScaleMax-ScaleMin);
					*pScanlineT++ = U8( MIN( 255, int(256 * V) ) );

					Min = MIN( Min, V );
					Max = MAX( Max, V );
				}
			}
		}

		SizeXY = MAX( 1, SizeXY >> 1 );
		SizeZ = MAX( 1, SizeZ >> 1 );
	}

	// Build actual R8 texture
	Texture3D*	pResult = new Texture3D( m_Device, TEXTURE_SIZE_XY, TEXTURE_SIZE_XY, TEXTURE_SIZE_Z, PixelFormatR8::DESCRIPTOR, TEXTURE_MIPS, (void**) ppMipsU8 );

	for ( int MipIndex=0; MipIndex < TEXTURE_MIPS; MipIndex++ )
		delete[] ppMipsU8[MipIndex];
	delete[] ppMipsU8;

#else
	// Build actual R32F texture
	Texture3D*	pResult = new Texture3D( m_Device, TEXTURE_SIZE_XY, TEXTURE_SIZE_XY, TEXTURE_SIZE_Z, PixelFormatR32F::DESCRIPTOR, TEXTURE_MIPS, (void**) ppMips );
#endif

	for ( int MipIndex=0; MipIndex < TEXTURE_MIPS; MipIndex++ )
		delete[] ppMips[MipIndex];
	delete[] ppMips;

	for ( int OctaveIndex=0; OctaveIndex < FRACTAL_OCTAVES; OctaveIndex++ )
		delete pNoises[OctaveIndex];

	return pResult;
}

#else
//////////////////////////////////////////////////////////////////////////
// This generator doesn't use a wide texture but a cube texture
// Most vertical space is wasted since we never sample there anyway!

namespace
{
	float	CombineDistances( float _pSqDistances[], int _pCellX[], int _pCellY[], int _pCellZ[], void* _pData )
	{
		return _pSqDistances[0];
	}
}


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


//////////////////////////////////////////////////////////////////////////
// 
const float Rg = 6360.0;
const float Rt = 6420.0;
const float RL = 6421.0;

const int TRANSMITTANCE_W = 256;
const int TRANSMITTANCE_H = 64;

const int SKY_W = 64;
const int SKY_H = 16;

const int RES_R = 32;
const int RES_MU = 128;
const int RES_MU_S = 32;
const int RES_NU = 8;

const int RES_3D_U = RES_MU_S * RES_NU;	// Full texture will be 256*128*32

struct	CBPreCompute
{
	NjFloat4	dUVW;
	bool		bFirstPass;	NjFloat3	__PAD1;
};

void	EffectVolumetric::PreComputeSkyTables()
{

	Texture2D*	pRTDeltaIrradiance = new Texture2D( m_Device, SKY_W, SKY_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );						// deltaE (temp)
	Texture3D*	pRTDeltaScatteringRayleigh = new Texture3D( m_Device, RES_3D_U, RES_MU, RES_R, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );		// deltaSR (temp)
	Texture3D*	pRTDeltaScatteringMie = new Texture3D( m_Device, RES_3D_U, RES_MU, RES_R, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );			// deltaSM (temp)
	Texture3D*	pRTDeltaScattering = new Texture3D( m_Device, RES_3D_U, RES_MU, RES_R, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );				// deltaJ (temp)

	Texture2D*	pRTTransmittance = new Texture2D( m_Device, TRANSMITTANCE_W, TRANSMITTANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );		// transmittance (final)
	Texture2D*	pRTIrradiance = new Texture2D( m_Device, SKY_W, SKY_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );							// irradiance (final)
	Texture3D*	pRTInScattering = new Texture3D( m_Device, RES_3D_U, RES_MU, RES_R, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );					// inscatter (final)

	Material*	pMatComputeTransmittance;
	Material*	pMatComputeIrradiance_Single;
	Material*	pMatComputeInScattering_Single;
	Material*	pMatComputeInScattering_Delta;
	Material*	pMatComputeIrradiance_Delta;
	Material*	pMatComputeInScattering_Multiple;
	Material*	pMatMergeInitialScattering;
	Material*	pMatAccumulateIrradiance;
	Material*	pMatAccumulateInScattering;
	CHECK_MATERIAL( pMatComputeTransmittance = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PreComputeTransmittance" ), 10 );
	CHECK_MATERIAL( pMatComputeIrradiance_Single = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PreComputeIrradiance_Single" ), 11 );				// irradiance1
	CHECK_MATERIAL( pMatComputeIrradiance_Delta = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PreComputeIrradiance_Delta" ), 12 );			// irradianceN
	CHECK_MATERIAL( pMatComputeInScattering_Single = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", "GS", "PreComputeInScattering_Single" ), 13 );			// inscatter1
	CHECK_MATERIAL( pMatComputeInScattering_Delta = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", "GS", "PreComputeInScattering_Delta" ), 14 );			// inscatterS
	CHECK_MATERIAL( pMatComputeInScattering_Multiple = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", "GS", "PreComputeInScattering_Multiple" ), 15 );		// inscatterN
	CHECK_MATERIAL( pMatMergeInitialScattering = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", "GS", "MergeInitialScattering" ), 16 );					// copyInscatter1
	CHECK_MATERIAL( pMatAccumulateIrradiance = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "AccumulateIrradiance" ), 17 );						// copyIrradiance
	CHECK_MATERIAL( pMatAccumulateInScattering = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", "GS", "AccumulateInScattering" ), 18 );					// copyInscatterN


	CB<CBPreCompute>	CB( m_Device, 10 );
	CB.m.bFirstPass = true;

	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );


	//////////////////////////////////////////////////////////////////////////
	// Computes transmittance texture T (line 1 in algorithm 4.1)
	USING_MATERIAL_START( *pMatComputeTransmittance )

		m_Device.SetRenderTarget( *pRTTransmittance );

		CB.m.dUVW = NjFloat4( pRTTransmittance->GetdUV(), 0.0f );
		CB.UpdateData();

		m_ScreenQuad.Render( M );

	USING_MATERIAL_END

	// Assign to slot 10
	m_Device.RemoveRenderTargets();
	pRTTransmittance->SetPS( 10 );

	//////////////////////////////////////////////////////////////////////////
	// Computes irradiance texture deltaE (line 2 in algorithm 4.1)
	USING_MATERIAL_START( *pMatComputeIrradiance_Single )

		m_Device.SetRenderTarget( *pRTDeltaIrradiance );

		CB.m.dUVW = NjFloat4( pRTDeltaIrradiance->GetdUV(), 0.0f );
		CB.UpdateData();

		m_ScreenQuad.Render( M );

	USING_MATERIAL_END

	// Assign to slot 13
	m_Device.RemoveRenderTargets();
	pRTDeltaIrradiance->SetPS( 13 );

	// ==================================================
 	// Clear irradiance texture E (line 4 in algorithm 4.1)
	m_Device.ClearRenderTarget( *pRTIrradiance, NjFloat4::Zero );

	//////////////////////////////////////////////////////////////////////////
	// Computes single scattering texture deltaS (line 3 in algorithm 4.1)
	// Rayleigh and Mie separated in deltaSR + deltaSM
	USING_MATERIAL_START( *pMatComputeInScattering_Single )

		ID3D11RenderTargetView*	ppTargets[] = { pRTDeltaScatteringRayleigh->GetTargetView( 0, 0, 0 ), pRTDeltaScatteringMie->GetTargetView( 0, 0, 0 ) };
		m_Device.SetRenderTargets( RES_3D_U, RES_MU, 2, ppTargets );

		CB.m.dUVW = pRTDeltaScatteringRayleigh->GetdUVW();
		CB.UpdateData();

		m_ScreenQuad.RenderInstanced( M, RES_R );

	USING_MATERIAL_END

	// Assign to slot 14 & 15
	m_Device.RemoveRenderTargets();
	pRTDeltaScatteringRayleigh->SetPS( 14 );
	pRTDeltaScatteringMie->SetPS( 15 );

	// ==================================================
	// Merges DeltaScatteringRayleigh & Mie into initial inscatter texture S (line 5 in algorithm 4.1)
	USING_MATERIAL_START( *pMatMergeInitialScattering )

		m_Device.SetRenderTarget( *pRTInScattering );

		CB.m.dUVW = pRTInScattering->GetdUVW();
		CB.UpdateData();

		m_ScreenQuad.RenderInstanced( M, RES_R );

	USING_MATERIAL_END

	//////////////////////////////////////////////////////////////////////////
	// Loop for each scattering order (line 6 in algorithm 4.1)
	for ( int Order=2; Order <= 4; Order++ )
	{
		// ==================================================
		// Computes deltaJ (line 7 in algorithm 4.1)
		USING_MATERIAL_START( *pMatComputeInScattering_Delta )

			m_Device.SetRenderTarget( *pRTDeltaScattering );

			CB.m.dUVW = pRTDeltaScattering->GetdUVW();
			CB.m.bFirstPass = Order == 2;
			CB.UpdateData();

			m_ScreenQuad.RenderInstanced( M, RES_R );

		USING_MATERIAL_END

		// Assign to slot 16
		m_Device.RemoveRenderTargets();
		pRTDeltaScattering->SetPS( 16 );

		// ==================================================
		// Computes deltaE (line 8 in algorithm 4.1)
		USING_MATERIAL_START( *pMatComputeIrradiance_Delta )

			m_Device.SetRenderTarget( *pRTDeltaIrradiance );

			CB.m.dUVW = NjFloat4( pRTDeltaIrradiance->GetdUV(), 0.0 );
			CB.m.bFirstPass = Order == 2;
			CB.UpdateData();

			m_ScreenQuad.Render( M );

		USING_MATERIAL_END

		// Assign to slot 13 again
		m_Device.RemoveRenderTargets();
		pRTDeltaIrradiance->SetPS( 13 );

		// ==================================================
		// Computes deltaS (line 9 in algorithm 4.1)
		USING_MATERIAL_START( *pMatComputeInScattering_Multiple )

			m_Device.SetRenderTarget( *pRTDeltaScatteringRayleigh );	// Warning: We're re-using Rayleigh slot.
																		// It doesn't matter for next orders where we don't sample from Rayleigh+Mie separately anymore (only done in first pass)

			CB.m.dUVW = pRTDeltaScattering->GetdUVW();
			CB.m.bFirstPass = Order == 2;
			CB.UpdateData();

			m_ScreenQuad.RenderInstanced( M, RES_R );

		USING_MATERIAL_END

		// Assign to slot 14 again
		m_Device.RemoveRenderTargets();
		pRTDeltaScatteringRayleigh->SetPS( 14 );

		// ==================================================
		// Adds deltaE into irradiance texture E (line 10 in algorithm 4.1)
		m_Device.SetStates( NULL, NULL, m_Device.m_pBS_Additive );

		USING_MATERIAL_START( *pMatAccumulateIrradiance )

			m_Device.SetRenderTarget( *pRTIrradiance );

			CB.m.dUVW = NjFloat4( pRTIrradiance->GetdUV(), 0 );
			CB.UpdateData();

			m_ScreenQuad.Render( M );

		USING_MATERIAL_END

		// ==================================================
 		// Adds deltaS into inscatter texture S (line 11 in algorithm 4.1)
		USING_MATERIAL_START( *pMatAccumulateInScattering )

			m_Device.SetRenderTarget( *pRTInScattering );

			CB.m.dUVW = pRTInScattering->GetdUVW();
			CB.UpdateData();

			m_ScreenQuad.RenderInstanced( M, RES_R );

		USING_MATERIAL_END

		m_Device.SetStates( NULL, NULL, m_Device.m_pBS_Disabled );
	}

	// Assign final textures to slots 11 & 12
	m_Device.RemoveRenderTargets();
	pRTInScattering->SetPS( 11 );
	pRTIrradiance->SetPS( 12 );

	// Release materials & temporary RTs
	delete pMatAccumulateInScattering;
	delete pMatAccumulateIrradiance;
	delete pMatMergeInitialScattering;
	delete pMatComputeInScattering_Multiple;
	delete pMatComputeInScattering_Delta;
	delete pMatComputeInScattering_Single;
	delete pMatComputeIrradiance_Delta;
	delete pMatComputeIrradiance_Single;
	delete pMatComputeTransmittance;

	delete pRTDeltaIrradiance;
	delete pRTDeltaScatteringRayleigh;
	delete pRTDeltaScatteringMie;
	delete pRTDeltaScattering;
}

void	EffectVolumetric::FreeSkyTables()
{
	delete m_pRTInScattering;
	delete m_pRTIrradiance;
	delete m_pRTTransmittance;
}