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

		[System.Diagnostics.DebuggerDisplay( "{m_tagString != null ? m_tagString : \"<empty>\"}" )]
		class EditedTag {
			public EditedTag	m_previous = null;
			public EditedTag	m_next = null;

			private Fiche		m_fiche = null;

			public string		m_tagString = null;

			public EditedTag	First {
				get { return m_previous == null ? this : m_previous.First; }
			}

			public EditedTag	Last {
				get { return m_next == null ? this : m_next.Last; }
			}

			public EditedTag	FirstUnRecognized {
				get {
					EditedTag	current = this;
					while ( true ) {
						if ( current.m_previous == null || current.m_previous.m_fiche != null )
							return current;

						current = current.m_previous;
					}
				}
			}

 			public int			StartIndex {
				get { return m_previous != null ? m_previous.StartIndex + m_previous.m_tagString.Length : 0; }
			}

 			public int			EndIndex {
				get { return StartIndex + m_tagString.Length; }
			}

			public Fiche		Fiche {
				get { return m_fiche; }
				set {
					m_fiche = value;
					if ( m_fiche == null ) {
						m_tagString = "";
						return;
					}

					// Build tag string
					m_tagString = CleanTagName( m_fiche.Title );
					if ( m_tagString.Length > MAX_TAG_LENGTH )
						m_tagString = m_tagString.Substring( 0, MAX_TAG_LENGTH ) + "...";
					if ( m_tagString.IndexOf( ' ' ) != -1 )
						m_tagString = "\"" + m_tagString + "\"";
					m_tagString += " ";	// Append end space
				}
			}

			public EditedTag( Fiche _fiche, EditedTag _previousTag ) {
				if ( _previousTag != null )
					InsertAfter( _previousTag );

				Fiche = _fiche;
			}

			public override string ToString() {
				return m_tagString;
			}

			/// <summary>
			/// Inserts THIS after _tag
			/// </summary>
			/// <param name="_tag"></param>
			public void	InsertAfter( EditedTag _tag ) {
				if ( _tag == null ) {
					m_next = null;
					m_previous = null;
					return;
				}

				m_previous = _tag;
				m_next = _tag.m_next;
				if ( m_next != null )
					m_next.m_previous = this;
				if ( m_previous != null )
					m_previous.m_next = this;
			}

			/// <summary>
			/// Inserts THIS before _tag
			/// </summary>
			/// <param name="_tag"></param>
			public void	InsertBefore( EditedTag _tag ) {
				if ( _tag == null ) {
					m_next = null;
					m_previous = null;
					return;
				}

				m_next = _tag;
				m_previous = _tag.m_previous;
				if ( m_next != null )
					m_next.m_previous = this;
				if ( m_previous != null )
					m_previous.m_next = this;
			}

			/// <summary>
			/// Removes this tag from the linked-list
			/// </summary>
			public void	Remove() {
				if ( m_next != null )
					m_next.m_previous = m_previous;
				if ( m_previous != null )
					m_previous.m_next = m_next;
				m_previous = m_next = null;
			}

			/// <summary>
			/// Starts from this tag and collate individual strings forkward until a tag separator is found so we obtain the full name of an unrecognized tag
			/// </summary>
			/// <param name="_tagName">The new unrecognized tag name</param>
			/// <returns>The tag where we ended up the collation</returns>
			public EditedTag		CollateUnRecognizedTags( out string _tagName ) {
				_tagName = null;

				bool		isComplexName = false;
				EditedTag	currentTag = this;
				while ( currentTag != null ) {
					if ( currentTag.Fiche != null )
						break;	// This is a recognized tag, break here...

					// Check for double-quotes that mark either the start or end of a complex tag name
					if ( currentTag.m_tagString == "\"" ) {
						currentTag = currentTag.m_next;	// Always skip double quote
						if ( isComplexName ) {
							break;					// End marker!
						} else {
							isComplexName = true;	// Start marker!
						}
					} else {
						// Check for space separators that mark the end of a simple tag name
						if ( !isComplexName && (currentTag.m_tagString == " " || currentTag.m_tagString == "\t") ) {
							// End marker that we need to skip
							currentTag = currentTag.m_next;
							break;
						}
					}

					// Append tag's string to current tag name and proceed
					_tagName += currentTag.m_tagString;
					currentTag = currentTag.m_next;
				}

				return currentTag;
			}

			/// <summary>
			/// Checks if the list from this tag to the provided _endTag contains the _tag tag
			/// </summary>
			/// <param name="_endTag">Tag marking the end of the list</param>
			/// <param name="_tag">The tag to check as part of the linked-list</param>
			/// <returns></returns>
			public bool	ContainsTag( EditedTag _endTag, EditedTag _tag ) {
				EditedTag	currentTag = this;
				while ( currentTag != null && currentTag != _endTag ) {
					if ( currentTag == _tag )
						return true;	// Found it
					currentTag = currentTag.m_next;
				}
				return false;
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

		EditedTag				m_selectedTag = null;				// The currently-selected tag

		List< Fiche >			m_matches = new List<Fiche>();		// The list of matches for the currently typed tag

		SuggestionForm			m_suggestionForm = new SuggestionForm();

		int						m_internalChange = 0;				// If > 0, we won't react to text change events

		#endregion

		#region PROPERTIES

		public BrainForm		OwnerForm {
			get { return m_ownerForm; }
			set { m_ownerForm = value; }
		}

		/// <summary>
		/// The list of recognized fiche tags
		/// </summary>
		public Fiche[]			RecognizedTags {
			get {
				if ( m_selectedTag == null )
					return new Fiche[0];

				List<Fiche>	result = new List<Fiche>();
				EditedTag	currentTag = m_selectedTag.First;
				while ( currentTag != null ) {
					if ( currentTag.Fiche != null )
						result.Add( currentTag.Fiche );
					currentTag = currentTag.m_next;
				}
				return result.ToArray();
			}
			set {
				if ( value == null )
					value = new Fiche[0];
				
				m_internalChange++;

				EditedTag	previousTag = null;
				string		text = "";
				foreach ( Fiche F in value ) {
					EditedTag	T = new EditedTag( F, previousTag );

					text += T.ToString();

					previousTag = T;
				}

				// Update text
				this.Text = text;
				m_internalChange--;
			}
		}

		/// <summary>
		/// The list of tags that were typed by the user but are not yet recognized
		/// </summary>
		public string[]			UnRecognizedTags {
			get {
				if ( m_selectedTag == null )
					return new string[0];

				List<string>	result = new List<string>();
				EditedTag		currentTag = m_selectedTag.First;
				while ( currentTag != null ) {
					if ( currentTag.Fiche != null ) {
						currentTag = currentTag.m_next;	// That's a recognized tag...
						continue;
					}

					// Collate unrecognized tags until they form a potential new tag name
					string	unRecognizedTag;
					currentTag = currentTag.CollateUnRecognizedTags( out unRecognizedTag );

					result.Add( currentTag.m_tagString );
				}

				return result.ToArray();
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
/*		void	AddTag( EditedTag _tag ) {
			if ( _tag == null )
				return;

			// Include it
			m_internalChange++;

			int	tagStartIndex = m_selectedTag != null ? m_selectedTag.StartIndex : 0;
			this.Text = this.Text.Substring( 0, tagStartIndex ) + _tag.m_tagString + this.Text.Substring( tagStartIndex );

			// Link tag in
			if ( m_selectedTag != null ) {
				_tag.m_previous = m_selectedTag.m_previous;
				_tag.m_next = m_selectedTag;
			}
			if ( _tag.m_previous != null )
				_tag.m_previous.m_next = _tag;
			if ( _tag.m_next != null )
				_tag.m_next.m_previous = _tag;

			SelectTag( _tag );

			Invalidate();
			m_internalChange--;
		}
*/
		void	DeleteTag( EditedTag _tag ) {
			if ( _tag == null )
				return;

			m_internalChange++;

			// Link over tag
			if ( m_selectedTag == _tag ) {
				SelectTag( _tag.m_previous != null ? _tag.m_previous : _tag.m_next );
			}
			_tag.Remove();

			// Remove it
			this.Text = this.Text.Remove( _tag.StartIndex, _tag.m_tagString.Length );

			Invalidate();
			m_internalChange--;
		}

		void	SelectTag( EditedTag _tag ) {
			if ( _tag == null )
				return;

			m_internalChange++;
			this.SelectionStart = _tag.StartIndex;
			m_selectedTag = _tag;
			Invalidate();
			m_internalChange--;
		}

		#endregion

		#region EVENTS

		protected override void OnPaintBackground(PaintEventArgs pevent) {
//			base.OnPaintBackground(pevent);
			pevent.Graphics.FillRectangle( Brushes.Black, pevent.ClipRectangle );
		}

		protected override void OnPaint(PaintEventArgs e) {

// @TODO:
//	• Render tags with background color depending on recognition
//	• Render suggested text as light gray

//			base.OnPaint(e);
		}

		protected override bool ProcessKeyMessage(ref Message m) {

			switch ( m.Msg ) {
				case Interop.WM_KEYDOWN:
					Keys	key = (Keys) m.WParam;
					switch ( key ) {
						case Keys.Escape:
							return base.ProcessKeyMessage(ref m);	// Feed to parent to close the form

						case Keys.Back:
							if ( m_selectedTag != null ) {
								EditedTag	tagToDelete = m_selectedTag.m_previous != null ? m_selectedTag.m_previous : m_selectedTag;
								if ( m_selectedTag.m_next == null ) {
									// Special care must be used for last tag in case the actual selection is beyond last tag, even though that's the last tag that's selected: we must delete the selected tag!
									if ( this.SelectionStart >= m_selectedTag.EndIndex )
										tagToDelete = m_selectedTag;
								}
								DeleteTag( tagToDelete );
							}
							return true;

						case Keys.Delete:
							if ( m_selectedTag != null ) {
								DeleteTag( m_selectedTag );
							}
							return true;

						case Keys.Left:
							if ( m_selectedTag != null )
								SelectTag( m_selectedTag.m_previous );
							return true;

						case Keys.Right:
							if ( m_selectedTag != null )
								SelectTag( m_selectedTag.m_next );
							return true;

						case Keys.Home:
							if ( m_selectedTag != null )
								SelectTag( m_selectedTag.First );
							return true;

						case Keys.End:
							if ( m_selectedTag != null )
								SelectTag( m_selectedTag.Last );
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
							char	C = Message2CharANSI( key );
 							if ( C == '\0' )
 								break;	// Unsupported...

							// Create a brand new edited tag that will host the text typed in by the user
							m_selectedTag = new EditedTag( null, m_selectedTag );
							m_selectedTag.m_tagString += C;

							// Retrieve the currently unrecognized tag for the character we just typed
							string		unRecognizedTagName = null;
							EditedTag	firstUnRecognizedTag = null;
							EditedTag	lastUnRecognizedTag	= m_selectedTag.FirstUnRecognized;
							do {
								firstUnRecognizedTag = lastUnRecognizedTag;
								lastUnRecognizedTag	= firstUnRecognizedTag.CollateUnRecognizedTags( out unRecognizedTagName );
							} while ( !firstUnRecognizedTag.ContainsTag( lastUnRecognizedTag, m_selectedTag ) );

							// Handle auto-completion
							m_matches.Clear();
 							m_ownerForm.Database.FindNearestTagMatches( unRecognizedTagName, m_matches );
							if ( m_matches.Count == 0 ) {
								// No match...
								if ( m_suggestionForm.Visible )
									m_suggestionForm.Hide();
								break;
							}

							// Show potential matches
							string[]	matchStrings = new string[Math.Min( MAX_MATCHES, m_matches.Count )];
							for ( int matchIndex=0; matchIndex < matchStrings.Length; matchIndex++ )
								matchStrings[matchIndex] = m_matches[matchIndex].Title;

							m_suggestionForm.UpdateList( matchStrings, 10 );

							if ( !m_suggestionForm.Visible )
								m_suggestionForm.Show( this );

							break;
					}

					break;
			}

			return base.ProcessKeyMessage(ref m);
		}

		KeysConverter	kc = new KeysConverter();
		char	Message2CharANSI( Keys _key ) {
			int		keyValue = (int) _key;
					keyValue &= 0x7F;	// Ignore culture-specific characters
			if ( keyValue < 32 )
				return '\0';	// Unsupported...

			Keys	rawKey = (Keys) keyValue;
			string	C = kc.ConvertToString( rawKey );
			if ( C.Length != 1 )
				return '\0';	// Unsupported...

//			if ( (_key & Keys.Shift) != 0 )
			if ( Control.ModifierKeys == Keys.Shift )
				C = C.ToUpper();
			else
				C = C.ToLower();

			return C[0];
		}

// 		protected override void OnKeyDown(KeyEventArgs e) {
// 			base.OnKeyDown(e);
// 		}
// 		protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
// 			base.OnPreviewKeyDown(e);
// 		}

		protected override void OnSelectionChanged(EventArgs e) {
			if ( m_internalChange > 0 || m_selectedTag == null )
				return;

			EditedTag	currentTag = m_selectedTag.First;
			while ( currentTag != null ) {
				int	startIndex = currentTag.StartIndex;
				if ( SelectionStart > startIndex && SelectionStart < startIndex + currentTag.m_tagString.Length ) {
					// Select new tag
					SelectTag( currentTag );
					break;
				}
				currentTag = currentTag.m_next;
			}

			base.OnSelectionChanged(e);
		}
/*
		protected override void OnTextChanged(EventArgs e) {
			base.OnTextChanged(e);
			if ( m_internalChange )
				return;

			// Retrieve the tag we're currently modifying
			m_selectedTag = ListEditedTagNames( this.Text, this.SelectionStart );
			if ( m_selectedTag == null )
				return;

			// Handle auto-completion
			m_matches.Clear();
 			m_database.FindNearestTagMatches( m_selectedTag.m_tag, m_matches );
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
		}
*/

		protected override void OnLocationChanged(EventArgs e) {
			base.OnLocationChanged(e);
			if ( !m_suggestionForm.Visible || m_selectedTag == null )
				return;

			// Locate either above or below the edit box depending on screen position
			string	textUntilTag = this.Text.Substring( 0, m_selectedTag.StartIndex );
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
			m_selectedTag.Fiche = suggestedFiche;
			Invalidate();
// 			EditedTag	newTag = new EditedTag( suggestedFiche, m_selectedTag );
// 			AddTag( newTag );
		}

		private void toolTipTag_Popup(object sender, PopupEventArgs e) {
			toolTipTag.ToolTipTitle = m_selectedTag != null && m_selectedTag.Fiche != null ? m_selectedTag.Fiche.Title : null;
		}

		#endregion
	}
}
