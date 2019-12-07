namespace ZetaHtmlEditControl.Code.Html
{
    public static class HtmlImageHelper
    {
        public static string ImagesFolderPathPlaceHolder
        {
            get { return @"http://pseudo-image-folder-path"; }
        }

        /// <summary>
        /// Stand-alone function to expand any placeholders inside
        /// a given HTML fragment.
        /// </summary>
        public static string ExpandImageFolderPathPlaceHolder(
            string html,
            string externalImagesFolderPath)
        {
            if (string.IsNullOrEmpty(html) || !html.Contains(ImagesFolderPathPlaceHolder))
            {
                return html;
            }
            else
            {
                using (var ch = new HtmlConversionHelper())
                {
                    return ch.ConvertSetHtml(
                        html,
                        externalImagesFolderPath,
                        ImagesFolderPathPlaceHolder);
                }
            }
        }
    }
}