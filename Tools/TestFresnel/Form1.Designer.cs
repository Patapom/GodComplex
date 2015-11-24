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
			this.floatTrackbarControlIOR = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.panelColor = new System.Windows.Forms.Panel();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.openFileDialogRefract = new System.Windows.Forms.OpenFileDialog();
			this.buttonLoadData = new System.Windows.Forms.Button();
			this.checkBoxData = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.radioButtonIOR = new System.Windows.Forms.RadioButton();
			this.radioButtonSpecularTint = new System.Windows.Forms.RadioButton();
			this.floatTrackbarControlVerticalScale = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.checkBoxusePreComputedTable = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlRoughness = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.outputPanel2 = new TestFresnel.OutputPanel2(this.components);
			this.outputPanel1 = new TestFresnel.OutputPanel(this.components);
			this.label3 = new System.Windows.Forms.Label();
			this.checkBoxPlotAgainstF0 = new System.Windows.Forms.CheckBox();
			this.panel1.SuspendLayout();
			this.outputPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// radioButtonSchlick
			// 
			this.radioButtonSchlick.AutoSize = true;
			this.radioButtonSchlick.Checked = true;
			this.radioButtonSchlick.Location = new System.Drawing.Point(580, 12);
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
			this.radioButtonPrecise.Location = new System.Drawing.Point(580, 28);
			this.radioButtonPrecise.Name = "radioButtonPrecise";
			this.radioButtonPrecise.Size = new System.Drawing.Size(60, 17);
			this.radioButtonPrecise.TabIndex = 1;
			this.radioButtonPrecise.Text = "Precise";
			this.radioButtonPrecise.UseVisualStyleBackColor = true;
			this.radioButtonPrecise.CheckedChanged += new System.EventHandler(this.radioButtonPrecise_CheckedChanged);
			// 
			// floatTrackbarControlIOR
			// 
			this.floatTrackbarControlIOR.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlIOR.Location = new System.Drawing.Point(43, 3);
			this.floatTrackbarControlIOR.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlIOR.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlIOR.Name = "floatTrackbarControlIOR";
			this.floatTrackbarControlIOR.RangeMax = 10000F;
			this.floatTrackbarControlIOR.RangeMin = 0.0001F;
			this.floatTrackbarControlIOR.Size = new System.Drawing.Size(197, 20);
			this.floatTrackbarControlIOR.TabIndex = 2;
			this.floatTrackbarControlIOR.Value = 1F;
			this.floatTrackbarControlIOR.VisibleRangeMin = 0.1F;
			this.floatTrackbarControlIOR.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControl1_ValueChanged);
			// 
			// panelColor
			// 
			this.panelColor.BackColor = System.Drawing.Color.White;
			this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelColor.Enabled = false;
			this.panelColor.Location = new System.Drawing.Point(94, 29);
			this.panelColor.Name = "panelColor";
			this.panelColor.Size = new System.Drawing.Size(48, 35);
			this.panelColor.TabIndex = 4;
			this.panelColor.Click += new System.EventHandler(this.panelColor_Click);
			// 
			// colorDialog1
			// 
			this.colorDialog1.FullOpen = true;
			// 
			// openFileDialogRefract
			// 
			this.openFileDialogRefract.DefaultExt = "*.txt";
			this.openFileDialogRefract.Filter = "Text Files|*.txt|All Files (*.*)|*.*";
			this.openFileDialogRefract.Title = "Choose the TXT file from refractiveindex.info";
			// 
			// buttonLoadData
			// 
			this.buttonLoadData.Location = new System.Drawing.Point(738, 9);
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
			this.checkBoxData.Location = new System.Drawing.Point(683, 13);
			this.checkBoxData.Name = "checkBoxData";
			this.checkBoxData.Size = new System.Drawing.Size(49, 17);
			this.checkBoxData.TabIndex = 6;
			this.checkBoxData.Text = "Data";
			this.checkBoxData.UseVisualStyleBackColor = true;
			this.checkBoxData.CheckedChanged += new System.EventHandler(this.checkBoxData_CheckedChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(9, 14);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(154, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Fresnel against cos(theta)";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(12, 431);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(266, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Diffuse Fresnel Reflectance (Fdr) against IOR";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.floatTrackbarControlIOR);
			this.panel1.Controls.Add(this.radioButtonIOR);
			this.panel1.Controls.Add(this.radioButtonSpecularTint);
			this.panel1.Controls.Add(this.panelColor);
			this.panel1.Location = new System.Drawing.Point(580, 61);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(243, 79);
			this.panel1.TabIndex = 10;
			// 
			// radioButtonIOR
			// 
			this.radioButtonIOR.AutoSize = true;
			this.radioButtonIOR.Checked = true;
			this.radioButtonIOR.Location = new System.Drawing.Point(0, 4);
			this.radioButtonIOR.Name = "radioButtonIOR";
			this.radioButtonIOR.Size = new System.Drawing.Size(44, 17);
			this.radioButtonIOR.TabIndex = 1;
			this.radioButtonIOR.TabStop = true;
			this.radioButtonIOR.Text = "IOR";
			this.radioButtonIOR.UseVisualStyleBackColor = true;
			this.radioButtonIOR.CheckedChanged += new System.EventHandler(this.radioButtonIOR_CheckedChanged);
			// 
			// radioButtonSpecularTint
			// 
			this.radioButtonSpecularTint.AutoSize = true;
			this.radioButtonSpecularTint.Location = new System.Drawing.Point(0, 34);
			this.radioButtonSpecularTint.Name = "radioButtonSpecularTint";
			this.radioButtonSpecularTint.Size = new System.Drawing.Size(88, 17);
			this.radioButtonSpecularTint.TabIndex = 1;
			this.radioButtonSpecularTint.Text = "Specular Tint";
			this.radioButtonSpecularTint.UseVisualStyleBackColor = true;
			this.radioButtonSpecularTint.CheckedChanged += new System.EventHandler(this.radioButtonSpecularTint_CheckedChanged);
			// 
			// floatTrackbarControlVerticalScale
			// 
			this.floatTrackbarControlVerticalScale.Location = new System.Drawing.Point(580, 463);
			this.floatTrackbarControlVerticalScale.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlVerticalScale.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlVerticalScale.Name = "floatTrackbarControlVerticalScale";
			this.floatTrackbarControlVerticalScale.RangeMax = 1F;
			this.floatTrackbarControlVerticalScale.RangeMin = 0.0001F;
			this.floatTrackbarControlVerticalScale.Size = new System.Drawing.Size(197, 20);
			this.floatTrackbarControlVerticalScale.TabIndex = 2;
			this.floatTrackbarControlVerticalScale.Value = 1F;
			this.floatTrackbarControlVerticalScale.VisibleRangeMax = 1F;
			this.floatTrackbarControlVerticalScale.VisibleRangeMin = 0.0001F;
			this.floatTrackbarControlVerticalScale.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlVerticalScale_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(580, 447);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 13);
			this.label1.TabIndex = 11;
			this.label1.Text = "Vertical Scale";
			// 
			// checkBoxusePreComputedTable
			// 
			this.checkBoxusePreComputedTable.AutoSize = true;
			this.checkBoxusePreComputedTable.Location = new System.Drawing.Point(577, 671);
			this.checkBoxusePreComputedTable.Name = "checkBoxusePreComputedTable";
			this.checkBoxusePreComputedTable.Size = new System.Drawing.Size(145, 17);
			this.checkBoxusePreComputedTable.TabIndex = 6;
			this.checkBoxusePreComputedTable.Text = "Use Pre-Computed Table";
			this.checkBoxusePreComputedTable.UseVisualStyleBackColor = true;
			this.checkBoxusePreComputedTable.CheckedChanged += new System.EventHandler(this.checkBoxusePreComputedtable_CheckedChanged);
			// 
			// floatTrackbarControlRoughness
			// 
			this.floatTrackbarControlRoughness.Enabled = false;
			this.floatTrackbarControlRoughness.Location = new System.Drawing.Point(577, 694);
			this.floatTrackbarControlRoughness.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlRoughness.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlRoughness.Name = "floatTrackbarControlRoughness";
			this.floatTrackbarControlRoughness.RangeMax = 1F;
			this.floatTrackbarControlRoughness.RangeMin = 0.0001F;
			this.floatTrackbarControlRoughness.Size = new System.Drawing.Size(197, 20);
			this.floatTrackbarControlRoughness.TabIndex = 2;
			this.floatTrackbarControlRoughness.Value = 1F;
			this.floatTrackbarControlRoughness.VisibleRangeMax = 1F;
			this.floatTrackbarControlRoughness.VisibleRangeMin = 0.0001F;
			this.floatTrackbarControlRoughness.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlRoughness_ValueChanged);
			// 
			// outputPanel2
			// 
			this.outputPanel2.FresnelType = TestFresnel.OutputPanel2.FRESNEL_TYPE.SCHLICK;
			this.outputPanel2.IOR = 1F;
			this.outputPanel2.Location = new System.Drawing.Point(12, 447);
			this.outputPanel2.MaxIOR = 10F;
			this.outputPanel2.Name = "outputPanel2";
			this.outputPanel2.Roughness = 1F;
			this.outputPanel2.Size = new System.Drawing.Size(559, 268);
			this.outputPanel2.TabIndex = 8;
			this.outputPanel2.VerticalScale = 1F;
			// 
			// outputPanel1
			// 
			this.outputPanel1.Controls.Add(this.label3);
			this.outputPanel1.Data = null;
			this.outputPanel1.FresnelType = TestFresnel.OutputPanel.FRESNEL_TYPE.SCHLICK;
			this.outputPanel1.FromData = false;
			this.outputPanel1.IOR_blue = 1F;
			this.outputPanel1.IOR_green = 1F;
			this.outputPanel1.IOR_red = 1F;
			this.outputPanel1.Location = new System.Drawing.Point(12, 35);
			this.outputPanel1.Name = "outputPanel1";
			this.outputPanel1.Size = new System.Drawing.Size(559, 377);
			this.outputPanel1.TabIndex = 0;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(-3, 396);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(128, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Fresnel against cos(theta)";
			// 
			// checkBoxPlotAgainstF0
			// 
			this.checkBoxPlotAgainstF0.AutoSize = true;
			this.checkBoxPlotAgainstF0.Location = new System.Drawing.Point(580, 489);
			this.checkBoxPlotAgainstF0.Name = "checkBoxPlotAgainstF0";
			this.checkBoxPlotAgainstF0.Size = new System.Drawing.Size(96, 17);
			this.checkBoxPlotAgainstF0.TabIndex = 6;
			this.checkBoxPlotAgainstF0.Text = "Plot against F0";
			this.checkBoxPlotAgainstF0.UseVisualStyleBackColor = true;
			this.checkBoxPlotAgainstF0.CheckedChanged += new System.EventHandler(this.checkBoxPlotAgainstF0_CheckedChanged);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(823, 727);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlRoughness);
			this.Controls.Add(this.floatTrackbarControlVerticalScale);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.outputPanel2);
			this.Controls.Add(this.checkBoxPlotAgainstF0);
			this.Controls.Add(this.checkBoxusePreComputedTable);
			this.Controls.Add(this.checkBoxData);
			this.Controls.Add(this.buttonLoadData);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.radioButtonPrecise);
			this.Controls.Add(this.radioButtonSchlick);
			this.Controls.Add(this.outputPanel1);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.outputPanel1.ResumeLayout(false);
			this.outputPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OutputPanel outputPanel1;
		private System.Windows.Forms.RadioButton radioButtonSchlick;
		private System.Windows.Forms.RadioButton radioButtonPrecise;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlIOR;
		private System.Windows.Forms.Panel panelColor;
		private System.Windows.Forms.ColorDialog colorDialog1;
		private System.Windows.Forms.OpenFileDialog openFileDialogRefract;
		private System.Windows.Forms.Button buttonLoadData;
		private System.Windows.Forms.CheckBox checkBoxData;
		private OutputPanel2 outputPanel2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.RadioButton radioButtonIOR;
		private System.Windows.Forms.RadioButton radioButtonSpecularTint;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlVerticalScale;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkBoxusePreComputedTable;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlRoughness;
		private System.Windows.Forms.CheckBox checkBoxPlotAgainstF0;
	}
}

