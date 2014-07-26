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
			this.panel1 = new System.Windows.Forms.Panel();
			this.radioButtonPos = new System.Windows.Forms.RadioButton();
			this.radioButtonAbs = new System.Windows.Forms.RadioButton();
			this.radioButtonNeg = new System.Windows.Forms.RadioButton();
			this.floatTrackbarControlOrientationPhi = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.floatTrackbarControlOrientationTheta = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlPositionZ = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlPositionX = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label4 = new System.Windows.Forms.Label();
			this.radioButtonMarchedLength = new System.Windows.Forms.RadioButton();
			this.checkBoxRenderVectors = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlVectorSize = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.checkBoxFullSurface = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlClipAbove = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.checkBoxClipAboveValue = new System.Windows.Forms.CheckBox();
			this.viewportPanel = new OfflineCloudRenderer.ViewportPanel(this.components);
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(1042, 603);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(94, 29);
			this.buttonReload.TabIndex = 1;
			this.buttonReload.Text = "Reload Shaders";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// floatTrackbarControlDebug0
			// 
			this.floatTrackbarControlDebug0.Location = new System.Drawing.Point(989, 655);
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
			this.label1.Location = new System.Drawing.Point(986, 639);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(39, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Debug";
			// 
			// floatTrackbarControlDebug1
			// 
			this.floatTrackbarControlDebug1.Location = new System.Drawing.Point(989, 681);
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
			this.floatTrackbarControlDebug2.Location = new System.Drawing.Point(989, 707);
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
			this.floatTrackbarControlDebug3.Location = new System.Drawing.Point(989, 733);
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
			this.buttonShootPhotons.Location = new System.Drawing.Point(1038, 222);
			this.buttonShootPhotons.Name = "buttonShootPhotons";
			this.buttonShootPhotons.Size = new System.Drawing.Size(93, 23);
			this.buttonShootPhotons.TabIndex = 4;
			this.buttonShootPhotons.Text = "Shoot Photons";
			this.buttonShootPhotons.UseVisualStyleBackColor = true;
			this.buttonShootPhotons.Click += new System.EventHandler(this.buttonShootPhotons_Click);
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(984, 251);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(200, 23);
			this.progressBar1.TabIndex = 5;
			// 
			// radioButtonExitPosition
			// 
			this.radioButtonExitPosition.AutoSize = true;
			this.radioButtonExitPosition.Location = new System.Drawing.Point(984, 280);
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
			this.radioButtonExitDirection.Location = new System.Drawing.Point(984, 303);
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
			this.radioButtonScatteringEventIndex.Location = new System.Drawing.Point(984, 326);
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
			this.radioButtonAccumFlux.Location = new System.Drawing.Point(984, 372);
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
			this.floatTrackbarControlFluxMultiplier.Location = new System.Drawing.Point(1003, 395);
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
			this.floatTrackbarControlCubeSize.Location = new System.Drawing.Point(984, 185);
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
			this.label2.Location = new System.Drawing.Point(981, 169);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Cube Size (m)";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.radioButtonPos);
			this.panel1.Controls.Add(this.radioButtonAbs);
			this.panel1.Controls.Add(this.radioButtonNeg);
			this.panel1.Location = new System.Drawing.Point(1149, 281);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(47, 72);
			this.panel1.TabIndex = 7;
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
			// floatTrackbarControlOrientationPhi
			// 
			this.floatTrackbarControlOrientationPhi.Location = new System.Drawing.Point(984, 123);
			this.floatTrackbarControlOrientationPhi.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlOrientationPhi.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlOrientationPhi.Name = "floatTrackbarControlOrientationPhi";
			this.floatTrackbarControlOrientationPhi.RangeMax = 180F;
			this.floatTrackbarControlOrientationPhi.RangeMin = -180F;
			this.floatTrackbarControlOrientationPhi.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlOrientationPhi.TabIndex = 2;
			this.floatTrackbarControlOrientationPhi.Value = 0F;
			this.floatTrackbarControlOrientationPhi.VisibleRangeMax = 180F;
			this.floatTrackbarControlOrientationPhi.VisibleRangeMin = -180F;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(981, 81);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(93, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Orientation Angles";
			// 
			// floatTrackbarControlOrientationTheta
			// 
			this.floatTrackbarControlOrientationTheta.Location = new System.Drawing.Point(984, 97);
			this.floatTrackbarControlOrientationTheta.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlOrientationTheta.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlOrientationTheta.Name = "floatTrackbarControlOrientationTheta";
			this.floatTrackbarControlOrientationTheta.RangeMax = 90F;
			this.floatTrackbarControlOrientationTheta.RangeMin = 0F;
			this.floatTrackbarControlOrientationTheta.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlOrientationTheta.TabIndex = 2;
			this.floatTrackbarControlOrientationTheta.Value = 0F;
			this.floatTrackbarControlOrientationTheta.VisibleRangeMax = 90F;
			// 
			// floatTrackbarControlPositionZ
			// 
			this.floatTrackbarControlPositionZ.Location = new System.Drawing.Point(984, 54);
			this.floatTrackbarControlPositionZ.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlPositionZ.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlPositionZ.Name = "floatTrackbarControlPositionZ";
			this.floatTrackbarControlPositionZ.RangeMax = 1F;
			this.floatTrackbarControlPositionZ.RangeMin = -1F;
			this.floatTrackbarControlPositionZ.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlPositionZ.TabIndex = 2;
			this.floatTrackbarControlPositionZ.Value = 0F;
			this.floatTrackbarControlPositionZ.VisibleRangeMax = 1F;
			this.floatTrackbarControlPositionZ.VisibleRangeMin = -1F;
			// 
			// floatTrackbarControlPositionX
			// 
			this.floatTrackbarControlPositionX.Location = new System.Drawing.Point(984, 28);
			this.floatTrackbarControlPositionX.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlPositionX.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlPositionX.Name = "floatTrackbarControlPositionX";
			this.floatTrackbarControlPositionX.RangeMax = 1F;
			this.floatTrackbarControlPositionX.RangeMin = -1F;
			this.floatTrackbarControlPositionX.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlPositionX.TabIndex = 2;
			this.floatTrackbarControlPositionX.Value = 0F;
			this.floatTrackbarControlPositionX.VisibleRangeMax = 1F;
			this.floatTrackbarControlPositionX.VisibleRangeMin = -1F;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(981, 12);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(44, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Position";
			// 
			// radioButtonMarchedLength
			// 
			this.radioButtonMarchedLength.AutoSize = true;
			this.radioButtonMarchedLength.Location = new System.Drawing.Point(984, 349);
			this.radioButtonMarchedLength.Name = "radioButtonMarchedLength";
			this.radioButtonMarchedLength.Size = new System.Drawing.Size(103, 17);
			this.radioButtonMarchedLength.TabIndex = 6;
			this.radioButtonMarchedLength.Text = "Marched Length";
			this.radioButtonMarchedLength.UseVisualStyleBackColor = true;
			this.radioButtonMarchedLength.CheckedChanged += new System.EventHandler(this.radioButtonExitPosition_CheckedChanged);
			// 
			// checkBoxRenderVectors
			// 
			this.checkBoxRenderVectors.AutoSize = true;
			this.checkBoxRenderVectors.Location = new System.Drawing.Point(984, 451);
			this.checkBoxRenderVectors.Name = "checkBoxRenderVectors";
			this.checkBoxRenderVectors.Size = new System.Drawing.Size(100, 17);
			this.checkBoxRenderVectors.TabIndex = 8;
			this.checkBoxRenderVectors.Text = "Render Vectors";
			this.checkBoxRenderVectors.UseVisualStyleBackColor = true;
			this.checkBoxRenderVectors.CheckedChanged += new System.EventHandler(this.checkBoxRenderVectors_CheckedChanged);
			// 
			// floatTrackbarControlVectorSize
			// 
			this.floatTrackbarControlVectorSize.Location = new System.Drawing.Point(1003, 474);
			this.floatTrackbarControlVectorSize.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlVectorSize.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlVectorSize.Name = "floatTrackbarControlVectorSize";
			this.floatTrackbarControlVectorSize.RangeMin = 0F;
			this.floatTrackbarControlVectorSize.Size = new System.Drawing.Size(181, 20);
			this.floatTrackbarControlVectorSize.TabIndex = 2;
			this.floatTrackbarControlVectorSize.Value = 1F;
			this.floatTrackbarControlVectorSize.VisibleRangeMax = 2F;
			this.floatTrackbarControlVectorSize.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlVectorSize_ValueChanged);
			// 
			// checkBoxFullSurface
			// 
			this.checkBoxFullSurface.AutoSize = true;
			this.checkBoxFullSurface.Location = new System.Drawing.Point(1036, 11);
			this.checkBoxFullSurface.Name = "checkBoxFullSurface";
			this.checkBoxFullSurface.Size = new System.Drawing.Size(82, 17);
			this.checkBoxFullSurface.TabIndex = 8;
			this.checkBoxFullSurface.Text = "Full Surface";
			this.checkBoxFullSurface.UseVisualStyleBackColor = true;
			this.checkBoxFullSurface.CheckedChanged += new System.EventHandler(this.checkBoxFullSurface_CheckedChanged);
			// 
			// floatTrackbarControlClipAbove
			// 
			this.floatTrackbarControlClipAbove.Location = new System.Drawing.Point(1003, 523);
			this.floatTrackbarControlClipAbove.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlClipAbove.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlClipAbove.Name = "floatTrackbarControlClipAbove";
			this.floatTrackbarControlClipAbove.RangeMin = 0F;
			this.floatTrackbarControlClipAbove.Size = new System.Drawing.Size(181, 20);
			this.floatTrackbarControlClipAbove.TabIndex = 2;
			this.floatTrackbarControlClipAbove.Value = 1F;
			this.floatTrackbarControlClipAbove.VisibleRangeMax = 2F;
			this.floatTrackbarControlClipAbove.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlClipAbove_ValueChanged);
			// 
			// checkBoxClipAboveValue
			// 
			this.checkBoxClipAboveValue.AutoSize = true;
			this.checkBoxClipAboveValue.Location = new System.Drawing.Point(984, 500);
			this.checkBoxClipAboveValue.Name = "checkBoxClipAboveValue";
			this.checkBoxClipAboveValue.Size = new System.Drawing.Size(143, 17);
			this.checkBoxClipAboveValue.TabIndex = 8;
			this.checkBoxClipAboveValue.Text = "Clip above value (x 0.01)";
			this.checkBoxClipAboveValue.UseVisualStyleBackColor = true;
			this.checkBoxClipAboveValue.CheckedChanged += new System.EventHandler(this.checkBoxClipAboveValue_CheckedChanged);
			// 
			// viewportPanel
			// 
			this.viewportPanel.Device = null;
			this.viewportPanel.Location = new System.Drawing.Point(12, 12);
			this.viewportPanel.Name = "viewportPanel";
			this.viewportPanel.Size = new System.Drawing.Size(963, 686);
			this.viewportPanel.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1200, 756);
			this.Controls.Add(this.checkBoxFullSurface);
			this.Controls.Add(this.checkBoxClipAboveValue);
			this.Controls.Add(this.checkBoxRenderVectors);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.radioButtonAccumFlux);
			this.Controls.Add(this.radioButtonMarchedLength);
			this.Controls.Add(this.radioButtonScatteringEventIndex);
			this.Controls.Add(this.radioButtonExitDirection);
			this.Controls.Add(this.radioButtonExitPosition);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.buttonShootPhotons);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.floatTrackbarControlPositionX);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.floatTrackbarControlPositionZ);
			this.Controls.Add(this.floatTrackbarControlOrientationTheta);
			this.Controls.Add(this.floatTrackbarControlOrientationPhi);
			this.Controls.Add(this.floatTrackbarControlDebug3);
			this.Controls.Add(this.floatTrackbarControlDebug2);
			this.Controls.Add(this.floatTrackbarControlDebug1);
			this.Controls.Add(this.floatTrackbarControlCubeSize);
			this.Controls.Add(this.floatTrackbarControlClipAbove);
			this.Controls.Add(this.floatTrackbarControlVectorSize);
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
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlOrientationPhi;
		private System.Windows.Forms.Label label3;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlOrientationTheta;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlPositionZ;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlPositionX;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.RadioButton radioButtonMarchedLength;
		private System.Windows.Forms.CheckBox checkBoxRenderVectors;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlVectorSize;
		private System.Windows.Forms.CheckBox checkBoxFullSurface;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlClipAbove;
		private System.Windows.Forms.CheckBox checkBoxClipAboveValue;
	}
}

