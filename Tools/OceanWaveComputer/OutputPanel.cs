using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace MotionTextureComputer
{
	public partial class OutputPanel : Panel
	{
		public Bitmap		m_Bitmap = null;

		public string		m_Title = "";
		public float		m_ScaleY = 1.0f;

		protected Pen[]	MyPens = new Pen[]
		{
			new Pen( System.Drawing.Brushes.Black, 2 ),
			new Pen( System.Drawing.Brushes.Red, 2 ),
			new Pen( System.Drawing.Brushes.DarkGreen, 2 ),
			new Pen( System.Drawing.Brushes.Blue, 2 ),
			new Pen( System.Drawing.Brushes.Gold, 2 ),
			new Pen( System.Drawing.Brushes.Gold, 2 ),
			new Pen( System.Drawing.Brushes.Gold, 2 ),
			new Pen( System.Drawing.Brushes.Gold, 2 ),
			new Pen( System.Drawing.Brushes.Gold, 2 ),
		};

		public OutputPanel()
		{
			InitializeComponent();
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			base.OnSizeChanged( e );

			if ( m_Bitmap != null )
				m_Bitmap.Dispose();

			m_Bitmap = new Bitmap( Width, Height, PixelFormat.Format32bppArgb );
			UpdateBitmap();
		}

		// Should return AARRGGBB
		public delegate uint	FillDelegate( int _X, int _Y, int _Width, int _Height );
		public unsafe void		FillBitmap( FillDelegate _Delegate )
		{
			BitmapData	LockedBitmap = m_Bitmap.LockBits( new Rectangle( 0, 0, m_Bitmap.Width, m_Bitmap.Height ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );

			int	W = LockedBitmap.Width;
			int H = LockedBitmap.Height;
			for ( int Y=0; Y < LockedBitmap.Height; Y++ )
			{
				byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + LockedBitmap.Stride * Y;
				for ( int X=0; X < LockedBitmap.Width; X++ )
				{
					uint	Color = _Delegate( X, Y, W, H );
					*pScanline++ = (byte) ((Color >> 0) & 0xFF);
					*pScanline++ = (byte) ((Color >> 8) & 0xFF);
					*pScanline++ = (byte) ((Color >> 16) & 0xFF);
					*pScanline++ = (byte) ((Color >> 24) & 0xFF);
				}
			}

			m_Bitmap.UnlockBits( LockedBitmap );
			Refresh();
		}

		public void		UpdateBitmap()
		{
			if ( m_Bitmap == null || IsDisposed )
				return;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				G.FillRectangle( Brushes.White, 0, 0, Width, Height );

// 				G.DrawString( m_Title + " - Scale = " + m_ScaleY.ToString( "G4" ) + " - T=" + m_ViewModeThickness + "m H=" + m_ViewModeHeight + "m", Font, Brushes.Black, 0, 0 );
			}
			Refresh();
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			if ( m_Bitmap != null )
				e.Graphics.DrawImage( m_Bitmap, 0, 0 );
		}
	}
}
