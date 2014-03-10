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
using Microsoft.Win32;

namespace TestGradientPNG
{
	public partial class FormCubeMapBaker : Form
	{
		#region NESTED TYPES

		/// <summary>
		/// This format is a special encoding of 3 floating point values into 4 byte values, aka "Real Pixels"
		/// The RGB encode the mantissa of each RGB float component while A encodes the exponent by which multiply these 3 mantissae
		/// In fact, we only use a single common exponent that we factor out to 3 different mantissae.
		/// This format was first created by Gregory Ward for his Radiance software (http://www.graphics.cornell.edu/~bjw/rgbe.html)
		///  and allows to store HDR values using standard 8-bits formats.
		/// It's also quite useful to pack some data as we divide the size by 3, from 3 floats (12 bytes) down to only 4 bytes.
		/// </summary>
		/// <remarks>This format only allows storage of POSITIVE floats !</remarks>
		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		public struct PF_RGBE
		{
			public byte B, G, R, E;

			#region IPixelFormat Members

			public bool sRGB { get { return false; } }

// 			// NOTE: Alpha is ignored, RGB is encoded in RGBE
// 			public void Write( Vector4 _Color )
// 			{
// 				float	fMaxComponent = Math.Max( _Color.x, Math.Max( _Color.y, _Color.z ) );
// 				if ( fMaxComponent < 1e-16f )
// 				{	// Too low to encode...
// 					R = G = B = E = 0;
// 					return;
// 				}
// 
// 				double	CompleteExponent = Math.Log( fMaxComponent ) / Math.Log( 2.0 );
// 				int		Exponent = (int) Math.Ceiling( CompleteExponent );
// 				double	Mantissa = fMaxComponent / Math.Pow( 2.0f, Exponent );
// 				if ( Mantissa == 1.0 )
// 				{	// Step to next order
// 					Mantissa = 0.5;
// 					Exponent++;
// 				}
// 
// 				double	Debug0 = Mantissa * Math.Pow( 2.0, Exponent );
// 
// 				fMaxComponent = (float) Mantissa * 255.99999999f / fMaxComponent;
// 
// 				R = (byte) (_Color.x * fMaxComponent);
// 				G = (byte) (_Color.y * fMaxComponent);
// 				B = (byte) (_Color.z * fMaxComponent);
// 				E = (byte) (Exponent + 128 );
// 			}

			#endregion

			public Vector4D DecodedColorAsVector
			{
				get
				{
					double Exponent = Math.Pow( 2.0, E - (128 + 8) );
					return new Vector4D( (float) ((R + .5) * Exponent),
										(float) ((G + .5) * Exponent),
										(float) ((B + .5) * Exponent),
										1.0f
										);
				}
			}
		}

		#endregion

		private RegistryKey			m_AppKey;

		public FormCubeMapBaker()
		{
			InitializeComponent();

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\HDRCubeMapConvolver" );

			// Restore cube map size
			string	CubeMapSizeAsString = m_AppKey.GetValue( "CubeMapSize", "256" ) as string;
			int		CubeMapSize;
			int.TryParse( CubeMapSizeAsString, out CubeMapSize );
			integerTrackbarControlCubeMapSize.Value = CubeMapSize;

			// Restore probe type
			string	ProbeTypeAsString = m_AppKey.GetValue( "ProbeType", 0 ) as string;
			int		ProbeType;
			int.TryParse( ProbeTypeAsString, out ProbeType );
			switch ( ProbeType )
			{
				case 0: radioButtonCross.Checked = true; break;
				case 1: radioButtonProbe.Checked = true; break;
				case 2: radioButtonCylindrical.Checked = true; break;
			}



// 			FileInfo	Pipo = new FileInfo( @"E:\Program Files\Marmoset Toolbag 2\data\sky\Grace Cathedral.tbsky" );
// 			using ( FileStream S = Pipo.OpenRead() )
// 				using ( BinaryReader R = new BinaryReader( S ) )
// 				{
// 					S.Position = 0x50;
// 
// 					string	Bloute = "";
// 					for ( int i=0; i < 4*10; i++ )
// 					{
// 						float	Pouet = R.ReadSingle();
// 						Bloute += ", " + Pouet;
// 					}
// 
// 					Bloute += "";
// 				}
		}

