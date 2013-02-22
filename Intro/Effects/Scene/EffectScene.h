//////////////////////////////////////////////////////////////////////////
// This is the main scene renderer that fills the G-Buffer with complex materials
//	and also renders the double-depth pass
#pragma once

template<typename> class CB;
class Scene;

class EffectScene
{
private:	// CONSTANTS

public:		// NESTED TYPES

	struct CBRender
	{
		NjFloat3	dUV;
		float		__PAD;
		NjFloat2	DeltaTime;
	};


private:	// FIELDS

	int					m_ErrorCode;

	Material*			m_pMatDepthPass;
	Material*			m_pMatBuildLinearZ;
	Material*			m_pMatFillGBuffer;
	Material*			m_pMatShading;

	Texture2D*			m_pDepthStencilFront;
	Texture2D*			m_pDepthStencilBack;
	Texture2D*			m_pRTZBuffer;	// Front & Back ZBuffers stored in linear space in a RG32F target

	Texture2D*			m_pRTGBuffer0_2;
	Texture2D*			m_pRTGBuffer3;

public:

	Device&				m_Device;
	Scene&				m_Scene;

	CB<CBRender>*		m_pCB_Render;
	Primitive&			m_ScreenQuad;

public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }

public:		// METHODS

	EffectScene( Device& _Device, Scene& _Scene, Primitive& _ScreenQuad );
	~EffectScene();

	void	Render( float _Time, float _DeltaTime, Texture2D* _pTex );

};