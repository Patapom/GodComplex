using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using SharpMath;
using ImageUtility;

namespace Brain2 {

	/// <summary>
	/// The main fiche class
	/// </summary>
	public partial class Fiche : IDisposable {

		#region CONSTANTS

		public const uint	SIGNATURE = 0x48434946U;	// 'FICH';
		public const ushort	VERSION_MAJOR = 1;
		public const ushort	VERSION_MINOR = 0;

		#endregion

		#region NESTED TYPES

		public enum STATUS {
			DISPOSED,			// Disposed, shouldn't be accessed!
			EMPTY,				// Empty fiche
			READY,				// Complete fiche
			UPDATING,			// In the process of being updated
			SAVING,				// In the process of being saved
			LOADING,			// In the process of being loaded
		}

		public enum TYPE {
			REMOTE_ANNOTABLE_WEBPAGE,	// Remote URL with immutable HTML content, only annotations, underlining and manual drawing are available
			LOCAL_EDITABLE_WEBPAGE,		// Local URL with simple editable HTML content
			LOCAL_FILE,					// Link to a local file with tracking
		}

		public abstract class ChunkBase : IDisposable {

			protected Fiche				m_owner;
			protected ulong				m_offset;
			protected uint				m_size;

			public Fiche				OwnerFiche	{ get { return m_owner; } }

			/// <summary>
			/// Returns the chunk's content
			/// </summary>
			/// <remarks>Should always return a valid placeholder while content is not available</remarks>
			public abstract object		Content { get; }

			/// <summary>
			/// Occurs when the content is updated
			/// </summary>
			/// <remarks>Can be raised from another thread!</remarks>
			public event EventHandler	ContentUpdated;

			/// <summary>
			/// Reads the chunk from a binary stream
			/// </summary>
			/// <param name="_reader"></param>
			public abstract void		Read( BinaryReader _reader );

			/// <summary>
			/// Writes the chunk to a binary stream
			/// </summary>
			/// <param name="_writer"></param>
			public abstract void		Write( BinaryWriter _writer );
		
			public ChunkBase( Fiche _owner, ulong _offset, uint _size ) {
				m_owner = _owner;
				m_offset = _offset;
				m_size = _size;
			}

			public void Dispose() {
				lock ( this )
					InternalDispose();
			}

			/// <summary>
			/// Override this to load the content.
			/// </summary>
			/// <param name="_content"></param>
			/// <remarks>This method is called from another thread</remarks>
			internal abstract void		Threaded_LoadContent( Stream _S );

			/// <summary>
			/// Override this to dispose of the content
			/// </summary>
			/// <remarks>"this" is locked by the caller thread when this method is called</remarks>
			protected abstract void		InternalDispose();

			protected void				NotifyContentUpdated() {
				if ( ContentUpdated != null )
					ContentUpdated( this, EventArgs.Empty );
//					ContentUpdated?.Invoke( this, EventArgs.Empty );
			}
		}

		/// <summary>
		/// Contains a JPEG-compressed thumbnail of the web page snapshot
		/// </summary>
		[System.Diagnostics.DebuggerDisplay( "{m_thumbnail.Width}x{m_thumbnail.Height}" )]
		public class ChunkThumbnail : ChunkBase {

			public const uint	THUMBNAIL_WIDTH = 128;	// 1/10th of the webpage size
			public const uint	THUMBNAIL_HEIGHT = 208;	// Phi * Width = Golden rectangle

			public static readonly ColorProfile	DEFAULT_PROFILE = new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB );

			private ImageFile	m_thumbnail = null;

			public override object Content {
				get {
					if ( m_thumbnail == null ) {
						// Launch load process & create a placeholder for now...
						m_owner.m_database.AsyncLoadChunk( this );
						m_thumbnail = new ImageFile( THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT, PIXEL_FORMAT.BGR8, DEFAULT_PROFILE );	// Note that it's important to create a 24-bits format here to be able to save as JPEG!
					}

					return m_thumbnail;
				}
			}

			public ChunkThumbnail( Fiche _owner, ulong _offset, uint _size ) : base( _owner, _offset, _size ) {
			}
			public ChunkThumbnail( Fiche _owner, ImageFile _imageWebPage ) : base( _owner, ~0UL, 0 ) {
				UpdateFromWebPageImage( _imageWebPage );
			}

