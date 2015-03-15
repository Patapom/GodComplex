//////////////////////////////////////////////////////////////////////////
// SH Probe Encoder
//
// Encodes a cube map containing albedo, normal, distance, material ID, static lighting and emissive material
//
// The cube map is sliced into pre-defined groups of pixels all centered around N principal directions,
//	N is usually 128.
//
// These pixels are analyzed to isolate an average position, direction and color. Or they get discared altogether
//	because the entire group is not considered significant enough to contribute to the lighting of the probe.
//
//
#pragma once

class	SHProbeEncoder
{
public:		// CONSTANTS

	static const U32		MAX_PROBE_SAMPLES = 128;			// Subdivide the sphere into 128 samples
	static const U32		MAX_PROBE_EMISSIVE_SURFACES = 16;	// We only deal with a maximum of 16 emissive surfaces

	static const U32		CUBE_MAP_SIZE = 128;
	static const int		CUBE_MAP_FACE_SIZE = CUBE_MAP_SIZE * CUBE_MAP_SIZE;

	// Various thresholds used to allow merging of adjacent pixels
	static float			DISTANCE_THRESHOLD;
	static float			ANGULAR_THRESHOLD;
	static float			ALBEDO_HUE_THRESHOLD;
	static float			ALBEDO_RGB_THRESHOLD;

	static const double		SAMPLE_SH_NORMALIZER;				// 1 / MAX_PROBE_SAMPLES, an equal share for all samples

private:

	static const float	Z_INFINITY;
	static const float	Z_INFINITY_TEST;


private:	// NESTED TYPES

	struct	PixelsList;
	class	Sample;

	// This represents all the information about the pixel of a cube map
	class	Pixel {
		static const double		f0;
		static const double		f1;
		static const double		f2;
		static const double		f3;

	public:
		static float	IMPORTANCE_THRESOLD;	// To be set manually before encoding

	public:
		Pixel*		pNext;					// Pointer to the next pixel in the list if they're part of a particular sample

		int			Index;					// Index of the pixel in the scene pixels (can help us locate the cube map face + position of the pixel when finding adjacent pixels)
		int			CubeFaceIndex;
		int			CubeFaceX;
		int			CubeFaceY;

		float3		Position;				// World position
		float3		Normal;					// World normal
		float3		Albedo;					// Material albedo
		float3		AlbedoHSL;				// Material albedo in HSL format
		float3		F0;						// Material Fresnel coefficient
		float3		StaticLitColor;			// Color of the statically lit environment
		float3		SmoothedStaticLitColor;	// Color of the statically lit environment
		U32			FaceIndex;				// Absolute scene face index
		U32			EmissiveMatID;			// ID of the emissive material or ~0UL if not emissive
		U32			NeighborProbeID;		// ID of the nearest neighbor probe
		float		NeighborProbeDistance;	// Distance to the neighbor probe plane
		double		Importance;				// A measure of "importance" of the scene pixel = -dot( View, Normal ) / Distance²
		float		Distance;				// Distance from the probe's center
		float		SmoothedDistance;		// Smoothed out distance for more tolerant merging of noisy surfaces
		bool		Infinity;				// True if not a scene pixel (i.e. sky pixel)
		float		SmoothedInfinity;		// Smoothed out infinity value for better SH encoding

		double		SolidAngle;				// Solid angle covered by the pixel
		float3		View;					// View vector pointing to that pixel
		double		SHCoeffs[9];			// SH coefficients of the pixel

		Sample*		pParentSample;			// The sample this pixel is part of
		bool		bUsedForSampling;		// Tells if the pixel is used by the sample

		PixelsList*	pParentList;			// The list this pixel is part of (only temporary, used when building)
		Pixel*		pNextInList;			// Next pixel in the list

		Pixel()
			: pNext( NULL )
			, Position( float3::Zero )
			, Normal( float3::Zero )
			, Albedo( float3::Zero )
			, AlbedoHSL( float3::Zero )
			, FaceIndex( ~0UL )
			, EmissiveMatID( ~0UL )
			, NeighborProbeID( ~0UL )
			, NeighborProbeDistance( 0.0f )
			, Importance( 0.0 )
			, Distance( 0.0f )
			, Infinity( false )
			, SolidAngle( 0.0 )
			, View( float3::Zero )
			, pParentSample( NULL )
			, pParentList( NULL )
			, pNextInList( NULL ) {}

