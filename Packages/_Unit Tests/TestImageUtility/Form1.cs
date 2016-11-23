#define TEST_BLACK_BODY_LOCUS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ImageUtility;
using SharpMath;

namespace ImageUtility.UnitTests
{
	public partial class TestForm : Form {

		ImageFile	m_imageFile = new ImageFile();

		float4	black = new float4( 0, 0, 0, 1 );
		float4	white = new float4( 1, 1, 1, 1 );
		float4	red = new float4( 1, 0, 0, 1 );
		float4	green = new float4( 0, 1, 0, 1 );
		float4	blue = new float4( 0, 0, 1, 1 );


		public TestForm() {
			InitializeComponent();
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

//			TestBuildImage();
//			TestLoadImage();
			FastFit();
//			TestConvertLDR2HDR();
//			TestBlackBodyRadiation();
//			TestGraph();
		}

		void	FastFit() {
			// Load response curve
//			string	text = "";
			List<float>	responseCurve = new List<float>();
			using ( System.IO.FileStream S = new System.IO.FileInfo( "../../responseCurve9.float" ).OpenRead() )
				using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) ) {
					for ( int i=0; i < 256; i++ ) {
						responseCurve.Add( R.ReadSingle() );
//						text += ", " + responseCurve[responseCurve.Count-1];
					}
				}

			// Perform fitting
			float	a = 0.0f, b = 1.0f, c = 0.0f, d = 0.0f;		// sumSqDiff = 21.664576085822642
//			float	a = -6.55077f, b = 0.1263f, c = -0.000435788f, d = 7.52068e-7f;
//			FindFit( responseCurve.ToArray(), ref a, ref b, ref c, ref d );
			FindFitBFGS( responseCurve.ToArray(), ref a, ref b, ref c, ref d );

			// Render
			m_imageFile.Init( 1024, 768, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			m_imageFile.Clear( new float4( 1, 1, 1, 1 ) );

			float2	rangeX = new float2( 0, 255 );
			float2	rangeY = new float2( -2, 2 );
/*
			m_imageFile.PlotGraphAutoRangeY( black, rangeX, ref rangeY, ( float x ) => {
				int		i0 = (int) Math.Min( 255, Math.Floor( x ) );
				int		i1 = (int) Math.Min( 255, i0+1 );
				float	g0 = responseCurve[i0];
				float	g1 = responseCurve[i1];
				float	t = x - i0;
				return TentFilter( x ) * (g0 + (g1-g0) * t);
//				return (float) Math.Pow( 2.0f, g0 + (g1-g0) * t );
			} );
			m_imageFile.PlotGraph( red, rangeX, rangeY, ( float x ) => {
				return TentFilter( x ) * (a + b * x + c * x*x + d * x*x*x);
			} );
			m_imageFile.PlotLogAxes( black, rangeX, rangeY, -16.0f, 2.0f );
*/

//			m_imageFile.PlotGraphAutoRangeY( black, rangeX, ref rangeY, ( float x ) => {
			rangeY = new float2( 0, 400 );
			m_imageFile.PlotGraph( black, rangeX, rangeY, ( float x ) => {
				int		i0 = (int) Math.Min( 255, Math.Floor( x ) );
				int		i1 = (int) Math.Min( 255, i0+1 );
				float	g0 = responseCurve[i0];
				float	g1 = responseCurve[i1];
				float	t = x - i0;
//				return (float) (Math.Log( (g0 + (g1-g0) * t) ) / Math.Log( 2 ));
				return (float) Math.Pow( 2.0, (g0 + (g1-g0) * t) );
			} );
			m_imageFile.PlotGraph( red, rangeX, rangeY, ( float x ) => {
//				return (float) (Math.Log( (a + b * x + c * x*x + d * x*x*x) ) / Math.Log( 2 ));
				return (float) Math.Pow( 2.0, (a + b * x + c * x*x + d * x*x*x) );
			} );

			panel1.Bitmap = m_imageFile.AsBitmap;
		}

		class BFGSModel : SharpMath.BFGS.Model {
			public float[]	m_curve;
			public double[]	m_parameters;

			public double[] Parameters {
				get { return m_parameters; }
				set { m_parameters = value; }
			}

