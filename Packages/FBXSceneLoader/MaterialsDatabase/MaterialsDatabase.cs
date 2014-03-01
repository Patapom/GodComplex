using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using WMath;
using FBX.SceneLoader.Objects;

namespace FBX.SceneLoader
{
	/// <summary>
	/// This materials database is a collection of materials that reference textures and parameters for each material
	/// </summary>
	public class	MaterialsDatabase
	{
		#region NESTED TYPES

		[System.Diagnostics.DebuggerDisplay( "Name={m_Name} File={m_SourceM2File.Name}" )]
		public class	Material
		{
			#region FIELDS

			protected FileInfo		m_SourceM2File = null;
			protected string		m_Name = null;

			// Textures referenced by the material
			protected string		m_TextureDiffuse = null;
			protected string		m_TextureNormal = null;
			protected string		m_TextureSpecular = null;

			#endregion

			#region PROPERTIES

			public FileInfo			SourceM2File	{ get { return m_SourceM2File; } }
			public string			Name			{ get { return m_Name; } }

			public string			TextureDiffuse	{ get { return m_TextureDiffuse; } }
			public string			TextureNormal	{ get { return m_TextureNormal; } }
			public string			TextureSpecular	{ get { return m_TextureSpecular; } }

			#endregion

			#region METHODS

			public Material( FileInfo _SourceM2File, string _Name, string _TextureDiffuse, string _TextureNormal, string _TextureSpecular )
			{
				m_SourceM2File = _SourceM2File;
				m_Name = _Name;

				m_TextureDiffuse = _TextureDiffuse;
				m_TextureNormal = _TextureNormal;
				m_TextureSpecular = _TextureSpecular;
			}

			#endregion
		}

		#endregion

		#region FIELDS

		protected List<Material>				m_Materials = new List<Material>();
		protected Dictionary<string,Material>	m_MaterialName2Material = new Dictionary<string,Material>();

		protected List<Material>				m_QueriedMaterials = new List<Material>();

		#endregion

		#region PROPERTIES

		public Material[]			QueriedMaterials	{ get { return m_QueriedMaterials.ToArray(); } }

		#endregion

		#region METHODS

		public MaterialsDatabase()
		{
		}

		/// <summary>
		/// Clears the database
		/// </summary>
		public void	Clear()
		{
			m_Materials.Clear();
			m_MaterialName2Material.Clear();
			ClearQueriedMaterialsList();
		}

		public void ClearQueriedMaterialsList()
		{
			m_QueriedMaterials.Clear();
		}

