namespace GenerateBlueNoise
{
	partial class GenerateBlueNoiseForm
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
			this.floatTrackbarControlScale = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlOffset = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlRadialScale = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlRadialOffset = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.floatTrackbarControlDC = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.buttonSolidAngleAlgorithm = new System.Windows.Forms.Button();
			this.labelAnnealingScore = new System.Windows.Forms.Label();
			this.buttonVoidAndCluster = new System.Windows.Forms.Button();
			this.integerTrackbarControlTexturePOT = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.integerTrackbarControlRandomSeed = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.integerTrackbarControlVectorDimension = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label8 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkBoxShowDistribution = new System.Windows.Forms.CheckBox();
			this.buttonSave = new System.Windows.Forms.Button();
			this.floatTrackbarControlSigma = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlDistributionPower = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label11 = new System.Windows.Forms.Label();
			this.integerTrackbarControlAnnealingIterations = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.floatTrackbarControlVariance = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.radioButtonNeighborMutations = new System.Windows.Forms.RadioButton();
			this.radioButtonRandomMutations = new System.Windows.Forms.RadioButton();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.panelImageSpectrum = new GenerateBlueNoise.PanelImage(this.components);
			this.panelImage = new GenerateBlueNoise.PanelImage(this.components);
			this.buttonCombine = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// floatTrackbarControlScale
			// 
			this.floatTrackbarControlScale.Location = new System.Drawing.Point(52, 19);
			this.floatTrackbarControlScale.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlScale.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlScale.Name = "floatTrackbarControlScale";
			this.floatTrackbarControlScale.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlScale.TabIndex = 2;
			this.floatTrackbarControlScale.Value = 4F;
			this.floatTrackbarControlScale.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScale_ValueChanged);
			// 
			// floatTrackbarControlOffset
			// 
			this.floatTrackbarControlOffset.Location = new System.Drawing.Point(52, 45);
			this.floatTrackbarControlOffset.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlOffset.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlOffset.Name = "floatTrackbarControlOffset";
			this.floatTrackbarControlOffset.RangeMax = 1F;
			this.floatTrackbarControlOffset.RangeMin = -1F;
			this.floatTrackbarControlOffset.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlOffset.TabIndex = 3;
			this.floatTrackbarControlOffset.Value = 0.5F;
			this.floatTrackbarControlOffset.VisibleRangeMax = 1F;
			this.floatTrackbarControlOffset.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlOffset_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(34, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Scale";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 47);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(27, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Bias";
			// 
			// floatTrackbarControlRadialScale
			// 
			this.floatTrackbarControlRadialScale.Location = new System.Drawing.Point(341, 19);
			this.floatTrackbarControlRadialScale.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlRadialScale.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlRadialScale.Name = "floatTrackbarControlRadialScale";
			this.floatTrackbarControlRadialScale.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlRadialScale.TabIndex = 4;
			this.floatTrackbarControlRadialScale.Value = 1F;
			this.floatTrackbarControlRadialScale.VisibleRangeMax = 2F;
			this.floatTrackbarControlRadialScale.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScale_ValueChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(263, 25);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Radial Range";
			// 
			// floatTrackbarControlRadialOffset
			// 
			this.floatTrackbarControlRadialOffset.Location = new System.Drawing.Point(341, 45);
			this.floatTrackbarControlRadialOffset.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlRadialOffset.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlRadialOffset.Name = "floatTrackbarControlRadialOffset";
			this.floatTrackbarControlRadialOffset.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlRadialOffset.TabIndex = 5;
			this.floatTrackbarControlRadialOffset.Value = 0F;
			this.floatTrackbarControlRadialOffset.VisibleRangeMax = 1F;
			this.floatTrackbarControlRadialOffset.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScale_ValueChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(263, 51);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(68, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "Radial Offset";
			// 
			// floatTrackbarControlDC
			// 
			this.floatTrackbarControlDC.Location = new System.Drawing.Point(636, 19);
			this.floatTrackbarControlDC.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDC.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDC.Name = "floatTrackbarControlDC";
			this.floatTrackbarControlDC.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDC.TabIndex = 6;
			this.floatTrackbarControlDC.Value = 0.5F;
			this.floatTrackbarControlDC.VisibleRangeMax = 1F;
			this.floatTrackbarControlDC.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScale_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(547, 24);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(49, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "DC Term";
			// 
			// buttonSolidAngleAlgorithm
			// 
			this.buttonSolidAngleAlgorithm.Location = new System.Drawing.Point(417, 534);
			this.buttonSolidAngleAlgorithm.Name = "buttonSolidAngleAlgorithm";
			this.buttonSolidAngleAlgorithm.Size = new System.Drawing.Size(152, 23);
			this.buttonSolidAngleAlgorithm.TabIndex = 1;
			this.buttonSolidAngleAlgorithm.Text = "Use Simulated Annealing";
			this.buttonSolidAngleAlgorithm.UseVisualStyleBackColor = true;
			this.buttonSolidAngleAlgorithm.Click += new System.EventHandler(this.buttonSolidAngleAlgorithm_Click);
			// 
			// labelAnnealingScore
			// 
			this.labelAnnealingScore.AutoSize = true;
			this.labelAnnealingScore.Location = new System.Drawing.Point(414, 591);
			this.labelAnnealingScore.Name = "labelAnnealingScore";
			this.labelAnnealingScore.Size = new System.Drawing.Size(63, 13);
			this.labelAnnealingScore.TabIndex = 2;
			this.labelAnnealingScore.Text = "Status: N/A";
			// 
			// buttonVoidAndCluster
			// 
			this.buttonVoidAndCluster.Location = new System.Drawing.Point(417, 610);
			this.buttonVoidAndCluster.Name = "buttonVoidAndCluster";
			this.buttonVoidAndCluster.Size = new System.Drawing.Size(152, 23);
			this.buttonVoidAndCluster.TabIndex = 0;
			this.buttonVoidAndCluster.Text = "Use Void-and-Cluster";
			this.buttonVoidAndCluster.UseVisualStyleBackColor = true;
			this.buttonVoidAndCluster.Click += new System.EventHandler(this.buttonVoidAndCluster_Click);
			// 
			// integerTrackbarControlTexturePOT
			// 
			this.integerTrackbarControlTexturePOT.Location = new System.Drawing.Point(159, 535);
			this.integerTrackbarControlTexturePOT.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlTexturePOT.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlTexturePOT.Name = "integerTrackbarControlTexturePOT";
			this.integerTrackbarControlTexturePOT.RangeMax = 10;
			this.integerTrackbarControlTexturePOT.RangeMin = 4;
			this.integerTrackbarControlTexturePOT.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlTexturePOT.TabIndex = 7;
			this.integerTrackbarControlTexturePOT.Value = 6;
			this.integerTrackbarControlTexturePOT.VisibleRangeMax = 8;
			this.integerTrackbarControlTexturePOT.VisibleRangeMin = 4;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(12, 539);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(141, 13);
			this.label6.TabIndex = 2;
			this.label6.Text = "Texture Size (Power of Two)";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(12, 565);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(75, 13);
			this.label7.TabIndex = 2;
			this.label7.Text = "Random Seed";
			// 
			// integerTrackbarControlRandomSeed
			// 
			this.integerTrackbarControlRandomSeed.Location = new System.Drawing.Point(159, 561);
			this.integerTrackbarControlRandomSeed.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlRandomSeed.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlRandomSeed.Name = "integerTrackbarControlRandomSeed";
			this.integerTrackbarControlRandomSeed.RangeMin = 1;
			this.integerTrackbarControlRandomSeed.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlRandomSeed.TabIndex = 8;
			this.integerTrackbarControlRandomSeed.Value = 1;
			this.integerTrackbarControlRandomSeed.VisibleRangeMax = 100000;
			this.integerTrackbarControlRandomSeed.VisibleRangeMin = 1;
			// 
			// integerTrackbarControlVectorDimension
			// 
			this.integerTrackbarControlVectorDimension.Location = new System.Drawing.Point(636, 535);
			this.integerTrackbarControlVectorDimension.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlVectorDimension.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlVectorDimension.Name = "integerTrackbarControlVectorDimension";
			this.integerTrackbarControlVectorDimension.RangeMax = 2;
			this.integerTrackbarControlVectorDimension.RangeMin = 1;
			this.integerTrackbarControlVectorDimension.Size = new System.Drawing.Size(115, 20);
			this.integerTrackbarControlVectorDimension.TabIndex = 7;
			this.integerTrackbarControlVectorDimension.Value = 1;
			this.integerTrackbarControlVectorDimension.VisibleRangeMax = 2;
			this.integerTrackbarControlVectorDimension.VisibleRangeMin = 1;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(577, 531);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(63, 28);
			this.label8.TabIndex = 2;
			this.label8.Text = "Vector Dimension";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.checkBoxShowDistribution);
			this.groupBox1.Controls.Add(this.buttonSave);
			this.groupBox1.Controls.Add(this.floatTrackbarControlScale);
			this.groupBox1.Controls.Add(this.floatTrackbarControlRadialScale);
			this.groupBox1.Controls.Add(this.floatTrackbarControlSigma);
			this.groupBox1.Controls.Add(this.floatTrackbarControlDistributionPower);
			this.groupBox1.Controls.Add(this.floatTrackbarControlDC);
			this.groupBox1.Controls.Add(this.floatTrackbarControlRadialOffset);
			this.groupBox1.Controls.Add(this.floatTrackbarControlOffset);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label11);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Location = new System.Drawing.Point(12, 664);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(951, 94);
			this.groupBox1.TabIndex = 9;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Shitty Spectrum Generation";
			// 
			// checkBoxShowDistribution
			// 
			this.checkBoxShowDistribution.AutoSize = true;
			this.checkBoxShowDistribution.Location = new System.Drawing.Point(860, 66);
			this.checkBoxShowDistribution.Name = "checkBoxShowDistribution";
			this.checkBoxShowDistribution.Size = new System.Drawing.Size(83, 17);
			this.checkBoxShowDistribution.TabIndex = 8;
			this.checkBoxShowDistribution.Text = "Show Noise";
			this.checkBoxShowDistribution.UseVisualStyleBackColor = true;
			this.checkBoxShowDistribution.CheckedChanged += new System.EventHandler(this.checkBoxShowDistribution_CheckedChanged);
			// 
			// buttonSave
			// 
			this.buttonSave.Location = new System.Drawing.Point(860, 19);
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(75, 23);
			this.buttonSave.TabIndex = 7;
			this.buttonSave.Text = "Save";
			this.buttonSave.UseVisualStyleBackColor = true;
			this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
			// 
			// floatTrackbarControlSigma
			// 
			this.floatTrackbarControlSigma.Location = new System.Drawing.Point(636, 66);
			this.floatTrackbarControlSigma.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSigma.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSigma.Name = "floatTrackbarControlSigma";
			this.floatTrackbarControlSigma.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlSigma.TabIndex = 6;
			this.floatTrackbarControlSigma.Value = 4F;
			this.floatTrackbarControlSigma.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScale_ValueChanged);
			// 
			// floatTrackbarControlDistributionPower
			// 
			this.floatTrackbarControlDistributionPower.Location = new System.Drawing.Point(636, 45);
			this.floatTrackbarControlDistributionPower.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDistributionPower.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDistributionPower.Name = "floatTrackbarControlDistributionPower";
			this.floatTrackbarControlDistributionPower.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDistributionPower.TabIndex = 6;
			this.floatTrackbarControlDistributionPower.Value = 0.8F;
			this.floatTrackbarControlDistributionPower.VisibleRangeMax = 2F;
			this.floatTrackbarControlDistributionPower.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlScale_ValueChanged);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(547, 50);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(92, 13);
			this.label11.TabIndex = 2;
			this.label11.Text = "Distribution Power";
			// 
			// integerTrackbarControlAnnealingIterations
			// 
			this.integerTrackbarControlAnnealingIterations.Location = new System.Drawing.Point(826, 535);
			this.integerTrackbarControlAnnealingIterations.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlAnnealingIterations.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlAnnealingIterations.Name = "integerTrackbarControlAnnealingIterations";
			this.integerTrackbarControlAnnealingIterations.RangeMin = 1;
			this.integerTrackbarControlAnnealingIterations.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlAnnealingIterations.TabIndex = 7;
			this.integerTrackbarControlAnnealingIterations.Value = 1000000;
			this.integerTrackbarControlAnnealingIterations.VisibleRangeMax = 2000000;
			this.integerTrackbarControlAnnealingIterations.VisibleRangeMin = 1;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(757, 531);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(63, 28);
			this.label9.TabIndex = 2;
			this.label9.Text = "Max Iterations";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(12, 591);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(84, 13);
			this.label10.TabIndex = 2;
			this.label10.Text = "Spatial Variance";
			// 
			// floatTrackbarControlVariance
			// 
			this.floatTrackbarControlVariance.Location = new System.Drawing.Point(159, 587);
			this.floatTrackbarControlVariance.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlVariance.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlVariance.Name = "floatTrackbarControlVariance";
			this.floatTrackbarControlVariance.RangeMax = 4F;
			this.floatTrackbarControlVariance.RangeMin = 0F;
			this.floatTrackbarControlVariance.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlVariance.TabIndex = 3;
			this.floatTrackbarControlVariance.Value = 1.5F;
			this.floatTrackbarControlVariance.VisibleRangeMax = 4F;
			this.floatTrackbarControlVariance.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlOffset_ValueChanged);
			// 
			// radioButtonNeighborMutations
			// 
			this.radioButtonNeighborMutations.AutoSize = true;
			this.radioButtonNeighborMutations.Location = new System.Drawing.Point(580, 565);
			this.radioButtonNeighborMutations.Name = "radioButtonNeighborMutations";
			this.radioButtonNeighborMutations.Size = new System.Drawing.Size(146, 17);
			this.radioButtonNeighborMutations.TabIndex = 10;
			this.radioButtonNeighborMutations.Text = "Neighbors Only Mutations";
			this.radioButtonNeighborMutations.UseVisualStyleBackColor = true;
			// 
			// radioButtonRandomMutations
			// 
			this.radioButtonRandomMutations.AutoSize = true;
			this.radioButtonRandomMutations.Checked = true;
			this.radioButtonRandomMutations.Location = new System.Drawing.Point(732, 565);
			this.radioButtonRandomMutations.Name = "radioButtonRandomMutations";
			this.radioButtonRandomMutations.Size = new System.Drawing.Size(114, 17);
			this.radioButtonRandomMutations.TabIndex = 10;
			this.radioButtonRandomMutations.TabStop = true;
			this.radioButtonRandomMutations.Text = "Random Mutations";
			this.radioButtonRandomMutations.UseVisualStyleBackColor = true;
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "*.png";
			this.openFileDialog.FileName = "openFileDialog";
			this.openFileDialog.Filter = "Image Files|*.*";
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.DefaultExt = "*.png";
			this.saveFileDialog.Filter = "PNG Image Files|*.png|All Files (*.*)|*.*";
			// 
			// panelImageSpectrum
			// 
			this.panelImageSpectrum.Bitmap = null;
			this.panelImageSpectrum.Location = new System.Drawing.Point(548, 12);
			this.panelImageSpectrum.Name = "panelImageSpectrum";
			this.panelImageSpectrum.Size = new System.Drawing.Size(512, 512);
			this.panelImageSpectrum.TabIndex = 0;
			this.panelImageSpectrum.Click += new System.EventHandler(this.panelImageSpectrum_Click);
			// 
			// panelImage
			// 
			this.panelImage.Bitmap = null;
			this.panelImage.Location = new System.Drawing.Point(12, 12);
			this.panelImage.Name = "panelImage";
			this.panelImage.Size = new System.Drawing.Size(512, 512);
			this.panelImage.TabIndex = 0;
			this.panelImage.Click += new System.EventHandler(this.panelImage_Click);
			// 
			// buttonCombine
			// 
			this.buttonCombine.Location = new System.Drawing.Point(417, 639);
			this.buttonCombine.Name = "buttonCombine";
			this.buttonCombine.Size = new System.Drawing.Size(75, 23);
			this.buttonCombine.TabIndex = 11;
			this.buttonCombine.Text = "Combine";
			this.buttonCombine.UseVisualStyleBackColor = true;
			this.buttonCombine.Click += new System.EventHandler(this.buttonCombine_Click);
			// 
			// GenerateBlueNoiseForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1072, 770);
			this.Controls.Add(this.buttonCombine);
			this.Controls.Add(this.radioButtonRandomMutations);
			this.Controls.Add(this.radioButtonNeighborMutations);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.integerTrackbarControlRandomSeed);
			this.Controls.Add(this.integerTrackbarControlVectorDimension);
			this.Controls.Add(this.integerTrackbarControlAnnealingIterations);
			this.Controls.Add(this.floatTrackbarControlVariance);
			this.Controls.Add(this.integerTrackbarControlTexturePOT);
			this.Controls.Add(this.buttonVoidAndCluster);
			this.Controls.Add(this.buttonSolidAngleAlgorithm);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.labelAnnealingScore);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.panelImageSpectrum);
			this.Controls.Add(this.panelImage);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "GenerateBlueNoiseForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Blue Noise Generator";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelImage panelImage;
		private PanelImage panelImageSpectrum;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlScale;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlOffset;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlRadialScale;
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlRadialOffset;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDC;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button buttonSolidAngleAlgorithm;
		private System.Windows.Forms.Label labelAnnealingScore;
		private System.Windows.Forms.Button buttonVoidAndCluster;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlTexturePOT;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlRandomSeed;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlVectorDimension;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.GroupBox groupBox1;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlAnnealingIterations;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlVariance;
		private System.Windows.Forms.RadioButton radioButtonNeighborMutations;
		private System.Windows.Forms.RadioButton radioButtonRandomMutations;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.Button buttonSave;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDistributionPower;
		private System.Windows.Forms.Label label11;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSigma;
		private System.Windows.Forms.CheckBox checkBoxShowDistribution;
		private System.Windows.Forms.Button buttonCombine;
	}
}

