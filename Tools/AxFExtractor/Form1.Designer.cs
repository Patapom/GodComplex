namespace AxFExtractor
{
	partial class AxFDumpForm
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
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.textBoxMatInfo = new System.Windows.Forms.TextBox();
			this.listBoxMaterials = new System.Windows.Forms.ListBox();
			this.buttonLoad = new System.Windows.Forms.Button();
			this.labelMaterialsCount = new System.Windows.Forms.Label();
			this.buttonDumpMaterial = new System.Windows.Forms.Button();
			this.buttonDumpAllMaterials = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.DefaultExt = "*.axf";
			this.openFileDialog1.Filter = "X-Rite AxF Material Files (*.axf)|*.axf|All Files|*.*";
			this.openFileDialog1.Title = "Choose an AxF file to explore...";
			// 
			// folderBrowserDialog1
			// 
			this.folderBrowserDialog1.Description = "Choose a target folder where to dump material textures to";
			// 
			// textBoxMatInfo
			// 
			this.textBoxMatInfo.Location = new System.Drawing.Point(212, 73);
			this.textBoxMatInfo.Multiline = true;
			this.textBoxMatInfo.Name = "textBoxMatInfo";
			this.textBoxMatInfo.ReadOnly = true;
			this.textBoxMatInfo.Size = new System.Drawing.Size(368, 342);
			this.textBoxMatInfo.TabIndex = 0;
			// 
			// listBoxMaterials
			// 
			this.listBoxMaterials.FormattingEnabled = true;
			this.listBoxMaterials.Location = new System.Drawing.Point(12, 73);
			this.listBoxMaterials.Name = "listBoxMaterials";
			this.listBoxMaterials.Size = new System.Drawing.Size(194, 342);
			this.listBoxMaterials.TabIndex = 1;
			this.listBoxMaterials.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
			// 
			// buttonLoad
			// 
			this.buttonLoad.Location = new System.Drawing.Point(12, 12);
			this.buttonLoad.Name = "buttonLoad";
			this.buttonLoad.Size = new System.Drawing.Size(95, 23);
			this.buttonLoad.TabIndex = 2;
			this.buttonLoad.Text = "Open AxF File";
			this.buttonLoad.UseVisualStyleBackColor = true;
			this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
			// 
			// labelMaterialsCount
			// 
			this.labelMaterialsCount.AutoSize = true;
			this.labelMaterialsCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelMaterialsCount.Location = new System.Drawing.Point(9, 52);
			this.labelMaterialsCount.Name = "labelMaterialsCount";
			this.labelMaterialsCount.Size = new System.Drawing.Size(108, 15);
			this.labelMaterialsCount.TabIndex = 3;
			this.labelMaterialsCount.Text = "No Material Selected";
			// 
			// buttonDumpMaterial
			// 
			this.buttonDumpMaterial.Enabled = false;
			this.buttonDumpMaterial.Location = new System.Drawing.Point(505, 421);
			this.buttonDumpMaterial.Name = "buttonDumpMaterial";
			this.buttonDumpMaterial.Size = new System.Drawing.Size(75, 23);
			this.buttonDumpMaterial.TabIndex = 4;
			this.buttonDumpMaterial.Text = "Dump";
			this.buttonDumpMaterial.UseVisualStyleBackColor = true;
			this.buttonDumpMaterial.Click += new System.EventHandler(this.buttonDumpMaterial_Click);
			// 
			// buttonDumpAllMaterials
			// 
			this.buttonDumpAllMaterials.Enabled = false;
			this.buttonDumpAllMaterials.Location = new System.Drawing.Point(113, 12);
			this.buttonDumpAllMaterials.Name = "buttonDumpAllMaterials";
			this.buttonDumpAllMaterials.Size = new System.Drawing.Size(120, 23);
			this.buttonDumpAllMaterials.TabIndex = 4;
			this.buttonDumpAllMaterials.Text = "Dump All Materials";
			this.buttonDumpAllMaterials.UseVisualStyleBackColor = true;
			this.buttonDumpAllMaterials.Click += new System.EventHandler(this.buttonDumpAllMaterials_Click);
			// 
			// AxFDumpForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(600, 461);
			this.Controls.Add(this.buttonDumpAllMaterials);
			this.Controls.Add(this.buttonDumpMaterial);
			this.Controls.Add(this.labelMaterialsCount);
			this.Controls.Add(this.buttonLoad);
			this.Controls.Add(this.listBoxMaterials);
			this.Controls.Add(this.textBoxMatInfo);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "AxFDumpForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "AxF File Explorer";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.TextBox textBoxMatInfo;
		private System.Windows.Forms.ListBox listBoxMaterials;
		private System.Windows.Forms.Button buttonLoad;
		private System.Windows.Forms.Label labelMaterialsCount;
		private System.Windows.Forms.Button buttonDumpMaterial;
		private System.Windows.Forms.Button buttonDumpAllMaterials;
	}
}

