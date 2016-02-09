namespace TestMSBSDF
{
	partial class CompletionArrayControl
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
			this.components = new System.ComponentModel.Container();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// toolTip1
			// 
			this.toolTip1.AutomaticDelay = 100;
			this.toolTip1.AutoPopDelay = 0;
			this.toolTip1.InitialDelay = 100;
			this.toolTip1.ReshowDelay = 20;
			this.toolTip1.ShowAlways = true;
			this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// CompletionArrayControl
			// 
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.Size = new System.Drawing.Size(200, 20);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Timer timer1;

	}
}
