using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Nuaj.Cirrus.Utility
{
	[System.ComponentModel.DefaultEvent( "BitmapUpdating" )]
	public class PanelOutput : Panel
	{
		private Bitmap	m_bitmap = null;

		public delegate void	UpdateBitmapDelegate( int W, int H, Graphics G );

		public event UpdateBitmapDelegate	BitmapUpdating;

		public PanelOutput()
		{
			InitializeComponent();
			UpdateBitmap();
		}

		public PanelOutput( IContainer container )
		{
			container.Add( this );

			InitializeComponent();
			UpdateBitmap();
		}

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
				if ( m_bitmap != null )
					m_bitmap.Dispose();
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

		public void	UpdateBitmap() {

			if ( m_bitmap == null || m_bitmap.Width != Width || m_bitmap.Height != Height ) {
				if ( m_bitmap != null )
					m_bitmap.Dispose();
				m_bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
			}

			using ( Graphics G = Graphics.FromImage( m_bitmap ) )
				if ( BitmapUpdating != null )
					BitmapUpdating( Width, Height, G );
				else
					G.FillRectangle( Brushes.White, 0, 0, Width, Height );

			Invalidate();
		}

		protected override void OnResize( EventArgs eventargs )
		{
			base.OnResize( eventargs );
			UpdateBitmap();
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );	// Don't!
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );
			if ( m_bitmap != null ) {
				e.Graphics.DrawImage( m_bitmap, 0, 0 );
			}
		}
	}
}
