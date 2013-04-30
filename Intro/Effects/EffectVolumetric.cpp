#include "../../GodComplex.h"
#include "EffectVolumetric.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

//#define BUILD_SKY_SCATTERING	// Build or load? (warning: the computation shader takes hell of a time to compile!) (but the computation itself takes less than a second! ^^)

static const float	SCREEN_TARGET_RATIO = 0.5f;

static const float	GROUND_RADIUS_KM = 6360.0f;
static const float	ATMOSPHERE_THICKNESS_KM = 60.0f;

static const float	BOX_BASE = 8.0f;	// 10km  <== Find better way to keep visual aspect!
static const float	BOX_HEIGHT = 4.0f;	// 4km high

static const float	TRANSMITTANCE_TAN_MAX = 1.5;	// Close to PI/2 to maximize precision at grazing angles
//#define USE_PRECISE_COS_THETA_MIN



EffectVolumetric::EffectVolumetric( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, Camera& _Camera ) : m_Device( _Device ), m_RTHDR( _RTHDR ), m_ScreenQuad( _ScreenQuad ), m_Camera( _Camera ), m_ErrorCode( 0 ), m_pTableTransmittance( NULL )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
 	CHECK_MATERIAL( m_pMatDepthWrite = CreateMaterial( IDR_SHADER_VOLUMETRIC_DEPTH_WRITE, VertexFormatP3::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
 	CHECK_MATERIAL( m_pMatSplatCameraFrustum = CreateMaterial( IDR_SHADER_VOLUMETRIC_COMPUTE_TRANSMITTANCE, VertexFormatP3::DESCRIPTOR, "VS_SplatFrustum", NULL, "PS_SplatFrustum" ), 2 );
 	CHECK_MATERIAL( m_pMatComputeTransmittance = CreateMaterial( IDR_SHADER_VOLUMETRIC_COMPUTE_TRANSMITTANCE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 3 );
 	CHECK_MATERIAL( m_pMatDisplay = CreateMaterial( IDR_SHADER_VOLUMETRIC_DISPLAY, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 4 );
 	CHECK_MATERIAL( m_pMatCombine = CreateMaterial( IDR_SHADER_VOLUMETRIC_COMBINE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 5 );
#ifdef SHOW_TERRAIN
	CHECK_MATERIAL( m_pMatTerrain = CreateMaterial( IDR_SHADER_VOLUMETRIC_TERRAIN, VertexFormatP3::DESCRIPTOR, "VS", NULL, "PS" ), 6 );
#endif

//	const char*	pCSO = LoadCSO( "./Resources/Shaders/CSO/VolumetricCombine.cso" );
//	CHECK_MATERIAL( m_pMatCombine = CreateMaterial( IDR_SHADER_VOLUMETRIC_COMBINE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, pCSO ), 4 );
//	delete[] pCSO;
	

	//////////////////////////////////////////////////////////////////////////
	// Pre-Compute multiple-scattering tables
	PreComputeSkyTables();

	//////////////////////////////////////////////////////////////////////////
	// Build the primitives
	{
		m_pPrimBox = new Primitive( m_Device, VertexFormatP3::DESCRIPTOR );
		GeometryBuilder::BuildCube( 1, 1, 1, *m_pPrimBox );
	}

// 	{
// 		float		TanFovH = m_Camera.GetCB().Params.x;
// 		float		TanFovV = m_Camera.GetCB().Params.y;
// 		float		FarClip = m_Camera.GetCB().Params.w;
// 
// 		// Build the 5 vertices of the frustum pyramid, in camera space
// 		NjFloat3	pVertices[5];
// 		pVertices[0] = NjFloat3( 0, 0, 0 );
// 		pVertices[1] = FarClip * NjFloat3( -TanFovH, +TanFovV, 1 );
// 		pVertices[2] = FarClip * NjFloat3( -TanFovH, -TanFovV, 1 );
// 		pVertices[3] = FarClip * NjFloat3( +TanFovH, -TanFovV, 1 );
// 		pVertices[4] = FarClip * NjFloat3( +TanFovH, +TanFovV, 1 );
// 
// 		U16			pIndices[18];
// 		pIndices[3*0+0] = 0;	// Left face
// 		pIndices[3*0+1] = 1;
// 		pIndices[3*0+2] = 2;
// 		pIndices[3*1+0] = 0;	// Bottom face
// 		pIndices[3*1+1] = 2;
// 		pIndices[3*1+2] = 3;
// 		pIndices[3*2+0] = 0;	// Right face
// 		pIndices[3*2+1] = 3;
// 		pIndices[3*2+2] = 4;
// 		pIndices[3*3+0] = 0;	// Top face
// 		pIndices[3*3+1] = 4;
// 		pIndices[3*3+2] = 1;
// 		pIndices[3*4+0] = 1;	// Back faces
// 		pIndices[3*4+1] = 3;
// 		pIndices[3*4+2] = 2;
// 		pIndices[3*5+0] = 1;
// 		pIndices[3*5+1] = 4;
// 		pIndices[3*5+2] = 3;
// 
// 		m_pPrimFrustum = new Primitive( m_Device, 5, pVertices, 18, pIndices, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST, VertexFormatP3::DESCRIPTOR );
// 	}

#ifdef SHOW_TERRAIN
	{
		m_pPrimTerrain = new Primitive( m_Device, VertexFormatP3::DESCRIPTOR );
		GeometryBuilder::BuildPlane( 200, 200, NjFloat3::UnitX, -NjFloat3::UnitZ, *m_pPrimTerrain );
	}
#endif

	//////////////////////////////////////////////////////////////////////////
	// Build textures & render targets
	m_pRTCameraFrustumSplat = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, PixelFormatR8::DESCRIPTOR, 1, NULL );
	m_pRTTransmittanceZ = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, PixelFormatRG16F::DESCRIPTOR, 1, NULL );
	m_pRTTransmittanceMap = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 2, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );

	int	W = m_Device.DefaultRenderTarget().GetWidth();
	int	H = m_Device.DefaultRenderTarget().GetHeight();
	m_RenderWidth = int( ceilf( W * SCREEN_TARGET_RATIO ) );
	m_RenderHeight = int( ceilf( H * SCREEN_TARGET_RATIO ) );

	m_pRTRenderZ = new Texture2D( m_Device, m_RenderWidth, m_RenderHeight, 1, PixelFormatRG16F::DESCRIPTOR, 1, NULL );
	m_pRTRender = new Texture2D( m_Device, m_RenderWidth, m_RenderHeight, 2, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );

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
	delete m_pRTCameraFrustumSplat;

#ifdef SHOW_TERRAIN
	delete m_pMatTerrain;
	delete m_pPrimTerrain;
#endif
	delete m_pPrimFrustum;
	delete m_pPrimBox;

	FreeSkyTables();

 	delete m_pMatCombine;
 	delete m_pMatDisplay;
 	delete m_pMatComputeTransmittance;
 	delete m_pMatSplatCameraFrustum;
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

void	EffectVolumetric::Render( float _Time, float _DeltaTime )
{
// DEBUG
float	t = 0.25f * _Time;
//m_LightDirection.Set( 0, 1, -1 );
//m_LightDirection.Set( 1, 2.0, -5 );
//m_LightDirection.Set( cosf(_Time), 2.0f * sinf( 0.324f * _Time ), sinf( _Time ) );
//m_LightDirection.Set( cosf(_Time), 1.0f, sinf( _Time ) );

float	SunAngle = LERP( -0.01f * PI, 0.499f * PI, 0.5f * (1.0f + sinf( t )) );		// Oscillating between slightly below horizon to zenith

//SunAngle = -0.015f * PI;	// Sexy Sunset
//SunAngle = 0.15f * PI;	// Sexy Sunset

float	SunPhi = 0.5923f * t;
//m_LightDirection.Set( sinf( SunPhi ), sinf( SunAngle ), -cosf( SunPhi ) );
m_LightDirection.Set( 0.0, sinf( SunAngle ), -cosf( SunAngle ) );
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
	m_Position.Set( 0, BOX_BASE + 0.5f * BOX_HEIGHT, -100 );
	m_Scale.Set( 200.0f, 0.5f * BOX_HEIGHT, 200.0f );

	m_Box2World.PRS( m_Position, m_Rotation, m_Scale );

	ComputeShadowTransform();

	m_pCB_Shadow->UpdateData();

	PERF_END_EVENT();

	//////////////////////////////////////////////////////////////////////////
	// 1] Compute the transmittance function map
	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );

// Actually, we lose some perf!
// 	// 1.1] Splat the camera frustum that will help us isolate the pixels we actually need to compute
// 	m_Device.ClearRenderTarget( *m_pRTCameraFrustumSplat, NjFloat4( 0.0f, 0.0f, 0.0f, 0.0f ) );
// 	m_Device.SetRenderTarget( *m_pRTCameraFrustumSplat );
// 
// 	USING_MATERIAL_START( *m_pMatSplatCameraFrustum )
// 
// 		m_pCB_Splat->m.dUV = m_pRTCameraFrustumSplat->GetdUV();
// 		m_pCB_Splat->UpdateData();
// 
// 		m_pPrimFrustum->Render( M );
// 
// 	USING_MATERIAL_END

	// 1.2] Compute transmittance map
	PERF_BEGIN_EVENT( D3DCOLOR( 0xFF400000 ), L"Render TFM" );

	m_Device.ClearRenderTarget( *m_pRTTransmittanceMap, NjFloat4( 0.0f, 0.0f, 0.0f, 0.0f ) );

	D3D11_VIEWPORT	Viewport = {
		0.0f,
		0.0f,
		float(m_ViewportWidth),
		float(m_ViewportHeight),
		0.0f,	// MinDepth
		1.0f,	// MaxDepth
	};

	ID3D11RenderTargetView*	ppViews[2] = {
		m_pRTTransmittanceMap->GetTargetView( 0, 0, 1 ),
		m_pRTTransmittanceMap->GetTargetView( 0, 1, 1 ),
	};
	m_Device.SetRenderTargets( m_pRTTransmittanceMap->GetWidth(), m_pRTTransmittanceMap->GetHeight(), 2, ppViews, NULL, &Viewport );

	USING_MATERIAL_START( *m_pMatComputeTransmittance )

		m_pRTTransmittanceZ->SetPS( 10 );
//		m_pRTCameraFrustumSplat->SetPS( 11 );

		m_pCB_Splat->m.dUV = m_pRTTransmittanceMap->GetdUV();
		m_pCB_Splat->UpdateData();

		m_ScreenQuad.Render( M );

	USING_MATERIAL_END

	// Remove contention on that Transmittance Z we don't need for the next pass...
	m_Device.RemoveShaderResources( 10 );

	PERF_END_EVENT();

	//////////////////////////////////////////////////////////////////////////
	// 2] Show terrain
#ifdef SHOW_TERRAIN
 	PERF_BEGIN_EVENT( D3DCOLOR( 0xFF000000 ), L"Render Terrain" );

	USING_MATERIAL_START( *m_pMatTerrain )

		m_Device.SetRenderTarget( m_RTHDR, &m_Device.DefaultDepthStencil() );
	 	m_Device.SetStates( m_Device.m_pRS_CullBack, m_Device.m_pDS_ReadWriteLess, m_Device.m_pBS_Disabled );

		m_pRTTransmittanceMap->SetPS( 12 );

		m_pCB_Object->m.Local2View.PRS( NjFloat3( 0, 0, 0 ), NjFloat4::QuatFromAngleAxis( 0.0f, NjFloat3::UnitY ), NjFloat3( 100, 1, 100 ) );
		m_pCB_Object->m.dUV = m_RTHDR.GetdUV();
		m_pCB_Object->UpdateData();

		m_pPrimTerrain->Render( M );

	USING_MATERIAL_END

	PERF_END_EVENT();
#endif


	//////////////////////////////////////////////////////////////////////////
	// 3] Render the actual volume

	// 3.1] Render front & back depths
	PERF_BEGIN_EVENT( D3DCOLOR( 0xFF800000 ), L"Render Volume Z" );

	m_Device.ClearRenderTarget( *m_pRTRenderZ, NjFloat4( 0.0f, -1e4f, 0.0f, 0.0f ) );

	USING_MATERIAL_START( *m_pMatDepthWrite )

		m_Device.SetRenderTarget( *m_pRTRenderZ );

		m_pCB_Object->m.Local2View = m_Box2World * m_Camera.GetCB().World2Camera;
		m_pCB_Object->m.View2Proj = m_Camera.GetCB().Camera2Proj;
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

	USING_MATERIAL_START( *m_pMatDisplay )

		m_Device.ClearRenderTarget( *m_pRTRender, NjFloat4( 0.0f, 0.0f, 0.0f, 1.0f ) );
//		m_Device.SetRenderTarget( *m_pRTRender );

		ID3D11RenderTargetView*	ppViews[] = {
			m_pRTRender->GetTargetView( 0, 0, 1 ),
			m_pRTRender->GetTargetView( 0, 1, 1 )
		};
		m_Device.SetRenderTargets( m_pRTRender->GetWidth(), m_pRTRender->GetHeight(), 2, ppViews );
		m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );

		m_pRTRenderZ->SetPS( 10 );
		m_Device.DefaultDepthStencil().SetPS( 11 );
		m_pRTTransmittanceMap->SetPS( 12 );

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
m_pRTRender->SetPS( 10 );	// Cloud rendering, with scattering and extinction
m_RTHDR.SetPS( 11 );		// Background scene
//m_pRTTransmittanceZ->SetPS( 11 );
//m_pRTRenderZ->SetPS( 11 );
//m_pRTTransmittanceMap->SetPS( 12 );
// DEBUG

		m_ScreenQuad.Render( M );

	USING_MATERIAL_END

	PERF_END_EVENT();

	// Remove contention on SRVs for next pass...
	m_Device.RemoveShaderResources( 11 );
	m_Device.RemoveShaderResources( 12 );
}

