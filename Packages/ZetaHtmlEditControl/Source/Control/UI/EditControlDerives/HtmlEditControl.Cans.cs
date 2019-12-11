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

        public bool CanOutdent
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Outdent"));
            }
        }

        public bool CanOrderedList
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

        public bool CanJustifyRight
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"JustifyRight"));
            }
        }

        public bool CanRemoveFormatting
        {
            get
            {
                return Document != null && Enabled && (IsTextSelection || IsNoneSelection);
            }
        }

        public bool CanJustifyLeft
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"JustifyLeft"));
            }
        }

        public bool CanJustifyCenter
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"JustifyCenter"));
            }
        }

        public bool CanIndent
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Indent"));
            }
        }

        public bool CanDelete
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Delete"));
            }
        }

        public bool CanPaste
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Paste"));
            }
        }

        public bool CanCopy
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

        public bool CanItalic
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Italic"));
            }
        }

        public bool CanUnderline
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Underline"));
            }
        }

        public bool CanBold
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"Bold"));
            }
        }

        public bool CanChangeFont
        {
            get
            {
                return Document != null && Enabled;
            }
        }

        public bool CanBullettedList
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"InsertUnorderedList"));
            }
        }

        public bool CanForeColor
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"ForeColor"));
            }
        }

        public bool CanBackColor
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"BackColor"));
            }
        }

        public bool CanInsertHyperlink
        {
            get
            {
                return Document != null && (Enabled &&
                                            ((HTMLDocument)Document.DomDocument).queryCommandEnabled(@"CreateLink"));
            }
        }

        public bool CanShowSource
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