		private void button1_Click( object sender, EventArgs e )
		{
			string	OldFileName = m_AppKey.GetValue( "LastProbeFilename", Path.GetDirectoryName( Application.ExecutablePath ) ) as string;
			openFileDialog.InitialDirectory = Path.GetDirectoryName( OldFileName );
			openFileDialog.FileName = Path.GetFileName( OldFileName );
			if ( openFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;
			m_AppKey.SetValue( "LastProbeFilename", openFileDialog.FileName );

			FileInfo	SourceFile = new FileInfo( openFileDialog.FileName );
			FileInfo	TargetFile = new FileInfo( Path.Combine( Path.GetDirectoryName( SourceFile.FullName ), Path.GetFileNameWithoutExtension( SourceFile.FullName ) + ".dds" ) );
			try
			{
				BakeCubeMap( SourceFile, TargetFile );
				MessageBox.Show( this, "Success!", "yay!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			}
			catch ( Exception _e )
			{
				MessageBox.Show( this, "An error occurred while opening HDR cube map \"" + SourceFile.FullName + "\":\n\n" + _e.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}

			progressBar1.Value = 0;
		}

		private void	BakeCubeMap( FileInfo _SourceFile, FileInfo _TargetFile )
		{
			byte[]		FileContent = null;
			using ( FileStream S = _SourceFile.OpenRead() )
			{
				FileContent = new byte[S.Length];
				S.Read( FileContent, 0, (int) S.Length );
			}

			// Read HDR cube map as a cross image
			Vector4D[,]	HDRValues = LoadAndDecodeHDRFormat( FileContent, false );

			int	CubeFaceSize = integerTrackbarControlCubeMapSize.Value;

			Vector4D[][,]	CubeFaces = new Vector4D[6][,] { null, null, null, null, null, null };
			if ( radioButtonCross.Checked )
				ExtractCross( HDRValues, CubeFaceSize, CubeFaces );
			else if ( radioButtonProbe.Checked )
				ExtractProbe( HDRValues, CubeFaceSize, CubeFaces );
			else if ( radioButtonCylindrical.Checked )
				ExtractCylindrical( HDRValues, CubeFaceSize, CubeFaces );
			else
				throw new Exception( "Unknown source type!" );

			// Convolve cube map
			Vector4D[][][,]	CubeFacesMips = ConvolveCubeMap( CubeFaceSize, CubeFaces );

			// Save as DDS
			DirectXTexManaged.CubeMapCreator.CreateCubeMapFile( _TargetFile.FullName, CubeFaceSize, CubeFacesMips );

			// Save SH convolution
			SaveSHConvolution( CubeFaceSize, CubeFacesMips, 3, _SourceFile, _TargetFile );
		}

		private void	SaveSHConvolution( int _CubeFaceSize, Vector4D[][][,] _CubeFaces, int _MipLevel, FileInfo _SourceFile, FileInfo _TargetFile )
		{
			m_CubeSize = _CubeFaceSize >> _MipLevel;
			m_CubeFaces = _CubeFaces[_MipLevel];

			double[]	SHCoeffs = new double[9];
			double[,]	SumSH = new double[9,3];

			Vector	View = new Vector();

			// Individual cube face sampling
			double	SumSolidAngle = 0.0;
			for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
			{
				Matrix3x3	FaceTransform = new Matrix3x3();
				switch ( CubeFaceIndex )
				{
					case 0:	// +X
						FaceTransform.SetRow0( -Vector.UnitZ );
						FaceTransform.SetRow1(  Vector.UnitY );
						FaceTransform.SetRow2(  Vector.UnitX );
						break;
					case 1:	// -X
						FaceTransform.SetRow0(  Vector.UnitZ );
						FaceTransform.SetRow1(  Vector.UnitY );
						FaceTransform.SetRow2( -Vector.UnitX );
						break;
					case 2:	// +Y
						FaceTransform.SetRow0(  Vector.UnitX );
						FaceTransform.SetRow1( -Vector.UnitZ );
						FaceTransform.SetRow2(  Vector.UnitY );
						break;
					case 3:	// -Y
						FaceTransform.SetRow0(  Vector.UnitX );
						FaceTransform.SetRow1(  Vector.UnitZ );
						FaceTransform.SetRow2( -Vector.UnitY );
						break;
					case 4:	// +Z
						FaceTransform.SetRow0(  Vector.UnitX );
						FaceTransform.SetRow1(  Vector.UnitY );
						FaceTransform.SetRow2(  Vector.UnitZ );
						break;
					case 5:	// -Z
						FaceTransform.SetRow0( -Vector.UnitX );
						FaceTransform.SetRow1(  Vector.UnitY );
						FaceTransform.SetRow2( -Vector.UnitZ );
						break;
				}

				for ( int Y=0; Y < m_CubeSize; Y++ )
				{
					for ( int X=0; X < m_CubeSize; X++ )
					{
						// Build camera space view direction
						View.x = 2.0f * (0.5f+X) / m_CubeSize - 1.0f;
						View.y = 1.0f - 2.0f * (0.5f+Y) / m_CubeSize;
						View.z = 1.0f;

						float	SqLength = View.LengthSquare;
						float	Length = (float) Math.Sqrt( SqLength );
						float	SolidAngle = 4.0f / (Length * SqLength);
								SolidAngle /= m_CubeSize*m_CubeSize;

						SumSolidAngle += SolidAngle;

						View /= Length;

						// Build world space view direction
						View *= FaceTransform;

						double	Theta = Math.Acos( View.y );
						double	Phi = Math.Atan2( View.x, View.z );

						//
						Vector4D	Color = m_CubeFaces[CubeFaceIndex][X,Y];
						float		Scale = (float) Math.Exp( 3.9120230054281460586187507879106 * Color.w );		// HDR unpacking

						Scale *= SolidAngle;

						for ( int l=0; l < 3; l++ )
						{
//							double	Filter = 1.0;																	// No filter
//							double	Filter = (Math.Cos(Math.PI * l / 3) + 1.0) * 0.5;								// Hanning filter
//							double	Filter = l > 0 ? (float) (Math.Sin(Math.PI * l / 3) / (Math.PI * l / 3)) : 1.0;	// Lancsoz filter (sinc)
							double	Filter = (float) Math.Exp( -(Math.PI * l / 3) * (Math.PI * l / 3) / 2.0 );		// Gaussian filter

							Filter *= Scale;

							for ( int m=-l; m <= l; m++)
							{
								double	Coeff = Filter * SphericalHarmonics.SHFunctions.ComputeSH( l, m, Theta, Phi );

								SumSH[l*(l+1)+m,0] += Coeff * Color.x;
								SumSH[l*(l+1)+m,1] += Coeff * Color.y;
								SumSH[l*(l+1)+m,2] += Coeff * Color.z;
							}
						}
					}
				}
			}

// 			// Equal weighted sampling on the sphere
// 			int	THETA_SAMPLES_COUNT = m_CubeSize;
// 			int	PHI_SAMPLES_COUNT = 2 * THETA_SAMPLES_COUNT;
//			double	Normalizer = 1.0 / (PHI_SAMPLES_COUNT * THETA_SAMPLES_COUNT);
// 			for ( int ThetaIndex=0; ThetaIndex < THETA_SAMPLES_COUNT; ThetaIndex++ )
// 			{
// 				double	Theta = 2.0 * Math.Asin( Math.Sqrt( (0.5 + ThetaIndex) / THETA_SAMPLES_COUNT ) );
// 				for ( int PhiIndex=0; PhiIndex < PHI_SAMPLES_COUNT; PhiIndex++ )
// 				{
// 					double	Phi = PhiIndex * 2.0 * Math.PI / PHI_SAMPLES_COUNT;
// 
// 					Dir.x = (float) (Math.Sin( Phi ) * Math.Sin( Theta ));
// 					Dir.y = (float) Math.Cos( Theta );
// 					Dir.z = -(float) (Math.Cos( Phi ) * Math.Sin( Theta ));
// 
// 					Vector4D	Color = PointSampleCubeMap( Dir );
// 					float	Scale = (float) Math.Exp( 3.9120230054281460586187507879106 * Color.w );
// 
// 					Scale *= Normalizer;
// 
// 					for ( int l=0; l < 3; l++ )
// 					{
// //						double	Filter = 1.0;																	// No filter
// //						double	Filter = (Math.Cos(Math.PI * l / 3) + 1.0) * 0.5;								// Hanning filter
// //						double	Filter = l > 0 ? (float) (Math.Sin(Math.PI * l / 3) / (Math.PI * l / 3)) : 1.0;	// Lancsoz filter (sinc)
// 						double	Filter = (float) Math.Exp( -(Math.PI * l / 3) * (Math.PI * l / 3) / 2.0 );		// Gaussian filter
// 
// 						Filter *= Scale;
// 
// 						for ( int m=-l; m <= l; m++)
// 						{
// 							double	Coeff = Filter * SphericalHarmonics.SHFunctions.ComputeSH( l, m, Theta, Phi );
// 
// 							SumSH[l*(l+1)+m,0] += Coeff * Color.x;
// 							SumSH[l*(l+1)+m,1] += Coeff * Color.y;
// 							SumSH[l*(l+1)+m,2] += Coeff * Color.z;
// 						}
// 					}
// 				}
// 			}

			// Save to disk as text format directly pasteable to shader code
			string	Text = "	// SH Coefficients from probe \"" + Path.GetFileName( _SourceFile.FullName ) + "\"\n";
			Text += "	float3	SH[9] = {\n";
			for ( int i=0; i < 9; i++ )
				Text += "		float3( " + SumSH[i,0] + ", " + SumSH[i,1] + ", " + SumSH[i,2] + " ), \n";
			Text += "		};\n\n";

			FileInfo	SHTargetFile = new FileInfo( Path.Combine( Path.GetDirectoryName( _TargetFile.FullName ), Path.GetFileNameWithoutExtension( _TargetFile.FullName ) + "_SH.txt" ) );
			using( StreamWriter Writer = SHTargetFile.CreateText() )
				Writer.Write( Text );
		}

		/// <summary>
		/// This function is the heart of that tool
		/// 
		/// The goal is to compute mip levels for the cube map where each new mip will match the corresponding roughness of an exponential lobe
		///	 like those used in standard normal distribution models (Ward, Beckmann, etc.) so that we can use a specific mip according to the
		///	 roughness parameter of the model.
		/// 
		/// Ignoring normalization factors, the typical reflection lobe is given by the following equation:
		///		f(theta) = exp( -tan(theta)² / m² )	     => m is the roughness in [0,1]
		///	
		/// Fooplot link for different plots with different roughnesses (reference cosine lobe in red):
		/// http://fooplot.com/#W3sidHlwZSI6MSwiZXEiOiJleHAoLSh0YW4oYWJzKHRoZXRhLXBpLzIpKS8wLjAyKV4yKSIsImNvbG9yIjoiIzAwODBDQyIsInRoZXRhbWluIjoiMCIsInRoZXRhbWF4IjoicGkiLCJ0aGV0YXN0ZXAiOiIuMDEifSx7InR5cGUiOjAsImVxIjoieC90YW4oMC4wMTI3KSIsImNvbG9yIjoiIzAwMDAwMCJ9LHsidHlwZSI6MSwiZXEiOiJleHAoLSh0YW4oYWJzKHRoZXRhLXBpLzIpKS8wLjEpXjIpIiwiY29sb3IiOiIjRkFCMzE5IiwidGhldGFtaW4iOiIwIiwidGhldGFtYXgiOiJwaSIsInRoZXRhc3RlcCI6Ii4wMSJ9LHsidHlwZSI6MCwiZXEiOiJ4L3RhbigwLjEyNykiLCJjb2xvciI6IiMwMDAwMDAifSx7InR5cGUiOjEsImVxIjoiZXhwKC0odGFuKGFicyh0aGV0YS1waS8yKSkvMC4yKV4yKSIsImNvbG9yIjoiIzAwQ0M0MSIsInRoZXRhbWluIjoiMCIsInRoZXRhbWF4IjoiMnBpIiwidGhldGFzdGVwIjoiLjAxIn0seyJ0eXBlIjowLCJlcSI6IngvdGFuKDAuMjQ3KSIsImNvbG9yIjoiIzAwMDAwMCJ9LHsidHlwZSI6MSwiZXEiOiJleHAoLSh0YW4oYWJzKHRoZXRhLXBpLzIpKS8wLjUpXjIpIiwiY29sb3IiOiIjMDA4MGNjIiwidGhldGFtaW4iOiIwIiwidGhldGFtYXgiOiIycGkiLCJ0aGV0YXN0ZXAiOiIuMDEifSx7InR5cGUiOjAsImVxIjoieC90YW4oMC41NDMpIiwiY29sb3IiOiIjMDAwMDAwIn0seyJ0eXBlIjoxLCJlcSI6ImV4cCgtKHRhbihhYnModGhldGEtcGkvMikpLzEuMCleMikiLCJjb2xvciI6IiMwMDgwY2MiLCJ0aGV0YW1pbiI6IjAiLCJ0aGV0YW1heCI6IjJwaSIsInRoZXRhc3RlcCI6Ii4wMSJ9LHsidHlwZSI6MCwiZXEiOiJ4L3RhbigwLjgzNCkiLCJjb2xvciI6IiMwMDAwMDAifSx7InR5cGUiOjEsImVxIjoiY29zKCh0aGV0YS1waS8yKSkiLCJjb2xvciI6IiNGRjAwNjYiLCJ0aGV0YW1pbiI6IjAiLCJ0aGV0YW1heCI6InBpIiwidGhldGFzdGVwIjoiLjAxIn0seyJ0eXBlIjoxMDAwLCJ3aW5kb3ciOlsiLTAuNzQ5OTk5OTk5OTk5OTk5OCIsIjAuNzQ5OTk5OTk5OTk5OTk5OCIsIjEuMTEwMjIzMDI0NjI1MTU2NWUtMTYiLCIxLjEiXX1d
		/// 
		/// The black lines are the "lobe tangents" found from roughness using equation (2) computed for an epsilon of 0.2
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
		///	   Y ^
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
		///		 |/_____________> X
		/// 
		/// 
		/// According to the problem we posed, we give an aperture angle of alpha and want to find the "tangent lobe".
		/// There is no exact way to do that since the tangent at theta=PI/2 is always 0, but we can fix conditions for the p coordinate.
		/// For example, at a given distance "epsilon" from the y=0 line, we want p to match a point p_epsilon on our tangent line:
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
		///		|	m = tan(alpha) / sqrt( -ln( eps / cos(alpha) ) )  |  (1)  gives us roughness from aperture half angle
		///		.-----------------------------------------------------.
		/// 
		/// 
		/// Fooplot link for different epsilons:
		///		http://www.fooplot.com/#W3sidHlwZSI6MCwiZXEiOiJ0YW4oeCkvc3FydCgtbG4oMC4wMDEvY29zKHgpKSkiLCJjb2xvciI6IiNGRjAwMDAifSx7InR5cGUiOjAsImVxIjoidGFuKHgpL3NxcnQoLWxuKDAuMDEvY29zKHgpKSkiLCJjb2xvciI6IiMwQkQxMzkifSx7InR5cGUiOjAsImVxIjoidGFuKHgpL3NxcnQoLWxuKDAuMDUvY29zKHgpKSkiLCJjb2xvciI6IiNGQ0NBMDAifSx7InR5cGUiOjAsImVxIjoidGFuKHgpL3NxcnQoLWxuKDAuMS9jb3MoeCkpKSIsImNvbG9yIjoiIzAwMDlGRiJ9LHsidHlwZSI6MTAwMCwid2luZG93IjpbIjAiLCIxLjU3IiwiMCIsIjgiXX1d
		/// 
		/// We can see from the graph that the lower the epsilon, the lower the roughness and the tighter the lobe.
		/// We choose a sufficiently large epsilon = 0.2 so the aperture angle encompasses roughly 90% of the lobe so our convolution algorithm will get most of the lobe's values of interest.
		/// 
		/// 
		/// ---------------------------------------------------------------
		/// Now, for the second part of the problem: in the shader, we're given the roughness m and we're required to find the proper
		///	 aperture half angle alpha so we can deduce the mip level to fetch from the cube map.
		/// 
		/// Unfortunately, I can't find an analytical solution to retrieve alpha from roughness.
		/// What we can do though is to fix an epsilon of say eps = 0.2 and find the alpha manually for different roughnesses,
		///	 store them in an array and try to fit a function through those points...
		/// 
		/// So, for epsilon = 0.2 and measuring from the following graph showing roughness as Y:
		///		http://www.fooplot.com/#W3sidHlwZSI6MCwiZXEiOiJ0YW4oeCkvc3FydCgtbG4oMC4yL2Nvcyh4KSkpIiwiY29sb3IiOiIjMDAwOUZGIn0seyJ0eXBlIjoxMDAwLCJ3aW5kb3ciOlsiLTAuMDQwNDQzOTk2MTg4MjIxMDYiLCIwLjg3NDMzNTYxODY3NTM0NjQiLCItMC4yNTE5NzAzMjkyODQ2Njc1NiIsIjEuMDkwNzA3NzQwNzgzNjkxNCJdLCJzaXplIjpbMTEwMCw3MDBdfV0-
		/// We get:
		/// 
		///		m		0.0100	0.10	0.20	0.30	0.40	0.50	0.60	0.70	0.80	0.90	1.00
		///		alpha	0.0127	0.127	0.247	0.357	0.456	0.543	0.618	0.684	0.74	0.79	0.834 ~= 47.784680113910655611249661114964° for the most diffuse lobe
		/// 
		/// 
		/// Using my online least square fitter (at http://patapom.com/topics/Misc/leastsquares/index.html) we find the excellent matching result:
		/// 
		///		.---------------------------------------------------------------------------------------.
		///		|	alpha = -0.5040552688878546 * m^2 + 1.3331290497744692 * m + 0.0003474660443456835	|	(2)   gives us aperture half angle from roughness
		///		.---------------------------------------------------------------------------------------.
		/// 
		/// 
		/// Fooplot link for the point list and its the linear and quadratic fits:
		/// http://www.fooplot.com/#W3sidHlwZSI6MywiZXEiOltbIjAuMDEwMCIsIiAwLjAxMjciXSxbIjAuMTAiLCIgMC4xMjciXSxbIjAuMjAiLCIgMC4zMjQ3Il0sWyIwLjMwIiwiIDAuMzU3Il0sWyIwLjQwIiwiIDAuNDU2Il0sWyIwLjUwIiwiIDAuNTQzIl0sWyIwLjYwIiwiIDAuNjE4Il0sWyIwLjcwIiwiIDAuNjg0Il0sWyIwLjgwIiwiIDAuNzQiXSxbIjAuOTAiLCIgMC43OSJdLFsiMS4wMCIsIiAwLjgzNCJdXSwiY29sb3IiOiIjMDAwMDAwIn0seyJ0eXBlIjowLCJlcSI6Ii0wLjUwNDA1NTI2ODg4Nzg1NDYqeF4yKzEuMzMzMTI5MDQ5Nzc0NDY5Mip4KzAuMDAwMzQ3NDY2MDQ0MzQ1NjgzNSIsImNvbG9yIjoiIzAwMDAwMCJ9LHsidHlwZSI6MTAwMCwid2luZG93IjpbIjAiLCIxLjEiLCIwIiwiMS4yIl19XQ--
		/// 
		/// 
		/// 
		/// ---------------------------------------------------------------
		///		Convolution
		/// ---------------------------------------------------------------
		/// 
		/// Normally, with each new mip level, the tangent of the aperture angle is doubled.
		/// 
		/// At mip #0, tan( alpha ) = 1 / CubeSize so we cover a single pixel
		/// At mip #1, tan( alpha ) = 2 / CubeSize
		/// At mip #2, tan( alpha ) = 4 / CubeSize
		/// At mip #3, tan( alpha ) = 8 / CubeSize
		/// ...
		/// At mip #N, tan( alpha ) = CubeSize / CubeSize => alpha = PI/4, we cover Omega = 4PI/6
		/// 
		/// 
		/// In our case, we want to be able to cover up to a lobe with aperture half angle = 48°, corresponding to the maximum roughness of m=1
		/// If you look at an example of a completely diffuse cube map http://www.3dvia.com/studio/wp-content/uploads/2009/11/hangar_diffuse.png you can see
		///  it still needs a little resolution and we can't simply assume it will be the highest mip level of size 1x1...
		/// 
		/// By fixing the "diffuse mip" so it's kept in the cube map mip of size 4x4, for example, we can have N-2 mips where the angle will vary from 0 to 48°
		/// 
		/// Take the example of a 64x64 cube map (N=7):
		///		Mip #0 = 64x64	-> alpha = 0°	(m=0.01)	Remember from eq. (1) that m = tan(alpha) / sqrt( -ln( eps / cos(alpha) ) )
		///		Mip #1 = 32x32	-> alpha = 12°	(m=0.168)
		///		Mip #2 = 16x16	-> alpha = 24°	(m=0.361)
		///		Mip #3 =  8x8	-> alpha = 36°	(m=0.614)
		///		Mip #4 =  4x4	-> alpha = 48°	(m~=1.0)
		///	  -------------------------------------------
		///		Mip #5 =  2x2	-> alpha = 48°
		///		Mip #6 =  1x1	-> alpha = 48°
		/// 
		/// Using the above formula for finding alpha from roughness, we can now easily get the mip level index:
		/// 
		///		Mip = (-0.5040552688878546 * m^2 + 1.3331290497744692 * m + 0.0003474660443456835) * (N-3)		(3)
		/// 
		/// 
		/// In practice, increasing the aperture angle linearly doesn't give enough precision for low roughnesses so we prefer a quadratic increase instead, so eq. (3) becomes:
		/// 
		///		.---------------------------------------------------------------------------------------------------------------.
		///		|	Mip = sqrt( (-0.5040552688878546 * m^2 + 1.3331290497744692 * m + 0.0003474660443456835) / 0.834 ) * (N-3)	|	(4)
		///		.---------------------------------------------------------------------------------------------------------------.
		/// 
		/// 
		/// This is the conclusion of this long explanation: we can deduces alpha and roughness from each other and we have a nice simple way of computing the mip levels of the cube map.
		/// 
		/// </summary>
		/// <param name="_CubeFaces"></param>
		/// <returns></returns>
		/// 
		private const float	EPS = 0.2f;					// The epsilon at which we perform the computations
//		private const float	MAX_ALPHA = 1.18f;			// The maximum angle reached at the last mip corresponding to the maximum roughness of 1.5
		private const float	MAX_ALPHA = 0.834f;			// The maximum angle reached at the last mip corresponding to the maximum roughness of 1.0

		private const float	SAMPLES_FACTOR = 1.0f;		// A global factor to avoid using too many samples for the convolution

		private const float	MIN_SAMPLES_COUNT = 512;	// Minimum amount of samples for smooth lobes
		private const float	MAX_SAMPLES_COUNT = 4096;	// Maximum amount of samples for rough lobes


		private int				m_CubeSize;
		private Vector4D[][,]	m_CubeFaces;
		private Vector4D[][][,]	ConvolveCubeMap( int _CubeSize, Vector4D[][,] _CubeFaces )
		{
			m_CubeSize = _CubeSize;
			m_CubeFaces = _CubeFaces;

			// Compute necessary mip levels
			int	MipLevels = 1 + (int) Math.Floor( Math.Log( m_CubeSize ) / Math.Log( 2 ) );	// This would be the total amount of mips for the entire chain
				MipLevels -= 2;																// But as stated above, we limit ourselves down to the lowest mip of 4x4 pixels per face

			// Compute the total amount of pixels from one side to the other side of the cube if we split the hemicube with a plane.
			// This roughly corresponds to the amount of pixels we would span if we had an aperture angle of PI
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
				// Progress feedback
				this.progressBar1.Value = progressBar1.Maximum * MipIndex / Math.Max( 1, MipLevels-1 );
				this.Refresh();

				int				MipCubeSize = m_CubeSize >> MipIndex;
				Vector4D[][,]	MipCubeFaces = new Vector4D[6][,];
				Result[MipIndex] = MipCubeFaces;

				// Compute expected lobe angle
				float	MipNorm = ((float) MipIndex) / (MipLevels-1);
						MipNorm *= MipNorm;			// <=== IMPORTANT! Note here that we encode lobe aperture half angle progression quadratically to have more precision near the low angles
				float	Alpha = MAX_ALPHA * MipNorm;

				// Compute equivalent roughness
				float	m = (float) (Math.Tan(Alpha) / Math.Sqrt( -Math.Log( EPS / Math.Cos(Alpha) ) ));

 				// Build samples

/*
// NEW ROUTINE THAT USES IMPORTANCE SAMPLING
//				int		SamplesCount = (int) Math.Ceiling( MIN_SAMPLES_COUNT + MipNorm * (MAX_SAMPLES_COUNT - MIN_SAMPLES_COUNT) );
				int		SamplesCount = (int) Math.Exp( (1.0-MipNorm) * Math.Log( MIN_SAMPLES_COUNT ) + MipNorm * Math.Log( MAX_SAMPLES_COUNT ) );
//				int		SamplesCount = (int) Math.Ceiling( MIN_SAMPLES_COUNT + Math.Pow( 2.0, MipIndex ) * (MAX_SAMPLES_COUNT - MIN_SAMPLES_COUNT) * Math.Pow( 2.0, -2.0 * (MipLevels-1) ) );

 				Vector4D[]	Samples = new Vector4D[SamplesCount];

				Random		RNG = new Random( 1 );
				for ( int SampleIndex=0; SampleIndex < SamplesCount; SampleIndex++ )
				{
					double	Phi = 2.0 * Math.PI * RNG.NextDouble();
//					double	Theta = Math.Sqrt( -(m*m) * Math.Log( 1.0 - RNG.NextDouble() ) );
					double	Theta = Math.Sqrt( -(m*m) * Math.Log( 1.0 - ((SampleIndex+RNG.NextDouble()) / SamplesCount) ) );	// Stratified version

					float	CosTheta = (float) Math.Cos( Theta );
					float	SinTheta = (float) Math.Sin( Theta );

					float	CosPhi = (float) Math.Cos( Phi );
					float	SinPhi = (float) Math.Sin( Phi );

// 					float	Reflectance = (float) Math.Exp( -Math.Pow( Math.Tan( Theta ) / m, 2.0 ) );	// Gaussian lobe reflectance in that direction, normalized against amount of samples taken
// 					SumReflectance += Reflectance;

					Samples[SampleIndex] = new Vector4D(
							SinTheta * CosPhi,
							SinTheta * SinPhi,
							CosTheta,
							1.0f / SamplesCount
						);
				}
//*/

//*
// OLD ROUTINE THAT SAMPLED MANY TIMES WITH VARYING WEIGHT
				// Compute amount of samples along alpha & phi depending on original cube size
				int		SamplesCountTheta = (int) Math.Ceiling( SAMPLES_FACTOR * TotalPixels * Alpha / Math.PI );	// A simple ratio based on the total pixels if we had a PI/2 aperture...

//SamplesCountTheta = 3 * MipIndex;

				float	dTheta = Alpha / SamplesCountTheta;
//				int		SamplesCountPhi = (int) Math.Floor( 2.0 * Math.PI / dTheta );	// Approximately the same spacing in Phi

//				Vector4D[]	Samples = new Vector4D[SamplesCountPhi * SamplesCountTheta];
				List<Vector4D>	SamplesList = new List<Vector4D>();
//				float		Normalizer = 1.0f / (SamplesCountPhi * SamplesCountTheta);	// First, normalize by amount of samples
//							Normalizer *= 1.0f / (float) (4.0 * Math.PI * m * m);		// Next, normalize for Ward (now done in the shader!)

				Random		RNG = new Random( 1 );
				float		SumReflectance = 0.0f;
				for ( int ThetaIndex=0; ThetaIndex < SamplesCountTheta; ThetaIndex++ )
				{
					// Here, the amount of samples along phi depends on the radius of the circle at average theta
					double	Radius = Math.Sin( (0.5 + ThetaIndex) * dTheta );
					int		SamplesCountPhi = (int) Math.Ceiling( 2.0f * Math.PI * Radius / dTheta );
					float	dPhi = 2.0f * (float) Math.PI / SamplesCountPhi;

					for ( int PhiIndex=0; PhiIndex < SamplesCountPhi; PhiIndex++ )
					{
						float	Theta = (float) Math.Sqrt( -m*m * Math.Log( (ThetaIndex + 1.0 - RNG.NextDouble()) / SamplesCountTheta ) );	// According to ward's monte-carlo sampling method (stratified version)
						float	Phi = (float) (PhiIndex + RNG.NextDouble()) * dPhi;

						float	CosTheta = (float) Math.Cos( Theta );
						float	SinTheta = (float) Math.Sin( Theta );

						float	CosPhi = (float) Math.Cos( Phi );
						float	SinPhi = (float) Math.Sin( Phi );

						float	Reflectance = (float) Math.Exp( -Math.Pow( Math.Tan( Theta ) / m, 2.0 ) );	// Gaussian lobe reflectance in that direction, normalized against amount of samples taken

								Reflectance *= (float) Math.Cos( Theta );

						SumReflectance += Reflectance;
//						Reflectance *= Normalizer;

						SamplesList.Add( new Vector4D(
								SinTheta * CosPhi,
								SinTheta * SinPhi,
								CosTheta,
								Reflectance
							) );
					}
				}

				// Normalize samples' reflectance
				Vector4D[]	Samples = SamplesList.ToArray();
				int			SamplesCount = Samples.Length;
//				float	Normalizer = 1.0f / SumReflectance;
				float	Normalizer = 1.0f / Samples.Length;
				for ( int SampleIndex=0; SampleIndex < SamplesCount; SampleIndex++ )
				{
					Samples[SampleIndex].w *= Normalizer;
				}
//*/

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
						V = 2.0f * (0.5f+y) / MipCubeSize - 1.0f;
						for ( int x=0; x < MipCubeSize; x++ )
						{
							U = 2.0f * (0.5f+x) / MipCubeSize - 1.0f;

							Direction = Z + U * X + V * Y;
							Direction.Normalize();

// Simple direction test...
//CubeFace[x,y] = EncodeHDR( 10.0f * new Vector4D( Direction.x, Direction.y, Direction.z, 1 ) );
// CubeFace[x,y] = EncodeHDR( PointSampleCubeMap( Direction ) );
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

							// Maya seems to cut HDR values!!
							CubeFace[x,y] = EncodeHDR( Accum );	// Here's our final result!
						}
					}
				}
			}

			// Encode first mip into HDR
			for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ )
			{
				Vector4D[,]	CubeFace = _CubeFaces[FaceIndex];
				for ( int y=0; y < _CubeSize; y++ )
					for ( int x=0; x < _CubeSize; x++ )
						CubeFace[x,y] = EncodeHDR( CubeFace[x,y] );
			}

			return Result;
		}

