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
using System.Xml;

namespace AlbedoDatabaseGenerator
{
	public partial class Form1 : Form
	{
		#region NESTED TYPES

		#endregion

		#region FIELDS

		private RegistryKey					m_AppKey;
		private string						m_ApplicationPath;

		private Database					m_Database = new Database();

		#endregion

		#region PROPERTIES
		#endregion

		#region METHODS

		public Form1()
		{
			InitializeComponent();

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\AlbedoDatabaseGenerator" );
			m_ApplicationPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

// 			try
// 			{
// 				m_Database = new Database();
// 				string	CurrentDatabaseFileName = GetRegKey( "DatabaseFileName", Path.Combine( m_ApplicationPath, "Database.rdb" ) );
// 				m_Database.Load( new FileInfo( CurrentDatabaseFileName ) );
// 			}
// 			catch ( Exception _e )
// 			{
// 				MessageBox( "An error occurred while opening the database:", _e );
// 			}
		}

		#region Helpers

		private string	GetRegKey( string _Key, string _Default )
		{
			string	Result = m_AppKey.GetValue( _Key ) as string;
			return Result != null ? Result : _Default;
		}
		private void	SetRegKey( string _Key, string _Value )
		{
			m_AppKey.SetValue( _Key, _Value );
		}

		private float	GetRegKeyFloat( string _Key, float _Default )
		{
			string	Value = GetRegKey( _Key, _Default.ToString() );
			float	Result;
			float.TryParse( Value, out Result );
			return Result;
		}

		private int		GetRegKeyInt( string _Key, float _Default )
		{
			string	Value = GetRegKey( _Key, _Default.ToString() );
			int		Result;
			int.TryParse( Value, out Result );
			return Result;
		}

		private DialogResult	MessageBox( string _Text )
		{
			return MessageBox( _Text, MessageBoxButtons.OK );
		}
		private DialogResult	MessageBox( string _Text, Exception _e )
		{
			return MessageBox( _Text + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons )
		{
			return MessageBox( _Text, _Buttons, MessageBoxIcon.Information );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxIcon _Icon )
		{
			return MessageBox( _Text, MessageBoxButtons.OK, _Icon );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon )
		{
			return System.Windows.Forms.MessageBox.Show( this, _Text, "Texture Reflectances Generator", _Buttons, _Icon );
		}

		#endregion

		#endregion

		#region EVENT HANDLERS

		private void buttonLoadDatabase_Click( object sender, EventArgs e )
		{
			if ( m_Database != null )
			{	// Caution!
				if ( MessageBox( "Loading a new database will lose existing database data, do you wish to continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning ) != DialogResult.Yes )
					return;
			}

			string	OldFileName = GetRegKey( "DatabaseFileName", Path.Combine( m_ApplicationPath, "Database.rdb" ) );
			openFileDialogDatabase.InitialDirectory = Path.GetFullPath( OldFileName );
			openFileDialogDatabase.FileName = Path.GetFileName( OldFileName );
			if ( openFileDialogDatabase.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "DatabaseFileName", openFileDialogDatabase.FileName );

			try
			{
				Database	D = new Database();
				D.Load( new FileInfo( openFileDialogDatabase.FileName ) );
				m_Database = D;
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the database:", _e );
			}
		}

		private void buttonSaveDatabase_Click( object sender, EventArgs e )
		{
			string	OldFileName = GetRegKey( "DatabaseFileName", Path.Combine( m_ApplicationPath, "Database.rdb" ) );
			saveFileDialogDatabase.InitialDirectory = Path.GetFullPath( OldFileName );
			saveFileDialogDatabase.FileName = Path.GetFileName( OldFileName );
			if ( saveFileDialogDatabase.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "DatabaseFileName", saveFileDialogDatabase.FileName );

			try
			{
				m_Database.Save( new FileInfo( saveFileDialogDatabase.FileName ) );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while saving the database:", _e );
			}
		}

		private void buttonChangeDatabaseLocation_Click( object sender, EventArgs e )
		{
			string	OldDatabasePath = GetRegKey( "DatabasePath", m_ApplicationPath );
			folderBrowserDialogDatabaseLocation.SelectedPath = OldDatabasePath;
			if ( folderBrowserDialogDatabaseLocation.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "DatabasePath", folderBrowserDialogDatabaseLocation.SelectedPath );

			try
			{
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while changing the database location:", _e );
			}
		}

		private void buttonExportJSON_Click( object sender, EventArgs e )
		{

		}

		private void buttonGenerateThumbnails_Click( object sender, EventArgs e )
		{

		}

		private void buttonLoadEnvImage_Click( object sender, EventArgs e )
		{

		}

		#endregion
	}
}
