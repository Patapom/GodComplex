namespace TestForm
{
	partial class MarsLanderForm
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
			this.panelOutput = new TestForm.PanelOutput(this.components);
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1025, 631);
			this.panelOutput.TabIndex = 0;
			this.panelOutput.OnUpdateBitmap += new TestForm.PanelOutput.PaintBitmap(this.panelOutput_OnUpdateBitmap);
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = 500;
			// 
			// MarsLanderForm
			// 
			this.ClientSize = new System.Drawing.Size(1049, 655);
			this.Controls.Add(this.panelOutput);
			this.Name = "MarsLanderForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.ResumeLayout(false);

		}

		#endregion

		private PanelOutput panelOutput;
		private System.Windows.Forms.Timer timer;
	}
}

