using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Brain2.Web_Interface {
	public partial class WebEditor : UserControl {

		#region NESTED TYPES

// 		class Configuration : ZetaHtmlEditControl.Code.Configuration.HtmlEditControlConfiguration {
// 
// 		}

		#endregion

		#region FIELDS

		private DirectoryInfo		m_tempImagesFolder;
		private string				m_cachedDocument = null;

		#endregion

		#region PROPERTIES

// 		public DirectoryInfo		TempImagesFolder {
// 			get { return m_tempImagesFolder; }
// 			set {
// 				if ( value == null || value == m_tempImagesFolder )
// 					return;
// 
// 				m_tempImagesFolder = value;
// 
// 				// Reset document and path
// 				m_cachedDocument = htmlEditControl.GetDocumentText( m_tempImagesFolder.FullName );
// 				htmlEditControl.SetDocumentText( m_cachedDocument, m_tempImagesFolder.FullName, true );
// 			}
// 		}
// 
// 		public string				Document {
// 			get { 
// 				m_cachedDocument = htmlEditControl.GetDocumentText( m_tempImagesFolder.FullName );
// 				return m_cachedDocument;
// 			}
// 			set {
// 				if ( value == null || value == m_cachedDocument )
// 					return;
// 
// 				m_cachedDocument = value;
// 				htmlEditControl.SetDocumentText( m_cachedDocument, m_tempImagesFolder.FullName, true );
// 			}
// 		}

		#endregion

		#region METHODS

		public WebEditor() {
			InitializeComponent();

// 			if ( DesignT)
// 			m_tempImagesFolder = new DirectoryInfo( Directory.GetCurrentDirectory() );
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

//			htmlEditControl.Configure()

//			Document = @"<P>Click the buttons below to set different texts. German Umlaute: Ä Ö Ü ä ö ü ß.</p>";
		}

		private void toolStripButtonItalic_Click(object sender, EventArgs e) {
            htmlEditControl.ExecuteItalic();
		}

		private void toolStripButtonBold_Click(object sender, EventArgs e) {
            htmlEditControl.ExecuteBold();
		}

		private void toolStripButtonLink_Click(object sender, EventArgs e) {
            htmlEditControl.ExecuteInsertHyperlink();
		}

		private void toolStripButtonImage_Click(object sender, EventArgs e) {
		}

		private void toolStripButtonBulletList_Click(object sender, EventArgs e) {
            htmlEditControl.ExecuteBullettedList();
		}

		private void toolStripButtonNumberedList_Click(object sender, EventArgs e) {
            htmlEditControl.ExecuteNumberedList();
		}

		#endregion
	}
}
