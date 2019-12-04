namespace TestVonMisesFisher
{
	partial class FittingForm
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
			this.panelOutput = new UIUtility.PanelOutput( this.components );
			this.panelOutputNormalDistribution = new UIUtility.PanelOutput( this.components );
			this.SuspendLayout();
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point( 12, 12 );
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size( 455, 455 );
			this.panelOutput.TabIndex = 0;
			this.panelOutput.BitmapUpdating += new UIUtility.PanelOutput.UpdateBitmapDelegate( this.panelOutput_BitmapUpdating );
			// 
			// panelOutputNormalDistribution
			// 
			this.panelOutputNormalDistribution.Location = new System.Drawing.Point( 502, 270 );
			this.panelOutputNormalDistribution.Name = "panelOutputNormalDistribution";
			this.panelOutputNormalDistribution.Size = new System.Drawing.Size( 434, 197 );
			this.panelOutputNormalDistribution.TabIndex = 0;
			this.panelOutputNormalDistribution.BitmapUpdating += new UIUtility.PanelOutput.UpdateBitmapDelegate( this.panelOutputNormalDistribution_BitmapUpdating );
			// 
			// FittingForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 969, 510 );
			this.Controls.Add( this.panelOutputNormalDistribution );
			this.Controls.Add( this.panelOutput );
			this.Name = "FittingForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "von Mises - Fisher Distribution Fitting Test";
			this.ResumeLayout( false );

		}

		#endregion

		private UIUtility.PanelOutput panelOutput;
		private UIUtility.PanelOutput panelOutputNormalDistribution;
	}
}

