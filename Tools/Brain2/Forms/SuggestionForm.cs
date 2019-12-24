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
	public partial class SuggestionForm : Form {
		public event EventHandler	SuggestionSelected;
		public int					SelectedSuggestionIndex { get { return listBox.SelectedIndex; } }

		public bool	IsSuggesting { get { return Visible && listBox.SelectedItem != null; } }

		public SuggestionForm() {
			InitializeComponent();
		}

		public void	AcceptSuggestion() {
			if ( listBox.SelectedItem == null )
				return;

			Hide();
			if ( SuggestionSelected != null )
				SuggestionSelected( this, EventArgs.Empty );
		}

		public void	UpdateList( string[] _suggestions, int _maxResults ) {
			listBox.SuspendLayout();
			listBox.Items.Clear();
			listBox.Items.AddRange( _suggestions );
			listBox.ResumeLayout();

			float	fontSizePixels = 4 * listBox.Font.SizeInPoints / 3;
			this.Height = (int) (Math.Min( _maxResults, _suggestions.Length ) * Math.Ceiling( fontSizePixels + 4 ));
//			this.Height = (int) (Math.Min( _maxResults, _suggestions.Length ) * Math.Ceiling( 16*listBox.Font.Size + 1 ));
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			if ( e.KeyCode == Keys.Escape ) {
//				DialogResult = DialogResult.Cancel;
				Hide();
			} else if ( e.KeyCode == Keys.Return ) {
				if ( listBox.SelectedItem != null )
					AcceptSuggestion();
//					listBox_DoubleClick( listBox, EventArgs.Empty );	// Simulate a selection
			}

			base.OnKeyDown(e);
		}

		private void listBox_DoubleClick(object sender, EventArgs e) {
//			DialogResult = DialogResult.OK;
			AcceptSuggestion();
		}
	}
}
