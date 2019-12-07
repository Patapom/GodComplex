namespace ZetaHtmlEditControl.Code.Html
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Klasse um auf Inline-CSS-Elemente in einem STYLE-Attribut eines HTML-Tags
    /// sauber zugreifen zu können.
    /// </summary>
    internal sealed class InlineCssParser
    {
        public InlineCssParser(string rawInlineCss)
        {
            InlineCss = rawInlineCss ?? string.Empty;
        }

        /// <summary>
        /// Immer aktuell.
        /// </summary>
        public string InlineCss { get; private set; }

        /// <summary>
        /// Z.B. "color" auf "#000000" setzen.
        /// </summary>
        public void SetValue(string name, string value)
        {
            var dic = parse(InlineCss);
            setDictionary(dic, name, value);
            InlineCss = combine(dic);
        }

        /// <summary>
        /// Z.B. "color" lesen.
        /// </summary>
        public string GetValue(string name)
        {
            return getDictionary(parse(InlineCss), name);
        }

        public void RemoveItem(string name)
        {
            var dic = parse(InlineCss);
            removeFromDictionary(dic, name);
            InlineCss = combine(dic);
        }

        public bool HasName(string name)
        {
            return hasDictionary(parse(InlineCss), name);
        }

        private static string combine(ICollection<KeyValuePair<string, string>> dic)
        {
            if (dic == null || dic.Count <= 0)
            {
                return string.Empty;
            }
            else
            {
                var result = new StringBuilder();

                foreach (var pair in dic)
                {
                    if (result.Length > 0) result.Append(@"; ");
                    result.AppendFormat(@"{0}: {1}", pair.Key, pair.Value);
                }

                return result.ToString();
            }
        }

        private static Dictionary<string, string> parse(string raw)
        {
            var result = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(raw))
            {
                var firsts = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var first in firsts)
                {
                    var f = first.Trim();
                    if (!string.IsNullOrEmpty(f))
                    {
                        var seconds = f.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                        if (seconds.Length == 2)
                        {
                            var a = seconds[0].Trim().ToLowerInvariant();
                            var b = seconds[1].Trim();

                            if (!string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b))
                            {
                                setDictionary(result, a, b);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static string getDictionary(IDictionary<string, string> dic, string a)
        {
            return dic.ContainsKey(a.ToLowerInvariant()) ? dic[a.ToLowerInvariant()] : null;
        }

        private static bool hasDictionary(IDictionary<string, string> dic, string a)
        {
            return dic.ContainsKey(a.ToLowerInvariant());
        }

        private static void removeFromDictionary(IDictionary<string, string> dic, string a)
        {
            if (dic.ContainsKey(a.ToLowerInvariant()))
            {
                dic.Remove(a.ToLowerInvariant());
            }
        }

        private static void setDictionary(IDictionary<string, string> dic, string a, string b)
        {
            if (dic.ContainsKey(a.ToLowerInvariant()))
            {
                dic[a.ToLowerInvariant()] = b;
            }
            else
            {
                dic.Add(a.ToLowerInvariant(), b);
            }
        }
    }
}