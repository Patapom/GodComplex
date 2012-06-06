namespace TestSPH
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
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlRepulsionForce = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlRepulsionCoefficient = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.timerSimulation = new System.Windows.Forms.Timer(this.components);
			this.floatTrackbarControlSizeFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.panelOutput = new TestSPH.OutputPanel();
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
			this.panel1.Controls.Add(this.label4);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.floatTrackbarControlRepulsionForce);
			this.panel1.Controls.Add(this.floatTrackbarControlSizeFactor);
			this.panel1.Controls.Add(this.floatTrackbarControlRepulsionCoefficient);
			this.panel1.Controls.Add(this.floatTrackbarControlAttractionFactor);
			this.panel1.Location = new System.Drawing.Point(6, 422);
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
			this.floatTrackbarControlRepulsionForce.Value = 100F;
			this.floatTrackbarControlRepulsionForce.VisibleRangeMax = 200F;
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
			// floatTrackbarControlSizeFactor
			// 
			this.floatTrackbarControlSizeFactor.Location = new System.Drawing.Point(116, 126);
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
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(3, 129);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "Size Factor";
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(400, 400);
			this.panelOutput.TabIndex = 0;
			this.panelOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseDown);
			this.panelOutput.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseMove);
			this.panelOutput.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseUp);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(425, 589);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.panelOutput);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "SPH Test";
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
	}
}