			double	EvalModel( double x, double[] _parameters ) {
				return _parameters[0] + _parameters[1] * x + _parameters[2] * x*x + _parameters[3] * x*x*x;
			}
			public double Eval( double[] _newParameters ) {
				double	sumSqDiff = 0.0f;
				for ( int i=0; i < 256; i++ ) {
					double	curveValue = m_curve[i];
					double	modelValue = EvalModel( i, _newParameters );
					double	diff = modelValue - curveValue;
							diff *= TentFilter( (float) i );
					sumSqDiff += diff * diff;
				}
				return sumSqDiff / (256.0*256.0);
			}

			public void Constrain( double[] _parameters ) {
			}
		}
		double	FindFitBFGS( float[] _curve, ref float a, ref float b, ref float c, ref float d ) {
			BFGSModel	model = new BFGSModel() {
				m_curve = _curve,
				m_parameters = new double[] { a, b, c, d }
			};

			SharpMath.BFGS	fitter = new SharpMath.BFGS();
			fitter.Minimize( model );

			a = (float) model.m_parameters[0];
			b = (float) model.m_parameters[1];
			c = (float) model.m_parameters[2];
			d = (float) model.m_parameters[3];

			return fitter.FunctionMinimum;
		}

		// Super simple fitting method
		static float	TentFilter( float x ) {
			return 1.0f - Math.Abs( x - 127.0f ) / 128.0f;
		}
		double	Model( double x, double[] _parms ) {
			return _parms[0] + _parms[1] * x + _parms[2] * x*x + _parms[3] * x*x*x;
		}
		double	EstimateSqDiff( float[] _curve, double[] _parms ) {
			double	sumSqDiff = 0.0f;
			for ( int i=0; i < 256; i++ ) {
				double	curveValue = _curve[i];
				double	modelValue = Model( i, _parms );
				double	diff = modelValue - curveValue;
						diff *= TentFilter( (float) i );
				sumSqDiff += diff * diff;
			}
			return sumSqDiff / (256.0*256.0);
		}
		double	FindFit( float[] _curve, ref float a, ref float b, ref float c, ref float d ) {
			const double	gradientDelta = 1e-6;
			const double	gradFactor = 1.0 / gradientDelta;
			const double	tolerance = 1e-4;
			const double	growthTolerance = 1e-9;

			double[]	oldParameterValues = new double[4];
			double[]	parameterValues = new double[4] { a, b, c, d };
			double[]	leftGradientValues = new double[4];
			double[]	rightGradientValues = new double[4];

			double		currentValue = EstimateSqDiff( _curve, parameterValues );
			double		stepSize = 1.0;

			for ( int iteration=0; iteration < 1000; iteration++ ) {
				// Change parameter values & compute gradients
				for ( int parmIndex=0; parmIndex < 4; parmIndex++ ) {
					double	parmValue = parameterValues[parmIndex];

					// Estimate left
					parameterValues[parmIndex] = parmValue - gradientDelta;
					double	leftValue = EstimateSqDiff( _curve, parameterValues );

					// Estimate right
					parameterValues[parmIndex] = parmValue + gradientDelta;
					double	rightValue = EstimateSqDiff( _curve, parameterValues );

					// Restore original parm value and compute gradients
					parameterValues[parmIndex] = parmValue;
					leftGradientValues[parmIndex] = gradFactor * (leftValue - currentValue);
					rightGradientValues[parmIndex] = gradFactor * (rightValue - currentValue);
				}

				// Follow gradient's downward slope
				for ( int parmIndex=0; parmIndex < 4; parmIndex++ )
					oldParameterValues[parmIndex] = parameterValues[parmIndex];

				double	oldValue = currentValue;
				do {
					for ( int parmIndex=0; parmIndex < 4; parmIndex++ ) {
						double	left = leftGradientValues[parmIndex];
						double	right = rightGradientValues[parmIndex];
						if ( left > 0.0 && right > 0.0 )
							continue;	// Both gradients are positive so we're basically at a minimum right where we are!
						if ( left < right )
							parameterValues[parmIndex] = oldParameterValues[parmIndex] + stepSize * left;
						else
							parameterValues[parmIndex] = oldParameterValues[parmIndex] + stepSize * right;
					}
					currentValue = EstimateSqDiff( _curve, parameterValues );
					if ( currentValue > oldValue )
						stepSize *= 0.5;	// Reduce step size
				} while ( currentValue > oldValue );
				stepSize *= 2.0;

				if ( Math.Abs( currentValue ) < tolerance )
					break;	// Found a minimum!

				double	absoluteDiffValue = currentValue - oldValue;
				double	relativeDiffValue = currentValue / oldValue - 1.0;
				if ( Math.Abs( relativeDiffValue ) < growthTolerance )
					break;	// Method doesn't grow anymore...
			}

			a = (float) parameterValues[0];
			b = (float) parameterValues[1];
			c = (float) parameterValues[2];
			d = (float) parameterValues[3];

			return currentValue;
		}

