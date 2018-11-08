namespace AreaLightTest
{
	partial class AreaLightForm
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
			this.floatTrackbarControlIlluminance = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.buttonReload = new System.Windows.Forms.Button();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLightPosX = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label7 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLightPosY = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label8 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLightPosZ = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label9 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLightRoll = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label13 = new System.Windows.Forms.Label();
			this.panelOutput = new AreaLightTest.PanelOutput(this.components);
			this.floatTrackbarControlLightScaleX = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label17 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLightScaleY = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// floatTrackbarControlIlluminance
			// 
			this.floatTrackbarControlIlluminance.Location = new System.Drawing.Point(1180, 12);
			this.floatTrackbarControlIlluminance.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlIlluminance.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlIlluminance.Name = "floatTrackbarControlIlluminance";
			this.floatTrackbarControlIlluminance.RangeMax = 1F;
			this.floatTrackbarControlIlluminance.RangeMin = 0F;
			this.floatTrackbarControlIlluminance.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlIlluminance.TabIndex = 1;
			this.floatTrackbarControlIlluminance.Value = 1F;
			this.floatTrackbarControlIlluminance.VisibleRangeMax = 1F;
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(1305, 629);
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
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(1059, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(60, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Illuminance";
			// 
			// floatTrackbarControlLightPosX
			// 
			this.floatTrackbarControlLightPosX.Location = new System.Drawing.Point(1180, 70);
			this.floatTrackbarControlLightPosX.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightPosX.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightPosX.Name = "floatTrackbarControlLightPosX";
			this.floatTrackbarControlLightPosX.RangeMax = 100F;
			this.floatTrackbarControlLightPosX.RangeMin = -100F;
			this.floatTrackbarControlLightPosX.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightPosX.TabIndex = 1;
			this.floatTrackbarControlLightPosX.Value = 0F;
			this.floatTrackbarControlLightPosX.VisibleRangeMax = 3F;
			this.floatTrackbarControlLightPosX.VisibleRangeMin = -3F;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(1059, 75);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(61, 13);
			this.label7.TabIndex = 3;
			this.label7.Text = "Light Pos X";
			// 
			// floatTrackbarControlLightPosY
			// 
			this.floatTrackbarControlLightPosY.Location = new System.Drawing.Point(1180, 96);
			this.floatTrackbarControlLightPosY.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightPosY.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightPosY.Name = "floatTrackbarControlLightPosY";
			this.floatTrackbarControlLightPosY.RangeMax = 100F;
			this.floatTrackbarControlLightPosY.RangeMin = -100F;
			this.floatTrackbarControlLightPosY.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightPosY.TabIndex = 1;
			this.floatTrackbarControlLightPosY.Value = 2F;
			this.floatTrackbarControlLightPosY.VisibleRangeMax = 4F;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(1059, 101);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(61, 13);
			this.label8.TabIndex = 3;
			this.label8.Text = "Light Pos Y";
			// 
			// floatTrackbarControlLightPosZ
			// 
			this.floatTrackbarControlLightPosZ.Location = new System.Drawing.Point(1180, 122);
			this.floatTrackbarControlLightPosZ.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightPosZ.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightPosZ.Name = "floatTrackbarControlLightPosZ";
			this.floatTrackbarControlLightPosZ.RangeMax = 100F;
			this.floatTrackbarControlLightPosZ.RangeMin = -100F;
			this.floatTrackbarControlLightPosZ.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightPosZ.TabIndex = 1;
			this.floatTrackbarControlLightPosZ.Value = 0F;
			this.floatTrackbarControlLightPosZ.VisibleRangeMax = 3F;
			this.floatTrackbarControlLightPosZ.VisibleRangeMin = -3F;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(1059, 127);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(61, 13);
			this.label9.TabIndex = 3;
			this.label9.Text = "Light Pos Z";
			// 
			// floatTrackbarControlLightRoll
			// 
			this.floatTrackbarControlLightRoll.Location = new System.Drawing.Point(1180, 238);
			this.floatTrackbarControlLightRoll.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightRoll.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightRoll.Name = "floatTrackbarControlLightRoll";
			this.floatTrackbarControlLightRoll.RangeMax = 180F;
			this.floatTrackbarControlLightRoll.RangeMin = -180F;
			this.floatTrackbarControlLightRoll.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightRoll.TabIndex = 1;
			this.floatTrackbarControlLightRoll.Value = 0F;
			this.floatTrackbarControlLightRoll.VisibleRangeMax = 180F;
			this.floatTrackbarControlLightRoll.VisibleRangeMin = -180F;
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(1059, 240);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(51, 13);
			this.label13.TabIndex = 3;
			this.label13.Text = "Light Roll";
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1024, 640);
			this.panelOutput.TabIndex = 0;
			this.panelOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseDown);
			this.panelOutput.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseMove);
			this.panelOutput.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseUp);
			// 
			// floatTrackbarControlLightScaleX
			// 
			this.floatTrackbarControlLightScaleX.Location = new System.Drawing.Point(1180, 172);
			this.floatTrackbarControlLightScaleX.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightScaleX.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightScaleX.Name = "floatTrackbarControlLightScaleX";
			this.floatTrackbarControlLightScaleX.RangeMax = 100000F;
			this.floatTrackbarControlLightScaleX.RangeMin = 0.001F;
			this.floatTrackbarControlLightScaleX.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightScaleX.TabIndex = 1;
			this.floatTrackbarControlLightScaleX.Value = 1F;
			this.floatTrackbarControlLightScaleX.VisibleRangeMax = 2F;
			this.floatTrackbarControlLightScaleX.VisibleRangeMin = 0.001F;
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(1059, 175);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(70, 13);
			this.label17.TabIndex = 3;
			this.label17.Text = "Light Scale X";
			// 
			// floatTrackbarControlLightScaleY
			// 
			this.floatTrackbarControlLightScaleY.Location = new System.Drawing.Point(1180, 198);
			this.floatTrackbarControlLightScaleY.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLightScaleY.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLightScaleY.Name = "floatTrackbarControlLightScaleY";
			this.floatTrackbarControlLightScaleY.RangeMax = 100000F;
			this.floatTrackbarControlLightScaleY.RangeMin = 0.001F;
			this.floatTrackbarControlLightScaleY.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLightScaleY.TabIndex = 1;
			this.floatTrackbarControlLightScaleY.Value = 1F;
			this.floatTrackbarControlLightScaleY.VisibleRangeMax = 2F;
			this.floatTrackbarControlLightScaleY.VisibleRangeMin = 0.001F;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(1059, 201);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(70, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Light Scale Y";
			// 
			// AreaLightForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1392, 665);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label17);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.floatTrackbarControlLightPosZ);
			this.Controls.Add(this.floatTrackbarControlLightPosY);
			this.Controls.Add(this.floatTrackbarControlLightRoll);
			this.Controls.Add(this.floatTrackbarControlLightScaleY);
			this.Controls.Add(this.floatTrackbarControlLightScaleX);
			this.Controls.Add(this.floatTrackbarControlLightPosX);
			this.Controls.Add(this.floatTrackbarControlIlluminance);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "AreaLightForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Area Light Test";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelOutput;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlIlluminance;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightPosX;
		private System.Windows.Forms.Label label7;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightPosY;
		private System.Windows.Forms.Label label8;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightPosZ;
		private System.Windows.Forms.Label label9;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightRoll;
		private System.Windows.Forms.Label label13;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightScaleX;
		private System.Windows.Forms.Label label17;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLightScaleY;
		private System.Windows.Forms.Label label2;
	}
}

