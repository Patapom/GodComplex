namespace WaterTankMonitorPassive {
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
			this.contextMenuStripPanel = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.createIntervalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.renameIntervalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteIntervalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.setTimeReferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.clearTimeReferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.setLowLevelWarningLimitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.setIntervalStartTimeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.panelWarning = new WaterTankMonitor.PanelWarning(this.components);
			this.buttonMonth = new System.Windows.Forms.Button();
			this.comboBoxCOMPort = new System.Windows.Forms.ComboBox();
			this.buttonWeek = new System.Windows.Forms.Button();
			this.buttonDay = new System.Windows.Forms.Button();
			this.timerTick = new System.Windows.Forms.Timer(this.components);
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenuStripTray = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.saveLogFileNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.buttonRefreshCOMPorts = new System.Windows.Forms.Button();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.button1 = new System.Windows.Forms.Button();
			this.buttonHour = new System.Windows.Forms.Button();
			this.setIntervalEndTimeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.panelOutput.SuspendLayout();
			this.contextMenuStripPanel.SuspendLayout();
			this.contextMenuStripTray.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonNow
			// 
			this.buttonNow.Location = new System.Drawing.Point(322, 1);
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
			this.panelOutput.ContextMenuStrip = this.contextMenuStripPanel;
			this.panelOutput.Controls.Add(this.panelWarning);
			this.panelOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.panelOutput.Location = new System.Drawing.Point(1, 32);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.PanelBitmap = ((System.Drawing.Bitmap)(resources.GetObject("panelOutput.PanelBitmap")));
			this.panelOutput.Size = new System.Drawing.Size(859, 463);
			this.panelOutput.TabIndex = 6;
			// 
			// contextMenuStripPanel
			// 
			this.contextMenuStripPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.createIntervalToolStripMenuItem,
            this.setIntervalStartTimeToolStripMenuItem,
            this.setIntervalEndTimeToolStripMenuItem,
            this.renameIntervalToolStripMenuItem,
            this.deleteIntervalToolStripMenuItem,
            this.toolStripMenuItem5,
            this.setTimeReferenceToolStripMenuItem,
            this.clearTimeReferenceToolStripMenuItem,
            this.toolStripMenuItem2,
            this.setLowLevelWarningLimitToolStripMenuItem});
			this.contextMenuStripPanel.Name = "contextMenuStripPanel";
			this.contextMenuStripPanel.Size = new System.Drawing.Size(224, 242);
			this.contextMenuStripPanel.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripPanel_Opening);
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(220, 6);
			// 
			// createIntervalToolStripMenuItem
			// 
			this.createIntervalToolStripMenuItem.Name = "createIntervalToolStripMenuItem";
			this.createIntervalToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
			this.createIntervalToolStripMenuItem.Text = "Create &Interval";
			this.createIntervalToolStripMenuItem.Click += new System.EventHandler(this.createIntervalToolStripMenuItem_Click);
			// 
			// renameIntervalToolStripMenuItem
			// 
			this.renameIntervalToolStripMenuItem.Enabled = false;
			this.renameIntervalToolStripMenuItem.Name = "renameIntervalToolStripMenuItem";
			this.renameIntervalToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
			this.renameIntervalToolStripMenuItem.Text = "Rename Interval";
			this.renameIntervalToolStripMenuItem.Click += new System.EventHandler(this.renameIntervalToolStripMenuItem_Click);
			// 
			// deleteIntervalToolStripMenuItem
			// 
			this.deleteIntervalToolStripMenuItem.Enabled = false;
			this.deleteIntervalToolStripMenuItem.Name = "deleteIntervalToolStripMenuItem";
			this.deleteIntervalToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
			this.deleteIntervalToolStripMenuItem.Text = "Delete Interval";
			this.deleteIntervalToolStripMenuItem.Click += new System.EventHandler(this.deleteIntervalToolStripMenuItem_Click);
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.toolStripMenuItem5.Size = new System.Drawing.Size(220, 6);
			// 
			// setTimeReferenceToolStripMenuItem
			// 
			this.setTimeReferenceToolStripMenuItem.Name = "setTimeReferenceToolStripMenuItem";
			this.setTimeReferenceToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
			this.setTimeReferenceToolStripMenuItem.Text = "Set Time Reference";
			this.setTimeReferenceToolStripMenuItem.Click += new System.EventHandler(this.setTimeReferenceToolStripMenuItem_Click);
			// 
			// clearTimeReferenceToolStripMenuItem
			// 
			this.clearTimeReferenceToolStripMenuItem.Name = "clearTimeReferenceToolStripMenuItem";
			this.clearTimeReferenceToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
			this.clearTimeReferenceToolStripMenuItem.Text = "Clear Time Reference";
			this.clearTimeReferenceToolStripMenuItem.Click += new System.EventHandler(this.clearTimeReferenceToolStripMenuItem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(220, 6);
			// 
			// setLowLevelWarningLimitToolStripMenuItem
			// 
			this.setLowLevelWarningLimitToolStripMenuItem.Name = "setLowLevelWarningLimitToolStripMenuItem";
			this.setLowLevelWarningLimitToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
			this.setLowLevelWarningLimitToolStripMenuItem.Text = "Set Low Level Warning Limit";
			this.setLowLevelWarningLimitToolStripMenuItem.Click += new System.EventHandler(this.setLowLevelWarningLimitToolStripMenuItem_Click);
			// 
			// setIntervalStartTimeToolStripMenuItem
			// 
			this.setIntervalStartTimeToolStripMenuItem.Enabled = false;
			this.setIntervalStartTimeToolStripMenuItem.Name = "setIntervalStartTimeToolStripMenuItem";
			this.setIntervalStartTimeToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
			this.setIntervalStartTimeToolStripMenuItem.Text = "Set Interval Start Time";
			this.setIntervalStartTimeToolStripMenuItem.Click += new System.EventHandler(this.setIntervalStartTimeToolStripMenuItem_Click);
			// 
			// panelWarning
			// 
			this.panelWarning.BackColor = System.Drawing.Color.Silver;
			this.panelWarning.BackColorFlash = System.Drawing.Color.White;
			this.panelWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.panelWarning.FontWarning = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.panelWarning.ForeColor = System.Drawing.Color.Red;
			this.panelWarning.Location = new System.Drawing.Point(100, 73);
			this.panelWarning.Message = "Achtung!";
			this.panelWarning.Name = "panelWarning";
			this.panelWarning.Size = new System.Drawing.Size(564, 265);
			this.panelWarning.TabIndex = 0;
			this.panelWarning.Visible = false;
			this.panelWarning.VisibleChanged += new System.EventHandler(this.panelWarning_VisibleChanged);
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
			this.comboBoxCOMPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCOMPort.FormattingEnabled = true;
			this.comboBoxCOMPort.Location = new System.Drawing.Point(3, 4);
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
			this.notifyIcon.ContextMenuStrip = this.contextMenuStripTray;
			this.notifyIcon.Icon = global::WaterTankMonitor.Properties.Resources.Icon;
			this.notifyIcon.Text = "Water Level Monitor";
			this.notifyIcon.Visible = true;
			this.notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseClick);
			// 
			// contextMenuStripTray
			// 
			this.contextMenuStripTray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveLogFileNowToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
			this.contextMenuStripTray.Name = "contextMenuStripTray";
			this.contextMenuStripTray.Size = new System.Drawing.Size(174, 76);
			// 
			// saveLogFileNowToolStripMenuItem
			// 
			this.saveLogFileNowToolStripMenuItem.Name = "saveLogFileNowToolStripMenuItem";
			this.saveLogFileNowToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
			this.saveLogFileNowToolStripMenuItem.Text = "Save Log File Now";
			this.saveLogFileNowToolStripMenuItem.Click += new System.EventHandler(this.saveLogFileNowToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(170, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// buttonRefreshCOMPorts
			// 
			this.buttonRefreshCOMPorts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonRefreshCOMPorts.Location = new System.Drawing.Point(78, 4);
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
			// buttonHour
			// 
			this.buttonHour.Location = new System.Drawing.Point(267, 1);
			this.buttonHour.Name = "buttonHour";
			this.buttonHour.Size = new System.Drawing.Size(49, 23);
			this.buttonHour.TabIndex = 4;
			this.buttonHour.Text = "Hour";
			this.buttonHour.UseVisualStyleBackColor = true;
			this.buttonHour.Click += new System.EventHandler(this.buttonHour_Click);
			// 
			// setIntervalEndTimeToolStripMenuItem
			// 
			this.setIntervalEndTimeToolStripMenuItem.Enabled = false;
			this.setIntervalEndTimeToolStripMenuItem.Name = "setIntervalEndTimeToolStripMenuItem";
			this.setIntervalEndTimeToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
			this.setIntervalEndTimeToolStripMenuItem.Text = "Set Interval End Time";
			this.setIntervalEndTimeToolStripMenuItem.Click += new System.EventHandler(this.setIntervalEndTimeToolStripMenuItem_Click);
			// 
			// WaterTankMonitorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(859, 492);
			this.Controls.Add(this.comboBoxCOMPort);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.buttonRefreshCOMPorts);
			this.Controls.Add(this.panelOutput);
			this.Controls.Add(this.buttonHour);
			this.Controls.Add(this.buttonDay);
			this.Controls.Add(this.buttonWeek);
			this.Controls.Add(this.buttonMonth);
			this.Controls.Add(this.buttonNow);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = new System.Drawing.Size(640, 400);
			this.Name = "WaterTankMonitorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Water Level Monitor";
			this.panelOutput.ResumeLayout(false);
			this.contextMenuStripPanel.ResumeLayout(false);
			this.contextMenuStripTray.ResumeLayout(false);
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
		private System.Windows.Forms.ContextMenuStrip contextMenuStripTray;
		private System.Windows.Forms.ToolStripMenuItem saveLogFileNowToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripPanel;
		private System.Windows.Forms.ToolStripMenuItem setLowLevelWarningLimitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem setTimeReferenceToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem clearTimeReferenceToolStripMenuItem;
		private System.Windows.Forms.Button buttonHour;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private WaterTankMonitor.PanelWarning panelWarning;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripMenuItem createIntervalToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem renameIntervalToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteIntervalToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
		private System.Windows.Forms.ToolStripMenuItem setIntervalStartTimeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem setIntervalEndTimeToolStripMenuItem;
	}
}

