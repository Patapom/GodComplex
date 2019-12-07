namespace ZetaHtmlEditControl.UI
{
    using EditControlDerives;

    partial class HtmlEditUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HtmlEditUserControl));
            this.topToolStrip = new System.Windows.Forms.ToolStrip();
            this.undoToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.textModulesToolStripItem = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.boldToolStripMenuItem = new System.Windows.Forms.ToolStripButton();
            this.italicToolStripMenuItem = new System.Windows.Forms.ToolStripButton();
            this.underlineToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.fontNameToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.fontSizeToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.bullettedListToolStripMenuItem = new System.Windows.Forms.ToolStripButton();
            this.numberedListToolStripMenuItem = new System.Windows.Forms.ToolStripButton();
            this.indentToolStripMenuItem = new System.Windows.Forms.ToolStripButton();
            this.outdentToolStripMenuItem = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.justifyLeftToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.justifyCenterToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.justifyRightToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.insertTableToolStripMenuItem = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.foreColorToolStripMenuItem = new System.Windows.Forms.ToolStripDropDownButton();
            this.foreColorNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.foreColor01ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.foreColor02ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.foreColor03ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.foreColor04ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.foreColor05ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.foreColor06ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.foreColor07ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.foreColor08ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.foreColor09ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.foreColor10ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backColorToolStripMenuItem = new System.Windows.Forms.ToolStripDropDownButton();
            this.BackColorNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.BackColor01ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BackColor02ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BackColor03ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BackColor04ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BackColor05ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeFormattingToolStripMenuItem = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.htmlEditControl = new ZetaHtmlEditControl.UI.EditControlDerives.HtmlEditControl();
            this.topToolStrip.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // topToolStrip
            // 
            resources.ApplyResources(this.topToolStrip, "topToolStrip");
            this.topToolStrip.GripMargin = new System.Windows.Forms.Padding(0);
            this.topToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.topToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripButton,
            this.textModulesToolStripItem,
            this.toolStripSeparator7,
            this.boldToolStripMenuItem,
            this.italicToolStripMenuItem,
            this.underlineToolStripButton,
            this.toolStripSeparator1,
            this.fontNameToolStripComboBox,
            this.fontSizeToolStripComboBox,
            this.toolStripSeparator8,
            this.bullettedListToolStripMenuItem,
            this.numberedListToolStripMenuItem,
            this.indentToolStripMenuItem,
            this.outdentToolStripMenuItem,
            this.toolStripSeparator2,
            this.justifyLeftToolStripButton,
            this.justifyCenterToolStripButton,
            this.justifyRightToolStripButton,
            this.toolStripSeparator6,
            this.insertTableToolStripMenuItem,
            this.toolStripSeparator3,
            this.foreColorToolStripMenuItem,
            this.backColorToolStripMenuItem,
            this.removeFormattingToolStripMenuItem});
            this.topToolStrip.Name = "topToolStrip";
            // 
            // undoToolStripButton
            // 
            this.undoToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.undoToolStripButton, "undoToolStripButton");
            this.undoToolStripButton.Name = "undoToolStripButton";
            this.undoToolStripButton.Click += new System.EventHandler(this.undoToolStripButton_Click);
            // 
            // textModulesToolStripItem
            // 
            this.textModulesToolStripItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.textModulesToolStripItem, "textModulesToolStripItem");
            this.textModulesToolStripItem.Name = "textModulesToolStripItem";
            this.textModulesToolStripItem.DropDownOpening += new System.EventHandler(this.textModulesToolStripItem_DropDownOpening);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            resources.ApplyResources(this.toolStripSeparator7, "toolStripSeparator7");
            // 
            // boldToolStripMenuItem
            // 
            this.boldToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.boldToolStripMenuItem, "boldToolStripMenuItem");
            this.boldToolStripMenuItem.Name = "boldToolStripMenuItem";
            this.boldToolStripMenuItem.Click += new System.EventHandler(this.boldToolStripMenuItem_Click);
            // 
            // italicToolStripMenuItem
            // 
            this.italicToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.italicToolStripMenuItem, "italicToolStripMenuItem");
            this.italicToolStripMenuItem.Name = "italicToolStripMenuItem";
            this.italicToolStripMenuItem.Click += new System.EventHandler(this.italicToolStripMenuItem_Click);
            // 
            // underlineToolStripButton
            // 
            this.underlineToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.underlineToolStripButton, "underlineToolStripButton");
            this.underlineToolStripButton.Name = "underlineToolStripButton";
            this.underlineToolStripButton.Click += new System.EventHandler(this.underlineToolStripButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // fontNameToolStripComboBox
            // 
            this.fontNameToolStripComboBox.DropDownWidth = 171;
            this.fontNameToolStripComboBox.Name = "fontNameToolStripComboBox";
            resources.ApplyResources(this.fontNameToolStripComboBox, "fontNameToolStripComboBox");
            // 
            // fontSizeToolStripComboBox
            // 
            resources.ApplyResources(this.fontSizeToolStripComboBox, "fontSizeToolStripComboBox");
            this.fontSizeToolStripComboBox.DropDownWidth = 30;
            this.fontSizeToolStripComboBox.Name = "fontSizeToolStripComboBox";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            resources.ApplyResources(this.toolStripSeparator8, "toolStripSeparator8");
            // 
            // bullettedListToolStripMenuItem
            // 
            this.bullettedListToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.bullettedListToolStripMenuItem, "bullettedListToolStripMenuItem");
            this.bullettedListToolStripMenuItem.Name = "bullettedListToolStripMenuItem";
            this.bullettedListToolStripMenuItem.Click += new System.EventHandler(this.bullettedListToolStripMenuItem_Click);
            // 
            // numberedListToolStripMenuItem
            // 
            this.numberedListToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.numberedListToolStripMenuItem, "numberedListToolStripMenuItem");
            this.numberedListToolStripMenuItem.Name = "numberedListToolStripMenuItem";
            this.numberedListToolStripMenuItem.Click += new System.EventHandler(this.numberedListToolStripMenuItem_Click);
            // 
            // indentToolStripMenuItem
            // 
            this.indentToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.indentToolStripMenuItem, "indentToolStripMenuItem");
            this.indentToolStripMenuItem.Name = "indentToolStripMenuItem";
            this.indentToolStripMenuItem.Click += new System.EventHandler(this.indentToolStripMenuItem_Click);
            // 
            // outdentToolStripMenuItem
            // 
            this.outdentToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.outdentToolStripMenuItem, "outdentToolStripMenuItem");
            this.outdentToolStripMenuItem.Name = "outdentToolStripMenuItem";
            this.outdentToolStripMenuItem.Click += new System.EventHandler(this.outdentToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // justifyLeftToolStripButton
            // 
            this.justifyLeftToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.justifyLeftToolStripButton, "justifyLeftToolStripButton");
            this.justifyLeftToolStripButton.Name = "justifyLeftToolStripButton";
            this.justifyLeftToolStripButton.Click += new System.EventHandler(this.justifyLeftToolStripButton_Click);
            // 
            // justifyCenterToolStripButton
            // 
            this.justifyCenterToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.justifyCenterToolStripButton, "justifyCenterToolStripButton");
            this.justifyCenterToolStripButton.Name = "justifyCenterToolStripButton";
            this.justifyCenterToolStripButton.Click += new System.EventHandler(this.justifyCenterToolStripButton_Click);
            // 
            // justifyRightToolStripButton
            // 
            this.justifyRightToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.justifyRightToolStripButton, "justifyRightToolStripButton");
            this.justifyRightToolStripButton.Name = "justifyRightToolStripButton";
            this.justifyRightToolStripButton.Click += new System.EventHandler(this.justifyRightToolStripButton_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
            // 
            // insertTableToolStripMenuItem
            // 
            this.insertTableToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.insertTableToolStripMenuItem, "insertTableToolStripMenuItem");
            this.insertTableToolStripMenuItem.Name = "insertTableToolStripMenuItem";
            this.insertTableToolStripMenuItem.Click += new System.EventHandler(this.insertTableToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // foreColorToolStripMenuItem
            // 
            this.foreColorToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.foreColorToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.foreColorNoneToolStripMenuItem,
            this.toolStripSeparator4,
            this.foreColor01ToolStripMenuItem,
            this.foreColor02ToolStripMenuItem,
            this.foreColor03ToolStripMenuItem,
            this.foreColor04ToolStripMenuItem,
            this.foreColor05ToolStripMenuItem,
            this.foreColor06ToolStripMenuItem,
            this.foreColor07ToolStripMenuItem,
            this.foreColor08ToolStripMenuItem,
            this.foreColor09ToolStripMenuItem,
            this.foreColor10ToolStripMenuItem});
            resources.ApplyResources(this.foreColorToolStripMenuItem, "foreColorToolStripMenuItem");
            this.foreColorToolStripMenuItem.Name = "foreColorToolStripMenuItem";
            // 
            // foreColorNoneToolStripMenuItem
            // 
            this.foreColorNoneToolStripMenuItem.Name = "foreColorNoneToolStripMenuItem";
            resources.ApplyResources(this.foreColorNoneToolStripMenuItem, "foreColorNoneToolStripMenuItem");
            this.foreColorNoneToolStripMenuItem.Click += new System.EventHandler(this.foreColorNoneToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // foreColor01ToolStripMenuItem
            // 
            resources.ApplyResources(this.foreColor01ToolStripMenuItem, "foreColor01ToolStripMenuItem");
            this.foreColor01ToolStripMenuItem.Name = "foreColor01ToolStripMenuItem";
            this.foreColor01ToolStripMenuItem.Click += new System.EventHandler(this.foreColor01ToolStripMenuItem_Click);
            // 
            // foreColor02ToolStripMenuItem
            // 
            resources.ApplyResources(this.foreColor02ToolStripMenuItem, "foreColor02ToolStripMenuItem");
            this.foreColor02ToolStripMenuItem.Name = "foreColor02ToolStripMenuItem";
            this.foreColor02ToolStripMenuItem.Click += new System.EventHandler(this.foreColor02ToolStripMenuItem_Click);
            // 
            // foreColor03ToolStripMenuItem
            // 
            resources.ApplyResources(this.foreColor03ToolStripMenuItem, "foreColor03ToolStripMenuItem");
            this.foreColor03ToolStripMenuItem.Name = "foreColor03ToolStripMenuItem";
            this.foreColor03ToolStripMenuItem.Click += new System.EventHandler(this.foreColor03ToolStripMenuItem_Click);
            // 
            // foreColor04ToolStripMenuItem
            // 
            resources.ApplyResources(this.foreColor04ToolStripMenuItem, "foreColor04ToolStripMenuItem");
            this.foreColor04ToolStripMenuItem.Name = "foreColor04ToolStripMenuItem";
            this.foreColor04ToolStripMenuItem.Click += new System.EventHandler(this.foreColor04ToolStripMenuItem_Click);
            // 
            // foreColor05ToolStripMenuItem
            // 
            resources.ApplyResources(this.foreColor05ToolStripMenuItem, "foreColor05ToolStripMenuItem");
            this.foreColor05ToolStripMenuItem.Name = "foreColor05ToolStripMenuItem";
            this.foreColor05ToolStripMenuItem.Click += new System.EventHandler(this.foreColor05ToolStripMenuItem_Click);
            // 
            // foreColor06ToolStripMenuItem
            // 
            resources.ApplyResources(this.foreColor06ToolStripMenuItem, "foreColor06ToolStripMenuItem");
            this.foreColor06ToolStripMenuItem.Name = "foreColor06ToolStripMenuItem";
            this.foreColor06ToolStripMenuItem.Click += new System.EventHandler(this.foreColor06ToolStripMenuItem_Click);
            // 
            // foreColor07ToolStripMenuItem
            // 
            resources.ApplyResources(this.foreColor07ToolStripMenuItem, "foreColor07ToolStripMenuItem");
            this.foreColor07ToolStripMenuItem.Name = "foreColor07ToolStripMenuItem";
            this.foreColor07ToolStripMenuItem.Click += new System.EventHandler(this.foreColor07ToolStripMenuItem_Click);
            // 
            // foreColor08ToolStripMenuItem
            // 
            resources.ApplyResources(this.foreColor08ToolStripMenuItem, "foreColor08ToolStripMenuItem");
            this.foreColor08ToolStripMenuItem.Name = "foreColor08ToolStripMenuItem";
            this.foreColor08ToolStripMenuItem.Click += new System.EventHandler(this.foreColor08ToolStripMenuItem_Click);
            // 
            // foreColor09ToolStripMenuItem
            // 
            resources.ApplyResources(this.foreColor09ToolStripMenuItem, "foreColor09ToolStripMenuItem");
            this.foreColor09ToolStripMenuItem.Name = "foreColor09ToolStripMenuItem";
            this.foreColor09ToolStripMenuItem.Click += new System.EventHandler(this.foreColor09ToolStripMenuItem_Click);
            // 
            // foreColor10ToolStripMenuItem
            // 
            resources.ApplyResources(this.foreColor10ToolStripMenuItem, "foreColor10ToolStripMenuItem");
            this.foreColor10ToolStripMenuItem.Name = "foreColor10ToolStripMenuItem";
            this.foreColor10ToolStripMenuItem.Click += new System.EventHandler(this.foreColor10ToolStripMenuItem_Click);
            // 
            // backColorToolStripMenuItem
            // 
            this.backColorToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.backColorToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BackColorNoneToolStripMenuItem,
            this.toolStripSeparator5,
            this.BackColor01ToolStripMenuItem,
            this.BackColor02ToolStripMenuItem,
            this.BackColor03ToolStripMenuItem,
            this.BackColor04ToolStripMenuItem,
            this.BackColor05ToolStripMenuItem});
            resources.ApplyResources(this.backColorToolStripMenuItem, "backColorToolStripMenuItem");
            this.backColorToolStripMenuItem.Name = "backColorToolStripMenuItem";
            // 
            // BackColorNoneToolStripMenuItem
            // 
            this.BackColorNoneToolStripMenuItem.Name = "BackColorNoneToolStripMenuItem";
            resources.ApplyResources(this.BackColorNoneToolStripMenuItem, "BackColorNoneToolStripMenuItem");
            this.BackColorNoneToolStripMenuItem.Click += new System.EventHandler(this.BackColorNoneToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            // 
            // BackColor01ToolStripMenuItem
            // 
            resources.ApplyResources(this.BackColor01ToolStripMenuItem, "BackColor01ToolStripMenuItem");
            this.BackColor01ToolStripMenuItem.Name = "BackColor01ToolStripMenuItem";
            this.BackColor01ToolStripMenuItem.Click += new System.EventHandler(this.BackColor01ToolStripMenuItem_Click);
            // 
            // BackColor02ToolStripMenuItem
            // 
            resources.ApplyResources(this.BackColor02ToolStripMenuItem, "BackColor02ToolStripMenuItem");
            this.BackColor02ToolStripMenuItem.Name = "BackColor02ToolStripMenuItem";
            this.BackColor02ToolStripMenuItem.Click += new System.EventHandler(this.BackColor02ToolStripMenuItem_Click);
            // 
            // BackColor03ToolStripMenuItem
            // 
            resources.ApplyResources(this.BackColor03ToolStripMenuItem, "BackColor03ToolStripMenuItem");
            this.BackColor03ToolStripMenuItem.Name = "BackColor03ToolStripMenuItem";
            this.BackColor03ToolStripMenuItem.Click += new System.EventHandler(this.BackColor03ToolStripMenuItem_Click);
            // 
            // BackColor04ToolStripMenuItem
            // 
            resources.ApplyResources(this.BackColor04ToolStripMenuItem, "BackColor04ToolStripMenuItem");
            this.BackColor04ToolStripMenuItem.Name = "BackColor04ToolStripMenuItem";
            this.BackColor04ToolStripMenuItem.Click += new System.EventHandler(this.BackColor04ToolStripMenuItem_Click);
            // 
            // BackColor05ToolStripMenuItem
            // 
            resources.ApplyResources(this.BackColor05ToolStripMenuItem, "BackColor05ToolStripMenuItem");
            this.BackColor05ToolStripMenuItem.Name = "BackColor05ToolStripMenuItem";
            this.BackColor05ToolStripMenuItem.Click += new System.EventHandler(this.BackColor05ToolStripMenuItem_Click);
            // 
            // removeFormattingToolStripMenuItem
            // 
            this.removeFormattingToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.removeFormattingToolStripMenuItem, "removeFormattingToolStripMenuItem");
            this.removeFormattingToolStripMenuItem.Name = "removeFormattingToolStripMenuItem";
            this.removeFormattingToolStripMenuItem.Click += new System.EventHandler(this.removeFormattingToolStripMenuItem_Click);
            // 
            // tableLayoutPanel
            // 
            resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
            this.tableLayoutPanel.Controls.Add(this.topToolStrip, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel1.Controls.Add(this.htmlEditControl);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // htmlEditControl
            // 
            this.htmlEditControl.AllowWebBrowserDrop = false;
            resources.ApplyResources(this.htmlEditControl, "htmlEditControl");
            this.htmlEditControl.IsWebBrowserContextMenuEnabled = false;
            this.htmlEditControl.Name = "htmlEditControl";
            // 
            // HtmlEditUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "HtmlEditUserControl";
            resources.ApplyResources(this, "$this");
            this.Load += new System.EventHandler(this.HtmlEditUserControl_Load);
            this.topToolStrip.ResumeLayout(false);
            this.topToolStrip.PerformLayout();
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStrip topToolStrip;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
		private System.Windows.Forms.ToolStripButton boldToolStripMenuItem;
		private System.Windows.Forms.Panel panel1;
		private HtmlEditControl htmlEditControl;
		private System.Windows.Forms.ToolStripButton italicToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton bullettedListToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton numberedListToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton outdentToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton indentToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton insertTableToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripDropDownButton foreColorToolStripMenuItem;
		private System.Windows.Forms.ToolStripDropDownButton backColorToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem foreColorNoneToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem foreColor01ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem foreColor02ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem foreColor03ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem foreColor04ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem foreColor05ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem foreColor06ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem foreColor07ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem foreColor08ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem foreColor09ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem foreColor10ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem BackColorNoneToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem BackColor01ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem BackColor02ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem BackColor03ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem BackColor04ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem BackColor05ToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton justifyLeftToolStripButton;
		private System.Windows.Forms.ToolStripButton justifyCenterToolStripButton;
		private System.Windows.Forms.ToolStripButton justifyRightToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
		private System.Windows.Forms.ToolStripButton undoToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
		private System.Windows.Forms.ToolStripButton underlineToolStripButton;
		private System.Windows.Forms.ToolStripDropDownButton textModulesToolStripItem;
        private System.Windows.Forms.ToolStripComboBox fontSizeToolStripComboBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripComboBox fontNameToolStripComboBox;
        private System.Windows.Forms.ToolStripButton removeFormattingToolStripMenuItem;
	}
}
