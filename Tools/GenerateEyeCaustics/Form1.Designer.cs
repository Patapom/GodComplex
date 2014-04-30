namespace GenerateEyeCaustics
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
			this.floatTrackbarControlTheta = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.integerTrackbarControlPhotonsCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.buttonShoot = new System.Windows.Forms.Button();
			this.outputPanel1 = new GenerateEyeCaustics.OutputPanel(this.components);
			this.SuspendLayout();
			// 
			// floatTrackbarControlTheta
			// 
			this.floatTrackbarControlTheta.Location = new System.Drawing.Point(119, 437);
			this.floatTrackbarControlTheta.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlTheta.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlTheta.Name = "floatTrackbarControlTheta";
			this.floatTrackbarControlTheta.RangeMax = 90F;
			this.floatTrackbarControlTheta.RangeMin = 0F;
			this.floatTrackbarControlTheta.Size = new System.Drawing.Size(232, 20);
			this.floatTrackbarControlTheta.TabIndex = 0;
			this.floatTrackbarControlTheta.Value = 0F;
			this.floatTrackbarControlTheta.VisibleRangeMax = 90F;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(25, 439);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(77, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Light Elevation";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(25, 465);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(77, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Photons Count";
			// 
			// integerTrackbarControlPhotonsCount
			// 
			this.integerTrackbarControlPhotonsCount.Location = new System.Drawing.Point(119, 463);
			this.integerTrackbarControlPhotonsCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlPhotonsCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlPhotonsCount.Name = "integerTrackbarControlPhotonsCount";
			this.integerTrackbarControlPhotonsCount.RangeMax = 10000000;
			this.integerTrackbarControlPhotonsCount.RangeMin = 1;
			this.integerTrackbarControlPhotonsCount.Size = new System.Drawing.Size(232, 20);
			this.integerTrackbarControlPhotonsCount.TabIndex = 3;
			this.integerTrackbarControlPhotonsCount.Value = 1000000;
			this.integerTrackbarControlPhotonsCount.VisibleRangeMax = 1000000;
			this.integerTrackbarControlPhotonsCount.VisibleRangeMin = 1;
			// 
			// buttonShoot
			// 
			this.buttonShoot.Location = new System.Drawing.Point(156, 489);
			this.buttonShoot.Name = "buttonShoot";
			this.buttonShoot.Size = new System.Drawing.Size(115, 59);
			this.buttonShoot.TabIndex = 4;
			this.buttonShoot.Text = "Shoot";
			this.buttonShoot.UseVisualStyleBackColor = true;
			this.buttonShoot.Click += new System.EventHandler(this.buttonShoot_Click);
			// 
			// outputPanel1
			// 
			this.outputPanel1.Location = new System.Drawing.Point(12, 12);
			this.outputPanel1.Name = "outputPanel1";
			this.outputPanel1.PhotonsAccumulation = null;
			this.outputPanel1.Size = new System.Drawing.Size(400, 400);
			this.outputPanel1.TabIndex = 1;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(574, 580);
			this.Controls.Add(this.buttonShoot);
			this.Controls.Add(this.integerTrackbarControlPhotonsCount);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.outputPanel1);
			this.Controls.Add(this.floatTrackbarControlTheta);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlTheta;
		private OutputPanel outputPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlPhotonsCount;
		private System.Windows.Forms.Button buttonShoot;
	}
}

