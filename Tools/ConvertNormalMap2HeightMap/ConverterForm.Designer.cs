namespace GenerateHeightMapFromNormalMap
{
	partial class TransformForm
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
			this.imagePanelNormal = new GenerateHeightMapFromNormalMap.ImagePanel();
			this.imagePanelHeight = new GenerateHeightMapFromNormalMap.ImagePanel();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.SuspendLayout();
			// 
			// imagePanelNormal
			// 
			this.imagePanelNormal.Bitmap = null;
			this.imagePanelNormal.Brightness = 0F;
			this.imagePanelNormal.Contrast = 0F;
			this.imagePanelNormal.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F);
			this.imagePanelNormal.Gamma = 0F;
			this.imagePanelNormal.Location = new System.Drawing.Point(12, 12);
			this.imagePanelNormal.MessageOnEmpty = "Click to load a Normal Map,\r\nor drag\'n drop...";
			this.imagePanelNormal.Name = "imagePanelNormal";
			this.imagePanelNormal.Size = new System.Drawing.Size(512, 512);
			this.imagePanelNormal.TabIndex = 0;
			this.imagePanelNormal.ViewLinear = false;
			this.imagePanelNormal.Click += new System.EventHandler(this.imagePanelNormal_Click);
			// 
			// imagePanelHeight
			// 
			this.imagePanelHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.imagePanelHeight.Bitmap = null;
			this.imagePanelHeight.Brightness = 0F;
			this.imagePanelHeight.Contrast = 0F;
			this.imagePanelHeight.Gamma = 0F;
			this.imagePanelHeight.Location = new System.Drawing.Point(722, 12);
			this.imagePanelHeight.MessageOnEmpty = null;
			this.imagePanelHeight.Name = "imagePanelHeight";
			this.imagePanelHeight.Size = new System.Drawing.Size(512, 512);
			this.imagePanelHeight.TabIndex = 0;
			this.imagePanelHeight.ViewLinear = false;
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "*.png";
			this.openFileDialog.Filter = "Image Files|*.png,*.tga,*.jpg|All Files|*.*";
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.DefaultExt = "*.png";
			this.saveFileDialog.Filter = "Image Files|*.png,*.tga,*.jpg|All Files|*.*";
			// 
			// TransformForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1246, 544);
			this.Controls.Add(this.imagePanelHeight);
			this.Controls.Add(this.imagePanelNormal);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "TransformForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Normal Map To Height Map Converter";
			this.ResumeLayout(false);

		}

		#endregion

		private ImagePanel imagePanelNormal;
		private ImagePanel imagePanelHeight;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
	}
}

