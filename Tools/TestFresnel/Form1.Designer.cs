namespace TestFresnel
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
			this.radioButtonSchlick = new System.Windows.Forms.RadioButton();
			this.radioButtonPrecise = new System.Windows.Forms.RadioButton();
			this.floatTrackbarControl1 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.panelColor = new System.Windows.Forms.Panel();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.outputPanel1 = new TestFresnel.OutputPanel(this.components);
			this.openFileDialogRefract = new System.Windows.Forms.OpenFileDialog();
			this.buttonLoadData = new System.Windows.Forms.Button();
			this.checkBoxData = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// radioButtonSchlick
			// 
			this.radioButtonSchlick.AutoSize = true;
			this.radioButtonSchlick.Checked = true;
			this.radioButtonSchlick.Location = new System.Drawing.Point(600, 12);
			this.radioButtonSchlick.Name = "radioButtonSchlick";
			this.radioButtonSchlick.Size = new System.Drawing.Size(60, 17);
			this.radioButtonSchlick.TabIndex = 1;
			this.radioButtonSchlick.TabStop = true;
			this.radioButtonSchlick.Text = "Schlick";
			this.radioButtonSchlick.UseVisualStyleBackColor = true;
			this.radioButtonSchlick.CheckedChanged += new System.EventHandler(this.radioButtonSchlick_CheckedChanged);
			// 
			// radioButtonPrecise
			// 
			this.radioButtonPrecise.AutoSize = true;
			this.radioButtonPrecise.Location = new System.Drawing.Point(600, 35);
			this.radioButtonPrecise.Name = "radioButtonPrecise";
			this.radioButtonPrecise.Size = new System.Drawing.Size(60, 17);
			this.radioButtonPrecise.TabIndex = 1;
			this.radioButtonPrecise.Text = "Precise";
			this.radioButtonPrecise.UseVisualStyleBackColor = true;
			this.radioButtonPrecise.CheckedChanged += new System.EventHandler(this.radioButtonPrecise_CheckedChanged);
			// 
			// floatTrackbarControl1
			// 
			this.floatTrackbarControl1.Location = new System.Drawing.Point(626, 81);
			this.floatTrackbarControl1.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl1.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl1.Name = "floatTrackbarControl1";
			this.floatTrackbarControl1.RangeMax = 10000F;
			this.floatTrackbarControl1.RangeMin = 1F;
			this.floatTrackbarControl1.Size = new System.Drawing.Size(207, 20);
			this.floatTrackbarControl1.TabIndex = 2;
			this.floatTrackbarControl1.Value = 1F;
			this.floatTrackbarControl1.VisibleRangeMin = 1F;
			this.floatTrackbarControl1.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControl1_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(597, 84);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(26, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "IOR";
			// 
			// panelColor
			// 
			this.panelColor.BackColor = System.Drawing.Color.White;
			this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelColor.Location = new System.Drawing.Point(685, 138);
			this.panelColor.Name = "panelColor";
			this.panelColor.Size = new System.Drawing.Size(48, 35);
			this.panelColor.TabIndex = 4;
			this.panelColor.Click += new System.EventHandler(this.panelColor_Click);
			// 
			// colorDialog1
			// 
			this.colorDialog1.FullOpen = true;
			// 
			// outputPanel1
			// 
			this.outputPanel1.Data = null;
			this.outputPanel1.FresnelType = TestFresnel.OutputPanel.FRESNEL_TYPE.SCHLICK;
			this.outputPanel1.IOR = 1F;
			this.outputPanel1.Location = new System.Drawing.Point(12, 12);
			this.outputPanel1.Name = "outputPanel1";
			this.outputPanel1.Size = new System.Drawing.Size(582, 516);
			this.outputPanel1.SpecularTint = System.Drawing.Color.White;
			this.outputPanel1.TabIndex = 0;
			// 
			// openFileDialogRefract
			// 
			this.openFileDialogRefract.DefaultExt = "*.txt";
			this.openFileDialogRefract.Filter = "Text Files|*.txt|All Files (*.*)|*.*";
			this.openFileDialogRefract.Title = "Choose the TXT file from refractiveindex.info";
			// 
			// buttonLoadData
			// 
			this.buttonLoadData.Location = new System.Drawing.Point(758, 9);
			this.buttonLoadData.Name = "buttonLoadData";
			this.buttonLoadData.Size = new System.Drawing.Size(75, 23);
			this.buttonLoadData.TabIndex = 5;
			this.buttonLoadData.Text = "Load Data";
			this.buttonLoadData.UseVisualStyleBackColor = true;
			this.buttonLoadData.Click += new System.EventHandler(this.buttonLoadData_Click);
			// 
			// checkBoxData
			// 
			this.checkBoxData.AutoSize = true;
			this.checkBoxData.Location = new System.Drawing.Point(703, 13);
			this.checkBoxData.Name = "checkBoxData";
			this.checkBoxData.Size = new System.Drawing.Size(49, 17);
			this.checkBoxData.TabIndex = 6;
			this.checkBoxData.Text = "Data";
			this.checkBoxData.UseVisualStyleBackColor = true;
			this.checkBoxData.CheckedChanged += new System.EventHandler(this.checkBoxData_CheckedChanged);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(845, 578);
			this.Controls.Add(this.checkBoxData);
			this.Controls.Add(this.buttonLoadData);
			this.Controls.Add(this.panelColor);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControl1);
			this.Controls.Add(this.radioButtonPrecise);
			this.Controls.Add(this.radioButtonSchlick);
			this.Controls.Add(this.outputPanel1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OutputPanel outputPanel1;
		private System.Windows.Forms.RadioButton radioButtonSchlick;
		private System.Windows.Forms.RadioButton radioButtonPrecise;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControl1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panelColor;
		private System.Windows.Forms.ColorDialog colorDialog1;
		private System.Windows.Forms.OpenFileDialog openFileDialogRefract;
		private System.Windows.Forms.Button buttonLoadData;
		private System.Windows.Forms.CheckBox checkBoxData;
	}
}

