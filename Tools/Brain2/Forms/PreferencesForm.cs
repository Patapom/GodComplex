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

		enum JSONState {
			ROOT,
			DICTIONARY_EXPECT_KEY,
			DICTIONARY_EXPECT_VALUE,
			DICTIONARY_EXPECT_KEY_SEPARATOR,			// Read key, expecting separator
			DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END,	// Read value, expecting separator or collection end
			ARRAY_EXPECT_ELEMENT,
			ARRAY_EXPECT_SEPARATOR_OR_END,
		}

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

			public Dictionary<string,object>	AsCollection { get {return m_object as Dictionary<string,object>; } }
			public bool							IsCollection { get { return AsCollection != null; } }

			public List<object>					AsArray { get {return m_object as List<object>; } }
			public bool							IsArray { get { return AsArray != null; } }
		}

		int	ImportBookmarksChrome( FileInfo _fileName ) {
			JSONObject	root = new JSONObject( null, JSONState.DICTIONARY_EXPECT_VALUE, new Dictionary<string,object>() );
						root.m_currentKey = "root";

			JSONObject	current = root;
			using ( StreamReader R = _fileName.OpenText() )
				while ( !R.EndOfStream ) {
					char	C = (char) R.Read();
					switch ( C ) {
						case '{': {
							// Enter a new dictionary
							JSONObject	v = new JSONObject( current, JSONState.DICTIONARY_EXPECT_KEY, new Dictionary<string,object>() );

							switch ( current.m_state ) {
								case JSONState.DICTIONARY_EXPECT_VALUE:
									current.AsCollection.Add( current.m_currentKey, v );
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
							current = current.m_previous;	// Restore previous object
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

							string	s = ReadString( C, R );
							if ( current.m_state == JSONState.DICTIONARY_EXPECT_KEY ) {
								// Just parsed a key
								current.m_currentKey = s;
								current.m_state = JSONState.DICTIONARY_EXPECT_KEY_SEPARATOR;
							} else if ( current.m_state == JSONState.DICTIONARY_EXPECT_VALUE ) {
								// Just parsed a value
								current.AsCollection.Add( current.m_currentKey, s );
								current.m_state = JSONState.DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END;
								current.m_currentKey = null;
							}
							break;
						}

						default:
							throw new Exception( "Unexpected character '" + C + "'!" );
					}
				}

			return 0;
		}

		string	ReadString( char _firstChar, StreamReader _reader ) {
			string	result = "";
			char	C = (char) _reader.Read();
			while ( C != '"' ) {
				result += C;
				C = (char) _reader.Read();
			}
			return result;
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
