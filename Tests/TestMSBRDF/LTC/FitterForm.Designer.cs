namespace TestMSBRDF.LTC
{
	partial class FitterForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FitterForm));
			this.panelOutputSourceBRDF = new Nuaj.Cirrus.Utility.PanelOutput(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.panelOutputTargetBRDF = new Nuaj.Cirrus.Utility.PanelOutput(this.components);
			this.panelOutputDifference = new Nuaj.Cirrus.Utility.PanelOutput(this.components);
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// panelOutputSourceBRDF
			// 
			this.panelOutputSourceBRDF.Location = new System.Drawing.Point(12, 12);
			this.panelOutputSourceBRDF.Name = "panelOutputSourceBRDF";
			this.panelOutputSourceBRDF.Size = new System.Drawing.Size(340, 340);
			this.panelOutputSourceBRDF.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 355);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(73, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Source BRDF";
			// 
			// panelOutputTargetBRDF
			// 
			this.panelOutputTargetBRDF.Location = new System.Drawing.Point(358, 12);
			this.panelOutputTargetBRDF.Name = "panelOutputTargetBRDF";
			this.panelOutputTargetBRDF.Size = new System.Drawing.Size(340, 340);
			this.panelOutputTargetBRDF.TabIndex = 0;
			// 
			// panelOutputDifference
			// 
			this.panelOutputDifference.Location = new System.Drawing.Point(704, 12);
			this.panelOutputDifference.Name = "panelOutputDifference";
			this.panelOutputDifference.Size = new System.Drawing.Size(340, 340);
			this.panelOutputDifference.TabIndex = 0;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(355, 355);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(78, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Mapped BRDF";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(701, 355);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(56, 13);
			this.label3.TabIndex = 1;
			this.label3.Text = "Difference";
			// 
			// panel1
			// 
			this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
			this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.panel1.Location = new System.Drawing.Point(91, 355);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(261, 13);
			this.panel1.TabIndex = 2;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(88, 371);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(28, 13);
			this.label4.TabIndex = 1;
			this.label4.Text = "1e-4";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(324, 371);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(31, 13);
			this.label5.TabIndex = 1;
			this.label5.Text = "1e+4";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(760, 371);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(28, 13);
			this.label6.TabIndex = 1;
			this.label6.Text = "1e-4";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(996, 371);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(31, 13);
			this.label7.TabIndex = 1;
			this.label7.Text = "1e+0";
			// 
			// panel2
			// 
			this.panel2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel2.BackgroundImage")));
			this.panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.panel2.Location = new System.Drawing.Point(763, 355);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(261, 13);
			this.panel2.TabIndex = 2;
			// 
			// FitterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1057, 396);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.panelOutputDifference);
			this.Controls.Add(this.panelOutputTargetBRDF);
			this.Controls.Add(this.panelOutputSourceBRDF);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FitterForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Fitter Debugger";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Nuaj.Cirrus.Utility.PanelOutput panelOutputSourceBRDF;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.PanelOutput panelOutputTargetBRDF;
		private Nuaj.Cirrus.Utility.PanelOutput panelOutputDifference;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Panel panel2;
	}
}