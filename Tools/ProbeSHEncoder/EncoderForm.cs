//////////////////////////////////////////////////////////////////////////
// The purpose of this little application is to analyze the rendering from a cube map probe to perform a grouping
//	of the pixels based on their position, normal and albedo to create a limited amount of sets that we'll be able
//	to replace by simple "disc surface elements" that can be lit with dynamic lights.
// 
// These pixels belonging to each set will be be considered having the same albedo and will light the probe with
//	precomputed spherical harmonic coefficients each pondered by the solid angle covered by the pixel in the direction
//	specific to the pixel.
// 
//////////////////////////////////////////////////////////////////////////
// 
// I'm testing several methods to create the sets:
//	(1) k-means clustering (http://en.wikipedia.org/wiki/K-means_clustering), that consists in creating initial sets
//		using an educated guess and aggregating pixels to each set depending on a metric.
//		The pixel gets assigned to the set whose metric is the lowest. I'm currently using a
//		metric mixing spatial distance, hue distance (for albedo similarity) and normal discrepancies measurement.
//
//		I believe it could give interesting results with a little effort but I'm lazy and I think it's still a bit
//		dodgy because it doesn't handle pixels vicinity and tends to fragment sets.
// 
//	(2) Filling method, it's an experimental method of mine that consists in browsing the pixels of the cube map and
//		perform a fill operation by joining adjacent pixels if and only if they're sufficiently close enough in terms
//		of distance, normal and color.
//		Each set created this way has its own list of pixels removed from the global list of free pixels, pixels whose
//		solid angle is too low are discarded.
//		The algorithm continues until all pixels have been discarded or added to a set, then the algorithm enters a second
//		phase of optimization where sets are merged together if sufficiently close, or discarded if not significant enough.
// 
//////////////////////////////////////////////////////////////////////////
// 
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ProbeSHEncoder
{
	public partial class EncoderForm : Form
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
			public int			PixelIndex;			// Index of the pixel in the scene pixels (can help us locate the cube map face + position of the pixel when finding adjacent pixels)

			public WMath.Point	Position;			// World position
			public WMath.Vector	Normal;				// World normal
			public WMath.Vector	Albedo;				// Material albedo
			public WMath.Vector	AlbedoHSL;			// Material albedo in HSL format
			public float		F0;					// Material Fresnel coefficient
			public double		SolidAngle;			// Solid angle covered by the pixel
			public double		SceneSolidAngle;	// Solid angle covered by the scene object = -dot( View, Normal ) / Distance²
			public float		Distance;			// Distance to hit point
			public bool			Infinity;			// True if not a scene pixel (i.e. sky pixel)
			public WMath.Vector	View;				// View vector pointing to that pixel

			public Set			ParentSet = null;

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
			public float	ComputeMetric( Pixel _Other, float _PositionDistanceWeight, float _NormalDistanceWeight, float _ColorDistanceWeight )
			{
				float	EuclidianDistance = (_Other.Position - Position).Length();
						EuclidianDistance *= _PositionDistanceWeight;

				float	NormalDistance = 0.5f * (1.0f - (_Other.Normal | Normal));
						NormalDistance *= _NormalDistanceWeight;	// Used to give the normal as much weight as general euclidian distances...

				float	ColorDistance0 = Math.Abs( AlbedoHSL.x - _Other.AlbedoHSL.x );
				float	ColorDistance1 = 6.0f - ColorDistance0;
				float	ColorDistance = Math.Min( ColorDistance0, ColorDistance1 ) / 6.0f;
						ColorDistance *= _ColorDistanceWeight;		// Used to give the color as much weight as general euclidian distances...

				return EuclidianDistance + NormalDistance + ColorDistance;
			}
		}

		/// <summary>
		/// A set is a special pixel with a centroid, a normal and an average albedo
		/// It also serves as accumulator for all pixels registering to the set
		/// </summary>
		public class	Set : Pixel
		{
			public WMath.Vector	AccumCentroid = WMath.Vector.Zero;
			public WMath.Vector	AccumNormal = WMath.Vector.Zero;
			public int			SetCardinality = 0;

			public int			LastSetCardinality = 0;
			public int			SetIndex = -1;	// Warning: Only available once the computation is over and all sets have been resolved!
		}

		#endregion

		#region FIELDS

		public WMath.Matrix4x4[]	m_Side2World = new WMath.Matrix4x4[6];

		public Pixel[][,]			m_CubeMap = new Pixel[6][,];
		public double				m_MeanDistance = 0.0;
		public double				m_MeanHarmonicDistance = 0.0;

		public Set[]				m_Sets = new Set[0];

		public List<Pixel>			m_ScenePixels = new List<Pixel>();

		private RegistryKey			m_AppKey;
		private string				m_ApplicationPath;

		#endregion

		public EncoderForm()
		{
			InitializeComponent();

			outputPanel1.m_Owner = this;

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\ProbeSHEncoder" );
			m_ApplicationPath = Path.GetDirectoryName( Application.ExecutablePath );

			PrepareCubeMapFaceTransforms();
			LoadCubeMap();

			outputPanel1.At = -WMath.Vector.UnitZ;
		}

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

		private void	LoadCubeMap()
		{
			// Load and convert POM files into a useable cube map
			Nuaj.Cirrus.Utility.TextureFilePOM	POM0 = new Nuaj.Cirrus.Utility.TextureFilePOM();
			Nuaj.Cirrus.Utility.TextureFilePOM	POM1 = new Nuaj.Cirrus.Utility.TextureFilePOM();
			POM0.Load( new FileInfo( "../../Probe_Albedo.pom" ) );
			POM1.Load( new FileInfo( "../../Probe_Geometry.pom" ) );

			if ( POM0.m_Width != POM1.m_Width || POM0.m_Height != POM1.m_Height || POM0.m_ArraySizeOrDepth != POM1.m_ArraySizeOrDepth || POM0.m_Width != CUBE_MAP_SIZE || POM0.m_Height != CUBE_MAP_SIZE || POM0.m_ArraySizeOrDepth != 6 )
				throw new Exception( "Unexpected cube map size!" );

			double	dA = 4.0 / (CUBE_MAP_SIZE*CUBE_MAP_SIZE);	// Cube face is supposed to be in [-1,+1], yielding a 2x2 square units
			double	SumSolidAngle = 0.0;

			for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
			{
				m_CubeMap[CubeFaceIndex] = new Pixel[CUBE_MAP_SIZE,CUBE_MAP_SIZE];
				for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
					for ( int X=0; X < CUBE_MAP_SIZE; X++ )
						m_CubeMap[CubeFaceIndex][X,Y] = new Pixel() { PixelIndex = X + CUBE_MAP_SIZE * (Y + CUBE_MAP_SIZE * CubeFaceIndex) };

				// Fill up albedo
				using ( MemoryStream S = new MemoryStream( POM0.m_Content[CubeFaceIndex] ) )
					using ( BinaryReader R = new BinaryReader( S ) )
						for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
							for ( int X=0; X < CUBE_MAP_SIZE; X++ )
							{
								float	Red = R.ReadSingle();
								float	Green = R.ReadSingle();
								float	Blue = R.ReadSingle();
								float	Alpha = R.ReadSingle();
								m_CubeMap[CubeFaceIndex][X,Y].SetAlbedo( new WMath.Vector( Red, Green, Blue ) );
							}

				// Fill up position & normal
				m_MeanDistance = 0.0;
				m_MeanHarmonicDistance = 0.0;
				using ( MemoryStream S = new MemoryStream( POM1.m_Content[CubeFaceIndex] ) )
					using ( BinaryReader R = new BinaryReader( S ) )
						for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
							for ( int X=0; X < CUBE_MAP_SIZE; X++ )
							{
								Pixel			Pix = m_CubeMap[CubeFaceIndex][X,Y];

								WMath.Vector	csView = new WMath.Vector( 2.0f * (0.5f + X) / CUBE_MAP_SIZE - 1.0f, 1.0f - 2.0f * (0.5f + Y) / CUBE_MAP_SIZE, 1.0f );
								float			Distance2Texel = csView.Length();
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
								Pix.Normal = new WMath.Vector( Nx, Ny, Nz );
								Pix.SolidAngle = SolidAngle;
								Pix.SceneSolidAngle = -(wsView | Pix.Normal) / (Distance * Distance);
								Pix.Distance = Distance;
								Pix.Infinity = Distance > Z_INFINITY_TEST;

								if ( !m_CubeMap[CubeFaceIndex][X,Y].Infinity )
								{	// Account for a new scene pixel (i.e. not infinity)
									m_ScenePixels.Add( Pix );
									m_MeanDistance += Distance;
									m_MeanHarmonicDistance += 1.0 / Distance;
								}
							}
			}

			m_MeanDistance /= (CUBE_MAP_SIZE * CUBE_MAP_SIZE * 6);
			m_MeanHarmonicDistance = (CUBE_MAP_SIZE * CUBE_MAP_SIZE * 6) / m_MeanHarmonicDistance;

			// Redraw cube map...
			outputPanel1.UpdateBitmap();
		}

		#region Helpers

		private string	GetRegKey( string _Key, string _Default )
		{
			string	Result = m_AppKey.GetValue( _Key ) as string;
			return Result != null ? Result : _Default;
		}
		private void	SetRegKey( string _Key, string _Value )
		{
			m_AppKey.SetValue( _Key, _Value );
		}

		private void	MessageBox( string _Text )
		{
			MessageBox( _Text, MessageBoxButtons.OK );
		}
		private void	MessageBox( string _Text, MessageBoxButtons _Buttons )
		{
			MessageBox( _Text, _Buttons, MessageBoxIcon.Information );
		}
		private void	MessageBox( string _Text, MessageBoxIcon _Icon )
		{
			MessageBox( _Text, MessageBoxButtons.OK, _Icon );
		}
		private void	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon )
		{
			System.Windows.Forms.MessageBox.Show( this, _Text, "Shader Interpreter", _Buttons, _Icon );
		}

		#endregion

		#region Computes k-Means Sets

		private void buttonCompute_Click( object sender, EventArgs e )
		{
//			const int	K = 32;
			int	K = integerTrackbarControlK.Value;

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
				CentroidNormal /= SumWeights;
				CentroidAlbedo /= SumWeights;
				float	EnsureDot = CentroidPosition.Normalized | TargetDirection;	// Should still point close to the original target direction...

				Set	S = new Set() { Position = (WMath.Point) CentroidPosition, Normal = CentroidNormal };
					S.SetAlbedo( CentroidAlbedo );
				Sets.Add( S );
			}

			// Iterate over the scene pixels to determine which set they prefer
			float	PreviousChangesRatio = 1.0f;
			float	SpatialDistanceWeight = floatTrackbarControlPosition.Value;
			float	NormalDistanceWeight = floatTrackbarControlNormal.Value * (float) m_MeanHarmonicDistance;
			float	ColorDistanceWeight = floatTrackbarControlAlbedo.Value * (float) m_MeanHarmonicDistance;
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
					BestSet.SetCardinality++;	// One more pixel in this set!

					// Check if there's any change in the set
					if ( P.ParentSet == BestSet )
						continue;

					// Update set!
					P.ParentSet = BestSet;
					ChangesCount++;
				}

				// Update new centroid positions
				int		RemoveSetCardinalityThreshold = (int) (floatTrackbarControlLambda.Value * m_ScenePixels.Count / Sets.Count);
				m_Sets = Sets.ToArray();
				for ( int SetIndex=0; SetIndex < m_Sets.Length; SetIndex++ )
				{
					Set	S = m_Sets[SetIndex];

//					if ( S.SetCardinality == 0 )	// Empty set
					if ( S.SetCardinality < RemoveSetCardinalityThreshold )	// Simple cardinality threshold
//					float	CardinalityFactor = Math.Min( 1.0f, 0.25f * (float) m_MeanHarmonicDistance / ((WMath.Vector) S.Position).Length() );
// 					float	CardinalityFactor = Math.Min( 1.0f, 1.0f * (float) m_MeanDistance / ((WMath.Vector) S.Position).Length() );
// 					if ( (int) (S.SetCardinality * CardinalityFactor) < RemoveSetCardinalityThreshold )	// Cardinality threshold with reduction with distance
					{	// This set is not valuable enough, meaning we can discard it...
						Sets.Remove( S );
						ChangesCount = m_ScenePixels.Count;	// This should force another loop!
						continue;
					}

					S.Position = (WMath.Point) (S.AccumCentroid / S.SetCardinality);
					S.Normal = S.AccumNormal / S.SetCardinality;

					// Reset accumulators
					S.AccumCentroid.MakeZero();
					S.AccumNormal.MakeZero();
					S.LastSetCardinality = S.SetCardinality;
					S.SetCardinality = 0;
				}

				// Check if we can leave because not many changes were made
				float	ChangesRatio = (float) ChangesCount / m_ScenePixels.Count;
				if ( PreviousChangesRatio < CHANGES_RATIO_THRESHOLD && ChangesRatio < CHANGES_RATIO_THRESHOLD )
					break;	// Done!

				PreviousChangesRatio = ChangesRatio;
			}

			// Finish by filling final set indices
			for ( int SetIndex=0; SetIndex < m_Sets.Length; SetIndex++ )
				m_Sets[SetIndex].SetIndex = SetIndex;


			// Refresh UI
			textBoxResults.Text = m_Sets.Length + " sets generated:\r\n\r\n";
			for ( int SetIndex=0; SetIndex < m_Sets.Length; SetIndex++ )
			{
				Set	S = m_Sets[SetIndex];
				textBoxResults.Text += SetIndex + ") " + S.LastSetCardinality + " pixels (" + (100.0f * S.LastSetCardinality / m_ScenePixels.Count).ToString( "G4" ) + "%)\r\n"
									+ "Albedo = (" + S.Albedo.x.ToString( "G4" ) + ", " + S.Albedo.y.ToString( "G4" ) + ", " + S.Albedo.z.ToString( "G4" ) + ")\r\n\r\n";
			}

			integerTrackbarControlSetIsolation.RangeMax = m_Sets.Length-1;
			integerTrackbarControlSetIsolation.VisibleRangeMax = integerTrackbarControlSetIsolation.RangeMax;

