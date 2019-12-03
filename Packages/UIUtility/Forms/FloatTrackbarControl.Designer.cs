namespace Nuaj.Cirrus.Utility
{
	partial class FloatTrackbarControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.textBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// textBox
			// 
			this.textBox.BackColor = System.Drawing.Color.FromArgb( ((int) (((byte) (200)))), ((int) (((byte) (200)))), ((int) (((byte) (200)))) );
			this.textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox.Location = new System.Drawing.Point( 0, 0 );
			this.textBox.Name = "textBox";
			this.textBox.Size = new System.Drawing.Size( 50, 13 );
			this.textBox.TabIndex = 0;
			this.textBox.Text = "0";
			this.textBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textBox.KeyDown += new System.Windows.Forms.KeyEventHandler( this.textBox_KeyDown );
			this.textBox.Validating += new System.ComponentModel.CancelEventHandler( this.textBox_Validating );
			// 
			// FloatTrackbarControl
			// 
			this.Controls.Add( this.textBox );
			this.MaximumSize = new System.Drawing.Size( 10000, 20 );
			this.MinimumSize = new System.Drawing.Size( 70, 20 );
			this.Size = new System.Drawing.Size( 200, 20 );
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBox;
	}
}
