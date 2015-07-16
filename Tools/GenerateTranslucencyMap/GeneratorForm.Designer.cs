namespace GenerateTranslucencyMap
{
	partial class GeneratorForm
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
			this.checkBoxShowsRGB = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.imagePanelResult0 = new GenerateTranslucencyMap.ImagePanel(this.components);
			this.imagePanelThicknessMap = new GenerateTranslucencyMap.ImagePanel(this.components);
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
			this.imagePanelNormalMap = new GenerateTranslucencyMap.ImagePanel(this.components);
			this.imagePanelAlbedoMap = new GenerateTranslucencyMap.ImagePanel(this.components);
			this.imagePanelTransmittanceMap = new GenerateTranslucencyMap.ImagePanel(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlAbsorptionCoefficient = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlScatteringAnisotropy = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlRefractionIndex = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.imagePanelResult1 = new GenerateTranslucencyMap.ImagePanel(this.components);
			this.imagePanelResult2 = new GenerateTranslucencyMap.ImagePanel(this.components);
			this.imagePanelResult3 = new GenerateTranslucencyMap.ImagePanel(this.components);
			this.groupBox1.SuspendLayout();
			this.groupBoxOptions.SuspendLayout();
			this.SuspendLayout();
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(530, 376);
			this.progressBar.Maximum = 1000;
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(320, 23);
			this.progressBar.TabIndex = 4;
			// 
			// radioButtonShowDirOccRGB
			// 
			this.radioButtonShowDirOccRGB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonShowDirOccRGB.AutoSize = true;
			this.radioButtonShowDirOccRGB.Checked = true;
			this.radioButtonShowDirOccRGB.Location = new System.Drawing.Point(10, 42);
			this.radioButtonShowDirOccRGB.Name = "radioButtonShowDirOccRGB";
			this.radioButtonShowDirOccRGB.Size = new System.Drawing.Size(151, 17);
			this.radioButtonShowDirOccRGB.TabIndex = 0;
			this.radioButtonShowDirOccRGB.TabStop = true;
			this.radioButtonShowDirOccRGB.Text = "Directional Occlusion RGB";
			this.radioButtonShowDirOccRGB.UseVisualStyleBackColor = true;
			this.radioButtonShowDirOccRGB.CheckedChanged += new System.EventHandler(this.radioButtonShowDirOccRGB_CheckedChanged);
			// 
			// radioButtonDirOccR
			// 
			this.radioButtonDirOccR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonDirOccR.AutoSize = true;
			this.radioButtonDirOccR.Location = new System.Drawing.Point(10, 88);
			this.radioButtonDirOccR.Name = "radioButtonDirOccR";
			this.radioButtonDirOccR.Size = new System.Drawing.Size(148, 17);
			this.radioButtonDirOccR.TabIndex = 2;
			this.radioButtonDirOccR.Text = "Directional Occlusion Red";
			this.radioButtonDirOccR.UseVisualStyleBackColor = true;
			this.radioButtonDirOccR.CheckedChanged += new System.EventHandler(this.radioButtonDirOccR_CheckedChanged);
			// 
			// radioButtonDirOccG
			// 
			this.radioButtonDirOccG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonDirOccG.AutoSize = true;
			this.radioButtonDirOccG.Location = new System.Drawing.Point(10, 111);
			this.radioButtonDirOccG.Name = "radioButtonDirOccG";
			this.radioButtonDirOccG.Size = new System.Drawing.Size(157, 17);
			this.radioButtonDirOccG.TabIndex = 3;
			this.radioButtonDirOccG.Text = "Directional Occlusion Green";
			this.radioButtonDirOccG.UseVisualStyleBackColor = true;
			this.radioButtonDirOccG.CheckedChanged += new System.EventHandler(this.radioButtonDirOccG_CheckedChanged);
			// 
			// radioButtonDirOccB
			// 
			this.radioButtonDirOccB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonDirOccB.AutoSize = true;
			this.radioButtonDirOccB.Location = new System.Drawing.Point(10, 134);
			this.radioButtonDirOccB.Name = "radioButtonDirOccB";
			this.radioButtonDirOccB.Size = new System.Drawing.Size(149, 17);
			this.radioButtonDirOccB.TabIndex = 4;
			this.radioButtonDirOccB.Text = "Directional Occlusion Blue";
			this.radioButtonDirOccB.UseVisualStyleBackColor = true;
			this.radioButtonDirOccB.CheckedChanged += new System.EventHandler(this.radioButtonDirOccB_CheckedChanged);
			// 
			// radioButtonAO
			// 
			this.radioButtonAO.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonAO.AutoSize = true;
			this.radioButtonAO.Location = new System.Drawing.Point(10, 157);
			this.radioButtonAO.Name = "radioButtonAO";
			this.radioButtonAO.Size = new System.Drawing.Size(113, 17);
			this.radioButtonAO.TabIndex = 5;
			this.radioButtonAO.Text = "Ambient Occlusion";
			this.radioButtonAO.UseVisualStyleBackColor = true;
			this.radioButtonAO.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
			// 
			// radioButtonDirOccRGBtimeAO
			// 
			this.radioButtonDirOccRGBtimeAO.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonDirOccRGBtimeAO.AutoSize = true;
			this.radioButtonDirOccRGBtimeAO.Location = new System.Drawing.Point(10, 65);
			this.radioButtonDirOccRGBtimeAO.Name = "radioButtonDirOccRGBtimeAO";
			this.radioButtonDirOccRGBtimeAO.Size = new System.Drawing.Size(176, 17);
			this.radioButtonDirOccRGBtimeAO.TabIndex = 1;
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
			this.saveFileDialogImage.Filter = "PNG Image Files|*.png|All Files (*.*)|*.*";
			this.saveFileDialogImage.Title = "Choose an image file to save to...";
			// 
			// radioButtonAOfromRGB
			// 
			this.radioButtonAOfromRGB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonAOfromRGB.AutoSize = true;
			this.radioButtonAOfromRGB.Location = new System.Drawing.Point(10, 180);
			this.radioButtonAOfromRGB.Name = "radioButtonAOfromRGB";
			this.radioButtonAOfromRGB.Size = new System.Drawing.Size(174, 17);
			this.radioButtonAOfromRGB.TabIndex = 6;
			this.radioButtonAOfromRGB.Text = "Ambient Occlusion length(RGB)";
			this.radioButtonAOfromRGB.UseVisualStyleBackColor = true;
			this.radioButtonAOfromRGB.CheckedChanged += new System.EventHandler(this.radioButtonAOfromRGB_CheckedChanged);
			// 
			// checkBoxShowsRGB
			// 
			this.checkBoxShowsRGB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxShowsRGB.AutoSize = true;
			this.checkBoxShowsRGB.Checked = true;
			this.checkBoxShowsRGB.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowsRGB.Location = new System.Drawing.Point(10, 19);
			this.checkBoxShowsRGB.Name = "checkBoxShowsRGB";
			this.checkBoxShowsRGB.Size = new System.Drawing.Size(98, 17);
			this.checkBoxShowsRGB.TabIndex = 7;
			this.checkBoxShowsRGB.Text = "Show as sRGB";
			this.checkBoxShowsRGB.UseVisualStyleBackColor = true;
			this.checkBoxShowsRGB.CheckedChanged += new System.EventHandler(this.checkBoxShowsRGB_CheckedChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.checkBoxShowsRGB);
			this.groupBox1.Controls.Add(this.radioButtonShowDirOccRGB);
			this.groupBox1.Controls.Add(this.radioButtonAOfromRGB);
			this.groupBox1.Controls.Add(this.radioButtonDirOccR);
			this.groupBox1.Controls.Add(this.radioButtonAO);
			this.groupBox1.Controls.Add(this.radioButtonDirOccRGBtimeAO);
			this.groupBox1.Controls.Add(this.radioButtonDirOccB);
			this.groupBox1.Controls.Add(this.radioButtonDirOccG);
			this.groupBox1.Location = new System.Drawing.Point(590, 405);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(190, 205);
			this.groupBox1.TabIndex = 9;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Result Display";
			// 
			// imagePanelResult0
			// 
			this.imagePanelResult0.Image = null;
			this.imagePanelResult0.Location = new System.Drawing.Point(856, 16);
			this.imagePanelResult0.MessageOnEmpty = null;
			this.imagePanelResult0.Name = "imagePanelResult0";
			this.imagePanelResult0.Size = new System.Drawing.Size(250, 250);
			this.imagePanelResult0.TabIndex = 0;
			this.imagePanelResult0.ViewLinear = false;
			this.imagePanelResult0.ViewMode = GenerateTranslucencyMap.ImagePanel.VIEW_MODE.RGB;
			this.imagePanelResult0.Click += new System.EventHandler(this.imagePanelResult0_Click);
			// 
			// imagePanelInputThicknessMap
			// 
			this.imagePanelThicknessMap.AllowDrop = true;
			this.imagePanelThicknessMap.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.imagePanelThicknessMap.Image = null;
			this.imagePanelThicknessMap.Location = new System.Drawing.Point(12, 12);
			this.imagePanelThicknessMap.MessageOnEmpty = "Click to load a thickness map,\r\nor drag and drop...";
			this.imagePanelThicknessMap.Name = "imagePanelInputThicknessMap";
			this.imagePanelThicknessMap.Size = new System.Drawing.Size(250, 250);
			this.imagePanelThicknessMap.TabIndex = 0;
			this.imagePanelThicknessMap.ViewLinear = false;
			this.imagePanelThicknessMap.ViewMode = GenerateTranslucencyMap.ImagePanel.VIEW_MODE.RGB;
			this.imagePanelThicknessMap.Click += new System.EventHandler(this.imagePanelInputThicknessMap_Click);
			this.imagePanelThicknessMap.DragDrop += new System.Windows.Forms.DragEventHandler(this.imagePanelInputThicknessMap_DragDrop);
			this.imagePanelThicknessMap.DragEnter += new System.Windows.Forms.DragEventHandler(this.imagePanelInputThicknessMap_DragEnter);
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
			this.integerTrackbarControlRaysCount.Value = 128;
			this.integerTrackbarControlRaysCount.VisibleRangeMax = 256;
			this.integerTrackbarControlRaysCount.VisibleRangeMin = 1;
			// 
			// buttonGenerate
			// 
			this.buttonGenerate.Location = new System.Drawing.Point(107, 317);
			this.buttonGenerate.Name = "buttonGenerate";
			this.buttonGenerate.Size = new System.Drawing.Size(105, 35);
			this.buttonGenerate.TabIndex = 19;
			this.buttonGenerate.Text = "Generate";
			this.buttonGenerate.UseVisualStyleBackColor = true;
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
			this.floatTrackbarControlScatteringCoefficient.Value = 10.2F;
			this.floatTrackbarControlScatteringCoefficient.VisibleRangeMax = 20F;
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
			this.floatTrackbarControlThickness.Value = 1F;
			this.floatTrackbarControlThickness.VisibleRangeMax = 1F;
			this.floatTrackbarControlThickness.VisibleRangeMin = 0.01F;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(9, 194);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(81, 13);
			this.label8.TabIndex = 30;
			this.label8.Text = "Pixels per meter";
			// 
			// floatTrackbarControlBilateralRadius
			// 
			this.floatTrackbarControlBilateralRadius.Location = new System.Drawing.Point(108, 266);
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
			this.label9.Location = new System.Drawing.Point(9, 295);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(95, 13);
			this.label9.TabIndex = 31;
			this.label9.Text = "Bilateral Tolerance";
			// 
			// floatTrackbarControlBilateralTolerance
			// 
			this.floatTrackbarControlBilateralTolerance.Location = new System.Drawing.Point(108, 292);
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
			this.label10.Location = new System.Drawing.Point(9, 269);
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
			this.floatTrackbarControlPixelDensity.Location = new System.Drawing.Point(108, 191);
			this.floatTrackbarControlPixelDensity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlPixelDensity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlPixelDensity.Name = "floatTrackbarControlPixelDensity";
			this.floatTrackbarControlPixelDensity.RangeMax = 10000F;
			this.floatTrackbarControlPixelDensity.RangeMin = 1F;
			this.floatTrackbarControlPixelDensity.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlPixelDensity.TabIndex = 26;
			this.floatTrackbarControlPixelDensity.Value = 512F;
			this.floatTrackbarControlPixelDensity.VisibleRangeMax = 1024F;
			this.floatTrackbarControlPixelDensity.VisibleRangeMin = 1F;
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
			this.groupBoxOptions.Controls.Add(this.label13);
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
			this.groupBoxOptions.Location = new System.Drawing.Point(530, 12);
			this.groupBoxOptions.Name = "groupBoxOptions";
			this.groupBoxOptions.Size = new System.Drawing.Size(320, 358);
			this.groupBoxOptions.TabIndex = 35;
			this.groupBoxOptions.TabStop = false;
			this.groupBoxOptions.Text = "Translucency Map Options";
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
			// imagePanelAlbedoMap
			// 
			this.imagePanelAlbedoMap.AllowDrop = true;
			this.imagePanelAlbedoMap.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.imagePanelAlbedoMap.Image = null;
			this.imagePanelAlbedoMap.Location = new System.Drawing.Point(12, 278);
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
			// imagePanelTransmittanceMap
			// 
			this.imagePanelTransmittanceMap.AllowDrop = true;
			this.imagePanelTransmittanceMap.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.imagePanelTransmittanceMap.Image = null;
			this.imagePanelTransmittanceMap.Location = new System.Drawing.Point(274, 278);
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
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 105);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(93, 13);
			this.label1.TabIndex = 29;
			this.label1.Text = "Absorption (1/mm)";
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
			this.floatTrackbarControlAbsorptionCoefficient.Value = 0.4F;
			this.floatTrackbarControlAbsorptionCoefficient.VisibleRangeMax = 1F;
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
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(8, 157);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(85, 13);
			this.label3.TabIndex = 29;
			this.label3.Text = "Refraction Index";
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
			// imagePanelResult1
			// 
			this.imagePanelResult1.Image = null;
			this.imagePanelResult1.Location = new System.Drawing.Point(1117, 16);
			this.imagePanelResult1.MessageOnEmpty = null;
			this.imagePanelResult1.Name = "imagePanelResult1";
			this.imagePanelResult1.Size = new System.Drawing.Size(250, 250);
			this.imagePanelResult1.TabIndex = 0;
			this.imagePanelResult1.ViewLinear = false;
			this.imagePanelResult1.ViewMode = GenerateTranslucencyMap.ImagePanel.VIEW_MODE.RGB;
			this.imagePanelResult1.Click += new System.EventHandler(this.imagePanelResult1_Click);
			// 
			// imagePanelResult2
			// 
			this.imagePanelResult2.Image = null;
			this.imagePanelResult2.Location = new System.Drawing.Point(856, 278);
			this.imagePanelResult2.MessageOnEmpty = null;
			this.imagePanelResult2.Name = "imagePanelResult2";
			this.imagePanelResult2.Size = new System.Drawing.Size(250, 250);
			this.imagePanelResult2.TabIndex = 0;
			this.imagePanelResult2.ViewLinear = false;
			this.imagePanelResult2.ViewMode = GenerateTranslucencyMap.ImagePanel.VIEW_MODE.RGB;
			this.imagePanelResult2.Click += new System.EventHandler(this.imagePanelResult2_Click);
			// 
			// imagePanelResult3
			// 
			this.imagePanelResult3.Image = null;
			this.imagePanelResult3.Location = new System.Drawing.Point(1117, 278);
			this.imagePanelResult3.MessageOnEmpty = null;
			this.imagePanelResult3.Name = "imagePanelResult3";
			this.imagePanelResult3.Size = new System.Drawing.Size(250, 250);
			this.imagePanelResult3.TabIndex = 0;
			this.imagePanelResult3.ViewLinear = false;
			this.imagePanelResult3.ViewMode = GenerateTranslucencyMap.ImagePanel.VIEW_MODE.RGB;
			this.imagePanelResult3.Click += new System.EventHandler(this.imagePanelResult3_Click);
			// 
			// GeneratorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1379, 540);
			this.Controls.Add(this.groupBoxOptions);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.imagePanelResult3);
			this.Controls.Add(this.imagePanelResult2);
			this.Controls.Add(this.imagePanelResult1);
			this.Controls.Add(this.imagePanelResult0);
			this.Controls.Add(this.imagePanelNormalMap);
			this.Controls.Add(this.imagePanelTransmittanceMap);
			this.Controls.Add(this.imagePanelAlbedoMap);
			this.Controls.Add(this.imagePanelThicknessMap);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "GeneratorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Self-Shadowed Bump Map Generator";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBoxOptions.ResumeLayout(false);
			this.groupBoxOptions.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ImagePanel imagePanelThicknessMap;
		private ImagePanel imagePanelResult0;
		private System.Windows.Forms.ProgressBar progressBar;
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
		private System.Windows.Forms.GroupBox groupBox1;
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
	}
}

