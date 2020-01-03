namespace Brain2 {
	partial class TagEditBox {
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

			this.InternalDispose();

			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.toolTipTag = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// toolTipTag
			// 
			this.toolTipTag.AutoPopDelay = 5000;
			this.toolTipTag.InitialDelay = 100;
			this.toolTipTag.ReshowDelay = 100;
			this.toolTipTag.Popup += new System.Windows.Forms.PopupEventHandler(this.toolTipTag_Popup);
			// 
			// TagEditBox
			// 
			this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.TabStop = true;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolTip toolTipTag;
	}
}
