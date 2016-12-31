using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace TestFourier
{
	public partial class ImagePanel : Panel
	{
		private Bitmap				m_Bitmap = null;

		private string				m_MessageOnEmpty = null;
		public string				MessageOnEmpty {
			get { return m_MessageOnEmpty; }
			set { m_MessageOnEmpty = value; Invalidate(); }
		}

		public Bitmap		Bitmap {
			get { return m_Bitmap; }
			set {
				m_Bitmap = value;
				Invalidate();
			}
		}

		private bool				m_skipPaint = false;
		public bool				SkipPaint {
			get { return m_skipPaint; }
			set { m_skipPaint = value; Invalidate(); }
		}

		private RectangleF		ImageClientRect {
			get {
				if ( m_Bitmap == null )
					return new RectangleF( 0, 0, Width, Height );

				int		SizeX = m_Bitmap.Width;
				int		SizeY = m_Bitmap.Height;

				int		WidthIfVertical = SizeX * Height / SizeY;	// Client width of the image if fitting vertically
				int		HeightIfHorizontal = SizeY * Width / SizeX;	// Client height of the image if fitting horizontally

				if ( WidthIfVertical > Width ) {
					// Fit horizontally
					return new RectangleF( 0, 0.5f * (Height-HeightIfHorizontal), Width, HeightIfHorizontal );
				}
				else {
					// Fit vertically
					return new RectangleF( 0.5f * (Width-WidthIfVertical), 0, WidthIfVertical, Height );
				}
			}
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			base.OnSizeChanged( e );
			Invalidate();
		}

		protected override void OnPaintBackground( PaintEventArgs e ) {
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e ) {
			base.OnPaint( e );
			if ( m_skipPaint )
				return;

			RectangleF	Rect = ImageClientRect;
 			if ( Rect.Width != Width || Rect.Height != Height )
 				e.Graphics.FillRectangle( Brushes.Black, 0, 0, Width, Height );

			if ( m_Bitmap != null ) {
				e.Graphics.DrawImage( m_Bitmap, Rect, new RectangleF( 0, 0, m_Bitmap.Width, m_Bitmap.Height ), GraphicsUnit.Pixel );
			} else if ( m_MessageOnEmpty != null ) {
				SizeF	MessageSize = e.Graphics.MeasureString( m_MessageOnEmpty, Font, Width );
				e.Graphics.DrawString( m_MessageOnEmpty, Font, Brushes.White, 0.5f * (Width-MessageSize.Width), 0.5f * (Height-MessageSize.Height) );
			}
		}
	}
}
