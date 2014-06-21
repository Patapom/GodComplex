using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Drawing.Imaging;

using WMath;

namespace AlbedoDatabaseGenerator
{
	public class	Database : IDisposable
	{
		#region NESTED TYPES

		public class InvalidDatabaseRootPathException : Exception
		{
			public InvalidDatabaseRootPathException( string _Message ) : base( _Message )
			{

			}
		}

		#endregion

		#region CONSTANTS

		private static readonly string	EOL = "\r\n";

		#endregion

		#region NESTED TYPES

		[System.Diagnostics.DebuggerDisplay( "File={m_RelativePath} Name={m_FriendlyName} Desc={m_Description}" )]
		public class	Entry : IDisposable
		{
			#region CONSTANTS

			public const int	THUMBNAIL_WIDTH = 256;

			#endregion

			#region NESTED TYPES

			public enum		TAGS_TYPE
			{
				NONE,
				WOOD,
				STONE,
				SKIN,
				FABRIC,
				PAPER_CANVAS,
				PAINT,
				PLASTIC,
			}

			public enum		TAGS_COLOR
			{
				NONE,
				BLACK,
				WHITE,
				GRAY,
				RED,
				GREEN,
				BLUE,
				YELLOW,
				CYAN,
				PURPLE,
				ORANGE,
			}

			public enum		TAGS_SHADE
			{
				NONE,
				DARK,
				BRIGHT,
				NEUTRAL,
			}

			[Flags]
			public enum		TAGS_NATURE
			{
				NONE = 0,
				NATURE = 256,
				LEAF = 1 | NATURE,
				SOIL = 2 | NATURE,
				BARK = 3 | NATURE,
			}

			[Flags]
			public enum		TAGS_FURNITURE
			{
				NONE = 0,
				FURNITURE = 256,
				TABLE = 1 | FURNITURE,
				CHAIR = 2 | FURNITURE,
				DESK = 3 | FURNITURE,
				WARDROBE = 4 | FURNITURE,
				CABINET = 5 | FURNITURE,
			}

			[Flags]
			public enum		TAGS_CONSTRUCTION
			{
				NONE = 0,
				CONSTRUCTION = 256,
				WALL = 1 | CONSTRUCTION,
				FLOOR = 2 | CONSTRUCTION,
				DOOR_WINDOW = 3 | CONSTRUCTION,
				ROAD_PAVEMENT = 4 | CONSTRUCTION,
			}

			[Flags]
			public enum		TAGS_MODIFIERS
			{
				NONE = 0,
				WET = 1,
				DUSTY = 2,
				FROSTY = 4,
				VARNISHED = 8,
				OLD = 16,
				NEW = 32,
			}

			#endregion

			#region FIELDS

			private Database			m_Owner = null;

			private string				m_RelativePath = null;
			private Manifest			m_Manifest = null;

			private Bitmap				m_Thumbnail = null;
			private Bitmap				m_OverviewImage = null;

			// User-fed data
			private string				m_FriendlyName = null;
			private string				m_Description = null;
			private string				m_OverviewImageRelativePath = null;
			private TAGS_TYPE			m_TagType = TAGS_TYPE.NONE;
			private TAGS_COLOR			m_TagColor = TAGS_COLOR.NONE;
			private TAGS_SHADE			m_TagShade = TAGS_SHADE.NONE;
			private TAGS_NATURE			m_TagNature = TAGS_NATURE.NONE;
			private TAGS_FURNITURE		m_TagFurniture = TAGS_FURNITURE.NONE;
			private TAGS_CONSTRUCTION	m_TagConstruction = TAGS_CONSTRUCTION.NONE;
			private TAGS_MODIFIERS		m_TagModifiers = TAGS_MODIFIERS.NONE;

			#endregion

			#region PROPERTIES

			public string		RelativePath
			{
				get { return m_RelativePath; }
				set { m_RelativePath = value; }
			}

			public FileInfo		FullPath
			{
				get { return new FileInfo( Path.Combine( m_Owner.RootPath.FullName, m_RelativePath ) ); }
			}

