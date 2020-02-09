using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Brain2 {

	/// <summary>
	/// Simple text fiche
	/// </summary>
	/// <remarks> This type also handles "http://" and "https://" strings and redirects them to the URL handler</remarks>
	public class	TextHandler : FichesDB.IDataHandler {

		public int	FormatScore( string _formatLowerCase ) {

//if ( _formatLowerCase == "html format" ) return 1000;	// Curious!

			return _formatLowerCase == "system.string" || _formatLowerCase == "text" ? 1 : 0;
		}

		public Fiche CreateFiche( FichesDB _database, string _format, object _data ) {
			string	text = _data.ToString();

			// Check if it's not a URL in disguise, in which case we'll create a webpage instead...
			if ( URLHandler.CanHandleURLScheme( text ) )
				return URLHandler.CreateURLFiche( _database, null, WebHelpers.CreateCanonicalURL( text ) );

			string	title = text;	// Do better?
			string	HTML = WebHelpers.BuildHTMLDocument( title, text );
			Fiche	F = _database.Sync_CreateFicheDescriptor( Fiche.TYPE.LOCAL_EDITABLE_WEBPAGE, text, null, null, HTML );
			_database.Async_SaveFiche( F, true, true );
			return F;
		}
	}
}

// Exemple of "HTML Format" data (a string):
//	Version:0.9 
//	StartHTML:0000000105
//	EndHTML:0000000278
//	StartFragment:0000000141
//	EndFragment:0000000242
//	<html>
//	<body>
//	<!--StartFragment--><a href="https://francisbach.com/jacobi-polynomials/">https://francisbach.com/jacobi-polynomials/</a><!--EndFragment-->
//	</body>
//	</html>