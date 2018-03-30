namespace TestForm
{
	partial class MarsLanderForm
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
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.checkBoxRun = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControl1 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControl2 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControl3 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControl4 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.panelOutput = new TestForm.PanelOutput(this.components);
			this.floatTrackbarControl5 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControl6 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.SuspendLayout();
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = 500;
			// 
			// checkBoxRun
			// 
			this.checkBoxRun.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBoxRun.AutoSize = true;
			this.checkBoxRun.Location = new System.Drawing.Point(12, 645);
			this.checkBoxRun.Name = "checkBoxRun";
			this.checkBoxRun.Size = new System.Drawing.Size(41, 23);
			this.checkBoxRun.TabIndex = 1;
			this.checkBoxRun.Text = "RUN";
			this.checkBoxRun.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControl1
			// 
			this.floatTrackbarControl1.Location = new System.Drawing.Point(1182, 12);
			this.floatTrackbarControl1.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl1.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl1.Name = "floatTrackbarControl1";
			this.floatTrackbarControl1.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControl1.TabIndex = 2;
			this.floatTrackbarControl1.Value = -3.676F;
			this.floatTrackbarControl1.VisibleRangeMin = -7.352F;
			this.floatTrackbarControl1.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControl1_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(1057, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(35, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "label1";
			// 
			// floatTrackbarControl2
			// 
			this.floatTrackbarControl2.Location = new System.Drawing.Point(1182, 38);
			this.floatTrackbarControl2.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl2.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl2.Name = "floatTrackbarControl2";
			this.floatTrackbarControl2.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControl2.TabIndex = 2;
			this.floatTrackbarControl2.Value = -15.37F;
			this.floatTrackbarControl2.VisibleRangeMin = -30.74F;
			this.floatTrackbarControl2.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControl1_ValueChanged);
			// 
			// floatTrackbarControl3
			// 
			this.floatTrackbarControl3.Location = new System.Drawing.Point(1182, 64);
			this.floatTrackbarControl3.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl3.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl3.Name = "floatTrackbarControl3";
			this.floatTrackbarControl3.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControl3.TabIndex = 2;
			this.floatTrackbarControl3.Value = -0.3676F;
			this.floatTrackbarControl3.VisibleRangeMin = -0.7352F;
			this.floatTrackbarControl3.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControl1_ValueChanged);
			// 
			// floatTrackbarControl4
			// 
			this.floatTrackbarControl4.Location = new System.Drawing.Point(1182, 90);
			this.floatTrackbarControl4.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl4.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl4.Name = "floatTrackbarControl4";
			this.floatTrackbarControl4.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControl4.TabIndex = 2;
			this.floatTrackbarControl4.Value = -15.15F;
			this.floatTrackbarControl4.VisibleRangeMin = -30.3F;
			this.floatTrackbarControl4.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControl1_ValueChanged);
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1025, 631);
			this.panelOutput.TabIndex = 0;
			this.panelOutput.OnUpdateBitmap += new TestForm.PanelOutput.PaintBitmap(this.panelOutput_OnUpdateBitmap);
			// 
			// floatTrackbarControl5
			// 
			this.floatTrackbarControl5.Location = new System.Drawing.Point(1182, 116);
			this.floatTrackbarControl5.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl5.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl5.Name = "floatTrackbarControl5";
			this.floatTrackbarControl5.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControl5.TabIndex = 2;
			this.floatTrackbarControl5.Value = 4.853F;
			this.floatTrackbarControl5.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControl1_ValueChanged);
			// 
			// floatTrackbarControl6
			// 
			this.floatTrackbarControl6.Location = new System.Drawing.Point(1182, 142);
			this.floatTrackbarControl6.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl6.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl6.Name = "floatTrackbarControl6";
			this.floatTrackbarControl6.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControl6.TabIndex = 2;
			this.floatTrackbarControl6.Value = 1F;
			this.floatTrackbarControl6.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControl1_ValueChanged);
			// 
			// MarsLanderForm
			// 
			this.ClientSize = new System.Drawing.Size(1405, 670);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControl6);
			this.Controls.Add(this.floatTrackbarControl5);
			this.Controls.Add(this.floatTrackbarControl4);
			this.Controls.Add(this.floatTrackbarControl3);
			this.Controls.Add(this.floatTrackbarControl2);
			this.Controls.Add(this.floatTrackbarControl1);
			this.Controls.Add(this.checkBoxRun);
			this.Controls.Add(this.panelOutput);
			this.Name = "MarsLanderForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelOutput;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.CheckBox checkBoxRun;
		private System.Windows.Forms.Label label1;
		internal Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControl1;
		internal Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControl2;
		internal Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControl3;
		internal Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControl4;
		internal Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControl5;
		internal Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControl6;
	}
}

