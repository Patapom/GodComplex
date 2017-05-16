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
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1444, 377);
			this.Controls.Add(this.panelPixelDensity);
			this.Controls.Add(this.panelOutputReconstruction);
			this.Controls.Add(this.panelSparseInputImage);
			this.Controls.Add(this.panelInputImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Pull-Push Algorithm Test";
			this.ResumeLayout(false);

		}

		#endregion

		private PanelOutput panelInputImage;
		private PanelOutput panelOutputReconstruction;
		private PanelOutput panelPixelDensity;
		private PanelOutput panelSparseInputImage;
	}
}

