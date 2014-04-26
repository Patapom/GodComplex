namespace MotionTextureComputer
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
			this.outputPanelSpatial = new MotionTextureComputer.OutputPanel();
			this.outputPanelFrequency = new MotionTextureComputer.OutputPanel();
			this.SuspendLayout();
			// 
			// outputPanelSpatial
			// 
			this.outputPanelSpatial.Location = new System.Drawing.Point(518, 0);
			this.outputPanelSpatial.Name = "outputPanelSpatial";
			this.outputPanelSpatial.Size = new System.Drawing.Size(512, 512);
			this.outputPanelSpatial.TabIndex = 0;
			// 
			// outputPanelFrequency
			// 
			this.outputPanelFrequency.Location = new System.Drawing.Point(0, 0);
			this.outputPanelFrequency.Name = "outputPanelFrequency";
			this.outputPanelFrequency.Size = new System.Drawing.Size(512, 512);
			this.outputPanelFrequency.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1030, 512);
			this.Controls.Add(this.outputPanelSpatial);
			this.Controls.Add(this.outputPanelFrequency);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Ocean Wave Texture Computer";
			this.ResumeLayout(false);

		}

		#endregion

		private OutputPanel outputPanelFrequency;
		private OutputPanel outputPanelSpatial;
	}
}

