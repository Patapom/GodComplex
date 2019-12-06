using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;

/// <summary>
/// From https://stackoverflow.com/questions/2715385/convert-webpage-to-image-from-asp-net
/// </summary>
public class WebPage2Image {

	public delegate void	ImageReadyDelegate( Bitmap _bitmap );

	private string	m_URL;
	private uint	m_pageWidth;
	private Thread	m_thread = null;

	private Bitmap	m_bitmap;

	ImageReadyDelegate	m_imageReady;

	/// <summary>
	/// Attemps to access the bitmap, only valid once web page is loaded...
	/// </summary>
	public Bitmap	GeneratedImage { get { lock( m_bitmap ) return  m_bitmap; } }

    public WebPage2Image( ImageReadyDelegate _imageReady ) {
		m_imageReady = _imageReady;
	}
    public WebPage2Image( string _URL, uint _pageWidth, ImageReadyDelegate _imageReady ) : this( _imageReady ) {
		Generate( _URL, _pageWidth );
	}

    public void	Generate( string _URL, uint _pageWidth ) {
		lock ( this ) {
			m_URL = _URL;
			m_pageWidth = _pageWidth;

			if ( m_thread != null )
				throw new Exception( "Thread is already running!" );

			m_thread = new Thread( _Generate );
			m_thread.SetApartmentState( ApartmentState.STA );
			m_thread.Start();
			m_thread.Join();
		}
    }

    private void _Generate() {
		WebBrowser	browser = new WebBrowser { ScrollBarsEnabled = false };
					browser.Navigate( m_URL );
					browser.ScriptErrorsSuppressed = true;
					browser.DocumentCompleted += WebBrowser_DocumentCompleted;

		while ( browser.ReadyState != WebBrowserReadyState.Complete ) {
			Application.DoEvents();
		}

		browser.Dispose();

		// Clear running thread
		m_thread = null;
    }

    private void WebBrowser_DocumentCompleted( object sender, WebBrowserDocumentCompletedEventArgs e ) {
		// Capture
		WebBrowser	browser = sender as WebBrowser;
					browser.ClientSize = new Size( (int) m_pageWidth, browser.Document.Body.ScrollRectangle.Bottom );
//					browser.ClientSize = new Size( browser.Document.Body.ScrollRectangle.Width, browser.Document.Body.ScrollRectangle.Bottom );
					browser.ScrollBarsEnabled = false;

		m_bitmap = new Bitmap( browser.Document.Body.ScrollRectangle.Width, browser.Document.Body.ScrollRectangle.Bottom );

		browser.BringToFront();
		browser.DrawToBitmap( m_bitmap, browser.Bounds );

		m_imageReady( m_bitmap );
    }
}

// public static class BitmapExtensions {
//     public static void SaveJPG100(this Bitmap bmp, string filename)
//     {
//         var encoderParameters = new EncoderParameters(1);
//         encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
//         bmp.Save(filename, GetEncoder(ImageFormat.Jpeg), encoderParameters);
//     }
// 
//     public static void SaveJPG100(this Bitmap bmp, Stream stream)
//     {
//         var encoderParameters = new EncoderParameters(1);
//         encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
//         bmp.Save(stream, GetEncoder(ImageFormat.Jpeg), encoderParameters);
//     }
// 
//     public static ImageCodecInfo GetEncoder(ImageFormat format)
//     {
//         var codecs = ImageCodecInfo.GetImageDecoders();
// 
//         foreach (var codec in codecs)
//         {
//             if (codec.FormatID == format.Guid)
//             {
//                 return codec;
//             }
//         }
// 
//         // Return 
//         return null;
//     }
// }
