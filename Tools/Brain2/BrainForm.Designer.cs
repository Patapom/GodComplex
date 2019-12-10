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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrainForm));
			this.timerDisplay = new System.Windows.Forms.Timer(this.components);
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.SuspendLayout();
			// 
			// timerDisplay
			// 
			this.timerDisplay.Enabled = true;
			this.timerDisplay.Interval = 10;
			// 
			// notifyIcon
			// 
			this.notifyIcon.BalloonTipText = "Brain 2";
			this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
			this.notifyIcon.Text = "Brain 2";
			this.notifyIcon.Visible = true;
			this.notifyIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseUp);
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
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Brain #2";
			this.TopMost = true;
			this.TransparencyKey = System.Drawing.Color.Transparent;
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BrainForm_MouseUp);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer timerDisplay;
		private System.Windows.Forms.NotifyIcon notifyIcon;
	}
}

