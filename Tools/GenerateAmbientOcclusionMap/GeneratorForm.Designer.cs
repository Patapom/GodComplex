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
			this.checkBoxWrap = new System.Windows.Forms.CheckBox();
			this.buttonGenerate = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.openFileDialogImage = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogImage = new System.Windows.Forms.SaveFileDialog();
			this.buttonReload = new System.Windows.Forms.Button();
			this.panelParameters = new System.Windows.Forms.Panel();
			this.buttonGenerateBentCone = new System.Windows.Forms.Button();
			this.buttonTestBilateral = new System.Windows.Forms.Button();
			this.floatTrackbarControlMaxConeAngle = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlHeight = new UIUtility.FloatTrackbarControl();
			this.integerTrackbarControlRaysCount = new UIUtility.IntegerTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.integerTrackbarControlMaxStepsCount = new UIUtility.IntegerTrackbarControl();
			this.floatTrackbarControlBilateralRadius = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlPixelDensity = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlBilateralTolerance = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlBrightness = new UIUtility.FloatTrackbarControl();
			this.label8 = new System.Windows.Forms.Label();
			this.floatTrackbarControlContrast = new UIUtility.FloatTrackbarControl();
			this.label9 = new System.Windows.Forms.Label();
			this.floatTrackbarControlGamma = new UIUtility.FloatTrackbarControl();
			this.label10 = new System.Windows.Forms.Label();
			this.checkBoxViewsRGB = new System.Windows.Forms.CheckBox();
			this.viewportPanelResult = new GenerateSelfShadowedBumpMap.ImagePanel();
			this.outputPanelInputHeightMap = new GenerateSelfShadowedBumpMap.ImagePanel();
			this.imagePanelNormalMap = new GenerateSelfShadowedBumpMap.ImagePanel();
			this.contextMenuStripNormal = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.clearNormalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.checkBoxBruteForce = new System.Windows.Forms.CheckBox();
			this.panelParameters.SuspendLayout();
			this.contextMenuStripNormal.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkBoxWrap
			// 
			this.checkBoxWrap.AutoSize = true;
			this.checkBoxWrap.Checked = true;
			this.checkBoxWrap.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxWrap.Location = new System.Drawing.Point(119, 56);
			this.checkBoxWrap.Name = "checkBoxWrap";
			this.checkBoxWrap.Size = new System.Drawing.Size(43, 17);
			this.checkBoxWrap.TabIndex = 4;
			this.checkBoxWrap.Text = "Tile";
			this.checkBoxWrap.UseVisualStyleBackColor = true;
			// 
			// buttonGenerate
			// 
			this.buttonGenerate.Location = new System.Drawing.Point(105, 256);
			this.buttonGenerate.Name = "buttonGenerate";
			this.buttonGenerate.Size = new System.Drawing.Size(105, 35);
			this.buttonGenerate.TabIndex = 0;
			this.buttonGenerate.Text = "Generate AO";
			this.buttonGenerate.UseVisualStyleBackColor = true;
			this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(1, 31);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(102, 31);
			this.label3.TabIndex = 3;
			this.label3.Text = "Physical Texture Size (cm)";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(1, 196);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(95, 13);
			this.label7.TabIndex = 3;
			this.label7.Text = "Bilateral Tolerance";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(1, 175);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(80, 13);
			this.label6.TabIndex = 3;
			this.label6.Text = "Bilateral Radius";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(1, 2);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(113, 26);
			this.label2.TabIndex = 3;
			this.label2.Text = "Max Encoded Height (cm)";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(1, 109);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(111, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Search Range (pixels)";
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
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(530, 309);
			this.progressBar.Maximum = 1000;
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(320, 23);
			this.progressBar.TabIndex = 4;
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
			// panelParameters
			// 
			this.panelParameters.Controls.Add(this.checkBoxBruteForce);
			this.panelParameters.Controls.Add(this.buttonGenerateBentCone);
			this.panelParameters.Controls.Add(this.buttonTestBilateral);
			this.panelParameters.Controls.Add(this.floatTrackbarControlMaxConeAngle);
			this.panelParameters.Controls.Add(this.floatTrackbarControlHeight);
			this.panelParameters.Controls.Add(this.checkBoxWrap);
			this.panelParameters.Controls.Add(this.label6);
			this.panelParameters.Controls.Add(this.integerTrackbarControlRaysCount);
			this.panelParameters.Controls.Add(this.label7);
			this.panelParameters.Controls.Add(this.label1);
			this.panelParameters.Controls.Add(this.buttonGenerate);
			this.panelParameters.Controls.Add(this.label5);
			this.panelParameters.Controls.Add(this.label3);
			this.panelParameters.Controls.Add(this.label2);
			this.panelParameters.Controls.Add(this.integerTrackbarControlMaxStepsCount);
			this.panelParameters.Controls.Add(this.floatTrackbarControlBilateralRadius);
			this.panelParameters.Controls.Add(this.floatTrackbarControlPixelDensity);
			this.panelParameters.Controls.Add(this.floatTrackbarControlBilateralTolerance);
			this.panelParameters.Controls.Add(this.label4);
			this.panelParameters.Enabled = false;
			this.panelParameters.Location = new System.Drawing.Point(530, 12);
			this.panelParameters.Name = "panelParameters";
			this.panelParameters.Size = new System.Drawing.Size(320, 291);
			this.panelParameters.TabIndex = 11;
			// 
			// buttonGenerateBentCone
			// 
			this.buttonGenerateBentCone.Location = new System.Drawing.Point(216, 256);
			this.buttonGenerateBentCone.Name = "buttonGenerateBentCone";
			this.buttonGenerateBentCone.Size = new System.Drawing.Size(103, 35);
			this.buttonGenerateBentCone.TabIndex = 13;
			this.buttonGenerateBentCone.Text = "Generate\nBent Cone";
			this.buttonGenerateBentCone.UseVisualStyleBackColor = true;
			this.buttonGenerateBentCone.Click += new System.EventHandler(this.buttonGenerateBentCone_Click);
			// 
			// buttonTestBilateral
			// 
			this.buttonTestBilateral.Location = new System.Drawing.Point(0, 217);
			this.buttonTestBilateral.Name = "buttonTestBilateral";
			this.buttonTestBilateral.Size = new System.Drawing.Size(84, 23);
			this.buttonTestBilateral.TabIndex = 12;
			this.buttonTestBilateral.Text = "Test Bilateral";
			this.buttonTestBilateral.UseVisualStyleBackColor = true;
			this.buttonTestBilateral.Click += new System.EventHandler(this.buttonTestBilateral_Click);
			// 
			// floatTrackbarControlMaxConeAngle
			// 
			this.floatTrackbarControlMaxConeAngle.Location = new System.Drawing.Point(119, 131);
			this.floatTrackbarControlMaxConeAngle.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlMaxConeAngle.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlMaxConeAngle.Name = "floatTrackbarControlMaxConeAngle";
			this.floatTrackbarControlMaxConeAngle.RangeMax = 180F;
			this.floatTrackbarControlMaxConeAngle.RangeMin = 1F;
			this.floatTrackbarControlMaxConeAngle.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlMaxConeAngle.TabIndex = 2;
			this.floatTrackbarControlMaxConeAngle.Value = 179F;
			this.floatTrackbarControlMaxConeAngle.VisibleRangeMax = 180F;
			this.floatTrackbarControlMaxConeAngle.VisibleRangeMin = 1F;
			this.floatTrackbarControlMaxConeAngle.SliderDragStop += new UIUtility.FloatTrackbarControl.SliderDragStopEventHandler(this.floatTrackbarControlMaxConeAngle_SliderDragStop);
			// 
			// floatTrackbarControlHeight
			// 
			this.floatTrackbarControlHeight.Location = new System.Drawing.Point(119, 3);
			this.floatTrackbarControlHeight.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlHeight.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlHeight.Name = "floatTrackbarControlHeight";
			this.floatTrackbarControlHeight.RangeMax = 1000F;
			this.floatTrackbarControlHeight.RangeMin = 0.01F;
			this.floatTrackbarControlHeight.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlHeight.TabIndex = 2;
			this.floatTrackbarControlHeight.Value = 45F;
			this.floatTrackbarControlHeight.VisibleRangeMax = 100F;
			this.floatTrackbarControlHeight.VisibleRangeMin = 0.01F;
			// 
			// integerTrackbarControlRaysCount
			// 
			this.integerTrackbarControlRaysCount.Location = new System.Drawing.Point(119, 79);
			this.integerTrackbarControlRaysCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlRaysCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlRaysCount.Name = "integerTrackbarControlRaysCount";
			this.integerTrackbarControlRaysCount.RangeMax = 1024;
			this.integerTrackbarControlRaysCount.RangeMin = 1;
			this.integerTrackbarControlRaysCount.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlRaysCount.TabIndex = 1;
			this.integerTrackbarControlRaysCount.Value = 1024;
			this.integerTrackbarControlRaysCount.VisibleRangeMax = 1024;
			this.integerTrackbarControlRaysCount.VisibleRangeMin = 1;
			this.integerTrackbarControlRaysCount.SliderDragStop += new UIUtility.IntegerTrackbarControl.SliderDragStopEventHandler(this.integerTrackbarControlRaysCount_SliderDragStop);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(1, 135);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(112, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "Search Cone Angle (°)";
			// 
			// integerTrackbarControlMaxStepsCount
			// 
			this.integerTrackbarControlMaxStepsCount.Location = new System.Drawing.Point(119, 105);
			this.integerTrackbarControlMaxStepsCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlMaxStepsCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlMaxStepsCount.Name = "integerTrackbarControlMaxStepsCount";
			this.integerTrackbarControlMaxStepsCount.RangeMax = 400;
			this.integerTrackbarControlMaxStepsCount.RangeMin = 1;
			this.integerTrackbarControlMaxStepsCount.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlMaxStepsCount.TabIndex = 1;
			this.integerTrackbarControlMaxStepsCount.Value = 200;
			this.integerTrackbarControlMaxStepsCount.VisibleRangeMax = 200;
			this.integerTrackbarControlMaxStepsCount.VisibleRangeMin = 1;
			// 
			// floatTrackbarControlBilateralRadius
			// 
			this.floatTrackbarControlBilateralRadius.Location = new System.Drawing.Point(120, 171);
			this.floatTrackbarControlBilateralRadius.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBilateralRadius.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBilateralRadius.Name = "floatTrackbarControlBilateralRadius";
			this.floatTrackbarControlBilateralRadius.RangeMax = 32F;
			this.floatTrackbarControlBilateralRadius.RangeMin = 0.001F;
			this.floatTrackbarControlBilateralRadius.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlBilateralRadius.TabIndex = 2;
			this.floatTrackbarControlBilateralRadius.Value = 1F;
			this.floatTrackbarControlBilateralRadius.VisibleRangeMax = 32F;
			this.floatTrackbarControlBilateralRadius.VisibleRangeMin = 0.001F;
			// 
			// floatTrackbarControlPixelDensity
			// 
			this.floatTrackbarControlPixelDensity.Location = new System.Drawing.Point(119, 30);
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
			this.floatTrackbarControlBilateralTolerance.Location = new System.Drawing.Point(120, 195);
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
			// floatTrackbarControlBrightness
			// 
			this.floatTrackbarControlBrightness.Location = new System.Drawing.Point(641, 360);
			this.floatTrackbarControlBrightness.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBrightness.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBrightness.Name = "floatTrackbarControlBrightness";
			this.floatTrackbarControlBrightness.RangeMax = 1F;
			this.floatTrackbarControlBrightness.RangeMin = -1F;
			this.floatTrackbarControlBrightness.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlBrightness.TabIndex = 2;
			this.floatTrackbarControlBrightness.Value = 0F;
			this.floatTrackbarControlBrightness.Visible = false;
			this.floatTrackbarControlBrightness.VisibleRangeMax = 1F;
			this.floatTrackbarControlBrightness.VisibleRangeMin = -1F;
			this.floatTrackbarControlBrightness.SliderDragStop += new UIUtility.FloatTrackbarControl.SliderDragStopEventHandler(this.floatTrackbarControlBrightness_SliderDragStop);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(579, 363);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(56, 13);
			this.label8.TabIndex = 3;
			this.label8.Text = "Brightness";
			this.label8.Visible = false;
			// 
			// floatTrackbarControlContrast
			// 
			this.floatTrackbarControlContrast.Location = new System.Drawing.Point(641, 386);
			this.floatTrackbarControlContrast.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlContrast.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlContrast.Name = "floatTrackbarControlContrast";
			this.floatTrackbarControlContrast.RangeMax = 1F;
			this.floatTrackbarControlContrast.RangeMin = -1F;
			this.floatTrackbarControlContrast.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlContrast.TabIndex = 2;
			this.floatTrackbarControlContrast.Value = 0F;
			this.floatTrackbarControlContrast.Visible = false;
			this.floatTrackbarControlContrast.VisibleRangeMax = 1F;
			this.floatTrackbarControlContrast.VisibleRangeMin = -1F;
			this.floatTrackbarControlContrast.SliderDragStop += new UIUtility.FloatTrackbarControl.SliderDragStopEventHandler(this.floatTrackbarControlContrast_SliderDragStop);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(579, 389);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(46, 13);
			this.label9.TabIndex = 3;
			this.label9.Text = "Contrast";
			this.label9.Visible = false;
			// 
			// floatTrackbarControlGamma
			// 
			this.floatTrackbarControlGamma.Location = new System.Drawing.Point(641, 412);
			this.floatTrackbarControlGamma.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlGamma.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlGamma.Name = "floatTrackbarControlGamma";
			this.floatTrackbarControlGamma.RangeMax = 1F;
			this.floatTrackbarControlGamma.RangeMin = -1F;
			this.floatTrackbarControlGamma.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlGamma.TabIndex = 2;
			this.floatTrackbarControlGamma.Value = 0F;
			this.floatTrackbarControlGamma.Visible = false;
			this.floatTrackbarControlGamma.VisibleRangeMax = 1F;
			this.floatTrackbarControlGamma.VisibleRangeMin = -1F;
			this.floatTrackbarControlGamma.SliderDragStop += new UIUtility.FloatTrackbarControl.SliderDragStopEventHandler(this.floatTrackbarControlGamma_SliderDragStop);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(579, 415);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(43, 13);
			this.label10.TabIndex = 3;
			this.label10.Text = "Gamma";
			this.label10.Visible = false;
			// 
			// checkBoxViewsRGB
			// 
			this.checkBoxViewsRGB.AutoSize = true;
			this.checkBoxViewsRGB.Checked = true;
			this.checkBoxViewsRGB.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxViewsRGB.Location = new System.Drawing.Point(750, 338);
			this.checkBoxViewsRGB.Name = "checkBoxViewsRGB";
			this.checkBoxViewsRGB.Size = new System.Drawing.Size(94, 17);
			this.checkBoxViewsRGB.TabIndex = 4;
			this.checkBoxViewsRGB.Text = "View as sRGB";
			this.checkBoxViewsRGB.UseVisualStyleBackColor = true;
			this.checkBoxViewsRGB.Visible = false;
			this.checkBoxViewsRGB.CheckedChanged += new System.EventHandler(this.checkBoxViewsRGB_CheckedChanged);
			// 
			// viewportPanelResult
			// 
			this.viewportPanelResult.Bitmap = null;
			this.viewportPanelResult.Brightness = 0F;
			this.viewportPanelResult.Contrast = 0F;
			this.viewportPanelResult.Gamma = 0F;
			this.viewportPanelResult.Location = new System.Drawing.Point(855, 12);
			this.viewportPanelResult.MessageOnEmpty = null;
			this.viewportPanelResult.Name = "viewportPanelResult";
			this.viewportPanelResult.Size = new System.Drawing.Size(512, 512);
			this.viewportPanelResult.TabIndex = 0;
			this.viewportPanelResult.ViewLinear = false;
			this.viewportPanelResult.Click += new System.EventHandler(this.viewportPanelResult_Click);
			// 
			// outputPanelInputHeightMap
			// 
			this.outputPanelInputHeightMap.AllowDrop = true;
			this.outputPanelInputHeightMap.Bitmap = null;
			this.outputPanelInputHeightMap.Brightness = 0F;
			this.outputPanelInputHeightMap.Contrast = 0F;
			this.outputPanelInputHeightMap.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.outputPanelInputHeightMap.Gamma = 0F;
			this.outputPanelInputHeightMap.Location = new System.Drawing.Point(12, 12);
			this.outputPanelInputHeightMap.MessageOnEmpty = "Click to load a height map,\r\nor drag and drop...";
			this.outputPanelInputHeightMap.Name = "outputPanelInputHeightMap";
			this.outputPanelInputHeightMap.Size = new System.Drawing.Size(512, 512);
			this.outputPanelInputHeightMap.TabIndex = 0;
			this.outputPanelInputHeightMap.ViewLinear = false;
			this.outputPanelInputHeightMap.Click += new System.EventHandler(this.outputPanelInputHeightMap_Click);
			this.outputPanelInputHeightMap.DragDrop += new System.Windows.Forms.DragEventHandler(this.outputPanelInputHeightMap_DragDrop);
			this.outputPanelInputHeightMap.DragEnter += new System.Windows.Forms.DragEventHandler(this.outputPanelInputHeightMap_DragEnter);
			// 
			// imagePanelNormalMap
			// 
			this.imagePanelNormalMap.AllowDrop = true;
			this.imagePanelNormalMap.Bitmap = null;
			this.imagePanelNormalMap.Brightness = 0F;
			this.imagePanelNormalMap.ContextMenuStrip = this.contextMenuStripNormal;
			this.imagePanelNormalMap.Contrast = 0F;
			this.imagePanelNormalMap.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.imagePanelNormalMap.Gamma = 0F;
			this.imagePanelNormalMap.Location = new System.Drawing.Point(533, 338);
			this.imagePanelNormalMap.MessageOnEmpty = "Click to load a normal map,\r\nor drag and drop...";
			this.imagePanelNormalMap.Name = "imagePanelNormalMap";
			this.imagePanelNormalMap.Size = new System.Drawing.Size(186, 186);
			this.imagePanelNormalMap.TabIndex = 0;
			this.imagePanelNormalMap.ViewLinear = false;
			this.imagePanelNormalMap.Click += new System.EventHandler(this.outputPanelInputNormalMap_Click);
			this.imagePanelNormalMap.DragDrop += new System.Windows.Forms.DragEventHandler(this.outputPanelInputNormalMap_DragDrop);
			this.imagePanelNormalMap.DragEnter += new System.Windows.Forms.DragEventHandler(this.outputPanelInputNormalMap_DragEnter);
			// 
			// contextMenuStripNormal
			// 
			this.contextMenuStripNormal.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearNormalToolStripMenuItem});
			this.contextMenuStripNormal.Name = "contextMenuStripNormal";
			this.contextMenuStripNormal.Size = new System.Drawing.Size(145, 26);
			// 
			// clearNormalToolStripMenuItem
			// 
			this.clearNormalToolStripMenuItem.Name = "clearNormalToolStripMenuItem";
			this.clearNormalToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.clearNormalToolStripMenuItem.Text = "Clear Normal";
			this.clearNormalToolStripMenuItem.Click += new System.EventHandler(this.clearNormalToolStripMenuItem_Click);
			// 
			// checkBoxBruteForce
			// 
			this.checkBoxBruteForce.AutoSize = true;
			this.checkBoxBruteForce.Location = new System.Drawing.Point(220, 233);
			this.checkBoxBruteForce.Name = "checkBoxBruteForce";
			this.checkBoxBruteForce.Size = new System.Drawing.Size(81, 17);
			this.checkBoxBruteForce.TabIndex = 14;
			this.checkBoxBruteForce.Text = "Brute Force";
			this.checkBoxBruteForce.UseVisualStyleBackColor = true;
			this.checkBoxBruteForce.Visible = false;
			// 
			// GeneratorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1379, 536);
			this.Controls.Add(this.imagePanelNormalMap);
			this.Controls.Add(this.panelParameters);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.checkBoxViewsRGB);
			this.Controls.Add(this.viewportPanelResult);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.outputPanelInputHeightMap);
			this.Controls.Add(this.floatTrackbarControlGamma);
			this.Controls.Add(this.floatTrackbarControlContrast);
			this.Controls.Add(this.floatTrackbarControlBrightness);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "GeneratorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Ambient Occlusion Map Generator";
			this.panelParameters.ResumeLayout(false);
			this.panelParameters.PerformLayout();
			this.contextMenuStripNormal.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ImagePanel outputPanelInputHeightMap;
		private UIUtility.FloatTrackbarControl floatTrackbarControlHeight;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlRaysCount;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private ImagePanel viewportPanelResult;
		private System.Windows.Forms.Label label3;
		private UIUtility.FloatTrackbarControl floatTrackbarControlPixelDensity;
		private System.Windows.Forms.Button buttonGenerate;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Label label4;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlMaxStepsCount;
		private System.Windows.Forms.Label label6;
		private UIUtility.FloatTrackbarControl floatTrackbarControlBilateralRadius;
		private System.Windows.Forms.Label label7;
		private UIUtility.FloatTrackbarControl floatTrackbarControlBilateralTolerance;
		private System.Windows.Forms.CheckBox checkBoxWrap;
		private System.Windows.Forms.OpenFileDialog openFileDialogImage;
		private System.Windows.Forms.SaveFileDialog saveFileDialogImage;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Panel panelParameters;
		private UIUtility.FloatTrackbarControl floatTrackbarControlMaxConeAngle;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button buttonTestBilateral;
		private UIUtility.FloatTrackbarControl floatTrackbarControlBrightness;
		private System.Windows.Forms.Label label8;
		private UIUtility.FloatTrackbarControl floatTrackbarControlContrast;
		private System.Windows.Forms.Label label9;
		private UIUtility.FloatTrackbarControl floatTrackbarControlGamma;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.CheckBox checkBoxViewsRGB;
		private ImagePanel imagePanelNormalMap;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripNormal;
		private System.Windows.Forms.ToolStripMenuItem clearNormalToolStripMenuItem;
		private System.Windows.Forms.Button buttonGenerateBentCone;
		private System.Windows.Forms.CheckBox checkBoxBruteForce;
	}
}

