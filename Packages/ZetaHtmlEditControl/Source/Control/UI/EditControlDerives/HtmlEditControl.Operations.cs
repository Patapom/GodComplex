namespace ZetaHtmlEditControl.UI.EditControlDerives
{
    using System.Windows.Forms;
    using Code.MsHtml;
    using Code.PInvoke;
    using mshtml;
    using Properties;
    using Tools;

    public partial class HtmlEditControl
    {
        private void ExecuteSystemInfo()
        {
            var msg =
                string.Format(
                    @"URL: {0}.",
                    Url);

            MessageBox.Show(
                FindForm(),
                msg,
                Resources.HtmlEditControl_ExecuteSystemInfo_Information,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ExecuteSelectAll()
        {
            if (Document != null)
            {
                var doc = (HTMLDocument) Document.DomDocument;
                doc.execCommand(@"SelectAll", false, null);
            }
        }

        internal void ExecuteUnderline()
        {
            if (Document != null)
            {
                var doc = (HTMLDocument) Document.DomDocument;
                doc.execCommand(@"Underline", false, null);
            }
        }

        private void ExecuteRedo()
        {
            if (Document != null) Document.ExecCommand(@"Redo", false, null);
        }

        internal void ExecuteUndo()
        {
            if (Document != null) Document.ExecCommand(@"Undo", false, null);
        }

        public void ExecutePrintPreview()
        {
            if (Document != null && Configuration.AllowPrint)
            {
                OleCommandTargetExecute(NativeMethods.IdmPrintpreview, null);
            }
        }

        public void ExecutePrint()
        {
            if (Document != null && Configuration.AllowPrint)
            {
                OleCommandTargetExecute(NativeMethods.IdmPrint, null);
            }
        }

        internal void ExecuteBold()
        {
            if (Document != null)
            {
                var doc = (HTMLDocument) Document.DomDocument;
                //var rr = FontName;
                //var rr1 = FontSize;

                doc.execCommand(@"Bold", false, null);
            }
        }

        //commands list
        //https://developer.mozilla.org/en/docs/Rich-Text_Editing_in_Mozilla
        internal void ExecuteFontSize(string newSize)
        {
            if (Document != null)
            {
                var doc = (HTMLDocument) Document.DomDocument;
                doc.execCommand(@"FontSize", false, newSize);
            }
        }

        internal void ExecuteFontName(string name)
        {
            if (Document != null)
            {
                var doc = (HTMLDocument) Document.DomDocument;
                doc.execCommand(@"FontName", false, name);
            }
        }

        internal void ExecuteItalic()
        {
            if (Document != null)
            {
                var doc =
                    (HTMLDocument) Document.DomDocument;

                doc.execCommand(@"Italic", false, null);
            }
        }

        internal void ExecutePaste()
        {
            handlePaste(PasteMode.Normal);
        }

        internal void ExecutePasteAsText()
        {
            handlePaste(PasteMode.Text);
        }

        internal void ExecutePasteFromWord()
        {
            handlePaste(PasteMode.MsWord);
        }

        internal void ExecuteShowSource()
        {
            using (var form = new HtmlSourceTextEditForm(DocumentText))
            {
                form.ExternalInformationProvider = Configuration == null
                    ? null
                    : Configuration.ExternalInformationProvider;

                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    DocumentText = form.HtmlText;
                    updateUI();
                }
            }
        }

        internal void ExecuteInsertHyperlink()
        {
            if (Document != null)
            {
                var doc = (HTMLDocument) Document.DomDocument;

                doc.execCommand(@"CreateLink", true, null);
            }
        }

        internal void ExecuteIndent()
        {
            if (Document != null)
            {
                var doc =
                    (HTMLDocument) Document.DomDocument;

                doc.execCommand(@"Indent", false, null);
            }
        }

        internal void ExecuteJustifyCenter()
        {
            if (Document != null)
            {
                var doc =
                    (HTMLDocument) Document.DomDocument;

                doc.execCommand(@"JustifyCenter", false, null);
            }
        }

        internal void ExecuteJustifyLeft()
        {
            if (Document != null)
            {
                var doc =
                    (HTMLDocument) Document.DomDocument;

                doc.execCommand(@"JustifyLeft", false, null);
            }
        }

        internal void ExecuteJustifyRight()
        {
            if (Document != null)
            {
                var doc =
                    (HTMLDocument) Document.DomDocument;

                doc.execCommand(@"JustifyRight", false, null);
            }
        }

        internal void ExecuteNumberedList()
        {
            if (Document != null)
            {
                var doc =
                    (HTMLDocument) Document.DomDocument;

                doc.execCommand(@"InsertOrderedList", false, null);
            }
        }

        internal void ExecuteOutdent()
        {
            if (Document != null)
            {
                var doc =
                    (HTMLDocument) Document.DomDocument;

                doc.execCommand(@"Outdent", false, null);
            }
        }

        internal void ExecuteBullettedList()
        {
            if (Document != null)
            {
                var doc = (HTMLDocument) Document.DomDocument;

                doc.execCommand(@"InsertUnorderedList", false, null);
            }
        }

        internal void ExecuteCopy()
        {
            if (Document != null)
            {
                var doc = (HTMLDocument) Document.DomDocument;

                doc.execCommand(@"Copy", false, null);
            }
        }

        internal void ExecuteCut()
        {
            if (Document != null)
            {
                var doc = (HTMLDocument) Document.DomDocument;

                doc.execCommand(@"Cut", false, null);
                //if (IsTextSelection)
                //{
                //    var range = (IHTMLTxtRange) doc.selection.createRange();

                //    Clipboard.SetText(range.htmlText);

                //    // 2011-10-20, added.
                //    ExecuteDelete();
                //}
            }
        }

        internal void ExecuteDelete()
        {
            if (Document != null)
            {
                var doc = (HTMLDocument) Document.DomDocument;

                doc.execCommand(@"Delete", false, null);
            }
        }

        public void ExecuteInsertTable()
        {
            using (var form = new HtmlEditorTableNewForm())
            {
                form.ExternalInformationProvider = Configuration == null
                    ? null
                    : Configuration.ExternalInformationProvider;

                if (form.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    InsertHtmlAtCurrentSelection(form.Html);
                }
            }
        }

        public void ExecuteInsertTableRow()
        {
            var table = CurrentSelectionTable as IHTMLTable;

            if (table != null)
            {
                int rowIndex = CurrentSelectionTableRowIndex;

                var row =
                    HtmlEditorTableNewForm.AddTableRowsAfterRow(
                        table,
                        rowIndex,
                        1);

                // Set focus to first cell in the new line.
                if (row != null)
                {
                    var cell = row.cells.item(0, 0) as IHTMLTableCell;
                    MoveCaretToElement(cell as IHTMLElement);
                }
            }
        }

        internal void ExecuteSetBackColor05()
        {
            setBackColor(@"ff00ff");
        }

        public void ExecuteInsertTableColumn()
        {
            var table = CurrentSelectionTable as IHTMLTable;

            if (table != null)
            {
                var columnIndex = CurrentSelectionTableColumnIndex;

                HtmlEditorTableNewForm.AddTableColumnsAfterColumn(
                    table,
                    columnIndex,
                    1);
            }
        }

        public void ExecuteTableAddTableRow()
        {
            var table = CurrentSelectionTable as IHTMLTable;

            if (table != null)
            {
                var row = HtmlEditorTableNewForm.AddTableRowsAtBottom(table, 1);

                MoveCaretToElement(row.cells.item(0, 0) as IHTMLElement);
            }
        }

        public void ExecuteTableAddTableColumn()
        {
            var table = CurrentSelectionTable as IHTMLTable;

            if (table != null)
            {
                HtmlEditorTableNewForm.AddTableColumnsAtRight(
                    table,
                    1);
            }
        }

        public void ExecuteTableProperties()
        {
            var table = CurrentSelectionTable as IHTMLTable;

            if (table != null)
            {
                using (var form = new HtmlEditorTableNewForm())
                {
                    form.ExternalInformationProvider = Configuration == null
                        ? null
                        : Configuration.ExternalInformationProvider;

                    form.Table = table;
                    form.ShowDialog(FindForm());
                }
            }
        }

        public void ExecuteTableDeleteRow()
        {
            var table = CurrentSelectionTable as IHTMLTable;
            var rowIndex = CurrentSelectionTableRowIndex;

            if (table != null && rowIndex != -1)
            {
                table.deleteRow(rowIndex);
            }
        }

        public void ExecuteTableDeleteColumn()
        {
            var table = CurrentSelectionTable as IHTMLTable;
            var columnIndex = CurrentSelectionTableColumnIndex;

            if (table != null && columnIndex != -1)
            {
                var rows = table.rows;

                if (rows != null)
                {
                    for (var i = 0; i < rows.length; ++i)
                    {
                        var row = rows.item(i, i) as IHTMLTableRow;
                        if (row != null)
                        {
                            row.deleteCell(columnIndex);
                        }
                    }
                }
            }
        }

        public void ExecuteTableDeleteTable()
        {
            var table = CurrentSelectionTable as IHTMLTable;

            if (table != null)
            {
                var tableNode = table as IHTMLDOMNode;

                if (tableNode != null)
                {
                    tableNode.removeNode(true);
                }
            }
        }

        public void ExecuteTableRowProperties()
        {
            var row = CurrentSelectionTableRow;

            if (row != null)
            {
                using (var form = new HtmlEditorCellPropertiesForm())
                {
                    form.ExternalInformationProvider = Configuration == null
                        ? null
                        : Configuration.ExternalInformationProvider;

                    form.Initialize(row);
                    form.ShowDialog(FindForm());
                }
            }
        }

        public void ExecuteTableColumnProperties()
        {
            var table = CurrentSelectionTable as IHTMLTable;
            var columnIndex = CurrentSelectionTableColumnIndex;

            if (table != null && columnIndex >= 0)
            {
                using (var form = new HtmlEditorCellPropertiesForm())
                {
                    form.ExternalInformationProvider = Configuration == null
                        ? null
                        : Configuration.ExternalInformationProvider;

                    form.Initialize(table, columnIndex);
                    form.ShowDialog(FindForm());
                }
            }
        }

        public void ExecuteTableCellProperties()
        {
            var cells = CurrentSelectionTableCells;

            if (cells != null && cells.Length > 0)
            {
                using (var form = new HtmlEditorCellPropertiesForm())
                {
                    form.ExternalInformationProvider = Configuration == null
                        ? null
                        : Configuration.ExternalInformationProvider;

                    form.Initialize(cells);
                    form.ShowDialog(FindForm());
                }
            }
        }

        internal void ExecuteSetForeColorNone()
        {
            setForeColor(MsHtmlLegacyFromBadToGoodTranslator.NoForegroundColor);
        }

        internal void ExecuteSetForeColor01()
        {
            setForeColor(@"c00000");
        }

        internal void ExecuteSetForeColor02()
        {
            setForeColor(@"ff0000");
        }

        internal void ExecuteSetForeColor03()
        {
            setForeColor(@"ffc000");
        }

        internal void ExecuteSetForeColor04()
        {
            setForeColor(@"ffff00");
        }

        internal void ExecuteSetBackColorNone()
        {
            setBackColor(MsHtmlLegacyFromBadToGoodTranslator.NoBackgroundColor);
        }

        internal void ExecuteSetBackColor02()
        {
            setBackColor(@"00ff00");
        }

        internal void ExecuteSetBackColor03()
        {
            setBackColor(@"00ffff");
        }

        internal void ExecuteSetBackColor04()
        {
            setBackColor(@"ff0000");
        }

        internal void ExecuteSetForeColor05()
        {
            setForeColor(@"92d050");
        }

        internal void ExecuteSetForeColor06()
        {
            setForeColor(@"00b050");
        }

        internal void ExecuteSetForeColor07()
        {
            setForeColor(@"00b0f0");
        }

        internal void ExecuteSetForeColor08()
        {
            setForeColor(@"0070c0");
        }

        internal void ExecuteSetForeColor09()
        {
            setForeColor(@"002060");
        }

        internal void ExecuteSetForeColor10()
        {
            setForeColor(@"7030a0");
        }

        internal void ExecuteSetBackColor01()
        {
            setBackColor(@"ffff00");
        }

        private void setForeColor(
            string color)
        {
            if (Document != null)
            {
                var doc = (HTMLDocument) Document.DomDocument;

                doc.execCommand(@"ForeColor", false, color);
            }
        }

        private void setBackColor(
            string color)
        {
            if (Document != null)
            {
                var doc = (HTMLDocument) Document.DomDocument;
                doc.execCommand(@"BackColor", false, color);
            }
        }

        /// <summary>
        /// Entweder von Auswahl oder von allem.
        /// </summary>
        internal void ExecuteRemoveFormatting()
        {
            if (IsTextSelection)
            {
                var sel = CurrentSelectionText;
                if (sel != null)
                {
                    sel.execCommand(@"removeFormat", false, null);
                }
            }
            else if (IsNoneSelection)
            {
                var range = CreateRangeOfWholeBody();
                range.execCommand(@"removeFormat", false, null);
            }
        }
    }
}