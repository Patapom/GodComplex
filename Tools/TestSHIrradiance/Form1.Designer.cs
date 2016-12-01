namespace TestSHIrradiance
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
			this.floatTrackbarControlThetaMax = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxResults = new System.Windows.Forms.TextBox();
			this.graphPanel = new TestSHIrradiance.GraphPanel(this.components);
			this.SuspendLayout();
			// 
			// floatTrackbarControlThetaMax
			// 
			this.floatTrackbarControlThetaMax.Location = new System.Drawing.Point(103, 569);
			this.floatTrackbarControlThetaMax.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlThetaMax.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlThetaMax.Name = "floatTrackbarControlThetaMax";
			this.floatTrackbarControlThetaMax.RangeMax = 90F;
			this.floatTrackbarControlThetaMax.RangeMin = 0F;
			this.floatTrackbarControlThetaMax.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlThetaMax.TabIndex = 0;
			this.floatTrackbarControlThetaMax.Value = 90F;
			this.floatTrackbarControlThetaMax.VisibleRangeMax = 90F;
			this.floatTrackbarControlThetaMax.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlThetaMax_ValueChanged);
			this.floatTrackbarControlThetaMax.SliderDragStop += new Nuaj.Cirrus.Utility.FloatTrackbarControl.SliderDragStopEventHandler(this.floatTrackbarControlThetaMax_SliderDragStop);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 573);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(85, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Cone Max Angle";
			// 
			// textBoxResults
			// 
			this.textBoxResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxResults.Location = new System.Drawing.Point(818, 12);
			this.textBoxResults.Multiline = true;
			this.textBoxResults.Name = "textBoxResults";
			this.textBoxResults.ReadOnly = true;
			this.textBoxResults.Size = new System.Drawing.Size(283, 550);
			this.textBoxResults.TabIndex = 3;
			// 
			// graphPanel
			// 
			this.graphPanel.Bitmap = null;
			this.graphPanel.Location = new System.Drawing.Point(12, 12);
			this.graphPanel.MessageOnEmpty = "Bisou";
			this.graphPanel.Name = "graphPanel";
			this.graphPanel.Size = new System.Drawing.Size(800, 550);
			this.graphPanel.TabIndex = 1;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1113, 687);
			this.Controls.Add(this.textBoxResults);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.graphPanel);
			this.Controls.Add(this.floatTrackbarControlThetaMax);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Test SH Irradiance Estimate Coefficients with AO factor";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlThetaMax;
		private GraphPanel graphPanel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxResults;
	}
}

