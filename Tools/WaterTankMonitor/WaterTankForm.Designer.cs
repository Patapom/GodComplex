namespace WaterTankMonitor {
	partial class WaterTankMonitorForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing ) {
			if ( disposing && (components != null) ) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WaterTankMonitorForm));
			this.buttonNow = new System.Windows.Forms.Button();
			this.panelOutput = new UIUtility.PanelOutput(this.components);
			this.buttonMonth = new System.Windows.Forms.Button();
			this.comboBoxCOMPort = new System.Windows.Forms.ComboBox();
			this.buttonWeek = new System.Windows.Forms.Button();
			this.buttonDay = new System.Windows.Forms.Button();
			this.timerTick = new System.Windows.Forms.Timer(this.components);
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.buttonRefreshCOMPorts = new System.Windows.Forms.Button();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// buttonNow
			// 
			this.buttonNow.Location = new System.Drawing.Point(267, 3);
			this.buttonNow.Name = "buttonNow";
			this.buttonNow.Size = new System.Drawing.Size(49, 23);
			this.buttonNow.TabIndex = 5;
			this.buttonNow.Text = "Now";
			this.buttonNow.UseVisualStyleBackColor = true;
			this.buttonNow.Click += new System.EventHandler(this.buttonNow_Click);
			// 
			// panelOutput
			// 
			this.panelOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelOutput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.panelOutput.Location = new System.Drawing.Point(1, 32);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.PanelBitmap = ((System.Drawing.Bitmap)(resources.GetObject("panelOutput.PanelBitmap")));
			this.panelOutput.Size = new System.Drawing.Size(859, 463);
			this.panelOutput.TabIndex = 6;
			// 
			// buttonMonth
			// 
			this.buttonMonth.Location = new System.Drawing.Point(102, 3);
			this.buttonMonth.Name = "buttonMonth";
			this.buttonMonth.Size = new System.Drawing.Size(49, 23);
			this.buttonMonth.TabIndex = 2;
			this.buttonMonth.Text = "Month";
			this.buttonMonth.UseVisualStyleBackColor = true;
			this.buttonMonth.Click += new System.EventHandler(this.buttonMonth_Click);
			// 
			// comboBoxCOMPort
			// 
			this.comboBoxCOMPort.FormattingEnabled = true;
			this.comboBoxCOMPort.Location = new System.Drawing.Point(1, 4);
			this.comboBoxCOMPort.Name = "comboBoxCOMPort";
			this.comboBoxCOMPort.Size = new System.Drawing.Size(73, 21);
			this.comboBoxCOMPort.TabIndex = 0;
			this.comboBoxCOMPort.SelectedIndexChanged += new System.EventHandler(this.comboBoxCOMPort_SelectedIndexChanged);
			// 
			// buttonWeek
			// 
			this.buttonWeek.Location = new System.Drawing.Point(157, 3);
			this.buttonWeek.Name = "buttonWeek";
			this.buttonWeek.Size = new System.Drawing.Size(49, 23);
			this.buttonWeek.TabIndex = 3;
			this.buttonWeek.Text = "Week";
			this.buttonWeek.UseVisualStyleBackColor = true;
			this.buttonWeek.Click += new System.EventHandler(this.buttonWeek_Click);
			// 
			// buttonDay
			// 
			this.buttonDay.Location = new System.Drawing.Point(212, 2);
			this.buttonDay.Name = "buttonDay";
			this.buttonDay.Size = new System.Drawing.Size(49, 23);
			this.buttonDay.TabIndex = 4;
			this.buttonDay.Text = "Day";
			this.buttonDay.UseVisualStyleBackColor = true;
			this.buttonDay.Click += new System.EventHandler(this.buttonDay_Click);
			// 
			// timerTick
			// 
			this.timerTick.Enabled = true;
			this.timerTick.Interval = 1000;
			// 
			// notifyIcon
			// 
			this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
			this.notifyIcon.Text = "Water Level Monitor";
			this.notifyIcon.Visible = true;
			// 
			// buttonRefreshCOMPorts
			// 
			this.buttonRefreshCOMPorts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonRefreshCOMPorts.Location = new System.Drawing.Point(76, 4);
			this.buttonRefreshCOMPorts.Margin = new System.Windows.Forms.Padding(0);
			this.buttonRefreshCOMPorts.Name = "buttonRefreshCOMPorts";
			this.buttonRefreshCOMPorts.Size = new System.Drawing.Size(21, 20);
			this.buttonRefreshCOMPorts.TabIndex = 1;
			this.buttonRefreshCOMPorts.Text = "@";
			this.buttonRefreshCOMPorts.UseVisualStyleBackColor = true;
			this.buttonRefreshCOMPorts.Click += new System.EventHandler(this.buttonRefreshCOMPorts_Click);
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Location = new System.Drawing.Point(481, 6);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(80, 17);
			this.checkBox1.TabIndex = 7;
			this.checkBox1.Text = "checkBox1";
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(567, 2);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 8;
			this.button1.Text = "button1";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// WaterTankMonitorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(859, 492);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.buttonRefreshCOMPorts);
			this.Controls.Add(this.comboBoxCOMPort);
			this.Controls.Add(this.panelOutput);
			this.Controls.Add(this.buttonDay);
			this.Controls.Add(this.buttonWeek);
			this.Controls.Add(this.buttonMonth);
			this.Controls.Add(this.buttonNow);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = new System.Drawing.Size(640, 400);
			this.Name = "WaterTankMonitorForm";
			this.Text = "Water Level Monitor";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonNow;
		private UIUtility.PanelOutput panelOutput;
		private System.Windows.Forms.Button buttonMonth;
		private System.Windows.Forms.ComboBox comboBoxCOMPort;
		private System.Windows.Forms.Button buttonWeek;
		private System.Windows.Forms.Button buttonDay;
		private System.Windows.Forms.Timer timerTick;
		private System.Windows.Forms.NotifyIcon notifyIcon;
		private System.Windows.Forms.Button buttonRefreshCOMPorts;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.Button button1;
	}
}

