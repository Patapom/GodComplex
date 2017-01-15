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
			this.floatTrackbarControlScale = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlOffset = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlRadialScale = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlRadialOffset = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.panelImageSpectrum = new GenerateBlueNoise.PanelImage(this.components);
			this.panelImage = new GenerateBlueNoise.PanelImage(this.components);
			this.floatTrackbarControlDC = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.buttonSolidAngleAlgorithm = new System.Windows.Forms.Button();
			this.labelAnnealingScore = new System.Windows.Forms.Label();
			this.buttonVoidAndCluster = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// floatTrackbarControlScale
			// 
			this.floatTrackbarControlScale.Location = new System.Drawing.Point(49, 531);
			this.floatTrackbarControlScale.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlScale.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlScale.Name = "floatTrackbarControlScale";
			this.floatTrackbarControlScale.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlScale.TabIndex = 2;
			this.floatTrackbarControlScale.Value = 6F;
			this.floatTrackbarControlScale.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScale_ValueChanged);
			// 
			// floatTrackbarControlOffset
			// 
			this.floatTrackbarControlOffset.Location = new System.Drawing.Point(49, 557);
			this.floatTrackbarControlOffset.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlOffset.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlOffset.Name = "floatTrackbarControlOffset";
			this.floatTrackbarControlOffset.RangeMax = 1F;
			this.floatTrackbarControlOffset.RangeMin = 0F;
			this.floatTrackbarControlOffset.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlOffset.TabIndex = 3;
			this.floatTrackbarControlOffset.Value = 0.5F;
			this.floatTrackbarControlOffset.VisibleRangeMax = 1F;
			this.floatTrackbarControlOffset.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlOffset_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 536);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(34, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Scale";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 559);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(27, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Bias";
			// 
			// floatTrackbarControlRadialScale
			// 
			this.floatTrackbarControlRadialScale.Location = new System.Drawing.Point(338, 531);
			this.floatTrackbarControlRadialScale.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlRadialScale.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlRadialScale.Name = "floatTrackbarControlRadialScale";
			this.floatTrackbarControlRadialScale.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlRadialScale.TabIndex = 4;
			this.floatTrackbarControlRadialScale.Value = 1F;
			this.floatTrackbarControlRadialScale.VisibleRangeMax = 2F;
			this.floatTrackbarControlRadialScale.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScale_ValueChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(260, 537);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Radial Range";
			// 
			// floatTrackbarControlRadialOffset
			// 
			this.floatTrackbarControlRadialOffset.Location = new System.Drawing.Point(338, 557);
			this.floatTrackbarControlRadialOffset.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlRadialOffset.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlRadialOffset.Name = "floatTrackbarControlRadialOffset";
			this.floatTrackbarControlRadialOffset.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlRadialOffset.TabIndex = 5;
			this.floatTrackbarControlRadialOffset.Value = 0F;
			this.floatTrackbarControlRadialOffset.VisibleRangeMax = 1F;
			this.floatTrackbarControlRadialOffset.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScale_ValueChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(260, 563);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(68, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "Radial Offset";
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
			// panelImage
			// 
			this.panelImage.Bitmap = null;
			this.panelImage.Location = new System.Drawing.Point(12, 12);
			this.panelImage.Name = "panelImage";
			this.panelImage.Size = new System.Drawing.Size(512, 512);
			this.panelImage.TabIndex = 0;
			this.panelImage.Click += new System.EventHandler(this.panelImage_Click);
			// 
			// floatTrackbarControlDC
			// 
			this.floatTrackbarControlDC.Location = new System.Drawing.Point(633, 531);
			this.floatTrackbarControlDC.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDC.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDC.Name = "floatTrackbarControlDC";
			this.floatTrackbarControlDC.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDC.TabIndex = 6;
			this.floatTrackbarControlDC.Value = 0.5F;
			this.floatTrackbarControlDC.VisibleRangeMax = 1F;
			this.floatTrackbarControlDC.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScale_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(555, 537);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(49, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "DC Term";
			// 
			// buttonSolidAngleAlgorithm
			// 
			this.buttonSolidAngleAlgorithm.Location = new System.Drawing.Point(558, 563);
			this.buttonSolidAngleAlgorithm.Name = "buttonSolidAngleAlgorithm";
			this.buttonSolidAngleAlgorithm.Size = new System.Drawing.Size(152, 23);
			this.buttonSolidAngleAlgorithm.TabIndex = 1;
			this.buttonSolidAngleAlgorithm.Text = "Use Simulated Annealing";
			this.buttonSolidAngleAlgorithm.UseVisualStyleBackColor = true;
			this.buttonSolidAngleAlgorithm.Click += new System.EventHandler(this.buttonSolidAngleAlgorithm_Click);
			// 
			// labelAnnealingScore
			// 
			this.labelAnnealingScore.AutoSize = true;
			this.labelAnnealingScore.Location = new System.Drawing.Point(725, 568);
			this.labelAnnealingScore.Name = "labelAnnealingScore";
			this.labelAnnealingScore.Size = new System.Drawing.Size(61, 13);
			this.labelAnnealingScore.TabIndex = 2;
			this.labelAnnealingScore.Text = "Score: N/A";
			// 
			// buttonVoidAndCluster
			// 
			this.buttonVoidAndCluster.Location = new System.Drawing.Point(558, 592);
			this.buttonVoidAndCluster.Name = "buttonVoidAndCluster";
			this.buttonVoidAndCluster.Size = new System.Drawing.Size(152, 23);
			this.buttonVoidAndCluster.TabIndex = 0;
			this.buttonVoidAndCluster.Text = "Use Void-and-Cluster";
			this.buttonVoidAndCluster.UseVisualStyleBackColor = true;
			this.buttonVoidAndCluster.Click += new System.EventHandler(this.buttonVoidAndCluster_Click);
			// 
			// GenerateBlueNoiseForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1072, 620);
			this.Controls.Add(this.buttonVoidAndCluster);
			this.Controls.Add(this.buttonSolidAngleAlgorithm);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.labelAnnealingScore);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlOffset);
			this.Controls.Add(this.floatTrackbarControlRadialOffset);
			this.Controls.Add(this.floatTrackbarControlDC);
			this.Controls.Add(this.floatTrackbarControlRadialScale);
			this.Controls.Add(this.floatTrackbarControlScale);
			this.Controls.Add(this.panelImageSpectrum);
			this.Controls.Add(this.panelImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "GenerateBlueNoiseForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Blue Noise Generator";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelImage panelImage;
		private PanelImage panelImageSpectrum;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlScale;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlOffset;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlRadialScale;
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlRadialOffset;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDC;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button buttonSolidAngleAlgorithm;
		private System.Windows.Forms.Label labelAnnealingScore;
		private System.Windows.Forms.Button buttonVoidAndCluster;
	}
}

