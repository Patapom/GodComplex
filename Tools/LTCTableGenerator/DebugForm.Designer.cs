namespace LTCTableGenerator
{
	partial class DebugForm
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
			this.panelOutput0 = new LTCTableGenerator.PanelOutput(this.components);
			this.panelOutput1 = new LTCTableGenerator.PanelOutput(this.components);
			this.panelOutput2 = new LTCTableGenerator.PanelOutput(this.components);
			this.panelOutput3 = new LTCTableGenerator.PanelOutput(this.components);
			this.label0 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// panelOutput0
			// 
			this.panelOutput0.Location = new System.Drawing.Point(12, 12);
			this.panelOutput0.Name = "panelOutput0";
			this.panelOutput0.Size = new System.Drawing.Size(377, 220);
			this.panelOutput0.TabIndex = 0;
			// 
			// panelOutput1
			// 
			this.panelOutput1.Location = new System.Drawing.Point(395, 12);
			this.panelOutput1.Name = "panelOutput1";
			this.panelOutput1.Size = new System.Drawing.Size(377, 220);
			this.panelOutput1.TabIndex = 0;
			// 
			// panelOutput2
			// 
			this.panelOutput2.Location = new System.Drawing.Point(12, 262);
			this.panelOutput2.Name = "panelOutput2";
			this.panelOutput2.Size = new System.Drawing.Size(377, 220);
			this.panelOutput2.TabIndex = 0;
			// 
			// panelOutput3
			// 
			this.panelOutput3.Location = new System.Drawing.Point(395, 262);
			this.panelOutput3.Name = "panelOutput3";
			this.panelOutput3.Size = new System.Drawing.Size(377, 220);
			this.panelOutput3.TabIndex = 0;
			// 
			// label0
			// 
			this.label0.AutoSize = true;
			this.label0.Location = new System.Drawing.Point(12, 239);
			this.label0.Name = "label0";
			this.label0.Size = new System.Drawing.Size(35, 13);
			this.label0.TabIndex = 1;
			this.label0.Text = "label1";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(392, 239);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(35, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "label1";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 485);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(35, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "label1";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(392, 485);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(35, 13);
			this.label3.TabIndex = 1;
			this.label3.Text = "label1";
			// 
			// DebugForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(784, 514);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label0);
			this.Controls.Add(this.panelOutput3);
			this.Controls.Add(this.panelOutput2);
			this.Controls.Add(this.panelOutput1);
			this.Controls.Add(this.panelOutput0);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DebugForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Debug Form";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private LTCTableGenerator.PanelOutput panelOutput0;
		private LTCTableGenerator.PanelOutput panelOutput1;
		private LTCTableGenerator.PanelOutput panelOutput2;
		private LTCTableGenerator.PanelOutput panelOutput3;
		private System.Windows.Forms.Label label0;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
	}
}