		void		InitSH() {
			// Build SH coeffs for that pixel
			SHCoeffs[0] = f0;
			SHCoeffs[1] = -f1 * View.x;
			SHCoeffs[2] = f1 * View.y;
			SHCoeffs[3] = -f1 * View.z;
			SHCoeffs[4] = f2 * View.x * View.z;
			SHCoeffs[5] = -f2 * View.x * View.y;
			SHCoeffs[6] = f3 * (3.0 * View.y*View.y - 1.0);
			SHCoeffs[7] = -f2 * View.z * View.y;
			SHCoeffs[8] = f2 * 0.5 * (View.z*View.z - View.x*View.x);
		}

		// Sets the albedo's RGB & HSL values
		void		SetAlbedo( const float3& _RGB )
		{
			Albedo = _RGB;

			// Convert into HSL
			float	Min = min( min( Albedo.x, Albedo.y ), Albedo.z );
			float	Max = max( max( Albedo.x, Albedo.y ), Albedo.z );
			float	Delta = Max - Min;

			float	L = 0.5f * (Max + Min);
			float	S = Delta;
			float	H = 0;
			if ( Delta > 0 )
			{
				S /= L < 0.5f ? 2.0f * L : 2.0f * (1.0f - L);
				if ( Max == Albedo.x )
					H = (Albedo.y - Albedo.z) / Delta;
				else if ( Max == Albedo.y )
					H = 2.0f + (Albedo.z - Albedo.x) / Delta;
				else if ( Max == Albedo.z )
					H = 4.0f + (Albedo.x - Albedo.y) / Delta;
				else
					ASSERT( false, "Rha!" );

				H = fmod( H + 6, 6.0f );
			}

// DEBUG => Should stop on saturated colors like the red/blue side walls
// if ( S > 0.5f )
// 	S += 1e-6f;

			AlbedoHSL.Set( H, S, L );
		}

		// Tells if the pixel is acceptable on its own.
		// The test checks if the pixel:
		//	_ doesn't already belong to a list
		//	_ is part of the same sample
		//	_ is a scene pixel (i.e. not at infinity)
		//	_ has enough importance
		bool		IsFloodFillAcceptable( Sample& _SourceSample )
		{
			if ( pParentList != NULL )
				return false;	// We don't accept pixels that are already part of a list
			if ( pParentSample != &_SourceSample )
				return false;	// We don't accept pixels that are part of another sample!
			if ( Infinity )
				return false;	// We only accept scene pixels!
			if ( EmissiveMatID != ~0UL )
				return false;	// Reject all emissive pixels no matter what!
			if ( Importance < IMPORTANCE_THRESOLD )
				return false;	// Not important enough!

			return true;
		}

		// Sorts a linked list of pixels using radix sort
		struct RadixNode_t {
			U32		Key;
			Pixel*	pPixel;
		};
		class ISortKeyProvider {
		public: virtual U32	GetKey( const Pixel& _Pixel ) const = 0;
		};
		static RadixNode_t*	ms_RadixNodes[2];
		static void	Sort( Pixel*& _pList, ISortKeyProvider& _KeyProvider, bool _ReverseSortOnExit );	// Directly takes a linked list and builds a sortable list. If reverse is used, list is rebuilt from largest to lowest key.
		static void	Sort( U32 _ElementsCount, RadixNode_t* _pList, RadixNode_t* _pSorted );				// Takes a sortable list and a temp buffer
	};

	// A sample is a collection of pixels averaged as a single position, direction and a set of SH coefficients representing its contribution
	// The main direction of the sample is stored in the View vector.
	// All cube map pixels point to a sample so the pParentSample field of pixels is never empty but the reverse is not true:
	//	a sample may not contain all the pixels that are part of it originally simply because the pixels have been discarded
	//	as not being relevant enough to be part of the sample.
	// If the sample ends up containing too few pixels then it's simply discarded.
	//
	class	Sample : public Pixel {
	public:
		U32				PixelsCount;			// Amount of pixels in the sample
		Pixel*			pPixels;				// The list of pixels belonging to this sample

