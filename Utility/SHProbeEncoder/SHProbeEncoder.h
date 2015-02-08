//////////////////////////////////////////////////////////////////////////
// SH Probe Encoder
//
// Encodes a cube map containing albedo, normal, distance, material ID, static lighting and emissive material
//
// The purpose of this class is to analyze the rendering from a cube map probe to perform a grouping
//	of the pixels based on their position, normal and albedo to create a limited amount of patches that we'll
//	be able to replace by simple "disc surface elements" that can be lit with dynamic lights.
// (http://graphics.pixar.com/library/PointBasedGlobalIlluminationForMovieProduction/paper.pdf)
// 
// The pixels belonging to each patch/patch will be considered having the same albedo and will light the probe with
//	precomputed spherical harmonic coefficients, each pondered by the solid angle covered by the pixel from the direction
//	specific to the pixel.
//
// To create the patches, I'm using a "Filling method", it's an experimental method of mine that consists in browsing
//	the pixels of the cube map and perform a fill operation by joining adjacent pixels if and only if they're sufficiently
//	close enough in terms of distance, normal and color.
// Each patch created this way has its own list of pixels removed from the global list of free pixels, pixels whose solid angle
//	is too low are discarded.
// The algorithm continues until all pixels have been discarded or added to a patch, then the algorithm enters a second
//	phase of optimization where sets are merged together if sufficiently close, or discarded if not significant enough.
//
//
#pragma once


class	SHProbeEncoder
{
private:	// CONSTANTS

	static const float	Z_INFINITY;
	static const float	Z_INFINITY_TEST;


private:	// NESTED TYPES

	class	Patch;

	// This represents all the information about the pixel of a cube map
	class	Pixel {
		static const double		f0;
		static const double		f1;
		static const double		f2;
		static const double		f3;

		static float	IMPORTANCE_THRESOLD;	// To be set manually before encoding

	public:
		int			Index;					// Index of the pixel in the scene pixels (can help us locate the cube map face + position of the pixel when finding adjacent pixels)
		int			CubeFaceIndex;
		int			CubeFaceX;
		int			CubeFaceY;

		float3		Position;				// World position
		float3		Normal;					// World normal
		float3		Albedo;					// Material albedo
		float3		AlbedoHSL;				// Material albedo in HSL format
		float		F0;						// Material Fresnel coefficient
		float3		StaticLitColor;			// Color of the statically lit environment
		U32			FaceIndex;				// Absolute scene face index
		int			EmissiveMatID;			// ID of the emissive material or -1 if not emissive
		int			NeighborProbeID;		// ID of the nearest neighbor probe
		float		NeighborProbeDistance;	// Distance to the neighbor probe plane
		double		SolidAngle;				// Solid angle covered by the pixel
		double		Importance;				// A measure of "importance" of the scene pixel = -dot( View, Normal ) / Distance²
		float		Distance;				// Distance to hit point
		bool		Infinity;				// True if not a scene pixel (i.e. sky pixel)
		float3		View;					// View vector pointing to that pixel

		double		SHCoeffs[9];

		Patch*		pParentPatch;
		int			ParentPatchSampleIndex;	// Index of the nearest sample this pixel is part of

		Pixel()
			: EmissiveMatID( -1 )
			, NeighborProbeID( -1 )
			, ParentPatchSampleIndex( -1 ) {
		}


		void			InitSH()
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

		void			SetAlbedo( float _R, float _G, float _B )
		{
			Albedo.Set( _R, _G, _B );

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
		float	ComputeMetric( const Pixel& _Other, float _PositionDistanceWeight, float _NormalDistanceWeight, float _AlbedoDistanceWeight )
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
		//	_ doesn't already belong to a patch
		//	_ is a scene pixel (i.e. not at infinity)
		//	_ has enough importance
		bool		IsFloodFillAcceptable()
		{
			if ( pParentPatch != NULL )
				return false;	// We don't accept pixels that are already part of a patch!
			if ( EmissiveMatID != -1 )
				return true;	// Accept all emissive pixels no matter what!
			if ( Infinity )
				return false;	// We only accept scene pixels!
			if ( Importance < IMPORTANCE_THRESOLD )
				return false;	// Not important enough!

			return true;
		}
	};

