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
			this.viewportPanel = new TestFourier.ViewportPanel();
			this.checkBoxShowInput = new System.Windows.Forms.CheckBox();
			this.tabControl1.SuspendLayout();
			this.tabPage1D.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.tabPage2D.SuspendLayout();
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
			this.imagePanel.TabIndex = 0;
			// 
			// tabPage2D
			// 
			this.tabPage2D.Controls.Add(this.viewportPanel);
			this.tabPage2D.Location = new System.Drawing.Point(4, 22);
			this.tabPage2D.Name = "tabPage2D";
			this.tabPage2D.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2D.Size = new System.Drawing.Size(1262, 707);
			this.tabPage2D.TabIndex = 1;
			this.tabPage2D.Text = "FFT 2D";
			this.tabPage2D.UseVisualStyleBackColor = true;
			// 
			// imagePanel2D
			// 
			this.viewportPanel.Location = new System.Drawing.Point(6, 45);
			this.viewportPanel.Name = "viewportPanel";
			this.viewportPanel.Size = new System.Drawing.Size(1000, 500);
			this.viewportPanel.TabIndex = 1;
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
			// FourierTestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1294, 777);
			this.Controls.Add(this.tabControl1);
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
			this.ResumeLayout(false);

		}

		#endregion

		private ImagePanel imagePanel;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1D;
		private System.Windows.Forms.TabPage tabPage2D;
		private ViewportPanel viewportPanel;
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
	}
}

