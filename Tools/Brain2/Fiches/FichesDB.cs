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
				string	HTML = Fiche.BuildHTMLDocument( title, text );
				return _database.AsyncCreateURLFiche( Fiche.TYPE.LOCAL_EDITABLE_WEBPAGE, text, null, HTML );
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
						throw new Exception( "Expected an even size! Apparently not dealing with a unicode string..." );
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

				return _database.AsyncCreateURLFiche( Fiche.TYPE.REMOTE_ANNOTABLE_WEBPAGE, title, URL, null );
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

//				string	content = "TODO! " + _URL;

// 				string	content = "<blockquote class=\"twitter-tweet\"><p lang=\"en\" dir=\"ltr\">Appropriate for December. <a href=\"https://t.co/dzNBXmcreS\">pic.twitter.com/dzNBXmcreS</a></p>&mdash; In Otter News (@Otter_News)" +
// 				"<a href=\"https://twitter.com/Otter_News/status/1204733982149160960?ref_src=twsrc%5Etfw\">December 11, 2019</a></blockquote> <script async src=\"https://platform.twitter.com/widgets.js\" charset=\"utf-8\"></script>";

/*string	content = @"<blockquote class=""Tweet h-entry js-tweetIdInfo subject expanded"" cite=""https://twitter.com/Poulin2012/status/1204065818432167937"" data-tweet-id=""1204065818432167937"" data-scribe=""section:subject"">
    <div class=""Tweet-header"">
      <a class=""TweetAuthor-avatar  Identity-avatar u-linkBlend"" data-scribe=""element:user_link"" href=""https://twitter.com/Poulin2012"" aria-label=""Alexis Poulin (nom d'utilisateur : Poulin2012)""><img class=""Avatar"" data-scribe=""element:avatar"" data-src-2x=""https://pbs.twimg.com/profile_images/1110858983357771778/aTqqWLFY_bigger.jpg"" alt="""" data-src-1x=""https://pbs.twimg.com/profile_images/1110858983357771778/aTqqWLFY_normal.jpg"" src=""https://pbs.twimg.com/profile_images/1110858983357771778/aTqqWLFY_normal.jpg""></a>

      


<div class=""TweetAuthor js-inViewportScribingTarget"" data-scribe=""component:author"">
  <a class=""TweetAuthor-link Identity u-linkBlend"" data-scribe=""element:user_link"" href=""https://twitter.com/Poulin2012"" aria-label=""Alexis Poulin (nom d'utilisateur : Poulin2012)"">
    <div class=""TweetAuthor-nameScreenNameContainer"">
      <span class=""TweetAuthor-decoratedName"">
        <span class=""TweetAuthor-name Identity-name customisable-highlight"" title=""Alexis Poulin"" data-scribe=""element:name"">Alexis Poulin</span>
        <span class=""TweetAuthor-verifiedBadge"" data-scribe=""element:verified_badge""><div class=""Icon Icon--verified "" aria-label=""Compte certifié"" title=""Compte certifié"" role=""img""></div>
<b class=""u-hiddenVisually"">✔</b></span>
      </span>
      <span class=""TweetAuthor-screenName Identity-screenName"" title=""@Poulin2012"" data-scribe=""element:screen_name"" dir=""ltr"">@Poulin2012</span>
    </div>
  </a>
</div>

        <div class=""Tweet-brand"">
          <a href=""https://twitter.com/Poulin2012/status/1204065818432167937"" data-scribe=""element:logo""><span class=""FollowButton-bird""><div class=""Icon Icon--twitter "" aria-label=""Voir sur Twitter"" title=""Voir sur Twitter"" role=""presentation""></div>
</span></a>
        </div>
    </div>
    <div class=""Tweet-body e-entry-content"" data-scribe=""component:tweet"">
      
      <div class=""Tweet-target js-inViewportScribingTarget""></div>
      <p class=""Tweet-text e-entry-title"" lang=""fr"" dir=""ltr"">A propos de «&nbsp;la haine&nbsp;» qui visiblement en novlangue veut dire «&nbsp;capter des images de violences policières&nbsp;»... <a href=""https://t.co/6W9zbPCuCG"" rel=""nofollow noopener"" dir=""ltr"" data-expanded-url=""https://twitter.com/davduf/status/1204059641413586946"" class=""link customisable"" target=""_blank"" title=""https://twitter.com/davduf/status/1204059641413586946"" data-tweet-id=""1204059641413586946"" data-tweet-item-type=""23"" data-scribe=""element:url""><span class=""u-hiddenVisually"">https://</span>twitter.com/davduf/status/<span class=""u-hiddenVisually"">1204059641413586946&nbsp;</span>…</a></p>



        <div class=""Tweet-card"">
<div class=""QuoteTweet"" tabindex=""0"" data-scribe=""section:quote"">
  <a class=""QuoteTweet-link"" data-tweet-id=""1204059641413586946"" data-tweet-item-type=""23"" href=""https://twitter.com/davduf/status/1204059641413586946"" target=""_blank"" rel=""noopener"">
    <div class=""QuoteTweet-nonMediaContainer"">
      


<div class=""TweetAuthor js-inViewportScribingTarget TweetAuthor--oneLine"" data-scribe=""component:author"">
  
    <div class=""TweetAuthor-nameScreenNameContainer"">
      <span class=""TweetAuthor-decoratedName"">
        <span class=""TweetAuthor-name Identity-name customisable-highlight"" title=""David Dufresne"" data-scribe=""element:name"">David Dufresne</span>
        <span class=""TweetAuthor-verifiedBadge"" data-scribe=""element:verified_badge""><div class=""Icon Icon--verified "" aria-label=""Compte certifié"" title=""Compte certifié"" role=""img""></div>
<b class=""u-hiddenVisually"">✔</b></span>
      </span>
      <span class=""TweetAuthor-screenName Identity-screenName"" title=""@davduf"" data-scribe=""element:screen_name"" dir=""ltr"">@davduf</span>
    </div>
  
</div>

      <div></div>
      <p class=""QuoteTweet-text e-entry-title"" lang=""fr"" dir=""ltr"">Le sénateur Grand, profitant de la proposition de loi de « Lutte contre la haine »  sur internet (PPL), propose une amande de 15 000 € pour captation d'image de policiers. <span class=""PrettyLink-prefix"">#</span><span class=""PrettyLink-value"">ViolencesPolicières</span> <span class=""PrettyLink-prefix"">#</span><span class=""PrettyLink-value"">LibertédInformer</span><br><br>Source: <span class=""u-hiddenVisually"">http://www.</span>senat.fr/amendements/co<span class=""u-hiddenVisually"">mmissions/2018-2019/645/Amdt_COM-13.html&nbsp;</span>…</p>
    </div>
    <div class=""QuotedTweet-media"">
  
<article class=""MediaCard
           
           customisable-border"" data-scribe=""component:card"" dir=""ltr"">
  <div class=""MediaCard-media"" data-scribe=""element:photo"">

    <div class=""MediaCard-widthConstraint js-cspForcedStyle"" style=""max-width: 1200px"" data-style=""max-width: 1200px"">
      <div class=""MediaCard-mediaContainer js-cspForcedStyle MediaCard--roundedBottom"" style=""padding-bottom: 68.9167%"" data-style=""padding-bottom: 68.9167%"">
        <div class=""MediaCard-mediaAsset NaturalImage"">
          <img class=""NaturalImage-image"" data-image=""https://pbs.twimg.com/media/ELWt20_W4AAea8q"" data-image-format=""png"" width=""1200"" height=""827"" title=""Voir l'image sur Twitter"" alt=""Voir l'image sur Twitter"" src=""https://pbs.twimg.com/media/ELWt20_W4AAea8q?format=png&amp;name=small"">
        </div>
      </div>
    </div>
  </div>
</article>

  
  
</div>
  </a>
</div>
</div>


      <div class=""TweetInfo"">
        <div class=""TweetInfo-like"">
<a class=""TweetInfo-heart"" title=""J'aime"" href=""https://twitter.com/intent/like?tweet_id=1204065818432167937"" data-scribe=""component:actions"">
  <div data-scribe=""element:heart""><div class=""Icon Icon--heart "" aria-label=""J'aime"" title=""J'aime"" role=""img""></div>
</div>
  <span class=""TweetInfo-heartStat"" data-scribe=""element:heart_count"">704</span>
</a>
</div>
        <div class=""TweetInfo-timeGeo"">

<a class=""u-linkBlend u-url customisable-highlight long-permalink"" data-datetime=""2019-12-09T15:50:36+0000"" data-scribe=""element:full_timestamp"" href=""https://twitter.com/Poulin2012/status/1204065818432167937"">








<time class=""dt-updated"" datetime=""2019-12-09T15:50:36+0000"" pubdate="""" title=""Heure de publication : 09 décembre 2019 15:50:36 (UTC)"">10:50 - 9 déc. 2019</time></a></div>
        <div class=""tweet-InformationCircle"" data-scribe=""element:notice""><a href=""https://support.twitter.com/articles/20175256"" class=""Icon Icon--informationCircleWhite js-inViewportScribingTarget"" title=""Informations sur les Publicités Twitter et confidentialité""><span class=""u-hiddenVisually"">Informations sur les Publicités Twitter et confidentialité</span></a>
</div>
      </div>
    </div>
  </blockquote>";
//*/
//				string	title = "dummy title";

