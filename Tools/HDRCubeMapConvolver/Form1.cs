using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using WMath;

namespace TestGradientPNG
{
	public partial class Form1 : Form
	{
		public unsafe Form1()
		{
			InitializeComponent();

			// Read HDR cube map as a cross image
			FileInfo	SourceFile = new FileInfo( "kitchen_cross.float" );

			int	Width, Height;
			Vector4D[,]	HDRValues = null;
			using ( Stream S = SourceFile.OpenRead() )
				using ( BinaryReader R = new BinaryReader( S ) )
				{
					Width = R.ReadInt32();
					Height = R.ReadInt32();
					HDRValues = new Vector4D[Width,Height];

					for ( int Y=0; Y < Height; Y++ )
						for ( int X=0; X < Width; X++ )
						{
							float	Red = R.ReadSingle();
							float	Green = R.ReadSingle();
							float	Blue = R.ReadSingle();
							HDRValues[X,Y] = new Vector4D( Red, Green, Blue, 1 );
						}
				}

			// Extract cube faces
			int	CubeSize = 0;
			Vector4D[][,]	CubeFaces = new Vector4D[6][,] { null, null, null, null, null, null };
			if ( Height > Width )
			{	// Vertical cross
				CubeSize = Height / 4;
				if ( Width != CubeSize * 3 )
					throw new Exception( "BROUTE!" );

				CubeFaces[0] = ReadCubeFace( HDRValues, CubeSize, 2, 1 );	// +X
				CubeFaces[1] = ReadCubeFace( HDRValues, CubeSize, 0, 1 );	// -X
				CubeFaces[2] = ReadCubeFace( HDRValues, CubeSize, 1, 0 );	// +Y
				CubeFaces[3] = ReadCubeFace( HDRValues, CubeSize, 1, 2 );	// -Y
				CubeFaces[4] = ReadCubeFace( HDRValues, CubeSize, 1, 1 );	// +Z
				CubeFaces[5] = ReadCubeFace( HDRValues, CubeSize, 1, 3, true );	// -Z
			}
			else
			{	// Horizontal cross
				CubeSize = Width / 4;
				if ( Height != CubeSize * 3 )
					throw new Exception( "BROUTE!" );

				CubeFaces[0] = ReadCubeFace( HDRValues, CubeSize, 2, 1 );	// +X
				CubeFaces[1] = ReadCubeFace( HDRValues, CubeSize, 0, 1 );	// -X
				CubeFaces[2] = ReadCubeFace( HDRValues, CubeSize, 1, 0 );	// +Y
				CubeFaces[3] = ReadCubeFace( HDRValues, CubeSize, 1, 2 );	// -Y
				CubeFaces[4] = ReadCubeFace( HDRValues, CubeSize, 1, 1 );	// +Z
				CubeFaces[5] = ReadCubeFace( HDRValues, CubeSize, 3, 1 );	// -Z
			}

			Vector4D[][][,]	CubeFacesMips = ConvolveCubeMap( CubeSize, CubeFaces );

			DirectXTexManaged.CubeMapCreator.CreateCubeMapFile( "Test.dds", CubeSize, CubeFacesMips );
		}