		// Maya seems to cut HDR values!!
		// Let's save exponent in alpha
		private Vector4D	EncodeHDR( Vector4D _value )
		{
			float	Max = Math.Max( Math.Max( _value.x, _value.y ), _value.z );
			if ( Max < 1.0f )
			{	// LDR values stay the same...
				_value.w = 0.0f;
				return _value;
			}

			float	LogVal = (float) Math.Log( Max ) / 3.9120230054281460586187507879106f;	// Assume a max of 50
			_value /= Max;
			_value.w = LogVal;
			return _value;
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

			return Face[Ui,Vi];
		}

		#region HDR Loaders

		/// <summary>
		/// Loads a bitmap in .HDR format into a Vector4 array directly useable by the image constructor
		/// </summary>
		/// <param name="_HDRFormatBinary"></param>
		/// <param name="_bTargetNeedsXYZ">Tells if the target needs to be in CIE XYZ space (true) or RGB (false)</param>
		/// <param name="_ColorProfile">The color profile for the image</param>
		/// <returns></returns>
		public Vector4D[,] LoadAndDecodeHDRFormat( byte[] _HDRFormatBinary, bool _bTargetNeedsXYZ )
		{
			bool bSourceIsXYZ;
			return DecodeRGBEImage( LoadHDRFormat( _HDRFormatBinary, out bSourceIsXYZ ), bSourceIsXYZ, _bTargetNeedsXYZ );
		}