	// A patch is a special surface with a centroid, a normal and an average albedo
	// It also serves as accumulator for all pixels registering to the patch
//	[System.Diagnostics.DebuggerDisplay( "C={SetPixels.Count} I={Importance} H={AlbedoHSL.x} S={AlbedoHSL.y} L={Albedo.z} P=({Position.x}, {Position.y}, {Position.z})" )]
	class	Patch//, IComparer<Pixel>
	{
	public:
		List<Pixel*>	Pixels;			// The list of pixels belonging to this patch
		int				ID;				// Warning: Only available once the computation is over and all sets have been resolved!

		// Tangent space generated from principal directions of the points patch
		float3			Normal;
		float3			Tangent;
		float3			BiTangent;

		float3			Albedo;

		// The generated SH coefficients for this patch
		float3			SH[9];

		// The generated samples that will be used at runtime to estimate lighting and update the patch's SH coefficients
		struct Sample 
		{
			float3	Position;
			float3	Normal;
			float	Radius;		// Radius of the disc encompassing all the pixels forming that sample
		};
		Sample			Samples[16];


//static readonly float	FILTER_WINDOW_SIZE = 3.0f;	// Our SH order is 3 so...

	public:

		Patch()
			: ID( -1 )
			, Normal( float3::Zero )
			, Tangent( float3::Zero )
			, BiTangent( float3::Zero ) {}

		// Performs the SH encoding of all the pixels belonging to the patch
		// It simply amounts to summing the directional SH contribution of every pixels in the patch, assuming they all receive the energy received by the patch's substitute center
		void			EncodeSH()
		{
			double		AlbedoR = Albedo.x / PI;
			double		AlbedoG = Albedo.y / PI;
			double		AlbedoB = Albedo.z / PI;

			double		SHR[9];
			double		SHG[9];
			double		SHB[9];
			for ( int i=0; i < 9; i++ ) {
				SHR[i] = SHG[i] = SHB[i] = 0.0;
			}

			int		PixelsCount = Pixels.GetCount();
			for ( int i=0; i < PixelsCount; i++ ) {
				Pixel&	P = *Pixels[i];

				// Compute weight factor based on patch's normal and pixel's normal but also based on pixel's normal and view direction
				double	Factor  = P.SolidAngle								// Solid angle for SH weight, obvious
								* max( 0.0, P.Normal.Dot( Normal ) )		// This weight is to account for the fact that the point is well aligned with the patch's plane the lighting was computed for
								* max( 0.0, -P.View.Dot( P.Normal ) );		// This weight is to account for the fact that the point is well aligned with the view vector
																			//	(for example, for a perfectly flat wall this weight will have the effect that points further from the probe's perpendicular will have less importance)

				for ( int i=0; i < 9; i++ )
				{
					SHR[i] += P.SHCoeffs[i] * Factor * AlbedoR;
					SHG[i] += P.SHCoeffs[i] * Factor * AlbedoG;
					SHB[i] += P.SHCoeffs[i] * Factor * AlbedoB;
				}
			}

//DON'T NORMALIZE	=> 4PI is part of the integral!!!
//				double	Normalizer = 1.0 / (4.0 * Math.PI);
			double	Normalizer = 1.0;
			for ( int i=0; i < 9; i++ )
				SH[i].Set( float(Normalizer * SHR[i]), float(Normalizer * SHG[i]), float(Normalizer * SHB[i]) );

			// Apply filtering
//			SphericalHarmonics.SHFunctions.FilterLanczos( SH, FILTER_WINDOW_SIZE );
		}

		// Performs the emissive SH encoding of all the pixels belonging to the patch
		// It simply amounts to summing the directional SH contribution of every pixels in the patch
		void			EncodeEmissiveSH() {
			double	SHCoeffs[9];

			int		PixelsCount = Pixels.GetCount();
			for ( int i=0; i < PixelsCount; i++ ) {
				Pixel&	P = *Pixels[i];
				for ( int i=0; i < 9; i++ )
					SHCoeffs[i] += P.SHCoeffs[i] * P.SolidAngle;
			}

//DON'T NORMALIZE	=> 4PI is part of the integral!!!
//				double	Normalizer = 1.0 / (4.0 * Math.PI);
			double	Normalizer = 1.0;
			for ( int i=0; i < 9; i++ )
				SH[i] = (float) (Normalizer * SHCoeffs[i]) * float3::One;

// 			// Apply filtering
// //				SphericalHarmonics.SHFunctions.FilterLanczos( FILTER_WINDOW_SIZE );
// //				SphericalHarmonics.SHFunctions.FilterGaussian( FILTER_WINDOW_SIZE );	// Smoothes A LOT but according to the source of filters code, it's better if using HDR light sources
// 			SphericalHarmonics.SHFunctions.FilterHanning( SH, FILTER_WINDOW_SIZE );
		}

