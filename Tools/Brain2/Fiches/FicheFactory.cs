using System;
using System.Collections.Generic;
using System.IO;

namespace Brain2 {

	/// <summary>
	/// The fiche factory capable of creating a new fiche from any data source
	/// </summary>
	public static class FicheFactory {

		#region CONSTANTS

		#endregion

		#region NESTED TYPES

		public interface IDataHandler {
			int		FormatScore( string _formatLowerCase );		// Must return 0 if not supported
			Fiche	CreateFiche( string _format, object _data );
		}

		public class	TextHandler : IDataHandler {

			public int	FormatScore( string _formatLowerCase ) {
				return _formatLowerCase == "system.string" || _formatLowerCase == "text" ? 1 : 0;
			}

			public Fiche CreateFiche( string _format, object _data ) {
				string	text = _data.ToString();
				if ( text.StartsWith( "http://" ) || text.StartsWith( "https://" ) )
					return URLHandler.CreateURLFiche( new Uri( text, UriKind.Absolute ) );

				string	HTML = "<body>" + text + "</body>";
				return new Fiche( text, null, null, HTML );
			}
		}

		public class	URLHandler : IDataHandler {

			public int	FormatScore( string _formatLowerCase ) {
				switch ( _formatLowerCase ) {
					case "text/x-moz-url":
					case "uniformresourcelocator":
					case "uniformresourcelocatorw":
						return 10;
				}

				return 0;
			}

			public Fiche CreateFiche( string _format, object _data ) {
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
						throw new Exception( "Expected an even size! Apparently not dealing with a unicode string..." );
					stringLength >>= 1;
				}

				char[]	URLchars = new char[stringLength];
				using ( StreamReader R = new StreamReader( S, encoding ) ) {
					R.Read( URLchars, 0, URLchars.Length );
				}
				Uri	URL = new Uri( new string( URLchars ), UriKind.Absolute );

				return CreateURLFiche( URL );
			}

			public static Fiche	CreateURLFiche( Uri _URL ) {
				if ( _URL == null )
					throw new Exception( "Invalid null URL to create fiche!" );

//				string	content = "TODO! " + _URL;

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
				string	title = "dummy title";

//				return new Fiche( title, _URL, null, Fiche.BuildHTMLDocument( title, content ) );
				return new Fiche( title, _URL, null, null );
			}
		}

		#endregion

		#region FIELDS

		public static List< IDataHandler >	ms_handlers = new List<IDataHandler>();

		#endregion

		#region METHODS

		static FicheFactory() {
			ms_handlers.Add( new URLHandler() );
			ms_handlers.Add( new TextHandler() );
		}

		public static Fiche	CreateFiche( System.Windows.Forms.IDataObject _data ) {
			if ( _data == null )
				throw new Exception( "Invalid data object!" );

			IDataHandler	bestHandler = null;
			int				bestScore = 0;
			string			bestFormat = null;
			foreach ( string format in _data.GetFormats() ) {
				if ( format == null )
					continue;

				string	formatLow = format.ToLower();
				foreach ( IDataHandler handler in ms_handlers ) {
					int	formatScore = handler.FormatScore( formatLow );
					if ( formatScore > bestScore ) {
						// Found a better handler...
						bestHandler = handler;
						bestScore = formatScore;
						bestFormat = format;
					}
				}
			}

			if ( bestHandler == null )
				return null;

			// Create the fiche using the best possible handler
			object	data = _data.GetData( bestFormat );
			if ( data == null )
				throw new Exception( "Failed to retrieve drop data for format \"" + bestFormat + "\"!" );

			Fiche	fiche = bestHandler.CreateFiche( bestFormat, data );
			return fiche;
		}

// DragContext => System.IO.MemoryStream
// DragImageBits => System.IO.MemoryStream
// text/x-moz-url => System.IO.MemoryStream
// FileGroupDescriptorW => System.IO.MemoryStream
// FileContents => <null>
// UniformResourceLocatorW => System.IO.MemoryStream
// UniformResourceLocator => System.IO.MemoryStream
// System.String => https://stackoverflow.com/questions/36822654/alternative-for-text-x-moz-url-in-chrome-ie-10-edge-in-event-datatransfer
// UnicodeText => https://stackoverflow.com/questions/36822654/alternative-for-text-x-moz-url-in-chrome-ie-10-edge-in-event-datatransfer
// Text => https://stackoverflow.com/questions/36822654/alternative-for-text-x-moz-url-in-chrome-ie-10-edge-in-event-datatransfer

		#endregion
	}
}
