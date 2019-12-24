namespace Brain2 {
	partial class FastTaggerForm {
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
			this.labelInfo = new System.Windows.Forms.Label();
			this.richTextBoxTags = new TagEditBox();
			this.SuspendLayout();
			// 
			// labelInfo
			// 
			this.labelInfo.AutoSize = true;
			this.labelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelInfo.Location = new System.Drawing.Point(12, 9);
			this.labelInfo.Name = "labelInfo";
			this.labelInfo.Size = new System.Drawing.Size(114, 13);
			this.labelInfo.TabIndex = 1;
			this.labelInfo.Text = "Enter some Tags...";
			// 
			// richTextBoxTags
			// 
			this.richTextBoxTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.richTextBoxTags.DetectUrls = false;
			this.richTextBoxTags.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
			this.richTextBoxTags.Location = new System.Drawing.Point(12, 25);
			this.richTextBoxTags.Margin = new System.Windows.Forms.Padding(8);
			this.richTextBoxTags.Multiline = false;
			this.richTextBoxTags.Name = "richTextBoxTags";
			this.richTextBoxTags.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
			this.richTextBoxTags.Size = new System.Drawing.Size(676, 42);
			this.richTextBoxTags.TabIndex = 0;
			this.richTextBoxTags.Text = "#Tag";
			this.richTextBoxTags.WordWrap = false;
			// 
			// FastTaggerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(700, 79);
			this.Controls.Add(this.richTextBoxTags);
			this.Controls.Add(this.labelInfo);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximumSize = new System.Drawing.Size(1920, 79);
			this.MinimumSize = new System.Drawing.Size(700, 79);
			this.Name = "FastTaggerForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Complex Tag Names Fixer";
			this.ShowInTaskbar = false;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label labelInfo;
		private TagEditBox richTextBoxTags;
	}
}