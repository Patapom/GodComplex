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
			this.buttonConvert = new System.Windows.Forms.Button();
			this.buttonConvertOneSided = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.radioButtonLeft = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.radioButtonRight = new System.Windows.Forms.RadioButton();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.radioButtonBottom = new System.Windows.Forms.RadioButton();
			this.radioButtonTop = new System.Windows.Forms.RadioButton();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
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
			this.imagePanelHeight.Click += new System.EventHandler(this.imagePanelHeight_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "*.png";
			this.openFileDialog.Filter = "Image Files|*.png;*.tga;*.jpg|All Files|*.*";
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.DefaultExt = "*.png";
			this.saveFileDialog.Filter = "PNG Files|*.png|All Files|*.*";
			// 
			// buttonConvert
			// 
			this.buttonConvert.Enabled = false;
			this.buttonConvert.Location = new System.Drawing.Point(571, 213);
			this.buttonConvert.Name = "buttonConvert";
			this.buttonConvert.Size = new System.Drawing.Size(108, 23);
			this.buttonConvert.TabIndex = 1;
			this.buttonConvert.Text = "Convert Central";
			this.buttonConvert.UseVisualStyleBackColor = true;
			this.buttonConvert.Click += new System.EventHandler(this.buttonConvert_Click);
			// 
			// buttonConvertOneSided
			// 
			this.buttonConvertOneSided.Enabled = false;
			this.buttonConvertOneSided.Location = new System.Drawing.Point(571, 242);
			this.buttonConvertOneSided.Name = "buttonConvertOneSided";
			this.buttonConvertOneSided.Size = new System.Drawing.Size(108, 23);
			this.buttonConvertOneSided.TabIndex = 1;
			this.buttonConvertOneSided.Text = "Convert One-Sided";
			this.buttonConvertOneSided.UseVisualStyleBackColor = true;
			this.buttonConvertOneSided.Click += new System.EventHandler(this.buttonConvertOneSided_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.radioButtonRight);
			this.panel1.Controls.Add(this.radioButtonLeft);
			this.panel1.Location = new System.Drawing.Point(530, 155);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(186, 18);
			this.panel1.TabIndex = 2;
			// 
			// radioButtonLeft
			// 
			this.radioButtonLeft.AutoSize = true;
			this.radioButtonLeft.Location = new System.Drawing.Point(94, -2);
			this.radioButtonLeft.Name = "radioButtonLeft";
			this.radioButtonLeft.Size = new System.Drawing.Size(39, 17);
			this.radioButtonLeft.TabIndex = 0;
			this.radioButtonLeft.Text = "left";
			this.radioButtonLeft.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(-2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(88, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Red points to the";
			// 
			// radioButtonRight
			// 
			this.radioButtonRight.AutoSize = true;
			this.radioButtonRight.Checked = true;
			this.radioButtonRight.Location = new System.Drawing.Point(131, -2);
			this.radioButtonRight.Name = "radioButtonRight";
			this.radioButtonRight.Size = new System.Drawing.Size(45, 17);
			this.radioButtonRight.TabIndex = 0;
			this.radioButtonRight.TabStop = true;
			this.radioButtonRight.Text = "right";
			this.radioButtonRight.UseVisualStyleBackColor = true;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.radioButtonBottom);
			this.panel2.Controls.Add(this.radioButtonTop);
			this.panel2.Controls.Add(this.label2);
			this.panel2.Location = new System.Drawing.Point(530, 179);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(191, 18);
			this.panel2.TabIndex = 2;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(-2, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(97, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Green points to the";
			// 
			// radioButtonBottom
			// 
			this.radioButtonBottom.AutoSize = true;
			this.radioButtonBottom.Location = new System.Drawing.Point(131, -2);
			this.radioButtonBottom.Name = "radioButtonBottom";
			this.radioButtonBottom.Size = new System.Drawing.Size(57, 17);
			this.radioButtonBottom.TabIndex = 0;
			this.radioButtonBottom.Text = "bottom";
			this.radioButtonBottom.UseVisualStyleBackColor = true;
			// 
			// radioButtonTop
			// 
			this.radioButtonTop.AutoSize = true;
			this.radioButtonTop.Checked = true;
			this.radioButtonTop.Location = new System.Drawing.Point(94, -2);
			this.radioButtonTop.Name = "radioButtonTop";
			this.radioButtonTop.Size = new System.Drawing.Size(40, 17);
			this.radioButtonTop.TabIndex = 0;
			this.radioButtonTop.TabStop = true;
			this.radioButtonTop.Text = "top";
			this.radioButtonTop.UseVisualStyleBackColor = true;
			// 
			// TransformForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1246, 544);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.buttonConvertOneSided);
			this.Controls.Add(this.buttonConvert);
			this.Controls.Add(this.imagePanelHeight);
			this.Controls.Add(this.imagePanelNormal);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "TransformForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Normal Map To Height Map Converter";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ImagePanel imagePanelNormal;
		private ImagePanel imagePanelHeight;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.Button buttonConvert;
		private System.Windows.Forms.Button buttonConvertOneSided;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton radioButtonLeft;
		private System.Windows.Forms.RadioButton radioButtonRight;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.RadioButton radioButtonBottom;
		private System.Windows.Forms.RadioButton radioButtonTop;
		private System.Windows.Forms.Label label2;
	}
}

