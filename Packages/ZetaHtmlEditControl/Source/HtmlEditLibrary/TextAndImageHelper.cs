namespace ZetaHtmlEditLibrary
{
    using ZetaHtmlEditControl.Code.Html;

    public static class TextAndImageHelper
    {
        public static string GetDocumentText(
            string html,
            string externalImagesFolderPath,
            bool useImagesFolderPathPlaceHolder)
        {
            return
                new HtmlConversionHelper().ConvertGetHtml(
                    html,
                    null,
                    externalImagesFolderPath,
                    useImagesFolderPathPlaceHolder ? HtmlImageHelper.ImagesFolderPathPlaceHolder : null);
        }
    }
}