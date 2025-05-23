﻿namespace TestGroundTruthAOFitting
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
			this.buttonComputeIndirect = new System.Windows.Forms.Button();
			this.buttonTestBilateral = new System.Windows.Forms.Button();
			this.floatTrackbarControlMaxConeAngle = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlHeight = new UIUtility.FloatTrackbarControl();
			this.integerTrackbarControlRaysCount = new UIUtility.IntegerTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.integerTrackbarControlMaxStepsCount = new UIUtility.IntegerTrackbarControl();
			this.floatTrackbarControlBilateralRadius = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlPixelDensity = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlBilateralTolerance = new UIUtility.FloatTrackbarControl();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.viewportPanelResult = new TestGroundTruthAOFitting.ImagePanel();
			this.outputPanelInputHeightMap = new TestGroundTruthAOFitting.ImagePanel();
			this.imagePanelNormalMap = new TestGroundTruthAOFitting.ImagePanel();
			this.contextMenuStripNormal = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.clearNormalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.integerTrackbarControlBounceIndex = new UIUtility.IntegerTrackbarControl();
			this.buttonDemo = new System.Windows.Forms.Button();
			this.imagePanelBentCone = new TestGroundTruthAOFitting.ImagePanel();
			this.panelParameters.SuspendLayout();
			this.contextMenuStripNormal.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkBoxWrap
			// 
			this.checkBoxWrap.AutoSize = true;
			this.checkBoxWrap.Checked = true;
			this.checkBoxWrap.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxWrap.Location = new System.Drawing.Point(15, 260);
			this.checkBoxWrap.Name = "checkBoxWrap";
			this.checkBoxWrap.Size = new System.Drawing.Size(43, 17);
			this.checkBoxWrap.TabIndex = 4;
			this.checkBoxWrap.Text = "Tile";
			this.checkBoxWrap.UseVisualStyleBackColor = true;
			// 
			// buttonGenerate
			// 
			this.buttonGenerate.Location = new System.Drawing.Point(74, 250);
			this.buttonGenerate.Name = "buttonGenerate";
			this.buttonGenerate.Size = new System.Drawing.Size(78, 38);
			this.buttonGenerate.TabIndex = 0;
			this.buttonGenerate.Text = "Generate Indirect Map";
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
			this.label7.Location = new System.Drawing.Point(2, 224);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(95, 13);
			this.label7.TabIndex = 3;
			this.label7.Text = "Bilateral Tolerance";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(2, 198);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(80, 13);
			this.label6.TabIndex = 3;
			this.label6.Text = "Bilateral Radius";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(0, 2);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(113, 26);
			this.label2.TabIndex = 3;
			this.label2.Text = "Max Encoded Height (cm)";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(0, 103);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(111, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Search Range (pixels)";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(0, 77);
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
			this.buttonReload.Location = new System.Drawing.Point(775, 437);
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
			this.panelParameters.Controls.Add(this.buttonComputeIndirect);
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
			// buttonComputeIndirect
			// 
			this.buttonComputeIndirect.Location = new System.Drawing.Point(158, 250);
			this.buttonComputeIndirect.Name = "buttonComputeIndirect";
			this.buttonComputeIndirect.Size = new System.Drawing.Size(75, 38);
			this.buttonComputeIndirect.TabIndex = 13;
			this.buttonComputeIndirect.Text = "Compute Indirect";
			this.buttonComputeIndirect.UseVisualStyleBackColor = true;
			this.buttonComputeIndirect.Click += new System.EventHandler(this.buttonComputeIndirect_Click);
			// 
			// buttonTestBilateral
			// 
			this.buttonTestBilateral.Location = new System.Drawing.Point(241, 250);
			this.buttonTestBilateral.Name = "buttonTestBilateral";
			this.buttonTestBilateral.Size = new System.Drawing.Size(72, 38);
			this.buttonTestBilateral.TabIndex = 12;
			this.buttonTestBilateral.Text = "Compile";
			this.buttonTestBilateral.UseVisualStyleBackColor = true;
			this.buttonTestBilateral.Click += new System.EventHandler(this.buttonTestBilateral_Click);
			// 
			// floatTrackbarControlMaxConeAngle
			// 
			this.floatTrackbarControlMaxConeAngle.Location = new System.Drawing.Point(119, 125);
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
			this.integerTrackbarControlRaysCount.Location = new System.Drawing.Point(119, 73);
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
			this.label5.Location = new System.Drawing.Point(0, 129);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(112, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "Search Cone Angle (°)";
			// 
			// integerTrackbarControlMaxStepsCount
			// 
			this.integerTrackbarControlMaxStepsCount.Location = new System.Drawing.Point(119, 99);
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
			this.floatTrackbarControlBilateralRadius.Location = new System.Drawing.Point(119, 194);
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
			this.floatTrackbarControlBilateralTolerance.Location = new System.Drawing.Point(119, 220);
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
			// integerTrackbarControlBounceIndex
			// 
			this.integerTrackbarControlBounceIndex.Location = new System.Drawing.Point(726, 397);
			this.integerTrackbarControlBounceIndex.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlBounceIndex.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlBounceIndex.Name = "integerTrackbarControlBounceIndex";
			this.integerTrackbarControlBounceIndex.RangeMax = 20;
			this.integerTrackbarControlBounceIndex.RangeMin = 0;
			this.integerTrackbarControlBounceIndex.Size = new System.Drawing.Size(124, 20);
			this.integerTrackbarControlBounceIndex.TabIndex = 1;
			this.integerTrackbarControlBounceIndex.Value = 0;
			this.integerTrackbarControlBounceIndex.VisibleRangeMax = 8;
			this.integerTrackbarControlBounceIndex.ValueChanged += new UIUtility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlBounceIndex_ValueChanged);
			// 
			// buttonDemo
			// 
			this.buttonDemo.Location = new System.Drawing.Point(775, 347);
			this.buttonDemo.Name = "buttonDemo";
			this.buttonDemo.Size = new System.Drawing.Size(75, 44);
			this.buttonDemo.TabIndex = 12;
			this.buttonDemo.Text = "Demo";
			this.buttonDemo.UseVisualStyleBackColor = true;
			this.buttonDemo.Click += new System.EventHandler(this.buttonDemo_Click);
			// 
			// imagePanelBentCone
			// 
			this.imagePanelBentCone.AllowDrop = true;
			this.imagePanelBentCone.Bitmap = null;
			this.imagePanelBentCone.Brightness = 0F;
			this.imagePanelBentCone.Contrast = 0F;
			this.imagePanelBentCone.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.imagePanelBentCone.Gamma = 0F;
			this.imagePanelBentCone.Location = new System.Drawing.Point(726, 474);
			this.imagePanelBentCone.MessageOnEmpty = "Bent cone";
			this.imagePanelBentCone.Name = "imagePanelBentCone";
			this.imagePanelBentCone.Size = new System.Drawing.Size(50, 50);
			this.imagePanelBentCone.TabIndex = 0;
			this.imagePanelBentCone.ViewLinear = false;
			this.imagePanelBentCone.Click += new System.EventHandler(this.outputPanelInputBentCone_Click);
			// 
			// GeneratorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1379, 536);
			this.Controls.Add(this.buttonDemo);
			this.Controls.Add(this.imagePanelBentCone);
			this.Controls.Add(this.imagePanelNormalMap);
			this.Controls.Add(this.panelParameters);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.integerTrackbarControlBounceIndex);
			this.Controls.Add(this.viewportPanelResult);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.outputPanelInputHeightMap);
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
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private ImagePanel imagePanelNormalMap;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripNormal;
		private System.Windows.Forms.ToolStripMenuItem clearNormalToolStripMenuItem;
		private System.Windows.Forms.Button buttonComputeIndirect;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlBounceIndex;
		private System.Windows.Forms.Button buttonDemo;
		private ImagePanel imagePanelBentCone;
	}
}

