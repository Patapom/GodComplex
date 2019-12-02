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

	struct DynamicUpdateParms {

		class IQueryMaterial {
		public: virtual Scene::Material*	operator()( U32 _MaterialID ) = 0;
		};

		U32				MaxProbeUpdatesPerFrame;// Maximum amount of probes we can update each frame

		IQueryMaterial*	pQueryMaterial;

//		float3			AmbientSkySH[9];		// The SH coefficients used for the ambient sky term
		float3			BounceFactorSun;		// Bounce factor for the Sun
		float3			BounceFactorSky;		// Bounce factor for the sky
		float3			BounceFactorDynamic;	// Bounce factor for dynamic lights
		float3			BounceFactorStatic;		// Bounce factor for static lights
		float			BounceFactorEmissive;	// Bounce factor for emissive materials
		float			BounceFactorNeighbors;	// Bounce factor for neighbor probes
	};

	class IRenderSceneDelegate {
	public: virtual void	operator()( Shader& _Material ) = 0;
	};

private:	// RUNTIME STRUCTURES

#pragma pack( push, 4 )

	struct CBProbe {		// Used by NeighborProbes splatting
		float3		CurrentProbePosition;
		U32			NeighborProbeID;
		float3		NeighborProbePosition;
		float		QuadHalfSize;
 	};

	struct CBUpdateProbes {	// Used by probes dynamic update
		float3		StaticLightingBoost;
		float		__PAD0;

		float3		SkyBoost;
		float		__PAD1;

		float3		SunBoost;
		float		__PAD2;

		float3		DynamicLightsBoost;
		float		EmissiveBoost;

		float		NeighborProbesContributionBoost;
//		float4		AmbientSH[9];		// Ambient sky (padded!)
 	};

	// Runtime probes buffer containing the probe's position and all neighbor probes in the Vorono� cell
	struct RuntimeProbe  {
		float3		Position;
		float		Radius;
		U32			NeighborsOffset;
		U32			NeighborsCount;
//		float3		pSH[9];
	};

	// Vorono� cell neighbor
	struct ProbeNeighbors {
		U32			ProbeID;			// ID of the neighbor probe
		float3		Position;			// Position of the neighbor probe
	};

	struct SHCoeffs1 {
		float		pSH[9];
	};

	struct SHCoeffs3 {
		float3		pSH[9];
	};

	// Probes update buffers
	struct RuntimeProbeUpdateInfo
	{
		U32			Index;							// The index of the probe we're updating
		U32			EmissiveSurfacesStart;			// Index of the first emissive surface for the probe
		U32			EmissiveSurfacesCount;			// Amount of emissive surfaces for the probe

		// Neighbor probes
		U32			NeighborProbeIDs[4];			// The IDs of the 4 most significant neighbor probes
		float4		SHConvolution[9];				// The SH coefficients to convolve the neighbor's SH with to obtain their contribution to this probe
	};

	struct	RuntimeProbeUpdateSampleInfo
	{
		float3		Position;						// World position of the sampling point
		float3		Normal;							// World normal of the sampling point
		float3		Albedo;							// Albedo of the sample
		float		Radius;							// Radius of the sampling point's disc approximation (set to 0 to discard sample)
	};

	struct	RuntimeProbeUpdateEmissiveSurfaceInfo
	{
		float3		EmissiveColor;					// Color of the emissive material
		float		SH[9];							// SH for the surface
	};

	struct RuntimeProbeNetworkInfos
	{
		U32			ProbeIDs[2];					// The IDs of the 2 connected probes
		float2		NeighborsSolidAngles;			// Their perception of each other's solid angle
	};

#pragma pack( pop )


private:	// BUILD TIME STRUCTURES

	struct ProbeInfluence {
		U32		ProbeID;
		double	Influence;
	};

	class MeshWithAdjacency {
	public:
		class	Primitive {
		private:
			// Original vertex structure
			struct Vertex {
				float3			wsPosition;				// World position (needed to interrogate probes's Vorono� cells that are in world space)
				U32				WeldedVertexIndex;		// Index of the welded vertex this vertex is collapsed into
				ProbeInfluence*	pInfluence;				// Probe influence associated to this vertex, if any

				Vertex() : WeldedVertexIndex( ~0U ), pInfluence( NULL ) {}
			};

			// Linked-list vertex structure
			struct VertexLink {
				VertexLink*		pNext;
				U32				V;						// Original vertex index
			};
			static VertexLink*		ms_ppCells[64*64*64];

			// Welded vertex structure
			struct WeldedVertex {
				float3				lsPosition;				// Local position
				float3				wsPosition;				// World position
				float3				lsNormal;				// Local normal
				ProbeInfluence		Influence;				// Probe influence for this vertex
				U32					SharingVerticesCount;	// Amount of vertices welded together
				VertexLink*			pSharingVertices;		// List of original vertices sharing this welded vertex
				List<WeldedVertex*>	AdjacentVertices;		// List of welded vertices adjacent to this vertex

				// Main code that propagates probe influences between adjacent vertices
 				bool	PropagateProbeInfluencesBetweenVertices( SHProbeNetwork& _Owner );
			};

		public:
			List< Vertex >			m_Vertices;
			List< VertexLink >		m_VertexCells;
			List< WeldedVertex >	m_WeldedVertices;		

			void	Build( SHProbeNetwork& _Owner, const float4x4& _Local2World, const Scene::Mesh::Primitive& _SourcePrimitive, ProbeInfluence* _pProbeInfluencePerFace );
			U32		PropagateProbeInfluences( SHProbeNetwork& _Owner );
			U32		AssignNearestProbe( SHProbeNetwork& _Owner );
			void	RedistributeProbeIDs2Vertices( ProbeInfluence const** _ppProbeInfluences ) const;
		};

		float4x4		m_Local2World;
		float4x4		m_World2Local;

		int				m_PrimitivesCount;
		Primitive*		m_pPrimitives;

		~MeshWithAdjacency() { SAFE_DELETE_ARRAY( m_pPrimitives ); }

		void	Build( SHProbeNetwork& _Owner, const Scene::Mesh& _Mesh, ProbeInfluence* _pProbeInfluencePerFace );
		U32		PropagateProbeInfluences( SHProbeNetwork& _Owner );
		U32		AssignNearestProbe( SHProbeNetwork& _Owner );
		void	RedistributeProbeIDs2Vertices( ProbeInfluence const**& _ppProbeInfluences ) const;
	};


