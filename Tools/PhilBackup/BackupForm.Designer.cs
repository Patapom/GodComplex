namespace PhilBackup
{
	partial class BackupForm
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
			this.button1 = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.button3 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.buttonTarget = new System.Windows.Forms.Button();
			this.labelTarget = new System.Windows.Forms.Label();
			this.textBoxTarget = new System.Windows.Forms.TextBox();
			this.buttonStart = new System.Windows.Forms.Button();
			this.checkBoxCreateDateFolder = new System.Windows.Forms.CheckBox();
			this.panelInfos = new System.Windows.Forms.Panel();
			this.groupBox1.SuspendLayout();
			this.panelInfos.SuspendLayout();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Location = new System.Drawing.Point( 356, 21 );
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size( 30, 23 );
			this.button1.TabIndex = 0;
			this.button1.Text = "...";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler( this.button1_Click );
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add( this.textBox3 );
			this.groupBox1.Controls.Add( this.textBox2 );
			this.groupBox1.Controls.Add( this.textBox1 );
			this.groupBox1.Controls.Add( this.label3 );
			this.groupBox1.Controls.Add( this.label2 );
			this.groupBox1.Controls.Add( this.label1 );
			this.groupBox1.Controls.Add( this.button3 );
			this.groupBox1.Controls.Add( this.button2 );
			this.groupBox1.Controls.Add( this.button1 );
			this.groupBox1.Location = new System.Drawing.Point( 12, 13 );
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size( 404, 114 );
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Source Folders";
			// 
			// textBox3
			// 
			this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox3.Location = new System.Drawing.Point( 64, 81 );
			this.textBox3.Name = "textBox3";
			this.textBox3.ReadOnly = true;
			this.textBox3.Size = new System.Drawing.Size( 286, 20 );
			this.textBox3.TabIndex = 4;
			this.textBox3.WordWrap = false;
			// 
			// textBox2
			// 
			this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox2.Location = new System.Drawing.Point( 64, 52 );
			this.textBox2.Name = "textBox2";
			this.textBox2.ReadOnly = true;
			this.textBox2.Size = new System.Drawing.Size( 286, 20 );
			this.textBox2.TabIndex = 4;
			this.textBox2.WordWrap = false;
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.Location = new System.Drawing.Point( 64, 23 );
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size( 286, 20 );
			this.textBox1.TabIndex = 4;
			this.textBox1.WordWrap = false;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point( 6, 84 );
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size( 52, 13 );
			this.label3.TabIndex = 2;
			this.label3.Text = "Folder #3";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point( 6, 55 );
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size( 52, 13 );
			this.label2.TabIndex = 2;
			this.label2.Text = "Folder #2";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point( 6, 26 );
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size( 52, 13 );
			this.label1.TabIndex = 2;
			this.label1.Text = "Folder #1";
			// 
			// button3
			// 
			this.button3.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button3.Location = new System.Drawing.Point( 356, 79 );
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size( 30, 23 );
			this.button3.TabIndex = 0;
			this.button3.Text = "...";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler( this.button3_Click );
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.Location = new System.Drawing.Point( 356, 50 );
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size( 30, 23 );
			this.button2.TabIndex = 0;
			this.button2.Text = "...";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler( this.button2_Click );
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.Location = new System.Drawing.Point( 12, 233 );
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size( 406, 16 );
			this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.progressBar1.TabIndex = 3;
			// 
			// buttonTarget
			// 
			this.buttonTarget.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonTarget.Location = new System.Drawing.Point( 386, 145 );
			this.buttonTarget.Name = "buttonTarget";
			this.buttonTarget.Size = new System.Drawing.Size( 30, 23 );
			this.buttonTarget.TabIndex = 0;
			this.buttonTarget.Text = "...";
			this.buttonTarget.UseVisualStyleBackColor = true;
			this.buttonTarget.Click += new System.EventHandler( this.buttonTarget_Click );
			// 
			// labelTarget
			// 
			this.labelTarget.AutoSize = true;
			this.labelTarget.Location = new System.Drawing.Point( 9, 150 );
			this.labelTarget.Name = "labelTarget";
			this.labelTarget.Size = new System.Drawing.Size( 70, 13 );
			this.labelTarget.TabIndex = 2;
			this.labelTarget.Text = "Target Folder";
			// 
			// textBoxTarget
			// 
			this.textBoxTarget.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxTarget.Location = new System.Drawing.Point( 88, 147 );
			this.textBoxTarget.Name = "textBoxTarget";
			this.textBoxTarget.ReadOnly = true;
			this.textBoxTarget.Size = new System.Drawing.Size( 292, 20 );
			this.textBoxTarget.TabIndex = 4;
			this.textBoxTarget.WordWrap = false;
			// 
			// buttonStart
			// 
			this.buttonStart.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.buttonStart.Enabled = false;
			this.buttonStart.Location = new System.Drawing.Point( 163, 204 );
			this.buttonStart.Name = "buttonStart";
			this.buttonStart.Size = new System.Drawing.Size( 105, 23 );
			this.buttonStart.TabIndex = 0;
			this.buttonStart.Text = "Start Backup";
			this.buttonStart.UseVisualStyleBackColor = true;
			this.buttonStart.Click += new System.EventHandler( this.buttonStart_Click );
			// 
			// checkBoxCreateDateFolder
			// 
			this.checkBoxCreateDateFolder.AutoSize = true;
			this.checkBoxCreateDateFolder.Checked = true;
			this.checkBoxCreateDateFolder.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxCreateDateFolder.Location = new System.Drawing.Point( 12, 173 );
			this.checkBoxCreateDateFolder.Name = "checkBoxCreateDateFolder";
			this.checkBoxCreateDateFolder.Size = new System.Drawing.Size( 207, 17 );
			this.checkBoxCreateDateFolder.TabIndex = 5;
			this.checkBoxCreateDateFolder.Text = "Create target folder with today\'s date ?";
			this.checkBoxCreateDateFolder.UseVisualStyleBackColor = true;
			// 
			// panelInfos
			// 
			this.panelInfos.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panelInfos.Controls.Add( this.checkBoxCreateDateFolder );
			this.panelInfos.Controls.Add( this.groupBox1 );
			this.panelInfos.Controls.Add( this.textBoxTarget );
			this.panelInfos.Controls.Add( this.labelTarget );
			this.panelInfos.Controls.Add( this.buttonTarget );
			this.panelInfos.Location = new System.Drawing.Point( 0, -1 );
			this.panelInfos.Name = "panelInfos";
			this.panelInfos.Size = new System.Drawing.Size( 429, 192 );
			this.panelInfos.TabIndex = 6;
			// 
			// BackupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 430, 261 );
			this.Controls.Add( this.progressBar1 );
			this.Controls.Add( this.buttonStart );
			this.Controls.Add( this.panelInfos );
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximumSize = new System.Drawing.Size( 1000, 295 );
			this.MinimumSize = new System.Drawing.Size( 446, 295 );
			this.Name = "BackupForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Phil Backup";
			this.groupBox1.ResumeLayout( false );
			this.groupBox1.PerformLayout();
			this.panelInfos.ResumeLayout( false );
			this.panelInfos.PerformLayout();
			this.ResumeLayout( false );

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.TextBox textBox3;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button buttonTarget;
		private System.Windows.Forms.Label labelTarget;
		private System.Windows.Forms.TextBox textBoxTarget;
		private System.Windows.Forms.Button buttonStart;
		private System.Windows.Forms.CheckBox checkBoxCreateDateFolder;
		private System.Windows.Forms.Panel panelInfos;
	}
}

