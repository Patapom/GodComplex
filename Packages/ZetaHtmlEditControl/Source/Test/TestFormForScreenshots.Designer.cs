namespace Test
{
    using ZetaHtmlEditControl.UI;

    partial class TestFormForScreenshots
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestFormForScreenshots));
            this.ToolbarVisibleCheckBox = new System.Windows.Forms.CheckBox();
            this.htmlEditUserControl1 = new ZetaHtmlEditControl.UI.HtmlEditUserControl();
            this.infoTextBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ToolbarVisibleCheckBox
            // 
            this.ToolbarVisibleCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ToolbarVisibleCheckBox.AutoSize = true;
            this.ToolbarVisibleCheckBox.Checked = true;
            this.ToolbarVisibleCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToolbarVisibleCheckBox.Location = new System.Drawing.Point(12, 650);
            this.ToolbarVisibleCheckBox.Name = "ToolbarVisibleCheckBox";
            this.ToolbarVisibleCheckBox.Size = new System.Drawing.Size(113, 21);
            this.ToolbarVisibleCheckBox.TabIndex = 2;
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
            this.htmlEditUserControl1.Size = new System.Drawing.Size(630, 601);
            this.htmlEditUserControl1.TabIndex = 0;
            // 
            // infoTextBox
            // 
            this.infoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.infoTextBox.Location = new System.Drawing.Point(12, 619);
            this.infoTextBox.Name = "infoTextBox";
            this.infoTextBox.ReadOnly = true;
            this.infoTextBox.Size = new System.Drawing.Size(630, 25);
            this.infoTextBox.TabIndex = 1;
            this.infoTextBox.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(278, 648);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // TestFormForScreenshots
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(654, 683);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.infoTextBox);
            this.Controls.Add(this.ToolbarVisibleCheckBox);
            this.Controls.Add(this.htmlEditUserControl1);
            this.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(525, 410);
            this.Name = "TestFormForScreenshots";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Test form for the Zeta Html Edit Control";
            this.Load += new System.EventHandler(this.TestForm_Load);
            this.Shown += new System.EventHandler(this.TestForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private HtmlEditUserControl htmlEditUserControl1;
        private System.Windows.Forms.CheckBox ToolbarVisibleCheckBox;
        private System.Windows.Forms.TextBox infoTextBox;
        private System.Windows.Forms.Button button1;
	}
}

