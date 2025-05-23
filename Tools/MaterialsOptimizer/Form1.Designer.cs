﻿namespace MaterialsOptimizer
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
			this.components = new System.ComponentModel.Container();
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
			this.panelFilterMaterials = new System.Windows.Forms.Panel();
			this.buttonAnalyzeConstantColorTextures = new System.Windows.Forms.Button();
			this.buttonSearch = new System.Windows.Forms.Button();
			this.textBoxSearchMaterial = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.integerTrackbarControlLayerMax = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.integerTrackbarControlErrorLevel = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.integerTrackbarControlLayerMin = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.labelTotalMaterials = new System.Windows.Forms.Label();
			this.checkBoxShowArkDefault = new System.Windows.Forms.CheckBox();
			this.checkBoxShowSkin = new System.Windows.Forms.CheckBox();
			this.checkBoxShowHair = new System.Windows.Forms.CheckBox();
			this.checkBoxInvertMaterialFilters = new System.Windows.Forms.CheckBox();
			this.checkBoxShowEye = new System.Windows.Forms.CheckBox();
			this.checkBoxShowAlpha = new System.Windows.Forms.CheckBox();
			this.checkBoxShowOtherMaterialTypes = new System.Windows.Forms.CheckBox();
			this.checkBoxShowVista = new System.Windows.Forms.CheckBox();
			this.checkBoxShowOptimizableMaterials = new System.Windows.Forms.CheckBox();
			this.checkBoxShowMissingPhysics = new System.Windows.Forms.CheckBox();
			this.checkBoxShowWarningMaterials = new System.Windows.Forms.CheckBox();
			this.checkBoxShowVegetation = new System.Windows.Forms.CheckBox();
			this.listViewMaterials = new System.Windows.Forms.ListView();
			this.columnHeaderMaterialName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderProgramType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderLayersCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderIsAlpha = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderIsOptimized = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderErrors = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderMaterialFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabPageTextures = new System.Windows.Forms.TabPage();
			this.panelFilterTextures = new System.Windows.Forms.Panel();
			this.buttonSearchTexture = new System.Windows.Forms.Button();
			this.textBoxSearchTexture = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.integerTrackbarControlMinRefCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.checkBoxShowTGA = new System.Windows.Forms.CheckBox();
			this.labelTotalTextures = new System.Windows.Forms.Label();
			this.checkBoxShowDiffuse = new System.Windows.Forms.CheckBox();
			this.checkBoxShowOtherFormats = new System.Windows.Forms.CheckBox();
			this.checkBoxShowNormal = new System.Windows.Forms.CheckBox();
			this.checkBoxShowPNG = new System.Windows.Forms.CheckBox();
			this.checkBoxShowGloss = new System.Windows.Forms.CheckBox();
			this.checkBoxInvertFilters = new System.Windows.Forms.CheckBox();
			this.checkBoxShowMetal = new System.Windows.Forms.CheckBox();
			this.checkBoxShowDiffuseGloss = new System.Windows.Forms.CheckBox();
			this.checkBoxShowMasks = new System.Windows.Forms.CheckBox();
			this.checkBoxShowOther = new System.Windows.Forms.CheckBox();
			this.checkBoxShowEmissive = new System.Windows.Forms.CheckBox();
			this.listViewTextures = new System.Windows.Forms.ListView();
			this.columnHeaderImageName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderImageSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderImageUsage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderChannels = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderMaterialsReferencesCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.buttonReExport = new System.Windows.Forms.Button();
			this.buttonParseReExportedMaterials = new System.Windows.Forms.Button();
			this.buttonCollect_dgTextures = new System.Windows.Forms.Button();
			this.buttonGenerate_dgTextures = new System.Windows.Forms.Button();
			this.buttonIntegratePerforce = new System.Windows.Forms.Button();
			this.progressBarMaterials = new System.Windows.Forms.ProgressBar();
			this.progressBarTextures = new System.Windows.Forms.ProgressBar();
			this.tabControlInfo = new System.Windows.Forms.TabControl();
			this.tabPageInfo = new System.Windows.Forms.TabPage();
			this.textBoxInfo = new System.Windows.Forms.TextBox();
			this.tabPageLog = new System.Windows.Forms.TabPage();
			this.textBoxLog = new System.Windows.Forms.TextBox();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.textBoxReExportPath = new System.Windows.Forms.TextBox();
			this.buttonSetMaterialsReExportPath = new System.Windows.Forms.Button();
			this.label9 = new System.Windows.Forms.Label();
			this.progressBarReExportMaterials = new System.Windows.Forms.ProgressBar();
			this.panel1 = new System.Windows.Forms.Panel();
			this.radioButtonViewOptimizedMaterials = new System.Windows.Forms.RadioButton();
			this.radioButtonViewSourceMaterials = new System.Windows.Forms.RadioButton();
			this.progressBarIntegrate = new System.Windows.Forms.ProgressBar();
			this.buttonCreateTestMap = new System.Windows.Forms.Button();
			this.saveFileDialogTestMap = new System.Windows.Forms.SaveFileDialog();
			this.tabControl.SuspendLayout();
			this.tabPageMaterials.SuspendLayout();
			this.panelFilterMaterials.SuspendLayout();
			this.tabPageTextures.SuspendLayout();
			this.panelFilterTextures.SuspendLayout();
			this.tabControlInfo.SuspendLayout();
			this.tabPageInfo.SuspendLayout();
			this.tabPageLog.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
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
			this.textBoxTexturesBasePath.Text = "V:\\Dishonored2\\Dishonored2\\base\\";
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
			this.label2.Size = new System.Drawing.Size(92, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Project Base Path";
			// 
			// buttonParseMaterials
			// 
			this.buttonParseMaterials.Location = new System.Drawing.Point(530, 22);
			this.buttonParseMaterials.Name = "buttonParseMaterials";
			this.buttonParseMaterials.Size = new System.Drawing.Size(75, 23);
			this.buttonParseMaterials.TabIndex = 4;
			this.buttonParseMaterials.Text = "Parse";
			this.toolTip.SetToolTip(this.buttonParseMaterials, "Parses all the *.M2 files in the material base directory and sub-directories");
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
			this.toolTip.SetToolTip(this.buttonCollectTextures, "Collects all the supported textures (tga, png, jpg) in the textures base director" +
        "y and sub-directories (WARNING: can take up to 20 minutes)");
			this.buttonCollectTextures.UseVisualStyleBackColor = true;
			this.buttonCollectTextures.Click += new System.EventHandler(this.buttonCollectTextures_Click);
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.tabPageMaterials);
			this.tabControl.Controls.Add(this.tabPageTextures);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = new System.Drawing.Point(0, 0);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(1299, 554);
			this.tabControl.TabIndex = 5;
			// 
			// tabPageMaterials
			// 
			this.tabPageMaterials.Controls.Add(this.panelFilterMaterials);
			this.tabPageMaterials.Controls.Add(this.listViewMaterials);
			this.tabPageMaterials.Location = new System.Drawing.Point(4, 22);
			this.tabPageMaterials.Name = "tabPageMaterials";
			this.tabPageMaterials.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageMaterials.Size = new System.Drawing.Size(1291, 528);
			this.tabPageMaterials.TabIndex = 0;
			this.tabPageMaterials.Text = "Materials";
			this.tabPageMaterials.UseVisualStyleBackColor = true;
			// 
			// panelFilterMaterials
			// 
			this.panelFilterMaterials.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelFilterMaterials.Controls.Add(this.buttonAnalyzeConstantColorTextures);
			this.panelFilterMaterials.Controls.Add(this.buttonSearch);
			this.panelFilterMaterials.Controls.Add(this.textBoxSearchMaterial);
			this.panelFilterMaterials.Controls.Add(this.label5);
			this.panelFilterMaterials.Controls.Add(this.label4);
			this.panelFilterMaterials.Controls.Add(this.label7);
			this.panelFilterMaterials.Controls.Add(this.label8);
			this.panelFilterMaterials.Controls.Add(this.label3);
			this.panelFilterMaterials.Controls.Add(this.integerTrackbarControlLayerMax);
			this.panelFilterMaterials.Controls.Add(this.integerTrackbarControlErrorLevel);
			this.panelFilterMaterials.Controls.Add(this.integerTrackbarControlLayerMin);
			this.panelFilterMaterials.Controls.Add(this.labelTotalMaterials);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowArkDefault);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowSkin);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowHair);
			this.panelFilterMaterials.Controls.Add(this.checkBoxInvertMaterialFilters);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowEye);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowAlpha);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowOtherMaterialTypes);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowVista);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowOptimizableMaterials);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowMissingPhysics);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowWarningMaterials);
			this.panelFilterMaterials.Controls.Add(this.checkBoxShowVegetation);
			this.panelFilterMaterials.Location = new System.Drawing.Point(6, 443);
			this.panelFilterMaterials.Name = "panelFilterMaterials";
			this.panelFilterMaterials.Size = new System.Drawing.Size(1279, 79);
			this.panelFilterMaterials.TabIndex = 5;
			// 
			// buttonAnalyzeConstantColorTextures
			// 
			this.buttonAnalyzeConstantColorTextures.Location = new System.Drawing.Point(1008, 50);
			this.buttonAnalyzeConstantColorTextures.Name = "buttonAnalyzeConstantColorTextures";
			this.buttonAnalyzeConstantColorTextures.Size = new System.Drawing.Size(173, 23);
			this.buttonAnalyzeConstantColorTextures.TabIndex = 12;
			this.buttonAnalyzeConstantColorTextures.Text = "Analyze Constant Color Textures";
			this.buttonAnalyzeConstantColorTextures.UseVisualStyleBackColor = true;
			this.buttonAnalyzeConstantColorTextures.Click += new System.EventHandler(this.buttonAnalyzeConstantColorTextures_Click);
			// 
			// buttonSearch
			// 
			this.buttonSearch.Location = new System.Drawing.Point(1020, 6);
			this.buttonSearch.Name = "buttonSearch";
			this.buttonSearch.Size = new System.Drawing.Size(75, 23);
			this.buttonSearch.TabIndex = 7;
			this.buttonSearch.Text = "Search";
			this.buttonSearch.UseVisualStyleBackColor = true;
			this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
			// 
			// textBoxSearchMaterial
			// 
			this.textBoxSearchMaterial.Location = new System.Drawing.Point(694, 8);
			this.textBoxSearchMaterial.Name = "textBoxSearchMaterial";
			this.textBoxSearchMaterial.Size = new System.Drawing.Size(320, 20);
			this.textBoxSearchMaterial.TabIndex = 6;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(3, 55);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(27, 13);
			this.label5.TabIndex = 5;
			this.label5.Text = "Max";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(3, 29);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(24, 13);
			this.label4.TabIndex = 5;
			this.label4.Text = "Min";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(632, 11);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(56, 13);
			this.label7.TabIndex = 5;
			this.label7.Text = "Search for";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(157, 55);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(167, 13);
			this.label8.TabIndex = 5;
			this.label8.Text = "Show Error Materials Above Level";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 6);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(85, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Layers Selection";
			// 
			// integerTrackbarControlLayerMax
			// 
			this.integerTrackbarControlLayerMax.Location = new System.Drawing.Point(36, 52);
			this.integerTrackbarControlLayerMax.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlLayerMax.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlLayerMax.Name = "integerTrackbarControlLayerMax";
			this.integerTrackbarControlLayerMax.RangeMax = 3;
			this.integerTrackbarControlLayerMax.RangeMin = 1;
			this.integerTrackbarControlLayerMax.Size = new System.Drawing.Size(111, 20);
			this.integerTrackbarControlLayerMax.TabIndex = 4;
			this.integerTrackbarControlLayerMax.Value = 3;
			this.integerTrackbarControlLayerMax.VisibleRangeMax = 3;
			this.integerTrackbarControlLayerMax.VisibleRangeMin = 1;
			this.integerTrackbarControlLayerMax.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlLayerMax_ValueChanged);
			// 
			// integerTrackbarControlErrorLevel
			// 
			this.integerTrackbarControlErrorLevel.Location = new System.Drawing.Point(330, 52);
			this.integerTrackbarControlErrorLevel.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlErrorLevel.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlErrorLevel.Name = "integerTrackbarControlErrorLevel";
			this.integerTrackbarControlErrorLevel.RangeMax = 3;
			this.integerTrackbarControlErrorLevel.RangeMin = 0;
			this.integerTrackbarControlErrorLevel.Size = new System.Drawing.Size(111, 20);
			this.integerTrackbarControlErrorLevel.TabIndex = 4;
			this.integerTrackbarControlErrorLevel.Value = 0;
			this.integerTrackbarControlErrorLevel.VisibleRangeMax = 3;
			this.integerTrackbarControlErrorLevel.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlErrorLevel_ValueChanged);
			// 
			// integerTrackbarControlLayerMin
			// 
			this.integerTrackbarControlLayerMin.Location = new System.Drawing.Point(36, 26);
			this.integerTrackbarControlLayerMin.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlLayerMin.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlLayerMin.Name = "integerTrackbarControlLayerMin";
			this.integerTrackbarControlLayerMin.RangeMax = 3;
			this.integerTrackbarControlLayerMin.RangeMin = 1;
			this.integerTrackbarControlLayerMin.Size = new System.Drawing.Size(111, 20);
			this.integerTrackbarControlLayerMin.TabIndex = 4;
			this.integerTrackbarControlLayerMin.Value = 1;
			this.integerTrackbarControlLayerMin.VisibleRangeMax = 3;
			this.integerTrackbarControlLayerMin.VisibleRangeMin = 1;
			this.integerTrackbarControlLayerMin.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlLayerMin_ValueChanged);
			// 
			// labelTotalMaterials
			// 
			this.labelTotalMaterials.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelTotalMaterials.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelTotalMaterials.Location = new System.Drawing.Point(1140, 2);
			this.labelTotalMaterials.Name = "labelTotalMaterials";
			this.labelTotalMaterials.Size = new System.Drawing.Size(136, 44);
			this.labelTotalMaterials.TabIndex = 3;
			this.labelTotalMaterials.Text = "Total Materials:";
			// 
			// checkBoxShowArkDefault
			// 
			this.checkBoxShowArkDefault.AutoSize = true;
			this.checkBoxShowArkDefault.Checked = true;
			this.checkBoxShowArkDefault.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowArkDefault.Location = new System.Drawing.Point(160, 5);
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
			this.checkBoxShowSkin.Location = new System.Drawing.Point(254, 5);
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
			this.checkBoxShowHair.Location = new System.Drawing.Point(347, 5);
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
			this.checkBoxInvertMaterialFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxInvertMaterialFilters.Location = new System.Drawing.Point(440, 28);
			this.checkBoxInvertMaterialFilters.Name = "checkBoxInvertMaterialFilters";
			this.checkBoxInvertMaterialFilters.Size = new System.Drawing.Size(97, 17);
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
			this.checkBoxShowEye.Location = new System.Drawing.Point(440, 5);
			this.checkBoxShowEye.Name = "checkBoxShowEye";
			this.checkBoxShowEye.Size = new System.Drawing.Size(74, 17);
			this.checkBoxShowEye.TabIndex = 2;
			this.checkBoxShowEye.Text = "Show Eye";
			this.checkBoxShowEye.UseVisualStyleBackColor = true;
			this.checkBoxShowEye.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
			// 
			// checkBoxShowAlpha
			// 
			this.checkBoxShowAlpha.AutoSize = true;
			this.checkBoxShowAlpha.Checked = true;
			this.checkBoxShowAlpha.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowAlpha.Location = new System.Drawing.Point(254, 26);
			this.checkBoxShowAlpha.Name = "checkBoxShowAlpha";
			this.checkBoxShowAlpha.Size = new System.Drawing.Size(83, 17);
			this.checkBoxShowAlpha.TabIndex = 2;
			this.checkBoxShowAlpha.Text = "Show Alpha";
			this.checkBoxShowAlpha.UseVisualStyleBackColor = true;
			this.checkBoxShowAlpha.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
			// 
			// checkBoxShowOtherMaterialTypes
			// 
			this.checkBoxShowOtherMaterialTypes.AutoSize = true;
			this.checkBoxShowOtherMaterialTypes.Checked = true;
			this.checkBoxShowOtherMaterialTypes.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowOtherMaterialTypes.Location = new System.Drawing.Point(347, 26);
			this.checkBoxShowOtherMaterialTypes.Name = "checkBoxShowOtherMaterialTypes";
			this.checkBoxShowOtherMaterialTypes.Size = new System.Drawing.Size(82, 17);
			this.checkBoxShowOtherMaterialTypes.TabIndex = 2;
			this.checkBoxShowOtherMaterialTypes.Text = "Show Other";
			this.checkBoxShowOtherMaterialTypes.UseVisualStyleBackColor = true;
			this.checkBoxShowOtherMaterialTypes.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
			// 
			// checkBoxShowVista
			// 
			this.checkBoxShowVista.AutoSize = true;
			this.checkBoxShowVista.Checked = true;
			this.checkBoxShowVista.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowVista.Location = new System.Drawing.Point(520, 5);
			this.checkBoxShowVista.Name = "checkBoxShowVista";
			this.checkBoxShowVista.Size = new System.Drawing.Size(79, 17);
			this.checkBoxShowVista.TabIndex = 2;
			this.checkBoxShowVista.Text = "Show Vista";
			this.checkBoxShowVista.UseVisualStyleBackColor = true;
			this.checkBoxShowVista.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
			// 
			// checkBoxShowOptimizableMaterials
			// 
			this.checkBoxShowOptimizableMaterials.AutoSize = true;
			this.checkBoxShowOptimizableMaterials.Location = new System.Drawing.Point(823, 54);
			this.checkBoxShowOptimizableMaterials.Name = "checkBoxShowOptimizableMaterials";
			this.checkBoxShowOptimizableMaterials.Size = new System.Drawing.Size(179, 17);
			this.checkBoxShowOptimizableMaterials.TabIndex = 2;
			this.checkBoxShowOptimizableMaterials.Text = "Show Optimizable Materials Only";
			this.checkBoxShowOptimizableMaterials.UseVisualStyleBackColor = true;
			this.checkBoxShowOptimizableMaterials.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
			// 
			// checkBoxShowMissingPhysics
			// 
			this.checkBoxShowMissingPhysics.AutoSize = true;
			this.checkBoxShowMissingPhysics.Location = new System.Drawing.Point(618, 54);
			this.checkBoxShowMissingPhysics.Name = "checkBoxShowMissingPhysics";
			this.checkBoxShowMissingPhysics.Size = new System.Drawing.Size(199, 17);
			this.checkBoxShowMissingPhysics.TabIndex = 2;
			this.checkBoxShowMissingPhysics.Text = "Show Missing Physics Materials Only";
			this.checkBoxShowMissingPhysics.UseVisualStyleBackColor = true;
			this.checkBoxShowMissingPhysics.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
			// 
			// checkBoxShowWarningMaterials
			// 
			this.checkBoxShowWarningMaterials.AutoSize = true;
			this.checkBoxShowWarningMaterials.Location = new System.Drawing.Point(447, 54);
			this.checkBoxShowWarningMaterials.Name = "checkBoxShowWarningMaterials";
			this.checkBoxShowWarningMaterials.Size = new System.Drawing.Size(165, 17);
			this.checkBoxShowWarningMaterials.TabIndex = 2;
			this.checkBoxShowWarningMaterials.Text = "Show Warning Materials Only";
			this.checkBoxShowWarningMaterials.UseVisualStyleBackColor = true;
			this.checkBoxShowWarningMaterials.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
			// 
			// checkBoxShowVegetation
			// 
			this.checkBoxShowVegetation.AutoSize = true;
			this.checkBoxShowVegetation.Checked = true;
			this.checkBoxShowVegetation.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowVegetation.Location = new System.Drawing.Point(160, 26);
			this.checkBoxShowVegetation.Name = "checkBoxShowVegetation";
			this.checkBoxShowVegetation.Size = new System.Drawing.Size(107, 17);
			this.checkBoxShowVegetation.TabIndex = 2;
			this.checkBoxShowVegetation.Text = "Show Vegetation";
			this.checkBoxShowVegetation.UseVisualStyleBackColor = true;
			this.checkBoxShowVegetation.CheckedChanged += new System.EventHandler(this.checkBoxShowArkDefault_CheckedChanged);
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
            this.columnHeaderIsOptimized,
            this.columnHeaderErrors,
            this.columnHeaderMaterialFileName});
			this.listViewMaterials.FullRowSelect = true;
			this.listViewMaterials.GridLines = true;
			this.listViewMaterials.HideSelection = false;
			this.listViewMaterials.Location = new System.Drawing.Point(6, 6);
			this.listViewMaterials.Name = "listViewMaterials";
			this.listViewMaterials.ShowItemToolTips = true;
			this.listViewMaterials.Size = new System.Drawing.Size(1279, 431);
			this.listViewMaterials.TabIndex = 2;
			this.listViewMaterials.UseCompatibleStateImageBehavior = false;
			this.listViewMaterials.View = System.Windows.Forms.View.Details;
			this.listViewMaterials.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewMaterials_ColumnClick);
			this.listViewMaterials.SelectedIndexChanged += new System.EventHandler(this.listViewMaterials_SelectedIndexChanged);
			this.listViewMaterials.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewMaterials_MouseDoubleClick);
			// 
			// columnHeaderMaterialName
			// 
			this.columnHeaderMaterialName.Text = "Material Name";
			this.columnHeaderMaterialName.Width = 600;
			// 
			// columnHeaderProgramType
			// 
			this.columnHeaderProgramType.Text = "Type";
			this.columnHeaderProgramType.Width = 100;
			// 
			// columnHeaderLayersCount
			// 
			this.columnHeaderLayersCount.Text = "Layers";
			// 
			// columnHeaderIsAlpha
			// 
			this.columnHeaderIsAlpha.Text = "Alpha?";
			this.columnHeaderIsAlpha.Width = 50;
			// 
			// columnHeaderIsOptimized
			// 
			this.columnHeaderIsOptimized.Text = "Optimized?";
			this.columnHeaderIsOptimized.Width = 70;
			// 
			// columnHeaderErrors
			// 
			this.columnHeaderErrors.Text = "Errors";
			this.columnHeaderErrors.Width = 150;
			// 
			// columnHeaderMaterialFileName
			// 
			this.columnHeaderMaterialFileName.Text = "File Name";
			this.columnHeaderMaterialFileName.Width = 600;
			// 
			// tabPageTextures
			// 
			this.tabPageTextures.Controls.Add(this.panelFilterTextures);
			this.tabPageTextures.Controls.Add(this.listViewTextures);
			this.tabPageTextures.Location = new System.Drawing.Point(4, 22);
			this.tabPageTextures.Name = "tabPageTextures";
			this.tabPageTextures.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageTextures.Size = new System.Drawing.Size(1291, 528);
			this.tabPageTextures.TabIndex = 1;
			this.tabPageTextures.Text = "Textures";
			this.tabPageTextures.UseVisualStyleBackColor = true;
			// 
			// panelFilterTextures
			// 
			this.panelFilterTextures.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelFilterTextures.Controls.Add(this.buttonSearchTexture);
			this.panelFilterTextures.Controls.Add(this.textBoxSearchTexture);
			this.panelFilterTextures.Controls.Add(this.label10);
			this.panelFilterTextures.Controls.Add(this.label6);
			this.panelFilterTextures.Controls.Add(this.integerTrackbarControlMinRefCount);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowTGA);
			this.panelFilterTextures.Controls.Add(this.labelTotalTextures);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowDiffuse);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowOtherFormats);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowNormal);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowPNG);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowGloss);
			this.panelFilterTextures.Controls.Add(this.checkBoxInvertFilters);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowMetal);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowDiffuseGloss);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowMasks);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowOther);
			this.panelFilterTextures.Controls.Add(this.checkBoxShowEmissive);
			this.panelFilterTextures.Location = new System.Drawing.Point(6, 443);
			this.panelFilterTextures.Name = "panelFilterTextures";
			this.panelFilterTextures.Size = new System.Drawing.Size(1279, 79);
			this.panelFilterTextures.TabIndex = 4;
			// 
			// buttonSearchTexture
			// 
			this.buttonSearchTexture.Location = new System.Drawing.Point(1046, 42);
			this.buttonSearchTexture.Name = "buttonSearchTexture";
			this.buttonSearchTexture.Size = new System.Drawing.Size(75, 23);
			this.buttonSearchTexture.TabIndex = 10;
			this.buttonSearchTexture.Text = "Search";
			this.buttonSearchTexture.UseVisualStyleBackColor = true;
			this.buttonSearchTexture.Click += new System.EventHandler(this.buttonSearchTexture_Click);
			// 
			// textBoxSearchTexture
			// 
			this.textBoxSearchTexture.Location = new System.Drawing.Point(720, 44);
			this.textBoxSearchTexture.Name = "textBoxSearchTexture";
			this.textBoxSearchTexture.Size = new System.Drawing.Size(320, 20);
			this.textBoxSearchTexture.TabIndex = 9;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(658, 47);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(56, 13);
			this.label10.TabIndex = 8;
			this.label10.Text = "Search for";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(194, 52);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(119, 13);
			this.label6.TabIndex = 7;
			this.label6.Text = "Show Ref Count Above";
			// 
			// integerTrackbarControlMinRefCount
			// 
			this.integerTrackbarControlMinRefCount.Location = new System.Drawing.Point(319, 47);
			this.integerTrackbarControlMinRefCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlMinRefCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlMinRefCount.Name = "integerTrackbarControlMinRefCount";
			this.integerTrackbarControlMinRefCount.RangeMax = 1000000;
			this.integerTrackbarControlMinRefCount.RangeMin = 0;
			this.integerTrackbarControlMinRefCount.Size = new System.Drawing.Size(111, 20);
			this.integerTrackbarControlMinRefCount.TabIndex = 6;
			this.integerTrackbarControlMinRefCount.Value = 0;
			this.integerTrackbarControlMinRefCount.VisibleRangeMax = 10;
			this.integerTrackbarControlMinRefCount.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlMinRefCount_ValueChanged);
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
			// labelTotalTextures
			// 
			this.labelTotalTextures.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelTotalTextures.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelTotalTextures.Location = new System.Drawing.Point(1183, 2);
			this.labelTotalTextures.Name = "labelTotalTextures";
			this.labelTotalTextures.Size = new System.Drawing.Size(93, 43);
			this.labelTotalTextures.TabIndex = 3;
			this.labelTotalTextures.Text = "Total Textures:";
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
			// checkBoxInvertFilters
			// 
			this.checkBoxInvertFilters.AutoSize = true;
			this.checkBoxInvertFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxInvertFilters.Location = new System.Drawing.Point(565, 28);
			this.checkBoxInvertFilters.Name = "checkBoxInvertFilters";
			this.checkBoxInvertFilters.Size = new System.Drawing.Size(97, 17);
			this.checkBoxInvertFilters.TabIndex = 2;
			this.checkBoxInvertFilters.Text = "Invert Filters";
			this.checkBoxInvertFilters.UseVisualStyleBackColor = true;
			this.checkBoxInvertFilters.CheckedChanged += new System.EventHandler(this.checkBoxShowDiffuse_CheckedChanged);
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
			// checkBoxShowDiffuseGloss
			// 
			this.checkBoxShowDiffuseGloss.AutoSize = true;
			this.checkBoxShowDiffuseGloss.Checked = true;
			this.checkBoxShowDiffuseGloss.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowDiffuseGloss.Location = new System.Drawing.Point(197, 28);
			this.checkBoxShowDiffuseGloss.Name = "checkBoxShowDiffuseGloss";
			this.checkBoxShowDiffuseGloss.Size = new System.Drawing.Size(120, 17);
			this.checkBoxShowDiffuseGloss.TabIndex = 2;
			this.checkBoxShowDiffuseGloss.Text = "Show Diffuse/Gloss";
			this.checkBoxShowDiffuseGloss.UseVisualStyleBackColor = true;
			this.checkBoxShowDiffuseGloss.CheckedChanged += new System.EventHandler(this.checkBoxShowDiffuse_CheckedChanged);
			// 
			// checkBoxShowMasks
			// 
			this.checkBoxShowMasks.AutoSize = true;
			this.checkBoxShowMasks.Checked = true;
			this.checkBoxShowMasks.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowMasks.Location = new System.Drawing.Point(565, 5);
			this.checkBoxShowMasks.Name = "checkBoxShowMasks";
			this.checkBoxShowMasks.Size = new System.Drawing.Size(87, 17);
			this.checkBoxShowMasks.TabIndex = 2;
			this.checkBoxShowMasks.Text = "Show Masks";
			this.checkBoxShowMasks.UseVisualStyleBackColor = true;
			this.checkBoxShowMasks.CheckedChanged += new System.EventHandler(this.checkBoxShowDiffuse_CheckedChanged);
			// 
			// checkBoxShowOther
			// 
			this.checkBoxShowOther.AutoSize = true;
			this.checkBoxShowOther.Checked = true;
			this.checkBoxShowOther.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowOther.Location = new System.Drawing.Point(426, 28);
			this.checkBoxShowOther.Name = "checkBoxShowOther";
			this.checkBoxShowOther.Size = new System.Drawing.Size(82, 17);
			this.checkBoxShowOther.TabIndex = 2;
			this.checkBoxShowOther.Text = "Show Other";
			this.checkBoxShowOther.UseVisualStyleBackColor = true;
			this.checkBoxShowOther.CheckedChanged += new System.EventHandler(this.checkBoxShowDiffuse_CheckedChanged);
			// 
			// checkBoxShowEmissive
			// 
			this.checkBoxShowEmissive.AutoSize = true;
			this.checkBoxShowEmissive.Checked = true;
			this.checkBoxShowEmissive.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowEmissive.Location = new System.Drawing.Point(323, 28);
			this.checkBoxShowEmissive.Name = "checkBoxShowEmissive";
			this.checkBoxShowEmissive.Size = new System.Drawing.Size(97, 17);
			this.checkBoxShowEmissive.TabIndex = 2;
			this.checkBoxShowEmissive.Text = "Show Emissive";
			this.checkBoxShowEmissive.UseVisualStyleBackColor = true;
			this.checkBoxShowEmissive.CheckedChanged += new System.EventHandler(this.checkBoxShowDiffuse_CheckedChanged);
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
            this.columnHeaderChannels,
            this.columnHeaderMaterialsReferencesCount});
			this.listViewTextures.FullRowSelect = true;
			this.listViewTextures.GridLines = true;
			this.listViewTextures.HideSelection = false;
			this.listViewTextures.Location = new System.Drawing.Point(6, 6);
			this.listViewTextures.Name = "listViewTextures";
			this.listViewTextures.Size = new System.Drawing.Size(1279, 431);
			this.listViewTextures.TabIndex = 1;
			this.listViewTextures.UseCompatibleStateImageBehavior = false;
			this.listViewTextures.View = System.Windows.Forms.View.Details;
			this.listViewTextures.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewTextures_ColumnClick);
			this.listViewTextures.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewTextures_MouseDoubleClick);
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
			// columnHeaderChannels
			// 
			this.columnHeaderChannels.Text = "Channels";
			// 
			// columnHeaderMaterialsReferencesCount
			// 
			this.columnHeaderMaterialsReferencesCount.Text = "Mat. Ref Count";
			this.columnHeaderMaterialsReferencesCount.Width = 100;
			// 
			// buttonReExport
			// 
			this.buttonReExport.Enabled = false;
			this.buttonReExport.Location = new System.Drawing.Point(530, 80);
			this.buttonReExport.Name = "buttonReExport";
			this.buttonReExport.Size = new System.Drawing.Size(75, 23);
			this.buttonReExport.TabIndex = 4;
			this.buttonReExport.Text = "Re-Export";
			this.toolTip.SetToolTip(this.buttonReExport, "Cleans, optimizies and re-exports brand new M2 materials into the target director" +
        "y, created M2 files follow the same directory hierarchy as original M2 files.");
			this.buttonReExport.UseVisualStyleBackColor = true;
			this.buttonReExport.Click += new System.EventHandler(this.buttonReExport_Click);
			// 
			// buttonParseReExportedMaterials
			// 
			this.buttonParseReExportedMaterials.Enabled = false;
			this.buttonParseReExportedMaterials.Location = new System.Drawing.Point(611, 80);
			this.buttonParseReExportedMaterials.Name = "buttonParseReExportedMaterials";
			this.buttonParseReExportedMaterials.Size = new System.Drawing.Size(75, 23);
			this.buttonParseReExportedMaterials.TabIndex = 4;
			this.buttonParseReExportedMaterials.Text = "Parse";
			this.toolTip.SetToolTip(this.buttonParseReExportedMaterials, "Parses all the cleaned-up, optimized and re-exported M2 files found in the re-exp" +
        "ort directory and sub-directories");
			this.buttonParseReExportedMaterials.UseVisualStyleBackColor = true;
			this.buttonParseReExportedMaterials.Click += new System.EventHandler(this.buttonParseReExportedMaterials_Click);
			// 
			// buttonCollect_dgTextures
			// 
			this.buttonCollect_dgTextures.Location = new System.Drawing.Point(611, 48);
			this.buttonCollect_dgTextures.Name = "buttonCollect_dgTextures";
			this.buttonCollect_dgTextures.Size = new System.Drawing.Size(75, 23);
			this.buttonCollect_dgTextures.TabIndex = 4;
			this.buttonCollect_dgTextures.Text = "Collect _dg";
			this.toolTip.SetToolTip(this.buttonCollect_dgTextures, "Collects only \"_dg\"-suffixed textures");
			this.buttonCollect_dgTextures.UseVisualStyleBackColor = true;
			this.buttonCollect_dgTextures.Click += new System.EventHandler(this.buttonCollect_dgTextures_Click);
			// 
			// buttonGenerate_dgTextures
			// 
			this.buttonGenerate_dgTextures.Enabled = false;
			this.buttonGenerate_dgTextures.Location = new System.Drawing.Point(692, 48);
			this.buttonGenerate_dgTextures.Name = "buttonGenerate_dgTextures";
			this.buttonGenerate_dgTextures.Size = new System.Drawing.Size(91, 23);
			this.buttonGenerate_dgTextures.TabIndex = 4;
			this.buttonGenerate_dgTextures.Text = "Generate _dg";
			this.toolTip.SetToolTip(this.buttonGenerate_dgTextures, "Generates all the missing _dg textures referenced by optimized materials");
			this.buttonGenerate_dgTextures.UseVisualStyleBackColor = true;
			this.buttonGenerate_dgTextures.Click += new System.EventHandler(this.buttonGenerate_dgTextures_Click);
			// 
			// buttonIntegratePerforce
			// 
			this.buttonIntegratePerforce.Enabled = false;
			this.buttonIntegratePerforce.Location = new System.Drawing.Point(530, 106);
			this.buttonIntegratePerforce.Name = "buttonIntegratePerforce";
			this.buttonIntegratePerforce.Size = new System.Drawing.Size(75, 23);
			this.buttonIntegratePerforce.TabIndex = 4;
			this.buttonIntegratePerforce.Text = "Integrate";
			this.toolTip.SetToolTip(this.buttonIntegratePerforce, "Integrates the optimized M2 and generated _dg files into perforce");
			this.buttonIntegratePerforce.UseVisualStyleBackColor = true;
			this.buttonIntegratePerforce.Click += new System.EventHandler(this.buttonIntegratePerforce_Click);
			// 
			// progressBarMaterials
			// 
			this.progressBarMaterials.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBarMaterials.Location = new System.Drawing.Point(611, 22);
			this.progressBarMaterials.Name = "progressBarMaterials";
			this.progressBarMaterials.Size = new System.Drawing.Size(700, 23);
			this.progressBarMaterials.TabIndex = 6;
			this.progressBarMaterials.Visible = false;
			// 
			// progressBarTextures
			// 
			this.progressBarTextures.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBarTextures.Location = new System.Drawing.Point(789, 48);
			this.progressBarTextures.Name = "progressBarTextures";
			this.progressBarTextures.Size = new System.Drawing.Size(522, 23);
			this.progressBarTextures.TabIndex = 6;
			this.progressBarTextures.Visible = false;
			// 
			// tabControlInfo
			// 
			this.tabControlInfo.Controls.Add(this.tabPageInfo);
			this.tabControlInfo.Controls.Add(this.tabPageLog);
			this.tabControlInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlInfo.Location = new System.Drawing.Point(0, 0);
			this.tabControlInfo.Name = "tabControlInfo";
			this.tabControlInfo.SelectedIndex = 0;
			this.tabControlInfo.Size = new System.Drawing.Size(1299, 164);
			this.tabControlInfo.TabIndex = 7;
			// 
			// tabPageInfo
			// 
			this.tabPageInfo.Controls.Add(this.textBoxInfo);
			this.tabPageInfo.Location = new System.Drawing.Point(4, 22);
			this.tabPageInfo.Name = "tabPageInfo";
			this.tabPageInfo.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageInfo.Size = new System.Drawing.Size(1291, 138);
			this.tabPageInfo.TabIndex = 0;
			this.tabPageInfo.Text = "Info";
			this.tabPageInfo.UseVisualStyleBackColor = true;
			// 
			// textBoxInfo
			// 
			this.textBoxInfo.AcceptsReturn = true;
			this.textBoxInfo.AcceptsTab = true;
			this.textBoxInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxInfo.Location = new System.Drawing.Point(3, 3);
			this.textBoxInfo.Multiline = true;
			this.textBoxInfo.Name = "textBoxInfo";
			this.textBoxInfo.ReadOnly = true;
			this.textBoxInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxInfo.Size = new System.Drawing.Size(1285, 132);
			this.textBoxInfo.TabIndex = 2;
			// 
			// tabPageLog
			// 
			this.tabPageLog.Controls.Add(this.textBoxLog);
			this.tabPageLog.Location = new System.Drawing.Point(4, 22);
			this.tabPageLog.Name = "tabPageLog";
			this.tabPageLog.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageLog.Size = new System.Drawing.Size(1291, 138);
			this.tabPageLog.TabIndex = 1;
			this.tabPageLog.Text = "Log";
			this.tabPageLog.UseVisualStyleBackColor = true;
			// 
			// textBoxLog
			// 
			this.textBoxLog.AcceptsReturn = true;
			this.textBoxLog.AcceptsTab = true;
			this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxLog.Location = new System.Drawing.Point(3, 3);
			this.textBoxLog.Multiline = true;
			this.textBoxLog.Name = "textBoxLog";
			this.textBoxLog.ReadOnly = true;
			this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxLog.Size = new System.Drawing.Size(1285, 132);
			this.textBoxLog.TabIndex = 1;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = new System.Drawing.Point(12, 135);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.tabControl);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.tabControlInfo);
			this.splitContainer1.Size = new System.Drawing.Size(1299, 722);
			this.splitContainer1.SplitterDistance = 554;
			this.splitContainer1.TabIndex = 8;
			// 
			// textBoxReExportPath
			// 
			this.textBoxReExportPath.Location = new System.Drawing.Point(143, 82);
			this.textBoxReExportPath.Name = "textBoxReExportPath";
			this.textBoxReExportPath.Size = new System.Drawing.Size(339, 20);
			this.textBoxReExportPath.TabIndex = 1;
			this.textBoxReExportPath.Text = "V:\\Test";
			// 
			// buttonSetMaterialsReExportPath
			// 
			this.buttonSetMaterialsReExportPath.Location = new System.Drawing.Point(488, 80);
			this.buttonSetMaterialsReExportPath.Name = "buttonSetMaterialsReExportPath";
			this.buttonSetMaterialsReExportPath.Size = new System.Drawing.Size(36, 23);
			this.buttonSetMaterialsReExportPath.TabIndex = 2;
			this.buttonSetMaterialsReExportPath.Text = "...";
			this.buttonSetMaterialsReExportPath.UseVisualStyleBackColor = true;
			this.buttonSetMaterialsReExportPath.Click += new System.EventHandler(this.buttonSetMaterialsReExportPath_Click);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(13, 85);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(124, 13);
			this.label9.TabIndex = 3;
			this.label9.Text = "Materials Re-Export Path";
			// 
			// progressBarReExportMaterials
			// 
			this.progressBarReExportMaterials.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBarReExportMaterials.Location = new System.Drawing.Point(692, 80);
			this.progressBarReExportMaterials.Name = "progressBarReExportMaterials";
			this.progressBarReExportMaterials.Size = new System.Drawing.Size(619, 23);
			this.progressBarReExportMaterials.TabIndex = 6;
			this.progressBarReExportMaterials.Visible = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.radioButtonViewOptimizedMaterials);
			this.panel1.Controls.Add(this.radioButtonViewSourceMaterials);
			this.panel1.Location = new System.Drawing.Point(12, 106);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(307, 22);
			this.panel1.TabIndex = 9;
			// 
			// radioButtonViewOptimizedMaterials
			// 
			this.radioButtonViewOptimizedMaterials.AutoSize = true;
			this.radioButtonViewOptimizedMaterials.Location = new System.Drawing.Point(136, 3);
			this.radioButtonViewOptimizedMaterials.Name = "radioButtonViewOptimizedMaterials";
			this.radioButtonViewOptimizedMaterials.Size = new System.Drawing.Size(142, 17);
			this.radioButtonViewOptimizedMaterials.TabIndex = 0;
			this.radioButtonViewOptimizedMaterials.Text = "View Optimized Materials";
			this.radioButtonViewOptimizedMaterials.UseVisualStyleBackColor = true;
			this.radioButtonViewOptimizedMaterials.CheckedChanged += new System.EventHandler(this.radioButtonViewMaterialsList_CheckedChanged);
			// 
			// radioButtonViewSourceMaterials
			// 
			this.radioButtonViewSourceMaterials.AutoSize = true;
			this.radioButtonViewSourceMaterials.Checked = true;
			this.radioButtonViewSourceMaterials.Location = new System.Drawing.Point(0, 3);
			this.radioButtonViewSourceMaterials.Name = "radioButtonViewSourceMaterials";
			this.radioButtonViewSourceMaterials.Size = new System.Drawing.Size(130, 17);
			this.radioButtonViewSourceMaterials.TabIndex = 0;
			this.radioButtonViewSourceMaterials.TabStop = true;
			this.radioButtonViewSourceMaterials.Text = "View Source Materials";
			this.radioButtonViewSourceMaterials.UseVisualStyleBackColor = true;
			this.radioButtonViewSourceMaterials.CheckedChanged += new System.EventHandler(this.radioButtonViewMaterialsList_CheckedChanged);
			// 
			// progressBarIntegrate
			// 
			this.progressBarIntegrate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBarIntegrate.Location = new System.Drawing.Point(692, 106);
			this.progressBarIntegrate.Name = "progressBarIntegrate";
			this.progressBarIntegrate.Size = new System.Drawing.Size(619, 23);
			this.progressBarIntegrate.TabIndex = 6;
			this.progressBarIntegrate.Visible = false;
			// 
			// buttonCreateTestMap
			// 
			this.buttonCreateTestMap.Enabled = false;
			this.buttonCreateTestMap.Location = new System.Drawing.Point(611, 106);
			this.buttonCreateTestMap.Name = "buttonCreateTestMap";
			this.buttonCreateTestMap.Size = new System.Drawing.Size(75, 23);
			this.buttonCreateTestMap.TabIndex = 4;
			this.buttonCreateTestMap.Text = "Test Map";
			this.toolTip.SetToolTip(this.buttonCreateTestMap, "Creates a test map with all the optimized materials containing merged (diffuse+gl" +
        "oss) textures");
			this.buttonCreateTestMap.UseVisualStyleBackColor = true;
			this.buttonCreateTestMap.Click += new System.EventHandler(this.buttonCreateTestMap_Click);
			// 
			// saveFileDialogTestMap
			// 
			this.saveFileDialogTestMap.DefaultExt = "*.map";
			this.saveFileDialogTestMap.Filter = "VoidEngine Map File (*.map)|*.map|All Files (*.*)|*.*";
			this.saveFileDialogTestMap.Title = "Indicate the test map file name to save";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1323, 869);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.progressBarTextures);
			this.Controls.Add(this.progressBarReExportMaterials);
			this.Controls.Add(this.progressBarIntegrate);
			this.Controls.Add(this.progressBarMaterials);
			this.Controls.Add(this.buttonCollect_dgTextures);
			this.Controls.Add(this.buttonCollectTextures);
			this.Controls.Add(this.buttonGenerate_dgTextures);
			this.Controls.Add(this.buttonReExport);
			this.Controls.Add(this.buttonCreateTestMap);
			this.Controls.Add(this.buttonIntegratePerforce);
			this.Controls.Add(this.buttonParseReExportedMaterials);
			this.Controls.Add(this.buttonParseMaterials);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonSetMaterialsReExportPath);
			this.Controls.Add(this.buttonSetTexturesBasePath);
			this.Controls.Add(this.buttonSetMaterialsBasePath);
			this.Controls.Add(this.textBoxReExportPath);
			this.Controls.Add(this.textBoxTexturesBasePath);
			this.Controls.Add(this.textBoxMaterialsBasePath);
			this.Name = "Form1";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Materials Optimizer";
			this.tabControl.ResumeLayout(false);
			this.tabPageMaterials.ResumeLayout(false);
			this.panelFilterMaterials.ResumeLayout(false);
			this.panelFilterMaterials.PerformLayout();
			this.tabPageTextures.ResumeLayout(false);
			this.panelFilterTextures.ResumeLayout(false);
			this.panelFilterTextures.PerformLayout();
			this.tabControlInfo.ResumeLayout(false);
			this.tabPageInfo.ResumeLayout(false);
			this.tabPageInfo.PerformLayout();
			this.tabPageLog.ResumeLayout(false);
			this.tabPageLog.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

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
		private System.Windows.Forms.ColumnHeader columnHeaderIsOptimized;
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
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlLayerMin;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlLayerMax;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.Label label6;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlMinRefCount;
		private System.Windows.Forms.ProgressBar progressBarMaterials;
		private System.Windows.Forms.ProgressBar progressBarTextures;
		private System.Windows.Forms.CheckBox checkBoxShowOptimizableMaterials;
		private System.Windows.Forms.TabControl tabControlInfo;
		private System.Windows.Forms.TabPage tabPageInfo;
		private System.Windows.Forms.TabPage tabPageLog;
		private System.Windows.Forms.TextBox textBoxLog;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TextBox textBoxInfo;
		private System.Windows.Forms.TextBox textBoxSearchMaterial;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button buttonSearch;
		private System.Windows.Forms.CheckBox checkBoxShowWarningMaterials;
		private System.Windows.Forms.CheckBox checkBoxShowMissingPhysics;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlErrorLevel;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.ColumnHeader columnHeaderChannels;
		private System.Windows.Forms.CheckBox checkBoxShowMasks;
		private System.Windows.Forms.Button buttonReExport;
		private System.Windows.Forms.TextBox textBoxReExportPath;
		private System.Windows.Forms.Button buttonSetMaterialsReExportPath;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.ProgressBar progressBarReExportMaterials;
		private System.Windows.Forms.Button buttonParseReExportedMaterials;
		private System.Windows.Forms.Button buttonIntegratePerforce;
		private System.Windows.Forms.Button buttonSearchTexture;
		private System.Windows.Forms.TextBox textBoxSearchTexture;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Button buttonAnalyzeConstantColorTextures;
		private System.Windows.Forms.CheckBox checkBoxShowAlpha;
		private System.Windows.Forms.Button buttonCollect_dgTextures;
		private System.Windows.Forms.Button buttonGenerate_dgTextures;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.RadioButton radioButtonViewSourceMaterials;
		private System.Windows.Forms.RadioButton radioButtonViewOptimizedMaterials;
		private System.Windows.Forms.CheckBox checkBoxShowDiffuseGloss;
		private System.Windows.Forms.ProgressBar progressBarIntegrate;
		private System.Windows.Forms.Button buttonCreateTestMap;
		private System.Windows.Forms.SaveFileDialog saveFileDialogTestMap;
	}
}

