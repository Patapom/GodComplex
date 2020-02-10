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

		protected const float	DEFAULT_DELAY_BEFORE_SAVE_SECONDS = 5.0f;	// Automatically save modified fiched after 5 seconds

		#endregion

		#region NESTED TYPES

		#region Fiche Types Handlers

		public interface IDataHandler {
			int		FormatScore( string _formatLowerCase );		// Must return 0 if not supported
			Fiche	CreateFiche( FichesDB _database, string _format, object _data );
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
				public uint			m_maxScrollsCount;
				public bool			m_unloadContentAfterSave;
				public JobFillFiche( FichesDB _owner, Fiche _fiche, uint _maxScrollsCount, bool _unloadContentAfterSave ) : base( _owner ) { m_fiche = _fiche; m_maxScrollsCount = _maxScrollsCount; m_unloadContentAfterSave = _unloadContentAfterSave; }
				public override void	Run() {
					// Request the content asynchronously
					m_owner.Async_RenderWebPage( m_fiche.URL, m_maxScrollsCount,

						// On page source & DOM available => Update content
						( string _title, string _HTMLContent, System.Xml.XmlDocument _DOMElements ) => {
							m_fiche.Lock( Fiche.STATUS.UPDATING, () => {
								if ( _title != null && _title != "" )
									m_fiche.Title = _title;
								else if ( m_fiche.URL != null )
									m_fiche.Title = m_fiche.URL.ToString();

								m_fiche.HTMLContent = _HTMLContent;
								m_fiche.DOMElements = DOMElement.FromPageRendererXML( _DOMElements );
							} );
						},

						// On page rendered => Update content
						( uint _imageIndex, ImageUtility.ImageFile _imageWebPage ) => {
							m_fiche.Lock( Fiche.STATUS.UPDATING, () => {

								if ( _imageWebPage.PixelFormat != ImageUtility.PIXEL_FORMAT.BGR8 ) {
									// Make sure it's the proper format for JPEG
									ImageUtility.ImageFile	properImageForJPG = new ImageUtility.ImageFile( _imageWebPage, ImageUtility.PIXEL_FORMAT.BGR8 );
									_imageWebPage.Dispose();
									_imageWebPage = properImageForJPG;
								}

								// Create image chunk
								m_fiche.CreateImageChunk( _imageIndex, new ImageUtility.ImageFile[] { _imageWebPage }, ImageUtility.ImageFile.FILE_FORMAT.JPEG );

								// Create thumbnail
								if ( _imageIndex == 0 ) {
									m_fiche.CreateThumbnailChunkFromImage( _imageWebPage );
								}
							} );
						},

						// On success => Request to save the fiche now that it's complete...
						() => {
							m_owner.Async_SaveFiche( m_fiche, m_unloadContentAfterSave, true );
						},

						// On error => Log error? Todo?
						( WebHelpers.WEB_ERROR_TYPE _error, int _errorCode, string _message ) => {
							m_owner.AsyncMain_ReportFicheStatus( m_fiche, FICHE_REPORT_STATUS.ERROR, "Page failed to load and returned error code " + _errorCode + " with message \"" + _message + "\"!" );
						},

						// Log
						( WebHelpers.LOG_TYPE _type, string _message ) => {
							m_owner.AsyncMain_Log( (LOG_TYPE) _type, _message );
						}
					);
				}
			}

			public class		JobLoadChunk : JobBase {
				public Fiche.ChunkBase	m_caller;
				public Action			m_delegate;
				public JobLoadChunk( FichesDB _owner, Fiche.ChunkBase _caller, Action _delegate ) : base( _owner ) { m_caller = _caller; m_delegate = _delegate; }
				public override void	Run() {
					using ( Stream S = m_owner.SyncRequestFicheStream( m_caller.OwnerFiche, true ) ) {
						S.Position = (long) m_caller.Offset;	// Jump to the chunk's start position

						using ( BinaryReader R = new BinaryReader( S ) ) {
							m_caller.OwnerFiche.Lock( Fiche.STATUS.LOADING, () => {
								m_caller.Threaded_LoadContent( R, true );	// Here this is part of a user request to see the content so we also want to prepare it (e.g. create the images from compressed data)
								m_delegate();	// Notify
							} );
						}
					}
				}
			}

			public class		JobLoadFiche : JobBase {
				public FileInfo		m_ficheFileName;
				public bool			m_unloadContentAfterSave;
				public JobLoadFiche( FichesDB _owner, FileInfo _ficheFileName ) : base( _owner ) { m_ficheFileName = _ficheFileName; }
				public override void	Run() {
					try {
						using ( FileStream S = m_ficheFileName.OpenRead() ) {
							using ( BinaryReader R = new BinaryReader( S ) ) {
								Fiche	F = new Fiche( m_owner, R );
							}
						}
					} catch ( Exception _e ) {
						throw new Exception( "Error reading fiche \"" + m_ficheFileName.FullName + "\"!", _e );
					}
				}
			}

			public class		JobSaveFiche : JobBase {
				public Fiche		m_caller;
				public bool			m_unloadContentAfterSave;
				public bool			m_notifyAfterSave;
				public JobSaveFiche( FichesDB _owner, Fiche _caller, bool _unloadContentAfterSave, bool _notifyAfterSave ) : base( _owner ) { m_caller = _caller; m_unloadContentAfterSave = _unloadContentAfterSave; m_notifyAfterSave = _notifyAfterSave; }
				public override void	Run() {
					m_owner.Sync_SaveFiche( m_caller, m_unloadContentAfterSave, m_notifyAfterSave );
				}
			}

			public class		JobExecute<T> : JobBase {
				public Action<T>	m_action;
				public T			m_userData;

				public JobExecute( FichesDB _owner, Action<T> _action, T _userData ) : base( _owner ) { m_action = _action; m_userData = _userData; }
				public override void	Run() {
					m_action( m_userData );
				}
			}

			public class		JobReportFicheStatus : JobBase {
				public Fiche				m_fiche;
				public FICHE_REPORT_STATUS	m_status;
				public string				m_errorOrWarning;
				public JobReportFicheStatus( FichesDB _owner, Fiche _fiche, FICHE_REPORT_STATUS _status, string _errorOrWarning ) : base( _owner ) { m_fiche = _fiche; m_status = _status; m_errorOrWarning = _errorOrWarning; }
				public override void	Run() {
					switch ( m_status ) {
						case FICHE_REPORT_STATUS.SUCCESS:
							m_owner.FicheSuccessOccurred?.Invoke( m_fiche );
							break;
						case FICHE_REPORT_STATUS.WARNING:
							m_owner.FicheWarningOccurred?.Invoke( m_fiche, m_errorOrWarning );
							break;
						case FICHE_REPORT_STATUS.ERROR:
							m_owner.FicheErrorOccurred?.Invoke( m_fiche, m_errorOrWarning );
							break;
					}
				}
			}

			public class		JobLog : JobBase {
				public LOG_TYPE	m_type;
				public string	m_message;
				public JobLog( FichesDB _owner, LOG_TYPE _type, string _message ) : base( _owner ) { m_type = _type; m_message = _message; }
				public override void	Run() {
					m_owner.Log?.Invoke( m_type, m_message );
				}
			}

			public class		JobLoadWebPage : JobBase {
				public Uri									m_URL;
				public uint									m_maxScrollsCount;
				public WebHelpers.WebPageSourceAvailable	m_delegateSourceAvailable;
				public WebHelpers.WebPagePieceRendered		m_delegatePagePieceRendered;
				public WebHelpers.WebPageSuccess			m_delegateSuccess;
				public WebHelpers.WebPageError				m_delegateError;
				public WebHelpers.Log						m_delegateLog;

				public JobLoadWebPage( FichesDB _owner, Uri _URL, uint _maxScrollsCount, WebHelpers.WebPageSourceAvailable _delegateSourceAvailable, WebHelpers.WebPagePieceRendered _delegatePagePieceRendered, WebHelpers.WebPageSuccess _delegateSuccess, WebHelpers.WebPageError _delegateError, WebHelpers.Log _delegateLog ) : base( _owner ) {
					m_URL = _URL;
					m_maxScrollsCount = _maxScrollsCount;
					m_delegateSourceAvailable = _delegateSourceAvailable;
					m_delegatePagePieceRendered = _delegatePagePieceRendered;
					m_delegateSuccess = _delegateSuccess;
					m_delegateError = _delegateError;
					m_delegateLog = _delegateLog;
				}
				public override void	Run() {
					WebHelpers.LoadWebPage( m_URL, m_maxScrollsCount, m_delegateSourceAvailable, m_delegatePagePieceRendered, m_delegateSuccess, m_delegateError, m_delegateLog );
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
								_this.m_database.AsyncMain_LogError( "A \"" + job.ToString() + "\" failed with error: " + _e.Message );
							}
						}
					}
				}
			}
		}

		#endregion

		public enum FICHE_REPORT_STATUS {
			SUCCESS,
			WARNING,
			ERROR,
		}

		public enum LOG_TYPE {
			INFO,
			WARNING,
			ERROR,
			DEBUG
		}


		/// <summary>
		/// Whenever a fiche is modified, it should call FicheDB.SyncNotifyFicheModifiedAndNeedsSaving() so a timer is started and the fiche gets automatically saved when the timer expires
		/// </summary>
		class FicheUpdatedNeedsSaving {
			public DateTime	m_timeLastModified;	// The time when the file was last accessed
			public Fiche	m_fiche;			// The fiche that needs saving after having been updated
			public FicheUpdatedNeedsSaving( Fiche _fiche ) {
				m_fiche = _fiche;
			}
		}

		public class DatabaseLoadException : Exception {
			public Exception[]	m_errors;
			public DatabaseLoadException( Exception[] _errors ) {
				m_errors = _errors;
			}
		}

		public delegate void	FicheEventHandler( Fiche _fiche );
		public delegate void	FicheErrorOrWarningHandler( Fiche _fiche, string _errorOrWarning );
		public delegate void	DatabaseErrorHandler( string _error );
		public delegate void	DatabaseLogHandler( LOG_TYPE _type, string _message );

		#endregion

		#region FIELDS

		private	DirectoryInfo						m_rootFolder = null;

		// Fiches and referencing structures
		private List< Fiche >						m_fiches = new List< Fiche >();
		private Dictionary< Guid, Fiche >			m_GUID2Fiche = new Dictionary<Guid, Fiche>();
		private Dictionary< Uri, List< Fiche > >	m_URL2Fiches = new Dictionary<Uri, List<Fiche>>();

		private Dictionary< Guid, List< Fiche > >				m_GUID2FichesRequiringTag = new Dictionary<Guid, List<Fiche>>();		// The list of fiches requiring the key GUID as a tag (polled each time a new fiche is registered)
		private Dictionary< Fiche, FicheUpdatedNeedsSaving >	m_fiche2NeedToSave = new Dictionary<Fiche, FicheUpdatedNeedsSaving>();	// The map of fiches that were accessed and should be saved ASAP

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

		public event DatabaseLogHandler			Log;				// Occurs whenever a log event occurred

		public event FicheEventHandler			FicheSuccessOccurred;	// Occurs whenever a fiche success occurred
		public event FicheErrorOrWarningHandler	FicheWarningOccurred;	// Occurs whenever a fiche warning occurred
		public event FicheErrorOrWarningHandler	FicheErrorOccurred;		// Occurs whenever a fiche error occurred and fiche should be deleted

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

			// Save remaining modified fiches now
			lock ( m_fiche2NeedToSave ) {
				foreach ( FicheUpdatedNeedsSaving value in m_fiche2NeedToSave.Values ) {
					Sync_SaveFiche( value.m_fiche, false, false );
				}
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

		public Fiche[]	FindFichesByURL( Uri _URL ) {
			if ( _URL == null )
				throw new Exception( "Invalid URL!" );

			List< Fiche >	result = null;
			m_URL2Fiches.TryGetValue( _URL, out result );
			return result != null ? result.ToArray() : null;
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
		public void	FindNearestTagMatches( string _tentativeName, Fiche[] _excludedTags, List< Fiche > _matches ) {
			if ( _tentativeName == null )
				throw new Exception( "Invalid title!" );
			_tentativeName = _tentativeName.ToLower();

			HashSet< Fiche >	uniqueMatches = new HashSet<Fiche>();
			HashSet< Fiche >	excludedTags = new HashSet<Fiche>();
			if ( _excludedTags != null ) {
				foreach ( Fiche excludedTag in _excludedTags ) {
					excludedTags.Add( excludedTag );
				}
			}

			// List exact matches first
			List<Fiche>			results = null;
			switch ( _tentativeName.Length ) {
				case 0: return;	// Can't process
				case 1:
					m_t2Fiches.TryGetValue( _tentativeName, out results );
					break;
				case 2:
					m_ti2Fiches.TryGetValue( _tentativeName, out results );
					break;
				case 3:
					m_tit2Fiches.TryGetValue( _tentativeName, out results );
					break;
				default:	// 4 characters and more...
					m_titl2Fiches.TryGetValue( _tentativeName, out results );
					break;
			}
			if ( results != null ) {
				foreach ( Fiche result in results ) {
					if ( !excludedTags.Contains( result ) ) {
						uniqueMatches.Add( result );
					}
				}
			}

			// List approximate results
			if ( _tentativeName.Length > 4 ) {

			}

			// Sort results by references count so most used tags are listed first
			_matches.AddRange( uniqueMatches );
			_matches.Sort( ( Fiche x, Fiche y ) => { return x.ReferencesCount > y.ReferencesCount ? -1 : (x.ReferencesCount < y.ReferencesCount ? 1 : 0); } );
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
						Async_LoadAndRegisterFiche( file );

					} catch ( Exception _e ) {
						errors.Add( _e );
					}
				}

			} catch ( Exception _e ) {
				errors.Add( _e );
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
			// Process main thread jobs
			lock ( m_mainThreadJobs ) {
				while ( m_mainThreadJobs.Count > 0 ) {
					WorkingThread.JobBase	job = m_mainThreadJobs.Dequeue();
					try {
						job.Run();
					} catch ( Exception _e ) {
						AsyncMain_LogError( "A \"" + job.ToString() + "\" failed with error: " + _e.Message );
					}
				}
			}

			// Process timed saving
			lock ( m_fiche2NeedToSave ) {
				DateTime	now = DateTime.Now;
				foreach ( FicheUpdatedNeedsSaving value in m_fiche2NeedToSave.Values ) {
					float	timeSinceLastModification = (float) (now - value.m_timeLastModified).TotalSeconds;
					if ( timeSinceLastModification > DEFAULT_DELAY_BEFORE_SAVE_SECONDS ) {
						// Okay, enough time has passed, we can asynchronously save the fiche now...
						m_fiche2NeedToSave.Remove( value.m_fiche );
						Async_SaveFiche( value.m_fiche, true, false );
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
		public Fiche	Sync_CreateFicheFromClipboard( System.Windows.Forms.IDataObject _data ) {
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

		internal void	Async_LoadAndRegisterFiche( FileInfo _ficheFileName ) {
			// Create a new job and let the threads handle it
			WorkingThread.JobLoadFiche	job = new WorkingThread.JobLoadFiche( this, _ficheFileName );
			lock ( m_threadedJobs )
				m_threadedJobs.Enqueue( job );
		}

		/// <summary>
		/// Ask a thread to perform the save asynchronously (we'll be notified on the main thread when the content is saved)
		/// </summary>
		/// <param name="_caller"></param>
		/// <param name="_unloadContentAfterSave">True to unload heavy content after the fiche is saved</param>
		internal void	Async_SaveFiche( Fiche _fiche, bool _unloadContentAfterSave, bool _notifyAfterSave ) {
			// Create a new job and let the threads handle it
			WorkingThread.JobSaveFiche	job = new WorkingThread.JobSaveFiche( this, _fiche, _unloadContentAfterSave, _notifyAfterSave );
			lock ( m_threadedJobs )
				m_threadedJobs.Enqueue( job );
		}

		/// <summary>
		/// Ask a thread to perform the load asynchronously (we'll be notified on the main thread when the content is available)
		/// </summary>
		/// <param name="_caller"></param>
		/// <param name="_delegate">Called once the chunk has been loaded</param>
		internal void	Async_LoadChunk( Fiche.ChunkBase _caller, Action _delegate ) {
			// Create a new job and let the loading threads handle it
			WorkingThread.JobLoadChunk	job = new WorkingThread.JobLoadChunk( this, _caller, _delegate );
			lock ( m_threadedJobs )
				m_threadedJobs.Enqueue( job );
		}

		/// <summary>
		/// Asks a thread to load a web page and render it to an image asynchronously
		/// </summary>
		/// <param name="_URL"></param>
		/// <param name="_delegate"></param>
		internal void	Async_RenderWebPage( Uri _URL, uint _maxScrollsCount, WebHelpers.WebPageSourceAvailable _onSourceAvailable, WebHelpers.WebPagePieceRendered _onPagePieceRendered, WebHelpers.WebPageSuccess _onSuccess, WebHelpers.WebPageError _onError, WebHelpers.Log _log ) {
			WorkingThread.JobLoadWebPage	job = new WorkingThread.JobLoadWebPage( this, _URL, _maxScrollsCount, _onSourceAvailable, _onPagePieceRendered, _onSuccess, _onError, _log );
			lock ( m_threadedJobs )
				m_threadedJobs.Enqueue( job );
		}

		/// <summary>
		/// Ask a thread to fill the content of the fiche asynchronously (we'll be notified on the main thread when the content is available)
		/// </summary>
		/// <param name="_fiche"></param>
		/// <param name="_maxScrollsCount">Amount of allowed scrolls to capture the entire page</param>
		/// <param name="_unloadContentAfterSave">True to unload heavy content after the fiche is saved</param>
		/// <returns>The temporary fiche with only a valid desciptor</returns>
		internal void	Async_LoadContentAndSaveFiche( Fiche _fiche, uint _maxScrollsCount, bool _unloadContentAfterSave ) {
			// Create a new job and let the threads handle it
			WorkingThread.JobFillFiche	job = new WorkingThread.JobFillFiche( this, _fiche, _maxScrollsCount, _unloadContentAfterSave );
			lock ( m_threadedJobs )
				m_threadedJobs.Enqueue( job );
		}

		/// <summary>
		/// Creates a fiche with only a valid descriptor (usually filled later asynchronouly)
		/// </summary>
		/// <param name="_type"></param>
		/// <param name="_title"></param>
		/// <param name="_URL"></param>
		/// <param name="_HTMLContent">Optional HTML content, will be replaced by actual content for remote URLs</param>
		/// <returns></returns>
		public Fiche	Sync_CreateFicheDescriptor( Fiche.TYPE _type, string _title, Uri _URL, Fiche[] _tags, string _HTMLContent ) {
			return new Fiche( this, _type, _title, _URL, _tags, _HTMLContent );
		}

		/// <summary>
		/// Finds or creates the requested tag fiche
		/// </summary>
		/// <param name="_tag"></param>
		/// <returns></returns>
		public Fiche	Sync_FindOrCreateTagFiche( string _tag ) {
			Fiche[]	tagFiches = FindFichesByTitle( _tag, false );
			if ( tagFiches.Length > 0 ) {
				return tagFiches[0];	// Arbitrarily use first tag...
			}

			// Create the tag
			return Sync_CreateFicheDescriptor( Fiche.TYPE.LOCAL_EDITABLE_WEBPAGE, _tag, new Uri( "tag://" + _tag ), null, WebHelpers.BuildHTMLDocument( _tag, null ) );
		}

		/// <summary>
		/// Ask the main thread to save the fiche whenever possible after some time has elapsed
		/// The idea is to notify often, but save only once the fiche is left alone for some time...
		/// </summary>
		/// <param name="_fiche"></param>
		internal void	Async_NotifyFicheModifiedAndNeedsAsyncSaving( Fiche _fiche ) {
			lock ( m_fiche2NeedToSave ) {
				FicheUpdatedNeedsSaving	saveTimer = null;
				if ( !m_fiche2NeedToSave.TryGetValue( _fiche, out saveTimer ) )
					m_fiche2NeedToSave.Add( _fiche, saveTimer = new FicheUpdatedNeedsSaving( _fiche ) );

				saveTimer.m_timeLastModified = DateTime.Now;	// Reset timer
			}
		}

		/// <summary>
		/// Perform the save immediately
		/// </summary>
		/// <param name="_fiche"></param>
		/// <param name="_unloadContentAfterSave">True to unload heavy content after the fiche is saved</param>
		/// <param name="_notifyAfterSave">True to send a success notification after the fiche is saved</param>
		internal void	Sync_SaveFiche( Fiche _fiche, bool _unloadContentAfterSave, bool _notifyAfterSave ) {

			// If the file already exists then notify the fiche it's its last chance to read data from the old file before it's overwritten!
			if ( Sync_FicheStreamAlreadyExists( _fiche ) ) {
				using ( Stream S = SyncRequestFicheStream( _fiche, true ) ) {
					using ( BinaryReader R = new BinaryReader( S ) ) {
						_fiche.LastChanceReadBeforeWrite( R );
					}
				}
			}

			// Now we can save the fiche peacefully
			using ( Stream S = SyncRequestFicheStream( _fiche, false ) ) {
				using ( BinaryWriter W = new BinaryWriter( S ) ) {
					// Ensure the fiche is ready so it can be saved
					if ( _fiche.Status != Fiche.STATUS.READY )
						throw new Exception( "Can't save while fiche is not ready!" );

					_fiche.Lock( Fiche.STATUS.SAVING, () => {
						_fiche.Write( W );

						// Unload heavy content if requested...
						if ( _unloadContentAfterSave )
							_fiche.UnloadImageChunk();

						// Optional notification
						if ( _notifyAfterSave ) {
							AsyncMain_ReportFicheStatus( _fiche, FICHE_REPORT_STATUS.SUCCESS, null );
						}
					} );
				}
			}
		}

		/// <summary>
		/// Asks the main thread to execute a delegate for us
		/// </summary>
		/// <param name="_delegate">The action to execute on the main thread</param>
		/// <param name="_userData"></param>
		internal void	AsyncMain_Execute<T>( Action<T> _delegate, T _userData ) {
			WorkingThread.JobExecute<T>	job = new WorkingThread.JobExecute<T>( this, _delegate, _userData );
			lock ( m_mainThreadJobs )
				m_mainThreadJobs.Enqueue( job );
		}

		/// <summary>
		/// Ask the main thread to report a fiche success (probably after a save)
		/// Use this to report status regarding fiche content that the user needs to know about, e.g.
		///		• The fiche was successfully created and saved
		///		• The web page failed to load and the fiche should probably be deleted
		/// </summary>
		/// <param name="_fiche">The fiche to report about</param>
		/// <param name="_status">The type of report</param>
		/// <param name="_report"></param>
		internal void	AsyncMain_ReportFicheStatus( Fiche _fiche, FICHE_REPORT_STATUS _status, string _report ) {
			WorkingThread.JobReportFicheStatus	job = new WorkingThread.JobReportFicheStatus( this, _fiche, _status, _report );
			lock ( m_mainThreadJobs )
				m_mainThreadJobs.Enqueue( job );
		}

		/// <summary>
		/// Asks the main thread to log the message for us
		/// </summary>
		/// <param name="_type"></param>
		/// <param name="_message"></param>
		internal void	AsyncMain_Log( LOG_TYPE _type, string _message ) {
			WorkingThread.JobLog	job = new WorkingThread.JobLog( this, _type, _message );
			lock ( m_mainThreadJobs )
				m_mainThreadJobs.Enqueue( job );
		}

		internal void	AsyncMain_LogError( string _message ) {
			AsyncMain_Log( LOG_TYPE.ERROR, _message );
		}

		/// <summary>
		/// Immediately returns the fiche's file name
		/// </summary>
		/// <param name="_fiche">The fiche to create a file stream for</param>
		/// <returns></returns>
		private FileInfo	Sync_GetFicheFileName( Fiche _fiche ) {
			FileInfo	ficheFileName = new FileInfo( Path.Combine( m_rootFolder.FullName, _fiche.FileName ) );
			return ficheFileName;
		}

		/// <summary>
		/// Tells if the fiche stream already exists
		/// </summary>
		/// <param name="_fiche">The fiche to check stream existence for</param>
		/// <returns>True if the stream is already available, false if the fiche needs to be created</returns>
		internal bool	Sync_FicheStreamAlreadyExists( Fiche _fiche ) {
			FileInfo	ficheFileName = Sync_GetFicheFileName( _fiche );
			return ficheFileName.Exists;
		}

		/// <summary>
		/// Internal stream abstraction
		/// At the moment the fiches come from individual files but it could be moved to an actual database or remote server if needed...
		/// NOTE: The caller need to dispose of the stream eventually!
		/// </summary>
		/// <param name="_fiche">The fiche to create a file stream for</param>
		/// <param name="_read">True for a read operation, false for a write operation</param>
		/// <returns></returns>
		internal Stream	SyncRequestFicheStream( Fiche _fiche, bool _read ) {
			FileInfo	ficheFileName = Sync_GetFicheFileName( _fiche );
			if ( _read ) {
				return ficheFileName.OpenRead();
			} else {
				return ficheFileName.Create();
			}
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
			if ( _formerURL != null ) {
				m_URL2Fiches[_formerURL].Remove( _fiche );
			}

			// Add to new position
			if ( _fiche.URL != null ) {
				List< Fiche >	fiches = null;
				if ( !m_URL2Fiches.TryGetValue( _fiche.URL, out fiches ) ) {
					m_URL2Fiches.Add( _fiche.URL, fiches = new List<Fiche>() );
				}
				fiches.Add( _fiche );
			}
		}

		internal void	FicheHTMLContentChanged( Fiche _fiche ) {
			if ( _fiche == null )
				throw new Exception( "Invalid fiche!" );

			// Maybe not useful? Don't know what to do here...
		}

		internal void	FicheDOMElementsChanged( Fiche _fiche ) {
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
			if ( _fiche.URL != null ) {
				FicheURLChanged( _fiche, null );
			}
			FicheTitleChanged( _fiche, null );	// Will add the fiche to dictionaries

			lock ( m_GUID2FichesRequiringTag ) {
				// Attempt to register the fiche's tags
				List< Fiche >	fichesTaggedWithNewFiche = null;
				if ( _fiche.m_tagGUIDs != null ) {
					foreach ( Guid tagGUID in _fiche.m_tagGUIDs ) {
						Fiche	tag = null;
						if ( m_GUID2Fiche.TryGetValue( tagGUID, out tag ) ) {
							_fiche.ResolveTag( tag );	// The tag already exists, resolve it now...
						} else {
							// The fiche isn't available yet, register ourselves as a requester...
							if ( !m_GUID2FichesRequiringTag.TryGetValue( tagGUID, out fichesTaggedWithNewFiche ) ) {
								m_GUID2FichesRequiringTag.Add( tagGUID, fichesTaggedWithNewFiche = new List<Fiche>() );
							}
							fichesTaggedWithNewFiche.Add( _fiche );
						}
					}
				}

				// Resolve fiches waiting for this fiche as a tag
				fichesTaggedWithNewFiche = null;
				if ( m_GUID2FichesRequiringTag.TryGetValue( _fiche.GUID, out fichesTaggedWithNewFiche ) ) {
					// Resolve all fiches requiring the new fiche
					foreach ( Fiche resolvableFiche in fichesTaggedWithNewFiche ) {
						resolvableFiche.ResolveTag( _fiche );
					}
					m_GUID2FichesRequiringTag.Remove( _fiche.GUID );	// We're done with that tag now!
				}
			}
		}

		internal void	UnRegisterFiche( Fiche _fiche ) {
			if ( _fiche == null )
				throw new Exception( "Invalid fiche!" );

			if ( m_GUID2Fiche.ContainsKey( _fiche.GUID ) )
				throw new Exception( "No fiche with this GUID is registered!" );

			m_fiches.Remove( _fiche );
			m_GUID2Fiche.Remove( _fiche.GUID );
			if ( _fiche.URL != null ) {
				m_URL2Fiches[_fiche.URL].Remove( _fiche );
			}
			_fiche.Title = null;	// Will remove the fiche from the dictionaries
		}

		#endregion

		#endregion
	}
}