		// This is a very simplistic approach to determine the principal axes of the patch:
		//  1) Create a dummy tangent space for the patch's plane
		//  2) Rotate an axis from 0 to 180° in that arbitrary tangent space
		//		2.1) Compute the max distance of each point of the patch to this axis (i.e. bounding rect extent in that direction)
		//		2.2) Keep the angle where we find the largest distance as our minor principal axis
		//		2.3) Keep the angle where we find the smallest distance as our major principal axis
		//
// 		void			FindPrincipalAxes() {
// 			// Create an arbitrary tangent space
// 			float3	Y = Normal;
// 			float3	X = float3.UnitY ^ Y;
// 			float3	Z;
// 			if ( X.Length < 1e-6 )
// 			{	// Invalid basis!
// 				X = float3.UnitX;
// 				Z = float3.UnitZ;
// 			}
// 			else
// 			{
// 				X.Normalize();
// 				Z = X ^ Y;
// 			}
// 
// 			// Perform a rotation of a line from 0 to 180°
// 			float	MaxDistance = 0.0f;
// 			float	MaxDistanceAngle = 0.0f;
// 			float	MinDistance = 1e6f;
// 			float	MinDistanceAngle = 0.0f;
// 			for ( int Angle=0; Angle < 180; Angle+=2 )
// 			{
// 				float			fAngle = (float) Math.PI * Angle / 180.0f;
// 				float3	OrthoDir = (float) -Math.Sin( fAngle ) * X + (float) Math.Cos( fAngle ) * Z;	// This is actually the orthogonal direction to the line we're rotating
// 
// 				// Compute the max of distances from each point to this line
// 				float	Distance = 0.0f;
// 				foreach ( Pixel P in Pixels )
// 				{
// 					float3	Center2Pixel = P.Position - Position;
// 					float			Dot = Math.Abs( Center2Pixel | OrthoDir );	// Gives the distance to the closest point on the rotating line
// 					Distance = Math.Max( Distance, Dot );
// 				}
// 
// 				// Check for new best candidates
// 				if ( Distance > MaxDistance )
// 				{	// New best candidate for minor axis!
// 					MaxDistance = Distance;
// 					MaxDistanceAngle = fAngle;
// 				}
// 				if ( Distance < MinDistance )
// 				{	// New best candidate for major axis!
// 					MinDistance = Distance;
// 					MinDistanceAngle = fAngle;
// 				}
// 			}
// 
// 			// Finalize axes
// 			BiTangent = (float) Math.Cos( MaxDistanceAngle ) * X + (float) Math.Sin( MaxDistanceAngle ) * Z;	// Our minor axis
// 			Tangent = (float) Math.Cos( MinDistanceAngle ) * X + (float) Math.Sin( MinDistanceAngle ) * Z;		// Our major axis
// 
// 			float	Diff = Tangent | BiTangent;	// We'd prefer a diff of 0, meaning the found axes are orthogonal...
// 
// 			// Scale minor/major axes by these distances
// 			Tangent *= MaxDistance;
// 			BiTangent *= MinDistance;
// 		}

/*
		// Generates N samples among the patch's pixels where lighting will be sampled
		Pixel	__ReferencePixel;
		void			GenerateSamples( int _SamplesCount ) {
			Samples = new Sample[_SamplesCount];
			if ( _SamplesCount > Pixels.GetCount() )
				ASSERT( false, "More samples than pixels in the patch! This is useless!" );

			int	PixelGroupSize = max( 1, Pixels.GetCount() / _SamplesCount );
			for ( int SampleIndex=0; SampleIndex < _SamplesCount; SampleIndex++ ) {
				Pixel&	P = Pixels[SampleIndex * PixelGroupSize];	// Arbitrary!
// TODO: Choose well spaced pixels to cover the maximum area for this patch!

				// Find the nearest pixels around that pixel
				List<Pixel>	NearestPixels;
				NearestPixels.AddRange( Pixels );
				__ReferencePixel = P;
				NearestPixels.Sort( this );	// Our comparer will sort by dot product of the view vector with the reference pixel's view vector, "efficiently" grouping pixels as disks
				NearestPixels.RemoveRange( PixelGroupSize, NearestPixels.Count-PixelGroupSize );	// Remove all pixels outside of the group

				// Compute an average normal & the disc radius (i.e. farthest pixel from reference)
				float			Radius = 0.0f;
				float3	AverageNormal = float3.Zero;
				foreach ( Pixel P2 in NearestPixels )
				{
					AverageNormal += P2.Normal;

					float	Distance = (P2.Position - P.Position).Length;
					Radius = Math.Max( Radius, Distance );
				}
				AverageNormal /= NearestPixels.Count;

				// Store sample
				Samples[SampleIndex].Position = P.Position;
				Samples[SampleIndex].Normal = AverageNormal.Normalized;
				Samples[SampleIndex].Radius = Radius;
			}

			// Associate pixels to their closest sample
			foreach ( Pixel P in Pixels )
			{
				float	ClosestDistance = 1e6f;
				int		ClosestSampleIndex = -1;
				for ( int SampleIndex=0; SampleIndex < Samples.Length; SampleIndex++ )
				{
					float	Distance2Sample = (P.Position - Samples[SampleIndex].Position).LengthSquare;
					if ( Distance2Sample >= ClosestDistance )
						continue;

					ClosestDistance = Distance2Sample;
					ClosestSampleIndex = SampleIndex;
				}

				P.ParentPatchSampleIndex = ClosestSampleIndex;
			}
		}

// 		#region IComparer<Pixel> Members
// 
// 		/// <summary>
// 		/// Compare best orientation toward reference pixel, this way we group pixels in a disk around our reference pixel
// 		/// </summary>
// 		/// <param name="x"></param>
// 		/// <param name="y"></param>
// 		/// <returns></returns>
// 		public int Compare( Pixel x, Pixel y )
// 		{
// 			float	DotX = x.View | __ReferencePixel.View;
// 			float	DotY = y.View | __ReferencePixel.View;
// 			return DotX > DotY ? -1 : (DotX < DotY ? 1 : 0);
// 		}
// 
// 		#endregion
*/
	};

