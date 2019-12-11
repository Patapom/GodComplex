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

		public override bool PreProcessMessage(ref Message msg) {

			if ( msg.Msg == Interop.WM_KEYDOWN ) {
				switch ( (Keys) msg.WParam ) {
					case Keys.Escape:
						Hide();
						break;
				}
			}

			return base.PreProcessMessage(ref msg);
		}

		protected override bool ProcessKeyPreview(ref Message m) {

			if ( m.Msg == Interop.WM_KEYDOWN ) {
				switch ( (Keys) m.WParam ) {
					case Keys.Escape:
// 					case SHORTCUT_KEY:
						Hide();
						break;
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
