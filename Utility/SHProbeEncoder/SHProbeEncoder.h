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

		PixelsList*	pParentList;			// The list thispixel is part of (only temporary, used when building)
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

		void		InitSH()
		{
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

		// Computes a "distance" between this pixel and another one
		float		ComputeMetric( const Pixel& _Other, float _PositionDistanceWeight, float _NormalDistanceWeight, float _AlbedoDistanceWeight )
		{
			float	EuclidianDistance = (_Other.Position - Position).Length();
					EuclidianDistance *= _PositionDistanceWeight;

			float	NormalDistance = 0.5f * (1.0f - (_Other.Normal | Normal));
					NormalDistance *= _NormalDistanceWeight;	// Used to give the normal as much weight as general euclidian distances...

			float	ColorDistance0 = fabs( AlbedoHSL.x - _Other.AlbedoHSL.x );
			float	ColorDistance1 = 6.0f - ColorDistance0;
			float	ColorDistance = min( ColorDistance0, ColorDistance1 ) / 6.0f;
					ColorDistance *= _AlbedoDistanceWeight;		// Used to give the color as much weight as general euclidian distances...

			return EuclidianDistance + NormalDistance + ColorDistance;
		}

		// Tells if the pixel is acceptable on its own.
		// The test checks if the pixel:
		//	_ doesn't already belong to a surface
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
				return true;	// Accept all emissive pixels no matter what!
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

		float			SH[9];					// The generated SH coefficients for this sample

		Sample()
			: PixelsCount( 0 )
			, pPixels( NULL )
			, OriginalPixelsCount( 0 )
			, pCenterPixel( NULL )
			, ID( ~0UL ) {}
	};

	// A surface is a collection of pixels with a centroid, a normal and an average albedo
