namespace PhilBackup
{
	partial class LogForm
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
			this.listBoxErrors = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// listBoxErrors
			// 
			this.listBoxErrors.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.listBoxErrors.FormattingEnabled = true;
			this.listBoxErrors.Location = new System.Drawing.Point( 12, 12 );
			this.listBoxErrors.Name = "listBoxErrors";
			this.listBoxErrors.Size = new System.Drawing.Size( 450, 472 );
			this.listBoxErrors.TabIndex = 0;
			// 
			// LogForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 474, 499 );
			this.Controls.Add( this.listBoxErrors );
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "LogForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Error Log";
			this.ResumeLayout( false );

		}

		#endregion

		private System.Windows.Forms.ListBox listBoxErrors;
	}
}