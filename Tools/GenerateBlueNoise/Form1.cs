//#define COMPUTE_RADIAL_SLICE
#define SIGMOID_FITTING

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

namespace GenerateBlueNoise
{
	public partial class GenerateBlueNoiseForm : Form {

		Renderer.Device			m_device = new Renderer.Device();
		SharpMath.FFT.FFT2D_GPU	m_FFT = null;

		#if COMPUTE_RADIAL_SLICE

			ImageFile	m_blueNoise;
			ImageFile	m_blueNoiseSpectrum;
			ImageFile	m_spectrumRadialSlice;
			ImageFile	m_spectrumNoiseDistribution;
			ImageFile	m_spectrumNoiseDistribution_Custom;

			Complex[]	m_radialSliceAverage_Smoothed;
			float4[]	m_scanline;
		#endif

		ImageFile	m_handMadeBlueNoise;
		ImageFile	m_handMadeSpectrum;

		public GenerateBlueNoiseForm() {
			InitializeComponent();

// Serious fun
// 			uint[]		testHisto = new uint[100];
// 			for ( uint i=0; i < 1000000; i++ ) {
// 				double	rnd = Math.Sqrt( SimpleRNG.GetUniform() );
// 				uint	binIndex = (uint) Math.Floor( rnd * 100 );
// 				testHisto[binIndex]++;
// 			}
// 
// 			float[]		normalized = new float[100];
// 			float[]		partialSum = new float[100];
// 			float		sum = 0.0f;
// 			for ( uint i=0; i < 100; i++ ) {
// 				float	value = (float) testHisto[i] / 10000.0f;
// 				sum += value;
// 				normalized[i] = value;
// 				partialSum[i] = sum;
// 			}

// Dick fun :D
// 			ImageFile	dick = new ImageFile( new System.IO.FileInfo( "dick.png" ) );
// 			uint[]		maxY = new uint[dick.Width];
// 			dick.ReadPixels( (uint X, uint Y, ref float4 _color ) => {
// 				if ( _color.x < 0.5f && Y > maxY[X] )
// 					maxY[X] = Y;
// 			} );
// 			using ( System.IO.FileStream S = new System.IO.FileInfo( "dickHisto.float" ).Create() )
// 				using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) ) {
// 					for ( int X=0; X < maxY.Length; X++ ) {
// 						W.Write( maxY[X] / 256.0f );
// 					}
// 				}
// 
// 			float[]	CDF = new float[dick.Width];
// 			float	sum = 0.0f;
// 			for ( int X=0; X < maxY.Length; X++ ) {
// 				sum += maxY[X] / 255.0f;
// 				CDF[X] = sum;
// 			}
// 			using ( System.IO.FileStream S = new System.IO.FileInfo( "dickCDF.float" ).Create() )
// 				using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) ) {
// 					for ( int X=0; X < CDF.Length; X++ ) {
// 						float	V = CDF[X];
// 								V /= sum;
// 						W.Write( V );
// 					}
// 				}
// 
//  			// Build inverse CDF
//  			float[]	inverseCDF = new float[dick.Width];
		}

		protected override void OnClosed( EventArgs e ) {
			base.OnClosed( e );

			m_FFT.Dispose();
			m_device.Dispose();
		}


		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );
			try {
				#if COMPUTE_RADIAL_SLICE
					m_blueNoise = new ImageFile( new System.IO.FileInfo( @"Images\Data\512_512\LDR_LLL1_0.png" ) );
//					m_blueNoise = new ImageFile( new System.IO.FileInfo( @"Images\BlueNoiseSpectrumCorners256.png" ) );
//					m_blueNoise = new ImageFile( new System.IO.FileInfo( @"BlueNoise512x512_VoidAndCluster.png" ) );
//					m_blueNoise = new ImageFile( new System.IO.FileInfo( @"BlueNoise256x256_VoidAndCluster.png" ) );

					uint	size = m_blueNoise.Width;
					uint	halfSize = size >> 1;

					try {
						m_device.Init( panelImageSpectrum.Handle, false, false );
						m_FFT = new SharpMath.FFT.FFT2D_GPU( m_device, size );
					} catch ( Exception ) {
					}

					if ( m_FFT == null ) {
						return;
					}

					m_blueNoiseSpectrum = new ImageFile( m_blueNoise.Width, m_blueNoise.Height, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
					m_handMadeBlueNoise = new ImageFile( m_blueNoise.Width, m_blueNoise.Height, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
					m_handMadeSpectrum = new ImageFile( m_blueNoise.Width, m_blueNoise.Height, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
					m_scanline = new float4[m_blueNoise.Width];

					//////////////////////////////////////////////////////////////////////////
					// Apply FFT to blue noise
					Complex[,]	input = new Complex[m_blueNoise.Width,m_blueNoise.Height];
					Complex[,]	output = new Complex[m_blueNoise.Width,m_blueNoise.Height];
					m_blueNoise.ReadPixels( ( uint X, uint Y, ref float4 _color ) => { input[X,Y].Set( _color.x, 0 ); } );
// 					for ( uint Y=0; Y < m_blueNoise.Height; Y++ ) {
// 						m_blueNoise.ReadScanline( Y, m_scanline );
// 						for ( uint X=0; X < m_blueNoise.Width; X++ )
// 							input[X,Y].Set( m_scanline[X].x, 0 );
// 					}
					m_FFT.FFT_Forward( input, output );

					//////////////////////////////////////////////////////////////////////////
					// Build the radial slice average
					Complex[]	radialSliceAverage = new Complex[halfSize];
					uint[]		radialSliceAverageCounters = new uint[halfSize];
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
//							radialSliceAverage[radius] += output[Xoff,Yoff];
							radialSliceAverage[radius].r += Math.Abs( output[Xoff,Yoff].r );
							radialSliceAverage[radius].i += Math.Abs( output[Xoff,Yoff].i );
							radialSliceAverageCounters[radius]++;
						}
					}
					for ( int i=0; i < halfSize; i++ ) {
//						radialSliceAverage[i].r = Math.Abs( radialSliceAverage[i].r );
						radialSliceAverage[i] *= radialSliceAverageCounters[i] > 0 ? 1.0f / radialSliceAverageCounters[i] : 1.0f;
					}
					double	minAverage = double.MaxValue;
					double	maxAverage = double.MinValue;
 					for ( int i=0; i < halfSize; i++ ) {
						if ( radialSliceAverage[i].r < 1e-12 )
							radialSliceAverage[i].r = 1e-12;
						if ( radialSliceAverage[i].i < 1e-12 )
							radialSliceAverage[i].i = 1e-12;

						if ( i > 1 ) {
							minAverage = Math.Min( minAverage, radialSliceAverage[i].r );
							maxAverage = Math.Max( maxAverage, radialSliceAverage[i].r );
	 					}
 					}

					// Write it to disk
					using ( System.IO.FileStream S = new System.IO.FileInfo( "BlueNoiseRadialProfile255.complex" ).Create() )
						using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) ) {
							for ( int i=1; i < radialSliceAverage.Length; i++ ) {
								W.Write( radialSliceAverage[i].r );
								W.Write( radialSliceAverage[i].i );
							}
						}

