namespace Mie2QuantileFunction
{
	partial class Form1
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
			this.panelOutput1 = new OutputPanel();
			this.radioButtonPolar = new System.Windows.Forms.RadioButton();
			this.radioButtonLog = new System.Windows.Forms.RadioButton();
			this.radioButtonBuckets = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
			// 
			// panelOutput1
			// 
			this.panelOutput1.Location = new System.Drawing.Point(12, 12);
			this.panelOutput1.Name = "panelOutput1";
			this.panelOutput1.Size = new System.Drawing.Size(644, 455);
			this.panelOutput1.TabIndex = 0;
			// 
			// radioButtonPolar
			// 
			this.radioButtonPolar.AutoSize = true;
			this.radioButtonPolar.Checked = true;
			this.radioButtonPolar.Location = new System.Drawing.Point(674, 12);
			this.radioButtonPolar.Name = "radioButtonPolar";
			this.radioButtonPolar.Size = new System.Drawing.Size(70, 17);
			this.radioButtonPolar.TabIndex = 1;
			this.radioButtonPolar.TabStop = true;
			this.radioButtonPolar.Text = "Polar Plot";
			this.radioButtonPolar.UseVisualStyleBackColor = true;
			this.radioButtonPolar.CheckedChanged += new System.EventHandler(this.radioButtonPolar_CheckedChanged);
			// 
			// radioButtonLog
			// 
			this.radioButtonLog.AutoSize = true;
			this.radioButtonLog.Location = new System.Drawing.Point(674, 35);
			this.radioButtonLog.Name = "radioButtonLog";
			this.radioButtonLog.Size = new System.Drawing.Size(64, 17);
			this.radioButtonLog.TabIndex = 1;
			this.radioButtonLog.Text = "Log Plot";
			this.radioButtonLog.UseVisualStyleBackColor = true;
			this.radioButtonLog.CheckedChanged += new System.EventHandler(this.radioButtonLog_CheckedChanged);
			// 
			// radioButtonBuckets
			// 
			this.radioButtonBuckets.AutoSize = true;
			this.radioButtonBuckets.Location = new System.Drawing.Point(674, 58);
			this.radioButtonBuckets.Name = "radioButtonBuckets";
			this.radioButtonBuckets.Size = new System.Drawing.Size(64, 17);
			this.radioButtonBuckets.TabIndex = 1;
			this.radioButtonBuckets.Text = "Buckets";
			this.radioButtonBuckets.UseVisualStyleBackColor = true;
			this.radioButtonBuckets.CheckedChanged += new System.EventHandler(this.radioButtonBuckets_CheckedChanged);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(790, 582);
			this.Controls.Add(this.radioButtonBuckets);
			this.Controls.Add(this.radioButtonLog);
			this.Controls.Add(this.radioButtonPolar);
			this.Controls.Add(this.panelOutput1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OutputPanel panelOutput1;
		private System.Windows.Forms.RadioButton radioButtonPolar;
		private System.Windows.Forms.RadioButton radioButtonLog;
		private System.Windows.Forms.RadioButton radioButtonBuckets;
	}
}

