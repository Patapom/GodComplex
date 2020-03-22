using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;

using CefSharp;
using CefSharp.OffScreen;

namespace WebServices {

	/// <summary>
	/// Pool of re-usable browsers
	/// </summary>
	public class BrowsersPool : IDisposable {

		#region NESTED TYPES

		/// <summary>
		/// Class wrapping CEF Sharp (Chromium Embedded Framework, .Net wrapper version)
		/// https://github.com/cefsharp/CefSharp/wiki/General-Usage
		/// </summary>
		[System.Diagnostics.DebuggerDisplay( "{m_name} {Size.Width,d}x{Size.Height,d} Loading={IsLoading}" )]
		public class Browser : IDisposable {

			#region NESTED TYPES

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

			#endregion

			#region FIELDS

			private BrowsersPool			m_owner;
			private string					m_name;
			private ChromiumWebBrowser		m_browser = null;

// 			public HostHandler host;
// 			private DownloadHandler dHandler;
// 			private ContextMenuHandler mHandler;
// 			private LifeSpanHandler lHandler;
// 			private KeyboardHandler kHandler;
// 			private RequestHandler rHandler;

			#endregion

			#region PROPERTIES

			public string					Name					{ get { return m_name; } }
			public bool						IsLoading				{ get { return m_browser.IsLoading; } }
			public bool						IsBrowserInitialized	{ get { return m_browser.IsBrowserInitialized; } }
			public System.Drawing.Size		Size					{ get { return m_browser.Size; } set { m_browser.Size = value; } }
			public IFrame					MainFrame				{ get { return m_browser.GetBrowser().MainFrame; } }

			public event EventHandler<FrameLoadStartEventArgs>				FrameLoadStart				{ add { m_browser.FrameLoadStart += value; } remove { m_browser.FrameLoadStart -= value; } }
//			public event EventHandler<AddressChangedEventArgs>				AddressChanged				{ add { m_browser.AddressChanged += value; } remove { m_browser.AddressChanged -= value; } }
			public event EventHandler<LoadingStateChangedEventArgs>			LoadingStateChanged			{ add { m_browser.LoadingStateChanged += value; } remove { m_browser.LoadingStateChanged -= value; } }
// 			public event EventHandler<StatusMessageEventArgs>				StatusMessage				{ add { m_browser.StatusMessage += value; } remove { m_browser.StatusMessage -= value; } }
// 			public event EventHandler										BrowserInitialized			{ add { m_browser.BrowserInitialized += value; } remove { m_browser.BrowserInitialized -= value; } }
// 			public event EventHandler<ConsoleMessageEventArgs>				ConsoleMessage				{ add { m_browser.ConsoleMessage += value; } remove { m_browser.ConsoleMessage -= value; } }
			public event EventHandler<FrameLoadEndEventArgs>				FrameLoadEnd				{ add { m_browser.FrameLoadEnd += value; } remove { m_browser.FrameLoadEnd -= value; } }
			public event EventHandler<LoadErrorEventArgs>					LoadError					{ add { m_browser.LoadError += value; } remove { m_browser.LoadError -= value; } }
// 			public event EventHandler<JavascriptMessageReceivedEventArgs>	JavascriptMessageReceived	{ add { m_browser.JavascriptMessageReceived += value; } remove { m_browser.JavascriptMessageReceived -= value; } }

			#endregion

			#region METHODS

			public Browser( BrowsersPool _owner, string _name ) {
				m_owner = _owner;
				m_name = _name;

				// https://github.com/cefsharp/CefSharp/wiki/General-Usage#cefsettings-and-browsersettings
				BrowserSettings	browserSettings = new BrowserSettings();

// 				dHandler = new DownloadHandler(this);
// 				lHandler = new LifeSpanHandler(this);
// 				mHandler = new ContextMenuHandler(this);
// 				kHandler = new KeyboardHandler(this);
// 				rHandler = new RequestHandler(this);
// 
// 				InitDownloads();
// 
// 				host = new HostHandler(this);

				m_browser = new ChromiumWebBrowser( "", browserSettings );
				m_browser.BrowserInitialized += browser_BrowserInitialized;
				m_browser.LifeSpanHandler = new LifeSpanHandler();

// 				// https://github.com/cefsharp/CefSharp/wiki/General-Usage#handlers
// 				m_browser.LoadError += browser_LoadError;
// 				m_browser.LoadingStateChanged += browser_LoadingStateChanged;
// 				m_browser.FrameLoadStart += browser_FrameLoadStart;
// 				m_browser.FrameLoadEnd += browser_FrameLoadEnd;
			}

