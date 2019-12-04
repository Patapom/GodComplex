namespace TestMSBRDF
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm));
			this.floatTrackbarControlRoughnessSphere = new UIUtility.FloatTrackbarControl();
			this.panelOutput = new UIUtility.PanelOutput(this.components);
			this.floatTrackbarControlReflectanceGround = new UIUtility.FloatTrackbarControl();
			this.buttonReload = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLightElevation = new UIUtility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.floatTrackbarControlRoughnessGround = new UIUtility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.checkBoxEnableMSBRDF = new System.Windows.Forms.CheckBox();
			this.checkBoxKeepSampling = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlReflectanceSphere = new UIUtility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.checkBoxPause = new System.Windows.Forms.CheckBox();
			this.groupBoxSphere = new System.Windows.Forms.GroupBox();
			this.label8 = new System.Windows.Forms.Label();
			this.floatTrackbarControlRoughnessSphere2 = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlReflectanceSphere2 = new UIUtility.FloatTrackbarControl();
			this.label7 = new System.Windows.Forms.Label();
			this.groupBoxPlane = new System.Windows.Forms.GroupBox();
			this.groupBoxEnvironment = new System.Windows.Forms.GroupBox();
			this.floatTrackbarControlCubeMapIntensity = new UIUtility.FloatTrackbarControl();
			this.label6 = new System.Windows.Forms.Label();
			this.checkBoxEnableMSFactor = new System.Windows.Forms.CheckBox();
			this.checkBoxUseRealTimeApprox = new System.Windows.Forms.CheckBox();
			this.checkBoxUseLTC = new System.Windows.Forms.CheckBox();
			this.groupBoxSphere.SuspendLayout();
			this.groupBoxPlane.SuspendLayout();
			this.groupBoxEnvironment.SuspendLayout();
			this.SuspendLayout();
			// 
			// floatTrackbarControlRoughnessSphere
			// 
			this.floatTrackbarControlRoughnessSphere.Location = new System.Drawing.Point(92, 19);
			this.floatTrackbarControlRoughnessSphere.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlRoughnessSphere.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlRoughnessSphere.Name = "floatTrackbarControlRoughnessSphere";
			this.floatTrackbarControlRoughnessSphere.RangeMin = 0F;
			this.floatTrackbarControlRoughnessSphere.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlRoughnessSphere.TabIndex = 0;
			this.floatTrackbarControlRoughnessSphere.Value = 0.1F;
			this.floatTrackbarControlRoughnessSphere.VisibleRangeMax = 1F;
			this.floatTrackbarControlRoughnessSphere.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlRoughnessSpec_ValueChanged);
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.PanelBitmap = ((System.Drawing.Bitmap)(resources.GetObject("panelOutput.PanelBitmap")));
			this.panelOutput.Size = new System.Drawing.Size(1280, 720);
			this.panelOutput.TabIndex = 1;
			// 
			// floatTrackbarControlReflectanceGround
			// 
			this.floatTrackbarControlReflectanceGround.Location = new System.Drawing.Point(94, 45);
			this.floatTrackbarControlReflectanceGround.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlReflectanceGround.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlReflectanceGround.Name = "floatTrackbarControlReflectanceGround";
			this.floatTrackbarControlReflectanceGround.RangeMax = 1F;
			this.floatTrackbarControlReflectanceGround.RangeMin = 0F;
			this.floatTrackbarControlReflectanceGround.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlReflectanceGround.TabIndex = 0;
			this.floatTrackbarControlReflectanceGround.Value = 0.75F;
			this.floatTrackbarControlReflectanceGround.VisibleRangeMax = 1F;
			this.floatTrackbarControlReflectanceGround.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlAlbedo_ValueChanged);
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(1515, 709);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 2;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 21);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(89, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Roughness Spec";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 49);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(65, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Reflectance";
			// 
			// floatTrackbarControlLightElevation
			// 
			this.floatTrackbarControlLightElevation.Location = new System.Drawing.Point(92, 19);
			this.floatTrackbarControlLightElevation.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightElevation.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightElevation.Name = "floatTrackbarControlLightElevation";
			this.floatTrackbarControlLightElevation.RangeMax = 1F;
			this.floatTrackbarControlLightElevation.RangeMin = -1F;
			this.floatTrackbarControlLightElevation.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightElevation.TabIndex = 0;
			this.floatTrackbarControlLightElevation.Value = 0F;
			this.floatTrackbarControlLightElevation.VisibleRangeMax = 1F;
			this.floatTrackbarControlLightElevation.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlLightElevation_ValueChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 23);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(47, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Rotation";
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 10;
			// 
			// floatTrackbarControlRoughnessGround
			// 
			this.floatTrackbarControlRoughnessGround.Location = new System.Drawing.Point(92, 19);
			this.floatTrackbarControlRoughnessGround.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlRoughnessGround.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlRoughnessGround.Name = "floatTrackbarControlRoughnessGround";
			this.floatTrackbarControlRoughnessGround.RangeMin = 0F;
			this.floatTrackbarControlRoughnessGround.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlRoughnessGround.TabIndex = 0;
			this.floatTrackbarControlRoughnessGround.Value = 0F;
			this.floatTrackbarControlRoughnessGround.VisibleRangeMax = 1F;
			this.floatTrackbarControlRoughnessGround.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlRoughnessDiffuse_ValueChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 23);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(64, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Roughness ";
			// 
			// checkBoxEnableMSBRDF
			// 
			this.checkBoxEnableMSBRDF.AutoSize = true;
			this.checkBoxEnableMSBRDF.Checked = true;
			this.checkBoxEnableMSBRDF.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnableMSBRDF.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxEnableMSBRDF.Location = new System.Drawing.Point(1307, 416);
			this.checkBoxEnableMSBRDF.Name = "checkBoxEnableMSBRDF";
			this.checkBoxEnableMSBRDF.Size = new System.Drawing.Size(244, 28);
			this.checkBoxEnableMSBRDF.TabIndex = 4;
			this.checkBoxEnableMSBRDF.Text = "Enable multiple scattering";
			this.checkBoxEnableMSBRDF.UseVisualStyleBackColor = true;
			this.checkBoxEnableMSBRDF.CheckedChanged += new System.EventHandler(this.checkBoxEnableMSBRDF_CheckedChanged);
			// 
			// checkBoxKeepSampling
			// 
			this.checkBoxKeepSampling.AutoSize = true;
			this.checkBoxKeepSampling.Checked = true;
			this.checkBoxKeepSampling.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxKeepSampling.Location = new System.Drawing.Point(1301, 690);
			this.checkBoxKeepSampling.Name = "checkBoxKeepSampling";
			this.checkBoxKeepSampling.Size = new System.Drawing.Size(97, 17);
			this.checkBoxKeepSampling.TabIndex = 4;
			this.checkBoxKeepSampling.Text = "Keep Sampling";
			this.checkBoxKeepSampling.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControlReflectanceSphere
			// 
			this.floatTrackbarControlReflectanceSphere.Location = new System.Drawing.Point(92, 45);
			this.floatTrackbarControlReflectanceSphere.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlReflectanceSphere.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlReflectanceSphere.Name = "floatTrackbarControlReflectanceSphere";
			this.floatTrackbarControlReflectanceSphere.RangeMax = 1F;
			this.floatTrackbarControlReflectanceSphere.RangeMin = 0F;
			this.floatTrackbarControlReflectanceSphere.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlReflectanceSphere.TabIndex = 0;
			this.floatTrackbarControlReflectanceSphere.Value = 1F;
			this.floatTrackbarControlReflectanceSphere.VisibleRangeMax = 1F;
			this.floatTrackbarControlReflectanceSphere.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlAlbedo_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 48);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(93, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "Reflectance Spec";
			// 
			// checkBoxPause
			// 
			this.checkBoxPause.AutoSize = true;
			this.checkBoxPause.Location = new System.Drawing.Point(1301, 713);
			this.checkBoxPause.Name = "checkBoxPause";
			this.checkBoxPause.Size = new System.Drawing.Size(108, 17);
			this.checkBoxPause.TabIndex = 4;
			this.checkBoxPause.Text = "Pause Rendering";
			this.checkBoxPause.UseVisualStyleBackColor = true;
			// 
			// groupBoxSphere
			// 
			this.groupBoxSphere.Controls.Add(this.label8);
			this.groupBoxSphere.Controls.Add(this.floatTrackbarControlRoughnessSphere2);
			this.groupBoxSphere.Controls.Add(this.floatTrackbarControlReflectanceSphere2);
			this.groupBoxSphere.Controls.Add(this.floatTrackbarControlRoughnessSphere);
			this.groupBoxSphere.Controls.Add(this.label7);
			this.groupBoxSphere.Controls.Add(this.floatTrackbarControlReflectanceSphere);
			this.groupBoxSphere.Controls.Add(this.label5);
			this.groupBoxSphere.Controls.Add(this.label1);
			this.groupBoxSphere.Location = new System.Drawing.Point(1298, 12);
			this.groupBoxSphere.Name = "groupBoxSphere";
			this.groupBoxSphere.Size = new System.Drawing.Size(300, 145);
			this.groupBoxSphere.TabIndex = 5;
			this.groupBoxSphere.TabStop = false;
			this.groupBoxSphere.Text = "Sphere";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(6, 88);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(80, 13);
			this.label8.TabIndex = 3;
			this.label8.Text = "Roughness Diff";
			// 
			// floatTrackbarControlRoughnessSphere2
			// 
			this.floatTrackbarControlRoughnessSphere2.Location = new System.Drawing.Point(92, 86);
			this.floatTrackbarControlRoughnessSphere2.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlRoughnessSphere2.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlRoughnessSphere2.Name = "floatTrackbarControlRoughnessSphere2";
			this.floatTrackbarControlRoughnessSphere2.RangeMin = 0F;
			this.floatTrackbarControlRoughnessSphere2.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlRoughnessSphere2.TabIndex = 0;
			this.floatTrackbarControlRoughnessSphere2.Value = 0.1F;
			this.floatTrackbarControlRoughnessSphere2.VisibleRangeMax = 1F;
			this.floatTrackbarControlRoughnessSphere2.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlRoughnessSpec_ValueChanged);
			// 
			// floatTrackbarControlReflectanceSphere2
			// 
			this.floatTrackbarControlReflectanceSphere2.Location = new System.Drawing.Point(92, 112);
			this.floatTrackbarControlReflectanceSphere2.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlReflectanceSphere2.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlReflectanceSphere2.Name = "floatTrackbarControlReflectanceSphere2";
			this.floatTrackbarControlReflectanceSphere2.RangeMax = 1F;
			this.floatTrackbarControlReflectanceSphere2.RangeMin = 0F;
			this.floatTrackbarControlReflectanceSphere2.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlReflectanceSphere2.TabIndex = 0;
			this.floatTrackbarControlReflectanceSphere2.Value = 1F;
			this.floatTrackbarControlReflectanceSphere2.VisibleRangeMax = 1F;
			this.floatTrackbarControlReflectanceSphere2.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlAlbedo_ValueChanged);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(6, 115);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(84, 13);
			this.label7.TabIndex = 3;
			this.label7.Text = "Reflectance Diff";
			// 
			// groupBoxPlane
			// 
			this.groupBoxPlane.Controls.Add(this.floatTrackbarControlRoughnessGround);
			this.groupBoxPlane.Controls.Add(this.label2);
			this.groupBoxPlane.Controls.Add(this.floatTrackbarControlReflectanceGround);
			this.groupBoxPlane.Controls.Add(this.label4);
			this.groupBoxPlane.Location = new System.Drawing.Point(1298, 173);
			this.groupBoxPlane.Name = "groupBoxPlane";
			this.groupBoxPlane.Size = new System.Drawing.Size(300, 83);
			this.groupBoxPlane.TabIndex = 6;
			this.groupBoxPlane.TabStop = false;
			this.groupBoxPlane.Text = "Ground Plane";
			// 
			// groupBoxEnvironment
			// 
			this.groupBoxEnvironment.Controls.Add(this.floatTrackbarControlCubeMapIntensity);
			this.groupBoxEnvironment.Controls.Add(this.label6);
			this.groupBoxEnvironment.Controls.Add(this.floatTrackbarControlLightElevation);
			this.groupBoxEnvironment.Controls.Add(this.label3);
			this.groupBoxEnvironment.Location = new System.Drawing.Point(1298, 272);
			this.groupBoxEnvironment.Name = "groupBoxEnvironment";
			this.groupBoxEnvironment.Size = new System.Drawing.Size(300, 99);
			this.groupBoxEnvironment.TabIndex = 7;
			this.groupBoxEnvironment.TabStop = false;
			this.groupBoxEnvironment.Text = "Environment Cube Map";
			// 
			// floatTrackbarControlCubeMapIntensity
			// 
			this.floatTrackbarControlCubeMapIntensity.Location = new System.Drawing.Point(92, 45);
			this.floatTrackbarControlCubeMapIntensity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlCubeMapIntensity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlCubeMapIntensity.Name = "floatTrackbarControlCubeMapIntensity";
			this.floatTrackbarControlCubeMapIntensity.RangeMax = 1000F;
			this.floatTrackbarControlCubeMapIntensity.RangeMin = 0F;
			this.floatTrackbarControlCubeMapIntensity.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlCubeMapIntensity.TabIndex = 0;
			this.floatTrackbarControlCubeMapIntensity.Value = 1F;
			this.floatTrackbarControlCubeMapIntensity.VisibleRangeMax = 4F;
			this.floatTrackbarControlCubeMapIntensity.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlLightElevation_ValueChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 49);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(46, 13);
			this.label6.TabIndex = 3;
			this.label6.Text = "Intensity";
			// 
			// checkBoxEnableMSFactor
			// 
			this.checkBoxEnableMSFactor.AutoSize = true;
			this.checkBoxEnableMSFactor.Checked = true;
			this.checkBoxEnableMSFactor.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnableMSFactor.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxEnableMSFactor.Location = new System.Drawing.Point(1307, 450);
			this.checkBoxEnableMSFactor.Name = "checkBoxEnableMSFactor";
			this.checkBoxEnableMSFactor.Size = new System.Drawing.Size(210, 28);
			this.checkBoxEnableMSFactor.TabIndex = 4;
			this.checkBoxEnableMSFactor.Text = "Enable MS Saturation";
			this.checkBoxEnableMSFactor.UseVisualStyleBackColor = true;
			this.checkBoxEnableMSFactor.CheckedChanged += new System.EventHandler(this.checkBoxEnableMSBRDF_CheckedChanged);
			// 
			// checkBoxUseRealTimeApprox
			// 
			this.checkBoxUseRealTimeApprox.AutoSize = true;
			this.checkBoxUseRealTimeApprox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxUseRealTimeApprox.Location = new System.Drawing.Point(1307, 484);
			this.checkBoxUseRealTimeApprox.Name = "checkBoxUseRealTimeApprox";
			this.checkBoxUseRealTimeApprox.Size = new System.Drawing.Size(281, 28);
			this.checkBoxUseRealTimeApprox.TabIndex = 4;
			this.checkBoxUseRealTimeApprox.Text = "Use Real-Time Approximation";
			this.checkBoxUseRealTimeApprox.UseVisualStyleBackColor = true;
			this.checkBoxUseRealTimeApprox.Visible = false;
			this.checkBoxUseRealTimeApprox.CheckedChanged += new System.EventHandler(this.checkBoxEnableMSBRDF_CheckedChanged);
			// 
			// checkBoxUseLTC
			// 
			this.checkBoxUseLTC.AutoSize = true;
			this.checkBoxUseLTC.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxUseLTC.Location = new System.Drawing.Point(1335, 518);
			this.checkBoxUseLTC.Name = "checkBoxUseLTC";
			this.checkBoxUseLTC.Size = new System.Drawing.Size(102, 28);
			this.checkBoxUseLTC.TabIndex = 4;
			this.checkBoxUseLTC.Text = "Use LTC";
			this.checkBoxUseLTC.UseVisualStyleBackColor = true;
			this.checkBoxUseLTC.Visible = false;
			this.checkBoxUseLTC.CheckedChanged += new System.EventHandler(this.checkBoxEnableMSBRDF_CheckedChanged);
			// 
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1602, 741);
			this.Controls.Add(this.groupBoxEnvironment);
			this.Controls.Add(this.groupBoxPlane);
			this.Controls.Add(this.groupBoxSphere);
			this.Controls.Add(this.checkBoxKeepSampling);
			this.Controls.Add(this.checkBoxPause);
			this.Controls.Add(this.checkBoxUseLTC);
			this.Controls.Add(this.checkBoxUseRealTimeApprox);
			this.Controls.Add(this.checkBoxEnableMSFactor);
			this.Controls.Add(this.checkBoxEnableMSBRDF);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.KeyPreview = true;
			this.Name = "TestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "MSBRDF Test Form";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TestForm_KeyDown);
			this.groupBoxSphere.ResumeLayout(false);
			this.groupBoxSphere.PerformLayout();
			this.groupBoxPlane.ResumeLayout(false);
			this.groupBoxPlane.PerformLayout();
			this.groupBoxEnvironment.ResumeLayout(false);
			this.groupBoxEnvironment.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UIUtility.FloatTrackbarControl floatTrackbarControlRoughnessSphere;
		private UIUtility.PanelOutput panelOutput;
		private UIUtility.FloatTrackbarControl floatTrackbarControlReflectanceGround;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private UIUtility.FloatTrackbarControl floatTrackbarControlLightElevation;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Timer timer1;
		private UIUtility.FloatTrackbarControl floatTrackbarControlRoughnessGround;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox checkBoxEnableMSBRDF;
		private System.Windows.Forms.CheckBox checkBoxKeepSampling;
		private UIUtility.FloatTrackbarControl floatTrackbarControlReflectanceSphere;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.CheckBox checkBoxPause;
		private System.Windows.Forms.GroupBox groupBoxSphere;
		private System.Windows.Forms.GroupBox groupBoxPlane;
		private System.Windows.Forms.GroupBox groupBoxEnvironment;
		private UIUtility.FloatTrackbarControl floatTrackbarControlCubeMapIntensity;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.CheckBox checkBoxEnableMSFactor;
		private System.Windows.Forms.Label label8;
		private UIUtility.FloatTrackbarControl floatTrackbarControlRoughnessSphere2;
		private UIUtility.FloatTrackbarControl floatTrackbarControlReflectanceSphere2;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox checkBoxUseRealTimeApprox;
		private System.Windows.Forms.CheckBox checkBoxUseLTC;
	}
}

