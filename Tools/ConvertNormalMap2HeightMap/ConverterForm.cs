#define FUCK
//////////////////////////////////////////////////////////////////////////
// I'm applying the technique of "deconvolution" described by http://stannum.co.il/blog/1/reconstructing-a-height-map-from-a-normal-map
//
// The idea is to think of the normal map n(x,y) as a convolution of a gradient filter g(Dx) and g(Dy) with the original height map h(x,y)
//
//		n(x,y) = (nx(x,y), ny(x,y), 1)
//
// Where:
//		nx(x,y) = dx * h(x,y) = Sum_d[-oo, +oo]{ dx(d) h(x+d,y) }
//		ny(x,y) = dy * h(x,y) = Sum_d[-oo, +oo]{ dy(d) h(x,y+d) }
//
// and the * is the convolution operator.
//
// As explained by Dave Eberly in https://www.geometrictools.com/Documentation/ReconstructHeightFromNormals.pdf,
//	the gradient filter is usually either a one-sided difference:
//		nx = h(x+1,y) - h(x,y)		and		ny = h(x,y+1) - h(x,y)
//
// Or a central difference:
//		nx = h(x+1,y) - h(x-1,y)	and		ny = h(x,y+1) - h(x,y-1)
//
// After a Fourier transform, the convolution is simply the product of the terms of DX, DY and H:
//
//		NX(x,y) = DX(x,y) H(x,y)
//		NY(x,y) = DY(x,y) H(x,y)
//
// We know that NX²(x,y) + NY²(x,y) = DX²(x,y) H²(x,y) + DY²(x,y) H²(x,y) = 1
// We then pose:
//
//		H²(x,y) [DX²(x,y) + DY²(x,y)] = NX²(x,y) + NY²(x,y)
//
// And finally:
//		H(x,y) = sqrt( [NX²(x,y) + NY²(x,y)] / [DX²(x,y) + DY²(x,y)] )
//
// By applying the inverse Fourier transform, we get the desired h(x,y)
//
//////////////////////////////////////////////////////////////////////////
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

using SharpMath;
using ImageUtility;
using Renderer;

namespace GenerateHeightMapFromNormalMap
{
	public partial class TransformForm : Form {

		private RegistryKey		m_AppKey;
		private string			m_ApplicationPath;

		Device					m_device = new Device();

		ImageFile				m_imageNormal = null;
		uint					m_size;
		Complex[,]				nx;
		Complex[,]				ny;

		ImageFile				m_imageHeight = null;

		public TransformForm() {
			InitializeComponent();

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\NormalMap2HeightMapConverter" );
			m_ApplicationPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			m_device.Init( imagePanelHeight.Handle, false, true );

			LoadNormalMap( new System.IO.FileInfo( "Example/normals.png" ) );
		}

		protected override void OnFormClosing(FormClosingEventArgs e) {
			base.OnFormClosing(e);

			if ( m_imageNormal != null )
				m_imageNormal.Dispose();
			if ( m_imageHeight != null )
				m_imageHeight.Dispose();

			m_device.Dispose();
		}

