using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Brain2
{
	public class PanelMultiBitmaps : Panel {
		private Bitmap[]		m_bitmaps = new Bitmap[0];
		public Bitmap[]		Bitmaps {
			get { return m_bitmaps; }
			set {
				if ( value == null )
					value = new Bitmap[0];
				if ( value == m_bitmaps )
					return;

				m_bitmaps = value;

				Invalidate();
			}
		}

		public Rectangle	TotalSize {
			get {
				Rectangle	result = new Rectangle();
				result.X = 0;
				result.Y = 0;
				foreach ( Bitmap B in m_bitmaps ) {
					if ( B.Width > result.Width )
						result.Width = B.Width;	// Keep largest width
					result.Height += B.Height;
				}

				return result;
			}
		}

		public PanelMultiBitmaps() {
			InitializeComponent();
			Invalidate();

			Init();
		}

		public PanelMultiBitmaps( IContainer container ) {
			container.Add( this );

			InitializeComponent();
			Invalidate();

			Init();
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
			if ( disposing && (components != null) ) {
				components.Dispose();
			}

			m_brushEmptyPage.Dispose();

			base.Dispose( disposing );
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// PanelMultiBitmaps
			// 
			this.TabStop = true;
			this.ResumeLayout(false);

		}

		SolidBrush	m_brushEmptyPage;
		void		Init() {
			m_brushEmptyPage = new SolidBrush( Color.White );

// 			SetStyle( ControlStyles.Selectable, true );
// 			SetStyle( ControlStyles.ResizeRedraw, true );
// 			SetStyle( ControlStyles.EnableNotifyMessage, true );
// 			SetStyle( ControlStyles.ContainerControl, false );
			this.DoubleBuffered = true;
		}

		#endregion

		protected override void OnResize( EventArgs eventargs ) {
			base.OnResize( eventargs );
			Invalidate();
		}

		protected override void OnMouseClick(MouseEventArgs e) {
			base.OnMouseClick(e);
			this.Parent.Focus();
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);

			Panel	parent = this.Parent as Panel;
			if ( parent == null )
				return;

			Point	scrollPosition = parent.AutoScrollPosition;

			switch ( e.KeyCode ) {
				case Keys.PageUp:	scrollPosition.Y -= 10; break;
				case Keys.PageDown:	scrollPosition.Y += 10; break;
				case Keys.Home:		scrollPosition.Y = 0; break;
				case Keys.End:		scrollPosition.Y = this.Height - parent.Height; break;
			}

			parent.AutoScrollPosition = scrollPosition;
		}

		protected override void OnPaintBackground( PaintEventArgs e ) {
//			base.OnPaintBackground( e );	// Don't!
		}

		protected override void OnPaint( PaintEventArgs e ) {
			base.OnPaint( e );

			if ( m_bitmaps.Length == 0 ) {
				e.Graphics.FillRectangle( m_brushEmptyPage, e.ClipRectangle );
				return;
			}

			int	Y = 0;
			for ( int bitmapIndex=0; bitmapIndex < m_bitmaps.Length; bitmapIndex++ ) {
				Bitmap	B = m_bitmaps[bitmapIndex];

				Rectangle	bitmapRect = new Rectangle( 0, Y, B.Width, B.Height );
				Rectangle	clippedRect = Rectangle.Intersect( bitmapRect, e.ClipRectangle );
				if ( clippedRect.Width > 0 && clippedRect.Height > 0 ) {
					Rectangle	sourceRect = bitmapRect;
								sourceRect.X -= clippedRect.X;
								sourceRect.Y -= clippedRect.Y;

//					e.Graphics.DrawImage( B, clippedRect, sourceRect, GraphicsUnit.Pixel );
					e.Graphics.DrawImage( B, 0, Y );
				}

				Y += B.Height;
			}
		}
	}
}
