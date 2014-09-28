namespace ImprovedNormalMapDistribution
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
			this.floatTrackbarControlPhi = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlTheta = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.outputPanel21 = new ImprovedNormalMapDistribution.OutputPanel2(this.components);
			this.outputPanel1 = new ImprovedNormalMapDistribution.OutputPanel(this.components);
			this.checkBoxSplat = new System.Windows.Forms.CheckBox();
			this.buttonConvert = new System.Windows.Forms.Button();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.buttonConvertNew = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// floatTrackbarControlPhi
			// 
			this.floatTrackbarControlPhi.Location = new System.Drawing.Point(728, 12);
			this.floatTrackbarControlPhi.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlPhi.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlPhi.Name = "floatTrackbarControlPhi";
			this.floatTrackbarControlPhi.RangeMax = 180F;
			this.floatTrackbarControlPhi.RangeMin = -180F;
			this.floatTrackbarControlPhi.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlPhi.TabIndex = 1;
			this.floatTrackbarControlPhi.Value = 0F;
			this.floatTrackbarControlPhi.VisibleRangeMax = 180F;
			this.floatTrackbarControlPhi.VisibleRangeMin = -180F;
			this.floatTrackbarControlPhi.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlPhi_ValueChanged);
			// 
			// floatTrackbarControlTheta
			// 
			this.floatTrackbarControlTheta.Location = new System.Drawing.Point(728, 38);
			this.floatTrackbarControlTheta.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlTheta.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlTheta.Name = "floatTrackbarControlTheta";
			this.floatTrackbarControlTheta.RangeMax = 90F;
			this.floatTrackbarControlTheta.RangeMin = 0F;
			this.floatTrackbarControlTheta.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlTheta.TabIndex = 1;
			this.floatTrackbarControlTheta.Value = 0F;
			this.floatTrackbarControlTheta.VisibleRangeMax = 90F;
			this.floatTrackbarControlTheta.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlTheta_ValueChanged);
			// 
			// outputPanel21
			// 
			this.outputPanel21.Location = new System.Drawing.Point(728, 179);
			this.outputPanel21.Name = "outputPanel21";
			this.outputPanel21.Size = new System.Drawing.Size(256, 256);
			this.outputPanel21.TabIndex = 2;
			this.outputPanel21.Theta = 0D;
			// 
			// outputPanel1
			// 
			this.outputPanel1.Location = new System.Drawing.Point(12, 12);
			this.outputPanel1.Name = "outputPanel1";
			this.outputPanel1.Size = new System.Drawing.Size(706, 539);
			this.outputPanel1.Splat = false;
			this.outputPanel1.TabIndex = 0;
			// 
			// checkBoxSplat
			// 
			this.checkBoxSplat.AutoSize = true;
			this.checkBoxSplat.Location = new System.Drawing.Point(728, 79);
			this.checkBoxSplat.Name = "checkBoxSplat";
			this.checkBoxSplat.Size = new System.Drawing.Size(182, 17);
			this.checkBoxSplat.TabIndex = 3;
			this.checkBoxSplat.Text = "Splat all possible RGBA8 normals";
			this.checkBoxSplat.UseVisualStyleBackColor = true;
			this.checkBoxSplat.CheckedChanged += new System.EventHandler(this.checkBoxSplat_CheckedChanged);
			// 
			// buttonConvert
			// 
			this.buttonConvert.Location = new System.Drawing.Point(724, 509);
			this.buttonConvert.Name = "buttonConvert";
			this.buttonConvert.Size = new System.Drawing.Size(133, 42);
			this.buttonConvert.TabIndex = 4;
			this.buttonConvert.Text = "Convert Normal Map  (old format)";
			this.buttonConvert.UseVisualStyleBackColor = true;
			this.buttonConvert.Click += new System.EventHandler(this.buttonConvertOld_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "*.tga";
			this.openFileDialog.Filter = "Image Files|*.png;*.tga;*.tif|All Files (*.*)|*.*";
			this.openFileDialog.Title = "Choose a normal map to convert";
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.DefaultExt = "*.png";
			this.saveFileDialog.Filter = "PNG Files|*.png";
			// 
			// buttonConvertNew
			// 
			this.buttonConvertNew.Location = new System.Drawing.Point(724, 464);
			this.buttonConvertNew.Name = "buttonConvertNew";
			this.buttonConvertNew.Size = new System.Drawing.Size(133, 39);
			this.buttonConvertNew.TabIndex = 4;
			this.buttonConvertNew.Text = "Convert Normal Map (new format)";
			this.buttonConvertNew.UseVisualStyleBackColor = true;
			this.buttonConvertNew.Click += new System.EventHandler(this.buttonConvertNew_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1004, 665);
			this.Controls.Add(this.buttonConvertNew);
			this.Controls.Add(this.buttonConvert);
			this.Controls.Add(this.checkBoxSplat);
			this.Controls.Add(this.outputPanel21);
			this.Controls.Add(this.floatTrackbarControlTheta);
			this.Controls.Add(this.floatTrackbarControlPhi);
			this.Controls.Add(this.outputPanel1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OutputPanel outputPanel1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlPhi;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlTheta;
		private OutputPanel2 outputPanel21;
		private System.Windows.Forms.CheckBox checkBoxSplat;
		private System.Windows.Forms.Button buttonConvert;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.Button buttonConvertNew;
	}
}

