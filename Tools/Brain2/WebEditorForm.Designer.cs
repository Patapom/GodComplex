namespace Brain2 {
	partial class WebEditorForm {
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
			this.webEditor1 = new Brain2.Web_Interface.WebEditor();
			this.SuspendLayout();
			// 
			// webEditor1
			// 
			this.webEditor1.Location = new System.Drawing.Point(26, 44);
			this.webEditor1.Name = "webEditor1";
			this.webEditor1.Size = new System.Drawing.Size(495, 484);
			this.webEditor1.TabIndex = 0;
			// 
			// WebEditorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(832, 600);
			this.Controls.Add(this.webEditor1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "WebEditorForm";
			this.Text = "Fiche Editor";
			this.ResumeLayout(false);

		}

		#endregion

		private Web_Interface.WebEditor webEditor1;
	}
}