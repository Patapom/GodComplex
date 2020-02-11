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
		public override Keys	ShortcutKey => Keys.None;

		public Fiche[]	Fiches {
//			get { return this.richTextBoxTags}
			set { }
		}

		public FastTaggerForm( BrainForm _owner ) : base( _owner ) {
			InitializeComponent();

			this.richTextBoxTags.ApplicationForm = _owner;
			this.richTextBoxTags.OwnerForm = this;
		}

		protected override void InternalDispose() {
		}

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			e.Graphics.DrawRectangle( Pens.Black, 0, 0, Width-1, Height-1 );
		}
	}
}
