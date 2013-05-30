#include "../../GodComplex.h"
#include "EffectVolumetric.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

//#define BUILD_SKY_SCATTERING	// Build or load? (warning: the computation shader takes hell of a time to compile!) (but the computation itself takes less than a second! ^^)

static const int	TERRAIN_SUBDIVISIONS_COUNT = 200;	// Don't push it over 254 or it will crash due to more than 65536 vertices!
static const float	TERRAIN_SIZE = 100.0f;

static const float	CLOUD_SIZE = 100.0f;

static const float	SCREEN_TARGET_RATIO = 0.5f;

static const float	GROUND_RADIUS_KM = 6360.0f;
static const float	ATMOSPHERE_THICKNESS_KM = 60.0f;

static const float	TRANSMITTANCE_TAN_MAX = 1.5f;	// Close to PI/2 to maximize precision at grazing angles
//#define USE_PRECISE_COS_THETA_MIN


EffectVolumetric::EffectVolumetric( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, Camera& _Camera ) : m_Device( _Device ), m_RTHDR( _RTHDR ), m_ScreenQuad( _ScreenQuad ), m_Camera( _Camera ), m_ErrorCode( 0 ), m_pTableTransmittance( NULL )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
 	CHECK_MATERIAL( m_pMatDepthWrite = CreateMaterial( IDR_SHADER_VOLUMETRIC_DEPTH_WRITE, "./Resources/Shaders/VolumetricDepthWrite.hlsl", VertexFormatP3::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
 	CHECK_MATERIAL( m_pMatSplatCameraFrustum = CreateMaterial( IDR_SHADER_VOLUMETRIC_COMPUTE_TRANSMITTANCE, "./Resources/Shaders/VolumetricComputeTransmittance.hlsl", VertexFormatP3::DESCRIPTOR, "VS_SplatFrustum", NULL, "PS_SplatFrustum" ), 2 );
 	CHECK_MATERIAL( m_pMatComputeTransmittance = CreateMaterial( IDR_SHADER_VOLUMETRIC_COMPUTE_TRANSMITTANCE, "./Resources/Shaders/VolumetricComputeTransmittance.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 3 );
 	CHECK_MATERIAL( m_pMatDisplay = CreateMaterial( IDR_SHADER_VOLUMETRIC_DISPLAY, "./Resources/Shaders/VolumetricDisplay.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 4 );
 	CHECK_MATERIAL( m_pMatCombine = CreateMaterial( IDR_SHADER_VOLUMETRIC_COMBINE, "./Resources/Shaders/VolumetricCombine.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 5 );

#ifdef SHOW_TERRAIN
	CHECK_MATERIAL( m_pMatTerrainShadow = CreateMaterial( IDR_SHADER_VOLUMETRIC_TERRAIN, "./Resources/Shaders/VolumetricTerrain.hlsl", VertexFormatP3::DESCRIPTOR, "VS", NULL, NULL ), 6 );
	CHECK_MATERIAL( m_pMatTerrain = CreateMaterial( IDR_SHADER_VOLUMETRIC_TERRAIN, "./Resources/Shaders/VolumetricTerrain.hlsl", VertexFormatP3::DESCRIPTOR, "VS", NULL, "PS" ), 7 );
#endif

//	const char*	pCSO = LoadCSO( "./Resources/Shaders/CSO/VolumetricCombine.cso" );
//	CHECK_MATERIAL( m_pMatCombine = CreateMaterial( IDR_SHADER_VOLUMETRIC_COMBINE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, pCSO ), 4 );
//	delete[] pCSO;


	//////////////////////////////////////////////////////////////////////////
	// Pre-Compute multiple-scattering tables

// I think it would be better to allocate [1] targets with the D3D11_BIND_UNORDERED_ACCESS flag and COPY the resource once it's been computed,
// 	rather than exchanging pointers. I'm not sure the device won't consider targets with the UAV and RT and SRV flags slower??
//
// After testing: Performance doesn't seem to be affected at all...

#define UAV	true
//#define UAV	false

	m_ppRTTransmittance[0] = new Texture2D( m_Device, TRANSMITTANCE_W, TRANSMITTANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, false, false, UAV );				// transmittance (final)
	m_ppRTTransmittance[1] = new Texture2D( m_Device, TRANSMITTANCE_W, TRANSMITTANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, false, false, UAV );
	m_ppRTIrradiance[0] = new Texture2D( m_Device, IRRADIANCE_W, IRRADIANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, false, false, UAV );							// irradiance (final)
	m_ppRTIrradiance[1] = new Texture2D( m_Device, IRRADIANCE_W, IRRADIANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, false, false, UAV );
	m_ppRTIrradiance[2] = new Texture2D( m_Device, IRRADIANCE_W, IRRADIANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, false, false, UAV );
	m_ppRTInScattering[0] = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, false, false, UAV );	// inscatter (final)
	m_ppRTInScattering[1] = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, false, false, UAV );
	m_ppRTInScattering[2] = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, false, false, UAV );

	PreComputeSkyTables();

	InitUpdateSkyTables();


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

#ifdef SHOW_TERRAIN
	m_pRTTerrainShadow = new Texture2D( m_Device, TERRAIN_SHADOW_MAP_SIZE, TERRAIN_SHADOW_MAP_SIZE, DepthStencilFormatD32F::DESCRIPTOR );
#endif

	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_Object = new CB<CBObject>( m_Device, 10 );
	m_pCB_Splat = new CB<CBSplat>( m_Device, 10 );
	m_pCB_Atmosphere = new CB<CBAtmosphere>( m_Device, 7, true );
	m_pCB_Shadow = new CB<CBShadow>( m_Device, 8, true );
	m_pCB_Volume = new CB<CBVolume>( m_Device, 9, true );

	//////////////////////////////////////////////////////////////////////////
	// Setup our volume & light
	m_Position = NjFloat3( 0.0f, 2.0f, 0.0f );
	m_Rotation = NjFloat4::QuatFromAngleAxis( 0.0f, NjFloat3::UnitY );
	m_Scale = NjFloat3( 1.0f, 2.0f, 1.0f );

	m_CloudAnimSpeedLoFreq = 1.0f;
	m_CloudAnimSpeedHiFreq = 1.0f;

	{
		float	SunPhi = 0.0f;
		float	SunTheta = 0.25f * PI;
		m_pCB_Atmosphere->m.LightDirection.Set( sinf(SunPhi)*sinf(SunTheta), cosf(SunTheta), -cosf(SunPhi)*sinf(SunTheta) );
	}

#ifdef _DEBUG
	m_pMMF = new MMF<ParametersBlock>( "BisouTest" );
	ParametersBlock	Params = {
		1, // WILL BE MARKED AS CHANGED!			// U32		Checksum;

		// // Atmosphere Params
		0.25f,		// float	SunTheta;
		0.0f,		// float	SunPhi;
		100.0f,		// float	SunIntensity;
		1.0f,		// float	AirAmount;		// Simply a multiplier of the default value
		0.004f,		// float	FogScattering;
		0.004f / 0.9f,		// float	FogExtinction;
		8.0f,		// float	AirReferenceAltitudeKm;
		1.2f,		// float	FogReferenceAltitudeKm;
		0.76f,		// float	FogAnisotropy;
		0.1f,		// float	AverageGroundReflectance;
		0.9f,		// float	GodraysStrength;
		-1.0f,		// float	AltitudeOffset;

		// // Volumetrics Params
		4.0f,		// float	CloudBaseAltitude;
		2.0f,		// float	CloudThickness;
		8.0f,		// float	CloudExtinction;
		8.0f,		// float	CloudScattering;
		0.1f,		// float	CloudAnisotropyIso;
		0.85f,		// float	CloudAnisotropyForward;
		0.9f,		// float	CloudShadowStrength;
				// 
		0.1f,		// float	CloudIsotropicScattering;	// Sigma_s for isotropic lighting
		1.0f,		// float	CloudIsoSkyRadianceFactor;
		0.25f,		// float	CloudIsoSunRadianceFactor;
		0.2f,		// float	CloudIsoTerrainReflectanceFactor;

		// // Noise Params
				// 	// Low frequency noise
		7.5f,		// float	NoiseLoFrequency;		// Horizontal frequency
		1.0f,		// float	NoiseLoVerticalLooping;	// Vertical frequency in amount of noise pixels
		1.0f,		// float	NoiseLoAnimSpeed;		// Animation speed
				// 	// High frequency noise
		0.12f,		// float	NoiseHiFrequency;
		0.01f,		// float	NoiseHiOffset;			// Second noise is added to first noise using NoiseHiStrength * (HiFreqNoise + NoiseHiOffset)
		-0.707f,	// float	NoiseHiStrength;
		1.0f,		// float	NoiseHiAnimSpeed;
				// 	// Combined noise params
		-0.16f,		// float	NoiseOffsetBottom;		// The noise offset to add when at the bottom altitude in the cloud
		0.01f,		// float	NoiseOffsetMiddle;		// The noise offset to add when at the middle altitude in the cloud
		-0.16f,		// float	NoiseOffsetTop;			// The noise offset to add when at the top altitude in the cloud
		1.0f,		// float	NoiseContrast;			// Final noise value is Noise' = pow( Contrast*(Noise+Offset), Gamma )
		0.5f,		// float	NoiseGamma;
				// 	// Final shaping params
		0.01f,		// float	NoiseShapingPower;		// Final noise value is shaped (multiplied) by pow( 1-abs(2*y-1), NoiseShapingPower ) to avoid flat plateaus at top or bottom

		// // Terrain Params
		1,			// int		TerrainEnabled;
		10.0f,		// float	TerrainHeight;
		2.0f,		// float	TerrainAlbedoMultiplier;
		0.9f,		// float	TerrainCloudShadowStrength;

	};
	EffectVolumetric::ParametersBlock&	MappedParams = m_pMMF->GetMappedMemory();

	// Copy our default params only if the checksum is 0 (meaning the control panel isn't loaded and hasn't set any valu yet)
	if ( MappedParams.Checksum == 0 )
		MappedParams = Params;

	m_CloudAnimSpeedLoFreq = Params.NoiseLoAnimSpeed;
	m_CloudAnimSpeedHiFreq = Params.NoiseHiAnimSpeed;

#endif

	m_pCB_Volume->m._CloudLoFreqPositionOffset.Set( 0, 0 );
	m_pCB_Volume->m._CloudHiFreqPositionOffset.Set( 0, 0 );
}

EffectVolumetric::~EffectVolumetric()
{
#ifdef _DEBUG
	delete m_pMMF;
#endif

	delete m_pCB_Volume;
	delete m_pCB_Shadow;
	delete m_pCB_Atmosphere;
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
	delete m_pRTTerrainShadow;
	delete m_pMatTerrainShadow;
	delete m_pMatTerrain;
	delete m_pPrimTerrain;
#endif
	delete m_pPrimFrustum;
	delete m_pPrimBox;

	ExitUpdateSkyTables();

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
float	t = 2*0.25f * _Time;
//m_LightDirection.Set( 0, 1, -1 );
//m_LightDirection.Set( 1, 2.0, -5 );
//m_LightDirection.Set( cosf(_Time), 2.0f * sinf( 0.324f * _Time ), sinf( _Time ) );
//m_LightDirection.Set( cosf(_Time), 1.0f, sinf( _Time ) );


	// Animate cloud position
	m_pCB_Volume->m._CloudLoFreqPositionOffset = m_pCB_Volume->m._CloudLoFreqPositionOffset + (_DeltaTime * m_CloudAnimSpeedLoFreq) * NjFloat2( 0.66f, -1.66f );
	m_pCB_Volume->m._CloudHiFreqPositionOffset = m_pCB_Volume->m._CloudHiFreqPositionOffset + (_DeltaTime * m_CloudAnimSpeedHiFreq) * NjFloat2( 0.0f, -0.08f );


#ifdef _DEBUG
	if ( m_pMMF->CheckForChange() )
	{
		ParametersBlock&	Params = m_pMMF->GetMappedMemory();

		//////////////////////////////////////////////////////////////////////////
		// Check if any change in params requires a sky table rebuild
		bool	bRequireSkyUpdate = false;

		bRequireSkyUpdate |= !ALMOST( m_pCB_Atmosphere->m.AirParams.x, Params.AirAmount );
		bRequireSkyUpdate |= !ALMOST( m_pCB_Atmosphere->m.AirParams.y, Params.AirReferenceAltitudeKm );
		bRequireSkyUpdate |= !ALMOST( m_pCB_Atmosphere->m.FogParams.x, Params.FogScattering );
		bRequireSkyUpdate |= !ALMOST( m_pCB_Atmosphere->m.FogParams.y, Params.FogExtinction );
		bRequireSkyUpdate |= !ALMOST( m_pCB_Atmosphere->m.FogParams.z, Params.FogReferenceAltitudeKm );
		bRequireSkyUpdate |= !ALMOST( m_pCB_Atmosphere->m.FogParams.w, Params.FogAnisotropy );
		bRequireSkyUpdate |= !ALMOST( m_pCB_PreComputeSky->m._AverageGroundReflectance, Params.AverageGroundReflectance );


		//////////////////////////////////////////////////////////////////////////
		// Atmosphere Params
		m_pCB_Atmosphere->m.LightDirection.Set( sinf(Params.SunPhi)*sinf(Params.SunTheta), cosf(Params.SunTheta), -cosf(Params.SunPhi)*sinf(Params.SunTheta) );
		m_pCB_Atmosphere->m.SunIntensity = Params.SunIntensity;

		m_pCB_Atmosphere->m.AirParams.Set( Params.AirAmount, Params.AirReferenceAltitudeKm );
		m_pCB_Atmosphere->m.GodraysStrength = Params.GodraysStrength;
		m_pCB_Atmosphere->m.AltitudeOffset = Params.AltitudeOffset;

		m_pCB_Atmosphere->m.FogParams.Set( Params.FogScattering, Params.FogExtinction, Params.FogReferenceAltitudeKm, Params.FogAnisotropy );

		m_pCB_Atmosphere->UpdateData();

		m_pCB_PreComputeSky->m._AverageGroundReflectance = Params.AverageGroundReflectance;

		if ( bRequireSkyUpdate )
			TriggerSkyTablesUpdate();	// Rebuild tables if change in atmosphere params!


		//////////////////////////////////////////////////////////////////////////
		// Volumetric Params
		m_CloudAltitude = Params.CloudBaseAltitude;
		m_CloudThickness = Params.CloudThickness;
// 		m_Position.Set( 0, Params.CloudBaseAltitude + 0.5f * Params.CloudThickness, -100 );
 		m_Scale.Set( 0.5f * CLOUD_SIZE, 0.5f * Params.CloudThickness, 0.5f * CLOUD_SIZE );

		m_pCB_Volume->m._CloudAltitudeThickness.Set( Params.CloudBaseAltitude, Params.CloudThickness );
		m_pCB_Volume->m._CloudExtinctionScattering.Set( Params.CloudExtinction, Params.CloudScattering );
		m_pCB_Volume->m._CloudPhases.Set( Params.CloudAnisotropyIso, Params.CloudAnisotropyForward );
		m_pCB_Volume->m._CloudShadowStrength = Params.CloudShadowStrength;

		// Isotropic lighting
		m_pCB_Volume->m._CloudIsotropicScattering = Params.CloudIsotropicScattering;
		m_pCB_Volume->m._CloudIsotropicFactors.Set( Params.CloudIsoSkyRadianceFactor, Params.CloudIsoSunRadianceFactor, Params.CloudIsoTerrainReflectanceFactor );

		// Noise
		m_pCB_Volume->m._CloudLoFreqParams.Set( Params.NoiseLoFrequency, Params.NoiseLoVerticalLooping );
		m_pCB_Volume->m._CloudHiFreqParams.Set( Params.NoiseHiFrequency, Params.NoiseHiOffset, Params.NoiseHiStrength );

		m_CloudAnimSpeedLoFreq = Params.NoiseLoAnimSpeed;
		m_CloudAnimSpeedHiFreq = Params.NoiseHiAnimSpeed;

		float	HalfMiddleOffset = Params.NoiseOffsetMiddle;
		m_pCB_Volume->m._CloudOffsets.Set( Params.NoiseOffsetBottom - HalfMiddleOffset, HalfMiddleOffset, Params.NoiseOffsetTop - HalfMiddleOffset );

		m_pCB_Volume->m._CloudContrastGamma.Set( Params.NoiseContrast, Params.NoiseGamma );
		m_pCB_Volume->m._CloudShapingPower = Params.NoiseShapingPower;

		m_pCB_Volume->UpdateData();


		//////////////////////////////////////////////////////////////////////////
		// Terrain Params
		m_bShowTerrain = Params.TerrainEnabled == 1;
		m_pCB_Object->m.TerrainHeight = Params.TerrainHeight;
		m_pCB_Object->m.AlbedoMultiplier = Params.TerrainAlbedoMultiplier;
		m_pCB_Object->m.CloudShadowStrength = Params.TerrainCloudShadowStrength;
	}
#endif

// float	SunAngle = LERP( -0.01f * PI, 0.499f * PI, 0.5f * (1.0f + sinf( t )) );		// Oscillating between slightly below horizon to zenith
// //float	SunAngle = 0.021f * PI;
// //float	SunAngle = -0.0001f * PI;
// 
// // SunAngle = _TV( 0.12f );
// //SunAngle = -0.015f * PI;	// Sexy Sunset
// //SunAngle = 0.15f * PI;	// Sexy Sunset
// 
// float	SunPhi = 0.5923f * t;
// m_LightDirection.Set( sinf( SunPhi ), sinf( SunAngle ), -cosf( SunPhi ) );
// //m_LightDirection.Set( 0.0, sinf( SunAngle ), -cosf( SunAngle ) );

// DEBUG


#ifdef _DEBUG
// 	if ( gs_WindowInfos.pKeys[VK_NUMPAD1] )
// 		m_pCB_Volume->m.Params.x -= 0.5f * _DeltaTime;
// 	if ( gs_WindowInfos.pKeys[VK_NUMPAD7] )
// 		m_pCB_Volume->m.Params.x += 0.5f * _DeltaTime;
// 
// 	m_pCB_Volume->m.Params.y = gs_WindowInfos.pKeys[VK_RETURN];
#endif

	m_pCB_Volume->UpdateData();

	if ( m_pTexFractal0 != NULL )
		m_pTexFractal0->SetPS( 16 );
	if ( m_pTexFractal1 != NULL )
		m_pTexFractal1->SetPS( 17 );


	//////////////////////////////////////////////////////////////////////////
	// Perform time-sliced update of the sky table if needed
	UpdateSkyTables();

	//////////////////////////////////////////////////////////////////////////
	// Compute transforms
	PERF_BEGIN_EVENT( D3DCOLOR( 0xFF00FF00 ), L"Compute Transforms" );

	NjFloat3	TerrainPosition = NjFloat3::Zero;

	// Snap terrain position to match camera position so it follows around without shitty altitude scrolling
	{
		NjFloat3	CameraPosition = m_Camera.GetCB().Camera2World.GetRow( 3 );
		NjFloat3	CameraAt = m_Camera.GetCB().Camera2World.GetRow( 2 );

		NjFloat3	TerrainCenter = CameraPosition + 0.45f * TERRAIN_SIZE * CameraAt;	// The center will be in front of us always

		float	VertexSnap = TERRAIN_SIZE / TERRAIN_SUBDIVISIONS_COUNT;	// World length between 2 vertices
		int		VertexX = floorf( TerrainCenter.x / VertexSnap );
		int		VertexZ = floorf( TerrainCenter.z / VertexSnap );

		TerrainPosition.x = VertexX * VertexSnap;
		TerrainPosition.z = VertexZ * VertexSnap;
	}

	m_Terrain2World.PRS( TerrainPosition, NjFloat4::QuatFromAngleAxis( 0.0f, NjFloat3::UnitY ), NjFloat3( 0.5f * TERRAIN_SIZE, 1, 0.5f * TERRAIN_SIZE) );

	// Set cloud slab center so it follows the camera around as well...
	{
		NjFloat3	CameraPosition = m_Camera.GetCB().Camera2World.GetRow( 3 );
		NjFloat3	CameraAt = m_Camera.GetCB().Camera2World.GetRow( 2 );

		NjFloat3	CloudCenter = CameraPosition + 0.45f * CLOUD_SIZE * CameraAt;	// The center will be in front of us always

 		m_Position.Set( CloudCenter.x, m_CloudAltitude + 0.5f * m_CloudThickness, CloudCenter.z );
	}

	m_Cloud2World.PRS( m_Position, m_Rotation, m_Scale );

	ComputeShadowTransform();

	m_pCB_Shadow->m.World2TerrainShadow = ComputeTerrainShadowTransform();

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

	m_pRTTransmittanceMap->RemoveFromLastAssignedSlots();

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
	m_Device.RemoveRenderTargets();

	PERF_END_EVENT();


	//////////////////////////////////////////////////////////////////////////
	// 2] Terrain shadow map
#ifdef SHOW_TERRAIN
	if ( m_bShowTerrain )
	{
		PERF_BEGIN_EVENT( D3DCOLOR( 0xFFFF8000 ), L"Compute Terrain Shadow" );

		m_pRTTerrainShadow->RemoveFromLastAssignedSlots();

		USING_MATERIAL_START( *m_pMatTerrainShadow )

			m_Device.ClearDepthStencil( *m_pRTTerrainShadow, 1, 0 );

			m_Device.SetRenderTargets( TERRAIN_SHADOW_MAP_SIZE, TERRAIN_SHADOW_MAP_SIZE, 0, NULL, m_pRTTerrainShadow->GetDepthStencilView() );
	 		m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_ReadWriteLess, m_Device.m_pBS_Disabled );

			m_pCB_Object->m.Local2View = m_Terrain2World;
			m_pCB_Object->m.View2Proj = m_pCB_Shadow->m.World2TerrainShadow;
			m_pCB_Object->m.dUV = m_pRTTerrainShadow->GetdUV();
			m_pCB_Object->UpdateData();

			m_pPrimTerrain->Render( M );

		USING_MATERIAL_END

//#define	INCLUDE_TERRAIN_SHADOWING_IN_TFM	// The resolution is poor anyway, and that adds a dependency on this map for all DrawCalls...

		m_Device.RemoveRenderTargets();
#ifdef INCLUDE_TERRAIN_SHADOWING_IN_TFM
		m_pRTTerrainShadow->SetPS( 6, true );
#endif

		PERF_END_EVENT();

		//////////////////////////////////////////////////////////////////////////
		// 3] Show terrain
		m_pRTTransmittanceMap->SetPS( 5, true );	// Now we need the TFM!

 		PERF_BEGIN_EVENT( D3DCOLOR( 0xFFFFFF00 ), L"Render Terrain" );

		USING_MATERIAL_START( *m_pMatTerrain )

			m_Device.SetRenderTarget( m_RTHDR, &m_Device.DefaultDepthStencil() );
	 		m_Device.SetStates( m_Device.m_pRS_CullBack, m_Device.m_pDS_ReadWriteLess, m_Device.m_pBS_Disabled );

			m_pCB_Object->m.Local2View = m_Terrain2World;
			m_pCB_Object->m.View2Proj = m_Camera.GetCB().World2Proj;
			m_pCB_Object->m.dUV = m_RTHDR.GetdUV();
			m_pCB_Object->UpdateData();

			m_pPrimTerrain->Render( M );

		USING_MATERIAL_END

		PERF_END_EVENT();
	}

#endif

	m_pRTTransmittanceMap->SetPS( 5, true );	// Now we need the TFM!


	//////////////////////////////////////////////////////////////////////////
	// 4] Render the cloud box's front & back
	PERF_BEGIN_EVENT( D3DCOLOR( 0xFF800000 ), L"Render Volume Front&Back" );

	m_Device.ClearRenderTarget( *m_pRTRenderZ, NjFloat4( 0.0f, -1e4f, 0.0f, 0.0f ) );

	USING_MATERIAL_START( *m_pMatDepthWrite )

		m_Device.SetRenderTarget( *m_pRTRenderZ );

		m_pCB_Object->m.Local2View = m_Cloud2World * m_Camera.GetCB().World2Camera;
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


	//////////////////////////////////////////////////////////////////////////
	// 5] Render the actual volume
	PERF_BEGIN_EVENT( D3DCOLOR( 0xFFFF0000 ), L"Render Volume" );

	USING_MATERIAL_START( *m_pMatDisplay )

		m_Device.ClearRenderTarget( *m_pRTRender, NjFloat4( 0.0f, 0.0f, 0.0f, 1.0f ) );

		ID3D11RenderTargetView*	ppViews[] = {
			m_pRTRender->GetTargetView( 0, 0, 1 ),
			m_pRTRender->GetTargetView( 0, 1, 1 )
		};
		m_Device.SetRenderTargets( m_pRTRender->GetWidth(), m_pRTRender->GetHeight(), 2, ppViews );
		m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );

		m_pRTRenderZ->SetPS( 10 );
		m_Device.DefaultDepthStencil().SetPS( 11 );


#ifdef SHOW_TERRAIN
#ifndef INCLUDE_TERRAIN_SHADOWING_IN_TFM
	if ( m_bShowTerrain )
		m_pRTTerrainShadow->SetPS( 6, true );	// We need it now for godrays
#endif
#endif

		m_pCB_Splat->m.dUV = m_pRTRender->GetdUV();
		m_pCB_Splat->m.bSampleTerrainShadow = m_bShowTerrain ? 1 : 0;
		m_pCB_Splat->UpdateData();

		m_ScreenQuad.Render( M );

	USING_MATERIAL_END

	PERF_END_EVENT();


	//////////////////////////////////////////////////////////////////////////
	// 6] Combine with screen
	PERF_BEGIN_EVENT( D3DCOLOR( 0xFF0000FF ), L"Combine" );

	m_Device.SetRenderTarget( m_Device.DefaultRenderTarget(), NULL );
	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );

	USING_MATERIAL_START( *m_pMatCombine )

		m_pCB_Splat->m.dUV = m_Device.DefaultRenderTarget().GetdUV();
		m_pCB_Splat->UpdateData();

// DEBUG
m_pRTRender->SetPS( 10 );	// Cloud rendering, with scattering and extinction
m_RTHDR.SetPS( 11 );		// Background scene
m_Device.DefaultDepthStencil().SetPS( 12 );

//m_pRTTransmittanceZ->SetPS( 11 );
//m_pRTRenderZ->SetPS( 11 );
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

	CornerWorld = NjFloat4( CornerLocal, 1 ) * m_Cloud2World;

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

		CornerWorld = NjFloat4( CornerLocal, 1 ) * m_Cloud2World;

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

	CornerWorld = NjFloat4( CornerLocal, 1 ) * m_Cloud2World;

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

	NjFloat3	LightDirection = m_pCB_Atmosphere->m.LightDirection;

	float		TanFovH = m_Camera.GetCB().Params.x;
	float		TanFovV = m_Camera.GetCB().Params.y;
	NjFloat4x4&	Camera2World = m_Camera.GetCB().Camera2World;
	NjFloat3	CameraPositionKm = Camera2World.GetRow( 3 );

//###	static const float	SHADOW_FAR_CLIP_DISTANCE = 250.0f;
	static const float	SHADOW_FAR_CLIP_DISTANCE = 70.0f;
	static const float	SHADOW_SCALE = 1.1f;

	//////////////////////////////////////////////////////////////////////////
	// Compute shadow plane tangent space
	NjFloat3	ClippedSunDirection = -LightDirection;
// 	if ( ClippedSunDirection.y > 0.0f )
// 		ClippedSunDirection = -ClippedSunDirection;	// We always require a vector facing down

	const float	MIN_THRESHOLD = 1e-2f;
	if ( ClippedSunDirection.y >= 0.0 && ClippedSunDirection.y < MIN_THRESHOLD )
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
	m_ShadowPlaneCenterKm = NjFloat4( 0, LightDirection.y > 0 ? 1.0f : -1.0f, 0, 1 ) * m_Cloud2World;

	float	ZSize = m_pCB_Volume->m._CloudAltitudeThickness.y / abs(ClippedSunDirection.y);	// Since we're blocking the XY axes on the plane, Z changes with the light's vertical component
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

	if ( LightDirection.y > 0.0f )
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
		ComputeFrustumIntersection( pCameraFrustumKm, m_pCB_Volume->m._CloudAltitudeThickness.x, QuadMin, QuadMax );
		ComputeFrustumIntersection( pCameraFrustumKm, m_pCB_Volume->m._CloudAltitudeThickness.x + m_pCB_Volume->m._CloudAltitudeThickness.y, QuadMin, QuadMax );
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
		NjFloat3	CornerWorld = NjFloat4( CornerLocal, 1 ) * m_Cloud2World;

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
// Computes the projection matrix for the terrain shadow map
NjFloat4x4	EffectVolumetric::ComputeTerrainShadowTransform()
{
	const float	FRUSTUM_FAR_CLIP = 60.0f;

	NjFloat3	LightDirection = m_pCB_Atmosphere->m.LightDirection;

	NjFloat3	Z = -LightDirection;
	if ( abs( Z.y ) > 1.0f - 1e-3f )
		Z = NjFloat3( 1e-2f, Z.y > 0.0f ? 1.0f : -1.0f, 0 ).Normalize();
	NjFloat3	X = (NjFloat3::UnitY ^ Z).Normalize();
	NjFloat3	Y = Z ^ X;

	NjFloat4x4	Light2World;
	Light2World.SetRow( 0, X, 0 );
	Light2World.SetRow( 1, Y, 0 );
	Light2World.SetRow( 2, Z, 0 );
	Light2World.SetRow( 3, m_Terrain2World.GetRow( 3 ) );

	NjFloat4x4	World2Light = Light2World.Inverse();

	// Build frustum points
	float		TanFovH = m_Camera.GetCB().Params.x;
	float		TanFovV = m_Camera.GetCB().Params.y;
	NjFloat4x4&	Camera2World = m_Camera.GetCB().Camera2World;

	NjFloat3	pFrustumWorld[5] = {
		NjFloat3::Zero,
		FRUSTUM_FAR_CLIP * NjFloat3( -TanFovH, +TanFovV, 1 ),
		FRUSTUM_FAR_CLIP * NjFloat3( -TanFovH, -TanFovV, 1 ),
		FRUSTUM_FAR_CLIP * NjFloat3( +TanFovH, -TanFovV, 1 ),
		FRUSTUM_FAR_CLIP * NjFloat3( +TanFovH, +TanFovV, 1 ),
	};
	for ( int i=0; i < 5; i++ )
		pFrustumWorld[i] = NjFloat4( pFrustumWorld[i], 1 ) * Camera2World;

	// Transform frustum and terrain into light space
	NjFloat3	FrustumMin( 1e6f, 1e6f, 1e6f );
	NjFloat3	FrustumMax( -1e6f, -1e6f, -1e6f );
	for ( int i=0; i < 5; i++ )
	{
		NjFloat3	FrustumLight = NjFloat4( pFrustumWorld[i], 1 ) * World2Light;
		FrustumMin = FrustumMin.Min( FrustumLight );
		FrustumMax = FrustumMax.Max( FrustumLight );
	}
	NjFloat3	TerrainMin( 1e6f, 1e6f, 1e6f );
	NjFloat3	TerrainMax( -1e6f, -1e6f, -1e6f );
	for ( int i=0; i < 8; i++ )
	{
		float	X = 2.0f * (i&1) - 1.0f;
		float	Y = float( (i>>1)&1 );
		float	Z = 2.0f * ((i>>2)&1) - 1.0f;

		NjFloat4	TerrainWorld = NjFloat4( X, 0, Z, 1 ) * m_Terrain2World;
					TerrainWorld.y = m_pCB_Object->m.TerrainHeight * Y;

		NjFloat3	TerrainLight = TerrainWorld * World2Light;
		TerrainMin = TerrainMin.Min( TerrainLight );
		TerrainMax = TerrainMax.Max( TerrainLight );
	}

	// Clip frustum with terrain as it's useless to render parts that aren't even covered by the terrain...
	FrustumMin = FrustumMin.Max( TerrainMin );
	FrustumMax = FrustumMax.Min( TerrainMax );

	NjFloat3	Center = 0.5f * (FrustumMin + FrustumMax);
	NjFloat3	Scale = 0.5f * (FrustumMax - FrustumMin);
	NjFloat4x4	Light2Proj;
	Light2Proj.SetRow( 0, NjFloat4( 1.0f / Scale.x, 0, 0, 0 ) );
	Light2Proj.SetRow( 1, NjFloat4( 0, 1.0f / Scale.y, 0, 0 ) );
	Light2Proj.SetRow( 2, NjFloat4( 0, 0, 0.5f / Scale.z, 0 ) );
	Light2Proj.SetRow( 3, NjFloat4( -Center.x / Scale.x, -Center.y / Scale.y, -0.5f * FrustumMin.z / Scale.z, 1 ) );

	NjFloat4x4	World2Proj = World2Light * Light2Proj;

// CHECK
NjFloat4	FrustumShadow;
for ( int i=0; i < 5; i++ )
{
	FrustumShadow = NjFloat4( pFrustumWorld[i], 1 ) * World2Proj;
}
// CHECK

	return World2Proj;
}


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
//#define RES_3D_U		(RES_3D_COS_THETA_SUN * RES_3D_COS_GAMMA)	// Full texture will be 256*128*32

#define FILENAME_IRRADIANCE		"./TexIrradiance_64x16.bin"
#define FILENAME_TRANSMITTANCE	"./TexTransmittance_256x64.bin"
#define FILENAME_SCATTERING		"./TexScattering_256x128x32.bin"


struct	CBPreCompute
{
	NjFloat4	dUVW;
	bool		bFirstPass;
	float		AverageGroundReflectance;
	NjFloat2	__PAD1;
};

void	EffectVolumetric::PreComputeSkyTables()
{
#ifdef BUILD_SKY_SCATTERING

	Texture2D*	pRTDeltaIrradiance = new Texture2D( m_Device, IRRADIANCE_W, IRRADIANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );			// deltaE (temp)
	Texture3D*	pRTDeltaScatteringRayleigh = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );		// deltaSR (temp)
	Texture3D*	pRTDeltaScatteringMie = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );			// deltaSM (temp)
	Texture3D*	pRTDeltaScattering = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );				// deltaJ (temp)

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
	CB.m.AverageGroundReflectance = 0.1f;

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
	m_ppRTTransmittance[0]->SetVS( 7, true );
	m_ppRTTransmittance[0]->SetPS( 7, true );

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
	m_Device.ClearRenderTarget( *m_ppRTIrradiance[0], NjFloat4::Zero );

	//////////////////////////////////////////////////////////////////////////
	// Computes single scattering texture deltaS (line 3 in algorithm 4.1)
	// Rayleigh and Mie separated in deltaSR + deltaSM
	USING_MATERIAL_START( *pMatComputeInScattering_Single )

		ID3D11RenderTargetView*	ppTargets[] = { pRTDeltaScatteringRayleigh->GetTargetView( 0, 0, 0 ), pRTDeltaScatteringMie->GetTargetView( 0, 0, 0 ) };
		m_Device.SetRenderTargets( RES_3D_U, RES_3D_COS_THETA_VIEW, 2, ppTargets );

		CB.m.dUVW = pRTDeltaScatteringRayleigh->GetdUVW();
		CB.UpdateData();

		m_ScreenQuad.RenderInstanced( M, RES_3D_ALTITUDE );

	USING_MATERIAL_END

	// Assign to slot 14 & 15
	m_Device.RemoveRenderTargets();
	pRTDeltaScatteringRayleigh->SetPS( 14 );
	pRTDeltaScatteringMie->SetPS( 15 );

	// ==================================================
	// Merges DeltaScatteringRayleigh & Mie into initial inscatter texture S (line 5 in algorithm 4.1)
	USING_MATERIAL_START( *pMatMergeInitialScattering )

		m_Device.SetRenderTarget( *m_ppRTInScattering[0] );

		CB.m.dUVW = m_ppRTInScattering[0]->GetdUVW();
		CB.UpdateData();

		m_ScreenQuad.RenderInstanced( M, RES_3D_ALTITUDE );

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

			m_ScreenQuad.RenderInstanced( M, RES_3D_ALTITUDE );

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

			m_ScreenQuad.RenderInstanced( M, RES_3D_ALTITUDE );

		USING_MATERIAL_END

		// Assign to slot 14 again
		m_Device.RemoveRenderTargets();
		pRTDeltaScatteringRayleigh->SetPS( 14 );

		// ==================================================
		// Adds deltaE into irradiance texture E (line 10 in algorithm 4.1)
		m_Device.SetStates( NULL, NULL, m_Device.m_pBS_Additive );

		USING_MATERIAL_START( *pMatAccumulateIrradiance )

			m_Device.SetRenderTarget( *m_ppRTIrradiance[0] );

			CB.m.dUVW = NjFloat4( m_ppRTIrradiance[0]->GetdUV(), 0 );
			CB.UpdateData();

			m_ScreenQuad.Render( M );

		USING_MATERIAL_END

		// ==================================================
 		// Adds deltaS into inscatter texture S (line 11 in algorithm 4.1)
		USING_MATERIAL_START( *pMatAccumulateInScattering )

			m_Device.SetRenderTarget( *m_ppRTInScattering[0] );

			CB.m.dUVW = m_ppRTInScattering[0]->GetdUVW();
			CB.UpdateData();

			m_ScreenQuad.RenderInstanced( M, RES_3D_ALTITUDE );

		USING_MATERIAL_END

		m_Device.SetStates( NULL, NULL, m_Device.m_pBS_Disabled );
	}

	// Assign final textures to slots 8 & 9
	m_Device.RemoveRenderTargets();
	m_ppRTInScattering[0]->SetVS( 8, true );
	m_ppRTInScattering[0]->SetPS( 8, true );
	m_ppRTIrradiance[0]->SetVS( 9, true );
	m_ppRTIrradiance[0]->SetPS( 9, true );

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
	Texture3D*	pStagingScattering = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true );
	Texture2D*	pStagingTransmittance = new Texture2D( m_Device, TRANSMITTANCE_W, TRANSMITTANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true );
	Texture2D*	pStagingIrradiance = new Texture2D( m_Device, IRRADIANCE_W, IRRADIANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true );

	pStagingScattering->CopyFrom( *m_ppRTInScattering[0] );
	pStagingTransmittance->CopyFrom( *m_ppRTTransmittance[0] );
	pStagingIrradiance->CopyFrom( *m_ppRTIrradiance[0] );

	pStagingIrradiance->Save( FILENAME_IRRADIANCE );
	pStagingTransmittance->Save( FILENAME_TRANSMITTANCE );
	pStagingScattering->Save( FILENAME_SCATTERING );

	delete pStagingIrradiance;
	delete pStagingTransmittance;
	delete pStagingScattering;

