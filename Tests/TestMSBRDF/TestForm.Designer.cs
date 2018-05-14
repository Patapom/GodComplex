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
			this.floatTrackbarControlRoughnessSphere = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.panelOutput = new Nuaj.Cirrus.Utility.PanelOutput(this.components);
			this.floatTrackbarControlReflectanceGround = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.buttonReload = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLightElevation = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.floatTrackbarControlRoughnessGround = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.checkBoxEnableMSBRDF = new System.Windows.Forms.CheckBox();
			this.checkBoxKeepSampling = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlReflectanceSphere = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.checkBoxPause = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.floatTrackbarControlCubeMapIntensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label6 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
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
			this.floatTrackbarControlRoughnessSphere.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlRoughnessSpec_ValueChanged);
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
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
			this.floatTrackbarControlReflectanceGround.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlAlbedo_ValueChanged);
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
			this.label1.Size = new System.Drawing.Size(61, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Roughness";
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
			this.floatTrackbarControlLightElevation.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlLightElevation_ValueChanged);
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
			this.floatTrackbarControlRoughnessGround.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlRoughnessDiffuse_ValueChanged);
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
			this.checkBoxEnableMSBRDF.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxEnableMSBRDF.Location = new System.Drawing.Point(1332, 343);
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
			this.floatTrackbarControlReflectanceSphere.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlAlbedo_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 48);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(65, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "Reflectance";
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
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.floatTrackbarControlRoughnessSphere);
			this.groupBox1.Controls.Add(this.floatTrackbarControlReflectanceSphere);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Location = new System.Drawing.Point(1298, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(300, 82);
			this.groupBox1.TabIndex = 5;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Sphere";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.floatTrackbarControlRoughnessGround);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.floatTrackbarControlReflectanceGround);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Location = new System.Drawing.Point(1298, 100);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(300, 83);
			this.groupBox2.TabIndex = 6;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Ground Plane";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.floatTrackbarControlCubeMapIntensity);
			this.groupBox3.Controls.Add(this.label6);
			this.groupBox3.Controls.Add(this.floatTrackbarControlLightElevation);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Location = new System.Drawing.Point(1298, 199);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(300, 99);
			this.groupBox3.TabIndex = 7;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Environment Cube Map";
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
			this.floatTrackbarControlCubeMapIntensity.Value = 2F;
			this.floatTrackbarControlCubeMapIntensity.VisibleRangeMax = 4F;
			this.floatTrackbarControlCubeMapIntensity.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlLightElevation_ValueChanged);
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
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1602, 741);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.checkBoxKeepSampling);
			this.Controls.Add(this.checkBoxPause);
			this.Controls.Add(this.checkBoxEnableMSBRDF);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.KeyPreview = true;
			this.Name = "TestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "MSBRDF Test Form";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TestForm_KeyDown);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlRoughnessSphere;
		private Nuaj.Cirrus.Utility.PanelOutput panelOutput;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlReflectanceGround;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightElevation;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Timer timer1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlRoughnessGround;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox checkBoxEnableMSBRDF;
		private System.Windows.Forms.CheckBox checkBoxKeepSampling;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlReflectanceSphere;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.CheckBox checkBoxPause;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlCubeMapIntensity;
		private System.Windows.Forms.Label label6;
	}
}

