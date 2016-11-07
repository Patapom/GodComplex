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

namespace DirectoryCompressor
{
	public partial class Form1 : Form {

		protected delegate void	ProgressDelegate( float _progress, int _identicalFilesCount );

		protected class	File {
			const int	BLOCK_SIZE = 1024 << 10;	// 1MB blocks

			public FileInfo		m_file = null;
			public string		m_fullName = null;
			public long			m_size = 0;
			public string		m_extension = "";

			public File( FileInfo _file ) {
				m_file = _file;
				m_size = _file.Length;
				m_extension = _file.Extension.ToLower();

				try {
					m_fullName = m_file.FullName;
				} catch ( Exception ) {
//					System.Reflection.PropertyInfo	hiddenPathProperty = m_file.GetType().GetProperty( "FullPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
//					System.Reflection.PropertyInfo[]	hiddenPathProperties = m_file.GetType().GetProperties( System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
// 					System.Reflection.FieldInfo[]	hiddenPathFields = m_file.GetType().GetFields( System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
// 					System.Reflection.FieldInfo		hiddenPathField = hiddenPathFields.Single( ( pi ) => { return pi.Name == "FullPath"; } );
					System.Reflection.FieldInfo	hiddenPathField = m_file.GetType().GetField( "FullPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
					m_fullName = hiddenPathField != null ? hiddenPathField.GetValue( m_file ) as string : m_file.Name;
				}
			}

			public override string ToString() {
				return m_fullName;
			}

			/// <summary>
			/// Performs a binary comparison of 2 files, returns true if both files are identical
			/// </summary>
			/// <param name="_other"></param>
			/// <returns></returns>
			public bool	CompareBinary( File _other ) {
				if ( m_size != _other.m_size )
					return false;

				byte[]		compareBlock0 = new byte[BLOCK_SIZE];
				byte[]		compareBlock1 = new byte[BLOCK_SIZE];
				using ( FileStream S0 = m_file.OpenRead() ) {
					using ( FileStream S1 = m_file.OpenRead() ) {
						S0.Read( compareBlock0, 0, BLOCK_SIZE );
						S1.Read( compareBlock1, 0, BLOCK_SIZE );
						if ( !compareBlock0.SequenceEqual( compareBlock1 ) )
							return false;	// This block differs...
					}
				}

				return true;
			}
		}

		/// <summary>
		/// Contains all the files with the same extension
		/// </summary>
		protected class ExtensionFilesGroup {
			public string		m_extension = null;
			public List< File >	m_files = new List< File >();

			public ExtensionFilesGroup( string _extension ) {
				m_extension = _extension;
			}

			public void		AddFile( File _file ) {
				m_files.Add( _file );
			}

			public List< Tuple< File, File > >	Compare( ExtensionFilesGroup _other, ProgressDelegate _progress ) {
				int	maxFilesCount = Math.Max( m_files.Count, _other.m_files.Count );

				// First, compare file sizes
				Dictionary< long, Tuple< List< File >, List< File > > >	size2Files = new Dictionary< long, Tuple< List< File >, List< File > > >( maxFilesCount );
				foreach ( File file in m_files ) {
					if ( !size2Files.ContainsKey( file.m_size ) )
						size2Files.Add( file.m_size, new Tuple< List< File >, List< File > >( new List< File >(), new List< File >() ) );
					size2Files[file.m_size].Item1.Add( file );
				}
				foreach ( File file in _other.m_files ) {
					if ( !size2Files.ContainsKey( file.m_size ) )
						size2Files.Add( file.m_size, new Tuple< List< File >, List< File > >( new List< File >(), new List< File >() ) );
					size2Files[file.m_size].Item2.Add( file );
				}

				// Compare among files of the same size
				List< Tuple< File, File > >	result = new List< Tuple< File, File > >( maxFilesCount );

				int	sameSizeFilesProgressCount = Math.Max( 1, size2Files.Values.Count / 100 );
				int	sameSizeFilesIndex = 0;
				foreach ( Tuple< List< File >, List< File > > sameSizeFiles in size2Files.Values ) {
					if ( (++sameSizeFilesIndex % sameSizeFilesProgressCount) == 0 ) {
						// Notify of progress
						_progress( (float) sameSizeFilesIndex / size2Files.Values.Count, result.Count );
					}

					for ( int fileIndex0=0; fileIndex0 < sameSizeFiles.Item1.Count; fileIndex0++ ) {
						File	file0 = sameSizeFiles.Item1[fileIndex0];
						for ( int fileIndex1=0; fileIndex1 < sameSizeFiles.Item2.Count; fileIndex1++ ) {
							File	file1 = sameSizeFiles.Item2[fileIndex1];
							if ( file0.m_fullName == file1.m_fullName )
								continue;	// Ignore same files...

							if ( file0.CompareBinary( file1 ) ) {
								result.Add( new Tuple<File,File>( file0, file1 ) );	// Same!
							}
						}
					}
				}
				return result;
			}
		}

		protected Microsoft.Win32.RegistryKey	m_appKey = null;

		protected List< File >								m_filesLeft = new List< File >();
		protected Dictionary< string, ExtensionFilesGroup >	m_extensionGroupLeft = new Dictionary< string, ExtensionFilesGroup >();
		protected List< File >								m_filesRight = new List< File >();
		protected Dictionary< string, ExtensionFilesGroup >	m_extensionGroupRight = new Dictionary< string, ExtensionFilesGroup >();

		protected List< Tuple< File, File > >	m_identicalFiles = new List< Tuple< File, File > >();

