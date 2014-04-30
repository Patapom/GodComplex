namespace StandardizedDiffuseAlbedoMaps
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
			this.outputPanel = new StandardizedDiffuseAlbedoMaps.OutputPanel(this.components);
			this.openFileDialogSourceImage = new System.Windows.Forms.OpenFileDialog();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// outputPanel
			// 
			this.outputPanel.Location = new System.Drawing.Point(67, 101);
			this.outputPanel.Name = "outputPanel";
			this.outputPanel.PhotonsAccumulation = null;
			this.outputPanel.Size = new System.Drawing.Size(695, 492);
			this.outputPanel.TabIndex = 0;
			// 
			// openFileDialogSourceImage
			// 
			this.openFileDialogSourceImage.DefaultExt = "*.png";
			this.openFileDialogSourceImage.Filter = "PNG (*.png)|*.png|All Files (*.*)|*.*";
			this.openFileDialogSourceImage.RestoreDirectory = true;
			this.openFileDialogSourceImage.Title = "Choose a source image of a diffuse albedo map...";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(375, 777);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "button1";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1831, 1024);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.outputPanel);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Standardized Diffuse Albedo Maps Creator";
			this.ResumeLayout(false);

		}

		#endregion

		private OutputPanel outputPanel;
		private System.Windows.Forms.OpenFileDialog openFileDialogSourceImage;
		private System.Windows.Forms.Button button1;
	}
}

