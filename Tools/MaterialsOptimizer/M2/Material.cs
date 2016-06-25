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
				public FileInfo	m_fileName = null;

				public bool		m_isConstantColor = false;
				public float4	m_constantColor = float4.Zero;

				public static	DirectoryInfo	ms_TexturesBasePath = null;

				public	Texture( string _textureName ) {
					m_name = _textureName;

					// Check if it's a constant color
					_textureName = _textureName.ToLower();
					if ( _textureName.StartsWith( "_" ) ) {
						// Procedural texture
						m_isConstantColor = true;
						switch ( _textureName ) {
							case "_black":				m_constantColor = new float4( 0, 0, 0, 0 ); break;
							case "_blackalphawhite":	m_constantColor = new float4( 0, 0, 0, 1 ); break;
							case "_white":				m_constantColor = new float4( 1, 1, 1, 1 ); break;
							default: throw new Exception( "Unsupported procedural texture type \"" + _textureName + "\"!" );
						} 
					} else if ( _textureName.StartsWith( "ipr_constantcolor" ) ) {
						m_isConstantColor = true;
						Parser	P = new Parser( _textureName );
						P.ConsumeString( "ipr_constantColor", false );
						string	strColor = P.ReadBlock( '(', ')' );
						m_constantColor = P.ReadFloat4();
					}

					if ( !m_isConstantColor ) {
						// Build file name
						string	fullPath = Path.Combine( ms_TexturesBasePath.FullName, m_name );
						m_fileName = new FileInfo( fullPath );
					}
				}
			}
			public enum		UV_SET {
				UV0,
				UV1,
			}
			public enum		MASKING_MODE {
				NONE,
				VERTEX_COLOR,
				MASK_MAP,
				MASK_MAP_AND_VERTEX_COLOR,
			}

			public Texture		m_diffuse = null;
			public Texture		m_normal = null;
			public Texture		m_gloss = null;
			public Texture		m_metal = null;
			public Texture		m_AO = null;

			public MASKING_MODE	m_maskingMode = MASKING_MODE.NONE;
			public Texture		m_mask = null;				// Layer mask

			public UV_SET		m_UVSet = UV_SET.UV0;
			public float2		m_UVOffset = float2.Zero;
			public float2		m_UVScale = float2.One;

			public UV_SET		m_maskUVSet = UV_SET.UV0;
			public float2		m_maskUVOffset = float2.Zero;
			public float2		m_maskUVScale = float2.One;

			public void			ParseScaleBias( Parser _P ) {
				float4	SB = _P.ReadFloat4();
				m_UVScale.Set( SB.x, SB.y );
				m_UVOffset.Set( SB.z, SB.w );
			}

			public void			ParseMaskScaleBias( Parser _P ) {
				float4	SB = _P.ReadFloat4();
				m_maskUVScale.Set( SB.x, SB.y );
				m_maskUVOffset.Set( SB.z, SB.w );
			}
		}

		public FileInfo			m_sourceFileName = null;
		public string			m_name = null;

		public Programs			m_programs = new Programs();

		public List< Layer >	m_layers = new List< Layer >();

		// Main variables
		public float2			m_glossMinMax = new float2( 0.0f, 0.5f );
		public float2			m_metallicMinMax = new float2( 0.0f, 0.5f );

		private Layer			Layer0 {
			get { return m_layers[0]; }
		}

		private Layer			Layer1 {
			get {
				if ( m_layers.Count < 1 )
					m_layers.Add( new Layer() );
				return m_layers[1];
			}
		}

		private Layer			Layer2 {
			get {
				if ( m_layers.Count < 2 ) {
					if ( m_layers.Count < 1 )
						m_layers.Add( new Layer() );
					m_layers.Add( new Layer() );
				}
				return m_layers[2];
			}
		}

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

					// Programs
					case "mainprogram": m_programs.m_main = P.ReadString(); break;
					case "zprepassprogram": m_programs.m_ZPrepass = P.ReadString(); break;
					case "shadowprogram": m_programs.m_shadow = P.ReadString(); break;

					// Textures
						// Layer 0
					case "diffusemap":			m_layers[0].m_diffuse = new Layer.Texture( P.ReadString() ); break;
					case "bumpmap":				m_layers[0].m_normal = new Layer.Texture( P.ReadString() ); break;
					case "glossmap":			m_layers[0].m_gloss = new Layer.Texture( P.ReadString() ); break;
					case "metallicmap":			m_layers[0].m_metal = new Layer.Texture( P.ReadString() ); break;
					case "occlusionmap":		m_layers[0].m_AO = new Layer.Texture( P.ReadString() ); break;
					case "layer0_maskmap":		m_layers[0].m_mask = new Layer.Texture( P.ReadString() ); break;
					case "layer0_scalebias":	m_layers[0].ParseScaleBias( P ); break;
					case "layer0_maskscalebias":m_layers[0].ParseMaskScaleBias( P ); break;

						// Layer 1
					case "layer1_diffusemap":	Layer1.m_diffuse = new Layer.Texture( P.ReadString() ); break;
					case "layer1_bumpmap":		Layer1.m_normal = new Layer.Texture( P.ReadString() ); break;
					case "layer1_glossmap":		Layer1.m_gloss = new Layer.Texture( P.ReadString() ); break;
					case "layer1_metallicmap":	Layer1.m_metal = new Layer.Texture( P.ReadString() ); break;
					case "layer1_maskmap":		Layer1.m_mask = new Layer.Texture( P.ReadString() ); break;
					case "layer1_scalebias":	Layer1.ParseScaleBias( P ); break;
					case "layer1_maskscalebias":Layer1.ParseMaskScaleBias( P ); break;

						// Layer 2
					case "layer2_diffusemap":	Layer2.m_diffuse = new Layer.Texture( P.ReadString() ); break;
					case "layer2_bumpmap":		Layer2.m_normal = new Layer.Texture( P.ReadString() ); break;
					case "layer2_glossmap":		Layer2.m_gloss = new Layer.Texture( P.ReadString() ); break;
					case "layer2_metallicmap":	Layer2.m_metal = new Layer.Texture( P.ReadString() ); break;
					case "layer2_maskmap":		Layer2.m_mask = new Layer.Texture( P.ReadString() ); break;
					case "layer2_scalebias":	Layer2.ParseScaleBias( P ); break;
					case "layer2_maskscalebias":Layer2.ParseMaskScaleBias( P ); break;


					// Main variables
					case "wardroughness":
						float4	roughness = P.ReadFloat4();
						m_glossMinMax.x = roughness.x;
						m_glossMinMax.y = roughness.y;
						break;
					case "metallicminmax":
						float4	metal = P.ReadFloat4();
						m_metallicMinMax.x = metal.x;
						m_metallicMinMax.y = metal.y;
						break;

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
			Parser	P = new Parser( _options );
			while ( P.OK ) {
				string	token = P.ReadString();
				if ( token == null )
					break;	// Done!

				int		value = P.ReadInteger();

				switch ( token.ToLower() ) {
					case "extra_layers":
						switch ( value ) {
							case 0: Layer0.m_mask = null; break;
							case 1: Layer1.m_mask = null; break;
							case 2: Layer2.m_mask = null; break;
							default: throw new Exception( "Unsupported amount of extra layers!" );
						}
						break;

						// LAYER 0
					case "layer0_uvset":
						switch ( value ) {
							case 0: Layer0.m_UVSet = Layer.UV_SET.UV0; break;
							case 1: Layer0.m_UVSet = Layer.UV_SET.UV1; break;
							default: throw new Exception( "Unsupported UV set!" );
						}
						break;
					case "layer0_mask_uvset":
						switch ( value ) {
							case 0: Layer0.m_maskUVSet = Layer.UV_SET.UV0; break;
							case 1: Layer0.m_maskUVSet = Layer.UV_SET.UV1; break;
							default: throw new Exception( "Unsupported UV set!" );
						}
						break;
					case "layer0_maskmode":
						switch ( value ) {
							case 0: Layer0.m_maskingMode = Layer.MASKING_MODE.VERTEX_COLOR; break;
							case 1: Layer0.m_maskingMode = Layer.MASKING_MODE.MASK_MAP; break;
							case 2: Layer0.m_maskingMode = Layer.MASKING_MODE.MASK_MAP_AND_VERTEX_COLOR; break;
							default: throw new Exception( "Unsupported UV set!" );
						}
						break;

						// LAYER 1
					case "layer1_uvset":
						switch ( value ) {
							case 0: Layer1.m_UVSet = Layer.UV_SET.UV0; break;
							case 1: Layer1.m_UVSet = Layer.UV_SET.UV1; break;
							default: throw new Exception( "Unsupported UV set!" );
						}
						break;
					case "layer1_mask_uvset":
						switch ( value ) {
							case 0: Layer1.m_maskUVSet = Layer.UV_SET.UV0; break;
							case 1: Layer1.m_maskUVSet = Layer.UV_SET.UV1; break;
							default: throw new Exception( "Unsupported UV set!" );
						}
						break;
					case "layer1_maskmode":
						switch ( value ) {
							case 0: Layer1.m_maskingMode = Layer.MASKING_MODE.VERTEX_COLOR; break;
							case 1: Layer1.m_maskingMode = Layer.MASKING_MODE.MASK_MAP; break;
							case 2: Layer1.m_maskingMode = Layer.MASKING_MODE.MASK_MAP_AND_VERTEX_COLOR; break;
							default: throw new Exception( "Unsupported UV set!" );
						}
						break;

						// LAYER 2
					case "layer2_uvset":
						switch ( value ) {
							case 0: Layer2.m_UVSet = Layer.UV_SET.UV0; break;
							case 1: Layer2.m_UVSet = Layer.UV_SET.UV1; break;
							default: throw new Exception( "Unsupported UV set!" );
						}
						break;
					case "layer2_mask_uvset":
						switch ( value ) {
							case 0: Layer2.m_maskUVSet = Layer.UV_SET.UV0; break;
							case 1: Layer2.m_maskUVSet = Layer.UV_SET.UV1; break;
							default: throw new Exception( "Unsupported UV set!" );
						}
						break;
					case "layer2_maskmode":
						switch ( value ) {
							case 0: Layer2.m_maskingMode = Layer.MASKING_MODE.VERTEX_COLOR; break;
							case 1: Layer2.m_maskingMode = Layer.MASKING_MODE.MASK_MAP; break;
							case 2: Layer2.m_maskingMode = Layer.MASKING_MODE.MASK_MAP_AND_VERTEX_COLOR; break;
							default: throw new Exception( "Unsupported UV set!" );
						}
						break;
			}
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
