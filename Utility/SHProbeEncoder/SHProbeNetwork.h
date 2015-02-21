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
			}				pSamples[SHProbeEncoder::MAX_SAMPLES_PER_PATCH];

		}				pSetInfos[SHProbeEncoder::MAX_PROBE_PATCHES];

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
		}				pEmissiveSetInfos[SHProbeEncoder::MAX_PROBE_EMISSIVE_PATCHES];

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
		float4		AmbientSH[9];				// Ambient sky (padded!)
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
	SB<RuntimeProbeUpdateInfos>*			m_pSB_RuntimeProbeUpdateInfos;
	SB<RuntimeProbeUpdateSetInfos>*			m_pSB_RuntimeProbeSetInfos;
	SB<RuntimeProbeUpdateEmissiveSetInfos>*	m_pSB_RuntimeProbeEmissiveSetInfos;
	SB<RuntimeSamplingPointInfos>*			m_pSB_RuntimeSamplingPointInfos;

	// Probes network debug
	SB<RuntimeProbeNetworkInfos>*			m_pSB_RuntimeProbeNetworkInfos;

	// Queue of probe indices to update each frame
	// TODO! I'm only storing the index of the sequence of probes I'll update each frame
	int						m_ProbeUpdateIndex;

	SHProbeEncoder			m_ProbeEncoder;


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
	void			PreComputeProbes( const char* _pPathToProbes, IRenderSceneDelegate& _RenderScene );
	void			LoadProbes( const char* _pPathToProbes, IQueryMaterial& _QueryMaterial, const float3& _SceneBBoxMin, const float3& _SceneBBoxMax );

friend static void	CopyProbeNetworkConnection( int _EntryIndex, SHProbeNetwork::RuntimeProbeNetworkInfos& _Value, void* _pUserData );

};
