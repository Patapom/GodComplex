namespace ZetaHtmlEditControl.UI.EditControlBases
{
    using System.ComponentModel;
    using System.Windows.Forms;
    using Code.HttpServer;
    using Microsoft.VisualBasic.CompilerServices;

    public class ExtendedWebBrowser : 
        WebBrowser
	{
        private static readonly object TypeLock = new object();
        private static IExternalWebServer _webServer;
        private static IExternalWebServer _externalWebServer;
        private int _documentCompletedCount;
		private int _documentSetCount;
		private string _textToSet = string.Empty;

        private bool _wasOn;

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get { return DocumentText; }
            set { DocumentText = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string DocumentText
        {
            get
            {
                if (DesignMode)
                {
                    return string.Empty;
                }
                else
                {
                    // --
                    // 2013-02-23, Uwe Keim:
                    //
                    // The idea here is that a condition could occur where a text was set by code 
                    // and immediately read back again by code, but the browser had not enough time
                    // to actually navigate to the text (i.e. the end-user does not see the text yet).
                    //
                    // In such a case, the reading of the DocumentText property would return the
                    // previously loaded text, which is a blank text when the control was just initialized.
                    // 
                    // To avoid this, we keep track of when setting the text and when actually finished
                    // loading that text. We only return the text from the HTML editor when it was finished
                    // loading. Otherwise, we just return the text that was programmatically set to
                    // the control.

                    // --
                    // 2013-03-07, Uwe Keim:
                    //
                    // I've noticed that in Zeta Helpdesk where I included this control, in some cases
                    // the control is not in edit mode and I have to re-open the dialog again. I'm not
                    // sure whether the change hier was the reason, but I've never seen the behaviour
                    // prior to making this change.

                    if (_documentCompletedCount > 0 && _documentSetCount > 0)
                    {
                        return base.DocumentText;
                    }
                    else
                    {
                        return _textToSet;
                    }
                }
            }
            set
            {
                if (!DesignMode)
                {
                    _documentCompletedCount = 0;
                    _documentSetCount++;
                    _textToSet = value;

                    Navigate(WebServer.SetDocumentText(this, value));
                }
            }
        }

        [Browsable(false)]
	    [EditorBrowsable(EditorBrowsableState.Never)]
	    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	    public static IExternalWebServer ExternalWebServer
	    {
	        get
	        {
	            lock (TypeLock)
	            {
	                return _externalWebServer;
	            }
	        }
	        set
	        {
	            lock (TypeLock)
	            {
	                _externalWebServer = value;
	            }
	        }
	    }

        internal static IExternalWebServer WebServer
		{
			get
			{
				lock (TypeLock)
				{
					if (_externalWebServer != null)
					{
						return _externalWebServer;
					}
					else
					{
						if (_webServer == null)
						{
							var ws = new WebServer();
							ws.Initialize();
							_webServer = ws;
						}

						return _webServer;
					}
				}
			}
		}

        public new void Navigate(string url)
        {
            // This Application.DoEvents() is necessary, 
            // otherwise the webbrowser gets a 
            // AccessViolationException, whyever.
            Application.DoEvents();

            // Turn off before navigating to get rid of the "Document was modified" message box.
            // http://social.msdn.microsoft.com/Forums/en/winforms/thread/4928c061-951a-43cc-aad2-8844084c148d
            turnWebBrowserDesignModeOff();

            // This Application.DoEvents() is necessary, 
            // otherwise the webbrowser gets a 
            // AccessViolationException, whyever.
            Application.DoEvents();

            base.Navigate(url);
        }

        protected override void OnDocumentCompleted(
            WebBrowserDocumentCompletedEventArgs e)
        {
            base.OnDocumentCompleted(e);

            turnWebBrowserDesignModeOn();

            // This Application.DoEvents() is necessary, 
            // otherwise the webbrowser gets a 
            // AccessViolationException, whyever.
            Application.DoEvents();

            _documentCompletedCount++;
        }

        private void turnWebBrowserDesignModeOn()
		{
			var axInstance = ActiveXInstance;
			if( axInstance!=null ) 
			{
				var instance =
					NewLateBinding.LateGet(
						axInstance,
						null,
						@"Document",
						new object[0],
						null,
						null,
						null);

				NewLateBinding.LateSetComplex(
					instance,
					null,
					@"designMode",
					new object[] {@"On"},
					null,
					null,
					false,
					true);

				_wasOn = true;
			}
		}

		private void turnWebBrowserDesignModeOff()
		{
			if (_wasOn)
			{
				var axInstance = ActiveXInstance;
				if( axInstance!=null ) 
				{
					var instance =
						NewLateBinding.LateGet(
							axInstance,
							null,
							@"Document",
							new object[0],
							null,
							null,
							null);

					NewLateBinding.LateSetComplex(
						instance,
						null,
						@"designMode",
						new object[] {@"Off"},
						null,
						null,
						false,
						true);
				}
			}
		}
	}
}