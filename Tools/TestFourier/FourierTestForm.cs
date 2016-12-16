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

namespace TestFourier
{
	public partial class FourierTestForm : Form {
		ImageFile		m_image = null;

		float[]			m_signal = null;
		float2[]		m_spectrum = null;


		public FourierTestForm() {
			InitializeComponent();
		}
		
		void	TestTransform() {

			// Build the input signal
			m_signal = new float[1024];
			for ( int i=0; i < 1024; i++ ) {
				m_signal[i] = (float) Math.Cos( 2.0 * Math.PI * i / 1024 );
			}

			// Transform
			m_spectrum = new float2[m_signal.Length >> 1];
			for ( int frequencyIndex=0; frequencyIndex < m_spectrum.Length; frequencyIndex++ ) {
				double	frequency = 2.0 * Math.PI * frequencyIndex;
				double	sumR = 0.0;
				double	sumI = 0.0;
				for ( int i=0; i < 1024; i++ ) {

				}
				m_spectrum[frequencyIndex].Set( (float) sumR, (float) sumI );
			}
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			TestTransform();

			m_image = new ImageFile( (uint) imagePanel.Width, (uint) imagePanel.Height, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			m_image.Clear( float4.One );

			float2	rangeX = new float2( 0.0f, 1024.0f );
			float2	rangeY = new float2();
			m_image.PlotGraphAutoRangeY( float4.Zero, rangeX, ref rangeY, ( float x ) => {
				int		X = Math.Max( 0, Math.Min( 1023, (int) x ) );
				return m_signal[X];
			} );
			m_image.PlotAxes( float4.Zero, rangeX, rangeY, 16.0f, 0.1f );

			float2	cornerMin = m_image.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( 0.0f, -1.0f ) );
			float2	cornerMax = m_image.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( rangeX.x, +1.0f ) );
			float2	delta = cornerMax - cornerMin;
			float	zeroY = cornerMin.y + 0.5f * delta.y;

			float2	Xr0 = new float2( 0, zeroY );
			float2	Xr1 = new float2( 0, 0 );
			float2	Xi0 = new float2( 0, zeroY );
			float2	Xi1 = new float2( 0, 0 );

			float4	spectrumColorRe = new float4( 1, 0.25f, 0, 1 );
			float4	spectrumColorIm = new float4( 0, 0.5f, 1, 1 );
			for ( int i=0; i < m_spectrum.Length; i++ ) {
				float	X = cornerMin.x + i * delta.x;
				Xr0.x = X;
				Xr1.x = X;
				Xr1.y = cornerMin.y + 0.5f * (m_spectrum[i].x + 1.0f) * delta.y;
				Xi0.x = X+1;
				Xi1.x = X+1;
				Xi1.y = cornerMin.y + 0.5f * (m_spectrum[i].y + 1.0f) * delta.y;

				m_image.DrawLine( spectrumColorRe, Xr0, Xr1 );
				m_image.DrawLine( spectrumColorIm, Xi0, Xi1 );
			}

			imagePanel.Bitmap = m_image.AsBitmap;
		}
	}
}
