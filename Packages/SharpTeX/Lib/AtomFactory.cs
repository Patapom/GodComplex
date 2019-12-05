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

		public static AtomsList	mathListForCharacters( string chars ) {
			if ( chars == null ) throw new Exception( "Invalid characters!" );

// 			char	buff = new char[chars.Length];
// 			[chars getCharacters:buff range:NSMakeRange(0, len)];

			AtomsList	list = new AtomsList();
			for ( int i=0; i < chars.Length; i++ ) {
				Atom	atom = atomForCharacter( chars[i] );
				if ( atom != null ) {
					list.AddAtom( atom );
				}
			}
			return list;
		}

		Atom	atomForCharacter( char ch ) {
			string	chStr = ch.ToString();
			if ( ch > 0x0410 && ch < 0x044F ){
				return Atom.CreateAtomWithType( TYPE.kMTMathAtomOrdinary, chStr );	// show basic cyrillic alphabet. Latin Modern Math font is not good for cyrillic symbols
			} else if ( ch < 0x21 || ch > 0x7E ) {
				return null;	// skip non ascii characters and spaces
			} else if ( ch == '$' || ch == '%' || ch == '#' || ch == '&' || ch == '~' || ch == '\'' ) {
				return null;	// These are latex control characters that have special meanings. We don't support them.
			} else if ( ch == '^' || ch == '_' || ch == '{' || ch == '}' || ch == '\\' ) {
				return null;		// more special characters for Latex.
			} else if ( ch == '(' || ch == '[' ) {
				return Atom.CreateAtomWithType( TYPE.kMTMathAtomOpen, chStr );
			} else if (ch == ')' || ch == ']' || ch == '!' || ch == '?') {
				return Atom.CreateAtomWithType( TYPE.kMTMathAtomClose, chStr );
			} else if (ch == ',' || ch == ';') {
				return Atom.CreateAtomWithType( TYPE.kMTMathAtomPunctuation, chStr );
			} else if (ch == '=' || ch == '>' || ch == '<') {
				return Atom.CreateAtomWithType( TYPE.kMTMathAtomRelation, chStr );
			} else if (ch == ':') {
				return Atom.CreateAtomWithType( TYPE.kMTMathAtomRelation, @"\u2236" );	// Math colon is ratio. Regular colon is \colon
			} else if (ch == '-') {
				return Atom.CreateAtomWithType( TYPE.kMTMathAtomBinaryOperator, @"\u2212" );	// Use the math minus sign
			} else if (ch == '+' || ch == '*') {
				return Atom.CreateAtomWithType( TYPE.kMTMathAtomBinaryOperator, chStr );
			} else if (ch == '.' || (ch >= '0' && ch <= '9')) {
				return Atom.CreateAtomWithType( TYPE.kMTMathAtomNumber, chStr );
			} else if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')) {
				return Atom.CreateAtomWithType( TYPE.kMTMathAtomVariable, chStr );
			} else if (ch == '"' || ch == '/' || ch == '@' || ch == '`' || ch == '|') {
				return Atom.CreateAtomWithType( TYPE.kMTMathAtomOrdinary, chStr );	// just an ordinary character. The following are allowed ordinary chars | / ` @ "
			} else {
				throw new Exception( @"Unknown ascii character '" + ch + "'. Should have been accounted for." );
			}
		}

		Atom	atomForLatexSymbolName( string symbolName ) {
			if ( symbolName == null ) throw new Exception( "Invalid symbol!" );

			Dictionary	aliases = [MTMathAtomFactory aliases];
			// First check if this is an alias
			NSString* canonicalName = aliases[symbolName];
			if (canonicalName) {
				// Switch to the canonical name
				symbolName = canonicalName;
			}
    
			NSDictionary* commands = [self supportedLatexSymbols];
			MTMathAtom* atom = commands[symbolName];
			if (atom) {
				// Return a copy of the atom since atoms are mutable.
				return [atom copy];
			}
			return nil;
		}

		+ (nullable NSString*) latexSymbolNameForAtom:(MTMathAtom*) atom
		{
			if (atom.nucleus.length == 0) {
				return nil;
			}
			NSDictionary* dict = [MTMathAtomFactory textToLatexSymbolNames];
			return dict[atom.nucleus];
		}

		#endregion

		#region Font Styles

		public static FontStyle	FontStyleWithName( string _fontName ) {
			if ( !FontStyles.ContainsKey( _fontName ) )
				return FontStyle.NOT_FOUND;

			return FontStyles[_fontName];
		}

		string	FontNameForStyle( FontStyle fontStyle ) {
			switch (fontStyle) {
				case FontStyle.kMTFontStyleDefault:		return @"mathnormal";
				case FontStyle.kMTFontStyleRoman:		return @"mathrm";
				case FontStyle.kMTFontStyleBold:		return @"mathbf";
				case FontStyle.kMTFontStyleFraktur:		return @"mathfrak";
				case FontStyle.kMTFontStyleCaligraphic:	return @"mathcal";
				case FontStyle.kMTFontStyleItalic:		return @"mathit";
				case FontStyle.kMTFontStyleSansSerif:	return @"mathsf";
				case FontStyle.kMTFontStyleBlackboard:	return @"mathbb";
				case FontStyle.kMTFontStyleTypewriter:	return @"mathtt";
				case FontStyle.kMTFontStyleBoldItalic:	return @"bm";
			}
			return null;
		}

		static Dictionary< string, FontStyle >	m_fontStyles = null;
		static Dictionary< string, FontStyle >	FontStyles {
			get {
			if ( m_fontStyles == null ) {
				m_fontStyles = new Dictionary<string, FontStyle>();
				m_fontStyles.Add( @"mathnormal",	FontStyle.kMTFontStyleDefault );
				m_fontStyles.Add( @"mathrm",		FontStyle.kMTFontStyleRoman );
				m_fontStyles.Add( @"textrm",		FontStyle.kMTFontStyleRoman );
				m_fontStyles.Add( @"rm",			FontStyle.kMTFontStyleTypewriter );
				m_fontStyles.Add( @"mathbf",		FontStyle.kMTFontStyleBold );
				m_fontStyles.Add( @"bf",			FontStyle.kMTFontStyleBold );
				m_fontStyles.Add( @"textbf",		FontStyle.kMTFontStyleBold );
				m_fontStyles.Add( @"mathcal",		FontStyle.kMTFontStyleCaligraphic );
				m_fontStyles.Add( @"cal",			FontStyle.kMTFontStyleCaligraphic );
				m_fontStyles.Add( @"mathtt",		FontStyle.kMTFontStyleTypewriter );
				m_fontStyles.Add( @"texttt",		FontStyle.kMTFontStyleTypewriter );
				m_fontStyles.Add( @"mathit",		FontStyle.kMTFontStyleItalic );
				m_fontStyles.Add( @"textit",		FontStyle.kMTFontStyleItalic );
				m_fontStyles.Add( @"mit",			FontStyle.kMTFontStyleItalic );
				m_fontStyles.Add( @"mathsf",		FontStyle.kMTFontStyleSansSerif );
				m_fontStyles.Add( @"textsf",		FontStyle.kMTFontStyleSansSerif );
				m_fontStyles.Add( @"mathfrak",		FontStyle.kMTFontStyleFraktur );
				m_fontStyles.Add( @"frak",			FontStyle.kMTFontStyleFraktur );
				m_fontStyles.Add( @"mathbb",		FontStyle.kMTFontStyleBlackboard );
				m_fontStyles.Add( @"mathbfit",		FontStyle.kMTFontStyleBoldItalic );
				m_fontStyles.Add( @"bm",			FontStyle.kMTFontStyleBoldItalic );
				m_fontStyles.Add( @"text",			FontStyle.kMTFontStyleRoman );
			}
			return m_fontStyles;
		}

		#endregion
    }
}
