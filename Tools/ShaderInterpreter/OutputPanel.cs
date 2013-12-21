using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace ShaderInterpreter
{
	public partial class OutputPanel : Panel
	{
		protected Bitmap	m_Bitmap = null;

		public OutputPanel()
		{
			InitializeComponent();
		}

		public OutputPanel( IContainer container )
		{
			container.Add( this );

			InitializeComponent();
		}

		protected void		UpdateBitmap()
		{
			if ( m_Bitmap == null )
				return;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				G.FillRectangle( Brushes.White, 0, 0, Width, Height );
// 
// 				G.DrawLine( Pens.Black, 10, 0, 10, Height );
// 				G.DrawLine( Pens.Black, 0, Height-10, Width, Height-10 );
// 
// 				float	x = 0.0f;
// 				float	y = GetFilmicCurve( x );
// 				for ( int X=10; X < Width; X++ )
// 				{
// 					float	px = x;
// 					float	py = y;
// 					x = (float) X / (Width - 20);
// 					y = m_ScaleY * GetFilmicCurve( m_ScaleX * x );
// 
// 					DrawLine( G, px, py, x, y );
// 				}
			}

			Invalidate();
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			if ( m_Bitmap != null )
				m_Bitmap.Dispose();

			m_Bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
			UpdateBitmap();

			base.OnSizeChanged( e );
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
