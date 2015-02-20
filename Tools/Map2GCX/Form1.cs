//////////////////////////////////////////////////////////////////////////
// Converts idTech5 map, bmodel and m2 formats into a readable GCX scene
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using RendererManaged;

namespace Map2GCX
{
	public partial class Form1 : Form
	{
		class	MapEntity {

			public enum	 TYPE {
				UNKNOWN,
				MODEL,
				LIGHT,
				PROBE,
				PLAYER_START,
				REF_MAP,
			}

			public static string	ms_BaseDirectory = null;	// Base directory where the map file is being parsed

			public Form1	m_Owner = null;
			public TYPE		m_Type = TYPE.UNKNOWN;
			public float4x4	m_Local2World = float4x4.Identity;
			public string	m_Name = "";

			public Model	m_Model = null;			// Only valid when m_Type == MODEL
			public string	m_RefMapName = null;	// Only valid when m_Type == REF_MAP
			public bool		m_IBL = false;			// Only valid when m_Type == PROBE

			public MapEntity( Form1 _Owner ) {
				m_Owner = _Owner;
			}

			#region Parsing

			public bool		Parse( string _Block ) {
				Parser	P = new Parser( _Block );
				P.ConsumeString( "entityDef" );
				m_Name = P.ReadString();

				string	entityDef = P.ReadBlock();

				return ParseEntityDef( entityDef );
			}

			private bool	ParseEntityDef( string _Block ) {
				Parser	P = new Parser( _Block );
				P.ConsumeString( "inherit =" );
				string	inherit = P.ReadString( true, true );

				if ( inherit == "func/static" ) {
					m_Type = TYPE.MODEL;
				} else if ( inherit == "player/start" ) {
					m_Type = TYPE.PLAYER_START;
				} else if ( inherit == "blacksparrow/probe" ) {
					m_Type = TYPE.PROBE;
				} else if ( inherit == "func/reference" ) {
					m_Type = TYPE.REF_MAP;
				}
				if ( m_Type == TYPE.UNKNOWN )
					return false;

				P.ConsumeString( "editorVars" );
				P.ReadBlock();
				P.ConsumeString( "edit =" );

				string	Content = P.ReadBlock();
				ParseContent( Content );

				return true;
			}

			private void	ParseContent( string _Content ) {
				Parser	P = new Parser( _Content );

				string	property = P.ReadString();
				while ( P.OK ) {
					switch ( property ) {
						case "spawnPosition":
							P.ConsumeString( "= " );
							float3	Position = P.ReadFloat3( float3.Zero );
							m_Local2World.r0.w = Position.x;
							m_Local2World.r1.w = Position.y;
							m_Local2World.r2.w = Position.z;
							break;
						case "spawnOrientation":
							P.ConsumeString( "= " );
							string	Orientation = P.ReadBlock();
							ParseOrientation( Orientation );
							break;
						case "renderModelInfo":
							P.ConsumeString( "= ! " );
							string	Model = P.ReadBlock();
							ParseModel( Model );
							break;
						case "m_probe":
							P.ConsumeString( "= " );
							string	ProbeName = P.UnQuote( P.ReadToEOL(), true );
							break;
						case "mapname":
							P.ConsumeString( "= " );
							m_RefMapName = RebaseFileName( P.UnQuote( P.ReadToEOL(), true ), "T:/", null );
							break;
						case "m_usedForIBL":
							P.ReadToEOL();
							m_IBL = true;
							break;

						// Don't care... (single line)
						case "entityPrefix":
							P.ReadToEOL();
							break;

						// Don't care... (block)
						case "m_bounceFactorSun":
						case "m_kiscule":
							P.ConsumeString( "= " );
							P.ReadBlock();
							break;

						default:
							P.Error( "Unexpected property!" );
							break;
					}
					property = P.ReadString();
				}
			}

