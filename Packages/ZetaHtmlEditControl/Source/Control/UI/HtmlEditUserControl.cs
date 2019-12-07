//Oleg Shilo 22.05.2013
// All code related to 
//    fontNameToolStripComboBox
//    fontNameToolStripComboBox
//    ZetaHtmlEditControl.ToolStrip
//    ZetaHtmlEditControl.IE10RenderingMode

namespace ZetaHtmlEditControl.UI
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Text;
    using System.Windows.Forms;
    using Code.Configuration;
    using EditControlDerives;

    public partial class HtmlEditUserControl : UserControl
    {
        private const string DefaultFontValue = "---";
        private readonly float _initialTopHeight;
        private bool _textModulesFilled;
        private int _updateCount;

        public HtmlEditUserControl()
        {
            IE10RenderingMode = true;

            InitializeComponent();

            if (!DesignMode && !HtmlEditorDesignModeManager.IsDesignMode)
            {
                fontNameToolStripComboBox.Items.Add(DefaultFontValue);
                foreach (var family in new InstalledFontCollection().Families)
                {
                    fontNameToolStripComboBox.Items.Add(family.Name);
                }

                fontSizeToolStripComboBox.Items.AddRange(new object[] { DefaultFontValue, "1", "2", "3", "4", "5", "6", "7" });

                fontNameToolStripComboBox.SelectedIndex =
                    fontSizeToolStripComboBox.SelectedIndex = 0;

                fontSizeToolStripComboBox.SelectedIndexChanged += fontSizeToolStripComboBox_SelectedIndexChanged;
                fontNameToolStripComboBox.SelectedIndexChanged += fontNameToolStripComboBox_SelectedIndexChanged;


                _initialTopHeight = tableLayoutPanel.RowStyles[0].Height;

                htmlEditControl.UINeedsUpdate += htmlEditControl_UINeedsUpdate;

                Configure(htmlEditControl.Configuration);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ToolStrip ToolStrip
        {
            get { return topToolStrip; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IE10RenderingMode { get; set; }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HtmlEditControl HtmlEditControl
        {
            get { return htmlEditControl; }
        }

        public bool IsToolbarVisible
        {
            get { return topToolStrip.Visible; }
            set
            {
                applyFont();

                // --

                if (topToolStrip.Visible != value)
                {
                    topToolStrip.Visible = value;

                    tableLayoutPanel.RowStyles[0].Height = value ? _initialTopHeight : 0;
                }
            }
        }

        void fontNameToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (htmlEditControl.Configuration.AllowFontChange)
            {
                var name = (string)fontNameToolStripComboBox.SelectedItem;
                if (!string.IsNullOrEmpty(name) && name != DefaultFontValue)
                {
                    htmlEditControl.ExecuteFontName(name);
                }
            }
        }

        void fontSizeToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (htmlEditControl.Configuration.AllowFontChange)
            {
                var size = (string)fontSizeToolStripComboBox.SelectedItem;
                if (!string.IsNullOrEmpty(size) && size != DefaultFontValue)
                {
                    htmlEditControl.ExecuteFontSize(size);
                }
            }
        }

        public void Configure(HtmlEditControlConfiguration configuration)
        {
            htmlEditControl.Configure(configuration);
            applyConfiguration();
        }

        private void applyConfiguration()
        {
            fontNameToolStripComboBox.Visible =
                fontSizeToolStripComboBox.Visible =
                toolStripSeparator8.Visible =
                htmlEditControl.Configuration.AllowFontChange;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            applyFont();
        }

        private void htmlEditControl_UINeedsUpdate(
            object sender,
            EventArgs e)
        {
            updateButtons();
        }

        private void applyFont()
        {
            if (!DesignMode && !HtmlEditorDesignModeManager.IsDesignMode)
            {
                if (htmlEditControl.Configuration != null &&
                htmlEditControl.Configuration.ExternalInformationProvider != null)
                {
                    var font = htmlEditControl.Configuration.ExternalInformationProvider.Font;
                    if (font != null)
                    {
                        Font = font;
                    }

                    if (htmlEditControl.Configuration.ExternalInformationProvider.ForeColor.HasValue)
                    {
                        ForeColor = htmlEditControl.Configuration.ExternalInformationProvider.ForeColor.Value;
                    }

                    if (htmlEditControl.Configuration.ExternalInformationProvider.ControlBorderColor.HasValue)
                    {
                        panel1.BackColor = htmlEditControl.Configuration.ExternalInformationProvider.ControlBorderColor.Value;
                    }
                }
            }

            topToolStrip.Font = Font;
        }

        private void updateButtons()
        {
            if (!DesignMode && !HtmlEditorDesignModeManager.IsDesignMode)
            {
                fontNameToolStripComboBox.Enabled =
                    fontSizeToolStripComboBox.Enabled = htmlEditControl.CanChangeFont;
                boldToolStripMenuItem.Enabled = htmlEditControl.CanBold;
                italicToolStripMenuItem.Enabled = htmlEditControl.CanItalic;
                underlineToolStripButton.Enabled = htmlEditControl.CanUnderline;
                bullettedListToolStripMenuItem.Enabled = htmlEditControl.CanBullettedList;
                numberedListToolStripMenuItem.Enabled = htmlEditControl.CanOrderedList;
                indentToolStripMenuItem.Enabled = htmlEditControl.CanIndent;
                outdentToolStripMenuItem.Enabled = htmlEditControl.CanOutdent;
                insertTableToolStripMenuItem.Enabled = htmlEditControl.CanInsertTable;
                foreColorToolStripMenuItem.Enabled = htmlEditControl.CanForeColor;
                backColorToolStripMenuItem.Enabled = htmlEditControl.CanBackColor;
                undoToolStripButton.Enabled = htmlEditControl.CanUndo;
                justifyLeftToolStripButton.Enabled = htmlEditControl.CanJustifyLeft;
                justifyCenterToolStripButton.Enabled = htmlEditControl.CanJustifyCenter;
                justifyRightToolStripButton.Enabled = htmlEditControl.CanJustifyRight;
                removeFormattingToolStripMenuItem.Enabled = htmlEditControl.CanRemoveFormatting;

                // --

                boldToolStripMenuItem.Checked = htmlEditControl.IsBold;
                italicToolStripMenuItem.Checked = htmlEditControl.IsItalic;
                underlineToolStripButton.Checked = htmlEditControl.IsUnderline;
                numberedListToolStripMenuItem.Checked = htmlEditControl.IsBullettedList;
                bullettedListToolStripMenuItem.Checked = htmlEditControl.IsOrderedList;
                justifyLeftToolStripButton.Checked = htmlEditControl.IsJustifyLeft;
                justifyCenterToolStripButton.Checked = htmlEditControl.IsJustifyCenter;
                justifyRightToolStripButton.Checked = htmlEditControl.IsJustifyRight;

                textModulesToolStripItem.Visible = htmlEditControl.HasTextModules;

                _updateCount++;

                // If WebBrowser is queried for the font properties at the initialization stage (updateCount==1)
                // then the rendering engine is pushed in a different rendering mode. 
                // 
                // This manifest itself in the following observable conditions:
                //   - Font=="Times New Roman"
                //   - Double-click triggers word selection
                //   - Spell-checker is disabled
                //   - AppDomain raises non-critical COM DRAGDROP_E_NOTREGISTERED Exception
                //   
                // While the default behaviour is:
                //   - Font=="Segoe UI"
                //   - Double-click does not trigger word selection
                //   - Spell-checker is enabled
                //   
                if (!IE10RenderingMode || _updateCount > 1)
                {
                    if (fontNameToolStripComboBox.Enabled)
                        fontNameToolStripComboBox.SelectedItem = htmlEditControl.FontName ?? DefaultFontValue;

                    if (fontSizeToolStripComboBox.Enabled)
                        fontSizeToolStripComboBox.SelectedIndex = htmlEditControl.FontSize; //0 means "default font"
                }
            }
        }

        private void boldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteBold();
        }

        private void italicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteItalic();
        }

        private void bullettedListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteBullettedList();
        }

        private void numberedListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteNumberedList();
        }

        private void indentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteIndent();
        }

        private void outdentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteOutdent();
        }

        private void insertTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteInsertTable();
        }

        private void foreColorNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetForeColorNone();
        }

        private void foreColor01ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetForeColor01();
        }

        private void foreColor02ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetForeColor02();
        }

        private void foreColor03ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetForeColor03();
        }

        private void foreColor04ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetForeColor04();
        }

        private void foreColor05ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetForeColor05();
        }

        private void foreColor06ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetForeColor06();
        }

        private void foreColor07ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetForeColor07();
        }

        private void foreColor08ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetForeColor08();
        }

        private void foreColor09ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetForeColor09();
        }

        private void foreColor10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetForeColor10();
        }

        private void BackColorNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetBackColorNone();
        }

        private void BackColor01ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetBackColor01();
        }

        private void BackColor02ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetBackColor02();
        }

        private void BackColor03ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetBackColor03();
        }

        private void BackColor04ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetBackColor04();
        }

        private void BackColor05ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteSetBackColor05();
        }

        private void HtmlEditUserControl_Load(object sender, EventArgs e)
        {
            updateButtons();
        }

        private void justifyLeftToolStripButton_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteJustifyLeft();
        }

        private void justifyCenterToolStripButton_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteJustifyCenter();
        }

        private void justifyRightToolStripButton_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteJustifyRight();
        }

        private void undoToolStripButton_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteUndo();
        }

        private void underlineToolStripButton_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteUnderline();
        }

        private void textModulesToolStripItem_DropDownOpening(object sender, EventArgs e)
        {
            if (!_textModulesFilled)
            {
                _textModulesFilled = true;
                htmlEditControl.FillTextModules(textModulesToolStripItem);
            }
        }

        private void removeFormattingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlEditControl.ExecuteRemoveFormatting();
        }
    }
}