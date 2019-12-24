using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Brain2 {
	public partial class TagEditBox : RichTextBox {

		#region CONSTANTS

		const int	MAX_MATCHES = 10;		// Max display matches
		const int	MAX_TAG_LENGTH = 10;	// Max tag length

		#endregion

		#region NESTED TYPES

		[System.Diagnostics.DebuggerDisplay( "{m_tagString}" )]
		class EditedTag {
			public EditedTag	m_previous = null;
			public EditedTag	m_next = null;
			public Fiche		m_tag = null;
			public string		m_tagString = null;

			public EditedTag	First {
				get { return m_previous == null ? this : m_previous.First; }
			}

			public EditedTag	Last {
				get { return m_next == null ? this : m_next.Last; }
			}

 			public int			StartIndex {
				get { return m_previous != null ? m_previous.StartIndex + m_previous.m_tagString.Length : 0; }
			}

			public EditedTag( Fiche _tag, EditedTag _previousTag ) {
				m_tag = _tag;
				m_previous = _previousTag;
				if ( m_previous != null )
					m_previous.m_next = this;

				// Build tag string
				m_tagString = CleanTagName( m_tag.Title );
				if ( m_tagString.Length > MAX_TAG_LENGTH )
					m_tagString = m_tagString.Substring( 0, MAX_TAG_LENGTH ) + "...";
				if ( m_tagString.IndexOf( ' ' ) != -1 )
					m_tagString = "\"" + m_tagString + "\"";
				m_tagString += " ";	// Append end space
			}

			public override string ToString() {
				return m_tagString;
			}

			public static string	CleanTagName( string _tagName ) {
				if ( _tagName == null || _tagName.Length == 0 )
					return null;

				_tagName = _tagName.ToLower();

				// Remove any start or end quotes
				_tagName.Trim( '"' );

				// Remove any head #
				if ( _tagName[0] == '#' )
					_tagName = _tagName.Substring( 1 );

				return _tagName;
			}
		}

		#endregion

		#region FIELDS

		BrainForm				m_ownerForm = null;

		List< EditedTag >		m_tags = new List<EditedTag>();		// The current list of tags (recognized or not)
		EditedTag				m_currentTag = null;				// The currently-selected tag

		List< Fiche >			m_matches = new List<Fiche>();		// The list of matches for the currently typed tag

		SuggestionForm			m_suggestionForm = new SuggestionForm();

		bool					m_internalChange = false;			// If true, we won't react to text change events

		#endregion

		#region PROPERTIES

		public BrainForm		OwnerForm {
			get { return m_ownerForm; }
			set { m_ownerForm = value; }
		}

		public Fiche[]		Tags {
			get {
				Fiche[]	result = new Fiche[m_tags.Count];
				for ( int i=0; i < m_tags.Count; i++ )
					result[i] = m_tags[i].m_tag;
				return result;
			}
			set {
				if ( value == null )
					value = new Fiche[0];
				
				m_tags.Clear();

				EditedTag	previousTag = null;
				string		text = "";
				foreach ( Fiche F in value ) {
					EditedTag	T = new EditedTag( F, previousTag );
					m_tags.Add( T );

					string	tagName = T.ToString();
					text += tagName;

					previousTag = T;
				}

				// Update text
				m_internalChange = true;
				this.Text = text;
				m_internalChange = false;
			}
		}

		#endregion

		#region METHODS

		public TagEditBox() {
			InitializeComponent();
			Init();
		}

		public TagEditBox(IContainer container) {
			container.Add(this);
			InitializeComponent();
			Init();
		}

		void	Init() {
			m_suggestionForm.SuggestionSelected += suggestionForm_SuggestionSelected;
		}
/*
		/// <summary>
		/// Lists all the tag names found in the text box
		/// </summary>
		/// <param name="_text"></param>
		/// <param name="_caretPosition"></param>
		/// <returns></returns>
		Tag		ListEditedTagNames( string _text, int _caretPosition ) {
			int		length = _text.Length;
			Tag		currentTag = null;

			m_tags.Clear();
			for ( int i=0; i < length; i++ ) {

				// Read next tag name
				int	startIndex = i;
				int	endIndex = GetTagEndIndex( _text, length, startIndex );
				string	currentTagName = _text.Substring( startIndex, endIndex - startIndex );
						currentTagName = Tag.CleanTagName( currentTagName );

				if ( currentTagName != null && currentTagName.Length > 0 ) {
					// Create a valid tag
					Tag	tag = new Tag( currentTagName, startIndex, endIndex );
					m_tags.Add( tag );

					if ( _caretPosition >= startIndex && _caretPosition <= endIndex )
						currentTag = tag;
				}

				// Skip tag
				i = endIndex;
			}

			return currentTag;
		}

		int		GetTagEndIndex( string _text, int _length, int _startIndex ) {
			bool	isQuoted = false;
			for ( ; _startIndex < _length; _startIndex++ ) {
				char C = _text[_startIndex];
				if ( C == '"' ) {
					if ( isQuoted )
						return ++_startIndex;	// Found end quote, denoting a tag separator...
					isQuoted = true;	// Start a quoted tag
				} else if ( C == ' ' && !isQuoted )
					return _startIndex;	// Found separator...
			}

			return _startIndex;
		}
*/
		void	AddTag( EditedTag _tag ) {
			if ( _tag == null )
				return;

			// Include it
			m_internalChange = true;

			m_tags.Add( _tag );
			int	tagStartIndex = m_currentTag != null ? m_currentTag.StartIndex : 0;
			this.Text = this.Text.Substring( 0, tagStartIndex ) + _tag.m_tagString + this.Text.Substring( tagStartIndex );

			// Link tag in
			if ( m_currentTag != null ) {
				_tag.m_previous = m_currentTag.m_previous;
				_tag.m_next = m_currentTag;
			}
			if ( _tag.m_previous != null )
				_tag.m_previous.m_next = _tag;
			if ( _tag.m_next != null )
				_tag.m_next.m_previous = _tag;

			SelectTag( _tag );

			m_internalChange = false;
		}

		void	DeleteTag( EditedTag _tag ) {
			if ( _tag == null )
				return;

			m_internalChange = true;

			// Link over tag
			if ( _tag.m_next != null ) {
				_tag.m_next.m_previous = _tag.m_previous;
				SelectTag( _tag.m_next );
			}
			if ( _tag.m_previous != null ) {
				_tag.m_previous.m_next = _tag.m_next;
				SelectTag( _tag.m_previous );
			}

			// Remove it
			m_tags.Remove( _tag );
			this.Text = this.Text.Remove( _tag.StartIndex, _tag.m_tagString.Length );

			m_internalChange = false;
		}

		void	SelectTag( EditedTag _tag ) {
			if ( _tag == null )
				return;

			m_internalChange = true;
			this.SelectionStart = _tag.StartIndex;
			m_currentTag = _tag;
			m_internalChange = false;
		}

		#endregion

		#region EVENTS

		protected override bool ProcessKeyMessage(ref Message m) {

			switch ( m.Msg ) {
				case Interop.WM_KEYDOWN:
					Keys	key = (Keys) m.WParam;
					switch ( key ) {
						case Keys.Escape:
							return base.ProcessKeyMessage(ref m);	// Feed to parent to close the form

						case Keys.Back:
							if ( m_currentTag != null ) {
								EditedTag	tagToDelete = this.SelectionStart > m_currentTag.StartIndex ? m_currentTag.m_previous : m_currentTag;
								DeleteTag( tagToDelete );
							}
							return true;

						case Keys.Delete:
							if ( m_currentTag != null ) {
								DeleteTag( m_currentTag );
							}
							return true;

						case Keys.Left:
							if ( m_currentTag != null )
								SelectTag( m_currentTag.m_previous );
							return true;

						case Keys.Right:
							if ( m_currentTag != null )
								SelectTag( m_currentTag.m_next );
							return true;

						case Keys.Home:
							if ( m_currentTag != null )
								SelectTag( m_currentTag.First );
							return true;

						case Keys.End:
							if ( m_currentTag != null )
								SelectTag( m_currentTag.Last );
							return true;

						case Keys.Down:
							if ( m_suggestionForm.IsSuggesting )
								m_suggestionForm.Focus();
							return true;

						case Keys.Return:
							if ( m_suggestionForm.IsSuggesting )
								m_suggestionForm.AcceptSuggestion();
							return true;

						default:
							// Handle any other key as a possible new tag to auto-complete using the suggestion form
							break;
					}

					break;
			}

			return base.ProcessKeyMessage(ref m);
		}

// 		protected override void OnKeyDown(KeyEventArgs e) {
// 			base.OnKeyDown(e);
// 		}
// 		protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
// 			base.OnPreviewKeyDown(e);
// 		}

		protected override void OnSelectionChanged(EventArgs e) {
			if ( m_internalChange )
				return;

			foreach ( EditedTag tag in m_tags ) {
				int	startIndex = tag.StartIndex;
				if ( SelectionStart > startIndex && SelectionStart < startIndex + tag.m_tagString.Length ) {
					// Select new tag
					SelectTag( tag );
					break;
				}
			}

			base.OnSelectionChanged(e);
		}

		protected override void OnTextChanged(EventArgs e) {
			base.OnTextChanged(e);
/*			if ( m_internalChange )
				return;

			// Retrieve the tag we're currently modifying
			m_currentTag = ListEditedTagNames( this.Text, this.SelectionStart );
			if ( m_currentTag == null )
				return;

			// Handle auto-completion
			m_matches.Clear();
 			m_database.FindNearestTagMatches( m_currentTag.m_tag, m_matches );
			if ( m_matches.Count == 0 ) {
				// No match...
				if ( m_suggestionForm.Visible )
					m_suggestionForm.Hide();
				return;
			}

			// Show potential matches
			string[]	matchStrings = new string[Math.Min( MAX_MATCHES, m_matches.Count )];
			for ( int matchIndex=0; matchIndex < matchStrings.Length; matchIndex++ )
				matchStrings[matchIndex] = m_matches[matchIndex].Title;

			m_suggestionForm.UpdateList( matchStrings, 10 );

			if ( !m_suggestionForm.Visible )
				m_suggestionForm.Show( this );

			// Update location
			OnLocationChanged( e );
			this.Focus();
*/
		}

		protected override void OnLocationChanged(EventArgs e) {
			base.OnLocationChanged(e);
			if ( !m_suggestionForm.Visible || m_currentTag == null )
				return;

			// Locate either above or below the edit box depending on screen position
			string	textUntilTag = this.Text.Substring( 0, m_currentTag.StartIndex );
			Size	textSizeUntilTag = TextRenderer.MeasureText( textUntilTag, this.Font );

			Point	screenBottomLeft = this.PointToScreen( this.Location );
					screenBottomLeft.Y += this.Height;				// Bottom
					screenBottomLeft.X += textSizeUntilTag.Width;	// Advance to current edition position

			if ( screenBottomLeft.Y + m_suggestionForm.Height > m_ownerForm.Bottom ) {
				screenBottomLeft.Y -= this.Height - m_suggestionForm.Height;	// Make the form pop above the text box instead, otherwise it will go too low
			}
			m_suggestionForm.Location = screenBottomLeft;
		}

		private void suggestionForm_SuggestionSelected(object sender, EventArgs e) {
			Fiche		suggestedFiche = m_matches[m_suggestionForm.SelectedSuggestionIndex];
			EditedTag	newTag = new EditedTag( suggestedFiche, m_currentTag );

			AddTag( newTag );
		}

		private void toolTipTag_Popup(object sender, PopupEventArgs e) {
			toolTipTag.ToolTipTitle = m_currentTag != null ? m_currentTag.m_tag.Title : "";
		}

		#endregion
	}
}
