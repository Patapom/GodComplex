//////////////////////////////////////////////////////////////////////////
// SH Probe Network
//
// Defines an array of probes linked together through their neighbors
//
//
#pragma once

#include "SHProbeEncoder.h"

class	SHProbeNetwork
{
public:		// CONSTANTS

	static const U32		MAX_PROBE_NEIGHBORS = 4;			// Only keep the 4 most significant neighbors
	static const U32		MAX_PROBE_UPDATES_PER_FRAME = 32;	// Update a maximum of 32 probes per frame


public:		// NESTED TYPES

	// The static probe structure that we read from disk and stream/keep in memory when probes need updating
	struct	SHProbe
	{
		U32				ProbeID;					// The ID is simply the probe's index in the array of probes
		Scene::Probe*	pSceneProbe;

		// Static SH infos
		float			pSHOcclusion[9];			// The pre-computed SH that gives back how much of the environment is perceived in a given direction
		float3			pSHBounceStatic[9];			// The pre-computed SH that gives back how much the probe perceives of indirectly bounced static lighting on static geometry

		// Geometric infos
		float			MeanDistance;				// Mean distance of all scene pixels
		float			MeanHarmonicDistance;		// Mean harmonic distance (1/sum(1/distance)) of all scene pixels
		float			MinDistance;				// Distance to closest scene pixel
		float			MaxDistance;				// Distance to farthest scene pixel
		float3			BBoxMin;					// Dimensions of the bounding box (axis-aligned) of the scene pixels
		float3			BBoxMax;

		// Generic reflective surfaces infos
		U32				SamplesCount;				// The amount of dynamic samples for that probe
		struct Sample
		{
			float3			Position;				// The position of the dynamic surface
			float3			Normal;					// The normal of the dynamic surface's plane
			float3			Tangent;				// The longest principal axis of the samples's points cluster (scaled by the length of the axis)
			float3			BiTangent;				// The shortest principal axis of the samples's points cluster (scaled by the length of the axis)
			float			Radius;					// An average radius for the sample so we can better filter shadows
			float3			Albedo;					// The albedo of the dynamic surface (not currently used, for info purpose)
			float3			F0;						// Surface's Fresnel coefficient
			float			pSHBounce[9];			// The pre-computed SH that gives back how much the probe perceives of indirectly bounced dynamic lighting on static geometry
		}				pSamples[SHProbeEncoder::MAX_PROBE_SAMPLES];

		// Emissive surfaces infos
		U32				EmissiveSurfacesCount;		// The amount of emissive surfaces for that probe
		struct EmissiveSurface
		{
			float3				Position;			// The position of the emissive surface
			float3				Normal;				// The normal of the emissive surface's plane
			float3				Tangent;			// The longest principal axis of the surface's points cluster (scaled by the length of the axis)
			float3				BiTangent;			// The shortest principal axis of the surface's points cluster (scaled by the length of the axis)
			Scene::Material*	pEmissiveMaterial;	// Direct pointer to the material
			float				pSHEmissive[9];		// The pre-computed SH that gives back how much the probe emits light
		}				pEmissiveSurfaces[SHProbeEncoder::MAX_PROBE_EMISSIVE_SURFACES];

		// Neighbor probes infos
		float			NearestProbeDistance;
		float			FarthestProbeDistance;
		struct NeighborProbeInfos
		{
			U32				ProbeID;				// ID of the neighbor probe
			float			Distance;				// Average distance to the probe
			float			SolidAngle;				// Perceived solid angle covered by the probe
			float3			Direction;				// Average direction to the probe
			float			SH[9];					// Convolution SH to use to isolate the contribution of the probe's SH this probe should perceive
		}				pNeighborProbeInfos[MAX_PROBE_NEIGHBORS];


//		// ===== Software Computation Section =====
//		float3			pSHBouncedLight[9];			// The resulting bounced irradiance * light(static+dynamic) + emissive for current frame (only valid if CPU computed)
// 
// 		// Clears the light bounce accumulator
// 		void			ClearLightBounce( const float3 _pSHAmbient[9] );
// 
// 		// Computes the product of SHLight and SHBounce to get the SH coefficients for the bounced light
// 		void			AccumulateLightBounce( const float3 _pSHSet[9] );
	};

	struct DynamicUpdateParms {

		U32		MaxProbeUpdatesPerFrame;// Maximum amount of probes we can update each frame

		float3	AmbientSkySH[9];		// The SH coefficients used for the ambient sky term
		float3	BounceFactorSun;		// Bounce factor for the Sun
		float3	BounceFactorSky;		// Bounce factor for the sky
		float3	BounceFactorDynamic;	// Bounce factor for dynamic lights
		float3	BounceFactorStatic;		// Bounce factor for static lights
		float3	BounceFactorEmissive;	// Bounce factor for emissive materials
		float3	BounceFactorNeighbors;	// Bounce factor for neighbor probes
	};

	class IRenderSceneDelegate {
	public: virtual void	operator()( Material& _Material ) = 0;
	};
	class IQueryMaterial {
	public: virtual Scene::Material*	operator()( U32 _MaterialID ) = 0;
	};


private:	// RUNTIME STRUCTURES

#pragma pack( push, 4 )

