using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

using WMath;

namespace SecondOrderPolynomialApproximation
{
	public partial class SphereView : Panel
	{
		protected const int		WIDTH = 256;
		protected const int		HEIGHT = 128;

		public delegate float	GetValue( float _x, float _y, float _z );
		public delegate Color	GetColorValue( float _x, float _y, float _z );

		protected Bitmap		m_Render = new Bitmap( WIDTH, HEIGHT, System.Drawing.Imaging.PixelFormat.Format24bppRgb );

		public SphereView()
		{
			InitializeComponent();
		}

		public SphereView( IContainer container )
		{
			container.Add( this );

			InitializeComponent();
		}

		public unsafe void		RebuildSphere( GetValue _Delegate )
		{
			BitmapData	LockedBitmap = m_Render.LockBits( new Rectangle( 0, 0, WIDTH, HEIGHT ), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb );

			for ( int Y=0; Y < HEIGHT; Y++ )
			{
				float	fDy = 1.0f - Y / (.5f * HEIGHT);

				byte*	pScanline = (byte*) LockedBitmap.Scan0 + Y * LockedBitmap.Stride;

				for ( int X=0; X < WIDTH; X++ )
				{
					if ( X < .5f * WIDTH )
					{	// Front sphere
						float	fDx = (X - .25f * WIDTH) / (.25f * WIDTH);
						float	fSDistance = fDx * fDx + fDy * fDy;
						if ( fSDistance > 1.0f )
						{	// Put a black pixel
							pScanline[3*X+0] = 32;
							pScanline[3*X+1] = 32;
							pScanline[3*X+2] = 32;
							continue;
						}

						float	fDz = (float) Math.Sqrt( 1.0f - fSDistance );

						float	fValue = _Delegate( fDx, fDy, fDz );
						if ( fValue > 0.0f )
						{
							pScanline[3*X+0] = 0;
							pScanline[3*X+1] = (byte) (255.0f * Math.Min( 1.0f, fValue ));		// Green lobe for positive values
							pScanline[3*X+2] = 0;
						}
						else
						{
							pScanline[3*X+0] = 0;
							pScanline[3*X+1] = 0;
							pScanline[3*X+2] = (byte) (255.0f * Math.Min( 1.0f, -fValue ));		// Red lobe for negative values
						}
					}
					else
					{	// Back sphere
						float	fDx = (X - .75f * WIDTH) / (.25f * WIDTH);
						float	fSDistance = fDx * fDx + fDy * fDy;
						if ( fSDistance > 1.0f )
						{	// Put a black pixel
							pScanline[3*X+0] = 32;
							pScanline[3*X+1] = 32;
							pScanline[3*X+2] = 32;
							continue;
						}

						float	fDz = -(float) Math.Sqrt( 1.0f - fSDistance );

						float	fValue = _Delegate( fDx, fDy, fDz );
						if ( fValue > 0.0f )
						{
							pScanline[3*X+0] = 0;
							pScanline[3*X+1] = (byte) (255.0f * Math.Min( 1.0f, fValue ));		// Green lobe for positive values
							pScanline[3*X+2] = 0;
						}
						else
						{
							pScanline[3*X+0] = 0;
							pScanline[3*X+1] = 0;
							pScanline[3*X+2] = (byte) (255.0f * Math.Min( 1.0f, -fValue ));		// Red lobe for negative values
						}
					}
				}
			}

			m_Render.UnlockBits( LockedBitmap );
		}

		public unsafe void		RebuildSphereWithColor( GetColorValue _Delegate )
		{
			BitmapData	LockedBitmap = m_Render.LockBits( new Rectangle( 0, 0, WIDTH, HEIGHT ), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb );

			for ( int Y=0; Y < HEIGHT; Y++ )
			{
				float	fDy = 1.0f - Y / (.5f * HEIGHT);

				byte*	pScanline = (byte*) LockedBitmap.Scan0 + Y * LockedBitmap.Stride;

				for ( int X=0; X < WIDTH; X++ )
				{
					if ( X < .5f * WIDTH )
					{	// Front sphere
						float	fDx = (X - .25f * WIDTH) / (.25f * WIDTH);
						float	fSDistance = fDx * fDx + fDy * fDy;
						if ( fSDistance > 1.0f )
						{	// Put a black pixel
							pScanline[3*X+0] = 32;
							pScanline[3*X+1] = 32;
							pScanline[3*X+2] = 32;
							continue;
						}

						float	fDz = (float) Math.Sqrt( 1.0f - fSDistance );

						Color	Value = _Delegate( fDx, fDy, fDz );
						pScanline[3*X+0] = Value.B;
						pScanline[3*X+1] = Value.G;
						pScanline[3*X+2] = Value.R;
					}
					else
					{	// Back sphere
						float	fDx = (X - .75f * WIDTH) / (.25f * WIDTH);
						float	fSDistance = fDx * fDx + fDy * fDy;
						if ( fSDistance > 1.0f )
						{	// Put a black pixel
							pScanline[3*X+0] = 32;
							pScanline[3*X+1] = 32;
							pScanline[3*X+2] = 32;
							continue;
						}

						float	fDz = -(float) Math.Sqrt( 1.0f - fSDistance );

						Color	Value = _Delegate( fDx, fDy, fDz );
						pScanline[3*X+0] = Value.B;
						pScanline[3*X+1] = Value.G;
						pScanline[3*X+2] = Value.R;
					}
				}
			}

			m_Render.UnlockBits( LockedBitmap );
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			e.Graphics.DrawImage( m_Render, ClientRectangle, new Rectangle( 0, 0, WIDTH, HEIGHT ), GraphicsUnit.Pixel );
		}
	}
}
