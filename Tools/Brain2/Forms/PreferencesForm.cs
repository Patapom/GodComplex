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

	public partial class PreferencesForm : Form {

		#region CONSTANTS

		public const Keys	SHORTCUT_KEY = Keys.F10;

		#endregion

		#region FIELDS

		private BrainForm					m_owner;
		private Microsoft.Win32.RegistryKey	m_appKey;

		private Size		m_relativeLocation = new Size( -1, -1 );

		#endregion

		#region PROPERTIES

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

		public PreferencesForm( BrainForm _owner ) {
			m_owner = _owner;
			m_appKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\Brain2" );

			InitializeComponent();

			// Fetch default values from registry
//			string	defaultDBFolder = Path.Combine( Path.GetDirectoryName( Application.ExecutablePath ), "BrainFiches" );
			string	defaultDBFolder = Path.Combine( Directory.GetCurrentDirectory(), "BrainFiches" );
			textBoxDatabaseRoot.Text = GetRegKey( "RootDBFolder", defaultDBFolder );
		}

		public new void	Show() {
			base.Show();
			Capture = true;

			if ( m_relativeLocation.Width < 0 ) {
				// Initial condition => Reset to center
				m_relativeLocation.Width = (m_owner.Width - Width) / 2;
				m_relativeLocation.Height = (m_owner.Height - Height) / 2;
			}

			Point	newLocation = m_owner.Location + m_relativeLocation;
			newLocation.X = Math.Max( 0, Math.Min( m_owner.Width - Width, newLocation.X ) );
			newLocation.Y = Math.Max( 0, Math.Min( m_owner.Height - Width, newLocation.Y ) );
			this.Location = newLocation;
		}

		public new void Hide() {
			Capture = false;
			base.Hide();
		}

		// 		protected override bool ProcessKeyPreview(ref Message m) {
		// 
		// 			switch ( m. )
		// 
		// 			return base.ProcessKeyPreview(ref m);
		// 		}

		protected override void OnKeyDown(KeyEventArgs e) {

			switch ( e.KeyCode ) {
				case Keys.Escape:
				case SHORTCUT_KEY:
					Hide();
					break;

			}

			base.OnKeyDown(e);
		}

		#region Registry

		private string	GetRegKey( string _key, string _default ) {
			string	result = m_appKey.GetValue( _key ) as string;
			return result != null ? result : _default;
		}
		private void	SetRegKey( string _key, string _Value ) {
			m_appKey.SetValue( _key, _Value );
		}

		private float	GetRegKeyFloat( string _key, float _default ) {
			string	value = GetRegKey( _key, _default.ToString() );
			float	result;
			float.TryParse( value, out result );
			return result;
		}

		private int		GetRegKeyInt( string _key, float _default ) {
			string	value = GetRegKey( _key, _default.ToString() );
			int		result;
			int.TryParse( value, out result );
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

		#endregion
	}
}
