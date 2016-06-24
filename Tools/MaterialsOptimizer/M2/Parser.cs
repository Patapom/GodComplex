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
		public int		m_Index = 0;
		public char		Cur			{ get { return m_Content[m_Index]; } }
		public bool		OK			{ get { return m_Index < m_Content.Length; } }
		public string	Remainder	{ get { return m_Content.Remove( 0, m_Index ); } }

		public Parser( string _Content ) {
			m_Content = _Content;
		}

		public void	Error( string _Error ) {
			throw new Exception( _Error );
		}
		public void	ConsumeString( string _ExpectedString ) {
			if ( !SkipSpaces() )
				return;
			if ( m_Content.Substring( m_Index, _ExpectedString.Length ) != _ExpectedString )
				Error( "Unexpected string" );
			m_Index += _ExpectedString.Length;
		}
		public int		ReadInteger() {
			if ( !SkipSpaces() )
				return -1;
			int	StartIndex = m_Index;
			while ( IsNumeric() || IsChar( '-' ) ) {
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
			while ( IsNumeric() || IsChar( '-' ) || IsChar( '.' ) ) {
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
			while ( !IsSpace() && !IsEOL() ) {
				m_Index++;
			}

			string	Result = m_Content.Substring( StartIndex, m_Index-StartIndex );
			if ( _UnQuote )
				Result = UnQuote( Result, _unSemiColon );
			return Result;
		}
		public string	ReadBlock() {
			if ( !SkipSpaces() )
				return null;
			ConsumeString( "{" );
			int	StartIndex = m_Index;
			int	BracesCount = 1;
			while ( BracesCount > 0 ) {
				if ( IsChar( '{' ) ) BracesCount++;
				if ( IsChar( '}' ) ) BracesCount--;
				m_Index++;
			}
			string	Block = m_Content.Substring( StartIndex, m_Index-StartIndex-1 );	// Skip last closing brace
			return Block;
		}
		public string	ReadToEOL() {
			int	StartIndex = m_Index;
			while ( !IsEOL() ) {
				m_Index++;
			}
			string	Result = m_Content.Substring( StartIndex, m_Index-StartIndex );
			return Result;
		}
		public float3	ReadFloat3( float3 _InitialValue) {
			string	Block = ReadBlock();
			Parser	P = new Parser( Block );

			float3	Result = _InitialValue;
			string	coord = P.ReadString();
			while ( P.OK ) {
				P.ConsumeString( "= " );
				float	value = P.ReadFloat();
				P.ConsumeString( ";" );

				switch ( coord ) {
					case "x": Result.x = value; break;
					case "y": Result.y = value; break;
					case "z": Result.z = value; break;
					default: Error( "Unexpected coordinate!" ); break;
				}
				coord = P.ReadString();
			}
			return Result;
		}
		public bool	SkipSpaces() {
			while ( OK && (IsSpace() || IsEOL()) ) {
				m_Index++;
			}
			return OK;
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
