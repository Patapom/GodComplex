namespace ZetaHtmlEditControl.UI.EditControlDerives
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;
    using Code.Configuration;
    using Code.PInvoke;
    using EditControlBases;
    using Helper;

    /// <summary>
    /// Edit control, primarily designed to work in conjunction
    /// with the ZetaHelpdesk application.
    /// </summary>
    /// <remarks>
    /// Oleg Shilo 22.05.2013, all code related to:
    ///   - EmbeddImages
    ///   - FontSize
    ///   - FontName
    ///   - PrintPreview
    ///   - Print
    /// </remarks>
    public partial class HtmlEditControl :
        CoreHtmlEditControl
    {
        private bool _everLoadedTextModules;
        private bool _firstCreate = true;
        private int _objectID = 1;
        private TextModuleInfo[] _textModules;
        private bool _textModulesFilled;
        private Timer _timerTextChange = new Timer();
        private string _tmpCacheTextChange = string.Empty;
        private string _tmpFolderPath = string.Empty;

        public HtmlEditControl()
        {
            InitializeComponent();

            if (!DesignMode && !HtmlEditorDesignModeManager.IsDesignMode)
            {
                AllowWebBrowserDrop = false;

                Navigate(@"about:blank");

                _tmpFolderPath = Path.Combine(Path.GetTempPath(), @"zhe1-"+ Guid.NewGuid());
                Directory.CreateDirectory(_tmpFolderPath);

                _timerTextChange.Tick += timerTextChange_Tick;
                _timerTextChange.Interval = 200;
                _timerTextChange.Start();

                // --

                constructHtmlEditControlKeyboard();

                Configure(Configuration);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TextChangeCheckInterval
        {
            get { return _timerTextChange.Interval; }
            set
            {
                if (value < 1000) //1 min
                {
                    _timerTextChange.Interval = value;
                }
            }
        }

        public bool HasTextModules
        {
            get
            {
                checkGetTextModules();
                return _textModules != null && _textModules.Length > 0;
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (!DesignMode && !HtmlEditorDesignModeManager.IsDesignMode)
            {
                if (_timerTextChange != null)
                {
                    _timerTextChange.Stop();
                    _timerTextChange.Dispose();
                    _timerTextChange = null;
                }

                if (!string.IsNullOrEmpty(_tmpFolderPath))
                {
                    if (Directory.Exists(_tmpFolderPath))
                    {
                        Directory.Delete(_tmpFolderPath, true);
                    }
                    _tmpFolderPath = null;
                }
            }

            base.OnHandleDestroyed(e);
        }

        public override void Configure(HtmlEditControlConfiguration configuration)
        {
            base.Configure(configuration);

            _everLoadedTextModules = false; // Reset to force reload.
            updateUI();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            if (!DesignMode && !HtmlEditorDesignModeManager.IsDesignMode)
            {
                if (Document != null)
                {
                    if (Document.Body != null)
                    {
                        Document.Body.Focus();
                    }
                }
            }
        }

        protected override void OnNavigated(WebBrowserNavigatedEventArgs e)
        {
            base.OnNavigated(e);

            if (!DesignMode && !HtmlEditorDesignModeManager.IsDesignMode)
            {
                if (_firstCreate)
                {
                    _firstCreate = false;

                    // 2012-08-28, Uwe Keim: Enable gray shortcut texts.
                    contextMenuStrip.Renderer = new MyToolStripRender();
                }
            }
        }

        private void timerTextChange_Tick(
            object sender,
            EventArgs e)
        {
            if (!DesignMode && !HtmlEditorDesignModeManager.IsDesignMode) {
                if (!IsDisposed) // Uwe Keim 2006-03-17.
            {
                var s = DocumentText ?? string.Empty;

                if (_tmpCacheTextChange != s)
                {
                    _tmpCacheTextChange = s;

                    var h = TextChanged;
                    if (h != null) h(this, new EventArgs());
                }
            }}
        }

        public new event EventHandler TextChanged;

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            updateUI();
        }

        private void updateUI()
        {
            if (Document != null && Document.DomDocument != null)
            {
                boldToolStripMenuItem.Enabled = CanBold;
                italicToolStripMenuItem.Enabled = CanItalic;
                cutToolStripMenuItem.Enabled = CanCut;
                copyToolStripMenuItem.Enabled = CanCopy;
                pasteAsTextToolStripMenuItem.Enabled = CanPaste;
                pasteToolStripMenuItem.Enabled = CanPaste;
                pasteFromMsWordToolStripItem.Enabled = CanPaste;
                deleteToolStripMenuItem.Enabled = CanDelete;
                indentToolStripMenuItem.Enabled = CanIndent;
                justifyCenterToolStripMenuItem.Enabled = CanJustifyCenter;
                justifyLeftToolStripMenuItem.Enabled = CanJustifyLeft;
                justifyRightToolStripMenuItem.Enabled = CanJustifyRight;
                numberedListToolStripMenuItem.Enabled = CanOrderedList;
                outdentToolStripMenuItem.Enabled = CanOutdent;
                bullettedListToolStripMenuItem.Enabled = CanBullettedList;
                foreColorToolStripMenuItem.Enabled = CanForeColor;
                backColorToolStripMenuItem.Enabled = CanBackColor;
                hyperLinkToolStripMenuItem.Enabled = CanInsertHyperlink;
                htmlToolStripMenuItem.Enabled = CanShowSource;
                removeFormattingToolStripMenuItem.Enabled = CanRemoveFormatting;

                // --
                // Table menu.

                insertNewTableToolStripMenuItem.Enabled = CanInsertTable;
                insertRowBeforeCurrentRowToolStripMenuItem.Enabled = CanInsertTableRow;
                insertColumnBeforeCurrentColumnToolStripMenuItem.Enabled = CanInsertTableColumn;
                addRowAfterTheLastTableRowToolStripMenuItem.Enabled = CanAddTableRow;
                addColumnAfterTheLastTableColumnToolStripMenuItem.Enabled = CanAddTableColumn;
                tablePropertiesToolStripMenuItem.Enabled = CanTableProperties;
                rowPropertiesToolStripMenuItem.Enabled = CanTableRowProperties;
                columnPropertiesToolStripMenuItem.Enabled = CanTableColumnProperties;
                cellPropertiesToolStripMenuItem.Enabled = CanTableCellProperties;
                deleteRowToolStripMenuItem.Enabled = CanTableDeleteRow;
                deleteColumnToolStripMenuItem.Enabled = CanTableDeleteColumn;
                deleteTableToolStripMenuItem.Enabled = CanTableDeleteTable;

                // --

                textModulesToolStripMenuItem.Visible = HasTextModules;
                textModulesSeparator.Visible = HasTextModules;
            }
        }

        private void boldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteBold();
        }

        private void italicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteItalic();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecutePaste();
        }

        private void pasteAsTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecutePasteAsText();
        }

        private void htmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteShowSource();
        }

        private void hyperLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteInsertHyperlink();
        }

        private void indentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteIndent();
        }

        private void justifyCenterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteJustifyCenter();
        }

        private void justifyLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteJustifyLeft();
        }

        private void justifyRightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteJustifyRight();
        }

        private void numberedListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteNumberedList();
        }

        private void outdentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteOutdent();
        }

        private void bullettedListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteBullettedList();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteCopy();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteCut();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteDelete();
        }

        private void foreColorNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetForeColorNone();
        }

        private void foreColor01ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetForeColor01();
        }

        private void foreColor02ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetForeColor02();
        }

        private void foreColor03ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetForeColor03();
        }

        private void foreColor04ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetForeColor04();
        }

        private void foreColor05ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetForeColor05();
        }

        private void foreColor06ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetForeColor06();
        }

        private void foreColor07ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetForeColor07();
        }

        private void foreColor08ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetForeColor08();
        }

        private void foreColor09ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetForeColor09();
        }

        private void foreColor10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetForeColor10();
        }

        private void backColorNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetBackColorNone();
        }

        private void backColor01ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetBackColor01();
        }

        private void backColor02ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetBackColor02();
        }

        private void backColor03ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetBackColor03();
        }

        private void backColor04ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetBackColor04();
        }

        private void backColor05ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSetBackColor05();
        }

        private void insertNewTableToolStripMenuItem_Click(
            object sender,
            EventArgs e)
        {
            ExecuteInsertTable();
        }

        private void insertRowBeforeCurrentRowToolStripMenuItem_Click(
            object sender,
            EventArgs e)
        {
            ExecuteInsertTableRow();
        }

        private void insertColumnBeforeCurrentColumnToolStripMenuItem_Click(
            object sender,
            EventArgs e)
        {
            ExecuteInsertTableColumn();
        }

        private void addRowAfterTheLastTableRowToolStripMenuItem_Click(
            object sender,
            EventArgs e)
        {
            ExecuteTableAddTableRow();
        }

        private void addColumnAfterTheLastTableColumnToolStripMenuItem_Click(
            object sender,
            EventArgs e)
        {
            ExecuteTableAddTableColumn();
        }

        private void tablePropertiesToolStripMenuItem_Click(
            object sender,
            EventArgs e)
        {
            ExecuteTableProperties();
        }

        private void rowPropertiesToolStripMenuItem_Click(
            object sender,
            EventArgs e)
        {
            ExecuteTableRowProperties();
        }

        private void columnPropertiesToolStripMenuItem_Click(
            object sender,
            EventArgs e)
        {
            ExecuteTableColumnProperties();
        }

        private void cellPropertiesToolStripMenuItem_Click(
            object sender,
            EventArgs e)
        {
            ExecuteTableCellProperties();
        }

        private void deleteRowToolStripMenuItem_Click(
            object sender,
            EventArgs e)
        {
            ExecuteTableDeleteRow();
        }

        private void deleteColumnToolStripMenuItem_Click(
            object sender,
            EventArgs e)
        {
            ExecuteTableDeleteColumn();
        }

        private void deleteTableToolStripMenuItem_Click(
            object sender,
            EventArgs e)
        {
            ExecuteTableDeleteTable();
        }

        protected override void OnUpdateUI()
        {
            if (!DesignMode && !HtmlEditorDesignModeManager.IsDesignMode)
            {
                var h = UINeedsUpdate;
                if (h != null) h(this, EventArgs.Empty);
            }
        }

        public event EventHandler UINeedsUpdate;

        internal void FillTextModules(
            ToolStripDropDownItem textModulesToolStripItem)
        {
            checkGetTextModules();

            textModulesToolStripItem.DropDownItems.Clear();

            foreach (var textModule in _textModules)
            {
                var mi = new ToolStripMenuItem(textModule.Name) { Tag = textModule };

                mi.Click += delegate
                {
                    var tm = (TextModuleInfo)mi.Tag;
                    InsertHtmlAtCurrentSelection(tm.Html);
                };

                textModulesToolStripItem.DropDownItems.Add(mi);
            }
        }

        private void checkGetTextModules()
        {
            if (Configuration != null && Configuration.ExternalInformationProvider != null && !_everLoadedTextModules)
            {
                _everLoadedTextModules = true;
                _textModules = Configuration.ExternalInformationProvider.GetTextModules();
            }
        }

        private void textModulesToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            checkFillTextModules(textModulesToolStripMenuItem);
        }

        private void checkFillTextModules(ToolStripDropDownItem toolStripMenuItem)
        {
            if (!_textModulesFilled)
            {
                _textModulesFilled = true;
                FillTextModules(toolStripMenuItem);
            }
        }

        protected override bool OnNeedShowContextMenu(
            NativeMethods.ContextMenuKind contextMenuKind,
            Point position,
            NativeMethods.IUnknown queryForStatus,
            NativeMethods.IDispatch objectAtScreenCoordinates)
        {
            base.OnNeedShowContextMenu(contextMenuKind, position, queryForStatus, objectAtScreenCoordinates);

            if (!DesignMode && !HtmlEditorDesignModeManager.IsDesignMode)
            {
                if (Configuration != null && Configuration.ExternalInformationProvider != null)
                {
                    var font = Configuration.ExternalInformationProvider.Font;
                    contextMenuStrip.Font = font ?? Font;

                    if (Configuration.ExternalInformationProvider.ForeColor.HasValue)
                    {
                        contextMenuStrip.ForeColor = Configuration.ExternalInformationProvider.ForeColor.Value;
                    }
                }
                else
                {
                    contextMenuStrip.Font = Font;
                }

                contextMenuStrip.Show(position);
            }

            return true;
        }

        private void underlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteUnderline();
        }

        private void pasteFromMsWordToolStripItem_Click(object sender, EventArgs e)
        {
            ExecutePasteFromWord();
        }

        private void removeFormattingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteRemoveFormatting();
        }
    }
}