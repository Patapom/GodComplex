using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
//using System.Drawing.Imaging;

namespace WaterTankMonitor {
	public partial class PanelWarning : Panel {

		const int	CLOSE_BUTTON_MARGIN = 12;
		const int	CLOSE_BUTTON_SIZE = 32;

		string	m_message;
		public string	Message {
			get => m_message;
			set => m_message = value;
		}

		Font		m_fontWarning = new Font( FontFamily.GenericSansSerif, 32.0f );
		SolidBrush	m_brushWarningText = null;
		public Font	FontWarning {
			get => m_fontWarning;
			set {
				m_fontWarning?.Dispose();
				m_fontWarning = value;
				Invalidate();
			}
		}

		SolidBrush	m_brushBackcolor = null;
		SolidBrush	m_brushBackcolorFlash = new SolidBrush( Color.White );
		public Color	BackColorFlash {
			get => m_brushBackcolorFlash.Color;
			set {
				m_brushBackcolorFlash?.Dispose();
				m_brushBackcolorFlash = new SolidBrush( value );
				
				Invalidate();
			}
		}

		public bool	m_flash = false;

		public PanelWarning() {
			InitializeComponent();
			Init();
		}

		public PanelWarning( IContainer container ) {
			container.Add( this );
			InitializeComponent();
			Init();
		}

		void	Init() {
			m_brushBackcolor = new SolidBrush( BackColor );
			m_brushWarningText = new SolidBrush( ForeColor );
		}

		protected override void OnVisibleChanged( EventArgs e ) {
			base.OnVisibleChanged( e );
			timerFlash.Enabled = Visible;
		}

		protected override void OnBackColorChanged( EventArgs e ) {
			base.OnBackColorChanged( e );
			m_brushBackcolor?.Dispose();
			m_brushBackcolor = new SolidBrush( BackColor );
		}

		protected override void OnForeColorChanged( EventArgs e ) {
			base.OnForeColorChanged( e );
			m_brushWarningText?.Dispose();
			m_brushWarningText = new SolidBrush( ForeColor );
		}

		protected override void OnPaintBackground( PaintEventArgs e ) {
//			base.OnPaintBackground( e );
		}

		Pen	m_penBorder = new Pen( Color.Red, 6 );
		Pen	m_penCloseButton = new Pen( Color.Red, 3 );
		protected override void OnPaint( PaintEventArgs e ) {
			base.OnPaint( e );

			// Draw border
			e.Graphics.FillRectangle( m_flash ? m_brushBackcolorFlash : m_brushBackcolor, 0, 0, Width, Height );
			e.Graphics.DrawRectangle( m_penBorder, 3, 3, Width-6, Height-6 );

			// Draw close button
			int	closeX = Width - CLOSE_BUTTON_MARGIN - CLOSE_BUTTON_SIZE;
			int	closeY = CLOSE_BUTTON_MARGIN;
			e.Graphics.DrawRectangle( m_penCloseButton, closeX, closeY, CLOSE_BUTTON_SIZE, CLOSE_BUTTON_SIZE );
			e.Graphics.DrawLine( m_penCloseButton, closeX, closeY, closeX + CLOSE_BUTTON_SIZE, closeY + CLOSE_BUTTON_SIZE );
			e.Graphics.DrawLine( m_penCloseButton, closeX, closeY + CLOSE_BUTTON_SIZE, closeX + CLOSE_BUTTON_SIZE, closeY );

			// Show big warning sign
			string	strWarning = "WARNING";
			SizeF	sizeWarning = e.Graphics.MeasureString( strWarning, m_fontWarning );
			e.Graphics.DrawString( strWarning, m_fontWarning, m_brushWarningText, 0.5f * (Width - sizeWarning.Width), 0.25f * (Height - sizeWarning.Height) );

			// Show message
			string	strMessage = m_message;
			SizeF	sizeMessage = e.Graphics.MeasureString( strMessage, Font );
			e.Graphics.DrawString( strMessage, Font, m_brushWarningText, 0.5f * (Width - sizeMessage.Width), 0.5f * Height + 0.5f * (0.5f * Height - sizeMessage.Height) );
		}

		protected override void OnMouseDown( MouseEventArgs e ) {
			base.OnMouseDown( e );
			int	closeX = Width - CLOSE_BUTTON_MARGIN - CLOSE_BUTTON_SIZE;
			int	closeY = CLOSE_BUTTON_MARGIN;
			if ( e.X >= closeX && e.X < closeX + CLOSE_BUTTON_SIZE && e.Y >= closeY && e.Y < closeY + CLOSE_BUTTON_SIZE ) {
				Visible = false;	// Close!
			}
		}

		private void timerFlash_Tick( object sender, EventArgs e ) {
			m_flash = !m_flash;
			Invalidate();
		}
	}
}