			public void Dispose() {
				m_browser.Stop();
				m_browser.Dispose();
				m_browser = null;
			}

			public void	Release() {
				m_owner.Release( this );
			}

			public void	Load( string _URL ) {
				m_browser.Load( _URL );
			}

			public void	Stop() {
				m_browser.Stop();
			}

			public async Task<System.Drawing.Bitmap>	ScreenshotAsync( bool ignoreExistingScreenshot = false, PopupBlending blend = PopupBlending.Main ) {
				return await m_browser.ScreenshotAsync( ignoreExistingScreenshot, blend );
			}

			#endregion

			#region EVENT HANDLERS

			private void browser_BrowserInitialized(object sender, EventArgs e) {
//Log( LOG_TYPE.DEBUG, "browser_BrowserInitialized" );
//				Load( m_URL );
			}

			#endregion
		}

		#endregion

		#region FIELDS

		Browser[]			m_pool = null;

		List< Browser >		m_usedBrowsers = new List<Browser>();
		Queue< Browser >	m_freeBrowsers = new Queue<Browser>();

		#endregion

		#region METHODS

		public BrowsersPool( uint _browsersCount ) {
			if ( !Cef.IsInitialized ) {
				InitChromium();
			}

			m_pool = new Browser[_browsersCount];
			for ( int browserIndex=0; browserIndex < _browsersCount; browserIndex++ ) {
				m_pool[browserIndex] = new Browser( this, browserIndex.ToString() );
				m_freeBrowsers.Enqueue( m_pool[browserIndex] );
			}

			// Wait until all browsers are initialized
			bool	allBrowsersReady = false;
			int		spinsCount = 0;
			while ( !allBrowsersReady ) {
				allBrowsersReady = true;
				for ( int browserIndex=0; browserIndex < _browsersCount; browserIndex++ ) {
					if ( !m_pool[browserIndex].IsBrowserInitialized ) {
						allBrowsersReady = false;
						spinsCount++;
						System.Threading.Thread.Sleep( 100 );
						continue;
					}
				}
			}
		}

		public void Dispose() {
			foreach ( Browser browser in m_pool )
				browser.Dispose();
			m_pool = null;

			ExitChromium();
		}

		/// <summary>
		/// Requests a free browser
		/// </summary>
		/// <returns></returns>
		public Browser	RequestBrowser() {
			Browser	result = null;
			while ( result == null ) {
				lock ( m_freeBrowsers ) {
					if ( m_freeBrowsers.Count > 0 ) {
						// Found a free browser!
						result = m_freeBrowsers.Dequeue();
						lock ( m_usedBrowsers )
							m_usedBrowsers.Add( result );	// Not free anymore!
					}
				}
				if ( result != null )
					return result;

				System.Threading.Thread.Sleep( 250 );
			}

			return null;
		}

		private void	Release( Browser _browser ) {
			lock ( m_usedBrowsers ) {
				if ( !m_usedBrowsers.Contains( _browser ) )
					throw new Exception( "Browser " + _browser.Name + " is not being used! No need to release it!" );

				m_usedBrowsers.Remove( _browser );
				lock ( m_freeBrowsers ) {
					m_freeBrowsers.Enqueue( _browser );
				}
			}
		}

		#region Static CEF Init/Exit

		// https://github.com/cefsharp/CefSharp/wiki/General-Usage#initialize-and-shutdown

		public static void	InitChromium() {
			// We're going to manually call Cef.Shutdown
            CefSharpSettings.ShutdownOnExit = false;

			CefSettings	settings = new CefSettings();
 			Cef.Initialize( settings, performDependencyCheck: true, browserProcessHandler: null );
		}

		public static void	ExitChromium() {
			Cef.Shutdown();
		}

		#endregion

		#endregion
	}
}
