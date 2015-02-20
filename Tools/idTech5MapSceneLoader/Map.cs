using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using RendererManaged;

namespace idTech5Map
{
	public class Map
	{
		public class	Entity {

			public enum	 TYPE {
				UNKNOWN,
				MODEL,
				LIGHT,
				PROBE,
				PLAYER_START,
				REF_MAP,
			}

			public Map		m_Owner = null;
			public TYPE		m_Type = TYPE.UNKNOWN;
			public float4x4	m_Local2World = float4x4.Identity;
			public string	m_Name = "";

			public Model	m_Model = null;			// Only valid when m_Type == MODEL
			public string	m_RefMapName = null;	// Only valid when m_Type == REF_MAP
			public bool		m_IBL = false;			// Only valid when m_Type == PROBE

			public Entity( Map _Owner ) {
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

			#endregion
		}

		public List<Entity>	m_Entities = new List<Entity>();

		public Map( string _SourceFileName ) {

			// Setup base directory for files rebasing
			ms_BaseDirectory = Path.GetDirectoryName( _SourceFileName );

			// Parse entities, models & materials
			m_Entities.Clear();
			using ( StreamReader R = new FileInfo( _SourceFileName ).OpenText() ) {
				string	Content = R.ReadToEnd();
				Parse( Content );
			}
		}

		#region Map/RefMap Parsing

		private void	Parse( string _Content ) {
			Parser	P = new Parser( _Content );

			List<Entity>	Entities = new List<Entity>();

			P.ConsumeString( "Version" );
			int	Version = P.ReadInteger();
			if ( Version != 4 )
				P.Error( "Unsupported file version!" );

			while ( P.OK ) {
				P.ConsumeString( "entity" );
				string	Block = P.ReadBlock();

				Entity	E = new Entity( this );
				if ( E.Parse( Block ) ) {
					Entities.Add( E );
				}

				P.SkipSpaces();
			}

			m_Entities.AddRange( Entities );

			// Parse all refmaps
			foreach ( Entity E in Entities )
				if ( E.m_Type == Entity.TYPE.REF_MAP ) {
					using ( StreamReader R = new FileInfo( E.m_RefMapName ).OpenText() ) {
						string	Content = R.ReadToEnd();
						Parse( Content );
					}
				}
		}

		public static string	ms_BaseDirectory = null;	// Base directory where the map file is being parsed
		internal static string	RebaseFileName( string _FileName, string _Base, string _Extension ) {
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
}
