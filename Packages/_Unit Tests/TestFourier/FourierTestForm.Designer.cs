namespace TestFourier
{
	partial class FourierTestForm
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
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1D = new System.Windows.Forms.TabPage();
			this.checkBoxUseFFTW = new System.Windows.Forms.CheckBox();
			this.checkBoxGPU = new System.Windows.Forms.CheckBox();
			this.labelDiff = new System.Windows.Forms.Label();
			this.checkBoxShowInput = new System.Windows.Forms.CheckBox();
			this.checkBoxShowReconstructedSignal = new System.Windows.Forms.CheckBox();
			this.checkBoxInvertFilter = new System.Windows.Forms.CheckBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.radioButtonFilterInverse = new System.Windows.Forms.RadioButton();
			this.radioButtonFilterGaussian = new System.Windows.Forms.RadioButton();
			this.radioButtonFilterExponential = new System.Windows.Forms.RadioButton();
			this.radioButtonFilterCutShort = new System.Windows.Forms.RadioButton();
			this.radioButtonFilterCutMedium = new System.Windows.Forms.RadioButton();
			this.radioButtonFilterCutLarge = new System.Windows.Forms.RadioButton();
			this.radioButtonFilterNone = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.radioButtonRandom = new System.Windows.Forms.RadioButton();
			this.radioButtonSinc = new System.Windows.Forms.RadioButton();
			this.radioButtonTriangle = new System.Windows.Forms.RadioButton();
			this.radioButtonSine = new System.Windows.Forms.RadioButton();
			this.radioButtonSquare = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.imagePanel = new TestFourier.ImagePanel();
			this.tabPage2D = new System.Windows.Forms.TabPage();
			this.label4 = new System.Windows.Forms.Label();
			this.floatTrackbarControlScaleV = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlScaleU = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.panel3 = new System.Windows.Forms.Panel();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.radioButton2 = new System.Windows.Forms.RadioButton();
			this.radioButton3 = new System.Windows.Forms.RadioButton();
			this.radioButton4 = new System.Windows.Forms.RadioButton();
			this.radioButtonSquare2D = new System.Windows.Forms.RadioButton();
			this.label3 = new System.Windows.Forms.Label();
			this.imagePanel2D = new TestFourier.ImagePanel();
			this.buttonReload = new System.Windows.Forms.Button();
			this.tabControl1.SuspendLayout();
			this.tabPage1D.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.tabPage2D.SuspendLayout();
			this.panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 10;
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1D);
			this.tabControl1.Controls.Add(this.tabPage2D);
			this.tabControl1.Location = new System.Drawing.Point(12, 12);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(1270, 733);
			this.tabControl1.TabIndex = 1;
			// 
			// tabPage1D
			// 
			this.tabPage1D.Controls.Add(this.checkBoxUseFFTW);
			this.tabPage1D.Controls.Add(this.checkBoxGPU);
			this.tabPage1D.Controls.Add(this.labelDiff);
			this.tabPage1D.Controls.Add(this.checkBoxShowInput);
			this.tabPage1D.Controls.Add(this.checkBoxShowReconstructedSignal);
			this.tabPage1D.Controls.Add(this.checkBoxInvertFilter);
			this.tabPage1D.Controls.Add(this.panel2);
			this.tabPage1D.Controls.Add(this.label2);
			this.tabPage1D.Controls.Add(this.panel1);
			this.tabPage1D.Controls.Add(this.label1);
			this.tabPage1D.Controls.Add(this.imagePanel);
			this.tabPage1D.Location = new System.Drawing.Point(4, 22);
			this.tabPage1D.Name = "tabPage1D";
			this.tabPage1D.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1D.Size = new System.Drawing.Size(1262, 707);
			this.tabPage1D.TabIndex = 0;
			this.tabPage1D.Text = "FFT 1D";
			this.tabPage1D.UseVisualStyleBackColor = true;
			// 
			// checkBoxUseFFTW
			// 
			this.checkBoxUseFFTW.AutoSize = true;
			this.checkBoxUseFFTW.Location = new System.Drawing.Point(1016, 14);
			this.checkBoxUseFFTW.Name = "checkBoxUseFFTW";
			this.checkBoxUseFFTW.Size = new System.Drawing.Size(147, 17);
			this.checkBoxUseFFTW.TabIndex = 6;
			this.checkBoxUseFFTW.Text = "Use FFTW (for reference)";
			this.checkBoxUseFFTW.UseVisualStyleBackColor = true;
			// 
			// checkBoxGPU
			// 
			this.checkBoxGPU.AutoSize = true;
			this.checkBoxGPU.Checked = true;
			this.checkBoxGPU.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxGPU.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxGPU.Location = new System.Drawing.Point(862, 10);
			this.checkBoxGPU.Name = "checkBoxGPU";
			this.checkBoxGPU.Size = new System.Drawing.Size(98, 22);
			this.checkBoxGPU.TabIndex = 5;
			this.checkBoxGPU.Text = "Use GPU";
			this.checkBoxGPU.UseVisualStyleBackColor = true;
			this.checkBoxGPU.CheckedChanged += new System.EventHandler(this.checkBoxGPU_CheckedChanged);
			// 
			// labelDiff
			// 
			this.labelDiff.AutoSize = true;
			this.labelDiff.Location = new System.Drawing.Point(669, 590);
			this.labelDiff.Name = "labelDiff";
			this.labelDiff.Size = new System.Drawing.Size(46, 13);
			this.labelDiff.TabIndex = 4;
			this.labelDiff.Text = "BISOU !";
			// 
			// checkBoxShowInput
			// 
			this.checkBoxShowInput.AutoSize = true;
			this.checkBoxShowInput.Checked = true;
			this.checkBoxShowInput.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowInput.Location = new System.Drawing.Point(503, 14);
			this.checkBoxShowInput.Name = "checkBoxShowInput";
			this.checkBoxShowInput.Size = new System.Drawing.Size(112, 17);
			this.checkBoxShowInput.TabIndex = 3;
			this.checkBoxShowInput.Text = "Show Input Signal";
			this.checkBoxShowInput.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowReconstructedSignal
			// 
			this.checkBoxShowReconstructedSignal.AutoSize = true;
			this.checkBoxShowReconstructedSignal.Checked = true;
			this.checkBoxShowReconstructedSignal.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowReconstructedSignal.Location = new System.Drawing.Point(621, 14);
			this.checkBoxShowReconstructedSignal.Name = "checkBoxShowReconstructedSignal";
			this.checkBoxShowReconstructedSignal.Size = new System.Drawing.Size(158, 17);
			this.checkBoxShowReconstructedSignal.TabIndex = 3;
			this.checkBoxShowReconstructedSignal.Text = "Show Reconstructed Signal";
			this.checkBoxShowReconstructedSignal.UseVisualStyleBackColor = true;
			// 
			// checkBoxInvertFilter
			// 
			this.checkBoxInvertFilter.AutoSize = true;
			this.checkBoxInvertFilter.Location = new System.Drawing.Point(51, 590);
			this.checkBoxInvertFilter.Name = "checkBoxInvertFilter";
			this.checkBoxInvertFilter.Size = new System.Drawing.Size(148, 17);
			this.checkBoxInvertFilter.TabIndex = 3;
			this.checkBoxInvertFilter.Text = "Apply to Low-Frequencies";
			this.checkBoxInvertFilter.UseVisualStyleBackColor = true;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.radioButtonFilterInverse);
			this.panel2.Controls.Add(this.radioButtonFilterGaussian);
			this.panel2.Controls.Add(this.radioButtonFilterExponential);
			this.panel2.Controls.Add(this.radioButtonFilterCutShort);
			this.panel2.Controls.Add(this.radioButtonFilterCutMedium);
			this.panel2.Controls.Add(this.radioButtonFilterCutLarge);
			this.panel2.Controls.Add(this.radioButtonFilterNone);
			this.panel2.Location = new System.Drawing.Point(48, 549);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(934, 34);
			this.panel2.TabIndex = 2;
			// 
			// radioButtonFilterInverse
			// 
			this.radioButtonFilterInverse.AutoSize = true;
			this.radioButtonFilterInverse.Location = new System.Drawing.Point(460, 10);
			this.radioButtonFilterInverse.Name = "radioButtonFilterInverse";
			this.radioButtonFilterInverse.Size = new System.Drawing.Size(60, 17);
			this.radioButtonFilterInverse.TabIndex = 0;
			this.radioButtonFilterInverse.Text = "Inverse";
			this.radioButtonFilterInverse.UseVisualStyleBackColor = true;
			this.radioButtonFilterInverse.CheckedChanged += new System.EventHandler(this.radioButtonFilterNone_CheckedChanged);
			// 
			// radioButtonFilterGaussian
			// 
			this.radioButtonFilterGaussian.AutoSize = true;
			this.radioButtonFilterGaussian.Location = new System.Drawing.Point(385, 10);
			this.radioButtonFilterGaussian.Name = "radioButtonFilterGaussian";
			this.radioButtonFilterGaussian.Size = new System.Drawing.Size(69, 17);
			this.radioButtonFilterGaussian.TabIndex = 0;
			this.radioButtonFilterGaussian.Text = "Gaussian";
			this.radioButtonFilterGaussian.UseVisualStyleBackColor = true;
			this.radioButtonFilterGaussian.CheckedChanged += new System.EventHandler(this.radioButtonFilterNone_CheckedChanged);
			// 
			// radioButtonFilterExponential
			// 
			this.radioButtonFilterExponential.AutoSize = true;
			this.radioButtonFilterExponential.Location = new System.Drawing.Point(299, 10);
			this.radioButtonFilterExponential.Name = "radioButtonFilterExponential";
			this.radioButtonFilterExponential.Size = new System.Drawing.Size(80, 17);
			this.radioButtonFilterExponential.TabIndex = 0;
			this.radioButtonFilterExponential.Text = "Exponential";
			this.radioButtonFilterExponential.UseVisualStyleBackColor = true;
			this.radioButtonFilterExponential.CheckedChanged += new System.EventHandler(this.radioButtonFilterNone_CheckedChanged);
			// 
			// radioButtonFilterCutShort
			// 
			this.radioButtonFilterCutShort.AutoSize = true;
			this.radioButtonFilterCutShort.Location = new System.Drawing.Point(224, 10);
			this.radioButtonFilterCutShort.Name = "radioButtonFilterCutShort";
			this.radioButtonFilterCutShort.Size = new System.Drawing.Size(69, 17);
			this.radioButtonFilterCutShort.TabIndex = 0;
			this.radioButtonFilterCutShort.Text = "Cut Short";
			this.radioButtonFilterCutShort.UseVisualStyleBackColor = true;
			this.radioButtonFilterCutShort.CheckedChanged += new System.EventHandler(this.radioButtonFilterNone_CheckedChanged);
			// 
			// radioButtonFilterCutMedium
			// 
			this.radioButtonFilterCutMedium.AutoSize = true;
			this.radioButtonFilterCutMedium.Location = new System.Drawing.Point(137, 10);
			this.radioButtonFilterCutMedium.Name = "radioButtonFilterCutMedium";
			this.radioButtonFilterCutMedium.Size = new System.Drawing.Size(81, 17);
			this.radioButtonFilterCutMedium.TabIndex = 0;
			this.radioButtonFilterCutMedium.Text = "Cut Medium";
			this.radioButtonFilterCutMedium.UseVisualStyleBackColor = true;
			this.radioButtonFilterCutMedium.CheckedChanged += new System.EventHandler(this.radioButtonFilterNone_CheckedChanged);
			// 
			// radioButtonFilterCutLarge
			// 
			this.radioButtonFilterCutLarge.AutoSize = true;
			this.radioButtonFilterCutLarge.Location = new System.Drawing.Point(60, 10);
			this.radioButtonFilterCutLarge.Name = "radioButtonFilterCutLarge";
			this.radioButtonFilterCutLarge.Size = new System.Drawing.Size(71, 17);
			this.radioButtonFilterCutLarge.TabIndex = 0;
			this.radioButtonFilterCutLarge.Text = "Cut Large";
			this.radioButtonFilterCutLarge.UseVisualStyleBackColor = true;
			this.radioButtonFilterCutLarge.CheckedChanged += new System.EventHandler(this.radioButtonFilterNone_CheckedChanged);
			// 
			// radioButtonFilterNone
			// 
			this.radioButtonFilterNone.AutoSize = true;
			this.radioButtonFilterNone.Checked = true;
			this.radioButtonFilterNone.Location = new System.Drawing.Point(3, 10);
			this.radioButtonFilterNone.Name = "radioButtonFilterNone";
			this.radioButtonFilterNone.Size = new System.Drawing.Size(51, 17);
			this.radioButtonFilterNone.TabIndex = 0;
			this.radioButtonFilterNone.TabStop = true;
			this.radioButtonFilterNone.Text = "None";
			this.radioButtonFilterNone.UseVisualStyleBackColor = true;
			this.radioButtonFilterNone.CheckedChanged += new System.EventHandler(this.radioButtonFilterNone_CheckedChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 561);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(29, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Filter";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.radioButtonRandom);
			this.panel1.Controls.Add(this.radioButtonSinc);
			this.panel1.Controls.Add(this.radioButtonTriangle);
			this.panel1.Controls.Add(this.radioButtonSine);
			this.panel1.Controls.Add(this.radioButtonSquare);
			this.panel1.Location = new System.Drawing.Point(48, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(359, 34);
			this.panel1.TabIndex = 2;
			// 
			// radioButtonRandom
			// 
			this.radioButtonRandom.AutoSize = true;
			this.radioButtonRandom.Location = new System.Drawing.Point(255, 10);
			this.radioButtonRandom.Name = "radioButtonRandom";
			this.radioButtonRandom.Size = new System.Drawing.Size(65, 17);
			this.radioButtonRandom.TabIndex = 0;
			this.radioButtonRandom.Text = "Random";
			this.radioButtonRandom.UseVisualStyleBackColor = true;
			this.radioButtonRandom.CheckedChanged += new System.EventHandler(this.radioButtonSquare_CheckedChanged);
			// 
			// radioButtonSinc
			// 
			this.radioButtonSinc.AutoSize = true;
			this.radioButtonSinc.Location = new System.Drawing.Point(203, 10);
			this.radioButtonSinc.Name = "radioButtonSinc";
			this.radioButtonSinc.Size = new System.Drawing.Size(46, 17);
			this.radioButtonSinc.TabIndex = 0;
			this.radioButtonSinc.Text = "Sinc";
			this.radioButtonSinc.UseVisualStyleBackColor = true;
			this.radioButtonSinc.CheckedChanged += new System.EventHandler(this.radioButtonSquare_CheckedChanged);
			// 
			// radioButtonTriangle
			// 
			this.radioButtonTriangle.AutoSize = true;
			this.radioButtonTriangle.Location = new System.Drawing.Point(125, 10);
			this.radioButtonTriangle.Name = "radioButtonTriangle";
			this.radioButtonTriangle.Size = new System.Drawing.Size(72, 17);
			this.radioButtonTriangle.TabIndex = 0;
			this.radioButtonTriangle.Text = "Triangular";
			this.radioButtonTriangle.UseVisualStyleBackColor = true;
			this.radioButtonTriangle.CheckedChanged += new System.EventHandler(this.radioButtonSquare_CheckedChanged);
			// 
			// radioButtonSine
			// 
			this.radioButtonSine.AutoSize = true;
			this.radioButtonSine.Location = new System.Drawing.Point(68, 10);
			this.radioButtonSine.Name = "radioButtonSine";
			this.radioButtonSine.Size = new System.Drawing.Size(51, 17);
			this.radioButtonSine.TabIndex = 0;
			this.radioButtonSine.Text = "Sinus";
			this.radioButtonSine.UseVisualStyleBackColor = true;
			this.radioButtonSine.CheckedChanged += new System.EventHandler(this.radioButtonSquare_CheckedChanged);
			// 
			// radioButtonSquare
			// 
			this.radioButtonSquare.AutoSize = true;
			this.radioButtonSquare.Checked = true;
			this.radioButtonSquare.Location = new System.Drawing.Point(3, 10);
			this.radioButtonSquare.Name = "radioButtonSquare";
			this.radioButtonSquare.Size = new System.Drawing.Size(59, 17);
			this.radioButtonSquare.TabIndex = 0;
			this.radioButtonSquare.TabStop = true;
			this.radioButtonSquare.Text = "Square";
			this.radioButtonSquare.UseVisualStyleBackColor = true;
			this.radioButtonSquare.CheckedChanged += new System.EventHandler(this.radioButtonSquare_CheckedChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(36, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Signal";
			// 
			// imagePanel
			// 
			this.imagePanel.Bitmap = null;
			this.imagePanel.Location = new System.Drawing.Point(6, 43);
			this.imagePanel.MessageOnEmpty = null;
			this.imagePanel.Name = "imagePanel";
			this.imagePanel.Size = new System.Drawing.Size(1000, 500);
			this.imagePanel.SkipPaint = false;
			this.imagePanel.TabIndex = 0;
			// 
			// tabPage2D
			// 
			this.tabPage2D.Controls.Add(this.label4);
			this.tabPage2D.Controls.Add(this.floatTrackbarControlScaleV);
			this.tabPage2D.Controls.Add(this.floatTrackbarControlScaleU);
			this.tabPage2D.Controls.Add(this.panel3);
			this.tabPage2D.Controls.Add(this.label3);
			this.tabPage2D.Controls.Add(this.imagePanel2D);
			this.tabPage2D.Location = new System.Drawing.Point(4, 22);
			this.tabPage2D.Name = "tabPage2D";
			this.tabPage2D.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2D.Size = new System.Drawing.Size(1262, 707);
			this.tabPage2D.TabIndex = 1;
			this.tabPage2D.Text = "FFT 2D";
			this.tabPage2D.UseVisualStyleBackColor = true;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(7, 663);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(52, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "Scale UV";
			// 
			// floatTrackbarControlScaleV
			// 
			this.floatTrackbarControlScaleV.Location = new System.Drawing.Point(271, 659);
			this.floatTrackbarControlScaleV.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlScaleV.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlScaleV.Name = "floatTrackbarControlScaleV";
			this.floatTrackbarControlScaleV.RangeMin = 0F;
			this.floatTrackbarControlScaleV.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlScaleV.TabIndex = 5;
			this.floatTrackbarControlScaleV.Value = 0F;
			this.floatTrackbarControlScaleV.VisibleRangeMax = 8F;
			// 
			// floatTrackbarControlScaleU
			// 
			this.floatTrackbarControlScaleU.Location = new System.Drawing.Point(65, 659);
			this.floatTrackbarControlScaleU.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlScaleU.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlScaleU.Name = "floatTrackbarControlScaleU";
			this.floatTrackbarControlScaleU.RangeMin = 0F;
			this.floatTrackbarControlScaleU.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlScaleU.TabIndex = 5;
			this.floatTrackbarControlScaleU.Value = 1F;
			this.floatTrackbarControlScaleU.VisibleRangeMax = 8F;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.radioButton1);
			this.panel3.Controls.Add(this.radioButton2);
			this.panel3.Controls.Add(this.radioButton3);
			this.panel3.Controls.Add(this.radioButton4);
			this.panel3.Controls.Add(this.radioButtonSquare2D);
			this.panel3.Location = new System.Drawing.Point(48, 3);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(359, 34);
			this.panel3.TabIndex = 4;
			// 
			// radioButton1
			// 
			this.radioButton1.AutoSize = true;
			this.radioButton1.Location = new System.Drawing.Point(255, 10);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(65, 17);
			this.radioButton1.TabIndex = 0;
			this.radioButton1.Text = "Random";
			this.radioButton1.UseVisualStyleBackColor = true;
			// 
			// radioButton2
			// 
			this.radioButton2.AutoSize = true;
			this.radioButton2.Location = new System.Drawing.Point(203, 10);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = new System.Drawing.Size(46, 17);
			this.radioButton2.TabIndex = 0;
			this.radioButton2.Text = "Sinc";
			this.radioButton2.UseVisualStyleBackColor = true;
			// 
			// radioButton3
			// 
			this.radioButton3.AutoSize = true;
			this.radioButton3.Location = new System.Drawing.Point(125, 10);
			this.radioButton3.Name = "radioButton3";
			this.radioButton3.Size = new System.Drawing.Size(72, 17);
			this.radioButton3.TabIndex = 0;
			this.radioButton3.Text = "Triangular";
			this.radioButton3.UseVisualStyleBackColor = true;
			// 
			// radioButton4
			// 
			this.radioButton4.AutoSize = true;
			this.radioButton4.Location = new System.Drawing.Point(68, 10);
			this.radioButton4.Name = "radioButton4";
			this.radioButton4.Size = new System.Drawing.Size(51, 17);
			this.radioButton4.TabIndex = 0;
			this.radioButton4.Text = "Sinus";
			this.radioButton4.UseVisualStyleBackColor = true;
			// 
			// radioButtonSquare2D
			// 
			this.radioButtonSquare2D.AutoSize = true;
			this.radioButtonSquare2D.Checked = true;
			this.radioButtonSquare2D.Location = new System.Drawing.Point(3, 10);
			this.radioButtonSquare2D.Name = "radioButtonSquare2D";
			this.radioButtonSquare2D.Size = new System.Drawing.Size(59, 17);
			this.radioButtonSquare2D.TabIndex = 0;
			this.radioButtonSquare2D.TabStop = true;
			this.radioButtonSquare2D.Text = "Square";
			this.radioButtonSquare2D.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 15);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(36, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Signal";
			// 
			// imagePanel2D
			// 
			this.imagePanel2D.Bitmap = null;
			this.imagePanel2D.Location = new System.Drawing.Point(6, 45);
			this.imagePanel2D.MessageOnEmpty = null;
			this.imagePanel2D.Name = "imagePanel2D";
			this.imagePanel2D.Size = new System.Drawing.Size(1200, 600);
			this.imagePanel2D.SkipPaint = false;
			this.imagePanel2D.TabIndex = 1;
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(1205, 4);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 2;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// FourierTestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1294, 777);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.tabControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FourierTestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Fourier Transform Test";
			this.tabControl1.ResumeLayout(false);
			this.tabPage1D.ResumeLayout(false);
			this.tabPage1D.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.tabPage2D.ResumeLayout(false);
			this.tabPage2D.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ImagePanel imagePanel;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1D;
		private System.Windows.Forms.TabPage tabPage2D;
		private ImagePanel imagePanel2D;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.RadioButton radioButtonSquare;
		private System.Windows.Forms.RadioButton radioButtonTriangle;
		private System.Windows.Forms.RadioButton radioButtonSine;
		private System.Windows.Forms.RadioButton radioButtonSinc;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.RadioButton radioButtonFilterCutShort;
		private System.Windows.Forms.RadioButton radioButtonFilterCutMedium;
		private System.Windows.Forms.RadioButton radioButtonFilterCutLarge;
		private System.Windows.Forms.RadioButton radioButtonFilterNone;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RadioButton radioButtonFilterGaussian;
		private System.Windows.Forms.RadioButton radioButtonFilterExponential;
		private System.Windows.Forms.RadioButton radioButtonFilterInverse;
		private System.Windows.Forms.CheckBox checkBoxInvertFilter;
		private System.Windows.Forms.CheckBox checkBoxShowReconstructedSignal;
		private System.Windows.Forms.RadioButton radioButtonRandom;
		private System.Windows.Forms.CheckBox checkBoxShowInput;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Label labelDiff;
		private System.Windows.Forms.CheckBox checkBoxGPU;
		private System.Windows.Forms.CheckBox checkBoxUseFFTW;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.RadioButton radioButton3;
		private System.Windows.Forms.RadioButton radioButton4;
		private System.Windows.Forms.RadioButton radioButtonSquare2D;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlScaleU;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlScaleV;
	}
}

