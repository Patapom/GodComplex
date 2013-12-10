#pragma once

template<typename> class CB;

class EffectGlobalIllum : public Scene::ISceneTagger, public Scene::ISceneRenderer
{
private:	// CONSTANTS

	static const U32		CUBE_MAP_SIZE = 128;

protected:	// NESTED TYPES

	struct CBObject
	{
		NjFloat4x4	Local2World;	// Local=>World transform to rotate the object
 	};

	struct CBMaterial
	{
		NjFloat3	DiffuseColor;
		bool		HasDiffuseTexture;
		NjFloat3	SpecularColor;
		bool		HasSpecularTexture;
		float		SpecularExponent;
	};

private:	// FIELDS

	int					m_ErrorCode;
	Device&				m_Device;
	Texture2D&			m_RTTarget;

	Material*			m_pMatRender;			// Displays the room
	Material*			m_pMatRenderCubeMap;	// Renders the room into a cubemap

	// Primitives
	Scene				m_Scene;
	bool				m_bDeleteSceneTags;

	// Textures
	Texture2D*			m_pTexWalls;

	// Constant buffers
 	CB<CBObject>*		m_pCB_Object;
 	CB<CBMaterial>*		m_pCB_Material;




	// Params
public:
	

public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }


public:		// METHODS

	EffectGlobalIllum( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, Camera& _Camera );
	~EffectGlobalIllum();

	void		Render( float _Time, float _DeltaTime );


	// ISceneTagger Implementation
	virtual void*	TagMaterial( const Scene::Material& _Material ) const override;
	virtual void*	TagTexture( const Scene::Material::Texture& _Texture ) const override;
	virtual void*	TagNode( const Scene::Node& _Node ) const override;
	virtual void*	TagPrimitive( const Scene::Mesh& _Mesh, const Scene::Mesh::Primitive& _Primitive ) const override;

	// ISceneRenderer Implementation
	virtual void	RenderMesh( const Scene::Mesh& _Mesh, Material* _pMaterialOverride ) const override;

private:

	void			PreComputeProbes();

};