					// Smooth it out
					m_radialSliceAverage_Smoothed = new Complex[halfSize];
					Complex	average = new Complex();
 					for ( int i=1; i < halfSize; i++ ) {
						average.Zero();
						for ( int j=-4; j <= 4; j++ ) {
							average += radialSliceAverage[Math.Max( 1, Math.Min( halfSize-1, i + j ))];
						}
						m_radialSliceAverage_Smoothed[i] = average / 9.0;
					}


					//////////////////////////////////////////////////////////////////////////
					// Build spectrum noise distribution histogram
					const int BUCKETS_COUNT = 1000;
					uint[]	noiseDistributionHistogram = new uint[BUCKETS_COUNT];
					for ( uint Y=0; Y < m_blueNoise.Height; Y++ ) {
						uint	Yoff = (Y + halfSize) & (size-1);
						int		Yrel = (int) Y - (int) halfSize;
						for ( uint X=0; X < m_blueNoise.Width; X++ ) {
							uint	Xoff = (X + halfSize) & (size-1);
							int		Xrel = (int) X - (int) halfSize;
							int		sqRadius = Xrel*Xrel + Yrel*Yrel;
							double	radius = Math.Sqrt( sqRadius ) / halfSize;
							if ( radius < 0.01f )
								continue;	// Avoid central values because of too much imprecision

							double	random = Math.Abs( output[Xoff,Yoff].r );
							double	profileAmplitude = RadialProfile( radius );
							double	normalizedRandom = random / profileAmplitude;

							int		bucketIndex = Math.Min( BUCKETS_COUNT-1, (int) Math.Floor( normalizedRandom * BUCKETS_COUNT ) );
							noiseDistributionHistogram[bucketIndex]++;
						}
					}