//				return new Fiche( _title, _URL, null, Fiche.BuildHTMLDocument( title, content ) );
				return new Fiche( _database, Fiche.TYPE.REMOTE_ANNOTABLE_WEBPAGE, _title, _URL, null, null );
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
				public JobFillFiche( FichesDB _owner, Fiche _fiche ) : base( _owner ) { m_fiche = _fiche; }
				public override void	Run() {
					// Request the content asynchronously
					m_owner.AsyncLoadWebPage( m_fiche.URL, ( Fiche _fiche, string _HTMLContent, ImageUtility.ImageFile _imageWebPage ) => {
						F.HTMLContent = _HTMLContent;

						// Create thumbnail
						F.CreateThumbnailChunk( _imageWebPage );

						// Create image chunk
						F.CreateImageChunk( _imageWebPage );
					} );

				}
			}

			public class		JobLoadChunk : JobBase {
				public Fiche.ChunkBase	m_caller;
				public JobLoadChunk( FichesDB _owner, Fiche.ChunkBase _caller ) : base( _owner ) { m_caller = _caller; }
				public override void	Run() {
					using ( Stream S = m_owner.RequestFicheStream( m_caller.OwnerFiche, true ) ) {
						m_caller.Threaded_LoadContent( S );
					}
				}
			}

			public class		JobSaveFiche : JobBase {
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

		#endregion

		#region FIELDS

		private	DirectoryInfo						m_rootFolder = null;

		private List< Fiche >						m_fiches = new List< Fiche >();
		private Dictionary< Guid, Fiche >			m_GUID2Fiche = new Dictionary<Guid, Fiche>();
		private Dictionary< string, List< Fiche > >	m_titleCaseSensitive2Fiches = new Dictionary<string, List<Fiche>>();
		private Dictionary< string, List< Fiche > >	m_titleNoCase2Fiches = new Dictionary<string, List<Fiche>>();
		private Dictionary< Uri, Fiche >			m_URL2Fiche = new Dictionary<Uri, Fiche>();

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
			m_workingThreads = new WorkingThread[MAX_WORKING_THREADS_COUNT];
			for (  uint i=0; i < m_workingThreads.Length; i++ ) {
				m_workingThreads[i] = new WorkingThread( this );
			}
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

// @TODO: Find content, by tag, 

		#endregion

		#region Fiches Creation

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
			try {
				if ( _rootFolder == null || !_rootFolder.Exists )
					throw new Exception( "Invalid root folder!" );

				m_rootFolder = _rootFolder;

				m_fiches.Clear();
				m_GUID2Fiche.Clear();

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

						FileInfo	file = new FileInfo( result.FullName );
						if ( !file.Exists )
							throw new Exception( "Result file \"" + result.PathName + "\" doesn't exist!" );

						// Attempt to read fiche from file
						Fiche	F = null;
						try {
							using ( FileStream S = file.OpenRead() ) {
								using ( BinaryReader R = new BinaryReader( S ) ) {
									F = new Fiche( this, R );
								}
							}
						} catch ( Exception _e ) {
							throw new Exception( "Error reading file \"" + file.FullName + "\"!", _e );
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
		/// Ask a thread to perform the creation asynchronously (we'll be notified on the main thread when the content is available)
		/// </summary>
		/// <param name="_type"></param>
		/// <param name="_title"></param>
		/// <param name="_URL"></param>
		/// <param name="_HTMLContent">Optional HTML content, will be replaced by actual content for remote URLs</param>
		/// <returns>The temporary fiche with only a valid desciptor</returns>
		internal Fiche	AsyncCreateURLFiche( Fiche.TYPE _type, string _title, Uri _URL, string _HTMLContent ) {
			Fiche	F = SyncCreateFicheDescriptor( _type, _title, _URL, _HTMLContent );

			// Create a new job and let the threads handle it
			WorkingThread.JobFillFiche	job = new WorkingThread.JobFillFiche( this, F );
			lock ( m_threadedJobs )
				m_threadedJobs.Enqueue( job );

			// Return the descriptor
			return F;
		}

		/// <summary>
		/// Creates a fiche with obly a valid descriptor (usually filled later asynchronouly)
		/// </summary>
		/// <param name="_type"></param>
		/// <param name="_title"></param>
		/// <param name="_URL"></param>
		/// <param name="_HTMLContent">Optional HTML content, will be replaced by actual content for remote URLs</param>
		/// <returns></returns>
		public Fiche	SyncCreateFicheDescriptor( Fiche.TYPE _type, string _title, Uri _URL, string _HTMLContent ) {
			Fiche	F = new Fiche( this, _type, _title, _URL, null, _HTMLContent );
			return F;
		}

		/// <summary>
		/// Create a fiche from drag'n drop data types.
		/// Depending on the data type, the fiche will be created synchronously or asynchronously...
		/// </summary>
		/// <param name="_data"></param>
		/// <returns></returns>
		public Fiche	CreateFiche( System.Windows.Forms.IDataObject _data ) {
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

			// Create the fiche using the best possible handler
			object	data = _data.GetData( bestFormat );
			if ( data == null )
				throw new Exception( "Failed to retrieve drop data for format \"" + bestFormat + "\"!" );

			Fiche	fiche = bestHandler.CreateFiche( this, bestFormat, data );
			return fiche;
		}

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
			return new Fiche( this, Fiche.TYPE.LOCAL_EDITABLE_WEBPAGE, _tag, null, null, Fiche.BuildHTMLDocument( _tag, null ) );
		}

		/// <summary>
		/// Ask a thread to perform the save asynchronously (we'll be notified on the main thread when the content is saved)
		/// </summary>
		/// <param name="_caller"></param>
		internal void	AsyncSave( Fiche _caller ) {
			// Create a new job and let the loading threads handle it
			WorkingThread.JobSaveFiche	job = new WorkingThread.JobSaveFiche( this, _caller );
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
		internal Stream	RequestFicheStream( Fiche _fiche, bool _readOnly ) {
			FileInfo	ficheFileName = new FileInfo( _fiche.FileName );
			if ( _readOnly )
				return ficheFileName.OpenRead();
			else
				return ficheFileName.Create();
		}

		#endregion

		#region Fiches Update Handlers

		internal void	FicheGUIDChanged( Fiche _fiche, Guid _formerGUID ) {
			if ( _fiche == null )
				throw new Exception( "Invalid fiche!" );

			// Remove from former position
			if ( m_GUID2Fiche.ContainsKey( _formerGUID ) )
				m_GUID2Fiche.Remove( _formerGUID );

			// Add to new position
			m_GUID2Fiche.Add( _fiche.GUID, _fiche );
		}

		internal void	FicheTitleChanged( Fiche _fiche, string _formerTitle ) {
			if ( _fiche == null )
				throw new Exception( "Invalid fiche!" );

			// Remove from former lists
			if ( _formerTitle != null ) {
				m_titleCaseSensitive2Fiches[_formerTitle].Remove( _fiche );
				m_titleNoCase2Fiches[_formerTitle.ToLower()].Remove( _fiche );
			}

			// Add to new lists
			string			title = _fiche.Title;
			if ( title != null ) {
				List< Fiche >	fiches;
				if ( !m_titleCaseSensitive2Fiches.TryGetValue( title, out fiches ) )
					m_titleCaseSensitive2Fiches.Add( title, fiches = new List<Fiche>() );
				fiches.Add( _fiche );

				title = title.ToLower();
				if ( !m_titleNoCase2Fiches.TryGetValue( title, out fiches ) )
					m_titleNoCase2Fiches.Add( title, fiches = new List<Fiche>() );
				fiches.Add( _fiche );
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
