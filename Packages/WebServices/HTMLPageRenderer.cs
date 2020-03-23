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

	/// <summary>
	/// Class rendering web pages into an offscreen bitmap
	/// The class will auto-dispose itself and release the browser once the page is successfully rendered, or if an error occurred
	/// </summary>
	public class HTMLPageRenderer : IDisposable {

		#region NESTED TYPES

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

		private BrowsersPool.Browser	m_browser = null;

		private int						m_delay_ms_StablePage = 5000;			// Page is deemed stable if no event have been received for over 5s
		private int						m_delay_ms_ScrollDown = 250;			// Wait for 250ms before taking a screenshot after a page scroll on ROUND 2
		private int						m_delay_ms_CleanDOM = 1000;				// Wait for 1000ms after cleaning DOM

		private int						m_timeOut_ms_JavascriptNoRender = 1000;	// Default timeout after 1s of a JS command that doesn't trigger a new rendering
		private int						m_timeOut_ms_PageRender = 30000;		// Default timeout after 30s for a page rendering
		private int						m_timeOut_ms_Screenshot = 10000;		// Default timeout after 1s for a screenshot

		private string					m_URL;
		private int						m_maxScreenshotsCount;

		private WebPageSourceAvailable	m_pageSourceAvailable;
		private WebPageRendered			m_pageRendered;
		private WebPageSuccess			m_pageSuccess;
		private WebPageErrorOccurred	m_pageError;

		private LogDelegate				m_logDelegate;

		#endregion

		#region METHODS

		public HTMLPageRenderer( BrowsersPool.Browser _browser, string _URL, int _browserViewportWidth, int _browserViewportHeight, int _maxScreenshotsCount, WebPageSourceAvailable _pageSourceAvailable, WebPageRendered _pageRendered, WebPageSuccess _pageSuccess, WebPageErrorOccurred _pageError, LogDelegate _logDelegate ) {

			m_browser = _browser;
			m_pageSourceAvailable = _pageSourceAvailable;
			m_pageRendered = _pageRendered;
			m_pageSuccess = _pageSuccess;
			m_pageError = _pageError;
			m_logDelegate = _logDelegate != null ? _logDelegate : DefaultLogger;

			// https://github.com/cefsharp/CefSharp/wiki/General-Usage#handlers
			m_browser.LoadError += browser_LoadError;
			m_browser.LoadingStateChanged += browser_LoadingStateChanged;
			m_browser.FrameLoadStart += browser_FrameLoadStart;
			m_browser.FrameLoadEnd += browser_FrameLoadEnd;

			// Setup page and size
			m_URL = _URL;
			m_maxScreenshotsCount = _maxScreenshotsCount;

 			if ( _browserViewportHeight == 0 )
 				_browserViewportHeight = (int) (_browserViewportWidth * 1.6180339887498948482045868343656);

			m_browser.Size = new Size( _browserViewportWidth, _browserViewportHeight );

			// No event have been registered yet
			m_hasPageEvents = false;

			// Start actual page loading
			m_browser.Load( m_URL );

			// Execute waiting task
			Task	T = new Task( WaitForPageRendered );
					T.Start();
		}

		bool	m_disposed = false;
		public virtual void	Dispose() {

			if ( m_disposed )
				throw new Exception( "Already disposed!" );

			m_disposed = true;

			m_browser.Stop();

			m_browser.LoadError -= browser_LoadError;
			m_browser.LoadingStateChanged -= browser_LoadingStateChanged;
			m_browser.FrameLoadStart -= browser_FrameLoadStart;
			m_browser.FrameLoadEnd -= browser_FrameLoadEnd;

			m_browser.Release();
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
			JavascriptResponse	JSResult = await ExecuteJS( Properties.Resources.Helpers, 100 );
			if ( !JSResult.Success )
				throw new Exception( "Failed to execute helper scripts: " + JSResult.Message );


			//////////////////////////////////////////////////////////////////////////
			// Ask for the page's height (not always reliable, especially on infinite scrolling feeds like facebook or twitter!)
			//
			JSResult = await ExecuteJS( "GetPageHeight();" );
			int	scrollHeight = (int) JSResult.Result;
//Console.WriteLine( "Scroll height = " + scrollHeight );

			// Ask for background color
			Color	backgroundColor = Color.White;
			JSResult = await ExecuteJS( "(function() { return window.getComputedStyle( document.body ).backgroundColor.toString(); })();" );
			if ( JSResult.Success ) {
//				backgroundColor = 
			}


			//////////////////////////////////////////////////////////////////////////
			// Retrieve initial scroll value
			JSResult = await ExecuteJS( "window.scrollY;" );
			int	initialScrollY = JSResult.Success ? (int) JSResult.Result : 0;


			//////////////////////////////////////////////////////////////////////////
			// Perform as many screenshots as necessary to capture the entire page
			//
			int	viewportHeight = m_browser.Size.Height;
			int	screenshotsCount = (int) Math.Ceiling( (double) scrollHeight / viewportHeight );
Log( LOG_TYPE.DEBUG, "Page scroll height = " + scrollHeight + " - Screenshots Count = " + screenshotsCount );

			using ( Brush backgroundBrush = new SolidBrush( backgroundColor ) )
				await DoScreenshots( screenshotsCount, initialScrollY, scrollHeight, backgroundBrush );


			//////////////////////////////////////////////////////////////////////////
			// Query the HTML source code and DOM content
			//
			await QueryContent( backgroundColor, initialScrollY );


			//////////////////////////////////////////////////////////////////////////
			// Notify the page was successfully loaded
			//
			m_pageSuccess();

			// Autodispose...
			Dispose();
		}

		/// <summary>
		/// Do multiple screenshots to capture the entire page
		/// </summary>
		/// <returns></returns>
		async Task	DoScreenshots( int _scrollsCount, int _initialScrollY, int _scrollHeight, Brush _backgroundBrush ) {
			_scrollsCount = Math.Min( m_maxScreenshotsCount, _scrollsCount );

			int			viewportWidth = m_browser.Size.Width;
			int			viewportHeight = m_browser.Size.Height;
			Rectangle	defaultTotalContentRectangle = new Rectangle( 0, 0, viewportWidth, _scrollHeight );	// Large rectangle covering several times the browser's rectangle

			try {
				// Code from https://github.com/WildGenie/OSIRTv2/blob/3e60d3ce908a1d25a7b4633dc9afdd53256cbb4f/OSIRT/Browser/MainBrowser.cs#L300
//				await m_browser.GetBrowser().MainFrame.EvaluateScriptAsync("(function() { document.documentElement.style.overflow = 'hidden'; })();");
//				await ExecuteTaskOrTimeOut( m_browser.GetBrowser().MainFrame.EvaluateScriptAsync( "(function() { document.documentElement.style.overflow = 'hidden'; })();" ), m_timeOut_ms_JavascriptNoRender );

//				await ExecuteJS( "(function() { document.documentElement.style.overflow = 'hidden'; })();" );


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
						RectangleF	clientContentRectangleF = await CleanDOMAndReturnMainContentRectangle();
						Rectangle	clientContentRectangle = new Rectangle(	(int) Mathf.Floor( clientContentRectangleF.X ),
																			(int) Mathf.Floor( clientContentRectangleF.Y ),
																			(int) (1 + Mathf.Ceiling( clientContentRectangleF.Right ) - Mathf.Floor( clientContentRectangleF.X ) ),
																			(int) (1 + Mathf.Ceiling( clientContentRectangleF.Bottom ) - Mathf.Floor( clientContentRectangleF.Y ) )
																	);
						if ( clientContentRectangle.IsEmpty ) {
							// Use default rectangle covering the entire screen...
							clientContentRectangle = defaultTotalContentRectangle;
							clientContentRectangle.Offset( 0, -(_initialScrollY + scrollIndex * viewportHeight) );
						}

Log( LOG_TYPE.DEBUG, "DoScreenshots() => (ROUND 2) Cleaning DOM and getting viewport ({0}, {1}, {2}, {3})", clientContentRectangle.X, clientContentRectangle.Y, clientContentRectangle.Width, clientContentRectangle.Height );


						//////////////////////////////////////////////////////////////////////////
						/// Request a screenshot
 						Task<Bitmap>	taskScreenshot = (await ExecuteTaskOrTimeOut( m_browser.ScreenshotAsync( false ), m_timeOut_ms_Screenshot, "m_browser.ScreenshotAsync()" )) as Task<Bitmap>;

Log( LOG_TYPE.DEBUG, "DoScreenshots() => (ROUND 2) Retrieved web page image screenshot {0} / {1}", 1+scrollIndex, _scrollsCount );


						//////////////////////////////////////////////////////////////////////////
						/// Clip bitmap with intersection of content and viewport rectangles
						/// 
 						int			remainingHeight = _scrollHeight - (_initialScrollY + scrollIndex * viewportHeight);
									remainingHeight = Math.Min( viewportHeight, remainingHeight );

						Rectangle	viewportRectangle = new Rectangle( 0, viewportHeight - remainingHeight, viewportWidth, remainingHeight );
						Rectangle	clippedContentRectangle = clientContentRectangle;
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

								// Compute absolute DOM rectangles
								await ComputeDOMRectangles( (uint) absoluteContentRectangle.Left, (uint) absoluteContentRectangle.Top );

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
							int	scrollPosition = Math.Min( _scrollHeight, _initialScrollY + (scrollIndex+1) * viewportHeight );
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

			} catch ( Exception _e ) {
				m_pageError( -1, "An error occurred while attempting to render a page screenshot for URL \"" + m_URL + "\": \r\n" + _e.Message );
			}
		}

		/// <summary>
		/// Reads back HTML content and do a screenshot
		/// </summary>
		/// <returns></returns>
		async Task	QueryContent( Color _backgroundColor, int _initialScrollY ) {
			try {
Log( LOG_TYPE.DEBUG, "QueryContent for " + m_URL );

				// From Line 162 https://github.com/WildGenie/OSIRTv2/blob/3e60d3ce908a1d25a7b4633dc9afdd53256cbb4f/OSIRT/Browser/MainBrowser.cs#L300
				string	source = await m_browser.MainFrame.GetSourceAsync();
				if ( source == null )
					throw new Exception( "Failed to retrieve HTML source!" );

Log( LOG_TYPE.DEBUG, "QueryContent() => Retrieved HTML code " + (source.Length < 100 ? source : source.Remove( 100 )) );

				JavascriptResponse	JSResult = await ExecuteJS( "(function() { return document.title; } )();" );
				string	pageTitle = JSResult.Result as string;

				// Retrive DOM elements
				XmlDocument	DOMContent = await RetrieveDOMContent( _initialScrollY );

Log( LOG_TYPE.DEBUG, "QueryContent() => Retrieved {0} content elements from DOM", DOMContent["root"].ChildNodes.Count );

				// Notify source is ready
				m_pageSourceAvailable( pageTitle, source, DOMContent );

			} catch ( Exception _e ) {
				m_pageError( -1, "An error occurred while attempting to retrieve HTML source and DOM content elements for URL \"" + m_URL + "\": \r\n" + _e.Message );
			}
		}

		#region Events

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

			// Autodispose...
			Dispose();
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

		#endregion

		#region Helpers

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
			Task<JavascriptResponse>	task = (await ExecuteTaskOrTimeOut( m_browser.MainFrame.EvaluateScriptAsync( _JS, null ), m_timeOut_ms_JavascriptNoRender, "EvaluateScriptAsync " + _JS )) as Task<JavascriptResponse>;

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
				if ( !m_browser.IsLoading && elapsedTimeSinceLastPageEvent > m_delay_ms_StablePage )
					return;	// Page seems stable enough...

//				Application.DoEvents();
				await Task.Delay( 250 );  // We do need these delays. Some pages, like facebook, may need to load viewport content.
			}

