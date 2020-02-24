using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;


namespace WebServices {

	/// <summary>
	/// Super simple JSON parser
	/// </summary>
	public class JSON {

		#region METHODS

		public enum JSONState {
			ROOT,
			DICTIONARY_EXPECT_KEY,
			DICTIONARY_EXPECT_VALUE,
			DICTIONARY_EXPECT_KEY_SEPARATOR,			// Read key, expecting separator
			DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END,	// Read value, expecting separator or collection end
			ARRAY_EXPECT_ELEMENT,
			ARRAY_EXPECT_SEPARATOR_OR_END,
		}

		[System.Diagnostics.DebuggerDisplay( "{m_state} Object={m_object}" )]
		public class JSONObject {
			public JSONObject	m_previous = null;
			public JSONState	m_state;
			public object		m_object = null;
			public string		m_currentKey = null;	// Last parsed key when parsing collections
			public JSONObject( JSONObject _previous, JSONState _newState, object _object ) {
				m_previous = _previous;
				m_state = _newState;
				m_object = _object;
			}

			public Dictionary<string,object>	AsDictionary { get {return m_object as Dictionary<string,object>; } }
			public bool							IsDictionary { get { return AsDictionary != null; } }
			public JSONObject					this[string _dictionaryKey] { get { return AsDictionary[_dictionaryKey] as JSONObject; } }

			public List<object>					AsArray { get {return m_object as List<object>; } }
			public bool							IsArray { get { return AsArray != null; } }
			public JSONObject					this[int _index] { get { return AsArray[_index] as JSONObject; } }

			public double						AsDouble { get {return (double) m_object; } }
			public bool							IsDouble { get { return m_object is double; } }
		}

		/// <summary>
		/// Reads a JSON text from a stream and transforms it into a JSON object
		/// </summary>
		/// <param name="_reader"></param>
		/// <returns></returns>
		public JSONObject	ReadJSON( TextReader _reader ) {
			JSONObject	root = new JSONObject( null, JSONState.DICTIONARY_EXPECT_VALUE, new Dictionary<string,object>() );
						root.m_currentKey = "root";

// For debugging purpose => writes the stream read so far into a string so we can see where it crashed. Set to null if you don't want to debug...
//StringBuilder	sb = new StringBuilder( (int) _reader.BaseStream.Length );
StringBuilder	sb = new StringBuilder();
//StringBuilder	sb = null;

			JSONObject	current = root;
			bool		needToExit = false;
			while ( !needToExit ) {
				char	C = (char) _reader.Read();
				if ( sb != null ) sb.Append( C );

				switch ( C ) {
					case '{': {
						// Enter a new dictionary
						JSONObject	v = new JSONObject( current, JSONState.DICTIONARY_EXPECT_KEY, new Dictionary<string,object>() );

						switch ( current.m_state ) {
							case JSONState.DICTIONARY_EXPECT_VALUE:
								current.AsDictionary.Add( current.m_currentKey, v );
								current.m_state = JSONState.DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END;
								current = v;
								break;

							case JSONState.ARRAY_EXPECT_ELEMENT:
								current.AsArray.Add( v );
								current.m_state = JSONState.ARRAY_EXPECT_SEPARATOR_OR_END;
								current = v;
								break;

							default:
								throw new Exception( "Encountered dictionary while not expecting a value!" );
						}
						break;
					}

					case '}': {
						if ( current.m_state != JSONState.DICTIONARY_EXPECT_KEY && current.m_state != JSONState.DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END )
							throw new Exception( "Exiting a collection early!" );
						current = current.m_previous;	// Restore previous object
						break;
					}

					case '[': {
						if ( current.m_state != JSONState.DICTIONARY_EXPECT_VALUE )
							throw new Exception( "Encountered array while not expecting a value!" );

						// Enter a new array
						current = new JSONObject( current, JSONState.ARRAY_EXPECT_ELEMENT, new List<object>() );
						break;
					}

					case ']': {
						if ( current.m_state != JSONState.ARRAY_EXPECT_ELEMENT && current.m_state != JSONState.ARRAY_EXPECT_SEPARATOR_OR_END )
							throw new Exception( "Exiting an array early!!" );

						JSONObject	value = current;

						// Restore previous object
						current = current.m_previous;
						if ( current.m_state != JSONState.DICTIONARY_EXPECT_VALUE )
							throw new Exception( "Finished parsing an array that is not an expected value!" );

						// Just parsed an array value
						current.AsDictionary.Add( current.m_currentKey, value );
						current.m_state = JSONState.DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END;
						current.m_currentKey = null;
						break;
					}

					case ' ':
					case '\t':
					case '\r':
					case '\n':
						// Just skip...
						break;

					case ':':
						if ( current.m_state != JSONState.DICTIONARY_EXPECT_KEY_SEPARATOR )
							throw new Exception( "Encountered separator not in dictionary!" );

						// Now expecting a value!
						current.m_state = JSONState.DICTIONARY_EXPECT_VALUE;
						break;

					case ',':
						switch ( current.m_state ) {
							case JSONState.DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END:
								// Now expecting a key again!
								current.m_state = JSONState.DICTIONARY_EXPECT_KEY;
								break;

							case JSONState.ARRAY_EXPECT_SEPARATOR_OR_END:
								// Now expecting an element again!
								current.m_state = JSONState.ARRAY_EXPECT_ELEMENT;
								break;

							default:
								throw new Exception( "Encountered separator not in dictionary!" );
						}
						break;

					case '"': {
						if ( current.m_state != JSONState.DICTIONARY_EXPECT_KEY && current.m_state != JSONState.DICTIONARY_EXPECT_VALUE )
							throw new Exception( "Encountered string not in dictionary key or value state!" );

						string	s = ReadString( C, _reader, sb );
						if ( current.m_state == JSONState.DICTIONARY_EXPECT_KEY ) {
							// Just parsed a key
							current.m_currentKey = s;
							current.m_state = JSONState.DICTIONARY_EXPECT_KEY_SEPARATOR;

							// Handle special case of end data we don't want to parse...
							if ( s == "sync_metadata" ) {
								needToExit = true;	// Early exit!
							}

						} else if ( current.m_state == JSONState.DICTIONARY_EXPECT_VALUE ) {
							// Just parsed a value
							current.AsDictionary.Add( current.m_currentKey, s );
							current.m_state = JSONState.DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END;
							current.m_currentKey = null;
						}
						break;
					}

				default:
					if ( IsNumberChar( C ) ) {
						// We got some double...
						if ( current.m_state != JSONState.DICTIONARY_EXPECT_VALUE )// && current.m_state != JSONState.DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END )
							throw new Exception( "Encountered number not in dictionary value state!" );

						double	value = ReadDouble( C, _reader, sb );

						current.AsDictionary.Add( current.m_currentKey, value );
						current.m_state = JSONState.DICTIONARY_EXPECT_VALUE_SEPARATOR_OR_END;
						current.m_currentKey = null;
						break;
					}

					if ( C == '\uffff' ) {
						needToExit = true;
						break;
					}

					throw new Exception( "Unexpected character '" + C + "'!" );
				}
			}

			return root;
		}

