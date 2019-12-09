namespace Brain2 {
	partial class BrainForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.timerDisplay = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// timerDisplay
			// 
			this.timerDisplay.Enabled = true;
			this.timerDisplay.Interval = 10;
			// 
			// BrainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(1190, 685);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.KeyPreview = true;
			this.Name = "BrainForm";
			this.Opacity = 0.75D;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Brain #2";
			this.TransparencyKey = System.Drawing.Color.Transparent;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer timerDisplay;
	}
}

