namespace TestMSBSDF
{
	partial class TestForm
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
			this.panelOutput = new System.Windows.Forms.Panel();
			this.timer1 = new System.Windows.Forms.Timer( this.components );
			this.buttonReload = new System.Windows.Forms.Button();
			this.floatTrackbarControlBeckmannRoughness = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label1 = new System.Windows.Forms.Label();
			this.checkBoxShowNormals = new System.Windows.Forms.RadioButton();
			this.floatTrackbarControlTheta = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonRayTrace = new System.Windows.Forms.Button();
			this.checkBoxShowOutgoingDirections = new System.Windows.Forms.RadioButton();
			this.integerTrackbarControlScatteringOrder = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.integerTrackbarControlIterationsCount = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.floatTrackbarControlPhi = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.checkBoxShowOutgoingDirectionsHistogram = new System.Windows.Forms.RadioButton();
			this.label5 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.radioButtonShowHeights = new System.Windows.Forms.RadioButton();
			this.groupBoxDisplay = new System.Windows.Forms.GroupBox();
			this.floatTrackbarControlBeckmannSizeFactor = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label6 = new System.Windows.Forms.Label();
			this.checkBoxShowLobe = new System.Windows.Forms.CheckBox();
			this.floatTrackbarControlLobeIntensity = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.radioButtonHideSurface = new System.Windows.Forms.RadioButton();
			this.panel1.SuspendLayout();
			this.groupBoxDisplay.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point( 12, 12 );
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size( 833, 630 );
			this.panelOutput.TabIndex = 0;
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 10;
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point( 984, 619 );
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size( 116, 23 );
			this.buttonReload.TabIndex = 1;
			this.buttonReload.Text = "Reload Shaders";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler( this.buttonReload_Click );
			// 
			// floatTrackbarControlBeckmannRoughness
			// 
			this.floatTrackbarControlBeckmannRoughness.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlBeckmannRoughness.Location = new System.Drawing.Point( 854, 28 );
			this.floatTrackbarControlBeckmannRoughness.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlBeckmannRoughness.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlBeckmannRoughness.Name = "floatTrackbarControlBeckmannRoughness";
			this.floatTrackbarControlBeckmannRoughness.RangeMax = 1F;
			this.floatTrackbarControlBeckmannRoughness.RangeMin = 0F;
			this.floatTrackbarControlBeckmannRoughness.Size = new System.Drawing.Size( 246, 20 );
			this.floatTrackbarControlBeckmannRoughness.TabIndex = 2;
			this.floatTrackbarControlBeckmannRoughness.Value = 0.8F;
			this.floatTrackbarControlBeckmannRoughness.VisibleRangeMax = 1F;
			this.floatTrackbarControlBeckmannRoughness.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlBeckmannRoughness_ValueChanged );
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point( 851, 12 );
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size( 155, 13 );
			this.label1.TabIndex = 3;
			this.label1.Text = "Beckmann Surface Roughness";
			// 
			// checkBoxShowNormals
			// 
			this.checkBoxShowNormals.AutoSize = true;
			this.checkBoxShowNormals.Location = new System.Drawing.Point( 0, 48 );
			this.checkBoxShowNormals.Name = "checkBoxShowNormals";
			this.checkBoxShowNormals.Size = new System.Drawing.Size( 93, 17 );
			this.checkBoxShowNormals.TabIndex = 4;
			this.checkBoxShowNormals.Text = "Show Normals";
			this.checkBoxShowNormals.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControlTheta
			// 
			this.floatTrackbarControlTheta.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlTheta.Location = new System.Drawing.Point( 854, 124 );
			this.floatTrackbarControlTheta.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlTheta.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlTheta.Name = "floatTrackbarControlTheta";
			this.floatTrackbarControlTheta.RangeMax = 89.9F;
			this.floatTrackbarControlTheta.RangeMin = 0F;
			this.floatTrackbarControlTheta.Size = new System.Drawing.Size( 246, 20 );
			this.floatTrackbarControlTheta.TabIndex = 5;
			this.floatTrackbarControlTheta.Value = 0F;
			this.floatTrackbarControlTheta.VisibleRangeMax = 89.9F;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point( 851, 108 );
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size( 111, 13 );
			this.label2.TabIndex = 3;
			this.label2.Text = "Incoming Angle Theta";
			// 
			// buttonRayTrace
			// 
			this.buttonRayTrace.Location = new System.Drawing.Point( 854, 228 );
			this.buttonRayTrace.Name = "buttonRayTrace";
			this.buttonRayTrace.Size = new System.Drawing.Size( 75, 23 );
			this.buttonRayTrace.TabIndex = 6;
			this.buttonRayTrace.Text = "Ray Trace";
			this.buttonRayTrace.UseVisualStyleBackColor = true;
			this.buttonRayTrace.Click += new System.EventHandler( this.buttonRayTrace_Click );
			// 
			// checkBoxShowOutgoingDirections
			// 
			this.checkBoxShowOutgoingDirections.AutoSize = true;
			this.checkBoxShowOutgoingDirections.Location = new System.Drawing.Point( 0, 94 );
			this.checkBoxShowOutgoingDirections.Name = "checkBoxShowOutgoingDirections";
			this.checkBoxShowOutgoingDirections.Size = new System.Drawing.Size( 148, 17 );
			this.checkBoxShowOutgoingDirections.TabIndex = 4;
			this.checkBoxShowOutgoingDirections.Text = "Show Outgoing Directions";
			this.checkBoxShowOutgoingDirections.UseVisualStyleBackColor = true;
			this.checkBoxShowOutgoingDirections.Visible = false;
			// 
			// integerTrackbarControlScatteringOrder
			// 
			this.integerTrackbarControlScatteringOrder.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.integerTrackbarControlScatteringOrder.Location = new System.Drawing.Point( 6, 183 );
			this.integerTrackbarControlScatteringOrder.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.integerTrackbarControlScatteringOrder.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.integerTrackbarControlScatteringOrder.Name = "integerTrackbarControlScatteringOrder";
			this.integerTrackbarControlScatteringOrder.RangeMax = 3;
			this.integerTrackbarControlScatteringOrder.RangeMin = 0;
			this.integerTrackbarControlScatteringOrder.Size = new System.Drawing.Size( 234, 20 );
			this.integerTrackbarControlScatteringOrder.TabIndex = 7;
			this.integerTrackbarControlScatteringOrder.Value = 0;
			this.integerTrackbarControlScatteringOrder.VisibleRangeMax = 3;
			// 
			// integerTrackbarControlIterationsCount
			// 
			this.integerTrackbarControlIterationsCount.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.integerTrackbarControlIterationsCount.Location = new System.Drawing.Point( 854, 202 );
			this.integerTrackbarControlIterationsCount.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.integerTrackbarControlIterationsCount.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.integerTrackbarControlIterationsCount.Name = "integerTrackbarControlIterationsCount";
			this.integerTrackbarControlIterationsCount.RangeMax = 2048;
			this.integerTrackbarControlIterationsCount.RangeMin = 1;
			this.integerTrackbarControlIterationsCount.Size = new System.Drawing.Size( 246, 20 );
			this.integerTrackbarControlIterationsCount.TabIndex = 7;
			this.integerTrackbarControlIterationsCount.Value = 1;
			this.integerTrackbarControlIterationsCount.VisibleRangeMax = 10;
			this.integerTrackbarControlIterationsCount.VisibleRangeMin = 1;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point( 851, 186 );
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size( 81, 13 );
			this.label3.TabIndex = 3;
			this.label3.Text = "Iterations Count";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point( 851, 147 );
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size( 98, 13 );
			this.label4.TabIndex = 3;
			this.label4.Text = "Incoming Angle Phi";
			// 
			// floatTrackbarControlPhi
			// 
			this.floatTrackbarControlPhi.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlPhi.Location = new System.Drawing.Point( 854, 163 );
			this.floatTrackbarControlPhi.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlPhi.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlPhi.Name = "floatTrackbarControlPhi";
			this.floatTrackbarControlPhi.RangeMax = 180F;
			this.floatTrackbarControlPhi.RangeMin = -180F;
			this.floatTrackbarControlPhi.Size = new System.Drawing.Size( 246, 20 );
			this.floatTrackbarControlPhi.TabIndex = 5;
			this.floatTrackbarControlPhi.Value = 0F;
			this.floatTrackbarControlPhi.VisibleRangeMax = 180F;
			this.floatTrackbarControlPhi.VisibleRangeMin = -180F;
			// 
			// checkBoxShowOutgoingDirectionsHistogram
			// 
			this.checkBoxShowOutgoingDirectionsHistogram.AutoSize = true;
			this.checkBoxShowOutgoingDirectionsHistogram.Location = new System.Drawing.Point( 0, 71 );
			this.checkBoxShowOutgoingDirectionsHistogram.Name = "checkBoxShowOutgoingDirectionsHistogram";
			this.checkBoxShowOutgoingDirectionsHistogram.Size = new System.Drawing.Size( 198, 17 );
			this.checkBoxShowOutgoingDirectionsHistogram.TabIndex = 4;
			this.checkBoxShowOutgoingDirectionsHistogram.Text = "Show Outgoing Directions Histogram";
			this.checkBoxShowOutgoingDirectionsHistogram.UseVisualStyleBackColor = true;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point( 3, 167 );
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size( 84, 13 );
			this.label5.TabIndex = 3;
			this.label5.Text = "Scattering Order";
			// 
			// panel1
			// 
			this.panel1.Controls.Add( this.radioButtonShowHeights );
			this.panel1.Controls.Add( this.radioButtonHideSurface );
			this.panel1.Controls.Add( this.checkBoxShowNormals );
			this.panel1.Controls.Add( this.checkBoxShowOutgoingDirections );
			this.panel1.Controls.Add( this.checkBoxShowOutgoingDirectionsHistogram );
			this.panel1.Location = new System.Drawing.Point( 6, 19 );
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size( 200, 128 );
			this.panel1.TabIndex = 8;
			// 
			// radioButtonShowHeights
			// 
			this.radioButtonShowHeights.AutoSize = true;
			this.radioButtonShowHeights.Checked = true;
			this.radioButtonShowHeights.Location = new System.Drawing.Point( 0, 25 );
			this.radioButtonShowHeights.Name = "radioButtonShowHeights";
			this.radioButtonShowHeights.Size = new System.Drawing.Size( 91, 17 );
			this.radioButtonShowHeights.TabIndex = 4;
			this.radioButtonShowHeights.TabStop = true;
			this.radioButtonShowHeights.Text = "Show Heights";
			this.radioButtonShowHeights.UseVisualStyleBackColor = true;
			// 
			// groupBoxDisplay
			// 
			this.groupBoxDisplay.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxDisplay.Controls.Add( this.checkBoxShowLobe );
			this.groupBoxDisplay.Controls.Add( this.panel1 );
			this.groupBoxDisplay.Controls.Add( this.label5 );
			this.groupBoxDisplay.Controls.Add( this.floatTrackbarControlLobeIntensity );
			this.groupBoxDisplay.Controls.Add( this.integerTrackbarControlScatteringOrder );
			this.groupBoxDisplay.Location = new System.Drawing.Point( 854, 304 );
			this.groupBoxDisplay.Name = "groupBoxDisplay";
			this.groupBoxDisplay.Size = new System.Drawing.Size( 246, 309 );
			this.groupBoxDisplay.TabIndex = 9;
			this.groupBoxDisplay.TabStop = false;
			this.groupBoxDisplay.Text = "Display";
			// 
			// floatTrackbarControlBeckmannSizeFactor
			// 
			this.floatTrackbarControlBeckmannSizeFactor.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlBeckmannSizeFactor.Location = new System.Drawing.Point( 854, 67 );
			this.floatTrackbarControlBeckmannSizeFactor.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlBeckmannSizeFactor.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlBeckmannSizeFactor.Name = "floatTrackbarControlBeckmannSizeFactor";
			this.floatTrackbarControlBeckmannSizeFactor.RangeMax = 10F;
			this.floatTrackbarControlBeckmannSizeFactor.RangeMin = 0F;
			this.floatTrackbarControlBeckmannSizeFactor.Size = new System.Drawing.Size( 246, 20 );
			this.floatTrackbarControlBeckmannSizeFactor.TabIndex = 2;
			this.floatTrackbarControlBeckmannSizeFactor.Value = 1F;
			this.floatTrackbarControlBeckmannSizeFactor.VisibleRangeMax = 2F;
			this.floatTrackbarControlBeckmannSizeFactor.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlBeckmannRoughness_ValueChanged );
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point( 851, 51 );
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size( 154, 13 );
			this.label6.TabIndex = 3;
			this.label6.Text = "Beckmann Surface Size Factor";
			// 
			// checkBoxShowLobe
			// 
			this.checkBoxShowLobe.AutoSize = true;
			this.checkBoxShowLobe.Location = new System.Drawing.Point( 7, 223 );
			this.checkBoxShowLobe.Name = "checkBoxShowLobe";
			this.checkBoxShowLobe.Size = new System.Drawing.Size( 198, 17 );
			this.checkBoxShowLobe.TabIndex = 9;
			this.checkBoxShowLobe.Text = "Show Outgoing Lobe with intensity...";
			this.checkBoxShowLobe.UseVisualStyleBackColor = true;
			// 
			// floatTrackbarControlLobeIntensity
			// 
			this.floatTrackbarControlLobeIntensity.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlLobeIntensity.Location = new System.Drawing.Point( 7, 246 );
			this.floatTrackbarControlLobeIntensity.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.floatTrackbarControlLobeIntensity.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.floatTrackbarControlLobeIntensity.Name = "floatTrackbarControlLobeIntensity";
			this.floatTrackbarControlLobeIntensity.RangeMax = 100F;
			this.floatTrackbarControlLobeIntensity.RangeMin = 0F;
			this.floatTrackbarControlLobeIntensity.Size = new System.Drawing.Size( 233, 20 );
			this.floatTrackbarControlLobeIntensity.TabIndex = 5;
			this.floatTrackbarControlLobeIntensity.Value = 1F;
			// 
			// radioButtonHideSurface
			// 
			this.radioButtonHideSurface.AutoSize = true;
			this.radioButtonHideSurface.Location = new System.Drawing.Point( 0, 3 );
			this.radioButtonHideSurface.Name = "radioButtonHideSurface";
			this.radioButtonHideSurface.Size = new System.Drawing.Size( 87, 17 );
			this.radioButtonHideSurface.TabIndex = 4;
			this.radioButtonHideSurface.Text = "Hide Surface";
			this.radioButtonHideSurface.UseVisualStyleBackColor = true;
			// 
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 1112, 654 );
			this.Controls.Add( this.groupBoxDisplay );
			this.Controls.Add( this.integerTrackbarControlIterationsCount );
			this.Controls.Add( this.buttonRayTrace );
			this.Controls.Add( this.floatTrackbarControlPhi );
			this.Controls.Add( this.floatTrackbarControlTheta );
			this.Controls.Add( this.label3 );
			this.Controls.Add( this.label4 );
			this.Controls.Add( this.label2 );
			this.Controls.Add( this.label6 );
			this.Controls.Add( this.floatTrackbarControlBeckmannSizeFactor );
			this.Controls.Add( this.label1 );
			this.Controls.Add( this.floatTrackbarControlBeckmannRoughness );
			this.Controls.Add( this.buttonReload );
			this.Controls.Add( this.panelOutput );
			this.Name = "TestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Multiple-Scattering BSDF Test";
			this.panel1.ResumeLayout( false );
			this.panel1.PerformLayout();
			this.groupBoxDisplay.ResumeLayout( false );
			this.groupBoxDisplay.PerformLayout();
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel panelOutput;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button buttonReload;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlBeckmannRoughness;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton checkBoxShowNormals;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlTheta;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonRayTrace;
		private System.Windows.Forms.RadioButton checkBoxShowOutgoingDirections;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlScatteringOrder;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlIterationsCount;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlPhi;
		private System.Windows.Forms.RadioButton checkBoxShowOutgoingDirectionsHistogram;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.RadioButton radioButtonShowHeights;
		private System.Windows.Forms.GroupBox groupBoxDisplay;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlBeckmannSizeFactor;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.CheckBox checkBoxShowLobe;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlLobeIntensity;
		private System.Windows.Forms.RadioButton radioButtonHideSurface;
	}
}

