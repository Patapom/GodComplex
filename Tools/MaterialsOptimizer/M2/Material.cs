using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using RendererManaged;

namespace MaterialsOptimizer
{
	public class Material {
		public class	Programs {
			public string		m_main;
			public string		m_ZPrepass;
			public string		m_shadow;
		}

		public class	Layer {
			[System.Diagnostics.DebuggerDisplay( "{m_name}" )]
			public class	Texture {
				public string	m_name;

				public	Texture( string _textureName ) {
					m_name = _textureName;
				}
			}
			public Texture	m_diffuse = null;
			public Texture	m_normal = null;
			public Texture	m_gloss = null;
			public Texture	m_metal = null;

// 			public void		Parse( string _texture ) {
// 				Parser	P = new Parser( _texture );
// 				while ( P.OK ) {
// 					string	token = P.ReadString();
// 					switch ( token.ToLower() ) {
// 						case "diffusemap":
// 							if ( m_diffuse != null ) throw new Exception( "Material has 2 diffuse textures!" );
// 							m_diffuse = new Texture( P.ReadToEOL() );
// 							break;
// 						case "bumpmap":
// 							if ( m_normal != null ) throw new Exception( "Material has 2 normal textures!" );
// 							m_normal = new Texture( P.ReadToEOL() );
// 							break;
// 						case "glossmap":
// 							if ( m_gloss != null ) throw new Exception( "Material has 2 gloss textures!" );
// 							m_gloss = new Texture( P.ReadToEOL() );
// 							break;
// 						case "metallicmap":
// 							if ( m_metal != null ) throw new Exception( "Material has 2 metal textures!" );
// 							m_metal = new Texture( P.ReadToEOL() );
// 							break;
// 					}
//  				} 
// 			}
		}

		public FileInfo			m_sourceFileName = null;
		public string			m_name = null;

		public Programs			m_programs = new Programs();

		public List< Layer >	m_layers = new List< Layer >();

		public Material( FileInfo _sourceFileName, string _name, string _content ) {
			m_sourceFileName = _sourceFileName;
			m_name = _name;
			m_layers.Add( new Layer() );	// We always have at least 1 layer
			Parse( _content );
		}

		#region Material Parsing

		private void	Parse( string _block ) {
			Parser	P = new Parser( _block );
			while ( P.OK ) {
				string	token = P.ReadString();
				if ( token == null )
					break;	// Done!

				switch ( token.ToLower() ) {
					case "parms":
						ParseParms( P.ReadBlock() );
						break;

					case "options":
						ParseOptions( P.ReadBlock() );
						break;

					case "mainprogram": m_programs.m_main = P.ReadString(); break;
					case "zprepassprogram": m_programs.m_ZPrepass = P.ReadString(); break;
					case "shadowprogram": m_programs.m_shadow = P.ReadString(); break;

					case "diffusemap":	m_layers[0].m_diffuse = new Layer.Texture( P.ReadString() ); break;
					case "bumpmap":		m_layers[0].m_normal = new Layer.Texture( P.ReadString() ); break;
					case "glossmap":	m_layers[0].m_gloss = new Layer.Texture( P.ReadString() ); break;
					case "metallicmap":	m_layers[0].m_metal = new Layer.Texture( P.ReadString() ); break;

// 					case "m_physicsmaterial":
// 					case "version":
					default:
						P.ReadToEOL();	// Don't care...
						break;
				}
			}
		}

		private void	ParseParms( string _parms ) {

		}

		private void	ParseOptions( string _options ) {

		}

// 		private bool	ParseEntityDef( string _Block ) {
// 			Parser	P = new Parser( _Block );
// 			P.ConsumeString( "inherit =" );
// 			string	inherit = P.ReadString( true, true );
// 
// 			if ( inherit == "func/static" ) {
// 				m_Type = TYPE.MODEL;
// 			} else if ( inherit == "player/start" ) {
// 				m_Type = TYPE.PLAYER_START;
// 			} else if ( inherit == "blacksparrow/probe" ) {
// 				m_Type = TYPE.PROBE;
// 			} else if ( inherit == "func/reference" ) {
// 				m_Type = TYPE.REF_MAP;
// 			}
// 			if ( m_Type == TYPE.UNKNOWN )
// 				return false;
// 
// 			P.ConsumeString( "editorVars" );
// 			P.ReadBlock();
// 			P.ConsumeString( "edit =" );
// 
// 			string	Content = P.ReadBlock();
// 			ParseContent( Content );
// 
// 			return true;
// 		}

		#endregion

		#region Materials Database

// 		public static List< Material >					ms_Materials = new List< Material >();
// 		public static Dictionary< string, Material >	ms_Name2Material = new Dictionary< string, Material >();
// 		public static Material	Find( string _SourceFileName ) {
// 			if ( _SourceFileName == null )
// 				return null;
// 
// 			_SourceFileName = _SourceFileName.ToLower();
// 			if ( ms_Name2Material.ContainsKey( _SourceFileName ) )
// 				return ms_Name2Material[_SourceFileName];
// 
// 			// Create a new material
// 			Material	M = new Material( _SourceFileName, ms_Name2Material.Count );
// 			ms_Name2Material.Add( _SourceFileName, M );
// 			ms_Materials.Add( M );	// In order
// 			return M;
// 		}

		#endregion
	}
}
