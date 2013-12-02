#pragma once

template<typename> class CB;

class EffectGlobalIllum : public Scene::ISceneTagger
{
private:	// CONSTANTS

protected:	// NESTED TYPES

	struct CBObject
	{
		NjFloat4x4	Local2World;	// Local=>World transform to rotate the object

		NjFloat4	LightColor0;
		NjFloat4	LightColor1;
		NjFloat4	LightColor2;
		NjFloat4	LightColor3;
 	};

	struct MaterialDescriptor 
	{
		int			LightSourceIndex;	// -1 For standard reflective materials
		NjFloat3	Color;				// Either the diffuse albedo or the emissive power depending on the emissive flag
	};

private:	// FIELDS

	int					m_ErrorCode;
	Device&				m_Device;
	Texture2D&			m_RTTarget;

	Material*			m_pMatDisplay;			// Displays the room
	Material*			m_pMatDisplayEmissive;	// Displays the lights

	// Primitives
	Scene				m_Scene;
	Primitive*			m_pPrimRoom;
	Primitive*			m_pPrimRoomLights;

	// Textures
	Texture2D*			m_pTexWalls;			// The fat texture for the walls
	Texture2D*			m_pTexLightMaps;		// The array texture that will contain the room's light map

public:

	// Constant buffers
 	CB<CBObject>*		m_pCB_Object;


	// Params
public:
	
public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }


public:		// METHODS

	EffectGlobalIllum( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, Camera& _Camera );
	~EffectGlobalIllum();

	void		Render( float _Time, float _DeltaTime );
	void		InitScene();


	// ISceneTagger Implementation
	virtual void*	TagMaterial( const Scene::Material& _Material ) const override;
	virtual void*	TagNode( const Scene::Node& _Node ) const override;
	virtual void*	TagPrimitive( const Scene::Mesh& _Mesh, const Scene::Mesh::Primitive& _Primitive ) const override;

protected:

};