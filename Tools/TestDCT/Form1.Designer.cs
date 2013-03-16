namespace TestDCT
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
			this.floatTrackbarControlCameraTheta = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.displayPanelCurve = new TestDCT.DisplayPanel( this.components );
			this.floatTrackbarControlProbaFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.displayPanelDCT = new TestDCT.DisplayPanel( this.components );
			this.SuspendLayout();
			// 
			// floatTrackbarControlCameraTheta
			// 
			this.floatTrackbarControlCameraTheta.Location = new System.Drawing.Point( 49, 433 );
			this.floatTrackbarControlCameraTheta.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlCameraTheta.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlCameraTheta.Name = "floatTrackbarControlCameraTheta";
			this.floatTrackbarControlCameraTheta.RangeMax = 89.5F;
			this.floatTrackbarControlCameraTheta.RangeMin = 0.5F;
			this.floatTrackbarControlCameraTheta.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlCameraTheta.TabIndex = 1;
			this.floatTrackbarControlCameraTheta.Value = 45F;
			this.floatTrackbarControlCameraTheta.VisibleRangeMax = 89.5F;
			this.floatTrackbarControlCameraTheta.VisibleRangeMin = 0.5F;
			// 
			// displayPanelCurve
			// 
			this.displayPanelCurve.Location = new System.Drawing.Point( 12, 12 );
			this.displayPanelCurve.Name = "displayPanelCurve";
			this.displayPanelCurve.Size = new System.Drawing.Size( 574, 396 );
			this.displayPanelCurve.TabIndex = 0;
			// 
			// floatTrackbarControlProbaFactor
			// 
			this.floatTrackbarControlProbaFactor.Location = new System.Drawing.Point( 49, 459 );
			this.floatTrackbarControlProbaFactor.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlProbaFactor.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlProbaFactor.Name = "floatTrackbarControlProbaFactor";
			this.floatTrackbarControlProbaFactor.RangeMax = 1000F;
			this.floatTrackbarControlProbaFactor.RangeMin = 1F;
			this.floatTrackbarControlProbaFactor.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlProbaFactor.TabIndex = 1;
			this.floatTrackbarControlProbaFactor.Value = 10F;
			this.floatTrackbarControlProbaFactor.VisibleRangeMax = 100F;
			this.floatTrackbarControlProbaFactor.VisibleRangeMin = 1F;
			// 
			// displayPanelDCT
			// 
			this.displayPanelDCT.Location = new System.Drawing.Point( 592, 12 );
			this.displayPanelDCT.Name = "displayPanelDCT";
			this.displayPanelDCT.Size = new System.Drawing.Size( 574, 396 );
			this.displayPanelDCT.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 1185, 506 );
			this.Controls.Add( this.floatTrackbarControlProbaFactor );
			this.Controls.Add( this.floatTrackbarControlCameraTheta );
			this.Controls.Add( this.displayPanelDCT );
			this.Controls.Add( this.displayPanelCurve );
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout( false );

		}

		#endregion

		private DisplayPanel displayPanelCurve;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlCameraTheta;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlProbaFactor;
		private DisplayPanel displayPanelDCT;
	}
}

