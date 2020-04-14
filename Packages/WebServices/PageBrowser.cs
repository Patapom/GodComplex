using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using CefSharp;
using CefSharp.WinForms;

namespace WebServices {

	/// <summary>
	/// </summary>
	public class PageBrowser : IDisposable {

		#region NESTED TYPES

		#endregion

		#region FIELDS


		#endregion

		#region METHODS

		public PageBrowser( string _URL ) {

			settings.RegisterScheme(new CefCustomScheme {
				SchemeName = "sharpbrowser",
				SchemeHandlerFactory = new SchemeHandlerFactory()
			});

			settings.UserAgent = UserAgent;

			settings.IgnoreCertificateErrors = true;
			
			settings.CachePath = GetAppDir("Cache");

			Cef.Initialize(settings);

			dHandler = new DownloadHandler(this);
			lHandler = new LifeSpanHandler(this);
			mHandler = new ContextMenuHandler(this);
			kHandler = new KeyboardHandler(this);
			rHandler = new RequestHandler(this);

			InitDownloads();

			host = new HostHandler(this);

			AddNewBrowser(tabStrip1, HomepageURL);
		}

		#endregion
	}
}