#else
	// Load tables
	Texture2D*	pStagingTransmittance = new Texture2D( m_Device, TRANSMITTANCE_W, TRANSMITTANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true, true );
	Texture3D*	pStagingScattering = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true, true );
	Texture2D*	pStagingIrradiance = new Texture2D( m_Device, IRRADIANCE_W, IRRADIANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true, true );

// This includes a dependency on disk files... Useless if we recompute them!
// #if 0
// 	BuildTransmittanceTable( TRANSMITTANCE_W, TRANSMITTANCE_H, *pStagingTransmittance );
// #else
// 	pStagingTransmittance->Load( FILENAME_TRANSMITTANCE );
// #endif
// 
// 	pStagingIrradiance->Load( FILENAME_IRRADIANCE );
// 	pStagingScattering->Load( FILENAME_SCATTERING );

	m_ppRTTransmittance[0]->CopyFrom( *pStagingTransmittance );
	m_ppRTInScattering[0]->CopyFrom( *pStagingScattering );
	m_ppRTIrradiance[0]->CopyFrom( *pStagingIrradiance );

	delete pStagingIrradiance;
	delete pStagingTransmittance;
	delete pStagingScattering;

	m_ppRTTransmittance[0]->SetVS( 7, true );
	m_ppRTTransmittance[0]->SetPS( 7, true );
	m_ppRTInScattering[0]->SetVS( 8, true );
	m_ppRTInScattering[0]->SetPS( 8, true );
	m_ppRTIrradiance[0]->SetVS( 9, true );
	m_ppRTIrradiance[0]->SetPS( 9, true );

