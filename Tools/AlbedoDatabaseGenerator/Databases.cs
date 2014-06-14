using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;

using WMath;

namespace AlbedoDatabaseGenerator
{
	public class	Database
	{
		#region NESTED TYPES

		[System.Diagnostics.DebuggerDisplay( "Name={m_FriendlyName} Desc={m_Description} Manifest={m_Manifest}" )]
		public class	Entry
		{
			#region NESTED TYPES

			public enum		TAGS_TYPE
			{
				NONE,
				WOOD,
				STONE,
				FABRIC,
				PAPER_CANVAS,
				PAINT,
//				METAL,
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

			public enum		TAGS_NATURE
			{
				NONE,
				NATURE,
				TABLE,
				CHAIR,
				WARDROBE,
				DESK,
				CABINET,
			}

			public enum		TAGS_FURNITURE
			{
				NONE,
				FURNITURE,
				TABLE,
				CHAIR,
				WARDROBE,
				DESK,
				CABINET,
			}

			public enum		TAGS_CONSTRUCTION
			{
				NONE,
				CONSTRUCTION,
				WALL,
				FLOOR,
				DOOR_WINDOW,
				ROAD_PAVEMENT,
			}

			[Flags]
			public enum		TAGS_MODIFIERS
			{
				NONE = 0,
				WET = 1,
				DUSTY = 2,
				FROSTY = 4,
				OLD = 8,
				NEW = 16,
			}

			#endregion

			#region FIELDS

			private string				m_RelativePath = null;
			private Manifest			m_Manifest = null;

			// User-fed data
			private string				m_FriendlyName = null;
			private string				m_Description = null;
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

			public Manifest		Manifest
			{
				get { return m_Manifest; }
				set { m_Manifest = value; }
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

			#endregion

			#region METHODS

			public override string ToString()
			{
				if ( m_FriendlyName != null )
					return m_FriendlyName;

				return Path.GetFileName( m_RelativePath );
			}

			public void		Load( Database _Owner, XmlElement _EntryElement )
			{
				m_RelativePath = _EntryElement.GetAttribute( "RelativePath" );
				m_FriendlyName = _EntryElement["FriendlyName"].GetAttribute( "Value" );
				m_Description = _EntryElement["FriendlyName"].GetAttribute( "Value" );
				
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

				XmlElement	TagsElement = _Owner.AppendElement( _EntryElement, "Tags" );
				_Owner.SetAttribute( _Owner.AppendElement( TagsElement, "Type" ), "Value", m_TagType.ToString() );
				_Owner.SetAttribute( _Owner.AppendElement( TagsElement, "Color" ), "Value", m_TagColor.ToString() );
				_Owner.SetAttribute( _Owner.AppendElement( TagsElement, "Shade" ), "Value", m_TagShade.ToString() );
				_Owner.SetAttribute( _Owner.AppendElement( TagsElement, "Nature" ), "Value", m_TagNature.ToString() );
				_Owner.SetAttribute( _Owner.AppendElement( TagsElement, "Furniture" ), "Value", m_TagFurniture.ToString() );
				_Owner.SetAttribute( _Owner.AppendElement( TagsElement, "Construction" ), "Value", m_TagConstruction.ToString() );
				_Owner.SetAttribute( _Owner.AppendElement( TagsElement, "Modifiers" ), "Value", m_TagModifiers.ToString() );
			}

			#endregion
		}

		[System.Diagnostics.DebuggerDisplay( "Name={m_SourceImageFileName} ISO={m_ISOSpeed} Shutter={m_ShutterSpeed} Aperture={m_Aperture}" )]
		public class	Manifest
		{
			#region NESTED TYPES

			[System.Diagnostics.DebuggerDisplay( "Name={m_ImageFileName} xyY={m_xyY} RGB={m_RGB}" )]
			public class	Swatch
			{
				public string	m_ImageFileName;
				public Vector	m_xyY;
				public Vector	m_RGB;
				public Vector2D	m_LocationTopLeft;
				public Vector2D	m_LocationBottomRight;

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
			}

			#endregion

			#region FIELDS

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

			#endregion

			#region METHODS

			public Manifest()
			{
			}

			public void		Load( FileInfo _FileName )
			{
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

			public void		GenerateThumbnail()
			{
				// TODO!
			}

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
					throw new Exception( "The provided database location does not exist on disk! Please change the location to an actual directory so entries should get reconnected to the texture manifests." );

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
							E = new Entry();
							E.RelativePath = GetRelativePath( m_RootPath.FullName, PotentialManifestFile.FullName );
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
			m_Entries.Clear();
			for ( int EntryIndex=0; EntryIndex < EntriesCount; EntryIndex++ )
			{
				try
				{
					XmlElement	EntryElement = Root.ChildNodes[EntryIndex] as XmlElement;
					Entry	E = new Entry();
					E.Load( this, EntryElement );
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
			string	RelativeFileName = GetRelativePath( m_RootPath.FullName, _ManifestFileName.FullName ).ToLower();
			foreach ( Entry E in m_Entries )
				if ( E.RelativePath.ToLower() == RelativeFileName )
					return E;	// Gotcha!

			return null;
		}

		/// <summary>
		/// From http://stackoverflow.com/questions/703281/getting-path-relative-to-the-current-working-directory
		/// </summary>
		/// <param name="filespec"></param>
		/// <param name="folder"></param>
		/// <returns></returns>
		string GetRelativePath( string folder, string filespec )
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

		#endregion
	}
}
