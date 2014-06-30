namespace GenerateSelfShadowedBumpMap
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

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
			this.checkBoxWrap = new System.Windows.Forms.CheckBox();
			this.buttonGenerate = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.integerTrackbarControlMaxStepsCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.floatTrackbarControlPixelDensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlBilateralTolerance = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlBilateralRadius = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlAOFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.radioButtonShowDirOccRGB = new System.Windows.Forms.RadioButton();
			this.radioButtonDirOccR = new System.Windows.Forms.RadioButton();
			this.radioButtonDirOccG = new System.Windows.Forms.RadioButton();
			this.radioButtonDirOccB = new System.Windows.Forms.RadioButton();
			this.radioButtonAO = new System.Windows.Forms.RadioButton();
			this.radioButtonDirOccRGBtimeAO = new System.Windows.Forms.RadioButton();
			this.openFileDialogImage = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogImage = new System.Windows.Forms.SaveFileDialog();
			this.radioButtonAOfromRGB = new System.Windows.Forms.RadioButton();
			this.viewportPanelResult = new GenerateSelfShadowedBumpMap.ImagePanel(this.components);
			this.outputPanelInputHeightMap = new GenerateSelfShadowedBumpMap.ImagePanel(this.components);
			this.checkBoxShowsRGB = new System.Windows.Forms.CheckBox();
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
			this.floatTrackbarControlHeight.Value = 10F;
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
			this.groupBox1.Controls.Add(this.checkBoxWrap);
			this.groupBox1.Controls.Add(this.buttonGenerate);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.integerTrackbarControlMaxStepsCount);
			this.groupBox1.Controls.Add(this.integerTrackbarControlRaysCount);
			this.groupBox1.Controls.Add(this.floatTrackbarControlPixelDensity);
			this.groupBox1.Controls.Add(this.floatTrackbarControlBilateralTolerance);
			this.groupBox1.Controls.Add(this.floatTrackbarControlBilateralRadius);
			this.groupBox1.Controls.Add(this.floatTrackbarControlHeight);
			this.groupBox1.Enabled = false;
			this.groupBox1.Location = new System.Drawing.Point(530, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(320, 275);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Settings";
			// 
			// checkBoxWrap
			// 
			this.checkBoxWrap.AutoSize = true;
			this.checkBoxWrap.Checked = true;
			this.checkBoxWrap.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxWrap.Location = new System.Drawing.Point(9, 207);
			this.checkBoxWrap.Name = "checkBoxWrap";
			this.checkBoxWrap.Size = new System.Drawing.Size(43, 17);
			this.checkBoxWrap.TabIndex = 4;
			this.checkBoxWrap.Text = "Tile";
			this.checkBoxWrap.UseVisualStyleBackColor = true;
			// 
			// buttonGenerate
			// 
			this.buttonGenerate.Location = new System.Drawing.Point(105, 234);
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
			this.label3.Size = new System.Drawing.Size(81, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Pixels per meter";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(6, 169);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(95, 13);
			this.label7.TabIndex = 3;
			this.label7.Text = "Bilateral Tolerance";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 143);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(80, 13);
			this.label6.TabIndex = 3;
			this.label6.Text = "Bilateral Radius";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(536, 507);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(55, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "AO Factor";
			this.label5.Visible = false;
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
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 106);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(88, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Max Steps Count";
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
			// integerTrackbarControlMaxStepsCount
			// 
			this.integerTrackbarControlMaxStepsCount.Location = new System.Drawing.Point(105, 103);
			this.integerTrackbarControlMaxStepsCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlMaxStepsCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlMaxStepsCount.Name = "integerTrackbarControlMaxStepsCount";
			this.integerTrackbarControlMaxStepsCount.RangeMax = 400;
			this.integerTrackbarControlMaxStepsCount.RangeMin = 1;
			this.integerTrackbarControlMaxStepsCount.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlMaxStepsCount.TabIndex = 1;
			this.integerTrackbarControlMaxStepsCount.Value = 100;
			this.integerTrackbarControlMaxStepsCount.VisibleRangeMax = 200;
			this.integerTrackbarControlMaxStepsCount.VisibleRangeMin = 1;
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
			// floatTrackbarControlBilateralTolerance
			// 
			this.floatTrackbarControlBilateralTolerance.Location = new System.Drawing.Point(105, 166);
			this.floatTrackbarControlBilateralTolerance.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBilateralTolerance.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBilateralTolerance.Name = "floatTrackbarControlBilateralTolerance";
			this.floatTrackbarControlBilateralTolerance.RangeMax = 1F;
			this.floatTrackbarControlBilateralTolerance.RangeMin = 0F;
			this.floatTrackbarControlBilateralTolerance.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlBilateralTolerance.TabIndex = 2;
			this.floatTrackbarControlBilateralTolerance.Value = 0.2F;
			this.floatTrackbarControlBilateralTolerance.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlBilateralRadius
			// 
			this.floatTrackbarControlBilateralRadius.Location = new System.Drawing.Point(105, 140);
			this.floatTrackbarControlBilateralRadius.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBilateralRadius.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBilateralRadius.Name = "floatTrackbarControlBilateralRadius";
			this.floatTrackbarControlBilateralRadius.RangeMax = 32F;
			this.floatTrackbarControlBilateralRadius.RangeMin = 0.001F;
			this.floatTrackbarControlBilateralRadius.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlBilateralRadius.TabIndex = 2;
			this.floatTrackbarControlBilateralRadius.Value = 10F;
			this.floatTrackbarControlBilateralRadius.VisibleRangeMax = 32F;
			this.floatTrackbarControlBilateralRadius.VisibleRangeMin = 0.001F;
			// 
			// floatTrackbarControlAOFactor
			// 
			this.floatTrackbarControlAOFactor.Location = new System.Drawing.Point(635, 504);
			this.floatTrackbarControlAOFactor.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlAOFactor.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlAOFactor.Name = "floatTrackbarControlAOFactor";
			this.floatTrackbarControlAOFactor.RangeMax = 10000F;
			this.floatTrackbarControlAOFactor.RangeMin = 0.001F;
			this.floatTrackbarControlAOFactor.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlAOFactor.TabIndex = 2;
			this.floatTrackbarControlAOFactor.Value = 10F;
			this.floatTrackbarControlAOFactor.Visible = false;
			this.floatTrackbarControlAOFactor.VisibleRangeMax = 100F;
			this.floatTrackbarControlAOFactor.VisibleRangeMin = 0.001F;
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(530, 293);
			this.progressBar.Maximum = 1000;
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(320, 23);
			this.progressBar.TabIndex = 4;
			// 
			// radioButtonShowDirOccRGB
			// 
			this.radioButtonShowDirOccRGB.AutoSize = true;
			this.radioButtonShowDirOccRGB.Checked = true;
			this.radioButtonShowDirOccRGB.Location = new System.Drawing.Point(530, 322);
			this.radioButtonShowDirOccRGB.Name = "radioButtonShowDirOccRGB";
			this.radioButtonShowDirOccRGB.Size = new System.Drawing.Size(151, 17);
			this.radioButtonShowDirOccRGB.TabIndex = 5;
			this.radioButtonShowDirOccRGB.TabStop = true;
			this.radioButtonShowDirOccRGB.Text = "Directional Occlusion RGB";
			this.radioButtonShowDirOccRGB.UseVisualStyleBackColor = true;
			this.radioButtonShowDirOccRGB.CheckedChanged += new System.EventHandler(this.radioButtonShowDirOccRGB_CheckedChanged);
			// 
			// radioButtonDirOccR
			// 
			this.radioButtonDirOccR.AutoSize = true;
			this.radioButtonDirOccR.Location = new System.Drawing.Point(530, 368);
			this.radioButtonDirOccR.Name = "radioButtonDirOccR";
			this.radioButtonDirOccR.Size = new System.Drawing.Size(148, 17);
			this.radioButtonDirOccR.TabIndex = 5;
			this.radioButtonDirOccR.Text = "Directional Occlusion Red";
			this.radioButtonDirOccR.UseVisualStyleBackColor = true;
			this.radioButtonDirOccR.CheckedChanged += new System.EventHandler(this.radioButtonDirOccR_CheckedChanged);
			// 
			// radioButtonDirOccG
			// 
			this.radioButtonDirOccG.AutoSize = true;
			this.radioButtonDirOccG.Location = new System.Drawing.Point(530, 391);
			this.radioButtonDirOccG.Name = "radioButtonDirOccG";
			this.radioButtonDirOccG.Size = new System.Drawing.Size(157, 17);
			this.radioButtonDirOccG.TabIndex = 5;
			this.radioButtonDirOccG.Text = "Directional Occlusion Green";
			this.radioButtonDirOccG.UseVisualStyleBackColor = true;
			this.radioButtonDirOccG.CheckedChanged += new System.EventHandler(this.radioButtonDirOccG_CheckedChanged);
			// 
			// radioButtonDirOccB
			// 
			this.radioButtonDirOccB.AutoSize = true;
			this.radioButtonDirOccB.Location = new System.Drawing.Point(530, 414);
			this.radioButtonDirOccB.Name = "radioButtonDirOccB";
			this.radioButtonDirOccB.Size = new System.Drawing.Size(149, 17);
			this.radioButtonDirOccB.TabIndex = 5;
			this.radioButtonDirOccB.Text = "Directional Occlusion Blue";
			this.radioButtonDirOccB.UseVisualStyleBackColor = true;
			this.radioButtonDirOccB.CheckedChanged += new System.EventHandler(this.radioButtonDirOccB_CheckedChanged);
			// 
			// radioButtonAO
			// 
			this.radioButtonAO.AutoSize = true;
			this.radioButtonAO.Location = new System.Drawing.Point(530, 437);
			this.radioButtonAO.Name = "radioButtonAO";
			this.radioButtonAO.Size = new System.Drawing.Size(113, 17);
			this.radioButtonAO.TabIndex = 5;
			this.radioButtonAO.Text = "Ambient Occlusion";
			this.radioButtonAO.UseVisualStyleBackColor = true;
			this.radioButtonAO.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
			// 
			// radioButtonDirOccRGBtimeAO
			// 
			this.radioButtonDirOccRGBtimeAO.AutoSize = true;
			this.radioButtonDirOccRGBtimeAO.Location = new System.Drawing.Point(530, 345);
			this.radioButtonDirOccRGBtimeAO.Name = "radioButtonDirOccRGBtimeAO";
			this.radioButtonDirOccRGBtimeAO.Size = new System.Drawing.Size(176, 17);
			this.radioButtonDirOccRGBtimeAO.TabIndex = 5;
			this.radioButtonDirOccRGBtimeAO.Text = "Directional Occlusion RGB * AO";
			this.radioButtonDirOccRGBtimeAO.UseVisualStyleBackColor = true;
			this.radioButtonDirOccRGBtimeAO.CheckedChanged += new System.EventHandler(this.radioButtonDirOccRGBtimeAO_CheckedChanged);
			// 
			// openFileDialogImage
			// 
			this.openFileDialogImage.DefaultExt = "*.png";
			this.openFileDialogImage.Filter = "All Image Files|*.jpg;*.png;*.tga;*.tif|All Files (*.*)|*.*";
			this.openFileDialogImage.Title = "Choose a height map to load for processing...";
			// 
			// saveFileDialogImage
			// 
			this.saveFileDialogImage.DefaultExt = "*.png";
			this.saveFileDialogImage.Filter = "All Image Files|*.jpg;*.png;*.tif|All Files (*.*)|*.*";
			this.saveFileDialogImage.Title = "Choose an image file to save to...";
			// 
			// radioButtonAOfromRGB
			// 
			this.radioButtonAOfromRGB.AutoSize = true;
			this.radioButtonAOfromRGB.Location = new System.Drawing.Point(530, 460);
			this.radioButtonAOfromRGB.Name = "radioButtonAOfromRGB";
			this.radioButtonAOfromRGB.Size = new System.Drawing.Size(174, 17);
			this.radioButtonAOfromRGB.TabIndex = 5;
			this.radioButtonAOfromRGB.Text = "Ambient Occlusion length(RGB)";
			this.radioButtonAOfromRGB.UseVisualStyleBackColor = true;
			this.radioButtonAOfromRGB.CheckedChanged += new System.EventHandler(this.radioButtonAOfromRGB_CheckedChanged);
			// 
			// viewportPanelResult
			// 
			this.viewportPanelResult.Image = null;
			this.viewportPanelResult.Location = new System.Drawing.Point(856, 16);
			this.viewportPanelResult.MessageOnEmpty = null;
			this.viewportPanelResult.Name = "viewportPanelResult";
			this.viewportPanelResult.Size = new System.Drawing.Size(512, 512);
			this.viewportPanelResult.TabIndex = 0;
			this.viewportPanelResult.ViewMode = GenerateSelfShadowedBumpMap.ImagePanel.VIEW_MODE.RGB;
			this.viewportPanelResult.Click += new System.EventHandler(this.viewportPanelResult_Click);
			// 
			// outputPanelInputHeightMap
			// 
			this.outputPanelInputHeightMap.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.outputPanelInputHeightMap.Image = null;
			this.outputPanelInputHeightMap.Location = new System.Drawing.Point(12, 12);
			this.outputPanelInputHeightMap.MessageOnEmpty = "Click to load a height map...";
			this.outputPanelInputHeightMap.Name = "outputPanelInputHeightMap";
			this.outputPanelInputHeightMap.Size = new System.Drawing.Size(512, 512);
			this.outputPanelInputHeightMap.TabIndex = 0;
			this.outputPanelInputHeightMap.ViewMode = GenerateSelfShadowedBumpMap.ImagePanel.VIEW_MODE.RGB;
			this.outputPanelInputHeightMap.Click += new System.EventHandler(this.outputPanelInputHeightMap_Click);
			// 
			// checkBoxShowsRGB
			// 
			this.checkBoxShowsRGB.AutoSize = true;
			this.checkBoxShowsRGB.Checked = true;
			this.checkBoxShowsRGB.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowsRGB.Location = new System.Drawing.Point(752, 322);
			this.checkBoxShowsRGB.Name = "checkBoxShowsRGB";
			this.checkBoxShowsRGB.Size = new System.Drawing.Size(98, 17);
			this.checkBoxShowsRGB.TabIndex = 4;
			this.checkBoxShowsRGB.Text = "Show as sRGB";
			this.checkBoxShowsRGB.UseVisualStyleBackColor = true;
			this.checkBoxShowsRGB.CheckedChanged += new System.EventHandler(this.checkBoxShowsRGB_CheckedChanged);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1379, 540);
			this.Controls.Add(this.checkBoxShowsRGB);
			this.Controls.Add(this.radioButtonAOfromRGB);
			this.Controls.Add(this.radioButtonAO);
			this.Controls.Add(this.radioButtonDirOccB);
			this.Controls.Add(this.radioButtonDirOccG);
			this.Controls.Add(this.radioButtonDirOccRGBtimeAO);
			this.Controls.Add(this.radioButtonDirOccR);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.radioButtonShowDirOccRGB);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.viewportPanelResult);
			this.Controls.Add(this.outputPanelInputHeightMap);
			this.Controls.Add(this.floatTrackbarControlAOFactor);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Self-Shadowed Bump Map Generator";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

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
		private System.Windows.Forms.Label label5;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlAOFactor;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlMaxStepsCount;
		private System.Windows.Forms.Label label6;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlBilateralRadius;
		private System.Windows.Forms.Label label7;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlBilateralTolerance;
		private System.Windows.Forms.CheckBox checkBoxWrap;
		private System.Windows.Forms.RadioButton radioButtonShowDirOccRGB;
		private System.Windows.Forms.RadioButton radioButtonDirOccR;
		private System.Windows.Forms.RadioButton radioButtonDirOccG;
		private System.Windows.Forms.RadioButton radioButtonDirOccB;
		private System.Windows.Forms.RadioButton radioButtonAO;
		private System.Windows.Forms.RadioButton radioButtonDirOccRGBtimeAO;
		private System.Windows.Forms.OpenFileDialog openFileDialogImage;
		private System.Windows.Forms.SaveFileDialog saveFileDialogImage;
		private System.Windows.Forms.RadioButton radioButtonAOfromRGB;
		private System.Windows.Forms.CheckBox checkBoxShowsRGB;
	}
}

