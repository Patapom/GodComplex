#pragma once

template<typename> class CB;

class EffectVolumetric
{
private:	// CONSTANTS

	static const int	SHADOW_MAP_SIZE = 512;
	static const int	FRACTAL_TEXTURE_POT = 7;
	static const int	FRACTAL_OCTAVES = 8;
	static const float	SCREEN_TARGET_RATIO;


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
		NjFloat2	ZMax;
	};

	struct CBVolume 
	{
		NjFloat4	Params;
	};

private:	// FIELDS

	int					m_ErrorCode;
	Device&				m_Device;
	Primitive&			m_ScreenQuad;

	// PRS of our volume box
	NjFloat3			m_Position;
	NjFloat4			m_Rotation;
	NjFloat3			m_Scale;

	NjFloat4x4			m_Box2World;

	// Light infos
	NjFloat3			m_LightDirection;

	// Internal Data
	Material*			m_pMatDepthWrite;
	Material*			m_pMatComputeTransmittance;
	Material*			m_pMatDisplay;
	Material*			m_pMatCombine;

	Primitive*			m_pPrimBox;

	Texture3D*			m_pTexFractal0;
	Texture3D*			m_pTexFractal1;
	Texture2D*			m_pRTTransmittanceZ;
	Texture2D*			m_pRTTransmittanceMap;
	Texture2D*			m_pRTRenderZ;
	Texture2D*			m_pRTRender;

	int					m_RenderWidth, m_RenderHeight;

	CB<CBObject>*		m_pCB_Object;
	CB<CBSplat>*		m_pCB_Splat;
	CB<CBShadow>*		m_pCB_Shadow;
	CB<CBVolume>*		m_pCB_Volume;

	NjFloat4x4			m_World2Light;
	NjFloat4x4			m_Light2ShadowNormalized;	// Yields a normalized Z instead of world units like World2Shadow


public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }

public:		// METHODS

	EffectVolumetric( Device& _Device, Primitive& _ScreenQuad );
	~EffectVolumetric();

	void		Render( float _Time, float _DeltaTime, Camera& _Camera );

protected:

	void		ComputeShadowTransform();
	Texture3D*	BuildFractalTexture( bool _bLoadFirst );
};