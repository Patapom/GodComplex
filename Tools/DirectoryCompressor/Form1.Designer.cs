﻿namespace DirectoryCompressor
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
			this.panelMain = new System.Windows.Forms.Panel();
			this.panelRight = new System.Windows.Forms.Panel();
			this.listBoxFilesRight = new System.Windows.Forms.ListBox();
			this.textBoxDirectoryRight = new System.Windows.Forms.TextBox();
			this.buttonBrowseDirectoryRight = new System.Windows.Forms.Button();
			this.panelLeft = new System.Windows.Forms.Panel();
			this.listBoxFilesLeft = new System.Windows.Forms.ListBox();
			this.textBoxDirectoryLeft = new System.Windows.Forms.TextBox();
			this.buttonBrowseDirectoryLeft = new System.Windows.Forms.Button();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.buttonCompare = new System.Windows.Forms.Button();
			this.labelFilesLeft = new System.Windows.Forms.Label();
			this.labelFilesRight = new System.Windows.Forms.Label();
			this.panelResults = new System.Windows.Forms.Panel();
			this.labelResults = new System.Windows.Forms.Label();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.panelMain.SuspendLayout();
			this.panelRight.SuspendLayout();
			this.panelLeft.SuspendLayout();
			this.panelResults.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelMain
			// 
			this.panelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelMain.Controls.Add(this.panelRight);
			this.panelMain.Controls.Add(this.panelLeft);
			this.panelMain.Location = new System.Drawing.Point(12, 12);
			this.panelMain.Name = "panelMain";
			this.panelMain.Size = new System.Drawing.Size(1049, 422);
			this.panelMain.TabIndex = 0;
			// 
			// panelRight
			// 
			this.panelRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelRight.Controls.Add(this.labelFilesRight);
			this.panelRight.Controls.Add(this.listBoxFilesRight);
			this.panelRight.Controls.Add(this.textBoxDirectoryRight);
			this.panelRight.Controls.Add(this.buttonBrowseDirectoryRight);
			this.panelRight.Location = new System.Drawing.Point(525, 3);
			this.panelRight.Name = "panelRight";
			this.panelRight.Size = new System.Drawing.Size(522, 416);
			this.panelRight.TabIndex = 0;
			// 
			// listBoxFilesRight
			// 
			this.listBoxFilesRight.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listBoxFilesRight.FormattingEnabled = true;
			this.listBoxFilesRight.IntegralHeight = false;
			this.listBoxFilesRight.Location = new System.Drawing.Point(2, 26);
			this.listBoxFilesRight.Name = "listBoxFilesRight";
			this.listBoxFilesRight.Size = new System.Drawing.Size(520, 369);
			this.listBoxFilesRight.TabIndex = 2;
			// 
			// textBoxDirectoryRight
			// 
			this.textBoxDirectoryRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxDirectoryRight.Location = new System.Drawing.Point(2, 0);
			this.textBoxDirectoryRight.Name = "textBoxDirectoryRight";
			this.textBoxDirectoryRight.ReadOnly = true;
			this.textBoxDirectoryRight.Size = new System.Drawing.Size(479, 20);
			this.textBoxDirectoryRight.TabIndex = 1;
			// 
			// buttonBrowseDirectoryRight
			// 
			this.buttonBrowseDirectoryRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonBrowseDirectoryRight.Location = new System.Drawing.Point(484, 0);
			this.buttonBrowseDirectoryRight.Name = "buttonBrowseDirectoryRight";
			this.buttonBrowseDirectoryRight.Size = new System.Drawing.Size(38, 21);
			this.buttonBrowseDirectoryRight.TabIndex = 0;
			this.buttonBrowseDirectoryRight.Text = "...";
			this.buttonBrowseDirectoryRight.UseVisualStyleBackColor = true;
			this.buttonBrowseDirectoryRight.Click += new System.EventHandler(this.buttonBrowseDirectoryRight_Click);
			// 
			// panelLeft
			// 
			this.panelLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelLeft.Controls.Add(this.labelFilesLeft);
			this.panelLeft.Controls.Add(this.listBoxFilesLeft);
			this.panelLeft.Controls.Add(this.textBoxDirectoryLeft);
			this.panelLeft.Controls.Add(this.buttonBrowseDirectoryLeft);
			this.panelLeft.Location = new System.Drawing.Point(3, 3);
			this.panelLeft.Name = "panelLeft";
			this.panelLeft.Size = new System.Drawing.Size(520, 416);
			this.panelLeft.TabIndex = 0;
			// 
			// listBoxFilesLeft
			// 
			this.listBoxFilesLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listBoxFilesLeft.FormattingEnabled = true;
			this.listBoxFilesLeft.IntegralHeight = false;
			this.listBoxFilesLeft.Location = new System.Drawing.Point(2, 26);
			this.listBoxFilesLeft.Name = "listBoxFilesLeft";
			this.listBoxFilesLeft.Size = new System.Drawing.Size(518, 369);
			this.listBoxFilesLeft.TabIndex = 2;
			// 
			// textBoxDirectoryLeft
			// 
			this.textBoxDirectoryLeft.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxDirectoryLeft.Location = new System.Drawing.Point(2, 0);
			this.textBoxDirectoryLeft.Name = "textBoxDirectoryLeft";
			this.textBoxDirectoryLeft.ReadOnly = true;
			this.textBoxDirectoryLeft.Size = new System.Drawing.Size(477, 20);
			this.textBoxDirectoryLeft.TabIndex = 1;
			// 
			// buttonBrowseDirectoryLeft
			// 
			this.buttonBrowseDirectoryLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonBrowseDirectoryLeft.Location = new System.Drawing.Point(482, 0);
			this.buttonBrowseDirectoryLeft.Name = "buttonBrowseDirectoryLeft";
			this.buttonBrowseDirectoryLeft.Size = new System.Drawing.Size(38, 21);
			this.buttonBrowseDirectoryLeft.TabIndex = 0;
			this.buttonBrowseDirectoryLeft.Text = "...";
			this.buttonBrowseDirectoryLeft.UseVisualStyleBackColor = true;
			this.buttonBrowseDirectoryLeft.Click += new System.EventHandler(this.buttonBrowseDirectoryLeft_Click);
			// 
			// folderBrowserDialog
			// 
			this.folderBrowserDialog.Description = "Select the folder for comparison";
			this.folderBrowserDialog.ShowNewFolderButton = false;
			// 
			// buttonCompare
			// 
			this.buttonCompare.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.buttonCompare.Enabled = false;
			this.buttonCompare.Location = new System.Drawing.Point(498, 440);
			this.buttonCompare.Name = "buttonCompare";
			this.buttonCompare.Size = new System.Drawing.Size(75, 23);
			this.buttonCompare.TabIndex = 1;
			this.buttonCompare.Text = "Compare";
			this.buttonCompare.UseVisualStyleBackColor = true;
			this.buttonCompare.Click += new System.EventHandler(this.buttonCompare_Click);
			// 
			// labelFilesLeft
			// 
			this.labelFilesLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelFilesLeft.AutoSize = true;
			this.labelFilesLeft.Location = new System.Drawing.Point(3, 398);
			this.labelFilesLeft.Name = "labelFilesLeft";
			this.labelFilesLeft.Size = new System.Drawing.Size(228, 13);
			this.labelFilesLeft.TabIndex = 3;
			this.labelFilesLeft.Text = "No files listed. Click the browse button above...";
			// 
			// labelFilesRight
			// 
			this.labelFilesRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelFilesRight.AutoSize = true;
			this.labelFilesRight.Location = new System.Drawing.Point(4, 398);
			this.labelFilesRight.Name = "labelFilesRight";
			this.labelFilesRight.Size = new System.Drawing.Size(228, 13);
			this.labelFilesRight.TabIndex = 3;
			this.labelFilesRight.Text = "No files listed. Click the browse button above...";
			// 
			// panelResults
			// 
			this.panelResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelResults.Controls.Add(this.labelResults);
			this.panelResults.Location = new System.Drawing.Point(12, 469);
			this.panelResults.Name = "panelResults";
			this.panelResults.Size = new System.Drawing.Size(1049, 212);
			this.panelResults.TabIndex = 2;
			// 
			// labelResults
			// 
			this.labelResults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelResults.AutoSize = true;
			this.labelResults.Location = new System.Drawing.Point(2, 0);
			this.labelResults.Name = "labelResults";
			this.labelResults.Size = new System.Drawing.Size(0, 13);
			this.labelResults.TabIndex = 3;
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar.Location = new System.Drawing.Point(579, 443);
			this.progressBar.Maximum = 1000;
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(480, 17);
			this.progressBar.TabIndex = 3;
			this.progressBar.Visible = false;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1073, 693);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.panelResults);
			this.Controls.Add(this.buttonCompare);
			this.Controls.Add(this.panelMain);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Directory Compressor";
			this.panelMain.ResumeLayout(false);
			this.panelRight.ResumeLayout(false);
			this.panelRight.PerformLayout();
			this.panelLeft.ResumeLayout(false);
			this.panelLeft.PerformLayout();
			this.panelResults.ResumeLayout(false);
			this.panelResults.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelMain;
		private System.Windows.Forms.Panel panelLeft;
		private System.Windows.Forms.TextBox textBoxDirectoryLeft;
		private System.Windows.Forms.Button buttonBrowseDirectoryLeft;
		private System.Windows.Forms.ListBox listBoxFilesLeft;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.Panel panelRight;
		private System.Windows.Forms.ListBox listBoxFilesRight;
		private System.Windows.Forms.TextBox textBoxDirectoryRight;
		private System.Windows.Forms.Button buttonBrowseDirectoryRight;
		private System.Windows.Forms.Button buttonCompare;
		private System.Windows.Forms.Label labelFilesLeft;
		private System.Windows.Forms.Label labelFilesRight;
		private System.Windows.Forms.Panel panelResults;
		private System.Windows.Forms.Label labelResults;
		private System.Windows.Forms.ProgressBar progressBar;

	}
}

