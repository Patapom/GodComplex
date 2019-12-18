namespace Brain2 {
	partial class ComplexTagNamesForm {
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
			this.groupBoxNames = new System.Windows.Forms.GroupBox();
			this.labelInfo = new System.Windows.Forms.Label();
			this.listViewTagNames = new System.Windows.Forms.ListView();
			this.groupBoxNames.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBoxNames
			// 
			this.groupBoxNames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxNames.Controls.Add(this.listViewTagNames);
			this.groupBoxNames.Location = new System.Drawing.Point(12, 85);
			this.groupBoxNames.Name = "groupBoxNames";
			this.groupBoxNames.Size = new System.Drawing.Size(465, 321);
			this.groupBoxNames.TabIndex = 0;
			this.groupBoxNames.TabStop = false;
			this.groupBoxNames.Text = "Complex Names";
			// 
			// labelInfo
			// 
			this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelInfo.Location = new System.Drawing.Point(12, 9);
			this.labelInfo.Name = "labelInfo";
			this.labelInfo.Size = new System.Drawing.Size(465, 73);
			this.labelInfo.TabIndex = 1;
			// 
			// listViewTagNames
			// 
			this.listViewTagNames.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewTagNames.LabelEdit = true;
			this.listViewTagNames.Location = new System.Drawing.Point(3, 16);
			this.listViewTagNames.MultiSelect = false;
			this.listViewTagNames.Name = "listViewTagNames";
			this.listViewTagNames.Size = new System.Drawing.Size(459, 302);
			this.listViewTagNames.TabIndex = 0;
			this.listViewTagNames.UseCompatibleStateImageBehavior = false;
			this.listViewTagNames.View = System.Windows.Forms.View.List;
			this.listViewTagNames.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listViewTagNames_AfterLabelEdit);
			// 
			// ComplexTagNamesForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(489, 418);
			this.Controls.Add(this.labelInfo);
			this.Controls.Add(this.groupBoxNames);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "ComplexTagNamesForm";
			this.Text = "Complex Tag Names Fixer";
			this.groupBoxNames.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBoxNames;
		private System.Windows.Forms.Label labelInfo;
		private System.Windows.Forms.ListView listViewTagNames;
	}
}