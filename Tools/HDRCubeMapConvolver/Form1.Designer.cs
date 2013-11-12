namespace TestGradientPNG
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
			this.panelGraph = new TestGradientPNG.OutputPanel(this.components);
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
			this.SuspendLayout();
			// 
			// floatTrackbarControlScaleX
			// 
			this.floatTrackbarControlScaleX.Location = new System.Drawing.Point(12, 418);
			this.floatTrackbarControlScaleX.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlScaleX.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlScaleX.Name = "floatTrackbarControlScaleX";
			this.floatTrackbarControlScaleX.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlScaleX.TabIndex = 1;
			this.floatTrackbarControlScaleX.Value = 1F;
			this.floatTrackbarControlScaleX.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScaleX_ValueChanged);
			// 
			// floatTrackbarControlScaleY
			// 
			this.floatTrackbarControlScaleY.Location = new System.Drawing.Point(12, 444);
			this.floatTrackbarControlScaleY.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlScaleY.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlScaleY.Name = "floatTrackbarControlScaleY";
			this.floatTrackbarControlScaleY.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlScaleY.TabIndex = 1;
			this.floatTrackbarControlScaleY.Value = 1F;
			this.floatTrackbarControlScaleY.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScaleY_ValueChanged);
			// 
			// floatTrackbarControlWhitePoint
			// 
			this.floatTrackbarControlWhitePoint.Location = new System.Drawing.Point(450, 12);
			this.floatTrackbarControlWhitePoint.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlWhitePoint.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlWhitePoint.Name = "floatTrackbarControlWhitePoint";
			this.floatTrackbarControlWhitePoint.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlWhitePoint.TabIndex = 1;
			this.floatTrackbarControlWhitePoint.Value = 10F;
			this.floatTrackbarControlWhitePoint.VisibleRangeMax = 20F;
			this.floatTrackbarControlWhitePoint.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlWhitePoint_ValueChanged);
			// 
			// panelGraph
			// 
			this.panelGraph.Location = new System.Drawing.Point(12, 12);
			this.panelGraph.Name = "panelGraph";
			this.panelGraph.ScaleX = 1F;
			this.panelGraph.ScaleY = 1F;
			this.panelGraph.Size = new System.Drawing.Size(400, 400);
			this.panelGraph.TabIndex = 0;
			this.panelGraph.WhitePoint = 10F;
			// 
			// floatTrackbarControlA
			// 
			this.floatTrackbarControlA.Location = new System.Drawing.Point(450, 89);
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
			this.floatTrackbarControlB.Location = new System.Drawing.Point(450, 115);
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
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(426, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(18, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "W";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(426, 92);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(14, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "A";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(426, 118);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(14, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "B";
			// 
			// floatTrackbarControlC
			// 
			this.floatTrackbarControlC.Location = new System.Drawing.Point(450, 141);
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
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(426, 144);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(14, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "C";
			// 
			// floatTrackbarControlD
			// 
			this.floatTrackbarControlD.Location = new System.Drawing.Point(450, 167);
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
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(426, 170);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(15, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "D";
			// 
			// floatTrackbarControlE
			// 
			this.floatTrackbarControlE.Location = new System.Drawing.Point(450, 193);
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
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(426, 196);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(14, 13);
			this.label6.TabIndex = 2;
			this.label6.Text = "E";
			// 
			// floatTrackbarControlF
			// 
			this.floatTrackbarControlF.Location = new System.Drawing.Point(450, 219);
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
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(426, 222);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(13, 13);
			this.label7.TabIndex = 2;
			this.label7.Text = "F";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(771, 638);
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
	}
}

