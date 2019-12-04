namespace LTCTableGenerator
{
	partial class FitterForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FitterForm));
			this.panelOutputSourceBRDF = new UIUtility.PanelOutput(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.panelOutputTargetBRDF = new UIUtility.PanelOutput(this.components);
			this.panelOutputDifference = new UIUtility.PanelOutput(this.components);
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.textBoxFitting = new System.Windows.Forms.TextBox();
			this.checkBoxPause = new System.Windows.Forms.CheckBox();
			this.integerTrackbarControlRoughnessIndex = new UIUtility.IntegerTrackbarControl();
			this.label8 = new System.Windows.Forms.Label();
			this.integerTrackbarControlThetaIndex = new UIUtility.IntegerTrackbarControl();
			this.label9 = new System.Windows.Forms.Label();
			this.checkBoxAutoRun = new System.Windows.Forms.CheckBox();
			this.checkBoxDoFitting = new System.Windows.Forms.CheckBox();
			this.integerTrackbarControlStepX = new UIUtility.IntegerTrackbarControl();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.integerTrackbarControlStepY = new UIUtility.IntegerTrackbarControl();
			this.buttonClear = new System.Windows.Forms.Button();
			this.floatTrackbarControl_m11 = new UIUtility.FloatTrackbarControl();
			this.label12 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.floatTrackbarControl_m13 = new UIUtility.FloatTrackbarControl();
			this.label15 = new System.Windows.Forms.Label();
			this.floatTrackbarControl_m22 = new UIUtility.FloatTrackbarControl();
			this.labelError = new System.Windows.Forms.Label();
			this.buttonClearRowsFromHere = new System.Windows.Forms.Button();
			this.checkBoxUsePreviousRoughness = new System.Windows.Forms.CheckBox();
			this.buttonClearColumnsFromHere = new System.Windows.Forms.Button();
			this.panelMatrixCoefficients = new System.Windows.Forms.Panel();
			this.buttonDebugLine = new System.Windows.Forms.Button();
			this.checkBoxClearPrev = new System.Windows.Forms.CheckBox();
			this.checkBoxClearNext = new System.Windows.Forms.CheckBox();
			this.panelMatrixCoefficients.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelOutputSourceBRDF
			// 
			this.panelOutputSourceBRDF.Location = new System.Drawing.Point(12, 35);
			this.panelOutputSourceBRDF.Name = "panelOutputSourceBRDF";
			this.panelOutputSourceBRDF.PanelBitmap = ((System.Drawing.Bitmap)(resources.GetObject("panelOutputSourceBRDF.PanelBitmap")));
			this.panelOutputSourceBRDF.Size = new System.Drawing.Size(340, 340);
			this.panelOutputSourceBRDF.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 378);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(73, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Source BRDF";
			// 
			// panelOutputTargetBRDF
			// 
			this.panelOutputTargetBRDF.Location = new System.Drawing.Point(358, 35);
			this.panelOutputTargetBRDF.Name = "panelOutputTargetBRDF";
			this.panelOutputTargetBRDF.PanelBitmap = ((System.Drawing.Bitmap)(resources.GetObject("panelOutputTargetBRDF.PanelBitmap")));
			this.panelOutputTargetBRDF.Size = new System.Drawing.Size(340, 340);
			this.panelOutputTargetBRDF.TabIndex = 0;
			// 
			// panelOutputDifference
			// 
			this.panelOutputDifference.Location = new System.Drawing.Point(704, 35);
			this.panelOutputDifference.Name = "panelOutputDifference";
			this.panelOutputDifference.PanelBitmap = ((System.Drawing.Bitmap)(resources.GetObject("panelOutputDifference.PanelBitmap")));
			this.panelOutputDifference.Size = new System.Drawing.Size(340, 340);
			this.panelOutputDifference.TabIndex = 0;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(355, 378);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(78, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Mapped BRDF";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(701, 378);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(71, 13);
			this.label3.TabIndex = 1;
			this.label3.Text = "Relative Error";
			// 
			// panel1
			// 
			this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
			this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.panel1.Location = new System.Drawing.Point(91, 378);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(261, 13);
			this.panel1.TabIndex = 2;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(88, 394);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(28, 13);
			this.label4.TabIndex = 1;
			this.label4.Text = "1e-4";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(324, 394);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(31, 13);
			this.label5.TabIndex = 1;
			this.label5.Text = "1e+4";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(780, 394);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(28, 13);
			this.label6.TabIndex = 1;
			this.label6.Text = "1e-4";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(1016, 394);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(31, 13);
			this.label7.TabIndex = 1;
			this.label7.Text = "1e+4";
			// 
			// panel2
			// 
			this.panel2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel2.BackgroundImage")));
			this.panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.panel2.Location = new System.Drawing.Point(783, 378);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(261, 13);
			this.panel2.TabIndex = 2;
			// 
			// textBoxFitting
			// 
			this.textBoxFitting.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxFitting.Location = new System.Drawing.Point(1050, 35);
			this.textBoxFitting.Multiline = true;
			this.textBoxFitting.Name = "textBoxFitting";
			this.textBoxFitting.ReadOnly = true;
			this.textBoxFitting.Size = new System.Drawing.Size(177, 340);
			this.textBoxFitting.TabIndex = 3;
			// 
			// checkBoxPause
			// 
			this.checkBoxPause.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBoxPause.AutoSize = true;
			this.checkBoxPause.Location = new System.Drawing.Point(604, 6);
			this.checkBoxPause.Name = "checkBoxPause";
			this.checkBoxPause.Size = new System.Drawing.Size(53, 23);
			this.checkBoxPause.TabIndex = 4;
			this.checkBoxPause.Text = "PAUSE";
			this.checkBoxPause.UseVisualStyleBackColor = true;
			this.checkBoxPause.CheckedChanged += new System.EventHandler(this.checkBoxPause_CheckedChanged);
			// 
			// integerTrackbarControlRoughnessIndex
			// 
			this.integerTrackbarControlRoughnessIndex.Enabled = false;
			this.integerTrackbarControlRoughnessIndex.Location = new System.Drawing.Point(95, 9);
			this.integerTrackbarControlRoughnessIndex.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlRoughnessIndex.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlRoughnessIndex.Name = "integerTrackbarControlRoughnessIndex";
			this.integerTrackbarControlRoughnessIndex.RangeMax = 63;
			this.integerTrackbarControlRoughnessIndex.RangeMin = 0;
			this.integerTrackbarControlRoughnessIndex.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlRoughnessIndex.TabIndex = 5;
			this.integerTrackbarControlRoughnessIndex.Value = 63;
			this.integerTrackbarControlRoughnessIndex.VisibleRangeMax = 63;
			this.integerTrackbarControlRoughnessIndex.ValueChanged += new UIUtility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlRoughnessIndex_ValueChanged);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(12, 11);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(77, 13);
			this.label8.TabIndex = 6;
			this.label8.Text = "Roughness (Y)";
			// 
			// integerTrackbarControlThetaIndex
			// 
			this.integerTrackbarControlThetaIndex.Enabled = false;
			this.integerTrackbarControlThetaIndex.Location = new System.Drawing.Point(386, 9);
			this.integerTrackbarControlThetaIndex.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlThetaIndex.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlThetaIndex.Name = "integerTrackbarControlThetaIndex";
			this.integerTrackbarControlThetaIndex.RangeMax = 63;
			this.integerTrackbarControlThetaIndex.RangeMin = 0;
			this.integerTrackbarControlThetaIndex.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlThetaIndex.TabIndex = 5;
			this.integerTrackbarControlThetaIndex.Value = 0;
			this.integerTrackbarControlThetaIndex.VisibleRangeMax = 63;
			this.integerTrackbarControlThetaIndex.ValueChanged += new UIUtility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlThetaIndex_ValueChanged);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(303, 11);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(81, 13);
			this.label9.TabIndex = 6;
			this.label9.Text = "cos(ThetaV) (X)";
			// 
			// checkBoxAutoRun
			// 
			this.checkBoxAutoRun.AutoSize = true;
			this.checkBoxAutoRun.Checked = true;
			this.checkBoxAutoRun.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAutoRun.Location = new System.Drawing.Point(663, 2);
			this.checkBoxAutoRun.Name = "checkBoxAutoRun";
			this.checkBoxAutoRun.Size = new System.Drawing.Size(71, 17);
			this.checkBoxAutoRun.TabIndex = 7;
			this.checkBoxAutoRun.Text = "Auto-Run";
			this.checkBoxAutoRun.UseVisualStyleBackColor = true;
			// 
			// checkBoxDoFitting
			// 
			this.checkBoxDoFitting.AutoSize = true;
			this.checkBoxDoFitting.Checked = true;
			this.checkBoxDoFitting.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxDoFitting.Location = new System.Drawing.Point(737, 2);
			this.checkBoxDoFitting.Name = "checkBoxDoFitting";
			this.checkBoxDoFitting.Size = new System.Drawing.Size(71, 17);
			this.checkBoxDoFitting.TabIndex = 7;
			this.checkBoxDoFitting.Text = "Do Fitting";
			this.checkBoxDoFitting.UseVisualStyleBackColor = true;
			// 
			// integerTrackbarControlStepX
			// 
			this.integerTrackbarControlStepX.Location = new System.Drawing.Point(942, 9);
			this.integerTrackbarControlStepX.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlStepX.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlStepX.Name = "integerTrackbarControlStepX";
			this.integerTrackbarControlStepX.RangeMax = 64;
			this.integerTrackbarControlStepX.RangeMin = 1;
			this.integerTrackbarControlStepX.Size = new System.Drawing.Size(117, 20);
			this.integerTrackbarControlStepX.TabIndex = 8;
			this.integerTrackbarControlStepX.Value = 1;
			this.integerTrackbarControlStepX.VisibleRangeMax = 64;
			this.integerTrackbarControlStepX.VisibleRangeMin = 1;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(900, 11);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(36, 13);
			this.label10.TabIndex = 6;
			this.label10.Text = "StepX";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(1068, 11);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(36, 13);
			this.label11.TabIndex = 6;
			this.label11.Text = "StepY";
			// 
			// integerTrackbarControlStepY
			// 
			this.integerTrackbarControlStepY.Location = new System.Drawing.Point(1110, 9);
			this.integerTrackbarControlStepY.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlStepY.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlStepY.Name = "integerTrackbarControlStepY";
			this.integerTrackbarControlStepY.RangeMax = 64;
			this.integerTrackbarControlStepY.RangeMin = 1;
			this.integerTrackbarControlStepY.Size = new System.Drawing.Size(117, 20);
			this.integerTrackbarControlStepY.TabIndex = 8;
			this.integerTrackbarControlStepY.Value = 1;
			this.integerTrackbarControlStepY.VisibleRangeMax = 64;
			this.integerTrackbarControlStepY.VisibleRangeMin = 1;
			// 
			// buttonClear
			// 
			this.buttonClear.Enabled = false;
			this.buttonClear.Location = new System.Drawing.Point(984, 412);
			this.buttonClear.Name = "buttonClear";
			this.buttonClear.Size = new System.Drawing.Size(75, 23);
			this.buttonClear.TabIndex = 9;
			this.buttonClear.Text = "Clear";
			this.buttonClear.UseVisualStyleBackColor = true;
			this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
			// 
			// floatTrackbarControl_m11
			// 
			this.floatTrackbarControl_m11.Location = new System.Drawing.Point(36, 4);
			this.floatTrackbarControl_m11.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl_m11.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl_m11.Name = "floatTrackbarControl_m11";
			this.floatTrackbarControl_m11.Size = new System.Drawing.Size(117, 20);
			this.floatTrackbarControl_m11.TabIndex = 10;
			this.floatTrackbarControl_m11.Value = 0F;
			this.floatTrackbarControl_m11.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControl_matrix_ValueChanged);
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(3, 7);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(27, 13);
			this.label12.TabIndex = 1;
			this.label12.Text = "m11";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(161, 7);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(27, 13);
			this.label14.TabIndex = 1;
			this.label14.Text = "m13";
			// 
			// floatTrackbarControl_m13
			// 
			this.floatTrackbarControl_m13.Location = new System.Drawing.Point(194, 4);
			this.floatTrackbarControl_m13.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl_m13.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl_m13.Name = "floatTrackbarControl_m13";
			this.floatTrackbarControl_m13.Size = new System.Drawing.Size(117, 20);
			this.floatTrackbarControl_m13.TabIndex = 10;
			this.floatTrackbarControl_m13.Value = 0F;
			this.floatTrackbarControl_m13.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControl_matrix_ValueChanged);
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(327, 7);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(27, 13);
			this.label15.TabIndex = 1;
			this.label15.Text = "m22";
			// 
			// floatTrackbarControl_m22
			// 
			this.floatTrackbarControl_m22.Location = new System.Drawing.Point(360, 4);
			this.floatTrackbarControl_m22.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl_m22.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl_m22.Name = "floatTrackbarControl_m22";
			this.floatTrackbarControl_m22.Size = new System.Drawing.Size(117, 20);
			this.floatTrackbarControl_m22.TabIndex = 10;
			this.floatTrackbarControl_m22.Value = 0F;
			this.floatTrackbarControl_m22.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControl_matrix_ValueChanged);
			// 
			// labelError
			// 
			this.labelError.AutoSize = true;
			this.labelError.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelError.Location = new System.Drawing.Point(483, 7);
			this.labelError.Name = "labelError";
			this.labelError.Size = new System.Drawing.Size(43, 15);
			this.labelError.TabIndex = 1;
			this.labelError.Text = "Error: 1";
			// 
			// buttonClearRowsFromHere
			// 
			this.buttonClearRowsFromHere.BackColor = System.Drawing.Color.IndianRed;
			this.buttonClearRowsFromHere.Enabled = false;
			this.buttonClearRowsFromHere.Location = new System.Drawing.Point(1065, 389);
			this.buttonClearRowsFromHere.Name = "buttonClearRowsFromHere";
			this.buttonClearRowsFromHere.Size = new System.Drawing.Size(85, 23);
			this.buttonClearRowsFromHere.TabIndex = 9;
			this.buttonClearRowsFromHere.Text = "Clear Rows";
			this.buttonClearRowsFromHere.UseVisualStyleBackColor = false;
			this.buttonClearRowsFromHere.Click += new System.EventHandler(this.buttonClearRowsFromHere_Click);
			// 
			// checkBoxUsePreviousRoughness
			// 
			this.checkBoxUsePreviousRoughness.AutoSize = true;
			this.checkBoxUsePreviousRoughness.Checked = true;
			this.checkBoxUsePreviousRoughness.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxUsePreviousRoughness.Location = new System.Drawing.Point(737, 18);
			this.checkBoxUsePreviousRoughness.Name = "checkBoxUsePreviousRoughness";
			this.checkBoxUsePreviousRoughness.Size = new System.Drawing.Size(130, 17);
			this.checkBoxUsePreviousRoughness.TabIndex = 7;
			this.checkBoxUsePreviousRoughness.Text = "Use Prev. Roughness";
			this.checkBoxUsePreviousRoughness.UseVisualStyleBackColor = true;
			// 
			// buttonClearColumnsFromHere
			// 
			this.buttonClearColumnsFromHere.BackColor = System.Drawing.Color.IndianRed;
			this.buttonClearColumnsFromHere.Enabled = false;
			this.buttonClearColumnsFromHere.Location = new System.Drawing.Point(1065, 412);
			this.buttonClearColumnsFromHere.Name = "buttonClearColumnsFromHere";
			this.buttonClearColumnsFromHere.Size = new System.Drawing.Size(85, 23);
			this.buttonClearColumnsFromHere.TabIndex = 9;
			this.buttonClearColumnsFromHere.Text = "Clear Columns";
			this.buttonClearColumnsFromHere.UseVisualStyleBackColor = false;
			this.buttonClearColumnsFromHere.Click += new System.EventHandler(this.buttonClearColumnsFromHere_Click);
			// 
			// panelMatrixCoefficients
			// 
			this.panelMatrixCoefficients.Controls.Add(this.label12);
			this.panelMatrixCoefficients.Controls.Add(this.floatTrackbarControl_m22);
			this.panelMatrixCoefficients.Controls.Add(this.labelError);
			this.panelMatrixCoefficients.Controls.Add(this.floatTrackbarControl_m13);
			this.panelMatrixCoefficients.Controls.Add(this.label14);
			this.panelMatrixCoefficients.Controls.Add(this.floatTrackbarControl_m11);
			this.panelMatrixCoefficients.Controls.Add(this.label15);
			this.panelMatrixCoefficients.Enabled = false;
			this.panelMatrixCoefficients.Location = new System.Drawing.Point(12, 407);
			this.panelMatrixCoefficients.Name = "panelMatrixCoefficients";
			this.panelMatrixCoefficients.Size = new System.Drawing.Size(674, 28);
			this.panelMatrixCoefficients.TabIndex = 11;
			// 
			// buttonDebugLine
			// 
			this.buttonDebugLine.Location = new System.Drawing.Point(713, 412);
			this.buttonDebugLine.Name = "buttonDebugLine";
			this.buttonDebugLine.Size = new System.Drawing.Size(75, 23);
			this.buttonDebugLine.TabIndex = 12;
			this.buttonDebugLine.Text = "Debug Line";
			this.buttonDebugLine.UseVisualStyleBackColor = true;
			this.buttonDebugLine.Click += new System.EventHandler(this.buttonDebugLine_Click);
			// 
			// checkBoxClearPrev
			// 
			this.checkBoxClearPrev.AutoSize = true;
			this.checkBoxClearPrev.Location = new System.Drawing.Point(1156, 393);
			this.checkBoxClearPrev.Name = "checkBoxClearPrev";
			this.checkBoxClearPrev.Size = new System.Drawing.Size(73, 17);
			this.checkBoxClearPrev.TabIndex = 13;
			this.checkBoxClearPrev.Text = "And Prev.";
			this.checkBoxClearPrev.UseVisualStyleBackColor = true;
			// 
			// checkBoxClearNext
			// 
			this.checkBoxClearNext.AutoSize = true;
			this.checkBoxClearNext.Location = new System.Drawing.Point(1156, 416);
			this.checkBoxClearNext.Name = "checkBoxClearNext";
			this.checkBoxClearNext.Size = new System.Drawing.Size(70, 17);
			this.checkBoxClearNext.TabIndex = 13;
			this.checkBoxClearNext.Text = "And Next";
			this.checkBoxClearNext.UseVisualStyleBackColor = true;
			// 
			// FitterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1239, 444);
			this.Controls.Add(this.checkBoxClearNext);
			this.Controls.Add(this.checkBoxClearPrev);
			this.Controls.Add(this.buttonDebugLine);
			this.Controls.Add(this.panelMatrixCoefficients);
			this.Controls.Add(this.buttonClearColumnsFromHere);
			this.Controls.Add(this.buttonClearRowsFromHere);
			this.Controls.Add(this.buttonClear);
			this.Controls.Add(this.integerTrackbarControlStepY);
			this.Controls.Add(this.integerTrackbarControlStepX);
			this.Controls.Add(this.checkBoxUsePreviousRoughness);
			this.Controls.Add(this.checkBoxDoFitting);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.checkBoxAutoRun);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.integerTrackbarControlThetaIndex);
			this.Controls.Add(this.integerTrackbarControlRoughnessIndex);
			this.Controls.Add(this.checkBoxPause);
			this.Controls.Add(this.textBoxFitting);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.panelOutputDifference);
			this.Controls.Add(this.panelOutputTargetBRDF);
			this.Controls.Add(this.panelOutputSourceBRDF);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FitterForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Fitter Debugger";
			this.panelMatrixCoefficients.ResumeLayout(false);
			this.panelMatrixCoefficients.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UIUtility.PanelOutput panelOutputSourceBRDF;
		private System.Windows.Forms.Label label1;
		private UIUtility.PanelOutput panelOutputTargetBRDF;
		private UIUtility.PanelOutput panelOutputDifference;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.TextBox textBoxFitting;
		private System.Windows.Forms.CheckBox checkBoxPause;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlRoughnessIndex;
		private System.Windows.Forms.Label label8;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlThetaIndex;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.CheckBox checkBoxAutoRun;
		private System.Windows.Forms.CheckBox checkBoxDoFitting;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlStepX;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlStepY;
		private System.Windows.Forms.Button buttonClear;
		private UIUtility.FloatTrackbarControl floatTrackbarControl_m11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label14;
		private UIUtility.FloatTrackbarControl floatTrackbarControl_m13;
		private System.Windows.Forms.Label label15;
		private UIUtility.FloatTrackbarControl floatTrackbarControl_m22;
		private System.Windows.Forms.Label labelError;
		private System.Windows.Forms.Button buttonClearRowsFromHere;
		private System.Windows.Forms.CheckBox checkBoxUsePreviousRoughness;
		private System.Windows.Forms.Button buttonClearColumnsFromHere;
		private System.Windows.Forms.Panel panelMatrixCoefficients;
		private System.Windows.Forms.Button buttonDebugLine;
		private System.Windows.Forms.CheckBox checkBoxClearPrev;
		private System.Windows.Forms.CheckBox checkBoxClearNext;
	}
}