#endif
}

void	EffectVolumetric::FreeSkyTables()
{
	delete m_ppRTInScattering[2];
	delete m_ppRTInScattering[1];
	delete m_ppRTInScattering[0];
	delete m_ppRTIrradiance[2];
	delete m_ppRTIrradiance[1];
	delete m_ppRTIrradiance[0];
	delete m_ppRTTransmittance[1];
	delete m_ppRTTransmittance[0];
}


//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//
namespace
{
	Texture2D*			m_pRTDeltaIrradiance = NULL;				// deltaE (temp)
	Texture3D*			m_pRTDeltaScatteringRayleigh = NULL;		// deltaSR (temp)
	Texture3D*			m_pRTDeltaScatteringMie = NULL;				// deltaSM (temp)
	Texture3D*			m_pRTDeltaScattering = NULL;				// deltaJ (temp)

	ComputeShader*		m_pCSComputeTransmittance = NULL;
	ComputeShader*		m_pCSComputeIrradiance_Single = NULL;
	ComputeShader*		m_pCSComputeInScattering_Single = NULL;
	ComputeShader*		m_pCSComputeInScattering_Delta = NULL;
	ComputeShader*		m_pCSComputeIrradiance_Delta = NULL;
	ComputeShader*		m_pCSComputeInScattering_Multiple = NULL;
	ComputeShader*		m_pCSMergeInitialScattering = NULL;
	ComputeShader*		m_pCSAccumulateIrradiance = NULL;
	ComputeShader*		m_pCSAccumulateInScattering = NULL;

