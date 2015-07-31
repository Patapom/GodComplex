namespace TestBoxFitting
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
			this.panelOutput = new TestBoxFitting.PanelOutput(this.components);
			this.panelHistogram = new TestBoxFitting.PanelHistogram(this.components);
			this.textBoxPlanes = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(691, 447);
			this.panelOutput.TabIndex = 0;
			// 
			// panelHistogram
			// 
			this.panelHistogram.Location = new System.Drawing.Point(727, 12);
			this.panelHistogram.Name = "panelHistogram";
			this.panelHistogram.Size = new System.Drawing.Size(347, 220);
			this.panelHistogram.TabIndex = 1;
			// 
			// textBoxPlanes
			// 
			this.textBoxPlanes.Location = new System.Drawing.Point(727, 251);
			this.textBoxPlanes.Multiline = true;
			this.textBoxPlanes.Name = "textBoxPlanes";
			this.textBoxPlanes.ReadOnly = true;
			this.textBoxPlanes.Size = new System.Drawing.Size(347, 296);
			this.textBoxPlanes.TabIndex = 2;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1117, 703);
			this.Controls.Add(this.textBoxPlanes);
			this.Controls.Add(this.panelOutput);
			this.Controls.Add(this.panelHistogram);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelOutput;
		private PanelHistogram panelHistogram;
		private System.Windows.Forms.TextBox textBoxPlanes;
	}
}

