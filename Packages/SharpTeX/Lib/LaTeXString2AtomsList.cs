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
		public enum ParseError {
			NONE,

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

		char[]			_chars;
		int				_currentChar;
		uint			_length = 0;
		AtomInner		_currentInnerAtom = null;

		MTEnvProperties	_currentEnv;
		Atom.FontStyle	_currentFontStyle = Atom.FontStyle.kMTFontStyleDefault;
		BOOL			_spacesAllowed = false;

		ParseError		_error = ParseError.NONE;		// Contains any error that occurred during parsing. */

		#endregion

		#region PROPERTIES

		bool	hasCharacters { get { return _currentChar < _length; } }

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
			if ( hasCharacters && _error != ParseError.NONE ) {
				// something went wrong most likely braces mismatched
				SetError( ParseError.MTParseErrorMismatchBraces );
				throw new Exception( "Mismatched braces: " + stringWithCharacters( _chars ) );
			}
			if ( _error != ParseError.NONE ) {
				return null;
			}
    
			return list;
		}

		AtomsList	BuildInternal( bool _oneCharOnly ) {
			return BuildInternal( _oneCharOnly, 0 );
		}


		AtomsList	BuildInternal( bool oneCharOnly, char stop ) {
			if ( oneCharOnly && stop > 0 )
				throw new Exception( @"Cannot set both oneCharOnly and stopChar." );

			AtomsList	list = new AtomsList();
			Atom		prevAtom = null;
			while ( hasCharacters ) {
				if ( _error != ParseError.NONE ) {
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
					prevAtom = sublist.atoms.lastObject;
					list.Append( sublist );
					if ( oneCharOnly ) {
						return list;
					}
					continue;

				} else if ( ch == '}' ) {
					if ( oneCharOnly ) throw new Exception( @"This should have been handled before" );
					if ( stop != 0 ) throw new Exception( @"This should have been handled before" );
					// We encountered a closing brace when there is no stop set, that means there was no corresponding opening brace.
					SetError( ParseError.MTParseErrorMismatchBraces, @"Mismatched braces." );
					return null;

				} else if ( ch == '\\' ) {
					// \ means a command
					string		command = ReadCommand();
					AtomsList	done = StopCommand( :command list:list stopChar:stop );
					if ( done != null ) {
						return done;
					} else if ( _error != ParseError.NONE ) {
						return null;
					}
					if ( ApplyModifier( :command atom:prevAtom )) {
						continue;
					}

					Atom.FontStyle	fontStyle = FontStyleWithName( command );
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

						prevAtom = sublist.atoms.lastObject;
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
						SetError( ParseError.MTParseErrorInternalError, @"Internal error" );
						return null;
					}

				} else if ( ch == '&' ) {
					// used for column separation in tables
					if ( oneCharOnly ) throw new Exception( @"This should have been handled before" );
					if ( _currentEnv ) {
						return list;
					} else {
						// Create a new table with the current list and a default env
						Atom	table = BuildTable( null firstList:list row:NO );
						return AtomsList. mathListWithAtoms:table, null];
					}
				} else if ( _spacesAllowed && ch == ' ' ) {
					// If spaces are allowed then spaces do not need escaping with a \ before being used.
					atom = Atom.AtomForLatexSymbolName( @" " );
				} else {
					atom = Atom.AtomForCharacter( ch );
					if ( atom = null ) {
						continue;	// Not a recognized character
					}
				}
				NSAssert(atom != null, @"Atom shouldn't be null");
				atom.fontStyle = _currentFontStyle;
				[list addAtom:atom];
				prevAtom = atom;
        
				if (oneCharOnly) {
					// we consumed our onechar
					return list;
				}
			}
			if (stop > 0) {
				if (stop == '}') {
					// We did not find a corresponding closing brace.
					[self setError:MTParseErrorMismatchBraces message:@"Missing closing brace"];
				} else {
					// we never found our stop character
					NSString* errorMessage = [NSString stringWithFormat:@"Expected character not found: %d", stop];
					[self setError:MTParseErrorCharacterNotFound message:errorMessage];
				}
			}
			return list;
		}


		/// <summary>
		/// Construct a math list from a given string. If there is parse error, returns null. To retrieve the error use the function Error
		/// </summary>
		/// <param name="_LaTeX"></param>
		/// <returns></returns>
		public AtomsList	BuildFromString( string _LaTeX ) {
		}

		/// <summary>
		/// Construct a math list from a given string. If there is parse error, returns null. The error is returned in the `error` parameter.
		/// </summary>
		/// <param name="_LaTeX"></param>
		/// <param name="_error"></param>
		/// <returns></returns>
		public AtomsList	BuildFromString( string _LaTeX, out ParseError _error ) {
		}

		/// <summary>
		/// This converts the list of atoms to LaTeX.
		/// </summary>
		/// <param name="_list"></param>
		/// <returns></returns>
		public string		AtomsListToLaTeX( AtomsList _list ) {

		}

		// 
		// @param str The LaTeX string to be used to build the `MTMathList`
		// 
		// - (nonnull instancetype) initWithString:(nonnull NSString*) str NS_DESIGNATED_INITIALIZER;
		// - (nonnull instancetype) init NS_UNAVAILABLE;
		// 
		// 
		// - (nullable AtomsList) build;
		// 
		// /** Construct a math list from a given string. If there is parse error, returns
		//  null. To retrieve the error use the function `[MTMathListBuilder buildFromString:error:]`.
		//  */
		// + (nullable AtomsList) buildFromString:(nonnull NSString*) str;
		// 
		// /** Construct a math list from a given string. If there is an error while constructing the string, this returns null. The error is returned in the `error` parameter.
		//  */
		// + (nullable AtomsList) buildFromString:(nonnull NSString*) str error:( NSError* _Nullable * _Nullable) error;
		// 
		// 
		// + (nonnull NSString*) mathListToString:(nonnull AtomsList) ml;

		// gets the next character and moves the pointer ahead
		char	GetNextCharacter() {
			return _chars[_currentChar++];
		}

		void	UnlookCharacter() {
			if ( _currentChar == 0 )
				throw new Exception( "Already at start of stream!" );
			_currentChar--;
		}

		#endregion
    }
}
