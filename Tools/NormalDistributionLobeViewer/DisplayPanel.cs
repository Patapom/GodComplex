using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace LobeViewer
{
	public partial class DisplayPanel : Panel
	{
		protected Bitmap	m_Bitmap = null;

		protected float		m_Roughness = 0.0f;
		public float	Roughness
		{
			get { return m_Roughness; }
			set { m_Roughness = value; RefreshLobe(); }
		}

		public DisplayPanel()
		{
			InitializeComponent();
		}

		public DisplayPanel( IContainer container )
		{
			container.Add( this );

			InitializeComponent();
		}

		protected void	RefreshLobe()
		{
			if ( m_Bitmap == null )
				return;

			int		Center = Width / 2;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				G.FillRectangle( Brushes.White, 0, 0, Width, Height );
				G.DrawLine( Pens.Black, 0, Height-10, Width, Height-10 );
				G.DrawLine( Pens.Black, Center, Height-10, Center, 0 );

				for ( int AngleIndex=0; AngleIndex <= 180; AngleIndex++ )
				{
					float	
				}
			}

			Invalidate();
		}

		protected override void OnResize( EventArgs eventargs )
		{
			base.OnResize( eventargs );

			if ( m_Bitmap != null )
				m_Bitmap.Dispose();

			m_Bitmap = new Bitmap( Width, Height, PixelFormat.Format32bppArgb );

			RefreshLobe();
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
}
