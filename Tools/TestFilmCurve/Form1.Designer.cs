namespace TestFilmicCurve
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
			this.floatTrackbarControlScaleX = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlScaleY = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlWhitePoint = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlA = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlB = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlC = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.floatTrackbarControlD = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.floatTrackbarControlE = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label6 = new System.Windows.Forms.Label();
			this.floatTrackbarControlF = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label7 = new System.Windows.Forms.Label();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.panelOutput = new TestFilmicCurve.PanelOutput3D(this.components);
			this.outputPanelHammersley1 = new TestFilmicCurve.OutputPanelHammersley(this.components);
			this.panelGraph = new TestFilmicCurve.OutputPanel(this.components);
			this.buttonReload = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// floatTrackbarControlScaleX
			// 
			this.floatTrackbarControlScaleX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlScaleX.Location = new System.Drawing.Point(977, 250);
			this.floatTrackbarControlScaleX.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlScaleX.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlScaleX.Name = "floatTrackbarControlScaleX";
			this.floatTrackbarControlScaleX.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlScaleX.TabIndex = 1;
			this.floatTrackbarControlScaleX.Value = 10F;
			this.floatTrackbarControlScaleX.VisibleRangeMax = 20F;
			this.floatTrackbarControlScaleX.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScaleX_ValueChanged);
			// 
			// floatTrackbarControlScaleY
			// 
			this.floatTrackbarControlScaleY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlScaleY.Location = new System.Drawing.Point(977, 276);
			this.floatTrackbarControlScaleY.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlScaleY.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlScaleY.Name = "floatTrackbarControlScaleY";
			this.floatTrackbarControlScaleY.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlScaleY.TabIndex = 1;
			this.floatTrackbarControlScaleY.Value = 1F;
			this.floatTrackbarControlScaleY.VisibleRangeMax = 5F;
			this.floatTrackbarControlScaleY.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScaleY_ValueChanged);
			// 
			// floatTrackbarControlWhitePoint
			// 
			this.floatTrackbarControlWhitePoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlWhitePoint.Location = new System.Drawing.Point(729, 12);
			this.floatTrackbarControlWhitePoint.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlWhitePoint.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlWhitePoint.Name = "floatTrackbarControlWhitePoint";
			this.floatTrackbarControlWhitePoint.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlWhitePoint.TabIndex = 1;
			this.floatTrackbarControlWhitePoint.Value = 10F;
			this.floatTrackbarControlWhitePoint.VisibleRangeMax = 20F;
			this.floatTrackbarControlWhitePoint.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlWhitePoint_ValueChanged);
			// 
			// floatTrackbarControlA
			// 
			this.floatTrackbarControlA.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlA.Location = new System.Drawing.Point(729, 89);
			this.floatTrackbarControlA.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlA.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlA.Name = "floatTrackbarControlA";
			this.floatTrackbarControlA.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlA.TabIndex = 1;
			this.floatTrackbarControlA.Value = 0.15F;
			this.floatTrackbarControlA.VisibleRangeMax = 1F;
			this.floatTrackbarControlA.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlA_ValueChanged);
			// 
			// floatTrackbarControlB
			// 
			this.floatTrackbarControlB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlB.Location = new System.Drawing.Point(729, 115);
			this.floatTrackbarControlB.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlB.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlB.Name = "floatTrackbarControlB";
			this.floatTrackbarControlB.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlB.TabIndex = 1;
			this.floatTrackbarControlB.Value = 0.5F;
			this.floatTrackbarControlB.VisibleRangeMax = 1F;
			this.floatTrackbarControlB.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlB_ValueChanged);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(705, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(18, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "W";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(705, 92);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(14, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "A";
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(705, 118);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(14, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "B";
			// 
			// floatTrackbarControlC
			// 
			this.floatTrackbarControlC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlC.Location = new System.Drawing.Point(729, 141);
			this.floatTrackbarControlC.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlC.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlC.Name = "floatTrackbarControlC";
			this.floatTrackbarControlC.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlC.TabIndex = 1;
			this.floatTrackbarControlC.Value = 0.1F;
			this.floatTrackbarControlC.VisibleRangeMax = 1F;
			this.floatTrackbarControlC.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlC_ValueChanged);
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(705, 144);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(14, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "C";
			// 
			// floatTrackbarControlD
			// 
			this.floatTrackbarControlD.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlD.Location = new System.Drawing.Point(729, 167);
			this.floatTrackbarControlD.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlD.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlD.Name = "floatTrackbarControlD";
			this.floatTrackbarControlD.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlD.TabIndex = 1;
			this.floatTrackbarControlD.Value = 0.2F;
			this.floatTrackbarControlD.VisibleRangeMax = 1F;
			this.floatTrackbarControlD.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlD_ValueChanged);
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(705, 170);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(15, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "D";
			// 
			// floatTrackbarControlE
			// 
			this.floatTrackbarControlE.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlE.Location = new System.Drawing.Point(729, 193);
			this.floatTrackbarControlE.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlE.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlE.Name = "floatTrackbarControlE";
			this.floatTrackbarControlE.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlE.TabIndex = 1;
			this.floatTrackbarControlE.Value = 0.02F;
			this.floatTrackbarControlE.VisibleRangeMax = 1F;
			this.floatTrackbarControlE.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlE_ValueChanged);
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(705, 196);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(14, 13);
			this.label6.TabIndex = 2;
			this.label6.Text = "E";
			// 
			// floatTrackbarControlF
			// 
			this.floatTrackbarControlF.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlF.Location = new System.Drawing.Point(729, 219);
			this.floatTrackbarControlF.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlF.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlF.Name = "floatTrackbarControlF";
			this.floatTrackbarControlF.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlF.TabIndex = 1;
			this.floatTrackbarControlF.Value = 0.3F;
			this.floatTrackbarControlF.VisibleRangeMax = 1F;
			this.floatTrackbarControlF.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlF_ValueChanged);
			// 
			// label7
			// 
			this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(705, 222);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(13, 13);
			this.label7.TabIndex = 2;
			this.label7.Text = "F";
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = 10;
			// 
			// panelOutput
			// 
			this.panelOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(687, 562);
			this.panelOutput.TabIndex = 4;
			// 
			// outputPanelHammersley1
			// 
			this.outputPanelHammersley1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.outputPanelHammersley1.Location = new System.Drawing.Point(977, 301);
			this.outputPanelHammersley1.Name = "outputPanelHammersley1";
			this.outputPanelHammersley1.Size = new System.Drawing.Size(275, 273);
			this.outputPanelHammersley1.TabIndex = 3;
			// 
			// panelGraph
			// 
			this.panelGraph.A = 0.15F;
			this.panelGraph.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.panelGraph.B = 0.5F;
			this.panelGraph.C = 0.1F;
			this.panelGraph.D = 0.2F;
			this.panelGraph.E = 0.02F;
			this.panelGraph.F = 0.3F;
			this.panelGraph.Location = new System.Drawing.Point(935, 12);
			this.panelGraph.Name = "panelGraph";
			this.panelGraph.ScaleX = 1F;
			this.panelGraph.ScaleY = 1F;
			this.panelGraph.Size = new System.Drawing.Size(317, 223);
			this.panelGraph.TabIndex = 0;
			this.panelGraph.WhitePoint = 10F;
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(708, 551);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 5;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1264, 586);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput);
			this.Controls.Add(this.outputPanelHammersley1);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlWhitePoint);
			this.Controls.Add(this.floatTrackbarControlF);
			this.Controls.Add(this.floatTrackbarControlE);
			this.Controls.Add(this.floatTrackbarControlD);
			this.Controls.Add(this.floatTrackbarControlC);
			this.Controls.Add(this.floatTrackbarControlB);
			this.Controls.Add(this.floatTrackbarControlScaleY);
			this.Controls.Add(this.floatTrackbarControlA);
			this.Controls.Add(this.floatTrackbarControlScaleX);
			this.Controls.Add(this.panelGraph);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OutputPanel panelGraph;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlScaleX;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlScaleY;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlWhitePoint;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlA;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlB;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlC;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlD;
		private System.Windows.Forms.Label label5;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlE;
		private System.Windows.Forms.Label label6;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlF;
		private System.Windows.Forms.Label label7;
		private OutputPanelHammersley outputPanelHammersley1;
		private System.Windows.Forms.Timer timer;
		private PanelOutput3D panelOutput;
		private System.Windows.Forms.Button buttonReload;
	}
}

