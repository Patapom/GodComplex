namespace VoxelConeTracing
{
	partial class VoxelForm
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
			this.outputPanel = new VoxelConeTracing.OutputPanel(this.components);
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.buttonReload = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// outputPanel
			// 
			this.outputPanel.Location = new System.Drawing.Point(12, 12);
			this.outputPanel.Name = "outputPanel";
			this.outputPanel.Size = new System.Drawing.Size(1024, 640);
			this.outputPanel.TabIndex = 0;
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = 10;
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(1149, 737);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 1;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// VoxelForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1236, 772);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.outputPanel);
			this.Name = "VoxelForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Voxel Cone Tracing Test";
			this.ResumeLayout(false);

		}

		#endregion

		private OutputPanel outputPanel;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.Button buttonReload;
	}
}

