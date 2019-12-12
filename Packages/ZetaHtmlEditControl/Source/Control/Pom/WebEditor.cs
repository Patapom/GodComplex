using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace ZetaHtmlEditControl.Pom {
	public partial class WebEditor : UserControl {

		#region CONSTANTS

		static readonly int[]	FONT_SIZES = new int[] {
			12,	// Normal
			20,	// H1
			18,	// H2
			16,	// H3
			14,	// H4
		};

		#endregion

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

		public DirectoryInfo		TempImagesFolder {
			get { return m_tempImagesFolder; }
			set {
				if ( value == null || value == m_tempImagesFolder )
					return;

				m_tempImagesFolder = value;

				// Reset document and path
				m_cachedDocument = htmlEditControl.GetDocumentText( m_tempImagesFolder.FullName );
				htmlEditControl.SetDocumentText( m_cachedDocument, m_tempImagesFolder.FullName, true );
			}
		}

		public string				Document {
			get { 
				m_cachedDocument = htmlEditControl.GetDocumentText( m_tempImagesFolder.FullName );
				return m_cachedDocument;
			}
			set {
				if ( value == null || value == m_cachedDocument )
					return;

				m_cachedDocument = value;
				htmlEditControl.SetDocumentText( m_cachedDocument, m_tempImagesFolder.FullName, true );
			}
		}

		#endregion

		#region METHODS

		public WebEditor() {
 			m_tempImagesFolder = new DirectoryInfo( Directory.GetCurrentDirectory() );

			InitializeComponent();

			htmlEditControl.DocumentTitleChanged += HtmlEditControl_DocumentTitleChanged;
			htmlEditControl.DocumentCompleted += HtmlEditControl_DocumentCompleted;
			htmlEditControl.TextChanged += HtmlEditControl_TextChanged;
			toolStripComboBoxStyle.SelectedIndex = 0;
		}

		private void HtmlEditControl_TextChanged(object sender, EventArgs e) {
			throw new NotImplementedException();
		}

		private void HtmlEditControl_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
//			throw new NotImplementedException();
		}

		private void HtmlEditControl_DocumentTitleChanged(object sender, EventArgs e) {
//			throw new NotImplementedException();
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

//			htmlEditControl.Configure()

//			Document = @"<P>Click the buttons below to set different texts. German Umlaute: Ä Ö Ü ä ö ü ß.</p>";
		}

		private void htmlEditControl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
			OnPreviewKeyDown( e );
// 			if ( e.KeyCode == Keys.Escape || e.KeyCode == Keys.F5 ) {
// 				Hide();
// 			}
		}

		protected override void OnEnabledChanged(EventArgs e) {
			toolStrip.Enabled = Enabled;
//			htmlEditControl.Enabled = Enabled;	// Not supported...

			base.OnEnabledChanged(e);
		}

		private void htmlEditControl_UINeedsUpdate(object sender, EventArgs e) {
			// Forward selection state to toolstrip buttons
			toolStripButtonBold.Checked = htmlEditControl.IsBold;
			toolStripButtonItalic.Checked = htmlEditControl.IsItalic;
//			toolStripButtonLink.Checked = htmlEditControl.IsLi;
			toolStripButtonBulletList.Checked = htmlEditControl.IsBullettedList;
			toolStripButtonNumberedList.Checked = htmlEditControl.IsOrderedList;

			// Get header style
// 			mshtml.IHTMLElement		selectedElement = htmlEditControl.CurrentSelectedElement;
// 			if ( selectedElement == null  )
// 				return;
// 			selectedElement.style.
			
			switch ( htmlEditControl.FontSize ) {
				case 12: toolStripComboBoxStyle.SelectedIndex = 0; break;
				case 20: toolStripComboBoxStyle.SelectedIndex = 1; break;
				case 18: toolStripComboBoxStyle.SelectedIndex = 2; break;
				case 16: toolStripComboBoxStyle.SelectedIndex = 3; break;
				case 14: toolStripComboBoxStyle.SelectedIndex = 4; break;
			}
		}

		private void toolStripButtonBold_Click(object sender, EventArgs e) {
            htmlEditControl.ExecuteBold();
		}

		private void toolStripButtonItalic_Click(object sender, EventArgs e) {
            htmlEditControl.ExecuteItalic();
		}

		private void toolStripButtonUnderline_Click(object sender, EventArgs e) {
            htmlEditControl.ExecuteUnderline();
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

		private void toolStripComboBoxStyle_SelectedIndexChanged(object sender, EventArgs e) {
			if ( toolStripComboBoxStyle.SelectedIndex < FONT_SIZES.Length )
				htmlEditControl.ExecuteFontSize( FONT_SIZES[toolStripComboBoxStyle.SelectedIndex].ToString() );
		}

		#endregion
	}
}
