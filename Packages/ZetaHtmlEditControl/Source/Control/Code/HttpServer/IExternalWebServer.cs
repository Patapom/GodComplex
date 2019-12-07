namespace ZetaHtmlEditControl.Code.HttpServer
{
    public interface IExternalWebServer
    {
        /// <summary>
        /// Call with HTML text, get back URL to navigate to.
        /// </summary>
        string SetDocumentText(object sender, string html);
    }
}