//////////////////////////////////////////////////////////////////////////
// Builds the sky tables using time-sliced Pixel Shader tasks
//

#define FILENAME_IRRADIANCE		"./TexIrradiance_64x16.pom"
#define FILENAME_TRANSMITTANCE	"./TexTransmittance_256x64.pom"
#define FILENAME_SCATTERING		"./TexScattering_256x128x32.pom"

//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//
namespace
{
	Texture2D*			m_pRTDeltaIrradiance = NULL;				// deltaE (temp)
	Texture3D*			m_pRTDeltaScatteringRayleigh = NULL;		// deltaSR (temp)
	Texture3D*			m_pRTDeltaScatteringMie = NULL;				// deltaSM (temp)
	Texture3D*			m_pRTDeltaScattering = NULL;				// deltaJ (temp)

	RasterizerState*	m_pRS_CullNoneWithScissoring;

	Material*			m_pMatPSTComputeTransmittance = NULL;		// PST stands for precompute sky table
	Material*			m_pMatPSTComputeTransmittance_Limited = NULL;
	Material*			m_pMatPSTComputeIrradiance_Single = NULL;
	Material*			m_pMatPSTComputeIrradiance_Delta = NULL;
	Material*			m_pMatPSTComputeInScattering_Single = NULL;
	Material*			m_pMatPSTComputeInScattering_Delta = NULL;
	Material*			m_pMatPSTtComputeIrradiance_Delta = NULL;
	Material*			m_pMatPSTComputeInScattering_Multiple = NULL;
	Material*			m_pMatPSTMergeInitialScattering = NULL;
	Material*			m_pMatPSTAccumulateIrradiance = NULL;
	Material*			m_pMatPSTAccumulateInScattering = NULL;

	bool				m_bSkyTableDirty = false;

	// Update Stages Description
	static const int	MAX_SCATTERING_ORDER = 4;						// Render up to order 4, further order events don't matter that much
//static const int	MAX_SCATTERING_ORDER = 2;//###

	static const int	THREADS_COUNT_X = 16;							// !!IMPORTANT ==> Must correspond to what's written in the shader!!
	static const int	THREADS_COUNT_Y = 16;
	static const int	THREADS_COUNT_Z = 4;							// 4 as the product of all thread counts cannot exceed 1024!

	enum STAGE_INDEX
	{
		COMPUTING_STOPPED = -1,

		// INITIAL COMPUTATION
		COMPUTING_TRANSMITTANCE = 0,
//		COMPUTING_TRANSMITTANCE_LIMITED = 1,
		COMPUTING_IRRADIANCE_SINGLE = 2,
		COMPUTING_SCATTERING_SINGLE = 3,

		// Multi-Pass
		COMPUTING_SCATTERING_DELTA = 4,
		COMPUTING_IRRADIANCE_DELTA = 5,
		COMPUTING_SCATTERING_MULTIPLE = 6,

		STAGES_COUNT,
	};

	STAGE_INDEX			m_CurrentStage = COMPUTING_STOPPED;
	bool				m_bStageStarting = true;
	int					m_ScatteringOrder = 2;

	U32					m_pStageTargetSizes[3*STAGES_COUNT] = {
		TRANSMITTANCE_W,			TRANSMITTANCE_H,			1,							// #1 Transmittance table
		TRANSMITTANCE_LIMITED_W,	TRANSMITTANCE_LIMITED_H,	TRANSMITTANCE_LIMITED_D,	// #2 Limited Transmittance table
		IRRADIANCE_W,				IRRADIANCE_H,				1,							// #3 Irradiance table (single scattering)
		RES_3D_U,					RES_3D_COS_THETA_VIEW,		RES_3D_ALTITUDE,			// #4 Scattering table (single scattering)

		// Multi-pass
		RES_3D_U,					RES_3D_COS_THETA_VIEW,		RES_3D_ALTITUDE,			// #5 Delta-Scattering table (used to compute actual irradiance & multiple-scattering at current order)
		IRRADIANCE_W,				IRRADIANCE_H,				1,							// #6 Irradiance table (multiple scattering)
		RES_3D_U,					RES_3D_COS_THETA_VIEW,		RES_3D_ALTITUDE,			// #7 Multiple Scattering table
	};

