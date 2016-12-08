namespace TestSHIrradiance
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
			this.floatTrackbarControlThetaMax = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxResults = new System.Windows.Forms.TextBox();
			this.graphPanel = new TestSHIrradiance.GraphPanel(this.components);
			this.radioButtonCoeffs = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.radioButtonSideBySide = new System.Windows.Forms.RadioButton();
			this.radioButtonSphereOFF = new System.Windows.Forms.RadioButton();
			this.radioButtonSphereON = new System.Windows.Forms.RadioButton();
			this.radioButtonSimpleScene = new System.Windows.Forms.RadioButton();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// floatTrackbarControlThetaMax
			// 
			this.floatTrackbarControlThetaMax.Location = new System.Drawing.Point(103, 608);
			this.floatTrackbarControlThetaMax.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlThetaMax.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlThetaMax.Name = "floatTrackbarControlThetaMax";
			this.floatTrackbarControlThetaMax.RangeMax = 90F;
			this.floatTrackbarControlThetaMax.RangeMin = 0F;
			this.floatTrackbarControlThetaMax.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlThetaMax.TabIndex = 0;
			this.floatTrackbarControlThetaMax.Value = 90F;
			this.floatTrackbarControlThetaMax.VisibleRangeMax = 90F;
			this.floatTrackbarControlThetaMax.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlThetaMax_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 612);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(85, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Cone Max Angle";
			// 
			// textBoxResults
			// 
			this.textBoxResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxResults.Location = new System.Drawing.Point(818, 51);
			this.textBoxResults.Multiline = true;
			this.textBoxResults.Name = "textBoxResults";
			this.textBoxResults.ReadOnly = true;
			this.textBoxResults.Size = new System.Drawing.Size(283, 550);
			this.textBoxResults.TabIndex = 3;
			// 
			// graphPanel
			// 
			this.graphPanel.Bitmap = null;
			this.graphPanel.Location = new System.Drawing.Point(12, 51);
			this.graphPanel.MessageOnEmpty = "Bisou";
			this.graphPanel.Name = "graphPanel";
			this.graphPanel.Size = new System.Drawing.Size(800, 550);
			this.graphPanel.TabIndex = 1;
			// 
			// radioButtonCoeffs
			// 
			this.radioButtonCoeffs.AutoSize = true;
			this.radioButtonCoeffs.Checked = true;
			this.radioButtonCoeffs.Location = new System.Drawing.Point(113, 11);
			this.radioButtonCoeffs.Name = "radioButtonCoeffs";
			this.radioButtonCoeffs.Size = new System.Drawing.Size(112, 17);
			this.radioButtonCoeffs.TabIndex = 4;
			this.radioButtonCoeffs.TabStop = true;
			this.radioButtonCoeffs.Text = "Coefficients Graph";
			this.radioButtonCoeffs.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 13);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(95, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Visualisation Mode";
			// 
			// radioButtonSideBySide
			// 
			this.radioButtonSideBySide.AutoSize = true;
			this.radioButtonSideBySide.Location = new System.Drawing.Point(231, 11);
			this.radioButtonSideBySide.Name = "radioButtonSideBySide";
			this.radioButtonSideBySide.Size = new System.Drawing.Size(121, 17);
			this.radioButtonSideBySide.TabIndex = 4;
			this.radioButtonSideBySide.TabStop = true;
			this.radioButtonSideBySide.Text = "Side by Side Sphere";
			this.radioButtonSideBySide.UseVisualStyleBackColor = true;
			// 
			// radioButtonSphereOFF
			// 
			this.radioButtonSphereOFF.AutoSize = true;
			this.radioButtonSphereOFF.Location = new System.Drawing.Point(358, 11);
			this.radioButtonSphereOFF.Name = "radioButtonSphereOFF";
			this.radioButtonSphereOFF.Size = new System.Drawing.Size(100, 17);
			this.radioButtonSphereOFF.TabIndex = 4;
			this.radioButtonSphereOFF.TabStop = true;
			this.radioButtonSphereOFF.Text = "Sphere AO OFF";
			this.radioButtonSphereOFF.UseVisualStyleBackColor = true;
			// 
			// radioButtonSphereON
			// 
			this.radioButtonSphereON.AutoSize = true;
			this.radioButtonSphereON.Location = new System.Drawing.Point(464, 11);
			this.radioButtonSphereON.Name = "radioButtonSphereON";
			this.radioButtonSphereON.Size = new System.Drawing.Size(96, 17);
			this.radioButtonSphereON.TabIndex = 4;
			this.radioButtonSphereON.TabStop = true;
			this.radioButtonSphereON.Text = "Sphere AO ON";
			this.radioButtonSphereON.UseVisualStyleBackColor = true;
			// 
			// radioButtonSimpleScene
			// 
			this.radioButtonSimpleScene.AutoSize = true;
			this.radioButtonSimpleScene.Location = new System.Drawing.Point(566, 11);
			this.radioButtonSimpleScene.Name = "radioButtonSimpleScene";
			this.radioButtonSimpleScene.Size = new System.Drawing.Size(90, 17);
			this.radioButtonSimpleScene.TabIndex = 4;
			this.radioButtonSimpleScene.TabStop = true;
			this.radioButtonSimpleScene.Text = "Simple Scene";
			this.radioButtonSimpleScene.UseVisualStyleBackColor = true;
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 10;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1113, 687);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.radioButtonSimpleScene);
			this.Controls.Add(this.radioButtonSphereON);
			this.Controls.Add(this.radioButtonSphereOFF);
			this.Controls.Add(this.radioButtonSideBySide);
			this.Controls.Add(this.radioButtonCoeffs);
			this.Controls.Add(this.textBoxResults);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.graphPanel);
			this.Controls.Add(this.floatTrackbarControlThetaMax);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Test SH Irradiance Estimate Coefficients with AO factor";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlThetaMax;
		private GraphPanel graphPanel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxResults;
		private System.Windows.Forms.RadioButton radioButtonCoeffs;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RadioButton radioButtonSideBySide;
		private System.Windows.Forms.RadioButton radioButtonSphereOFF;
		private System.Windows.Forms.RadioButton radioButtonSphereON;
		private System.Windows.Forms.RadioButton radioButtonSimpleScene;
		private System.Windows.Forms.Timer timer1;
	}
}