		void TestGraph() {
			ColorProfile	sRGB = new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB );
			m_imageFile.Init( 1024, 768, ImageFile.PIXEL_FORMAT.RGBA8, sRGB );
			m_imageFile.Clear( new float4( 1, 1, 1, 1 ) );
//			m_imageFile.Clear( new float4( 0, 0, 0, 1 ) );

			if ( true ) {
				// Unit test simple graph
				float2	rangeY = new float2( -1.0f, 1.0f );
//				m_imageFile.PlotGraph( black, new float2( -30.0f, 30.0f ), rangeY, ( float x ) => { return (float) Math.Sin( x ) / x; } );
// 				m_imageFile.PlotGraphAutoRangeY( black, new float2( -30.0f, 30.0f ), ref rangeY, ( float x ) => { return (float) Math.Sin( x ) / x; } );
// 				m_imageFile.PlotAxes( black, new float2( -30.0f, 30.0f ), rangeY, (float) (0.5 * Math.PI), 0.1f );

				m_imageFile.PlotLogGraph( red, new float2( 0.0f, 2.0f ), new float2( 0.0f, 100.0f ), ( float x ) => { return (float) Math.Pow( 10.0, x ); }, 1.0f, 1.0f );
//				m_imageFile.PlotLogGraph( green, new float2( -2.0f, 2.0f ), new float2( 0.0f, 100.0f ), ( float x ) => { return (float) Math.Pow( 10.0, x ); }, 10.0f, 1.0f );
				m_imageFile.PlotLogGraph( green, new float2( 0.0f, 2.0f ), new float2( 0.0f, 2.0f ), ( float x ) => { return (float) Math.Pow( 10.0, x ); }, 1.0f, 10.0f );
				m_imageFile.PlotLogGraph( blue, new float2( -2.0f, 2.0f ), new float2( -2.0f, 2.0f ), ( float x ) => { return (float) Math.Pow( 10.0, x ); }, 10.0f, 10.0f );
// 				m_imageFile.PlotLogAxes( black, new float2( -1000.0f, 1000.0f ), new float2( -100.0f, 100.0f ), -100.0f, -10.0f );
// 				m_imageFile.PlotLogAxes( black, new float2( -100.0f, 1000.0f ), new float2( -2.0f, 2.0f ), -10.0f, 10.0f );
 				m_imageFile.PlotLogAxes( black, new float2( -2.0f, 2.0f ), new float2( -2.0f, 2.0f ), 10.0f, 2.0f );

			} else if ( true ) {
				// Unit test a LOT of clipped lines!
				int	W = (int) m_imageFile.Width;
				int	H = (int) m_imageFile.Height;
				Random	R = new Random( 1 );
				float2	P0 = new float2();
				float2	P1 = new float2();
				for ( int i=0; i < 10000; i++ ) {
					P0.x = (float) (R.NextDouble() * 3*W) - W;
					P0.y = (float) (R.NextDouble() * 3*H) - H;
					P1.x = (float) (R.NextDouble() * 3*W) - W;
					P1.y = (float) (R.NextDouble() * 3*H) - H;
					m_imageFile.DrawLine( black, P0, P1 );
//					m_imageFile.DrawLine( R.NextDouble() > 0.5 ? white : black, P0, P1 );
				}
			}

			panel1.Bitmap = m_imageFile.AsBitmap;
		}

		protected void	DrawPoint( int _X, int _Y, int _size, ref float4 _color ) {
			uint	minX = (uint) Math.Max( 0, _X-_size );
			uint	minY = (uint) Math.Max( 0, _Y-_size );
			uint	maxX = (uint) Math.Min( m_imageFile.Width, _X + _size+1 );
			uint	maxY = (uint) Math.Min( m_imageFile.Height, _Y + _size+1 );
			for ( uint Y=minY; Y < maxY; Y++ )
				for ( uint X=minX; X < maxX; X++ )
					m_imageFile[X,Y] = _color;
		}
		protected void	TestBlackBodyRadiation() {
			ColorProfile	sRGB = new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB );

#if TEST_BLACK_BODY_LOCUS
			// Load the color gamut and try and plot the locii of various white points
			//
			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\xyGamut.png" ) );

			float2	cornerZero = new float2( 114, 1336 );			// xy=(0.0, 0.0)
			float2	cornerPoint8Point9 = new float2( 1257, 49 );	// xy=(0.8, 0.9)

