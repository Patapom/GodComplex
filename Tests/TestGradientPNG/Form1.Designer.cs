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
			this.panelGraph = new TestGradientPNG.OutputPanel(this.components);
			this.SuspendLayout();
			// 
			// panelGraph
			// 
			this.panelGraph.Gradient = null;
			this.panelGraph.Location = new System.Drawing.Point(12, 12);
			this.panelGraph.Name = "panelGraph";
			this.panelGraph.Size = new System.Drawing.Size(400, 400);
			this.panelGraph.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(771, 638);
			this.Controls.Add(this.panelGraph);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private OutputPanel panelGraph;
	}
}

