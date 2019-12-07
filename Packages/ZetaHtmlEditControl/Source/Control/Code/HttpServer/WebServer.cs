namespace ZetaHtmlEditControl.Code.HttpServer
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;
    using global::HttpServer;
    using global::HttpServer.HttpModules;
    using global::HttpServer.Sessions;
    using Helper;
    using Properties;

    internal sealed class WebServer :
        IExternalWebServer
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());
        private static readonly List<int> ReservedPorts = new List<int>();
        private readonly List<TextInfo> _texts = new List<TextInfo>();
        private readonly object _thisLock = new object();
        private int _counter;
        private string _localHost;
        private int _port;
        private HttpServer _server;

        private string baseUrl
        {
            get
            {
                return
                    string.Format(
                    @"http://{0}:{1}/texts/",
                    localHost,
                    _port);
            }
        }

        //private string localHost
        //{
        //    get
        //    {
        //        return _localHost ?? (_localHost = @"127.0.0.1");
        //    }
        //}

        /*
         * The following is commented out, because on some systems with IE6/7/8,
         * nothing gets loaded in the HTML editor when accessing LOCALHOST. WTF?!?
         */

        private string localHost
        {
            get
            {
                // Try to use something without dots, if available.
                // http://serverfault.com/questions/19820/internet-explorer-not-bypassing-proxy-for-local-addresses/19916#19916

                if (_localHost == null)
                {
                    // Default.
                    _localHost = @"127.0.0.1";

                    var i = ConfigurationManager.AppSettings[@"htmlEditor.localHost.intelligent"];

                    if (string.IsNullOrEmpty(i) || i.ToLowerInvariant() == @"true")
                    {
                        try
                        {
                            var lh = Dns.GetHostEntry(@"localhost");
                            if (lh.AddressList.Length > 0)
                            {
                                foreach (var address in lh.AddressList)
                                {
                                    if (address.ToString() == @"127.0.0.1")
                                    {
                                        _localHost = @"localhost";
                                    }
                                }
                            }
                        }
                        catch (SocketException)
                        {
                            // Do nothing, use default, set above.
                        }
                    }
                }

                return _localHost;
            }
        }

        public string SetDocumentText(object sender, string html)
        {
            lock (_thisLock)
            {
                var hc = sender.GetHashCode();

                // Look first, whether already present.
                foreach (var info in _texts)
                {
                    if (info.SenderHashCode == hc)
                    {
                        info.Html = html;
                        var r = baseUrl + info.Key;

                        Trace.WriteLine(
                            string.Format(
                                @"[Web server] Setting EXISTING document text for URL '{0}': '{1}'. Stack: {2}.",
                                r,
                                html,
                                new StackTrace()));
                        return r;
                    }
                }

                // --

                var key = (++_counter).ToString(CultureInfo.InvariantCulture);
                _texts.Add(new TextInfo { SenderHashCode = hc, Key = key, Html = html });

                var s = baseUrl + key;

                Trace.WriteLine(
                    string.Format(
                        @"[Web server] Setting NEW document text for URL '{0}': '{1}'. Stack: {2}.",
                        s,
                        html,
                        new StackTrace()));

                return s;
            }
        }

        private TextInfo getDictionary(string key)
        {
            lock (_thisLock)
            {
                return _texts.FirstOrDefault(info => info.Key == key);

                // Not found.
            }
        }

        public void Uninitialize()
        {
            if (_server != null)
            {
                var listener = _server;
                _server = null;
                listener.Stop();
            }
        }

        public void Initialize()
        {
            if (_server == null)
            {
                _port = getFreePort();

                _server = new HttpServer(new MyLogWriter());

                _server.Add(new MyModule(this));
                _server.Start(IPAddress.Loopback, _port);

                Trace.WriteLine(
                    string.Format(
                        @"[Web server] Started local web server for URL '{0}'.",
                        baseUrl));
            }
        }

        private static void checkSendText(
            IHttpRequest request,
            IHttpResponse response,
            string text)
        {
            //if (!string.IsNullOrEmpty(text))
            {
                response.ContentType = @"text/html";

                if (!string.IsNullOrEmpty(request.Headers[@"if-Modified-Since"]))
                {
#pragma warning disable 162
                    response.Status = HttpStatusCode.OK;
#pragma warning restore 162
                }

                addNeverCache(response);

                if (request.Method != @"Headers" && response.Status != HttpStatusCode.NotModified)
                {
                    Trace.WriteLine(
                        string.Format(
                            @"[Web server] Sending text for URL '{0}': '{1}'.",
                            request.Uri.AbsolutePath,
                            text));

                    var buffer2 = getBytesWithBom(text);

                    response.ContentLength = buffer2.Length;
                    response.SendHeaders();

                    response.SendBody(buffer2, 0, buffer2.Length);
                }
                else
                {
                    response.ContentLength = 0;
                    response.SendHeaders();

                    Trace.WriteLine(@"[Web server] Not sending.");
                }
            }
        }

        private static byte[] getBytesWithBom(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        private static void addNeverCache(IHttpResponse response)
        {
            response.AddHeader(@"Last-modified", new DateTime(2005, 1, 1).ToUniversalTime().ToString(@"r"));

            response.AddHeader(@"Cache-Control", @"no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
            response.AddHeader(@"Pragma", @"no-cache");
        }

        private static string removeUriStart(string uriStart, string uri)
        {
            return uri.StartsWith(uriStart) ? uri.Substring(uriStart.Length) : uri;
        }

        private static string cleanUriEnd(string uri)
        {
            var index = uri.IndexOf('?');
            return index >= 0 ? uri.Substring(0, index) : uri;
        }

        private static int getFreePort()
        {
            var alt = ConvertHelper.ToInt32(ConfigurationManager.AppSettings[@"webserver.listenPort"]);

            if (alt > 0)
            {
                alt += ReservedPorts.Count;
                if (isPortFree(alt))
                {
                    ReservedPorts.Add(alt);
                    return alt;
                }
            }

            // --

            for (var i = 0; i < 10; ++i)
            {
                var port = Random.Next(9000, 15000);
                if (isPortFree(port))
                {
                    ReservedPorts.Add(port);
                    return port;
                }
            }

            throw new Exception(Resources.WebServer_getFreePort_Unable_to_acquire_free_port_);
        }

        private static bool isPortFree(int port)
        {
            if (ReservedPorts.Contains(port))
            {
                return false;
            }
            else
            {
                // http://stackoverflow.com/a/570126/107625

                var globalProperties = IPGlobalProperties.GetIPGlobalProperties();
                var informations = globalProperties.GetActiveTcpListeners();

                return informations.All(information => information.Port != port);

                /*
                // http://stackoverflow.com/questions/570098/in-c-how-to-check-if-a-tcp-port-is-available

                var globalProperties = IPGlobalProperties.GetIPGlobalProperties();
                var informations = globalProperties.GetActiveTcpConnections();

                foreach (var information in informations)
                {
                    if (information.LocalEndPoint.Port == port)
                    {
                        return false;
                    }
                }

                return true;
                */
            }
        }

        private class MyLogWriter :
            ILogWriter
        {
            private static void traceLog(
                string type,
                string message)
            {
                Trace.WriteLine(
                    string.Format(
                        @"[Preview web server, {0}] {1}",
                        type,
                        message));
            }

            #region Implementation of ILogWriter

            public void Write(object source, LogPrio priority, string message)
            {
                traceLog(priority.ToString(), message);
            }

            #endregion
        }

        private class MyModule :
            HttpModule
        {
            private readonly WebServer _owner;

            public MyModule(WebServer owner)
            {
                _owner = owner;
            }

            public override bool Process(
                IHttpRequest request,
                IHttpResponse response,
                IHttpSession session)
            {
                var urlAbsolute = request.Uri.AbsolutePath;

                if (urlAbsolute.StartsWith(@"/texts"))
                {
                    var text = _owner.getDictionary(cleanUriEnd(removeUriStart(@"/texts/", urlAbsolute)));
                    checkSendText(request, response, text == null ? null : text.Html);
                    return true;
                }
                else
                {
                    // Ignore several known URLs.
                    if (urlAbsolute.StartsWith(@"/favicon.ico"))
                    {
                        return false;
                    }
                    else
                    {
                        throw new Exception(string.Format(Resources.MyModule_Process_Unexpected_path___0___, urlAbsolute));
                    }
                }
            }
        }

        private class TextInfo
        {
            public int SenderHashCode { get; set; }
            public string Key { get; set; }
            public string Html { get; set; }
        }
    }
}