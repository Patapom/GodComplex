namespace OfflineCloudRenderer2
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
			this.buttonReload = new System.Windows.Forms.Button();
			this.floatTrackbarControlDebug0 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlDebug1 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlDebug2 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlDebug3 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.buttonShootPhotons = new System.Windows.Forms.Button();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.floatTrackbarControlCloudscapeThickness = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlSigmaScattering = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.radioButtonShowFlux = new System.Windows.Forms.RadioButton();
			this.radioButtonShowDirection = new System.Windows.Forms.RadioButton();
			this.floatTrackbarControlDisplayIntensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.checkBoxShowNormalized = new System.Windows.Forms.CheckBox();
			this.radioButtonShowDensityField = new System.Windows.Forms.RadioButton();
			this.integerTrackbarControlLayerDisplayStart = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.integerTrackbarControlLayerDisplayEnd = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.viewportPanel = new OfflineCloudRenderer2.ViewportPanel(this.components);
			this.SuspendLayout();
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(881, 715);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(94, 29);
			this.buttonReload.TabIndex = 1;
			this.buttonReload.Text = "Reload Shaders";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// floatTrackbarControlDebug0
			// 
			this.floatTrackbarControlDebug0.Location = new System.Drawing.Point(989, 655);
			this.floatTrackbarControlDebug0.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDebug0.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDebug0.Name = "floatTrackbarControlDebug0";
			this.floatTrackbarControlDebug0.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDebug0.TabIndex = 2;
			this.floatTrackbarControlDebug0.Value = 0F;
			this.floatTrackbarControlDebug0.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlDebug3_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(986, 639);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(39, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Debug";
			// 
			// floatTrackbarControlDebug1
			// 
			this.floatTrackbarControlDebug1.Location = new System.Drawing.Point(989, 681);
			this.floatTrackbarControlDebug1.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDebug1.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDebug1.Name = "floatTrackbarControlDebug1";
			this.floatTrackbarControlDebug1.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDebug1.TabIndex = 2;
			this.floatTrackbarControlDebug1.Value = 0F;
			this.floatTrackbarControlDebug1.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlDebug3_ValueChanged);
			// 
			// floatTrackbarControlDebug2
			// 
			this.floatTrackbarControlDebug2.Location = new System.Drawing.Point(989, 707);
			this.floatTrackbarControlDebug2.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDebug2.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDebug2.Name = "floatTrackbarControlDebug2";
			this.floatTrackbarControlDebug2.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDebug2.TabIndex = 2;
			this.floatTrackbarControlDebug2.Value = 0F;
			this.floatTrackbarControlDebug2.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlDebug3_ValueChanged);
			// 
			// floatTrackbarControlDebug3
			// 
			this.floatTrackbarControlDebug3.Location = new System.Drawing.Point(989, 733);
			this.floatTrackbarControlDebug3.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDebug3.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDebug3.Name = "floatTrackbarControlDebug3";
			this.floatTrackbarControlDebug3.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDebug3.TabIndex = 2;
			this.floatTrackbarControlDebug3.Value = 0F;
			this.floatTrackbarControlDebug3.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlDebug3_ValueChanged);
			// 
			// buttonShootPhotons
			// 
			this.buttonShootPhotons.Location = new System.Drawing.Point(1038, 153);
			this.buttonShootPhotons.Name = "buttonShootPhotons";
			this.buttonShootPhotons.Size = new System.Drawing.Size(93, 23);
			this.buttonShootPhotons.TabIndex = 4;
			this.buttonShootPhotons.Text = "Shoot Photons";
			this.buttonShootPhotons.UseVisualStyleBackColor = true;
			this.buttonShootPhotons.Click += new System.EventHandler(this.buttonShootPhotons_Click);
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(985, 197);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(200, 23);
			this.progressBar1.TabIndex = 5;
			// 
			// floatTrackbarControlCloudscapeThickness
			// 
			this.floatTrackbarControlCloudscapeThickness.Location = new System.Drawing.Point(984, 28);
			this.floatTrackbarControlCloudscapeThickness.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlCloudscapeThickness.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlCloudscapeThickness.Name = "floatTrackbarControlCloudscapeThickness";
			this.floatTrackbarControlCloudscapeThickness.RangeMin = 1F;
			this.floatTrackbarControlCloudscapeThickness.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlCloudscapeThickness.TabIndex = 2;
			this.floatTrackbarControlCloudscapeThickness.Value = 1000F;
			this.floatTrackbarControlCloudscapeThickness.VisibleRangeMax = 1000F;
			this.floatTrackbarControlCloudscapeThickness.VisibleRangeMin = 1F;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(981, 12);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(159, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Total Cloudscape Thickness (m)";
			// 
			// floatTrackbarControlSigmaScattering
			// 
			this.floatTrackbarControlSigmaScattering.Location = new System.Drawing.Point(985, 67);
			this.floatTrackbarControlSigmaScattering.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSigmaScattering.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSigmaScattering.Name = "floatTrackbarControlSigmaScattering";
			this.floatTrackbarControlSigmaScattering.RangeMax = 10F;
			this.floatTrackbarControlSigmaScattering.RangeMin = 0F;
			this.floatTrackbarControlSigmaScattering.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlSigmaScattering.TabIndex = 2;
			this.floatTrackbarControlSigmaScattering.Value = 0.0452389F;
			this.floatTrackbarControlSigmaScattering.VisibleRangeMax = 0.1F;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(982, 51);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(136, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Scattering Coefficient (1/m)";
			// 
			// radioButtonShowFlux
			// 
			this.radioButtonShowFlux.AutoSize = true;
			this.radioButtonShowFlux.Checked = true;
			this.radioButtonShowFlux.Location = new System.Drawing.Point(985, 254);
			this.radioButtonShowFlux.Name = "radioButtonShowFlux";
			this.radioButtonShowFlux.Size = new System.Drawing.Size(74, 17);
			this.radioButtonShowFlux.TabIndex = 6;
			this.radioButtonShowFlux.TabStop = true;
			this.radioButtonShowFlux.Text = "Show Flux";
			this.radioButtonShowFlux.UseVisualStyleBackColor = true;
			this.radioButtonShowFlux.CheckedChanged += new System.EventHandler(this.radioButtonShowFlux_CheckedChanged);
			// 
			// radioButtonShowDirection
			// 
			this.radioButtonShowDirection.AutoSize = true;
			this.radioButtonShowDirection.Location = new System.Drawing.Point(984, 277);
			this.radioButtonShowDirection.Name = "radioButtonShowDirection";
			this.radioButtonShowDirection.Size = new System.Drawing.Size(97, 17);
			this.radioButtonShowDirection.TabIndex = 6;
			this.radioButtonShowDirection.Text = "Show Direction";
			this.radioButtonShowDirection.UseVisualStyleBackColor = true;
			this.radioButtonShowDirection.CheckedChanged += new System.EventHandler(this.radioButtonShowDirection_CheckedChanged);
			// 
			// floatTrackbarControlDisplayIntensity
			// 
			this.floatTrackbarControlDisplayIntensity.Location = new System.Drawing.Point(984, 301);
			this.floatTrackbarControlDisplayIntensity.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDisplayIntensity.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDisplayIntensity.Name = "floatTrackbarControlDisplayIntensity";
			this.floatTrackbarControlDisplayIntensity.RangeMax = 100F;
			this.floatTrackbarControlDisplayIntensity.RangeMin = 0F;
			this.floatTrackbarControlDisplayIntensity.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDisplayIntensity.TabIndex = 2;
			this.floatTrackbarControlDisplayIntensity.Value = 0.01F;
			this.floatTrackbarControlDisplayIntensity.VisibleRangeMax = 0.1F;
			this.floatTrackbarControlDisplayIntensity.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlDisplayIntensity_ValueChanged);
			// 
			// checkBoxShowNormalized
			// 
			this.checkBoxShowNormalized.AutoSize = true;
			this.checkBoxShowNormalized.Checked = true;
			this.checkBoxShowNormalized.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowNormalized.Location = new System.Drawing.Point(1103, 278);
			this.checkBoxShowNormalized.Name = "checkBoxShowNormalized";
			this.checkBoxShowNormalized.Size = new System.Drawing.Size(78, 17);
			this.checkBoxShowNormalized.TabIndex = 7;
			this.checkBoxShowNormalized.Text = "Normalized";
			this.checkBoxShowNormalized.UseVisualStyleBackColor = true;
			this.checkBoxShowNormalized.CheckedChanged += new System.EventHandler(this.checkBoxShowNormalized_CheckedChanged);
			// 
			// radioButtonShowDensityField
			// 
			this.radioButtonShowDensityField.AutoSize = true;
			this.radioButtonShowDensityField.Location = new System.Drawing.Point(984, 327);
			this.radioButtonShowDensityField.Name = "radioButtonShowDensityField";
			this.radioButtonShowDensityField.Size = new System.Drawing.Size(115, 17);
			this.radioButtonShowDensityField.TabIndex = 6;
			this.radioButtonShowDensityField.Text = "Show Density Field";
			this.radioButtonShowDensityField.UseVisualStyleBackColor = true;
			this.radioButtonShowDensityField.CheckedChanged += new System.EventHandler(this.radioButtonShowDirection_CheckedChanged);
			// 
			// integerTrackbarControlLayerDisplayStart
			// 
			this.integerTrackbarControlLayerDisplayStart.Location = new System.Drawing.Point(984, 400);
			this.integerTrackbarControlLayerDisplayStart.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlLayerDisplayStart.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlLayerDisplayStart.Name = "integerTrackbarControlLayerDisplayStart";
			this.integerTrackbarControlLayerDisplayStart.RangeMax = 10000;
			this.integerTrackbarControlLayerDisplayStart.RangeMin = 0;
			this.integerTrackbarControlLayerDisplayStart.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlLayerDisplayStart.TabIndex = 8;
			this.integerTrackbarControlLayerDisplayStart.Value = 0;
			this.integerTrackbarControlLayerDisplayStart.VisibleRangeMax = 10000;
			this.integerTrackbarControlLayerDisplayStart.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlLayerDisplayStart_ValueChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(982, 384);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(119, 13);
			this.label4.TabIndex = 9;
			this.label4.Text = "Layer Display Start/End";
			// 
			// integerTrackbarControlLayerDisplayEnd
			// 
			this.integerTrackbarControlLayerDisplayEnd.Location = new System.Drawing.Point(984, 426);
			this.integerTrackbarControlLayerDisplayEnd.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlLayerDisplayEnd.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlLayerDisplayEnd.Name = "integerTrackbarControlLayerDisplayEnd";
			this.integerTrackbarControlLayerDisplayEnd.RangeMax = 10000;
			this.integerTrackbarControlLayerDisplayEnd.RangeMin = 0;
			this.integerTrackbarControlLayerDisplayEnd.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlLayerDisplayEnd.TabIndex = 8;
			this.integerTrackbarControlLayerDisplayEnd.Value = 0;
			this.integerTrackbarControlLayerDisplayEnd.VisibleRangeMax = 10000;
			this.integerTrackbarControlLayerDisplayEnd.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlLayerDisplayEnd_ValueChanged);
			// 
			// viewportPanel
			// 
			this.viewportPanel.Device = null;
			this.viewportPanel.Location = new System.Drawing.Point(12, 12);
			this.viewportPanel.Name = "viewportPanel";
			this.viewportPanel.Size = new System.Drawing.Size(963, 686);
			this.viewportPanel.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1200, 756);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.integerTrackbarControlLayerDisplayEnd);
			this.Controls.Add(this.integerTrackbarControlLayerDisplayStart);
			this.Controls.Add(this.checkBoxShowNormalized);
			this.Controls.Add(this.radioButtonShowDensityField);
			this.Controls.Add(this.radioButtonShowDirection);
			this.Controls.Add(this.radioButtonShowFlux);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.buttonShootPhotons);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlDebug3);
			this.Controls.Add(this.floatTrackbarControlDebug2);
			this.Controls.Add(this.floatTrackbarControlSigmaScattering);
			this.Controls.Add(this.floatTrackbarControlDebug1);
			this.Controls.Add(this.floatTrackbarControlCloudscapeThickness);
			this.Controls.Add(this.floatTrackbarControlDisplayIntensity);
			this.Controls.Add(this.floatTrackbarControlDebug0);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.viewportPanel);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ViewportPanel viewportPanel;
		private System.Windows.Forms.Button buttonReload;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDebug0;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDebug1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDebug2;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDebug3;
		private System.Windows.Forms.Button buttonShootPhotons;
		private System.Windows.Forms.ProgressBar progressBar1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlCloudscapeThickness;
		private System.Windows.Forms.Label label2;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSigmaScattering;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.RadioButton radioButtonShowFlux;
		private System.Windows.Forms.RadioButton radioButtonShowDirection;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDisplayIntensity;
		private System.Windows.Forms.CheckBox checkBoxShowNormalized;
		private System.Windows.Forms.RadioButton radioButtonShowDensityField;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlLayerDisplayStart;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlLayerDisplayEnd;
	}
}

