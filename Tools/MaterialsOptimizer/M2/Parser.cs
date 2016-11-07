using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RendererManaged;

namespace MaterialsOptimizer
{
	/// <summary>
	/// Small & lenient parser for .M2, .MAP, .REFMAP, etc.
	/// </summary>
	[System.Diagnostics.DebuggerDisplay( "C={Cur} -> {Remainder}" )]
	public class	Parser {
		public string	m_Content = null;
		public int		m_ContentLength = 0;
		public int		m_Index = 0;
		public char		Cur			{ get { return m_Content[m_Index]; } }
		public bool		OK			{ get { return m_Index < m_ContentLength; } }
		public string	Remainder	{ get { return m_Content.Remove( 0, m_Index ); } }

		public char		this[int _index] {
			get { return m_Content[m_Index+_index]; }
		}

		public Parser( string _Content ) {
			m_Content = _Content;
			m_ContentLength = m_Content.Length;
		}

		public void	Error( string _Error ) {
			throw new Exception( _Error );
		}
		public void	ConsumeString( string _ExpectedString, bool _caseSensitive ) {
			if ( !SkipSpaces() )
				return;
			string	subString = m_Content.Substring( m_Index, _ExpectedString.Length );
			if ( (_caseSensitive && subString != _ExpectedString) || (!_caseSensitive && subString.ToLower() != _ExpectedString.ToLower()) )
				Error( "Unexpected string" );
			m_Index += _ExpectedString.Length;
		}
		public int		ReadInteger() {
			if ( !SkipSpaces() )
				return -1;
			int	StartIndex = m_Index;
			while ( OK && (IsNumeric() || IsChar( '-' )) ) {
				m_Index++;
			}

			string	Number = m_Content.Substring( StartIndex, m_Index-StartIndex );
			int		Result = int.Parse( Number );
			return Result;
		}
		public float	ReadFloat() {
			if ( !SkipSpaces() )
				return 0.0f;
			int	StartIndex = m_Index;
			while ( OK && (IsNumeric() || IsChar( '-' ) || IsChar( '.' )) ) {
				m_Index++;
			}

			string	Number = m_Content.Substring( StartIndex, m_Index-StartIndex );
			float	Result = float.Parse( Number );
			return Result;
		}
		public string	ReadString() {
			return ReadString( false, false );
		}
		public string	ReadString( bool _UnQuote, bool _unSemiColon ) {
			if ( !SkipSpaces() )
				return null;
			int	StartIndex = m_Index;
			while ( OK && !IsSpace() && !IsEOL() ) {
				m_Index++;
			}

			string	Result = m_Content.Substring( StartIndex, m_Index-StartIndex );
			if ( _UnQuote )
				Result = UnQuote( Result, _unSemiColon );
			return Result;
		}
		public string	ReadBlock() {
			return ReadBlock( '{', '}' );
		}
		public string	ReadBlock( char _blockStartCharacter, char _blockEndCharacter ) {
			if ( !SkipSpaces() )
				return null;
			ConsumeString( _blockStartCharacter.ToString(), true );
			int	StartIndex = m_Index;
			int	BracesCount = 1;
			while ( BracesCount > 0 ) {
				if ( IsChar( _blockStartCharacter ) ) BracesCount++;
				if ( IsChar( _blockEndCharacter ) ) BracesCount--;
				m_Index++;
			}
			string	Block = m_Content.Substring( StartIndex, m_Index-StartIndex-1 );	// Skip last closing brace
			return Block;
		}
		public string	ReadToEOL() {
			int	StartIndex = m_Index;
			while ( OK && !IsEOL() ) {
				m_Index++;
			}
			string	Result = m_Content.Substring( StartIndex, m_Index-StartIndex );
			return Result;
		}

		// Reads a float2 in the form { <x>, <y>, <z> }
		public float2	ReadFloat2() {
			string	Block = ReadBlock();
			return ReadFloat2( Block );
		}
		public float2	ReadFloat2( string _block ) {
			Parser	P = new Parser( _block );
			float2	Result = new float2();

			int		coordinateIndex = 0;
			while ( P.OK ) {
				float	value = P.ReadFloat();
				P.ReadString();	// Skip separator

				switch ( coordinateIndex ) {
					case 0: Result.x = value; break;
					case 1: Result.y = value; break;
					default: Error( "Unexpected coordinate!" ); break;
				}
				
				coordinateIndex++;
			}
			return Result;
		}

		// Reads a float3 in the form { <x>, <y>, <z> }
		public float3	ReadFloat3() {
			string	Block = ReadBlock();
			return ReadFloat3( Block );
		}
		public float3	ReadFloat3( string _block ) {
			Parser	P = new Parser( _block );
			float3	Result = new float3();

			int		coordinateIndex = 0;
			while ( P.OK ) {
				float	value = P.ReadFloat();
				P.ReadString();	// Skip separator

				switch ( coordinateIndex ) {
					case 0: Result.x = value; break;
					case 1: Result.y = value; break;
					case 2: Result.z = value; break;
					default: Error( "Unexpected coordinate!" ); break;
				}
				
				coordinateIndex++;
			}
			return Result;
		}

		// Reads a float3 in the form { <x>, <y>, <z>, <w> }
		public float4	ReadFloat4() {
			string	Block = ReadBlock();
			return ReadFloat4( Block );
		}
		public float4	ReadFloat4( string _block ) {
			Parser	P = new Parser( _block );

			float4	Result = new float4();

			int		coordinateIndex = 0;
			while ( P.OK ) {
				float	value = P.ReadFloat();
				P.ReadString();	// Skip separator

				switch ( coordinateIndex ) {
					case 0: Result.x = value; break;
					case 1: Result.y = value; break;
					case 2: Result.z = value; break;
					case 3: Result.w = value; break;
					default: Error( "Unexpected coordinate!" ); break;
				}
				
				coordinateIndex++;
			}
			return Result;
		}

		public bool	SkipSpaces() {
			while ( OK && (IsSpace() || IsEOL()) ) {
				m_Index++;
			}
			return OK;
		}
		public string	SkipComment() {
			int	startIndex = m_Index;
			while ( OK ) {
				if ( m_Index >= m_ContentLength-2 ) {
					m_Index = m_ContentLength;
					return m_Content.Substring( startIndex, m_Index - startIndex );	// End of text
				}
				if ( m_Content[m_Index] == '*' && m_Content[m_Index+1] == '/' ) {
					m_Index += 2;
					return m_Content.Substring( startIndex, m_Index - startIndex );
				}
				m_Index++;
			}
			return m_Content.Substring( startIndex, m_Index - startIndex );
		}
		public bool	IsChar( char _Char ) {
			return Cur == _Char;
		}
		public bool	IsSpace() {
			return Cur == ' ' || Cur == '\t';
		}
		public bool	IsNumeric() {
			return Cur >= '0' && Cur <= '9';
		}
		public bool	IsEOL() {
			return Cur == '\r' || Cur == '\n';
		}
		public string	UnQuote( string s, bool _unSemiColon ) {
			if ( s[0] == '\"' ) s = s.Remove( 0, 1 );
			if ( _unSemiColon && s[s.Length-1] == ';' ) s = s.Remove( s.Length-2 );
			if ( s[s.Length-1] == '\"' ) s = s.Remove( s.Length-2 );
			return s;
		}
	}

}
