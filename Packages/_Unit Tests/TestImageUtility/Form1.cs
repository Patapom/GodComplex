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

namespace UnitTests.ImageUtility
{
	public partial class TestForm : Form {

		ImageFile	m_imageFile = new ImageFile();

		public TestForm() {
			InitializeComponent();
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\LDR2HDR\FromJPG\IMG_0868.jpg" ) );
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\JPG\R8.jpg" ) );

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
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\TIFF\RGB8_ICC.tif" ) );
				// 16-bits
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\TIFF\RGB16.tif" ) );
//			m_imageFile.Load( new System.IO.FileInfo( @"..\..\Images\In\TIFF\RGB16_ICC.tif" ) );


			// High-Dynamic Range Images
			ImageFile	originalImageFile = m_imageFile;
			if ( true ) {
				ImageFile	tempHDR = new ImageFile();
				originalImageFile = tempHDR;

				// HDR
//				tempHDR.Load( new System.IO.FileInfo( @"..\..\Images\In\HDR\RGB32F.hdr" ) );

				// EXR
// 				tempHDR.Load( new System.IO.FileInfo( @"..\..\Images\In\EXR\RGB32F.exr" ) );

				// TIFF
					// 16-bits floating-point
//				tempHDR.Load( new System.IO.FileInfo( @"..\..\Images\In\TIFF\RGB16F.tif" ) );
//				tempHDR.Load( new System.IO.FileInfo( @"..\..\Images\In\TIFF\RGB16F_ICC.tif" ) );

					// 32-bits floating-point
				tempHDR.Load( new System.IO.FileInfo( @"..\..\Images\In\TIFF\RGB32F.tif" ) );
//				tempHDR.Load( new System.IO.FileInfo( @"..\..\Images\In\TIFF\RGB32F_ICC.tif" ) );

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
				"Gamma Exponent = " + MD.GammaExponent,
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
