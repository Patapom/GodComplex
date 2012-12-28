using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace BRDFSlices
{
	public partial class DisplayPanel : Panel
	{
		protected Bitmap	m_Slice = null;
		public Bitmap	Slice
		{
			get { return m_Slice; }
			set
			{
				m_Slice = value;
				Invalidate();
			}
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

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			if ( m_Slice != null )
				e.Graphics.DrawImage( m_Slice, 0, 0, Width, Height );
		}
	}
}
