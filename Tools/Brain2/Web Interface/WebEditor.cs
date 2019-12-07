using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brain2.Web_Interface {
	public partial class WebEditor : UserControl {
		public WebEditor() {
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			const string s = @"<P>Click the buttons below to set different texts. German Umlaute: Ä Ö Ü ä ö ü ß.</p>";
			htmlEditUserControl1.HtmlEditControl.SetDocumentText(s, @"C:\", true);
		}

		private void toolStripButton1_Click(object sender, EventArgs e) {

		}
	}
}
