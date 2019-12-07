namespace ZetaHtmlEditControl.UI.EditControlDerives
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;
    using Code.Helper;
    using Code.Html;
    using mshtml;

    public partial class HtmlEditControl
    {
        private void handlePaste(
            PasteMode pasteMode)
        {
            if (Document != null)
            {
                var doc = (HTMLDocument)Document.DomDocument;

                if (IsControlSelection)
                {
                    doc.execCommand(@"Delete", false, null);
                }

                string html;

                if (Clipboard.ContainsImage() &&
                    // 2014-08-19, Uwe Keim: Excel hat HTML _und_ Bild, deshalb hier prüfen, sonst
                    //                       wird Excel auch als Bild eingefügt.
                    !Clipboard.ContainsText(TextDataFormat.Html)) 
                {
                    var image = Clipboard.GetImage();
                    var file = Path.Combine(_tmpFolderPath, _objectID.ToString(CultureInfo.InvariantCulture));
                    if (image != null)
                    {
                        image.Save(file, image.RawFormat);
                    }

                    _objectID++;

                    if (Configuration.AllowEmbeddedImages)
                    {
                        var data = File.ReadAllBytes(file);
                        var imageContent = Convert.ToBase64String(data, 0, data.Length);
                        File.Delete(file);

                        html = string.Format(@"<img src=""data:image;base64,{0}"" />", imageContent);
                    }
                    else
                    {
                        html = string.Format(@"<img src=""{0}"" id=""Img{1}"" />", file, DateTime.Now.Ticks);
                    }
                }
                else
                {
                    if (pasteMode != PasteMode.Text && Clipboard.ContainsText(TextDataFormat.Html))
                    {
                        // only body from fragment
                        html = HtmlClipboardHelper.GetHtmlFromClipboard().GetBodyFromHtmlCode().CheckCompleteHtmlTable();

                        // images save or load from web
                        html = checkImages(
                            HtmlConversionHelper.FindImgs(html),
                            html,
                            HtmlClipboardHelper.GetSourceUrlFromClipboard());

                        if (pasteMode == PasteMode.MsWord)
                        {
                            html = html.CleanMsWordHtml();
                        }
                    }
                    else if (Clipboard.ContainsText(TextDataFormat.UnicodeText))
                    {
                        html = Clipboard.GetText(TextDataFormat.UnicodeText);

                        if (pasteMode == PasteMode.Text)
                        {
                            html = html.GetOnlyTextFromHtmlCode();
                        }

                        html = PathHelper.HtmlEncode(html);
                        html = HtmlStringHelper.AddNewLineToText(html);
                    }
                    else if (Clipboard.ContainsText(TextDataFormat.Text))
                    {
                        html = Clipboard.GetText(TextDataFormat.Text);

                        if (pasteMode == PasteMode.Text)
                        {
                            html = html.GetOnlyTextFromHtmlCode();
                        }

                        html = PathHelper.HtmlEncode(html);
                        html = HtmlStringHelper.AddNewLineToText(html);
                    }
                    else
                    {
                        html = string.Empty;
                    }
                }

                var selection = doc.selection;
                var range = (IHTMLTxtRange)selection.createRange();
                range.pasteHTML(html);
            }
        }

        private enum PasteMode
        {
            Normal,
            Text,
            MsWord
        }
    }
}