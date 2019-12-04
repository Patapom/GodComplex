﻿namespace TestMSBSDF
{
	partial class AutomationForm
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
			this.groupBoxSimulationParameters = new System.Windows.Forms.GroupBox();
			this.panel6 = new System.Windows.Forms.Panel();
			this.checkBoxParam2_InclusiveStart = new System.Windows.Forms.CheckBox();
			this.checkBoxParam2_InclusiveEnd = new System.Windows.Forms.CheckBox();
			this.integerTrackbarControlParam2_Steps = new UIUtility.IntegerTrackbarControl();
			this.floatTrackbarControlParam2_Max = new UIUtility.FloatTrackbarControl();
			this.label15 = new System.Windows.Forms.Label();
			this.floatTrackbarControlParam2_Min = new UIUtility.FloatTrackbarControl();
			this.label16 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.labelParm2 = new System.Windows.Forms.Label();
			this.panel5 = new System.Windows.Forms.Panel();
			this.checkBoxParam1_InclusiveStart = new System.Windows.Forms.CheckBox();
			this.checkBoxParam1_InclusiveEnd = new System.Windows.Forms.CheckBox();
			this.integerTrackbarControlParam1_Steps = new UIUtility.IntegerTrackbarControl();
			this.floatTrackbarControlParam1_Max = new UIUtility.FloatTrackbarControl();
			this.label6 = new System.Windows.Forms.Label();
			this.floatTrackbarControlParam1_Min = new UIUtility.FloatTrackbarControl();
			this.label12 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.integerTrackbarControlScatteringOrder_Max = new UIUtility.IntegerTrackbarControl();
			this.integerTrackbarControlRayCastingIterations = new UIUtility.IntegerTrackbarControl();
			this.integerTrackbarControlScatteringOrder_Min = new UIUtility.IntegerTrackbarControl();
			this.panelIncidentAngle = new System.Windows.Forms.Panel();
			this.checkBoxParam0_InclusiveStart = new System.Windows.Forms.CheckBox();
			this.checkBoxParam0_InclusiveEnd = new System.Windows.Forms.CheckBox();
			this.integerTrackbarControlParam0_Steps = new UIUtility.IntegerTrackbarControl();
			this.floatTrackbarControlParam0_Max = new UIUtility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.floatTrackbarControlParam0_Min = new UIUtility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label20 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.radioButtonSurfaceTypeDiffuse = new System.Windows.Forms.RadioButton();
			this.radioButtonSurfaceTypeDielectric = new System.Windows.Forms.RadioButton();
			this.radioButtonSurfaceTypeConductor = new System.Windows.Forms.RadioButton();
			this.label22 = new System.Windows.Forms.Label();
			this.labelTotalRaysCount = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label21 = new System.Windows.Forms.Label();
			this.panelParameters = new System.Windows.Forms.Panel();
			this.checkBoxSkipSimulation = new System.Windows.Forms.CheckBox();
			this.textBoxLog = new UIUtility.LogTextBox(this.components);
			this.integerTrackbarControlViewAlbedoSlice = new UIUtility.IntegerTrackbarControl();
			this.groupBoxLobeFitterConfig = new System.Windows.Forms.GroupBox();
			this.floatTrackbarControlFitOversize = new UIUtility.FloatTrackbarControl();
			this.label26 = new System.Windows.Forms.Label();
			this.label23 = new System.Windows.Forms.Label();
			this.label19 = new System.Windows.Forms.Label();
			this.integerTrackbarControlRetries = new UIUtility.IntegerTrackbarControl();
			this.label24 = new System.Windows.Forms.Label();
			this.label25 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.integerTrackbarControlMaxIterations = new UIUtility.IntegerTrackbarControl();
			this.floatTrackbarControlGradientTolerance = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlGoalTolerance = new UIUtility.FloatTrackbarControl();
			this.integerTrackbarControlViewScatteringOrder = new UIUtility.IntegerTrackbarControl();
			this.groupBoxAnalyticalLobeModel = new System.Windows.Forms.GroupBox();
			this.groupBoxCustomInitialGuesses = new System.Windows.Forms.GroupBox();
			this.panel9 = new System.Windows.Forms.Panel();
			this.radioButtonInitMasking_Custom = new System.Windows.Forms.RadioButton();
			this.radioButtonInitMasking_Fixed = new System.Windows.Forms.RadioButton();
			this.radioButtonInitMasking_NoChange = new System.Windows.Forms.RadioButton();
			this.checkBoxInitMasking_InheritLeft = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlInit_FixedMasking = new UIUtility.FloatTrackbarControl();
			this.checkBoxInitMasking_Inherit = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlInit_CustomMaskingImportance = new UIUtility.FloatTrackbarControl();
			this.label8 = new System.Windows.Forms.Label();
			this.panel8 = new System.Windows.Forms.Panel();
			this.radioButtonInitFlatten_Custom = new System.Windows.Forms.RadioButton();
			this.radioButtonInitFlatten_Analytical = new System.Windows.Forms.RadioButton();
			this.radioButtonInitFlatten_Fixed = new System.Windows.Forms.RadioButton();
			this.radioButtonInitFlatten_NoChange = new System.Windows.Forms.RadioButton();
			this.checkBoxInitFlatten_InheritLeft = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlInit_FixedFlatten = new UIUtility.FloatTrackbarControl();
			this.checkBoxInitFlatten_Inherit = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlInit_CustomFlatten = new UIUtility.FloatTrackbarControl();
			this.label28 = new System.Windows.Forms.Label();
			this.panel7 = new System.Windows.Forms.Panel();
			this.radioButtonInitScale_CoMFactor = new System.Windows.Forms.RadioButton();
			this.radioButtonInitScale_Analytical = new System.Windows.Forms.RadioButton();
			this.radioButtonInitScale_Fixed = new System.Windows.Forms.RadioButton();
			this.radioButtonInitScale_NoChange = new System.Windows.Forms.RadioButton();
			this.floatTrackbarControlInit_FixedScale = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlInit_Scale = new UIUtility.FloatTrackbarControl();
			this.checkBoxInitScale_InheritLeft = new System.Windows.Forms.CheckBox();
			this.checkBoxInitScale_Inherit = new System.Windows.Forms.CheckBox();
			this.label27 = new System.Windows.Forms.Label();
			this.panel4 = new System.Windows.Forms.Panel();
			this.radioButtonInitDirection_TowardCoM = new System.Windows.Forms.RadioButton();
			this.radioButtonInitDirection_Fixed = new System.Windows.Forms.RadioButton();
			this.radioButtonInitDirection_NoChange = new System.Windows.Forms.RadioButton();
			this.radioButtonInitDirection_TowardReflected = new System.Windows.Forms.RadioButton();
			this.checkBoxInitDirection_InheritLeft = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlInit_FixedDirection = new UIUtility.FloatTrackbarControl();
			this.checkBoxInitDirection_Inherit = new System.Windows.Forms.CheckBox();
			this.label10 = new System.Windows.Forms.Label();
			this.panel3 = new System.Windows.Forms.Panel();
			this.floatTrackbarControlInit_CustomRoughness = new UIUtility.FloatTrackbarControl();
			this.radioButtonInitRoughness_Analytical = new System.Windows.Forms.RadioButton();
			this.radioButtonInitRoughness_Fixed = new System.Windows.Forms.RadioButton();
			this.radioButtonInitRoughness_NoChange = new System.Windows.Forms.RadioButton();
			this.radioButtonInitRoughness_UseSurface = new System.Windows.Forms.RadioButton();
			this.floatTrackbarControlInit_FixedRoughness = new UIUtility.FloatTrackbarControl();
			this.checkBoxInitRoughness_InheritLeft = new System.Windows.Forms.CheckBox();
			this.checkBoxInitRoughness_Inherit = new System.Windows.Forms.CheckBox();
			this.radioButtonInitRoughness_Custom = new System.Windows.Forms.RadioButton();
			this.label9 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label7 = new System.Windows.Forms.Label();
			this.radioButtonLobe_GGX = new System.Windows.Forms.RadioButton();
			this.radioButtonLobe_ModifiedPhongAniso = new System.Windows.Forms.RadioButton();
			this.radioButtonLobe_Beckmann = new System.Windows.Forms.RadioButton();
			this.radioButtonLobe_ModifiedPhong = new System.Windows.Forms.RadioButton();
			this.label30 = new System.Windows.Forms.Label();
			this.label29 = new System.Windows.Forms.Label();
			this.contextMenuStripSelection = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.computeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.startFromHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.clearColumnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.clearRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.clearSliceFromHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.recentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.resultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.buttonCompute = new System.Windows.Forms.Button();
			this.buttonClearResults = new System.Windows.Forms.Button();
			this.openFileDialogResults = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogResults = new System.Windows.Forms.SaveFileDialog();
			this.saveFileDialogExport = new System.Windows.Forms.SaveFileDialog();
			this.integerTrackbarControlThreadsCount = new UIUtility.IntegerTrackbarControl();
			this.label31 = new System.Windows.Forms.Label();
			this.openFileDialogExport = new System.Windows.Forms.OpenFileDialog();
			this.completionArrayControl = new TestMSBSDF.CompletionArrayControl();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.exportTotalReflectanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.groupBoxSimulationParameters.SuspendLayout();
			this.panel6.SuspendLayout();
			this.panel5.SuspendLayout();
			this.panelIncidentAngle.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panelParameters.SuspendLayout();
			this.groupBoxLobeFitterConfig.SuspendLayout();
			this.groupBoxAnalyticalLobeModel.SuspendLayout();
			this.groupBoxCustomInitialGuesses.SuspendLayout();
			this.panel9.SuspendLayout();
			this.panel8.SuspendLayout();
			this.panel7.SuspendLayout();
			this.panel4.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel2.SuspendLayout();
			this.contextMenuStripSelection.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBoxSimulationParameters
			// 
			this.groupBoxSimulationParameters.Controls.Add(this.panel6);
			this.groupBoxSimulationParameters.Controls.Add(this.panel5);
			this.groupBoxSimulationParameters.Controls.Add(this.integerTrackbarControlScatteringOrder_Max);
			this.groupBoxSimulationParameters.Controls.Add(this.integerTrackbarControlRayCastingIterations);
			this.groupBoxSimulationParameters.Controls.Add(this.integerTrackbarControlScatteringOrder_Min);
			this.groupBoxSimulationParameters.Controls.Add(this.panelIncidentAngle);
			this.groupBoxSimulationParameters.Controls.Add(this.label20);
			this.groupBoxSimulationParameters.Controls.Add(this.panel1);
			this.groupBoxSimulationParameters.Controls.Add(this.label22);
			this.groupBoxSimulationParameters.Controls.Add(this.labelTotalRaysCount);
			this.groupBoxSimulationParameters.Controls.Add(this.label11);
			this.groupBoxSimulationParameters.Controls.Add(this.label21);
			this.groupBoxSimulationParameters.Location = new System.Drawing.Point(12, 31);
			this.groupBoxSimulationParameters.Name = "groupBoxSimulationParameters";
			this.groupBoxSimulationParameters.Size = new System.Drawing.Size(616, 344);
			this.groupBoxSimulationParameters.TabIndex = 0;
			this.groupBoxSimulationParameters.TabStop = false;
			this.groupBoxSimulationParameters.Text = "Simulation Parameters";
			// 
			// panel6
			// 
			this.panel6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.panel6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel6.Controls.Add(this.checkBoxParam2_InclusiveStart);
			this.panel6.Controls.Add(this.checkBoxParam2_InclusiveEnd);
			this.panel6.Controls.Add(this.integerTrackbarControlParam2_Steps);
			this.panel6.Controls.Add(this.floatTrackbarControlParam2_Max);
			this.panel6.Controls.Add(this.label15);
			this.panel6.Controls.Add(this.floatTrackbarControlParam2_Min);
			this.panel6.Controls.Add(this.label16);
			this.panel6.Controls.Add(this.label17);
			this.panel6.Controls.Add(this.labelParm2);
			this.panel6.Location = new System.Drawing.Point(12, 174);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(594, 62);
			this.panel6.TabIndex = 0;
			// 
			// checkBoxParam2_InclusiveStart
			// 
			this.checkBoxParam2_InclusiveStart.AutoSize = true;
			this.checkBoxParam2_InclusiveStart.Checked = true;
			this.checkBoxParam2_InclusiveStart.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxParam2_InclusiveStart.Location = new System.Drawing.Point(358, 36);
			this.checkBoxParam2_InclusiveStart.Name = "checkBoxParam2_InclusiveStart";
			this.checkBoxParam2_InclusiveStart.Size = new System.Drawing.Size(99, 17);
			this.checkBoxParam2_InclusiveStart.TabIndex = 4;
			this.checkBoxParam2_InclusiveStart.Text = "First step at min";
			this.toolTip1.SetToolTip(this.checkBoxParam2_InclusiveStart, "If checked, the first step will be at min albedo, if not then the first step will" +
        " be at (max-min) * 1 / StepsCount");
			this.checkBoxParam2_InclusiveStart.UseVisualStyleBackColor = true;
			this.checkBoxParam2_InclusiveStart.CheckedChanged += new System.EventHandler(this.checkBoxParm2_InclusiveStart_CheckedChanged);
			// 
			// checkBoxParam2_InclusiveEnd
			// 
			this.checkBoxParam2_InclusiveEnd.AutoSize = true;
			this.checkBoxParam2_InclusiveEnd.Location = new System.Drawing.Point(482, 36);
			this.checkBoxParam2_InclusiveEnd.Name = "checkBoxParam2_InclusiveEnd";
			this.checkBoxParam2_InclusiveEnd.Size = new System.Drawing.Size(103, 17);
			this.checkBoxParam2_InclusiveEnd.TabIndex = 4;
			this.checkBoxParam2_InclusiveEnd.Text = "Last step at max";
			this.toolTip1.SetToolTip(this.checkBoxParam2_InclusiveEnd, "If checked, the last step will be at max albedo, if not then the last step will b" +
        "e at (max-min) * (StepsCount-1)/StepsCount");
			this.checkBoxParam2_InclusiveEnd.UseVisualStyleBackColor = true;
			this.checkBoxParam2_InclusiveEnd.CheckedChanged += new System.EventHandler(this.checkBoxParm2_InclusiveEnd_CheckedChanged);
			// 
			// integerTrackbarControlParam2_Steps
			// 
			this.integerTrackbarControlParam2_Steps.Location = new System.Drawing.Point(149, 35);
			this.integerTrackbarControlParam2_Steps.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlParam2_Steps.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlParam2_Steps.Name = "integerTrackbarControlParam2_Steps";
			this.integerTrackbarControlParam2_Steps.RangeMin = 0;
			this.integerTrackbarControlParam2_Steps.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlParam2_Steps.TabIndex = 3;
			this.integerTrackbarControlParam2_Steps.Value = 4;
			this.integerTrackbarControlParam2_Steps.VisibleRangeMax = 20;
			this.integerTrackbarControlParam2_Steps.ValueChanged += new UIUtility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlParam2_Steps_ValueChanged);
			// 
			// floatTrackbarControlParam2_Max
			// 
			this.floatTrackbarControlParam2_Max.Location = new System.Drawing.Point(385, 9);
			this.floatTrackbarControlParam2_Max.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlParam2_Max.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlParam2_Max.Name = "floatTrackbarControlParam2_Max";
			this.floatTrackbarControlParam2_Max.RangeMax = 1F;
			this.floatTrackbarControlParam2_Max.RangeMin = 0F;
			this.floatTrackbarControlParam2_Max.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlParam2_Max.TabIndex = 2;
			this.floatTrackbarControlParam2_Max.Value = 0F;
			this.floatTrackbarControlParam2_Max.VisibleRangeMax = 1F;
			this.floatTrackbarControlParam2_Max.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlParam2_Max_ValueChanged);
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(355, 12);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(27, 13);
			this.label15.TabIndex = 1;
			this.label15.Text = "Max";
			// 
			// floatTrackbarControlParam2_Min
			// 
			this.floatTrackbarControlParam2_Min.Location = new System.Drawing.Point(149, 9);
			this.floatTrackbarControlParam2_Min.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlParam2_Min.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlParam2_Min.Name = "floatTrackbarControlParam2_Min";
			this.floatTrackbarControlParam2_Min.RangeMax = 1F;
			this.floatTrackbarControlParam2_Min.RangeMin = 0F;
			this.floatTrackbarControlParam2_Min.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlParam2_Min.TabIndex = 2;
			this.floatTrackbarControlParam2_Min.Value = 1F;
			this.floatTrackbarControlParam2_Min.VisibleRangeMax = 1F;
			this.floatTrackbarControlParam2_Min.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlParam2_Min_ValueChanged);
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(113, 37);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(34, 13);
			this.label16.TabIndex = 1;
			this.label16.Text = "Steps";
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(113, 12);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(24, 13);
			this.label17.TabIndex = 1;
			this.label17.Text = "Min";
			// 
			// labelParm2
			// 
			this.labelParm2.BackColor = System.Drawing.Color.PaleTurquoise;
			this.labelParm2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelParm2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelParm2.Location = new System.Drawing.Point(5, 5);
			this.labelParm2.Name = "labelParm2";
			this.labelParm2.Size = new System.Drawing.Size(102, 27);
			this.labelParm2.TabIndex = 1;
			this.labelParm2.Text = "Albedo";
			this.labelParm2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// panel5
			// 
			this.panel5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel5.Controls.Add(this.checkBoxParam1_InclusiveStart);
			this.panel5.Controls.Add(this.checkBoxParam1_InclusiveEnd);
			this.panel5.Controls.Add(this.integerTrackbarControlParam1_Steps);
			this.panel5.Controls.Add(this.floatTrackbarControlParam1_Max);
			this.panel5.Controls.Add(this.label6);
			this.panel5.Controls.Add(this.floatTrackbarControlParam1_Min);
			this.panel5.Controls.Add(this.label12);
			this.panel5.Controls.Add(this.label13);
			this.panel5.Controls.Add(this.label14);
			this.panel5.Location = new System.Drawing.Point(12, 106);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(594, 62);
			this.panel5.TabIndex = 0;
			// 
			// checkBoxParam1_InclusiveStart
			// 
			this.checkBoxParam1_InclusiveStart.AutoSize = true;
			this.checkBoxParam1_InclusiveStart.Checked = true;
			this.checkBoxParam1_InclusiveStart.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxParam1_InclusiveStart.Location = new System.Drawing.Point(358, 36);
			this.checkBoxParam1_InclusiveStart.Name = "checkBoxParam1_InclusiveStart";
			this.checkBoxParam1_InclusiveStart.Size = new System.Drawing.Size(99, 17);
			this.checkBoxParam1_InclusiveStart.TabIndex = 4;
			this.checkBoxParam1_InclusiveStart.Text = "First step at min";
			this.toolTip1.SetToolTip(this.checkBoxParam1_InclusiveStart, "If checked, the first step will be at min roughness, if not then the first step w" +
        "ill be at (max-min) * 1 / StepsCount");
			this.checkBoxParam1_InclusiveStart.UseVisualStyleBackColor = true;
			this.checkBoxParam1_InclusiveStart.CheckedChanged += new System.EventHandler(this.checkBoxParm1_InclusiveStart_CheckedChanged);
			// 
			// checkBoxParam1_InclusiveEnd
			// 
			this.checkBoxParam1_InclusiveEnd.AutoSize = true;
			this.checkBoxParam1_InclusiveEnd.Checked = true;
			this.checkBoxParam1_InclusiveEnd.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxParam1_InclusiveEnd.Location = new System.Drawing.Point(482, 36);
			this.checkBoxParam1_InclusiveEnd.Name = "checkBoxParam1_InclusiveEnd";
			this.checkBoxParam1_InclusiveEnd.Size = new System.Drawing.Size(103, 17);
			this.checkBoxParam1_InclusiveEnd.TabIndex = 4;
			this.checkBoxParam1_InclusiveEnd.Text = "Last step at max";
			this.toolTip1.SetToolTip(this.checkBoxParam1_InclusiveEnd, "If checked, the last step will be at max roughness, if not then the last step wil" +
        "l be at (max-min) * (StepsCount-1)/StepsCount");
			this.checkBoxParam1_InclusiveEnd.UseVisualStyleBackColor = true;
			this.checkBoxParam1_InclusiveEnd.CheckedChanged += new System.EventHandler(this.checkBoxParam1_InclusiveEnd_CheckedChanged);
			// 
			// integerTrackbarControlParam1_Steps
			// 
			this.integerTrackbarControlParam1_Steps.Location = new System.Drawing.Point(149, 35);
			this.integerTrackbarControlParam1_Steps.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlParam1_Steps.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlParam1_Steps.Name = "integerTrackbarControlParam1_Steps";
			this.integerTrackbarControlParam1_Steps.RangeMin = 0;
			this.integerTrackbarControlParam1_Steps.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlParam1_Steps.TabIndex = 3;
			this.integerTrackbarControlParam1_Steps.Value = 10;
			this.integerTrackbarControlParam1_Steps.VisibleRangeMax = 20;
			this.integerTrackbarControlParam1_Steps.ValueChanged += new UIUtility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlParam1_Steps_ValueChanged);
			// 
			// floatTrackbarControlParam1_Max
			// 
			this.floatTrackbarControlParam1_Max.Location = new System.Drawing.Point(385, 9);
			this.floatTrackbarControlParam1_Max.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlParam1_Max.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlParam1_Max.Name = "floatTrackbarControlParam1_Max";
			this.floatTrackbarControlParam1_Max.RangeMax = 1F;
			this.floatTrackbarControlParam1_Max.RangeMin = 0F;
			this.floatTrackbarControlParam1_Max.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlParam1_Max.TabIndex = 2;
			this.floatTrackbarControlParam1_Max.Value = 1F;
			this.floatTrackbarControlParam1_Max.VisibleRangeMax = 1F;
			this.floatTrackbarControlParam1_Max.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlParam1_Max_ValueChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(355, 12);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(27, 13);
			this.label6.TabIndex = 1;
			this.label6.Text = "Max";
			// 
			// floatTrackbarControlParam1_Min
			// 
			this.floatTrackbarControlParam1_Min.Location = new System.Drawing.Point(149, 9);
			this.floatTrackbarControlParam1_Min.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlParam1_Min.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlParam1_Min.Name = "floatTrackbarControlParam1_Min";
			this.floatTrackbarControlParam1_Min.RangeMax = 1F;
			this.floatTrackbarControlParam1_Min.RangeMin = 0F;
			this.floatTrackbarControlParam1_Min.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlParam1_Min.TabIndex = 2;
			this.floatTrackbarControlParam1_Min.Value = 0F;
			this.floatTrackbarControlParam1_Min.VisibleRangeMax = 1F;
			this.floatTrackbarControlParam1_Min.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlParam1_Min_ValueChanged);
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(113, 37);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(34, 13);
			this.label12.TabIndex = 1;
			this.label12.Text = "Steps";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(113, 12);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(24, 13);
			this.label13.TabIndex = 1;
			this.label13.Text = "Min";
			// 
			// label14
			// 
			this.label14.BackColor = System.Drawing.Color.LightGreen;
			this.label14.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label14.Location = new System.Drawing.Point(5, 5);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(102, 27);
			this.label14.TabIndex = 1;
			this.label14.Text = "Roughness";
			this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// integerTrackbarControlScatteringOrder_Max
			// 
			this.integerTrackbarControlScatteringOrder_Max.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.integerTrackbarControlScatteringOrder_Max.Location = new System.Drawing.Point(398, 261);
			this.integerTrackbarControlScatteringOrder_Max.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlScatteringOrder_Max.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlScatteringOrder_Max.Name = "integerTrackbarControlScatteringOrder_Max";
			this.integerTrackbarControlScatteringOrder_Max.RangeMax = 4;
			this.integerTrackbarControlScatteringOrder_Max.RangeMin = 1;
			this.integerTrackbarControlScatteringOrder_Max.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlScatteringOrder_Max.TabIndex = 3;
			this.integerTrackbarControlScatteringOrder_Max.Value = 4;
			this.integerTrackbarControlScatteringOrder_Max.VisibleRangeMax = 4;
			this.integerTrackbarControlScatteringOrder_Max.VisibleRangeMin = 1;
			this.integerTrackbarControlScatteringOrder_Max.ValueChanged += new UIUtility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlScatteringOrder_Max_ValueChanged);
			// 
			// integerTrackbarControlRayCastingIterations
			// 
			this.integerTrackbarControlRayCastingIterations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.integerTrackbarControlRayCastingIterations.Location = new System.Drawing.Point(162, 311);
			this.integerTrackbarControlRayCastingIterations.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlRayCastingIterations.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlRayCastingIterations.Name = "integerTrackbarControlRayCastingIterations";
			this.integerTrackbarControlRayCastingIterations.RangeMax = 4096;
			this.integerTrackbarControlRayCastingIterations.RangeMin = 1;
			this.integerTrackbarControlRayCastingIterations.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlRayCastingIterations.TabIndex = 3;
			this.integerTrackbarControlRayCastingIterations.Value = 1;
			this.integerTrackbarControlRayCastingIterations.VisibleRangeMax = 2048;
			this.integerTrackbarControlRayCastingIterations.VisibleRangeMin = 1;
			this.integerTrackbarControlRayCastingIterations.ValueChanged += new UIUtility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlRayCastingIterations_ValueChanged);
			// 
			// integerTrackbarControlScatteringOrder_Min
			// 
			this.integerTrackbarControlScatteringOrder_Min.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.integerTrackbarControlScatteringOrder_Min.Location = new System.Drawing.Point(163, 261);
			this.integerTrackbarControlScatteringOrder_Min.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlScatteringOrder_Min.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlScatteringOrder_Min.Name = "integerTrackbarControlScatteringOrder_Min";
			this.integerTrackbarControlScatteringOrder_Min.RangeMax = 4;
			this.integerTrackbarControlScatteringOrder_Min.RangeMin = 1;
			this.integerTrackbarControlScatteringOrder_Min.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlScatteringOrder_Min.TabIndex = 3;
			this.integerTrackbarControlScatteringOrder_Min.Value = 2;
			this.integerTrackbarControlScatteringOrder_Min.VisibleRangeMax = 4;
			this.integerTrackbarControlScatteringOrder_Min.VisibleRangeMin = 1;
			this.integerTrackbarControlScatteringOrder_Min.ValueChanged += new UIUtility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlScatteringOrder_Min_ValueChanged);
			// 
			// panelIncidentAngle
			// 
			this.panelIncidentAngle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.panelIncidentAngle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelIncidentAngle.Controls.Add(this.checkBoxParam0_InclusiveStart);
			this.panelIncidentAngle.Controls.Add(this.checkBoxParam0_InclusiveEnd);
			this.panelIncidentAngle.Controls.Add(this.integerTrackbarControlParam0_Steps);
			this.panelIncidentAngle.Controls.Add(this.floatTrackbarControlParam0_Max);
			this.panelIncidentAngle.Controls.Add(this.label4);
			this.panelIncidentAngle.Controls.Add(this.floatTrackbarControlParam0_Min);
			this.panelIncidentAngle.Controls.Add(this.label5);
			this.panelIncidentAngle.Controls.Add(this.label3);
			this.panelIncidentAngle.Controls.Add(this.label2);
			this.panelIncidentAngle.Location = new System.Drawing.Point(12, 38);
			this.panelIncidentAngle.Name = "panelIncidentAngle";
			this.panelIncidentAngle.Size = new System.Drawing.Size(594, 62);
			this.panelIncidentAngle.TabIndex = 0;
			// 
			// checkBoxParam0_InclusiveStart
			// 
			this.checkBoxParam0_InclusiveStart.AutoSize = true;
			this.checkBoxParam0_InclusiveStart.Checked = true;
			this.checkBoxParam0_InclusiveStart.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxParam0_InclusiveStart.Location = new System.Drawing.Point(358, 36);
			this.checkBoxParam0_InclusiveStart.Name = "checkBoxParam0_InclusiveStart";
			this.checkBoxParam0_InclusiveStart.Size = new System.Drawing.Size(99, 17);
			this.checkBoxParam0_InclusiveStart.TabIndex = 4;
			this.checkBoxParam0_InclusiveStart.Text = "First step at min";
			this.toolTip1.SetToolTip(this.checkBoxParam0_InclusiveStart, "If checked, the first step will be at min incident angle, if not then the first s" +
        "tep will be at (max-min) * 1 / StepsCount");
			this.checkBoxParam0_InclusiveStart.UseVisualStyleBackColor = true;
			this.checkBoxParam0_InclusiveStart.CheckedChanged += new System.EventHandler(this.checkBoxParam0_InclusiveStart_CheckedChanged);
			// 
			// checkBoxParam0_InclusiveEnd
			// 
			this.checkBoxParam0_InclusiveEnd.AutoSize = true;
			this.checkBoxParam0_InclusiveEnd.Location = new System.Drawing.Point(482, 36);
			this.checkBoxParam0_InclusiveEnd.Name = "checkBoxParam0_InclusiveEnd";
			this.checkBoxParam0_InclusiveEnd.Size = new System.Drawing.Size(103, 17);
			this.checkBoxParam0_InclusiveEnd.TabIndex = 4;
			this.checkBoxParam0_InclusiveEnd.Text = "Last step at max";
			this.toolTip1.SetToolTip(this.checkBoxParam0_InclusiveEnd, "If checked, the last step will be at max incident angle, if not then the last ste" +
        "p will be at (max-min) * (StepsCount-1)/StepsCount");
			this.checkBoxParam0_InclusiveEnd.UseVisualStyleBackColor = true;
			this.checkBoxParam0_InclusiveEnd.CheckedChanged += new System.EventHandler(this.checkBoxParam0_InclusiveEnd_CheckedChanged);
			// 
			// integerTrackbarControlParam0_Steps
			// 
			this.integerTrackbarControlParam0_Steps.Location = new System.Drawing.Point(149, 35);
			this.integerTrackbarControlParam0_Steps.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlParam0_Steps.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlParam0_Steps.Name = "integerTrackbarControlParam0_Steps";
			this.integerTrackbarControlParam0_Steps.RangeMin = 0;
			this.integerTrackbarControlParam0_Steps.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlParam0_Steps.TabIndex = 3;
			this.integerTrackbarControlParam0_Steps.Value = 30;
			this.integerTrackbarControlParam0_Steps.ValueChanged += new UIUtility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlParam0_Steps_ValueChanged);
			// 
			// floatTrackbarControlParam0_Max
			// 
			this.floatTrackbarControlParam0_Max.Location = new System.Drawing.Point(385, 9);
			this.floatTrackbarControlParam0_Max.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlParam0_Max.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlParam0_Max.Name = "floatTrackbarControlParam0_Max";
			this.floatTrackbarControlParam0_Max.RangeMax = 90F;
			this.floatTrackbarControlParam0_Max.RangeMin = 0F;
			this.floatTrackbarControlParam0_Max.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlParam0_Max.TabIndex = 2;
			this.floatTrackbarControlParam0_Max.Value = 90F;
			this.floatTrackbarControlParam0_Max.VisibleRangeMax = 90F;
			this.floatTrackbarControlParam0_Max.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlParam0_Max_ValueChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(355, 12);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(27, 13);
			this.label4.TabIndex = 1;
			this.label4.Text = "Max";
			// 
			// floatTrackbarControlParam0_Min
			// 
			this.floatTrackbarControlParam0_Min.Location = new System.Drawing.Point(149, 9);
			this.floatTrackbarControlParam0_Min.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlParam0_Min.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlParam0_Min.Name = "floatTrackbarControlParam0_Min";
			this.floatTrackbarControlParam0_Min.RangeMax = 90F;
			this.floatTrackbarControlParam0_Min.RangeMin = 0F;
			this.floatTrackbarControlParam0_Min.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlParam0_Min.TabIndex = 2;
			this.floatTrackbarControlParam0_Min.Value = 0F;
			this.floatTrackbarControlParam0_Min.VisibleRangeMax = 90F;
			this.floatTrackbarControlParam0_Min.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlParam0_Min_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(113, 37);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(34, 13);
			this.label5.TabIndex = 1;
			this.label5.Text = "Steps";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(113, 12);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(24, 13);
			this.label3.TabIndex = 1;
			this.label3.Text = "Min";
			// 
			// label2
			// 
			this.label2.BackColor = System.Drawing.Color.LightCoral;
			this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(5, 5);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(102, 27);
			this.label2.TabIndex = 1;
			this.label2.Text = "Incident Angle";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label20
			// 
			this.label20.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label20.BackColor = System.Drawing.Color.Orange;
			this.label20.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label20.Location = new System.Drawing.Point(18, 250);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(102, 42);
			this.label20.TabIndex = 1;
			this.label20.Text = "Scattering Orders";
			this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.radioButtonSurfaceTypeDiffuse);
			this.panel1.Controls.Add(this.radioButtonSurfaceTypeDielectric);
			this.panel1.Controls.Add(this.radioButtonSurfaceTypeConductor);
			this.panel1.Location = new System.Drawing.Point(12, 14);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(307, 27);
			this.panel1.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 5);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(74, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Surface Type:";
			// 
			// radioButtonSurfaceTypeDiffuse
			// 
			this.radioButtonSurfaceTypeDiffuse.AutoSize = true;
			this.radioButtonSurfaceTypeDiffuse.Checked = true;
			this.radioButtonSurfaceTypeDiffuse.Location = new System.Drawing.Point(238, 3);
			this.radioButtonSurfaceTypeDiffuse.Name = "radioButtonSurfaceTypeDiffuse";
			this.radioButtonSurfaceTypeDiffuse.Size = new System.Drawing.Size(58, 17);
			this.radioButtonSurfaceTypeDiffuse.TabIndex = 0;
			this.radioButtonSurfaceTypeDiffuse.TabStop = true;
			this.radioButtonSurfaceTypeDiffuse.Text = "Diffuse";
			this.radioButtonSurfaceTypeDiffuse.UseVisualStyleBackColor = true;
			this.radioButtonSurfaceTypeDiffuse.CheckedChanged += new System.EventHandler(this.radioButtonSurfaceType_CheckedChanged);
			// 
			// radioButtonSurfaceTypeDielectric
			// 
			this.radioButtonSurfaceTypeDielectric.AutoSize = true;
			this.radioButtonSurfaceTypeDielectric.Location = new System.Drawing.Point(163, 3);
			this.radioButtonSurfaceTypeDielectric.Name = "radioButtonSurfaceTypeDielectric";
			this.radioButtonSurfaceTypeDielectric.Size = new System.Drawing.Size(69, 17);
			this.radioButtonSurfaceTypeDielectric.TabIndex = 0;
			this.radioButtonSurfaceTypeDielectric.Text = "Dielectric";
			this.radioButtonSurfaceTypeDielectric.UseVisualStyleBackColor = true;
			this.radioButtonSurfaceTypeDielectric.CheckedChanged += new System.EventHandler(this.radioButtonSurfaceType_CheckedChanged);
			// 
			// radioButtonSurfaceTypeConductor
			// 
			this.radioButtonSurfaceTypeConductor.AutoSize = true;
			this.radioButtonSurfaceTypeConductor.Location = new System.Drawing.Point(83, 3);
			this.radioButtonSurfaceTypeConductor.Name = "radioButtonSurfaceTypeConductor";
			this.radioButtonSurfaceTypeConductor.Size = new System.Drawing.Size(74, 17);
			this.radioButtonSurfaceTypeConductor.TabIndex = 0;
			this.radioButtonSurfaceTypeConductor.Text = "Conductor";
			this.radioButtonSurfaceTypeConductor.UseVisualStyleBackColor = true;
			this.radioButtonSurfaceTypeConductor.CheckedChanged += new System.EventHandler(this.radioButtonSurfaceType_CheckedChanged);
			// 
			// label22
			// 
			this.label22.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label22.AutoSize = true;
			this.label22.Location = new System.Drawing.Point(368, 265);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(27, 13);
			this.label22.TabIndex = 1;
			this.label22.Text = "Max";
			// 
			// labelTotalRaysCount
			// 
			this.labelTotalRaysCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelTotalRaysCount.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelTotalRaysCount.Location = new System.Drawing.Point(371, 310);
			this.labelTotalRaysCount.Name = "labelTotalRaysCount";
			this.labelTotalRaysCount.Size = new System.Drawing.Size(210, 23);
			this.labelTotalRaysCount.TabIndex = 1;
			this.labelTotalRaysCount.Text = "Total Simulated Rays: ";
			this.labelTotalRaysCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label11
			// 
			this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(18, 314);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(142, 13);
			this.label11.TabIndex = 1;
			this.label11.Text = "Ray-Tracing Iterations Count";
			// 
			// label21
			// 
			this.label21.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label21.AutoSize = true;
			this.label21.Location = new System.Drawing.Point(127, 263);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(24, 13);
			this.label21.TabIndex = 1;
			this.label21.Text = "Min";
			// 
			// panelParameters
			// 
			this.panelParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelParameters.Controls.Add(this.checkBoxSkipSimulation);
			this.panelParameters.Controls.Add(this.textBoxLog);
			this.panelParameters.Controls.Add(this.integerTrackbarControlViewAlbedoSlice);
			this.panelParameters.Controls.Add(this.groupBoxLobeFitterConfig);
			this.panelParameters.Controls.Add(this.integerTrackbarControlViewScatteringOrder);
			this.panelParameters.Controls.Add(this.groupBoxAnalyticalLobeModel);
			this.panelParameters.Controls.Add(this.label30);
			this.panelParameters.Controls.Add(this.label29);
			this.panelParameters.Location = new System.Drawing.Point(12, 27);
			this.panelParameters.Name = "panelParameters";
			this.panelParameters.Size = new System.Drawing.Size(1033, 756);
			this.panelParameters.TabIndex = 1;
			// 
			// checkBoxSkipSimulation
			// 
			this.checkBoxSkipSimulation.AutoSize = true;
			this.checkBoxSkipSimulation.Location = new System.Drawing.Point(751, 600);
			this.checkBoxSkipSimulation.Name = "checkBoxSkipSimulation";
			this.checkBoxSkipSimulation.Size = new System.Drawing.Size(244, 17);
			this.checkBoxSkipSimulation.TabIndex = 8;
			this.checkBoxSkipSimulation.Text = "Skip Simulation (only collect total reflectances)";
			this.checkBoxSkipSimulation.UseVisualStyleBackColor = true;
			this.checkBoxSkipSimulation.CheckedChanged += new System.EventHandler(this.checkBoxSkipSimulation_CheckedChanged);
			// 
			// textBoxLog
			// 
			this.textBoxLog.BackColor = System.Drawing.SystemColors.Info;
			this.textBoxLog.Location = new System.Drawing.Point(0, 655);
			this.textBoxLog.Name = "textBoxLog";
			this.textBoxLog.ReadOnly = true;
			this.textBoxLog.Size = new System.Drawing.Size(616, 98);
			this.textBoxLog.TabIndex = 4;
			this.textBoxLog.Text = "";
			// 
			// integerTrackbarControlViewAlbedoSlice
			// 
			this.integerTrackbarControlViewAlbedoSlice.Location = new System.Drawing.Point(403, 623);
			this.integerTrackbarControlViewAlbedoSlice.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlViewAlbedoSlice.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlViewAlbedoSlice.Name = "integerTrackbarControlViewAlbedoSlice";
			this.integerTrackbarControlViewAlbedoSlice.RangeMax = 4;
			this.integerTrackbarControlViewAlbedoSlice.RangeMin = 0;
			this.integerTrackbarControlViewAlbedoSlice.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlViewAlbedoSlice.TabIndex = 3;
			this.integerTrackbarControlViewAlbedoSlice.Value = 0;
			this.integerTrackbarControlViewAlbedoSlice.VisibleRangeMax = 4;
			this.integerTrackbarControlViewAlbedoSlice.ValueChanged += new UIUtility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlViewAlbedoSlice_ValueChanged);
			// 
			// groupBoxLobeFitterConfig
			// 
			this.groupBoxLobeFitterConfig.Controls.Add(this.floatTrackbarControlFitOversize);
			this.groupBoxLobeFitterConfig.Controls.Add(this.label26);
			this.groupBoxLobeFitterConfig.Controls.Add(this.label23);
			this.groupBoxLobeFitterConfig.Controls.Add(this.label19);
			this.groupBoxLobeFitterConfig.Controls.Add(this.integerTrackbarControlRetries);
			this.groupBoxLobeFitterConfig.Controls.Add(this.label24);
			this.groupBoxLobeFitterConfig.Controls.Add(this.label25);
			this.groupBoxLobeFitterConfig.Controls.Add(this.label18);
			this.groupBoxLobeFitterConfig.Controls.Add(this.integerTrackbarControlMaxIterations);
			this.groupBoxLobeFitterConfig.Controls.Add(this.floatTrackbarControlGradientTolerance);
			this.groupBoxLobeFitterConfig.Controls.Add(this.floatTrackbarControlGoalTolerance);
			this.groupBoxLobeFitterConfig.Location = new System.Drawing.Point(622, 601);
			this.groupBoxLobeFitterConfig.Name = "groupBoxLobeFitterConfig";
			this.groupBoxLobeFitterConfig.Size = new System.Drawing.Size(395, 152);
			this.groupBoxLobeFitterConfig.TabIndex = 2;
			this.groupBoxLobeFitterConfig.TabStop = false;
			this.groupBoxLobeFitterConfig.Text = "Lobe Fitter Configuration";
			// 
			// floatTrackbarControlFitOversize
			// 
			this.floatTrackbarControlFitOversize.Location = new System.Drawing.Point(146, 122);
			this.floatTrackbarControlFitOversize.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlFitOversize.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlFitOversize.Name = "floatTrackbarControlFitOversize";
			this.floatTrackbarControlFitOversize.RangeMax = 2F;
			this.floatTrackbarControlFitOversize.RangeMin = 0F;
			this.floatTrackbarControlFitOversize.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlFitOversize.TabIndex = 7;
			this.floatTrackbarControlFitOversize.Value = 1.02F;
			this.floatTrackbarControlFitOversize.VisibleRangeMax = 1.1F;
			this.floatTrackbarControlFitOversize.VisibleRangeMin = 1F;
			this.floatTrackbarControlFitOversize.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlFitOversize_ValueChanged);
			// 
			// label26
			// 
			this.label26.AutoSize = true;
			this.label26.Location = new System.Drawing.Point(7, 127);
			this.label26.Name = "label26";
			this.label26.Size = new System.Drawing.Size(112, 13);
			this.label26.TabIndex = 6;
			this.label26.Text = "Fitting Oversize Factor";
			// 
			// label23
			// 
			this.label23.AutoSize = true;
			this.label23.Location = new System.Drawing.Point(7, 74);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(133, 13);
			this.label23.TabIndex = 1;
			this.label23.Text = "Gradient Tolerance (log10)";
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(7, 48);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(115, 13);
			this.label19.TabIndex = 1;
			this.label19.Text = "Goal Tolerance (log10)";
			// 
			// integerTrackbarControlRetries
			// 
			this.integerTrackbarControlRetries.Location = new System.Drawing.Point(197, 96);
			this.integerTrackbarControlRetries.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlRetries.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlRetries.Name = "integerTrackbarControlRetries";
			this.integerTrackbarControlRetries.RangeMax = 10;
			this.integerTrackbarControlRetries.RangeMin = 0;
			this.integerTrackbarControlRetries.Size = new System.Drawing.Size(149, 20);
			this.integerTrackbarControlRetries.TabIndex = 3;
			this.integerTrackbarControlRetries.Value = 2;
			this.integerTrackbarControlRetries.VisibleRangeMax = 4;
			this.integerTrackbarControlRetries.ValueChanged += new UIUtility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlRetries_ValueChanged);
			// 
			// label24
			// 
			this.label24.Location = new System.Drawing.Point(7, 99);
			this.label24.Name = "label24";
			this.label24.Size = new System.Drawing.Size(199, 17);
			this.label24.TabIndex = 1;
			this.label24.Text = "If fitting fails, retry from current position";
			// 
			// label25
			// 
			this.label25.AutoSize = true;
			this.label25.Location = new System.Drawing.Point(352, 99);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(31, 13);
			this.label25.TabIndex = 1;
			this.label25.Text = "times";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(7, 23);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(97, 13);
			this.label18.TabIndex = 1;
			this.label18.Text = "Maximum Iterations";
			// 
			// integerTrackbarControlMaxIterations
			// 
			this.integerTrackbarControlMaxIterations.Location = new System.Drawing.Point(146, 18);
			this.integerTrackbarControlMaxIterations.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlMaxIterations.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlMaxIterations.Name = "integerTrackbarControlMaxIterations";
			this.integerTrackbarControlMaxIterations.RangeMin = 0;
			this.integerTrackbarControlMaxIterations.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlMaxIterations.TabIndex = 3;
			this.integerTrackbarControlMaxIterations.Value = 200;
			this.integerTrackbarControlMaxIterations.VisibleRangeMax = 200;
			this.integerTrackbarControlMaxIterations.ValueChanged += new UIUtility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlMaxIterations_ValueChanged);
			// 
			// floatTrackbarControlGradientTolerance
			// 
			this.floatTrackbarControlGradientTolerance.Location = new System.Drawing.Point(146, 70);
			this.floatTrackbarControlGradientTolerance.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlGradientTolerance.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlGradientTolerance.Name = "floatTrackbarControlGradientTolerance";
			this.floatTrackbarControlGradientTolerance.RangeMax = 0F;
			this.floatTrackbarControlGradientTolerance.RangeMin = -10F;
			this.floatTrackbarControlGradientTolerance.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlGradientTolerance.TabIndex = 2;
			this.floatTrackbarControlGradientTolerance.Value = -6F;
			this.floatTrackbarControlGradientTolerance.VisibleRangeMax = 0F;
			this.floatTrackbarControlGradientTolerance.VisibleRangeMin = -8F;
			this.floatTrackbarControlGradientTolerance.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlGradientTolerance_ValueChanged);
			// 
			// floatTrackbarControlGoalTolerance
			// 
			this.floatTrackbarControlGoalTolerance.Location = new System.Drawing.Point(146, 44);
			this.floatTrackbarControlGoalTolerance.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlGoalTolerance.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlGoalTolerance.Name = "floatTrackbarControlGoalTolerance";
			this.floatTrackbarControlGoalTolerance.RangeMax = 0F;
			this.floatTrackbarControlGoalTolerance.RangeMin = -10F;
			this.floatTrackbarControlGoalTolerance.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlGoalTolerance.TabIndex = 2;
			this.floatTrackbarControlGoalTolerance.Value = -6F;
			this.floatTrackbarControlGoalTolerance.VisibleRangeMax = 0F;
			this.floatTrackbarControlGoalTolerance.VisibleRangeMin = -8F;
			this.floatTrackbarControlGoalTolerance.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlGoalTolerance_ValueChanged);
			// 
			// integerTrackbarControlViewScatteringOrder
			// 
			this.integerTrackbarControlViewScatteringOrder.Location = new System.Drawing.Point(111, 623);
			this.integerTrackbarControlViewScatteringOrder.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlViewScatteringOrder.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlViewScatteringOrder.Name = "integerTrackbarControlViewScatteringOrder";
			this.integerTrackbarControlViewScatteringOrder.RangeMax = 4;
			this.integerTrackbarControlViewScatteringOrder.RangeMin = 1;
			this.integerTrackbarControlViewScatteringOrder.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlViewScatteringOrder.TabIndex = 3;
			this.integerTrackbarControlViewScatteringOrder.Value = 1;
			this.integerTrackbarControlViewScatteringOrder.VisibleRangeMax = 4;
			this.integerTrackbarControlViewScatteringOrder.VisibleRangeMin = 1;
			this.integerTrackbarControlViewScatteringOrder.ValueChanged += new UIUtility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlViewScatteringOrder_ValueChanged);
			// 
			// groupBoxAnalyticalLobeModel
			// 
			this.groupBoxAnalyticalLobeModel.Controls.Add(this.groupBoxCustomInitialGuesses);
			this.groupBoxAnalyticalLobeModel.Controls.Add(this.panel2);
			this.groupBoxAnalyticalLobeModel.Location = new System.Drawing.Point(622, 4);
			this.groupBoxAnalyticalLobeModel.Name = "groupBoxAnalyticalLobeModel";
			this.groupBoxAnalyticalLobeModel.Size = new System.Drawing.Size(395, 591);
			this.groupBoxAnalyticalLobeModel.TabIndex = 0;
			this.groupBoxAnalyticalLobeModel.TabStop = false;
			this.groupBoxAnalyticalLobeModel.Text = "Analytical Lobe Model";
			// 
			// groupBoxCustomInitialGuesses
			// 
			this.groupBoxCustomInitialGuesses.Controls.Add(this.panel9);
			this.groupBoxCustomInitialGuesses.Controls.Add(this.panel8);
			this.groupBoxCustomInitialGuesses.Controls.Add(this.panel7);
			this.groupBoxCustomInitialGuesses.Controls.Add(this.panel4);
			this.groupBoxCustomInitialGuesses.Controls.Add(this.panel3);
			this.groupBoxCustomInitialGuesses.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBoxCustomInitialGuesses.Location = new System.Drawing.Point(6, 52);
			this.groupBoxCustomInitialGuesses.Name = "groupBoxCustomInitialGuesses";
			this.groupBoxCustomInitialGuesses.Size = new System.Drawing.Size(377, 539);
			this.groupBoxCustomInitialGuesses.TabIndex = 2;
			this.groupBoxCustomInitialGuesses.TabStop = false;
			this.groupBoxCustomInitialGuesses.Text = "Initial Guesses for Parameters";
			// 
			// panel9
			// 
			this.panel9.Controls.Add(this.radioButtonInitMasking_Custom);
			this.panel9.Controls.Add(this.radioButtonInitMasking_Fixed);
			this.panel9.Controls.Add(this.radioButtonInitMasking_NoChange);
			this.panel9.Controls.Add(this.checkBoxInitMasking_InheritLeft);
			this.panel9.Controls.Add(this.floatTrackbarControlInit_FixedMasking);
			this.panel9.Controls.Add(this.checkBoxInitMasking_Inherit);
			this.panel9.Controls.Add(this.floatTrackbarControlInit_CustomMaskingImportance);
			this.panel9.Controls.Add(this.label8);
			this.panel9.Location = new System.Drawing.Point(3, 464);
			this.panel9.Name = "panel9";
			this.panel9.Size = new System.Drawing.Size(355, 71);
			this.panel9.TabIndex = 9;
			// 
			// radioButtonInitMasking_Custom
			// 
			this.radioButtonInitMasking_Custom.AutoSize = true;
			this.radioButtonInitMasking_Custom.Checked = true;
			this.radioButtonInitMasking_Custom.Location = new System.Drawing.Point(102, 4);
			this.radioButtonInitMasking_Custom.Name = "radioButtonInitMasking_Custom";
			this.radioButtonInitMasking_Custom.Size = new System.Drawing.Size(14, 13);
			this.radioButtonInitMasking_Custom.TabIndex = 0;
			this.radioButtonInitMasking_Custom.TabStop = true;
			this.radioButtonInitMasking_Custom.UseVisualStyleBackColor = true;
			this.radioButtonInitMasking_Custom.CheckedChanged += new System.EventHandler(this.radioButtonInitMasking_CheckedChanged);
			// 
			// radioButtonInitMasking_Fixed
			// 
			this.radioButtonInitMasking_Fixed.AutoSize = true;
			this.radioButtonInitMasking_Fixed.Location = new System.Drawing.Point(102, 49);
			this.radioButtonInitMasking_Fixed.Name = "radioButtonInitMasking_Fixed";
			this.radioButtonInitMasking_Fixed.Size = new System.Drawing.Size(50, 17);
			this.radioButtonInitMasking_Fixed.TabIndex = 0;
			this.radioButtonInitMasking_Fixed.Text = "Fixed";
			this.radioButtonInitMasking_Fixed.UseVisualStyleBackColor = true;
			this.radioButtonInitMasking_Fixed.CheckedChanged += new System.EventHandler(this.radioButtonInitMasking_CheckedChanged);
			// 
			// radioButtonInitMasking_NoChange
			// 
			this.radioButtonInitMasking_NoChange.AutoSize = true;
			this.radioButtonInitMasking_NoChange.Location = new System.Drawing.Point(102, 26);
			this.radioButtonInitMasking_NoChange.Name = "radioButtonInitMasking_NoChange";
			this.radioButtonInitMasking_NoChange.Size = new System.Drawing.Size(139, 17);
			this.radioButtonInitMasking_NoChange.TabIndex = 0;
			this.radioButtonInitMasking_NoChange.Text = "No Change from Current";
			this.radioButtonInitMasking_NoChange.UseVisualStyleBackColor = true;
			this.radioButtonInitMasking_NoChange.CheckedChanged += new System.EventHandler(this.radioButtonInitMasking_CheckedChanged);
			// 
			// checkBoxInitMasking_InheritLeft
			// 
			this.checkBoxInitMasking_InheritLeft.AutoSize = true;
			this.checkBoxInitMasking_InheritLeft.Location = new System.Drawing.Point(9, 42);
			this.checkBoxInitMasking_InheritLeft.Name = "checkBoxInitMasking_InheritLeft";
			this.checkBoxInitMasking_InheritLeft.Size = new System.Drawing.Size(76, 17);
			this.checkBoxInitMasking_InheritLeft.TabIndex = 4;
			this.checkBoxInitMasking_InheritLeft.Text = "Inherit Left";
			this.checkBoxInitMasking_InheritLeft.UseVisualStyleBackColor = true;
			this.checkBoxInitMasking_InheritLeft.CheckedChanged += new System.EventHandler(this.checkBoxInitMasking_InheritLeft_CheckedChanged);
			// 
			// floatTrackbarControlInit_FixedMasking
			// 
			this.floatTrackbarControlInit_FixedMasking.Enabled = false;
			this.floatTrackbarControlInit_FixedMasking.Location = new System.Drawing.Point(158, 47);
			this.floatTrackbarControlInit_FixedMasking.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlInit_FixedMasking.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlInit_FixedMasking.Name = "floatTrackbarControlInit_FixedMasking";
			this.floatTrackbarControlInit_FixedMasking.RangeMax = 1F;
			this.floatTrackbarControlInit_FixedMasking.RangeMin = 0F;
			this.floatTrackbarControlInit_FixedMasking.Size = new System.Drawing.Size(164, 20);
			this.floatTrackbarControlInit_FixedMasking.TabIndex = 2;
			this.floatTrackbarControlInit_FixedMasking.Value = 1F;
			this.floatTrackbarControlInit_FixedMasking.VisibleRangeMax = 1F;
			this.floatTrackbarControlInit_FixedMasking.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlInit_FixedMasking_ValueChanged);
			// 
			// checkBoxInitMasking_Inherit
			// 
			this.checkBoxInitMasking_Inherit.AutoSize = true;
			this.checkBoxInitMasking_Inherit.Location = new System.Drawing.Point(9, 23);
			this.checkBoxInitMasking_Inherit.Name = "checkBoxInitMasking_Inherit";
			this.checkBoxInitMasking_Inherit.Size = new System.Drawing.Size(77, 17);
			this.checkBoxInitMasking_Inherit.TabIndex = 4;
			this.checkBoxInitMasking_Inherit.Text = "Inherit Top";
			this.toolTip1.SetToolTip(this.checkBoxInitMasking_Inherit, "If checked, the parameter will be first initialized with your option of choice th" +
        "en it will inherit the value that was previosuly fitted");
			this.checkBoxInitMasking_Inherit.UseVisualStyleBackColor = true;
			this.checkBoxInitMasking_Inherit.CheckedChanged += new System.EventHandler(this.checkBoxInitMasking_Inherit_CheckedChanged);
			// 
			// floatTrackbarControlInit_CustomMaskingImportance
			// 
			this.floatTrackbarControlInit_CustomMaskingImportance.Location = new System.Drawing.Point(119, 1);
			this.floatTrackbarControlInit_CustomMaskingImportance.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlInit_CustomMaskingImportance.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlInit_CustomMaskingImportance.Name = "floatTrackbarControlInit_CustomMaskingImportance";
			this.floatTrackbarControlInit_CustomMaskingImportance.RangeMax = 1F;
			this.floatTrackbarControlInit_CustomMaskingImportance.RangeMin = 0F;
			this.floatTrackbarControlInit_CustomMaskingImportance.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlInit_CustomMaskingImportance.TabIndex = 2;
			this.floatTrackbarControlInit_CustomMaskingImportance.Value = 1F;
			this.floatTrackbarControlInit_CustomMaskingImportance.VisibleRangeMax = 1F;
			this.floatTrackbarControlInit_CustomMaskingImportance.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlInit_MaskingImportance_ValueChanged);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label8.Location = new System.Drawing.Point(3, 4);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(89, 13);
			this.label8.TabIndex = 1;
			this.label8.Text = "Initial Masking";
			// 
			// panel8
			// 
			this.panel8.Controls.Add(this.radioButtonInitFlatten_Custom);
			this.panel8.Controls.Add(this.radioButtonInitFlatten_Analytical);
			this.panel8.Controls.Add(this.radioButtonInitFlatten_Fixed);
			this.panel8.Controls.Add(this.radioButtonInitFlatten_NoChange);
			this.panel8.Controls.Add(this.checkBoxInitFlatten_InheritLeft);
			this.panel8.Controls.Add(this.floatTrackbarControlInit_FixedFlatten);
			this.panel8.Controls.Add(this.checkBoxInitFlatten_Inherit);
			this.panel8.Controls.Add(this.floatTrackbarControlInit_CustomFlatten);
			this.panel8.Controls.Add(this.label28);
			this.panel8.Location = new System.Drawing.Point(3, 363);
			this.panel8.Name = "panel8";
			this.panel8.Size = new System.Drawing.Size(353, 95);
			this.panel8.TabIndex = 8;
			// 
			// radioButtonInitFlatten_Custom
			// 
			this.radioButtonInitFlatten_Custom.AutoSize = true;
			this.radioButtonInitFlatten_Custom.Checked = true;
			this.radioButtonInitFlatten_Custom.Location = new System.Drawing.Point(102, 4);
			this.radioButtonInitFlatten_Custom.Name = "radioButtonInitFlatten_Custom";
			this.radioButtonInitFlatten_Custom.Size = new System.Drawing.Size(14, 13);
			this.radioButtonInitFlatten_Custom.TabIndex = 0;
			this.radioButtonInitFlatten_Custom.TabStop = true;
			this.radioButtonInitFlatten_Custom.UseVisualStyleBackColor = true;
			this.radioButtonInitFlatten_Custom.CheckedChanged += new System.EventHandler(this.radioButtonInitFlatten_CheckedChanged);
			// 
			// radioButtonInitFlatten_Analytical
			// 
			this.radioButtonInitFlatten_Analytical.AutoSize = true;
			this.radioButtonInitFlatten_Analytical.Location = new System.Drawing.Point(102, 72);
			this.radioButtonInitFlatten_Analytical.Name = "radioButtonInitFlatten_Analytical";
			this.radioButtonInitFlatten_Analytical.Size = new System.Drawing.Size(146, 17);
			this.radioButtonInitFlatten_Analytical.TabIndex = 0;
			this.radioButtonInitFlatten_Analytical.Text = "Use Analytical Expression";
			this.radioButtonInitFlatten_Analytical.UseVisualStyleBackColor = true;
			this.radioButtonInitFlatten_Analytical.CheckedChanged += new System.EventHandler(this.radioButtonInitFlatten_CheckedChanged);
			// 
			// radioButtonInitFlatten_Fixed
			// 
			this.radioButtonInitFlatten_Fixed.AutoSize = true;
			this.radioButtonInitFlatten_Fixed.Location = new System.Drawing.Point(102, 49);
			this.radioButtonInitFlatten_Fixed.Name = "radioButtonInitFlatten_Fixed";
			this.radioButtonInitFlatten_Fixed.Size = new System.Drawing.Size(50, 17);
			this.radioButtonInitFlatten_Fixed.TabIndex = 0;
			this.radioButtonInitFlatten_Fixed.Text = "Fixed";
			this.radioButtonInitFlatten_Fixed.UseVisualStyleBackColor = true;
			this.radioButtonInitFlatten_Fixed.CheckedChanged += new System.EventHandler(this.radioButtonInitFlatten_CheckedChanged);
			// 
			// radioButtonInitFlatten_NoChange
			// 
			this.radioButtonInitFlatten_NoChange.AutoSize = true;
			this.radioButtonInitFlatten_NoChange.Location = new System.Drawing.Point(102, 27);
			this.radioButtonInitFlatten_NoChange.Name = "radioButtonInitFlatten_NoChange";
			this.radioButtonInitFlatten_NoChange.Size = new System.Drawing.Size(139, 17);
			this.radioButtonInitFlatten_NoChange.TabIndex = 0;
			this.radioButtonInitFlatten_NoChange.Text = "No Change from Current";
			this.radioButtonInitFlatten_NoChange.UseVisualStyleBackColor = true;
			this.radioButtonInitFlatten_NoChange.CheckedChanged += new System.EventHandler(this.radioButtonInitFlatten_CheckedChanged);
			// 
			// checkBoxInitFlatten_InheritLeft
			// 
			this.checkBoxInitFlatten_InheritLeft.AutoSize = true;
			this.checkBoxInitFlatten_InheritLeft.Location = new System.Drawing.Point(6, 44);
			this.checkBoxInitFlatten_InheritLeft.Name = "checkBoxInitFlatten_InheritLeft";
			this.checkBoxInitFlatten_InheritLeft.Size = new System.Drawing.Size(76, 17);
			this.checkBoxInitFlatten_InheritLeft.TabIndex = 4;
			this.checkBoxInitFlatten_InheritLeft.Text = "Inherit Left";
			this.checkBoxInitFlatten_InheritLeft.UseVisualStyleBackColor = true;
			this.checkBoxInitFlatten_InheritLeft.CheckedChanged += new System.EventHandler(this.checkBoxInitFlatten_InheritLeft_CheckedChanged);
			// 
			// floatTrackbarControlInit_FixedFlatten
			// 
			this.floatTrackbarControlInit_FixedFlatten.Enabled = false;
			this.floatTrackbarControlInit_FixedFlatten.Location = new System.Drawing.Point(158, 47);
			this.floatTrackbarControlInit_FixedFlatten.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlInit_FixedFlatten.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlInit_FixedFlatten.Name = "floatTrackbarControlInit_FixedFlatten";
			this.floatTrackbarControlInit_FixedFlatten.RangeMax = 2F;
			this.floatTrackbarControlInit_FixedFlatten.RangeMin = 0F;
			this.floatTrackbarControlInit_FixedFlatten.Size = new System.Drawing.Size(164, 20);
			this.floatTrackbarControlInit_FixedFlatten.TabIndex = 2;
			this.floatTrackbarControlInit_FixedFlatten.Value = 1F;
			this.floatTrackbarControlInit_FixedFlatten.VisibleRangeMax = 2F;
			this.floatTrackbarControlInit_FixedFlatten.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlInit_FixedFlatten_ValueChanged);
			// 
			// checkBoxInitFlatten_Inherit
			// 
			this.checkBoxInitFlatten_Inherit.AutoSize = true;
			this.checkBoxInitFlatten_Inherit.Location = new System.Drawing.Point(6, 21);
			this.checkBoxInitFlatten_Inherit.Name = "checkBoxInitFlatten_Inherit";
			this.checkBoxInitFlatten_Inherit.Size = new System.Drawing.Size(77, 17);
			this.checkBoxInitFlatten_Inherit.TabIndex = 4;
			this.checkBoxInitFlatten_Inherit.Text = "Inherit Top";
			this.toolTip1.SetToolTip(this.checkBoxInitFlatten_Inherit, "If checked, the parameter will be first initialized with your option of choice th" +
        "en it will inherit the value that was previosuly fitted");
			this.checkBoxInitFlatten_Inherit.UseVisualStyleBackColor = true;
			this.checkBoxInitFlatten_Inherit.CheckedChanged += new System.EventHandler(this.checkBoxInitFlatten_Inherit_CheckedChanged);
			// 
			// floatTrackbarControlInit_CustomFlatten
			// 
			this.floatTrackbarControlInit_CustomFlatten.Location = new System.Drawing.Point(119, 1);
			this.floatTrackbarControlInit_CustomFlatten.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlInit_CustomFlatten.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlInit_CustomFlatten.Name = "floatTrackbarControlInit_CustomFlatten";
			this.floatTrackbarControlInit_CustomFlatten.RangeMax = 2F;
			this.floatTrackbarControlInit_CustomFlatten.RangeMin = 0F;
			this.floatTrackbarControlInit_CustomFlatten.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlInit_CustomFlatten.TabIndex = 2;
			this.toolTip1.SetToolTip(this.floatTrackbarControlInit_CustomFlatten, "Specifies the lobe\'s flattening to start with (diffuse lobes tend to be flatter)");
			this.floatTrackbarControlInit_CustomFlatten.Value = 0.5F;
			this.floatTrackbarControlInit_CustomFlatten.VisibleRangeMax = 2F;
			this.floatTrackbarControlInit_CustomFlatten.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlInit_Flatten_ValueChanged);
			// 
			// label28
			// 
			this.label28.AutoSize = true;
			this.label28.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label28.Location = new System.Drawing.Point(3, 4);
			this.label28.Name = "label28";
			this.label28.Size = new System.Drawing.Size(98, 13);
			this.label28.TabIndex = 1;
			this.label28.Text = "Initial Flattening";
			// 
			// panel7
			// 
			this.panel7.Controls.Add(this.radioButtonInitScale_CoMFactor);
			this.panel7.Controls.Add(this.radioButtonInitScale_Analytical);
			this.panel7.Controls.Add(this.radioButtonInitScale_Fixed);
			this.panel7.Controls.Add(this.radioButtonInitScale_NoChange);
			this.panel7.Controls.Add(this.floatTrackbarControlInit_FixedScale);
			this.panel7.Controls.Add(this.floatTrackbarControlInit_Scale);
			this.panel7.Controls.Add(this.checkBoxInitScale_InheritLeft);
			this.panel7.Controls.Add(this.checkBoxInitScale_Inherit);
			this.panel7.Controls.Add(this.label27);
			this.panel7.Location = new System.Drawing.Point(3, 239);
			this.panel7.Name = "panel7";
			this.panel7.Size = new System.Drawing.Size(353, 118);
			this.panel7.TabIndex = 7;
			// 
			// radioButtonInitScale_CoMFactor
			// 
			this.radioButtonInitScale_CoMFactor.AutoSize = true;
			this.radioButtonInitScale_CoMFactor.Checked = true;
			this.radioButtonInitScale_CoMFactor.Location = new System.Drawing.Point(102, 3);
			this.radioButtonInitScale_CoMFactor.Name = "radioButtonInitScale_CoMFactor";
			this.radioButtonInitScale_CoMFactor.Size = new System.Drawing.Size(218, 17);
			this.radioButtonInitScale_CoMFactor.TabIndex = 0;
			this.radioButtonInitScale_CoMFactor.TabStop = true;
			this.radioButtonInitScale_CoMFactor.Text = "Factor of Center of Mass Vector\'s Length";
			this.radioButtonInitScale_CoMFactor.UseVisualStyleBackColor = true;
			this.radioButtonInitScale_CoMFactor.CheckedChanged += new System.EventHandler(this.radioButtonInitScale_CheckedChanged);
			// 
			// radioButtonInitScale_Analytical
			// 
			this.radioButtonInitScale_Analytical.AutoSize = true;
			this.radioButtonInitScale_Analytical.Location = new System.Drawing.Point(102, 93);
			this.radioButtonInitScale_Analytical.Name = "radioButtonInitScale_Analytical";
			this.radioButtonInitScale_Analytical.Size = new System.Drawing.Size(146, 17);
			this.radioButtonInitScale_Analytical.TabIndex = 0;
			this.radioButtonInitScale_Analytical.Text = "Use Analytical Expression";
			this.radioButtonInitScale_Analytical.UseVisualStyleBackColor = true;
			this.radioButtonInitScale_Analytical.CheckedChanged += new System.EventHandler(this.radioButtonInitScale_CheckedChanged);
			// 
			// radioButtonInitScale_Fixed
			// 
			this.radioButtonInitScale_Fixed.AutoSize = true;
			this.radioButtonInitScale_Fixed.Location = new System.Drawing.Point(102, 70);
			this.radioButtonInitScale_Fixed.Name = "radioButtonInitScale_Fixed";
			this.radioButtonInitScale_Fixed.Size = new System.Drawing.Size(50, 17);
			this.radioButtonInitScale_Fixed.TabIndex = 0;
			this.radioButtonInitScale_Fixed.Text = "Fixed";
			this.radioButtonInitScale_Fixed.UseVisualStyleBackColor = true;
			this.radioButtonInitScale_Fixed.CheckedChanged += new System.EventHandler(this.radioButtonInitScale_CheckedChanged);
			// 
			// radioButtonInitScale_NoChange
			// 
			this.radioButtonInitScale_NoChange.AutoSize = true;
			this.radioButtonInitScale_NoChange.Location = new System.Drawing.Point(102, 47);
			this.radioButtonInitScale_NoChange.Name = "radioButtonInitScale_NoChange";
			this.radioButtonInitScale_NoChange.Size = new System.Drawing.Size(139, 17);
			this.radioButtonInitScale_NoChange.TabIndex = 0;
			this.radioButtonInitScale_NoChange.Text = "No Change from Current";
			this.radioButtonInitScale_NoChange.UseVisualStyleBackColor = true;
			this.radioButtonInitScale_NoChange.CheckedChanged += new System.EventHandler(this.radioButtonInitScale_CheckedChanged);
			// 
			// floatTrackbarControlInit_FixedScale
			// 
			this.floatTrackbarControlInit_FixedScale.Enabled = false;
			this.floatTrackbarControlInit_FixedScale.Location = new System.Drawing.Point(158, 68);
			this.floatTrackbarControlInit_FixedScale.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlInit_FixedScale.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlInit_FixedScale.Name = "floatTrackbarControlInit_FixedScale";
			this.floatTrackbarControlInit_FixedScale.RangeMax = 1F;
			this.floatTrackbarControlInit_FixedScale.RangeMin = 0F;
			this.floatTrackbarControlInit_FixedScale.Size = new System.Drawing.Size(164, 20);
			this.floatTrackbarControlInit_FixedScale.TabIndex = 2;
			this.floatTrackbarControlInit_FixedScale.Value = 1F;
			this.floatTrackbarControlInit_FixedScale.VisibleRangeMax = 1F;
			this.floatTrackbarControlInit_FixedScale.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlInit_FixedScale_ValueChanged);
			// 
			// floatTrackbarControlInit_Scale
			// 
			this.floatTrackbarControlInit_Scale.Location = new System.Drawing.Point(119, 21);
			this.floatTrackbarControlInit_Scale.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlInit_Scale.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlInit_Scale.Name = "floatTrackbarControlInit_Scale";
			this.floatTrackbarControlInit_Scale.RangeMax = 1F;
			this.floatTrackbarControlInit_Scale.RangeMin = 0F;
			this.floatTrackbarControlInit_Scale.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlInit_Scale.TabIndex = 2;
			this.toolTip1.SetToolTip(this.floatTrackbarControlInit_Scale, "Specifies the percentage of the size of the simulated lobe to start with (smaller" +
        " initial lobes make simulation converge faster)");
			this.floatTrackbarControlInit_Scale.Value = 0.05F;
			this.floatTrackbarControlInit_Scale.VisibleRangeMax = 1F;
			this.floatTrackbarControlInit_Scale.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlInit_Scale_ValueChanged);
			// 
			// checkBoxInitScale_InheritLeft
			// 
			this.checkBoxInitScale_InheritLeft.AutoSize = true;
			this.checkBoxInitScale_InheritLeft.Location = new System.Drawing.Point(6, 47);
			this.checkBoxInitScale_InheritLeft.Name = "checkBoxInitScale_InheritLeft";
			this.checkBoxInitScale_InheritLeft.Size = new System.Drawing.Size(76, 17);
			this.checkBoxInitScale_InheritLeft.TabIndex = 4;
			this.checkBoxInitScale_InheritLeft.Text = "Inherit Left";
			this.checkBoxInitScale_InheritLeft.UseVisualStyleBackColor = true;
			this.checkBoxInitScale_InheritLeft.CheckedChanged += new System.EventHandler(this.checkBoxInitScale_InheritLeft_CheckedChanged);
			// 
			// checkBoxInitScale_Inherit
			// 
			this.checkBoxInitScale_Inherit.AutoSize = true;
			this.checkBoxInitScale_Inherit.Location = new System.Drawing.Point(6, 24);
			this.checkBoxInitScale_Inherit.Name = "checkBoxInitScale_Inherit";
			this.checkBoxInitScale_Inherit.Size = new System.Drawing.Size(77, 17);
			this.checkBoxInitScale_Inherit.TabIndex = 4;
			this.checkBoxInitScale_Inherit.Text = "Inherit Top";
			this.toolTip1.SetToolTip(this.checkBoxInitScale_Inherit, "If checked, the parameter will be first initialized with your option of choice th" +
        "en it will inherit the value that was previosuly fitted");
			this.checkBoxInitScale_Inherit.UseVisualStyleBackColor = true;
			this.checkBoxInitScale_Inherit.CheckedChanged += new System.EventHandler(this.checkBoxInitScale_Inherit_CheckedChanged);
			// 
			// label27
			// 
			this.label27.AutoSize = true;
			this.label27.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label27.Location = new System.Drawing.Point(3, 5);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(74, 13);
			this.label27.TabIndex = 1;
			this.label27.Text = "Initial Scale";
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.radioButtonInitDirection_TowardCoM);
			this.panel4.Controls.Add(this.radioButtonInitDirection_Fixed);
			this.panel4.Controls.Add(this.radioButtonInitDirection_NoChange);
			this.panel4.Controls.Add(this.radioButtonInitDirection_TowardReflected);
			this.panel4.Controls.Add(this.checkBoxInitDirection_InheritLeft);
			this.panel4.Controls.Add(this.floatTrackbarControlInit_FixedDirection);
			this.panel4.Controls.Add(this.checkBoxInitDirection_Inherit);
			this.panel4.Controls.Add(this.label10);
			this.panel4.Location = new System.Drawing.Point(3, 19);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(353, 87);
			this.panel4.TabIndex = 6;
			// 
			// radioButtonInitDirection_TowardCoM
			// 
			this.radioButtonInitDirection_TowardCoM.AutoSize = true;
			this.radioButtonInitDirection_TowardCoM.Checked = true;
			this.radioButtonInitDirection_TowardCoM.Location = new System.Drawing.Point(102, 2);
			this.radioButtonInitDirection_TowardCoM.Name = "radioButtonInitDirection_TowardCoM";
			this.radioButtonInitDirection_TowardCoM.Size = new System.Drawing.Size(96, 17);
			this.radioButtonInitDirection_TowardCoM.TabIndex = 0;
			this.radioButtonInitDirection_TowardCoM.TabStop = true;
			this.radioButtonInitDirection_TowardCoM.Text = "Center of Mass";
			this.toolTip1.SetToolTip(this.radioButtonInitDirection_TowardCoM, "Initial orientation is aligned toward the Center of Mass of the simulated lobe (b" +
        "etter estimate)");
			this.radioButtonInitDirection_TowardCoM.UseVisualStyleBackColor = true;
			this.radioButtonInitDirection_TowardCoM.CheckedChanged += new System.EventHandler(this.radioButtonInitDirection_CheckedChanged);
			// 
			// radioButtonInitDirection_Fixed
			// 
			this.radioButtonInitDirection_Fixed.AutoSize = true;
			this.radioButtonInitDirection_Fixed.Location = new System.Drawing.Point(102, 63);
			this.radioButtonInitDirection_Fixed.Name = "radioButtonInitDirection_Fixed";
			this.radioButtonInitDirection_Fixed.Size = new System.Drawing.Size(50, 17);
			this.radioButtonInitDirection_Fixed.TabIndex = 0;
			this.radioButtonInitDirection_Fixed.Text = "Fixed";
			this.radioButtonInitDirection_Fixed.UseVisualStyleBackColor = true;
			this.radioButtonInitDirection_Fixed.CheckedChanged += new System.EventHandler(this.radioButtonInitDirection_CheckedChanged);
			// 
			// radioButtonInitDirection_NoChange
			// 
			this.radioButtonInitDirection_NoChange.AutoSize = true;
			this.radioButtonInitDirection_NoChange.Location = new System.Drawing.Point(102, 40);
			this.radioButtonInitDirection_NoChange.Name = "radioButtonInitDirection_NoChange";
			this.radioButtonInitDirection_NoChange.Size = new System.Drawing.Size(139, 17);
			this.radioButtonInitDirection_NoChange.TabIndex = 0;
			this.radioButtonInitDirection_NoChange.Text = "No Change from Current";
			this.radioButtonInitDirection_NoChange.UseVisualStyleBackColor = true;
			this.radioButtonInitDirection_NoChange.CheckedChanged += new System.EventHandler(this.radioButtonInitDirection_CheckedChanged);
			// 
			// radioButtonInitDirection_TowardReflected
			// 
			this.radioButtonInitDirection_TowardReflected.AutoSize = true;
			this.radioButtonInitDirection_TowardReflected.Location = new System.Drawing.Point(102, 21);
			this.radioButtonInitDirection_TowardReflected.Name = "radioButtonInitDirection_TowardReflected";
			this.radioButtonInitDirection_TowardReflected.Size = new System.Drawing.Size(168, 17);
			this.radioButtonInitDirection_TowardReflected.TabIndex = 0;
			this.radioButtonInitDirection_TowardReflected.Text = "Reflected/Refracted Direction";
			this.toolTip1.SetToolTip(this.radioButtonInitDirection_TowardReflected, "Initial orientation is aligned toward the reflected/refracted direction");
			this.radioButtonInitDirection_TowardReflected.UseVisualStyleBackColor = true;
			this.radioButtonInitDirection_TowardReflected.CheckedChanged += new System.EventHandler(this.radioButtonInitDirection_CheckedChanged);
			// 
			// checkBoxInitDirection_InheritLeft
			// 
			this.checkBoxInitDirection_InheritLeft.AutoSize = true;
			this.checkBoxInitDirection_InheritLeft.Location = new System.Drawing.Point(6, 45);
			this.checkBoxInitDirection_InheritLeft.Name = "checkBoxInitDirection_InheritLeft";
			this.checkBoxInitDirection_InheritLeft.Size = new System.Drawing.Size(76, 17);
			this.checkBoxInitDirection_InheritLeft.TabIndex = 4;
			this.checkBoxInitDirection_InheritLeft.Text = "Inherit Left";
			this.checkBoxInitDirection_InheritLeft.UseVisualStyleBackColor = true;
			this.checkBoxInitDirection_InheritLeft.CheckedChanged += new System.EventHandler(this.checkBoxInitDirection_InheritLeft_CheckedChanged);
			// 
			// floatTrackbarControlInit_FixedDirection
			// 
			this.floatTrackbarControlInit_FixedDirection.Enabled = false;
			this.floatTrackbarControlInit_FixedDirection.Location = new System.Drawing.Point(158, 61);
			this.floatTrackbarControlInit_FixedDirection.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlInit_FixedDirection.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlInit_FixedDirection.Name = "floatTrackbarControlInit_FixedDirection";
			this.floatTrackbarControlInit_FixedDirection.RangeMax = 90F;
			this.floatTrackbarControlInit_FixedDirection.RangeMin = 0F;
			this.floatTrackbarControlInit_FixedDirection.Size = new System.Drawing.Size(164, 20);
			this.floatTrackbarControlInit_FixedDirection.TabIndex = 2;
			this.floatTrackbarControlInit_FixedDirection.Value = 0F;
			this.floatTrackbarControlInit_FixedDirection.VisibleRangeMax = 90F;
			this.floatTrackbarControlInit_FixedDirection.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlInit_FixedDirection_ValueChanged);
			// 
			// checkBoxInitDirection_Inherit
			// 
			this.checkBoxInitDirection_Inherit.AutoSize = true;
			this.checkBoxInitDirection_Inherit.Location = new System.Drawing.Point(6, 22);
			this.checkBoxInitDirection_Inherit.Name = "checkBoxInitDirection_Inherit";
			this.checkBoxInitDirection_Inherit.Size = new System.Drawing.Size(77, 17);
			this.checkBoxInitDirection_Inherit.TabIndex = 4;
			this.checkBoxInitDirection_Inherit.Text = "Inherit Top";
			this.toolTip1.SetToolTip(this.checkBoxInitDirection_Inherit, "If checked, the parameter will be first initialized with your option of choice th" +
        "en it will inherit the value that was previosuly fitted");
			this.checkBoxInitDirection_Inherit.UseVisualStyleBackColor = true;
			this.checkBoxInitDirection_Inherit.CheckedChanged += new System.EventHandler(this.checkBoxInitDirection_Inherit_CheckedChanged);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label10.Location = new System.Drawing.Point(3, 4);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(93, 13);
			this.label10.TabIndex = 1;
			this.label10.Text = "Initial Direction";
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.floatTrackbarControlInit_CustomRoughness);
			this.panel3.Controls.Add(this.radioButtonInitRoughness_Analytical);
			this.panel3.Controls.Add(this.radioButtonInitRoughness_Fixed);
			this.panel3.Controls.Add(this.radioButtonInitRoughness_NoChange);
			this.panel3.Controls.Add(this.radioButtonInitRoughness_UseSurface);
			this.panel3.Controls.Add(this.floatTrackbarControlInit_FixedRoughness);
			this.panel3.Controls.Add(this.checkBoxInitRoughness_InheritLeft);
			this.panel3.Controls.Add(this.checkBoxInitRoughness_Inherit);
			this.panel3.Controls.Add(this.radioButtonInitRoughness_Custom);
			this.panel3.Controls.Add(this.label9);
			this.panel3.Location = new System.Drawing.Point(3, 112);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(356, 121);
			this.panel3.TabIndex = 5;
			// 
			// floatTrackbarControlInit_CustomRoughness
			// 
			this.floatTrackbarControlInit_CustomRoughness.Enabled = false;
			this.floatTrackbarControlInit_CustomRoughness.Location = new System.Drawing.Point(125, 22);
			this.floatTrackbarControlInit_CustomRoughness.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlInit_CustomRoughness.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlInit_CustomRoughness.Name = "floatTrackbarControlInit_CustomRoughness";
			this.floatTrackbarControlInit_CustomRoughness.RangeMax = 1F;
			this.floatTrackbarControlInit_CustomRoughness.RangeMin = 0F;
			this.floatTrackbarControlInit_CustomRoughness.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlInit_CustomRoughness.TabIndex = 2;
			this.floatTrackbarControlInit_CustomRoughness.Value = 0.5F;
			this.floatTrackbarControlInit_CustomRoughness.VisibleRangeMax = 1F;
			this.floatTrackbarControlInit_CustomRoughness.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlInit_CustomRoughness_ValueChanged);
			// 
			// radioButtonInitRoughness_Analytical
			// 
			this.radioButtonInitRoughness_Analytical.AutoSize = true;
			this.radioButtonInitRoughness_Analytical.Location = new System.Drawing.Point(105, 95);
			this.radioButtonInitRoughness_Analytical.Name = "radioButtonInitRoughness_Analytical";
			this.radioButtonInitRoughness_Analytical.Size = new System.Drawing.Size(146, 17);
			this.radioButtonInitRoughness_Analytical.TabIndex = 0;
			this.radioButtonInitRoughness_Analytical.Text = "Use Analytical Expression";
			this.radioButtonInitRoughness_Analytical.UseVisualStyleBackColor = true;
			this.radioButtonInitRoughness_Analytical.CheckedChanged += new System.EventHandler(this.radioButtonInitRoughness_CheckedChanged);
			// 
			// radioButtonInitRoughness_Fixed
			// 
			this.radioButtonInitRoughness_Fixed.AutoSize = true;
			this.radioButtonInitRoughness_Fixed.Location = new System.Drawing.Point(105, 71);
			this.radioButtonInitRoughness_Fixed.Name = "radioButtonInitRoughness_Fixed";
			this.radioButtonInitRoughness_Fixed.Size = new System.Drawing.Size(50, 17);
			this.radioButtonInitRoughness_Fixed.TabIndex = 0;
			this.radioButtonInitRoughness_Fixed.Text = "Fixed";
			this.radioButtonInitRoughness_Fixed.UseVisualStyleBackColor = true;
			this.radioButtonInitRoughness_Fixed.CheckedChanged += new System.EventHandler(this.radioButtonInitRoughness_CheckedChanged);
			// 
			// radioButtonInitRoughness_NoChange
			// 
			this.radioButtonInitRoughness_NoChange.AutoSize = true;
			this.radioButtonInitRoughness_NoChange.Location = new System.Drawing.Point(105, 48);
			this.radioButtonInitRoughness_NoChange.Name = "radioButtonInitRoughness_NoChange";
			this.radioButtonInitRoughness_NoChange.Size = new System.Drawing.Size(139, 17);
			this.radioButtonInitRoughness_NoChange.TabIndex = 0;
			this.radioButtonInitRoughness_NoChange.Text = "No Change from Current";
			this.radioButtonInitRoughness_NoChange.UseVisualStyleBackColor = true;
			this.radioButtonInitRoughness_NoChange.CheckedChanged += new System.EventHandler(this.radioButtonInitRoughness_CheckedChanged);
			// 
			// radioButtonInitRoughness_UseSurface
			// 
			this.radioButtonInitRoughness_UseSurface.AutoSize = true;
			this.radioButtonInitRoughness_UseSurface.Checked = true;
			this.radioButtonInitRoughness_UseSurface.Location = new System.Drawing.Point(105, 3);
			this.radioButtonInitRoughness_UseSurface.Name = "radioButtonInitRoughness_UseSurface";
			this.radioButtonInitRoughness_UseSurface.Size = new System.Drawing.Size(119, 17);
			this.radioButtonInitRoughness_UseSurface.TabIndex = 0;
			this.radioButtonInitRoughness_UseSurface.TabStop = true;
			this.radioButtonInitRoughness_UseSurface.Text = "Surface Roughness";
			this.radioButtonInitRoughness_UseSurface.UseVisualStyleBackColor = true;
			this.radioButtonInitRoughness_UseSurface.CheckedChanged += new System.EventHandler(this.radioButtonInitRoughness_CheckedChanged);
			// 
			// floatTrackbarControlInit_FixedRoughness
			// 
			this.floatTrackbarControlInit_FixedRoughness.Enabled = false;
			this.floatTrackbarControlInit_FixedRoughness.Location = new System.Drawing.Point(161, 69);
			this.floatTrackbarControlInit_FixedRoughness.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlInit_FixedRoughness.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlInit_FixedRoughness.Name = "floatTrackbarControlInit_FixedRoughness";
			this.floatTrackbarControlInit_FixedRoughness.RangeMax = 1F;
			this.floatTrackbarControlInit_FixedRoughness.RangeMin = 0F;
			this.floatTrackbarControlInit_FixedRoughness.Size = new System.Drawing.Size(164, 20);
			this.floatTrackbarControlInit_FixedRoughness.TabIndex = 2;
			this.floatTrackbarControlInit_FixedRoughness.Value = 1F;
			this.floatTrackbarControlInit_FixedRoughness.VisibleRangeMax = 1F;
			this.floatTrackbarControlInit_FixedRoughness.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlInit_FixedRoughness_ValueChanged);
			// 
			// checkBoxInitRoughness_InheritLeft
			// 
			this.checkBoxInitRoughness_InheritLeft.AutoSize = true;
			this.checkBoxInitRoughness_InheritLeft.Location = new System.Drawing.Point(9, 47);
			this.checkBoxInitRoughness_InheritLeft.Name = "checkBoxInitRoughness_InheritLeft";
			this.checkBoxInitRoughness_InheritLeft.Size = new System.Drawing.Size(76, 17);
			this.checkBoxInitRoughness_InheritLeft.TabIndex = 4;
			this.checkBoxInitRoughness_InheritLeft.Text = "Inherit Left";
			this.checkBoxInitRoughness_InheritLeft.UseVisualStyleBackColor = true;
			this.checkBoxInitRoughness_InheritLeft.CheckedChanged += new System.EventHandler(this.checkBoxInitRoughness_InheritLeft_CheckedChanged);
			// 
			// checkBoxInitRoughness_Inherit
			// 
			this.checkBoxInitRoughness_Inherit.AutoSize = true;
			this.checkBoxInitRoughness_Inherit.Location = new System.Drawing.Point(9, 25);
			this.checkBoxInitRoughness_Inherit.Name = "checkBoxInitRoughness_Inherit";
			this.checkBoxInitRoughness_Inherit.Size = new System.Drawing.Size(77, 17);
			this.checkBoxInitRoughness_Inherit.TabIndex = 4;
			this.checkBoxInitRoughness_Inherit.Text = "Inherit Top";
			this.toolTip1.SetToolTip(this.checkBoxInitRoughness_Inherit, "If checked, the parameter will be first initialized with your option of choice th" +
        "en it will inherit the value that was previosuly fitted");
			this.checkBoxInitRoughness_Inherit.UseVisualStyleBackColor = true;
			this.checkBoxInitRoughness_Inherit.CheckedChanged += new System.EventHandler(this.checkBoxInitRoughness_Inherit_CheckedChanged);
			// 
			// radioButtonInitRoughness_Custom
			// 
			this.radioButtonInitRoughness_Custom.AutoSize = true;
			this.radioButtonInitRoughness_Custom.Location = new System.Drawing.Point(105, 26);
			this.radioButtonInitRoughness_Custom.Name = "radioButtonInitRoughness_Custom";
			this.radioButtonInitRoughness_Custom.Size = new System.Drawing.Size(14, 13);
			this.radioButtonInitRoughness_Custom.TabIndex = 0;
			this.radioButtonInitRoughness_Custom.UseVisualStyleBackColor = true;
			this.radioButtonInitRoughness_Custom.CheckedChanged += new System.EventHandler(this.radioButtonInitRoughness_CheckedChanged);
			// 
			// label9
			// 
			this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label9.Location = new System.Drawing.Point(3, 5);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(105, 18);
			this.label9.TabIndex = 1;
			this.label9.Text = "Initial Roughness";
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.label7);
			this.panel2.Controls.Add(this.radioButtonLobe_GGX);
			this.panel2.Controls.Add(this.radioButtonLobe_ModifiedPhongAniso);
			this.panel2.Controls.Add(this.radioButtonLobe_Beckmann);
			this.panel2.Controls.Add(this.radioButtonLobe_ModifiedPhong);
			this.panel2.Location = new System.Drawing.Point(6, 19);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(374, 27);
			this.panel2.TabIndex = 1;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(6, 5);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(71, 13);
			this.label7.TabIndex = 1;
			this.label7.Text = "Lobe Type:";
			// 
			// radioButtonLobe_GGX
			// 
			this.radioButtonLobe_GGX.AutoSize = true;
			this.radioButtonLobe_GGX.Enabled = false;
			this.radioButtonLobe_GGX.Location = new System.Drawing.Point(323, 3);
			this.radioButtonLobe_GGX.Name = "radioButtonLobe_GGX";
			this.radioButtonLobe_GGX.Size = new System.Drawing.Size(48, 17);
			this.radioButtonLobe_GGX.TabIndex = 0;
			this.radioButtonLobe_GGX.Text = "GGX";
			this.radioButtonLobe_GGX.UseVisualStyleBackColor = true;
			this.radioButtonLobe_GGX.CheckedChanged += new System.EventHandler(this.LobeTypeCheckChanged);
			// 
			// radioButtonLobe_ModifiedPhongAniso
			// 
			this.radioButtonLobe_ModifiedPhongAniso.AutoSize = true;
			this.radioButtonLobe_ModifiedPhongAniso.Location = new System.Drawing.Point(153, 3);
			this.radioButtonLobe_ModifiedPhongAniso.Name = "radioButtonLobe_ModifiedPhongAniso";
			this.radioButtonLobe_ModifiedPhongAniso.Size = new System.Drawing.Size(85, 17);
			this.radioButtonLobe_ModifiedPhongAniso.TabIndex = 0;
			this.radioButtonLobe_ModifiedPhongAniso.Text = "Phong Aniso";
			this.radioButtonLobe_ModifiedPhongAniso.UseVisualStyleBackColor = true;
			this.radioButtonLobe_ModifiedPhongAniso.CheckedChanged += new System.EventHandler(this.LobeTypeCheckChanged);
			// 
			// radioButtonLobe_Beckmann
			// 
			this.radioButtonLobe_Beckmann.AutoSize = true;
			this.radioButtonLobe_Beckmann.Enabled = false;
			this.radioButtonLobe_Beckmann.Location = new System.Drawing.Point(244, 3);
			this.radioButtonLobe_Beckmann.Name = "radioButtonLobe_Beckmann";
			this.radioButtonLobe_Beckmann.Size = new System.Drawing.Size(76, 17);
			this.radioButtonLobe_Beckmann.TabIndex = 0;
			this.radioButtonLobe_Beckmann.Text = "Beckmann";
			this.radioButtonLobe_Beckmann.UseVisualStyleBackColor = true;
			this.radioButtonLobe_Beckmann.CheckedChanged += new System.EventHandler(this.LobeTypeCheckChanged);
			// 
			// radioButtonLobe_ModifiedPhong
			// 
			this.radioButtonLobe_ModifiedPhong.AutoSize = true;
			this.radioButtonLobe_ModifiedPhong.Checked = true;
			this.radioButtonLobe_ModifiedPhong.Location = new System.Drawing.Point(83, 3);
			this.radioButtonLobe_ModifiedPhong.Name = "radioButtonLobe_ModifiedPhong";
			this.radioButtonLobe_ModifiedPhong.Size = new System.Drawing.Size(73, 17);
			this.radioButtonLobe_ModifiedPhong.TabIndex = 0;
			this.radioButtonLobe_ModifiedPhong.TabStop = true;
			this.radioButtonLobe_ModifiedPhong.Text = "Phong Iso";
			this.radioButtonLobe_ModifiedPhong.UseVisualStyleBackColor = true;
			this.radioButtonLobe_ModifiedPhong.CheckedChanged += new System.EventHandler(this.LobeTypeCheckChanged);
			// 
			// label30
			// 
			this.label30.AutoSize = true;
			this.label30.Location = new System.Drawing.Point(317, 626);
			this.label30.Name = "label30";
			this.label30.Size = new System.Drawing.Size(80, 13);
			this.label30.TabIndex = 1;
			this.label30.Text = "albedo/F0 slice";
			// 
			// label29
			// 
			this.label29.AutoSize = true;
			this.label29.Location = new System.Drawing.Point(-3, 626);
			this.label29.Name = "label29";
			this.label29.Size = new System.Drawing.Size(106, 13);
			this.label29.TabIndex = 1;
			this.label29.Text = "View scattering order";
			// 
			// contextMenuStripSelection
			// 
			this.contextMenuStripSelection.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.computeToolStripMenuItem,
            this.startFromHereToolStripMenuItem,
            this.toolStripMenuItem3,
            this.clearToolStripMenuItem,
            this.clearColumnToolStripMenuItem,
            this.clearRowToolStripMenuItem,
            this.clearSliceFromHereToolStripMenuItem});
			this.contextMenuStripSelection.Name = "contextMenuStripSelection";
			this.contextMenuStripSelection.Size = new System.Drawing.Size(209, 142);
			this.contextMenuStripSelection.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripSelection_Opening);
			// 
			// computeToolStripMenuItem
			// 
			this.computeToolStripMenuItem.Name = "computeToolStripMenuItem";
			this.computeToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
			this.computeToolStripMenuItem.Text = "&Compute";
			this.computeToolStripMenuItem.Click += new System.EventHandler(this.computeToolStripMenuItem_Click);
			// 
			// startFromHereToolStripMenuItem
			// 
			this.startFromHereToolStripMenuItem.Name = "startFromHereToolStripMenuItem";
			this.startFromHereToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
			this.startFromHereToolStripMenuItem.Text = "Compute &Slice from Here";
			this.startFromHereToolStripMenuItem.Click += new System.EventHandler(this.startFromHereToolStripMenuItem_Click);
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(205, 6);
			// 
			// clearToolStripMenuItem
			// 
			this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
			this.clearToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
			this.clearToolStripMenuItem.Text = "Clea&r Single Result";
			this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
			// 
			// clearColumnToolStripMenuItem
			// 
			this.clearColumnToolStripMenuItem.Name = "clearColumnToolStripMenuItem";
			this.clearColumnToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
			this.clearColumnToolStripMenuItem.Text = "Clear Column from Here";
			this.clearColumnToolStripMenuItem.Click += new System.EventHandler(this.clearColumnToolStripMenuItem_Click);
			// 
			// clearRowToolStripMenuItem
			// 
			this.clearRowToolStripMenuItem.Name = "clearRowToolStripMenuItem";
			this.clearRowToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
			this.clearRowToolStripMenuItem.Text = "Clear Row from Here";
			this.clearRowToolStripMenuItem.Click += new System.EventHandler(this.clearRowToolStripMenuItem_Click);
			// 
			// clearSliceFromHereToolStripMenuItem
			// 
			this.clearSliceFromHereToolStripMenuItem.Name = "clearSliceFromHereToolStripMenuItem";
			this.clearSliceFromHereToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
			this.clearSliceFromHereToolStripMenuItem.Text = "Clear Slice from Here";
			this.clearSliceFromHereToolStripMenuItem.Click += new System.EventHandler(this.clearSliceFromHereToolStripMenuItem_Click);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.resultsToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1057, 24);
			this.menuStrip1.TabIndex = 2;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripMenuItem2,
            this.recentToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// newToolStripMenuItem
			// 
			this.newToolStripMenuItem.Name = "newToolStripMenuItem";
			this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.newToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.newToolStripMenuItem.Text = "&New";
			this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.openToolStripMenuItem.Text = "&Open";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.saveToolStripMenuItem.Text = "&Save";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
			this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.saveAsToolStripMenuItem.Text = "Save As...";
			this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(192, 6);
			// 
			// recentToolStripMenuItem
			// 
			this.recentToolStripMenuItem.Name = "recentToolStripMenuItem";
			this.recentToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.recentToolStripMenuItem.Text = "&Recent";
			// 
			// resultsToolStripMenuItem
			// 
			this.resultsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToolStripMenuItem,
            this.importToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exportTotalReflectanceToolStripMenuItem});
			this.resultsToolStripMenuItem.Name = "resultsToolStripMenuItem";
			this.resultsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
			this.resultsToolStripMenuItem.Text = "&Results";
			// 
			// exportToolStripMenuItem
			// 
			this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
			this.exportToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
			this.exportToolStripMenuItem.Text = "E&xport";
			this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
			// 
			// importToolStripMenuItem
			// 
			this.importToolStripMenuItem.Name = "importToolStripMenuItem";
			this.importToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
			this.importToolStripMenuItem.Text = "&Import";
			this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
			// 
			// buttonCompute
			// 
			this.buttonCompute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonCompute.BackColor = System.Drawing.Color.MediumAquamarine;
			this.buttonCompute.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonCompute.Location = new System.Drawing.Point(443, 789);
			this.buttonCompute.Name = "buttonCompute";
			this.buttonCompute.Size = new System.Drawing.Size(170, 39);
			this.buttonCompute.TabIndex = 3;
			this.buttonCompute.Text = "&Start";
			this.buttonCompute.UseVisualStyleBackColor = false;
			this.buttonCompute.Click += new System.EventHandler(this.buttonCompute_Click);
			// 
			// buttonClearResults
			// 
			this.buttonClearResults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonClearResults.Location = new System.Drawing.Point(12, 798);
			this.buttonClearResults.Name = "buttonClearResults";
			this.buttonClearResults.Size = new System.Drawing.Size(132, 23);
			this.buttonClearResults.TabIndex = 4;
			this.buttonClearResults.Text = "Clear Existing Results";
			this.buttonClearResults.UseVisualStyleBackColor = true;
			this.buttonClearResults.Click += new System.EventHandler(this.buttonClearResults_Click);
			// 
			// openFileDialogResults
			// 
			this.openFileDialogResults.DefaultExt = "*.xml";
			this.openFileDialogResults.FileName = "result.xml";
			this.openFileDialogResults.Filter = "Result Files (*.xml)|*.xml|All Files|*.*";
			this.openFileDialogResults.Title = "Choose an XML results file to open";
			// 
			// saveFileDialogResults
			// 
			this.saveFileDialogResults.DefaultExt = "*.xml";
			this.saveFileDialogResults.Filter = "Result Files (*.xml)|*.xml|All Files|*.*";
			this.saveFileDialogResults.Title = "Choose an XML results file to save";
			// 
			// saveFileDialogExport
			// 
			this.saveFileDialogExport.DefaultExt = "*.bin";
			this.saveFileDialogExport.Filter = "Binary Results (*.bin)|*.bin|All Files|*.*";
			// 
			// integerTrackbarControlThreadsCount
			// 
			this.integerTrackbarControlThreadsCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.integerTrackbarControlThreadsCount.Location = new System.Drawing.Point(253, 800);
			this.integerTrackbarControlThreadsCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlThreadsCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlThreadsCount.Name = "integerTrackbarControlThreadsCount";
			this.integerTrackbarControlThreadsCount.RangeMax = 16;
			this.integerTrackbarControlThreadsCount.RangeMin = 1;
			this.integerTrackbarControlThreadsCount.Size = new System.Drawing.Size(130, 20);
			this.integerTrackbarControlThreadsCount.TabIndex = 3;
			this.integerTrackbarControlThreadsCount.Value = 1;
			this.integerTrackbarControlThreadsCount.VisibleRangeMax = 4;
			this.integerTrackbarControlThreadsCount.VisibleRangeMin = 1;
			// 
			// label31
			// 
			this.label31.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label31.AutoSize = true;
			this.label31.Location = new System.Drawing.Point(171, 803);
			this.label31.Name = "label31";
			this.label31.Size = new System.Drawing.Size(77, 13);
			this.label31.TabIndex = 1;
			this.label31.Text = "Threads Count";
			// 
			// openFileDialogExport
			// 
			this.openFileDialogExport.CheckFileExists = false;
			this.openFileDialogExport.DefaultExt = "*.bin";
			this.openFileDialogExport.FileName = "result.xml";
			this.openFileDialogExport.Filter = "Binary Results (*.bin)|*.bin|All Files|*.*";
			this.openFileDialogExport.Title = "Choose a BIN results file to import";
			// 
			// completionArrayControl
			// 
			this.completionArrayControl.BackColor = System.Drawing.SystemColors.ControlLight;
			this.completionArrayControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.completionArrayControl.ContextMenuStrip = this.contextMenuStripSelection;
			this.completionArrayControl.GridColor = System.Drawing.Color.Black;
			this.completionArrayControl.Location = new System.Drawing.Point(12, 381);
			this.completionArrayControl.Name = "completionArrayControl";
			this.completionArrayControl.SelectedState = 1F;
			this.completionArrayControl.SelectedText = null;
			this.completionArrayControl.SelectedX = 0;
			this.completionArrayControl.SelectedY = 0;
			this.completionArrayControl.SelectedZ = 0;
			this.completionArrayControl.Size = new System.Drawing.Size(616, 263);
			this.completionArrayControl.TabIndex = 3;
			this.completionArrayControl.SelectionChanged += new TestMSBSDF.CompletionArrayControl.SelectionChangedEventHandler(this.completionArrayControl_SelectionChanged);
			this.completionArrayControl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.completionArrayControl_MouseDoubleClick);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(198, 6);
			// 
			// exportTotalReflectanceToolStripMenuItem
			// 
			this.exportTotalReflectanceToolStripMenuItem.Name = "exportTotalReflectanceToolStripMenuItem";
			this.exportTotalReflectanceToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			this.exportTotalReflectanceToolStripMenuItem.Text = "Export Total Reflectance";
			this.exportTotalReflectanceToolStripMenuItem.Click += new System.EventHandler(this.exportTotalReflectanceToolStripMenuItem_Click);
			// 
			// AutomationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1057, 840);
			this.Controls.Add(this.groupBoxSimulationParameters);
			this.Controls.Add(this.completionArrayControl);
			this.Controls.Add(this.integerTrackbarControlThreadsCount);
			this.Controls.Add(this.buttonClearResults);
			this.Controls.Add(this.buttonCompute);
			this.Controls.Add(this.label31);
			this.Controls.Add(this.panelParameters);
			this.Controls.Add(this.menuStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "AutomationForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Automation Form";
			this.groupBoxSimulationParameters.ResumeLayout(false);
			this.groupBoxSimulationParameters.PerformLayout();
			this.panel6.ResumeLayout(false);
			this.panel6.PerformLayout();
			this.panel5.ResumeLayout(false);
			this.panel5.PerformLayout();
			this.panelIncidentAngle.ResumeLayout(false);
			this.panelIncidentAngle.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panelParameters.ResumeLayout(false);
			this.panelParameters.PerformLayout();
			this.groupBoxLobeFitterConfig.ResumeLayout(false);
			this.groupBoxLobeFitterConfig.PerformLayout();
			this.groupBoxAnalyticalLobeModel.ResumeLayout(false);
			this.groupBoxCustomInitialGuesses.ResumeLayout(false);
			this.panel9.ResumeLayout(false);
			this.panel9.PerformLayout();
			this.panel8.ResumeLayout(false);
			this.panel8.PerformLayout();
			this.panel7.ResumeLayout(false);
			this.panel7.PerformLayout();
			this.panel4.ResumeLayout(false);
			this.panel4.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.contextMenuStripSelection.ResumeLayout(false);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBoxSimulationParameters;
		private System.Windows.Forms.Panel panelParameters;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton radioButtonSurfaceTypeConductor;
		private System.Windows.Forms.RadioButton radioButtonSurfaceTypeDielectric;
		private System.Windows.Forms.RadioButton radioButtonSurfaceTypeDiffuse;
		private System.Windows.Forms.Panel panelIncidentAngle;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private UIUtility.FloatTrackbarControl floatTrackbarControlParam0_Min;
		private UIUtility.FloatTrackbarControl floatTrackbarControlParam0_Max;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlParam0_Steps;
		private System.Windows.Forms.CheckBox checkBoxParam0_InclusiveEnd;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.GroupBox groupBoxAnalyticalLobeModel;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.RadioButton radioButtonLobe_GGX;
		private System.Windows.Forms.RadioButton radioButtonLobe_Beckmann;
		private System.Windows.Forms.RadioButton radioButtonLobe_ModifiedPhong;
		private System.Windows.Forms.GroupBox groupBoxCustomInitialGuesses;
		private UIUtility.FloatTrackbarControl floatTrackbarControlInit_Scale;
		private UIUtility.FloatTrackbarControl floatTrackbarControlInit_CustomFlatten;
		private System.Windows.Forms.RadioButton radioButtonInitRoughness_UseSurface;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.RadioButton radioButtonInitRoughness_Custom;
		private UIUtility.FloatTrackbarControl floatTrackbarControlInit_CustomRoughness;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.RadioButton radioButtonInitDirection_TowardCoM;
		private System.Windows.Forms.RadioButton radioButtonInitDirection_TowardReflected;
		private UIUtility.FloatTrackbarControl floatTrackbarControlInit_CustomMaskingImportance;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.CheckBox checkBoxParam1_InclusiveEnd;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlParam1_Steps;
		private UIUtility.FloatTrackbarControl floatTrackbarControlParam1_Max;
		private System.Windows.Forms.Label label6;
		private UIUtility.FloatTrackbarControl floatTrackbarControlParam1_Min;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Panel panel6;
		private System.Windows.Forms.CheckBox checkBoxParam2_InclusiveEnd;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlParam2_Steps;
		private UIUtility.FloatTrackbarControl floatTrackbarControlParam2_Max;
		private System.Windows.Forms.Label label15;
		private UIUtility.FloatTrackbarControl floatTrackbarControlParam2_Min;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label labelParm2;
		private System.Windows.Forms.CheckBox checkBoxParam0_InclusiveStart;
		private System.Windows.Forms.CheckBox checkBoxParam2_InclusiveStart;
		private System.Windows.Forms.CheckBox checkBoxParam1_InclusiveStart;
		private System.Windows.Forms.GroupBox groupBoxLobeFitterConfig;
		private System.Windows.Forms.Label label18;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlMaxIterations;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Label label20;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlScatteringOrder_Min;
		private System.Windows.Forms.Label label21;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlScatteringOrder_Max;
		private System.Windows.Forms.Label label22;
		private UIUtility.FloatTrackbarControl floatTrackbarControlGoalTolerance;
		private System.Windows.Forms.Label label23;
		private UIUtility.FloatTrackbarControl floatTrackbarControlGradientTolerance;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlRetries;
		private System.Windows.Forms.Label label24;
		private System.Windows.Forms.Label label25;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem resultsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem recentToolStripMenuItem;
		private UIUtility.FloatTrackbarControl floatTrackbarControlFitOversize;
		private System.Windows.Forms.Label label26;
		private CompletionArrayControl completionArrayControl;
		private System.Windows.Forms.RadioButton radioButtonInitDirection_NoChange;
		private System.Windows.Forms.Panel panel7;
		private System.Windows.Forms.Label label27;
		private System.Windows.Forms.RadioButton radioButtonInitScale_CoMFactor;
		private System.Windows.Forms.RadioButton radioButtonInitScale_NoChange;
		private System.Windows.Forms.CheckBox checkBoxInitScale_Inherit;
		private System.Windows.Forms.CheckBox checkBoxInitDirection_Inherit;
		private System.Windows.Forms.Panel panel8;
		private System.Windows.Forms.Label label28;
		private System.Windows.Forms.RadioButton radioButtonInitFlatten_Custom;
		private System.Windows.Forms.CheckBox checkBoxInitFlatten_Inherit;
		private System.Windows.Forms.CheckBox checkBoxInitRoughness_Inherit;
		private System.Windows.Forms.Panel panel9;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.RadioButton radioButtonInitMasking_Custom;
		private System.Windows.Forms.CheckBox checkBoxInitMasking_Inherit;
		private System.Windows.Forms.Button buttonCompute;
		private System.Windows.Forms.Button buttonClearResults;
		private System.Windows.Forms.OpenFileDialog openFileDialogResults;
		private System.Windows.Forms.SaveFileDialog saveFileDialogResults;
		private System.Windows.Forms.Label label11;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlRayCastingIterations;
		private System.Windows.Forms.Label labelTotalRaysCount;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
		private System.Windows.Forms.RadioButton radioButtonInitMasking_NoChange;
		private System.Windows.Forms.RadioButton radioButtonInitFlatten_NoChange;
		private System.Windows.Forms.RadioButton radioButtonInitRoughness_NoChange;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlViewScatteringOrder;
		private System.Windows.Forms.Label label29;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlViewAlbedoSlice;
		private System.Windows.Forms.Label label30;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripSelection;
		private System.Windows.Forms.ToolStripMenuItem computeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem startFromHereToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
		private UIUtility.LogTextBox textBoxLog;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
		private System.Windows.Forms.SaveFileDialog saveFileDialogExport;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlThreadsCount;
		private System.Windows.Forms.Label label31;
		private System.Windows.Forms.ToolStripMenuItem clearSliceFromHereToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem clearColumnToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem clearRowToolStripMenuItem;
		private System.Windows.Forms.CheckBox checkBoxInitMasking_InheritLeft;
		private System.Windows.Forms.CheckBox checkBoxInitFlatten_InheritLeft;
		private System.Windows.Forms.CheckBox checkBoxInitScale_InheritLeft;
		private System.Windows.Forms.CheckBox checkBoxInitDirection_InheritLeft;
		private System.Windows.Forms.CheckBox checkBoxInitRoughness_InheritLeft;
		private System.Windows.Forms.RadioButton radioButtonInitScale_Fixed;
		private UIUtility.FloatTrackbarControl floatTrackbarControlInit_FixedScale;
		private System.Windows.Forms.RadioButton radioButtonInitFlatten_Fixed;
		private UIUtility.FloatTrackbarControl floatTrackbarControlInit_FixedFlatten;
		private System.Windows.Forms.RadioButton radioButtonInitRoughness_Fixed;
		private UIUtility.FloatTrackbarControl floatTrackbarControlInit_FixedRoughness;
		private System.Windows.Forms.RadioButton radioButtonInitMasking_Fixed;
		private UIUtility.FloatTrackbarControl floatTrackbarControlInit_FixedMasking;
		private System.Windows.Forms.RadioButton radioButtonInitDirection_Fixed;
		private UIUtility.FloatTrackbarControl floatTrackbarControlInit_FixedDirection;
		private System.Windows.Forms.RadioButton radioButtonInitFlatten_Analytical;
		private System.Windows.Forms.RadioButton radioButtonInitScale_Analytical;
		private System.Windows.Forms.RadioButton radioButtonInitRoughness_Analytical;
		private System.Windows.Forms.RadioButton radioButtonLobe_ModifiedPhongAniso;
		private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
		private System.Windows.Forms.OpenFileDialog openFileDialogExport;
		private System.Windows.Forms.CheckBox checkBoxSkipSimulation;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem exportTotalReflectanceToolStripMenuItem;
	}
}