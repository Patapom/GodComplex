using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace LTCTableGenerator
{
	public class PanelOutput : Panel
	{
		public PanelOutput()
		{
			InitializeComponent();
		}

		public PanelOutput( IContainer container )
		{
			container.Add( this );

			InitializeComponent();
		}

		public Bitmap	m_bitmap = null;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && (components != null) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}

		#endregion

		protected override void OnResize( EventArgs eventargs )
		{
			base.OnResize( eventargs );
			Invalidate();
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );	// Don't!
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );
			if ( m_bitmap == null )
				return;

			e.Graphics.DrawImage( m_bitmap, 0, 0, new Rectangle( 0, 0, m_bitmap.Width, m_bitmap.Height ), GraphicsUnit.Pixel );
		}
	}
}