			public Manifest		Manifest
			{
				get { return m_Manifest; }
				set
				{
					m_Manifest = value;

					if ( m_Manifest == null )
						return;

					// Attempt to reload thumbnail & overview image
					FileInfo	ThumbnailFileName = new FileInfo( m_Manifest.GetFullPath( Path.GetFileNameWithoutExtension( m_Manifest.m_CalibratedTextureFileName ) + ".jpg" ) );
					if ( ThumbnailFileName.Exists )
						m_Thumbnail = Bitmap.FromFile( ThumbnailFileName.FullName ) as Bitmap;

					FileInfo	OverviewFileName = OverviewImageFileName;
					if ( OverviewFileName != null && OverviewFileName.Exists )
						m_OverviewImage = Bitmap.FromFile( OverviewFileName.FullName ) as Bitmap;
				}
			}

			public string				FriendlyName	{ get { return m_FriendlyName; } set { m_FriendlyName = value; } }
 			public string				Description		{ get { return m_Description; } set { m_Description = value; } }
			public TAGS_TYPE			TagType			{ get { return m_TagType; } set { m_TagType = value; } }
			public TAGS_COLOR			TagColor		{ get { return m_TagColor; } set { m_TagColor = value; } }
			public TAGS_SHADE			TagShade		{ get { return m_TagShade; } set { m_TagShade = value; } }
			public TAGS_NATURE			TagNature		{ get { return m_TagNature; } set { m_TagNature = value; } }
			public TAGS_FURNITURE		TagFurniture	{ get { return m_TagFurniture; } set { m_TagFurniture = value; } }
			public TAGS_CONSTRUCTION	TagConstruction	{ get { return m_TagConstruction; } set { m_TagConstruction = value; } }
			public TAGS_MODIFIERS		TagModifiers	{ get { return m_TagModifiers; } set { m_TagModifiers = value; } }

			FileInfo					ThumbnailFileName
			{
				get { return new FileInfo( m_Manifest.GetFullPath( Path.GetFileNameWithoutExtension( m_Manifest.m_CalibratedTextureFileName ) + ".jpg" ) ); }
			}

 			public FileInfo				OverviewImageFileName
			{
				get { return m_OverviewImageRelativePath != null ? new FileInfo( Path.Combine( Path.GetDirectoryName( FullPath.FullName ), m_OverviewImageRelativePath ) ) : null; }
				set
				{
					m_OverviewImageRelativePath = value != null ? GetRelativePath( Path.GetDirectoryName( FullPath.FullName ), value.FullName ) : null;

					// Load the bitmap
					if ( m_OverviewImage != null )
						m_OverviewImage.Dispose();
					m_OverviewImage = null;

					if ( m_OverviewImageRelativePath != null )
						m_OverviewImage = Bitmap.FromFile( value.FullName ) as Bitmap;
				}
			}

			public Bitmap				Thumbnail			{ get { return m_Thumbnail; } }
			public Bitmap				OverviewImage		{ get { return m_OverviewImage; } }

			#endregion

			#region METHODS

			public Entry( Database _Owner )
			{
				m_Owner = _Owner;
			}

			public override string ToString()
			{
				if ( m_FriendlyName != null && m_FriendlyName != "" )
					return m_FriendlyName;

				return Path.GetFileName( m_RelativePath );
			}

			public void		Load( Database _Owner, XmlElement _EntryElement )
			{
				m_RelativePath = _EntryElement.GetAttribute( "RelativePath" );
				m_FriendlyName = _EntryElement["FriendlyName"].GetAttribute( "Value" );
				m_Description = _EntryElement["Description"].GetAttribute( "Value" );

				if ( _EntryElement["EnvironmentImage"] != null )
					m_OverviewImageRelativePath = _EntryElement["EnvironmentImage"].GetAttribute( "RelativePath" );
				
				XmlElement	TagsElement = _EntryElement["Tags"];
				if ( TagsElement == null )
					throw new Exception( "Failed to retrieve \"Tags\" element!" );

				m_TagType = (TAGS_TYPE) Enum.Parse( typeof(TAGS_TYPE), TagsElement["Type"].GetAttribute( "Value" ) );
				m_TagColor = (TAGS_COLOR) Enum.Parse( typeof(TAGS_COLOR), TagsElement["Color"].GetAttribute( "Value" ) );
				m_TagShade = (TAGS_SHADE) Enum.Parse( typeof(TAGS_SHADE), TagsElement["Shade"].GetAttribute( "Value" ) );
				m_TagNature = (TAGS_NATURE) Enum.Parse( typeof(TAGS_NATURE), TagsElement["Nature"].GetAttribute( "Value" ) );
				m_TagFurniture = (TAGS_FURNITURE) Enum.Parse( typeof(TAGS_FURNITURE), TagsElement["Furniture"].GetAttribute( "Value" ) );
				m_TagConstruction = (TAGS_CONSTRUCTION) Enum.Parse( typeof(TAGS_CONSTRUCTION), TagsElement["Construction"].GetAttribute( "Value" ) );
				m_TagModifiers = (TAGS_MODIFIERS) Enum.Parse( typeof(TAGS_MODIFIERS), TagsElement["Modifiers"].GetAttribute( "Value" ) );
			}

