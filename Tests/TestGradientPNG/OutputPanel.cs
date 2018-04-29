using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace TestGradientPNG
{
	public partial class OutputPanel : Panel
	{
		protected Bitmap	m_Bitmap = null;

		protected byte[]	m_Gradient = null;

		public byte[]		Gradient
		{
			get { return m_Gradient; }
			set
			{
				if ( value == m_Gradient )
					return;

				m_Gradient = value;
				UpdateBitmap();
			}
		}

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
			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				G.FillRectangle( Brushes.White, 0, 0, Width, Height );

				if ( m_Gradient != null )
				{
					for ( int X=1; X < 256; X++ )
					{
						int	X0 = (Width-1) * (X-1) / 255;
						int	X1 = (Width-1) * X / 255;
						int	Y0 = (int) ((Height-1) * (1.0f - m_Gradient[X-1] / 255.0f));
						int	Y1 = (int) ((Height-1) * (1.0f - m_Gradient[X] / 255.0f));

						G.DrawLine( Pens.Black, X0, Y0, X1, Y1 );
					}
				}
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
