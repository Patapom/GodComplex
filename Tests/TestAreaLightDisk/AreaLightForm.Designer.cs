namespace AreaLightTest
{
	partial class AreaLightForm
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
				m_device.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AreaLightForm));
			this.floatTrackbarControlLuminance = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.buttonReload = new System.Windows.Forms.Button();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLightPosX = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label7 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLightPosY = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label8 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLightPosZ = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label9 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLightRoll = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label13 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLightScaleX = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label17 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLightScaleY = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.radioButtonFreeTarget = new System.Windows.Forms.RadioButton();
			this.radioButtonHorizontalTarget = new System.Windows.Forms.RadioButton();
			this.radioButtonNegativeFreeTarget = new System.Windows.Forms.RadioButton();
			this.textBoxResults = new System.Windows.Forms.TextBox();
			this.checkBoxShowReference = new System.Windows.Forms.CheckBox();
			this.checkBoxDebugMatrix = new System.Windows.Forms.CheckBox();
			this.panelVisualizeLTCTransform = new System.Windows.Forms.Panel();
			this.radioButtonGGX = new System.Windows.Forms.RadioButton();
			this.radioButtonOrenNayar = new System.Windows.Forms.RadioButton();
			this.floatTrackbarControlViewAngle = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlRoughness = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.panelOutput = new AreaLightTest.PanelOutput(this.components);
			this.checkBoxShowDiff = new System.Windows.Forms.CheckBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.panelVisualizeLTCTransform.SuspendLayout();
			this.SuspendLayout();
			// 
			// floatTrackbarControlLuminance
			// 
			this.floatTrackbarControlLuminance.Location = new System.Drawing.Point(1180, 12);
			this.floatTrackbarControlLuminance.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLuminance.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLuminance.Name = "floatTrackbarControlLuminance";
			this.floatTrackbarControlLuminance.RangeMax = 10000F;
			this.floatTrackbarControlLuminance.RangeMin = 0F;
			this.floatTrackbarControlLuminance.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLuminance.TabIndex = 1;
			this.floatTrackbarControlLuminance.Value = 1F;
			this.floatTrackbarControlLuminance.VisibleRangeMax = 1F;
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(1305, 629);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 2;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = 10;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(1059, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(59, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Luminance";
			// 
			// floatTrackbarControlLightPosX
			// 
			this.floatTrackbarControlLightPosX.Location = new System.Drawing.Point(1180, 70);
			this.floatTrackbarControlLightPosX.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightPosX.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightPosX.Name = "floatTrackbarControlLightPosX";
			this.floatTrackbarControlLightPosX.RangeMax = 100F;
			this.floatTrackbarControlLightPosX.RangeMin = -100F;
			this.floatTrackbarControlLightPosX.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightPosX.TabIndex = 1;
			this.floatTrackbarControlLightPosX.Value = 0F;
			this.floatTrackbarControlLightPosX.VisibleRangeMax = 3F;
			this.floatTrackbarControlLightPosX.VisibleRangeMin = -3F;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(1059, 75);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(61, 13);
			this.label7.TabIndex = 3;
			this.label7.Text = "Light Pos X";
			// 
			// floatTrackbarControlLightPosY
			// 
			this.floatTrackbarControlLightPosY.Location = new System.Drawing.Point(1180, 96);
			this.floatTrackbarControlLightPosY.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightPosY.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightPosY.Name = "floatTrackbarControlLightPosY";
			this.floatTrackbarControlLightPosY.RangeMax = 100F;
			this.floatTrackbarControlLightPosY.RangeMin = -100F;
			this.floatTrackbarControlLightPosY.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightPosY.TabIndex = 1;
			this.floatTrackbarControlLightPosY.Value = 2F;
			this.floatTrackbarControlLightPosY.VisibleRangeMax = 4F;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(1059, 101);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(61, 13);
			this.label8.TabIndex = 3;
			this.label8.Text = "Light Pos Y";
			// 
			// floatTrackbarControlLightPosZ
			// 
			this.floatTrackbarControlLightPosZ.Location = new System.Drawing.Point(1180, 122);
			this.floatTrackbarControlLightPosZ.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightPosZ.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightPosZ.Name = "floatTrackbarControlLightPosZ";
			this.floatTrackbarControlLightPosZ.RangeMax = 100F;
			this.floatTrackbarControlLightPosZ.RangeMin = -100F;
			this.floatTrackbarControlLightPosZ.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightPosZ.TabIndex = 1;
			this.floatTrackbarControlLightPosZ.Value = 0F;
			this.floatTrackbarControlLightPosZ.VisibleRangeMax = 3F;
			this.floatTrackbarControlLightPosZ.VisibleRangeMin = -3F;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(1059, 127);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(61, 13);
			this.label9.TabIndex = 3;
			this.label9.Text = "Light Pos Z";
			// 
			// floatTrackbarControlLightRoll
			// 
			this.floatTrackbarControlLightRoll.Location = new System.Drawing.Point(1180, 238);
			this.floatTrackbarControlLightRoll.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightRoll.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightRoll.Name = "floatTrackbarControlLightRoll";
			this.floatTrackbarControlLightRoll.RangeMax = 180F;
			this.floatTrackbarControlLightRoll.RangeMin = -180F;
			this.floatTrackbarControlLightRoll.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightRoll.TabIndex = 1;
			this.floatTrackbarControlLightRoll.Value = 0F;
			this.floatTrackbarControlLightRoll.VisibleRangeMax = 180F;
			this.floatTrackbarControlLightRoll.VisibleRangeMin = -180F;
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(1059, 240);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(51, 13);
			this.label13.TabIndex = 3;
			this.label13.Text = "Light Roll";
			// 
			// floatTrackbarControlLightScaleX
			// 
			this.floatTrackbarControlLightScaleX.Location = new System.Drawing.Point(1180, 172);
			this.floatTrackbarControlLightScaleX.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightScaleX.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightScaleX.Name = "floatTrackbarControlLightScaleX";
			this.floatTrackbarControlLightScaleX.RangeMax = 100000F;
			this.floatTrackbarControlLightScaleX.RangeMin = 0.001F;
			this.floatTrackbarControlLightScaleX.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightScaleX.TabIndex = 1;
			this.floatTrackbarControlLightScaleX.Value = 1F;
			this.floatTrackbarControlLightScaleX.VisibleRangeMax = 2F;
			this.floatTrackbarControlLightScaleX.VisibleRangeMin = 0.001F;
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(1059, 175);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(70, 13);
			this.label17.TabIndex = 3;
			this.label17.Text = "Light Scale X";
			// 
			// floatTrackbarControlLightScaleY
			// 
			this.floatTrackbarControlLightScaleY.Location = new System.Drawing.Point(1180, 198);
			this.floatTrackbarControlLightScaleY.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightScaleY.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightScaleY.Name = "floatTrackbarControlLightScaleY";
			this.floatTrackbarControlLightScaleY.RangeMax = 100000F;
			this.floatTrackbarControlLightScaleY.RangeMin = 0.001F;
			this.floatTrackbarControlLightScaleY.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightScaleY.TabIndex = 1;
			this.floatTrackbarControlLightScaleY.Value = 1F;
			this.floatTrackbarControlLightScaleY.VisibleRangeMax = 2F;
			this.floatTrackbarControlLightScaleY.VisibleRangeMin = 0.001F;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(1059, 201);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(70, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Light Scale Y";
			// 
			// radioButtonFreeTarget
			// 
			this.radioButtonFreeTarget.AutoSize = true;
			this.radioButtonFreeTarget.Checked = true;
			this.radioButtonFreeTarget.Location = new System.Drawing.Point(1062, 279);
			this.radioButtonFreeTarget.Name = "radioButtonFreeTarget";
			this.radioButtonFreeTarget.Size = new System.Drawing.Size(80, 17);
			this.radioButtonFreeTarget.TabIndex = 4;
			this.radioButtonFreeTarget.TabStop = true;
			this.radioButtonFreeTarget.Text = "Free Target";
			this.radioButtonFreeTarget.UseVisualStyleBackColor = true;
			// 
			// radioButtonHorizontalTarget
			// 
			this.radioButtonHorizontalTarget.AutoSize = true;
			this.radioButtonHorizontalTarget.Location = new System.Drawing.Point(1148, 279);
			this.radioButtonHorizontalTarget.Name = "radioButtonHorizontalTarget";
			this.radioButtonHorizontalTarget.Size = new System.Drawing.Size(72, 17);
			this.radioButtonHorizontalTarget.TabIndex = 4;
			this.radioButtonHorizontalTarget.TabStop = true;
			this.radioButtonHorizontalTarget.Text = "Horizontal";
			this.radioButtonHorizontalTarget.UseVisualStyleBackColor = true;
			// 
			// radioButtonNegativeFreeTarget
			// 
			this.radioButtonNegativeFreeTarget.AutoSize = true;
			this.radioButtonNegativeFreeTarget.Location = new System.Drawing.Point(1226, 279);
			this.radioButtonNegativeFreeTarget.Name = "radioButtonNegativeFreeTarget";
			this.radioButtonNegativeFreeTarget.Size = new System.Drawing.Size(102, 17);
			this.radioButtonNegativeFreeTarget.TabIndex = 4;
			this.radioButtonNegativeFreeTarget.TabStop = true;
			this.radioButtonNegativeFreeTarget.Text = "Negative Target";
			this.radioButtonNegativeFreeTarget.UseVisualStyleBackColor = true;
			// 
			// textBoxResults
			// 
			this.textBoxResults.Location = new System.Drawing.Point(1062, 450);
			this.textBoxResults.Multiline = true;
			this.textBoxResults.Name = "textBoxResults";
			this.textBoxResults.ReadOnly = true;
			this.textBoxResults.Size = new System.Drawing.Size(318, 173);
			this.textBoxResults.TabIndex = 5;
			// 
			// checkBoxShowReference
			// 
			this.checkBoxShowReference.AutoSize = true;
			this.checkBoxShowReference.Location = new System.Drawing.Point(1062, 302);
			this.checkBoxShowReference.Name = "checkBoxShowReference";
			this.checkBoxShowReference.Size = new System.Drawing.Size(178, 17);
			this.checkBoxShowReference.TabIndex = 6;
			this.checkBoxShowReference.Text = "Show Reference (Ground Truth)";
			this.checkBoxShowReference.UseVisualStyleBackColor = true;
			// 
			// checkBoxDebugMatrix
			// 
			this.checkBoxDebugMatrix.AutoSize = true;
			this.checkBoxDebugMatrix.Location = new System.Drawing.Point(1062, 342);
			this.checkBoxDebugMatrix.Name = "checkBoxDebugMatrix";
			this.checkBoxDebugMatrix.Size = new System.Drawing.Size(140, 17);
			this.checkBoxDebugMatrix.TabIndex = 7;
			this.checkBoxDebugMatrix.Text = "Visualize LTC Transform";
			this.checkBoxDebugMatrix.UseVisualStyleBackColor = true;
			this.checkBoxDebugMatrix.CheckedChanged += new System.EventHandler(this.checkBoxDebugMatrix_CheckedChanged);
			// 
			// panelVisualizeLTCTransform
			// 
			this.panelVisualizeLTCTransform.Controls.Add(this.radioButtonGGX);
			this.panelVisualizeLTCTransform.Controls.Add(this.radioButtonOrenNayar);
			this.panelVisualizeLTCTransform.Controls.Add(this.floatTrackbarControlViewAngle);
			this.panelVisualizeLTCTransform.Controls.Add(this.floatTrackbarControlRoughness);
			this.panelVisualizeLTCTransform.Controls.Add(this.label4);
			this.panelVisualizeLTCTransform.Controls.Add(this.label3);
			this.panelVisualizeLTCTransform.Enabled = false;
			this.panelVisualizeLTCTransform.Location = new System.Drawing.Point(1062, 366);
			this.panelVisualizeLTCTransform.Name = "panelVisualizeLTCTransform";
			this.panelVisualizeLTCTransform.Size = new System.Drawing.Size(318, 78);
			this.panelVisualizeLTCTransform.TabIndex = 8;
			// 
			// radioButtonGGX
			// 
			this.radioButtonGGX.AutoSize = true;
			this.radioButtonGGX.Location = new System.Drawing.Point(86, 3);
			this.radioButtonGGX.Name = "radioButtonGGX";
			this.radioButtonGGX.Size = new System.Drawing.Size(48, 17);
			this.radioButtonGGX.TabIndex = 0;
			this.radioButtonGGX.Text = "GGX";
			this.radioButtonGGX.UseVisualStyleBackColor = true;
			// 
			// radioButtonOrenNayar
			// 
			this.radioButtonOrenNayar.AutoSize = true;
			this.radioButtonOrenNayar.Checked = true;
			this.radioButtonOrenNayar.Location = new System.Drawing.Point(3, 3);
			this.radioButtonOrenNayar.Name = "radioButtonOrenNayar";
			this.radioButtonOrenNayar.Size = new System.Drawing.Size(79, 17);
			this.radioButtonOrenNayar.TabIndex = 0;
			this.radioButtonOrenNayar.TabStop = true;
			this.radioButtonOrenNayar.Text = "Oren-Nayar";
			this.radioButtonOrenNayar.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControlViewAngle
			// 
			this.floatTrackbarControlViewAngle.Location = new System.Drawing.Point(118, 52);
			this.floatTrackbarControlViewAngle.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlViewAngle.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlViewAngle.Name = "floatTrackbarControlViewAngle";
			this.floatTrackbarControlViewAngle.RangeMax = 90F;
			this.floatTrackbarControlViewAngle.RangeMin = 0F;
			this.floatTrackbarControlViewAngle.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlViewAngle.TabIndex = 1;
			this.floatTrackbarControlViewAngle.Value = 0F;
			this.floatTrackbarControlViewAngle.VisibleRangeMax = 90F;
			// 
			// floatTrackbarControlRoughness
			// 
			this.floatTrackbarControlRoughness.Location = new System.Drawing.Point(118, 26);
			this.floatTrackbarControlRoughness.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlRoughness.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlRoughness.Name = "floatTrackbarControlRoughness";
			this.floatTrackbarControlRoughness.RangeMax = 1F;
			this.floatTrackbarControlRoughness.RangeMin = 0F;
			this.floatTrackbarControlRoughness.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlRoughness.TabIndex = 1;
			this.floatTrackbarControlRoughness.Value = 0F;
			this.floatTrackbarControlRoughness.VisibleRangeMax = 1F;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 56);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "View Angle";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 30);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(101, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Surface Roughness";
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1024, 640);
			this.panelOutput.TabIndex = 0;
			this.panelOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseDown);
			this.panelOutput.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseMove);
			this.panelOutput.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseUp);
			// 
			// checkBoxShowDiff
			// 
			this.checkBoxShowDiff.AutoSize = true;
			this.checkBoxShowDiff.Location = new System.Drawing.Point(1246, 302);
			this.checkBoxShowDiff.Name = "checkBoxShowDiff";
			this.checkBoxShowDiff.Size = new System.Drawing.Size(138, 17);
			this.checkBoxShowDiff.TabIndex = 6;
			this.checkBoxShowDiff.Text = "Show Log10 Difference";
			this.checkBoxShowDiff.UseVisualStyleBackColor = true;
			// 
			// panel1
			// 
			this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
			this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Location = new System.Drawing.Point(1180, 324);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(186, 12);
			this.panel1.TabIndex = 9;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(1139, 322);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(34, 13);
			this.label5.TabIndex = 10;
			this.label5.Text = "1e-10";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(1372, 323);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(13, 13);
			this.label6.TabIndex = 10;
			this.label6.Text = "1";
			// 
			// AreaLightForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1392, 665);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.panelVisualizeLTCTransform);
			this.Controls.Add(this.checkBoxDebugMatrix);
			this.Controls.Add(this.checkBoxShowDiff);
			this.Controls.Add(this.checkBoxShowReference);
			this.Controls.Add(this.textBoxResults);
			this.Controls.Add(this.radioButtonNegativeFreeTarget);
			this.Controls.Add(this.radioButtonHorizontalTarget);
			this.Controls.Add(this.radioButtonFreeTarget);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label17);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.floatTrackbarControlLightPosZ);
			this.Controls.Add(this.floatTrackbarControlLightPosY);
			this.Controls.Add(this.floatTrackbarControlLightRoll);
			this.Controls.Add(this.floatTrackbarControlLightScaleY);
			this.Controls.Add(this.floatTrackbarControlLightScaleX);
			this.Controls.Add(this.floatTrackbarControlLightPosX);
			this.Controls.Add(this.floatTrackbarControlLuminance);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "AreaLightForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Area Light Test";
			this.panelVisualizeLTCTransform.ResumeLayout(false);
			this.panelVisualizeLTCTransform.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelOutput;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLuminance;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightPosX;
		private System.Windows.Forms.Label label7;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightPosY;
		private System.Windows.Forms.Label label8;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightPosZ;
		private System.Windows.Forms.Label label9;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightRoll;
		private System.Windows.Forms.Label label13;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightScaleX;
		private System.Windows.Forms.Label label17;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightScaleY;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RadioButton radioButtonFreeTarget;
		private System.Windows.Forms.RadioButton radioButtonHorizontalTarget;
		private System.Windows.Forms.RadioButton radioButtonNegativeFreeTarget;
		private System.Windows.Forms.TextBox textBoxResults;
		private System.Windows.Forms.CheckBox checkBoxShowReference;
		private System.Windows.Forms.CheckBox checkBoxDebugMatrix;
		private System.Windows.Forms.Panel panelVisualizeLTCTransform;
		private System.Windows.Forms.RadioButton radioButtonGGX;
		private System.Windows.Forms.RadioButton radioButtonOrenNayar;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlViewAngle;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlRoughness;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.CheckBox checkBoxShowDiff;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
	}
}

