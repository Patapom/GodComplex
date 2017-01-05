using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GenerateBlueNoise
{
	public partial class PanelImage : Panel {
		private Bitmap	m_Bitmap = null;
		public Bitmap	Bitmap {
			get { return m_Bitmap; }
			set { m_Bitmap = value; Refresh(); }
		}

		public PanelImage()
		{
			InitializeComponent();
		}

		public PanelImage( IContainer container )
		{
			container.Add( this );

			InitializeComponent();
		}

// 		protected override void OnSizeChanged( EventArgs e )
// 		{
// 			if ( m_Bitmap != null )
// 				m_Bitmap.Dispose();
// 
// 			m_Bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
// 			UpdateBitmap();
// 
// 			base.OnSizeChanged( e );
// 		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			if ( m_Bitmap != null )
				e.Graphics.DrawImage( m_Bitmap, 0, 0, Width, Height );
		}
	}
}
