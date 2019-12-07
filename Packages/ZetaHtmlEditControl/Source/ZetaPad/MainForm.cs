namespace Test
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using ZetaHtmlEditControl.Code.Configuration;
    using ZetaHtmlEditControl.UI.EditControlDerives;

    public partial class MainForm : Form
    {
        string DocumentFile;
        int DocumentTextHashCode;

        public MainForm()
        {
            InitializeComponent();
            AllowDrop = true;
            DragDrop += (sender, e) =>
                {
                    if (e.Data is DataObject && ((DataObject)e.Data).ContainsFileDropList())
                        foreach (string filePath in ((DataObject)e.Data).GetFileDropList())
                        {
                            LoadFile(filePath);
                            break;
                        }
                };
            DragEnter += (sender, e) =>
                {
                    if (e.Data != null && ((DataObject)e.Data).ContainsFileDropList())
                        e.Effect = DragDropEffects.Move;
                };

            Text = "ZetaPad - Untitled";

            //htmlEditUserControl1.IE10RenderingMode = false; //for testing only

            htmlEditUserControl1.ToolStrip.Items.RemoveAt(0);

            Action<int, ToolStripMenuItem> DuplicateAsToolbarButton =
                (index, menuItem) =>
                {
                    var button = new ToolStripButton { Image = menuItem.Image };
                    button.Click += (s, e) => menuItem.PerformClick();
                    button.ToolTipText = menuItem.ToolTipText;
                    htmlEditUserControl1.ToolStrip.Items.Insert(index, button);
                };

            DuplicateAsToolbarButton(0, printToolStripMenuItem);
            DuplicateAsToolbarButton(0, printPreviewToolStripMenuItem);
            htmlEditUserControl1.ToolStrip.Items.Insert(0, new ToolStripSeparator());
            DuplicateAsToolbarButton(0, saveToolStripMenuItem);
            DuplicateAsToolbarButton(0, openToolStripMenuItem);
            DuplicateAsToolbarButton(0, newToolStripMenuItem);
        }

        HtmlEditControl HtmlEditControl
        {
            get
            {
                return htmlEditUserControl1.HtmlEditControl;
            }
        }

        string DocumentText
        {
            get
            {
                //return HtmlEditControl.CompleteDocumentText;
                
                string dir = @"C:\";
                if (DocumentFile != null)
                    dir = Path.GetDirectoryName(DocumentFile);

                return HtmlEditControl.GetDocumentText(dir, true);
                //return HtmlEditControl.DocumentText;
            }
        }

        public void SelectWord()
        {
            SendKeys.SendWait("^{LEFT}");
            SendKeys.SendWait("^+{RIGHT}");
        }

        public void OnMouseDblClick()
        {
            //explicit word selection is only needed for the default IE10 rendering mode
            if (htmlEditUserControl1.IE10RenderingMode)
                SelectWord();
        }

        private void TestForm_Shown(object sender, EventArgs e)
        {
            var configuration = new HtmlEditControlConfiguration
                {
                    AllowEmbeddedImages = true,
                    AllowFontChange = true,
                    AllowPrint = true
                };
            HtmlEditControl.Configure(configuration);
            New();
        }

        void New()
        {
            HtmlEditControl.SetDocumentText("<p></p>", @"C:\", true);

            //DocumentTextHashCode = DocumentText.GetHashCode();
            //The code above does not work.
            //At this stage the document is not finalized and DocumentText does not have
            //yet it's final value ("empty text"), which is in HTML format is "\r\n<P></P>"
            DocumentTextHashCode = "\r\n<P></P>".GetHashCode();
        }

        private void TestForm_Load(object sender, EventArgs e)
        {
            LoadHistory();
            //HtmlEditControl.WantCloseDialogWithOK += delegate { MessageBox.Show("Close."); };
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = NeedToSaveDocument();

            if (result == DialogResult.Yes)
                Save(DocumentFile);
            else if (result == DialogResult.Cancel)
                return;

            Open();
        }

        void Open()
        {
            using (var form = new OpenFileDialog())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    form.CheckFileExists = true;
                    form.Filter = "HTML files" + @"|*.html";
                    LoadFile(form.FileName);
                }
            }
        }

        void LoadFile(string file)
        {
            string text = File.ReadAllText(file);
            HtmlEditControl.SetDocumentText(text, Path.GetDirectoryName(file), true);
            DocumentTextHashCode = DocumentText.GetHashCode();
            DocumentFile = file;
            Text = "ZetaPad - " + DocumentFile;
            AddToHistory(file);
        }

        void SaveFile(string file)
        {
            //File.WriteAllText(file, DocumentText);
            File.WriteAllText(file, HtmlEditControl.CompleteDocumentText);
            DocumentTextHashCode = DocumentText.GetHashCode();
            DocumentFile = file;
            Text = "ZetaPad - " + DocumentFile;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save(DocumentFile);
        }

        void Save(string file)
        {
            if (file == null)
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Filter = "HTML files" + @"|*.html";
                    dialog.CheckPathExists = true;

                    if (dialog.ShowDialog(this) == DialogResult.OK)
                        SaveFile(dialog.FileName);
                }
            else
                SaveFile(file);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = NeedToSaveDocument();

            if (result == DialogResult.Yes)
                Save(DocumentFile);
            else if (result == DialogResult.Cancel)
                return;

            New();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save(null);
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HtmlEditControl.ExecutePrint();
        }

        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HtmlEditControl.ExecutePrintPreview();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = NeedToSaveDocument();

            if (result == DialogResult.Yes)
                Save(DocumentFile);
            else if (result == DialogResult.Cancel)
                e.Cancel = true;
        }

        DialogResult NeedToSaveDocument()
        {
            if (DocumentTextHashCode != DocumentText.GetHashCode())
            {
                string message = "Do you want to save changes to " + (DocumentFile ?? "Untitled") + "?";

                return MessageBox.Show(message, "ZetaPad", MessageBoxButtons.YesNoCancel);
            }
            else
                return DialogResult.No;
        }

        void LoadHistory()
        {
        }

        void ClearHistory()
        {
        }

        void AddToHistory(string file)
        {
        }
    }
}