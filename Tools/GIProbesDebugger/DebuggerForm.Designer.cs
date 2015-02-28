namespace GIProbesDebugger
{
	partial class DebuggerForm
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
				m_Device.Dispose();
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
			this.floatTrackbarControlProjectionDiffusion = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.buttonReload = new System.Windows.Forms.Button();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.buttonLoadProbe = new System.Windows.Forms.Button();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.integerTrackbarControlDisplayType = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.checkBoxShowCubeMapFaces = new System.Windows.Forms.CheckBox();
			this.checkBoxShowDistance = new System.Windows.Forms.CheckBox();
			this.panelOutput = new GIProbesDebugger.PanelOutput(this.components);
			this.checkBoxShowWSPosition = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// floatTrackbarControlProjectionDiffusion
			// 
			this.floatTrackbarControlProjectionDiffusion.Location = new System.Drawing.Point(1147, 229);
			this.floatTrackbarControlProjectionDiffusion.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlProjectionDiffusion.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlProjectionDiffusion.Name = "floatTrackbarControlProjectionDiffusion";
			this.floatTrackbarControlProjectionDiffusion.RangeMax = 1F;
			this.floatTrackbarControlProjectionDiffusion.RangeMin = 0F;
			this.floatTrackbarControlProjectionDiffusion.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlProjectionDiffusion.TabIndex = 1;
			this.floatTrackbarControlProjectionDiffusion.Value = 0F;
			this.floatTrackbarControlProjectionDiffusion.VisibleRangeMax = 1F;
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(1272, 629);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 2;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = 10;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(1043, 234);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(98, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Projection Diffusion";
			// 
			// buttonLoadProbe
			// 
			this.buttonLoadProbe.Location = new System.Drawing.Point(1042, 9);
			this.buttonLoadProbe.Name = "buttonLoadProbe";
			this.buttonLoadProbe.Size = new System.Drawing.Size(88, 23);
			this.buttonLoadProbe.TabIndex = 0;
			this.buttonLoadProbe.Text = "Load Probe";
			this.buttonLoadProbe.UseVisualStyleBackColor = true;
			this.buttonLoadProbe.Click += new System.EventHandler(this.buttonLoadProbe_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "*.probepixels";
			this.openFileDialog.Filter = "Probe Pixel Files (*.probepixels)|*.probepixels|All Files|*.*";
			// 
			// integerTrackbarControlDisplayType
			// 
			this.integerTrackbarControlDisplayType.Location = new System.Drawing.Point(1119, 47);
			this.integerTrackbarControlDisplayType.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlDisplayType.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlDisplayType.Name = "integerTrackbarControlDisplayType";
			this.integerTrackbarControlDisplayType.RangeMax = 7;
			this.integerTrackbarControlDisplayType.RangeMin = 0;
			this.integerTrackbarControlDisplayType.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlDisplayType.TabIndex = 4;
			this.integerTrackbarControlDisplayType.Value = 0;
			this.integerTrackbarControlDisplayType.VisibleRangeMax = 7;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(1045, 49);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(68, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Display Type";
			// 
			// checkBoxShowCubeMapFaces
			// 
			this.checkBoxShowCubeMapFaces.AutoSize = true;
			this.checkBoxShowCubeMapFaces.Location = new System.Drawing.Point(1050, 86);
			this.checkBoxShowCubeMapFaces.Name = "checkBoxShowCubeMapFaces";
			this.checkBoxShowCubeMapFaces.Size = new System.Drawing.Size(132, 17);
			this.checkBoxShowCubeMapFaces.TabIndex = 5;
			this.checkBoxShowCubeMapFaces.Text = "Show cube map faces";
			this.checkBoxShowCubeMapFaces.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowDistance
			// 
			this.checkBoxShowDistance.AutoSize = true;
			this.checkBoxShowDistance.Location = new System.Drawing.Point(1050, 109);
			this.checkBoxShowDistance.Name = "checkBoxShowDistance";
			this.checkBoxShowDistance.Size = new System.Drawing.Size(96, 17);
			this.checkBoxShowDistance.TabIndex = 5;
			this.checkBoxShowDistance.Text = "Show distance";
			this.checkBoxShowDistance.UseVisualStyleBackColor = true;
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1024, 640);
			this.panelOutput.TabIndex = 0;
			// 
			// checkBoxShowWSPosition
			// 
			this.checkBoxShowWSPosition.AutoSize = true;
			this.checkBoxShowWSPosition.Location = new System.Drawing.Point(1050, 132);
			this.checkBoxShowWSPosition.Name = "checkBoxShowWSPosition";
			this.checkBoxShowWSPosition.Size = new System.Drawing.Size(152, 17);
			this.checkBoxShowWSPosition.TabIndex = 5;
			this.checkBoxShowWSPosition.Text = "Show world-space position";
			this.checkBoxShowWSPosition.UseVisualStyleBackColor = true;
			// 
			// DebuggerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1359, 663);
			this.Controls.Add(this.checkBoxShowWSPosition);
			this.Controls.Add(this.checkBoxShowDistance);
			this.Controls.Add(this.checkBoxShowCubeMapFaces);
			this.Controls.Add(this.integerTrackbarControlDisplayType);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonLoadProbe);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.floatTrackbarControlProjectionDiffusion);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DebuggerForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "GI Probes Debugger";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelOutput;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlProjectionDiffusion;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonLoadProbe;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlDisplayType;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox checkBoxShowCubeMapFaces;
		private System.Windows.Forms.CheckBox checkBoxShowDistance;
		private System.Windows.Forms.CheckBox checkBoxShowWSPosition;
	}
}