		/// <summary>
		/// This function is the heart of that tool
		/// 
		/// The goal is to compute mip levels for the cube map where each new mip will match the corresponding roughness of an exponential lobe
		///		like those used in standard normal distribution models (Ward, Beckmann, etc.) so that we can use a specific mip according to the
		///		roughness parameter of the model.
		/// 
		/// Ignoring normalization factors, the typical reflection lobe is given by the following equation:
		///		f(theta) = exp( -tan(theta)² / m² )	     => m is the roughness
		///	
		/// Fooplot link for different plots with different roughnesses (reference cosine lobe in red) (black lines are the tangents found from roughness):
		///	http://fooplot.com/#W3sidHlwZSI6MSwiZXEiOiJleHAoLSh0YW4oYWJzKHRoZXRhLXBpLzIpKS8wLjAyKV4yKSIsImNvbG9yIjoiIzAwODBDQyIsInRoZXRhbWluIjoiMCIsInRoZXRhbWF4IjoicGkiLCJ0aGV0YXN0ZXAiOiIuMDEifSx7InR5cGUiOjAsImVxIjoieC90YW4oMC4wMTI3KSIsImNvbG9yIjoiIzAwMDAwMCJ9LHsidHlwZSI6MSwiZXEiOiJleHAoLSh0YW4oYWJzKHRoZXRhLXBpLzIpKS8wLjEpXjIpIiwiY29sb3IiOiIjRkFCMzE5IiwidGhldGFtaW4iOiIwIiwidGhldGFtYXgiOiJwaSIsInRoZXRhc3RlcCI6Ii4wMSJ9LHsidHlwZSI6MCwiZXEiOiJ4L3RhbigwLjEzKSIsImNvbG9yIjoiIzAwMDAwMCJ9LHsidHlwZSI6MSwiZXEiOiJleHAoLSh0YW4oYWJzKHRoZXRhLXBpLzIpKS8wLjIpXjIpIiwiY29sb3IiOiIjMDBDQzQxIiwidGhldGFtaW4iOiIwIiwidGhldGFtYXgiOiIycGkiLCJ0aGV0YXN0ZXAiOiIuMDEifSx7InR5cGUiOjAsImVxIjoieC90YW4oMC4yNSkiLCJjb2xvciI6IiMwMDAwMDAifSx7InR5cGUiOjEsImVxIjoiZXhwKC0odGFuKGFicyh0aGV0YS1waS8yKSkvMC41KV4yKSIsImNvbG9yIjoiIzAwODBjYyIsInRoZXRhbWluIjoiMCIsInRoZXRhbWF4IjoiMnBpIiwidGhldGFzdGVwIjoiLjAxIn0seyJ0eXBlIjowLCJlcSI6IngvdGFuKDAuNTkpIiwiY29sb3IiOiIjMDAwMDAwIn0seyJ0eXBlIjoxLCJlcSI6ImV4cCgtKHRhbihhYnModGhldGEtcGkvMikpLzEuMCleMikiLCJjb2xvciI6IiMwMDgwY2MiLCJ0aGV0YW1pbiI6IjAiLCJ0aGV0YW1heCI6IjJwaSIsInRoZXRhc3RlcCI6Ii4wMSJ9LHsidHlwZSI6MCwiZXEiOiJ4L3RhbigwLjk3KSIsImNvbG9yIjoiIzAwMDAwMCJ9LHsidHlwZSI6MSwiZXEiOiJleHAoLSh0YW4oYWJzKHRoZXRhLXBpLzIpKS8xLjUpXjIpIiwiY29sb3IiOiIjMDA4MGNjIiwidGhldGFtaW4iOiIwIiwidGhldGFtYXgiOiIycGkiLCJ0aGV0YXN0ZXAiOiIuMDEifSx7InR5cGUiOjAsImVxIjoieC90YW4oMS4xOCkiLCJjb2xvciI6IiMwMDAwMDAifSx7InR5cGUiOjEsImVxIjoiY29zKCh0aGV0YS1waS8yKSkiLCJjb2xvciI6IiNGRjAwNjYiLCJ0aGV0YW1pbiI6IjAiLCJ0aGV0YW1heCI6InBpIiwidGhldGFzdGVwIjoiLjAxIn0seyJ0eXBlIjoxMDAwLCJ3aW5kb3ciOlsiLTAuNzQ5OTk5OTk5OTk5OTk5OCIsIjAuNzQ5OTk5OTk5OTk5OTk5OCIsIjEuMTEwMjIzMDI0NjI1MTU2NWUtMTYiLCIxLjEiXX1d
		/// 
		/// 
		///  Problem:
		/// ----------
		///  Given a solid angle omega, find the corresponding roughness of the gaussian lobe model that generates a lobe that roughly spans omega steradians
		/// 
		/// > Assuming the lobe is rotationaly symetric about its main direction, we can reduce the solid angle to a single aperture angle "alpha"
		/// 
		/// > In polar coordinates, a point p is on the lobe if:
		/// 
		///		Xp = r.sin(theta)
		///		Yp = r.cos(theta)
		///		r = exp( -(tan(theta)/m)² )
		/// 
		/// > We can draw:
		/// 
		///		 ^
		///		 |...  alpha
		///		 |    ... 
		///		 |        ./
		///		 |___     /
		///		 |   \   /
		///		 |    \ /
		///		 |    |/
		///		 |    |
		///		 |   /
		///		 |  / 
		///		 | * p_epsilon
		///		 |/_____________
		/// 
		/// According to the problem we posed, we give an aperture angle of alpha and want to find the "tangent lobe".
		/// There is no exact way to do that since the tangent at theta=PI/2 is always 0, but we can fix conditions for the p coordinate.
		/// For example, at a given epsilon we want p to match a point p_epsilon on our tangent line:
		/// 
		///		Xp_epsilon = eps.tan(alpha)
		///		Yp_epsilon = eps
		///		
		/// In polar coordinates:
		///		Xp_epsilon = r_epsilon * sin(alpha)
		///		Yp_epsilon = r_epsilon * cos(alpha)
		///		
		/// =>	r_epsilon = Yp_epsilon / cos(alpha)
		/// =>	r_epsilon = eps / cos(alpha)
		/// 
		/// So we pose p = p_epsilon and the following equalities arise:
		/// 
		///		Xp = Xp_epsilon
		///		Yp = Yp_epsilon
		///		theta = alpha
		///		r = r_epsilon
		/// 
		/// Or:
		///		exp( -(tan(alpha)/m)² ).sin(alpha) = eps.tan(alpha)
		///		exp( -(tan(alpha)/m)² ).cos(alpha) = eps
		///		exp( -(tan(alpha)/m)² ) = eps / cos(alpha)
		/// 
		/// We have 3 identical equations:
		/// 
		///		exp( -(tan(alpha)/m)² ) = eps / cos(alpha)
		///	
		/// =>	(tan(alpha)/m)² = -ln( eps / cos(alpha) )
		///	=>	tan(alpha)/m = sqrt( -ln( eps / cos(alpha) ) )
		///	
		/// Finally:
		/// 
		///		.-----------------------------------------------------.
		///		|	m = tan(alpha) / sqrt( -ln( eps / cos(alpha) ) )  |
		///		.-----------------------------------------------------.
		/// 
		/// 
		/// Fooplot link for different epsilons:
		/// http://www.fooplot.com/#W3sidHlwZSI6MCwiZXEiOiJ0YW4oeCkvc3FydCgtbG4oMC4wMDEvY29zKHgpKSkiLCJjb2xvciI6IiNGRjAwMDAifSx7InR5cGUiOjAsImVxIjoidGFuKHgpL3NxcnQoLWxuKDAuMDEvY29zKHgpKSkiLCJjb2xvciI6IiMwQkQxMzkifSx7InR5cGUiOjAsImVxIjoidGFuKHgpL3NxcnQoLWxuKDAuMDUvY29zKHgpKSkiLCJjb2xvciI6IiNGQ0NBMDAifSx7InR5cGUiOjAsImVxIjoidGFuKHgpL3NxcnQoLWxuKDAuMS9jb3MoeCkpKSIsImNvbG9yIjoiIzAwMDlGRiJ9LHsidHlwZSI6MTAwMCwid2luZG93IjpbIjAiLCIxLjU3IiwiMCIsIjgiXX1d
		/// 
		/// We can see from the graph that the lower the epsilon, the lower the roughness and the tighter the lobe.
		/// 
		/// 
		/// ---------------------------------------------------------------
		/// Now, for the second part of the problem: in the shader, we're given the roughness m and we're required to find the proper
		///	 aperture angle alpha so we can deduce the mip level to fetch from the cube map.
		/// 
		/// Unfortunately, there's no analytical solution for the roots of equation exp( -(tan(alpha)/m)² ).cos(alpha) - eps = 0
		/// 
		/// What we can do though is to fix an epsilon of say eps = 0.2 and find the roots manually for different roughnesses, store them in
		///  an array and try to fit a function through those points...
		/// 
		/// So, for epsilon = 0.2 we get:
		/// 
		///		m		0.0100	0.10	0.20	0.30	0.40	0.50	0.60	0.70	0.80	0.90	1.00	1.10	1.20	1.30	1.40	1.50
		///		alpha	0.0127	0.13	0.25	0.37	0.48	0.59	0.69	0.77	0.85	0.92	0.97	1.03	1.07	1.11	1.15	1.18 ~= 67° for the most diffuse lobe
		/// 
		/// Fooplot link with the equations used for finding the roots:
		/// http://www.fooplot.com/#W3sidHlwZSI6MCwiZXEiOiJleHAoLSh0YW4oeCkvMC41KV4yKS9jb3MoeCktMC4yIiwiY29sb3IiOiIjRkYwMDAwIn0seyJ0eXBlIjowLCJlcSI6ImV4cCgtKHRhbih4KS8wLjAxKV4yKS9jb3MoeCktMC4yIiwiY29sb3IiOiIjMjZEMTA0In0seyJ0eXBlIjowLCJlcSI6ImV4cCgtKHRhbih4KS8xLjQpXjIpL2Nvcyh4KS0wLjIiLCJjb2xvciI6IiMwMDQ4RkYifSx7InR5cGUiOjEwMDAsIndpbmRvdyI6WyIwIiwiMS40IiwiLTAuMyIsIjEiXX1d
		/// 
		/// Using an online least square fitter (http://www.akiti.ca/LinLeastSqPoly4.html) we find the excellent matching result:
		/// 
		///		alpha = -0.003801480606631629 + 1.3782584461528633 * m - 0.3967194013427133 * m^2
		/// 
		/// Fooplot link for the point list and its the linear and quadratic fits:
		/// http://www.fooplot.com/#W3sidHlwZSI6MywiZXEiOltbIjAuMDEwMCIsIiAwLjAxMjciXSxbIjAuMTAiLCIgMC4xMyJdLFsiMC4yMCIsIiAwLjI1Il0sWyIwLjMwIiwiIDAuMzciXSxbIjAuNDAiLCIgMC40OCJdLFsiMC41MCIsIiAwLjU5Il0sWyIwLjYwIiwiIDAuNjkiXSxbIjAuNzAiLCIgMC43NyJdLFsiMC44MCIsIiAwLjg1Il0sWyIwLjkwIiwiIDAuOTIiXSxbIjEuMDAiLCIgMC45NyJdLFsiMS4xMCIsIiAxLjAzIl0sWyIxLjIwIiwiIDEuMDciXSxbIjEuMzAiLCIgMS4xMSJdLFsiMS40MCIsIiAxLjE1XHQiXSxbIjEuNTAiLCIgMS4xOFx0XHRcdFx0XHRcdFx0XHRcdFx0XHRcdFx0Il1dLCJjb2xvciI6IiMwMDAwMDAifSx7InR5cGUiOjAsImVxIjoiMC4xMzY2OTg0Mjk2OTYzNyswLjc4MTQ3NTg2Mzg1MTYzKngiLCJjb2xvciI6IiMwMDAwMDAifSx7InR5cGUiOjAsImVxIjoiLTAuMzk2NzE5NDAxMzQyNzEzMyp4XjIrMS4zNzgyNTg0NDYxNTI4NjMzKngtMC4wMDM4MDE0ODA2MDY2MzE2MjkiLCJjb2xvciI6IiMwMDAwMDAifSx7InR5cGUiOjEwMDAsIndpbmRvdyI6WyIwIiwiMS42IiwiMCIsIjEuNCJdfV0-
		/// 
		/// 
		/// ---------------------------------------------------------------
		/// Normally, with each new mip level, the tangent of the aperture angle is doubled.
		/// 
		/// At mip #0, tan( alpha ) = 0.5 / CubeSize so we cover a single pixel
		/// At mip #1, tan( alpha ) = 2 * 0.5 / CubeSize
		/// At mip #2, tan( alpha ) = 4 * 0.5 / CubeSize
		/// At mip #3, tan( alpha ) = 8 * 0.5 / CubeSize
		/// ...
		/// At mip #N, tan( alpha ) = CubeSize / CubeSize => alpha = PI/4, we cover Omega = 2PI * (1 - cos(alpha)) = PI * (2 - sqrt(2)) ~= 1.84 steradians (from http://en.wikipedia.org/wiki/Solid_angle#Cone.2C_spherical_cap.2C_hemisphere)
		/// 
		/// In our case, we want to be able to cover up to alpha = 67°, corresponding to the maximum roughness of m = 1.5
		/// If you look at an example of a diffuse cube map http://www.3dvia.com/studio/wp-content/uploads/2009/11/hangar_diffuse.png you can see it still needs a little resolution
		///	 and so we can't assume it will be the highest mip level...
		/// 
		/// By fixing the "diffuse mip" so it's a cube map of 4x4, for example, we can have N-2 mips where the angle will vary from 0 to 67°
		/// 
		/// Take the example of a 64x64 cube map (N=7):
		///		Mip #0 = 64x64	-> alpha = 0°	(m=0.01)
		///		Mip #1 = 32x32	-> alpha = 15°	(m=0.20)
		///		Mip #2 = 16x16	-> alpha = 33°	(m=0.50)
		///		Mip #3 =  8x8	-> alpha = 48°	(m=0.80)
		///		Mip #4 =  4x4	-> alpha = 67°	(m=1.50)
		///	  -------------------------------------------
		///		Mip #5 =  2x2	-> alpha = 67°
		///		Mip #6 =  1x1	-> alpha = 67°
		/// 
		/// Using the above formula for finding alpha from roughness, we can easily get the mip level index:
		/// 
		///		Mip = (-0.003801480606631629 + 1.3782584461528633 * m - 0.3967194013427133 * m^2) * (N-3) / 1.18
		/// 
		/// 
		/// This is the conclusion of this long explanation: we can deduces alpha and roughness from each other and we have a nice simple way of computing the mip levels of the cube map.
		/// 
		/// </summary>
		/// <param name="_CubeFaces"></param>
		/// <returns></returns>
		/// 
		private const float	EPS = 0.2f;				// The epsilon at which we perform the computations
		private const float	MAX_ALPHA = 1.18f;		// The maximum angle reached at the last mip corresponding to the maximum roughness of 1.5
		private const float	SAMPLES_FACTOR = 0.1f;	// A global factor to avoid using too many samples for the convolution