	struct CBProbe			// Used by NeighborProbes splatting
	{
		float3		CurrentProbePosition;
		U32			NeighborProbeID;
		float3		NeighborProbePosition;
 	};

	struct CBUpdateProbes	// Used by probes dynamic update
	{
		float4		AmbientSH[9];		// Ambient sky (padded!)
//		float		SunBoost;	// Merged into the last float4

		float		SkyBoost;
		float		DynamicLightsBoost;
		float		StaticLightingBoost;

		float		EmissiveBoost;
		float		NeighborProbesContributionBoost;
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
	struct RuntimeProbeUpdateInfo
	{
		U32			Index;							// The index of the probe we're updating
		U32			SamplesStart;					// Index of the first sample for the probe
		U32			SamplesCount;					// Amount of samples for the probe
		U32			EmissiveSurfacesStart;			// Index of the first emissive surface for the probe
		U32			EmissiveSurfacesCount;			// Amount of emissive surfaces for the probe
		float3		SHStatic[9];					// Precomputed static SH (static geometry + static lights)
		float		SHOcclusion[9];					// Directional ambient occlusion for the probe

		// Neighbor probes
//		U32			NeighborProbeIDs[4];			// The IDs of the 4 most significant neighbor probes
		float4		NeighborProbeSH[9];				// The SH coefficients to convolve the neighbor's SH with to obtain their contribution to this probe
	};

	struct	RuntimeProbeUpdateSampleInfo
	{
		float3		Position;						// World position of the sampling point
		float3		Normal;							// World normal of the sampling point
		float		Radius;							// Radius of the sampling point's disc approximation
		float3		Albedo;							// Albedo of the sample
		float		SH[9];							// SH for the sample
	};

	struct	RuntimeProbeUpdateEmissiveSurfaceInfo
	{
		float3		EmissiveColor;					// Color of the emissive material
		float		SH[9];							// SH for the surface
	};

//public:
	struct RuntimeProbeNetworkInfos
	{
		U32			ProbeIDs[2];					// The IDs of the 2 connected probes
		float2		NeighborsSolidAngles;			// Their perception of each other's solid angle
	};

#pragma pack( pop )


private:	// FIELDS
	
	Device*					m_pDevice;
	U32						m_ErrorCode;

	Primitive*				m_pScreenQuad;

	Texture2D*				m_pRTCubeMap;

	Material*				m_pMatRenderCubeMap;		// Renders the scene into a cubemap
	Material*				m_pMatRenderNeighborProbe;	// Renders the neighbor probes as planes to form a 3D voronoï cell
	ComputeShader*			m_pCSUpdateProbe;			// Dynamically update probes

	Octree<const SHProbe*>	m_ProbeOctree;				// Scene octree containing probes, queried by dynamic objects

	// Constant buffers
 	CB<CBProbe>*			m_pCB_Probe;
 	CB<CBUpdateProbes>*		m_pCB_UpdateProbes;

	// Runtime probes
	SB<RuntimeProbe>*		m_pSB_RuntimeProbes;

	// Probes Update
	U32						m_ProbesCount;
	U32						m_MaxProbesCount;
	SHProbe*				m_pProbes;
	SB<RuntimeProbeUpdateInfo>*					m_pSB_RuntimeProbeUpdateInfos;
	SB<RuntimeProbeUpdateSampleInfo>*			m_pSB_RuntimeProbeSamples;
	SB<RuntimeProbeUpdateEmissiveSurfaceInfo>*	m_pSB_RuntimeProbeEmissiveSurfaces;

	// Probes network debug
	SB<RuntimeProbeNetworkInfos>*				m_pSB_RuntimeProbeNetworkInfos;

	// Queue of probe indices to update each frame
	// TODO! I'm only storing the index of the sequence of probes I'll update each frame
	int						m_ProbeUpdateIndex;

	SHProbeEncoder			m_ProbeEncoder;

	// List of probe influences for each face of the scene
	struct ProbeInfluence {
		U32		ProbeID;
		double	Influence;
	};
	List< ProbeInfluence >	m_ProbeInfluencePerFace;


public:

	SHProbeNetwork();
	~SHProbeNetwork();

	void			Init( Device& _Device, Primitive& _ScreenQuad );
	void			Exit();

	void			PreAllocateProbes( int _ProbesCount ) {
		m_MaxProbesCount = _ProbesCount;
		m_pProbes = new SHProbe[m_MaxProbesCount];
	}

	void			AddProbe( Scene::Probe& _Probe );
	U32				GetProbesCount() const	{ return m_ProbesCount; }

	// Runtime use
	void			UpdateDynamicProbes( DynamicUpdateParms& _Parms );
	U32				GetNearestProbe( const float3& _wsPosition ) const;

	// Build/Load/Save
	void			PreComputeProbes( const char* _pPathToProbes, IRenderSceneDelegate& _RenderScene, Scene& _Scene, U32 _TotalFacesCount );
	void			LoadProbes( const char* _pPathToProbes, IQueryMaterial& _QueryMaterial, const float3& _SceneBBoxMin, const float3& _SceneBBoxMax );

private:

	void			BuildProbeInfluenceVertexStream( Scene& _Scene, const char* _pPathToStreamFile );

friend static void	CopyProbeNetworkConnection( int _EntryIndex, SHProbeNetwork::RuntimeProbeNetworkInfos& _Value, void* _pUserData );

};
