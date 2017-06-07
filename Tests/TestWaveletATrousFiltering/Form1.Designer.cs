namespace TestWaveletATrousFiltering
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
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
			this.buttonReload = new System.Windows.Forms.Button();
			this.panelOutput = new TestWaveletATrousFiltering.PanelOutput(this.components);
			this.floatTrackbarControlLightSize = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlSigmaColor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlSigmaNormal = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlSigmaPosition = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.checkBoxToggleFilter = new System.Windows.Forms.CheckBox();
			this.integerTrackbarControlFilterLevel = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.SuspendLayout();
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 10;
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(1229, 802);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 1;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1280, 720);
			this.panelOutput.TabIndex = 0;
			// 
			// floatTrackbarControlLightSize
			// 
			this.floatTrackbarControlLightSize.Location = new System.Drawing.Point(844, 802);
			this.floatTrackbarControlLightSize.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightSize.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightSize.Name = "floatTrackbarControlLightSize";
			this.floatTrackbarControlLightSize.RangeMax = 10F;
			this.floatTrackbarControlLightSize.RangeMin = 0F;
			this.floatTrackbarControlLightSize.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightSize.TabIndex = 2;
			this.floatTrackbarControlLightSize.Value = 1.05F;
			this.floatTrackbarControlLightSize.VisibleRangeMax = 4F;
			// 
			// floatTrackbarControlSigmaColor
			// 
			this.floatTrackbarControlSigmaColor.Location = new System.Drawing.Point(88, 745);
			this.floatTrackbarControlSigmaColor.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSigmaColor.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSigmaColor.Name = "floatTrackbarControlSigmaColor";
			this.floatTrackbarControlSigmaColor.RangeMax = 1000F;
			this.floatTrackbarControlSigmaColor.RangeMin = 0F;
			this.floatTrackbarControlSigmaColor.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlSigmaColor.TabIndex = 2;
			this.floatTrackbarControlSigmaColor.Value = 1F;
			this.floatTrackbarControlSigmaColor.VisibleRangeMax = 1F;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 747);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(63, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Sigma Color";
			// 
			// floatTrackbarControlSigmaNormal
			// 
			this.floatTrackbarControlSigmaNormal.Location = new System.Drawing.Point(88, 771);
			this.floatTrackbarControlSigmaNormal.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSigmaNormal.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSigmaNormal.Name = "floatTrackbarControlSigmaNormal";
			this.floatTrackbarControlSigmaNormal.RangeMax = 1000F;
			this.floatTrackbarControlSigmaNormal.RangeMin = 0F;
			this.floatTrackbarControlSigmaNormal.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlSigmaNormal.TabIndex = 2;
			this.floatTrackbarControlSigmaNormal.Value = 0.01F;
			this.floatTrackbarControlSigmaNormal.VisibleRangeMax = 1F;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 773);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Sigma Normal";
			// 
			// floatTrackbarControlSigmaPosition
			// 
			this.floatTrackbarControlSigmaPosition.Location = new System.Drawing.Point(88, 797);
			this.floatTrackbarControlSigmaPosition.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSigmaPosition.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSigmaPosition.Name = "floatTrackbarControlSigmaPosition";
			this.floatTrackbarControlSigmaPosition.RangeMax = 1000F;
			this.floatTrackbarControlSigmaPosition.RangeMin = 0F;
			this.floatTrackbarControlSigmaPosition.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlSigmaPosition.TabIndex = 2;
			this.floatTrackbarControlSigmaPosition.Value = 0.3F;
			this.floatTrackbarControlSigmaPosition.VisibleRangeMax = 1F;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 799);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(76, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Sigma Position";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(381, 747);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(58, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Filter Level";
			// 
			// checkBoxToggleFilter
			// 
			this.checkBoxToggleFilter.AutoSize = true;
			this.checkBoxToggleFilter.Location = new System.Drawing.Point(384, 772);
			this.checkBoxToggleFilter.Name = "checkBoxToggleFilter";
			this.checkBoxToggleFilter.Size = new System.Drawing.Size(90, 17);
			this.checkBoxToggleFilter.TabIndex = 4;
			this.checkBoxToggleFilter.Text = "Show Filtered";
			this.checkBoxToggleFilter.UseVisualStyleBackColor = true;
			this.checkBoxToggleFilter.Checked = true;
			// 
			// integerTrackbarControlFilterLevel
			// 
			this.integerTrackbarControlFilterLevel.Location = new System.Drawing.Point(445, 745);
			this.integerTrackbarControlFilterLevel.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlFilterLevel.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlFilterLevel.Name = "integerTrackbarControlFilterLevel";
			this.integerTrackbarControlFilterLevel.RangeMax = 5;
			this.integerTrackbarControlFilterLevel.RangeMin = 0;
			this.integerTrackbarControlFilterLevel.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlFilterLevel.TabIndex = 5;
			this.integerTrackbarControlFilterLevel.Value = 5;
			this.integerTrackbarControlFilterLevel.VisibleRangeMax = 5;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1316, 837);
			this.Controls.Add(this.integerTrackbarControlFilterLevel);
			this.Controls.Add(this.checkBoxToggleFilter);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlSigmaPosition);
			this.Controls.Add(this.floatTrackbarControlSigmaNormal);
			this.Controls.Add(this.floatTrackbarControlSigmaColor);
			this.Controls.Add(this.floatTrackbarControlLightSize);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "A-Trous Wavelet Filtering Test";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelOutput;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button buttonReload;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightSize;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSigmaColor;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSigmaNormal;
		private System.Windows.Forms.Label label2;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSigmaPosition;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox checkBoxToggleFilter;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlFilterLevel;
	}
}

