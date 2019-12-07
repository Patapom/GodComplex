namespace ZetaHtmlEditControl.Code.MsHtml
{
    using mshtml;
    using PInvoke;

    /// <summary>
    /// Um Auswahlen usw. handeln zu können.
    /// </summary>
    public static class MsHtmlMarkupPointerExtensions
    {
        private static bool IsRightOf(this IMarkupPointer p1, IMarkupPointer p2)
        {
            int flag;
            p1.IsRightOf(p2, out flag);
            return flag == NativeMethods.BOOL_TRUE;
        }

        public static void CheckSwap(ref IMarkupPointer mpStart, ref IMarkupPointer mpEnd)
        {
            if (mpStart.IsRightOf(mpEnd))
            {
                var tmp = mpStart;
                mpStart = mpEnd;
                mpEnd = tmp;
            }
        }
    }
}