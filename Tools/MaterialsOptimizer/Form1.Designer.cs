namespace MaterialsOptimizer
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
			this.textBoxLog = new System.Windows.Forms.TextBox();
			this.textBoxMaterialsBasePath = new System.Windows.Forms.TextBox();
			this.buttonSetMaterialsBasePath = new System.Windows.Forms.Button();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxTexturesBasePath = new System.Windows.Forms.TextBox();
			this.buttonSetTexturesBasePath = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonParseMaterials = new System.Windows.Forms.Button();
			this.buttonCollectTextures = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// textBoxLog
			// 
			this.textBoxLog.Location = new System.Drawing.Point(501, 212);
			this.textBoxLog.Multiline = true;
			this.textBoxLog.Name = "textBoxLog";
			this.textBoxLog.ReadOnly = true;
			this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxLog.Size = new System.Drawing.Size(354, 589);
			this.textBoxLog.TabIndex = 0;
			// 
			// textBoxMaterialsBasePath
			// 
			this.textBoxMaterialsBasePath.Location = new System.Drawing.Point(120, 24);
			this.textBoxMaterialsBasePath.Name = "textBoxMaterialsBasePath";
			this.textBoxMaterialsBasePath.Size = new System.Drawing.Size(362, 20);
			this.textBoxMaterialsBasePath.TabIndex = 1;
			this.textBoxMaterialsBasePath.Text = "V:\\Dishonored2\\Dishonored2\\base\\decls\\m2";
			// 
			// buttonSetMaterialsBasePath
			// 
			this.buttonSetMaterialsBasePath.Location = new System.Drawing.Point(488, 22);
			this.buttonSetMaterialsBasePath.Name = "buttonSetMaterialsBasePath";
			this.buttonSetMaterialsBasePath.Size = new System.Drawing.Size(36, 23);
			this.buttonSetMaterialsBasePath.TabIndex = 2;
			this.buttonSetMaterialsBasePath.Text = "...";
			this.buttonSetMaterialsBasePath.UseVisualStyleBackColor = true;
			this.buttonSetMaterialsBasePath.Click += new System.EventHandler(this.buttonSetMaterialsBasePath_Click);
			// 
			// folderBrowserDialog
			// 
			this.folderBrowserDialog.Description = "Select the base folder for parsing";
			this.folderBrowserDialog.ShowNewFolderButton = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 27);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(101, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Materials Base Path";
			// 
			// textBoxTexturesBasePath
			// 
			this.textBoxTexturesBasePath.Location = new System.Drawing.Point(120, 50);
			this.textBoxTexturesBasePath.Name = "textBoxTexturesBasePath";
			this.textBoxTexturesBasePath.Size = new System.Drawing.Size(362, 20);
			this.textBoxTexturesBasePath.TabIndex = 1;
			this.textBoxTexturesBasePath.Text = "V:\\Dishonored2\\Dishonored2\\base";
			// 
			// buttonSetTexturesBasePath
			// 
			this.buttonSetTexturesBasePath.Location = new System.Drawing.Point(488, 48);
			this.buttonSetTexturesBasePath.Name = "buttonSetTexturesBasePath";
			this.buttonSetTexturesBasePath.Size = new System.Drawing.Size(36, 23);
			this.buttonSetTexturesBasePath.TabIndex = 2;
			this.buttonSetTexturesBasePath.Text = "...";
			this.buttonSetTexturesBasePath.UseVisualStyleBackColor = true;
			this.buttonSetTexturesBasePath.Click += new System.EventHandler(this.buttonSetTexturesBasePath_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 53);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Textures Base Path";
			// 
			// buttonParseMaterials
			// 
			this.buttonParseMaterials.Location = new System.Drawing.Point(530, 22);
			this.buttonParseMaterials.Name = "buttonParseMaterials";
			this.buttonParseMaterials.Size = new System.Drawing.Size(75, 23);
			this.buttonParseMaterials.TabIndex = 4;
			this.buttonParseMaterials.Text = "Parse";
			this.buttonParseMaterials.UseVisualStyleBackColor = true;
			this.buttonParseMaterials.Click += new System.EventHandler(this.buttonParseMaterials_Click);
			// 
			// buttonCollectTextures
			// 
			this.buttonCollectTextures.Location = new System.Drawing.Point(530, 48);
			this.buttonCollectTextures.Name = "buttonCollectTextures";
			this.buttonCollectTextures.Size = new System.Drawing.Size(75, 23);
			this.buttonCollectTextures.TabIndex = 4;
			this.buttonCollectTextures.Text = "Collect";
			this.buttonCollectTextures.UseVisualStyleBackColor = true;
			this.buttonCollectTextures.Click += new System.EventHandler(this.buttonCollectTextures_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1323, 869);
			this.Controls.Add(this.buttonCollectTextures);
			this.Controls.Add(this.buttonParseMaterials);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonSetTexturesBasePath);
			this.Controls.Add(this.buttonSetMaterialsBasePath);
			this.Controls.Add(this.textBoxTexturesBasePath);
			this.Controls.Add(this.textBoxMaterialsBasePath);
			this.Controls.Add(this.textBoxLog);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBoxLog;
		private System.Windows.Forms.TextBox textBoxMaterialsBasePath;
		private System.Windows.Forms.Button buttonSetMaterialsBasePath;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxTexturesBasePath;
		private System.Windows.Forms.Button buttonSetTexturesBasePath;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonParseMaterials;
		private System.Windows.Forms.Button buttonCollectTextures;
	}
}

