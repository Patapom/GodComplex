using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace TestImportanceSampling
{
	public partial class DisplayPanel : Panel
	{
		protected Bitmap	m_Bitmap = null;

		public double		m_Factor = 10.0;
		private Pen			m_Pen = null;

		public DisplayPanel( IContainer container )
		{
			container.Add( this );

			InitializeComponent();

			m_Bitmap = new Bitmap( 128, 128, PixelFormat.Format32bppArgb );
		}

		public unsafe void		Update( double[,] _Probabilities )
		{
			BitmapData	LockedBitmap = m_Bitmap.LockBits( new Rectangle( 0, 0, 128, 128 ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );

			for ( int Y=0; Y < 128; Y++ )
			{
				byte*	pScanline = (byte*) LockedBitmap.Scan0 + LockedBitmap.Stride * Y;
				for ( int X=0; X < 128; X++ )
				{
					double	P = m_Factor * _Probabilities[X,Y];
					byte	C = (byte) Math.Max( 0, Math.Min( 255, 255 * P ) );

					*pScanline++ = C;
					*pScanline++ = C;
					*pScanline++ = C;
					*pScanline++ = 0xFF;
				}
			}

			m_Bitmap.UnlockBits( LockedBitmap );

			Invalidate();
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			if ( m_Bitmap != null )
				e.Graphics.DrawImage( m_Bitmap, 0, 0, Height, Height );
		}
	}
}
