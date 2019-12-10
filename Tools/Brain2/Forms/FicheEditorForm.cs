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

	public partial class FicheEditorForm : Form {

		#region CONSTANTS

		public const Keys	SHORTCUT_KEY = Keys.F5;

		#endregion

		#region FIELDS

		private BrainForm					m_owner;

		#endregion

		#region PROPERTIES

		#endregion

		#region METHODS

		public FicheEditorForm( BrainForm _owner ) {
			m_owner = _owner;

			InitializeComponent();
		}

		protected override void OnKeyDown(KeyEventArgs e) {

			switch ( e.KeyCode ) {
				case Keys.Escape:
				case SHORTCUT_KEY:
					Hide();
					break;

			}

			base.OnKeyDown(e);
		}

		#endregion

		#region EVENTS

		#endregion
	}
}
