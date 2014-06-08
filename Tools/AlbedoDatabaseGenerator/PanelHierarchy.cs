using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace AlbedoDatabaseGenerator
{
	public partial class PanelHierarchy : Panel
	{
		private int	m_ChildrenCount = 2;
		public int	ChildrenCount
		{
			get { return m_ChildrenCount; }
			set
			{
				m_ChildrenCount = Math.Max( 2, value );
				Invalidate();
			}
		}

		public PanelHierarchy( IContainer container )
		{
			container.Add( this );

			InitializeComponent();
		}

		protected override void OnResize( EventArgs eventargs )
		{
			base.OnResize( eventargs );
			Invalidate();
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			e.Graphics.DrawLine( Pens.Black, 0.5f * Width, 0, 0.5f * Width, Height );
			e.Graphics.DrawLine( Pens.Black, 0, 0.5f * (Height-1), 0.5f * Width, 0.5f * (Height-1) );
			for ( int i=0; i < m_ChildrenCount; i++ )
				e.Graphics.DrawLine( Pens.Black, 0.5f * Width, (float) i / (m_ChildrenCount-1) * (Height-1), Width, (float) i / (m_ChildrenCount-1) * (Height-1) );
		}
	}
}
