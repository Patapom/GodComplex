using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;

namespace AlbedoDatabaseGenerator
{
	public class	Database
	{
		#region NESTED TYPES

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
			public TAGS_NATURE			TagNature		{ get { return m_TagNature; } set { m_TagNature = value; } }
			public TAGS_FURNITURE		TagFurniture	{ get { return m_TagFurniture; } set { m_TagFurniture = value; } }
			public TAGS_CONSTRUCTION	TagConstruction	{ get { return m_TagConstruction; } set { m_TagConstruction = value; } }
			public TAGS_MODIFIERS		TagModifiers	{ get { return m_TagModifiers; } set { m_TagModifiers = value; } }

			#endregion

			#region METHODS

			#endregion
		}

		public class	Manifest
		{
			#region FIELDS

			#endregion

			#region PROPERTIES

			#endregion

			#region METHODS

			public Manifest()
			{

			}

			public void		Load( FileInfo _FileName )
			{

			}

			public void		GenerateThumbnail
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

				m_Errors = "";

				// Reconnect all manifests
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

		#endregion

		#region METHODS

		public	Database()
		{

		}

		public void	Load( FileInfo _FileName )
		{

		}

		public void	Save( FileInfo _FileName )
		{

		}

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
		/// From stack overflow
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
