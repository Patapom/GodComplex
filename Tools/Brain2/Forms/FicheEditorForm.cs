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
//	public partial class FicheEditorForm : Form {

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
				richTextBoxTitle.Text = enable ? m_fiche.m_title : "";

				richTextBoxTags.Enabled = enable;
				richTextBoxTags.Text = enable ? "@TODO: handle parents as tags" : "";

				webEditor.Document = enable ? m_fiche.m_HTMLContent : "<body/>";
				webEditor.Enabled = enable;
			}
		}

		#endregion

		#region METHODS

		public FicheEditorForm( BrainForm _owner ) : base( _owner ) {
			m_sizeable = true;
			InitializeComponent();

//			webEditor.Document = "Pipo!";
		}

		private void webEditor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
			if ( e.KeyCode == Keys.Escape || e.KeyCode == SHORTCUT_KEY ) {
				Hide();
			}
		}

		#endregion

		#region EVENTS

		#endregion
	}
}
