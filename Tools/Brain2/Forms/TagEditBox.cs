﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace Brain2 {
	public partial class TagEditBox : Panel {

		#region CONSTANTS

		const int	MAX_MATCHES = 10;		// Max display matches
		const int	MAX_TAG_LENGTH = 10;	// Max tag string length

		#endregion

		#region NESTED TYPES

		[System.Diagnostics.DebuggerDisplay( "{m_tagString != null ? m_tagString : \"<empty>\"}" )]
		class EditedTag {
			public EditedTag	m_previous = null;
			public EditedTag	m_next = null;

			private Fiche		m_fiche = null;

			public string		m_tagString = null;
			public bool			m_ellipsed = false;

			public EditedTag	First {
				get { return m_previous == null ? this : m_previous.First; }
			}

			public EditedTag	Last {
				get { return m_next == null ? this : m_next.Last; }
			}

			public EditedTag	FirstUnRecognized {
				get {
					if ( m_fiche != null )
						return null;	// We are a recognized tag!

					EditedTag	current = this;
					while ( true ) {
						if ( current.m_previous == null || current.m_previous.m_fiche != null )
							return current;

						current = current.m_previous;
					}
				}
			}

			/// <summary>
			/// Gets the index of this tag in the list of tags
			/// </summary>
			public int			Index {
				get { return m_previous != null ? m_previous.Index + 1 : 0; }
			}

			/// <summary>
			/// Gets the index of the start character in the string of characters formed by the concatenation of all tags in the linked list
			/// </summary>
 			public int			StartCharIndex {
				get { return m_previous != null ? m_previous.StartCharIndex + m_previous.m_tagString.Length : 0; }
			}

			/// <summary>
			/// Gets the index of the end character in the string of characters formed by the concatenation of all tags in the linked list
			/// </summary>
 			public int			EndCharIndex {
				get { return StartCharIndex + m_tagString.Length; }
			}

			/// <summary>
			/// Gets or sets the fiche attached to this tag. A tag with a valid Fiche is known as a "recognized tag", otherwise it's an unrecorgnized tag.
			/// </summary>
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
					if ( m_tagString.Length > MAX_TAG_LENGTH ) {
						m_tagString = m_tagString.Substring( 0, MAX_TAG_LENGTH ) + "...";
						m_ellipsed = true;
					} else {
						m_ellipsed = false;
					}
					if ( m_tagString.IndexOf( ' ' ) != -1 )
						m_tagString = "\"" + m_tagString + "\"";
					m_tagString += " ";	// Append end space
				}
			}

			public EditedTag( Fiche _fiche, EditedTag _nextTag ) {
				InsertBefore( _nextTag );
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

		BrainForm				m_applicationForm = null;
		Form					m_ownerForm = null;

		EditedTag				m_selectedTag = null;				// The currently selected tag
		EditedTag				m_hoveredTag = null;				// The tag currently hovered with the mouse

		EditedTag				m_firstUnRecognizedTag = null;
		EditedTag				m_lastUnRecognizedTag = null;
		List< Fiche >			m_matches = new List<Fiche>();		// The list of matches for the currently typed tag

		SuggestionForm			m_suggestionForm = new SuggestionForm();

		int						m_internalChange = 0;				// If > 0, we won't react to text change events
		RectangleF[]			m_renderedRectangles = new RectangleF[0];

		#endregion

		#region PROPERTIES

		public BrainForm		ApplicationForm {
			get { return m_applicationForm; }
			set {
				m_applicationForm = value;
				m_suggestionForm.Owner = m_applicationForm;
			}
		}

		public Form				OwnerForm {
			get { return m_ownerForm; }
			set {
				if ( value == m_ownerForm )
					return;

				if ( m_ownerForm != null )
					m_ownerForm.LocationChanged -= ownerForm_LocationChanged;
				m_ownerForm = value;
				if ( m_ownerForm != null )
					m_ownerForm.LocationChanged += ownerForm_LocationChanged;
			}
		}

		private void ownerForm_LocationChanged(object sender, EventArgs e) {
			Invalidate();	// Makes the suggestion form move along with the parent form
		}

		/// <summary>
		/// The list of recognized fiche tags
		/// </summary>
		public Fiche[]			RecognizedTags {
			get {
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

				m_selectedTag = new EditedTag( null, null );	// Default end tag
				foreach ( Fiche F in value ) {
					new EditedTag( F, m_selectedTag );
				}

				m_internalChange--;

				Invalidate();
			}
		}

		/// <summary>
		/// The list of tags that were typed by the user but are not yet recognized
		/// </summary>
		public string[]			UnRecognizedTags {
			get {
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

		/// <summary>
		/// Sets the currently hovered tag and possibily popup a tooltip if the full title is not visible
		/// </summary>
		private EditedTag		HoveredTag {
			get { return m_hoveredTag; }
			set {
				if ( value == m_hoveredTag )
					return;

				m_hoveredTag = value;
				if ( m_hoveredTag != null && m_hoveredTag.Fiche != null && m_hoveredTag.m_ellipsed ) {
					toolTipTag.Show( m_hoveredTag.Fiche.Title, this );
				} else {
					toolTipTag.Hide( this );
				}
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

//comprendre pourquoi on peut pas l'utiliser dans les modeless forms ?

		Brush	m_brushBack = null;
		Brush	m_brushTag = null;
		Brush	m_brushTagSelected = null;

		// From https://stackoverflow.com/questions/11708621/how-to-measure-width-of-a-string-precisely
		StringFormat	m_stringFormat = new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Center, Trimming = StringTrimming.None };//, FormatFlags = StringFormatFlags.NoClip };

		void	Init() {
			m_suggestionForm.SuggestionSelected += suggestionForm_SuggestionSelected;

			m_brushBack = new SolidBrush( this.BackColor );
			m_brushTag = new SolidBrush( Color.IndianRed );
			m_brushTagSelected = new SolidBrush( Color.RosyBrown );

			m_stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

			SetStyle( ControlStyles.Selectable, true );
			SetStyle( ControlStyles.ResizeRedraw, true );
			SetStyle( ControlStyles.EnableNotifyMessage, true );
			SetStyle( ControlStyles.ContainerControl, false );
			this.DoubleBuffered = true;

			// Create the end tag (not editable, not deletable, can't go past it)
			m_selectedTag = new EditedTag( null, null );

			this.Select();
		}

		void	InternalDispose() {
			m_brushBack.Dispose();
			m_brushTag.Dispose();
			m_brushTagSelected.Dispose();
			m_suggestionForm.Dispose();
		}

		void	DeleteTag( EditedTag _tag, bool _selectPrevious ) {
			if ( _tag == null )
				return;
			if ( _tag.m_next == null )
				throw new Exception( "Can't delete end tag!" );

			m_internalChange++;

			// Link over tag
			if ( m_selectedTag == _tag ) {
//				SelectTag( _tag.m_previous != null ? _tag.m_previous : _tag.m_next );
				m_selectedTag = _selectPrevious ? (_tag.m_previous != null ? _tag.m_previous : _tag.m_next)
												: (_tag.m_next != null ? _tag.m_next : _tag.m_previous);
			}

			_tag.Remove();

			m_internalChange--;

			Invalidate();

			UpdateSuggestionForm();
		}

		void	SelectTag( EditedTag _tag ) {
			if ( _tag == m_selectedTag )
				return;

			m_selectedTag = _tag;

			Invalidate();

			UpdateSuggestionForm();
		}

		/// <summary>
		/// Attempts to update the suggestion form with the currently entered string
		/// </summary>
		void	UpdateSuggestionForm() {
			if ( m_selectedTag == null ) {
				m_suggestionForm.Visible = false;
				return;
			}

			// Retrieve the currently unrecognized tag for the character we just typed
			m_firstUnRecognizedTag = null;
			m_lastUnRecognizedTag = m_selectedTag.FirstUnRecognized;
			if ( m_lastUnRecognizedTag == null ) {
				m_suggestionForm.Visible = false;
				return;
			}

			string	unRecognizedTagName = null;
			do {
				m_firstUnRecognizedTag = m_lastUnRecognizedTag;
				m_lastUnRecognizedTag = m_firstUnRecognizedTag.CollateUnRecognizedTags( out unRecognizedTagName );
			} while ( !m_firstUnRecognizedTag.ContainsTag( m_lastUnRecognizedTag, m_selectedTag ) );

			m_matches.Clear();
			if ( unRecognizedTagName != null ) {
				// Handle auto-completion
 				m_applicationForm.Database.FindNearestTagMatches( unRecognizedTagName, RecognizedTags, m_matches );
				if ( m_matches.Count > 0 ) {
					// Show potential matches
					string[]	matchStrings = new string[Math.Min( MAX_MATCHES, m_matches.Count )];
					for ( int matchIndex=0; matchIndex < matchStrings.Length; matchIndex++ ) {
						matchStrings[matchIndex] = m_matches[matchIndex].Title;
					}

					m_suggestionForm.UpdateList( matchStrings, 10 );
				}
			}

			if ( !m_suggestionForm.Visible &&  m_matches.Count > 0 ) {
				m_suggestionForm.Show( this );
				this.Focus();	// We must keep the focus!
			}
			m_suggestionForm.Visible = m_matches.Count > 0;
		}

		#endregion

		#region EVENTS

		protected override void OnBackColorChanged(EventArgs e) {
			base.OnBackColorChanged(e);
		}

		protected override void OnPaintBackground(PaintEventArgs e) {
			if ( m_internalChange > 0 )
				return;

//			base.OnPaintBackground(pevent);
			e.Graphics.FillRectangle( m_brushBack, e.ClipRectangle );

			if ( m_selectedTag == null )
				return;	// Nothing to paint...

			List< RectangleF >	renderedRectangles = new List<RectangleF>();

			float		X = this.Margin.Left;
			float		Y = this.Margin.Top;
			int			tagIndex = 0;
			EditedTag	currentTag = m_selectedTag.First;
			while ( currentTag != null ) {

				SizeF		textSize = e.Graphics.MeasureString( currentTag.m_tagString, this.Font, this.Width, m_stringFormat );
				RectangleF	R = new RectangleF( X, Y, textSize.Width, textSize.Height );

				if ( currentTag.Fiche != null ) {
					R.X += 4;	// Include a little margin for tags
					RectangleF	backgroundR = R;
					backgroundR.X -= 2;
					backgroundR.Y -= 2;
					backgroundR.Width += 4;
					backgroundR.Height += 4;

					textSize.Width += 8;
					e.Graphics.FillRectangle( currentTag == m_selectedTag ? m_brushTagSelected : m_brushTag, backgroundR );
				}

				if ( currentTag == m_selectedTag ) {
					e.Graphics.DrawLine( Pens.Black, X, this.Margin.Top-1, X, Height - this.Margin.Bottom-1 );
				}

				renderedRectangles.Add( R );
				X += textSize.Width;
				currentTag = currentTag.m_next;
				tagIndex++;
			}

			m_renderedRectangles = renderedRectangles.ToArray();
		}

		protected override void OnPaint(PaintEventArgs e) {
			if ( m_internalChange > 0 )
				return;

//			base.OnPaint(e);

			int			tagIndex = 0;
			EditedTag	currentTag = m_selectedTag.First;
			while ( currentTag != null ) {
				RectangleF	R = m_renderedRectangles[tagIndex++];
				e.Graphics.DrawString( currentTag.m_tagString, this.Font, Brushes.Black, R, m_stringFormat );
				currentTag = currentTag.m_next;
			}

			if ( !m_suggestionForm.Visible || m_firstUnRecognizedTag == null || m_firstUnRecognizedTag.Index >= m_renderedRectangles.Length )
				return;

			// Locate either above or below the edit box depending on screen position
 			Point	screenBottomLeft = this.PointToScreen( Point.Empty );
 					screenBottomLeft.Y += this.Height;				// Bottom
 					screenBottomLeft.X += (int) m_renderedRectangles[m_firstUnRecognizedTag.Index].X;	// Advance to current edition position

			if ( screenBottomLeft.Y + m_suggestionForm.Height > m_applicationForm.Bottom ) {
				screenBottomLeft.Y -= this.Height - m_suggestionForm.Height;	// Make the form pop above the text box instead, otherwise it will go too low
			}
			m_suggestionForm.Location = screenBottomLeft;
		}
		protected override void OnEnter(EventArgs e) {
			this.Invalidate();
			base.OnEnter(e);
		}
		protected override void OnLeave(EventArgs e) {
			HoveredTag = null;
			this.Invalidate();
			base.OnLeave(e);
		}
		protected override void OnMouseDown(MouseEventArgs e) {
			Invalidate();
			Focus();

			// Select the proper tag according to where they were drawn last
			EditedTag	currentTag = m_selectedTag.First;
			for ( int tagIndex=0; tagIndex < m_renderedRectangles.Length; tagIndex++ ) {
				RectangleF	R = m_renderedRectangles[tagIndex];
				if ( R.Contains( e.Location ) ) {
					// Found the selected tag!
					float	clickRatio = (float) (e.X - R.Left) / R.Width;
					if ( currentTag.m_next != null && clickRatio > 0.66f )
						currentTag = currentTag.m_next;	// If the user clicks too far on the right then select next tag instead...
					SelectTag( currentTag );
					return;
				}
				currentTag = currentTag.m_next;
			}

			// Select "after last tag" if none was found
			SelectTag( m_selectedTag.Last );

			base.OnMouseDown(e);
		}
		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			if ( m_internalChange > 0 )
				return;

			int			tagIndex = 0;
			EditedTag	currentTag = m_selectedTag.First;
			while ( currentTag != null && tagIndex < m_renderedRectangles.Length ) {
				RectangleF	R = m_renderedRectangles[tagIndex++];
				if ( R.Contains( e.Location) ) {
					HoveredTag = currentTag;
					return;
				}
				currentTag = currentTag.m_next;
			}

			HoveredTag = null;
		}

		protected override void OnVisibleChanged(EventArgs e) {
			base.OnVisibleChanged(e);
			m_suggestionForm.Visible = false;
		}

		// https://stackoverflow.com/questions/3562235/panel-not-getting-focus
		protected override bool IsInputKey(Keys keyData) {
			switch ( keyData ) {
				case Keys.Up:
				case Keys.Down:
				case Keys.Left:
				case Keys.Right:
				case Keys.Tab:
					return true;
			}
			return base.IsInputKey(keyData);
		}

// 		protected override bool ProcessKeyMessage(ref Message m) {
// 			switch ( m.Msg ) {
// 				case Interop.WM_KEYDOWN:
// 					Keys	key = (Keys) m.WParam;
// 					switch ( key ) {
// 						case Keys.Escape:
// 							return base.ProcessKeyMessage(ref m);	// Feed to parent to close the form
//  					}
//  					break;
// 			}
// 
// 			return base.ProcessKeyMessage(ref m);
// 		}

		protected override void OnKeyDown(KeyEventArgs e) {
			e.SuppressKeyPress = true;

			switch ( e.KeyCode ) {
				case Keys.Back:
					if ( m_selectedTag.m_previous != null ) {
						DeleteTag( m_selectedTag.m_previous, false );
					}
					break;

				case Keys.Delete:
					if ( m_selectedTag.m_next != null ) {
						DeleteTag( m_selectedTag, false );
					}
					break;

				case Keys.Left:
					if ( m_selectedTag.m_previous != null ) {
						SelectTag( m_selectedTag.m_previous );
					}
					break;

				case Keys.Right:
					if ( m_selectedTag.m_next != null ) {
						SelectTag( m_selectedTag.m_next );
					}
					break;

				case Keys.Home:
					SelectTag( m_selectedTag.First );
					break;

				case Keys.End:
					SelectTag( m_selectedTag.Last );
					break;

				case Keys.Down:
					if ( m_suggestionForm.IsSuggesting ) {
						m_suggestionForm.SelectedSuggestionIndex++;
					}
					break;

				case Keys.Up:
					if ( m_suggestionForm.IsSuggesting ) {
						m_suggestionForm.SelectedSuggestionIndex--;
					}
					break;

				case Keys.Return:
				case Keys.Tab:
					if ( m_suggestionForm.IsSuggesting )
						m_suggestionForm.AcceptSuggestion();
					break;

 				default:
					e.SuppressKeyPress = false;
 					break;
			}

			base.OnKeyDown(e);
		}

		protected override void OnKeyPress(KeyPressEventArgs e) {
			base.OnKeyPress(e);

// BrainForm.Debug( "KeyPress => " + (((int) e.KeyChar) >= 32	? e.KeyChar.ToString()
// 															: ("0x" + ((int) e.KeyChar).ToString( "X2" )))
// 				);

			// Create a brand new edited tag that will host the text typed in by the user
			EditedTag	newTag = new EditedTag( null, m_selectedTag );
						newTag.m_tagString = e.KeyChar.ToString();
			UpdateSuggestionForm();
			Invalidate();
		}

// 		KeysConverter	kc = new KeysConverter();
// 		char	Message2CharANSI( Keys _key ) {
// 			int		keyValue = (int) _key;
// 					keyValue &= 0x7F;	// Ignore culture-specific characters
// 			if ( keyValue < 32 )
// 				return '\0';	// Unsupported...
// 
// 			if ( _key == Keys.Shift )
// 				return '\0';
// 
// 			Keys	rawKey = (Keys) keyValue;
// 			if ( rawKey == Keys.Space )
// 				return ' ';
// 
// 			string	C = kc.ConvertToString( rawKey );
// 			if ( C.Length != 1 )
// 				return '\0';	// Unsupported...
// 
// //			if ( (_key & Keys.Shift) != 0 )
// 			if ( Control.ModifierKeys == Keys.Shift )
// 				C = C.ToUpper();
// 			else
// 				C = C.ToLower();
// 
// 			return C[0];
// 		}

		private void suggestionForm_SuggestionSelected(object sender, EventArgs e) {
			m_internalChange++;

			// Keep a copy of the matched selection as it will get treashed by the Select/Remove operations that follow
			EditedTag	firstMatchedTag = m_firstUnRecognizedTag;
			EditedTag	lastMatchedTag = m_lastUnRecognizedTag;

			// Select the first tag used for the match & assign the fiche
			firstMatchedTag.Fiche = m_matches[m_suggestionForm.SelectedSuggestionIndex];

			// Remove orphan tags that were part of the match
			if ( firstMatchedTag != lastMatchedTag ) {
				EditedTag	currentTag = firstMatchedTag.m_next;
				while ( currentTag != lastMatchedTag && currentTag.m_next != null ) {
					EditedTag	tagToRemove = currentTag;
					currentTag = currentTag.m_next;
					DeleteTag( tagToRemove, true );
				}
			}

			SelectTag( firstMatchedTag.m_next );

			m_internalChange--;

			Invalidate();
		}

		private void toolTipTag_Popup(object sender, PopupEventArgs e) {
//			toolTipTag.ToolTipTitle = m_selectedTag != null && m_selectedTag.Fiche != null ? m_selectedTag.Fiche.Title : null;
		}

		#endregion
	}
}
