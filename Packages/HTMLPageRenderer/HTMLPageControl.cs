using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CefSharp;
using CefSharp.OffScreen;

namespace HTMLPageRenderer {

// TODO:
//	• Use schemes to handle local files: https://github.com/cefsharp/CefSharp/wiki/General-Usage#scheme-handler

	/// <summary>
	/// Class wrapping CEF Sharp (Chromium Embedded Framework, .Net wrapper version) to render web pages in an offscreen bitmap
	/// https://github.com/cefsharp/CefSharp/wiki/General-Usage
	/// </summary>
	public class HTMLPageControl : IDisposable {

		class LifeSpanHandler : ILifeSpanHandler {
			public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser) {
				return false;
			}

			public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser) {
			}

			public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser) {
			}

			public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser) {
				// See https://github.com/cefsharp/CefSharp/wiki/General-Usage#popups
				newBrowser = null;
				return true;
			}
		}

		public delegate void	WebPageRendered( string _HTMLContent, ImageUtility.ImageFile _imageWebPage );
		public delegate void	WebPageErrorOccurred( int _errorCode, string _errorText );

		private ChromiumWebBrowser	m_browser = null;
		public Timer				m_timer = new Timer() { Enabled = false, Interval = 1000 };

// 		public HostHandler host;
// 		private DownloadHandler dHandler;
// 		private ContextMenuHandler mHandler;
// 		private LifeSpanHandler lHandler;
// 		private KeyboardHandler kHandler;
// 		private RequestHandler rHandler;

		private string					m_URL;
		private WebPageRendered			m_pageRendered;
		private WebPageErrorOccurred	m_pageError;

		public HTMLPageControl( string _url, int _pageWidth, WebPageRendered _pageRendered, WebPageErrorOccurred _pageError ) {
			m_URL = _url;
			m_pageRendered = _pageRendered;
			m_pageError = _pageError;

			if ( !Cef.IsInitialized ) {
				InitChromium();
			}

			// https://github.com/cefsharp/CefSharp/wiki/General-Usage#cefsettings-and-browsersettings
			BrowserSettings	browserSettings = new BrowserSettings();

// 			dHandler = new DownloadHandler(this);
// 			lHandler = new LifeSpanHandler(this);
// 			mHandler = new ContextMenuHandler(this);
// 			kHandler = new KeyboardHandler(this);
// 			rHandler = new RequestHandler(this);
// 
// 			InitDownloads();
// 
// 			host = new HostHandler(this);

			m_browser = new ChromiumWebBrowser( "", browserSettings );
			m_browser.LifeSpanHandler = new LifeSpanHandler();

			// https://github.com/cefsharp/CefSharp/wiki/General-Usage#handlers
			m_browser.LoadError += browser_LoadError;
			m_browser.LoadingStateChanged += browser_LoadingStateChanged;
			m_browser.FrameLoadEnd += browser_FrameLoadEnd;

			m_timer.Tick += timer_Tick;

			m_browser.Size = new System.Drawing.Size( _pageWidth, _pageWidth * 9 / 16 );

			m_browser.BrowserInitialized += browser_BrowserInitialized;
		}

		private void browser_BrowserInitialized(object sender, EventArgs e) {
			m_browser.Load( m_URL );
		}

		private void browser_LoadError(object sender, LoadErrorEventArgs e) {
			m_pageError( (int) e.ErrorCode, e.ErrorText );
		}

		private void browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e) {
			RestartTimer();
		}

		private void browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e) {
			if ( e.IsLoading )
				return;

			RestartTimer();
		}

		void	RestartTimer() {
			m_timer.Enabled = false;
			m_timer.Enabled = true;
		}

		private void timer_Tick(object sender, EventArgs e) {
			m_timer.Enabled = false;	// Prevent any further tick

			try {
				// Read back HTML content and do a screenshot
				HTMLSourceReader	sourceReader = new HTMLSourceReader();
				m_browser.GetBrowser().MainFrame.GetSource( sourceReader );

source null et bitmap null! Flûte caca boudin!

				ImageUtility.ImageFile	image = null;
				using ( System.Drawing.Bitmap B = m_browser.ScreenshotOrNull( PopupBlending.Main ) ) {
					image = new ImageUtility.ImageFile( B, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
				}

				try {
					m_pageRendered( sourceReader.m_HTMLContent, image );
				} catch ( Exception _e ) {
					throw new Exception( "An error occurred during the notification of web page successfully loaded!", _e );
				} finally {
					image.Dispose();
				}

			} catch ( Exception _e ) {
				throw new Exception( "An error occurred while attempting to retrieve HTML source and page screenshot!", _e );
			}
		}

		class HTMLSourceReader : CefSharp.IStringVisitor {
			public string	m_HTMLContent = null;
			public HTMLSourceReader() {
			}
			public void Visit( string _str ) {
				m_HTMLContent = _str;
			}

			public void Dispose() {
			}
		}

		public void Dispose() {
			m_browser.Dispose();
		}

		#region Static CEF Init/Exit

		// https://github.com/cefsharp/CefSharp/wiki/General-Usage#initialize-and-shutdown

		public static void	InitChromium() {
			// We're going to manually call Cef.Shutdown
            CefSharpSettings.ShutdownOnExit = false;

			CefSettings	settings = new CefSettings();
 			Cef.Initialize( settings );
		}

		public static void	ExitChromium() {
			Cef.Shutdown();
		}

		#endregion
	}
}
