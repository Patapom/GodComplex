//////////////////////////////////////////////////////////////////////////
// Implements the Valve technique for self-shadowed bump maps
// Source: http://www.valvesoftware.com/publications/2007/SIGGRAPH2007_EfficientSelfShadowedRadiosityNormalMapping.pdf
// More: http://n00body.squarespace.com/journal/2010/2/7/self-shadowed-bump-maps.html
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GenerateSelfShadowedBumpMap
{
	public partial class Form1 : Form
	{
		private int				W, H;
		private Bitmap			m_BitmapSource = null;
		private Bitmap			m_BitmapResult = null;
		private float[,]		m_HeightMap = null;
		private WMath.Vector[,]	m_Normal = null;
		private WMath.Vector[,]	m_Result = null;

		public unsafe Form1()
		{
			InitializeComponent();

			m_BitmapSource = Bitmap.FromFile( "eye_generic_01_disp.png" ) as Bitmap;
			outputPanelInputHeightMap.Image = m_BitmapSource;
			W = m_BitmapSource.Width;
			H = m_BitmapSource.Height;

			m_BitmapResult = new Bitmap( W,H, PixelFormat.Format32bppRgb );

			m_Result = new WMath.Vector[W,H];
			m_Normal = new WMath.Vector[W,H];

			// Fill source height map with heights in [0,1]
			m_HeightMap = new float[W,H];
//			BitmapData	LockedBitmap = m_BitmapSource.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.ReadOnly, PixelFormat.Format64bppArgb );
			BitmapData	LockedBitmap = m_BitmapSource.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb );
			for ( int Y=0; Y < H; Y++ )
			{
//				ushort*	pScanline = (ushort*) ((byte*) LockedBitmap.Scan0.ToPointer() + LockedBitmap.Stride * Y);
				byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + LockedBitmap.Stride * Y;
				for ( int X=0; X < W; X++, pScanline+=4 )
				{
//					float	Height = pScanline[0] / 65535.0f;
					float	Height = pScanline[0] / 255.0f;
					m_HeightMap[X,Y] = Height;
				}
			}
			m_BitmapSource.UnlockBits( LockedBitmap );
		}

		private void MessageBox( string _Message )
		{
			MessageBox( _Message, MessageBoxButtons.OK, MessageBoxIcon.Information );
		}
		private void MessageBox( string _Message, MessageBoxButtons _Buttons, MessageBoxIcon _Icon )
		{
			System.Windows.Forms.MessageBox.Show( this, _Message, "Generator", _Buttons, _Icon );
		}

		private unsafe void buttonGenerate_Click( object sender, EventArgs e )
		{
			try
			{
				groupBox1.Enabled = false;

				// Half-life basis (Z points outside of the surface, as in normal maps)
				WMath.Vector[]	Basis = new WMath.Vector[] {
					new WMath.Vector( (float) (-1.0 / Math.Sqrt( 6.0 )), (float) (1.0 / Math.Sqrt( 2.0 )), (float) (1.0 / Math.Sqrt( 3.0 )) ),
					new WMath.Vector( (float) (-1.0 / Math.Sqrt( 6.0 )), (float) (-1.0 / Math.Sqrt( 2.0 )), (float) (1.0 / Math.Sqrt( 3.0 )) ),
					new WMath.Vector( (float) (Math.Sqrt( 2.0 ) / Math.Sqrt( 6.0 )), (float) 0.0, (float) (1.0 / Math.Sqrt( 3.0 )) ),
				};

				// 1] Compute normal map
				WMath.Vector	dX = new WMath.Vector();
				WMath.Vector	dY = new WMath.Vector();
				WMath.Vector	N;
				float			ddX = floatTrackbarControlPixelSize.Value;
				float			ddH = floatTrackbarControlHeight.Value;
				for ( int Y=0; Y < H; Y++ )
				{
					int	Y0 = Math.Max( 0, Y-1 );
					int	Y1 = Math.Min( H-1, Y+1 );
					for ( int X=0; X < W; X++ )
					{
						int	X0 = Math.Max( 0, X-1 );
						int	X1 = Math.Min( W-1, X+1 );

						float	Hx0 = m_HeightMap[X0,Y];
						float	Hx1 = m_HeightMap[X1,Y];
						float	Hy0 = m_HeightMap[X,Y0];
						float	Hy1 = m_HeightMap[X,Y1];

						dX.Set( 2.0f * ddX, 0.0f, ddH * (Hx1 - Hx0) );
						dY.Set( 0.0f, 2.0f * ddX, ddH * (Hy1 - Hy0) );

						N = dX.Cross( dY ).Normalized;

						m_Normal[X,Y] = new WMath.Vector(
							N.Dot( Basis[0] ),
							N.Dot( Basis[1] ),
							N.Dot( Basis[2] ) );
					}

					// Update and show progress
					UpdateProgress( m_Normal, Y, true );
				}
				UpdateProgress( m_Normal, H, true );

				// 2] Compute directional occlusion
				float	Exponent = floatTrackbarControlLobeExponent.Value;
				float	Scale = floatTrackbarControlPixelSize.Value / floatTrackbarControlHeight.Value;	// Scale factor to apply to pixel distances so they're renormalized in [0,1], our "heights space"...
						Scale *= floatTrackbarControlZFactor.Value;	// Cheat Z velocity so AO is amplified!

				for ( int Y=0; Y < H; Y++ )
				{
					for ( int X=0; X < W; X++ )
					{
						float	R = ComputeAO( Basis[0], X, Y, Scale, Exponent );
						float	G = ComputeAO( Basis[1], X, Y, Scale, Exponent );
						float	B = ComputeAO( Basis[2], X, Y, Scale, Exponent );
						N = m_Normal[X,Y];

						m_Result[X,Y] = new WMath.Vector( R * N.x, G * N.y, B * N.z );
					}

					// Update and show progress
					UpdateProgress( m_Result, Y, true );
				}
				UpdateProgress( m_Result, H, true );

				m_BitmapResult.Save( "eye_generic_01_disp_hl2.png", ImageFormat.Png );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred during generation:\r\n" + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
			finally
			{
				groupBox1.Enabled = true;
			}
		}

		/// <summary>
		/// Computes the ambient occlusion of the specified coordinate by shooting N rays in a lobe oriented in the specified light direction
		/// </summary>
		/// <param name="_Light"></param>
		/// <param name="_X"></param>
		/// <param name="_Y"></param>
		/// <param name="_Z2HeightScale">Scale factor to apply to world Z coordinate to be remapped into the heights' [0,1] range</param>
		/// <param name="_LobeExponent">1 is a simple cosine lobe</param>
		/// <returns></returns>
		/// 
		WMath.Vector	m_X;
		WMath.Vector	m_Y;
		WMath.Vector	m_RayLocal = new WMath.Vector();
		WMath.Vector	m_RayWorld = new WMath.Vector();
		private float	ComputeAO( WMath.Vector _Light, int _X, int _Y, float _Z2HeightScale, float _LobeExponent )
		{
			double	Exponent = 1.0 / (1.0 + _LobeExponent);

			WMath.Vector	ScaledLight = new WMath.Vector( _Light.x, _Light.y, _Z2HeightScale *_Light.y ).Normalized;

			// Create orthonormal basis to orient the lobe
			m_X = ScaledLight.Cross( WMath.Vector.UnitZ ).Normalized;	// We can safely use (0,0,1) as the "up" direction since the HL2 basis doesn't have any vertical direction
			m_Y = m_X.Cross( ScaledLight );

			// Start from the provided coordinates
			float	X = _X;
			float	Y = _Y;
			float	Z = m_HeightMap[_X,_Y];

			double	AO = 0.0f;
			int		SamplesCount = 0;
			for ( int RayIndex=0; RayIndex < integerTrackbarControlRaysCount.Value; RayIndex++ )
			{
				double	Phi = 2.0 * Math.PI * WMath.SimpleRNG.GetUniform();
				double	Theta = Math.Acos( Math.Pow( WMath.SimpleRNG.GetUniform(), Exponent ) );
				m_RayLocal.x = (float) (Math.Cos( Phi ) * Math.Sin( Theta ));
				m_RayLocal.y = (float) (Math.Sin( Phi ) * Math.Sin( Theta ));
				m_RayLocal.z = (float) Math.Cos( Theta );
				m_RayWorld = m_RayLocal.x * m_X + m_RayLocal.y * m_Y + m_RayLocal.z * ScaledLight;
 				if ( m_RayWorld.z < 0.0f )
				{
// AO += 1.0;
// SamplesCount++;
 					continue;	// Pointing to the ground so don't account for it...
				}

				// Make sure the ray has a unit step so we always travel at least one pixel
//				m_RayWorld.z *= _Z2HeightScale;
// 				float	Normalizer = 1.0f / Math.Max( Math.Abs( m_RayWorld.x ), Math.Abs( m_RayWorld.y ) );
// 				float	Normalizer = 1.0f;
// 				Normalizer = Math.Max( Normalizer, (1.0f - Z) / (128.0f * m_RayWorld.z) );	// This makes sure we can't use more than 128 steps to escape the heightfield

				float	Normalizer = (1.0f - Z) / (128.0f * m_RayWorld.z);

				m_RayWorld.x *= Normalizer;
				m_RayWorld.y *= Normalizer;
				m_RayWorld.z *= Normalizer;

				// Compute intersection with the height field
				while ( Z < 1.0f )
				{
					X += m_RayWorld.x;
					Y += m_RayWorld.y;
					Z += m_RayWorld.z;

					float	Height = SampleHeightField( X, Y );
					if ( Height >= Z )
					{	// Hit!
						AO += 1.0;
						break;
					}
				}

				SamplesCount++;
			}
			AO /= SamplesCount;

			return (float) (1.0 - AO);
		}

		private float	SampleHeightField( float _X, float _Y )
		{
			_X *= W;
			_Y *= H;
			int		X0 = (int) Math.Floor( _X );
			int		Y0 = (int) Math.Floor( _Y );
			float	x = _X - X0;
			float	y = _Y - Y0;
			X0 = Math.Max( 0, Math.Min( W-1, X0 ) );
			Y0 = Math.Max( 0, Math.Min( H-1, Y0 ) );
			int		X1 = Math.Min( W-1, X0+1 );
			int		Y1 = Math.Min( H-1, Y0+1 );

			float	V00 = m_HeightMap[X0,Y0];
			float	V01 = m_HeightMap[X1,Y0];
			float	V10 = m_HeightMap[X0,Y1];
			float	V11 = m_HeightMap[X1,Y1];

			float	V0 = V00 + (V01-V00) * x;
			float	V1 = V10 + (V11-V10) * x;

			float	V = V0 + (V1-V0) * y;
			return V;
		}

		private unsafe void	UpdateProgress( WMath.Vector[,] _Source, int Y, bool _Bias )
		{
			const int	REFRESH_EVERY_N_SCANLINES = 4;

			if ( Y == 0 || (Y & (REFRESH_EVERY_N_SCANLINES-1)) != 0 )
				return;

//			BitmapData	LockedBitmap = m_BitmapResult.LockBits( new Rectangle( 0, Y-REFRESH_EVERY_N_SCANLINES, W, REFRESH_EVERY_N_SCANLINES ), ImageLockMode.WriteOnly, PixelFormat.Format64bppRgb );
			BitmapData	LockedBitmap = m_BitmapResult.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb );
			for ( int y=Y-REFRESH_EVERY_N_SCANLINES; y < Y; y++ )
			{
//						ushort*	pScanline = (ushort*) ((byte*) LockedBitmap.Scan0.ToPointer() + LockedBitmap.Stride * y);
				byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + LockedBitmap.Stride * y;
				for ( int X=0; X < W; X++ )
				{
					WMath.Vector	V = _Source[X,y];
// 							*pScanline++ = (ushort) 65535;												// A
// 							*pScanline++ = (ushort) (65535.0 * Math.Max( 0, Math.Min( 1.0, V.x ) ));	// R
// 							*pScanline++ = (ushort) (65535.0 * Math.Max( 0, Math.Min( 1.0, V.y ) ));	// G
// 							*pScanline++ = (ushort) (65535.0 * Math.Max( 0, Math.Min( 1.0, V.z ) ));	// B

					*pScanline++ = (byte) (Math.Max( 0, Math.Min( 255, 255.0 * (_Bias ? 0.5f * (1.0f + V.z) : V.z) ) ));	// B
					*pScanline++ = (byte) (Math.Max( 0, Math.Min( 255, 255.0 * (_Bias ? 0.5f * (1.0f + V.y) : V.y) ) ));	// G
					*pScanline++ = (byte) (Math.Max( 0, Math.Min( 255, 255.0 * (_Bias ? 0.5f * (1.0f + V.x) : V.x) ) ));	// R
					*pScanline++ = (byte) 255;												// A
				}
			}
			m_BitmapResult.UnlockBits( LockedBitmap );
			LockedBitmap = null;
//			outputPanelResult.Image = m_BitmapResult;
//			Application.DoEvents();
		}

	}
}
