namespace TestGraphQuery
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
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.buttonLoad = new System.Windows.Forms.Button();
			this.buttonSave = new System.Windows.Forms.Button();
			this.panelOutput = new TestGraphQuery.PanelOutput(this.components);
			this.floatTrackbarControlDiffusionConstant = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.buttonGrabResults = new System.Windows.Forms.Button();
			this.floatTrackbarControlResultsTolerance = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.integerTrackbarControlShowQuerySourceIndex = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.radioButtonShowResults = new System.Windows.Forms.RadioButton();
			this.radioButtonShowBarycentrics = new System.Windows.Forms.RadioButton();
			this.radioButtonShowTemperature = new System.Windows.Forms.RadioButton();
			this.labelSearchResults = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.textBoxSearch = new System.Windows.Forms.TextBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.labelProcessedQuery = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.integerTrackbarControlSignificantResultsCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkBoxRun
			// 
			this.checkBoxRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxRun.AutoSize = true;
			this.checkBoxRun.Location = new System.Drawing.Point(6, 19);
			this.checkBoxRun.Name = "checkBoxRun";
			this.checkBoxRun.Size = new System.Drawing.Size(46, 17);
			this.checkBoxRun.TabIndex = 2;
			this.checkBoxRun.Text = "Run";
			this.checkBoxRun.UseVisualStyleBackColor = true;
			// 
			// buttonReset
			// 
			this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReset.Location = new System.Drawing.Point(58, 15);
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
			this.label2.Size = new System.Drawing.Size(93, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Diffusion Constant";
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 10;
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
			// floatTrackbarControlDiffusionConstant
			// 
			this.floatTrackbarControlDiffusionConstant.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlDiffusionConstant.Location = new System.Drawing.Point(900, 13);
			this.floatTrackbarControlDiffusionConstant.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDiffusionConstant.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDiffusionConstant.Name = "floatTrackbarControlDiffusionConstant";
			this.floatTrackbarControlDiffusionConstant.RangeMax = 1000F;
			this.floatTrackbarControlDiffusionConstant.RangeMin = 0F;
			this.floatTrackbarControlDiffusionConstant.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDiffusionConstant.TabIndex = 17;
			this.floatTrackbarControlDiffusionConstant.Value = 1F;
			this.floatTrackbarControlDiffusionConstant.VisibleRangeMax = 1F;
			// 
			// buttonGrabResults
			// 
			this.buttonGrabResults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonGrabResults.Location = new System.Drawing.Point(6, 19);
			this.buttonGrabResults.Name = "buttonGrabResults";
			this.buttonGrabResults.Size = new System.Drawing.Size(80, 23);
			this.buttonGrabResults.TabIndex = 21;
			this.buttonGrabResults.Text = "Grab Results";
			this.buttonGrabResults.UseVisualStyleBackColor = true;
			this.buttonGrabResults.Click += new System.EventHandler(this.buttonGrabResults_Click);
			// 
			// floatTrackbarControlResultsTolerance
			// 
			this.floatTrackbarControlResultsTolerance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlResultsTolerance.Location = new System.Drawing.Point(199, 92);
			this.floatTrackbarControlResultsTolerance.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlResultsTolerance.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlResultsTolerance.Name = "floatTrackbarControlResultsTolerance";
			this.floatTrackbarControlResultsTolerance.RangeMax = 1000000F;
			this.floatTrackbarControlResultsTolerance.RangeMin = 0F;
			this.floatTrackbarControlResultsTolerance.Size = new System.Drawing.Size(116, 20);
			this.floatTrackbarControlResultsTolerance.TabIndex = 17;
			this.floatTrackbarControlResultsTolerance.Value = 0.2F;
			this.floatTrackbarControlResultsTolerance.VisibleRangeMax = 1F;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.integerTrackbarControlShowQuerySourceIndex);
			this.groupBox1.Controls.Add(this.radioButtonShowResults);
			this.groupBox1.Controls.Add(this.radioButtonShowBarycentrics);
			this.groupBox1.Controls.Add(this.radioButtonShowTemperature);
			this.groupBox1.Controls.Add(this.checkBoxRun);
			this.groupBox1.Controls.Add(this.buttonReset);
			this.groupBox1.Controls.Add(this.floatTrackbarControlResultsTolerance);
			this.groupBox1.Location = new System.Drawing.Point(794, 315);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(315, 127);
			this.groupBox1.TabIndex = 23;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Simulation";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 71);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(34, 13);
			this.label3.TabIndex = 19;
			this.label3.Text = "Show";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 46);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(98, 13);
			this.label1.TabIndex = 19;
			this.label1.Text = "Show query source";
			// 
			// integerTrackbarControlShowQuerySourceIndex
			// 
			this.integerTrackbarControlShowQuerySourceIndex.Location = new System.Drawing.Point(107, 43);
			this.integerTrackbarControlShowQuerySourceIndex.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlShowQuerySourceIndex.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlShowQuerySourceIndex.Name = "integerTrackbarControlShowQuerySourceIndex";
			this.integerTrackbarControlShowQuerySourceIndex.RangeMin = 0;
			this.integerTrackbarControlShowQuerySourceIndex.Enabled = false;
			this.integerTrackbarControlShowQuerySourceIndex.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlShowQuerySourceIndex.TabIndex = 22;
			this.integerTrackbarControlShowQuerySourceIndex.Value = 0;
			this.integerTrackbarControlShowQuerySourceIndex.VisibleRangeMax = 1;
			// 
			// radioButtonShowResults
			// 
			this.radioButtonShowResults.AutoSize = true;
			this.radioButtonShowResults.Location = new System.Drawing.Point(199, 69);
			this.radioButtonShowResults.Name = "radioButtonShowResults";
			this.radioButtonShowResults.Size = new System.Drawing.Size(60, 17);
			this.radioButtonShowResults.TabIndex = 18;
			this.radioButtonShowResults.Text = "Results";
			this.radioButtonShowResults.UseVisualStyleBackColor = true;
			// 
			// radioButtonShowBarycentrics
			// 
			this.radioButtonShowBarycentrics.AutoSize = true;
			this.radioButtonShowBarycentrics.Location = new System.Drawing.Point(110, 69);
			this.radioButtonShowBarycentrics.Name = "radioButtonShowBarycentrics";
			this.radioButtonShowBarycentrics.Size = new System.Drawing.Size(83, 17);
			this.radioButtonShowBarycentrics.TabIndex = 18;
			this.radioButtonShowBarycentrics.Text = "Barycentrics";
			this.radioButtonShowBarycentrics.UseVisualStyleBackColor = true;
			// 
			// radioButtonShowTemperature
			// 
			this.radioButtonShowTemperature.AutoSize = true;
			this.radioButtonShowTemperature.Checked = true;
			this.radioButtonShowTemperature.Location = new System.Drawing.Point(49, 69);
			this.radioButtonShowTemperature.Name = "radioButtonShowTemperature";
			this.radioButtonShowTemperature.Size = new System.Drawing.Size(55, 17);
			this.radioButtonShowTemperature.TabIndex = 18;
			this.radioButtonShowTemperature.TabStop = true;
			this.radioButtonShowTemperature.Text = "Temp.";
			this.radioButtonShowTemperature.UseVisualStyleBackColor = true;
			// 
			// labelSearchResults
			// 
			this.labelSearchResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSearchResults.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelSearchResults.Location = new System.Drawing.Point(6, 45);
			this.labelSearchResults.Name = "labelSearchResults";
			this.labelSearchResults.Size = new System.Drawing.Size(303, 190);
			this.labelSearchResults.TabIndex = 2;
			this.labelSearchResults.Text = "No result.";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(8, 122);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(86, 13);
			this.label9.TabIndex = 1;
			this.label9.Text = "Processed input:";
			// 
			// textBoxSearch
			// 
			this.textBoxSearch.Location = new System.Drawing.Point(11, 19);
			this.textBoxSearch.Multiline = true;
			this.textBoxSearch.Name = "textBoxSearch";
			this.textBoxSearch.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxSearch.Size = new System.Drawing.Size(295, 96);
			this.textBoxSearch.TabIndex = 0;
			this.textBoxSearch.WordWrap = false;
			this.textBoxSearch.TextChanged += new System.EventHandler(this.textBoxSearch_TextChanged);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.labelProcessedQuery);
			this.groupBox2.Controls.Add(this.textBoxSearch);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.Location = new System.Drawing.Point(794, 67);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(315, 242);
			this.groupBox2.TabIndex = 24;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Search Query";
			// 
			// labelProcessedQuery
			// 
			this.labelProcessedQuery.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelProcessedQuery.Location = new System.Drawing.Point(11, 145);
			this.labelProcessedQuery.Name = "labelProcessedQuery";
			this.labelProcessedQuery.Size = new System.Drawing.Size(295, 89);
			this.labelProcessedQuery.TabIndex = 2;
			this.labelProcessedQuery.Text = "No query source.";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.labelSearchResults);
			this.groupBox3.Controls.Add(this.integerTrackbarControlSignificantResultsCount);
			this.groupBox3.Controls.Add(this.buttonGrabResults);
			this.groupBox3.Location = new System.Drawing.Point(794, 513);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(315, 238);
			this.groupBox3.TabIndex = 25;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Search Results";
			// 
			// integerTrackbarControlSignificantResultsCount
			// 
			this.integerTrackbarControlSignificantResultsCount.Location = new System.Drawing.Point(106, 20);
			this.integerTrackbarControlSignificantResultsCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlSignificantResultsCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlSignificantResultsCount.Name = "integerTrackbarControlSignificantResultsCount";
			this.integerTrackbarControlSignificantResultsCount.RangeMin = 1;
			this.integerTrackbarControlSignificantResultsCount.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlSignificantResultsCount.TabIndex = 22;
			this.integerTrackbarControlSignificantResultsCount.Value = 10;
			this.integerTrackbarControlSignificantResultsCount.VisibleRangeMax = 10;
			this.integerTrackbarControlSignificantResultsCount.VisibleRangeMin = 1;
			// 
			// GraphForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1118, 793);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.floatTrackbarControlDiffusionConstant);
			this.Controls.Add(this.buttonSave);
			this.Controls.Add(this.buttonLoad);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "GraphForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Graph Viz Test";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
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
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button buttonLoad;
		private System.Windows.Forms.Button buttonSave;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDiffusionConstant;
		private System.Windows.Forms.Button buttonGrabResults;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlResultsTolerance;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox textBoxSearch;
		private System.Windows.Forms.Label labelSearchResults;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label labelProcessedQuery;
		private System.Windows.Forms.GroupBox groupBox3;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlSignificantResultsCount;
		private System.Windows.Forms.RadioButton radioButtonShowTemperature;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton radioButtonShowBarycentrics;
		private System.Windows.Forms.RadioButton radioButtonShowResults;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlShowQuerySourceIndex;
		private System.Windows.Forms.Label label3;
	}
}