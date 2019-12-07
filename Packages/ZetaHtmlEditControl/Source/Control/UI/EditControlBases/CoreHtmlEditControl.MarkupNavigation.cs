namespace ZetaHtmlEditControl.UI.EditControlBases
{
    using Code.PInvoke;
    using mshtml;

    public partial class CoreHtmlEditControl
    {
        public static bool compareLt(
            IMarkupPointer p1,
            IMarkupPointer p2)
        {
            int flag;
            p1.IsLeftOf(p2, out flag);
            return flag == NativeMethods.BOOL_TRUE;
        }

        public static bool compareLte(
            IMarkupPointer p1,
            IMarkupPointer p2)
        {
            int flag;
            p1.IsLeftOfOrEqualTo(p2, out flag);
            return flag == NativeMethods.BOOL_TRUE;
        }

        public static bool CompareE(
            IMarkupPointer p1,
            IMarkupPointer p2)
        {
            int flag;
            p1.IsEqualTo(p2, out flag);
            return flag == NativeMethods.BOOL_TRUE;
        }

        public static bool compareGte(
            IMarkupPointer p1,
            IMarkupPointer p2)
        {
            int flag;
            p1.IsRightOfOrEqualTo(p2, out flag);
            return flag == NativeMethods.BOOL_TRUE;
        }

        public static bool compareGt(
            IMarkupPointer p1,
            IMarkupPointer p2)
        {
            int flag;
            p1.IsRightOf(p2, out flag);
            return flag == NativeMethods.BOOL_TRUE;
        }
    }
}