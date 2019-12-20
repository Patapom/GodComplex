//#define DEBUG_SINGLE_THREADED

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Brain2 {

	/// <summary>
	/// The main fiches database
	/// </summary>
	[System.Diagnostics.DebuggerDisplay( "{m_fiches.Count} Registered Fiches" )]
	public class FichesDB : IDisposable {

		#region CONSTANTS

		protected const uint	MAX_WORKING_THREADS_COUNT = 10;

// 		public const uint	SIGNATURE = 0x48434946U;	// 'FICH';
// 		public const ushort	VERSION_MAJOR = 1;
// 		public const ushort	VERSION_MINOR = 0;

		#endregion

		#region NESTED TYPES

		#region Fiche Types Handlers

		public interface IDataHandler {
			int		FormatScore( string _formatLowerCase );		// Must return 0 if not supported
			Fiche	CreateFiche( FichesDB _database, string _format, object _data );
		}

		/// <summary>
		/// Simple text fiche
		/// </summary>
		/// <remarks> This type also handles "http://" and "https://" strings and redirects them to the URL handler</remarks>
		public class	TextHandler : IDataHandler {

			public int	FormatScore( string _formatLowerCase ) {
				return _formatLowerCase == "system.string" || _formatLowerCase == "text" ? 1 : 0;
			}

			public Fiche CreateFiche( FichesDB _database, string _format, object _data ) {
				string	text = _data.ToString();
				if ( text.StartsWith( "http://" ) || text.StartsWith( "https://" ) )
					return URLHandler.CreateURLFiche( _database, null, WebHelpers.CreateCanonicalURL( text ) );

				string	title = text;	// Do better?
				string	HTML = WebHelpers.BuildHTMLDocument( title, text );
				Fiche	F = _database.SyncCreateFicheDescriptor( Fiche.TYPE.LOCAL_EDITABLE_WEBPAGE, text, null, null, HTML );
				_database.AsyncSaveFiche( F, true );
				return F;
			}
		}

		/// <summary>
		/// URL fiche: the web page is queried and the fiche is created once the web content and a screenshot are available
		/// </summary>
		public class	URLHandler : IDataHandler {

			public int	FormatScore( string _formatLowerCase ) {
				switch ( _formatLowerCase ) {
					case "text/x-moz-url":
					case "uniformresourcelocator":
					case "uniformresourcelocatorw":
						return 10;
				}

				return 0;
			}

			public Fiche CreateFiche( FichesDB _database, string _format, object _data ) {
				MemoryStream	S = _data as MemoryStream;
				if ( S == null )
					throw new Exception( "Provided data is not the expected MemoryStream type!" );

				_format = _format.ToLower();
				bool	isUnicode = _format == "text/x-moz-url" || _format == "uniformresourcelocatorw";

// string	debug = "Length = " + S.Length + "\r\n<bold>";
// for ( int i=0; i < S.Length; i++ ) {
// 	debug += S.ReadByte().ToString( "X2" );
// }
// BrainForm.Debug( debug );
//string	HTML = "<body>" + debug + "</bold></body>";

				// Assume the memory stream contains a unicode string
				int						stringLength = (int) S.Length;
				System.Text.Encoding	encoding = isUnicode ? System.Text.Encoding.Unicode : System.Text.Encoding.ASCII;

				if ( isUnicode ) {
					if ( (stringLength & 1) != 0 )
						throw new Exception( "Expected an even size! Apparently we're not really dealing with a unicode string..." );
					stringLength >>= 1;
				}

				char[]	URLchars = new char[stringLength];
				using ( StreamReader R = new StreamReader( S, encoding ) ) {
					R.Read( URLchars, 0, URLchars.Length );
				}

				string		title = null;
				string		strURL = new string( URLchars );
				string[]	URLLines = strURL.Split( '\n' );
				if ( URLLines.Length > 1 ) {
					// URL is multi-part: must have a title after the actual link...
					strURL = URLLines[0];
					title = URLLines[1];
				}

				Uri	URL = WebHelpers.CreateCanonicalURL( strURL );

				return CreateURLFiche( _database, title, URL );
			}

			public static Fiche	CreateURLFiche( FichesDB _database, string _title, Uri _URL ) {
				if ( _URL == null )
					throw new Exception( "Invalid null URL to create fiche!" );
				if ( !Uri.IsWellFormedUriString( _URL.AbsoluteUri, UriKind.Absolute ) ) {
					throw new Exception( "Invalid URL to create fiche!" );
				}

				// Patch any empty title
				if ( _title == null )
					_title = "";

				// Extract any tags from the title and fetch or create the tags ourselves
				string[]		tags = WebHelpers.ExtractTags( _title );
				List< Fiche >	tagFiches = new List<Fiche>();
				foreach ( string tag in tags ) {
					Fiche	tagFiche = _database.SyncFindOrCreateTagFiche( tag );
					tagFiches.Add( tagFiche );
				}

				// Create the descriptor
				Fiche	F = _database.SyncCreateFicheDescriptor( Fiche.TYPE.REMOTE_ANNOTABLE_WEBPAGE, _title, _URL, tagFiches.ToArray(), null );

				// Load the web page and save the fiche when ready
				_database.AsyncLoadContentAndSaveFiche( F, true );

				return F;
			}
		}

		#endregion

		#region Asynchronous Jobs

		protected class WorkingThread : IDisposable {

			#region NESTED TYPES

			public abstract class JobBase {
				protected FichesDB	m_owner;

				protected JobBase( FichesDB _owner ) {
					m_owner = _owner;
				}

				public abstract void	Run();
			}

			public class		JobFillFiche : JobBase {
				public Fiche		m_fiche;
				public bool			m_unloadContentAfterSave;
				public JobFillFiche( FichesDB _owner, Fiche _fiche, bool _unloadContentAfterSave ) : base( _owner ) { m_fiche = _fiche; m_unloadContentAfterSave = _unloadContentAfterSave; }
				public override void	Run() {
					// Request the content asynchronously
					m_owner.AsyncRenderWebPage( m_fiche.URL,
						// On success => update content
						( string _HTMLContent, ImageUtility.ImageFile _imageWebPage ) => {
							m_fiche.Lock( Fiche.STATUS.UPDATING, () => {

								m_fiche.HTMLContent = _HTMLContent;

								// Create image chunk
								m_fiche.CreateImageChunk( _imageWebPage );

								// Create thumbnail
								m_fiche.CreateThumbnailChunkFromImage( _imageWebPage );

								// Request to save the fiche now that it's complete...
								m_owner.AsyncSaveFiche( m_fiche, m_unloadContentAfterSave );
							} );
						},
						// On error => Log error? Todo?
						( WebHelpers.WEB_ERROR_TYPE _error, int _errorCode, string _message ) => {
							throw new Exception( "Handle web errors!" );
						} );
				}
			}

			public class		JobLoadChunk : JobBase {
				public Fiche.ChunkBase	m_caller;
				public JobLoadChunk( FichesDB _owner, Fiche.ChunkBase _caller ) : base( _owner ) { m_caller = _caller; }
				public override void	Run() {
					using ( Stream S = m_owner.SyncRequestFicheStream( m_caller.OwnerFiche, true ) ) {
						m_caller.OwnerFiche.Lock( Fiche.STATUS.LOADING, () => {
							m_caller.Threaded_LoadContent( S );
						} );
					}
				}
			}

			public class		JobSaveFiche : JobBase {
				public Fiche		m_caller;
				public bool			m_unloadContentAfterSave;
				public JobSaveFiche( FichesDB _owner, Fiche _caller, bool _unloadContentAfterSave ) : base( _owner ) { m_caller = _caller; m_unloadContentAfterSave = _unloadContentAfterSave; }
				public override void	Run() {
					using ( Stream S = m_owner.SyncRequestFicheStream( m_caller, false ) ) {
						using ( BinaryWriter W = new BinaryWriter( S ) ) {
							// Ensure the fiche is ready so it can be saved
							if ( m_caller.Status != Fiche.STATUS.READY )
								throw new Exception( "Can't save while fiche is not ready!" );

							m_caller.Lock( Fiche.STATUS.SAVING, () => {
								m_caller.Write( W );

								if ( m_unloadContentAfterSave )
									m_caller.UnloadImageChunk();
							} );
						}
					}
				}
			}

			public class		JobNotify : JobBase {
				public Action	m_action;
				public JobNotify( FichesDB _owner, Action _action ) : base( _owner ) { m_action = _action; }
				public override void	Run() {
					m_action();
				}
			}

			public class		JobReportError : JobBase {
				public string	m_error;
				public JobReportError( FichesDB _owner, string _error ) : base( _owner ) { m_error = _error; }
				public override void	Run() {
					m_owner.SyncReportError( m_error );
				}
			}

			public class		JobLoadWebPage : JobBase {
				public Uri							m_URL;
				public WebHelpers.WebPageRendered	m_delegateSuccess;
				public WebHelpers.WebPageError		m_delegateError;
				public JobLoadWebPage( FichesDB _owner, Uri _URL, WebHelpers.WebPageRendered _onSuccess, WebHelpers.WebPageError _onError ) : base( _owner ) {
					m_URL = _URL;
					m_delegateSuccess = _onSuccess;
					m_delegateError = _onError;
				}
				public override void	Run() {
					WebHelpers.LoadWebPage( m_URL, m_delegateSuccess, m_delegateError );
				}
			}

			#endregion

			public FichesDB		m_database = null;
			public Thread		m_thread = null;

			public WorkingThread( FichesDB _database ) {
				m_database = _database;
				m_thread = new Thread( WorkingThreadDelegate );
				m_thread.IsBackground = true;
				m_thread.Start( this );
			}
			public void Dispose() {
				m_thread.Abort();
				m_thread = null;
			}

			protected static void	WorkingThreadDelegate( object _param ) {
				WorkingThread	_this = _param as WorkingThread;
				Thread			thisThread = Thread.CurrentThread;

				while ( true ) {
					Thread.Sleep( 100 );	// Check for jobs every 1/10 of a second

					lock ( _this.m_database.m_threadedJobs ) {
						if ( _this.m_database.m_threadedJobs.Count > 0 ) {
							JobBase	job = _this.m_database.m_threadedJobs.Dequeue();
							try {
								job.Run();
							} catch ( Exception _e ) {
								_this.m_database.SyncReportError( "A \"" + job.ToString() + "\" failed with error: " + _e.Message );
							}
						}
					}
				}
			}
		}

		#endregion

		public class DatabaseLoadException : Exception {
			public Exception[]	m_errors;
			public DatabaseLoadException( Exception[] _errors ) {
				m_errors = _errors;
			}
		}

		#endregion

		#region FIELDS

		private	DirectoryInfo						m_rootFolder = null;

		private List< Fiche >						m_fiches = new List< Fiche >();
		private Dictionary< Guid, Fiche >			m_GUID2Fiche = new Dictionary<Guid, Fiche>();
		private Dictionary< Uri, Fiche >			m_URL2Fiche = new Dictionary<Uri, Fiche>();

		// Tags/titles references
		private Dictionary< string, List< Fiche > >	m_titleCaseSensitive2Fiches = new Dictionary<string, List<Fiche>>();
		private Dictionary< string, List< Fiche > >	m_titleNoCase2Fiches = new Dictionary<string, List<Fiche>>();

		private Dictionary< string, List< Fiche > >	m_t2Fiches = new Dictionary<string, List<Fiche>>();
		private Dictionary< string, List< Fiche > >	m_ti2Fiches = new Dictionary<string, List<Fiche>>();
		private Dictionary< string, List< Fiche > >	m_tit2Fiches = new Dictionary<string, List<Fiche>>();
		private Dictionary< string, List< Fiche > >	m_titl2Fiches = new Dictionary<string, List<Fiche>>();

		// Asynchronous workers & job queues
		protected WorkingThread[]					m_workingThreads = null;
		protected Queue< WorkingThread.JobBase >	m_threadedJobs = new Queue<WorkingThread.JobBase>();
		protected Queue< WorkingThread.JobBase >	m_mainThreadJobs = new Queue<WorkingThread.JobBase>();

		// Fiche creation handlers
		public List< IDataHandler >					m_ficheTypeHandlers = new List<IDataHandler>();

		#endregion

		#region PROPERTIES

		#endregion

		#region METHODS

		public	FichesDB() {
			// Register fiche handlers
			m_ficheTypeHandlers.Add( new URLHandler() );
			m_ficheTypeHandlers.Add( new TextHandler() );

			// Create working threads
			#if !DEBUG_SINGLE_THREADED
				m_workingThreads = new WorkingThread[MAX_WORKING_THREADS_COUNT];
				for (  uint i=0; i < m_workingThreads.Length; i++ ) {
					m_workingThreads[i] = new WorkingThread( this );
				}
			#endif
		}

		public void Dispose() {
			for (  uint i=0; i < m_workingThreads.Length; i++ ) {
				m_workingThreads[i].Dispose();	// This will abort the threads
			}

			// Release all fiches
			Fiche[]	fiches = m_fiches.ToArray();
			foreach ( Fiche F in fiches ) {
				F.Dispose();
			}
		}

		#region Queries

		public Fiche	FindFicheByGUID( Guid _GUID ) {
			Fiche	result = null;
			m_GUID2Fiche.TryGetValue( _GUID, out result );
			return result;
		}

		public Fiche	FindFicheByURL( Uri _URL ) {
			if ( _URL == null )
				throw new Exception( "Invalid URL!" );

			Fiche	result = null;
			m_URL2Fiche.TryGetValue( _URL, out result );
			return result;
		}

		public Fiche[]	FindFichesByTitle( string _title, bool _caseSensitive ) {
			List< Fiche >	fiches = null;
			if ( _caseSensitive ) {
				m_titleCaseSensitive2Fiches.TryGetValue( _title, out fiches );
			} else {
				_title = _title.ToLower();
				m_titleNoCase2Fiches.TryGetValue( _title, out fiches );
			}
			return fiches == null ? new Fiche[0] : fiches.ToArray();
		}

		/// <summary>
		/// Used for auto-completion
		/// </summary>
		/// <param name="_title"></param>
		/// <returns></returns>
		public void	FindNearestTagMatches( string _title, List< Fiche > _matches ) {
			if ( _title == null )
				throw new Exception( "Invalid title!" );

			// List exact matches first
			List<Fiche>	results = null;
			switch ( _title.Length ) {
				case 0: return;	// Can't process
				case 1:
					if ( m_t2Fiches.TryGetValue( _title, out results ) )
						_matches.AddRange( results );
					break;
				case 2:
					if ( m_ti2Fiches.TryGetValue( _title, out results ) )
						_matches.AddRange( results );
					break;
				case 3:
					if ( m_tit2Fiches.TryGetValue( _title, out results ) )
						_matches.AddRange( results );
					break;
				case 4:
					if ( m_titl2Fiches.TryGetValue( _title, out results ) )
						_matches.AddRange( results );
					break;
			}

			// List approximate results
			if ( _title.Length > 4 ) {

			}

			// Sort results by references count so most used tags are listed first
			_matches.Sort( ( Fiche x, Fiche y ) => { return x.ReferencesCount < y.ReferencesCount ? -1 : (x.ReferencesCount > y.ReferencesCount ? 1 : 0); } );
		}

		// @TODO: Advanced search => in content, by tag, etc.

		#endregion

		#region I/O

		// 		public void	SaveDatabase( DirectoryInfo _rootFolder ) {
		// 			List< Exception >	errors = new List<Exception>();
		// 			foreach ( Fiche fiche in m_fiches ) {
		// 				try {
		// 
		// 					string		fileName = fiche.FileName;
		// 					FileInfo	file = new FileInfo( fileName );
		// 					using ( FileStream S = file.Create() )
		// 						using ( BinaryWriter W = new BinaryWriter( S ) )
		// 							fiche.Write( W );
		// 
		// 				} catch ( Exception _e ) {
		// 					errors.Add( _e );
		// 				}
		// 			}
		// 
		// 			if ( errors.Count > 0 )
		// 				throw new Exception( "Errors while saving database:" );
		// 		}

		public void	LoadFichesDescription( DirectoryInfo _rootFolder ) {
			List< Exception >	errors = new List<Exception>();
			try {
				if ( _rootFolder == null || !_rootFolder.Exists )
					throw new Exception( "Invalid root folder!" );

				m_rootFolder = _rootFolder;

// 				// Release all existing fiches first
// 				Fiche[]	fiches = m_fiches.ToArray();
// 				foreach ( Fiche F in fiches ) {
// 					F.Dispose();
// 				}

				// Prepare the Everything query
				string	everythingQuery = "parent:" + _rootFolder.FullName.Replace( "/", "\\" );
				if ( !everythingQuery.EndsWith( "\\" ) )
					everythingQuery += "\\";
				everythingQuery += " .fiche";

				Everything.Search.Result[]	results = null;
				try {
					Everything.Search.MatchPath = true;
					Everything.Search.SearchExpression = everythingQuery;
					Everything.Search.ExecuteQuery();	// Synchronous query
					results = Everything.Search.Results;
				} catch ( Exception _e ) {
					throw new Exception( "Everything query failed!", _e );
				}

				// Process fiches
				foreach ( Everything.Search.Result result in results ) {
					try {
						if ( !result.IsFile )
							continue;

						FileInfo	file = new FileInfo( result.FullName );
						if ( !file.Exists )
							throw new Exception( "Returned fiche file \"" + result.PathName + "\" doesn't exist!" );

						// Attempt to read fiche from file
						Fiche	F = null;
						try {
							using ( FileStream S = file.OpenRead() ) {
								using ( BinaryReader R = new BinaryReader( S ) ) {
									F = new Fiche( this, R );
								}
							}
						} catch ( Exception _e ) {
							throw new Exception( "Error reading fiche \"" + file.FullName + "\"!", _e );
						}

					} catch ( Exception _e ) {
						errors.Add( _e );
					}
				}

			} catch ( Exception _e ) {
				errors.Add( _e );
			} finally {
				// Resolve all possible tag fiches given their GUID
				foreach ( Fiche F in m_fiches ) {
					F.ResolveTags( m_GUID2Fiche );
				}
			}

			if ( errors.Count > 0 )
				throw new DatabaseLoadException( errors.ToArray() );
		}

		public void	Rebase( DirectoryInfo _rootFolder ) {
			if ( _rootFolder == null )
				throw new Exception( "Invalid rebase folder!" );

			if ( !_rootFolder.Exists )
				_rootFolder.Create();

throw new Exception( "TODO!" );
// 			// Save existing database to new folder and reload from there
// 			SaveDatabase( _rootFolder );
// 			LoadDatabase( _rootFolder );
		}

		/// <summary>
		/// Processes main thread messages
		/// </summary>
		public void	OnIdle() {
			lock ( m_mainThreadJobs ) {
				while ( m_mainThreadJobs.Count > 0 ) {
					WorkingThread.JobBase	job = m_mainThreadJobs.Dequeue();
					try {
						job.Run();
					} catch ( Exception _e ) {
						SyncReportError( "A \"" + job.ToString() + "\" failed with error: " + _e.Message );
					}
				}
			}

#if DEBUG_SINGLE_THREADED
	lock ( m_threadedJobs )
		while ( m_threadedJobs.Count > 0 ) {
			WorkingThread.JobBase	job = m_threadedJobs.Dequeue();
			try {
				job.Run();
			} catch ( Exception _e ) {
				SyncReportError( "A \"" + job.ToString() + "\" failed with error: " + _e.Message );
			}
		}
#endif
		}

		#endregion

		#region Synchronous, Asynchronous & Multithreaded Operations

		/// <summary>
		/// Create a fiche from drag'n drop data types.
		/// Depending on the data type, the fiche will be created synchronously or asynchronously...
		/// </summary>
		/// <param name="_data"></param>
		/// <returns></returns>
		public Fiche	CreateFicheFromClipboard( System.Windows.Forms.IDataObject _data ) {
			if ( _data == null )
				throw new Exception( "Invalid data object!" );

			IDataHandler	bestHandler = null;
			int				bestScore = 0;
			string			bestFormat = null;
			foreach ( string format in _data.GetFormats() ) {
				if ( format == null )
					continue;

				string	formatLow = format.ToLower();
				foreach ( IDataHandler handler in m_ficheTypeHandlers ) {
					int	formatScore = handler.FormatScore( formatLow );
					if ( formatScore > bestScore ) {
						// Found a better handler...
						bestHandler = handler;
						bestScore = formatScore;
						bestFormat = format;
					}
				}
			}

			if ( bestHandler == null )
				return null;

// DragContext => System.IO.MemoryStream
// DragImageBits => System.IO.MemoryStream
// text/x-moz-url => System.IO.MemoryStream
// FileGroupDescriptorW => System.IO.MemoryStream
// FileContents => <null>
// UniformResourceLocatorW => System.IO.MemoryStream
// UniformResourceLocator => System.IO.MemoryStream
// System.String => https://stackoverflow.com/questions/36822654/alternative-for-text-x-moz-url-in-chrome-ie-10-edge-in-event-datatransfer
// UnicodeText => https://stackoverflow.com/questions/36822654/alternative-for-text-x-moz-url-in-chrome-ie-10-edge-in-event-datatransfer
// Text => https://stackoverflow.com/questions/36822654/alternative-for-text-x-moz-url-in-chrome-ie-10-edge-in-event-datatransfer

			// Create the fiche using the best possible handler
			object	data = _data.GetData( bestFormat );
			if ( data == null )
				throw new Exception( "Failed to retrieve drop data for format \"" + bestFormat + "\"!" );

			Fiche	fiche = bestHandler.CreateFiche( this, bestFormat, data );
			return fiche;
		}

		/// <summary>
		/// Ask a thread to perform the save asynchronously (we'll be notified on the main thread when the content is saved)
		/// </summary>
		/// <param name="_caller"></param>
		/// <param name="_unloadContentAfterSave">True to unload heavy content after the fiche is saved</param>
		internal void	AsyncSaveFiche( Fiche _caller, bool _unloadContentAfterSave ) {
			// Create a new job and let the loading threads handle it
			WorkingThread.JobSaveFiche	job = new WorkingThread.JobSaveFiche( this, _caller, _unloadContentAfterSave );
			lock ( m_threadedJobs )
				m_threadedJobs.Enqueue( job );
		}

		/// <summary>
		/// Ask a thread to perform the load asynchronously (we'll be notified on the main thread when the content is available)
		/// </summary>
		/// <param name="_caller"></param>
		internal void	AsyncLoadChunk( Fiche.ChunkBase _caller ) {
			// Create a new job and let the loading threads handle it
			WorkingThread.JobLoadChunk	job = new WorkingThread.JobLoadChunk( this, _caller );
			lock ( m_threadedJobs )
				m_threadedJobs.Enqueue( job );
		}

		/// <summary>
		/// Asks a thread to load a web page and render it to an image asynchronously
		/// </summary>
		/// <param name="_URL"></param>
		/// <param name="_delegate"></param>
		internal void	AsyncRenderWebPage( Uri _URL, WebHelpers.WebPageRendered _onSuccess, WebHelpers.WebPageError _onError ) {
			WorkingThread.JobLoadWebPage	job = new WorkingThread.JobLoadWebPage( this, _URL, _onSuccess, _onError );
			lock ( m_threadedJobs )
				m_threadedJobs.Enqueue( job );
		}

		/// <summary>
		/// Ask a thread to fill the content of the fiche asynchronously (we'll be notified on the main thread when the content is available)
		/// </summary>
		/// <param name="_fiche"></param>
		/// <param name="_unloadContentAfterSave">True to unload heavy content after the fiche is saved</param>
		/// <returns>The temporary fiche with only a valid desciptor</returns>
		internal void	AsyncLoadContentAndSaveFiche( Fiche _fiche, bool _unloadContentAfterSave ) {
			// Create a new job and let the threads handle it
			WorkingThread.JobFillFiche	job = new WorkingThread.JobFillFiche( this, _fiche, _unloadContentAfterSave );
			lock ( m_threadedJobs )
				m_threadedJobs.Enqueue( job );
		}

		/// <summary>
		/// Creates a fiche with obly a valid descriptor (usually filled later asynchronouly)
		/// </summary>
		/// <param name="_type"></param>
		/// <param name="_title"></param>
		/// <param name="_URL"></param>
		/// <param name="_HTMLContent">Optional HTML content, will be replaced by actual content for remote URLs</param>
		/// <returns></returns>
		public Fiche	SyncCreateFicheDescriptor( Fiche.TYPE _type, string _title, Uri _URL, Fiche[] _tags, string _HTMLContent ) {
			return new Fiche( this, _type, _title, _URL, _tags, _HTMLContent );
		}

		/// <summary>
		/// Finds or creates the requested tag fiche
		/// </summary>
		/// <param name="_tag"></param>
		/// <returns></returns>
		public Fiche	SyncFindOrCreateTagFiche( string _tag ) {
			Fiche[]	tagFiches = FindFichesByTitle( _tag, false );
			if ( tagFiches.Length > 0 ) {
				return tagFiches[0];	// Arbitrarily use first tag...
			}

			// Create the tag
			return SyncCreateFicheDescriptor( Fiche.TYPE.LOCAL_EDITABLE_WEBPAGE, _tag, null, null, WebHelpers.BuildHTMLDocument( _tag, null ) );
		}

		/// <summary>
		/// Ask the main thread to perform our notification for us
		/// </summary>
		/// <param name="_delegate"></param>
		internal void	SyncNotify( Action _delegate ) {
			WorkingThread.JobNotify	job = new WorkingThread.JobNotify( this, _delegate );
			lock ( m_mainThreadJobs )
				m_mainThreadJobs.Enqueue( job );
		}

		/// <summary>
		/// Ask the main thread to report our error
		/// </summary>
		/// <param name="_error"></param>
		internal void	SyncReportError( string _error ) {
			WorkingThread.JobReportError	job = new WorkingThread.JobReportError( this, _error );
			lock ( m_mainThreadJobs )
				m_mainThreadJobs.Enqueue( job );
		}

		/// <summary>
		/// Internal stream abstraction
		/// At the moment the fiches come from individual files but it could be moved to an actual database or remote server if needed...
		/// NOTE: The caller need to dispose of the stream eventually!
		/// </summary>
		/// <param name="_fiche"></param>
		/// <param name="_readOnly"></param>
		/// <returns></returns>
		internal Stream	SyncRequestFicheStream( Fiche _fiche, bool _readOnly ) {
			FileInfo	ficheFileName = new FileInfo( Path.Combine( m_rootFolder.FullName, _fiche.FileName ) );
			if ( _readOnly )
				return ficheFileName.OpenRead();
			else
				return ficheFileName.Create();
		}

		#endregion

		#region Fiches Update Handlers (don't call these yourself!)

		internal void	FicheGUIDChanged( Fiche _fiche, Guid _formerGUID ) {
			if ( _fiche == null )
				throw new Exception( "Invalid fiche!" );

			// Remove from former position
			if ( m_GUID2Fiche.ContainsKey( _formerGUID ) )
				m_GUID2Fiche.Remove( _formerGUID );

			// Add to new position
			m_GUID2Fiche.Add( _fiche.GUID, _fiche );
		}

		internal void	FicheCreationDateChanged( Fiche _fiche, DateTime _formerDate ) {
			if ( _fiche == null )
				throw new Exception( "Invalid fiche!" );

			// Maybe not useful? Don't know what to do here...
		}

		internal void	FicheTitleChanged( Fiche _fiche, string _formerTitle ) {
			if ( _fiche == null )
				throw new Exception( "Invalid fiche!" );

			// Remove from former lists
			if ( _formerTitle != null ) {
				string	formerTitle = _formerTitle;
				m_titleCaseSensitive2Fiches[formerTitle].Remove( _fiche );

				formerTitle = formerTitle.ToLower();
				m_titleNoCase2Fiches[formerTitle].Remove( _fiche );
				if ( formerTitle.Length >= 1 ) {
					m_t2Fiches[formerTitle.Substring( 0, 1 )].Remove( _fiche );
					if ( formerTitle.Length >= 2 ) {
						m_ti2Fiches[formerTitle.Substring( 0, 2 )].Remove( _fiche );
						if ( formerTitle.Length >= 3 ) {
							m_tit2Fiches[formerTitle.Substring( 0, 3 )].Remove( _fiche );
							if ( formerTitle.Length >= 4 ) {
								m_titl2Fiches[formerTitle.Substring( 0, 4 )].Remove( _fiche );
							}
						}
					}
				}
			}

			// Add to new lists
			string	title = _fiche.Title;
			if ( title != null ) {
				List< Fiche >	fiches;
				if ( !m_titleCaseSensitive2Fiches.TryGetValue( title, out fiches ) )
					m_titleCaseSensitive2Fiches.Add( title, fiches = new List<Fiche>() );
				fiches.Add( _fiche );

				title = title.ToLower();
				if ( !m_titleNoCase2Fiches.TryGetValue( title, out fiches ) )
					m_titleNoCase2Fiches.Add( title, fiches = new List<Fiche>() );
				fiches.Add( _fiche );

				if ( title.Length >= 1 ) {
					string	t = title.Substring( 0, 1 );
					if ( !m_t2Fiches.TryGetValue( t, out fiches ) ) m_t2Fiches.Add( t, fiches = new List<Fiche>() );
					fiches.Add( _fiche );

					if ( title.Length >= 2 ) {
						t += title[1];
						if ( !m_ti2Fiches.TryGetValue( t, out fiches ) ) m_ti2Fiches.Add( t, fiches = new List<Fiche>() );
						fiches.Add( _fiche );

						if ( title.Length >= 3 ) {
							t += title[2];
							if ( !m_tit2Fiches.TryGetValue( t, out fiches ) ) m_tit2Fiches.Add( t, fiches = new List<Fiche>() );
							fiches.Add( _fiche );
						}

						if ( title.Length >= 4 ) {
							t += title[3];
							if ( !m_titl2Fiches.TryGetValue( t, out fiches ) ) m_titl2Fiches.Add( t, fiches = new List<Fiche>() );
							fiches.Add( _fiche );
						}
					}
				}
			}
		}

		internal void	FicheURLChanged( Fiche _fiche, Uri _formerURL ) {
			if ( _fiche == null )
				throw new Exception( "Invalid fiche!" );

			// Remove from former position
			if ( _formerURL != null )
				m_URL2Fiche.Remove( _formerURL );

			// Add to new position
			if ( _fiche.URL != null )
				m_URL2Fiche.Add( _fiche.URL, _fiche );
		}

		internal void	FicheHTMLContentChanged( Fiche _fiche ) {
			if ( _fiche == null )
				throw new Exception( "Invalid fiche!" );

			// Maybe not useful? Don't know what to do here...
		}

// 		/// <summary>
// 		/// Used by fiches to notify of a change so the database should act accordingly (e.g. resaving the updated fiche)
// 		/// </summary>
// 		/// <param name="_fiche"></param>
// 		internal void	FicheUpdated( Fiche _fiche ) {
// 			if ( _fiche == null )
// 				return;
// 
// 			try {
// 				FileInfo	ficheFileName = new FileInfo( _fiche.FileName );
// 				using ( FileStream S = ficheFileName.Create() ) {
// 					using ( BinaryWriter W = new BinaryWriter( S ) ) {
// 						_fiche.Write( W );
// 					}
// 				}
// 
// 			} catch ( Exception _e ) {
// 				BrainForm.Debug( "Failed saving updated fiche \"" + _fiche + "\": " + _e.Message );
// 			}
// 		}

		#endregion

		#region Registration

		internal void	RegisterFiche( Fiche _fiche ) {
			if ( _fiche == null )
				throw new Exception( "Invalid fiche!" );

			if ( m_GUID2Fiche.ContainsKey( _fiche.GUID ) )
				throw new Exception( "A fiche with this GUID is already registered!" );

			m_fiches.Add( _fiche );
			m_GUID2Fiche.Add( _fiche.GUID, _fiche );
			if ( _fiche.URL != null )
				m_URL2Fiche.Add( _fiche.URL, _fiche );
			FicheTitleChanged( _fiche, null );	// Will add the fiche to dictionaries
		}

		internal void	UnRegisterFiche( Fiche _fiche ) {
			if ( _fiche == null )
				throw new Exception( "Invalid fiche!" );

			if ( m_GUID2Fiche.ContainsKey( _fiche.GUID ) )
				throw new Exception( "No fiche with this GUID is registered!" );

			m_fiches.Remove( _fiche );
			m_GUID2Fiche.Remove( _fiche.GUID );
			if ( _fiche.URL != null )
				m_URL2Fiche.Remove( _fiche.URL );
			_fiche.Title = null;	// Will remove the fiche from the dictionaries
		}

		#endregion

		#endregion
	}
}