	bool				m_bSkyTableDirty = false;
//	bool				m_bSkyTableUpdating = false;

	// Update Stages Description
	static const int	MAX_SCATTERING_ORDER = 4;						// Render up to order 4, later order events don't matter that much

	static const int	THREADS_COUNT_X = 16;							// !!IMPORTANT ==> Must correspond to what's written in the shader!!
	static const int	THREADS_COUNT_Y = 16;
	static const int	THREADS_COUNT_Z = 4;							// 4 as the product of all thread counts cannot exceed 1024!

	enum STAGE_INDEX
	{
		COMPUTING_STOPPED = -1,

		COMPUTING_TRANSMITTANCE = 0,
		COMPUTING_IRRADIANCE_SINGLE = 1,
		COMPUTING_SCATTERING_SINGLE = 2,

		// Multi-Pass
		COMPUTING_SCATTERING_DELTA = 3,
		COMPUTING_IRRADIANCE_DELTA = 4,
		COMPUTING_SCATTERING_MULTIPLE = 5,

		STAGES_COUNT,
	};

	STAGE_INDEX			m_CurrentStage = COMPUTING_STOPPED;
	bool				m_bStageStarting = true;
	int					m_ScatteringOrder = 2;

	U32					m_pStageTargetSizes[3*STAGES_COUNT] = {
		TRANSMITTANCE_W,	TRANSMITTANCE_H,		1,					// #1 Transmittance table
		IRRADIANCE_W,		IRRADIANCE_H,			1,					// #2 Irradiance table (single scattering)
		RES_3D_U,			RES_3D_COS_THETA_VIEW,	RES_3D_ALTITUDE,	// #3 Scattering table (single scattering)

		// Multi-pass
		RES_3D_U,			RES_3D_COS_THETA_VIEW,	RES_3D_ALTITUDE,	// #4 Delta-Scattering table (used to compute actual irradiance & multiple-scattering at current order)
		IRRADIANCE_W,		IRRADIANCE_H,			1,					// #5 Irradiance table (multiple scattering)
		RES_3D_U,			RES_3D_COS_THETA_VIEW,	RES_3D_ALTITUDE,	// #6 Multiple Scattering table
	};

