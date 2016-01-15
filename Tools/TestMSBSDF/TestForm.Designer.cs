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
			this.checkBoxShowNormals = new System.Windows.Forms.CheckBox();
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
			this.buttonReload.Location = new System.Drawing.Point( 1025, 619 );
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size( 75, 23 );
			this.buttonReload.TabIndex = 1;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler( this.buttonReload_Click );
			// 
			// floatTrackbarControlBeckmannRoughness
			// 
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
			this.checkBoxShowNormals.Location = new System.Drawing.Point( 854, 55 );
			this.checkBoxShowNormals.Name = "checkBoxShowNormals";
			this.checkBoxShowNormals.Size = new System.Drawing.Size( 94, 17 );
			this.checkBoxShowNormals.TabIndex = 4;
			this.checkBoxShowNormals.Text = "Show Normals";
			this.checkBoxShowNormals.UseVisualStyleBackColor = true;
			// 
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 1112, 654 );
			this.Controls.Add( this.checkBoxShowNormals );
			this.Controls.Add( this.label1 );
			this.Controls.Add( this.floatTrackbarControlBeckmannRoughness );
			this.Controls.Add( this.buttonReload );
			this.Controls.Add( this.panelOutput );
			this.Name = "TestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Multiple-Scattering BSDF Test";
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel panelOutput;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button buttonReload;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlBeckmannRoughness;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkBoxShowNormals;
	}
}

