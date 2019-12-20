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
	public partial class FastTaggerForm : Form {

		Fiche	m_fiche;

		public FastTaggerForm( Fiche _fiche ) {
			InitializeComponent();
			m_fiche = _fiche;


		}

		private void richTextBoxTags_TextChanged(object sender, EventArgs e) {

			// Isolate 
			FicheDB.FindNearestTagMatches()

			richTextBoxTags.SelectionStart
		}
	}
}
