using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace StandardizedDiffuseAlbedoMaps
{
	public partial class Form1 : Form
	{
		private RegistryKey			m_AppKey;
		private string				m_ApplicationPath;

		private Bitmap2				m_BitmapXYZ = null;

		#region METHODS

		public Form1()
		{
 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\StandardizedDiffuseAlbedoMaps" );
			m_ApplicationPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );

			InitializeComponent();
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

		private float	GetRegKeyFloat( string _Key, float _Default )
		{
			string	Value = GetRegKey( _Key, _Default.ToString() );
			float	Result;
			float.TryParse( Value, out Result );
			return Result;
		}

		private int		GetRegKeyInt( string _Key, float _Default )
		{
			string	Value = GetRegKey( _Key, _Default.ToString() );
			int		Result;
			int.TryParse( Value, out Result );
			return Result;
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
			System.Windows.Forms.MessageBox.Show( this, _Text, "SH Encoder", _Buttons, _Icon );
		}

		#endregion

		#endregion

		#region EVENT HANDLERS

		private void button1_Click( object sender, EventArgs e )
		{
 			string	OldFileName = GetRegKey( "LastImageFilename", m_ApplicationPath );
			openFileDialogSourceImage.InitialDirectory = System.IO.Path.GetDirectoryName( OldFileName );
			openFileDialogSourceImage.FileName = System.IO.Path.GetFileName( OldFileName );

			if ( openFileDialogSourceImage.ShowDialog( this ) != DialogResult.OK )
 				return;

			SetRegKey( "LastImageFilename", openFileDialogSourceImage.FileName );

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

		#endregion
	}
}
