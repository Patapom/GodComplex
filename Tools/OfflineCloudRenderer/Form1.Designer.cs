namespace OfflineCloudRenderer
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
			this.floatTrackbarControlDebug0 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlDebug1 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlDebug2 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlDebug3 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.buttonShootPhotons = new System.Windows.Forms.Button();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.radioButtonExitPosition = new System.Windows.Forms.RadioButton();
			this.radioButtonExitDirection = new System.Windows.Forms.RadioButton();
			this.radioButtonScatteringEventIndex = new System.Windows.Forms.RadioButton();
			this.radioButtonAccumFlux = new System.Windows.Forms.RadioButton();
			this.floatTrackbarControlFluxMultiplier = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlCubeSize = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.viewportPanel = new OfflineCloudRenderer.ViewportPanel(this.components);
			this.panel1 = new System.Windows.Forms.Panel();
			this.radioButtonAbs = new System.Windows.Forms.RadioButton();
			this.radioButtonNeg = new System.Windows.Forms.RadioButton();
			this.radioButtonPos = new System.Windows.Forms.RadioButton();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(1027, 41);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(94, 29);
			this.buttonReload.TabIndex = 1;
			this.buttonReload.Text = "Reload Shaders";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// floatTrackbarControlDebug0
			// 
			this.floatTrackbarControlDebug0.Location = new System.Drawing.Point(988, 135);
			this.floatTrackbarControlDebug0.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDebug0.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDebug0.Name = "floatTrackbarControlDebug0";
			this.floatTrackbarControlDebug0.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDebug0.TabIndex = 2;
			this.floatTrackbarControlDebug0.Value = 0F;
			this.floatTrackbarControlDebug0.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlDebug3_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(985, 119);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(39, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Debug";
			// 
			// floatTrackbarControlDebug1
			// 
			this.floatTrackbarControlDebug1.Location = new System.Drawing.Point(988, 161);
			this.floatTrackbarControlDebug1.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDebug1.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDebug1.Name = "floatTrackbarControlDebug1";
			this.floatTrackbarControlDebug1.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDebug1.TabIndex = 2;
			this.floatTrackbarControlDebug1.Value = 0F;
			this.floatTrackbarControlDebug1.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlDebug3_ValueChanged);
			// 
			// floatTrackbarControlDebug2
			// 
			this.floatTrackbarControlDebug2.Location = new System.Drawing.Point(988, 187);
			this.floatTrackbarControlDebug2.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDebug2.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDebug2.Name = "floatTrackbarControlDebug2";
			this.floatTrackbarControlDebug2.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDebug2.TabIndex = 2;
			this.floatTrackbarControlDebug2.Value = 0F;
			this.floatTrackbarControlDebug2.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlDebug3_ValueChanged);
			// 
			// floatTrackbarControlDebug3
			// 
			this.floatTrackbarControlDebug3.Location = new System.Drawing.Point(988, 213);
			this.floatTrackbarControlDebug3.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlDebug3.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlDebug3.Name = "floatTrackbarControlDebug3";
			this.floatTrackbarControlDebug3.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlDebug3.TabIndex = 2;
			this.floatTrackbarControlDebug3.Value = 0F;
			this.floatTrackbarControlDebug3.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlDebug3_ValueChanged);
			// 
			// buttonShootPhotons
			// 
			this.buttonShootPhotons.Location = new System.Drawing.Point(1042, 421);
			this.buttonShootPhotons.Name = "buttonShootPhotons";
			this.buttonShootPhotons.Size = new System.Drawing.Size(93, 23);
			this.buttonShootPhotons.TabIndex = 4;
			this.buttonShootPhotons.Text = "Shoot Photons";
			this.buttonShootPhotons.UseVisualStyleBackColor = true;
			this.buttonShootPhotons.Click += new System.EventHandler(this.buttonShootPhotons_Click);
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(988, 450);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(200, 23);
			this.progressBar1.TabIndex = 5;
			// 
			// radioButtonExitPosition
			// 
			this.radioButtonExitPosition.AutoSize = true;
			this.radioButtonExitPosition.Location = new System.Drawing.Point(988, 479);
			this.radioButtonExitPosition.Name = "radioButtonExitPosition";
			this.radioButtonExitPosition.Size = new System.Drawing.Size(109, 17);
			this.radioButtonExitPosition.TabIndex = 6;
			this.radioButtonExitPosition.Text = "Splat Exit Position";
			this.radioButtonExitPosition.UseVisualStyleBackColor = true;
			this.radioButtonExitPosition.CheckedChanged += new System.EventHandler(this.radioButtonExitPosition_CheckedChanged);
			// 
			// radioButtonExitDirection
			// 
			this.radioButtonExitDirection.AutoSize = true;
			this.radioButtonExitDirection.Location = new System.Drawing.Point(988, 502);
			this.radioButtonExitDirection.Name = "radioButtonExitDirection";
			this.radioButtonExitDirection.Size = new System.Drawing.Size(114, 17);
			this.radioButtonExitDirection.TabIndex = 6;
			this.radioButtonExitDirection.Text = "Splat Exit Direction";
			this.radioButtonExitDirection.UseVisualStyleBackColor = true;
			this.radioButtonExitDirection.CheckedChanged += new System.EventHandler(this.radioButtonExitPosition_CheckedChanged);
			// 
			// radioButtonScatteringEventIndex
			// 
			this.radioButtonScatteringEventIndex.AutoSize = true;
			this.radioButtonScatteringEventIndex.Location = new System.Drawing.Point(988, 525);
			this.radioButtonScatteringEventIndex.Name = "radioButtonScatteringEventIndex";
			this.radioButtonScatteringEventIndex.Size = new System.Drawing.Size(160, 17);
			this.radioButtonScatteringEventIndex.TabIndex = 6;
			this.radioButtonScatteringEventIndex.Text = "Splat Scattering Event Index";
			this.radioButtonScatteringEventIndex.UseVisualStyleBackColor = true;
			this.radioButtonScatteringEventIndex.CheckedChanged += new System.EventHandler(this.radioButtonExitPosition_CheckedChanged);
			// 
			// radioButtonAccumFlux
			// 
			this.radioButtonAccumFlux.AutoSize = true;
			this.radioButtonAccumFlux.Checked = true;
			this.radioButtonAccumFlux.Location = new System.Drawing.Point(988, 548);
			this.radioButtonAccumFlux.Name = "radioButtonAccumFlux";
			this.radioButtonAccumFlux.Size = new System.Drawing.Size(147, 17);
			this.radioButtonAccumFlux.TabIndex = 6;
			this.radioButtonAccumFlux.TabStop = true;
			this.radioButtonAccumFlux.Text = "Accumulated Flux (x1000)";
			this.radioButtonAccumFlux.UseVisualStyleBackColor = true;
			this.radioButtonAccumFlux.CheckedChanged += new System.EventHandler(this.radioButtonExitPosition_CheckedChanged);
			// 
			// floatTrackbarControlFluxMultiplier
			// 
			this.floatTrackbarControlFluxMultiplier.Location = new System.Drawing.Point(1007, 571);
			this.floatTrackbarControlFluxMultiplier.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlFluxMultiplier.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlFluxMultiplier.Name = "floatTrackbarControlFluxMultiplier";
			this.floatTrackbarControlFluxMultiplier.RangeMin = 0F;
			this.floatTrackbarControlFluxMultiplier.Size = new System.Drawing.Size(181, 20);
			this.floatTrackbarControlFluxMultiplier.TabIndex = 2;
			this.floatTrackbarControlFluxMultiplier.Value = 100F;
			this.floatTrackbarControlFluxMultiplier.VisibleRangeMax = 400F;
			this.floatTrackbarControlFluxMultiplier.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlFluxMultiplier_ValueChanged);
			// 
			// floatTrackbarControlCubeSize
			// 
			this.floatTrackbarControlCubeSize.Location = new System.Drawing.Point(988, 384);
			this.floatTrackbarControlCubeSize.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlCubeSize.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlCubeSize.Name = "floatTrackbarControlCubeSize";
			this.floatTrackbarControlCubeSize.RangeMin = 1F;
			this.floatTrackbarControlCubeSize.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlCubeSize.TabIndex = 2;
			this.floatTrackbarControlCubeSize.Value = 100F;
			this.floatTrackbarControlCubeSize.VisibleRangeMax = 1000F;
			this.floatTrackbarControlCubeSize.VisibleRangeMin = 1F;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(985, 368);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Cube Size (m)";
			// 
			// viewportPanel
			// 
			this.viewportPanel.Device = null;
			this.viewportPanel.Location = new System.Drawing.Point(12, 12);
			this.viewportPanel.Name = "viewportPanel";
			this.viewportPanel.Size = new System.Drawing.Size(963, 686);
			this.viewportPanel.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.radioButtonPos);
			this.panel1.Controls.Add(this.radioButtonAbs);
			this.panel1.Controls.Add(this.radioButtonNeg);
			this.panel1.Location = new System.Drawing.Point(1153, 480);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(47, 72);
			this.panel1.TabIndex = 7;
			// 
			// radioButtonAbs
			// 
			this.radioButtonAbs.AutoSize = true;
			this.radioButtonAbs.Checked = true;
			this.radioButtonAbs.Location = new System.Drawing.Point(2, 0);
			this.radioButtonAbs.Name = "radioButtonAbs";
			this.radioButtonAbs.Size = new System.Drawing.Size(43, 17);
			this.radioButtonAbs.TabIndex = 6;
			this.radioButtonAbs.TabStop = true;
			this.radioButtonAbs.Text = "Abs";
			this.radioButtonAbs.UseVisualStyleBackColor = true;
			this.radioButtonAbs.CheckedChanged += new System.EventHandler(this.radioButtonExitPosition_CheckedChanged);
			// 
			// radioButtonNeg
			// 
			this.radioButtonNeg.AutoSize = true;
			this.radioButtonNeg.Location = new System.Drawing.Point(2, 46);
			this.radioButtonNeg.Name = "radioButtonNeg";
			this.radioButtonNeg.Size = new System.Drawing.Size(45, 17);
			this.radioButtonNeg.TabIndex = 6;
			this.radioButtonNeg.Text = "Neg";
			this.radioButtonNeg.UseVisualStyleBackColor = true;
			this.radioButtonNeg.CheckedChanged += new System.EventHandler(this.radioButtonExitPosition_CheckedChanged);
			// 
			// radioButtonPos
			// 
			this.radioButtonPos.AutoSize = true;
			this.radioButtonPos.Location = new System.Drawing.Point(2, 23);
			this.radioButtonPos.Name = "radioButtonPos";
			this.radioButtonPos.Size = new System.Drawing.Size(43, 17);
			this.radioButtonPos.TabIndex = 6;
			this.radioButtonPos.Text = "Pos";
			this.radioButtonPos.UseVisualStyleBackColor = true;
			this.radioButtonPos.CheckedChanged += new System.EventHandler(this.radioButtonExitPosition_CheckedChanged);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1200, 756);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.radioButtonAccumFlux);
			this.Controls.Add(this.radioButtonScatteringEventIndex);
			this.Controls.Add(this.radioButtonExitDirection);
			this.Controls.Add(this.radioButtonExitPosition);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.buttonShootPhotons);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlDebug3);
			this.Controls.Add(this.floatTrackbarControlDebug2);
			this.Controls.Add(this.floatTrackbarControlDebug1);
			this.Controls.Add(this.floatTrackbarControlCubeSize);
			this.Controls.Add(this.floatTrackbarControlFluxMultiplier);
			this.Controls.Add(this.floatTrackbarControlDebug0);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.viewportPanel);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ViewportPanel viewportPanel;
		private System.Windows.Forms.Button buttonReload;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDebug0;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDebug1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDebug2;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlDebug3;
		private System.Windows.Forms.Button buttonShootPhotons;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.RadioButton radioButtonExitPosition;
		private System.Windows.Forms.RadioButton radioButtonExitDirection;
		private System.Windows.Forms.RadioButton radioButtonScatteringEventIndex;
		private System.Windows.Forms.RadioButton radioButtonAccumFlux;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlFluxMultiplier;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlCubeSize;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.RadioButton radioButtonAbs;
		private System.Windows.Forms.RadioButton radioButtonNeg;
		private System.Windows.Forms.RadioButton radioButtonPos;
	}
}

