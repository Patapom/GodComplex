namespace TestBinaryTreeIntersect
{
	partial class IntersectForm
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
			this.floatTrackbarControl1 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.panelOutput1 = new Nuaj.Cirrus.Utility.PanelOutput(this.components);
			this.SuspendLayout();
			// 
			// floatTrackbarControl1
			// 
			this.floatTrackbarControl1.Location = new System.Drawing.Point(654, 211);
			this.floatTrackbarControl1.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl1.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl1.Name = "floatTrackbarControl1";
			this.floatTrackbarControl1.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControl1.TabIndex = 0;
			this.floatTrackbarControl1.Value = 0F;
			// 
			// panelOutput1
			// 
			this.panelOutput1.Location = new System.Drawing.Point(12, 12);
			this.panelOutput1.Name = "panelOutput1";
			this.panelOutput1.Size = new System.Drawing.Size(511, 366);
			this.panelOutput1.TabIndex = 1;
			// 
			// IntersectForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(900, 582);
			this.Controls.Add(this.panelOutput1);
			this.Controls.Add(this.floatTrackbarControl1);
			this.Name = "IntersectForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Binary Tree Intersector";
			this.ResumeLayout(false);

		}

		#endregion

		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControl1;
		private Nuaj.Cirrus.Utility.PanelOutput panelOutput1;
	}
}

