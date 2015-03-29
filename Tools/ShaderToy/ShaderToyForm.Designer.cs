namespace ShaderToy
{
	partial class ShaderToyForm
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
			this.floatTrackbarControlWeightMultiplier = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.buttonReload = new System.Windows.Forms.Button();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.checkBoxShowWeights = new System.Windows.Forms.CheckBox();
			this.checkBoxSmoothStep = new System.Windows.Forms.CheckBox();
			this.panelOutput = new ShaderToy.PanelOutput(this.components);
			this.SuspendLayout();
			// 
			// floatTrackbarControlWeightMultiplier
			// 
			this.floatTrackbarControlWeightMultiplier.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlWeightMultiplier.Location = new System.Drawing.Point(1020, 61);
			this.floatTrackbarControlWeightMultiplier.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlWeightMultiplier.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlWeightMultiplier.Name = "floatTrackbarControlWeightMultiplier";
			this.floatTrackbarControlWeightMultiplier.RangeMax = 10F;
			this.floatTrackbarControlWeightMultiplier.RangeMin = 0F;
			this.floatTrackbarControlWeightMultiplier.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlWeightMultiplier.TabIndex = 1;
			this.floatTrackbarControlWeightMultiplier.Value = 1F;
			this.floatTrackbarControlWeightMultiplier.VisibleRangeMax = 1F;
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(1020, 631);
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
			// checkBoxShowWeights
			// 
			this.checkBoxShowWeights.AutoSize = true;
			this.checkBoxShowWeights.Location = new System.Drawing.Point(1020, 12);
			this.checkBoxShowWeights.Name = "checkBoxShowWeights";
			this.checkBoxShowWeights.Size = new System.Drawing.Size(92, 17);
			this.checkBoxShowWeights.TabIndex = 3;
			this.checkBoxShowWeights.Text = "Show weights";
			this.checkBoxShowWeights.UseVisualStyleBackColor = true;
			// 
			// checkBoxSmoothStep
			// 
			this.checkBoxSmoothStep.AutoSize = true;
			this.checkBoxSmoothStep.Location = new System.Drawing.Point(1020, 35);
			this.checkBoxSmoothStep.Name = "checkBoxSmoothStep";
			this.checkBoxSmoothStep.Size = new System.Drawing.Size(125, 17);
			this.checkBoxSmoothStep.TabIndex = 3;
			this.checkBoxSmoothStep.Text = "Use smooth minimum";
			this.checkBoxSmoothStep.UseVisualStyleBackColor = true;
			// 
			// panelOutput
			// 
			this.panelOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1002, 642);
			this.panelOutput.TabIndex = 0;
			this.panelOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseDown);
			this.panelOutput.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseMove);
			this.panelOutput.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelOutput_MouseUp);
			// 
			// ShaderToyForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1232, 666);
			this.Controls.Add(this.checkBoxSmoothStep);
			this.Controls.Add(this.checkBoxShowWeights);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.floatTrackbarControlWeightMultiplier);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "ShaderToyForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ShaderToy";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelOutput;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlWeightMultiplier;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.CheckBox checkBoxShowWeights;
		private System.Windows.Forms.CheckBox checkBoxSmoothStep;
	}
}

