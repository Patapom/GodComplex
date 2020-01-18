using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

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
	public class Renderer : IDisposable {

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

// 		class HTMLSourceReader : CefSharp.IStringVisitor {
// 			public string	m_HTMLContent = null;
// 			public HTMLSourceReader() {
// 			}
// 			public void Visit( string _str ) {
// 				m_HTMLContent = _str;
// 			}
// 
// 			public void Dispose() {
// 			}
// 		}

		/// <summary>
		/// Used to notify the HTML source is available
		/// </summary>
		/// <param name="_HTMLContent">HTML source</param>
		/// <param name="_DOMElements">The XML document describing the DOM elements present in the returned web page</param>
		public delegate void	WebPageSourceAvailable( string _HTMLContent, XmlDocument _DOMElements );

		/// <summary>
		/// Used to notify a new piece of the web page is available in the form of a rendering
		/// </summary>
		/// <param name="_imageIndex">Index of the piece of image that is available</param>
		/// <param name="_imageWebPage">The piece of rendering of the web page</param>
		public delegate void	WebPageRendered( uint _imageIndex, ImageUtility.ImageFile _imageWebPage );

		/// <summary>
		/// Used to notify of an error in rendering
		/// </summary>
		/// <param name="_errorCode"></param>
		/// <param name="_errorText"></param>
		public delegate void	WebPageErrorOccurred( int _errorCode, string _errorText );

		#endregion

		#region FIELDS

		private int					m_timeOut_ms = 30000;	// Default timeout after 30s

		private ChromiumWebBrowser	m_browser = null;
		public Timer				m_timer = new Timer() { Enabled = false, Interval = 1000 };

// 		public HostHandler host;
// 		private DownloadHandler dHandler;
// 		private ContextMenuHandler mHandler;
// 		private LifeSpanHandler lHandler;
// 		private KeyboardHandler kHandler;
// 		private RequestHandler rHandler;

		private string					m_URL;
		private int						m_scrollsCount;

		private WebPageSourceAvailable	m_pageSourceAvailable;
		private WebPageRendered			m_pageRendered;
		private WebPageErrorOccurred	m_pageError;

		#endregion

		#region

		public Renderer( string _URL, int _browserViewportWidth, int _browserViewportHeight, int _scrollsCount, WebPageSourceAvailable _pageSourceAvailable, WebPageRendered _pageRendered, WebPageErrorOccurred _pageError ) {

//Main( null );

			m_URL = _URL;
			m_scrollsCount = _scrollsCount;
			m_pageSourceAvailable = _pageSourceAvailable;
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

 			if ( _browserViewportHeight == 0 )
 				_browserViewportHeight = (int) (_browserViewportWidth * 1.6180339887498948482045868343656);

			m_browser.Size = new System.Drawing.Size( _browserViewportWidth, _browserViewportHeight );

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

			// First query the HTML source code and DOM content
			await QueryContent();

			// Ask for the page's height (not always reliable, especially on infinite scrolling feeds like facebook or twitter!)
            Task<JavascriptResponse>	task = m_browser.GetBrowser().MainFrame.EvaluateScriptAsync( "(function() { var body = document.body, html = document.documentElement; return Math.max( body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight ); } )();", null );
            task.Wait();
            int	scrollHeight = (int) task.Result.Result;

			// Perform as many screenshots as necessary to capture the entire page
			int	screenshotsCount = (int) Math.Ceiling( (scrollHeight + m_browser.Size.Height-1) / m_browser.Size.Height );

			await DoScreenshots( screenshotsCount );
		}

		/// <summary>
		/// Reads back HTML content and do a screenshot
		/// </summary>
		/// <returns></returns>
		async Task	QueryContent() {
			try {
System.Diagnostics.Debug.WriteLine( "QueryContent for " + m_URL );

				// From Line 162 https://github.com/WildGenie/OSIRTv2/blob/3e60d3ce908a1d25a7b4633dc9afdd53256cbb4f/OSIRT/Browser/MainBrowser.cs#L300
				string	source = await m_browser.GetBrowser().MainFrame.GetSourceAsync();
				if ( source == null )
					throw new Exception( "Failed to retrieve HTML source!" );

System.Diagnostics.Debug.WriteLine( "QueryContent() => Retrieved HTML code " + (source.Length < 100 ? source : source.Remove( 100 )) );

				m_pageSourceAvailable( source, null );

			} catch ( Exception _e ) {
				m_pageError( -1, "An error occurred while attempting to retrieve HTML source for URL \"" + m_URL + "\": \r\n" + _e.Message );
			}
		}

		/// <summary>
		/// Do multiple screenshots
		/// </summary>
		/// <returns></returns>
		async Task	DoScreenshots( int _scrollsCount ) {
			_scrollsCount = Math.Min( m_scrollsCount, _scrollsCount );

			try {
#if true
				// Handle success or timeout (code from https://stackoverflow.com/questions/4238345/asynchronously-wait-for-taskt-to-complete-with-timeout)
//				Task<System.Drawing.Bitmap>	task = m_browser.ScreenshotAsync( true, PopupBlending.Main );
				Task<System.Drawing.Bitmap>	task = m_browser.ScreenshotAsync();
				if ( (await Task.WhenAny( task, Task.Delay( m_timeOut_ms ) )) == task ) {
System.Diagnostics.Debug.WriteLine( "QueryContent() => Retrieved web page image" );

					try {
						ImageUtility.ImageFile image = new ImageUtility.ImageFile( task.Result, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
						m_pageRendered( source, image );
					} catch ( Exception _e ) {
						throw new Exception( "Failed to create image from web page bitmap: \r\n" + _e.Message, _e );
					} finally {
						task.Result.Dispose();	// Always dispose of the bitmap anyway!
					}

				} else {
System.Diagnostics.Debug.WriteLine( "QueryContent() => TIMEOUT!" );
					throw new Exception( "Page rendering timed out" );
				}
#else
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
#endif

			} catch ( Exception _e ) {
				m_pageError( -1, "An error occurred while attempting to render a page screenshot for URL \"" + m_URL + "\": \r\n" + _e.Message );
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

		#endregion
	}
}
