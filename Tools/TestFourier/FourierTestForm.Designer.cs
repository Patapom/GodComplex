namespace TestFourier
{
	partial class FourierTestForm
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
			this.imagePanel = new TestFourier.ImagePanel();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// imagePanel
			// 
			this.imagePanel.Bitmap = null;
			this.imagePanel.Location = new System.Drawing.Point(12, 80);
			this.imagePanel.MessageOnEmpty = null;
			this.imagePanel.Name = "imagePanel";
			this.imagePanel.Size = new System.Drawing.Size(1000, 500);
			this.imagePanel.TabIndex = 0;
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			timer1.Interval = 10;
			// 
			// FourierTestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1294, 777);
			this.Controls.Add(this.imagePanel);
			this.Name = "FourierTestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Fourier Transform Test";
			this.ResumeLayout(false);

		}

		#endregion

		private ImagePanel imagePanel;
		private System.Windows.Forms.Timer timer1;
	}
}

