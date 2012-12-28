namespace BRDFSlices
{
	partial class DisplayForm
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
				m_Slice.Dispose();
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
			this.label1 = new System.Windows.Forms.Label();
			this.integerTrackbarControlPhiD = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.floatTrackbarControlGamma = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlExposure = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.checkBoxDifferences = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.floatTrackbarControlWarpFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.checkBoxUseWarping = new System.Windows.Forms.CheckBox();
			this.panelDisplay = new BRDFSlices.DisplayPanel( this.components );
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point( 12, 478 );
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size( 30, 13 );
			this.label1.TabIndex = 1;
			this.label1.Text = "PhiD";
			// 
			// integerTrackbarControlPhiD
			// 
			this.integerTrackbarControlPhiD.Location = new System.Drawing.Point( 60, 475 );
			this.integerTrackbarControlPhiD.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.integerTrackbarControlPhiD.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.integerTrackbarControlPhiD.Name = "integerTrackbarControlPhiD";
			this.integerTrackbarControlPhiD.RangeMax = 179;
			this.integerTrackbarControlPhiD.RangeMin = 0;
			this.integerTrackbarControlPhiD.Size = new System.Drawing.Size( 440, 20 );
			this.integerTrackbarControlPhiD.TabIndex = 2;
			this.integerTrackbarControlPhiD.Value = 90;
			this.integerTrackbarControlPhiD.VisibleRangeMax = 179;
			this.integerTrackbarControlPhiD.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler( this.integerTrackbarControlPhiD_ValueChanged );
			// 
			// floatTrackbarControlGamma
			// 
			this.floatTrackbarControlGamma.Location = new System.Drawing.Point( 60, 501 );
			this.floatTrackbarControlGamma.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlGamma.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlGamma.Name = "floatTrackbarControlGamma";
			this.floatTrackbarControlGamma.RangeMax = 5F;
			this.floatTrackbarControlGamma.RangeMin = 1F;
			this.floatTrackbarControlGamma.Size = new System.Drawing.Size( 440, 20 );
			this.floatTrackbarControlGamma.TabIndex = 4;
			this.floatTrackbarControlGamma.Value = 2.2F;
			this.floatTrackbarControlGamma.VisibleRangeMax = 5F;
			this.floatTrackbarControlGamma.VisibleRangeMin = 1F;
			this.floatTrackbarControlGamma.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlGamma_ValueChanged );
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point( 12, 503 );
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size( 43, 13 );
			this.label2.TabIndex = 1;
			this.label2.Text = "Gamma";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point( 12, 529 );
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size( 51, 13 );
			this.label3.TabIndex = 1;
			this.label3.Text = "Exposure";
			// 
			// floatTrackbarControlExposure
			// 
			this.floatTrackbarControlExposure.Location = new System.Drawing.Point( 60, 527 );
			this.floatTrackbarControlExposure.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlExposure.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlExposure.Name = "floatTrackbarControlExposure";
			this.floatTrackbarControlExposure.RangeMax = 6F;
			this.floatTrackbarControlExposure.RangeMin = -6F;
			this.floatTrackbarControlExposure.Size = new System.Drawing.Size( 440, 20 );
			this.floatTrackbarControlExposure.TabIndex = 4;
			this.floatTrackbarControlExposure.Value = 0F;
			this.floatTrackbarControlExposure.VisibleRangeMax = 6F;
			this.floatTrackbarControlExposure.VisibleRangeMin = -6F;
			this.floatTrackbarControlExposure.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlExposure_ValueChanged );
			// 
			// checkBoxDifferences
			// 
			this.checkBoxDifferences.AutoSize = true;
			this.checkBoxDifferences.Location = new System.Drawing.Point( 12, 553 );
			this.checkBoxDifferences.Name = "checkBoxDifferences";
			this.checkBoxDifferences.Size = new System.Drawing.Size( 132, 17 );
			this.checkBoxDifferences.TabIndex = 5;
			this.checkBoxDifferences.Text = "Show slice differences";
			this.checkBoxDifferences.UseVisualStyleBackColor = true;
			this.checkBoxDifferences.CheckedChanged += new System.EventHandler( this.checkBoxDifferences_CheckedChanged );
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point( 12, 580 );
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size( 33, 13 );
			this.label4.TabIndex = 1;
			this.label4.Text = "Warp";
			// 
			// floatTrackbarControlWarpFactor
			// 
			this.floatTrackbarControlWarpFactor.Enabled = false;
			this.floatTrackbarControlWarpFactor.Location = new System.Drawing.Point( 60, 578 );
			this.floatTrackbarControlWarpFactor.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlWarpFactor.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlWarpFactor.Name = "floatTrackbarControlWarpFactor";
			this.floatTrackbarControlWarpFactor.RangeMax = 90F;
			this.floatTrackbarControlWarpFactor.RangeMin = 0F;
			this.floatTrackbarControlWarpFactor.Size = new System.Drawing.Size( 440, 20 );
			this.floatTrackbarControlWarpFactor.TabIndex = 4;
			this.floatTrackbarControlWarpFactor.Value = 90F;
			this.floatTrackbarControlWarpFactor.VisibleRangeMax = 90F;
			this.floatTrackbarControlWarpFactor.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlWarpFactor_ValueChanged );
			// 
			// checkBoxUseWarping
			// 
			this.checkBoxUseWarping.AutoSize = true;
			this.checkBoxUseWarping.Location = new System.Drawing.Point( 150, 553 );
			this.checkBoxUseWarping.Name = "checkBoxUseWarping";
			this.checkBoxUseWarping.Size = new System.Drawing.Size( 88, 17 );
			this.checkBoxUseWarping.TabIndex = 5;
			this.checkBoxUseWarping.Text = "Use Warping";
			this.checkBoxUseWarping.UseVisualStyleBackColor = true;
			this.checkBoxUseWarping.CheckedChanged += new System.EventHandler( this.checkBoxUseWarping_CheckedChanged );
			// 
			// panelDisplay
			// 
			this.panelDisplay.Location = new System.Drawing.Point( 31, 12 );
			this.panelDisplay.Name = "panelDisplay";
			this.panelDisplay.Size = new System.Drawing.Size( 450, 450 );
			this.panelDisplay.Slice = null;
			this.panelDisplay.TabIndex = 3;
			// 
			// DisplayForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 512, 610 );
			this.Controls.Add( this.checkBoxUseWarping );
			this.Controls.Add( this.checkBoxDifferences );
			this.Controls.Add( this.floatTrackbarControlWarpFactor );
			this.Controls.Add( this.floatTrackbarControlExposure );
			this.Controls.Add( this.floatTrackbarControlGamma );
			this.Controls.Add( this.label4 );
			this.Controls.Add( this.panelDisplay );
			this.Controls.Add( this.label3 );
			this.Controls.Add( this.integerTrackbarControlPhiD );
			this.Controls.Add( this.label2 );
			this.Controls.Add( this.label1 );
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DisplayForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "BRDF Slice";
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlPhiD;
		private DisplayPanel panelDisplay;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlGamma;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlExposure;
		private System.Windows.Forms.CheckBox checkBoxDifferences;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlWarpFactor;
		private System.Windows.Forms.CheckBox checkBoxUseWarping;

	}
}