namespace LTCTableGenerator
{
	partial class FitterForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FitterForm));
			this.panelOutputSourceBRDF = new Nuaj.Cirrus.Utility.PanelOutput(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.panelOutputTargetBRDF = new Nuaj.Cirrus.Utility.PanelOutput(this.components);
			this.panelOutputDifference = new Nuaj.Cirrus.Utility.PanelOutput(this.components);
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.textBoxFitting = new System.Windows.Forms.TextBox();
			this.checkBoxPause = new System.Windows.Forms.CheckBox();
			this.integerTrackbarControlRoughnessIndex = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label8 = new System.Windows.Forms.Label();
			this.integerTrackbarControlThetaIndex = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label9 = new System.Windows.Forms.Label();
			this.checkBoxAutoRun = new System.Windows.Forms.CheckBox();
			this.checkBoxDoFitting = new System.Windows.Forms.CheckBox();
			this.integerTrackbarControlStepX = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.integerTrackbarControlStepY = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.SuspendLayout();
			// 
			// panelOutputSourceBRDF
			// 
			this.panelOutputSourceBRDF.Location = new System.Drawing.Point(12, 35);
			this.panelOutputSourceBRDF.Name = "panelOutputSourceBRDF";
			this.panelOutputSourceBRDF.PanelBitmap = ((System.Drawing.Bitmap)(resources.GetObject("panelOutputSourceBRDF.PanelBitmap")));
			this.panelOutputSourceBRDF.Size = new System.Drawing.Size(340, 340);
			this.panelOutputSourceBRDF.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 378);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(73, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Source BRDF";
			// 
			// panelOutputTargetBRDF
			// 
			this.panelOutputTargetBRDF.Location = new System.Drawing.Point(358, 35);
			this.panelOutputTargetBRDF.Name = "panelOutputTargetBRDF";
			this.panelOutputTargetBRDF.PanelBitmap = ((System.Drawing.Bitmap)(resources.GetObject("panelOutputTargetBRDF.PanelBitmap")));
			this.panelOutputTargetBRDF.Size = new System.Drawing.Size(340, 340);
			this.panelOutputTargetBRDF.TabIndex = 0;
			// 
			// panelOutputDifference
			// 
			this.panelOutputDifference.Location = new System.Drawing.Point(704, 35);
			this.panelOutputDifference.Name = "panelOutputDifference";
			this.panelOutputDifference.PanelBitmap = ((System.Drawing.Bitmap)(resources.GetObject("panelOutputDifference.PanelBitmap")));
			this.panelOutputDifference.Size = new System.Drawing.Size(340, 340);
			this.panelOutputDifference.TabIndex = 0;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(355, 378);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(78, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Mapped BRDF";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(701, 378);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(71, 13);
			this.label3.TabIndex = 1;
			this.label3.Text = "Relative Error";
			// 
			// panel1
			// 
			this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
			this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.panel1.Location = new System.Drawing.Point(91, 378);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(261, 13);
			this.panel1.TabIndex = 2;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(88, 394);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(28, 13);
			this.label4.TabIndex = 1;
			this.label4.Text = "1e-4";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(324, 394);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(31, 13);
			this.label5.TabIndex = 1;
			this.label5.Text = "1e+4";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(780, 394);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(28, 13);
			this.label6.TabIndex = 1;
			this.label6.Text = "1e-4";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(1016, 394);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(31, 13);
			this.label7.TabIndex = 1;
			this.label7.Text = "1e+4";
			// 
			// panel2
			// 
			this.panel2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel2.BackgroundImage")));
			this.panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.panel2.Location = new System.Drawing.Point(783, 378);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(261, 13);
			this.panel2.TabIndex = 2;
			// 
			// textBoxFitting
			// 
			this.textBoxFitting.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxFitting.Location = new System.Drawing.Point(1050, 35);
			this.textBoxFitting.Multiline = true;
			this.textBoxFitting.Name = "textBoxFitting";
			this.textBoxFitting.ReadOnly = true;
			this.textBoxFitting.Size = new System.Drawing.Size(177, 340);
			this.textBoxFitting.TabIndex = 3;
			// 
			// checkBoxPause
			// 
			this.checkBoxPause.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBoxPause.AutoSize = true;
			this.checkBoxPause.Location = new System.Drawing.Point(604, 6);
			this.checkBoxPause.Name = "checkBoxPause";
			this.checkBoxPause.Size = new System.Drawing.Size(53, 23);
			this.checkBoxPause.TabIndex = 4;
			this.checkBoxPause.Text = "PAUSE";
			this.checkBoxPause.UseVisualStyleBackColor = true;
			this.checkBoxPause.CheckedChanged += new System.EventHandler(this.checkBoxPause_CheckedChanged);
			// 
			// integerTrackbarControlRoughnessIndex
			// 
			this.integerTrackbarControlRoughnessIndex.Enabled = false;
			this.integerTrackbarControlRoughnessIndex.Location = new System.Drawing.Point(95, 9);
			this.integerTrackbarControlRoughnessIndex.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlRoughnessIndex.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlRoughnessIndex.Name = "integerTrackbarControlRoughnessIndex";
			this.integerTrackbarControlRoughnessIndex.RangeMax = 63;
			this.integerTrackbarControlRoughnessIndex.RangeMin = 0;
			this.integerTrackbarControlRoughnessIndex.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlRoughnessIndex.TabIndex = 5;
			this.integerTrackbarControlRoughnessIndex.Value = 63;
			this.integerTrackbarControlRoughnessIndex.VisibleRangeMax = 63;
			this.integerTrackbarControlRoughnessIndex.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlRoughnessIndex_ValueChanged);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(12, 11);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(77, 13);
			this.label8.TabIndex = 6;
			this.label8.Text = "Roughness (Y)";
			// 
			// integerTrackbarControlThetaIndex
			// 
			this.integerTrackbarControlThetaIndex.Enabled = false;
			this.integerTrackbarControlThetaIndex.Location = new System.Drawing.Point(386, 9);
			this.integerTrackbarControlThetaIndex.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlThetaIndex.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlThetaIndex.Name = "integerTrackbarControlThetaIndex";
			this.integerTrackbarControlThetaIndex.RangeMax = 63;
			this.integerTrackbarControlThetaIndex.RangeMin = 0;
			this.integerTrackbarControlThetaIndex.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlThetaIndex.TabIndex = 5;
			this.integerTrackbarControlThetaIndex.Value = 0;
			this.integerTrackbarControlThetaIndex.VisibleRangeMax = 63;
			this.integerTrackbarControlThetaIndex.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlThetaIndex_ValueChanged);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(303, 11);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(81, 13);
			this.label9.TabIndex = 6;
			this.label9.Text = "cos(ThetaV) (X)";
			// 
			// checkBoxAutoRun
			// 
			this.checkBoxAutoRun.AutoSize = true;
			this.checkBoxAutoRun.Checked = true;
			this.checkBoxAutoRun.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAutoRun.Location = new System.Drawing.Point(663, 10);
			this.checkBoxAutoRun.Name = "checkBoxAutoRun";
			this.checkBoxAutoRun.Size = new System.Drawing.Size(71, 17);
			this.checkBoxAutoRun.TabIndex = 7;
			this.checkBoxAutoRun.Text = "Auto-Run";
			this.checkBoxAutoRun.UseVisualStyleBackColor = true;
			// 
			// checkBoxDoFitting
			// 
			this.checkBoxDoFitting.AutoSize = true;
			this.checkBoxDoFitting.Checked = true;
			this.checkBoxDoFitting.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxDoFitting.Location = new System.Drawing.Point(737, 10);
			this.checkBoxDoFitting.Name = "checkBoxDoFitting";
			this.checkBoxDoFitting.Size = new System.Drawing.Size(71, 17);
			this.checkBoxDoFitting.TabIndex = 7;
			this.checkBoxDoFitting.Text = "Do Fitting";
			this.checkBoxDoFitting.UseVisualStyleBackColor = true;
			// 
			// integerTrackbarControlStepX
			// 
			this.integerTrackbarControlStepX.Location = new System.Drawing.Point(873, 9);
			this.integerTrackbarControlStepX.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlStepX.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlStepX.Name = "integerTrackbarControlStepX";
			this.integerTrackbarControlStepX.RangeMax = 64;
			this.integerTrackbarControlStepX.RangeMin = 1;
			this.integerTrackbarControlStepX.Size = new System.Drawing.Size(117, 20);
			this.integerTrackbarControlStepX.TabIndex = 8;
			this.integerTrackbarControlStepX.Value = 1;
			this.integerTrackbarControlStepX.VisibleRangeMax = 64;
			this.integerTrackbarControlStepX.VisibleRangeMin = 1;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(831, 11);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(36, 13);
			this.label10.TabIndex = 6;
			this.label10.Text = "StepX";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(999, 11);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(36, 13);
			this.label11.TabIndex = 6;
			this.label11.Text = "StepY";
			// 
			// integerTrackbarControlStepY
			// 
			this.integerTrackbarControlStepY.Location = new System.Drawing.Point(1041, 9);
			this.integerTrackbarControlStepY.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlStepY.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlStepY.Name = "integerTrackbarControlStepY";
			this.integerTrackbarControlStepY.RangeMax = 64;
			this.integerTrackbarControlStepY.RangeMin = 1;
			this.integerTrackbarControlStepY.Size = new System.Drawing.Size(117, 20);
			this.integerTrackbarControlStepY.TabIndex = 8;
			this.integerTrackbarControlStepY.Value = 1;
			this.integerTrackbarControlStepY.VisibleRangeMax = 64;
			this.integerTrackbarControlStepY.VisibleRangeMin = 1;
			// 
			// FitterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1239, 423);
			this.Controls.Add(this.integerTrackbarControlStepY);
			this.Controls.Add(this.integerTrackbarControlStepX);
			this.Controls.Add(this.checkBoxDoFitting);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.checkBoxAutoRun);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.integerTrackbarControlThetaIndex);
			this.Controls.Add(this.integerTrackbarControlRoughnessIndex);
			this.Controls.Add(this.checkBoxPause);
			this.Controls.Add(this.textBoxFitting);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.panelOutputDifference);
			this.Controls.Add(this.panelOutputTargetBRDF);
			this.Controls.Add(this.panelOutputSourceBRDF);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FitterForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Fitter Debugger";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Nuaj.Cirrus.Utility.PanelOutput panelOutputSourceBRDF;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.PanelOutput panelOutputTargetBRDF;
		private Nuaj.Cirrus.Utility.PanelOutput panelOutputDifference;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.TextBox textBoxFitting;
		private System.Windows.Forms.CheckBox checkBoxPause;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlRoughnessIndex;
		private System.Windows.Forms.Label label8;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlThetaIndex;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.CheckBox checkBoxAutoRun;
		private System.Windows.Forms.CheckBox checkBoxDoFitting;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlStepX;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlStepY;
	}
}