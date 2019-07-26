namespace TestGraphHeatEquation
{
	partial class GraphForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.checkBoxRun = new System.Windows.Forms.CheckBox();
			this.buttonReset = new System.Windows.Forms.Button();
			this.floatTrackbarControlDiffusionCoefficient = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.panelOutput = new TestGraphHeatEquation.PanelOutput(this.components);
			this.buttonReload = new System.Windows.Forms.Button();
			this.floatTrackbarControlDeltaTime = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.buttonResetObstacles = new System.Windows.Forms.Button();
			this.integerTrackbarControlStartPosition = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.textBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.buttonResetSimulation = new System.Windows.Forms.Button();
			this.buttonStepSimulation = new System.Windows.Forms.Button();
			this.buttonRunSimulation = new System.Windows.Forms.Button();
			this.checkBoxShowSearch = new System.Windows.Forms.CheckBox();
			this.radioButtonAlgo0 = new System.Windows.Forms.RadioButton();
			this.radioButtonAlgo1 = new System.Windows.Forms.RadioButton();
			this.checkBoxShowLog = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// checkBoxRun
			// 
			this.checkBoxRun.AutoSize = true;
			this.checkBoxRun.Checked = true;
			this.checkBoxRun.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxRun.Location = new System.Drawing.Point(533, 76);
			this.checkBoxRun.Name = "checkBoxRun";
			this.checkBoxRun.Size = new System.Drawing.Size(46, 17);
			this.checkBoxRun.TabIndex = 2;
			this.checkBoxRun.Text = "Run";
			this.checkBoxRun.UseVisualStyleBackColor = true;
			this.checkBoxRun.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
			// 
			// buttonReset
			// 
			this.buttonReset.Location = new System.Drawing.Point(585, 72);
			this.buttonReset.Name = "buttonReset";
			this.buttonReset.Size = new System.Drawing.Size(75, 23);
			this.buttonReset.TabIndex = 3;
			this.buttonReset.Text = "Reset";
			this.buttonReset.UseVisualStyleBackColor = true;
			this.buttonReset.Click += new System.EventHandler(this.button1_Click);
			// 
			// floatTrackbarControlDiffusionCoefficient
			// 
			this.floatTrackbarControlDiffusionCoefficient.Location = new System.Drawing.Point(645, 40);
			this.floatTrackbarControlDiffusionCoefficient.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDiffusionCoefficient.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDiffusionCoefficient.Name = "floatTrackbarControlDiffusionCoefficient";
			this.floatTrackbarControlDiffusionCoefficient.RangeMax = 1F;
			this.floatTrackbarControlDiffusionCoefficient.RangeMin = 0F;
			this.floatTrackbarControlDiffusionCoefficient.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDiffusionCoefficient.TabIndex = 1;
			this.floatTrackbarControlDiffusionCoefficient.Value = 0.95F;
			this.floatTrackbarControlDiffusionCoefficient.VisibleRangeMax = 1F;
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(512, 512);
			this.panelOutput.TabIndex = 0;
			this.panelOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseDown);
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(770, 508);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 4;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// floatTrackbarControlDeltaTime
			// 
			this.floatTrackbarControlDeltaTime.Location = new System.Drawing.Point(645, 14);
			this.floatTrackbarControlDeltaTime.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDeltaTime.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDeltaTime.Name = "floatTrackbarControlDeltaTime";
			this.floatTrackbarControlDeltaTime.RangeMax = 100000F;
			this.floatTrackbarControlDeltaTime.RangeMin = 0F;
			this.floatTrackbarControlDeltaTime.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDeltaTime.TabIndex = 1;
			this.floatTrackbarControlDeltaTime.Value = 1F;
			this.floatTrackbarControlDeltaTime.VisibleRangeMax = 1F;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(530, 19);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(58, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "Delta Time";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(530, 45);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(101, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Diffusion Coefficient";
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 10;
			// 
			// buttonResetObstacles
			// 
			this.buttonResetObstacles.Location = new System.Drawing.Point(533, 101);
			this.buttonResetObstacles.Name = "buttonResetObstacles";
			this.buttonResetObstacles.Size = new System.Drawing.Size(178, 23);
			this.buttonResetObstacles.TabIndex = 3;
			this.buttonResetObstacles.Text = "Reset Obstacles";
			this.buttonResetObstacles.UseVisualStyleBackColor = true;
			this.buttonResetObstacles.Click += new System.EventHandler(this.buttonResetObstacles_Click);
			// 
			// integerTrackbarControlStartPosition
			// 
			this.integerTrackbarControlStartPosition.Enabled = false;
			this.integerTrackbarControlStartPosition.Location = new System.Drawing.Point(645, 204);
			this.integerTrackbarControlStartPosition.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlStartPosition.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlStartPosition.Name = "integerTrackbarControlStartPosition";
			this.integerTrackbarControlStartPosition.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlStartPosition.TabIndex = 6;
			this.integerTrackbarControlStartPosition.Value = 0;
			this.integerTrackbarControlStartPosition.EnabledChanged += new System.EventHandler(this.integerTrackbarControlStartPosition_EnabledChanged);
			// 
			// textBox
			// 
			this.textBox.Location = new System.Drawing.Point(530, 321);
			this.textBox.Multiline = true;
			this.textBox.Name = "textBox";
			this.textBox.ReadOnly = true;
			this.textBox.Size = new System.Drawing.Size(315, 181);
			this.textBox.TabIndex = 7;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(530, 209);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(69, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Start Position";
			// 
			// buttonResetSimulation
			// 
			this.buttonResetSimulation.Enabled = false;
			this.buttonResetSimulation.Location = new System.Drawing.Point(533, 238);
			this.buttonResetSimulation.Name = "buttonResetSimulation";
			this.buttonResetSimulation.Size = new System.Drawing.Size(75, 23);
			this.buttonResetSimulation.TabIndex = 8;
			this.buttonResetSimulation.Text = "Reset";
			this.buttonResetSimulation.UseVisualStyleBackColor = true;
			this.buttonResetSimulation.Click += new System.EventHandler(this.buttonResetSimulation_Click);
			// 
			// buttonStepSimulation
			// 
			this.buttonStepSimulation.Enabled = false;
			this.buttonStepSimulation.Location = new System.Drawing.Point(614, 238);
			this.buttonStepSimulation.Name = "buttonStepSimulation";
			this.buttonStepSimulation.Size = new System.Drawing.Size(75, 23);
			this.buttonStepSimulation.TabIndex = 8;
			this.buttonStepSimulation.Text = "Step";
			this.buttonStepSimulation.UseVisualStyleBackColor = true;
			this.buttonStepSimulation.Click += new System.EventHandler(this.buttonStepSimulation_Click);
			// 
			// buttonRunSimulation
			// 
			this.buttonRunSimulation.Enabled = false;
			this.buttonRunSimulation.Location = new System.Drawing.Point(695, 238);
			this.buttonRunSimulation.Name = "buttonRunSimulation";
			this.buttonRunSimulation.Size = new System.Drawing.Size(75, 23);
			this.buttonRunSimulation.TabIndex = 8;
			this.buttonRunSimulation.Text = "Run";
			this.buttonRunSimulation.UseVisualStyleBackColor = true;
			this.buttonRunSimulation.Click += new System.EventHandler(this.buttonRunSimulation_Click);
			// 
			// checkBoxShowSearch
			// 
			this.checkBoxShowSearch.AutoSize = true;
			this.checkBoxShowSearch.Checked = true;
			this.checkBoxShowSearch.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowSearch.Location = new System.Drawing.Point(533, 267);
			this.checkBoxShowSearch.Name = "checkBoxShowSearch";
			this.checkBoxShowSearch.Size = new System.Drawing.Size(123, 17);
			this.checkBoxShowSearch.TabIndex = 9;
			this.checkBoxShowSearch.Text = "Show Search Result";
			this.checkBoxShowSearch.UseVisualStyleBackColor = true;
			// 
			// radioButtonAlgo0
			// 
			this.radioButtonAlgo0.AutoSize = true;
			this.radioButtonAlgo0.Checked = true;
			this.radioButtonAlgo0.Location = new System.Drawing.Point(533, 290);
			this.radioButtonAlgo0.Name = "radioButtonAlgo0";
			this.radioButtonAlgo0.Size = new System.Drawing.Size(75, 17);
			this.radioButtonAlgo0.TabIndex = 10;
			this.radioButtonAlgo0.TabStop = true;
			this.radioButtonAlgo0.Text = "Algo Local";
			this.radioButtonAlgo0.UseVisualStyleBackColor = true;
			// 
			// radioButtonAlgo1
			// 
			this.radioButtonAlgo1.AutoSize = true;
			this.radioButtonAlgo1.Location = new System.Drawing.Point(618, 290);
			this.radioButtonAlgo1.Name = "radioButtonAlgo1";
			this.radioButtonAlgo1.Size = new System.Drawing.Size(79, 17);
			this.radioButtonAlgo1.TabIndex = 10;
			this.radioButtonAlgo1.TabStop = true;
			this.radioButtonAlgo1.Text = "Algo Global";
			this.radioButtonAlgo1.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowLog
			// 
			this.checkBoxShowLog.AutoSize = true;
			this.checkBoxShowLog.Location = new System.Drawing.Point(533, 155);
			this.checkBoxShowLog.Name = "checkBoxShowLog";
			this.checkBoxShowLog.Size = new System.Drawing.Size(101, 17);
			this.checkBoxShowLog.TabIndex = 11;
			this.checkBoxShowLog.Text = "Show Log(heat)";
			this.checkBoxShowLog.UseVisualStyleBackColor = true;
			// 
			// GraphForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(857, 543);
			this.Controls.Add(this.checkBoxShowLog);
			this.Controls.Add(this.radioButtonAlgo1);
			this.Controls.Add(this.radioButtonAlgo0);
			this.Controls.Add(this.checkBoxShowSearch);
			this.Controls.Add(this.buttonRunSimulation);
			this.Controls.Add(this.buttonStepSimulation);
			this.Controls.Add(this.buttonResetSimulation);
			this.Controls.Add(this.textBox);
			this.Controls.Add(this.integerTrackbarControlStartPosition);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.buttonResetObstacles);
			this.Controls.Add(this.buttonReset);
			this.Controls.Add(this.checkBoxRun);
			this.Controls.Add(this.floatTrackbarControlDeltaTime);
			this.Controls.Add(this.floatTrackbarControlDiffusionCoefficient);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "GraphForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Heat Wave Test";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelOutput;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDiffusionCoefficient;
		private System.Windows.Forms.CheckBox checkBoxRun;
		private System.Windows.Forms.Button buttonReset;
		private System.Windows.Forms.Button buttonReload;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDeltaTime;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button buttonResetObstacles;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlStartPosition;
		private System.Windows.Forms.TextBox textBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button buttonResetSimulation;
		private System.Windows.Forms.Button buttonStepSimulation;
		private System.Windows.Forms.Button buttonRunSimulation;
		private System.Windows.Forms.CheckBox checkBoxShowSearch;
		private System.Windows.Forms.RadioButton radioButtonAlgo0;
		private System.Windows.Forms.RadioButton radioButtonAlgo1;
		private System.Windows.Forms.CheckBox checkBoxShowLog;
	}
}