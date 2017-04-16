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
			this.floatTrackbarControl1 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.integerTrackbarControl1 = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.buttonReload = new System.Windows.Forms.Button();
			this.panelOutput3D = new TestPathTracing.PanelOutput3D(this.components);
			this.SuspendLayout();
			// 
			// floatTrackbarControl1
			// 
			this.floatTrackbarControl1.Location = new System.Drawing.Point(1371, 87);
			this.floatTrackbarControl1.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl1.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl1.Name = "floatTrackbarControl1";
			this.floatTrackbarControl1.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControl1.TabIndex = 1;
			this.floatTrackbarControl1.Value = 0F;
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
			// panelOutput3D
			// 
			this.panelOutput3D.Location = new System.Drawing.Point(12, 12);
			this.panelOutput3D.Name = "panelOutput3D";
			this.panelOutput3D.Size = new System.Drawing.Size(1280, 720);
			this.panelOutput3D.TabIndex = 3;
			// 
			// PathTracingForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1600, 741);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.integerTrackbarControl1);
			this.Controls.Add(this.floatTrackbarControl1);
			this.Controls.Add(this.panelOutput3D);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "PathTracingForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Path Tracing Test";
			this.ResumeLayout(false);

		}

		#endregion

		private PanelOutput3D panelOutput3D;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControl1;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControl1;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.Button buttonReload;
	}
}