//Log( LOG_TYPE.DEBUG, "AsyncWaitForStablePage( {0} ) => Exiting after {1} loops!", _waiter, counter );
		}

		async Task	WaitForStablePage( string _waiter ) {
			await ExecuteTaskOrTimeOut( AsyncWaitForStablePage( _waiter ), m_timeOut_ms_PageRender, "AsyncWaitForStablePage()" );
		}

		public void	Log( LOG_TYPE _type, string _text, params object[] _arguments ) {
			_text = string.Format( "[" + m_browser.Name + " - " + m_URL + "] " + _text, _arguments );
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

		#region Javascript Code

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
		/// Cleans the DOM of annoying content like fixed banners or popups and returns a rectangle containing the main content of the web page
		/// </summary>
		/// <returns></returns>
		async Task<RectangleF>	CleanDOMAndReturnMainContentRectangle() {

			JavascriptResponse	JSResult = await ExecuteJS( Properties.Resources.IsolateMainContent );
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
		/// Computes the absolute DOM rectangle positions
		/// </summary>
		/// <returns></returns>
		async Task	ComputeDOMRectangles( uint _offsetX, uint _offsetY ) {
			JavascriptResponse	JSResult = await ExecuteJS( Properties.Resources.ComputeDOMRectangles + "\r\nComputeDOMRectangles( " + _offsetX + ", " + _offsetY + ")" );
			if ( !JSResult.Success )
				throw new Exception( "JS request failed: DOM content rectangles not computed. Reason: " + JSResult.Message );
		}

		/// <summary>
		/// Lists all the content elements in the DOM (images, links and text) 
		/// </summary>
		/// <returns></returns>
		async Task<XmlDocument>	RetrieveDOMContent( int _initialScrollY ) {

//Console.WriteLine( "InitialY = " + _initialScrollY );

			// Execute DOM retrieval script
			JavascriptResponse	JSResult = await ExecuteJS( Properties.Resources.RetrieveDOMElements );
			if ( !JSResult.Success )
				throw new Exception( "Failed to execute retrieval of DOM content elements: " + JSResult.Message );

			// Readback results
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

						RectangleF	clientRect = new RectangleF(
							(float) element["x"].AsDouble,
							(float) element["y"].AsDouble,
							(float) element["w"].AsDouble,
							(float) element["h"].AsDouble
						);
						clientRect.Offset( 0, -_initialScrollY );

//Console.WriteLine( "Rectangle Y = " + clientRect.Y );

						xmlElement.SetAttribute( "path", element["path"].AsString );
						xmlElement.SetAttribute( "type", elementTypeName );
						xmlElement.SetAttribute( "x", clientRect.X.ToString() );
						xmlElement.SetAttribute( "y", clientRect.Y.ToString() );
						xmlElement.SetAttribute( "w", clientRect.Width.ToString() );
						xmlElement.SetAttribute( "h", clientRect.Height.ToString() );
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

		#endregion

		#endregion
	}
}
