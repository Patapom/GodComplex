namespace TestFerrofluids
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
			this.floatTrackbarControlAttractionFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.panel1 = new System.Windows.Forms.Panel();
			this.buttonReset = new System.Windows.Forms.Button();
			this.checkBoxSimulate = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlRepulsionForce = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlDistThresholdSelf = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlDistThresholdMag = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlSizeFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlRepulsionCoefficient = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.timerSimulation = new System.Windows.Forms.Timer(this.components);
			this.floatTrackbarControlDeltaTime = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label8 = new System.Windows.Forms.Label();
			this.floatTrackbarControlSimulationSize = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.panelOutput = new TestFerrofluids.OutputPanel();
			this.buttonResetSliders = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// floatTrackbarControlAttractionFactor
			// 
			this.floatTrackbarControlAttractionFactor.Location = new System.Drawing.Point(116, 37);
			this.floatTrackbarControlAttractionFactor.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlAttractionFactor.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlAttractionFactor.Name = "floatTrackbarControlAttractionFactor";
			this.floatTrackbarControlAttractionFactor.RangeMax = 10000F;
			this.floatTrackbarControlAttractionFactor.RangeMin = 0F;
			this.floatTrackbarControlAttractionFactor.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlAttractionFactor.TabIndex = 1;
			this.floatTrackbarControlAttractionFactor.Value = 100F;
			this.floatTrackbarControlAttractionFactor.VisibleRangeMax = 200F;
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.panel1.Controls.Add(this.buttonReset);
			this.panel1.Controls.Add(this.checkBoxSimulate);
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.label7);
			this.panel1.Controls.Add(this.label6);
			this.panel1.Controls.Add(this.label5);
			this.panel1.Controls.Add(this.label4);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.label8);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.floatTrackbarControlRepulsionForce);
			this.panel1.Controls.Add(this.floatTrackbarControlDistThresholdSelf);
			this.panel1.Controls.Add(this.floatTrackbarControlDistThresholdMag);
			this.panel1.Controls.Add(this.floatTrackbarControlSizeFactor);
			this.panel1.Controls.Add(this.floatTrackbarControlRepulsionCoefficient);
			this.panel1.Controls.Add(this.floatTrackbarControlDeltaTime);
			this.panel1.Controls.Add(this.floatTrackbarControlAttractionFactor);
			this.panel1.Location = new System.Drawing.Point(6, 446);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(412, 164);
			this.panel1.TabIndex = 2;
			// 
			// buttonReset
			// 
			this.buttonReset.Location = new System.Drawing.Point(75, 8);
			this.buttonReset.Name = "buttonReset";
			this.buttonReset.Size = new System.Drawing.Size(75, 23);
			this.buttonReset.TabIndex = 4;
			this.buttonReset.Text = "Reset";
			this.buttonReset.UseVisualStyleBackColor = true;
			this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
			// 
			// checkBoxSimulate
			// 
			this.checkBoxSimulate.AutoSize = true;
			this.checkBoxSimulate.Checked = true;
			this.checkBoxSimulate.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxSimulate.Location = new System.Drawing.Point(3, 12);
			this.checkBoxSimulate.Name = "checkBoxSimulate";
			this.checkBoxSimulate.Size = new System.Drawing.Size(66, 17);
			this.checkBoxSimulate.TabIndex = 3;
			this.checkBoxSimulate.Text = "Simulate";
			this.checkBoxSimulate.UseVisualStyleBackColor = true;
			this.checkBoxSimulate.CheckedChanged += new System.EventHandler(this.checkBoxSimulate_CheckedChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 63);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(84, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Repulsion Force";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(244, 145);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(25, 13);
			this.label7.TabIndex = 2;
			this.label7.Text = "Self";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(113, 145);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(31, 13);
			this.label6.TabIndex = 2;
			this.label6.Text = "Mag.";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(3, 145);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(104, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "Distance Thresholds";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(3, 108);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "Size Factor";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 85);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(87, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Repulsion Power";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 41);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(82, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Attraction Force";
			// 
			// floatTrackbarControlRepulsionForce
			// 
			this.floatTrackbarControlRepulsionForce.Location = new System.Drawing.Point(116, 59);
			this.floatTrackbarControlRepulsionForce.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlRepulsionForce.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlRepulsionForce.Name = "floatTrackbarControlRepulsionForce";
			this.floatTrackbarControlRepulsionForce.RangeMax = 10000F;
			this.floatTrackbarControlRepulsionForce.RangeMin = 0F;
			this.floatTrackbarControlRepulsionForce.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlRepulsionForce.TabIndex = 1;
			this.floatTrackbarControlRepulsionForce.Value = 0.4F;
			this.floatTrackbarControlRepulsionForce.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlDistThresholdSelf
			// 
			this.floatTrackbarControlDistThresholdSelf.Location = new System.Drawing.Point(275, 141);
			this.floatTrackbarControlDistThresholdSelf.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDistThresholdSelf.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDistThresholdSelf.Name = "floatTrackbarControlDistThresholdSelf";
			this.floatTrackbarControlDistThresholdSelf.RangeMax = 1000F;
			this.floatTrackbarControlDistThresholdSelf.RangeMin = 0F;
			this.floatTrackbarControlDistThresholdSelf.Size = new System.Drawing.Size(92, 20);
			this.floatTrackbarControlDistThresholdSelf.TabIndex = 1;
			this.floatTrackbarControlDistThresholdSelf.Value = 1F;
			this.floatTrackbarControlDistThresholdSelf.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlDistThresholdMag
			// 
			this.floatTrackbarControlDistThresholdMag.Location = new System.Drawing.Point(144, 141);
			this.floatTrackbarControlDistThresholdMag.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDistThresholdMag.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDistThresholdMag.Name = "floatTrackbarControlDistThresholdMag";
			this.floatTrackbarControlDistThresholdMag.RangeMax = 1000F;
			this.floatTrackbarControlDistThresholdMag.RangeMin = 0F;
			this.floatTrackbarControlDistThresholdMag.Size = new System.Drawing.Size(92, 20);
			this.floatTrackbarControlDistThresholdMag.TabIndex = 1;
			this.floatTrackbarControlDistThresholdMag.Value = 0.5F;
			this.floatTrackbarControlDistThresholdMag.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlSizeFactor
			// 
			this.floatTrackbarControlSizeFactor.Location = new System.Drawing.Point(116, 105);
			this.floatTrackbarControlSizeFactor.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSizeFactor.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSizeFactor.Name = "floatTrackbarControlSizeFactor";
			this.floatTrackbarControlSizeFactor.RangeMax = 1000F;
			this.floatTrackbarControlSizeFactor.RangeMin = 0F;
			this.floatTrackbarControlSizeFactor.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlSizeFactor.TabIndex = 1;
			this.floatTrackbarControlSizeFactor.Value = 100F;
			this.floatTrackbarControlSizeFactor.VisibleRangeMax = 200F;
			// 
			// floatTrackbarControlRepulsionCoefficient
			// 
			this.floatTrackbarControlRepulsionCoefficient.Location = new System.Drawing.Point(116, 82);
			this.floatTrackbarControlRepulsionCoefficient.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlRepulsionCoefficient.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlRepulsionCoefficient.Name = "floatTrackbarControlRepulsionCoefficient";
			this.floatTrackbarControlRepulsionCoefficient.RangeMax = 100F;
			this.floatTrackbarControlRepulsionCoefficient.RangeMin = 0F;
			this.floatTrackbarControlRepulsionCoefficient.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlRepulsionCoefficient.TabIndex = 1;
			this.floatTrackbarControlRepulsionCoefficient.Value = 3F;
			// 
			// timerSimulation
			// 
			this.timerSimulation.Enabled = true;
			this.timerSimulation.Interval = 10;
			// 
			// floatTrackbarControlDeltaTime
			// 
			this.floatTrackbarControlDeltaTime.Location = new System.Drawing.Point(235, 10);
			this.floatTrackbarControlDeltaTime.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDeltaTime.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDeltaTime.Name = "floatTrackbarControlDeltaTime";
			this.floatTrackbarControlDeltaTime.RangeMax = 1000F;
			this.floatTrackbarControlDeltaTime.RangeMin = 0F;
			this.floatTrackbarControlDeltaTime.Size = new System.Drawing.Size(173, 20);
			this.floatTrackbarControlDeltaTime.TabIndex = 1;
			this.floatTrackbarControlDeltaTime.Value = 10F;
			this.floatTrackbarControlDeltaTime.VisibleRangeMax = 20F;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(156, 13);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(77, 13);
			this.label8.TabIndex = 2;
			this.label8.Text = "DeltaTime (ms)";
			// 
			// floatTrackbarControlSimulationSize
			// 
			this.floatTrackbarControlSimulationSize.Location = new System.Drawing.Point(12, 12);
			this.floatTrackbarControlSimulationSize.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSimulationSize.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSimulationSize.Name = "floatTrackbarControlSimulationSize";
			this.floatTrackbarControlSimulationSize.RangeMax = 10000F;
			this.floatTrackbarControlSimulationSize.RangeMin = 0F;
			this.floatTrackbarControlSimulationSize.Size = new System.Drawing.Size(319, 20);
			this.floatTrackbarControlSimulationSize.TabIndex = 1;
			this.floatTrackbarControlSimulationSize.Value = 100F;
			this.floatTrackbarControlSimulationSize.VisibleRangeMax = 500F;
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 39);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(400, 400);
			this.panelOutput.TabIndex = 0;
			this.panelOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseDown);
			this.panelOutput.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseMove);
			this.panelOutput.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseUp);
			// 
			// buttonResetSliders
			// 
			this.buttonResetSliders.BackColor = System.Drawing.Color.LightCoral;
			this.buttonResetSliders.Location = new System.Drawing.Point(338, 10);
			this.buttonResetSliders.Name = "buttonResetSliders";
			this.buttonResetSliders.Size = new System.Drawing.Size(75, 23);
			this.buttonResetSliders.TabIndex = 4;
			this.buttonResetSliders.Text = "Reset";
			this.buttonResetSliders.UseVisualStyleBackColor = false;
			this.buttonResetSliders.Click += new System.EventHandler(this.buttonResetSliders_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(425, 613);
			this.Controls.Add(this.buttonResetSliders);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.panelOutput);
			this.Controls.Add(this.floatTrackbarControlSimulationSize);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Ferrofluid Spikes Placement Test";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private OutputPanel panelOutput;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlAttractionFactor;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlRepulsionCoefficient;
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlRepulsionForce;
		private System.Windows.Forms.CheckBox checkBoxSimulate;
		private System.Windows.Forms.Button buttonReset;
		private System.Windows.Forms.Timer timerSimulation;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSizeFactor;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDistThresholdSelf;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDistThresholdMag;
		private System.Windows.Forms.Label label8;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDeltaTime;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSimulationSize;
		private System.Windows.Forms.Button buttonResetSliders;
	}
}

