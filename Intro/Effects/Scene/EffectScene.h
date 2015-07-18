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
		float3	dUV;
	};

	struct CBLight
	{
		float3	Position;
		float		__PAD0;
		float3	Direction;
		float		__PAD1;
		float3	Radiance;
		float		__PAD2;
		float4	Data;
	};


private:	// FIELDS

	Device&				m_Device;
	Scene&				m_Scene;

	int					m_ErrorCode;

	Shader*			m_pMatDepthPass;
	Shader*			m_pMatBuildLinearZ;
	Shader*			m_pMatFillGBuffer;
	Shader*			m_pMatFillGBufferBackFaces;
	Shader*			m_pMatDownSample;
	Shader*			m_pMatShading_Directional_StencilPass;
	Shader*			m_pMatShading_Directional;
	Shader*			m_pMatShading_Point_StencilPass;
	Shader*			m_pMatShading_Point;
	Shader*			m_pMatShading_Spot_StencilPass;
	Shader*			m_pMatShading_Spot;
	Shader*			m_pMatIndirectLighting;
	Shader*			m_pMatBokehSplat;
	Shader*			m_pMatDownSampleBokeh0;
	Shader*			m_pMatDownSampleBokeh1;
	Shader*			m_pMatFinalize;

	Texture2D*			m_pDepthStencilFront;
	Texture2D*			m_pDepthStencilBack;
	Texture2D*			m_pRTZBuffer;	// Front & Back ZBuffers stored in linear space in a RG32F target

	// Front & back GBuffers
	Texture2D*			m_pRTGBuffer0_2;
	Texture2D*			m_pRTGBuffer3;
	Texture2D*			m_pRTGBufferBack;

	Texture2D*			m_pRTAccumulatorDiffuseSpecular;

	Texture2D*			m_pTexBokeh;

	Primitive*			m_pPrimCylinder;
	Primitive*			m_pPrimSphere;
	Primitive&			m_ScreenQuad;
	Primitive*			m_pPrimBokeh;

	CB<CBRender>*		m_pCB_Render;
	CB<CBRender>*		m_pCB_RenderDownSampled;
	CB<CBLight>*		m_pCB_Light;

public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }

public:		// METHODS

	EffectScene( Device& _Device, Scene& _Scene, Primitive& _ScreenQuad );
	~EffectScene();

	void	Render( float _Time, float _DeltaTime, Texture2D& _RTHDR );

};