// 	class	Surface : public Pixel {
// 	public:
// 		U32				PixelsCount;	// Amount of pixels in the surface
// 		Pixel*			pPixels;		// The list of pixels belonging to this surface
// 
// 		U32				ID;				// Warning: Only available once the computation is over and all surfaces have been resolved!
// 
// 		// Tangent space generated from principal directions of the points surface
// 		float3			Tangent;
// 		float3			BiTangent;
// 
// 		// The generated SH coefficients for this surface
// 		float3			SH[9];
// 
// 		// The generated samples that will be used at runtime to estimate lighting and update the surface's SH coefficients
// 		struct Sample 
// 		{
// 			float3			Position;
// 			float3			Normal;
// 			float			Radius;		// Radius of the disc encompassing all the pixels forming that sample
// 		};
// 		int				SamplesCount;
// 		Sample			Samples[MAX_SAMPLES_PER_SURFACE];
// 
// 
// //static readonly float	FILTER_WINDOW_SIZE = 3.0f;	// Our SH order is 3 so...
// 
// 	public:
// 
// 		Surface() : Pixel()
// 			, PixelsCount( 0 )
// 			, pPixels( NULL )
// 			, ID( -1 )
// 			, Tangent( float3::Zero )
// 			, BiTangent( float3::Zero )
// 			, SamplesCount( 0 ) {}
// 
// 		// Performs the SH encoding of all the pixels belonging to the surface
// 		// It simply amounts to summing the directional SH contribution of every pixels in the surface, assuming they all receive the energy received by the surface's substitute center
// 		void			EncodeSH()
// 		{
// 			double		AlbedoR = Albedo.x / PI;
// 			double		AlbedoG = Albedo.y / PI;
// 			double		AlbedoB = Albedo.z / PI;
// 
// 			double		SHR[9];
// 			double		SHG[9];
// 			double		SHB[9];
// 			for ( int i=0; i < 9; i++ ) {
// 				SHR[i] = SHG[i] = SHB[i] = 0.0;
// 			}
// 
// 			Pixel*	pPixel = pPixels;
// 			while ( pPixel != NULL ) {
// 				Pixel&	P = *pPixel;
// 				pPixel = pPixel->pNext;
// 
// 				// Compute weight factor based on surface's normal and pixel's normal but also based on pixel's normal and view direction
// 				double	Factor  = P.SolidAngle								// Solid angle for SH weight, obvious
// 								* max( 0.0, P.Normal.Dot( Normal ) )		// This weight is to account for the fact that the point is well aligned with the surface's plane the lighting was computed for
// 								* max( 0.0, -P.View.Dot( P.Normal ) );		// This weight is to account for the fact that the point is well aligned with the view vector
// 																			//	(for example, for a perfectly flat wall this weight will have the effect that points further from the probe's perpendicular will have less importance)
// 
// 				for ( int i=0; i < 9; i++ )
// 				{
// 					SHR[i] += P.SHCoeffs[i] * Factor * AlbedoR;
// 					SHG[i] += P.SHCoeffs[i] * Factor * AlbedoG;
// 					SHB[i] += P.SHCoeffs[i] * Factor * AlbedoB;
// 				}
// 			}
// 
// 			for ( int i=0; i < 9; i++ )
// 				SH[i].Set( float(SHR[i]), float(SHG[i]), float(SHB[i]) );
// 
// 			// Apply filtering
// //			SphericalHarmonics.SHFunctions.FilterLanczos( SH, FILTER_WINDOW_SIZE );
// 		}
// 
// 		// Performs the emissive SH encoding of all the pixels belonging to the surface
// 		// It simply amounts to summing the directional SH contribution of every pixels in the surface
// 		void			EncodeEmissiveSH() {
// 			double	SHCoeffs[9];
// 
// 			Pixel*	pPixel = pPixels;
// 			while ( pPixel != NULL ) {
// 				Pixel&	P = *pPixel;
// 				pPixel++;
// 
// 				for ( int i=0; i < 9; i++ )
// 					SHCoeffs[i] += P.SHCoeffs[i] * P.SolidAngle;
// 			}
// 
// //DON'T NORMALIZE	=> 4PI is part of the integral!!!
// //				double	Normalizer = 1.0 / (4.0 * Math.PI);
// 			double	Normalizer = 1.0;
// 			for ( int i=0; i < 9; i++ )
// 				SH[i] = (float) (Normalizer * SHCoeffs[i]) * float3::One;
// 
// // 			// Apply filtering
// // //				SphericalHarmonics.SHFunctions.FilterLanczos( FILTER_WINDOW_SIZE );
// // //				SphericalHarmonics.SHFunctions.FilterGaussian( FILTER_WINDOW_SIZE );	// Smoothes A LOT but according to the source of filters code, it's better if using HDR light sources
// // 			SphericalHarmonics.SHFunctions.FilterHanning( SH, FILTER_WINDOW_SIZE );
// 		}
// 
// 		// Generates N samples among the set's pixels where lighting will be sampled
// 		Pixel			__ReferencePixel;
// 		void			GenerateSamples( int _SamplesCount );
// 
// 		// This is a very simplistic approach to determine the principal axes of the surface:
// 		//  1) Create a dummy tangent space for the surface's plane
// 		//  2) Rotate an axis from 0 to 180° in that arbitrary tangent space
// 		//		2.1) Compute the max distance of each point of the surface to this axis (i.e. bounding rect extent in that direction)
// 		//		2.2) Keep the angle where we find the largest distance as our minor principal axis
// 		//		2.3) Keep the angle where we find the smallest distance as our major principal axis
// 		//
// // 		void			FindPrincipalAxes() {
// // 			// Create an arbitrary tangent space
// // 			float3	Y = Normal;
// // 			float3	X = float3.UnitY ^ Y;
// // 			float3	Z;
// // 			if ( X.Length < 1e-6 )
// // 			{	// Invalid basis!
// // 				X = float3.UnitX;
// // 				Z = float3.UnitZ;
// // 			}
// // 			else
// // 			{
// // 				X.Normalize();
// // 				Z = X ^ Y;
// // 			}
// // 
// // 			// Perform a rotation of a line from 0 to 180°
// // 			float	MaxDistance = 0.0f;
// // 			float	MaxDistanceAngle = 0.0f;
// // 			float	MinDistance = 1e6f;
// // 			float	MinDistanceAngle = 0.0f;
// // 			for ( int Angle=0; Angle < 180; Angle+=2 )
// // 			{
// // 				float			fAngle = (float) Math.PI * Angle / 180.0f;
// // 				float3	OrthoDir = (float) -Math.Sin( fAngle ) * X + (float) Math.Cos( fAngle ) * Z;	// This is actually the orthogonal direction to the line we're rotating
// // 
// // 				// Compute the max of distances from each point to this line
// // 				float	Distance = 0.0f;
// // 				foreach ( Pixel P in Pixels )
// // 				{
// // 					float3	Center2Pixel = P.Position - Position;
// // 					float			Dot = Math.Abs( Center2Pixel | OrthoDir );	// Gives the distance to the closest point on the rotating line
// // 					Distance = Math.Max( Distance, Dot );
// // 				}
// // 
// // 				// Check for new best candidates
// // 				if ( Distance > MaxDistance )
// // 				{	// New best candidate for minor axis!
// // 					MaxDistance = Distance;
// // 					MaxDistanceAngle = fAngle;
// // 				}
// // 				if ( Distance < MinDistance )
// // 				{	// New best candidate for major axis!
// // 					MinDistance = Distance;
// // 					MinDistanceAngle = fAngle;
// // 				}
// // 			}
// // 
// // 			// Finalize axes
// // 			BiTangent = (float) Math.Cos( MaxDistanceAngle ) * X + (float) Math.Sin( MaxDistanceAngle ) * Z;	// Our minor axis
// // 			Tangent = (float) Math.Cos( MinDistanceAngle ) * X + (float) Math.Sin( MinDistanceAngle ) * Z;		// Our major axis
// // 
// // 			float	Diff = Tangent | BiTangent;	// We'd prefer a diff of 0, meaning the found axes are orthogonal...
// // 
// // 			// Scale minor/major axes by these distances
// // 			Tangent *= MaxDistance;
// // 			BiTangent *= MinDistance;
// // 		}
// 	};

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
		Pixel*	pPixel;
		double	Importance;

		PixelsList() : PixelsCount( 0 ), pPixel( NULL ), Importance( 0.0 ) {}
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
	U32					m_ScenePixelsCount;
 	Pixel*				m_pScenePixels;						// List of pixels that participate to the scene geometry (i.e. not at infinity)

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

