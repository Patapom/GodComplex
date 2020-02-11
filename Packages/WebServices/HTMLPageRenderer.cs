using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using CefSharp;
using CefSharp.OffScreen;

namespace WebServices {

// TODO:
//	• Use schemes to handle local files: https://github.com/cefsharp/CefSharp/wiki/General-Usage#scheme-handler
//	• Example: Capture Full Page Using Scrolling https://github.com/WildGenie/OSIRTv2/blob/3e60d3ce908a1d25a7b4633dc9afdd53256cbb4f/OSIRT/Browser/MainBrowser.cs#L300
//	• Download element rectangles! => Use JS

	/// <summary>
	/// Class wrapping CEF Sharp (Chromium Embedded Framework, .Net wrapper version) to render web pages in an offscreen bitmap
	/// https://github.com/cefsharp/CefSharp/wiki/General-Usage
	/// </summary>
	public class HTMLPageRenderer : IDisposable {

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
		/// <param name="_pageTitle">HTML Document's title</param>
		/// <param name="_HTMLContent">HTML source</param>
		/// <param name="_DOMElements">The XML document describing the DOM elements present in the returned web page</param>
		public delegate void	WebPageSourceAvailable( string _pageTitle, string _HTMLContent, XmlDocument _DOMElements );

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

		public enum LOG_TYPE {
			INFO,
			WARNING,
			ERROR,
			DEBUG,
		}

		public delegate void	LogDelegate( LOG_TYPE _type, string _text );

		#endregion

		#region FIELDS

		private int					m_time_ms_StablePage = 5000;			// Page is deemed stable if no event have been received for 5s
		private int					m_time_ms_ScrollDownPrepare = 250;		// Wait for 250ms before taking a screenshot after a page scroll on ROUND 1
		private int					m_time_ms_ScrollDown = 100;				// Wait for 100ms before taking a screenshot after a page scroll on ROUND 2

		private int					m_timeOut_ms_JavascriptNoRender = 1000;	// Default timeout after 1s of a JS command that doesn't trigger a new rendering
		private int					m_timeOut_ms_PageRender = 30000;		// Default timeout after 30s for a page rendering
		private int					m_timeOut_ms_Screenshot = 10000;		// Default timeout after 1s for a screenshot

		private ChromiumWebBrowser	m_browser = null;

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

		private LogDelegate				m_logDelegate;

		#endregion

		#region METHODS

		public HTMLPageRenderer( string _URL, int _browserViewportWidth, int _browserViewportHeight, int _maxScreenshotsCount, WebPageSourceAvailable _pageSourceAvailable, WebPageRendered _pageRendered, WebPageSuccess _pageSuccess, WebPageErrorOccurred _pageError, LogDelegate _logDelegate ) {

//Main( null );

			m_URL = _URL;
			m_maxScreenshotsCount = _maxScreenshotsCount;
			m_pageSourceAvailable = _pageSourceAvailable;
			m_pageRendered = _pageRendered;
			m_pageSuccess = _pageSuccess;
			m_pageError = _pageError;
			m_logDelegate = _logDelegate != null ? _logDelegate : DefaultLogger;

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

 			if ( _browserViewportHeight == 0 )
 				_browserViewportHeight = (int) (_browserViewportWidth * 1.6180339887498948482045868343656);

			m_browser.Size = new System.Drawing.Size( _browserViewportWidth, _browserViewportHeight );

			m_browser.BrowserInitialized += browser_BrowserInitialized;
		}

		private void browser_BrowserInitialized(object sender, EventArgs e) {
Log( LOG_TYPE.DEBUG, "browser_BrowserInitialized" );

			// No event have been registered yet
			m_hasPageEvents = false;

			// Start actual page loading
			m_browser.Load( m_URL );

			// Execute waiting task
//			var	T = ExecuteTaskOrTimeOut( WaitForPageRendered(), m_timeOut_ms_PageRender );
			Task	T = new Task( WaitForPageRendered );
					T.Start();
		}

		private void browser_LoadError(object sender, LoadErrorEventArgs e) {
Log( LOG_TYPE.DEBUG, "browser_LoadError: " + e.ErrorText );

			switch ( (uint) e.ErrorCode ) {
				case 0xffffffe5U:
					return;	// Ignore...
			}

			m_pageError( (int) e.ErrorCode, e.ErrorText );
		}

		private void browser_FrameLoadStart(object sender, FrameLoadStartEventArgs e) {
			RegisterPageEvent( "browser_FrameLoadStart" );
		}
		
		private void browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e) {
			RegisterPageEvent( "browser_FrameLoadEnd" );
		}

