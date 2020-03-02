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

	public partial class FicheWebPageAnnotatorForm : Brain2.ModelessForm, IFicheEditor {

		#region CONSTANTS

		#endregion

		#region FIELDS

		private Fiche		m_fiche = null;

		private Dictionary< Fiche.ChunkWebPageSnapshot.WebPageImagePart, PanelMultiBitmaps.BitmapWithRectangle >	m_webPageImagePart2Bitmap = new Dictionary<Fiche.ChunkWebPageSnapshot.WebPageImagePart, PanelMultiBitmaps.BitmapWithRectangle>();

		#endregion

		#region PROPERTIES

		protected override bool Sizeable => true;
		protected override bool CloseOnEscape => false;
		public override Keys ShortcutKey => Keys.F5;

		public Form			EditorForm { get {return this; } }
		public Fiche		EditedFiche {
			get { return m_fiche; }
			set {
				if ( value == m_fiche )
					return;

				if ( m_fiche != null ) {
					m_fiche.TitleChanged -= fiche_TitleChanged;
					m_fiche.WebPageImageChanged -= fiche_WebPageImageChanged;
					m_fiche.DOMElementsChanged -= fiche_DOMElementsChanged;
					DisposeFicheBitmaps();
				}

				m_fiche = value;

				if ( m_fiche != null ) {
					m_fiche.TitleChanged += fiche_TitleChanged;
					m_fiche.WebPageImageChanged += fiche_WebPageImageChanged;
					m_fiche.DOMElementsChanged += fiche_DOMElementsChanged;
					fiche_TitleChanged( m_fiche );			// Ask for title right now
					fiche_WebPageImageChanged( m_fiche );	// Ask for image right now
				}

				// Update UI
				bool	enable = m_fiche != null;

				richTextBoxTitle.Enabled = enable;
				richTextBoxTitle.Text = enable ? m_fiche.Title : "";

				richTextBoxURL.Enabled = enable;
				richTextBoxURL.Text = enable && m_fiche.URL != null ? m_fiche.URL.ToString() : "";

				tagEditBox.Enabled = enable;
				tagEditBox.RecognizedTags = enable ? m_fiche.Tags : null;

 				panelHost.Enabled = enable;
			}
		}

		public PanelMultiBitmaps.BitmapWithRectangle[]	Bitmaps {
			get {
				if ( m_fiche == null )
					return null;

				// Assign as many bitmaps as the web page requires
				Fiche.ChunkWebPageSnapshot.WebPageImagePart[]	images = m_fiche.WebPageImages;
				if ( images == null )
					return null;	// Can occur when the fiche just got created and is currently loading... (no image chunk yet!)

				// Extract bitmaps
				List<PanelMultiBitmaps.BitmapWithRectangle>	bitmaps = new List<PanelMultiBitmaps.BitmapWithRectangle>();
				foreach ( Fiche.ChunkWebPageSnapshot.WebPageImagePart image in images ) {
					if ( image == null )
						continue;

					PanelMultiBitmaps.BitmapWithRectangle	bitmap = null;
					try {
						if ( !m_webPageImagePart2Bitmap.TryGetValue( image, out bitmap ) ) {
							// Convert to bitmap
							bitmap = new PanelMultiBitmaps.BitmapWithRectangle() {
								m_displayRectangle = image.m_contentRectangle,
								m_bitmap = image.m_image != null ? image.m_image.AsBitmap : null
							};
							m_webPageImagePart2Bitmap.Add( image, bitmap );
						}
						bitmaps.Add( bitmap );
					} catch ( Exception _e ) {
						BrainForm.LogError( new Exception( "FicheWebPageAnnotatorForm => Failed to create bitmap for fiche image file!", _e ) );
					}
				}

				return bitmaps.ToArray();
			}
		}

		#endregion

		#region METHODS

		public FicheWebPageAnnotatorForm( BrainForm _owner ) : base( _owner ) {
			InitializeComponent();

			tagEditBox.ApplicationForm = m_owner;
			tagEditBox.OwnerForm = this;

			panelHost.m_childPanel = panelWebPage;

			panelHost.Focus();
		}

		private void webEditor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
			if ( e.KeyCode == Keys.Escape || e.KeyCode == ShortcutKey ) {
				Hide();
			}
		}

		protected override void InternalDispose() {
			DisposeFicheBitmaps();
		}

		void	DisposeFicheBitmaps() {
			foreach ( PanelMultiBitmaps.BitmapWithRectangle B in m_webPageImagePart2Bitmap.Values ) {
				B.m_bitmap.Dispose();
			}
			m_webPageImagePart2Bitmap.Clear();
			panelWebPage.Bitmaps = null;
			panelWebPage.ClearRectangles();
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

		delegate void fiche_TitleChangedDelegate( Fiche _sender );
		private void fiche_TitleChanged(Fiche _sender) {
			if ( InvokeRequired ) {
				BeginInvoke( new fiche_TitleChangedDelegate( fiche_TitleChanged ), _sender );
				return;
			}

			this.richTextBoxTitle.Text = _sender.Title;
		}

		delegate void fiche_WebPageImageChangedDelegate( Fiche _sender );
		private void fiche_WebPageImageChanged( Fiche _sender ) {
			if ( InvokeRequired ) {
				BeginInvoke( new fiche_WebPageImageChangedDelegate( fiche_WebPageImageChanged ), _sender );
				return;
			}

//System.Diagnostics.Debug.WriteLine( "New web page images need to be displayed!" );
//			panelWebPage.BackgroundImage = _sender.WebPageImage.AsBitmap;

			PanelMultiBitmaps.BitmapWithRectangle[]	bitmaps = this.Bitmaps;
			if ( bitmaps == null )
				return;

			panelWebPage.Bitmaps = bitmaps;
			panelWebPage.ClearRectangles();

			// Make sure the panel is at least as large as the host
			// It can be much bigger but can't be smaller
			Rectangle	pageRectangle = panelWebPage.TotalRectangle;
			panelWebPage.Width = Math.Max( panelHost.Width, pageRectangle.Right );
			panelWebPage.Height = Math.Max( panelHost.Height, pageRectangle.Bottom );
			panelWebPage.Invalidate();
		}

		private void fiche_DOMElementsChanged( Fiche _sender ) {
			panelWebPage.SuspendLayout();
			panelWebPage.ClearRectangles();
			if ( _sender.DOMElements != null ) {
				foreach ( DOMElement element in _sender.DOMElements.m_children ) {
					Color	color = Color.Magenta;
					switch ( element.m_type ) {
						case DOMElement.ELEMENT_TYPE.TEXT: color = Color.ForestGreen; break;
						case DOMElement.ELEMENT_TYPE.LINK: color = Color.CornflowerBlue; break;
						case DOMElement.ELEMENT_TYPE.IMAGE: color = Color.IndianRed; break;
					}
					panelWebPage.AddRectangle( element.m_rectangle, color );
				}
			}
			panelWebPage.ResumeLayout();
			panelWebPage.PerformLayout();
		}

		#endregion
	}
}
