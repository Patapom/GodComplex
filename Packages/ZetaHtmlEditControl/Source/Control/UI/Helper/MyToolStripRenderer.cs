namespace ZetaHtmlEditControl.UI.Helper
{
    using System;
    using System.Drawing;
    using System.Reflection;
    using System.Windows.Forms;

    public class MyToolStripRender :
        ToolStripProfessionalRenderer
    {
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            var mi = e.Item as ToolStripMenuItem;
            if (mi != null)
            {
                var isShortcut = e.Text == getShortcutText(mi);

                // Tastenkürzel in grau zeichnen, damit weniger ablenkt.
                e.TextColor = isShortcut
                    ? Color.Gainsboro /*SystemColors.GrayText*/
                    : SystemColors.MenuText;

                doDraw(e, isShortcut);
            }
            else
            {
                base.OnRenderItemText(e);
            }
        }

        private static void doDraw(ToolStripItemTextRenderEventArgs e, bool isShortcut)
        {
            // 2013-10-31, Uwe Keim: Hier selbst zeichnen (via ILSpy kopiert und vereinfacht),
            //                       weil sonst ein disabled shortcut in einer anderen Farbe
            //                       gezeichnet werden würde.

            var item = e.Item;
            var graphics = e.Graphics;
            var color = e.TextColor;
            var textFont = e.TextFont;
            var text = e.Text;
            var textRectangle = e.TextRectangle;
            var textFormat = e.TextFormat;
            color = (item.Enabled ? color : (isShortcut ? color : SystemColors.GrayText));

            TextRenderer.DrawText(graphics, text, textFont, textRectangle, color, textFormat);
        }

        private static string getShortcutText(IDisposable mi)
        {
            var t = (string)mi.GetType().InvokeMember(
                @"GetShortcutText",
                BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                mi,
                new object[] { });

            return t;
        }
    }
}