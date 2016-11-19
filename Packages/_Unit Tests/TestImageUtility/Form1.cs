//#define TEST_BLACK_BODY_LOCUS

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

		public TestForm() {
			InitializeComponent();
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

//			TestBuildImage();
//			TestLoadImage();
			TestConvertLDR2HDR();
//			TestBlackBodyRadiation();
		}

		protected void	DrawPoint( int _X, int _Y, ref float4 _color ) {
			uint	minX = (uint) Math.Max( 0, _X-6 );
			uint	minY = (uint) Math.Max( 0, _Y-6 );
			uint	maxX = (uint) Math.Min( m_imageFile.Width, _X + 7 );
			uint	maxY = (uint) Math.Min( m_imageFile.Height, _Y + 7 );
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
			for ( int locusIndex=0; locusIndex < 20; locusIndex++ ) {
//				float	T = 1500.0f + (8000.0f - 1500.0f) * locusIndex / 20.0f;
				float	T = 1500.0f + 500.0f * locusIndex;

				ColorProfile.ComputeWhitePointChromaticities( T, ref xy );

// Plot with the color of the white point
// ColorProfile.xyY2XYZ( new float3( xy, 1.0f ), ref XYZ );
// sRGB.XYZ2RGB( new float4( XYZ, 1.0f ), ref color );

				float2	fPos = cornerZero + (cornerPoint8Point9 - cornerZero) * new float2( xy.x / 0.8f, xy.y / 0.9f );
				DrawPoint( (int) fPos.x, (int) fPos.y, ref color );
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
				new System.IO.FileInfo( @"..\..\Images\In\LDR2HDR\FromJPG\IMG_0864.jpg" ),
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

//panel1.Bitmap = Bitmap.DEBUG.AsBitmap;

				// Render the response curve as a bitmap
				ImageFile	tempCurveBitmap = new ImageFile( 256, 32, ImageFile.PIXEL_FORMAT.RGB8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
				float4		tempValue = new float4();
				for ( int i=0; i < 256; i++ ) {
					float	g = responseCurve[i];
					float	v = (float) Math.Pow( 2.0, g );

					v *= 0.15f;

					tempValue.Set( v, v, v, 1.0f );
					for ( uint Y=0; Y < 32; Y++ )
						tempCurveBitmap[(uint)i,Y] = tempValue;
				}
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
