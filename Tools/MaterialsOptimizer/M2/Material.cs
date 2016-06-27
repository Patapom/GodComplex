using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using RendererManaged;

namespace MaterialsOptimizer
{
	[System.Diagnostics.DebuggerDisplay( "{m_name} - {m_options} - {m_layers.Count} Layers" )]
	public class Material {

		[System.Diagnostics.DebuggerDisplay( "{m_type}" )]
		public class	Programs {
			public enum		KNOWN_TYPES {
				UNKNOWN,
				DEFAULT,
				EYE,
				SKIN,
				HAIR,
				VEGETATION,
				WATER,
				OCEAN,
				VISTA,
				SKY,
				CLOUDS,
				CABLE,
				FX,
				DECAL,
			}

			public KNOWN_TYPES	m_type = KNOWN_TYPES.UNKNOWN;
			public string		m_main;
			public string		m_ZPrepass;
			public string		m_shadow;

			public void		SetMainProgram( string _mainProgramName ) {
				m_main = _mainProgramName;

				_mainProgramName = _mainProgramName.ToLower();
				if ( _mainProgramName == "arkdefault" )
					m_type = KNOWN_TYPES.DEFAULT;
				else if ( _mainProgramName == "arkeyeball" )
					m_type = KNOWN_TYPES.EYE;
				else if ( _mainProgramName == "arkhair" )
					m_type = KNOWN_TYPES.HAIR;
				else if ( _mainProgramName == "arksssrender" )
					m_type = KNOWN_TYPES.SKIN;
				else if ( _mainProgramName == "arkvista" )
					m_type = KNOWN_TYPES.VISTA;
				else if ( _mainProgramName == "arkwater" )
					m_type = KNOWN_TYPES.WATER;
				else if ( _mainProgramName == "arkvegetation" )
					m_type = KNOWN_TYPES.VEGETATION;
				else if ( _mainProgramName == "arkclouds" )
					m_type = KNOWN_TYPES.CLOUDS;
				else if ( _mainProgramName == "arksky" )
					m_type = KNOWN_TYPES.SKY;
				else if ( _mainProgramName == "arkdecal" )
					m_type = KNOWN_TYPES.DECAL;
				else if ( _mainProgramName == "arkcable" )
					m_type = KNOWN_TYPES.CABLE;
				else {
					if (	_mainProgramName.StartsWith( "particle" )
						||	_mainProgramName.StartsWith( "postfx" ) )
						m_type = KNOWN_TYPES.FX;
				}
// 				if ( m_type == KNOWN_TYPES.UNKNOWN )
// 					throw new Exception( "Urecognized program type!" );
			}
		}

		[System.Diagnostics.DebuggerDisplay( "Alpha={m_isAlpha}" )]
		public class	Options {
			public bool				m_isAlpha = false;
			public bool				m_isMasking = false;
			public bool				m_hasNormal = false;
			public bool				m_hasSpecular = false;
			public bool				m_hasGloss = false;
			public bool				m_hasMetal = false;
			public bool				m_translucencyEnabled = false;
			public bool				m_translucencyUseVertexColor = true;
		}

		public class	Layer {

			[System.Diagnostics.DebuggerDisplay( "{m_name} CstColorType={m_constantColorType}" )]
			public class	Texture {
				public string		m_name;
				public FileInfo		m_fileName = null;
				public Exception	m_error = null;		// Any error that occurred during texture creation

				public enum	 CONSTANT_COLOR_TYPE {
					TEXTURE,
					DEFAULT,
					BLACK,
					BLACK_ALPHA_WHITE,
					WHITE,
					CUSTOM,
				}

				public CONSTANT_COLOR_TYPE	m_constantColorType = CONSTANT_COLOR_TYPE.TEXTURE;
				public float4				m_customConstantColor = float4.Zero;

