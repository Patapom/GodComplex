//////////////////////////////////////////////////////////////////////////
/// This is an adaptation of the iOS LaTeX library by Kostub Deshmukh
/// Initial GitHub project: https://github.com/kostub/iosMath
///
//////////////////////////////////////////////////////////////////////////
///
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTeX
{
	/// <summary>
	/// Converts a LaTeX-formatted string into a list of atoms used internally
	/// Equivalent of the original MTMathListBuilder class in the original library
	/// </summary>
    public class LaTeXStringToAtomsList {

		#region NESTED TYPES

		/// <summary>
		/// The error encountered when parsing a LaTeX string.
		/// The `code` in the `NSError` is one of the following indiciating why the LaTeX string could not be parsed.
		/// </summary>
		public class ParseError {
			public enum ErrorType {
				/// The braces { } do not match.
				MTParseErrorMismatchBraces = 1,
				/// A command in the string is not recognized.
				MTParseErrorInvalidCommand,
				/// An expected character such as ] was not found.
				MTParseErrorCharacterNotFound,
				/// The \left or \right command was not followed by a delimiter.
				MTParseErrorMissingDelimiter,
				/// The delimiter following \left or \right was not a valid delimiter.
				MTParseErrorInvalidDelimiter,
				/// There is no \right corresponding to the \left command.
				MTParseErrorMissingRight,
				/// There is no \left corresponding to the \right command.
				MTParseErrorMissingLeft,
				/// The environment given to the \begin command is not recognized
				MTParseErrorInvalidEnv,
				/// A command is used which is only valid inside a \begin,\end environment
				MTParseErrorMissingEnv,
				/// There is no \begin corresponding to the \end command.
				MTParseErrorMissingBegin,
				/// There is no \end corresponding to the \begin command.
				MTParseErrorMissingEnd,
				/// The number of columns do not match the environment
				MTParseErrorInvalidNumColumns,
				/// Internal error, due to a programming mistake.
				MTParseErrorInternalError,
				/// Limit control applied incorrectly
				MTParseErrorInvalidLimits,
			}

			public ErrorType	error;
			public string		message;
		}

		class	MTEnvProperties {

			public string	envName;
			public bool		ended = false;
			public uint		numRows = 0;

			public MTEnvProperties( string _name ) {
				envName = _name;
			}
		}

		#endregion

		#region FIELDS

		string			_chars;
		int				_currentChar;
		uint			_length = 0;
		AtomInner		_currentInnerAtom = null;

		MTEnvProperties	_currentEnv;
		Atom.FontStyle	_currentFontStyle = Atom.FontStyle.kMTFontStyleDefault;
		bool			_spacesAllowed = false;

		ParseError		_error = null;		// Contains any error that occurred during parsing.

		#endregion

		#region PROPERTIES

		bool	HasCharacters { get { return _currentChar < _length; } }

		#endregion

		#region METHODS

		/// <summary>
		/// Create a `MTMathListBuilder` for the given string. After instantiating the `MTMathListBuilder, use `build` to build the mathlist. Create a new `MTMathListBuilder` for each string that needs to be parsed. Do not reuse the object.
		/// </summary>
		/// <param name="_LaTeX"></param>
		public LaTeXStringToAtomsList( string _LaTeX ) {

		}

		/// <summary>
		/// Builds a mathlist from the given string. Returns null if there is an error.
		/// </summary>
		/// <returns></returns>
		public AtomsList	Build() {
			AtomsList	list = BuildInternal( false );
			if ( HasCharacters && _error != null ) {
				// something went wrong most likely braces mismatched
				SetError( ParseError.ErrorType.MTParseErrorMismatchBraces );
				throw new Exception( "Mismatched braces: " + _chars );
			}
			if ( _error != null ) {
				return null;
			}
    
			return list;
		}

		AtomsList	BuildInternal( bool _oneCharOnly ) {
			return BuildInternal( _oneCharOnly, (char) 0 );
		}


		AtomsList	BuildInternal( bool oneCharOnly, char stop ) {
			if ( oneCharOnly && stop > 0 )
				throw new Exception( @"Cannot set both oneCharOnly and stopChar." );

			AtomsList	list = new AtomsList();
			Atom		prevAtom = null;
			while ( HasCharacters ) {
				if ( _error != null ) {
					return null;	// If there is an error thus far then bail out.
				}
				Atom	atom = null;
				char	ch = GetNextCharacter();
				if ( oneCharOnly ) {
					if ( ch == '^' || ch == '}' || ch == '_' || ch == '&' ) {
						// this is not the character we are looking for.
						// They are meant for the caller to look at.
						UnlookCharacter();
						return list;
					}
				}
				// If there is a stop character, keep scanning till we find it
				if ( stop > 0 && ch == stop ) {
					return list;
				}

				if ( ch == '^' ) {
					if ( oneCharOnly ) throw new Exception( @"This should have been handled before" );
					if ( prevAtom != null || prevAtom.SuperScript != null || !prevAtom.ScriptsAllowed ) {
						// If there is no previous atom, or if it already has a superscript or if scripts are not allowed for it, then add an empty node.
						prevAtom = new Atom( Atom.TYPE.kMTMathAtomOrdinary );
						list.AddAtom( prevAtom );
					}
					// this is a superscript for the previous atom
					// note: if the next char is the stopChar it will be consumed by the ^ and so it doesn't count as stop
					prevAtom.SuperScript = BuildInternal( true );
					continue;

				} else if ( ch == '_' ) {
					if ( oneCharOnly ) throw new Exception( @"This should have been handled before" );
					if ( prevAtom != null || prevAtom.SubScript != null || !prevAtom.ScriptsAllowed ) {
						// If there is no previous atom, or if it already has a subcript or if scripts are not allowed for it, then add an empty node.
						prevAtom = new Atom( Atom.TYPE.kMTMathAtomOrdinary );
						list.AddAtom( prevAtom );
					}
					// this is a subscript for the previous atom
					// note: if the next char is the stopChar it will be consumed by the _ and so it doesn't count as stop
					prevAtom.SubScript = BuildInternal( true );
					continue;

				} else if ( ch == '{' ) {
					// this puts us in a recursive routine, and sets oneCharOnly to false and no stop character
					AtomsList	sublist = BuildInternal( false, '}' );
					prevAtom = sublist.Last;
					list.Append( sublist );
					if ( oneCharOnly ) {
						return list;
					}
					continue;

				} else if ( ch == '}' ) {
					if ( oneCharOnly ) throw new Exception( @"This should have been handled before" );
					if ( stop != 0 ) throw new Exception( @"This should have been handled before" );
					// We encountered a closing brace when there is no stop set, that means there was no corresponding opening brace.
					SetError( ParseError.ErrorType.MTParseErrorMismatchBraces, @"Mismatched braces." );
					return null;

				} else if ( ch == '\\' ) {
					// \ means a command
					string		command = ReadCommand();
					AtomsList	done = StopCommand( command, list, stop );
					if ( done != null ) {
						return done;
					} else if ( _error != null ) {
						return null;
					}
					if ( ApplyModifier( command, prevAtom )) {
						continue;
					}

					Atom.FontStyle	fontStyle = AtomFactory.FontStyleWithName( command );
					if ( fontStyle != Atom.FontStyle.NOT_FOUND ) {
						bool			oldSpacesAllowed = _spacesAllowed;
						Atom.FontStyle	oldFontStyle = _currentFontStyle;

						// Text has special consideration where it allows spaces without escaping.
						_spacesAllowed = command == @"text";
						_currentFontStyle = fontStyle;
						AtomsList	sublist = BuildInternal( true );

						// Restore the font style.
						_currentFontStyle = oldFontStyle;
						_spacesAllowed = oldSpacesAllowed;

						prevAtom = sublist.Last;
						list.Append( sublist );
						if ( oneCharOnly ) {
							return list;
						}
						continue;
					}

					atom = AtomForCommand( command );
					if ( atom == null ) {
						// this was an unknown command, we flag an error and return
						// (note setError will not set the error if there is already one, so we flag internal error in the odd case that an _error is not set).
						SetError( ParseError.ErrorType.MTParseErrorInternalError, @"Internal error" );
						return null;
					}

				} else if ( ch == '&' ) {
					// used for column separation in tables
					if ( oneCharOnly ) throw new Exception( @"This should have been handled before" );
					if ( _currentEnv != null ) {
						return list;
					} else {
						// Create a new table with the current list and a default env
						Atom	table = BuildTable( null, list, false );
						return new AtomsList( table, null );
					}
				} else if ( _spacesAllowed && ch == ' ' ) {
					// If spaces are allowed then spaces do not need escaping with a \ before being used.
					atom = AtomFactory.AtomForLatexSymbolName( @" " );
				} else {
					atom = AtomFactory.AtomForCharacter( ch );
					if ( atom == null ) {
						continue;	// Not a recognized character
					}
				}

				if ( atom == null) throw new Exception( @"Atom shouldn't be null" );

				atom.fontStyle = _currentFontStyle;
				list.AddAtom( atom );
				prevAtom = atom;
        
				if ( oneCharOnly ) {
					return list;	// we consumed our onechar
				}
			}

			if ( stop > 0 ) {
				if ( stop == '}' ) {
					SetError( ParseError.ErrorType.MTParseErrorMismatchBraces, @"Missing closing brace" );	// We did not find a corresponding closing brace.
				} else {
					SetError( ParseError.ErrorType.MTParseErrorCharacterNotFound, @"Expected character not found:" + stop );	// we never found our stop character
				}
			}

			return list;
		}

		/// <summary>
		/// Construct a math list from a given string. If there is parse error, returns null. To retrieve the error use the function Error
		/// </summary>
		/// <param name="_LaTeX"></param>
		/// <returns></returns>
		public static AtomsList	BuildFromString( string _LaTeX ) {
			LaTeXStringToAtomsList	builder = new LaTeXStringToAtomsList( _LaTeX );
			return builder.Build();
		}

		/// <summary>
		/// Construct a math list from a given string. If there is parse error, returns null. The error is returned in the `error` parameter.
		/// </summary>
		/// <param name="_LaTeX"></param>
		/// <param name="_error"></param>
		/// <returns></returns>
		public static AtomsList	BuildFromString( string _LaTeX, out ParseError _error ) {
			LaTeXStringToAtomsList	builder = new LaTeXStringToAtomsList( _LaTeX );
			AtomsList	output = builder.Build();
			_error = builder._error;
			return output;
		}

// 		/// <summary>
// 		/// This converts the list of atoms to LaTeX.
// 		/// </summary>
// 		/// <param name="_list"></param>
// 		/// <returns></returns>
// 		public string		AtomsListToLaTeX( AtomsList _list ) {
// 
// 		}

		// 
		// @param str The LaTeX string to be used to build the `AtomsList`
		// 
		// - (nonnull instancetype) initWithString:(nonnull string) str NS_DESIGNATED_INITIALIZER;
		// - (nonnull instancetype) init NS_UNAVAILABLE;
		// 
		// 
		// - (nullable AtomsList) build;
		// 
		// /** Construct a math list from a given string. If there is parse error, returns
		//  null. To retrieve the error use the function `[MTMathListBuilder buildFromString:error:]`.
		//  */
		// + (nullable AtomsList) buildFromString:(nonnull string) str;
		// 
		// /** Construct a math list from a given string. If there is an error while constructing the string, this returns null. The error is returned in the `error` parameter.
		//  */
		// + (nullable AtomsList) buildFromString:(nonnull string) str error:( NSError* _Nullable * _Nullable) error;
		// 
		// 
		// + (nonnull string) mathListToString:(nonnull AtomsList) ml;

		// gets the next character and moves the pointer ahead
		char	GetNextCharacter() {
			return _chars[_currentChar++];
		}

		void	UnlookCharacter() {
			if ( _currentChar == 0 )
				throw new Exception( "Already at start of stream!" );
			_currentChar--;
		}


		string	ReadString() {
			// a string of all upper and lower case characters.
			string	result = "";
			while ( HasCharacters ) {
				char	ch = GetNextCharacter();
				if ( (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') ) {
					result += ch;;
				} else {
					// we went too far
					UnlookCharacter();
					break;
				}
			}
			return result;
		}

		string	ReadColor() {
			if ( !ExpectCharacter( '{' ) ) {
				// We didn't find an opening brace, so no env found.
				SetError( ParseError.ErrorType.MTParseErrorCharacterNotFound, @"Missing {" );
				return null;
			}
    
			// Ignore spaces and nonascii.
			SkipSpaces();

			// a string of all upper and lower case characters.
			string	result = "";
			while ( HasCharacters ) {
				char	ch = GetNextCharacter();
				if ( ch == '#' || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f') || (ch >= '0' && ch <= '9') ) {
					result += ch;
				} else {
					// we went too far
					UnlookCharacter();
					break;
				}
			}

			if ( !ExpectCharacter( '}' ) ) {
				// We didn't find an closing brace, so invalid format.
				SetError( ParseError.ErrorType.MTParseErrorCharacterNotFound, @"Missing }" );
				return null;
			}
			return result;
		}

		void	SkipSpaces() {
			while ( HasCharacters ) {
				char ch = GetNextCharacter();
				if ( ch < 0x21 || ch > 0x7E ) {
					continue;	// skip non ascii characters and spaces
				} else {
					UnlookCharacter();
					return;
				}
			}
		}

		void	MTAssert( bool _condition, string _message ) {
			if ( !_condition ) throw new Exception( _message );
		}
		void	MTAssert( bool _condition ) {
			MTAssert( _condition, "ASSERT!" );
		}
		void	MTAssertNotSpace( char _ch ) {
			MTAssert( _ch >= 0x21 && _ch <= 0x7E, @"Expected non space character '" + _ch + "'" );
		}

		bool	ExpectCharacter( char ch ) {
			MTAssertNotSpace( ch );
			SkipSpaces();
    
			if ( HasCharacters ) {
				char c = GetNextCharacter();
				MTAssertNotSpace(c);
				if (c == ch) {
					return true;
				} else {
					UnlookCharacter();
					return false;
				}
			}
			return false;
		}

		static HashSet< char >	singleCharCommands = null;
		public string	ReadCommand() {

			if ( singleCharCommands == null ) {
				singleCharCommands = new HashSet<char>( new char[] { '{', '}', '$', '#', '%', '_', '|', ' ', ',', '>', ';', '!', '\\' } );
			}
			if ( HasCharacters ) {
				// Check if we have a single character command.
				char ch = GetNextCharacter();

				// Single char commands
				if ( singleCharCommands.Contains( ch ) ) {
					return ch.ToString();
				} else {
					UnlookCharacter();	// not a known single character command
				}
			}

			// otherwise a command is a string of all upper and lower case characters.
			return ReadString();
		}

		string	 ReadDelimiter() {
			// Ignore spaces and nonascii.
			SkipSpaces();
			while ( HasCharacters ) {
				char ch = GetNextCharacter();
				MTAssertNotSpace(ch);
				if ( ch == '\\' ) {
					// \ means a command
					string	command = ReadCommand();
					if ( command == @"|" ) {
						return @"||";	// | is a command and also a regular delimiter. We use the || command to distinguish between the 2 cases for the caller.
					}
					return command;
				} else {
					return ch.ToString();
				}
			}

			// We ran out of characters for delimiter
			return null;
		}

		string	ReadEnvironment() {
			if ( !ExpectCharacter( '{' ) ) {
				// We didn't find an opening brace, so no env found.
				SetError( ParseError.ErrorType.MTParseErrorCharacterNotFound, @"Missing {" );
				return null;
			}
    
			// Ignore spaces and nonascii.
			SkipSpaces();
			string	env = ReadString();
    
			if (!ExpectCharacter( '}' )) {
				// We didn't find an closing brace, so invalid format.
				SetError( ParseError.ErrorType.MTParseErrorCharacterNotFound, @"Missing }" );
				return null;
			}
			return env;
		}

		Atom	GetBoundaryAtom( string delimiterType ) {
			string delim = ReadDelimiter();
			if ( delim == null ) {
				SetError( ParseError.ErrorType.MTParseErrorMissingDelimiter, @"Missing delimiter for \\" + delimiterType );
				return null;
			}
			Atom	boundary = AtomFactory.boundaryAtomForDelimiterName( delim );
			if ( boundary == null ) {
				SetError( ParseError.ErrorType.MTParseErrorInvalidDelimiter, @"Invalid delimiter for \\" + delimiterType + ": " + delim );
				return null;
			}
			return boundary;
		}

		Atom	AtomForCommand( string command ) {
			Atom	atom = AtomFactory.AtomForLatexSymbolName( command );
			if ( atom != null ) {
				return atom;
			}

			AtomAccent	accent = AtomFactory.AccentWithName( command );
			if ( accent != null ) {
				// The command is an accent
				accent.innerList = BuildInternal( true );
				return accent;

			} else if ( command == @"frac" ) {
				// A fraction command has 2 arguments
				AtomFraction	frac = new AtomFraction( true );
				frac.numerator = BuildInternal( true );
				frac.denominator = BuildInternal( true );
				return frac;

			} else if ( command == @"binom" ) {
				// A binom command has 2 arguments
				AtomFraction	frac = new AtomFraction( false );
				frac.numerator = BuildInternal( true );
				frac.denominator = BuildInternal( true );
				frac.leftDelimiter = @"(";
				frac.rightDelimiter = @")";
				return frac;

			} else if (command == @"sqrt") {
				// A sqrt command with one argument
				AtomRadical	rad = new AtomRadical();
				char ch = GetNextCharacter();
				if (ch == '[') {
					// special handling for sqrt[degree]{radicand}
					rad.degree = BuildInternal( false, ']' );
					rad.radicand = BuildInternal( true );
				} else {
					UnlookCharacter();
					rad.radicand = BuildInternal( true );
				}
				return rad;

			} else if ( command == @"left" ) {
				// Save the current inner while a new one gets built.
				AtomInner	oldInner = _currentInnerAtom;
				_currentInnerAtom = new AtomInner();
				_currentInnerAtom.LeftBoundary = GetBoundaryAtom( @"left" );
				if ( _currentInnerAtom.LeftBoundary != null ) {
					return null;
				}
				_currentInnerAtom.innerList = BuildInternal( false );
				if ( _currentInnerAtom.RightBoundary == null ) {
					// A right node would have set the right boundary so we must be missing the right node.
					SetError( ParseError.ErrorType.MTParseErrorMissingRight, @"Missing \\right" );
					return null;
				}
				// reinstate the old inner atom.
				AtomInner	newInner = _currentInnerAtom;
				_currentInnerAtom = oldInner;
				return newInner;

			} else if ( command == @"overline" ) {
				// The overline command has 1 arguments
				AtomOverLine	over = new AtomOverLine();
				over.innerList = BuildInternal( true );
				return over;

			} else if (command == @"underline" ) {
				// The underline command has 1 arguments
				AtomUnderLine	under = new AtomUnderLine();
				under.innerList = BuildInternal( true );
				return under;

			} else if ( command == @"begin" ) {
				string	env = ReadEnvironment();
				if ( env == null ) {
					return null;
				}
				Atom	table = BuildTable( env, firstList:null, row:false );
				return table;

			} else if ( command == @"color" ) {
				// A color command has 2 arguments
				AtomColor	mathColor = new AtomColor();
				mathColor.colorString = ReadColor();
				mathColor.innerList = BuildInternal( true );
				return mathColor;

			} else if (command == @"colorbox" ) {
				// A color command has 2 arguments
				AtomColorBox	mathColorbox = new AtomColorBox();
				mathColorbox.colorString = ReadColor();
				mathColorbox.innerList = BuildInternal( true );
				return mathColorbox;

			} else {
				SetError( ParseError.ErrorType.MTParseErrorInvalidCommand, @"Invalid command \\" + command );
				return null;
			}
		}

		static Dictionary< string, string[] >	fractionCommands = null;
		AtomsList	StopCommand( string command, AtomsList list, char stopChar ) {
			if ( fractionCommands == null ) {
				fractionCommands = new Dictionary<string, string[]>();
				fractionCommands.Add( @"over",		new string[] {} );
				fractionCommands.Add( @"atop",		new string[] {} );
				fractionCommands.Add( @"choose",	new string[] { @"(", @")" } );
				fractionCommands.Add( @"brack",		new string[] { @"[", @"]" } );
				fractionCommands.Add( @"brace",		new string[] { @"{", @"}" } );
			}
			if ( command == @"right" ) {
				if ( _currentInnerAtom == null ) {
					SetError( ParseError.ErrorType.MTParseErrorMissingLeft, @"Missing \\left" );
					return null;
				}
				_currentInnerAtom.RightBoundary = GetBoundaryAtom( @"right" );
				if ( _currentInnerAtom.RightBoundary == null ) {
					return null;
				}
				// return the list read so far.
				return list;

			} else if ( fractionCommands.ContainsKey( command ) ) {
				AtomFraction	frac = null;
				if ( command == @"over" ) {
					frac = new AtomFraction( true );
				} else {
					frac = new AtomFraction( false );
				}
				string[]	delims = fractionCommands[command];
				if ( delims.Length == 2 ) {
					frac.leftDelimiter = delims[0];
					frac.rightDelimiter = delims[1];
				}
				frac.numerator = list;
				frac.denominator = BuildInternal( false, stopChar );
				if ( _error != null ) {
					return null;
				}
				AtomsList	fracList = new AtomsList();
							fracList.AddAtom( frac );
				return fracList;

			} else if ( command == @"\\" || command == @"cr" ) {
				if ( _currentEnv != null ) {
					// Stop the current list and increment the row count
					_currentEnv.numRows++;
					return list;

				} else {
					// Create a new table with the current list and a default env
					Atom	table = BuildTable( null, firstList:list, row:true );
					return new AtomsList( table, null );
				}
			} else if (command == @"end" ) {
				if ( _currentEnv == null ) {
					SetError( ParseError.ErrorType.MTParseErrorMissingBegin, @"Missing \\begin" );
					return null;
				}
				string	env = ReadEnvironment();
				if ( env == null ) {
					return null;
				}
				if ( env != _currentEnv.envName ) {
					SetError( ParseError.ErrorType.MTParseErrorInvalidEnv, @"Begin environment name " + _currentEnv.envName + " does not match end name: " + env );
					return null;
				}
				// Finish the current environment.
				_currentEnv.ended = true;
				return list;
			}
			return null;
		}

		// Applies the modifier to the atom. Returns true if modifier applied.
		bool	ApplyModifier( string modifier, Atom atom ) {
			if ( modifier == @"limits" ) {
				if ( atom is AtomLargeOperator ) {
					(atom as AtomLargeOperator).limits = true;
				} else {
					SetError( ParseError.ErrorType.MTParseErrorInvalidLimits, @"limits can only be applied to an operator." );
				}
				return true;

			} else if ( modifier == @"nolimits" ) {
				if ( atom is AtomLargeOperator ) {
					(atom as AtomLargeOperator).limits = false;
				} else {
					SetError( ParseError.ErrorType.MTParseErrorInvalidLimits, @"nolimits can only be applied to an operator." );
				}
				return true;
			}
			return false;
		}

		void	SetError( ParseError.ErrorType _code, string _message ) {
			// Only record the first error.
			if ( _error == null ) {
				_error = new ParseError() { error = _code, message = _message };
			}
		}
		void	SetError( ParseError.ErrorType _code ) {
			SetError( _code, null );
		}

		Atom	BuildTable( string env, AtomsList firstList, bool isRow ) {
			// Save the current env till an new one gets built.
			MTEnvProperties	oldEnv = _currentEnv;
			_currentEnv = new MTEnvProperties( env );

 			int	currentRow = 0;
 			int	currentCol = 0;

			List< List< AtomsList > >	rows = new List<List<AtomsList>>();
			rows.Add( new List<AtomsList>() );
			if ( firstList != null ) {
				rows[currentRow].Add( firstList );
				if ( isRow ) {
					_currentEnv.numRows++;
					currentRow++;
					rows.Add( new List<AtomsList>() );
				} else {
					currentCol++;
				}
			}
			while ( !_currentEnv.ended && HasCharacters ) {
				AtomsList	list = BuildInternal( false );
				if ( list != null ) {
					return null;	// If there is an error building the list, bail out early.
				}
				rows[currentRow].Add( list );
//				currentCol++;
				if ( _currentEnv.numRows > currentRow ) {
					currentRow = _currentEnv.numRows;
					rows.Add( new List<AtomsList>() );
					currentCol = 0;
				}
			}
			if ( !_currentEnv.ended && _currentEnv.envName != null ) {
				SetError( ParseError.ErrorType.MTParseErrorMissingEnd, @"Missing \\end" );
				return null;
			}
			ParseError	error = null;
			Atom		table = AtomFactory.TableWithEnvironment( _currentEnv.envName, rows:rows, error:&error );
			if ( table != null && _error != null ) {
				_error = error;
				return null;
			}

			// reinstate the old env.
			_currentEnv = oldEnv;
			return table;
		}

		static Dictionary< int, string >	spaceToCommands = null;
		Dictionary< int, string >	SpaceToCommands {
			get {
				if ( spaceToCommands == null ) {
					spaceToCommands = new Dictionary<int, string>();
					spaceToCommands.Add( 3, @"," );
					spaceToCommands.Add( 4, @">" );
					spaceToCommands.Add( 5, @";" );
					spaceToCommands.Add( (-3), @"!" );
					spaceToCommands.Add( 18, @"quad" );
					spaceToCommands.Add( 36, @"qquad" );
				}
				return spaceToCommands;
			}
		}

		static Dictionary< AtomStyle.LineStyle, string >	styleToCommands = null;
		Dictionary< AtomStyle.LineStyle, string >	StyleToCommands {
			get {
				if ( styleToCommands == null ) {
					styleToCommands = new Dictionary<AtomStyle.LineStyle, string>();
					styleToCommands.Add( AtomStyle.LineStyle.kMTLineStyleDisplay, @"displaystyle" );
					styleToCommands.Add( AtomStyle.LineStyle.kMTLineStyleText, @"textstyle" );
					styleToCommands.Add( AtomStyle.LineStyle.kMTLineStyleScript, @"scriptstyle" );
					styleToCommands.Add( AtomStyle.LineStyle.kMTLineStyleScriptScript, @"scriptscriptstyle" );
				}
				return styleToCommands;
			}
		}

		string	DelimToString( Atom delim ) {
			string	command = AtomFactory.DelimiterNameForBoundaryAtom( delim );
			if (command == null ) {
				return @"";
			}

			string[]	singleChars = new string[] { @"(", @")", @"[", @"]", @"<", @">", @"|", @".", @"/" };
			if ( singleChars.containsObject( command ) ) {
				return command;
			} else if ( command == @"||" ) {
				return @"\\|"; // special case for ||
			} else {
				return @"\\" + command;
			}
		}

/*		string	MathListToString( AtomsList ml ) {
			string	str = "";

			Atom.FontStyle	currentfontStyle = Atom.FontStyle.kMTFontStyleDefault;
			for ( Atom atom in ml.atoms ) {
				if ( currentfontStyle != atom.fontStyle ) {
					if (currentfontStyle != kMTFontStyleDefault) {
						// close the previous font style.
						[str appendString:@"}"];
					}
					if (atom.fontStyle != kMTFontStyleDefault) {
						// open new font style
						string fontStyleName = AtomFactory.fontNameForStyle:atom.fontStyle];
						[str appendFormat:@"\\%@{", fontStyleName];
					}
					currentfontStyle = atom.fontStyle;
				}
				if (atom.type == kMTMathAtomFraction) {
					MTFraction* frac = (MTFraction*) atom;
					if (frac.hasRule) {
						[str appendFormat:@"\\frac{%@}{%@}", [self mathListToString:frac.numerator], [self mathListToString:frac.denominator]];
					} else {
						string command = null;
						if (!frac.leftDelimiter && !frac.rightDelimiter) {
							command = @"atop";
						} else if ([frac.leftDelimiter isEqualToString:@"("] && [frac.rightDelimiter isEqualToString:@")"]) {
							command = @"choose";
						} else if ([frac.leftDelimiter isEqualToString:@"{"] && [frac.rightDelimiter isEqualToString:@"}"]) {
							command = @"brace";
						} else if ([frac.leftDelimiter isEqualToString:@"["] && [frac.rightDelimiter isEqualToString:@"]"]) {
							command = @"brack";
						} else {
							command = [NSString stringWithFormat:@"atopwithdelims%@%@", frac.leftDelimiter, frac.rightDelimiter];
						}
						[str appendFormat:@"{%@ \\%@ %@}", [self mathListToString:frac.numerator], command, [self mathListToString:frac.denominator]];
					}
				} else if (atom.type == kMTMathAtomRadical) {
					[str appendString:@"\\sqrt"];
					MTRadical* rad = (MTRadical*) atom;
					if (rad.degree) {
						[str appendFormat:@"[%@]", [self mathListToString:rad.degree]];
					}
					[str appendFormat:@"{%@}", [self mathListToString:rad.radicand]];
				} else if (atom.type == kMTMathAtomInner) {
					MTInner* inner = (MTInner*) atom;
					if (inner.leftBoundary || inner.rightBoundary) {
						if (inner.leftBoundary) {
							[str appendFormat:@"\\left%@ ", [self delimToString:inner.leftBoundary]];
						} else {
							[str appendString:@"\\left. "];
						}
						[str appendString:[self mathListToString:inner.innerList]];
						if (inner.rightBoundary) {
							[str appendFormat:@"\\right%@ ", [self delimToString:inner.rightBoundary]];
						} else {
							[str appendString:@"\\right. "];
						}
					} else {
						[str appendFormat:@"{%@}", [self mathListToString:inner.innerList]];
					}
				} else if (atom.type == kMTMathAtomTable) {
					MTMathTable* table = (MTMathTable*) atom;
					if (table.environment) {
						[str appendFormat:@"\\begin{%@}", table.environment];
					}
					for (int i = 0; i < table.numRows; i++) {
						NSArray<AtomsList*>* row = table.cells[i];
						for (int j = 0; j < row.count; j++) {
							AtomsList* cell = row[j];
							if ([table.environment isEqualToString:@"matrix"]) {
								if (cell.atoms.count >= 1 && cell.atoms[0].type == kMTMathAtomStyle) {
									// remove the first atom.
									NSArray* atoms = [cell.atoms subarrayWithRange:NSMakeRange(1, cell.atoms.count-1)];
									cell = [AtomsList mathListWithAtomsArray:atoms];
								}
							}
							if ([table.environment isEqualToString:@"eqalign"] || [table.environment isEqualToString:@"aligned"] || [table.environment isEqualToString:@"split"]) {
								if (j == 1 && cell.atoms.count >= 1 && cell.atoms[0].type == kMTMathAtomOrdinary && cell.atoms[0].nucleus.length == 0) {
									// Empty nucleus added for spacing. Remove it.
									NSArray* atoms = [cell.atoms subarrayWithRange:NSMakeRange(1, cell.atoms.count-1)];
									cell = [AtomsList mathListWithAtomsArray:atoms];
								}
							}
							[str appendString:[self mathListToString:cell]];
							if (j < row.count - 1) {
								[str appendString:@"&"];
							}
						}
						if (i < table.numRows - 1) {
							[str appendString:@"\\\\ "];
						}
					}
					if (table.environment) {
						[str appendFormat:@"\\end{%@}", table.environment];
					}
				} else if (atom.type == kMTMathAtomOverline) {
					[str appendString:@"\\overline"];
					MTOverLine* over = (MTOverLine*) atom;
					[str appendFormat:@"{%@}", [self mathListToString:over.innerList]];
				} else if (atom.type == kMTMathAtomUnderline) {
					[str appendString:@"\\underline"];
					MTUnderLine* under = (MTUnderLine*) atom;
					[str appendFormat:@"{%@}", [self mathListToString:under.innerList]];
				} else if (atom.type == kMTMathAtomAccent) {
					MTAccent* accent = (MTAccent*) atom;
					[str appendFormat:@"\\%@{%@}", AtomFactory.accentName:accent], [self mathListToString:accent.innerList]];
				} else if (atom.type == kMTMathAtomLargeOperator) {
					MTLargeOperator* op = (MTLargeOperator*) atom;
					string command = AtomFactory.latexSymbolNameForAtom:atom];
					MTLargeOperator* originalOp = (MTLargeOperator*) AtomFactory.atomForLatexSymbolName:command];
					[str appendFormat:@"\\%@ ", command];
					if (originalOp.limits != op.limits) {
						if (op.limits) {
							[str appendString:@"\\limits "];
						} else {
							[str appendString:@"\\nolimits "];
						}
					}
				} else if (atom.type == kMTMathAtomSpace) {
					MTMathSpace* space = (MTMathSpace*) atom;
					NSDictionary* spaceToCommands = [MTMathListBuilder spaceToCommands];
					string command = spaceToCommands[@(space.space)];
					if (command) {
						[str appendFormat:@"\\%@ ", command];
					} else {
						[str appendFormat:@"\\mkern%.1fmu", space.space];
					}
				} else if (atom.type == kMTMathAtomStyle) {
					MTMathStyle* style = (MTMathStyle*) atom;
					NSDictionary* styleToCommands = [MTMathListBuilder styleToCommands];
					string command = styleToCommands[@(style.style)];
					[str appendFormat:@"\\%@ ", command];
				} else if (atom.nucleus.length == 0) {
					[str appendString:@"{}"];
				} else if ([atom.nucleus isEqualToString:@"\u2236"]) {
					// math colon
					[str appendString:@":"];
				} else if ([atom.nucleus isEqualToString:@"\u2212"]) {
					// math minus
					[str appendString:@"-"];
				} else {
					string command = AtomFactory.latexSymbolNameForAtom:atom];
					if (command) {
						[str appendFormat:@"\\%@ ", command];
					} else {
						[str appendString:atom.nucleus];
					}
				}

				if (atom.superScript) {
					[str appendFormat:@"^{%@}", [self mathListToString:atom.superScript]];
				}
        
				if (atom.subScript) {
					[str appendFormat:@"_{%@}", [self mathListToString:atom.subScript]];
				}
			}
			if (currentfontStyle != kMTFontStyleDefault) {
				[str appendString:@"}"];
			}
			return [str copy];
		}
*/
		#endregion
    }
}
