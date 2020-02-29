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

		#region NESTED TYPES

		public class BitmapWithRectangle {
			public Rectangle	m_displayRectangle;
			public Bitmap		m_bitmap;
		}

		#endregion

		#region FIELDS

		private BitmapWithRectangle[]	m_bitmaps = new BitmapWithRectangle[0];

		public BitmapWithRectangle[]	Bitmaps {
			get { return m_bitmaps; }
			set {
				if ( value == null )
					value = new BitmapWithRectangle[0];
				if ( value == m_bitmaps )
					return;

				m_bitmaps = value;

				Invalidate();
			}
		}

		public Rectangle	TotalRectangle {
			get {
				int	minX = 10000;
				int	minY = 10000;
				int	maxX = -10000;
				int	maxY = -10000;
				foreach ( BitmapWithRectangle B in m_bitmaps ) {
					minX = Math.Min( minX, B.m_displayRectangle.Left );
					minY = Math.Min( minY, B.m_displayRectangle.Top );
					maxX = Math.Max( maxX, B.m_displayRectangle.Right );
					maxY = Math.Max( maxY, B.m_displayRectangle.Bottom );
				}

				Rectangle	result = new Rectangle( minX, minY, maxX - minX, maxY - minY );
				return result;
			}
		}

		#endregion

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

		protected override void OnPaintBackground( PaintEventArgs e ) {
//			base.OnPaintBackground( e );	// Don't!
		}

		protected override void OnPaint( PaintEventArgs e ) {
			base.OnPaint( e );

			if ( m_bitmaps.Length == 0 ) {
				e.Graphics.FillRectangle( m_brushEmptyPage, e.ClipRectangle );
				return;
			}

//			int	Y = 0;
			for ( int bitmapIndex=0; bitmapIndex < m_bitmaps.Length; bitmapIndex++ ) {
				BitmapWithRectangle	B = m_bitmaps[bitmapIndex];

				if ( B.m_bitmap != null ) {
					e.Graphics.DrawImage( B.m_bitmap, B.m_displayRectangle.Location );
				} else {
					e.Graphics.DrawRectangle( Pens.Red, B.m_displayRectangle );

					string	textError = "Null bitmap...";
					SizeF	textSize = e.Graphics.MeasureString( textError, this.Font );
					e.Graphics.DrawString( textError, this.Font, Brushes.Red, B.m_displayRectangle.Location + new Size( B.m_displayRectangle.Width - (int) textSize.Width, B.m_displayRectangle.Height - (int) textSize.Height ) );
				}
//			if ( image.m_image == null ) {
//				BrainForm.LogError( new Exception( "FicheWebPageAnnotatorForm => Null image file in web page snapshot chunk! Can't create bitmap..." ) );
//				continue;
//			}

// 				Rectangle	bitmapRect = new Rectangle( 0, Y, B.Width, B.Height );
// 				Rectangle	clippedRect = Rectangle.Intersect( bitmapRect, e.ClipRectangle );
// 				if ( clippedRect.Width > 0 && clippedRect.Height > 0 ) {
// 					Rectangle	sourceRect = bitmapRect;
// 								sourceRect.X -= clippedRect.X;
// 								sourceRect.Y -= clippedRect.Y;
// 
// //					e.Graphics.DrawImage( B, clippedRect, sourceRect, GraphicsUnit.Pixel );
// 					e.Graphics.DrawImage( B, 0, Y );
// 				}
// 
// 				Y += B.Height;
			}
		}
	}
}
