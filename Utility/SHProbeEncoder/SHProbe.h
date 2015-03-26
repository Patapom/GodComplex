//////////////////////////////////////////////////////////////////////////
// SH Probe
//
// Defines an environment probe used for indirect lighting
//
//
#pragma once

// The static probe structure that we read from disk and stream/keep in memory when probes need updating
class	SHProbe {
public:		// CONSTANTS
	static const U32		SAMPLES_COUNT = 128;			// Subdivide the sphere into 128 samples
	static const U32		MAX_EMISSIVE_SURFACES = 16;	// We only deal with a maximum of 16 emissive surfaces

public:		// FIELDS

	U32				m_ProbeID;					// The ID is simply the probe's index in the array of probes
	Scene::Probe*	m_pSceneProbe;

	// Static SH infos
	float			m_pSHOcclusion[9];			// The pre-computed SH that gives back how much of the environment is perceived in a given direction
	float3			m_pSHStaticLighting[9];		// The pre-computed SH that gives back how much the probe perceives of indirectly bounced static lighting on static geometry

	// Geometric infos
	float			m_MeanDistance;				// Mean distance of all scene pixels
	float			m_MeanHarmonicDistance;		// Mean harmonic distance (1/sum(1/distance)) of all scene pixels
	float			m_MinDistance;				// Distance to closest scene pixel
	float			m_MaxDistance;				// Distance to farthest scene pixel
	float3			m_BBoxMin;					// Dimensions of the bounding box (axis-aligned) of the scene pixels
	float3			m_BBoxMax;

	// Generic reflective surfaces infos
	struct Sample {
		float3			Position;				// The position of the dynamic surface
		float3			AverageDirection;		// Average direction to the dynamic surface
		float3			Normal;					// The normal of the dynamic surface's plane
		float3			Tangent;				// The longest principal axis of the samples's points cluster (scaled by the length of the axis)
		float3			BiTangent;				// The shortest principal axis of the samples's points cluster (scaled by the length of the axis)
		float			Radius;					// An average radius for the sample so we can better filter shadows
		float3			Albedo;					// The albedo of the dynamic surface (not currently used, for info purpose)
		float3			F0;						// Surface's Fresnel coefficient
		float			SHFactor;				// The ratio of pixels occupied by the sample area compared to the total amount of original pixels

		Sample() : Radius( 0.0f ) {}			// A radius of 0 will discard the sample at runtime
	}				m_pSamples[SAMPLES_COUNT];

	// Emissive surfaces infos
	U32				m_EmissiveSurfacesCount;	// The amount of emissive surfaces for that probe
	struct EmissiveSurface {
		U32				MaterialID;				// Emissive material ID
		float			pSH[9];					// The pre-computed SH that gives back how much the probe emits light
	}				m_pEmissiveSurfaces[MAX_EMISSIVE_SURFACES];

	// Neighbor probes infos
	float			m_NearestNeighborProbeDistance;
	float			m_FarthestNeighborProbeDistance;
	struct NeighborProbeInfo {
		U32				ProbeID;				// ID of the neighbor probe
		bool			DirectlyVisible;		// True if the very center of the neighbor probe is directly visible to this probe
		float			Distance;				// Average distance to the probe
		float			SolidAngle;				// Perceived solid angle covered by the probe
		float3			Direction;				// Average direction to the probe
		float			SH[9];					// Convolution SH to use to isolate the contribution of the probe's SH this probe should perceive

		NeighborProbeInfo()
			: ProbeID( -1 )
			, Distance( 0.0f )
			, SolidAngle( 0.0f )
			, Direction( float3::Zero )
			{}
	};
	List< NeighborProbeInfo >	m_NeighborProbes;

	// Voronoï probes infos
	struct VoronoiProbeInfo {
		U32				ProbeID;				// ID of the neighbor probe defining a Voronoï cell surface
		float3			Position;				// Position of the Voronoï plane
		float3			Normal;					// Normal to the Voronoï plane (pointing INWARD the cell, toward this probe)

		VoronoiProbeInfo()
			: ProbeID( -1 )
			{}
	};
	List< VoronoiProbeInfo >	m_VoronoiProbes;


	// Static list of samples directions
	static float3			ms_SampleDirections[SHProbe::SAMPLES_COUNT];


public:		// METHODS

	// Tells if the specified position is within the probe's Voronoï cell
	bool	IsInsideVoronoiCell( const float3& _Position ) const;

	// I/O
	void	Save( FILE* _pFile ) const;
	void	Load( FILE* _pFile );
};
