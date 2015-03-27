#pragma once

#define SUN_INTENSITY	200.0f
#define SKY_INTENSITY	(0.025f*SUN_INTENSITY)

template<typename> class CB;

class EffectGlobalIllum2 : public Scene::ISceneTagger, public Scene::ISceneRenderer, public SHProbeNetwork::DynamicUpdateParms::IQueryMaterial
{
private:	// CONSTANTS

	static const U32		MAX_SCENE_PRIMITIVES = 1024;		// We handle a maximum of 1024 scene primitives. That's not because the tech is limited but simply because I don't have a dynamic list class! ^^

	static const U32		MAX_LIGHTS = 64;

	static const U32		MAX_DYNAMIC_OBJECTS = 128;

	static const U32		SHADOW_MAP_SIZE = 1024;
	static const U32		SHADOW_MAP_POINT_SIZE = 256;		// Point light shadow map


protected:	// NESTED TYPES
	
#pragma pack( push, 4 )

	struct CBGeneral {
		float3		Ambient;
		U32			ShowIndirect;
		U32			ShowOnlyIndirect;
		U32			ShowWhiteDiffuse;
		U32			ShowVertexProbeID;
 	};

	struct CBScene {
		U32			StaticLightsCount;
		U32			DynamicLightsCount;
		U32			ProbesCount;
 	};

	struct CBObject {
		float4x4	Local2World;	// Local=>World transform to rotate the object
 	};

	struct CBObjectVoronoi {
		float4		Color;
	};

	struct CBSplat {
		float3	dUV;
	};

	struct CBDynamicObject {
		float3		Position;
		U32			ProbeID;
 	};

	struct CBMaterial {
		U32			ID;
		float3		DiffuseAlbedo;

		U32			HasDiffuseTexture;
		float3		SpecularAlbedo;

		U32			HasSpecularTexture;
		float3		EmissiveColor;

		float		SpecularExponent;
		U32			FaceOffset;		// The offset to apply to the object's face index to obtain an absolute face index
		U32			HasNormalTexture;
	};

	struct CBShadowMap {
		float4x4	Light2World;
		float4x4	World2Light;
		float3		BoundsMin;					// Coordinates of the bounding box (in world space) covered by the shadow
		float		__PAD0;
		float3		BoundsMax;
 	};

	struct CBShadowMapPoint {
		float3		Position;					// Position of the light in world space
		float		FarClipDistance;
	};

	// Structured Buffers
	// Light buffer
	struct	LightStruct
	{
		Scene::Light::LIGHT_TYPE	Type;
		float3		Position;
		float3		Direction;
		float3		Color;
		float4		Parms;						// X=Falloff radius, Y=Cutoff radius, Z=Cos(Falloff angle), W=Cos(Cutoff angle)
	};

#pragma pack( pop )

	// Functors for using probe network
	class	RenderScene : public SHProbeNetwork::IRenderSceneDelegate {
	public:	EffectGlobalIllum2&	m_this;
		RenderScene( EffectGlobalIllum2& _this ) : m_this( _this ) {}
		void	operator()( Material& _Material ) {
			for ( int MeshIndex=0; MeshIndex < m_this.m_Scene.m_MeshesCount; MeshIndex++ )
				m_this.RenderMesh( *m_this.m_ppCachedMeshes[MeshIndex], &_Material, true );
		}
	};


protected:

	struct	DynamicObject
	{
		float3		PositionStart;
		float3		PositionEnd;
		float		Interpolation;
	};


private:	// FIELDS

	int					m_ErrorCode;
	Device&				m_Device;
	Texture2D&			m_RTTarget;
	Primitive&			m_ScreenQuad;

	Material*			m_pMatRender;					// Displays the scene
	Material*			m_pMatRenderEmissive;			// Displays the scene's emissive objects (area lights)
	Material*			m_pMatRenderLights;				// Displays the lights as small emissive balls
	Material*			m_pMatRenderDynamic;			// Displays the dynamic objects as balls with a normal map
	Material*			m_pCSComputeShadowMapBounds;	// Computes the shadow map bounds
	Material*			m_pMatRenderShadowMap;			// Renders the directional shadowmap
	Material*			m_pMatRenderShadowMapPoint;		// Renders the point light shadowmap
	Material*			m_pMatPostProcess;				// Post-processes the result
	Material*			m_pMatRenderDebugProbes;		// Displays the probes as small spheres
	Material*			m_pMatRenderDebugProbesNetwork;	// Displays the probes network
	Material*			m_pMatRenderDebugProbeVoronoi;	// Displays the probe Voronoï cells

	// Scene & Primitives
	Scene				m_Scene;
	float3				m_SceneBBoxMin;
	float3				m_SceneBBoxMax;
	CompositeVertexFormatDescriptor	m_SceneVertexFormatDesc;
	bool				m_bDeleteSceneTags;
	Primitive*			m_pPrimSphere;
	Primitive*			m_pPrimPoint;

	U32					m_DebugVoronoiCellIndex;
	Primitive*			m_pPrimVoronoiCellPlanes;
	Primitive*			m_pPrimVoronoiCellEdges;

		// Cached list of meshes
	Scene::Mesh**		m_ppCachedMeshes;

