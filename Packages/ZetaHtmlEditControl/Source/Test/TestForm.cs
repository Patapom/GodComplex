namespace Test
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using ZetaHtmlEditControl.Code.Html;

    public partial class TestForm :
		Form
	{
		public TestForm()
		{
			InitializeComponent();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			const string s = @"<P><b>Some tests</b></P><p>Random content. <font color=green>Please edit</font>.</p><p>Use right-click for options.</p>";
			htmlEditControl1.DocumentText = s;
			updateUI();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			const string s = @"<P><b>Some tests</b></P><p>Random content. <font color=red>Also editable</font>.</p>";
			htmlEditControl1.DocumentText = s;
			updateUI();
		}

		private void TestForm_Shown(object sender, EventArgs e)
		{
			const string s = @"<P>Click the buttons below to set different texts. German Umlaute: Ä Ö Ü ä ö ü ß.</p>";
			htmlEditControl1.SetDocumentText(s, @"C:\", true);

			const string s2 = @"<P></p>";
			htmlEditUserControl1.HtmlEditControl.SetDocumentText(s2, @"C:\", true);

			updateUI();
		}

		private void buttonGetHtml_Click(object sender, EventArgs e)
		{
			var s = htmlEditControl1.GetDocumentText(@"C:\", true);

			s += Environment.NewLine;
			s += Environment.NewLine;

			s += "(This text was also copied to the clipboard)";

			MessageBox.Show(
				this,
				s,
				"Zeta Html Edit Control",
				MessageBoxButtons.OK);

			Clipboard.SetText(s, TextDataFormat.Html);

			updateUI();
		}

		private void updateUI()
		{
		}

		private void buttonLoad_Click(object sender, EventArgs e)
		{
			using (var form = new OpenFileDialog())
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					form.CheckFileExists = true;
					form.Filter = "HTML files" + @"|*.html";

					using (var rd = File.OpenText(form.FileName))
					{
						htmlEditControl1.SetDocumentText(
							rd.ReadToEnd(),
							Path.GetDirectoryName(form.FileName),
							true);
					}
				}
			}
		}

		private void buttonSave_Click(object sender, EventArgs e)
		{
			using (var form = new SaveFileDialog())
			{
				form.Filter = "HTML files" + @"|*.html";
				form.CheckPathExists = true;

				if (form.ShowDialog(this) == DialogResult.OK)
				{
					using (var wr = File.CreateText(form.FileName))
					{
						wr.Write(
							htmlEditControl1.GetDocumentText(
								Path.GetDirectoryName(form.FileName),
								true));
					}
				}
			}
		}

		private void ToolbarVisibleCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			htmlEditUserControl1.IsToolbarVisible = ToolbarVisibleCheckBox.Checked;
		}

		private void TestForm_Load(object sender, EventArgs e)
		{
//			WebBrowserHelper.SafeSwitchToHighestInternetExplorerVersionAsync();

			const string html =
				@"<P>Mit Bild:</P>
				<P><IMG 
				src=""http://pseudo-image-folder-path/d0906191-5a75-4568-97d4-924ee727426d""></P>
				<P>Yes!</P>";

			var images = HtmlConversionHelper.GetContainedImageFileNames(html);
			foreach (var image in images)
			{
				Console.WriteLine(image);
			}

		    htmlEditUserControl1.HtmlEditControl.WantCloseDialogWithOK += delegate { MessageBox.Show("Close."); };
		}

		private void button3_Click(object sender, EventArgs e)
		{
			htmlEditUserControl1.HtmlEditControl.DocumentText = "lalala";
			MessageBox.Show(htmlEditUserControl1.HtmlEditControl.DocumentText);
		}
	}
}