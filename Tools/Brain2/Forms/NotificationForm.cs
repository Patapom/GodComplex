using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Brain2 {
	public partial class NotificationForm : Form {

		#region Public Interface

		public enum NOTIFICATION_TYPE {
			SUCCESS,
			WARNING,
			ERROR,
		}

		public void	NotifyFiche( Fiche _fiche, NOTIFICATION_TYPE _type ) {

			// Register notification
			m_lastNotificationTime = DateTime.Now;

			int	panelsCount = 0;
			switch ( _type ) {
				case NOTIFICATION_TYPE.SUCCESS:	m_successCounter++; panelsCount++; break;
				case NOTIFICATION_TYPE.WARNING:	m_warningCounter++; panelsCount++; break;
				case NOTIFICATION_TYPE.ERROR:	m_errorCounter++; panelsCount++; break;
			}

			// Resize depending on amount of panels we need
			this.Size = new Size( panelsCount * ICON_PANEL_SIZE, ICON_PANEL_SIZE );

			// Center form on screen
			Point	sourcePosition;
			if ( Visible ) {
				sourcePosition = this.Location;			// Next, simply use the screen where the form already is
			} else {
				sourcePosition = Control.MousePosition;	// First popup on the screen where the mouse is
			}
			Screen	containingScreen;
			IntPtr	hMonitor;
			Interop.GetMonitorFromPosition( sourcePosition, out containingScreen, out hMonitor );
			this.Location = new Point( (containingScreen.Bounds.Width - this.Width) / 2, (containingScreen.Bounds.Height - this.Height) / 2 );

			// Ask for a repaint
			Invalidate();

			// Show if not already visible
			if ( !Visible )
				Show( m_owner );
		}

		public void	Animate() {
			if ( !Visible )
				return;

			double	displayTime = (DateTime.Now - m_lastNotificationTime).TotalMilliseconds;
			if ( displayTime < m_displayDuration_ms ) {
				// Fully visible!
				this.Opacity = 1.0;
				return;
			}

			// Start fading
			displayTime -= m_displayDuration_ms;
			if ( displayTime < m_fadeDuration_ms ) {
				this.Opacity = 1.0 - displayTime / m_fadeDuration_ms;
				return;
			}

			// Fade stopped, hide, reset and stay quiet until further notification...
			Hide();

			m_successCounter = 0;
			m_warningCounter = 0;
			m_errorCounter = 0;
		}

		#endregion

		#region Internal Stuff

		const int	ICON_PANEL_SIZE = 256;
		const float	COUNTER_ROUND_CORNER_SIZE = 4.0f;
		const float	COUNTER_MARGIN = 4.0f;

		uint			m_displayDuration_ms = 4000;
		uint			m_fadeDuration_ms = 1000;

		/// <summary>
		/// Gets or sets the duration of display after a notification before fading starts
		/// </summary>
		public uint	DisplayDuration { get { return m_displayDuration_ms; } set { m_displayDuration_ms = value; } }

		/// <summary>
		/// Gets or sets the duration of fading when no notification occurred for a moment
		/// </summary>
		public uint	FadeDuration { get { return m_fadeDuration_ms; } set { m_fadeDuration_ms = value; } }

		BrainForm		m_owner;

		DateTime		m_lastNotificationTime;
		uint			m_successCounter = 0;
		uint			m_warningCounter = 0;
		uint			m_errorCounter = 0;

		SolidBrush		m_backgroundBrush;
		SolidBrush		m_counterBrush;
		SolidBrush		m_counterTextBrush;
		GraphicsPath	m_pathCounter1Digit;
		GraphicsPath	m_pathCounter2Digits;

		public NotificationForm( BrainForm _owner ) {
			m_owner = _owner;
			InitializeComponent();

			m_backgroundBrush = new SolidBrush( this.TransparencyKey );
			m_counterBrush = new SolidBrush( Color.FromArgb( 255, 0, 34 ) );	// Facebook red
			m_counterTextBrush = new SolidBrush( Color.White );					// Facebook white
			m_pathCounter1Digit = CreateRoundedBoxPath( "8" );
			m_pathCounter2Digits = CreateRoundedBoxPath( "88" );

			SetStyle( ControlStyles.ResizeRedraw, true );
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}

			m_pathCounter2Digits.Dispose();
			m_pathCounter1Digit.Dispose();
			m_counterTextBrush.Dispose();
			m_counterBrush.Dispose();
			m_backgroundBrush.Dispose();

			base.Dispose(disposing);
		}

		GraphicsPath	CreateRoundedBoxPath( string _text ) {

			SizeF	textSize = TextRenderer.MeasureText( _text, this.Font );

			GraphicsPath	P = new GraphicsPath();

			P.AddArc( 0, 0, 2*COUNTER_ROUND_CORNER_SIZE, 2*COUNTER_ROUND_CORNER_SIZE, 180, 90 );																					// Top-left
			P.AddArc( COUNTER_ROUND_CORNER_SIZE + textSize.Width, 0, 2*COUNTER_ROUND_CORNER_SIZE, 2*COUNTER_ROUND_CORNER_SIZE, 270, 90 );											// Top-right
			P.AddArc( COUNTER_ROUND_CORNER_SIZE + textSize.Width, COUNTER_ROUND_CORNER_SIZE + textSize.Height, 2*COUNTER_ROUND_CORNER_SIZE, 2*COUNTER_ROUND_CORNER_SIZE,   0, 90 );	// Bottom-right
			P.AddArc( 0, COUNTER_ROUND_CORNER_SIZE + textSize.Height, 2*COUNTER_ROUND_CORNER_SIZE, 2*COUNTER_ROUND_CORNER_SIZE,  90, 90 );											// Bottom-left
			P.CloseFigure();

			return P;
		}

		protected override void OnPaintBackground(PaintEventArgs e) {
//			base.OnPaintBackground(e);
			e.Graphics.FillRectangle( m_backgroundBrush, e.ClipRectangle );
		}

		protected override void OnPaint(PaintEventArgs e) {
			uint	X = 0;
			PaintPanel( e.Graphics, ref X, Properties.Resources.OK256, m_successCounter );
			PaintPanel( e.Graphics, ref X, Properties.Resources.Warning256, m_warningCounter );
			PaintPanel( e.Graphics, ref X, Properties.Resources.Error256, m_errorCounter );

//			base.OnPaint(e);
		}

		void	PaintPanel( Graphics G, ref uint _panelX, Bitmap _icon, uint _counter ) {
			if ( _counter == 0 )
				return;

			// Render the panel
			Rectangle	sourceRect = new Rectangle( 0, 0, _icon.Width, _icon.Height );
			Rectangle	targetRect = new Rectangle( (int) _panelX, 0, ICON_PANEL_SIZE, ICON_PANEL_SIZE );
			G.DrawImage( _icon, targetRect, sourceRect, GraphicsUnit.Pixel );

			// Render the small counter
			if ( _counter > 1 ) {
				string			strCounter = _counter < 100 ? _counter.ToString() : "+99";
				GraphicsPath	path = strCounter.Length == 1 ? m_pathCounter1Digit : m_pathCounter2Digits;
				SizeF			textSize = G.MeasureString( strCounter, this.Font );
				G.TranslateTransform( _panelX + ICON_PANEL_SIZE - (COUNTER_MARGIN-2*COUNTER_ROUND_CORNER_SIZE-textSize.Width), COUNTER_MARGIN );
				G.FillPath( m_counterBrush, m_pathCounter1Digit );
				G.DrawString( strCounter, this.Font, m_counterTextBrush, COUNTER_ROUND_CORNER_SIZE, COUNTER_ROUND_CORNER_SIZE );
				G.ResetTransform();
			}

			_panelX += ICON_PANEL_SIZE;
		}

		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
		}

		protected override void OnActivated(EventArgs e) {
			base.OnActivated(e);
			m_owner.NotifyFormActivated( this );
		}

		protected override void OnDeactivate(EventArgs e) {
			base.OnDeactivate(e);
			m_owner.NotifyFormDeactivated( this );
		}

		// Code from https://stackoverflow.com/questions/21894343/enable-disable-activation-of-a-form
		protected override CreateParams CreateParams {
			get {
				CreateParams	createParams = base.CreateParams;
								createParams.ExStyle |= Interop.WS_EX_NOACTIVATE;
								createParams.ExStyle |= Interop.WS_EX_TRANSPARENT;
				return createParams;
			}
		}
		#endregion
	}
}
