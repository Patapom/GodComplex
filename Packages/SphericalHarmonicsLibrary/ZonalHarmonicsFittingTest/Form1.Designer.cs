namespace ZonalHarmonicsFittingTest
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.sphereViewOriginalZonalHarmonics = new ZonalHarmonicsFittingTest.SphereView( this.components );
			this.sphereViewRotatedZonalHarmonics = new ZonalHarmonicsFittingTest.SphereView( this.components );
			this.label3 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.labelRMSError = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.buttonMap = new System.Windows.Forms.Button();
			this.sphereViewEvaluatedSHEncoding = new ZonalHarmonicsFittingTest.SphereView( this.components );
			this.sphereViewRotatedZHEncodedFunction = new ZonalHarmonicsFittingTest.SphereView( this.components );
			this.sphereViewZHEncodedFunction = new ZonalHarmonicsFittingTest.SphereView( this.components );
			this.sphereViewOriginalFunction = new ZonalHarmonicsFittingTest.SphereView( this.components );
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add( this.label2 );
			this.groupBox1.Controls.Add( this.label1 );
			this.groupBox1.Controls.Add( this.sphereViewOriginalZonalHarmonics );
			this.groupBox1.Controls.Add( this.sphereViewRotatedZonalHarmonics );
			this.groupBox1.Location = new System.Drawing.Point( 18, 12 );
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size( 1047, 312 );
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Zonal Harmonics Tests";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
			this.label2.Location = new System.Drawing.Point( 540, 27 );
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size( 410, 16 );
			this.label2.TabIndex = 1;
			this.label2.Text = "Rotated Zonal Harmonics (click on a hemisphere to rotate)";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
			this.label1.Location = new System.Drawing.Point( 19, 27 );
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size( 263, 16 );
			this.label1.TabIndex = 1;
			this.label1.Text = "Original Zonal Harmonics (Y aligned)";
			// 
			// sphereViewOriginalZonalHarmonics
			// 
			this.sphereViewOriginalZonalHarmonics.Location = new System.Drawing.Point( 8, 43 );
			this.sphereViewOriginalZonalHarmonics.Name = "sphereViewOriginalZonalHarmonics";
			this.sphereViewOriginalZonalHarmonics.Size = new System.Drawing.Size( 512, 256 );
			this.sphereViewOriginalZonalHarmonics.TabIndex = 0;
			// 
			// sphereViewRotatedZonalHarmonics
			// 
			this.sphereViewRotatedZonalHarmonics.Location = new System.Drawing.Point( 526, 43 );
			this.sphereViewRotatedZonalHarmonics.Name = "sphereViewRotatedZonalHarmonics";
			this.sphereViewRotatedZonalHarmonics.Size = new System.Drawing.Size( 512, 256 );
			this.sphereViewRotatedZonalHarmonics.TabIndex = 0;
			this.sphereViewRotatedZonalHarmonics.MouseUp += new System.Windows.Forms.MouseEventHandler( this.sphereView4_MouseUp );
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
			this.label3.Location = new System.Drawing.Point( 16, 342 );
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size( 124, 16 );
			this.label3.TabIndex = 1;
			this.label3.Text = "Original Function";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
			this.label5.Location = new System.Drawing.Point( 542, 626 );
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size( 259, 16 );
			this.label5.TabIndex = 1;
			this.label5.Text = "ZH Encoded Function mapped to SH";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
			this.label6.Location = new System.Drawing.Point( 16, 626 );
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size( 322, 16 );
			this.label6.TabIndex = 1;
			this.label6.Text = "Rotated ZH Encoded Function (click to rotate)";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
			this.label7.Location = new System.Drawing.Point( 543, 904 );
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size( 87, 16 );
			this.label7.TabIndex = 1;
			this.label7.Text = "RMS Error :";
			// 
			// labelRMSError
			// 
			this.labelRMSError.AutoSize = true;
			this.labelRMSError.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
			this.labelRMSError.Location = new System.Drawing.Point( 636, 904 );
			this.labelRMSError.Name = "labelRMSError";
			this.labelRMSError.Size = new System.Drawing.Size( 16, 16 );
			this.labelRMSError.TabIndex = 1;
			this.labelRMSError.Text = "0";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
			this.label8.Location = new System.Drawing.Point( 542, 342 );
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size( 276, 16 );
			this.label8.TabIndex = 1;
			this.label8.Text = "Evaluation of the SH Encoded Function";
			// 
			// buttonMap
			// 
			this.buttonMap.Location = new System.Drawing.Point( 367, 927 );
			this.buttonMap.Name = "buttonMap";
			this.buttonMap.Size = new System.Drawing.Size( 164, 42 );
			this.buttonMap.TabIndex = 2;
			this.buttonMap.Text = "Perform Mapping";
			this.buttonMap.UseVisualStyleBackColor = true;
			this.buttonMap.Click += new System.EventHandler( this.buttonMap_Click );
			// 
			// sphereViewEvaluatedSHEncoding
			// 
			this.sphereViewEvaluatedSHEncoding.Location = new System.Drawing.Point( 545, 361 );
			this.sphereViewEvaluatedSHEncoding.Name = "sphereViewEvaluatedSHEncoding";
			this.sphereViewEvaluatedSHEncoding.Size = new System.Drawing.Size( 512, 256 );
			this.sphereViewEvaluatedSHEncoding.TabIndex = 0;
			// 
			// sphereViewRotatedZHEncodedFunction
			// 
			this.sphereViewRotatedZHEncodedFunction.Location = new System.Drawing.Point( 19, 645 );
			this.sphereViewRotatedZHEncodedFunction.Name = "sphereViewRotatedZHEncodedFunction";
			this.sphereViewRotatedZHEncodedFunction.Size = new System.Drawing.Size( 512, 256 );
			this.sphereViewRotatedZHEncodedFunction.TabIndex = 0;
			this.sphereViewRotatedZHEncodedFunction.MouseUp += new System.Windows.Forms.MouseEventHandler( this.sphereViewRotatedZHEncodedFunction_MouseUp );
			// 
			// sphereViewZHEncodedFunction
			// 
			this.sphereViewZHEncodedFunction.Location = new System.Drawing.Point( 545, 645 );
			this.sphereViewZHEncodedFunction.Name = "sphereViewZHEncodedFunction";
			this.sphereViewZHEncodedFunction.Size = new System.Drawing.Size( 512, 256 );
			this.sphereViewZHEncodedFunction.TabIndex = 0;
			// 
			// sphereViewOriginalFunction
			// 
			this.sphereViewOriginalFunction.Location = new System.Drawing.Point( 19, 361 );
			this.sphereViewOriginalFunction.Name = "sphereViewOriginalFunction";
			this.sphereViewOriginalFunction.Size = new System.Drawing.Size( 512, 256 );
			this.sphereViewOriginalFunction.TabIndex = 0;
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point( 545, 946 );
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size( 512, 23 );
			this.progressBar.TabIndex = 3;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 1085, 986 );
			this.Controls.Add( this.progressBar );
			this.Controls.Add( this.buttonMap );
			this.Controls.Add( this.groupBox1 );
			this.Controls.Add( this.label8 );
			this.Controls.Add( this.label6 );
			this.Controls.Add( this.labelRMSError );
			this.Controls.Add( this.label7 );
			this.Controls.Add( this.label5 );
			this.Controls.Add( this.label3 );
			this.Controls.Add( this.sphereViewEvaluatedSHEncoding );
			this.Controls.Add( this.sphereViewRotatedZHEncodedFunction );
			this.Controls.Add( this.sphereViewZHEncodedFunction );
			this.Controls.Add( this.sphereViewOriginalFunction );
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ZH Tests";
			this.groupBox1.ResumeLayout( false );
			this.groupBox1.PerformLayout();
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private SphereView sphereViewOriginalZonalHarmonics;
		private SphereView sphereViewOriginalFunction;
		private SphereView sphereViewRotatedZonalHarmonics;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		private SphereView sphereViewZHEncodedFunction;
		private System.Windows.Forms.Label label5;
		private SphereView sphereViewRotatedZHEncodedFunction;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label labelRMSError;
		private SphereView sphereViewEvaluatedSHEncoding;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button buttonMap;
		private System.Windows.Forms.ProgressBar progressBar;
	}
}

