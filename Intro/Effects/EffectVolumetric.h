#pragma once

#define SHOW_TERRAIN

#define	TRANSMITTANCE_W	256
#define	TRANSMITTANCE_H	64
#define	TRANSMITTANCE_TABLE_STEPS_COUNT	500	// Default amount of integration steps to perform to compute this table

#define	IRRADIANCE_W	64
#define	IRRADIANCE_H	16

#define	RES_3D_ALTITUDE			32
#define	RES_3D_COS_THETA_VIEW	128
#define	RES_3D_COS_THETA_SUN	32
#define	RES_3D_COS_GAMMA		8
#define	RES_3D_U				(RES_3D_COS_THETA_SUN * RES_3D_COS_GAMMA)	// Full texture will be 256*128*32


template<typename> class CB;

class EffectVolumetric
{
private:	// CONSTANTS

	static const int		SHADOW_MAP_SIZE = 512;//256;
	static const int		TERRAIN_SHADOW_MAP_SIZE = 512;

	static const int		FRACTAL_TEXTURE_POT = 7;
	static const int		FRACTAL_OCTAVES = 8;


public:		// NESTED TYPES

	struct CBObject
	{
//		NjFloat4x4	Local2Proj;	// Local=>Proj transform to locate & project the object to the render target
		NjFloat4x4	Local2View;
		NjFloat4x4	View2Proj;
		NjFloat3	dUV;

		// Terrain Parameters
		float		TerrainHeight;
		float		AlbedoMultiplier;
		float		CloudShadowStrength;
		NjFloat2	__PAD;
	};

	struct CBSplat
	{
		NjFloat3	dUV;
	};

	struct CBAtmosphere
	{
		NjFloat3	LightDirection;
		float		SunIntensity;

		NjFloat2	AirParams;		// X=Scattering Factor, Y=Reference Altitude (km)
		float		GodraysStrength;
		float		__PAD;

		NjFloat4	FogParams;		// X=Scattering Coeff, Y=Extinction Coeff, Z=Reference Altitude (km), W=Anisotropy
	};

	struct CBShadow
	{
//		NjFloat4	LightDirection;
		NjFloat4x4	World2Shadow;
		NjFloat4x4	Shadow2World;
		NjFloat4x4	World2TerrainShadow;
		NjFloat2	ZMinMax;
	};

	struct CBVolume 
	{
		// Location & Direct lighting
		NjFloat2	_CloudAltitudeThickness;
		NjFloat2	_CloudExtinctionScattering;
		NjFloat2	_CloudPhases;
		float		_CloudShadowStrength;

		// Isotropic lighting
		float		_CloudIsotropicScattering;
		NjFloat3	_CloudIsotropicFactors;		// X=Sky factor, Y=Sun factor, Z=Terrain reflectance factor
		float		__PAD0;

		// Noise
		NjFloat3	_CloudLoFreqParams;			// X=Frequency Multiplier, Y=Vertical Looping, Z=Animation speed
		float		__PAD1;
		NjFloat4	_CloudHiFreqParams;			// X=Frequency Multiplier, Y=Offset, Z=Factor, W=Animation Speed

		NjFloat3	_CloudOffsets;				// X=Low Altitude Offset, Y=Mid Altitude Offset, Z=High Altitude Offset
		float		__PAD2;

		NjFloat2	_CloudContrastGamma;		// X=Contrast Y=Gamma
		float		_CloudShapingPower;
		float		__PAD3;
	};

	struct	CBPreComputeCS
	{
		U32		_TargetSizeX;	// Final render target size (2D or 3D)
		U32		_TargetSizeY;
		U32		_TargetSizeZ;
		U32		__PAD0;

		U32		_GroupsCountX;	// Amount of render groups (2D or 3D) for a single pass
		U32		_GroupsCountY;
		U32		_GroupsCountZ;
		U32		__PAD1;

		U32		_PassIndexX;	// Index of the X,Y,Z pass (each pass computes THREAD_COUNT_X*THREAD_COUNT_Y*THREAD_COUNT_Z texels)
		U32		_PassIndexY;
		U32		_PassIndexZ;

		U32		_bFirstPass;	// True if we're computing the first pass that reads single-scattering for Rayleigh & Mie from 2 separate tables
		float	_AverageGroundReflectance;

