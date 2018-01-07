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
			this.panelOutput.TabIndex = 2;
			this.panelOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseDown);
			this.panelOutput.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseMove);
			this.panelOutput.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseUp);
			// 
			// floatTrackbarControlReflectance
			// 
			this.floatTrackbarControlReflectance.Location = new System.Drawing.Point(312, 518);
			this.floatTrackbarControlReflectance.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlReflectance.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlReflectance.Name = "floatTrackbarControlReflectance";
			this.floatTrackbarControlReflectance.RangeMax = 1F;
			this.floatTrackbarControlReflectance.RangeMin = 0F;
			this.floatTrackbarControlReflectance.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlReflectance.TabIndex = 3;
			this.floatTrackbarControlReflectance.Value = 0.5F;
			this.floatTrackbarControlReflectance.VisibleRangeMax = 1F;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(241, 521);
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
			this.radioButtonOff.TabIndex = 5;
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
			this.radioButtonOn.TabIndex = 5;
			this.radioButtonOn.TabStop = true;
			this.radioButtonOn.Text = "ON";
			this.radioButtonOn.UseVisualStyleBackColor = true;
			// 
			// radioButtonGroundTruth
			// 
			this.radioButtonGroundTruth.AutoSize = true;
			this.radioButtonGroundTruth.Location = new System.Drawing.Point(114, 519);
			this.radioButtonGroundTruth.Name = "radioButtonGroundTruth";
			this.radioButtonGroundTruth.Size = new System.Drawing.Size(88, 17);
			this.radioButtonGroundTruth.TabIndex = 5;
			this.radioButtonGroundTruth.Text = "Ground Truth";
			this.radioButtonGroundTruth.UseVisualStyleBackColor = true;
			this.radioButtonGroundTruth.CheckedChanged += new System.EventHandler(this.radioButtonGroundTruth_CheckedChanged);
			// 
			// DemoForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(525, 546);
			this.Controls.Add(this.radioButtonGroundTruth);
			this.Controls.Add(this.radioButtonOn);
			this.Controls.Add(this.radioButtonOff);
			this.Controls.Add(this.label1);
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
	}
}