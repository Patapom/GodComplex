using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Drawing;
using SharpMath;

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
		/// <param name="_contentRectangle">The rectangle where the image should be placed to represent its original web page location</param>
		/// <param name="_imageWebPage">The piece of rendering of the web page</param>
		public delegate void	WebPageRendered( uint _imageIndex, Rectangle _contentRectangle, ImageUtility.ImageFile _imageWebPage );

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

		private int					m_delay_ms_StablePage = 5000;			// Page is deemed stable if no event have been received for over 5s
		private int					m_delay_ms_ScrollDown = 250;			// Wait for 250ms before taking a screenshot after a page scroll on ROUND 2
		private int					m_delay_ms_CleanDOM = 1000;				// Wait for 1000ms after cleaning DOM

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
			m_browser.BrowserInitialized += browser_BrowserInitialized;
			m_browser.LifeSpanHandler = new LifeSpanHandler();

			// https://github.com/cefsharp/CefSharp/wiki/General-Usage#handlers
			m_browser.LoadError += browser_LoadError;
			m_browser.LoadingStateChanged += browser_LoadingStateChanged;
			m_browser.FrameLoadStart += browser_FrameLoadStart;
			m_browser.FrameLoadEnd += browser_FrameLoadEnd;

 			if ( _browserViewportHeight == 0 )
 				_browserViewportHeight = (int) (_browserViewportWidth * 1.6180339887498948482045868343656);

			m_browser.Size = new Size( _browserViewportWidth, _browserViewportHeight );
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
Log( LOG_TYPE.DEBUG, "browser_LoadError: " + e.ErrorText + " on URL " + e.FailedUrl );

			if ( !m_URL.ToLower().StartsWith( e.FailedUrl.ToLower() ) )
				return;	// Not our URL! Not our concern...

// 			switch ( e.ErrorCode ) {
// 				case CefErrorCode.Aborted:
// 					break;
// 			}
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

			//////////////////////////////////////////////////////////////////////////
			// Include general JS helpers
			//
			await ExecuteJS( Properties.Resources.Helpers );

			//////////////////////////////////////////////////////////////////////////
			// Ask for the page's height (not always reliable, especially on infinite scrolling feeds like facebook or twitter!)
			//
			JavascriptResponse	JSResult = await ExecuteJS( "GetPageHeight();" );
			int	scrollHeight = (int) JSResult.Result;

			// Ask for background color
			Color	backgroundColor = Color.White;
			JSResult = await ExecuteJS( "(function() { return window.getComputedStyle( document.body ).backgroundColor.toString(); })();" );
			if ( JSResult.Success ) {
//				backgroundColor = 
			}

			//////////////////////////////////////////////////////////////////////////
			// Perform as many screenshots as necessary to capture the entire page
			//
			int	viewportHeight = m_browser.Size.Height;
			int	screenshotsCount = (int) Math.Ceiling( (double) scrollHeight / viewportHeight );
