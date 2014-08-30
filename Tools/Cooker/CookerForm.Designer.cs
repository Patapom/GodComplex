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
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.textBoxExecutablePath = new System.Windows.Forms.TextBox();
			this.comboBoxExecutable = new System.Windows.Forms.ComboBox();
			this.buttonCustomExecutable = new System.Windows.Forms.Button();
			this.openFileDialogExecutable = new System.Windows.Forms.OpenFileDialog();
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
			this.buttonCook.Location = new System.Drawing.Point(181, 152);
			this.buttonCook.Name = "buttonCook";
			this.buttonCook.Size = new System.Drawing.Size(494, 34);
			this.buttonCook.TabIndex = 1;
			this.buttonCook.Text = "Cook";
			this.buttonCook.UseVisualStyleBackColor = true;
			this.buttonCook.Click += new System.EventHandler(this.buttonCook_Click);
			// 
			// richTextBoxOutput
			// 
			this.richTextBoxOutput.BackColor = System.Drawing.Color.Gainsboro;
			this.richTextBoxOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBoxOutput.Location = new System.Drawing.Point(3, 16);
			this.richTextBoxOutput.Name = "richTextBoxOutput";
			this.richTextBoxOutput.Size = new System.Drawing.Size(826, 494);
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
			this.textBoxMapName.Size = new System.Drawing.Size(740, 20);
			this.textBoxMapName.TabIndex = 4;
			// 
			// buttonLoadMap
			// 
			this.buttonLoadMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLoadMap.Location = new System.Drawing.Point(800, 6);
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
			this.groupBoxOutput.Location = new System.Drawing.Point(12, 192);
			this.groupBoxOutput.Name = "groupBoxOutput";
			this.groupBoxOutput.Size = new System.Drawing.Size(832, 513);
			this.groupBoxOutput.TabIndex = 0;
			this.groupBoxOutput.TabStop = false;
			this.groupBoxOutput.Text = "Output";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(218, 41);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(106, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "Command Line Prefix";
			// 
			// textBoxCommandLine
			// 
			this.textBoxCommandLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxCommandLine.Location = new System.Drawing.Point(330, 38);
			this.textBoxCommandLine.Multiline = true;
			this.textBoxCommandLine.Name = "textBoxCommandLine";
			this.textBoxCommandLine.Size = new System.Drawing.Size(505, 77);
			this.textBoxCommandLine.TabIndex = 2;
			this.textBoxCommandLine.Text = "+com_assertOutOfDebugger  1 +r_fullscreen 0 +win_crashDmp_enable 1 +fs_basepath \"" +
    "V:\\blacksparrow\\idtech5\\blacksparrow\"";
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
			this.panelInput.Controls.Add(this.textBox1);
			this.panelInput.Controls.Add(this.label1);
			this.panelInput.Controls.Add(this.label4);
			this.panelInput.Controls.Add(this.label2);
			this.panelInput.Controls.Add(this.textBoxCommandLine);
			this.panelInput.Controls.Add(this.label3);
			this.panelInput.Controls.Add(this.textBoxExecutablePath);
			this.panelInput.Controls.Add(this.textBoxMapName);
			this.panelInput.Controls.Add(this.comboBoxExecutable);
			this.panelInput.Controls.Add(this.comboBoxPlatform);
			this.panelInput.Controls.Add(this.buttonCustomExecutable);
			this.panelInput.Controls.Add(this.buttonLoadMap);
			this.panelInput.Location = new System.Drawing.Point(0, 1);
			this.panelInput.Name = "panelInput";
			this.panelInput.Size = new System.Drawing.Size(855, 145);
			this.panelInput.TabIndex = 6;
			// 
			// textBox1
			// 
			this.textBox1.BackColor = System.Drawing.SystemColors.Info;
			this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBox1.Location = new System.Drawing.Point(16, 60);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(308, 55);
			this.textBox1.TabIndex = 5;
			this.textBox1.Text = "NOTE: Command line is always appended with \"+com_production 1 +ark_useStdOut 1 +b" +
    "uildgame -fast \"";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(3, 124);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 13);
			this.label4.TabIndex = 0;
			this.label4.Text = "Executable";
			// 
			// textBoxExecutablePath
			// 
			this.textBoxExecutablePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxExecutablePath.Location = new System.Drawing.Point(181, 121);
			this.textBoxExecutablePath.Name = "textBoxExecutablePath";
			this.textBoxExecutablePath.ReadOnly = true;
			this.textBoxExecutablePath.Size = new System.Drawing.Size(613, 20);
			this.textBoxExecutablePath.TabIndex = 4;
			// 
			// comboBoxExecutable
			// 
			this.comboBoxExecutable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxExecutable.FormattingEnabled = true;
			this.comboBoxExecutable.Items.AddRange(new object[] {
            "Debug",
            "Release",
            "Perforce Latest",
            "Custom"});
			this.comboBoxExecutable.Location = new System.Drawing.Point(68, 121);
			this.comboBoxExecutable.Name = "comboBoxExecutable";
			this.comboBoxExecutable.Size = new System.Drawing.Size(107, 21);
			this.comboBoxExecutable.TabIndex = 3;
			this.comboBoxExecutable.SelectedIndexChanged += new System.EventHandler(this.comboBoxExecutable_SelectedIndexChanged);
			// 
			// buttonCustom
			// 
			this.buttonCustomExecutable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCustomExecutable.Enabled = false;
			this.buttonCustomExecutable.Location = new System.Drawing.Point(800, 118);
			this.buttonCustomExecutable.Name = "buttonCustom";
			this.buttonCustomExecutable.Size = new System.Drawing.Size(35, 24);
			this.buttonCustomExecutable.TabIndex = 0;
			this.buttonCustomExecutable.Text = "...";
			this.buttonCustomExecutable.UseVisualStyleBackColor = true;
			this.buttonCustomExecutable.Click += new System.EventHandler(this.buttonCustom_Click);
			// 
			// openFileDialogExecutable
			// 
			this.openFileDialogExecutable.DefaultExt = "*.exe";
			this.openFileDialogExecutable.Filter = "Executables (*.exe)|*.exe|All files|*.*";
			this.openFileDialogExecutable.Title = "Choose an executable...";
			// 
			// CookerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(856, 717);
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
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button buttonCustomExecutable;
		private System.Windows.Forms.OpenFileDialog openFileDialogExecutable;
	}
}

