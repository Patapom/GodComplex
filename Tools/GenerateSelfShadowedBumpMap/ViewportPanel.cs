using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace GenerateSelfShadowedBumpMap
{
	public partial class ViewportPanel : Panel
	{
		#region FIELDS

		private RendererManaged.Device	m_Device;

		#endregion

		#region PROPERTIES

		public RendererManaged.Device	Device
		{
			get { return m_Device; }
			set { m_Device = value; }
		}

		#endregion

		#region METHODS

		public ViewportPanel( IContainer container )
		{
			container.Add( this );
			InitializeComponent();
			OnSizeChanged( EventArgs.Empty );
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			Invalidate();
			base.OnSizeChanged( e );
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
//			base.OnPaint( e );

			if ( m_Device != null )
				m_Device.Present();
			else
				e.Graphics.FillRectangle( Brushes.Black, 0, 0, Width, Height );
		}

		#endregion
	}
}
