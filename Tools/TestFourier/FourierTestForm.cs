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
		float4			m_red = new float4( 1, 0, 0, 1 );
		float4			m_blue = new float4( 0, 0, 1, 1 );

		ImageFile		m_image = null;



		public FourierTestForm() {
			InitializeComponent();
		}
		
		Complex[]		m_signalSource = new Complex[1024];
		Complex[]		m_spectrum = new Complex[1024];
		Complex[]		m_signalReconstructed = new Complex[1024];

		void	TestTransform( double _time ) {
			// Build the input signal
			Array.Clear( m_signalSource, 0, 1024 );
			for ( int i=0; i < 1024; i++ ) {
//				m_signalSource[i].r = Math.Cos( 2.0 * Math.PI * i / 1024 + _time );
//				m_signalSource[i].r = Math.Cos( (4.0 * (1.0 + Math.Sin( _time ))) * 2.0 * Math.PI * i / 1024 );
				m_signalSource[i].r = 0.5 * Math.Sin( _time ) + ((i + 50.0 * _time) % 512 < 256 ? 0.5 : -0.5);
//				m_signalSource[i].r = Math.Sin( (4.0 * (1.0 + Math.Sin( _time ))) * 2.0 * Math.PI * (1+i) / 1024 ) / ((4.0 * (1.0 + Math.Sin( _time ))) * 2.0 * Math.PI * (1+i) / 1024);
//				m_signalSource[i].r = SimpleRNG.GetUniform();
			}

			// Transform
			SharpMath.FFT.DFT1D.DFT_Forward( m_signalSource, m_spectrum );

/*			// Transform
//			double	normalizer = Math.Sqrt( 1.0 / m_signalSource.Length );
			double	normalizer = 1.0 / m_signalSource.Length;
			for ( int frequencyIndex=0; frequencyIndex < 1024; frequencyIndex++ ) {
				double	frequency = 2.0 * Math.PI * (frequencyIndex - 512);
				double	sumR = 0.0;
				double	sumI = 0.0;
				for ( int i=0; i < 1024; i++ ) {
					double	omega = -frequency * i / 1024.0;	// Notice the - sign here!
					double	c = Math.Cos( omega );
					double	s = Math.Sin( omega );
					Complex	v = m_signalSource[i];
					sumR += c * v.r - s * v.i;
					sumI += s * v.r + c * v.i;
				}
				sumR *= normalizer;
				sumI *= normalizer;

				m_spectrum[frequencyIndex].Set( (float) sumR, (float) sumI );
			}
//*/
/*			// Reconstruct signal
			Array.Clear( m_signalReconstructed, 0, m_signalReconstructed.Length );
			for ( int frequencyIndex=0; frequencyIndex < m_spectrum.Length; frequencyIndex++ ) {
				double	frequency = 2.0 * Math.PI * (frequencyIndex - 512);
				Complex	v = m_spectrum[frequencyIndex];
				for ( int i=0; i < 1024; i++ ) {
					double	omega = frequency * i / 1024.0;	// Notice the + sign here!
					double	c = Math.Cos( omega );
					double	s = Math.Sin( omega );
					m_signalReconstructed[i].r += (float) (c * v.r - s * v.i);
					m_signalReconstructed[i].i += (float) (s * v.r + c * v.i);
				}
			}
//*/

			// Inverse Transform
			SharpMath.FFT.DFT1D.DFT_Inverse( m_spectrum, m_signalReconstructed );
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			m_image = new ImageFile( (uint) imagePanel.Width, (uint) imagePanel.Height, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );

			UpdateGraph();

			Application.Idle += Application_Idle;
		}

		void Application_Idle( object sender, EventArgs e ) {
			UpdateGraph();
		}

		DateTime	m_startTime = DateTime.Now;
		void	UpdateGraph() {

			double	time = (DateTime.Now - m_startTime).TotalSeconds;

			TestTransform( time );

			m_image.Clear( float4.One );

			float2	rangeX = new float2( 0.0f, 1024.0f );
			float2	rangeY = new float2( -1, 1 );

			// Plot input signal
//			m_image.PlotGraphAutoRangeY( m_black, rangeX, ref rangeY, ( float x ) => {
			m_image.PlotGraph( m_black, rangeX, rangeY, ( float x ) => {
				int		X = Math.Max( 0, Math.Min( 1023, (int) x ) );
				return (float) m_signalSource[X].r;
			} );

			// Plot reconstructed signals (Real and Imaginary parts)
			m_image.PlotGraph( m_red, rangeX, rangeY, ( float x ) => {
				int		X = Math.Max( 0, Math.Min( 1023, (int) x ) );
				return (float) m_signalReconstructed[X].r;
			} );
			m_image.PlotGraph( m_blue, rangeX, rangeY, ( float x ) => {
				int		X = Math.Max( 0, Math.Min( 1023, (int) x ) );
				return (float) m_signalReconstructed[X].i;
			} );
			m_image.PlotAxes( m_black, rangeX, rangeY, 16.0f, 0.1f );

			//////////////////////////////////////////////////////////////////////////
			// Render spectrum as (Real=Red, Imaginary=Blue) vertical lines for each frequency
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
				Xr1.y = cornerMin.y + 0.5f * (scale * (float) m_spectrum[i].r + 1.0f) * delta.y;
				Xi0.x = X+1;
				Xi1.x = X+1;
				Xi1.y = cornerMin.y + 0.5f * (scale * (float) m_spectrum[i].i + 1.0f) * delta.y;

				m_image.DrawLine( spectrumColorRe, Xr0, Xr1 );
				m_image.DrawLine( spectrumColorIm, Xi0, Xi1 );
			}

			imagePanel.Bitmap = m_image.AsBitmap;
		}
	}
}
