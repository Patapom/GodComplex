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
			this.timer = new System.Windows.Forms.Timer(this.components);
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
			this.panelOutput = new TestFilmicCurve.PanelOutput3D(this.components);
			this.outputPanelHammersley1 = new TestFilmicCurve.OutputPanelHammersley(this.components);
			this.panelGraph = new TestFilmicCurve.OutputPanel(this.components);
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(973, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(18, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "W";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(973, 92);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(14, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "A";
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(973, 118);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(14, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "B";
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(973, 144);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(14, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "C";
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(973, 170);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(15, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "D";
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(973, 196);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(14, 13);
			this.label6.TabIndex = 2;
			this.label6.Text = "E";
			// 
			// label7
			// 
			this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(973, 222);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(13, 13);
			this.label7.TabIndex = 2;
			this.label7.Text = "F";
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = 10;
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(976, 794);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 5;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// label8
			// 
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(1195, 252);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(44, 13);
			this.label8.TabIndex = 2;
			this.label8.Text = "Scale X";
			// 
			// label9
			// 
			this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(1195, 279);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(44, 13);
			this.label9.TabIndex = 2;
			this.label9.Text = "Scale Y";
			// 
			// label10
			// 
			this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(976, 590);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(21, 13);
			this.label10.TabIndex = 2;
			this.label10.Text = "EV";
			// 
			// checkBoxEnable
			// 
			this.checkBoxEnable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxEnable.AutoSize = true;
			this.checkBoxEnable.Checked = true;
			this.checkBoxEnable.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnable.Location = new System.Drawing.Point(979, 612);
			this.checkBoxEnable.Name = "checkBoxEnable";
			this.checkBoxEnable.Size = new System.Drawing.Size(131, 17);
			this.checkBoxEnable.TabIndex = 6;
			this.checkBoxEnable.Text = "Enable Tone Mapping";
			this.checkBoxEnable.UseVisualStyleBackColor = true;
			// 
			// buttonReset
			// 
			this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReset.Location = new System.Drawing.Point(976, 493);
			this.buttonReset.Name = "buttonReset";
			this.buttonReset.Size = new System.Drawing.Size(75, 23);
			this.buttonReset.TabIndex = 5;
			this.buttonReset.Text = "Reset";
			this.buttonReset.UseVisualStyleBackColor = true;
			this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
			// 
			// label11
			// 
			this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(976, 675);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(33, 13);
			this.label11.TabIndex = 2;
			this.label11.Text = "Luma";
			// 
			// checkBoxDebugLuminanceLevel
			// 
			this.checkBoxDebugLuminanceLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxDebugLuminanceLevel.AutoSize = true;
			this.checkBoxDebugLuminanceLevel.Location = new System.Drawing.Point(979, 648);
			this.checkBoxDebugLuminanceLevel.Name = "checkBoxDebugLuminanceLevel";
			this.checkBoxDebugLuminanceLevel.Size = new System.Drawing.Size(142, 17);
			this.checkBoxDebugLuminanceLevel.TabIndex = 6;
			this.checkBoxDebugLuminanceLevel.Text = "Debug Luminance Level";
			this.checkBoxDebugLuminanceLevel.UseVisualStyleBackColor = true;
			this.checkBoxDebugLuminanceLevel.CheckedChanged += new System.EventHandler(this.checkBoxDebugLuminanceLevel_CheckedChanged);
			// 
			// floatTrackbarControlWhitePoint
			// 
			this.floatTrackbarControlWhitePoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlWhitePoint.Location = new System.Drawing.Point(997, 12);
			this.floatTrackbarControlWhitePoint.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlWhitePoint.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlWhitePoint.Name = "floatTrackbarControlWhitePoint";
			this.floatTrackbarControlWhitePoint.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlWhitePoint.TabIndex = 1;
			this.floatTrackbarControlWhitePoint.Value = 8F;
			this.floatTrackbarControlWhitePoint.VisibleRangeMax = 20F;
			this.floatTrackbarControlWhitePoint.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlWhitePoint_ValueChanged);
			// 
			// floatTrackbarControlF
			// 
			this.floatTrackbarControlF.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlF.Location = new System.Drawing.Point(997, 219);
			this.floatTrackbarControlF.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlF.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlF.Name = "floatTrackbarControlF";
			this.floatTrackbarControlF.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlF.TabIndex = 1;
			this.floatTrackbarControlF.Value = 0.3F;
			this.floatTrackbarControlF.VisibleRangeMax = 1F;
			this.floatTrackbarControlF.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlF_ValueChanged);
			// 
			// floatTrackbarControlE
			// 
			this.floatTrackbarControlE.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlE.Location = new System.Drawing.Point(997, 193);
			this.floatTrackbarControlE.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlE.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlE.Name = "floatTrackbarControlE";
			this.floatTrackbarControlE.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlE.TabIndex = 1;
			this.floatTrackbarControlE.Value = 0.02F;
			this.floatTrackbarControlE.VisibleRangeMax = 1F;
			this.floatTrackbarControlE.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlE_ValueChanged);
			// 
			// floatTrackbarControlD
			// 
			this.floatTrackbarControlD.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlD.Location = new System.Drawing.Point(997, 167);
			this.floatTrackbarControlD.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlD.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlD.Name = "floatTrackbarControlD";
			this.floatTrackbarControlD.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlD.TabIndex = 1;
			this.floatTrackbarControlD.Value = 0.2F;
			this.floatTrackbarControlD.VisibleRangeMax = 1F;
			this.floatTrackbarControlD.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlD_ValueChanged);
			// 
			// floatTrackbarControlC
			// 
			this.floatTrackbarControlC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlC.Location = new System.Drawing.Point(997, 141);
			this.floatTrackbarControlC.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlC.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlC.Name = "floatTrackbarControlC";
			this.floatTrackbarControlC.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlC.TabIndex = 1;
			this.floatTrackbarControlC.Value = 0.1F;
			this.floatTrackbarControlC.VisibleRangeMax = 1F;
			this.floatTrackbarControlC.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlC_ValueChanged);
			// 
			// floatTrackbarControlB
			// 
			this.floatTrackbarControlB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlB.Location = new System.Drawing.Point(997, 115);
			this.floatTrackbarControlB.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlB.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlB.Name = "floatTrackbarControlB";
			this.floatTrackbarControlB.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlB.TabIndex = 1;
			this.floatTrackbarControlB.Value = 0.5F;
			this.floatTrackbarControlB.VisibleRangeMax = 1F;
			this.floatTrackbarControlB.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlB_ValueChanged);
			// 
			// floatTrackbarControlScaleY
			// 
			this.floatTrackbarControlScaleY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlScaleY.Location = new System.Drawing.Point(1245, 274);
			this.floatTrackbarControlScaleY.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlScaleY.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlScaleY.Name = "floatTrackbarControlScaleY";
			this.floatTrackbarControlScaleY.RangeMax = 1F;
			this.floatTrackbarControlScaleY.RangeMin = 0.0001F;
			this.floatTrackbarControlScaleY.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlScaleY.TabIndex = 1;
			this.floatTrackbarControlScaleY.Value = 1F;
			this.floatTrackbarControlScaleY.VisibleRangeMax = 1F;
			this.floatTrackbarControlScaleY.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScaleY_ValueChanged);
			// 
			// floatTrackbarControlA
			// 
			this.floatTrackbarControlA.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlA.Location = new System.Drawing.Point(997, 89);
			this.floatTrackbarControlA.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlA.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlA.Name = "floatTrackbarControlA";
			this.floatTrackbarControlA.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlA.TabIndex = 1;
			this.floatTrackbarControlA.Value = 0.15F;
			this.floatTrackbarControlA.VisibleRangeMax = 1F;
			this.floatTrackbarControlA.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlA_ValueChanged);
			// 
			// floatTrackbarControlDebugLuminanceLevel
			// 
			this.floatTrackbarControlDebugLuminanceLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlDebugLuminanceLevel.Location = new System.Drawing.Point(1015, 671);
			this.floatTrackbarControlDebugLuminanceLevel.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDebugLuminanceLevel.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDebugLuminanceLevel.Name = "floatTrackbarControlDebugLuminanceLevel";
			this.floatTrackbarControlDebugLuminanceLevel.RangeMin = 0.0001F;
			this.floatTrackbarControlDebugLuminanceLevel.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDebugLuminanceLevel.TabIndex = 1;
			this.floatTrackbarControlDebugLuminanceLevel.Value = 1F;
			this.floatTrackbarControlDebugLuminanceLevel.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlDebugLuminanceLevel_ValueChanged);
			// 
			// floatTrackbarControlExposure
			// 
			this.floatTrackbarControlExposure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlExposure.Location = new System.Drawing.Point(1015, 586);
			this.floatTrackbarControlExposure.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlExposure.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlExposure.Name = "floatTrackbarControlExposure";
			this.floatTrackbarControlExposure.RangeMax = 20F;
			this.floatTrackbarControlExposure.RangeMin = -20F;
			this.floatTrackbarControlExposure.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlExposure.TabIndex = 1;
			this.floatTrackbarControlExposure.Value = 0F;
			this.floatTrackbarControlExposure.VisibleRangeMax = 6F;
			this.floatTrackbarControlExposure.VisibleRangeMin = -6F;
			// 
			// floatTrackbarControlScaleX
			// 
			this.floatTrackbarControlScaleX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlScaleX.Location = new System.Drawing.Point(1245, 248);
			this.floatTrackbarControlScaleX.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlScaleX.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlScaleX.Name = "floatTrackbarControlScaleX";
			this.floatTrackbarControlScaleX.RangeMin = 1.0001F;
			this.floatTrackbarControlScaleX.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlScaleX.TabIndex = 1;
			this.floatTrackbarControlScaleX.Value = 10F;
			this.floatTrackbarControlScaleX.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScaleX_ValueChanged);
			// 
			// checkBoxShowHistogram
			// 
			this.checkBoxShowHistogram.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxShowHistogram.AutoSize = true;
			this.checkBoxShowHistogram.Location = new System.Drawing.Point(979, 697);
			this.checkBoxShowHistogram.Name = "checkBoxShowHistogram";
			this.checkBoxShowHistogram.Size = new System.Drawing.Size(138, 17);
			this.checkBoxShowHistogram.TabIndex = 6;
			this.checkBoxShowHistogram.Text = "Show Debug Histogram";
			this.checkBoxShowHistogram.UseVisualStyleBackColor = true;
			this.checkBoxShowHistogram.CheckedChanged += new System.EventHandler(this.checkBoxDebugLuminanceLevel_CheckedChanged);
			// 
			// panelOutput
			// 
			this.panelOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(955, 805);
			this.panelOutput.TabIndex = 4;
			// 
			// outputPanelHammersley1
			// 
			this.outputPanelHammersley1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.outputPanelHammersley1.Location = new System.Drawing.Point(1245, 544);
			this.outputPanelHammersley1.Name = "outputPanelHammersley1";
			this.outputPanelHammersley1.Size = new System.Drawing.Size(275, 273);
			this.outputPanelHammersley1.TabIndex = 3;
			// 
			// panelGraph
			// 
			this.panelGraph.A = 0.15F;
			this.panelGraph.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.panelGraph.B = 0.5F;
			this.panelGraph.C = 0.1F;
			this.panelGraph.D = 0.2F;
			this.panelGraph.DebugLuminance = 1F;
			this.panelGraph.E = 0.02F;
			this.panelGraph.F = 0.3F;
			this.panelGraph.Location = new System.Drawing.Point(1203, 12);
			this.panelGraph.Name = "panelGraph";
			this.panelGraph.ScaleX = 1F;
			this.panelGraph.ScaleY = 1F;
			this.panelGraph.ShowDebugLuminance = false;
			this.panelGraph.Size = new System.Drawing.Size(317, 223);
			this.panelGraph.TabIndex = 0;
			this.panelGraph.WhitePoint = 10F;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1532, 829);
			this.Controls.Add(this.checkBoxShowHistogram);
			this.Controls.Add(this.checkBoxDebugLuminanceLevel);
			this.Controls.Add(this.checkBoxEnable);
			this.Controls.Add(this.buttonReset);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput);
			this.Controls.Add(this.outputPanelHammersley1);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlWhitePoint);
			this.Controls.Add(this.floatTrackbarControlF);
			this.Controls.Add(this.floatTrackbarControlE);
			this.Controls.Add(this.floatTrackbarControlD);
			this.Controls.Add(this.floatTrackbarControlC);
			this.Controls.Add(this.floatTrackbarControlB);
			this.Controls.Add(this.floatTrackbarControlScaleY);
			this.Controls.Add(this.floatTrackbarControlA);
			this.Controls.Add(this.floatTrackbarControlDebugLuminanceLevel);
			this.Controls.Add(this.floatTrackbarControlExposure);
			this.Controls.Add(this.floatTrackbarControlScaleX);
			this.Controls.Add(this.panelGraph);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Film Tone Mapping Test";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OutputPanel panelGraph;
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
	}
}

