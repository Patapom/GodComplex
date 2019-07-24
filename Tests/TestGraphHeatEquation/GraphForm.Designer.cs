namespace TestGraphHeatEquation
{
	partial class GraphForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.button1 = new System.Windows.Forms.Button();
			this.floatTrackbarControl1 = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.panelOutput = new TestGraphHeatEquation.PanelOutput(this.components);
			this.SuspendLayout();
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Location = new System.Drawing.Point(688, 284);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(80, 17);
			this.checkBox1.TabIndex = 2;
			this.checkBox1.Text = "checkBox1";
			this.checkBox1.UseVisualStyleBackColor = true;
			this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(664, 370);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 3;
			this.button1.Text = "button1";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// floatTrackbarControl1
			// 
			this.floatTrackbarControl1.Location = new System.Drawing.Point(605, 98);
			this.floatTrackbarControl1.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl1.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl1.Name = "floatTrackbarControl1";
			this.floatTrackbarControl1.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControl1.TabIndex = 1;
			this.floatTrackbarControl1.Value = 0F;
			this.floatTrackbarControl1.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControl1_ValueChanged);
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(512, 512);
			this.panelOutput.TabIndex = 0;
			// 
			// GraphForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(873, 543);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.floatTrackbarControl1);
			this.Controls.Add(this.panelOutput);
			this.Name = "GraphForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Heat Wave Test";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private PanelOutput panelOutput;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControl1;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.Button button1;
	}
}