			public void		Save( Database _Owner, XmlElement _EntryElement )
			{
				_Owner.SetAttribute( _EntryElement, "RelativePath", m_RelativePath );
				_Owner.SetAttribute( _Owner.AppendElement( _EntryElement, "FriendlyName" ), "Value", m_FriendlyName );
				_Owner.SetAttribute( _Owner.AppendElement( _EntryElement, "Description" ), "Value", m_Description );

				if ( m_OverviewImageRelativePath != null )
					_Owner.SetAttribute( _Owner.AppendElement( _EntryElement, "EnvironmentImage" ), "RelativePath", m_OverviewImageRelativePath );

				XmlElement	TagsElement = _Owner.AppendElement( _EntryElement, "Tags" );
				_Owner.SetAttribute( _Owner.AppendElement( TagsElement, "Type" ), "Value", m_TagType.ToString() );
				_Owner.SetAttribute( _Owner.AppendElement( TagsElement, "Color" ), "Value", m_TagColor.ToString() );
				_Owner.SetAttribute( _Owner.AppendElement( TagsElement, "Shade" ), "Value", m_TagShade.ToString() );
				_Owner.SetAttribute( _Owner.AppendElement( TagsElement, "Nature" ), "Value", m_TagNature.ToString() );
				_Owner.SetAttribute( _Owner.AppendElement( TagsElement, "Furniture" ), "Value", m_TagFurniture.ToString() );
				_Owner.SetAttribute( _Owner.AppendElement( TagsElement, "Construction" ), "Value", m_TagConstruction.ToString() );
				_Owner.SetAttribute( _Owner.AppendElement( TagsElement, "Modifiers" ), "Value", m_TagModifiers.ToString() );
			}

