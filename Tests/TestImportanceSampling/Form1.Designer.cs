namespace TestImportanceSampling
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
			this.displayPanel = new TestImportanceSampling.DisplayPanel( this.components );
			this.floatTrackbarControlProbaFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.SuspendLayout();
			// 
			// floatTrackbarControlCameraTheta
			// 
			this.floatTrackbarControlCameraTheta.Location = new System.Drawing.Point( 639, 12 );
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
			this.floatTrackbarControlCameraTheta.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlCameraTheta_ValueChanged );
			// 
			// displayPanel
			// 
			this.displayPanel.Location = new System.Drawing.Point( 12, 12 );
			this.displayPanel.Name = "displayPanel";
			this.displayPanel.Size = new System.Drawing.Size( 611, 433 );
			this.displayPanel.TabIndex = 0;
			// 
			// floatTrackbarControlProbaFactor
			// 
			this.floatTrackbarControlProbaFactor.Location = new System.Drawing.Point( 639, 38 );
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
			this.floatTrackbarControlProbaFactor.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlProbaFactor_ValueChanged );
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 937, 596 );
			this.Controls.Add( this.floatTrackbarControlProbaFactor );
			this.Controls.Add( this.floatTrackbarControlCameraTheta );
			this.Controls.Add( this.displayPanel );
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout( false );

		}

		#endregion

		private DisplayPanel displayPanel;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlCameraTheta;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlProbaFactor;
	}
}

