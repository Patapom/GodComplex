namespace GloubiBoule
{
	partial class GloubiForm
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
			this.panelOutput = new Nuaj.Cirrus.Utility.PanelOutput(this.components);
			this.floatTrackbarControl2 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.buttonReload = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// floatTrackbarControl1
			// 
			this.floatTrackbarControl1.Location = new System.Drawing.Point(1390, 12);
			this.floatTrackbarControl1.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl1.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl1.Name = "floatTrackbarControl1";
			this.floatTrackbarControl1.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControl1.TabIndex = 0;
			this.floatTrackbarControl1.Value = 0F;
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1280, 720);
			this.panelOutput.TabIndex = 1;
			// 
			// floatTrackbarControl2
			// 
			this.floatTrackbarControl2.Location = new System.Drawing.Point(1390, 38);
			this.floatTrackbarControl2.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl2.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl2.Name = "floatTrackbarControl2";
			this.floatTrackbarControl2.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControl2.TabIndex = 0;
			this.floatTrackbarControl2.Value = 0F;
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(1515, 709);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 2;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			// 
			// GloubiForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1602, 741);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput);
			this.Controls.Add(this.floatTrackbarControl2);
			this.Controls.Add(this.floatTrackbarControl1);
			this.Name = "GloubiForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControl1;
		private Nuaj.Cirrus.Utility.PanelOutput panelOutput;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControl2;
		private System.Windows.Forms.Button buttonReload;
	}
}

