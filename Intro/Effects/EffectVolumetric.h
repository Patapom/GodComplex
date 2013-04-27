#pragma once

#define SHOW_TERRAIN

template<typename> class CB;

class EffectVolumetric
{
private:	// CONSTANTS

	static const int	SHADOW_MAP_SIZE = 256;
	static const int	FRACTAL_TEXTURE_POT = 7;
	static const int	FRACTAL_OCTAVES = 8;


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

	NjFloat4x4			m_Box2World;

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


public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }

public:		// METHODS

	EffectVolumetric( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, Camera& _Camera );
	~EffectVolumetric();

	void		Render( float _Time, float _DeltaTime );

protected:

	void		PreComputeSkyTables();
	void		FreeSkyTables();

	void		ComputeShadowTransform();
	NjFloat3	Project2ShadowPlane( const NjFloat3& _PositionKm, float& Distance2PlaneKm );
	NjFloat2	World2ShadowQuad( const NjFloat3& _PositionKm, float& Distance2PlaneKm );
	NjFloat3	FindTangent( NjFloat4x4& _Camera2World, float _TanFovV );
	void		ComputeFrustumIntersection( NjFloat3 _pCameraFrustumKm[5], float _PlaneHeight, NjFloat2& _QuadMin, NjFloat2& _QuadMax );

	Texture3D*	BuildFractalTexture( bool _bLoadFirst );
};