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

	public partial class FicheEditorForm : ModelessForm {

		#region CONSTANTS

		#endregion

		#region FIELDS

		private Fiche		m_fiche = null;

		#endregion

		#region PROPERTIES

		public override Keys SHORTCUT_KEY => Keys.F5;

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

		public FicheEditorForm( BrainForm _owner ) : base( _owner ) {
			InitializeComponent();

//			webEditor.Document = "Pipo!";
		}

// 		protected override bool ProcessKeyMessage(ref Message m) {
// 			return base.ProcessKeyMessage(ref m);
// 		}
// 
// 		protected override void DefWndProc(ref Message m) {
// 			if ( msg.Msg == Interop.WM_KEYDOWN ) {
// 			}
// 
// 			base.DefWndProc(ref m);
// 		}
// 
// 		public override bool PreProcessMessage(ref Message msg) {
// 
// 			if ( msg.Msg == Interop.WM_KEYDOWN ) {
// 				switch ( (Keys) msg.WParam ) {
// 					case Keys.Escape:
// 						Hide();
// 						break;
// 				}
// 			}
// 
// 			return base.PreProcessMessage(ref msg);
// 		}

		private void webEditor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
			if ( e.KeyCode == Keys.Escape || e.KeyCode == SHORTCUT_KEY ) {
				Hide();
			}
		}

		protected override bool ProcessKeyPreview(ref Message m) {
			if ( m.Msg == Interop.WM_KEYDOWN ) {
				Keys	key = (Keys) m.WParam;
				if ( key == Keys.Escape || key == SHORTCUT_KEY ) {
					Hide();
				}
			}

			return base.ProcessKeyPreview(ref m);
		}

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

		#endregion

		#region EVENTS

		#endregion
	}
}
