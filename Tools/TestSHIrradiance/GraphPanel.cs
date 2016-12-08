using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

using SharpMath;

namespace TestSHIrradiance
{
	public partial class GraphPanel : Panel {
		private Bitmap				m_Bitmap = null;
		public Bitmap		Bitmap {
			get { return m_Bitmap; }
			set {
				m_Bitmap = value;
				Invalidate();
			}
		}
		private bool				m_EnablePaint = true;
		public bool					EnablePaint {
			get { return m_EnablePaint; }
			set { m_EnablePaint = value; Invalidate(); }
		}

		private string				m_MessageOnEmpty = "Bisou";
		public string				MessageOnEmpty {
			get { return m_MessageOnEmpty; }
			set { m_MessageOnEmpty = value; Invalidate(); }
		}

		private RectangleF		ImageClientRect {
			get {
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

		public GraphPanel( IContainer container )
		{
			container.Add( this );
			InitializeComponent();
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			base.OnSizeChanged( e );
			Invalidate();
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );
			if ( !m_EnablePaint )
				return;

			if ( m_Bitmap != null ) {
				RectangleF	Rect = ImageClientRect;
				if ( Rect.Width != Width && Rect.Height != Height )
					e.Graphics.FillRectangle( Brushes.Black, 0, 0, Width, Height );
				e.Graphics.DrawImage( m_Bitmap, Rect, new RectangleF( 0, 0, m_Bitmap.Width, m_Bitmap.Height ), GraphicsUnit.Pixel );
			}
			else if ( m_MessageOnEmpty != null ) {
				SizeF	MessageSize = e.Graphics.MeasureString( m_MessageOnEmpty, Font );
				e.Graphics.FillRectangle( Brushes.Black, 0, 0, Width, Height );
				e.Graphics.DrawString( m_MessageOnEmpty, Font, Brushes.White, 0.5f * (Width-MessageSize.Width), 0.5f * (Height-MessageSize.Height) );
			}
		}
	}
}
