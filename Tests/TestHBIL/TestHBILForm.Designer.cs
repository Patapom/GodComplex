﻿namespace TestHBIL
{
	partial class TestHBILForm
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
				m_device.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestHBILForm));
			this.buttonReload = new System.Windows.Forms.Button();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.floatTrackbarControlEnvironmentIntensity = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlExposure = new UIUtility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.floatTrackbarControlAlbedo = new UIUtility.FloatTrackbarControl();
			this.label6 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.checkBoxAnimate = new System.Windows.Forms.CheckBox();
			this.checkBoxPause = new System.Windows.Forms.CheckBox();
			this.buttonClear = new System.Windows.Forms.Button();
			this.checkBoxEnableHBIL = new System.Windows.Forms.CheckBox();
			this.checkBoxEnableBentNormal = new System.Windows.Forms.CheckBox();
			this.checkBoxEnableConeVisibility = new System.Windows.Forms.CheckBox();
			this.checkBoxForceAlbedo = new System.Windows.Forms.CheckBox();
			this.textBoxInfo = new System.Windows.Forms.TextBox();
			this.checkBoxMonochrome = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlConeAngleBias = new UIUtility.FloatTrackbarControl();
			this.integerTrackbarControlDebugMip = new UIUtility.IntegerTrackbarControl();
			this.checkBoxAutoRotateCamera = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlCameraRotateSpeed = new UIUtility.FloatTrackbarControl();
			this.radioButtonPUSH = new System.Windows.Forms.RadioButton();
			this.radioButtonPULL = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlHarmonicPreferedDepth = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlBilateralDepthDeltaMax = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlBilateralDepthDeltaMin = new UIUtility.FloatTrackbarControl();
			this.panelOutput = new TestHBIL.PanelOutput(this.components);
			this.checkBoxFreezePrev2CurrentCamMatrix = new System.Windows.Forms.CheckBox();
			this.groupBoxPushPull = new System.Windows.Forms.GroupBox();
			this.groupBoxHBIL = new System.Windows.Forms.GroupBox();
			this.floatTrackbarControlBilateral3 = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlBilateral1 = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlBilateral2 = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlBilateral0 = new UIUtility.FloatTrackbarControl();
			this.label9 = new System.Windows.Forms.Label();
			this.floatTrackbarControlGatherSphereRadius_pixels = new UIUtility.FloatTrackbarControl();
			this.label10 = new System.Windows.Forms.Label();
			this.floatTrackbarControlGatherSphereRadius_meters = new UIUtility.FloatTrackbarControl();
			this.label8 = new System.Windows.Forms.Label();
			this.groupBoxLighting = new System.Windows.Forms.GroupBox();
			this.checkBoxEnableBentNormalDirect = new System.Windows.Forms.CheckBox();
			this.checkBoxEnableConeVisibilityDirect = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlSunIntensity = new UIUtility.FloatTrackbarControl();
			this.label11 = new System.Windows.Forms.Label();
			this.checkBoxEnableTAA = new System.Windows.Forms.CheckBox();
			this.checkBoxShowAO = new System.Windows.Forms.CheckBox();
			this.checkBoxShowIrradiance = new System.Windows.Forms.CheckBox();
			this.checkBoxEnablePushPull = new System.Windows.Forms.CheckBox();
			this.groupBoxPushPull.SuspendLayout();
			this.groupBoxHBIL.SuspendLayout();
			this.groupBoxLighting.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(1552, 757);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 2;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = 10;
			// 
			// floatTrackbarControlEnvironmentIntensity
			// 
			this.floatTrackbarControlEnvironmentIntensity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlEnvironmentIntensity.Location = new System.Drawing.Point(124, 149);
			this.floatTrackbarControlEnvironmentIntensity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlEnvironmentIntensity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlEnvironmentIntensity.Name = "floatTrackbarControlEnvironmentIntensity";
			this.floatTrackbarControlEnvironmentIntensity.RangeMax = 100F;
			this.floatTrackbarControlEnvironmentIntensity.RangeMin = 0F;
			this.floatTrackbarControlEnvironmentIntensity.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlEnvironmentIntensity.TabIndex = 1;
			this.floatTrackbarControlEnvironmentIntensity.Value = 1F;
			this.floatTrackbarControlEnvironmentIntensity.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlExposure
			// 
			this.floatTrackbarControlExposure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlExposure.Location = new System.Drawing.Point(1427, 729);
			this.floatTrackbarControlExposure.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlExposure.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlExposure.Name = "floatTrackbarControlExposure";
			this.floatTrackbarControlExposure.RangeMax = 1000F;
			this.floatTrackbarControlExposure.RangeMin = 0F;
			this.floatTrackbarControlExposure.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlExposure.TabIndex = 1;
			this.floatTrackbarControlExposure.Value = 1F;
			this.floatTrackbarControlExposure.VisibleRangeMax = 1F;
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(1371, 732);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(51, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "Exposure";
			// 
			// floatTrackbarControlAlbedo
			// 
			this.floatTrackbarControlAlbedo.Enabled = false;
			this.floatTrackbarControlAlbedo.Location = new System.Drawing.Point(124, 81);
			this.floatTrackbarControlAlbedo.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlAlbedo.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlAlbedo.Name = "floatTrackbarControlAlbedo";
			this.floatTrackbarControlAlbedo.RangeMax = 1F;
			this.floatTrackbarControlAlbedo.RangeMin = 0F;
			this.floatTrackbarControlAlbedo.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlAlbedo.TabIndex = 1;
			this.floatTrackbarControlAlbedo.Value = 0.5F;
			this.floatTrackbarControlAlbedo.VisibleRangeMax = 1F;
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(3, 153);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(108, 13);
			this.label6.TabIndex = 3;
			this.label6.Text = "Environment Intensity";
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
			this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Location = new System.Drawing.Point(134, 57);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(186, 12);
			this.panel1.TabIndex = 5;
			// 
			// checkBoxAnimate
			// 
			this.checkBoxAnimate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxAnimate.AutoSize = true;
			this.checkBoxAnimate.Location = new System.Drawing.Point(1309, 761);
			this.checkBoxAnimate.Name = "checkBoxAnimate";
			this.checkBoxAnimate.Size = new System.Drawing.Size(64, 17);
			this.checkBoxAnimate.TabIndex = 4;
			this.checkBoxAnimate.Text = "Animate";
			this.checkBoxAnimate.UseVisualStyleBackColor = true;
			this.checkBoxAnimate.CheckedChanged += new System.EventHandler(this.checkBoxPause_CheckedChanged);
			// 
			// checkBoxPause
			// 
			this.checkBoxPause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxPause.AutoSize = true;
			this.checkBoxPause.Location = new System.Drawing.Point(1379, 761);
			this.checkBoxPause.Name = "checkBoxPause";
			this.checkBoxPause.Size = new System.Drawing.Size(56, 17);
			this.checkBoxPause.TabIndex = 4;
			this.checkBoxPause.Text = "Pause";
			this.checkBoxPause.UseVisualStyleBackColor = true;
			this.checkBoxPause.CheckedChanged += new System.EventHandler(this.checkBoxPause_CheckedChanged);
			// 
			// buttonClear
			// 
			this.buttonClear.Location = new System.Drawing.Point(1441, 755);
			this.buttonClear.Name = "buttonClear";
			this.buttonClear.Size = new System.Drawing.Size(75, 23);
			this.buttonClear.TabIndex = 6;
			this.buttonClear.Text = "Clear";
			this.buttonClear.UseVisualStyleBackColor = true;
			this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
			// 
			// checkBoxEnableHBIL
			// 
			this.checkBoxEnableHBIL.AutoSize = true;
			this.checkBoxEnableHBIL.Checked = true;
			this.checkBoxEnableHBIL.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnableHBIL.Location = new System.Drawing.Point(6, 19);
			this.checkBoxEnableHBIL.Name = "checkBoxEnableHBIL";
			this.checkBoxEnableHBIL.Size = new System.Drawing.Size(50, 17);
			this.checkBoxEnableHBIL.TabIndex = 7;
			this.checkBoxEnableHBIL.Text = "HBIL";
			this.checkBoxEnableHBIL.UseVisualStyleBackColor = true;
			// 
			// checkBoxEnableBentNormal
			// 
			this.checkBoxEnableBentNormal.AutoSize = true;
			this.checkBoxEnableBentNormal.Checked = true;
			this.checkBoxEnableBentNormal.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnableBentNormal.Location = new System.Drawing.Point(76, 19);
			this.checkBoxEnableBentNormal.Name = "checkBoxEnableBentNormal";
			this.checkBoxEnableBentNormal.Size = new System.Drawing.Size(106, 17);
			this.checkBoxEnableBentNormal.TabIndex = 8;
			this.checkBoxEnableBentNormal.Text = "Use Bent Normal";
			this.checkBoxEnableBentNormal.UseVisualStyleBackColor = true;
			// 
			// checkBoxEnableConeVisibility
			// 
			this.checkBoxEnableConeVisibility.AutoSize = true;
			this.checkBoxEnableConeVisibility.Checked = true;
			this.checkBoxEnableConeVisibility.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnableConeVisibility.Location = new System.Drawing.Point(188, 19);
			this.checkBoxEnableConeVisibility.Name = "checkBoxEnableConeVisibility";
			this.checkBoxEnableConeVisibility.Size = new System.Drawing.Size(108, 17);
			this.checkBoxEnableConeVisibility.TabIndex = 8;
			this.checkBoxEnableConeVisibility.Text = "Use Cone Angles";
			this.checkBoxEnableConeVisibility.UseVisualStyleBackColor = true;
			// 
			// checkBoxForceAlbedo
			// 
			this.checkBoxForceAlbedo.AutoSize = true;
			this.checkBoxForceAlbedo.Location = new System.Drawing.Point(6, 82);
			this.checkBoxForceAlbedo.Name = "checkBoxForceAlbedo";
			this.checkBoxForceAlbedo.Size = new System.Drawing.Size(89, 17);
			this.checkBoxForceAlbedo.TabIndex = 4;
			this.checkBoxForceAlbedo.Text = "Force Albedo";
			this.checkBoxForceAlbedo.UseVisualStyleBackColor = true;
			this.checkBoxForceAlbedo.CheckedChanged += new System.EventHandler(this.checkBoxForceAlbedo_CheckedChanged);
			// 
			// textBoxInfo
			// 
			this.textBoxInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxInfo.Location = new System.Drawing.Point(1298, 12);
			this.textBoxInfo.Multiline = true;
			this.textBoxInfo.Name = "textBoxInfo";
			this.textBoxInfo.ReadOnly = true;
			this.textBoxInfo.Size = new System.Drawing.Size(329, 152);
			this.textBoxInfo.TabIndex = 9;
			// 
			// checkBoxMonochrome
			// 
			this.checkBoxMonochrome.AutoSize = true;
			this.checkBoxMonochrome.Enabled = false;
			this.checkBoxMonochrome.Location = new System.Drawing.Point(124, 104);
			this.checkBoxMonochrome.Name = "checkBoxMonochrome";
			this.checkBoxMonochrome.Size = new System.Drawing.Size(90, 17);
			this.checkBoxMonochrome.TabIndex = 4;
			this.checkBoxMonochrome.Text = "Keep Chroma";
			this.checkBoxMonochrome.UseVisualStyleBackColor = true;
			this.checkBoxMonochrome.CheckedChanged += new System.EventHandler(this.checkBoxForceAlbedo_CheckedChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 59);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(85, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Cone Angle Bias";
			// 
			// floatTrackbarControlConeAngleBias
			// 
			this.floatTrackbarControlConeAngleBias.Location = new System.Drawing.Point(124, 55);
			this.floatTrackbarControlConeAngleBias.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlConeAngleBias.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlConeAngleBias.Name = "floatTrackbarControlConeAngleBias";
			this.floatTrackbarControlConeAngleBias.RangeMax = 1F;
			this.floatTrackbarControlConeAngleBias.RangeMin = -1F;
			this.floatTrackbarControlConeAngleBias.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlConeAngleBias.TabIndex = 1;
			this.floatTrackbarControlConeAngleBias.Value = -0.2F;
			this.floatTrackbarControlConeAngleBias.VisibleRangeMax = 1F;
			this.floatTrackbarControlConeAngleBias.VisibleRangeMin = -1F;
			// 
			// integerTrackbarControlDebugMip
			// 
			this.integerTrackbarControlDebugMip.Location = new System.Drawing.Point(127, 75);
			this.integerTrackbarControlDebugMip.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlDebugMip.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlDebugMip.Name = "integerTrackbarControlDebugMip";
			this.integerTrackbarControlDebugMip.RangeMin = 0;
			this.integerTrackbarControlDebugMip.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlDebugMip.TabIndex = 10;
			this.integerTrackbarControlDebugMip.Value = 0;
			this.integerTrackbarControlDebugMip.VisibleRangeMax = 10;
			// 
			// checkBoxAutoRotateCamera
			// 
			this.checkBoxAutoRotateCamera.AutoSize = true;
			this.checkBoxAutoRotateCamera.Location = new System.Drawing.Point(6, 19);
			this.checkBoxAutoRotateCamera.Name = "checkBoxAutoRotateCamera";
			this.checkBoxAutoRotateCamera.Size = new System.Drawing.Size(122, 17);
			this.checkBoxAutoRotateCamera.TabIndex = 4;
			this.checkBoxAutoRotateCamera.Text = "Auto-Rotate Camera";
			this.checkBoxAutoRotateCamera.UseVisualStyleBackColor = true;
			this.checkBoxAutoRotateCamera.CheckedChanged += new System.EventHandler(this.checkBoxForceAlbedo_CheckedChanged);
			// 
			// floatTrackbarControlCameraRotateSpeed
			// 
			this.floatTrackbarControlCameraRotateSpeed.Location = new System.Drawing.Point(134, 17);
			this.floatTrackbarControlCameraRotateSpeed.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlCameraRotateSpeed.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlCameraRotateSpeed.Name = "floatTrackbarControlCameraRotateSpeed";
			this.floatTrackbarControlCameraRotateSpeed.RangeMax = 1000F;
			this.floatTrackbarControlCameraRotateSpeed.RangeMin = -1000F;
			this.floatTrackbarControlCameraRotateSpeed.Size = new System.Drawing.Size(195, 20);
			this.floatTrackbarControlCameraRotateSpeed.TabIndex = 1;
			this.floatTrackbarControlCameraRotateSpeed.Value = 4F;
			this.floatTrackbarControlCameraRotateSpeed.VisibleRangeMax = 4F;
			this.floatTrackbarControlCameraRotateSpeed.VisibleRangeMin = -4F;
			// 
			// radioButtonPUSH
			// 
			this.radioButtonPUSH.AutoSize = true;
			this.radioButtonPUSH.Location = new System.Drawing.Point(6, 97);
			this.radioButtonPUSH.Name = "radioButtonPUSH";
			this.radioButtonPUSH.Size = new System.Drawing.Size(85, 17);
			this.radioButtonPUSH.TabIndex = 11;
			this.radioButtonPUSH.TabStop = true;
			this.radioButtonPUSH.Text = "PUSH Chain";
			this.radioButtonPUSH.UseVisualStyleBackColor = true;
			// 
			// radioButtonPULL
			// 
			this.radioButtonPULL.AutoSize = true;
			this.radioButtonPULL.Checked = true;
			this.radioButtonPULL.Location = new System.Drawing.Point(6, 75);
			this.radioButtonPULL.Name = "radioButtonPULL";
			this.radioButtonPULL.Size = new System.Drawing.Size(82, 17);
			this.radioButtonPULL.TabIndex = 11;
			this.radioButtonPULL.TabStop = true;
			this.radioButtonPULL.Text = "PULL Chain";
			this.radioButtonPULL.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(97, 77);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(24, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Mip";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(5, 179);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(79, 13);
			this.label7.TabIndex = 3;
			this.label7.Text = "Prefered Depth";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 153);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(95, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Bilateral Delta Max";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 127);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(92, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Bilateral Delta Min";
			// 
			// floatTrackbarControlHarmonicPreferedDepth
			// 
			this.floatTrackbarControlHarmonicPreferedDepth.Location = new System.Drawing.Point(129, 176);
			this.floatTrackbarControlHarmonicPreferedDepth.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlHarmonicPreferedDepth.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlHarmonicPreferedDepth.Name = "floatTrackbarControlHarmonicPreferedDepth";
			this.floatTrackbarControlHarmonicPreferedDepth.RangeMax = 1000F;
			this.floatTrackbarControlHarmonicPreferedDepth.RangeMin = 0F;
			this.floatTrackbarControlHarmonicPreferedDepth.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlHarmonicPreferedDepth.TabIndex = 1;
			this.floatTrackbarControlHarmonicPreferedDepth.Value = 5F;
			// 
			// floatTrackbarControlBilateralDepthDeltaMax
			// 
			this.floatTrackbarControlBilateralDepthDeltaMax.Location = new System.Drawing.Point(129, 150);
			this.floatTrackbarControlBilateralDepthDeltaMax.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBilateralDepthDeltaMax.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBilateralDepthDeltaMax.Name = "floatTrackbarControlBilateralDepthDeltaMax";
			this.floatTrackbarControlBilateralDepthDeltaMax.RangeMax = 1000F;
			this.floatTrackbarControlBilateralDepthDeltaMax.RangeMin = 0F;
			this.floatTrackbarControlBilateralDepthDeltaMax.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlBilateralDepthDeltaMax.TabIndex = 1;
			this.floatTrackbarControlBilateralDepthDeltaMax.Value = 4F;
			this.floatTrackbarControlBilateralDepthDeltaMax.VisibleRangeMax = 4F;
			// 
			// floatTrackbarControlBilateralDepthDeltaMin
			// 
			this.floatTrackbarControlBilateralDepthDeltaMin.Location = new System.Drawing.Point(129, 124);
			this.floatTrackbarControlBilateralDepthDeltaMin.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBilateralDepthDeltaMin.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBilateralDepthDeltaMin.Name = "floatTrackbarControlBilateralDepthDeltaMin";
			this.floatTrackbarControlBilateralDepthDeltaMin.RangeMax = 1000F;
			this.floatTrackbarControlBilateralDepthDeltaMin.RangeMin = 0F;
			this.floatTrackbarControlBilateralDepthDeltaMin.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlBilateralDepthDeltaMin.TabIndex = 1;
			this.floatTrackbarControlBilateralDepthDeltaMin.Value = 0F;
			this.floatTrackbarControlBilateralDepthDeltaMin.VisibleRangeMax = 1F;
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1280, 720);
			this.panelOutput.TabIndex = 0;
			this.panelOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseDown);
			this.panelOutput.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseMove);
			this.panelOutput.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseUp);
			// 
			// checkBoxFreezePrev2CurrentCamMatrix
			// 
			this.checkBoxFreezePrev2CurrentCamMatrix.AutoSize = true;
			this.checkBoxFreezePrev2CurrentCamMatrix.Location = new System.Drawing.Point(6, 42);
			this.checkBoxFreezePrev2CurrentCamMatrix.Name = "checkBoxFreezePrev2CurrentCamMatrix";
			this.checkBoxFreezePrev2CurrentCamMatrix.Size = new System.Drawing.Size(175, 17);
			this.checkBoxFreezePrev2CurrentCamMatrix.TabIndex = 4;
			this.checkBoxFreezePrev2CurrentCamMatrix.Text = "Freeze Temporal Camera Matrix";
			this.checkBoxFreezePrev2CurrentCamMatrix.UseVisualStyleBackColor = true;
			this.checkBoxFreezePrev2CurrentCamMatrix.CheckedChanged += new System.EventHandler(this.checkBoxForceAlbedo_CheckedChanged);
			// 
			// groupBoxPushPull
			// 
			this.groupBoxPushPull.Controls.Add(this.panel1);
			this.groupBoxPushPull.Controls.Add(this.radioButtonPUSH);
			this.groupBoxPushPull.Controls.Add(this.checkBoxAutoRotateCamera);
			this.groupBoxPushPull.Controls.Add(this.radioButtonPULL);
			this.groupBoxPushPull.Controls.Add(this.integerTrackbarControlDebugMip);
			this.groupBoxPushPull.Controls.Add(this.checkBoxFreezePrev2CurrentCamMatrix);
			this.groupBoxPushPull.Controls.Add(this.label2);
			this.groupBoxPushPull.Controls.Add(this.floatTrackbarControlCameraRotateSpeed);
			this.groupBoxPushPull.Controls.Add(this.label7);
			this.groupBoxPushPull.Controls.Add(this.floatTrackbarControlBilateralDepthDeltaMin);
			this.groupBoxPushPull.Controls.Add(this.label4);
			this.groupBoxPushPull.Controls.Add(this.floatTrackbarControlBilateralDepthDeltaMax);
			this.groupBoxPushPull.Controls.Add(this.label3);
			this.groupBoxPushPull.Controls.Add(this.floatTrackbarControlHarmonicPreferedDepth);
			this.groupBoxPushPull.Location = new System.Drawing.Point(1298, 170);
			this.groupBoxPushPull.Name = "groupBoxPushPull";
			this.groupBoxPushPull.Size = new System.Drawing.Size(335, 201);
			this.groupBoxPushPull.TabIndex = 12;
			this.groupBoxPushPull.TabStop = false;
			this.groupBoxPushPull.Text = "Reprojection + Push / Pull";
			// 
			// groupBoxHBIL
			// 
			this.groupBoxHBIL.Controls.Add(this.floatTrackbarControlBilateral3);
			this.groupBoxHBIL.Controls.Add(this.floatTrackbarControlBilateral1);
			this.groupBoxHBIL.Controls.Add(this.floatTrackbarControlBilateral2);
			this.groupBoxHBIL.Controls.Add(this.floatTrackbarControlBilateral0);
			this.groupBoxHBIL.Controls.Add(this.label9);
			this.groupBoxHBIL.Controls.Add(this.floatTrackbarControlGatherSphereRadius_pixels);
			this.groupBoxHBIL.Controls.Add(this.label10);
			this.groupBoxHBIL.Controls.Add(this.floatTrackbarControlGatherSphereRadius_meters);
			this.groupBoxHBIL.Controls.Add(this.label8);
			this.groupBoxHBIL.Location = new System.Drawing.Point(1298, 377);
			this.groupBoxHBIL.Name = "groupBoxHBIL";
			this.groupBoxHBIL.Size = new System.Drawing.Size(335, 146);
			this.groupBoxHBIL.TabIndex = 13;
			this.groupBoxHBIL.TabStop = false;
			this.groupBoxHBIL.Text = "HBIL";
			// 
			// floatTrackbarControlBilateral3
			// 
			this.floatTrackbarControlBilateral3.Location = new System.Drawing.Point(229, 120);
			this.floatTrackbarControlBilateral3.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBilateral3.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBilateral3.Name = "floatTrackbarControlBilateral3";
			this.floatTrackbarControlBilateral3.RangeMax = 1000F;
			this.floatTrackbarControlBilateral3.RangeMin = 0F;
			this.floatTrackbarControlBilateral3.Size = new System.Drawing.Size(100, 20);
			this.floatTrackbarControlBilateral3.TabIndex = 1;
			this.floatTrackbarControlBilateral3.Value = 1F;
			this.floatTrackbarControlBilateral3.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlBilateral1
			// 
			this.floatTrackbarControlBilateral1.Location = new System.Drawing.Point(228, 94);
			this.floatTrackbarControlBilateral1.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBilateral1.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBilateral1.Name = "floatTrackbarControlBilateral1";
			this.floatTrackbarControlBilateral1.RangeMax = 1000F;
			this.floatTrackbarControlBilateral1.RangeMin = 0F;
			this.floatTrackbarControlBilateral1.Size = new System.Drawing.Size(100, 20);
			this.floatTrackbarControlBilateral1.TabIndex = 1;
			this.floatTrackbarControlBilateral1.Value = 1F;
			this.floatTrackbarControlBilateral1.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlBilateral2
			// 
			this.floatTrackbarControlBilateral2.Location = new System.Drawing.Point(129, 120);
			this.floatTrackbarControlBilateral2.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBilateral2.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBilateral2.Name = "floatTrackbarControlBilateral2";
			this.floatTrackbarControlBilateral2.RangeMax = 1000F;
			this.floatTrackbarControlBilateral2.RangeMin = 0F;
			this.floatTrackbarControlBilateral2.Size = new System.Drawing.Size(100, 20);
			this.floatTrackbarControlBilateral2.TabIndex = 1;
			this.floatTrackbarControlBilateral2.Value = 0.5F;
			this.floatTrackbarControlBilateral2.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlBilateral0
			// 
			this.floatTrackbarControlBilateral0.Location = new System.Drawing.Point(129, 94);
			this.floatTrackbarControlBilateral0.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBilateral0.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBilateral0.Name = "floatTrackbarControlBilateral0";
			this.floatTrackbarControlBilateral0.RangeMax = 1000F;
			this.floatTrackbarControlBilateral0.RangeMin = 0F;
			this.floatTrackbarControlBilateral0.Size = new System.Drawing.Size(100, 20);
			this.floatTrackbarControlBilateral0.TabIndex = 1;
			this.floatTrackbarControlBilateral0.Value = 0F;
			this.floatTrackbarControlBilateral0.VisibleRangeMax = 1F;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(5, 101);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(79, 13);
			this.label9.TabIndex = 3;
			this.label9.Text = "Bilateral Values";
			// 
			// floatTrackbarControlGatherSphereRadius_pixels
			// 
			this.floatTrackbarControlGatherSphereRadius_pixels.Location = new System.Drawing.Point(129, 45);
			this.floatTrackbarControlGatherSphereRadius_pixels.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlGatherSphereRadius_pixels.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlGatherSphereRadius_pixels.Name = "floatTrackbarControlGatherSphereRadius_pixels";
			this.floatTrackbarControlGatherSphereRadius_pixels.RangeMax = 10000F;
			this.floatTrackbarControlGatherSphereRadius_pixels.RangeMin = 0F;
			this.floatTrackbarControlGatherSphereRadius_pixels.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlGatherSphereRadius_pixels.TabIndex = 1;
			this.floatTrackbarControlGatherSphereRadius_pixels.Value = 800F;
			this.floatTrackbarControlGatherSphereRadius_pixels.VisibleRangeMax = 800F;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(6, 48);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(98, 13);
			this.label10.TabIndex = 3;
			this.label10.Text = "Max Radius (pixels)";
			// 
			// floatTrackbarControlGatherSphereRadius_meters
			// 
			this.floatTrackbarControlGatherSphereRadius_meters.Location = new System.Drawing.Point(128, 19);
			this.floatTrackbarControlGatherSphereRadius_meters.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlGatherSphereRadius_meters.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlGatherSphereRadius_meters.Name = "floatTrackbarControlGatherSphereRadius_meters";
			this.floatTrackbarControlGatherSphereRadius_meters.RangeMax = 1000F;
			this.floatTrackbarControlGatherSphereRadius_meters.RangeMin = 0F;
			this.floatTrackbarControlGatherSphereRadius_meters.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlGatherSphereRadius_meters.TabIndex = 1;
			this.floatTrackbarControlGatherSphereRadius_meters.Value = 8F;
			this.floatTrackbarControlGatherSphereRadius_meters.VisibleRangeMax = 8F;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(5, 22);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(112, 13);
			this.label8.TabIndex = 3;
			this.label8.Text = "Gather Sphere Radius";
			// 
			// groupBoxLighting
			// 
			this.groupBoxLighting.Controls.Add(this.checkBoxEnableHBIL);
			this.groupBoxLighting.Controls.Add(this.label1);
			this.groupBoxLighting.Controls.Add(this.checkBoxEnableBentNormalDirect);
			this.groupBoxLighting.Controls.Add(this.checkBoxEnableBentNormal);
			this.groupBoxLighting.Controls.Add(this.checkBoxEnableConeVisibilityDirect);
			this.groupBoxLighting.Controls.Add(this.checkBoxEnableConeVisibility);
			this.groupBoxLighting.Controls.Add(this.floatTrackbarControlSunIntensity);
			this.groupBoxLighting.Controls.Add(this.floatTrackbarControlEnvironmentIntensity);
			this.groupBoxLighting.Controls.Add(this.label11);
			this.groupBoxLighting.Controls.Add(this.floatTrackbarControlConeAngleBias);
			this.groupBoxLighting.Controls.Add(this.label6);
			this.groupBoxLighting.Controls.Add(this.checkBoxMonochrome);
			this.groupBoxLighting.Controls.Add(this.checkBoxForceAlbedo);
			this.groupBoxLighting.Controls.Add(this.floatTrackbarControlAlbedo);
			this.groupBoxLighting.Location = new System.Drawing.Point(1298, 529);
			this.groupBoxLighting.Name = "groupBoxLighting";
			this.groupBoxLighting.Size = new System.Drawing.Size(335, 175);
			this.groupBoxLighting.TabIndex = 14;
			this.groupBoxLighting.TabStop = false;
			this.groupBoxLighting.Text = "Lighting";
			// 
			// checkBoxEnableBentNormalDirect
			// 
			this.checkBoxEnableBentNormalDirect.AutoSize = true;
			this.checkBoxEnableBentNormalDirect.Location = new System.Drawing.Point(76, 37);
			this.checkBoxEnableBentNormalDirect.Name = "checkBoxEnableBentNormalDirect";
			this.checkBoxEnableBentNormalDirect.Size = new System.Drawing.Size(94, 17);
			this.checkBoxEnableBentNormalDirect.TabIndex = 8;
			this.checkBoxEnableBentNormalDirect.Text = "For Direct Too";
			this.checkBoxEnableBentNormalDirect.UseVisualStyleBackColor = true;
			// 
			// checkBoxEnableConeVisibilityDirect
			// 
			this.checkBoxEnableConeVisibilityDirect.AutoSize = true;
			this.checkBoxEnableConeVisibilityDirect.Location = new System.Drawing.Point(188, 37);
			this.checkBoxEnableConeVisibilityDirect.Name = "checkBoxEnableConeVisibilityDirect";
			this.checkBoxEnableConeVisibilityDirect.Size = new System.Drawing.Size(94, 17);
			this.checkBoxEnableConeVisibilityDirect.TabIndex = 8;
			this.checkBoxEnableConeVisibilityDirect.Text = "For Direct Too";
			this.checkBoxEnableConeVisibilityDirect.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControlSunIntensity
			// 
			this.floatTrackbarControlSunIntensity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlSunIntensity.Location = new System.Drawing.Point(124, 124);
			this.floatTrackbarControlSunIntensity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSunIntensity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSunIntensity.Name = "floatTrackbarControlSunIntensity";
			this.floatTrackbarControlSunIntensity.RangeMax = 10000F;
			this.floatTrackbarControlSunIntensity.RangeMin = 0F;
			this.floatTrackbarControlSunIntensity.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlSunIntensity.TabIndex = 1;
			this.floatTrackbarControlSunIntensity.Value = 10F;
			// 
			// label11
			// 
			this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(3, 128);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(68, 13);
			this.label11.TabIndex = 3;
			this.label11.Text = "Sun Intensity";
			// 
			// checkBoxEnableTAA
			// 
			this.checkBoxEnableTAA.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxEnableTAA.AutoSize = true;
			this.checkBoxEnableTAA.Checked = true;
			this.checkBoxEnableTAA.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnableTAA.Location = new System.Drawing.Point(1309, 733);
			this.checkBoxEnableTAA.Name = "checkBoxEnableTAA";
			this.checkBoxEnableTAA.Size = new System.Drawing.Size(47, 17);
			this.checkBoxEnableTAA.TabIndex = 4;
			this.checkBoxEnableTAA.Text = "TAA";
			this.checkBoxEnableTAA.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowAO
			// 
			this.checkBoxShowAO.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxShowAO.AutoSize = true;
			this.checkBoxShowAO.Location = new System.Drawing.Point(1309, 710);
			this.checkBoxShowAO.Name = "checkBoxShowAO";
			this.checkBoxShowAO.Size = new System.Drawing.Size(71, 17);
			this.checkBoxShowAO.TabIndex = 4;
			this.checkBoxShowAO.Text = "Show AO";
			this.checkBoxShowAO.UseVisualStyleBackColor = true;
			this.checkBoxShowAO.CheckedChanged += new System.EventHandler(this.checkBoxShowAO_CheckedChanged);
			// 
			// checkBoxShowIrradiance
			// 
			this.checkBoxShowIrradiance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxShowIrradiance.AutoSize = true;
			this.checkBoxShowIrradiance.Location = new System.Drawing.Point(1386, 710);
			this.checkBoxShowIrradiance.Name = "checkBoxShowIrradiance";
			this.checkBoxShowIrradiance.Size = new System.Drawing.Size(63, 17);
			this.checkBoxShowIrradiance.TabIndex = 4;
			this.checkBoxShowIrradiance.Text = "Show E";
			this.checkBoxShowIrradiance.UseVisualStyleBackColor = true;
			this.checkBoxShowIrradiance.CheckedChanged += new System.EventHandler(this.checkBoxShowAO_CheckedChanged);
			// 
			// checkBoxEnablePushPull
			// 
			this.checkBoxEnablePushPull.AutoSize = true;
			this.checkBoxEnablePushPull.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkBoxEnablePushPull.Checked = true;
			this.checkBoxEnablePushPull.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnablePushPull.Location = new System.Drawing.Point(1301, 169);
			this.checkBoxEnablePushPull.Name = "checkBoxEnablePushPull";
			this.checkBoxEnablePushPull.Size = new System.Drawing.Size(150, 17);
			this.checkBoxEnablePushPull.TabIndex = 8;
			this.checkBoxEnablePushPull.Text = "Reprojection + Push / Pull";
			this.checkBoxEnablePushPull.UseVisualStyleBackColor = true;
			// 
			// TestHBILForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1639, 792);
			this.Controls.Add(this.groupBoxLighting);
			this.Controls.Add(this.groupBoxHBIL);
			this.Controls.Add(this.checkBoxEnablePushPull);
			this.Controls.Add(this.groupBoxPushPull);
			this.Controls.Add(this.textBoxInfo);
			this.Controls.Add(this.buttonClear);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.floatTrackbarControlExposure);
			this.Controls.Add(this.checkBoxPause);
			this.Controls.Add(this.checkBoxEnableTAA);
			this.Controls.Add(this.checkBoxShowIrradiance);
			this.Controls.Add(this.checkBoxShowAO);
			this.Controls.Add(this.checkBoxAnimate);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.KeyPreview = true;
			this.Name = "TestHBILForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "HBIL Test";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TestHBILForm_KeyDown);
			this.groupBoxPushPull.ResumeLayout(false);
			this.groupBoxPushPull.PerformLayout();
			this.groupBoxHBIL.ResumeLayout(false);
			this.groupBoxHBIL.PerformLayout();
			this.groupBoxLighting.ResumeLayout(false);
			this.groupBoxLighting.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelOutput;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Timer timer;
		private UIUtility.FloatTrackbarControl floatTrackbarControlEnvironmentIntensity;
		private UIUtility.FloatTrackbarControl floatTrackbarControlExposure;
		private System.Windows.Forms.Label label5;
		private UIUtility.FloatTrackbarControl floatTrackbarControlAlbedo;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.CheckBox checkBoxAnimate;
		private System.Windows.Forms.CheckBox checkBoxPause;
		private System.Windows.Forms.Button buttonClear;
		private System.Windows.Forms.CheckBox checkBoxEnableHBIL;
		private System.Windows.Forms.CheckBox checkBoxEnableBentNormal;
		private System.Windows.Forms.CheckBox checkBoxEnableConeVisibility;
		private System.Windows.Forms.CheckBox checkBoxForceAlbedo;
		private System.Windows.Forms.TextBox textBoxInfo;
		private System.Windows.Forms.CheckBox checkBoxMonochrome;
		private System.Windows.Forms.Label label1;
		private UIUtility.FloatTrackbarControl floatTrackbarControlConeAngleBias;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlDebugMip;
		private System.Windows.Forms.CheckBox checkBoxAutoRotateCamera;
		private UIUtility.FloatTrackbarControl floatTrackbarControlCameraRotateSpeed;
		private System.Windows.Forms.RadioButton radioButtonPUSH;
		private System.Windows.Forms.RadioButton radioButtonPULL;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private UIUtility.FloatTrackbarControl floatTrackbarControlBilateralDepthDeltaMax;
		private UIUtility.FloatTrackbarControl floatTrackbarControlBilateralDepthDeltaMin;
		private System.Windows.Forms.Label label7;
		private UIUtility.FloatTrackbarControl floatTrackbarControlHarmonicPreferedDepth;
		private System.Windows.Forms.CheckBox checkBoxFreezePrev2CurrentCamMatrix;
		private System.Windows.Forms.GroupBox groupBoxPushPull;
		private System.Windows.Forms.GroupBox groupBoxHBIL;
		private System.Windows.Forms.GroupBox groupBoxLighting;
		private UIUtility.FloatTrackbarControl floatTrackbarControlGatherSphereRadius_meters;
		private System.Windows.Forms.Label label8;
		private UIUtility.FloatTrackbarControl floatTrackbarControlBilateral1;
		private UIUtility.FloatTrackbarControl floatTrackbarControlBilateral0;
		private System.Windows.Forms.Label label9;
		private UIUtility.FloatTrackbarControl floatTrackbarControlBilateral3;
		private UIUtility.FloatTrackbarControl floatTrackbarControlBilateral2;
		private System.Windows.Forms.CheckBox checkBoxEnableBentNormalDirect;
		private System.Windows.Forms.CheckBox checkBoxEnableConeVisibilityDirect;
		private UIUtility.FloatTrackbarControl floatTrackbarControlGatherSphereRadius_pixels;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.CheckBox checkBoxEnableTAA;
		private System.Windows.Forms.CheckBox checkBoxShowAO;
		private UIUtility.FloatTrackbarControl floatTrackbarControlSunIntensity;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.CheckBox checkBoxShowIrradiance;
		private System.Windows.Forms.CheckBox checkBoxEnablePushPull;
	}
}

