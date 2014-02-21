namespace TestGradientPNG
{
	partial class FormCubeMapBaker
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
			this.button1 = new System.Windows.Forms.Button();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.radioButtonCross = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.integerTrackbarControlCubeMapSize = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.radioButtonProbe = new System.Windows.Forms.RadioButton();
			this.radioButtonCylindrical = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(109, 95);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(166, 46);
			this.button1.TabIndex = 0;
			this.button1.Text = "Select HDR Cube Map...";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(12, 152);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(368, 23);
			this.progressBar1.TabIndex = 1;
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "*.hdr";
			this.openFileDialog.Filter = "HDR Files (*.hdr)|*.hdr|All Files (*.*)|*.*";
			this.openFileDialog.Title = "Choose a .HDR cross cube map";
			// 
			// radioButtonCross
			// 
			this.radioButtonCross.AutoSize = true;
			this.radioButtonCross.Checked = true;
			this.radioButtonCross.Location = new System.Drawing.Point(81, 60);
			this.radioButtonCross.Name = "radioButtonCross";
			this.radioButtonCross.Size = new System.Drawing.Size(51, 17);
			this.radioButtonCross.TabIndex = 2;
			this.radioButtonCross.TabStop = true;
			this.radioButtonCross.Text = "Cross";
			this.radioButtonCross.UseVisualStyleBackColor = true;
			this.radioButtonCross.CheckedChanged += new System.EventHandler(this.radioButtonCross_CheckedChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 46);
			this.label1.TabIndex = 4;
			this.label1.Text = "Final Cube Face Size (leave it to 256 for Maya shader)";
			// 
			// integerTrackbarControlCubeMapSize
			// 
			this.integerTrackbarControlCubeMapSize.Location = new System.Drawing.Point(118, 23);
			this.integerTrackbarControlCubeMapSize.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlCubeMapSize.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlCubeMapSize.Name = "integerTrackbarControlCubeMapSize";
			this.integerTrackbarControlCubeMapSize.RangeMax = 1024;
			this.integerTrackbarControlCubeMapSize.RangeMin = 1;
			this.integerTrackbarControlCubeMapSize.Size = new System.Drawing.Size(262, 20);
			this.integerTrackbarControlCubeMapSize.TabIndex = 5;
			this.integerTrackbarControlCubeMapSize.Value = 256;
			this.integerTrackbarControlCubeMapSize.VisibleRangeMax = 512;
			this.integerTrackbarControlCubeMapSize.VisibleRangeMin = 1;
			this.integerTrackbarControlCubeMapSize.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlCubeMapSize_ValueChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 62);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(63, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Image Type";
			// 
			// radioButtonProbe
			// 
			this.radioButtonProbe.AutoSize = true;
			this.radioButtonProbe.Location = new System.Drawing.Point(138, 60);
			this.radioButtonProbe.Name = "radioButtonProbe";
			this.radioButtonProbe.Size = new System.Drawing.Size(53, 17);
			this.radioButtonProbe.TabIndex = 2;
			this.radioButtonProbe.Text = "Probe";
			this.radioButtonProbe.UseVisualStyleBackColor = true;
			this.radioButtonProbe.CheckedChanged += new System.EventHandler(this.radioButtonProbe_CheckedChanged);
			// 
			// radioButtonCylindrical
			// 
			this.radioButtonCylindrical.AutoSize = true;
			this.radioButtonCylindrical.Location = new System.Drawing.Point(197, 60);
			this.radioButtonCylindrical.Name = "radioButtonCylindrical";
			this.radioButtonCylindrical.Size = new System.Drawing.Size(72, 17);
			this.radioButtonCylindrical.TabIndex = 2;
			this.radioButtonCylindrical.Text = "Cylindrical";
			this.radioButtonCylindrical.UseVisualStyleBackColor = true;
			this.radioButtonCylindrical.CheckedChanged += new System.EventHandler(this.radioButtonCylindrical_CheckedChanged);
			// 
			// FormCubeMapBaker
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(393, 187);
			this.Controls.Add(this.integerTrackbarControlCubeMapSize);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.radioButtonCylindrical);
			this.Controls.Add(this.radioButtonProbe);
			this.Controls.Add(this.radioButtonCross);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.button1);
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormCubeMapBaker";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "HDR Cube Map baker";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.RadioButton radioButtonCross;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlCubeMapSize;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RadioButton radioButtonProbe;
		private System.Windows.Forms.RadioButton radioButtonCylindrical;
	}
}

