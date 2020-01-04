using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CefSharp;
using CefSharp.OffScreen;

namespace HTMLPageRenderer {
	public class HTMLPageControl : IDisposable {

		#region Static Browser

		private ChromiumWebBrowser	m_browser = null;

// 		public HostHandler host;
// 		private DownloadHandler dHandler;
// 		private ContextMenuHandler mHandler;
// 		private LifeSpanHandler lHandler;
// 		private KeyboardHandler kHandler;
// 		private RequestHandler rHandler;

		public HTMLPageControl( string _url ) {

//			CefSettings	settings = new CefSettings();
//			BrowserSettings	settings 

// 			Cef.Initialize(settings);
// 
// 			dHandler = new DownloadHandler(this);
// 			lHandler = new LifeSpanHandler(this);
// 			mHandler = new ContextMenuHandler(this);
// 			kHandler = new KeyboardHandler(this);
// 			rHandler = new RequestHandler(this);
// 
// 			InitDownloads();
// 
// 			host = new HostHandler(this);

			m_browser = new ChromiumWebBrowser( _url );
		}

		public void Dispose() {
			m_browser.Dispose();
		}

		#endregion
	}
}
