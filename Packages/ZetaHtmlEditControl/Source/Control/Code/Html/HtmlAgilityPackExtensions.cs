namespace ZetaHtmlEditControl.Code.Html
{
    using System;
    using System.Linq;
    using HtmlAgilityPack;

    internal static class HtmlAgilityPackExtensions
    {
        /// <summary>
        /// Bugfix-Version, siehe http://stackoverflow.com/questions/7911455/html-agility-pack-removechild-not-behaving-as-expected
        /// </summary>
        public static void RemoveNodeKeepChildren(this HtmlNode node)
        {
            foreach (var child in node.ChildNodes)
            {
                node.ParentNode.InsertBefore(child, node);
            }
            node.Remove();
        }

        public static bool HasAttributeWithName(this HtmlNode node, string name)
        {
            return node.HasAttributes &&
                   node.Attributes.Any(a => string.Equals(a.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }

        public static string ReadAttributeValue(this HtmlNode node, string name)
        {
            return HasAttributeWithName(node, name)
                ? node.Attributes.First(a => string.Equals(a.Name, name, StringComparison.InvariantCultureIgnoreCase))
                    .Value
                : null;
        }

        public static void RemoveAttributeWithName(this HtmlNode node, string name)
        {
            if (HasAttributeWithName(node, name))
            {
                node.Attributes.Remove(
                    node.Attributes.First(a => string.Equals(a.Name, name, StringComparison.InvariantCultureIgnoreCase)));
            }
        }

        public static void RemoveAttributeWithNameIfEmpty(this HtmlNode node, string name)
        {
            if (HasAttributeWithName(node, name) && string.IsNullOrWhiteSpace(ReadAttributeValue(node, name)))
            {
                node.Attributes.Remove(
                    node.Attributes.First(a => string.Equals(a.Name, name, StringComparison.InvariantCultureIgnoreCase)));
            }
        }

        public static void SetInlineCss(this HtmlNode node, string cssPropertyName, string cssPropertyValue)
        {
            var doc = node.OwnerDocument;

            var b =
                node.Attributes.FirstOrDefault(
                    a => string.Equals(a.Name, @"style", StringComparison.InvariantCultureIgnoreCase));
            if (b == null)
            {
                b = doc.CreateAttribute(@"style");
                node.Attributes.Add(b);
            }

            var p = new InlineCssParser(b.Value);
            p.SetValue(cssPropertyName, cssPropertyValue);

            b.Value = p.InlineCss;
        }

        public static bool HasInlineCssWithName(this HtmlNode node, string name)
        {
            var p = new InlineCssParser(ReadAttributeValue(node, @"style"));
            return p.HasName(name);
        }

        public static string ReadInlineCssValue(this HtmlNode node, string name)
        {
            var p = new InlineCssParser(ReadAttributeValue(node, @"style"));
            return p.GetValue(name);
        }

        public static void RemoveInlineCssItem(this HtmlNode node, string name)
        {
            if (HasInlineCssWithName(node, name))
            {
                var p = new InlineCssParser(ReadAttributeValue(node, @"style"));
                p.RemoveItem(name);

                var b = node.Attributes.First(
                    a => string.Equals(a.Name, @"style", StringComparison.InvariantCultureIgnoreCase));
                b.Value = p.InlineCss;
            }
        }
    }
}