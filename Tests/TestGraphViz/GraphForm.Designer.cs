﻿namespace TestGraphViz
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
			this.floatTrackbarControlDeltaTime = new UIUtility.FloatTrackbarControl();
			this.buttonResetAll = new System.Windows.Forms.Button();
			this.integerTrackbarControlIterationsCount = new UIUtility.IntegerTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlSpringConstant = new UIUtility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlDampingConstant = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlK0 = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlK1 = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlK2 = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlK3 = new UIUtility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.checkBoxAutoCenter = new System.Windows.Forms.CheckBox();
			this.label8 = new System.Windows.Forms.Label();
			this.floatTrackbarControlRestDistance = new UIUtility.FloatTrackbarControl();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkBoxShowChildren = new System.Windows.Forms.CheckBox();
			this.labelSearchResults = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.textBoxSearch = new System.Windows.Forms.TextBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkBoxRun
			// 
			this.checkBoxRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxRun.AutoSize = true;
			this.checkBoxRun.Location = new System.Drawing.Point(791, 330);
			this.checkBoxRun.Name = "checkBoxRun";
			this.checkBoxRun.Size = new System.Drawing.Size(46, 17);
			this.checkBoxRun.TabIndex = 2;
			this.checkBoxRun.Text = "Run";
			this.checkBoxRun.UseVisualStyleBackColor = true;
			this.checkBoxRun.CheckedChanged += new System.EventHandler(this.checkBoxRun_CheckedChanged);
			// 
			// buttonReset
			// 
			this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReset.Location = new System.Drawing.Point(843, 326);
			this.buttonReset.Name = "buttonReset";
			this.buttonReset.Size = new System.Drawing.Size(46, 23);
			this.buttonReset.TabIndex = 3;
			this.buttonReset.Text = "Reset";
			this.buttonReset.UseVisualStyleBackColor = true;
			this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(1031, 757);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 4;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(791, 17);
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
			this.buttonResetObstacles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonResetObstacles.Location = new System.Drawing.Point(1002, 326);
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
			this.saveFileDialog1.Filter = "Graph Simulation Files (*.graphpos)|*.graphpos";
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
			this.buttonLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLoad.Location = new System.Drawing.Point(816, 757);
			this.buttonLoad.Name = "buttonLoad";
			this.buttonLoad.Size = new System.Drawing.Size(75, 23);
			this.buttonLoad.TabIndex = 15;
			this.buttonLoad.Text = "Load";
			this.buttonLoad.UseVisualStyleBackColor = true;
			// 
			// buttonSave
			// 
			this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSave.Location = new System.Drawing.Point(900, 757);
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
			this.panelOutput.Size = new System.Drawing.Size(768, 768);
			this.panelOutput.TabIndex = 0;
			this.panelOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseDown);
			this.panelOutput.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseMove);
			// 
			// floatTrackbarControlDeltaTime
			// 
			this.floatTrackbarControlDeltaTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlDeltaTime.Location = new System.Drawing.Point(900, 13);
			this.floatTrackbarControlDeltaTime.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDeltaTime.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDeltaTime.Name = "floatTrackbarControlDeltaTime";
			this.floatTrackbarControlDeltaTime.RangeMax = 1000F;
			this.floatTrackbarControlDeltaTime.RangeMin = 0F;
			this.floatTrackbarControlDeltaTime.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDeltaTime.TabIndex = 17;
			this.floatTrackbarControlDeltaTime.Value = 0.01F;
			this.floatTrackbarControlDeltaTime.VisibleRangeMax = 0.01F;
			// 
			// buttonResetAll
			// 
			this.buttonResetAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonResetAll.Location = new System.Drawing.Point(895, 326);
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
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(791, 43);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(82, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "Spring Constant";
			// 
			// floatTrackbarControlSpringConstant
			// 
			this.floatTrackbarControlSpringConstant.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlSpringConstant.Location = new System.Drawing.Point(900, 39);
			this.floatTrackbarControlSpringConstant.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSpringConstant.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSpringConstant.Name = "floatTrackbarControlSpringConstant";
			this.floatTrackbarControlSpringConstant.RangeMax = 1000000F;
			this.floatTrackbarControlSpringConstant.RangeMin = -1000000F;
			this.floatTrackbarControlSpringConstant.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlSpringConstant.TabIndex = 17;
			this.floatTrackbarControlSpringConstant.Value = 10000F;
			this.floatTrackbarControlSpringConstant.VisibleRangeMax = 10000F;
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(791, 69);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(94, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Damping Constant";
			// 
			// floatTrackbarControlDampingConstant
			// 
			this.floatTrackbarControlDampingConstant.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlDampingConstant.Location = new System.Drawing.Point(900, 65);
			this.floatTrackbarControlDampingConstant.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDampingConstant.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDampingConstant.Name = "floatTrackbarControlDampingConstant";
			this.floatTrackbarControlDampingConstant.RangeMax = 1000000F;
			this.floatTrackbarControlDampingConstant.RangeMin = -1000000F;
			this.floatTrackbarControlDampingConstant.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDampingConstant.TabIndex = 17;
			this.floatTrackbarControlDampingConstant.Value = -1000F;
			this.floatTrackbarControlDampingConstant.VisibleRangeMax = 1F;
			this.floatTrackbarControlDampingConstant.VisibleRangeMin = -2000F;
			// 
			// floatTrackbarControlK0
			// 
			this.floatTrackbarControlK0.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlK0.Location = new System.Drawing.Point(900, 145);
			this.floatTrackbarControlK0.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlK0.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlK0.Name = "floatTrackbarControlK0";
			this.floatTrackbarControlK0.RangeMax = 1000000F;
			this.floatTrackbarControlK0.RangeMin = -1000000F;
			this.floatTrackbarControlK0.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlK0.TabIndex = 17;
			this.floatTrackbarControlK0.Value = 1F;
			this.floatTrackbarControlK0.VisibleRangeMax = 1F;
			// 
			// floatTrackbarControlK1
			// 
			this.floatTrackbarControlK1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlK1.Location = new System.Drawing.Point(900, 171);
			this.floatTrackbarControlK1.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlK1.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlK1.Name = "floatTrackbarControlK1";
			this.floatTrackbarControlK1.RangeMax = 1000000F;
			this.floatTrackbarControlK1.RangeMin = -1000000F;
			this.floatTrackbarControlK1.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlK1.TabIndex = 17;
			this.floatTrackbarControlK1.Value = 0.1F;
			this.floatTrackbarControlK1.VisibleRangeMax = 0.5F;
			// 
			// floatTrackbarControlK2
			// 
			this.floatTrackbarControlK2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlK2.Location = new System.Drawing.Point(900, 218);
			this.floatTrackbarControlK2.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlK2.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlK2.Name = "floatTrackbarControlK2";
			this.floatTrackbarControlK2.RangeMax = 1000000F;
			this.floatTrackbarControlK2.RangeMin = -1000000F;
			this.floatTrackbarControlK2.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlK2.TabIndex = 17;
			this.floatTrackbarControlK2.Value = 1.5F;
			this.floatTrackbarControlK2.VisibleRangeMax = 2F;
			// 
			// floatTrackbarControlK3
			// 
			this.floatTrackbarControlK3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlK3.Location = new System.Drawing.Point(900, 244);
			this.floatTrackbarControlK3.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlK3.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlK3.Name = "floatTrackbarControlK3";
			this.floatTrackbarControlK3.RangeMax = 1000000F;
			this.floatTrackbarControlK3.RangeMin = -1000000F;
			this.floatTrackbarControlK3.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlK3.TabIndex = 17;
			this.floatTrackbarControlK3.Value = 0.08F;
			this.floatTrackbarControlK3.VisibleRangeMax = 0.1F;
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(871, 145);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(20, 13);
			this.label4.TabIndex = 5;
			this.label4.Text = "K0";
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(871, 171);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(20, 13);
			this.label5.TabIndex = 5;
			this.label5.Text = "K1";
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(869, 222);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(20, 13);
			this.label6.TabIndex = 5;
			this.label6.Text = "K2";
			// 
			// label7
			// 
			this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(869, 248);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(20, 13);
			this.label7.TabIndex = 5;
			this.label7.Text = "K3";
			// 
			// checkBoxAutoCenter
			// 
			this.checkBoxAutoCenter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxAutoCenter.AutoSize = true;
			this.checkBoxAutoCenter.Checked = true;
			this.checkBoxAutoCenter.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAutoCenter.Location = new System.Drawing.Point(791, 293);
			this.checkBoxAutoCenter.Name = "checkBoxAutoCenter";
			this.checkBoxAutoCenter.Size = new System.Drawing.Size(121, 17);
			this.checkBoxAutoCenter.TabIndex = 22;
			this.checkBoxAutoCenter.Text = "Auto-Center Camera";
			this.checkBoxAutoCenter.UseVisualStyleBackColor = true;
			// 
			// label8
			// 
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(791, 95);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(74, 13);
			this.label8.TabIndex = 5;
			this.label8.Text = "Rest Distance";
			// 
			// floatTrackbarControlRestDistance
			// 
			this.floatTrackbarControlRestDistance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlRestDistance.Location = new System.Drawing.Point(900, 91);
			this.floatTrackbarControlRestDistance.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlRestDistance.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlRestDistance.Name = "floatTrackbarControlRestDistance";
			this.floatTrackbarControlRestDistance.RangeMax = 1000000F;
			this.floatTrackbarControlRestDistance.RangeMin = -1000000F;
			this.floatTrackbarControlRestDistance.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlRestDistance.TabIndex = 17;
			this.floatTrackbarControlRestDistance.Value = 4F;
			this.floatTrackbarControlRestDistance.VisibleRangeMax = 4F;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.checkBoxShowChildren);
			this.groupBox1.Controls.Add(this.labelSearchResults);
			this.groupBox1.Controls.Add(this.label9);
			this.groupBox1.Controls.Add(this.textBoxSearch);
			this.groupBox1.Location = new System.Drawing.Point(791, 375);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(315, 211);
			this.groupBox1.TabIndex = 23;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Search Box";
			// 
			// checkBoxShowChildren
			// 
			this.checkBoxShowChildren.AutoSize = true;
			this.checkBoxShowChildren.Location = new System.Drawing.Point(14, 45);
			this.checkBoxShowChildren.Name = "checkBoxShowChildren";
			this.checkBoxShowChildren.Size = new System.Drawing.Size(94, 17);
			this.checkBoxShowChildren.TabIndex = 3;
			this.checkBoxShowChildren.Text = "Show Children";
			this.checkBoxShowChildren.UseVisualStyleBackColor = true;
			this.checkBoxShowChildren.CheckedChanged += new System.EventHandler(this.checkBoxShowChildren_CheckedChanged);
			// 
			// labelSearchResults
			// 
			this.labelSearchResults.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelSearchResults.Location = new System.Drawing.Point(14, 65);
			this.labelSearchResults.Name = "labelSearchResults";
			this.labelSearchResults.Size = new System.Drawing.Size(295, 143);
			this.labelSearchResults.TabIndex = 2;
			this.labelSearchResults.Text = "No result.";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(11, 22);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(62, 13);
			this.label9.TabIndex = 1;
			this.label9.Text = "Search for :";
			// 
			// textBoxSearch
			// 
			this.textBoxSearch.Location = new System.Drawing.Point(79, 19);
			this.textBoxSearch.Name = "textBoxSearch";
			this.textBoxSearch.Size = new System.Drawing.Size(230, 20);
			this.textBoxSearch.TabIndex = 0;
			this.textBoxSearch.TextChanged += new System.EventHandler(this.textBoxSearch_TextChanged);
			// 
			// GraphForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1118, 793);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.checkBoxAutoCenter);
			this.Controls.Add(this.buttonResetAll);
			this.Controls.Add(this.floatTrackbarControlK3);
			this.Controls.Add(this.floatTrackbarControlK2);
			this.Controls.Add(this.floatTrackbarControlK1);
			this.Controls.Add(this.floatTrackbarControlK0);
			this.Controls.Add(this.floatTrackbarControlRestDistance);
			this.Controls.Add(this.floatTrackbarControlDampingConstant);
			this.Controls.Add(this.floatTrackbarControlSpringConstant);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.floatTrackbarControlDeltaTime);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.buttonSave);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonLoad);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.buttonResetObstacles);
			this.Controls.Add(this.buttonReset);
			this.Controls.Add(this.checkBoxRun);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "GraphForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Graph Viz Test";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
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
		private UIUtility.FloatTrackbarControl floatTrackbarControlDeltaTime;
		private System.Windows.Forms.Button buttonResetAll;
		private UIUtility.IntegerTrackbarControl integerTrackbarControlIterationsCount;
		private System.Windows.Forms.Label label1;
		private UIUtility.FloatTrackbarControl floatTrackbarControlSpringConstant;
		private System.Windows.Forms.Label label3;
		private UIUtility.FloatTrackbarControl floatTrackbarControlDampingConstant;
		private UIUtility.FloatTrackbarControl floatTrackbarControlK0;
		private UIUtility.FloatTrackbarControl floatTrackbarControlK1;
		private UIUtility.FloatTrackbarControl floatTrackbarControlK2;
		private UIUtility.FloatTrackbarControl floatTrackbarControlK3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox checkBoxAutoCenter;
		private System.Windows.Forms.Label label8;
		private UIUtility.FloatTrackbarControl floatTrackbarControlRestDistance;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox textBoxSearch;
		private System.Windows.Forms.Label labelSearchResults;
		private System.Windows.Forms.CheckBox checkBoxShowChildren;
	}
}