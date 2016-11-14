namespace ImageUtility.UnitTests
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
			this.panel1 = new PanelOutput();
			this.textBoxEXIF = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Location = new System.Drawing.Point(12, 37);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(842, 568);
			this.panel1.TabIndex = 0;
			// 
			// textBoxEXIF
			// 
			this.textBoxEXIF.Location = new System.Drawing.Point(861, 37);
			this.textBoxEXIF.Multiline = true;
			this.textBoxEXIF.Name = "textBoxEXIF";
			this.textBoxEXIF.ReadOnly = true;
			this.textBoxEXIF.Size = new System.Drawing.Size(297, 568);
			this.textBoxEXIF.TabIndex = 1;
			// 
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1170, 722);
			this.Controls.Add(this.textBoxEXIF);
			this.Controls.Add(this.panel1);
			this.Name = "TestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Image Utility Lib Unit Test";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panel1;
		private System.Windows.Forms.TextBox textBoxEXIF;

	}
}

