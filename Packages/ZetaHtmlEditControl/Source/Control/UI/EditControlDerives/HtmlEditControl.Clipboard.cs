namespace ZetaHtmlEditControl.UI.EditControlDerives
{
    using System;
    using System.Collections.Generic;
    using Code.Html;

    public partial class HtmlEditControl
    {
        private static string checkImages(
            ICollection<HtmlConversionHelper.ImageInfo> originalNames,
            string html,
            string url)
        {
            if (originalNames != null && originalNames.Count > 0)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var s in originalNames)
                {
                    if (!s.Source.StartsWith(@"http") && !s.Source.StartsWith(@"https"))
                    {
                        html = html.Replace(
                            s.Source,
                            HtmlConversionHelper.GetPathFromFile(s.Source, new Uri(url)));
                    }
                }
            }
            return html;
        }
    }
}