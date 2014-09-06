//////////////////////////////////////////////////////////////////////////
// SH Probe Network
//
// Defines an array of probes linked together through their neighbors
//
//
#pragma once

class	SHProbeNetwork
{
public:
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

};