			private string	m_Indent = "";
			public string	Export( string _Indent )
			{
				m_Indent = _Indent;
				string	JSON = L( "{" );

				// Write standard infos
				Indent();
				JSON += L( "RelativePath : \"" + Path.GetDirectoryName( m_RelativePath ).Replace( '\\', '/' ) + "\"," );
				JSON += L( "FriendlyName : \"" + m_FriendlyName + "\"," );
				JSON += L( "Description : \"" + m_Description + "\"," );
				if ( m_OverviewImageRelativePath != null )
					JSON += L( "OverviewImagePath: \"" + m_OverviewImageRelativePath + "\"," );

				string	Tags = "";
				if ( m_TagType != TAGS_TYPE.NONE )
					Tags += "," + m_TagType.ToString();
				if ( m_TagColor != TAGS_COLOR.NONE )
					Tags += "," + m_TagColor.ToString();
				if ( m_TagShade != TAGS_SHADE.NONE )
					Tags += "," + m_TagShade.ToString();
				if ( m_TagNature != TAGS_NATURE.NONE )
					Tags += "," + m_TagNature.ToString()
						+ (m_TagNature != TAGS_NATURE.NATURE ? ",NATURE" : "");	// Always append parent tag as soon as we have a tag belonging to the category
				if ( m_TagFurniture != TAGS_FURNITURE.NONE )
					Tags += "," + m_TagFurniture.ToString()
						+ (m_TagFurniture != TAGS_FURNITURE.FURNITURE ? ",FURNITURE" : "");	// Always append parent tag as soon as we have a tag belonging to the category
				if ( m_TagConstruction != TAGS_CONSTRUCTION.NONE )
					Tags += "," + m_TagConstruction.ToString()
						+ (m_TagConstruction != TAGS_CONSTRUCTION.CONSTRUCTION ? ",CONSTRUCTION" : "");	// Always append parent tag as soon as we have a tag belonging to the category
				if ( m_TagModifiers != TAGS_MODIFIERS.NONE )
					Tags += "," + m_TagModifiers.ToString();
				if ( Tags != "" )
					Tags = Tags.Remove( 0, 1 );	// Remove first comma
				JSON += L( "Tags : \"" + Tags + "\"," );

				// Write texture infos
				if ( m_Manifest != null )
				{
					JSON += L( "TextureInfos : {" );
					Indent();

					JSON += L( "ISOSpeed : " + m_Manifest.m_ISOSpeed + "," );
					JSON += L( "ShutterSpeed : " + m_Manifest.m_ShutterSpeed + "," );
					JSON += L( "Aperture : " + m_Manifest.m_Aperture + "," );

					JSON += L( "ThumbnailFileName : \"" + F( ThumbnailFileName.FullName ) + "\"," );
					JSON += L( "TextureFileName : \"" + F( m_Manifest.GetFullPath( m_Manifest.m_CalibratedTextureFileName ) ) + "\"," );
					JSON += L( "TextureWidth : " + m_Manifest.m_CalibratedTextureWidth + "," );
					JSON += L( "TextureHeight : " + m_Manifest.m_CalibratedTextureHeight + "," );
					if ( OverviewImageFileName != null )
						JSON += L( "OverviewImageFileName : \"" + F( OverviewImageFileName.FullName ) + "\"," );
					
					{	// Swatches
						JSON += L( "Swatches : {" );
						Indent();

						JSON += L( "Min : " ) + ExportSwatch( m_Manifest.m_SwatchMin );
						JSON += L( "Max : " ) + ExportSwatch( m_Manifest.m_SwatchMax );
						JSON += L( "Avg : " ) + ExportSwatch( m_Manifest.m_SwatchAvg );

						{	// Custom
							JSON += L( "Custom : [" );
							Indent();
							foreach ( Manifest.Swatch CS in m_Manifest.m_CustomSwatches )
								JSON += ExportSwatch( CS );
							UnIndent();
							JSON += L( "]" );
						}

						UnIndent();
						JSON += L( "}" );
					}

					UnIndent();
					JSON += L( "}" );
				}

				UnIndent();
				JSON += L( "}," );
				return JSON;
			}

			private void	Indent()
			{
				m_Indent += "	";
			}
			private void	UnIndent()
			{
				m_Indent = m_Indent.Remove( m_Indent.Length-1, 1 );
			}
			private string	L( string _Line )
			{
				return m_Indent + _Line + EOL;
			}
			private string	F( string _FullPath )
			{
				string	RelativeFileName = m_Owner.GetRelativePath( _FullPath );
				RelativeFileName = RelativeFileName.Replace( '\\', '/' );
				return RelativeFileName;
			}
			private string	ExportSwatch( Manifest.Swatch _Swatch )
			{
				string	JSON = L( "{" );
				Indent();

				JSON += L( "xyY : \"" + _Swatch.m_xyY.ToString() + "\"," );
				JSON += L( "RGB : \"" + _Swatch.m_RGB.ToString() + "\"," );
				JSON += L( "Color : \"" + (_Swatch.Color.ToArgb() & 0xFFFFFF).ToString( "X6" ) + "\"," );

				if ( _Swatch.m_LocationTopLeft != null )
					JSON += L( "TopLeft : { x : " + _Swatch.m_LocationTopLeft.x + ", y : " + _Swatch.m_LocationTopLeft.y + " }," );
				if ( _Swatch.m_LocationBottomRight != null )
					JSON += L( "BottomRight : { x : " + _Swatch.m_LocationBottomRight.x + ", y : " + _Swatch.m_LocationBottomRight.y + " }," );

				UnIndent();
				JSON += L( "}," );

				return JSON;
			}