// 	// Dynamic & Emissive surfaces
// 	List< Surface >		m_AllSurfaces;
// 	U32					m_SurfacesCount;
// 	Surface*			m_ppSurfaces[MAX_PROBE_SURFACES];
// 	U32					m_EmissiveSurfacesCount;
// 	Surface*			m_ppEmissiveSurfaces[MAX_PROBE_EMISSIVE_SURFACES];
// 
// 	float3				m_SHSumDynamic[9];
// 	float3				m_SHSumEmissive[9];

	// List of neighbor probes
	float				m_NearestNeighborProbeDistance;
	float				m_FarthestNeighborProbeDistance;
	List<NeighborProbe>	m_NeighborProbes;

// 	// List of influence weights per face index
// 	U32					m_MaxFaceIndex;
// 	Dictionary<double>	m_ProbeInfluencePerFace;


public:		// PROPERTIES

public:		// METHODS

	SHProbeEncoder();
	~SHProbeEncoder();

	// Encodes the MRT cube map into basic SH elements that can later be combined at runtime to form a dynamically updatable probe
	void	EncodeProbeCubeMap( Texture2D& _StagingCubeMap, U32 _ProbeID, U32 _ProbesCount );

	// Saves the resulting encoded probe to disk
	void	Save( const char* _FileName ) const;

	// Saves a debugging structure of all the pixels and surfaces
	void	SavePixels( const char* _FileName ) const;


private:
	// Reads back the cube map and populates cube map pixels, probe pixels and scene pixels.
	// After this, the probe is ready for encoding
	void	ReadBackProbeCubeMap( Texture2D& _StagingCubeMap );

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