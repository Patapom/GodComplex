using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;
using ImageUtility;
using Renderer;

namespace GenerateHeightMapFromNormalMap
{
	public partial class TransformForm : Form {

		ImageFile		m_imageNormal = null;
		float3[,]		m_normals = null;

		ImageFile		m_imageHeight = null;
		float[,]		m_heights = null;

		public TransformForm() {
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			LoadNormalMap( new System.IO.FileInfo( "Example/normals.png" ) );
		}

		protected override void OnFormClosing(FormClosingEventArgs e) {
			base.OnFormClosing(e);

			if ( m_imageNormal != null )
				m_imageNormal.Dispose();
			if ( m_imageHeight != null )
				m_imageHeight.Dispose();
		}

		void	LoadNormalMap( System.IO.FileInfo _normalMapFileName ) {
			try {
				if ( m_imageNormal != null ) {
					m_imageNormal.Dispose();
					m_imageNormal = null;
					m_normals = null;
				}
				m_imageNormal = new ImageFile( _normalMapFileName );
				imagePanelNormal.Bitmap = m_imageNormal.AsBitmap;

				// Read normals
				m_normals = new float3[m_imageNormal.Width, m_imageNormal.Height];
				m_imageNormal.ReadPixels( ( uint X, uint Y, ref float4 _color ) => {
					m_normals[X,Y].Set( 2.0f * (_color.x - 0.5f), 2.0f * (_color.y - 0.5f), 2.0f * (_color.z - 0.5f) );
					m_normals[X,Y].Normalize();
				} );

			} catch ( Exception _e ) {
				MessageBox.Show( this, "gna! " + _e.Message );
			}
		}

		private void imagePanelNormal_Click(object sender, EventArgs e) {
		}

		private void buttonConvert_Click(object sender, EventArgs e)
		{
			Device	device = new Device();
			device.Init( imagePanelHeight.Handle, false, true );
			SharpMath.FFT.FFT2D_GPU	FFT = new SharpMath.FFT.FFT2D_GPU( device, m_imageNormal.Width );
			Complex[,]	signal = new Complex[m_imageNormal.Width,m_imageNormal.Height];
			m_imageNormal.ReadPixels( ( uint X, uint Y, ref float4 _color ) => { signal[X,Y].Set( _color.x, 0 ); } );

			Complex[,]	spectrum = FFT.FFT_Forward( signal );

			float	factor =  m_imageNormal.Width * m_imageNormal.Height;
			m_imageHeight = new ImageFile( m_imageNormal.Width, m_imageNormal.Height, ImageFile.PIXEL_FORMAT.R8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			m_imageHeight.WritePixels( ( uint X, uint Y, ref float4 _color ) => { _color.x = factor * (float) spectrum[X,Y].r; } );
			imagePanelHeight.Bitmap = m_imageHeight.AsBitmap;
			device.Dispose();
		}
	}
}