private:	// FIELDS
	
	Device*					m_pDevice;
	U32						m_ErrorCode;

	// The list of probes in the scene
	U32						m_ProbesCount;
	U32						m_MaxProbesCount;
	SHProbe*				m_pProbes;

	Primitive*				m_pScreenQuad;

	Texture2D*				m_pRTCubeMap;

	Shader*				m_pMatRenderCubeMap;		// Renders the scene into a cubemap
	Shader*				m_pMatRenderNeighborProbe;	// Renders the neighbor probes as planes to form a 3D vorono� cell
	ComputeShader*			m_pCSUpdateProbeDynamicSH;	// Dynamically update probes (spread across several frames)
	ComputeShader*			m_pCSAccumulateProbeSH;		// Dynamically update probes' SH by accumulating static + sky + dynamic SH (done each frame)

	Octree<const SHProbe*>	m_ProbeOctree;				// Scene octree containing probes, queried by dynamic objects

	// Constant buffers
 	CB<CBProbe>*			m_pCB_Probe;
 	CB<CBUpdateProbes>*		m_pCB_UpdateProbes;

	// Runtime probes
	SB<RuntimeProbe>*		m_pSB_RuntimeProbes;		// (SRV) Position + Radius + info for each probe

	SB<SHCoeffs3>*			m_ppSB_RuntimeSHStatic[2];	// (SRV) 2 sets of static SH (2 sets of lights, A and B, render in these)
	SB<SHCoeffs1>*			m_pSB_RuntimeSHAmbient;		// (SRV) 1 set of ambient sky SH 
	SB<SHCoeffs3>*			m_pSB_RuntimeSHDynamic;		// (UAV) 1 sets of dynamic SH (updated in real time across several frames)
	SB<SHCoeffs3>*			m_pSB_RuntimeSHDynamicSun;	// (UAV) 1 sets of dynamic SH for the Sun (updated in real time across several frames)
	SB<SHCoeffs3>*			m_pSB_RuntimeSHFinal;		// (UAV) The sum of all the above, updated each frame...

	// Vorono� cell neighbors
	SB<ProbeNeighbors>*		m_pSB_ProbeNeighbors;

	// Probes Update
	SB<RuntimeProbeUpdateInfo>*					m_pSB_RuntimeProbeUpdateInfos;		// (SRV) Update info for each probe we're updating (e.g. index, emissive surface index/count, neighbor influences, etc.)
	SB<RuntimeProbeUpdateSampleInfo>*			m_pSB_RuntimeProbeSamples;			// (SRV) Info for each probe sample we're updating (position, normal, albedo, radius)
	SB<RuntimeProbeUpdateEmissiveSurfaceInfo>*	m_pSB_RuntimeProbeEmissiveSurfaces;	// (SRV) Info for each emissive surface we're updating (color, SH)
	SB<SHCoeffs1>*								m_pSB_RuntimeProbeSamplesSH;		// (SRV) SH for each sample direction

	// Additional vertex stream containing probe IDs for each vertex
	Primitive*				m_pPrimProbeIDs;

	// Probes network debug
	SB<RuntimeProbeNetworkInfos>*	m_pSB_RuntimeProbeNetworkInfos;

	// Queue of probe indices to update each frame
	// TODO! I'm only storing the index of the sequence of probes I'll update each frame
	int						m_ProbeUpdateIndex;

	// The encoder that will render cube maps and process them to generate runtime probe data
	SHProbeEncoder			m_ProbeEncoder;

	// List of probe influences for each face of the scene
	List< ProbeInfluence >	m_ProbeInfluencePerFace;


public:

	SHProbeNetwork();
	~SHProbeNetwork();

	void			Init( Device& _Device, Primitive& _ScreenQuad );
	void			Exit();

	void			PreAllocateProbes( int _ProbesCount );

	void			AddProbe( Scene::Probe& _Probe );
	U32				GetProbesCount() const			{ return m_ProbesCount; }
	const SHProbe&	GetProbe( U32 _Index ) const	{ return m_pProbes[_Index]; }

	Primitive*		GetProbeIDVertexStream() const	{ return m_pPrimProbeIDs; }

	// Runtime use
	void			UpdateDynamicProbes( DynamicUpdateParms& _Parms );
	U32				GetNearestProbe( const float3& _wsPosition ) const;

	// Build/Load/Save
	void			PreComputeProbes( const char* _pPathToProbes, IRenderSceneDelegate& _RenderScene, Scene& _Scene, U32 _TotalFacesCount );
	void			LoadProbes( const char* _pPathToProbes, const float3& _SceneBBoxMin, const float3& _SceneBBoxMax );

private:

	void			BuildProbeInfluenceVertexStream( Scene& _Scene, const char* _pPathToStreamFile );

friend class SHProbeEncoder;
friend static void	CopyProbeNetworkConnection( int _EntryIndex, SHProbeNetwork::RuntimeProbeNetworkInfos& _Value, void* _pUserData );

};
