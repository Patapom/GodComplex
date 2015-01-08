namespace ImageSizeChecker
{
	partial class ImageSizeCheckerForm
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
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.buttonCheck = new System.Windows.Forms.Button();
			this.integerTrackbarControlSize = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.richTextBoxResults = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// folderBrowserDialog
			// 
			this.folderBrowserDialog.Description = "Choose a directory to recursively parse for images";
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar.Location = new System.Drawing.Point(12, 40);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(593, 12);
			this.progressBar.TabIndex = 0;
			this.progressBar.Visible = false;
			// 
			// buttonCheck
			// 
			this.buttonCheck.Location = new System.Drawing.Point(12, 12);
			this.buttonCheck.Name = "buttonCheck";
			this.buttonCheck.Size = new System.Drawing.Size(126, 23);
			this.buttonCheck.TabIndex = 1;
			this.buttonCheck.Text = "Check for Images";
			this.buttonCheck.UseVisualStyleBackColor = true;
			this.buttonCheck.Click += new System.EventHandler(this.buttonCheck_Click);
			// 
			// integerTrackbarControlSize
			// 
			this.integerTrackbarControlSize.Location = new System.Drawing.Point(272, 14);
			this.integerTrackbarControlSize.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlSize.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlSize.Name = "integerTrackbarControlSize";
			this.integerTrackbarControlSize.RangeMax = 65536;
			this.integerTrackbarControlSize.RangeMin = 1;
			this.integerTrackbarControlSize.Size = new System.Drawing.Size(233, 20);
			this.integerTrackbarControlSize.TabIndex = 3;
			this.integerTrackbarControlSize.Value = 2048;
			this.integerTrackbarControlSize.VisibleRangeMax = 4096;
			this.integerTrackbarControlSize.VisibleRangeMin = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(144, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(122, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "whose size is larger than";
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Location = new System.Drawing.Point(611, 35);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(80, 20);
			this.buttonCancel.TabIndex = 1;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Visible = false;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// richTextBoxResults
			// 
			this.richTextBoxResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.richTextBoxResults.BackColor = System.Drawing.Color.Gainsboro;
			this.richTextBoxResults.Location = new System.Drawing.Point(12, 59);
			this.richTextBoxResults.Name = "richTextBoxResults";
			this.richTextBoxResults.Size = new System.Drawing.Size(679, 490);
			this.richTextBoxResults.TabIndex = 6;
			this.richTextBoxResults.Text = "";
			this.richTextBoxResults.WordWrap = false;
			this.richTextBoxResults.DoubleClick += new System.EventHandler(this.richTextBoxResults_DoubleClick);
			// 
			// ImageSizeCheckerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(703, 561);
			this.Controls.Add(this.richTextBoxResults);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.integerTrackbarControlSize);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonCheck);
			this.Controls.Add(this.progressBar);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "ImageSizeCheckerForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Image Size Checker";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Button buttonCheck;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlSize;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.RichTextBox richTextBoxResults;
	}
}

