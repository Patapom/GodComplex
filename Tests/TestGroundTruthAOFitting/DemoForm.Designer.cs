namespace TestGroundTruthAOFitting
{
	partial class DemoForm
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
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.panelOutput = new TestGroundTruthAOFitting.PanelOutput(this.components);
			this.floatTrackbarControlReflectance = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.radioButtonOff = new System.Windows.Forms.RadioButton();
			this.radioButtonOn = new System.Windows.Forms.RadioButton();
			this.radioButtonGroundTruth = new System.Windows.Forms.RadioButton();
			this.integerTrackbarControlBounceIndex = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.floatTrackbarControlExposure = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.radioButtonBentCone = new System.Windows.Forms.RadioButton();
			this.floatTrackbarControlDebug0 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlDebug1 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.SuspendLayout();
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = 10;
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(500, 500);
			this.panelOutput.TabIndex = 6;
			this.panelOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseDown);
			this.panelOutput.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseMove);
			this.panelOutput.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseUp);
			// 
			// floatTrackbarControlReflectance
			// 
			this.floatTrackbarControlReflectance.Location = new System.Drawing.Point(354, 517);
			this.floatTrackbarControlReflectance.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlReflectance.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlReflectance.Name = "floatTrackbarControlReflectance";
			this.floatTrackbarControlReflectance.RangeMax = 1F;
			this.floatTrackbarControlReflectance.RangeMin = 0F;
			this.floatTrackbarControlReflectance.Size = new System.Drawing.Size(158, 20);
			this.floatTrackbarControlReflectance.TabIndex = 4;
			this.floatTrackbarControlReflectance.Value = 0.5F;
			this.floatTrackbarControlReflectance.VisibleRangeMax = 1F;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(283, 521);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(65, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Reflectance";
			// 
			// radioButtonOff
			// 
			this.radioButtonOff.AutoSize = true;
			this.radioButtonOff.Location = new System.Drawing.Point(12, 519);
			this.radioButtonOff.Name = "radioButtonOff";
			this.radioButtonOff.Size = new System.Drawing.Size(45, 17);
			this.radioButtonOff.TabIndex = 0;
			this.radioButtonOff.Text = "OFF";
			this.radioButtonOff.UseVisualStyleBackColor = true;
			// 
			// radioButtonOn
			// 
			this.radioButtonOn.AutoSize = true;
			this.radioButtonOn.Checked = true;
			this.radioButtonOn.Location = new System.Drawing.Point(63, 519);
			this.radioButtonOn.Name = "radioButtonOn";
			this.radioButtonOn.Size = new System.Drawing.Size(41, 17);
			this.radioButtonOn.TabIndex = 1;
			this.radioButtonOn.TabStop = true;
			this.radioButtonOn.Text = "ON";
			this.radioButtonOn.UseVisualStyleBackColor = true;
			// 
			// radioButtonGroundTruth
			// 
			this.radioButtonGroundTruth.AutoSize = true;
			this.radioButtonGroundTruth.Location = new System.Drawing.Point(189, 519);
			this.radioButtonGroundTruth.Name = "radioButtonGroundTruth";
			this.radioButtonGroundTruth.Size = new System.Drawing.Size(88, 17);
			this.radioButtonGroundTruth.TabIndex = 3;
			this.radioButtonGroundTruth.Text = "Ground Truth";
			this.radioButtonGroundTruth.UseVisualStyleBackColor = true;
			this.radioButtonGroundTruth.CheckedChanged += new System.EventHandler(this.radioButtonGroundTruth_CheckedChanged);
			// 
			// integerTrackbarControlBounceIndex
			// 
			this.integerTrackbarControlBounceIndex.Location = new System.Drawing.Point(12, 542);
			this.integerTrackbarControlBounceIndex.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlBounceIndex.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlBounceIndex.Name = "integerTrackbarControlBounceIndex";
			this.integerTrackbarControlBounceIndex.RangeMax = 20;
			this.integerTrackbarControlBounceIndex.RangeMin = 0;
			this.integerTrackbarControlBounceIndex.Size = new System.Drawing.Size(114, 20);
			this.integerTrackbarControlBounceIndex.TabIndex = 7;
			this.integerTrackbarControlBounceIndex.Value = 0;
			this.integerTrackbarControlBounceIndex.VisibleRangeMax = 20;
			// 
			// floatTrackbarControlExposure
			// 
			this.floatTrackbarControlExposure.Location = new System.Drawing.Point(354, 541);
			this.floatTrackbarControlExposure.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlExposure.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlExposure.Name = "floatTrackbarControlExposure";
			this.floatTrackbarControlExposure.RangeMax = 10F;
			this.floatTrackbarControlExposure.RangeMin = 0F;
			this.floatTrackbarControlExposure.Size = new System.Drawing.Size(158, 20);
			this.floatTrackbarControlExposure.TabIndex = 5;
			this.floatTrackbarControlExposure.Value = 1F;
			this.floatTrackbarControlExposure.VisibleRangeMax = 1F;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(283, 545);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(51, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Exposure";
			// 
			// radioButtonBentCone
			// 
			this.radioButtonBentCone.AutoSize = true;
			this.radioButtonBentCone.Location = new System.Drawing.Point(110, 519);
			this.radioButtonBentCone.Name = "radioButtonBentCone";
			this.radioButtonBentCone.Size = new System.Drawing.Size(75, 17);
			this.radioButtonBentCone.TabIndex = 2;
			this.radioButtonBentCone.Text = "Bent Cone";
			this.radioButtonBentCone.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControlDebug0
			// 
			this.floatTrackbarControlDebug0.Location = new System.Drawing.Point(12, 568);
			this.floatTrackbarControlDebug0.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDebug0.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDebug0.Name = "floatTrackbarControlDebug0";
			this.floatTrackbarControlDebug0.RangeMax = 10F;
			this.floatTrackbarControlDebug0.RangeMin = 0F;
			this.floatTrackbarControlDebug0.Size = new System.Drawing.Size(145, 20);
			this.floatTrackbarControlDebug0.TabIndex = 5;
			this.floatTrackbarControlDebug0.Value = 0.3333F;
			this.floatTrackbarControlDebug0.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlDebug1
			// 
			this.floatTrackbarControlDebug1.Location = new System.Drawing.Point(163, 568);
			this.floatTrackbarControlDebug1.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDebug1.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDebug1.Name = "floatTrackbarControlDebug1";
			this.floatTrackbarControlDebug1.RangeMax = 10F;
			this.floatTrackbarControlDebug1.RangeMin = 0F;
			this.floatTrackbarControlDebug1.Size = new System.Drawing.Size(145, 20);
			this.floatTrackbarControlDebug1.TabIndex = 5;
			this.floatTrackbarControlDebug1.Value = 0.5185F;
			this.floatTrackbarControlDebug1.VisibleRangeMax = 1F;
			// 
			// DemoForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(525, 587);
			this.Controls.Add(this.integerTrackbarControlBounceIndex);
			this.Controls.Add(this.radioButtonGroundTruth);
			this.Controls.Add(this.radioButtonOn);
			this.Controls.Add(this.radioButtonBentCone);
			this.Controls.Add(this.radioButtonOff);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlDebug1);
			this.Controls.Add(this.floatTrackbarControlDebug0);
			this.Controls.Add(this.floatTrackbarControlExposure);
			this.Controls.Add(this.floatTrackbarControlReflectance);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DemoForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Test AO";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Timer timer;
		private PanelOutput panelOutput;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlReflectance;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton radioButtonOff;
		private System.Windows.Forms.RadioButton radioButtonOn;
		private System.Windows.Forms.RadioButton radioButtonGroundTruth;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlBounceIndex;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlExposure;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RadioButton radioButtonBentCone;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDebug0;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDebug1;
	}
}