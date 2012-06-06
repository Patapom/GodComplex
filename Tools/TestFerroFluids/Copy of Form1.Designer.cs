namespace TestFerrofluids
{
	partial class OldForm1
	{
// 		/// <summary>
// 		/// Required designer variable.
// 		/// </summary>
// 		private System.ComponentModel.IContainer components = null;
// 
// 		/// <summary>
// 		/// Clean up any resources being used.
// 		/// </summary>
// 		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
// 		protected override void Dispose( bool disposing )
// 		{
// 			if ( disposing && (components != null) )
// 			{
// 				components.Dispose();
// 			}
// 			base.Dispose( disposing );
// 		}
// 
// 		#region Windows Form Designer generated code
// 
// 		/// <summary>
// 		/// Required method for Designer support - do not modify
// 		/// the contents of this method with the code editor.
// 		/// </summary>
// 		private void InitializeComponent()
// 		{
// 			this.floatTrackbarControlCloudExtinction = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
// 			this.panel1 = new System.Windows.Forms.Panel();
// 			this.checkBoxSimulate = new System.Windows.Forms.CheckBox();
// 			this.label3 = new System.Windows.Forms.Label();
// 			this.label2 = new System.Windows.Forms.Label();
// 			this.label1 = new System.Windows.Forms.Label();
// 			this.floatTrackbarControlStepSize = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
// 			this.floatTrackbarControlOpacityCoefficient = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
// 			this.panelOutput = new TestFerrofluids.OutputPanel();
// 			this.buttonReset = new System.Windows.Forms.Button();
// 			this.panel1.SuspendLayout();
// 			this.SuspendLayout();
// 			// 
// 			// floatTrackbarControlCloudExtinction
// 			// 
// 			this.floatTrackbarControlCloudExtinction.Location = new System.Drawing.Point(127, 3);
// 			this.floatTrackbarControlCloudExtinction.MaximumSize = new System.Drawing.Size(10000, 20);
// 			this.floatTrackbarControlCloudExtinction.MinimumSize = new System.Drawing.Size(70, 20);
// 			this.floatTrackbarControlCloudExtinction.Name = "floatTrackbarControlCloudExtinction";
// 			this.floatTrackbarControlCloudExtinction.RangeMax = 10F;
// 			this.floatTrackbarControlCloudExtinction.RangeMin = 0F;
// 			this.floatTrackbarControlCloudExtinction.Size = new System.Drawing.Size(200, 20);
// 			this.floatTrackbarControlCloudExtinction.TabIndex = 1;
// 			this.floatTrackbarControlCloudExtinction.Value = 0.1F;
// 			this.floatTrackbarControlCloudExtinction.VisibleRangeMax = 1F;
// 			this.floatTrackbarControlCloudExtinction.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlCloudExtinction_ValueChanged);
// 			// 
// 			// panel1
// 			// 
// 			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
// 			this.panel1.Controls.Add(this.buttonReset);
// 			this.panel1.Controls.Add(this.checkBoxSimulate);
// 			this.panel1.Controls.Add(this.label3);
// 			this.panel1.Controls.Add(this.label2);
// 			this.panel1.Controls.Add(this.label1);
// 			this.panel1.Controls.Add(this.floatTrackbarControlStepSize);
// 			this.panel1.Controls.Add(this.floatTrackbarControlOpacityCoefficient);
// 			this.panel1.Controls.Add(this.floatTrackbarControlCloudExtinction);
// 			this.panel1.Location = new System.Drawing.Point(0, 470);
// 			this.panel1.Name = "panel1";
// 			this.panel1.Size = new System.Drawing.Size(712, 164);
// 			this.panel1.TabIndex = 2;
// 			// 
// 			// checkBoxSimulate
// 			// 
// 			this.checkBoxSimulate.AutoSize = true;
// 			this.checkBoxSimulate.Checked = true;
// 			this.checkBoxSimulate.CheckState = System.Windows.Forms.CheckState.Checked;
// 			this.checkBoxSimulate.Location = new System.Drawing.Point(388, 7);
// 			this.checkBoxSimulate.Name = "checkBoxSimulate";
// 			this.checkBoxSimulate.Size = new System.Drawing.Size(66, 17);
// 			this.checkBoxSimulate.TabIndex = 3;
// 			this.checkBoxSimulate.Text = "Simulate";
// 			this.checkBoxSimulate.UseVisualStyleBackColor = true;
// 			// 
// 			// label3
// 			// 
// 			this.label3.AutoSize = true;
// 			this.label3.Location = new System.Drawing.Point(3, 58);
// 			this.label3.Name = "label3";
// 			this.label3.Size = new System.Drawing.Size(87, 13);
// 			this.label3.TabIndex = 2;
// 			this.label3.Text = "Step Size (metre)";
// 			// 
// 			// label2
// 			// 
// 			this.label2.AutoSize = true;
// 			this.label2.Location = new System.Drawing.Point(3, 32);
// 			this.label2.Name = "label2";
// 			this.label2.Size = new System.Drawing.Size(96, 13);
// 			this.label2.TabIndex = 2;
// 			this.label2.Text = "Opacity Coefficient";
// 			// 
// 			// label1
// 			// 
// 			this.label1.AutoSize = true;
// 			this.label1.Location = new System.Drawing.Point(3, 7);
// 			this.label1.Name = "label1";
// 			this.label1.Size = new System.Drawing.Size(83, 13);
// 			this.label1.TabIndex = 2;
// 			this.label1.Text = "Cloud Extinction";
// 			// 
// 			// floatTrackbarControlStepSize
// 			// 
// 			this.floatTrackbarControlStepSize.Location = new System.Drawing.Point(127, 55);
// 			this.floatTrackbarControlStepSize.MaximumSize = new System.Drawing.Size(10000, 20);
// 			this.floatTrackbarControlStepSize.MinimumSize = new System.Drawing.Size(70, 20);
// 			this.floatTrackbarControlStepSize.Name = "floatTrackbarControlStepSize";
// 			this.floatTrackbarControlStepSize.RangeMax = 10000F;
// 			this.floatTrackbarControlStepSize.RangeMin = 0F;
// 			this.floatTrackbarControlStepSize.Size = new System.Drawing.Size(200, 20);
// 			this.floatTrackbarControlStepSize.TabIndex = 1;
// 			this.floatTrackbarControlStepSize.Value = 10F;
// 			this.floatTrackbarControlStepSize.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlStepSize_ValueChanged);
// 			// 
// 			// floatTrackbarControlOpacityCoefficient
// 			// 
// 			this.floatTrackbarControlOpacityCoefficient.Location = new System.Drawing.Point(127, 29);
// 			this.floatTrackbarControlOpacityCoefficient.MaximumSize = new System.Drawing.Size(10000, 20);
// 			this.floatTrackbarControlOpacityCoefficient.MinimumSize = new System.Drawing.Size(70, 20);
// 			this.floatTrackbarControlOpacityCoefficient.Name = "floatTrackbarControlOpacityCoefficient";
// 			this.floatTrackbarControlOpacityCoefficient.RangeMax = 10F;
// 			this.floatTrackbarControlOpacityCoefficient.RangeMin = 0F;
// 			this.floatTrackbarControlOpacityCoefficient.Size = new System.Drawing.Size(200, 20);
// 			this.floatTrackbarControlOpacityCoefficient.TabIndex = 1;
// 			this.floatTrackbarControlOpacityCoefficient.Value = 1F;
// 			this.floatTrackbarControlOpacityCoefficient.VisibleRangeMax = 1F;
// 			this.floatTrackbarControlOpacityCoefficient.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlOpacityCoefficient_ValueChanged);
// 			// 
// 			// panelOutput
// 			// 
// 			this.panelOutput.Location = new System.Drawing.Point(12, 12);
// 			this.panelOutput.Name = "panelOutput";
// 			this.panelOutput.Size = new System.Drawing.Size(700, 438);
// 			this.panelOutput.TabIndex = 0;
// 			this.panelOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseDown);
// 			// 
// 			// buttonReset
// 			// 
// 			this.buttonReset.Location = new System.Drawing.Point(460, 3);
// 			this.buttonReset.Name = "buttonReset";
// 			this.buttonReset.Size = new System.Drawing.Size(75, 23);
// 			this.buttonReset.TabIndex = 4;
// 			this.buttonReset.Text = "Reset";
// 			this.buttonReset.UseVisualStyleBackColor = true;
// 			this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
// 			// 
// 			// OldForm1
// 			// 
// 			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
// 			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
// 			this.ClientSize = new System.Drawing.Size(723, 635);
// 			this.Controls.Add(this.panel1);
// 			this.Controls.Add(this.panelOutput);
// 			this.Name = "OldForm1";
// 			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
// 			this.Text = "SPH Test";
// 			this.panel1.ResumeLayout(false);
// 			this.panel1.PerformLayout();
// 			this.ResumeLayout(false);
// 
// 		}
// 
// 		#endregion
// 
// 		private OutputPanel panelOutput;
// 		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlCloudExtinction;
// 		private System.Windows.Forms.Panel panel1;
// 		private System.Windows.Forms.Label label2;
// 		private System.Windows.Forms.Label label1;
// 		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlOpacityCoefficient;
// 		private System.Windows.Forms.Label label3;
// 		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlStepSize;
// 		private System.Windows.Forms.CheckBox checkBoxSimulate;
// 		private System.Windows.Forms.Button buttonReset;
 	}
}

