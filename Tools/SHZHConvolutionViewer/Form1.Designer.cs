namespace SHZHConvolutionViewer
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
			this.panelOutput1 = new SHZHConvolutionViewer.PanelOutput(this.components);
			this.buttonReload = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// panelOutput1
			// 
			this.panelOutput1.Location = new System.Drawing.Point(12, 12);
			this.panelOutput1.Name = "panelOutput1";
			this.panelOutput1.Size = new System.Drawing.Size(829, 628);
			this.panelOutput1.TabIndex = 0;
			// 
			// buttonReload
			// 
			this.buttonReload.Location = new System.Drawing.Point(1041, 617);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 1;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1128, 652);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput1);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "SH and ZH product viewer";
			this.ResumeLayout(false);

		}

		#endregion

		private PanelOutput panelOutput1;
		private System.Windows.Forms.Button buttonReload;
	}
}

