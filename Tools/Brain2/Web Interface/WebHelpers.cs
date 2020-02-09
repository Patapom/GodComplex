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

		static readonly string[]	UTMTrackers = new string[] { "utm_source", "utm_medium", "utm_campaign", "utm_term", "utm_content" };

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

			// Clean up urchin tracking shit (e.g. "www.myurl.com?utm_term=Autofeed&utm_medium=Social&utm_source=Twitter&Echobox=1576707437#xtor=CS3-5083" => "www.myurl.com")
			// https://en.wikipedia.org/wiki/UTM_parameters
			foreach ( string UTMTracker in UTMTrackers ) {
				int	indexOfTracker = _URL.IndexOf( "?" + UTMTracker );
				if ( indexOfTracker == -1 ) {
					indexOfTracker = _URL.IndexOf( "&" + UTMTracker );	// Try with a '&' instead
				}
				if ( indexOfTracker != -1 ) {
					_URL = _URL.Remove( indexOfTracker );
				}
			}

			Uri	URL = null;
			if ( !Uri.TryCreate( _URL, UriKind.Absolute, out URL ) )
				throw new Exception( "URL could not be created from an badly formatted string \"" + URL + "\"!" );

			return URL;
		}

		/// <summary>
		/// Only collate alpha-numerical characters and underscores
		/// </summary>
		/// <param name="_name"></param>
		/// <returns></returns>
		public static string	MakeAlphaNumerical( string _name ) {
			string	result = "";
			foreach ( char C in _name ) {
				if (	(C >= 'a' && C <= 'z')
					||	(C >= 'A' && C <= 'Z')
					||	(C >= '0' && C <= '9')
					||	C == '_' ) {
					result += C;
				}
			}
			return result;
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

		public enum WEB_ERROR_TYPE {
			NO_INTERNET,
			ERROR_404,
			TIMEOUT,
			UNKNOWN,
		}

		public enum LOG_TYPE {
			INFO,
			WARNING,
			ERROR,
			DEBUG
		}

		public delegate void	WebPageSourceAvailable( string _title, string _HTMLContent, System.Xml.XmlDocument _DOMElements );
		public delegate void	WebPagePieceRendered( uint _webPagePieceIndex, ImageUtility.ImageFile _imageWebPage );
		public delegate void	WebPageSuccess();
		public delegate void	WebPageError( WEB_ERROR_TYPE _error, int _errorCode, string _message );
		public delegate void	Log( LOG_TYPE _type, string _message );

		/// <summary>
		/// Loads a web page and renders it into an image
		/// </summary>
		/// <param name="_URL"></param>
		public static void	LoadWebPage( Uri _URL, WebPageSourceAvailable _onSourceAvailable, WebPagePieceRendered _onPagePieceRendered, WebPageSuccess _onSuccess, WebPageError _onError, Log _log ) {
			WebServices.HTMLPageRenderer	pageRenderer = new WebServices.HTMLPageRenderer(
				_URL.ToString(),
				(int) Fiche.ChunkWebPageSnapshot.DEFAULT_WEBPAGE_WIDTH, (int) Fiche.ChunkWebPageSnapshot.DEFAULT_WEBPAGE_PIECE_HEIGHT,
				(int) Fiche.ChunkWebPageSnapshot.MAX_WEBPAGE_PIECES,

				// Occurs whenever the page's HTML source is available
				( string _title, string _HTMLContent, System.Xml.XmlDocument _DOMElements ) => {
					_onSourceAvailable( _title, _HTMLContent, _DOMElements );
				},

				// Occurs whenever a piece of the web page was successfully rendered
				( uint _webPagePieceIndex, ImageUtility.ImageFile _imageWebPage ) => {
					_onPagePieceRendered( _webPagePieceIndex, _imageWebPage );
				},

				// Occurs when the page successfully rendered
				() => {
					_onSuccess();
				},

				// Occurs whenever the web pge failed to load
				( int _errorCode, string _errorText ) => {
					WEB_ERROR_TYPE	type = WEB_ERROR_TYPE.UNKNOWN;
					// TODO: Resolve known errors from Cef errors...
					_onError( type, (int) _errorCode, _errorText );
				},

				( WebServices.HTMLPageRenderer.LOG_TYPE _type, string _text ) => {
					_log( (LOG_TYPE) _type, _text );
				}
			);
		}

		#region Dummy Page Loader
		public static void	DummyLoadWebPage( Uri _URL, WebPageSourceAvailable _onSourceAvailable, WebPagePieceRendered _onPieceRendered, WebPageSuccess _onSuccess, WebPageError _onError ) {

//				string	content = "DUMMY CONTENT!";

 				string	content = "<blockquote class=\"twitter-tweet\"><p lang=\"en\" dir=\"ltr\">Appropriate for December. <a href=\"https://t.co/dzNBXmcreS\">pic.twitter.com/dzNBXmcreS</a></p>&mdash; In Otter News (@Otter_News)" +
 				"<a href=\"https://twitter.com/Otter_News/status/1204733982149160960?ref_src=twsrc%5Etfw\">December 11, 2019</a></blockquote> <script async src=\"https://platform.twitter.com/widgets.js\" charset=\"utf-8\"></script>";

/*string	content = @"<blockquote class=""Tweet h-entry js-tweetIdInfo subject expanded"" cite=""https://twitter.com/Poulin2012/status/1204065818432167937"" data-tweet-id=""1204065818432167937"" data-scribe=""section:subject"">
    <div class=""Tweet-header"">
      <a class=""TweetAuthor-avatar  Identity-avatar u-linkBlend"" data-scribe=""element:user_link"" href=""https://twitter.com/Poulin2012"" aria-label=""Alexis Poulin (nom d'utilisateur : Poulin2012)""><img class=""Avatar"" data-scribe=""element:avatar"" data-src-2x=""https://pbs.twimg.com/profile_images/1110858983357771778/aTqqWLFY_bigger.jpg"" alt="""" data-src-1x=""https://pbs.twimg.com/profile_images/1110858983357771778/aTqqWLFY_normal.jpg"" src=""https://pbs.twimg.com/profile_images/1110858983357771778/aTqqWLFY_normal.jpg""></a>

      


<div class=""TweetAuthor js-inViewportScribingTarget"" data-scribe=""component:author"">
  <a class=""TweetAuthor-link Identity u-linkBlend"" data-scribe=""element:user_link"" href=""https://twitter.com/Poulin2012"" aria-label=""Alexis Poulin (nom d'utilisateur : Poulin2012)"">
    <div class=""TweetAuthor-nameScreenNameContainer"">
      <span class=""TweetAuthor-decoratedName"">
        <span class=""TweetAuthor-name Identity-name customisable-highlight"" title=""Alexis Poulin"" data-scribe=""element:name"">Alexis Poulin</span>
        <span class=""TweetAuthor-verifiedBadge"" data-scribe=""element:verified_badge""><div class=""Icon Icon--verified "" aria-label=""Compte certifié"" title=""Compte certifié"" role=""img""></div>
<b class=""u-hiddenVisually"">✔</b></span>
      </span>
      <span class=""TweetAuthor-screenName Identity-screenName"" title=""@Poulin2012"" data-scribe=""element:screen_name"" dir=""ltr"">@Poulin2012</span>
    </div>
  </a>
</div>

        <div class=""Tweet-brand"">
          <a href=""https://twitter.com/Poulin2012/status/1204065818432167937"" data-scribe=""element:logo""><span class=""FollowButton-bird""><div class=""Icon Icon--twitter "" aria-label=""Voir sur Twitter"" title=""Voir sur Twitter"" role=""presentation""></div>
</span></a>
        </div>
    </div>
    <div class=""Tweet-body e-entry-content"" data-scribe=""component:tweet"">
      
      <div class=""Tweet-target js-inViewportScribingTarget""></div>
      <p class=""Tweet-text e-entry-title"" lang=""fr"" dir=""ltr"">A propos de «&nbsp;la haine&nbsp;» qui visiblement en novlangue veut dire «&nbsp;capter des images de violences policières&nbsp;»... <a href=""https://t.co/6W9zbPCuCG"" rel=""nofollow noopener"" dir=""ltr"" data-expanded-url=""https://twitter.com/davduf/status/1204059641413586946"" class=""link customisable"" target=""_blank"" title=""https://twitter.com/davduf/status/1204059641413586946"" data-tweet-id=""1204059641413586946"" data-tweet-item-type=""23"" data-scribe=""element:url""><span class=""u-hiddenVisually"">https://</span>twitter.com/davduf/status/<span class=""u-hiddenVisually"">1204059641413586946&nbsp;</span>…</a></p>



        <div class=""Tweet-card"">
<div class=""QuoteTweet"" tabindex=""0"" data-scribe=""section:quote"">
  <a class=""QuoteTweet-link"" data-tweet-id=""1204059641413586946"" data-tweet-item-type=""23"" href=""https://twitter.com/davduf/status/1204059641413586946"" target=""_blank"" rel=""noopener"">
    <div class=""QuoteTweet-nonMediaContainer"">
      


<div class=""TweetAuthor js-inViewportScribingTarget TweetAuthor--oneLine"" data-scribe=""component:author"">
  
    <div class=""TweetAuthor-nameScreenNameContainer"">
      <span class=""TweetAuthor-decoratedName"">
        <span class=""TweetAuthor-name Identity-name customisable-highlight"" title=""David Dufresne"" data-scribe=""element:name"">David Dufresne</span>
        <span class=""TweetAuthor-verifiedBadge"" data-scribe=""element:verified_badge""><div class=""Icon Icon--verified "" aria-label=""Compte certifié"" title=""Compte certifié"" role=""img""></div>
<b class=""u-hiddenVisually"">✔</b></span>
      </span>
      <span class=""TweetAuthor-screenName Identity-screenName"" title=""@davduf"" data-scribe=""element:screen_name"" dir=""ltr"">@davduf</span>
    </div>
  
</div>

      <div></div>
      <p class=""QuoteTweet-text e-entry-title"" lang=""fr"" dir=""ltr"">Le sénateur Grand, profitant de la proposition de loi de « Lutte contre la haine »  sur internet (PPL), propose une amande de 15 000 € pour captation d'image de policiers. <span class=""PrettyLink-prefix"">#</span><span class=""PrettyLink-value"">ViolencesPolicières</span> <span class=""PrettyLink-prefix"">#</span><span class=""PrettyLink-value"">LibertédInformer</span><br><br>Source: <span class=""u-hiddenVisually"">http://www.</span>senat.fr/amendements/co<span class=""u-hiddenVisually"">mmissions/2018-2019/645/Amdt_COM-13.html&nbsp;</span>…</p>
    </div>
    <div class=""QuotedTweet-media"">
  
<article class=""MediaCard
           
           customisable-border"" data-scribe=""component:card"" dir=""ltr"">
  <div class=""MediaCard-media"" data-scribe=""element:photo"">

    <div class=""MediaCard-widthConstraint js-cspForcedStyle"" style=""max-width: 1200px"" data-style=""max-width: 1200px"">
      <div class=""MediaCard-mediaContainer js-cspForcedStyle MediaCard--roundedBottom"" style=""padding-bottom: 68.9167%"" data-style=""padding-bottom: 68.9167%"">
        <div class=""MediaCard-mediaAsset NaturalImage"">
          <img class=""NaturalImage-image"" data-image=""https://pbs.twimg.com/media/ELWt20_W4AAea8q"" data-image-format=""png"" width=""1200"" height=""827"" title=""Voir l'image sur Twitter"" alt=""Voir l'image sur Twitter"" src=""https://pbs.twimg.com/media/ELWt20_W4AAea8q?format=png&amp;name=small"">
        </div>
      </div>
    </div>
  </div>
</article>

  
  
</div>
  </a>
</div>
</div>


      <div class=""TweetInfo"">
        <div class=""TweetInfo-like"">
<a class=""TweetInfo-heart"" title=""J'aime"" href=""https://twitter.com/intent/like?tweet_id=1204065818432167937"" data-scribe=""component:actions"">
  <div data-scribe=""element:heart""><div class=""Icon Icon--heart "" aria-label=""J'aime"" title=""J'aime"" role=""img""></div>
</div>
  <span class=""TweetInfo-heartStat"" data-scribe=""element:heart_count"">704</span>
</a>
</div>
        <div class=""TweetInfo-timeGeo"">

<a class=""u-linkBlend u-url customisable-highlight long-permalink"" data-datetime=""2019-12-09T15:50:36+0000"" data-scribe=""element:full_timestamp"" href=""https://twitter.com/Poulin2012/status/1204065818432167937"">








<time class=""dt-updated"" datetime=""2019-12-09T15:50:36+0000"" pubdate="""" title=""Heure de publication : 09 décembre 2019 15:50:36 (UTC)"">10:50 - 9 déc. 2019</time></a></div>
        <div class=""tweet-InformationCircle"" data-scribe=""element:notice""><a href=""https://support.twitter.com/articles/20175256"" class=""Icon Icon--informationCircleWhite js-inViewportScribingTarget"" title=""Informations sur les Publicités Twitter et confidentialité""><span class=""u-hiddenVisually"">Informations sur les Publicités Twitter et confidentialité</span></a>
</div>
      </div>
    </div>
  </blockquote>";
//*/
//				string	title = "dummy title";

//				return new Fiche( _title, _URL, null, Fiche.BuildHTMLDocument( title, content ) );


			string	dummyTitle = "Dummy Title";
			string	dummyHTML = BuildHTMLDocument( dummyTitle, content );

			uint	seed = (uint) _URL.GetHashCode();

			ImageUtility.ImageFile	dummyPage = new ImageUtility.ImageFile( Fiche.ChunkWebPageSnapshot.DEFAULT_WEBPAGE_WIDTH, Fiche.ChunkWebPageSnapshot.DEFAULT_WEBPAGE_PIECE_HEIGHT, ImageUtility.PIXEL_FORMAT.BGRA8, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
			dummyPage.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
				_color.x = Mathf.Sin( 0.1f * (seed + _X) );
				_color.y = Mathf.Sin( 0.123f * (seed + _Y) );
				_color.z = Mathf.Sin( 0.01234f * (seed + _X + _Y) );
				_color.w = 1.0f;
			} );

			// Notify
			_onSourceAvailable( dummyTitle, dummyHTML, null );
			_onPieceRendered( 0, dummyPage );
			_onSuccess();
		}
		#endregion
	}
}