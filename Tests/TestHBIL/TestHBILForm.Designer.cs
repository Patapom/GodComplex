namespace TestHBIL
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
			this.floatTrackbarControlProjectionDiffusion = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.buttonReload = new System.Windows.Forms.Button();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlProjectionTheta = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlProjectionPhi = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlEnvironmentIntensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLightIntensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.floatTrackbarControlMetal = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label6 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.checkBoxAnimate = new System.Windows.Forms.CheckBox();
			this.checkBoxPause = new System.Windows.Forms.CheckBox();
			this.buttonClear = new System.Windows.Forms.Button();
			this.panelOutput = new TestHBIL.PanelOutput(this.components);
			this.checkBoxEnableHBIL = new System.Windows.Forms.CheckBox();
			this.checkBoxEnableBentNormal = new System.Windows.Forms.CheckBox();
			this.checkBoxEnableConeVisibility = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// floatTrackbarControlProjectionDiffusion
			// 
			this.floatTrackbarControlProjectionDiffusion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlProjectionDiffusion.Location = new System.Drawing.Point(1427, 12);
			this.floatTrackbarControlProjectionDiffusion.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlProjectionDiffusion.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlProjectionDiffusion.Name = "floatTrackbarControlProjectionDiffusion";
			this.floatTrackbarControlProjectionDiffusion.RangeMax = 1F;
			this.floatTrackbarControlProjectionDiffusion.RangeMin = 0F;
			this.floatTrackbarControlProjectionDiffusion.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlProjectionDiffusion.TabIndex = 1;
			this.floatTrackbarControlProjectionDiffusion.Value = 1F;
			this.floatTrackbarControlProjectionDiffusion.VisibleRangeMax = 1F;
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(1552, 709);
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
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(1306, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(98, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Projection Diffusion";
			// 
			// floatTrackbarControlProjectionTheta
			// 
			this.floatTrackbarControlProjectionTheta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlProjectionTheta.Location = new System.Drawing.Point(1427, 38);
			this.floatTrackbarControlProjectionTheta.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlProjectionTheta.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlProjectionTheta.Name = "floatTrackbarControlProjectionTheta";
			this.floatTrackbarControlProjectionTheta.RangeMax = 90F;
			this.floatTrackbarControlProjectionTheta.RangeMin = -90F;
			this.floatTrackbarControlProjectionTheta.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlProjectionTheta.TabIndex = 1;
			this.floatTrackbarControlProjectionTheta.Value = -22.5F;
			this.floatTrackbarControlProjectionTheta.VisibleRangeMax = 90F;
			this.floatTrackbarControlProjectionTheta.VisibleRangeMin = -90F;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(1306, 43);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(85, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Projection Theta";
			// 
			// floatTrackbarControlProjectionPhi
			// 
			this.floatTrackbarControlProjectionPhi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlProjectionPhi.Location = new System.Drawing.Point(1427, 64);
			this.floatTrackbarControlProjectionPhi.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlProjectionPhi.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlProjectionPhi.Name = "floatTrackbarControlProjectionPhi";
			this.floatTrackbarControlProjectionPhi.RangeMax = 180F;
			this.floatTrackbarControlProjectionPhi.RangeMin = -180F;
			this.floatTrackbarControlProjectionPhi.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlProjectionPhi.TabIndex = 1;
			this.floatTrackbarControlProjectionPhi.Value = 0F;
			this.floatTrackbarControlProjectionPhi.VisibleRangeMax = 180F;
			this.floatTrackbarControlProjectionPhi.VisibleRangeMin = -180F;
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(1306, 69);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Projection Phi";
			// 
			// floatTrackbarControlEnvironmentIntensity
			// 
			this.floatTrackbarControlEnvironmentIntensity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlEnvironmentIntensity.Location = new System.Drawing.Point(1427, 634);
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
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(1306, 154);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(33, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Gloss";
			// 
			// floatTrackbarControlLightIntensity
			// 
			this.floatTrackbarControlLightIntensity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlLightIntensity.Location = new System.Drawing.Point(1427, 90);
			this.floatTrackbarControlLightIntensity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightIntensity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightIntensity.Name = "floatTrackbarControlLightIntensity";
			this.floatTrackbarControlLightIntensity.RangeMax = 1000F;
			this.floatTrackbarControlLightIntensity.RangeMin = 0F;
			this.floatTrackbarControlLightIntensity.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightIntensity.TabIndex = 1;
			this.floatTrackbarControlLightIntensity.Value = 10F;
			this.floatTrackbarControlLightIntensity.VisibleRangeMax = 100F;
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(1306, 95);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(72, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "Light Intensity";
			// 
			// floatTrackbarControlMetal
			// 
			this.floatTrackbarControlMetal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlMetal.Location = new System.Drawing.Point(1427, 173);
			this.floatTrackbarControlMetal.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlMetal.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlMetal.Name = "floatTrackbarControlMetal";
			this.floatTrackbarControlMetal.RangeMax = 1F;
			this.floatTrackbarControlMetal.RangeMin = 0F;
			this.floatTrackbarControlMetal.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlMetal.TabIndex = 1;
			this.floatTrackbarControlMetal.Value = 0F;
			this.floatTrackbarControlMetal.VisibleRangeMax = 1F;
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(1306, 638);
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
			this.panel1.Location = new System.Drawing.Point(1441, 688);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(186, 12);
			this.panel1.TabIndex = 5;
			// 
			// checkBoxAnimate
			// 
			this.checkBoxAnimate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxAnimate.AutoSize = true;
			this.checkBoxAnimate.Location = new System.Drawing.Point(1309, 687);
			this.checkBoxAnimate.Name = "checkBoxAnimate";
			this.checkBoxAnimate.Size = new System.Drawing.Size(64, 17);
			this.checkBoxAnimate.TabIndex = 4;
			this.checkBoxAnimate.Text = "Animate";
			this.checkBoxAnimate.UseVisualStyleBackColor = true;
			// 
			// checkBoxPause
			// 
			this.checkBoxPause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxPause.AutoSize = true;
			this.checkBoxPause.Location = new System.Drawing.Point(1379, 687);
			this.checkBoxPause.Name = "checkBoxPause";
			this.checkBoxPause.Size = new System.Drawing.Size(56, 17);
			this.checkBoxPause.TabIndex = 4;
			this.checkBoxPause.Text = "Pause";
			this.checkBoxPause.UseVisualStyleBackColor = true;
			// 
			// buttonClear
			// 
			this.buttonClear.Location = new System.Drawing.Point(1309, 711);
			this.buttonClear.Name = "buttonClear";
			this.buttonClear.Size = new System.Drawing.Size(75, 23);
			this.buttonClear.TabIndex = 6;
			this.buttonClear.Text = "Clear";
			this.buttonClear.UseVisualStyleBackColor = true;
			this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
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
			// checkBoxEnableHBIL
			// 
			this.checkBoxEnableHBIL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxEnableHBIL.AutoSize = true;
			this.checkBoxEnableHBIL.Checked = true;
			this.checkBoxEnableHBIL.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnableHBIL.Location = new System.Drawing.Point(1309, 664);
			this.checkBoxEnableHBIL.Name = "checkBoxEnableHBIL";
			this.checkBoxEnableHBIL.Size = new System.Drawing.Size(50, 17);
			this.checkBoxEnableHBIL.TabIndex = 7;
			this.checkBoxEnableHBIL.Text = "HBIL";
			this.checkBoxEnableHBIL.UseVisualStyleBackColor = true;
			// 
			// checkBoxEnableBentNormal
			// 
			this.checkBoxEnableBentNormal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxEnableBentNormal.AutoSize = true;
			this.checkBoxEnableBentNormal.Checked = true;
			this.checkBoxEnableBentNormal.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnableBentNormal.Location = new System.Drawing.Point(1379, 664);
			this.checkBoxEnableBentNormal.Name = "checkBoxEnableBentNormal";
			this.checkBoxEnableBentNormal.Size = new System.Drawing.Size(106, 17);
			this.checkBoxEnableBentNormal.TabIndex = 8;
			this.checkBoxEnableBentNormal.Text = "Use Bent Normal";
			this.checkBoxEnableBentNormal.UseVisualStyleBackColor = true;
			// 
			// checkBoxEnableConeVisibility
			// 
			this.checkBoxEnableConeVisibility.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxEnableConeVisibility.AutoSize = true;
			this.checkBoxEnableConeVisibility.Checked = true;
			this.checkBoxEnableConeVisibility.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnableConeVisibility.Location = new System.Drawing.Point(1491, 664);
			this.checkBoxEnableConeVisibility.Name = "checkBoxEnableConeVisibility";
			this.checkBoxEnableConeVisibility.Size = new System.Drawing.Size(108, 17);
			this.checkBoxEnableConeVisibility.TabIndex = 8;
			this.checkBoxEnableConeVisibility.Text = "Use Cone Angles";
			this.checkBoxEnableConeVisibility.UseVisualStyleBackColor = true;
			// 
			// TestHBILForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1639, 744);
			this.Controls.Add(this.floatTrackbarControlEnvironmentIntensity);
			this.Controls.Add(this.checkBoxEnableConeVisibility);
			this.Controls.Add(this.checkBoxEnableBentNormal);
			this.Controls.Add(this.checkBoxEnableHBIL);
			this.Controls.Add(this.buttonClear);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.floatTrackbarControlMetal);
			this.Controls.Add(this.floatTrackbarControlLightIntensity);
			this.Controls.Add(this.floatTrackbarControlProjectionPhi);
			this.Controls.Add(this.floatTrackbarControlProjectionTheta);
			this.Controls.Add(this.floatTrackbarControlProjectionDiffusion);
			this.Controls.Add(this.checkBoxPause);
			this.Controls.Add(this.checkBoxAnimate);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "TestHBILForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "HBIL Test";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelOutput;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlProjectionDiffusion;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlProjectionTheta;
		private System.Windows.Forms.Label label2;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlProjectionPhi;
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlEnvironmentIntensity;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightIntensity;
		private System.Windows.Forms.Label label5;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlMetal;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.CheckBox checkBoxAnimate;
		private System.Windows.Forms.CheckBox checkBoxPause;
		private System.Windows.Forms.Button buttonClear;
		private System.Windows.Forms.CheckBox checkBoxEnableHBIL;
		private System.Windows.Forms.CheckBox checkBoxEnableBentNormal;
		private System.Windows.Forms.CheckBox checkBoxEnableConeVisibility;
	}
}

