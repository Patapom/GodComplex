using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Brain2 {

	public partial class PreferencesForm : ModelessForm {
//	public partial class PreferencesForm : Form {

		#region CONSTANTS

		#endregion

		#region FIELDS

		private Microsoft.Win32.RegistryKey	m_appKey;

		#endregion

		#region PROPERTIES

		public override Keys SHORTCUT_KEY => Keys.F9;

		public string		RootDBFolder {
			get { return textBoxDatabaseRoot.Text; }
			set {
				if ( value == textBoxDatabaseRoot.Text )
					return;	// No change...

				// Rebase database folder
				SetRegKey( "RootDBFolder", value );
				textBoxDatabaseRoot.Text = value;

				// Notify
				if ( RootDBFolderChanged != null )
					RootDBFolderChanged( this, EventArgs.Empty );
			}
		}

		public event EventHandler		RootDBFolderChanged;

		#endregion

		#region METHODS

		public PreferencesForm( BrainForm _owner ) : base( _owner ) {
			m_appKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\Brain2" );

			InitializeComponent();

			// Fetch default values from registry
//			string	defaultDBFolder = Path.Combine( Path.GetDirectoryName( Application.ExecutablePath ), "BrainFiches" );
			string	defaultDBFolder = Path.Combine( Directory.GetCurrentDirectory(), "BrainFiches" );
			textBoxDatabaseRoot.Text = GetRegKey( "RootDBFolder", defaultDBFolder );

			LocateBookmarks();
		}

		#region Registry

		public string	GetRegKey( string _key, string _default ) {
			string	result = m_appKey.GetValue( _key ) as string;
			return result != null ? result : _default;
		}
		public void	SetRegKey( string _key, string _Value ) {
			m_appKey.SetValue( _key, _Value );
		}

		public float	GetRegKeyFloat( string _key, float _default ) {
			string	value = GetRegKey( _key, _default.ToString() );
			float	result;
			float.TryParse( value, out result );
			return result;
		}

		public int		GetRegKeyInt( string _key, float _default ) {
			string	value = GetRegKey( _key, _default.ToString() );
			int		result;
			int.TryParse( value, out result );
			return result;
		}

		#endregion

		#region Bookmarks Import

		class BookmarkFile {
			public enum BOOKMARK_TYPE {
				CHROME,
				FIREFOX,
				INTERNET_EXPLORER,
				SAFARI,
				OPERA,
			}
			public BOOKMARK_TYPE	m_type;
			public FileInfo			m_fileName;
		}

		void	LocateBookmarks() {
			try {
				listViewBookmarks.SuspendLayout();
				listViewBookmarks.Clear();
				buttonImportBookmarks.Enabled = false;

				// List Chrome bookmarks
				const string	chromeRootPath = @"appdata\local\google\chrome\user data\";

				Everything.Search.MatchPath = true;
//				Everything.Search.SearchExpression = @"parent:" + chromeRootPath + " bookmark";
				Everything.Search.SearchExpression = chromeRootPath + " bookmarks";
				Everything.Search.ExecuteQuery();

				foreach ( Everything.Search.Result result in Everything.Search.Results ) {
					try {
						FileInfo	bookmarkFile = new FileInfo( result.FullName );
						if ( !bookmarkFile.Exists || bookmarkFile.Name.ToLower() != "bookmarks" )
							continue;	// Not a valid bookmark file...

						// Retrieve profile name from path
						int		pathStartIndex = bookmarkFile.DirectoryName.ToLower().IndexOf( chromeRootPath );
						if ( pathStartIndex == -1 )
							throw new Exception( "Failed to retrieve Chrome path in bookmark file path!" );
						string	profileName = bookmarkFile.DirectoryName.Substring( pathStartIndex + chromeRootPath.Length );	// Strip relative path

						// Add a successfully recognized bookmark file to the list
						ListViewItem	bookmarkItem = new ListViewItem( "Chrome - " + profileName );
										bookmarkItem.Tag = new BookmarkFile() { m_fileName = bookmarkFile, m_type = BookmarkFile.BOOKMARK_TYPE.CHROME };
						listViewBookmarks.Items.Add( bookmarkItem );


if ( bookmarkFile.FullName.IndexOf( "Profile 2" ) != -1 )
	ImportBookmarksChrome( bookmarkFile );


					} catch ( Exception _e ) {
						BrainForm.Debug( "Error while listing bookmark file for result " + result.FullName + ": " + _e.Message );
					}
				}

				if ( listViewBookmarks.Items.Count == 0 ) {
					listViewBookmarks.Items.Add( "No bookmarks found" );
				}

			} catch ( Exception _e ) {
				listViewBookmarks.Items.Add( "Error " + _e.Message );
			} finally {
				listViewBookmarks.ResumeLayout();
			}
		}

		#region JSON Parsing

		enum JSONState {
			ROOT,
			DICTIONARY_EXPECT_KEY,
			DICTIONARY_EXPECT_VALUE,
			DICTIONARY_EXPECT_KEY_SEPARATOR,			// Read key, expecting separator
			DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END,	// Read value, expecting separator or collection end
			ARRAY_EXPECT_ELEMENT,
			ARRAY_EXPECT_SEPARATOR_OR_END,
		}
		[System.Diagnostics.DebuggerDisplay( "{m_state} Object={m_object}" )]
		class JSONObject {
			public JSONObject	m_previous = null;
			public JSONState	m_state;
			public object		m_object = null;
			public string		m_currentKey = null;	// Last parsed key when parsing collections
			public JSONObject( JSONObject _previous, JSONState _newState, object _object ) {
				m_previous = _previous;
				m_state = _newState;
				m_object = _object;
			}

			public Dictionary<string,object>	AsDictionary { get {return m_object as Dictionary<string,object>; } }
			public bool							IsDictionary { get { return AsDictionary != null; } }
			public JSONObject					this[string _dictionaryKey] { get { return AsDictionary[_dictionaryKey] as JSONObject; } }

			public List<object>					AsArray { get {return m_object as List<object>; } }
			public bool							IsArray { get { return AsArray != null; } }
			public JSONObject					this[int _index] { get { return AsArray[_index] as JSONObject; } }
		}

		JSONObject	ReadJSON( StreamReader _reader ) {
			JSONObject	root = new JSONObject( null, JSONState.DICTIONARY_EXPECT_VALUE, new Dictionary<string,object>() );
						root.m_currentKey = "root";

// For debugging purpose => writes the stream read so far into a string so we can see where it crashed
StringBuilder	sb = new StringBuilder( (int) _reader.BaseStream.Length );
//StringBuilder	sb = null;

			JSONObject	current = root;
			bool		needToExit = false;
			while ( !_reader.EndOfStream && !needToExit ) {
				char	C = (char) _reader.Read();
				if ( sb != null ) sb.Append( C );

				switch ( C ) {
					case '{': {
						// Enter a new dictionary
						JSONObject	v = new JSONObject( current, JSONState.DICTIONARY_EXPECT_KEY, new Dictionary<string,object>() );

						switch ( current.m_state ) {
							case JSONState.DICTIONARY_EXPECT_VALUE:
								current.AsDictionary.Add( current.m_currentKey, v );
								current.m_state = JSONState.DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END;
								current = v;
								break;

							case JSONState.ARRAY_EXPECT_ELEMENT:
								current.AsArray.Add( v );
								current.m_state = JSONState.ARRAY_EXPECT_SEPARATOR_OR_END;
								current = v;
								break;

							default:
								throw new Exception( "Encountered dictionary while not expecting a value!" );
						}
						break;
					}

					case '}': {
						if ( current.m_state != JSONState.DICTIONARY_EXPECT_KEY && current.m_state != JSONState.DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END )
							throw new Exception( "Exiting a collection early!" );
						current = current.m_previous;	// Restore previous object
						break;
					}

					case '[': {
						if ( current.m_state != JSONState.DICTIONARY_EXPECT_VALUE )
							throw new Exception( "Encountered array while not expecting a value!" );

						// Enter a new array
						current = new JSONObject( current, JSONState.ARRAY_EXPECT_ELEMENT, new List<object>() );
						break;
					}

					case ']': {
						if ( current.m_state != JSONState.ARRAY_EXPECT_ELEMENT && current.m_state != JSONState.ARRAY_EXPECT_SEPARATOR_OR_END )
							throw new Exception( "Exiting an array early!!" );

						JSONObject	value = current;

						// Restore previous object
						current = current.m_previous;
						if ( current.m_state != JSONState.DICTIONARY_EXPECT_VALUE )
							throw new Exception( "Finished parsing an array that is not an expected value!" );

						// Just parsed an array value
						current.AsDictionary.Add( current.m_currentKey, value );
						current.m_state = JSONState.DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END;
						current.m_currentKey = null;
						break;
					}

					case ' ':
					case '\t':
					case '\r':
					case '\n':
						// Just skip...
						break;

					case ':':
						if ( current.m_state != JSONState.DICTIONARY_EXPECT_KEY_SEPARATOR )
							throw new Exception( "Encountered separator not in dictionary!" );

						// Now expecting a value!
						current.m_state = JSONState.DICTIONARY_EXPECT_VALUE;
						break;

					case ',':
						switch ( current.m_state ) {
							case JSONState.DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END:
								// Now expecting a key again!
								current.m_state = JSONState.DICTIONARY_EXPECT_KEY;
								break;

							case JSONState.ARRAY_EXPECT_SEPARATOR_OR_END:
								// Now expecting an element again!
								current.m_state = JSONState.ARRAY_EXPECT_ELEMENT;
								break;

							default:
								throw new Exception( "Encountered separator not in dictionary!" );
						}
						break;

					case '"': {
						if ( current.m_state != JSONState.DICTIONARY_EXPECT_KEY && current.m_state != JSONState.DICTIONARY_EXPECT_VALUE )
							throw new Exception( "Encountered string not in dictionary key or value state!" );

						string	s = ReadString( C, _reader, sb );
						if ( current.m_state == JSONState.DICTIONARY_EXPECT_KEY ) {
							// Just parsed a key
							current.m_currentKey = s;
							current.m_state = JSONState.DICTIONARY_EXPECT_KEY_SEPARATOR;

							// Handle special case of end data we don't want to parse...
							if ( s == "sync_metadata" ) {
								needToExit = true;	// Early exit!
							}

						} else if ( current.m_state == JSONState.DICTIONARY_EXPECT_VALUE ) {
							// Just parsed a value
							current.AsDictionary.Add( current.m_currentKey, s );
							current.m_state = JSONState.DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END;
							current.m_currentKey = null;
						}
						break;
					}

					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						// Ignore numbers... (only encountered when parsing the "version"
						if ( current.m_state != JSONState.DICTIONARY_EXPECT_VALUE && current.m_state != JSONState.DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END )
							throw new Exception( "Encountered number not in dictionary value state!" );
						current.m_state = JSONState.DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END;
						break;

					default:
						throw new Exception( "Unexpected character '" + C + "'!" );
				}
			}

			return root;
		}

		StringBuilder	m_stringBuilder = new StringBuilder();
		string	ReadString( char _firstChar, StreamReader _reader, StringBuilder _sb ) {
			m_stringBuilder.Clear();

			char	C = (char) _reader.Read();
			while ( C != '"' ) {

				if ( C == '\\' ) {
					// Read escaped character
					char	escaped = (char) _reader.Read();
					switch( escaped ) {
						case '\\': C = '\\'; break;
						case 'r': C = '\r'; break;
						case 'n': C = '\n'; break;
						case '"': C = '"'; break;
						case 't': C = '\t'; break;
						case 'u': {
							string	strUnicode = "" + (char) _reader.Read() + (char) _reader.Read() + (char) _reader.Read() + (char) _reader.Read();
							int		unicode = 0;
							if ( int.TryParse( strUnicode, System.Globalization.NumberStyles.HexNumber, Application.CurrentCulture, out unicode ) ) {
								C = (char) unicode;
							}
							break;
						}

						default: throw new Exception( "Unrecognized escaped character \"" + C + escaped + "\"!" );
					}
				}

				m_stringBuilder.Append( C );
				if ( _sb != null ) _sb.Append( C );

				C = (char) _reader.Read();
			}

			return m_stringBuilder.ToString();
		}

		#endregion

		int	ImportBookmarksChrome( FileInfo _fileName ) {
			// Attempt to parse JSON file
			JSONObject	root = null;
			try {
				using ( StreamReader R = _fileName.OpenText() ) {
					root = ReadJSON( R );
				}
			} catch ( Exception _e ) {
				throw new Exception( "Failed to parse JSON file!", _e );
			}

			// Read bookmarks
			List< Bookmark >	bookmarks = new List<Bookmark>();
			try {
				root = root["root"]["roots"];	// Fetch the actual root
				foreach ( object value in root.AsDictionary.Values ) {
					JSONObject	rootFolder = value as JSONObject;
					if ( rootFolder != null ) {
						// Add a new root folder containing bookmarks
						bookmarks.Add( new Bookmark( m_owner.Database, null, rootFolder, bookmarks ) );
					}
				}
			} catch ( Exception _e ) {
				throw new Exception( "Failed to parse bookmarks!", _e );
			}

			// Now convert bookmarks into fiches
			int	successfullyImportedBookmarksCounter = 0;
			foreach ( Bookmark bookmark in bookmarks ) {
				try {
					if ( bookmark.m_parent == null )
						continue;	// Don't create root folders...

					// Reading the fiche property will create it
					Fiche	F = bookmark.Fiche;

					successfullyImportedBookmarksCounter++;
				} catch ( Exception _e ) {
					throw new Exception( "Failed to parse bookmarks!", _e );
				}
			}

			return successfullyImportedBookmarksCounter;
		}

		[System.Diagnostics.DebuggerDisplay( "{m_name} - {m_URL} ({m_GUID})" )]
		class	Bookmark {

			public enum TYPE {
				URL,	// An actual bookmark
				FOLDER,	// Only a folder containing other bookmarks
				UNKNOWN
			}

			private FichesDB		m_database;

			public Bookmark			m_parent;
			public Guid				m_GUID;
			public DateTime			m_dateAdded;
			public string			m_name;
			public Uri				m_URL;
			public TYPE				m_type = TYPE.UNKNOWN;
			public List< Bookmark >	m_children = new List<Bookmark>();

			// Cached fiche
			private Fiche			m_fiche = null;
			public Fiche			Fiche {
				get {
					if ( m_fiche != null )
						return m_fiche;	// Use cached version

					// Create the fiche
					switch ( m_type ) {
						case Bookmark.TYPE.FOLDER:
							// Create a "tag" fiche, if it doesn't already exist...
							m_fiche = m_database.SyncFindOrCreateTagFiche( m_name );
							break;

						case Bookmark.TYPE.URL:
							// Create a regular fiche
							Fiche	existingFiche = null;
							if ( m_GUID != Guid.Empty ) {
								existingFiche = m_database.FindFicheByGUID( m_GUID );
							} else {
								m_GUID = Guid.NewGuid();

								// Attempt to find the fiche by URL
								existingFiche = m_database.FindFicheByURL( m_URL );
								if ( existingFiche == null ) {
									// Attempt to find a unique fiche with this title (it's okay to have multiple fiches with the same title but we only consider the fiche already exists if there's a single one with this title!)
									Fiche[]	existingFiches = m_database.FindFichesByTitle( m_name, false );
									existingFiche = existingFiches.Length == 1 ? existingFiches[0] : null;
								}
							}

							if ( existingFiche != null && existingFiche.URL == m_URL ) {
								break;	// No need to create the fiche as it already exists...
							}

							// Attempt to retrieve parent fiches
							List< Fiche >	parents = new List<Fiche>();
							if ( m_parent != null ) {
								parents.Add( m_parent.Fiche );	// This should in turn create the parent fiche if it doesn't exist yet
							}
							string[]	tags = WebHelpers.ExtractTags( m_name );
							foreach ( string tag in tags ) {
								Fiche[]	tagFiches = m_database.FindFichesByTitle( tag, false );
								if ( tagFiches.Length > 0 )
									parents.Add( tagFiches[0] );
							}

							// Create the new fiche
							m_fiche = m_database.AsyncCreateURLFiche( Fiche.TYPE.REMOTE_ANNOTABLE_WEBPAGE, m_name, m_URL, null );
							m_fiche.AddTags( parents );
							m_fiche.GUID = m_GUID;
							break;
					}

					return m_fiche;
				}
			}

			public Bookmark( FichesDB _database, Bookmark _parent, JSONObject _JSON, List< Bookmark > _bookmarks ) {
				m_database = _database;
				m_parent = _parent;
				if ( _JSON == null || !_JSON.IsDictionary )
					throw new Exception( "Invalid JSON object type!" );

				Dictionary< string, object >	dictionary = _JSON.AsDictionary;
				foreach ( string key in dictionary.Keys ) {
					switch ( key ) {
						case "name":
							m_name = dictionary[key] as string;
							break;

						case "date_added":
							// From https://stackoverflow.com/questions/19074423/how-to-parse-the-date-added-field-in-chrome-bookmarks-file
							string	strTicks = dictionary[key] as string;
							long	microseconds;
 							if ( long.TryParse( strTicks, out microseconds ) ) {
								long	milliseconds = microseconds / 1000;
								long	seconds = milliseconds / 1000;
								long	minutes = seconds / 60;
								long	hours = minutes / 60;
								long	days = hours / 24;

								TimeSpan	delay = new TimeSpan( (int) days, (int) (hours % 24), (int) (minutes % 60), (int) (seconds % 60), (int) (milliseconds % 1000) );
								m_dateAdded = new DateTime( 1601, 1, 1 ) + delay;
 							}
							break;

						case "guid":
							string	strGUID = dictionary[key] as string;
							Guid.TryParse( strGUID, out m_GUID );
							break;

						case "url":
							string	strURL = dictionary[key] as string;
							Uri.TryCreate( strURL, UriKind.Absolute, out m_URL );
							break;

						case "children":
							RecurseImportBookmarks( this, dictionary[key] as JSONObject, _bookmarks );
							break;

						case "type":
							string	strType = dictionary[key] as string;
							switch ( strType ) {
								case "url": m_type = TYPE.URL; break;
								case "folder": m_type = TYPE.FOLDER; break;
							}
							break;

						default:
							// Try import children...
							RecurseImportBookmarks( this, dictionary[key] as JSONObject, _bookmarks );
							break;
					}
				}
			}

			public void	RecurseImportBookmarks( Bookmark _parent, JSONObject _object, List< Bookmark > _bookmarks ) {
				if ( _object == null )
					return;
//					throw new Exception( "Invalid JSON object!" );

				if ( _object.IsDictionary ) {
					Dictionary< string, object >	dictionary = _object.AsDictionary;
					foreach ( string key in dictionary.Keys ) {
						JSONObject	bookmarkObject = dictionary[key] as JSONObject;
						if ( bookmarkObject == null )
							continue;	// Can't parse value

						Bookmark	bookmark = new Bookmark( m_database, _parent, bookmarkObject, _bookmarks );
						_bookmarks.Add( bookmark );
						_parent.m_children.Add( bookmark );
					}
				} else if ( _object.IsArray ) {
					List< object >	array = _object.AsArray;
					foreach ( object element in array ) {
						JSONObject	bookmarkObject = element as JSONObject;
						if ( bookmarkObject == null )
							continue;	// Can't parse value

						// Each element must be a dictionary of properties for the bookmark
						Bookmark	bookmark = new Bookmark( m_database, _parent, bookmarkObject, _bookmarks );
						_bookmarks.Add( bookmark );
						_parent.m_children.Add( bookmark );
					}
// 
// 				} else if ( _object.m_object is string ) {

				} else {
					throw new Exception( "Unsupported JSON object type!" );
				}
			}
		}

		#endregion

		#endregion

		#region EVENTS

		private void buttonSelectRootDBFolder_Click(object sender, EventArgs e) {
			folderBrowserDialog.SelectedPath = RootDBFolder;
			if ( folderBrowserDialog.ShowDialog( this ) != DialogResult.OK )
				return;

			RootDBFolder = folderBrowserDialog.SelectedPath;
		}

		private void labelShortcut_Click(object sender, EventArgs e) {
			labelShortcut.Capture = true;
			labelShortcut.Text = "";	// Expecting keys...
		}

		private void labelShortcut_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
			if ( !labelShortcut.Capture )
				return;

// Also check https://stackoverflow.com/questions/400113/best-way-to-implement-keyboard-shortcuts-in-a-windows-forms-application

			labelShortcut.Text = e.KeyCode.ToString();
		}

		private void listViewBookmarks_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e) {
			buttonImportBookmarks.Enabled = e.Item != null && e.Item.Selected && e.Item.Tag != null;
		}

		private void buttonImportBookmarks_Click(object sender, EventArgs e) {
			int	totalImportedBookmarksCount = 0;
			foreach ( ListViewItem item in listViewBookmarks.SelectedItems ) {
				try {
					BookmarkFile	bookmark = item.Tag as BookmarkFile;

					switch ( bookmark.m_type ) {
						case BookmarkFile.BOOKMARK_TYPE.CHROME:
							totalImportedBookmarksCount += ImportBookmarksChrome( bookmark.m_fileName );
							break;
					}
				} catch ( Exception _e ) {
					BrainForm.MessageBox( "An error occurred while attempting to import bookmark file!", _e );
				}
			}

			if ( totalImportedBookmarksCount == 0 )
				BrainForm.MessageBox( "No bookmarks were imported...", MessageBoxButtons.OK, MessageBoxIcon.Warning );
			else
				BrainForm.MessageBox( totalImportedBookmarksCount.ToString() + " bookmarks were successfully imported!", MessageBoxButtons.OK, MessageBoxIcon.Information );
		}

		#endregion
	}
}
