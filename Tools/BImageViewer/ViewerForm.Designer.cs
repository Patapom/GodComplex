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
			this.components = new System.ComponentModel.Container();
			this.integerTrackbarControlMipLevel = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.labelEV = new System.Windows.Forms.Label();
			this.floatTrackbarControlEV = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
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
			// labelEV
			// 
			this.labelEV.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelEV.AutoSize = true;
			this.labelEV.BackColor = System.Drawing.SystemColors.Control;
			this.labelEV.ForeColor = System.Drawing.SystemColors.ControlText;
			this.labelEV.Location = new System.Drawing.Point(1210, 32);
			this.labelEV.Name = "labelEV";
			this.labelEV.Size = new System.Drawing.Size(44, 13);
			this.labelEV.TabIndex = 1;
			this.labelEV.Text = "EV Bias";
			this.labelEV.Visible = false;
			// 
			// floatTrackbarControlEV
			// 
			this.floatTrackbarControlEV.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlEV.Location = new System.Drawing.Point(1260, 28);
			this.floatTrackbarControlEV.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlEV.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlEV.Name = "floatTrackbarControlEV";
			this.floatTrackbarControlEV.RangeMax = 10F;
			this.floatTrackbarControlEV.RangeMin = -10F;
			this.floatTrackbarControlEV.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlEV.TabIndex = 2;
			this.floatTrackbarControlEV.Value = 0F;
			this.floatTrackbarControlEV.Visible = false;
			this.floatTrackbarControlEV.VisibleRangeMin = -10F;
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 10;
			// 
			// ViewerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1467, 859);
			this.ControlBox = false;
			this.Controls.Add(this.floatTrackbarControlEV);
			this.Controls.Add(this.labelEV);
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
		private System.Windows.Forms.Label labelEV;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlEV;
		private System.Windows.Forms.Timer timer1;
	}
}

