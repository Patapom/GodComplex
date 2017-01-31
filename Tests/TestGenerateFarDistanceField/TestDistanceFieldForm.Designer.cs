namespace TestGenerateFarDistanceField
{
	partial class TestDistanceFieldForm
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
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.buttonReload = new System.Windows.Forms.Button();
			this.panelOutput = new TestGenerateFarDistanceField.PanelOutput(this.components);
			this.floatTrackbarControlGloss = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.SuspendLayout();
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 10;
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(1080, 716);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 1;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(955, 638);
			this.panelOutput.TabIndex = 0;
			// 
			// floatTrackbarControlGloss
			// 
			this.floatTrackbarControlGloss.Location = new System.Drawing.Point(12, 682);
			this.floatTrackbarControlGloss.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlGloss.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlGloss.Name = "floatTrackbarControlGloss";
			this.floatTrackbarControlGloss.RangeMax = 1F;
			this.floatTrackbarControlGloss.RangeMin = 0F;
			this.floatTrackbarControlGloss.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlGloss.TabIndex = 2;
			this.floatTrackbarControlGloss.Value = 0F;
			this.floatTrackbarControlGloss.VisibleRangeMax = 1F;
			// 
			// TestDistanceFieldForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1167, 751);
			this.Controls.Add(this.floatTrackbarControlGloss);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput);
			this.Name = "TestDistanceFieldForm";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private PanelOutput panelOutput;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button buttonReload;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlGloss;
	}
}

