//////////////////////////////////////////////////////////////////////////
// Implements a simple concepts parser
// Builds a collection of syntactic tokens that can later be semantically interpreted
// 
// The syntax to create concepts is:
//
//	a.b."c" : ( d, e => (f, g), h = i, j( "3" ) )
//	<context> = k
//
// Here, the *fully-qualified* concept "a.b.c" has a group (indicated by the '(' character) of *features* (indicated by the ':' character) composed of:
//	• Concept "d"
//	• Concept "e" which is composed of a group of *children* concepts (indicated by the '=>' character) "f" and "g"
//	• Concept "i" which is *aliased* (indicated by the '=' character) by the string "h"
//	• Concept "j" which is *constructed* (indicated by the '(' character) with the *value* "3"
//
// On the 2nd line, we define the "<context>" as the concept "k"
// 
//////////////////////////////////////////////////////////////////////////
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtoParser
{	[System.Diagnostics.DebuggerDisplay( "State = {m_state.Peek()}" )]
	public class Parser {

		#region CONSTANTS

		const char	CHAR_COMMENT = '#';							// Represents a line comment
		const char	CHAR_SPECIAL_STRING_DELIMITER_START = '<';	// e.g. <context>
		const char	CHAR_SPECIAL_STRING_DELIMITER_END = '>';	// e.g. <root>
		const char	CHAR_STRING_DELIMITER = '"';				// e.g. "Hello World!"
		const char	CHAR_HIERARCHY = '>';						// e.g. a => b (only the 2nd character is important since the first character is the assignment character and the disambuiation is then made on the 2nd character)
		const char	CHAR_CHILD_MARKER = '.';					// e.g. a.b.c
		const char	CHAR_COLLECTION_START = '(';				// e.g. ( a, b, c )
		const char	CHAR_COLLECTION_END = ')';					// e.g. ( a, b, c )
		const char	CHAR_COLLECTION_ITEM_SEPARATOR = ',';		// e.g. ( a, b, c )
		const char	CHAR_ALIAS_ASSIGNMENT = '=';				// e.g. alias = "stuff"
		const char	CHAR_FEATURES_COLLECTION_MARKER = ':';		// e.g. a : ( feature, feature, feature )
		const char	CHAR_CONSTRUCTOR_START = '(';				// e.g. Prototype( "Name" )
		const char	CHAR_CONSTRUCTOR_END = ')';					// e.g. Prototype( "Name" )

		#endregion

		#region NESTED TYPES

		enum STATE	{
			NAME,						// We have read a name
			VALUE,						// We have read a constructor value
			ALIAS,						// We have read an alias
			COLLECTION,					// We have begun a collection
			CONSTRUCTOR,				// We have begun a constructor
			CHILD,						// We have read a child relationship (either hierarchical or feature)
		}

		[System.Diagnostics.DebuggerDisplay( "{ToString()}" )]
		public class	Name {
			public string[]	m_name = null;

			public bool		IsValid				{ get {return m_name != null && m_name.Length > 0; } }
			public bool		IsFullyQualified	{ get { return m_name != null && m_name.Length > 1; } }
			public int		Length				{ get { return m_name.Length; } }
			public string	this[int _index]	{ get { return m_name[_index]; } }

			public Name( string[] _name ) {
				m_name = _name;
			}
			public Name( string _name ) {
				m_name = new string[] { _name };
			}

			public override string ToString() {
 				return string.Join( ".", m_name );
			}

			/// <summary>
			/// Builds a name
			/// </summary>
			/// <param name="c"></param>
			/// <param name="S"></param>
			/// <returns></returns>
			public static Name	Read( string _fullyQualifiedName ) {
				if ( _fullyQualifiedName == null || _fullyQualifiedName == "" )
					return null;

				TextStream S = new TextStream( _fullyQualifiedName );
				return Read( S.Pop(), S );
			}

			/// <summary>
			/// Reads a name while alphanumeric characters are found
			/// </summary>
			internal static Name	Read( char c, TextStream S ) {
				List<string>	results = new List<string>();

				string	result = "";
				while ( c == CHAR_CHILD_MARKER || c == CHAR_STRING_DELIMITER || c == CHAR_SPECIAL_STRING_DELIMITER_START || c == CHAR_SPECIAL_STRING_DELIMITER_END || IsAlphaNumeric( c ) ) {
					if ( c == CHAR_STRING_DELIMITER ) {
						// Read a quoted-name like "\"example\""
						Name	temp = ReadQuotedName( c, S );
						for ( int i=0; i < temp.m_name.Length-1; i++ )
							results.Add( temp.m_name[i] );
						result = temp.m_name[temp.m_name.Length-1];
					} else  if ( c == CHAR_SPECIAL_STRING_DELIMITER_START ) {
						// Read a special-name like "<example>"
						result = "" + c;
						while ( (c = S.Pop()) != CHAR_SPECIAL_STRING_DELIMITER_END ) {
							result += c;
						}
						result += c;
					} else  if ( c == CHAR_CHILD_MARKER ) {
						// Add a new string
						results.Add( result );
						result = "";
					} else {
						result += c;
					}
					c = S.Pop();
				}
				results.Add( result );
				S.GoBackOne();

				return new Name( results.ToArray() );
			}

			/// <summary>
			/// Reads a quoted name with any character in it, until a '"' or '>' is found
			/// </summary>
			internal static Name	ReadQuotedName( char c, TextStream S ) {
				List<string>	results = new List<string>();

				string	result = "";

				// Read until we reach the closing '"'
				c = S.Pop();
				while ( c != CHAR_STRING_DELIMITER ) {
					if ( c == CHAR_CHILD_MARKER ) {
						// Add a new string
						results.Add( result );
						result = "";
					} else {
						result += c;
					}
					c = S.Pop();
				}

				results.Add( result );

				return new Name( results.ToArray() );
			}
		}

		[System.Diagnostics.DebuggerDisplay( "{ToString()}" )]
		public class	Token {
			public enum TYPE {
				NAME,					// Token is a name
				COLLECTION,				// Token is a collection
				ALIAS,					// Token is an alias
				CONSTRUCTOR,			// Token is a collection of parameters used to construct the prototype
				HIERARCHY_MARKER,		// Token is the parent-child hierarchy marker
				FEATURE_MARKER,			// Token is the parent-child feature marker
			}

			public int		m_sourceStreamPosition = -1;
			public TYPE		m_type;
			public object	m_value;

			public	Token( int _sourceStreamPosition, TYPE _type, object _value ) {
				m_sourceStreamPosition = _sourceStreamPosition;
				m_type = _type;
				m_value = _value;
			}
			public override string ToString() {
				return string.Format( "Type = {0} Value = {1}", m_type, m_value == null ? "null" : ("{" + m_value + "}") );
			}
		}

		[System.Diagnostics.DebuggerDisplay( "{ToString()}" )]
		public class	TokensCollection {

			public Token			m_parent = null;
			public List< Token >	m_tokens = new List<Token>();
			public TokensCollection( Token _parent ) {
				m_parent = _parent;
			}
			public override string ToString() {
				return string.Format( "Size = {0} Last = [{1}]", m_tokens.Count, Last );
			}

			public void	Add( Token T ) {
				m_tokens.Add( T );
			}
			public Token	Last { get { return m_tokens.Count > 0 ? m_tokens[m_tokens.Count-1] : null; } }
		}

		#region Parsers

		internal class TextStream {

			string	m_text = null;
			int		m_length = 0;
			int		m_index = 0;	// Character index

			public bool	EOT		{ get { return m_index >= m_length; } }
			public char	Peek	{ get { return m_text[m_index]; } }

			public int	Index	{ get { return m_index; } }
			public string	Context	{ get {
					int	startIndex = Math.Max( 0, Index - 16 );
					int	endIndex = Math.Min( m_length, Index + 16 );
					return m_text.Substring( startIndex, endIndex - startIndex );
				}
			}

			int[]	m_EOLIndices;
			public Tuple<int,int>	LineColumnIndex { get {
					int	lastIndex = 0;
					int	lineIndex = 0;
					foreach ( int currentIndex in m_EOLIndices ) {
						if ( m_index >= lastIndex && m_index < currentIndex )
							break;
						lastIndex = currentIndex;
						lineIndex++;
					}

					return new Tuple<int, int>( lineIndex, m_index - lastIndex - 1 );
				}
			}

			public TextStream( string _text ) {
				m_text = _text;
				m_length = m_text.Length;

				// Count lines for easy error report
				List<int>	EOLIndices = new List<int>();
				int			EOLindex  = m_text.IndexOf( '\n' );
				while ( EOLindex != -1 ) {
					EOLIndices.Add( EOLindex );
					EOLindex = m_text.IndexOf( '\n', EOLindex+1 );
				}
				m_EOLIndices = EOLIndices.ToArray();
			}

			public char Pop() {
				return m_text[m_index++];
			}

			public void	GoBackOne() { m_index--; }
		}

		/// <summary>
		/// Reads a comment to the end of line
		/// </summary>
		class CommentParser {

			public static string	Read( TextStream S ) {
				string	result = "";

				char	c = S.Pop();
				if ( c == CHAR_COMMENT )
					c = S.Pop();

				// Read until we reach EOL
				while ( c != '\n' ) {
					result += c;
					c = S.Pop();
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region FIELDS

		TextStream					m_stream;
		Stack< STATE >				m_state = new Stack<STATE>();

		Stack< TokensCollection	>	m_tokenCollections = new Stack<TokensCollection	>();

		TokensCollection			m_root;

		#endregion

		#region PROPERTIES

		public TokensCollection		Root { get { return m_root; } }

		#endregion

		#region METHODS

		public Parser( string _text ) {
			m_stream = new TextStream( _text );

			Push( STATE.COLLECTION );
			m_root = PushCollection( null );

			Name	name = null;
			char	c;		// Last read character in the stream

			while ( !m_stream.EOT ) {

				c = m_stream.Pop();	// Get current character

				if ( IsSpaceNoEOL( c ) )
					continue;	// Skip space

				if ( c == CHAR_COMMENT ) {
					// Read till the end of line
					CommentParser.Read( m_stream );
 					if ( m_state.Peek() == STATE.NAME ) {
 						// In NAME state, a new line acts as a collection item separator
 						Pop();
 					}
					continue;
				}

				switch ( m_state.Peek() ) {
					//////////////////////////////////////////////////////////////////////////
					// In NAME state, we expect:
					//	• an alias assignment
					//	• a child assignment
					//	• a feature assignment
					//	• a constructor
					//	• a collection end
					// 
					case STATE.NAME:
						if ( c == CHAR_COLLECTION_ITEM_SEPARATOR || c == '\n' ) {
							// A new line acts as a collection item separator, go back to previous state
							Pop();
							continue;

						} else if ( c == CHAR_ALIAS_ASSIGNMENT ) {
							// Check if it's an assignment or a parent-child definition
							if ( m_stream.Peek == CHAR_HIERARCHY ) {
								// Parent-Child hierarchy marker
								c = m_stream.Pop();	// Consume character
								AddToken( Token.TYPE.HIERARCHY_MARKER, null );
								Pop();
								Push( STATE.CHILD );
								continue;
							}

							// Last parsed name was in fact an alias
							MakeLastTokenA( Token.TYPE.ALIAS );
							Pop();
							Push( STATE.ALIAS );
							continue;

						} else if ( c == CHAR_FEATURES_COLLECTION_MARKER ) {
							// Parent-Child features marker
							AddToken( Token.TYPE.FEATURE_MARKER, null );
							Pop();
							Push( STATE.CHILD );
							continue;

						} else if ( c == CHAR_CONSTRUCTOR_START ) {
							// Entering constructor definition
							Token	T = AddToken( Token.TYPE.CONSTRUCTOR, null );
							T.m_value = PushCollection( T );
							Pop();
							Push( STATE.CONSTRUCTOR );
							continue;

						} else if ( c == CHAR_COLLECTION_END ) {
							// Exiting collection
							Pop();	// Pop name
							Pop();	// Pop collection
							PopCollection();
							Push( STATE.NAME );	// Assume an entire collection is a single name token
							continue;

						}

						SyntaxError( m_stream, c, "a name" );

						break;

					//////////////////////////////////////////////////////////////////////////
					// In VALUE state, we expect:
					//	• A value separator
					//	• The end of the constructor
					//
					case STATE.VALUE:
						if ( c == CHAR_COLLECTION_ITEM_SEPARATOR || c == '\n' ) {
							// A new line acts as a separator, go back to previous state
							Pop();
							continue;

						} else if ( c == CHAR_CONSTRUCTOR_END ) {
							// Exit the constructor
							Pop();	// Pop value
							Pop();	// Pop constructor
							PopCollection();
							Push( STATE.NAME );	// Assume a constructed name is a single name token
							continue;
						}

						SyntaxError( m_stream, c, "a constructor value" );

						break;

					//////////////////////////////////////////////////////////////////////////
					// In collection state, we expect:
					//	• a name
					//	• a collection start
					//	• a collection end
					//	• a separator
					// 
					case STATE.COLLECTION:
						if ( c == CHAR_COLLECTION_ITEM_SEPARATOR || c == '\n' ) {
							continue;
						}
						if ( c == CHAR_COLLECTION_START ) {
							// Start a new collection
							Token	T = AddToken( Token.TYPE.COLLECTION, null );
							T.m_value = PushCollection( T );
							Push( STATE.COLLECTION );
							continue;

						} else if ( c == CHAR_COLLECTION_END ) {
							// Exit the collection
							Pop();	// Pop collection
							PopCollection();
							Push( STATE.NAME );	// Assume an entire collection is a single name token
							continue;

						} else if ( ReadName( c, m_stream, ref name ) ) {
							// We just read a name
							AddToken( Token.TYPE.NAME, name );
							Push( STATE.NAME );
							continue;

						}

						SyntaxError( m_stream, c, "collection" );

						break;

					//////////////////////////////////////////////////////////////////////////
					// In CONSTRUCTOR state, we expect:
					//	• A value
					//	• An alias name
					//	• The end of the constructor
					//
					case STATE.CONSTRUCTOR:
						if ( c == '\n' ) {
							continue;
						}

						if ( c == CHAR_CONSTRUCTOR_END ) {
							// Exit the constructor
							Pop();	// Pop constructor
							PopCollection();
							Push( STATE.NAME );	// Assume a constructed name is a single name token
							continue;

						} else if ( ReadName( c, m_stream, ref name ) ) {
							// We just read a name and it MUST be an alias!
							AddToken( Token.TYPE.NAME, name );
							Push( STATE.NAME );
							continue;

						}

						SyntaxError( m_stream, c, "a constructor" );

						break;

					//////////////////////////////////////////////////////////////////////////
					// In CHILD state, we expect:
					//	• a new NAME
					//	• a new collection
					//
					case STATE.CHILD:
						if ( c == '\n' ) {
							continue;
						}

						if ( c == CHAR_COLLECTION_START ) {
							// Start a new collection
							Pop();

							Token	T = AddToken( Token.TYPE.COLLECTION, null );
							T.m_value = PushCollection( T );
							Push( STATE.COLLECTION );
							continue;

						} else if ( ReadName( c, m_stream, ref name ) ) {
							// We just read a name
							Pop();

							AddToken( Token.TYPE.NAME, name );
							Push( STATE.NAME );
							continue;
						}

						SyntaxError( m_stream, c, "child" );

						break;

					//////////////////////////////////////////////////////////////////////////
					// In ALIAS state, we expect:
					//	• a new NAME
					//
					case STATE.ALIAS:
						if ( c == '\n' ) {
							continue;
						}

						if ( ReadName( c, m_stream, ref name ) ) {
							// We just read a name
							Pop();
							AddToken( Token.TYPE.NAME, name );
							Push( STATE.NAME );
							continue;
						}

						SyntaxError( m_stream, c, "an alias" );

						break;
				}
			}

			// Verify we're back to the root collection
			if ( m_state.Count != 1 ) {
				throw new Exception( "Leaving parsing with " + (m_state.Count-1) + " states remaining, not at root level!" );
			}
			if ( m_state.Peek() != STATE.COLLECTION )
				throw new Exception( "Leaving parsing with a single state that is not the root COLLECTION!" );
		}

		#region Helpers

		/// <summary>
		/// Pushes a new state
		/// </summary>
		/// <param name="_state"></param>
		void	Push( STATE _state ) {
			m_state.Push( _state );
		}

		/// <summary>
		/// Pops the current state from the stack
		/// </summary>
		void	Pop() {
			m_state.Pop();
		}

		/// <summary>
		/// Pushes a new tokens collection
		/// </summary>
		/// <param name="_parent">The parent token hosting the collection</param>
		TokensCollection	PushCollection( Token _parent ) {
			TokensCollection	newCollection = new TokensCollection( _parent );
			m_tokenCollections.Push( newCollection );
			return newCollection;
		}

		/// <summary>
		/// Pops the current collection from the stack
		/// </summary>
		void	PopCollection() {
			if ( m_tokenCollections.Count == 0 )
				ThrowError( m_stream, "Unexpected end of collection character '" + CHAR_COLLECTION_END + "' while parsing" );

			m_tokenCollections.Pop();
		}

		/// <summary>
		/// Adds a new token to the current collection
		/// </summary>
		/// <param name="T"></param>
		Token	AddToken( Token.TYPE _type, object _value ) {
			TokensCollection	tokens = m_tokenCollections.Peek();
			Token	T = new Token( m_stream.Index, _type, _value );
			tokens.Add( T );
			return T;
		}

		/// <summary>
		/// Retrieves the last token, that must be a name, and make it an alias instead
		/// </summary>
		void	MakeLastTokenA( Token.TYPE _newType ) {
			TokensCollection	tokens = m_tokenCollections.Peek();
			Token				T = tokens.Last;
			if ( T.m_type != Token.TYPE.NAME )
				throw new Exception( "Last token is not a name! Can't convert into an alias!" );
			T.m_type = _newType;
		}

		static void	SyntaxError( TextStream _stream, char c, string _message ) {
			string	C = "" + c;
			if ( c == '\r' )
				C = "\r";
			else if ( c == '\n' )
				C = "\n";

			ThrowError( _stream, "Unexpected character '" + C + "' while parsing " + _message );
		}
		static void	ThrowError( TextStream _stream, string _message ) {
			string	context = _stream.Context;
					context = context.Replace( "\r", " " );
					context = context.Replace( "\n", " " );
					context = context.Replace( "\t", " " );

			Tuple<int,int>	lineColumnIndex = _stream.LineColumnIndex;

			throw new Exception( _message + " at line " + (1+lineColumnIndex.Item1) + ", column " + lineColumnIndex.Item2 + "\n" + context + "\n               ^" );
		}

		static bool ReadName( char c, TextStream S, ref Name _name ) {
			if ( c == CHAR_STRING_DELIMITER || c == CHAR_SPECIAL_STRING_DELIMITER_START || c == CHAR_CHILD_MARKER || IsAlphaNumeric( c ) ) {
				_name = Name.Read( c, S );
			} else {
				_name = null;
			}

// For debugging purpose
// if ( _name != null && _name.Length > 1 && _name[1] == "Chélicère" )
// 	return true;

			return _name != null;
		}

// 		// At the moment, same conditions as names
// 		static bool ReadValue( char c, TextStream S, ref Name _value ) {
// 			if ( c != CHAR_STRING_DELIMITER )
// 				return false;
// 
// 			c = S.Pop();
// 			string	value = "";
// 			while ( c != CHAR_STRING_DELIMITER ) {
// 				value += c;
// 				c = S.Pop();
// 			}
// 
// 			_value = new Name( value );
// 			return true;
// 		}

		static bool	IsAlphaNumeric( char c ) {
			if ( c >= 'a' && c <= 'z' ) return true;
			if ( c >= 'A' && c <= 'Z' ) return true;
			if ( c >= '0' && c <= '9' ) return true;
			if ( c == '_' ) return true;
//			if ( c == 'é' || c == 'è' || c == 'ô' || c == 'à' || c == 'ï' || c == 'î' || c == 'ç' ) return true;
			return false;
		}

		static bool	IsSpace( char c ) {
			if ( IsSpaceNoEOL( c )  ) return true;
			if ( c == '\n' ) return true;
			return false;
		}

 		static bool	IsSpaceNoEOL( char c ) {
 			if ( c == ' '  ) return true;
 			if ( c == '\t' ) return true;
 			if ( c == '\r' ) return true;
 			return false;
 		}

		#endregion

		#endregion
	}
}
