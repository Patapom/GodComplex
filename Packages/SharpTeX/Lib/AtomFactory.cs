//////////////////////////////////////////////////////////////////////////
/// This is an adaptation of the iOS LaTeX library by Kostub Deshmukh
/// Initial GitHub project: https://github.com/kostub/iosMath
///
//////////////////////////////////////////////////////////////////////////
///
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTeX
{

	/// <summary>
	/// The error encountered when parsing a LaTeX string.
	/// The `code` in the `NSError` is one of the following indicating why the LaTeX string could not be parsed.
	/// </summary>
	public class	ParseException : Exception {

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

		public string		message;
		public ErrorType	error;

		public ParseException( string _message, ErrorType _error ) {
			message = _message;
			error = _error;
		}
	}

    public static class AtomFactory {

		public static void	BuildDictionaryFromList< Tkey, Tvalue >( Dictionary< Tkey, Tvalue > _dictionary, params object[] _keyValuePairs ) where Tvalue : class {
			int	count = _keyValuePairs.Length;
			if ( (count & 1) != 0 )
				throw new Exception( "Expected even number of objects!" );

			for ( int i=0; i < count; ) {
				Tkey	key = (Tkey) _keyValuePairs[i++];
				Tvalue	value = _keyValuePairs[i++] as Tvalue;
				if ( key == null || value == null )
					throw new Exception( "Invalid object type at index " + i );
				_dictionary.Add( key, value );
			}
		}
		public static Dictionary< Tkey, Tvalue >	BuildDictionaryFromList< Tkey, Tvalue >( params object[] _keyValuePairs ) where Tvalue : class {
			Dictionary<Tkey, Tvalue>	result = new Dictionary<Tkey, Tvalue>();
			BuildDictionaryFromList( result, _keyValuePairs );
			return result;
		}

		public static string	 TypeToText( Atom.TYPE _type ) {
			switch (_type) {
				case Atom.TYPE.kMTMathAtomOrdinary:			return @"Ordinary";
				case Atom.TYPE.kMTMathAtomNumber:			return @"Number";
				case Atom.TYPE.kMTMathAtomVariable:			return @"Variable";
				case Atom.TYPE.kMTMathAtomBinaryOperator:	return @"Binary Operator";
				case Atom.TYPE.kMTMathAtomUnaryOperator:	return @"Unary Operator";
				case Atom.TYPE.kMTMathAtomRelation:			return @"Relation";
				case Atom.TYPE.kMTMathAtomOpen:				return @"Open";
				case Atom.TYPE.kMTMathAtomClose:			return @"Close";
				case Atom.TYPE.kMTMathAtomFraction:			return @"Fraction";
				case Atom.TYPE.kMTMathAtomRadical:			return @"Radical";
				case Atom.TYPE.kMTMathAtomPunctuation:		return @"Punctuation";
				case Atom.TYPE.kMTMathAtomPlaceholder:		return @"Placeholder";
				case Atom.TYPE.kMTMathAtomLargeOperator:	return @"Large Operator";
				case Atom.TYPE.kMTMathAtomInner:			return @"Inner";
				case Atom.TYPE.kMTMathAtomUnderline:		return @"Underline";
				case Atom.TYPE.kMTMathAtomOverline:			return @"Overline";
				case Atom.TYPE.kMTMathAtomAccent:			return @"Accent";
				case Atom.TYPE.kMTMathAtomBoundary:			return @"Boundary";
				case Atom.TYPE.kMTMathAtomSpace:			return @"Space";
				case Atom.TYPE.kMTMathAtomStyle:			return @"Style";
				case Atom.TYPE.kMTMathAtomColor:			return @"Color";
				case Atom.TYPE.kMTMathAtomColorbox:			return @"Colorbox";
				case Atom.TYPE.kMTMathAtomTable:			return @"Table";
			}
			return null;
		}

		#region Atoms <=> LaTeX Correspondance

		public static AtomsList	AtomsListForCharacters( string chars ) {
			if ( chars == null ) throw new Exception( "Invalid characters!" );

			AtomsList	list = new AtomsList();
			for ( int i=0; i < chars.Length; i++ ) {
				Atom	atom = AtomForCharacter( chars[i] );
				if ( atom != null ) {
					list.AddAtom( atom );
				}
			}
			return list;
		}

		public static Atom	AtomForCharacter( char ch ) {
			string	chStr = ch.ToString();
			if ( ch > 0x0410 && ch < 0x044F ){
				return Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, chStr );	// show basic cyrillic alphabet. Latin Modern Math font is not good for cyrillic symbols
			} else if ( ch < 0x21 || ch > 0x7E ) {
				return null;	// skip non ascii characters and spaces
			} else if ( ch == '$' || ch == '%' || ch == '#' || ch == '&' || ch == '~' || ch == '\'' ) {
				return null;	// These are latex control characters that have special meanings. We don't support them.
			} else if ( ch == '^' || ch == '_' || ch == '{' || ch == '}' || ch == '\\' ) {
				return null;		// more special characters for Latex.
			} else if ( ch == '(' || ch == '[' ) {
				return Atom.Create( Atom.TYPE.kMTMathAtomOpen, chStr );
			} else if (ch == ')' || ch == ']' || ch == '!' || ch == '?') {
				return Atom.Create( Atom.TYPE.kMTMathAtomClose, chStr );
			} else if (ch == ',' || ch == ';') {
				return Atom.Create( Atom.TYPE.kMTMathAtomPunctuation, chStr );
			} else if (ch == '=' || ch == '>' || ch == '<') {
				return Atom.Create( Atom.TYPE.kMTMathAtomRelation, chStr );
			} else if (ch == ':') {
				return Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2236" );	// Math colon is ratio. Regular colon is \colon
			} else if (ch == '-') {
				return Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2212" );	// Use the math minus sign
			} else if (ch == '+' || ch == '*') {
				return Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, chStr );
			} else if (ch == '.' || (ch >= '0' && ch <= '9')) {
				return Atom.Create( Atom.TYPE.kMTMathAtomNumber, chStr );
			} else if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')) {
				return Atom.Create( Atom.TYPE.kMTMathAtomVariable, chStr );
			} else if (ch == '"' || ch == '/' || ch == '@' || ch == '`' || ch == '|') {
				return Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, chStr );	// just an ordinary character. The following are allowed ordinary chars | / ` @ "
			} else {
				throw new Exception( @"Unknown ascii character '" + ch + "'. Should have been accounted for." );
			}
		}

		public static Atom	AtomForLatexSymbolName( string symbolName ) {
			if ( symbolName == null ) throw new Exception( "Invalid symbol!" );

			// First check if this is an alias
			string	canonicalName;
			if ( Aliases.TryGetValue( symbolName, out canonicalName ) ) {
				symbolName = canonicalName;		// Switch to the canonical name
			}
    
			Atom	symbol = null;
			if ( !SupportedLatexSymbols.TryGetValue( symbolName, out symbol ) )
				return null;

			// Return a copy of the atom since atoms are mutable.
			symbol = symbol.Copy();
			return symbol;
		}

		public static string	LatexSymbolNameForAtom( Atom atom ) {
			if ( atom.nucleus.Length == 0 ) {
				return null;
			}

			string	symbolName = null;;
			TextToLatexSymbolNames.TryGetValue( atom.nucleus, out symbolName );
			return symbolName;
		}

