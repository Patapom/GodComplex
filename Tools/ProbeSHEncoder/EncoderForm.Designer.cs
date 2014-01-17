using ProbeSHEncoder;

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
			WMath.Vector vector1 = new WMath.Vector();
			WMath.Vector vector2 = new WMath.Vector();
			WMath.Vector vector3 = new WMath.Vector();
			WMath.Vector vector4 = new WMath.Vector();
			WMath.Vector vector5 = new WMath.Vector();
			WMath.Vector vector6 = new WMath.Vector();
			WMath.Vector vector7 = new WMath.Vector();
			WMath.Vector vector8 = new WMath.Vector();
			WMath.Vector vector9 = new WMath.Vector();
			WMath.Vector vector10 = new WMath.Vector();
			WMath.Vector vector11 = new WMath.Vector();
			WMath.Vector vector12 = new WMath.Vector();
			WMath.Vector vector13 = new WMath.Vector();
			WMath.Vector vector14 = new WMath.Vector();
			WMath.Vector vector15 = new WMath.Vector();
			WMath.Vector vector16 = new WMath.Vector();
			WMath.Vector vector17 = new WMath.Vector();
			WMath.Vector vector18 = new WMath.Vector();
			WMath.Vector vector19 = new WMath.Vector();
			WMath.Vector vector20 = new WMath.Vector();
			WMath.Vector vector21 = new WMath.Vector();
			WMath.Vector vector22 = new WMath.Vector();
			WMath.Vector vector23 = new WMath.Vector();
			WMath.Vector vector24 = new WMath.Vector();
			WMath.Vector vector25 = new WMath.Vector();
			WMath.Vector vector26 = new WMath.Vector();
			WMath.Vector vector27 = new WMath.Vector();
			WMath.Vector vector28 = new WMath.Vector();
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadProbeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.batchEncodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.convertShaderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
			this.outputPanel1 = new ProbeSHEncoder.OutputPanel(this.components);
			this.checkBoxSHNormalized = new System.Windows.Forms.CheckBox();
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
            this.convertShaderToolStripMenuItem});
			this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
			this.toolsToolStripMenuItem.Text = "Tools";
			// 
			// convertShaderToolStripMenuItem
			// 
			this.convertShaderToolStripMenuItem.Name = "convertShaderToolStripMenuItem";
			this.convertShaderToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
			this.convertShaderToolStripMenuItem.Text = "Convert Shader...";
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "pom";
			this.openFileDialog.Filter = "Probe File (*.pom)|*.pom|All Files (*.*)|*.*";
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
			this.buttonCompute.Location = new System.Drawing.Point(711, 130);
			this.buttonCompute.Name = "buttonCompute";
			this.buttonCompute.Size = new System.Drawing.Size(97, 38);
			this.buttonCompute.TabIndex = 2;
			this.buttonCompute.Text = "Compute k-Means";
			this.buttonCompute.UseVisualStyleBackColor = true;
			this.buttonCompute.Click += new System.EventHandler(this.buttonCompute_Click);
			// 
			// radioButtonAlbedo
			// 
			this.radioButtonAlbedo.AutoSize = true;
			this.radioButtonAlbedo.Checked = true;
			this.radioButtonAlbedo.Location = new System.Drawing.Point(712, 218);
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
			this.radioButtonDistance.Location = new System.Drawing.Point(712, 241);
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
			this.radioButtonSetIndex.Location = new System.Drawing.Point(711, 333);
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
			this.radioButtonSetColor.Location = new System.Drawing.Point(711, 356);
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
			this.radioButtonNormal.Location = new System.Drawing.Point(712, 264);
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
			this.radioButtonSetDistance.Location = new System.Drawing.Point(711, 379);
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
			this.radioButtonSetNormal.Location = new System.Drawing.Point(711, 402);
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
			this.integerTrackbarControlSetIsolation.Location = new System.Drawing.Point(731, 494);
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
			this.checkBoxSetIsolation.Location = new System.Drawing.Point(711, 471);
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
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(708, 34);
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
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(709, 60);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(45, 13);
			this.label5.TabIndex = 7;
			this.label5.Text = "Lambda";
			// 
			// buttonComputeFilling
			// 
			this.buttonComputeFilling.Location = new System.Drawing.Point(711, 174);
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
			this.radioButtonSHStatic.Location = new System.Drawing.Point(711, 425);
			this.radioButtonSHStatic.Name = "radioButtonSHStatic";
			this.radioButtonSHStatic.Size = new System.Drawing.Size(73, 17);
			this.radioButtonSHStatic.TabIndex = 4;
			this.radioButtonSHStatic.Text = "Result SH";
			this.radioButtonSHStatic.UseVisualStyleBackColor = true;
			this.radioButtonSHStatic.CheckedChanged += new System.EventHandler(this.radioButtonSH_CheckedChanged);
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(695, 80);
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
			// 
			// radioButtonSetSamples
			// 
			this.radioButtonSetSamples.AutoSize = true;
			this.radioButtonSetSamples.Location = new System.Drawing.Point(711, 448);
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
			this.progressBarBatchConvert.Location = new System.Drawing.Point(12, 661);
			this.progressBarBatchConvert.Name = "progressBarBatchConvert";
			this.progressBarBatchConvert.Size = new System.Drawing.Size(387, 19);
			this.progressBarBatchConvert.TabIndex = 10;
			this.progressBarBatchConvert.Visible = false;
			// 
			// radioButtonStaticLit
			// 
			this.radioButtonStaticLit.AutoSize = true;
			this.radioButtonStaticLit.Location = new System.Drawing.Point(712, 287);
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
			this.radioButtonEmissiveMatID.Location = new System.Drawing.Point(711, 310);
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
			this.checkBoxSHStatic.Location = new System.Drawing.Point(790, 425);
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
			this.checkBoxSHDynamic.Location = new System.Drawing.Point(849, 425);
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
			this.checkBoxSHEmissive.Location = new System.Drawing.Point(922, 425);
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
			this.checkBoxSHOcclusion.Location = new System.Drawing.Point(922, 448);
			this.checkBoxSHOcclusion.Name = "checkBoxSHOcclusion";
			this.checkBoxSHOcclusion.Size = new System.Drawing.Size(73, 17);
			this.checkBoxSHOcclusion.TabIndex = 9;
			this.checkBoxSHOcclusion.Text = "Occlusion";
			this.checkBoxSHOcclusion.UseVisualStyleBackColor = true;
			this.checkBoxSHOcclusion.CheckedChanged += new System.EventHandler(this.checkBoxSHOcclusion_CheckedChanged);
			// 
			// outputPanel1
			// 
			vector1.X = 0F;
			vector1.Y = 0F;
			vector1.Z = 1F;
			this.outputPanel1.At = vector1;
			this.outputPanel1.IsolatedSetIndex = 0;
			this.outputPanel1.IsolateSet = false;
			this.outputPanel1.Location = new System.Drawing.Point(12, 27);
			this.outputPanel1.Name = "outputPanel1";
			vector2.X = 0F;
			vector2.Y = 0F;
			vector2.Z = 0F;
			vector3.X = 0F;
			vector3.Y = 0F;
			vector3.Z = 0F;
			vector4.X = 0F;
			vector4.Y = 0F;
			vector4.Z = 0F;
			vector5.X = 0F;
			vector5.Y = 0F;
			vector5.Z = 0F;
			vector6.X = 0F;
			vector6.Y = 0F;
			vector6.Z = 0F;
			vector7.X = 0F;
			vector7.Y = 0F;
			vector7.Z = 0F;
			vector8.X = 0F;
			vector8.Y = 0F;
			vector8.Z = 0F;
			vector9.X = 0F;
			vector9.Y = 0F;
			vector9.Z = 0F;
			vector10.X = 0F;
			vector10.Y = 0F;
			vector10.Z = 0F;
			this.outputPanel1.SHDynamic = new WMath.Vector[] {
        vector2,
        vector3,
        vector4,
        vector5,
        vector6,
        vector7,
        vector8,
        vector9,
        vector10};
			vector11.X = 0F;
			vector11.Y = 0F;
			vector11.Z = 0F;
			vector12.X = 0F;
			vector12.Y = 0F;
			vector12.Z = 0F;
			vector13.X = 0F;
			vector13.Y = 0F;
			vector13.Z = 0F;
			vector14.X = 0F;
			vector14.Y = 0F;
			vector14.Z = 0F;
			vector15.X = 0F;
			vector15.Y = 0F;
			vector15.Z = 0F;
			vector16.X = 0F;
			vector16.Y = 0F;
			vector16.Z = 0F;
			vector17.X = 0F;
			vector17.Y = 0F;
			vector17.Z = 0F;
			vector18.X = 0F;
			vector18.Y = 0F;
			vector18.Z = 0F;
			vector19.X = 0F;
			vector19.Y = 0F;
			vector19.Z = 0F;
			this.outputPanel1.SHEmissive = new WMath.Vector[] {
        vector11,
        vector12,
        vector13,
        vector14,
        vector15,
        vector16,
        vector17,
        vector18,
        vector19};
			this.outputPanel1.SHOcclusion = new float[] {
        0F,
        0F,
        0F,
        0F,
        0F,
        0F,
        0F,
        0F,
        0F};
			this.outputPanel1.ShowSetAverage = false;
			this.outputPanel1.ShowSHDynamic = true;
			this.outputPanel1.ShowSHEmissive = false;
			this.outputPanel1.ShowSHOcclusion = false;
			this.outputPanel1.ShowSHStatic = false;
			vector20.X = 0F;
			vector20.Y = 0F;
			vector20.Z = 0F;
			vector21.X = 0F;
			vector21.Y = 0F;
			vector21.Z = 0F;
			vector22.X = 0F;
			vector22.Y = 0F;
			vector22.Z = 0F;
			vector23.X = 0F;
			vector23.Y = 0F;
			vector23.Z = 0F;
			vector24.X = 0F;
			vector24.Y = 0F;
			vector24.Z = 0F;
			vector25.X = 0F;
			vector25.Y = 0F;
			vector25.Z = 0F;
			vector26.X = 0F;
			vector26.Y = 0F;
			vector26.Z = 0F;
			vector27.X = 0F;
			vector27.Y = 0F;
			vector27.Z = 0F;
			vector28.X = 0F;
			vector28.Y = 0F;
			vector28.Z = 0F;
			this.outputPanel1.SHStatic = new WMath.Vector[] {
        vector20,
        vector21,
        vector22,
        vector23,
        vector24,
        vector25,
        vector26,
        vector27,
        vector28};
			this.outputPanel1.Size = new System.Drawing.Size(677, 546);
			this.outputPanel1.TabIndex = 3;
			this.outputPanel1.Viz = ProbeSHEncoder.OutputPanel.VIZ_TYPE.ALBEDO;
			// 
			// checkBoxSHNormalized
			// 
			this.checkBoxSHNormalized.AutoSize = true;
			this.checkBoxSHNormalized.Location = new System.Drawing.Point(849, 448);
			this.checkBoxSHNormalized.Name = "checkBoxSHNormalized";
			this.checkBoxSHNormalized.Size = new System.Drawing.Size(78, 17);
			this.checkBoxSHNormalized.TabIndex = 9;
			this.checkBoxSHNormalized.Text = "Normalized";
			this.checkBoxSHNormalized.UseVisualStyleBackColor = true;
			this.checkBoxSHNormalized.CheckedChanged += new System.EventHandler(this.checkBoxSHNormalized_CheckedChanged);
			// 
			// EncoderForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1027, 692);
			this.Controls.Add(this.checkBoxSHOcclusion);
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
			this.Controls.Add(this.radioButtonEmissiveMatID);
			this.Controls.Add(this.radioButtonStaticLit);
			this.Controls.Add(this.radioButtonNormal);
			this.Controls.Add(this.radioButtonDistance);
			this.Controls.Add(this.radioButtonAlbedo);
			this.Controls.Add(this.outputPanel1);
			this.Controls.Add(this.buttonComputeFilling);
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
		private System.Windows.Forms.ToolStripMenuItem convertShaderToolStripMenuItem;
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
	}
}

