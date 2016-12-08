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
			this.radioButtonCoeffs = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.radioButtonSideBySide = new System.Windows.Forms.RadioButton();
			this.radioButtonSingleSphere = new System.Windows.Forms.RadioButton();
			this.radioButtonSimpleScene = new System.Windows.Forms.RadioButton();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.buttonReload = new System.Windows.Forms.Button();
			this.checkBoxAO = new System.Windows.Forms.CheckBox();
			this.checkBoxShowAO = new System.Windows.Forms.CheckBox();
			this.checkBoxShowBentNormal = new System.Windows.Forms.CheckBox();
			this.checkBoxEnvironmentSH = new System.Windows.Forms.CheckBox();
			this.graphPanel = new TestSHIrradiance.GraphPanel(this.components);
			this.floatTrackbarControlLuminanceFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// floatTrackbarControlThetaMax
			// 
			this.floatTrackbarControlThetaMax.Location = new System.Drawing.Point(103, 614);
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
			this.label1.Location = new System.Drawing.Point(12, 618);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(85, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Cone Max Angle";
			// 
			// textBoxResults
			// 
			this.textBoxResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxResults.Location = new System.Drawing.Point(818, 57);
			this.textBoxResults.Multiline = true;
			this.textBoxResults.Name = "textBoxResults";
			this.textBoxResults.ReadOnly = true;
			this.textBoxResults.Size = new System.Drawing.Size(283, 550);
			this.textBoxResults.TabIndex = 3;
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
			this.radioButtonCoeffs.CheckedChanged += new System.EventHandler(this.radioButtonCoeffs_CheckedChanged);
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
			this.radioButtonSideBySide.CheckedChanged += new System.EventHandler(this.radioButtonCoeffs_CheckedChanged);
			// 
			// radioButtonSingleSphere
			// 
			this.radioButtonSingleSphere.AutoSize = true;
			this.radioButtonSingleSphere.Location = new System.Drawing.Point(358, 11);
			this.radioButtonSingleSphere.Name = "radioButtonSingleSphere";
			this.radioButtonSingleSphere.Size = new System.Drawing.Size(91, 17);
			this.radioButtonSingleSphere.TabIndex = 4;
			this.radioButtonSingleSphere.TabStop = true;
			this.radioButtonSingleSphere.Text = "Single Sphere";
			this.radioButtonSingleSphere.UseVisualStyleBackColor = true;
			this.radioButtonSingleSphere.CheckedChanged += new System.EventHandler(this.radioButtonCoeffs_CheckedChanged);
			// 
			// radioButtonSimpleScene
			// 
			this.radioButtonSimpleScene.AutoSize = true;
			this.radioButtonSimpleScene.Location = new System.Drawing.Point(455, 11);
			this.radioButtonSimpleScene.Name = "radioButtonSimpleScene";
			this.radioButtonSimpleScene.Size = new System.Drawing.Size(90, 17);
			this.radioButtonSimpleScene.TabIndex = 4;
			this.radioButtonSimpleScene.TabStop = true;
			this.radioButtonSimpleScene.Text = "Simple Scene";
			this.radioButtonSimpleScene.UseVisualStyleBackColor = true;
			this.radioButtonSimpleScene.CheckedChanged += new System.EventHandler(this.radioButtonCoeffs_CheckedChanged);
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 10;
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(1026, 652);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 6;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// checkBoxAO
			// 
			this.checkBoxAO.AutoSize = true;
			this.checkBoxAO.Checked = true;
			this.checkBoxAO.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAO.Location = new System.Drawing.Point(358, 34);
			this.checkBoxAO.Name = "checkBoxAO";
			this.checkBoxAO.Size = new System.Drawing.Size(97, 17);
			this.checkBoxAO.TabIndex = 7;
			this.checkBoxAO.Text = "AO Correct ON";
			this.checkBoxAO.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowAO
			// 
			this.checkBoxShowAO.AutoSize = true;
			this.checkBoxShowAO.Location = new System.Drawing.Point(461, 34);
			this.checkBoxShowAO.Name = "checkBoxShowAO";
			this.checkBoxShowAO.Size = new System.Drawing.Size(71, 17);
			this.checkBoxShowAO.TabIndex = 7;
			this.checkBoxShowAO.Text = "Show AO";
			this.checkBoxShowAO.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowBentNormal
			// 
			this.checkBoxShowBentNormal.AutoSize = true;
			this.checkBoxShowBentNormal.Location = new System.Drawing.Point(538, 34);
			this.checkBoxShowBentNormal.Name = "checkBoxShowBentNormal";
			this.checkBoxShowBentNormal.Size = new System.Drawing.Size(114, 17);
			this.checkBoxShowBentNormal.TabIndex = 7;
			this.checkBoxShowBentNormal.Text = "Show Bent Normal";
			this.checkBoxShowBentNormal.UseVisualStyleBackColor = true;
			// 
			// checkBoxEnvironmentSH
			// 
			this.checkBoxEnvironmentSH.AutoSize = true;
			this.checkBoxEnvironmentSH.Location = new System.Drawing.Point(231, 34);
			this.checkBoxEnvironmentSH.Name = "checkBoxEnvironmentSH";
			this.checkBoxEnvironmentSH.Size = new System.Drawing.Size(103, 17);
			this.checkBoxEnvironmentSH.TabIndex = 7;
			this.checkBoxEnvironmentSH.Text = "Environment SH";
			this.checkBoxEnvironmentSH.UseVisualStyleBackColor = true;
			// 
			// graphPanel
			// 
			this.graphPanel.Bitmap = null;
			this.graphPanel.EnablePaint = true;
			this.graphPanel.Location = new System.Drawing.Point(12, 57);
			this.graphPanel.MessageOnEmpty = "Bisou";
			this.graphPanel.Name = "graphPanel";
			this.graphPanel.Size = new System.Drawing.Size(800, 550);
			this.graphPanel.TabIndex = 1;
			// 
			// floatTrackbarControlLuminanceFactor
			// 
			this.floatTrackbarControlLuminanceFactor.Location = new System.Drawing.Point(750, 9);
			this.floatTrackbarControlLuminanceFactor.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLuminanceFactor.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLuminanceFactor.Name = "floatTrackbarControlLuminanceFactor";
			this.floatTrackbarControlLuminanceFactor.RangeMax = 100F;
			this.floatTrackbarControlLuminanceFactor.RangeMin = 0F;
			this.floatTrackbarControlLuminanceFactor.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLuminanceFactor.TabIndex = 0;
			this.floatTrackbarControlLuminanceFactor.Value = 1F;
			this.floatTrackbarControlLuminanceFactor.VisibleRangeMax = 2F;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(659, 13);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(92, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Luminance Factor";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1113, 687);
			this.Controls.Add(this.checkBoxShowBentNormal);
			this.Controls.Add(this.checkBoxEnvironmentSH);
			this.Controls.Add(this.checkBoxShowAO);
			this.Controls.Add(this.checkBoxAO);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.radioButtonSimpleScene);
			this.Controls.Add(this.radioButtonSingleSphere);
			this.Controls.Add(this.radioButtonSideBySide);
			this.Controls.Add(this.radioButtonCoeffs);
			this.Controls.Add(this.textBoxResults);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.graphPanel);
			this.Controls.Add(this.floatTrackbarControlLuminanceFactor);
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
		private System.Windows.Forms.RadioButton radioButtonSingleSphere;
		private System.Windows.Forms.RadioButton radioButtonSimpleScene;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.CheckBox checkBoxAO;
		private System.Windows.Forms.CheckBox checkBoxShowAO;
		private System.Windows.Forms.CheckBox checkBoxShowBentNormal;
		private System.Windows.Forms.CheckBox checkBoxEnvironmentSH;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLuminanceFactor;
		private System.Windows.Forms.Label label3;
	}
}

