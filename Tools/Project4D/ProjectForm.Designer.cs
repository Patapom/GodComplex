namespace Project4D
{
	partial class ProjectForm
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
			this.timer1 = new System.Windows.Forms.Timer( this.components );
			this.panelOutput = new Nuaj.Cirrus.Utility.PanelOutput( this.components );
			this.buttonReload = new System.Windows.Forms.Button();
			this.floatTrackbarControlX = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlY = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlZ = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlW = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.checkBoxAuto = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 10;
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point( 12, 12 );
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size( 783, 573 );
			this.panelOutput.TabIndex = 0;
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point( 810, 562 );
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size( 75, 23 );
			this.buttonReload.TabIndex = 1;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler( this.buttonReload_Click );
			// 
			// floatTrackbarControlX
			// 
			this.floatTrackbarControlX.Location = new System.Drawing.Point( 31, 23 );
			this.floatTrackbarControlX.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlX.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlX.Name = "floatTrackbarControlX";
			this.floatTrackbarControlX.RangeMax = 4F;
			this.floatTrackbarControlX.RangeMin = -4F;
			this.floatTrackbarControlX.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlX.TabIndex = 2;
			this.floatTrackbarControlX.Value = 0F;
			this.floatTrackbarControlX.VisibleRangeMax = 4F;
			this.floatTrackbarControlX.VisibleRangeMin = -4F;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add( this.checkBoxAuto );
			this.groupBox1.Controls.Add( this.label4 );
			this.groupBox1.Controls.Add( this.label3 );
			this.groupBox1.Controls.Add( this.label2 );
			this.groupBox1.Controls.Add( this.label1 );
			this.groupBox1.Controls.Add( this.floatTrackbarControlW );
			this.groupBox1.Controls.Add( this.floatTrackbarControlZ );
			this.groupBox1.Controls.Add( this.floatTrackbarControlY );
			this.groupBox1.Controls.Add( this.floatTrackbarControlX );
			this.groupBox1.Location = new System.Drawing.Point( 801, 12 );
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size( 269, 166 );
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "4D Camera Position";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point( 6, 28 );
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size( 14, 13 );
			this.label1.TabIndex = 3;
			this.label1.Text = "X";
			// 
			// floatTrackbarControlY
			// 
			this.floatTrackbarControlY.Location = new System.Drawing.Point( 31, 49 );
			this.floatTrackbarControlY.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlY.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlY.Name = "floatTrackbarControlY";
			this.floatTrackbarControlY.RangeMax = 4F;
			this.floatTrackbarControlY.RangeMin = -4F;
			this.floatTrackbarControlY.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlY.TabIndex = 2;
			this.floatTrackbarControlY.Value = 0F;
			this.floatTrackbarControlY.VisibleRangeMax = 4F;
			this.floatTrackbarControlY.VisibleRangeMin = -4F;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point( 6, 54 );
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size( 14, 13 );
			this.label2.TabIndex = 3;
			this.label2.Text = "Y";
			// 
			// floatTrackbarControlZ
			// 
			this.floatTrackbarControlZ.Location = new System.Drawing.Point( 31, 75 );
			this.floatTrackbarControlZ.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlZ.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlZ.Name = "floatTrackbarControlZ";
			this.floatTrackbarControlZ.RangeMax = 4F;
			this.floatTrackbarControlZ.RangeMin = -4F;
			this.floatTrackbarControlZ.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlZ.TabIndex = 2;
			this.floatTrackbarControlZ.Value = 0F;
			this.floatTrackbarControlZ.VisibleRangeMax = 4F;
			this.floatTrackbarControlZ.VisibleRangeMin = -4F;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point( 6, 80 );
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size( 14, 13 );
			this.label3.TabIndex = 3;
			this.label3.Text = "Z";
			// 
			// floatTrackbarControlW
			// 
			this.floatTrackbarControlW.Location = new System.Drawing.Point( 31, 101 );
			this.floatTrackbarControlW.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlW.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlW.Name = "floatTrackbarControlW";
			this.floatTrackbarControlW.RangeMax = 4F;
			this.floatTrackbarControlW.RangeMin = -4F;
			this.floatTrackbarControlW.Size = new System.Drawing.Size( 200, 20 );
			this.floatTrackbarControlW.TabIndex = 2;
			this.floatTrackbarControlW.Value = 4F;
			this.floatTrackbarControlW.VisibleRangeMax = 4F;
			this.floatTrackbarControlW.VisibleRangeMin = -4F;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point( 6, 106 );
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size( 18, 13 );
			this.label4.TabIndex = 3;
			this.label4.Text = "W";
			// 
			// checkBoxAuto
			// 
			this.checkBoxAuto.AutoSize = true;
			this.checkBoxAuto.Location = new System.Drawing.Point( 9, 138 );
			this.checkBoxAuto.Name = "checkBoxAuto";
			this.checkBoxAuto.Size = new System.Drawing.Size( 48, 17 );
			this.checkBoxAuto.TabIndex = 4;
			this.checkBoxAuto.Text = "Auto";
			this.checkBoxAuto.UseVisualStyleBackColor = true;
			// 
			// ProjectForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 1090, 597 );
			this.Controls.Add( this.groupBox1 );
			this.Controls.Add( this.buttonReload );
			this.Controls.Add( this.panelOutput );
			this.Name = "ProjectForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "4D Projection Test";
			this.groupBox1.ResumeLayout( false );
			this.groupBox1.PerformLayout();
			this.ResumeLayout( false );

		}

		#endregion

		private Nuaj.Cirrus.Utility.PanelOutput panelOutput;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button buttonReload;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlX;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlW;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlZ;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlY;
		private System.Windows.Forms.CheckBox checkBoxAuto;
	}
}

