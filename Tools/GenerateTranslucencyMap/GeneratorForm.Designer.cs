namespace GenerateTranslucencyMap
{
	partial class GeneratorForm
	{
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
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.openFileDialogImage = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogImage = new System.Windows.Forms.SaveFileDialog();
			this.integerTrackbarControlRaysCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.buttonGenerate = new System.Windows.Forms.Button();
			this.floatTrackbarControlScatteringCoefficient = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlThickness = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label8 = new System.Windows.Forms.Label();
			this.floatTrackbarControlBilateralRadius = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label9 = new System.Windows.Forms.Label();
			this.floatTrackbarControlBilateralTolerance = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label10 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.floatTrackbarControlPixelDensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label11 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.groupBoxOptions = new System.Windows.Forms.GroupBox();
			this.label5 = new System.Windows.Forms.Label();
			this.integerTrackbarControlKernelSize = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.floatTrackbarControlScatteringAnisotropy = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlRefractionIndex = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlAbsorptionCoefficient = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonShowViewer = new System.Windows.Forms.Button();
			this.buttonReload = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.floatTrackbarControlDominantHue = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.panelDominantHue = new System.Windows.Forms.Panel();
			this.imagePanelResult3 = new GenerateTranslucencyMap.ImagePanel(this.components);
			this.imagePanelResult2 = new GenerateTranslucencyMap.ImagePanel(this.components);
			this.imagePanelResult1 = new GenerateTranslucencyMap.ImagePanel(this.components);
			this.imagePanelResult0 = new GenerateTranslucencyMap.ImagePanel(this.components);
			this.imagePanelNormalMap = new GenerateTranslucencyMap.ImagePanel(this.components);
			this.imagePanelTransmittanceMap = new GenerateTranslucencyMap.ImagePanel(this.components);
			this.imagePanelAlbedoMap = new GenerateTranslucencyMap.ImagePanel(this.components);
			this.imagePanelThicknessMap = new GenerateTranslucencyMap.ImagePanel(this.components);
			this.groupBoxOptions.SuspendLayout();
			this.SuspendLayout();
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(530, 364);
			this.progressBar.Maximum = 1000;
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(320, 23);
			this.progressBar.TabIndex = 4;
			// 
			// openFileDialogImage
			// 
			this.openFileDialogImage.DefaultExt = "*.png";
			this.openFileDialogImage.Filter = "All Image Files|*.jpg;*.png;*.tga;*.tif|All Files (*.*)|*.*";
			this.openFileDialogImage.Title = "Choose a map to load for processing...";
			// 
			// saveFileDialogImage
			// 
			this.saveFileDialogImage.DefaultExt = "*.png";
			this.saveFileDialogImage.Filter = "PNG Image Files|*.png|All Files (*.*)|*.*";
			this.saveFileDialogImage.Title = "Choose an image file to save to...";
			// 
			// integerTrackbarControlRaysCount
			// 
			this.integerTrackbarControlRaysCount.Location = new System.Drawing.Point(107, 24);
			this.integerTrackbarControlRaysCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlRaysCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlRaysCount.Name = "integerTrackbarControlRaysCount";
			this.integerTrackbarControlRaysCount.RangeMax = 1024;
			this.integerTrackbarControlRaysCount.RangeMin = 1;
			this.integerTrackbarControlRaysCount.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlRaysCount.TabIndex = 21;
			this.integerTrackbarControlRaysCount.Value = 1;
			this.integerTrackbarControlRaysCount.VisibleRangeMax = 128;
			this.integerTrackbarControlRaysCount.VisibleRangeMin = 1;
			this.integerTrackbarControlRaysCount.SliderDragStop += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.SliderDragStopEventHandler(this.integerTrackbarControlRaysCount_SliderDragStop);
			// 
			// buttonGenerate
			// 
			this.buttonGenerate.Location = new System.Drawing.Point(108, 304);
			this.buttonGenerate.Name = "buttonGenerate";
			this.buttonGenerate.Size = new System.Drawing.Size(105, 35);
			this.buttonGenerate.TabIndex = 19;
			this.buttonGenerate.Text = "Generate";
			this.buttonGenerate.UseVisualStyleBackColor = true;
			this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
			// 
			// floatTrackbarControlScatteringCoefficient
			// 
			this.floatTrackbarControlScatteringCoefficient.Location = new System.Drawing.Point(107, 76);
			this.floatTrackbarControlScatteringCoefficient.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlScatteringCoefficient.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlScatteringCoefficient.Name = "floatTrackbarControlScatteringCoefficient";
			this.floatTrackbarControlScatteringCoefficient.RangeMax = 1000F;
			this.floatTrackbarControlScatteringCoefficient.RangeMin = 0.001F;
			this.floatTrackbarControlScatteringCoefficient.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlScatteringCoefficient.TabIndex = 25;
			this.floatTrackbarControlScatteringCoefficient.Value = 1F;
			this.floatTrackbarControlScatteringCoefficient.VisibleRangeMax = 2F;
			this.floatTrackbarControlScatteringCoefficient.VisibleRangeMin = 0.001F;
			// 
			// floatTrackbarControlThickness
			// 
			this.floatTrackbarControlThickness.Location = new System.Drawing.Point(107, 50);
			this.floatTrackbarControlThickness.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlThickness.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlThickness.Name = "floatTrackbarControlThickness";
			this.floatTrackbarControlThickness.RangeMax = 100F;
			this.floatTrackbarControlThickness.RangeMin = 0.01F;
			this.floatTrackbarControlThickness.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlThickness.TabIndex = 24;
			this.floatTrackbarControlThickness.Value = 2F;
			this.floatTrackbarControlThickness.VisibleRangeMax = 4F;
			this.floatTrackbarControlThickness.VisibleRangeMin = 0.01F;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(5, 193);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(87, 13);
			this.label8.TabIndex = 30;
			this.label8.Text = "Subject size (cm)";
			this.toolTip1.SetToolTip(this.label8, "Size of the subject in the image in centimeters (e.g. about 8cm for the vertical " +
        "leaf in the example)");
			// 
			// floatTrackbarControlBilateralRadius
			// 
			this.floatTrackbarControlBilateralRadius.Location = new System.Drawing.Point(108, 253);
			this.floatTrackbarControlBilateralRadius.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBilateralRadius.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBilateralRadius.Name = "floatTrackbarControlBilateralRadius";
			this.floatTrackbarControlBilateralRadius.RangeMax = 32F;
			this.floatTrackbarControlBilateralRadius.RangeMin = 0.001F;
			this.floatTrackbarControlBilateralRadius.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlBilateralRadius.TabIndex = 22;
			this.floatTrackbarControlBilateralRadius.Value = 10F;
			this.floatTrackbarControlBilateralRadius.VisibleRangeMax = 32F;
			this.floatTrackbarControlBilateralRadius.VisibleRangeMin = 0.001F;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(8, 282);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(95, 13);
			this.label9.TabIndex = 31;
			this.label9.Text = "Bilateral Tolerance";
			// 
			// floatTrackbarControlBilateralTolerance
			// 
			this.floatTrackbarControlBilateralTolerance.Location = new System.Drawing.Point(108, 279);
			this.floatTrackbarControlBilateralTolerance.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBilateralTolerance.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBilateralTolerance.Name = "floatTrackbarControlBilateralTolerance";
			this.floatTrackbarControlBilateralTolerance.RangeMax = 1F;
			this.floatTrackbarControlBilateralTolerance.RangeMin = 0F;
			this.floatTrackbarControlBilateralTolerance.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlBilateralTolerance.TabIndex = 23;
			this.floatTrackbarControlBilateralTolerance.Value = 0.2F;
			this.floatTrackbarControlBilateralTolerance.VisibleRangeMax = 1F;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(8, 256);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(80, 13);
			this.label10.TabIndex = 32;
			this.label10.Text = "Bilateral Radius";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(8, 79);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(91, 13);
			this.label14.TabIndex = 29;
			this.label14.Text = "Scattering (1/mm)";
			// 
			// floatTrackbarControlPixelDensity
			// 
			this.floatTrackbarControlPixelDensity.Location = new System.Drawing.Point(108, 189);
			this.floatTrackbarControlPixelDensity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlPixelDensity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlPixelDensity.Name = "floatTrackbarControlPixelDensity";
			this.floatTrackbarControlPixelDensity.RangeMax = 100F;
			this.floatTrackbarControlPixelDensity.RangeMin = 0F;
			this.floatTrackbarControlPixelDensity.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlPixelDensity.TabIndex = 26;
			this.toolTip1.SetToolTip(this.floatTrackbarControlPixelDensity, "Size of the subject in the image in centimeters (e.g. about 8cm for the vertical " +
        "leaf in the example)");
			this.floatTrackbarControlPixelDensity.Value = 8F;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(8, 53);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(86, 13);
			this.label11.TabIndex = 28;
			this.label11.Text = "Thickness in mm";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(8, 27);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(74, 13);
			this.label13.TabIndex = 33;
			this.label13.Text = "Rays per Pixel";
			// 
			// groupBoxOptions
			// 
			this.groupBoxOptions.Controls.Add(this.label5);
			this.groupBoxOptions.Controls.Add(this.label13);
			this.groupBoxOptions.Controls.Add(this.integerTrackbarControlKernelSize);
			this.groupBoxOptions.Controls.Add(this.integerTrackbarControlRaysCount);
			this.groupBoxOptions.Controls.Add(this.buttonGenerate);
			this.groupBoxOptions.Controls.Add(this.floatTrackbarControlScatteringAnisotropy);
			this.groupBoxOptions.Controls.Add(this.floatTrackbarControlRefractionIndex);
			this.groupBoxOptions.Controls.Add(this.floatTrackbarControlAbsorptionCoefficient);
			this.groupBoxOptions.Controls.Add(this.floatTrackbarControlScatteringCoefficient);
			this.groupBoxOptions.Controls.Add(this.label11);
			this.groupBoxOptions.Controls.Add(this.floatTrackbarControlThickness);
			this.groupBoxOptions.Controls.Add(this.label2);
			this.groupBoxOptions.Controls.Add(this.label3);
			this.groupBoxOptions.Controls.Add(this.floatTrackbarControlPixelDensity);
			this.groupBoxOptions.Controls.Add(this.label1);
			this.groupBoxOptions.Controls.Add(this.label8);
			this.groupBoxOptions.Controls.Add(this.label14);
			this.groupBoxOptions.Controls.Add(this.floatTrackbarControlBilateralRadius);
			this.groupBoxOptions.Controls.Add(this.label10);
			this.groupBoxOptions.Controls.Add(this.label9);
			this.groupBoxOptions.Controls.Add(this.floatTrackbarControlBilateralTolerance);
			this.groupBoxOptions.Enabled = false;
			this.groupBoxOptions.Location = new System.Drawing.Point(530, 12);
			this.groupBoxOptions.Name = "groupBoxOptions";
			this.groupBoxOptions.Size = new System.Drawing.Size(320, 346);
			this.groupBoxOptions.TabIndex = 35;
			this.groupBoxOptions.TabStop = false;
			this.groupBoxOptions.Text = "Translucency Map Options";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(5, 223);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(60, 13);
			this.label5.TabIndex = 33;
			this.label5.Text = "Kernel Size";
			this.label5.Visible = false;
			// 
			// integerTrackbarControlKernelSize
			// 
			this.integerTrackbarControlKernelSize.Location = new System.Drawing.Point(107, 218);
			this.integerTrackbarControlKernelSize.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlKernelSize.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlKernelSize.Name = "integerTrackbarControlKernelSize";
			this.integerTrackbarControlKernelSize.RangeMax = 256;
			this.integerTrackbarControlKernelSize.RangeMin = 1;
			this.integerTrackbarControlKernelSize.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlKernelSize.TabIndex = 21;
			this.integerTrackbarControlKernelSize.Value = 16;
			this.integerTrackbarControlKernelSize.Visible = false;
			this.integerTrackbarControlKernelSize.VisibleRangeMax = 32;
			this.integerTrackbarControlKernelSize.VisibleRangeMin = 1;
			// 
			// floatTrackbarControlScatteringAnisotropy
			// 
			this.floatTrackbarControlScatteringAnisotropy.Location = new System.Drawing.Point(108, 128);
			this.floatTrackbarControlScatteringAnisotropy.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlScatteringAnisotropy.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlScatteringAnisotropy.Name = "floatTrackbarControlScatteringAnisotropy";
			this.floatTrackbarControlScatteringAnisotropy.RangeMax = 1F;
			this.floatTrackbarControlScatteringAnisotropy.RangeMin = -1F;
			this.floatTrackbarControlScatteringAnisotropy.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlScatteringAnisotropy.TabIndex = 25;
			this.floatTrackbarControlScatteringAnisotropy.Value = 0.07F;
			this.floatTrackbarControlScatteringAnisotropy.VisibleRangeMax = 1F;
			this.floatTrackbarControlScatteringAnisotropy.VisibleRangeMin = -1F;
			// 
			// floatTrackbarControlRefractionIndex
			// 
			this.floatTrackbarControlRefractionIndex.Location = new System.Drawing.Point(107, 154);
			this.floatTrackbarControlRefractionIndex.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlRefractionIndex.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlRefractionIndex.Name = "floatTrackbarControlRefractionIndex";
			this.floatTrackbarControlRefractionIndex.RangeMax = 1000F;
			this.floatTrackbarControlRefractionIndex.RangeMin = 0F;
			this.floatTrackbarControlRefractionIndex.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlRefractionIndex.TabIndex = 25;
			this.floatTrackbarControlRefractionIndex.Value = 1.33F;
			this.floatTrackbarControlRefractionIndex.VisibleRangeMax = 2F;
			// 
			// floatTrackbarControlAbsorptionCoefficient
			// 
			this.floatTrackbarControlAbsorptionCoefficient.Location = new System.Drawing.Point(107, 102);
			this.floatTrackbarControlAbsorptionCoefficient.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlAbsorptionCoefficient.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlAbsorptionCoefficient.Name = "floatTrackbarControlAbsorptionCoefficient";
			this.floatTrackbarControlAbsorptionCoefficient.RangeMax = 1000F;
			this.floatTrackbarControlAbsorptionCoefficient.RangeMin = 0.001F;
			this.floatTrackbarControlAbsorptionCoefficient.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlAbsorptionCoefficient.TabIndex = 25;
			this.floatTrackbarControlAbsorptionCoefficient.Value = 0.1F;
			this.floatTrackbarControlAbsorptionCoefficient.VisibleRangeMax = 2F;
			this.floatTrackbarControlAbsorptionCoefficient.VisibleRangeMin = 0.001F;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 131);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(87, 13);
			this.label2.TabIndex = 29;
			this.label2.Text = "Scatt. Anisotropy";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(8, 157);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(85, 13);
			this.label3.TabIndex = 29;
			this.label3.Text = "Refraction Index";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 105);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(93, 13);
			this.label1.TabIndex = 29;
			this.label1.Text = "Absorption (1/mm)";
			// 
			// buttonShowViewer
			// 
			this.buttonShowViewer.Location = new System.Drawing.Point(775, 427);
			this.buttonShowViewer.Name = "buttonShowViewer";
			this.buttonShowViewer.Size = new System.Drawing.Size(75, 23);
			this.buttonShowViewer.TabIndex = 36;
			this.buttonShowViewer.Text = "View";
			this.buttonShowViewer.UseVisualStyleBackColor = true;
			this.buttonShowViewer.Click += new System.EventHandler(this.buttonShowViewer_Click);
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(775, 500);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 37;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// floatTrackbarControlDominantHue
			// 
			this.floatTrackbarControlDominantHue.Location = new System.Drawing.Point(1117, 519);
			this.floatTrackbarControlDominantHue.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDominantHue.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDominantHue.Name = "floatTrackbarControlDominantHue";
			this.floatTrackbarControlDominantHue.RangeMax = 360F;
			this.floatTrackbarControlDominantHue.RangeMin = 0F;
			this.floatTrackbarControlDominantHue.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDominantHue.TabIndex = 22;
			this.floatTrackbarControlDominantHue.Value = 120F;
			this.floatTrackbarControlDominantHue.VisibleRangeMax = 360F;
			this.floatTrackbarControlDominantHue.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlDominantHue_ValueChanged);
			this.floatTrackbarControlDominantHue.SliderDragStop += new Nuaj.Cirrus.Utility.FloatTrackbarControl.SliderDragStopEventHandler(this.floatTrackbarControlDominantHue_SliderDragStop);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(981, 522);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(134, 13);
			this.label4.TabIndex = 32;
			this.label4.Text = "Dominant Hue for Combine";
			// 
			// panelDominantHue
			// 
			this.panelDominantHue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelDominantHue.Location = new System.Drawing.Point(1324, 519);
			this.panelDominantHue.Name = "panelDominantHue";
			this.panelDominantHue.Size = new System.Drawing.Size(43, 19);
			this.panelDominantHue.TabIndex = 38;
			// 
			// imagePanelResult3
			// 
			this.imagePanelResult3.Image = null;
			this.imagePanelResult3.Location = new System.Drawing.Point(1117, 268);
			this.imagePanelResult3.MessageOnEmpty = null;
			this.imagePanelResult3.Name = "imagePanelResult3";
			this.imagePanelResult3.Size = new System.Drawing.Size(250, 250);
			this.imagePanelResult3.TabIndex = 0;
			this.imagePanelResult3.ViewLinear = false;
			this.imagePanelResult3.ViewMode = GenerateTranslucencyMap.ImagePanel.VIEW_MODE.RGB;
			this.imagePanelResult3.Click += new System.EventHandler(this.imagePanelResult3_Click);
			// 
			// imagePanelResult2
			// 
			this.imagePanelResult2.Image = null;
			this.imagePanelResult2.Location = new System.Drawing.Point(856, 268);
			this.imagePanelResult2.MessageOnEmpty = null;
			this.imagePanelResult2.Name = "imagePanelResult2";
			this.imagePanelResult2.Size = new System.Drawing.Size(250, 250);
			this.imagePanelResult2.TabIndex = 0;
			this.imagePanelResult2.ViewLinear = false;
			this.imagePanelResult2.ViewMode = GenerateTranslucencyMap.ImagePanel.VIEW_MODE.RGB;
			this.imagePanelResult2.Click += new System.EventHandler(this.imagePanelResult2_Click);
			// 
			// imagePanelResult1
			// 
			this.imagePanelResult1.Image = null;
			this.imagePanelResult1.Location = new System.Drawing.Point(1117, 12);
			this.imagePanelResult1.MessageOnEmpty = null;
			this.imagePanelResult1.Name = "imagePanelResult1";
			this.imagePanelResult1.Size = new System.Drawing.Size(250, 250);
			this.imagePanelResult1.TabIndex = 0;
			this.imagePanelResult1.ViewLinear = false;
			this.imagePanelResult1.ViewMode = GenerateTranslucencyMap.ImagePanel.VIEW_MODE.RGB;
			this.imagePanelResult1.Click += new System.EventHandler(this.imagePanelResult1_Click);
			// 
			// imagePanelResult0
			// 
			this.imagePanelResult0.Image = null;
			this.imagePanelResult0.Location = new System.Drawing.Point(856, 12);
			this.imagePanelResult0.MessageOnEmpty = null;
			this.imagePanelResult0.Name = "imagePanelResult0";
			this.imagePanelResult0.Size = new System.Drawing.Size(250, 250);
			this.imagePanelResult0.TabIndex = 0;
			this.imagePanelResult0.ViewLinear = false;
			this.imagePanelResult0.ViewMode = GenerateTranslucencyMap.ImagePanel.VIEW_MODE.RGB;
			this.imagePanelResult0.Click += new System.EventHandler(this.imagePanelResult0_Click);
			// 
			// imagePanelNormalMap
			// 
			this.imagePanelNormalMap.AllowDrop = true;
			this.imagePanelNormalMap.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.imagePanelNormalMap.Image = null;
			this.imagePanelNormalMap.Location = new System.Drawing.Point(274, 12);
			this.imagePanelNormalMap.MessageOnEmpty = "Click to load a normal map,\r\nor drag and drop...";
			this.imagePanelNormalMap.Name = "imagePanelNormalMap";
			this.imagePanelNormalMap.Size = new System.Drawing.Size(250, 250);
			this.imagePanelNormalMap.TabIndex = 0;
			this.imagePanelNormalMap.ViewLinear = false;
			this.imagePanelNormalMap.ViewMode = GenerateTranslucencyMap.ImagePanel.VIEW_MODE.RGB;
			this.imagePanelNormalMap.Click += new System.EventHandler(this.imagePanelNormalMap_Click);
			this.imagePanelNormalMap.DragDrop += new System.Windows.Forms.DragEventHandler(this.imagePanelNormalMap_DragDrop);
			this.imagePanelNormalMap.DragEnter += new System.Windows.Forms.DragEventHandler(this.imagePanelNormalMap_DragEnter);
			// 
			// imagePanelTransmittanceMap
			// 
			this.imagePanelTransmittanceMap.AllowDrop = true;
			this.imagePanelTransmittanceMap.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.imagePanelTransmittanceMap.Image = null;
			this.imagePanelTransmittanceMap.Location = new System.Drawing.Point(274, 268);
			this.imagePanelTransmittanceMap.MessageOnEmpty = "Click to load a transmittance map,\r\nor drag and drop...";
			this.imagePanelTransmittanceMap.Name = "imagePanelTransmittanceMap";
			this.imagePanelTransmittanceMap.Size = new System.Drawing.Size(250, 250);
			this.imagePanelTransmittanceMap.TabIndex = 0;
			this.imagePanelTransmittanceMap.ViewLinear = false;
			this.imagePanelTransmittanceMap.ViewMode = GenerateTranslucencyMap.ImagePanel.VIEW_MODE.RGB;
			this.imagePanelTransmittanceMap.Click += new System.EventHandler(this.imagePanelTransmittanceMap_Click);
			this.imagePanelTransmittanceMap.DragDrop += new System.Windows.Forms.DragEventHandler(this.imagePanelTransmittanceMap_DragDrop);
			this.imagePanelTransmittanceMap.DragEnter += new System.Windows.Forms.DragEventHandler(this.imagePanelTransmittanceMap_DragEnter);
			// 
			// imagePanelAlbedoMap
			// 
			this.imagePanelAlbedoMap.AllowDrop = true;
			this.imagePanelAlbedoMap.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.imagePanelAlbedoMap.Image = null;
			this.imagePanelAlbedoMap.Location = new System.Drawing.Point(12, 268);
			this.imagePanelAlbedoMap.MessageOnEmpty = "Click to load an albedo map,\r\nor drag and drop...";
			this.imagePanelAlbedoMap.Name = "imagePanelAlbedoMap";
			this.imagePanelAlbedoMap.Size = new System.Drawing.Size(250, 250);
			this.imagePanelAlbedoMap.TabIndex = 0;
			this.imagePanelAlbedoMap.ViewLinear = false;
			this.imagePanelAlbedoMap.ViewMode = GenerateTranslucencyMap.ImagePanel.VIEW_MODE.RGB;
			this.imagePanelAlbedoMap.Click += new System.EventHandler(this.imagePanelAlbedoMap_Click);
			this.imagePanelAlbedoMap.DragDrop += new System.Windows.Forms.DragEventHandler(this.imagePanelAlbedoMap_DragDrop);
			this.imagePanelAlbedoMap.DragEnter += new System.Windows.Forms.DragEventHandler(this.imagePanelAlbedoMap_DragEnter);
			// 
			// imagePanelThicknessMap
			// 
			this.imagePanelThicknessMap.AllowDrop = true;
			this.imagePanelThicknessMap.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.imagePanelThicknessMap.Image = null;
			this.imagePanelThicknessMap.Location = new System.Drawing.Point(12, 12);
			this.imagePanelThicknessMap.MessageOnEmpty = "Click to load a thickness map,\r\nor drag and drop...";
			this.imagePanelThicknessMap.Name = "imagePanelThicknessMap";
			this.imagePanelThicknessMap.Size = new System.Drawing.Size(250, 250);
			this.imagePanelThicknessMap.TabIndex = 0;
			this.imagePanelThicknessMap.ViewLinear = false;
			this.imagePanelThicknessMap.ViewMode = GenerateTranslucencyMap.ImagePanel.VIEW_MODE.RGB;
			this.imagePanelThicknessMap.Click += new System.EventHandler(this.imagePanelInputThicknessMap_Click);
			this.imagePanelThicknessMap.DragDrop += new System.Windows.Forms.DragEventHandler(this.imagePanelInputThicknessMap_DragDrop);
			this.imagePanelThicknessMap.DragEnter += new System.Windows.Forms.DragEventHandler(this.imagePanelInputThicknessMap_DragEnter);
			// 
			// GeneratorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1379, 540);
			this.Controls.Add(this.panelDominantHue);
			this.Controls.Add(this.floatTrackbarControlDominantHue);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.buttonShowViewer);
			this.Controls.Add(this.groupBoxOptions);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.imagePanelResult3);
			this.Controls.Add(this.imagePanelResult2);
			this.Controls.Add(this.imagePanelResult1);
			this.Controls.Add(this.imagePanelResult0);
			this.Controls.Add(this.imagePanelNormalMap);
			this.Controls.Add(this.imagePanelTransmittanceMap);
			this.Controls.Add(this.imagePanelAlbedoMap);
			this.Controls.Add(this.imagePanelThicknessMap);
			this.Controls.Add(this.label4);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "GeneratorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Self-Shadowed Bump Map Generator";
			this.groupBoxOptions.ResumeLayout(false);
			this.groupBoxOptions.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ImagePanel imagePanelThicknessMap;
		private ImagePanel imagePanelResult0;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.OpenFileDialog openFileDialogImage;
		private System.Windows.Forms.SaveFileDialog saveFileDialogImage;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlRaysCount;
		private System.Windows.Forms.Button buttonGenerate;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlScatteringCoefficient;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlThickness;
		private System.Windows.Forms.Label label8;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlBilateralRadius;
		private System.Windows.Forms.Label label9;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlBilateralTolerance;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label14;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlPixelDensity;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.GroupBox groupBoxOptions;
		private ImagePanel imagePanelNormalMap;
		private ImagePanel imagePanelAlbedoMap;
		private ImagePanel imagePanelTransmittanceMap;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlAbsorptionCoefficient;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlScatteringAnisotropy;
		private System.Windows.Forms.Label label2;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlRefractionIndex;
		private System.Windows.Forms.Label label3;
		private ImagePanel imagePanelResult1;
		private ImagePanel imagePanelResult2;
		private ImagePanel imagePanelResult3;
		private System.Windows.Forms.Button buttonShowViewer;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.ToolTip toolTip1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDominantHue;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Panel panelDominantHue;
		private System.Windows.Forms.Label label5;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlKernelSize;
	}
}

