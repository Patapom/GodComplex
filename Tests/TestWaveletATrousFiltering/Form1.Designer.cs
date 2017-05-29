namespace TestWaveletATrousFiltering
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.buttonReload = new System.Windows.Forms.Button();
			this.panelOutput = new TestWaveletATrousFiltering.PanelOutput(this.components);
			this.floatTrackbarControlLightSize = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.SuspendLayout();
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 10;
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(1229, 802);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 1;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1280, 720);
			this.panelOutput.TabIndex = 0;
			// 
			// floatTrackbarControlLightSize
			// 
			this.floatTrackbarControlLightSize.Location = new System.Drawing.Point(33, 767);
			this.floatTrackbarControlLightSize.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightSize.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightSize.Name = "floatTrackbarControlLightSize";
			this.floatTrackbarControlLightSize.RangeMax = 10F;
			this.floatTrackbarControlLightSize.RangeMin = 0F;
			this.floatTrackbarControlLightSize.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightSize.TabIndex = 2;
			this.floatTrackbarControlLightSize.Value = 1.05F;
			this.floatTrackbarControlLightSize.VisibleRangeMax = 4F;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1316, 837);
			this.Controls.Add(this.floatTrackbarControlLightSize);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "A-Trous Wavelet Filtering Test";
			this.ResumeLayout(false);

		}

		#endregion

		private PanelOutput panelOutput;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button buttonReload;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightSize;
	}
}

