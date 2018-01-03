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
			this.checkBoxEnableAO = new System.Windows.Forms.CheckBox();
			this.panelOutput = new TestGroundTruthAOFitting.PanelOutput(this.components);
			this.floatTrackbarControlReflectance = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = 10;
			// 
			// checkBoxEnableAO
			// 
			this.checkBoxEnableAO.AutoSize = true;
			this.checkBoxEnableAO.Checked = true;
			this.checkBoxEnableAO.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnableAO.Location = new System.Drawing.Point(12, 518);
			this.checkBoxEnableAO.Name = "checkBoxEnableAO";
			this.checkBoxEnableAO.Size = new System.Drawing.Size(88, 17);
			this.checkBoxEnableAO.TabIndex = 1;
			this.checkBoxEnableAO.Text = "Improved AO";
			this.checkBoxEnableAO.UseVisualStyleBackColor = true;
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
			this.floatTrackbarControlReflectance.Location = new System.Drawing.Point(203, 516);
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
			this.label1.Location = new System.Drawing.Point(132, 519);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(65, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Reflectance";
			// 
			// DemoForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(525, 546);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlReflectance);
			this.Controls.Add(this.panelOutput);
			this.Controls.Add(this.checkBoxEnableAO);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DemoForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Test AO";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.CheckBox checkBoxEnableAO;
		private PanelOutput panelOutput;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlReflectance;
		private System.Windows.Forms.Label label1;
	}
}