		/// <summary>
		/// Loads a bitmap in .HDR format into a RGBE array
		/// </summary>
		/// <param name="_HDRFormatBinary"></param>
		/// <param name="_bIsXYZ">Tells if the image is encoded as XYZE rather than RGBE</param>
		/// <param name="_ColorProfile">The color profile for the image</param>
		/// <returns></returns>
		public unsafe PF_RGBE[,]	LoadHDRFormat( byte[] _HDRFormatBinary, out bool _bIsXYZ )
		{
			try
			{
				// The header of a .HDR image file consists of lines terminated by '\n'
				// It ends when there are 2 successive '\n' characters, then follows a single line containing the resolution of the image and only then, real scanlines begin...
				//

				// 1] We must isolate the header and find where it ends.
				//		To do this, we seek and replace every '\n' characters by '\0' (easier to read) until we find a double '\n'
				List<string> HeaderLines = new List<string>();
				int CharacterIndex = 0;
				int LineStartCharacterIndex = 0;

				while ( true )
				{
					if ( _HDRFormatBinary[CharacterIndex] == '\n' || _HDRFormatBinary[CharacterIndex] == '\0' )
					{	// Found a new line!
						_HDRFormatBinary[CharacterIndex] = 0;
						fixed ( byte* pLineStart = &_HDRFormatBinary[LineStartCharacterIndex] )
							HeaderLines.Add( new string( (sbyte*) pLineStart, 0, CharacterIndex - LineStartCharacterIndex, System.Text.Encoding.ASCII ) );

						LineStartCharacterIndex = CharacterIndex + 1;

						// Check for header end
						if ( _HDRFormatBinary[CharacterIndex + 2] == '\n' )
						{
							CharacterIndex += 3;
							break;
						}
						if ( _HDRFormatBinary[CharacterIndex + 1] == '\n' )
						{
							CharacterIndex += 2;
							break;
						}
					}

					// Next character
					CharacterIndex++;
				}

				// 2] Read the last line containing the resolution of the image
				byte* pScanlines = null;
				string Resolution = null;
				LineStartCharacterIndex = CharacterIndex;
				while ( true )
				{
					if ( _HDRFormatBinary[CharacterIndex] == '\n' || _HDRFormatBinary[CharacterIndex] == '\0' )
					{
						_HDRFormatBinary[CharacterIndex] = 0;
						fixed ( byte* pLineStart = &_HDRFormatBinary[LineStartCharacterIndex] )
							Resolution = new string( (sbyte*) pLineStart, 0, CharacterIndex - LineStartCharacterIndex, System.Text.Encoding.ASCII );

						fixed ( byte* pScanlinesStart = &_HDRFormatBinary[CharacterIndex + 1] )
							pScanlines = pScanlinesStart;

						break;
					}

					// Next character
					CharacterIndex++;
				}

				// 3] Check format and retrieve resolution
				// 3.1] Search lines for "#?RADIANCE" or "#?RGBE"
				if ( RadianceFileFindInHeader( HeaderLines, "#?RADIANCE" ) == null && RadianceFileFindInHeader( HeaderLines, "#?RGBE" ) == null )
					throw new NotSupportedException( "Unknown HDR format!" );		// Unknown HDR file format!

				// 3.2] Search lines for format
				string FileFormat = RadianceFileFindInHeader( HeaderLines, "FORMAT=" );
				if ( FileFormat == null )
					throw new Exception( "No format description!" );			// Couldn't get FORMAT

				_bIsXYZ = false;
				if ( FileFormat.IndexOf( "32-bit_rle_rgbe" ) == -1 )
				{	// Check for XYZ encoding
					_bIsXYZ = true;
					if ( FileFormat.IndexOf( "32-bit_rle_xyze" ) == -1 )
						throw new Exception( "Can't read format \"" + FileFormat + "\". Only 32-bit-rle-rgbe or 32-bit_rle_xyze is currently supported!" );
				}

				// 3.3] Search lines for the exposure
				float fExposure = 0.0f;
				string ExposureText = RadianceFileFindInHeader( HeaderLines, "EXPOSURE=" );
				if ( ExposureText != null )
					float.TryParse( ExposureText, out fExposure );

				// 3.4] Read the color primaries
				// 				ColorProfile.Chromaticities	Chromas = ColorProfile.Chromaticities.Radiance;	// Default chromaticities
				// 				string	PrimariesText = RadianceFileFindInHeader( HeaderLines, "PRIMARIES=" );
				// 				if ( PrimariesText != null )
				// 				{
				// 					string[]	Primaries = PrimariesText.Split( ' ' );
				// 					if ( Primaries == null || Primaries.Length != 8 )
				// 						throw new Exception( "Failed to parse color profile chromaticities !" );
				// 
				// 					float.TryParse( Primaries[0], out Chromas.R.X );
				// 					float.TryParse( Primaries[1], out Chromas.R.Y );
				// 					float.TryParse( Primaries[2], out Chromas.G.X );
				// 					float.TryParse( Primaries[3], out Chromas.G.Y );
				// 					float.TryParse( Primaries[4], out Chromas.B.X );
				// 					float.TryParse( Primaries[5], out Chromas.B.Y );
				// 					float.TryParse( Primaries[6], out Chromas.W.X );
				// 					float.TryParse( Primaries[7], out Chromas.W.Y );
				// 				}
				// 
				// 					// 3.5] Create the color profile
				// 				_ColorProfile = new ColorProfile( Chromas, ColorProfile.GAMMA_CURVE.STANDARD, 1.0f );
				// 				_ColorProfile.Exposure = fExposure;

				// 3.6] Read the resolution out of the last line
				int WayX = +1, WayY = +1;
				int Width = 0, Height = 0;

				int XIndex = Resolution.IndexOf( "+X" );
				if ( XIndex == -1 )
				{	// Wrong way!
					WayX = -1;
					XIndex = Resolution.IndexOf( "-X" );
				}
				if ( XIndex == -1 )
					throw new Exception( "Couldn't find image width in resolution string \"" + Resolution + "\"!" );
				int WidthEndCharacterIndex = Resolution.IndexOf( ' ', XIndex + 3 );
				if ( WidthEndCharacterIndex == -1 )
					WidthEndCharacterIndex = Resolution.Length;
				Width = int.Parse( Resolution.Substring( XIndex + 2, WidthEndCharacterIndex - XIndex - 2 ) );

				int YIndex = Resolution.IndexOf( "+Y" );
				if ( YIndex == -1 )
				{	// Flipped !
					WayY = -1;
					YIndex = Resolution.IndexOf( "-Y" );
				}
				if ( YIndex == -1 )
					throw new Exception( "Couldn't find image height in resolution string \"" + Resolution + "\"!" );
				int HeightEndCharacterIndex = Resolution.IndexOf( ' ', YIndex + 3 );
				if ( HeightEndCharacterIndex == -1 )
					HeightEndCharacterIndex = Resolution.Length;
				Height = int.Parse( Resolution.Substring( YIndex + 2, HeightEndCharacterIndex - YIndex - 2 ) );

				// The encoding of the image data is quite simple:
				//
				//	_ Each floating-point component is first encoded in Greg Ward's packed-pixel format which encodes 3 floats into a single DWORD organized this way: RrrrrrrrGgggggggBbbbbbbbEeeeeeee (E being the common exponent)
				//	_ Each component of the packed-pixel is then encoded separately using a simple run-length encoding format
				//

				// 1] Allocate memory for the image and the temporary p_HDRFormatBinaryScanline
				PF_RGBE[,] Dest = new PF_RGBE[Width, Height];
				byte[,] TempScanline = new byte[Width, 4];

				// 2] Read the scanlines
				int ImageY = WayY == +1 ? 0 : Height - 1;
				for ( int y = 0; y < Height; y++, ImageY += WayY )
				{
					if ( Width < 8 || Width > 0x7FFF || pScanlines[0] != 0x02 )
						throw new Exception( "Unsupported old encoding format!" );

					byte Temp;
					byte Green, Blue;

					// 2.1] Read an entire scanline
					pScanlines++;
					Green = *pScanlines++;
					Blue = *pScanlines++;
					Temp = *pScanlines++;

					if ( Green != 2 || (Blue & 0x80) != 0 )
						throw new Exception( "Unsupported old encoding format!" );

					if ( ((Blue << 8) | Temp) != Width )
						throw new Exception( "Line and image widths mismatch!" );

					for ( int ComponentIndex = 0; ComponentIndex < 4; ComponentIndex++ )
					{
						for ( int x = 0; x < Width; )
						{
							byte Code = *pScanlines++;
							if ( Code > 128 )
							{	// Run-Length encoding
								Code &= 0x7F;
								byte RLValue = *pScanlines++;
								while ( Code-- > 0 && x < Width )
									TempScanline[x++, ComponentIndex] = RLValue;
							}
							else
							{	// Normal encoding
								while ( Code-- > 0 && x < Width )
									TempScanline[x++, ComponentIndex] = *pScanlines++;
							}
						}	// For every pixels of the scanline
					}	// For every color components (including exponent)

					// 2.2] Post-process the scanline and re-order it correctly
					int ImageX = WayX == +1 ? 0 : Width - 1;
					for ( int x = 0; x < Width; x++, ImageX += WayX )
					{
						Dest[x, y].R = TempScanline[ImageX, 0];
						Dest[x, y].G = TempScanline[ImageX, 1];
						Dest[x, y].B = TempScanline[ImageX, 2];
						Dest[x, y].E = TempScanline[ImageX, 3];
					}
				}

				return Dest;
			}
			catch ( Exception _e )
			{	// Ouch!
				throw new Exception( "An exception occured while attempting to load an HDR file!", _e );
			}
		}