		private int				m_CubeSize;
		private Vector4D[][,]	m_CubeFaces;
		private Vector4D[][][,]	ConvolveCubeMap( int _CubeSize, Vector4D[][,] _CubeFaces )
		{
			m_CubeSize = _CubeSize;
			m_CubeFaces = _CubeFaces;

			// Compute necessary mip levels
			int	MipLevels = 1 + (int) Math.Floor( Math.Log( m_CubeSize ) / Math.Log( 2 ) );	// This would be the total amount of mips for the entire chain
				MipLevels -= 2;																// But as stated above, we limit ourselves down to the lowest mip of 4x4 pixels

			// Compute the total amount of pixels from one side to the other side of the cube
			//	if we split the hemicube with a plane.
			// This roughly corresponds to the amount of pixels we would span if we had an aperture angle of PI/2 (total angle of PI)
			// That will help us determine the amount of samples to take based on actual aperture angle...
			//
			//			 Full
			//		  ___________
			//		 |           |
			//  Half |           | Half
			//		 |...........|
			//
			int	TotalPixels = m_CubeSize / 2	// Half a cube size on the left
							+ m_CubeSize		// A full cube size on the top
							+ m_CubeSize / 2;	// Half a cube size on the right


			// Build all the mips from mip 0
			Vector4D[][][,]	Result = new Vector4D[MipLevels][][,];
			Result[0] = _CubeFaces;	// Mip 0 is already available

			Vector	X = new Vector();
			Vector	Y = new Vector();
			Vector	Z = new Vector();
			Vector	Direction = new Vector();
			float	U, V;
			for ( int MipIndex=1; MipIndex < MipLevels; MipIndex++ )
			{
				int				MipCubeSize = m_CubeSize >> MipIndex;
				Vector4D[][,]	MipCubeFaces = new Vector4D[6][,];
				Result[MipIndex] = MipCubeFaces;

				// Compute expected lobe angle
				float	Alpha = MAX_ALPHA * MipIndex / (MipLevels-1);

				// Compute equivalent roughness
				float	m = (float) (Math.Tan(Alpha) / Math.Sqrt( -Math.Log( EPS / Math.Cos(Alpha) ) ));

				// Compute amount of samples along alpha & phi depending on original cube size
				int		SamplesCountTheta = (int) Math.Floor( SAMPLES_FACTOR * 2.0 * Alpha * TotalPixels / Math.PI );	// A simple ratio based on the total pixels if we had a PI/2 aperture...

SamplesCountTheta = 3 * MipIndex;

				float	dTheta = Alpha / SamplesCountTheta;
				int		SamplesCountPhi = (int) Math.Floor( 2.0 * Math.PI / dTheta );	// Approximately the same spacing in Phi

				// Build samples
				Vector4D[]	Samples = new Vector4D[SamplesCountPhi * SamplesCountTheta];
				int			SamplesCount = 0;
				float		Normalizer = 1.0f / SamplesCountPhi * SamplesCountTheta;

				Random		RNG = new Random( 1 );
				for ( int ThetaIndex=0; ThetaIndex < SamplesCountTheta; ThetaIndex++ )
				{
					for ( int PhiIndex=0; PhiIndex < SamplesCountPhi; PhiIndex++ )
					{
						float	Theta = (float) Math.Sqrt( -m*m * Math.Log( (ThetaIndex + RNG.NextDouble()) / SamplesCountTheta ) );	// According to ward's monte-carlo sampling method (stratified version)
						float	Phi = (float) (PhiIndex + RNG.NextDouble()) * dTheta;

						float	CosTheta = (float) Math.Cos( Theta );
						float	SinTheta = (float) Math.Sin( Theta );

						float	CosPhi = (float) Math.Cos( Phi );
						float	SinPhi = (float) Math.Sin( Phi );

						float	Reflectance = (float) Math.Exp( -Math.Pow( Math.Tan( Theta ) / m, 2.0 ) );	// Gaussian lobe reflectance in that direction, normalized against amount of samples taken
//Reflectance = 1;
								Reflectance *= Normalizer;

						Samples[SamplesCount++] = new Vector4D(
								SinTheta * CosPhi,
								SinTheta * SinPhi,
								CosTheta,
								Reflectance
							);
					}
				}


				// Perform convolution
				for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ )
				{
					Vector4D[,]	CubeFace = new Vector4D[MipCubeSize,MipCubeSize];
					MipCubeFaces[FaceIndex] = CubeFace;

					switch ( FaceIndex )
					{
						case 0:	// +X
							X.Set( 0, 0, -1 );
							Y.Set( 0, -1, 0 );
							Z.Set( 1, 0, 0 );
							break;
						case 1:	// -X
							X.Set( 0, 0, 1 );
							Y.Set( 0, -1, 0 );
							Z.Set( -1, 0, 0 );
							break;
						case 2:	// +Y
							X.Set( 1, 0, 0 );
							Y.Set( 0, 0, 1 );
							Z.Set( 0, 1, 0 );
							break;
						case 3:	// -Y
							X.Set( 1, 0, 0 );
							Y.Set( 0, 0, -1 );
							Z.Set( 0, -1, 0 );
							break;
						case 4:	// +Z
							X.Set( 1, 0, 0 );
							Y.Set( 0, -1, 0 );
							Z.Set( 0, 0, 1 );
							break;
						case 5:	// -Z
							X.Set( -1, 0, 0 );
							Y.Set( 0, -1, 0 );
							Z.Set( 0, 0, -1 );
							break;
					}

					Vector	Up = new Vector( 0, 1, 0 );
					Vector	T, B;
					float	Length;
					for ( int y=0; y < MipCubeSize; y++ )
					{
						V = 2.0f * (1+y) / (MipCubeSize+1) - 1.0f;
						for ( int x=0; x < MipCubeSize; x++ )
						{
							U = 2.0f * (1+x) / (MipCubeSize+1) - 1.0f;

							Direction = Z + U * X + V * Y;
							Direction.Normalize();

// Simple direction test...
// CubeFace[x,y] = new Vector4D( Direction.x, Direction.y, Direction.z, 1 );
// CubeFace[x,y] = PointSampleCubeMap( Direction );
// continue;

							// Establish a tangent space
							T = Direction ^ Up;
							Length = T.Magnitude();
							if ( Length > 1e-6f )
							{
								T /= Length;
								B = T ^ Direction;
							}
							else
							{	// Degenerate case
								T = new Vector( 1, 0, 0 );
								B = new Vector( 0, 0, 1 );
							}

							// Accumulate samples
							Vector4D	Accum = new Vector4D();
							for ( int SampleIndex=0; SampleIndex < SamplesCount; SampleIndex++ )
							{
								Vector4D	Sample = Samples[SampleIndex];

//Sample.Set( 0, 0, 1, 1.0f/SamplesCount );

								Vector		SamplingDirection = Sample.x * T + Sample.y * B + Sample.z * Direction;	//X = 0.7170359 Y = -0.530138135 Z = -0.45256263

								Vector4D	C = PointSampleCubeMap( SamplingDirection );
								Accum.x += Sample.w * C.x;
								Accum.y += Sample.w * C.y;
								Accum.z += Sample.w * C.z;
							}

							CubeFace[x,y] = Accum;	// Here's our final result!


// 							// Maya seems to cut HDR values!!
// 							// Let's save exponent in alpha
// 							float	Max = Math.Max( Math.Max( Accum.x, Accum.y ), Accum.z );
// 							float	LogVal = (float) Math.Log( Max ) / 3.9120230054281460586187507879106f;	// Assume a max of 50
// 							Accum /= Max;
// 							Accum.w = LogVal;
//							CubeFace[x,y] = Accum;	// Here's our final result!

						}
					}
				}
			}

