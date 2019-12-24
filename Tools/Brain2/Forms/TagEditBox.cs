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

		const int	MAX_MATCHES = 10;	// Max display matches

		#endregion

		#region NESTED TYPES

		class TagName {
			public string	m_tag;
			public int		m_startIndex, m_endIndex;

			public TagName( string _tag, int _startIndex, int _endIndex ) {
				m_tag = _tag;
				m_startIndex = _startIndex;
				m_endIndex = _endIndex;
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

		BrainForm		m_ownerForm = null;
		FichesDB		m_database =  null;

		bool			m_internalChange = false;			// If true, we won't react to text change events

		List< TagName >	m_editedTags = new List<TagName>();	// The current list of tags (recognized or not)
		TagName			m_currentTag = null;

		List< Fiche >	m_matches = new List<Fiche>();		// The list of matches for the currently typed tag

		SuggestionForm	m_suggestionForm = new SuggestionForm();

		#endregion

		#region PROPERTIES

		FichesDB		Database {
			get { return m_database; }
			set { m_database = value; }
		}

		#endregion

		#region METHODS

		public TagEditBox() {
			InitializeComponent();
			m_suggestionForm.SuggestionSelected += suggestionForm_SuggestionSelected;
		}

		public TagEditBox(IContainer container) {
			container.Add(this);

			InitializeComponent();
		}

		/// <summary>
		/// Lists all the tag names found in the text box
		/// </summary>
		/// <param name="_text"></param>
		/// <param name="_caretPosition"></param>
		/// <returns></returns>
		TagName		ListEditedTagNames( string _text, int _caretPosition ) {
			int		length = _text.Length;
			TagName		currentTag = null;

			m_editedTags.Clear();
			for ( int i=0; i < length; i++ ) {

				// Read next tag name
				int	startIndex = i;
				int	endIndex = GetTagEndIndex( _text, length, startIndex );
				string	currentTagName = _text.Substring( startIndex, endIndex - startIndex );
						currentTagName = TagName.CleanTagName( currentTagName );

				if ( currentTagName != null && currentTagName.Length > 0 ) {
					// Create a valid tag
					TagName	tag = new TagName( currentTagName, startIndex, endIndex );
					m_editedTags.Add( tag );

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

		#endregion

		#region EVENTS

		protected override void OnLocationChanged(EventArgs e) {
			base.OnLocationChanged(e);
			if ( !m_suggestionForm.Visible || m_currentTag == null )
				return;

			// Locate either above or below the edit box depending on screen position
			string	textUntilTag = this.Text.Substring( 0, m_currentTag.m_startIndex );
			Size	textSizeUntilTag = TextRenderer.MeasureText( textUntilTag, this.Font );

			Point	screenBottomLeft = this.PointToScreen( this.Location );
					screenBottomLeft.Y += this.Height;				// Bottom
					screenBottomLeft.X += textSizeUntilTag.Width;	// Advance to current edition position

			if ( screenBottomLeft.Y + m_suggestionForm.Height > m_ownerForm.Bottom ) {
				screenBottomLeft.Y -= this.Height - m_suggestionForm.Height;	// Make the form pop above the text box instead, otherwise it will go too low
			}
			m_suggestionForm.Location = screenBottomLeft;
		}

		protected override void OnTextChanged(EventArgs e) {
//			base.OnTextChanged(e);
			if ( m_internalChange )
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
		}

		private void suggestionForm_SuggestionSelected(object sender, EventArgs e) {
			m_internalChange = true;
			this.Text = "PLOUP ! TODO !";
			m_internalChange = false;
		}

		#endregion
	}
}