// 		void	AddLatexSymbol( string name, Atom atom ) {
// 			NSParameterAssert(name);
// 			NSParameterAssert(atom);
// 			NSMutableDictionary<string, MTMathAtom*>* commands = [self supportedLatexSymbols];
// 			commands[name] = atom;
// 			if (atom.nucleus.length != 0) {
// 				NSMutableDictionary<string, string>* dict = [self textToLatexSymbolNames];
// 				dict[atom.nucleus] = name;
// 			}
// 		}
// 
// 		string[]	supportedLatexSymbolNames {
// 			NSDictionary<string, MTMathAtom*>* commands = [MTMathAtomFactory supportedLatexSymbols];
// 			return commands.allKeys;
// 		}

		public static AtomAccent	AccentWithName( string accentName ) {
			string	accentNucleus;
			if ( !accents.TryGetValue( accentName, out accentNucleus ) )
				return null;

			return new AtomAccent( accentNucleus );
		}

// 		+(string) accentName:(MTAccent*) accent
// 		{
// 			NSDictionary* dict = [MTMathAtomFactory accentValueToName];
// 			return dict[accent.nucleus];
// 		}

		static Dictionary<string, string>	ms_delimiters = null;
		static Dictionary<string, string>	delimiters {
			get {
				if ( ms_delimiters == null ) {
					ms_delimiters = BuildDictionaryFromList<string,string>( 
						   @"." , @"", // . means no delimiter
						   @"(" , @"(",
						   @")" , @")",
						   @"[" , @"[",
						   @"]" , @"]",
						   @"<" , @"\u2329",
						   @">" , @"\u232A",
						   @"/" , @"/",
						   @"\\" , @"\\",
						   @"|" , @"|",
						   @"lgroup" , @"\u27EE",
						   @"rgroup" , @"\u27EF",
						   @"||" , @"\u2016",
						   @"Vert" , @"\u2016",
						   @"vert" , @"|",
						   @"uparrow" , @"\u2191",
						   @"downarrow" , @"\u2193",
						   @"updownarrow" , @"\u2195",
						   @"Uparrow" , @"21D1",
						   @"Downarrow" , @"21D3",
						   @"Updownarrow" , @"21D5",
						   @"backslash" , @"\\",
						   @"rangle" , @"\u232A",
						   @"langle" , @"\u2329",
						   @"rbrace" , @"}",
						   @"}" , @"}",
						   @"{" , @"{",
						   @"lbrace" , @"{",
						   @"lceil" , @"\u2308",
						   @"rceil" , @"\u2309",
						   @"lfloor" , @"\u230A",
						   @"rfloor" , @"\u230B"
					);
				}
				return ms_delimiters;
			}
		}

		public static Atom	boundaryAtomForDelimiterName( string delimName ) {
			string delimValue;
			if ( !delimiters.TryGetValue( delimName, out delimValue ) )
				return null;

			return Atom.Create( Atom.TYPE.kMTMathAtomBoundary, delimValue );
		}

		public static string	delimiterNameForBoundaryAtom( Atom boundary ) {
			if ( boundary.type != Atom.TYPE.kMTMathAtomBoundary ) {
				return null;
			}
			string	name = null;
			delimValueToName.TryGetValue( boundary.nucleus, out name );
			return name;
		}

		static Dictionary< string, string >	ms_delimToCommands = null;
		static Dictionary< string, string >	delimValueToName {
			get {
				if ( ms_delimToCommands == null ) {
					Dictionary< string, string >	delims = delimiters;
					ms_delimToCommands = new Dictionary< string, string >( delims.Count );

					foreach ( string command in delims.Keys ) {
						string	delim = delims[command];
						string	existingCommand;
						if ( ms_delimToCommands.TryGetValue( delim, out existingCommand ) ) {
							if ( command.Length > existingCommand.Length ) {
								continue;	// Keep the shorter command
							} else if (command.Length == existingCommand.Length) {
								// If the length is the same, keep the alphabetically first
								if ( command.CompareTo( existingCommand ) <= 0 ) {
									continue;
								}
							}
						}
						// In other cases replace the command.
						ms_delimToCommands[delim] = command;
					}
				}
				return ms_delimToCommands;
			}
		}

		static Dictionary<string, string[]>	ms_matrixEnvs = BuildDictionaryFromList<string, string[]>(
			@"matrix", new string[] { },
			@"pmatrix" , new string[] { @"(", @")" },
			@"bmatrix" , new string[] { @"[", @"]" },
			@"Bmatrix" , new string[] { @"{", @"}" },
			@"vmatrix" , new string[] { @"vert", @"vert" },
			@"Vmatrix" , new string[] { @"Vert", @"Vert" }
		);
		public static Atom	tableWithEnvironment( string env, AtomsList[][] rows ) {

			AtomTable table = new AtomTable( env );
			for ( uint i = 0; i < (uint) rows.Length; i++ ) {
				AtomsList[]	row = rows[i];
				for ( uint j = 0; j < (uint) row.Length; j++ ) {
					table.SetCell( i, j, row[j] );
				}
			}

			if ( env == null ) {
				// The default env.
				table.interRowAdditionalSpacing = 1;
				table.interColumnSpacing = 0;
				for ( uint i = 0; i < table.ColumnsCount; i++) {
					table.SetAlignment( i, AtomTable.ColumnAlignment.kMTColumnAlignmentLeft );
				}
				return table;
			}

			string[]	delims;
			if ( ms_matrixEnvs.TryGetValue( env, out delims ) ) {
				// it is set to matrix as the delimiters are converted to latex outside the table.
				table.environment = @"matrix";
				table.interRowAdditionalSpacing = 0;
				table.interColumnSpacing = 18;

				// All the lists are in textstyle
				AtomStyle	style = new AtomStyle( AtomStyle.LineStyle.kMTLineStyleText );
				table.InsertStyleAtom( style );

				// Add delimiters
				if ( delims.Length != 2 )
					return table;

				AtomInner	inner = new AtomInner() { LeftBoundary = boundaryAtomForDelimiterName( delims[0] ), RightBoundary = boundaryAtomForDelimiterName( delims[1] ), innerList = new AtomsList( table ) };
				return inner;

			} 
 
			switch ( env ) {
				case @"eqalign":
				case @"split":
				case @"aligned": {

					if ( table.ColumnsCount != 2 ) {
						throw new ParseException( "Environment \"" + env + "\" can only have 2 columns", ParseException.ErrorType.MTParseErrorInvalidNumColumns );
					}

					// Add a spacer before each of the second column elements. This is to create the correct spacing for = and other relations.
					Atom	spacer = Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"" );
					table.InsertStyleAtom( spacer );

					table.interRowAdditionalSpacing = 1;
					table.interColumnSpacing = 0;
					table.SetAlignment( 0, AtomTable.ColumnAlignment.kMTColumnAlignmentRight );
					table.SetAlignment( 1, AtomTable.ColumnAlignment.kMTColumnAlignmentLeft );

					return table;
				}
			
				case @"displaylines":
				case @"gather":
					if ( table.ColumnsCount != 1 ) {
						throw new ParseException( "Environment \"" + env + "\" can only have 1 columns", ParseException.ErrorType.MTParseErrorInvalidNumColumns );
					}
					table.interRowAdditionalSpacing = 1;
					table.interColumnSpacing = 0;
					table.SetAlignment( 0, AtomTable.ColumnAlignment.kMTColumnAlignmentCenter );
					return table;

				case @"eqnarray":
					if ( table.ColumnsCount != 3 ) {
						throw new ParseException( "Environment \"" + env + "\" can only have 3 columns", ParseException.ErrorType.MTParseErrorInvalidNumColumns );
					}

					table.interRowAdditionalSpacing = 1;
					table.interColumnSpacing = 18;
					table.SetAlignment( 0, AtomTable.ColumnAlignment.kMTColumnAlignmentRight );
					table.SetAlignment( 1, AtomTable.ColumnAlignment.kMTColumnAlignmentCenter );
					table.SetAlignment( 2, AtomTable.ColumnAlignment.kMTColumnAlignmentLeft );

					return table;
	
				case @"cases": {
					if ( table.ColumnsCount != 2 ) {
						throw new ParseException( "Environment \"" + env + "\" can only have 2 columns", ParseException.ErrorType.MTParseErrorInvalidNumColumns );
					}

					table.interRowAdditionalSpacing = 0;
					table.interColumnSpacing = 18;
					table.SetAlignment( 0, AtomTable.ColumnAlignment.kMTColumnAlignmentLeft );
					table.SetAlignment( 1, AtomTable.ColumnAlignment.kMTColumnAlignmentLeft );

					// All the lists are in textstyle
					Atom	style = new AtomStyle( AtomStyle.LineStyle.kMTLineStyleText );
					table.InsertStyleAtom( style );

					// Add delimiters
					AtomInner	inner = new AtomInner() { LeftBoundary = boundaryAtomForDelimiterName( @"{" ), RightBoundary = boundaryAtomForDelimiterName( @"." ), innerList = new AtomsList( AtomForLatexSymbolName( @"," ), table ) };
					return inner;
				}

				default:
					throw new ParseException( @"Unknown environment: " + env, ParseException.ErrorType.MTParseErrorInvalidEnv );
			}
		}


		static Dictionary< string, Atom >	ms_supportedLatexSymbols = null;
		public static Dictionary< string, Atom >	SupportedLatexSymbols {
			get {
				if ( ms_supportedLatexSymbols == null ) {
					ms_supportedLatexSymbols = BuildDictionaryFromList< string, Atom >( 
							 @"square", placeholder,
                     
							 // Greek characters
							 @"alpha", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03B1" ),
							 @"beta", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03B2" ),
							 @"gamma", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03B3" ),
							 @"delta", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03B4" ),
							 @"varepsilon", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03B5" ),
							 @"zeta", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03B6" ),
							 @"eta", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03B7" ),
							 @"theta", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03B8" ),
							 @"iota", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03B9" ),
							 @"kappa", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03BA" ),
							 @"lambda", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03BB" ),
							 @"mu", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03BC" ),
							 @"nu", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03BD" ),
							 @"xi", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03BE" ),
							 @"omicron", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03BF" ),
							 @"pi", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03C0" ),
							 @"rho", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03C1" ),
							 @"varsigma", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03C2" ),
							 @"sigma", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03C3" ),
							 @"tau", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03C4" ),
							 @"upsilon", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03C5" ),
							 @"varphi", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03C6" ),
							 @"chi", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03C7" ),
							 @"psi", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03C8" ),
							 @"omega", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03C9" ),

							 @"vartheta", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03D1" ),
							 @"phi", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03D5" ),
							 @"varpi", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03D6" ),
							 @"varkappa", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03F0" ),
							 @"varrho", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03F1" ),
							 @"epsilon", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03F5" ),

							 // Capital greek characters
							 @"Gamma", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u0393" ),
							 @"Delta", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u0394" ),
							 @"Theta", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u0398" ),
							 @"Lambda", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u039B" ),
							 @"Xi", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u039E" ),
							 @"Pi", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03A0" ),
							 @"Sigma", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03A3" ),
							 @"Upsilon", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03A5" ),
							 @"Phi", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03A6" ),
							 @"Psi", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03A8" ),
							 @"Omega", Atom.Create( Atom.TYPE.kMTMathAtomVariable, @"\u03A9" ),
                     
							 // Open
							 @"lceil", Atom.Create( Atom.TYPE.kMTMathAtomOpen, @"\u2308" ),
							 @"lfloor", Atom.Create( Atom.TYPE.kMTMathAtomOpen, @"\u230A" ),
							 @"langle", Atom.Create( Atom.TYPE.kMTMathAtomOpen, @"\u27E8" ),
							 @"lgroup", Atom.Create( Atom.TYPE.kMTMathAtomOpen, @"\u27EE" ),
                     
							 // Close
							 @"rceil", Atom.Create( Atom.TYPE.kMTMathAtomClose, @"\u2309" ),
							 @"rfloor", Atom.Create( Atom.TYPE.kMTMathAtomClose, @"\u230B" ),
							 @"rangle", Atom.Create( Atom.TYPE.kMTMathAtomClose, @"\u27E9" ),
							 @"rgroup", Atom.Create( Atom.TYPE.kMTMathAtomClose, @"\u27EF" ),
                     
							 // Arrows
							 @"leftarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2190" ),
							 @"uparrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2191" ),
							 @"rightarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2192" ),
							 @"downarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2193" ),
							 @"leftrightarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2194" ),
							 @"updownarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2195" ),
							 @"nwarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2196" ),
							 @"nearrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2197" ),
							 @"searrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2198" ),
							 @"swarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2199" ),
							 @"mapsto", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u21A6" ),
							 @"Leftarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u21D0" ),
							 @"Uparrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u21D1" ),
							 @"Rightarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u21D2" ),
							 @"Downarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u21D3" ),
							 @"Leftrightarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u21D4" ),
							 @"Updownarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u21D5" ),
							 @"longleftarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u27F5" ),
							 @"longrightarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u27F6" ),
							 @"longleftrightarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u27F7" ),
							 @"Longleftarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u27F8" ),
							 @"Longrightarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u27F9" ),
							 @"Longleftrightarrow", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u27FA" ),
                     
                     
							 // Relations
							 @"leq", Atom.Create( Atom.TYPE.kMTMathAtomRelation, MTSymbolLessEqual ),
							 @"geq", Atom.Create( Atom.TYPE.kMTMathAtomRelation, MTSymbolGreaterEqual ),
							 @"neq", Atom.Create( Atom.TYPE.kMTMathAtomRelation, MTSymbolNotEqual ),
							 @"in", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2208" ),
							 @"notin", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2209" ),
							 @"ni", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u220B" ),
							 @"propto", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u221D" ),
							 @"mid", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2223" ),
							 @"parallel", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2225" ),
							 @"sim", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u223C" ),
							 @"simeq", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2243" ),
							 @"cong", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2245" ),
							 @"approx", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2248" ),
							 @"asymp", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u224D" ),
							 @"doteq", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2250" ),
							 @"equiv", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2261" ),
							 @"gg", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u226A" ),
							 @"ll", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u226B" ),
							 @"prec", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u227A" ),
							 @"succ", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u227B" ),
							 @"subset", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2282" ),
							 @"supset", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2283" ),
							 @"subseteq", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2286" ),
							 @"supseteq", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2287" ),
							 @"sqsubset", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u228F" ),
							 @"sqsupset", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2290" ),
							 @"sqsubseteq", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2291" ),
							 @"sqsupseteq", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u2292" ),
							 @"models", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u22A7" ),
							 @"perp", Atom.Create( Atom.TYPE.kMTMathAtomRelation, @"\u27C2" ),
                     
							 // operators
							 @"times", AtomFactory.times,
							 @"div"  , AtomFactory.divide,
							 @"pm"   , Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u00B1" ),
							 @"dagger", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2020" ),
							 @"ddagger", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2021" ),
							 @"mp"   , Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2213" ),
							 @"setminus", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2216" ),
							 @"ast"  , Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2217" ),
							 @"circ" , Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2218" ),
							 @"bullet", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2219" ),
							 @"wedge", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2227" ),
							 @"vee", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2228" ),
							 @"cap", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2229" ),
							 @"cup", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u222A" ),
							 @"wr", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2240" ),
							 @"uplus", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u228E" ),
							 @"sqcap", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2293" ),
							 @"sqcup", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2294" ),
							 @"oplus", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2295" ),
							 @"ominus", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2296" ),
							 @"otimes", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2297" ),
							 @"oslash", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2298" ),
							 @"odot", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2299" ),
							 @"star" , Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u22C6" ),
							 @"cdot" , Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u22C5" ),
							 @"amalg", Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2A3F" ),
                     
							 // No limit operators
							 @"log", OperatorWithName( @"log", false ),
							 @"lg", OperatorWithName( @"lg", false ),
							 @"ln", OperatorWithName( @"ln", false ),
							 @"sin", OperatorWithName( @"sin", false ),
							 @"arcsin", OperatorWithName( @"arcsin", false ),
							 @"sinh", OperatorWithName( @"sinh", false ),
							 @"cos", OperatorWithName( @"cos", false ),
							 @"arccos", OperatorWithName( @"arccos", false ),
							 @"cosh", OperatorWithName( @"cosh", false ),
							 @"tan", OperatorWithName( @"tan", false ),
							 @"arctan", OperatorWithName( @"arctan", false ),
							 @"tanh", OperatorWithName( @"tanh", false ),
							 @"cot", OperatorWithName( @"cot", false ),
							 @"coth", OperatorWithName( @"coth", false ),
							 @"sec", OperatorWithName( @"sec", false ),
							 @"csc", OperatorWithName( @"csc", false ),
							 @"arg", OperatorWithName( @"arg", false ),
							 @"ker", OperatorWithName( @"ker", false ),
							 @"dim", OperatorWithName( @"dim", false ),
							 @"hom", OperatorWithName( @"hom", false ),
							 @"exp", OperatorWithName( @"exp", false ),
							 @"deg", OperatorWithName( @"deg", false ),
                     
							 // Limit operators
							 @"lim", OperatorWithName( @"lim", true ),
							 @"limsup", OperatorWithName( @"lim sup", true ),
							 @"liminf", OperatorWithName( @"lim inf", true ),
							 @"max", OperatorWithName( @"max", true ),
							 @"min", OperatorWithName( @"min", true ),
							 @"sup", OperatorWithName( @"sup", true ),
							 @"inf", OperatorWithName( @"inf", true ),
							 @"det", OperatorWithName( @"det", true ),
							 @"Pr", OperatorWithName( @"Pr", true ),
							 @"gcd", OperatorWithName( @"gcd", true ),
                     
							 // Large operators
							 @"prod", OperatorWithName( @"\u220F", true ),
							 @"coprod", OperatorWithName( @"\u2210", true ),
							 @"sum", OperatorWithName( @"\u2211", true ),
							 @"int", OperatorWithName( @"\u222B", false ),
							 @"oint", OperatorWithName( @"\u222E", false ),
							 @"bigwedge", OperatorWithName( @"\u22C0", true ),
							 @"bigvee", OperatorWithName( @"\u22C1", true ),
							 @"bigcap", OperatorWithName( @"\u22C2", true ),
							 @"bigcup", OperatorWithName( @"\u22C3", true ),
							 @"bigodot", OperatorWithName( @"\u2A00", true ),
							 @"bigoplus", OperatorWithName( @"\u2A01", true ),
							 @"bigotimes", OperatorWithName( @"\u2A02", true ),
							 @"biguplus", OperatorWithName( @"\u2A04", true ),
							 @"bigsqcup", OperatorWithName( @"\u2A06", true ),
                     
							 // Latex command characters
							 @"{", Atom.Create( Atom.TYPE.kMTMathAtomOpen, @"{" ),
							 @"}", Atom.Create( Atom.TYPE.kMTMathAtomClose, @"}" ),
							 @"$", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"$" ),
							 @"&", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"&" ),
							 @"#", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"#" ),
							 @"%", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"%" ),
							 @"_", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"_" ),
							 @" ", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @" " ),
							 @"backslash", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\\" ),
                     
							 // Punctuation
							 // Note: \colon is different from, which is a relation
							 @"colon", Atom.Create( Atom.TYPE.kMTMathAtomPunctuation, @":" ),
							 @"cdotp", Atom.Create( Atom.TYPE.kMTMathAtomPunctuation, @"\u00B7" ),
                     
							 // Other symbols
							 @"degree", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u00B0" ),
							 @"neg", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u00AC" ),
							 @"angstrom", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u00C5" ),
							 @"|", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u2016" ),
							 @"vert", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"|" ),
							 @"ldots", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u2026" ),
							 @"prime", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u2032" ),
							 @"hbar", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u210F" ),
							 @"Im", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u2111" ),
							 @"ell", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u2113" ),
							 @"wp", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u2118" ),
							 @"Re", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u211C" ),
							 @"mho", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u2127" ),
							 @"aleph", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u2135" ),
							 @"forall", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u2200" ),
							 @"exists", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u2203" ),
							 @"emptyset", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u2205" ),
							 @"nabla", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u2207" ),
							 @"infty", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u221E" ),
							 @"angle", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u2220" ),
							 @"top", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u22A4" ),
							 @"bot", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u22A5" ),
							 @"vdots", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u22EE" ),
							 @"cdots", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u22EF" ),
							 @"ddots", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u22F1" ),
							 @"triangle", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\u25B3" ),
							 @"imath", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\U0001D6A4" ),
							 @"jmath", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\U0001D6A5" ),
							 @"partial", Atom.Create( Atom.TYPE.kMTMathAtomOrdinary, @"\U0001D715" ),
                     
							 // Spacing
							 @",", new AtomSpace( 3 ),
							 @">", new AtomSpace( 4 ),
							 @";", new AtomSpace( 5 ),
							 @"!", new AtomSpace( -3 ),
							 @"quad",	new AtomSpace( 18 ),	// quad = 1em = 18mu
							 @"qquad",	new AtomSpace( 36 ),	// qquad = 2em
                     
							 // Style
							 @"displaystyle", new AtomStyle( AtomStyle.LineStyle.kMTLineStyleDisplay ),
							 @"textstyle",			new AtomStyle( AtomStyle.LineStyle.kMTLineStyleText ),
							 @"scriptstyle",		new AtomStyle( AtomStyle.LineStyle.kMTLineStyleScript ),
							 @"scriptscriptstyle",	new AtomStyle( AtomStyle.LineStyle.kMTLineStyleScriptScript )
					);
				}
				return ms_supportedLatexSymbols;
			}
		}

		static Dictionary< string, string >	ms_aliases = null;
		static Dictionary< string, string >	Aliases {
			get {
				if ( ms_aliases == null ) {
					ms_aliases = BuildDictionaryFromList< string, string >( 
						@"lnot", @"neg",
						@"land", @"wedge",
						@"lor", @"vee",
						@"ne", @"neq",
						@"le", @"leq",
						@"ge", @"geq",
						@"lbrace", @"{",
						@"rbrace", @"}",
						@"Vert", @"|",
						@"gets", @"leftarrow",
						@"to", @"rightarrow",
						@"iff", @"Longleftrightarrow",
						@"AA", @"angstrom"
					);
				}
				return ms_aliases;
			}
		}

		static Dictionary<string, string>	ms_textToLatexSymbolNames = null;
		static Dictionary<string, string>	TextToLatexSymbolNames {
			get {
				if ( ms_textToLatexSymbolNames == null ) {
					Dictionary< string, Atom >	commands = SupportedLatexSymbols;
					ms_textToLatexSymbolNames = new Dictionary<string, string>( commands.Count );
					foreach ( string command in commands.Keys ) {
						Atom	atom = commands[command];
						if ( atom.nucleus.Length == 0 ) {
							continue;
						}
            
						string	existingCommand;
						if ( ms_textToLatexSymbolNames.TryGetValue( atom.nucleus, out existingCommand ) ) {
							// If there are 2 commands for the same symbol, choose one deterministically.
							if ( command.Length > existingCommand.Length ) {
								continue;	// Keep the shorter command
							} else if ( command.Length == existingCommand.Length ) {
								// If the length is the same, keep the alphabetically first
								if ( command.CompareTo( existingCommand ) <= 0 ) {
									continue;
								}
							}
						}
						// In other cases replace the command.
						ms_textToLatexSymbolNames[atom.nucleus] = command;
					}
				}
				return ms_textToLatexSymbolNames;
			}
		}

		static Dictionary<string, string>	ms_accents = null;
		static Dictionary<string, string>	accents {
			get {
				if ( ms_accents == null ) {
					ms_accents = BuildDictionaryFromList<string,string>(
								@"grave", @"\u0300",
								@"acute", @"\u0301",
								@"hat", @"\u0302",  // In our implementation hat and widehat behave the same.
								@"tilde", @"\u0303", // In our implementation tilde and widetilde behave the same.
								@"bar", @"\u0304",
								@"breve", @"\u0306",
								@"dot", @"\u0307",
								@"ddot", @"\u0308",
								@"check", @"\u030C",
								@"vec", @"\u20D7",
								@"widehat", @"\u0302",
								@"widetilde", @"\u0303"
							);
				}
				return accents;
			}
		}

		#endregion

		#region Font Styles

		public static Atom.FontStyle	FontStyleWithName( string _fontName ) {
			if ( !FontStyles.ContainsKey( _fontName ) )
				return Atom.FontStyle.NOT_FOUND;

			return FontStyles[_fontName];
		}

		public static string	FontNameForStyle( Atom.FontStyle _fontStyle ) {
			switch ( _fontStyle ) {
				case Atom.FontStyle.kMTFontStyleDefault:		return @"mathnormal";
				case Atom.FontStyle.kMTFontStyleRoman:			return @"mathrm";
				case Atom.FontStyle.kMTFontStyleBold:			return @"mathbf";
				case Atom.FontStyle.kMTFontStyleFraktur:		return @"mathfrak";
				case Atom.FontStyle.kMTFontStyleCaligraphic:	return @"mathcal";
				case Atom.FontStyle.kMTFontStyleItalic:			return @"mathit";
				case Atom.FontStyle.kMTFontStyleSansSerif:		return @"mathsf";
				case Atom.FontStyle.kMTFontStyleBlackboard:		return @"mathbb";
				case Atom.FontStyle.kMTFontStyleTypewriter:		return @"mathtt";
				case Atom.FontStyle.kMTFontStyleBoldItalic:		return @"bm";
			}
			return null;
		}

		static Dictionary< string, Atom.FontStyle >	m_fontStyles = null;
		static Dictionary< string, Atom.FontStyle >	FontStyles {
			get {
				if ( m_fontStyles == null ) {
					m_fontStyles = new Dictionary<string, Atom.FontStyle>();
					m_fontStyles.Add( @"mathnormal",	Atom.FontStyle.kMTFontStyleDefault );
					m_fontStyles.Add( @"mathrm",		Atom.FontStyle.kMTFontStyleRoman );
					m_fontStyles.Add( @"textrm",		Atom.FontStyle.kMTFontStyleRoman );
					m_fontStyles.Add( @"rm",			Atom.FontStyle.kMTFontStyleTypewriter );
					m_fontStyles.Add( @"mathbf",		Atom.FontStyle.kMTFontStyleBold );
					m_fontStyles.Add( @"bf",			Atom.FontStyle.kMTFontStyleBold );
					m_fontStyles.Add( @"textbf",		Atom.FontStyle.kMTFontStyleBold );
					m_fontStyles.Add( @"mathcal",		Atom.FontStyle.kMTFontStyleCaligraphic );
					m_fontStyles.Add( @"cal",			Atom.FontStyle.kMTFontStyleCaligraphic );
					m_fontStyles.Add( @"mathtt",		Atom.FontStyle.kMTFontStyleTypewriter );
					m_fontStyles.Add( @"texttt",		Atom.FontStyle.kMTFontStyleTypewriter );
					m_fontStyles.Add( @"mathit",		Atom.FontStyle.kMTFontStyleItalic );
					m_fontStyles.Add( @"textit",		Atom.FontStyle.kMTFontStyleItalic );
					m_fontStyles.Add( @"mit",			Atom.FontStyle.kMTFontStyleItalic );
					m_fontStyles.Add( @"mathsf",		Atom.FontStyle.kMTFontStyleSansSerif );
					m_fontStyles.Add( @"textsf",		Atom.FontStyle.kMTFontStyleSansSerif );
					m_fontStyles.Add( @"mathfrak",		Atom.FontStyle.kMTFontStyleFraktur );
					m_fontStyles.Add( @"frak",			Atom.FontStyle.kMTFontStyleFraktur );
					m_fontStyles.Add( @"mathbb",		Atom.FontStyle.kMTFontStyleBlackboard );
					m_fontStyles.Add( @"mathbfit",		Atom.FontStyle.kMTFontStyleBoldItalic );
					m_fontStyles.Add( @"bm",			Atom.FontStyle.kMTFontStyleBoldItalic );
					m_fontStyles.Add( @"text",			Atom.FontStyle.kMTFontStyleRoman );
				}
				return m_fontStyles;
			}
		}

		#endregion

		#region Helpers

		const string	MTSymbolMultiplication = @"\u00D7";
		const string	MTSymbolDivision = @"\u00F7";
		const string	MTSymbolFractionSlash = @"\u2044";
		const string	MTSymbolWhiteSquare = @"\u25A1";
		const string	MTSymbolBlackSquare = @"\u25A0";
		const string	MTSymbolLessEqual = @"\u2264";
		const string	MTSymbolGreaterEqual = @"\u2265";
		const string	MTSymbolNotEqual = @"\u2260";
		const string	MTSymbolSquareRoot = @"\u221A"; // \sqrt
		const string	MTSymbolCubeRoot = @"\u221B";
		const string	MTSymbolInfinity = @"\u221E"; // \infty
		const string	MTSymbolAngle = @"\u2220"; // \angle
		const string	MTSymbolDegree = @"\u00B0"; // \circ

		static Atom				times					{ get { return Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, MTSymbolMultiplication ); } }
		static Atom				divide					{ get { return Atom.Create( Atom.TYPE.kMTMathAtomBinaryOperator, MTSymbolDivision ); } }
		static Atom				placeholder				{ get { return Atom.Create( Atom.TYPE.kMTMathAtomPlaceholder, MTSymbolWhiteSquare ); } }
		static AtomFraction		placeholderFraction		{ get { return new AtomFraction( true ) { numerator = new AtomsList( placeholder ), denominator = new AtomsList( placeholder ) }; } }
		static AtomRadical		placeholderRadical		{ get { return new AtomRadical() { degree = new AtomsList( placeholder ), radicand = new AtomsList( placeholder ) }; } }
		static AtomRadical		placeholderSquareRoot	{ get { return new AtomRadical() { radicand = new AtomsList( placeholder ) }; } }

		static AtomLargeOperator	OperatorWithName( string name, bool limits ) {
			return new AtomLargeOperator( name, limits );
		}

		#endregion
    }
}
