using System;
using System.Collections.Generic;
using System.IO;

namespace Brain2 {

	/// <summary>
	/// The main fiches database
	/// </summary>
	public class FichesDB {

		#region CONSTANTS

// 		public const uint	SIGNATURE = 0x48434946U;	// 'FICH';
// 		public const ushort	VERSION_MAJOR = 1;
// 		public const ushort	VERSION_MINOR = 0;

		#endregion

		#region NESTED TYPES

		#endregion

		#region FIELDS

		private	DirectoryInfo				m_rootFolder = null;

		private List< Fiche >				m_fiches = new List< Fiche >();
		private Dictionary< Guid, Fiche >	m_ID2Fiche = new Dictionary<Guid, Fiche>();

		#endregion

		#region PROPERTIES

		#endregion

		#region METHODS

		public	FichesDB() {
			Fiche.ms_database = this;
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

		#endregion

		#region Asynchronous & Multithreaded Operations

		internal 

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