		#region Readers for Strings & Numbers

		StringBuilder	m_stringBuilder = new StringBuilder();
		string	ReadString( char _firstChar, TextReader _reader, StringBuilder _sb ) {
			m_stringBuilder.Clear();

			char	C = (char) _reader.Read();
			while ( C != '"' ) {

				if ( C == '\\' ) {
					// Read escaped character
					char	escaped = (char) _reader.Read();
					switch( escaped ) {
						case '\\': C = '\\'; break;
						case 'r': C = '\r'; break;
						case 'n': C = '\n'; break;
						case '"': C = '"'; break;
						case 't': C = '\t'; break;
						case 'u': {
							string	strUnicode = "" + (char) _reader.Read() + (char) _reader.Read() + (char) _reader.Read() + (char) _reader.Read();
							int		unicode = 0;
							if ( int.TryParse( strUnicode, System.Globalization.NumberStyles.HexNumber, Application.CurrentCulture, out unicode ) ) {
								C = (char) unicode;
							}
							break;
						}

						default: throw new Exception( "Unrecognized escaped character \"" + C + escaped + "\"!" );
					}
				}

				m_stringBuilder.Append( C );
				if ( _sb != null ) _sb.Append( C );

				C = (char) _reader.Read();
			}

			return m_stringBuilder.ToString();
		}

		double	ReadDouble( char C, TextReader _reader, StringBuilder _sb ) {
			m_stringBuilder.Clear();
			m_stringBuilder.Append( C );

			// Read the number
			while ( IsNumberChar( (char) _reader.Peek() ) ) {
				C = (char) _reader.Read();
				m_stringBuilder.Append( C );
				if ( _sb != null ) _sb.Append( C );
			}
			string	stringDouble = m_stringBuilder.ToString();

			return double.Parse( stringDouble );
		}

		bool	IsNumberChar( char C ) {
			switch ( C ) {
				case '-':
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					return true;
			}

			return false;
		}

		#endregion

		#endregion
	}
}
