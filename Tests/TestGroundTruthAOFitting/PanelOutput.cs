using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace TestGroundTruthAOFitting
{
	public partial class PanelOutput : Panel {
		public PanelOutput() {
			InitializeComponent();
		}

		public PanelOutput( IContainer container ) {
			container.Add( this );
			InitializeComponent();
		}

		protected override void OnPaintBackground( PaintEventArgs e ) {
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e ) {
			base.OnPaint( e );
		}
	}
}