	U32					m_pStageGroupsCountPerFrame[3*STAGES_COUNT] = {
//		1,	1,	1,	// #1 Transmittance table						<= This computes a 16x16 slice each frame (takes 16x4 frames to complete the entire table)
		16,	4,	1,	// #1 Transmittance table						<= This computes a 256x64 slice each frame (takes 1 frames to complete the entire table)
//		1,	1,	1,	// #2 Limited Transmittance table				<= This computes a 16x16 slice each frame (takes 16x4x16 frames to complete the entire table)
		16,	4,	1,	// #2 Limited Transmittance table				<= This computes a 256x64 slice each frame (takes 16 frames to complete the entire table)
//		1,	1,	1,	// #3 Irradiance table (single scattering)		<= This computes a 16x16 slice each frame (takes 4x1 frames to complete the entire table)
		4,	1,	1,	// #3 Irradiance table (single scattering)		<= This computes a 64x16 slice each frame (takes 1 frames to complete the entire table)
//		4,	4,	1,	// #4 Scattering table (single scattering)		<= This computes a 64x64x1 slice each frame (takes 8 frames to complete a single Z slice, 8x32 frames to update the entire table)
		16,	8,	1,	// #4 Scattering table (single scattering)		<= This computes a single Z slice of (16*16)x(16*8) = 256x128 each frame (maybe heavy but at least updates faster!)

		// Multi-pass
//		4,	4,	1,	// #5 Delta-Scattering table (used to compute actual irradiance & multiple-scattering at current order)	<= This computes a 64x64x1 slice each frame (takes 8 frames to complete a single Z slice, 8x32 frames to update the entire table)
		16,	8,	1,	// #5 Delta-Scattering table (used to compute actual irradiance & multiple-scattering at current order)	<= This computes a single Z slice of (16*16)x(16*8) = 256x128 each frame (maybe heavy but at least updates faster!)
//		1,	1,	1,	// #6 Irradiance table (multiple scattering)	<= This computes a 16x16 slice each frame (takes 4x1 frames to complete the entire table)
		4,	1,	1,	// #6 Irradiance table (multiple scattering)	<= This computes a 64x16 slice each frame (takes 1 frames to complete the entire table)
//		4,	4,	1,	// #7 Multiple Scattering table					<= This computes a 64x64x1 slice each frame (takes 8 frames to complete a single Z slice, 8x32 frames to update the entire table)
		16,	8,	1,	// #7 Multiple Scattering table					<= This computes a single Z slice of (16*16)x(16*8) = 256x128 each frame (maybe heavy but at least updates faster!)
	};

	U32					m_pStagePassesCount[3*STAGES_COUNT];	// Filled automatically in InitUpdateSkyTables(), derived from the 2 tables above

#ifdef _DEBUG
//#define ENABLE_PROFILING
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

void	EffectVolumetric::InitSkyTables()
{
	m_pRTDeltaIrradiance = new Texture2D( m_Device, IRRADIANCE_W, IRRADIANCE_H, 1, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, false, UAV );								// deltaE (temp)
	m_pRTDeltaScatteringRayleigh = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, false, UAV );	// deltaSR (temp)
	m_pRTDeltaScatteringMie = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, false, UAV );			// deltaSM (temp)
	m_pRTDeltaScattering = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, false, UAV );			// deltaJ (temp)

	// Create the rasterizer state that enables scissoring
	{
		D3D11_RASTERIZER_DESC	Desc;
		memset( &Desc, 0, sizeof(Desc) );
		Desc.FillMode = D3D11_FILL_SOLID;
        Desc.CullMode = D3D11_CULL_NONE;
        Desc.FrontCounterClockwise = TRUE;
        Desc.DepthBias = D3D11_DEFAULT_DEPTH_BIAS;
        Desc.DepthBiasClamp = D3D11_DEFAULT_DEPTH_BIAS_CLAMP;
        Desc.SlopeScaledDepthBias = D3D11_DEFAULT_SLOPE_SCALED_DEPTH_BIAS;
        Desc.DepthClipEnable = TRUE;
        Desc.ScissorEnable = TRUE;		// ENABLED!
        Desc.MultisampleEnable = FALSE;
        Desc.AntialiasedLineEnable = FALSE;

		m_pRS_CullNoneWithScissoring = new RasterizerState( m_Device, Desc );
	}

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

#if 1
	// Build heavy shaders
	CHECK_MATERIAL( m_pMatPSTComputeTransmittance = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,			"VS", "GS", "PreComputeTransmittance" ), 10 );
	CHECK_MATERIAL( m_pMatPSTComputeTransmittance_Limited = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,	"VS", "GS", "PreComputeTransmittance_Limited" ), 11 );
	CHECK_MATERIAL( m_pMatPSTComputeIrradiance_Single = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,		"VS", "GS", "PreComputeIrradiance_Single" ), 12 );		// irradiance1
	CHECK_MATERIAL( m_pMatPSTComputeIrradiance_Delta = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,		"VS", "GS", "PreComputeIrradiance_Delta" ), 13 );		// irradianceN*
	CHECK_MATERIAL( m_pMatPSTComputeInScattering_Single = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,	"VS", "GS", "PreComputeInScattering_Single" ), 14 );	// inscatter1
	CHECK_MATERIAL( m_pMatPSTComputeInScattering_Delta = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,		"VS", "GS", "PreComputeInScattering_Delta" ), 15 );		// inscatterS
	CHECK_MATERIAL( m_pMatPSTComputeInScattering_Multiple = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,	"VS", "GS", "PreComputeInScattering_Multiple" ), 16 );	// inscatterN
	CHECK_MATERIAL( m_pMatPSTMergeInitialScattering = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,		"VS", "GS", "MergeInitialScattering" ), 17 );			// copyInscatter1
	CHECK_MATERIAL( m_pMatPSTAccumulateIrradiance = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,			"VS", "GS", "AccumulateIrradiance" ), 18 );				// copyIrradiance
	CHECK_MATERIAL( m_pMatPSTAccumulateInScattering = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,		"VS", "GS", "AccumulateInScattering" ), 19 );			// copyInscatterN
