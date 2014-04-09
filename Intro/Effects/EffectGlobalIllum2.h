#pragma once

#define SUN_INTENSITY	200.0f
#define SKY_INTENSITY	(0.025f*SUN_INTENSITY)

template<typename> class CB;

class EffectGlobalIllum2 : public Scene::ISceneTagger, public Scene::ISceneRenderer
{
private:	// CONSTANTS

	static const U32		MAX_SCENE_PRIMITIVES = 1024;		// We handle a maximum of 1024 scene primitives. That's not because the tech is limited but simply because I don't have a dynamic list class! ^^

	static const U32		CUBE_MAP_SIZE = 128;
	static const U32		MAX_NEIGHBOR_PROBES = 32;

	static const U32		MAX_LIGHTS = 64;
	static const U32		MAX_PROBE_SETS = 16;
	static const U32		MAX_PROBE_NEIGHBORS = 4;			// Only keep the 4 most significant neighbors
	static const U32		MAX_PROBE_EMISSIVE_SETS = 16;
	static const U32		MAX_SET_SAMPLES = 64;				// Accept a maximum of 64 samples per set

	static const U32		MAX_PROBE_UPDATES_PER_FRAME = 32;	// Update a maximum of 32 probes per frame

	static const U32		MAX_DYNAMIC_OBJECTS = 128;

	static const U32		SHADOW_MAP_SIZE = 1024;
	static const U32		SHADOW_MAP_POINT_SIZE = 256;		// Point light shadow map


protected:	// NESTED TYPES
	
#pragma pack( push, 4 )

	struct CBGeneral
	{
		float3		Ambient;
		U32			ShowIndirect;
		U32			ShowOnlyIndirect;
		U32			ShowWhiteDiffuse;
		U32			ShowVertexProbeID;
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

	struct CBDynamicObject
	{
		float3		Position;
		U32			ProbeID;
 	};

	struct CBMaterial
	{
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

	struct CBProbe
	{
		float3		CurrentProbePosition;
		U32			NeighborProbeID;
		float3		NeighborProbePosition;
 	};

	struct CBSplat
	{
		float3	dUV;
	};

	struct CBShadowMap
	{
		float4x4	Light2World;
		float4x4	World2Light;
		float3		BoundsMin;					// Coordinates of the bounding box (in world space) covered by the shadow
		float		__PAD0;
		float3		BoundsMax;
 	};

	struct CBShadowMapPoint
	{
		float3		Position;					// Position of the light in world space
		float		FarClipDistance;
	};

	struct CBUpdateProbes
	{
		float4		AmbientSH[9];				// Ambient sky (padded!)
//		float		SunBoost;	// Merged into the last float4

		float		SkyBoost;
		float		DynamicLightsBoost;
		float		StaticLightingBoost;

		float		EmissiveBoost;
		float		NeighborProbesContributionBoost;
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

	// Runtime probes buffer that we'll use to light objects
	struct RuntimeProbe 
	{
		float3		Position;
		float		Radius;
		float3		pSH[9];

		// Neighbor probes
		U16			NeighborProbeIDs[4];			// IDs of the 4 most significant neighbors
	};

	// Probes update buffers
	struct RuntimeProbeUpdateInfos
	{
		U32			Index;							// The index of the probe we're updating
		U32			SetsStart;						// Index of the first set for the probe
		U32			SetsCount;						// Amount of sets for the probe
		U32			EmissiveSetsStart;				// Index of the first emissive set for the probe
		U32			EmissiveSetsCount;				// Amount of emissive sets for the probe
		U32			SamplingPointsStart;			// Index of the first sampling point for the probe
		U32			SamplingPointsCount;			// Amount of sampling points for the probe
		float3		SHStatic[9];					// Precomputed static SH (static geometry + static lights)
		float		SHOcclusion[9];					// Directional ambient occlusion for the probe

		// Neighbor probes
//		U32			NeighborProbeIDs[4];			// The IDs of the 4 most significant neighbor probes
		float4		NeighborProbeSH[9];				// The SH coefficients to convolve the neighbor's SH with to obtain their contribution to this probe
	};

	struct	RuntimeProbeUpdateSetInfos
	{
		U32			SamplingPointsStart;			// Index of the first sampling point
		U32			SamplingPointsCount;			// Amount of sampling points
		float3		SH[9];							// SH for the set
	};

