namespace ZetaHtmlEditControl.UI.EditControlDerives
{
    using System;
    using System.Windows.Forms;
    using Code.PInvoke;
    using mshtml;

    public partial class HtmlEditControl
    {
        public override bool PreProcessMessage(
            ref Message msg)
        {
            if (!DesignMode && !HtmlEditorDesignModeManager.IsDesignMode)
            {
                if (msg.Msg == NativeMethods.WmKeydown || msg.Msg == NativeMethods.WmSyskeydown)
                {
                    var isShift = (ModifierKeys & Keys.Shift) != 0;

                    var key = ((Keys)((int)msg.WParam));

                    var e = new PreviewKeyDownEventArgs(key | ModifierKeys);

                    // Check all shortcuts that I handle by myself.
                    if (doHandleShortcutKey(e, false))
                    {
                        return true;
                    }
                    else
                    {
                        if (key == Keys.Enter)
                        {
                            // 2010-11-02, Uwe Keim:
                            // Just as in TortoiseSVN dialogs, use Ctrl+Enter as default button.
                            if (e.Control && !e.Alt && !e.Shift)
                            {
                                closeDialogWithOK();
                                return true;
                            }

                            if (!e.Alt && !e.Shift && !e.Control)
                            {
                                return handleEnterKey();
                            }
                            else
                            {
                                return false;
                            }
                        }
                        if (key == Keys.Tab)
                        {
                            // TAB key.
                            if (!e.Control && !e.Alt)
                            {
                                if (handleTabKeyInsideTable(isShift))
                                {
                                    return true;
                                }
                                else
                                {
                                    // Forward or backward?
                                    var forward = !isShift;

                                    var form = FindForm();
                                    if (form != null)
                                    {
                                        var c = form.GetNextControl(this, forward);

                                        while (c != null &&
                                               c != this &&
                                               !c.TabStop)
                                        {
                                            c = form.GetNextControl(c, forward);
                                        }

                                        if (c != null)
                                        {
                                            c.Focus();
                                        }
                                    }
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            return base.PreProcessMessage(ref msg);
        }

        public event EventHandler WantCloseDialogWithOK;

        private void closeDialogWithOK()
        {
            var h = WantCloseDialogWithOK;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        private static bool handleEnterKey()
        {
            return false;
        }

        /// <summary>
        /// Give derived classes the chance to handle the TAB key
        /// when inside a table.
        /// Return TRUE if handled, FALSE if not handled.
        /// </summary>
        private bool handleTabKeyInsideTable(
            bool isShift)
        {
            if (CanTableCellProperties || CanAddTableRow)
            {
                if (IsControlSelection)
                {
                    // The whole table is selected, add row at the end.

                    if (!isShift && CanAddTableRow)
                    {
                        ExecuteTableAddTableRow();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    // A cell is selected. Move to next/previous cell
                    // or add row if on last.

                    var rowIndex = CurrentSelectionTableRowIndex;
                    var columnIndex = CurrentSelectionTableColumnIndex;

                    var rowCount = CurrentSelectionTableRowCount;
                    var columnCount = CurrentSelectionTableColumnCount;

                    if (isShift)
                    {
                        // Previous cell.

                        if (columnIndex == 0 && rowIndex == 0)
                        {
                            return false;
                        }
                        else if (columnIndex > 0)
                        {
                            // Previous cell.
                            var row = CurrentSelectionTableRow;

                            var element = (IHTMLElement)
                                row.cells.item(
                                    columnIndex - 1,
                                    columnIndex - 1);
                            MoveCaretToElement(element);
                            return true;
                        }
                        else
                        {
                            // Previous line, last cell.
                            var table = (IHTMLTable)CurrentSelectionTable;
                            var previousRow =
                                (IHTMLTableRow)
                                    table.rows.item(
                                        rowIndex - 1,
                                        rowIndex - 1);

                            var element =
                                (IHTMLElement)
                                    previousRow.cells.item(
                                        previousRow.cells.length - 1,
                                        previousRow.cells.length - 1);
                            MoveCaretToElement(element);
                            return true;
                        }
                    }
                    else
                    {
                        // Next cell.

                        if (columnIndex < columnCount - 1)
                        {
                            // Next cell.
                            var row =
                                CurrentSelectionTableRow;

                            var element =
                                row.cells.item(
                                    columnIndex + 1,
                                    columnIndex + 1) as IHTMLElement;
                            MoveCaretToElement(element);
                            return true;
                        }
                        else if (columnIndex == columnCount - 1 &&
                                 rowIndex < rowCount - 1)
                        {
                            // Next row, first cell.
                            var table = (IHTMLTable)CurrentSelectionTable;
                            var nextRow = (IHTMLTableRow)
                                table.rows.item(
                                    rowIndex + 1,
                                    rowIndex + 1);

                            var element =
                                nextRow.cells.item(0, 0) as IHTMLElement;
                            MoveCaretToElement(element);
                            return true;
                        }
                        else
                        {
                            // Add new row.
                            ExecuteTableAddTableRow();
                            return true;
                        }
                    }
                }
            }
            else
            {
                return false;
            }
        }

        private bool doHandleShortcutKey(
            PreviewKeyDownEventArgs e,
            bool onlyCheck)
        {
            if (e.KeyCode == Keys.V && e.Control && e.Shift) //v + ctrl + shift
            {
                if (!onlyCheck)
                {
                    handlePaste(PasteMode.Text);
                }
                return true;
            }
            else if (e.KeyCode == Keys.V && e.Control) //v + ctrl
            {
                if (!onlyCheck)
                {
                    handlePaste(PasteMode.Normal);
                }
                return true;
            }
            else if (e.KeyCode == Keys.W && e.Control && e.Shift) //v + ctrl
            {
                if (!onlyCheck)
                {
                    handlePaste(PasteMode.MsWord);
                }
                return true;
            }
            else if (e.KeyCode == Keys.I && e.Control && e.Shift && e.Alt) //ctrl + alt + shift + i.
            {
                if (!onlyCheck)
                {
                    ExecuteSystemInfo();
                }
                return true;
            }
            else if (e.KeyCode == Keys.Delete) //del
            {
                if (!onlyCheck)
                {
                    ExecuteDelete();
                }
                return true;
            }
            else if (e.KeyCode == Keys.X && e.Control) //x + ctrl
            {
                if (!onlyCheck)
                {
                    ExecuteCut();
                }
                return true;
            }
            else if (e.KeyCode == Keys.C && e.Control) //c + ctrl
            {
                if (!onlyCheck)
                {
                    ExecuteCopy();
                }
                return true;
            }
            else if (e.KeyCode == Keys.Z && e.Control) //z + ctrl
            {
                if (!onlyCheck)
                {
                    ExecuteUndo();
                }
                return true;
            }
            else if (e.KeyCode == Keys.Y && e.Control) //y + ctrl
            {
                if (!onlyCheck)
                {
                    ExecuteRedo();
                }
                return true;
            }
            else if (
                e.KeyCode == Keys.U && e.Control ||
                e.KeyCode == Keys.U && e.Control && e.Shift) //u+ ctrl+ shift
            {
                if (!onlyCheck)
                {
                    ExecuteUnderline();
                }
                return true;
            }
            else if (
                e.KeyCode == Keys.I && e.Control ||
                e.KeyCode == Keys.K && e.Control && e.Shift) //k+ ctrl+ shift
            {
                if (!onlyCheck)
                {
                    ExecuteItalic();
                }
                return true;
            }
            else if (
                e.KeyCode == Keys.B && e.Control ||
                e.KeyCode == Keys.F && e.Control && e.Shift) //f+ ctrl+ shift
            {
                if (!onlyCheck)
                {
                    ExecuteBold();
                }
                return true;
            }
            else if (e.KeyCode == Keys.K && e.Control) //k+ ctrl
            {
                if (!onlyCheck)
                {
                    ExecuteInsertHyperlink();
                }
                return true;
            }
            else if (e.KeyCode == Keys.A && e.Control) //a+ ctrl
            {
                if (!onlyCheck)
                {
                    ExecuteSelectAll();
                }
                return true;
            }
            else if (e.KeyCode == Keys.E && e.Control) //E+ ctrl
            {
                if (!onlyCheck)
                {
                    ExecuteJustifyCenter();
                }
                return true;
            }
            else if (e.KeyCode == Keys.L && e.Control) //l+ ctrl
            {
                if (!onlyCheck)
                {
                    ExecuteJustifyLeft();
                }
                return true;
            }
            else if (e.KeyCode == Keys.R && e.Control) //r+ ctrl
            {
                if (!onlyCheck)
                {
                    ExecuteJustifyRight();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void constructHtmlEditControlKeyboard()
        {
            setMenuShortcutKeys();
        }

        private void setMenuShortcutKeys()
        {
            deleteToolStripMenuItem.ShortcutKeys = Keys.Delete;
            copyToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            pasteToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.V;
            pasteAsTextToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.V;
            pasteFromMsWordToolStripItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.W;
            justifyLeftToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.L;
            justifyCenterToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.E;
            justifyRightToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.R;
            hyperLinkToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.K;
            boldToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.F;
            italicToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.I;
            cutToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.X;
        }
    }
}