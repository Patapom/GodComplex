namespace TestPullPush
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
			this.panelInputImage = new TestPullPush.PanelOutput(this.components);
			this.panelOutputReconstruction = new TestPullPush.PanelOutput(this.components);
			this.panelPixelDensity = new TestPullPush.PanelOutput(this.components);
			this.panelSparseInputImage = new TestPullPush.PanelOutput(this.components);
			this.floatTrackbarControlGamma = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// panelInputImage
			// 
			this.panelInputImage.Location = new System.Drawing.Point(12, 12);
			this.panelInputImage.Name = "panelInputImage";
			this.panelInputImage.Size = new System.Drawing.Size(350, 350);
			this.panelInputImage.TabIndex = 0;
			// 
			// panelOutputReconstruction
			// 
			this.panelOutputReconstruction.Location = new System.Drawing.Point(724, 12);
			this.panelOutputReconstruction.Name = "panelOutputReconstruction";
			this.panelOutputReconstruction.Size = new System.Drawing.Size(350, 350);
			this.panelOutputReconstruction.TabIndex = 0;
			// 
			// panelPixelDensity
			// 
			this.panelPixelDensity.Location = new System.Drawing.Point(1080, 12);
			this.panelPixelDensity.Name = "panelPixelDensity";
			this.panelPixelDensity.Size = new System.Drawing.Size(350, 350);
			this.panelPixelDensity.TabIndex = 0;
			// 
			// panelSparseInputImage
			// 
			this.panelSparseInputImage.Location = new System.Drawing.Point(368, 12);
			this.panelSparseInputImage.Name = "panelSparseInputImage";
			this.panelSparseInputImage.Size = new System.Drawing.Size(350, 350);
			this.panelSparseInputImage.TabIndex = 0;
			// 
			// floatTrackbarControlGamma
			// 
			this.floatTrackbarControlGamma.Location = new System.Drawing.Point(779, 368);
			this.floatTrackbarControlGamma.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlGamma.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlGamma.Name = "floatTrackbarControlGamma";
			this.floatTrackbarControlGamma.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlGamma.TabIndex = 1;
			this.floatTrackbarControlGamma.Value = 4F;
			this.floatTrackbarControlGamma.VisibleRangeMax = 16F;
			this.floatTrackbarControlGamma.SliderDragStop += new Nuaj.Cirrus.Utility.FloatTrackbarControl.SliderDragStopEventHandler(this.floatTrackbarControlGamma_SliderDragStop);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(721, 371);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(52, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Exponent";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1444, 397);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlGamma);
			this.Controls.Add(this.panelPixelDensity);
			this.Controls.Add(this.panelOutputReconstruction);
			this.Controls.Add(this.panelSparseInputImage);
			this.Controls.Add(this.panelInputImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Pull-Push Algorithm Test";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelInputImage;
		private PanelOutput panelOutputReconstruction;
		private PanelOutput panelPixelDensity;
		private PanelOutput panelSparseInputImage;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlGamma;
		private System.Windows.Forms.Label label1;
	}
}

