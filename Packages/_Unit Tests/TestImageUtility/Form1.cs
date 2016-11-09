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

namespace UnitTests.ImageUtility
{
	public partial class TestForm : Form {

		ImageFile	m_imageFile = new ImageFile();

		public TestForm() {
			InitializeComponent();
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			m_imageFile.Load( new System.IO.FileInfo( @".\Images\In\LDR2HDR\FromJPG\IMG_0868.jpg" ) );
			panel1.Bitmap = m_imageFile.AsBitmap;

			// Write out metadata
			MetaData		MD = m_imageFile.Metadata;
			ColorProfile	Profile = MD.ColorProfile;
			textBoxEXIF.Lines = new string[] {
				"Profile:",
				"  • Chromaticities: ",
				"    R = " + Profile.Chromas.RecognizedChromaticity.Red.ToString(),
				"    G = " + Profile.Chromas.RecognizedChromaticity.Green.ToString(),
				"    B = " + Profile.Chromas.RecognizedChromaticity.Blue.ToString(),
				"    W = " + Profile.Chromas.RecognizedChromaticity.White.ToString(),
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
