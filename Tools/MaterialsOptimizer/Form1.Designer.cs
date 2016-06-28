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
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabPageMaterials = new System.Windows.Forms.TabPage();
			this.tabPageTextures = new System.Windows.Forms.TabPage();
			this.listViewTextures = new System.Windows.Forms.ListView();
			this.columnHeaderImageName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderImageSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderImageUsage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderMaterialsReferencesCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.checkBoxShowTGA = new System.Windows.Forms.CheckBox();
			this.checkBoxShowPNG = new System.Windows.Forms.CheckBox();
			this.checkBoxShowOtherFormats = new System.Windows.Forms.CheckBox();
			this.tabControl.SuspendLayout();
			this.tabPageTextures.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBoxLog
			// 
			this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxLog.Location = new System.Drawing.Point(16, 670);
			this.textBoxLog.Multiline = true;
			this.textBoxLog.Name = "textBoxLog";
			this.textBoxLog.ReadOnly = true;
			this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxLog.Size = new System.Drawing.Size(1295, 187);
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
			this.textBoxTexturesBasePath.Text = "V:\\Dishonored2\\Dishonored2\\base\\models";
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
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl.Controls.Add(this.tabPageMaterials);
			this.tabControl.Controls.Add(this.tabPageTextures);
			this.tabControl.Location = new System.Drawing.Point(12, 96);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(1299, 568);
			this.tabControl.TabIndex = 5;
			// 
			// tabPageMaterials
			// 
			this.tabPageMaterials.Location = new System.Drawing.Point(4, 22);
			this.tabPageMaterials.Name = "tabPageMaterials";
			this.tabPageMaterials.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageMaterials.Size = new System.Drawing.Size(1291, 542);
			this.tabPageMaterials.TabIndex = 0;
			this.tabPageMaterials.Text = "Materials";
			this.tabPageMaterials.UseVisualStyleBackColor = true;
			// 
			// tabPageTextures
			// 
			this.tabPageTextures.Controls.Add(this.checkBoxShowOtherFormats);
			this.tabPageTextures.Controls.Add(this.checkBoxShowPNG);
			this.tabPageTextures.Controls.Add(this.checkBoxShowTGA);
			this.tabPageTextures.Controls.Add(this.listViewTextures);
			this.tabPageTextures.Location = new System.Drawing.Point(4, 22);
			this.tabPageTextures.Name = "tabPageTextures";
			this.tabPageTextures.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageTextures.Size = new System.Drawing.Size(1291, 542);
			this.tabPageTextures.TabIndex = 1;
			this.tabPageTextures.Text = "Textures";
			this.tabPageTextures.UseVisualStyleBackColor = true;
			// 
			// listViewTextures
			// 
			this.listViewTextures.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listViewTextures.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderImageName,
            this.columnHeaderImageSize,
            this.columnHeaderImageUsage,
            this.columnHeaderMaterialsReferencesCount});
			this.listViewTextures.GridLines = true;
			this.listViewTextures.Location = new System.Drawing.Point(6, 6);
			this.listViewTextures.Name = "listViewTextures";
			this.listViewTextures.Size = new System.Drawing.Size(1279, 475);
			this.listViewTextures.TabIndex = 1;
			this.listViewTextures.UseCompatibleStateImageBehavior = false;
			this.listViewTextures.View = System.Windows.Forms.View.Details;
			this.listViewTextures.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewTextures_ColumnClick);
			// 
			// columnHeaderImageName
			// 
			this.columnHeaderImageName.Text = "File Name";
			this.columnHeaderImageName.Width = 300;
			// 
			// columnHeaderImageSize
			// 
			this.columnHeaderImageSize.Text = "Size";
			this.columnHeaderImageSize.Width = 100;
			// 
			// columnHeaderImageUsage
			// 
			this.columnHeaderImageUsage.Text = "Usage";
			// 
			// columnHeaderMaterialsReferencesCount
			// 
			this.columnHeaderMaterialsReferencesCount.Text = "Material Ref Count";
			this.columnHeaderMaterialsReferencesCount.Width = 100;
			// 
			// checkBoxShowTGA
			// 
			this.checkBoxShowTGA.AutoSize = true;
			this.checkBoxShowTGA.Checked = true;
			this.checkBoxShowTGA.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowTGA.Location = new System.Drawing.Point(7, 488);
			this.checkBoxShowTGA.Name = "checkBoxShowTGA";
			this.checkBoxShowTGA.Size = new System.Drawing.Size(76, 17);
			this.checkBoxShowTGA.TabIndex = 2;
			this.checkBoxShowTGA.Text = "show TGA";
			this.checkBoxShowTGA.UseVisualStyleBackColor = true;
			this.checkBoxShowTGA.CheckedChanged += new System.EventHandler(this.checkBoxShowTGA_CheckedChanged);
			// 
			// checkBoxShowPNG
			// 
			this.checkBoxShowPNG.AutoSize = true;
			this.checkBoxShowPNG.Checked = true;
			this.checkBoxShowPNG.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowPNG.Location = new System.Drawing.Point(121, 488);
			this.checkBoxShowPNG.Name = "checkBoxShowPNG";
			this.checkBoxShowPNG.Size = new System.Drawing.Size(77, 17);
			this.checkBoxShowPNG.TabIndex = 2;
			this.checkBoxShowPNG.Text = "show PNG";
			this.checkBoxShowPNG.UseVisualStyleBackColor = true;
			this.checkBoxShowPNG.CheckedChanged += new System.EventHandler(this.checkBoxShowPNG_CheckedChanged);
			// 
			// checkBoxShowOtherFormats
			// 
			this.checkBoxShowOtherFormats.AutoSize = true;
			this.checkBoxShowOtherFormats.Checked = true;
			this.checkBoxShowOtherFormats.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowOtherFormats.Location = new System.Drawing.Point(240, 487);
			this.checkBoxShowOtherFormats.Name = "checkBoxShowOtherFormats";
			this.checkBoxShowOtherFormats.Size = new System.Drawing.Size(115, 17);
			this.checkBoxShowOtherFormats.TabIndex = 2;
			this.checkBoxShowOtherFormats.Text = "show misc. formats";
			this.checkBoxShowOtherFormats.UseVisualStyleBackColor = true;
			this.checkBoxShowOtherFormats.CheckedChanged += new System.EventHandler(this.checkBoxShowOtherFormats_CheckedChanged);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1323, 869);
			this.Controls.Add(this.tabControl);
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
			this.tabControl.ResumeLayout(false);
			this.tabPageTextures.ResumeLayout(false);
			this.tabPageTextures.PerformLayout();
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
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabPageMaterials;
		private System.Windows.Forms.TabPage tabPageTextures;
		private System.Windows.Forms.ListView listViewTextures;
		private System.Windows.Forms.ColumnHeader columnHeaderImageName;
		private System.Windows.Forms.ColumnHeader columnHeaderImageSize;
		private System.Windows.Forms.ColumnHeader columnHeaderImageUsage;
		private System.Windows.Forms.ColumnHeader columnHeaderMaterialsReferencesCount;
		private System.Windows.Forms.CheckBox checkBoxShowTGA;
		private System.Windows.Forms.CheckBox checkBoxShowPNG;
		private System.Windows.Forms.CheckBox checkBoxShowOtherFormats;
	}
}

