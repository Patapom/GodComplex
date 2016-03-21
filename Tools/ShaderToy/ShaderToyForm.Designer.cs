namespace ShaderToy
{
	partial class ShaderToyFor
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
				m_Device.Dispose();
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
			this.buttonReload = new System.Windows.Forms.Button();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.checkBoxShowWeights = new System.Windows.Forms.CheckBox();
			this.checkBoxSmoothStep = new System.Windows.Forms.CheckBox();
			this.checkBoxShowOrder3 = new System.Windows.Forms.CheckBox();
			this.checkBoxShowOnlyMS = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlParm = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlWeightMultiplier = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlGlassThickness = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlGlassRoughness = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.floatTrackbarControlGlassF0 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.floatTrackbarControlGlassOpacity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label6 = new System.Windows.Forms.Label();
			this.floatTrackbarControlGlassColoringFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label7 = new System.Windows.Forms.Label();
			this.floatTrackbarControlGlassCurvature = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.outputPanelFermat1 = new ShaderToy.OutputPanelFermat(this.components);
			this.panelOutput = new ShaderToy.PanelOutput(this.components);
			this.SuspendLayout();
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(1232, 629);
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
			// checkBoxShowWeights
			// 
			this.checkBoxShowWeights.AutoSize = true;
			this.checkBoxShowWeights.Location = new System.Drawing.Point(1020, 12);
			this.checkBoxShowWeights.Name = "checkBoxShowWeights";
			this.checkBoxShowWeights.Size = new System.Drawing.Size(110, 17);
			this.checkBoxShowWeights.TabIndex = 3;
			this.checkBoxShowWeights.Text = "Show Roughness";
			this.checkBoxShowWeights.UseVisualStyleBackColor = true;
			// 
			// checkBoxSmoothStep
			// 
			this.checkBoxSmoothStep.AutoSize = true;
			this.checkBoxSmoothStep.Checked = true;
			this.checkBoxSmoothStep.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxSmoothStep.Location = new System.Drawing.Point(1020, 35);
			this.checkBoxSmoothStep.Name = "checkBoxSmoothStep";
			this.checkBoxSmoothStep.Size = new System.Drawing.Size(132, 17);
			this.checkBoxSmoothStep.TabIndex = 3;
			this.checkBoxSmoothStep.Text = "Use MS Diffuse Model";
			this.checkBoxSmoothStep.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowOrder3
			// 
			this.checkBoxShowOrder3.AutoSize = true;
			this.checkBoxShowOrder3.Checked = true;
			this.checkBoxShowOrder3.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowOrder3.Location = new System.Drawing.Point(1020, 58);
			this.checkBoxShowOrder3.Name = "checkBoxShowOrder3";
			this.checkBoxShowOrder3.Size = new System.Drawing.Size(83, 17);
			this.checkBoxShowOrder3.TabIndex = 3;
			this.checkBoxShowOrder3.Text = "Use Order 3";
			this.checkBoxShowOrder3.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowOnlyMS
			// 
			this.checkBoxShowOnlyMS.AutoSize = true;
			this.checkBoxShowOnlyMS.Location = new System.Drawing.Point(1158, 35);
			this.checkBoxShowOnlyMS.Name = "checkBoxShowOnlyMS";
			this.checkBoxShowOnlyMS.Size = new System.Drawing.Size(47, 17);
			this.checkBoxShowOnlyMS.TabIndex = 3;
			this.checkBoxShowOnlyMS.Text = "Only";
			this.checkBoxShowOnlyMS.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControlParm
			// 
			this.floatTrackbarControlParm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlParm.Location = new System.Drawing.Point(1116, 79);
			this.floatTrackbarControlParm.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlParm.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlParm.Name = "floatTrackbarControlParm";
			this.floatTrackbarControlParm.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlParm.TabIndex = 1;
			this.floatTrackbarControlParm.Value = 1F;
			this.floatTrackbarControlParm.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlWeightMultiplier
			// 
			this.floatTrackbarControlWeightMultiplier.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlWeightMultiplier.Location = new System.Drawing.Point(1116, 307);
			this.floatTrackbarControlWeightMultiplier.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlWeightMultiplier.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlWeightMultiplier.Name = "floatTrackbarControlWeightMultiplier";
			this.floatTrackbarControlWeightMultiplier.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlWeightMultiplier.TabIndex = 1;
			this.floatTrackbarControlWeightMultiplier.Value = 0F;
			this.floatTrackbarControlWeightMultiplier.VisibleRangeMax = 1F;
			this.floatTrackbarControlWeightMultiplier.VisibleRangeMin = -1F;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(1016, 83);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(94, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Roughness Factor";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(1016, 125);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(85, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Glass Thickness";
			// 
			// floatTrackbarControlGlassThickness
			// 
			this.floatTrackbarControlGlassThickness.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlGlassThickness.Location = new System.Drawing.Point(1116, 122);
			this.floatTrackbarControlGlassThickness.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlGlassThickness.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlGlassThickness.Name = "floatTrackbarControlGlassThickness";
			this.floatTrackbarControlGlassThickness.RangeMin = 0F;
			this.floatTrackbarControlGlassThickness.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlGlassThickness.TabIndex = 1;
			this.floatTrackbarControlGlassThickness.Value = 0.2F;
			this.floatTrackbarControlGlassThickness.VisibleRangeMax = 1F;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(1016, 203);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(90, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Glass Roughness";
			// 
			// floatTrackbarControlGlassRoughness
			// 
			this.floatTrackbarControlGlassRoughness.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlGlassRoughness.Location = new System.Drawing.Point(1116, 200);
			this.floatTrackbarControlGlassRoughness.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlGlassRoughness.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlGlassRoughness.Name = "floatTrackbarControlGlassRoughness";
			this.floatTrackbarControlGlassRoughness.RangeMax = 1F;
			this.floatTrackbarControlGlassRoughness.RangeMin = 0F;
			this.floatTrackbarControlGlassRoughness.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlGlassRoughness.TabIndex = 1;
			this.floatTrackbarControlGlassRoughness.Value = 0F;
			this.floatTrackbarControlGlassRoughness.VisibleRangeMax = 1F;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(1017, 231);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(48, 13);
			this.label4.TabIndex = 4;
			this.label4.Text = "Glass F0";
			// 
			// floatTrackbarControlGlassF0
			// 
			this.floatTrackbarControlGlassF0.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlGlassF0.Location = new System.Drawing.Point(1116, 226);
			this.floatTrackbarControlGlassF0.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlGlassF0.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlGlassF0.Name = "floatTrackbarControlGlassF0";
			this.floatTrackbarControlGlassF0.RangeMax = 1F;
			this.floatTrackbarControlGlassF0.RangeMin = 0F;
			this.floatTrackbarControlGlassF0.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlGlassF0.TabIndex = 1;
			this.floatTrackbarControlGlassF0.Value = 0.04F;
			this.floatTrackbarControlGlassF0.VisibleRangeMax = 1F;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(1016, 255);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(72, 13);
			this.label5.TabIndex = 4;
			this.label5.Text = "Glass Opacity";
			// 
			// floatTrackbarControlGlassOpacity
			// 
			this.floatTrackbarControlGlassOpacity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlGlassOpacity.Location = new System.Drawing.Point(1116, 252);
			this.floatTrackbarControlGlassOpacity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlGlassOpacity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlGlassOpacity.Name = "floatTrackbarControlGlassOpacity";
			this.floatTrackbarControlGlassOpacity.RangeMax = 1F;
			this.floatTrackbarControlGlassOpacity.RangeMin = 0F;
			this.floatTrackbarControlGlassOpacity.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlGlassOpacity.TabIndex = 1;
			this.floatTrackbarControlGlassOpacity.Value = 0F;
			this.floatTrackbarControlGlassOpacity.VisibleRangeMax = 1F;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(1016, 177);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(74, 13);
			this.label6.TabIndex = 4;
			this.label6.Text = "Glass Coloring";
			// 
			// floatTrackbarControlGlassColoringFactor
			// 
			this.floatTrackbarControlGlassColoringFactor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlGlassColoringFactor.Location = new System.Drawing.Point(1116, 174);
			this.floatTrackbarControlGlassColoringFactor.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlGlassColoringFactor.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlGlassColoringFactor.Name = "floatTrackbarControlGlassColoringFactor";
			this.floatTrackbarControlGlassColoringFactor.RangeMax = 1F;
			this.floatTrackbarControlGlassColoringFactor.RangeMin = 0F;
			this.floatTrackbarControlGlassColoringFactor.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlGlassColoringFactor.TabIndex = 1;
			this.floatTrackbarControlGlassColoringFactor.Value = 1F;
			this.floatTrackbarControlGlassColoringFactor.VisibleRangeMax = 1F;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(1016, 151);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(82, 13);
			this.label7.TabIndex = 4;
			this.label7.Text = "Glass Curvature";
			// 
			// floatTrackbarControlGlassCurvature
			// 
			this.floatTrackbarControlGlassCurvature.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlGlassCurvature.Location = new System.Drawing.Point(1116, 148);
			this.floatTrackbarControlGlassCurvature.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlGlassCurvature.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlGlassCurvature.Name = "floatTrackbarControlGlassCurvature";
			this.floatTrackbarControlGlassCurvature.RangeMax = 1F;
			this.floatTrackbarControlGlassCurvature.RangeMin = -1F;
			this.floatTrackbarControlGlassCurvature.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlGlassCurvature.TabIndex = 1;
			this.floatTrackbarControlGlassCurvature.Value = 0F;
			this.floatTrackbarControlGlassCurvature.VisibleRangeMax = 1F;
			this.floatTrackbarControlGlassCurvature.VisibleRangeMin = -1F;
			// 
			// outputPanelFermat1
			// 
			this.outputPanelFermat1.Location = new System.Drawing.Point(1057, 374);
			this.outputPanelFermat1.Name = "outputPanelFermat1";
			this.outputPanelFermat1.Size = new System.Drawing.Size(228, 208);
			this.outputPanelFermat1.TabIndex = 5;
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1000, 640);
			this.panelOutput.TabIndex = 0;
			this.panelOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseDown);
			this.panelOutput.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseMove);
			this.panelOutput.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseUp);
			this.panelOutput.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.panelOutput_PreviewKeyDown);
			// 
			// ShaderToyFor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1319, 666);
			this.Controls.Add(this.outputPanelFermat1);
			this.Controls.Add(this.checkBoxSmoothStep);
			this.Controls.Add(this.checkBoxShowOnlyMS);
			this.Controls.Add(this.checkBoxShowOrder3);
			this.Controls.Add(this.checkBoxShowWeights);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.floatTrackbarControlParm);
			this.Controls.Add(this.floatTrackbarControlGlassF0);
			this.Controls.Add(this.floatTrackbarControlGlassCurvature);
			this.Controls.Add(this.floatTrackbarControlGlassColoringFactor);
			this.Controls.Add(this.floatTrackbarControlGlassRoughness);
			this.Controls.Add(this.floatTrackbarControlGlassOpacity);
			this.Controls.Add(this.floatTrackbarControlGlassThickness);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.floatTrackbarControlWeightMultiplier);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.panelOutput);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ShaderToyFor";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ShaderToy";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelOutput;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlWeightMultiplier;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.CheckBox checkBoxShowWeights;
		private System.Windows.Forms.CheckBox checkBoxSmoothStep;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlParm;
		private System.Windows.Forms.CheckBox checkBoxShowOrder3;
		private System.Windows.Forms.CheckBox checkBoxShowOnlyMS;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlGlassThickness;
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlGlassRoughness;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlGlassF0;
		private System.Windows.Forms.Label label5;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlGlassOpacity;
		private System.Windows.Forms.Label label6;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlGlassColoringFactor;
		private System.Windows.Forms.Label label7;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlGlassCurvature;
		private OutputPanelFermat outputPanelFermat1;
	}
}

