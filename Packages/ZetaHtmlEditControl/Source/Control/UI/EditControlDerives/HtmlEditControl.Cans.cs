namespace ZetaHtmlEditControl.UI.EditControlDerives
{
    using mshtml;

    /// <summary>
    /// Zentrale Stelle für grundlegende kann/kann-nicht und aktuelle Zustände abfragen.
    /// </summary>
    public partial class HtmlEditControl
    {
        public bool IsBold
        {
            get { return (bool)DomDocument.queryCommandValue(@"Bold"); }
        }

        public bool IsItalic
        {
            get { return (bool)DomDocument.queryCommandValue(@"Italic"); }
        }

        public bool IsUnderline
        {
            get { return (bool)DomDocument.queryCommandValue(@"Underline"); }
        }

        public bool IsOrderedList
        {
            get { return (bool)DomDocument.queryCommandValue(@"InsertOrderedList"); }
        }

        public bool IsBullettedList
        {
            get { return (bool)DomDocument.queryCommandValue(@"InsertUnorderedList"); }
        }

        public int FontSize
        {
            get
            {
                var value = DomDocument.queryCommandValue(@"FontSize");
                return (value is int) ? (int)value : 0;
            }
        }

        public string FontName
        {
            get { return (string)DomDocument.queryCommandValue(@"FontName"); }
        }

        public bool IsJustifyLeft
        {
            get { return (bool)DomDocument.queryCommandValue(@"JustifyLeft"); }
        }

        public bool IsJustifyCenter
        {
            get { return (bool)DomDocument.queryCommandValue(@"JustifyCenter"); }
        }

        public bool IsJustifyRight
        {
            get { return (bool)DomDocument.queryCommandValue(@"JustifyRight"); }
        }

        internal bool CanOutdent
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Outdent"));
            }
        }

        internal bool CanOrderedList
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"InsertOrderedList"));
            }
        }

        public bool CanUndo
        {
            get { return DomDocument.queryCommandEnabled(@"Undo"); }
        }

        internal bool CanJustifyRight
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"JustifyRight"));
            }
        }

        internal bool CanRemoveFormatting
        {
            get
            {
                return Document != null && Enabled && (IsTextSelection || IsNoneSelection);
            }
        }

        internal bool CanJustifyLeft
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"JustifyLeft"));
            }
        }

        internal bool CanJustifyCenter
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"JustifyCenter"));
            }
        }

        internal bool CanIndent
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Indent"));
            }
        }

        internal bool CanDelete
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Delete"));
            }
        }

        internal bool CanPaste
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Paste"));
            }
        }

        internal bool CanCopy
        {
            get { return Document != null && ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Copy"); }
        }

        private bool CanCut
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Cut"));
            }
        }

        internal bool CanItalic
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Italic"));
            }
        }

        internal bool CanUnderline
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Underline"));
            }
        }

        internal bool CanBold
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Bold"));
            }
        }

        internal bool CanChangeFont
        {
            get
            {
                return Document != null && Enabled;
            }
        }

        internal bool CanBullettedList
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"InsertUnorderedList"));
            }
        }

        internal bool CanForeColor
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"ForeColor"));
            }
        }

        internal bool CanBackColor
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"BackColor"));
            }
        }

        internal bool CanInsertHyperlink
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"CreateLink"));
            }
        }

        internal bool CanShowSource
        {
            get { return true; }
        }

        public bool CanTableProperties
        {
            get
            {
                return IsTableCurrentSelectionInsideTable;
            }
        }

        public bool CanAddTableRow
        {
            get { return IsTableCurrentSelectionInsideTable; }
        }

        public bool CanAddTableColumn
        {
            get { return IsTableCurrentSelectionInsideTable; }
        }

        public bool CanInsertTable
        {
            get { return true; }
        }

        public bool CanInsertTableRow
        {
            get { return IsTableCurrentSelectionInsideTable; }
        }

        public bool CanInsertTableColumn
        {
            get { return IsTableCurrentSelectionInsideTable; }
        }

        public bool CanTableDeleteRow
        {
            get { return CurrentSelectionTableCell != null; }
        }

        public bool CanTableDeleteColumn
        {
            get { return CurrentSelectionTableCell != null; }
        }

        public bool CanTableDeleteTable
        {
            get { return CurrentSelectionTable != null; }
        }

        public bool CanTableRowProperties
        {
            get { return CurrentSelectionTableRow != null; }
        }

        public bool CanTableColumnProperties
        {
            get { return CurrentSelectionTableCell != null; }
        }

        public bool CanTableCellProperties
        {
            get
            {
                var cells = CurrentSelectionTableCells;
                return cells != null && cells.Length > 0;
            }
        }
    }
}