		// Cached list of materials
	int					m_EmissiveMaterialsCount;
	Scene::Material*	m_ppEmissiveMaterials[100];

		// Face offsets to add to each primitive's face indices to obtain an absolute unique face index
	U32					m_TotalVerticesCount;
	U32					m_TotalFacesCount;
	U32					m_TotalPrimitivesCount;
	U32					m_pPrimitiveFaceOffset[MAX_SCENE_PRIMITIVES];
	U32					m_pPrimitiveVertexOffset[MAX_SCENE_PRIMITIVES];

		// Dynamic objects
	DynamicObject		m_pDynamicObjects[MAX_DYNAMIC_OBJECTS];


	// Textures
	int					m_TexturesCount;
	Texture2D**			m_ppTextures;
	Texture2D*			m_pTexDynamicNormalMap;
	Texture2D*			m_pRTShadowMap;
	Texture2D*			m_pRTShadowMapPoint;

	// Constant buffers
 	CB<CBGeneral>*			m_pCB_General;
 	CB<CBScene>*			m_pCB_Scene;
 	CB<CBObject>*			m_pCB_Object;
	CB<CBObjectVoronoi>*	m_pCB_ObjectVoronoi;
	CB<CBSplat>*			m_pCB_Splat;
	CB<CBDynamicObject>*	m_pCB_DynamicObject;
 	CB<CBMaterial>*			m_pCB_Material;
 	CB<CBShadowMap>*		m_pCB_ShadowMap;
 	CB<CBShadowMapPoint>*	m_pCB_ShadowMapPoint;

	// Runtime scene lights
	SB<LightStruct>*	m_pSB_LightsStatic;
	SB<LightStruct>*	m_pSB_LightsDynamic;


	// Ambient SH computed from CIE overcast sky model
	float3				m_pSHAmbientSky[9];

	// Probes network
	SHProbeNetwork		m_ProbesNetwork;


#ifdef _DEBUG
	struct ParametersBlock
	{
		U32		Checksum;

		// Atmosphere Params
		U32		EnableSun;
		float	SunTheta;
		float	SunPhi;
		float	SunIntensity;

		U32		EnableSky;
		float	SkyIntensity;
		float	SkyColorR;
		float	SkyColorG;
		float	SkyColorB;

		// Dynamic lights params
		U32		EnablePointLight;
		U32		AnimatePointLight;
		float	PointLightIntensity;
		float	PointLightColorR;
		float	PointLightColorG;
		float	PointLightColorB;

		// Static lighting params
		U32		EnableStaticLighting;

		// Emissive params
		U32		EnableEmissiveMaterials;
		float	EmissiveIntensity;
		float	EmissiveColorR;
		float	EmissiveColorG;
		float	EmissiveColorB;

		// Dynamic objects
		U32		DynamicObjectsCount;

		// Bounce params
		float	BounceFactorSun;
		float	BounceFactorSky;
		float	BounceFactorPoint;
		float	BounceFactorStaticLights;
		float	BounceFactorEmissive;

		// Neighborhood
		U32		EnableNeighborsRedistribution;
		float	NeighborProbesContributionBoost;

		// Probes Update
		U32		MaxProbeUpdatesPerFrame;

		// Misc
		U32		ShowDebugProbes;
		U32		ShowDebugProbesNetwork;
		float	DebugProbesIntensity;
		U32		ShowDebugProbeInfluences;
		U32		ShowDebugProbeVoronoiCell;
		U32		ProbeVoronoiCellIndex;
	};

	// Memory-Mapped File for tweaking
	MMF<ParametersBlock>*	m_pMMF;
	ParametersBlock			m_CachedCopy;	// Latest cached copy of the update parms

#endif


public:		// PROPERTIES

	int				GetErrorCode() const	{ return m_ErrorCode; }


public:		// METHODS

	EffectGlobalIllum2( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, FPSCamera& _Camera );
	~EffectGlobalIllum2();

	void			Render( float _Time, float _DeltaTime );


	// ISceneTagger Implementation
	virtual void*	TagMaterial( const Scene& _Owner, Scene::Material& _Material ) override;
	virtual void*	TagTexture( const Scene& _Owner, Scene::Material::Texture& _Texture ) override;
	virtual void*	TagNode( const Scene& _Owner, Scene::Node& _Node ) override;
	virtual void*	TagPrimitive( const Scene& _Owner, Scene::Mesh& _Mesh, Scene::Mesh::Primitive& _Primitive ) override;

	// ISceneRenderer Implementation
	virtual void	RenderMesh( const Scene::Mesh& _Mesh, Material* _pMaterialOverride, bool _SetMaterial ) override;

private:

	void			RenderShadowMap( const float3& _SunDirection );
	void			RenderShadowMapPoint( const float3& _Position, float _FarClipDistance );

	void			BuildVoronoiPrimitives();

	// SHProbeNetwork::DynamicUpdateParms::IQueryMaterial Implementation
	Scene::Material*	operator()( U32 _MaterialID ) {
		ASSERT( _MaterialID < U32(m_Scene.m_MaterialsCount), "Material ID out of range!" );
		return m_Scene.m_ppMaterials[_MaterialID];
	}
};