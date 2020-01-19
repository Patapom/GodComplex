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

		protected override bool Sizeable => false;
		protected override bool CloseOnEscape => false;
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

			// Display shortcuts
			labelShortcutToggle.Text = m_shortcuts[0].ToString();
			labelShortcutPaste.Text = m_shortcuts[1].ToString();
			labelShortcutNew.Text = m_shortcuts[2].ToString();

			// Attempt to locate known browsers' bookmarks
			LocateBookmarks();
		}

		protected override void InternalDispose() {
			UnRegisterHotKeys();
		}

		#region Shortcuts Management

		public class Shortcut {
			public enum SHORTCUT {
				TOGGLE,	// Toggle the application
				PASTE,	// Global paste of clipboard content
				NEW,	// Create a new manual fiche
				WEBCAM,	// Create a new webcam screenshot fiche
			}

			public SHORTCUT						m_type;
			public Interop.NativeModifierKeys	m_modifier;
			public Keys							m_key;
			public int							m_ID = -1;

			public override string ToString() {
				string	str = "";
				if ( m_modifier != Interop.NativeModifierKeys.None ) {
					if ( (m_modifier & Interop.NativeModifierKeys.Win) != 0 )
						str += "Win+";
					if ( (m_modifier & Interop.NativeModifierKeys.Control) != 0 )
						str += "Ctrl+";
					if ( (m_modifier & Interop.NativeModifierKeys.Alt) != 0 )
						str += "Alt+";
					if ( (m_modifier & Interop.NativeModifierKeys.Shift) != 0 )
						str += "Shift+";
				}
				str += m_key;
				return str;
			}

			public delegate void	KeyCombinationUpdateHandler( Shortcut _S, Interop.NativeModifierKeys _newModifiers, Keys _newKey, bool _canceled );

			public void	HandleKeyCombination( Interop.NativeModifierKeys _modifiers, Keys _key, KeyCombinationUpdateHandler _update ) {
				if ( _key != Keys.Y )
					return;
				bool	canceled = _key == Keys.Escape;

				_update( this, _modifiers, _key, canceled );
			}

			public void	Update( PreviewKeyDownEventArgs e ) {
				m_modifier = Interop.NativeModifierKeys.None;
				if ( (e.Modifiers & Keys.LWin) != 0 ) m_modifier |= Interop.NativeModifierKeys.Win;
				if ( e.Control ) m_modifier |= Interop.NativeModifierKeys.Control;
				if ( e.Alt ) m_modifier |= Interop.NativeModifierKeys.Alt;
				if ( e.Shift ) m_modifier |= Interop.NativeModifierKeys.Shift;
				m_key = e.KeyCode;
			}
			public void Update( Interop.NativeModifierKeys _modifiers, Keys _key ) {
				m_modifier = _modifiers;
				m_key = _key;
			}
		}

		private Shortcut[]	m_shortcuts = new Shortcut[] {
			new Shortcut() { m_type = Shortcut.SHORTCUT.TOGGLE,	m_modifier = Interop.NativeModifierKeys.Win, m_key = Keys.X },
			new Shortcut() { m_type = Shortcut.SHORTCUT.PASTE,	m_modifier = Interop.NativeModifierKeys.Win, m_key = Keys.V },
			new Shortcut() { m_type = Shortcut.SHORTCUT.NEW,	m_modifier = Interop.NativeModifierKeys.Win, m_key = Keys.N },
			new Shortcut() { m_type = Shortcut.SHORTCUT.WEBCAM,	m_modifier = Interop.NativeModifierKeys.Win, m_key = Keys.W },
		};

		public void	RegisterHotKeys() {
			int	keyID = 0;
			foreach ( Shortcut S in m_shortcuts ) {
				try {
					Interop.RegisterHotKey( m_owner, keyID, S.m_modifier, S.m_key );
					S.m_ID = keyID++;
				} catch ( Exception _e ) {
					// Maybe already hooked?
					BrainForm.LogError( new Exception( "Failed to register " + S.m_type + " hotkey", _e ) );
				}
			}
		}

		public void	UnRegisterHotKeys() {
			foreach ( Shortcut S in m_shortcuts ) {
				try {
					if ( S.m_ID >= 0 )
						Interop.UnregisterHotKey( m_owner.Handle, S.m_ID );
				} catch ( Exception _e ) {
					BrainForm.LogError( new Exception( "Failed to unregister " + S.m_type + " hotkey", _e ) );
				}
			}
		}

		public Shortcut	HandleHotKey( Interop.NativeModifierKeys _modifier, Keys _key ) {
			foreach ( Shortcut S in m_shortcuts ) {
				if ( _modifier == S.m_modifier && _key == S.m_key )
					return S;
			}
			return null;
		}

		private Shortcut	m_currentlyEditingShortcut = null;
		private Label		m_currentlyEditingShortcutLabel = null;

		protected override bool ProcessKeyPreview(ref Message m) {
			if ( m_currentlyEditingShortcut == null )
				return base.ProcessKeyPreview(ref m);

			// Let's handle the message ourselves!
// 			Keys						key = (Keys) (((int)m.LParam >> 16) & 0xFFFF);
// 			Interop.NativeModifierKeys	modifiers = (Interop.NativeModifierKeys) ((int)m.LParam & 0xFFFF);

			Interop.NativeModifierKeys	modifiers = Interop.NativeModifierKeys.None;
			Keys						key = (Keys) m.WParam.ToInt32();

			m_currentlyEditingShortcut.HandleKeyCombination( modifiers, key, ( Shortcut _S, Interop.NativeModifierKeys _newModifiers, Keys _newKey, bool _canceled ) => {
				if ( !_canceled ) {
					_S.Update( _newModifiers, _newKey );		// Validate combination
				}

				RegisterHotKeys();
				m_currentlyEditingShortcutLabel.Text = _S.ToString();
			} );

			return true;	// Processed!
		}

		#endregion

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


