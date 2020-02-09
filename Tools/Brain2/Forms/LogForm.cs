using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brain2 {
	public partial class LogForm : Form {
		public LogForm() {
			InitializeComponent();
		}

		public void	Log( string _text ) { logTextBox.Log( _text + "\r\n" ); }
		public void	LogSuccess( string _text ) { logTextBox.LogSuccess( _text + "\r\n" ); }
		public void	LogWarning( string _text ) { logTextBox.LogWarning( _text + "\r\n" ); }
		public void	LogError( string _text ) { logTextBox.LogError( _text + "\r\n" ); }
		public void	LogDebug( string _text ) { logTextBox.LogSuccess( _text + "\r\n" ); }

		protected override void OnKeyDown(KeyEventArgs e) {
			if ( e.KeyCode == Keys.Escape ) {
				Hide();
			}

			base.OnKeyDown(e);
		}

		protected override void OnFormClosing(FormClosingEventArgs e) {
			if ( e.CloseReason == CloseReason.UserClosing ) {
				e.Cancel = true;
				Hide();
			}

			base.OnFormClosing(e);
		}
	}
}
