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
				Fiche	tagFiche = _database.Sync_FindOrCreateTagFiche( tag );
				tagFiches.Add( tagFiche );
			}

			// Create the descriptor
			Fiche	F = _database.Sync_CreateFicheDescriptor( Fiche.TYPE.REMOTE_ANNOTABLE_WEBPAGE, _title, _URL, tagFiches.ToArray(), null );

			// Load the web page and save the fiche when ready
			uint	maxScrollsCount = Fiche.ChunkWebPageSnapshot.ms_maxWebPagePieces;	// @TODO: make that depend on the domain, like tweets shouldn't have any comments and a single screen should suffice, for example...

			_database.Async_LoadContentAndSaveFiche( F, maxScrollsCount, true );

			return F;
		}
	}
}
