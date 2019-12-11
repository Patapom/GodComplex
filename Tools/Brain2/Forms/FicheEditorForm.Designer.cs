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
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.label1 = new System.Windows.Forms.Label();
			this.richTextBoxTitle = new System.Windows.Forms.RichTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.webEditor = new ZetaHtmlEditControl.Pom.WebEditor();
			this.label3 = new System.Windows.Forms.Label();
			this.richTextBoxTags = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// folderBrowserDialog
			// 
			this.folderBrowserDialog.Description = "Choose a folder for the database of fiches";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(78, 29);
			this.label1.TabIndex = 0;
			this.label1.Text = "Fiche";
			// 
			// richTextBoxTitle
			// 
			this.richTextBoxTitle.Location = new System.Drawing.Point(52, 32);
			this.richTextBoxTitle.Multiline = false;
			this.richTextBoxTitle.Name = "richTextBoxTitle";
			this.richTextBoxTitle.Size = new System.Drawing.Size(632, 26);
			this.richTextBoxTitle.TabIndex = 1;
			this.richTextBoxTitle.Text = "";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
			this.label2.Location = new System.Drawing.Point(3, 36);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(43, 20);
			this.label2.TabIndex = 0;
			this.label2.Text = "Title";
			// 
			// webEditor
			// 
			this.webEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.webEditor.Location = new System.Drawing.Point(5, 96);
			this.webEditor.Name = "webEditor";
			this.webEditor.Size = new System.Drawing.Size(686, 403);
			this.webEditor.TabIndex = 2;
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
			this.richTextBoxTags.Location = new System.Drawing.Point(52, 64);
			this.richTextBoxTags.Multiline = false;
			this.richTextBoxTags.Name = "richTextBoxTags";
			this.richTextBoxTags.Size = new System.Drawing.Size(632, 26);
			this.richTextBoxTags.TabIndex = 1;
			this.richTextBoxTags.Text = "";
			// 
			// FicheEditorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlDark;
			this.ClientSize = new System.Drawing.Size(696, 504);
			this.Controls.Add(this.webEditor);
			this.Controls.Add(this.richTextBoxTags);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.richTextBoxTitle);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.KeyPreview = true;
			this.Name = "FicheEditorForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "PreferencesForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RichTextBox richTextBoxTitle;
		private System.Windows.Forms.Label label2;
		private ZetaHtmlEditControl.Pom.WebEditor webEditor;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.RichTextBox richTextBoxTags;
	}
}