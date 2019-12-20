using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brain2 {
	public partial class FastTaggerForm : ModelessForm {
//	public partial class FastTaggerForm : Form {

		protected override bool Sizeable => true;
		public override Keys	SHORTCUT_KEY => Keys.None;

		Fiche[]			m_fiches = null;
		List< Fiche >	m_tags = new List<Fiche>();

		// The list of edited tags
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
		List< TagName >		m_editedTags = new List<TagName>();

		public FastTaggerForm( BrainForm _owner, Fiche[] _fiches ) : base( _owner ) {
			InitializeComponent();
			m_fiches = _fiches;

			// List common tags
			Dictionary< Fiche, uint >	tag2Count = new Dictionary<Fiche, uint>();
			foreach ( Fiche F in _fiches )
				foreach ( Fiche tag in F.Tags )
					tag2Count[tag]++;

			string	tagsText = "";
			foreach ( Fiche F in tag2Count.Keys ) {
				if ( tag2Count[F] == _fiches.Length ) {
					// New common tag
					m_tags.Add( F );

					if ( F.Title.IndexOf( ' ' ) != -1 )
						tagsText += " \"" + F.Title + "\"";
					else
						tagsText += " " + F.Title;
				}
			}

			if ( tagsText.Length > 0 )
				tagsText.Remove( 0, 1 );
			richTextBoxTags.Text = tagsText;
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

		private void richTextBoxTags_TextChanged(object sender, EventArgs e) {

			// Retrieve the tag we're currently modifying
			TagName	currentTag = ListEditedTagNames( richTextBoxTags.Text, richTextBoxTags.SelectionStart );

			// Handle auto-completion
// 			FicheDB.FindNearestTagMatches()
// 
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
	}
}