		void	LoadNormalMap( System.IO.FileInfo _normalMapFileName ) {
			try {
				ImageFile	tempImageNormal = new ImageFile( _normalMapFileName );

				// Make sure we accept the image
				double	isPOT = Math.Log( tempImageNormal.Width ) / Math.Log( 2.0 );
				if (	tempImageNormal.Width != tempImageNormal.Height
					|| (int) isPOT != isPOT ) {
					throw new Exception( "The converter only supports square power-of-two textures!" );
				}

				// Replace existing
				if ( m_imageNormal != null ) {
					m_imageNormal.Dispose();
					m_imageNormal = null;
				}
				m_imageNormal = tempImageNormal;

				imagePanelNormal.Bitmap = m_imageNormal.AsBitmap;

				// Read normals
				m_size = m_imageNormal.Width;
				nx = new Complex[m_size,m_size];
				ny = new Complex[m_size,m_size];

				float3	normal = float3.Zero;
				m_imageNormal.ReadPixels( ( uint X, uint Y, ref float4 _color ) => {
					normal.Set( 2.0f * _color.x - 1.0f, 2.0f * _color.y - 1.0f, 2.0f * _color.z - 1.0f );
					normal.Normalize();

					double	fact = normal.z != 0.0 ? 1.0 / normal.z : 0.0;
					nx[X,Y].Set( fact * normal.x, 0 );
					ny[X,Y].Set( fact * normal.y, 0 );
				} );

				// Enable conversion
				buttonConvert.Enabled = true;
				buttonConvertOneSided.Enabled = true;

			} catch ( Exception _e ) {
				MessageBox( "An error occurred while loading the normal map:\r\n\r\n" + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		#region Helpers

		DialogResult	MessageBox( string _text, MessageBoxButtons _buttons ) {
			return System.Windows.Forms.MessageBox.Show( this, _text, "Normal Map -> Height Map Converter", _buttons );
		}
		DialogResult	MessageBox( string _text, MessageBoxButtons _buttons, MessageBoxIcon _icon ) {
			return System.Windows.Forms.MessageBox.Show( this, _text, "Normal Map -> Height Map Converter", _buttons, _icon );
		}

		private string	GetRegKey( string _key, string _default ) {
			string	Result = m_AppKey.GetValue( _key ) as string;
			return Result != null ? Result : _default;
		}
		private void	SetRegKey( string _key, string _Value ) {
			m_AppKey.SetValue( _key, _Value );
		}

		#endregion

		#region Conversion

		private void Convert( bool _centralValue ) {

			// Initialize central-valued gradient filter
			Complex[,]	dx = new Complex[m_size,m_size];
			Complex[,]	dy = new Complex[m_size,m_size];
			Array.Clear( dx, 0, (int) (m_size*m_size) );
			Array.Clear( dy, 0, (int) (m_size*m_size) );

			if ( _centralValue ) {
				dx[m_size-1,0].Set( -1, 0 );
				dx[1,0].Set( 1, 0 );
				dy[0,m_size-1].Set( 1, 0 );
				dy[0,1].Set( -1, 0 );
			} else {
				dx[0,0].Set( -1, 0 );
				dx[1,0].Set( 1, 0 );
				dy[0,m_size-1].Set( 1, 0 );
				dy[0,0].Set( -1, 0 );
			}

			// Apply forward Fourier transform to obtain spectrum
			SharpMath.FFT.FFT2D_GPU	FFT = new SharpMath.FFT.FFT2D_GPU( m_device, m_size );
			Complex[,]	NX = FFT.FFT_Forward( nx );
			Complex[,]	NY = FFT.FFT_Forward( ny );
			Complex[,]	DX = FFT.FFT_Forward( dx );
			Complex[,]	DY = FFT.FFT_Forward( dy );

			// Compute de-convolution
			float		factor = m_imageNormal.Width * m_imageNormal.Height;
			Complex[,]	H = new Complex[m_size,m_size];
			Complex		temp = new Complex(), sqrtTemp;
			for ( uint Y=0; Y < m_size; Y++ ) {
				for ( uint X=0; X < m_size; X++ ) {
#if !FUCK
					double	sqNX = NX[X,Y].SquareMagnitude;
					double	sqNY = NY[X,Y].SquareMagnitude;
					double	num = sqNX + sqNY;

					double	sqDX = DX[X,Y].SquareMagnitude;
					double	sqDY = DY[X,Y].SquareMagnitude;
					double	den = factor * (sqDX + sqDY);

					temp.Set( Math.Abs( den ) > 0.0 ? num / den : 0.0, 0.0 );
					sqrtTemp = temp.Sqrt();

					H[X,Y] = sqrtTemp;
#else
					Complex	Nx = NX[X,Y];
					Complex	Ny = NY[X,Y];
					Complex	Dx = DX[X,Y];
					Complex	Dy = DY[X,Y];
					double	den = factor * (DX[X,Y].SquareMagnitude + DY[X,Y].SquareMagnitude);
					if ( Math.Abs( den ) > 0.0 ) {
						temp = -(Dx * Nx + Dy * Ny) / den;
						H[X,Y] = temp;
					} else {
						H[X,Y].Zero();
					}
#endif
				}
			}

			// Apply inverse transform
			Complex[,]	h = FFT.FFT_Inverse( H );

			Complex	heightMin = new Complex( double.MaxValue, double.MaxValue );
			Complex	heightMax = new Complex( -double.MaxValue, -double.MaxValue );
			for ( uint Y=0; Y < m_size; Y++ ) {
				for ( uint X=0; X < m_size; X++ ) {
					Complex	height = h[X,Y];
					heightMin.Min( height );
					heightMax.Max( height );
				}
			}

			// Render result
			if ( m_imageHeight != null ) {
				m_imageHeight.Dispose();
				m_imageHeight = null;
			}

			factor = 1.0f / (float) (heightMax.r - heightMin.r);

			m_imageHeight = new ImageFile( m_imageNormal.Width, m_imageNormal.Height, ImageFile.PIXEL_FORMAT.R16, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
//			m_imageHeight.WritePixels( ( uint X, uint Y, ref float4 _color ) => { _color.x = 1e3f * (float) H[X,Y].Magnitude; } );
//			m_imageHeight.WritePixels( ( uint X, uint Y, ref float4 _color ) => { _color.x = 1e5f * (float) DX[X,Y].Magnitude; } );
 			m_imageHeight.WritePixels( ( uint X, uint Y, ref float4 _color ) => { _color.x = factor * (float) (h[X,Y].r - heightMin.r); } );

			imagePanelHeight.Bitmap = m_imageHeight.AsBitmap;

			FFT.Dispose();
		}

		#endregion

		private void imagePanelNormal_Click(object sender, EventArgs e) {
			openFileDialog.FileName = GetRegKey( "NormalMapFileName", "MyNormalMap.png" );
			if ( openFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;
			SetRegKey( "NormalMapFileName", openFileDialog.FileName );

			LoadNormalMap( new System.IO.FileInfo( openFileDialog.FileName ) );
		}

		private void buttonConvert_Click(object sender, EventArgs e) {
			try {
				Convert( true );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred during conversion:\r\n\r\n" + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		private void buttonConvertOneSided_Click( object sender, EventArgs e ) {
			try {
				Convert( false );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred during conversion:\r\n\r\n" + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		private void imagePanelHeight_Click( object sender, EventArgs e ) {
			if ( m_imageHeight == null ) {
				MessageBox( "No conversion was computed... Nothing to save yet!", MessageBoxButtons.OK, MessageBoxIcon.Information );
				return;
			}

			saveFileDialog.FileName = GetRegKey( "HeightMapFileName", "MyHeightMap.png" );
			if ( saveFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;
			SetRegKey( "HeightMapFileName", saveFileDialog.FileName );

			m_imageHeight.Save( new System.IO.FileInfo( saveFileDialog.FileName ) );
		}
	}
}
