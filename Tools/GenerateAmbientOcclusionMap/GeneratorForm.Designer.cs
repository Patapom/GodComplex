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
			this.floatTrackbarControlHeight = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.integerTrackbarControlRaysCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.integerTrackbarControlMaxStepsCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.floatTrackbarControlBilateralRadius = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlPixelDensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlBilateralTolerance = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.floatTrackbarControlMaxConeAngle = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.viewportPanelResult = new GenerateSelfShadowedBumpMap.ImagePanel(this.components);
			this.outputPanelInputHeightMap = new GenerateSelfShadowedBumpMap.ImagePanel(this.components);
			this.panelParameters.SuspendLayout();
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
			this.buttonGenerate.Location = new System.Drawing.Point(111, 250);
			this.buttonGenerate.Name = "buttonGenerate";
			this.buttonGenerate.Size = new System.Drawing.Size(105, 35);
			this.buttonGenerate.TabIndex = 0;
			this.buttonGenerate.Text = "Generate";
			this.buttonGenerate.UseVisualStyleBackColor = true;
			this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(9, 32);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(102, 31);
			this.label3.TabIndex = 3;
			this.label3.Text = "Physical Texture Size (cm)";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(10, 223);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(95, 13);
			this.label7.TabIndex = 3;
			this.label7.Text = "Bilateral Tolerance";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(10, 197);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(80, 13);
			this.label6.TabIndex = 3;
			this.label6.Text = "Bilateral Radius";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(8, 6);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(107, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Encoded Height (cm)";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(8, 102);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(88, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Max Steps Count";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 76);
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
			this.panelParameters.Controls.Add(this.floatTrackbarControlMaxConeAngle);
			this.panelParameters.Controls.Add(this.floatTrackbarControlHeight);
			this.panelParameters.Controls.Add(this.checkBoxWrap);
			this.panelParameters.Controls.Add(this.label6);
			this.panelParameters.Controls.Add(this.label4);
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
			this.panelParameters.Location = new System.Drawing.Point(530, 12);
			this.panelParameters.Name = "panelParameters";
			this.panelParameters.Size = new System.Drawing.Size(320, 291);
			this.panelParameters.TabIndex = 11;
			// 
			// floatTrackbarControlHeight
			// 
			this.floatTrackbarControlHeight.Location = new System.Drawing.Point(113, 3);
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
			this.integerTrackbarControlRaysCount.Location = new System.Drawing.Point(113, 73);
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
			// integerTrackbarControlMaxStepsCount
			// 
			this.integerTrackbarControlMaxStepsCount.Location = new System.Drawing.Point(113, 99);
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
			// floatTrackbarControlBilateralRadius
			// 
			this.floatTrackbarControlBilateralRadius.Location = new System.Drawing.Point(113, 194);
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
			// floatTrackbarControlPixelDensity
			// 
			this.floatTrackbarControlPixelDensity.Location = new System.Drawing.Point(113, 29);
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
			this.floatTrackbarControlBilateralTolerance.Location = new System.Drawing.Point(113, 220);
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
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(8, 128);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(98, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "Max Cone Angle (°)";
			// 
			// floatTrackbarControlMaxConeAngle
			// 
			this.floatTrackbarControlMaxConeAngle.Location = new System.Drawing.Point(113, 125);
			this.floatTrackbarControlMaxConeAngle.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlMaxConeAngle.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlMaxConeAngle.Name = "floatTrackbarControlMaxConeAngle";
			this.floatTrackbarControlMaxConeAngle.RangeMax = 180F;
			this.floatTrackbarControlMaxConeAngle.RangeMin = 1F;
			this.floatTrackbarControlMaxConeAngle.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlMaxConeAngle.TabIndex = 2;
			this.floatTrackbarControlMaxConeAngle.Value = 150F;
			this.floatTrackbarControlMaxConeAngle.VisibleRangeMax = 180F;
			this.floatTrackbarControlMaxConeAngle.VisibleRangeMin = 1F;
			// 
			// viewportPanelResult
			// 
			this.viewportPanelResult.Image = null;
			this.viewportPanelResult.Location = new System.Drawing.Point(855, 12);
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
			// GeneratorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1379, 536);
			this.Controls.Add(this.panelParameters);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.viewportPanelResult);
			this.Controls.Add(this.outputPanelInputHeightMap);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "GeneratorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Ambient Occlusion Map Generator";
			this.panelParameters.ResumeLayout(false);
			this.panelParameters.PerformLayout();
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
		private System.Windows.Forms.OpenFileDialog openFileDialogImage;
		private System.Windows.Forms.SaveFileDialog saveFileDialogImage;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Panel panelParameters;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlMaxConeAngle;
		private System.Windows.Forms.Label label5;
	}
}