				public float4	ConstantColor {
					get {
						switch ( m_constantColorType ) {
							case CONSTANT_COLOR_TYPE.TEXTURE:			throw new Exception( "Not a constant color!" );
							case CONSTANT_COLOR_TYPE.DEFAULT:			throw new Exception( "Default constant color!" );
							case CONSTANT_COLOR_TYPE.BLACK:				return float4.Zero;
							case CONSTANT_COLOR_TYPE.BLACK_ALPHA_WHITE:	return float4.UnitW;
							case CONSTANT_COLOR_TYPE.WHITE:				return float4.One;
							case CONSTANT_COLOR_TYPE.CUSTOM:			return m_customConstantColor;
							default:									throw new Exception( "Unsupported constant color type!" );
						}
					}
				}

				public static	DirectoryInfo	ms_TexturesBasePath;

				public	Texture( string _textureLine ) {
					// Parse texture name
					Parser	P = new Parser( _textureLine );
					m_name = P.ReadString( true, false );

					try {
						// Check if it's a constant color
						m_name = m_name.ToLower();
						if ( m_name.StartsWith( "_" ) ) {
							// Procedural texture
							switch ( m_name ) {
								case "_default":			m_constantColorType = CONSTANT_COLOR_TYPE.DEFAULT; break;
								case "_black":				m_constantColorType = CONSTANT_COLOR_TYPE.BLACK; break;
								case "_blackalphawhite":	m_constantColorType = CONSTANT_COLOR_TYPE.BLACK_ALPHA_WHITE; break;
								case "_white":				m_constantColorType = CONSTANT_COLOR_TYPE.WHITE; break;
								default: throw new Exception( "Unsupported procedural texture type \"" + m_name + "\"!" );
							} 
						} else if ( m_name.StartsWith( "ipr_constantcolor" ) ) {
							m_constantColorType = CONSTANT_COLOR_TYPE.CUSTOM;
							P = new Parser( _textureLine );
							P.ConsumeString( "ipr_constantColor", false );
							string	strColor = P.ReadBlock( '(', ')' );
							m_customConstantColor = P.ReadFloat4( strColor );
						}

						if ( m_constantColorType == CONSTANT_COLOR_TYPE.TEXTURE ) {
							// Build file name
							string	fullPath = Path.Combine( ms_TexturesBasePath.FullName, m_name );
							if ( Path.GetExtension( fullPath.ToLower() ) == "" )
								fullPath += ".tga";	// Assume tga files if unspecified
							m_fileName = new FileInfo( fullPath );
						}
					} catch ( Exception _e ) {
						m_error = _e;
					}
				}
			}
			public enum		UV_SET {
				UV0,
				UV1,
			}
			public enum		REUSE_MODE {
				DONT_REUSE,
				REUSE_LAYER0,
				REUSE_LAYER1,	// Only available for layer 2
			}
			public enum		MASKING_MODE {
				NONE,
				VERTEX_COLOR,
				MASK_MAP,
				MASK_MAP_AND_VERTEX_COLOR,
			}

			public Texture		m_diffuse = null;
			public REUSE_MODE	m_diffuseReUse = REUSE_MODE.DONT_REUSE;
			public Texture		m_normal = null;
			public REUSE_MODE	m_normalReUse = REUSE_MODE.DONT_REUSE;
			public Texture		m_gloss = null;
			public REUSE_MODE	m_glossReUse = REUSE_MODE.DONT_REUSE;
			public Texture		m_metal = null;
			public REUSE_MODE	m_metalReUse = REUSE_MODE.DONT_REUSE;
			public Texture		m_AO = null;
			public Texture		m_translucency = null;
			public Texture		m_emissive = null;

			public REUSE_MODE	m_specularReUse = REUSE_MODE.DONT_REUSE;
			public Texture		m_specular = null;	// Special specular map!

			public MASKING_MODE	m_maskingMode = MASKING_MODE.NONE;
			public Texture		m_mask = null;				// Layer mask
			public REUSE_MODE	m_maskReUse = REUSE_MODE.DONT_REUSE;

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

		public Options			m_options = new Options();	// Options

		// Textures
		public List< Layer >	m_layers = new List< Layer >();

		public Layer.Texture	m_height = null;	// Special height map!

