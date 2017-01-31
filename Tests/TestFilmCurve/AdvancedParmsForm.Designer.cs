namespace TestFilmicCurve
{
	partial class AdvancedParmsForm
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
			this.floatTrackbarControlMinLuminance = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label12 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlMaxLuminance = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlAdaptationSpeedBright = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlAdaptationSpeedDark = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.buttonReset = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// floatTrackbarControlMinLuminance
			// 
			this.floatTrackbarControlMinLuminance.Location = new System.Drawing.Point(175, 15);
			this.floatTrackbarControlMinLuminance.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlMinLuminance.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlMinLuminance.Name = "floatTrackbarControlMinLuminance";
			this.floatTrackbarControlMinLuminance.RangeMax = 1000F;
			this.floatTrackbarControlMinLuminance.RangeMin = 0.0001F;
			this.floatTrackbarControlMinLuminance.Size = new System.Drawing.Size(235, 20);
			this.floatTrackbarControlMinLuminance.TabIndex = 5;
			this.floatTrackbarControlMinLuminance.Value = 0.1F;
			this.floatTrackbarControlMinLuminance.VisibleRangeMax = 1F;
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(12, 19);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(157, 13);
			this.label12.TabIndex = 6;
			this.label12.Text = "Minimum Adaptation Luminance";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 45);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(160, 13);
			this.label1.TabIndex = 6;
			this.label1.Text = "Maximum Adaptation Luminance";
			// 
			// floatTrackbarControlMaxLuminance
			// 
			this.floatTrackbarControlMaxLuminance.Location = new System.Drawing.Point(175, 41);
			this.floatTrackbarControlMaxLuminance.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlMaxLuminance.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlMaxLuminance.Name = "floatTrackbarControlMaxLuminance";
			this.floatTrackbarControlMaxLuminance.RangeMax = 10000F;
			this.floatTrackbarControlMaxLuminance.RangeMin = 0.0001F;
			this.floatTrackbarControlMaxLuminance.Size = new System.Drawing.Size(235, 20);
			this.floatTrackbarControlMaxLuminance.TabIndex = 5;
			this.floatTrackbarControlMaxLuminance.Value = 3000F;
			this.floatTrackbarControlMaxLuminance.VisibleRangeMax = 4000F;
			this.floatTrackbarControlMaxLuminance.VisibleRangeMin = 0.0001F;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 71);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(134, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "Adaptation Speed to Bright";
			// 
			// floatTrackbarControlAdaptationSpeedBright
			// 
			this.floatTrackbarControlAdaptationSpeedBright.Location = new System.Drawing.Point(175, 67);
			this.floatTrackbarControlAdaptationSpeedBright.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlAdaptationSpeedBright.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlAdaptationSpeedBright.Name = "floatTrackbarControlAdaptationSpeedBright";
			this.floatTrackbarControlAdaptationSpeedBright.RangeMax = 1F;
			this.floatTrackbarControlAdaptationSpeedBright.RangeMin = 0F;
			this.floatTrackbarControlAdaptationSpeedBright.Size = new System.Drawing.Size(235, 20);
			this.floatTrackbarControlAdaptationSpeedBright.TabIndex = 5;
			this.floatTrackbarControlAdaptationSpeedBright.Value = 0.99F;
			this.floatTrackbarControlAdaptationSpeedBright.VisibleRangeMax = 1F;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 97);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(130, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Adaptation Speed to Dark";
			// 
			// floatTrackbarControlAdaptationSpeedDark
			// 
			this.floatTrackbarControlAdaptationSpeedDark.Location = new System.Drawing.Point(175, 93);
			this.floatTrackbarControlAdaptationSpeedDark.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlAdaptationSpeedDark.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlAdaptationSpeedDark.Name = "floatTrackbarControlAdaptationSpeedDark";
			this.floatTrackbarControlAdaptationSpeedDark.RangeMax = 1F;
			this.floatTrackbarControlAdaptationSpeedDark.RangeMin = 0F;
			this.floatTrackbarControlAdaptationSpeedDark.Size = new System.Drawing.Size(235, 20);
			this.floatTrackbarControlAdaptationSpeedDark.TabIndex = 5;
			this.floatTrackbarControlAdaptationSpeedDark.Value = 0.99F;
			this.floatTrackbarControlAdaptationSpeedDark.VisibleRangeMax = 1F;
			// 
			// buttonReset
			// 
			this.buttonReset.Location = new System.Drawing.Point(15, 129);
			this.buttonReset.Name = "buttonReset";
			this.buttonReset.Size = new System.Drawing.Size(75, 23);
			this.buttonReset.TabIndex = 7;
			this.buttonReset.Text = "Reset";
			this.buttonReset.UseVisualStyleBackColor = true;
			this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
			// 
			// AdvancedParmsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(425, 172);
			this.Controls.Add(this.buttonReset);
			this.Controls.Add(this.floatTrackbarControlAdaptationSpeedDark);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.floatTrackbarControlAdaptationSpeedBright);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.floatTrackbarControlMaxLuminance);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlMinLuminance);
			this.Controls.Add(this.label12);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "AdvancedParmsForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Advanced Parameters";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button buttonReset;
		public Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlMinLuminance;
		public Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlMaxLuminance;
		public Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlAdaptationSpeedBright;
		public Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlAdaptationSpeedDark;
	}
}