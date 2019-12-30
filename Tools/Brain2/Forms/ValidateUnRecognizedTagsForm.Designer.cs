namespace Brain2 {
	partial class ValidateUnRecognizedTagsForm {
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
			this.groupBoxNames = new System.Windows.Forms.GroupBox();
			this.checkedListBox = new System.Windows.Forms.CheckedListBox();
			this.labelInfo = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.groupBoxNames.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBoxNames
			// 
			this.groupBoxNames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxNames.Controls.Add(this.checkedListBox);
			this.groupBoxNames.Location = new System.Drawing.Point(12, 85);
			this.groupBoxNames.Name = "groupBoxNames";
			this.groupBoxNames.Size = new System.Drawing.Size(268, 297);
			this.groupBoxNames.TabIndex = 0;
			this.groupBoxNames.TabStop = false;
			this.groupBoxNames.Text = "Tag Names";
			// 
			// checkedListBox
			// 
			this.checkedListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkedListBox.FormattingEnabled = true;
			this.checkedListBox.IntegralHeight = false;
			this.checkedListBox.Location = new System.Drawing.Point(3, 16);
			this.checkedListBox.Name = "checkedListBox";
			this.checkedListBox.Size = new System.Drawing.Size(262, 278);
			this.checkedListBox.TabIndex = 0;
			// 
			// labelInfo
			// 
			this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelInfo.Location = new System.Drawing.Point(12, 9);
			this.labelInfo.Name = "labelInfo";
			this.labelInfo.Size = new System.Drawing.Size(268, 73);
			this.labelInfo.TabIndex = 1;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(205, 388);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 2;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// ValidateUnRecognizedTagsForm
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(292, 418);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.labelInfo);
			this.Controls.Add(this.groupBoxNames);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "ValidateUnRecognizedTagsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Validate New Tag Names";
			this.groupBoxNames.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBoxNames;
		private System.Windows.Forms.Label labelInfo;
		private System.Windows.Forms.CheckedListBox checkedListBox;
		private System.Windows.Forms.Button buttonOK;
	}
}