// Check XYZ<->RGB and XYZ<->xyY converter code
// 			float3	xyY = new float3();
// 			float3	XYZ = new float3();
// 
// float4	testRGB = new float4();
// float4	testXYZ = new float4();
// for ( int i=1; i <= 10; i++ ) {
// 	float	f = i / 10.0f;
// 	testRGB.Set( 1*f, 1*f, 1*f, 1.0f );
// 	sRGB.RGB2XYZ( testRGB, ref testXYZ );
// 
// XYZ.Set( testXYZ.x, testXYZ.y, testXYZ.z );
// ColorProfile.XYZ2xyY( XYZ, ref xyY );
// ColorProfile.xyY2XYZ( xyY, ref XYZ );
// testXYZ.Set( XYZ, 1.0f );
// 
// 	sRGB.XYZ2RGB( testXYZ, ref testRGB );
// }

			float2	xy = new float2();
			float4	color = new float4( 1, 0, 0, 1 );
			float4	color2 = new float4( 0, 0.5f, 1, 1 );
			for ( int locusIndex=0; locusIndex < 20; locusIndex++ ) {
//				float	T = 1500.0f + (8000.0f - 1500.0f) * locusIndex / 20.0f;
				float	T = 1500.0f + 500.0f * locusIndex;

				ColorProfile.ComputeWhitePointChromaticities( T, ref xy );

// Plot with the color of the white point
// ColorProfile.xyY2XYZ( new float3( xy, 1.0f ), ref XYZ );
// sRGB.XYZ2RGB( new float4( XYZ, 1.0f ), ref color );

				float2	fPos = cornerZero + (cornerPoint8Point9 - cornerZero) * new float2( xy.x / 0.8f, xy.y / 0.9f );
				DrawPoint( (int) fPos.x, (int) fPos.y, 6, ref color );

				ColorProfile.ComputeWhitePointChromaticitiesAnalytical( T, ref xy );
				fPos = cornerZero + (cornerPoint8Point9 - cornerZero) * new float2( xy.x / 0.8f, xy.y / 0.9f );
				DrawPoint( (int) fPos.x, (int) fPos.y, 3, ref color2 );
			}

#else
			// Compute white balancing from a D50 to a D65 illuminant
			float3x3	WhiteBalancingXYZ = ColorProfile.ComputeWhiteBalanceXYZMatrix( ColorProfile.Chromaticities.sRGB, ColorProfile.ILLUMINANT_D50 );
			// Compute white balancing from a D65 to a D50 illuminant
//			float3x3	WhiteBalancingXYZ = ColorProfile.ComputeWhiteBalanceXYZMatrix( ColorProfile.Chromaticities.AdobeRGB_D50, ColorProfile.ILLUMINANT_D65 );

			// Build a gradient of white points from 1500K to 8000K
			m_imageFile.Init( 650, 32, ImageFile.PIXEL_FORMAT.RGBA8, sRGB );

			float3	RGB = new float3( 0, 0, 0 );
			float3	XYZ = new float3( 0, 0, 0 );
			float2	xy = new float2();
			for ( uint X=0; X < 650; X++ ) {
				float	T = 1500 + 10 * X;	// From 1500K to 8000K
				ColorProfile.ComputeWhitePointChromaticities( T, ref xy );

				ColorProfile.xyY2XYZ( new float3( xy, 1.0f ), ref XYZ );

				// Apply white balancing
//				XYZ *= WhiteBalancingXYZ;

				sRGB.XYZ2RGB( XYZ, ref RGB );

// "Normalize"
//RGB /= Math.Max( Math.Max( RGB.x, RGB.y ), RGB.z );

// Isolate D65
if ( Math.Abs( T - 6500.0f ) < 10.0f )
	RGB.Set( 1, 0, 1 );

				for ( uint Y=0; Y < 32; Y++ ) {
					m_imageFile[X,Y] = new float4( RGB, 1.0f );
				}
			}

// Check white balancing yields correct results
// float3	XYZ_R_in = new float3();
// float3	XYZ_G_in = new float3();
// float3	XYZ_B_in = new float3();
// float3	XYZ_W_in = new float3();
// sRGB.RGB2XYZ( new float3( 1, 0, 0 ), ref XYZ_R_in );
// sRGB.RGB2XYZ( new float3( 0, 1, 0 ), ref XYZ_G_in );
// sRGB.RGB2XYZ( new float3( 0, 0, 1 ), ref XYZ_B_in );
// sRGB.RGB2XYZ( new float3( 1, 1, 1 ), ref XYZ_W_in );
// 
// float3	XYZ_R_out = XYZ_R_in * XYZ_D65_D50;
// float3	XYZ_G_out = XYZ_G_in * XYZ_D65_D50;
// float3	XYZ_B_out = XYZ_B_in * XYZ_D65_D50;
// float3	XYZ_W_out = XYZ_W_in * XYZ_D65_D50;
// 

