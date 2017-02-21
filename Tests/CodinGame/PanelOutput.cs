using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace TestForm
{
	public partial class PanelOutput : Panel
	{
		protected Bitmap	m_Bitmap = null;

		public delegate void	PaintBitmap( Graphics _G );
		public event PaintBitmap	OnUpdateBitmap;

		public PanelOutput() {
			InitializeComponent();
		}

		public PanelOutput( IContainer container ) {
			container.Add( this );
			InitializeComponent();
		}

		public void		UpdateBitmap() {
			if ( m_Bitmap == null )
				return;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) ) {
				if ( OnUpdateBitmap != null )
					OnUpdateBitmap( G );
				else
					G.FillRectangle( Brushes.White, 0, 0, Width, Height );
// 				G.DrawLine( Pens.Black, 10, 0, 10, Height );
// 				G.DrawLine( Pens.Black, 0, Height-10, Width, Height-10 );
			}

			Invalidate();
		}

		protected override void OnSizeChanged( EventArgs e ) {
			if ( m_Bitmap != null )
				m_Bitmap.Dispose();

			m_Bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
			UpdateBitmap();

			base.OnSizeChanged( e );
		}

		protected override void OnPaintBackground( PaintEventArgs e ) {
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e ) {
			base.OnPaint( e );

			if ( m_Bitmap != null )
				e.Graphics.DrawImage( m_Bitmap, 0, 0 );
		}
	}
}
