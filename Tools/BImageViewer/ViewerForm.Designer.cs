namespace BImageViewer
{
	partial class ViewerForm
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
			this.integerTrackbarControlMipLevel = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// integerTrackbarControlMipLevel
			// 
			this.integerTrackbarControlMipLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.integerTrackbarControlMipLevel.Location = new System.Drawing.Point(1260, 6);
			this.integerTrackbarControlMipLevel.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlMipLevel.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlMipLevel.Name = "integerTrackbarControlMipLevel";
			this.integerTrackbarControlMipLevel.RangeMax = 10;
			this.integerTrackbarControlMipLevel.RangeMin = 0;
			this.integerTrackbarControlMipLevel.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlMipLevel.TabIndex = 0;
			this.integerTrackbarControlMipLevel.Value = 0;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.SystemColors.Control;
			this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.label1.Location = new System.Drawing.Point(1201, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(53, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Mip Level";
			// 
			// ViewerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1467, 859);
			this.ControlBox = false;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.integerTrackbarControlMipLevel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.KeyPreview = true;
			this.Name = "ViewerForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ViewerForm_KeyDown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlMipLevel;
	}
}

