namespace ShaderToy
{
	partial class ShaderToyForm
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
			this.panelOutput = new ShaderToy.PanelOutput(this.components);
			this.floatTrackbarControlBeta = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.buttonReload = new System.Windows.Forms.Button();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(601, 432);
			this.panelOutput.TabIndex = 0;
			// 
			// floatTrackbarControlBeta
			// 
			this.floatTrackbarControlBeta.Location = new System.Drawing.Point(651, 12);
			this.floatTrackbarControlBeta.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlBeta.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlBeta.Name = "floatTrackbarControlBeta";
			this.floatTrackbarControlBeta.RangeMax = 100F;
			this.floatTrackbarControlBeta.RangeMin = 0F;
			this.floatTrackbarControlBeta.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlBeta.TabIndex = 1;
			this.floatTrackbarControlBeta.Value = 0.1F;
			this.floatTrackbarControlBeta.VisibleRangeMax = 1F;
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(759, 235);
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
			// ShaderToyForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(902, 544);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.floatTrackbarControlBeta);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "ShaderToyForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ShaderToy";
			this.ResumeLayout(false);

		}

		#endregion

		private PanelOutput panelOutput;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlBeta;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Timer timer;
	}
}