			public void	UpdateFromWebPageImage( ImageFile _imageWebPage ) {
				if ( _imageWebPage == null )
					throw new Exception( "Invalid image!" );

				if ( m_thumbnail != null )
					m_thumbnail.Dispose();

				// Create a tiny thumbnail from the image
				uint	thumbnailHeight = Mathf.Min( _imageWebPage.Height * THUMBNAIL_WIDTH / _imageWebPage.Width, THUMBNAIL_HEIGHT );	// At most our preferred ratio => we must crop the full page!
				float	imageScale = (float) _imageWebPage.Width / THUMBNAIL_WIDTH;

				m_thumbnail = new ImageFile( THUMBNAIL_WIDTH, thumbnailHeight, PIXEL_FORMAT.BGR8, DEFAULT_PROFILE );	// Note that it's important to create a 24-bits format here to be able to save as JPEG!

				// Read "height" scanlines
				float4[]	sourceScanline = new float4[_imageWebPage.Width];
				float4[]	targetScanline = new float4[THUMBNAIL_WIDTH];
				for ( uint Y=0; Y < thumbnailHeight; Y++ ) {
					uint	sourceY = (uint) (imageScale * Y);
					_imageWebPage.ReadScanline( sourceY, sourceScanline );
					for ( uint X=0; X < THUMBNAIL_WIDTH; X++ ) {
						targetScanline[X] = sourceScanline[(uint) (imageScale * (X+0.5f))];
					}
					m_thumbnail.WriteScanline( Y, targetScanline );
				}

				// Notify?
				m_owner.NotifyThumbnailChanged( this );
			}

			public override void Read(BinaryReader _reader) {
				// Read is performed asynchronously when "Content" is requested
			}

			public override void Write(BinaryWriter _writer) {
				if ( m_thumbnail == null )
					throw new Exception( "Attempting to save an ampty thumbnail chunk! Don't create the chunk if it's normal to be empty..." );

				using ( NativeByteArray content = m_thumbnail.Save( ImageFile.FILE_FORMAT.JPEG, ImageFile.SAVE_FLAGS.SF_JPEG_FAST ) ) {
					byte[]	managedContent = content.AsByteArray;
					_writer.Write( managedContent );
				}
			}

			internal override void	Threaded_LoadContent( Stream _S ) {
				try {
					_S.Position = (long) m_offset;

					byte[]	content = new byte[m_size];
					_S.Read( content, 0, (int) m_size );

					// Attempt to read the JPEG file
					ImageFile	temp = null;
					using ( NativeByteArray imageContent = new NativeByteArray( content ) ) {
						temp = new ImageFile( imageContent, ImageFile.FILE_FORMAT.JPEG );
					}

					// Replace current thumbnail
					m_thumbnail.Dispose();
					m_thumbnail = temp;

					// Notify
					m_owner.m_database.SyncNotify( () => { NotifyContentUpdated(); } );

				} catch ( Exception _e ) {
					m_owner.m_database.SyncReportError( "An error occurred while attempting to read thumbnail chunk for fiche \"" + m_owner.ToString() + "\": " + _e.Message );
				}
			}

			protected override void InternalDispose() {
				if ( m_thumbnail != null ) {
					m_thumbnail.Dispose();
					m_thumbnail = null;
				}
			}
		}

		/// <summary>
		/// Contains the full web page snapshot
		/// </summary>
		[System.Diagnostics.DebuggerDisplay( "{m_width}x{m_height}" )]
		public class ChunkWebPageSnapshot : ChunkBase {

			public const uint	DEFAULT_WEBPAGE_WIDTH = 1280;	// Default snapshot width
			public const uint	DEFAULT_WEBPAGE_HEIGHT = 2071;	// Phi * Width = Golden rectangle 

			public static readonly ColorProfile	DEFAULT_PROFILE = new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB );

			private uint		m_width = DEFAULT_WEBPAGE_WIDTH;
			private uint		m_height = DEFAULT_WEBPAGE_HEIGHT;

			private ImageFile	m_image = null;

			public override object Content {
				get {
					if ( m_image == null ) {
						// Launch load process & create a placeholder for now...
						m_owner.m_database.AsyncLoadChunk( this );
						m_image = new ImageFile( m_width, m_height, PIXEL_FORMAT.BGRA8, DEFAULT_PROFILE );
					}

					return m_image;
				}
			}

