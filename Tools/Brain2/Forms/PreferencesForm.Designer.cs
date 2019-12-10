namespace Brain2 {
	partial class PreferencesForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxDatabaseRoot = new System.Windows.Forms.TextBox();
			this.buttonSelectRootDBFolder = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// folderBrowserDialog
			// 
			this.folderBrowserDialog.Description = "Choose a folder for the database of fiches";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(155, 29);
			this.label1.TabIndex = 0;
			this.label1.Text = "Preferences";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(14, 65);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(111, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Database Root Folder";
			// 
			// textBoxDatabaseRoot
			// 
			this.textBoxDatabaseRoot.Location = new System.Drawing.Point(131, 62);
			this.textBoxDatabaseRoot.Name = "textBoxDatabaseRoot";
			this.textBoxDatabaseRoot.ReadOnly = true;
			this.textBoxDatabaseRoot.Size = new System.Drawing.Size(349, 20);
			this.textBoxDatabaseRoot.TabIndex = 2;
			// 
			// buttonSelectRootDBFolder
			// 
			this.buttonSelectRootDBFolder.Location = new System.Drawing.Point(486, 60);
			this.buttonSelectRootDBFolder.Name = "buttonSelectRootDBFolder";
			this.buttonSelectRootDBFolder.Size = new System.Drawing.Size(34, 23);
			this.buttonSelectRootDBFolder.TabIndex = 3;
			this.buttonSelectRootDBFolder.Text = "...";
			this.buttonSelectRootDBFolder.UseVisualStyleBackColor = true;
			this.buttonSelectRootDBFolder.Click += new System.EventHandler(this.buttonSelectRootDBFolder_Click);
			// 
			// PreferencesForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(696, 504);
			this.Controls.Add(this.buttonSelectRootDBFolder);
			this.Controls.Add(this.textBoxDatabaseRoot);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "PreferencesForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "PreferencesForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBoxDatabaseRoot;
		private System.Windows.Forms.Button buttonSelectRootDBFolder;
	}
}