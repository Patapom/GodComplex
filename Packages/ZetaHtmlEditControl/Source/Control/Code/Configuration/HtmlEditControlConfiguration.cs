namespace ZetaHtmlEditControl.Code.Configuration
{
    public sealed class HtmlEditControlConfiguration
    {
        public IExternalInformationProvider ExternalInformationProvider { get; set; }

        public bool AllowFontChange { get; set; }
        public bool AllowPrint { get; set; }
        public bool AllowEmbeddedImages { get; set; }

        /// <summary>
        /// Internet Explorer as the used editor sometimes inserts &nbsp; for
        /// no recognizable reason. If this property is set to TRUE, those are
        /// automatically replaced in the returned HTML from the control.
        /// </summary>
        public bool ReplaceNonBreakingSpaceOnGet { get; set; }
    }
}