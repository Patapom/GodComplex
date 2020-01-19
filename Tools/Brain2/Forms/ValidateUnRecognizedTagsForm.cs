using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brain2 {
	public partial class ValidateUnRecognizedTagsForm : Form {
		FichesDB	m_database;

		public ValidateUnRecognizedTagsForm( FichesDB _database, string[] _unRecognizedTagNames ) {
			InitializeComponent();

			m_database = _database;

			labelInfo.Text = "The following tag names are unrecognized: do you want to create them?";

			checkedListBox.SuspendLayout();
			foreach ( string tagName in _unRecognizedTagNames ) {
				checkedListBox.Items.Add( tagName, true );
			}
			checkedListBox.ResumeLayout();
		}

		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);
			if ( DialogResult != DialogResult.OK )
				return;

			// Create fiches for each selected tag
			foreach ( string tag in checkedListBox.CheckedItems ) {
				Fiche	F = m_database.SyncFindOrCreateTagFiche( tag );
				m_database.SyncNotifyFicheModifiedAndNeedsAsyncSaving( F );
			}
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);
			if ( e.KeyCode == Keys.Escape ) {
				DialogResult = DialogResult.Cancel;
				Close();
			}
		}
	}
}
