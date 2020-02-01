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

	public partial class OldFicheWebPageEditorForm : Brain2.ModelessForm {

		#region CONSTANTS

		#endregion

		#region FIELDS

		private Fiche		m_fiche = null;

		#endregion

		#region PROPERTIES

		protected override bool Sizeable => true;
		protected override bool CloseOnEscape => false;
		public override Keys SHORTCUT_KEY => Keys.F5;

		public Fiche		EditedFiche {
			get { return m_fiche; }
			set {
				if ( value == m_fiche )
					return;

				m_fiche = value;

				// Update UI
				bool	enable = m_fiche != null;
				richTextBoxTitle.Enabled = enable;
				richTextBoxTitle.Text = enable ? m_fiche.Title : "";

				richTextBoxURL.Enabled = enable;
				richTextBoxURL.Text = enable && m_fiche.URL != null ? m_fiche.URL.ToString() : "";

				tagEditBox.Enabled = enable;
				tagEditBox.RecognizedTags = enable ? m_fiche.Tags : null;

				if ( enable ) {
					if ( m_fiche.HTMLContent != null ) {
						// Use cached HTML document by default
						webEditor.Document = m_fiche.HTMLContent;
					} else if ( m_fiche.URL != null ) {
						// Use URL instead
						webEditor.URL = m_fiche.URL;
					} else {
						// Setup empty document
						webEditor.Document = WebHelpers.BuildHTMLDocument( "Invalid Fiche Content", "Fiche has no content and no source URL!" );
					}
				} else {
					webEditor.Document = WebHelpers.BuildHTMLDocument( "", "<body/>" );
				}
				webEditor.Enabled = enable;
			}
		}

		#endregion

		#region METHODS

		public OldFicheWebPageEditorForm( BrainForm _owner ) : base( _owner ) {
			InitializeComponent();

			tagEditBox.ApplicationForm = m_owner;
			tagEditBox.OwnerForm = this;

			webEditor.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.webEditor_PreviewKeyDown);
			webEditor.DocumentUpdated += WebEditor_DocumentUpdated;
		}

		private void webEditor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
			if ( e.KeyCode == Keys.Escape || e.KeyCode == SHORTCUT_KEY ) {
				Hide();
			}
		}

		protected override void InternalDispose() {
			throw new Exception( "TODO!" );
		}

		#endregion

		#region EVENTS

		private void richTextBoxURL_LinkClicked(object sender, LinkClickedEventArgs e) {
			if ( m_fiche == null || m_fiche.URL == null )
				return;

			try {
				System.Diagnostics.Process.Start( m_fiche.URL.AbsoluteUri );
			} catch ( Exception _e ) {
				BrainForm.MessageBox( "Failed to open URL \"" + m_fiche.URL.AbsoluteUri + "\": ", _e );
			}
		}

		private void WebEditor_DocumentUpdated(object sender, EventArgs e) {
			if ( m_fiche != null )
				m_fiche.HTMLContent = webEditor.Document;	// Update fiche
		}

		#endregion
	}
}
