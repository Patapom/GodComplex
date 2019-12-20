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
			this.label3 = new System.Windows.Forms.Label();
			this.labelShortcutToggle = new System.Windows.Forms.Label();
			this.buttonImportBookmarks = new System.Windows.Forms.Button();
			this.groupBoxImport = new System.Windows.Forms.GroupBox();
			this.listViewBookmarks = new System.Windows.Forms.ListView();
			this.label4 = new System.Windows.Forms.Label();
			this.folderBrowserDialogBookmarks = new System.Windows.Forms.FolderBrowserDialog();
			this.openFileDialogBookmarks = new System.Windows.Forms.OpenFileDialog();
			this.label5 = new System.Windows.Forms.Label();
			this.labelShortcutPaste = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.labelShortcutNew = new System.Windows.Forms.Label();
			this.groupBoxImport.SuspendLayout();
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
			this.label2.Location = new System.Drawing.Point(14, 49);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(111, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Database Root Folder";
			// 
			// textBoxDatabaseRoot
			// 
			this.textBoxDatabaseRoot.Location = new System.Drawing.Point(131, 46);
			this.textBoxDatabaseRoot.Name = "textBoxDatabaseRoot";
			this.textBoxDatabaseRoot.ReadOnly = true;
			this.textBoxDatabaseRoot.Size = new System.Drawing.Size(349, 20);
			this.textBoxDatabaseRoot.TabIndex = 2;
			// 
			// buttonSelectRootDBFolder
			// 
			this.buttonSelectRootDBFolder.Location = new System.Drawing.Point(486, 44);
			this.buttonSelectRootDBFolder.Name = "buttonSelectRootDBFolder";
			this.buttonSelectRootDBFolder.Size = new System.Drawing.Size(34, 23);
			this.buttonSelectRootDBFolder.TabIndex = 3;
			this.buttonSelectRootDBFolder.Text = "...";
			this.buttonSelectRootDBFolder.UseVisualStyleBackColor = true;
			this.buttonSelectRootDBFolder.Click += new System.EventHandler(this.buttonSelectRootDBFolder_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(14, 75);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(116, 13);
			this.label3.TabIndex = 1;
			this.label3.Text = "Global Shortcut Toggle";
			// 
			// labelShortcutToggle
			// 
			this.labelShortcutToggle.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.labelShortcutToggle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelShortcutToggle.Location = new System.Drawing.Point(131, 70);
			this.labelShortcutToggle.Name = "labelShortcutToggle";
			this.labelShortcutToggle.Size = new System.Drawing.Size(100, 23);
			this.labelShortcutToggle.TabIndex = 4;
			this.labelShortcutToggle.Text = "Win+X";
			this.labelShortcutToggle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelShortcutToggle.Click += new System.EventHandler(this.labelShortcutToggle_Click);
			// 
			// buttonImportBookmarks
			// 
			this.buttonImportBookmarks.Enabled = false;
			this.buttonImportBookmarks.Location = new System.Drawing.Point(413, 58);
			this.buttonImportBookmarks.Name = "buttonImportBookmarks";
			this.buttonImportBookmarks.Size = new System.Drawing.Size(90, 54);
			this.buttonImportBookmarks.TabIndex = 5;
			this.buttonImportBookmarks.Text = "Import Selected Bookmarks";
			this.buttonImportBookmarks.UseVisualStyleBackColor = true;
			this.buttonImportBookmarks.Click += new System.EventHandler(this.buttonImportBookmarks_Click);
			// 
			// groupBoxImport
			// 
			this.groupBoxImport.Controls.Add(this.listViewBookmarks);
			this.groupBoxImport.Controls.Add(this.label4);
			this.groupBoxImport.Controls.Add(this.buttonImportBookmarks);
			this.groupBoxImport.Location = new System.Drawing.Point(17, 319);
			this.groupBoxImport.Name = "groupBoxImport";
			this.groupBoxImport.Size = new System.Drawing.Size(512, 118);
			this.groupBoxImport.TabIndex = 6;
			this.groupBoxImport.TabStop = false;
			this.groupBoxImport.Text = "Import Bookmarks";
			// 
			// listViewBookmarks
			// 
			this.listViewBookmarks.HideSelection = false;
			this.listViewBookmarks.Location = new System.Drawing.Point(6, 58);
			this.listViewBookmarks.Name = "listViewBookmarks";
			this.listViewBookmarks.Size = new System.Drawing.Size(401, 54);
			this.listViewBookmarks.TabIndex = 7;
			this.listViewBookmarks.UseCompatibleStateImageBehavior = false;
			this.listViewBookmarks.View = System.Windows.Forms.View.List;
			this.listViewBookmarks.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewBookmarks_ItemSelectionChanged);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(7, 25);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(496, 30);
			this.label4.TabIndex = 6;
			this.label4.Text = "Brain 2 can import bookmarks from well known web browsers and automatically creat" +
    "e fiches for each one.The following bookmarks have been located on your machine:" +
    "";
			// 
			// folderBrowserDialogBookmarks
			// 
			this.folderBrowserDialogBookmarks.Description = "Choose bookmarks folder";
			// 
			// openFileDialogBookmarks
			// 
			this.openFileDialogBookmarks.DefaultExt = "xml";
			this.openFileDialogBookmarks.Filter = "Bookmark Files (*.xml)|*.xml|All Files (*.xml)|*.*";
			this.openFileDialogBookmarks.Title = "Select the bookmark files to import";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(14, 103);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(110, 13);
			this.label5.TabIndex = 1;
			this.label5.Text = "Global Shortcut Paste";
			// 
			// labelShortcutPaste
			// 
			this.labelShortcutPaste.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.labelShortcutPaste.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelShortcutPaste.Location = new System.Drawing.Point(131, 98);
			this.labelShortcutPaste.Name = "labelShortcutPaste";
			this.labelShortcutPaste.Size = new System.Drawing.Size(100, 23);
			this.labelShortcutPaste.TabIndex = 4;
			this.labelShortcutPaste.Text = "Win+V";
			this.labelShortcutPaste.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelShortcutPaste.Click += new System.EventHandler(this.labelShortcutPaste_Click);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(14, 131);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(105, 13);
			this.label6.TabIndex = 1;
			this.label6.Text = "Global Shortcut New";
			// 
			// labelShortcutNew
			// 
			this.labelShortcutNew.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.labelShortcutNew.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelShortcutNew.Location = new System.Drawing.Point(131, 126);
			this.labelShortcutNew.Name = "labelShortcutNew";
			this.labelShortcutNew.Size = new System.Drawing.Size(100, 23);
			this.labelShortcutNew.TabIndex = 4;
			this.labelShortcutNew.Text = "Win+N";
			this.labelShortcutNew.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelShortcutNew.Click += new System.EventHandler(this.labelShortcutNew_Click);
			// 
			// PreferencesForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(541, 449);
			this.Controls.Add(this.groupBoxImport);
			this.Controls.Add(this.labelShortcutNew);
			this.Controls.Add(this.labelShortcutPaste);
			this.Controls.Add(this.labelShortcutToggle);
			this.Controls.Add(this.buttonSelectRootDBFolder);
			this.Controls.Add(this.textBoxDatabaseRoot);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.KeyPreview = true;
			this.Name = "PreferencesForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "PreferencesForm";
			this.groupBoxImport.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBoxDatabaseRoot;
		private System.Windows.Forms.Button buttonSelectRootDBFolder;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label labelShortcutToggle;
		private System.Windows.Forms.Button buttonImportBookmarks;
		private System.Windows.Forms.GroupBox groupBoxImport;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogBookmarks;
		private System.Windows.Forms.OpenFileDialog openFileDialogBookmarks;
		private System.Windows.Forms.ListView listViewBookmarks;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label labelShortcutPaste;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label labelShortcutNew;
	}
}