namespace ZetaHtmlEditControl.Code.Html
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using HtmlAgilityPack;

    internal static class HtmlStringHelper
    {
        /// <summary>
        /// Wenn aus Excel kopiert wird, sind das TABLE-Beginn- und Ende-Tag
        /// jeweils _außerhalb_ vom kopierten Text. Das hier jetzt ergänzen.
        /// </summary>
        internal static string CheckCompleteHtmlTable(
            this string htmlCode)
        {
            if (string.IsNullOrEmpty(htmlCode))
            {
                return htmlCode;
            }
            else
            {
                htmlCode = htmlCode.Trim();

                if ((htmlCode.StartsWith(@"<col", StringComparison.InvariantCultureIgnoreCase) ||
                     htmlCode.StartsWith(@"<tr")) &&
                    htmlCode.EndsWith(@"</tr>", StringComparison.InvariantCultureIgnoreCase))
                {
                    htmlCode = string.Format(@"<table>{0}</table>", htmlCode);
                    htmlCode = htmlCode.CleanMsExcelHtml();
                    return htmlCode;
                }
                else
                {
                    return htmlCode;
                }
            }
        }

        internal static string GetBodyFromHtmlCode(
            this string htmlCode)
        {
            if (string.IsNullOrEmpty(htmlCode))
            {
                return htmlCode;
            }
            else if (htmlCode.IndexOf(@"<body", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                var regex = new Regex(
                    @".*?<body[^>]*>(.*?)</body>",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);

                var m = regex.Match(htmlCode);

                return m.Success ? m.Groups[1].Value : htmlCode;
            }
            else
            {
                return htmlCode;
            }
        }

        private static string unescapeHtmlEntities(
            string htmlCode)
        {
            htmlCode = htmlCode.Replace(@"&nbsp;", @" ");

            htmlCode = htmlCode.Replace(@"&Auml;", @"Ä");
            htmlCode = htmlCode.Replace(@"&absp;", @"ä");
            htmlCode = htmlCode.Replace(@"&obsp;", @"ö");
            htmlCode = htmlCode.Replace(@"&Obsp;", @"Ö");
            htmlCode = htmlCode.Replace(@"&ubsp;", @"ü");
            htmlCode = htmlCode.Replace(@"&Ubsp;", @"Ü");
            htmlCode = htmlCode.Replace(@"&szlig;", @"ß");

            htmlCode = htmlCode.Replace(@"&pound;", @"£");
            htmlCode = htmlCode.Replace(@"&sect;", @"§");
            htmlCode = htmlCode.Replace(@"&copy;", @"©");
            htmlCode = htmlCode.Replace(@"&reg;", @"®");
            htmlCode = htmlCode.Replace(@"&micro;", @"µ");
            htmlCode = htmlCode.Replace(@"&para;", @"¶");
            htmlCode = htmlCode.Replace(@"&Oslash;", @"Ø");
            htmlCode = htmlCode.Replace(@"&oslash;", @"ø");
            htmlCode = htmlCode.Replace(@"&divide;", @"÷");
            htmlCode = htmlCode.Replace(@"&times;", @"×");
            return htmlCode;
        }

        private static string removeTagFromHtmlCode(
            string tag,
            string htmlCode)
        {
            return Regex.Replace(
                htmlCode,
                String.Format(@"<{0}.*?</{1}>", tag, tag),
                String.Empty,
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }

        internal static string GetOnlyTextFromHtmlCode(this string htmlCode)
        {
            //<br>
            htmlCode = htmlCode.Replace("\r\n", @" ");
            htmlCode = htmlCode.Replace("\r", @" ");
            htmlCode = htmlCode.Replace("\n", @" ");

            htmlCode = htmlCode.Replace(@"</p>", Environment.NewLine + Environment.NewLine);
            htmlCode = htmlCode.Replace(@"</P>", Environment.NewLine + Environment.NewLine);

            //html comment
            htmlCode = Regex.Replace(
                htmlCode,
                @"<!--.*?-->",
                String.Empty,
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            //<p>
            htmlCode = Regex.Replace(htmlCode,
                @"<br[^>]*>",
                Environment.NewLine,
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            //tags
            htmlCode = removeTagFromHtmlCode(@"style", htmlCode);
            htmlCode = removeTagFromHtmlCode(@"script", htmlCode);

            //html
            htmlCode = Regex.Replace(
                htmlCode,
                "<(.|\n)+?>",
                String.Empty,
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            //umlaute
            htmlCode = unescapeHtmlEntities(htmlCode);

            //whitespaces
            htmlCode = Regex.Replace(
                htmlCode,
                @" +",
                @" ",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            return htmlCode;
        }

        internal static string AddNewLineToText(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }
            else
            {
                text = text.Replace("\r\n", "\n");
                text = text.Replace("\r", "\n");
                text = text.Replace("\n", @"<br />");

                return text;
            }
        }

        public static string MakeLinkTargets(this string html, string target)
        {
            if (string.IsNullOrWhiteSpace(html) || string.IsNullOrEmpty(target))
            {
                return html;
            }
            else
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // --

                var anchorTagNodes = doc.DocumentNode.SelectNodes(@"//a");
                if (anchorTagNodes != null)
                {
                    foreach (var anchorTagNode in anchorTagNodes)
                    {
                        var currentTarget = anchorTagNode.ReadAttributeValue(@"target");
                        if (string.IsNullOrEmpty(currentTarget))
                        {
                            if (anchorTagNode.Attributes.Contains(@"target"))
                            {
                                anchorTagNode.Attributes[@"target"].Value = target;
                            }
                            else
                            {
                                anchorTagNode.Attributes.Add(@"target", target);
                            }
                        }
                    }
                }

                // --

                return doc.DocumentNode.OuterHtml;
            }
        }

        public static string CleanMsExcelHtml(this string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return html;
            }
            else
            {
                var sc =
                    new[]
                {
                    @"<col\b[^>]*>",
                    @"\s?height=\w+",
                    @"\s?height='[^']+'",
                    @"\s?height=""[^""]+""",
                    @"\s?width=\w+",
                    @"\s?width='[^']+'",
                    @"\s?width=""[^""]+""",
                    @"\s?class=\w+",
                    @"\s?class='[^']+'",
                    @"\s?class=""[^""]+""",
                    @"\s+style='[^']+'",
                    @"\s+style=""[^""]+""",
                    @"\s+v:\w+=""[^""]+""",
                    @"(\n\r){2,}"
                };

                return sc.Aggregate(html, (current, s) => Regex.Replace(current, s, String.Empty, RegexOptions.IgnoreCase));
            }
        }

        public static
            string CleanMsWordHtml(this string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return html;
            }
            else
            {
                // http://www.codinghorror.com/blog/2006/01/cleaning-words-nasty-html.html
                var sc =
                    new[]
                {
                    @"<!--(\w|\W)+?-->",
                    @"<title>(\w|\W)+?</title>",
                    @"\s?class=\w+",
                    @"\s?class='[^']+'",
                    @"\s?class=""[^""]+""",
                    @"\s+style='[^']+'",
                    @"\s+style=""[^""]+""",
                    @"<(meta|link|/?o:|/?style|/?div|/?st\d|/?head|/?html|body|/?body|/?span|!\[)[^>]*?>",
                    @"(<[^>]+>)+&nbsp;(</\w+>)+",
                    @"\s+v:\w+=""[^""]+""",
                    @"(\n\r){2,}"
                };
                // get rid of unnecessary tag spans (comments and title)
                // Get rid of classes and styles
                // Get rid of unnecessary tags
                // Get rid of empty paragraph tags
                // remove bizarre v: element attached to <img> tag
                // remove extra lines

                return sc.Aggregate(html, (current, s) => Regex.Replace(current, s, String.Empty, RegexOptions.IgnoreCase));
            }
        }
    }
}