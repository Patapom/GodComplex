using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StandardizedDiffuseAlbedoMaps
{
	public partial class Form1 : Form
	{
		Bitmap2		m_BitmapXYZ = null;

		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click( object sender, EventArgs e )
		{
 			if ( openFileDialogSourceImage.ShowDialog( this ) != DialogResult.OK )
 				return;

// 			System.IO.FileInfo	ImageFile = new System.IO.FileInfo( @"Camera Calibration\Photoshop\CRW_7443.png" );
//			System.IO.FileInfo	ImageFile = new System.IO.FileInfo( @"Camera Calibration\Photoshop\CRW_7443_small.png" );
// 			System.IO.FileInfo	ImageFile = new System.IO.FileInfo( @"Camera Calibration\Photoshop\CRW_7443.tif" );
//			System.IO.FileInfo	ImageFile = new System.IO.FileInfo( @"Camera Calibration\Photoshop\CRW_7443_128bpp.tif" );

			System.IO.FileInfo	ImageFile = new System.IO.FileInfo( openFileDialogSourceImage.FileName );

			m_BitmapXYZ = new Bitmap2( ImageFile );

			checkBoxsRGB_CheckedChanged( null, EventArgs.Empty );
		}

		private void outputPanel_MouseMove( object sender, MouseEventArgs e )
		{
			if ( m_BitmapXYZ == null )
				return;

			float	Lum = m_BitmapXYZ.ContentXYZ[e.X*m_BitmapXYZ.Width/outputPanel.Width,e.Y*m_BitmapXYZ.Height/outputPanel.Height].y;
			if ( checkBoxsRGB.Checked )
				Lum = Bitmap2.ColorProfile.Linear2sRGB( Lum );

			labelLuminance.Text = "L=" + Lum.ToString( "G4" ) + " (" + (int) (Lum*255) + ")";
		}

		private void checkBoxsRGB_CheckedChanged( object sender, EventArgs e )
		{
			bool		sRGB = checkBoxsRGB.Checked;
			float[,]	Lum = new float[m_BitmapXYZ.Width,m_BitmapXYZ.Height];
			for ( int Y=0; Y < m_BitmapXYZ.Height; Y++ )
				for ( int X=0; X < m_BitmapXYZ.Width; X++ )
				{
					float	L = m_BitmapXYZ.ContentXYZ[X,Y].y;
					Lum[X,Y] = sRGB ? Bitmap2.ColorProfile.Linear2sRGB( L ) : L;
				}
			outputPanel.Luminance = Lum;
		}
	}
}
