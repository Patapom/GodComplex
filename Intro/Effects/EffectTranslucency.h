#pragma once

template<typename> class CB;

class EffectTranslucency
{
private:	// CONSTANTS

	static const int	ZBUFFER_SIZE = 256;


public:		// NESTED TYPES

	struct CBObject
	{
		NjFloat4x4	Local2World;	// Local=>World transform to rotate the object
	};


private:	// FIELDS

	int					m_ErrorCode;

	Material*			m_pMatDisplay;		// Some test material for primitive display
	Material*			m_pMatBuildZBuffer;	// Renders the internal & exteral objects into a single RGBA16F linear ZBuffer

	Primitive*			m_pPrimSphereInternal;
	Primitive*			m_pPrimSphereExternal;

	Texture2D*			m_pRTZBuffer;
	Texture2D*			m_pDepthStencil;	// The depth stencil adapted to the ZBuffers rendering

	CB<CBObject>*		m_pCB_Object;


public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }

	Texture2D*	GetZBuffer()			{ return m_pRTZBuffer; }

public:		// METHODS

	EffectTranslucency();
	~EffectTranslucency();

	void	Render( float _Time, float _DeltaTime );

};