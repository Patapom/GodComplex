namespace FBXTestConverter
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
			this.textBoxReport = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// textBoxReport
			// 
			this.textBoxReport.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxReport.Location = new System.Drawing.Point(0, 0);
			this.textBoxReport.Multiline = true;
			this.textBoxReport.Name = "textBoxReport";
			this.textBoxReport.ReadOnly = true;
			this.textBoxReport.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxReport.Size = new System.Drawing.Size(478, 548);
			this.textBoxReport.TabIndex = 0;
			this.textBoxReport.WordWrap = false;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(478, 548);
			this.Controls.Add(this.textBoxReport);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBoxReport;

	}
}