			private void	ParseOrientation( string _Orientation ) {
				Parser	P = new Parser( _Orientation );
				P.ConsumeString( "mat = " );

				string	Rows = P.ReadBlock();

				P = new Parser( Rows );
				string	row = P.ReadString();
				while ( P.OK ) {
					P.ConsumeString( "= " );
					switch ( row ) {
						case "mat[0]": {
							float3	value = P.ReadFloat3( float3.UnitX );
							m_Local2World.r0.Set( value, m_Local2World.r0.w );
							break;
						}
						case "mat[1]": {
							float3	value = P.ReadFloat3( float3.UnitY );
							m_Local2World.r1.Set( value, m_Local2World.r1.w );
							break;
						}
						case "mat[2]": {
							float3	value = P.ReadFloat3( float3.UnitZ );
							m_Local2World.r2.Set( value, m_Local2World.r2.w );
							break;
						}
						default: P.Error( "Unexpected coordinate!" ); break;
					}
					row = P.ReadString();
				}
			}

			private void	ParseModel( string _Model ) {
				Parser	P = new Parser( _Model );
				P.ConsumeString( "model = " );

				string	ModelFileName = RebaseFileName( P.ReadString( true, true ), "T:/generated/model/", ".bmodel" );
				m_Model = new Model( ModelFileName );
			}

			private string	RebaseFileName( string _FileName, string _Base, string _Extension ) {
				string	Directory = Path.GetDirectoryName( _FileName );
				string	FileName = Path.GetFileNameWithoutExtension( _FileName );
				string	Extension = _Extension != null ? _Extension : Path.GetExtension( _FileName );
				FileName = FileName + Extension;
				Directory = Path.Combine( _Base, Directory );

				// Now, we can find the file in either one of these 2 directories...
				string	AbsolutePath = Path.Combine( Directory, FileName );
				string	LocalPath = Path.Combine( ms_BaseDirectory, FileName );

				if ( File.Exists( AbsolutePath ) )
					return AbsolutePath;
				else if ( File.Exists( LocalPath ) )
					return LocalPath;
				else
					throw new Exception( "Target file cannot be found!" );
			}

			#endregion
		}

		List<MapEntity>		m_Entities = new List<MapEntity>();

		public Form1()
		{
			InitializeComponent();
			ConvertMap( @"..\Arkane\SimpleMapWithManyProbes\test_probes_p - Fixed.map", @"..\Arkane\SimpleMapWithManyProbes\scene.gcx" );
		}

		#region Map/RefMap Parsing

		private void	ConvertMap( string _SourceFileName, string _TargetFileName ) {

			MapEntity.ms_BaseDirectory = Path.GetDirectoryName( _SourceFileName );

			m_Entities.Clear();
			using ( StreamReader R = new FileInfo( _SourceFileName ).OpenText() ) {
				string	Content = R.ReadToEnd();
				ParseMap( Content );
			}

			// Parse all refmaps
			MapEntity[]	ExistingEntities = m_Entities.ToArray();
			foreach ( MapEntity E in ExistingEntities )
				if ( E.m_Type == MapEntity.TYPE.REF_MAP ) {
					using ( StreamReader R = new FileInfo( E.m_RefMapName ).OpenText() ) {
						string	Content = R.ReadToEnd();
						ParseMap( Content );
					}
				}
		}

		private void	ParseMap( string _Content ) {
			Parser	P = new Parser( _Content );

			P.ConsumeString( "Version" );
			int	Version = P.ReadInteger();
			if ( Version != 4 )
				P.Error( "Unsupported file version!" );

			while ( P.OK ) {
				P.ConsumeString( "entity" );
				string	Block = P.ReadBlock();

				MapEntity	E = new MapEntity( this );
				if ( E.Parse( Block ) ) {
					m_Entities.Add( E );
				}

				P.SkipSpaces();
			}
		}

		[System.Diagnostics.DebuggerDisplay( "C={Cur} -> {Remainder}" )]
		class	Parser {
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

		#endregion
	}
}
