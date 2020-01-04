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
	public partial class FastTaggerForm : ModelessForm {
//	public partial class FastTaggerForm : Form {

		protected override bool Sizeable => true;
		protected override bool CloseOnEscape => true;
		public override Keys	SHORTCUT_KEY => Keys.None;

		public FastTaggerForm( BrainForm _owner, Fiche[] _fiches ) : base( _owner ) {
			InitializeComponent();

			this.richTextBoxTags.ApplicationForm = _owner;
			this.richTextBoxTags.OwnerForm = this;
		}

		protected override void InternalDispose() {
		} 
	}
}
