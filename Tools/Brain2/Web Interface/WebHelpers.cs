using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using SharpMath;

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

		/// <summary>
		/// Creates a simple HTML document with a head and a body
		/// </summary>
		/// <param name="_title"></param>
		/// <param name="_content"></param>
		/// <returns></returns>
		public static string	BuildHTMLDocument( string _title, string _content ) {

			string	template =
@"<!DOCTYPE html>
<html>

<head>
  <title>@TITLE@</title>
</head>

<body>
@CONTENT@
</body>

</html>";

			string	doc = template.Replace( "@TITLE@", _title ).Replace( "@CONTENT@", _content );
			return doc;
		}

		public delegate void	WebPageRendered( string _HTMLContent, ImageUtility.ImageFile _imageWebPage );

		/// <summary>
		/// Loads a web page and renders it into an image
		/// </summary>
		/// <param name="_URL"></param>
		public static void	LoadWebPage( Uri _URL, WebPageRendered _delegate ) {

			string	dummyHTML = BuildHTMLDocument( "Dummy Title", "DUMMY CONTENT!" );

			uint	seed = (uint) _URL.GetHashCode();

			ImageUtility.ImageFile	dummyPage = new ImageUtility.ImageFile( Fiche.ChunkWebPageSnapshot.DEFAULT_WEBPAGE_WIDTH, Fiche.ChunkWebPageSnapshot.DEFAULT_WEBPAGE_HEIGHT, ImageUtility.PIXEL_FORMAT.BGRA8, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
			dummyPage.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
				_color.x = Mathf.Sin( 0.1f * (seed + _X) );
				_color.y = Mathf.Sin( 0.123f * (seed + _Y) );
				_color.z = Mathf.Sin( 0.01234f * (seed + _X + _Y) );
				_color.w = 1.0f;
			} );

			// Notify
			_delegate( dummyHTML, dummyPage );
		}
	}
}