			return Result;
		}

		private Vector4D	PointSampleCubeMap( Vector _Direction )
		{
			int			Xs = Math.Sign( _Direction.x );
			int			Ys = Math.Sign( _Direction.y );
			int			Zs = Math.Sign( _Direction.z );
			float		X = Math.Abs( _Direction.x );
			float		Y = Math.Abs( _Direction.y );
			float		Z = Math.Abs( _Direction.z );
			float		I;
			float		U, V;
			Vector4D[,]	Face;
			if ( X >= Y )
			{
				if ( X >= Z )
				{	// X face
					I = -1.0f / X;
					U = Xs * _Direction.z * I;
					V = _Direction.y * I;
					Face = Xs > 0 ? m_CubeFaces[0] : m_CubeFaces[1];
				}
				else
				{	// Z face
					I = 1.0f / Z;
					U = Zs * _Direction.x * I;
					V = -_Direction.y * I;
					Face = Zs > 0 ? m_CubeFaces[4] : m_CubeFaces[5];
				}
			}
			else
			{
				if ( Y >= Z )
				{	// Y face
					I = 1.0f / Y;
					U = _Direction.x * I;
					V = Ys * _Direction.z * I;
					Face = Ys > 0 ? m_CubeFaces[2] : m_CubeFaces[3];
				}
				else
				{	// Z face
					I = 1.0f / Z;
					U = Zs * _Direction.x * I;
					V = -_Direction.y * I;
					Face = Zs > 0 ? m_CubeFaces[4] : m_CubeFaces[5];
				}
			}

			// Sample the face
			int	Ui = (int) Math.Floor( m_CubeSize * 0.5f*(1.0f+U) );
				Ui = Math.Min( m_CubeSize-1, Ui );
			int	Vi = (int) Math.Floor( m_CubeSize * 0.5f*(1.0f+V) );
				Vi = Math.Min( m_CubeSize-1, Vi );

			Vector4D	Result = Face[Ui,Vi];
			return Result;
		}

