namespace ZetaHtmlEditControl.UI.Tools
{
	partial class HtmlEditorCellPropertiesForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HtmlEditorCellPropertiesForm));
			this.panel1 = new System.Windows.Forms.Panel();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.noWrapCheckBox = new System.Windows.Forms.CheckBox();
			this.containsHeadlineCheckBox = new System.Windows.Forms.CheckBox();
			this.verticalAlignmentComboBox = new System.Windows.Forms.ComboBox();
			this.horizontalAlignmentComboBox = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.heightDropDownControl = new System.Windows.Forms.ComboBox();
			this.widthDropDownControl = new System.Windows.Forms.ComboBox();
			this.heightTextBox = new System.Windows.Forms.NumericUpDown();
			this.widthTextBox = new System.Windows.Forms.NumericUpDown();
			this.defineHeightCheckBox = new System.Windows.Forms.CheckBox();
			this.defineWidthCheckBox = new System.Windows.Forms.CheckBox();
			this.panel1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.heightTextBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.widthTextBox)).BeginInit();
			this.SuspendLayout();
			// 
			// panel1
			// 
			resources.ApplyResources(this.panel1, "panel1");
			this.panel1.Controls.Add(this.buttonCancel);
			this.panel1.Controls.Add(this.buttonOK);
			this.panel1.Controls.Add(this.groupBox2);
			this.panel1.Controls.Add(this.groupBox1);
			this.panel1.Name = "panel1";
			// 
			// buttonCancel
			// 
			resources.ApplyResources(this.buttonCancel, "buttonCancel");
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			resources.ApplyResources(this.buttonOK, "buttonOK");
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// groupBox2
			// 
			resources.ApplyResources(this.groupBox2, "groupBox2");
			this.groupBox2.Controls.Add(this.noWrapCheckBox);
			this.groupBox2.Controls.Add(this.containsHeadlineCheckBox);
			this.groupBox2.Controls.Add(this.verticalAlignmentComboBox);
			this.groupBox2.Controls.Add(this.horizontalAlignmentComboBox);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.TabStop = false;
			// 
			// noWrapCheckBox
			// 
			resources.ApplyResources(this.noWrapCheckBox, "noWrapCheckBox");
			this.noWrapCheckBox.Name = "noWrapCheckBox";
			this.noWrapCheckBox.UseVisualStyleBackColor = true;
			this.noWrapCheckBox.CheckedChanged += new System.EventHandler(this.noWrapCheckBox_CheckedChanged);
			// 
			// containsHeadlineCheckBox
			// 
			resources.ApplyResources(this.containsHeadlineCheckBox, "containsHeadlineCheckBox");
			this.containsHeadlineCheckBox.Name = "containsHeadlineCheckBox";
			this.containsHeadlineCheckBox.UseVisualStyleBackColor = true;
			this.containsHeadlineCheckBox.CheckedChanged += new System.EventHandler(this.containsHeadlineCheckBox_CheckedChanged);
			// 
			// verticalAlignmentComboBox
			// 
			resources.ApplyResources(this.verticalAlignmentComboBox, "verticalAlignmentComboBox");
			this.verticalAlignmentComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.verticalAlignmentComboBox.FormattingEnabled = true;
			this.verticalAlignmentComboBox.Name = "verticalAlignmentComboBox";
			// 
			// horizontalAlignmentComboBox
			// 
			resources.ApplyResources(this.horizontalAlignmentComboBox, "horizontalAlignmentComboBox");
			this.horizontalAlignmentComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.horizontalAlignmentComboBox.FormattingEnabled = true;
			this.horizontalAlignmentComboBox.Name = "horizontalAlignmentComboBox";
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// groupBox1
			// 
			resources.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Controls.Add(this.heightDropDownControl);
			this.groupBox1.Controls.Add(this.widthDropDownControl);
			this.groupBox1.Controls.Add(this.heightTextBox);
			this.groupBox1.Controls.Add(this.widthTextBox);
			this.groupBox1.Controls.Add(this.defineHeightCheckBox);
			this.groupBox1.Controls.Add(this.defineWidthCheckBox);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			// 
			// heightDropDownControl
			// 
			resources.ApplyResources(this.heightDropDownControl, "heightDropDownControl");
			this.heightDropDownControl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.heightDropDownControl.FormattingEnabled = true;
			this.heightDropDownControl.Items.AddRange(new object[] {
            resources.GetString("heightDropDownControl.Items"),
            resources.GetString("heightDropDownControl.Items1")});
			this.heightDropDownControl.Name = "heightDropDownControl";
			// 
			// widthDropDownControl
			// 
			resources.ApplyResources(this.widthDropDownControl, "widthDropDownControl");
			this.widthDropDownControl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.widthDropDownControl.FormattingEnabled = true;
			this.widthDropDownControl.Items.AddRange(new object[] {
            resources.GetString("widthDropDownControl.Items"),
            resources.GetString("widthDropDownControl.Items1")});
			this.widthDropDownControl.Name = "widthDropDownControl";
			// 
			// heightTextBox
			// 
			resources.ApplyResources(this.heightTextBox, "heightTextBox");
			this.heightTextBox.Name = "heightTextBox";
			// 
			// widthTextBox
			// 
			resources.ApplyResources(this.widthTextBox, "widthTextBox");
			this.widthTextBox.Name = "widthTextBox";
			// 
			// defineHeightCheckBox
			// 
			resources.ApplyResources(this.defineHeightCheckBox, "defineHeightCheckBox");
			this.defineHeightCheckBox.Name = "defineHeightCheckBox";
			this.defineHeightCheckBox.UseVisualStyleBackColor = true;
			this.defineHeightCheckBox.CheckedChanged += new System.EventHandler(this.defineHeightCheckBox_CheckedChanged);
			// 
			// defineWidthCheckBox
			// 
			resources.ApplyResources(this.defineWidthCheckBox, "defineWidthCheckBox");
			this.defineWidthCheckBox.Name = "defineWidthCheckBox";
			this.defineWidthCheckBox.UseVisualStyleBackColor = true;
			this.defineWidthCheckBox.CheckedChanged += new System.EventHandler(this.defineWidthCheckBox_CheckedChanged);
			// 
			// HtmlEditorCellPropertiesForm
			// 
			this.AcceptButton = this.buttonOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.buttonCancel;
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "HtmlEditorCellPropertiesForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.HtmlEditorCellPropertiesForm_Load);
			this.panel1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.heightTextBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.widthTextBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox heightDropDownControl;
		private System.Windows.Forms.ComboBox widthDropDownControl;
		private System.Windows.Forms.NumericUpDown heightTextBox;
		private System.Windows.Forms.NumericUpDown widthTextBox;
		private System.Windows.Forms.CheckBox defineHeightCheckBox;
		private System.Windows.Forms.CheckBox defineWidthCheckBox;
		private System.Windows.Forms.CheckBox noWrapCheckBox;
		private System.Windows.Forms.CheckBox containsHeadlineCheckBox;
		private System.Windows.Forms.ComboBox verticalAlignmentComboBox;
		private System.Windows.Forms.ComboBox horizontalAlignmentComboBox;
		private System.Windows.Forms.Label label2;
	}
}