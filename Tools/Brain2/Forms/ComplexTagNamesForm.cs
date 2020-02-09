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
	public partial class ComplexTagNamesForm : Form {
		public ComplexTagNamesForm( Fiche[] _complexTagFiches ) {
			InitializeComponent();

			this.labelInfo.Text = @"Bookmark folders are transformed into #Tags but some folder have names that were deemed too complex.
You are encouraged to rename these folders into simpler names more appropriate for tags.
Of course, there is no obligation to do that as you can use arbitrary tag names, any degree of complexity is acceptable and tag simplification is only a suggestion.";



			listViewTagNames.SuspendLayout();
			foreach ( Fiche fiche in _complexTagFiches ) {
				ListViewItem	item = new ListViewItem( fiche.Title );
								item.Tag = fiche;
				listViewTagNames.Items.Add( item );
			}
			listViewTagNames.ResumeLayout();
		}

		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);

			// Save fiches that changed
			foreach ( ListViewItem item in listViewTagNames.Items ) {
				Fiche	fiche = item.Tag as Fiche;
				if ( item.Text == fiche.Title )
					continue;	// No change...

				// Update title & request saving
				fiche.Title = item.Text;
				fiche.Database.Async_NotifyFicheModifiedAndNeedsAsyncSaving( fiche );
			}
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);
			if ( e.KeyCode == Keys.Escape )
				Close();
		}

		private void listViewTagNames_AfterLabelEdit(object sender, LabelEditEventArgs e) {
// 			ListViewItem	item = listViewTagNames.Items[e.Item];
// 			Fiche	F = item.Tag as Fiche;
// 					F.Title = e.Label;
		}
	}
}