// float3	xyY_R_out = new float3();
// float3	xyY_G_out = new float3();
// float3	xyY_B_out = new float3();
// float3	xyY_W_out = new float3();
// ColorProfile.XYZ2xyY( XYZ_R_out, ref xyY_R_out );
// ColorProfile.XYZ2xyY( XYZ_G_out, ref xyY_G_out );
// ColorProfile.XYZ2xyY( XYZ_B_out, ref xyY_B_out );
// ColorProfile.XYZ2xyY( XYZ_W_out, ref xyY_W_out );

#endif
			panel1.Bitmap = m_imageFile.AsBitmap;
		}

		protected void TestBuildImage() {
			m_imageFile = new ImageFile( 378, 237, ImageFile.PIXEL_FORMAT.RGB16, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );

			if ( false ) {
				// Write pixel per pixel
				for ( uint Y=0; Y < m_imageFile.Height; Y++ ) {
					for ( uint X=0; X < m_imageFile.Width; X++ ) {
						float	R = (float) (1+X) / m_imageFile.Width;
						float	G = (float) (1+Y) / m_imageFile.Height;
						m_imageFile[X,Y] = new float4( R, G, 1.0f-0.5f*(R+G), 1 );
					}
				}
			} else if ( false ) {
				// Write scanline per scanline
				float4[]	scanline = new float4[m_imageFile.Width];
				for ( uint Y=0; Y < m_imageFile.Height; Y++ ) {
					for ( uint X=0; X < m_imageFile.Width; X++ ) {
						float	R = (float) (1+X) / m_imageFile.Width;
						float	G = (float) (1+Y) / m_imageFile.Height;
						scanline[X].Set( R, G, 1.0f-0.5f*(R+G), 1 );
					}

					m_imageFile.WriteScanline( Y, scanline );
				}
			} else {
				// Buddhabrot
				uint	W = m_imageFile.Width;
				uint	H = m_imageFile.Height;
				float2	Z, Z0;
				int		iterations = 50;
				float4	inc = (1.0f / iterations) * float4.One;
				float	zoom = 2.0f;
				float	invZoom = 1.0f / zoom;

#if DIRECT_WRITE	// Either directly accumulate to image
				for ( uint Y=0; Y < H; Y++ ) {
					Z0.y = zoom * (Y - 0.5f * H) / H;
					for ( uint X=0; X < W; X++ ) {
						Z0.x = zoom * (X - 0.5f * W) / H;
						Z = Z0;
						for ( int i=0; i < iterations; i++ ) {
							Z.Set( Z.x*Z.x - Z.y*Z.y + Z0.x, 2.0f * Z.x * Z.y + Z0.y );

							int	Nx = (int) (invZoom * Z.x * H + 0.5f * W);
							int	Ny = (int) (invZoom * Z.y * H + 0.5f * H);
							if ( Nx >= 0 && Nx < W && Ny >= 0 && Ny < H ) {
// 								float4	tagada = (float4) m_imageFile[(uint)Nx,(uint)Ny];
// 								tagada += inc;
// 								m_imageFile[(uint)Nx,(uint)Ny] = tagada;
 								m_imageFile.Add( (uint)Nx, (uint)Ny, inc );
							}
						}
					}
				}
#else				// Or accumulate to a temp array and write result (this is obviously faster!)
				float[,]	accumulators = new float[W,H];
				for ( uint Y=0; Y < H; Y++ ) {
					Z0.y = zoom * (Y - 0.5f * H) / H;
					for ( uint X=0; X < W; X++ ) {
						Z0.x = zoom * (X - 0.5f * W) / H;
						Z = Z0;
						for ( int i=0; i < iterations; i++ ) {
							Z.Set( Z.x*Z.x - Z.y*Z.y + Z0.x, 2.0f * Z.x * Z.y + Z0.y );

							int	Nx = (int) (invZoom * Z.x * H + 0.5f * W);
							int	Ny = (int) (invZoom * Z.y * H + 0.5f * H);
							if ( Nx >= 0 && Nx < W && Ny >= 0 && Ny < H )
								accumulators[Nx, Ny] += inc.x;
						}
					}
				}
				float4	temp = new float4();
				for ( uint Y=0; Y < H; Y++ ) {
					for ( uint X=0; X < W; X++ ) {
						float	a = accumulators[X,Y];
						temp.Set( a, a, a, 1 );
						m_imageFile[X,Y] = temp;
					}
				}
#endif
			}


			panel1.Bitmap = m_imageFile.AsBitmap;
		}

		protected void TestConvertLDR2HDR() {

			// Load a bunch of LDR images
			System.IO.FileInfo[]	LDRImageFileNames = new System.IO.FileInfo[] {
				new System.IO.FileInfo( @"..\..\Images\In\LDR2HDR\FromJPG\IMG_0860.jpg" ),
//				new System.IO.FileInfo( @"..\..\Images\In\LDR2HDR\FromJPG\IMG_0861.jpg" ),
//				new System.IO.FileInfo( @"..\..\Images\In\LDR2HDR\FromJPG\IMG_0862.jpg" ),
//				new System.IO.FileInfo( @"..\..\Images\In\LDR2HDR\FromJPG\IMG_0863.jpg" ),
				new System.IO.FileInfo( @"..\..\Images\In\LDR2HDR\FromJPG\IMG_0864.jpg" ),
//				new System.IO.FileInfo( @"..\..\Images\In\LDR2HDR\FromJPG\IMG_0865.jpg" ),
//				// Don't use 866 because of bad ISO
//				new System.IO.FileInfo( @"..\..\Images\In\LDR2HDR\FromJPG\IMG_0867.jpg" ),
//				new System.IO.FileInfo( @"..\..\Images\In\LDR2HDR\FromJPG\IMG_0868.jpg" ),
				new System.IO.FileInfo( @"..\..\Images\In\LDR2HDR\FromJPG\IMG_0869.jpg" ),
			};
			List< ImageFile >	LDRImages = new List< ImageFile >();
			foreach ( System.IO.FileInfo LDRImageFileName in LDRImageFileNames )
				LDRImages.Add( new ImageFile( LDRImageFileName ) );

			// Retrieve the shutter speeds
			List< float >	shutterSpeeds = new List< float >();
			foreach ( ImageFile LDRImage in LDRImages ) {
				shutterSpeeds.Add( LDRImage.Metadata.ExposureTime );
			}

			// Build the HDR device-independent bitmap
			Bitmap.HDRParms	parms = new Bitmap.HDRParms() {
				_inputBitsPerComponent = 8,
				_luminanceFactor = 1.0f,
				_curveSmoothnessConstraint = 1.0f,
				_quality = 1.0f
			};

			ImageUtility.Bitmap	HDRImage = new ImageUtility.Bitmap();
			try {
//				HDRImage.LDR2HDR( LDRImages.ToArray(), shutterSpeeds.ToArray(), parms );

				List< float >	responseCurve = new List< float >();
				Bitmap.ComputeCameraResponseCurve( LDRImages.ToArray(), shutterSpeeds.ToArray(), parms, responseCurve );

				using ( System.IO.FileStream S = new System.IO.FileInfo( "../../responseCurve3.float" ).Create() )
					using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) ) {
						for ( int i=0; i < 256; i++ )
							W.Write( responseCurve[i] );
					}

