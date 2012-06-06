namespace SecondOrderPolynomialApproximation
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
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.sphereViewLSH = new SecondOrderPolynomialApproximation.SphereView( this.components );
			this.sphereView = new SecondOrderPolynomialApproximation.SphereView( this.components );
			this.sphereViewLSHOptimized = new SecondOrderPolynomialApproximation.SphereView( this.components );
			this.SuspendLayout();
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "*.lsh";
			this.openFileDialog.Filter = "LSH Files|*.lsh|All files (*.*)|*.*";
			// 
			// sphereViewLSH
			// 
			this.sphereViewLSH.Location = new System.Drawing.Point( 12, 318 );
			this.sphereViewLSH.Name = "sphereViewLSH";
			this.sphereViewLSH.Size = new System.Drawing.Size( 600, 300 );
			this.sphereViewLSH.TabIndex = 0;
			this.sphereViewLSH.MouseUp += new System.Windows.Forms.MouseEventHandler( this.sphereViewLSH_MouseUp );
			// 
			// sphereView
			// 
			this.sphereView.Location = new System.Drawing.Point( 12, 12 );
			this.sphereView.Name = "sphereView";
			this.sphereView.Size = new System.Drawing.Size( 600, 300 );
			this.sphereView.TabIndex = 0;
			this.sphereView.MouseUp += new System.Windows.Forms.MouseEventHandler( this.sphereView_MouseUp );
			// 
			// sphereViewLSHOptimized
			// 
			this.sphereViewLSHOptimized.Location = new System.Drawing.Point( 12, 624 );
			this.sphereViewLSHOptimized.Name = "sphereViewLSHOptimized";
			this.sphereViewLSHOptimized.Size = new System.Drawing.Size( 600, 300 );
			this.sphereViewLSHOptimized.TabIndex = 0;
			this.sphereViewLSHOptimized.MouseUp += new System.Windows.Forms.MouseEventHandler( this.sphereViewLSH_MouseUp );
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 626, 942 );
			this.Controls.Add( this.sphereViewLSHOptimized );
			this.Controls.Add( this.sphereViewLSH );
			this.Controls.Add( this.sphereView );
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout( false );

		}

		#endregion

		private SecondOrderPolynomialApproximation.SphereView sphereView;
		private SphereView sphereViewLSH;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private SphereView sphereViewLSHOptimized;
	}
}

