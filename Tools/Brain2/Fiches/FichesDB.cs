using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Brain2 {

	/// <summary>
	/// The main fiches database
	/// </summary>
	public class FichesDB : IDisposable {

		#region CONSTANTS

		protected const uint	MAX_WORKING_THREADS_COUNT = 10;

// 		public const uint	SIGNATURE = 0x48434946U;	// 'FICH';
// 		public const ushort	VERSION_MAJOR = 1;
// 		public const ushort	VERSION_MINOR = 0;

		#endregion

		#region NESTED TYPES

		public abstract class JobBase {
			protected FichesDB	m_owner;

			protected JobBase( FichesDB _owner ) {
				m_owner = _owner;
			}

			public abstract void	Run();
		}

		protected class		JobLoadChunk : JobBase {
			public Fiche.ChunkBase	m_caller;
			public JobLoadChunk( FichesDB _owner, Fiche.ChunkBase _caller ) : base( _owner ) { m_caller = _caller; }
			public override void	Run() {
				using ( Stream S = m_owner.RequestFicheStream( m_caller.OwnerFiche, true ) ) {
					m_caller.Threaded_LoadContent( S );
				}
			}
		}

		protected class		JobSaveFiche : JobBase {
			public Fiche	m_caller;
			public JobSaveFiche( FichesDB _owner, Fiche _caller ) : base( _owner ) { m_caller = _caller; }
			public override void	Run() {
				using ( Stream S = m_owner.RequestFicheStream( m_caller, true ) ) {
					using ( BinaryWriter W = new BinaryWriter( S ) ) {
						m_caller.Write( W );
					}
				}
			}
		}

		protected class		JobNotify : JobBase {
			public Action	m_action;
			public JobNotify( FichesDB _owner, Action _action ) : base( _owner ) { m_action = _action; }
			public override void	Run() {
				m_action();
			}
		}

		protected class		JobReportError : JobBase {
			public string	m_error;
			public JobReportError( FichesDB _owner, string _error ) : base( _owner ) { m_error = _error; }
			public override void	Run() {
				m_owner.SyncReportError( m_error );
			}
		}

		protected class WorkingThread : IDisposable {

			#region NESTED TYPES

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
						if ( _this.m_database.m_threadedJobs.Peek() != null ) {
							JobBase	job = _this.m_database.m_threadedJobs.Dequeue();
							job.Run();
						}
					}
				}
			}
		}

		#endregion

		#region FIELDS

		private	DirectoryInfo				m_rootFolder = null;

		private List< Fiche >				m_fiches = new List< Fiche >();
		private Dictionary< Guid, Fiche >	m_ID2Fiche = new Dictionary<Guid, Fiche>();

		protected WorkingThread[]			m_workingThreads = null;					// Asynchronous workers
		protected Queue< JobBase >			m_threadedJobs = new Queue<JobBase>();
		protected Queue< JobBase >			m_mainThreadJobs = new Queue<JobBase>();

		#endregion

		#region PROPERTIES

		#endregion

		#region METHODS

		public	FichesDB() {
			Fiche.ms_database = this;

			// Create working threads
			m_workingThreads = new WorkingThread[MAX_WORKING_THREADS_COUNT];
			for (  uint i=0; i < m_workingThreads.Length; i++ ) {
				m_workingThreads[i] = new WorkingThread( this );
			}
		}

		public void Dispose() {
			for (  uint i=0; i < m_workingThreads.Length; i++ ) {
				m_workingThreads[i].Dispose();	// This will abort the threads
			}
		}

		void	RegisterFiche( Fiche _fiche ) {
			if ( _fiche == null )
				throw new Exception( "Invalid fiche!" );

			if ( m_ID2Fiche.ContainsKey( _fiche.GUID ) )
				throw new Exception( "A fiche with this GUID is already registered!" );

			m_ID2Fiche.Add( _fiche.GUID, _fiche );
			m_fiches.Add( _fiche );
		}


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
			try {
				if ( _rootFolder == null || !_rootFolder.Exists )
					throw new Exception( "Invalid root folder!" );

				m_rootFolder = _rootFolder;

				m_fiches.Clear();
				m_ID2Fiche.Clear();

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
				List< Exception >	errors = new List<Exception>();
				foreach ( Everything.Search.Result result in results ) {
					try {
						if ( !result.IsFile )
							continue;

						// Attempt to read fiche from file
						FileInfo	file = new FileInfo( result.FullName );
						if ( !file.Exists )
							throw new Exception( "Result file \"" + result.PathName + "\" doesn't exist!" );

						Fiche	F = new Fiche();
						try {
							using ( FileStream S = file.OpenRead() ) {
								using ( BinaryReader R = new BinaryReader( S ) ) {
									F.Read( R );
								}
							}
						} catch ( Exception _e ) {
							throw new Exception( "Error reading file \"" + file.FullName + "\"!", _e );
						}

						// Register valid file
						try {
							RegisterFiche( F );
						} catch ( Exception _e ) {
							throw new Exception( "Error registering new fiche from file \"" + file.FullName + "\"!", _e );
						}

					} catch ( Exception _e ) {
						errors.Add( _e );
					}
				}
				if ( errors.Count > 0 )
					throw new Exception( "Some errors occurred while processing Everything results..." );

			} catch ( Exception _e ) {
				throw new Exception( "An error occurred while loading the database: " + _e.Message, _e );
			}
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
					m_mainThreadJobs.Dequeue().Run();
				}
			}
		}

		#endregion

		#region Synchronous, Asynchronous & Multithreaded Operations

		/// <summary>
		/// Ask a thread to perform the load asynchronously (we'll be notified on the main thread when the content is available)
		/// </summary>
		/// <param name="_caller"></param>
		internal void	AsyncLoad( Fiche.ChunkBase _caller ) {
			// Create a new job and let the loading threads handle it
			JobLoadChunk	job = new JobLoadChunk( this, _caller );
			lock ( m_threadedJobs )
				m_threadedJobs.Enqueue( job );
		}

		/// <summary>
		/// Ask a thread to perform the save asynchronously (we'll be notified on the main thread when the content is saved)
		/// </summary>
		/// <param name="_caller"></param>
		internal void	AsyncSave( Fiche _caller ) {
			// Create a new job and let the loading threads handle it
			JobSaveFiche	job = new JobSaveFiche( this, _caller );
			lock ( m_threadedJobs )
				m_threadedJobs.Enqueue( job );
		}

		/// <summary>
		/// Ask the main thread to perform our notification for us
		/// </summary>
		/// <param name="_delegate"></param>
		internal void	SyncNotify( Action _delegate ) {
			JobNotify	job = new JobNotify( this, _delegate );
			lock ( m_mainThreadJobs )
				m_mainThreadJobs.Enqueue( job );
		}

		/// <summary>
		/// Ask the main thread to report our error
		/// </summary>
		/// <param name="_error"></param>
		internal void	SyncReportError( string _error ) {
			JobReportError	job = new JobReportError( this, _error );
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
		internal Stream	RequestFicheStream( Fiche _fiche, bool _readOnly ) {
			FileInfo	ficheFileName = new FileInfo( _fiche.FileName );
			if ( _readOnly )
				return ficheFileName.OpenRead();
			else
				return ficheFileName.Create();
		}

		#endregion


		/// <summary>
		/// Used by fiches to notify of a change so the database should act accordingly (e.g. resaving the updated fiche)
		/// </summary>
		/// <param name="_fiche"></param>
		internal void	FicheUpdated( Fiche _fiche ) {
			if ( _fiche == null )
				return;

			try {
				FileInfo	ficheFileName = new FileInfo( _fiche.FileName );
				using ( FileStream S = ficheFileName.Create() ) {
					using ( BinaryWriter W = new BinaryWriter( S ) ) {
						_fiche.Write( W );
					}
				}

			} catch ( Exception _e ) {
				BrainForm.Debug( "Failed saving updated fiche \"" + _fiche + "\": " + _e.Message );
			}
		}

		#endregion
	}
}
