namespace ZetaHtmlEditControl.Code.Helper
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    internal static class StringHelper
	{
        private static readonly Dictionary<Enum, string> RecentEnumDescriptions =
            new Dictionary<Enum, string>();

        public static string GetEnumDescription(
			Enum value,
			bool allowCaching = true)
		{
			string result;
			if (allowCaching && RecentEnumDescriptions.TryGetValue(value, out result))
			{
				return result;
			}
			else
			{
				var fi = value.GetType().GetField(value.ToString());

				var attributes =
					(DescriptionAttribute[])fi.GetCustomAttributes(
						typeof(DescriptionAttribute),
						false);

				result = attributes.Length > 0 ? attributes[0].Description : value.ToString();

				RecentEnumDescriptions[value] = result;
				return result;
			}
		}
	}
}