			public unsafe void		GenerateThumbnail( bool _ForceRegenerate )
			{
				if ( m_Thumbnail != null && !_ForceRegenerate )
					return;	// Already generated

				if ( m_Manifest == null )
					throw new Exception( "Invalid manifest to load textures from!" );

				// Attempt to load an existing thumbnail
				FileInfo	FileName = ThumbnailFileName;
				if ( FileName.Exists )
				{
					if ( !_ForceRegenerate )
					{	// It does!
						m_Thumbnail = Bitmap.FromFile( FileName.FullName ) as Bitmap;
						return;
					}

					// Erase existing thumbnail
					if ( m_Thumbnail != null )
						m_Thumbnail.Dispose();

					FileName.Delete();
				}

				// Make sure the textures are ready
				m_Manifest.LoadTextures();

				Bitmap		Source = m_Manifest.m_Texture;
				BitmapData	LockedBitmap = Source.LockBits( new Rectangle( 0, 0, Source.Width, Source.Height ), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb );

				int			TargetHeight = THUMBNAIL_WIDTH * Source.Height / Source.Width;
				Bitmap		Target = new Bitmap( THUMBNAIL_WIDTH, TargetHeight, PixelFormat.Format32bppArgb );
				BitmapData	LockedBitmap2 = Target.LockBits( new Rectangle( 0, 0, Target.Width, Target.Height ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );

				for ( int Y=0; Y < TargetHeight; Y++ )
				{
					byte*	pScanlineSource = (byte*) LockedBitmap.Scan0.ToPointer() + (Y * Source.Height / TargetHeight) * LockedBitmap.Stride;
					byte*	pScanlineTarget = (byte*) LockedBitmap2.Scan0.ToPointer() + Y * LockedBitmap2.Stride;
					for ( int X=0; X < THUMBNAIL_WIDTH; X++ )
					{
						byte*	pPixelSource = pScanlineSource + (X * Source.Width / THUMBNAIL_WIDTH) * 4;
						*pScanlineTarget++ = *pPixelSource++;
						*pScanlineTarget++ = *pPixelSource++;
						*pScanlineTarget++ = *pPixelSource++;
						*pScanlineTarget++ = *pPixelSource++;
					}
				}

				Target.UnlockBits( LockedBitmap2 );
				Source.UnlockBits( LockedBitmap );

				// We have a new thumbnail!
				m_Thumbnail = Target;
				m_Thumbnail.Save( FileName.FullName, ImageFormat.Jpeg );
			}

			#region IDisposable Members

			public void Dispose()
			{
				if ( m_Manifest != null )
					m_Manifest.Dispose();
				if ( m_Thumbnail != null )
					m_Thumbnail.Dispose();
				if ( m_OverviewImage != null )
					m_OverviewImage.Dispose();
			}

			#endregion

			#endregion
		}

		[System.Diagnostics.DebuggerDisplay( "Name={m_SourceImageFileName} ISO={m_ISOSpeed} Shutter={m_ShutterSpeed} Aperture={m_Aperture}" )]
		public class	Manifest : IDisposable
		{
			#region NESTED TYPES

			[System.Diagnostics.DebuggerDisplay( "Name={m_ImageFileName} xyY={m_xyY} RGB={m_RGB}" )]
			public class	Swatch : IDisposable
			{
				public string	m_ImageFileName;
				public Vector	m_xyY;
				public Vector	m_RGB;
				public Vector2D	m_LocationTopLeft;
				public Vector2D	m_LocationBottomRight;

				public Bitmap	m_Texture = null;

				public Color	Color
				{
					get
					{
						return Color.FromArgb(
							(int) Math.Max( 0, Math.Min( 255, 255 * m_RGB.x ) ),
							(int) Math.Max( 0, Math.Min( 255, 255 * m_RGB.y ) ),
							(int) Math.Max( 0, Math.Min( 255, 255 * m_RGB.z ) )
							);
					}
				}

				public void		Load( XmlElement _Element )
				{
					m_ImageFileName = _Element.GetAttribute( "Name" );

					m_xyY = Vector.Parse( _Element.GetAttribute( "xyY" ) );
					m_RGB = Vector.Parse( _Element.GetAttribute( "RGB" ) );

					if ( _Element.GetAttribute( "SampleTopLeft" ) != "" )
						m_LocationTopLeft = Vector2D.Parse( _Element.GetAttribute( "SampleTopLeft" ) );
					if ( _Element.GetAttribute( "SampleBottomRight" ) != "" )
						m_LocationBottomRight = Vector2D.Parse( _Element.GetAttribute( "SampleBottomRight" ) );
				}

