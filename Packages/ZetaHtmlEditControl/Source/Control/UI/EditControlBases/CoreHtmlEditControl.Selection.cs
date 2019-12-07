namespace ZetaHtmlEditControl.UI.EditControlBases
{
    using System.Collections.Generic;
    using Code.PInvoke;
    using mshtml;

    public partial class CoreHtmlEditControl
    {
        public IHTMLElement CurrentSelectedElement
        {
            get
            {
                if (DomDocument.all.length == 0)
                {
                    return null;
                }
                else
                {
                    var txt = CurrentSelectionText;

                    if (txt == null)
                    {
                        var ctrl = CurrentSelectionControl;
                        return ctrl.commonParentElement();
                    }
                    else
                    {
                        return txt.parentElement();
                    }
                }
            }
        }

        protected bool IsTableCurrentSelectionInsideTable
        {
            get { return CurrentSelectionTable != null; }
        }

        protected IHTMLTableRow CurrentSelectionTableRow
        {
            get
            {
                if (CurrentSelectionTable == null)
                {
                    return null;
                }
                    // A complete table.
                else if (IsTableSelection || IsControlSelection)
                {
                    return null;
                }
                else
                {
                    // --
                    // Go up until the TR is found.

                    IHTMLElement element;

                    if (IsControlSelection)
                    {
                        var rng = CurrentSelectionControl;
                        element = rng.item(0);
                    }
                    else
                    {
                        var rng = CurrentSelectionText;
                        element = rng == null ? null : rng.parentElement();
                    }

                    while (element != null)
                    {
                        var tagName = element.tagName.ToLowerInvariant();
                        if (tagName == @"tr")
                        {
                            return element as IHTMLTableRow;
                        }
                        else
                        {
                            // Go up.
                            element = element.parentElement;
                        }
                    }

                    // --

                    // Not found.
                    return null;
                }
            }
        }

        protected IHTMLTableCell CurrentSelectionTableCell
        {
            get
            {
                if (CurrentSelectionTable == null)
                {
                    return null;
                }
                    // A complete table.
                else if (IsTableSelection || IsControlSelection)
                {
                    return null;
                }
                else
                {
                    // --
                    // Go up until the TH or TD is found.

                    IHTMLElement element;

                    if (IsControlSelection)
                    {
                        var rng = CurrentSelectionControl;
                        element = rng.item(0);
                    }
                    else
                    {
                        var rng = CurrentSelectionText;
                        element = rng == null ? null : rng.parentElement();
                    }

                    while (element != null)
                    {
                        var tagName = element.tagName.ToLowerInvariant();
                        if (tagName == @"th" || tagName == @"td")
                        {
                            return element as IHTMLTableCell;
                        }
                        else
                        {
                            // Go up.
                            element = element.parentElement;
                        }
                    }

                    // --

                    // Not found.
                    return null;
                }
            }
        }

        protected IHTMLTableCell[] CurrentSelectionTableCells
        {
            get
            {
                var result = new List<IHTMLTableCell>();

                if (CurrentSelectionTable != null)
                {
                    IMarkupPointer mp1;
                    IMarkupPointer mp2;
                    GetCurrentSelection(out mp1, out mp2);

                    // --

                    // Walk from left to right of the current selection,
                    // storing all TH and TD tags.

                    var walk = mp1;

                    while (compareLte(walk, mp2))
                    {
                        // walk right.
                        _MARKUP_CONTEXT_TYPE context;
                        IHTMLElement element;
                        var minus1 = -1;
                        ushort unused;
                        walk.right(
                            NativeMethods.BOOL_TRUE,
                            out context,
                            out element,
                            ref minus1,
                            out unused);

                        if (element != null && context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope)
                        {
                            var tagName = element.tagName.ToLowerInvariant();
                            if (tagName == @"th" || tagName == @"td")
                            {
                                result.Add(element as IHTMLTableCell);
                            }
                        }
                    }

                    // Nothing selected, just the caret inside a table cell?
                    if (result.Count <= 0)
                    {
                        // --
                        // Go up until the TH or TD is found.

                        IHTMLElement element;

                        if (IsControlSelection)
                        {
                            var rng = CurrentSelectionControl;
                            element = rng.item(0);
                        }
                        else
                        {
                            var rng = CurrentSelectionText;
                            element = rng.parentElement();
                        }

                        while (element != null)
                        {
                            var tagName = element.tagName.ToLowerInvariant();
                            if (tagName == @"th" || tagName == @"td")
                            {
                                result.Add(element as IHTMLTableCell);
                                break;
                            }
                            else
                            {
                                // Go up.
                                element = element.parentElement;
                            }
                        }
                    }
                }

                return result.ToArray();
            }
        }

        /// <summary>
        /// Returns "-1" if none/not found.
        /// </summary>
        protected int CurrentSelectionTableRowIndex
        {
            get
            {
                var row = CurrentSelectionTableRow;
                return row == null ? -1 : row.rowIndex;
            }
        }

        /// <summary>
        /// Returns "-1" if none/not found.
        /// </summary>
        protected int CurrentSelectionTableColumnIndex
        {
            get
            {
                var cell = CurrentSelectionTableCell;
                return cell == null ? -1 : cell.cellIndex;
            }
        }

        /// <summary>
        /// Returns "0" if none/not found.
        /// </summary>
        protected int CurrentSelectionTableRowCount
        {
            get
            {
                var table = CurrentSelectionTable as IHTMLTable;
                if (table == null)
                {
                    return 0;
                }
                else
                {
                    var rows = table.rows;
                    return rows == null ? 0 : rows.length;
                }
            }
        }

        /// <summary>
        /// Returns "0" if none/not found.
        /// </summary>
        protected int CurrentSelectionTableColumnCount
        {
            get
            {
                var row = CurrentSelectionTableRow;

                if (row == null)
                {
                    return 0;
                }
                else
                {
                    var cells = row.cells;
                    return cells == null ? 0 : cells.length;
                }
            }
        }

        public IHTMLTxtRange CurrentSelectionText
        {
            get
            {
                if (DomDocument == null || DomDocument.all.length == 0)
                {
                    return null;
                }
                else
                {
                    var selection = DomDocument.selection;
                    var rangeDisp = selection.createRange();

                    var textRange = rangeDisp as IHTMLTxtRange;

                    return textRange;
                }
            }
        }

        public IHTMLTxtRange CreateRangeOfWholeBody()
        {
            var ms = (IMarkupServices)DomDocument;
            IMarkupPointer mpStart;
            ms.CreateMarkupPointer(out mpStart);

            IMarkupPointer mpEnd;
            ms.CreateMarkupPointer(out mpEnd);

            var mpStart2 = (IMarkupPointer2)mpStart;
            mpStart2.MoveToContent(DomDocument.body, NativeMethods.BOOL_TRUE);

            var mpEnd2 = (IMarkupPointer2)mpEnd;
            mpEnd2.MoveToContent(DomDocument.body, NativeMethods.BOOL_FALSE);

            var range = (IHTMLTxtRange)DomDocument.selection.createRange();

            ms.MoveRangeToPointers(mpStart2, mpEnd2, range);
            return range;
        }

        public IHTMLControlRange CurrentSelectionControl
        {
            get
            {
                if (DomDocument == null || DomDocument.all.length == 0)
                {
                    return null;
                }
                else
                {
                    var selection = DomDocument.selection;
                    var rangeDisp = selection.createRange();

                    var textRange = rangeDisp as IHTMLControlRange;

                    return textRange;
                }
            }
        }

        public bool IsControlSelection
        {
            get
            {
                var selection = DomDocument.selection;
                var st = selection.type.ToLowerInvariant();

                return st == @"control";
            }
        }

        public bool IsTextSelection
        {
            get
            {
                var selection = DomDocument.selection;
                var st = selection.type.ToLowerInvariant();

                return st == @"text";
            }
        }

        public bool IsNoneSelection
        {
            get
            {
                var selection = DomDocument.selection;
                var st = selection.type.ToLowerInvariant();

                return st == @"none";
            }
        }

        public bool IsTableSelection
        {
            get
            {
                if (!IsControlSelection)
                {
                    return false;
                }
                else
                {
                    var rng = CurrentSelectionControl;

                    if (rng.length <= 0)
                    {
                        return false;
                    }
                    else
                    {
                        var element = rng.item(0);

                        var tagName = element.tagName.ToLowerInvariant();
                        return tagName == @"table";
                    }
                }
            }
        }

        protected IHTMLTable2 CurrentSelectionTable
        {
            get
            {
                // A complete table.
                if (IsTableSelection)
                {
                    var rng = CurrentSelectionControl;
                    var element = rng.item(0);

                    return element as IHTMLTable2;
                }
                    // Inside a table (nested)?
                else
                {
                    IHTMLElement element;

                    if (IsControlSelection)
                    {
                        var rng = CurrentSelectionControl;
                        element = rng.item(0);
                    }
                    else
                    {
                        var rng = CurrentSelectionText;
                        element = rng == null ? null : rng.parentElement();
                    }

                    while (element != null)
                    {
                        var tagName = element.tagName.ToLowerInvariant();
                        if (tagName == @"table")
                        {
                            return element as IHTMLTable2;
                        }
                        else
                        {
                            // Go up.
                            element = element.parentElement;
                        }
                    }

                    // Not found.
                    return null;
                }
            }
        }

        public void InsertHtmlAtCurrentSelection(
            string html)
        {
            if (IsControlSelection)
            {
                // if its a control range, it must be deleted before.
                var sel = CurrentSelectionControl;
                sel.execCommand(@"Delete", false, null);
            }

            var sel2 = CurrentSelectionText;
            sel2.pasteHTML(html);
        }

        public string SelectWord()
        {
            var range = CurrentSelectionText;
            range.moveStart(@"word", -1);
            range.moveEnd(@"word");
            return string.Empty;
        }

        protected void MoveCaretToElement(
            IHTMLElement element)
        {
            if (element != null)
            {
                var ms = (IMarkupServices) DomDocument;
                IMarkupPointer mp;
                ms.CreateMarkupPointer(out mp);

                var mp2 = (IMarkupPointer2) mp;
                mp2.MoveToContent(element, NativeMethods.BOOL_TRUE);

                var ds = (IDisplayServices) DomDocument;
                IDisplayPointer dp;
                ds.CreateDisplayPointer(out dp);

                dp.MoveToMarkupPointer(mp, null);

                // --

                IHTMLCaret caret;
                ds.GetCaret(out caret);

                caret.MoveCaretToPointer(
                    dp,
                    NativeMethods.BOOL_TRUE,
                    _CARET_DIRECTION.CARET_DIRECTION_SAME);
                caret.Show(NativeMethods.BOOL_TRUE);
            }
        }

        public void GetCurrentSelection(
            out IMarkupPointer selectionMPStart,
            out IMarkupPointer selectionMPEnd)
        {
            // get markup container of the whole document.
            var mc = (IMarkupContainer) DomDocument;

            // get the markup services.
            var ms = (IMarkupServices) DomDocument;

            // create two markup pointers.
            ms.CreateMarkupPointer(out selectionMPStart);
            ms.CreateMarkupPointer(out selectionMPEnd);

            selectionMPStart.MoveToContainer(mc, NativeMethods.BOOL_TRUE);
            selectionMPEnd.MoveToContainer(mc, NativeMethods.BOOL_TRUE);

            // --
            // position start and end pointers around the current selection.

            var selection = DomDocument.selection;

            var st = selection.type.ToLowerInvariant();

            switch (st)
            {
                case @"none":
                {
                    var ds = (IDisplayServices) DomDocument;

                    IHTMLCaret caret;
                    ds.GetCaret(out caret);

                    caret.MoveMarkupPointerToCaret(selectionMPStart);
                    caret.MoveMarkupPointerToCaret(selectionMPEnd);

                    // Set gravity, as in "Introduction to Markup Services" in MSDN.
                    // http://msdn.microsoft.com/en-us/library/bb508514(v=vs.85).aspx
                    selectionMPStart.SetGravity(_POINTER_GRAVITY.POINTER_GRAVITY_Right);
                }
                    break;
                case @"text":
                {
                    // MoveToSelectionAnchor does only work with "text" selections.
                    var selectionText = DomDocument.selection;

                    var range = (IHTMLTxtRange) selectionText.createRange();

                    ms.MovePointersToRange(range, selectionMPStart, selectionMPEnd);

                    // swap if wrong direction.
                    if (compareGt(selectionMPStart, selectionMPEnd))
                    {
                        var tmp = selectionMPStart;
                        selectionMPStart = selectionMPEnd;
                        selectionMPEnd = tmp;
                    }

                    // Set gravity, as in "Introduction to Markup Services" in MSDN.
                    // http://msdn.microsoft.com/en-us/library/bb508514(v=vs.85).aspx
                    selectionMPStart.SetGravity(_POINTER_GRAVITY.POINTER_GRAVITY_Right);
                }
                    break;
                case @"control":
                {
                    // MoveToSelectionAnchor does only work with "text" selections.
                    var selectionControl = DomDocument.selection;

                    var range = selectionControl.createRange() as IHTMLControlRange;

                    // Strangly, range was null sometimes.
                    // E.g. when I resized a table (=control selection)
                    // and then did an undo.
                    if (range != null)
                    {
                        if (range.length > 0)
                        {
                            var start = range.item(0);
                            var end = range.item(range.length - 1);

                            selectionMPStart.MoveAdjacentToElement(
                                start,
                                _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                            selectionMPEnd.MoveAdjacentToElement(
                                end,
                                _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                        }
                    }
                }
                    break;
            }
        }

        /// <summary>
        /// Liefert das nächstliegendste Element unter dem Caret.
        /// Nur sinnvoll, IMHO, wenn zurzeit keine Auswahl ist.
        /// </summary>
        /// <remarks>
        /// 2013-10-26, Uwe Keim: Ergänzt.
        /// </remarks>
        public IHTMLElement GetElementAtCaret()
        {
            // Idee von http://stackoverflow.com/a/1911389/107625.

            var dom = DomDocument;
            if (dom == null)
            {
                return null;
            }
            else
            {
                var ds = (IDisplayServices) dom;

                IHTMLCaret c;
                ds.GetCaret(out c);

                if (c == null)
                {
                    return null;
                }
                else
                {
                    var ms = (IMarkupServices) dom;
                    IMarkupPointer mp;
                    ms.CreateMarkupPointer(out mp);

                    c.MoveMarkupPointerToCaret(mp);

                    IHTMLElement el;
                    mp.CurrentScope(out el);

                    return el;
                }
            }
        }
    }
}