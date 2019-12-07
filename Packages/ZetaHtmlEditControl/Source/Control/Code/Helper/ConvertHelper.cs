namespace ZetaHtmlEditControl.Code.Helper
{
    using System;
    using System.Globalization;

    internal static class ConvertHelper
	{
        private const NumberStyles FloatNumberStyle =
            NumberStyles.Float |
            NumberStyles.Number |
            NumberStyles.AllowThousands |
            NumberStyles.AllowDecimalPoint |
            NumberStyles.AllowLeadingSign |
            NumberStyles.AllowLeadingWhite |
            NumberStyles.AllowTrailingWhite;

        public static string ToString(
			object o,
			string fallBackTo)
		{
			return ToString(o, fallBackTo, CultureInfo.CurrentCulture);
		}

        private static string ToString(
			object o,
			string fallBackTo,
			IFormatProvider provider)
		{
			if (o == null)
			{
				return fallBackTo;
			}
			// This is the fastest way, see
			// http://www.google.de/url?sa=t&ct=res&cd=4&url=http%3A%2F%2Fblogs.msdn.com%2Fvancem%2Farchive%2F2006%2F10%2F01%2F779503.aspx&ei=nOuTRY7TAoXe2QLi7qX3Dg&usg=__GUu0brYrkgjJl63ZZ3JBOzJCVH8=&sig2=1wvt78Kof6Bw7Drs3LL_ng
			else
			{
			    var s = o as string;
			    return s ?? Convert.ToString(o, provider);
			}
		}

		public static int ToInt32(
			object o)
		{
			return ToInt32(o, CultureInfo.CurrentCulture);
		}

        private static int ToInt32(
			object o,
			IFormatProvider provider)
		{
			return ToInt32(o, 0, provider);
		}

		public static int ToInt32(
			object o,
			int fallBackTo)
		{
			return ToInt32(o, fallBackTo, CultureInfo.CurrentCulture);
		}

        private static int ToInt32(
			object o,
			int fallBackTo,
			IFormatProvider provider)
		{
			if (o == null)
			{
				return fallBackTo;
			}
			// This is the fastest way, see
			// http://www.google.de/url?sa=t&ct=res&cd=4&url=http%3A%2F%2Fblogs.msdn.com%2Fvancem%2Farchive%2F2006%2F10%2F01%2F779503.aspx&ei=nOuTRY7TAoXe2QLi7qX3Dg&usg=__GUu0brYrkgjJl63ZZ3JBOzJCVH8=&sig2=1wvt78Kof6Bw7Drs3LL_ng
			else if (o is int)
			{
				return (int)o;
			}
			else if (IsInteger(o, provider))
			{
				return Convert.ToInt32(o, provider);
			}
			else if (IsDouble(o, provider))
			{
				return (int)Convert.ToDouble(o, provider);
			}
			else if (o is Enum)
			{
				return (int)o;
			}
			else
			{
				return fallBackTo;
			}
		}

        private static bool IsDouble(
			object o,
			IFormatProvider provider)
		{
			if (o == null)
			{
				return false;
			}
			// This is the fastest way, see
			// http://www.google.de/url?sa=t&ct=res&cd=4&url=http%3A%2F%2Fblogs.msdn.com%2Fvancem%2Farchive%2F2006%2F10%2F01%2F779503.aspx&ei=nOuTRY7TAoXe2QLi7qX3Dg&usg=__GUu0brYrkgjJl63ZZ3JBOzJCVH8=&sig2=1wvt78Kof6Bw7Drs3LL_ng
			else if (o is double)
			{
				return true;
			}
			// This is the fastest way, see
			// http://www.google.de/url?sa=t&ct=res&cd=4&url=http%3A%2F%2Fblogs.msdn.com%2Fvancem%2Farchive%2F2006%2F10%2F01%2F779503.aspx&ei=nOuTRY7TAoXe2QLi7qX3Dg&usg=__GUu0brYrkgjJl63ZZ3JBOzJCVH8=&sig2=1wvt78Kof6Bw7Drs3LL_ng
			else if (o is float)
			{
				return true;
			}
			else
			{
				return doIsNumeric(o,
					FloatNumberStyle,
					provider);
			}
		}

        private static bool IsInteger(
			object o,
			IFormatProvider provider)
		{
			if (o == null)
			{
				return false;
			}
			// This is the fastest way, see
			// http://www.google.de/url?sa=t&ct=res&cd=4&url=http%3A%2F%2Fblogs.msdn.com%2Fvancem%2Farchive%2F2006%2F10%2F01%2F779503.aspx&ei=nOuTRY7TAoXe2QLi7qX3Dg&usg=__GUu0brYrkgjJl63ZZ3JBOzJCVH8=&sig2=1wvt78Kof6Bw7Drs3LL_ng
			else if (o is int)
			{
				return true;
			}
			// This is the fastest way, see
			// http://www.google.de/url?sa=t&ct=res&cd=4&url=http%3A%2F%2Fblogs.msdn.com%2Fvancem%2Farchive%2F2006%2F10%2F01%2F779503.aspx&ei=nOuTRY7TAoXe2QLi7qX3Dg&usg=__GUu0brYrkgjJl63ZZ3JBOzJCVH8=&sig2=1wvt78Kof6Bw7Drs3LL_ng
			else if (o is long)
			{
				return true;
			}
			else if (o is Enum)
			{
				return true;
			}
			else
			{
				return doIsNumeric(o, NumberStyles.Integer, provider);
			}
		}

        public static decimal ToDecimal(
			object o)
		{
			return ToDecimal(o, CultureInfo.CurrentCulture);
		}

        private static decimal ToDecimal(
			object o,
			IFormatProvider provider)
		{
			return ToDecimal(o, decimal.Zero, provider);
		}

        private static decimal ToDecimal(
			object o,
			decimal fallBackTo,
			IFormatProvider provider)
		{
			if (o == null)
			{
				return fallBackTo;
			}
			// This is the fastest way, see
			// http://www.google.de/url?sa=t&ct=res&cd=4&url=http%3A%2F%2Fblogs.msdn.com%2Fvancem%2Farchive%2F2006%2F10%2F01%2F779503.aspx&ei=nOuTRY7TAoXe2QLi7qX3Dg&usg=__GUu0brYrkgjJl63ZZ3JBOzJCVH8=&sig2=1wvt78Kof6Bw7Drs3LL_ng
			else if (o is decimal)
			{
				return (decimal)o;
			}
			else if (IsDecimal(o, provider))
			{
				return Convert.ToDecimal(o, provider);
			}
			else
			{
				return fallBackTo;
			}
		}

        private static bool IsDecimal(
			object o,
			IFormatProvider provider)
		{
			if (o == null)
			{
				return false;
			}
			// This is the fastest way, see
			// http://www.google.de/url?sa=t&ct=res&cd=4&url=http%3A%2F%2Fblogs.msdn.com%2Fvancem%2Farchive%2F2006%2F10%2F01%2F779503.aspx&ei=nOuTRY7TAoXe2QLi7qX3Dg&usg=__GUu0brYrkgjJl63ZZ3JBOzJCVH8=&sig2=1wvt78Kof6Bw7Drs3LL_ng
			else if (o is decimal)
			{
				return true;
			}
			else
			{
				return doIsNumeric(
					o,
					FloatNumberStyle,
					provider);
			}
		}

		private static bool doIsNumeric(
			object o,
			NumberStyles styles,
			IFormatProvider provider)
		{
			if (o == null)
			{
				return false;
			}
			else if (Convert.ToString(o, provider).Length <= 0)
			{
				return false;
			}
			else
			{
				double result;
				return double.TryParse(
					o.ToString(),
					styles,
					provider,
					out result);
			}
		}
	}
}