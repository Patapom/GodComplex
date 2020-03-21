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
		public override Keys ShortcutKey => Keys.F9;

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
//			new Shortcut() { m_type = Shortcut.SHORTCUT.TOGGLE,	m_modifier = Interop.NativeModifierKeys.Win, m_key = Keys.Space },	// Unfortunately already used by Windows for some useless stuff... :/
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

		int	ImportBookmarksChrome( FileInfo _fileName, List< Fiche > _createdTags, List< Fiche > _complexNameTags ) {
			// Attempt to parse JSON file
			WebServices.JSON			parser = new WebServices.JSON();
			WebServices.JSON.JSONObject	root = null;
			try {
				using ( StreamReader R = _fileName.OpenText() ) {
					root = parser.ReadJSON( R );
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
					WebServices.JSON.JSONObject	rootFolder = value as WebServices.JSON.JSONObject;
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
								m_GUID = m_database.CreateGUID();
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
									m_fiche = m_database.Sync_FindOrCreateTagFiche( name );
									m_fiche.GUID = m_GUID;
									m_fiche.CreationTime = m_dateAdded;

									// Save whenever possible
									m_database.Async_SaveFiche( m_fiche, true, false );

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
								m_GUID = m_database.CreateGUID();

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
BrainForm.LogWarning( "Fiche with URL " + existingFiche.URL + " already exists! @TODO: MERGE TAGS!" );
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
// 							m_fiche = m_database.Sync_CreateFicheDescriptor( Fiche.TYPE.REMOTE_ANNOTABLE_WEBPAGE, m_name, m_URL, parents.ToArray(), null );
// 							m_fiche.GUID = m_GUID;
// 							m_fiche.CreationTime = m_dateAdded;
// 
// 							// Asynchronously load content & save the fiche when ready
//							m_database.Async_LoadContentAndSaveFiche( m_fiche, true );

							m_fiche = URLHandler.CreateURLFiche( m_database, m_name, m_URL );
							if ( m_fiche != null ) {
								m_fiche.GUID = m_GUID;
 								m_fiche.CreationTime = m_dateAdded;
								m_fiche.AddTags( parents.ToArray() );
							}

							break;
						}
					}

					return m_fiche;
				}
			}

			public Bookmark( FichesDB _database, Bookmark _parent, WebServices.JSON.JSONObject _JSON, List< Bookmark > _bookmarks ) {
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
							RecurseImportBookmarks( this, dictionary[key] as WebServices.JSON.JSONObject, _bookmarks );
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
							RecurseImportBookmarks( this, dictionary[key] as WebServices.JSON.JSONObject, _bookmarks );
							break;
					}
				}
			}

			public void	RecurseImportBookmarks( Bookmark _parent, WebServices.JSON.JSONObject _object, List< Bookmark > _bookmarks ) {
				if ( _object == null )
					return;
//					throw new Exception( "Invalid JSON object!" );

				if ( _object.IsDictionary ) {
					// Create all bookmark objects in the dictionary
					Dictionary< string, object >	dictionary = _object.AsDictionary;
					foreach ( string key in dictionary.Keys ) {
						WebServices.JSON.JSONObject	bookmarkObject = dictionary[key] as WebServices.JSON.JSONObject;
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
						WebServices.JSON.JSONObject	bookmarkObject = element as WebServices.JSON.JSONObject;
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
