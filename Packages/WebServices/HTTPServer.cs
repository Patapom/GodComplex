using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;


namespace WebServices {

	/// <summary>
	/// MIT License - Copyright (c) 2016 Can Güney Aksakalli
	/// https://aksakalli.github.io/2014/02/24/simple-http-server-with-csparp.html
	/// Code from https://gist.github.com/aksakalli/9191056
	/// </summary>
	public class HTTPServer : IDisposable {

		#region CONSTANTS

		private const int	DEFAULT_BUFFER_SIZE = 16 * 1024;	// 16KB

		#endregion

		#region NESTED TYPES

		public delegate Stream	FileServerDelegate( string _filename );

		#endregion

		#region FIELDS

// 		private readonly string[] ms_indexFiles = { 
// 			"index.html", 
// 			"index.htm", 
// 			"default.html", 
// 			"default.htm" 
// 		};
//  
		private static IDictionary<string, string>	ms_mimeTypeMappings = new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase ) {
			#region extension to MIME type list
			{".asf", "video/x-ms-asf"},
			{".asx", "video/x-ms-asf"},
			{".avi", "video/x-msvideo"},
			{".bin", "application/octet-stream"},
			{".cco", "application/x-cocoa"},
			{".crt", "application/x-x509-ca-cert"},
			{".css", "text/css"},
			{".deb", "application/octet-stream"},
			{".der", "application/x-x509-ca-cert"},
			{".dll", "application/octet-stream"},
			{".dmg", "application/octet-stream"},
			{".ear", "application/java-archive"},
			{".eot", "application/octet-stream"},
			{".exe", "application/octet-stream"},
			{".flv", "video/x-flv"},
			{".gif", "image/gif"},
			{".hqx", "application/mac-binhex40"},
			{".htc", "text/x-component"},
			{".htm", "text/html"},
			{".html", "text/html"},
			{".ico", "image/x-icon"},
			{".img", "application/octet-stream"},
			{".iso", "application/octet-stream"},
			{".jar", "application/java-archive"},
			{".jardiff", "application/x-java-archive-diff"},
			{".jng", "image/x-jng"},
			{".jnlp", "application/x-java-jnlp-file"},
			{".jpeg", "image/jpeg"},
			{".jpg", "image/jpeg"},
			{".js", "application/x-javascript"},
			{".mml", "text/mathml"},
			{".mng", "video/x-mng"},
			{".mov", "video/quicktime"},
			{".mp3", "audio/mpeg"},
			{".mpeg", "video/mpeg"},
			{".mpg", "video/mpeg"},
			{".msi", "application/octet-stream"},
			{".msm", "application/octet-stream"},
			{".msp", "application/octet-stream"},
			{".pdb", "application/x-pilot"},
			{".pdf", "application/pdf"},
			{".pem", "application/x-x509-ca-cert"},
			{".pl", "application/x-perl"},
			{".pm", "application/x-perl"},
			{".png", "image/png"},
			{".prc", "application/x-pilot"},
			{".ra", "audio/x-realaudio"},
			{".rar", "application/x-rar-compressed"},
			{".rpm", "application/x-redhat-package-manager"},
			{".rss", "text/xml"},
			{".run", "application/x-makeself"},
			{".sea", "application/x-sea"},
			{".shtml", "text/html"},
			{".sit", "application/x-stuffit"},
			{".swf", "application/x-shockwave-flash"},
			{".tcl", "application/x-tcl"},
			{".tk", "application/x-tcl"},
			{".txt", "text/plain"},
			{".war", "application/java-archive"},
			{".wbmp", "image/vnd.wap.wbmp"},
			{".wmv", "video/x-ms-wmv"},
			{".xml", "text/xml"},
			{".xpi", "application/x-xpinstall"},
			{".zip", "application/zip"},
			#endregion
		};

		private FileServerDelegate	m_fileServer;
		private int					m_port;

		private HttpListener		m_listener;
		private Thread				m_serverThread;

		private byte[]				m_buffer = new byte[DEFAULT_BUFFER_SIZE];

		#endregion

		#region PROPERTIES
 
		public int Port {
			get { return m_port; }
			private set { }
		}

		#endregion

		#region METHODS
 
		/// <summary>
		/// Construct server with given port.
		/// </summary>
		/// <param name="_port">Port of the server.</param>
		public HTTPServer( FileServerDelegate _fileServer, int _port ) {
			this.Initialize( _fileServer, _port );
		}
 
		/// <summary>
		/// Construct server with suitable port.
		/// </summary>
		/// <param name="path">Directory path to serve.</param>
		public HTTPServer( FileServerDelegate _fileServer ) {
			// Get an empty port
			TcpListener	l = new TcpListener( IPAddress.Loopback, 0 );
			l.Start();
			int port = ((IPEndPoint) l.LocalEndpoint).Port;
			l.Stop();

			Initialize( _fileServer, port );
		}
 
		public void Dispose() {
			m_serverThread.Abort();
			m_listener.Stop();
		}
 
		private void Listen() {
			m_listener = new HttpListener();
			m_listener.Prefixes.Add( "http://*:" + m_port.ToString() + "/" );
			m_listener.Start();

			while ( true ) {
				try {
					HttpListenerContext context = m_listener.GetContext();
					Process( context );
				} catch ( Exception ) {
				}
			}
		}
 
		private void Process( HttpListenerContext context ) {
			string	filename = context.Request.Url.AbsolutePath;

System.Diagnostics.Debug.WriteLine( "Serving file " + filename );

			filename = filename.Substring( 1 );
 
			Stream	input = null;
			try {
				input = m_fileServer( filename );
				if ( input == null ) {
					context.Response.StatusCode = (int) HttpStatusCode.NotFound;
					context.Response.OutputStream.Close();
					return;
				}
                
				// Adding permanent http response headers
				string	mime;
				context.Response.ContentType = ms_mimeTypeMappings.TryGetValue( Path.GetExtension(filename), out mime ) ? mime : "application/octet-stream";
				context.Response.ContentLength64 = input.Length;
				context.Response.AddHeader( "Date", DateTime.Now.ToString("r") );
//				context.Response.AddHeader( "Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r") );
 
				int		nbytes;
				while ( (nbytes = input.Read(m_buffer, 0, m_buffer.Length)) > 0 ) {
					context.Response.OutputStream.Write( m_buffer, 0, nbytes );
				}
				input.Close();
                
				context.Response.StatusCode = (int) HttpStatusCode.OK;
				context.Response.OutputStream.Flush();
			} catch ( Exception ) {
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			} finally {
				if ( input != null ) {
					input.Dispose();
				}
			}
        
			context.Response.OutputStream.Close();
		}
 
		private void Initialize( FileServerDelegate _fileServer, int _port) {
			m_fileServer = _fileServer;
			m_port = _port;
			m_serverThread = new Thread( this.Listen );
			m_serverThread.Start();
		}

		#endregion
	}
}
