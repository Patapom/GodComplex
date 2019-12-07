namespace ZetaHtmlEditControl.UI.Tools
{
	partial class HtmlSourceTextEditForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HtmlSourceTextEditForm));
			this.textboxEdit = new System.Windows.Forms.TextBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.wordWrapCheckBox = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// textboxEdit
			// 
			this.textboxEdit.AcceptsReturn = true;
			this.textboxEdit.AcceptsTab = true;
			resources.ApplyResources(this.textboxEdit, "textboxEdit");
			this.textboxEdit.Name = "textboxEdit";
			this.textboxEdit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textboxEdit_KeyDown);
			// 
			// buttonCancel
			// 
			resources.ApplyResources(this.buttonCancel, "buttonCancel");
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonOK
			// 
			resources.ApplyResources(this.buttonOK, "buttonOK");
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// wordWrapCheckBox
			// 
			resources.ApplyResources(this.wordWrapCheckBox, "wordWrapCheckBox");
			this.wordWrapCheckBox.Name = "wordWrapCheckBox";
			this.wordWrapCheckBox.UseVisualStyleBackColor = true;
			this.wordWrapCheckBox.CheckedChanged += new System.EventHandler(this.wordWrapCheckBox_CheckedChanged);
			// 
			// HtmlSourceTextEditForm
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.buttonCancel;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.wordWrapCheckBox);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.textboxEdit);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "HtmlSourceTextEditForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.HtmlSourceTextEditForm_Load);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HtmlSourceTextEditForm_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textboxEdit;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.CheckBox wordWrapCheckBox;
	}
}