		public Form1() {
			InitializeComponent();

			m_appKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( @"Software\Patapom\DirectoryCompressor" );

			string	leftDir = m_appKey.GetValue( "LastDirectoryLeft", "" ) as string;
			if ( Directory.Exists( leftDir ) )
				SetDirectoryLeft( new DirectoryInfo( leftDir ) );

			string	rightDir = m_appKey.GetValue( "LastDirectoryRight", "" ) as string;
			if ( Directory.Exists( rightDir ) )
				SetDirectoryRight( new DirectoryInfo( rightDir ) );
		}

		void	PopulateListBox( List< File > _files, ListBox _listBox, Label _label, Dictionary< string, ExtensionFilesGroup > _extensionGroup ) {
			_listBox.SuspendLayout();
			_listBox.Items.Clear();
			_listBox.Items.AddRange( _files.ToArray() );
			_listBox.ResumeLayout();

			// Build the extension group
			_extensionGroup.Clear();
			foreach ( File F in _files ) {
				if ( !_extensionGroup.ContainsKey( F.m_extension ) )
					_extensionGroup.Add( F.m_extension, new ExtensionFilesGroup( F.m_extension ) );
				ExtensionFilesGroup	group = _extensionGroup[F.m_extension];
				group.AddFile( F );
			}

			_label.Text = _files.Count + " files. " + _extensionGroup.Keys.Count + " extensions.";
		}

		void	SetDirectoryLeft( DirectoryInfo _directory ) {
			textBoxDirectoryLeft.Text = _directory.FullName;

			bool	taskDone = false;
			Task.Run( () => {
				m_filesLeft.Clear();
				IEnumerable< FileInfo >	files = _directory.EnumerateFiles( "*.*", SearchOption.AllDirectories );
				foreach ( FileInfo file in files ) {
					File	F = new File( file );
					m_filesLeft.Add( F );
				}
				taskDone = true;
			} );

			while ( !taskDone )
				System.Threading.Thread.Sleep( 500 );

			PopulateListBox( m_filesLeft, listBoxFilesLeft, labelFilesLeft, m_extensionGroupLeft );

			buttonCompare.Enabled = m_extensionGroupLeft.Count > 0 && m_extensionGroupRight.Count > 0;
		}
		void	SetDirectoryRight( DirectoryInfo _directory ) {
			textBoxDirectoryRight.Text = _directory.FullName;

			bool	taskDone = false;
			Task.Run( () => {
				m_filesRight.Clear();
				IEnumerable< FileInfo >	files = _directory.EnumerateFiles( "*.*", SearchOption.AllDirectories );
				foreach ( FileInfo file in files ) {
					File	F = new File( file );
					m_filesRight.Add( F );
				}
				taskDone = true;
			} );

			while ( !taskDone )
				System.Threading.Thread.Sleep( 500 );

			PopulateListBox( m_filesRight, listBoxFilesRight, labelFilesRight, m_extensionGroupRight );

			buttonCompare.Enabled = m_extensionGroupLeft.Count > 0 && m_extensionGroupRight.Count > 0;
		}

		void	Compare() {
			progressBar.Value = 0;
			progressBar.Visible = true;

			m_identicalFiles.Clear();
			int		extensionIndex = 0;
			float	currentProgress = 0.0f;
			foreach ( string extension in m_extensionGroupLeft.Keys ) {
				if ( !m_extensionGroupRight.ContainsKey( extension ) )
					continue;

				ExtensionFilesGroup	leftGroup = m_extensionGroupLeft[extension];
				ExtensionFilesGroup	rightGroup = m_extensionGroupRight[extension];

				float	extensionProgress = (float) leftGroup.m_files.Count / m_filesLeft.Count;
				float	currentExtensionProgress = extensionIndex++ * extensionProgress;

				List< Tuple< File, File > >	identicalFiles = leftGroup.Compare( rightGroup, ( float _progress, int _identicalFilesCount ) => {
					progressBar.Value = (int) (progressBar.Maximum * (currentProgress + _progress * extensionProgress));
					labelResults.Text = "Processing " + leftGroup.m_files.Count + " \"*" + extension + "\" files. " + _identicalFilesCount + " identical so far. Total = " + (m_identicalFiles.Count + _identicalFilesCount) + " identical files of all types.";
					progressBar.Refresh();
					labelResults.Refresh();
				} );
				m_identicalFiles.AddRange( identicalFiles );

				currentProgress += extensionProgress;
			}

			labelResults.Text = m_identicalFiles.Count + " identical files that can be optimized.";
			progressBar.Visible = false;
		}

		private void buttonBrowseDirectoryLeft_Click(object sender, EventArgs e) {
			string	leftDir = m_appKey.GetValue( "LastDirectoryLeft", "" ) as string;
			if ( Directory.Exists( leftDir ) )
				folderBrowserDialog.SelectedPath = leftDir;
			if ( folderBrowserDialog.ShowDialog( this ) != DialogResult.OK )
				return;
			m_appKey.SetValue( "LastDirectoryLeft", folderBrowserDialog.SelectedPath );

			SetDirectoryLeft( new DirectoryInfo( folderBrowserDialog.SelectedPath ) );
		}

		private void buttonBrowseDirectoryRight_Click(object sender, EventArgs e) {
			string	rightDir = m_appKey.GetValue( "LastDirectoryRight", "" ) as string;
			if ( Directory.Exists( rightDir ) )
				folderBrowserDialog.SelectedPath = rightDir;
			if ( folderBrowserDialog.ShowDialog( this ) != DialogResult.OK )
				return;
			m_appKey.SetValue( "LastDirectoryRight", folderBrowserDialog.SelectedPath );

			SetDirectoryRight( new DirectoryInfo( folderBrowserDialog.SelectedPath ) );
		}

		private void buttonCompare_Click(object sender, EventArgs e) {
			Compare();
		}
	}
}
