namespace TestGraphViz
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
			this.buttonReload = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.buttonResetObstacles = new System.Windows.Forms.Button();
			this.textBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.buttonResetSimulation = new System.Windows.Forms.Button();
			this.buttonStepSimulation = new System.Windows.Forms.Button();
			this.buttonRunSimulation = new System.Windows.Forms.Button();
			this.checkBoxShowSearch = new System.Windows.Forms.CheckBox();
			this.radioButtonSearchAlgo0 = new System.Windows.Forms.RadioButton();
			this.radioButtonSearchAlgo1 = new System.Windows.Forms.RadioButton();
			this.panel3 = new System.Windows.Forms.Panel();
			this.floatTrackbarControlResultsSpaceConfinement = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.radioButtonShowResultsSpace = new System.Windows.Forms.RadioButton();
			this.checkBoxShowLog = new System.Windows.Forms.CheckBox();
			this.radioButtonShowNormalizedSpace = new System.Windows.Forms.RadioButton();
			this.radioButtonShowHeat = new System.Windows.Forms.RadioButton();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.buttonLoad = new System.Windows.Forms.Button();
			this.buttonSave = new System.Windows.Forms.Button();
			this.panelOutput = new TestGraphViz.PanelOutput(this.components);
			this.integerTrackbarControlStartPosition = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.integerTrackbarControlTargetPosition = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.floatTrackbarControlDiffusionCoefficient = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.groupBoxSearch = new System.Windows.Forms.GroupBox();
			this.label5 = new System.Windows.Forms.Label();
			this.integerTrackbarControlSimulationSourceIndex = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonResetAll = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.integerTrackbarControlIterationsCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.checkBoxAutoSimulate = new System.Windows.Forms.CheckBox();
			this.panel3.SuspendLayout();
			this.groupBoxSearch.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkBoxRun
			// 
			this.checkBoxRun.AutoSize = true;
			this.checkBoxRun.Location = new System.Drawing.Point(530, 69);
			this.checkBoxRun.Name = "checkBoxRun";
			this.checkBoxRun.Size = new System.Drawing.Size(46, 17);
			this.checkBoxRun.TabIndex = 2;
			this.checkBoxRun.Text = "Run";
			this.checkBoxRun.UseVisualStyleBackColor = true;
			this.checkBoxRun.CheckedChanged += new System.EventHandler(this.checkBoxRun_CheckedChanged);
			// 
			// buttonReset
			// 
			this.buttonReset.Location = new System.Drawing.Point(582, 65);
			this.buttonReset.Name = "buttonReset";
			this.buttonReset.Size = new System.Drawing.Size(46, 23);
			this.buttonReset.TabIndex = 3;
			this.buttonReset.Text = "Reset";
			this.buttonReset.UseVisualStyleBackColor = true;
			this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
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
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(530, 17);
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
			this.buttonResetObstacles.Location = new System.Drawing.Point(741, 65);
			this.buttonResetObstacles.Name = "buttonResetObstacles";
			this.buttonResetObstacles.Size = new System.Drawing.Size(98, 23);
			this.buttonResetObstacles.TabIndex = 3;
			this.buttonResetObstacles.Text = "Reset Obstacles";
			this.buttonResetObstacles.UseVisualStyleBackColor = true;
			this.buttonResetObstacles.Click += new System.EventHandler(this.buttonResetObstacles_Click);
			// 
			// textBox
			// 
			this.textBox.Location = new System.Drawing.Point(9, 141);
			this.textBox.Multiline = true;
			this.textBox.Name = "textBox";
			this.textBox.ReadOnly = true;
			this.textBox.Size = new System.Drawing.Size(297, 120);
			this.textBox.TabIndex = 7;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 22);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(69, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Start Position";
			// 
			// buttonResetSimulation
			// 
			this.buttonResetSimulation.Location = new System.Drawing.Point(9, 68);
			this.buttonResetSimulation.Name = "buttonResetSimulation";
			this.buttonResetSimulation.Size = new System.Drawing.Size(75, 23);
			this.buttonResetSimulation.TabIndex = 8;
			this.buttonResetSimulation.Text = "Reset";
			this.buttonResetSimulation.UseVisualStyleBackColor = true;
			// 
			// buttonStepSimulation
			// 
			this.buttonStepSimulation.Location = new System.Drawing.Point(90, 68);
			this.buttonStepSimulation.Name = "buttonStepSimulation";
			this.buttonStepSimulation.Size = new System.Drawing.Size(75, 23);
			this.buttonStepSimulation.TabIndex = 8;
			this.buttonStepSimulation.Text = "Step";
			this.buttonStepSimulation.UseVisualStyleBackColor = true;
			// 
			// buttonRunSimulation
			// 
			this.buttonRunSimulation.Location = new System.Drawing.Point(171, 68);
			this.buttonRunSimulation.Name = "buttonRunSimulation";
			this.buttonRunSimulation.Size = new System.Drawing.Size(75, 23);
			this.buttonRunSimulation.TabIndex = 8;
			this.buttonRunSimulation.Text = "Run";
			this.buttonRunSimulation.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowSearch
			// 
			this.checkBoxShowSearch.AutoSize = true;
			this.checkBoxShowSearch.Checked = true;
			this.checkBoxShowSearch.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowSearch.Location = new System.Drawing.Point(9, 97);
			this.checkBoxShowSearch.Name = "checkBoxShowSearch";
			this.checkBoxShowSearch.Size = new System.Drawing.Size(123, 17);
			this.checkBoxShowSearch.TabIndex = 9;
			this.checkBoxShowSearch.Text = "Show Search Result";
			this.checkBoxShowSearch.UseVisualStyleBackColor = true;
			// 
			// radioButtonSearchAlgo0
			// 
			this.radioButtonSearchAlgo0.AutoSize = true;
			this.radioButtonSearchAlgo0.Checked = true;
			this.radioButtonSearchAlgo0.Location = new System.Drawing.Point(9, 120);
			this.radioButtonSearchAlgo0.Name = "radioButtonSearchAlgo0";
			this.radioButtonSearchAlgo0.Size = new System.Drawing.Size(75, 17);
			this.radioButtonSearchAlgo0.TabIndex = 10;
			this.radioButtonSearchAlgo0.TabStop = true;
			this.radioButtonSearchAlgo0.Text = "Algo Local";
			this.radioButtonSearchAlgo0.UseVisualStyleBackColor = true;
			// 
			// radioButtonSearchAlgo1
			// 
			this.radioButtonSearchAlgo1.AutoSize = true;
			this.radioButtonSearchAlgo1.Location = new System.Drawing.Point(94, 120);
			this.radioButtonSearchAlgo1.Name = "radioButtonSearchAlgo1";
			this.radioButtonSearchAlgo1.Size = new System.Drawing.Size(79, 17);
			this.radioButtonSearchAlgo1.TabIndex = 10;
			this.radioButtonSearchAlgo1.Text = "Algo Global";
			this.radioButtonSearchAlgo1.UseVisualStyleBackColor = true;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.floatTrackbarControlResultsSpaceConfinement);
			this.panel3.Controls.Add(this.radioButtonShowResultsSpace);
			this.panel3.Controls.Add(this.checkBoxShowLog);
			this.panel3.Controls.Add(this.radioButtonShowNormalizedSpace);
			this.panel3.Controls.Add(this.radioButtonShowHeat);
			this.panel3.Location = new System.Drawing.Point(533, 148);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(312, 43);
			this.panel3.TabIndex = 14;
			// 
			// floatTrackbarControlResultsSpaceConfinement
			// 
			this.floatTrackbarControlResultsSpaceConfinement.Location = new System.Drawing.Point(181, 20);
			this.floatTrackbarControlResultsSpaceConfinement.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlResultsSpaceConfinement.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlResultsSpaceConfinement.Name = "floatTrackbarControlResultsSpaceConfinement";
			this.floatTrackbarControlResultsSpaceConfinement.RangeMax = 1F;
			this.floatTrackbarControlResultsSpaceConfinement.RangeMin = 0F;
			this.floatTrackbarControlResultsSpaceConfinement.Size = new System.Drawing.Size(131, 20);
			this.floatTrackbarControlResultsSpaceConfinement.TabIndex = 2;
			this.floatTrackbarControlResultsSpaceConfinement.Value = 0.1F;
			this.floatTrackbarControlResultsSpaceConfinement.VisibleRangeMax = 1F;
			// 
			// radioButtonShowResultsSpace
			// 
			this.radioButtonShowResultsSpace.AutoSize = true;
			this.radioButtonShowResultsSpace.Location = new System.Drawing.Point(181, 0);
			this.radioButtonShowResultsSpace.Name = "radioButtonShowResultsSpace";
			this.radioButtonShowResultsSpace.Size = new System.Drawing.Size(94, 17);
			this.radioButtonShowResultsSpace.TabIndex = 0;
			this.radioButtonShowResultsSpace.Text = "Results Space";
			this.radioButtonShowResultsSpace.UseVisualStyleBackColor = true;
			this.radioButtonShowResultsSpace.CheckedChanged += new System.EventHandler(this.radioButtonShowResultsSpace_CheckedChanged);
			// 
			// checkBoxShowLog
			// 
			this.checkBoxShowLog.AutoSize = true;
			this.checkBoxShowLog.Checked = true;
			this.checkBoxShowLog.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowLog.Location = new System.Drawing.Point(3, 23);
			this.checkBoxShowLog.Name = "checkBoxShowLog";
			this.checkBoxShowLog.Size = new System.Drawing.Size(44, 17);
			this.checkBoxShowLog.TabIndex = 1;
			this.checkBoxShowLog.Text = "Log";
			this.checkBoxShowLog.UseVisualStyleBackColor = true;
			// 
			// radioButtonShowNormalizedSpace
			// 
			this.radioButtonShowNormalizedSpace.AutoSize = true;
			this.radioButtonShowNormalizedSpace.Location = new System.Drawing.Point(64, 0);
			this.radioButtonShowNormalizedSpace.Name = "radioButtonShowNormalizedSpace";
			this.radioButtonShowNormalizedSpace.Size = new System.Drawing.Size(111, 17);
			this.radioButtonShowNormalizedSpace.TabIndex = 0;
			this.radioButtonShowNormalizedSpace.Text = "Normalized Space";
			this.radioButtonShowNormalizedSpace.UseVisualStyleBackColor = true;
			this.radioButtonShowNormalizedSpace.CheckedChanged += new System.EventHandler(this.radioButtonShowNormalizedSpace_CheckedChanged);
			// 
			// radioButtonShowHeat
			// 
			this.radioButtonShowHeat.AutoSize = true;
			this.radioButtonShowHeat.Checked = true;
			this.radioButtonShowHeat.Location = new System.Drawing.Point(1, 0);
			this.radioButtonShowHeat.Name = "radioButtonShowHeat";
			this.radioButtonShowHeat.Size = new System.Drawing.Size(48, 17);
			this.radioButtonShowHeat.TabIndex = 0;
			this.radioButtonShowHeat.TabStop = true;
			this.radioButtonShowHeat.Text = "Heat";
			this.radioButtonShowHeat.UseVisualStyleBackColor = true;
			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.DefaultExt = "*.sim";
			this.saveFileDialog1.Filter = "Simulation Files (*.sim)|*.sim";
			this.saveFileDialog1.RestoreDirectory = true;
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.DefaultExt = "*.sim";
			this.openFileDialog1.FileName = "TestObstacles.obs";
			this.openFileDialog1.Filter = "Simulation Files (*.sim)|*.sim";
			this.openFileDialog1.RestoreDirectory = true;
			// 
			// buttonLoad
			// 
			this.buttonLoad.Location = new System.Drawing.Point(555, 508);
			this.buttonLoad.Name = "buttonLoad";
			this.buttonLoad.Size = new System.Drawing.Size(75, 23);
			this.buttonLoad.TabIndex = 15;
			this.buttonLoad.Text = "Load";
			this.buttonLoad.UseVisualStyleBackColor = true;
			this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
			// 
			// buttonSave
			// 
			this.buttonSave.Location = new System.Drawing.Point(639, 508);
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(75, 23);
			this.buttonSave.TabIndex = 15;
			this.buttonSave.Text = "Save";
			this.buttonSave.UseVisualStyleBackColor = true;
			this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(512, 512);
			this.panelOutput.TabIndex = 0;
			this.panelOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseDown);
			// 
			// integerTrackbarControlStartPosition
			// 
			this.integerTrackbarControlStartPosition.Location = new System.Drawing.Point(90, 18);
			this.integerTrackbarControlStartPosition.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlStartPosition.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlStartPosition.Name = "integerTrackbarControlStartPosition";
			this.integerTrackbarControlStartPosition.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlStartPosition.TabIndex = 16;
			this.integerTrackbarControlStartPosition.Value = 0;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 46);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(78, 13);
			this.label4.TabIndex = 5;
			this.label4.Text = "Target Position";
			// 
			// integerTrackbarControlTargetPosition
			// 
			this.integerTrackbarControlTargetPosition.Location = new System.Drawing.Point(90, 42);
			this.integerTrackbarControlTargetPosition.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlTargetPosition.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlTargetPosition.Name = "integerTrackbarControlTargetPosition";
			this.integerTrackbarControlTargetPosition.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlTargetPosition.TabIndex = 16;
			this.integerTrackbarControlTargetPosition.Value = 0;
			// 
			// floatTrackbarControlDiffusionCoefficient
			// 
			this.floatTrackbarControlDiffusionCoefficient.Location = new System.Drawing.Point(639, 13);
			this.floatTrackbarControlDiffusionCoefficient.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDiffusionCoefficient.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDiffusionCoefficient.Name = "floatTrackbarControlDiffusionCoefficient";
			this.floatTrackbarControlDiffusionCoefficient.RangeMax = 1000F;
			this.floatTrackbarControlDiffusionCoefficient.RangeMin = 0F;
			this.floatTrackbarControlDiffusionCoefficient.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDiffusionCoefficient.TabIndex = 17;
			this.floatTrackbarControlDiffusionCoefficient.Value = 1F;
			this.floatTrackbarControlDiffusionCoefficient.VisibleRangeMax = 1F;
			// 
			// groupBoxSearch
			// 
			this.groupBoxSearch.Controls.Add(this.label3);
			this.groupBoxSearch.Controls.Add(this.label4);
			this.groupBoxSearch.Controls.Add(this.buttonResetSimulation);
			this.groupBoxSearch.Controls.Add(this.integerTrackbarControlTargetPosition);
			this.groupBoxSearch.Controls.Add(this.buttonStepSimulation);
			this.groupBoxSearch.Controls.Add(this.integerTrackbarControlStartPosition);
			this.groupBoxSearch.Controls.Add(this.buttonRunSimulation);
			this.groupBoxSearch.Controls.Add(this.checkBoxShowSearch);
			this.groupBoxSearch.Controls.Add(this.textBox);
			this.groupBoxSearch.Controls.Add(this.radioButtonSearchAlgo0);
			this.groupBoxSearch.Controls.Add(this.radioButtonSearchAlgo1);
			this.groupBoxSearch.Enabled = false;
			this.groupBoxSearch.Location = new System.Drawing.Point(533, 235);
			this.groupBoxSearch.Name = "groupBoxSearch";
			this.groupBoxSearch.Size = new System.Drawing.Size(312, 267);
			this.groupBoxSearch.TabIndex = 18;
			this.groupBoxSearch.TabStop = false;
			this.groupBoxSearch.Text = "Search";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(530, 206);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(312, 29);
			this.label5.TabIndex = 19;
			this.label5.Text = "*NOTE* Search gets enabled once you have more than a single constant heat source " +
    "(middle mouse button)";
			// 
			// integerTrackbarControlSimulationSourceIndex
			// 
			this.integerTrackbarControlSimulationSourceIndex.Enabled = false;
			this.integerTrackbarControlSimulationSourceIndex.Location = new System.Drawing.Point(639, 39);
			this.integerTrackbarControlSimulationSourceIndex.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlSimulationSourceIndex.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlSimulationSourceIndex.Name = "integerTrackbarControlSimulationSourceIndex";
			this.integerTrackbarControlSimulationSourceIndex.RangeMax = 100;
			this.integerTrackbarControlSimulationSourceIndex.RangeMin = 0;
			this.integerTrackbarControlSimulationSourceIndex.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlSimulationSourceIndex.TabIndex = 20;
			this.integerTrackbarControlSimulationSourceIndex.Value = 0;
			this.integerTrackbarControlSimulationSourceIndex.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlSimulationSourceIndex_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(530, 42);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(70, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "Source Index";
			// 
			// buttonResetAll
			// 
			this.buttonResetAll.Location = new System.Drawing.Point(634, 65);
			this.buttonResetAll.Name = "buttonResetAll";
			this.buttonResetAll.Size = new System.Drawing.Size(75, 23);
			this.buttonResetAll.TabIndex = 21;
			this.buttonResetAll.Text = "Reset All";
			this.buttonResetAll.UseVisualStyleBackColor = true;
			this.buttonResetAll.Click += new System.EventHandler(this.buttonResetAll_Click);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(789, 109);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(50, 13);
			this.label6.TabIndex = 5;
			this.label6.Text = "Iterations";
			// 
			// integerTrackbarControlIterationsCount
			// 
			this.integerTrackbarControlIterationsCount.Location = new System.Drawing.Point(634, 105);
			this.integerTrackbarControlIterationsCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlIterationsCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlIterationsCount.Name = "integerTrackbarControlIterationsCount";
			this.integerTrackbarControlIterationsCount.RangeMax = 100000;
			this.integerTrackbarControlIterationsCount.RangeMin = 0;
			this.integerTrackbarControlIterationsCount.Size = new System.Drawing.Size(149, 20);
			this.integerTrackbarControlIterationsCount.TabIndex = 23;
			this.integerTrackbarControlIterationsCount.Value = 200;
			this.integerTrackbarControlIterationsCount.VisibleRangeMax = 1000;
			// 
			// checkBoxAutoSimulate
			// 
			this.checkBoxAutoSimulate.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBoxAutoSimulate.AutoSize = true;
			this.checkBoxAutoSimulate.Enabled = false;
			this.checkBoxAutoSimulate.Location = new System.Drawing.Point(533, 104);
			this.checkBoxAutoSimulate.Name = "checkBoxAutoSimulate";
			this.checkBoxAutoSimulate.Size = new System.Drawing.Size(82, 23);
			this.checkBoxAutoSimulate.TabIndex = 24;
			this.checkBoxAutoSimulate.Text = "Auto-Simulate";
			this.checkBoxAutoSimulate.UseVisualStyleBackColor = true;
			this.checkBoxAutoSimulate.CheckedChanged += new System.EventHandler(this.checkBoxAutoSimulate_CheckedChanged);
			// 
			// GraphForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(857, 543);
			this.Controls.Add(this.checkBoxAutoSimulate);
			this.Controls.Add(this.buttonResetAll);
			this.Controls.Add(this.integerTrackbarControlSimulationSourceIndex);
			this.Controls.Add(this.groupBoxSearch);
			this.Controls.Add(this.floatTrackbarControlDiffusionCoefficient);
			this.Controls.Add(this.buttonSave);
			this.Controls.Add(this.buttonLoad);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.buttonResetObstacles);
			this.Controls.Add(this.buttonReset);
			this.Controls.Add(this.checkBoxRun);
			this.Controls.Add(this.panelOutput);
			this.Controls.Add(this.label5);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "GraphForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Heat Wave Test";
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.groupBoxSearch.ResumeLayout(false);
			this.groupBoxSearch.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelOutput;
		private System.Windows.Forms.CheckBox checkBoxRun;
		private System.Windows.Forms.Button buttonReset;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button buttonResetObstacles;
		private System.Windows.Forms.TextBox textBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button buttonResetSimulation;
		private System.Windows.Forms.Button buttonStepSimulation;
		private System.Windows.Forms.Button buttonRunSimulation;
		private System.Windows.Forms.CheckBox checkBoxShowSearch;
		private System.Windows.Forms.RadioButton radioButtonSearchAlgo0;
		private System.Windows.Forms.RadioButton radioButtonSearchAlgo1;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.RadioButton radioButtonShowNormalizedSpace;
		private System.Windows.Forms.RadioButton radioButtonShowHeat;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button buttonLoad;
		private System.Windows.Forms.Button buttonSave;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlStartPosition;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlTargetPosition;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDiffusionCoefficient;
		private System.Windows.Forms.GroupBox groupBoxSearch;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.CheckBox checkBoxShowLog;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlSimulationSourceIndex;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonResetAll;
		private System.Windows.Forms.Label label6;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlIterationsCount;
		private System.Windows.Forms.CheckBox checkBoxAutoSimulate;
		private System.Windows.Forms.RadioButton radioButtonShowResultsSpace;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlResultsSpaceConfinement;
	}
}