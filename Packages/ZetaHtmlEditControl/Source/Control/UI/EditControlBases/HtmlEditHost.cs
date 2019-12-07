namespace ZetaHtmlEditControl.UI.EditControlBases
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using mshtml;

    internal class HtmlEditHost :
        IHTMLEditHost
    {
        // http://www.codeproject.com/Articles/6845/Implementing-snap-to-grid-in-an-MSHTML-based-appli
        // http://computer-programming-forum.com/4-csharp/42ab66e9d5d57b60.htm
        // https://code.google.com/p/slowandsteadyparser/source/browse/trunk/SlowAndSteadyParser/csExWB/General_Interfaces/IServiceProvider.cs

        private readonly Dictionary<string, Size> _initialImgSizes = new Dictionary<string, Size>();

        /// <summary>
        /// Diese Funktion dient dazu, die Grid-Handles proportional zu skalieren, so dass
        /// Benutzer das Bild zwar in der Größe ändern kann, jedoch das Seitenverhältnis immer
        /// beibehalten bleibt.
        /// </summary>
        /// <remarks>
        /// Stichwörter: bild, image, resize, size, größe, groesse, scale, "width", "height".
        /// </remarks>
        void IHTMLEditHost.SnapRect(IHTMLElement pIElement, ref tagRECT prcNew, _ELEMENT_CORNER eHandle)
        {
            var img = pIElement as IHTMLImgElement;
            if (img != null)
            {
                var key = String.Format(@"{0}-{1}", pIElement.id, img.src);

                if (!_initialImgSizes.ContainsKey(key))
                {
                    _initialImgSizes.Add(key, new Size(img.width, img.height));
                }

                var initialSize = _initialImgSizes[key];

                switch (eHandle)
                {
                    case _ELEMENT_CORNER.ELEMENT_CORNER_RIGHT:
                    case _ELEMENT_CORNER.ELEMENT_CORNER_LEFT:
                    {
                        var fac = initialSize.Height / (float)initialSize.Width;

                        var newWidth = prcNew.right - prcNew.left;
                        var newHeight = fac * newWidth;

                        // Niemals > 100%.
                        newWidth = Math.Min(newWidth, initialSize.Width);
                        newHeight = Math.Min(newHeight, initialSize.Height);

                        prcNew.right = prcNew.left + newWidth;
                        prcNew.bottom = (int)(prcNew.top + newHeight);

                        img.width = newWidth;
                        img.height = (int)newHeight;
                    }
                        break;

                    case _ELEMENT_CORNER.ELEMENT_CORNER_TOP:
                    case _ELEMENT_CORNER.ELEMENT_CORNER_BOTTOM:
                    case _ELEMENT_CORNER.ELEMENT_CORNER_BOTTOMLEFT:
                    case _ELEMENT_CORNER.ELEMENT_CORNER_BOTTOMRIGHT:
                    case _ELEMENT_CORNER.ELEMENT_CORNER_TOPLEFT:
                    case _ELEMENT_CORNER.ELEMENT_CORNER_TOPRIGHT:
                    {
                        var fac = initialSize.Width / (float)initialSize.Height;

                        var newHeight = prcNew.bottom - prcNew.top;
                        var newWidth = fac * newHeight;

                        // Niemals > 100%.
                        newWidth = Math.Min(newWidth, initialSize.Width);
                        newHeight = Math.Min(newHeight, initialSize.Height);

                        prcNew.right = (int)(prcNew.left + newWidth);
                        prcNew.bottom = prcNew.top + newHeight;

                        img.width = (int)newWidth;
                        img.height = newHeight;
                    }
                        break;

                        // TODO
                }
            }
        }
    }
}