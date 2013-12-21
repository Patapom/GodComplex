namespace Cooker
{
	partial class CookerForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.comboBoxPlatform = new System.Windows.Forms.ComboBox();
			this.buttonCook = new System.Windows.Forms.Button();
			this.richTextBoxOutput = new System.Windows.Forms.RichTextBox();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.textBoxMapName = new System.Windows.Forms.TextBox();
			this.buttonLoadMap = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBoxOutput = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textBoxCommandLine = new System.Windows.Forms.TextBox();
			this.processCook = new System.Diagnostics.Process();
			this.panelInput = new System.Windows.Forms.Panel();
			this.comboBoxExecutable = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.textBoxExecutablePath = new System.Windows.Forms.TextBox();
			this.groupBoxOutput.SuspendLayout();
			this.panelInput.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(28, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Map";
			// 
			// comboBoxPlatform
			// 
			this.comboBoxPlatform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxPlatform.FormattingEnabled = true;
			this.comboBoxPlatform.Items.AddRange(new object[] {
            "PC",
            "ORBIS",
            "DURANGO"});
			this.comboBoxPlatform.Location = new System.Drawing.Point(68, 38);
			this.comboBoxPlatform.Name = "comboBoxPlatform";
			this.comboBoxPlatform.Size = new System.Drawing.Size(107, 21);
			this.comboBoxPlatform.TabIndex = 1;
			this.comboBoxPlatform.SelectedIndexChanged += new System.EventHandler(this.comboBoxPlatform_SelectedIndexChanged);
			// 
			// buttonCook
			// 
			this.buttonCook.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCook.Enabled = false;
			this.buttonCook.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
			this.buttonCook.Location = new System.Drawing.Point(179, 102);
			this.buttonCook.Name = "buttonCook";
			this.buttonCook.Size = new System.Drawing.Size(348, 34);
			this.buttonCook.TabIndex = 1;
			this.buttonCook.Text = "Cook";
			this.buttonCook.UseVisualStyleBackColor = true;
			this.buttonCook.Click += new System.EventHandler(this.buttonCook_Click);
			// 
			// richTextBoxOutput
			// 
			this.richTextBoxOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBoxOutput.Location = new System.Drawing.Point(3, 16);
			this.richTextBoxOutput.Name = "richTextBoxOutput";
			this.richTextBoxOutput.Size = new System.Drawing.Size(680, 328);
			this.richTextBoxOutput.TabIndex = 0;
			this.richTextBoxOutput.Text = "";
			this.richTextBoxOutput.WordWrap = false;
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "map";
			this.openFileDialog.Filter = "Map Files (*.map)|*.map|All Files (*.*)|*.*";
			this.openFileDialog.Title = "Choose a map file to cook...";
			// 
			// textBoxMapName
			// 
			this.textBoxMapName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxMapName.Location = new System.Drawing.Point(54, 9);
			this.textBoxMapName.Name = "textBoxMapName";
			this.textBoxMapName.ReadOnly = true;
			this.textBoxMapName.Size = new System.Drawing.Size(594, 20);
			this.textBoxMapName.TabIndex = 4;
			// 
			// buttonLoadMap
			// 
			this.buttonLoadMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLoadMap.Location = new System.Drawing.Point(654, 6);
			this.buttonLoadMap.Name = "buttonLoadMap";
			this.buttonLoadMap.Size = new System.Drawing.Size(35, 24);
			this.buttonLoadMap.TabIndex = 0;
			this.buttonLoadMap.Text = "...";
			this.buttonLoadMap.UseVisualStyleBackColor = true;
			this.buttonLoadMap.Click += new System.EventHandler(this.buttonLoadMap_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 41);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(45, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Platform";
			// 
			// groupBoxOutput
			// 
			this.groupBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxOutput.Controls.Add(this.richTextBoxOutput);
			this.groupBoxOutput.Location = new System.Drawing.Point(12, 142);
			this.groupBoxOutput.Name = "groupBoxOutput";
			this.groupBoxOutput.Size = new System.Drawing.Size(686, 347);
			this.groupBoxOutput.TabIndex = 0;
			this.groupBoxOutput.TabStop = false;
			this.groupBoxOutput.Text = "Output";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(193, 41);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(106, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "Command Line Prefix";
			// 
			// textBoxCommandLine
			// 
			this.textBoxCommandLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxCommandLine.Location = new System.Drawing.Point(305, 38);
			this.textBoxCommandLine.Name = "textBoxCommandLine";
			this.textBoxCommandLine.Size = new System.Drawing.Size(384, 20);
			this.textBoxCommandLine.TabIndex = 2;
			this.textBoxCommandLine.Text = "+fs_basepath V:\\blacksparrow\\idtech5\\blacksparrow";
			this.textBoxCommandLine.TextChanged += new System.EventHandler(this.textBoxCommandLine_TextChanged);
			// 
			// processCook
			// 
			this.processCook.EnableRaisingEvents = true;
			this.processCook.StartInfo.Domain = "";
			this.processCook.StartInfo.LoadUserProfile = false;
			this.processCook.StartInfo.Password = null;
			this.processCook.StartInfo.StandardErrorEncoding = null;
			this.processCook.StartInfo.StandardOutputEncoding = null;
			this.processCook.StartInfo.UserName = "";
			this.processCook.SynchronizingObject = this;
			this.processCook.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(this.processCook_OutputDataReceived);
			this.processCook.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(this.processCook_ErrorDataReceived);
			// 
			// panelInput
			// 
			this.panelInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelInput.Controls.Add(this.label1);
			this.panelInput.Controls.Add(this.label4);
			this.panelInput.Controls.Add(this.label2);
			this.panelInput.Controls.Add(this.textBoxCommandLine);
			this.panelInput.Controls.Add(this.label3);
			this.panelInput.Controls.Add(this.textBoxExecutablePath);
			this.panelInput.Controls.Add(this.textBoxMapName);
			this.panelInput.Controls.Add(this.comboBoxExecutable);
			this.panelInput.Controls.Add(this.comboBoxPlatform);
			this.panelInput.Controls.Add(this.buttonLoadMap);
			this.panelInput.Location = new System.Drawing.Point(0, 1);
			this.panelInput.Name = "panelInput";
			this.panelInput.Size = new System.Drawing.Size(709, 95);
			this.panelInput.TabIndex = 6;
			// 
			// comboBoxExecutable
			// 
			this.comboBoxExecutable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxExecutable.FormattingEnabled = true;
			this.comboBoxExecutable.Items.AddRange(new object[] {
            "Debug",
            "Release",
            "Perforce Latest"});
			this.comboBoxExecutable.Location = new System.Drawing.Point(68, 65);
			this.comboBoxExecutable.Name = "comboBoxExecutable";
			this.comboBoxExecutable.Size = new System.Drawing.Size(107, 21);
			this.comboBoxExecutable.TabIndex = 3;
			this.comboBoxExecutable.SelectedIndexChanged += new System.EventHandler(this.comboBoxExecutable_SelectedIndexChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(3, 68);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 13);
			this.label4.TabIndex = 0;
			this.label4.Text = "Executable";
			// 
			// textBoxExecutablePath
			// 
			this.textBoxExecutablePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxExecutablePath.Location = new System.Drawing.Point(181, 65);
			this.textBoxExecutablePath.Name = "textBoxExecutablePath";
			this.textBoxExecutablePath.ReadOnly = true;
			this.textBoxExecutablePath.Size = new System.Drawing.Size(508, 20);
			this.textBoxExecutablePath.TabIndex = 4;
			// 
			// CookerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(710, 501);
			this.Controls.Add(this.panelInput);
			this.Controls.Add(this.groupBoxOutput);
			this.Controls.Add(this.buttonCook);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "CookerForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Map Cooker";
			this.groupBoxOutput.ResumeLayout(false);
			this.panelInput.ResumeLayout(false);
			this.panelInput.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboBoxPlatform;
		private System.Windows.Forms.Button buttonCook;
		private System.Windows.Forms.RichTextBox richTextBoxOutput;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.TextBox textBoxMapName;
		private System.Windows.Forms.Button buttonLoadMap;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox groupBoxOutput;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBoxCommandLine;
		private System.Diagnostics.Process processCook;
		private System.Windows.Forms.Panel panelInput;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox comboBoxExecutable;
		private System.Windows.Forms.TextBox textBoxExecutablePath;
	}
}