			public ChunkWebPageSnapshot( Fiche _owner, ulong _offset, uint _size ) : base( _owner, _offset, _size ) {
			}
			public ChunkWebPageSnapshot( Fiche _owner, ImageFile _image ) : base( _owner, ~0UL, 0 ) {
				UpdateImage( _image );
			}

			public void	UpdateImage( ImageFile _image ) {
				if ( _image == null )
					throw new Exception( "Invalid image!" );

				if ( m_image != null )
					m_image.Dispose();

				m_image = _image;
				m_width = m_image.Width;
				m_height = m_image.Height;

				// Notify?
				m_owner.NotifyWebPageImageChanged( this );
			}

			/// <summary>
			/// Releases the image
			/// </summary>
			public void	UnloadImage() {
				if ( m_image != null ) {
					m_image.Dispose();
					m_image = null;
				}
			}

			public override void Read(BinaryReader _reader) {
				// Only read image width & height
				m_width = _reader.ReadUInt32();
				m_height = _reader.ReadUInt32();

				// The rest of the read is performed asynchronously whenever "Content" is accessed
			}

			public override void Write(BinaryWriter _writer) {
				if ( m_image == null )
					throw new Exception( "Attempting to save an ampty thumbnail chunk! Don't create the chunk if it's normal to be empty..." );

				// Write width & height
				_writer.Write( m_image.Width );
				_writer.Write( m_image.Height );

				// Write actual content
				using ( NativeByteArray content = m_image.Save( ImageFile.FILE_FORMAT.PNG, ImageFile.SAVE_FLAGS.SF_PNG_Z_BEST_COMPRESSION ) ) {
					byte[]	managedContent = content.AsByteArray;
					_writer.Write( managedContent );
				}
			}

			internal override void	Threaded_LoadContent( Stream _S ) {
				try {
					_S.Position = (long) m_offset;

					// Use "light" regular reader
					using ( BinaryReader R = new BinaryReader( _S ) )
						Read( R );

					// Read remaining
					uint	contentSize = m_size - (uint) ((ulong) _S.Position - m_offset);
					byte[]	content = new byte[contentSize];
					_S.Read( content, 0, (int) contentSize );

					// Attempt to read the PNG file
					ImageFile	temp = null;
					using ( NativeByteArray imageContent = new NativeByteArray( content ) ) {
						temp = new ImageFile( imageContent, ImageFile.FILE_FORMAT.PNG );
					}

					// Replace current image
					m_image.Dispose();
					m_image = temp;

					// Notify
					m_owner.m_database.SyncNotify( () => { NotifyContentUpdated(); } );

				} catch ( Exception _e ) {
					m_owner.m_database.SyncReportError( "An error occurred while attempting to read image chunk for fiche \"" + m_owner.ToString() + "\": " + _e.Message );
				}
			}