	struct	RuntimeProbeUpdateEmissiveSetInfos
	{
		float3		EmissiveColor;					// Color of the emissive material
		float		SH[9];							// SH for the set
	};

	struct RuntimeSamplingPointInfos
	{
		float3		Position;						// World position of the sampling point
		float3		Normal;							// World normal of the sampling point
		float		Radius;							// Radius of the sampling point's disc approximation
	};

public:
	struct RuntimeProbeNetworkInfos
	{
		U32			ProbeIDs[2];					// The IDs of the 2 connected probes
		float2		NeighborsSolidAngles;			// Their perception of each other's solid angle
	};

#pragma pack( pop )


protected:

	// The static probe structure that we read from disk and stream/keep in memory when probes need updating
	struct	ProbeStruct
	{
		U32				ProbeID;				// The ID is simply the probe's index in the array of probes
		Scene::Probe*	pSceneProbe;

		// Static SH infos
		float			pSHOcclusion[9];		// The pre-computed SH that gives back how much of the environment is perceived in a given direction
		float3			pSHBounceStatic[9];		// The pre-computed SH that gives back how much the probe perceives of indirectly bounced static lighting on static geometry

		// Geometric infos
		float			MeanDistance;			// Mean distance of all scene pixels
		float			MeanHarmonicDistance;	// Mean harmonic distance (1/sum(1/distance)) of all scene pixels
		float			MinDistance;			// Distance to closest scene pixel
		float			MaxDistance;			// Distance to farthest scene pixel
		float3			BBoxMin;				// Dimensions of the bounding box (axis-aligned) of the scene pixels
		float3			BBoxMax;

		// Generic reflective sets infos
		U32				SetsCount;				// The amount of dynamic sets for that probe
		struct SetInfos
		{
			float3			Position;			// The position of the dynamic set
			float3			Normal;				// The normal of the dynamic set's plane
			float3			Tangent;			// The longest principal axis of the set's points cluster (scaled by the length of the axis)
			float3			BiTangent;			// The shortest principal axis of the set's points cluster (scaled by the length of the axis)
			float3			Albedo;				// The albedo of the dynamic set (not currently used, for info purpose)
			float3			pSHBounce[9];		// The pre-computed SH that gives back how much the probe perceives of indirectly bounced dynamic lighting on static geometry, for each dynamic set

			U32				SamplesCount;		// The amount of samples for that probe
			struct	Sample
			{
				float3			Position;
				float3			Normal;
				float			Radius;
			}				pSamples[MAX_SET_SAMPLES];

		}				pSetInfos[MAX_PROBE_SETS];

		// Emissive sets infos
		U32				EmissiveSetsCount;		// The amount of emissive sets for that probe
		struct EmissiveSetInfos
		{
			float3			Position;			// The position of the emissive set
			float3			Normal;				// The normal of the emissive set's plane
			float3			Tangent;			// The longest principal axis of the set's points cluster (scaled by the length of the axis)
			float3			BiTangent;			// The shortest principal axis of the set's points cluster (scaled by the length of the axis)
			Scene::Material*	pEmissiveMaterial;	// Direct pointer to the material

			float			pSHEmissive[9];		// The pre-computed SH that gives back how much the probe emits light
		}				pEmissiveSetInfos[MAX_PROBE_EMISSIVE_SETS];

		// Neighbor probes infos
		float			NearestProbeDistance;
		float			FarthestProbeDistance;
		struct NeighborProbeInfos
		{
			U32				ProbeID;			// ID of the neighbor probe
			float			Distance;			// Average distance to the probe
			float			SolidAngle;			// Perceived solid angle covered by the probe
			float3			Direction;			// Average direction to the probe
			float			SH[9];				// Convolution SH to use to isolate the contribution of the probe's SH this probe should perceive
		}				pNeighborProbeInfos[MAX_PROBE_NEIGHBORS];


		// ===== Software Computation Section =====
		float3		pSHBouncedLight[9];		// The resulting bounced irradiance * light(static+dynamic) + emissive for current frame

		// Clears the light bounce accumulator
		void			ClearLightBounce( const float3 _pSHAmbient[9] );

		// Computes the product of SHLight and SHBounce to get the SH coefficients for the bounced light
		void			AccumulateLightBounce( const float3 _pSHSet[9] );
	};

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

