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
	public partial class BrainForm : Form {
		#region FIELDS

		WebPage2Image	m_webPage2Image;

		#endregion

		#region METHODS

		public BrainForm() {
			InitializeComponent();
			m_webPage2Image = new WebPage2Image( BitmapReady );
		}

		private void buttonGo_Click(object sender, EventArgs e) {
			m_webPage2Image.Generate( textBoxURL.Text, (uint) panelOutput.Width );
		}

		private void	BitmapReady( Bitmap _bitmap ) {
			panelOutput.BackgroundImage = _bitmap;
		}

		private void buttonEdit_Click(object sender, EventArgs e) {
			WebEditorForm	form = new WebEditorForm();
			form.ShowDialog( this );
		}

		#endregion
	}
}