//panel1.Bitmap = Bitmap.DEBUG.AsBitmap;

// 				// Render the response curve as a bitmap
// 				ImageFile	tempCurveBitmap = new ImageFile( 256, 32, ImageFile.PIXEL_FORMAT.RGB8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
// 				float4		tempValue = new float4();
// 				for ( int i=0; i < 256; i++ ) {
// 					float	g = responseCurve[i];
// 					float	v = (float) Math.Pow( 2.0, g );
// 
// 					v *= 0.15f;
// 
// 					tempValue.Set( v, v, v, 1.0f );
// 					for ( uint Y=0; Y < 32; Y++ )
// 						tempCurveBitmap[(uint)i,Y] = tempValue;
// 				}
// 				panel1.Bitmap = tempCurveBitmap.AsBitmap;

				//////////////////////////////////////////////////////////////////////////////////////////////
				// Render the response curve as a graph
 				ImageFile	tempCurveBitmap = new ImageFile( 1024, 768, ImageFile.PIXEL_FORMAT.RGB8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );

				string		info = "Exposures:\r\n";
				foreach ( float shutterSpeed in shutterSpeeds )
					info += " " + shutterSpeed + "s + ";
				info += "\r\nLog2 exposures (EV):\r\n";
				foreach ( float shutterSpeed in shutterSpeeds )
					info += " " + (float) (Math.Log( shutterSpeed ) / Math.Log(2)) + "EV + ";
				info += "\r\n\r\n";

				float4		black = new float4( 0, 0, 0, 1 );
				float2		rangeX = new float2( 0, 256 );
				float2		rangeY = new float2( 0, 1000 );
				tempCurveBitmap.Clear( new float4( 1, 1, 1, 1 ) );