					// Write histogram to disk
					using ( System.IO.FileStream S = new System.IO.FileInfo( "noiseDistribution.float" ).Create() )
						using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) ) {
							for ( int i=0; i < BUCKETS_COUNT; i++ ) {
								W.Write( (float) noiseDistributionHistogram[i] / BUCKETS_COUNT );
							}
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

					// =====================================================
					// Average radial slice
					m_spectrumRadialSlice = new ImageFile( m_blueNoise.Width, m_blueNoise.Height, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
					m_spectrumRadialSlice.Clear( float4.One );
					float2	rangeX = new float2( 0, halfSize );
//					float2	rangeY = new float2( -8, 0 );
//						m_spectrumRadialSlice.PlotLogGraphAutoRangeY( float4.UnitW, rangeX, ref rangeY, ( float _x ) => {
// 					m_spectrumRadialSlice.PlotLogGraph( new float4( 0, 0, 0, 1 ), rangeX, rangeY, ( float _x ) => {
//  						return (float) radialSliceAverage[(int) Math.Floor( _x )].r;
//  					}, -1.0f, 10.0f );
// 
// 					m_spectrumRadialSlice.PlotLogGraph( new float4( 0, 0.5f, 0, 1 ), rangeX, rangeY, ( float _x ) => {
//  						return (float) RadialProfile( _x / halfSize );
//  					}, -1.0f, 10.0f );
// 					m_spectrumRadialSlice.PlotLogGraph( new float4( 0.25f, 0.25f, 0.25f, 1 ), rangeX, rangeY, ( float _x ) => { return (float) radialSliceAverage[(int) Math.Floor( _x )].i; }, -1.0f, 10.0f );
// 					m_spectrumRadialSlice.PlotLogGraph( new float4( 1, 0, 0, 1 ), rangeX, rangeY, ( float _x ) => { return (float) m_radialSliceAverage_Smoothed[(int) Math.Floor( _x )].r; }, -1.0f, 10.0f );
// 					m_spectrumRadialSlice.PlotLogGraph( new float4( 0, 0, 1, 1 ), rangeX, rangeY, ( float _x ) => { return (float) m_radialSliceAverage_Smoothed[(int) Math.Floor( _x )].i; }, -1.0f, 10.0f );
// 					m_spectrumRadialSlice.PlotLogAxes( float4.UnitW, rangeX, rangeY, -16.0f, 10.0f );

					float2	rangeY = new float2( 0, 0.0005f );
					m_spectrumRadialSlice.PlotGraph( new float4( 0, 0, 0, 1 ), rangeX, rangeY, ( float _x ) => {
 						return (float) radialSliceAverage[(int) Math.Floor( _x )].r;
 					} );

					m_spectrumRadialSlice.PlotGraph( new float4( 0, 0.5f, 0, 1 ), rangeX, rangeY, ( float _x ) => {
 						return (float) RadialProfile( _x / halfSize );
 					} );

					m_spectrumRadialSlice.PlotGraph( new float4( 0.25f, 0.25f, 0.25f, 1 ), rangeX, rangeY, ( float _x ) => { return (float) radialSliceAverage[(int) Math.Floor( _x )].i; } );
					m_spectrumRadialSlice.PlotGraph( new float4( 1, 0, 0, 1 ), rangeX, rangeY, ( float _x ) => { return (float) m_radialSliceAverage_Smoothed[(int) Math.Floor( _x )].r; } );
					m_spectrumRadialSlice.PlotGraph( new float4( 0, 0, 1, 1 ), rangeX, rangeY, ( float _x ) => { return (float) m_radialSliceAverage_Smoothed[(int) Math.Floor( _x )].i; } );

					m_spectrumRadialSlice.PlotAxes( float4.UnitW, rangeX, rangeY, 16.0f, 1e-4f );

					// =====================================================
					// Noise distribution
					m_spectrumNoiseDistribution = new ImageFile( m_blueNoise.Width, m_blueNoise.Height, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
					m_spectrumNoiseDistribution_Custom = new ImageFile( m_blueNoise.Width, m_blueNoise.Height, ImageFile.PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
					m_spectrumNoiseDistribution.Clear( float4.One );
					rangeX = new float2( 0, 1 );
					rangeY = new float2( 0, 1.0f / BUCKETS_COUNT );
					m_spectrumNoiseDistribution.PlotGraph( float4.UnitW, rangeX, rangeY, ( float _x ) => {
						int	bucketIndex = Math.Min( BUCKETS_COUNT-1, (int) Math.Floor( _x * BUCKETS_COUNT ) );
 						return (float) noiseDistributionHistogram[bucketIndex] / (m_blueNoise.Width * m_blueNoise.Height);
 					} );

				#else
					// Create our hand made textures
					const uint	NOISE_SIZE = 512;

					try {
						m_device.Init( panelImageSpectrum.Handle, false, false );
						m_FFT = new SharpMath.FFT.FFT2D_GPU( m_device, NOISE_SIZE );
					} catch ( Exception ) {
					}

					m_handMadeBlueNoise = new ImageFile( NOISE_SIZE, NOISE_SIZE, PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
					m_handMadeSpectrum = new ImageFile( m_handMadeBlueNoise.Width, m_handMadeBlueNoise.Height, PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );

					// Read radial profile from disk
					Complex[]	radialSliceAverage = new Complex[256];
					using ( System.IO.FileStream S = new System.IO.FileInfo( "BlueNoiseRadialProfile255.complex" ).OpenRead() )
						using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) ) {
							for ( int i=1; i < radialSliceAverage.Length; i++ ) {
								radialSliceAverage[i].r = R.ReadSingle();
								radialSliceAverage[i].i = R.ReadSingle();
							}
						}
				#endif

				//////////////////////////////////////////////////////////////////////////
				// Build initial blue-noise
				RebuildNoise();

			} catch ( Exception _e ) {
				MessageBox.Show( this, "Error during Load() => " + _e.Message, "BlueNoiseGenerator" );
			}
		}

		void	UpdatePanels() {
			#if COMPUTE_RADIAL_SLICE
				if ( m_displayType ) {
					panelImage.Bitmap = m_spectrumRadialSlice.AsBitmap;
					panelImageSpectrum.Bitmap = checkBoxShowDistribution.Checked ? m_spectrumNoiseDistribution_Custom.AsBitmap : m_spectrumNoiseDistribution.AsBitmap;
				} else {
					panelImage.Bitmap = m_displayHandMadeSpectrum ? m_handMadeBlueNoise.AsBitmap : m_blueNoise.AsBitmap;
					panelImageSpectrum.Bitmap = m_displayHandMadeSpectrum ? m_handMadeSpectrum.AsBitmap : m_blueNoiseSpectrum.AsBitmap;
				}

// 				panelImage.Bitmap = m_blueNoise.AsBitmap;
// 				panelImageSpectrum.Bitmap = m_blueNoiseSpectrum.AsBitmap;
			#else
				panelImage.Bitmap = m_handMadeBlueNoise.AsBitmap;
				panelImageSpectrum.Bitmap = m_handMadeSpectrum.AsBitmap;
			#endif
		}

		/// <summary>
		/// Very wasteful method that computes the spectrum of a source image
		/// </summary>
		/// <param name="_source"></param>
		/// <returns></returns>
		ImageFile	ComputeSpectrum( ImageFile _source, float _scaleFactor ) {
			uint	size = _source.Width;
			uint	offset = size >> 1;
			uint	mask = size - 1;

			Complex[,]	signal = new Complex[size,size];
			_source.ReadPixels( ( uint _X, uint _Y, ref float4 _color ) => {
				signal[_X,_Y].Set( _color.x, 0 );
			} );

			SharpMath.FFT.FFT2D_GPU	FFT = new SharpMath.FFT.FFT2D_GPU( m_device, size );
			Complex[,]	spectrum = FFT.FFT_Forward( signal );
			FFT.Dispose();

			ImageFile	result = new ImageFile( size, size, _source.PixelFormat, _source.ColorProfile );
			result.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
				float	V = _scaleFactor * (float) spectrum[(_X+offset)&mask,(_Y+offset)&mask].r;
				_color.Set( V, V, V, 1.0f );
			} );
			return result;
		}

		#region Poor Spectrum-based Method (doesn't work => Nothing guarantees a proper distribution of values and a correct dithering array)

