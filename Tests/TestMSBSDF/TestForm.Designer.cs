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
			this.checkBoxUseCenterOfMassForBetterFitting = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlSurfaceAlbedo = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label11 = new System.Windows.Forms.Label();
			this.groupBoxSurface = new System.Windows.Forms.GroupBox();
			this.buttonTestImage = new System.Windows.Forms.Button();
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
			this.checkBoxShowDiffuseModel = new System.Windows.Forms.CheckBox();
			this.checkBoxInitializeDirectionTowardCenterOfMass = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkBoxCompensateScatteringFactor = new System.Windows.Forms.CheckBox();
			this.checkBoxShowXRay = new System.Windows.Forms.CheckBox();
			this.buttonAutomation = new System.Windows.Forms.Button();
			this.radioButtonAnalyticalPhongAnisotropic = new System.Windows.Forms.RadioButton();
			this.panelOutput = new TestMSBSDF.PanelOutput3D(this.components);
			this.groupBoxDisplay.SuspendLayout();
			this.panel2.SuspendLayout();
			this.groupBoxSurface.SuspendLayout();
			this.groupBoxSimulation.SuspendLayout();
			this.groupBoxAnalyticalLobe.SuspendLayout();
			this.tabControlAnalyticalLobes.SuspendLayout();
			this.tabPageReflectedLobe.SuspendLayout();
			this.tabPageTransmittedLobe.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// timer1
			// 
			this.timer1.Interval = 10;
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(772, 853);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(116, 23);
			this.buttonReload.TabIndex = 1;
			this.buttonReload.Text = "Reload Shaders";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(61, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Roughness";
			// 
			// checkBoxShowNormals
			// 
			this.checkBoxShowNormals.AutoSize = true;
			this.checkBoxShowNormals.Location = new System.Drawing.Point(9, 64);
			this.checkBoxShowNormals.Name = "checkBoxShowNormals";
			this.checkBoxShowNormals.Size = new System.Drawing.Size(93, 17);
			this.checkBoxShowNormals.TabIndex = 4;
			this.checkBoxShowNormals.Text = "Show Normals";
			this.checkBoxShowNormals.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 25);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(111, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Incoming Angle Theta";
			// 
			// buttonRayTrace
			// 
			this.buttonRayTrace.Location = new System.Drawing.Point(142, 101);
			this.buttonRayTrace.Name = "buttonRayTrace";
			this.buttonRayTrace.Size = new System.Drawing.Size(75, 23);
			this.buttonRayTrace.TabIndex = 6;
			this.buttonRayTrace.Text = "Ray Trace";
			this.buttonRayTrace.UseVisualStyleBackColor = true;
			this.buttonRayTrace.Click += new System.EventHandler(this.buttonRayTrace_Click);
			// 
			// checkBoxShowTransmittedDirectionsHistogram
			// 
			this.checkBoxShowTransmittedDirectionsHistogram.AutoSize = true;
			this.checkBoxShowTransmittedDirectionsHistogram.Location = new System.Drawing.Point(163, 41);
			this.checkBoxShowTransmittedDirectionsHistogram.Name = "checkBoxShowTransmittedDirectionsHistogram";
			this.checkBoxShowTransmittedDirectionsHistogram.Size = new System.Drawing.Size(210, 17);
			this.checkBoxShowTransmittedDirectionsHistogram.TabIndex = 4;
			this.checkBoxShowTransmittedDirectionsHistogram.Text = "Show Transmitted Directions Histogram";
			this.checkBoxShowTransmittedDirectionsHistogram.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 77);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(81, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Iterations Count";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 51);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(98, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Incoming Angle Phi";
			// 
			// checkBoxShowReflectedDirectionsHistogram
			// 
			this.checkBoxShowReflectedDirectionsHistogram.AutoSize = true;
			this.checkBoxShowReflectedDirectionsHistogram.Location = new System.Drawing.Point(163, 19);
			this.checkBoxShowReflectedDirectionsHistogram.Name = "checkBoxShowReflectedDirectionsHistogram";
			this.checkBoxShowReflectedDirectionsHistogram.Size = new System.Drawing.Size(201, 17);
			this.checkBoxShowReflectedDirectionsHistogram.TabIndex = 4;
			this.checkBoxShowReflectedDirectionsHistogram.Text = "Show Reflected Directions Histogram";
			this.checkBoxShowReflectedDirectionsHistogram.UseVisualStyleBackColor = true;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 53);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(106, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "Lobe Intensity Factor";
			// 
			// radioButtonShowHeights
			// 
			this.radioButtonShowHeights.AutoSize = true;
			this.radioButtonShowHeights.Checked = true;
			this.radioButtonShowHeights.Location = new System.Drawing.Point(9, 41);
			this.radioButtonShowHeights.Name = "radioButtonShowHeights";
			this.radioButtonShowHeights.Size = new System.Drawing.Size(91, 17);
			this.radioButtonShowHeights.TabIndex = 4;
			this.radioButtonShowHeights.TabStop = true;
			this.radioButtonShowHeights.Text = "Show Heights";
			this.radioButtonShowHeights.UseVisualStyleBackColor = true;
			// 
			// radioButtonHideSurface
			// 
			this.radioButtonHideSurface.AutoSize = true;
			this.radioButtonHideSurface.Location = new System.Drawing.Point(9, 19);
			this.radioButtonHideSurface.Name = "radioButtonHideSurface";
			this.radioButtonHideSurface.Size = new System.Drawing.Size(87, 17);
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
			this.groupBoxDisplay.Location = new System.Drawing.Point(894, 289);
			this.groupBoxDisplay.Name = "groupBoxDisplay";
			this.groupBoxDisplay.Size = new System.Drawing.Size(386, 94);
			this.groupBoxDisplay.TabIndex = 9;
			this.groupBoxDisplay.TabStop = false;
			this.groupBoxDisplay.Text = "Surface Display";
			// 
			// buttonFit
			// 
			this.buttonFit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonFit.Location = new System.Drawing.Point(1048, 847);
			this.buttonFit.Name = "buttonFit";
			this.buttonFit.Size = new System.Drawing.Size(97, 25);
			this.buttonFit.TabIndex = 11;
			this.buttonFit.Text = "&FIT";
			this.buttonFit.UseVisualStyleBackColor = true;
			this.buttonFit.Click += new System.EventHandler(this.buttonFit_Click);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.radioButtonAnalyticalPhong);
			this.panel2.Controls.Add(this.radioButtonAnalyticalPhongAnisotropic);
			this.panel2.Controls.Add(this.radioButtonAnalyticalGGX);
			this.panel2.Controls.Add(this.radioButtonAnalyticalBeckmann);
			this.panel2.Location = new System.Drawing.Point(68, 16);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(292, 22);
			this.panel2.TabIndex = 10;
			// 
			// radioButtonAnalyticalPhong
			// 
			this.radioButtonAnalyticalPhong.AutoSize = true;
			this.radioButtonAnalyticalPhong.Checked = true;
			this.radioButtonAnalyticalPhong.Location = new System.Drawing.Point(140, 2);
			this.radioButtonAnalyticalPhong.Name = "radioButtonAnalyticalPhong";
			this.radioButtonAnalyticalPhong.Size = new System.Drawing.Size(56, 17);
			this.radioButtonAnalyticalPhong.TabIndex = 0;
			this.radioButtonAnalyticalPhong.TabStop = true;
			this.radioButtonAnalyticalPhong.Text = "Phong";
			this.radioButtonAnalyticalPhong.UseVisualStyleBackColor = true;
			// 
			// radioButtonAnalyticalGGX
			// 
			this.radioButtonAnalyticalGGX.AutoSize = true;
			this.radioButtonAnalyticalGGX.Location = new System.Drawing.Point(86, 2);
			this.radioButtonAnalyticalGGX.Name = "radioButtonAnalyticalGGX";
			this.radioButtonAnalyticalGGX.Size = new System.Drawing.Size(48, 17);
			this.radioButtonAnalyticalGGX.TabIndex = 0;
			this.radioButtonAnalyticalGGX.Text = "GGX";
			this.radioButtonAnalyticalGGX.UseVisualStyleBackColor = true;
			// 
			// radioButtonAnalyticalBeckmann
			// 
			this.radioButtonAnalyticalBeckmann.AutoSize = true;
			this.radioButtonAnalyticalBeckmann.Location = new System.Drawing.Point(9, 2);
			this.radioButtonAnalyticalBeckmann.Name = "radioButtonAnalyticalBeckmann";
			this.radioButtonAnalyticalBeckmann.Size = new System.Drawing.Size(76, 17);
			this.radioButtonAnalyticalBeckmann.TabIndex = 0;
			this.radioButtonAnalyticalBeckmann.Text = "Beckmann";
			this.radioButtonAnalyticalBeckmann.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowAnalyticalLobe
			// 
			this.checkBoxShowAnalyticalLobe.AutoSize = true;
			this.checkBoxShowAnalyticalLobe.Checked = true;
			this.checkBoxShowAnalyticalLobe.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowAnalyticalLobe.Location = new System.Drawing.Point(9, 19);
			this.checkBoxShowAnalyticalLobe.Name = "checkBoxShowAnalyticalLobe";
			this.checkBoxShowAnalyticalLobe.Size = new System.Drawing.Size(56, 17);
			this.checkBoxShowAnalyticalLobe.TabIndex = 9;
			this.checkBoxShowAnalyticalLobe.Text = "Show ";
			this.checkBoxShowAnalyticalLobe.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowWireframe
			// 
			this.checkBoxShowWireframe.AutoSize = true;
			this.checkBoxShowWireframe.Checked = true;
			this.checkBoxShowWireframe.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowWireframe.Location = new System.Drawing.Point(123, 0);
			this.checkBoxShowWireframe.Name = "checkBoxShowWireframe";
			this.checkBoxShowWireframe.Size = new System.Drawing.Size(104, 17);
			this.checkBoxShowWireframe.TabIndex = 9;
			this.checkBoxShowWireframe.Text = "Show Wireframe";
			this.checkBoxShowWireframe.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowLobe
			// 
			this.checkBoxShowLobe.AutoSize = true;
			this.checkBoxShowLobe.Checked = true;
			this.checkBoxShowLobe.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowLobe.Location = new System.Drawing.Point(9, 25);
			this.checkBoxShowLobe.Name = "checkBoxShowLobe";
			this.checkBoxShowLobe.Size = new System.Drawing.Size(133, 17);
			this.checkBoxShowLobe.TabIndex = 9;
			this.checkBoxShowLobe.Text = "Show Scattering Order";
			this.checkBoxShowLobe.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControlAnalyticalLobeTheta
			// 
			this.floatTrackbarControlAnalyticalLobeTheta.Location = new System.Drawing.Point(123, 6);
			this.floatTrackbarControlAnalyticalLobeTheta.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlAnalyticalLobeTheta.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlAnalyticalLobeTheta.Name = "floatTrackbarControlAnalyticalLobeTheta";
			this.floatTrackbarControlAnalyticalLobeTheta.RangeMax = 89.999F;
			this.floatTrackbarControlAnalyticalLobeTheta.RangeMin = -89.999F;
			this.floatTrackbarControlAnalyticalLobeTheta.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlAnalyticalLobeTheta.TabIndex = 5;
			this.floatTrackbarControlAnalyticalLobeTheta.Value = 0F;
			this.floatTrackbarControlAnalyticalLobeTheta.VisibleRangeMax = 89.999F;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(6, 36);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(61, 13);
			this.label8.TabIndex = 3;
			this.label8.Text = "Roughness";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(6, 89);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(53, 13);
			this.label7.TabIndex = 3;
			this.label7.Text = "Flattening";
			// 
			// floatTrackbarControlLobeScaleB
			// 
			this.floatTrackbarControlLobeScaleB.Enabled = false;
			this.floatTrackbarControlLobeScaleB.Location = new System.Drawing.Point(123, 136);
			this.floatTrackbarControlLobeScaleB.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLobeScaleB.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLobeScaleB.Name = "floatTrackbarControlLobeScaleB";
			this.floatTrackbarControlLobeScaleB.RangeMax = 10000F;
			this.floatTrackbarControlLobeScaleB.RangeMin = 0F;
			this.floatTrackbarControlLobeScaleB.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlLobeScaleB.TabIndex = 5;
			this.floatTrackbarControlLobeScaleB.Value = 1F;
			this.floatTrackbarControlLobeScaleB.Visible = false;
			this.floatTrackbarControlLobeScaleB.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlLobeScaleT
			// 
			this.floatTrackbarControlLobeScaleT.Location = new System.Drawing.Point(123, 58);
			this.floatTrackbarControlLobeScaleT.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLobeScaleT.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLobeScaleT.Name = "floatTrackbarControlLobeScaleT";
			this.floatTrackbarControlLobeScaleT.RangeMax = 10000F;
			this.floatTrackbarControlLobeScaleT.RangeMin = 0F;
			this.floatTrackbarControlLobeScaleT.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlLobeScaleT.TabIndex = 5;
			this.floatTrackbarControlLobeScaleT.Value = 1F;
			this.floatTrackbarControlLobeScaleT.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlAnalyticalLobeRoughness
			// 
			this.floatTrackbarControlAnalyticalLobeRoughness.Location = new System.Drawing.Point(123, 32);
			this.floatTrackbarControlAnalyticalLobeRoughness.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlAnalyticalLobeRoughness.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlAnalyticalLobeRoughness.Name = "floatTrackbarControlAnalyticalLobeRoughness";
			this.floatTrackbarControlAnalyticalLobeRoughness.RangeMax = 1F;
			this.floatTrackbarControlAnalyticalLobeRoughness.RangeMin = 0F;
			this.floatTrackbarControlAnalyticalLobeRoughness.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlAnalyticalLobeRoughness.TabIndex = 2;
			this.floatTrackbarControlAnalyticalLobeRoughness.Value = 0.9444F;
			this.floatTrackbarControlAnalyticalLobeRoughness.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlLobeScaleR
			// 
			this.floatTrackbarControlLobeScaleR.Location = new System.Drawing.Point(123, 84);
			this.floatTrackbarControlLobeScaleR.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLobeScaleR.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLobeScaleR.Name = "floatTrackbarControlLobeScaleR";
			this.floatTrackbarControlLobeScaleR.RangeMax = 10000F;
			this.floatTrackbarControlLobeScaleR.RangeMin = 0F;
			this.floatTrackbarControlLobeScaleR.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlLobeScaleR.TabIndex = 5;
			this.floatTrackbarControlLobeScaleR.Value = 1F;
			this.floatTrackbarControlLobeScaleR.VisibleRangeMax = 2F;
			// 
			// floatTrackbarControlLobeIntensity
			// 
			this.floatTrackbarControlLobeIntensity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlLobeIntensity.Location = new System.Drawing.Point(142, 48);
			this.floatTrackbarControlLobeIntensity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLobeIntensity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLobeIntensity.Name = "floatTrackbarControlLobeIntensity";
			this.floatTrackbarControlLobeIntensity.RangeMax = 10000F;
			this.floatTrackbarControlLobeIntensity.RangeMin = 0F;
			this.floatTrackbarControlLobeIntensity.Size = new System.Drawing.Size(233, 20);
			this.floatTrackbarControlLobeIntensity.TabIndex = 5;
			this.floatTrackbarControlLobeIntensity.Value = 1F;
			this.floatTrackbarControlLobeIntensity.VisibleRangeMax = 2F;
			// 
			// integerTrackbarControlScatteringOrder
			// 
			this.integerTrackbarControlScatteringOrder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.integerTrackbarControlScatteringOrder.Location = new System.Drawing.Point(142, 24);
			this.integerTrackbarControlScatteringOrder.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlScatteringOrder.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlScatteringOrder.Name = "integerTrackbarControlScatteringOrder";
			this.integerTrackbarControlScatteringOrder.RangeMax = 4;
			this.integerTrackbarControlScatteringOrder.RangeMin = 1;
			this.integerTrackbarControlScatteringOrder.Size = new System.Drawing.Size(233, 20);
			this.integerTrackbarControlScatteringOrder.TabIndex = 7;
			this.integerTrackbarControlScatteringOrder.Value = 2;
			this.integerTrackbarControlScatteringOrder.VisibleRangeMax = 4;
			this.integerTrackbarControlScatteringOrder.VisibleRangeMin = 1;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 48);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(60, 13);
			this.label6.TabIndex = 3;
			this.label6.Text = "Size Factor";
			// 
			// integerTrackbarControlIterationsCount
			// 
			this.integerTrackbarControlIterationsCount.Location = new System.Drawing.Point(123, 75);
			this.integerTrackbarControlIterationsCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlIterationsCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlIterationsCount.Name = "integerTrackbarControlIterationsCount";
			this.integerTrackbarControlIterationsCount.RangeMax = 2048;
			this.integerTrackbarControlIterationsCount.RangeMin = 1;
			this.integerTrackbarControlIterationsCount.Size = new System.Drawing.Size(238, 20);
			this.integerTrackbarControlIterationsCount.TabIndex = 7;
			this.integerTrackbarControlIterationsCount.Value = 1024;
			this.integerTrackbarControlIterationsCount.VisibleRangeMax = 2048;
			this.integerTrackbarControlIterationsCount.VisibleRangeMin = 1;
			// 
			// floatTrackbarControlPhi
			// 
			this.floatTrackbarControlPhi.Location = new System.Drawing.Point(123, 49);
			this.floatTrackbarControlPhi.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlPhi.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlPhi.Name = "floatTrackbarControlPhi";
			this.floatTrackbarControlPhi.RangeMax = 180F;
			this.floatTrackbarControlPhi.RangeMin = -180F;
			this.floatTrackbarControlPhi.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlPhi.TabIndex = 5;
			this.floatTrackbarControlPhi.Value = 0F;
			this.floatTrackbarControlPhi.VisibleRangeMax = 180F;
			this.floatTrackbarControlPhi.VisibleRangeMin = -180F;
			// 
			// floatTrackbarControlTheta
			// 
			this.floatTrackbarControlTheta.Location = new System.Drawing.Point(123, 23);
			this.floatTrackbarControlTheta.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlTheta.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlTheta.Name = "floatTrackbarControlTheta";
			this.floatTrackbarControlTheta.RangeMax = 89.9F;
			this.floatTrackbarControlTheta.RangeMin = 0F;
			this.floatTrackbarControlTheta.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlTheta.TabIndex = 5;
			this.floatTrackbarControlTheta.Value = 45F;
			this.floatTrackbarControlTheta.VisibleRangeMax = 89.9F;
			// 
			// floatTrackbarControlBeckmannSizeFactor
			// 
			this.floatTrackbarControlBeckmannSizeFactor.Location = new System.Drawing.Point(123, 45);
			this.floatTrackbarControlBeckmannSizeFactor.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBeckmannSizeFactor.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBeckmannSizeFactor.Name = "floatTrackbarControlBeckmannSizeFactor";
			this.floatTrackbarControlBeckmannSizeFactor.RangeMax = 10F;
			this.floatTrackbarControlBeckmannSizeFactor.RangeMin = 0F;
			this.floatTrackbarControlBeckmannSizeFactor.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlBeckmannSizeFactor.TabIndex = 2;
			this.floatTrackbarControlBeckmannSizeFactor.Value = 1F;
			this.floatTrackbarControlBeckmannSizeFactor.VisibleRangeMax = 2F;
			this.floatTrackbarControlBeckmannSizeFactor.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlBeckmannRoughness_ValueChanged);
			// 
			// floatTrackbarControlBeckmannRoughness
			// 
			this.floatTrackbarControlBeckmannRoughness.Location = new System.Drawing.Point(123, 19);
			this.floatTrackbarControlBeckmannRoughness.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBeckmannRoughness.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBeckmannRoughness.Name = "floatTrackbarControlBeckmannRoughness";
			this.floatTrackbarControlBeckmannRoughness.RangeMax = 1F;
			this.floatTrackbarControlBeckmannRoughness.RangeMin = 0F;
			this.floatTrackbarControlBeckmannRoughness.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlBeckmannRoughness.TabIndex = 2;
			this.floatTrackbarControlBeckmannRoughness.Value = 0.8F;
			this.floatTrackbarControlBeckmannRoughness.VisibleRangeMax = 1F;
			this.floatTrackbarControlBeckmannRoughness.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlBeckmannRoughness_ValueChanged);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(6, 11);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(34, 13);
			this.label9.TabIndex = 3;
			this.label9.Text = "Angle";
			// 
			// floatTrackbarControlFitOversize
			// 
			this.floatTrackbarControlFitOversize.Location = new System.Drawing.Point(122, 274);
			this.floatTrackbarControlFitOversize.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlFitOversize.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlFitOversize.Name = "floatTrackbarControlFitOversize";
			this.floatTrackbarControlFitOversize.RangeMax = 2F;
			this.floatTrackbarControlFitOversize.RangeMin = 0F;
			this.floatTrackbarControlFitOversize.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlFitOversize.TabIndex = 5;
			this.floatTrackbarControlFitOversize.Value = 1.02F;
			this.floatTrackbarControlFitOversize.VisibleRangeMax = 1.1F;
			this.floatTrackbarControlFitOversize.VisibleRangeMin = 1F;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(8, 278);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(112, 13);
			this.label10.TabIndex = 3;
			this.label10.Text = "Fitting Oversize Factor";
			// 
			// checkBoxUseCenterOfMassForBetterFitting
			// 
			this.checkBoxUseCenterOfMassForBetterFitting.AutoSize = true;
			this.checkBoxUseCenterOfMassForBetterFitting.Checked = true;
			this.checkBoxUseCenterOfMassForBetterFitting.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxUseCenterOfMassForBetterFitting.Location = new System.Drawing.Point(8, 251);
			this.checkBoxUseCenterOfMassForBetterFitting.Name = "checkBoxUseCenterOfMassForBetterFitting";
			this.checkBoxUseCenterOfMassForBetterFitting.Size = new System.Drawing.Size(196, 17);
			this.checkBoxUseCenterOfMassForBetterFitting.TabIndex = 9;
			this.checkBoxUseCenterOfMassForBetterFitting.Text = "Use Center of Mass for Better Fitting";
			this.checkBoxUseCenterOfMassForBetterFitting.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControlSurfaceAlbedo
			// 
			this.floatTrackbarControlSurfaceAlbedo.Location = new System.Drawing.Point(123, 71);
			this.floatTrackbarControlSurfaceAlbedo.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSurfaceAlbedo.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSurfaceAlbedo.Name = "floatTrackbarControlSurfaceAlbedo";
			this.floatTrackbarControlSurfaceAlbedo.RangeMax = 1F;
			this.floatTrackbarControlSurfaceAlbedo.RangeMin = 0F;
			this.floatTrackbarControlSurfaceAlbedo.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlSurfaceAlbedo.TabIndex = 2;
			this.floatTrackbarControlSurfaceAlbedo.Value = 1F;
			this.floatTrackbarControlSurfaceAlbedo.VisibleRangeMax = 1F;
			this.floatTrackbarControlSurfaceAlbedo.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlBeckmannRoughness_ValueChanged);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(6, 73);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(63, 13);
			this.label11.TabIndex = 3;
			this.label11.Text = "Albedo / F0";
			// 
			// groupBoxSurface
			// 
			this.groupBoxSurface.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxSurface.Controls.Add(this.buttonTestImage);
			this.groupBoxSurface.Controls.Add(this.radioButtonDiffuse);
			this.groupBoxSurface.Controls.Add(this.radioButtonDielectric);
			this.groupBoxSurface.Controls.Add(this.radioButtonConductor);
			this.groupBoxSurface.Controls.Add(this.floatTrackbarControlBeckmannRoughness);
			this.groupBoxSurface.Controls.Add(this.label1);
			this.groupBoxSurface.Controls.Add(this.floatTrackbarControlBeckmannSizeFactor);
			this.groupBoxSurface.Controls.Add(this.floatTrackbarControlSurfaceAlbedo);
			this.groupBoxSurface.Controls.Add(this.label6);
			this.groupBoxSurface.Controls.Add(this.label11);
			this.groupBoxSurface.Location = new System.Drawing.Point(894, 12);
			this.groupBoxSurface.Name = "groupBoxSurface";
			this.groupBoxSurface.Size = new System.Drawing.Size(386, 128);
			this.groupBoxSurface.TabIndex = 10;
			this.groupBoxSurface.TabStop = false;
			this.groupBoxSurface.Text = "Beckmann Surface Parameters";
			// 
			// buttonTestImage
			// 
			this.buttonTestImage.Location = new System.Drawing.Point(298, 93);
			this.buttonTestImage.Name = "buttonTestImage";
			this.buttonTestImage.Size = new System.Drawing.Size(75, 23);
			this.buttonTestImage.TabIndex = 6;
			this.buttonTestImage.Text = "Test Image";
			this.buttonTestImage.UseVisualStyleBackColor = true;
			this.buttonTestImage.Click += new System.EventHandler(this.buttonTestImage_Click);
			// 
			// radioButtonDiffuse
			// 
			this.radioButtonDiffuse.AutoSize = true;
			this.radioButtonDiffuse.Location = new System.Drawing.Point(163, 97);
			this.radioButtonDiffuse.Name = "radioButtonDiffuse";
			this.radioButtonDiffuse.Size = new System.Drawing.Size(58, 17);
			this.radioButtonDiffuse.TabIndex = 4;
			this.radioButtonDiffuse.Text = "Diffuse";
			this.radioButtonDiffuse.UseVisualStyleBackColor = true;
			// 
			// radioButtonDielectric
			// 
			this.radioButtonDielectric.AutoSize = true;
			this.radioButtonDielectric.Location = new System.Drawing.Point(89, 97);
			this.radioButtonDielectric.Name = "radioButtonDielectric";
			this.radioButtonDielectric.Size = new System.Drawing.Size(69, 17);
			this.radioButtonDielectric.TabIndex = 4;
			this.radioButtonDielectric.Text = "Dielectric";
			this.radioButtonDielectric.UseVisualStyleBackColor = true;
			// 
			// radioButtonConductor
			// 
			this.radioButtonConductor.AutoSize = true;
			this.radioButtonConductor.Checked = true;
			this.radioButtonConductor.Location = new System.Drawing.Point(9, 97);
			this.radioButtonConductor.Name = "radioButtonConductor";
			this.radioButtonConductor.Size = new System.Drawing.Size(74, 17);
			this.radioButtonConductor.TabIndex = 4;
			this.radioButtonConductor.TabStop = true;
			this.radioButtonConductor.Text = "Conductor";
			this.radioButtonConductor.UseVisualStyleBackColor = true;
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
			this.groupBoxSimulation.Location = new System.Drawing.Point(894, 146);
			this.groupBoxSimulation.Name = "groupBoxSimulation";
			this.groupBoxSimulation.Size = new System.Drawing.Size(386, 137);
			this.groupBoxSimulation.TabIndex = 11;
			this.groupBoxSimulation.TabStop = false;
			this.groupBoxSimulation.Text = "Simulation";
			// 
			// groupBoxAnalyticalLobe
			// 
			this.groupBoxAnalyticalLobe.Controls.Add(this.panel2);
			this.groupBoxAnalyticalLobe.Controls.Add(this.tabControlAnalyticalLobes);
			this.groupBoxAnalyticalLobe.Controls.Add(this.checkBoxShowDiffuseModel);
			this.groupBoxAnalyticalLobe.Controls.Add(this.checkBoxShowAnalyticalLobe);
			this.groupBoxAnalyticalLobe.Controls.Add(this.checkBoxInitializeDirectionTowardCenterOfMass);
			this.groupBoxAnalyticalLobe.Controls.Add(this.checkBoxUseCenterOfMassForBetterFitting);
			this.groupBoxAnalyticalLobe.Controls.Add(this.floatTrackbarControlFitOversize);
			this.groupBoxAnalyticalLobe.Controls.Add(this.label10);
			this.groupBoxAnalyticalLobe.Location = new System.Drawing.Point(894, 496);
			this.groupBoxAnalyticalLobe.Name = "groupBoxAnalyticalLobe";
			this.groupBoxAnalyticalLobe.Size = new System.Drawing.Size(386, 307);
			this.groupBoxAnalyticalLobe.TabIndex = 12;
			this.groupBoxAnalyticalLobe.TabStop = false;
			this.groupBoxAnalyticalLobe.Text = "Analytical Lobe";
			// 
			// tabControlAnalyticalLobes
			// 
			this.tabControlAnalyticalLobes.Controls.Add(this.tabPageReflectedLobe);
			this.tabControlAnalyticalLobes.Controls.Add(this.tabPageTransmittedLobe);
			this.tabControlAnalyticalLobes.Location = new System.Drawing.Point(5, 42);
			this.tabControlAnalyticalLobes.Name = "tabControlAnalyticalLobes";
			this.tabControlAnalyticalLobes.SelectedIndex = 0;
			this.tabControlAnalyticalLobes.Size = new System.Drawing.Size(374, 193);
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
			this.tabPageReflectedLobe.Location = new System.Drawing.Point(4, 22);
			this.tabPageReflectedLobe.Name = "tabPageReflectedLobe";
			this.tabPageReflectedLobe.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageReflectedLobe.Size = new System.Drawing.Size(366, 167);
			this.tabPageReflectedLobe.TabIndex = 0;
			this.tabPageReflectedLobe.Text = "Reflected";
			this.tabPageReflectedLobe.UseVisualStyleBackColor = true;
			// 
			// label20
			// 
			this.label20.AutoSize = true;
			this.label20.Location = new System.Drawing.Point(6, 115);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(103, 13);
			this.label20.TabIndex = 3;
			this.label20.Text = "Masking Importance";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(6, 62);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(67, 13);
			this.label13.TabIndex = 3;
			this.label13.Text = "Global Scale";
			// 
			// floatTrackbarControlLobeMaskingImportance
			// 
			this.floatTrackbarControlLobeMaskingImportance.Location = new System.Drawing.Point(123, 110);
			this.floatTrackbarControlLobeMaskingImportance.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLobeMaskingImportance.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLobeMaskingImportance.Name = "floatTrackbarControlLobeMaskingImportance";
			this.floatTrackbarControlLobeMaskingImportance.RangeMax = 1F;
			this.floatTrackbarControlLobeMaskingImportance.RangeMin = 0F;
			this.floatTrackbarControlLobeMaskingImportance.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlLobeMaskingImportance.TabIndex = 5;
			this.floatTrackbarControlLobeMaskingImportance.Value = 1F;
			this.floatTrackbarControlLobeMaskingImportance.VisibleRangeMax = 1F;
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Enabled = false;
			this.label14.Location = new System.Drawing.Point(6, 140);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(86, 13);
			this.label14.TabIndex = 3;
			this.label14.Text = "Scale BiTangent";
			this.label14.Visible = false;
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
			this.tabPageTransmittedLobe.Location = new System.Drawing.Point(4, 22);
			this.tabPageTransmittedLobe.Name = "tabPageTransmittedLobe";
			this.tabPageTransmittedLobe.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageTransmittedLobe.Size = new System.Drawing.Size(366, 167);
			this.tabPageTransmittedLobe.TabIndex = 1;
			this.tabPageTransmittedLobe.Text = "Transmitted";
			this.tabPageTransmittedLobe.UseVisualStyleBackColor = true;
			// 
			// label21
			// 
			this.label21.AutoSize = true;
			this.label21.Location = new System.Drawing.Point(6, 115);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(103, 13);
			this.label21.TabIndex = 16;
			this.label21.Text = "Masking Importance";
			// 
			// floatTrackbarControlLobeMaskingImportance_T
			// 
			this.floatTrackbarControlLobeMaskingImportance_T.Location = new System.Drawing.Point(123, 110);
			this.floatTrackbarControlLobeMaskingImportance_T.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLobeMaskingImportance_T.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLobeMaskingImportance_T.Name = "floatTrackbarControlLobeMaskingImportance_T";
			this.floatTrackbarControlLobeMaskingImportance_T.RangeMax = 1F;
			this.floatTrackbarControlLobeMaskingImportance_T.RangeMin = 0F;
			this.floatTrackbarControlLobeMaskingImportance_T.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlLobeMaskingImportance_T.TabIndex = 17;
			this.floatTrackbarControlLobeMaskingImportance_T.Value = 1F;
			this.floatTrackbarControlLobeMaskingImportance_T.VisibleRangeMax = 1F;
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(6, 11);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(34, 13);
			this.label15.TabIndex = 7;
			this.label15.Text = "Angle";
			// 
			// floatTrackbarControlLobeScaleB_T
			// 
			this.floatTrackbarControlLobeScaleB_T.Enabled = false;
			this.floatTrackbarControlLobeScaleB_T.Location = new System.Drawing.Point(123, 136);
			this.floatTrackbarControlLobeScaleB_T.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLobeScaleB_T.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLobeScaleB_T.Name = "floatTrackbarControlLobeScaleB_T";
			this.floatTrackbarControlLobeScaleB_T.RangeMax = 10000F;
			this.floatTrackbarControlLobeScaleB_T.RangeMin = 0F;
			this.floatTrackbarControlLobeScaleB_T.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlLobeScaleB_T.TabIndex = 12;
			this.floatTrackbarControlLobeScaleB_T.Value = 1F;
			this.floatTrackbarControlLobeScaleB_T.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlAnalyticalLobeTheta_T
			// 
			this.floatTrackbarControlAnalyticalLobeTheta_T.Location = new System.Drawing.Point(123, 6);
			this.floatTrackbarControlAnalyticalLobeTheta_T.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlAnalyticalLobeTheta_T.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlAnalyticalLobeTheta_T.Name = "floatTrackbarControlAnalyticalLobeTheta_T";
			this.floatTrackbarControlAnalyticalLobeTheta_T.RangeMax = 89.999F;
			this.floatTrackbarControlAnalyticalLobeTheta_T.RangeMin = -89.999F;
			this.floatTrackbarControlAnalyticalLobeTheta_T.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlAnalyticalLobeTheta_T.TabIndex = 13;
			this.floatTrackbarControlAnalyticalLobeTheta_T.Value = 0F;
			this.floatTrackbarControlAnalyticalLobeTheta_T.VisibleRangeMax = 89.999F;
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(6, 36);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(61, 13);
			this.label16.TabIndex = 8;
			this.label16.Text = "Roughness";
			// 
			// floatTrackbarControlLobeScaleT_T
			// 
			this.floatTrackbarControlLobeScaleT_T.Location = new System.Drawing.Point(123, 58);
			this.floatTrackbarControlLobeScaleT_T.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLobeScaleT_T.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLobeScaleT_T.Name = "floatTrackbarControlLobeScaleT_T";
			this.floatTrackbarControlLobeScaleT_T.RangeMax = 10000F;
			this.floatTrackbarControlLobeScaleT_T.RangeMin = 0F;
			this.floatTrackbarControlLobeScaleT_T.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlLobeScaleT_T.TabIndex = 14;
			this.floatTrackbarControlLobeScaleT_T.Value = 1F;
			this.floatTrackbarControlLobeScaleT_T.VisibleRangeMax = 1F;
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(6, 89);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(53, 13);
			this.label17.TabIndex = 9;
			this.label17.Text = "Flattening";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(6, 62);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(67, 13);
			this.label18.TabIndex = 10;
			this.label18.Text = "Global Scale";
			// 
			// floatTrackbarControlLobeScaleR_T
			// 
			this.floatTrackbarControlLobeScaleR_T.Location = new System.Drawing.Point(123, 84);
			this.floatTrackbarControlLobeScaleR_T.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLobeScaleR_T.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLobeScaleR_T.Name = "floatTrackbarControlLobeScaleR_T";
			this.floatTrackbarControlLobeScaleR_T.RangeMax = 2F;
			this.floatTrackbarControlLobeScaleR_T.RangeMin = 0F;
			this.floatTrackbarControlLobeScaleR_T.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlLobeScaleR_T.TabIndex = 15;
			this.floatTrackbarControlLobeScaleR_T.Value = 1F;
			this.floatTrackbarControlLobeScaleR_T.VisibleRangeMax = 2F;
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Enabled = false;
			this.label19.Location = new System.Drawing.Point(6, 140);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(86, 13);
			this.label19.TabIndex = 11;
			this.label19.Text = "Scale BiTangent";
			// 
			// floatTrackbarControlAnalyticalLobeRoughness_T
			// 
			this.floatTrackbarControlAnalyticalLobeRoughness_T.Location = new System.Drawing.Point(123, 32);
			this.floatTrackbarControlAnalyticalLobeRoughness_T.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlAnalyticalLobeRoughness_T.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlAnalyticalLobeRoughness_T.Name = "floatTrackbarControlAnalyticalLobeRoughness_T";
			this.floatTrackbarControlAnalyticalLobeRoughness_T.RangeMax = 1F;
			this.floatTrackbarControlAnalyticalLobeRoughness_T.RangeMin = 0F;
			this.floatTrackbarControlAnalyticalLobeRoughness_T.Size = new System.Drawing.Size(238, 20);
			this.floatTrackbarControlAnalyticalLobeRoughness_T.TabIndex = 6;
			this.floatTrackbarControlAnalyticalLobeRoughness_T.Value = 0.9444F;
			this.floatTrackbarControlAnalyticalLobeRoughness_T.VisibleRangeMax = 1F;
			// 
			// checkBoxShowDiffuseModel
			// 
			this.checkBoxShowDiffuseModel.AutoSize = true;
			this.checkBoxShowDiffuseModel.Location = new System.Drawing.Point(272, 0);
			this.checkBoxShowDiffuseModel.Name = "checkBoxShowDiffuseModel";
			this.checkBoxShowDiffuseModel.Size = new System.Drawing.Size(121, 17);
			this.checkBoxShowDiffuseModel.TabIndex = 9;
			this.checkBoxShowDiffuseModel.Text = "Show Diffuse Model";
			this.checkBoxShowDiffuseModel.UseVisualStyleBackColor = true;
			// 
			// checkBoxInitializeDirectionTowardCenterOfMass
			// 
			this.checkBoxInitializeDirectionTowardCenterOfMass.AutoSize = true;
			this.checkBoxInitializeDirectionTowardCenterOfMass.Checked = true;
			this.checkBoxInitializeDirectionTowardCenterOfMass.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxInitializeDirectionTowardCenterOfMass.Location = new System.Drawing.Point(210, 251);
			this.checkBoxInitializeDirectionTowardCenterOfMass.Name = "checkBoxInitializeDirectionTowardCenterOfMass";
			this.checkBoxInitializeDirectionTowardCenterOfMass.Size = new System.Drawing.Size(176, 17);
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
			this.groupBox1.Location = new System.Drawing.Point(894, 389);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(386, 101);
			this.groupBox1.TabIndex = 13;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Simulated Lobe Display";
			// 
			// checkBoxCompensateScatteringFactor
			// 
			this.checkBoxCompensateScatteringFactor.AutoSize = true;
			this.checkBoxCompensateScatteringFactor.Checked = true;
			this.checkBoxCompensateScatteringFactor.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxCompensateScatteringFactor.Location = new System.Drawing.Point(142, 74);
			this.checkBoxCompensateScatteringFactor.Name = "checkBoxCompensateScatteringFactor";
			this.checkBoxCompensateScatteringFactor.Size = new System.Drawing.Size(241, 17);
			this.checkBoxCompensateScatteringFactor.TabIndex = 9;
			this.checkBoxCompensateScatteringFactor.Text = "Use pow( 3, scattering ) Compensation Factor";
			this.checkBoxCompensateScatteringFactor.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowXRay
			// 
			this.checkBoxShowXRay.AutoSize = true;
			this.checkBoxShowXRay.Checked = true;
			this.checkBoxShowXRay.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowXRay.Location = new System.Drawing.Point(233, 0);
			this.checkBoxShowXRay.Name = "checkBoxShowXRay";
			this.checkBoxShowXRay.Size = new System.Drawing.Size(85, 17);
			this.checkBoxShowXRay.TabIndex = 9;
			this.checkBoxShowXRay.Text = "Show X-Ray";
			this.checkBoxShowXRay.UseVisualStyleBackColor = true;
			// 
			// buttonAutomation
			// 
			this.buttonAutomation.Location = new System.Drawing.Point(1205, 848);
			this.buttonAutomation.Name = "buttonAutomation";
			this.buttonAutomation.Size = new System.Drawing.Size(75, 23);
			this.buttonAutomation.TabIndex = 14;
			this.buttonAutomation.Text = "&Automation";
			this.buttonAutomation.UseVisualStyleBackColor = true;
			this.buttonAutomation.Click += new System.EventHandler(this.buttonAutomation_Click);
			// 
			// radioButtonAnalyticalPhongAnisotropic
			// 
			this.radioButtonAnalyticalPhongAnisotropic.AutoSize = true;
			this.radioButtonAnalyticalPhongAnisotropic.Location = new System.Drawing.Point(202, 2);
			this.radioButtonAnalyticalPhongAnisotropic.Name = "radioButtonAnalyticalPhongAnisotropic";
			this.radioButtonAnalyticalPhongAnisotropic.Size = new System.Drawing.Size(85, 17);
			this.radioButtonAnalyticalPhongAnisotropic.TabIndex = 0;
			this.radioButtonAnalyticalPhongAnisotropic.Text = "Phong Aniso";
			this.radioButtonAnalyticalPhongAnisotropic.UseVisualStyleBackColor = true;
			// 
			// panelOutput
			// 
			this.panelOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(876, 835);
			this.panelOutput.TabIndex = 0;
			// 
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1287, 884);
			this.Controls.Add(this.buttonAutomation);
			this.Controls.Add(this.groupBoxAnalyticalLobe);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.buttonFit);
			this.Controls.Add(this.groupBoxSimulation);
			this.Controls.Add(this.groupBoxSurface);
			this.Controls.Add(this.groupBoxDisplay);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput);
			this.Name = "TestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Multiple-Scattering BSDF Test";
			this.groupBoxDisplay.ResumeLayout(false);
			this.groupBoxDisplay.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.groupBoxSurface.ResumeLayout(false);
			this.groupBoxSurface.PerformLayout();
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
			this.ResumeLayout(false);

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
		private System.Windows.Forms.CheckBox checkBoxUseCenterOfMassForBetterFitting;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSurfaceAlbedo;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.GroupBox groupBoxSurface;
		private System.Windows.Forms.GroupBox groupBoxSimulation;
		private System.Windows.Forms.GroupBox groupBoxAnalyticalLobe;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label13;
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
		private System.Windows.Forms.CheckBox checkBoxShowDiffuseModel;
		private System.Windows.Forms.RadioButton radioButtonAnalyticalPhongAnisotropic;
	}
}