	U32					m_pStageGroupsCountPerFrame[3*STAGES_COUNT] = {
		1,	1,	1,	// #1 Transmittance table
		1,	1,	1,	// #2 Irradiance table (single scattering)
//		4,	4,	1,	// #3 Scattering table (single scattering)
		16,	8,	1,	// #3 Scattering table (single scattering)

		// Multi-pass
//		4,	4,	1,	// #4 Delta-Scattering table (used to compute actual irradiance & multiple-scattering at current order)
		16,	8,	1,	// #4 Delta-Scattering table (used to compute actual irradiance & multiple-scattering at current order)
		1,	1,	1,	// #5 Irradiance table (multiple scattering)
//		4,	4,	1,	// #6 Multiple Scattering table
		16,	8,	1,	// #6 Multiple Scattering table
	};

	U32					m_pStagePassesCount[3*STAGES_COUNT];	// Filled automatically in InitUpdateSkyTables(), derived from the 2 tables above

#ifdef _DEBUG
#define ENABLE_PROFILING
#endif

#ifdef ENABLE_PROFILING
	// Profiling
	double				m_pStageTimingCurrent[STAGES_COUNT];
	double				m_pStageTimingMin[STAGES_COUNT];
	double				m_pStageTimingMax[STAGES_COUNT];
	int					m_pStageTimingCount[STAGES_COUNT];
	double				m_pStageTimingTotal[STAGES_COUNT];
	double				m_pStageTimingAvg[STAGES_COUNT];

	void	UpdateStageProfiling( int _StageIndex )
	{
		m_pStageTimingMin[_StageIndex] = MIN( m_pStageTimingMin[_StageIndex], m_pStageTimingCurrent[_StageIndex] );
		m_pStageTimingMax[_StageIndex] = MAX( m_pStageTimingMax[_StageIndex], m_pStageTimingCurrent[_StageIndex] );
		m_pStageTimingTotal[_StageIndex] += m_pStageTimingCurrent[_StageIndex];
		m_pStageTimingCount[_StageIndex]++;
	}
	void	FinalizeStageProfiling( int _StageIndex )
	{
		m_pStageTimingAvg[_StageIndex] = m_pStageTimingTotal[_StageIndex] / MAX( 1, m_pStageTimingCount[_StageIndex] );
	}
#endif
}

void	EffectVolumetric::InitUpdateSkyTables()
{
	m_pRTDeltaIrradiance = new Texture2D( m_Device, IRRADIANCE_W, IRRADIANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, false, false, UAV );							// deltaE (temp)
	m_pRTDeltaScatteringRayleigh = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, false, false, UAV );	// deltaSR (temp)
	m_pRTDeltaScatteringMie = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, false, false, UAV );		// deltaSM (temp)
	m_pRTDeltaScattering = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, false, false, UAV );			// deltaJ (temp)

	m_pCB_PreComputeSky = new CB<CBPreComputeCS>( m_Device, 10 );
	// Clear pass indices (needs to be done only once as each stage will reset it to 0 at its end)
	m_pCB_PreComputeSky->m._PassIndexX = 0;
	m_pCB_PreComputeSky->m._PassIndexY = 0;
	m_pCB_PreComputeSky->m._PassIndexZ = 0;
	m_pCB_PreComputeSky->m._AverageGroundReflectance = 0.1f;	// Default value given in the paper

	// Build passes count for each stage
	for ( int StageIndex=0; StageIndex < STAGES_COUNT; StageIndex++ )
	{
		{
			int	GroupsX = m_pStageGroupsCountPerFrame[3*StageIndex+0];
			int	CoveredSizeX = GroupsX * THREADS_COUNT_X;
			int	SizeX = m_pStageTargetSizes[3*StageIndex+0];
			int	PassesCountX = SizeX / CoveredSizeX;
			ASSERT( (m_pStageTargetSizes[3*StageIndex+0] % CoveredSizeX) == 0, "GroupsCountPerFrameX * THREADS_COUNT_X yields a non-integer amount of passes!" );
			m_pStagePassesCount[3*StageIndex+0] = PassesCountX;
		}

		{
			int	GroupsY = m_pStageGroupsCountPerFrame[3*StageIndex+1];
			int	CoveredSizeY = GroupsY * THREADS_COUNT_Y;
			int	SizeY = m_pStageTargetSizes[3*StageIndex+1];
			int	PassesCountY = SizeY / CoveredSizeY;
			ASSERT( (SizeY % CoveredSizeY) == 0, "GroupsCountPerFrameY * THREADS_COUNT_Y yields a non-integer amount of passes!" );
			m_pStagePassesCount[3*StageIndex+1] = PassesCountY;
		}
		
		if ( m_pStageTargetSizes[3*StageIndex+2] > 1 )
		{	// 3D Target
			int	GroupsZ = m_pStageGroupsCountPerFrame[3*StageIndex+2];
			int	CoveredSizeZ = GroupsZ * THREADS_COUNT_Z;
			int	SizeZ = m_pStageTargetSizes[3*StageIndex+2];
			int	PassesCountZ = SizeZ / CoveredSizeZ;
			ASSERT( (SizeZ % CoveredSizeZ) == 0, "GroupsCountPerFrameZ * THREADS_COUNT_Z yields a non-integer amount of passes!" );
			m_pStagePassesCount[3*StageIndex+2] = PassesCountZ;
		}
		else
		{	// 2D Target
			m_pStagePassesCount[3*StageIndex+2] = 1;
		}
	}