				public void	LoadTexture( Manifest _Owner )
				{
					if ( m_Texture == null )
						m_Texture = Bitmap.FromFile( _Owner.GetFullPath( m_ImageFileName ) ) as Bitmap;
				}

				#region IDisposable Members

				public void Dispose()
				{
					if ( m_Texture != null )
						m_Texture.Dispose();
				}

				#endregion
			}

			#endregion

			#region FIELDS

			public FileInfo		m_ManifestFileName = null;

			// Source infos
			public string		m_SourceImageFileName;
			public float		m_ISOSpeed;
			public float		m_ShutterSpeed;
			public float		m_Aperture;

			public bool			m_SpatialCorrectionEnabled;
			public float		m_WhiteBalanceCorrectionFactor;
			public Vector		m_WhiteBalancexyY;

			public int			m_SwatchesWidth;
			public int			m_SwatchesHeight;

			public string		m_TargetFormat;

			// Calibrated texture infos
			public string		m_CalibratedTextureFileName;
			public int			m_CalibratedTextureWidth;
			public int			m_CalibratedTextureHeight;

			public Swatch		m_SwatchMin = new Swatch();
			public Swatch		m_SwatchMax = new Swatch();
			public Swatch		m_SwatchAvg = new Swatch();

			// Custom swatches infos
			public Swatch[]		m_CustomSwatches = new Swatch[0];

			// Texture
			public Bitmap		m_Texture = null;

			#endregion

			#region METHODS

			public Manifest()
			{
			}

			public void		Load( FileInfo _FileName )
			{
				m_ManifestFileName = _FileName;

				XmlDocument	Doc = new XmlDocument();
				Doc.Load( _FileName.FullName );

				XmlElement	Root = Doc["Manifest"];
				if ( Root == null )
					throw new Exception( "Failed to retrieve the root \"Manifest\" node. Is this a manifest file?" );

				// Load source image infos
				XmlElement	SourceInfosElement = Root["SourceInfos"];
				if ( SourceInfosElement == null )
					throw new Exception( "Failed to retrieve the \"SourceInfos\" element!" );
				m_SourceImageFileName = SourceInfosElement["SourceImageName"].GetAttribute( "Value" );
				m_ISOSpeed = float.Parse( SourceInfosElement["ISOSpeed"].GetAttribute( "Value" ) );
				m_ShutterSpeed = float.Parse( SourceInfosElement["ShutterSpeed"].GetAttribute( "Value" ) );
				m_Aperture = float.Parse( SourceInfosElement["Aperture"].GetAttribute( "Value" ) );

				m_SpatialCorrectionEnabled = SourceInfosElement["SpatialCorrection"].GetAttribute( "Status" ) == "Enabled";
				m_WhiteBalanceCorrectionFactor = float.Parse( SourceInfosElement["WhiteReflectanceCorrectionFactor"].GetAttribute( "Value" ) );
				if ( SourceInfosElement["WhiteBalance"] != null )
					m_WhiteBalancexyY = Vector.Parse( SourceInfosElement["WhiteBalance"].GetAttribute( "xyY" ) );

				m_SwatchesWidth = int.Parse( SourceInfosElement["SwatchesSize"].GetAttribute( "Width" ) );
				m_SwatchesHeight = int.Parse( SourceInfosElement["SwatchesSize"].GetAttribute( "Height" ) );

				m_TargetFormat = SourceInfosElement["TargetFormat"].GetAttribute( "Value" );

				// Load calibrated texture infos
				XmlElement	CalibratedTextureElement = Root["CalibratedTexture"];
				if ( CalibratedTextureElement == null )
					throw new Exception( "Failed to retrieve the \"CalibratedTexture\" element!" );
				m_CalibratedTextureFileName = CalibratedTextureElement.GetAttribute( "Name" );
				m_CalibratedTextureWidth = int.Parse( CalibratedTextureElement.GetAttribute( "Width" ) );
				m_CalibratedTextureHeight = int.Parse( CalibratedTextureElement.GetAttribute( "Height" ) );

				XmlElement	DefaultSwatchesElement = CalibratedTextureElement["DefaultSwatches"];
				m_SwatchMin.Load( DefaultSwatchesElement["Min"] );
				m_SwatchMax.Load( DefaultSwatchesElement["Max"] );
				m_SwatchAvg.Load( DefaultSwatchesElement["Avg"] );

				// Load custom swatches
				XmlElement	CustomSwatchesElement = Root["CustomSwatches"];
				if ( CustomSwatchesElement != null )
				{
					int	CustomSwatchesCount = int.Parse( CustomSwatchesElement.GetAttribute( "Count" ) );
					m_CustomSwatches = new Swatch[CustomSwatchesCount];
					for ( int CustomSwatchIndex=0; CustomSwatchIndex < CustomSwatchesCount; CustomSwatchIndex++ )
					{
						m_CustomSwatches[CustomSwatchIndex] = new Swatch();
						XmlElement	CustomSwatchElement = CustomSwatchesElement["Custom" + CustomSwatchIndex];
						if ( CustomSwatchElement == null )
							throw new Exception( "Failed to retrieve custom swatch element!" );
						m_CustomSwatches[CustomSwatchIndex].Load( CustomSwatchElement );
					}
				}
			}

