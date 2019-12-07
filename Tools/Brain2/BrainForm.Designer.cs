namespace Brain2 {
	partial class BrainForm {
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
			this.floatTrackbarControl = new UIUtility.FloatTrackbarControl();
			this.buttonGo = new System.Windows.Forms.Button();
			this.textBoxURL = new System.Windows.Forms.TextBox();
			this.panelOutput = new System.Windows.Forms.Panel();
			this.buttonEdit = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// floatTrackbarControl
			// 
			this.floatTrackbarControl.Location = new System.Drawing.Point(893, 45);
			this.floatTrackbarControl.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControl.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControl.Name = "floatTrackbarControl";
			this.floatTrackbarControl.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControl.TabIndex = 0;
			this.floatTrackbarControl.Value = 0F;
			// 
			// buttonGo
			// 
			this.buttonGo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonGo.Location = new System.Drawing.Point(1128, 10);
			this.buttonGo.Name = "buttonGo";
			this.buttonGo.Size = new System.Drawing.Size(55, 23);
			this.buttonGo.TabIndex = 2;
			this.buttonGo.Text = "Go";
			this.buttonGo.UseVisualStyleBackColor = true;
			this.buttonGo.Click += new System.EventHandler(this.buttonGo_Click);
			// 
			// textBoxURL
			// 
			this.textBoxURL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxURL.Location = new System.Drawing.Point(811, 12);
			this.textBoxURL.Name = "textBoxURL";
			this.textBoxURL.Size = new System.Drawing.Size(311, 20);
			this.textBoxURL.TabIndex = 3;
			this.textBoxURL.Text = "www.patapom.com/blog";
			// 
			// panelOutput
			// 
			this.panelOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelOutput.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelOutput.Location = new System.Drawing.Point(12, 12);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(793, 661);
			this.panelOutput.TabIndex = 4;
			// 
			// buttonEdit
			// 
			this.buttonEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonEdit.Location = new System.Drawing.Point(933, 147);
			this.buttonEdit.Name = "buttonEdit";
			this.buttonEdit.Size = new System.Drawing.Size(75, 23);
			this.buttonEdit.TabIndex = 5;
			this.buttonEdit.Text = "Edit";
			this.buttonEdit.UseVisualStyleBackColor = true;
			this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
			// 
			// BrainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1190, 685);
			this.Controls.Add(this.buttonEdit);
			this.Controls.Add(this.panelOutput);
			this.Controls.Add(this.textBoxURL);
			this.Controls.Add(this.buttonGo);
			this.Controls.Add(this.floatTrackbarControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "BrainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Brain #2";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UIUtility.FloatTrackbarControl floatTrackbarControl;
		private System.Windows.Forms.Button buttonGo;
		private System.Windows.Forms.TextBox textBoxURL;
		private System.Windows.Forms.Panel panelOutput;
		private System.Windows.Forms.Button buttonEdit;
	}
}