		// Main variables
		public float2			m_glossMinMax = new float2( 0.0f, 0.5f );
		public float2			m_metallicMinMax = new float2( 0.0f, 0.5f );

		private Layer			Layer0 {
			get { return m_layers[0]; }
		}

		private Layer			Layer1 {
			get {
				if ( m_layers.Count < 2 )
					m_layers.Add( new Layer() );
				return m_layers[1];
			}
		}

		private Layer			Layer2 {
			get {
				if ( m_layers.Count < 3 ) {
					if ( m_layers.Count < 2 )
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
				if ( token.StartsWith( "//" ) ) {
					P.ReadToEOL();
					continue;
				}
				if ( token.StartsWith( "/*" ) ) {
					P.SkipComment();
					continue;
				}

				if ( token.EndsWith( "{" ) ) {
					// Handle problematic parms without space before their values
					token = token.Substring( 0, token.Length-1 );
					P.m_Index--;
				}

				P.SkipSpaces();

				switch ( token.ToLower() ) {
					case "state":
						ParseState( P.ReadBlock() );
						break;

					case "parms":
						ParseParms( P.ReadBlock() );
						break;

					case "options":
						ParseOptions( P.ReadBlock() );
						break;

					// Programs
					case "mainprogram":			m_programs.SetMainProgram( P.ReadString() ); break;
					case "zprepassprogram":		m_programs.m_ZPrepass = P.ReadString(); break;
					case "shadowprogram":		m_programs.m_shadow = P.ReadString(); break;

					// Textures
						// Layer 0
					case "diffusemap":			Layer0.m_diffuse = new Layer.Texture( P.ReadToEOL() ); break;
					case "bumpmap":				Layer0.m_normal = new Layer.Texture( P.ReadToEOL() ); break;
					case "glossmap":			Layer0.m_gloss = new Layer.Texture( P.ReadToEOL() ); break;
					case "metallicmap":			Layer0.m_metal = new Layer.Texture( P.ReadToEOL() ); break;
					case "specularmap":			Layer0.m_specular = new Layer.Texture( P.ReadToEOL() ); break;
					case "heightmap":			m_height = new Layer.Texture( P.ReadToEOL() ); break;
					case "occlusionmap":		Layer0.m_AO = new Layer.Texture( P.ReadToEOL() ); break;
					case "translucencymap":		Layer0.m_translucency = new Layer.Texture( P.ReadToEOL() ); break;
					case "emissivemap":			Layer0.m_emissive = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer0_maskmap":		Layer0.m_mask = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer0_scalebias":	Layer0.ParseScaleBias( P ); break;
					case "layer0_maskscalebias":Layer0.ParseMaskScaleBias( P ); break;

						// Layer 1
					case "layer1_diffusemap":	Layer1.m_diffuse = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer1_bumpmap":		Layer1.m_normal = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer1_glossmap":		Layer1.m_gloss = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer1_specularmap":	Layer1.m_specular = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer1_metallicmap":	Layer1.m_metal = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer1_maskmap":		Layer1.m_mask = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer1_emissivemap":	throw new Exception( "Shouldn't be allowed!" );//P.SkipSpaces(); Layer1.m_emissive = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer1_scalebias":	Layer1.ParseScaleBias( P ); break;
					case "layer1_maskscalebias":Layer1.ParseMaskScaleBias( P ); break;

						// Layer 2
					case "layer2_diffusemap":	Layer2.m_diffuse = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer2_bumpmap":		Layer2.m_normal = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer2_glossmap":		Layer2.m_gloss = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer2_specularmap":	Layer2.m_specular = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer2_metallicmap":	Layer2.m_metal = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer2_emissivemap":	throw new Exception( "Shouldn't be allowed!" );//P.SkipSpaces(); Layer2.m_emissive = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer2_maskmap":		Layer2.m_mask = new Layer.Texture( P.ReadToEOL() ); break;
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

					default:
						#if DEBUG
							CheckSafeTokens( token );
						#endif
						P.ReadToEOL();	// Don't care...
						break;
				}
			}
		}

		private void	ParseState( string _state ) {
			try {
			} catch ( Exception _e ) {
				throw new Exception( "Failed parsing state block!", _e );
			}
		}

		private void	ParseParms( string _parms ) {
			try {
			} catch ( Exception _e ) {
				throw new Exception( "Failed parsing parms block!", _e );
			}
		}

		private void	ParseOptions( string _options ) {
			try {
				Parser	P = new Parser( _options );
				while ( P.OK ) {
					string	token = P.ReadString();
					if ( token == null )
						break;	// Done!
					if ( token.StartsWith( "//" ) ) {
						P.ReadToEOL();
						continue;
					}
					if ( token.StartsWith( "/*" ) ) {
						P.SkipComment();
						continue;
					}
					if ( !P.IsNumeric() )
						continue;	// Ill-formed option?

					int		value = P.ReadInteger();

					switch ( token.ToLower() ) {

						case "isalpha":						m_options.m_isAlpha = value != 0; break;
						case "alphatest":					m_options.m_isAlpha = value != 0; break;
						case "ismasking":					m_options.m_isMasking = value != 0; break;
						case "hasbumpmap":					m_options.m_hasNormal = value != 0; break;
						case "hasspecularmap":				m_options.m_hasSpecular = value != 0; break;
						case "hasglossmap":					m_options.m_hasGloss = value != 0; break;
						case "hasmetallicmap":				m_options.m_hasMetal = value != 0; break;
						case "translucency/enable":			m_options.m_translucencyEnabled = value != 0; break;
						case "translucencyusevertexcolor":	m_options.m_translucencyUseVertexColor = value != 0; break;

						case "extralayers":
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
						case "layer1_diffusereuselayer":
							switch ( value ) {
								case 0:	Layer1.m_diffuseReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer1.m_diffuseReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
								default: throw new Exception( "Unsupported re-use mode!" );
							}
							break;
						case "layer1_glossreuselayer":
							switch ( value ) {
								case 0:	Layer1.m_glossReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer1.m_glossReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
								default: throw new Exception( "Unsupported re-use mode!" );
							}
							break;
						case "layer1_specularreuselayer":
							switch ( value ) {
								case 0:	Layer1.m_specularReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer1.m_specularReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
								default: throw new Exception( "Unsupported re-use mode!" );
							}
							break;
						case "layer1_metallicreuselayer":
							switch ( value ) {
								case 0:	Layer1.m_metalReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer1.m_metalReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
								default: throw new Exception( "Unsupported re-use mode!" );
							}
							break;
						case "layer1_maskreuselayer":
							switch ( value ) {
								case 0:	Layer1.m_maskReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer1.m_maskReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
								default: throw new Exception( "Unsupported re-use mode!" );
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
						case "layer2_diffusereuselayer":
							switch ( value ) {
								case 0:	Layer1.m_diffuseReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer1.m_diffuseReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
								case 2:	Layer2.m_diffuseReUse = Layer.REUSE_MODE.REUSE_LAYER1; break;
								default: throw new Exception( "Unsupported re-use mode!" );
							}
							break;
						case "layer2_glossreuselayer":
							switch ( value ) {
								case 0:	Layer1.m_glossReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer1.m_glossReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
								case 2:	Layer2.m_glossReUse = Layer.REUSE_MODE.REUSE_LAYER1; break;
								default: throw new Exception( "Unsupported re-use mode!" );
							}
							break;
						case "layer2_specularreuselayer":
							switch ( value ) {
								case 0:	Layer2.m_specularReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer2.m_specularReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
								default: throw new Exception( "Unsupported re-use mode!" );
							}
							break;
						case "layer2_metallicreuselayer":
							switch ( value ) {
								case 0:	Layer1.m_metalReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer1.m_metalReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
								case 2:	Layer2.m_metalReUse = Layer.REUSE_MODE.REUSE_LAYER1; break;
								default: throw new Exception( "Unsupported re-use mode!" );
							}
							break;
						case "layer2_maskreuselayer":
							switch ( value ) {
								case 0:	Layer2.m_maskReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer2.m_maskReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
								case 2:	Layer2.m_maskReUse = Layer.REUSE_MODE.REUSE_LAYER1; break;
								default: throw new Exception( "Unsupported re-use mode!" );
							}
							break;

						default:
							CheckSafeOptionsTokens( token, value );
							break;
					}
				}
			} catch ( Exception _e ) {
				throw new Exception( "Failed parsing options block!", _e );
			}
		}

		#region Tokens Checking

		void	CheckSafeTokens( string token ) {
			if ( m_programs.m_type != Programs.KNOWN_TYPES.DEFAULT )
				return;	// Don't care about other programs than arkDefault

			string[]	recognizedStrings = new string[] {
				"m_physicsmaterial",
				"version",
				"darkvisionprogram",
				"sssprogram",
				"shadowProgram",
				"transmap",
				"debugcolor",
				"DefaultAlphaTest",
				"displacement",
				"displacement_tiling",
				"dispmap_novmtr",
				"maskmap",
				"sheen",
				"sheenImportance",
				"editorcolor",
				"editormap",
				"emissiveMaskMap",
				"emissiveGradientMap",
				"emissiveLookUp",
				"emissiveClipParms",
				"emissiverevealparams",
				"emissivenoise1map",
				"emissivenoise2map",
				"emissivenoise2scalepan",
				"emissivenoise1scalepan",
				"emissiveColorIndirectLighting",
				"qer_editorimage",
				"emissiveIntensity",
				"emissiveColorConstant",
				"texturemap",
				"specularTintDielectric",
				"diffusealbedometallic",
				"water/causticsChromaticAberration",
				"water/causticsFade",
				"water/causticsTiling",
				"water/causticsSpeed",
				"water/specularPower",
				"water/extinction",
				"water/inscattering",
				"water/refraction",
				"water/waterColor",
				"water/perlinAmplitude",
				"water/perlinFrequency",
				"water/perlinSpeed",
				"water/screenspacereflectionlod",
				"water/skyColor",
				"water/screenSpaceReflectionZThicknessMul",
				"water/screenSpaceReflectionZThicknessMulNear",
				"water/screenSpaceReflectionZThicknessDistanceNear",
				"water/screenSpaceReflectionZThicknessMulFar",
				"water/screenSpaceReflectionZThicknessDistanceFar",
				"water/screenSpaceReflectionZThicknessPow",
				"water/screenSpaceReflectionSoften",
				"water/screenSpaceReflectionBorderFade",
				"water/screenSpaceReflectionVerticalFade",
				"water/screenSpaceReflectionMaxStepsCount",
				"water/waterLevel",
				"water/godraysIntensity",
				"water/underWaterVisibilityBoost",
				"ocean/perlinPersistence",
				"ocean/perlinMaxOctaves",
				"ocean/perlinAntiAlias",
				"ocean/perlinAmplitude",
				"ocean/perlinFrequency",
				"ocean/perlinSpeed",
				"translucency/intensity",
				"translucency/distortion",
				"translucency/power",
				"translucency/scale",
				"translucency/ramp/innercolor",
				"translucency/ramp/mediumcolor",
				"translucency/ramp/outercolor",
				"translucency/min",
				"translucency/mask",
				"translucency_hq/extinction",
				"translucency_hq/transmittance",
				"translucency_hq/minmaxangle",
				"translucencyCoef",
				"diffuseLookup",
				"sss/skinFresnelLookup",
				"sss/mask",
				"sss/scale",
				"sss/tweakSpecularIntensity",
				"selfShadowedBumpMap",
				"hairflowmap",
				"hairSpecularMask",
				"hairSpecularNoiseMask",
				"specularNoiseMaskScale",
				"hairIBL",
				"hairDiffuseLUT",
				"hairFresnelLUT",
				"primarySpecularIntensity",
				"primarySpecularShift",
				"primarySpecularPower",
				"secondarySpecularIntensity",
				"secondarySpecularShift",
				"secondarySpecularPower",
				"diffuseBlur",
				"detailBumpMap",
				"detailBumpMapTiling",
				"detailBumpMapIntensity",
				"specularFlowAngle",
				"fresnelStrength",
				"wardRoughness2",
				"wardRoughness3",
				"eye/causticMap",
				"eye/causticStrength",
				"eye/sssLUT",
				"eye/refraction",
				"eye/UVSettings",
				"eye/glossParm",
				"eye/fakeLight_1",
				"eye/fakeLight_2",
				"fuzz/fresnel",
				"fuzz/mask",
				"fuzz/intensity",
				"alpha/refractionIOR",
				"alpha/transmittance",
				"alpha/blurSize",
				"alpha/normalMask",
				"zbias",
				"layer0_colorConstant",
				"layer1_rescalevalues",
				"layer1_colorConstant",
				"layer2_rescalevalues",
				"layer2_colorConstant",
				"outlineDepthBias",
				"parallaxHeight_cm",
				"parallaxMaxStepsCount",
				"parallaxnormalsmoothfactor",
				"texelsPerMeter",

				"sheenMap",

// Vista-related parms
"lightMap",
"lightMapLuminance",

// Vegetation-related parms
"vegetanim/leaffreq",
"vegetanim/branchamplitude",
"vegetanim/mainbendingamplitude",
"vegetanim/mainbendingvar",
"vegetanim/mainbendingfreq",
"vegetanim/leafamplitude",

// Cloud-related parms
"cloudProjOnFar",
"cloudDensityCoef",
"cloudVerticalAlpha",
"cloudBackLitCoef",

// Not sure what those are
"vibrationamplituderange",
"vibrationdistancefactorrange",
"vibrationfrequencyrange",
"vibrationdistancerange",

// Annoying FX parms but used by arkDefault!
"opacity_Tilling",
"opacity_PanningSpeed",
"radial_RampPow",
"color_Adjust",
"disto1_strength",
"disto1_tilling",
"disto1_panningspeed",
"particlefresnelstrength",
"particlefresnelthreshold",
"particledistancefadeout",
"opacity_rotationspeed",
"material_seed",
"emissivegoonormalsmoothing",
"emissivegoogloss",
"emissivegoocolor",





// Problematic variables that shouldn't be used by the user!
"water/wsRefractedLightDir",
"precomputeIL/materialID",
"arealightglobalparms",
"indirectLighting/glossMetalOverrides",
"color",
"env/atm/sky/cloudShadow",
"viewport",
"debug_fdrfactor",
"worldTime",
"outlineSeedPower",
"outlineColor",
"outlineDistances",
"IBL/texCubeMap",
"dynamiccanvaslightscattering",
"env/indirectlighting/bouncefactor/static",	// ENV ONLY

// Obsolete stuff
"roughnessMap",
"wardDiffuseRoughness",
"IBL/intensity",
			};
			token = token.ToLower();
			if ( token.StartsWith( "//" ) )
				return;
			if ( token.StartsWith( "materialeffects" ) )
				return;
			if ( token.StartsWith( "decal" ) )
				return;
			if ( token.StartsWith( "fx/" ) )
				return;

			foreach ( string recognizedString in recognizedStrings ) {
				if ( recognizedString.ToLower() == token )
					return;	// Okay!
			}

			int	glou = 0;
			glou++;
//			throw new Exception( "Unrecognized token!" );
		}

		void	CheckSafeOptionsTokens( string token, int value ) {
// 			if ( m_programs.m_type != Programs.KNOWN_TYPES.DEFAULT )
// 				return;	// Don't care about other programs than arkDefault

			string[]	recognizedStrings = new string[] {
			};
			token = token.ToLower();
			if ( token.StartsWith( "//" ) )
				return;
			if ( token.StartsWith( "materialeffects" ) )
				return;
			if ( token.StartsWith( "decal" ) )
				return;
			if ( token.StartsWith( "fx/" ) )
				return;

			foreach ( string recognizedString in recognizedStrings ) {
				if ( recognizedString.ToLower() == token )
					return;	// Okay!
			}

			int	glou = 0;
			glou++;
//			throw new Exception( "Unrecognized token!" );
		}

		#endregion

		#endregion
	}
}
