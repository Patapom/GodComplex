namespace ZetaHtmlEditControl.Code.MsHtml
{
    using System;
    using System.Globalization;
    using Html;
    using HtmlAgilityPack;

    /// <summary>
    /// Übersetzen von schlechtem HTML-Code, wie er im MS-HTML-Editor generiert wird
    /// in guten HTML-Code für valides CSS.
    /// </summary>
    /// <remarks>
    /// Siehe auch http://connect.microsoft.com/IE/feedback/details/789619/mshtml-dll-of-ie10-still-generates-deprecated-element-tags-attributes
    /// </remarks>
    public static class MsHtmlLegacyFromBadToGoodTranslator
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

        public const string NoBackgroundColor = @"window";
        public const string NoForegroundColor = @"windowtext";

        public static string Translate(string html)
        {
            return string.IsNullOrWhiteSpace(html) ? html : doTranslate(html);
        }

        private static string doTranslate(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // --

            var nodes = doc.DocumentNode.SelectNodes(@"//font");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    node.Name = @"span";

                    if (node.HasAttributeWithName(@"color"))
                    {
                        var color = node.ReadAttributeValue(@"color");
                        if (!string.Equals(color, NoForegroundColor, StringComparison.InvariantCultureIgnoreCase))
                        {
                            node.SetInlineCss(@"color", color);
                        }
                        node.RemoveAttributeWithName(@"color");
                    }

                    var bgColor = node.ReadInlineCssValue(@"background-color");
                    if (bgColor != null &&
                        string.Equals(bgColor, NoBackgroundColor, StringComparison.InvariantCultureIgnoreCase))
                    {
                        node.RemoveInlineCssItem(@"background-color");
                    }

                    if (node.HasAttributeWithName(@"face"))
                    {
                        var face = node.ReadAttributeValue(@"face");
                        if (!string.IsNullOrEmpty(face))
                        {
                            node.SetInlineCss(@"font-family", face);
                        }
                        node.RemoveAttributeWithName(@"face");
                    }

                    if (node.HasAttributeWithName(@"size"))
                    {
                        var size = translateFontSizeToCss(node.ReadAttributeValue(@"size"));
                        if (!string.IsNullOrEmpty(size) && size != 3.ToString(CultureInfo.InvariantCulture))
                        {
                            node.SetInlineCss(@"font-size", size);
                        }
                        node.RemoveAttributeWithName(@"size");
                    }

                    node.RemoveAttributeWithNameIfEmpty(@"style");
                }
            }

            // --

            nodes =
                doc.DocumentNode.SelectNodes(
                    @"//*[@align='left' or @align='center' or @align='right' or @align='justify']");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node.HasAttributeWithName(@"align"))
                    {
                        var align = node.ReadAttributeValue(@"align");
                        if (!string.IsNullOrEmpty(align))
                        {
                            node.SetInlineCss(@"text-align", align);
                        }
                        node.RemoveAttributeWithName(@"align");
                    }

                    node.RemoveAttributeWithNameIfEmpty(@"style");
                }
            }

            // --
            // "Müll" entfernen.

            nodes = doc.DocumentNode.SelectNodes(@"//*[self::blockquote or self::p]");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node.HasAttributeWithName(@"style"))
                    {
                        var align = node.ReadInlineCssValue(@"margin-right");
                        if (!string.IsNullOrEmpty(align) &&
                            string.Equals(align, @"0px", StringComparison.InvariantCultureIgnoreCase))
                        {
                            node.RemoveInlineCssItem(@"margin-right");
                        }
                    }

                    if (node.HasAttributeWithName(@"dir"))
                    {
                        var align = node.ReadAttributeValue(@"dir");
                        if (!string.IsNullOrEmpty(align) &&
                            string.Equals(align, @"ltr", StringComparison.InvariantCultureIgnoreCase))
                        {
                            node.RemoveAttributeWithName(@"dir");
                        }
                    }

                    node.RemoveAttributeWithNameIfEmpty(@"style");
                }
            }

            // --

            removeEmptySpanTags(doc);

            return doc.DocumentNode.OuterHtml;
        }

        public static string RemoveEmptySpanTags(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            removeEmptySpanTags(doc);

            return doc.DocumentNode.OuterHtml;
        }

        private static void removeEmptySpanTags(HtmlDocument doc)
        {
            // http://stackoverflow.com/a/12093377/107625

            // Zur Sicherheit immer wieder neu anfangen, sonst verhaspelt sich die Reihenfolge irgendwie.
            while (true)
            {
                var nodes = doc.DocumentNode.SelectNodes(@"//span");
                if (nodes == null)
                {
                    break;
                }
                else
                {
                    var any = false;

                    foreach (var node in nodes)
                    {
                        if (!node.HasAttributes)
                        {
                            node.RemoveNodeKeepChildren();
                            any = true;
                            break;
                        }
                    }

                    if (!any) break;
                }
            }
        }

        private static string translateFontSizeToCss(string size)
        {
            int sizeClassic;
            if (int.TryParse(size, out sizeClassic))
            {
                switch (sizeClassic)
                {
                    case 1:
                        return @"xx-small";
                    case 2:
                        return @"x-small";
                    case 3:
                        return @"small";
                    case 4:
                        return @"medium";
                    case 5:
                        return @"large";
                    case 6:
                        return @"x-large";
                    case 7:
                        return @"xx-large";

                    default:
                        return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }
    }
}