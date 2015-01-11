using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ImageUtility;

namespace ImprovedNormalMapDistribution
{
	/// <summary>
	/// Computes an improved normal distribution following http://rgba32.blogspot.fr/2011/02/improved-normal-map-distributions.html
	/// I'm attempting to use the more advanced quartic function f(x,y)=(1-x²)(1-y²) for which there is a more involved intersection computation.
	/// 
	/// I'm first trying with Newton-Raphson then I'll use the quartic intersection that can obviously have only a single intersection...
	/// </summary>
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void floatTrackbarControlPhi_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			UpdateNormal();
		}

		private void floatTrackbarControlTheta_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			UpdateNormal();
		}

		void	UpdateNormal()
		{
			double	Phi = floatTrackbarControlPhi.Value * Math.PI / 180;
			double	Theta = floatTrackbarControlTheta.Value * Math.PI / 180;

			double	X = Math.Cos( Phi ) * Math.Sin( Theta );
			double	Y = Math.Sin( Phi ) * Math.Sin( Theta );
			double	Z = Math.Cos( Theta );

			if ( Z < 1e-12 )
				Z = 0.0;

			outputPanel1.SetAngles( Phi, Theta );
			outputPanel21.Theta = Theta;
		}

		private void checkBoxSplat_CheckedChanged( object sender, EventArgs e )
		{
			outputPanel1.Splat = checkBoxSplat.Checked;
		}

		private unsafe void buttonConvertNew_Click( object sender, EventArgs _e )
		{
			if ( openFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;
			if ( saveFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;

			FileInfo	SourceFileName = new FileInfo( openFileDialog.FileName );
			FileInfo	TargetFileName = new FileInfo( saveFileDialog.FileName );

			int			W, H;
			float3[,]	Vectors;
			float		x, y, z;
			using ( TargaImage TGA = new TargaImage( SourceFileName.FullName, false ) )
			{
				// Convert
				byte[]		ImageContent = Bitmap.LoadBitmap( TGA.Image, out W, out H );
				Vectors = new float3[W,H];
				int		rha = 0;
				for ( int Y=0; Y < H; Y++ )
					for ( int X=0; X < W; X++ )
					{
						x = 2.0f * ImageContent[rha++] / 255 - 1.0f;
						y = 2.0f * ImageContent[rha++] / 255 - 1.0f;
						z = 2.0f * ImageContent[rha++] / 255 - 1.0f;
						rha++;	// Skip alpha

						z = Math.Max( 0.0f, z );

						float	Norm = 1.0f / (float) Math.Sqrt( x*x + y*y + z*z );
						Vectors[X,Y].x = x * Norm;
						Vectors[X,Y].y = y * Norm;
						Vectors[X,Y].z = z * Norm;
					}
			}

			// Convert to RG improved normal
			double	Nx, Ny;
			double	CosPhi, SinPhi, CosTheta, SinTheta, Normalizer;
			double	a = 1.0, b, c, d = 0.0, e, t;

			ushort[,]	PackedNormal = new ushort[W,H];
			for ( int Y=0; Y < H; Y++ )
				for ( int X=0; X < W; X++ )
				{
					x = Vectors[X,Y].x;
					y = Vectors[X,Y].y;
					z = Vectors[X,Y].z;

					CosTheta = z;
					SinTheta = Math.Sqrt( 1 - z*z );
					Normalizer = 1.0 / Math.Max( 1e-10, SinTheta );
					CosPhi = x * Normalizer;
					SinPhi = y * Normalizer;

					e = SinTheta*SinTheta*SinTheta*SinTheta * CosPhi*CosPhi * SinPhi*SinPhi;
					c = -SinTheta*SinTheta;
					b = -CosTheta;

					double[]	roots = Polynomial.solvePolynomial( a, b, c, d, e );

					t = Math.Sqrt( 2.0 );
					for ( int i=0; i < roots.Length; i++ )
						if ( !double.IsNaN( roots[i] ) && roots[i] >= 0.0 )
							t = Math.Min( t, roots[i] );

// 					Nx = t * CosPhi * SinTheta;
// 					Ny = t * SinPhi * SinTheta;
					Nx = t * x;
					Ny = t * y;

					Vectors[X,Y].x = (float) (0.5 * (1.0 + Nx));
					Vectors[X,Y].y = (float) (0.5 * (1.0 + Ny));
					Vectors[X,Y].z = 0.0f;
				}


			// Save as target PNG
			using ( System.Drawing.Bitmap B = new System.Drawing.Bitmap( W, H, System.Drawing.Imaging.PixelFormat.Format32bppRgb ) )
			{
				System.Drawing.Imaging.BitmapData	LockedBitmap = B.LockBits( new System.Drawing.Rectangle( 0, 0, W, H ), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb );

				for ( int Y=0; Y < H; Y++ )
				{
					byte*	pScanline = (byte*) LockedBitmap.Scan0 + LockedBitmap.Stride * Y;
					for ( int X=0; X < W; X++ )
					{
						*pScanline++ = 0;
						*pScanline++ = (byte) (255 * Vectors[X,Y].y);
						*pScanline++ = (byte) (255 * Vectors[X,Y].x);
						*pScanline++ = 0xFF;
					}
				}

				B.UnlockBits( LockedBitmap );
				
				B.Save( TargetFileName.FullName );
			}
		}

		private unsafe void buttonConvertOld_Click( object sender, EventArgs args )
		{
			if ( openFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;
			if ( saveFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;

			FileInfo	SourceFileName = new FileInfo( openFileDialog.FileName );
			FileInfo	TargetFileName = new FileInfo( saveFileDialog.FileName );

			int			W, H;
			float3[,]	Vectors;
			float		x, y, z;
			using ( TargaImage TGA = new TargaImage( SourceFileName.FullName, false ) )
			{
				// Convert
				byte[]		ImageContent = Bitmap.LoadBitmap( TGA.Image, out W, out H );
				Vectors = new float3[W,H];
				int		rha = 0;
				for ( int Y=0; Y < H; Y++ )
					for ( int X=0; X < W; X++ )
					{
						x = 2.0f * ImageContent[rha++] / 255 - 1.0f;
						y = 2.0f * ImageContent[rha++] / 255 - 1.0f;
						z = 2.0f * ImageContent[rha++] / 255 - 1.0f;
						rha++;	// Skip alpha

						z = Math.Max( 0.0f, z );

						float	Norm = 1.0f / (float) Math.Sqrt( x*x + y*y + z*z );
						Vectors[X,Y].x = x * Norm;
						Vectors[X,Y].y = y * Norm;
						Vectors[X,Y].z = z * Norm;
					}
			}

			// Convert to RG slightly less improved normal
			double	Nx, Ny;
			double	a, b, c = -1.0, d, t;

			ushort[,]	PackedNormal = new ushort[W,H];
			for ( int Y=0; Y < H; Y++ )
				for ( int X=0; X < W; X++ )
				{
					x = Vectors[X,Y].x;
					y = Vectors[X,Y].y;
					z = Vectors[X,Y].z;

					// Here I'm using the exact algorithm described by http://rgba32.blogspot.fr/2011/02/improved-normal-map-distributions.html
					a = (x * x) + (y * y);
					b = z;
					d = b*b - 4.0*a*c;
					t = (-b + Math.Sqrt( d )) / (2.0 * a);

					Nx = x * t;
					Ny = y * t;

					Vectors[X,Y].x = (float) (0.5 * (1.0 + Nx));
					Vectors[X,Y].y = (float) (0.5 * (1.0 + Ny));
					Vectors[X,Y].z = 0.0f;
				}


			// Save as target PNG
			using ( System.Drawing.Bitmap B = new System.Drawing.Bitmap( W, H, System.Drawing.Imaging.PixelFormat.Format32bppRgb ) )
			{
				System.Drawing.Imaging.BitmapData	LockedBitmap = B.LockBits( new System.Drawing.Rectangle( 0, 0, W, H ), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb );

				for ( int Y=0; Y < H; Y++ )
				{
					byte*	pScanline = (byte*) LockedBitmap.Scan0 + LockedBitmap.Stride * Y;
					for ( int X=0; X < W; X++ )
					{
						*pScanline++ = 0;
						*pScanline++ = (byte) (255 * Vectors[X,Y].y);
						*pScanline++ = (byte) (255 * Vectors[X,Y].x);
						*pScanline++ = 0xFF;
					}
				}

				B.UnlockBits( LockedBitmap );
				
				B.Save( TargetFileName.FullName );
			}
		}
	}
}
