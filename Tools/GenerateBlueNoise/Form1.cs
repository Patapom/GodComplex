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

namespace GenerateBlueNoise
{
	public partial class GenerateBlueNoiseForm : Form {

		ImageFile				m_blueNoise;
		ImageFile				m_blueNoiseSpectrum;
		ImageFile				m_spectrumRadialSlice;

		Renderer.Device			m_device = new Renderer.Device();
		SharpMath.FFT.FFT2D_GPU	m_FFT;

		public GenerateBlueNoiseForm() {
			InitializeComponent();
		}

		protected override void OnClosed( EventArgs e ) {
			base.OnClosed( e );

			m_FFT.Dispose();
			m_device.Dispose();
		}

		Complex[]	m_radialSliceHistogram;
		uint[]		m_radialSliceHistogramCounters;

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );
			m_blueNoise = new ImageFile( new System.IO.FileInfo( @"Images\Data\512_512\LDR_LLL1_0.png" ) );
//			m_blueNoise = new ImageFile( new System.IO.FileInfo( @"Images\BlueNoiseSpectrumCorners256.png" ) );
			panelImage.Bitmap = m_blueNoise.AsBitmap;

			uint	size = m_blueNoise.Width;
			uint	halfSize = size >> 1;

			try {
				m_device.Init( panelImageSpectrum.Handle, false, false );
				m_FFT = new SharpMath.FFT.FFT2D_GPU( m_device, (int) size );
			} catch ( Exception ) {
			}

			if ( m_FFT != null ) {
				Complex[,]	input = new Complex[m_blueNoise.Width,m_blueNoise.Height];
				Complex[,]	output = new Complex[m_blueNoise.Width,m_blueNoise.Height];
				float4[]	scanline = new float4[m_blueNoise.Width];
				for ( uint Y=0; Y < m_blueNoise.Height; Y++ ) {
					m_blueNoise.ReadScanline( Y, scanline );
					for ( uint X=0; X < m_blueNoise.Width; X++ )
						input[X,Y].Set( scanline[X].x, 0 );
				}
				m_FFT.FFT_Forward( input, output );

				m_blueNoiseSpectrum = new ImageFile( m_blueNoise.Width, m_blueNoise.Height, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
				for ( uint Y=0; Y < m_blueNoiseSpectrum.Height; Y++ ) {
					uint	Yoff = (Y + halfSize) & (size-1);
					for ( uint X=0; X < m_blueNoiseSpectrum.Width; X++ ) {
						uint	Xoff = (X + halfSize) & (size-1);
						float	R = (float) output[Xoff,Yoff].r;
						float	I = (float) output[Xoff,Yoff].i;

						R *= 500.0f;
						I *= 500.0f;
						R = Math.Abs( R );

						scanline[X].Set( R, R, R, 1.0f );
					}
					m_blueNoiseSpectrum.WriteScanline( Y, scanline );
				}
				panelImageSpectrum.Bitmap = m_blueNoiseSpectrum.AsBitmap;

				// Build the radial slice histogram
				m_radialSliceHistogram = new Complex[halfSize];
				m_radialSliceHistogramCounters = new uint[halfSize];
				for ( uint Y=0; Y < m_blueNoiseSpectrum.Height; Y++ ) {
					uint	Yoff = (Y + halfSize) & (size-1);
					int		Yrel = (int) Y - (int) halfSize;
					for ( uint X=0; X < m_blueNoiseSpectrum.Width; X++ ) {
						uint	Xoff = (X + halfSize) & (size-1);
						int		Xrel = (int) X - (int) halfSize;
						int		sqRadius = Xrel*Xrel + Yrel*Yrel;
						if ( sqRadius > (halfSize-1)*(halfSize-1) )
							continue;

// 						float	R = (float) output[Xoff,Yoff].r;
// 						float	I = (float) output[Xoff,Yoff].i;

						int		radius = (int) Math.Floor( Math.Sqrt( sqRadius ) );
						m_radialSliceHistogram[radius] += output[Xoff,Yoff];
// 						m_radialSliceHistogram[radius].r += Math.Abs( output[Xoff,Yoff].r );
// 						m_radialSliceHistogram[radius].i += Math.Abs( output[Xoff,Yoff].i );
						m_radialSliceHistogramCounters[radius]++;
					}
				}
				for ( int i=0; i < halfSize; i++ ) {
					m_radialSliceHistogram[i].r = Math.Abs( m_radialSliceHistogram[i].r );
					m_radialSliceHistogram[i] *= m_radialSliceHistogramCounters[i] > 0 ? 1.0f / m_radialSliceHistogramCounters[i] : 1.0f;
				}
				double	min = double.MaxValue;
				double	max = double.MinValue;
 				for ( int i=0; i < halfSize; i++ ) {
					if ( m_radialSliceHistogram[i].r < 1e-12 )
						m_radialSliceHistogram[i].r = 1e-6;

					min = Math.Min( min, m_radialSliceHistogram[i].r );
					max = Math.Max( max, m_radialSliceHistogram[i].r );
 				}

				m_spectrumRadialSlice = new ImageFile( m_blueNoise.Width, m_blueNoise.Height, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
				m_spectrumRadialSlice.Clear( float4.One );
				float2	rangeX = new float2( 0, halfSize );
				float2	rangeY = new float2( -8, 0 );
//				m_spectrumRadialSlice.PlotLogGraphAutoRangeY( float4.UnitW, rangeX, ref rangeY, ( float _x ) => {
				m_spectrumRadialSlice.PlotLogGraph( float4.UnitW, rangeX, rangeY, ( float _x ) => {
 					return (float) m_radialSliceHistogram[(int) Math.Floor( _x )].r;
 				}, -1.0f, 10.0f );
				m_spectrumRadialSlice.PlotLogAxes( float4.UnitW, rangeX, rangeY, -16.0f, 10.0f );
			}
		}

		bool	m_displayType = false;
		private void panelImage_Click( object sender, EventArgs e ) {
			m_displayType = !m_displayType;
			if ( m_displayType ) {
				panelImage.Bitmap = m_spectrumRadialSlice.AsBitmap;
			} else {
				panelImage.Bitmap = m_blueNoise.AsBitmap;
			}
		}
	}
}
