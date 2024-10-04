namespace WoodCordsComputer {
	partial class Form1 {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing ) {
			if ( disposing && (components != null) ) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.floatTrackbarControlUnitX = new UIUtility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlUnitY = new UIUtility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.floatTrackbarControlLogDepth = new UIUtility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.floatTrackbarControlSurfaceCoverage = new UIUtility.FloatTrackbarControl();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.textBoxCoordinates = new System.Windows.Forms.TextBox();
			this.radioButton2 = new System.Windows.Forms.RadioButton();
			this.radioButton3 = new System.Windows.Forms.RadioButton();
			this.radioButton4 = new System.Windows.Forms.RadioButton();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.textBoxResults = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// floatTrackbarControlUnitX
			// 
			this.floatTrackbarControlUnitX.Location = new System.Drawing.Point(125, 15);
			this.floatTrackbarControlUnitX.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlUnitX.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlUnitX.Name = "floatTrackbarControlUnitX";
			this.floatTrackbarControlUnitX.RangeMin = 0F;
			this.floatTrackbarControlUnitX.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlUnitX.TabIndex = 0;
			this.floatTrackbarControlUnitX.Value = 0.41F;
			this.floatTrackbarControlUnitX.VisibleRangeMax = 1F;
			this.floatTrackbarControlUnitX.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlUnitX_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(107, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "1 Unit along X axis = ";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(331, 20);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(38, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "meters";
			// 
			// floatTrackbarControlUnitY
			// 
			this.floatTrackbarControlUnitY.Location = new System.Drawing.Point(125, 41);
			this.floatTrackbarControlUnitY.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlUnitY.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlUnitY.Name = "floatTrackbarControlUnitY";
			this.floatTrackbarControlUnitY.RangeMin = 0F;
			this.floatTrackbarControlUnitY.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlUnitY.TabIndex = 0;
			this.floatTrackbarControlUnitY.Value = 0.26F;
			this.floatTrackbarControlUnitY.VisibleRangeMax = 1F;
			this.floatTrackbarControlUnitY.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlUnitY_ValueChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 46);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(107, 13);
			this.label3.TabIndex = 1;
			this.label3.Text = "1 Unit along Y axis = ";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(331, 46);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(38, 13);
			this.label4.TabIndex = 1;
			this.label4.Text = "meters";
			// 
			// floatTrackbarControlLogDepth
			// 
			this.floatTrackbarControlLogDepth.Location = new System.Drawing.Point(88, 19);
			this.floatTrackbarControlLogDepth.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlLogDepth.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlLogDepth.Name = "floatTrackbarControlLogDepth";
			this.floatTrackbarControlLogDepth.RangeMin = 0F;
			this.floatTrackbarControlLogDepth.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlLogDepth.TabIndex = 0;
			this.floatTrackbarControlLogDepth.Value = 0.38F;
			this.floatTrackbarControlLogDepth.VisibleRangeMax = 1F;
			this.floatTrackbarControlLogDepth.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlLogDepth_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(13, 24);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(69, 13);
			this.label5.TabIndex = 1;
			this.label5.Text = "Log Depth = ";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(294, 24);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(38, 13);
			this.label6.TabIndex = 1;
			this.label6.Text = "meters";
			// 
			// floatTrackbarControlSurfaceCoverage
			// 
			this.floatTrackbarControlSurfaceCoverage.Location = new System.Drawing.Point(505, 30);
			this.floatTrackbarControlSurfaceCoverage.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSurfaceCoverage.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSurfaceCoverage.Name = "floatTrackbarControlSurfaceCoverage";
			this.floatTrackbarControlSurfaceCoverage.RangeMax = 100F;
			this.floatTrackbarControlSurfaceCoverage.RangeMin = 0F;
			this.floatTrackbarControlSurfaceCoverage.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlSurfaceCoverage.TabIndex = 0;
			this.floatTrackbarControlSurfaceCoverage.Value = 75F;
			this.floatTrackbarControlSurfaceCoverage.VisibleRangeMax = 100F;
			this.floatTrackbarControlSurfaceCoverage.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlSurfaceCoverage_ValueChanged);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(394, 34);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(105, 13);
			this.label7.TabIndex = 1;
			this.label7.Text = "Surface Coverage = ";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(711, 35);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(15, 13);
			this.label8.TabIndex = 1;
			this.label8.Text = "%";
			// 
			// radioButton1
			// 
			this.radioButton1.AutoSize = true;
			this.radioButton1.Checked = true;
			this.radioButton1.Location = new System.Drawing.Point(15, 80);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(54, 17);
			this.radioButton1.TabIndex = 3;
			this.radioButton1.TabStop = true;
			this.radioButton1.Text = "Line 1";
			this.radioButton1.UseVisualStyleBackColor = true;
			this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.textBoxCoordinates);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.floatTrackbarControlLogDepth);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.label10);
			this.groupBox1.Location = new System.Drawing.Point(16, 103);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(342, 203);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Line Info";
			// 
			// textBoxCoordinates
			// 
			this.textBoxCoordinates.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxCoordinates.Location = new System.Drawing.Point(6, 45);
			this.textBoxCoordinates.Multiline = true;
			this.textBoxCoordinates.Name = "textBoxCoordinates";
			this.textBoxCoordinates.Size = new System.Drawing.Size(326, 127);
			this.textBoxCoordinates.TabIndex = 3;
			this.textBoxCoordinates.TextChanged += new System.EventHandler(this.textBoxCoordinates_TextChanged);
			// 
			// radioButton2
			// 
			this.radioButton2.AutoSize = true;
			this.radioButton2.Location = new System.Drawing.Point(75, 80);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = new System.Drawing.Size(54, 17);
			this.radioButton2.TabIndex = 3;
			this.radioButton2.Text = "Line 2";
			this.radioButton2.UseVisualStyleBackColor = true;
			this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
			// 
			// radioButton3
			// 
			this.radioButton3.AutoSize = true;
			this.radioButton3.Location = new System.Drawing.Point(135, 80);
			this.radioButton3.Name = "radioButton3";
			this.radioButton3.Size = new System.Drawing.Size(54, 17);
			this.radioButton3.TabIndex = 3;
			this.radioButton3.Text = "Line 3";
			this.radioButton3.UseVisualStyleBackColor = true;
			this.radioButton3.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
			// 
			// radioButton4
			// 
			this.radioButton4.AutoSize = true;
			this.radioButton4.Location = new System.Drawing.Point(195, 80);
			this.radioButton4.Name = "radioButton4";
			this.radioButton4.Size = new System.Drawing.Size(54, 17);
			this.radioButton4.TabIndex = 3;
			this.radioButton4.Text = "Line 4";
			this.radioButton4.UseVisualStyleBackColor = true;
			this.radioButton4.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.textBoxResults);
			this.groupBox2.Location = new System.Drawing.Point(364, 103);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(351, 197);
			this.groupBox2.TabIndex = 5;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Results";
			// 
			// textBoxResults
			// 
			this.textBoxResults.AcceptsReturn = true;
			this.textBoxResults.AcceptsTab = true;
			this.textBoxResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxResults.Location = new System.Drawing.Point(6, 19);
			this.textBoxResults.Multiline = true;
			this.textBoxResults.Name = "textBoxResults";
			this.textBoxResults.ReadOnly = true;
			this.textBoxResults.Size = new System.Drawing.Size(339, 172);
			this.textBoxResults.TabIndex = 0;
			// 
			// label9
			// 
			this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label9.Location = new System.Drawing.Point(367, 62);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(342, 38);
			this.label9.TabIndex = 1;
			this.label9.Text = "A cord is 8\' x 4\' x 4\' (L x H x D) = 128 ft³\n or 2.44 x 1.22 x 1.22 = 3.6245 m³";
			// 
			// label10
			// 
			this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(6, 178);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(226, 13);
			this.label10.TabIndex = 1;
			this.label10.Text = "Use X,Y; X,Y; X,Y; ... syntax for polygon points";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(727, 318);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.radioButton4);
			this.Controls.Add(this.radioButton3);
			this.Controls.Add(this.radioButton2);
			this.Controls.Add(this.radioButton1);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlSurfaceCoverage);
			this.Controls.Add(this.floatTrackbarControlUnitY);
			this.Controls.Add(this.floatTrackbarControlUnitX);
			this.Name = "Form1";
			this.Text = "Form1";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UIUtility.FloatTrackbarControl floatTrackbarControlUnitX;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private UIUtility.FloatTrackbarControl floatTrackbarControlUnitY;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private UIUtility.FloatTrackbarControl floatTrackbarControlLogDepth;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private UIUtility.FloatTrackbarControl floatTrackbarControlSurfaceCoverage;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox textBoxCoordinates;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.RadioButton radioButton3;
		private System.Windows.Forms.RadioButton radioButton4;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox textBoxResults;
		private System.Windows.Forms.Label label10;
	}
}

