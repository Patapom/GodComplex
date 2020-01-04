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

	public partial class FicheWebPageAnnotatorForm : Brain2.ModelessForm {

		#region CONSTANTS

		#endregion

		#region FIELDS

		private Fiche		m_fiche = null;

		#endregion

		#region PROPERTIES

		protected override bool Sizeable => true;
		protected override bool CloseOnEscape => false;
		public override Keys SHORTCUT_KEY => Keys.F6;

		public Fiche		EditedFiche {
			get { return m_fiche; }
			set {
				if ( value == m_fiche )
					return;

				if ( m_fiche != null ) {
					m_fiche.WebPageImageChanged -= fiche_WebPageImageChanged;
				}

				m_fiche = value;

				if ( m_fiche != null ) {
					m_fiche.WebPageImageChanged += fiche_WebPageImageChanged;
				}

				// Update UI
				bool	enable = m_fiche != null;
				richTextBoxTitle.Enabled = enable;
				richTextBoxTitle.Text = enable ? m_fiche.Title : "";

				richTextBoxURL.Enabled = enable;
				richTextBoxURL.Text = enable && m_fiche.URL != null ? m_fiche.URL.ToString() : "";

				tagEditBox.Enabled = enable;
				tagEditBox.RecognizedTags = enable ? m_fiche.Tags : null;

 				if ( enable && m_fiche.WebPageImage != null ) {
					panelWebPage.BackgroundImage = m_fiche.WebPageImage.AsBitmap;
 				}
 				panelWebPage.Enabled = enable;
			}
		}

		private void fiche_WebPageImageChanged( Fiche _sender ) {
			panelWebPage.BackgroundImage = _sender.WebPageImage.AsBitmap;
		}

		#endregion

		#region METHODS

		public FicheWebPageAnnotatorForm( BrainForm _owner ) : base( _owner ) {
			InitializeComponent();

			tagEditBox.ApplicationForm = m_owner;
			tagEditBox.OwnerForm = this;
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

		#endregion
	}
}