	Material*			m_pMatRender;				// Displays the scene
	Material*			m_pMatRenderEmissive;		// Displays the scene's emissive objects (area lights)
	Material*			m_pMatRenderLights;			// Displays the lights as small emissive balls
	Material*			m_pMatRenderDynamic;		// Displays the dynamic objects as balls with a normal map
	Material*			m_pMatRenderCubeMap;		// Renders the scene into a cubemap
	Material*			m_pMatRenderNeighborProbe;	// Renders the neighbor probes as planes to form a 3D voronoï cell
	Material*			m_pCSComputeShadowMapBounds;// Computes the shadow map bounds
	Material*			m_pMatRenderShadowMap;		// Renders the directional shadowmap
	Material*			m_pMatRenderShadowMapPoint;	// Renders the point light shadowmap
	Material*			m_pMatPostProcess;			// Post-processes the result
	Material*			m_pMatRenderDebugProbes;	// Displays the probes as small spheres
	Material*			m_pMatRenderDebugProbesNetwork;	// Displays the probes network
	
	ComputeShader*		m_pCSUpdateProbe;			// Dynamically update probes

	// Scene & Primitives
	Scene				m_Scene;
	float3				m_SceneBBoxMin;
	float3				m_SceneBBoxMax;
	CompositeVertexFormatDescriptor	m_SceneVertexFormatDesc;
	bool				m_bDeleteSceneTags;
	Primitive*			m_pPrimSphere;
	Primitive*			m_pPrimPoint;

		// Cached list of meshes
	U32					m_MeshesCount;
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

		// Optional vertex stream containing probe IDs for each vertex
	U32					m_VertexStreamProbeIDsLength;
	U32*				m_pVertexStreamProbeIDs;
	Primitive*			m_pPrimProbeIDs;


		// Scene octree containing probes
	Octree<const ProbeStruct*>	m_ProbeOctree;

		// Dynamic objects
	DynamicObject		m_pDynamicObjects[MAX_DYNAMIC_OBJECTS];


	// Textures
	int					m_TexturesCount;
	Texture2D**			m_ppTextures;
	Texture2D*			m_pTexDynamicNormalMap;
	Texture2D*			m_pRTShadowMap;
	Texture2D*			m_pRTShadowMapPoint;

	// Constant buffers
 	CB<CBGeneral>*		m_pCB_General;
 	CB<CBScene>*		m_pCB_Scene;
 	CB<CBObject>*		m_pCB_Object;
	CB<CBDynamicObject>*m_pCB_DynamicObject;
 	CB<CBMaterial>*		m_pCB_Material;
 	CB<CBProbe>*		m_pCB_Probe;
	CB<CBSplat>*		m_pCB_Splat;
 	CB<CBShadowMap>*	m_pCB_ShadowMap;
 	CB<CBShadowMapPoint>*	m_pCB_ShadowMapPoint;
 	CB<CBUpdateProbes>*	m_pCB_UpdateProbes;

	// Runtime scene lights & probes
	SB<LightStruct>*	m_pSB_LightsStatic;
	SB<LightStruct>*	m_pSB_LightsDynamic;
	SB<RuntimeProbe>*	m_pSB_RuntimeProbes;

	// Probes Update
	U32					m_ProbesCount;
	ProbeStruct*		m_pProbes;
	SB<RuntimeProbeUpdateInfos>*			m_pSB_RuntimeProbeUpdateInfos;
	SB<RuntimeProbeUpdateSetInfos>*			m_pSB_RuntimeProbeSetInfos;
	SB<RuntimeProbeUpdateEmissiveSetInfos>*	m_pSB_RuntimeProbeEmissiveSetInfos;
	SB<RuntimeSamplingPointInfos>*			m_pSB_RuntimeSamplingPointInfos;

	// Probes network debug
	SB<RuntimeProbeNetworkInfos>*			m_pSB_RuntimeProbeNetworkInfos;


	// Ambient SH computed from CIE overcast sky model
	float3			m_pSHAmbientSky[9];


	// Queue of probe indices to update each frame
	// TODO! I'm only storing the index of the sequence of probes I'll update each frame
	int				m_ProbeUpdateIndex;


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
	};

	// Memory-Mapped File for tweaking
	MMF<ParametersBlock>*	m_pMMF;
	ParametersBlock			m_CachedCopy;	// Latest cached copy of the update parms

#endif


public:		// PROPERTIES

	int				GetErrorCode() const	{ return m_ErrorCode; }


public:		// METHODS

	EffectGlobalIllum2( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, Camera& _Camera );
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
	void			PreComputeProbes();
};