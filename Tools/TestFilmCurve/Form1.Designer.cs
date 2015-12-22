namespace TestFilmicCurve
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.timer = new System.Windows.Forms.Timer( this.components );
			this.buttonReload = new System.Windows.Forms.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.checkBoxEnable = new System.Windows.Forms.CheckBox();
			this.buttonReset = new System.Windows.Forms.Button();
			this.label11 = new System.Windows.Forms.Label();
			this.checkBoxDebugLuminanceLevel = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlWhitePoint = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlF = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlE = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlD = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlC = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlB = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlScaleY = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlA = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlDebugLuminanceLevel = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlExposure = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlScaleX = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.checkBoxShowHistogram = new System.Windows.Forms.CheckBox();
			this.tabControlToneMappingTypes = new System.Windows.Forms.TabControl();
			this.tabPageCustom = new System.Windows.Forms.TabPage();
			this.floatTrackbarControlIG_JunctionPoint = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlIG_WhitePoint = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlIG_ShoulderStrength = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlIG_ToeStrength = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label15 = new System.Windows.Forms.Label();
			this.floatTrackbarControlIG_BlackPoint = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label16 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.tabPageToneMappingHabble = new System.Windows.Forms.TabPage();
			this.floatTrackbarControlTest = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.panelOutput = new TestFilmicCurve.PanelOutput3D( this.components );
			this.outputPanelHammersley1 = new TestFilmicCurve.OutputPanelHammersley( this.components );
			this.outputPanelFilmic_Insomniac = new TestFilmicCurve.OutputPanelFilmic_Insomniac( this.components );
			this.panelGraph_Hable = new TestFilmicCurve.OutputPanelFilmic_Hable( this.components );
			this.tabControlToneMappingTypes.SuspendLayout();
			this.tabPageCustom.SuspendLayout();
			this.tabPageToneMappingHabble.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point( 10, 11 );
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size( 62, 13 );
			this.label1.TabIndex = 2;
			this.label1.Text = "White Point";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point( 10, 48 );
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size( 92, 13 );
			this.label2.TabIndex = 2;
			this.label2.Text = "Shoulder Strength";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point( 10, 72 );
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size( 79, 13 );
			this.label3.TabIndex = 2;
			this.label3.Text = "Linear Strength";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point( 10, 98 );
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size( 66, 13 );
			this.label4.TabIndex = 2;
			this.label4.Text = "Linear Angle";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point( 10, 124 );
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size( 69, 13 );
			this.label5.TabIndex = 2;
			this.label5.Text = "Toe Strength";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point( 10, 150 );
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size( 78, 13 );
			this.label6.TabIndex = 2;
			this.label6.Text = "Toe Numerator";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point( 10, 176 );
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size( 89, 13 );
			this.label7.TabIndex = 2;
			this.label7.Text = "Toe Denominator";
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = 10;
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point( 1128, 794 );
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size( 75, 23 );
			this.buttonReload.TabIndex = 5;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler( this.buttonReload_Click );
			// 
			// label8
			// 
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point( 1270, 269 );
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size( 44, 13 );
			this.label8.TabIndex = 2;
			this.label8.Text = "Scale X";
			// 
			// label9
			// 
			this.label9.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point( 1270, 296 );
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size( 44, 13 );
			this.label9.TabIndex = 2;
			this.label9.Text = "Scale Y";
			// 
			// label10
			// 
			this.label10.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point( 1131, 591 );
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size( 21, 13 );
			this.label10.TabIndex = 2;
			this.label10.Text = "EV";
			// 
			// checkBoxEnable
			// 
			this.checkBoxEnable.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxEnable.AutoSize = true;
			this.checkBoxEnable.Checked = true;
			this.checkBoxEnable.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnable.Location = new System.Drawing.Point( 1128, 304 );
			this.checkBoxEnable.Name = "checkBoxEnable";
			this.checkBoxEnable.Size = new System.Drawing.Size( 131, 17 );
			this.checkBoxEnable.TabIndex = 6;
			this.checkBoxEnable.Text = "Enable Tone Mapping";
			this.checkBoxEnable.UseVisualStyleBackColor = true;
			// 
			// buttonReset
			// 
			this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReset.Location = new System.Drawing.Point( 1131, 262 );
			this.buttonReset.Name = "buttonReset";
			this.buttonReset.Size = new System.Drawing.Size( 75, 23 );
			this.buttonReset.TabIndex = 5;
			this.buttonReset.Text = "Reset";
			this.buttonReset.UseVisualStyleBackColor = true;
			this.buttonReset.Click += new System.EventHandler( this.buttonReset_Click );
			// 
			// label11
			// 
			this.label11.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point( 1131, 683 );
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size( 33, 13 );
			this.label11.TabIndex = 2;
			this.label11.Text = "Luma";
			// 
			// checkBoxDebugLuminanceLevel
			// 
			this.checkBoxDebugLuminanceLevel.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxDebugLuminanceLevel.AutoSize = true;
			this.checkBoxDebugLuminanceLevel.Location = new System.Drawing.Point( 1134, 656 );
			this.checkBoxDebugLuminanceLevel.Name = "checkBoxDebugLuminanceLevel";
			this.checkBoxDebugLuminanceLevel.Size = new System.Drawing.Size( 142, 17 );
			this.checkBoxDebugLuminanceLevel.TabIndex = 6;
			this.checkBoxDebugLuminanceLevel.Text = "Debug Luminance Level";
			this.checkBoxDebugLuminanceLevel.UseVisualStyleBackColor = true;
			this.checkBoxDebugLuminanceLevel.CheckedChanged += new System.EventHandler( this.checkBoxDebugLuminanceLevel_CheckedChanged );
			// 
			// floatTrackbarControlWhitePoint
			// 
			this.floatTrackbarControlWhitePoint.Location = new System.Drawing.Point( 103, 6 );
			this.floatTrackbarControlWhitePoint.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlWhitePoint.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlWhitePoint.Name = "floatTrackbarControlWhitePoint";
			this.floatTrackbarControlWhitePoint.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlWhitePoint.TabIndex = 1;
			this.floatTrackbarControlWhitePoint.Value = 10F;
			this.floatTrackbarControlWhitePoint.VisibleRangeMax = 20F;
			this.floatTrackbarControlWhitePoint.VisibleRangeMin = 1F;
			this.floatTrackbarControlWhitePoint.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlWhitePoint_ValueChanged );
			// 
			// floatTrackbarControlF
			// 
			this.floatTrackbarControlF.Location = new System.Drawing.Point( 103, 174 );
			this.floatTrackbarControlF.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlF.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlF.Name = "floatTrackbarControlF";
			this.floatTrackbarControlF.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlF.TabIndex = 1;
			this.floatTrackbarControlF.Value = 0.3F;
			this.floatTrackbarControlF.VisibleRangeMax = 1F;
			this.floatTrackbarControlF.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlF_ValueChanged );
			// 
			// floatTrackbarControlE
			// 
			this.floatTrackbarControlE.Location = new System.Drawing.Point( 103, 148 );
			this.floatTrackbarControlE.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlE.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlE.Name = "floatTrackbarControlE";
			this.floatTrackbarControlE.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlE.TabIndex = 1;
			this.floatTrackbarControlE.Value = 0.02F;
			this.floatTrackbarControlE.VisibleRangeMax = 1F;
			this.floatTrackbarControlE.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlE_ValueChanged );
			// 
			// floatTrackbarControlD
			// 
			this.floatTrackbarControlD.Location = new System.Drawing.Point( 103, 122 );
			this.floatTrackbarControlD.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlD.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlD.Name = "floatTrackbarControlD";
			this.floatTrackbarControlD.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlD.TabIndex = 1;
			this.floatTrackbarControlD.Value = 0.2F;
			this.floatTrackbarControlD.VisibleRangeMax = 1F;
			this.floatTrackbarControlD.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlD_ValueChanged );
			// 
			// floatTrackbarControlC
			// 
			this.floatTrackbarControlC.Location = new System.Drawing.Point( 103, 96 );
			this.floatTrackbarControlC.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlC.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlC.Name = "floatTrackbarControlC";
			this.floatTrackbarControlC.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlC.TabIndex = 1;
			this.floatTrackbarControlC.Value = 0.1F;
			this.floatTrackbarControlC.VisibleRangeMax = 1F;
			this.floatTrackbarControlC.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlC_ValueChanged );
			// 
			// floatTrackbarControlB
			// 
			this.floatTrackbarControlB.Location = new System.Drawing.Point( 103, 70 );
			this.floatTrackbarControlB.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlB.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlB.Name = "floatTrackbarControlB";
			this.floatTrackbarControlB.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlB.TabIndex = 1;
			this.floatTrackbarControlB.Value = 0.5F;
			this.floatTrackbarControlB.VisibleRangeMax = 1F;
			this.floatTrackbarControlB.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlB_ValueChanged );
			// 
			// floatTrackbarControlScaleY
			// 
			this.floatTrackbarControlScaleY.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlScaleY.Location = new System.Drawing.Point( 1320, 291 );
			this.floatTrackbarControlScaleY.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlScaleY.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlScaleY.Name = "floatTrackbarControlScaleY";
			this.floatTrackbarControlScaleY.RangeMax = 100F;
			this.floatTrackbarControlScaleY.RangeMin = 0.0001F;
			this.floatTrackbarControlScaleY.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlScaleY.TabIndex = 1;
			this.floatTrackbarControlScaleY.Value = 1F;
			this.floatTrackbarControlScaleY.VisibleRangeMax = 2F;
			this.floatTrackbarControlScaleY.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlScaleY_ValueChanged );
			// 
			// floatTrackbarControlA
			// 
			this.floatTrackbarControlA.Location = new System.Drawing.Point( 103, 44 );
			this.floatTrackbarControlA.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlA.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlA.Name = "floatTrackbarControlA";
			this.floatTrackbarControlA.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlA.TabIndex = 1;
			this.floatTrackbarControlA.Value = 0.15F;
			this.floatTrackbarControlA.VisibleRangeMax = 1F;
			this.floatTrackbarControlA.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlA_ValueChanged );
			// 
			// floatTrackbarControlDebugLuminanceLevel
			// 
			this.floatTrackbarControlDebugLuminanceLevel.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlDebugLuminanceLevel.Location = new System.Drawing.Point( 1170, 679 );
			this.floatTrackbarControlDebugLuminanceLevel.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlDebugLuminanceLevel.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlDebugLuminanceLevel.Name = "floatTrackbarControlDebugLuminanceLevel";
			this.floatTrackbarControlDebugLuminanceLevel.RangeMin = 0.0001F;
			this.floatTrackbarControlDebugLuminanceLevel.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlDebugLuminanceLevel.TabIndex = 1;
			this.floatTrackbarControlDebugLuminanceLevel.Value = 1F;
			this.floatTrackbarControlDebugLuminanceLevel.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlDebugLuminanceLevel_ValueChanged );
			// 
			// floatTrackbarControlExposure
			// 
			this.floatTrackbarControlExposure.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlExposure.Location = new System.Drawing.Point( 1170, 587 );
			this.floatTrackbarControlExposure.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlExposure.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlExposure.Name = "floatTrackbarControlExposure";
			this.floatTrackbarControlExposure.RangeMax = 20F;
			this.floatTrackbarControlExposure.RangeMin = -20F;
			this.floatTrackbarControlExposure.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlExposure.TabIndex = 1;
			this.floatTrackbarControlExposure.Value = 0F;
			this.floatTrackbarControlExposure.VisibleRangeMax = 6F;
			this.floatTrackbarControlExposure.VisibleRangeMin = -6F;
			// 
			// floatTrackbarControlScaleX
			// 
			this.floatTrackbarControlScaleX.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlScaleX.Location = new System.Drawing.Point( 1320, 265 );
			this.floatTrackbarControlScaleX.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlScaleX.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlScaleX.Name = "floatTrackbarControlScaleX";
			this.floatTrackbarControlScaleX.RangeMin = 1.0001F;
			this.floatTrackbarControlScaleX.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlScaleX.TabIndex = 1;
			this.floatTrackbarControlScaleX.Value = 10F;
			this.floatTrackbarControlScaleX.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlScaleX_ValueChanged );
			// 
			// checkBoxShowHistogram
			// 
			this.checkBoxShowHistogram.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxShowHistogram.AutoSize = true;
			this.checkBoxShowHistogram.Checked = true;
			this.checkBoxShowHistogram.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowHistogram.Location = new System.Drawing.Point( 1134, 613 );
			this.checkBoxShowHistogram.Name = "checkBoxShowHistogram";
			this.checkBoxShowHistogram.Size = new System.Drawing.Size( 138, 17 );
			this.checkBoxShowHistogram.TabIndex = 6;
			this.checkBoxShowHistogram.Text = "Show Debug Histogram";
			this.checkBoxShowHistogram.UseVisualStyleBackColor = true;
			this.checkBoxShowHistogram.CheckedChanged += new System.EventHandler( this.checkBoxDebugLuminanceLevel_CheckedChanged );
			// 
			// tabControlToneMappingTypes
			// 
			this.tabControlToneMappingTypes.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControlToneMappingTypes.Controls.Add( this.tabPageCustom );
			this.tabControlToneMappingTypes.Controls.Add( this.tabPageToneMappingHabble );
			this.tabControlToneMappingTypes.Location = new System.Drawing.Point( 1131, 13 );
			this.tabControlToneMappingTypes.Name = "tabControlToneMappingTypes";
			this.tabControlToneMappingTypes.SelectedIndex = 0;
			this.tabControlToneMappingTypes.Size = new System.Drawing.Size( 334, 236 );
			this.tabControlToneMappingTypes.TabIndex = 7;
			this.tabControlToneMappingTypes.SelectedIndexChanged += new System.EventHandler( this.tabControlToneMappingTypes_SelectedIndexChanged );
			// 
			// tabPageCustom
			// 
			this.tabPageCustom.Controls.Add( this.floatTrackbarControlIG_JunctionPoint );
			this.tabPageCustom.Controls.Add( this.floatTrackbarControlIG_WhitePoint );
			this.tabPageCustom.Controls.Add( this.floatTrackbarControlTest );
			this.tabPageCustom.Controls.Add( this.floatTrackbarControlIG_ShoulderStrength );
			this.tabPageCustom.Controls.Add( this.floatTrackbarControlIG_ToeStrength );
			this.tabPageCustom.Controls.Add( this.label15 );
			this.tabPageCustom.Controls.Add( this.floatTrackbarControlIG_BlackPoint );
			this.tabPageCustom.Controls.Add( this.label16 );
			this.tabPageCustom.Controls.Add( this.label14 );
			this.tabPageCustom.Controls.Add( this.label13 );
			this.tabPageCustom.Controls.Add( this.label12 );
			this.tabPageCustom.Location = new System.Drawing.Point( 4, 22 );
			this.tabPageCustom.Name = "tabPageCustom";
			this.tabPageCustom.Padding = new System.Windows.Forms.Padding( 3 );
			this.tabPageCustom.Size = new System.Drawing.Size( 326, 210 );
			this.tabPageCustom.TabIndex = 0;
			this.tabPageCustom.Text = "Insomniac";
			this.tabPageCustom.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControlIG_JunctionPoint
			// 
			this.floatTrackbarControlIG_JunctionPoint.Location = new System.Drawing.Point( 99, 61 );
			this.floatTrackbarControlIG_JunctionPoint.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlIG_JunctionPoint.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlIG_JunctionPoint.Name = "floatTrackbarControlIG_JunctionPoint";
			this.floatTrackbarControlIG_JunctionPoint.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlIG_JunctionPoint.TabIndex = 3;
			this.floatTrackbarControlIG_JunctionPoint.Value = 0.2F;
			this.floatTrackbarControlIG_JunctionPoint.VisibleRangeMax = 1F;
			this.floatTrackbarControlIG_JunctionPoint.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlIG_JunctionPoint_ValueChanged );
			// 
			// floatTrackbarControlIG_WhitePoint
			// 
			this.floatTrackbarControlIG_WhitePoint.Location = new System.Drawing.Point( 99, 35 );
			this.floatTrackbarControlIG_WhitePoint.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlIG_WhitePoint.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlIG_WhitePoint.Name = "floatTrackbarControlIG_WhitePoint";
			this.floatTrackbarControlIG_WhitePoint.RangeMin = 0.001F;
			this.floatTrackbarControlIG_WhitePoint.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlIG_WhitePoint.TabIndex = 3;
			this.floatTrackbarControlIG_WhitePoint.Value = 10F;
			this.floatTrackbarControlIG_WhitePoint.VisibleRangeMin = 1F;
			this.floatTrackbarControlIG_WhitePoint.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlIG_WhitePoint_ValueChanged );
			// 
			// floatTrackbarControlIG_ShoulderStrength
			// 
			this.floatTrackbarControlIG_ShoulderStrength.Location = new System.Drawing.Point( 99, 143 );
			this.floatTrackbarControlIG_ShoulderStrength.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlIG_ShoulderStrength.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlIG_ShoulderStrength.Name = "floatTrackbarControlIG_ShoulderStrength";
			this.floatTrackbarControlIG_ShoulderStrength.RangeMax = 1F;
			this.floatTrackbarControlIG_ShoulderStrength.RangeMin = 0F;
			this.floatTrackbarControlIG_ShoulderStrength.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlIG_ShoulderStrength.TabIndex = 3;
			this.floatTrackbarControlIG_ShoulderStrength.Value = 0.8F;
			this.floatTrackbarControlIG_ShoulderStrength.VisibleRangeMax = 1F;
			this.floatTrackbarControlIG_ShoulderStrength.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlIG_ShoulderStrength_ValueChanged );
			// 
			// floatTrackbarControlIG_ToeStrength
			// 
			this.floatTrackbarControlIG_ToeStrength.Location = new System.Drawing.Point( 99, 117 );
			this.floatTrackbarControlIG_ToeStrength.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlIG_ToeStrength.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlIG_ToeStrength.Name = "floatTrackbarControlIG_ToeStrength";
			this.floatTrackbarControlIG_ToeStrength.RangeMax = 1F;
			this.floatTrackbarControlIG_ToeStrength.RangeMin = 0F;
			this.floatTrackbarControlIG_ToeStrength.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlIG_ToeStrength.TabIndex = 3;
			this.floatTrackbarControlIG_ToeStrength.Value = 0.25F;
			this.floatTrackbarControlIG_ToeStrength.VisibleRangeMax = 1F;
			this.floatTrackbarControlIG_ToeStrength.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlIG_ToeStrength_ValueChanged );
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point( 6, 147 );
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size( 92, 13 );
			this.label15.TabIndex = 4;
			this.label15.Text = "Shoulder Strength";
			// 
			// floatTrackbarControlIG_BlackPoint
			// 
			this.floatTrackbarControlIG_BlackPoint.Location = new System.Drawing.Point( 99, 9 );
			this.floatTrackbarControlIG_BlackPoint.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlIG_BlackPoint.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlIG_BlackPoint.Name = "floatTrackbarControlIG_BlackPoint";
			this.floatTrackbarControlIG_BlackPoint.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlIG_BlackPoint.TabIndex = 3;
			this.floatTrackbarControlIG_BlackPoint.Value = 0F;
			this.floatTrackbarControlIG_BlackPoint.VisibleRangeMax = 0.1F;
			this.floatTrackbarControlIG_BlackPoint.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlIG_BlackPoint_ValueChanged );
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point( 6, 66 );
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size( 74, 13 );
			this.label16.TabIndex = 4;
			this.label16.Text = "Junction Point";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point( 6, 121 );
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size( 69, 13 );
			this.label14.TabIndex = 4;
			this.label14.Text = "Toe Strength";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point( 6, 40 );
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size( 62, 13 );
			this.label13.TabIndex = 4;
			this.label13.Text = "White Point";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point( 6, 14 );
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size( 61, 13 );
			this.label12.TabIndex = 4;
			this.label12.Text = "Black Point";
			// 
			// tabPageToneMappingHabble
			// 
			this.tabPageToneMappingHabble.Controls.Add( this.floatTrackbarControlWhitePoint );
			this.tabPageToneMappingHabble.Controls.Add( this.label1 );
			this.tabPageToneMappingHabble.Controls.Add( this.floatTrackbarControlA );
			this.tabPageToneMappingHabble.Controls.Add( this.floatTrackbarControlB );
			this.tabPageToneMappingHabble.Controls.Add( this.floatTrackbarControlC );
			this.tabPageToneMappingHabble.Controls.Add( this.floatTrackbarControlD );
			this.tabPageToneMappingHabble.Controls.Add( this.floatTrackbarControlE );
			this.tabPageToneMappingHabble.Controls.Add( this.floatTrackbarControlF );
			this.tabPageToneMappingHabble.Controls.Add( this.label2 );
			this.tabPageToneMappingHabble.Controls.Add( this.label3 );
			this.tabPageToneMappingHabble.Controls.Add( this.label4 );
			this.tabPageToneMappingHabble.Controls.Add( this.label5 );
			this.tabPageToneMappingHabble.Controls.Add( this.label7 );
			this.tabPageToneMappingHabble.Controls.Add( this.label6 );
			this.tabPageToneMappingHabble.Location = new System.Drawing.Point( 4, 22 );
			this.tabPageToneMappingHabble.Name = "tabPageToneMappingHabble";
			this.tabPageToneMappingHabble.Padding = new System.Windows.Forms.Padding( 3 );
			this.tabPageToneMappingHabble.Size = new System.Drawing.Size( 326, 210 );
			this.tabPageToneMappingHabble.TabIndex = 1;
			this.tabPageToneMappingHabble.Text = "Hable";
			this.tabPageToneMappingHabble.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControlTest
			// 
			this.floatTrackbarControlTest.Location = new System.Drawing.Point( 99, 169 );
			this.floatTrackbarControlTest.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlTest.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlTest.Name = "floatTrackbarControlTest";
			this.floatTrackbarControlTest.RangeMin = 0F;
			this.floatTrackbarControlTest.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlTest.TabIndex = 3;
			this.floatTrackbarControlTest.Value = 1F;
			this.floatTrackbarControlTest.Visible = false;
			this.floatTrackbarControlTest.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlTest_ValueChanged );
			// 
			// panelOutput
			// 
			this.panelOutput.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panelOutput.Location = new System.Drawing.Point( 12, 12 );
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size( 1110, 805 );
			this.panelOutput.TabIndex = 4;
			// 
			// outputPanelHammersley1
			// 
			this.outputPanelHammersley1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.outputPanelHammersley1.Location = new System.Drawing.Point( 1387, 587 );
			this.outputPanelHammersley1.Name = "outputPanelHammersley1";
			this.outputPanelHammersley1.Size = new System.Drawing.Size( 106, 103 );
			this.outputPanelHammersley1.TabIndex = 3;
			// 
			// outputPanelFilmic_Insomniac
			// 
			this.outputPanelFilmic_Insomniac.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.outputPanelFilmic_Insomniac.BlackPoint = 0F;
			this.outputPanelFilmic_Insomniac.DebugLuminance = 1F;
			this.outputPanelFilmic_Insomniac.JunctionPoint = 0.5F;
			this.outputPanelFilmic_Insomniac.Location = new System.Drawing.Point( 1131, 325 );
			this.outputPanelFilmic_Insomniac.Name = "outputPanelFilmic_Insomniac";
			this.outputPanelFilmic_Insomniac.ScaleX = 1F;
			this.outputPanelFilmic_Insomniac.ScaleY = 1F;
			this.outputPanelFilmic_Insomniac.ShoulderStrength = 0F;
			this.outputPanelFilmic_Insomniac.ShowDebugLuminance = false;
			this.outputPanelFilmic_Insomniac.Size = new System.Drawing.Size( 389, 223 );
			this.outputPanelFilmic_Insomniac.TabIndex = 0;
			this.outputPanelFilmic_Insomniac.ToeStrength = 0F;
			this.outputPanelFilmic_Insomniac.WhitePoint = 10F;
			// 
			// panelGraph_Hable
			// 
			this.panelGraph_Hable.A = 0.15F;
			this.panelGraph_Hable.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.panelGraph_Hable.B = 0.5F;
			this.panelGraph_Hable.C = 0.1F;
			this.panelGraph_Hable.D = 0.2F;
			this.panelGraph_Hable.DebugLuminance = 1F;
			this.panelGraph_Hable.E = 0.02F;
			this.panelGraph_Hable.F = 0.3F;
			this.panelGraph_Hable.Location = new System.Drawing.Point( 1131, 327 );
			this.panelGraph_Hable.Name = "panelGraph_Hable";
			this.panelGraph_Hable.ScaleX = 1F;
			this.panelGraph_Hable.ScaleY = 1F;
			this.panelGraph_Hable.ShowDebugLuminance = false;
			this.panelGraph_Hable.Size = new System.Drawing.Size( 389, 223 );
			this.panelGraph_Hable.TabIndex = 0;
			this.panelGraph_Hable.Visible = false;
			this.panelGraph_Hable.WhitePoint = 10F;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 1532, 829 );
			this.Controls.Add( this.tabControlToneMappingTypes );
			this.Controls.Add( this.checkBoxShowHistogram );
			this.Controls.Add( this.checkBoxDebugLuminanceLevel );
			this.Controls.Add( this.checkBoxEnable );
			this.Controls.Add( this.buttonReset );
			this.Controls.Add( this.buttonReload );
			this.Controls.Add( this.panelOutput );
			this.Controls.Add( this.outputPanelHammersley1 );
			this.Controls.Add( this.label9 );
			this.Controls.Add( this.label11 );
			this.Controls.Add( this.label10 );
			this.Controls.Add( this.label8 );
			this.Controls.Add( this.floatTrackbarControlScaleY );
			this.Controls.Add( this.floatTrackbarControlDebugLuminanceLevel );
			this.Controls.Add( this.floatTrackbarControlExposure );
			this.Controls.Add( this.floatTrackbarControlScaleX );
			this.Controls.Add( this.outputPanelFilmic_Insomniac );
			this.Controls.Add( this.panelGraph_Hable );
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Film Tone Mapping Test";
			this.tabControlToneMappingTypes.ResumeLayout( false );
			this.tabPageCustom.ResumeLayout( false );
			this.tabPageCustom.PerformLayout();
			this.tabPageToneMappingHabble.ResumeLayout( false );
			this.tabPageToneMappingHabble.PerformLayout();
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private OutputPanelFilmic_Hable panelGraph_Hable;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlScaleX;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlScaleY;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlWhitePoint;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlA;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlB;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlC;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlD;
		private System.Windows.Forms.Label label5;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlE;
		private System.Windows.Forms.Label label6;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlF;
		private System.Windows.Forms.Label label7;
		private OutputPanelHammersley outputPanelHammersley1;
		private System.Windows.Forms.Timer timer;
		private PanelOutput3D panelOutput;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlExposure;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.CheckBox checkBoxEnable;
		private System.Windows.Forms.Button buttonReset;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDebugLuminanceLevel;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.CheckBox checkBoxDebugLuminanceLevel;
		private System.Windows.Forms.CheckBox checkBoxShowHistogram;
		private System.Windows.Forms.TabControl tabControlToneMappingTypes;
		private System.Windows.Forms.TabPage tabPageCustom;
		private System.Windows.Forms.TabPage tabPageToneMappingHabble;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlIG_WhitePoint;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlIG_BlackPoint;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label12;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlIG_ShoulderStrength;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlIG_ToeStrength;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label14;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlIG_JunctionPoint;
		private System.Windows.Forms.Label label16;
		private OutputPanelFilmic_Insomniac outputPanelFilmic_Insomniac;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlTest;
	}
}