		U32				OriginalPixelsCount;	// The amount of pixels belonging to the sample, discarded or not (theoretically, all samples should contain an equal amount of pixels since we uniformly subdivided the sphere)
		Pixel*			pCenterPixel;			// The pixel at the center of this sample

		U32				ID;						// Warning: Only available once the computation is over and all samples have been resolved!
												// That's because some samples can be entirely discarded and remaining samples will be packed together as a list
												//	so the final ID may not equal the original index

		float3			Direction;				// The average direction toward this sample
		float3			Tangent;				// The longest principal axis of the samples's points cluster (scaled by the length of the axis)
		float3			BiTangent;				// The shortest principal axis of the samples's points cluster (scaled by the length of the axis)
		float			Radius;					// Approximate radius of the samples's points cluster (used for shadow filtering)

//		float			SH[9];					// The generated SH coefficients for this sample

		Sample()
			: PixelsCount( 0 )
			, pPixels( NULL )
			, OriginalPixelsCount( 0 )
			, pCenterPixel( NULL )
			, ID( ~0UL ) {}
	};

	// A surface is a collection of pixels with a centroid, a normal and an average albedo
 	class	EmissiveSurface {
 	public:
		U32				PixelsCount;	// Amount of pixels in the surface
		Pixel*			pPixels;		// The list of pixels belonging to this surface

		U32				EmissiveMatID;	// ID of the emissive material or ~0UL if not emissive
		U32				ID;				// Warning: Only available once the computation is over and all surfaces have been resolved!

		double			SolidAngle;		// Accumulated solid angle

 		// The generated SH coefficients for this surface
 		double			SH[9];

 	public:

		EmissiveSurface()
			: PixelsCount( 0 )
			, pPixels( NULL )
			, EmissiveMatID( ~0UL )
			, ID( ~0UL )
			, SolidAngle( 0.0 ) {
				memset( SH, 0, 9*sizeof(double) );
			}
 	};

	// Contains information on a neighbor probe
	class	NeighborProbe {
	public:
		U32			ProbeID;
		float		Distance;			// Distance to the neighbor probe
		double		SolidAngle;			// Solid angle covered by the neighbor probe, as perceived by our probe
		float3		Direction;			// Direction to the probe
		double		SH[9];				// SH coefficients used to isolate the probe's contribution to our probe

		int			PixelsCount;

	public:
		NeighborProbe()
			: ProbeID( -1 )
			, Distance( 0.0f )
			, SolidAngle( 0.0f )
			, Direction( float3::Zero )
			, PixelsCount( 0 ) {}
	};

	struct PixelsList {
		U32		PixelsCount;
		Pixel*	pPixels;
		double	Importance;

		PixelsList() : PixelsCount( 0 ), pPixels( NULL ), Importance( 0.0 ) {}
	};

private:

	class	CubeMapPixelWalker {
		const SHProbeEncoder&	Owner;
		U32						CubeFaceIndex;
		int						pUV[2];		// Current position on the cube map face, each coordinate in [0,CUBE_MAP_SIZE[
		int						pRight[2];	// Points to right
		int						pDown[2];	// Points to down
	public:

		CubeMapPixelWalker( const SHProbeEncoder& _Owner, Pixel& _Pixel ) : Owner( _Owner ) {
			Set( _Pixel );
		}

		void	Set( Pixel& _Pixel );
		Pixel&	Get() const;

		Pixel&	Left();
		Pixel&	Right();
		Pixel&	Down();
		Pixel&	Up();

	private:
		void	TransformUV( const int _Transform[6] );
		void	GoToAdjacentPixel( int _dU, int _dV );
	};


private:	// FIELDS

	float4x4			m_Side2World[6];

	U32					m_ProbeID;							// This is extracted from the cube map file name... Not very robust but good enough!

	Pixel*				m_pCubeMapPixels;					// Original cube map
	U32					m_ScenePixelsCount;					// Amount of pixels that participate to the scene geometry (i.e. not at infinity)

