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
		float4			m_black = float4.UnitW;

		ImageFile		m_image = null;

		double[]		m_signal = new double[1024];
		float2[]		m_spectrum = null;


		public FourierTestForm() {
			InitializeComponent();
		}
		
		void	TestTransform( double _time ) {
			// Build the input signal
			for ( int i=0; i < 1024; i++ ) {
//				m_signal[i] = Math.Cos( 2.0 * Math.PI * i / 1024 + _time );
//				m_signal[i] = Math.Cos( (40.0 * (1.0 + Math.Sin( _time ))) * 2.0 * Math.PI * i / 1024 );
				m_signal[i] = (i + 50.0 * _time) % 1024 < 512 ? 1 : 0;
//				m_signal[i] = SimpleRNG.GetUniform();
			}

			// Transform
			m_spectrum = new float2[m_signal.Length >> 1];
			for ( int frequencyIndex=0; frequencyIndex < m_spectrum.Length; frequencyIndex++ ) {
				double	frequency = 2.0 * Math.PI * frequencyIndex;
				double	sumR = 0.0;
				double	sumI = 0.0;
				for ( int i=0; i < 1024; i++ ) {
					double	omega = -frequency * i / 1024.0;
					double	c = Math.Cos( omega );
					double	s = Math.Sin( omega );
					double	v = m_signal[i];
					sumR += c * v;
					sumI += s * v;
				}
				sumR /= 512.0;
				sumI /= 512.0;

// sumR = 1.0;
// sumI = -1.0;

				m_spectrum[frequencyIndex].Set( (float) sumR, (float) sumI );
			}
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			UpdateGraph();

			Application.Idle += Application_Idle;
		}

		void Application_Idle( object sender, EventArgs e ) {
			UpdateGraph();
		}

		DateTime	m_startTime = DateTime.Now;
		void	UpdateGraph() {

			TestTransform( (DateTime.Now - m_startTime).TotalSeconds );

			m_image = new ImageFile( (uint) imagePanel.Width, (uint) imagePanel.Height, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			m_image.Clear( float4.One );

			float2	rangeX = new float2( 0.0f, 1024.0f );
			float2	rangeY = new float2( -1, 1 );
//			m_image.PlotGraphAutoRangeY( m_black, rangeX, ref rangeY, ( float x ) => {
			m_image.PlotGraph( m_black, rangeX, rangeY, ( float x ) => {
				int		X = Math.Max( 0, Math.Min( 1023, (int) x ) );
				return (float) m_signal[X];
			} );
			m_image.PlotAxes( m_black, rangeX, rangeY, 16.0f, 0.1f );

			float2	cornerMin = m_image.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( rangeX.x, -1.0f ) );
			float2	cornerMax = m_image.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( rangeX.y, +1.0f ) );
			float2	delta = cornerMax - cornerMin;
			float	zeroY = cornerMin.y + 0.5f * delta.y;

			float2	Xr0 = new float2( 0, zeroY );
			float2	Xr1 = new float2( 0, 0 );
			float2	Xi0 = new float2( 0, zeroY );
			float2	Xi1 = new float2( 0, 0 );

			float	scale = 10.0f;

			float4	spectrumColorRe = new float4( 1, 0.25f, 0, 1 );
			float4	spectrumColorIm = new float4( 0, 0.5f, 1, 1 );
			for ( int i=0; i < m_spectrum.Length; i++ ) {
				float	X = cornerMin.x + i * delta.x / m_spectrum.Length;
				Xr0.x = X;
				Xr1.x = X;
				Xr1.y = cornerMin.y + 0.5f * (scale * m_spectrum[i].x + 1.0f) * delta.y;
				Xi0.x = X+1;
				Xi1.x = X+1;
				Xi1.y = cornerMin.y + 0.5f * (scale * m_spectrum[i].y + 1.0f) * delta.y;

				m_image.DrawLine( spectrumColorRe, Xr0, Xr1 );
				m_image.DrawLine( spectrumColorIm, Xi0, Xi1 );
			}

			imagePanel.Bitmap = m_image.AsBitmap;
		}
	}
}
