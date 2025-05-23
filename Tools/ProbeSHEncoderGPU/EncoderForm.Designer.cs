﻿using ProbeSHEncoder;

namespace ProbeSHEncoder
{
	partial class EncoderForm
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
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadProbeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.batchEncodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.encodeFaceProbesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.buttonCompute = new System.Windows.Forms.Button();
			this.radioButtonAlbedo = new System.Windows.Forms.RadioButton();
			this.radioButtonDistance = new System.Windows.Forms.RadioButton();
			this.radioButtonSetIndex = new System.Windows.Forms.RadioButton();
			this.radioButtonSetColor = new System.Windows.Forms.RadioButton();
			this.radioButtonNormal = new System.Windows.Forms.RadioButton();
			this.radioButtonSetDistance = new System.Windows.Forms.RadioButton();
			this.radioButtonSetNormal = new System.Windows.Forms.RadioButton();
			this.textBoxResults = new System.Windows.Forms.TextBox();
			this.floatTrackbarControlAlbedo = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlNormal = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlPosition = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.integerTrackbarControlSetIsolation = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.checkBoxSetIsolation = new System.Windows.Forms.CheckBox();
			this.integerTrackbarControlK = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLambda = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.buttonComputeFilling = new System.Windows.Forms.Button();
			this.radioButtonSHStatic = new System.Windows.Forms.RadioButton();
			this.label6 = new System.Windows.Forms.Label();
			this.integerTrackbarControlLightSamples = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.radioButtonSetSamples = new System.Windows.Forms.RadioButton();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.progressBarBatchConvert = new System.Windows.Forms.ProgressBar();
			this.radioButtonStaticLit = new System.Windows.Forms.RadioButton();
			this.radioButtonEmissiveMatID = new System.Windows.Forms.RadioButton();
			this.checkBoxSHStatic = new System.Windows.Forms.CheckBox();
			this.checkBoxSHDynamic = new System.Windows.Forms.CheckBox();
			this.checkBoxSHEmissive = new System.Windows.Forms.CheckBox();
			this.checkBoxSHOcclusion = new System.Windows.Forms.CheckBox();
			this.checkBoxSHNormalized = new System.Windows.Forms.CheckBox();
			this.radioButtonFaceIndex = new System.Windows.Forms.RadioButton();
			this.buttonReset = new System.Windows.Forms.Button();
			this.radioButtonNeighborProbeID = new System.Windows.Forms.RadioButton();
			this.openFileDialogProbeInfluence = new System.Windows.Forms.OpenFileDialog();
			this.openFileDialogScene = new System.Windows.Forms.OpenFileDialog();
			this.progressBarSubTask = new System.Windows.Forms.ProgressBar();
			this.outputPanel1 = new ProbeSHEncoder.OutputPanel(this.components);
			this.menuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip
			// 
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem});
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(1027, 24);
			this.menuStrip.TabIndex = 1;
			this.menuStrip.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadProbeToolStripMenuItem,
            this.saveResultsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.batchEncodeToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// loadProbeToolStripMenuItem
			// 
			this.loadProbeToolStripMenuItem.Name = "loadProbeToolStripMenuItem";
			this.loadProbeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.loadProbeToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
			this.loadProbeToolStripMenuItem.Text = "&Load Probe";
			this.loadProbeToolStripMenuItem.Click += new System.EventHandler(this.loadProbeToolStripMenuItem_Click);
			// 
			// saveResultsToolStripMenuItem
			// 
			this.saveResultsToolStripMenuItem.Enabled = false;
			this.saveResultsToolStripMenuItem.Name = "saveResultsToolStripMenuItem";
			this.saveResultsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveResultsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
			this.saveResultsToolStripMenuItem.Text = "&Save Results";
			this.saveResultsToolStripMenuItem.Click += new System.EventHandler(this.saveResultsToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(184, 6);
			// 
			// batchEncodeToolStripMenuItem
			// 
			this.batchEncodeToolStripMenuItem.Name = "batchEncodeToolStripMenuItem";
			this.batchEncodeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
			this.batchEncodeToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
			this.batchEncodeToolStripMenuItem.Text = "&Batch Encode";
			this.batchEncodeToolStripMenuItem.Click += new System.EventHandler(this.batchEncodeToolStripMenuItem_Click);
			// 
			// toolsToolStripMenuItem
			// 
			this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.encodeFaceProbesToolStripMenuItem});
			this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
			this.toolsToolStripMenuItem.Text = "&Tools";
			// 
			// encodeFaceProbesToolStripMenuItem
			// 
			this.encodeFaceProbesToolStripMenuItem.Name = "encodeFaceProbesToolStripMenuItem";
			this.encodeFaceProbesToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
			this.encodeFaceProbesToolStripMenuItem.Text = "&Encode Face Probes";
			this.encodeFaceProbesToolStripMenuItem.Click += new System.EventHandler(this.encodeFaceProbesToolStripMenuItem_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "pom";
			this.openFileDialog.Filter = "Probe File (*.pom)|*.pom|All Files (*.*)|*.*";
			this.openFileDialog.RestoreDirectory = true;
			this.openFileDialog.Title = "Choose a probe file to convert...";
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.DefaultExt = "probesets";
			this.saveFileDialog.Filter = "Probe Sets File (*.probeset)|*.probeset|All Files (*.*)|*.*";
			this.saveFileDialog.Title = "Choose a target file to save the encoded probe to...";
			// 
			// buttonCompute
			// 
			this.buttonCompute.Enabled = false;
			this.buttonCompute.Location = new System.Drawing.Point(776, 109);
			this.buttonCompute.Name = "buttonCompute";
			this.buttonCompute.Size = new System.Drawing.Size(97, 38);
			this.buttonCompute.TabIndex = 2;
			this.buttonCompute.Text = "Compute k-Means";
			this.buttonCompute.UseVisualStyleBackColor = true;
			this.buttonCompute.Visible = false;
			// 
			// radioButtonAlbedo
			// 
			this.radioButtonAlbedo.AutoSize = true;
			this.radioButtonAlbedo.Checked = true;
			this.radioButtonAlbedo.Location = new System.Drawing.Point(712, 195);
			this.radioButtonAlbedo.Name = "radioButtonAlbedo";
			this.radioButtonAlbedo.Size = new System.Drawing.Size(58, 17);
			this.radioButtonAlbedo.TabIndex = 4;
			this.radioButtonAlbedo.TabStop = true;
			this.radioButtonAlbedo.Text = "Albedo";
			this.radioButtonAlbedo.UseVisualStyleBackColor = true;
			this.radioButtonAlbedo.CheckedChanged += new System.EventHandler(this.radioButtonAlbedo_CheckedChanged);
			// 
			// radioButtonDistance
			// 
			this.radioButtonDistance.AutoSize = true;
			this.radioButtonDistance.Location = new System.Drawing.Point(712, 218);
			this.radioButtonDistance.Name = "radioButtonDistance";
			this.radioButtonDistance.Size = new System.Drawing.Size(67, 17);
			this.radioButtonDistance.TabIndex = 4;
			this.radioButtonDistance.Text = "Distance";
			this.radioButtonDistance.UseVisualStyleBackColor = true;
			this.radioButtonDistance.CheckedChanged += new System.EventHandler(this.radioButtonDistance_CheckedChanged);
			// 
			// radioButtonSetIndex
			// 
			this.radioButtonSetIndex.AutoSize = true;
			this.radioButtonSetIndex.Location = new System.Drawing.Point(712, 356);
			this.radioButtonSetIndex.Name = "radioButtonSetIndex";
			this.radioButtonSetIndex.Size = new System.Drawing.Size(70, 17);
			this.radioButtonSetIndex.TabIndex = 4;
			this.radioButtonSetIndex.Text = "Set Index";
			this.radioButtonSetIndex.UseVisualStyleBackColor = true;
			this.radioButtonSetIndex.CheckedChanged += new System.EventHandler(this.radioButtonSetIndex_CheckedChanged);
			// 
			// radioButtonSetColor
			// 
			this.radioButtonSetColor.AutoSize = true;
			this.radioButtonSetColor.Location = new System.Drawing.Point(712, 379);
			this.radioButtonSetColor.Name = "radioButtonSetColor";
			this.radioButtonSetColor.Size = new System.Drawing.Size(77, 17);
			this.radioButtonSetColor.TabIndex = 4;
			this.radioButtonSetColor.Text = "Set Albedo";
			this.radioButtonSetColor.UseVisualStyleBackColor = true;
			this.radioButtonSetColor.CheckedChanged += new System.EventHandler(this.radioButtonSetColor_CheckedChanged);
			// 
			// radioButtonNormal
			// 
			this.radioButtonNormal.AutoSize = true;
			this.radioButtonNormal.Location = new System.Drawing.Point(712, 241);
			this.radioButtonNormal.Name = "radioButtonNormal";
			this.radioButtonNormal.Size = new System.Drawing.Size(58, 17);
			this.radioButtonNormal.TabIndex = 4;
			this.radioButtonNormal.Text = "Normal";
			this.radioButtonNormal.UseVisualStyleBackColor = true;
			this.radioButtonNormal.CheckedChanged += new System.EventHandler(this.radioButtonNormal_CheckedChanged);
			// 
			// radioButtonSetDistance
			// 
			this.radioButtonSetDistance.AutoSize = true;
			this.radioButtonSetDistance.Location = new System.Drawing.Point(712, 402);
			this.radioButtonSetDistance.Name = "radioButtonSetDistance";
			this.radioButtonSetDistance.Size = new System.Drawing.Size(86, 17);
			this.radioButtonSetDistance.TabIndex = 4;
			this.radioButtonSetDistance.Text = "Set Distance";
			this.radioButtonSetDistance.UseVisualStyleBackColor = true;
			this.radioButtonSetDistance.CheckedChanged += new System.EventHandler(this.radioButtonSetDistance_CheckedChanged);
			// 
			// radioButtonSetNormal
			// 
			this.radioButtonSetNormal.AutoSize = true;
			this.radioButtonSetNormal.Location = new System.Drawing.Point(712, 425);
			this.radioButtonSetNormal.Name = "radioButtonSetNormal";
			this.radioButtonSetNormal.Size = new System.Drawing.Size(77, 17);
			this.radioButtonSetNormal.TabIndex = 4;
			this.radioButtonSetNormal.Text = "Set Normal";
			this.radioButtonSetNormal.UseVisualStyleBackColor = true;
			this.radioButtonSetNormal.CheckedChanged += new System.EventHandler(this.radioButtonSetNormal_CheckedChanged);
			// 
			// textBoxResults
			// 
			this.textBoxResults.Location = new System.Drawing.Point(827, 130);
			this.textBoxResults.Multiline = true;
			this.textBoxResults.Name = "textBoxResults";
			this.textBoxResults.ReadOnly = true;
			this.textBoxResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxResults.Size = new System.Drawing.Size(188, 259);
			this.textBoxResults.TabIndex = 5;
			// 
			// floatTrackbarControlAlbedo
			// 
			this.floatTrackbarControlAlbedo.Location = new System.Drawing.Point(711, 665);
			this.floatTrackbarControlAlbedo.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlAlbedo.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlAlbedo.Name = "floatTrackbarControlAlbedo";
			this.floatTrackbarControlAlbedo.RangeMax = 100F;
			this.floatTrackbarControlAlbedo.RangeMin = 0F;
			this.floatTrackbarControlAlbedo.Size = new System.Drawing.Size(303, 20);
			this.floatTrackbarControlAlbedo.TabIndex = 6;
			this.floatTrackbarControlAlbedo.Value = 1F;
			this.floatTrackbarControlAlbedo.VisibleRangeMax = 1F;
			this.floatTrackbarControlAlbedo.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlAlbedo_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(708, 649);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(150, 13);
			this.label1.TabIndex = 7;
			this.label1.Text = "Albedo Separation Importance";
			// 
			// floatTrackbarControlNormal
			// 
			this.floatTrackbarControlNormal.Location = new System.Drawing.Point(711, 626);
			this.floatTrackbarControlNormal.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlNormal.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlNormal.Name = "floatTrackbarControlNormal";
			this.floatTrackbarControlNormal.RangeMax = 100F;
			this.floatTrackbarControlNormal.RangeMin = 0F;
			this.floatTrackbarControlNormal.Size = new System.Drawing.Size(303, 20);
			this.floatTrackbarControlNormal.TabIndex = 6;
			this.floatTrackbarControlNormal.Value = 1F;
			this.floatTrackbarControlNormal.VisibleRangeMax = 1F;
			this.floatTrackbarControlNormal.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlNormal_ValueChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(708, 610);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(150, 13);
			this.label2.TabIndex = 7;
			this.label2.Text = "Normal Separation Importance";
			// 
			// floatTrackbarControlPosition
			// 
			this.floatTrackbarControlPosition.Location = new System.Drawing.Point(712, 587);
			this.floatTrackbarControlPosition.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlPosition.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlPosition.Name = "floatTrackbarControlPosition";
			this.floatTrackbarControlPosition.RangeMax = 100F;
			this.floatTrackbarControlPosition.RangeMin = 0F;
			this.floatTrackbarControlPosition.Size = new System.Drawing.Size(303, 20);
			this.floatTrackbarControlPosition.TabIndex = 6;
			this.floatTrackbarControlPosition.Value = 1F;
			this.floatTrackbarControlPosition.VisibleRangeMax = 1F;
			this.floatTrackbarControlPosition.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlPosition_ValueChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(709, 571);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(149, 13);
			this.label3.TabIndex = 7;
			this.label3.Text = "Spatial Separation Importance";
			// 
			// integerTrackbarControlSetIsolation
			// 
			this.integerTrackbarControlSetIsolation.Location = new System.Drawing.Point(732, 517);
			this.integerTrackbarControlSetIsolation.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlSetIsolation.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlSetIsolation.Name = "integerTrackbarControlSetIsolation";
			this.integerTrackbarControlSetIsolation.RangeMax = 100;
			this.integerTrackbarControlSetIsolation.RangeMin = 0;
			this.integerTrackbarControlSetIsolation.Size = new System.Drawing.Size(248, 20);
			this.integerTrackbarControlSetIsolation.TabIndex = 8;
			this.integerTrackbarControlSetIsolation.Value = 0;
			this.integerTrackbarControlSetIsolation.VisibleRangeMax = 10;
			this.integerTrackbarControlSetIsolation.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlSetIsolation_ValueChanged);
			// 
			// checkBoxSetIsolation
			// 
			this.checkBoxSetIsolation.AutoSize = true;
			this.checkBoxSetIsolation.Location = new System.Drawing.Point(712, 494);
			this.checkBoxSetIsolation.Name = "checkBoxSetIsolation";
			this.checkBoxSetIsolation.Size = new System.Drawing.Size(76, 17);
			this.checkBoxSetIsolation.TabIndex = 9;
			this.checkBoxSetIsolation.Text = "Isolate Set";
			this.checkBoxSetIsolation.UseVisualStyleBackColor = true;
			this.checkBoxSetIsolation.CheckedChanged += new System.EventHandler(this.checkBoxSetIsolation_CheckedChanged);
			// 
			// integerTrackbarControlK
			// 
			this.integerTrackbarControlK.Location = new System.Drawing.Point(757, 31);
			this.integerTrackbarControlK.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlK.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlK.Name = "integerTrackbarControlK";
			this.integerTrackbarControlK.RangeMax = 128;
			this.integerTrackbarControlK.RangeMin = 1;
			this.integerTrackbarControlK.Size = new System.Drawing.Size(257, 20);
			this.integerTrackbarControlK.TabIndex = 8;
			this.integerTrackbarControlK.Value = 32;
			this.integerTrackbarControlK.VisibleRangeMax = 64;
			this.integerTrackbarControlK.VisibleRangeMin = 1;
			this.integerTrackbarControlK.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlK_ValueChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(695, 34);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(14, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "K";
			// 
			// floatTrackbarControlLambda
			// 
			this.floatTrackbarControlLambda.Location = new System.Drawing.Point(757, 57);
			this.floatTrackbarControlLambda.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLambda.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLambda.Name = "floatTrackbarControlLambda";
			this.floatTrackbarControlLambda.RangeMax = 1F;
			this.floatTrackbarControlLambda.RangeMin = 0F;
			this.floatTrackbarControlLambda.Size = new System.Drawing.Size(258, 20);
			this.floatTrackbarControlLambda.TabIndex = 6;
			this.floatTrackbarControlLambda.Value = 0.5F;
			this.floatTrackbarControlLambda.VisibleRangeMax = 1F;
			this.floatTrackbarControlLambda.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlLambda_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(695, 60);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(60, 13);
			this.label5.TabIndex = 7;
			this.label5.Text = "Importance";
			// 
			// buttonComputeFilling
			// 
			this.buttonComputeFilling.Enabled = false;
			this.buttonComputeFilling.Location = new System.Drawing.Point(711, 151);
			this.buttonComputeFilling.Name = "buttonComputeFilling";
			this.buttonComputeFilling.Size = new System.Drawing.Size(97, 38);
			this.buttonComputeFilling.TabIndex = 2;
			this.buttonComputeFilling.Text = "Compute Filling";
			this.buttonComputeFilling.UseVisualStyleBackColor = true;
			this.buttonComputeFilling.Click += new System.EventHandler(this.buttonComputeFilling_Click);
			// 
			// radioButtonSHStatic
			// 
			this.radioButtonSHStatic.AutoSize = true;
			this.radioButtonSHStatic.Location = new System.Drawing.Point(712, 448);
			this.radioButtonSHStatic.Name = "radioButtonSHStatic";
			this.radioButtonSHStatic.Size = new System.Drawing.Size(73, 17);
			this.radioButtonSHStatic.TabIndex = 4;
			this.radioButtonSHStatic.Text = "Result SH";
			this.radioButtonSHStatic.UseVisualStyleBackColor = true;
			this.radioButtonSHStatic.CheckedChanged += new System.EventHandler(this.radioButtonSH_CheckedChanged);
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(695, 85);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(75, 31);
			this.label6.TabIndex = 7;
			this.label6.Text = "Amount of light samples";
			// 
			// integerTrackbarControlLightSamples
			// 
			this.integerTrackbarControlLightSamples.Location = new System.Drawing.Point(757, 83);
			this.integerTrackbarControlLightSamples.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlLightSamples.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlLightSamples.Name = "integerTrackbarControlLightSamples";
			this.integerTrackbarControlLightSamples.RangeMax = 256;
			this.integerTrackbarControlLightSamples.RangeMin = 1;
			this.integerTrackbarControlLightSamples.Size = new System.Drawing.Size(257, 20);
			this.integerTrackbarControlLightSamples.TabIndex = 8;
			this.integerTrackbarControlLightSamples.Value = 64;
			this.integerTrackbarControlLightSamples.VisibleRangeMax = 128;
			this.integerTrackbarControlLightSamples.VisibleRangeMin = 1;
			this.integerTrackbarControlLightSamples.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlLightSamples_ValueChanged);
			// 
			// radioButtonSetSamples
			// 
			this.radioButtonSetSamples.AutoSize = true;
			this.radioButtonSetSamples.Location = new System.Drawing.Point(712, 471);
			this.radioButtonSetSamples.Name = "radioButtonSetSamples";
			this.radioButtonSetSamples.Size = new System.Drawing.Size(84, 17);
			this.radioButtonSetSamples.TabIndex = 4;
			this.radioButtonSetSamples.Text = "Set Samples";
			this.radioButtonSetSamples.UseVisualStyleBackColor = true;
			this.radioButtonSetSamples.CheckedChanged += new System.EventHandler(this.radioButtonSetSamples_CheckedChanged);
			// 
			// folderBrowserDialog
			// 
			this.folderBrowserDialog.Description = "Select the folder to parse for POM files of probes shooting";
			// 
			// progressBarBatchConvert
			// 
			this.progressBarBatchConvert.Location = new System.Drawing.Point(12, 652);
			this.progressBarBatchConvert.Name = "progressBarBatchConvert";
			this.progressBarBatchConvert.Size = new System.Drawing.Size(677, 19);
			this.progressBarBatchConvert.TabIndex = 10;
			this.progressBarBatchConvert.Visible = false;
			// 
			// radioButtonStaticLit
			// 
			this.radioButtonStaticLit.AutoSize = true;
			this.radioButtonStaticLit.Location = new System.Drawing.Point(712, 264);
			this.radioButtonStaticLit.Name = "radioButtonStaticLit";
			this.radioButtonStaticLit.Size = new System.Drawing.Size(66, 17);
			this.radioButtonStaticLit.TabIndex = 4;
			this.radioButtonStaticLit.Text = "Static Lit";
			this.radioButtonStaticLit.UseVisualStyleBackColor = true;
			this.radioButtonStaticLit.CheckedChanged += new System.EventHandler(this.radioButtonStaticLit_CheckedChanged);
			// 
			// radioButtonEmissiveMatID
			// 
			this.radioButtonEmissiveMatID.AutoSize = true;
			this.radioButtonEmissiveMatID.Location = new System.Drawing.Point(712, 310);
			this.radioButtonEmissiveMatID.Name = "radioButtonEmissiveMatID";
			this.radioButtonEmissiveMatID.Size = new System.Drawing.Size(101, 17);
			this.radioButtonEmissiveMatID.TabIndex = 4;
			this.radioButtonEmissiveMatID.Text = "Emissive Mat ID";
			this.radioButtonEmissiveMatID.UseVisualStyleBackColor = true;
			this.radioButtonEmissiveMatID.CheckedChanged += new System.EventHandler(this.radioButtonEmissiveMatID_CheckedChanged);
			// 
			// checkBoxSHStatic
			// 
			this.checkBoxSHStatic.AutoSize = true;
			this.checkBoxSHStatic.Location = new System.Drawing.Point(791, 448);
			this.checkBoxSHStatic.Name = "checkBoxSHStatic";
			this.checkBoxSHStatic.Size = new System.Drawing.Size(53, 17);
			this.checkBoxSHStatic.TabIndex = 9;
			this.checkBoxSHStatic.Text = "Static";
			this.checkBoxSHStatic.UseVisualStyleBackColor = true;
			this.checkBoxSHStatic.CheckedChanged += new System.EventHandler(this.checkBoxSHStatic_CheckedChanged);
			// 
			// checkBoxSHDynamic
			// 
			this.checkBoxSHDynamic.AutoSize = true;
			this.checkBoxSHDynamic.Checked = true;
			this.checkBoxSHDynamic.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxSHDynamic.Location = new System.Drawing.Point(850, 448);
			this.checkBoxSHDynamic.Name = "checkBoxSHDynamic";
			this.checkBoxSHDynamic.Size = new System.Drawing.Size(67, 17);
			this.checkBoxSHDynamic.TabIndex = 9;
			this.checkBoxSHDynamic.Text = "Dynamic";
			this.checkBoxSHDynamic.UseVisualStyleBackColor = true;
			this.checkBoxSHDynamic.CheckedChanged += new System.EventHandler(this.checkBoxSHDynamic_CheckedChanged);
			// 
			// checkBoxSHEmissive
			// 
			this.checkBoxSHEmissive.AutoSize = true;
			this.checkBoxSHEmissive.Location = new System.Drawing.Point(923, 448);
			this.checkBoxSHEmissive.Name = "checkBoxSHEmissive";
			this.checkBoxSHEmissive.Size = new System.Drawing.Size(67, 17);
			this.checkBoxSHEmissive.TabIndex = 9;
			this.checkBoxSHEmissive.Text = "Emissive";
			this.checkBoxSHEmissive.UseVisualStyleBackColor = true;
			this.checkBoxSHEmissive.CheckedChanged += new System.EventHandler(this.checkBoxSHEmissive_CheckedChanged);
			// 
			// checkBoxSHOcclusion
			// 
			this.checkBoxSHOcclusion.AutoSize = true;
			this.checkBoxSHOcclusion.Location = new System.Drawing.Point(923, 471);
			this.checkBoxSHOcclusion.Name = "checkBoxSHOcclusion";
			this.checkBoxSHOcclusion.Size = new System.Drawing.Size(73, 17);
			this.checkBoxSHOcclusion.TabIndex = 9;
			this.checkBoxSHOcclusion.Text = "Occlusion";
			this.checkBoxSHOcclusion.UseVisualStyleBackColor = true;
			this.checkBoxSHOcclusion.CheckedChanged += new System.EventHandler(this.checkBoxSHOcclusion_CheckedChanged);
			// 
			// checkBoxSHNormalized
			// 
			this.checkBoxSHNormalized.AutoSize = true;
			this.checkBoxSHNormalized.Location = new System.Drawing.Point(850, 471);
			this.checkBoxSHNormalized.Name = "checkBoxSHNormalized";
			this.checkBoxSHNormalized.Size = new System.Drawing.Size(78, 17);
			this.checkBoxSHNormalized.TabIndex = 9;
			this.checkBoxSHNormalized.Text = "Normalized";
			this.checkBoxSHNormalized.UseVisualStyleBackColor = true;
			this.checkBoxSHNormalized.CheckedChanged += new System.EventHandler(this.checkBoxSHNormalized_CheckedChanged);
			// 
			// radioButtonFaceIndex
			// 
			this.radioButtonFaceIndex.AutoSize = true;
			this.radioButtonFaceIndex.Location = new System.Drawing.Point(712, 287);
			this.radioButtonFaceIndex.Name = "radioButtonFaceIndex";
			this.radioButtonFaceIndex.Size = new System.Drawing.Size(78, 17);
			this.radioButtonFaceIndex.TabIndex = 4;
			this.radioButtonFaceIndex.Text = "Face Index";
			this.radioButtonFaceIndex.UseVisualStyleBackColor = true;
			this.radioButtonFaceIndex.CheckedChanged += new System.EventHandler(this.radioButtonFaceIndex_CheckedChanged);
			// 
			// buttonReset
			// 
			this.buttonReset.Location = new System.Drawing.Point(948, 557);
			this.buttonReset.Name = "buttonReset";
			this.buttonReset.Size = new System.Drawing.Size(66, 24);
			this.buttonReset.TabIndex = 2;
			this.buttonReset.Text = "Reset";
			this.buttonReset.UseVisualStyleBackColor = true;
			this.buttonReset.Visible = false;
			this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
			// 
			// radioButtonNeighborProbeID
			// 
			this.radioButtonNeighborProbeID.AutoSize = true;
			this.radioButtonNeighborProbeID.Location = new System.Drawing.Point(712, 333);
			this.radioButtonNeighborProbeID.Name = "radioButtonNeighborProbeID";
			this.radioButtonNeighborProbeID.Size = new System.Drawing.Size(113, 17);
			this.radioButtonNeighborProbeID.TabIndex = 4;
			this.radioButtonNeighborProbeID.Text = "Neighbor Probe ID";
			this.radioButtonNeighborProbeID.UseVisualStyleBackColor = true;
			this.radioButtonNeighborProbeID.CheckedChanged += new System.EventHandler(this.radioButtonNeighborProbeID_CheckedChanged);
			// 
			// openFileDialogProbeInfluence
			// 
			this.openFileDialogProbeInfluence.DefaultExt = "pim";
			this.openFileDialogProbeInfluence.Filter = "Probe Influence File (*.pim)|*.pim|All Files (*.*)|*.*";
			this.openFileDialogProbeInfluence.RestoreDirectory = true;
			this.openFileDialogProbeInfluence.Title = "Choose a probe influence file to convert...";
			// 
			// openFileDialogScene
			// 
			this.openFileDialogScene.DefaultExt = "gcx";
			this.openFileDialogScene.Filter = "Scene File (*.gcx)|*.gcx|All Files (*.*)|*.*";
			this.openFileDialogScene.RestoreDirectory = true;
			this.openFileDialogScene.Title = "Choose a scene file to apply probe influences to...";
			// 
			// progressBarSubTask
			// 
			this.progressBarSubTask.Location = new System.Drawing.Point(12, 677);
			this.progressBarSubTask.Name = "progressBarSubTask";
			this.progressBarSubTask.Size = new System.Drawing.Size(677, 10);
			this.progressBarSubTask.TabIndex = 10;
			this.progressBarSubTask.Visible = false;
			// 
			// outputPanel1
			// 
			this.outputPanel1.IsolatedSetIndex = 0;
			this.outputPanel1.IsolateSet = false;
			this.outputPanel1.Location = new System.Drawing.Point(12, 27);
			this.outputPanel1.Name = "outputPanel1";
			this.outputPanel1.NormalizeSH = false;
			this.outputPanel1.Probe = null;
			this.outputPanel1.ShowSetAverage = false;
			this.outputPanel1.ShowSHDynamic = true;
			this.outputPanel1.ShowSHEmissive = false;
			this.outputPanel1.ShowSHOcclusion = false;
			this.outputPanel1.ShowSHStatic = false;
			this.outputPanel1.Size = new System.Drawing.Size(677, 619);
			this.outputPanel1.TabIndex = 3;
			this.outputPanel1.Viz = ProbeSHEncoder.OutputPanel.VIZ_TYPE.ALBEDO;
			// 
			// EncoderForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1027, 692);
			this.Controls.Add(this.checkBoxSHOcclusion);
			this.Controls.Add(this.progressBarSubTask);
			this.Controls.Add(this.progressBarBatchConvert);
			this.Controls.Add(this.checkBoxSHNormalized);
			this.Controls.Add(this.checkBoxSHEmissive);
			this.Controls.Add(this.checkBoxSHDynamic);
			this.Controls.Add(this.checkBoxSHStatic);
			this.Controls.Add(this.checkBoxSetIsolation);
			this.Controls.Add(this.integerTrackbarControlLightSamples);
			this.Controls.Add(this.integerTrackbarControlK);
			this.Controls.Add(this.integerTrackbarControlSetIsolation);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.floatTrackbarControlLambda);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlPosition);
			this.Controls.Add(this.floatTrackbarControlNormal);
			this.Controls.Add(this.floatTrackbarControlAlbedo);
			this.Controls.Add(this.textBoxResults);
			this.Controls.Add(this.radioButtonSetSamples);
			this.Controls.Add(this.radioButtonSHStatic);
			this.Controls.Add(this.radioButtonSetNormal);
			this.Controls.Add(this.radioButtonSetDistance);
			this.Controls.Add(this.radioButtonSetColor);
			this.Controls.Add(this.radioButtonSetIndex);
			this.Controls.Add(this.radioButtonNeighborProbeID);
			this.Controls.Add(this.radioButtonEmissiveMatID);
			this.Controls.Add(this.radioButtonFaceIndex);
			this.Controls.Add(this.radioButtonStaticLit);
			this.Controls.Add(this.radioButtonNormal);
			this.Controls.Add(this.radioButtonDistance);
			this.Controls.Add(this.radioButtonAlbedo);
			this.Controls.Add(this.outputPanel1);
			this.Controls.Add(this.buttonComputeFilling);
			this.Controls.Add(this.buttonReset);
			this.Controls.Add(this.buttonCompute);
			this.Controls.Add(this.menuStrip);
			this.MainMenuStrip = this.menuStrip;
			this.Name = "EncoderForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Probe SH Encoder";
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.Button buttonCompute;
		private OutputPanel outputPanel1;
		private System.Windows.Forms.RadioButton radioButtonAlbedo;
		private System.Windows.Forms.RadioButton radioButtonDistance;
		private System.Windows.Forms.RadioButton radioButtonSetIndex;
		private System.Windows.Forms.RadioButton radioButtonSetColor;
		private System.Windows.Forms.RadioButton radioButtonNormal;
		private System.Windows.Forms.RadioButton radioButtonSetDistance;
		private System.Windows.Forms.RadioButton radioButtonSetNormal;
		private System.Windows.Forms.TextBox textBoxResults;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlAlbedo;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlNormal;
		private System.Windows.Forms.Label label2;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlPosition;
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlSetIsolation;
		private System.Windows.Forms.CheckBox checkBoxSetIsolation;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlK;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLambda;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button buttonComputeFilling;
		private System.Windows.Forms.ToolStripMenuItem saveResultsToolStripMenuItem;
		private System.Windows.Forms.RadioButton radioButtonSHStatic;
		private System.Windows.Forms.ToolStripMenuItem loadProbeToolStripMenuItem;
		private System.Windows.Forms.Label label6;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlLightSamples;
		private System.Windows.Forms.RadioButton radioButtonSetSamples;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem batchEncodeToolStripMenuItem;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.ProgressBar progressBarBatchConvert;
		private System.Windows.Forms.RadioButton radioButtonStaticLit;
		private System.Windows.Forms.RadioButton radioButtonEmissiveMatID;
		private System.Windows.Forms.CheckBox checkBoxSHStatic;
		private System.Windows.Forms.CheckBox checkBoxSHDynamic;
		private System.Windows.Forms.CheckBox checkBoxSHEmissive;
		private System.Windows.Forms.CheckBox checkBoxSHOcclusion;
		private System.Windows.Forms.CheckBox checkBoxSHNormalized;
		private System.Windows.Forms.RadioButton radioButtonFaceIndex;
		private System.Windows.Forms.Button buttonReset;
		private System.Windows.Forms.RadioButton radioButtonNeighborProbeID;
		private System.Windows.Forms.ToolStripMenuItem encodeFaceProbesToolStripMenuItem;
		private System.Windows.Forms.OpenFileDialog openFileDialogProbeInfluence;
		private System.Windows.Forms.OpenFileDialog openFileDialogScene;
		private System.Windows.Forms.ProgressBar progressBarSubTask;
	}
}

