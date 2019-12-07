namespace ZetaHtmlEditControl.Code.MsHtml
{
    using mshtml;

    internal static class MsHtmlMarkupServicesExtensions
    {
        public static IMarkupPointer CreateMarkupPointer(this IMarkupServices ms)
        {
            IMarkupPointer mp;
            ms.CreateMarkupPointer(out mp);
            return mp;
        }
    }
}