//				tempCurveBitmap.PlotGraphAutoRangeY( red, rangeX, ref rangeY, ( float x ) => {
				tempCurveBitmap.PlotGraph( red, rangeX, rangeY, ( float x ) => {
					int		i0 = (int) Math.Min( 255, Math.Floor( x ) );
					int		i1 = (int) Math.Min( 255, i0+1 );
					float	g0 = responseCurve[i0];
					float	g1 = responseCurve[i1];
					float	t = x - i0;
//					return g0 + (g1-g0) * t;
					return (float) Math.Pow( 2.0f, g0 + (g1-g0) * t );
				} );
//				tempCurveBitmap.PlotAxes( black, rangeX, rangeY, 8, 2 );

				info += "• Linear range Y = [" + rangeY.x + ", " + rangeY.y + "]\r\n";

				rangeY = new float2( -4, 4 );
				tempCurveBitmap.PlotLogGraphAutoRangeY( black, rangeX, ref rangeY, ( float x ) => {
//				tempCurveBitmap.PlotLogGraph( black, rangeX, rangeY, ( float x ) => {
					int		i0 = (int) Math.Min( 255, Math.Floor( x ) );
					int		i1 = (int) Math.Min( 255, i0+1 );
					float	g0 = responseCurve[i0];
					float	g1 = responseCurve[i1];
					float	t = x - i0;
//					return g0 + (g1-g0) * t;
					return (float) Math.Pow( 2.0f, g0 + (g1-g0) * t );
				}, -1.0f, 2.0f );
				tempCurveBitmap.PlotLogAxes( black, rangeX, rangeY, -16, 2 );

				info += "• Log2 range Y = [" + rangeY.x + ", " + rangeY.y + "]\r\n";
				textBoxEXIF.Text = info;

 				panel1.Bitmap = tempCurveBitmap.AsBitmap;

