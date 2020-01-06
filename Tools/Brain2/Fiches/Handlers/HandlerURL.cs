using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Brain2 {

	/// <summary>
	/// URL fiche: the web page is queried and the fiche is created once the web content and a screenshot are available
	/// </summary>
	public class	URLHandler : FichesDB.IDataHandler {

		public interface IScheme {
			/// <summary>
			/// Return true if the URL is supported by this scheme, false otherwise
			/// </summary>
			/// <param name="_URL"></param>
			/// <returns></returns>
			bool		SupportsURL( string _candidateURL );

			/// <summary>
			/// Create the fiche for the specified URL
			/// </summary>
			/// <param name="_URL"></param>
			/// <returns></returns>
			Fiche		CreateFiche( FichesDB _database, string _title, Uri _URL );
		}

		static List< IScheme >		ms_schemes = new List<IScheme>();
		static List< IScheme >	Schemes {
			get {
				if ( ms_schemes.Count == 0 ) {
					// Register schemes
					ms_schemes.Add( new SchemeHTTP() );
				}

				return ms_schemes;
			}
		}

		public int	FormatScore( string _formatLowerCase ) {
			switch ( _formatLowerCase ) {
				case "text/x-moz-url":
				case "uniformresourcelocator":
				case "uniformresourcelocatorw":
					return 10;
			}

			return 0;
		}

		public Fiche CreateFiche( FichesDB _database, string _format, object _data ) {
			MemoryStream	S = _data as MemoryStream;
			if ( S == null )
				throw new Exception( "Provided data is not the expected MemoryStream type!" );

			_format = _format.ToLower();
			bool	isUnicode = _format == "text/x-moz-url" || _format == "uniformresourcelocatorw";

// string	debug = "Length = " + S.Length + "\r\n<bold>";
// for ( int i=0; i < S.Length; i++ ) {
// 	debug += S.ReadByte().ToString( "X2" );
// }
// BrainForm.Debug( debug );
//string	HTML = "<body>" + debug + "</bold></body>";

			// Assume the memory stream contains a unicode string
			int						stringLength = (int) S.Length;
			System.Text.Encoding	encoding = isUnicode ? System.Text.Encoding.Unicode : System.Text.Encoding.ASCII;

			if ( isUnicode ) {
				if ( (stringLength & 1) != 0 )
					throw new Exception( "Expected an even size! Apparently we're not really dealing with a unicode string..." );
				stringLength >>= 1;
			}

			char[]	URLchars = new char[stringLength];
			using ( StreamReader R = new StreamReader( S, encoding ) ) {
				R.Read( URLchars, 0, URLchars.Length );
			}

			string		title = null;
			string		strURL = new string( URLchars );
			string[]	URLLines = strURL.Split( '\n' );
			if ( URLLines.Length > 1 ) {
				// URL is multi-part: must have a title after the actual link...
				strURL = URLLines[0];
				title = URLLines[1];
			}

			Uri	URL = WebHelpers.CreateCanonicalURL( strURL );

			return CreateURLFiche( _database, title, URL );
		}

		public static bool	CanHandleURLScheme( string _candidateURL ) {
			_candidateURL = _candidateURL.ToLower();
			foreach ( IScheme scheme in Schemes ) {
				if ( scheme.SupportsURL( _candidateURL ) )
					return true;
			}

			return false;
		}

		public static Fiche	CreateURLFiche( FichesDB _database, string _title, Uri _URL ) {
			if ( _URL == null )
				throw new Exception( "Invalid null URL to create fiche!" );
			if ( !Uri.IsWellFormedUriString( _URL.AbsoluteUri, UriKind.Absolute ) ) {
				throw new Exception( "Invalid URL to create fiche!" );
			}

			// Attempt to create the fiche itself
			string	candidateURL = _URL.AbsoluteUri.ToLower();
			foreach ( IScheme scheme in Schemes ) {
				if ( scheme.SupportsURL( candidateURL ) ) {
					return scheme.CreateFiche( _database, _title, _URL );
				}
			}

			return null;
		}
	}
}
