namespace TriangleCurvature
{
	partial class TestTriangleCurvatureForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
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
			this.checkBoxShowNormal = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlCurvatureStrength = new UIUtility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.checkBoxEnableCorrection = new System.Windows.Forms.CheckBox();
			this.labelResult = new System.Windows.Forms.Label();
			this.floatTrackbarControlA = new UIUtility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackbarControlB = new UIUtility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlC = new UIUtility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.labelMeshInfo = new System.Windows.Forms.Label();
			this.floatTrackbarControlD = new UIUtility.FloatTrackbarControl();
			this.label5 = new System.Windows.Forms.Label();
			this.panelOutputGraph = new TriangleCurvature.PanelOutput(this.components);
			this.panelOutput = new TriangleCurvature.PanelOutput3D(this.components);
			this.SuspendLayout();
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(1577, 780);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 1;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// checkBoxShowNormal
			// 
			this.checkBoxShowNormal.AutoSize = true;
			this.checkBoxShowNormal.Location = new System.Drawing.Point(348, 744);
			this.checkBoxShowNormal.Name = "checkBoxShowNormal";
			this.checkBoxShowNormal.Size = new System.Drawing.Size(94, 17);
			this.checkBoxShowNormal.TabIndex = 2;
			this.checkBoxShowNormal.Text = "Show Normals";
			this.checkBoxShowNormal.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControlCurvatureStrength
			// 
			this.floatTrackbarControlCurvatureStrength.Location = new System.Drawing.Point(109, 742);
			this.floatTrackbarControlCurvatureStrength.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlCurvatureStrength.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlCurvatureStrength.Name = "floatTrackbarControlCurvatureStrength";
			this.floatTrackbarControlCurvatureStrength.RangeMax = 1F;
			this.floatTrackbarControlCurvatureStrength.RangeMin = 0F;
			this.floatTrackbarControlCurvatureStrength.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlCurvatureStrength.TabIndex = 3;
			this.floatTrackbarControlCurvatureStrength.Value = 1F;
			this.floatTrackbarControlCurvatureStrength.VisibleRangeMax = 1F;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 745);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Curvature Strength";
			// 
			// checkBoxEnableCorrection
			// 
			this.checkBoxEnableCorrection.AutoSize = true;
			this.checkBoxEnableCorrection.Checked = true;
			this.checkBoxEnableCorrection.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxEnableCorrection.Location = new System.Drawing.Point(15, 770);
			this.checkBoxEnableCorrection.Name = "checkBoxEnableCorrection";
			this.checkBoxEnableCorrection.Size = new System.Drawing.Size(159, 17);
			this.checkBoxEnableCorrection.TabIndex = 2;
			this.checkBoxEnableCorrection.Text = "Enable Curvature Correction";
			this.checkBoxEnableCorrection.UseVisualStyleBackColor = true;
			// 
			// labelResult
			// 
			this.labelResult.AutoSize = true;
			this.labelResult.Location = new System.Drawing.Point(1298, 365);
			this.labelResult.Name = "labelResult";
			this.labelResult.Size = new System.Drawing.Size(31, 13);
			this.labelResult.TabIndex = 6;
			this.labelResult.Text = "aaaa";
			// 
			// floatTrackbarControlA
			// 
			this.floatTrackbarControlA.Location = new System.Drawing.Point(1361, 415);
			this.floatTrackbarControlA.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlA.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlA.Name = "floatTrackbarControlA";
			this.floatTrackbarControlA.RangeMax = 1000F;
			this.floatTrackbarControlA.RangeMin = -1000F;
			this.floatTrackbarControlA.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlA.TabIndex = 3;
			this.floatTrackbarControlA.Value = -1.154701F;
			this.floatTrackbarControlA.VisibleRangeMax = 0F;
			this.floatTrackbarControlA.VisibleRangeMin = -2F;
			this.floatTrackbarControlA.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlA_ValueChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(1308, 418);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(47, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Vertex A";
			// 
			// floatTrackbarControlB
			// 
			this.floatTrackbarControlB.Location = new System.Drawing.Point(1361, 441);
			this.floatTrackbarControlB.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlB.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlB.Name = "floatTrackbarControlB";
			this.floatTrackbarControlB.RangeMax = 1000F;
			this.floatTrackbarControlB.RangeMin = -1000F;
			this.floatTrackbarControlB.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlB.TabIndex = 3;
			this.floatTrackbarControlB.Value = -1.154701F;
			this.floatTrackbarControlB.VisibleRangeMax = 0F;
			this.floatTrackbarControlB.VisibleRangeMin = -2F;
			this.floatTrackbarControlB.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlA_ValueChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(1308, 444);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(47, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Vertex B";
			// 
			// floatTrackbarControlC
			// 
			this.floatTrackbarControlC.Location = new System.Drawing.Point(1361, 467);
			this.floatTrackbarControlC.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlC.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlC.Name = "floatTrackbarControlC";
			this.floatTrackbarControlC.RangeMax = 1000F;
			this.floatTrackbarControlC.RangeMin = -1000F;
			this.floatTrackbarControlC.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlC.TabIndex = 3;
			this.floatTrackbarControlC.Value = -1.154701F;
			this.floatTrackbarControlC.VisibleRangeMax = 0F;
			this.floatTrackbarControlC.VisibleRangeMin = -2F;
			this.floatTrackbarControlC.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlA_ValueChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(1308, 470);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(47, 13);
			this.label4.TabIndex = 4;
			this.label4.Text = "Vertex C";
			// 
			// labelMeshInfo
			// 
			this.labelMeshInfo.AutoSize = true;
			this.labelMeshInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelMeshInfo.Location = new System.Drawing.Point(569, 745);
			this.labelMeshInfo.Name = "labelMeshInfo";
			this.labelMeshInfo.Size = new System.Drawing.Size(33, 15);
			this.labelMeshInfo.TabIndex = 6;
			this.labelMeshInfo.Text = "aaaa";
			// 
			// floatTrackbarControlD
			// 
			this.floatTrackbarControlD.Location = new System.Drawing.Point(1361, 493);
			this.floatTrackbarControlD.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlD.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlD.Name = "floatTrackbarControlD";
			this.floatTrackbarControlD.RangeMax = 1000F;
			this.floatTrackbarControlD.RangeMin = -1000F;
			this.floatTrackbarControlD.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlD.TabIndex = 3;
			this.floatTrackbarControlD.Value = -1.154701F;
			this.floatTrackbarControlD.VisibleRangeMax = 0F;
			this.floatTrackbarControlD.VisibleRangeMin = -2F;
			this.floatTrackbarControlD.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlA_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(1308, 496);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(48, 13);
			this.label5.TabIndex = 4;
			this.label5.Text = "Vertex D";
			// 
			// panelOutputGraph
			// 
			this.panelOutputGraph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelOutputGraph.Location = new System.Drawing.Point(1298, 12);
			this.panelOutputGraph.Name = "panelOutputGraph";
			this.panelOutputGraph.Size = new System.Drawing.Size(350, 350);
			this.panelOutputGraph.TabIndex = 5;
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1280, 720);
			this.panelOutput.TabIndex = 0;
			// 
			// TestTriangleCurvatureForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1664, 815);
			this.Controls.Add(this.labelMeshInfo);
			this.Controls.Add(this.labelResult);
			this.Controls.Add(this.panelOutputGraph);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlD);
			this.Controls.Add(this.floatTrackbarControlC);
			this.Controls.Add(this.floatTrackbarControlB);
			this.Controls.Add(this.floatTrackbarControlA);
			this.Controls.Add(this.floatTrackbarControlCurvatureStrength);
			this.Controls.Add(this.checkBoxEnableCorrection);
			this.Controls.Add(this.checkBoxShowNormal);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput);
			this.Name = "TestTriangleCurvatureForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Triangle Curvature Test";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput3D panelOutput;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.CheckBox checkBoxShowNormal;
		private UIUtility.FloatTrackbarControl floatTrackbarControlCurvatureStrength;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkBoxEnableCorrection;
		private PanelOutput panelOutputGraph;
		private System.Windows.Forms.Label labelResult;
		private UIUtility.FloatTrackbarControl floatTrackbarControlA;
		private System.Windows.Forms.Label label2;
		private UIUtility.FloatTrackbarControl floatTrackbarControlB;
		private System.Windows.Forms.Label label3;
		private UIUtility.FloatTrackbarControl floatTrackbarControlC;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label labelMeshInfo;
		private UIUtility.FloatTrackbarControl floatTrackbarControlD;
		private System.Windows.Forms.Label label5;
	}
}

