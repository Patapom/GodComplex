using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;

namespace PhilBackup
{
	public partial class BackupForm : Form
	{
		const int	SOURCE_FOLDERS_COUNT = 3;

		RegistryKey		m_ROOTKey = null;
		DirectoryInfo[]	m_SourceFolders = new DirectoryInfo[SOURCE_FOLDERS_COUNT];
		DirectoryInfo	m_TargetFolder = null;

		public BackupForm()
		{
			InitializeComponent();

			// Read settings from the registry
			m_ROOTKey = Registry.CurrentUser.CreateSubKey( @"Software\Patapom\PhilBackup\" );

			string	ExecutableDir = Path.GetDirectoryName( Application.ExecutablePath );
			for ( int i=0; i < SOURCE_FOLDERS_COUNT; i++ )
				m_SourceFolders[i] = new DirectoryInfo( m_ROOTKey.GetValue( "SourceFolder" + i, ExecutableDir ) as string );
			m_TargetFolder = new DirectoryInfo( m_ROOTKey.GetValue( "TargetFolder", ExecutableDir ) as string );

			bool	bCreateFolder = true;
			bool.TryParse( m_ROOTKey.GetValue( "CreateTimeStamp", "true" ) as string, out bCreateFolder );
			checkBoxCreateDateFolder.Checked = bCreateFolder;

			UpdateState();
		}

		protected override void OnClosing( CancelEventArgs e )
		{
			// Write settings to the registry
			for ( int i=0; i < SOURCE_FOLDERS_COUNT; i++ )
				m_ROOTKey.SetValue( "SourceFolder" + i, m_SourceFolders[i].FullName );
			m_ROOTKey.SetValue( "TargetFolder", m_TargetFolder.FullName );
			m_ROOTKey.SetValue( "CreateTimeStamp", checkBoxCreateDateFolder.Checked.ToString() );

			base.OnClosing( e );
		}

		void	UpdateState()
		{
			bool	Okay = true;
			for ( int i=0; i < SOURCE_FOLDERS_COUNT; i++ )
				Okay &= m_SourceFolders[i].Exists;
			Okay &= m_TargetFolder.Exists;

			buttonStart.Enabled = Okay;

			textBox1.Text = m_SourceFolders[0].FullName;
			textBox2.Text = m_SourceFolders[1].FullName;
			textBox3.Text = m_SourceFolders[2].FullName;
			textBoxTarget.Text = m_TargetFolder.FullName;
		}

		private void button1_Click( object sender, EventArgs e )
		{
			folderBrowserDialog1.SelectedPath = m_SourceFolders[0].FullName;
			if ( folderBrowserDialog1.ShowDialog( this ) != DialogResult.OK )
				return;

			m_SourceFolders[0] = new DirectoryInfo( folderBrowserDialog1.SelectedPath );
			UpdateState();
		}

		private void button2_Click( object sender, EventArgs e )
		{
			folderBrowserDialog1.SelectedPath = m_SourceFolders[1].FullName;
			if ( folderBrowserDialog1.ShowDialog( this ) != DialogResult.OK )
				return;

			m_SourceFolders[1] = new DirectoryInfo( folderBrowserDialog1.SelectedPath );
			UpdateState();
		}

		private void button3_Click( object sender, EventArgs e )
		{
			folderBrowserDialog1.SelectedPath = m_SourceFolders[2].FullName;
			if ( folderBrowserDialog1.ShowDialog( this ) != DialogResult.OK )
				return;

			m_SourceFolders[2] = new DirectoryInfo( folderBrowserDialog1.SelectedPath );
			UpdateState();
		}

		private void buttonTarget_Click( object sender, EventArgs e )
		{
			folderBrowserDialog1.SelectedPath = m_TargetFolder.FullName;
			if ( folderBrowserDialog1.ShowDialog( this ) != DialogResult.OK )
				return;

			m_TargetFolder = new DirectoryInfo( folderBrowserDialog1.SelectedPath );
			UpdateState();
		}

		delegate bool	FileVisitor( FileInfo _File );
		private bool	RecurseVisit( DirectoryInfo _Directory, FileVisitor _Visitor )
		{
			// Visit files
			FileInfo[]	Files = _Directory.GetFiles();
			foreach ( FileInfo File in Files )
				if ( !_Visitor( File ) )
					return false;

			// Recurse through directories
			DirectoryInfo[]	Directories = _Directory.GetDirectories();
			foreach ( DirectoryInfo Dir in Directories )
				if ( !RecurseVisit( Dir, _Visitor ) )
					return false;

			return true;
		}

		bool	bStarted = false;
		bool	bCancel = false;
		private void buttonStart_Click( object sender, EventArgs e )
		{
			if ( !bStarted )
			{
				// Disable all controls except start button that now becomes cancel
				panelInfos.Enabled = false;
				buttonStart.Text = "Cancel";
				bStarted = true;
				bCancel = false;

				// Estimate total size
				Dictionary<FileInfo,string>	ErrorFiles = new Dictionary<FileInfo,string>();
				long	TotalSize = 0;
				long	TotalFiles = 0;
				progressBar1.Style = ProgressBarStyle.Marquee;
				for ( int i=0; i < SOURCE_FOLDERS_COUNT && !bCancel; i++ )
					RecurseVisit( m_SourceFolders[i], ( FileInfo _File ) =>
						{
							try
							{
								TotalSize += _File.Length;
								TotalFiles++;

								// Check events every 256 files for a chance to press cancel...
								if ( (TotalFiles & 0xFF) == 0 )
									Application.DoEvents();
							}
							catch ( Exception _e )
							{
								ErrorFiles.Add( _File, _e.Message );
							}

							return !bCancel;
						} );

				// Start copying
				long	UpdateProgressFilesCount = Math.Max( 1, TotalFiles / 100 );
				long	SumCopiedSize = 0;
				long	TotalCopiedFiles = 0;
				progressBar1.Style = ProgressBarStyle.Continuous;

				string	TargetFolderName = m_TargetFolder.FullName;
				if ( checkBoxCreateDateFolder.Checked )
				{	// Append current date to destination
					DateTime	Now = DateTime.Now;
					string	TimeStamp = Now.Year.ToString( "D04" ) + "-" + Now.Month.ToString( "D02" ) + "-" + Now.Day.ToString( "D02" ) + "_" + Now.Hour.ToString( "D02" ) + "." + Now.Minute.ToString( "D02" );
					TargetFolderName += "\\" + TimeStamp;
				}

				for ( int i=0; i < SOURCE_FOLDERS_COUNT && !bCancel; i++ )
				{
					try
					{
						string	SourceFolder = m_SourceFolders[i].FullName;
						int		SourceFolderNameLength = SourceFolder.LastIndexOf( '\\' );
						if ( SourceFolderNameLength == -1 )
						{	// Means we're copying an entire drive?
							SourceFolderNameLength = SourceFolder.LastIndexOf( ':' );
							if ( SourceFolderNameLength == -1 )
								throw new Exception( "Can't isolate folder name for source folder \"" + SourceFolder + "\"... Can't backup that folder!" );
							SourceFolderNameLength++;
						}

						RecurseVisit( m_SourceFolders[i], ( FileInfo _File ) =>
							{
								// Skip files that posed problem in the first place...
								if ( ErrorFiles.ContainsKey( _File ) )
									return !bCancel;

								try
								{
									// Update progress
									SumCopiedSize += _File.Length;
									TotalCopiedFiles++;
									if ( (TotalCopiedFiles % UpdateProgressFilesCount) == 0 )
									{
										progressBar1.Value = (int) ((progressBar1.Maximum * SumCopiedSize) / TotalSize);
										Application.DoEvents();
									}

									// Attempt copy
									string	DirectoryWithoutRoot = _File.FullName.Remove( 0, SourceFolderNameLength );
									string	TargetFileName = TargetFolderName + DirectoryWithoutRoot;

									Directory.CreateDirectory( Path.GetDirectoryName( TargetFileName ) );

									_File.CopyTo( TargetFileName, true );
								}
								catch ( Exception _e )
								{
									ErrorFiles.Add( _File, _e.Message );
								}

								return !bCancel;
							} );

						if ( bCancel )
							throw new Exception( "Cancelled by user..." );
					}
					catch ( Exception _e )
					{
						ErrorFiles.Add( new FileInfo( m_SourceFolders[i].FullName ), _e.Message );	// can't backup!
					}
				}

				if ( ErrorFiles.Count > 0 )
				{
					if ( MessageBox.Show( this, "Copy is finished with errors for " + ErrorFiles.Count + " files out of " + TotalFiles + " (" + (100*TotalCopiedFiles / TotalFiles) + "% files copied)\r\nDo you want to see the log?", "Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Error ) == DialogResult.Yes )
					{	// Show log...
						LogForm	Log = new LogForm();
								Log.Errors = ErrorFiles;
						Log.ShowDialog( this );
					}
				}
				else
					MessageBox.Show( this, "Success!  " + TotalFiles + " files copied...", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information );

				bStarted = false;
			}
			else
			{	// Cancel
				bCancel = true;
				return;
			}

			// Enable all controls and bring back everything to normal
			buttonStart.Text = "Start Backup";
			panelInfos.Enabled = true;
			progressBar1.Value = 0;
		}
	}
}
