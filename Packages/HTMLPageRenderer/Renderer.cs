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
		/// Used to notify the web page was successfully loaded
		/// </summary>
		public delegate void	WebPageSuccess();

		/// <summary>
		/// Used to notify of an error in rendering
		/// </summary>
		/// <param name="_errorCode"></param>
		/// <param name="_errorText"></param>
		public delegate void	WebPageErrorOccurred( int _errorCode, string _errorText );

		#endregion

		#region FIELDS

		private int					m_TimeOut_ms_JavascriptNoRender = 1000;	// Default timeout after 1s of a JS command that doesn't trigger a new rendering
		private int					m_TimeOut_ms_PageRender = 30000;		// Default timeout after 30s for a page rendering
		private int					m_TimeOut_ms_Screenshot = 1000;			// Default timeout after 1s for a screenshot

		private ChromiumWebBrowser	m_browser = null;
		public Timer				m_timer = new Timer() { Enabled = false, Interval = 1000 };

// 		public HostHandler host;
// 		private DownloadHandler dHandler;
// 		private ContextMenuHandler mHandler;
// 		private LifeSpanHandler lHandler;
// 		private KeyboardHandler kHandler;
// 		private RequestHandler rHandler;

		private string					m_URL;
		private int						m_maxScreenshotsCount;

		private WebPageSourceAvailable	m_pageSourceAvailable;
		private WebPageRendered			m_pageRendered;
		private WebPageSuccess			m_pageSuccess;
		private WebPageErrorOccurred	m_pageError;

		#endregion

		#region

		public Renderer( string _URL, int _browserViewportWidth, int _browserViewportHeight, int _maxScreenshotsCount, WebPageSourceAvailable _pageSourceAvailable, WebPageRendered _pageRendered, WebPageSuccess _pageSuccess, WebPageErrorOccurred _pageError ) {

//Main( null );

			m_URL = _URL;
			m_maxScreenshotsCount = _maxScreenshotsCount;
			m_pageSourceAvailable = _pageSourceAvailable;
			m_pageRendered = _pageRendered;
			m_pageSuccess = _pageSuccess;
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
			m_browser.FrameLoadStart += browser_FrameLoadStart;
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

		private void browser_FrameLoadStart(object sender, FrameLoadStartEventArgs e) {
System.Diagnostics.Debug.WriteLine( "browser_FrameLoadStart" );

			RestartTimer();
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

		bool	m_pageStable = false;
		void	RestartTimer() {
			// Mark the page as unstable... It will be marked as stable again if the timer fires, meaning there hasn't been a single loading event for enough time to consider the page loading as completed.
			m_pageStable = false;

			m_timer.Stop();
			m_timer.Start();
		}

		/// <summary>
		/// Executes a task for a given amount of time before it times out
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_task"></param>
		/// <param name="_timeOut_ms"></param>
		/// <returns></returns>
		async Task<Task>	ExecuteTaskOrTimeOut< T >( T _task, int _timeOut_ms ) where T : Task {
			if ( (await Task.WhenAny( _task, Task.Delay( _timeOut_ms ) )) != _task ) {
				_task.Dispose();
				throw new TimeoutException();
			}

			return _task;
		}

		async Task	AsyncWaitForStablePage() {
			while( !m_pageStable ) {
				await Task.Delay( 500 );  // We do need these delays. Some pages, like facebook, may need to load viewport content.
			}
		}

		async Task	WaitForStablePage() {
			await ExecuteTaskOrTimeOut( AsyncWaitForStablePage(), m_TimeOut_ms_PageRender );
		}

		async Task<JavascriptResponse>	ExecuteJS( string _JS ) {
//			return (await ExecuteTaskOrTimeOut( m_browser.GetBrowser().MainFrame.EvaluateScriptAsync( _JS ), m_TimeOut_ms_JavascriptNoRender )) as Task<JavascriptResponse>;

			Task<JavascriptResponse>	task = (await ExecuteTaskOrTimeOut( m_browser.GetBrowser().MainFrame.EvaluateScriptAsync( _JS, null ), m_TimeOut_ms_JavascriptNoRender )) as Task<JavascriptResponse>;
			return task.Result;
		}

		bool	m_contentQueried = false;
		private async void timer_Tick(object sender, EventArgs e) {
System.Diagnostics.Debug.WriteLine( "timer_Tick()" );

			m_timer.Enabled = false;	// Prevent any further tick

// 			m_browser.LoadError -= browser_LoadError;
// 			m_browser.LoadingStateChanged -= browser_LoadingStateChanged;
// 			m_browser.FrameLoadEnd -= browser_FrameLoadEnd;

			// Raise a "stable" flag once dust seems to have settled for a moment...
			m_pageStable = true;

			//////////////////////////////////////////////////////////////////////////
			/// If first occurrence of stable page then we can start our grabbing operation
			if ( m_contentQueried )
				return;

			m_contentQueried = true;	// Don't re-enter!

			// First query the HTML source code and DOM content
			await QueryContent();

			// Ask for the page's height (not always reliable, especially on infinite scrolling feeds like facebook or twitter!)
//			Task<JavascriptResponse>	task = m_browser.GetBrowser().MainFrame.EvaluateScriptAsync( "(function() { var body = document.body, html = document.documentElement; return Math.max( body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight ); } )();", null );
// 			task.Wait();

//			Task<JavascriptResponse>	task = ExecuteTaskOrTimeOut( m_browser.GetBrowser().MainFrame.EvaluateScriptAsync( "(function() { var body = document.body, html = document.documentElement; return Math.max( body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight ); } )();", null ), m_TimeOut_ms_JavascriptNoRender ).Result as Task<JavascriptResponse>;
//			int	scrollHeight = (int) task.Result.Result;
			JavascriptResponse	JSResult = await ExecuteJS( "(function() { var body = document.body, html = document.documentElement; return Math.max( body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight ); } )();" );
			int	scrollHeight = (int) JSResult.Result;

			// Perform as many screenshots as necessary to capture the entire page
			int	viewportHeight = m_browser.Size.Height;
			int	screenshotsCount = (int) Math.Ceiling( (double) scrollHeight / viewportHeight );

			var	onSenFout = DoScreenshots( screenshotsCount );
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

// @TODO: Parse DOM!

				// Notify source is ready
				m_pageSourceAvailable( source, null );

			} catch ( Exception _e ) {
				m_pageError( -1, "An error occurred while attempting to retrieve HTML source for URL \"" + m_URL + "\": \r\n" + _e.Message );
			}
		}


		/// <summary>
		/// Do multiple screenshots to capture the entire page
		/// </summary>
		/// <returns></returns>
		async Task	DoScreenshots( int _scrollsCount ) {
			_scrollsCount = Math.Min( m_maxScreenshotsCount, _scrollsCount );

			try {
				// Code from https://github.com/WildGenie/OSIRTv2/blob/3e60d3ce908a1d25a7b4633dc9afdd53256cbb4f/OSIRT/Browser/MainBrowser.cs#L300
//				await m_browser.GetBrowser().MainFrame.EvaluateScriptAsync("(function() { document.documentElement.style.overflow = 'hidden'; })();");
//				await ExecuteTaskOrTimeOut( m_browser.GetBrowser().MainFrame.EvaluateScriptAsync( "(function() { document.documentElement.style.overflow = 'hidden'; })();" ), m_TimeOut_ms_JavascriptNoRender );
				await ExecuteJS( "(function() { document.documentElement.style.overflow = 'hidden'; })();" );

				uint	viewportHeight = (uint) m_browser.Size.Height;
				for ( uint scrollIndex=0; scrollIndex < _scrollsCount; scrollIndex++ ) {

					try {
						//////////////////////////////////////////////////////////////////////////
						/// Request a screenshot
System.Diagnostics.Debug.WriteLine( "DoScreenshots() => Requesting screenshot {0}", scrollIndex );

// 						Task<System.Drawing.Bitmap>	task = m_browser.ScreenshotAsync();
// 						if ( (await Task.WhenAny( task, Task.Delay( m_TimeOut_ms_PageRender ) )) == task ) {

 						Task<System.Drawing.Bitmap>	task = (await ExecuteTaskOrTimeOut( m_browser.ScreenshotAsync(), m_TimeOut_ms_Screenshot )) as Task<System.Drawing.Bitmap>;

System.Diagnostics.Debug.WriteLine( "DoScreenshots() => Retrieved web page image screenshot {0} / {1}", 1+scrollIndex, _scrollsCount );

						try {
							ImageUtility.ImageFile image = new ImageUtility.ImageFile( task.Result, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
							m_pageRendered( scrollIndex, image );
						} catch ( Exception _e ) {
							throw new Exception( "Failed to create image from web page bitmap: \r\n" + _e.Message, _e );
						} finally {
							task.Result.Dispose();	// Always dispose of the bitmap anyway!
						}

						//////////////////////////////////////////////////////////////////////////
						/// Scroll down the page
						if ( scrollIndex < _scrollsCount-1 ) {
System.Diagnostics.Debug.WriteLine( "DoScreenshots() => Requesting scrolling... (should retrigger rendering)" );

							// Mark the page as "unstable" and scroll down until we reach the bottom (if it exists, or until we reach the specified maximum amount of authorized screenshots)
							RestartTimer();
//							await m_browser.GetBrowser().MainFrame.EvaluateScriptAsync("(function() { window.scroll(0," + ((scrollIndex+1) * viewportHeight) + "); })();");
							await ExecuteJS( "(function() { window.scroll(0," + ((scrollIndex+1) * viewportHeight) + "); })();" );

							// Wait for the page to stabilize (i.e. the timer hasn't been reset for some time, indicating most elements should be ready)
							await WaitForStablePage();

System.Diagnostics.Debug.WriteLine( "DoScreenshots() => Scrolling done!" );
						}

					} catch ( TimeoutException ) {
System.Diagnostics.Debug.WriteLine( "DoScreenshots() => TIMEOUT!" );
//						throw new Exception( "Page rendering timed out" );
//m_pageError()
					} catch ( Exception _e ) {
System.Diagnostics.Debug.WriteLine( "DoScreenshots() => EXCEPTION! " + _e.Message );
					}
				}

				// Notify the page was successfully loaded
				m_pageSuccess();

			} catch ( Exception _e ) {
				m_pageError( -1, "An error occurred while attempting to render a page screenshot for URL \"" + m_URL + "\": \r\n" + _e.Message );
			}
		}


		/// <summary>
		/// Do multiple screenshots to capture the entire page
		/// </summary>
		/// <returns></returns>
		async Task	OLD_DoScreenshots( int _scrollsCount ) {
			_scrollsCount = Math.Min( m_maxScreenshotsCount, _scrollsCount );

			try {
#if true
				// Code from https://github.com/WildGenie/OSIRTv2/blob/3e60d3ce908a1d25a7b4633dc9afdd53256cbb4f/OSIRT/Browser/MainBrowser.cs#L300
//				await m_browser.GetBrowser().MainFrame.EvaluateScriptAsync("(function() { document.documentElement.style.overflow = 'hidden'; })();");
//				await ExecuteTaskOrTimeOut( m_browser.GetBrowser().MainFrame.EvaluateScriptAsync( "(function() { document.documentElement.style.overflow = 'hidden'; })();" ), m_TimeOut_ms_JavascriptNoRender );
				await ExecuteJS( "(function() { document.documentElement.style.overflow = 'hidden'; })();" );

				uint	viewportHeight = (uint) m_browser.Size.Height;
				for ( uint scrollIndex=0; scrollIndex < _scrollsCount; scrollIndex++ ) {

					try {
						//////////////////////////////////////////////////////////////////////////
						/// Request a screenshot
System.Diagnostics.Debug.WriteLine( "DoScreenshots() => Requesting screenshot {0}", scrollIndex );

// 						Task<System.Drawing.Bitmap>	task = m_browser.ScreenshotAsync();
// 						if ( (await Task.WhenAny( task, Task.Delay( m_TimeOut_ms_PageRender ) )) == task ) {

 						Task<System.Drawing.Bitmap>	task = (await ExecuteTaskOrTimeOut( m_browser.ScreenshotAsync(), m_TimeOut_ms_Screenshot )) as Task<System.Drawing.Bitmap>;

System.Diagnostics.Debug.WriteLine( "DoScreenshots() => Retrieved web page image screenshot {0} / {1}", 1+scrollIndex, _scrollsCount );

						try {
							ImageUtility.ImageFile image = new ImageUtility.ImageFile( task.Result, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
							m_pageRendered( scrollIndex, image );
						} catch ( Exception _e ) {
							throw new Exception( "Failed to create image from web page bitmap: \r\n" + _e.Message, _e );
						} finally {
							task.Result.Dispose();	// Always dispose of the bitmap anyway!
						}

						//////////////////////////////////////////////////////////////////////////
						/// Scroll down the page
						if ( scrollIndex < _scrollsCount-1 ) {
System.Diagnostics.Debug.WriteLine( "DoScreenshots() => Requesting scrolling... (should retrigger rendering)" );

							// Mark the page as "unstable" and scroll down until we reach the bottom (if it exists, or until we reach the specified maximum amount of authorized screenshots)
							RestartTimer();
//							await m_browser.GetBrowser().MainFrame.EvaluateScriptAsync("(function() { window.scroll(0," + ((scrollIndex+1) * viewportHeight) + "); })();");
							await ExecuteJS( "(function() { window.scroll(0," + ((scrollIndex+1) * viewportHeight) + "); })();" );

							// Wait for the page to stabilize (i.e. the timer hasn't been reset for some time, indicating most elements should be ready)
							await WaitForStablePage();

System.Diagnostics.Debug.WriteLine( "DoScreenshots() => Scrolling done!" );
						}

					} catch ( TimeoutException ) {
System.Diagnostics.Debug.WriteLine( "DoScreenshots() => TIMEOUT!" );
//						throw new Exception( "Page rendering timed out" );
//m_pageError()
					} catch ( Exception _e ) {
System.Diagnostics.Debug.WriteLine( "DoScreenshots() => EXCEPTION! " + _e.Message );
					}
				}

				// Notify the page was successfully loaded
				m_pageSuccess();

// Code from https://github.com/WildGenie/OSIRTv2/blob/3e60d3ce908a1d25a7b4633dc9afdd53256cbb4f/OSIRT/Browser/MainBrowser.cs#L300
// 				int count = 0;
// 				int pageLeft = scrollHeight;
// 				bool atBottom = false;
// //Debug.WriteLine($"OUTSIDE --- PAGE LEFT: {pageLeft}. VIEWPORT HEIGHT: {viewportHeight}");
// //				ImageDiskCache cache = new ImageDiskCache();
// 
// 				while (!atBottom)
// 				{
// 					if (pageLeft > viewportHeight)
// 					{
// 						//if we can scroll using the viewport, let's do that
// 						await m_browser.GetBrowser().MainFrame.EvaluateScriptAsync("(function() { window.scroll(0," + (count * viewportHeight) + "); })();");
// 						count++;
// 						await PutTaskDelay();  //we do need these delays. Some pages, like facebook, may need to load viewport content.
// 						using (Bitmap image = GetCurrentViewScreenshot())
// 						{
// 							cache.AddImage(count, image);
// 						}
// 
// // 						if (!OsirtHelper.IsOnGoogle(URL))
// // 							await m_browser.GetBrowser().MainFrame.EvaluateScriptAsync("(function() { var elements = document.querySelectorAll('*'); for (var i = 0; i < elements.length; i++) { var position = window.getComputedStyle(elements[i]).position; if (position === 'fixed') { elements[i].style.visibility = 'hidden'; } } })(); ");
// 					}
// 					else 
// 					{
// 						//find out what's left of the page to scroll, then take screenshot
// 						//if it's the last image, we're going to need to crop what we need, as it'll take
// 						//a capture of the entire viewport.
// 
//            
// 					   await GetBrowser().MainFrame.EvaluateScriptAsync("(function() { window.scrollBy(0," + pageLeft + "); })();");
// 
// 						atBottom = true;
// 						count++;
// 
// 						await PutTaskDelay();
// 						Rectangle cropRect = new Rectangle(new Point(0, viewportHeight - pageLeft), new Size(viewportWidth, pageLeft));
// 
// 						using (Bitmap src = GetCurrentViewScreenshot())
// 						using (Bitmap target = new Bitmap(cropRect.Width, cropRect.Height))
// 						using (Graphics g = Graphics.FromImage(target))
// 						{
// 							g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
// 							cache.AddImage(count, target);
// 						}
//                   
// 					}
// 
// 					pageLeft = pageLeft - viewportHeight;
// 					Debug.WriteLine($"IN WHILE --- PAGE LEFT: {pageLeft}. VIEWPORT HEIGHT: {viewportHeight}");
// 				}//end while
// 
// 		public async Task PutTaskDelay()
//         {
//             await Task.Delay(MaxWait);
//         }
#elif true

				// Handle success or timeout (code from https://stackoverflow.com/questions/4238345/asynchronously-wait-for-taskt-to-complete-with-timeout)
//				Task<System.Drawing.Bitmap>	task = m_browser.ScreenshotAsync( true, PopupBlending.Main );
				Task<System.Drawing.Bitmap>	task = m_browser.ScreenshotAsync();
				if ( (await Task.WhenAny( task, Task.Delay( m_TimeOut_ms_PageRender ) )) == task ) {
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
