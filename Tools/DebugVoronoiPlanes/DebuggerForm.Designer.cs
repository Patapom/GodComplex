namespace DebugVoronoiPlanes
{
	partial class DebuggerForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.panelOutput = new System.Windows.Forms.Panel();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.buttonReload = new System.Windows.Forms.Button();
			this.buttonClean = new System.Windows.Forms.Button();
			this.floatTrackbarControlParm = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.integerTrackbarControlCell = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.checkBoxDebugCell = new System.Windows.Forms.CheckBox();
			this.integerTrackbarControlPlane = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.checkBoxDebugPlane = new System.Windows.Forms.CheckBox();
			this.integerTrackbarControlLine = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.checkBoxDebugLine = new System.Windows.Forms.CheckBox();
			this.integerTrackbarControlVertex = new Nuaj.Cirrus.Utility.IntegerTrackbarControl();
			this.checkBoxDebugVertex = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// panelOutput
			// 
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(1000, 640);
			this.panelOutput.TabIndex = 0;
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 20;
			// 
			// buttonReload
			// 
			this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReload.Location = new System.Drawing.Point(1220, 629);
			this.buttonReload.Name = "buttonReload";
			this.buttonReload.Size = new System.Drawing.Size(75, 23);
			this.buttonReload.TabIndex = 1;
			this.buttonReload.Text = "Reload";
			this.buttonReload.UseVisualStyleBackColor = true;
			this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
			// 
			// buttonClean
			// 
			this.buttonClean.Location = new System.Drawing.Point(1018, 628);
			this.buttonClean.Name = "buttonClean";
			this.buttonClean.Size = new System.Drawing.Size(75, 23);
			this.buttonClean.TabIndex = 2;
			this.buttonClean.Text = "Clean Cells";
			this.buttonClean.UseVisualStyleBackColor = true;
			this.buttonClean.Click += new System.EventHandler(this.buttonClean_Click);
			// 
			// floatTrackbarControlParm
			// 
			this.floatTrackbarControlParm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackbarControlParm.Location = new System.Drawing.Point(1095, 312);
			this.floatTrackbarControlParm.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlParm.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlParm.Name = "floatTrackbarControlParm";
			this.floatTrackbarControlParm.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlParm.TabIndex = 3;
			this.floatTrackbarControlParm.Value = 1F;
			this.floatTrackbarControlParm.VisibleRangeMax = 1F;
			// 
			// integerTrackbarControlCell
			// 
			this.integerTrackbarControlCell.Enabled = false;
			this.integerTrackbarControlCell.Location = new System.Drawing.Point(1168, 12);
			this.integerTrackbarControlCell.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlCell.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlCell.Name = "integerTrackbarControlCell";
			this.integerTrackbarControlCell.RangeMin = 0;
			this.integerTrackbarControlCell.Size = new System.Drawing.Size(127, 20);
			this.integerTrackbarControlCell.TabIndex = 4;
			this.integerTrackbarControlCell.Value = 0;
			this.integerTrackbarControlCell.VisibleRangeMax = 10;
			// 
			// checkBoxDebugCell
			// 
			this.checkBoxDebugCell.AutoSize = true;
			this.checkBoxDebugCell.Location = new System.Drawing.Point(1019, 13);
			this.checkBoxDebugCell.Name = "checkBoxDebugCell";
			this.checkBoxDebugCell.Size = new System.Drawing.Size(143, 17);
			this.checkBoxDebugCell.TabIndex = 5;
			this.checkBoxDebugCell.Text = "Enable debugging of cell";
			this.checkBoxDebugCell.UseVisualStyleBackColor = true;
			// 
			// integerTrackbarControlPlane
			// 
			this.integerTrackbarControlPlane.Enabled = false;
			this.integerTrackbarControlPlane.Location = new System.Drawing.Point(1168, 35);
			this.integerTrackbarControlPlane.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlPlane.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlPlane.Name = "integerTrackbarControlPlane";
			this.integerTrackbarControlPlane.RangeMin = 0;
			this.integerTrackbarControlPlane.Size = new System.Drawing.Size(127, 20);
			this.integerTrackbarControlPlane.TabIndex = 4;
			this.integerTrackbarControlPlane.Value = 0;
			this.integerTrackbarControlPlane.VisibleRangeMax = 10;
			// 
			// checkBoxDebugPlane
			// 
			this.checkBoxDebugPlane.AutoSize = true;
			this.checkBoxDebugPlane.Location = new System.Drawing.Point(1019, 36);
			this.checkBoxDebugPlane.Name = "checkBoxDebugPlane";
			this.checkBoxDebugPlane.Size = new System.Drawing.Size(153, 17);
			this.checkBoxDebugPlane.TabIndex = 5;
			this.checkBoxDebugPlane.Text = "Enable debugging of plane";
			this.checkBoxDebugPlane.UseVisualStyleBackColor = true;
			// 
			// integerTrackbarControlLine
			// 
			this.integerTrackbarControlLine.Enabled = false;
			this.integerTrackbarControlLine.Location = new System.Drawing.Point(1168, 58);
			this.integerTrackbarControlLine.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlLine.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlLine.Name = "integerTrackbarControlLine";
			this.integerTrackbarControlLine.RangeMin = 0;
			this.integerTrackbarControlLine.Size = new System.Drawing.Size(127, 20);
			this.integerTrackbarControlLine.TabIndex = 4;
			this.integerTrackbarControlLine.Value = 0;
			this.integerTrackbarControlLine.VisibleRangeMax = 10;
			// 
			// checkBoxDebugLine
			// 
			this.checkBoxDebugLine.AutoSize = true;
			this.checkBoxDebugLine.Location = new System.Drawing.Point(1019, 59);
			this.checkBoxDebugLine.Name = "checkBoxDebugLine";
			this.checkBoxDebugLine.Size = new System.Drawing.Size(143, 17);
			this.checkBoxDebugLine.TabIndex = 5;
			this.checkBoxDebugLine.Text = "Enable debugging of line";
			this.checkBoxDebugLine.UseVisualStyleBackColor = true;
			// 
			// integerTrackbarControlVertex
			// 
			this.integerTrackbarControlVertex.Enabled = false;
			this.integerTrackbarControlVertex.Location = new System.Drawing.Point(1168, 81);
			this.integerTrackbarControlVertex.MaximumSize = new System.Drawing.Size(10000, 20);
			this.integerTrackbarControlVertex.MinimumSize = new System.Drawing.Size(70, 20);
			this.integerTrackbarControlVertex.Name = "integerTrackbarControlVertex";
			this.integerTrackbarControlVertex.RangeMax = 1;
			this.integerTrackbarControlVertex.RangeMin = 0;
			this.integerTrackbarControlVertex.Size = new System.Drawing.Size(127, 20);
			this.integerTrackbarControlVertex.TabIndex = 4;
			this.integerTrackbarControlVertex.Value = 0;
			this.integerTrackbarControlVertex.VisibleRangeMax = 1;
			// 
			// checkBoxDebugVertex
			// 
			this.checkBoxDebugVertex.AutoSize = true;
			this.checkBoxDebugVertex.Location = new System.Drawing.Point(1019, 82);
			this.checkBoxDebugVertex.Name = "checkBoxDebugVertex";
			this.checkBoxDebugVertex.Size = new System.Drawing.Size(156, 17);
			this.checkBoxDebugVertex.TabIndex = 5;
			this.checkBoxDebugVertex.Text = "Enable debugging of vertex";
			this.checkBoxDebugVertex.UseVisualStyleBackColor = true;
			// 
			// DebuggerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1307, 663);
			this.Controls.Add(this.integerTrackbarControlVertex);
			this.Controls.Add(this.checkBoxDebugLine);
			this.Controls.Add(this.integerTrackbarControlLine);
			this.Controls.Add(this.integerTrackbarControlPlane);
			this.Controls.Add(this.checkBoxDebugCell);
			this.Controls.Add(this.integerTrackbarControlCell);
			this.Controls.Add(this.floatTrackbarControlParm);
			this.Controls.Add(this.buttonClean);
			this.Controls.Add(this.buttonReload);
			this.Controls.Add(this.panelOutput);
			this.Controls.Add(this.checkBoxDebugPlane);
			this.Controls.Add(this.checkBoxDebugVertex);
			this.Name = "DebuggerForm";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel panelOutput;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button buttonReload;
		private System.Windows.Forms.Button buttonClean;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlParm;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlCell;
		private System.Windows.Forms.CheckBox checkBoxDebugCell;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlPlane;
		private System.Windows.Forms.CheckBox checkBoxDebugPlane;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlLine;
		private System.Windows.Forms.CheckBox checkBoxDebugLine;
		private Nuaj.Cirrus.Utility.IntegerTrackbarControl integerTrackbarControlVertex;
		private System.Windows.Forms.CheckBox checkBoxDebugVertex;
	}
}

