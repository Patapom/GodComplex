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
			this.floatTrackbarControlCurvatureStrength = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.checkBoxEnableCorrection = new System.Windows.Forms.CheckBox();
			this.panelOutput = new TriangleCurvature.PanelOutput3D(this.components);
			this.panelOutputGraph = new TriangleCurvature.PanelOutput(this.components);
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
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1280, 720);
			this.panelOutput.TabIndex = 0;
			// 
			// panelOutputGraph
			// 
			this.panelOutputGraph.Location = new System.Drawing.Point(1298, 12);
			this.panelOutputGraph.Name = "panelOutputGraph";
			this.panelOutputGraph.Size = new System.Drawing.Size(350, 350);
			this.panelOutputGraph.TabIndex = 5;
			// 
			// TestTriangleCurvatureForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1664, 815);
			this.Controls.Add(this.panelOutputGraph);
			this.Controls.Add(this.label1);
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
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlCurvatureStrength;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkBoxEnableCorrection;
		private PanelOutput panelOutputGraph;
	}
}

