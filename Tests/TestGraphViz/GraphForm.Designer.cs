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
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.buttonLoad = new System.Windows.Forms.Button();
			this.buttonSave = new System.Windows.Forms.Button();
			this.panelOutput = new TestGraphViz.PanelOutput(this.components);
			this.floatTrackbarControlDeltaTime = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.buttonResetAll = new System.Windows.Forms.Button();
			this.integerTrackbarControlIterationsCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlSpringConstant = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlDampingConstant = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.SuspendLayout();
			// 
			// checkBoxRun
			// 
			this.checkBoxRun.AutoSize = true;
			this.checkBoxRun.Location = new System.Drawing.Point(530, 177);
			this.checkBoxRun.Name = "checkBoxRun";
			this.checkBoxRun.Size = new System.Drawing.Size(46, 17);
			this.checkBoxRun.TabIndex = 2;
			this.checkBoxRun.Text = "Run";
			this.checkBoxRun.UseVisualStyleBackColor = true;
			this.checkBoxRun.CheckedChanged += new System.EventHandler(this.checkBoxRun_CheckedChanged);
			// 
			// buttonReset
			// 
			this.buttonReset.Location = new System.Drawing.Point(582, 173);
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
			this.label2.Size = new System.Drawing.Size(58, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Delta-Time";
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 10;
			// 
			// buttonResetObstacles
			// 
			this.buttonResetObstacles.Location = new System.Drawing.Point(741, 173);
			this.buttonResetObstacles.Name = "buttonResetObstacles";
			this.buttonResetObstacles.Size = new System.Drawing.Size(98, 23);
			this.buttonResetObstacles.TabIndex = 3;
			this.buttonResetObstacles.Text = "Reset Obstacles";
			this.buttonResetObstacles.UseVisualStyleBackColor = true;
			this.buttonResetObstacles.Click += new System.EventHandler(this.buttonResetObstacles_Click);
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
			// 
			// buttonSave
			// 
			this.buttonSave.Location = new System.Drawing.Point(639, 508);
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(75, 23);
			this.buttonSave.TabIndex = 15;
			this.buttonSave.Text = "Save";
			this.buttonSave.UseVisualStyleBackColor = true;
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(512, 512);
			this.panelOutput.TabIndex = 0;
			this.panelOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseDown);
			// 
			// floatTrackbarControlDeltaTime
			// 
			this.floatTrackbarControlDeltaTime.Location = new System.Drawing.Point(639, 13);
			this.floatTrackbarControlDeltaTime.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDeltaTime.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDeltaTime.Name = "floatTrackbarControlDeltaTime";
			this.floatTrackbarControlDeltaTime.RangeMax = 1000F;
			this.floatTrackbarControlDeltaTime.RangeMin = 0F;
			this.floatTrackbarControlDeltaTime.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDeltaTime.TabIndex = 17;
			this.floatTrackbarControlDeltaTime.Value = 0.01F;
			this.floatTrackbarControlDeltaTime.VisibleRangeMax = 0.005F;
			// 
			// buttonResetAll
			// 
			this.buttonResetAll.Location = new System.Drawing.Point(634, 173);
			this.buttonResetAll.Name = "buttonResetAll";
			this.buttonResetAll.Size = new System.Drawing.Size(75, 23);
			this.buttonResetAll.TabIndex = 21;
			this.buttonResetAll.Text = "Reset All";
			this.buttonResetAll.UseVisualStyleBackColor = true;
			this.buttonResetAll.Click += new System.EventHandler(this.buttonResetAll_Click);
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
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(530, 43);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(82, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "Spring Constant";
			// 
			// floatTrackbarControlSpringConstant
			// 
			this.floatTrackbarControlSpringConstant.Location = new System.Drawing.Point(639, 39);
			this.floatTrackbarControlSpringConstant.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSpringConstant.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSpringConstant.Name = "floatTrackbarControlSpringConstant";
			this.floatTrackbarControlSpringConstant.RangeMax = 1000000F;
			this.floatTrackbarControlSpringConstant.RangeMin = -1000000F;
			this.floatTrackbarControlSpringConstant.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlSpringConstant.TabIndex = 17;
			this.floatTrackbarControlSpringConstant.Value = 1000F;
			this.floatTrackbarControlSpringConstant.VisibleRangeMax = 1F;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(530, 69);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(94, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Damping Constant";
			// 
			// floatTrackbarControlDampingConstant
			// 
			this.floatTrackbarControlDampingConstant.Location = new System.Drawing.Point(639, 65);
			this.floatTrackbarControlDampingConstant.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDampingConstant.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDampingConstant.Name = "floatTrackbarControlDampingConstant";
			this.floatTrackbarControlDampingConstant.RangeMax = 1000000F;
			this.floatTrackbarControlDampingConstant.RangeMin = -1000000F;
			this.floatTrackbarControlDampingConstant.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDampingConstant.TabIndex = 17;
			this.floatTrackbarControlDampingConstant.Value = -10F;
			this.floatTrackbarControlDampingConstant.VisibleRangeMax = 1F;
			// 
			// GraphForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(857, 543);
			this.Controls.Add(this.buttonResetAll);
			this.Controls.Add(this.floatTrackbarControlDampingConstant);
			this.Controls.Add(this.floatTrackbarControlSpringConstant);
			this.Controls.Add(this.floatTrackbarControlDeltaTime);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.buttonSave);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonLoad);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.buttonResetObstacles);
			this.Controls.Add(this.buttonReset);
			this.Controls.Add(this.checkBoxRun);
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
		private System.Windows.Forms.CheckBox checkBoxRun;
		private System.Windows.Forms.Button buttonReset;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button buttonResetObstacles;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button buttonLoad;
		private System.Windows.Forms.Button buttonSave;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDeltaTime;
		private System.Windows.Forms.Button buttonResetAll;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlIterationsCount;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSpringConstant;
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDampingConstant;
	}
}