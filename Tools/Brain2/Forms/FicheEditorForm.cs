using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Brain2 {

	public partial class FicheEditorForm : Form {

		#region CONSTANTS

		public const Keys	SHORTCUT_KEY = Keys.F5;

		#endregion

		#region FIELDS

		private BrainForm	m_owner;

		private Fiche		m_fiche = null;

		private Size		m_relativeLocation = new Size( -1, -1 );

		#endregion

		#region PROPERTIES

		public Fiche		EditedFiche {
			get { return m_fiche; }
			set {
				if ( value == m_fiche )
					return;

				m_fiche = value;

				// Update UI
				bool	enable = m_fiche != null;
				richTextBoxTitle.Enabled = enable;
				richTextBoxTags.Enabled = enable;
				webEditor.Enabled = enable;
			}
		}

		#endregion

		#region METHODS

		public FicheEditorForm( BrainForm _owner ) {
			m_owner = _owner;

			InitializeComponent();

//			webEditor.Document = "Pipo!";

		}

		public new void	Show() {
			base.Show();
			Capture = true;

			if ( m_relativeLocation.Width < 0 ) {
				// Initial condition => Reset to center
				m_relativeLocation.Width = (m_owner.Width - Width) / 2;
				m_relativeLocation.Height = (m_owner.Height - Height) / 2;
			}

			Point	newLocation = m_owner.Location + m_relativeLocation;
			newLocation.X = Math.Max( 0, Math.Min( m_owner.Width - Width, newLocation.X ) );
			newLocation.Y = Math.Max( 0, Math.Min( m_owner.Height - Width, newLocation.Y ) );
			this.Location = newLocation;
		}

		public new void Hide() {
			Capture = false;
			base.Hide();
		}

		public override bool PreProcessMessage(ref Message msg) {

			if ( msg.Msg == Interop.WM_KEYDOWN ) {
				switch ( (Keys) msg.WParam ) {
					case Keys.Escape:
					case SHORTCUT_KEY:
						Hide();
						break;
				}
			}

			return base.PreProcessMessage(ref msg);
		}

// 		protected override bool ProcessKeyPreview(ref Message m) {
// 
// // 			if ( m.Msg == Brain2.BrainForm.WM_KEYDOWN ) {
// // 				switch ( (Keys) m.WParam ) {
// // 					case Keys.Escape:
// // 					case SHORTCUT_KEY:
// // 						Hide();
// // 						break;
// // 				}
// // 			}
// 
// 			return base.ProcessKeyPreview(ref m);
// 		}
// 
// 		protected override void OnKeyDown(KeyEventArgs e) {
// 
// // 			switch ( e.KeyCode ) {
// // 				case Keys.Escape:
// // 				case SHORTCUT_KEY:
// // 					HideWindow();
// // 					break;
// // 
// // 			}
// 
// 			base.OnKeyDown(e);
// 		}

// 		private Point			m_mouseDownFormLocation;
// 		private Point			m_mouseDownPosition;
// 		private MouseButtons	m_mouseDownButtons = MouseButtons.None;

		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
// 			m_mouseDownFormLocation = this.Location;
// 			m_mouseDownPosition = e.Location;
// 			m_mouseDownButtons = e.Button;

			if ( e.Button == MouseButtons.Left ) {
				Interop.ReleaseCapture();
				Interop.SendMessage( Handle, Interop.WM_NCLBUTTONDOWN, Interop.HT_CAPTION, 0 );
			}
		}

// 		protected override void OnMouseMove(MouseEventArgs e) {
// 			if ( m_mouseDownButtons == MouseButtons.Left ) {
// 				// Move form
// 				Size	delta = new Size( e.Location.X - m_mouseDownPosition.X, e.Location.Y - m_mouseDownPosition.Y );
// 				this.Location = m_mouseDownFormLocation + delta;
// 			}
// 
// 			base.OnMouseMove(e);
// 		}
// 
// 		protected override void OnMouseUp(MouseEventArgs e) {
// 			base.OnMouseUp(e);
// 			m_mouseDownButtons = MouseButtons.None;
// 		}

		#endregion

		#region EVENTS

		#endregion
	}
}
