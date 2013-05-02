#pragma once

#define SHOW_TERRAIN

template<typename> class CB;

class EffectVolumetric
{
private:	// CONSTANTS

	static const int		SHADOW_MAP_SIZE = 512;//256;
	static const int		TERRAIN_SHADOW_MAP_SIZE = 512;

	static const int		FRACTAL_TEXTURE_POT = 7;
	static const int		FRACTAL_OCTAVES = 8;

	static const int		TRANSMITTANCE_W = 256;
	static const int		TRANSMITTANCE_H = 64;
	static const int		TRANSMITTANCE_TABLE_STEPS_COUNT = 500;	// Default amount of integration steps to perform to compute this table

	static const int		IRRADIANCE_W = 64;
	static const int		IRRADIANCE_H = 16;

	static const int		RES_R	 = 32;
	static const int		RES_MU	 = 128;
	static const int		RES_MU_S = 32;
	static const int		RES_NU	 = 8;


public:		// NESTED TYPES

	struct CBObject
	{
//		NjFloat4x4	Local2Proj;	// Local=>Proj transform to locate & project the object to the render target
		NjFloat4x4	Local2View;
		NjFloat4x4	View2Proj;
		NjFloat3	dUV;
	};

	struct CBSplat
	{
		NjFloat3	dUV;
	};

	struct CBShadow
	{
		NjFloat4	LightDirection;
		NjFloat4x4	World2Shadow;
		NjFloat4x4	Shadow2World;
		NjFloat4x4	World2TerrainShadow;
		NjFloat2	ZMinMax;
	};

	struct CBVolume 
	{
		NjFloat4	Params;
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
	NjFloat3			m_LightDirection;

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
	Texture2D*			m_pRTTransmittance;
	Texture2D*			m_pRTIrradiance;
	Texture3D*			m_pRTInScattering;

	int					m_RenderWidth, m_RenderHeight;

	CB<CBObject>*		m_pCB_Object;
	CB<CBSplat>*		m_pCB_Splat;
	CB<CBShadow>*		m_pCB_Shadow;
	CB<CBVolume>*		m_pCB_Volume;

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