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
			this.panelOutput = new TestBoxFitting.PanelOutput(this.components);
			this.panelHistogram = new TestBoxFitting.PanelHistogram(this.components);
			this.integerTrackbarControlRandomSeed = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// textBoxPlanes
			// 
			this.textBoxPlanes.Location = new System.Drawing.Point(365, 657);
			this.textBoxPlanes.Multiline = true;
			this.textBoxPlanes.Name = "textBoxPlanes";
			this.textBoxPlanes.ReadOnly = true;
			this.textBoxPlanes.Size = new System.Drawing.Size(324, 192);
			this.textBoxPlanes.TabIndex = 2;
			// 
			// integerTrackbarControlRoomPlanesCount
			// 
			this.integerTrackbarControlRoomPlanesCount.Location = new System.Drawing.Point(841, 657);
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
			this.label1.Location = new System.Drawing.Point(711, 662);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(95, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Room Walls Count";
			// 
			// integerTrackbarControlObstacles
			// 
			this.integerTrackbarControlObstacles.Location = new System.Drawing.Point(841, 683);
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
			this.label2.Location = new System.Drawing.Point(711, 688);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(85, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Obstacles Count";
			// 
			// integerTrackbarControlResultPlanesCount
			// 
			this.integerTrackbarControlResultPlanesCount.Location = new System.Drawing.Point(841, 724);
			this.integerTrackbarControlResultPlanesCount.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlResultPlanesCount.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlResultPlanesCount.Name = "integerTrackbarControlResultPlanesCount";
			this.integerTrackbarControlResultPlanesCount.RangeMax = 20;
			this.integerTrackbarControlResultPlanesCount.RangeMin = 1;
			this.integerTrackbarControlResultPlanesCount.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlResultPlanesCount.TabIndex = 3;
			this.integerTrackbarControlResultPlanesCount.Value = 4;
			this.integerTrackbarControlResultPlanesCount.VisibleRangeMax = 10;
			this.integerTrackbarControlResultPlanesCount.VisibleRangeMin = 1;
			this.integerTrackbarControlResultPlanesCount.ValueChanged += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.ValueChangedEventHandler(this.integerTrackbarControlResultPlanesCount_ValueChanged);
			this.integerTrackbarControlResultPlanesCount.SliderDragStop += new Nuaj.Cirrus.Utility.IntegerTrackbarControl.SliderDragStopEventHandler(this.integerTrackbarControlResultPlanesCount_SliderDragStop);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(711, 729);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(125, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Requested Planes Count";
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1029, 611);
			this.panelOutput.TabIndex = 0;
			// 
			// panelHistogram
			// 
			this.panelHistogram.Location = new System.Drawing.Point(12, 629);
			this.panelHistogram.Name = "panelHistogram";
			this.panelHistogram.Size = new System.Drawing.Size(347, 220);
			this.panelHistogram.TabIndex = 1;
			// 
			// integerTrackbarControlRandomSeed
			// 
			this.integerTrackbarControlRandomSeed.Location = new System.Drawing.Point(841, 829);
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
			this.label4.Location = new System.Drawing.Point(711, 834);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(75, 13);
			this.label4.TabIndex = 4;
			this.label4.Text = "Random Seed";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1053, 861);
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
			this.Text = "Form1";
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
	}
}

