namespace GenerateSelfShadowedBumpMap
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
			this.floatTrackbarControlHeight = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.integerTrackbarControlRaysCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.buttonGenerate = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLobeExponent = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlPixelDensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlZFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.viewportPanelResult = new GenerateSelfShadowedBumpMap.ImagePanel(this.components);
			this.outputPanelInputHeightMap = new GenerateSelfShadowedBumpMap.ImagePanel(this.components);
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// floatTrackbarControlHeight
			// 
			this.floatTrackbarControlHeight.Location = new System.Drawing.Point(105, 51);
			this.floatTrackbarControlHeight.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlHeight.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlHeight.Name = "floatTrackbarControlHeight";
			this.floatTrackbarControlHeight.RangeMax = 1000F;
			this.floatTrackbarControlHeight.RangeMin = 0.01F;
			this.floatTrackbarControlHeight.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlHeight.TabIndex = 2;
			this.floatTrackbarControlHeight.Value = 2F;
			this.floatTrackbarControlHeight.VisibleRangeMin = 0.01F;
			// 
			// integerTrackbarControlRaysCount
			// 
			this.integerTrackbarControlRaysCount.Location = new System.Drawing.Point(105, 25);
			this.integerTrackbarControlRaysCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlRaysCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlRaysCount.Name = "integerTrackbarControlRaysCount";
			this.integerTrackbarControlRaysCount.RangeMax = 1024;
			this.integerTrackbarControlRaysCount.RangeMin = 1;
			this.integerTrackbarControlRaysCount.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlRaysCount.TabIndex = 1;
			this.integerTrackbarControlRaysCount.Value = 300;
			this.integerTrackbarControlRaysCount.VisibleRangeMax = 1024;
			this.integerTrackbarControlRaysCount.VisibleRangeMin = 1;
			this.integerTrackbarControlRaysCount.SliderDragStop += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.SliderDragStopEventHandler(this.integerTrackbarControlRaysCount_SliderDragStop);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.buttonGenerate);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.integerTrackbarControlRaysCount);
			this.groupBox1.Controls.Add(this.floatTrackbarControlLobeExponent);
			this.groupBox1.Controls.Add(this.floatTrackbarControlPixelDensity);
			this.groupBox1.Controls.Add(this.floatTrackbarControlZFactor);
			this.groupBox1.Controls.Add(this.floatTrackbarControlHeight);
			this.groupBox1.Location = new System.Drawing.Point(530, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(320, 277);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Settings";
			// 
			// buttonGenerate
			// 
			this.buttonGenerate.Location = new System.Drawing.Point(108, 236);
			this.buttonGenerate.Name = "buttonGenerate";
			this.buttonGenerate.Size = new System.Drawing.Size(105, 35);
			this.buttonGenerate.TabIndex = 0;
			this.buttonGenerate.Text = "Generate";
			this.buttonGenerate.UseVisualStyleBackColor = true;
			this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 80);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(76, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Pixel per meter";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 166);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(78, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Lobe exponent";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 192);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(97, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "Z cheating velocity";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 54);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(66, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Height in cm";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 28);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(74, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Rays per Pixel";
			// 
			// floatTrackbarControlLobeExponent
			// 
			this.floatTrackbarControlLobeExponent.Location = new System.Drawing.Point(105, 163);
			this.floatTrackbarControlLobeExponent.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLobeExponent.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLobeExponent.Name = "floatTrackbarControlLobeExponent";
			this.floatTrackbarControlLobeExponent.RangeMax = 1000F;
			this.floatTrackbarControlLobeExponent.RangeMin = 1F;
			this.floatTrackbarControlLobeExponent.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLobeExponent.TabIndex = 2;
			this.floatTrackbarControlLobeExponent.Value = 1F;
			this.floatTrackbarControlLobeExponent.VisibleRangeMin = 1F;
			// 
			// floatTrackbarControlPixelDensity
			// 
			this.floatTrackbarControlPixelDensity.Location = new System.Drawing.Point(105, 77);
			this.floatTrackbarControlPixelDensity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlPixelDensity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlPixelDensity.Name = "floatTrackbarControlPixelDensity";
			this.floatTrackbarControlPixelDensity.RangeMax = 10000F;
			this.floatTrackbarControlPixelDensity.RangeMin = 1F;
			this.floatTrackbarControlPixelDensity.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlPixelDensity.TabIndex = 3;
			this.floatTrackbarControlPixelDensity.Value = 512F;
			this.floatTrackbarControlPixelDensity.VisibleRangeMax = 1024F;
			this.floatTrackbarControlPixelDensity.VisibleRangeMin = 1F;
			// 
			// floatTrackbarControlZFactor
			// 
			this.floatTrackbarControlZFactor.Location = new System.Drawing.Point(105, 189);
			this.floatTrackbarControlZFactor.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlZFactor.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlZFactor.Name = "floatTrackbarControlZFactor";
			this.floatTrackbarControlZFactor.RangeMax = 1F;
			this.floatTrackbarControlZFactor.RangeMin = 0.001F;
			this.floatTrackbarControlZFactor.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlZFactor.TabIndex = 2;
			this.floatTrackbarControlZFactor.Value = 0.1F;
			this.floatTrackbarControlZFactor.VisibleRangeMax = 1F;
			this.floatTrackbarControlZFactor.VisibleRangeMin = 0.001F;
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(530, 295);
			this.progressBar.Maximum = 1000;
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(320, 23);
			this.progressBar.TabIndex = 4;
			// 
			// viewportPanelResult
			// 
			this.viewportPanelResult.Image = null;
			this.viewportPanelResult.Location = new System.Drawing.Point(856, 16);
			this.viewportPanelResult.Name = "viewportPanelResult";
			this.viewportPanelResult.Size = new System.Drawing.Size(512, 512);
			this.viewportPanelResult.TabIndex = 0;
			// 
			// outputPanelInputHeightMap
			// 
			this.outputPanelInputHeightMap.Image = null;
			this.outputPanelInputHeightMap.Location = new System.Drawing.Point(12, 12);
			this.outputPanelInputHeightMap.Name = "outputPanelInputHeightMap";
			this.outputPanelInputHeightMap.Size = new System.Drawing.Size(512, 512);
			this.outputPanelInputHeightMap.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1379, 540);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.viewportPanelResult);
			this.Controls.Add(this.outputPanelInputHeightMap);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ImagePanel outputPanelInputHeightMap;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlHeight;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlRaysCount;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private ImagePanel viewportPanelResult;
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlPixelDensity;
		private System.Windows.Forms.Button buttonGenerate;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLobeExponent;
		private System.Windows.Forms.Label label5;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlZFactor;
		private System.Windows.Forms.ProgressBar progressBar;
	}
}

