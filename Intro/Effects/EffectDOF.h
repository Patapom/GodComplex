#pragma once

#define SHADERTOY

#define DOF_NB_SAMPLE		8
#define DOF_OFFSET_COUNT	(DOF_NB_SAMPLE * 2)
#define DOF_WEIGHT_COUNT	(DOF_NB_SAMPLE * 4)

template<typename> class CB;

class EffectDOF
{
private:	// CONSTANTS

	static const int		SHADOW_MAP_SIZE = 1024;

protected:	// NESTED TYPES

#pragma pack( push, 4 )

	struct CBGeneral
	{
		U32			ShowIndirect;
 	};

	struct CBScene
	{
		U32			StaticLightsCount;
		U32			DynamicLightsCount;
		U32			ProbesCount;
 	};

	struct CBObject
	{
		float4x4	Local2World;	// Local=>World transform to rotate the object
 	};

	struct CBMaterial
	{
		U32			ID;
		float3	DiffuseAlbedo;

		U32			HasDiffuseTexture;
		float3	SpecularAlbedo;

		U32			HasSpecularTexture;
		float3	EmissiveColor;

		float		SpecularExponent;
		U32			FaceOffset;		// The offset to apply to the object's face index to obtain an absolute face index
	};

	struct CBSplat
	{
		float3	dUV;
		float		__PAD;
		float4	Offsets[4];
		float4	Weights[8];
	};

#pragma  pack( pop )

	enum DOWNSAMPLE_TYPE
	{
		MIN,
		MAX,
		AVG
	};

private:	// FIELDS

	int					m_ErrorCode;
	Device&				m_Device;
	Texture2D&			m_RTTarget;
	Primitive&			m_ScreenQuad;

	Material*			m_pMatRender;				// Renders the scene
	Material*			m_pMatRenderCube;			// Renders the gloubi cube
	Material*			m_pMatDownsampleMin;		// Downsample color scene
	Material*			m_pMatDownsampleMax;		// Downsample color scene
	Material*			m_pMatDownsampleAvg;		// Downsample color scene
	Material*			m_pMatComputeFuzzinessNear;	// Compute near field fuzziness
	Material*			m_pMatComputeFuzzinessFar;	// Compute far field fuzziness
	Material*			m_pMatComputeFuzzinessBlur;	// Blurs near field fuzziness
	Material*			m_pMatDOFNear;				// Compute near field DOF
	Material*			m_pMatDOFFar;				// Compute far field DOF
	Material*			m_pMatDOFCombine;			// Combine DOF & scene


	Material*			m_pMatShadertoy;

	// Primitives
	Scene				m_Scene;
	bool				m_bDeleteSceneTags;
	Primitive*			m_pPrimCube;

	// Textures & RTs
	Texture2D*			m_pTexWalls;			// Wall albedo
	Texture2D*			m_pRTShadowMap;

	Texture2D*			m_pRTDOFMask;
	Texture2D*			m_pRTDepthAlphaBuffer;
	Texture2D*			m_pRTDOF;
	Texture2D*			m_pRTDownsampledHDRTarget;

	Texture2D*			m_pRTTemp;

	// Constant buffers
 	CB<CBGeneral>*		m_pCB_General;
 	CB<CBScene>*		m_pCB_Scene;
 	CB<CBObject>*		m_pCB_Object;
 	CB<CBMaterial>*		m_pCB_Material;

	CB<CBSplat>*		m_pCB_Splat;


public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }


public:		// METHODS

	EffectDOF( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, Camera& _Camera );
	~EffectDOF();

	void		Render( float _Time, float _DeltaTime );


// 	// ISceneTagger Implementation
// 	virtual void*	TagMaterial( const Scene& _Owner, const Scene::Material& _Material ) override;
// 	virtual void*	TagTexture( const Scene& _Owner, const Scene::Material::Texture& _Texture ) override;
// 	virtual void*	TagNode( const Scene& _Owner, const Scene::Node& _Node ) override;
// 	virtual void*	TagPrimitive( const Scene& _Owner, const Scene::Mesh& _Mesh, const Scene::Mesh::Primitive& _Primitive ) override;
// 
// 	// ISceneRenderer Implementation
// 	virtual void	RenderMesh( const Scene::Mesh& _Mesh, Material* _pMaterialOverride ) override;

protected:

	void			Downsample( int _TargetWidth, int _TargetHeight, const ID3D11ShaderResourceView& _Source, const ID3D11RenderTargetView& _Target, DOWNSAMPLE_TYPE _Type ) const;
	void			RenderFuzziness() const;
	void			RenderDOF() const;

	void			ComputeKernel( float* _Offsets, float* _Weights, const float _weights[4], bool vertical ) const;
	void			ScaleKernel( float* _Offsets, int _Width, int _Height ) const;
};