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
	public class Fiche : IDisposable {

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
			CREATING,			// In the process of being created
			SAVING,				// In the process of being saved
			LOADING,			// In the process of being loaded
		}

		public enum TYPE {
			ANNOTABLE_WEBPAGE,	// Distant URL with immutable HTML content, only annotations, underlining and manual drawing are available
			EDITABLE_WEBPAGE,	// Local URL with simple editable HTML content
			LOCAL_FILE,			// Link to a local file with tracking
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

			#region Async Loading

			#endregion
		}

		/// <summary>
		/// Contains a JPEG-compressed thumbnail of the web page snapshot
		/// </summary>
		public class ChunkThumbnail : ChunkBase {

			public const uint	THUMBNAIL_WIDTH = 128;	// 1/10th of the webpage size
			public const uint	THUMBNAIL_HEIGHT = 208;	// Phi * Width = Golden rectangle

			public static readonly ColorProfile	DEFAULT_PROFILE = new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB );

			private ImageFile	m_thumbnail = null;

			public override object Content {
				get {
					if ( m_thumbnail == null ) {
						// Launch load process & create a placeholder for now...
						ms_database.AsyncLoad( this );
						m_thumbnail = new ImageFile( THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT, PIXEL_FORMAT.BGRA8, DEFAULT_PROFILE );
					}

					return m_thumbnail;
				}
			}

			public ChunkThumbnail( Fiche _owner, ulong _offset, uint _size ) : base( _owner, _offset, _size ) {

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
					ms_database.SyncNotify( () => { NotifyContentUpdated(); } );

				} catch ( Exception _e ) {
					ms_database.SyncReportError( "An error occurred while attempting to read thumbnail chunk for fiche \"" + m_owner.ToString() + "\": " + _e.Message );
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
						ms_database.AsyncLoad( this );
						m_image = new ImageFile( m_width, m_height, PIXEL_FORMAT.BGRA8, DEFAULT_PROFILE );
					}

					return m_image;
				}
			}

			public ChunkWebPageSnapshot( Fiche _owner, ulong _offset, uint _size ) : base( _owner, _offset, _size ) {
			}

			public override void Read(BinaryReader _reader) {
				// Only read image width & height
				m_width = _reader.ReadUInt32();
				m_height = _reader.ReadUInt32();

				// The rest of the read is performed asynchronously when "Content" is requested
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

					byte[]	content = new byte[m_size];
					_S.Read( content, 0, (int) m_size );

					// Attempt to read the PNG file
					ImageFile	temp = null;
					using ( NativeByteArray imageContent = new NativeByteArray( content ) ) {
						temp = new ImageFile( imageContent, ImageFile.FILE_FORMAT.PNG );
					}

					// Replace current image
					m_image.Dispose();
					m_image = temp;

					// Notify
					ms_database.SyncNotify( () => { NotifyContentUpdated(); } );

				} catch ( Exception _e ) {
					ms_database.SyncReportError( "An error occurred while attempting to read image chunk for fiche \"" + m_owner.ToString() + "\": " + _e.Message );
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

		internal static FichesDB	ms_database = null;

		private Guid				m_GUID;
		private DateTime			m_creationTime = DateTime.Now;

		private List< Fiche >		m_parents = new List< Fiche >();
		private Guid[]				m_parentGUIDs = null;

		private TYPE				m_type = TYPE.ANNOTABLE_WEBPAGE;	// Default for fiches built from a URL
		private string				m_title = "";
		private Uri					m_URL = null;
		private string				m_HTMLContent = null;

		private STATUS				m_status = STATUS.EMPTY;

		private List< ChunkBase >	m_chunks = new List< ChunkBase >();

		#endregion

		#region PROPERTIES

		public STATUS				Status { get { lock ( this ) return m_status; } }

		public Guid					GUID { get { return m_GUID; } }

		public TYPE					Type { get {return m_type; } }

		public string				Title {
			get { return m_title; }
			set {
				if ( value == null )
					value = "";
				if ( value == m_title )
					return;

				m_title = value;
				ms_database.FicheUpdated( this );
			}
		}

		public string				HTMLContent {
			get { return m_HTMLContent; }
			set {
				if ( value == m_HTMLContent )
					return;

				m_HTMLContent = value;
				ms_database.FicheUpdated( this );
			}
		}

		public Uri					URL {
			get { return m_URL; }
			set {
				if ( value == m_URL )
					return;

				m_URL = value;
				ms_database.FicheUpdated( this );
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

		#endregion

		#region METHODS

		public	Fiche() {
			m_GUID = Guid.NewGuid();
		}
		public	Fiche( FichesDB _database, TYPE _type, string _title, Uri _URL, Fiche[] _tags, string _HTMLContent ) : this() {
			m_type = _type;
			m_title = _title;
			if ( _tags != null )
				m_parents.AddRange( _tags );
			m_URL = _URL;
			m_HTMLContent = _HTMLContent;
		}

		public void Dispose() {
			foreach ( ChunkBase chunk in m_chunks ) {
				chunk.Dispose();
			}
		}

		public override string ToString() {
			return (m_title != "" ? m_title + "\r\n" : "") + m_GUID + "\r\n" + (m_URL != null ? m_URL + "\r\n" : "") + (m_HTMLContent != null ? m_HTMLContent : "<body/>");
		}

		#region I/O

		public void		Write( BinaryWriter _writer ) {
			lock ( this ) {
				STATUS	oldStatus = m_status;
				try {
					if ( m_status != STATUS.READY )
						throw new Exception( "Can't save while fiche is not ready!" );

					m_status = STATUS.SAVING;	// We lock this so noone can modify it while we're saving, and we're also changing the status in any case

					_writer.Write( SIGNATURE );
					_writer.Write( VERSION_MAJOR );
					_writer.Write( VERSION_MINOR );

					// Write hierarchy
					_writer.Write( m_GUID.ToString() );
					_writer.Write( m_creationTime.ToString() );
					_writer.Write( (uint) m_parents.Count );
					foreach ( Fiche parent in m_parents ) {
						_writer.Write( parent.m_GUID.ToString() );
					}

					// Write content
					_writer.Write( m_type.ToString() );
					_writer.Write( m_title );
					_writer.Write( m_URL != null );
					if ( m_URL != null ) {
						_writer.Write( true );
						_writer.Write( m_URL.OriginalString );
					}
					if ( m_HTMLContent != null ) {
						_writer.Write( true );
						_writer.Write( m_HTMLContent );
					}

					// Write chunks
					_writer.Write( (uint) m_chunks.Count );
					foreach ( ChunkBase chunk in m_chunks ) {
						_writer.Write( chunk.GetType().Name );
						_writer.Write( (uint) 0 );
						ulong	chunkStartOffset = (ulong) _writer.BaseStream.Position;

						chunk.Write( _writer );

						// Go back to write chunk size
						ulong	chunkEndOffset = (ulong) _writer.BaseStream.Position;
						uint	chunkSize = (uint) (chunkEndOffset - chunkStartOffset);
						_writer.BaseStream.Position = (long) (chunkStartOffset - sizeof(ulong));
						_writer.Write( chunkSize );
						_writer.BaseStream.Position = (long) chunkEndOffset;
					}
				} catch ( Exception _e ) {
//					BrainForm.Debug( "Error while saving fiche \"" + ToString() + "\": " + _e.Message );
					throw _e;
				} finally {
					m_status = oldStatus;	// Restore status anyway
				}
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
			m_parents.Clear();
			m_parentGUIDs = new Guid[parentsCount];
			for ( int parentIndex=0; parentIndex < parentsCount; parentIndex++ ) {
				strGUID = _reader.ReadString();
				if ( !Guid.TryParse( strGUID, out m_parentGUIDs[parentIndex] ) )
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
				m_URL = new Uri( strURL );
			}
			if ( _reader.ReadBoolean() ) {
				m_HTMLContent = _reader.ReadString();
			}

			// Read chunks
			m_chunks.Clear();
			uint	chunksCount = _reader.ReadUInt32();
			for ( uint chunkIndex=0; chunkIndex < chunksCount; chunkIndex++ ) {
				string		chunkType = _reader.ReadString();
				uint		chunkLength = _reader.ReadUInt32();
				ulong		chunkStartOffset = (ulong) _reader.BaseStream.Position;

				ChunkBase	chunk = CreateChunkFromType( chunkType, chunkStartOffset, chunkLength );
				if ( chunk != null ) {
					chunk.Read( _reader );	// Only shallow data will be available, heavy data will be loaded asynchonously
				}

				// Always jump to chunk's end, whether it read something or not...
				ulong		chunkEndOffset = chunkStartOffset + chunkLength;
				_reader.BaseStream.Seek( (long) chunkEndOffset, SeekOrigin.Begin );
			}
		}

		/// <summary>
		/// Creates a chunk from its type name
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
		/// Called as a post-process to finally resolve actual parent links after read
		/// </summary>
		/// <param name="_ID2Fiche"></param>
		public void		ResolveParents( Dictionary< Guid, Fiche > _ID2Fiche ) {
			m_parents.Clear();
			foreach ( Guid parentID in m_parentGUIDs ) {
				Fiche	parent = null;
				if ( _ID2Fiche.TryGetValue( parentID, out parent ) )
					m_parents.Add( parent );
			}
		}

		#endregion

		public static string	BuildHTMLDocument( string _title, string _content ) {

			string	template =
@"<!DOCTYPE html>
<html>

<head>
  <title>@TITLE@</title>
</head>

<body>
@CONTENT@
</body>

</html>";

			string	doc = template.Replace( "@TITLE@", _title ).Replace( "@CONTENT@", _content );
			return doc;
		}

		#endregion
	}
}