#if 0
	// Build heavy compute shaders
	CHECK_MATERIAL( m_pCSComputeTransmittance = CreateComputeShader( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE_CS, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",			"PreComputeTransmittance" ), 10 );
	CHECK_MATERIAL( m_pCSComputeIrradiance_Single = CreateComputeShader( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE_CS, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",		"PreComputeIrradiance_Single" ), 11 );		// irradiance1
	CHECK_MATERIAL( m_pCSComputeIrradiance_Delta = CreateComputeShader( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE_CS, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",		"PreComputeIrradiance_Delta" ), 12 );		// irradianceN
	CHECK_MATERIAL( m_pCSComputeInScattering_Single = CreateComputeShader( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE_CS, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",		"PreComputeInScattering_Single" ), 13 );	// inscatter1
	CHECK_MATERIAL( m_pCSComputeInScattering_Delta = CreateComputeShader( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE_CS, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",		"PreComputeInScattering_Delta" ), 14 );		// inscatterS
	CHECK_MATERIAL( m_pCSComputeInScattering_Multiple = CreateComputeShader( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE_CS, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",	"PreComputeInScattering_Multiple" ), 15 );	// inscatterN
	CHECK_MATERIAL( m_pCSMergeInitialScattering = CreateComputeShader( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE_CS, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",			"MergeInitialScattering" ), 16 );			// copyInscatter1
	CHECK_MATERIAL( m_pCSAccumulateIrradiance = CreateComputeShader( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE_CS, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",			"AccumulateIrradiance" ), 17 );				// copyIrradiance
	CHECK_MATERIAL( m_pCSAccumulateInScattering = CreateComputeShader( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE_CS, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",			"AccumulateInScattering" ), 18 );			// copyInscatterN
#else
	// Reload from binary blobs
	CHECK_MATERIAL( m_pCSComputeTransmittance = ComputeShader::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",			"PreComputeTransmittance" ), 10 );
	CHECK_MATERIAL( m_pCSComputeIrradiance_Single = ComputeShader::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",		"PreComputeIrradiance_Single" ), 11 );
	CHECK_MATERIAL( m_pCSComputeIrradiance_Delta = ComputeShader::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",		"PreComputeIrradiance_Delta" ), 12 );
	CHECK_MATERIAL( m_pCSComputeInScattering_Single = ComputeShader::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",	"PreComputeInScattering_Single" ), 13 );
	CHECK_MATERIAL( m_pCSComputeInScattering_Delta = ComputeShader::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",	"PreComputeInScattering_Delta" ), 14 );
	CHECK_MATERIAL( m_pCSComputeInScattering_Multiple = ComputeShader::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",	"PreComputeInScattering_Multiple" ), 15 );
	CHECK_MATERIAL( m_pCSMergeInitialScattering = ComputeShader::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",		"MergeInitialScattering" ), 16 );
	CHECK_MATERIAL( m_pCSAccumulateIrradiance = ComputeShader::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",			"AccumulateIrradiance" ), 17 );
	CHECK_MATERIAL( m_pCSAccumulateInScattering = ComputeShader::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",		"AccumulateInScattering" ), 18 );

//	CHECK_MATERIAL( m_pCSAccumulateInScattering = CreateComputeShader( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE_CS, "./Resources/Shaders/VolumetricPreComputeAtmosphereCS.hlsl",			"AccumulateInScattering" ), 18 );			// copyInscatterN
#endif
}

void	EffectVolumetric::ExitUpdateSkyTables()
{
	// Release materials & temporary RTs
	delete m_pCSAccumulateInScattering;
	delete m_pCSAccumulateIrradiance;
	delete m_pCSMergeInitialScattering;
	delete m_pCSComputeInScattering_Multiple;
	delete m_pCSComputeInScattering_Delta;
	delete m_pCSComputeInScattering_Single;
	delete m_pCSComputeIrradiance_Delta;
	delete m_pCSComputeIrradiance_Single;
	delete m_pCSComputeTransmittance;

	delete m_pCB_PreComputeSky;

	delete m_pRTDeltaIrradiance;
	delete m_pRTDeltaScatteringRayleigh;
	delete m_pRTDeltaScatteringMie;
	delete m_pRTDeltaScattering;
}

void	EffectVolumetric::TriggerSkyTablesUpdate()
{
	m_bSkyTableDirty = true;	// Should start the update process as soon as updating is done...
}

void	EffectVolumetric::InitStage( int _StageIndex )
{
	// Setup groups count
	m_pCB_PreComputeSky->m.SetGroupsCount(
		m_pStageGroupsCountPerFrame[3*_StageIndex+0],
		m_pStageGroupsCountPerFrame[3*_StageIndex+1],
		m_pStageGroupsCountPerFrame[3*_StageIndex+2]
	);

#ifdef ENABLE_PROFILING
	m_pStageTimingMin[_StageIndex] = MAX_FLOAT;
	m_pStageTimingMax[_StageIndex] = 0.0;
	m_pStageTimingCount[_StageIndex] = 0;
	m_pStageTimingTotal[_StageIndex] = 0.0;
	m_pStageTimingAvg[_StageIndex] = 0.0;
#endif
}

void	EffectVolumetric::InitSinglePassStage( int _TargetSizeX, int _TargetSizeY, int _TargetSizeZ, int _GroupsCount[3] )
{
	m_pCB_PreComputeSky->m.SetTargetSize( _TargetSizeX, _TargetSizeY, _TargetSizeZ );

	_GroupsCount[0] = _TargetSizeX >> 4;
	_GroupsCount[1] = _TargetSizeY >> 4;
	_GroupsCount[2] = _TargetSizeZ > 1 ? _TargetSizeZ >> 2 : 1;
	m_pCB_PreComputeSky->m.SetGroupsCount( _GroupsCount[0], _GroupsCount[1], _GroupsCount[2] );
}

bool	EffectVolumetric::IncreaseStagePass( int _StageIndex )
{
#ifdef ENABLE_PROFILING
	UpdateStageProfiling( _StageIndex );
#endif

	// Increase pass indices
	U32	PassesCountX = m_pStagePassesCount[3*_StageIndex+0];
	U32	PassesCountY = m_pStagePassesCount[3*_StageIndex+1];
	U32	PassesCountZ = m_pStagePassesCount[3*_StageIndex+2];

	m_pCB_PreComputeSky->m._PassIndexX++;
	if ( m_pCB_PreComputeSky->m._PassIndexX >= PassesCountX )
	{	// X line is over, wrap X and increase Y
		m_pCB_PreComputeSky->m._PassIndexX = 0;
		m_pCB_PreComputeSky->m._PassIndexY++;
		if ( m_pCB_PreComputeSky->m._PassIndexY >= PassesCountY )
		{	// Y slice is over, wrap Y and increase Z
			m_pCB_PreComputeSky->m._PassIndexY = 0;
			m_pCB_PreComputeSky->m._PassIndexZ++;
			if ( m_pCB_PreComputeSky->m._PassIndexZ >= PassesCountZ )
			{	// Z box is over, wrap Z and return completed state
				m_pCB_PreComputeSky->m._PassIndexZ = 0;

#ifdef ENABLE_PROFILING
				FinalizeStageProfiling( _StageIndex );
#endif

				return true;	// We're done!
			}
		}
	}

	return false;
}

//////////////////////////////////////////////////////////////////////////
// This very important routine updates the sky table using time slicing
// There are  stages for the table computation:
//	1] Compute transmittance table
//	2] Compute irradiance table (accounting only fo single scattering)
//	3] Compute single-scattering table
//	=> Then we loop 3 times (for 3 additional orders of scattering)
//		4] Compute delta-scattering table (using previous scattering order)
//		5] Compute delta-irradiance table (using previous scattering order)
//		6] Compute multiple scattering table (using previous table and delta-scattering & delta-irradiance table)
//
// Each stage is computed using a Compute Shader that processes a certain amount of (2D or 3D) blocks depending on what is allocated each frame
// For example, the scattering integration computation which is quite greedy will be allocated less blocks than the transmittance computation
//	that could almost be performed entirely each frame.
// Each block computes 16x16(x16) texels of our tables.
//
// So this functions is merely a state machine keeping track of what has been computed and what remains to be computed until the tables have all been updated.
//
//
void	EffectVolumetric::UpdateSkyTables()
{
//return;


	if ( !m_bSkyTableDirty && m_CurrentStage == COMPUTING_STOPPED )
		return;

	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////
	// STARTING POINT
	if ( m_CurrentStage == COMPUTING_STOPPED )
	{	// Initiate update process
		m_bSkyTableDirty = false;	// Clear immediately so we can still trigger a new update while updating... This new update will only start once this update is complete.
		m_CurrentStage = COMPUTING_TRANSMITTANCE;
		m_ScatteringOrder = 2;	// We start the loop at order 2 up to MAX_SCATTERING_ORDER
		m_pCB_PreComputeSky->m._bFirstPass = true;
		m_bStageStarting = true;
	}
	// STARTING POINT
	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////

	int	CurrentStageIndex = int(m_CurrentStage);

	switch ( m_CurrentStage )
	{
	//////////////////////////////////////////////////////////////////////////
	// Computes transmittance texture T (line 1 in algorithm 4.1)
	case COMPUTING_TRANSMITTANCE:
		{
			if ( m_bStageStarting )
			{	// First step into that stage...
				InitStage( CurrentStageIndex );
				m_pCB_PreComputeSky->m.SetTargetSize( m_ppRTTransmittance[0]->GetWidth(), m_ppRTTransmittance[0]->GetHeight() );
				m_bStageStarting = false;
			}

			USING_COMPUTESHADER_START( *m_pCSComputeTransmittance )
	
#ifdef ENABLE_PROFILING
				TimeProfile	Profile( m_pStageTimingCurrent[CurrentStageIndex] );
#endif

				m_ppRTTransmittance[1]->SetCSUAV( 0 );

				m_pCB_PreComputeSky->UpdateData();

				M.Dispatch( m_pCB_PreComputeSky->m._GroupsCountX, m_pCB_PreComputeSky->m._GroupsCountY, m_pCB_PreComputeSky->m._GroupsCountZ );

			USING_COMPUTE_SHADER_END

			if ( IncreaseStagePass( CurrentStageIndex ) )
			{	// Stage is over!
				m_CurrentStage = COMPUTING_IRRADIANCE_SINGLE;
				m_bStageStarting = true;

				// Assign to slot 7
				m_ppRTTransmittance[1]->RemoveFromLastAssignedSlotUAV();
				m_ppRTTransmittance[1]->Set( 7, true );

				// This is our new default texture
				Texture2D*	pTemp = m_ppRTTransmittance[0];
				m_ppRTTransmittance[0] = m_ppRTTransmittance[1];
				m_ppRTTransmittance[1] = pTemp;
			}
		}
		break;

	//////////////////////////////////////////////////////////////////////////
	// Computes irradiance texture deltaE (line 2 in algorithm 4.1)
	case COMPUTING_IRRADIANCE_SINGLE:
		{
			if ( m_bStageStarting )
			{	// First step into that stage...
				InitStage( CurrentStageIndex );
				m_pCB_PreComputeSky->m.SetTargetSize( m_pRTDeltaIrradiance->GetWidth(), m_pRTDeltaIrradiance->GetHeight() );
				m_bStageStarting = false;
			}

			USING_COMPUTESHADER_START( *m_pCSComputeIrradiance_Single )

#ifdef ENABLE_PROFILING
				TimeProfile	Profile( m_pStageTimingCurrent[CurrentStageIndex] );
#endif

				m_pRTDeltaIrradiance->SetCSUAV( 0 );

				m_pCB_PreComputeSky->UpdateData();

				M.Dispatch( m_pCB_PreComputeSky->m._GroupsCountX, m_pCB_PreComputeSky->m._GroupsCountY, m_pCB_PreComputeSky->m._GroupsCountZ );

			USING_COMPUTE_SHADER_END

			if ( IncreaseStagePass( CurrentStageIndex ) )
			{	// Stage is over!
				m_CurrentStage = COMPUTING_SCATTERING_SINGLE;
				m_bStageStarting = true;

				// Will be assigned to slot 13 next stage
				m_pRTDeltaIrradiance->RemoveFromLastAssignedSlotUAV();

				// ==================================================
 				// Clear irradiance texture E (line 4 in algorithm 4.1)
				m_Device.ClearRenderTarget( *m_ppRTIrradiance[1], NjFloat4::Zero );
			}
		}
		break;

	//////////////////////////////////////////////////////////////////////////
	// Computes single scattering texture deltaS (line 3 in algorithm 4.1)
	// Rayleigh and Mie separated in deltaSR + deltaSM
	case COMPUTING_SCATTERING_SINGLE:
		{
			if ( m_bStageStarting )
			{	// First step into that stage...
				InitStage( CurrentStageIndex );
				m_pCB_PreComputeSky->m.SetTargetSize( m_pRTDeltaScatteringRayleigh->GetWidth(), m_pRTDeltaScatteringRayleigh->GetHeight(), m_pRTDeltaScatteringRayleigh->GetDepth() );
				m_bStageStarting = false;
			}

			USING_COMPUTESHADER_START( *m_pCSComputeInScattering_Single )

#ifdef ENABLE_PROFILING
				TimeProfile	Profile( m_pStageTimingCurrent[CurrentStageIndex] );
#endif

				m_pRTDeltaScatteringRayleigh->SetCSUAV( 1 );
				m_pRTDeltaScatteringMie->SetCSUAV( 2 );

				m_pRTDeltaIrradiance->SetCS( 10 );	// Input from last stage

				m_pCB_PreComputeSky->UpdateData();

				M.Dispatch( m_pCB_PreComputeSky->m._GroupsCountX, m_pCB_PreComputeSky->m._GroupsCountY, m_pCB_PreComputeSky->m._GroupsCountZ );

			USING_COMPUTE_SHADER_END

			if ( IncreaseStagePass( CurrentStageIndex ) )
			{	// Stage is over!
				m_CurrentStage = COMPUTING_SCATTERING_DELTA;
				m_bStageStarting = true;

				// Will be assigned to slot 11 & 12 next stage
				m_pRTDeltaScatteringRayleigh->RemoveFromLastAssignedSlotUAV();
				m_pRTDeltaScatteringMie->RemoveFromLastAssignedSlotUAV();
				m_pRTDeltaIrradiance->RemoveFromLastAssignedSlots();

				// ==================================================
				// Merges DeltaScatteringRayleigh & Mie into initial inscatter texture S (line 5 in algorithm 4.1)
				{
					int		GroupsCount[3];
					InitSinglePassStage( RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, GroupsCount );

					USING_COMPUTESHADER_START( *m_pCSMergeInitialScattering )

						m_ppRTInScattering[1]->SetCSUAV( 1 );

						m_pRTDeltaScatteringRayleigh->SetCS( 11 );
						m_pRTDeltaScatteringMie->SetCS( 12 );

						m_pCB_PreComputeSky->UpdateData();

						M.Dispatch( GroupsCount[0], GroupsCount[1], GroupsCount[2] );

					USING_COMPUTE_SHADER_END

					m_ppRTInScattering[1]->RemoveFromLastAssignedSlotUAV();
					m_pRTDeltaScatteringRayleigh->RemoveFromLastAssignedSlots();
					m_pRTDeltaScatteringMie->RemoveFromLastAssignedSlots();
				}
			}
		}
		break;

	//////////////////////////////////////////////////////////////////////////
	// Loop for each scattering order (line 6 in algorithm 4.1)

	// ==================================================
	// Computes deltaJ (line 7 in algorithm 4.1)
	case COMPUTING_SCATTERING_DELTA:
		{
			if ( m_bStageStarting )
			{	// First step into that stage...
				InitStage( CurrentStageIndex );
				m_pCB_PreComputeSky->m.SetTargetSize( m_pRTDeltaScattering->GetWidth(), m_pRTDeltaScattering->GetHeight(), m_pRTDeltaScattering->GetDepth() );
				m_bStageStarting = false;
			}

			USING_COMPUTESHADER_START( *m_pCSComputeInScattering_Delta )

#ifdef ENABLE_PROFILING
				TimeProfile	Profile( m_pStageTimingCurrent[CurrentStageIndex] );
#endif

				m_pRTDeltaScattering->SetCSUAV( 1 );

				m_pRTDeltaIrradiance->SetCS( 10 );			// Input from 2 stages ago
				m_pRTDeltaScatteringRayleigh->SetCS( 11 );	// Input from last stage
//###Just to avoid the annoying warning each frame				if ( m_ScatteringOrder == 2 )
					m_pRTDeltaScatteringMie->SetCS( 12 );	// We only need Mie for the first stage...

				m_pCB_PreComputeSky->m._bFirstPass = m_ScatteringOrder == 2;
				m_pCB_PreComputeSky->UpdateData();

				M.Dispatch( m_pCB_PreComputeSky->m._GroupsCountX, m_pCB_PreComputeSky->m._GroupsCountY, m_pCB_PreComputeSky->m._GroupsCountZ );

			USING_COMPUTE_SHADER_END

			if ( IncreaseStagePass( CurrentStageIndex ) )
			{	// Stage is over!
				m_CurrentStage = COMPUTING_IRRADIANCE_DELTA;
				m_bStageStarting = true;

				m_pRTDeltaScattering->RemoveFromLastAssignedSlotUAV();// Will be assigned to slot 13 next stage
				m_pRTDeltaIrradiance->RemoveFromLastAssignedSlots();
				m_pRTDeltaScatteringRayleigh->RemoveFromLastAssignedSlots();
				m_pRTDeltaScatteringMie->RemoveFromLastAssignedSlots();
			}
		}
		break;

	//////////////////////////////////////////////////////////////////////////
	// ==================================================
	// Computes deltaE (line 8 in algorithm 4.1)
	case COMPUTING_IRRADIANCE_DELTA:
		{
			if ( m_bStageStarting )
			{	// First step into that stage...
				InitStage( CurrentStageIndex );
				m_pCB_PreComputeSky->m.SetTargetSize( m_pRTDeltaIrradiance->GetWidth(), m_pRTDeltaIrradiance->GetHeight() );
				m_bStageStarting = false;
			}

			USING_COMPUTESHADER_START( *m_pCSComputeIrradiance_Delta )

#ifdef ENABLE_PROFILING
				TimeProfile	Profile( m_pStageTimingCurrent[CurrentStageIndex] );
#endif

				m_pRTDeltaIrradiance->SetCSUAV( 0 );

				m_pRTDeltaScatteringRayleigh->SetCS( 11 );	// Input from last stage
//###Just to avoid the annoying warning each frame				if ( m_ScatteringOrder == 2 )
					m_pRTDeltaScatteringMie->SetCS( 12 );	// We only need Mie for the first stage...

				m_pCB_PreComputeSky->m._bFirstPass = m_ScatteringOrder == 2;
				m_pCB_PreComputeSky->UpdateData();

				M.Dispatch( m_pCB_PreComputeSky->m._GroupsCountX, m_pCB_PreComputeSky->m._GroupsCountY, m_pCB_PreComputeSky->m._GroupsCountZ );

			USING_COMPUTE_SHADER_END

			if ( IncreaseStagePass( CurrentStageIndex ) )
			{	// Stage is over!
				m_CurrentStage = COMPUTING_SCATTERING_MULTIPLE;
				m_bStageStarting = true;

				m_pRTDeltaIrradiance->RemoveFromLastAssignedSlotUAV();	// Will be assigned to slot 10 for accumulation at the end of next stage
				m_pRTDeltaScatteringRayleigh->RemoveFromLastAssignedSlots();
				m_pRTDeltaScatteringMie->RemoveFromLastAssignedSlots();
			}
		}
		break;

	//////////////////////////////////////////////////////////////////////////
	// ==================================================
	// Computes deltaS (line 9 in algorithm 4.1)
	case COMPUTING_SCATTERING_MULTIPLE:
		{
			if ( m_bStageStarting )
			{	// First step into that stage...
				InitStage( CurrentStageIndex );
				m_pCB_PreComputeSky->m.SetTargetSize( m_pRTDeltaScatteringRayleigh->GetWidth(), m_pRTDeltaScatteringRayleigh->GetHeight(), m_pRTDeltaScatteringRayleigh->GetDepth() );
				m_bStageStarting = false;
			}

			USING_COMPUTESHADER_START( *m_pCSComputeInScattering_Multiple )

#ifdef ENABLE_PROFILING
				TimeProfile	Profile( m_pStageTimingCurrent[CurrentStageIndex] );
#endif

				m_pRTDeltaScatteringRayleigh->SetCSUAV( 1 );	// Warning: We're re-using Rayleigh slot.
																// It doesn't matter for orders > 2 where we don't sample from Rayleigh+Mie separately anymore (only done in first pass)

				m_pRTDeltaScattering->SetCS( 13 );	// Input from 2 stages ago

				m_pCB_PreComputeSky->m._bFirstPass = m_ScatteringOrder == 2;
				m_pCB_PreComputeSky->UpdateData();

				M.Dispatch( m_pCB_PreComputeSky->m._GroupsCountX, m_pCB_PreComputeSky->m._GroupsCountY, m_pCB_PreComputeSky->m._GroupsCountZ );

			USING_COMPUTE_SHADER_END

			if ( IncreaseStagePass( CurrentStageIndex ) )
			{	// Stage is over!
				m_CurrentStage = COMPUTING_SCATTERING_DELTA;	// Loop back for another scattering order
				m_bStageStarting = true;
				m_ScatteringOrder++;							// Next scattering order!

				m_pRTDeltaScattering->RemoveFromLastAssignedSlots();
//				m_Device.RemoveShaderResources( 13, 1, SSF_COMPUTE_SHADER );	// Remove from input

				// Will be assigned to slot 11 when we loop back to stage COMPUTING_SCATTERING_DELTA
				m_pRTDeltaScatteringRayleigh->RemoveFromLastAssignedSlotUAV();

				// ==================================================
				// Adds deltaE to irradiance texture E (line 10 in algorithm 4.1)
				{
					int		GroupsCount[3];
					InitSinglePassStage( IRRADIANCE_W, IRRADIANCE_H, 1, GroupsCount );

					USING_COMPUTESHADER_START( *m_pCSAccumulateIrradiance )

						m_ppRTIrradiance[2]->SetCSUAV( 0 );

						m_ppRTIrradiance[1]->SetCS( 14 );	// Previous values as SRV

						m_pRTDeltaIrradiance->SetCS( 10 );	// Input from last stage

						m_pCB_PreComputeSky->UpdateData();

						M.Dispatch( GroupsCount[0], GroupsCount[1], GroupsCount[2] );

					USING_COMPUTE_SHADER_END

					m_ppRTIrradiance[2]->RemoveFromLastAssignedSlotUAV();
					m_ppRTIrradiance[1]->RemoveFromLastAssignedSlots();
					m_pRTDeltaIrradiance->RemoveFromLastAssignedSlots();

					{	// Swap double-buffered accumulators
						Texture2D*	pTemp = m_ppRTIrradiance[1];
						m_ppRTIrradiance[1] = m_ppRTIrradiance[2];
						m_ppRTIrradiance[2] = pTemp;
					}
				}

				// ==================================================
//*				// Adds deltaS to inscatter texture S (line 11 in algorithm 4.1)
				{
					int		GroupsCount[3];
					InitSinglePassStage( RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, GroupsCount );

					USING_COMPUTESHADER_START( *m_pCSAccumulateInScattering )

						m_ppRTInScattering[2]->SetCSUAV( 1 );

						m_ppRTInScattering[1]->SetCS( 15 );	// Previous values as SRV

						m_pRTDeltaScatteringRayleigh->SetCS( 11 );

						m_pCB_PreComputeSky->UpdateData();

						M.Dispatch( GroupsCount[0], GroupsCount[1], GroupsCount[2] );

					USING_COMPUTE_SHADER_END

					m_ppRTInScattering[2]->RemoveFromLastAssignedSlotUAV();
					m_ppRTInScattering[1]->RemoveFromLastAssignedSlots();
					m_pRTDeltaScatteringRayleigh->RemoveFromLastAssignedSlots();

					{	// Swap double-buffered accumulators
						Texture3D*	pTemp = m_ppRTInScattering[1];
						m_ppRTInScattering[1] = m_ppRTInScattering[2];
						m_ppRTInScattering[2] = pTemp;
					}
				}
//*/

				//////////////////////////////////////////////////////////////////////////
				//////////////////////////////////////////////////////////////////////////
				// COMPLETION POINT
				if ( m_ScatteringOrder > MAX_SCATTERING_ORDER )
				{	// And we're done!
					m_CurrentStage = COMPUTING_STOPPED;

					// If we clear now, this will discard any change of parameter that could have happened during this update...
					// Only changes from now on will trigger an update again...
//					m_bSkyTableDirty = false;

					// Assign final textures to slots 8 & 9
					m_ppRTInScattering[0]->RemoveFromLastAssignedSlots();
					m_ppRTIrradiance[0]->RemoveFromLastAssignedSlots();
					m_ppRTInScattering[1]->Set( 8, true );
					m_ppRTIrradiance[1]->Set( 9, true );

					// Swap double-buffered slots
					Texture3D*	pTemp0 = m_ppRTInScattering[0];
					m_ppRTInScattering[0] = m_ppRTInScattering[1];
					m_ppRTInScattering[1] = pTemp0;

					Texture2D*	pTemp1 = m_ppRTIrradiance[0];
					m_ppRTIrradiance[0] = m_ppRTIrradiance[1];
					m_ppRTIrradiance[1] = pTemp1;
				}
				// COMPLETION POINT
				//////////////////////////////////////////////////////////////////////////
				//////////////////////////////////////////////////////////////////////////
			}
		}
		break;
	}
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

	float		HRefRayleigh = 8.0f;
	float		HRefMie = 1.2f;

	float		Sigma_s_Mie = 0.004f;	// !!!May cause strong optical depths and very low values if increased!!
	NjFloat3	Sigma_t_Mie = (Sigma_s_Mie / 0.9f) * NjFloat3::One;	// Should this be a parameter as well?? People might set it to values > 1 and that's physically incorrect...
	NjFloat3	Sigma_s_Rayleigh( 0.0058f, 0.0135f, 0.0331f );
//	NjFloat3	Sigma_s_Rayleigh( 0, 0, 0 );
								  
NjFloat3	MaxopticalDepth = NjFloat3::Zero;
int		MaxOpticalDepthX = -1;
int		MaxOpticalDepthY = -1;
	NjFloat2	UV;
	for ( int Y=0; Y < _Height; Y++ ) {
		UV.y = Y / (_Height-1.0f);
		float	AltitudeKm = UV.y*UV.y * ATMOSPHERE_THICKNESS_KM;					// Grow quadratically to have more precision near the ground

#ifdef USE_PRECISE_COS_THETA_MIN
		float	RadiusKm = GROUND_RADIUS_KM + 1e-2f + AltitudeKm;
		float	CosThetaMin = -1e-2f + -sqrtf( 1.0f - GROUND_RADIUS_KM*GROUND_RADIUS_KM / (RadiusKm*RadiusKm) );	// -0.13639737868529368408722196006097 at 60km
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

// if ( CosTheta > 0.0f )
// 	CosTheta = LERP( 0.02f, 1.0f, CosTheta );
// else
// 	CosTheta = LERP( -0.02f, -1.0f, -CosTheta );

// float	RadiusKm = GROUND_RADIUS_KM + 1e-2f + AltitudeKm;
// float	CosThetaGround = -sqrtf( 1.0f - GROUND_RADIUS_KM*GROUND_RADIUS_KM / (RadiusKm*RadiusKm) );	// -0.13639737868529368408722196006097 at 60km
// if ( CosTheta > CosThetaGround )
// 	CosTheta = LERP( CosThetaGround+0.01f, 1.0f, CosTheta / (1.0f-CosThetaGround) );
// else
// 	CosTheta = LERP( CosThetaGround+0.1001f, -1.0f, -CosTheta / (1.0f+CosThetaGround) );

//CosTheta = 1.0f - 0.999f * (1.0f - CosTheta);

			bool		groundHit = false;
			NjFloat3	OpticalDepth = Sigma_s_Rayleigh * ComputeOpticalDepth( AltitudeKm, CosTheta, HRefRayleigh, groundHit ) + Sigma_t_Mie * ComputeOpticalDepth( AltitudeKm, CosTheta, HRefMie, groundHit );
// 			if ( groundHit ) {
// 				scanline->Set( 1e-4f, 1e-4f, 1e-4f );	// Special case...
// 				continue;
// 			}

if ( OpticalDepth.z > MaxopticalDepth.z ) {
	MaxopticalDepth = OpticalDepth;
	MaxOpticalDepthX = X;
	MaxOpticalDepthY = Y;
}

//### 			// Here, the blue channel's optical depth's max value has been reported to be 19.6523819
// 			//	but the minimum supported value for Half16 has been measured to be something like 6.10351563e-5 (equivalent to d = -ln(6.10351563e-5) = 9.7040605270200343321767940202312)
// 			// What I'm doing here is patching very long optical depths in the blue channel to remap
// 			//	the [8,19.6523819] interval into [8,9.704061]
// 			//
// 			static const float	MAX_OPTICAL_DEPTH = 19.652382f;
// 			if ( OpticalDepth.z > 8.0f ) {
// 				OpticalDepth.z = 8.0f + (9.70f-8.0f) * SATURATE( (OpticalDepth.z - 8.0f) / (MAX_OPTICAL_DEPTH-8.0f) );
// 			}

//			scanline->Set( expf( -OpticalDepth.x ), expf( -OpticalDepth.y ), expf( -OpticalDepth.z ) );
			*scanline = OpticalDepth;
//			scanline->Set( sqrtf(OpticalDepth.x), sqrtf(OpticalDepth.y), sqrtf(OpticalDepth.z) );

//scanline->Set( 1e-4f+expf( -OpticalDepth.x ), 1e-4f+expf( -OpticalDepth.y ), 1e-4f+expf( -OpticalDepth.z ) );	// Just to avoid any division by 0 in the shader...

// 			scanline->x = 1.0f - idMath::Pow( scanline->x, 1.0f/8 );
// 			scanline->y = 1.0f - idMath::Pow( scanline->y, 1.0f/8 );
// 			scanline->z = 1.0f - idMath::Pow( scanline->z, 1.0f/8 );

// #ifdef _DEBUG
// // CHECK Ensure we never get 0 from a non 0 value
// NjHalf	TestX( scanline->x );
// if ( scanline->x != 0.0f && TestX.raw == 0 ) {
// 	DebugBreak();
// }
// NjHalf	TestY( scanline->y );
// if ( scanline->y != 0.0f && TestY.raw == 0 ) {
// 	DebugBreak();
// }
// NjHalf	TestZ( scanline->z );
// if ( scanline->z != 0.0f && TestZ.raw == 0 ) {
// 	DebugBreak();
// }
// // CHECK
// #endif
		}
	}




#ifdef _DEBUG
const float	WORLD2KM = 0.5f;
const float	CameraAltitudeKm = WORLD2KM * 1.5f;
NjFloat3	PositionWorldKm( 0, CameraAltitudeKm, 0 );

//float		ViewAngle = NUAJDEG2RAD(90.0f + 1.0f);	// Slightly downward
//float		ViewAngle = NUAJDEG2RAD(90.0f + 0.5f);	// Slightly downward

float		CameraRadiusKm = GROUND_RADIUS_KM+CameraAltitudeKm;
float		ViewAngle = acosf( -sqrtf( 1-GROUND_RADIUS_KM*GROUND_RADIUS_KM/(CameraRadiusKm*CameraRadiusKm) ) );

NjFloat3	View = NjFloat3( sinf(ViewAngle), cosf(ViewAngle), 0 );

float		HitDistanceKm = 200.0f;

	HitDistanceKm = MIN( HitDistanceKm, SphereIntersectionExit( PositionWorldKm, View, ATMOSPHERE_THICKNESS_KM ) );	// Limit to the atmosphere

if ( View.y < 0.0f )
	HitDistanceKm = MIN( HitDistanceKm, SphereIntersectionEnter( PositionWorldKm, View, 0.0f ) );					// Limit to the ground

NjFloat3	Test0 = GetTransmittance( PositionWorldKm.y, View.y, HitDistanceKm );
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

// 	if ( _CosTheta * CosTheta2 < 0.0f )
// 		DebugBreak();

	float	CosThetaGround = -sqrtf( 1.0f - (GROUND_RADIUS_KM*GROUND_RADIUS_KM) / (RadiusKm*RadiusKm) );

	NjFloat3	T0, T1;
	if ( _CosTheta > CosThetaGround )
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
// NjHalf4		T0Half = NjHalf4( NjFloat4( T0, 0 ) );
// NjHalf4		T1Half = NjHalf4( NjFloat4( T1, 0 ) );
// NjFloat4	ResultHalf = NjHalf4( NjFloat4( T0Half ) / NjFloat4( T1Half ) );
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
float	EffectVolumetric::SphereIntersectionEnter( const NjFloat3& _PositionKm, const NjFloat3& _View, float _SphereAltitudeKm ) const
{
	NjFloat3	EarthCenterKm( 0, -GROUND_RADIUS_KM, 0 );
	float		R = _SphereAltitudeKm + GROUND_RADIUS_KM;
	NjFloat3	D = _PositionKm - EarthCenterKm;
	float		c = (D | D) - R*R;
	float		b = D | _View;

	float		Delta = b*b - c;
	if ( Delta < 0.0f )
		return 1e6f;

	Delta = sqrt(Delta);
	float	HitDistance = -b - Delta;
	return  HitDistance;
}

// Computes the exit intersection of a ray and a sphere
// (No check for validity!)
float	EffectVolumetric::SphereIntersectionExit( const NjFloat3& _PositionKm, const NjFloat3& _View, float _SphereAltitudeKm ) const
{
	NjFloat3	EarthCenterKm( 0, -GROUND_RADIUS_KM, 0 );
	float		R = _SphereAltitudeKm + GROUND_RADIUS_KM;
	NjFloat3	D = _PositionKm - EarthCenterKm;
	float		c = (D | D) - R*R;
	float		b = D | _View;

	float		Delta = b*b - c;
	if ( Delta < 0.0f )
		return 1e6f;

	Delta = sqrt(Delta);
	float	HitDistance = -b + Delta;
	return  HitDistance;
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