			/// <summary>
			/// Loads the textures described by the manifest
			/// </summary>
			public void		LoadTextures()
			{
				if ( m_Texture == null )
					m_Texture = Bitmap.FromFile( GetFullPath( m_CalibratedTextureFileName ) ) as Bitmap;

				m_SwatchMin.LoadTexture( this );
				m_SwatchMax.LoadTexture( this );
				m_SwatchAvg.LoadTexture( this );
				foreach ( Swatch CS in m_CustomSwatches )
					CS.LoadTexture( this );
			}

			public string	GetFullPath( string _RelativeFileName )
			{
				return Path.Combine( m_ManifestFileName.DirectoryName, _RelativeFileName );
			}

			#region IDisposable Members

			public void Dispose()
			{
				if ( m_Texture != null )
					m_Texture.Dispose();
				m_SwatchMin.Dispose();
				m_SwatchMax.Dispose();
				m_SwatchAvg.Dispose();
				foreach ( Swatch CS in m_CustomSwatches )
					CS.Dispose();
			}

			#endregion

			#endregion
		}

		#endregion

		#region FIELDS

		private DirectoryInfo	m_RootPath = null;
		private List<Entry>		m_Entries = new List<Entry>();

		private string			m_Errors = "";

		#endregion

		#region PROPERTIES

		public DirectoryInfo	RootPath
		{
			get { return m_RootPath; }
			set
			{
				if ( value == null )
					return;

				m_RootPath = value;
				if ( !m_RootPath.Exists )
					throw new InvalidDatabaseRootPathException( "The provided database location does not exist on disk! Please change the location to an actual directory so entries should get reconnected to the texture manifests." );

				m_Errors = "";

				// Reconnect all manifests to our existing entries
				foreach ( Entry E in m_Entries )
					E.Manifest = null;	// Clear manifest

				FileInfo[]	PotentialManifestFiles = m_RootPath.GetFiles( "*.xml", SearchOption.AllDirectories );
				foreach ( FileInfo PotentialManifestFile in PotentialManifestFiles )
				{
					try
					{
						Manifest	M = new Manifest();
						M.Load( PotentialManifestFile );

						Entry	E = FindEntry( PotentialManifestFile );
						if ( E == null )
						{	// Create a new entry for our database
							E = new Entry( this );
							E.RelativePath = GetRelativePath( PotentialManifestFile.FullName );
							m_Entries.Add( E );
						}
						E.Manifest = M;
					}
					catch ( Exception _e )
					{
						m_Errors += "An error occurred while loading manifest file \"" + PotentialManifestFile.FullName + "\": " + _e.Message;
					}
				}
			}
		}

		public Entry[]	Entries
		{
			get { return m_Entries.ToArray(); }
		}

		public bool		HasErrors	{ get { return m_Errors != ""; } }
		public string	Errors		{ get { return m_Errors; } }

		#endregion

		#region METHODS

		public	Database()
		{

		}

