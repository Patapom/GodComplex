namespace TestPathTracing
{
	partial class PathTracingForm
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
			this.floatTrackbarControlGlossWall = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.integerTrackbarControl1 = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.buttonReload = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlGlossSphere = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.panelOutput3D = new TestPathTracing.PanelOutput3D(this.components);
			this.floatTrackbarControlNoise = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// floatTrackbarControlGlossWall
			// 
			this.floatTrackbarControlGlossWall.Location = new System.Drawing.Point(1388, 12);
			this.floatTrackbarControlGlossWall.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlGlossWall.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlGlossWall.Name = "floatTrackbarControlGlossWall";
			this.floatTrackbarControlGlossWall.RangeMax = 1F;
			this.floatTrackbarControlGlossWall.RangeMin = 0F;
			this.floatTrackbarControlGlossWall.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlGlossWall.TabIndex = 1;
			this.floatTrackbarControlGlossWall.Value = 0.95F;
			this.floatTrackbarControlGlossWall.VisibleRangeMax = 1F;
			// 
			// integerTrackbarControl1
			// 
			this.integerTrackbarControl1.Location = new System.Drawing.Point(1371, 127);
			this.integerTrackbarControl1.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControl1.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControl1.Name = "integerTrackbarControl1";
			this.integerTrackbarControl1.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControl1.TabIndex = 2;
			this.integerTrackbarControl1.Value = 0;
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = 10;
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(1513, 706);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 0;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(1314, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Gloss Wall";
			// 
			// floatTrackbarControlGlossSphere
			// 
			this.floatTrackbarControlGlossSphere.Location = new System.Drawing.Point(1388, 38);
			this.floatTrackbarControlGlossSphere.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlGlossSphere.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlGlossSphere.Name = "floatTrackbarControlGlossSphere";
			this.floatTrackbarControlGlossSphere.RangeMax = 1F;
			this.floatTrackbarControlGlossSphere.RangeMin = 0F;
			this.floatTrackbarControlGlossSphere.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlGlossSphere.TabIndex = 1;
			this.floatTrackbarControlGlossSphere.Value = 0.95F;
			this.floatTrackbarControlGlossSphere.VisibleRangeMax = 1F;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(1314, 42);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(70, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Gloss Sphere";
			// 
			// panelOutput3D
			// 
			this.panelOutput3D.Location = new System.Drawing.Point(12, 12);
			this.panelOutput3D.Name = "panelOutput3D";
			this.panelOutput3D.Size = new System.Drawing.Size(1280, 720);
			this.panelOutput3D.TabIndex = 3;
			// 
			// floatTrackbarControlNoise
			// 
			this.floatTrackbarControlNoise.Location = new System.Drawing.Point(1388, 84);
			this.floatTrackbarControlNoise.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlNoise.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlNoise.Name = "floatTrackbarControlNoise";
			this.floatTrackbarControlNoise.RangeMax = 1F;
			this.floatTrackbarControlNoise.RangeMin = 0F;
			this.floatTrackbarControlNoise.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlNoise.TabIndex = 1;
			this.floatTrackbarControlNoise.Value = 1F;
			this.floatTrackbarControlNoise.VisibleRangeMax = 1F;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(1302, 88);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(81, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Noise Influence";
			// 
			// PathTracingForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1600, 741);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.integerTrackbarControl1);
			this.Controls.Add(this.floatTrackbarControlNoise);
			this.Controls.Add(this.floatTrackbarControlGlossSphere);
			this.Controls.Add(this.floatTrackbarControlGlossWall);
			this.Controls.Add(this.panelOutput3D);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "PathTracingForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Path Tracing Test";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput3D panelOutput3D;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlGlossWall;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControl1;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlGlossSphere;
		private System.Windows.Forms.Label label2;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlNoise;
		private System.Windows.Forms.Label label3;
	}
}