			protected override void InternalDispose() {
				if ( m_image != null ) {
					m_image.Dispose();
					m_image = null;
				}
			}
		}

		#endregion

		#region FIELDS

		private FichesDB			m_database = null;

		private Guid				m_GUID;
		private DateTime			m_creationTime = DateTime.Now;

		private List< Fiche >		m_tags = new List< Fiche >();
		private List< Fiche >		m_references = new List< Fiche >();
		private Guid[]				m_tagGUIDs = null;

		private TYPE				m_type = TYPE.REMOTE_ANNOTABLE_WEBPAGE;	// Default for fiches built from a URL
		private string				m_title = "";
		private Uri					m_URL = null;
		private string				m_HTMLContent = null;

		private STATUS				m_status = STATUS.EMPTY;

		private List< ChunkBase >	m_chunks = new List< ChunkBase >();

		#endregion

		#region PROPERTIES

		public FichesDB			Database {
			get { return m_database; }
			set {
				if ( value == m_database )
					return;

				if ( m_database != null )
					m_database.UnRegisterFiche( this );

				m_database = value;

				if ( m_database != null )
					m_database.RegisterFiche( this );
			}
		}

		public STATUS				Status { get { lock ( this ) return m_status; } }

		public Guid					GUID {
			get { return m_GUID; }
			set {
				Lock( STATUS.UPDATING, () => {
					if ( value == m_GUID )
						return;

					Guid	oldGUID = m_GUID;
					m_GUID = value;

					m_database.FicheGUIDChanged( this, oldGUID );
				} );
			}
		}

		public DateTime				CreationTime {
			get { return m_creationTime; }
			set {
				Lock( STATUS.UPDATING, () => {
					if ( value == m_creationTime )
						return;

					DateTime	oldCreationTime = m_creationTime;
					m_creationTime = value;

					m_database.FicheCreationDateChanged( this, oldCreationTime );
				} );
			}
		}
		public TYPE					Type { get {return m_type; } }

		public string				Title {
			get { return m_title; }
			set {
				Lock( STATUS.UPDATING, () => {
					if ( value == null )
						value = "";
					if ( value == m_title )
						return;

					string	oldTitle = m_title;
					m_title = value;
					Database.FicheTitleChanged( this, oldTitle );
				} );
			}
		}

		public string				HTMLContent {
			get { return m_HTMLContent; }
			set {
				Lock( STATUS.UPDATING, () => {
					if ( value == m_HTMLContent )
						return;

					m_HTMLContent = value;
					Database.FicheHTMLContentChanged( this );
				} );
			}
		}

		public Uri					URL {
			get { return m_URL; }
			set {
				Lock( STATUS.UPDATING, () => {
					if ( value == m_URL )
						return;

					Uri	oldURL = m_URL;
					m_URL = value;
					Database.FicheURLChanged( this, oldURL );
				} );
			}
		}

		/// <summary>
		/// Generates a unique filename for the fiche
		/// </summary>
		public string				FileName { get {
//				return m_GUID.ToString() + (m_title != "" ? "." + m_title : "") + ".fiche";
				return m_GUID.ToString() + ".fiche";
			}
		}

		/// <summary>
		/// Returns the amount of fiches that reference this fiche (as a tag)
		/// </summary>
		public int					ReferencesCount { get { return m_references.Count; } }

		/// <summary>
		/// Raised whenever the web page image changed
		/// </summary>
		public event EventHandler	WebPageImageChanged;

		/// <summary>
		/// Raised whenever the thumbnail changed
		/// </summary>
		public event EventHandler	ThumbnailChanged;

		#endregion

		#region METHODS

		protected	Fiche( FichesDB _database, string _title ) {
			m_GUID = Guid.NewGuid();
			m_title = _title;

			Database = _database;
		}
		public	Fiche( FichesDB _database, TYPE _type, string _title, Uri _URL, Fiche[] _tags, string _HTMLContent ) {
			m_GUID = Guid.NewGuid();
			m_type = _type;
			m_title = _title;
			m_URL = _URL;
			AddTags( _tags );
			m_HTMLContent = _HTMLContent;

			Database = _database;
			m_status = STATUS.READY;
		}
		public	Fiche( FichesDB _database, BinaryReader _reader ) {
			Read( _reader );
			Database = _database;	// Register afterward so our registration data (e.g. GUID, URL, title, etc.) are ready
		}

		public void Dispose() {
			lock ( this ) {
				foreach ( ChunkBase chunk in m_chunks ) {
					chunk.Dispose();
				}

				// This will unregister us from the database
				Database = null;

				// Change status
				m_status = STATUS.DISPOSED;
			}
		}

		public override string ToString() {
			return (m_title != "" ? m_title + "\r\n" : "") + m_GUID + "\r\n" + (m_URL != null ? m_URL + "\r\n" : "") + (m_HTMLContent != null ? m_HTMLContent : "<body/>");
		}

		#region I/O

		public void		Write( BinaryWriter _writer ) {
			try {
				_writer.Write( SIGNATURE );
				_writer.Write( VERSION_MAJOR );
				_writer.Write( VERSION_MINOR );

				// Write hierarchy
				_writer.Write( m_GUID.ToString() );
				_writer.Write( m_creationTime.ToString() );
				_writer.Write( (uint) m_tags.Count );
				foreach ( Fiche parent in m_tags ) {
					_writer.Write( parent.m_GUID.ToString() );
				}

				// Write content
				_writer.Write( m_type.ToString() );
				_writer.Write( m_title );
				_writer.Write( m_URL != null );
				if ( m_URL != null ) {
					_writer.Write( m_URL.OriginalString );
				}
				_writer.Write( m_HTMLContent != null );
				if ( m_HTMLContent != null ) {
					_writer.Write( m_HTMLContent );
				}

				// Write chunks
				_writer.Write( (uint) m_chunks.Count );
				foreach ( ChunkBase chunk in m_chunks ) {
					_writer.Write( chunk.GetType().Name );
					_writer.Write( (uint) 0 );	// Placeholder => Will be filled with proper size when chunk is actually written
					ulong	chunkStartOffset = (ulong) _writer.BaseStream.Position;

					chunk.Write( _writer );

					// Go back to write chunk size
					ulong	chunkEndOffset = (ulong) _writer.BaseStream.Position;
					uint	chunkSize = (uint) (chunkEndOffset - chunkStartOffset);
					_writer.BaseStream.Position = (long) (chunkStartOffset - sizeof(uint));
					_writer.Write( chunkSize );
					_writer.BaseStream.Position = (long) chunkEndOffset;
				}
			} catch ( Exception _e ) {
//				BrainForm.Debug( "Error while saving fiche \"" + ToString() + "\": " + _e.Message );
				throw _e;
			}
		}

		/// <summary>
		/// Reads the fiche's description and HTML content
		/// </summary>
		/// <param name="_reader"></param>
		/// <remarks>Heavy chunks are NOT read and will only be accessible asynchronously</remarks>
		public void		Read( BinaryReader _reader ) {
			uint	signature = _reader.ReadUInt32();
			if ( signature != SIGNATURE )
				throw new Exception( "Unexpected signature!" );

			uint	versionMajor, versionMinor;
			versionMajor = (uint) _reader.ReadUInt16();
			versionMinor = (uint) _reader.ReadUInt16();
			uint	version = (versionMajor << 16) | versionMinor;
			
			// Read hierarchy
			string	strGUID	= _reader.ReadString();
			if ( !Guid.TryParse( strGUID, out m_GUID ) )
				throw new Exception( "Failed to parse fiche GUID!" );

			string	strCreationTime = _reader.ReadString();
			if ( !DateTime.TryParse( strCreationTime, out m_creationTime ) )
				throw new Exception( "Failed to parse fiche creation time!" );

				// We only read the GUIDs while the actual fiches will be processed later
			uint	parentsCount = _reader.ReadUInt32();
			while ( m_tags.Count > 0 ) {
				RemoveTag( m_tags[0] );
			}
			m_tagGUIDs = new Guid[parentsCount];
			for ( int parentIndex=0; parentIndex < parentsCount; parentIndex++ ) {
				strGUID = _reader.ReadString();
				if ( !Guid.TryParse( strGUID, out m_tagGUIDs[parentIndex] ) )
					throw new Exception( "Failed to parse fiche's parent GUID!" );
			}

			// Read content
			string	strType = _reader.ReadString();
			if ( !Enum.TryParse( strType, out m_type ) ) {
				throw new Exception( "Failed to parse fiche's type!" );
			}
			m_title = _reader.ReadString();
			if ( _reader.ReadBoolean() ) {
				string	strURL = _reader.ReadString();
				m_URL = WebHelpers.CreateCanonicalURL( strURL );
			}
			if ( _reader.ReadBoolean() ) {
				m_HTMLContent = _reader.ReadString();
			}

			// Read chunks
			while ( m_chunks.Count > 0 ) {
				m_chunks[0].Dispose();
				m_chunks.RemoveAt( 0 );
			}
			uint	chunksCount = _reader.ReadUInt32();
			for ( uint chunkIndex=0; chunkIndex < chunksCount; chunkIndex++ ) {
				string		chunkType = _reader.ReadString();
				uint		chunkLength = _reader.ReadUInt32();
				ulong		chunkStartOffset = (ulong) _reader.BaseStream.Position;

				ChunkBase	chunk = CreateChunkFromType( chunkType, chunkStartOffset, chunkLength );
				if ( chunk != null ) {
					chunk.Read( _reader );	// Only shallow data will be available, heavy data will be loaded asynchonously on demand
				}

				// Always jump to chunk's end, whether it read something or not...
				ulong		chunkEndOffset = chunkStartOffset + chunkLength;
				_reader.BaseStream.Seek( (long) chunkEndOffset, SeekOrigin.Begin );
			}

			// Fiche is now ready!
			m_status = STATUS.READY;
		}

		/// <summary>
		/// Called as a post-process to finally resolve actual tag links after read
		/// </summary>
		/// <param name="_ID2Fiche"></param>
		public void		ResolveTags( Dictionary< Guid, Fiche > _ID2Fiche ) {
			foreach ( Guid parentID in m_tagGUIDs ) {
				Fiche	parent = null;
				if ( _ID2Fiche.TryGetValue( parentID, out parent ) )
					AddTag( parent );
			}
		}

		#endregion

		#region Tags Management

		public void	AddTags( IEnumerable<Fiche> _tags ) {
			if ( _tags != null ) {
				foreach ( Fiche tag in _tags ) {
					AddTag( tag );
				}
			}
		}

		public void	AddTag( Fiche _tag ) {
			if ( m_tags.Contains( _tag ) )
				return;	// Already got it!
			
			m_tags.Add( _tag );
			_tag.AddReference( this );
		}

		public void	RemoveTag( Fiche _tag ) {
			if ( !m_tags.Contains( _tag ) )
				return;	// Don't have it!
			
			m_tags.Remove( _tag );
			_tag.RemoveReference( this );
		}

		public void	AddReference( Fiche _reference ) {
			if ( m_references.Contains( _reference ) )
				return;	// Already got it!

			m_references.Add( _reference );
		}

		public void	RemoveReference( Fiche _reference ) {
			if ( !m_references.Contains( _reference ) )
				return;	// Don't have it!

			m_references.Remove( _reference );
		}

		#endregion

		#region Chunks Management

		internal T	FindChunkByType<T>() where T : ChunkBase {
			foreach ( ChunkBase chunk in m_chunks ) {
				if ( chunk is T )
					return chunk as T;
			}

			return null;
		}

		/// <summary>
		/// Creates a chunk from its type name, used by the Read() function to create placeholders
		/// </summary>
		/// <param name="_chunkType"></param>
		/// <returns></returns>
		private ChunkBase	CreateChunkFromType( string _chunkType, ulong _chunkOffset, uint _chunkLength ) {
			switch ( _chunkType ) {
				case "ChunkThumbnail": return new ChunkThumbnail( this, _chunkOffset, _chunkLength );
				case "ChunkWebPageSnapshot": return new ChunkWebPageSnapshot( this, _chunkOffset, _chunkLength );
			}

			return null;
		}

		/// <summary>
		/// Create the image chunk and feed it our image
		/// </summary>
		/// <param name="_imageWebPage"></param>
		internal void	CreateImageChunk( ImageFile _imageWebPage ) {
			ChunkWebPageSnapshot	chunk = FindChunkByType<ChunkWebPageSnapshot>();
			if ( chunk == null ) {
				chunk = new ChunkWebPageSnapshot( this, _imageWebPage );
				m_chunks.Add( chunk );
			} else {
				chunk.UpdateImage( _imageWebPage );
			}
		}

		// Create thumbnail chunk from the full webpage
		internal void	CreateThumbnailChunkFromImage( ImageFile _imageWebPage ) {
			ChunkThumbnail	chunk = FindChunkByType<ChunkThumbnail>();
			if ( chunk == null ) {
				chunk = new ChunkThumbnail( this, _imageWebPage );
				m_chunks.Add( chunk );
			} else {
				chunk.UpdateFromWebPageImage( _imageWebPage );
			}
		}

		/// <summary>
		/// Unloads the image chunk's image
		/// </summary>
		internal void	UnloadImageChunk() {
			ChunkWebPageSnapshot	chunk = FindChunkByType<ChunkWebPageSnapshot>();
			if ( chunk != null )
				chunk.UnloadImage();
		}

		#endregion

		/// <summary>
		/// Locks the fiche in a given status. Used by external workers that will work on the fiche (e.g. update, load, save, etc.)
		/// </summary>
		/// <param name="_status">The status to assign to the fiche while the lock is effective</param>
		/// <param name="_delegate">The delegate that will be called when the lock is effective</param>
		internal void	Lock( STATUS _status, Action _delegate ) {
			lock ( this ) {
				STATUS	oldStatus = m_status;
				m_status = _status;

				// The worker can work now!
				_delegate();

				m_status = oldStatus;
			}
		}

		private void	NotifyWebPageImageChanged( ChunkWebPageSnapshot _caller ) {
			if ( WebPageImageChanged != null )
				WebPageImageChanged( _caller, EventArgs.Empty );
		}

		private void	NotifyThumbnailChanged( ChunkThumbnail _caller ) {
			if ( ThumbnailChanged != null )
				ThumbnailChanged( _caller, EventArgs.Empty );
		}

		#endregion
	}
}