#else
	// Reload from binary blobs
	CHECK_MATERIAL( m_pMatPSTComputeTransmittance = Material::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,					"VS", NULL, NULL, "GS",	"PreComputeTransmittance" ), 10 );
	CHECK_MATERIAL( m_pMatPSTComputeTransmittance_Limited = Material::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,			"VS", NULL, NULL, "GS",	"PreComputeTransmittance_Limited" ), 11 );
	CHECK_MATERIAL( m_pMatPSTComputeIrradiance_Single = Material::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,				"VS", NULL, NULL, "GS",	"PreComputeIrradiance_Single" ), 12 );
	CHECK_MATERIAL( m_pMatPSTComputeIrradiance_Delta = Material::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,				"VS", NULL, NULL, "GS",	"PreComputeIrradiance_Delta" ), 13 );
	CHECK_MATERIAL( m_pMatPSTComputeInScattering_Single = Material::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,			"VS", NULL, NULL, "GS",	"PreComputeInScattering_Single" ), 14 );
	CHECK_MATERIAL( m_pMatPSTComputeInScattering_Delta = Material::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,			"VS", NULL, NULL, "GS",	"PreComputeInScattering_Delta" ), 15 );
	CHECK_MATERIAL( m_pMatPSTComputeInScattering_Multiple = Material::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,			"VS", NULL, NULL, "GS",	"PreComputeInScattering_Multiple" ), 16 );
	CHECK_MATERIAL( m_pMatPSTMergeInitialScattering = Material::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,				"VS", NULL, NULL, "GS",	"MergeInitialScattering" ), 17 );
	CHECK_MATERIAL( m_pMatPSTAccumulateIrradiance = Material::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,					"VS", NULL, NULL, "GS",	"AccumulateIrradiance" ), 18 );
	CHECK_MATERIAL( m_pMatPSTAccumulateInScattering = Material::CreateFromBinaryBlob( m_Device, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,				"VS", NULL, NULL, "GS",	"AccumulateInScattering" ), 19 );
#endif

//###
// 	CHECK_MATERIAL( m_pMatPSTComputeInScattering_Single = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,	"VS", "GS", "PreComputeInScattering_Single" ), 13 );	// inscatter1
// 	CHECK_MATERIAL( m_pMatPSTMergeInitialScattering = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,		"VS", "GS", "MergeInitialScattering" ), 16 );			// copyInscatter1
// 	CHECK_MATERIAL( m_pMatPSTAccumulateIrradiance = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,			"VS", "GS", "AccumulateIrradiance" ), 17 );				// copyIrradiance
// 	CHECK_MATERIAL( m_pMatPSTAccumulateInScattering = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,		"VS", "GS", "AccumulateInScattering" ), 18 );			// copyInscatterN

//	CHECK_MATERIAL( m_pMatPSTComputeTransmittance_Limited = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, "./Resources/Shaders/VolumetricPreComputeAtmospherePS.hlsl", VertexFormatPt4::DESCRIPTOR,	"VS", "GS", "PreComputeTransmittance_Limited" ), 11 );

}

