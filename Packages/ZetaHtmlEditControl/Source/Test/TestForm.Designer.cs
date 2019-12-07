namespace Test
{
    using ZetaHtmlEditControl.UI;
    using ZetaHtmlEditControl.UI.EditControlDerives;

    partial class TestForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.buttonGetHtml = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.htmlEditControl1 = new ZetaHtmlEditControl.UI.EditControlDerives.HtmlEditControl();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.ToolbarVisibleCheckBox = new System.Windows.Forms.CheckBox();
            this.htmlEditUserControl1 = new ZetaHtmlEditControl.UI.HtmlEditUserControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.button3 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.Location = new System.Drawing.Point(12, 235);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(108, 26);
            this.button1.TabIndex = 2;
            this.button1.Text = "Set text 2";
            this.button1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.Location = new System.Drawing.Point(12, 203);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(108, 26);
            this.button2.TabIndex = 1;
            this.button2.Text = "Set text 1";
            this.button2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // buttonGetHtml
            // 
            this.buttonGetHtml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonGetHtml.Image = ((System.Drawing.Image)(resources.GetObject("buttonGetHtml.Image")));
            this.buttonGetHtml.Location = new System.Drawing.Point(135, 203);
            this.buttonGetHtml.Name = "buttonGetHtml";
            this.buttonGetHtml.Size = new System.Drawing.Size(98, 26);
            this.buttonGetHtml.TabIndex = 3;
            this.buttonGetHtml.Text = "Get &HTML";
            this.buttonGetHtml.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonGetHtml.UseVisualStyleBackColor = true;
            this.buttonGetHtml.Click += new System.EventHandler(this.buttonGetHtml_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel1.Controls.Add(this.htmlEditControl1);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(1);
            this.panel1.Size = new System.Drawing.Size(453, 185);
            this.panel1.TabIndex = 1;
            // 
            // htmlEditControl1
            // 
            this.htmlEditControl1.AllowWebBrowserDrop = false;
            this.htmlEditControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.htmlEditControl1.IsWebBrowserContextMenuEnabled = false;
            this.htmlEditControl1.Location = new System.Drawing.Point(1, 1);
            this.htmlEditControl1.Name = "htmlEditControl1";
            this.htmlEditControl1.Size = new System.Drawing.Size(451, 183);
            this.htmlEditControl1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(468, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "This is a test form for the Zeta Html Edit Control. Please use the buttons below." +
    "";
            // 
            // buttonLoad
            // 
            this.buttonLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonLoad.Image = ((System.Drawing.Image)(resources.GetObject("buttonLoad.Image")));
            this.buttonLoad.Location = new System.Drawing.Point(248, 203);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(98, 26);
            this.buttonLoad.TabIndex = 4;
            this.buttonLoad.Text = "Load...";
            this.buttonLoad.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSave.Image = ((System.Drawing.Image)(resources.GetObject("buttonSave.Image")));
            this.buttonSave.Location = new System.Drawing.Point(248, 235);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(98, 26);
            this.buttonSave.TabIndex = 5;
            this.buttonSave.Text = "Save...";
            this.buttonSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 33);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(695, 519);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.ToolbarVisibleCheckBox);
            this.tabPage1.Controls.Add(this.htmlEditUserControl1);
            this.tabPage1.Location = new System.Drawing.Point(4, 26);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(9);
            this.tabPage1.Size = new System.Drawing.Size(687, 489);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "With tool bar";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // ToolbarVisibleCheckBox
            // 
            this.ToolbarVisibleCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ToolbarVisibleCheckBox.AutoSize = true;
            this.ToolbarVisibleCheckBox.Checked = true;
            this.ToolbarVisibleCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToolbarVisibleCheckBox.Location = new System.Drawing.Point(12, 428);
            this.ToolbarVisibleCheckBox.Name = "ToolbarVisibleCheckBox";
            this.ToolbarVisibleCheckBox.Size = new System.Drawing.Size(113, 21);
            this.ToolbarVisibleCheckBox.TabIndex = 1;
            this.ToolbarVisibleCheckBox.Text = "Toolbar visible";
            this.ToolbarVisibleCheckBox.UseVisualStyleBackColor = true;
            this.ToolbarVisibleCheckBox.CheckedChanged += new System.EventHandler(this.ToolbarVisibleCheckBox_CheckedChanged);
            // 
            // htmlEditUserControl1
            // 
            this.htmlEditUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.htmlEditUserControl1.IsToolbarVisible = false;
            this.htmlEditUserControl1.Location = new System.Drawing.Point(12, 12);
            this.htmlEditUserControl1.Name = "htmlEditUserControl1";
            this.htmlEditUserControl1.Size = new System.Drawing.Size(663, 414);
            this.htmlEditUserControl1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panel1);
            this.tabPage2.Controls.Add(this.button1);
            this.tabPage2.Controls.Add(this.button2);
            this.tabPage2.Controls.Add(this.buttonSave);
            this.tabPage2.Controls.Add(this.buttonGetHtml);
            this.tabPage2.Controls.Add(this.buttonLoad);
            this.tabPage2.Location = new System.Drawing.Point(4, 26);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(9);
            this.tabPage2.Size = new System.Drawing.Size(687, 489);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Without tool bar";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(632, 12);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 0;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // TestForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(719, 564);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(525, 410);
            this.Name = "TestForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Test form for the Zeta Html Edit Control";
            this.Load += new System.EventHandler(this.TestForm_Load);
            this.Shown += new System.EventHandler(this.TestForm_Shown);
            this.panel1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private HtmlEditControl htmlEditControl1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button buttonGetHtml;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonLoad;
		private System.Windows.Forms.Button buttonSave;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private HtmlEditUserControl htmlEditUserControl1;
		private System.Windows.Forms.CheckBox ToolbarVisibleCheckBox;
		private System.Windows.Forms.Button button3;
	}
}

