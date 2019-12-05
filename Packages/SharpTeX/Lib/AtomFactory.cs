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
    public static class AtomFactory {

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
				return Atom.CreateAtomWithType( Atom.TYPE.kMTMathAtomOrdinary, chStr );	// show basic cyrillic alphabet. Latin Modern Math font is not good for cyrillic symbols
			} else if ( ch < 0x21 || ch > 0x7E ) {
				return null;	// skip non ascii characters and spaces
			} else if ( ch == '$' || ch == '%' || ch == '#' || ch == '&' || ch == '~' || ch == '\'' ) {
				return null;	// These are latex control characters that have special meanings. We don't support them.
			} else if ( ch == '^' || ch == '_' || ch == '{' || ch == '}' || ch == '\\' ) {
				return null;		// more special characters for Latex.
			} else if ( ch == '(' || ch == '[' ) {
				return Atom.CreateAtomWithType( Atom.TYPE.kMTMathAtomOpen, chStr );
			} else if (ch == ')' || ch == ']' || ch == '!' || ch == '?') {
				return Atom.CreateAtomWithType( Atom.TYPE.kMTMathAtomClose, chStr );
			} else if (ch == ',' || ch == ';') {
				return Atom.CreateAtomWithType( Atom.TYPE.kMTMathAtomPunctuation, chStr );
			} else if (ch == '=' || ch == '>' || ch == '<') {
				return Atom.CreateAtomWithType( Atom.TYPE.kMTMathAtomRelation, chStr );
			} else if (ch == ':') {
				return Atom.CreateAtomWithType( Atom.TYPE.kMTMathAtomRelation, @"\u2236" );	// Math colon is ratio. Regular colon is \colon
			} else if (ch == '-') {
				return Atom.CreateAtomWithType( Atom.TYPE.kMTMathAtomBinaryOperator, @"\u2212" );	// Use the math minus sign
			} else if (ch == '+' || ch == '*') {
				return Atom.CreateAtomWithType( Atom.TYPE.kMTMathAtomBinaryOperator, chStr );
			} else if (ch == '.' || (ch >= '0' && ch <= '9')) {
				return Atom.CreateAtomWithType( Atom.TYPE.kMTMathAtomNumber, chStr );
			} else if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')) {
				return Atom.CreateAtomWithType( Atom.TYPE.kMTMathAtomVariable, chStr );
			} else if (ch == '"' || ch == '/' || ch == '@' || ch == '`' || ch == '|') {
				return Atom.CreateAtomWithType( Atom.TYPE.kMTMathAtomOrdinary, chStr );	// just an ordinary character. The following are allowed ordinary chars | / ` @ "
			} else {
				throw new Exception( @"Unknown ascii character '" + ch + "'. Should have been accounted for." );
			}
		}

		public static Atom	AtomForLatexSymbolName( string symbolName ) {
			if ( symbolName == null ) throw new Exception( "Invalid symbol!" );

			// First check if this is an alias
			Dictionary< string, string >	aliases = Aliases;
			string							canonicalName = aliases[symbolName];
			if ( canonicalName != null ) {
				symbolName = canonicalName;		// Switch to the canonical name
			}
    
			Dictionary< string, Atom >	commands = SupportedLatexSymbols;
			Atom						atom = commands[symbolName];
			if ( atom == null )
				return null;

			// Return a copy of the atom since atoms are mutable.
			atom = atom.Copy();
			return atom;
		}

		public static Atom	LatexSymbolNameForAtom( Atom atom ) {
			if ( atom.nucleus.Length == 0) {
				return null;
			}

			Dictionary< string, Atom >	dict = TextToLatexSymbolNames;
			return dict.ContainsKey( atom.nucleus ) ? dict[atom.nucleus] : null;
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
					m_fontStyles = new Dictionary<string, FontStyle>();
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
    }
}
