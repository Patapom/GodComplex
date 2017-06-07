namespace VoronoiVisualizer
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
			this.buttonReload = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.checkBoxSimulate = new System.Windows.Forms.CheckBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.floatTrackbarControlForce = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.integerTrackbarControlNeighborsCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.radioButtonHammersley = new System.Windows.Forms.RadioButton();
			this.radioButtonRandom = new System.Windows.Forms.RadioButton();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.buttonBuildCell = new System.Windows.Forms.Button();
			this.checkBoxRenderCell = new System.Windows.Forms.CheckBox();
			this.labelStats = new System.Windows.Forms.Label();
			this.buttonDumpDirections = new System.Windows.Forms.Button();
			this.checkBoxHemisphere = new System.Windows.Forms.CheckBox();
			this.checkBoxForceOneSample = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(950, 634);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 2;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(733, 41);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(86, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Neighbors Count";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(733, 65);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(34, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Force";
			// 
			// checkBoxSimulate
			// 
			this.checkBoxSimulate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxSimulate.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBoxSimulate.AutoSize = true;
			this.checkBoxSimulate.Location = new System.Drawing.Point(825, 89);
			this.checkBoxSimulate.Name = "checkBoxSimulate";
			this.checkBoxSimulate.Size = new System.Drawing.Size(57, 23);
			this.checkBoxSimulate.TabIndex = 4;
			this.checkBoxSimulate.Text = "Simulate";
			this.checkBoxSimulate.UseVisualStyleBackColor = true;
			this.checkBoxSimulate.CheckedChanged += new System.EventHandler(this.checkBoxSimulate_CheckedChanged);
			// 
			// panel1
			// 
			this.panel1.Location = new System.Drawing.Point(12, 12);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(715, 645);
			this.panel1.TabIndex = 5;
			// 
			// floatTrackbarControlForce
			// 
			this.floatTrackbarControlForce.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlForce.Location = new System.Drawing.Point(825, 63);
			this.floatTrackbarControlForce.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlForce.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlForce.Name = "floatTrackbarControlForce";
			this.floatTrackbarControlForce.RangeMax = 10000F;
			this.floatTrackbarControlForce.RangeMin = 0F;
			this.floatTrackbarControlForce.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlForce.TabIndex = 1;
			this.floatTrackbarControlForce.Value = 100F;
			this.floatTrackbarControlForce.VisibleRangeMax = 200F;
			// 
			// integerTrackbarControlNeighborsCount
			// 
			this.integerTrackbarControlNeighborsCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.integerTrackbarControlNeighborsCount.Location = new System.Drawing.Point(825, 37);
			this.integerTrackbarControlNeighborsCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlNeighborsCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlNeighborsCount.Name = "integerTrackbarControlNeighborsCount";
			this.integerTrackbarControlNeighborsCount.RangeMax = 256;
			this.integerTrackbarControlNeighborsCount.RangeMin = 1;
			this.integerTrackbarControlNeighborsCount.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlNeighborsCount.TabIndex = 0;
			this.integerTrackbarControlNeighborsCount.Value = 128;
			this.integerTrackbarControlNeighborsCount.VisibleRangeMax = 128;
			this.integerTrackbarControlNeighborsCount.VisibleRangeMin = 1;
			this.integerTrackbarControlNeighborsCount.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlNeighborsCount_ValueChanged);
			// 
			// radioButtonHammersley
			// 
			this.radioButtonHammersley.AutoSize = true;
			this.radioButtonHammersley.Checked = true;
			this.radioButtonHammersley.Location = new System.Drawing.Point(736, 12);
			this.radioButtonHammersley.Name = "radioButtonHammersley";
			this.radioButtonHammersley.Size = new System.Drawing.Size(82, 17);
			this.radioButtonHammersley.TabIndex = 6;
			this.radioButtonHammersley.TabStop = true;
			this.radioButtonHammersley.Text = "Hammersley";
			this.radioButtonHammersley.UseVisualStyleBackColor = true;
			this.radioButtonHammersley.CheckedChanged += new System.EventHandler(this.radioButtonHammersley_CheckedChanged);
			// 
			// radioButtonRandom
			// 
			this.radioButtonRandom.AutoSize = true;
			this.radioButtonRandom.Location = new System.Drawing.Point(825, 12);
			this.radioButtonRandom.Name = "radioButtonRandom";
			this.radioButtonRandom.Size = new System.Drawing.Size(65, 17);
			this.radioButtonRandom.TabIndex = 6;
			this.radioButtonRandom.Text = "Random";
			this.radioButtonRandom.UseVisualStyleBackColor = true;
			this.radioButtonRandom.CheckedChanged += new System.EventHandler(this.radioButtonRandom_CheckedChanged);
			// 
			// timer1
			// 
			this.timer1.Interval = 5;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// buttonBuildCell
			// 
			this.buttonBuildCell.Location = new System.Drawing.Point(825, 119);
			this.buttonBuildCell.Name = "buttonBuildCell";
			this.buttonBuildCell.Size = new System.Drawing.Size(96, 23);
			this.buttonBuildCell.TabIndex = 7;
			this.buttonBuildCell.Text = "Build Cell Mesh";
			this.buttonBuildCell.UseVisualStyleBackColor = true;
			this.buttonBuildCell.Click += new System.EventHandler(this.buttonBuildCell_Click);
			// 
			// checkBoxRenderCell
			// 
			this.checkBoxRenderCell.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBoxRenderCell.AutoSize = true;
			this.checkBoxRenderCell.Location = new System.Drawing.Point(736, 119);
			this.checkBoxRenderCell.Name = "checkBoxRenderCell";
			this.checkBoxRenderCell.Size = new System.Drawing.Size(72, 23);
			this.checkBoxRenderCell.TabIndex = 8;
			this.checkBoxRenderCell.Text = "Render Cell";
			this.checkBoxRenderCell.CheckedChanged += new System.EventHandler(this.checkBoxRenderCell_CheckedChanged);
			// 
			// labelStats
			// 
			this.labelStats.AutoSize = true;
			this.labelStats.Location = new System.Drawing.Point(733, 166);
			this.labelStats.Name = "labelStats";
			this.labelStats.Size = new System.Drawing.Size(35, 13);
			this.labelStats.TabIndex = 9;
			this.labelStats.Text = "label3";
			// 
			// buttonDumpDirections
			// 
			this.buttonDumpDirections.Location = new System.Drawing.Point(733, 634);
			this.buttonDumpDirections.Name = "buttonDumpDirections";
			this.buttonDumpDirections.Size = new System.Drawing.Size(149, 23);
			this.buttonDumpDirections.TabIndex = 10;
			this.buttonDumpDirections.Text = "Copy Directions to Clipboard";
			this.buttonDumpDirections.UseVisualStyleBackColor = true;
			this.buttonDumpDirections.Click += new System.EventHandler(this.buttonDumpDirections_Click);
			// 
			// checkBoxHemisphere
			// 
			this.checkBoxHemisphere.AutoSize = true;
			this.checkBoxHemisphere.Location = new System.Drawing.Point(927, 123);
			this.checkBoxHemisphere.Name = "checkBoxHemisphere";
			this.checkBoxHemisphere.Size = new System.Drawing.Size(106, 17);
			this.checkBoxHemisphere.TabIndex = 11;
			this.checkBoxHemisphere.Text = "Only Hemisphere";
			this.checkBoxHemisphere.UseVisualStyleBackColor = true;
			// 
			// checkBoxForceOneSample
			// 
			this.checkBoxForceOneSample.AutoSize = true;
			this.checkBoxForceOneSample.Location = new System.Drawing.Point(927, 146);
			this.checkBoxForceOneSample.Name = "checkBoxForceOneSample";
			this.checkBoxForceOneSample.Size = new System.Drawing.Size(100, 17);
			this.checkBoxForceOneSample.TabIndex = 11;
			this.checkBoxForceOneSample.Text = "Force 1 Sample";
			this.checkBoxForceOneSample.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1037, 669);
			this.Controls.Add(this.checkBoxForceOneSample);
			this.Controls.Add(this.checkBoxHemisphere);
			this.Controls.Add(this.buttonDumpDirections);
			this.Controls.Add(this.labelStats);
			this.Controls.Add(this.checkBoxRenderCell);
			this.Controls.Add(this.buttonBuildCell);
			this.Controls.Add(this.radioButtonRandom);
			this.Controls.Add(this.radioButtonHammersley);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.checkBoxSimulate);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.floatTrackbarControlForce);
			this.Controls.Add(this.integerTrackbarControlNeighborsCount);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Voronoï Visualizer";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlNeighborsCount;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlForce;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox checkBoxSimulate;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.RadioButton radioButtonHammersley;
		private System.Windows.Forms.RadioButton radioButtonRandom;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button buttonBuildCell;
		private System.Windows.Forms.CheckBox checkBoxRenderCell;
		private System.Windows.Forms.Label labelStats;
		private System.Windows.Forms.Button buttonDumpDirections;
		private System.Windows.Forms.CheckBox checkBoxHemisphere;
		private System.Windows.Forms.CheckBox checkBoxForceOneSample;
	}
}

