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
		ImageFile				m_handMadeSpectrum;
		ImageFile				m_handMadeBlueNoise;
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

		Complex[]	m_radialSliceAverage;
		uint[]		m_radialSliceAverageCounters;
		Complex[]	m_radialSliceAverage_Smoothed;
		float4[]	m_scanline;

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );
			m_blueNoise = new ImageFile( new System.IO.FileInfo( @"Images\Data\512_512\LDR_LLL1_0.png" ) );
//			m_blueNoise = new ImageFile( new System.IO.FileInfo( @"Images\BlueNoiseSpectrumCorners256.png" ) );

			uint	size = m_blueNoise.Width;
			uint	halfSize = size >> 1;

			try {
				m_device.Init( panelImageSpectrum.Handle, false, false );
				m_FFT = new SharpMath.FFT.FFT2D_GPU( m_device, (int) size );
			} catch ( Exception ) {
			}

			if ( m_FFT == null ) {
				return;
			}

			m_blueNoiseSpectrum = new ImageFile( m_blueNoise.Width, m_blueNoise.Height, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			m_handMadeSpectrum = new ImageFile( m_blueNoise.Width, m_blueNoise.Height, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			m_handMadeBlueNoise = new ImageFile( m_blueNoise.Width, m_blueNoise.Height, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			m_scanline = new float4[m_blueNoise.Width];

			//////////////////////////////////////////////////////////////////////////
			// Apply FFT to blue noise
			Complex[,]	input = new Complex[m_blueNoise.Width,m_blueNoise.Height];
			Complex[,]	output = new Complex[m_blueNoise.Width,m_blueNoise.Height];
			for ( uint Y=0; Y < m_blueNoise.Height; Y++ ) {
				m_blueNoise.ReadScanline( Y, m_scanline );
				for ( uint X=0; X < m_blueNoise.Width; X++ )
					input[X,Y].Set( m_scanline[X].x, 0 );
			}
			m_FFT.FFT_Forward( input, output );

			//////////////////////////////////////////////////////////////////////////
			// Build the radial slice average
			m_radialSliceAverage = new Complex[halfSize];
			m_radialSliceAverageCounters = new uint[halfSize];
			for ( uint Y=0; Y < m_blueNoise.Height; Y++ ) {
				uint	Yoff = (Y + halfSize) & (size-1);
				int		Yrel = (int) Y - (int) halfSize;
				for ( uint X=0; X < m_blueNoise.Width; X++ ) {
					uint	Xoff = (X + halfSize) & (size-1);
					int		Xrel = (int) X - (int) halfSize;
					int		sqRadius = Xrel*Xrel + Yrel*Yrel;
					if ( sqRadius > (halfSize-1)*(halfSize-1) )
						continue;

					int		radius = (int) Math.Floor( Math.Sqrt( sqRadius ) );
//					m_radialSliceAverage[radius] += output[Xoff,Yoff];
					m_radialSliceAverage[radius].r += Math.Abs( output[Xoff,Yoff].r );
					m_radialSliceAverage[radius].i += Math.Abs( output[Xoff,Yoff].i );
					m_radialSliceAverageCounters[radius]++;
				}
			}
			for ( int i=0; i < halfSize; i++ ) {
//				m_radialSliceAverage[i].r = Math.Abs( m_radialSliceAverage[i].r );
				m_radialSliceAverage[i] *= m_radialSliceAverageCounters[i] > 0 ? 1.0f / m_radialSliceAverageCounters[i] : 1.0f;
			}
			double	minAverage = double.MaxValue;
			double	maxAverage = double.MinValue;
 			for ( int i=0; i < halfSize; i++ ) {
				if ( m_radialSliceAverage[i].r < 1e-12 )
					m_radialSliceAverage[i].r = 1e-12;
				if ( m_radialSliceAverage[i].i < 1e-12 )
					m_radialSliceAverage[i].i = 1e-12;

				if ( i > 1 ) {
					minAverage = Math.Min( minAverage, m_radialSliceAverage[i].r );
					maxAverage = Math.Max( maxAverage, m_radialSliceAverage[i].r );
	 			}
 			}

			// Write it to disk
			using ( System.IO.FileStream S = new System.IO.FileInfo( "BlueNoiseRadialProfile255.complex" ).Create() )
				using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) ) {
					for ( int i=1; i < m_radialSliceAverage.Length; i++ ) {
						W.Write( m_radialSliceAverage[i].r );
						W.Write( m_radialSliceAverage[i].i );
					}
				}

			// Smooth it out
			m_radialSliceAverage_Smoothed = new Complex[halfSize];
			Complex	average = new Complex();
 			for ( int i=1; i < halfSize; i++ ) {
				average.Zero();
				for ( int j=-4; j <= 4; j++ ) {
					average += m_radialSliceAverage[Math.Max( 1, Math.Min( halfSize-1, i + j ))];
				}
				m_radialSliceAverage_Smoothed[i] = average / 9.0;
			}

			//////////////////////////////////////////////////////////////////////////
			// Build the images

			// Initial Blue Noise Spectrum
			for ( uint Y=0; Y < m_blueNoiseSpectrum.Height; Y++ ) {
				uint	Yoff = (Y + halfSize) & (size-1);

				// Initial blue noise spectrum
				for ( uint X=0; X < m_blueNoiseSpectrum.Width; X++ ) {
					uint	Xoff = (X + halfSize) & (size-1);

					float	R = (float) output[Xoff,Yoff].r;
					float	I = (float) output[Xoff,Yoff].i;

					R *= 500.0f;
					I *= 500.0f;
					R = Math.Abs( R );

					m_scanline[X].Set( R, R, R, 1.0f );
				}
				m_blueNoiseSpectrum.WriteScanline( Y, m_scanline );
			}

			// Average radial slice
			m_spectrumRadialSlice = new ImageFile( m_blueNoise.Width, m_blueNoise.Height, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			m_spectrumRadialSlice.Clear( float4.One );
			float2	rangeX = new float2( 0, halfSize );
//			float2	rangeY = new float2( -8, 0 );
//				m_spectrumRadialSlice.PlotLogGraphAutoRangeY( float4.UnitW, rangeX, ref rangeY, ( float _x ) => {
// 			m_spectrumRadialSlice.PlotLogGraph( new float4( 0, 0, 0, 1 ), rangeX, rangeY, ( float _x ) => {
//  				return (float) m_radialSliceAverage[(int) Math.Floor( _x )].r;
//  			}, -1.0f, 10.0f );
// 
// 			m_spectrumRadialSlice.PlotLogGraph( new float4( 0, 0.5f, 0, 1 ), rangeX, rangeY, ( float _x ) => {
//  				return (float) Math.Pow( 10.0, RadialProfile( _x / halfSize ) );
//  			}, -1.0f, 10.0f );
// 			m_spectrumRadialSlice.PlotLogGraph( new float4( 0.25f, 0.25f, 0.25f, 1 ), rangeX, rangeY, ( float _x ) => { return (float) m_radialSliceAverage[(int) Math.Floor( _x )].i; }, -1.0f, 10.0f );
// 			m_spectrumRadialSlice.PlotLogGraph( new float4( 1, 0, 0, 1 ), rangeX, rangeY, ( float _x ) => { return (float) m_radialSliceAverage_Smoothed[(int) Math.Floor( _x )].r; }, -1.0f, 10.0f );
// 			m_spectrumRadialSlice.PlotLogGraph( new float4( 0, 0, 1, 1 ), rangeX, rangeY, ( float _x ) => { return (float) m_radialSliceAverage_Smoothed[(int) Math.Floor( _x )].i; }, -1.0f, 10.0f );
// 			m_spectrumRadialSlice.PlotLogAxes( float4.UnitW, rangeX, rangeY, -16.0f, 10.0f );

			float2	rangeY = new float2( 0, 0.0005f );
			m_spectrumRadialSlice.PlotGraph( new float4( 0, 0, 0, 1 ), rangeX, rangeY, ( float _x ) => {
 				return (float) m_radialSliceAverage[(int) Math.Floor( _x )].r;
 			} );

			m_spectrumRadialSlice.PlotGraph( new float4( 0, 0.5f, 0, 1 ), rangeX, rangeY, ( float _x ) => {
 				return (float) Math.Pow( 10.0, RadialProfile( _x / halfSize ) );
 			} );

			m_spectrumRadialSlice.PlotGraph( new float4( 0.25f, 0.25f, 0.25f, 1 ), rangeX, rangeY, ( float _x ) => { return (float) m_radialSliceAverage[(int) Math.Floor( _x )].i; } );
			m_spectrumRadialSlice.PlotGraph( new float4( 1, 0, 0, 1 ), rangeX, rangeY, ( float _x ) => { return (float) m_radialSliceAverage_Smoothed[(int) Math.Floor( _x )].r; } );
			m_spectrumRadialSlice.PlotGraph( new float4( 0, 0, 1, 1 ), rangeX, rangeY, ( float _x ) => { return (float) m_radialSliceAverage_Smoothed[(int) Math.Floor( _x )].i; } );

			m_spectrumRadialSlice.PlotAxes( float4.UnitW, rangeX, rangeY, 16.0f, 1e-4f );

			//////////////////////////////////////////////////////////////////////////
			// Build initial blue-noise
			RebuildNoise();
		}

		void	UpdatePanels() {
			if ( m_displayType ) {
				panelImage.Bitmap = m_spectrumRadialSlice.AsBitmap;
			} else {
				panelImage.Bitmap = m_displayHandMadeSpectrum ? m_handMadeBlueNoise.AsBitmap : m_blueNoise.AsBitmap;
			}
			if ( m_displayHandMadeSpectrum ) {
				panelImageSpectrum.Bitmap = m_handMadeSpectrum.AsBitmap;
			} else {
				panelImageSpectrum.Bitmap = m_blueNoiseSpectrum.AsBitmap;
			}
// 			panelImage.Bitmap = m_blueNoise.AsBitmap;
// 			panelImageSpectrum.Bitmap = m_blueNoiseSpectrum.AsBitmap;
		}

		/// <summary>
		/// This piecewise-continuous analytical function was fitted using Mathematica:
		///		• The left and right parts are straight half-lines
		///		• The middle part is a 3rd order polynomial joining the left & right half-lines together
		/// </summary>
		/// <param name="_radius"></param>
		/// <returns></returns>
		double		RadialProfile( double _radius ) {
//			double	x = (_radius - 0.4313725490) * 4.25;	// Rescale so the curved part of the profile is now in [0,1]
// 			const double	a = -4.28309;
// 			const double	b = 0.980825;
// 			const double	c = 0.67974;
// 			const double	d = -0.787163;
			double	x = (_radius - 0.4313725490) * 4.636363636;
			const double	a = -4.28309;
			const double	b = 0.899089;
			const double	c = 0.829611;
			const double	d = -0.857921;

			// Out of range? Simply estimate a simple straight line...
			double	y;
			if ( x < 0.0 ) {
				const double	y0 = a;
				const double	slope = b;
				y = y0 + slope * x;
			} else if ( x > 1.0 ) {
				const double	y0 = a+b+c+d;
				const double	slope = b+2*c+3*d;
				y = y0 + slope * (x-1.0);
			} else {
				// Estimate a regular 3rd order polynomial
				y = a + x * (b + x * (c + x * d));
			}
			return y;
		}

		void	RebuildNoise() {
			uint	size = m_blueNoise.Width;
			uint	halfSize = size >> 1;

			double	noiseScale = floatTrackbarControlScale.Value;
			double	noiseBias = floatTrackbarControlOffset.Value;
			double	radialOffset = floatTrackbarControlRadialOffset.Value;
			double	radialScale = floatTrackbarControlRadialScale.Value;

			//////////////////////////////////////////////////////////////////////////
			// Reconstruct an artificial spectrum
			Complex[,]	handmadeSpectrum = new Complex[m_blueNoise.Width, m_blueNoise.Height];
			Complex		Cnoise = new Complex();
			for ( uint Y=0; Y < m_blueNoise.Height; Y++ ) {
				uint	Yoff = (Y + halfSize) & (size-1);
				int		Yrel = (int) Y - (int) halfSize;
				for ( uint X=0; X < m_blueNoise.Width; X++ ) {
					uint	Xoff = (X + halfSize) & (size-1);
					int		Xrel = (int) X - (int) halfSize;

					// Fetch "center noise" from blue noise
//					Cnoise = output[(halfSize>>1) + (X & (halfSize-1)), (halfSize>>1) + (Y & (halfSize-1))];
//					Cnoise /= maxAverage;	// Center noise is already factored by the maximum average so we "renormalize it"

					// Apply simple uniform noise
					Cnoise.r = noiseScale * (SimpleRNG.GetUniform() - noiseBias);
 					Cnoise.i = noiseScale * (SimpleRNG.GetUniform() - noiseBias);
// 					Cnoise.r = 2.0 * SimpleRNG.GetNormal( 0, 1 ) - 1.0;
// 					Cnoise.i = 2.0 * SimpleRNG.GetNormal( 0, 1 ) - 1.0;

// Cnoise.r = 1.0;
// Cnoise.i = 0.0;

					// Apply weighting by radial profile
					int		sqRadius = Xrel*Xrel + Yrel*Yrel;

					// Use averaged radial profile extracted from the noise texture
//					int		radius = (int) Math.Max( 1, Math.Min( halfSize-1, Math.Sqrt( sqRadius ) ) );
//					double	profileFactor = m_radialSliceAverage_Smoothed[radius].r;
//profileFactor *= 2.0;

					// Use the Mathematica hand-fitted curve
					double	profileFactor = Math.Pow( 10.0, RadialProfile( radialOffset + radialScale * Math.Sqrt( sqRadius ) / halfSize ) );
//profileFactor *= 0.75 / 1;
//profileFactor *= 3.0;

//profileFactor *= Math.Sqrt( 2.0 );
//profileFactor *= 1.1;

					Cnoise *= profileFactor;

//Cnoise = output[Xoff,Yoff];
//Cnoise *= Math.Exp( 0.01 * Math.Max( 0.0, radius - 128 ) );

					handmadeSpectrum[Xoff,Yoff] = Cnoise;
				}
			}
			handmadeSpectrum[0,0].Set( floatTrackbarControlDC.Value, 0.0 );	// Central value for constant term

			//////////////////////////////////////////////////////////////////////////
			// Reconstruct a hand-made "blue noise"
			Complex[,]	handMadeBlueNoise = new Complex[m_blueNoise.Width,m_blueNoise.Height];
			m_FFT.FFT_Inverse( handmadeSpectrum, handMadeBlueNoise );
//			m_FFT.FFT_Inverse( output, handMadeBlueNoise );


			//////////////////////////////////////////////////////////////////////////
			// Build the resulting images
			for ( uint Y=0; Y < m_blueNoiseSpectrum.Height; Y++ ) {
				uint	Yoff = (Y + halfSize) & (size-1);
				for ( uint X=0; X < m_blueNoiseSpectrum.Width; X++ ) {
					uint	Xoff = (X + halfSize) & (size-1);

					float	R = (float) handmadeSpectrum[Xoff,Yoff].r;
					float	I = (float) handmadeSpectrum[Xoff,Yoff].i;

					R *= 500.0f;
					I *= 500.0f;
					R = Math.Abs( R );

					m_scanline[X].Set( R, R, R, 1.0f );
				}
				m_handMadeSpectrum.WriteScanline( Y, m_scanline );
			}

			// Hand-made blue noise
			for ( uint Y=0; Y < m_blueNoise.Height; Y++ ) {
				for ( uint X=0; X < m_blueNoise.Width; X++ ) {
					float	R = (float) handMadeBlueNoise[X,Y].r;
					float	I = (float) handMadeBlueNoise[X,Y].i;
					m_scanline[X].Set( R, R, R, 1.0f );
				}
				m_handMadeBlueNoise.WriteScanline( Y, m_scanline );
			}

			UpdatePanels();
		}

		bool	m_displayType = false;
		private void panelImage_Click( object sender, EventArgs e ) {
			m_displayType = !m_displayType;
			UpdatePanels();
		}

		bool	m_displayHandMadeSpectrum = false;
		private void panelImageSpectrum_Click( object sender, EventArgs e ) {
			m_displayHandMadeSpectrum = !m_displayHandMadeSpectrum;
			UpdatePanels();
		}

		private void floatTrackbarControlScale_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			RebuildNoise();
		}

		private void floatTrackbarControlOffset_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			RebuildNoise();
		}
	}
}
