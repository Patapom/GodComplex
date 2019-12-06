using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Brain2.Web_Interface {
	public partial class WebEditor : UserControl {

		#region FIELDS

		#endregion

		#region METHODS

		public WebEditor() {
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e) {
			object path = Directory.GetCurrentDirectory() + "\\test.htm";
			object missing = System.Reflection.Missing.Value;
			axWebBrowser1.Navigate2( ref path, ref missing, ref missing, ref missing, ref missing );
			(axWebBrowser1.Document as mshtml.HTMLDocumentClass).designMode = "On";		// This turns the control into an editor
		}

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e) {
			mshtml.HTMLDocumentClass	doc = axWebBrowser1.Document as mshtml.HTMLDocumentClass;

			object empty = null;
			switch (e.Button.ImageIndex) {
				case 10:		// Open
					this.openFileDialog1.ShowDialog();
					break;

				case 0:			// Save
					this.saveFileDialog1.ShowDialog();
					break;

				case 2:		// Cut
					doc.execCommand("Cut" ,false, empty);
					break;

				case 3:		// Copy
					doc.execCommand("Copy" ,false, empty);
					break;

				case 4:		// Paste
					doc.execCommand("Paste" ,false, empty);
					break;

				case 7:		// Bold
					doc.execCommand("Bold" ,false, empty);
					break;

				case 8:		// Italic
					doc.execCommand("Italic" ,false, empty);
					break;

				case 9:		// Underline
					doc.execCommand("Underline" ,false, empty);
					break;

				case 6:		// Undo
					doc.execCommand("Undo" ,false, empty);
					break;

				case 5:		// Redo
					doc.execCommand("Redo" ,false, empty);
					break;
			}

			if (e.Button.Text != "")
			{				
				doc.execCommand("FontSize" ,false, (object)e.Button.Text);
			}
		}

		private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
			try {
				object path = this.openFileDialog1.FileName;
				object missing = System.Reflection.Missing.Value;
				this.axWebBrowser1.Navigate2(ref path, ref missing, ref missing, ref missing, ref missing);		
			} catch(Exception ex) {
				MessageBox.Show(ex.Message, "Error");
				return;
			}
		}

		private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
			try {
				string filename = this.saveFileDialog1.FileName;
				StreamWriter file = new StreamWriter(filename, false, System.Text.Encoding.Unicode);
				mshtml.HTMLDocumentClass doc = (mshtml.HTMLDocumentClass)this.axWebBrowser1.Document;
				string str = "<body>" + doc.IHTMLDocument2_body.innerHTML + "</body>";
				file.Write(str);
				file.Close();
			} catch(Exception ex) {
				MessageBox.Show(ex.Message, "Error");
				return;
			}
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e) {
			try {
				if (this.tabControl1.SelectedIndex == 1) {	// Code View
					mshtml.HTMLDocumentClass doc = (mshtml.HTMLDocumentClass)this.axWebBrowser1.Document;
					this.textBox1.Text = "<body>" + doc.IHTMLDocument2_body.innerHTML + "</body>";
				}
			} catch(Exception ex) {
				MessageBox.Show(ex.Message, "Error");
				return;
			}
		}

		private void linkLabel2_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e) {
			System.Diagnostics.Process.Start("mailto:shlomo.schwarcz@shefertech.com");				
		}

		private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e) {
			System.Diagnostics.Process.Start("http://www.shefertech.com");		
		}

		#endregion
	}
}