void	EffectVolumetric::ExitUpdateSkyTables()
{
	// Release materials & temporary RTs
	delete m_pMatPSTAccumulateInScattering;
	delete m_pMatPSTAccumulateIrradiance;
	delete m_pMatPSTMergeInitialScattering;
	delete m_pMatPSTComputeInScattering_Multiple;
	delete m_pMatPSTComputeInScattering_Delta;
	delete m_pMatPSTComputeInScattering_Single;
	delete m_pMatPSTComputeIrradiance_Delta;
	delete m_pMatPSTComputeIrradiance_Single;
	delete m_pMatPSTComputeTransmittance_Limited;
	delete m_pMatPSTComputeTransmittance;

	delete m_pRS_CullNoneWithScissoring;

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

void	EffectVolumetric::InitMultiPassStage( int _StageIndex, int _TargetSizeX, int _TargetSizeY, int _TargetSizeZ, int _StepsCount )
{
	ASSERT( m_pCB_PreComputeSky->m._PassIndexX == 0 && m_pCB_PreComputeSky->m._PassIndexY == 0 && m_pCB_PreComputeSky->m._PassIndexZ == 0, "Pass index should always equal 0 at the beginning of a new stage!" );

	m_pCB_PreComputeSky->m._StepsCount = _StepsCount;

	m_pCB_PreComputeSky->m.SetTargetSize( _TargetSizeX, _TargetSizeY, _TargetSizeZ );

	// Setup groups count
	int	GroupsCountX = m_pStageGroupsCountPerFrame[3*_StageIndex+0];
	int	GroupsCountY = m_pStageGroupsCountPerFrame[3*_StageIndex+1];
	int	GroupsCountZ = m_pStageGroupsCountPerFrame[3*_StageIndex+2];
	m_pCB_PreComputeSky->m.SetGroupsCount( GroupsCountX, GroupsCountY, GroupsCountZ );

//CHECK
float	PassesCountX = float( _TargetSizeX >> 4 ) / GroupsCountX;	// Must not have mantissa!
float	PassesCountY = float( _TargetSizeY >> 4 ) / GroupsCountY;	// Must not have mantissa!
float	PassesCountZ = float( _TargetSizeZ > 1 ? (_TargetSizeZ >> 2) : 1 ) / GroupsCountZ;	// Must not have mantissa!
//CHECK

#ifdef ENABLE_PROFILING
	m_pStageTimingMin[_StageIndex] = MAX_FLOAT;
	m_pStageTimingMax[_StageIndex] = 0.0;
	m_pStageTimingCount[_StageIndex] = 0;
	m_pStageTimingTotal[_StageIndex] = 0.0;
	m_pStageTimingAvg[_StageIndex] = 0.0;
#endif
}

void	EffectVolumetric::InitSinglePassStage( int _TargetSizeX, int _TargetSizeY, int _TargetSizeZ, int _StepsCount )
{
	m_pCB_PreComputeSky->m._StepsCount = _StepsCount;

	m_pCB_PreComputeSky->m.SetTargetSize( _TargetSizeX, _TargetSizeY, _TargetSizeZ );
	m_pCB_PreComputeSky->m.SetGroupsCount( _TargetSizeX >> 4, _TargetSizeY >> 4, _TargetSizeZ > 1 ? _TargetSizeZ >> 2 : 1 );
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

void	EffectVolumetric::DispatchStage( Material& M )
{
	m_pCB_PreComputeSky->UpdateData();

	// Instead of a CS dispatch, I simply set the scissor rect to render only the portion we need
	//	and call a draw instanced for the GS to output the result to the correct slice
	int	ScissorWidth = m_pCB_PreComputeSky->m._GroupsCountX * THREADS_COUNT_X;
	int	ScissorHeight = m_pCB_PreComputeSky->m._GroupsCountY * THREADS_COUNT_Y;
	int	ScissorX = m_pCB_PreComputeSky->m._PassIndexX * ScissorWidth;
	int	ScissorY = m_pCB_PreComputeSky->m._PassIndexY * ScissorHeight;
	D3D11_RECT	Scissor = {
		ScissorX, ScissorY,
		ScissorX + ScissorWidth, ScissorY + ScissorHeight
	};
	m_Device.SetScissorRect( &Scissor );

	m_ScreenQuad.RenderInstanced( M, m_pCB_PreComputeSky->m._TargetSizeZ > 1 ? m_pCB_PreComputeSky->m._GroupsCountZ * THREADS_COUNT_Z : 1 );
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
// Each block computes 16x16(x4) texels of our tables.
//
// So this functions is merely a state machine keeping track of what has been computed and what remains to be computed until the tables have all been updated.
//
//
void	EffectVolumetric::UpdateSkyTables()
{
	if ( !m_bSkyTableDirty && m_CurrentStage == COMPUTING_STOPPED )
		return;

	// Set the rasterizer state that enables scissoring
	m_Device.SetStates( m_pRS_CullNoneWithScissoring, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );

	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////
	// STARTING POINT
	if ( m_CurrentStage == COMPUTING_STOPPED )
	{	// Initiate update process
		m_bSkyTableDirty = false;	// Clear immediately so we can still trigger a new update while updating... This new update will only start once this update is complete.
		m_CurrentStage = COMPUTING_TRANSMITTANCE;
		m_ScatteringOrder = 2;		// We start the loop at order 2 so we loop up to MAX_SCATTERING_ORDER
		m_pCB_PreComputeSky->m._bFirstPass = true;
		m_bStageStarting = true;

		// Remove targets from any assigned SRV slots so we can start writing in them again
		// They could still be bound to SRVs for debug purpose
		m_pRTDeltaIrradiance->RemoveFromLastAssignedSlots();
		m_pRTDeltaScatteringRayleigh->RemoveFromLastAssignedSlots();
		m_pRTDeltaScatteringMie->RemoveFromLastAssignedSlots();
		m_pRTDeltaScattering->RemoveFromLastAssignedSlots();
	}
	// STARTING POINT
	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////

	int	CurrentStageIndex = int(m_CurrentStage);

	switch ( m_CurrentStage )
	{
	//////////////////////////////////////////////////////////////////////////
	// Computes transmittance texture T (line 1 in algorithm 4.1)
	// This integrates Air/Fog density along a ray until it exits the atmosphere or hits the ground
	// We thus obtain the optical depth and store exp( -Optical Depth ), the transmittance of the atmosphere along the ray...
	//
	case COMPUTING_TRANSMITTANCE:
		{
			if ( m_bStageStarting )
			{	// First step into that stage...
				const int	STEPS_COUNT = 500;
				InitMultiPassStage( CurrentStageIndex, m_ppRTTransmittance[0]->GetWidth(), m_ppRTTransmittance[0]->GetHeight(), 1, STEPS_COUNT );
				m_bStageStarting = false;
			}

			USING_MATERIAL_START( *m_pMatPSTComputeTransmittance )
	
#ifdef ENABLE_PROFILING
				TimeProfile	Profile( m_pStageTimingCurrent[CurrentStageIndex] );
#endif

				m_Device.SetRenderTarget( *m_ppRTTransmittance[1] );

				DispatchStage( M );

			USING_MATERIAL_END

			if ( IncreaseStagePass( CurrentStageIndex ) )
			{	// Stage is over!
//				m_CurrentStage = COMPUTING_TRANSMITTANCE_LIMITED;
				m_CurrentStage = COMPUTING_IRRADIANCE_SINGLE;
				m_bStageStarting = true;

				// Assign to slot 6
				m_Device.RemoveRenderTargets();
				m_ppRTTransmittance[1]->Set( 6, true );

				// This is our new default texture
				Texture2D*	pTemp = m_ppRTTransmittance[0];
				m_ppRTTransmittance[0] = m_ppRTTransmittance[1];
				m_ppRTTransmittance[1] = pTemp;
			}
		}
		break;

// 	//////////////////////////////////////////////////////////////////////////
// 	// Computes LIMITED transmittance texture T (line 1 in algorithm 4.1)
// 	// This integrates Air/Fog density along a ray until it reaches a specified distance
// 	// We thus obtain the optical depth and store exp( -Optical Depth ), the transmittance of the atmosphere along the ray...
// 	//
// 	case COMPUTING_TRANSMITTANCE_LIMITED:
// 		{
// 			if ( m_bStageStarting )
// 			{	// First step into that stage...
// 				InitMultiPassStage( CurrentStageIndex, m_ppRTTransmittanceLimited[0]->GetWidth(), m_ppRTTransmittanceLimited[0]->GetHeight(), m_ppRTTransmittanceLimited[0]->GetDepth() );
// 				m_bStageStarting = false;
// 			}
// 
// 			USING_MATERIAL_START( *m_pMatPSTComputeTransmittance_Limited )
// 	
// #ifdef ENABLE_PROFILING
// 				TimeProfile	Profile( m_pStageTimingCurrent[CurrentStageIndex] );
// #endif
// 
// 				m_Device.SetRenderTarget( *m_ppRTTransmittanceLimited[1] );
// 
// 				DispatchStage( M );
// 
// 			USING_MATERIAL_END
// 
// 			if ( IncreaseStagePass( CurrentStageIndex ) )
// 			{	// Stage is over!
// 				m_CurrentStage = COMPUTING_IRRADIANCE_SINGLE;
// 				m_bStageStarting = true;
// 
// 				// Assign to slot 7
// 				m_Device.RemoveRenderTargets();
// 				m_ppRTTransmittanceLimited[1]->Set( 7, true );
// 
// 				// This is our new default texture
// 				Texture3D*	pTemp = m_ppRTTransmittanceLimited[0];
// 				m_ppRTTransmittanceLimited[0] = m_ppRTTransmittanceLimited[1];
// 				m_ppRTTransmittanceLimited[1] = pTemp;
// 			}
// 		}
// 		break;

	//////////////////////////////////////////////////////////////////////////
	// Computes irradiance texture deltaE (line 2 in algorithm 4.1)
	// Stores Transmittance( Theta_Sun ) * cos(Theta_Sun), the direct Sun light irradiance
	//	arriving at a point of given altitude...
	//
	// This step is NOT stored in the final irradiance: only indirect irradiance
	//	is computed, simply because Sun Intensity * Transmittance * cos(Theta_Sun) is something
	//	you write as the diffuse lighting in your shader, and it's usually combined with a
	//	shadow map which we don't include here so better leave the direct lighting for real time...
	//
	case COMPUTING_IRRADIANCE_SINGLE:
		{
			if ( m_bStageStarting )
			{	// First step into that stage...
				const int	STEPS_COUNT = 0;
				InitMultiPassStage( CurrentStageIndex, m_pRTDeltaIrradiance->GetWidth(), m_pRTDeltaIrradiance->GetHeight(), 1, STEPS_COUNT );
				m_bStageStarting = false;

				m_pRTDeltaIrradiance->RemoveFromLastAssignedSlots();
			}

			USING_MATERIAL_START( *m_pMatPSTComputeIrradiance_Single )

#ifdef ENABLE_PROFILING
				TimeProfile	Profile( m_pStageTimingCurrent[CurrentStageIndex] );
#endif

				m_Device.SetRenderTarget( *m_pRTDeltaIrradiance );

				DispatchStage( M );

			USING_MATERIAL_END

			if ( IncreaseStagePass( CurrentStageIndex ) )
			{	// Stage is over!
				m_CurrentStage = COMPUTING_SCATTERING_SINGLE;
				m_bStageStarting = true;

				// Will be assigned to slot 13 next stage
				m_Device.RemoveRenderTargets();

				// ==================================================
 				// Clear irradiance texture E (line 4 in algorithm 4.1)
				m_Device.ClearRenderTarget( *m_ppRTIrradiance[1], float4::Zero );
			}
		}
		break;

	//////////////////////////////////////////////////////////////////////////
	// Computes single scattering texture deltaS (line 3 in algorithm 4.1)
	// Rayleigh and Mie separated in deltaSR + deltaSM
	//
	// Integrates along the view ray all incoming light from the Sun, attenuated through the atmosphere.
	// Doesn't multiply integral by phase functions (done at the next stage where the data are read back)
	//
	// Performs for each step until the ground/atmosphere:
	//	Scattering_Rayleigh += air_scatt * air_density(altitude) * Transmittance( View => Hit ) * Transmittance( Hit => Atmosphere ) * StepSize
	//	Scattering_Mie += fog_scatt * fog_density(altitude) * Transmittance( View => Hit ) * Transmittance( Hit => Atmosphere ) * StepSize
	// ==================================================
	//
	case COMPUTING_SCATTERING_SINGLE:
		{
			if ( m_bStageStarting )
			{	// First step into that stage...
				const int	STEPS_COUNT = 50;
				InitMultiPassStage( CurrentStageIndex, m_pRTDeltaScatteringRayleigh->GetWidth(), m_pRTDeltaScatteringRayleigh->GetHeight(), m_pRTDeltaScatteringRayleigh->GetDepth(), STEPS_COUNT );
				m_bStageStarting = false;

				m_pRTDeltaScatteringRayleigh->RemoveFromLastAssignedSlots();
				m_pRTDeltaScatteringMie->RemoveFromLastAssignedSlots();
			}

			USING_MATERIAL_START( *m_pMatPSTComputeInScattering_Single )

#ifdef ENABLE_PROFILING
				TimeProfile	Profile( m_pStageTimingCurrent[CurrentStageIndex] );
#endif

				ID3D11RenderTargetView*	ppTargets[2] = {
					m_pRTDeltaScatteringRayleigh->GetTargetView( 0, 0, 0 ),
					m_pRTDeltaScatteringMie->GetTargetView( 0, 0, 0 ),
				};
				m_Device.SetRenderTargets( m_pRTDeltaScatteringRayleigh->GetWidth(), m_pRTDeltaScatteringRayleigh->GetHeight(), 2, ppTargets );

				m_pRTDeltaIrradiance->SetPS( 64 );	// Input from last stage

				DispatchStage( M );

			USING_MATERIAL_END

			if ( IncreaseStagePass( CurrentStageIndex ) )
			{	// Stage is over!
				m_CurrentStage = COMPUTING_SCATTERING_DELTA;
				m_bStageStarting = true;

				// Will be assigned to slot 11 & 12 next stage
				m_Device.RemoveRenderTargets();

				// ==================================================
				// Merges DeltaScattering Rayleigh & Mie into initial inscatter texture S (line 5 in algorithm 4.1)
				// Simply stores float4( Rayleigh.xyz, Mie.x )
				{
					InitSinglePassStage( RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, 0 );

					USING_MATERIAL_START( *m_pMatPSTMergeInitialScattering )

						m_Device.SetRenderTarget( *m_ppRTScattering[1] );

						m_pRTDeltaScatteringRayleigh->SetPS( 65 );
						m_pRTDeltaScatteringMie->SetPS( 66 );

						DispatchStage( M );

					USING_MATERIAL_END

					m_Device.RemoveRenderTargets();

// 					m_pRTDeltaScatteringRayleigh->RemoveFromLastAssignedSlots();
// 					m_pRTDeltaScatteringMie->RemoveFromLastAssignedSlots();
				}

				// Special case when we want to stop at order 1 scattering (debug)
				if ( m_ScatteringOrder > MAX_SCATTERING_ORDER )
				{
					m_CurrentStage = COMPUTING_STOPPED;

					m_Device.SetStates( NULL, NULL, m_Device.m_pBS_Additive );

					// ==================================================
					// Adds deltaE to irradiance texture E (line 10 in algorithm 4.1)
					{
						InitSinglePassStage( IRRADIANCE_W, IRRADIANCE_H, 1, 0 );

						USING_MATERIAL_START( *m_pMatPSTAccumulateIrradiance )

//							m_Device.SetRenderTarget( *m_ppRTIrradiance[2] );
							m_Device.SetRenderTarget( *m_ppRTIrradiance[1] );

//							m_ppRTIrradiance[1]->SetPS( 68 );	// Previous values as SRV (cleared to 0 at the moment)
							m_pRTDeltaIrradiance->SetPS( 64 );	// Input from last stage

							DispatchStage( M );

						USING_MATERIAL_END

						m_Device.RemoveRenderTargets();
						m_ppRTIrradiance[1]->RemoveFromLastAssignedSlots();
//						m_pRTDeltaIrradiance->RemoveFromLastAssignedSlots();

// 						{	// Swap double-buffered accumulators
// 							Texture2D*	pTemp = m_ppRTIrradiance[1];
// 							m_ppRTIrradiance[1] = m_ppRTIrradiance[2];
// 							m_ppRTIrradiance[2] = pTemp;
// 						}
					}

					// Assign final textures to slots 8 & 9
					m_ppRTScattering[0]->RemoveFromLastAssignedSlots();
					m_ppRTIrradiance[0]->RemoveFromLastAssignedSlots();
					m_ppRTScattering[1]->Set( 8, true );
					m_ppRTIrradiance[1]->Set( 9, true );

					// Swap double-buffered slots
					Texture3D*	pTemp0 = m_ppRTScattering[0];
					m_ppRTScattering[0] = m_ppRTScattering[1];
					m_ppRTScattering[1] = pTemp0;

					Texture2D*	pTemp1 = m_ppRTIrradiance[0];
					m_ppRTIrradiance[0] = m_ppRTIrradiance[1];
					m_ppRTIrradiance[1] = pTemp1;
				}
			}
		}
		break;

	//////////////////////////////////////////////////////////////////////////
	// Loop for each scattering order (line 6 in algorithm 4.1)

	// ==================================================
	// Computes deltaJ (line 7 in algorithm 4.1)
	//
	// Integrates scattered light over the entire sphere of directions.
	// Every time it uses the values from previous stage, sampling lower irradiance/scattering values each new pass
	//
	// Performs for each sphere direction w and solid angle dw
	//	dScattering = (GroundScattering(w) + LightScattering(w)) dw
	//	Scattering += dScattering * (air_scatt * air_density(altitude) * PhaseRayleigh + fog_scatt * fog_density(altitude) * PhaseMie)
	//
	// => GroundScattering(w) = (GroundReflectance/PI) * Transmittance(w) * Irradiance		or 0 if not hitting the ground
	//
	// => LightScattering(w) = Sample4DScatteringTable( w )
	// ==================================================
	//
	case COMPUTING_SCATTERING_DELTA:
		{
			if ( m_bStageStarting )
			{	// First step into that stage...
				const int	STEPS_COUNT = 16;
				InitMultiPassStage( CurrentStageIndex, m_pRTDeltaScattering->GetWidth(), m_pRTDeltaScattering->GetHeight(), m_pRTDeltaScattering->GetDepth(), STEPS_COUNT );
				m_bStageStarting = false;

				m_pRTDeltaScattering->RemoveFromLastAssignedSlots();
			}

			USING_MATERIAL_START( *m_pMatPSTComputeInScattering_Delta )

#ifdef ENABLE_PROFILING
				TimeProfile	Profile( m_pStageTimingCurrent[CurrentStageIndex] );
#endif

				m_Device.SetRenderTarget( *m_pRTDeltaScattering );

				m_pRTDeltaIrradiance->SetPS( 64 );			// Input from 2 stages ago
				m_pRTDeltaScatteringRayleigh->SetPS( 65 );	// Input from last stage
//###Just to avoid the annoying warning each frame				if ( m_ScatteringOrder == 2 )
					m_pRTDeltaScatteringMie->SetPS( 66 );	// We only need Mie for the first stage...

				m_pCB_PreComputeSky->m._bFirstPass = m_ScatteringOrder == 2;

				DispatchStage( M );

			USING_MATERIAL_END

			if ( IncreaseStagePass( CurrentStageIndex ) )
			{	// Stage is over!
				m_CurrentStage = COMPUTING_IRRADIANCE_DELTA;
				m_bStageStarting = true;

				m_Device.RemoveRenderTargets();		// Will be assigned to slot 13 next stage
//				m_pRTDeltaIrradiance->RemoveFromLastAssignedSlots();
// 				m_pRTDeltaScatteringRayleigh->RemoveFromLastAssignedSlots();
// 				m_pRTDeltaScatteringMie->RemoveFromLastAssignedSlots();
			}
		}
		break;

	//////////////////////////////////////////////////////////////////////////
	// ==================================================
	// Computes deltaE (line 8 in algorithm 4.1)
	//
	// Integrates scattered radiance into irradiance over the upper hemisphere of directions.
	// Every time it uses the values from previous stage, sampling lower irradiance/scattering values each new pass
	//
	// Performs for each hemisphere direction w and solid angle dw
	//	Irradiance += LightScattering(w) (w.y) dw
	//
	// => LightScattering(w) = Sample4DScatteringTable( w )
	// => w.y = cos(theta) since we're computing the irradiance
	// ==================================================
	//
	case COMPUTING_IRRADIANCE_DELTA:
		{
			if ( m_bStageStarting )
			{	// First step into that stage...
				const int	STEPS_COUNT = 32;
				InitMultiPassStage( CurrentStageIndex, m_pRTDeltaIrradiance->GetWidth(), m_pRTDeltaIrradiance->GetHeight(), 1, 32 );
				m_bStageStarting = false;

				m_pRTDeltaIrradiance->RemoveFromLastAssignedSlots();
			}

			USING_MATERIAL_START( *m_pMatPSTComputeIrradiance_Delta )

#ifdef ENABLE_PROFILING
				TimeProfile	Profile( m_pStageTimingCurrent[CurrentStageIndex] );
#endif

				m_Device.SetRenderTarget( *m_pRTDeltaIrradiance );

				m_pRTDeltaScatteringRayleigh->SetPS( 65 );	// Input from last stage
//###Just to avoid the annoying warning each frame				if ( m_ScatteringOrder == 2 )
					m_pRTDeltaScatteringMie->SetPS( 66 );	// We only need Mie for the first stage...

				m_pCB_PreComputeSky->m._bFirstPass = m_ScatteringOrder == 2;

				DispatchStage( M );

			USING_MATERIAL_END

			if ( IncreaseStagePass( CurrentStageIndex ) )
			{	// Stage is over!
				m_CurrentStage = COMPUTING_SCATTERING_MULTIPLE;
				m_bStageStarting = true;

				m_Device.RemoveRenderTargets();	// Will be assigned to slot 10 for accumulation at the end of next stage
// 				m_pRTDeltaScatteringRayleigh->RemoveFromLastAssignedSlots();
// 				m_pRTDeltaScatteringMie->RemoveFromLastAssignedSlots();
			}
		}
		break;

	//////////////////////////////////////////////////////////////////////////
	// ==================================================
	// Computes deltaS (line 9 in algorithm 4.1)
	//
	// Integrates along the view ray all incoming light from the Sun scattered by previous passes.
	//
	// Performs for each step until the ground/atmosphere:
	//	Scattering += Transmittance( View => Hit ) * Sample4DScatteringTable( View ) * StepSize
	// ==================================================
	//
	case COMPUTING_SCATTERING_MULTIPLE:
		{
			if ( m_bStageStarting )
			{	// First step into that stage...
				const int	STEPS_COUNT = 50;
				InitMultiPassStage( CurrentStageIndex, m_pRTDeltaScatteringRayleigh->GetWidth(), m_pRTDeltaScatteringRayleigh->GetHeight(), m_pRTDeltaScatteringRayleigh->GetDepth(), STEPS_COUNT );
				m_bStageStarting = false;

				m_pRTDeltaScatteringRayleigh->RemoveFromLastAssignedSlots();
			}

			USING_MATERIAL_START( *m_pMatPSTComputeInScattering_Multiple )

#ifdef ENABLE_PROFILING
				TimeProfile	Profile( m_pStageTimingCurrent[CurrentStageIndex] );
#endif

				m_Device.SetRenderTarget( *m_pRTDeltaScatteringRayleigh );	// Warning: We're re-using Rayleigh slot.
																			// It doesn't matter for orders > 2 where we don't sample from Rayleigh+Mie separately anymore (only done in first pass)

				m_pRTDeltaScattering->SetPS( 67 );	// Input from 2 stages ago

				m_pCB_PreComputeSky->m._bFirstPass = m_ScatteringOrder == 2;

				DispatchStage( M );

			USING_MATERIAL_END

			if ( IncreaseStagePass( CurrentStageIndex ) )
			{	// Stage is over!
				m_CurrentStage = COMPUTING_SCATTERING_DELTA;	// Loop back for another scattering order
				m_bStageStarting = true;
				m_ScatteringOrder++;							// Next scattering order!

				m_Device.RemoveRenderTargets();

				// We're now accumulating values...
				m_Device.SetStates( NULL, NULL, m_Device.m_pBS_Additive );

				// ==================================================
				// Adds deltaE to irradiance texture E (line 10 in algorithm 4.1)
				{
					InitSinglePassStage( IRRADIANCE_W, IRRADIANCE_H, 1, 0 );

					USING_MATERIAL_START( *m_pMatPSTAccumulateIrradiance )

//						m_Device.SetRenderTarget( *m_ppRTIrradiance[2] );
						m_Device.SetRenderTarget( *m_ppRTIrradiance[1] );

//						m_ppRTIrradiance[1]->SetPS( 68 );	// Previous values as SRV
						m_pRTDeltaIrradiance->SetPS( 64 );	// Input from last stage

						DispatchStage( M );

					USING_MATERIAL_END

					m_Device.RemoveRenderTargets();
//					m_ppRTIrradiance[1]->RemoveFromLastAssignedSlots();
//					m_pRTDeltaIrradiance->RemoveFromLastAssignedSlots();

// 					{	// Swap double-buffered accumulators
// 						Texture2D*	pTemp = m_ppRTIrradiance[1];
// 						m_ppRTIrradiance[1] = m_ppRTIrradiance[2];
// 						m_ppRTIrradiance[2] = pTemp;
// 					}
				}

				// ==================================================
				// Adds deltaS to inscatter texture S (line 11 in algorithm 4.1)
				{
					InitSinglePassStage( RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, 0 );

					USING_MATERIAL_START( *m_pMatPSTAccumulateInScattering )

//						m_Device.SetRenderTarget( *m_ppRTScattering[2] );
						m_Device.SetRenderTarget( *m_ppRTScattering[1] );

//						m_ppRTScattering[1]->SetPS( 69 );	// Previous values as SRV
						m_pRTDeltaScatteringRayleigh->SetPS( 65 );

						DispatchStage( M );

					USING_MATERIAL_END

					m_Device.RemoveRenderTargets();
//					m_ppRTScattering[1]->RemoveFromLastAssignedSlots();
//					m_pRTDeltaScatteringRayleigh->RemoveFromLastAssignedSlots();

// 					{	// Swap triple-buffered accumulators
// 						Texture3D*	pTemp = m_ppRTScattering[1];
// 						m_ppRTScattering[1] = m_ppRTScattering[2];
// 						m_ppRTScattering[2] = pTemp;
// 					}
				}


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
					m_ppRTScattering[0]->RemoveFromLastAssignedSlots();
					m_ppRTIrradiance[0]->RemoveFromLastAssignedSlots();
					m_ppRTScattering[1]->Set( 8, true );
					m_ppRTIrradiance[1]->Set( 9, true );


#if 1
{
	Texture3D*	pStagingScattering = new Texture3D( m_Device, m_ppRTScattering[1]->GetWidth(), m_ppRTScattering[1]->GetHeight(), m_ppRTScattering[1]->GetDepth(), PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, true, true );
	pStagingScattering->CopyFrom( *m_ppRTScattering[1] );
	pStagingScattering->Save( FILENAME_SCATTERING );
	delete pStagingScattering;

	Texture2D*	pStagingIrradiance = new Texture2D( m_Device, m_ppRTIrradiance[1]->GetWidth(), m_ppRTIrradiance[1]->GetHeight(), 1, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, true, true );
	pStagingIrradiance->CopyFrom( *m_ppRTIrradiance[1] );
	pStagingIrradiance->Save( FILENAME_IRRADIANCE );
	delete pStagingIrradiance;

	Texture2D*	pStagingTransmittance = new Texture2D( m_Device, m_ppRTTransmittance[0]->GetWidth(), m_ppRTTransmittance[0]->GetHeight(), 1, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, true, true );
	pStagingTransmittance->CopyFrom( *m_ppRTTransmittance[0] );
	pStagingTransmittance->Save( FILENAME_TRANSMITTANCE );
	delete pStagingTransmittance;
}
#endif


					// Swap double-buffered slots
					Texture3D*	pTemp0 = m_ppRTScattering[0];
					m_ppRTScattering[0] = m_ppRTScattering[1];
					m_ppRTScattering[1] = pTemp0;

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
