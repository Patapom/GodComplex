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
			this.openFileDialogSourceImage = new System.Windows.Forms.OpenFileDialog();
			this.button1 = new System.Windows.Forms.Button();
			this.labelLuminance = new System.Windows.Forms.Label();
			this.outputPanel = new StandardizedDiffuseAlbedoMaps.OutputPanel(this.components);
			this.checkBoxsRGB = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// openFileDialogSourceImage
			// 
			this.openFileDialogSourceImage.DefaultExt = "*.png";
			this.openFileDialogSourceImage.Filter = "All supported formats|*.PNG;*.GIF;*.BMP;*.HDR;*.CRW;*.TIFF;*.TGA;*.JPG|PNG (*.png)|*.png|Canon Raw (*.crw)|*.crw|All Files (*.*)|*.*";
			this.openFileDialogSourceImage.RestoreDirectory = true;
			this.openFileDialogSourceImage.Title = "Choose a source image of a diffuse albedo map...";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(321, 554);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "button1";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// labelLuminance
			// 
			this.labelLuminance.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelLuminance.Location = new System.Drawing.Point(512, 554);
			this.labelLuminance.Name = "labelLuminance";
			this.labelLuminance.Size = new System.Drawing.Size(100, 23);
			this.labelLuminance.TabIndex = 2;
			// 
			// outputPanel
			// 
			this.outputPanel.Location = new System.Drawing.Point(12, 12);
			this.outputPanel.Luminance = null;
			this.outputPanel.Name = "outputPanel";
			this.outputPanel.Size = new System.Drawing.Size(768, 512);
			this.outputPanel.TabIndex = 0;
			this.outputPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.outputPanel_MouseMove);
			// 
			// checkBoxsRGB
			// 
			this.checkBoxsRGB.AutoSize = true;
			this.checkBoxsRGB.Checked = true;
			this.checkBoxsRGB.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxsRGB.Location = new System.Drawing.Point(78, 554);
			this.checkBoxsRGB.Name = "checkBoxsRGB";
			this.checkBoxsRGB.Size = new System.Drawing.Size(54, 17);
			this.checkBoxsRGB.TabIndex = 3;
			this.checkBoxsRGB.Text = "sRGB";
			this.checkBoxsRGB.UseVisualStyleBackColor = true;
			this.checkBoxsRGB.CheckedChanged += new System.EventHandler(this.checkBoxsRGB_CheckedChanged);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(797, 591);
			this.Controls.Add(this.checkBoxsRGB);
			this.Controls.Add(this.labelLuminance);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.outputPanel);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Standardized Diffuse Albedo Maps Creator";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OutputPanel outputPanel;
		private System.Windows.Forms.OpenFileDialog openFileDialogSourceImage;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label labelLuminance;
		private System.Windows.Forms.CheckBox checkBoxsRGB;
	}
}

