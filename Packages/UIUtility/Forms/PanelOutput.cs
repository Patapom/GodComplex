using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace UIUtility
{
	[System.ComponentModel.DefaultEvent( "BitmapUpdating" )]
	public class PanelOutput : Panel
	{
		private Bitmap	m_bitmap = null;
		private bool	m_internalBitmap = true;

		public delegate void	UpdateBitmapDelegate( int W, int H, Graphics G );

		public Bitmap	PanelBitmap {
			get { return m_bitmap; }
			set {
				if ( value == m_bitmap )
					return;

				m_bitmap = value;
				m_internalBitmap = m_bitmap != null;
			}
		}

		public event UpdateBitmapDelegate	BitmapUpdating;

		public PanelOutput() {
			InitializeComponent();
			SetStyle( ControlStyles.ResizeRedraw, true );
			SetStyle( ControlStyles.Selectable, true );
			UpdateBitmap();
		}

		public PanelOutput( IContainer container ) {
			container.Add( this );
			InitializeComponent();
			SetStyle( ControlStyles.ResizeRedraw, true );
			SetStyle( ControlStyles.Selectable, true );
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
			this.DoubleBuffered = true;
		}

		#endregion

		public void	UpdateBitmap() {

			if ( m_bitmap == null || m_bitmap.Width != Width || m_bitmap.Height != Height ) {
				if ( m_bitmap != null )
					m_bitmap.Dispose();
				m_bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
			}

			if ( m_internalBitmap ) {
				using ( Graphics G = Graphics.FromImage( m_bitmap ) )
					if ( BitmapUpdating != null )
						BitmapUpdating( Width, Height, G );
					else
						G.FillRectangle( Brushes.White, 0, 0, Width, Height );
			}

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