		/// <summary>
		/// Builds the database from a recursive parsing of M2 files on disk
		/// </summary>
		/// <param name="_RootDirectory"></param>
		/// <returns>Returns the amount of successfully parsed materials</returns>
		public int	BuildFromM2( DirectoryInfo _RootDirectory )
		{
			Clear();

			string	DuplicateMaterials = "";

			IEnumerable<FileInfo>	M2Files = _RootDirectory.EnumerateFiles( "*.m2", SearchOption.AllDirectories );
			foreach ( FileInfo M2File in M2Files )
			{
				try
				{
					Material[]	Materials = ParseM2( M2File );
					foreach ( Material M in Materials )
					{
						if ( m_MaterialName2Material.ContainsKey( M.Name ) )
						{	// Oops!
							DuplicateMaterials += M.Name + ": Found in " + M.SourceM2File.FullName.Replace( @"D:\Workspaces\Arkane\", "" ) + "  AND  " + m_MaterialName2Material[M.Name].SourceM2File.FullName.Replace( @"D:\Workspaces\Arkane\", "" ) + "\r\n";
							continue;
						}
						m_Materials.Add( M );
						m_MaterialName2Material.Add( M.Name, M );
					}
				}
				catch ( Exception )
				{
					
				}
			}

			return m_Materials.Count;
		}

		/// <summary>
		/// Finds a material by its name
		/// </summary>
		/// <param name="_Name"></param>
		/// <returns></returns>
		public Material	FindByName( string _Name )
		{
			int			BestMaterialScore = 0;
			Material	BestMaterial = null;

			foreach ( Material M in m_Materials )
			{
				if ( M.Name.Length <= BestMaterialScore )
					continue;	// It's a less specific material so we don't give a shit...
				if ( _Name.IndexOf( M.Name, StringComparison.CurrentCultureIgnoreCase ) == -1 )
					continue;

				// Found a better material!
				BestMaterial = M;
				BestMaterialScore = M.Name.Length;
			}

			if ( BestMaterial == null )
				return null;

			// Store the material as "queried"
			if ( !m_QueriedMaterials.Contains( BestMaterial) )
				m_QueriedMaterials.Add( BestMaterial );

			return BestMaterial;
		}

		/// <summary>
		/// Parses a M2 file and creates the materials from it
		/// </summary>
		/// <param name="_M2File"></param>
		/// <returns></returns>
		public static Material[]	ParseM2( FileInfo _M2File )
		{
			string	Text;
			using ( StreamReader R = _M2File.OpenText() )
				Text = R.ReadToEnd();

			List<Material>	Result = new List<Material>();

			int	CurrentIndex = 0;
			while( true )
			{
//				int	MaterialStartIndex = Text.IndexOf( "material", CurrentIndex, StringComparison.CurrentCultureIgnoreCase );
				int	MaterialStartIndex = MatchWholeWord( Text, "material", CurrentIndex );
				if ( MaterialStartIndex == -1 )
					break;	// Done!

				// Ensure it's at the beginning of a new line
				int	PreviousEOLIndex = Text.LastIndexOf( '\n', MaterialStartIndex );
				if ( PreviousEOLIndex == -1 )
					PreviousEOLIndex = 0;	// Assume it's the beginning of the file...
				if ( MaterialStartIndex - PreviousEOLIndex > 1 )
				{	// Either a comment or some unsupported "material" occurrence...
					string	DEBUG = Text.Substring( MaterialStartIndex, Text.Length - MaterialStartIndex );
					CurrentIndex = MaterialStartIndex+1;
					continue;
				}

				int	NameStartIndex = MaterialStartIndex + "material".Length;
				if ( MaterialStartIndex >= Text.Length )
					throw new Exception( "Unexpected end of file found after material declaration #" + Result.Count + " !" );

				int	OpeningBraceIndex = Text.IndexOf( '{', NameStartIndex );
				if ( OpeningBraceIndex == -1 )
					throw new Exception( "Opening brace not found after material declaration #" + Result.Count + " !" );

				string	MaterialName = Text.Substring( NameStartIndex, OpeningBraceIndex-NameStartIndex );
						MaterialName = MaterialName.Trim();
				if ( MaterialName == "" )
				{	// Simply skip that, must be something like "choubidou.material {" or the like...
// 					throw new Exception( "Invalid material name!" );
					CurrentIndex = OpeningBraceIndex;
					continue;
				}

				// Our new declaration starts here
				int	MaterialDeclarationStartIndex = OpeningBraceIndex+1;
				int	MaterialDeclarationEndIndex = IndexOfClosingBrace( Text, MaterialDeclarationStartIndex );
				if ( MaterialDeclarationEndIndex == -1 )
					throw new Exception( "Closing brace not found for material declaration \"" + MaterialName + "\"!" );

				string	MaterialDeclaration = Text.Substring( MaterialDeclarationStartIndex, MaterialDeclarationEndIndex-MaterialDeclarationStartIndex );
				MaterialDeclaration += "\n";	// Add a mandatory EOL so parameters search is always successful!

				// Parse the declaration for parameters and texture names
				try
				{
					string	DiffuseTextureName = ParseParameter( MaterialDeclaration, "diffuseMap", ".tga" );
					string	NormalTextureName = ParseParameter( MaterialDeclaration, "bumpmap", ".tga" );
					string	SpecularTextureName = ParseParameter( MaterialDeclaration, "specularMap", ".tga" );

					// Create the new material
					Material	M = new Material( _M2File, MaterialName, DiffuseTextureName, NormalTextureName, SpecularTextureName );
					Result.Add( M );
				}
				catch ( Exception _e )
				{
					throw new Exception( "Error while parsing parameters for material \"" + MaterialName + "\"!", _e );
				}

				// Go to the next material...
				CurrentIndex = MaterialDeclarationEndIndex;
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Provided an index right after an opening brace, finds the corresponding closing brace or -1 if none is found
		/// </summary>
		/// <param name="_Text"></param>
		/// <param name="_StartIndex"></param>
		/// <returns></returns>
		private static int	IndexOfClosingBrace( string _Text, int _StartIndex )
		{
			int	BraceLevel = 1;	// We start AFTER the opening brace so we're already at level 1
			while ( _StartIndex < _Text.Length )
			{
				char	c = _Text[_StartIndex];
				if ( c == '{' )
					BraceLevel++;
				if ( c == '}' )
					BraceLevel--;

				if ( BraceLevel == 0 )
					return _StartIndex;	// We've exited the last level of braces so we found the corresponding closing brace...

				_StartIndex++;
			}

			return -1;
		}

		private static string	ParseParameter( string _Text, string _Pattern, string _ApendExtensionIfMissing )
		{
			int	ParameterIndex = MatchWholeWord( _Text, _Pattern );
			if ( ParameterIndex == -1 )
				return null;	// Not found...

			ParameterIndex += _Pattern.Length;

			int	EOLIndex = FindEOL( _Text, ParameterIndex );
			if ( EOLIndex == -1 )
				throw new Exception( "End of line not found while looking for parameter" );

			// The parameter should lie between current index and the EOL
			string	ParameterValue = _Text.Substring( ParameterIndex, EOLIndex-ParameterIndex );
			ParameterValue = ParameterValue.Trim();

			if ( ParameterValue.Length == 0 )
				return null;
			if ( ParameterValue[0] == '#' )
				return null;	// This is the marker for a placeholder for a texture name, not a real name...

			try
			{
// 				if ( ParameterValue.IndexOf( "moss_02" ) != -1 )
// 				{
// 					string	Check = Path.Combine( ParameterValue, _ApendExtensionIfMissing );
// 				}
				if ( Path.GetFileNameWithoutExtension( ParameterValue ) == Path.GetFileName( ParameterValue ) )
					ParameterValue += _ApendExtensionIfMissing;
			}
			catch ( Exception )
			{	// Certainly not a valid path...
			}

			return ParameterValue;
		}

		private static int	FindEOL( string _Text, int _StartIndex )
		{
			int	EOLIndex = _Text.IndexOf( '\n', _StartIndex );

			// Strange lines happen in M2 files where there's only a single \r character!
			int	CRIndex = _Text.IndexOf( '\r', _StartIndex );

			return Math.Min( CRIndex, EOLIndex );
		}

		/// <summary>
		/// Whole word pattern matching (case insensitive)
		/// </summary>
		/// <param name="_Text"></param>
		/// <param name="_Pattern"></param>
		/// <returns></returns>
		private static int	MatchWholeWord( string _Text, string _Pattern )
		{
			return MatchWholeWord( _Text, _Pattern, 0 );
		}
		private static int	MatchWholeWord( string _Text, string _Pattern, int _Index )
		{
			int	L = _Pattern.Length;
			for ( ; _Index < _Text.Length-L; _Index++ )
			{
				bool	PreviousCharIsWord = _Index > 0 && IsWordChar( _Text[_Index-1] );
				if ( PreviousCharIsWord )
					continue;	// No use to compare...
				bool	NextCharIsWord = _Index+L < _Text.Length && IsWordChar( _Text[_Index+L] );
				if ( NextCharIsWord )
					continue;	// No use to compare...

				if ( string.Compare( _Text, _Index, _Pattern, 0, L, true ) == 0 )
				{
					string	DEBUG = _Text.Substring( _Index );
					return _Index;
				}
			}

			return -1;
		}

		private static bool	IsWordChar( char C )
		{
			return (C >= 'a' && C <= 'z') || (C >= 'A' && C <= 'Z') || (C >= '0' && C <= '9') || C == '_';
		}

		#endregion
	}
}