		public void	Load( FileInfo _FileName )
		{
			XmlDocument	Doc = new XmlDocument();
			Doc.Load( _FileName.FullName );

			XmlElement	Root = Doc["TexturesDatabase"];
			if ( Root == null )
				throw new Exception( "Failed to retrieve the root \"TextureDatabase\" element! Not a valid database file?" );

			string	Location = Root.GetAttribute( "Location" );
			if ( Location == "" )
				throw new Exception( "Failed to retrieve the location path for the database!" );

			int	EntriesCount = 0;
			if ( !int.TryParse( Root.GetAttribute( "EntriesCount" ), out EntriesCount ) )
				throw new Exception( "Failed to parse amount of entries in the database!" );

			m_Errors = "";

			foreach ( Entry E in m_Entries )
				E.Dispose();
			m_Entries.Clear();
			for ( int EntryIndex=0; EntryIndex < EntriesCount; EntryIndex++ )
			{
				try
				{
					XmlElement	EntryElement = Root.ChildNodes[EntryIndex] as XmlElement;
					Entry	E = new Entry( this );
					E.Load( this, EntryElement );
					m_Entries.Add( E );
				}
				catch ( Exception _e )
				{
					m_Errors += "An error occurred while loading database entry #" + EntryIndex + ": " + _e.Message;
				}
			}

			// Reconnect manifests found from the specified location
			RootPath = new DirectoryInfo( Location );
		}

		public void	Save( FileInfo _FileName )
		{
			XmlDocument	Doc = new XmlDocument();

			XmlElement	Root = Doc.CreateElement( "TexturesDatabase" );
			Doc.AppendChild( Root );

			SetAttribute( Root, "Location", m_RootPath.FullName );

			SetAttribute( Root, "EntriesCount", m_Entries.Count.ToString() );
			foreach ( Entry E in m_Entries )
			{
				XmlElement	EntryElement = AppendElement( Root, "Entry" );
				E.Save( this, EntryElement );
			}

			Doc.Save( _FileName.FullName );
		}

		/// <summary>
		/// Exports the database to a JSON file readable by the javascript web page that will help browsing the textures
		/// </summary>
		/// <param name="_FileName"></param>
		public void		Export( FileInfo _FileName )
		{
			string	JSON = "{"+EOL;
			JSON += "	Entries : ["+EOL;
			foreach ( Entry E in m_Entries )
				JSON += E.Export( "		" );
			JSON += "	]"+EOL;
			JSON += "}"+EOL;

			using ( StreamWriter W = _FileName.CreateText() )
				W.Write( JSON );
		}

		#region Xml Helpers

		private XmlElement	AppendElement( XmlNode _ParentNode, string _ElementName )
		{
			XmlElement	E = _ParentNode.OwnerDocument.CreateElement( _ElementName );
			_ParentNode.AppendChild( E );
			return E;
		}
		private XmlElement	m_CurrentElement = null;
		private Database	SetAttribute( XmlElement _Element, string _Attribute, string _Value )
		{
			m_CurrentElement = _Element;
			m_CurrentElement.SetAttribute( _Attribute, _Value );
			return this;
		}
		private Database	SetAttribute( string _Attribute, string _Value )
		{
			m_CurrentElement.SetAttribute( _Attribute, _Value );
			return this;
		}

		#endregion

		/// <summary>
		/// Attempts to retrieve an existing entry from the provided manifest file name
		/// </summary>
		/// <param name="_ManifestFileName"></param>
		/// <returns></returns>
		private Entry	FindEntry( FileInfo _ManifestFileName )
		{
			string	RelativeFileName = GetRelativePath( _ManifestFileName.FullName ).ToLower();
			foreach ( Entry E in m_Entries )
				if ( E.RelativePath.ToLower() == RelativeFileName )
					return E;	// Gotcha!

			return null;
		}

		public string GetRelativePath( string filespec )
		{
			return GetRelativePath( m_RootPath.FullName, filespec );
		}

		/// <summary>
		/// From http://stackoverflow.com/questions/703281/getting-path-relative-to-the-current-working-directory
		/// </summary>
		/// <param name="filespec"></param>
		/// <param name="folder"></param>
		/// <returns></returns>
		public static string GetRelativePath( string folder, string filespec )
		{
			Uri pathUri = new Uri(filespec);
			// Folders must end in a slash
			if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				folder += Path.DirectorySeparatorChar;
			}
			Uri folderUri = new Uri(folder);
			return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
		}

		#region IDisposable Members

		public void Dispose()
		{
			foreach ( Entry E in m_Entries )
				E.Dispose();
		}

		#endregion

		#endregion
	}
}
