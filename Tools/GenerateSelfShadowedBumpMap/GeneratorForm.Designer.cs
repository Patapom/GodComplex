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
			this.tabPageTranslucency = new System.Windows.Forms.TabPage();
			this.checkBoxWrapTr = new System.Windows.Forms.CheckBox();
			this.integerTrackbarControlRaysCount2 = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.buttonGenerateTranslucency = new System.Windows.Forms.Button();
			this.floatTrackbarControlDensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlThickness = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label8 = new System.Windows.Forms.Label();
			this.floatTrackbarControlBilateralRadiusTr = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label9 = new System.Windows.Forms.Label();
			this.floatTrackbarControlBilateralToleranceTr = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label10 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.floatTrackbarControlPixelDensityTr = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label11 = new System.Windows.Forms.Label();
			this.integerTrackbarControlMaxStepsCountTr = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label12 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.tabPageNormal = new System.Windows.Forms.TabPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.viewportPanelResult = new GenerateSelfShadowedBumpMap.ImagePanel( this.components );
			this.outputPanelInputHeightMap = new GenerateSelfShadowedBumpMap.ImagePanel( this.components );
			this.tabControlGenerators.SuspendLayout();
			this.tabPageSSBump.SuspendLayout();
			this.tabPageTranslucency.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// floatTrackbarControlHeight
			// 
			this.floatTrackbarControlHeight.Location = new System.Drawing.Point( 105, 30 );
			this.floatTrackbarControlHeight.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlHeight.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlHeight.Name = "floatTrackbarControlHeight";
			this.floatTrackbarControlHeight.RangeMax = 1000F;
			this.floatTrackbarControlHeight.RangeMin = 0.01F;
			this.floatTrackbarControlHeight.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlHeight.TabIndex = 2;
			this.floatTrackbarControlHeight.Value = 10F;
			this.floatTrackbarControlHeight.VisibleRangeMin = 0.01F;
			// 
			// integerTrackbarControlRaysCount
			// 
			this.integerTrackbarControlRaysCount.Location = new System.Drawing.Point( 105, 4 );
			this.integerTrackbarControlRaysCount.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.integerTrackbarControlRaysCount.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.integerTrackbarControlRaysCount.Name = "integerTrackbarControlRaysCount";
			this.integerTrackbarControlRaysCount.RangeMax = 1024;
			this.integerTrackbarControlRaysCount.RangeMin = 1;
			this.integerTrackbarControlRaysCount.Size = new System.Drawing.Size( 200, 20 );
			this.integerTrackbarControlRaysCount.TabIndex = 1;
			this.integerTrackbarControlRaysCount.Value = 300;
			this.integerTrackbarControlRaysCount.VisibleRangeMax = 1024;
			this.integerTrackbarControlRaysCount.VisibleRangeMin = 1;
			this.integerTrackbarControlRaysCount.SliderDragStop += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.SliderDragStopEventHandler( this.integerTrackbarControlRaysCount_SliderDragStop );
			// 
			// checkBoxWrap
			// 
			this.checkBoxWrap.AutoSize = true;
			this.checkBoxWrap.Checked = true;
			this.checkBoxWrap.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxWrap.Location = new System.Drawing.Point( 8, 218 );
			this.checkBoxWrap.Name = "checkBoxWrap";
			this.checkBoxWrap.Size = new System.Drawing.Size( 43, 17 );
			this.checkBoxWrap.TabIndex = 4;
			this.checkBoxWrap.Text = "Tile";
			this.checkBoxWrap.UseVisualStyleBackColor = true;
			// 
			// buttonGenerate
			// 
			this.buttonGenerate.Location = new System.Drawing.Point( 104, 208 );
			this.buttonGenerate.Name = "buttonGenerate";
			this.buttonGenerate.Size = new System.Drawing.Size( 105, 35 );
			this.buttonGenerate.TabIndex = 0;
			this.buttonGenerate.Text = "Generate";
			this.buttonGenerate.UseVisualStyleBackColor = true;
			this.buttonGenerate.Click += new System.EventHandler( this.buttonGenerate_Click );
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point( 7, 92 );
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size( 81, 13 );
			this.label3.TabIndex = 3;
			this.label3.Text = "Pixels per meter";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point( 7, 181 );
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size( 95, 13 );
			this.label7.TabIndex = 3;
			this.label7.Text = "Bilateral Tolerance";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point( 7, 155 );
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size( 80, 13 );
			this.label6.TabIndex = 3;
			this.label6.Text = "Bilateral Radius";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point( 6, 33 );
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size( 66, 13 );
			this.label2.TabIndex = 3;
			this.label2.Text = "Height in cm";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point( 7, 118 );
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size( 88, 13 );
			this.label4.TabIndex = 3;
			this.label4.Text = "Max Steps Count";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point( 6, 7 );
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size( 74, 13 );
			this.label1.TabIndex = 3;
			this.label1.Text = "Rays per Pixel";
			// 
			// integerTrackbarControlMaxStepsCount
			// 
			this.integerTrackbarControlMaxStepsCount.Location = new System.Drawing.Point( 106, 115 );
			this.integerTrackbarControlMaxStepsCount.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.integerTrackbarControlMaxStepsCount.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.integerTrackbarControlMaxStepsCount.Name = "integerTrackbarControlMaxStepsCount";
			this.integerTrackbarControlMaxStepsCount.RangeMax = 400;
			this.integerTrackbarControlMaxStepsCount.RangeMin = 1;
			this.integerTrackbarControlMaxStepsCount.Size = new System.Drawing.Size( 200, 20 );
			this.integerTrackbarControlMaxStepsCount.TabIndex = 1;
			this.integerTrackbarControlMaxStepsCount.Value = 100;
			this.integerTrackbarControlMaxStepsCount.VisibleRangeMax = 200;
			this.integerTrackbarControlMaxStepsCount.VisibleRangeMin = 1;
			// 
			// floatTrackbarControlPixelDensity
			// 
			this.floatTrackbarControlPixelDensity.Location = new System.Drawing.Point( 106, 89 );
			this.floatTrackbarControlPixelDensity.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlPixelDensity.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlPixelDensity.Name = "floatTrackbarControlPixelDensity";
			this.floatTrackbarControlPixelDensity.RangeMax = 10000F;
			this.floatTrackbarControlPixelDensity.RangeMin = 1F;
			this.floatTrackbarControlPixelDensity.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlPixelDensity.TabIndex = 3;
			this.floatTrackbarControlPixelDensity.Value = 512F;
			this.floatTrackbarControlPixelDensity.VisibleRangeMax = 1024F;
			this.floatTrackbarControlPixelDensity.VisibleRangeMin = 1F;
			// 
			// floatTrackbarControlBilateralTolerance
			// 
			this.floatTrackbarControlBilateralTolerance.Location = new System.Drawing.Point( 106, 178 );
			this.floatTrackbarControlBilateralTolerance.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlBilateralTolerance.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlBilateralTolerance.Name = "floatTrackbarControlBilateralTolerance";
			this.floatTrackbarControlBilateralTolerance.RangeMax = 1F;
			this.floatTrackbarControlBilateralTolerance.RangeMin = 0F;
			this.floatTrackbarControlBilateralTolerance.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlBilateralTolerance.TabIndex = 2;
			this.floatTrackbarControlBilateralTolerance.Value = 0.2F;
			this.floatTrackbarControlBilateralTolerance.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlBilateralRadius
			// 
			this.floatTrackbarControlBilateralRadius.Location = new System.Drawing.Point( 106, 152 );
			this.floatTrackbarControlBilateralRadius.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlBilateralRadius.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlBilateralRadius.Name = "floatTrackbarControlBilateralRadius";
			this.floatTrackbarControlBilateralRadius.RangeMax = 32F;
			this.floatTrackbarControlBilateralRadius.RangeMin = 0.001F;
			this.floatTrackbarControlBilateralRadius.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlBilateralRadius.TabIndex = 2;
			this.floatTrackbarControlBilateralRadius.Value = 10F;
			this.floatTrackbarControlBilateralRadius.VisibleRangeMax = 32F;
			this.floatTrackbarControlBilateralRadius.VisibleRangeMin = 0.001F;
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point( 530, 293 );
			this.progressBar.Maximum = 1000;
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size( 320, 23 );
			this.progressBar.TabIndex = 4;
			// 
			// radioButtonShowDirOccRGB
			// 
			this.radioButtonShowDirOccRGB.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonShowDirOccRGB.AutoSize = true;
			this.radioButtonShowDirOccRGB.Checked = true;
			this.radioButtonShowDirOccRGB.Location = new System.Drawing.Point( 10, 42 );
			this.radioButtonShowDirOccRGB.Name = "radioButtonShowDirOccRGB";
			this.radioButtonShowDirOccRGB.Size = new System.Drawing.Size( 151, 17 );
			this.radioButtonShowDirOccRGB.TabIndex = 0;
			this.radioButtonShowDirOccRGB.TabStop = true;
			this.radioButtonShowDirOccRGB.Text = "Directional Occlusion RGB";
			this.radioButtonShowDirOccRGB.UseVisualStyleBackColor = true;
			this.radioButtonShowDirOccRGB.CheckedChanged += new System.EventHandler( this.radioButtonShowDirOccRGB_CheckedChanged );
			// 
			// radioButtonDirOccR
			// 
			this.radioButtonDirOccR.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonDirOccR.AutoSize = true;
			this.radioButtonDirOccR.Location = new System.Drawing.Point( 10, 88 );
			this.radioButtonDirOccR.Name = "radioButtonDirOccR";
			this.radioButtonDirOccR.Size = new System.Drawing.Size( 148, 17 );
			this.radioButtonDirOccR.TabIndex = 2;
			this.radioButtonDirOccR.Text = "Directional Occlusion Red";
			this.radioButtonDirOccR.UseVisualStyleBackColor = true;
			this.radioButtonDirOccR.CheckedChanged += new System.EventHandler( this.radioButtonDirOccR_CheckedChanged );
			// 
			// radioButtonDirOccG
			// 
			this.radioButtonDirOccG.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonDirOccG.AutoSize = true;
			this.radioButtonDirOccG.Location = new System.Drawing.Point( 10, 111 );
			this.radioButtonDirOccG.Name = "radioButtonDirOccG";
			this.radioButtonDirOccG.Size = new System.Drawing.Size( 157, 17 );
			this.radioButtonDirOccG.TabIndex = 3;
			this.radioButtonDirOccG.Text = "Directional Occlusion Green";
			this.radioButtonDirOccG.UseVisualStyleBackColor = true;
			this.radioButtonDirOccG.CheckedChanged += new System.EventHandler( this.radioButtonDirOccG_CheckedChanged );
			// 
			// radioButtonDirOccB
			// 
			this.radioButtonDirOccB.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonDirOccB.AutoSize = true;
			this.radioButtonDirOccB.Location = new System.Drawing.Point( 10, 134 );
			this.radioButtonDirOccB.Name = "radioButtonDirOccB";
			this.radioButtonDirOccB.Size = new System.Drawing.Size( 149, 17 );
			this.radioButtonDirOccB.TabIndex = 4;
			this.radioButtonDirOccB.Text = "Directional Occlusion Blue";
			this.radioButtonDirOccB.UseVisualStyleBackColor = true;
			this.radioButtonDirOccB.CheckedChanged += new System.EventHandler( this.radioButtonDirOccB_CheckedChanged );
			// 
			// radioButtonAO
			// 
			this.radioButtonAO.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonAO.AutoSize = true;
			this.radioButtonAO.Location = new System.Drawing.Point( 10, 157 );
			this.radioButtonAO.Name = "radioButtonAO";
			this.radioButtonAO.Size = new System.Drawing.Size( 113, 17 );
			this.radioButtonAO.TabIndex = 5;
			this.radioButtonAO.Text = "Ambient Occlusion";
			this.radioButtonAO.UseVisualStyleBackColor = true;
			this.radioButtonAO.CheckedChanged += new System.EventHandler( this.radioButton1_CheckedChanged );
			// 
			// radioButtonDirOccRGBtimeAO
			// 
			this.radioButtonDirOccRGBtimeAO.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonDirOccRGBtimeAO.AutoSize = true;
			this.radioButtonDirOccRGBtimeAO.Location = new System.Drawing.Point( 10, 65 );
			this.radioButtonDirOccRGBtimeAO.Name = "radioButtonDirOccRGBtimeAO";
			this.radioButtonDirOccRGBtimeAO.Size = new System.Drawing.Size( 176, 17 );
			this.radioButtonDirOccRGBtimeAO.TabIndex = 1;
			this.radioButtonDirOccRGBtimeAO.Text = "Directional Occlusion RGB * AO";
			this.radioButtonDirOccRGBtimeAO.UseVisualStyleBackColor = true;
			this.radioButtonDirOccRGBtimeAO.CheckedChanged += new System.EventHandler( this.radioButtonDirOccRGBtimeAO_CheckedChanged );
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
			this.radioButtonAOfromRGB.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonAOfromRGB.AutoSize = true;
			this.radioButtonAOfromRGB.Location = new System.Drawing.Point( 10, 180 );
			this.radioButtonAOfromRGB.Name = "radioButtonAOfromRGB";
			this.radioButtonAOfromRGB.Size = new System.Drawing.Size( 174, 17 );
			this.radioButtonAOfromRGB.TabIndex = 6;
			this.radioButtonAOfromRGB.Text = "Ambient Occlusion length(RGB)";
			this.radioButtonAOfromRGB.UseVisualStyleBackColor = true;
			this.radioButtonAOfromRGB.CheckedChanged += new System.EventHandler( this.radioButtonAOfromRGB_CheckedChanged );
			// 
			// checkBoxShowsRGB
			// 
			this.checkBoxShowsRGB.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxShowsRGB.AutoSize = true;
			this.checkBoxShowsRGB.Checked = true;
			this.checkBoxShowsRGB.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowsRGB.Location = new System.Drawing.Point( 10, 19 );
			this.checkBoxShowsRGB.Name = "checkBoxShowsRGB";
			this.checkBoxShowsRGB.Size = new System.Drawing.Size( 98, 17 );
			this.checkBoxShowsRGB.TabIndex = 7;
			this.checkBoxShowsRGB.Text = "Show as sRGB";
			this.checkBoxShowsRGB.UseVisualStyleBackColor = true;
			this.checkBoxShowsRGB.CheckedChanged += new System.EventHandler( this.checkBoxShowsRGB_CheckedChanged );
			// 
			// tabControlGenerators
			// 
			this.tabControlGenerators.Controls.Add( this.tabPageSSBump );
			this.tabControlGenerators.Controls.Add( this.tabPageTranslucency );
			this.tabControlGenerators.Controls.Add( this.tabPageNormal );
			this.tabControlGenerators.Location = new System.Drawing.Point( 530, 12 );
			this.tabControlGenerators.Name = "tabControlGenerators";
			this.tabControlGenerators.SelectedIndex = 0;
			this.tabControlGenerators.Size = new System.Drawing.Size( 320, 275 );
			this.tabControlGenerators.TabIndex = 8;
			// 
			// tabPageSSBump
			// 
			this.tabPageSSBump.Controls.Add( this.checkBoxWrap );
			this.tabPageSSBump.Controls.Add( this.integerTrackbarControlRaysCount );
			this.tabPageSSBump.Controls.Add( this.buttonGenerate );
			this.tabPageSSBump.Controls.Add( this.floatTrackbarControlHeight );
			this.tabPageSSBump.Controls.Add( this.label3 );
			this.tabPageSSBump.Controls.Add( this.floatTrackbarControlBilateralRadius );
			this.tabPageSSBump.Controls.Add( this.label7 );
			this.tabPageSSBump.Controls.Add( this.floatTrackbarControlBilateralTolerance );
			this.tabPageSSBump.Controls.Add( this.label6 );
			this.tabPageSSBump.Controls.Add( this.floatTrackbarControlPixelDensity );
			this.tabPageSSBump.Controls.Add( this.label2 );
			this.tabPageSSBump.Controls.Add( this.integerTrackbarControlMaxStepsCount );
			this.tabPageSSBump.Controls.Add( this.label4 );
			this.tabPageSSBump.Controls.Add( this.label1 );
			this.tabPageSSBump.Location = new System.Drawing.Point( 4, 22 );
			this.tabPageSSBump.Name = "tabPageSSBump";
			this.tabPageSSBump.Padding = new System.Windows.Forms.Padding( 3 );
			this.tabPageSSBump.Size = new System.Drawing.Size( 312, 249 );
			this.tabPageSSBump.TabIndex = 0;
			this.tabPageSSBump.Text = "SSBump";
			this.tabPageSSBump.UseVisualStyleBackColor = true;
			// 
			// tabPageTranslucency
			// 
			this.tabPageTranslucency.Controls.Add( this.checkBoxWrapTr );
			this.tabPageTranslucency.Controls.Add( this.integerTrackbarControlRaysCount2 );
			this.tabPageTranslucency.Controls.Add( this.buttonGenerateTranslucency );
			this.tabPageTranslucency.Controls.Add( this.floatTrackbarControlDensity );
			this.tabPageTranslucency.Controls.Add( this.floatTrackbarControlThickness );
			this.tabPageTranslucency.Controls.Add( this.label8 );
			this.tabPageTranslucency.Controls.Add( this.floatTrackbarControlBilateralRadiusTr );
			this.tabPageTranslucency.Controls.Add( this.label9 );
			this.tabPageTranslucency.Controls.Add( this.floatTrackbarControlBilateralToleranceTr );
			this.tabPageTranslucency.Controls.Add( this.label10 );
			this.tabPageTranslucency.Controls.Add( this.label14 );
			this.tabPageTranslucency.Controls.Add( this.floatTrackbarControlPixelDensityTr );
			this.tabPageTranslucency.Controls.Add( this.label11 );
			this.tabPageTranslucency.Controls.Add( this.integerTrackbarControlMaxStepsCountTr );
			this.tabPageTranslucency.Controls.Add( this.label12 );
			this.tabPageTranslucency.Controls.Add( this.label13 );
			this.tabPageTranslucency.Location = new System.Drawing.Point( 4, 22 );
			this.tabPageTranslucency.Name = "tabPageTranslucency";
			this.tabPageTranslucency.Padding = new System.Windows.Forms.Padding( 3 );
			this.tabPageTranslucency.Size = new System.Drawing.Size( 312, 249 );
			this.tabPageTranslucency.TabIndex = 1;
			this.tabPageTranslucency.Text = "Translucency";
			this.tabPageTranslucency.UseVisualStyleBackColor = true;
			// 
			// checkBoxWrapTr
			// 
			this.checkBoxWrapTr.AutoSize = true;
			this.checkBoxWrapTr.Checked = true;
			this.checkBoxWrapTr.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxWrapTr.Location = new System.Drawing.Point( 8, 218 );
			this.checkBoxWrapTr.Name = "checkBoxWrapTr";
			this.checkBoxWrapTr.Size = new System.Drawing.Size( 43, 17 );
			this.checkBoxWrapTr.TabIndex = 18;
			this.checkBoxWrapTr.Text = "Tile";
			this.checkBoxWrapTr.UseVisualStyleBackColor = true;
			// 
			// integerTrackbarControlRaysCount2
			// 
			this.integerTrackbarControlRaysCount2.Location = new System.Drawing.Point( 105, 4 );
			this.integerTrackbarControlRaysCount2.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.integerTrackbarControlRaysCount2.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.integerTrackbarControlRaysCount2.Name = "integerTrackbarControlRaysCount2";
			this.integerTrackbarControlRaysCount2.RangeMax = 1024;
			this.integerTrackbarControlRaysCount2.RangeMin = 1;
			this.integerTrackbarControlRaysCount2.Size = new System.Drawing.Size( 200, 20 );
			this.integerTrackbarControlRaysCount2.TabIndex = 7;
			this.integerTrackbarControlRaysCount2.Value = 128;
			this.integerTrackbarControlRaysCount2.VisibleRangeMax = 256;
			this.integerTrackbarControlRaysCount2.VisibleRangeMin = 1;
			this.integerTrackbarControlRaysCount2.SliderDragStop += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.SliderDragStopEventHandler( this.integerTrackbarControlRaysCount2_SliderDragStop );
			// 
			// buttonGenerateTranslucency
			// 
			this.buttonGenerateTranslucency.Location = new System.Drawing.Point( 104, 208 );
			this.buttonGenerateTranslucency.Name = "buttonGenerateTranslucency";
			this.buttonGenerateTranslucency.Size = new System.Drawing.Size( 105, 35 );
			this.buttonGenerateTranslucency.TabIndex = 5;
			this.buttonGenerateTranslucency.Text = "Generate";
			this.buttonGenerateTranslucency.UseVisualStyleBackColor = true;
			this.buttonGenerateTranslucency.Click += new System.EventHandler( this.buttonGenerateTranslucency_Click );
			// 
			// floatTrackbarControlDensity
			// 
			this.floatTrackbarControlDensity.Location = new System.Drawing.Point( 105, 56 );
			this.floatTrackbarControlDensity.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlDensity.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlDensity.Name = "floatTrackbarControlDensity";
			this.floatTrackbarControlDensity.RangeMax = 1000F;
			this.floatTrackbarControlDensity.RangeMin = 0.01F;
			this.floatTrackbarControlDensity.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlDensity.TabIndex = 10;
			this.floatTrackbarControlDensity.Value = 10F;
			this.floatTrackbarControlDensity.VisibleRangeMin = 0.01F;
			// 
			// floatTrackbarControlThickness
			// 
			this.floatTrackbarControlThickness.Location = new System.Drawing.Point( 105, 30 );
			this.floatTrackbarControlThickness.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlThickness.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlThickness.Name = "floatTrackbarControlThickness";
			this.floatTrackbarControlThickness.RangeMax = 1000F;
			this.floatTrackbarControlThickness.RangeMin = 0.01F;
			this.floatTrackbarControlThickness.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlThickness.TabIndex = 10;
			this.floatTrackbarControlThickness.Value = 10F;
			this.floatTrackbarControlThickness.VisibleRangeMin = 0.01F;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point( 7, 92 );
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size( 81, 13 );
			this.label8.TabIndex = 14;
			this.label8.Text = "Pixels per meter";
			// 
			// floatTrackbarControlBilateralRadiusTr
			// 
			this.floatTrackbarControlBilateralRadiusTr.Location = new System.Drawing.Point( 106, 152 );
			this.floatTrackbarControlBilateralRadiusTr.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlBilateralRadiusTr.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlBilateralRadiusTr.Name = "floatTrackbarControlBilateralRadiusTr";
			this.floatTrackbarControlBilateralRadiusTr.RangeMax = 32F;
			this.floatTrackbarControlBilateralRadiusTr.RangeMin = 0.001F;
			this.floatTrackbarControlBilateralRadiusTr.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlBilateralRadiusTr.TabIndex = 8;
			this.floatTrackbarControlBilateralRadiusTr.Value = 10F;
			this.floatTrackbarControlBilateralRadiusTr.VisibleRangeMax = 32F;
			this.floatTrackbarControlBilateralRadiusTr.VisibleRangeMin = 0.001F;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point( 7, 181 );
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size( 95, 13 );
			this.label9.TabIndex = 15;
			this.label9.Text = "Bilateral Tolerance";
			// 
			// floatTrackbarControlBilateralToleranceTr
			// 
			this.floatTrackbarControlBilateralToleranceTr.Location = new System.Drawing.Point( 106, 178 );
			this.floatTrackbarControlBilateralToleranceTr.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlBilateralToleranceTr.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlBilateralToleranceTr.Name = "floatTrackbarControlBilateralToleranceTr";
			this.floatTrackbarControlBilateralToleranceTr.RangeMax = 1F;
			this.floatTrackbarControlBilateralToleranceTr.RangeMin = 0F;
			this.floatTrackbarControlBilateralToleranceTr.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlBilateralToleranceTr.TabIndex = 9;
			this.floatTrackbarControlBilateralToleranceTr.Value = 0.2F;
			this.floatTrackbarControlBilateralToleranceTr.VisibleRangeMax = 1F;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point( 7, 155 );
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size( 80, 13 );
			this.label10.TabIndex = 16;
			this.label10.Text = "Bilateral Radius";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point( 6, 59 );
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size( 82, 13 );
			this.label14.TabIndex = 13;
			this.label14.Text = "Medium Density";
			// 
			// floatTrackbarControlPixelDensityTr
			// 
			this.floatTrackbarControlPixelDensityTr.Location = new System.Drawing.Point( 106, 89 );
			this.floatTrackbarControlPixelDensityTr.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlPixelDensityTr.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlPixelDensityTr.Name = "floatTrackbarControlPixelDensityTr";
			this.floatTrackbarControlPixelDensityTr.RangeMax = 10000F;
			this.floatTrackbarControlPixelDensityTr.RangeMin = 1F;
			this.floatTrackbarControlPixelDensityTr.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlPixelDensityTr.TabIndex = 11;
			this.floatTrackbarControlPixelDensityTr.Value = 512F;
			this.floatTrackbarControlPixelDensityTr.VisibleRangeMax = 1024F;
			this.floatTrackbarControlPixelDensityTr.VisibleRangeMin = 1F;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point( 6, 33 );
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size( 84, 13 );
			this.label11.TabIndex = 13;
			this.label11.Text = "Thickness in cm";
			// 
			// integerTrackbarControlMaxStepsCountTr
			// 
			this.integerTrackbarControlMaxStepsCountTr.Location = new System.Drawing.Point( 106, 115 );
			this.integerTrackbarControlMaxStepsCountTr.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.integerTrackbarControlMaxStepsCountTr.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.integerTrackbarControlMaxStepsCountTr.Name = "integerTrackbarControlMaxStepsCountTr";
			this.integerTrackbarControlMaxStepsCountTr.RangeMax = 400;
			this.integerTrackbarControlMaxStepsCountTr.RangeMin = 1;
			this.integerTrackbarControlMaxStepsCountTr.Size = new System.Drawing.Size( 200, 20 );
			this.integerTrackbarControlMaxStepsCountTr.TabIndex = 6;
			this.integerTrackbarControlMaxStepsCountTr.Value = 100;
			this.integerTrackbarControlMaxStepsCountTr.VisibleRangeMax = 200;
			this.integerTrackbarControlMaxStepsCountTr.VisibleRangeMin = 1;
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point( 7, 118 );
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size( 88, 13 );
			this.label12.TabIndex = 12;
			this.label12.Text = "Max Steps Count";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point( 6, 7 );
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size( 74, 13 );
			this.label13.TabIndex = 17;
			this.label13.Text = "Rays per Pixel";
			// 
			// tabPageNormal
			// 
			this.tabPageNormal.Location = new System.Drawing.Point( 4, 22 );
			this.tabPageNormal.Name = "tabPageNormal";
			this.tabPageNormal.Padding = new System.Windows.Forms.Padding( 3 );
			this.tabPageNormal.Size = new System.Drawing.Size( 312, 249 );
			this.tabPageNormal.TabIndex = 2;
			this.tabPageNormal.Text = "Normal";
			this.tabPageNormal.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add( this.checkBoxShowsRGB );
			this.groupBox1.Controls.Add( this.radioButtonShowDirOccRGB );
			this.groupBox1.Controls.Add( this.radioButtonAOfromRGB );
			this.groupBox1.Controls.Add( this.radioButtonDirOccR );
			this.groupBox1.Controls.Add( this.radioButtonAO );
			this.groupBox1.Controls.Add( this.radioButtonDirOccRGBtimeAO );
			this.groupBox1.Controls.Add( this.radioButtonDirOccB );
			this.groupBox1.Controls.Add( this.radioButtonDirOccG );
			this.groupBox1.Location = new System.Drawing.Point( 596, 323 );
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size( 190, 205 );
			this.groupBox1.TabIndex = 9;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Result Display";
			// 
			// viewportPanelResult
			// 
			this.viewportPanelResult.Image = null;
			this.viewportPanelResult.Location = new System.Drawing.Point( 856, 16 );
			this.viewportPanelResult.MessageOnEmpty = null;
			this.viewportPanelResult.Name = "viewportPanelResult";
			this.viewportPanelResult.Size = new System.Drawing.Size( 512, 512 );
			this.viewportPanelResult.TabIndex = 0;
			this.viewportPanelResult.ViewLinear = false;
			this.viewportPanelResult.ViewMode = GenerateSelfShadowedBumpMap.ImagePanel.VIEW_MODE.RGB;
			this.viewportPanelResult.Click += new System.EventHandler( this.viewportPanelResult_Click );
			// 
			// outputPanelInputHeightMap
			// 
			this.outputPanelInputHeightMap.AllowDrop = true;
			this.outputPanelInputHeightMap.Font = new System.Drawing.Font( "Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
			this.outputPanelInputHeightMap.Image = null;
			this.outputPanelInputHeightMap.Location = new System.Drawing.Point( 12, 12 );
			this.outputPanelInputHeightMap.MessageOnEmpty = "Click to load a height map,\r\nor drag and drop...";
			this.outputPanelInputHeightMap.Name = "outputPanelInputHeightMap";
			this.outputPanelInputHeightMap.Size = new System.Drawing.Size( 512, 512 );
			this.outputPanelInputHeightMap.TabIndex = 0;
			this.outputPanelInputHeightMap.ViewLinear = false;
			this.outputPanelInputHeightMap.ViewMode = GenerateSelfShadowedBumpMap.ImagePanel.VIEW_MODE.RGB;
			this.outputPanelInputHeightMap.Click += new System.EventHandler( this.outputPanelInputHeightMap_Click );
			this.outputPanelInputHeightMap.DragDrop += new System.Windows.Forms.DragEventHandler( this.outputPanelInputHeightMap_DragDrop );
			this.outputPanelInputHeightMap.DragEnter += new System.Windows.Forms.DragEventHandler( this.outputPanelInputHeightMap_DragEnter );
			// 
			// GeneratorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 1379, 540 );
			this.Controls.Add( this.groupBox1 );
			this.Controls.Add( this.tabControlGenerators );
			this.Controls.Add( this.progressBar );
			this.Controls.Add( this.viewportPanelResult );
			this.Controls.Add( this.outputPanelInputHeightMap );
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "GeneratorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Self-Shadowed Bump Map Generator";
			this.tabControlGenerators.ResumeLayout( false );
			this.tabPageSSBump.ResumeLayout( false );
			this.tabPageSSBump.PerformLayout();
			this.tabPageTranslucency.ResumeLayout( false );
			this.tabPageTranslucency.PerformLayout();
			this.groupBox1.ResumeLayout( false );
			this.groupBox1.PerformLayout();
			this.ResumeLayout( false );

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
		private System.Windows.Forms.TabPage tabPageTranslucency;
		private System.Windows.Forms.TabPage tabPageNormal;
		private System.Windows.Forms.CheckBox checkBoxWrapTr;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlRaysCount2;
		private System.Windows.Forms.Button buttonGenerateTranslucency;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDensity;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlThickness;
		private System.Windows.Forms.Label label8;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlBilateralRadiusTr;
		private System.Windows.Forms.Label label9;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlBilateralToleranceTr;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label14;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlPixelDensityTr;
		private System.Windows.Forms.Label label11;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlMaxStepsCountTr;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.GroupBox groupBox1;
	}
}

