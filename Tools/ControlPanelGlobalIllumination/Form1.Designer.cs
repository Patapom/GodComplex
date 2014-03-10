namespace ControlPanelGlobalIllumination
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
			this.groupBoxAtmosphere = new System.Windows.Forms.GroupBox();
			this.panelSkyColor = new System.Windows.Forms.Panel();
			this.checkBoxEnableSky = new System.Windows.Forms.CheckBox();
			this.checkBoxEnableSun = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlSunIntensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlSkyIntensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlSunAzimuth = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlSunTheta = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label42 = new System.Windows.Forms.Label();
			this.groupBoxClouds = new System.Windows.Forms.GroupBox();
			this.floatTrackbarControlPointLightBounceFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.floatTrackbarControlEmissiveLightsBounceFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlStaticLightsBounceFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlSkyBounceFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlSunBounceFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label10 = new System.Windows.Forms.Label();
			this.groupBoxTerrain = new System.Windows.Forms.GroupBox();
			this.panelLightColor = new System.Windows.Forms.Panel();
			this.checkBoxEnableDynamicPointLight = new System.Windows.Forms.CheckBox();
			this.checkBoxAnimatePointLight = new System.Windows.Forms.CheckBox();
			this.label21 = new System.Windows.Forms.Label();
			this.floatTrackbarControlPointLightIntensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.buttonLoadPreset = new System.Windows.Forms.Button();
			this.buttonSavePreset = new System.Windows.Forms.Button();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.colorDialog = new System.Windows.Forms.ColorDialog();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.floatTrackbarControlEmissiveIntensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.panelEmissiveColor = new System.Windows.Forms.Panel();
			this.checkBoxEmissiveRandomAnimation = new System.Windows.Forms.CheckBox();
			this.checkBoxEnableStaticLighting = new System.Windows.Forms.CheckBox();
			this.checkBoxEnableEmissive = new System.Windows.Forms.CheckBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.checkBoxShowDebugProbes = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlDebugProbeIntensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label8 = new System.Windows.Forms.Label();
			this.checkBoxShowNetwork = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlNeighborProbesContribution = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label9 = new System.Windows.Forms.Label();
			this.checkBoxEnableRedistribution = new System.Windows.Forms.CheckBox();
			this.groupBoxAtmosphere.SuspendLayout();
			this.groupBoxClouds.SuspendLayout();
			this.groupBoxTerrain.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBoxAtmosphere
			// 
			this.groupBoxAtmosphere.Controls.Add(this.panelSkyColor);
			this.groupBoxAtmosphere.Controls.Add(this.checkBoxEnableSky);
			this.groupBoxAtmosphere.Controls.Add(this.checkBoxEnableSun);
			this.groupBoxAtmosphere.Controls.Add(this.label4);
			this.groupBoxAtmosphere.Controls.Add(this.label3);
			this.groupBoxAtmosphere.Controls.Add(this.floatTrackbarControlSunIntensity);
			this.groupBoxAtmosphere.Controls.Add(this.label2);
			this.groupBoxAtmosphere.Controls.Add(this.label1);
			this.groupBoxAtmosphere.Controls.Add(this.floatTrackbarControlSkyIntensity);
			this.groupBoxAtmosphere.Controls.Add(this.floatTrackbarControlSunAzimuth);
			this.groupBoxAtmosphere.Controls.Add(this.floatTrackbarControlSunTheta);
			this.groupBoxAtmosphere.Controls.Add(this.label42);
			this.groupBoxAtmosphere.Location = new System.Drawing.Point(12, 12);
			this.groupBoxAtmosphere.Name = "groupBoxAtmosphere";
			this.groupBoxAtmosphere.Size = new System.Drawing.Size(308, 236);
			this.groupBoxAtmosphere.TabIndex = 0;
			this.groupBoxAtmosphere.TabStop = false;
			this.groupBoxAtmosphere.Text = "Atmosphere";
			// 
			// panelSkyColor
			// 
			this.panelSkyColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(163)))), ((int)(((byte)(201)))), ((int)(((byte)(255)))));
			this.panelSkyColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelSkyColor.Location = new System.Drawing.Point(88, 201);
			this.panelSkyColor.Name = "panelSkyColor";
			this.panelSkyColor.Size = new System.Drawing.Size(34, 24);
			this.panelSkyColor.TabIndex = 4;
			this.panelSkyColor.Click += new System.EventHandler(this.panelSkyColor_Click);
			// 
			// checkBoxEnableSky
			// 
			this.checkBoxEnableSky.AutoSize = true;
			this.checkBoxEnableSky.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxEnableSky.Location = new System.Drawing.Point(9, 152);
			this.checkBoxEnableSky.Name = "checkBoxEnableSky";
			this.checkBoxEnableSky.Size = new System.Drawing.Size(90, 17);
			this.checkBoxEnableSky.TabIndex = 3;
			this.checkBoxEnableSky.Text = "Enable Sky";
			this.checkBoxEnableSky.UseVisualStyleBackColor = true;
			this.checkBoxEnableSky.CheckedChanged += new System.EventHandler(this.checkBoxEnableSky_CheckedChanged);
			// 
			// checkBoxEnableSun
			// 
			this.checkBoxEnableSun.AutoSize = true;
			this.checkBoxEnableSun.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxEnableSun.Location = new System.Drawing.Point(9, 25);
			this.checkBoxEnableSun.Name = "checkBoxEnableSun";
			this.checkBoxEnableSun.Size = new System.Drawing.Size(91, 17);
			this.checkBoxEnableSun.TabIndex = 3;
			this.checkBoxEnableSun.Text = "Enable Sun";
			this.checkBoxEnableSun.UseVisualStyleBackColor = true;
			this.checkBoxEnableSun.CheckedChanged += new System.EventHandler(this.checkBoxEnableSun_CheckedChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 204);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(52, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "Sky Color";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 179);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(67, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Sky Intensity";
			// 
			// floatTrackbarControlSunIntensity
			// 
			this.floatTrackbarControlSunIntensity.Location = new System.Drawing.Point(88, 97);
			this.floatTrackbarControlSunIntensity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSunIntensity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSunIntensity.Name = "floatTrackbarControlSunIntensity";
			this.floatTrackbarControlSunIntensity.RangeMax = 1000F;
			this.floatTrackbarControlSunIntensity.RangeMin = 0F;
			this.floatTrackbarControlSunIntensity.Size = new System.Drawing.Size(214, 20);
			this.floatTrackbarControlSunIntensity.TabIndex = 2;
			this.floatTrackbarControlSunIntensity.Value = 30F;
			this.floatTrackbarControlSunIntensity.VisibleRangeMax = 100F;
			this.floatTrackbarControlSunIntensity.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlSunIntensity_ValueChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 78);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(66, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Sun Azimuth";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 52);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(73, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Sun Elevation";
			// 
			// floatTrackbarControlSkyIntensity
			// 
			this.floatTrackbarControlSkyIntensity.Location = new System.Drawing.Point(88, 175);
			this.floatTrackbarControlSkyIntensity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSkyIntensity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSkyIntensity.Name = "floatTrackbarControlSkyIntensity";
			this.floatTrackbarControlSkyIntensity.RangeMax = 1000F;
			this.floatTrackbarControlSkyIntensity.RangeMin = 0F;
			this.floatTrackbarControlSkyIntensity.Size = new System.Drawing.Size(214, 20);
			this.floatTrackbarControlSkyIntensity.TabIndex = 3;
			this.floatTrackbarControlSkyIntensity.Value = 3F;
			this.floatTrackbarControlSkyIntensity.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlSkyIntensity_ValueChanged);
			// 
			// floatTrackbarControlSunAzimuth
			// 
			this.floatTrackbarControlSunAzimuth.Location = new System.Drawing.Point(88, 74);
			this.floatTrackbarControlSunAzimuth.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSunAzimuth.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSunAzimuth.Name = "floatTrackbarControlSunAzimuth";
			this.floatTrackbarControlSunAzimuth.RangeMax = 180F;
			this.floatTrackbarControlSunAzimuth.RangeMin = -180F;
			this.floatTrackbarControlSunAzimuth.Size = new System.Drawing.Size(214, 20);
			this.floatTrackbarControlSunAzimuth.TabIndex = 1;
			this.floatTrackbarControlSunAzimuth.Value = 0F;
			this.floatTrackbarControlSunAzimuth.VisibleRangeMax = 180F;
			this.floatTrackbarControlSunAzimuth.VisibleRangeMin = -180F;
			this.floatTrackbarControlSunAzimuth.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlSunAzimuth_ValueChanged);
			// 
			// floatTrackbarControlSunTheta
			// 
			this.floatTrackbarControlSunTheta.Location = new System.Drawing.Point(88, 48);
			this.floatTrackbarControlSunTheta.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSunTheta.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSunTheta.Name = "floatTrackbarControlSunTheta";
			this.floatTrackbarControlSunTheta.RangeMax = 180F;
			this.floatTrackbarControlSunTheta.RangeMin = 1F;
			this.floatTrackbarControlSunTheta.Size = new System.Drawing.Size(214, 20);
			this.floatTrackbarControlSunTheta.TabIndex = 0;
			this.floatTrackbarControlSunTheta.Value = 45F;
			this.floatTrackbarControlSunTheta.VisibleRangeMax = 120F;
			this.floatTrackbarControlSunTheta.VisibleRangeMin = 1F;
			this.floatTrackbarControlSunTheta.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlSunTheta_ValueChanged);
			// 
			// label42
			// 
			this.label42.AutoSize = true;
			this.label42.Location = new System.Drawing.Point(6, 101);
			this.label42.Name = "label42";
			this.label42.Size = new System.Drawing.Size(68, 13);
			this.label42.TabIndex = 2;
			this.label42.Text = "Sun Intensity";
			// 
			// groupBoxClouds
			// 
			this.groupBoxClouds.Controls.Add(this.floatTrackbarControlPointLightBounceFactor);
			this.groupBoxClouds.Controls.Add(this.label11);
			this.groupBoxClouds.Controls.Add(this.label12);
			this.groupBoxClouds.Controls.Add(this.label13);
			this.groupBoxClouds.Controls.Add(this.label14);
			this.groupBoxClouds.Controls.Add(this.floatTrackbarControlEmissiveLightsBounceFactor);
			this.groupBoxClouds.Controls.Add(this.floatTrackbarControlStaticLightsBounceFactor);
			this.groupBoxClouds.Controls.Add(this.floatTrackbarControlSkyBounceFactor);
			this.groupBoxClouds.Controls.Add(this.floatTrackbarControlSunBounceFactor);
			this.groupBoxClouds.Controls.Add(this.label10);
			this.groupBoxClouds.Location = new System.Drawing.Point(12, 254);
			this.groupBoxClouds.Name = "groupBoxClouds";
			this.groupBoxClouds.Size = new System.Drawing.Size(308, 171);
			this.groupBoxClouds.TabIndex = 1;
			this.groupBoxClouds.TabStop = false;
			this.groupBoxClouds.Text = "Bounce Factors";
			// 
			// floatTrackbarControlPointLightBounceFactor
			// 
			this.floatTrackbarControlPointLightBounceFactor.Location = new System.Drawing.Point(88, 82);
			this.floatTrackbarControlPointLightBounceFactor.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlPointLightBounceFactor.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlPointLightBounceFactor.Name = "floatTrackbarControlPointLightBounceFactor";
			this.floatTrackbarControlPointLightBounceFactor.RangeMax = 1000F;
			this.floatTrackbarControlPointLightBounceFactor.RangeMin = 0F;
			this.floatTrackbarControlPointLightBounceFactor.Size = new System.Drawing.Size(214, 20);
			this.floatTrackbarControlPointLightBounceFactor.TabIndex = 2;
			this.floatTrackbarControlPointLightBounceFactor.Value = 100F;
			this.floatTrackbarControlPointLightBounceFactor.VisibleRangeMax = 100F;
			this.floatTrackbarControlPointLightBounceFactor.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlPointLightBounceFactor_ValueChanged);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(6, 110);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(65, 13);
			this.label11.TabIndex = 2;
			this.label11.Text = "Static Lights";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(6, 84);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(79, 13);
			this.label12.TabIndex = 2;
			this.label12.Text = "Dynamic Lights";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(6, 58);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(25, 13);
			this.label13.TabIndex = 2;
			this.label13.Text = "Sky";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(6, 32);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(26, 13);
			this.label14.TabIndex = 2;
			this.label14.Text = "Sun";
			// 
			// floatTrackbarControlEmissiveLightsBounceFactor
			// 
			this.floatTrackbarControlEmissiveLightsBounceFactor.Location = new System.Drawing.Point(88, 134);
			this.floatTrackbarControlEmissiveLightsBounceFactor.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlEmissiveLightsBounceFactor.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlEmissiveLightsBounceFactor.Name = "floatTrackbarControlEmissiveLightsBounceFactor";
			this.floatTrackbarControlEmissiveLightsBounceFactor.RangeMax = 1000F;
			this.floatTrackbarControlEmissiveLightsBounceFactor.RangeMin = 0F;
			this.floatTrackbarControlEmissiveLightsBounceFactor.Size = new System.Drawing.Size(214, 20);
			this.floatTrackbarControlEmissiveLightsBounceFactor.TabIndex = 4;
			this.floatTrackbarControlEmissiveLightsBounceFactor.Value = 100F;
			this.floatTrackbarControlEmissiveLightsBounceFactor.VisibleRangeMax = 100F;
			this.floatTrackbarControlEmissiveLightsBounceFactor.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlEmissiveLightsBounceFactor_ValueChanged);
			// 
			// floatTrackbarControlStaticLightsBounceFactor
			// 
			this.floatTrackbarControlStaticLightsBounceFactor.Location = new System.Drawing.Point(88, 108);
			this.floatTrackbarControlStaticLightsBounceFactor.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlStaticLightsBounceFactor.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlStaticLightsBounceFactor.Name = "floatTrackbarControlStaticLightsBounceFactor";
			this.floatTrackbarControlStaticLightsBounceFactor.RangeMax = 1000F;
			this.floatTrackbarControlStaticLightsBounceFactor.RangeMin = 0F;
			this.floatTrackbarControlStaticLightsBounceFactor.Size = new System.Drawing.Size(214, 20);
			this.floatTrackbarControlStaticLightsBounceFactor.TabIndex = 3;
			this.floatTrackbarControlStaticLightsBounceFactor.Value = 100F;
			this.floatTrackbarControlStaticLightsBounceFactor.VisibleRangeMax = 100F;
			this.floatTrackbarControlStaticLightsBounceFactor.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlStaticLightsBounceFactor_ValueChanged);
			// 
			// floatTrackbarControlSkyBounceFactor
			// 
			this.floatTrackbarControlSkyBounceFactor.Location = new System.Drawing.Point(88, 56);
			this.floatTrackbarControlSkyBounceFactor.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSkyBounceFactor.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSkyBounceFactor.Name = "floatTrackbarControlSkyBounceFactor";
			this.floatTrackbarControlSkyBounceFactor.RangeMax = 1000F;
			this.floatTrackbarControlSkyBounceFactor.RangeMin = 0F;
			this.floatTrackbarControlSkyBounceFactor.Size = new System.Drawing.Size(214, 20);
			this.floatTrackbarControlSkyBounceFactor.TabIndex = 1;
			this.floatTrackbarControlSkyBounceFactor.Value = 100F;
			this.floatTrackbarControlSkyBounceFactor.VisibleRangeMax = 100F;
			this.floatTrackbarControlSkyBounceFactor.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlSkyBounceFactor_ValueChanged);
			// 
			// floatTrackbarControlSunBounceFactor
			// 
			this.floatTrackbarControlSunBounceFactor.Location = new System.Drawing.Point(88, 30);
			this.floatTrackbarControlSunBounceFactor.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSunBounceFactor.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSunBounceFactor.Name = "floatTrackbarControlSunBounceFactor";
			this.floatTrackbarControlSunBounceFactor.RangeMax = 1000F;
			this.floatTrackbarControlSunBounceFactor.RangeMin = 0F;
			this.floatTrackbarControlSunBounceFactor.Size = new System.Drawing.Size(214, 20);
			this.floatTrackbarControlSunBounceFactor.TabIndex = 0;
			this.floatTrackbarControlSunBounceFactor.Value = 100F;
			this.floatTrackbarControlSunBounceFactor.VisibleRangeMax = 100F;
			this.floatTrackbarControlSunBounceFactor.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlSunBounceFactor_ValueChanged);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(6, 136);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(79, 13);
			this.label10.TabIndex = 2;
			this.label10.Text = "Emissive Lights";
			// 
			// groupBoxTerrain
			// 
			this.groupBoxTerrain.Controls.Add(this.panelLightColor);
			this.groupBoxTerrain.Controls.Add(this.checkBoxEnableDynamicPointLight);
			this.groupBoxTerrain.Controls.Add(this.checkBoxAnimatePointLight);
			this.groupBoxTerrain.Controls.Add(this.label21);
			this.groupBoxTerrain.Controls.Add(this.floatTrackbarControlPointLightIntensity);
			this.groupBoxTerrain.Controls.Add(this.label5);
			this.groupBoxTerrain.Location = new System.Drawing.Point(334, 12);
			this.groupBoxTerrain.Name = "groupBoxTerrain";
			this.groupBoxTerrain.Size = new System.Drawing.Size(308, 117);
			this.groupBoxTerrain.TabIndex = 2;
			this.groupBoxTerrain.TabStop = false;
			this.groupBoxTerrain.Text = "Dynamic Lights";
			// 
			// panelLightColor
			// 
			this.panelLightColor.BackColor = System.Drawing.Color.White;
			this.panelLightColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelLightColor.Location = new System.Drawing.Point(88, 80);
			this.panelLightColor.Name = "panelLightColor";
			this.panelLightColor.Size = new System.Drawing.Size(34, 24);
			this.panelLightColor.TabIndex = 2;
			this.panelLightColor.Click += new System.EventHandler(this.panelLightColor_Click);
			// 
			// checkBoxEnableDynamicPointLight
			// 
			this.checkBoxEnableDynamicPointLight.AutoSize = true;
			this.checkBoxEnableDynamicPointLight.Checked = true;
			this.checkBoxEnableDynamicPointLight.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnableDynamicPointLight.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxEnableDynamicPointLight.Location = new System.Drawing.Point(9, 25);
			this.checkBoxEnableDynamicPointLight.Name = "checkBoxEnableDynamicPointLight";
			this.checkBoxEnableDynamicPointLight.Size = new System.Drawing.Size(182, 17);
			this.checkBoxEnableDynamicPointLight.TabIndex = 0;
			this.checkBoxEnableDynamicPointLight.Text = "Enable Dynamic Point Light";
			this.checkBoxEnableDynamicPointLight.UseVisualStyleBackColor = true;
			this.checkBoxEnableDynamicPointLight.CheckedChanged += new System.EventHandler(this.checkBoxEnableDynamicPointLight_CheckedChanged);
			// 
			// checkBoxAnimatePointLight
			// 
			this.checkBoxAnimatePointLight.AutoSize = true;
			this.checkBoxAnimatePointLight.Location = new System.Drawing.Point(193, 25);
			this.checkBoxAnimatePointLight.Name = "checkBoxAnimatePointLight";
			this.checkBoxAnimatePointLight.Size = new System.Drawing.Size(64, 17);
			this.checkBoxAnimatePointLight.TabIndex = 3;
			this.checkBoxAnimatePointLight.Text = "Animate";
			this.checkBoxAnimatePointLight.UseVisualStyleBackColor = true;
			this.checkBoxAnimatePointLight.CheckedChanged += new System.EventHandler(this.checkBoxAnimatePointLight_CheckedChanged);
			// 
			// label21
			// 
			this.label21.AutoSize = true;
			this.label21.Location = new System.Drawing.Point(6, 58);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(72, 13);
			this.label21.TabIndex = 2;
			this.label21.Text = "Light Intensity";
			// 
			// floatTrackbarControlPointLightIntensity
			// 
			this.floatTrackbarControlPointLightIntensity.Location = new System.Drawing.Point(88, 54);
			this.floatTrackbarControlPointLightIntensity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlPointLightIntensity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlPointLightIntensity.Name = "floatTrackbarControlPointLightIntensity";
			this.floatTrackbarControlPointLightIntensity.RangeMax = 100F;
			this.floatTrackbarControlPointLightIntensity.RangeMin = 0F;
			this.floatTrackbarControlPointLightIntensity.Size = new System.Drawing.Size(214, 20);
			this.floatTrackbarControlPointLightIntensity.TabIndex = 1;
			this.floatTrackbarControlPointLightIntensity.Value = 10F;
			this.floatTrackbarControlPointLightIntensity.VisibleRangeMax = 20F;
			this.floatTrackbarControlPointLightIntensity.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlPointLightIntensity_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 84);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(57, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "Light Color";
			// 
			// buttonLoadPreset
			// 
			this.buttonLoadPreset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLoadPreset.Location = new System.Drawing.Point(11, 431);
			this.buttonLoadPreset.Name = "buttonLoadPreset";
			this.buttonLoadPreset.Size = new System.Drawing.Size(75, 23);
			this.buttonLoadPreset.TabIndex = 1;
			this.buttonLoadPreset.Text = "Load Preset";
			this.buttonLoadPreset.UseVisualStyleBackColor = true;
			this.buttonLoadPreset.Click += new System.EventHandler(this.buttonLoadPreset_Click);
			// 
			// buttonSavePreset
			// 
			this.buttonSavePreset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSavePreset.Location = new System.Drawing.Point(92, 431);
			this.buttonSavePreset.Name = "buttonSavePreset";
			this.buttonSavePreset.Size = new System.Drawing.Size(75, 23);
			this.buttonSavePreset.TabIndex = 2;
			this.buttonSavePreset.Text = "Save Preset";
			this.buttonSavePreset.UseVisualStyleBackColor = true;
			this.buttonSavePreset.Click += new System.EventHandler(this.buttonSavePreset_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "*.xml";
			this.openFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files|*.*";
			this.openFileDialog.Title = "Choose a preset file...";
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.DefaultExt = "*.xml";
			this.saveFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files|*.*";
			this.saveFileDialog.Title = "Choose a preset file...";
			// 
			// colorDialog
			// 
			this.colorDialog.AnyColor = true;
			this.colorDialog.FullOpen = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.floatTrackbarControlEmissiveIntensity);
			this.groupBox1.Controls.Add(this.panelEmissiveColor);
			this.groupBox1.Controls.Add(this.checkBoxEmissiveRandomAnimation);
			this.groupBox1.Controls.Add(this.checkBoxEnableStaticLighting);
			this.groupBox1.Controls.Add(this.checkBoxEnableEmissive);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Location = new System.Drawing.Point(334, 135);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(308, 144);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Static Lighting && Emissive Surfaces";
			// 
			// floatTrackbarControlEmissiveIntensity
			// 
			this.floatTrackbarControlEmissiveIntensity.Location = new System.Drawing.Point(88, 83);
			this.floatTrackbarControlEmissiveIntensity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlEmissiveIntensity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlEmissiveIntensity.Name = "floatTrackbarControlEmissiveIntensity";
			this.floatTrackbarControlEmissiveIntensity.RangeMax = 100F;
			this.floatTrackbarControlEmissiveIntensity.RangeMin = 0F;
			this.floatTrackbarControlEmissiveIntensity.Size = new System.Drawing.Size(214, 20);
			this.floatTrackbarControlEmissiveIntensity.TabIndex = 2;
			this.floatTrackbarControlEmissiveIntensity.Value = 10F;
			this.floatTrackbarControlEmissiveIntensity.VisibleRangeMax = 20F;
			this.floatTrackbarControlEmissiveIntensity.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlEmissiveIntensity_ValueChanged);
			// 
			// panelEmissiveColor
			// 
			this.panelEmissiveColor.BackColor = System.Drawing.Color.LightYellow;
			this.panelEmissiveColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelEmissiveColor.Location = new System.Drawing.Point(88, 109);
			this.panelEmissiveColor.Name = "panelEmissiveColor";
			this.panelEmissiveColor.Size = new System.Drawing.Size(34, 24);
			this.panelEmissiveColor.TabIndex = 3;
			this.panelEmissiveColor.Click += new System.EventHandler(this.panelEmissiveColor_Click);
			// 
			// checkBoxEmissiveRandomAnimation
			// 
			this.checkBoxEmissiveRandomAnimation.AutoSize = true;
			this.checkBoxEmissiveRandomAnimation.Location = new System.Drawing.Point(155, 112);
			this.checkBoxEmissiveRandomAnimation.Name = "checkBoxEmissiveRandomAnimation";
			this.checkBoxEmissiveRandomAnimation.Size = new System.Drawing.Size(115, 17);
			this.checkBoxEmissiveRandomAnimation.TabIndex = 3;
			this.checkBoxEmissiveRandomAnimation.Text = "Random Animation";
			this.checkBoxEmissiveRandomAnimation.UseVisualStyleBackColor = true;
			this.checkBoxEmissiveRandomAnimation.CheckedChanged += new System.EventHandler(this.checkBoxEmissiveRandomAnimation_CheckedChanged);
			// 
			// checkBoxEnableStaticLighting
			// 
			this.checkBoxEnableStaticLighting.AutoSize = true;
			this.checkBoxEnableStaticLighting.Checked = true;
			this.checkBoxEnableStaticLighting.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnableStaticLighting.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxEnableStaticLighting.Location = new System.Drawing.Point(9, 29);
			this.checkBoxEnableStaticLighting.Name = "checkBoxEnableStaticLighting";
			this.checkBoxEnableStaticLighting.Size = new System.Drawing.Size(151, 17);
			this.checkBoxEnableStaticLighting.TabIndex = 0;
			this.checkBoxEnableStaticLighting.Text = "Enable Static Lighting";
			this.checkBoxEnableStaticLighting.UseVisualStyleBackColor = true;
			this.checkBoxEnableStaticLighting.CheckedChanged += new System.EventHandler(this.checkBoxEnableStaticLighting_CheckedChanged);
			// 
			// checkBoxEnableEmissive
			// 
			this.checkBoxEnableEmissive.AutoSize = true;
			this.checkBoxEnableEmissive.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxEnableEmissive.Location = new System.Drawing.Point(9, 60);
			this.checkBoxEnableEmissive.Name = "checkBoxEnableEmissive";
			this.checkBoxEnableEmissive.Size = new System.Drawing.Size(186, 17);
			this.checkBoxEnableEmissive.TabIndex = 1;
			this.checkBoxEnableEmissive.Text = "Enable Emissive Area Lights";
			this.checkBoxEnableEmissive.UseVisualStyleBackColor = true;
			this.checkBoxEnableEmissive.CheckedChanged += new System.EventHandler(this.checkBoxEnableEmissive_CheckedChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 87);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(46, 13);
			this.label6.TabIndex = 2;
			this.label6.Text = "Intensity";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(6, 113);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(75, 13);
			this.label7.TabIndex = 2;
			this.label7.Text = "Emissive Color";
			// 
			// checkBoxShowDebugProbes
			// 
			this.checkBoxShowDebugProbes.AutoSize = true;
			this.checkBoxShowDebugProbes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxShowDebugProbes.Location = new System.Drawing.Point(9, 85);
			this.checkBoxShowDebugProbes.Name = "checkBoxShowDebugProbes";
			this.checkBoxShowDebugProbes.Size = new System.Drawing.Size(141, 17);
			this.checkBoxShowDebugProbes.TabIndex = 3;
			this.checkBoxShowDebugProbes.Text = "Show Debug Probes";
			this.checkBoxShowDebugProbes.UseVisualStyleBackColor = true;
			this.checkBoxShowDebugProbes.CheckedChanged += new System.EventHandler(this.checkBoxShowDebugProbes_CheckedChanged);
			// 
			// floatTrackbarControlDebugProbeIntensity
			// 
			this.floatTrackbarControlDebugProbeIntensity.Location = new System.Drawing.Point(88, 108);
			this.floatTrackbarControlDebugProbeIntensity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDebugProbeIntensity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDebugProbeIntensity.Name = "floatTrackbarControlDebugProbeIntensity";
			this.floatTrackbarControlDebugProbeIntensity.RangeMax = 1000F;
			this.floatTrackbarControlDebugProbeIntensity.RangeMin = 0F;
			this.floatTrackbarControlDebugProbeIntensity.Size = new System.Drawing.Size(214, 20);
			this.floatTrackbarControlDebugProbeIntensity.TabIndex = 0;
			this.floatTrackbarControlDebugProbeIntensity.Value = 1F;
			this.floatTrackbarControlDebugProbeIntensity.VisibleRangeMax = 2F;
			this.floatTrackbarControlDebugProbeIntensity.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlDebugProbeIntensity_ValueChanged);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.floatTrackbarControlNeighborProbesContribution);
			this.groupBox2.Controls.Add(this.checkBoxShowNetwork);
			this.groupBox2.Controls.Add(this.checkBoxShowDebugProbes);
			this.groupBox2.Controls.Add(this.floatTrackbarControlDebugProbeIntensity);
			this.groupBox2.Controls.Add(this.checkBoxEnableRedistribution);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Location = new System.Drawing.Point(334, 287);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(308, 168);
			this.groupBox2.TabIndex = 4;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Misc.";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(6, 110);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(79, 13);
			this.label8.TabIndex = 2;
			this.label8.Text = "Intensity Factor";
			// 
			// checkBoxShowNetwork
			// 
			this.checkBoxShowNetwork.AutoSize = true;
			this.checkBoxShowNetwork.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxShowNetwork.Location = new System.Drawing.Point(9, 134);
			this.checkBoxShowNetwork.Name = "checkBoxShowNetwork";
			this.checkBoxShowNetwork.Size = new System.Drawing.Size(192, 17);
			this.checkBoxShowNetwork.TabIndex = 3;
			this.checkBoxShowNetwork.Text = "Show Debug Probes Network";
			this.checkBoxShowNetwork.UseVisualStyleBackColor = true;
			this.checkBoxShowNetwork.CheckedChanged += new System.EventHandler(this.checkBoxShowNetwork_CheckedChanged);
			// 
			// floatTrackbarControlNeighborProbesContribution
			// 
			this.floatTrackbarControlNeighborProbesContribution.Location = new System.Drawing.Point(88, 49);
			this.floatTrackbarControlNeighborProbesContribution.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlNeighborProbesContribution.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlNeighborProbesContribution.Name = "floatTrackbarControlNeighborProbesContribution";
			this.floatTrackbarControlNeighborProbesContribution.RangeMax = 1000F;
			this.floatTrackbarControlNeighborProbesContribution.RangeMin = 0F;
			this.floatTrackbarControlNeighborProbesContribution.Size = new System.Drawing.Size(214, 20);
			this.floatTrackbarControlNeighborProbesContribution.TabIndex = 0;
			this.floatTrackbarControlNeighborProbesContribution.Value = 10F;
			this.floatTrackbarControlNeighborProbesContribution.VisibleRangeMax = 20F;
			this.floatTrackbarControlNeighborProbesContribution.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlNeighborProbesContribution_ValueChanged);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(6, 51);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(86, 13);
			this.label9.TabIndex = 2;
			this.label9.Text = "Neighbor Redist.";
			// 
			// checkBoxEnableRedistribution
			// 
			this.checkBoxEnableRedistribution.AutoSize = true;
			this.checkBoxEnableRedistribution.Checked = true;
			this.checkBoxEnableRedistribution.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnableRedistribution.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxEnableRedistribution.Location = new System.Drawing.Point(9, 24);
			this.checkBoxEnableRedistribution.Name = "checkBoxEnableRedistribution";
			this.checkBoxEnableRedistribution.Size = new System.Drawing.Size(292, 17);
			this.checkBoxEnableRedistribution.TabIndex = 1;
			this.checkBoxEnableRedistribution.Text = "Enable Energy Redistribution among Neighbors";
			this.checkBoxEnableRedistribution.UseVisualStyleBackColor = true;
			this.checkBoxEnableRedistribution.CheckedChanged += new System.EventHandler(this.checkBoxEnableRedistribution_CheckedChanged);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(654, 467);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.groupBoxTerrain);
			this.Controls.Add(this.buttonSavePreset);
			this.Controls.Add(this.groupBoxClouds);
			this.Controls.Add(this.groupBoxAtmosphere);
			this.Controls.Add(this.buttonLoadPreset);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Form1";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Global Illumination Demo Controller";
			this.groupBoxAtmosphere.ResumeLayout(false);
			this.groupBoxAtmosphere.PerformLayout();
			this.groupBoxClouds.ResumeLayout(false);
			this.groupBoxClouds.PerformLayout();
			this.groupBoxTerrain.ResumeLayout(false);
			this.groupBoxTerrain.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSunTheta;
		private System.Windows.Forms.GroupBox groupBoxAtmosphere;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSunAzimuth;
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSkyIntensity;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.GroupBox groupBoxClouds;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label14;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlEmissiveLightsBounceFactor;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlStaticLightsBounceFactor;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlPointLightBounceFactor;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSkyBounceFactor;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSunBounceFactor;
		private System.Windows.Forms.GroupBox groupBoxTerrain;
		private System.Windows.Forms.Label label21;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlPointLightIntensity;
		private System.Windows.Forms.Button buttonLoadPreset;
		private System.Windows.Forms.Button buttonSavePreset;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSunIntensity;
		private System.Windows.Forms.Label label42;
		private System.Windows.Forms.CheckBox checkBoxEnableSun;
		private System.Windows.Forms.CheckBox checkBoxEnableSky;
		private System.Windows.Forms.Panel panelSkyColor;
		private System.Windows.Forms.ColorDialog colorDialog;
		private System.Windows.Forms.CheckBox checkBoxEnableDynamicPointLight;
		private System.Windows.Forms.Panel panelLightColor;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Panel panelEmissiveColor;
		private System.Windows.Forms.CheckBox checkBoxEnableEmissive;
		private System.Windows.Forms.Label label6;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlEmissiveIntensity;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox checkBoxEmissiveRandomAnimation;
		private System.Windows.Forms.CheckBox checkBoxEnableStaticLighting;
		private System.Windows.Forms.CheckBox checkBoxAnimatePointLight;
		private System.Windows.Forms.CheckBox checkBoxShowDebugProbes;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDebugProbeIntensity;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.CheckBox checkBoxShowNetwork;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlNeighborProbesContribution;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.CheckBox checkBoxEnableRedistribution;
	}
}