//#define	SPLAT_TO_BOX
#define USE_NUAJ_SHADOW
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

#elif !defined(USE_NUAJ_SHADOW)
//////////////////////////////////////////////////////////////////////////
// Here, shadow is fit to bounding volume as seen from light
void	EffectVolumetric::ComputeShadowTransform()
{
	// Build basis for directional light
	m_LightDirection.Normalize();

	NjFloat3	X, Y;
#if 0
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

#else

// Projects a world position in kilometers into the shadow plane
NjFloat3	EffectVolumetric::Project2ShadowPlane( const NjFloat3& _PositionKm, float& _Distance2PlaneKm )
{
// 	NjFloat3	Center2PositionKm = _PositionKm - m_ShadowPlaneCenterKm;
// 	_Distance2PlaneKm = Center2PositionKm | m_ShadowPlaneNormal;
// 	return _PositionKm + _Distance2PlaneKm * m_ShadowPlaneNormal;

	// We're now assuming the plane normal is always Y up and we need to project the position to the plane following m_ShadowPlaneNormal, which is the light's direction
	float	VerticalDistanceToPlane = _PositionKm.y - m_ShadowPlaneCenterKm.y;
	_Distance2PlaneKm = VerticalDistanceToPlane / m_ShadowPlaneNormal.y;
	return _PositionKm - _Distance2PlaneKm * m_ShadowPlaneNormal;
}

// Projects a world position in kilometers into the shadow quad
NjFloat2	EffectVolumetric::World2ShadowQuad( const NjFloat3& _PositionKm, float& _Distance2PlaneKm )
{
	NjFloat3	ProjectedPositionKm = Project2ShadowPlane( _PositionKm, _Distance2PlaneKm );
	NjFloat3	Center2ProjPositionKm = ProjectedPositionKm - m_ShadowPlaneCenterKm;
	return NjFloat2( Center2ProjPositionKm | m_ShadowPlaneX, Center2ProjPositionKm | m_ShadowPlaneY );
}

//////////////////////////////////////////////////////////////////////////
// Update shadow parameters
// The idea is this:
//
//        ..           /
//  top       ..      / Light direction
//  cloud ------ ..  /
//               ---x..
//                     --..
//        -------          -- ..
//        ///////-----         -  .. Tangent plane to the top cloud sphere
//        /////////////--        -
//        //Earth/////////-
//
// 1) We compute the tangent plane to the top cloud sphere by projecting the Earth's center to the cloud sphere's surface following the Sun's direction.
// 2) We project the camera frustum onto that plane
// 3) We compute the bounding quadrilateral to that frustum
// 4) We compute the data necessary to transform a world position into a shadow map position, and the reverse
//
void	EffectVolumetric::ComputeShadowTransform()
{
	static const NjFloat3	PLANET_CENTER_KM = NjFloat3( 0, -GROUND_RADIUS_KM, 0 );

	float		TanFovH = m_Camera.GetCB().Params.x;
	float		TanFovV = m_Camera.GetCB().Params.y;
	NjFloat4x4&	Camera2World = m_Camera.GetCB().Camera2World;
	NjFloat3	CameraPositionKm = Camera2World.GetRow( 3 );

	static const float	SHADOW_FAR_CLIP_DISTANCE = 250.0f;
	static const float	SHADOW_SCALE = 1.1f;

	//////////////////////////////////////////////////////////////////////////
	// Compute shadow plane tangent space
	m_LightDirection.Normalize();

	NjFloat3	ClippedSunDirection = -m_LightDirection;
// 	if ( ClippedSunDirection.y > 0.0f )
// 		ClippedSunDirection = -ClippedSunDirection;	// We always require a vector facing down

	const float	MIN_THRESHOLD = 1e-2f;
	if ( ClippedSunDirection.y > 0.0 && ClippedSunDirection.y < MIN_THRESHOLD )
	{	// If the ray is too horizontal, the matrix becomes full of zeroes and is not inversible...
		ClippedSunDirection.y = MIN_THRESHOLD;
		ClippedSunDirection.Normalize();
	}
	else if ( ClippedSunDirection.y < 0.0 && ClippedSunDirection.y > -MIN_THRESHOLD )
	{	// If the ray is too horizontal, the matrix becomes full of zeroes and is not inversible...
		ClippedSunDirection.y = -MIN_THRESHOLD;
		ClippedSunDirection.Normalize();
	}

// TODO: Fix this !
//			 // Clip Sun's direction to avoid grazing angles
//			 if ( ClippedSunDirection.y < (float) Math.Cos( m_shadowSunSetMaxAngle ) )
//			 {
// //				ClippedSunDirection.y = Math.Max( ClippedSunDirection.y, (float) Math.Cos( m_shadowSunSetMaxAngle ) );
// //				ClippedSunDirection /= Math.Max( 1e-3f, 1.0f-ClippedSunDirection.y );		// Project to unit circle
//				 ClippedSunDirection /= (float) Math.Sqrt( ClippedSunDirection.x*ClippedSunDirection.x + ClippedSunDirection.Z*ClippedSunDirection.Z );		// Project to unit circle
//				 ClippedSunDirection.y = 1.0f / (float) Math.Tan( m_shadowSunSetMaxAngle );	// Replace Y by tangent
//				 ClippedSunDirection.Normalize();
//			 }

	// Force both X,Y vectors to the ground, normal is light's direction alwaus pointing toward the ground
	float	fFactor = 1;//(m_LightDirection.y > 0.0f ? 1.0f : -1.0f);
	m_ShadowPlaneX = fFactor * NjFloat3::UnitX;
	m_ShadowPlaneY = NjFloat3::UnitZ;
 	m_ShadowPlaneNormal = ClippedSunDirection;
//	m_ShadowPlaneCenterKm = NjFloat3( CameraPositionKm.x, 0, CameraPositionKm.z ) + (BOX_BASE + (m_LightDirection.y > 0.0f ? 1 : 0) * MAX( 1e-3f, BOX_HEIGHT )) * NjFloat3::UnitY;	// Center on camera for a start...
	m_ShadowPlaneCenterKm = NjFloat4( 0, m_LightDirection.y > 0 ? 1.0f : -1.0f, 0, 1 ) * m_Box2World;

	float	ZSize = BOX_HEIGHT / abs(ClippedSunDirection.y);	// Since we're blocking the XY axes on the plane, Z changes with the light's vertical component
																// Slanter rays will yield longer Z's

	//////////////////////////////////////////////////////////////////////////
	// Build camera frustum
	NjFloat3  pCameraFrustumKm[5];
	pCameraFrustumKm[0] = NjFloat3::Zero;
	pCameraFrustumKm[1] = SHADOW_FAR_CLIP_DISTANCE * NjFloat3( -TanFovH, -TanFovV, 1.0f );
	pCameraFrustumKm[2] = SHADOW_FAR_CLIP_DISTANCE * NjFloat3( +TanFovH, -TanFovV, 1.0f );
	pCameraFrustumKm[3] = SHADOW_FAR_CLIP_DISTANCE * NjFloat3( +TanFovH, +TanFovV, 1.0f );
	pCameraFrustumKm[4] = SHADOW_FAR_CLIP_DISTANCE * NjFloat3( -TanFovH, +TanFovV, 1.0f );

	// Transform into WORLD space
	for ( int i=0; i < 5; i++ )
		pCameraFrustumKm[i] = NjFloat4( pCameraFrustumKm[i], 1.0f ) * Camera2World;


	//////////////////////////////////////////////////////////////////////////
	// Build WORLD => LIGHT transform & inverse
	//
	//	^ N                     ^
	//	|                      / -Z, pointing toward the light
	//	|                    /
	// C|                P'/
	//  *-----*----------*-------------> X  <== Cloud plane
	//	 \    |        /
	//	  \   |h     /
	//	   \  |    / d
	//	    \ |  /
	//	     \|/
	//	    P *
	//
	// P is the point we need to transform into light space
	// X is one axis of the 2 axes of the 2D plane
	// -Z is the vector pointing toward the light
	// N=(0,1,0) is the plane normal
	// C is m_ShadowPlaneCenterKm
	// 
	// We pose h = [C-P].N
	// We need to find	d = h / -Zy  which is the distance to the plane by following the light direction (i.e. projection)
	//					d = [P-C].N/Zy
	//
	// We retrieve the projected point's position P' = P - d*Z and we're now on the shadow plane
	// We then retrieve the x component of the 2D plane vector by writing:
	//
	// Plane x	= [P' - C].X
	//			= [P - d*Z - C].X
	//			= [P - [(P-C).N/Zy]*Z - C].X
	//			= [P - [(P-C).N]*Z/Zy - C].X
	//
	// Let's write Z' = Z/Zy , the scaled version of Z that accounts for lengthening due to grazing angles
	// So:
	// Plane x	= [P - [(P-C).N]*Z' - C].X
	//			= [P - [P.N - C.N]*Z' - C].X
	//			= P.X - [P.N - C.N]*(Z'.X) - C.X
	//			= P.X - (P.N)*(Z'.X) + (C.N)*(Z'.X) - C.X
	//			= P.[X - N*(Z'.X)] + C.[N*(Z'.X) - X]
	//
	// Writing X' = X - N*(Z'.X), we finally have:
	//
	//		Plane x = P.X' - C.X'
	//
	// And the same obviously goes for Y.
	// In matrix form, this gives:
	//
	//	|  X'x   Y'x     0   |
	//	|  X'y   Y'y    1/Zy |
	//	|  X'z   Y'z     0   |
	//	| -C.X' -C.Y' -Cy/Zy |
	//
	NjFloat3	ScaledZ = m_ShadowPlaneNormal / m_ShadowPlaneNormal.y;

	float		ZdotX = ScaledZ | m_ShadowPlaneX;
	NjFloat3	NewX = m_ShadowPlaneX - NjFloat3( 0, ZdotX, 0 );

	float		ZdotY = ScaledZ | m_ShadowPlaneY;
	NjFloat3	NewY = m_ShadowPlaneY - NjFloat3( 0, ZdotY, 0 );

	NjFloat3	NewZ( 0, 1/m_ShadowPlaneNormal.y, 0 );

	NjFloat3	Translation = NjFloat3( -m_ShadowPlaneCenterKm | NewX, -m_ShadowPlaneCenterKm | NewY, -m_ShadowPlaneCenterKm | NewZ );

	m_World2Light.SetRow( 0, NjFloat4( NewX.x, NewY.x, NewZ.x, 0 ) );
	m_World2Light.SetRow( 1, NjFloat4( NewX.y, NewY.y, NewZ.y, 0 ) );
	m_World2Light.SetRow( 2, NjFloat4( NewX.z, NewY.z, NewZ.z, 0 ) );
	m_World2Light.SetRow( 3, NjFloat4( Translation.x, Translation.y, Translation.z, 1 ) );

	NjFloat4x4	Light2World = m_World2Light.Inverse();

// CHECK: We reproject the frustum and verify the values
NjFloat4	pCheckProjected[5];
for ( int i=0; i < 5; i++ )
	pCheckProjected[i] = NjFloat4( pCameraFrustumKm[i], 1 ) * m_World2Light;
// CHECK


	//////////////////////////////////////////////////////////////////////////
	// Compute bounding quad
	// Simply use a coarse quad and don't give a fuck (advantage is that it's always axis aligned despite camera orientation)
	NjFloat2	QuadMin( +1e6f, +1e6f );
	NjFloat2	QuadMax( -1e6f, -1e6f );

	if ( m_LightDirection.y > 0.0f )
	{	// When the Sun is above the clouds, project the frustum's corners to plane and keep the bounding quad of these points

		// Project frustum to shadow plane
		float		pDistances2Plane[5];
		NjFloat2	pFrustumProjKm[5];
		NjFloat2	Center = NjFloat2::Zero;
		for ( int i=0; i < 5; i++ )
		{
			pFrustumProjKm[i] = World2ShadowQuad( pCameraFrustumKm[i], pDistances2Plane[i] );
			Center = Center + pFrustumProjKm[i];
		}

//		// Re-center about the center
// 		Center = Center / 5;
// 		m_ShadowPlaneCenterKm = m_ShadowPlaneCenterKm + Center.x * m_ShadowPlaneX + Center.y * m_ShadowPlaneY;
// 
// 		// Reproject using new center
// 		NjFloat2	Center2 = NjFloat2::Zero;
// 		for ( int i=0; i < 5; i++ )
// 		{
// 			pFrustumProjKm[i] = World2ShadowQuad( pCameraFrustumKm[i], pDistances2Plane[i] );
// 			Center2 = Center2 + pFrustumProjKm[i];
// 		}
// 		Center2 = Center2 / 5;	// Ensure it's equal to 0!

		for ( int i=0; i < 5; i++ )
		{
			QuadMin = QuadMin.Min( pFrustumProjKm[i] );
			QuadMax = QuadMax.Max( pFrustumProjKm[i] );
		}
	}
	else
	{	// If the Sun is below the clouds then there's no need to account for godrays and we should only focus
		//	on the cloud volume that will actually be lit by the Sun and that we can see.
		// To do this, we unfortunately have to compute the intersection of the camera frustum with the cloud box
		//	and compute our bounding quad from there...
		//
		ComputeFrustumIntersection( pCameraFrustumKm, BOX_BASE, QuadMin, QuadMax );
		ComputeFrustumIntersection( pCameraFrustumKm, BOX_BASE + BOX_HEIGHT, QuadMin, QuadMax );
	}

	// Also compute the cloud box's bounding quad and clip our quad values with it since it's useless to have a quad larger
	//	than what the cloud is covering anyway...
	NjFloat2	CloudQuadMin( +1e6f, +1e6f );
	NjFloat2	CloudQuadMax( -1e6f, -1e6f );
	for ( int BoxCornerIndex=0; BoxCornerIndex < 8; BoxCornerIndex++ )
	{
		int			Z = BoxCornerIndex >> 2;
		int			Y = (BoxCornerIndex >> 1) & 1;
		int			X = BoxCornerIndex & 1;
		NjFloat3	CornerLocal( 2*float(X)-1, 2*float(Y)-1, 2*float(Z)-1 );
		NjFloat3	CornerWorld = NjFloat4( CornerLocal, 1 ) * m_Box2World;

		float		Distance;
		NjFloat2	CornerProj = World2ShadowQuad( CornerWorld, Distance );

		CloudQuadMin = CloudQuadMin.Min( CornerProj );
		CloudQuadMax = CloudQuadMax.Max( CornerProj );
	}

	QuadMin = QuadMin.Max( CloudQuadMin );
	QuadMax = QuadMax.Min( CloudQuadMax );

	NjFloat2	QuadSize = QuadMax - QuadMin;


	//////////////////////////////////////////////////////////////////////////
	// Determine the rendering viewport based on quad's size

// Statistics of min/max sizes
static NjFloat2		s_QuadSizeMin = NjFloat2( +1e6f, +1e6f );
static NjFloat2		s_QuadSizeMax = NjFloat2( -1e6f, -1e6f );
s_QuadSizeMin = s_QuadSizeMin.Min( QuadSize );
s_QuadSizeMax = s_QuadSizeMax.Max( QuadSize );

	// This is the max reported size for a quad when clipping is set at 100km
	// A quad of exactly this size should fit the shadow map's size exactly
	const float	REFERENCE_SHADOW_FAR_CLIP = 100.0f;				// The size was determined using a default clip distance of 100km
	float		MaxWorldSize = 180.0f;							// The 180 factor is arbitrary and comes from experimenting with s_QuadSizeMax and moving the camera around and storing the largest value...
				MaxWorldSize *= SHADOW_FAR_CLIP_DISTANCE / (SHADOW_SCALE * REFERENCE_SHADOW_FAR_CLIP);

// 	ASSERT( QuadSize.x < MaxWorldSize, "Increase max world size!" );
// 	ASSERT( QuadSize.y < MaxWorldSize, "Increase max world size!" );
	float		WorldSizeX = MAX( MaxWorldSize, QuadSize.x );
	float		WorldSizeY = MAX( MaxWorldSize, QuadSize.y );

	float		TexelRatioX = MaxWorldSize / WorldSizeX;		// Scale factor compared to our max accountable world size.
	float		TexelRatioY = MaxWorldSize / WorldSizeY;		// A ratio below 1 means we exceeded the maximum bounds and texels will be skipped.
																// This should be avoided as much as possible as it results in a lack of precision...
//	ASSERT( TexelRatioX >= 1.0f, "Increase quad size max to avoid texel squeeze!" );	// We can't avoid that with slant rays...
//	ASSERT( TexelRatioY >= 1.0f, "Increase quad size max to avoid texel squeeze!" );	// We can't avoid that with slant rays...

	NjFloat2	World2TexelScale( (SHADOW_MAP_SIZE-1) / WorldSizeX, (SHADOW_MAP_SIZE-1) / WorldSizeY );	// Actual scale to convert from world to shadow texels

	// Normalized size in texels
	NjFloat2	QuadMinTexel = World2TexelScale * QuadMin;
	NjFloat2	QuadMaxTexel = World2TexelScale * QuadMax;
	NjFloat2	QuadSizeTexel = QuadMaxTexel - QuadMinTexel;

	// Compute viewport size in texels
	int	ViewMinX = int( floorf( QuadMinTexel.x ) );
	int	ViewMinY = int( floorf( QuadMinTexel.y ) );
	int	ViewMaxX = int(  ceilf( QuadMaxTexel.x ) );
	int	ViewMaxY = int(  ceilf( QuadMaxTexel.y ) );

	m_ViewportWidth = ViewMaxX - ViewMinX;
	m_ViewportHeight = ViewMaxY - ViewMinY;

	NjFloat2	UVMin( (QuadMinTexel.x - ViewMinX) / SHADOW_MAP_SIZE, (QuadMinTexel.y - ViewMinY) / SHADOW_MAP_SIZE );
	NjFloat2	UVMax( (QuadMaxTexel.x - ViewMinX) / SHADOW_MAP_SIZE, (QuadMaxTexel.y - ViewMinY) / SHADOW_MAP_SIZE );
	NjFloat2	UVSize = UVMax - UVMin;

	//////////////////////////////////////////////////////////////////////////
	// Build the matrix to transform UVs into Light coordinates
	// We can write QuadPos = QuadMin + (QuadMax-QuadMin) / (UVmax-UVmin) * (UV - UVmin)
	// Unfolding, we obtain:
	//
	//	QuadPos = [QuadMin - (QuadMax-QuadMin) / (UVmax-UVmin) * UVmin] + [(QuadMax-QuadMin) / (UVmax-UVmin)] * UV
	//
	NjFloat2	Scale( QuadSize.x / UVSize.x, QuadSize.y / UVSize.y );
	NjFloat2	Offset = QuadMin - Scale * UVMin;

	NjFloat4x4	UV2Light;
	UV2Light.SetRow( 0, NjFloat4( Scale.x, 0, 0, 0 ) );
	UV2Light.SetRow( 1, NjFloat4( 0, Scale.y, 0, 0 ) );
	UV2Light.SetRow( 2, NjFloat4( 0, 0, 1, 0 ) );
	UV2Light.SetRow( 3, NjFloat4( Offset.x, Offset.y, 0, 1 ) );

	NjFloat4x4	Light2UV = UV2Light.Inverse();


// CHECK
NjFloat4	Test0 = NjFloat4( QuadMin.x, QuadMin.y, 0, 1 ) * Light2UV;	// Get back UV min/max
NjFloat4	Test1 = NjFloat4( QuadMax.x, QuadMax.y, 0, 1 ) * Light2UV;
NjFloat4	Test2 = NjFloat4( UVMin.x, UVMin.y, 0, 1 ) * UV2Light;			// Get back quad min/max
NjFloat4	Test3 = NjFloat4( UVMax.x, UVMax.y, 0, 1 ) * UV2Light;
// CHECK

	m_pCB_Shadow->m.LightDirection = NjFloat4( m_LightDirection, 0 );
	m_pCB_Shadow->m.World2Shadow = m_World2Light * Light2UV;
	m_pCB_Shadow->m.Shadow2World = UV2Light * Light2World;
	m_pCB_Shadow->m.ZMinMax.Set( 0, ZSize );


// CHECK: We reproject the frustum and verify the UVs...
NjFloat4	pCheckUVs[5];
for ( int i=0; i < 5; i++ )
	pCheckUVs[i] = NjFloat4( pCameraFrustumKm[i], 1 ) * m_pCB_Shadow->m.World2Shadow;
// CHECK


	// Create an alternate projection matrix that doesn't keep the World Z but instead projects it in [0,1]
// 	UV2Light.SetRow( 2, NjFloat4( 0, 0, ZSize, 0 ) );
// 
// 	m_Light2ShadowNormalized = UV2Light.Inverse();
}

// Computes the intersection of the 5 points camera frustum in WORLD space with a plane and returns the bounding quad of the intersected points
void	EffectVolumetric::ComputeFrustumIntersection( NjFloat3 _pCameraFrustumKm[5], float _PlaneHeight, NjFloat2& _QuadMin, NjFloat2& _QuadMax )
{
	int		pEdges[2*8] = {
		0, 1,
		0, 2,
		0, 3,
		0, 4,
		1, 2,
		2, 3,
		3, 4,
		4, 1
	};
	for ( int EdgeIndex=0; EdgeIndex < 8; EdgeIndex++ )
	{
		NjFloat3&	V0 = _pCameraFrustumKm[pEdges[2*EdgeIndex+0]];
		NjFloat3&	V1 = _pCameraFrustumKm[pEdges[2*EdgeIndex+1]];
		NjFloat3	V = V1 - V0;
		float		VerticalDistance = _PlaneHeight - V0.y;
		float		Distance2Intersection = VerticalDistance / V.y;	// Time until we reach the plane following V
		if ( Distance2Intersection < 0.0f || Distance2Intersection > 1.0f )
			continue;	// No intersection...

		NjFloat3	Intersection = V0 + Distance2Intersection * V;

		// Project to shadow plane
		float		Distance2ShadowPlane;
		NjFloat2	Projection = World2ShadowQuad( Intersection, Distance2ShadowPlane );

		// Update bounding-quad
		_QuadMin = _QuadMin.Min( Projection );
		_QuadMax = _QuadMax.Max( Projection );
	}
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
#define RES_3D_U		(RES_MU_S * RES_NU)	// Full texture will be 256*128*32

#define FILENAME_IRRADIANCE		"./TexIrradiance_64x16.bin"
#define FILENAME_TRANSMITTANCE	"./TexTransmittance_256x64.bin"
#define FILENAME_SCATTERING		"./TexScattering_256x128x32.bin"


struct	CBPreCompute
{
	NjFloat4	dUVW;
	bool		bFirstPass;	NjFloat3	__PAD1;
};

void	EffectVolumetric::PreComputeSkyTables()
{
	m_pRTTransmittance = new Texture2D( m_Device, TRANSMITTANCE_W, TRANSMITTANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );				// transmittance (final)
	m_pRTIrradiance = new Texture2D( m_Device, IRRADIANCE_W, IRRADIANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );						// irradiance (final)
	m_pRTInScattering = new Texture3D( m_Device, RES_3D_U, RES_MU, RES_R, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );							// inscatter (final)

#ifdef BUILD_SKY_SCATTERING

	Texture2D*	pRTDeltaIrradiance = new Texture2D( m_Device, IRRADIANCE_W, IRRADIANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );			// deltaE (temp)
	Texture3D*	pRTDeltaScatteringRayleigh = new Texture3D( m_Device, RES_3D_U, RES_MU, RES_R, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );		// deltaSR (temp)
	Texture3D*	pRTDeltaScatteringMie = new Texture3D( m_Device, RES_3D_U, RES_MU, RES_R, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );			// deltaSM (temp)
	Texture3D*	pRTDeltaScattering = new Texture3D( m_Device, RES_3D_U, RES_MU, RES_R, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );				// deltaJ (temp)

	Material*	pMatComputeTransmittance;
	Material*	pMatComputeIrradiance_Single;
	Material*	pMatComputeInScattering_Single;
	Material*	pMatComputeInScattering_Delta;
	Material*	pMatComputeIrradiance_Delta;
	Material*	pMatComputeInScattering_Multiple;
	Material*	pMatMergeInitialScattering;
	Material*	pMatAccumulateIrradiance;
	Material*	pMatAccumulateInScattering;

	CHECK_MATERIAL( pMatComputeInScattering_Delta = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", "GS", "PreComputeInScattering_Delta" ), 14 );			// inscatterS

	CHECK_MATERIAL( pMatComputeTransmittance = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PreComputeTransmittance" ), 10 );
	CHECK_MATERIAL( pMatComputeIrradiance_Single = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PreComputeIrradiance_Single" ), 11 );				// irradiance1
	CHECK_MATERIAL( pMatComputeIrradiance_Delta = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PreComputeIrradiance_Delta" ), 12 );				// irradianceN
	CHECK_MATERIAL( pMatComputeInScattering_Single = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", "GS", "PreComputeInScattering_Single" ), 13 );			// inscatter1
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

		m_Device.SetRenderTarget( *m_pRTTransmittance );

		CB.m.dUVW = NjFloat4( m_pRTTransmittance->GetdUV(), 0.0f );
		CB.UpdateData();

		m_ScreenQuad.Render( M );

	USING_MATERIAL_END

	// Assign to slot 7
	m_Device.RemoveRenderTargets();
	m_pRTTransmittance->SetVS( 7, true );
	m_pRTTransmittance->SetPS( 7, true );

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
	m_Device.ClearRenderTarget( *m_pRTIrradiance, NjFloat4::Zero );

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

		m_Device.SetRenderTarget( *m_pRTInScattering );

		CB.m.dUVW = m_pRTInScattering->GetdUVW();
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

			m_Device.SetRenderTarget( *m_pRTIrradiance );

			CB.m.dUVW = NjFloat4( m_pRTIrradiance->GetdUV(), 0 );
			CB.UpdateData();

			m_ScreenQuad.Render( M );

		USING_MATERIAL_END

		// ==================================================
 		// Adds deltaS into inscatter texture S (line 11 in algorithm 4.1)
		USING_MATERIAL_START( *pMatAccumulateInScattering )

			m_Device.SetRenderTarget( *m_pRTInScattering );

			CB.m.dUVW = m_pRTInScattering->GetdUVW();
			CB.UpdateData();

			m_ScreenQuad.RenderInstanced( M, RES_R );

		USING_MATERIAL_END

		m_Device.SetStates( NULL, NULL, m_Device.m_pBS_Disabled );
	}

	// Assign final textures to slots 8 & 9
	m_Device.RemoveRenderTargets();
	m_pRTInScattering->SetVS( 8, true );
	m_pRTInScattering->SetPS( 8, true );
	m_pRTIrradiance->SetVS( 9, true );
	m_pRTIrradiance->SetPS( 9, true );

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

	// Save tables
	Texture3D*	pStagingScattering = new Texture3D( m_Device, RES_3D_U, RES_MU, RES_R, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true );
	Texture2D*	pStagingTransmittance = new Texture2D( m_Device, TRANSMITTANCE_W, TRANSMITTANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true );
	Texture2D*	pStagingIrradiance = new Texture2D( m_Device, IRRADIANCE_W, IRRADIANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true );

	pStagingScattering->CopyFrom( *m_pRTInScattering );
	pStagingTransmittance->CopyFrom( *m_pRTTransmittance );
	pStagingIrradiance->CopyFrom( *m_pRTIrradiance );

	pStagingIrradiance->Save( FILENAME_IRRADIANCE );
	pStagingTransmittance->Save( FILENAME_TRANSMITTANCE );
	pStagingScattering->Save( FILENAME_SCATTERING );

	delete pStagingIrradiance;
	delete pStagingTransmittance;
	delete pStagingScattering;

#else
	// Load tables
	Texture2D*	pStagingTransmittance = new Texture2D( m_Device, TRANSMITTANCE_W, TRANSMITTANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true, true );
	Texture3D*	pStagingScattering = new Texture3D( m_Device, RES_3D_U, RES_MU, RES_R, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true, true );
	Texture2D*	pStagingIrradiance = new Texture2D( m_Device, IRRADIANCE_W, IRRADIANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true, true );

#if 1
	BuildTransmittanceTable( TRANSMITTANCE_W, TRANSMITTANCE_H, *pStagingTransmittance );
#else
	pStagingTransmittance->Load( FILENAME_TRANSMITTANCE );
#endif

	pStagingIrradiance->Load( FILENAME_IRRADIANCE );
	pStagingScattering->Load( FILENAME_SCATTERING );

	m_pRTTransmittance->CopyFrom( *pStagingTransmittance );
	m_pRTInScattering->CopyFrom( *pStagingScattering );
	m_pRTIrradiance->CopyFrom( *pStagingIrradiance );

	delete pStagingIrradiance;
	delete pStagingTransmittance;
	delete pStagingScattering;

	m_pRTTransmittance->SetVS( 7, true );
	m_pRTTransmittance->SetPS( 7, true );
	m_pRTInScattering->SetVS( 8, true );
	m_pRTInScattering->SetPS( 8, true );
	m_pRTIrradiance->SetVS( 9, true );
	m_pRTIrradiance->SetPS( 9, true );

#endif
}

void	EffectVolumetric::FreeSkyTables()
{
	delete m_pRTInScattering;
	delete m_pRTIrradiance;
	delete m_pRTTransmittance;
}

//////////////////////////////////////////////////////////////////////////
// This computes the transmittance of Sun light as seen through the atmosphere
// This results in a 2D table with [CosSunDirection, AltitudeKm] as the 2 entries.
// It's done with the CPU because:
//	1] It's fast to compute
//	2] We need to access it from the CPU to compute the Sun's intensity & color for the directional light
//
void	EffectVolumetric::BuildTransmittanceTable( int _Width, int _Height, Texture2D& _StagingTexture ) {

	if ( m_pTableTransmittance != NULL ) {
		delete[] m_pTableTransmittance;
	}

	m_pTableTransmittance = new NjFloat3[_Width*_Height];

	float	HRefRayleigh = 8.0f;
	float	HRefMie = 1.2f;

	float	Sigma_s_Mie = 0.004f;	// !!!May cause strong optical depths and very low values if increased!!
	NjFloat3	Sigma_t_Mie = (Sigma_s_Mie / 0.9f) * NjFloat3::One;	// Should this be a parameter as well?? People might set it to values > 1 and that's physically incorrect...
	NjFloat3	Sigma_s_Rayleigh( 0.0058f, 0.0135f, 0.0331f );
								  
NjFloat3	MaxopticalDepth = NjFloat3::Zero;
int		MaxOpticalDepthX = -1;
int		MaxOpticalDepthY = -1;
	NjFloat2	UV;
	for ( int Y=0; Y < _Height; Y++ ) {
		UV.y = Y / (_Height-1.0f);
		float	AltitudeKm = UV.y*UV.y * ATMOSPHERE_THICKNESS_KM;					// Grow quadratically to have more precision near the ground

#ifdef USE_PRECISE_COS_THETA_MIN
		float	RadiusKm = GROUND_RADIUS_KM + AltitudeKm;
		float	CosThetaMin = -1e-3f - sqrtf( 1.0f - GROUND_RADIUS_KM*GROUND_RADIUS_KM / (RadiusKm*RadiusKm) );	// -0.13639737868529368408722196006097 at 60km
#else
		float	CosThetaMin = -0.15f;
#endif

		NjFloat3*	scanline = m_pTableTransmittance + _Width * Y;

		for ( int X=0; X < _Width; X++, scanline++ ) {	// CosTheta changes sign at X=0xB8 (UV.x = 71%) ==> 0xB7=-0.00226515974 & 0xB8=+0.00191573682
			UV.x = float(X) / _Width;

			float	t = tan( TRANSMITTANCE_TAN_MAX * UV.x ) / tan(TRANSMITTANCE_TAN_MAX);	// Grow tangentially to have more precision horizontally
// 			float	t = UV.x;									// Grow linearly
// 			float	t = UV.x*UV.x;								// Grow quadratically
//			float	CosTheta = LERP( -0.15f, 1.0f, t );
			float	CosTheta = LERP( CosThetaMin, 1.0f, t );


// const float	CosThetaEps = 8e-3f;
// if ( CosTheta > 0.0f && CosTheta < CosThetaEps )	CosTheta = CosThetaEps;
// if ( CosTheta < 0.0f && CosTheta > -CosThetaEps )	CosTheta = -CosThetaEps;

if ( CosTheta > 0.0f )
	CosTheta = LERP( 0.02f, 1.0f, CosTheta );
else
	CosTheta = LERP( -0.02f, -1.0f, -CosTheta );


			bool	groundHit = false;
			NjFloat3	OpticalDepth = Sigma_s_Rayleigh * ComputeOpticalDepth( AltitudeKm, CosTheta, HRefRayleigh, groundHit ) + Sigma_t_Mie * ComputeOpticalDepth( AltitudeKm, CosTheta, HRefMie, groundHit );
			if ( groundHit ) {
				scanline->Set( 1e-4f, 1e-4f, 1e-4f );	// Special case...
				continue;
			}

if ( OpticalDepth.z > MaxopticalDepth.z ) {
	MaxopticalDepth = OpticalDepth;
	MaxOpticalDepthX = X;
	MaxOpticalDepthY = Y;
}

			// Here, the blue channel's optical depth's max value has been reported to be 19.6523819
			//	but the minimum supported value for Half16 has been measured to be something like 6.10351563e-5 (equivalent to d = -ln(6.10351563e-5) = 9.7040605270200343321767940202312)
			// What I'm doing here is patching very long optical depths in the blue channel to remap
			//	the [8,19.6523819] interval into [8,9.704061]
			//
			static const float	MAX_OPTICAL_DEPTH = 19.652382f;
			if ( OpticalDepth.z > 8.0f ) {
				OpticalDepth.z = 8.0f + (9.70f-8.0f) * SATURATE( (OpticalDepth.z - 8.0f) / (MAX_OPTICAL_DEPTH-8.0f) );
			}

			scanline->Set( expf( -OpticalDepth.x ), expf( -OpticalDepth.y ), expf( -OpticalDepth.z )  );

scanline->Set( 1e-4f+expf( -OpticalDepth.x ), 1e-4f+expf( -OpticalDepth.y ), 1e-4f+expf( -OpticalDepth.z )  );

// 			scanline->x = 1.0f - idMath::Pow( scanline->x, 1.0f/8 );
// 			scanline->y = 1.0f - idMath::Pow( scanline->y, 1.0f/8 );
// 			scanline->z = 1.0f - idMath::Pow( scanline->z, 1.0f/8 );

#ifdef _DEBUG
// CHECK Ensure we never get 0 from a non 0 value
NjHalf	TestX( scanline->x );
if ( scanline->x != 0.0f && TestX.raw == 0 ) {
	DebugBreak();
}
NjHalf	TestY( scanline->y );
if ( scanline->y != 0.0f && TestY.raw == 0 ) {
	DebugBreak();
}
NjHalf	TestZ( scanline->z );
if ( scanline->z != 0.0f && TestZ.raw == 0 ) {
	DebugBreak();
}
// CHECK
#endif
		}
	}




#ifdef _DEBUG
float	MaxHitDistanceKm = SphereIntersectionExit( NjFloat3::Zero, NjFloat3::UnitX, ATMOSPHERE_THICKNESS_KM );
NjFloat3	Test0 = GetTransmittance( 0.0f, cosf( NUAJDEG2RAD(90.0f + 0.5f) ), 10.0f );
NjFloat3	Test1 = GetTransmittance( 0.0f, cosf( HALFPI ), MaxHitDistanceKm );
#endif




	// Build an actual RGBA16F texture from this table
	{
		D3D11_MAPPED_SUBRESOURCE	LockedResource = _StagingTexture.Map( 0, 0 );
		NjHalf4*	pTarget = (NjHalf4*) LockedResource.pData;
		for ( int Y=0; Y < _Height; Y++ ) {
			NjFloat3*	pScanlineSource = m_pTableTransmittance + _Width*Y;
			NjHalf4*	pScanlineTarget = pTarget + _Width*Y;
			for ( int X=0; X < _Width; X++, pScanlineSource++, pScanlineTarget++ ) {

				*pScanlineTarget = NjFloat4( *pScanlineSource, 0 );
			}
		}
		_StagingTexture.UnMap( 0, 0 );
	}
}

float		EffectVolumetric::ComputeOpticalDepth( float _AltitudeKm, float _CosTheta, const float _Href, bool& _bGroundHit, int _StepsCount ) const
{
	// Compute distance to atmosphere or ground, whichever comes first
	NjFloat4	PositionKm = NjFloat4( 0.0f, 1e-2f + _AltitudeKm, 0.0f, 0.0f );
	NjFloat3	View = NjFloat3( sqrtf( 1.0f - _CosTheta*_CosTheta ), _CosTheta, 0.0f );
	float	TraceDistanceKm = ComputeNearestHit( PositionKm, View, ATMOSPHERE_THICKNESS_KM, _bGroundHit );
	if ( _bGroundHit )
		return 1e5f;	// Completely opaque due to hit with ground: no light can come this way...
						// Be careful with large values in 16F!

	NjFloat3	EarthCenterKm( 0, -GROUND_RADIUS_KM, 0 );

	float	Result = 0.0;
	NjFloat4	StepKm = (TraceDistanceKm / _StepsCount) * NjFloat4( View, 1.0 );

	// Integrate until the hit
	float	PreviousAltitudeKm = _AltitudeKm;
	for ( int i=0; i < _StepsCount; i++ )
	{
		PositionKm = PositionKm + StepKm;
		_AltitudeKm = (NjFloat3(PositionKm) - EarthCenterKm).Length() - GROUND_RADIUS_KM;
		Result += expf( (PreviousAltitudeKm + _AltitudeKm) * (-0.5f / _Href) );	// Gives the integral of a linear interpolation in altitude
		PreviousAltitudeKm = _AltitudeKm;
	}

	return Result * StepKm.w;
}

NjFloat3	EffectVolumetric::GetTransmittance( float _AltitudeKm, float _CosTheta ) const
{
	float	NormalizedAltitude = sqrtf( max( 0.0f, _AltitudeKm ) * (1.0f / ATMOSPHERE_THICKNESS_KM) );

#ifdef USE_PRECISE_COS_THETA_MIN
	float	RadiusKm = GROUND_RADIUS_KM + _AltitudeKm;
	float	CosThetaMin = -sqrt( 1.0f - (GROUND_RADIUS_KM*GROUND_RADIUS_KM) / (RadiusKm*RadiusKm) );
#else
	float	CosThetaMin = -0.15;
#endif
 	float	NormalizedCosTheta = atan( (_CosTheta - CosThetaMin) / (1.0f - CosThetaMin) * tan(TRANSMITTANCE_TAN_MAX) ) / TRANSMITTANCE_TAN_MAX;

	NjFloat2	UV( NormalizedCosTheta, NormalizedAltitude );

	NjFloat3	Result = SampleTransmittance( UV );
	return Result;
}

NjFloat3	EffectVolumetric::GetTransmittance( float _AltitudeKm, float _CosTheta, float _DistanceKm ) const
{
	// P0 = [0, _RadiusKm]
	// V  = [SinTheta, CosTheta]
	//
	float	RadiusKm = GROUND_RADIUS_KM + _AltitudeKm;
	float	RadiusKm2 = sqrt( RadiusKm*RadiusKm + _DistanceKm*_DistanceKm + 2.0f * RadiusKm * _CosTheta * _DistanceKm );	// sqrt[ (P0 + d.V)² ]
	float	CosTheta2 = (RadiusKm * _CosTheta + _DistanceKm) / RadiusKm2;													// dot( P0 + d.V, V ) / RadiusKm2
	float	AltitudeKm2 = RadiusKm2 - GROUND_RADIUS_KM;

	NjFloat3	T0, T1;
	if ( _CosTheta > -1e-3f )
	{
		T0 = GetTransmittance( _AltitudeKm, _CosTheta );
		T1 = GetTransmittance( AltitudeKm2, CosTheta2 );
	}
	else
	{
		T0 = GetTransmittance( AltitudeKm2, -CosTheta2 );
		T1 = GetTransmittance( _AltitudeKm, -_CosTheta );
	}

	NjFloat3	Result = T0 / T1;

//CHECK No really 16-bits precision computation...
NjHalf4		T0Half = NjHalf4( NjFloat4( T0, 0 ) );
NjHalf4		T1Half = NjHalf4( NjFloat4( T1, 0 ) );
NjFloat4	ResultHalf = NjHalf4( NjFloat4( T0Half ) / NjFloat4( T1Half ) );
//CHECK

	return Result;
}

NjFloat3	EffectVolumetric::SampleTransmittance( const NjFloat2 _UV ) const {

	float	X = _UV.x * (TRANSMITTANCE_W-1);
	float	Y = _UV.y * TRANSMITTANCE_H;
	int		X0 = floorf( X );
	X0 = CLAMP( 0, TRANSMITTANCE_W-1, X0 );
	float	x = X - X0;
	int		Y0 = floorf( Y );
	Y0 = CLAMP( 0, TRANSMITTANCE_H-1, Y0 );
	float	y = Y - Y0;
	int		X1 = MIN( TRANSMITTANCE_W-1, X0+1 );
	int		Y1 = MIN( TRANSMITTANCE_H-1, Y0+1 );

	// Bilerp values
	const NjFloat3&	V00 = m_pTableTransmittance[TRANSMITTANCE_W*Y0+X0];
	const NjFloat3&	V01 = m_pTableTransmittance[TRANSMITTANCE_W*Y0+X1];
	const NjFloat3&	V10 = m_pTableTransmittance[TRANSMITTANCE_W*Y1+X0];
	const NjFloat3&	V11 = m_pTableTransmittance[TRANSMITTANCE_W*Y1+X1];
	NjFloat3	V0 = V00 + x * (V01 - V00);
	NjFloat3	V1 = V10 + x * (V11 - V10);
	NjFloat3	V = V0 + y * (V1 - V0);
	return V;
}

////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////
// Planetary Helpers
//
void	EffectVolumetric::ComputeSphericalData( const NjFloat3& _PositionKm, float& _AltitudeKm, NjFloat3& _Normal ) const
{
	NjFloat3	EarthCenterKm( 0, -GROUND_RADIUS_KM, 0 );
	NjFloat3	Center2Position = _PositionKm - EarthCenterKm;
	float	Radius2PositionKm = Center2Position.Length();
	_AltitudeKm = Radius2PositionKm - GROUND_RADIUS_KM;
	_Normal = Center2Position / Radius2PositionKm;
}

// ====== Intersections ======

// Computes the enter intersection of a ray and a sphere
// (No check for validity!)
float	EffectVolumetric::SphereIntersectionEnter( const NjFloat3& _PositionKm, const NjFloat3& _View, float _SphereAltitudeKm ) const
{
	NjFloat3	EarthCenterKm( 0, -GROUND_RADIUS_KM, 0 );
	float	R = _SphereAltitudeKm + GROUND_RADIUS_KM;
	NjFloat3	D = _PositionKm - EarthCenterKm;
	float	c = (D | D) - R*R;
	float	b = D | _View;

	float	Delta = b*b - c;

	return -b - sqrt(Delta);
}

// Computes the exit intersection of a ray and a sphere
// (No check for validity!)
float	EffectVolumetric::SphereIntersectionExit( const NjFloat3& _PositionKm, const NjFloat3& _View, float _SphereAltitudeKm ) const
{
	NjFloat3	EarthCenterKm( 0, -GROUND_RADIUS_KM, 0 );
	float	R = _SphereAltitudeKm + GROUND_RADIUS_KM;
	NjFloat3	D = _PositionKm - EarthCenterKm;
	float	c = (D | D) - R*R;
	float	b = D | _View;

	float	Delta = b*b - c;

	return -b + sqrt(Delta);
}

// Computes both intersections of a ray and a sphere
// Returns INFINITY if no hit is found
void	EffectVolumetric::SphereIntersections( const NjFloat3& _PositionKm, const NjFloat3& _View, float _SphereAltitudeKm, NjFloat2& _Hits ) const
{
	NjFloat3	EarthCenterKm( 0, -GROUND_RADIUS_KM, 0 );
	float	R = _SphereAltitudeKm + GROUND_RADIUS_KM;
	NjFloat3	D = _PositionKm - EarthCenterKm;
	float	c = (D | D) - R*R;
	float	b = D | _View;

	float	Delta = b*b - c;
	if ( Delta < 0.0 ) {
		_Hits.Set( 1e6f, 1e6f );
		return;
	}

	Delta = sqrt(Delta);

	_Hits.Set( -b - Delta, -b + Delta );
}

// Computes the nearest hit between provided sphere and ground sphere
float	EffectVolumetric::ComputeNearestHit( const NjFloat3& _PositionKm, const NjFloat3& _View, float _SphereAltitudeKm, bool& _IsGround ) const
{
	NjFloat2	GroundHits;
	SphereIntersections( _PositionKm, _View, 0.0, GroundHits );
	float	SphereHit = SphereIntersectionExit( _PositionKm, _View, _SphereAltitudeKm );

	_IsGround = false;
	if ( GroundHits.x < 0.0f || SphereHit < GroundHits.x )
		return SphereHit;	// We hit the top of the atmosphere...
	
	// We hit the ground first
	_IsGround = true;
	return GroundHits.x;
}
