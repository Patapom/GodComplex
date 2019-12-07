namespace ZetaHtmlEditControl.Code.MsHtml
{
    using System.Globalization;
    using Html;
    using HtmlAgilityPack;

    /// <summary>
    /// Übersetzen von gutem HTML-Code für valides CSS in "schlechten" HTML-Code
    /// wie er im MS-HTML-Editor generiert und erkannt wird.
    /// </summary>
    /// <remarks>
    /// Siehe auch http://connect.microsoft.com/IE/feedback/details/789619/mshtml-dll-of-ie10-still-generates-deprecated-element-tags-attributes
    /// </remarks>
    internal static class MsHtmlLegacyFromGoodToBadTranslator
    {
        /*
        IDM_BACKCOLOR : uses FONT element
        IDM_FONTNAME : uses FONT element    
        IDM_FONTSIZE : uses FONT element
        IDM_FORECOLOR : uses FONT element
        IDM_JUSTIFYCENTER : uses "align" element attribute
        IDM_JUSTIFYFULL : uses "align" element attribute
        IDM_JUSTIFYLEFT : uses "align" element attribute
        IDM_JUSTIFYNONE : uses "align" element attribute
        IDM_JUSTIFYRIGHT: uses "align" element attribute
        IDM_STRIKETHROUGH : uses STRIKE element
        IDM_UNDERLINE : uses U element
         */

        public static string Translate(string html)
        {
            return string.IsNullOrWhiteSpace(html) ? html : doTranslate(html);
        }

        private static string doTranslate(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // --

            var nodes = doc.DocumentNode.SelectNodes(@"//span");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node.HasInlineCssWithName(@"background-color"))
                    {
                        node.Name = @"font";
                    }

                    if (node.HasInlineCssWithName(@"color"))
                    {
                        node.Name = @"font";
                        node.SetAttributeValue(@"color", node.ReadInlineCssValue(@"color"));
                        node.RemoveInlineCssItem(@"color");
                    }

                    if (node.HasInlineCssWithName(@"font-family"))
                    {
                        node.Name = @"font";
                        node.SetAttributeValue(@"face", node.ReadInlineCssValue(@"font-family"));
                        node.RemoveInlineCssItem(@"font-family");
                    }

                    if (node.HasInlineCssWithName(@"font-size"))
                    {
                        node.Name = @"font";
                        node.SetAttributeValue(@"size", translateFontSizeFromCss(node.ReadInlineCssValue(@"font-size")));
                        node.RemoveInlineCssItem(@"font-size");
                    }

                    node.RemoveAttributeWithNameIfEmpty(@"style");
                }
            }

            // --

            nodes = doc.DocumentNode.SelectNodes(@"//*");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node.HasInlineCssWithName(@"text-align"))
                    {
                        node.SetAttributeValue(@"align", node.ReadInlineCssValue(@"text-align"));
                        node.RemoveInlineCssItem(@"text-align");
                    }

                    node.RemoveAttributeWithNameIfEmpty(@"style");
                }
            }

            // --

            return doc.DocumentNode.OuterHtml;
        }

        private static string translateFontSizeFromCss(string cssSize)
        {
            if (string.IsNullOrWhiteSpace(cssSize))
            {
                return 3.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                switch (cssSize)
                {
                    case @"xx-small":
                        return 1.ToString(CultureInfo.InvariantCulture);
                    case @"x-small":
                        return 2.ToString(CultureInfo.InvariantCulture);
                    case @"small":
                        return 3.ToString(CultureInfo.InvariantCulture);
                    case @"medium":
                        return 4.ToString(CultureInfo.InvariantCulture);
                    case @"large":
                        return 5.ToString(CultureInfo.InvariantCulture);
                    case @"x-large":
                        return 6.ToString(CultureInfo.InvariantCulture);
                    case @"xx-large":
                        return 7.ToString(CultureInfo.InvariantCulture);

                    default:
                        return 3.ToString(CultureInfo.InvariantCulture);

                }
            }
        }
    }
}