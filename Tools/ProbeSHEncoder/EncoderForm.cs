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
		public const int	CUBE_MAP_SIZE = 128;
		public const float	Z_INFINITY = 1e6f;
		public const float	Z_INFINITY_TEST = 0.99f * Z_INFINITY;

		public class	Pixel 
		{
			public WMath.Point	Position;	// World position
			public WMath.Vector	Normal;		// World normal
			public WMath.Vector	Albedo;		// Material albedo
			public WMath.Vector	AlbedoHSL;	// Material albedo in HSL format
			public float		F0;			// Material Fresnel coefficient
			public double		SolidAngle;	// Solid angle covered by the pixel
			public float		Distance;	// Distance to hit point
			public bool			Infinity;	// True if not a scene pixel (i.e. sky pixel)
			public WMath.Vector	View;		// View vector pointing to that pixel

			public int			SetIndex = -1;

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
					S /=  1.0f - Math.Abs( 2.0f * L - 1.0f );
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

				AlbedoHSL = new WMath.Vector( H, S, L );
			}

			/// <summary>
			/// Computes a "distance" between this pixel and another one
			/// </summary>
			/// <param name="_Other"></param>
			/// <returns></returns>
			public float	ComputeMetric( Pixel _Other, float _ColorDistanceWeight )
			{
				float	EuclidianDistance = (_Other.Position - Position).Length();
				float	ColorDistance = Math.Min( Math.Abs(_Other.AlbedoHSL.x - AlbedoHSL.x) / 6.0f, Math.Abs(AlbedoHSL.x - _Other.AlbedoHSL.x) / 6.0f );
						ColorDistance *= _ColorDistanceWeight;	// Used to give the color as much weight as general euclidian distances...

				return EuclidianDistance + ColorDistance;
			}
		}

		public WMath.Matrix4x4[]	m_Side2World = new WMath.Matrix4x4[6];

		public Pixel[][,]			m_CubeMap = new Pixel[6][,];
		public double				m_MeanHarmonicDistance = 0.0;

		public List<Pixel>			m_ScenePixels = new List<Pixel>();

		private RegistryKey			m_AppKey;
		private string				m_ApplicationPath;

		public EncoderForm()
		{
			InitializeComponent();

			outputPanel1.m_Owner = this;

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\ProbeSHEncoder" );
			m_ApplicationPath = Path.GetDirectoryName( Application.ExecutablePath );

			PrepareCubeMapFaceTransforms();
			LoadCubeMap();
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
						m_CubeMap[CubeFaceIndex][X,Y] = new Pixel();

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
								Pix.Distance = Distance;
								Pix.Infinity = Distance > Z_INFINITY_TEST;

								if ( !m_CubeMap[CubeFaceIndex][X,Y].Infinity )
								{	// Account for a new scene pixel (i.e. not infinity)
									m_ScenePixels.Add( Pix );
									m_MeanHarmonicDistance += 1.0 / Distance;
								}
							}
			}

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

		private void buttonCompute_Click( object sender, EventArgs e )
		{
			const int	K = 8;
			const float	CHANGES_RATIO_THRESHOLD = 0.01f;	// We can exit the loop if less than 1% of the scene pixels changed of set during the last 2 loop iterations

			// Determine K centroids by splitting the sphere
			Pixel[]	Centroids = new Pixel[K];
			for ( int CentroidIndex=0; CentroidIndex < K; CentroidIndex++ )
			{
				// We first choose a "random" direction (actually, we're splitting the sphere into 8 equal parts) (an octahedron)
				float	Phi = 2.0f * (float) Math.PI * (CentroidIndex & 3) / 4.0f;
				float	Theta = (float) Math.PI * (0.5f + (CentroidIndex >> 3) ) / 2.0f;

				WMath.Vector	TargetDirection = new WMath.Vector( 
						(float) (Math.Cos( Phi ) * Math.Sin( Theta )),
						(float) Math.Cos( Theta ),
						(float) (Math.Sin( Phi ) * Math.Sin( Theta )) );

				// Now we collect all the points that are the closest to the direction and keep the ones that have the most weight
				WMath.Vector	CentroidPosition = WMath.Vector.Zero;
				WMath.Vector	CentroidAlbedo = WMath.Vector.Zero;
				float			SumWeights = 0.0f;
				float			SumWeightColors = 0.0f;
				foreach ( Pixel P in m_ScenePixels )
				{
					float	WeightDirection = Math.Max( 0.0f, TargetDirection | P.View );
					float	WeightColor = 1.0f + 1.0f * P.AlbedoHSL.y;	// Favor the most saturated colors
					float	Weight = WeightDirection * WeightColor;

					CentroidPosition += Weight * (WMath.Vector) P.Position;
					CentroidAlbedo += WeightColor * P.Albedo;
					SumWeights += Weight;
					SumWeightColors += WeightColor;
				}

				CentroidPosition /= SumWeights;
				CentroidAlbedo /= SumWeightColors;
				float	EnsureDot = CentroidPosition.Normalized | TargetDirection;	// Should still point close to the original target direction...

				Centroids[CentroidIndex] = new Pixel() { Position = (WMath.Point) CentroidPosition };
				Centroids[CentroidIndex].SetAlbedo( CentroidAlbedo );
				Centroids[CentroidIndex].View = WMath.Vector.Zero;	// Reset view vectors that we'll use as centroid accumulators
				Centroids[CentroidIndex].SetIndex = 0;				// And we'll use this to count the amount of pixels related to this set
				Centroids[CentroidIndex].Infinity = true;			// We use this flag to check if the set is valid
			}

			// Iterate over the scene pixels to determine which set they prefer
			float	PreviousChangesRatio = 1.0f;
			float	ColorDistanceWeight = (float) m_MeanHarmonicDistance;
			while ( true )
			{
				// Iterate over all pixels and see where their loyalty lies, depending on their "distance" to each set
				int	ChangesCount = 0;
				foreach ( Pixel P in m_ScenePixels )
				{
					float	BestSetDistance = 1e6f;
					int		BestSetIndex = -1;
					for ( int CentroidIndex=0; CentroidIndex < K; CentroidIndex++ )
						if ( Centroids[CentroidIndex].Infinity )
						{
							float	SetDistance = P.ComputeMetric( Centroids[CentroidIndex], ColorDistanceWeight );
							if ( SetDistance >= BestSetDistance )
								continue;

							BestSetDistance = SetDistance;
							BestSetIndex = CentroidIndex;
						}

					if ( BestSetIndex == -1 )
						throw new Exception( "WTF?! Means no more sets are available!" );

					// Accumulate centroid position
					Centroids[BestSetIndex].View += (WMath.Vector) P.Position;
					Centroids[BestSetIndex].SetIndex++;	// One more pixel in this set!

					// Check if there's any change in the set
					if ( BestSetIndex == P.SetIndex )
						continue;

					// Update set!
					P.SetIndex = BestSetIndex;
					ChangesCount++;
				}

				// Update new centroid positions
				for ( int CentroidIndex=0; CentroidIndex < K; CentroidIndex++ )
					if ( Centroids[CentroidIndex].Infinity )
					{
						if ( Centroids[CentroidIndex].SetIndex > 0 )
							Centroids[CentroidIndex].Position = (WMath.Point) (Centroids[CentroidIndex].View / Centroids[CentroidIndex].SetIndex);
						else
						{	// This set is empty, we should mark it as disabled...
							Centroids[CentroidIndex].Infinity = false;
						}

						Centroids[CentroidIndex].View.MakeZero();	// Reset view vectors that we'll use as centroid accumulators
						Centroids[CentroidIndex].SetIndex = 0;		// And we'll use this to count the amount of pixels related to this set
					}

				// Check if we can leave because not many changes were made
				float	ChangesRatio = (float) ChangesCount / m_ScenePixels.Count;
				if ( PreviousChangesRatio < CHANGES_RATIO_THRESHOLD && ChangesRatio < CHANGES_RATIO_THRESHOLD )
					break;	// Done!

				PreviousChangesRatio = ChangesRatio;
			}
		}
	}
}
