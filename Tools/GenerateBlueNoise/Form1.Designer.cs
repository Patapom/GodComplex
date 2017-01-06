namespace GenerateBlueNoise
{
	partial class GenerateBlueNoiseForm
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
			this.panelImage = new GenerateBlueNoise.PanelImage(this.components);
			this.panelImageSpectrum = new GenerateBlueNoise.PanelImage(this.components);
			this.SuspendLayout();
			// 
			// panelImage
			// 
			this.panelImage.Bitmap = null;
			this.panelImage.Location = new System.Drawing.Point(12, 12);
			this.panelImage.Name = "panelImage";
			this.panelImage.Size = new System.Drawing.Size(512, 512);
			this.panelImage.TabIndex = 0;
			this.panelImage.Click += new System.EventHandler(this.panelImage_Click);
			// 
			// panelImageSpectrum
			// 
			this.panelImageSpectrum.Bitmap = null;
			this.panelImageSpectrum.Location = new System.Drawing.Point(548, 12);
			this.panelImageSpectrum.Name = "panelImageSpectrum";
			this.panelImageSpectrum.Size = new System.Drawing.Size(512, 512);
			this.panelImageSpectrum.TabIndex = 0;
			this.panelImageSpectrum.Click += new System.EventHandler(this.panelImageSpectrum_Click);
			// 
			// GenerateBlueNoiseForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1072, 595);
			this.Controls.Add(this.panelImageSpectrum);
			this.Controls.Add(this.panelImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "GenerateBlueNoiseForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Blue Noise Generator";
			this.ResumeLayout(false);

		}

		#endregion

		private PanelImage panelImage;
		private PanelImage panelImageSpectrum;
	}
}

