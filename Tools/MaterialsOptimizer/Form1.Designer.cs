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
			this.checkBoxShowDiffuse = new System.Windows.Forms.CheckBox();
			this.checkBoxShowNormal = new System.Windows.Forms.CheckBox();
			this.checkBoxShowGloss = new System.Windows.Forms.CheckBox();
			this.checkBoxShowMetal = new System.Windows.Forms.CheckBox();
			this.checkBoxShowEmissive = new System.Windows.Forms.CheckBox();
			this.checkBoxShowOther = new System.Windows.Forms.CheckBox();
			this.labelTotalTextures = new System.Windows.Forms.Label();
			this.checkBoxInvertFilters = new System.Windows.Forms.CheckBox();
			this.panelFilterTextures = new System.Windows.Forms.Panel();
			this.listViewMaterials = new System.Windows.Forms.ListView();
			this.columnHeaderMaterialFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderLayersCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderIsAlpha = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderIsOptimizable = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderMaterialName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.panelFilterMaterials = new System.Windows.Forms.Panel();
			this.labelTotalMaterials = new System.Windows.Forms.Label();
			this.checkBoxShowArkDefault = new System.Windows.Forms.CheckBox();
			this.checkBoxShowSkin = new System.Windows.Forms.CheckBox();
			this.checkBoxShowHair = new System.Windows.Forms.CheckBox();
			this.checkBoxInvertMaterialFilters = new System.Windows.Forms.CheckBox();
			this.checkBoxShowEye = new System.Windows.Forms.CheckBox();
			this.checkBoxShowVista = new System.Windows.Forms.CheckBox();
			this.checkBoxShowVegetation = new System.Windows.Forms.CheckBox();
			this.checkBoxShowOtherMaterialTypes = new System.Windows.Forms.CheckBox();
			this.columnHeaderErrors = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderProgramType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabControl.SuspendLayout();
			this.tabPageMaterials.SuspendLayout();
			this.tabPageTextures.SuspendLayout();
			this.panelFilterTextures.SuspendLayout();
			this.panelFilterMaterials.SuspendLayout();
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
			this.tabPageMaterials.Controls.Add(this.panelFilterMaterials);
			this.tabPageMaterials.Controls.Add(this.listViewMaterials);
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
			this.tabPageTextures.Controls.Add(this.panelFilterTextures);
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
			this.listViewTextures.Size = new System.Drawing.Size(1279, 445);
			this.listViewTextures.TabIndex = 1;
			this.listViewTextures.UseCompatibleStateImageBehavior = false;
			this.listViewTextures.View = System.Windows.Forms.View.Details;
			this.listViewTextures.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewTextures_ColumnClick);
			// 
			// columnHeaderImageName
			// 
			this.columnHeaderImageName.Text = "File Name";
			this.columnHeaderImageName.Width = 600;
			// 
			// columnHeaderImageSize
			// 
			this.columnHeaderImageSize.Text = "Size";
			this.columnHeaderImageSize.Width = 80;
			// 
			// columnHeaderImageUsage
			// 
			this.columnHeaderImageUsage.Text = "Usage";
			this.columnHeaderImageUsage.Width = 100;
			// 
			// columnHeaderMaterialsReferencesCount
			// 
			this.columnHeaderMaterialsReferencesCount.Text = "Mat. Ref Count";
			this.columnHeaderMaterialsReferencesCount.Width = 100;
			// 
			// checkBoxShowTGA
			// 
			this.checkBoxShowTGA.AutoSize = true;
			this.checkBoxShowTGA.Checked = true;
			this.checkBoxShowTGA.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowTGA.Location = new System.Drawing.Point(3, 5);
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
			this.checkBoxShowPNG.Location = new System.Drawing.Point(3, 28);
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
			this.checkBoxShowOtherFormats.Location = new System.Drawing.Point(3, 51);
			this.checkBoxShowOtherFormats.Name = "checkBoxShowOtherFormats";
			this.checkBoxShowOtherFormats.Size = new System.Drawing.Size(115, 17);
			this.checkBoxShowOtherFormats.TabIndex = 2;
			this.checkBoxShowOtherFormats.Text = "show misc. formats";
			this.checkBoxShowOtherFormats.UseVisualStyleBackColor = true;
			this.checkBoxShowOtherFormats.CheckedChanged += new System.EventHandler(this.checkBoxShowOtherFormats_CheckedChanged);
			// 
			// checkBoxShowDiffuse
			// 
			this.checkBoxShowDiffuse.AutoSize = true;
			this.checkBoxShowDiffuse.Checked = true;
			this.checkBoxShowDiffuse.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowDiffuse.Location = new System.Drawing.Point(197, 5);
			this.checkBoxShowDiffuse.Name = "checkBoxShowDiffuse";
			this.checkBoxShowDiffuse.Size = new System.Drawing.Size(89, 17);
			this.checkBoxShowDiffuse.TabIndex = 2;
			this.checkBoxShowDiffuse.Text = "Show Diffuse";
			this.checkBoxShowDiffuse.UseVisualStyleBackColor = true;
			this.checkBoxShowDiffuse.CheckedChanged += new System.EventHandler(this.checkBoxShowDiffuse_CheckedChanged);
			// 
			// checkBoxShowNormal
			// 
			this.checkBoxShowNormal.AutoSize = true;
			this.checkBoxShowNormal.Checked = true;
			this.checkBoxShowNormal.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowNormal.Location = new System.Drawing.Point(291, 5);
			this.checkBoxShowNormal.Name = "checkBoxShowNormal";
			this.checkBoxShowNormal.Size = new System.Drawing.Size(89, 17);
			this.checkBoxShowNormal.TabIndex = 2;
			this.checkBoxShowNormal.Text = "Show Normal";
			this.checkBoxShowNormal.UseVisualStyleBackColor = true;
			this.checkBoxShowNormal.CheckedChanged += new System.EventHandler(this.checkBoxShowDiffuse_CheckedChanged);
			// 
			// checkBoxShowGloss
			// 
			this.checkBoxShowGloss.AutoSize = true;
			this.checkBoxShowGloss.Checked = true;
			this.checkBoxShowGloss.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowGloss.Location = new System.Drawing.Point(384, 5);
			this.checkBoxShowGloss.Name = "checkBoxShowGloss";
			this.checkBoxShowGloss.Size = new System.Drawing.Size(82, 17);
			this.checkBoxShowGloss.TabIndex = 2;
			this.checkBoxShowGloss.Text = "Show Gloss";
			this.checkBoxShowGloss.UseVisualStyleBackColor = true;
			this.checkBoxShowGloss.CheckedChanged += new System.EventHandler(this.checkBoxShowDiffuse_CheckedChanged);
			// 
			// checkBoxShowMetal
			// 
			this.checkBoxShowMetal.AutoSize = true;
			this.checkBoxShowMetal.Checked = true;
			this.checkBoxShowMetal.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowMetal.Location = new System.Drawing.Point(477, 5);
			this.checkBoxShowMetal.Name = "checkBoxShowMetal";
			this.checkBoxShowMetal.Size = new System.Drawing.Size(82, 17);
			this.checkBoxShowMetal.TabIndex = 2;
			this.checkBoxShowMetal.Text = "Show Metal";
			this.checkBoxShowMetal.UseVisualStyleBackColor = true;
			this.checkBoxShowMetal.CheckedChanged += new System.EventHandler(this.checkBoxShowDiffuse_CheckedChanged);
			// 
			// checkBoxShowEmissive
			// 
			this.checkBoxShowEmissive.AutoSize = true;
			this.checkBoxShowEmissive.Checked = true;
			this.checkBoxShowEmissive.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowEmissive.Location = new System.Drawing.Point(197, 28);
			this.checkBoxShowEmissive.Name = "checkBoxShowEmissive";
			this.checkBoxShowEmissive.Size = new System.Drawing.Size(97, 17);
			this.checkBoxShowEmissive.TabIndex = 2;
			this.checkBoxShowEmissive.Text = "Show Emissive";
			this.checkBoxShowEmissive.UseVisualStyleBackColor = true;
			this.checkBoxShowEmissive.CheckedChanged += new System.EventHandler(this.checkBoxShowDiffuse_CheckedChanged);
			// 
			// checkBoxShowOther
			// 
			this.checkBoxShowOther.AutoSize = true;
			this.checkBoxShowOther.Checked = true;
			this.checkBoxShowOther.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowOther.Location = new System.Drawing.Point(291, 28);
			this.checkBoxShowOther.Name = "checkBoxShowOther";
			this.checkBoxShowOther.Size = new System.Drawing.Size(82, 17);
			this.checkBoxShowOther.TabIndex = 2;
			this.checkBoxShowOther.Text = "Show Other";
			this.checkBoxShowOther.UseVisualStyleBackColor = true;
			this.checkBoxShowOther.CheckedChanged += new System.EventHandler(this.checkBoxShowDiffuse_CheckedChanged);
			// 
			// labelTotalTextures
			// 
			this.labelTotalTextures.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelTotalTextures.Location = new System.Drawing.Point(579, 2);
			this.labelTotalTextures.Name = "labelTotalTextures";
			this.labelTotalTextures.Size = new System.Drawing.Size(93, 43);
			this.labelTotalTextures.TabIndex = 3;
			this.labelTotalTextures.Text = "Total Textures:";
			// 
			// checkBoxInvertFilters
			// 
			this.checkBoxInvertFilters.AutoSize = true;
			this.checkBoxInvertFilters.Location = new System.Drawing.Point(477, 28);
			this.checkBoxInvertFilters.Name = "checkBoxInvertFilters";
			this.checkBoxInvertFilters.Size = new System.Drawing.Size(83, 17);
			this.checkBoxInvertFilters.TabIndex = 2;
			this.checkBoxInvertFilters.Text = "Invert Filters";
			this.checkBoxInvertFilters.UseVisualStyleBackColor = true;
			this.checkBoxInvertFilters.CheckedChanged += new System.EventHandler(this.checkBoxShowDiffuse_CheckedChanged);
			// 
			// panelFilterTextures
			// 
			this.panelFilterTextures.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelFilterTextures.Controls.Add(this.checkBoxShowTGA);
			this.panelFilterTextures.Controls.Add(this.labelTotalTextures);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowDiffuse);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowOtherFormats);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowNormal);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowPNG);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowGloss);
			this.panelFilterTextures.Controls.Add(this.checkBoxInvertFilters);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowMetal);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowOther);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowEmissive);
			this.panelFilterTextures.Location = new System.Drawing.Point(6, 457);
			this.panelFilterTextures.Name = "panelFilterTextures";
			this.panelFilterTextures.Size = new System.Drawing.Size(1279, 79);
			this.panelFilterTextures.TabIndex = 4;
			// 
			// listViewMaterials
			// 
			this.listViewMaterials.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listViewMaterials.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderMaterialName,
            this.columnHeaderProgramType,
            this.columnHeaderLayersCount,
            this.columnHeaderIsAlpha,
            this.columnHeaderIsOptimizable,
            this.columnHeaderErrors,
            this.columnHeaderMaterialFileName});
			this.listViewMaterials.GridLines = true;
			this.listViewMaterials.Location = new System.Drawing.Point(6, 6);
			this.listViewMaterials.Name = "listViewMaterials";
			this.listViewMaterials.Size = new System.Drawing.Size(1279, 445);
			this.listViewMaterials.TabIndex = 2;
			this.listViewMaterials.UseCompatibleStateImageBehavior = false;
			this.listViewMaterials.View = System.Windows.Forms.View.Details;
			// 
			// columnHeaderMaterialFileName
			// 
			this.columnHeaderMaterialFileName.Text = "File Name";
			this.columnHeaderMaterialFileName.Width = 600;
			// 
			// columnHeaderLayersCount
			// 
			this.columnHeaderLayersCount.Text = "Layers";
			// 
			// columnHeaderIsAlpha
			// 
			this.columnHeaderIsAlpha.Text = "isAlpha";
			this.columnHeaderIsAlpha.Width = 50;
			// 
			// columnHeaderIsOptimizable
			// 
			this.columnHeaderIsOptimizable.Text = "Can Optimize";
			this.columnHeaderIsOptimizable.Width = 150;
			// 
			// columnHeaderMaterialName
			// 
			this.columnHeaderMaterialName.Text = "Material Name";
			this.columnHeaderMaterialName.Width = 200;
			// 
			// panelFilterMaterials
			// 
			this.panelFilterMaterials.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelFilterMaterials.Controls.Add(this.labelTotalMaterials);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowArkDefault);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowSkin);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowHair);
			this.panelFilterMaterials.Controls.Add(this.checkBoxInvertMaterialFilters);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowEye);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowOtherMaterialTypes);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowVista);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowVegetation);
			this.panelFilterMaterials.Location = new System.Drawing.Point(6, 457);
			this.panelFilterMaterials.Name = "panelFilterMaterials";
			this.panelFilterMaterials.Size = new System.Drawing.Size(1279, 79);
			this.panelFilterMaterials.TabIndex = 5;
			// 
			// labelTotalMaterials
			// 
			this.labelTotalMaterials.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelTotalMaterials.Location = new System.Drawing.Point(579, 2);
			this.labelTotalMaterials.Name = "labelTotalMaterials";
			this.labelTotalMaterials.Size = new System.Drawing.Size(93, 43);
			this.labelTotalMaterials.TabIndex = 3;
			this.labelTotalMaterials.Text = "Total Materials:";
			// 
			// checkBoxShowArkDefault
			// 
			this.checkBoxShowArkDefault.AutoSize = true;
			this.checkBoxShowArkDefault.Checked = true;
			this.checkBoxShowArkDefault.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowArkDefault.Location = new System.Drawing.Point(197, 5);
			this.checkBoxShowArkDefault.Name = "checkBoxShowArkDefault";
			this.checkBoxShowArkDefault.Size = new System.Drawing.Size(90, 17);
			this.checkBoxShowArkDefault.TabIndex = 2;
			this.checkBoxShowArkDefault.Text = "Show Default";
			this.checkBoxShowArkDefault.UseVisualStyleBackColor = true;
			this.checkBoxShowArkDefault.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
			// 
			// checkBoxShowSkin
			// 
			this.checkBoxShowSkin.AutoSize = true;
			this.checkBoxShowSkin.Checked = true;
			this.checkBoxShowSkin.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowSkin.Location = new System.Drawing.Point(291, 5);
			this.checkBoxShowSkin.Name = "checkBoxShowSkin";
			this.checkBoxShowSkin.Size = new System.Drawing.Size(77, 17);
			this.checkBoxShowSkin.TabIndex = 2;
			this.checkBoxShowSkin.Text = "Show Skin";
			this.checkBoxShowSkin.UseVisualStyleBackColor = true;
			this.checkBoxShowSkin.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
			// 
			// checkBoxShowHair
			// 
			this.checkBoxShowHair.AutoSize = true;
			this.checkBoxShowHair.Checked = true;
			this.checkBoxShowHair.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowHair.Location = new System.Drawing.Point(384, 5);
			this.checkBoxShowHair.Name = "checkBoxShowHair";
			this.checkBoxShowHair.Size = new System.Drawing.Size(75, 17);
			this.checkBoxShowHair.TabIndex = 2;
			this.checkBoxShowHair.Text = "Show Hair";
			this.checkBoxShowHair.UseVisualStyleBackColor = true;
			this.checkBoxShowHair.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
			// 
			// checkBoxInvertMaterialFilters
			// 
			this.checkBoxInvertMaterialFilters.AutoSize = true;
			this.checkBoxInvertMaterialFilters.Location = new System.Drawing.Point(477, 28);
			this.checkBoxInvertMaterialFilters.Name = "checkBoxInvertMaterialFilters";
			this.checkBoxInvertMaterialFilters.Size = new System.Drawing.Size(83, 17);
			this.checkBoxInvertMaterialFilters.TabIndex = 2;
			this.checkBoxInvertMaterialFilters.Text = "Invert Filters";
			this.checkBoxInvertMaterialFilters.UseVisualStyleBackColor = true;
			this.checkBoxInvertMaterialFilters.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
			// 
			// checkBoxShowEye
			// 
			this.checkBoxShowEye.AutoSize = true;
			this.checkBoxShowEye.Checked = true;
			this.checkBoxShowEye.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowEye.Location = new System.Drawing.Point(477, 5);
			this.checkBoxShowEye.Name = "checkBoxShowEye";
			this.checkBoxShowEye.Size = new System.Drawing.Size(74, 17);
			this.checkBoxShowEye.TabIndex = 2;
			this.checkBoxShowEye.Text = "Show Eye";
			this.checkBoxShowEye.UseVisualStyleBackColor = true;
			this.checkBoxShowEye.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
			// 
			// checkBoxShowVista
			// 
			this.checkBoxShowVista.AutoSize = true;
			this.checkBoxShowVista.Checked = true;
			this.checkBoxShowVista.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowVista.Location = new System.Drawing.Point(291, 28);
			this.checkBoxShowVista.Name = "checkBoxShowVista";
			this.checkBoxShowVista.Size = new System.Drawing.Size(79, 17);
			this.checkBoxShowVista.TabIndex = 2;
			this.checkBoxShowVista.Text = "Show Vista";
			this.checkBoxShowVista.UseVisualStyleBackColor = true;
			this.checkBoxShowVista.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
			// 
			// checkBoxShowVegetation
			// 
			this.checkBoxShowVegetation.AutoSize = true;
			this.checkBoxShowVegetation.Checked = true;
			this.checkBoxShowVegetation.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowVegetation.Location = new System.Drawing.Point(197, 28);
			this.checkBoxShowVegetation.Name = "checkBoxShowVegetation";
			this.checkBoxShowVegetation.Size = new System.Drawing.Size(107, 17);
			this.checkBoxShowVegetation.TabIndex = 2;
			this.checkBoxShowVegetation.Text = "Show Vegetation";
			this.checkBoxShowVegetation.UseVisualStyleBackColor = true;
			this.checkBoxShowVegetation.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
			// 
			// checkBoxShowOtherMaterialTypes
			// 
			this.checkBoxShowOtherMaterialTypes.AutoSize = true;
			this.checkBoxShowOtherMaterialTypes.Checked = true;
			this.checkBoxShowOtherMaterialTypes.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowOtherMaterialTypes.Location = new System.Drawing.Point(384, 28);
			this.checkBoxShowOtherMaterialTypes.Name = "checkBoxShowOtherMaterialTypes";
			this.checkBoxShowOtherMaterialTypes.Size = new System.Drawing.Size(82, 17);
			this.checkBoxShowOtherMaterialTypes.TabIndex = 2;
			this.checkBoxShowOtherMaterialTypes.Text = "Show Other";
			this.checkBoxShowOtherMaterialTypes.UseVisualStyleBackColor = true;
			this.checkBoxShowOtherMaterialTypes.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
			// 
			// columnHeaderErrors
			// 
			this.columnHeaderErrors.Text = "Errors";
			this.columnHeaderErrors.Width = 150;
			// 
			// columnHeaderProgramType
			// 
			this.columnHeaderProgramType.Text = "Type";
			this.columnHeaderProgramType.Width = 100;
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
			this.tabPageMaterials.ResumeLayout(false);
			this.tabPageTextures.ResumeLayout(false);
			this.panelFilterTextures.ResumeLayout(false);
			this.panelFilterTextures.PerformLayout();
			this.panelFilterMaterials.ResumeLayout(false);
			this.panelFilterMaterials.PerformLayout();
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
		private System.Windows.Forms.CheckBox checkBoxShowDiffuse;
		private System.Windows.Forms.CheckBox checkBoxShowMetal;
		private System.Windows.Forms.CheckBox checkBoxShowGloss;
		private System.Windows.Forms.CheckBox checkBoxShowNormal;
		private System.Windows.Forms.CheckBox checkBoxShowEmissive;
		private System.Windows.Forms.CheckBox checkBoxShowOther;
		private System.Windows.Forms.Label labelTotalTextures;
		private System.Windows.Forms.CheckBox checkBoxInvertFilters;
		private System.Windows.Forms.Panel panelFilterTextures;
		private System.Windows.Forms.ListView listViewMaterials;
		private System.Windows.Forms.ColumnHeader columnHeaderMaterialFileName;
		private System.Windows.Forms.ColumnHeader columnHeaderLayersCount;
		private System.Windows.Forms.ColumnHeader columnHeaderIsAlpha;
		private System.Windows.Forms.ColumnHeader columnHeaderIsOptimizable;
		private System.Windows.Forms.ColumnHeader columnHeaderMaterialName;
		private System.Windows.Forms.Panel panelFilterMaterials;
		private System.Windows.Forms.Label labelTotalMaterials;
		private System.Windows.Forms.CheckBox checkBoxShowArkDefault;
		private System.Windows.Forms.CheckBox checkBoxShowSkin;
		private System.Windows.Forms.CheckBox checkBoxShowHair;
		private System.Windows.Forms.CheckBox checkBoxInvertMaterialFilters;
		private System.Windows.Forms.CheckBox checkBoxShowEye;
		private System.Windows.Forms.CheckBox checkBoxShowVista;
		private System.Windows.Forms.CheckBox checkBoxShowVegetation;
		private System.Windows.Forms.CheckBox checkBoxShowOtherMaterialTypes;
		private System.Windows.Forms.ColumnHeader columnHeaderErrors;
		private System.Windows.Forms.ColumnHeader columnHeaderProgramType;
	}
}

