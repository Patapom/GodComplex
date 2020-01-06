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
//	• Example: Capture Full Page Using Scrolling https://github.com/WildGenie/OSIRTv2/blob/3e60d3ce908a1d25a7b4633dc9afdd53256cbb4f/OSIRT/Browser/MainBrowser.cs#L300
//	• Download element rectangles! => Use JS

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

		public HTMLPageControl( string _url, int _browserViewportWidth, int _maxPageHeight, WebPageRendered _pageRendered, WebPageErrorOccurred _pageError ) {

//Main( null );

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

			if ( _maxPageHeight == 0 )
				_maxPageHeight = _browserViewportWidth * 9 / 16;

			m_browser.Size = new System.Drawing.Size( _browserViewportWidth, _maxPageHeight );

			m_browser.BrowserInitialized += browser_BrowserInitialized;
		}

		private void browser_BrowserInitialized(object sender, EventArgs e) {
System.Diagnostics.Debug.WriteLine( "browser_BrowserInitialized" );

			m_browser.Load( m_URL );
		}

		private void browser_LoadError(object sender, LoadErrorEventArgs e) {
System.Diagnostics.Debug.WriteLine( "browser_LoadError: " + e.ErrorText );

			switch ( (uint) e.ErrorCode ) {
				case 0xffffffe5U:
					return;	// Ignore...
			}

			m_pageError( (int) e.ErrorCode, e.ErrorText );
		}

		private void browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e) {
System.Diagnostics.Debug.WriteLine( "browser_FrameLoadEnd" );

			RestartTimer();
		}

		private void browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e) {
			if ( e.IsLoading )
				return;

System.Diagnostics.Debug.WriteLine( "browser_LoadingStateChanged" );

			RestartTimer();
		}

		void	RestartTimer() {
			m_timer.Enabled = false;
			m_timer.Enabled = true;
		}

		bool	m_contentQueried = false;
		private async void timer_Tick(object sender, EventArgs e) {
			m_timer.Enabled = false;	// Prevent any further tick

			m_browser.LoadError -= browser_LoadError;
			m_browser.LoadingStateChanged -= browser_LoadingStateChanged;
			m_browser.FrameLoadEnd -= browser_FrameLoadEnd;

			if ( m_contentQueried )
				return;

			m_contentQueried = true;	// Don't re-enter!

			await QueryContent();
		}

		async Task	QueryContent() {
			try {
System.Diagnostics.Debug.WriteLine( "QueryContent for " + m_URL );

				// Read back HTML content and do a screenshot
// 				HTMLSourceReader	sourceReader = new HTMLSourceReader();
// 				m_browser.GetBrowser().MainFrame.GetSource( sourceReader );

				// From Line 162 https://github.com/WildGenie/OSIRTv2/blob/3e60d3ce908a1d25a7b4633dc9afdd53256cbb4f/OSIRT/Browser/MainBrowser.cs#L300
				string	source = await m_browser.GetBrowser().MainFrame.GetSourceAsync();
				if ( source == null )
					throw new Exception( "Failed to retrieve HTML source!" );

System.Diagnostics.Debug.WriteLine( "QueryContent() => Retrieved HTML code " + (source.Length < 100 ? source : source.Remove( 100 )) );

				ImageUtility.ImageFile	image = null;
//				using ( System.Drawing.Bitmap B = m_browser.ScreenshotOrNull( PopupBlending.Main ) ) {
				using ( System.Drawing.Bitmap B = await m_browser.ScreenshotAsync( true, PopupBlending.Main ) ) {
					image = new ImageUtility.ImageFile( B, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
				}

System.Diagnostics.Debug.WriteLine( "QueryContent() => Retrieved web page image" );

				try {
//					m_pageRendered( sourceReader.m_HTMLContent, image );
					m_pageRendered( source, image );
				} catch ( Exception _e ) {
					throw new Exception( "An error occurred during the notification of web page successfully loaded!", _e );
				} finally {
					image.Dispose();
				}

			} catch ( Exception _e ) {
				m_pageError( -1, "An error occurred while attempting to retrieve HTML source and page screenshot!\r\n" + _e.Message );
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
 			Cef.Initialize( settings, performDependencyCheck: true, browserProcessHandler: null );
		}

		public static void	ExitChromium() {
			Cef.Shutdown();
		}

		#endregion

		#region Minimal Offscreen Rendering Example (working!)

		// Source from https://github.com/cefsharp/CefSharp.MinimalExample/blob/master/CefSharp.MinimalExample.OffScreen/Program.cs

		private static ChromiumWebBrowser browser;

		public static void Main(string[] args)
		{
			const string testUrl = "https://www.patapom.com/";

			Console.WriteLine("This example application will load {0}, take a screenshot, and save it to your desktop.", testUrl);
			Console.WriteLine("You may see Chromium debugging output, please wait...");
			Console.WriteLine();

			var settings = new CefSettings()
			{
				//By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
				CachePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
			};

			//Perform dependency check to make sure all relevant resources are in our output directory.
			Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

			// Create the offscreen Chromium browser.
			browser = new ChromiumWebBrowser(testUrl);

			// An event that is fired when the first page is finished loading.
			// This returns to us from another thread.
			browser.LoadingStateChanged += BrowserLoadingStateChanged;

			// We have to wait for something, otherwise the process will exit too soon.
//			Console.ReadKey();
			System.Threading.Thread.Sleep( 30000 );

			// Clean up Chromium objects.  You need to call this in your application otherwise
			// you will get a crash when closing.
			Cef.Shutdown();
		}

		private static void BrowserLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
		{
			// Check to see if loading is complete - this event is called twice, one when loading starts
			// second time when it's finished
			// (rather than an iframe within the main frame).
			if (!e.IsLoading)
			{
				// Remove the load event handler, because we only want one snapshot of the initial page.
				browser.LoadingStateChanged -= BrowserLoadingStateChanged;

				var scriptTask = browser.EvaluateScriptAsync("document.getElementById('lst-ib').value = 'CefSharp Was Here!'");

				scriptTask.ContinueWith(t =>
				{
					//Give the browser a little time to render
					System.Threading.Thread.Sleep(500);
					// Wait for the screenshot to be taken.
					var task = browser.ScreenshotAsync();
					task.ContinueWith(x =>
					{
						// Make a file to save it to (e.g. C:\Users\jan\Desktop\CefSharp screenshot.png)
						var screenshotPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CefSharp screenshot.png");

						Console.WriteLine();
						Console.WriteLine("Screenshot ready. Saving to {0}", screenshotPath);

						// Save the Bitmap to the path.
						// The image type is auto-detected via the ".png" extension.
						task.Result.Save(screenshotPath);

						// We no longer need the Bitmap.
						// Dispose it to avoid keeping the memory alive.  Especially important in 32-bit applications.
						task.Result.Dispose();

						Console.WriteLine("Screenshot saved.  Launching your default image viewer...");

						// Tell Windows to launch the saved image.
						System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(screenshotPath)
						{
							// UseShellExecute is false by default on .NET Core.
							UseShellExecute = true
						});

						Console.WriteLine("Image viewer launched.  Press any key to exit.");
					}, TaskScheduler.Default);
				});
			}
		}

		#endregion

	}
}
