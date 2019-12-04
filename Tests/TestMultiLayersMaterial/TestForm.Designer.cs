namespace TestMultiLayersMaterial
{
	partial class TestForm
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
			this.floatTrackbarControlTangent0 = new UIUtility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlTangent1 = new UIUtility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlTangent2 = new UIUtility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlTangent3 = new UIUtility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.floatTrackbarControlBrushStrength = new UIUtility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.buttonClear = new System.Windows.Forms.Button();
			this.buttonClearGradient = new System.Windows.Forms.Button();
			this.panelOutputResult = new TestMultiLayersMaterial.PanelOutput(this.components);
			this.panelOutputLevels = new TestMultiLayersMaterial.PanelOutput(this.components);
			this.panelOutputMask = new TestMultiLayersMaterial.PanelOutput(this.components);
			this.buttonResetLevels = new System.Windows.Forms.Button();
			this.floatTrackbarControlTangent1_Out = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlTangent2_Out = new UIUtility.FloatTrackbarControl();
			this.checkBoxSplit1 = new System.Windows.Forms.CheckBox();
			this.checkBoxSplit2 = new System.Windows.Forms.CheckBox();
			this.buttonSaveMask = new System.Windows.Forms.Button();
			this.buttonLoadMat1 = new System.Windows.Forms.Button();
			this.buttonLoadMat0 = new System.Windows.Forms.Button();
			this.buttonLoadMat2 = new System.Windows.Forms.Button();
			this.buttonLoadMat3 = new System.Windows.Forms.Button();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.buttonLoadMask = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// floatTrackbarControlTangent0
			// 
			this.floatTrackbarControlTangent0.Location = new System.Drawing.Point(100, 601);
			this.floatTrackbarControlTangent0.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlTangent0.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlTangent0.Name = "floatTrackbarControlTangent0";
			this.floatTrackbarControlTangent0.RangeMax = 1F;
			this.floatTrackbarControlTangent0.RangeMin = -1F;
			this.floatTrackbarControlTangent0.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlTangent0.TabIndex = 1;
			this.floatTrackbarControlTangent0.Value = 0F;
			this.floatTrackbarControlTangent0.VisibleRangeMax = 1F;
			this.floatTrackbarControlTangent0.VisibleRangeMin = -1F;
			this.floatTrackbarControlTangent0.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlTangent0_ValueChanged_1);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 603);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(85, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Tangent Layer 0";
			// 
			// floatTrackbarControlTangent1
			// 
			this.floatTrackbarControlTangent1.Location = new System.Drawing.Point(100, 627);
			this.floatTrackbarControlTangent1.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlTangent1.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlTangent1.Name = "floatTrackbarControlTangent1";
			this.floatTrackbarControlTangent1.RangeMax = 1F;
			this.floatTrackbarControlTangent1.RangeMin = -1F;
			this.floatTrackbarControlTangent1.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlTangent1.TabIndex = 1;
			this.floatTrackbarControlTangent1.Value = 0F;
			this.floatTrackbarControlTangent1.VisibleRangeMax = 1F;
			this.floatTrackbarControlTangent1.VisibleRangeMin = -1F;
			this.floatTrackbarControlTangent1.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlTangent0_ValueChanged_1);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 629);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(85, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Tangent Layer 1";
			// 
			// floatTrackbarControlTangent2
			// 
			this.floatTrackbarControlTangent2.Location = new System.Drawing.Point(100, 653);
			this.floatTrackbarControlTangent2.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlTangent2.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlTangent2.Name = "floatTrackbarControlTangent2";
			this.floatTrackbarControlTangent2.RangeMax = 1F;
			this.floatTrackbarControlTangent2.RangeMin = -1F;
			this.floatTrackbarControlTangent2.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlTangent2.TabIndex = 1;
			this.floatTrackbarControlTangent2.Value = 0F;
			this.floatTrackbarControlTangent2.VisibleRangeMax = 1F;
			this.floatTrackbarControlTangent2.VisibleRangeMin = -1F;
			this.floatTrackbarControlTangent2.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlTangent0_ValueChanged_1);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(9, 655);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(85, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Tangent Layer 2";
			// 
			// floatTrackbarControlTangent3
			// 
			this.floatTrackbarControlTangent3.Location = new System.Drawing.Point(100, 679);
			this.floatTrackbarControlTangent3.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlTangent3.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlTangent3.Name = "floatTrackbarControlTangent3";
			this.floatTrackbarControlTangent3.RangeMax = 1F;
			this.floatTrackbarControlTangent3.RangeMin = -1F;
			this.floatTrackbarControlTangent3.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlTangent3.TabIndex = 1;
			this.floatTrackbarControlTangent3.Value = 0F;
			this.floatTrackbarControlTangent3.Visible = false;
			this.floatTrackbarControlTangent3.VisibleRangeMax = 1F;
			this.floatTrackbarControlTangent3.VisibleRangeMin = -1F;
			this.floatTrackbarControlTangent3.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlTangent0_ValueChanged_1);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(9, 681);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(85, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "Tangent Layer 3";
			this.label4.Visible = false;
			// 
			// floatTrackbarControlBrushStrength
			// 
			this.floatTrackbarControlBrushStrength.Location = new System.Drawing.Point(100, 540);
			this.floatTrackbarControlBrushStrength.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBrushStrength.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBrushStrength.Name = "floatTrackbarControlBrushStrength";
			this.floatTrackbarControlBrushStrength.RangeMax = 1F;
			this.floatTrackbarControlBrushStrength.RangeMin = 0F;
			this.floatTrackbarControlBrushStrength.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlBrushStrength.TabIndex = 1;
			this.floatTrackbarControlBrushStrength.Value = 0.1F;
			this.floatTrackbarControlBrushStrength.VisibleRangeMax = 1F;
			this.floatTrackbarControlBrushStrength.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlBrushStrength_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(9, 542);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(77, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "Brush Strength";
			// 
			// buttonClear
			// 
			this.buttonClear.Location = new System.Drawing.Point(360, 537);
			this.buttonClear.Name = "buttonClear";
			this.buttonClear.Size = new System.Drawing.Size(75, 23);
			this.buttonClear.TabIndex = 3;
			this.buttonClear.Text = "Clear";
			this.buttonClear.UseVisualStyleBackColor = true;
			this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
			// 
			// buttonClearGradient
			// 
			this.buttonClearGradient.Location = new System.Drawing.Point(441, 537);
			this.buttonClearGradient.Name = "buttonClearGradient";
			this.buttonClearGradient.Size = new System.Drawing.Size(83, 23);
			this.buttonClearGradient.TabIndex = 3;
			this.buttonClearGradient.Text = "Clear Gradient";
			this.buttonClearGradient.UseVisualStyleBackColor = true;
			this.buttonClearGradient.Click += new System.EventHandler(this.buttonClearGradient_Click);
			// 
			// panelOutputResult
			// 
			this.panelOutputResult.Location = new System.Drawing.Point(530, 12);
			this.panelOutputResult.Name = "panelOutputResult";
			this.panelOutputResult.Size = new System.Drawing.Size(512, 512);
			this.panelOutputResult.TabIndex = 0;
			// 
			// panelOutputLevels
			// 
			this.panelOutputLevels.Location = new System.Drawing.Point(630, 537);
			this.panelOutputLevels.Name = "panelOutputLevels";
			this.panelOutputLevels.Size = new System.Drawing.Size(412, 262);
			this.panelOutputLevels.TabIndex = 0;
			// 
			// panelOutputMask
			// 
			this.panelOutputMask.Location = new System.Drawing.Point(12, 12);
			this.panelOutputMask.Name = "panelOutputMask";
			this.panelOutputMask.Size = new System.Drawing.Size(512, 512);
			this.panelOutputMask.TabIndex = 0;
			this.panelOutputMask.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOutputMask_MouseDown);
			this.panelOutputMask.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelOutputMask_MouseMove);
			this.panelOutputMask.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelOutputMask_MouseUp);
			// 
			// buttonResetLevels
			// 
			this.buttonResetLevels.Location = new System.Drawing.Point(225, 705);
			this.buttonResetLevels.Name = "buttonResetLevels";
			this.buttonResetLevels.Size = new System.Drawing.Size(75, 23);
			this.buttonResetLevels.TabIndex = 3;
			this.buttonResetLevels.Text = "Reset";
			this.buttonResetLevels.UseVisualStyleBackColor = true;
			this.buttonResetLevels.Click += new System.EventHandler(this.buttonResetLevels_Click);
			// 
			// floatTrackbarControlTangent1_Out
			// 
			this.floatTrackbarControlTangent1_Out.Enabled = false;
			this.floatTrackbarControlTangent1_Out.Location = new System.Drawing.Point(345, 627);
			this.floatTrackbarControlTangent1_Out.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlTangent1_Out.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlTangent1_Out.Name = "floatTrackbarControlTangent1_Out";
			this.floatTrackbarControlTangent1_Out.RangeMax = 1F;
			this.floatTrackbarControlTangent1_Out.RangeMin = -1F;
			this.floatTrackbarControlTangent1_Out.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlTangent1_Out.TabIndex = 1;
			this.floatTrackbarControlTangent1_Out.Value = 0F;
			this.floatTrackbarControlTangent1_Out.Visible = false;
			this.floatTrackbarControlTangent1_Out.VisibleRangeMax = 1F;
			this.floatTrackbarControlTangent1_Out.VisibleRangeMin = -1F;
			this.floatTrackbarControlTangent1_Out.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlTangent0_ValueChanged_1);
			// 
			// floatTrackbarControlTangent2_Out
			// 
			this.floatTrackbarControlTangent2_Out.Enabled = false;
			this.floatTrackbarControlTangent2_Out.Location = new System.Drawing.Point(345, 653);
			this.floatTrackbarControlTangent2_Out.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlTangent2_Out.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlTangent2_Out.Name = "floatTrackbarControlTangent2_Out";
			this.floatTrackbarControlTangent2_Out.RangeMax = 1F;
			this.floatTrackbarControlTangent2_Out.RangeMin = -1F;
			this.floatTrackbarControlTangent2_Out.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlTangent2_Out.TabIndex = 1;
			this.floatTrackbarControlTangent2_Out.Value = 0F;
			this.floatTrackbarControlTangent2_Out.Visible = false;
			this.floatTrackbarControlTangent2_Out.VisibleRangeMax = 1F;
			this.floatTrackbarControlTangent2_Out.VisibleRangeMin = -1F;
			this.floatTrackbarControlTangent2_Out.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlTangent0_ValueChanged_1);
			// 
			// checkBoxSplit1
			// 
			this.checkBoxSplit1.AutoSize = true;
			this.checkBoxSplit1.Location = new System.Drawing.Point(324, 629);
			this.checkBoxSplit1.Name = "checkBoxSplit1";
			this.checkBoxSplit1.Size = new System.Drawing.Size(15, 14);
			this.checkBoxSplit1.TabIndex = 4;
			this.checkBoxSplit1.UseVisualStyleBackColor = true;
			this.checkBoxSplit1.Visible = false;
			this.checkBoxSplit1.CheckedChanged += new System.EventHandler(this.checkBoxSplit1_CheckedChanged);
			// 
			// checkBoxSplit2
			// 
			this.checkBoxSplit2.AutoSize = true;
			this.checkBoxSplit2.Location = new System.Drawing.Point(324, 655);
			this.checkBoxSplit2.Name = "checkBoxSplit2";
			this.checkBoxSplit2.Size = new System.Drawing.Size(15, 14);
			this.checkBoxSplit2.TabIndex = 4;
			this.checkBoxSplit2.UseVisualStyleBackColor = true;
			this.checkBoxSplit2.Visible = false;
			this.checkBoxSplit2.CheckedChanged += new System.EventHandler(this.checkBoxSplit2_CheckedChanged);
			// 
			// buttonSaveMask
			// 
			this.buttonSaveMask.Location = new System.Drawing.Point(290, 776);
			this.buttonSaveMask.Name = "buttonSaveMask";
			this.buttonSaveMask.Size = new System.Drawing.Size(111, 23);
			this.buttonSaveMask.TabIndex = 3;
			this.buttonSaveMask.Text = "Save Height Mask";
			this.buttonSaveMask.UseVisualStyleBackColor = true;
			this.buttonSaveMask.Click += new System.EventHandler(this.buttonSaveMask_Click);
			// 
			// buttonLoadMat1
			// 
			this.buttonLoadMat1.Location = new System.Drawing.Point(12, 776);
			this.buttonLoadMat1.Name = "buttonLoadMat1";
			this.buttonLoadMat1.Size = new System.Drawing.Size(111, 23);
			this.buttonLoadMat1.TabIndex = 3;
			this.buttonLoadMat1.Text = "Load Material 1";
			this.buttonLoadMat1.UseVisualStyleBackColor = true;
			this.buttonLoadMat1.Click += new System.EventHandler(this.buttonLoadMat1_Click);
			// 
			// buttonLoadMat0
			// 
			this.buttonLoadMat0.Location = new System.Drawing.Point(12, 747);
			this.buttonLoadMat0.Name = "buttonLoadMat0";
			this.buttonLoadMat0.Size = new System.Drawing.Size(111, 23);
			this.buttonLoadMat0.TabIndex = 3;
			this.buttonLoadMat0.Text = "Load Material 0";
			this.buttonLoadMat0.UseVisualStyleBackColor = true;
			this.buttonLoadMat0.Click += new System.EventHandler(this.buttonLoadMat0_Click);
			// 
			// buttonLoadMat2
			// 
			this.buttonLoadMat2.Location = new System.Drawing.Point(129, 747);
			this.buttonLoadMat2.Name = "buttonLoadMat2";
			this.buttonLoadMat2.Size = new System.Drawing.Size(111, 23);
			this.buttonLoadMat2.TabIndex = 3;
			this.buttonLoadMat2.Text = "Load Material 2";
			this.buttonLoadMat2.UseVisualStyleBackColor = true;
			this.buttonLoadMat2.Click += new System.EventHandler(this.buttonLoadMat2_Click);
			// 
			// buttonLoadMat3
			// 
			this.buttonLoadMat3.Location = new System.Drawing.Point(129, 776);
			this.buttonLoadMat3.Name = "buttonLoadMat3";
			this.buttonLoadMat3.Size = new System.Drawing.Size(111, 23);
			this.buttonLoadMat3.TabIndex = 3;
			this.buttonLoadMat3.Text = "Load Material 3";
			this.buttonLoadMat3.UseVisualStyleBackColor = true;
			this.buttonLoadMat3.Click += new System.EventHandler(this.buttonLoadMat3_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "*.png";
			this.openFileDialog.Filter = "Image Files|*.png;*.jpg";
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.DefaultExt = "*.png";
			this.saveFileDialog.Filter = "PNG Files|*.png";
			this.saveFileDialog.RestoreDirectory = true;
			// 
			// buttonLoadMask
			// 
			this.buttonLoadMask.Location = new System.Drawing.Point(290, 747);
			this.buttonLoadMask.Name = "buttonLoadMask";
			this.buttonLoadMask.Size = new System.Drawing.Size(111, 23);
			this.buttonLoadMask.TabIndex = 3;
			this.buttonLoadMask.Text = "Load Height Mask";
			this.buttonLoadMask.UseVisualStyleBackColor = true;
			this.buttonLoadMask.Click += new System.EventHandler(this.buttonLoadMask_Click);
			// 
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1055, 811);
			this.Controls.Add(this.checkBoxSplit2);
			this.Controls.Add(this.checkBoxSplit1);
			this.Controls.Add(this.panelOutputLevels);
			this.Controls.Add(this.buttonClearGradient);
			this.Controls.Add(this.buttonLoadMat0);
			this.Controls.Add(this.buttonLoadMat3);
			this.Controls.Add(this.buttonLoadMat2);
			this.Controls.Add(this.buttonLoadMat1);
			this.Controls.Add(this.buttonLoadMask);
			this.Controls.Add(this.buttonSaveMask);
			this.Controls.Add(this.buttonResetLevels);
			this.Controls.Add(this.buttonClear);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlTangent3);
			this.Controls.Add(this.floatTrackbarControlTangent2_Out);
			this.Controls.Add(this.floatTrackbarControlTangent2);
			this.Controls.Add(this.floatTrackbarControlTangent1_Out);
			this.Controls.Add(this.floatTrackbarControlTangent1);
			this.Controls.Add(this.floatTrackbarControlBrushStrength);
			this.Controls.Add(this.floatTrackbarControlTangent0);
			this.Controls.Add(this.panelOutputResult);
			this.Controls.Add(this.panelOutputMask);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "TestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Multi-Layers Material Painter";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelOutputMask;
		private PanelOutput panelOutputResult;
		private UIUtility.FloatTrackbarControl floatTrackbarControlTangent0;
		private System.Windows.Forms.Label label1;
		private UIUtility.FloatTrackbarControl floatTrackbarControlTangent1;
		private System.Windows.Forms.Label label2;
		private UIUtility.FloatTrackbarControl floatTrackbarControlTangent2;
		private System.Windows.Forms.Label label3;
		private UIUtility.FloatTrackbarControl floatTrackbarControlTangent3;
		private System.Windows.Forms.Label label4;
		private UIUtility.FloatTrackbarControl floatTrackbarControlBrushStrength;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button buttonClear;
		private System.Windows.Forms.Button buttonClearGradient;
		private PanelOutput panelOutputLevels;
		private System.Windows.Forms.Button buttonResetLevels;
		private UIUtility.FloatTrackbarControl floatTrackbarControlTangent1_Out;
		private UIUtility.FloatTrackbarControl floatTrackbarControlTangent2_Out;
		private System.Windows.Forms.CheckBox checkBoxSplit1;
		private System.Windows.Forms.CheckBox checkBoxSplit2;
		private System.Windows.Forms.Button buttonSaveMask;
		private System.Windows.Forms.Button buttonLoadMat1;
		private System.Windows.Forms.Button buttonLoadMat0;
		private System.Windows.Forms.Button buttonLoadMat2;
		private System.Windows.Forms.Button buttonLoadMat3;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.Button buttonLoadMask;
	}
}