		private Vector4D[,]	ReadCubeFace( Vector4D[,] _Source, int _CubeSize, int _X, int _Y )
		{
			return ReadCubeFace( _Source, _CubeSize, _X, _Y, false );
		}
		private Vector4D[,]	ReadCubeFace( Vector4D[,] _Source, int _CubeSize, int _X, int _Y, bool _Flip )
		{
			int	X = _CubeSize * _X;
			int	Y = _CubeSize * _Y;

			Vector4D[,]	Result = new Vector4D[_CubeSize,_CubeSize];
			if ( _Flip )
			{
				for ( int y = 0; y < _CubeSize; y++ )
					for ( int x = 0; x < _CubeSize; x++ )
						Result[x, y] = _Source[X + _CubeSize-1-x, Y + _CubeSize-1-y];
			}
			else
			{
				for ( int y = 0; y < _CubeSize; y++ )
					for ( int x = 0; x < _CubeSize; x++ )
						Result[x, y] = _Source[X + x, Y + y];
			}

			return Result;
		}

		private void floatTrackbarControlScaleX_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.ScaleX = _Sender.Value;
		}

		private void floatTrackbarControlScaleY_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.ScaleY = _Sender.Value;
		}

		private void floatTrackbarControlWhitePoint_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.WhitePoint = _Sender.Value;
		}

		private void floatTrackbarControlA_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.A = _Sender.Value;
		}

		private void floatTrackbarControlB_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.B = _Sender.Value;
		}

		private void floatTrackbarControlC_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.C = _Sender.Value;
		}

		private void floatTrackbarControlD_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.D = _Sender.Value;
		}

		private void floatTrackbarControlE_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.E = _Sender.Value;
		}

		private void floatTrackbarControlF_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.F = _Sender.Value;
		}
	}
}