/*
				// Recompose the HDR image
				HDRImage.LDR2HDR( LDRImages.ToArray(), shutterSpeeds.ToArray(), responseCurve, 1.0f );

				// Display as a tone-mapped bitmap
				ImageFile	tempHDR = new ImageFile();
				HDRImage.ToImageFile( tempHDR, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );

				ImageFile	tempToneMappedHDR = new ImageFile();
				tempToneMappedHDR.ToneMapFrom( tempHDR,( float3 _HDRColor, ref float3 _LDRColor ) => {
					// Do nothing (linear space to gamma space without care!)
//					_LDRColor = _HDRColor;

					// Just do gamma un-correction, don't care about actual HDR range...
					_LDRColor.x = (float) Math.Pow( Math.Max( 0.0f, _HDRColor.x ), 1.0f / 2.2f );	// Here we need to clamp negative values that we sometimes get in EXR format
					_LDRColor.y = (float) Math.Pow( Math.Max( 0.0f, _HDRColor.y ), 1.0f / 2.2f );	//  (must be coming from the log encoding I suppose)
					_LDRColor.z = (float) Math.Pow( Math.Max( 0.0f, _HDRColor.z ), 1.0f / 2.2f );
				} );

				panel1.Bitmap = tempToneMappedHDR.AsBitmap;
*/

			} catch ( Exception _e ) {
				MessageBox.Show( "Error: " + _e.Message );

// Show debug image
panel1.Bitmap = Bitmap.DEBUG.AsBitmap;
			}
		}

		protected void TestLoadImage() {
			// BMP
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\BMP\RGB8.bmp" ) );
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\BMP\RGBA8.bmp" ) );

			// GIF
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\GIF\RGB8P.gif" ) );

			// JPG
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\JPG\R8.jpg" ) );
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\JPG\RGB8.jpg" ) );
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\JPG\RGB8_ICC.jpg" ) );

			// PNG
				// 8-bits
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\PNG\R8P.png" ) );
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\PNG\RGB8.png" ) );
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\PNG\RGB8_SaveforWeb.png" ) );
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\PNG\RGBA8.png" ) );
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\PNG\RGBA8_SaveforWeb.png" ) );
				// 16-bits
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\PNG\RGB16.png" ) );

			// TGA
			// @TODO => Check why I can't retrieve my custom metas!
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\TGA\RGB8.tga" ) );
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\TGA\RGBA8.tga" ) );

			// TIFF
				// 8-bits
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\TIFF\RGB8.tif" ) );
			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\TIFF\RGB8_ICC.tif" ) );
				// 16-bits
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\TIFF\RGB16.tif" ) );
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\TIFF\RGB16_ICC.tif" ) );


			// High-Dynamic Range Images
			ImageFile	originalImageFile = m_imageFile;
			if ( false ) {
				ImageFile	tempHDR = new ImageFile();
				originalImageFile = tempHDR;

				// HDR
//				tempHDR.Load( new System.IO.FileInfo( @"..\..\Images\In\HDR\RGB32F.hdr" ) );

				// EXR
 				tempHDR.Load( new System.IO.FileInfo( @"..\..\Images\In\EXR\RGB32F.exr" ) );

				// TIFF
					// 16-bits floating-point
//				tempHDR.Load( new System.IO.FileInfo( @"..\..\Images\In\TIFF\RGB16F.tif" ) );
//				tempHDR.Load( new System.IO.FileInfo( @"..\..\Images\In\TIFF\RGB16F_ICC.tif" ) );

// Pom (2016-11-14) This crashes as FreeImage is not capable of reading 32-bits floating point TIFs but I think I don't care, we have enough formats!
// 					// 32-bits floating-point
// 				tempHDR.Load( new System.IO.FileInfo( @"..\..\Images\In\TIFF\RGB32F.tif" ) );
// //				tempHDR.Load( new System.IO.FileInfo( @"..\..\Images\In\TIFF\RGB32F_ICC.tif" ) );

				m_imageFile.ToneMapFrom( tempHDR, ( float3 _HDRColor, ref float3 _LDRColor ) => {
					// Do nothing (linear space to gamma space without care!)
//					_LDRColor = _HDRColor;

					// Just do gamma un-correction, don't care about actual HDR range...
					_LDRColor.x = (float) Math.Pow( Math.Max( 0.0f, _HDRColor.x ), 1.0f / 2.2f );	// Here we need to clamp negative values that we sometimes get in EXR format
					_LDRColor.y = (float) Math.Pow( Math.Max( 0.0f, _HDRColor.y ), 1.0f / 2.2f );	//  (must be coming from the log encoding I suppose)
					_LDRColor.z = (float) Math.Pow( Math.Max( 0.0f, _HDRColor.z ), 1.0f / 2.2f );
				} );
 			}

			panel1.Bitmap = m_imageFile.AsBitmap;

			// Write out metadata
			MetaData		MD = originalImageFile.Metadata;
			ColorProfile	Profile = MD.ColorProfile;
			textBoxEXIF.Lines = new string[] {
				"File Format: " + originalImageFile.FileFormat,
				"Pixel Format: " + originalImageFile.PixelFormat + (originalImageFile.HasAlpha ? " (ALPHA)" : " (NO ALPHA)"),
				"",
				"Profile:",
				"  • Chromaticities: ",
				"    R = " + Profile.Chromas.Red.ToString(),
				"    G = " + Profile.Chromas.Green.ToString(),
				"    B = " + Profile.Chromas.Blue.ToString(),
				"    W = " + Profile.Chromas.White.ToString(),
				"    Recognized chromaticities = " + Profile.Chromas.RecognizedChromaticity,
				"  • Gamma Curve: " + Profile.GammaCurve.ToString(),
				"  • Gamma Exponent: " + Profile.GammaExponent.ToString(),
				"",
				"Gamma Found in File = " + MD.GammaSpecifiedInFile,
				"",
				"MetaData are valid: " + MD.IsValid,
				"  • ISO Speed = " + MD.ISOSpeed,
				"  • Exposure Time = " + (MD.ExposureTime > 1.0f ? (MD.ExposureTime + " seconds") : ("1/" + (1.0f / MD.ExposureTime) + " seconds")),
				"  • Tv = " + MD.Tv + " EV",
				"  • Av = " + MD.Av + " EV",
				"  • F = 1/" + MD.FNumber + " stops",
				"  • Focal Length = " + MD.FocalLength + " mm",
			};
		}

		protected override void OnClosed( EventArgs e ) {

			m_imageFile.Dispose();

			base.OnClosed( e );
		}
	}
}
