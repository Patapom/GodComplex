// Sources:
//	https://dotnetbrowser.support.teamdev.com/support/solutions/articles/9000109708-saving-web-page-to-png-image
//	https://stackoverflow.com/questions/2715385/convert-webpage-to-image-from-asp-net
//	https://www.codeproject.com/Articles/12629/WYSIWYG-HTML-Editor	<== Old COM issues
//	https://github.com/UweKeim/ZetaHtmlEditControl	<== Currently used
//	https://www.telerik.com/support/kb/winforms/details/how-to-embed-chrome-browser-in-a-winforms-application	<== Chrome in Control
//	https://mkunc.com/2012/02/18/automating-chrome-browser-from-csharp/ <== Driving Chrome Dev Tools
//
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
