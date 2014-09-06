namespace Mie2QuantileFunction
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
			this.radioButtonPolar = new System.Windows.Forms.RadioButton();
			this.radioButtonLog = new System.Windows.Forms.RadioButton();
			this.radioButtonQuantilesPeak = new System.Windows.Forms.RadioButton();
			this.radioButtonQuantilesOffPeak = new System.Windows.Forms.RadioButton();
			this.floatTrackbarControlPeakAngle = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.radioButtonScattering = new System.Windows.Forms.RadioButton();
			this.buttonReShoot = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.panelOutputScattering = new Mie2QuantileFunction.OutputPanel(this.components);
			this.SuspendLayout();
			// 
			// radioButtonPolar
			// 
			this.radioButtonPolar.AutoSize = true;
			this.radioButtonPolar.Checked = true;
			this.radioButtonPolar.Location = new System.Drawing.Point(662, 12);
			this.radioButtonPolar.Name = "radioButtonPolar";
			this.radioButtonPolar.Size = new System.Drawing.Size(91, 17);
			this.radioButtonPolar.TabIndex = 1;
			this.radioButtonPolar.TabStop = true;
			this.radioButtonPolar.Text = "Polar Log Plot";
			this.radioButtonPolar.UseVisualStyleBackColor = true;
			this.radioButtonPolar.CheckedChanged += new System.EventHandler(this.radioButtonPolar_CheckedChanged);
			// 
			// radioButtonLog
			// 
			this.radioButtonLog.AutoSize = true;
			this.radioButtonLog.Location = new System.Drawing.Point(662, 35);
			this.radioButtonLog.Name = "radioButtonLog";
			this.radioButtonLog.Size = new System.Drawing.Size(64, 17);
			this.radioButtonLog.TabIndex = 1;
			this.radioButtonLog.Text = "Log Plot";
			this.radioButtonLog.UseVisualStyleBackColor = true;
			this.radioButtonLog.CheckedChanged += new System.EventHandler(this.radioButtonLog_CheckedChanged);
			// 
			// radioButtonQuantilesPeak
			// 
			this.radioButtonQuantilesPeak.AutoSize = true;
			this.radioButtonQuantilesPeak.Location = new System.Drawing.Point(662, 58);
			this.radioButtonQuantilesPeak.Name = "radioButtonQuantilesPeak";
			this.radioButtonQuantilesPeak.Size = new System.Drawing.Size(102, 17);
			this.radioButtonQuantilesPeak.TabIndex = 1;
			this.radioButtonQuantilesPeak.Text = "Quantiles (peak)";
			this.radioButtonQuantilesPeak.UseVisualStyleBackColor = true;
			this.radioButtonQuantilesPeak.CheckedChanged += new System.EventHandler(this.radioButtonBuckets_CheckedChanged);
			// 
			// radioButtonQuantilesOffPeak
			// 
			this.radioButtonQuantilesOffPeak.AutoSize = true;
			this.radioButtonQuantilesOffPeak.Location = new System.Drawing.Point(662, 81);
			this.radioButtonQuantilesOffPeak.Name = "radioButtonQuantilesOffPeak";
			this.radioButtonQuantilesOffPeak.Size = new System.Drawing.Size(117, 17);
			this.radioButtonQuantilesOffPeak.TabIndex = 1;
			this.radioButtonQuantilesOffPeak.Text = "Quantiles (off-peak)";
			this.radioButtonQuantilesOffPeak.UseVisualStyleBackColor = true;
			this.radioButtonQuantilesOffPeak.CheckedChanged += new System.EventHandler(this.radioButtonQuantilesOffPeak_CheckedChanged);
			// 
			// floatTrackbarControlPeakAngle
			// 
			this.floatTrackbarControlPeakAngle.Location = new System.Drawing.Point(12, 498);
			this.floatTrackbarControlPeakAngle.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlPeakAngle.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlPeakAngle.Name = "floatTrackbarControlPeakAngle";
			this.floatTrackbarControlPeakAngle.RangeMax = 180F;
			this.floatTrackbarControlPeakAngle.RangeMin = 0F;
			this.floatTrackbarControlPeakAngle.Size = new System.Drawing.Size(263, 20);
			this.floatTrackbarControlPeakAngle.TabIndex = 2;
			this.floatTrackbarControlPeakAngle.Value = 5F;
			this.floatTrackbarControlPeakAngle.VisibleRangeMax = 20F;
			this.floatTrackbarControlPeakAngle.SliderDragStop += new Nuaj.Cirrus.Utility.FloatTrackbarControl.SliderDragStopEventHandler(this.floatTrackbarControlPeakAngle_SliderDragStop);
			// 
			// radioButtonScattering
			// 
			this.radioButtonScattering.AutoSize = true;
			this.radioButtonScattering.Location = new System.Drawing.Point(662, 104);
			this.radioButtonScattering.Name = "radioButtonScattering";
			this.radioButtonScattering.Size = new System.Drawing.Size(124, 17);
			this.radioButtonScattering.TabIndex = 1;
			this.radioButtonScattering.Text = "Scattering Simulation";
			this.radioButtonScattering.UseVisualStyleBackColor = true;
			this.radioButtonScattering.CheckedChanged += new System.EventHandler(this.radioButtonScattering_CheckedChanged);
			// 
			// buttonReShoot
			// 
			this.buttonReShoot.Location = new System.Drawing.Point(689, 127);
			this.buttonReShoot.Name = "buttonReShoot";
			this.buttonReShoot.Size = new System.Drawing.Size(75, 23);
			this.buttonReShoot.TabIndex = 3;
			this.buttonReShoot.Text = "Reshoot";
			this.buttonReShoot.UseVisualStyleBackColor = true;
			this.buttonReShoot.Click += new System.EventHandler(this.buttonReShoot_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 479);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(81, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Peak Cut Angle";
			// 
			// panelOutputScattering
			// 
			this.panelOutputScattering.DisplayType = Mie2QuantileFunction.OutputPanel.DISPLAY_TYPE.LOG;
			this.panelOutputScattering.Location = new System.Drawing.Point(12, 12);
			this.panelOutputScattering.Name = "panelOutputScattering";
			this.panelOutputScattering.Phase = null;
			this.panelOutputScattering.PhaseQuantilesOffPeak = null;
			this.panelOutputScattering.PhaseQuantilesPeak = null;
			this.panelOutputScattering.Size = new System.Drawing.Size(644, 455);
			this.panelOutputScattering.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(790, 582);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonReShoot);
			this.Controls.Add(this.floatTrackbarControlPeakAngle);
			this.Controls.Add(this.radioButtonScattering);
			this.Controls.Add(this.radioButtonQuantilesOffPeak);
			this.Controls.Add(this.radioButtonQuantilesPeak);
			this.Controls.Add(this.radioButtonLog);
			this.Controls.Add(this.radioButtonPolar);
			this.Controls.Add(this.panelOutputScattering);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OutputPanel panelOutputScattering;
		private System.Windows.Forms.RadioButton radioButtonPolar;
		private System.Windows.Forms.RadioButton radioButtonLog;
		private System.Windows.Forms.RadioButton radioButtonQuantilesPeak;
		private System.Windows.Forms.RadioButton radioButtonQuantilesOffPeak;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlPeakAngle;
		private System.Windows.Forms.RadioButton radioButtonScattering;
		private System.Windows.Forms.Button buttonReShoot;
		private System.Windows.Forms.Label label1;
	}
}

