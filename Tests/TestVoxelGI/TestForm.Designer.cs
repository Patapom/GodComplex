namespace TestVoxelGI
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
			this.panelOutput = new TestVoxelGI.PanelOutput(this.components);
			this.buttonReload = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.integerTrackbarControlVoxelMipIndex = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.buttonComputeIndirect = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1280, 720);
			this.panelOutput.TabIndex = 1;
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(1449, 884);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 2;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 741);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(82, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Voxel Mip Level";
			// 
			// integerTrackbarControlVoxelMipIndex
			// 
			this.integerTrackbarControlVoxelMipIndex.Location = new System.Drawing.Point(100, 738);
			this.integerTrackbarControlVoxelMipIndex.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlVoxelMipIndex.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlVoxelMipIndex.Name = "integerTrackbarControlVoxelMipIndex";
			this.integerTrackbarControlVoxelMipIndex.RangeMax = 7;
			this.integerTrackbarControlVoxelMipIndex.RangeMin = 0;
			this.integerTrackbarControlVoxelMipIndex.Size = new System.Drawing.Size(200, 20);
			this.integerTrackbarControlVoxelMipIndex.TabIndex = 4;
			this.integerTrackbarControlVoxelMipIndex.Value = 0;
			this.integerTrackbarControlVoxelMipIndex.VisibleRangeMax = 7;
			// 
			// buttonComputeIndirect
			// 
			this.buttonComputeIndirect.Location = new System.Drawing.Point(400, 738);
			this.buttonComputeIndirect.Name = "buttonComputeIndirect";
			this.buttonComputeIndirect.Size = new System.Drawing.Size(75, 23);
			this.buttonComputeIndirect.TabIndex = 5;
			this.buttonComputeIndirect.Text = "Compute Indirect";
			this.buttonComputeIndirect.UseVisualStyleBackColor = true;
			this.buttonComputeIndirect.Click += new System.EventHandler(this.buttonComputeIndirect_Click);
			// 
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1536, 919);
			this.Controls.Add(this.buttonComputeIndirect);
			this.Controls.Add(this.integerTrackbarControlVoxelMipIndex);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "TestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Voxel GI Test";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private TestVoxelGI.PanelOutput panelOutput;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlVoxelMipIndex;
		private System.Windows.Forms.Button buttonComputeIndirect;
	}
}

