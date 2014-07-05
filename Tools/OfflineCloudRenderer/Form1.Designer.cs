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

				// Dispose of the device
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
			this.viewportPanel = new OfflineCloudRenderer.ViewportPanel(this.components);
			this.SuspendLayout();
			// 
			// viewportPanel
			// 
			this.viewportPanel.Device = null;
			this.viewportPanel.Location = new System.Drawing.Point(12, 12);
			this.viewportPanel.Name = "viewportPanel";
			this.viewportPanel.Size = new System.Drawing.Size(910, 512);
			this.viewportPanel.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1200, 756);
			this.Controls.Add(this.viewportPanel);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private ViewportPanel viewportPanel;
	}
}

