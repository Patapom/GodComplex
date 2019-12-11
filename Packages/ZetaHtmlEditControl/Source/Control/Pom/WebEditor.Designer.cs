namespace ZetaHtmlEditControl.Pom {
	partial class WebEditor {
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.toolStripButtonBold = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonItalic = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonLink = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonImage = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonBulletList = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonNumberedList = new System.Windows.Forms.ToolStripButton();
			this.toolStripComboBoxStyle = new System.Windows.Forms.ToolStripComboBox();
			this.htmlEditControl = new ZetaHtmlEditControl.UI.EditControlDerives.HtmlEditControl();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip
			// 
			this.toolStrip.AutoSize = false;
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonBold,
            this.toolStripButtonItalic,
            this.toolStripButtonLink,
            this.toolStripButtonImage,
            this.toolStripButtonBulletList,
            this.toolStripButtonNumberedList,
            this.toolStripComboBoxStyle});
			this.toolStrip.Location = new System.Drawing.Point(0, 0);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = new System.Drawing.Size(1069, 38);
			this.toolStrip.TabIndex = 0;
			this.toolStrip.Text = "toolStrip1";
			// 
			// toolStripButtonBold
			// 
			this.toolStripButtonBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonBold.Image = global::ZetaHtmlEditControl.Properties.Resources.ButtonBold;
			this.toolStripButtonBold.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonBold.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonBold.Name = "toolStripButtonBold";
			this.toolStripButtonBold.Size = new System.Drawing.Size(36, 35);
			this.toolStripButtonBold.Text = "Bold";
			this.toolStripButtonBold.Click += new System.EventHandler(this.toolStripButtonBold_Click);
			// 
			// toolStripButtonItalic
			// 
			this.toolStripButtonItalic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonItalic.Image = global::ZetaHtmlEditControl.Properties.Resources.ButtonItalic;
			this.toolStripButtonItalic.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonItalic.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonItalic.Name = "toolStripButtonItalic";
			this.toolStripButtonItalic.Size = new System.Drawing.Size(36, 35);
			this.toolStripButtonItalic.Text = "toolStripButton2";
			this.toolStripButtonItalic.ToolTipText = "Italic";
			this.toolStripButtonItalic.Click += new System.EventHandler(this.toolStripButtonItalic_Click);
			// 
			// toolStripButtonLink
			// 
			this.toolStripButtonLink.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonLink.Image = global::ZetaHtmlEditControl.Properties.Resources.ButtonLink;
			this.toolStripButtonLink.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonLink.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonLink.Name = "toolStripButtonLink";
			this.toolStripButtonLink.Size = new System.Drawing.Size(36, 35);
			this.toolStripButtonLink.Text = "Link";
			this.toolStripButtonLink.ToolTipText = "Link";
			this.toolStripButtonLink.Click += new System.EventHandler(this.toolStripButtonLink_Click);
			// 
			// toolStripButtonImage
			// 
			this.toolStripButtonImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonImage.Image = global::ZetaHtmlEditControl.Properties.Resources.ButtonImage;
			this.toolStripButtonImage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonImage.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonImage.Name = "toolStripButtonImage";
			this.toolStripButtonImage.Size = new System.Drawing.Size(36, 35);
			this.toolStripButtonImage.Text = "Image";
			this.toolStripButtonImage.Click += new System.EventHandler(this.toolStripButtonImage_Click);
			// 
			// toolStripButtonBulletList
			// 
			this.toolStripButtonBulletList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonBulletList.Image = global::ZetaHtmlEditControl.Properties.Resources.ButtonBulletList;
			this.toolStripButtonBulletList.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonBulletList.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonBulletList.Name = "toolStripButtonBulletList";
			this.toolStripButtonBulletList.Size = new System.Drawing.Size(36, 35);
			this.toolStripButtonBulletList.Text = "Bullet List";
			this.toolStripButtonBulletList.Click += new System.EventHandler(this.toolStripButtonBulletList_Click);
			// 
			// toolStripButtonNumberedList
			// 
			this.toolStripButtonNumberedList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonNumberedList.Image = global::ZetaHtmlEditControl.Properties.Resources.ButtonNumberedList;
			this.toolStripButtonNumberedList.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonNumberedList.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonNumberedList.Name = "toolStripButtonNumberedList";
			this.toolStripButtonNumberedList.Size = new System.Drawing.Size(36, 35);
			this.toolStripButtonNumberedList.Text = "Numbered List";
			this.toolStripButtonNumberedList.Click += new System.EventHandler(this.toolStripButtonNumberedList_Click);
			// 
			// toolStripComboBoxStyle
			// 
			this.toolStripComboBoxStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.toolStripComboBoxStyle.Items.AddRange(new object[] {
            "Normal",
            "Header 1",
            "Header 2",
            "Header 3",
            "Header 4"});
			this.toolStripComboBoxStyle.Name = "toolStripComboBoxStyle";
			this.toolStripComboBoxStyle.Size = new System.Drawing.Size(121, 38);
			// 
			// htmlEditControl
			// 
			this.htmlEditControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.htmlEditControl.IsWebBrowserContextMenuEnabled = false;
			this.htmlEditControl.Location = new System.Drawing.Point(0, 38);
			this.htmlEditControl.MinimumSize = new System.Drawing.Size(20, 20);
			this.htmlEditControl.Name = "htmlEditControl";
			this.htmlEditControl.Size = new System.Drawing.Size(1069, 339);
			this.htmlEditControl.TabIndex = 1;
			this.htmlEditControl.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.htmlEditControl_PreviewKeyDown);
			// 
			// WebEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.htmlEditControl);
			this.Controls.Add(this.toolStrip);
			this.Name = "WebEditor";
			this.Size = new System.Drawing.Size(1069, 377);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton toolStripButtonBold;
		private System.Windows.Forms.ToolStripButton toolStripButtonItalic;
		private System.Windows.Forms.ToolStripButton toolStripButtonLink;
		private System.Windows.Forms.ToolStripButton toolStripButtonImage;
		private System.Windows.Forms.ToolStripButton toolStripButtonBulletList;
		private System.Windows.Forms.ToolStripButton toolStripButtonNumberedList;
		private System.Windows.Forms.ToolStripComboBox toolStripComboBoxStyle;
		private ZetaHtmlEditControl.UI.EditControlDerives.HtmlEditControl htmlEditControl;
	}
}
