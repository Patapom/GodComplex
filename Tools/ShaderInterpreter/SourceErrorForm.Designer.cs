namespace ShaderInterpreter
{
	partial class SourceErrorForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && (components != null) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.richTextBoxErrors = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// richTextBoxErrors
			// 
			this.richTextBoxErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.richTextBoxErrors.Location = new System.Drawing.Point(12, 12);
			this.richTextBoxErrors.Name = "richTextBoxErrors";
			this.richTextBoxErrors.Size = new System.Drawing.Size(673, 485);
			this.richTextBoxErrors.TabIndex = 0;
			this.richTextBoxErrors.Text = "";
			this.richTextBoxErrors.WordWrap = false;
			// 
			// SourceErrorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(697, 509);
			this.Controls.Add(this.richTextBoxErrors);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "SourceErrorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Source Error";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox richTextBoxErrors;
	}
}