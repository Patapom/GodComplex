namespace TestBoxFitting
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
			this.textBoxPlanes = new System.Windows.Forms.TextBox();
			this.integerTrackbarControlRoomPlanesCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.integerTrackbarControlObstacles = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.integerTrackbarControlResultPlanesCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.integerTrackbarControlRandomSeed = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.floatTrackbarControlWeightExponent = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.radioButtonBest = new System.Windows.Forms.RadioButton();
			this.radioButtonProbabilities = new System.Windows.Forms.RadioButton();
			this.radioButtonNormalWeight = new System.Windows.Forms.RadioButton();
			this.label6 = new System.Windows.Forms.Label();
			this.floatTrackbarControlDismissFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.radioButtonDismissWeight = new System.Windows.Forms.RadioButton();
			this.radioButtonDismissKappa = new System.Windows.Forms.RadioButton();
			this.radioButtonUseBest = new System.Windows.Forms.RadioButton();
			this.integerTrackbarControlKeepBestPlanesCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label7 = new System.Windows.Forms.Label();
			this.panelDismissFactor = new System.Windows.Forms.Panel();
			this.panelKeepBestPlanes = new System.Windows.Forms.Panel();
			this.label8 = new System.Windows.Forms.Label();
			this.floatTrackbarControlSimilarPlanes = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.checkBoxShowDismissedPlanes = new System.Windows.Forms.CheckBox();
			this.radioButtonDefaultEMWeight = new System.Windows.Forms.RadioButton();
			this.panel3 = new System.Windows.Forms.Panel();
			this.radioButtonLargestD = new System.Windows.Forms.RadioButton();
			this.radioButtonWeightHybrid = new System.Windows.Forms.RadioButton();
			this.radioButtonNormalAffinity = new System.Windows.Forms.RadioButton();
			this.label9 = new System.Windows.Forms.Label();
			this.panelOutput = new TestBoxFitting.PanelOutput(this.components);
			this.panelHistogram = new TestBoxFitting.PanelHistogram(this.components);
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panelDismissFactor.SuspendLayout();
			this.panelKeepBestPlanes.SuspendLayout();
			this.panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBoxPlanes
			// 
			this.textBoxPlanes.Location = new System.Drawing.Point(12, 629);
			this.textBoxPlanes.Multiline = true;
			this.textBoxPlanes.Name = "textBoxPlanes";
			this.textBoxPlanes.ReadOnly = true;
			this.textBoxPlanes.Size = new System.Drawing.Size(500, 220);
			this.textBoxPlanes.TabIndex = 2;
			// 
			// integerTrackbarControlRoomPlanesCount
			// 
			this.integerTrackbarControlRoomPlanesCount.Location = new System.Drawing.Point(657, 627);
			this.integerTrackbarControlRoomPlanesCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlRoomPlanesCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlRoomPlanesCount.Name = "integerTrackbarControlRoomPlanesCount";
			this.integerTrackbarControlRoomPlanesCount.RangeMax = 20;
			this.integerTrackbarControlRoomPlanesCount.RangeMin = 1;
			this.integerTrackbarControlRoomPlanesCount.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlRoomPlanesCount.TabIndex = 3;
			this.integerTrackbarControlRoomPlanesCount.Value = 4;
			this.integerTrackbarControlRoomPlanesCount.VisibleRangeMax = 10;
			this.integerTrackbarControlRoomPlanesCount.VisibleRangeMin = 1;
			this.integerTrackbarControlRoomPlanesCount.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlRoomPlanesCount_ValueChanged);
			this.integerTrackbarControlRoomPlanesCount.SliderDragStop += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.SliderDragStopEventHandler(this.integerTrackbarControlRoomPlanesCount_SliderDragStop);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(527, 632);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(95, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Room Walls Count";
			// 
			// integerTrackbarControlObstacles
			// 
			this.integerTrackbarControlObstacles.Location = new System.Drawing.Point(657, 653);
			this.integerTrackbarControlObstacles.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlObstacles.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlObstacles.Name = "integerTrackbarControlObstacles";
			this.integerTrackbarControlObstacles.RangeMax = 100;
			this.integerTrackbarControlObstacles.RangeMin = 0;
			this.integerTrackbarControlObstacles.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlObstacles.TabIndex = 3;
			this.integerTrackbarControlObstacles.Value = 30;
			this.integerTrackbarControlObstacles.VisibleRangeMax = 40;
			this.integerTrackbarControlObstacles.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlObstacles_ValueChanged);
			this.integerTrackbarControlObstacles.SliderDragStop += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.SliderDragStopEventHandler(this.integerTrackbarControlObstacles_SliderDragStop);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(527, 658);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(85, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Obstacles Count";
			// 
			// integerTrackbarControlResultPlanesCount
			// 
			this.integerTrackbarControlResultPlanesCount.Location = new System.Drawing.Point(657, 679);
			this.integerTrackbarControlResultPlanesCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlResultPlanesCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlResultPlanesCount.Name = "integerTrackbarControlResultPlanesCount";
			this.integerTrackbarControlResultPlanesCount.RangeMax = 20;
			this.integerTrackbarControlResultPlanesCount.RangeMin = 1;
			this.integerTrackbarControlResultPlanesCount.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlResultPlanesCount.TabIndex = 3;
			this.integerTrackbarControlResultPlanesCount.Value = 8;
			this.integerTrackbarControlResultPlanesCount.VisibleRangeMax = 10;
			this.integerTrackbarControlResultPlanesCount.VisibleRangeMin = 1;
			this.integerTrackbarControlResultPlanesCount.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlResultPlanesCount_ValueChanged);
			this.integerTrackbarControlResultPlanesCount.SliderDragStop += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.SliderDragStopEventHandler(this.integerTrackbarControlResultPlanesCount_SliderDragStop);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(527, 684);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(125, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Requested Planes Count";
			// 
			// integerTrackbarControlRandomSeed
			// 
			this.integerTrackbarControlRandomSeed.Location = new System.Drawing.Point(996, 627);
			this.integerTrackbarControlRandomSeed.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlRandomSeed.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlRandomSeed.Name = "integerTrackbarControlRandomSeed";
			this.integerTrackbarControlRandomSeed.RangeMax = 1000;
			this.integerTrackbarControlRandomSeed.RangeMin = 1;
			this.integerTrackbarControlRandomSeed.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlRandomSeed.TabIndex = 3;
			this.integerTrackbarControlRandomSeed.Value = 1;
			this.integerTrackbarControlRandomSeed.VisibleRangeMax = 1000;
			this.integerTrackbarControlRandomSeed.VisibleRangeMin = 1;
			this.integerTrackbarControlRandomSeed.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlRoomPlanesCount_ValueChanged);
			this.integerTrackbarControlRandomSeed.SliderDragStop += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.SliderDragStopEventHandler(this.integerTrackbarControlRoomPlanesCount_SliderDragStop);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(913, 632);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(75, 13);
			this.label4.TabIndex = 4;
			this.label4.Text = "Random Seed";
			// 
			// floatTrackbarControlWeightExponent
			// 
			this.floatTrackbarControlWeightExponent.Location = new System.Drawing.Point(657, 778);
			this.floatTrackbarControlWeightExponent.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlWeightExponent.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlWeightExponent.Name = "floatTrackbarControlWeightExponent";
			this.floatTrackbarControlWeightExponent.RangeMin = 0F;
			this.floatTrackbarControlWeightExponent.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlWeightExponent.TabIndex = 5;
			this.floatTrackbarControlWeightExponent.Value = 20F;
			this.floatTrackbarControlWeightExponent.VisibleRangeMax = 100F;
			this.floatTrackbarControlWeightExponent.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlWeightExponent_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(527, 780);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(89, 13);
			this.label5.TabIndex = 4;
			this.label5.Text = "Weight Exponent";
			// 
			// radioButtonBest
			// 
			this.radioButtonBest.AutoSize = true;
			this.radioButtonBest.Checked = true;
			this.radioButtonBest.Location = new System.Drawing.Point(10, 1);
			this.radioButtonBest.Name = "radioButtonBest";
			this.radioButtonBest.Size = new System.Drawing.Size(68, 17);
			this.radioButtonBest.TabIndex = 6;
			this.radioButtonBest.TabStop = true;
			this.radioButtonBest.Text = "Use Best";
			this.radioButtonBest.UseVisualStyleBackColor = true;
			this.radioButtonBest.CheckedChanged += new System.EventHandler(this.radioButtonNormalWeight_CheckedChanged);
			// 
			// radioButtonProbabilities
			// 
			this.radioButtonProbabilities.AutoSize = true;
			this.radioButtonProbabilities.Location = new System.Drawing.Point(84, 1);
			this.radioButtonProbabilities.Name = "radioButtonProbabilities";
			this.radioButtonProbabilities.Size = new System.Drawing.Size(103, 17);
			this.radioButtonProbabilities.TabIndex = 6;
			this.radioButtonProbabilities.Text = "Use Probabilities";
			this.radioButtonProbabilities.UseVisualStyleBackColor = true;
			this.radioButtonProbabilities.CheckedChanged += new System.EventHandler(this.radioButtonNormalWeight_CheckedChanged);
			// 
			// radioButtonNormalWeight
			// 
			this.radioButtonNormalWeight.AutoSize = true;
			this.radioButtonNormalWeight.Location = new System.Drawing.Point(193, 1);
			this.radioButtonNormalWeight.Name = "radioButtonNormalWeight";
			this.radioButtonNormalWeight.Size = new System.Drawing.Size(117, 17);
			this.radioButtonNormalWeight.TabIndex = 6;
			this.radioButtonNormalWeight.Text = "Use Normal Weight";
			this.radioButtonNormalWeight.UseVisualStyleBackColor = true;
			this.radioButtonNormalWeight.CheckedChanged += new System.EventHandler(this.radioButtonNormalWeight_CheckedChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(3, 6);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(75, 13);
			this.label6.TabIndex = 4;
			this.label6.Text = "Dismiss Factor";
			// 
			// floatTrackbarControlDismissFactor
			// 
			this.floatTrackbarControlDismissFactor.Location = new System.Drawing.Point(133, 3);
			this.floatTrackbarControlDismissFactor.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDismissFactor.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDismissFactor.Name = "floatTrackbarControlDismissFactor";
			this.floatTrackbarControlDismissFactor.RangeMax = 1F;
			this.floatTrackbarControlDismissFactor.RangeMin = 0F;
			this.floatTrackbarControlDismissFactor.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDismissFactor.TabIndex = 5;
			this.floatTrackbarControlDismissFactor.Value = 0.5F;
			this.floatTrackbarControlDismissFactor.VisibleRangeMax = 1F;
			this.floatTrackbarControlDismissFactor.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlWeightExponent_ValueChanged);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.radioButtonBest);
			this.panel1.Controls.Add(this.radioButtonProbabilities);
			this.panel1.Controls.Add(this.radioButtonNormalWeight);
			this.panel1.Location = new System.Drawing.Point(520, 758);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(337, 18);
			this.panel1.TabIndex = 8;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.radioButtonDismissWeight);
			this.panel2.Controls.Add(this.radioButtonDismissKappa);
			this.panel2.Controls.Add(this.radioButtonUseBest);
			this.panel2.Location = new System.Drawing.Point(520, 799);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(337, 18);
			this.panel2.TabIndex = 8;
			// 
			// radioButtonDismissWeight
			// 
			this.radioButtonDismissWeight.AutoSize = true;
			this.radioButtonDismissWeight.Location = new System.Drawing.Point(84, 1);
			this.radioButtonDismissWeight.Name = "radioButtonDismissWeight";
			this.radioButtonDismissWeight.Size = new System.Drawing.Size(108, 17);
			this.radioButtonDismissWeight.TabIndex = 6;
			this.radioButtonDismissWeight.Text = "Dismiss by weight";
			this.radioButtonDismissWeight.UseVisualStyleBackColor = true;
			this.radioButtonDismissWeight.CheckedChanged += new System.EventHandler(this.radioButtonUseBest_CheckedChanged);
			// 
			// radioButtonDismissKappa
			// 
			this.radioButtonDismissKappa.AutoSize = true;
			this.radioButtonDismissKappa.Location = new System.Drawing.Point(193, 1);
			this.radioButtonDismissKappa.Name = "radioButtonDismissKappa";
			this.radioButtonDismissKappa.Size = new System.Drawing.Size(108, 17);
			this.radioButtonDismissKappa.TabIndex = 6;
			this.radioButtonDismissKappa.Text = "Dismiss by Kappa";
			this.radioButtonDismissKappa.UseVisualStyleBackColor = true;
			this.radioButtonDismissKappa.CheckedChanged += new System.EventHandler(this.radioButtonUseBest_CheckedChanged);
			// 
			// radioButtonUseBest
			// 
			this.radioButtonUseBest.AutoSize = true;
			this.radioButtonUseBest.Checked = true;
			this.radioButtonUseBest.Location = new System.Drawing.Point(10, 1);
			this.radioButtonUseBest.Name = "radioButtonUseBest";
			this.radioButtonUseBest.Size = new System.Drawing.Size(77, 17);
			this.radioButtonUseBest.TabIndex = 6;
			this.radioButtonUseBest.TabStop = true;
			this.radioButtonUseBest.Text = "No Dismiss";
			this.radioButtonUseBest.UseVisualStyleBackColor = true;
			this.radioButtonUseBest.CheckedChanged += new System.EventHandler(this.radioButtonUseBest_CheckedChanged);
			// 
			// integerTrackbarControlKeepBestPlanesCount
			// 
			this.integerTrackbarControlKeepBestPlanesCount.Location = new System.Drawing.Point(131, 4);
			this.integerTrackbarControlKeepBestPlanesCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlKeepBestPlanesCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlKeepBestPlanesCount.Name = "integerTrackbarControlKeepBestPlanesCount";
			this.integerTrackbarControlKeepBestPlanesCount.RangeMax = 20;
			this.integerTrackbarControlKeepBestPlanesCount.RangeMin = 1;
			this.integerTrackbarControlKeepBestPlanesCount.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlKeepBestPlanesCount.TabIndex = 3;
			this.integerTrackbarControlKeepBestPlanesCount.Value = 4;
			this.integerTrackbarControlKeepBestPlanesCount.VisibleRangeMax = 10;
			this.integerTrackbarControlKeepBestPlanesCount.VisibleRangeMin = 1;
			this.integerTrackbarControlKeepBestPlanesCount.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlResultPlanesCount_ValueChanged);
			this.integerTrackbarControlKeepBestPlanesCount.SliderDragStop += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.SliderDragStopEventHandler(this.integerTrackbarControlResultPlanesCount_SliderDragStop);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(3, 9);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(122, 13);
			this.label7.TabIndex = 4;
			this.label7.Text = "Keep Best Planes Count";
			// 
			// panelDismissFactor
			// 
			this.panelDismissFactor.Controls.Add(this.floatTrackbarControlDismissFactor);
			this.panelDismissFactor.Controls.Add(this.label6);
			this.panelDismissFactor.Location = new System.Drawing.Point(518, 821);
			this.panelDismissFactor.Name = "panelDismissFactor";
			this.panelDismissFactor.Size = new System.Drawing.Size(339, 28);
			this.panelDismissFactor.TabIndex = 9;
			this.panelDismissFactor.Visible = false;
			// 
			// panelKeepBestPlanes
			// 
			this.panelKeepBestPlanes.Controls.Add(this.integerTrackbarControlKeepBestPlanesCount);
			this.panelKeepBestPlanes.Controls.Add(this.label7);
			this.panelKeepBestPlanes.Location = new System.Drawing.Point(865, 675);
			this.panelKeepBestPlanes.Name = "panelKeepBestPlanes";
			this.panelKeepBestPlanes.Size = new System.Drawing.Size(339, 28);
			this.panelKeepBestPlanes.TabIndex = 10;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(527, 712);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(110, 13);
			this.label8.TabIndex = 4;
			this.label8.Text = "Dismiss Similar Planes";
			// 
			// floatTrackbarControlSimilarPlanes
			// 
			this.floatTrackbarControlSimilarPlanes.Location = new System.Drawing.Point(657, 710);
			this.floatTrackbarControlSimilarPlanes.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlSimilarPlanes.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlSimilarPlanes.Name = "floatTrackbarControlSimilarPlanes";
			this.floatTrackbarControlSimilarPlanes.RangeMax = 1F;
			this.floatTrackbarControlSimilarPlanes.RangeMin = 0F;
			this.floatTrackbarControlSimilarPlanes.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlSimilarPlanes.TabIndex = 5;
			this.floatTrackbarControlSimilarPlanes.Value = 0.5F;
			this.floatTrackbarControlSimilarPlanes.VisibleRangeMax = 1F;
			this.floatTrackbarControlSimilarPlanes.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlWeightExponent_ValueChanged);
			// 
			// checkBoxShowDismissedPlanes
			// 
			this.checkBoxShowDismissedPlanes.AutoSize = true;
			this.checkBoxShowDismissedPlanes.Location = new System.Drawing.Point(1055, 239);
			this.checkBoxShowDismissedPlanes.Name = "checkBoxShowDismissedPlanes";
			this.checkBoxShowDismissedPlanes.Size = new System.Drawing.Size(138, 17);
			this.checkBoxShowDismissedPlanes.TabIndex = 11;
			this.checkBoxShowDismissedPlanes.Text = "Show Dismissed Planes";
			this.checkBoxShowDismissedPlanes.UseVisualStyleBackColor = true;
			this.checkBoxShowDismissedPlanes.CheckedChanged += new System.EventHandler(this.checkBoxShowDismissedPlanes_CheckedChanged);
			// 
			// radioButtonDefaultEMWeight
			// 
			this.radioButtonDefaultEMWeight.AutoSize = true;
			this.radioButtonDefaultEMWeight.Location = new System.Drawing.Point(54, 5);
			this.radioButtonDefaultEMWeight.Name = "radioButtonDefaultEMWeight";
			this.radioButtonDefaultEMWeight.Size = new System.Drawing.Size(78, 17);
			this.radioButtonDefaultEMWeight.TabIndex = 14;
			this.radioButtonDefaultEMWeight.Text = "Default EM";
			this.radioButtonDefaultEMWeight.UseVisualStyleBackColor = true;
			this.radioButtonDefaultEMWeight.CheckedChanged += new System.EventHandler(this.radioButtonDefaultEMWeight_CheckedChanged);
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.radioButtonLargestD);
			this.panel3.Controls.Add(this.radioButtonWeightHybrid);
			this.panel3.Controls.Add(this.radioButtonNormalAffinity);
			this.panel3.Controls.Add(this.radioButtonDefaultEMWeight);
			this.panel3.Controls.Add(this.label9);
			this.panel3.Location = new System.Drawing.Point(520, 731);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(376, 25);
			this.panel3.TabIndex = 15;
			// 
			// radioButtonLargestD
			// 
			this.radioButtonLargestD.AutoSize = true;
			this.radioButtonLargestD.Location = new System.Drawing.Point(134, 5);
			this.radioButtonLargestD.Name = "radioButtonLargestD";
			this.radioButtonLargestD.Size = new System.Drawing.Size(71, 17);
			this.radioButtonLargestD.TabIndex = 14;
			this.radioButtonLargestD.Text = "Largest D";
			this.radioButtonLargestD.UseVisualStyleBackColor = true;
			this.radioButtonLargestD.CheckedChanged += new System.EventHandler(this.radioButtonDefaultEMWeight_CheckedChanged);
			// 
			// radioButtonWeightHybrid
			// 
			this.radioButtonWeightHybrid.AutoSize = true;
			this.radioButtonWeightHybrid.Checked = true;
			this.radioButtonWeightHybrid.Location = new System.Drawing.Point(309, 5);
			this.radioButtonWeightHybrid.Name = "radioButtonWeightHybrid";
			this.radioButtonWeightHybrid.Size = new System.Drawing.Size(55, 17);
			this.radioButtonWeightHybrid.TabIndex = 14;
			this.radioButtonWeightHybrid.TabStop = true;
			this.radioButtonWeightHybrid.Text = "Hybrid";
			this.radioButtonWeightHybrid.UseVisualStyleBackColor = true;
			this.radioButtonWeightHybrid.CheckedChanged += new System.EventHandler(this.radioButtonDefaultEMWeight_CheckedChanged);
			// 
			// radioButtonNormalAffinity
			// 
			this.radioButtonNormalAffinity.AutoSize = true;
			this.radioButtonNormalAffinity.Location = new System.Drawing.Point(211, 5);
			this.radioButtonNormalAffinity.Name = "radioButtonNormalAffinity";
			this.radioButtonNormalAffinity.Size = new System.Drawing.Size(92, 17);
			this.radioButtonNormalAffinity.TabIndex = 14;
			this.radioButtonNormalAffinity.Text = "Normal Affinity";
			this.radioButtonNormalAffinity.UseVisualStyleBackColor = true;
			this.radioButtonNormalAffinity.CheckedChanged += new System.EventHandler(this.radioButtonDefaultEMWeight_CheckedChanged);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(7, 7);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(44, 13);
			this.label9.TabIndex = 4;
			this.label9.Text = "Weight:";
			// 
			// panelOutput
			// 
			this.panelOutput.BackColor = System.Drawing.Color.MistyRose;
			this.panelOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.ShowDismissedPlanes = false;
			this.panelOutput.Size = new System.Drawing.Size(1029, 611);
			this.panelOutput.TabIndex = 0;
			// 
			// panelHistogram
			// 
			this.panelHistogram.Location = new System.Drawing.Point(1055, 12);
			this.panelHistogram.Name = "panelHistogram";
			this.panelHistogram.Size = new System.Drawing.Size(322, 220);
			this.panelHistogram.TabIndex = 1;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1523, 861);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.checkBoxShowDismissedPlanes);
			this.Controls.Add(this.panelKeepBestPlanes);
			this.Controls.Add(this.panelDismissFactor);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.floatTrackbarControlSimilarPlanes);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.floatTrackbarControlWeightExponent);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.integerTrackbarControlResultPlanesCount);
			this.Controls.Add(this.integerTrackbarControlObstacles);
			this.Controls.Add(this.integerTrackbarControlRandomSeed);
			this.Controls.Add(this.integerTrackbarControlRoomPlanesCount);
			this.Controls.Add(this.textBoxPlanes);
			this.Controls.Add(this.panelOutput);
			this.Controls.Add(this.panelHistogram);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Planes Fitting Test";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panelDismissFactor.ResumeLayout(false);
			this.panelDismissFactor.PerformLayout();
			this.panelKeepBestPlanes.ResumeLayout(false);
			this.panelKeepBestPlanes.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelOutput;
		private PanelHistogram panelHistogram;
		private System.Windows.Forms.TextBox textBoxPlanes;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlRoomPlanesCount;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlObstacles;
		private System.Windows.Forms.Label label2;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlResultPlanesCount;
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlRandomSeed;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlWeightExponent;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.RadioButton radioButtonBest;
		private System.Windows.Forms.RadioButton radioButtonProbabilities;
		private System.Windows.Forms.RadioButton radioButtonNormalWeight;
		private System.Windows.Forms.Label label6;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDismissFactor;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.RadioButton radioButtonUseBest;
		private System.Windows.Forms.RadioButton radioButtonDismissWeight;
		private System.Windows.Forms.RadioButton radioButtonDismissKappa;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlKeepBestPlanesCount;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Panel panelDismissFactor;
		private System.Windows.Forms.Panel panelKeepBestPlanes;
		private System.Windows.Forms.Label label8;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlSimilarPlanes;
		private System.Windows.Forms.CheckBox checkBoxShowDismissedPlanes;
		private System.Windows.Forms.RadioButton radioButtonDefaultEMWeight;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.RadioButton radioButtonLargestD;
		private System.Windows.Forms.RadioButton radioButtonNormalAffinity;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.RadioButton radioButtonWeightHybrid;
	}
}

