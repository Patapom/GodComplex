using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MotionTextureComputer
{
	/// <summary>
	/// This precomputer generates ocean wave height maps to render ocean water
	///  as specified in the paper "Simulating Ocean Water" by Tessendorf (2005)
	///  (http://graphics.ucsd.edu/courses/rendering/2005/jdewall/tessendorf.pdf)
	/// 
	/// The goal is to generate a heightfield at discrete spatial positions on a grid:
	/// 
	///		h(X,t) = Sum_over_K[ h~(K,t) . exp( -i.K.X ) ]
	///	
	/// Where:
	///		X = the discrete position on the (x,z) grid
	///		x = n.Lx/N		n € [-N/2,N/2[
	///		z = m.Lz/M		m € [-M/2,M/2[
	///		N, M = the size of the grid on the x and z axes respectively
	///		t = time
	///		K = (kx,kz), kx=2.PI.n/Lx, kz=2.PI.m/Lz
	///		h~ = height of the wave in frequency domain
	/// 
	/// 
	/// The texture is first initialized in frequency domain by filling it with random
	///  real and imaginary coefficients weighted by the square root of the Philips spectrum:
	///  
	///		h~0(K) = 1/sqrt(2) (Rx+i.Ry).sqrt( Ph(K) )
	/// 
	/// The Phillips spectrum:
	/// 
	///		Ph(K) = A.[exp( -1/(k.L)² ) / k^4].dot( Kn, Wn )²
	///	
	///	Where:
	///		Rx, Ry = Gaussian distributed random numbers
	///		A = a numerical factor
	///		K = (kx, kz), kx=2.PI.n/Lx, kz=2.PI.m/Lz	n€[-N/2,N/2] and m€[-M/2,M/2]
	///		k = norm of K
	///		Kn = normalized K
	///		Wn = normalized wind direction
	///		L = V²/g the largest possible wave arising from a continuous wind of speed V
	///		g = 9.8m/s² the gravitational constant
	/// 
	/// Then, we simply use a backward FFT to obtain the spatial displacement equivalent
	///  to that spectrum and store the result in a texture.
	/// </summary>
	public partial class Form1 : Form
	{
		#region METHODS

		public Form1()
		{
			InitializeComponent();
		}

		protected unsafe override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			using ( fftwlib.FFT2D FFT = new fftwlib.FFT2D( 256, 256 ) )
			{
				// Build the source data
//				FillInput( FFT.Input );

				{
					const double	Lx = 400.0;
					const double	Lz = Lx;

					const double	WindAngle = 45.0 * Math.PI / 180.0;
					const double	WindVelocity = 20.0;

					double	Wx = Math.Cos( WindAngle );
					double	Wz = Math.Sin( WindAngle );

					double	U1, U2, fX, fZ;

					Random	RNG = new Random( 1 );

					FFT.FillInputFrequency( ( int Fx, int Fy, out float r, out float i ) => {
						double	kx = (2.0 * Math.PI / Lx) * Fx;
						double	kz = (2.0 * Math.PI / Lz) * Fy;

						// Build white gaussian noise
						// (source: http://www.dspguru.com/dsp/howtos/how-to-generate-white-gaussian-noise)
						U1 = 1e-10 + RNG.NextDouble();
						U2 = RNG.NextDouble();
						fX = Math.Sqrt(-2.0 * Math.Log( U1 )) * Math.Sin( 2.0 * Math.PI * U2 );
						U1 = 1e-10 + RNG.NextDouble();
						U2 = RNG.NextDouble();
						fZ = Math.Sqrt(-2.0 * Math.Log( U1 )) * Math.Sin( 2.0 * Math.PI * U2 );

fX = fZ = 1.0;

						// Build Phillips spectrum
						double	SqrtPhillips = Math.Sqrt( Phillips( kx, kz, WindVelocity, Wx, Wz ) );
						r = (float) (1.0 / Math.Sqrt( 2.0 ) * fX * SqrtPhillips);
						i = (float) (1.0 / Math.Sqrt( 2.0 ) * fZ * SqrtPhillips);

r = (float) (U1);// * Math.Exp( -0.001 * Math.Abs( Fx ) ));
i = 0.0f;//(float) U2;

// r = 0.0;
// for ( int j=1; j < 100; j++ )
// 	r += 0.25 * Math.Cos( 2.0 * Math.PI * -j * (X+2*Y) / 256.0 ) / j;
// i = 0.0;

// r = Math.Exp( -0.1 * kx );
// i = Math.Exp( -0.1 * kz );

						} );
				}

 
				// Fill in bitmap
				outputPanelFrequency.FillBitmap( ( int _X, int _Y, int _Width, int _Height ) => 
					{
						int	X = 256 * _X / _Width;
						int	Y = 256 * _Y / _Height;
						byte	R = (byte) (255 * Math.Min( 1.0, Math.Abs( FFT.Input[2*(256*Y+X)+0] )));
						byte	I = (byte) (255 * Math.Min( 1.0, Math.Abs( FFT.Input[2*(256*Y+X)+1] )));

						return 0xFF000000 | (uint) ((R << 16) | (I << 8));
					} );

				// Inverse FFT
				FFT.InputIsSpatial = false;	// We fed frequencies and need an inverse transform
				FFT.Execute( fftwlib.FFT2D.Normalization.SQUARE_ROOT_OF_DIMENSIONS_PRODUCT );

// DEBUG: Test we get back what we fed!
// FFT.SwapInputOutput();
// FFT.InputIsSpatial = false;
// FFT.Execute( fftwlib.FFT2D.Normalization.SQUARE_ROOT_OF_DIMENSIONS_PRODUCT );

				// Retrieve results
				outputPanelSpatial.FillBitmap( ( int _X, int _Y, int _Width, int _Height ) => 
					{
						int	X = 256 * _X / _Width;
						int	Y = 256 * _Y / _Height;
						byte	R = (byte) (255 * Math.Min( 1.0f, Math.Abs( FFT.Output[2*(256*Y+X)+0] )));
						byte	I = (byte) (255 * Math.Min( 1.0f, Math.Abs( FFT.Output[2*(256*Y+X)+1] )));
// byte	R = (byte) (255 * Math.Min( 1.0f, 1.0e6 * Math.Abs( FFT.Output[2*(256*Y+X)+0] ) ));
// byte	I = (byte) (255 * Math.Min( 1.0f, 1.0e6 * Math.Abs( FFT.Output[2*(256*Y+X)+1] ) ));

						return 0xFF000000 | (uint) ((R << 16) | (I << 8));
					} );

				//////////////////////////////////////////////////////////////////////////
				// Save the results
				System.IO.FileInfo		F = new System.IO.FileInfo( @".\Water0_256x256.complex" );
				System.IO.FileStream	S = F.Create();
				System.IO.BinaryWriter	Writer = new System.IO.BinaryWriter( S );

				for ( int Y=0; Y < 256; Y++ )
					for ( int X=0; X < 256; X++ )
					{
						Writer.Write( FFT.Output[2*(256*Y+X)+0] );
						Writer.Write( FFT.Output[2*(256*Y+X)+1] );
					}

				Writer.Close();
				S.Close();
			}
		}

		protected void	FillInput( float[] _In )
		{
			const double	Lx = 400.0;
			const double	Lz = Lx;

			const double	WindAngle = 45.0 * Math.PI / 180.0;
			const double	WindVelocity = 20.0;

			double	Wx = Math.Cos( WindAngle );
			double	Wz = Math.Sin( WindAngle );

			double	U1, U2, fX, fZ;

			Random	RNG = new Random( 1 );
			for ( int Y=0; Y < 256; Y++ )
			{
//				double	kz = 2.0 * Math.PI * (Y-128) / Lz;
				double	kz = (2.0 * Math.PI / Lz) * (Y < 128 ? Y : Y-255);
				for ( int X=0; X < 256; X++ )
				{
//					double	kx = 2.0 * Math.PI * (X-128) / Lx;
					double	kx = (2.0 * Math.PI / Lx) * (X < 128 ? X : X-255);

					// Build white gaussian noise
					// (source: http://www.dspguru.com/dsp/howtos/how-to-generate-white-gaussian-noise)
					U1 = 1e-10 + RNG.NextDouble();
					U2 = RNG.NextDouble();
					fX = Math.Sqrt(-2.0 * Math.Log( U1 )) * Math.Sin( 2.0 * Math.PI * U2 );
					U1 = 1e-10 + RNG.NextDouble();
					U2 = RNG.NextDouble();
					fZ = Math.Sqrt(-2.0 * Math.Log( U1 )) * Math.Sin( 2.0 * Math.PI * U2 );

fX = fZ = 1.0;

					// Build Phillips spectrum
					double	SqrtPhillips = Math.Sqrt( Phillips( kx, kz, WindVelocity, Wx, Wz ) );
					double	r = 1.0 / Math.Sqrt( 2.0 ) * fX * SqrtPhillips;
					double	i = 1.0 / Math.Sqrt( 2.0 ) * fZ * SqrtPhillips;

// r = 0.0;
// for ( int j=1; j < 100; j++ )
// 	r += 0.25 * Math.Cos( 2.0 * Math.PI * -j * (X+2*Y) / 256.0 ) / j;
// i = 0.0;

// r = Math.Exp( -0.1 * kx );
// i = Math.Exp( -0.1 * kz );

					// We got our value
					_In[2*(256*Y+X)+0] = (float) r;
					_In[2*(256*Y+X)+1] = (float) i;
				}
			}
		}

		/// <summary>
		/// The Phillips spectrum:
		/// 
		///		Ph(K) = A.[exp( -1/(k.L)² ) / k^4].dot( Kn, Wn )²
		///	
		///	Where:
		///		A = a numerical factor
		///		K = (kx, kz), kx=2.PI.n/Lx, kz=2.PI.m/Lz	n€[-N/2,N/2] and m€[-M/2,M/2]
		///		k = norm of K
		///		Kn = normalized K
		///		Wn = normalized wind direction
		///		L = V²/g the largest possible wave arising from a continuous wind of speed V
		///		g = 9.8m/s² the gravitational constant
		///		
		/// </summary>
		/// <param name="kx"></param>
		/// <param name="kz"></param>
		/// <param name="V">Wind velocity in m/s</param>
		/// <param name="Wx">Normalized wind direction</param>
		/// <param name="Wz"></param>
		protected double	Phillips( double kx, double kz, double V, double Wx, double Wz )
		{
			const double	A = 1.0;
//			const double	l = 0.1;

			double	k = Math.Sqrt( kx*kx + kz*kz );
			kx /= Math.Max( 1e-3, k );
			kz /= Math.Max( 1e-3, k );

			double	dot = kx * Wx + kz * Wz;
					dot *= dot;
			double	Fact = A * dot / Math.Pow( k, 4.0 );

			// Eliminate large frequencies
//			Fact *= Math.Exp( -k*k*l*l );

			double	L = V*V / 9.8;

			return Fact * Math.Exp( -1.0 / Math.Pow( k * L, 2.0 ) );
		}

		// To fill the input with a bitmap (that was a test check FWD then BWD FFT to see if I got the image back again)
		protected unsafe void	FillWithBitmap( float[] _In )
		{
			System.Drawing.Imaging.BitmapData	LockedBitmap = Properties.Resources.TestPic.LockBits( new Rectangle( 0, 0, Properties.Resources.TestPic.Width, Properties.Resources.TestPic.Height ), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
			for ( int Y=0; Y < LockedBitmap.Height; Y++ )
			{
				byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + LockedBitmap.Stride * Y;
				for ( int X=0; X < LockedBitmap.Width; X++ )
				{
					byte	B = *pScanline++;
					byte	G = *pScanline++;
					byte	R = *pScanline++;
					byte	A = *pScanline++;
					float	fLuminance = (0.3f * R + 0.5f * G + 0.2f * B) / 255.0f;

					int CX = 256 * X / LockedBitmap.Width;
					int CY = 256 * Y / LockedBitmap.Height;
					_In[2*(256*CY+CX)+0] = fLuminance;
					_In[2*(256*CY+CX)+1] = 0.0f;
				}
			}

			// Don't do that as it's a stock resource
//			Properties.Resources.TestPic.UnlockBits( LockedBitmap );
		}

		#endregion
	}

}
