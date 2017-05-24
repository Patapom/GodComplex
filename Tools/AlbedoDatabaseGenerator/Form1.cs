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
		private Database.Entry				m_SelectedEntry = null;

		#endregion

		#region PROPERTIES

		private Database					Database
		{
			get { return m_Database; }
			set
			{
				if ( value == m_Database )
					return;

				m_Database = value;
				SelectedEntry = null;

				// Update UI
				textBoxDatabaseRootPath.BackColor = m_Database != null && m_Database.RootPath.Exists ? BackColor : Color.Red;
				textBoxDatabaseRootPath.Text = m_Database != null ? m_Database.RootPath.FullName : "";
				buttonExportJSON.Enabled = m_Database != null;
				buttonGenerateThumbnails.Enabled = m_Database != null;
			}
		}

		private Database.Entry				SelectedEntry
		{
			get { return m_SelectedEntry; }
			set
			{
				if ( value == m_SelectedEntry )
					return;

				m_SelectedEntry = value;

				// Update UI
				groupBoxDatabaseEntry.Enabled = m_SelectedEntry != null;
				UpdateUIFromEntry( m_SelectedEntry );
			}
		}

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
// 				Database	D = new Database();
// 				string	CurrentDatabaseFileName = GetRegKey( "DatabaseFileName", Path.Combine( m_ApplicationPath, "Database.rdb" ) );
// 				D.Load( new FileInfo( CurrentDatabaseFileName ) );
//				Database = D;
// 			}
// 			catch ( Exception _e )
// 			{
// 				MessageBox( "An error occurred while opening the database:", _e );
// 			}
		}

		protected override void OnClosing( CancelEventArgs e )
		{
			base.OnClosing( e );

			if ( m_Database != null && m_Database.Entries.Length > 0 )
			{
				DialogResult	R = MessageBox( "Do you wish to save the database before closing the application?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question );
				if ( R == DialogResult.Cancel )
				{
					e.Cancel = true;
					return;
				}

				if ( R == DialogResult.Yes )
					buttonSaveDatabase_Click( null, EventArgs.Empty );
			}
		}

		protected void	UpdateDatabaseEntries()
		{
			listBoxDatabaseEntries.BeginUpdate();
			listBoxDatabaseEntries.Items.Clear();
			foreach ( Database.Entry Entry in m_Database.Entries )
				listBoxDatabaseEntries.Items.Add( Entry );
			listBoxDatabaseEntries.EndUpdate();
		}

		private void	UpdateUIFromEntry( Database.Entry _Entry )
		{
			textBoxRelativePath.Text = _Entry != null ? _Entry.RelativePath : "";
			textBoxFriendlyName.Text = _Entry != null ? _Entry.FriendlyName : "";
			textBoxDescription.Text = _Entry != null ? _Entry.Description : "";
			textBoxOverviewImage.Text = _Entry != null && _Entry.OverviewImageFileName != null ? _Entry.OverviewImageFileName.FullName : "";
			panelOverviewImage.SourceImage = _Entry != null ? _Entry.OverviewImage : null;
			panelThumbnail.SourceImage = _Entry != null ? _Entry.Thumbnail : null;

			UpdateTagsUIFromEntry( _Entry );

			Database.Manifest	M = _Entry != null ? _Entry.Manifest : null;
			if ( M != null )
				M.LoadTextures();	// Make sure textures are ready

			panelSwatchMin.BackColor = M != null ? M.m_SwatchMin.Color : BackColor;
			panelSwatchMax.BackColor = M != null ? M.m_SwatchMax.Color : BackColor;
			panelSwatchAvg.BackColor = M != null ? M.m_SwatchAvg.Color : BackColor;

			Panel[]	CustomSwatchPanels = new Panel[] {
				panelCS0,
				panelCS1,
				panelCS2,
				panelCS3,
				panelCS4,
				panelCS5,
				panelCS6,
				panelCS7,
				panelCS8,
			};
			for ( int i=0; i < CustomSwatchPanels.Length; i++ )
			{
				bool	Available = M != null && i < M.m_CustomSwatches.Length;
				CustomSwatchPanels[i].BackColor = Available ? M.m_CustomSwatches[i].Color : BackColor;
				panelTexture.CustomSwatches[i] = Available ? new SharpMath.float4( M.m_CustomSwatches[i].m_LocationTopLeft.x, M.m_CustomSwatches[i].m_LocationTopLeft.y, M.m_CustomSwatches[i].m_LocationBottomRight.x, M.m_CustomSwatches[i].m_LocationBottomRight.y ) : SharpMath.float4.Zero;
			}

			panelTexture.SourceImage = M != null ? M.m_Texture : null;
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
			if ( m_Database != null && m_Database.Entries.Length > 0 )
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
				try
				{
					D.Load( new FileInfo( openFileDialogDatabase.FileName ) );
				}
				catch ( Database.InvalidDatabaseRootPathException _e )
				{
					MessageBox( "The database could not be opened completely as it did not manage to reconnect manifest files on disk based on its embedded location path.\nConsider changing the root folder location to a valid path.\n\nError: " + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning );
				}

				if ( m_Database != null )
					m_Database.Dispose();

				Database = D;

				// Update UI
				textBoxDatabaseFileName.Text = openFileDialogDatabase.FileName;
				UpdateDatabaseEntries();
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the database:\n\n", _e );
			}
		}

		private void buttonSaveDatabase_Click( object sender, EventArgs e )
		{
			string	OldFileName = GetRegKey( "DatabaseFileName", Path.Combine( m_ApplicationPath, "Database.rdb" ) );
			saveFileDialogDatabase.InitialDirectory = Path.GetDirectoryName( OldFileName );
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
				MessageBox( "An error occurred while saving the database:\n\n", _e );
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
				m_Database.RootPath = new DirectoryInfo( folderBrowserDialogDatabaseLocation.SelectedPath );
				textBoxDatabaseRootPath.BackColor = m_Database.RootPath.Exists ? BackColor : Color.Red;
				textBoxDatabaseRootPath.Text = m_Database.RootPath.FullName;
				UpdateDatabaseEntries();
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while changing the database location:\n\n", _e );
			}
		}

		private void listBoxDatabaseEntries_SelectedIndexChanged( object sender, EventArgs e )
		{
			Database.Entry	Selection = listBoxDatabaseEntries.SelectedItem as Database.Entry;
			SelectedEntry = Selection;
		}

		private void textBoxFriendlyName_TextChanged( object sender, EventArgs e )
		{
			m_SelectedEntry.FriendlyName = textBoxFriendlyName.Text;
		}

		private void textBoxDescription_TextChanged( object sender, EventArgs e )
		{
			m_SelectedEntry.Description = textBoxDescription.Text;
		}

		private void textBoxFriendlyName_Validated( object sender, EventArgs e )
		{
			object	Selection = listBoxDatabaseEntries.SelectedItem;

			// Rebuild the list to update the names
			UpdateDatabaseEntries();

			listBoxDatabaseEntries.SelectedItem = Selection;
		}

		private void buttonLoadOverviewImage_Click( object sender, EventArgs e )
		{
//Prefer using current entry's path
// 			string	OldFileName = GetRegKey( "LastOverviewImageFileName", Path.Combine( m_ApplicationPath, "Stuff.jpg" ) );
			string	OldFileName = m_SelectedEntry.FullPath.FullName;

			openFileDialogOverviewImage.InitialDirectory = Path.GetDirectoryName( OldFileName );
			openFileDialogOverviewImage.FileName = Path.GetFileName( OldFileName );
			if ( openFileDialogOverviewImage.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "LastOverviewImageFileName", openFileDialogOverviewImage.FileName );

			try
			{
				string	RelativePath =  Database.GetRelativePath( m_Database.RootPath.FullName, openFileDialogOverviewImage.FileName );
				if ( RelativePath.StartsWith( ".." ) )
					throw new Exception( "The overview image path is not contained under the database root path! Choose an image that is inside the database folder hierarchy." );

				m_SelectedEntry.OverviewImageFileName = new FileInfo( openFileDialogOverviewImage.FileName );
				UpdateUIFromEntry( m_SelectedEntry );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the overview image:\n\n", _e );
			}
		}

		private void buttonExportJSON_Click( object sender, EventArgs e )
		{
// 			string	OldFileName = GetRegKey( "JSONDatabaseFileName", Path.Combine( m_ApplicationPath, "Database.json" ) );
// 			saveFileDialogExportJSON.InitialDirectory = Path.GetFullPath( OldFileName );
// 			saveFileDialogExportJSON.FileName = Path.GetFileName( OldFileName );
// 			if ( saveFileDialogExportJSON.ShowDialog( this ) != DialogResult.OK )
// 				return;
// 
// 			SetRegKey( "JSONDatabaseFileName", saveFileDialogExportJSON.FileName );

			try
			{
//				m_Database.Export( new FileInfo( saveFileDialogExportJSON.FileName ) );

				FileInfo	Target = new FileInfo( Path.Combine( m_Database.RootPath.FullName, "database.json" ) );
				m_Database.Export( Target );

				MessageBox( "Success!\r\nJSON file was successfully exported to \"" + Target.FullName + "\"!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while exporting the database:\n\n", _e );
			}
		}

		private void buttonGenerateThumbnails_Click( object sender, EventArgs e )
		{
			DialogResult	R = DialogResult.None;
			string	Errors = "";
			int		ThumbnailsCount = 0;
			foreach ( Database.Entry E in m_Database.Entries )
				if ( E.Manifest != null )
					try
					{
						if ( R == DialogResult.None && E.Thumbnail != null )
						{
							R = MessageBox( "Do you want to force re-generating existing thumbnails?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question );
							if ( R == DialogResult.Cancel )
								return;
						}
						E.GenerateThumbnail( R == DialogResult.Yes );
						ThumbnailsCount++;
					}
					catch ( Exception _e )
					{
						Errors += "Failed to generate thumbnail for \"" + E.Manifest.m_CalibratedTextureFileName + "\": " + _e.Message + "\n";
					}
			
			// Update UI to refresh thumbnail preview
			UpdateUIFromEntry( m_SelectedEntry );

			if ( Errors == "" )
				MessageBox( "Success!\nGenerated " + ThumbnailsCount + " thumbnails...", MessageBoxButtons.OK, MessageBoxIcon.Information );
			else
				MessageBox( "Warning!\nGenerated " + ThumbnailsCount + " thumbnails with errors:\n\n" + Errors, MessageBoxButtons.OK, MessageBoxIcon.Warning );
		}

		#region Tags

		private bool	m_ModifyingCheckboxes = false;
		private int	MutuallyExclusiveChoice( object _Sender, CheckBox[] _Choices )
		{
			m_ModifyingCheckboxes = true;

			CheckBox	C = _Sender as CheckBox;
			if ( !C.Checked )
			{	// User unchecked the only possible choice so we know that this tag is empty
				m_ModifyingCheckboxes = false;
				return 0;
			}

			// Uncheck all others and keep the only one that's checked
			int		SelectedChoice = 0;
			for ( int ChoiceIndex=0; ChoiceIndex < _Choices.Length; ChoiceIndex++ )
				if ( _Choices[ChoiceIndex] == C )
					SelectedChoice = ChoiceIndex;
				else
					_Choices[ChoiceIndex].Checked = false;

			m_ModifyingCheckboxes = false;
			return 1+SelectedChoice;	// 1+ because choice 0 is NONE
		}
		private int	MutuallyExclusiveChoiceWithMaster( object _Sender, CheckBox[] _Choices, int _MasterChoice )
		{
			m_ModifyingCheckboxes = true;

			CheckBox	C = _Sender as CheckBox;
			if ( C == _Choices[0] )
			{	// Master choice
				if ( !C.Checked )
				{	// Unchecking the master just clears all other choices
					for ( int ChoiceIndex=1; ChoiceIndex < _Choices.Length; ChoiceIndex++ )
						_Choices[ChoiceIndex].Checked = false;

					m_ModifyingCheckboxes = false;
					return 0;
				}
				m_ModifyingCheckboxes = false;
				return _MasterChoice;
			}

			// Always check the master
			_Choices[0].Checked = true;

			if ( !C.Checked )
			{	// Unchecking a non master tick means we're left with the master choice anyway
				m_ModifyingCheckboxes = false;
				return _MasterChoice;
			}

			// Uncheck all others and keep the only one that's checked
			int		SelectedChoice = 0;
			for ( int ChoiceIndex=1; ChoiceIndex < _Choices.Length; ChoiceIndex++ )
				if ( _Choices[ChoiceIndex] == C )
					SelectedChoice = ChoiceIndex;
				else
					_Choices[ChoiceIndex].Checked = false;

			m_ModifyingCheckboxes = false;
			return SelectedChoice | _MasterChoice;
		}
		private int ORedFlags( object _Sender, CheckBox[] _Choices )
		{
			CheckBox	C = _Sender as CheckBox;
			int		Flags = 0;
			for ( int i=0; i < _Choices.Length; i++ )
				if ( _Choices[i].Checked )
					Flags |= 1 << i;

			return Flags;
		}

			CheckBox[]	c0 { get { return new CheckBox[] {
		checkBoxTagWood,			 // WOOD,
		checkBoxTagStone,			 // STONE,
		checkBoxTagSkin,			 // SKIN
		checkBoxTagFabric,			 // FABRIC,
		checkBoxTagPaperCanvas,		 // PAPER_CANVAS,
		checkBoxTagPaint,			 // PAINT,
		checkBoxTagPlastic,			 // PLASTIC,
		checkBoxTagMetal,			 // METAL,
			}; } }
			CheckBox[]	c1 { get { return new CheckBox[] {
		checkBoxTagBlack,			 // BLACK,
		checkBoxTagWhite,			 // WHITE,
		checkBoxTagGray,			 // GRAY,
		checkBoxTagRed,				 // RED,
		checkBoxTagGreen,			 // GREEN,
		checkBoxTagBlue,			 // BLUE,
		checkBoxYellow,				 // YELLOW,
		checkBoxTagCyan,			 // CYAN,
		checkBoxTagPurple,			 // PURPLE,
		checkBoxTagOrange,			 // ORANGE,
			}; } }
			CheckBox[]	c2 { get { return new CheckBox[] {
		checkBoxTagDark,			 // DARK,
		checkBoxTagBright,			 // BRIGHT,
		checkBoxTagNeutral,			 // NEUTRAL,
			}; } }
			CheckBox[]	c3 { get { return new CheckBox[] {
		checkBoxTagNature,			 // NATURE,
		checkBoxTagLeaf,			 // LEAF,
		checkBoxTagSoil,			 // SOIL,
		checkBoxTagBark,			 // BARK,
			}; } }
			CheckBox[]	c4 { get { return new CheckBox[] {
		checkBoxTagFurniture,		 // FURNITURE,
		checkBoxTagTable,			 // TABLE,
		checkBoxTagChair,			 // CHAIR,
		checkBoxTagDesk,			 // DESK,
		checkBoxTagWardrobe,		 // WARDROBE,
		checkBoxTagCabinet,			 // CABINET,
			}; } }
			CheckBox[]	c5 { get { return new CheckBox[] {
		checkBoxTagConstruction,	 // CONSTRUCTION,
		checkBoxTagWall,			 // WALL,
		checkBoxTagFloor,			 // FLOOR,
		checkBoxTagDoorWindow,		 // DOOR_WINDOW,
		checkBoxTagRoadPavement,	 // ROAD_PAVEMENT,
			}; } }
			CheckBox[]	c6 { get { return new CheckBox[] {
		checkBoxTagWet,				 // WET = 1,
		checkBoxTagDusty,			 // DUSTY = 2,
		checkBoxTagFrosty,			 // FROSTY = 4,
		checkBoxTagRusty,			 // RUSTY = 8,
		checkBoxTagVarnished,		 // VARNISHED = 16,
		checkBoxTagOld,				 // OLD = 32,
		checkBoxTagNew,				 // NEW = 64,
			}; } }

		private void checkBoxTagType_CheckedChanged( object sender, EventArgs e )
		{
			if ( m_ModifyingCheckboxes )
				return;
			m_SelectedEntry.TagType = (Database.Entry.TAGS_TYPE) MutuallyExclusiveChoice( sender, c0 );
		}

		private void checkBoxTagColor_CheckedChanged( object sender, EventArgs e )
		{
			if ( m_ModifyingCheckboxes )
				return;
//			m_SelectedEntry.TagColor = (Database.Entry.TAGS_COLOR) MutuallyExclusiveChoice( sender, c1 );
			m_SelectedEntry.TagColor = (Database.Entry.TAGS_COLOR) ORedFlags( sender, c1 );
		}

		private void checkBoxTagShade_CheckedChanged( object sender, EventArgs e )
		{
			if ( m_ModifyingCheckboxes )
				return;
			m_SelectedEntry.TagShade = (Database.Entry.TAGS_SHADE) MutuallyExclusiveChoice( sender, c2 );
		}

		private void checkBoxTagNature_CheckedChanged( object sender, EventArgs e )
		{
			if ( m_ModifyingCheckboxes )
				return;
			m_SelectedEntry.TagNature = (Database.Entry.TAGS_NATURE) MutuallyExclusiveChoiceWithMaster( sender, c3, (int) Database.Entry.TAGS_NATURE.NATURE );
		}

		private void checkBoxTagFurniture_CheckedChanged( object sender, EventArgs e )
		{
			if ( m_ModifyingCheckboxes )
				return;
			m_SelectedEntry.TagFurniture = (Database.Entry.TAGS_FURNITURE) MutuallyExclusiveChoiceWithMaster( sender, c4, (int) Database.Entry.TAGS_FURNITURE.FURNITURE );
		}

		private void checkBoxTagConstruction_CheckedChanged( object sender, EventArgs e )
		{
			if ( m_ModifyingCheckboxes )
				return;
			m_SelectedEntry.TagConstruction = (Database.Entry.TAGS_CONSTRUCTION) MutuallyExclusiveChoiceWithMaster( sender, c5, (int) Database.Entry.TAGS_CONSTRUCTION.CONSTRUCTION );
		}
		private void checkBoxTagModifiers_CheckedChanged( object sender, EventArgs e )
		{
			if ( m_ModifyingCheckboxes )
				return;
			m_SelectedEntry.TagModifiers = (Database.Entry.TAGS_MODIFIERS) ORedFlags( sender, c6 );
		}

		private void	SetMutuallyExclusiveChoice( int _Value, CheckBox[] _Choices )
		{
			_Value--;
			for ( int i=0; i < _Choices.Length; i++ )
				_Choices[i].Checked = i == _Value;
		}
		private void	SetMutuallyExclusiveChoiceWithMaster( int _Value, int _MasterValue, CheckBox[] _Choices )
		{
			bool	IsMasterChecked = (_Value & _MasterValue) != 0;
			_Choices[0].Checked = IsMasterChecked;

			_Value = _Value & ~_MasterValue;
			for ( int i=1; i < _Choices.Length; i++ )
				_Choices[i].Checked = i == _Value;
		}
		private void	SetFlagChoice( int _Value, CheckBox[] _Choices )
		{
			for ( int i=0; i < _Choices.Length; i++ )
				_Choices[i].Checked = (_Value & (1 << i)) != 0;
		}
		private void	UpdateTagsUIFromEntry( Database.Entry _Entry )
		{
			m_ModifyingCheckboxes = true;

			SetMutuallyExclusiveChoice( _Entry != null ? (int) _Entry.TagType : 0, c0 );
//			SetMutuallyExclusiveChoice( _Entry != null ? (int) _Entry.TagColor : 0, c1 );
			SetFlagChoice( _Entry != null ? (int) _Entry.TagColor : 0, c1 );
			SetMutuallyExclusiveChoice( _Entry != null ? (int) _Entry.TagShade : 0, c2 );
			SetMutuallyExclusiveChoiceWithMaster( _Entry != null ? (int) _Entry.TagNature : 0, (int) Database.Entry.TAGS_NATURE.NATURE, c3 );
			SetMutuallyExclusiveChoiceWithMaster( _Entry != null ? (int) _Entry.TagFurniture : 0, (int) Database.Entry.TAGS_FURNITURE.FURNITURE, c4 );
			SetMutuallyExclusiveChoiceWithMaster( _Entry != null ? (int) _Entry.TagConstruction : 0, (int) Database.Entry.TAGS_CONSTRUCTION.CONSTRUCTION, c5 );
			SetFlagChoice( _Entry != null ? (int) _Entry.TagModifiers : 0, c6 );

			m_ModifyingCheckboxes = false;
		}

		#endregion

		#endregion
	}
}