		void	SetTargetSize( U32 _X, U32 _Y, U32 _Z=1 )	{ _TargetSizeX = _X; _TargetSizeY = _Y; _TargetSizeZ = _Z; }
		void	SetGroupsCount( U32 _X, U32 _Y, U32 _Z=1 )	{ _GroupsCountX = _X; _GroupsCountY = _Y; _GroupsCountZ = _Z; }
		void	SetPassIndex( U32 _X, U32 _Y, U32 _Z=0 )	{ _PassIndexX = _X; _PassIndexY = _Y; _PassIndexZ = _Z; }
	};

private:	// FIELDS

	int					m_ErrorCode;
	Device&				m_Device;

	Texture2D&			m_RTHDR;
	Primitive&			m_ScreenQuad;
	Camera&				m_Camera;

	// PRS of our volume box
	NjFloat3			m_Position;
	NjFloat4			m_Rotation;
	NjFloat3			m_Scale;

	NjFloat4x4			m_Cloud2World;
	NjFloat4x4			m_Terrain2World;

	// Light infos
//	NjFloat3			m_LightDirection;

	// Internal Data
	Material*			m_pMatDepthWrite;
	Material*			m_pMatSplatCameraFrustum;
	Material*			m_pMatComputeTransmittance;
	Material*			m_pMatDisplay;
	Material*			m_pMatCombine;

	Primitive*			m_pPrimBox;
	Primitive*			m_pPrimFrustum;
#ifdef SHOW_TERRAIN
	Primitive*			m_pPrimTerrain;
	Texture2D*			m_pRTTerrainShadow;
	Material*			m_pMatTerrainShadow;
	Material*			m_pMatTerrain;
#endif

	Texture3D*			m_pTexFractal0;
	Texture3D*			m_pTexFractal1;
	Texture2D*			m_pRTCameraFrustumSplat;
	Texture2D*			m_pRTTransmittanceZ;
	Texture2D*			m_pRTTransmittanceMap;
	Texture2D*			m_pRTRenderZ;
	Texture2D*			m_pRTRender;

	// Sky rendering
	Texture2D*			m_ppRTTransmittance[2];
	Texture2D*			m_ppRTIrradiance[3];
	Texture3D*			m_ppRTInScattering[3];

	int					m_RenderWidth, m_RenderHeight;

	CB<CBObject>*		m_pCB_Object;
	CB<CBSplat>*		m_pCB_Splat;
	CB<CBAtmosphere>*	m_pCB_Atmosphere;
	CB<CBShadow>*		m_pCB_Shadow;
	CB<CBVolume>*		m_pCB_Volume;
	CB<CBPreComputeCS>*	m_pCB_PreComputeSky;

	NjFloat4x4			m_World2Light;
	NjFloat4x4			m_Light2ShadowNormalized;	// Yields a normalized Z instead of world units like World2Shadow

	NjFloat3			m_ShadowPlaneCenterKm;
	NjFloat3			m_ShadowPlaneNormal;
	NjFloat3			m_ShadowPlaneX;
	NjFloat3			m_ShadowPlaneY;
	NjFloat2			m_ShadowPlaneOffsetKm;

	int					m_ViewportWidth;
	int					m_ViewportHeight;

	// Atmosphere Pre-Computation
	NjFloat3*			m_pTableTransmittance;



#ifdef _DEBUG
	struct ParametersBlock
	{
		U32		Checksum;

		// Atmosphere Params
		float	SunTheta;
		float	SunPhi;
		float	SunIntensity;
		float	AirAmount;		// Simply a multiplier of the default value
		float	FogScattering;
		float	FogExtinction;
		float	AirReferenceAltitudeKm;
		float	FogReferenceAltitudeKm;
		float	FogAnisotropy;
		float	AverageGroundReflectance;
		float	GodraysStrength;

		// Volumetrics Params
		float	CloudBaseAltitude;
		float	CloudThickness;
		float	CloudExtinction;
		float	CloudScattering;
		float	CloudAnisotropyIso;
		float	CloudAnisotropyForward;
		float	CloudShadowStrength;

		float	CloudIsotropicScattering;	// Sigma_s for isotropic lighting
		float	CloudIsoSkyRadianceFactor;
		float	CloudIsoSunRadianceFactor;
		float	CloudIsoTerrainReflectanceFactor;

