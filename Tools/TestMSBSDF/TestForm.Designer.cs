namespace TestMSBSDF
{
	partial class TestForm
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

			m_automation.Dispose();

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
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.buttonReload = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.checkBoxShowNormals = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonRayTrace = new System.Windows.Forms.Button();
			this.checkBoxShowTransmittedDirectionsHistogram = new System.Windows.Forms.RadioButton();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.checkBoxShowReflectedDirectionsHistogram = new System.Windows.Forms.RadioButton();
			this.label5 = new System.Windows.Forms.Label();
			this.radioButtonShowHeights = new System.Windows.Forms.RadioButton();
			this.radioButtonHideSurface = new System.Windows.Forms.RadioButton();
			this.groupBoxDisplay = new System.Windows.Forms.GroupBox();
			this.buttonFit = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.radioButtonAnalyticalPhong = new System.Windows.Forms.RadioButton();
			this.radioButtonAnalyticalGGX = new System.Windows.Forms.RadioButton();
			this.radioButtonAnalyticalBeckmann = new System.Windows.Forms.RadioButton();
			this.checkBoxShowAnalyticalLobe = new System.Windows.Forms.CheckBox();
			this.checkBoxShowWireframe = new System.Windows.Forms.CheckBox();
			this.checkBoxShowLobe = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlAnalyticalLobeTheta = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLobeScaleB = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlLobeScaleT = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlAnalyticalLobeRoughness = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlLobeScaleR = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlLobeIntensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.integerTrackbarControlScatteringOrder = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label6 = new System.Windows.Forms.Label();
			this.integerTrackbarControlIterationsCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.floatTrackbarControlPhi = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlTheta = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlBeckmannSizeFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlBeckmannRoughness = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label9 = new System.Windows.Forms.Label();
			this.floatTrackbarControlFitOversize = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label10 = new System.Windows.Forms.Label();
			this.checkBoxTest = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlSurfaceAlbedo = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label11 = new System.Windows.Forms.Label();
			this.groupBoxSurface = new System.Windows.Forms.GroupBox();
			this.buttonTestImage = new System.Windows.Forms.Button();
			this.panelDielectric = new System.Windows.Forms.Panel();
			this.floatTrackbarControlF0 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label12 = new System.Windows.Forms.Label();
			this.radioButtonDiffuse = new System.Windows.Forms.RadioButton();
			this.radioButtonDielectric = new System.Windows.Forms.RadioButton();
			this.radioButtonConductor = new System.Windows.Forms.RadioButton();
			this.groupBoxSimulation = new System.Windows.Forms.GroupBox();
			this.groupBoxAnalyticalLobe = new System.Windows.Forms.GroupBox();
			this.tabControlAnalyticalLobes = new System.Windows.Forms.TabControl();
			this.tabPageReflectedLobe = new System.Windows.Forms.TabPage();
			this.label20 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLobeMaskingImportance = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label14 = new System.Windows.Forms.Label();
			this.tabPageTransmittedLobe = new System.Windows.Forms.TabPage();
			this.label21 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLobeMaskingImportance_T = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label15 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLobeScaleB_T = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlAnalyticalLobeTheta_T = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label16 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLobeScaleT_T = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label17 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLobeScaleR_T = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label19 = new System.Windows.Forms.Label();
			this.floatTrackbarControlAnalyticalLobeRoughness_T = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.checkBoxInitializeDirectionTowardCenterOfMass = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkBoxCompensateScatteringFactor = new System.Windows.Forms.CheckBox();
			this.checkBoxShowXRay = new System.Windows.Forms.CheckBox();
			this.buttonAutomation = new System.Windows.Forms.Button();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabBeckmann = new System.Windows.Forms.TabPage();
			this.tabLoadTex = new System.Windows.Forms.TabPage();
			this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
			this.floatTrackbarScale = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.Scale = new System.Windows.Forms.Label();
			this.panelOutput = new TestMSBSDF.PanelOutput3D(this.components);
			this.groupBoxDisplay.SuspendLayout();
			this.panel2.SuspendLayout();
			this.groupBoxSurface.SuspendLayout();
			this.panelDielectric.SuspendLayout();
			this.groupBoxSimulation.SuspendLayout();
			this.groupBoxAnalyticalLobe.SuspendLayout();
			this.tabControlAnalyticalLobes.SuspendLayout();
			this.tabPageReflectedLobe.SuspendLayout();
			this.tabPageTransmittedLobe.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabBeckmann.SuspendLayout();
			this.tabLoadTex.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
			this.SuspendLayout();
			// 
			// timer1
			// 
			this.timer1.Interval = 10;
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(1158, 1312);
			this.buttonReload.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(174, 35);
			this.buttonReload.TabIndex = 1;
			this.buttonReload.Text = "Reload Shaders";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 35);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 20);
			this.label1.TabIndex = 3;
			this.label1.Text = "Roughness";
			this.label1.Click += new System.EventHandler(this.label1_Click);
			// 
			// checkBoxShowNormals
			// 
			this.checkBoxShowNormals.AutoSize = true;
			this.checkBoxShowNormals.Location = new System.Drawing.Point(14, 98);
			this.checkBoxShowNormals.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.checkBoxShowNormals.Name = "checkBoxShowNormals";
			this.checkBoxShowNormals.Size = new System.Drawing.Size(136, 24);
			this.checkBoxShowNormals.TabIndex = 4;
			this.checkBoxShowNormals.Text = "Show Normals";
			this.checkBoxShowNormals.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 24);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(164, 20);
			this.label2.TabIndex = 3;
			this.label2.Text = "Incoming Angle Theta";
			// 
			// buttonRayTrace
			// 
			this.buttonRayTrace.Location = new System.Drawing.Point(183, 147);
			this.buttonRayTrace.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.buttonRayTrace.Name = "buttonRayTrace";
			this.buttonRayTrace.Size = new System.Drawing.Size(112, 35);
			this.buttonRayTrace.TabIndex = 6;
			this.buttonRayTrace.Text = "Ray Trace";
			this.buttonRayTrace.UseVisualStyleBackColor = true;
			this.buttonRayTrace.Click += new System.EventHandler(this.buttonRayTrace_Click);
			// 
			// checkBoxShowTransmittedDirectionsHistogram
			// 
			this.checkBoxShowTransmittedDirectionsHistogram.AutoSize = true;
			this.checkBoxShowTransmittedDirectionsHistogram.Location = new System.Drawing.Point(244, 63);
			this.checkBoxShowTransmittedDirectionsHistogram.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.checkBoxShowTransmittedDirectionsHistogram.Name = "checkBoxShowTransmittedDirectionsHistogram";
			this.checkBoxShowTransmittedDirectionsHistogram.Size = new System.Drawing.Size(314, 24);
			this.checkBoxShowTransmittedDirectionsHistogram.TabIndex = 4;
			this.checkBoxShowTransmittedDirectionsHistogram.Text = "Show Transmitted Directions Histogram";
			this.checkBoxShowTransmittedDirectionsHistogram.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(14, 106);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(123, 20);
			this.label3.TabIndex = 3;
			this.label3.Text = "Iterations Count";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 65);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(145, 20);
			this.label4.TabIndex = 3;
			this.label4.Text = "Incoming Angle Phi";
			this.label4.Click += new System.EventHandler(this.label4_Click);
			// 
			// checkBoxShowReflectedDirectionsHistogram
			// 
			this.checkBoxShowReflectedDirectionsHistogram.AutoSize = true;
			this.checkBoxShowReflectedDirectionsHistogram.Location = new System.Drawing.Point(244, 29);
			this.checkBoxShowReflectedDirectionsHistogram.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.checkBoxShowReflectedDirectionsHistogram.Name = "checkBoxShowReflectedDirectionsHistogram";
			this.checkBoxShowReflectedDirectionsHistogram.Size = new System.Drawing.Size(299, 24);
			this.checkBoxShowReflectedDirectionsHistogram.TabIndex = 4;
			this.checkBoxShowReflectedDirectionsHistogram.Text = "Show Reflected Directions Histogram";
			this.checkBoxShowReflectedDirectionsHistogram.UseVisualStyleBackColor = true;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(4, 70);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(159, 20);
			this.label5.TabIndex = 3;
			this.label5.Text = "Lobe Intensity Factor";
			// 
			// radioButtonShowHeights
			// 
			this.radioButtonShowHeights.AutoSize = true;
			this.radioButtonShowHeights.Checked = true;
			this.radioButtonShowHeights.Location = new System.Drawing.Point(13, 63);
			this.radioButtonShowHeights.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.radioButtonShowHeights.Name = "radioButtonShowHeights";
			this.radioButtonShowHeights.Size = new System.Drawing.Size(133, 24);
			this.radioButtonShowHeights.TabIndex = 4;
			this.radioButtonShowHeights.TabStop = true;
			this.radioButtonShowHeights.Text = "Show Heights";
			this.radioButtonShowHeights.UseVisualStyleBackColor = true;
			// 
			// radioButtonHideSurface
			// 
			this.radioButtonHideSurface.AutoSize = true;
			this.radioButtonHideSurface.Location = new System.Drawing.Point(13, 29);
			this.radioButtonHideSurface.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.radioButtonHideSurface.Name = "radioButtonHideSurface";
			this.radioButtonHideSurface.Size = new System.Drawing.Size(127, 24);
			this.radioButtonHideSurface.TabIndex = 4;
			this.radioButtonHideSurface.Text = "Hide Surface";
			this.radioButtonHideSurface.UseVisualStyleBackColor = true;
			// 
			// groupBoxDisplay
			// 
			this.groupBoxDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxDisplay.Controls.Add(this.radioButtonShowHeights);
			this.groupBoxDisplay.Controls.Add(this.radioButtonHideSurface);
			this.groupBoxDisplay.Controls.Add(this.checkBoxShowNormals);
			this.groupBoxDisplay.Controls.Add(this.checkBoxShowTransmittedDirectionsHistogram);
			this.groupBoxDisplay.Controls.Add(this.checkBoxShowReflectedDirectionsHistogram);
			this.groupBoxDisplay.Location = new System.Drawing.Point(1345, 576);
			this.groupBoxDisplay.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBoxDisplay.Name = "groupBoxDisplay";
			this.groupBoxDisplay.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBoxDisplay.Size = new System.Drawing.Size(579, 129);
			this.groupBoxDisplay.TabIndex = 9;
			this.groupBoxDisplay.TabStop = false;
			this.groupBoxDisplay.Text = "Surface Display";
			// 
			// buttonFit
			// 
			this.buttonFit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonFit.Location = new System.Drawing.Point(1653, 1303);
			this.buttonFit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.buttonFit.Name = "buttonFit";
			this.buttonFit.Size = new System.Drawing.Size(146, 38);
			this.buttonFit.TabIndex = 11;
			this.buttonFit.Text = "&FIT";
			this.buttonFit.UseVisualStyleBackColor = true;
			this.buttonFit.Click += new System.EventHandler(this.buttonFit_Click);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.radioButtonAnalyticalPhong);
			this.panel2.Controls.Add(this.radioButtonAnalyticalGGX);
			this.panel2.Controls.Add(this.radioButtonAnalyticalBeckmann);
			this.panel2.Location = new System.Drawing.Point(95, 19);
			this.panel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(297, 34);
			this.panel2.TabIndex = 10;
			// 
			// radioButtonAnalyticalPhong
			// 
			this.radioButtonAnalyticalPhong.AutoSize = true;
			this.radioButtonAnalyticalPhong.Checked = true;
			this.radioButtonAnalyticalPhong.Location = new System.Drawing.Point(210, 3);
			this.radioButtonAnalyticalPhong.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.radioButtonAnalyticalPhong.Name = "radioButtonAnalyticalPhong";
			this.radioButtonAnalyticalPhong.Size = new System.Drawing.Size(80, 24);
			this.radioButtonAnalyticalPhong.TabIndex = 0;
			this.radioButtonAnalyticalPhong.TabStop = true;
			this.radioButtonAnalyticalPhong.Text = "Phong";
			this.radioButtonAnalyticalPhong.UseVisualStyleBackColor = true;
			// 
			// radioButtonAnalyticalGGX
			// 
			this.radioButtonAnalyticalGGX.AutoSize = true;
			this.radioButtonAnalyticalGGX.Location = new System.Drawing.Point(129, 3);
			this.radioButtonAnalyticalGGX.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.radioButtonAnalyticalGGX.Name = "radioButtonAnalyticalGGX";
			this.radioButtonAnalyticalGGX.Size = new System.Drawing.Size(71, 24);
			this.radioButtonAnalyticalGGX.TabIndex = 0;
			this.radioButtonAnalyticalGGX.Text = "GGX";
			this.radioButtonAnalyticalGGX.UseVisualStyleBackColor = true;
			// 
			// radioButtonAnalyticalBeckmann
			// 
			this.radioButtonAnalyticalBeckmann.AutoSize = true;
			this.radioButtonAnalyticalBeckmann.Location = new System.Drawing.Point(11, 3);
			this.radioButtonAnalyticalBeckmann.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.radioButtonAnalyticalBeckmann.Name = "radioButtonAnalyticalBeckmann";
			this.radioButtonAnalyticalBeckmann.Size = new System.Drawing.Size(110, 24);
			this.radioButtonAnalyticalBeckmann.TabIndex = 0;
			this.radioButtonAnalyticalBeckmann.Text = "Beckmann";
			this.radioButtonAnalyticalBeckmann.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowAnalyticalLobe
			// 
			this.checkBoxShowAnalyticalLobe.AutoSize = true;
			this.checkBoxShowAnalyticalLobe.Checked = true;
			this.checkBoxShowAnalyticalLobe.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowAnalyticalLobe.Location = new System.Drawing.Point(8, 29);
			this.checkBoxShowAnalyticalLobe.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.checkBoxShowAnalyticalLobe.Name = "checkBoxShowAnalyticalLobe";
			this.checkBoxShowAnalyticalLobe.Size = new System.Drawing.Size(79, 24);
			this.checkBoxShowAnalyticalLobe.TabIndex = 9;
			this.checkBoxShowAnalyticalLobe.Text = "Show ";
			this.checkBoxShowAnalyticalLobe.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowWireframe
			// 
			this.checkBoxShowWireframe.AutoSize = true;
			this.checkBoxShowWireframe.Checked = true;
			this.checkBoxShowWireframe.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowWireframe.Location = new System.Drawing.Point(184, 0);
			this.checkBoxShowWireframe.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.checkBoxShowWireframe.Name = "checkBoxShowWireframe";
			this.checkBoxShowWireframe.Size = new System.Drawing.Size(152, 24);
			this.checkBoxShowWireframe.TabIndex = 9;
			this.checkBoxShowWireframe.Text = "Show Wireframe";
			this.checkBoxShowWireframe.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowLobe
			// 
			this.checkBoxShowLobe.AutoSize = true;
			this.checkBoxShowLobe.Checked = true;
			this.checkBoxShowLobe.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowLobe.Location = new System.Drawing.Point(13, 29);
			this.checkBoxShowLobe.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.checkBoxShowLobe.Name = "checkBoxShowLobe";
			this.checkBoxShowLobe.Size = new System.Drawing.Size(196, 24);
			this.checkBoxShowLobe.TabIndex = 9;
			this.checkBoxShowLobe.Text = "Show Scattering Order";
			this.checkBoxShowLobe.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControlAnalyticalLobeTheta
			// 
			this.floatTrackbarControlAnalyticalLobeTheta.Location = new System.Drawing.Point(184, 9);
			this.floatTrackbarControlAnalyticalLobeTheta.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlAnalyticalLobeTheta.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlAnalyticalLobeTheta.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlAnalyticalLobeTheta.Name = "floatTrackbarControlAnalyticalLobeTheta";
			this.floatTrackbarControlAnalyticalLobeTheta.RangeMax = 89.999F;
			this.floatTrackbarControlAnalyticalLobeTheta.RangeMin = 0F;
			this.floatTrackbarControlAnalyticalLobeTheta.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlAnalyticalLobeTheta.TabIndex = 5;
			this.floatTrackbarControlAnalyticalLobeTheta.Value = 0F;
			this.floatTrackbarControlAnalyticalLobeTheta.VisibleRangeMax = 89.999F;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(9, 55);
			this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(91, 20);
			this.label8.TabIndex = 3;
			this.label8.Text = "Roughness";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(9, 97);
			this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(103, 20);
			this.label7.TabIndex = 3;
			this.label7.Text = "Scale Normal";
			// 
			// floatTrackbarControlLobeScaleB
			// 
			this.floatTrackbarControlLobeScaleB.Enabled = false;
			this.floatTrackbarControlLobeScaleB.Location = new System.Drawing.Point(184, 169);
			this.floatTrackbarControlLobeScaleB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlLobeScaleB.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlLobeScaleB.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlLobeScaleB.Name = "floatTrackbarControlLobeScaleB";
			this.floatTrackbarControlLobeScaleB.RangeMax = 10000F;
			this.floatTrackbarControlLobeScaleB.RangeMin = 0F;
			this.floatTrackbarControlLobeScaleB.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlLobeScaleB.TabIndex = 5;
			this.floatTrackbarControlLobeScaleB.Value = 1F;
			this.floatTrackbarControlLobeScaleB.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlLobeScaleT
			// 
			this.floatTrackbarControlLobeScaleT.Location = new System.Drawing.Point(184, 129);
			this.floatTrackbarControlLobeScaleT.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlLobeScaleT.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlLobeScaleT.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlLobeScaleT.Name = "floatTrackbarControlLobeScaleT";
			this.floatTrackbarControlLobeScaleT.RangeMax = 10000F;
			this.floatTrackbarControlLobeScaleT.RangeMin = 0F;
			this.floatTrackbarControlLobeScaleT.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlLobeScaleT.TabIndex = 5;
			this.floatTrackbarControlLobeScaleT.Value = 1F;
			this.floatTrackbarControlLobeScaleT.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlAnalyticalLobeRoughness
			// 
			this.floatTrackbarControlAnalyticalLobeRoughness.Location = new System.Drawing.Point(184, 49);
			this.floatTrackbarControlAnalyticalLobeRoughness.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlAnalyticalLobeRoughness.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlAnalyticalLobeRoughness.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlAnalyticalLobeRoughness.Name = "floatTrackbarControlAnalyticalLobeRoughness";
			this.floatTrackbarControlAnalyticalLobeRoughness.RangeMax = 1F;
			this.floatTrackbarControlAnalyticalLobeRoughness.RangeMin = 0F;
			this.floatTrackbarControlAnalyticalLobeRoughness.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlAnalyticalLobeRoughness.TabIndex = 2;
			this.floatTrackbarControlAnalyticalLobeRoughness.Value = 0.9444F;
			this.floatTrackbarControlAnalyticalLobeRoughness.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlLobeScaleR
			// 
			this.floatTrackbarControlLobeScaleR.Location = new System.Drawing.Point(184, 89);
			this.floatTrackbarControlLobeScaleR.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlLobeScaleR.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlLobeScaleR.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlLobeScaleR.Name = "floatTrackbarControlLobeScaleR";
			this.floatTrackbarControlLobeScaleR.RangeMax = 10000F;
			this.floatTrackbarControlLobeScaleR.RangeMin = 0F;
			this.floatTrackbarControlLobeScaleR.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlLobeScaleR.TabIndex = 5;
			this.floatTrackbarControlLobeScaleR.Value = 0.1666F;
			this.floatTrackbarControlLobeScaleR.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlLobeIntensity
			// 
			this.floatTrackbarControlLobeIntensity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlLobeIntensity.Location = new System.Drawing.Point(213, 70);
			this.floatTrackbarControlLobeIntensity.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlLobeIntensity.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlLobeIntensity.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlLobeIntensity.Name = "floatTrackbarControlLobeIntensity";
			this.floatTrackbarControlLobeIntensity.RangeMax = 10000F;
			this.floatTrackbarControlLobeIntensity.RangeMin = 0F;
			this.floatTrackbarControlLobeIntensity.Size = new System.Drawing.Size(350, 31);
			this.floatTrackbarControlLobeIntensity.TabIndex = 5;
			this.floatTrackbarControlLobeIntensity.Value = 1F;
			this.floatTrackbarControlLobeIntensity.VisibleRangeMax = 2F;
			// 
			// integerTrackbarControlScatteringOrder
			// 
			this.integerTrackbarControlScatteringOrder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.integerTrackbarControlScatteringOrder.Location = new System.Drawing.Point(213, 29);
			this.integerTrackbarControlScatteringOrder.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.integerTrackbarControlScatteringOrder.MaximumSize = new System.Drawing.Size(15000, 31);
			this.integerTrackbarControlScatteringOrder.MinimumSize = new System.Drawing.Size(105, 31);
			this.integerTrackbarControlScatteringOrder.Name = "integerTrackbarControlScatteringOrder";
			this.integerTrackbarControlScatteringOrder.RangeMax = 4;
			this.integerTrackbarControlScatteringOrder.RangeMin = 1;
			this.integerTrackbarControlScatteringOrder.Size = new System.Drawing.Size(350, 31);
			this.integerTrackbarControlScatteringOrder.TabIndex = 7;
			this.integerTrackbarControlScatteringOrder.Value = 2;
			this.integerTrackbarControlScatteringOrder.VisibleRangeMax = 4;
			this.integerTrackbarControlScatteringOrder.VisibleRangeMin = 1;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(9, 74);
			this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(90, 20);
			this.label6.TabIndex = 3;
			this.label6.Text = "Size Factor";
			// 
			// integerTrackbarControlIterationsCount
			// 
			this.integerTrackbarControlIterationsCount.Location = new System.Drawing.Point(186, 106);
			this.integerTrackbarControlIterationsCount.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.integerTrackbarControlIterationsCount.MaximumSize = new System.Drawing.Size(15000, 31);
			this.integerTrackbarControlIterationsCount.MinimumSize = new System.Drawing.Size(105, 31);
			this.integerTrackbarControlIterationsCount.Name = "integerTrackbarControlIterationsCount";
			this.integerTrackbarControlIterationsCount.RangeMax = 2048;
			this.integerTrackbarControlIterationsCount.RangeMin = 1;
			this.integerTrackbarControlIterationsCount.Size = new System.Drawing.Size(357, 31);
			this.integerTrackbarControlIterationsCount.TabIndex = 7;
			this.integerTrackbarControlIterationsCount.Value = 1024;
			this.integerTrackbarControlIterationsCount.VisibleRangeMax = 2048;
			this.integerTrackbarControlIterationsCount.VisibleRangeMin = 1;
			// 
			// floatTrackbarControlPhi
			// 
			this.floatTrackbarControlPhi.Location = new System.Drawing.Point(189, 65);
			this.floatTrackbarControlPhi.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlPhi.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlPhi.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlPhi.Name = "floatTrackbarControlPhi";
			this.floatTrackbarControlPhi.RangeMax = 180F;
			this.floatTrackbarControlPhi.RangeMin = -180F;
			this.floatTrackbarControlPhi.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlPhi.TabIndex = 5;
			this.floatTrackbarControlPhi.Value = 0F;
			this.floatTrackbarControlPhi.VisibleRangeMax = 180F;
			this.floatTrackbarControlPhi.VisibleRangeMin = -180F;
			// 
			// floatTrackbarControlTheta
			// 
			this.floatTrackbarControlTheta.Location = new System.Drawing.Point(189, 24);
			this.floatTrackbarControlTheta.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlTheta.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlTheta.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlTheta.Name = "floatTrackbarControlTheta";
			this.floatTrackbarControlTheta.RangeMax = 89.9F;
			this.floatTrackbarControlTheta.RangeMin = 0F;
			this.floatTrackbarControlTheta.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlTheta.TabIndex = 5;
			this.floatTrackbarControlTheta.Value = 45F;
			this.floatTrackbarControlTheta.VisibleRangeMax = 89.9F;
			// 
			// floatTrackbarControlBeckmannSizeFactor
			// 
			this.floatTrackbarControlBeckmannSizeFactor.Location = new System.Drawing.Point(184, 69);
			this.floatTrackbarControlBeckmannSizeFactor.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlBeckmannSizeFactor.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlBeckmannSizeFactor.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlBeckmannSizeFactor.Name = "floatTrackbarControlBeckmannSizeFactor";
			this.floatTrackbarControlBeckmannSizeFactor.RangeMax = 10F;
			this.floatTrackbarControlBeckmannSizeFactor.RangeMin = 0F;
			this.floatTrackbarControlBeckmannSizeFactor.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlBeckmannSizeFactor.TabIndex = 2;
			this.floatTrackbarControlBeckmannSizeFactor.Value = 1F;
			this.floatTrackbarControlBeckmannSizeFactor.VisibleRangeMax = 2F;
			this.floatTrackbarControlBeckmannSizeFactor.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlBeckmannRoughness_ValueChanged);
			// 
			// floatTrackbarControlBeckmannRoughness
			// 
			this.floatTrackbarControlBeckmannRoughness.Location = new System.Drawing.Point(184, 29);
			this.floatTrackbarControlBeckmannRoughness.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlBeckmannRoughness.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlBeckmannRoughness.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlBeckmannRoughness.Name = "floatTrackbarControlBeckmannRoughness";
			this.floatTrackbarControlBeckmannRoughness.RangeMax = 1F;
			this.floatTrackbarControlBeckmannRoughness.RangeMin = 0F;
			this.floatTrackbarControlBeckmannRoughness.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlBeckmannRoughness.TabIndex = 2;
			this.floatTrackbarControlBeckmannRoughness.Value = 0.8F;
			this.floatTrackbarControlBeckmannRoughness.VisibleRangeMax = 1F;
			this.floatTrackbarControlBeckmannRoughness.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlBeckmannRoughness_ValueChanged);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(9, 17);
			this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(50, 20);
			this.label9.TabIndex = 3;
			this.label9.Text = "Angle";
			// 
			// floatTrackbarControlFitOversize
			// 
			this.floatTrackbarControlFitOversize.Location = new System.Drawing.Point(198, 400);
			this.floatTrackbarControlFitOversize.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlFitOversize.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlFitOversize.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlFitOversize.Name = "floatTrackbarControlFitOversize";
			this.floatTrackbarControlFitOversize.RangeMax = 2F;
			this.floatTrackbarControlFitOversize.RangeMin = 0F;
			this.floatTrackbarControlFitOversize.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlFitOversize.TabIndex = 5;
			this.floatTrackbarControlFitOversize.Value = 1.02F;
			this.floatTrackbarControlFitOversize.VisibleRangeMax = 1.1F;
			this.floatTrackbarControlFitOversize.VisibleRangeMin = 1F;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(8, 395);
			this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(168, 20);
			this.label10.TabIndex = 3;
			this.label10.Text = "Fitting Oversize Factor";
			// 
			// checkBoxTest
			// 
			this.checkBoxTest.AutoSize = true;
			this.checkBoxTest.Checked = true;
			this.checkBoxTest.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxTest.Location = new System.Drawing.Point(8, 366);
			this.checkBoxTest.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.checkBoxTest.Name = "checkBoxTest";
			this.checkBoxTest.Size = new System.Drawing.Size(295, 24);
			this.checkBoxTest.TabIndex = 9;
			this.checkBoxTest.Text = "Use Center of Mass for Better Fitting";
			this.checkBoxTest.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControlSurfaceAlbedo
			// 
			this.floatTrackbarControlSurfaceAlbedo.Location = new System.Drawing.Point(184, 109);
			this.floatTrackbarControlSurfaceAlbedo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlSurfaceAlbedo.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlSurfaceAlbedo.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlSurfaceAlbedo.Name = "floatTrackbarControlSurfaceAlbedo";
			this.floatTrackbarControlSurfaceAlbedo.RangeMax = 1F;
			this.floatTrackbarControlSurfaceAlbedo.RangeMin = 0F;
			this.floatTrackbarControlSurfaceAlbedo.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlSurfaceAlbedo.TabIndex = 2;
			this.floatTrackbarControlSurfaceAlbedo.Value = 1F;
			this.floatTrackbarControlSurfaceAlbedo.VisibleRangeMax = 1F;
			this.floatTrackbarControlSurfaceAlbedo.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlBeckmannRoughness_ValueChanged);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(9, 112);
			this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(59, 20);
			this.label11.TabIndex = 3;
			this.label11.Text = "Albedo";
			// 
			// groupBoxSurface
			// 
			this.groupBoxSurface.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxSurface.Controls.Add(this.floatTrackbarControlBeckmannRoughness);
			this.groupBoxSurface.Controls.Add(this.label1);
			this.groupBoxSurface.Controls.Add(this.floatTrackbarControlBeckmannSizeFactor);
			this.groupBoxSurface.Controls.Add(this.floatTrackbarControlSurfaceAlbedo);
			this.groupBoxSurface.Controls.Add(this.label6);
			this.groupBoxSurface.Controls.Add(this.label11);
			this.groupBoxSurface.Location = new System.Drawing.Point(4, 5);
			this.groupBoxSurface.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBoxSurface.Name = "groupBoxSurface";
			this.groupBoxSurface.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBoxSurface.Size = new System.Drawing.Size(579, 148);
			this.groupBoxSurface.TabIndex = 10;
			this.groupBoxSurface.TabStop = false;
			this.groupBoxSurface.Text = "Beckmann Surface Parameters";
			// 
			// buttonTestImage
			// 
			this.buttonTestImage.Location = new System.Drawing.Point(375, 79);
			this.buttonTestImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.buttonTestImage.Name = "buttonTestImage";
			this.buttonTestImage.Size = new System.Drawing.Size(112, 35);
			this.buttonTestImage.TabIndex = 6;
			this.buttonTestImage.Text = "Test Image";
			this.buttonTestImage.UseVisualStyleBackColor = true;
			this.buttonTestImage.Click += new System.EventHandler(this.buttonTestImage_Click);
			// 
			// panelDielectric
			// 
			this.panelDielectric.Controls.Add(this.floatTrackbarControlF0);
			this.panelDielectric.Controls.Add(this.label12);
			this.panelDielectric.Enabled = false;
			this.panelDielectric.Location = new System.Drawing.Point(1345, 220);
			this.panelDielectric.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panelDielectric.Name = "panelDielectric";
			this.panelDielectric.Size = new System.Drawing.Size(548, 35);
			this.panelDielectric.TabIndex = 5;
			// 
			// floatTrackbarControlF0
			// 
			this.floatTrackbarControlF0.Location = new System.Drawing.Point(176, 0);
			this.floatTrackbarControlF0.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlF0.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlF0.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlF0.Name = "floatTrackbarControlF0";
			this.floatTrackbarControlF0.RangeMax = 1F;
			this.floatTrackbarControlF0.RangeMin = 0F;
			this.floatTrackbarControlF0.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlF0.TabIndex = 2;
			this.floatTrackbarControlF0.Value = 1F;
			this.floatTrackbarControlF0.VisibleRangeMax = 1F;
			this.floatTrackbarControlF0.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlBeckmannRoughness_ValueChanged);
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(0, 0);
			this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(28, 20);
			this.label12.TabIndex = 3;
			this.label12.Text = "F0";
			// 
			// radioButtonDiffuse
			// 
			this.radioButtonDiffuse.AutoSize = true;
			this.radioButtonDiffuse.Location = new System.Drawing.Point(1579, 265);
			this.radioButtonDiffuse.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.radioButtonDiffuse.Name = "radioButtonDiffuse";
			this.radioButtonDiffuse.Size = new System.Drawing.Size(85, 24);
			this.radioButtonDiffuse.TabIndex = 4;
			this.radioButtonDiffuse.Text = "Diffuse";
			this.radioButtonDiffuse.UseVisualStyleBackColor = true;
			this.radioButtonDiffuse.CheckedChanged += new System.EventHandler(this.radioButtonSurfaceTypeChanged);
			// 
			// radioButtonDielectric
			// 
			this.radioButtonDielectric.AutoSize = true;
			this.radioButtonDielectric.Location = new System.Drawing.Point(1462, 265);
			this.radioButtonDielectric.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.radioButtonDielectric.Name = "radioButtonDielectric";
			this.radioButtonDielectric.Size = new System.Drawing.Size(99, 24);
			this.radioButtonDielectric.TabIndex = 4;
			this.radioButtonDielectric.Text = "Dielectric";
			this.radioButtonDielectric.UseVisualStyleBackColor = true;
			this.radioButtonDielectric.CheckedChanged += new System.EventHandler(this.radioButtonSurfaceTypeChanged);
			// 
			// radioButtonConductor
			// 
			this.radioButtonConductor.AutoSize = true;
			this.radioButtonConductor.Checked = true;
			this.radioButtonConductor.Location = new System.Drawing.Point(1345, 265);
			this.radioButtonConductor.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.radioButtonConductor.Name = "radioButtonConductor";
			this.radioButtonConductor.Size = new System.Drawing.Size(108, 24);
			this.radioButtonConductor.TabIndex = 4;
			this.radioButtonConductor.TabStop = true;
			this.radioButtonConductor.Text = "Conductor";
			this.radioButtonConductor.UseVisualStyleBackColor = true;
			this.radioButtonConductor.CheckedChanged += new System.EventHandler(this.radioButtonSurfaceTypeChanged);
			// 
			// groupBoxSimulation
			// 
			this.groupBoxSimulation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxSimulation.Controls.Add(this.buttonRayTrace);
			this.groupBoxSimulation.Controls.Add(this.floatTrackbarControlTheta);
			this.groupBoxSimulation.Controls.Add(this.label2);
			this.groupBoxSimulation.Controls.Add(this.integerTrackbarControlIterationsCount);
			this.groupBoxSimulation.Controls.Add(this.label4);
			this.groupBoxSimulation.Controls.Add(this.floatTrackbarControlPhi);
			this.groupBoxSimulation.Controls.Add(this.label3);
			this.groupBoxSimulation.Location = new System.Drawing.Point(1345, 376);
			this.groupBoxSimulation.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBoxSimulation.Name = "groupBoxSimulation";
			this.groupBoxSimulation.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBoxSimulation.Size = new System.Drawing.Size(579, 190);
			this.groupBoxSimulation.TabIndex = 11;
			this.groupBoxSimulation.TabStop = false;
			this.groupBoxSimulation.Text = "Simulation";
			// 
			// groupBoxAnalyticalLobe
			// 
			this.groupBoxAnalyticalLobe.Controls.Add(this.panel2);
			this.groupBoxAnalyticalLobe.Controls.Add(this.tabControlAnalyticalLobes);
			this.groupBoxAnalyticalLobe.Controls.Add(this.checkBoxShowAnalyticalLobe);
			this.groupBoxAnalyticalLobe.Controls.Add(this.checkBoxInitializeDirectionTowardCenterOfMass);
			this.groupBoxAnalyticalLobe.Controls.Add(this.checkBoxTest);
			this.groupBoxAnalyticalLobe.Controls.Add(this.floatTrackbarControlFitOversize);
			this.groupBoxAnalyticalLobe.Controls.Add(this.label10);
			this.groupBoxAnalyticalLobe.Location = new System.Drawing.Point(1345, 869);
			this.groupBoxAnalyticalLobe.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBoxAnalyticalLobe.Name = "groupBoxAnalyticalLobe";
			this.groupBoxAnalyticalLobe.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBoxAnalyticalLobe.Size = new System.Drawing.Size(579, 434);
			this.groupBoxAnalyticalLobe.TabIndex = 12;
			this.groupBoxAnalyticalLobe.TabStop = false;
			this.groupBoxAnalyticalLobe.Text = "Analytical Lobe";
			// 
			// tabControlAnalyticalLobes
			// 
			this.tabControlAnalyticalLobes.Controls.Add(this.tabPageReflectedLobe);
			this.tabControlAnalyticalLobes.Controls.Add(this.tabPageTransmittedLobe);
			this.tabControlAnalyticalLobes.Location = new System.Drawing.Point(10, 63);
			this.tabControlAnalyticalLobes.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tabControlAnalyticalLobes.Name = "tabControlAnalyticalLobes";
			this.tabControlAnalyticalLobes.SelectedIndex = 0;
			this.tabControlAnalyticalLobes.Size = new System.Drawing.Size(561, 297);
			this.tabControlAnalyticalLobes.TabIndex = 14;
			// 
			// tabPageReflectedLobe
			// 
			this.tabPageReflectedLobe.Controls.Add(this.label9);
			this.tabPageReflectedLobe.Controls.Add(this.floatTrackbarControlLobeScaleB);
			this.tabPageReflectedLobe.Controls.Add(this.floatTrackbarControlAnalyticalLobeTheta);
			this.tabPageReflectedLobe.Controls.Add(this.label8);
			this.tabPageReflectedLobe.Controls.Add(this.floatTrackbarControlLobeScaleT);
			this.tabPageReflectedLobe.Controls.Add(this.label20);
			this.tabPageReflectedLobe.Controls.Add(this.label7);
			this.tabPageReflectedLobe.Controls.Add(this.label13);
			this.tabPageReflectedLobe.Controls.Add(this.floatTrackbarControlLobeMaskingImportance);
			this.tabPageReflectedLobe.Controls.Add(this.floatTrackbarControlLobeScaleR);
			this.tabPageReflectedLobe.Controls.Add(this.label14);
			this.tabPageReflectedLobe.Controls.Add(this.floatTrackbarControlAnalyticalLobeRoughness);
			this.tabPageReflectedLobe.Location = new System.Drawing.Point(4, 29);
			this.tabPageReflectedLobe.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tabPageReflectedLobe.Name = "tabPageReflectedLobe";
			this.tabPageReflectedLobe.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tabPageReflectedLobe.Size = new System.Drawing.Size(553, 264);
			this.tabPageReflectedLobe.TabIndex = 0;
			this.tabPageReflectedLobe.Text = "Reflected";
			this.tabPageReflectedLobe.UseVisualStyleBackColor = true;
			// 
			// label20
			// 
			this.label20.AutoSize = true;
			this.label20.Location = new System.Drawing.Point(9, 217);
			this.label20.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(153, 20);
			this.label20.TabIndex = 3;
			this.label20.Text = "Masking Importance";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(9, 135);
			this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(99, 20);
			this.label13.TabIndex = 3;
			this.label13.Text = "Global Scale";
			// 
			// floatTrackbarControlLobeMaskingImportance
			// 
			this.floatTrackbarControlLobeMaskingImportance.Location = new System.Drawing.Point(184, 209);
			this.floatTrackbarControlLobeMaskingImportance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlLobeMaskingImportance.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlLobeMaskingImportance.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlLobeMaskingImportance.Name = "floatTrackbarControlLobeMaskingImportance";
			this.floatTrackbarControlLobeMaskingImportance.RangeMax = 1F;
			this.floatTrackbarControlLobeMaskingImportance.RangeMin = 0F;
			this.floatTrackbarControlLobeMaskingImportance.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlLobeMaskingImportance.TabIndex = 5;
			this.floatTrackbarControlLobeMaskingImportance.Value = 1F;
			this.floatTrackbarControlLobeMaskingImportance.VisibleRangeMax = 1F;
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Enabled = false;
			this.label14.Location = new System.Drawing.Point(9, 175);
			this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(126, 20);
			this.label14.TabIndex = 3;
			this.label14.Text = "Scale BiTangent";
			// 
			// tabPageTransmittedLobe
			// 
			this.tabPageTransmittedLobe.Controls.Add(this.label21);
			this.tabPageTransmittedLobe.Controls.Add(this.floatTrackbarControlLobeMaskingImportance_T);
			this.tabPageTransmittedLobe.Controls.Add(this.label15);
			this.tabPageTransmittedLobe.Controls.Add(this.floatTrackbarControlLobeScaleB_T);
			this.tabPageTransmittedLobe.Controls.Add(this.floatTrackbarControlAnalyticalLobeTheta_T);
			this.tabPageTransmittedLobe.Controls.Add(this.label16);
			this.tabPageTransmittedLobe.Controls.Add(this.floatTrackbarControlLobeScaleT_T);
			this.tabPageTransmittedLobe.Controls.Add(this.label17);
			this.tabPageTransmittedLobe.Controls.Add(this.label18);
			this.tabPageTransmittedLobe.Controls.Add(this.floatTrackbarControlLobeScaleR_T);
			this.tabPageTransmittedLobe.Controls.Add(this.label19);
			this.tabPageTransmittedLobe.Controls.Add(this.floatTrackbarControlAnalyticalLobeRoughness_T);
			this.tabPageTransmittedLobe.Location = new System.Drawing.Point(4, 29);
			this.tabPageTransmittedLobe.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tabPageTransmittedLobe.Name = "tabPageTransmittedLobe";
			this.tabPageTransmittedLobe.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tabPageTransmittedLobe.Size = new System.Drawing.Size(553, 264);
			this.tabPageTransmittedLobe.TabIndex = 1;
			this.tabPageTransmittedLobe.Text = "Transmitted";
			this.tabPageTransmittedLobe.UseVisualStyleBackColor = true;
			// 
			// label21
			// 
			this.label21.AutoSize = true;
			this.label21.Location = new System.Drawing.Point(9, 217);
			this.label21.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(153, 20);
			this.label21.TabIndex = 16;
			this.label21.Text = "Masking Importance";
			// 
			// floatTrackbarControlLobeMaskingImportance_T
			// 
			this.floatTrackbarControlLobeMaskingImportance_T.Location = new System.Drawing.Point(184, 209);
			this.floatTrackbarControlLobeMaskingImportance_T.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlLobeMaskingImportance_T.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlLobeMaskingImportance_T.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlLobeMaskingImportance_T.Name = "floatTrackbarControlLobeMaskingImportance_T";
			this.floatTrackbarControlLobeMaskingImportance_T.RangeMax = 1F;
			this.floatTrackbarControlLobeMaskingImportance_T.RangeMin = 0F;
			this.floatTrackbarControlLobeMaskingImportance_T.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlLobeMaskingImportance_T.TabIndex = 17;
			this.floatTrackbarControlLobeMaskingImportance_T.Value = 1F;
			this.floatTrackbarControlLobeMaskingImportance_T.VisibleRangeMax = 1F;
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(9, 17);
			this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(50, 20);
			this.label15.TabIndex = 7;
			this.label15.Text = "Angle";
			// 
			// floatTrackbarControlLobeScaleB_T
			// 
			this.floatTrackbarControlLobeScaleB_T.Enabled = false;
			this.floatTrackbarControlLobeScaleB_T.Location = new System.Drawing.Point(184, 169);
			this.floatTrackbarControlLobeScaleB_T.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlLobeScaleB_T.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlLobeScaleB_T.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlLobeScaleB_T.Name = "floatTrackbarControlLobeScaleB_T";
			this.floatTrackbarControlLobeScaleB_T.RangeMax = 10000F;
			this.floatTrackbarControlLobeScaleB_T.RangeMin = 0F;
			this.floatTrackbarControlLobeScaleB_T.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlLobeScaleB_T.TabIndex = 12;
			this.floatTrackbarControlLobeScaleB_T.Value = 1F;
			this.floatTrackbarControlLobeScaleB_T.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlAnalyticalLobeTheta_T
			// 
			this.floatTrackbarControlAnalyticalLobeTheta_T.Location = new System.Drawing.Point(184, 9);
			this.floatTrackbarControlAnalyticalLobeTheta_T.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlAnalyticalLobeTheta_T.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlAnalyticalLobeTheta_T.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlAnalyticalLobeTheta_T.Name = "floatTrackbarControlAnalyticalLobeTheta_T";
			this.floatTrackbarControlAnalyticalLobeTheta_T.RangeMax = 89.999F;
			this.floatTrackbarControlAnalyticalLobeTheta_T.RangeMin = 0F;
			this.floatTrackbarControlAnalyticalLobeTheta_T.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlAnalyticalLobeTheta_T.TabIndex = 13;
			this.floatTrackbarControlAnalyticalLobeTheta_T.Value = 0F;
			this.floatTrackbarControlAnalyticalLobeTheta_T.VisibleRangeMax = 89.999F;
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(9, 55);
			this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(91, 20);
			this.label16.TabIndex = 8;
			this.label16.Text = "Roughness";
			// 
			// floatTrackbarControlLobeScaleT_T
			// 
			this.floatTrackbarControlLobeScaleT_T.Location = new System.Drawing.Point(184, 129);
			this.floatTrackbarControlLobeScaleT_T.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlLobeScaleT_T.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlLobeScaleT_T.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlLobeScaleT_T.Name = "floatTrackbarControlLobeScaleT_T";
			this.floatTrackbarControlLobeScaleT_T.RangeMax = 10000F;
			this.floatTrackbarControlLobeScaleT_T.RangeMin = 0F;
			this.floatTrackbarControlLobeScaleT_T.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlLobeScaleT_T.TabIndex = 14;
			this.floatTrackbarControlLobeScaleT_T.Value = 1F;
			this.floatTrackbarControlLobeScaleT_T.VisibleRangeMax = 1F;
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(9, 97);
			this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(103, 20);
			this.label17.TabIndex = 9;
			this.label17.Text = "Scale Normal";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(9, 135);
			this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(99, 20);
			this.label18.TabIndex = 10;
			this.label18.Text = "Global Scale";
			// 
			// floatTrackbarControlLobeScaleR_T
			// 
			this.floatTrackbarControlLobeScaleR_T.Location = new System.Drawing.Point(184, 89);
			this.floatTrackbarControlLobeScaleR_T.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlLobeScaleR_T.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlLobeScaleR_T.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlLobeScaleR_T.Name = "floatTrackbarControlLobeScaleR_T";
			this.floatTrackbarControlLobeScaleR_T.RangeMax = 10000F;
			this.floatTrackbarControlLobeScaleR_T.RangeMin = 0F;
			this.floatTrackbarControlLobeScaleR_T.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlLobeScaleR_T.TabIndex = 15;
			this.floatTrackbarControlLobeScaleR_T.Value = 0.1666F;
			this.floatTrackbarControlLobeScaleR_T.VisibleRangeMax = 1F;
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Enabled = false;
			this.label19.Location = new System.Drawing.Point(9, 175);
			this.label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(126, 20);
			this.label19.TabIndex = 11;
			this.label19.Text = "Scale BiTangent";
			// 
			// floatTrackbarControlAnalyticalLobeRoughness_T
			// 
			this.floatTrackbarControlAnalyticalLobeRoughness_T.Location = new System.Drawing.Point(184, 49);
			this.floatTrackbarControlAnalyticalLobeRoughness_T.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.floatTrackbarControlAnalyticalLobeRoughness_T.MaximumSize = new System.Drawing.Size(15000, 31);
			this.floatTrackbarControlAnalyticalLobeRoughness_T.MinimumSize = new System.Drawing.Size(105, 31);
			this.floatTrackbarControlAnalyticalLobeRoughness_T.Name = "floatTrackbarControlAnalyticalLobeRoughness_T";
			this.floatTrackbarControlAnalyticalLobeRoughness_T.RangeMax = 1F;
			this.floatTrackbarControlAnalyticalLobeRoughness_T.RangeMin = 0F;
			this.floatTrackbarControlAnalyticalLobeRoughness_T.Size = new System.Drawing.Size(357, 31);
			this.floatTrackbarControlAnalyticalLobeRoughness_T.TabIndex = 6;
			this.floatTrackbarControlAnalyticalLobeRoughness_T.Value = 0.9444F;
			this.floatTrackbarControlAnalyticalLobeRoughness_T.VisibleRangeMax = 1F;
			// 
			// checkBoxInitializeDirectionTowardCenterOfMass
			// 
			this.checkBoxInitializeDirectionTowardCenterOfMass.AutoSize = true;
			this.checkBoxInitializeDirectionTowardCenterOfMass.Checked = true;
			this.checkBoxInitializeDirectionTowardCenterOfMass.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxInitializeDirectionTowardCenterOfMass.Location = new System.Drawing.Point(311, 370);
			this.checkBoxInitializeDirectionTowardCenterOfMass.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.checkBoxInitializeDirectionTowardCenterOfMass.Name = "checkBoxInitializeDirectionTowardCenterOfMass";
			this.checkBoxInitializeDirectionTowardCenterOfMass.Size = new System.Drawing.Size(258, 24);
			this.checkBoxInitializeDirectionTowardCenterOfMass.TabIndex = 9;
			this.checkBoxInitializeDirectionTowardCenterOfMass.Text = "Initial Dir. Toward Cent. of Mass";
			this.checkBoxInitializeDirectionTowardCenterOfMass.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.floatTrackbarControlLobeIntensity);
			this.groupBox1.Controls.Add(this.integerTrackbarControlScatteringOrder);
			this.groupBox1.Controls.Add(this.checkBoxCompensateScatteringFactor);
			this.groupBox1.Controls.Add(this.checkBoxShowXRay);
			this.groupBox1.Controls.Add(this.checkBoxShowWireframe);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.checkBoxShowLobe);
			this.groupBox1.Location = new System.Drawing.Point(1345, 715);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox1.Size = new System.Drawing.Size(579, 144);
			this.groupBox1.TabIndex = 13;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Simulated Lobe Display";
			// 
			// checkBoxCompensateScatteringFactor
			// 
			this.checkBoxCompensateScatteringFactor.AutoSize = true;
			this.checkBoxCompensateScatteringFactor.Checked = true;
			this.checkBoxCompensateScatteringFactor.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxCompensateScatteringFactor.Location = new System.Drawing.Point(212, 110);
			this.checkBoxCompensateScatteringFactor.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.checkBoxCompensateScatteringFactor.Name = "checkBoxCompensateScatteringFactor";
			this.checkBoxCompensateScatteringFactor.Size = new System.Drawing.Size(359, 24);
			this.checkBoxCompensateScatteringFactor.TabIndex = 9;
			this.checkBoxCompensateScatteringFactor.Text = "Use pow( 3, scattering ) Compensation Factor";
			this.checkBoxCompensateScatteringFactor.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowXRay
			// 
			this.checkBoxShowXRay.AutoSize = true;
			this.checkBoxShowXRay.Checked = true;
			this.checkBoxShowXRay.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowXRay.Location = new System.Drawing.Point(350, 0);
			this.checkBoxShowXRay.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.checkBoxShowXRay.Name = "checkBoxShowXRay";
			this.checkBoxShowXRay.Size = new System.Drawing.Size(123, 24);
			this.checkBoxShowXRay.TabIndex = 9;
			this.checkBoxShowXRay.Text = "Show X-Ray";
			this.checkBoxShowXRay.UseVisualStyleBackColor = true;
			// 
			// buttonAutomation
			// 
			this.buttonAutomation.Location = new System.Drawing.Point(1808, 1305);
			this.buttonAutomation.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.buttonAutomation.Name = "buttonAutomation";
			this.buttonAutomation.Size = new System.Drawing.Size(112, 35);
			this.buttonAutomation.TabIndex = 14;
			this.buttonAutomation.Text = "&Automation";
			this.buttonAutomation.UseVisualStyleBackColor = true;
			this.buttonAutomation.Click += new System.EventHandler(this.buttonAutomation_Click);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabBeckmann);
			this.tabControl1.Controls.Add(this.tabLoadTex);
			this.tabControl1.Location = new System.Drawing.Point(1341, 18);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(577, 194);
			this.tabControl1.TabIndex = 7;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// tabBeckmann
			// 
			this.tabBeckmann.Controls.Add(this.groupBoxSurface);
			this.tabBeckmann.Location = new System.Drawing.Point(4, 29);
			this.tabBeckmann.Name = "tabBeckmann";
			this.tabBeckmann.Padding = new System.Windows.Forms.Padding(3);
			this.tabBeckmann.Size = new System.Drawing.Size(569, 161);
			this.tabBeckmann.TabIndex = 0;
			this.tabBeckmann.Text = "Beckmann";
			this.tabBeckmann.UseVisualStyleBackColor = true;
			// 
			// tabLoadTex
			// 
			this.tabLoadTex.Controls.Add(this.buttonTestImage);
			this.tabLoadTex.Controls.Add(this.Scale);
			this.tabLoadTex.Controls.Add(this.floatTrackbarScale);
			this.tabLoadTex.Location = new System.Drawing.Point(4, 29);
			this.tabLoadTex.Name = "tabLoadTex";
			this.tabLoadTex.Padding = new System.Windows.Forms.Padding(3);
			this.tabLoadTex.Size = new System.Drawing.Size(569, 242);
			this.tabLoadTex.TabIndex = 1;
			this.tabLoadTex.Text = "LoadTex";
			this.tabLoadTex.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarScale
			// 
			this.floatTrackbarScale.Location = new System.Drawing.Point(189, 26);
			this.floatTrackbarScale.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarScale.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarScale.Name = "floatTrackbarScale";
			this.floatTrackbarScale.RangeMax = 5F;
			this.floatTrackbarScale.RangeMin = 9.999999E-39F;
			this.floatTrackbarScale.Size = new System.Drawing.Size(354, 20);
			this.floatTrackbarScale.TabIndex = 0;
			this.floatTrackbarScale.Value = 1F;
			this.floatTrackbarScale.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControl1_ValueChanged);
			// 
			// Scale
			// 
			this.Scale.AutoSize = true;
			this.Scale.Location = new System.Drawing.Point(23, 26);
			this.Scale.Name = "Scale";
			this.Scale.Size = new System.Drawing.Size(49, 20);
			this.Scale.TabIndex = 1;
			this.Scale.Text = "Scale";
			this.Scale.Click += new System.EventHandler(this.label22_Click);
			// 
			// panelOutput
			// 
			this.panelOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelOutput.Location = new System.Drawing.Point(18, 18);
			this.panelOutput.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1314, 1285);
			this.panelOutput.TabIndex = 0;
			// 
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1930, 1360);
			this.Controls.Add(this.panelDielectric);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.radioButtonConductor);
			this.Controls.Add(this.radioButtonDielectric);
			this.Controls.Add(this.radioButtonDiffuse);
			this.Controls.Add(this.buttonAutomation);
			this.Controls.Add(this.groupBoxAnalyticalLobe);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.buttonFit);
			this.Controls.Add(this.groupBoxSimulation);
			this.Controls.Add(this.groupBoxDisplay);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput);
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "TestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Multiple-Scattering BSDF Test";
			this.groupBoxDisplay.ResumeLayout(false);
			this.groupBoxDisplay.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.groupBoxSurface.ResumeLayout(false);
			this.groupBoxSurface.PerformLayout();
			this.panelDielectric.ResumeLayout(false);
			this.panelDielectric.PerformLayout();
			this.groupBoxSimulation.ResumeLayout(false);
			this.groupBoxSimulation.PerformLayout();
			this.groupBoxAnalyticalLobe.ResumeLayout(false);
			this.groupBoxAnalyticalLobe.PerformLayout();
			this.tabControlAnalyticalLobes.ResumeLayout(false);
			this.tabPageReflectedLobe.ResumeLayout(false);
			this.tabPageReflectedLobe.PerformLayout();
			this.tabPageTransmittedLobe.ResumeLayout(false);
			this.tabPageTransmittedLobe.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.tabControl1.ResumeLayout(false);
			this.tabBeckmann.ResumeLayout(false);
			this.tabLoadTex.ResumeLayout(false);
			this.tabLoadTex.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput3D panelOutput;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button buttonReload;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlBeckmannRoughness;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton checkBoxShowNormals;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlTheta;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonRayTrace;
		private System.Windows.Forms.RadioButton checkBoxShowTransmittedDirectionsHistogram;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlScatteringOrder;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlIterationsCount;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlPhi;
		private System.Windows.Forms.RadioButton checkBoxShowReflectedDirectionsHistogram;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.RadioButton radioButtonShowHeights;
		private System.Windows.Forms.GroupBox groupBoxDisplay;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlBeckmannSizeFactor;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.CheckBox checkBoxShowLobe;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLobeIntensity;
		private System.Windows.Forms.RadioButton radioButtonHideSurface;
		private System.Windows.Forms.CheckBox checkBoxShowWireframe;
		private System.Windows.Forms.CheckBox checkBoxShowAnalyticalLobe;
		private System.Windows.Forms.Label label7;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLobeScaleT;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLobeScaleR;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.RadioButton radioButtonAnalyticalPhong;
		private System.Windows.Forms.RadioButton radioButtonAnalyticalGGX;
		private System.Windows.Forms.RadioButton radioButtonAnalyticalBeckmann;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLobeScaleB;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlAnalyticalLobeTheta;
		private System.Windows.Forms.Label label8;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlAnalyticalLobeRoughness;
		private System.Windows.Forms.Button buttonFit;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlFitOversize;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.CheckBox checkBoxTest;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSurfaceAlbedo;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.GroupBox groupBoxSurface;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlF0;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.GroupBox groupBoxSimulation;
		private System.Windows.Forms.GroupBox groupBoxAnalyticalLobe;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Panel panelDielectric;
		private System.Windows.Forms.RadioButton radioButtonConductor;
		private System.Windows.Forms.RadioButton radioButtonDielectric;
		private System.Windows.Forms.RadioButton radioButtonDiffuse;
		private System.Windows.Forms.TabControl tabControlAnalyticalLobes;
		private System.Windows.Forms.TabPage tabPageReflectedLobe;
		private System.Windows.Forms.TabPage tabPageTransmittedLobe;
		private System.Windows.Forms.Label label15;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLobeScaleB_T;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlAnalyticalLobeTheta_T;
		private System.Windows.Forms.Label label16;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLobeScaleT_T;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label18;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLobeScaleR_T;
		private System.Windows.Forms.Label label19;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlAnalyticalLobeRoughness_T;
		private System.Windows.Forms.CheckBox checkBoxCompensateScatteringFactor;
		private System.Windows.Forms.CheckBox checkBoxShowXRay;
		private System.Windows.Forms.CheckBox checkBoxInitializeDirectionTowardCenterOfMass;
		private System.Windows.Forms.Label label20;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLobeMaskingImportance;
		private System.Windows.Forms.Label label21;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLobeMaskingImportance_T;
		private System.Windows.Forms.Button buttonAutomation;
		private System.Windows.Forms.Button buttonTestImage;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabBeckmann;
		private System.Windows.Forms.TabPage tabLoadTex;
		private System.Windows.Forms.Label Scale;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarScale;
		private System.Windows.Forms.BindingSource bindingSource1;
	}
}

