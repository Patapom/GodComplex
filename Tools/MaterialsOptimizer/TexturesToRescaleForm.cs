using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MaterialsOptimizer
{
	public partial class TexturesToRescaleForm : Form
	{
		List< TextureFileInfo >			m_glossTexturesToRescale = null;
		public List< TextureFileInfo >	glossTexturesToRescale {
			get { return m_glossTexturesToRescale; }
			set {
				m_glossTexturesToRescale = value;
				if ( m_glossTexturesToRescale == null )
					return;

				List< string >	fileNames = new List< string >();
				foreach ( TextureFileInfo TFI in m_glossTexturesToRescale ) {
					fileNames.Add( TFI.m_fileName.FullName );
				}

				textBoxFileNames.Lines = fileNames.ToArray();
			}
		}

		public TexturesToRescaleForm()
		{
			InitializeComponent();
		}

		private void buttonCopyToClipboard_Click(object sender, EventArgs e) {
			Clipboard.SetText( textBoxFileNames.Text );
		}
	}
}
