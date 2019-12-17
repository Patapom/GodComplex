using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Brain2 {
	/// <summary>
	/// </summary>
	public static class WebHelpers {

		/// <summary>
		/// Extracts the tags from a text (e.g. "#Tag0 #Tag1" will return { "Tag0", "Tag1" })
		/// </summary>
		/// <param name="_textWithTags"></param>
		/// <returns>The list of tags, without the leading #</returns>
		public static string[]	ExtractTags( string _textWithTags ) {
			_textWithTags = _textWithTags.Replace( '\r', ' ' );
			_textWithTags = _textWithTags.Replace( '\n', ' ' );
			_textWithTags = _textWithTags.Replace( '\t', ' ' );
			string[]		words = _textWithTags.Split( ' ' );
			List< string >	tags = new List<string>();
			foreach ( string word in words ) {
				if ( word.Length < 2 )
					continue;
				if ( word.StartsWith( "#" ) )
					tags.Add( word.Substring( 1 ) );
			}
			return tags.ToArray();
		}

		/// <summary>
		/// Cleans up a URL of garbage and returns a canonical URL
		/// </summary>
		/// <param name="_URL"></param>
		/// <returns></returns>
		public static Uri		CreateCanonicalURL( string _URL ) {
			if ( _URL == null )
				throw new Exception( "Invalid URL!" );

			// Easy clean up
			_URL = _URL.ToLower();
			_URL = _URL.Replace( '\\', '/' );

			// Clean up facebook tracking shit (e.g. "www.myurl.com?fbclid=IwAR0C-JE26XJWpAk1VcBzk9-2y8NwbKhGB_jbIGKMLLakvGWYimKu96xDGMk" => "www.myurl.com")
			int	indexOfFacebookID = _URL.IndexOf( "?fbclid" );
			if ( indexOfFacebookID != -1 ) {
				_URL = _URL.Remove( indexOfFacebookID );
			}

			return new Uri( _URL, UriKind.Absolute );
		}
	}
}