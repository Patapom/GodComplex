namespace ZetaHtmlEditControl.UI
{
    /// <summary>
    /// 2014-03-31, Uwe Keim:
    /// Zurück gebrochen auf das eigentlich benötigte, nämlich das "IsDesignMode".
    /// Alles andere gelöscht.
    /// 
    /// 2014-03-24, Uwe Keim:
    /// Damit im Windows Forms Designer keine Fehler und Abstürze kommen, erst zur
    /// Laufzeit hier das Control tatsächlich erstellen lassen. Im Editor also nur
    /// einen Platzhalter.
    /// </summary>
    public static class HtmlEditorDesignModeManager 
    {
        private static bool _isDesignMode = true;

        public static bool IsDesignMode
        {
            get { return _isDesignMode; }
            set { _isDesignMode = value; }
        }

        //public HtmlEditorDesignModeManager()
        //{
        //    if (!DesignMode)
        //    {
        //        CreateRealHtmlEditor();
        //    }
        //}

        //private void CreateRealHtmlEditor()
        //{
        //    if (RealHtmlEditor == null)
        //    {
        //        var editor = new HtmlEditUserControl {Dock = DockStyle.Fill, TabStop = true, TabIndex = 0};
        //        Controls.Add(editor);
        //    }
        //}

        //public HtmlEditUserControl RealHtmlEditor
        //{
        //    get
        //    {
        //        return Controls.Count <= 0 || !(Controls[0] is HtmlEditUserControl)
        //            ? null
        //            : (HtmlEditUserControl) Controls[0];
        //    }
        //}

        //protected override void OnEnter(EventArgs e)
        //{
        //    base.OnEnter(e);

        //    var editor = RealHtmlEditor;
        //    if (editor != null)
        //    {
        //        editor.Focus();
        //        editor.Select();
        //    }
        //}
    }
}