		/// <summary>
		/// Decodes a RGBE formatted image into a plain floating-point image
		/// </summary>
		/// <param name="_Source">The source RGBE formatted image</param>
		/// <param name="_bSourceIsXYZ">Tells if the source image is encoded as XYZE rather than RGBE</param>
		/// <param name="_bTargetNeedsXYZ">Tells if the target needs to be in CIE XYZ space (true) or RGB (false)</param>
		/// <param name="_ColorProfile">The color profile for the image</param>
		/// <returns>A HDR image as floats</returns>
		public static Vector4D[,] DecodeRGBEImage( PF_RGBE[,] _Source, bool _bSourceIsXYZ, bool _bTargetNeedsXYZ )
		{
			if ( _Source == null )
				return null;

			Vector4D[,] Result = new Vector4D[_Source.GetLength( 0 ), _Source.GetLength( 1 )];
			DecodeRGBEImage( _Source, _bSourceIsXYZ, Result, _bTargetNeedsXYZ );

			return Result;
		}

		/// <summary>
		/// Decodes a RGBE formatted image into a plain floating-point image
		/// </summary>
		/// <param name="_Source">The source RGBE formatted image</param>
		/// <param name="_bSourceIsXYZ">Tells if the source image is encoded as XYZE rather than RGBE</param>
		/// <param name="_Target">The target Vector4 image</param>
		/// <param name="_bTargetNeedsXYZ">Tells if the target needs to be in CIE XYZ space (true) or RGB (false)</param>
		/// <param name="_ColorProfile">The color profile for the image</param>
		public static void DecodeRGBEImage( PF_RGBE[,] _Source, bool _bSourceIsXYZ, Vector4D[,] _Target, bool _bTargetNeedsXYZ )
		{
			for ( int Y = 0; Y < _Source.GetLength( 1 ); Y++ )
				for ( int X = 0; X < _Source.GetLength( 0 ); X++ )
					_Target[X, Y] = _Source[X, Y].DecodedColorAsVector;
		}

