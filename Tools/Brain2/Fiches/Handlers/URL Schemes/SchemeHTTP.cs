using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Brain2 {

	/// <summary>
	/// Scheme for http:// and https:// URLs
	/// </summary>
	public class	SchemeHTTP : URLHandler.IScheme {
		public bool		SupportsURL( string _candidateURL ) {
			if ( _candidateURL.StartsWith( "http://" ) || _candidateURL.StartsWith( "https://" ) )
				return true;

			return false;
		}

		public Fiche		CreateFiche( FichesDB _database, string _title, Uri _URL ) {

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

			// Create the descriptor
			Fiche	F = _database.SyncCreateFicheDescriptor( Fiche.TYPE.REMOTE_ANNOTABLE_WEBPAGE, _title, _URL, tagFiches.ToArray(), null );

			// Load the web page and save the fiche when ready
			_database.AsyncLoadContentAndSaveFiche( F, true );

			return F;
		}
	}
}
