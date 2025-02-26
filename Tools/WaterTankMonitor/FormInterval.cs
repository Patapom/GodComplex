using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WaterTankMonitor {
	public partial class FormInterval : Form {

		public string	Label {
			get {
				string	label = textBoxLabel.Text.Trim();
						label = label.Replace( "\\n", "\n" );	// Expand '\n'
				return label;
			}
			set => textBoxLabel.Text = value;
		}

		public FormInterval() {
			InitializeComponent();
		}
	}
}
