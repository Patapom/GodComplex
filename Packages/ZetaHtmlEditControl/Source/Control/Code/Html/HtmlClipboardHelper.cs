namespace ZetaHtmlEditControl.Code.Html
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;
    using System.Windows.Forms;
    using IDataObject = System.Windows.Forms.IDataObject;

    internal static class HtmlClipboardHelper
    {
        [DllImport(@"kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GlobalLock(HandleRef handle);

        [DllImport(@"kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        private static extern bool GlobalUnlock(HandleRef handle);

        [DllImport(@"kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        private static extern int GlobalSize(HandleRef handle);

        /// <summary>
        /// Extracts data of type <c>Dataformat.Html</c> from an <c>IDataObject</c> data container
        /// This method shouldn't throw any exception but writes relevant exception informations in the debug window
        /// </summary>
        /// <param name="data">data container</param>
        /// <returns>A byte[] array with the decoded string or null if the method fails</returns>
        /// <remarks>Added 2006-06-12, <c>Uwe Keim</c>.</remarks>
        private static byte[] getHtml(
            IDataObject data)
        {
            var interopData = (System.Runtime.InteropServices.ComTypes.IDataObject)data;

            var format =
                new FORMATETC
                {
                    cfFormat = ((short)DataFormats.GetFormat(DataFormats.Html).Id),
                    dwAspect = DVASPECT.DVASPECT_CONTENT,
                    lindex = (-1),
                    tymed = TYMED.TYMED_HGLOBAL
                };

            STGMEDIUM stgmedium;
            stgmedium.tymed = TYMED.TYMED_HGLOBAL;
            stgmedium.pUnkForRelease = null;

            var queryResult = interopData.QueryGetData(ref format);

            if (queryResult != 0)
            {
                return null;
            }

            interopData.GetData(ref format, out stgmedium);

            if (stgmedium.unionmember == IntPtr.Zero)
            {
                return null;
            }

            var pointer = stgmedium.unionmember;

            var handleRef = new HandleRef(null, pointer);

            byte[] rawArray;

            try
            {
                var ptr1 = GlobalLock(handleRef);

                var length = GlobalSize(handleRef);

                rawArray = new byte[length];

                Marshal.Copy(ptr1, rawArray, 0, length);
            }
            finally
            {
                GlobalUnlock(handleRef);
            }

            return rawArray;
        }

        /// <summary>
        /// See http://66.249.93.104/search?q=cache:yfQWT9XlYogJ:www.eggheadcafe.com/aspnet_answers/NETFrameworkNETWindowsForms/Apr2006/post26606306.asp+IDataObject+html+utf-8&amp;hl=de&amp;gl=de&amp;ct=clnk&amp;cd=1&amp;client=firefox-a
        /// See http://bakamachine.blogspot.com/2006/05/workarond-for-dataobject-html.html
        /// </summary>
        /// <remarks>Added 2006-06-12, <c>Uwe Keim</c>.</remarks>
        /// <returns></returns>
        internal static void GetHtmlFromClipboard(
            out string clipText,
            out byte[] originalBuffer)
        {
            originalBuffer = getHtml(Clipboard.GetDataObject());
            clipText = Encoding.UTF8.GetString(originalBuffer);
        }

        public static string GetSourceUrlFromClipboard()
        {
            // Get HTML from Clipboard.
            // Modified 2006-06-12, Uwe Keim.
            string clipText;
            byte[] originalBuffer;
            GetHtmlFromClipboard(out clipText, out originalBuffer);

            return getSourceUrlFromClipboardHtmlCode(clipText);
        }

        private static string getSourceUrlFromClipboardHtmlCode(
            string htmlCode)
        {
            var htmlInfo = htmlCode.Substring(0, htmlCode.IndexOf('<') - 1);

            var i = htmlInfo.IndexOf(@"SourceURL:", StringComparison.Ordinal);
            var url = htmlInfo.Substring(i + 10);
            url = url.Substring(0, url.IndexOf('\r'));
            return url;
        }

        public static string GetHtmlFromClipboard()
        {
            // Get HTML from Clipboard.
            // Modified 2006-06-12, Uwe Keim.
            string clipText;
            byte[] originalBuffer;
            GetHtmlFromClipboard(out clipText, out originalBuffer);

            //selected fragment
            return getHtmlFragmentFromClipboardCode(clipText, originalBuffer);
        }

        private static string getHtmlFragmentFromClipboardCode(
            string htmlCode,
            byte[] originalBuffer)
        {
            //split Html to htmlInfo (and htmlSource)
            var htmlInfo = htmlCode.Substring(0, htmlCode.IndexOf('<') - 1);

            //get Fragment positions
            var tmp = htmlInfo.Substring(htmlInfo.IndexOf(@"StartFragment:", StringComparison.Ordinal) + 14);
            tmp = tmp.Substring(0, tmp.IndexOf('\r'));
            var posStartSelection = Convert.ToInt32(tmp);

            tmp = htmlInfo.Substring(htmlInfo.IndexOf(@"EndFragment:", StringComparison.Ordinal) + 12);
            tmp = tmp.Substring(0, tmp.IndexOf('\r'));
            var posEndSelection = Convert.ToInt32(tmp);

            // get Fragment. Always UTF-8 as of spec.
            var s = Encoding.UTF8.GetString(
                originalBuffer,
                posStartSelection,
                posEndSelection - posStartSelection);

            return s;
        }
    }
}