		// Noise Params
			// Low frequency noise
		float	NoiseLoFrequency;		// Horizontal frequency
		float	NoiseLoVerticalLooping;	// Vertical frequency in amount of noise pixels
		float	NoiseLoAnimSpeed;		// Animation speed
			// High frequency noise
		float	NoiseHiFrequency;
		float	NoiseHiOffset;			// Second noise is added to first noise using NoiseHiStrength * (HiFreqNoise + NoiseHiOffset)
		float	NoiseHiStrength;
		float	NoiseHiAnimSpeed;
			// Combined noise params
		float	NoiseOffsetBottom;		// The noise offset to add when at the bottom altitude in the cloud
		float	NoiseOffsetMiddle;		// The noise offset to add when at the middle altitude in the cloud
		float	NoiseOffsetTop;			// The noise offset to add when at the top altitude in the cloud
		float	NoiseContrast;			// Final noise value is Noise' = pow( Contrast*(Noise+Offset), Gamma )
		float	NoiseGamma;
			// Final shaping params
		float	NoiseShapingPower;		// Final noise value is shaped (multiplied) by pow( 1-abs(2*y-1), NoiseShapingPower ) to avoid flat plateaus at top or bottom

		// Terrain Params
		float	TerrainHeight;
		float	TerrainAlbedoMultiplier;
		float	TerrainCloudShadowStrength;
	};

	// Memory-Mapped File for tweaking
	MMF<ParametersBlock>*	m_pMMF;

#endif


public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }

public:		// METHODS

	EffectVolumetric( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, Camera& _Camera );
	~EffectVolumetric();

	void		Render( float _Time, float _DeltaTime );

protected:

	// Sky tables computation
	void		PreComputeSkyTables();
	void		FreeSkyTables();

		// Time-sliced update
	void		InitUpdateSkyTables();
	void		ExitUpdateSkyTables();
	void		TriggerSkyTablesUpdate();
	void		UpdateSkyTables();

	void		InitStage( int _StageIndex );
	bool		IncreaseStagePass( int _StageIndex );	// Returns true if the stage is over


	// Tables Pre-computation
	void		BuildTransmittanceTable( int _Width, int _Height, Texture2D& _StagingTexture );

	float		ComputeOpticalDepth( float _AltitudeKm, float _CosTheta, const float _Href, bool& _bGroundHit, int _StepsCount=TRANSMITTANCE_TABLE_STEPS_COUNT ) const;
	NjFloat3	GetTransmittance( float _AltitudeKm, float _CosTheta ) const;
	NjFloat3	GetTransmittance( float _AltitudeKm, float _CosTheta, float _DistanceKm ) const;
	NjFloat3	SampleTransmittance( const NjFloat2 _UV ) const;

	// Sphere-tracing
	void		ComputeSphericalData( const NjFloat3& _PositionKm, float& _AltitudeKm, NjFloat3& _Normal ) const;
	float		SphereIntersectionEnter( const NjFloat3& _PositionKm, const NjFloat3& _View, float _SphereAltitudeKm ) const;
	float		SphereIntersectionExit( const NjFloat3& _PositionKm, const NjFloat3& _View, float _SphereAltitudeKm ) const;
	void		SphereIntersections( const NjFloat3& _PositionKm, const NjFloat3& _View, float _SphereAltitudeKm, NjFloat2& _Hits ) const;
	float		ComputeNearestHit( const NjFloat3& _PositionKm, const NjFloat3& _View, float _SphereAltitudeKm, bool& _IsGround ) const;

	// Shadow computation
	void		ComputeShadowTransform();
	NjFloat3	Project2ShadowPlane( const NjFloat3& _PositionKm, float& Distance2PlaneKm );
	NjFloat2	World2ShadowQuad( const NjFloat3& _PositionKm, float& Distance2PlaneKm );
	NjFloat3	FindTangent( NjFloat4x4& _Camera2World, float _TanFovV );
	void		ComputeFrustumIntersection( NjFloat3 _pCameraFrustumKm[5], float _PlaneHeight, NjFloat2& _QuadMin, NjFloat2& _QuadMax );

	NjFloat4x4	ComputeTerrainShadowTransform();

	Texture3D*	BuildFractalTexture( bool _bLoadFirst );
};