		private void browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e) {
// 			if ( e.IsLoading )
// 				return;	// Still loading...

			RegisterPageEvent( "browser_LoadingStateChanged (" + (e.IsLoading ? "loading" : "finished") + ")" );
		}

		bool		m_hasPageEvents = false;
 		DateTime	m_lastPageEvent;
		void	RegisterPageEvent( string _eventType ) {
			m_lastPageEvent = DateTime.Now;
			m_hasPageEvents = true;
Log( LOG_TYPE.DEBUG, _eventType );
		}

		/// <summary>
		/// Asynchronous task that will wait for the page to be stable (i.e. all elements have been loaded for some time) before accessing the DOM and taking screenshots
		/// </summary>
		async void	WaitForPageRendered() {
			// Wait until the page is stable a first time...
			await WaitForStablePage( "WaitForPageRendered() => Wait before querying content" );

			// First query the HTML source code and DOM content
			await QueryContent();

			// Ask for the page's height (not always reliable, especially on infinite scrolling feeds like facebook or twitter!)
			JavascriptResponse	JSResult = await ExecuteJS( "(function() { var body = document.body, html = document.documentElement; return Math.max( body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight ); } )();" );
			if ( JSResult.Result == null ) {
Log( LOG_TYPE.DEBUG, "JS scrollHeight returned null => 2nd call" );
				JSResult = await ExecuteJS( "(function() { var body = document.body; return Math.max( body.scrollHeight, body.offsetHeight ); } )();" );
				if ( JSResult.Result == null ) {
Log( LOG_TYPE.DEBUG, "JS scrollHeight returned null => 3rd call" );
					JSResult = await ExecuteJS( "(function() { var html = document.documentElement; return Math.max( html.clientHeight, html.scrollHeight, html.offsetHeight ); } )();" );
					if ( JSResult.Result == null ) {
Log( LOG_TYPE.ERROR, "JS scrollHeight returned null => Exception!" );
						throw new Exception( "None of the 3 attempts at querying page height was successful!" );
					}
				}
			}
			int	scrollHeight = (int) JSResult.Result;

			// Perform as many screenshots as necessary to capture the entire page
			int	viewportHeight = m_browser.Size.Height;
			int	screenshotsCount = (int) Math.Ceiling( (double) scrollHeight / viewportHeight );
Log( LOG_TYPE.DEBUG, "Page scroll height = " + scrollHeight + " - Screenshots Count = " + screenshotsCount );

			await DoScreenshots( screenshotsCount, scrollHeight );
		}

		/// <summary>
		/// Reads back HTML content and do a screenshot
		/// </summary>
		/// <returns></returns>
		async Task	QueryContent() {
			try {
Log( LOG_TYPE.DEBUG, "QueryContent for " + m_URL );

				// From Line 162 https://github.com/WildGenie/OSIRTv2/blob/3e60d3ce908a1d25a7b4633dc9afdd53256cbb4f/OSIRT/Browser/MainBrowser.cs#L300
				string	source = await m_browser.GetBrowser().MainFrame.GetSourceAsync();
				if ( source == null )
					throw new Exception( "Failed to retrieve HTML source!" );

Log( LOG_TYPE.DEBUG, "QueryContent() => Retrieved HTML code " + (source.Length < 100 ? source : source.Remove( 100 )) );

				JavascriptResponse	JSResult = await ExecuteJS( "(function() { return document.title; } )();" );
				string	pageTitle = JSResult.Result as string;

// @TODO: Parse DOM!
Log( LOG_TYPE.WARNING, "QueryContent() => @TODO: Parse DOM!" );

				// Notify source is ready
				m_pageSourceAvailable( pageTitle, source, null );

			} catch ( Exception _e ) {
				m_pageError( -1, "An error occurred while attempting to retrieve HTML source for URL \"" + m_URL + "\": \r\n" + _e.Message );
			}
		}

		string	JSCodeScroll( uint _Y ) {
			return "(function() { window.scrollTo( 0," +_Y + " ); })();";
		}

		/// <summary>
		/// Do multiple screenshots to capture the entire page
		/// </summary>
		/// <returns></returns>
		async Task	DoScreenshots( int _scrollsCount, int _scrollHeight ) {
			_scrollsCount = Math.Min( m_maxScreenshotsCount, _scrollsCount );

			int	viewportHeight = m_browser.Size.Height;

			try {
				// Code from https://github.com/WildGenie/OSIRTv2/blob/3e60d3ce908a1d25a7b4633dc9afdd53256cbb4f/OSIRT/Browser/MainBrowser.cs#L300
//				await m_browser.GetBrowser().MainFrame.EvaluateScriptAsync("(function() { document.documentElement.style.overflow = 'hidden'; })();");
//				await ExecuteTaskOrTimeOut( m_browser.GetBrowser().MainFrame.EvaluateScriptAsync( "(function() { document.documentElement.style.overflow = 'hidden'; })();" ), m_timeOut_ms_JavascriptNoRender );

//				await ExecuteJS( "(function() { document.documentElement.style.overflow = 'hidden'; })();" );

				//////////////////////////////////////////////////////////////////////////
				/// Execute a first rounde to "prep the page"
				/// 
				if ( _scrollsCount > 1 ) {
Log( LOG_TYPE.DEBUG, "DoScreenshots() => (ROUND 1) Requesting {0} scrollings", _scrollsCount );
					for ( uint scrollIndex=0; scrollIndex < _scrollsCount; scrollIndex++ ) {

						try {
							/// Request a screenshot
// Log( LOG_TYPE.DEBUG, "DoScreenshots() => (ROUND 1) Requesting screenshot {0}", scrollIndex );

 							Task<System.Drawing.Bitmap>	task = (await ExecuteTaskOrTimeOut( m_browser.ScreenshotAsync( false ), m_timeOut_ms_Screenshot, "m_browser.ScreenshotAsync()" )) as Task<System.Drawing.Bitmap>;
							task.Dispose();

// Log( LOG_TYPE.DEBUG, "DoScreenshots() => Retrieved web page image screenshot {0} / {1}", 1+scrollIndex, _scrollsCount );

							//////////////////////////////////////////////////////////////////////////
							/// Scroll down the page
							if ( scrollIndex < _scrollsCount-1 ) {
// Log( LOG_TYPE.DEBUG, "DoScreenshots() => (ROUND 1) Requesting scrolling..." );
								await ExecuteJS( JSCodeScroll( (uint) ((scrollIndex+1) * viewportHeight) ), m_time_ms_ScrollDownPrepare );
							}

						} catch ( TimeoutException _e ) {
// Log( LOG_TYPE.ERROR, "DoScreenshots() => (ROUND 1) TIMEOUT EXCEPTION! " + _e.Message );
//							throw new Exception( "Page rendering timed out" );
//m_pageError()
						} catch ( Exception _e ) {
// Log( LOG_TYPE.ERROR, "DoScreenshots() => (ROUND 1) EXCEPTION! " + _e.Message );
						}
					}

					// Do another screenshot
 					await ExecuteTaskOrTimeOut( m_browser.ScreenshotAsync( false ), m_timeOut_ms_Screenshot, "m_browser.ScreenshotAsync()" );

					// Scroll all the way back up
					await ExecuteJS( JSCodeScroll( 0 ), m_time_ms_ScrollDownPrepare );
				}

				//////////////////////////////////////////////////////////////////////////
				/// Do a 2nd batch where we actually store the screenshots!
				///
				for ( int scrollIndex=0; scrollIndex < _scrollsCount; scrollIndex++ ) {

					try {
						//////////////////////////////////////////////////////////////////////////
						/// Request a screenshot
Log( LOG_TYPE.DEBUG, "DoScreenshots() => (ROUND 2) Requesting screenshot {0}", scrollIndex );

// 						Task<System.Drawing.Bitmap>	task = m_browser.ScreenshotAsync();
// 						if ( (await Task.WhenAny( task, Task.Delay( m_timeOut_ms_PageRender ) )) == task ) {

 						Task<System.Drawing.Bitmap>	task = (await ExecuteTaskOrTimeOut( m_browser.ScreenshotAsync( false ), m_timeOut_ms_Screenshot, "m_browser.ScreenshotAsync()" )) as Task<System.Drawing.Bitmap>;

Log( LOG_TYPE.DEBUG, "DoScreenshots() => (ROUND 2) Retrieved web page image screenshot {0} / {1}", 1+scrollIndex, _scrollsCount );

						System.Drawing.Bitmap	bitmap = task.Result;
						int	remainingHeight = _scrollHeight - scrollIndex * viewportHeight;
						if ( remainingHeight < bitmap.Height ) {
							// Clip bitmap and keep only the last relevant lines
							if ( remainingHeight < 16 ) {
								bitmap = null;	// Ignore super small slices
							} else {
								using ( System.Drawing.Bitmap oldBitmap = bitmap ) {
									bitmap = new System.Drawing.Bitmap( oldBitmap.Width, (int) remainingHeight, oldBitmap.PixelFormat );
									using ( System.Drawing.Graphics G = System.Drawing.Graphics.FromImage( bitmap ) ) {
										G.DrawImage( oldBitmap, 0, remainingHeight - oldBitmap.Height );
									}
								}
							}
						}

						if ( bitmap != null ) {
							try {
								ImageUtility.ImageFile image = new ImageUtility.ImageFile( bitmap, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
								m_pageRendered( (uint) scrollIndex, image );
							} catch ( Exception _e ) {
								throw new Exception( "Failed to create image from web page bitmap: \r\n" + _e.Message, _e );
							} finally {
								bitmap.Dispose();	// Always dispose of the bitmap anyway!
							}
						}

						//////////////////////////////////////////////////////////////////////////
						/// Scroll down the page
						if ( scrollIndex < _scrollsCount-1 ) {
Log( LOG_TYPE.DEBUG, "DoScreenshots() => (ROUND 2) Requesting scrolling to position {0}...", ((scrollIndex+1) * viewportHeight) );
							await ExecuteJS( JSCodeScroll( (uint) ((scrollIndex+1) * viewportHeight) ), m_time_ms_ScrollDown );
						}

					} catch ( TimeoutException _e ) {
Log( LOG_TYPE.ERROR, "DoScreenshots() => (ROUND 2) TIMEOUT EXCEPTION! " + _e.Message );
//						throw new Exception( "Page rendering timed out" );
//m_pageError()
					} catch ( Exception _e ) {
Log( LOG_TYPE.ERROR, "DoScreenshots() => (ROUND 2) EXCEPTION! " + _e.Message );
					}
				}

				// Notify the page was successfully loaded
				m_pageSuccess();

			} catch ( Exception _e ) {
				m_pageError( -1, "An error occurred while attempting to render a page screenshot for URL \"" + m_URL + "\": \r\n" + _e.Message );
			}
		}

		/// <summary>
		/// Executes a task for a given amount of time before it times out
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_task"></param>
		/// <param name="_timeOut_ms"></param>
		/// <returns></returns>
		async Task<Task>	ExecuteTaskOrTimeOut< T >( T _task, int _timeOut_ms, string _timeOutMessage ) where T : Task {
			if ( (await Task.WhenAny( _task, Task.Delay( _timeOut_ms ) )) != _task ) {
//				_task.Dispose();
				throw new TimeoutException( _timeOutMessage );
			}

			return _task;
		}

		async Task<JavascriptResponse>	ExecuteJS( string _JS ) {
			return await ExecuteJS( _JS, 0 );
		}
		async Task<JavascriptResponse>	ExecuteJS( string _JS, int _delay_ms ) {
			Task<JavascriptResponse>	task = (await ExecuteTaskOrTimeOut( m_browser.GetBrowser().MainFrame.EvaluateScriptAsync( _JS, null ), m_timeOut_ms_JavascriptNoRender, "EvaluateScriptAsync " + _JS )) as Task<JavascriptResponse>;

			if ( _delay_ms > 0 ) {
				await Task.Delay( _delay_ms );
			}

			return task.Result;
		}

		async Task	AsyncWaitForStablePage( string _waiter ) {
			const int	MAX_COUNTER = 100;

			int	counter = 0;
			while ( counter < MAX_COUNTER ) {
Log( LOG_TYPE.DEBUG, "AsyncWaitForStablePage( {0} ) => Waiting {1}", _waiter, counter++ );

				double	elapsedTimeSinceLastPageEvent = m_hasPageEvents ? (DateTime.Now - m_lastPageEvent).TotalMilliseconds : 0;
				if ( elapsedTimeSinceLastPageEvent > m_time_ms_StablePage )
					return;	// Page seems stable enough...

//				Application.DoEvents();
				await Task.Delay( 250 );  // We do need these delays. Some pages, like facebook, may need to load viewport content.
			}

Log( LOG_TYPE.DEBUG, "AsyncWaitForStablePage( {0} ) => Exiting after {1} loops!", _waiter, counter );
		}

		async Task	WaitForStablePage( string _waiter ) {
			await ExecuteTaskOrTimeOut( AsyncWaitForStablePage( _waiter ), m_timeOut_ms_PageRender, "AsyncWaitForStablePage()" );
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

		public void	Log( LOG_TYPE _type, string _text, params object[] _arguments ) {
			_text = string.Format( _text, _arguments );
			m_logDelegate( _type, _text );
		}

		public void	DefaultLogger( LOG_TYPE _type, string _text ) {
			switch ( _type ) {
				case LOG_TYPE.WARNING:	_text = "<WARNING> " + _text; break;
				case LOG_TYPE.ERROR:	_text = "<ERROR> " + _text; break;
				case LOG_TYPE.DEBUG:	_text = "<DEBUG> " + _text; break;
			}

System.Diagnostics.Debug.WriteLine( _text );
		}

		#endregion
	}
}