Log( LOG_TYPE.DEBUG, "Page scroll height = " + scrollHeight + " - Screenshots Count = " + screenshotsCount );

			using ( Brush backgroundBrush = new SolidBrush( backgroundColor ) )
				await DoScreenshots( screenshotsCount, scrollHeight, backgroundBrush );

			//////////////////////////////////////////////////////////////////////////
			// Query the HTML source code and DOM content
			//
			await QueryContent( backgroundColor );
		}

		/// <summary>
		/// Do multiple screenshots to capture the entire page
		/// </summary>
		/// <returns></returns>
		async Task	DoScreenshots( int _scrollsCount, int _scrollHeight, Brush _backgroundBrush ) {
			_scrollsCount = Math.Min( m_maxScreenshotsCount, _scrollsCount );

			int			viewportWidth = m_browser.Size.Width;
			int			viewportHeight = m_browser.Size.Height;
			Rectangle	defaultTotalContentRectangle = new Rectangle( 0, 0, viewportWidth, _scrollHeight );	// Large rectangle covering several times the browser's rectangle

			try {
				// Code from https://github.com/WildGenie/OSIRTv2/blob/3e60d3ce908a1d25a7b4633dc9afdd53256cbb4f/OSIRT/Browser/MainBrowser.cs#L300
//				await m_browser.GetBrowser().MainFrame.EvaluateScriptAsync("(function() { document.documentElement.style.overflow = 'hidden'; })();");
//				await ExecuteTaskOrTimeOut( m_browser.GetBrowser().MainFrame.EvaluateScriptAsync( "(function() { document.documentElement.style.overflow = 'hidden'; })();" ), m_timeOut_ms_JavascriptNoRender );

//				await ExecuteJS( "(function() { document.documentElement.style.overflow = 'hidden'; })();" );

				// Retrieve initial scroll value
				int					initialScrollY = 0;
				JavascriptResponse	JSResult = await ExecuteJS( "window.scrollY;" );
				if ( JSResult.Success ) {
					initialScrollY = (int) JSResult.Result;
	 			}


#if false
				//////////////////////////////////////////////////////////////////////////
				/// Execute a first round to "prep the page"
				/// 
				if ( _scrollsCount > 1 ) {
Log( LOG_TYPE.DEBUG, "DoScreenshots() => (ROUND 1) Requesting {0} scrollings", _scrollsCount );
					for ( uint scrollIndex=0; scrollIndex < _scrollsCount; scrollIndex++ ) {

						try {
							/// Request a screenshot
// Log( LOG_TYPE.DEBUG, "DoScreenshots() => (ROUND 1) Requesting screenshot {0}", scrollIndex );

 							Task<Bitmap>	task = (await ExecuteTaskOrTimeOut( m_browser.ScreenshotAsync( false ), m_timeOut_ms_Screenshot, "m_browser.ScreenshotAsync()" )) as Task<Bitmap>;
							task.Dispose();

// Log( LOG_TYPE.DEBUG, "DoScreenshots() => Retrieved web page image screenshot {0} / {1}", 1+scrollIndex, _scrollsCount );

							//////////////////////////////////////////////////////////////////////////
							/// Scroll down the page
							if ( scrollIndex < _scrollsCount-1 ) {
// Log( LOG_TYPE.DEBUG, "DoScreenshots() => (ROUND 1) Requesting scrolling..." );
								await ExecuteJS( JSCodeScroll( (uint) (initialScrollY + (scrollIndex+1) * viewportHeight) ), m_delay_ms_ScrollDown );
							}

						} catch ( TimeoutException _e ) {
Log( LOG_TYPE.ERROR, "DoScreenshots() => (ROUND 1) TIMEOUT EXCEPTION! " + _e.Message );
//							throw new Exception( "Page rendering timed out" );
//m_pageError()
						} catch ( Exception _e ) {
Log( LOG_TYPE.ERROR, "DoScreenshots() => (ROUND 1) EXCEPTION! " + _e.Message );
						}
					}

					// Do another screenshot
 					await ExecuteTaskOrTimeOut( m_browser.ScreenshotAsync( false ), m_timeOut_ms_Screenshot, "m_browser.ScreenshotAsync()" );

					// Scroll all the way back up and wait longer
					await ExecuteJS( JSCodeScroll( initialScrollY ), 4 * m_delay_ms_ScrollDown );
				}
#endif
				//////////////////////////////////////////////////////////////////////////
				/// Do a 2nd batch where we actually store the screenshots!
				///
				for ( int scrollIndex=0; scrollIndex < _scrollsCount; scrollIndex++ ) {

					try {
						//////////////////////////////////////////////////////////////////////////
						/// Clean the DOM and retrieve workable area
						/// 
						RectangleF	contentRectangleF = await CleanDOMAndReturnMainContentRectangle();
						Rectangle	contentRectangle = new Rectangle(	(int) Mathf.Floor( contentRectangleF.X ), (int) Mathf.Floor( contentRectangleF.Y ),
																		(int) (1 + Mathf.Ceiling( contentRectangleF.Right ) - Mathf.Floor( contentRectangleF.X ) ),
																		(int) (1 + Mathf.Ceiling( contentRectangleF.Bottom ) - Mathf.Floor( contentRectangleF.Y ) )
																	);
						if ( contentRectangle.IsEmpty ) {
							// Use default rectangle covering the entire screen...
							contentRectangle = defaultTotalContentRectangle;
							contentRectangle.Offset( 0, -(initialScrollY + scrollIndex * viewportHeight) );
						}

Log( LOG_TYPE.DEBUG, "DoScreenshots() => (ROUND 2) Cleaning DOM and getting viewport ({0}, {1}, {2}, {3})", contentRectangle.X, contentRectangle.Y, contentRectangle.Width, contentRectangle.Height );


						//////////////////////////////////////////////////////////////////////////
						/// Request a screenshot
 						Task<Bitmap>	taskScreenshot = (await ExecuteTaskOrTimeOut( m_browser.ScreenshotAsync( false ), m_timeOut_ms_Screenshot, "m_browser.ScreenshotAsync()" )) as Task<Bitmap>;

Log( LOG_TYPE.DEBUG, "DoScreenshots() => (ROUND 2) Retrieved web page image screenshot {0} / {1}", 1+scrollIndex, _scrollsCount );


						//////////////////////////////////////////////////////////////////////////
						/// Clip bitmap with intersection of content and viewport rectangles
						/// 
 						int			remainingHeight = _scrollHeight - (initialScrollY + scrollIndex * viewportHeight);
									remainingHeight = Math.Min( viewportHeight, remainingHeight );

						Rectangle	viewportRectangle = new Rectangle( 0, viewportHeight - remainingHeight, viewportWidth, remainingHeight );
						Rectangle	clippedContentRectangle = contentRectangle;
									clippedContentRectangle.Intersect( viewportRectangle );

						Bitmap		bitmap = null;
						if ( clippedContentRectangle.Width == viewportWidth && clippedContentRectangle.Height == viewportHeight ) {
							// No need to clip anything... Use full bitmap!
							bitmap = taskScreenshot.Result;
						} else if ( clippedContentRectangle.Width > 16 && clippedContentRectangle.Height > 16 ) {
							// Clip to sub-rectangle
							using ( Bitmap oldBitmap = taskScreenshot.Result ) {
//								bitmap = new Bitmap( (int) clippedContentRectangle.Width, (int) clippedContentRectangle.Height, oldBitmap.PixelFormat );
								bitmap = new Bitmap( (int) clippedContentRectangle.Width, (int) clippedContentRectangle.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb );
								using ( Graphics G = Graphics.FromImage( bitmap ) ) {
									if ( oldBitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb ) {
										G.FillRectangle( _backgroundBrush, 0, 0, bitmap.Width, bitmap.Height );
									}
									G.DrawImage( oldBitmap, -clippedContentRectangle.X, -clippedContentRectangle.Y );
								}
							}
						}

						//////////////////////////////////////////////////////////////////////////
						/// Forward bitmap to caller
						///
						if ( bitmap != null ) {
							try {
								ImageUtility.ImageFile image = new ImageUtility.ImageFile( bitmap, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );

								// We need the absolute content rectangle, in other words the location of this image chunk as it would be in the web page from the topmost and left most location...
								Rectangle	absoluteContentRectangle = clippedContentRectangle;
											absoluteContentRectangle.Offset( 0, scrollIndex * viewportHeight - viewportRectangle.Y );

								m_pageRendered( (uint) scrollIndex, absoluteContentRectangle, image );

							} catch ( Exception _e ) {
								throw new Exception( "Failed to create image from web page bitmap: \r\n" + _e.Message, _e );
							} finally {
								bitmap.Dispose();	// Always dispose of the bitmap anyway!
							}
						}

						//////////////////////////////////////////////////////////////////////////
						/// Scroll down the page
						if ( scrollIndex < _scrollsCount-1 ) {
							int	scrollPosition = Math.Min( _scrollHeight, initialScrollY + (scrollIndex+1) * viewportHeight );
Log( LOG_TYPE.DEBUG, "DoScreenshots() => (ROUND 2) Requesting scrolling to position {0}...", scrollPosition );
							await ExecuteJS( JSCodeScroll( (uint) scrollPosition ), m_delay_ms_ScrollDown );
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
		/// Reads back HTML content and do a screenshot
		/// </summary>
		/// <returns></returns>
		async Task	QueryContent( Color _backgroundColor ) {
			try {
Log( LOG_TYPE.DEBUG, "QueryContent for " + m_URL );

				// From Line 162 https://github.com/WildGenie/OSIRTv2/blob/3e60d3ce908a1d25a7b4633dc9afdd53256cbb4f/OSIRT/Browser/MainBrowser.cs#L300
				string	source = await m_browser.GetBrowser().MainFrame.GetSourceAsync();
				if ( source == null )
					throw new Exception( "Failed to retrieve HTML source!" );

Log( LOG_TYPE.DEBUG, "QueryContent() => Retrieved HTML code " + (source.Length < 100 ? source : source.Remove( 100 )) );

				JavascriptResponse	JSResult = await ExecuteJS( "(function() { return document.title; } )();" );
				string	pageTitle = JSResult.Result as string;

				// Retrive DOM elements
				XmlDocument	DOMContent = await RetrieveDOMContent();

Log( LOG_TYPE.DEBUG, "QueryContent() => Retrieved {0} content elements from DOM", DOMContent["root"].ChildNodes.Count );

				// Notify source is ready
				m_pageSourceAvailable( pageTitle, source, DOMContent );

			} catch ( Exception _e ) {
				m_pageError( -1, "An error occurred while attempting to retrieve HTML source and DOM content elements for URL \"" + m_URL + "\": \r\n" + _e.Message );
			}
		}

		/// <summary>
		/// Lists all the content elements in the DOM (images, links and text) 
		/// </summary>
		/// <returns></returns>
		async Task<XmlDocument>	RetrieveDOMContent() {
			JavascriptResponse	JSResult = await ExecuteJS( Properties.Resources.RetrieveDOMElements );
			if ( !JSResult.Success )
				throw new Exception( "Failed to execute retrieval of DOM content elements: " + JSResult.Message );

			XmlDocument	result = null;
			try {
				JSON			parser = new JSON();
				JSON.JSONObject	root = null;
				using ( System.IO.StringReader R = new System.IO.StringReader( JSResult.Result as string ) ) {
					root = parser.ReadJSON( R )["root"];

					if ( !root.IsArray )
						throw new Exception( "Expected an array of element descriptors. Found " + root.m_object );

					result = new XmlDocument();
					XmlElement	xmlRoot = result.CreateElement( "root" );
					result.AppendChild( xmlRoot );

					foreach ( JSON.JSONObject element in root.AsArray ) {
						XmlElement	xmlElement = result.CreateElement( "element" );
						xmlRoot.AppendChild( xmlElement );

						int		elementType = ((int) element["type"].AsDouble);
						string	elementTypeName = "UNKNOWN";
						switch ( elementType ) {
							case 1: elementTypeName = "LINK"; break;
							case 2: elementTypeName = "IMAGE"; break;
							case 3: elementTypeName = "TEXT"; break;
						}

						xmlElement.SetAttribute( "path", element["path"].AsString );
						xmlElement.SetAttribute( "type", elementTypeName );
						xmlElement.SetAttribute( "x", element["x"].AsDouble.ToString() );
						xmlElement.SetAttribute( "y", element["y"].AsDouble.ToString() );
						xmlElement.SetAttribute( "w", element["w"].AsDouble.ToString() );
						xmlElement.SetAttribute( "h", element["h"].AsDouble.ToString() );
						if ( elementType == 1 ) {
							xmlElement.SetAttribute( "URL", element["URL"].AsString );
						}
					}
				}
			} catch ( Exception _e ) {
				throw new Exception( "Failed to build XML Document from JSON object of DOM content elements: " + _e.Message );
			}

			return result;
		}

		/// <summary>
		/// Cleans the DOM of annoying content like fixed banners or popups and returns a rectangle containing the main content of the web page
		/// </summary>
		async Task<RectangleF>	CleanDOMAndReturnMainContentRectangle() {

			JavascriptResponse	JSResult = await ExecuteJS( JSCodeIsolateMainContent() );
			if ( !JSResult.Success )
				throw new Exception( "JS request failed: DOM not cleared and no workable viewport dimension was returned... Reason: " + JSResult.Message );
			if ( !(JSResult.Result is string) ) {
				Log( LOG_TYPE.WARNING, "Failed to retrieve workable rectangle for page's main content: defaulting to entire browser window..." );
				return RectangleF.Empty;
			}

			// Wait for a while before continuing
			await Delay_ms( m_delay_ms_CleanDOM );

			// Parse the resulting bounding rectangle
			try {
				JSON			parser = new JSON();
				JSON.JSONObject	root = null;
				using ( System.IO.StringReader R = new System.IO.StringReader( JSResult.Result as string ) ) {
					root = parser.ReadJSON( R )["root"];

					RectangleF	result = new RectangleF();
					result.X = (float) root["x"].AsDouble;
					result.Y = (float) root["y"].AsDouble;
					result.Width = (float) root["width"].AsDouble;
					result.Height = (float) root["height"].AsDouble;

					return result;
				}
			} catch ( Exception _e ) {
				Log( LOG_TYPE.ERROR, "Failed to parse resulting page's main rectangle because of \"{0}\" : defaulting to entire browser window...", _e.Message );
				return RectangleF.Empty;
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
				await Delay_ms( _delay_ms );
			}

			return task.Result;
		}

		async Task	Delay_ms( int _delay_ms ) {
			await Task.Delay( _delay_ms );
		}

		async Task	AsyncWaitForStablePage( string _waiter ) {
			const int	MAX_COUNTER = 100;

			int	counter = 0;
			while ( counter < MAX_COUNTER ) {
//Log( LOG_TYPE.DEBUG, "AsyncWaitForStablePage( {0} ) => Waiting {1}", _waiter, counter++ );

				double	elapsedTimeSinceLastPageEvent = m_hasPageEvents ? (DateTime.Now - m_lastPageEvent).TotalMilliseconds : 0;
				if ( elapsedTimeSinceLastPageEvent > m_delay_ms_StablePage )
					return;	// Page seems stable enough...

//				Application.DoEvents();
				await Task.Delay( 250 );  // We do need these delays. Some pages, like facebook, may need to load viewport content.
			}

//Log( LOG_TYPE.DEBUG, "AsyncWaitForStablePage( {0} ) => Exiting after {1} loops!", _waiter, counter );
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

		#region Javascript Code

		/// <summary>
		/// Makes the page scroll down to the provided position
		/// The expected return value should be undefined
		/// </summary>
		/// <param name="_Y"></param>
		/// <returns></returns>
		string	JSCodeScroll( uint _Y ) {
			return "(function() { window.scrollTo( 0," +_Y + " ); })();";
		}

		/// <summary>
		/// Lists all the leaf DOM elements in the document
		/// The expected return value should be a JSON string of an array of leaf elements with information for each of them
		/// </summary>
		/// <returns></returns>
		string	JSCodeListDOMLeafElements() {
			return
@"
// From https://stackoverflow.com/questions/22289391/how-to-create-an-array-of-leaf-nodes-of-an-html-dom-using-javascript
function getLeafNodes( _root ) {
    var nodes = Array.prototype.slice.call( _root.getElementsByTagName( '*' ), 0 );
    var leafNodes = nodes.filter( function( _element ) {
        return !_element.hasChildNodes();
    });
    return leafNodes;
}

function IsFixedElement( _element ) {
	var	position = window.getComputedStyle( leafNode ).position;
	return position == 'sticky' || position == 'fixed';
}

(function() { 
	// Enumerate leaf DOM elements
	var	leafNodes = getLeafNodes( document.body );
//return leafNodes.length;

	// Query information for each
	var	leafNodeInformation = [];
	for ( var i=0; i < leafNodes.length; i++ ) {
		var	leafNode = leafNodes[i];

		var	leafNodeInfo = {
			bounds : leafNode.getBoundingClientRect(),
			fixed : IsFixedElement( leafNode ),			// True if the element is fixed
			index : 0									// Index of the element in the DOM
		};

		leafNodeInformation.push( leafNodeInfo );
	}

	// Convert into JSON
	return JSON.stringify( leafNodeInformation );
} )();
";
		}

		/// <summary>
		/// Isolates the main window with content
		/// The expected return value should be a JSON string of the bounding rectangle of the main element
		/// </summary>
		/// <returns></returns>
		string	JSCodeIsolateMainContent() {
return Properties.Resources.IsolateMainContent;
/*
return @"
// This function is used to know if an element is set with a 'fixed' position, which is what we're looking for: fixed elements that may block the viewport
function IsFixedElement( _element ) {
	var	position = window.getComputedStyle( _element ).position;
	return position == 'sticky' || position == 'fixed';
}

// Returns the top parent node that contains only this child node (meaning we stop going up to the parent if the parent has more than one child)
function GetParentWithSingleChild( _element ) {
	var	parent = _element.parentNode;
	if ( parent == null || parent.children.length > 1 )
		return _element;	// Stop at this element...

	return GetParentWithSingleChild( parent );
}

function RecurseGetFixedNodes( _element ) {
	if ( IsFixedElement( _element ) )
		return [ GetParentWithSingleChild( _element ) ];

	// Query information for each child
	var	childNodes = _element.children;
	var	fixedChildNodes = [];
	for ( var i=0; i < childNodes.length; i++ ) {
		fixedChildNodes = fixedChildNodes.concat( RecurseGetFixedNodes( childNodes[i] ) );
	}

	return fixedChildNodes;
}

function RemoveFixedNodes( _root ) {
	if ( _root == null )
		_root = document.body;

	RecurseGetFixedNodes( _root ).forEach( _element => _element.remove() );
}

(function() { 
	// Recursively enumerate fixed DOM elements
	var	leafNodes = RecurseGetFixedNodes( document.body );
//return leafNodes.length;

	// Remove them from the DOM
	leafNodes.forEach( _element => _elemente.remove() );

	// Enumerate all non empty nodes that contain either text or an image
	

//	// Query information for each
//	var	leafNodeInformation = [];
//	for ( var i=0; i < leafNodes.length; i++ ) {
//		var	leafNode = leafNodes[i];
//		var	nodeBounds = leafNode.getBoundingClientRect();
//
//		var	leafNodeInfo = {
//			bounds : { x: nodeBounds.x, y: nodeBounds.y, w: nodeBounds.width, h: nodeBounds.height },
//		};
//
//		leafNodeInformation.push( leafNodeInfo );
//	}
//
//	// Convert into JSON
//	return JSON.stringify( leafNodeInformation );
} )();
";*/
		}

// Some invalid example: correctly queries leaves but useless since we need to climb back up!
		string	JSCodeListDOMFixedElements_GetLeavesThenGoBackUp() {
			return @"
// From https://stackoverflow.com/questions/22289391/how-to-create-an-array-of-leaf-nodes-of-an-html-dom-using-javascript
function getLeafNodes( _root ) {
    var nodes = Array.prototype.slice.call( _root.getElementsByTagName( '*' ), 0 );
    var leafNodes = nodes.filter( function( _element ) {
        return !_element.hasChildNodes();
    });
    return leafNodes;
}

function IsFixedElement( _element ) {
	var	position = window.getComputedStyle( _element ).position;
	return position == 'sticky' || position == 'fixed';
}

function GetParentFixedElement( _element ) {
	if ( _element == null )
		return null;

	if ( IsFixedElement( _element ) )
		return _element;

	return GetParentFixedElement( _element.parentNode );
}

(function() { 
	// Enumerate leaf DOM elements
	var	leafNodes = getLeafNodes( document.body );
//return leafNodes.length;

	// Query information for each
	var	leafNodeInformation = [];
	for ( var i=0; i < leafNodes.length; i++ ) {
		var	leafNode = leafNodes[i];
		var	fixedParentNode = GetParentFixedElement( leafNode );
		if ( fixedParentNode == null )
			continue;	// Not a fixed node

		var	leafNodeInfo = {
			bounds : fixedParentNode.getBoundingClientRect(),
		};

		leafNodeInformation.push( leafNodeInfo );
	}

	// Convert into JSON
	return JSON.stringify( leafNodeInformation );
} )();
";
		}

		/// Récupère un DOM element via son selector
		/// document.querySelector( "selector" )
		///
		/// Récupère les child elements
		/// document.querySelector( "selector" ).children
		/// 
		/// Récupère le bounding rect
		/// document.querySelector( "selector" ).getBoundingClientRect()
		///
		/// Exemple de selector:
		/// "#react-root > div > div > div > main > div > div > div > div.css-1dbjc4n.r-14lw9ot.r-1tlfku8.r-1ljd8xs.r-13l2t4g.r-1phboty.r-1jgb5lz.r-11wrixw.r-61z16t.r-1ye8kvj.r-13qz1uu.r-184en5c > div > div.css-1dbjc4n.r-aqfbo4.r-14lw9ot.r-my5ep6.r-rull8r.r-qklmqi.r-gtdqiz.r-ipm5af.r-1g40b8q"




		#endregion

		#endregion
	}
}