	// Contains information on a neighbor probe
//	[System.Diagnostics.DebuggerDisplay( "ID={ProbeID} Dist={Distance} Omega={SolidAngle} Dir=({Direction.x}, {Direction.y}, {Direction.z})" )]
	class	NeighborProbe {
	public:
		int			ProbeID;
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


private:	// FIELDS

	float4x4			m_Side2World[6];

	int					m_ProbeID;							// This is extracted from the cube map file name... Not very robust but good enough!

	Pixel*				m_CubeMapPixels;					// Original cube map
	List<Pixel*>		m_ProbePixels;						// List of all pixels in the probe
	List<Pixel*>		m_ScenePixels;						// List of pixels that participate to the scene (i.e. not at infinity)

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

	// Dynamic & Emissive sets
	Patch*				m_Patches;
	Patch*				m_EmissivePatches;

	float3				m_SHSumDynamic[9];
	float3				m_SHSumEmissive[9];

	// List of neighbor probes
	float				m_NearestNeighborProbeDistance;
	float				m_FarthestNeighborProbeDistance;
	List<NeighborProbe>	m_NeighborProbes;

	// List of influence weights per face index
	U32					m_MaxFaceIndex;
	Dictionary<double>	m_ProbeInfluencePerFace;


public:		// PROPERTIES

public:		// METHODS

	SHProbeEncoder();
	~SHProbeEncoder();

	// Encodes the MRT cube map into basic SH elements that can later be combined at runtime to form a dynamically updatable probe
	void	EncodeProbeCubeMap( Texture2D& _StagingCubeMap, U32 _ProbeID );
};