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
	/// This precomputer generates motion textures to animate grass & branch trees
	///  as specified in the paper "Physically Guided Animation of Trees" by Habel et al.
	///  (http://www.cg.tuwien.ac.at/research/publications/2009/Habel_09_PGT/)
	/// This paper is based on stochastic motion textures animation by Chuang et a.
	///  (http://grail.cs.washington.edu/projects/StochasticMotionTextures/)
	/// 
	/// The texture is first initialized in frequency domain by filling it with the 
	///  velocity spectrum of a branch oscillator for given wind mean velocities, mass
	///  of the branch, branch length and velocity damping with frequency.
	/// 
	/// Then, we simply use a backward FFT to obtain the spatial displacement equivalent
	///  to that velocity spectrum and store the result in a texture.
	/// 
	/// The texture can then be used by applying :
	///  P' = P + WarpingMotionTexture[ StartUV + RandomTrajectoryUV * Time ]
	/// 
	/// Given that RandomTrajectoryUV's components are as far as possible as a rational
	///  to avoid noticeable periodical repetitions.
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

			// Create memory & plan
			IntPtr		pIn = fftwlib.fftwf.malloc( 256*256*2*sizeof(float) );
			IntPtr		pOut = fftwlib.fftwf.malloc( 256*256*2*sizeof(float) );
			IntPtr		Plan = fftwlib.fftwf.dft_2d( 256, 256, pIn, pOut, fftwlib.fftw_direction.Backward, fftwlib.fftw_flags.Measure );

			// Build the source data
			float[]		In = new float[2*256*256];	// 256x256 complex values
			FillInput( In );
 
			// Fill in bitmap
			panelOutput.FillBitmap( ( int _X, int _Y, int _Width, int _Height ) => 
				{
					int	X = 256 * _X / _Width;
					int	Y = 256 * _Y / _Height;
					byte	R = (byte) (255 * 1e6 * Math.Abs( In[2*(256*Y+X)+0] ));
					byte	I = (byte) (255 * In[2*(256*Y+X)+1]);

					return 0xFF000000 | (uint) ((R << 16) | (I << 8));
				} );

//			return;

			// Copy source data to FFTW memory
            GCHandle	hIn = GCHandle.Alloc( In, GCHandleType.Pinned );
            Marshal.Copy( In, 0, pIn, 2*256*256 );

			// Inverse FFT
			fftwlib.fftwf.execute( Plan );

			// Retrieve results
			float[]		Out = new float[2*256*256];
            GCHandle	hOut = GCHandle.Alloc( Out, GCHandleType.Pinned );
			Marshal.Copy( pOut, Out, 0, 2*256*256 );

			// Fill in bitmap
			panelOutput.FillBitmap( ( int _X, int _Y, int _Width, int _Height ) => 
				{
					int	X = 256 * _X / _Width;
					int	Y = 256 * _Y / _Height;
					byte	R = (byte) (255 * Math.Min( 1.0f, 5e-2 * Math.Abs( Out[2*(256*Y+X)+0] ) ));
					byte	I = (byte) (255 * Math.Min( 1.0f, 5e-2 * Math.Abs( Out[2*(256*Y+X)+1] ) ));
// 					byte	R = (byte) (255 * Out[2*(256*Y+X)+0] / 65536.0);	// FWD + BWD FFT => Multiplied by N*N
// 					byte	I = (byte) (255 * Out[2*(256*Y+X)+1] / 65536.0);

					return 0xFF000000 | (uint) ((R << 16) | (I << 8));
				} );

			//////////////////////////////////////////////////////////////////////////
			// Save the results
			System.IO.FileInfo		F = new System.IO.FileInfo( @".\Motion0_256x256.complex" );
			System.IO.FileStream	S = F.Create();
			System.IO.BinaryWriter	Writer = new System.IO.BinaryWriter( S );

			for ( int Y=0; Y < 256; Y++ )
				for ( int X=0; X < 256; X++ )
				{
					Writer.Write( Out[2*(256*Y+X)+0] );
					Writer.Write( Out[2*(256*Y+X)+1] );
				}

			Writer.Close();
			S.Close();

			// Free willy
			hIn.Free();
			hOut.Free();
     		fftwlib.fftwf.free( pIn );
			fftwlib.fftwf.free( pOut );
			fftwlib.fftwf.destroy_plan( Plan );
		}

		protected void	FillInput( float[] _In )
		{
			double	Mass = 1.0;					// Mass of branch
			double	WindVelocity = 1.0;			// Wind mean velocity
			double	VelocityDamping = 1.0;		// Velocity damping (attenuation of velocity with frequency => the higher the frequency, the more the damping)
			double	NaturalFrequency = 0.25;	// Natural oscillatory frequency of the branch
												// Should obey the empirical formula : fh = 2.55 * Length ^ -0.59

			double	U1, U2, fX, fY;
			double	Gamma2 = VelocityDamping*VelocityDamping;
			double	F02 = NaturalFrequency*NaturalFrequency;

			Random	RNG = new Random( 1 );
			for ( int Y=0; Y < 256; Y++ )
			{
				for ( int X=0; X < 256; X++ )
				{
//					double	Frequency = Math.Pow( 2.0, X+Y );
					double	Frequency = 0.01 * Math.Sqrt( X*X+Y*Y );

					// Build white gaussian noise
					// (source: http://www.dspguru.com/dsp/howtos/how-to-generate-white-gaussian-noise)
					U1 = 1e-10 + RNG.NextDouble();
					U2 = RNG.NextDouble();
	
					fX = Math.Sqrt(-2.0 * Math.Log( U1 )) * Math.Cos( 2.0 * Math.PI * U2 );
					fY = Math.Sqrt(-2.0 * Math.Log( U1 )) * Math.Sin( 2.0 * Math.PI * U2 );

					// Build force spectrum for wind
					// (source: "Animating Pictures with Stochastic Motion Textures" by Chuang et al.)
					// (http://grail.cs.washington.edu/projects/StochasticMotionTextures/sig05.pdf)
					//
					// Pv(f) ~ Vm / (1 + f/Vm)^5/3
					double	Pv = WindVelocity * Math.Pow( 1.0 + Frequency / WindVelocity, -5.0 / 3.0 );
					//
					// V(f) = G(f) * sqrt( Pv(f) )
					double	Vf = fX * Math.Sqrt( Pv );
					//
					//                      V(f) e^2iPI.Theta 
					// Dtip(f) = -----------------------------------------
					//           2PI m sqrt( [2PI.(f²-f0²)]² + Gamma².f² )
					//
					double	F2 = Frequency*Frequency;
					double	Den = 2.0 * Math.PI * Mass * Math.Sqrt( Math.Pow( 2.0 * Math.PI * (F2 - F02), 2.0 ) + Gamma2 * F2 );
					double	Dtip = Vf / Den;

					_In[2*(256*Y+X)+0] = (float) Dtip;
					_In[2*(256*Y+X)+1] = 0.0f;
				}
			}
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
