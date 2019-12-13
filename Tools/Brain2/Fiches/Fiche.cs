using System;
using System.Collections.Generic;
using System.IO;

namespace Brain2 {

	/// <summary>
	/// The main fiche class
	/// </summary>
	public class Fiche {

		#region CONSTANTS

		public const uint	SIGNATURE = 0x48434946U;	// 'FICH';
		public const ushort	VERSION_MAJOR = 1;
		public const ushort	VERSION_MINOR = 0;

		#endregion

		#region NESTED TYPES

		public abstract class ChunkBase {

			/// <summary>
			/// Gets the size of the chunk
			/// </summary>
			public abstract uint		Size	{ get; }

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
		}

		#endregion

		#region FIELDS

		private FichesDB			m_ownerDatabase = null;

		private Guid				m_GUID;
		private DateTime			m_creationTime = DateTime.Now;

		private List< Fiche >		m_parents = new List< Fiche >();
		private Guid[]				m_parentGUIDs = null;

		private string				m_title = "";
		private Uri					m_URL = null;
		private string				m_HTMLContent = null;
		private List< ChunkBase >	m_chunks = new List< ChunkBase >();

		#endregion

		#region PROPERTIES

		public Guid					GUID { get { return m_GUID; } }

		public string				Title {
			get { return m_title; }
			set {
				if ( value == null )
					value = "";
				if ( value == m_title )
					return;

				m_title = value;
				m_ownerDatabase.FicheUpdated( this );
			}
		}

		public string				HTMLContent {
			get { return m_HTMLContent; }
			set {
				if ( value == m_HTMLContent )
					return;

				m_HTMLContent = value;
				m_ownerDatabase.FicheUpdated( this );
			}
		}

		public Uri				URL {
			get { return m_URL; }
			set {
				if ( value == m_URL )
					return;

				m_URL = value;
				m_ownerDatabase.FicheUpdated( this );
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

		public	Fiche( FichesDB _database ) {
			m_ownerDatabase = _database;
			m_GUID = Guid.NewGuid();
		}
		public	Fiche( FichesDB _database, string _title, Uri _URL, Fiche[] _tags, string _HTMLContent ) : this( _database ) {
			m_title = _title;
			if ( _tags != null )
				m_parents.AddRange( _tags );
			m_URL = _URL;
			m_HTMLContent = _HTMLContent;
		}

		public override string ToString() {
			return (m_title != "" ? m_title + "\r\n" : "") + m_GUID + "\r\n" + (m_URL != null ? m_URL + "\r\n" : "") + (m_HTMLContent != null ? m_HTMLContent : "<body/>");
		}

		#region I/O

		public void		Write( BinaryWriter _writer ) {
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
				_writer.Write( chunk.Size );

				chunk.Write( _writer );
			}
		}

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
				ChunkBase	chunk = CreateChunkFromType( chunkType );
				if ( chunk != null ) {
					chunk.Read( _reader );
				} else {
					_reader.BaseStream.Seek( chunkLength, SeekOrigin.Current );	// Just bypass that chunk... :/
				}
			}
		}

		/// <summary>
		/// Creates a chunk from its type name
		/// </summary>
		/// <param name="_chunkType"></param>
		/// <returns></returns>
		private ChunkBase	CreateChunkFromType( string _chunkType ) {
			switch ( _chunkType ) {

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
