namespace sRGBLinearConverter
{
	partial class ConverterForm
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
			this.colorDialog = new System.Windows.Forms.ColorDialog();
			this.panelColorsRGB = new System.Windows.Forms.Panel();
			this.groupBoxsRGB = new System.Windows.Forms.GroupBox();
			this.integerTrackbarControl255_sRGB = new UIUtility.IntegerTrackbarControl();
			this.floatTrackbarControlNormalized_sRGB = new UIUtility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.textBoxColorWeb_sRGB = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxColor255_sRGB = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxColorNormalized_sRGB = new System.Windows.Forms.TextBox();
			this.groupBoxLinear = new System.Windows.Forms.GroupBox();
			this.integerTrackbarControl255_Linear = new UIUtility.IntegerTrackbarControl();
			this.floatTrackbarControlNormalized_Linear = new UIUtility.FloatTrackbarControl();
			this.label6 = new System.Windows.Forms.Label();
			this.textBoxColorWeb_Linear = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.textBoxColor255_Linear = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.textBoxColorNormalized_Linear = new System.Windows.Forms.TextBox();
			this.panelColorLinear = new System.Windows.Forms.Panel();
			this.buttonLinear2sRGB = new System.Windows.Forms.Button();
			this.buttonsRGB2Linear = new System.Windows.Forms.Button();
			this.label11 = new System.Windows.Forms.Label();
			this.groupBoxsRGB.SuspendLayout();
			this.groupBoxLinear.SuspendLayout();
			this.SuspendLayout();
			// 
			// colorDialog
			// 
			this.colorDialog.AnyColor = true;
			this.colorDialog.FullOpen = true;
			// 
			// panelColorsRGB
			// 
			this.panelColorsRGB.BackColor = System.Drawing.Color.White;
			this.panelColorsRGB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelColorsRGB.Location = new System.Drawing.Point(79, 19);
			this.panelColorsRGB.Name = "panelColorsRGB";
			this.panelColorsRGB.Size = new System.Drawing.Size(102, 65);
			this.panelColorsRGB.TabIndex = 0;
			this.panelColorsRGB.Click += new System.EventHandler(this.panelColorsRGB_Click);
			// 
			// groupBoxsRGB
			// 
			this.groupBoxsRGB.Controls.Add(this.integerTrackbarControl255_sRGB);
			this.groupBoxsRGB.Controls.Add(this.floatTrackbarControlNormalized_sRGB);
			this.groupBoxsRGB.Controls.Add(this.label3);
			this.groupBoxsRGB.Controls.Add(this.textBoxColorWeb_sRGB);
			this.groupBoxsRGB.Controls.Add(this.label2);
			this.groupBoxsRGB.Controls.Add(this.textBoxColor255_sRGB);
			this.groupBoxsRGB.Controls.Add(this.label5);
			this.groupBoxsRGB.Controls.Add(this.label4);
			this.groupBoxsRGB.Controls.Add(this.label1);
			this.groupBoxsRGB.Controls.Add(this.textBoxColorNormalized_sRGB);
			this.groupBoxsRGB.Controls.Add(this.panelColorsRGB);
			this.groupBoxsRGB.Location = new System.Drawing.Point(10, 13);
			this.groupBoxsRGB.Name = "groupBoxsRGB";
			this.groupBoxsRGB.Size = new System.Drawing.Size(282, 235);
			this.groupBoxsRGB.TabIndex = 1;
			this.groupBoxsRGB.TabStop = false;
			this.groupBoxsRGB.Text = "sRGB";
			// 
			// integerTrackbarControl255_sRGB
			// 
			this.integerTrackbarControl255_sRGB.Location = new System.Drawing.Point(79, 205);
			this.integerTrackbarControl255_sRGB.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControl255_sRGB.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControl255_sRGB.Name = "integerTrackbarControl255_sRGB";
			this.integerTrackbarControl255_sRGB.RangeMax = 255;
			this.integerTrackbarControl255_sRGB.RangeMin = 0;
			this.integerTrackbarControl255_sRGB.Size = new System.Drawing.Size(194, 20);
			this.integerTrackbarControl255_sRGB.TabIndex = 5;
			this.integerTrackbarControl255_sRGB.Value = 255;
			this.integerTrackbarControl255_sRGB.VisibleRangeMax = 255;
			this.integerTrackbarControl255_sRGB.ValueChanged += new UIUtility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControl255_sRGB_ValueChanged);
			// 
			// floatTrackbarControlNormalized_sRGB
			// 
			this.floatTrackbarControlNormalized_sRGB.Location = new System.Drawing.Point(79, 180);
			this.floatTrackbarControlNormalized_sRGB.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlNormalized_sRGB.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlNormalized_sRGB.Name = "floatTrackbarControlNormalized_sRGB";
			this.floatTrackbarControlNormalized_sRGB.RangeMax = 1F;
			this.floatTrackbarControlNormalized_sRGB.RangeMin = 0F;
			this.floatTrackbarControlNormalized_sRGB.Size = new System.Drawing.Size(194, 20);
			this.floatTrackbarControlNormalized_sRGB.TabIndex = 4;
			this.floatTrackbarControlNormalized_sRGB.Value = 1F;
			this.floatTrackbarControlNormalized_sRGB.VisibleRangeMax = 1F;
			this.floatTrackbarControlNormalized_sRGB.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlNormalized_sRGB_ValueChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 144);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(87, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Color #RRGGBB";
			// 
			// textBoxColorWeb_sRGB
			// 
			this.textBoxColorWeb_sRGB.Location = new System.Drawing.Point(99, 141);
			this.textBoxColorWeb_sRGB.Name = "textBoxColorWeb_sRGB";
			this.textBoxColorWeb_sRGB.ReadOnly = true;
			this.textBoxColorWeb_sRGB.Size = new System.Drawing.Size(174, 20);
			this.textBoxColorWeb_sRGB.TabIndex = 2;
			this.textBoxColorWeb_sRGB.DoubleClick += new System.EventHandler(this.textBoxColorWeb_sRGB_DoubleClick);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 117);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(67, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Color [0,255]";
			// 
			// textBoxColor255_sRGB
			// 
			this.textBoxColor255_sRGB.Location = new System.Drawing.Point(79, 114);
			this.textBoxColor255_sRGB.Name = "textBoxColor255_sRGB";
			this.textBoxColor255_sRGB.ReadOnly = true;
			this.textBoxColor255_sRGB.Size = new System.Drawing.Size(194, 20);
			this.textBoxColor255_sRGB.TabIndex = 2;
			this.textBoxColor255_sRGB.DoubleClick += new System.EventHandler(this.textBoxColor255_sRGB_DoubleClick);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(7, 208);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(70, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "Value [0,255]";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(7, 183);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(58, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Value [0,1]";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 92);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(55, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Color [0,1]";
			// 
			// textBoxColorNormalized_sRGB
			// 
			this.textBoxColorNormalized_sRGB.Location = new System.Drawing.Point(79, 89);
			this.textBoxColorNormalized_sRGB.Name = "textBoxColorNormalized_sRGB";
			this.textBoxColorNormalized_sRGB.ReadOnly = true;
			this.textBoxColorNormalized_sRGB.Size = new System.Drawing.Size(194, 20);
			this.textBoxColorNormalized_sRGB.TabIndex = 2;
			this.textBoxColorNormalized_sRGB.DoubleClick += new System.EventHandler(this.textBoxColorNormalized_sRGB_DoubleClick);
			// 
			// groupBoxLinear
			// 
			this.groupBoxLinear.Controls.Add(this.integerTrackbarControl255_Linear);
			this.groupBoxLinear.Controls.Add(this.floatTrackbarControlNormalized_Linear);
			this.groupBoxLinear.Controls.Add(this.label6);
			this.groupBoxLinear.Controls.Add(this.textBoxColorWeb_Linear);
			this.groupBoxLinear.Controls.Add(this.label7);
			this.groupBoxLinear.Controls.Add(this.textBoxColor255_Linear);
			this.groupBoxLinear.Controls.Add(this.label8);
			this.groupBoxLinear.Controls.Add(this.label9);
			this.groupBoxLinear.Controls.Add(this.label10);
			this.groupBoxLinear.Controls.Add(this.textBoxColorNormalized_Linear);
			this.groupBoxLinear.Controls.Add(this.panelColorLinear);
			this.groupBoxLinear.Location = new System.Drawing.Point(374, 13);
			this.groupBoxLinear.Name = "groupBoxLinear";
			this.groupBoxLinear.Size = new System.Drawing.Size(282, 235);
			this.groupBoxLinear.TabIndex = 1;
			this.groupBoxLinear.TabStop = false;
			this.groupBoxLinear.Text = "Linear";
			// 
			// integerTrackbarControl255_Linear
			// 
			this.integerTrackbarControl255_Linear.Location = new System.Drawing.Point(79, 205);
			this.integerTrackbarControl255_Linear.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControl255_Linear.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControl255_Linear.Name = "integerTrackbarControl255_Linear";
			this.integerTrackbarControl255_Linear.RangeMax = 255;
			this.integerTrackbarControl255_Linear.RangeMin = 0;
			this.integerTrackbarControl255_Linear.Size = new System.Drawing.Size(194, 20);
			this.integerTrackbarControl255_Linear.TabIndex = 5;
			this.integerTrackbarControl255_Linear.Value = 255;
			this.integerTrackbarControl255_Linear.VisibleRangeMax = 255;
			this.integerTrackbarControl255_Linear.ValueChanged += new UIUtility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControl255_Linear_ValueChanged);
			// 
			// floatTrackbarControlNormalized_Linear
			// 
			this.floatTrackbarControlNormalized_Linear.Location = new System.Drawing.Point(79, 180);
			this.floatTrackbarControlNormalized_Linear.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlNormalized_Linear.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlNormalized_Linear.Name = "floatTrackbarControlNormalized_Linear";
			this.floatTrackbarControlNormalized_Linear.RangeMax = 1F;
			this.floatTrackbarControlNormalized_Linear.RangeMin = 0F;
			this.floatTrackbarControlNormalized_Linear.Size = new System.Drawing.Size(194, 20);
			this.floatTrackbarControlNormalized_Linear.TabIndex = 4;
			this.floatTrackbarControlNormalized_Linear.Value = 1F;
			this.floatTrackbarControlNormalized_Linear.VisibleRangeMax = 1F;
			this.floatTrackbarControlNormalized_Linear.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlNormalized_Linear_ValueChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 144);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(87, 13);
			this.label6.TabIndex = 3;
			this.label6.Text = "Color #RRGGBB";
			// 
			// textBoxColorWeb_Linear
			// 
			this.textBoxColorWeb_Linear.Location = new System.Drawing.Point(99, 141);
			this.textBoxColorWeb_Linear.Name = "textBoxColorWeb_Linear";
			this.textBoxColorWeb_Linear.ReadOnly = true;
			this.textBoxColorWeb_Linear.Size = new System.Drawing.Size(174, 20);
			this.textBoxColorWeb_Linear.TabIndex = 2;
			this.textBoxColorWeb_Linear.DoubleClick += new System.EventHandler(this.textBoxColorWeb_Linear_DoubleClick);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(6, 117);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(67, 13);
			this.label7.TabIndex = 3;
			this.label7.Text = "Color [0,255]";
			// 
			// textBoxColor255_Linear
			// 
			this.textBoxColor255_Linear.Location = new System.Drawing.Point(79, 114);
			this.textBoxColor255_Linear.Name = "textBoxColor255_Linear";
			this.textBoxColor255_Linear.ReadOnly = true;
			this.textBoxColor255_Linear.Size = new System.Drawing.Size(194, 20);
			this.textBoxColor255_Linear.TabIndex = 2;
			this.textBoxColor255_Linear.DoubleClick += new System.EventHandler(this.textBoxColor255_Linear_DoubleClick);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(7, 208);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(70, 13);
			this.label8.TabIndex = 3;
			this.label8.Text = "Value [0,255]";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(7, 183);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(58, 13);
			this.label9.TabIndex = 3;
			this.label9.Text = "Value [0,1]";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(7, 92);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(55, 13);
			this.label10.TabIndex = 3;
			this.label10.Text = "Color [0,1]";
			// 
			// textBoxColorNormalized_Linear
			// 
			this.textBoxColorNormalized_Linear.Location = new System.Drawing.Point(79, 89);
			this.textBoxColorNormalized_Linear.Name = "textBoxColorNormalized_Linear";
			this.textBoxColorNormalized_Linear.ReadOnly = true;
			this.textBoxColorNormalized_Linear.Size = new System.Drawing.Size(194, 20);
			this.textBoxColorNormalized_Linear.TabIndex = 2;
			this.textBoxColorNormalized_Linear.DoubleClick += new System.EventHandler(this.textBoxColorNormalized_Linear_DoubleClick);
			// 
			// panelColorLinear
			// 
			this.panelColorLinear.BackColor = System.Drawing.Color.White;
			this.panelColorLinear.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelColorLinear.Location = new System.Drawing.Point(79, 19);
			this.panelColorLinear.Name = "panelColorLinear";
			this.panelColorLinear.Size = new System.Drawing.Size(102, 65);
			this.panelColorLinear.TabIndex = 0;
			this.panelColorLinear.Click += new System.EventHandler(this.panelColorLinear_Click);
			// 
			// buttonLinear2sRGB
			// 
			this.buttonLinear2sRGB.Image = global::sRGBLinearConverter.Properties.Resources.ArrowLeft;
			this.buttonLinear2sRGB.Location = new System.Drawing.Point(298, 126);
			this.buttonLinear2sRGB.Name = "buttonLinear2sRGB";
			this.buttonLinear2sRGB.Size = new System.Drawing.Size(70, 70);
			this.buttonLinear2sRGB.TabIndex = 2;
			this.buttonLinear2sRGB.UseVisualStyleBackColor = true;
			this.buttonLinear2sRGB.Click += new System.EventHandler(this.buttonLinear2sRGB_Click);
			// 
			// buttonsRGB2Linear
			// 
			this.buttonsRGB2Linear.Image = global::sRGBLinearConverter.Properties.Resources.ArrowRight;
			this.buttonsRGB2Linear.Location = new System.Drawing.Point(298, 52);
			this.buttonsRGB2Linear.Name = "buttonsRGB2Linear";
			this.buttonsRGB2Linear.Size = new System.Drawing.Size(70, 70);
			this.buttonsRGB2Linear.TabIndex = 2;
			this.buttonsRGB2Linear.UseVisualStyleBackColor = true;
			this.buttonsRGB2Linear.Click += new System.EventHandler(this.buttonsRGB2Linear_Click);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label11.Location = new System.Drawing.Point(12, 260);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(358, 13);
			this.label11.TabIndex = 3;
			this.label11.Text = "HINT: Double click color textboxes to copy value to clipboard";
			// 
			// ConverterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(667, 282);
			this.Controls.Add(this.buttonLinear2sRGB);
			this.Controls.Add(this.buttonsRGB2Linear);
			this.Controls.Add(this.groupBoxLinear);
			this.Controls.Add(this.groupBoxsRGB);
			this.Controls.Add(this.label11);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ConverterForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "sRGB <=> Linear Converter";
			this.groupBoxsRGB.ResumeLayout(false);
			this.groupBoxsRGB.PerformLayout();
			this.groupBoxLinear.ResumeLayout(false);
			this.groupBoxLinear.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ColorDialog colorDialog;
		private System.Windows.Forms.Panel panelColorsRGB;
		private System.Windows.Forms.GroupBox groupBoxsRGB;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxColorNormalized_sRGB;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBoxColorWeb_sRGB;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBoxColor255_sRGB;
		private UIUtility.FloatTrackbarControl floatTrackbarControlNormalized_sRGB;
		private System.Windows.Forms.Label label4;
		private UIUtility.IntegerTrackbarControl integerTrackbarControl255_sRGB;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.GroupBox groupBoxLinear;
		private UIUtility.IntegerTrackbarControl integerTrackbarControl255_Linear;
		private UIUtility.FloatTrackbarControl floatTrackbarControlNormalized_Linear;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox textBoxColorWeb_Linear;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox textBoxColor255_Linear;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox textBoxColorNormalized_Linear;
		private System.Windows.Forms.Panel panelColorLinear;
		private System.Windows.Forms.Button buttonsRGB2Linear;
		private System.Windows.Forms.Button buttonLinear2sRGB;
		private System.Windows.Forms.Label label11;
	}
}

