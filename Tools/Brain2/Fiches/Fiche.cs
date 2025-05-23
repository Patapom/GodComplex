﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Drawing;

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
			ACCESSING,			// In the process of being accessed by a user
		}

		public enum TYPE {
			REMOTE_ANNOTABLE_WEBPAGE,	// Remote URL with immutable HTML content, only annotations, underlining and manual drawing are available
			LOCAL_EDITABLE_WEBPAGE,		// Local URL with simple editable HTML content
			LOCAL_FILE,					// Link to a local file with tracking
		}

		public delegate void	FicheEventHandler( Fiche _sender );

		#region Chunks

		public abstract class ChunkBase : IDisposable {

			protected Fiche				m_owner;
			protected ulong				m_offset;
			protected uint				m_size;

			public Fiche				OwnerFiche	{ get { return m_owner; } }

			/// <summary>
			/// Gets the chunk's offset in the stream
			/// </summary>
			/// <remarks>An offset of ~0UL indicates the chunk has not been serialized to a stream yet</remarks>
			public ulong				Offset	{ get { return m_offset; } }

			/// <summary>
			/// Gets the chunk's size
			/// </summary>
			/// <remarks>A size of 0 indicates the chunk has not been serialized to a stream yet</remarks>
			public uint					Size	{ get { return m_size; } }

			/// <summary>
			/// Tells if the content is FULLY available (i.e. Content has been called and is ready) or the chunk is still incomplete
			/// If the chunk answers false then this will trigger an immediate call to Threaded_LoadContent( reader, false ) to ask the chunk to fully load its content (but not prepare it for display)
			/// This occurs right before a chunk is saved again with an updated version of the fiche
			/// </summary>
			public abstract bool		IsFullyLoaded	{ get; }

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

// 			/// <summary>
// 			/// Last chance to reads the chunk from the existing binary stream before it's saved again
// 			/// </summary>
// 			/// <param name="_reader"></param>
// 			public abstract void		LastChanceReadBeforeWrite( BinaryReader _reader );

			/// <summary>
			/// Writes the chunk to a binary stream
			/// </summary>
			/// <param name="_writer"></param>
			public abstract void		Write( BinaryWriter _writer );
		
			public ChunkBase( Fiche _owner, ulong _offset, uint _size ) {
				m_owner = _owner;
				if ( m_owner != null )
					m_owner.m_chunks.Add( this );

				m_offset = _offset;
				m_size = _size;
			}

			public void Dispose() {
				lock ( this )
					InternalDispose();

				if ( m_owner != null )
					m_owner.m_chunks.Remove( this );
			}

			/// <summary>
			/// Override this to load the content.
			/// </summary>
			/// <param name="_reader">Stream reader</param>
			/// <param name="_prepareContent">True to prepare data relevant for content display, false if the load is only internal and you only need to completely load the chunk so it's ready to be saved again</param>
			/// <remarks>This method is called from another thread than the main thread.
			/// The stream is already positioned at the beginning of the chunk's data</remarks>
			internal abstract void		Threaded_LoadContent( BinaryReader _reader, bool _prepareContent );

			/// <summary>
			/// Override this to dispose of the content
			/// </summary>
			/// <remarks>"this" is locked by the caller thread when this method is called</remarks>
			protected abstract void		InternalDispose();

			protected void				NotifyContentUpdated() {
				ContentUpdated?.Invoke( this, EventArgs.Empty );
			}
		}

		/// <summary>
		/// Contains the full web page as an array of single page screenshots
		/// </summary>
		[System.Diagnostics.DebuggerDisplay( "{m_images} images" )]
		public class ChunkWebPageSnapshot : ChunkBase {

			private const uint						PLACEHOLDER_WIDTH = 256;
			private const uint						PLACEHOLDER_HEIGHT = (uint) (Mathf.PHI * PLACEHOLDER_WIDTH);
			internal const ImageFile.FILE_FORMAT	DEFAULT_COMPRESSION_FORMAT = ImageFile.FILE_FORMAT.JPEG;
			private static readonly ColorProfile	DEFAULT_PROFILE = new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB );

			[System.Diagnostics.DebuggerDisplay( "{m_contentRectangle.Width}x{m_contentRectangle.Height} {m_compressedImageFormat} {m_compressedImage != null ? \"Compressed\" : \"Failed to Compress\"}" )]
			public class WebPageImagePart : IDisposable {
				public Rectangle				m_contentRectangle = Rectangle.Empty;
				public ImageFile				m_image = null;
				public ImageFile.FILE_FORMAT	m_compressedImageFormat;
				public byte[]					m_compressedImage = null;

				internal WebPageImagePart( Rectangle _contentRectangle, ImageFile _image, ImageFile.FILE_FORMAT _compressedImageFormat ) {
					if ( _image == null )
						throw new Exception( "Invalid image part!" );

					m_contentRectangle = _contentRectangle;
					m_image = _image;
					m_compressedImageFormat = _compressedImageFormat;

					// Compress the image
					try {
						NativeByteArray content = null;
						switch ( m_compressedImageFormat ) {
							case ImageFile.FILE_FORMAT.PNG:
								content = m_image.Save( ImageFile.FILE_FORMAT.PNG, ImageFile.SAVE_FLAGS.SF_PNG_Z_BEST_COMPRESSION );
								break;

							case ImageFile.FILE_FORMAT.JPEG:
								content = m_image.Save( ImageFile.FILE_FORMAT.JPEG, ImageFile.SAVE_FLAGS.SF_JPEG_QUALITYNORMAL );
								break;

							default:
								content = m_image.Save( m_compressedImageFormat, ImageFile.SAVE_FLAGS.NONE );
								break;
						}

						if ( content == null )
							throw new Exception( "Failed to save image using \"" + m_compressedImageFormat + "\" format! Image library returned null..." );

						m_compressedImage = content.AsByteArray;
						content.Dispose();

					} catch ( Exception _e ) {
						throw new Exception( "An error occurred during image compression!", _e );
					}
				}

				internal WebPageImagePart( BinaryReader _reader, bool _decompressImage ) {
					Read( _reader, _decompressImage );
				}

				internal void	Write( BinaryWriter _writer ) {
					_writer.Write( m_contentRectangle.X );
					_writer.Write( m_contentRectangle.Y );
					_writer.Write( m_contentRectangle.Width );
					_writer.Write( m_contentRectangle.Height );

					_writer.Write( (int) m_compressedImageFormat );

					if ( m_compressedImage == null ) {
						_writer.Write( (int) 0 );
						return;
					}

					// Save compressed image
					_writer.Write( m_compressedImage.Length );
					_writer.Write( m_compressedImage );
				}

				internal void	Read( BinaryReader _reader, bool _decompressImage ) {
					m_contentRectangle = new Rectangle( _reader.ReadInt32(), _reader.ReadInt32(), _reader.ReadInt32(), _reader.ReadInt32() );
					m_compressedImageFormat = (ImageFile.FILE_FORMAT) _reader.ReadInt32();

					// Read compressed image
					int	compressedImageLength = _reader.ReadInt32();
					m_compressedImage = new byte[compressedImageLength];
					_reader.Read( m_compressedImage, 0, compressedImageLength );

					if ( !_decompressImage )
						return;

					// Attempt to read the image file
					try {
						using ( NativeByteArray imageContent = new NativeByteArray( m_compressedImage ) ) {
							m_image = new ImageFile( imageContent, m_compressedImageFormat );
						}
					} catch ( Exception _e ) {
						throw new Exception( "An error occurred while attempting to decompress image from byte[] (length = " + compressedImageLength + " format = " + m_compressedImageFormat + ")", _e );
					}
				}

				public void Dispose() {
					if ( m_image != null )
						m_image.Dispose();
					m_image = null;
					m_contentRectangle = Rectangle.Empty;
				}
			}

			private WebPageImagePart[]		m_images = new WebPageImagePart[0];

			// These are the default rendering sizes of a web-page capture
			// The user would expect these to be set to their screen's dimensions...
			//
			public static uint				ms_defaultWebPageWidth = 1280;											// Default screenshot width
			public static uint				ms_defaultWebPageHeight = (uint) (Mathf.PHI * ms_defaultWebPageWidth);	// Golden rectangle = 2071;
			public static uint				ms_maxWebPagePieces = 10;												// So up to 20710 in height

			/// <summary>
			/// Content is fully loaded whenever its compressed images are available
			/// </summary>
			public override bool IsFullyLoaded => m_images.Length == 0 || m_images[0].m_compressedImage != null;

			/// <summary>
			/// Will be of type ImageFile[]
			/// </summary>
			public override object Content {
				get {
					if ( m_images.Length > 0 && m_images[0] == null ) {
						// Create placeholders for now & launch loading process...
						for ( int imageIndex=0; imageIndex < m_images.Length; imageIndex++ ) {
							m_images[imageIndex] = new WebPageImagePart(
								new Rectangle( 0, (int) (imageIndex * PLACEHOLDER_HEIGHT), (int) PLACEHOLDER_WIDTH, (int) PLACEHOLDER_HEIGHT ),
								new ImageFile( PLACEHOLDER_WIDTH, PLACEHOLDER_HEIGHT, PIXEL_FORMAT.BGR8, DEFAULT_PROFILE ),
								DEFAULT_COMPRESSION_FORMAT
							);
						}

						m_owner.m_database.Async_LoadChunk( this, () => {
							m_owner.m_database.AsyncMain_Execute<object>( ( object _userData ) => { NotifyContentUpdated(); }, null );	// Notify on the main thread
						} );
					}

					return m_images;
				}
			}

			public ChunkWebPageSnapshot( Fiche _owner, ulong _offset, uint _size ) : base( _owner, _offset, _size ) {
			}
			public ChunkWebPageSnapshot( Fiche _owner, uint _imageStartIndex, Rectangle[] _contentRectangles, ImageFile[] _images, ImageFile.FILE_FORMAT _targetFormat ) : base( _owner, ~0UL, 0 ) {
				UpdateImages( _imageStartIndex, _contentRectangles, _images, _targetFormat );
			}

			public void	UpdateImages( uint _imageStartIndex, Rectangle[] _contentRectangles, ImageFile[] _images, ImageFile.FILE_FORMAT _compressedFormat ) {
				int					newSize = Mathf.Max( m_images.Length, (int) _imageStartIndex + _images.Length );
				WebPageImagePart[]	newImages = new WebPageImagePart[newSize];

				// Copy existing images
				for ( uint imageIndex=0; imageIndex < m_images.Length; imageIndex++ ) {
					newImages[imageIndex] = m_images[imageIndex];
				}

				// Replace with new images
				for ( uint imageIndex=0; imageIndex < _images.Length; imageIndex++ ) {
					if ( newImages[_imageStartIndex + imageIndex] != null ) {
						newImages[_imageStartIndex + imageIndex].Dispose();	// We're replacing this image with a new one...
					}

					if ( _images[imageIndex] == null )
						throw new Exception( "Invalid image! Fiche web images must not be null!" );

					try {
						newImages[_imageStartIndex + imageIndex] = new WebPageImagePart( _contentRectangles[imageIndex], _images[imageIndex], _compressedFormat );
					} catch ( Exception _e ) {
						// Something went wrong!
						m_owner.Database.AsyncMain_ReportFicheStatus( m_owner, FichesDB.FICHE_REPORT_STATUS.ERROR, "An error occurred while saving a part of the web page image! " + _e.Message );
					}
				}

				// Replace old array
				m_images = newImages;

				// Notify?
				m_owner.NotifyWebPageImageChanged( this );
			}

			/// <summary>
			/// Releases the images
			/// </summary>
			public void	UnloadImages() {
				for ( int imageIndex=0; imageIndex < m_images.Length; imageIndex++ ) {
					if ( m_images[imageIndex] != null ) {
						m_images[imageIndex].Dispose();
					}
					m_images[imageIndex] = null;
				}
			}

			public override void Write( BinaryWriter _writer ) {
				// Write image parts
				_writer.Write( m_images.Length );
				for ( int imageIndex=0; imageIndex < m_images.Length; imageIndex++ ) {
					m_images[imageIndex].Write( _writer );
				}
			}

			public override void Read(BinaryReader _reader) {
				UnloadImages();

				// Only read images count & create empty array
				m_images = new WebPageImagePart[_reader.ReadInt32()];

				// The rest of the read is performed asynchronously whenever "Content" is accessed (lazy initialization)
			}

			internal override void	Threaded_LoadContent( BinaryReader _reader, bool _prepareContent ) {
				try {

					// Use the "light" regular reader that will initialize the array of images
					Read( _reader );

// 					if ( m_compressedImages[imageIndex].Length == 0 ) {
// 						// Create an error placeholder
// 						uint	imageWidth = m_images.Length > 0 && m_images[0] != null ? m_images[0].Width : PLACEHOLDER_WIDTH;
// 						m_images[imageIndex] = new ImageFile( imageWidth, (uint) (Mathf.PHI * imageWidth), PIXEL_FORMAT.BGR8, DEFAULT_PROFILE );
// 						m_images[imageIndex].Clear( float4.One );
// 						m_images[imageIndex].DrawLine( new float4( 1, 0, 0, 1 ), new float2( 0, 0 ), new float2( m_images[imageIndex].Width-1, m_images[imageIndex].Height-1 ) );
// 						m_images[imageIndex].DrawLine( new float4( 1, 0, 0, 1 ), new float2( 0, m_images[imageIndex].Height-1 ), new float2( m_images[imageIndex].Width-1, 0 ) );
// 						continue;
// 					}
// 
// 					if ( m_images[imageIndex] != null ) {
// 						// Dispose of any existing image first...
// 						m_images[imageIndex].Dispose();
// 						m_images[imageIndex] = null;
// 					}

					// Read image parts
					for ( int imageIndex=0; imageIndex < m_images.Length; imageIndex++ ) {
						try {
							m_images[imageIndex] = new WebPageImagePart( _reader, _prepareContent );
						} catch ( Exception _e ) {
							m_images[imageIndex] = null;
							throw new Exception( "Failed to read part " + imageIndex + " of the web image chunk", _e );
						}
					}

				} catch ( Exception _e ) {
					m_owner.m_database.AsyncMain_LogError( "An error occurred while attempting to read image chunk for fiche \"" + m_owner.ToString() + "\": " + _e.Message );
				}
			}

			protected override void InternalDispose() {
				UnloadImages();
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

			private byte[]		m_compressedContent = null;
			private ImageFile	m_thumbnail = null;

			/// <summary>
			/// Content is fully loaded whenever its compressed images are available
			/// </summary>
			public override bool IsFullyLoaded => m_compressedContent != null;

			public override object Content {
				get {
					if ( m_thumbnail == null ) {
						// Create a placeholder for now and launch loading process...
						m_thumbnail = new ImageFile( THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT, PIXEL_FORMAT.BGR8, DEFAULT_PROFILE );	// Note that it's important to create a 24-bits format here to be able to save as JPEG!

						m_owner.m_database.Async_LoadChunk( this, () => {
							m_owner.m_database.AsyncMain_Execute<object>( ( object _userData ) => { NotifyContentUpdated(); }, null );	// Notify on the main thread
						} );
					}

					return m_thumbnail;
				}
			}

			public ChunkThumbnail( Fiche _owner, ulong _offset, uint _size ) : base( _owner, _offset, _size ) {
			}
			public ChunkThumbnail( Fiche _owner, ImageFile _imagesWebPage ) : base( _owner, ~0UL, 0 ) {
				UpdateFromWebPageImage( _imagesWebPage );
			}

			public void	UpdateFromWebPageImage( ImageFile _imagesWebPage ) {
				if ( _imagesWebPage == null )
					throw new Exception( "Invalid image!" );

				if ( m_thumbnail != null )
					m_thumbnail.Dispose();
				m_compressedContent = null;	// Clear compressed content as it will be repalced with our new thumbnail anyway

				// Create a tiny thumbnail from the image
				uint	thumbnailHeight = Mathf.Min( _imagesWebPage.Height * THUMBNAIL_WIDTH / _imagesWebPage.Width, THUMBNAIL_HEIGHT );	// At most our preferred ratio => we must crop the full page!
				float	imageScale = (float) _imagesWebPage.Width / THUMBNAIL_WIDTH;

				m_thumbnail = new ImageFile( THUMBNAIL_WIDTH, thumbnailHeight, PIXEL_FORMAT.BGR8, DEFAULT_PROFILE );	// Note that it's important to create a 24-bits format here to be able to save as JPEG!

				// Read "height" scanlines
				float4[]	sourceScanline = new float4[_imagesWebPage.Width];
				float4[]	targetScanline = new float4[THUMBNAIL_WIDTH];
				for ( uint Y=0; Y < thumbnailHeight; Y++ ) {
					uint	sourceY = (uint) (imageScale * Y);
					_imagesWebPage.ReadScanline( sourceY, sourceScanline );
					for ( uint X=0; X < THUMBNAIL_WIDTH; X++ ) {
						targetScanline[X] = sourceScanline[(uint) (imageScale * (X+0.5f))];
					}
					m_thumbnail.WriteScanline( Y, targetScanline );
				}

				// Notify?
				m_owner.NotifyThumbnailChanged( this );
			}

			public override void Read( BinaryReader _reader ) {
				// Read is performed asynchronously when "Content" is requested
			}

			public override void Write( BinaryWriter _writer ) {
				if ( m_compressedContent == null ) {
					// We need to compress content
					if ( m_thumbnail == null )
						throw new Exception( "Attempting to save an ampty thumbnail chunk! Don't create the chunk if it's normal to be empty..." );

					using ( NativeByteArray content = m_thumbnail.Save( ImageFile.FILE_FORMAT.JPEG, ImageFile.SAVE_FLAGS.SF_JPEG_FAST ) ) {
						m_compressedContent = content.AsByteArray;
					}
				}

				_writer.Write( m_compressedContent );
			}

			internal override void	Threaded_LoadContent( BinaryReader _reader, bool _prepareContent ) {
				try {
					m_compressedContent = new byte[m_size];
					_reader.Read( m_compressedContent, 0, (int) m_size );

					if ( _prepareContent ) {
						// Attempt to read the JPEG file
						ImageFile	temp = null;
						using ( NativeByteArray imageContent = new NativeByteArray( m_compressedContent ) ) {
							temp = new ImageFile( imageContent, ImageFile.FILE_FORMAT.JPEG );
						}

						// Replace current thumbnail
						m_thumbnail.Dispose();
						m_thumbnail = temp;
					}

				} catch ( Exception _e ) {
					m_owner.m_database.AsyncMain_LogError( "An error occurred while attempting to read thumbnail chunk for fiche \"" + m_owner.ToString() + "\": " + _e.Message );
				}
			}

			protected override void InternalDispose() {
				if ( m_thumbnail != null ) {
					m_thumbnail.Dispose();
					m_thumbnail = null;
				}
			}
		}

		#endregion

		#endregion

		#region FIELDS

		private FichesDB			m_database = null;

		private Guid				m_GUID;
		private DateTime			m_creationTime = DateTime.Now;

		private List< Fiche >		m_tags = new List< Fiche >();
		private List< Fiche >		m_references = new List< Fiche >();
		internal Guid[]				m_tagGUIDs = null;

		private TYPE				m_type = TYPE.REMOTE_ANNOTABLE_WEBPAGE;	// Default for fiches built from a URL
		private string				m_title = "";
		private Uri					m_URL = null;
		private string				m_HTMLContent = null;
		private DOMElement			m_rootElement = null;

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

					// Notify
					TitleChanged?.Invoke( this );
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
					HTMLContentChanged?.Invoke( this );
				} );
			}
		}

		public DOMElement			DOMElements {
			get { return m_rootElement; }
			set {
				Lock( STATUS.UPDATING, () => {
					if ( value == m_rootElement )
						return;

					m_rootElement = value;
					DOMElementsChanged?.Invoke( this );
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

		public Fiche[]				Tags {
			get { return m_tags.ToArray(); }
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
		/// Returns the captured web page images if the chunk exists
		/// </summary>
		/// <remarks>Will launch an asynchronous loading of the image and return a placeholder whenever the image is actually loaded and ready.
		/// You should subscribe to the WebPageImageChanged event to update the image once it's available</remarks>
		public ChunkWebPageSnapshot.WebPageImagePart[]	WebPageImages {
			get {
				ChunkWebPageSnapshot.WebPageImagePart[]	result = null;

				Lock( STATUS.ACCESSING, () => {
					ChunkWebPageSnapshot	chunk = FindChunkByType<ChunkWebPageSnapshot>();
					result = chunk != null ? chunk.Content as ChunkWebPageSnapshot.WebPageImagePart[] : null;
				} );

				return result;
			}
		}

		/// <summary>
		/// Returns the thumbnail for the captured web page image if the chunk exists
		/// </summary>
		/// <remarks>Will launch an asynchronous loading of the image and return a placeholder whenever the image is actually loaded and ready</remarks>
		public ImageFile			ThumbnailImage {
			get {
				ImageFile	result = null;

				Lock( STATUS.ACCESSING, () => {
					ChunkThumbnail	chunk = FindChunkByType<ChunkThumbnail>();
					result = chunk != null ? chunk.Content as ImageFile : null;
				} );

				return result;
			}
		}

		/// <summary>
		/// Raised whenever the fiche's title changed
		/// </summary>
		public event FicheEventHandler	TitleChanged;

		/// <summary>
		/// Raised whenever the web page image changed (not necessarily on the main thread!)
		/// </summary>
		public event FicheEventHandler	WebPageImageChanged;

		/// <summary>
		/// Raised whenever the thumbnail changed (not necessarily on the main thread!)
		/// </summary>
		public event FicheEventHandler	ThumbnailChanged;

		/// <summary>
		/// Raised whenever the web page HTML content changed (not necessarily on the main thread!)
		/// </summary>
		public event FicheEventHandler	HTMLContentChanged;

		/// <summary>
		/// Raised whenever the web page HTML content changed (not necessarily on the main thread!)
		/// </summary>
		public event FicheEventHandler	DOMElementsChanged;

		#endregion

		#region METHODS

// 		protected	Fiche( FichesDB _database, string _title ) {
// 			m_GUID = CreateGUID();
// 			m_title = _title;
// 
// 			Database = _database;
// 		}
		public	Fiche( FichesDB _database, TYPE _type, string _title, Uri _URL, Fiche[] _tags, string _HTMLContent ) {
			m_GUID = _database.CreateGUID();
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

		/// <summary>
		/// Internal and protected fiche creation
		/// Shouldn't be called manually, only the database should use this method
		/// </summary>
		/// <param name="_database"></param>
		internal Fiche( FichesDB _database ) {
			m_database = _database;
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

		/// <summary>
		/// Writes the entire fiche
		/// </summary>
		/// <param name="_writer"></param>
		/// <remarks>All chunks should have read their data and be ready to write their full content. Use the LastChanceReadBeforeWrite() to make sure your chunks (especially lazy-initialized chunks) are complete.</remarks>
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
				_writer.Write( m_rootElement != null );
				if ( m_rootElement != null ) {
					m_rootElement.Write( _writer );
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
			if ( _reader.ReadBoolean() ) {
				m_rootElement = new Brain2.DOMElement( _reader );
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
		/// Called with the existing fiche stream, just before we save new data and overwrite the old fiche
		/// Use this as the last opportunity to gather all the data your chunks need to get properly saved (this is especially true for lazy-initialized chunks that need to read the data they haven't read yet so they can save it again)
		/// </summary>
		/// <param name="_reader"></param>
		public void		LastChanceReadBeforeWrite( BinaryReader _reader ) {
			foreach ( ChunkBase chunk in m_chunks ) {
				if ( chunk.Size == 0 )
					continue;	// 0-length chunks are new and haven't been saved yet so they don't need to be loaded
				if ( chunk.IsFullyLoaded )
					continue;	// The chunk's content is already fully available so it can be saved immediately

				// Force loading the chunk's content immediately so we can save it right afterward...
				chunk.Threaded_LoadContent( _reader, false );
			}
		}

// 		/// <summary>
// 		/// Called as a post-process to finally resolve actual tag links after read
// 		/// </summary>
// 		/// <param name="_ID2Fiche"></param>
// 		public void		ResolveTags( Dictionary< Guid, Fiche > _ID2Fiche ) {
// 			foreach ( Guid parentID in m_tagGUIDs ) {
// 				Fiche	parent = null;
// 				if ( _ID2Fiche.TryGetValue( parentID, out parent ) )
// 					AddTag( parent );
// 			}
// 		}

		/// <summary>
		/// Resolve one of our missing tags
		/// </summary>
		/// <param name="_fiche"></param>
		public void		ResolveTag( Fiche _fiche ) {
			foreach ( Guid tagGUIID in m_tagGUIDs ) {
				if ( tagGUIID == _fiche.GUID ) {
					// Found it!
					AddTag( _fiche );
					return;
				}
			}

			throw new Exception( "Fiche is not one of our tags!" );
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
		/// <param name="_imagesWebPage"></param>
		internal void	CreateImageChunk( uint _imageStartIndex, Rectangle[] _contentRectangles, ImageFile[] _imagesWebPage, ImageFile.FILE_FORMAT _targetFormat ) {
			ChunkWebPageSnapshot	chunk = FindChunkByType<ChunkWebPageSnapshot>();
			if ( chunk == null ) {
				chunk = new ChunkWebPageSnapshot( this, _imageStartIndex, _contentRectangles, _imagesWebPage, _targetFormat );
			} else {
				chunk.UpdateImages( _imageStartIndex, _contentRectangles, _imagesWebPage, _targetFormat );
			}
		}

		// Create thumbnail chunk from the full webpage
		internal void	CreateThumbnailChunkFromImage( ImageFile _imagesWebPage ) {
			ChunkThumbnail	chunk = FindChunkByType<ChunkThumbnail>();
			if ( chunk == null ) {
				chunk = new ChunkThumbnail( this, _imagesWebPage );
			} else {
				chunk.UpdateFromWebPageImage( _imagesWebPage );
			}
		}

		/// <summary>
		/// Unloads the image chunk's image
		/// </summary>
		internal void	UnloadImageChunk() {
			ChunkWebPageSnapshot	chunk = FindChunkByType<ChunkWebPageSnapshot>();
			if ( chunk != null )
				chunk.UnloadImages();
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
			WebPageImageChanged?.Invoke( _caller.OwnerFiche );
		}

		private void	NotifyThumbnailChanged( ChunkThumbnail _caller ) {
			ThumbnailChanged?.Invoke( _caller.OwnerFiche );
		}

		#endregion
	}
}
