using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace ProbeSHEncoder
{
	public class	Probe : IComparer<Probe.Set>, IComparer<Probe.NeighborProbe>
	{
		#region CONSTANTS

		public const int	CUBE_MAP_SIZE = 128;
		public const float	Z_INFINITY = 1e6f;
		public const float	Z_INFINITY_TEST = 0.99f * Z_INFINITY;

		#endregion

		#region NESTED TYPES

		/// <summary>
		/// This represents all the informations about the pixels viewed by a probe (i.e. cube map)
		/// </summary>
		[System.Diagnostics.DebuggerDisplay( "H={AlbedoHSL.x} S={AlbedoHSL.y} L={Albedo.z} P=({Position.x}, {Position.y}, {Position.z})" )]
		public class	Pixel
		{
			static readonly double		f0 = 0.5 / Math.Sqrt(Math.PI);
			static readonly double		f1 = Math.Sqrt(3.0) * f0;
			static readonly double		f2 = Math.Sqrt(15.0) * f0;
			static readonly double		f3 = Math.Sqrt(5.0) * 0.5 * f0;

			public int			PixelIndex;			// Index of the pixel in the scene pixels (can help us locate the cube map face + position of the pixel when finding adjacent pixels)
			public int			CubeFaceIndex;
			public int			CubeFaceX;
			public int			CubeFaceY;

			public WMath.Point	Position;				// World position
			public WMath.Vector	Normal;					// World normal
			public WMath.Vector	Albedo;					// Material albedo
			public WMath.Vector	AlbedoHSL;				// Material albedo in HSL format
			public float		F0;						// Material Fresnel coefficient
			public WMath.Vector	StaticLitColor;			// Color of the statically lit environment
			public UInt32		FaceIndex;				// Absolute scene face index
			public int			EmissiveMatID = -1;		// ID of the emissive material or -1 if not emissive
			public bool			IsEmissive	{ get { return EmissiveMatID != -1; } }
			public int			NeighborProbeID = -1;	// ID of the nearest neighbor probe
			public float		NeighborProbeDistance;	// Distance to the neighbor probe plane
			public double		SolidAngle;				// Solid angle covered by the pixel
			public double		Importance;				// A measure of "importance" of the scene pixel = -dot( View, Normal ) / Distance²
			public float		Distance;				// Distance to hit point
			public bool			Infinity;				// True if not a scene pixel (i.e. sky pixel)
			public WMath.Vector	View;					// View vector pointing to that pixel

			public double[]		SHCoeffs = new double[9];

			public Set			ParentSet = null;
			public int			ParentSetSampleIndex = -1;	// Index of the nearest sample this pixel is part of

			public static float	IMPORTANCE_THRESOLD = 0.0f;

			public void			InitSH()
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

			public void			SetAlbedo( WMath.Vector _Albedo )
			{
				Albedo = _Albedo;

				// Convert into HSL
				float	Min = Math.Min( Math.Min( _Albedo.x, _Albedo.y ), _Albedo.z );
				float	Max = Math.Max( Math.Max( _Albedo.x, _Albedo.y ), _Albedo.z );
				float	Delta = Max - Min;

				float	L = 0.5f * (Max + Min);
				float	S = Delta;
				float	H = 0;
				if ( Delta > 0 )
				{
					S /= L < 0.5f ? 2.0f * L : 2.0f * (1.0f - L);
					if ( Max == _Albedo.x )
						H = (_Albedo.y - _Albedo.z) / Delta;
					else if ( Max == _Albedo.y )
						H = 2.0f + (_Albedo.z - _Albedo.x) / Delta;
					else if ( Max == _Albedo.z )
						H = 4.0f + (_Albedo.x - _Albedo.y) / Delta;
					else
						throw new Exception( "Rha!" );

					H = (H + 6) % 6.0f;
				}

// DEBUG => Should stop on saturated colors like the red/blue side walls
// if ( S > 0.5f )
// 	S += 1e-6f;

				AlbedoHSL = new WMath.Vector( H, S, L );
			}

			/// <summary>
			/// Computes a "distance" between this pixel and another one
			/// </summary>
			/// <param name="_Other"></param>
			/// <returns></returns>
			public float	ComputeMetric( Pixel _Other, float _PositionDistanceWeight, float _NormalDistanceWeight, float _AlbedoDistanceWeight )
			{
				float	EuclidianDistance = (_Other.Position - Position).Length;
						EuclidianDistance *= _PositionDistanceWeight;

				float	NormalDistance = 0.5f * (1.0f - (_Other.Normal | Normal));
						NormalDistance *= _NormalDistanceWeight;	// Used to give the normal as much weight as general euclidian distances...

				float	ColorDistance0 = Math.Abs( AlbedoHSL.x - _Other.AlbedoHSL.x );
				float	ColorDistance1 = 6.0f - ColorDistance0;
				float	ColorDistance = Math.Min( ColorDistance0, ColorDistance1 ) / 6.0f;
						ColorDistance *= _AlbedoDistanceWeight;		// Used to give the color as much weight as general euclidian distances...

				return EuclidianDistance + NormalDistance + ColorDistance;
			}

			/// <summary>
			/// Tells if the pixel is acceptable on its own.
			/// The test checks if the pixel:
			///		_ doesn't already belong to a set
			///		_ is a scene pixel (i.e. not at infinity)
			///		_ has enough importance
			/// </summary>
			/// <returns></returns>
			public bool		IsFloodFillAcceptable()
			{
				if ( ParentSet != null )
					return false;	// We don't accept pixels that are already part of a set!
				if ( IsEmissive )
					return true;	// Accept all emissive pixels no matter what!
				if ( Infinity )
					return false;	// We only accept scene pixels!
				if ( Importance < IMPORTANCE_THRESOLD )
					return false;	// Not important enough!

				return true;
			}
		}

		/// <summary>
		/// A set is a special pixel with a centroid, a normal and an average albedo
		/// It also serves as accumulator for all pixels registering to the set
		/// </summary>
		[System.Diagnostics.DebuggerDisplay( "C={SetPixels.Count} I={Importance} H={AlbedoHSL.x} S={AlbedoHSL.y} L={Albedo.z} P=({Position.x}, {Position.y}, {Position.z})" )]
		public class	Set : Pixel, IComparer<Pixel>
		{
			public List<Pixel>		SetPixels = new List<Pixel>();	// The list of pixels belonging to this set
			public int				SetIndex = -1;					// Warning: Only available once the computation is over and all sets have been resolved!

			// Tangent space generated from principal directions of the points set
			public WMath.Vector		Tangent = WMath.Vector.Zero;
			public WMath.Vector		BiTangent = WMath.Vector.Zero;

			// The generated SH coefficients for this set
			public WMath.Vector[]	SH = new WMath.Vector[9];

			// The generated samples
			public struct Sample 
			{
				public WMath.Point	Position;
				public WMath.Vector	Normal;
				public float		Radius;		// Radius of the disc encompassing all the pixels forming that sample
			}
			public Sample[]			Samples = new Sample[0];

// Used by k-means method to determine average set centroid/normal
public WMath.Vector		AccumCentroid = WMath.Vector.Zero;
public WMath.Vector		AccumNormal = WMath.Vector.Zero;


static readonly int	FILTER_WINDOW_SIZE = 3;	// Our SH order is 3 so...


			/// <summary>
			/// Performs the SH encoding of all the pixels belonging to the set
			/// It simply amounts to summing the directional SH contribution of every pixels in the set, assuming they
			///  all receive the energy received by the set's substitute center
			/// </summary>
			public void			EncodeSH()
			{
				double		AlbedoR = Albedo.x / Math.PI;
				double		AlbedoG = Albedo.y / Math.PI;
				double		AlbedoB = Albedo.z / Math.PI;

				double[]	SHR = new double[9];
				double[]	SHG = new double[9];
				double[]	SHB = new double[9];

				foreach ( Pixel P in SetPixels )
				{
					// Compute weight factor based on set's normal and pixel's normal but also based on pixel's normal and view direction
					double	Factor  = P.SolidAngle									// Solid angle for SH weight, obvious
									* Math.Max( 0.0, (P.Normal | this.Normal) )		// This weight is to account for the fact that the point is well aligned with the set's plane the lighting was computed for
									* Math.Max( 0.0, -(P.View | P.Normal) );		// This weight is to account for the fact that the point is well aligned with the view vector
																					//	(for example, for a perfectly flat wall this weight will have the effect that points further from the probe's perpendicular will have less importance)

					for ( int i=0; i < 9; i++ )
					{
						SHR[i] += P.SHCoeffs[i] * Factor * AlbedoR;
						SHG[i] += P.SHCoeffs[i] * Factor * AlbedoG;
						SHB[i] += P.SHCoeffs[i] * Factor * AlbedoB;
					}
				}

//DON'T NORMALIZE	=> 4PI is part of the integral!!!
				double	Normalizer = 1.0 / (4.0 * Math.PI);
				for ( int i=0; i < 9; i++ )
					SH[i] = new WMath.Vector( (float) (Normalizer * SHR[i]), (float) (Normalizer * SHG[i]), (float) (Normalizer * SHB[i]) );

				// Apply filtering
				SphericalHarmonics.SHFunctions.FilterLanczos( SH, FILTER_WINDOW_SIZE );
			}

			/// <summary>
			/// Performs the emissive SH encoding of all the pixels belonging to the set
			/// It simply amounts to summing the directional SH contribution of every pixels in the set
			/// </summary>
			public void			EncodeEmissiveSH()
			{
				double[]	SHCoeffs = new double[9];

				foreach ( Pixel P in SetPixels )
					for ( int i=0; i < 9; i++ )
						SHCoeffs[i] += P.SHCoeffs[i] * P.SolidAngle;

//DON'T NORMALIZE	=> 4PI is part of the integral!!!
				double	Normalizer = 1.0 / (4.0 * Math.PI);
				for ( int i=0; i < 9; i++ )
					SH[i] = (float) (Normalizer * SHCoeffs[i]) * WMath.Vector.One;

				// Apply filtering
//				SphericalHarmonics.SHFunctions.FilterLanczos( FILTER_WINDOW_SIZE );
//				SphericalHarmonics.SHFunctions.FilterGaussian( FILTER_WINDOW_SIZE );	// Smoothes A LOT but according to the source of filters code, it's better if using HDR light sources
				SphericalHarmonics.SHFunctions.FilterHanning( SH, FILTER_WINDOW_SIZE );
			}

			/// <summary>
			/// This is a very simplistic approach to determine the principal axes of the set:
			///  1) Create a dummy tangent space for the set's plane
			///  2) Rotate an axis from 0 to 180° in that arbitrary tangent space
			///		2.1) Compute the max distance of each point of the set to this axis (i.e. bounding rect extent in that direction)
			///		2.2) Keep the angle where we find the largest distance as our minor principal axis
			///		2.3) Keep the angle where we find the smallest distance as our major principal axis
			/// </summary>
			public void			FindPrincipalAxes()
			{
				// Create an arbitrary tangent space
				WMath.Vector	Y = Normal;
				WMath.Vector	X = WMath.Vector.UnitY ^ Y;
				WMath.Vector	Z;
				if ( X.Length < 1e-6 )
				{	// Invalid basis!
					X = WMath.Vector.UnitX;
					Z = WMath.Vector.UnitZ;
				}
				else
				{
					X.Normalize();
					Z = X ^ Y;
				}

				// Perform a rotation of a line from 0 to 180°
				float	MaxDistance = 0.0f;
				float	MaxDistanceAngle = 0.0f;
				float	MinDistance = 1e6f;
				float	MinDistanceAngle = 0.0f;
				for ( int Angle=0; Angle < 180; Angle+=2 )
				{
					float			fAngle = (float) Math.PI * Angle / 180.0f;
					WMath.Vector	OrthoDir = (float) -Math.Sin( fAngle ) * X + (float) Math.Cos( fAngle ) * Z;	// This is actually the orthogonal direction to the line we're rotating

					// Compute the max of distances from each point to this line
					float	Distance = 0.0f;
					foreach ( Pixel P in SetPixels )
					{
						WMath.Vector	Center2Pixel = P.Position - Position;
						float			Dot = Math.Abs( Center2Pixel | OrthoDir );	// Gives the distance to the closest point on the rotating line
						Distance = Math.Max( Distance, Dot );
					}

					// Check for new best candidates
					if ( Distance > MaxDistance )
					{	// New best candidate for minor axis!
						MaxDistance = Distance;
						MaxDistanceAngle = fAngle;
					}
					if ( Distance < MinDistance )
					{	// New best candidate for major axis!
						MinDistance = Distance;
						MinDistanceAngle = fAngle;
					}
				}

				// Finalize axes
				BiTangent = (float) Math.Cos( MaxDistanceAngle ) * X + (float) Math.Sin( MaxDistanceAngle ) * Z;	// Our minor axis
				Tangent = (float) Math.Cos( MinDistanceAngle ) * X + (float) Math.Sin( MinDistanceAngle ) * Z;		// Our major axis

				float	Diff = Tangent | BiTangent;	// We'd prefer a diff of 0, meaning the found axes are orthogonal...

				// Scale minor/major axes by these distances
				Tangent *= MaxDistance;
				BiTangent *= MinDistance;
			}

			/// <summary>
			/// Generates N samples among the set's pixels where lighting will be sampled
			/// </summary>
			private Pixel	__ReferencePixel;
			public void			GenerateSamples( int _SamplesCount )
			{
				Samples = new Sample[_SamplesCount];
				if ( _SamplesCount > SetPixels.Count )
					throw new Exception( "More samples than pixels in the set! This is useless!" );

				int	PixelGroupSize = Math.Max( 1, SetPixels.Count / _SamplesCount );
				for ( int SampleIndex=0; SampleIndex < _SamplesCount; SampleIndex++ )
				{
					Pixel	P = SetPixels[SampleIndex * PixelGroupSize];	// Arbitrary!
// TODO: Choose well spaced pixels to cover the maximum area for this set!

					// Find the nearest pixels around that pixel
					List<Pixel>	NearestPixels = new List<Pixel>();
					NearestPixels.AddRange( SetPixels );
					__ReferencePixel = P;
					NearestPixels.Sort( this );	// Our comparer will sort by dot product of the view vector with the reference pixel's view vector, "efficiently" grouping pixels as disks
					NearestPixels.RemoveRange( PixelGroupSize, NearestPixels.Count-PixelGroupSize );	// Remove all pixels outside of the group

					// Compute an average normal & the disc radius (i.e. farthest pixel from reference)
					float			Radius = 0.0f;
					WMath.Vector	AverageNormal = WMath.Vector.Zero;
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
				foreach ( Pixel P in SetPixels )
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

					P.ParentSetSampleIndex = ClosestSampleIndex;
				}
			}

			#region IComparer<Pixel> Members

			/// <summary>
			/// Compare best orientation toward reference pixel, this way we group pixels in a disk around our reference pixel
			/// </summary>
			/// <param name="x"></param>
			/// <param name="y"></param>
			/// <returns></returns>
			public int Compare( Pixel x, Pixel y )
			{
				float	DotX = x.View | __ReferencePixel.View;
				float	DotY = y.View | __ReferencePixel.View;
				return DotX > DotY ? -1 : (DotX < DotY ? 1 : 0);
			}

			#endregion
		}

		/// <summary>
		/// Contains information on a neighbor probe
		/// </summary>
		[System.Diagnostics.DebuggerDisplay( "ID={ProbeID} Dist={Distance} Omega={SolidAngle} Dir=({Direction.x}, {Direction.y}, {Direction.z})" )]
		public class	NeighborProbe
		{
			public int				ProbeID = -1;
			public float			Distance = 0.0f;				// Distance to the neighbor probe
			public double			SolidAngle = 0.0f;				// Solid angle covered by the neighbor probe, as perceived by our probe
			public WMath.Vector		Direction = WMath.Vector.Zero;	// Direction to the probe
			public double[]			SH = new double[9];				// SH coefficients used to isolate the probe's contribution to our probe

			public int				PixelsCount;
		}

		#endregion

		#region FIELDS

		public WMath.Matrix4x4[]	m_Side2World = new WMath.Matrix4x4[6];

		public int					m_ProbeID = -1;						// This is extracted from the cube map file name... Not very robust but good enough!

		public Pixel[][,]			m_CubeMap = null;					// Original cube map
		public List<Pixel>			m_ProbePixels = new List<Pixel>();	// List of all pixels in the probe
		public List<Pixel>			m_ScenePixels = new List<Pixel>();	// List of pixels that participate to the scene (i.e. not at infinity)

		// Generated geometric informations
		public double				m_MeanDistance = 0.0;
		public double				m_MeanHarmonicDistance = 0.0;
		public double				m_MinDistance = 0.0;
		public double				m_MaxDistance = 0.0;
		public WMath.BoundingBox	m_BBox = new WMath.BoundingBox();

		// Static and occlusion SH
		public WMath.Vector[]		m_StaticSH = new WMath.Vector[9];
		public float[]				m_OcclusionSH = new float[9];

		// Dynamic & Emissive sets
		public Set[]				m_Sets = new Set[0];
		public Set[]				m_EmissiveSets = new Set[0];

		public WMath.Vector[]		m_SHSumDynamic = new WMath.Vector[9];
		public WMath.Vector[]		m_SHSumEmissive = new WMath.Vector[9];

		// List of neighbor probes
		public float				m_NearestNeighborProbeDistance = 0.0f;
		public float				m_FarthestNeighborProbeDistance = 0.0f;
		public List<NeighborProbe>	m_NeighborProbes = new List<NeighborProbe>();

		// List of influence weights per face index
		public UInt32				m_MaxFaceIndex = 0;
		public Dictionary<UInt32,double>	m_ProbeInfluencePerFace = new Dictionary<UInt32,double>();

		#endregion

		#region METHODS

		public	Probe()
		{
			PrepareCubeMapFaceTransforms();
		}

		public void	LoadCubeMap( FileInfo _POMCubeMap )
		{
			// Load and convert POM files into a useable cube map
			Nuaj.Cirrus.Utility.TextureFilePOM	POM = new Nuaj.Cirrus.Utility.TextureFilePOM();
			POM.Load( _POMCubeMap );
			if ( POM.m_ArraySizeOrDepth < 6 )
				throw new Exception( "Provided POM file is not a cube map!" );
			if ( POM.m_Width != CUBE_MAP_SIZE || POM.m_Height != CUBE_MAP_SIZE || POM.m_ArraySizeOrDepth != 4*6 )
				throw new Exception( "Unexpected cube map size!" );

			// Extract probe ID from the file name
			string	CubeMapName = Path.GetFileNameWithoutExtension( _POMCubeMap.FullName );
			int		LastDigitIndex = CubeMapName.Length-1;
			while ( CubeMapName[LastDigitIndex] >= '0' && CubeMapName[LastDigitIndex] <= '9' && LastDigitIndex > 0 )
			{
				LastDigitIndex--;
			}
			CubeMapName = CubeMapName.Substring( LastDigitIndex+1 );
			if ( !int.TryParse( CubeMapName, out m_ProbeID ) )
				throw new Exception( "Can't retrieve probe ID from cube map file name!" );

			m_CubeMap = new Pixel[6][,];
			m_ProbePixels.Clear();
			m_ScenePixels.Clear();

			double	dA = 4.0 / (CUBE_MAP_SIZE*CUBE_MAP_SIZE);	// Cube face is supposed to be in [-1,+1], yielding a 2x2 square units
			double	SumSolidAngle = 0.0;

			m_MeanDistance = 0.0;
			m_MeanHarmonicDistance = 0.0;
			m_MinDistance = 1e6;
			m_MaxDistance = 0.0;
			m_BBox = WMath.BoundingBox.Empty;

			int		NegativeImportancePixelsCount = 0;
			for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
			{
				m_CubeMap[CubeFaceIndex] = new Pixel[CUBE_MAP_SIZE,CUBE_MAP_SIZE];
				for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
					for ( int X=0; X < CUBE_MAP_SIZE; X++ )
						m_CubeMap[CubeFaceIndex][X,Y] = new Pixel() { PixelIndex = X + CUBE_MAP_SIZE * (Y + CUBE_MAP_SIZE * CubeFaceIndex), CubeFaceIndex = CubeFaceIndex, CubeFaceX = X, CubeFaceY = Y };

				// Fill up albedo
				using ( MemoryStream S = new MemoryStream( POM.m_Content[CubeFaceIndex] ) )
					using ( BinaryReader R = new BinaryReader( S ) )
						for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
							for ( int X=0; X < CUBE_MAP_SIZE; X++ )
							{
								Pixel	Pix = m_CubeMap[CubeFaceIndex][X,Y];

								float	Red = R.ReadSingle();
								float	Green = R.ReadSingle();
								float	Blue = R.ReadSingle();
									
								Pix.FaceIndex = R.ReadUInt32();

								// Work with better precision
								Red *= (float) Math.PI;
								Green *= (float) Math.PI;
								Blue *= (float) Math.PI;

								Pix.SetAlbedo( new WMath.Vector( Red, Green, Blue ) );
							}

				// Fill up static lit environment and emissive IDs
				using ( MemoryStream S = new MemoryStream( POM.m_Content[6*2+CubeFaceIndex] ) )
					using ( BinaryReader R = new BinaryReader( S ) )
						for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
							for ( int X=0; X < CUBE_MAP_SIZE; X++ )
							{
								Pixel	Pix = m_CubeMap[CubeFaceIndex][X,Y];

								float	Red = R.ReadSingle();
								float	Green = R.ReadSingle();
								float	Blue = R.ReadSingle();
								int		ID = (int) R.ReadUInt32();

								Pix.StaticLitColor = new WMath.Vector( Red, Green, Blue );
								Pix.EmissiveMatID = ID;
							}

				// Fill up position & normal
				using ( MemoryStream S = new MemoryStream( POM.m_Content[6*1+CubeFaceIndex] ) )
					using ( BinaryReader R = new BinaryReader( S ) )
						for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
							for ( int X=0; X < CUBE_MAP_SIZE; X++ )
							{
								Pixel			Pix = m_CubeMap[CubeFaceIndex][X,Y];

								WMath.Vector	csView = new WMath.Vector( 2.0f * (0.5f + X) / CUBE_MAP_SIZE - 1.0f, 1.0f - 2.0f * (0.5f + Y) / CUBE_MAP_SIZE, 1.0f );
								float			Distance2Texel = csView.Length;
												csView /= Distance2Texel;
								WMath.Vector	wsView = csView * m_Side2World[CubeFaceIndex];

								// Retrieve the cube map texel's solid angle (from http://people.cs.kuleuven.be/~philip.dutre/GI/TotalCompendium.pdf)
								// dw = cos(Theta).dA / r²
								// cos(Theta) = Adjacent/Hypothenuse = 1/r
								//
								double	SolidAngle = dA / (Distance2Texel * Distance2Texel * Distance2Texel);
								SumSolidAngle += SolidAngle;

								float	Nx = R.ReadSingle();
								float	Ny = R.ReadSingle();
								float	Nz = R.ReadSingle();
								float	Distance = R.ReadSingle();

								WMath.Point		wsPosition = new WMath.Point( Distance * wsView.x, Distance * wsView.y, Distance * wsView.z );

								Pix.Position = wsPosition;
								Pix.View = wsView;
								Pix.Normal = new WMath.Vector( Nx, Ny, Nz ).Normalized;
								Pix.SolidAngle = SolidAngle;
								Pix.Importance = -(wsView | Pix.Normal) / (Distance * Distance);
								if ( Pix.Importance < 0.0 )
								{
Pix.Normal = -Pix.Normal;
Pix.Importance = -(wsView | Pix.Normal) / (Distance * Distance);
//Pix.Importance = -Pix.Importance;
NegativeImportancePixelsCount++;
//										throw new Exception( "WTH?? Negative importance here!" );
								}
								Pix.Distance = Distance;
								Pix.Infinity = Distance > Z_INFINITY_TEST;

								Pix.InitSH();

								m_ProbePixels.Add( Pix );
								if ( m_CubeMap[CubeFaceIndex][X,Y].Infinity )
									continue;

								// Account for a new scene pixel (i.e. not infinity)
								m_ScenePixels.Add( Pix );

								// Update dimensions
								m_MeanDistance += Distance;
								m_MeanHarmonicDistance += 1.0 / Distance;
								m_MinDistance = Math.Min( m_MinDistance, Distance );
								m_MaxDistance = Math.Max( m_MaxDistance, Distance );
								m_BBox.Grow( wsPosition );
							}

				// Fill up neighbor probes ID
				using ( MemoryStream S = new MemoryStream( POM.m_Content[6*3+CubeFaceIndex] ) )
					using ( BinaryReader R = new BinaryReader( S ) )
						for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
							for ( int X=0; X < CUBE_MAP_SIZE; X++ )
							{
								Pixel	Pix = m_CubeMap[CubeFaceIndex][X,Y];

								int		ID = (int) R.ReadUInt32();
								float	NeighborProbeDistance = R.ReadSingle();
								float	Blue = R.ReadSingle();
								float	Alpha = R.ReadSingle();

								Pix.NeighborProbeID = ID;
								Pix.NeighborProbeDistance = NeighborProbeDistance;
							}
			}

if ( (float) NegativeImportancePixelsCount / (CUBE_MAP_SIZE * CUBE_MAP_SIZE * 6) > 0.1f )
throw new Exception( "More than 10% invalid pixels with inverted normals in that probe!" );

			m_MeanDistance /= (CUBE_MAP_SIZE * CUBE_MAP_SIZE * 6);
			m_MeanHarmonicDistance = (CUBE_MAP_SIZE * CUBE_MAP_SIZE * 6) / m_MeanHarmonicDistance;
		}

		public void	SaveSets( FileInfo _FileName )
		{
			using ( FileStream Stream = _FileName.Create() )
				using ( BinaryWriter W = new BinaryWriter( Stream ) )
				{
					// Write the mean, harmonic mean, min, max distances
					W.Write( (float) m_MeanDistance );
					W.Write( (float) m_MeanHarmonicDistance );
					W.Write( (float) m_MinDistance );
					W.Write( (float) m_MaxDistance );

					// Write the BBox
					W.Write( m_BBox.m_Min.x );
					W.Write( m_BBox.m_Min.y );
					W.Write( m_BBox.m_Min.z );
					W.Write( m_BBox.m_Max.x );
					W.Write( m_BBox.m_Max.y );
					W.Write( m_BBox.m_Max.z );

					// Write static SH
					for ( int i=0; i < 9; i++ )
					{
						W.Write( m_StaticSH[i].x );
						W.Write( m_StaticSH[i].y );
						W.Write( m_StaticSH[i].z );
					}

					// Write occlusion SH
					for ( int i=0; i < 9; i++ )
						W.Write( m_OcclusionSH[i] );

					// Write the result sets
					W.Write( (UInt32) m_Sets.Length );

					foreach ( Set S in m_Sets )
					{
						// Write position, normal, albedo
						W.Write( S.Position.x );
						W.Write( S.Position.y );
						W.Write( S.Position.z );

						W.Write( S.Normal.x );
						W.Write( S.Normal.y );
						W.Write( S.Normal.z );

						W.Write( S.Tangent.x );
						W.Write( S.Tangent.y );
						W.Write( S.Tangent.z );

						W.Write( S.BiTangent.x );
						W.Write( S.BiTangent.y );
						W.Write( S.BiTangent.z );

							// Not used, just for information purpose
						W.Write( (float) (S.Albedo.x / Math.PI) );
						W.Write( (float) (S.Albedo.y / Math.PI) );
						W.Write( (float) (S.Albedo.z / Math.PI) );

						// Write SH coefficients (albedo is already factored in)
						for ( int i=0; i < 9; i++ )
						{
							W.Write( S.SH[i].x );
							W.Write( S.SH[i].y );
							W.Write( S.SH[i].z );
						}

						// Write amount of samples
						W.Write( (UInt32) S.Samples.Length );

						// Write each sample
						foreach ( Set.Sample Sample in S.Samples )
						{
							// Write position
							W.Write( Sample.Position.x );
							W.Write( Sample.Position.y );
							W.Write( Sample.Position.z );

							// Write normal
							W.Write( Sample.Normal.x );
							W.Write( Sample.Normal.y );
							W.Write( Sample.Normal.z );

							// Write radius
							W.Write( Sample.Radius );
						}
					}

					// Write the emissive sets
					W.Write( (UInt32) m_EmissiveSets.Length );

					foreach ( Set S in m_EmissiveSets )
					{
						// Write position, normal, albedo
						W.Write( S.Position.x );
						W.Write( S.Position.y );
						W.Write( S.Position.z );

						W.Write( S.Normal.x );
						W.Write( S.Normal.y );
						W.Write( S.Normal.z );

						W.Write( S.Tangent.x );
						W.Write( S.Tangent.y );
						W.Write( S.Tangent.z );

						W.Write( S.BiTangent.x );
						W.Write( S.BiTangent.y );
						W.Write( S.BiTangent.z );

						// Write emissive mat
						W.Write( S.EmissiveMatID );

						// Write SH coefficients (we only write luminance here, we don't have the color info that is provided at runtime)
						for ( int i=0; i < 9; i++ )
							W.Write( S.SH[i].x );
					}

					// Write the neighbor probes
					W.Write( (UInt32) m_NeighborProbes.Count );

					// Write nearest/farthest probe distance
					W.Write( m_NearestNeighborProbeDistance );
					W.Write( m_FarthestNeighborProbeDistance );

					foreach ( NeighborProbe NP in m_NeighborProbes )
					{
						// Write probe ID, distance, solid angle, direction
						W.Write( (UInt32) NP.ProbeID );
						W.Write( NP.Distance );
						W.Write( (float) NP.SolidAngle );
						W.Write( NP.Direction.x );
						W.Write( NP.Direction.y );
						W.Write( NP.Direction.z );

						// Write SH coefficients (only luminance here since they're used for the product with the neighbor probe's SH)
						for ( int i=0; i < 9; i++ )
							W.Write( (float) NP.SH[i] );
					}
				}

// We now save a single file
// 			// Save probe influence for each scene face
// 			FileInfo	InfluenceFileName = new FileInfo( Path.Combine( Path.GetDirectoryName( _FileName.FullName ), Path.GetFileNameWithoutExtension( _FileName.FullName ) + ".FaceInfluence" ) );
// 			using ( FileStream S = InfluenceFileName.Create() )
// 				using ( BinaryWriter W = new BinaryWriter( S ) )
// 				{
// 					for ( UInt32 FaceIndex=0; FaceIndex < m_MaxFaceIndex; FaceIndex++ )
// 					{
// 						W.Write( (float) (m_ProbeInfluencePerFace.ContainsKey( FaceIndex ) ? m_ProbeInfluencePerFace[FaceIndex] : 0.0) );
// 					}
// 				}
		}

		#region Computes k-Means Sets

		public void	ComputeKMeans( int K, float _SpatialDistanceWeight, float _NormalDistanceWeight, float _AlbedoDistanceWeight, float _Lambda )
		{
			const float	CHANGES_RATIO_THRESHOLD = 0.01f;	// We can exit the loop if less than 1% of the scene pixels changed of set during the last 2 loop iterations

			// Build a sequence of quasi-random vectors on a sphere using Hammersley distribution
			WMath.Hammersley	QRNG = new WMath.Hammersley();
			double[,]			HammersleySequence = QRNG.BuildSequence( K, 2 );
			WMath.Vector[]		InitialSetDirections = QRNG.MapSequenceToSphere( HammersleySequence, false );

			// Determine a cosine power to focus on points in the initial spherical zone
//			float	CosinePower = K - 1;	// A single point covers the entire sphere, we're assuming the lobe grows linearly with k...

			// cos(a)^n ~= exp( -n.a²/2 ) <= Check Toksvig
			// We expect exp( -n.a²/2 ) = epsilon for an angle a = 2/K (4PI/k sr / 2PI to account for full lobe)
			// We get n = -2.ln(epsilon)/(2/K)² = -K.ln(epsilon)/2
			const float	epsilon = 0.01f;
			float	CosinePower = -0.5f * K * (float) Math.Log( epsilon );

			// Determine K centroids by splitting the sphere
			List<Set>	Sets = new List<Set>();
			for ( int SetIndex=0; SetIndex < K; SetIndex++ )
			{
// 				// We first choose a "random" direction (actually, we're splitting the sphere into 8 equal parts) (an octahedron)
// 				float	Phi = 2.0f * (float) Math.PI * (SetIndex & 3) / 4.0f;
// 				float	Theta = (float) Math.PI * (0.5f + (SetIndex >> 2) ) / 2.0f;
// 
// 				WMath.Vector	TargetDirection = new WMath.Vector( 
// 						(float) (Math.Cos( Phi ) * Math.Sin( Theta )),
// 						(float) Math.Cos( Theta ),
// 						(float) (Math.Sin( Phi ) * Math.Sin( Theta )) );

				WMath.Vector	TargetDirection = InitialSetDirections[SetIndex];

				// Now we collect all the points that are the closest to the direction and keep the ones that have the most weight
				WMath.Vector	CentroidPosition = WMath.Vector.Zero;
				WMath.Vector	CentroidNormal = WMath.Vector.Zero;
				WMath.Vector	CentroidAlbedo = WMath.Vector.Zero;
				float			SumWeights = 0.0f;
				float			SumWeightColors = 0.0f;
				foreach ( Pixel P in m_ScenePixels )
				{
					float	WeightDirection = Math.Max( 0.0f, TargetDirection | P.View );

					WeightDirection = (float) Math.Pow( WeightDirection, CosinePower );

					float	WeightColor = 0.1f + 0.9f * P.AlbedoHSL.y;	// Favor the most saturated colors
					float	Weight = WeightDirection * WeightColor;

// DEBUG => Should stop on saturated colors, see how they shift the weight
if ( WeightColor > 0.5f )
	WeightColor += 1e-8f;

					CentroidPosition += Weight * (WMath.Vector) P.Position;
					CentroidNormal += Weight * P.Normal;
					CentroidAlbedo += Weight * P.Albedo;
					SumWeights += Weight;
					SumWeightColors += WeightColor;
				}

				CentroidPosition /= SumWeights;
				CentroidNormal.Normalize();
				CentroidAlbedo /= SumWeights;
				float	EnsureDot = CentroidPosition.Normalized | TargetDirection;	// Should still point close to the original target direction...

				Set	S = new Set() { Position = (WMath.Point) CentroidPosition, Normal = CentroidNormal };
					S.SetAlbedo( CentroidAlbedo );
				Sets.Add( S );
			}

			// Iterate over the scene pixels to determine which set they prefer
			float	PreviousChangesRatio = 1.0f;
			float	SpatialDistanceWeight = _SpatialDistanceWeight;
			float	NormalDistanceWeight = _NormalDistanceWeight * (float) m_MeanHarmonicDistance;
			float	ColorDistanceWeight = _AlbedoDistanceWeight * (float) m_MeanHarmonicDistance;
			while ( true )
			{
				// Iterate over all pixels and see where their loyalty lies, depending on their "distance" to each set
				int	ChangesCount = 0;
				foreach ( Pixel P in m_ScenePixels )
				{
					float	BestSetDistance = 1e6f;
					Set		BestSet = null;
					foreach ( Set S in Sets )
					{
						float	SetDistance = S.ComputeMetric( P, SpatialDistanceWeight, NormalDistanceWeight, ColorDistanceWeight );
						if ( SetDistance >= BestSetDistance )
							continue;	// Not the best candidate set

						BestSetDistance = SetDistance;
						BestSet = S;
					}

					if ( BestSet == null )
						throw new Exception( "WTF?! Means no more sets are available!" );

					// Accumulate centroid position
					BestSet.AccumCentroid += (WMath.Vector) P.Position;
					BestSet.AccumNormal += P.Normal;
					BestSet.SetPixels.Add( P );	// One more pixel in this set!

					// Check if there's any change in the set
					if ( P.ParentSet == BestSet )
						continue;

					// Update set!
					P.ParentSet = BestSet;
					ChangesCount++;
				}

				// Update new centroid positions
				int		RemoveSetCardinalityThreshold = (int) (_Lambda * m_ScenePixels.Count / Sets.Count);
				m_Sets = Sets.ToArray();
				for ( int SetIndex=0; SetIndex < m_Sets.Length; SetIndex++ )
				{
					Set	S = m_Sets[SetIndex];

//					if ( S.SetPixels.Count == 0 )	// Empty set
					if ( S.SetPixels.Count < RemoveSetCardinalityThreshold )	// Simple cardinality threshold
//					float	CardinalityFactor = Math.Min( 1.0f, 0.25f * (float) m_MeanHarmonicDistance / ((WMath.Vector) S.Position).Length() );
// 					float	CardinalityFactor = Math.Min( 1.0f, 1.0f * (float) m_MeanDistance / ((WMath.Vector) S.Position).Length() );
// 					if ( (int) (S.SetPixels.Count * CardinalityFactor) < RemoveSetCardinalityThreshold )	// Cardinality threshold with reduction with distance
					{	// This set is not valuable enough, meaning we can discard it...
						Sets.Remove( S );
						ChangesCount = m_ScenePixels.Count;	// This should force another loop!
						continue;
					}

					S.Position = (WMath.Point) (S.AccumCentroid / S.SetPixels.Count);
					S.Normal = S.AccumNormal / S.SetPixels.Count;

					// Reset accumulators
					S.AccumCentroid.MakeZero();
					S.AccumNormal.MakeZero();
				}

				// Check if we can leave because not many changes were made
				float	ChangesRatio = (float) ChangesCount / m_ScenePixels.Count;
				if ( PreviousChangesRatio < CHANGES_RATIO_THRESHOLD && ChangesRatio < CHANGES_RATIO_THRESHOLD )
					break;	// Done!

				// Reset pixels for each set
				foreach ( Set S in m_Sets )
					S.SetPixels.Clear();

				PreviousChangesRatio = ChangesRatio;
			}

			// Finish by filling final set indices
			for ( int SetIndex=0; SetIndex < m_Sets.Length; SetIndex++ )
				m_Sets[SetIndex].SetIndex = SetIndex;
		}

		#endregion

		#region Computes Sets by Flood Fill Method

int	DEBUG_PixelIndex = 0;

		public void	ComputeFloodFill( int _MaxSetsCount, int _MaxLightingSamplesCount, float _SpatialDistanceWeight, float _NormalDistanceWeight, float _AlbedoDistanceWeight, float _MinimumImportanceDiscardThreshold )
		{
			// Clear the sets for each pixel
			foreach ( Pixel P in m_ProbePixels )
				P.ParentSet = null;

			// Setup the reference thresholds for pixels' acceptance
//			Pixel.IMPORTANCE_THRESOLD = (float) ((4.0f * Math.PI / CUBE_MAP_FACE_SIZE) / (m_MeanDistance * m_MeanDistance));	// Compute an average solid angle threshold based on average pixels' distance
			Pixel.IMPORTANCE_THRESOLD = (float) (0.01f * _MinimumImportanceDiscardThreshold / (m_MeanHarmonicDistance * m_MeanHarmonicDistance));	// Simply use the mean harmonic distance as a good approximation of important pixels
																											// Pixels that are further or not facing the probe will have less importance...

			DISTANCE_THRESHOLD = 0.02f * _SpatialDistanceWeight;									// 2cm
			ANGULAR_THRESHOLD = (float) Math.Cos( 45.0 * _NormalDistanceWeight * Math.PI / 180 );	// 45° (we're very generous here!)
			ALBEDO_HUE_THRESHOLD = 0.04f * _AlbedoDistanceWeight;									// Close colors!
			ALBEDO_RGB_THRESHOLD = 0.32f * _AlbedoDistanceWeight;									// Close colors!

			//////////////////////////////////////////////////////////////////////////
			// 1] Compute occlusion & static lighting SH
			double[]	SHR = new double[9];
			double[]	SHG = new double[9];
			double[]	SHB = new double[9];
			double[]	SHOcclusion = new double[9];

			for ( int PixelIndex=0; PixelIndex < m_ProbePixels.Count; PixelIndex++ )
			{
				Pixel	P = m_ProbePixels[PixelIndex];
				for ( int i=0; i < 9; i++ )
				{
					SHR[i] += P.SHCoeffs[i] * P.SolidAngle * P.StaticLitColor.x;
					SHG[i] += P.SHCoeffs[i] * P.SolidAngle * P.StaticLitColor.y;
					SHB[i] += P.SHCoeffs[i] * P.SolidAngle * P.StaticLitColor.z;
				}

				if ( !P.Infinity )
					continue;

				// No obstacle means direct lighting from the ambient sky...
				// Accumulate SH coefficients in that direction, weighted by the solid angle
				for ( int i=0; i < 9; i++ )
					SHOcclusion[i] += P.SolidAngle * P.SHCoeffs[i];
			}

			double	Normalizer = 1.0 / (4.0 * Math.PI);
//DON'T NORMALIZE	=> 4PI is part of the integral!!!
			for ( int i=0; i < 9; i++ )
			{
				m_StaticSH[i] = new WMath.Vector( (float) (Normalizer * SHR[i]), (float) (Normalizer * SHG[i]), (float) (Normalizer * SHB[i]) );
				m_OcclusionSH[i] = (float) (Normalizer * SHOcclusion[i]);
			}

			// Apply filtering
			SphericalHarmonics.SHFunctions.FilterLanczos( m_StaticSH, 3 );		// Lanczos should be okay for static lighting
			SphericalHarmonics.SHFunctions.FilterHanning( m_OcclusionSH, 3 );


			//////////////////////////////////////////////////////////////////////////
			// 2] Compute the influence of the probe on each scene face
			m_MaxFaceIndex = 0;
			m_ProbeInfluencePerFace.Clear();
			for ( int PixelIndex=0; PixelIndex < m_ProbePixels.Count; PixelIndex++ )
			{
				Pixel	P = m_ProbePixels[PixelIndex];
				if ( P.Infinity )
					continue;

				if ( !m_ProbeInfluencePerFace.ContainsKey( P.FaceIndex ) )
					m_ProbeInfluencePerFace[P.FaceIndex] = 0.0f;

				m_ProbeInfluencePerFace[P.FaceIndex] += P.SolidAngle;
				m_MaxFaceIndex = Math.Max( m_MaxFaceIndex, P.FaceIndex );
			}


			//////////////////////////////////////////////////////////////////////////
			// 3] Build the neighbor probes network
			m_NeighborProbes.Clear();

			Dictionary<int,NeighborProbe>	NeighborProbeID2Probe = new Dictionary<int,NeighborProbe>();
			for ( int PixelIndex=0; PixelIndex < m_ProbePixels.Count; PixelIndex++ )
			{
				Pixel	P = m_ProbePixels[PixelIndex];
				if ( P.NeighborProbeID == -1 )
					continue;

				NeighborProbe	NP = null;
				if ( !NeighborProbeID2Probe.ContainsKey( P.NeighborProbeID ) )
				{
					NP = new NeighborProbe();
					NP.ProbeID = P.NeighborProbeID;
					NeighborProbeID2Probe[P.NeighborProbeID] = NP;
					m_NeighborProbes.Add( NP );
				}
				else
					NP = NeighborProbeID2Probe[P.NeighborProbeID];

				// Accumulate direction, solid angle & distance
				NP.SolidAngle += P.SolidAngle;
				NP.Distance += P.NeighborProbeDistance;
				NP.Direction += P.View;

				// Accumulate SH
				for ( int i=0; i < 9; i++ )
					NP.SH[i] += P.SolidAngle * P.SHCoeffs[i];

				NP.PixelsCount++;
			}

			// Normalize everything
			m_NearestNeighborProbeDistance = float.MaxValue;
			m_FarthestNeighborProbeDistance = 0.0f;
			foreach ( NeighborProbe NP in m_NeighborProbes )
			{
				NP.Distance /= NP.PixelsCount;
				NP.Direction.Normalize();
				for ( int i=0; i < 9; i++ )
					NP.SH[i] /= 4.0 * Math.PI;

				m_NearestNeighborProbeDistance = Math.Min( m_NearestNeighborProbeDistance, NP.Distance );
				m_FarthestNeighborProbeDistance = Math.Max( m_FarthestNeighborProbeDistance, NP.Distance );
			}

			// Sort from most important to least important
			m_NeighborProbes.Sort( this );


			//////////////////////////////////////////////////////////////////////////
			// 4] Iterate on the list of free pixels that belong to no set and create iterative sets
			List<Set>	Sets = new List<Set>();
			for ( int PixelIndex=0; PixelIndex < m_ProbePixels.Count; PixelIndex++ )
			{
DEBUG_PixelIndex = PixelIndex;

				Pixel	P0 = m_ProbePixels[PixelIndex];
				if ( P0.IsFloodFillAcceptable() )
				{
					// Create a new set for this pixel
					Set	S = new Set() { Position = P0.Position, Normal = P0.Normal, Distance = P0.Distance, EmissiveMatID = P0.EmissiveMatID };
						S.SetAlbedo( P0.Albedo );
					Sets.Add( S );


// if ( PixelIndex == 0x2680 )
// 	P0.Albedo.x += 1e-6f;

// if ( P0.IsEmissive )
//  	P0.Albedo.x += 1e-6f;


					// Flood fill adjacent pixels based on a criterion
 					List<Pixel>	SetRejectedPixels = new List<Pixel>();
					try
					{
						S.SetPixels.Clear();

						m_ScanlinePixelIndex = 0;	// VEEERY important line where we reset the pixel index of the pool of flood filled pixels!
						FloodFill( S, S, P0, SetRejectedPixels );

						if ( m_ScanlinePixelIndex == 0 )
							throw new Exception( "Can't have empty sets!" );
					}
					catch ( Exception )
					{
						// Oops! Stack overflow!
						continue;
					}

					// Remove rejected pixels from the set (we only temporarily marked them to avoid them being processed twice by the flood filler)
					foreach ( Pixel P in SetRejectedPixels )
						P.ParentSet = null;	// Ready for another round!

					// Finalize importance
					S.Importance /= S.SetPixels.Count;
// DEBUG
//break;
				}
			}

			//////////////////////////////////////////////////////////////////////////
			// Try and merge sets together

				// Merge separate emissive sets that have the same mat ID together
			List<Set>	EmissiveSets = new List<Set>();

			m_Sets = Sets.ToArray();
			for ( int SetIndex0=0; SetIndex0 < m_Sets.Length-1; SetIndex0++ )
			{
				Set	S0 = m_Sets[SetIndex0];
				if ( S0.IsEmissive && S0.ParentSet == null )
				{
					// Remove it from regular sets and inscribe it as an emissive set
					EmissiveSets.Add( S0 );
					Sets.Remove( S0 );

					// Merge with any other set with same Mat ID
					for ( int SetIndex1=SetIndex0+1; SetIndex1 < m_Sets.Length; SetIndex1++ )
					{
						Set	S1 = m_Sets[SetIndex1];
						if ( !S1.IsEmissive || S1.EmissiveMatID != S0.EmissiveMatID )
							continue;

						// Merge!
						S0.Importance *= S0.SetPixels.Count;
						S1.Importance *= S1.SetPixels.Count;
						S0.SetPixels.AddRange( S1.SetPixels );
						S0.Importance = (S0.Importance + S1.Importance) / S0.SetPixels.Count;

						// Remove the merged set
						Sets.Remove( S1 );
						S1.ParentSet = S0;	// Mark S0 as its parent so it doesn't get processed again
					}
				}
			}

			//////////////////////////////////////////////////////////////////////////
			// Sort and cull unimportant sets

			// Sort and cull sets above our designated maximum
			Sets.Sort( this );
			if ( Sets.Count > _MaxSetsCount )
			{	// Cull sets above our chosen value
				for ( int SetIndex=_MaxSetsCount; SetIndex < Sets.Count; SetIndex++ )
					Sets[SetIndex].SetIndex = -1;	// Invalid index for invalid sets!

				Sets.RemoveRange( _MaxSetsCount, Sets.Count - _MaxSetsCount );
			}

			// Do the same for emissive sets
			EmissiveSets.Sort( this );
			if ( EmissiveSets.Count > _MaxSetsCount )
			{	// Cull sets above our chosen value
				for ( int SetIndex=_MaxSetsCount; SetIndex < EmissiveSets.Count; SetIndex++ )
					EmissiveSets[SetIndex].SetIndex = -1;	// Invalid index for invalid sets!

				EmissiveSets.RemoveRange( _MaxSetsCount, EmissiveSets.Count - _MaxSetsCount );
			}
			m_EmissiveSets = EmissiveSets.ToArray();	// Store as our final emissive sets

			// Remove sets that contain less than 0.4% of our pixels (arbitrary!)
			m_Sets = Sets.ToArray();
			int	DiscardThreshold = (int) (0.004f * m_ScenePixels.Count);
			foreach ( Set S in m_Sets )
				if ( S.SetPixels.Count < DiscardThreshold )
				{
					S.SetIndex = -1;	// Now invalid!
					Sets.Remove( S );
				}

			// Remove sets that are not important enough
			m_Sets = Sets.ToArray();
			foreach ( Set S in m_Sets )
				if ( S.Importance < Pixel.IMPORTANCE_THRESOLD )
				{
					S.SetIndex = -1;	// Now invalid!
					Sets.Remove( S );
				}

			m_Sets = Sets.ToArray();


			//////////////////////////////////////////////////////////////////////////
			// Compute informations on each set
			m_MeanDistance = 0.0;
			m_MeanHarmonicDistance = 0.0;
			m_MinDistance = 1e6;
			m_MaxDistance = 0.0;

			int		SumCardinality = 0;
			for ( int SetIndex=0; SetIndex < m_Sets.Length; SetIndex++ )
			{
				Set	S = m_Sets[SetIndex];
				S.SetIndex = SetIndex;

				// Post-process the pixels to find the one closest to the probe as our new centroid
				Pixel			BestPixel = S.SetPixels[0];
				WMath.Vector	AverageNormal = WMath.Vector.Zero;
				WMath.Vector	AverageAlbedo = WMath.Vector.Zero;
				foreach ( Pixel P in S.SetPixels )
				{
					if ( P.Distance < BestPixel.Distance )
						BestPixel = P;

					AverageNormal += P.Normal;
					AverageAlbedo += P.Albedo;

					// Update min/max/avg
					m_MeanDistance += P.Distance;
					m_MinDistance = Math.Min( m_MinDistance, P.Distance );
					m_MaxDistance = Math.Max( m_MaxDistance, P.Distance );
					m_MeanHarmonicDistance += 1.0 / P.Distance;
				}

				AverageNormal /= S.SetPixels.Count;
				AverageAlbedo /= S.SetPixels.Count;

				S.Position = BestPixel.Position;	// Our new winner!
				S.Normal = AverageNormal;
				S.SetAlbedo( AverageAlbedo );

				// Count pixels in the set for statistics
				SumCardinality += S.SetPixels.Count;

				// Finally, encode SH & find principal axes
				S.EncodeSH();

// Find a faster way!
//				S.FindPrincipalAxes();
			}

			m_MeanHarmonicDistance = SumCardinality / m_MeanHarmonicDistance;
			m_MeanDistance /= SumCardinality;

			// Do the same for emissive sets
			for ( int SetIndex=0; SetIndex < m_EmissiveSets.Length; SetIndex++ )
			{
				Set	S = m_EmissiveSets[SetIndex];
				S.SetIndex = SetIndex;

				// Post-process the pixels to find the one closest to the probe as our new centroid
				Pixel			BestPixel = S.SetPixels[0];
				foreach ( Pixel P in S.SetPixels )
				{
					if ( P.Distance < BestPixel.Distance )
						BestPixel = P;
				}

				S.Position = BestPixel.Position;	// Our new winner!

				// Finally, encode SH & find principal axes & samples
				S.EncodeEmissiveSH();
			}


			// Generate samples for regular sets
			int		TotalSamplesCount = _MaxLightingSamplesCount;
			if ( TotalSamplesCount < m_Sets.Length )
			{	// Force samples count to match sets count!
//				MessageBox( "The amount of samples for the probe was chosen to be " + TotalSamplesCount + " which is inferior to the amount of sets, this would mean some sets wouldn't even get sampled so the actual amount of samples is at least set to the amount of sets (" + m_Sets.Length + ")", MessageBoxButtons.OK, MessageBoxIcon.Warning );
				TotalSamplesCount = m_Sets.Length;
			}

			for ( int SetIndex=m_Sets.Length-1; SetIndex >= 0; SetIndex-- )	// We start from the smallest sets to ensure they get some samples
			{
				Set	S = m_Sets[SetIndex];
				if ( S.IsEmissive )
					throw new Exception( "Shouldn't be any emissive set left in the list of regular sets??!" );

				int	SamplesCount = TotalSamplesCount * S.SetPixels.Count / SumCardinality;
					SamplesCount = Math.Max( 1, SamplesCount );					// Ensure we have at least 1 sample no matter what!
					SamplesCount = Math.Min( SamplesCount, S.SetPixels.Count );	// Can't have more samples than pixels!

				if ( SamplesCount == 0 )
					throw new Exception( "We have a set with NO light sample!" );

				S.GenerateSamples( SamplesCount );

				// Reduce the amount of available samples and the count of remaining pixels so the remaining sets share the remaining samples...
				TotalSamplesCount -= SamplesCount;
				SumCardinality -= S.SetPixels.Count;
			}


			//////////////////////////////////////////////////////////////////////////
			// Sum all SH for UI display
			for ( int i=0; i < 9; i++ )
			{
				m_SHSumDynamic[i] = WMath.Vector.Zero;
				foreach ( Set S in m_Sets )
					m_SHSumDynamic[i] += S.SH[i];
			}

			for ( int i=0; i < 9; i++ )
			{
				m_SHSumEmissive[i] = WMath.Vector.Zero;
				foreach ( Set S in m_EmissiveSets )
					m_SHSumEmissive[i] += S.SH[i];
			}
		}

		#region Flood Fill Algorithm

		float		DISTANCE_THRESHOLD = 0.02f;										// 2cm
		float		ANGULAR_THRESHOLD = (float) Math.Cos( 0.5 * Math.PI / 180 );	// 0.5°
		float		ALBEDO_HUE_THRESHOLD = 0.04f;									// Close colors!
		float		ALBEDO_RGB_THRESHOLD = 0.16f;									// Close colors!

		int			RecursionLevel = 0;

		private int		m_ScanlinePixelIndex = 0;
		private Pixel[]	m_ScanlinePixelsPool = new Pixel[6 * CUBE_MAP_SIZE * CUBE_MAP_SIZE];

		/// <summary>
		/// This should be a faster version (and much less recursive!) than the original flood fill
		/// The idea here is to process an entire scanline first (going left and right and collecting valid scanline pixels along the way)
		///  then for each of these pixels we move up/down and fill the top/bottom scanlines from these new seeds...
		/// </summary>
		/// <param name="_S"></param>
		/// <param name="_PreviousPixel"></param>
		/// <param name="_P"></param>
		/// <param name="_SetPixels"></param>
		/// <param name="_SetRejectedPixels"></param>
		private void	FloodFill( Set _S, Pixel _PreviousPixel, Pixel _P, List<Pixel> _SetRejectedPixels )
		{
			if ( !CheckAndAcceptPixel( _S, _PreviousPixel, _P, _SetRejectedPixels ) )
				return;

// if ( DEBUG_PixelIndex == 0x2680 )
// 	Console.WriteLine( "R=" + RecursionLevel + " - S=" + _SetPixels.Count );
// 
// if ( DEBUG_PixelIndex == 0x700 && _SetPixels.Count == 2056 )
// 	Console.WriteLine( "GOTCHA!" );

			//////////////////////////////////////////////////////////////////////////
			// Check the entire scanline
			int	ScanlineStartIndex = m_ScanlinePixelIndex;

			m_ScanlinePixelsPool[m_ScanlinePixelIndex++] = _P;	// This pixel is implicitly on the scanline

			// Start going right
			Pixel	Previous = _P;
			Pixel	Current = FindAdjacentPixel( Previous, 1, 0 );
			while ( CheckAndAcceptPixel( _S, Previous, Current, _SetRejectedPixels ) )
			{
				m_ScanlinePixelsPool[m_ScanlinePixelIndex++] = Current;
				Previous = Current;
				Current = FindAdjacentPixel( Current, 1, 0 );
			}

			// Start going left
			Previous = _P;
			Current = FindAdjacentPixel( Previous, -1, 0 );
			while ( CheckAndAcceptPixel( _S, Previous, Current, _SetRejectedPixels ) )
			{
				m_ScanlinePixelsPool[m_ScanlinePixelIndex++] = Current;
				Previous = Current;
				Current = FindAdjacentPixel( Current, -1, 0 );
			}

			RecursionLevel++;

			int	ScanlineEndIndex = m_ScanlinePixelIndex;

			//////////////////////////////////////////////////////////////////////////
			// Recurse into each pixel of the top scanline
			for ( int ScanlinePixelIndex=ScanlineStartIndex; ScanlinePixelIndex < ScanlineEndIndex; ScanlinePixelIndex++ )
			{
				Pixel	P = m_ScanlinePixelsPool[ScanlinePixelIndex];
				Pixel	Top = FindAdjacentPixel( P, 0, 1 );

				FloodFill( _S, P, Top, _SetRejectedPixels );
			}

			//////////////////////////////////////////////////////////////////////////
			// Recurse into each pixel of the bottom scanline
			for ( int ScanlinePixelIndex=ScanlineStartIndex; ScanlinePixelIndex < ScanlineEndIndex; ScanlinePixelIndex++ )
			{
				Pixel	P = m_ScanlinePixelsPool[ScanlinePixelIndex];
				Pixel	Bottom = FindAdjacentPixel( P, 0, -1 );
				FloodFill( _S, P, Bottom, _SetRejectedPixels );
			}

			RecursionLevel--;
		}

		private bool	CheckAndAcceptPixel( Set _S, Pixel _PreviousPixel, Pixel _P, List<Pixel> _SetRejectedPixels )
		{
			if ( !_P.IsFloodFillAcceptable() )
				return false;

			//////////////////////////////////////////////////////////////////////////
			// Check some criterions for a match
			bool	Accepted = false;

			// Emissive pixels get grouped together
			if ( _PreviousPixel.IsEmissive || _P.IsEmissive )
			{
				Accepted = _PreviousPixel.EmissiveMatID == _P.EmissiveMatID;
			}
			else
			{
				// First, let's check the angular discrepancy
				float	Dot = _PreviousPixel.Normal | _P.Normal;
				if ( Dot > ANGULAR_THRESHOLD )
				{
					// Next, let's check the distance discrepancy
					float	DistanceDiff = Math.Abs( _PreviousPixel.Distance - _P.Distance );
					float	ToleranceFactor = -(_P.Normal | _P.View);						// Weight by the surface's slope to be more tolerant for slant surfaces
							DistanceDiff *= ToleranceFactor;
					if ( DistanceDiff < DISTANCE_THRESHOLD )
					{
						// Next, let's check the hue discrepancy
// 						float	HueDiff0 = Math.Abs( _PreviousPixel.AlbedoHSL.x - _P.AlbedoHSL.x );
// 						float	HueDiff1 = 6.0f - HueDiff0;
// 						float	HueDiff = Math.Min( HueDiff0, HueDiff1 );
// //								HueDiff *= 0.5f * (_PreviousPixel.AlbedoHSL.y + _P.AlbedoHSL.y);	// Weight by saturation to be less severe with unsaturated colors that can change in hue quite fast
// 								HueDiff *= Math.Max( _PreviousPixel.AlbedoHSL.y, _P.AlbedoHSL.y );	// Weight by saturation to be less severe with unsaturated colors that can change in hue quite fast
// 						if ( HueDiff < ALBEDO_HUE_THRESHOLD )
// 						{
// 							Accepted = true;	// Winner!
// 						}


						// Next, let's check color discrepancy
						// I'm using the simplest metric here...
						float	ColorDiff = (_PreviousPixel.Albedo - _P.Albedo).Length;
						if ( ColorDiff < ALBEDO_RGB_THRESHOLD )
						{
							Accepted = true;	// Winner!
						}
					}
				}
			}

			// Mark the pixel as member of this set, even if it's temporary (rejected pixels get removed in the end)
			_P.ParentSet = _S;

			if ( Accepted )
			{
				_S.SetPixels.Add( _P );			// We got a new member for the set!
				_S.Importance += _P.Importance;	// Accumulate average importance
			}
			else
				_SetRejectedPixels.Add( _P );	// Sorry buddy, we'll add you to the rejects...

			return Accepted;
		}

/*
				/// <summary>
				/// The very recursive version that is quite annoying to debug!
				/// </summary>
				/// <param name="_S"></param>
				/// <param name="_PreviousPixel"></param>
				/// <param name="_P"></param>
				/// <param name="_SetPixels"></param>
				/// <param name="_SetRejectedPixels"></param>
				private void	FloodFill_Recursive( Set _S, Pixel _PreviousPixel, Pixel _P, List<Pixel> _SetRejectedPixels )
				{
					if ( !_P.IsFloodFillAcceptable() )
						return;

					//////////////////////////////////////////////////////////////////////////
					// Check some criterions for a match
					bool	Accepted = false;

					// First, let's check the angular discrepancy
					float	Dot = _PreviousPixel.Normal | _P.Normal;
					if ( Dot > ANGULAR_THRESHOLD )
					{
						// Next, let's check the distance discrepancy
						float	DistanceDiff = Math.Abs( _PreviousPixel.Distance - _P.Distance );
						float	ToleranceFactor = -(_P.Normal | _P.View);						// Weight by the surface's slope to be more tolerant for slant surfaces
								DistanceDiff *= ToleranceFactor;
						if ( DistanceDiff < DISTANCE_THRESHOLD )
						{
							// Next, let's check the hue discrepancy
							float	HueDiff0 = Math.Abs( _PreviousPixel.AlbedoHSL.x - _P.AlbedoHSL.x );
							float	HueDiff1 = 6.0f - HueDiff0;
							float	HueDiff = Math.Min( HueDiff0, HueDiff1 );
									HueDiff *= 0.5f * (_PreviousPixel.AlbedoHSL.y + _P.AlbedoHSL.y);	// Weight by saturation to be less severe with unsaturated colors that can change hue quite fast
							if ( HueDiff < ALBEDO_HUE_THRESHOLD )
							{
								Accepted = true;	// Winner!
							}
						}
					}

					// Mark the pixel as member of this set, even if it's temporary (rejected pixels get removed in the end)
					_P.ParentSet = _S;

					if ( !Accepted )
					{	// Sorry buddy, we'll add you to the rejects...
						_SetRejectedPixels.Add( _P );
						return;
					}


					//////////////////////////////////////////////////////////////////////////
					// We got a new member for the set!
					_S.SetPixels.Add( _P );


		if ( DEBUG_PixelIndex == 0x700 )
			Console.WriteLine( "R=" + RecursionLevel + " - S=" + _S.SetPixels.Count );

		if ( DEBUG_PixelIndex == 0x700 && _S.SetPixels.Count == 2056 )
			Console.WriteLine( "GOTCHA!" );

					//////////////////////////////////////////////////////////////////////////
					// Recurse to 4 neighbors
					RecursionLevel++;
					if ( RecursionLevel > 10000 )
						throw new Exception();

					Pixel	L = FindAdjacentPixel( _P, -1, 0 );
					FloodFill( _S, _P, L, _SetRejectedPixels );

					Pixel	R = FindAdjacentPixel( _P, 1, 0 );
					FloodFill( _S, _P, R, _SetRejectedPixels );

					Pixel	D = FindAdjacentPixel( _P, 0, -1 );
					FloodFill( _S, _P, D, _SetRejectedPixels );

					Pixel	U = FindAdjacentPixel( _P, 0, 1 );
					FloodFill( _S, _P, U, _SetRejectedPixels );

					RecursionLevel--;
				}
*/

		#region Adjacency Walker

		const int	CUBE_MAP_FACE_SIZE = CUBE_MAP_SIZE * CUBE_MAP_SIZE;

		// Contains the new cube face index when stepping outside of a cube face by the left/right/top/bottom
		int[]	GoLeft = new int[6] {
			4,	// Step to +Z
			5,	// Step to -Z
			1,	// Step to -X
			1,	// Step to -X
			1,	// Step to -X
			0,	// Step to +X
		};
		int[]	GoRight = new int[6] {
			5,	// Step to -Z
			4,	// Step to +Z
			0,	// Step to +X
			0,	// Step to +X
			0,	// Step to +X
			1,	// Step to -X
		};
		int[]	GoDown = new int[6] {
			2,	// Step to +Y
			2,	// Step to +Y
			4,	// Step to +Z
			5,	// Step to -Z
			2,	// Step to +Y
			2,	// Step to +Y
		};
		int[]	GoUp = new int[6] {
			3,	// Step to -Y
			3,	// Step to -Y
			5,	// Step to -Z
			4,	// Step to +Z
			3,	// Step to -Y
			3,	// Step to -Y
		};

		// Contains the matrices that indicate how the (X,Y) pixel coordinates should be transformed to step from one face to the other
		// Transforms arrays are simple matrices:
		//	Tx Xx Xy
		//	Ty Yx Yy
		//
		// Which are used like this:
		//	X' = Tx * CUBE_MAP_SIZE + Xx * X + Xy * Y
		//	Y' = Ty * CUBE_MAP_SIZE + Yx * X + Yy * Y
		//
		const int C = CUBE_MAP_SIZE;
		const int C2 = 2*C;
		const int C_ = CUBE_MAP_SIZE-1;
		int[][]	GoLeftTransforms = new int[6][] {
			// Going left from +X sends us to +Z
			new int[6] {	C,  1,  0,		// X' = C + X	(C is the CUBE_MAP_SIZE)
							0,  0,  1 },	// Y' = Y
			// Going left from -X sends us to -Z
			new int[6] {	C,  1,  0,		// X' = C + X
							0,  0,  1 },	// Y' = Y
			// Going left from +Y sends us to -X
			new int[6] {	C_,  0, -1,		// X' = C - Y
							0, -1,  0 },	// Y' = -X
			// Going left from -Y sends us to -X
			new int[6] {	0,  0,  1,		// X' = Y
							C,  1,  0 },	// Y' = C + X
			// Going left from +Z sends us to -X
			new int[6] {	C,  1,  0,		// X' = C + X
							0,  0,  1 },	// Y' = Y
			// Going left from -Z sends us to +X
			new int[6] {	C,  1,  0,		// X' = C + X
							0,  0,  1 },	// Y' = Y
		};
		int[][]	GoRightTransforms = new int[6][] {
			// Going right from +X sends us to -Z
			new int[6] {	-C,  1,  0,		// X' = -C + X	(C is the CUBE_MAP_SIZE)
							0,  0,  1 },	// Y' = Y
			// Going right from -X sends us to +Z
			new int[6] {	-C,  1,  0,		// X' = -C + X
							0,  0,  1 },	// Y' = Y
			// Going right from +Y sends us to +X
			new int[6] {	0,  0,  1,		// X' = Y
							-C, 1,  0 },	// Y' = -C + X
			// Going right from -Y sends us to +X
			new int[6] {	C_, 0,  -1,		// X' = C - Y
							C,  1,  0 },	// Y' = C + X
			// Going right from +Z sends us to +X
			new int[6] {	-C,  1,  0,		// X' = -C + X
							0,  0,  1 },	// Y' = Y
			// Going right from -Z sends us to -X
			new int[6] {	-C,  1,  0,		// X' = -C + X
							0,  0,  1 },	// Y' = Y
		};
		int[][]	GoDownTransforms = new int[6][] {
			// Going down from +X sends us to +Y
			new int[6] {	C,  0,  1,		// X' = C + Y	(C is the CUBE_MAP_SIZE)
							0,  1,  0 },	// Y' = X
			// Going down from -X sends us to +Y
			new int[6] {	0,  0, -1,		// X' = -Y
							C_, -1,  0 },	// Y' = C - X
			// Going down from +Y sends us to +Z
			new int[6] {	0,  1,  0,		// X' = X
							0,  0, -1 },	// Y' = -Y
			// Going down from -Y sends us to -Z
			new int[6] {	C_, -1,  0,		// X' = C - X
							C,  0,  1 },	// Y' = C + Y
			// Going down from +Z sends us to +Y
			new int[6] {	0,  1,  0,		// X' = X
							0,  0, -1 },	// Y' = -Y
			// Going down from -Z sends us to +Y
			new int[6] {	C_, -1,  0,		// X' = C - X
							C,  0,  1 },	// Y' = C + Y
		};
		int[][]	GoUpTransforms = new int[6][] {
			// Going up from +X sends us to -Y
			new int[6] {	C2,  0, -1,		// X' = 2C - Y	(C is the CUBE_MAP_SIZE)
							C_, -1,  0 },	// Y' = C - X
			// Going up from -X sends us to -Y
			new int[6] {	-C,  0,  1,		// X' = -C + Y
							0,  1,  0 },	// Y' = X
			// Going up from +Y sends us to -Z
			new int[6] {	C_, -1,  0,		// X' = C - X
							-C, 0,  1 },	// Y' = -C + Y
			// Going up from -Y sends us to +Z
			new int[6] {	0,  1,  0,		// X' = X
							C2,  0, -1 },	// Y' = 2C - Y
			// Going up from +Z sends us to -Y
			new int[6] {	0,  1,  0,		// X' = X
							C2,  0, -1 },	// Y' = 2C - Y
			// Going up from -Z sends us to -Y
			new int[6] {	C_, -1,  0,		// X' = C - X
							-C, 0,  1 },	// Y' = -C + Y
		};

		private Pixel	FindAdjacentPixel( Pixel _P, int _Dx, int _Dy )
		{
			int	CubeFaceIndex = _P.CubeFaceIndex;
			int	X = _P.CubeFaceX;
			int	Y = _P.CubeFaceY;

			X += _Dx;
			if ( X < 0 )
			{	// Stepped out through left side
				TransformXY( GoLeftTransforms[CubeFaceIndex], ref X, ref Y );
				CubeFaceIndex = GoLeft[CubeFaceIndex];
			}
			if ( X >= CUBE_MAP_SIZE )
			{	// Stepped out through right side
				TransformXY( GoRightTransforms[CubeFaceIndex], ref X, ref Y );
				CubeFaceIndex = GoRight[CubeFaceIndex];
			}

			Y += _Dy;
			if ( Y < 0 )
			{	// Stepped out through bottom side
				TransformXY( GoDownTransforms[CubeFaceIndex], ref X, ref Y );
				CubeFaceIndex = GoDown[CubeFaceIndex];
			}
			if ( Y >= CUBE_MAP_SIZE )
			{	// Stepped out through top side
				TransformXY( GoUpTransforms[CubeFaceIndex], ref X, ref Y );
				CubeFaceIndex = GoUp[CubeFaceIndex];
			}

			int	FinalPixelIndex = CUBE_MAP_FACE_SIZE * CubeFaceIndex + CUBE_MAP_SIZE * Y + X;

			return m_ProbePixels[FinalPixelIndex];
		}

		private void	TransformXY( int[] _Transform, ref int _X, ref int _Y )
		{
			int	TempX = _Transform[0] + _Transform[1] * _X + _Transform[2] * _Y;
			int	TempY = _Transform[3] + _Transform[4] * _X + _Transform[5] * _Y;
			_X = TempX;
			_Y = TempY;
		}

		#endregion

		#endregion

		#endregion

		#region IComparer<Set> Members

		public int Compare( Set x, Set y )
		{
			if ( x.SetPixels.Count < y.SetPixels.Count )
				return +1;
			if ( x.SetPixels.Count > y.SetPixels.Count )
				return -1;

			return 0;
		}

		#endregion

		#region IComparer<NeighborProbe> Members

		public int Compare( Probe.NeighborProbe x, Probe.NeighborProbe y )
		{
			if ( x.SolidAngle < y.SolidAngle )
				return +1;
			if ( x.SolidAngle > y.SolidAngle )
				return -1;

			return 0;
		}

		#endregion

		private void	PrepareCubeMapFaceTransforms()
		{
			//////////////////////////////////////////////////////////////////////////
			// Prepare the cube map face transforms
			// Here are the transform to render the 6 faces of a cube map
			// Remember the +Z face is not oriented the same way as our Z vector: http://msdn.microsoft.com/en-us/library/windows/desktop/bb204881(v=vs.85).aspx
			//
			//
			//		^ +Y
			//		|   +Z  (our actual +Z faces the other way!)
			//		|  /
			//		| /
			//		|/
			//		o------> +X
			//
			//
			WMath.Vector[]	SideAt = new WMath.Vector[6]
			{
				new WMath.Vector(  1, 0, 0 ),
				new WMath.Vector( -1, 0, 0 ),
				new WMath.Vector( 0,  1, 0 ),
				new WMath.Vector( 0, -1, 0 ),
				new WMath.Vector( 0, 0,  1 ),
				new WMath.Vector( 0, 0, -1 ),
			};
			WMath.Vector[]	SideRight = new WMath.Vector[6]
			{
				new WMath.Vector( 0, 0, -1 ),
				new WMath.Vector( 0, 0,  1 ),
				new WMath.Vector(  1, 0, 0 ),
				new WMath.Vector(  1, 0, 0 ),
				new WMath.Vector(  1, 0, 0 ),
				new WMath.Vector( -1, 0, 0 ),
			};

			for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
			{
				WMath.Matrix4x4	Camera2Local = new WMath.Matrix4x4();
				Camera2Local.SetRow( 0, SideRight[CubeFaceIndex], 0 );
				Camera2Local.SetRow( 1, SideAt[CubeFaceIndex] ^ SideRight[CubeFaceIndex], 0 );
				Camera2Local.SetRow( 2, SideAt[CubeFaceIndex], 0 );
				Camera2Local.SetRow( 3, WMath.Vector.Zero, 1 );

				m_Side2World[CubeFaceIndex] = Camera2Local;	// We don't care about the local=>world transform of the probe, we assume it's the identity matrix
			}
		}

		#endregion
	}
}
