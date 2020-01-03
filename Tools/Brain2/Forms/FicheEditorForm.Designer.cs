namespace Brain2 {
	partial class FicheEditorForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FicheEditorForm));
			this.label1 = new System.Windows.Forms.Label();
			this.richTextBoxTitle = new System.Windows.Forms.RichTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.richTextBoxTags = new TagEditBox();
			this.webEditor = new ZetaHtmlEditControl.Pom.WebEditor();
			this.richTextBoxURL = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(2, 4);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(78, 29);
			this.label1.TabIndex = 0;
			this.label1.Text = "Fiche";
			// 
			// richTextBoxTitle
			// 
			this.richTextBoxTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.richTextBoxTitle.Enabled = false;
			this.richTextBoxTitle.Location = new System.Drawing.Point(78, 6);
			this.richTextBoxTitle.Multiline = false;
			this.richTextBoxTitle.Name = "richTextBoxTitle";
			this.richTextBoxTitle.Size = new System.Drawing.Size(934, 26);
			this.richTextBoxTitle.TabIndex = 0;
			this.richTextBoxTitle.Text = "";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
			this.label2.Location = new System.Drawing.Point(3, 37);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(45, 20);
			this.label2.TabIndex = 0;
			this.label2.Text = "URL";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
			this.label3.Location = new System.Drawing.Point(3, 68);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(48, 20);
			this.label3.TabIndex = 0;
			this.label3.Text = "Tags";
			// 
			// richTextBoxTags
			// 
			this.richTextBoxTags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.richTextBoxTags.Enabled = false;
			this.richTextBoxTags.Location = new System.Drawing.Point(52, 64);
			this.richTextBoxTags.Name = "richTextBoxTags";
			this.richTextBoxTags.Size = new System.Drawing.Size(960, 26);
			this.richTextBoxTags.TabIndex = 1;
			// 
			// webEditor
			// 
			this.webEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.webEditor.Enabled = false;
			this.webEditor.Location = new System.Drawing.Point(12, 96);
			this.webEditor.Name = "webEditor";
			this.webEditor.Size = new System.Drawing.Size(1000, 614);
			this.webEditor.TabIndex = 2;
			this.webEditor.TempImagesFolder = ((System.IO.DirectoryInfo)(resources.GetObject("webEditor.TempImagesFolder")));
			this.webEditor.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.webEditor_PreviewKeyDown);
			// 
			// richTextBoxURL
			// 
			this.richTextBoxURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.richTextBoxURL.BackColor = System.Drawing.SystemColors.ControlLight;
			this.richTextBoxURL.Enabled = false;
			this.richTextBoxURL.Location = new System.Drawing.Point(52, 36);
			this.richTextBoxURL.Multiline = false;
			this.richTextBoxURL.Name = "richTextBoxURL";
			this.richTextBoxURL.ReadOnly = true;
			this.richTextBoxURL.Size = new System.Drawing.Size(960, 26);
			this.richTextBoxURL.TabIndex = 1;
			this.richTextBoxURL.Text = "";
			this.richTextBoxURL.WordWrap = false;
			this.richTextBoxURL.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextBoxURL_LinkClicked);
			// 
			// FicheEditorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Gainsboro;
			this.ClientSize = new System.Drawing.Size(1024, 720);
			this.Controls.Add(this.webEditor);
			this.Controls.Add(this.richTextBoxURL);
			this.Controls.Add(this.richTextBoxTags);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.richTextBoxTitle);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(1024, 720);
			this.Name = "FicheEditorForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "PreferencesForm";
			this.TopMost = true;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RichTextBox richTextBoxTitle;
		private System.Windows.Forms.Label label2;
		private ZetaHtmlEditControl.Pom.WebEditor webEditor;
		private System.Windows.Forms.Label label3;
		private TagEditBox richTextBoxTags;
		private System.Windows.Forms.RichTextBox richTextBoxURL;
	}
}