#if SIGMOID_FITTING
		/// <summary>
		/// This is a modified sigmoïd fitted with Mathematica.
		/// It shows a closer fit than the piecewise-continuous fit below
		/// 
		/// The exact formula is:
		///		model = (a*Exp[g + h*(x + i)]) / (b + c*Exp[d + e*(x^j + f)])
		/// </summary>
		/// <param name="_radius"></param>
		/// <returns></returns>
		double		RadialProfile( double _radius ) {
			const double	a = 0.000385792, b = 0.625297, c = 8.02121, d = 4.08171, e = -20.6229, f = 0.0866656, g = -0.208976, h = -0.283764, i = 0.154578, j = 2.36722;
			double	x = Math.Min( 1.0, _radius );
			double	num = a * Math.Exp( g + h * (x+i) );
			double	den = b + c * Math.Exp( d + e * (Math.Pow( x, j ) + f) );
			double	value = num / den;
			return value;
		}
#else
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
			y = Math.Pow( 10.0, y );
			return y;
		}
#endif

		void	CreateNoiseSpectrum( Complex[,] _spectrum ) {
			uint	size = m_handMadeBlueNoise.Width;
			uint	halfSize = size >> 1;

			double	noiseScale = floatTrackbarControlScale.Value;
			double	noiseBias = floatTrackbarControlOffset.Value;
			double	radialOffset = floatTrackbarControlRadialOffset.Value;
			double	radialScale = floatTrackbarControlRadialScale.Value;
			double	distributionPower = floatTrackbarControlDistributionPower.Value;
			double	sigma = floatTrackbarControlSigma.Value;

			Complex		Cnoise = new Complex();
			for ( uint Y=0; Y < m_handMadeBlueNoise.Height; Y++ ) {
				uint	Yoff = (Y + halfSize) & (size-1);
				int		Yrel = (int) Y - (int) halfSize;
				for ( uint X=0; X < m_handMadeBlueNoise.Width; X++ ) {
					uint	Xoff = (X + halfSize) & (size-1);
					int		Xrel = (int) X - (int) halfSize;

					// Fetch "center noise" from blue noise
//					Cnoise = output[(halfSize>>1) + (X & (halfSize-1)), (halfSize>>1) + (Y & (halfSize-1))];
//					Cnoise /= maxAverage;	// Center noise is already factored by the maximum average so we "renormalize it"

					// Apply simple uniform noise
					Cnoise.r = noiseScale * (Math.Pow( SimpleRNG.GetUniform(), distributionPower ) - noiseBias);
					Cnoise.i = noiseScale * (Math.Pow( SimpleRNG.GetUniform(), distributionPower ) - noiseBias);

// 					Cnoise.r = noiseScale * (SimpleRNG.GetNormal() - noiseBias);
// 					Cnoise.i = noiseScale * (SimpleRNG.GetNormal() - noiseBias);

// 					Cnoise.r = noiseScale * (SimpleRNG.GetLaplace( 0, sigma ) - noiseBias);
// 					Cnoise.i = noiseScale * (SimpleRNG.GetLaplace( 0, sigma ) - noiseBias);

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
					double	profileFactor = RadialProfile( radialOffset + radialScale * Math.Sqrt( sqRadius ) / halfSize );
//profileFactor *= 0.75 / 1;
//profileFactor *= 3.0;

//profileFactor *= Math.Sqrt( 2.0 );
//profileFactor *= 1.1;

					Cnoise *= profileFactor;

//Cnoise = output[Xoff,Yoff];
//Cnoise *= Math.Exp( 0.01 * Math.Max( 0.0, radius - 128 ) );

					_spectrum[Xoff,Yoff] = Cnoise;
				}
			}
			_spectrum[0,0].Set( floatTrackbarControlDC.Value, 0.0 );	// Central value for constant term


			#if COMPUTE_RADIAL_SLICE

				const int BUCKETS_COUNT = 1000;
				uint[]	noiseDistributionHistogram = new uint[BUCKETS_COUNT];
				for ( uint i=0; i < m_handMadeBlueNoise.Width*m_handMadeBlueNoise.Height; i++ ) {
					double	noiseValue = noiseScale * (Math.Pow( SimpleRNG.GetUniform(), distributionPower ) - noiseBias);
					double	noiseValue2 = noiseScale * (Math.Pow( SimpleRNG.GetUniform(), distributionPower ) - noiseBias);
							noiseValue = Math.Abs( noiseValue + noiseValue2 );

noiseValue = Math.Pow( SimpleRNG.GetUniform(), 0.5 );
noiseValue = SimpleRNG.GetUniform() < 0.5 ? 0.5 * Math.Sqrt( 1.0 - noiseValue ) : 1.0 - 0.5 * Math.Sqrt( 1.0 - noiseValue );

					int		bucketIndex = Math.Min( BUCKETS_COUNT-1, (int) Math.Floor( noiseValue * BUCKETS_COUNT ) );
					noiseDistributionHistogram[bucketIndex]++;
				}

// 				for ( uint Y=0; Y < m_handMadeBlueNoise.Height; Y++ ) {
// 					uint	Yoff = (Y + halfSize) & (size-1);
// 					int		Yrel = (int) Y - (int) halfSize;
// 					for ( uint X=0; X < m_handMadeBlueNoise.Width; X++ ) {
// 						uint	Xoff = (X + halfSize) & (size-1);
// 						int		Xrel = (int) X - (int) halfSize;
// 						int		sqRadius = Xrel*Xrel + Yrel*Yrel;
// 						double	radius = Math.Sqrt( sqRadius ) / halfSize;
// 						if ( radius < 0.01f )
// 							continue;	// Avoid central values because of too much imprecision
// 
// 						double	random = Math.Abs( _spectrum[Xoff,Yoff].r );
// 						double	profileAmplitude = RadialProfile( radius );
// 						double	normalizedRandom = random / profileAmplitude;
// 
// 						int		bucketIndex = Math.Min( BUCKETS_COUNT-1, (int) Math.Floor( normalizedRandom * BUCKETS_COUNT ) );
// 						noiseDistributionHistogram[bucketIndex]++;
// 					}
// 				}

				m_spectrumNoiseDistribution_Custom.Clear( float4.One );
				float2	rangeX = new float2( 0, 1 );
				float2	rangeY = new float2( 0, 4.0f / BUCKETS_COUNT );
				m_spectrumNoiseDistribution_Custom.PlotGraph( float4.UnitW, rangeX, rangeY, ( float _x ) => {
					int	bucketIndex = Math.Min( BUCKETS_COUNT-1, (int) Math.Floor( _x * BUCKETS_COUNT ) );
 					return (float) noiseDistributionHistogram[bucketIndex] / (m_blueNoise.Width * m_blueNoise.Height);
 				} );
			#endif
		}

		void	CreateTestSpectrum( Complex[,] _spectrum ) {
			uint	size = m_handMadeBlueNoise.Width;
			uint	halfSize = size >> 1;

			double	angle = Math.PI * floatTrackbarControlRadialOffset.Value;
			double	distance = 50.0 * floatTrackbarControlRadialScale.Value;

			double	centerPosX0 = distance * Math.Cos( angle );
			double	centerPosY0 = distance * Math.Sin( angle );
			double	centerPosX1 = -distance * Math.Cos( angle );
			double	centerPosY1 = -distance * Math.Sin( angle );

			double	k = -0.01 * floatTrackbarControlScale.Value;
			double	amplitudeFactor = floatTrackbarControlOffset.Value;// 1000.0 / (size*size);

			Complex		Cnoise = new Complex();
			for ( uint Y=0; Y < m_handMadeBlueNoise.Height; Y++ ) {
				uint	Yoff = (Y + halfSize) & (size-1);
				int		Yrel = (int) Y - (int) halfSize;
				for ( uint X=0; X < m_handMadeBlueNoise.Width; X++ ) {
					uint	Xoff = (X + halfSize) & (size-1);
					int		Xrel = (int) X - (int) halfSize;

					double	Dx0 = Xrel - centerPosX0;
					double	Dy0 = Yrel - centerPosY0;
					double	amplitude0 = amplitudeFactor * Math.Exp( k * (Dx0*Dx0 + Dy0*Dy0) );
					double	Dx1 = Xrel - centerPosX1;
					double	Dy1 = Yrel - centerPosY1;
					double	amplitude1 = amplitudeFactor * Math.Exp( k * (Dx1*Dx1 + Dy1*Dy1) );

					Cnoise.r = amplitude0 + amplitude1;

					_spectrum[Xoff,Yoff] = Cnoise;
				}
			}
			Cnoise.r = floatTrackbarControlDC.Value;
			for ( uint Y=0; Y < m_handMadeBlueNoise.Height; Y++ ) {
				_spectrum[Y,0] = Cnoise;
				_spectrum[0,Y] = Cnoise;
			}
		}

		void	RebuildNoise() {
			uint	size = m_handMadeBlueNoise.Width;
			uint	halfSize = size >> 1;

			//////////////////////////////////////////////////////////////////////////
			// Reconstruct an artificial spectrum
			Complex[,]	handmadeSpectrum = new Complex[m_handMadeBlueNoise.Width, m_handMadeBlueNoise.Height];
			CreateNoiseSpectrum( handmadeSpectrum );
//CreateTestSpectrum( handmadeSpectrum );


			//////////////////////////////////////////////////////////////////////////
			// Reconstruct a hand-made "blue noise"
			Complex[,]	handMadeBlueNoise = new Complex[m_handMadeBlueNoise.Width,m_handMadeBlueNoise.Height];
			m_FFT.FFT_Inverse( handmadeSpectrum, handMadeBlueNoise );
//			m_FFT.FFT_Inverse( output, handMadeBlueNoise );


			//////////////////////////////////////////////////////////////////////////
			// Build the resulting images
			m_handMadeSpectrum.WritePixels( ( uint X, uint Y, ref float4 _color ) => {
				uint	Yoff = (Y + halfSize) & (size-1);
				uint	Xoff = (X + halfSize) & (size-1);
				float	R = (float) handmadeSpectrum[Xoff,Yoff].r;
				R *= 1.0f * size;
				R = Math.Abs( R );

// 				float	I = (float) handmadeSpectrum[Xoff,Yoff].i;
// 				I *= 2.0f * size;

				_color.Set( R, R, R, 1.0f );
			} );

			// Hand-made blue noise
			m_handMadeBlueNoise.WritePixels( ( uint X, uint Y, ref float4 _color ) => {
				float	R = (float) handMadeBlueNoise[X,Y].r;
				float	I = (float) handMadeBlueNoise[X,Y].i;
				_color.Set( R, R, R, 1.0f );
			} );

			UpdatePanels();
		}

		bool	m_displayType = false;
		private void panelImage_Click( object sender, EventArgs e ) {
			m_displayType = !m_displayType;
			UpdatePanels();
		}

		private void floatTrackbarControlScale_ValueChanged( UIUtility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			RebuildNoise();
		}

		private void floatTrackbarControlOffset_ValueChanged( UIUtility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			RebuildNoise();
		}

		private void buttonSave_Click( object sender, EventArgs e ) {
			if ( saveFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;

			try {
				m_handMadeBlueNoise.Save( new System.IO.FileInfo( saveFileDialog.FileName ) );
			} catch ( Exception ) {
			}
		}

		#endregion

		#region Simulated Annealing Method

#if !MEUGLE
		ImageFile	m_blueNoiseAnnealing;
		private void buttonSolidAngleAlgorithm_Click(object sender, EventArgs e) {
			uint	textureSize = 1U << integerTrackbarControlTexturePOT.Value;
			uint	dimensions = (uint) integerTrackbarControlVectorDimension.Value;
			uint	randomSeed = (uint) integerTrackbarControlRandomSeed.Value;
			uint	iterations = (uint) integerTrackbarControlAnnealingIterations.Value;

			if ( m_blueNoiseAnnealing != null )
				m_blueNoiseAnnealing.Dispose();
			m_blueNoiseAnnealing = new ImageFile( textureSize, textureSize, PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );

			float4[]	scanline = new float4[textureSize];

			ImageFile	graphStatistics = new ImageFile( (uint) panelImageSpectrum.Width, (uint) panelImageSpectrum.Height, PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			float4		black = new float4( 0, 0, 0, 1 );
			float2		rangeX = new float2( 0, 10000 );
//			float2		rangeY = new float2( -10, 10 );
//			float2		rangeY = new float2( -1, 2 );
			float2		rangeY = new float2( 0, 100 );

			float		sigma_i = floatTrackbarControlVariance.Value;	// Default, recommended value is 2.1
			float		sigma_s = 1.0f;	// Default, recommended value

//			uint		notificationIterationsCount = Math.Max( 1U, (uint) (0.025f * iterations) );
			uint		notificationIterationsCount = 1000;

			DateTime	startTime = DateTime.Now;

			this.Enabled = false;

			GeneratorSolidAngleGPU	generator = new GeneratorSolidAngleGPU( m_device, (uint) (Math.Log(m_blueNoiseAnnealing.Width)/Math.Log(2.0)), dimensions );
			generator.Generate( randomSeed, iterations, sigma_i, sigma_s, radioButtonNeighborMutations.Checked, notificationIterationsCount, ( uint _iterationIndex, uint _mutationsRate, float _energyScore, Array _texture, List< float > _statistics ) => {

				switch ( dimensions ) {
					case 1: {
						float[,]	texture = _texture as float[,];
						m_blueNoiseAnnealing.WritePixels( ( uint X, uint Y, ref float4 _color ) => {
							float	V = texture[X,Y];
							_color.Set( V, V, V, 1.0f );
						} );
						break;
					}

					case 2: {
						float2[,]	texture = _texture as float2[,];
						m_blueNoiseAnnealing.WritePixels( ( uint X, uint Y, ref float4 _color ) => {
							float2	V = texture[X,Y];
							_color.Set( V.x, V.y, 0, 1.0f );
						} );
						break;
					}
				}

				if ( m_blueNoiseAnnealing.Width < panelImage.Width )
					panelImage.Bitmap = m_blueNoiseAnnealing.AsTiledBitmap( (uint) panelImage.Width, (uint) panelImage.Height );
				else
					panelImage.Bitmap = m_blueNoiseAnnealing.AsBitmap;

				TimeSpan	deltaTime = DateTime.Now - startTime;

				labelAnnealingScore.Text = "Time: " + deltaTime.ToString( @"hh\:mm\:ss" ) + " ► Score: " + _energyScore + " - Iterations = " + _iterationIndex + " - Mutations Rate: " + _mutationsRate;
				labelAnnealingScore.Refresh();

				Application.DoEvents();

/*				// Plot statistics
				int	lastX = 0;
				graphStatistics.Clear( float4.One );
//				graphStatistics.PlotGraphAutoRangeY( black, rangeX, ref rangeY, ( float _X ) => {
				graphStatistics.PlotGraph( black, rangeX, rangeY, ( float _X ) => {
					int	X = Math.Max( 0, Math.Min( _statistics.Count-1, (int) (_statistics.Count - 10000.0f + _X) ) );
//					return _statistics[X];

					// Integrate...
					float	sum = 0.0f;
					for ( int x=lastX; x <= X; x++ )
						sum += _statistics[x];
					sum *= X != lastX ? 1.0f / (X - lastX) : 0.0f;
					lastX = X;

					return sum;
				} );
				graphStatistics.PlotAxes( black, rangeX, rangeY, 1000.0f, 100.0f );
				panelImageSpectrum.Bitmap = graphStatistics.AsBitmap;
//*/
			} );

			this.Enabled = true;

			m_blueNoiseAnnealing.Save( new System.IO.FileInfo( "BlueNoise" + m_blueNoiseAnnealing.Width + "x" + m_blueNoiseAnnealing.Height + "_SimulatedAnnealing" + dimensions + "D.png" ) );

			// Quick FFT
			panelImageSpectrum.Bitmap = ComputeSpectrum( m_blueNoiseAnnealing, 2.0f * textureSize ).AsBitmap;
		}
#else
		ImageFile	m_blueNoiseAnnealing = new ImageFile( 64, 64, ImageFile.PIXEL_FORMAT.R8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
		private void buttonSolidAngleAlgorithm_Click(object sender, EventArgs e) {

			uint		W = m_blueNoiseAnnealing.Width;
			uint		H = m_blueNoiseAnnealing.Height;
			float4[]	scanline = new float4[W];

			GeneratorSolidAngle	generator = new GeneratorSolidAngle( 6 );
			generator.Generate( 1, 1e-6f, 1000000, 2.1f, 1.0f, ( int _iterationIndex, float _energyScore, float[,] _texture ) => {
				if ( (_iterationIndex & 0x1F) != 1 )
					return;

				for ( uint Y=0; Y < H; Y++ ) {
					for ( uint X=0; X < W; X++ ) {
						float	V = _texture[X,Y];
						scanline[X].Set( V, V, V, 1.0f );
					}
					m_blueNoiseAnnealing.WriteScanline( Y, scanline );
				}

				panelImage.Bitmap = m_blueNoiseAnnealing.AsBitmap;
				labelAnnealingScore.Text = "Score: " + _energyScore + " - Iterations = " + _iterationIndex;
				labelAnnealingScore.Refresh();
			} );

			m_blueNoiseAnnealing.Save( new System.IO.FileInfo( "MyBlueNoiseMoisi64x64.png" ) );
		}
#endif

		#endregion

		#region Void-and-Cluster Method

		ImageFile	m_blueNoiseVoidAndCluster = null;
		private void buttonVoidAndCluster_Click( object sender, EventArgs e ) {
			uint	textureSize = 1U << integerTrackbarControlTexturePOT.Value;
			uint	randomSeed = (uint) integerTrackbarControlRandomSeed.Value;

			if ( m_blueNoiseVoidAndCluster != null )
				m_blueNoiseVoidAndCluster.Dispose();
//			m_blueNoiseVoidAndCluster = new ImageFile( textureSize, textureSize, PIXEL_FORMAT.R8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			m_blueNoiseVoidAndCluster = new ImageFile( textureSize, textureSize, PIXEL_FORMAT.R16, new ColorProfile( ColorProfile.STANDARD_PROFILE.LINEAR ) );

			float4[]	scanline = new float4[textureSize];

			ImageFile	graphStatistics = new ImageFile( (uint) panelImageSpectrum.Width, (uint) panelImageSpectrum.Height, PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			float4		black = new float4( 0, 0, 0, 1 );
			float2		rangeX = new float2( 0, 10000 );
//			float2		rangeY = new float2( -10, 10 );
//			float2		rangeY = new float2( -1, 2 );
			float2		rangeY = new float2( 0, 1 );

			float	sigma_i = floatTrackbarControlVariance.Value;	// Default, recommended value by Ulichney is 1.5

			DateTime	startTime = DateTime.Now;

			this.Enabled = false;

			GeneratorVoidAndClusterGPU	generator = new GeneratorVoidAndClusterGPU( m_device, (uint) (Math.Log(m_blueNoiseVoidAndCluster.Width)/Math.Log(2.0)) );
			generator.Generate( randomSeed, sigma_i, 0.025f, ( float _progress, float[,] _texture, List< float > _statistics ) => {

				m_blueNoiseVoidAndCluster.WritePixels( ( uint X, uint Y, ref float4 _color ) => {
					float	V = _texture[X,Y];
					_color.Set( V, V, V, 1.0f );
				} );

				if ( m_blueNoiseVoidAndCluster.Width < panelImage.Width )
					panelImage.Bitmap = m_blueNoiseVoidAndCluster.AsTiledBitmap( (uint) panelImage.Width, (uint) panelImage.Height );
				else
					panelImage.Bitmap = m_blueNoiseVoidAndCluster.AsBitmap;

				TimeSpan	deltaTime = DateTime.Now - startTime;

				labelAnnealingScore.Text = "Time: " + deltaTime.ToString( @"hh\:mm\:ss" ) + " ► Progress: " + (100.0f*_progress).ToString( "G3" ) + "%";
				labelAnnealingScore.Refresh();

				Application.DoEvents();

/*				// Plot statistics
				int	lastX = 0;
				graphStatistics.Clear( float4.One );
//				graphStatistics.PlotGraphAutoRangeY( black, rangeX, ref rangeY, ( float _X ) => {
				graphStatistics.PlotGraph( black, rangeX, rangeY, ( float _X ) => {
					int	X = Math.Max( 0, Math.Min( _statistics.Count-1, (int) (_statistics.Count * _X / rangeX.y) ) );
//					return _statistics[X];

					// Integrate...
					float	sum = 0.0f;
					for ( int x=lastX; x <= X; x++ )
						sum += _statistics[x];
					sum *= X != lastX ? 1.0f / (X - lastX) : 0.0f;
					lastX = X;

					return sum;
				} );
				graphStatistics.PlotAxes( black, rangeX, rangeY, 1000.0f, 100.0f );
				panelImageSpectrum.Bitmap = graphStatistics.AsBitmap;
//*/
// 				if ( _progress == 1.0f ) {
// 					using ( System.IO.FileStream S = new System.IO.FileInfo( "indices.txt" ).Create() ) {
// 						using ( System.IO.StreamWriter W = new System.IO.StreamWriter( S ) ) {
// 							for ( int i=0; i < _statistics.Count; i++ ) {
// 								W.WriteLine( (int) Math.Floor( m_blueNoiseVoidAndCluster.Width*m_blueNoiseVoidAndCluster.Height* _statistics[i] ) );
// 							}
// 						}
// 					}
// 				}
			} );

			this.Enabled = true;

			m_blueNoiseVoidAndCluster.Save( new System.IO.FileInfo( "BlueNoise" + m_blueNoiseVoidAndCluster.Width + "x" + m_blueNoiseVoidAndCluster.Height + "_VoidAndCluster.png" ) );

			// Quick FFT
			panelImageSpectrum.Bitmap = ComputeSpectrum( m_blueNoiseVoidAndCluster, 2.0f * textureSize ).AsBitmap;
		}

		#endregion

		bool	m_displayHandMadeSpectrum = false;
		private void panelImageSpectrum_Click( object sender, EventArgs e ) {
			#if COMPUTE_RADIAL_SLICE
				m_displayHandMadeSpectrum = !m_displayHandMadeSpectrum;
				UpdatePanels();
			#else
				if ( openFileDialog.ShowDialog( this ) != DialogResult.OK )
					return;

				try {
					ImageFile	noiseImage = new ImageFile( new System.IO.FileInfo( openFileDialog.FileName ) );
					panelImage.Bitmap = noiseImage.AsTiledBitmap( (uint) panelImage.Width, (uint) panelImage.Height );
					panelImageSpectrum.Bitmap = ComputeSpectrum( noiseImage, 2.0f * noiseImage.Width ).AsBitmap;
				} catch ( Exception ) {

				}
			#endif
		}

		private void checkBoxShowDistribution_CheckedChanged( object sender, EventArgs e ) {
			UpdatePanels();
		}

		private void buttonCombine_Click( object sender, EventArgs e ) {

			List< ImageFile >	layers = new List<ImageFile>();
			try {
				while ( layers.Count < 4 ) {
					if ( openFileDialog.ShowDialog( this ) != DialogResult.OK )
						return;	// Cancel

					ImageFile	layer = new ImageFile( new System.IO.FileInfo( openFileDialog.FileName ) );
					if ( layer.Width != layer.Height )
						throw new Exception( "Texture must be square!" );
					if ( layers.Count != 0 && layers[0].Width != layer.Width )
						throw new Exception( "Layer sizes mismatch!" );

					layers.Add( layer );

					if ( layers.Count > 1 && (layers.Count == 4 || MessageBox.Show( this, "Do you wish to combine another layer?", "Blue Noise Generator", MessageBoxButtons.YesNo ) == DialogResult.No) )
						break;
				}

				CombineNoise	combine = new CombineNoise();
				ImageFile		alignedLayers = combine.Combine( layers.ToArray() );
				panelImage.Bitmap = alignedLayers.AsBitmap;

				if ( saveFileDialog.ShowDialog( this ) != DialogResult.OK )
					return;

				alignedLayers.Save( new System.IO.FileInfo( saveFileDialog.FileName ) );

			} catch ( Exception _e ) {
				MessageBox.Show( this, "An error occurred while combining layers:\r\n\r\n" + _e.Message, "Blue Noise Generator", MessageBoxButtons.OK );
			} finally {
				while ( layers.Count > 0 ) {
					layers[0].Dispose();
					layers.RemoveAt( 0 );
				}
			}
		}
	}
}
