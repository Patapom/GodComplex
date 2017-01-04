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
			this.tabPage2D = new System.Windows.Forms.TabPage();
			this.panel5 = new System.Windows.Forms.Panel();
			this.radioButtonShowSignalDiff = new System.Windows.Forms.RadioButton();
			this.radioButtonShowReconstructedSignal = new System.Windows.Forms.RadioButton();
			this.radioButtonShowInitialSignal = new System.Windows.Forms.RadioButton();
			this.checkBoxShowSignalMagnitude = new System.Windows.Forms.CheckBox();
			this.labelDiff2D = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.floatTrackbarControlScaleV = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlScaleU = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.panelSignalY = new System.Windows.Forms.Panel();
			this.radioButtonConstantY = new System.Windows.Forms.RadioButton();
			this.radioButtonRandomY = new System.Windows.Forms.RadioButton();
			this.radioButtonSincY = new System.Windows.Forms.RadioButton();
			this.radioButtonTriY = new System.Windows.Forms.RadioButton();
			this.radioButtonSinusY = new System.Windows.Forms.RadioButton();
			this.radioButtonSquareY = new System.Windows.Forms.RadioButton();
			this.label5 = new System.Windows.Forms.Label();
			this.panelSignalX = new System.Windows.Forms.Panel();
			this.radioButtonConstantX = new System.Windows.Forms.RadioButton();
			this.radioButtonRandomX = new System.Windows.Forms.RadioButton();
			this.radioButtonSincX = new System.Windows.Forms.RadioButton();
			this.radioButtonTriX = new System.Windows.Forms.RadioButton();
			this.radioButtonSinusX = new System.Windows.Forms.RadioButton();
			this.radioButtonSquareX = new System.Windows.Forms.RadioButton();
			this.label3 = new System.Windows.Forms.Label();
			this.buttonReload = new System.Windows.Forms.Button();
			this.panel6 = new System.Windows.Forms.Panel();
			this.radioButtonFilterInverse2D = new System.Windows.Forms.RadioButton();
			this.radioButtonFilterGaussian2D = new System.Windows.Forms.RadioButton();
			this.radioButtonFilterExp2D = new System.Windows.Forms.RadioButton();
			this.radioButtonFilterCutShort2D = new System.Windows.Forms.RadioButton();
			this.radioButtonFilterCutMedium2D = new System.Windows.Forms.RadioButton();
			this.radioButtonFilterCutLarge2D = new System.Windows.Forms.RadioButton();
			this.radioButtonFilterNone2D = new System.Windows.Forms.RadioButton();
			this.label6 = new System.Windows.Forms.Label();
			this.floatTrackbarControlTimeScale = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label7 = new System.Windows.Forms.Label();
			this.imagePanel = new TestFourier.ImagePanel();
			this.imagePanel2D = new TestFourier.ImagePanel();
			this.checkBoxRadial = new System.Windows.Forms.CheckBox();
			this.tabControl1.SuspendLayout();
			this.tabPage1D.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.tabPage2D.SuspendLayout();
			this.panel5.SuspendLayout();
			this.panelSignalY.SuspendLayout();
			this.panelSignalX.SuspendLayout();
			this.panel6.SuspendLayout();
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
			// tabPage2D
			// 
			this.tabPage2D.Controls.Add(this.checkBoxRadial);
			this.tabPage2D.Controls.Add(this.panel6);
			this.tabPage2D.Controls.Add(this.label6);
			this.tabPage2D.Controls.Add(this.panel5);
			this.tabPage2D.Controls.Add(this.checkBoxShowSignalMagnitude);
			this.tabPage2D.Controls.Add(this.labelDiff2D);
			this.tabPage2D.Controls.Add(this.label7);
			this.tabPage2D.Controls.Add(this.label4);
			this.tabPage2D.Controls.Add(this.floatTrackbarControlScaleV);
			this.tabPage2D.Controls.Add(this.floatTrackbarControlTimeScale);
			this.tabPage2D.Controls.Add(this.floatTrackbarControlScaleU);
			this.tabPage2D.Controls.Add(this.panelSignalY);
			this.tabPage2D.Controls.Add(this.label5);
			this.tabPage2D.Controls.Add(this.panelSignalX);
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
			// panel5
			// 
			this.panel5.Controls.Add(this.radioButtonShowSignalDiff);
			this.panel5.Controls.Add(this.radioButtonShowReconstructedSignal);
			this.panel5.Controls.Add(this.radioButtonShowInitialSignal);
			this.panel5.Location = new System.Drawing.Point(636, 3);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(570, 34);
			this.panel5.TabIndex = 9;
			// 
			// radioButtonShowSignalDiff
			// 
			this.radioButtonShowSignalDiff.AutoSize = true;
			this.radioButtonShowSignalDiff.Location = new System.Drawing.Point(284, 10);
			this.radioButtonShowSignalDiff.Name = "radioButtonShowSignalDiff";
			this.radioButtonShowSignalDiff.Size = new System.Drawing.Size(162, 17);
			this.radioButtonShowSignalDiff.TabIndex = 2;
			this.radioButtonShowSignalDiff.TabStop = true;
			this.radioButtonShowSignalDiff.Text = "Show Signal Difference (x100)";
			this.radioButtonShowSignalDiff.UseVisualStyleBackColor = true;
			// 
			// radioButtonShowReconstructedSignal
			// 
			this.radioButtonShowReconstructedSignal.AutoSize = true;
			this.radioButtonShowReconstructedSignal.Checked = true;
			this.radioButtonShowReconstructedSignal.Location = new System.Drawing.Point(121, 10);
			this.radioButtonShowReconstructedSignal.Name = "radioButtonShowReconstructedSignal";
			this.radioButtonShowReconstructedSignal.Size = new System.Drawing.Size(157, 17);
			this.radioButtonShowReconstructedSignal.TabIndex = 1;
			this.radioButtonShowReconstructedSignal.TabStop = true;
			this.radioButtonShowReconstructedSignal.Text = "Show Reconstructed Signal";
			this.radioButtonShowReconstructedSignal.UseVisualStyleBackColor = true;
			// 
			// radioButtonShowInitialSignal
			// 
			this.radioButtonShowInitialSignal.AutoSize = true;
			this.radioButtonShowInitialSignal.Location = new System.Drawing.Point(4, 10);
			this.radioButtonShowInitialSignal.Name = "radioButtonShowInitialSignal";
			this.radioButtonShowInitialSignal.Size = new System.Drawing.Size(111, 17);
			this.radioButtonShowInitialSignal.TabIndex = 0;
			this.radioButtonShowInitialSignal.TabStop = true;
			this.radioButtonShowInitialSignal.Text = "Show Initial Signal";
			this.radioButtonShowInitialSignal.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowSignalMagnitude
			// 
			this.checkBoxShowSignalMagnitude.AutoSize = true;
			this.checkBoxShowSignalMagnitude.Location = new System.Drawing.Point(640, 46);
			this.checkBoxShowSignalMagnitude.Name = "checkBoxShowSignalMagnitude";
			this.checkBoxShowSignalMagnitude.Size = new System.Drawing.Size(106, 17);
			this.checkBoxShowSignalMagnitude.TabIndex = 8;
			this.checkBoxShowSignalMagnitude.Text = "Show Magnitude";
			this.checkBoxShowSignalMagnitude.UseVisualStyleBackColor = true;
			// 
			// labelDiff2D
			// 
			this.labelDiff2D.AutoSize = true;
			this.labelDiff2D.Location = new System.Drawing.Point(821, 47);
			this.labelDiff2D.Name = "labelDiff2D";
			this.labelDiff2D.Size = new System.Drawing.Size(46, 13);
			this.labelDiff2D.TabIndex = 7;
			this.labelDiff2D.Text = "BISOU !";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(7, 685);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(52, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "Scale UV";
			// 
			// floatTrackbarControlScaleV
			// 
			this.floatTrackbarControlScaleV.Location = new System.Drawing.Point(210, 681);
			this.floatTrackbarControlScaleV.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlScaleV.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlScaleV.Name = "floatTrackbarControlScaleV";
			this.floatTrackbarControlScaleV.RangeMin = 0F;
			this.floatTrackbarControlScaleV.Size = new System.Drawing.Size(140, 20);
			this.floatTrackbarControlScaleV.TabIndex = 5;
			this.floatTrackbarControlScaleV.Value = 6F;
			this.floatTrackbarControlScaleV.VisibleRangeMax = 32F;
			// 
			// floatTrackbarControlScaleU
			// 
			this.floatTrackbarControlScaleU.Location = new System.Drawing.Point(65, 681);
			this.floatTrackbarControlScaleU.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlScaleU.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlScaleU.Name = "floatTrackbarControlScaleU";
			this.floatTrackbarControlScaleU.RangeMin = 0F;
			this.floatTrackbarControlScaleU.Size = new System.Drawing.Size(140, 20);
			this.floatTrackbarControlScaleU.TabIndex = 5;
			this.floatTrackbarControlScaleU.Value = 3F;
			this.floatTrackbarControlScaleU.VisibleRangeMax = 32F;
			// 
			// panelSignalY
			// 
			this.panelSignalY.Controls.Add(this.radioButtonConstantY);
			this.panelSignalY.Controls.Add(this.radioButtonRandomY);
			this.panelSignalY.Controls.Add(this.radioButtonSincY);
			this.panelSignalY.Controls.Add(this.radioButtonTriY);
			this.panelSignalY.Controls.Add(this.radioButtonSinusY);
			this.panelSignalY.Controls.Add(this.radioButtonSquareY);
			this.panelSignalY.Location = new System.Drawing.Point(65, 35);
			this.panelSignalY.Name = "panelSignalY";
			this.panelSignalY.Size = new System.Drawing.Size(404, 34);
			this.panelSignalY.TabIndex = 4;
			// 
			// radioButtonConstantY
			// 
			this.radioButtonConstantY.AutoSize = true;
			this.radioButtonConstantY.Location = new System.Drawing.Point(3, 10);
			this.radioButtonConstantY.Name = "radioButtonConstantY";
			this.radioButtonConstantY.Size = new System.Drawing.Size(67, 17);
			this.radioButtonConstantY.TabIndex = 0;
			this.radioButtonConstantY.Text = "Constant";
			this.radioButtonConstantY.UseVisualStyleBackColor = true;
			this.radioButtonConstantY.CheckedChanged += new System.EventHandler(this.radioButtonSignalY_CheckedChanged);
			// 
			// radioButtonRandomY
			// 
			this.radioButtonRandomY.AutoSize = true;
			this.radioButtonRandomY.Location = new System.Drawing.Point(332, 10);
			this.radioButtonRandomY.Name = "radioButtonRandomY";
			this.radioButtonRandomY.Size = new System.Drawing.Size(65, 17);
			this.radioButtonRandomY.TabIndex = 0;
			this.radioButtonRandomY.Text = "Random";
			this.radioButtonRandomY.UseVisualStyleBackColor = true;
			this.radioButtonRandomY.CheckedChanged += new System.EventHandler(this.radioButtonSignalY_CheckedChanged);
			// 
			// radioButtonSincY
			// 
			this.radioButtonSincY.AutoSize = true;
			this.radioButtonSincY.Checked = true;
			this.radioButtonSincY.Location = new System.Drawing.Point(280, 10);
			this.radioButtonSincY.Name = "radioButtonSincY";
			this.radioButtonSincY.Size = new System.Drawing.Size(46, 17);
			this.radioButtonSincY.TabIndex = 0;
			this.radioButtonSincY.TabStop = true;
			this.radioButtonSincY.Text = "Sinc";
			this.radioButtonSincY.UseVisualStyleBackColor = true;
			this.radioButtonSincY.CheckedChanged += new System.EventHandler(this.radioButtonSignalY_CheckedChanged);
			// 
			// radioButtonTriY
			// 
			this.radioButtonTriY.AutoSize = true;
			this.radioButtonTriY.Location = new System.Drawing.Point(202, 10);
			this.radioButtonTriY.Name = "radioButtonTriY";
			this.radioButtonTriY.Size = new System.Drawing.Size(72, 17);
			this.radioButtonTriY.TabIndex = 0;
			this.radioButtonTriY.Text = "Triangular";
			this.radioButtonTriY.UseVisualStyleBackColor = true;
			this.radioButtonTriY.CheckedChanged += new System.EventHandler(this.radioButtonSignalY_CheckedChanged);
			// 
			// radioButtonSinusY
			// 
			this.radioButtonSinusY.AutoSize = true;
			this.radioButtonSinusY.Location = new System.Drawing.Point(145, 10);
			this.radioButtonSinusY.Name = "radioButtonSinusY";
			this.radioButtonSinusY.Size = new System.Drawing.Size(51, 17);
			this.radioButtonSinusY.TabIndex = 0;
			this.radioButtonSinusY.Text = "Sinus";
			this.radioButtonSinusY.UseVisualStyleBackColor = true;
			this.radioButtonSinusY.CheckedChanged += new System.EventHandler(this.radioButtonSignalY_CheckedChanged);
			// 
			// radioButtonSquareY
			// 
			this.radioButtonSquareY.AutoSize = true;
			this.radioButtonSquareY.Location = new System.Drawing.Point(80, 10);
			this.radioButtonSquareY.Name = "radioButtonSquareY";
			this.radioButtonSquareY.Size = new System.Drawing.Size(59, 17);
			this.radioButtonSquareY.TabIndex = 0;
			this.radioButtonSquareY.Text = "Square";
			this.radioButtonSquareY.UseVisualStyleBackColor = true;
			this.radioButtonSquareY.CheckedChanged += new System.EventHandler(this.radioButtonSignalY_CheckedChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 47);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(46, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "Signal Y";
			// 
			// panelSignalX
			// 
			this.panelSignalX.Controls.Add(this.radioButtonConstantX);
			this.panelSignalX.Controls.Add(this.radioButtonRandomX);
			this.panelSignalX.Controls.Add(this.radioButtonSincX);
			this.panelSignalX.Controls.Add(this.radioButtonTriX);
			this.panelSignalX.Controls.Add(this.radioButtonSinusX);
			this.panelSignalX.Controls.Add(this.radioButtonSquareX);
			this.panelSignalX.Location = new System.Drawing.Point(65, 3);
			this.panelSignalX.Name = "panelSignalX";
			this.panelSignalX.Size = new System.Drawing.Size(404, 34);
			this.panelSignalX.TabIndex = 4;
			// 
			// radioButtonConstantX
			// 
			this.radioButtonConstantX.AutoSize = true;
			this.radioButtonConstantX.Location = new System.Drawing.Point(3, 10);
			this.radioButtonConstantX.Name = "radioButtonConstantX";
			this.radioButtonConstantX.Size = new System.Drawing.Size(67, 17);
			this.radioButtonConstantX.TabIndex = 0;
			this.radioButtonConstantX.Text = "Constant";
			this.radioButtonConstantX.UseVisualStyleBackColor = true;
			this.radioButtonConstantX.CheckedChanged += new System.EventHandler(this.radioButtonSignalX_CheckedChanged);
			// 
			// radioButtonRandomX
			// 
			this.radioButtonRandomX.AutoSize = true;
			this.radioButtonRandomX.Location = new System.Drawing.Point(332, 10);
			this.radioButtonRandomX.Name = "radioButtonRandomX";
			this.radioButtonRandomX.Size = new System.Drawing.Size(65, 17);
			this.radioButtonRandomX.TabIndex = 0;
			this.radioButtonRandomX.Text = "Random";
			this.radioButtonRandomX.UseVisualStyleBackColor = true;
			this.radioButtonRandomX.CheckedChanged += new System.EventHandler(this.radioButtonSignalX_CheckedChanged);
			// 
			// radioButtonSincX
			// 
			this.radioButtonSincX.AutoSize = true;
			this.radioButtonSincX.Location = new System.Drawing.Point(280, 10);
			this.radioButtonSincX.Name = "radioButtonSincX";
			this.radioButtonSincX.Size = new System.Drawing.Size(46, 17);
			this.radioButtonSincX.TabIndex = 0;
			this.radioButtonSincX.Text = "Sinc";
			this.radioButtonSincX.UseVisualStyleBackColor = true;
			this.radioButtonSincX.CheckedChanged += new System.EventHandler(this.radioButtonSignalX_CheckedChanged);
			// 
			// radioButtonTriX
			// 
			this.radioButtonTriX.AutoSize = true;
			this.radioButtonTriX.Location = new System.Drawing.Point(202, 10);
			this.radioButtonTriX.Name = "radioButtonTriX";
			this.radioButtonTriX.Size = new System.Drawing.Size(72, 17);
			this.radioButtonTriX.TabIndex = 0;
			this.radioButtonTriX.Text = "Triangular";
			this.radioButtonTriX.UseVisualStyleBackColor = true;
			this.radioButtonTriX.CheckedChanged += new System.EventHandler(this.radioButtonSignalX_CheckedChanged);
			// 
			// radioButtonSinusX
			// 
			this.radioButtonSinusX.AutoSize = true;
			this.radioButtonSinusX.Location = new System.Drawing.Point(145, 10);
			this.radioButtonSinusX.Name = "radioButtonSinusX";
			this.radioButtonSinusX.Size = new System.Drawing.Size(51, 17);
			this.radioButtonSinusX.TabIndex = 0;
			this.radioButtonSinusX.Text = "Sinus";
			this.radioButtonSinusX.UseVisualStyleBackColor = true;
			this.radioButtonSinusX.CheckedChanged += new System.EventHandler(this.radioButtonSignalX_CheckedChanged);
			// 
			// radioButtonSquareX
			// 
			this.radioButtonSquareX.AutoSize = true;
			this.radioButtonSquareX.Checked = true;
			this.radioButtonSquareX.Location = new System.Drawing.Point(80, 10);
			this.radioButtonSquareX.Name = "radioButtonSquareX";
			this.radioButtonSquareX.Size = new System.Drawing.Size(59, 17);
			this.radioButtonSquareX.TabIndex = 0;
			this.radioButtonSquareX.TabStop = true;
			this.radioButtonSquareX.Text = "Square";
			this.radioButtonSquareX.UseVisualStyleBackColor = true;
			this.radioButtonSquareX.CheckedChanged += new System.EventHandler(this.radioButtonSignalX_CheckedChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 15);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(46, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Signal X";
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
			// panel6
			// 
			this.panel6.Controls.Add(this.radioButtonFilterInverse2D);
			this.panel6.Controls.Add(this.radioButtonFilterGaussian2D);
			this.panel6.Controls.Add(this.radioButtonFilterExp2D);
			this.panel6.Controls.Add(this.radioButtonFilterCutShort2D);
			this.panel6.Controls.Add(this.radioButtonFilterCutMedium2D);
			this.panel6.Controls.Add(this.radioButtonFilterCutLarge2D);
			this.panel6.Controls.Add(this.radioButtonFilterNone2D);
			this.panel6.Location = new System.Drawing.Point(649, 675);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(557, 32);
			this.panel6.TabIndex = 11;
			// 
			// radioButtonFilterInverse2D
			// 
			this.radioButtonFilterInverse2D.AutoSize = true;
			this.radioButtonFilterInverse2D.Location = new System.Drawing.Point(460, 10);
			this.radioButtonFilterInverse2D.Name = "radioButtonFilterInverse2D";
			this.radioButtonFilterInverse2D.Size = new System.Drawing.Size(60, 17);
			this.radioButtonFilterInverse2D.TabIndex = 0;
			this.radioButtonFilterInverse2D.Text = "Inverse";
			this.radioButtonFilterInverse2D.UseVisualStyleBackColor = true;
			this.radioButtonFilterInverse2D.CheckedChanged += new System.EventHandler(this.radioButtonFilterNone2D_CheckedChanged);
			// 
			// radioButtonFilterGaussian2D
			// 
			this.radioButtonFilterGaussian2D.AutoSize = true;
			this.radioButtonFilterGaussian2D.Location = new System.Drawing.Point(385, 10);
			this.radioButtonFilterGaussian2D.Name = "radioButtonFilterGaussian2D";
			this.radioButtonFilterGaussian2D.Size = new System.Drawing.Size(69, 17);
			this.radioButtonFilterGaussian2D.TabIndex = 0;
			this.radioButtonFilterGaussian2D.Text = "Gaussian";
			this.radioButtonFilterGaussian2D.UseVisualStyleBackColor = true;
			this.radioButtonFilterGaussian2D.CheckedChanged += new System.EventHandler(this.radioButtonFilterNone2D_CheckedChanged);
			// 
			// radioButtonFilterExp2D
			// 
			this.radioButtonFilterExp2D.AutoSize = true;
			this.radioButtonFilterExp2D.Location = new System.Drawing.Point(299, 10);
			this.radioButtonFilterExp2D.Name = "radioButtonFilterExp2D";
			this.radioButtonFilterExp2D.Size = new System.Drawing.Size(80, 17);
			this.radioButtonFilterExp2D.TabIndex = 0;
			this.radioButtonFilterExp2D.Text = "Exponential";
			this.radioButtonFilterExp2D.UseVisualStyleBackColor = true;
			this.radioButtonFilterExp2D.CheckedChanged += new System.EventHandler(this.radioButtonFilterNone2D_CheckedChanged);
			// 
			// radioButtonFilterCutShort2D
			// 
			this.radioButtonFilterCutShort2D.AutoSize = true;
			this.radioButtonFilterCutShort2D.Location = new System.Drawing.Point(224, 10);
			this.radioButtonFilterCutShort2D.Name = "radioButtonFilterCutShort2D";
			this.radioButtonFilterCutShort2D.Size = new System.Drawing.Size(69, 17);
			this.radioButtonFilterCutShort2D.TabIndex = 0;
			this.radioButtonFilterCutShort2D.Text = "Cut Short";
			this.radioButtonFilterCutShort2D.UseVisualStyleBackColor = true;
			this.radioButtonFilterCutShort2D.CheckedChanged += new System.EventHandler(this.radioButtonFilterNone2D_CheckedChanged);
			// 
			// radioButtonFilterCutMedium2D
			// 
			this.radioButtonFilterCutMedium2D.AutoSize = true;
			this.radioButtonFilterCutMedium2D.Location = new System.Drawing.Point(137, 10);
			this.radioButtonFilterCutMedium2D.Name = "radioButtonFilterCutMedium2D";
			this.radioButtonFilterCutMedium2D.Size = new System.Drawing.Size(81, 17);
			this.radioButtonFilterCutMedium2D.TabIndex = 0;
			this.radioButtonFilterCutMedium2D.Text = "Cut Medium";
			this.radioButtonFilterCutMedium2D.UseVisualStyleBackColor = true;
			this.radioButtonFilterCutMedium2D.CheckedChanged += new System.EventHandler(this.radioButtonFilterNone2D_CheckedChanged);
			// 
			// radioButtonFilterCutLarge2D
			// 
			this.radioButtonFilterCutLarge2D.AutoSize = true;
			this.radioButtonFilterCutLarge2D.Location = new System.Drawing.Point(60, 10);
			this.radioButtonFilterCutLarge2D.Name = "radioButtonFilterCutLarge2D";
			this.radioButtonFilterCutLarge2D.Size = new System.Drawing.Size(71, 17);
			this.radioButtonFilterCutLarge2D.TabIndex = 0;
			this.radioButtonFilterCutLarge2D.Text = "Cut Large";
			this.radioButtonFilterCutLarge2D.UseVisualStyleBackColor = true;
			this.radioButtonFilterCutLarge2D.CheckedChanged += new System.EventHandler(this.radioButtonFilterNone2D_CheckedChanged);
			// 
			// radioButtonFilterNone2D
			// 
			this.radioButtonFilterNone2D.AutoSize = true;
			this.radioButtonFilterNone2D.Checked = true;
			this.radioButtonFilterNone2D.Location = new System.Drawing.Point(3, 10);
			this.radioButtonFilterNone2D.Name = "radioButtonFilterNone2D";
			this.radioButtonFilterNone2D.Size = new System.Drawing.Size(51, 17);
			this.radioButtonFilterNone2D.TabIndex = 0;
			this.radioButtonFilterNone2D.TabStop = true;
			this.radioButtonFilterNone2D.Text = "None";
			this.radioButtonFilterNone2D.UseVisualStyleBackColor = true;
			this.radioButtonFilterNone2D.CheckedChanged += new System.EventHandler(this.radioButtonFilterNone2D_CheckedChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(607, 687);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(29, 13);
			this.label6.TabIndex = 10;
			this.label6.Text = "Filter";
			// 
			// floatTrackbarControlTimeScale
			// 
			this.floatTrackbarControlTimeScale.Location = new System.Drawing.Point(427, 681);
			this.floatTrackbarControlTimeScale.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlTimeScale.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlTimeScale.Name = "floatTrackbarControlTimeScale";
			this.floatTrackbarControlTimeScale.RangeMin = 0F;
			this.floatTrackbarControlTimeScale.Size = new System.Drawing.Size(140, 20);
			this.floatTrackbarControlTimeScale.TabIndex = 5;
			this.floatTrackbarControlTimeScale.Value = 0.2F;
			this.floatTrackbarControlTimeScale.VisibleRangeMax = 1F;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(361, 685);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(60, 13);
			this.label7.TabIndex = 6;
			this.label7.Text = "Time Scale";
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
			// imagePanel2D
			// 
			this.imagePanel2D.Bitmap = null;
			this.imagePanel2D.Location = new System.Drawing.Point(6, 75);
			this.imagePanel2D.MessageOnEmpty = null;
			this.imagePanel2D.Name = "imagePanel2D";
			this.imagePanel2D.Size = new System.Drawing.Size(1200, 600);
			this.imagePanel2D.SkipPaint = false;
			this.imagePanel2D.TabIndex = 1;
			// 
			// checkBoxRadial
			// 
			this.checkBoxRadial.AutoSize = true;
			this.checkBoxRadial.Location = new System.Drawing.Point(476, 13);
			this.checkBoxRadial.Name = "checkBoxRadial";
			this.checkBoxRadial.Size = new System.Drawing.Size(88, 17);
			this.checkBoxRadial.TabIndex = 12;
			this.checkBoxRadial.Text = "Radial Signal";
			this.checkBoxRadial.UseVisualStyleBackColor = true;
			this.checkBoxRadial.CheckedChanged += new System.EventHandler(this.checkBoxRadial_CheckedChanged);
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
			this.panel5.ResumeLayout(false);
			this.panel5.PerformLayout();
			this.panelSignalY.ResumeLayout(false);
			this.panelSignalY.PerformLayout();
			this.panelSignalX.ResumeLayout(false);
			this.panelSignalX.PerformLayout();
			this.panel6.ResumeLayout(false);
			this.panel6.PerformLayout();
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
		private System.Windows.Forms.Panel panelSignalX;
		private System.Windows.Forms.RadioButton radioButtonRandomX;
		private System.Windows.Forms.RadioButton radioButtonSincX;
		private System.Windows.Forms.RadioButton radioButtonTriX;
		private System.Windows.Forms.RadioButton radioButtonSinusX;
		private System.Windows.Forms.RadioButton radioButtonSquareX;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlScaleU;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlScaleV;
		private System.Windows.Forms.Label labelDiff2D;
		private System.Windows.Forms.CheckBox checkBoxShowSignalMagnitude;
		private System.Windows.Forms.Panel panelSignalY;
		private System.Windows.Forms.RadioButton radioButtonRandomY;
		private System.Windows.Forms.RadioButton radioButtonSincY;
		private System.Windows.Forms.RadioButton radioButtonTriY;
		private System.Windows.Forms.RadioButton radioButtonSinusY;
		private System.Windows.Forms.RadioButton radioButtonSquareY;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.RadioButton radioButtonConstantY;
		private System.Windows.Forms.RadioButton radioButtonConstantX;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.RadioButton radioButtonShowSignalDiff;
		private System.Windows.Forms.RadioButton radioButtonShowReconstructedSignal;
		private System.Windows.Forms.RadioButton radioButtonShowInitialSignal;
		private System.Windows.Forms.Panel panel6;
		private System.Windows.Forms.RadioButton radioButtonFilterInverse2D;
		private System.Windows.Forms.RadioButton radioButtonFilterGaussian2D;
		private System.Windows.Forms.RadioButton radioButtonFilterExp2D;
		private System.Windows.Forms.RadioButton radioButtonFilterCutShort2D;
		private System.Windows.Forms.RadioButton radioButtonFilterCutMedium2D;
		private System.Windows.Forms.RadioButton radioButtonFilterCutLarge2D;
		private System.Windows.Forms.RadioButton radioButtonFilterNone2D;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlTimeScale;
		private System.Windows.Forms.CheckBox checkBoxRadial;
	}
}

