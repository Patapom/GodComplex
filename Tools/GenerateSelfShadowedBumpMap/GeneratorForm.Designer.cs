namespace GenerateSelfShadowedBumpMap
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
			this.floatTrackbarControlHeight = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.integerTrackbarControlRaysCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.checkBoxWrap = new System.Windows.Forms.CheckBox();
			this.buttonGenerate = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.integerTrackbarControlMaxStepsCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.floatTrackbarControlPixelDensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlBilateralTolerance = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlBilateralRadius = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
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
			this.tabControlGenerators = new System.Windows.Forms.TabControl();
			this.tabPageSSBump = new System.Windows.Forms.TabPage();
			this.tabPageNormal = new System.Windows.Forms.TabPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.viewportPanelResult = new GenerateSelfShadowedBumpMap.ImagePanel(this.components);
			this.outputPanelInputHeightMap = new GenerateSelfShadowedBumpMap.ImagePanel(this.components);
			this.buttonTest = new System.Windows.Forms.Button();
			this.buttonReload = new System.Windows.Forms.Button();
			this.tabControlGenerators.SuspendLayout();
			this.tabPageSSBump.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// floatTrackbarControlHeight
			// 
			this.floatTrackbarControlHeight.Location = new System.Drawing.Point(106, 7);
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
			this.integerTrackbarControlRaysCount.Location = new System.Drawing.Point(106, 80);
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
			// checkBoxWrap
			// 
			this.checkBoxWrap.AutoSize = true;
			this.checkBoxWrap.Checked = true;
			this.checkBoxWrap.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxWrap.Location = new System.Drawing.Point(8, 218);
			this.checkBoxWrap.Name = "checkBoxWrap";
			this.checkBoxWrap.Size = new System.Drawing.Size(43, 17);
			this.checkBoxWrap.TabIndex = 4;
			this.checkBoxWrap.Text = "Tile";
			this.checkBoxWrap.UseVisualStyleBackColor = true;
			// 
			// buttonGenerate
			// 
			this.buttonGenerate.Location = new System.Drawing.Point(104, 208);
			this.buttonGenerate.Name = "buttonGenerate";
			this.buttonGenerate.Size = new System.Drawing.Size(105, 35);
			this.buttonGenerate.TabIndex = 0;
			this.buttonGenerate.Text = "Generate";
			this.buttonGenerate.UseVisualStyleBackColor = true;
			this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(2, 36);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(102, 31);
			this.label3.TabIndex = 3;
			this.label3.Text = "Physical Texture Size (cm)";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(3, 181);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(95, 13);
			this.label7.TabIndex = 3;
			this.label7.Text = "Bilateral Tolerance";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(3, 155);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(80, 13);
			this.label6.TabIndex = 3;
			this.label6.Text = "Bilateral Radius";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(1, 10);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(107, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Encoded Height (cm)";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(1, 109);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(88, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Max Steps Count";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(1, 83);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(74, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Rays per Pixel";
			// 
			// integerTrackbarControlMaxStepsCount
			// 
			this.integerTrackbarControlMaxStepsCount.Location = new System.Drawing.Point(106, 106);
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
			this.floatTrackbarControlPixelDensity.Location = new System.Drawing.Point(106, 33);
			this.floatTrackbarControlPixelDensity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlPixelDensity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlPixelDensity.Name = "floatTrackbarControlPixelDensity";
			this.floatTrackbarControlPixelDensity.RangeMax = 10000F;
			this.floatTrackbarControlPixelDensity.RangeMin = 1F;
			this.floatTrackbarControlPixelDensity.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlPixelDensity.TabIndex = 3;
			this.floatTrackbarControlPixelDensity.Value = 100F;
			this.floatTrackbarControlPixelDensity.VisibleRangeMax = 200F;
			this.floatTrackbarControlPixelDensity.VisibleRangeMin = 1F;
			// 
			// floatTrackbarControlBilateralTolerance
			// 
			this.floatTrackbarControlBilateralTolerance.Location = new System.Drawing.Point(106, 178);
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
			this.floatTrackbarControlBilateralRadius.Location = new System.Drawing.Point(106, 152);
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
			// tabControlGenerators
			// 
			this.tabControlGenerators.Controls.Add(this.tabPageSSBump);
			this.tabControlGenerators.Controls.Add(this.tabPageNormal);
			this.tabControlGenerators.Enabled = false;
			this.tabControlGenerators.Location = new System.Drawing.Point(530, 12);
			this.tabControlGenerators.Name = "tabControlGenerators";
			this.tabControlGenerators.SelectedIndex = 0;
			this.tabControlGenerators.Size = new System.Drawing.Size(320, 275);
			this.tabControlGenerators.TabIndex = 8;
			// 
			// tabPageSSBump
			// 
			this.tabPageSSBump.Controls.Add(this.checkBoxWrap);
			this.tabPageSSBump.Controls.Add(this.integerTrackbarControlRaysCount);
			this.tabPageSSBump.Controls.Add(this.buttonGenerate);
			this.tabPageSSBump.Controls.Add(this.floatTrackbarControlHeight);
			this.tabPageSSBump.Controls.Add(this.floatTrackbarControlBilateralRadius);
			this.tabPageSSBump.Controls.Add(this.floatTrackbarControlBilateralTolerance);
			this.tabPageSSBump.Controls.Add(this.floatTrackbarControlPixelDensity);
			this.tabPageSSBump.Controls.Add(this.integerTrackbarControlMaxStepsCount);
			this.tabPageSSBump.Controls.Add(this.label3);
			this.tabPageSSBump.Controls.Add(this.label7);
			this.tabPageSSBump.Controls.Add(this.label6);
			this.tabPageSSBump.Controls.Add(this.label2);
			this.tabPageSSBump.Controls.Add(this.label4);
			this.tabPageSSBump.Controls.Add(this.label1);
			this.tabPageSSBump.Location = new System.Drawing.Point(4, 22);
			this.tabPageSSBump.Name = "tabPageSSBump";
			this.tabPageSSBump.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageSSBump.Size = new System.Drawing.Size(312, 249);
			this.tabPageSSBump.TabIndex = 0;
			this.tabPageSSBump.Text = "SSBump";
			this.tabPageSSBump.UseVisualStyleBackColor = true;
			// 
			// tabPageNormal
			// 
			this.tabPageNormal.Location = new System.Drawing.Point(4, 22);
			this.tabPageNormal.Name = "tabPageNormal";
			this.tabPageNormal.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageNormal.Size = new System.Drawing.Size(312, 249);
			this.tabPageNormal.TabIndex = 2;
			this.tabPageNormal.Text = "Normal";
			this.tabPageNormal.UseVisualStyleBackColor = true;
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
			this.groupBox1.Location = new System.Drawing.Point(530, 322);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(190, 205);
			this.groupBox1.TabIndex = 9;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Result Display";
			// 
			// viewportPanelResult
			// 
			this.viewportPanelResult.Image = null;
			this.viewportPanelResult.Location = new System.Drawing.Point(856, 16);
			this.viewportPanelResult.MessageOnEmpty = null;
			this.viewportPanelResult.Name = "viewportPanelResult";
			this.viewportPanelResult.Size = new System.Drawing.Size(512, 512);
			this.viewportPanelResult.TabIndex = 0;
			this.viewportPanelResult.ViewLinear = false;
			this.viewportPanelResult.ViewMode = GenerateSelfShadowedBumpMap.ImagePanel.VIEW_MODE.RGB;
			this.viewportPanelResult.Click += new System.EventHandler(this.viewportPanelResult_Click);
			// 
			// outputPanelInputHeightMap
			// 
			this.outputPanelInputHeightMap.AllowDrop = true;
			this.outputPanelInputHeightMap.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.outputPanelInputHeightMap.Image = null;
			this.outputPanelInputHeightMap.Location = new System.Drawing.Point(12, 12);
			this.outputPanelInputHeightMap.MessageOnEmpty = "Click to load a height map,\r\nor drag and drop...";
			this.outputPanelInputHeightMap.Name = "outputPanelInputHeightMap";
			this.outputPanelInputHeightMap.Size = new System.Drawing.Size(512, 512);
			this.outputPanelInputHeightMap.TabIndex = 0;
			this.outputPanelInputHeightMap.ViewLinear = false;
			this.outputPanelInputHeightMap.ViewMode = GenerateSelfShadowedBumpMap.ImagePanel.VIEW_MODE.RGB;
			this.outputPanelInputHeightMap.Click += new System.EventHandler(this.outputPanelInputHeightMap_Click);
			this.outputPanelInputHeightMap.DragDrop += new System.Windows.Forms.DragEventHandler(this.outputPanelInputHeightMap_DragDrop);
			this.outputPanelInputHeightMap.DragEnter += new System.Windows.Forms.DragEventHandler(this.outputPanelInputHeightMap_DragEnter);
			// 
			// buttonTest
			// 
			this.buttonTest.Location = new System.Drawing.Point(771, 433);
			this.buttonTest.Name = "buttonTest";
			this.buttonTest.Size = new System.Drawing.Size(75, 23);
			this.buttonTest.TabIndex = 10;
			this.buttonTest.Text = "Test";
			this.buttonTest.UseVisualStyleBackColor = true;
			this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(771, 499);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 10;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Visible = false;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// GeneratorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1379, 540);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.buttonTest);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.tabControlGenerators);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.viewportPanelResult);
			this.Controls.Add(this.outputPanelInputHeightMap);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "GeneratorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Self-Shadowed Bump Map Generator";
			this.tabControlGenerators.ResumeLayout(false);
			this.tabPageSSBump.ResumeLayout(false);
			this.tabPageSSBump.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ImagePanel outputPanelInputHeightMap;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlHeight;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlRaysCount;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private ImagePanel viewportPanelResult;
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlPixelDensity;
		private System.Windows.Forms.Button buttonGenerate;
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
		private System.Windows.Forms.TabControl tabControlGenerators;
		private System.Windows.Forms.TabPage tabPageSSBump;
		private System.Windows.Forms.TabPage tabPageNormal;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button buttonTest;
		private System.Windows.Forms.Button buttonReload;
	}
}