// if ( bookmarkFile.FullName.IndexOf( "Profile 2" ) != -1 )
// 	ImportBookmarksChrome( bookmarkFile );


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

		int	ImportBookmarksChrome( FileInfo _fileName, List< Fiche > _createdTags, List< Fiche > _complexNameTags ) {
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
			Bookmark.ms_tags = _createdTags;
			Bookmark.ms_complexNameTags = _complexNameTags;
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

					// Reading the fiche property will create it and create its parent tags as well...
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

			// List of tags that may be too complex
			public static List< Fiche >	ms_tags;// = new List<Fiche>();
			public static List< Fiche >	ms_complexNameTags;// = new List<Fiche>();

			// Cached fiche
			private Fiche			m_fiche = null;
			public Fiche			Fiche {
				get {
					if ( m_fiche != null )
						return m_fiche;	// Use cached version

					// Create the fiche
					switch ( m_type ) {
						case Bookmark.TYPE.FOLDER: {
							// Folders are like tags, we will create the tag if it doesn't exist or reference it otherwise
							if ( m_GUID != Guid.Empty ) {
								m_fiche = m_database.FindFicheByGUID( m_GUID );
							} else {
								m_GUID = Guid.NewGuid();
							}

							if ( m_fiche == null ) {
								// Create a "tag" fiche...

								// Check the tag name is not too complex...
								string[]	tagWords = m_name.Split( ' ' );
								bool		tooComplex = false;
								string		name = "";
								if ( tagWords.Length >= 4 )
									tooComplex = true;	// Starts getting complex!
								float	averageWordLength = 0;
								foreach ( string word in tagWords ) {
									averageWordLength += word.Length;
									string	alphaNumericalWord = WebHelpers.MakeAlphaNumerical( word );
									if ( alphaNumericalWord != word )
										tooComplex = true;	// Invalid characters

									name += "_" + alphaNumericalWord;
								}
								name = name.Substring( 1 );	// Remove header '_'
								averageWordLength /= Math.Max( 2, tagWords.Length );	// Single-words are privileged, even though the word is long!
								if ( averageWordLength >= 6 )
									tooComplex = true;	// Starts getting complex!

								// Check if a fiche with the same name and URL exist
								Fiche[]	existingFiches = m_database.FindFichesByTitle( name, false );
								foreach ( Fiche existingFiche in existingFiches ) {
									if ( existingFiche.URL == null ) {
										// So we have a small fiche with the same name and an empty URL, which is exactly the fiche we would create
										// Let's re-use the existing fiche instead...
										m_fiche = existingFiche;
										break;
									}
								}

								if ( m_fiche == null ) {
									// Create the tag fiche
									m_fiche = m_database.SyncFindOrCreateTagFiche( name );
									m_fiche.GUID = m_GUID;
									m_fiche.CreationTime = m_dateAdded;

									// Save whenever possible
									m_database.AsyncSaveFiche( m_fiche, true, false );

									// Register new tag
									ms_tags.Add( m_fiche );
									if ( tooComplex )
										ms_complexNameTags.Add( m_fiche );
								}
							}
							break;
						}

						case Bookmark.TYPE.URL: {
							// Create a regular fiche
							Fiche	existingFiche = null;
							if ( m_GUID != Guid.Empty ) {
								existingFiche = m_database.FindFicheByGUID( m_GUID );
							} else {
								m_GUID = Guid.NewGuid();

								// Attempt to find the fiche by URL
								Fiche[]	existingFiches = m_database.FindFichesByURL( m_URL );
								existingFiche = existingFiches != null ? existingFiches[0] : null;
								if ( existingFiche == null ) {
									// Attempt to find a unique fiche with this title (it's okay to have multiple fiches with the same title but we only consider the fiche already exists if there's a single one with this title!)
									existingFiches = m_database.FindFichesByTitle( m_name, false );
									existingFiche = existingFiches.Length == 1 ? existingFiches[0] : null;
								}
							}

							if ( existingFiche != null && existingFiche.URL == m_URL ) {
								break;	// No need to create the fiche as it already exists...
							}

							// Attempt to retrieve parent fiches unless they're root folders (the root folders are usually useless)
							List< Fiche >	parents = new List<Fiche>();
							if ( m_parent != null && m_parent.m_parent != null ) {
								parents.Add( m_parent.Fiche );	// This should in turn create the parent fiche if it doesn't exist yet
							}
							string[]	tags = WebHelpers.ExtractTags( m_name );
							foreach ( string tag in tags ) {
								Fiche[]	tagFiches = m_database.FindFichesByTitle( tag, false );
								if ( tagFiches.Length > 0 )
									parents.Add( tagFiches[0] );
							}

							// Create the new fiche
							m_fiche = m_database.SyncCreateFicheDescriptor( Fiche.TYPE.REMOTE_ANNOTABLE_WEBPAGE, m_name, m_URL, parents.ToArray(), null );
							m_fiche.GUID = m_GUID;
							m_fiche.CreationTime = m_dateAdded;

							// Asynchronously load content & save the fiche when ready
							m_database.AsyncLoadContentAndSaveFiche( m_fiche, true );
							break;
						}
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
							m_URL = WebHelpers.CreateCanonicalURL( strURL );
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
					// Create all bookmark objects in the dictionary
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
					// Create all bookmark objects in the array
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

		#region Shortcuts Edition

// Also check https://stackoverflow.com/questions/400113/best-way-to-implement-keyboard-shortcuts-in-a-windows-forms-application

		void	SetCurrentlyEditingShortcut( Shortcut _S, Label _label ) {
			m_currentlyEditingShortcut = _S;
			m_currentlyEditingShortcutLabel = _label;

			m_currentlyEditingShortcutLabel.Text = "...";
			UnRegisterHotKeys();
		}

		private void labelShortcutToggle_Click(object sender, EventArgs e) {
			SetCurrentlyEditingShortcut( m_shortcuts[0], sender as Label );
		}

		private void labelShortcutPaste_Click(object sender, EventArgs e) {
			SetCurrentlyEditingShortcut( m_shortcuts[1], sender as Label );
		}

		private void labelShortcutNew_Click(object sender, EventArgs e) {
			SetCurrentlyEditingShortcut( m_shortcuts[2], sender as Label );
		}

		private void labelShortcutWebcam_Click(object sender, EventArgs e) {
			SetCurrentlyEditingShortcut( m_shortcuts[3], sender as Label );
		}

		#endregion

		private void listViewBookmarks_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e) {
			buttonImportBookmarks.Enabled = e.Item != null && e.Item.Selected && e.Item.Tag != null;
		}

		private void buttonImportBookmarks_Click(object sender, EventArgs e) {
			int				totalImportedBookmarksCount = 0;
			List< Fiche >	createdTags = new List<Fiche>();
			List< Fiche >	complexNameTags = new List<Fiche>();
			foreach ( ListViewItem item in listViewBookmarks.SelectedItems ) {
				try {
					BookmarkFile	bookmark = item.Tag as BookmarkFile;

					switch ( bookmark.m_type ) {
						case BookmarkFile.BOOKMARK_TYPE.CHROME:
							totalImportedBookmarksCount += ImportBookmarksChrome( bookmark.m_fileName, createdTags, complexNameTags );
							break;
					}
				} catch ( Exception _e ) {
					BrainForm.MessageBox( "An error occurred while attempting to import bookmark file!", _e );
				}
			}

			if ( totalImportedBookmarksCount == 0 ) {
				BrainForm.MessageBox( "No bookmarks were imported...", MessageBoxButtons.OK, MessageBoxIcon.Warning );
				return;
			}

			if ( complexNameTags.Count == 0 ) {
				BrainForm.MessageBox( (totalImportedBookmarksCount - createdTags.Count).ToString() + " bookmarks were successfully imported.\r\n" + createdTags.Count + " tags have been discovered.", MessageBoxButtons.OK, MessageBoxIcon.Information );
				return;
			}

			// Ask the user to rename the tags that were marked as "too complex"
			BrainForm.MessageBox( (totalImportedBookmarksCount - createdTags.Count).ToString() + " bookmarks were successfully imported.\r\n" + createdTags.Count + " tags have been discovered but " + complexNameTags.Count + " of them have names that are deemed too complex."
				+ "\r\n\r\nClick OK to open the list where you can rename them into easier-to-read tags (this is totally optional, you can use long tag names if they seem okay to you!).", MessageBoxButtons.OK, MessageBoxIcon.Warning );

			// Ask the user to rename complex names
			ComplexTagNamesForm	F = new ComplexTagNamesForm( Bookmark.ms_complexNameTags.ToArray() );
			F.ShowDialog( this );
		}

		#endregion
	}
}