		protected static string RadianceFileFindInHeader( List<string> _HeaderLines, string _Search )
		{
			foreach ( string Line in _HeaderLines )
				if ( Line.IndexOf( _Search ) != -1 )
					return Line.Replace( _Search, "" );	// Return line and remove Search criterium

			return null;
		}

		#endregion

		#region Encoding Decryption

		#region Cross

		private void	ExtractCross( Vector4D[,] _HDRValues, int _CubeFaceSize, Vector4D[][,] _CubeFaces )
		{
			int	Width = _HDRValues.GetLength( 0 ), Height = _HDRValues.GetLength( 1 );

			// Extract cube faces
			Vector4D[][,]	TempCubeFaces = new Vector4D[6][,];
			int	SourceCubeFaceSize = 0;
			if ( Height > Width )
			{	// Vertical cross
				SourceCubeFaceSize = Height / 4;
				if ( Width != SourceCubeFaceSize * 3 )
					throw new Exception( "The cube map " + Width + "x" + Height + " was detected as a VERTICAL CROSS with a cube face size of " + SourceCubeFaceSize + " (Height/4) but the width is not of the expected size of 3x" + SourceCubeFaceSize + " (3xWidth)! Are you sure you're using a CROSS cube map?" );

				TempCubeFaces[0] = ReadCubeFace( _HDRValues, SourceCubeFaceSize, 2, 1 );	// +X
				TempCubeFaces[1] = ReadCubeFace( _HDRValues, SourceCubeFaceSize, 0, 1 );	// -X
				TempCubeFaces[2] = ReadCubeFace( _HDRValues, SourceCubeFaceSize, 1, 0 );	// +Y
				TempCubeFaces[3] = ReadCubeFace( _HDRValues, SourceCubeFaceSize, 1, 2 );	// -Y
				TempCubeFaces[4] = ReadCubeFace( _HDRValues, SourceCubeFaceSize, 1, 1 );	// +Z
				TempCubeFaces[5] = ReadCubeFace( _HDRValues, SourceCubeFaceSize, 1, 3, true );	// -Z
			}
			else
			{	// Horizontal cross
				SourceCubeFaceSize = Width / 4;
				if ( Height != SourceCubeFaceSize * 3 )
					throw new Exception( "The cube map " + Width + "x" + Height + " was detected as a HORIZONTAL CROSS with a cube face size of " + SourceCubeFaceSize + " (Width/4) but the height is not of the expected size of 3x" + SourceCubeFaceSize + " (3xHeight)! Are you sure you're using a CROSS cube map?" );

				TempCubeFaces[0] = ReadCubeFace( _HDRValues, SourceCubeFaceSize, 2, 1 );	// +X
				TempCubeFaces[1] = ReadCubeFace( _HDRValues, SourceCubeFaceSize, 0, 1 );	// -X
				TempCubeFaces[2] = ReadCubeFace( _HDRValues, SourceCubeFaceSize, 1, 0 );	// +Y
				TempCubeFaces[3] = ReadCubeFace( _HDRValues, SourceCubeFaceSize, 1, 2 );	// -Y
				TempCubeFaces[4] = ReadCubeFace( _HDRValues, SourceCubeFaceSize, 1, 1 );	// +Z
				TempCubeFaces[5] = ReadCubeFace( _HDRValues, SourceCubeFaceSize, 3, 1 );	// -Z
			}

			// Convert to final size
			m_CubeSize = SourceCubeFaceSize;
			m_CubeFaces = TempCubeFaces;
			BuildTargetCubeMapMip0( _CubeFaceSize, _CubeFaces, ( Vector _Direction ) => {
				Vector4D	Result = PointSampleCubeMap( _Direction );
				return Result;
			} );
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

		#endregion

		#region Probe

		private void	ExtractProbe( Vector4D[,] _HDRValues, int _CubeFaceSize, Vector4D[][,] _CubeFaces )
		{
			int	Width = _HDRValues.GetLength( 0 ), Height = _HDRValues.GetLength( 1 );

			BuildTargetCubeMapMip0( _CubeFaceSize, _CubeFaces, ( Vector _Direction ) => {

				// Stolen from http://www.pauldebevec.com/Probes/
				double	R = (1.0/ Math.PI) * Math.Acos( _Direction.z ) / Math.Sqrt( _Direction.x*_Direction.x + _Direction.y*_Direction.y );
				double	U = _Direction.x * R;
				double	V = _Direction.y * R;

				// Bilinear interpolate
				float	X = (float) (Width * (0.5 * (1.0 + U)));
				float	Y = (float) (Height * (0.5 * (1.0 - V)));
				int		X0 = (int) Math.Floor( X );
				int		Y0 = (int) Math.Floor( Y );
				float	x = X - X0;
				float	y = Y - Y0;
				X0 = Math.Min( Width-1, X0 );
				Y0 = Math.Min( Height-1, Y0 );
				int		X1 = Math.Min( Width-1, X0+1 );
				int		Y1 = Math.Min( Height-1, Y0+1 );

				Vector4D	V00 = _HDRValues[X0,Y0];
				Vector4D	V01 = _HDRValues[X1,Y0];
				Vector4D	V10 = _HDRValues[X0,Y1];
				Vector4D	V11 = _HDRValues[X1,Y1];

				Vector4D	V0 = V00 + x * (V01 - V00);
				Vector4D	V1 = V10 + x * (V11 - V10);

				Vector4D	Result = V0 + y * (V1 - V0);
				return Result;
			} );
		}

		#endregion

		#region Cylindrical

		private void	ExtractCylindrical( Vector4D[,] _HDRValues, int _CubeFaceSize, Vector4D[][,] _CubeFaces )
		{
			int	Width = _HDRValues.GetLength( 0 ), Height = _HDRValues.GetLength( 1 );

			BuildTargetCubeMapMip0( _CubeFaceSize, _CubeFaces, ( Vector _Direction ) => {

				// Stolen from http://www.pauldebevec.com/Probes/
				double	U = Math.Atan2( _Direction.x, _Direction.z ) / Math.PI;
				double	V = 1.0 - 2.0 * Math.Acos( _Direction.y ) / Math.PI;

				// Bilinear interpolate
				float	X = (float) (Width * (0.5 * (1.0 + U)));
				float	Y = (float) (Height * (0.5 * (1.0 - V)));
				int		X0 = (int) Math.Floor( X );
				int		Y0 = (int) Math.Floor( Y );
				float	x = X - X0;
				float	y = Y - Y0;
				X0 = Math.Min( Width-1, X0 );
				Y0 = Math.Min( Height-1, Y0 );
				int		X1 = Math.Min( Width-1, X0+1 );
				int		Y1 = Math.Min( Height-1, Y0+1 );

				Vector4D	V00 = _HDRValues[X0,Y0];
				Vector4D	V01 = _HDRValues[X1,Y0];
				Vector4D	V10 = _HDRValues[X0,Y1];
				Vector4D	V11 = _HDRValues[X1,Y1];

				Vector4D	V0 = V00 + x * (V01 - V00);
				Vector4D	V1 = V10 + x * (V11 - V10);

				Vector4D	Result = V0 + y * (V1 - V0);
				return Result;
			} );
		}

		#endregion

		private delegate Vector4D	CubeMapSamplerDelegate( Vector _Direction );

		// Cube map faces orientation: http://msdn.microsoft.com/en-us/library/windows/desktop/bb204881(v=vs.85).aspx
		private void	BuildTargetCubeMapMip0( int _CubeFaceSize, Vector4D[][,] _CubeFaces, CubeMapSamplerDelegate _Sampler )
		{
			Vector	View = Vector.Zero;
			for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
			{
				Vector4D[,]	Face = new Vector4D[_CubeFaceSize,_CubeFaceSize];
				_CubeFaces[CubeFaceIndex] = Face;

				Matrix3x3	FaceTransform = new Matrix3x3();
				switch ( CubeFaceIndex )
				{
					case 0:	// +X
						FaceTransform.SetRow0( -Vector.UnitZ );
						FaceTransform.SetRow1(  Vector.UnitY );
						FaceTransform.SetRow2(  Vector.UnitX );
						break;
					case 1:	// -X
						FaceTransform.SetRow0(  Vector.UnitZ );
						FaceTransform.SetRow1(  Vector.UnitY );
						FaceTransform.SetRow2( -Vector.UnitX );
						break;
					case 2:	// +Y
						FaceTransform.SetRow0(  Vector.UnitX );
						FaceTransform.SetRow1( -Vector.UnitZ );
						FaceTransform.SetRow2(  Vector.UnitY );
						break;
					case 3:	// -Y
						FaceTransform.SetRow0(  Vector.UnitX );
						FaceTransform.SetRow1(  Vector.UnitZ );
						FaceTransform.SetRow2( -Vector.UnitY );
						break;
					case 4:	// +Z
						FaceTransform.SetRow0(  Vector.UnitX );
						FaceTransform.SetRow1(  Vector.UnitY );
						FaceTransform.SetRow2(  Vector.UnitZ );
						break;
					case 5:	// -Z
						FaceTransform.SetRow0( -Vector.UnitX );
						FaceTransform.SetRow1(  Vector.UnitY );
						FaceTransform.SetRow2( -Vector.UnitZ );
						break;
				}

				for ( int Y=0; Y < _CubeFaceSize; Y++ )
				{
					for ( int X=0; X < _CubeFaceSize; X++ )
					{
						// Build camera space view direction
						View.x = 2.0f * (0.5f+X) / _CubeFaceSize - 1.0f;
						View.y = 1.0f - 2.0f * (0.5f+Y) / _CubeFaceSize;
						View.z = 1.0f;

						View.Normalize();

						// Build world space view direction
						View *= FaceTransform;

						Vector4D	V = _Sampler( View );
						Face[X,Y] = V;
					}
				}
			}
		}

		#endregion

		private void radioButtonCross_CheckedChanged( object sender, EventArgs e )
		{
			m_AppKey.SetValue( "ProbeType", "0" );
		}

		private void radioButtonProbe_CheckedChanged( object sender, EventArgs e )
		{
			m_AppKey.SetValue( "ProbeType", "1" );
		}

		private void radioButtonCylindrical_CheckedChanged( object sender, EventArgs e )
		{
			m_AppKey.SetValue( "ProbeType", "2" );
		}

		private void integerTrackbarControlCubeMapSize_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			m_AppKey.SetValue( "CubeMapSize", _Sender.Value.ToString() );
		}
	}
}