//			radioButtonSetIndex.Checked = true;
			outputPanel1.UpdateBitmap();
		}

		#endregion

		#region Computes Sets by Flood Fill Method

		private void buttonComputeFilling_Click( object sender, EventArgs e )
		{
			// 1] Clear the sets for each pixel
			foreach ( Pixel P in m_ScenePixels )
				P.ParentSet = null;

			// Compute an average solid angle threshold based on average pixels' distance
			SOLID_ANGLE_THRESOLD = (float) ((4.0f * Math.PI / CUBE_MAP_FACE_SIZE) / (m_MeanDistance * m_MeanDistance));


			//////////////////////////////////////////////////////////////////////////
			// 2] Iterate on the list of free pixels that belong to no set and create iterative sets
			List<Set>	Sets = new List<Set>();
			for ( int PixelIndex=0; PixelIndex < m_ScenePixels.Count; PixelIndex++ )
			{
				Pixel	P0 = m_ScenePixels[PixelIndex];
				if ( P0.Infinity )
					continue;	// Not a valid pixel...

				if ( P0.ParentSet == null )
				{
					// Create a new set for this pixel
					Set	S = new Set() { Position = P0.Position, Normal = P0.Normal, Distance = P0.Distance };
						S.SetAlbedo( P0.Albedo );
					Sets.Add( S );

					// Flood fill adjacent pixels based on a criterion
					List<Pixel>	SetPixels = new List<Pixel>();
					List<Pixel>	SetRejectedPixels = new List<Pixel>();
					FloodFill( S, S, P0, SetPixels, SetRejectedPixels );

					// Remove rejected pixels from the set (we only temporarily marked them to avoid processing them twice)
					foreach ( Pixel P in SetRejectedPixels )
						P.ParentSet = null;	// Ready for another round!

					// Post-process the pixels to find the one closest to the probe as our new centroid
					Pixel			BestPixel = P0;
					WMath.Vector	AverageNormal = WMath.Vector.Zero;
					WMath.Vector	AverageAlbedo = WMath.Vector.Zero;
					foreach ( Pixel P in SetPixels )
					{
						if ( P.Distance < BestPixel.Distance )
							BestPixel = P;

						AverageNormal += P.Normal;
						AverageAlbedo += P.Albedo;
					}

					AverageNormal /= SetPixels.Count;
					AverageAlbedo /= SetPixels.Count;

					S.Position = BestPixel.Position;	// Our new winner!
					S.Normal = AverageNormal;
					S.SetAlbedo( AverageAlbedo );

					// Store amount of pixels in the set for statistics
					S.SetCardinality = S.LastSetCardinality = SetPixels.Count;
// DEBUG
//break;
				}
			}

			m_Sets = Sets.ToArray();

			// Finish by filling final set indices
			for ( int SetIndex=0; SetIndex < m_Sets.Length; SetIndex++ )
				m_Sets[SetIndex].SetIndex = SetIndex;

			// Refresh UI
			textBoxResults.Text = m_Sets.Length + " sets generated:\r\n\r\n";
			for ( int SetIndex=0; SetIndex < m_Sets.Length; SetIndex++ )
			{
				Set	S = m_Sets[SetIndex];
				textBoxResults.Text += SetIndex + ") " + S.LastSetCardinality + " pixels (" + (100.0f * S.LastSetCardinality / m_ScenePixels.Count).ToString( "G4" ) + "%)\r\n"
									+ "Albedo = (" + S.Albedo.x.ToString( "G4" ) + ", " + S.Albedo.y.ToString( "G4" ) + ", " + S.Albedo.z.ToString( "G4" ) + ")\r\n\r\n";
			}

			integerTrackbarControlSetIsolation.RangeMax = m_Sets.Length-1;
			integerTrackbarControlSetIsolation.VisibleRangeMax = integerTrackbarControlSetIsolation.RangeMax;

//			radioButtonSetIndex.Checked = true;
			outputPanel1.UpdateBitmap();
		}

		#region Flood Fill Algorithm

		const float		DISTANCE_THRESHOLD = 0.02f;										// 2cm
		readonly float	ANGULAR_THRESHOLD = (float) Math.Cos( 0.5 * Math.PI / 180 );	// 0.5°
		readonly float	ALBEDO_HUE_THRESHOLD = 0.02f;									// Close colors!
		float			SOLID_ANGLE_THRESOLD;											// Computed before first call to FloodFill()

		private void	FloodFill( Set _S, Pixel _PreviousPixel, Pixel _P, List<Pixel> _SetPixels, List<Pixel> _SetRejectedPixels )
		{
			if ( _P.ParentSet != null )
				return;	// This pixel is alerady part of some set (possibly this set)
			if ( _P.Infinity )
				return;	// We only account for scene pixels!

			//////////////////////////////////////////////////////////////////////////
			// Check some criterions for a match
			bool	Accepted = false;

			// First, let's check the solid angle
			if ( _P.SceneSolidAngle > SOLID_ANGLE_THRESOLD )	// Shouldn't even be in the list...
			{
				// Next, let's check the distance discrepancy
				float	DistanceDiff = Math.Abs( _PreviousPixel.Distance - _P.Distance );	// We should weight by slope! dot(view,Normal) or something...
				if ( DistanceDiff < DISTANCE_THRESHOLD )
				{
					// Next, let's check the hue discrepancy
					float	HueDiff = Math.Abs( _PreviousPixel.AlbedoHSL.x - _P.AlbedoHSL.x );
							HueDiff *= 0.5f * (_PreviousPixel.AlbedoHSL.y + _P.AlbedoHSL.y);	// Weight by saturation to be less severe with unsaturated colors that can change hue quite fast
					if ( HueDiff < ALBEDO_HUE_THRESHOLD )
					{
						// Next, let's check the angular discrepancy
						float	Dot = _PreviousPixel.Normal | _P.Normal;
						if ( Dot > ANGULAR_THRESHOLD )
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
			_SetPixels.Add( _P );


			//////////////////////////////////////////////////////////////////////////
			// Recurse to 4 neighbors
			Pixel	L = FindAdjacentPixel( _P, -1, 0 );
			FloodFill( _S, _P, L, _SetPixels, _SetRejectedPixels );

			Pixel	R = FindAdjacentPixel( _P, 1, 0 );
			FloodFill( _S, _P, R, _SetPixels, _SetRejectedPixels );

			Pixel	D = FindAdjacentPixel( _P, 0, -1 );
			FloodFill( _S, _P, D, _SetPixels, _SetRejectedPixels );

			Pixel	U = FindAdjacentPixel( _P, 0, 1 );
			FloodFill( _S, _P, U, _SetPixels, _SetRejectedPixels );
		}

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
		int[][]	GoLeftTransforms = new int[6][] {
			// Going left from +X sends us to +Z
			new int[6] {	1,  1,  0,		// X' = C + X	(C is the CUBE_MAP_SIZE)
							0,  0,  1 },	// Y' = Y
			// Going left from -X sends us to -Z
			new int[6] {	1,  1,  0,		// X' = C + X
							0,  0,  1 },	// Y' = Y
			// Going left from +Y sends us to -X
			new int[6] {	1,  0, -1,		// X' = C - Y
							0, -1,  0 },	// Y' = -X
			// Going left from -Y sends us to -X
			new int[6] {	0,  0,  1,		// X' = Y
							1,  1,  0 },	// Y' = C + X
			// Going left from +Z sends us to -X
			new int[6] {	1,  1,  0,		// X' = C + X
							0,  0,  1 },	// Y' = Y
			// Going left from -Z sends us to +X
			new int[6] {	1,  1,  0,		// X' = C + X
							0,  0,  1 },	// Y' = Y
		};
		int[][]	GoRightTransforms = new int[6][] {
			// Going right from +X sends us to -Z
			new int[6] {	-1,  1,  0,		// X' = -C + X	(C is the CUBE_MAP_SIZE)
							0,  0,  1 },	// Y' = Y
			// Going right from -X sends us to +Z
			new int[6] {	-1,  1,  0,		// X' = -C + X
							0,  0,  1 },	// Y' = Y
			// Going right from +Y sends us to +X
			new int[6] {	0,  0,  1,		// X' = Y
							-1, 1,  0 },	// Y' = -C + X
			// Going right from -Y sends us to +X
			new int[6] {	1,  0,  -1,		// X' = C - Y
							1,  1,  0 },	// Y' = C + X
			// Going right from +Z sends us to +X
			new int[6] {	-1,  1,  0,		// X' = -C + X
							0,  0,  1 },	// Y' = Y
			// Going right from -Z sends us to -X
			new int[6] {	-1,  1,  0,		// X' = -C + X
							0,  0,  1 },	// Y' = Y
		};
		int[][]	GoDownTransforms = new int[6][] {
			// Going down from +X sends us to +Y
			new int[6] {	1,  0,  1,		// X' = C + Y	(C is the CUBE_MAP_SIZE)
							0,  1,  0 },	// Y' = X
			// Going down from -X sends us to +Y
			new int[6] {	0,  0, -1,		// X' = -Y
							1, -1,  0 },	// Y' = C - X
			// Going down from +Y sends us to +Z
			new int[6] {	0,  1,  0,		// X' = X
							0,  0, -1 },	// Y' = -Y
			// Going down from -Y sends us to -Z
			new int[6] {	1, -1,  0,		// X' = C - X
							1,  0,  1 },	// Y' = C + Y
			// Going down from +Z sends us to +Y
			new int[6] {	0,  1,  0,		// X' = X
							0,  0, -1 },	// Y' = -Y
			// Going down from -Z sends us to +Y
			new int[6] {	1, -1,  0,		// X' = C - X
							1,  0,  1 },	// Y' = C + Y
		};
		int[][]	GoUpTransforms = new int[6][] {
			// Going up from +X sends us to -Y
			new int[6] {	2,  0, -1,		// X' = 2C - Y	(C is the CUBE_MAP_SIZE)
							1, -1,  0 },	// Y' = C - X
			// Going up from -X sends us to -Y
			new int[6] {	-1,  0,  1,		// X' = -C + Y
							0,  1,  0 },	// Y' = X
			// Going up from +Y sends us to -Z
			new int[6] {	1, -1,  0,		// X' = C - X
							-1, 0,  1 },	// Y' = -C + Y
			// Going up from -Y sends us to +Z
			new int[6] {	0,  1,  0,		// X' = X
							2,  0, -1 },	// Y' = 2C - Y
			// Going up from +Z sends us to -Y
			new int[6] {	0,  1,  0,		// X' = X
							2,  0, -1 },	// Y' = 2C - Y
			// Going up from -Z sends us to -Y
			new int[6] {	1, -1,  0,		// X' = C - X
							-1, 0,  1 },	// Y' = -C + Y
		};

		private Pixel	FindAdjacentPixel( Pixel _P, int _Dx, int _Dy )
		{
			int	PixelIndex = _P.PixelIndex;
			int	CubeFaceIndex = PixelIndex / CUBE_MAP_FACE_SIZE;
			int	CubeFacePixelIndex = PixelIndex - CubeFaceIndex * CUBE_MAP_FACE_SIZE;
			int	Y = PixelIndex / CUBE_MAP_SIZE;
			int	X = PixelIndex - Y * CUBE_MAP_SIZE;

			X += _Dx;
			if ( X < 0 )
			{	// Stepped out through left side
				TransformXY( GoLeftTransforms[CubeFaceIndex], ref X, ref Y );
				CubeFaceIndex = GoLeft[CubeFaceIndex];
			}
			if ( X > CUBE_MAP_SIZE-1 )
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
			if ( Y > CUBE_MAP_SIZE-1 )
			{	// Stepped out through top side
				TransformXY( GoUpTransforms[CubeFaceIndex], ref X, ref Y );
				CubeFaceIndex = GoUp[CubeFaceIndex];
			}

			int	FinalPixelIndex = CUBE_MAP_FACE_SIZE * CubeFaceIndex + CUBE_MAP_SIZE * Y + X;

			return m_ScenePixels[FinalPixelIndex];
		}

		private void	TransformXY( int[] _Transform, ref int _X, ref int _Y )
		{
			int	TempX = _Transform[0] * CUBE_MAP_SIZE + _Transform[1] * _X + _Transform[2] * _Y;
			int	TempY = _Transform[3] * CUBE_MAP_SIZE + _Transform[4] * _X + _Transform[5] * _Y;
			_X = TempX;
			_Y = TempY;
		}

		#endregion

		#endregion

		#region EVENT HANDLERS

		private void radioButtonAlbedo_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.ALBEDO;
		}

		private void radioButtonDistance_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.DISTANCE;
		}

		private void radioButtonNormal_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.NORMAL;
		}

		private void radioButtonSetIndex_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.SET_INDEX;
		}

		private void radioButtonSetColor_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.SET_ALBEDO;
		}

		private void radioButtonSetNormal_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.SET_NORMAL;
		}

		private void radioButtonSetDistance_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.SET_DISTANCE;
		}

		private void integerTrackbarControlSetIsolation_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			outputPanel1.IsolatedSetIndex = _Sender.Value;
		}

		private void checkBoxSetIsolation_CheckedChanged( object sender, EventArgs e )
		{
			outputPanel1.IsolateSet = checkBoxSetIsolation.Checked;
		}

		#endregion
	}
}
