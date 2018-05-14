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
			this.floatTrackbarControlRoughnessSpec = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.panelOutput = new Nuaj.Cirrus.Utility.PanelOutput(this.components);
			this.floatTrackbarControlAlbedo = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.buttonReload = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLightElevation = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.floatTrackbarControlRoughnessDiffuse = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.checkBoxEnableMSBRDF = new System.Windows.Forms.CheckBox();
			this.checkBoxKeepSampling = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlF0 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.checkBoxPause = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// floatTrackbarControlRoughnessSpec
			// 
			this.floatTrackbarControlRoughnessSpec.Location = new System.Drawing.Point(1390, 12);
			this.floatTrackbarControlRoughnessSpec.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlRoughnessSpec.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlRoughnessSpec.Name = "floatTrackbarControlRoughnessSpec";
			this.floatTrackbarControlRoughnessSpec.RangeMin = 0F;
			this.floatTrackbarControlRoughnessSpec.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlRoughnessSpec.TabIndex = 0;
			this.floatTrackbarControlRoughnessSpec.Value = 0.1F;
			this.floatTrackbarControlRoughnessSpec.VisibleRangeMax = 1F;
			this.floatTrackbarControlRoughnessSpec.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlRoughnessSpec_ValueChanged);
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1280, 720);
			this.panelOutput.TabIndex = 1;
			// 
			// floatTrackbarControlAlbedo
			// 
			this.floatTrackbarControlAlbedo.Location = new System.Drawing.Point(1390, 72);
			this.floatTrackbarControlAlbedo.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlAlbedo.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlAlbedo.Name = "floatTrackbarControlAlbedo";
			this.floatTrackbarControlAlbedo.RangeMax = 1F;
			this.floatTrackbarControlAlbedo.RangeMin = 0F;
			this.floatTrackbarControlAlbedo.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlAlbedo.TabIndex = 0;
			this.floatTrackbarControlAlbedo.Value = 0.75F;
			this.floatTrackbarControlAlbedo.VisibleRangeMax = 1F;
			this.floatTrackbarControlAlbedo.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlAlbedo_ValueChanged);
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
			this.label1.Location = new System.Drawing.Point(1298, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(92, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Roughness Spec.";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(1298, 76);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(78, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Ground Albedo";
			// 
			// floatTrackbarControlLightElevation
			// 
			this.floatTrackbarControlLightElevation.Location = new System.Drawing.Point(1390, 131);
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
			this.label3.Location = new System.Drawing.Point(1298, 136);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(61, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Light Theta";
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 10;
			// 
			// floatTrackbarControlRoughnessDiffuse
			// 
			this.floatTrackbarControlRoughnessDiffuse.Location = new System.Drawing.Point(1390, 38);
			this.floatTrackbarControlRoughnessDiffuse.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlRoughnessDiffuse.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlRoughnessDiffuse.Name = "floatTrackbarControlRoughnessDiffuse";
			this.floatTrackbarControlRoughnessDiffuse.RangeMin = 0F;
			this.floatTrackbarControlRoughnessDiffuse.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlRoughnessDiffuse.TabIndex = 0;
			this.floatTrackbarControlRoughnessDiffuse.Value = 0F;
			this.floatTrackbarControlRoughnessDiffuse.VisibleRangeMax = 1F;
			this.floatTrackbarControlRoughnessDiffuse.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlRoughnessDiffuse_ValueChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(1298, 41);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(97, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Roughness Diffuse";
			// 
			// checkBoxEnableMSBRDF
			// 
			this.checkBoxEnableMSBRDF.AutoSize = true;
			this.checkBoxEnableMSBRDF.Location = new System.Drawing.Point(1301, 174);
			this.checkBoxEnableMSBRDF.Name = "checkBoxEnableMSBRDF";
			this.checkBoxEnableMSBRDF.Size = new System.Drawing.Size(146, 17);
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
			this.checkBoxKeepSampling.Location = new System.Drawing.Point(1301, 197);
			this.checkBoxKeepSampling.Name = "checkBoxKeepSampling";
			this.checkBoxKeepSampling.Size = new System.Drawing.Size(97, 17);
			this.checkBoxKeepSampling.TabIndex = 4;
			this.checkBoxKeepSampling.Text = "Keep Sampling";
			this.checkBoxKeepSampling.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControlF0
			// 
			this.floatTrackbarControlF0.Location = new System.Drawing.Point(1390, 98);
			this.floatTrackbarControlF0.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlF0.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlF0.Name = "floatTrackbarControlF0";
			this.floatTrackbarControlF0.RangeMax = 1F;
			this.floatTrackbarControlF0.RangeMin = 0F;
			this.floatTrackbarControlF0.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlF0.TabIndex = 0;
			this.floatTrackbarControlF0.Value = 1F;
			this.floatTrackbarControlF0.VisibleRangeMax = 1F;
			this.floatTrackbarControlF0.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlAlbedo_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(1298, 102);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(56, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "Sphere F0";
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
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1602, 741);
			this.Controls.Add(this.checkBoxKeepSampling);
			this.Controls.Add(this.checkBoxPause);
			this.Controls.Add(this.checkBoxEnableMSBRDF);
			this.Controls.Add(this.floatTrackbarControlRoughnessDiffuse);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput);
			this.Controls.Add(this.floatTrackbarControlLightElevation);
			this.Controls.Add(this.floatTrackbarControlF0);
			this.Controls.Add(this.floatTrackbarControlAlbedo);
			this.Controls.Add(this.floatTrackbarControlRoughnessSpec);
			this.Name = "TestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "MSBRDF Test Form";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlRoughnessSpec;
		private Nuaj.Cirrus.Utility.PanelOutput panelOutput;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlAlbedo;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightElevation;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Timer timer1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlRoughnessDiffuse;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox checkBoxEnableMSBRDF;
		private System.Windows.Forms.CheckBox checkBoxKeepSampling;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlF0;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.CheckBox checkBoxPause;
	}
}