	// Pre-computed samples
	Sample				m_pSamples[MAX_PROBE_SAMPLES];		// The array of samples best representing the probe's environment
	U32					m_MaxSamplePixelsCount;				// The maximum amount of pixels encountered on the samples

	// Generated geometric informations
	double				m_MeanDistance;
	double				m_MeanHarmonicDistance;
	double				m_MinDistance;
	double				m_MaxDistance;
	float3				m_BBoxMin;
	float3				m_BBoxMax;

	// Static and occlusion SH
	float3				m_StaticSH[9];
	float				m_OcclusionSH[9];

	// Generated samples
	U32					m_ValidSamplesCount;
	Sample*				m_ppValidSamples[MAX_PROBE_SAMPLES];

	List< PixelsList >	m_SamplePixelGroups;

 	// Emissive surfaces
	List< EmissiveSurface >	m_EmissiveSurfaces;

 	U32					m_EmissiveSurfacesCount;
 	EmissiveSurface*	m_ppEmissiveSurfaces[MAX_PROBE_EMISSIVE_SURFACES];

	// List of neighbor probes
	float				m_NearestNeighborProbeDistance;
	float				m_FarthestNeighborProbeDistance;
	List<NeighborProbe>	m_NeighborProbes;

 	// List of influence weights per face index
	List< double >		m_ProbeInfluencePerFace;


public:		// PROPERTIES

	const List< double >&	GetProbeInfluences() const	{ return m_ProbeInfluencePerFace; }


public:		// METHODS

	SHProbeEncoder();
	~SHProbeEncoder();

	// Encodes the MRT cube map into basic SH elements that can later be combined at runtime to form a dynamically updatable probe
	void	EncodeProbeCubeMap( Texture2D& _StagingCubeMap, U32 _ProbeID, U32 _ProbesCount, U32 _SceneTotalFacesCount );

	// Saves the resulting encoded probe to disk
	void	Save( const char* _FileName ) const;

	// Saves a debugging structure of all the pixels and surfaces
	void	SavePixels( const char* _FileName ) const;


private:
	// Reads back the cube map and populates cube map pixels, probe pixels and scene pixels.
	// After this, the probe is ready for encoding
	void	ReadBackProbeCubeMap( Texture2D& _StagingCubeMap, U32 _SceneTotalFacesCount );

	// Build surfaces using flood fill and adjacency propagation
	void	ComputeFloodFill( float _SpatialDistanceWeight, float _NormalDistanceWeight, float _AlbedoDistanceWeight, float _MinimumImportanceDiscardThreshold );

	// Intensive flood fill routine
	mutable int		m_ScanlinePixelIndex;
	mutable Pixel*	m_ppScanlinePixelsPool[6 * CUBE_MAP_SIZE * CUBE_MAP_SIZE];

	void	FloodFill( Sample& _S, Pixel* _PreviousPixel, Pixel* _P, PixelsList& _AcceptedPixels, PixelsList& _RejectedPixels ) const;
	bool	CheckAndAcceptPixel( Sample& _Sample, Pixel& _PreviousPixel, Pixel& _P, PixelsList& _AcceptedPixels, PixelsList& _RejectedPixels ) const;
	Pixel&	FindAdjacentPixel( const Pixel& _P, int _dU, int _dV, int& _DirectionU, int& _DirectionV ) const;

	// Helpers
	template< typename T > void	ToArray( const List<T>& _List, T* _Array, U32 _Max, U32& _ArraySize ) {
		_ArraySize = min( U32(_List.GetCount()), _Max );
		memcpy_s( _Array, _ArraySize*sizeof(T), &_List[0], _ArraySize*sizeof(T) );
	}

	// Code from http://forum.unity3d.com/threads/bitwise-operation-hammersley-point-sampling-is-there-an-alternate-method.200000/
	float ReverseBits( U32 bits ) {
		bits = (bits << 16u) | (bits >> 16u);
		bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
		bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
		bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
		bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
		float	Result = float(bits * 2.3283064365386963e-10); // / 0x100000000
		return Result;
	}

};