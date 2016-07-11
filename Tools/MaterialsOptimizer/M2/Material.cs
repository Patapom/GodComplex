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

		#region NESTED TYPES

		public enum ERROR_LEVEL	{
			NONE = 0,
			DIRTY = 1,
			STANDARD = 2,
			DANGEROUS = 3,
		}

		[System.Diagnostics.DebuggerDisplay( "{m_type}" )]
		public class	Programs {
			public enum		KNOWN_TYPES {
				DEFAULT,
				EYE,
				SKIN,
				HAIR,
				VEGETATION,
				VISTA,
				WATER,
				OCEAN,
				SKY,
				CLOUDS,
				CABLE,
				FX,
				DECAL,

				UNKNOWN,
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

			#region Serialization

			public void	Write( BinaryWriter W ) {
				W.Write( (int) m_type );
				W.Write( m_main != null ? m_main : "" );
				W.Write( m_ZPrepass != null ? m_ZPrepass : "" );
				W.Write( m_shadow != null ? m_shadow : "" );
			}

			public void	Read( BinaryReader R ) {
				m_type = (KNOWN_TYPES) R.ReadInt32();

				m_main = R.ReadString();
				m_main = m_main == string.Empty ? null : m_main;
				m_ZPrepass = R.ReadString();
				m_ZPrepass = m_ZPrepass == string.Empty ? null : m_ZPrepass;
				m_shadow = R.ReadString();
				m_shadow = m_shadow == string.Empty ? null : m_shadow;
			}

			#endregion

			public void		Export( StringWriter W, string T ) {
				if ( m_main != null )
					W.Write( T + "mainprogram	" + m_main );
				if ( m_ZPrepass != null )
					W.Write( T + "zprepassprogram	" + m_ZPrepass );
				if ( m_shadow != null )
					W.Write( T + "shadowprogram	" + m_shadow );
			}
		}

		[System.Diagnostics.DebuggerDisplay( "{m_sortStage}" )]
		public class	Parms {
			public enum		SORT_STAGE {
				sortCoverage,			// { Vec	0	 }
				sortBoatVolume,			// { Vec	80	 }
				sortEmit,				// { Vec	100  }	// things that go in the glare pass and are additionally rendered to the main pass
				sortEmitOnly,			// { Vec	200  }	// things that go in the glare pass only (ie certain particle effects)
				sortShadowWalk,			// { Vec	250  }
				sortWater,				// { Vec	300  }
				sortDecal,				// { Vec	400  }
				sortTransSort,			// { Vec	495  }
				sortTrans,				// { Vec	500  }
				sortDarkVision,			// { Vec	800  }
				sortHud,				// { Vec	1050 }	// things like blood splats for the hud
				sortPerturber,			// { Vec	1200 }	// surfaces which distort
				sortAutomap,			// { Vec	1250 }	// surfaces for the automap that need to be rendered after post process
				sortPostTonemap,		// { Vec	1400 }	// surfaces for the gui ( Iggy ) 

				UNKNOWN,
			}

			public SORT_STAGE		m_sortStage = SORT_STAGE.sortCoverage;	// Default is opaque

			public List< string >	m_unknownParms = new List< string >();

			public bool			IsAlpha {
				get { return (int) m_sortStage > (int) SORT_STAGE.sortTransSort; }
			}

			public void	Parse( string _parms ) {
				m_sortStage = SORT_STAGE.UNKNOWN;

				Parser	P = new Parser( _parms );
				while ( P.OK ) {
					string	token = P.ReadString();
					if ( token == null )
						break;	// Done!
					if ( token.StartsWith( "//" ) ) {
						RecordSingleLineCommentVariable( m_unknownParms, token, P );
						continue;
					}
					if ( token.StartsWith( "/*" ) ) {
						RecordCommentVariable( m_unknownParms, token, P );
						continue;
					}

					if ( token.EndsWith( "{" ) ) {
						// Handle problematic parms without space before their values
						token = token.Substring( 0, token.Length-1 );
						P.m_Index--;
					}

					P.SkipSpaces();

					switch ( token.ToLower() ) {
						case "stagesort": {
							string	sortStageName = P.ReadString();
							switch ( sortStageName.ToLower() ) {
								case "sortcoverage":	m_sortStage = SORT_STAGE.sortCoverage; break;
								case "sortboatvolume":	m_sortStage = SORT_STAGE.sortBoatVolume; break;
								case "sortemit":		m_sortStage = SORT_STAGE.sortEmit; break;
								case "sortemitonly":	m_sortStage = SORT_STAGE.sortEmitOnly; break;
								case "sortshadowwalk":	m_sortStage = SORT_STAGE.sortShadowWalk; break;	
								case "sortwater":		m_sortStage = SORT_STAGE.sortWater; break;
								case "sortdecal":		m_sortStage = SORT_STAGE.sortDecal; break;
								case "sorttranssort":	m_sortStage = SORT_STAGE.sortTransSort; break;
								case "sorttrans":		m_sortStage = SORT_STAGE.sortTrans; break;
								case "sortdarkvision":	m_sortStage = SORT_STAGE.sortDarkVision; break;
								case "sorthud":			m_sortStage = SORT_STAGE.sortHud; break;
								case "sortperturber":	m_sortStage = SORT_STAGE.sortPerturber; break;
								case "sortautomap":		m_sortStage = SORT_STAGE.sortAutomap; break;
								case "sortposttonemap":	m_sortStage = SORT_STAGE.sortPostTonemap; break;
								default: throw new Exception( "Unhandled sort stage!" );
							}
							break;
						}
						default:
							m_unknownParms.Add( token + "	" + P.ReadToEOL() );
							break;
					}
				}
			}

			#region Serialization

			public void	Write( BinaryWriter W ) {
				W.Write( (int) m_sortStage );

				WriteListOfStrings( W, m_unknownParms );
			}

			public void	Read( BinaryReader R ) {
				m_sortStage = (SORT_STAGE) R.ReadInt32();

				ReadListOfStrings( R, m_unknownParms );
			}

			#endregion

			public void		Export( StringWriter W, string T ) {
				string	oldT = T;

				W.WriteLine( T + "parms {" );
				T += "\t";

				if ( m_sortStage != SORT_STAGE.UNKNOWN )
					W.WriteLine( T + "stageSort	" + m_sortStage.ToString() );

				foreach ( string unknownParm in m_unknownParms )
					W.WriteLine( T + unknownParm );

				T = oldT;
				W.WriteLine( T + "}" );
			}
		}

		[System.Diagnostics.DebuggerDisplay( "{}" )]
		public class	States {

			public enum		BLEND_STATE {
				OPAQUE,
				ALPHA,
			}

			public string			m_rawBlendState = null;
			public BLEND_STATE		m_blendState = BLEND_STATE.OPAQUE;	// Default is opaque

			public List< string >	m_unknownStates = new List< string >();

			public bool			IsAlpha {
				get { return m_blendState != BLEND_STATE.OPAQUE; }
			}

			public void	Parse( string _parms ) {
				m_rawBlendState = null;
				m_blendState = BLEND_STATE.OPAQUE;

				Parser	P = new Parser( _parms );
				while ( P.OK ) {
					string	token = P.ReadString();
					if ( token == null )
						break;	// Done!
					if ( token.StartsWith( "//" ) ) {
						RecordSingleLineCommentVariable( m_unknownStates, token, P );
						continue;
					}
					if ( token.StartsWith( "/*" ) ) {
						RecordCommentVariable( m_unknownStates, token, P );
						continue;
					}

					if ( token.EndsWith( "{" ) ) {
						// Handle problematic parms without space before their values
						token = token.Substring( 0, token.Length-1 );
						P.m_Index--;
					}

					P.SkipSpaces();

					switch ( token.ToLower() ) {
						case "blend": {
							P.SkipSpaces();
							m_rawBlendState = P.ReadToEOL();
							ParseBlendState( m_rawBlendState );
							break;
						}
						default:
							m_unknownStates.Add( token + "	" + P.ReadToEOL() );
							break;
					}
				}
			}

			#region Serialization

			public void	Write( BinaryWriter W ) {
				W.Write( m_rawBlendState != null ? m_rawBlendState : "" );
				W.Write( (int) m_blendState );

				WriteListOfStrings( W, m_unknownStates );
			}

			public void	Read( BinaryReader R ) {
				m_rawBlendState = R.ReadString();
				m_rawBlendState = m_rawBlendState != string.Empty ? m_rawBlendState : null;
				m_blendState = (BLEND_STATE) R.ReadInt32();

				ReadListOfStrings( R, m_unknownStates );
			}

			#endregion

			public void		Export( StringWriter W, string T ) {
				string	oldT = T;
				W.WriteLine( T + "state {" );
				T += "\t";

				if ( m_rawBlendState != null ) {
					W.WriteLine( T + "blend	" + m_rawBlendState );
				}

				foreach ( string unknownState in m_unknownStates )
					W.WriteLine( T + unknownState );

				T = oldT;
				W.WriteLine( T + "}" );
			}

			void	ParseBlendState( string _rawBlendState ) {
				m_blendState = BLEND_STATE.OPAQUE;

				Parser		P = new Parser( _rawBlendState );
				string		sourceOp = P.ReadString();
				string		destOp = P.OK ? P.ReadString() : null;
				string[]	ops = new string[] { sourceOp, destOp };
				foreach ( string op in ops ) {
					if ( op != null )
						switch ( op.ToUpper() ) {
							case "SRC_ALPHA":
							case "ONE_MINUS_SRC_ALPHA":
							case "DST_ALPHA":
							case "ONE_MINUS_DST_ALPHA":
								m_blendState = BLEND_STATE.ALPHA;
								break;
						}
				}
			}
		}

		[System.Diagnostics.DebuggerDisplay( "Alpha={m_isAlpha}" )]
		public class	Options {
			public bool				m_glossInDiffuseAlpha = false;	// The new option that allows (diffuse+gloss) merging into a single texture

			public bool				m_isAlphaTest = false;
			public bool				m_isMasking = false;
			public bool				m_hasNormal = false;
			public bool				m_hasSpecular = false;
			public bool				m_hasOcclusionMap = false;
			public bool				m_hasGloss = false;
			public bool				m_hasMetal = false;
			public bool				m_hasEmissive = false;
			public bool				m_translucencyEnabled = false;
			public bool				m_translucencyUseVertexColor = true;
			public int				m_extraLayers = 0;

			public List< string >	m_unknownOptions = new List< string >();

			public bool				IsAlpha {
				get { return m_isAlphaTest; }
			}

			#region Serialization

			public void	Write( BinaryWriter W ) {
				W.Write( m_glossInDiffuseAlpha );

				W.Write( m_isAlphaTest );
				W.Write( m_isMasking );
				W.Write( m_hasNormal );
				W.Write( m_hasSpecular );
				W.Write( m_hasOcclusionMap );
				W.Write( m_hasGloss );
				W.Write( m_hasMetal );
				W.Write( m_hasEmissive );
				W.Write( m_translucencyEnabled );
				W.Write( m_translucencyUseVertexColor );
				W.Write( m_extraLayers );

				WriteListOfStrings( W, m_unknownOptions );
			}

			public void	Read( BinaryReader R ) {
				m_glossInDiffuseAlpha = R.ReadBoolean();

				m_isAlphaTest = R.ReadBoolean();
				m_isMasking = R.ReadBoolean();
				m_hasNormal = R.ReadBoolean();
				m_hasSpecular = R.ReadBoolean();
				m_hasOcclusionMap = R.ReadBoolean();
				m_hasGloss = R.ReadBoolean();
				m_hasMetal = R.ReadBoolean();
				m_hasEmissive = R.ReadBoolean();
				m_translucencyEnabled = R.ReadBoolean();
				m_translucencyUseVertexColor = R.ReadBoolean();
				m_extraLayers = R.ReadInt32();

				ReadListOfStrings( R, m_unknownOptions );
			}

			#endregion

			public void		Export( StringWriter W, string T, List< Layer > _layers ) {
				T += "\t";	// Indent

				// Write known options
				W.WriteLine( T + "extralayer		" + m_extraLayers );
				W.WriteLine( T + "glossInDiffuseAlpha	" + (m_glossInDiffuseAlpha ? 1 : 0) );
				W.WriteLine( T + "alphatest		" + (m_isAlphaTest ? 1 : 0) );
				W.WriteLine( T + "ismasking		" + (m_isMasking ? 1 : 0) );
				W.WriteLine( T + "hasbumpmap		" + (m_hasNormal ? 1 : 0) );
				W.WriteLine( T + "hasspecularmap	" + (m_hasSpecular ? 1 : 0) );
				W.WriteLine( T + "hasocclusionmap	" + (m_hasOcclusionMap ? 1 : 0) );
				W.WriteLine( T + "hasglossmap		" + (m_hasGloss ? 1 : 0) );
				W.WriteLine( T + "hasmetallicmap	" + (m_hasMetal ? 1 : 0) );
				W.WriteLine( T + "hasemissivemap	" + (m_hasEmissive ? 1 : 0) );
				W.WriteLine( T + "translucency/enable			" + (m_translucencyEnabled ? 1 : 0) );
				W.WriteLine( T + "translucencyusevertexcolor	" + (m_translucencyUseVertexColor ? 1 : 0) );

				// LAYER 0
				Layer	L = _layers[0];
				W.WriteLine( T + "use_Layer0_ColorConstant	" + (L.m_useColorConstant ? 1 : 0) );
				WriteLayerUVSet( W, T + "layer0_uvset", L.m_UVSet );
				WriteLayerMaskingMode( W, T + "layer0_maskmode", L.m_maskingMode );
				if ( L.m_maskingMode != Layer.MASKING_MODE.VERTEX_COLOR ) {
					WriteLayerUVSet( W, T + "layer0_mask_uvset", L.m_maskUVSet );
				}

				if ( m_extraLayers > 0 ) {
					// LAYER 1
					L = _layers[1];
					WriteLayerUVSet( W, T + "layer1_uvset", L.m_UVSet );
					W.WriteLine( T + "use_Layer1_ColorConstant	" + (L.m_useColorConstant ? 1 : 0) );
					WriteLayerReUseMode( W, T + "layer1_diffusereuselayer", L.m_diffuseReUse );
					WriteLayerReUseMode( W, T + "layer1_bumpreuselayer", L.m_normalReUse );
					if ( m_hasGloss )
						WriteLayerReUseMode( W, T + "layer1_glossreuselayer", L.m_glossReUse );
					if ( m_hasMetal )
						WriteLayerReUseMode( W, T + "layer1_metallicreuselayer", L.m_metalReUse );

					WriteLayerMaskingMode( W, T + "layer1_maskmode", L.m_maskingMode );
					if ( L.m_maskingMode != Layer.MASKING_MODE.VERTEX_COLOR ) {
						WriteLayerUVSet( W, T + "layer1_mask_uvset", L.m_maskUVSet );
						WriteLayerReUseMode( W, T + "layer1_maskreuselayer", L.m_maskReUse );
					}
					if ( m_hasSpecular )
						WriteLayerReUseMode( W, T + "layer1_specularreuselayer", L.m_specularReUse );

					if ( m_extraLayers > 1 ) {
						// LAYER 2
						L = _layers[2];
						WriteLayerUVSet( W, T + "layer2_uvset", L.m_UVSet );
						W.WriteLine( T + "use_Layer2_ColorConstant	" + (L.m_useColorConstant ? 1 : 0) );
						WriteLayerReUseMode( W, T + "layer2_diffusereuselayer", L.m_diffuseReUse );
						WriteLayerReUseMode( W, T + "layer2_bumpreuselayer", L.m_normalReUse );
						if ( m_hasGloss )
							WriteLayerReUseMode( W, T + "layer2_glossreuselayer", L.m_glossReUse );
						if ( m_hasMetal )
							WriteLayerReUseMode( W, T + "layer2_metallicreuselayer", L.m_metalReUse );

						WriteLayerMaskingMode( W, T + "layer2_maskmode", L.m_maskingMode );
						if ( L.m_maskingMode != Layer.MASKING_MODE.VERTEX_COLOR ) {
							WriteLayerUVSet( W, T + "layer2_mask_uvset", L.m_maskUVSet );
							WriteLayerReUseMode( W, T + "layer2_maskreuselayer", L.m_maskReUse );
						}
						if ( m_hasSpecular )
							WriteLayerReUseMode( W, T + "layer2_specularreuselayer", L.m_specularReUse );
					}
				}

				// Write unknown options
				foreach ( string unknownOption in m_unknownOptions ) {
					W.WriteLine( T + unknownOption );
				}
			}

			void	WriteLayerUVSet( StringWriter W, string _optionName, Layer.UV_SET _UVSet ) {
				int	UVSetValue = 0;
				switch ( _UVSet ) {
					case Layer.UV_SET.UV0: UVSetValue = 0; break;
					case Layer.UV_SET.UV1: UVSetValue = 1; break;
				}
				W.WriteLine( _optionName + "	" + UVSetValue );
			}

			void	WriteLayerMaskingMode( StringWriter W, string _optionName, Layer.MASKING_MODE _maskingMode ) {
				int	maskingModeValue = 0;
				switch ( _maskingMode ) {
					case Layer.MASKING_MODE.VERTEX_COLOR: maskingModeValue = 0; break;
					case Layer.MASKING_MODE.MASK_MAP: maskingModeValue = 1; break;
					case Layer.MASKING_MODE.MASK_MAP_AND_VERTEX_COLOR: maskingModeValue = 2; break;
				}
				W.WriteLine( _optionName + "	" + maskingModeValue );
			}

			void	WriteLayerReUseMode( StringWriter W, string _optionName, Layer.REUSE_MODE _reUseMode ) {
				int	reUseModeValue = 0;
				switch ( _reUseMode ) {
					case Layer.REUSE_MODE.DONT_REUSE: reUseModeValue = 0; break;
					case Layer.REUSE_MODE.REUSE_LAYER0: reUseModeValue = 1; break;
					case Layer.REUSE_MODE.REUSE_LAYER1: reUseModeValue = 2; break;
				}
				W.WriteLine( _optionName + "	" + reUseModeValue );
			}
		}

		public class	Layer {

			#region NESTED TYPES

			[System.Diagnostics.DebuggerDisplay("{m_name} CstColorType={m_constantColorType} Usage={m_textureFileInfo!=null?m_textureFileInfo.m_usage.ToString():\"<NOT A TEXTURE>\"}")]
			public class	Texture {
				public enum	 CONSTANT_COLOR_TYPE {
					TEXTURE,
					DEFAULT,
					BLACK,
					BLACK_ALPHA_WHITE,
					WHITE,
					INVALID,		// <= Used for replacement when diffuse textures are missing (creates a lovely RED)
					CUSTOM,
				}

				public string				m_rawTextureLine;
				public string				m_name;
				public FileInfo				m_fileName = null;
				public Exception			m_error = null;		// Any error that occurred during texture creation
				public CONSTANT_COLOR_TYPE	m_constantColorType = CONSTANT_COLOR_TYPE.TEXTURE;
				public float4				m_customConstantColor = float4.Zero;

				// Resolve by analysis
				public TextureFileInfo		m_textureFileInfo = null;

				public int					m_dummyCounter = 0;

				public float4	ConstantColor {
					get {
						switch ( m_constantColorType ) {
							case CONSTANT_COLOR_TYPE.TEXTURE:			throw new Exception( "Not a constant color!" );
							case CONSTANT_COLOR_TYPE.DEFAULT:			throw new Exception( "Default constant color!" );
							case CONSTANT_COLOR_TYPE.BLACK:				return float4.Zero;
							case CONSTANT_COLOR_TYPE.BLACK_ALPHA_WHITE:	return float4.UnitW;
							case CONSTANT_COLOR_TYPE.WHITE:				return float4.One;
							case CONSTANT_COLOR_TYPE.INVALID:			return new float4( 1, 0, 0, 1 );	// Red pétant!
							case CONSTANT_COLOR_TYPE.CUSTOM:			return m_customConstantColor;
							default:									throw new Exception( "Unsupported constant color type!" );
						}
					}
				}

				public static	DirectoryInfo	ms_TexturesBasePath;

				public	Texture( string _textureLine ) {
					m_rawTextureLine = _textureLine;

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
								case "_invalid":			m_constantColorType = CONSTANT_COLOR_TYPE.INVALID; break;
								default: throw new Exception( "Unsupported procedural texture type \"" + m_name + "\"!" );
							} 
						} else if ( m_name.StartsWith( "ipr_constantcolor" ) ) {
							m_constantColorType = CONSTANT_COLOR_TYPE.CUSTOM;
							P = new Parser( _textureLine );
							P.ConsumeString( "ipr_constantColor", false );
							string	strColor = P.ReadBlock( '(', ')' );
							m_customConstantColor = P.ReadFloat4( strColor );

							// Compare known colors to revert to basic types
							if ( CompareFloat4( m_customConstantColor, float4.Zero ) ) {
								m_constantColorType = CONSTANT_COLOR_TYPE.BLACK;
							} else if ( CompareFloat4( m_customConstantColor, float4.UnitW ) ) {
								m_constantColorType = CONSTANT_COLOR_TYPE.BLACK_ALPHA_WHITE;
							} else if ( CompareFloat4( m_customConstantColor, float4.One ) ) {
								m_constantColorType = CONSTANT_COLOR_TYPE.WHITE;
							}
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

				public	Texture( BinaryReader R ) {
					Read( R );
				}

				public static bool	operator==( Texture a, Texture b ) {
					if ( (object) a == null && (object ) b == null )
						return true;
					if ( (object) a == null || (object ) b == null )
						return false;

					if ( a.m_constantColorType != b.m_constantColorType )
						return false;
					switch ( a.m_constantColorType ) {
						case CONSTANT_COLOR_TYPE.BLACK:
						case CONSTANT_COLOR_TYPE.BLACK_ALPHA_WHITE:
						case CONSTANT_COLOR_TYPE.WHITE:
						case CONSTANT_COLOR_TYPE.INVALID:
						case CONSTANT_COLOR_TYPE.DEFAULT:
							return true;
						case CONSTANT_COLOR_TYPE.CUSTOM:
							return	CompareFloat4( a.m_customConstantColor, b.m_customConstantColor );

						case CONSTANT_COLOR_TYPE.TEXTURE:
							if ( a.m_textureFileInfo != null ) {
								return a.m_textureFileInfo == b.m_textureFileInfo;
							}
							return a.m_name.ToLower() == b.m_name.ToLower();
					}

					throw new Exception( "Unhandled case!" );
				}
				public static bool	operator!=( Texture a, Texture b ) {
					return !(a == b);	// Don't want to bother! :D
				}
				public override bool Equals(object obj) {
					return base.Equals(obj);
				}
				public override int GetHashCode() {
					return base.GetHashCode();
				}

				static bool	CompareFloat4( float4 a, float4 b ) {
					return	Math.Abs( a.x - b.x ) < 1e-3f
						&&	Math.Abs( a.y - b.y ) < 1e-3f
						&&	Math.Abs( a.z - b.z ) < 1e-3f
						&&	Math.Abs( a.w - b.w ) < 1e-3f;
				}

				#region Serialization

				public void	Write( BinaryWriter W ) {
					W.Write( m_rawTextureLine );
					W.Write( m_name );
					W.Write( m_fileName != null ? m_fileName.FullName : "" );	// Can be null when using ipr_constantColor
					W.Write( m_error != null ? m_error.Message : "" );
					W.Write( (int) m_constantColorType );
					W.Write( m_customConstantColor.x );
					W.Write( m_customConstantColor.y );
					W.Write( m_customConstantColor.z );
					W.Write( m_customConstantColor.w );
				}

				public void	Read( BinaryReader R ) {
					m_rawTextureLine = R.ReadString();
					m_name = R.ReadString();
					string	fileName = R.ReadString();
					m_fileName = fileName != string.Empty ? new FileInfo( fileName ) : null;
					string	errorText = R.ReadString();
					m_error = errorText != string.Empty ? new Exception( errorText ) : null;
					m_constantColorType = (CONSTANT_COLOR_TYPE) R.ReadInt32();
					m_customConstantColor.x = R.ReadSingle();
					m_customConstantColor.y = R.ReadSingle();
					m_customConstantColor.z = R.ReadSingle();
					m_customConstantColor.w = R.ReadSingle();
				}

				#endregion

				public string	Export() {
					return m_rawTextureLine;
// 					switch ( m_constantColorType ) {
// 						case CONSTANT_COLOR_TYPE.TEXTURE:
// 							return 
// 					}
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
				VERTEX_COLOR,
				MASK_MAP,
				MASK_MAP_AND_VERTEX_COLOR,
			}

			#endregion

			public Material		m_owner = null;
			public int			m_index = 0;

			public Texture		m_diffuse = null;
			public REUSE_MODE	m_diffuseReUse = REUSE_MODE.DONT_REUSE;
			public Texture		m_normal = null;
			public REUSE_MODE	m_normalReUse = REUSE_MODE.DONT_REUSE;
			public Texture		m_gloss = null;
			public REUSE_MODE	m_glossReUse = REUSE_MODE.DONT_REUSE;
			public Texture		m_metal = null;
			public REUSE_MODE	m_metalReUse = REUSE_MODE.DONT_REUSE;

			public REUSE_MODE	m_specularReUse = REUSE_MODE.DONT_REUSE;
			public Texture		m_specular = null;	// Special specular map!

			public MASKING_MODE	m_maskingMode = MASKING_MODE.VERTEX_COLOR;
			public Texture		m_mask = null;				// Layer mask
			public REUSE_MODE	m_maskReUse = REUSE_MODE.DONT_REUSE;

			public Texture		m_AO = null;
			public Texture		m_translucency = null;
			public Texture		m_emissive = null;

			public UV_SET		m_UVSet = UV_SET.UV0;
			public float2		m_UVOffset = float2.Zero;
			public float2		m_UVScale = float2.One;

			public UV_SET		m_maskUVSet = UV_SET.UV0;
			public float2		m_maskUVOffset = float2.Zero;
			public float2		m_maskUVScale = float2.One;

			public bool			m_useColorConstant = false;
			public float4		m_colorConstant = float4.One;


			// When a layer's diffuse+gloss layer gets optimized then we must keep track of the original textures so we can generate the merged map
			public Texture		m_diffuseBeforeOptimization = null;
			public Texture		m_glossBeforeOptimization = null;


			// Generated errors + level
			private ERROR_LEVEL	m_errorLevel = ERROR_LEVEL.NONE;
			public string		m_errors = null;
			public string		m_warnings = null;

			#region PROPERTIES

			public ERROR_LEVEL		ErrorLevel {
				get { return m_errorLevel; }
			}

			public bool				HasErrors {
				get { return m_errors != null && m_errors != ""; }
			}

			public bool				HasWarnings {
				get { return m_warnings != null && m_warnings != ""; }
			}

			/// <summary>
			/// Gets the diffuse texture used by the layer, accounting for re-use mode
			/// </summary>
			public Texture			Diffuse {
				get {
					Texture	result = m_diffuse;
					switch ( m_diffuseReUse ) {
						case Layer.REUSE_MODE.REUSE_LAYER0:	result = m_owner.m_layers[0].m_diffuse; break;
						case Layer.REUSE_MODE.REUSE_LAYER1:	result = m_owner.m_layers.Count > 1 ? m_owner.m_layers[1].m_diffuse : null; break;
					}
					return result;
				}
			}

			/// <summary>
			/// Gets the normal texture used by the layer, accounting for re-use mode
			/// </summary>
			public Texture			Normal {
				get {
					Layer.Texture	result = m_normal;
					switch ( m_normalReUse ) {
						case Layer.REUSE_MODE.REUSE_LAYER0:	result = m_owner.m_layers[0].m_normal; break;
						case Layer.REUSE_MODE.REUSE_LAYER1:	result = m_owner.m_layers.Count > 1 ? m_owner.m_layers[1].m_normal : null; break;
					}
					return result;
				}
			}

			/// <summary>
			/// Gets the gloss texture used by the layer, accounting for re-use mode
			/// </summary>
			public Texture			Gloss {
				get {
					Layer.Texture	result = m_gloss;
					switch ( m_glossReUse ) {
						case Layer.REUSE_MODE.REUSE_LAYER0:	result = m_owner.m_layers[0].m_gloss; break;
						case Layer.REUSE_MODE.REUSE_LAYER1:	result = m_owner.m_layers.Count > 1 ? m_owner.m_layers[1].m_gloss : null; break;
					}
					return result;
				}
			}

			/// <summary>
			/// Gets the metal texture used by the layer, accounting for re-use mode
			/// </summary>
			public Texture			Metal {
				get {
					Layer.Texture	result = m_metal;
					switch ( m_metalReUse ) {
						case Layer.REUSE_MODE.REUSE_LAYER0:	result = m_owner.m_layers[0].m_metal; break;
						case Layer.REUSE_MODE.REUSE_LAYER1:	result = m_owner.m_layers.Count > 1 ? m_owner.m_layers[1].m_metal : null; break;
					}
					return result;
				}
			}

			/// <summary>
			/// Gets the mask texture used by the layer, accounting for re-use mode
			/// </summary>
			public Texture			Mask {
				get {
					Layer.Texture	result = m_mask;
					switch ( m_maskReUse ) {
						case Layer.REUSE_MODE.REUSE_LAYER0:	result = m_owner.m_layers[0].m_mask; break;
						case Layer.REUSE_MODE.REUSE_LAYER1:	result = m_owner.m_layers.Count > 1 ? m_owner.m_layers[1].m_mask : null; break;
					}
					return result;
				}
			}

			/// <summary>
			/// Gets the specular texture used by the layer, accounting for re-use mode
			/// </summary>
			public Texture			Specular {
				get {
					Layer.Texture	result = m_specular;
					switch ( m_specularReUse ) {
						case Layer.REUSE_MODE.REUSE_LAYER0:	result = m_owner.m_layers[0].m_specular; break;
						case Layer.REUSE_MODE.REUSE_LAYER1:	result = m_owner.m_layers.Count > 1 ? m_owner.m_layers[1].m_specular : null; break;
					}
					return result;
				}
			}

			/// <summary>
			/// Returns true if the layer's diffuse texture is a (diffuse+gloss) packed texture
			/// </summary>
			public bool				IsOptimized {
				get {
					Texture	diffuse = Diffuse;
					return diffuse != null
						&& diffuse.m_textureFileInfo != null
						&& diffuse.m_textureFileInfo.m_usage == TextureFileInfo.USAGE.DIFFUSE_GLOSS;
				}
			}

			#endregion

			public Layer( Material _owner ) {
				m_owner = _owner;
			}

			public Layer( Material _owner, int _index ) : this( _owner ) {
				m_index = _index;
			}

			public Layer( Material _owner, int _index, BinaryReader R ) : this( _owner, _index ) {
				Read( R );
			}

			public override string ToString() {
				string	R = null;
				if ( m_errors != null && m_errors != "" )
					R += "   Errors:\n" + m_errors;
				if ( m_warnings != null && m_warnings != "" )
					R += "   Warnings:\n" + m_warnings;

				return R;
			}

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

			/// <summary>
			/// Checks the UV sets and the tiling/offsets are the same
			/// </summary>
			/// <param name="_other"></param>
			/// <returns></returns>
			public bool		SameUVs( Layer _other ) {
				if ( m_UVSet != _other.m_UVSet )
					return false;

				if ( Math.Abs( m_UVOffset.x - _other.m_UVOffset.x ) > 1e-3f )
					return false;
				if ( Math.Abs( m_UVOffset.y - _other.m_UVOffset.y ) > 1e-3f )
					return false;

				if ( Math.Abs( m_UVScale.x - _other.m_UVScale.x ) > 1e-3f )
					return false;
				if ( Math.Abs( m_UVScale.y - _other.m_UVScale.y ) > 1e-3f )
					return false;

				return true;
			}

			/// <summary>
			/// Checks the UV sets and the tiling/offsets are the same
			/// </summary>
			/// <param name="_other"></param>
			/// <returns></returns>
			public bool		SameMaskUVs( Layer _other ) {
				if ( m_maskUVSet != _other.m_maskUVSet )
					return false;

				if ( Math.Abs( m_maskUVOffset.x - _other.m_maskUVOffset.x ) > 1e-3f )
					return false;
				if ( Math.Abs( m_maskUVOffset.y - _other.m_maskUVOffset.y ) > 1e-3f )
					return false;

				if ( Math.Abs( m_maskUVScale.x - _other.m_maskUVScale.x ) > 1e-3f )
					return false;
				if ( Math.Abs( m_maskUVScale.y - _other.m_maskUVScale.y ) > 1e-3f )
					return false;

				return true;
			}

			public void		ClearErrorLevel() {
				m_errorLevel = ERROR_LEVEL.NONE;
			}
			public void		RaiseErrorLevel( ERROR_LEVEL _errorLevel ) {
				if ( (int) m_errorLevel < (int) _errorLevel )
					m_errorLevel = _errorLevel;
			}

			#region Serialization

			public void	Write( BinaryWriter W ) {
				W.Write( m_diffuse != null );
				if ( m_diffuse != null )
					m_diffuse.Write( W );
				W.Write( (int) m_diffuseReUse );

				W.Write( m_normal != null );
				if ( m_normal != null )
					m_normal.Write( W );
				W.Write( (int) m_normalReUse );

				W.Write( m_gloss != null );
				if ( m_gloss != null )
					m_gloss.Write( W );
				W.Write( (int) m_glossReUse );

				W.Write( m_metal != null );
				if ( m_metal != null )
					m_metal.Write( W );
				W.Write( (int) m_metalReUse );

				W.Write( m_specular != null );
				if ( m_specular != null )
					m_specular.Write( W );
				W.Write( (int) m_specularReUse );

				W.Write( (int) m_maskingMode );
				W.Write( m_mask != null );
				if ( m_mask != null )
					m_mask.Write( W );
				W.Write( (int) m_maskReUse );

				// Single textures
				W.Write( m_AO != null );
				if ( m_AO != null )
					m_AO.Write( W );
				W.Write( m_translucency != null );
				if ( m_translucency != null )
					m_translucency.Write( W );
				W.Write( m_emissive != null );
				if ( m_emissive != null )
					m_emissive.Write( W );

				// UV sets
				W.Write( (int) m_UVSet );
				W.Write( m_UVOffset.x );
				W.Write( m_UVOffset.y );
				W.Write( m_UVScale.x );
				W.Write( m_UVScale.y );

				W.Write( (int) m_maskUVSet );
				W.Write( m_maskUVOffset.x );
				W.Write( m_maskUVOffset.y );
				W.Write( m_maskUVScale.x );
				W.Write( m_maskUVScale.y );

				W.Write( m_useColorConstant );
				W.Write( m_colorConstant.x );
				W.Write( m_colorConstant.y );
				W.Write( m_colorConstant.z );
				W.Write( m_colorConstant.w );

				W.Write( (int) m_errorLevel );
				W.Write( m_errors != null ? m_errors : "" );
				W.Write( m_warnings != null ? m_warnings : "" );
			}

			public void	Read( BinaryReader R ) {
				m_diffuse = R.ReadBoolean() ? new Texture( R ) : null;
				m_diffuseReUse = (REUSE_MODE) R.ReadInt32();

				m_normal = R.ReadBoolean() ? new Texture( R ) : null;
				m_normalReUse = (REUSE_MODE) R.ReadInt32();

				m_gloss = R.ReadBoolean() ? new Texture( R ) : null;
				m_glossReUse = (REUSE_MODE) R.ReadInt32();

				m_metal = R.ReadBoolean() ? new Texture( R ) : null;
				m_metalReUse = (REUSE_MODE) R.ReadInt32();

				m_specular = R.ReadBoolean() ? new Texture( R ) : null;
				m_specularReUse = (REUSE_MODE) R.ReadInt32();

				m_maskingMode = (MASKING_MODE) R.ReadInt32();
				m_mask = R.ReadBoolean() ? new Texture( R ) : null;
				m_maskReUse = (REUSE_MODE) R.ReadInt32();

				// Single textures
				m_AO = R.ReadBoolean() ? new Texture( R ) : null;
				m_translucency = R.ReadBoolean() ? new Texture( R ) : null;
				m_emissive = R.ReadBoolean() ? new Texture( R ) : null;

				// UV sets
				m_UVSet = (UV_SET) R.ReadInt32();
				m_UVOffset.x = R.ReadSingle();
				m_UVOffset.y = R.ReadSingle();
				m_UVScale.x = R.ReadSingle();
				m_UVScale.y = R.ReadSingle();

				m_maskUVSet = (UV_SET) R.ReadInt32();
				m_maskUVOffset.x = R.ReadSingle();
				m_maskUVOffset.y = R.ReadSingle();
				m_maskUVScale.x = R.ReadSingle();
				m_maskUVScale.y = R.ReadSingle();

				m_useColorConstant = R.ReadBoolean();
				m_colorConstant.x = R.ReadSingle();
				m_colorConstant.y = R.ReadSingle();
				m_colorConstant.z = R.ReadSingle();
				m_colorConstant.w = R.ReadSingle();

				m_errorLevel = (ERROR_LEVEL) R.ReadInt32();
				m_errors = R.ReadString();
				if ( m_errors == string.Empty )
					m_errors = null;
				m_warnings = R.ReadString();
				if ( m_warnings == string.Empty )
					m_warnings = null;
			}

			#endregion

			#region Export

			public void		Export( StringWriter W, string T, string _layerPrefix, Options _options ) {

				string	regularTexturesPrefix = m_index > 0 ? _layerPrefix : "";

				if ( m_useColorConstant )
					W.WriteLine( T + _layerPrefix + "ColorConstant	{ " + m_colorConstant.x + ", " + m_colorConstant.y + ", " + m_colorConstant.z + ", " + m_colorConstant.w + " }" );

				if ( m_diffuse != null )
					W.WriteLine( T + regularTexturesPrefix + "diffusemap	" + m_diffuse.Export() );
				if ( _options.m_hasNormal && m_normal != null )
					W.WriteLine( T + regularTexturesPrefix + "bumpmap	" + m_normal.Export() );
				if ( _options.m_hasGloss && m_gloss != null )
					W.WriteLine( T + regularTexturesPrefix + "glossmap	" + m_gloss.Export() );
				if ( _options.m_hasMetal && m_metal != null )
					W.WriteLine( T + regularTexturesPrefix + "metallicmap	" + m_metal.Export() );
				if ( _options.m_hasEmissive && m_emissive != null )
					W.WriteLine( T + regularTexturesPrefix + "emissivemap	" + m_emissive.Export() );
				if ( _options.m_hasSpecular && m_specular != null )
					W.WriteLine( T + regularTexturesPrefix + "specularmap	" + m_specular.Export() );

				// Write scale/offset
				W.WriteLine( T + _layerPrefix + "scalebias	{ " + m_UVScale.x + ", " + m_UVScale.y + ", " + m_UVOffset.x + ", " + m_UVOffset.y + " }" );

				// Masks
				if ( m_maskingMode != MASKING_MODE.VERTEX_COLOR && m_mask != null ) {
					W.WriteLine( T + _layerPrefix + "maskmap	" + m_mask.Export() );
					W.WriteLine( T + _layerPrefix + "maskscalebias	{ " + m_maskUVScale.x + ", " + m_maskUVScale.y + ", " + m_maskUVOffset.x + ", " + m_maskUVOffset.y + " }" );
				}

				// Layer 0-only maps
				if ( _options.m_hasOcclusionMap && m_AO != null ) {
					W.WriteLine( T + "occlusionmap	" + m_AO.Export() );		// <= At the moment, only layer 0 should write AO, it's an error to write AO for more than 1 layer!
					if ( m_index > 0 )
						throw new Exception( "A material shouldn't be writing an AO map for other layers than layer 0!" );
				}
				if ( _options.m_translucencyEnabled && !_options.m_translucencyUseVertexColor && m_translucency != null ) {
					W.WriteLine( T + "translucencymap	" + m_translucency.Export() );	// <= At the moment, only layer 0 should write translucency, it's an error to write translucency for more than 1 layer!
					if ( m_index > 0 )
						throw new Exception( "A material shouldn't be writing an AO map for other layers than layer 0!" );
				}
			}

			#endregion

			#region Analysis

			/// <summary>
			/// Checks the provided texture is valid
			/// </summary>
			/// <param name="_texture"></param>
			/// <param name="_reUseMode"></param>
			/// <param name="T"></param>
			/// <param name="_textureName"></param>
			/// <returns></returns>
			public void	CheckTexture( Texture _texture, bool _hasUseOption, REUSE_MODE _reUseMode, string T, string _textureName, int _expectedChannelsCount ) {
				if ( _reUseMode != REUSE_MODE.DONT_REUSE )
					return;	// Don't error when re-using previous channel textures

				if ( _texture == null ) {
					if ( _hasUseOption ) {
						m_errors += T + "• No " + _textureName + " texture!\n";
						RaiseErrorLevel( ERROR_LEVEL.DANGEROUS );
					}
				} else if ( _texture.m_constantColorType == Texture.CONSTANT_COLOR_TYPE.TEXTURE ) {
					if ( !_hasUseOption )
						m_warnings += T + "• Specifying " + _textureName + " texture whereas option is not set!\n";

					// In case of texture, ensure it exists!
					if ( !_texture.m_fileName.Exists ) {
						m_errors += T + "• " + _textureName + " texture \"" + _texture.m_fileName.FullName + "\" not found on disk!\n";
						RaiseErrorLevel( ERROR_LEVEL.DIRTY );
					}
					else if ( _texture.m_textureFileInfo == null ) {
						m_errors += T + "• " + _textureName + " texture \"" + _texture.m_fileName.FullName + "\"  not found in collected textures!\n";
						RaiseErrorLevel( ERROR_LEVEL.DIRTY );
					}
					else {
						// Ensure we have the proper amount of channels
						if ( _texture.m_textureFileInfo.ColorChannelsCount != _expectedChannelsCount ) {
							string	errorText = T + "• " + _textureName + " texture \"" + _texture.m_fileName.FullName + "\" provides " + _texture.m_textureFileInfo.ColorChannelsCount + " color channels whereas " + _expectedChannelsCount + " are expected!\n";
							if ( _texture.m_textureFileInfo.m_usage == TextureFileInfo.USAGE.DIFFUSE && _expectedChannelsCount == 1 ) {
								m_warnings = errorText;	// If an _d was set instead of a gloss or a metal, only issue a warning
							} else {
								m_errors += errorText;	// Otherwise it's an actual error!
								RaiseErrorLevel( ERROR_LEVEL.DANGEROUS );
							}
						}
					}
				}
			}

			public void	CompareTextures( Texture _texture0, REUSE_MODE _reUseMode0, Texture _texture1, REUSE_MODE _reUseMode1, Layer _layer1, string T, string _textureName0, string _textureName1 ) {
				if ( _texture0 == null && _texture1 == null )
					return;	// Easy!

				if ( _texture0 == null ) {
					if ( _reUseMode0 == REUSE_MODE.DONT_REUSE )
						m_errors += T + "• " + _textureName0 + " is not provided whereas " + _textureName1 + " is specified and re-use mode is " + _reUseMode0 + "\n";
					return;
				} else if ( _texture1 == null ) {
					if ( _reUseMode1 == REUSE_MODE.DONT_REUSE  )
						_layer1.m_errors += T + "• " + _textureName1 + " is not provided whereas " + _textureName0 + " is specified and re-use mode is " + _reUseMode0 + "\n";
					return;
				}

				// At this point we know both textures are set
				if ( _reUseMode0 != REUSE_MODE.DONT_REUSE )
					m_warnings += T + "• " + _textureName0 + " is provided (\"" + _texture0.m_name + "\") whereas re-use mode is specified. It's not optimal, you should remove the texture...\n";
				if ( _reUseMode1 != REUSE_MODE.DONT_REUSE )
					_layer1.m_warnings += T + "• " + _textureName1 + " is provided (\"" + _texture1.m_name + "\")  whereas re-use mode is specified. It's not optimal, you should remove the texture...\n";

				if ( _reUseMode0 == REUSE_MODE.DONT_REUSE && _reUseMode1 == REUSE_MODE.DONT_REUSE ) {
					// Check if both textures are the same
					if ( _texture0 == _texture1 ) {
						// Check if UV sets and tiling is the same
						if ( SameUVs( _layer1 ) ) 
							m_errors += T + "• " + _textureName0 + " and " + _textureName1 + " are identical and use the same UV sets, tiling and offsets! Consider re-using other layer texture instead!\n";
					}
				}
			}

			#endregion

			#region Cleanup + Optimization

			public void	CleanUp( List< Layer > _layers, Options _options, ref int _removedTexturesCount, ref int _blackColorConstantsCount, ref int _swappedSlotsCount, ref int _missingTexturesReplacedCount, ref int _removedHasOcclusionMapOptionsCount, ref int _reUseOptionsSetCount ) {
				// Cleanup textures that are present although the option is not set
				if ( !_options.m_hasNormal && m_normal != null ) {
					m_normal = null;
					_removedTexturesCount++;
				}
				if ( !_options.m_hasGloss && m_gloss != null ) {
					m_gloss = null;
					_removedTexturesCount++;
				}
				if ( !_options.m_hasMetal && m_metal != null ) {
					m_metal = null;
					_removedTexturesCount++;
				}
				if ( !_options.m_hasEmissive && m_emissive != null ) {
					m_emissive = null;
					_removedTexturesCount++;
				}
				if ( !_options.m_hasSpecular && m_specular != null ) {
					m_specular = null;
					_removedTexturesCount++;
				}
				if ( !_options.m_hasOcclusionMap && m_AO != null ) {
					m_AO = null;
					_removedTexturesCount++;
				}

				// Cleanup textures that are present whereas a re-use option is set
				if ( m_diffuse != null && m_diffuseReUse != REUSE_MODE.DONT_REUSE )
					m_diffuse = null;
				if ( m_normal != null && m_normalReUse != REUSE_MODE.DONT_REUSE )
					m_normal = null;
				if ( m_gloss != null && m_glossReUse != REUSE_MODE.DONT_REUSE )
					m_gloss = null;
				if ( m_metal != null && m_metalReUse != REUSE_MODE.DONT_REUSE )
					m_metal = null;
				if ( m_specular != null && m_specularReUse != REUSE_MODE.DONT_REUSE )
					m_specular = null;

				// Patch missing textures
				if ( m_diffuse == null && m_diffuseReUse == REUSE_MODE.DONT_REUSE ) {
					m_diffuse = new Texture( "_invalid" );
					_missingTexturesReplacedCount++;
				}
				if ( _options.m_hasNormal && m_normal == null && m_normalReUse == REUSE_MODE.DONT_REUSE ) {
					m_normal = new Texture( "ipr_constantcolor( 0.5, 0.5, 0, 0 )" );
					_missingTexturesReplacedCount++;
				}
				if ( _options.m_hasGloss && m_gloss == null && m_glossReUse == REUSE_MODE.DONT_REUSE ) {
					m_gloss = new Texture( "_white" );
					_missingTexturesReplacedCount++;
				}
				if ( _options.m_hasMetal && m_metal == null && m_metalReUse == REUSE_MODE.DONT_REUSE ) {
					m_metal = new Texture( "_white" );
					_missingTexturesReplacedCount++;
				}
				if ( _options.m_hasEmissive && m_emissive == null ) {
					m_emissive = new Texture( "_invalid" );
					_missingTexturesReplacedCount++;
				}
				if ( _options.m_hasSpecular && m_specular == null && m_specularReUse == REUSE_MODE.DONT_REUSE ) {
					m_specular = new Texture( "_invalid" );
					_missingTexturesReplacedCount++;
				}

				// Clear "hasOcclusionMap" option when we don't have a map after all
				if ( _options.m_hasOcclusionMap && m_AO == null && m_index == 0 ) {
					_options.m_hasOcclusionMap = false;
					_removedHasOcclusionMapOptionsCount++;
				}

				// Replace diffuse textures that use a black constant color multiplier
				if ( m_useColorConstant && (m_colorConstant.x*m_colorConstant.x + m_colorConstant.y*m_colorConstant.y + m_colorConstant.z*m_colorConstant.z) < 1e-6f ) {
					m_diffuse = new Texture( "_black" );
					_blackColorConstantsCount++;
				}

				// Try swapping slots if the user made obvious mistakes
				TrySwapping( ref m_diffuse,	ref m_normal,	TextureFileInfo.USAGE.DIFFUSE,	TextureFileInfo.USAGE.NORMAL, ref _swappedSlotsCount );
				TrySwapping( ref m_diffuse,	ref m_gloss,	TextureFileInfo.USAGE.DIFFUSE,	TextureFileInfo.USAGE.GLOSS, ref _swappedSlotsCount );
				TrySwapping( ref m_diffuse,	ref m_metal,	TextureFileInfo.USAGE.DIFFUSE,	TextureFileInfo.USAGE.METAL, ref _swappedSlotsCount );
				TrySwapping( ref m_diffuse,	ref m_emissive,	TextureFileInfo.USAGE.DIFFUSE,	TextureFileInfo.USAGE.EMISSIVE, ref _swappedSlotsCount );
				TrySwapping( ref m_normal,	ref m_gloss,	TextureFileInfo.USAGE.NORMAL,	TextureFileInfo.USAGE.GLOSS, ref _swappedSlotsCount );
				TrySwapping( ref m_normal,	ref m_metal,	TextureFileInfo.USAGE.NORMAL,	TextureFileInfo.USAGE.METAL, ref _swappedSlotsCount );
				TrySwapping( ref m_normal,	ref m_emissive,	TextureFileInfo.USAGE.NORMAL,	TextureFileInfo.USAGE.EMISSIVE, ref _swappedSlotsCount );
				TrySwapping( ref m_gloss,	ref m_metal,	TextureFileInfo.USAGE.GLOSS,	TextureFileInfo.USAGE.METAL, ref _swappedSlotsCount );
				TrySwapping( ref m_gloss,	ref m_emissive,	TextureFileInfo.USAGE.GLOSS,	TextureFileInfo.USAGE.EMISSIVE, ref _swappedSlotsCount );
				TrySwapping( ref m_metal,	ref m_emissive,	TextureFileInfo.USAGE.METAL,	TextureFileInfo.USAGE.EMISSIVE, ref _swappedSlotsCount );

				// Apply re-use options whenever a texture is identical from the previous layer
				for ( int previousLayerIndex=0; previousLayerIndex < m_index; previousLayerIndex++ ) {
					Layer		previousLayer = _layers[previousLayerIndex];
					REUSE_MODE	previousLayerReUseMode = REUSE_MODE.DONT_REUSE;
					switch ( previousLayerIndex ) {
						case 0: previousLayerReUseMode = REUSE_MODE.REUSE_LAYER0; break;
						case 1: previousLayerReUseMode = REUSE_MODE.REUSE_LAYER1; break;
					}

					if ( previousLayer.SameUVs( previousLayer ) ) {
						// Can re-use some textures?
						if ( m_diffuse != null && previousLayer.m_diffuse != null && m_diffuseReUse == REUSE_MODE.DONT_REUSE && previousLayer.m_diffuseReUse == REUSE_MODE.DONT_REUSE ) {
							if ( m_diffuse == previousLayer.m_diffuse ) {
								m_diffuseReUse = previousLayerReUseMode;
								m_diffuse = null;
								_reUseOptionsSetCount++;
								_removedTexturesCount++;
							}
						}
						if ( m_normal != null && previousLayer.m_normal != null && m_normalReUse == REUSE_MODE.DONT_REUSE && previousLayer.m_normalReUse == REUSE_MODE.DONT_REUSE ) {
							if ( m_normal == previousLayer.m_normal ) {
								m_normalReUse = previousLayerReUseMode;
								m_normal = null;
								_reUseOptionsSetCount++;
								_removedTexturesCount++;
							}
						}
						if ( m_gloss != null && previousLayer.m_gloss != null && m_glossReUse == REUSE_MODE.DONT_REUSE && previousLayer.m_glossReUse == REUSE_MODE.DONT_REUSE ) {
							if ( m_gloss == previousLayer.m_gloss ) {
								m_glossReUse = previousLayerReUseMode;
								m_gloss = null;
								_reUseOptionsSetCount++;
								_removedTexturesCount++;
							}
						}
						if ( m_metal != null && previousLayer.m_metal != null && m_metalReUse == REUSE_MODE.DONT_REUSE && previousLayer.m_metalReUse == REUSE_MODE.DONT_REUSE ) {
							if ( m_metal == previousLayer.m_metal ) {
								m_metalReUse = previousLayerReUseMode;
								m_metal = null;
								_reUseOptionsSetCount++;
								_removedTexturesCount++;
							}
						}
						if ( m_specular != null && previousLayer.m_specular != null && m_specularReUse == REUSE_MODE.DONT_REUSE && previousLayer.m_specularReUse == REUSE_MODE.DONT_REUSE ) {
							if ( m_specular == previousLayer.m_specular ) {
								m_specularReUse = previousLayerReUseMode;
								m_specular = null;
								_reUseOptionsSetCount++;
								_removedTexturesCount++;
							}
						}
					}
				}
			}

			void TrySwapping( ref Texture _a, ref Texture _b, TextureFileInfo.USAGE _expectedUsageForA, TextureFileInfo.USAGE _expectedUsageForB, ref int _swappedSlotsCount ) {
				if ( _a == null || _a.m_textureFileInfo == null )
					return;
				if ( _b == null || _b.m_textureFileInfo == null )
					return;

				if ( _a.m_textureFileInfo.m_usage == _expectedUsageForB && _b.m_textureFileInfo.m_usage == _expectedUsageForA ) {
					// Switch!
					Texture	temp = _a;
					_a = _b;
					_b = temp;
					_swappedSlotsCount++;
				}
			}

			#endregion
		}

		#endregion

		public FileInfo			m_sourceFileName = null;
		public string			m_name = null;

		public int				m_version = -1;

		public Programs			m_programs = new Programs();

		public Parms			m_parms = new Parms();		// Parms (with sort stage)
		public States			m_states = new States();	// Render states (with blend states)
		public Options			m_options = new Options();	// Options

		// Textures
		public List< Layer >	m_layers = new List< Layer >();
		public Layer.Texture	m_height = null;	// Special height map!
		public Layer.Texture	m_lightMap = null;	// Special light map for vista!

		// Main variables
		public string			m_physicsMaterial = null;
		public float2			m_glossMinMax = new float2( 0.0f, 0.5f );
		public float2			m_metallicMinMax = new float2( 0.0f, 0.5f );

		// Shader specific parms
		public bool				m_isUsingVegetationParms = false;
		public bool				m_isUsingCloudParms = false;
		public bool				m_isUsingWaterParms = false;

		// Unknown parms/options/etc. that need to be restored as-is
		public List< string >	m_unknownVariables = new List< string >();

		// Forbidden parms
		public List< string >	m_forbiddenParms = new List< string >();


		// Filled by analyzer
		private ERROR_LEVEL		m_errorLevel = ERROR_LEVEL.NONE;
		public string			m_errors = null;
		public string			m_warnings = null;

		#region PROPERTIES

		public bool				IsAlpha {
			get { return m_options.IsAlpha || m_states.IsAlpha || m_parms.IsAlpha; }
		}

		public bool				IsOptimized {
			get {
				foreach ( Layer L in m_layers )
					if ( !L.IsOptimized )
						return false;
				return true;
			}
		}

		/// <summary>
		/// Returns the ACTUAL amount of layers used by the shader
		/// </summary>
		public int				LayersCount {
			get { return 1+m_options.m_extraLayers; }
		}
		public int				SafeLayersCount {
			get { return Math.Min( LayersCount, m_layers.Count ); }
		}

		public bool				HasErrors {
			get {
				if ( m_errors != null && m_errors != "" )
					return true;
				foreach ( Layer L in m_layers )
					if ( L.HasErrors )
						return true;
				return false;
			}
		}

		public ERROR_LEVEL		ErrorLevel_MaterialOnly {
			get { return m_errorLevel; }
		}
		public ERROR_LEVEL		ErrorLevel {
			get {
				int	maxErrorLevel = (int) m_errorLevel;
				foreach ( Layer L in m_layers ) {
					maxErrorLevel = Math.Max( maxErrorLevel, (int) L.ErrorLevel );
				}

				return (ERROR_LEVEL) maxErrorLevel;
			}
		}

		/// <summary>
		/// Build error summary
		/// </summary>
		public string			ErrorString {
			get {
				string	R = null;
				if ( m_errors != null && m_errors != "" ) {
					R += "General errors:\n" + m_errors + "\n\n";
				}
				for ( int layerIndex=0; layerIndex < SafeLayersCount; layerIndex++ ) {
					Layer	L = m_layers[layerIndex];
					if ( L.m_errors != null && L.m_errors != "" ) {
						R += "Layer " + layerIndex + " errors:\n" + L.m_errors + "\n\n";
					}
				}
				return R;
			}
		}

		public bool				HasWarnings {
			get {
				if ( m_warnings != null && m_warnings != "" )
					return true;
				foreach ( Layer L in m_layers )
					if ( L.HasWarnings )
						return true;
				return false;
			}
		}

		/// <summary>
		/// Build warning summary
		/// </summary>
		public string			WarningString {
			get {
				string	R = null;
				if ( m_warnings != null && m_warnings != "" ) {
					R += "General warnings:\n" + m_warnings + "\n\n";
				}
				for ( int layerIndex=0; layerIndex < SafeLayersCount; layerIndex++ ) {
					Layer	L = m_layers[layerIndex];
					if ( L.m_warnings != null && L.m_warnings != "" ) {
						R += "Layer " + layerIndex + " warnings:\n" + L.m_warnings + "\n\n";
					}
				}
				return R;
			}
		}

		public bool				HasPhysicsMaterial {
			get { return m_physicsMaterial != null && m_physicsMaterial != ""; }
		}

		private Layer			Layer0 {
			get { return m_layers[0]; }
		}

		private Layer			Layer1 {
			get {
				if ( m_layers.Count < 2 )
					m_layers.Add( new Layer( this, 1 ) );
				return m_layers[1];
			}
		}

		private Layer			Layer2 {
			get {
				if ( m_layers.Count < 3 ) {
					if ( m_layers.Count < 2 )
						m_layers.Add( new Layer( this, 1 ) );
					m_layers.Add( new Layer( this, 2 ) );
				}
				return m_layers[2];
			}
		}

		#endregion

		public Material( FileInfo _sourceFileName, string _name, string _content ) {
			m_sourceFileName = _sourceFileName;
			m_name = _name;
			m_layers.Add( new Layer( this, 0 ) );	// We always have at least 1 layer
			Parse( _content );
		}
		public Material( BinaryReader R ) {
			Read( R );
		}

		public override string ToString() {
			string	R = null;

			R += m_name + "\n";
			R += "M2 File: " + m_sourceFileName.FullName + "\n";
			R += "Material type = " + m_programs.m_type + "\n";
			R += (1+m_options.m_extraLayers) + " layers (" + m_layers.Count + " encountered)\n";

			for ( int layerIndex=0; layerIndex < LayersCount; layerIndex++ ) {
				string	layerString = m_layers[layerIndex].ToString();
				if ( layerString != null && layerString != "" )
					R += "\n• Layer " + layerIndex + "\n----------\n" + layerString + "\n";
			}

			if ( m_errors != null && m_errors != "" ) {
				R += "\nGeneral Errors:\n----------------------\n" + m_errors;
			}
			if ( m_warnings != null && m_warnings != "" ) {
				R += "\nGeneral Warnings:\n----------------------\n" + m_warnings;
			}

			return R;
		}

		public void		ClearErrorLevel() {
			m_errorLevel = ERROR_LEVEL.NONE;
		}
		public void		RaiseErrorLevel( ERROR_LEVEL _errorLevel ) {
			if ( (int) m_errorLevel < (int) _errorLevel )
				m_errorLevel = _errorLevel;
		}

		#region Material Parsing

		private void	Parse( string _block ) {
			m_states.m_unknownStates.Clear();
			m_parms.m_unknownParms.Clear();
			m_options.m_unknownOptions.Clear();
			m_unknownVariables.Clear();
			m_forbiddenParms.Clear();

			m_isUsingVegetationParms = false;
			m_isUsingCloudParms = false;
			m_isUsingWaterParms = false;

			Parser	P = new Parser( _block );
			while ( P.OK ) {
				string	token = P.ReadString();
				if ( token == null )
					break;	// Done!
				if ( token.StartsWith( "//" ) ) {
					RecordSingleLineCommentVariable( token, P );
					continue;
				}
				if ( token.StartsWith( "/*" ) ) {
					RecordCommentVariable( token, P );
					continue;
				}

				if ( token.EndsWith( "{" ) ) {
					// Handle problematic parms without space before their values
					token = token.Substring( 0, token.Length-1 );
					P.m_Index--;
				}

				P.SkipSpaces();

				switch ( token.ToLower() ) {
					case "version":
						P.SkipSpaces();
						m_version = P.ReadInteger();
						break;

					case "state":
						m_states.Parse( P.ReadBlock() );
						break;

					case "parms":
						m_parms.Parse( P.ReadBlock() );
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
					case "lightmap":			m_lightMap = new Layer.Texture( P.ReadToEOL() ); break;
					case "occlusionmap":		Layer0.m_AO = new Layer.Texture( P.ReadToEOL() ); break;
					case "translucencymap":		Layer0.m_translucency = new Layer.Texture( P.ReadToEOL() ); break;
					case "emissivemap":			Layer0.m_emissive = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer0_maskmap":		Layer0.m_mask = new Layer.Texture( P.ReadToEOL() ); break;
					case "layer0_scalebias":	Layer0.ParseScaleBias( P ); break;
					case "layer0_maskscalebias":Layer0.ParseMaskScaleBias( P ); break;
					case "layer0_colorconstant":Layer0.m_colorConstant = P.ReadFloat4(); break;

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
					case "layer1_colorconstant":Layer1.m_colorConstant = P.ReadFloat4(); break;

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
					case "layer2_colorconstant":Layer2.m_colorConstant = P.ReadFloat4(); break;

					// Main variables
					case "m_physicsmaterial":
						m_physicsMaterial = P.ReadToEOL();
						break;

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
						if ( CheckSafeTokens( token ) )
							RecordUnknownVariable( token, P );
						else
							RecordForbiddenVariable( token, P );
						break;
				}
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
						RecordSingleLineCommentOption( token, P );
						continue;
					}
					if ( token.StartsWith( "/*" ) ) {
						RecordCommentOption( token, P );
						continue;
					}
					P.SkipSpaces();
					if ( !P.IsNumeric() )
						continue;	// Ill-formed option?

					int		value = P.ReadInteger();

					switch ( token.ToLower() ) {

						case "glossindiffusealpha":			m_options.m_glossInDiffuseAlpha = value != 0; break;
						case "alphatest":					m_options.m_isAlphaTest = value != 0; break;
						case "ismasking":					m_options.m_isMasking = value != 0; break;
						case "hasbumpmap":					m_options.m_hasNormal = value != 0; break;
						case "hasspecularmap":				m_options.m_hasSpecular = value != 0; break;
						case "hasocclusionmap":				m_options.m_hasOcclusionMap = value != 0; break;
						case "hasglossmap":					m_options.m_hasGloss = value != 0; break;
						case "hasmetallicmap":				m_options.m_hasMetal = value != 0; break;
						case "hasemissivemap":				m_options.m_hasEmissive = value != 0; break;
						case "translucency/enable":			m_options.m_translucencyEnabled = value != 0; break;
						case "translucencyusevertexcolor":	m_options.m_translucencyUseVertexColor = value != 0; break;

						case "extralayer":
							switch ( value ) {
								case 0: Layer0.m_mask = null; m_options.m_extraLayers = 0; break;
								case 1: Layer1.m_mask = null; m_options.m_extraLayers = 1; break;
								case 2: Layer2.m_mask = null; m_options.m_extraLayers = 2; break;
								default: throw new Exception( "Unsupported amount of extra layers!" );
							}
							break;

							// LAYER 0
						case "use_layer0_colorconstant": Layer0.m_useColorConstant = value != 0; break;

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
						case "use_layer1_colorconstant": Layer1.m_useColorConstant = value != 0; break;

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
						case "layer1_bumpreuselayer":
							switch ( value ) {
								case 0:	Layer1.m_normalReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer1.m_normalReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
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
						case "use_layer2_colorconstant": Layer2.m_useColorConstant = value != 0; break;

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
								case 0:	Layer2.m_diffuseReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer2.m_diffuseReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
								case 2:	Layer2.m_diffuseReUse = Layer.REUSE_MODE.REUSE_LAYER1; break;
								default: throw new Exception( "Unsupported re-use mode!" );
							}
							break;
						case "layer2_bumpreuselayer":
							switch ( value ) {
								case 0:	Layer2.m_normalReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer2.m_normalReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
								case 2:	Layer2.m_normalReUse = Layer.REUSE_MODE.REUSE_LAYER1; break;
								default: throw new Exception( "Unsupported re-use mode!" );
							}
							break;
						case "layer2_glossreuselayer":
							switch ( value ) {
								case 0:	Layer2.m_glossReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer2.m_glossReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
								case 2:	Layer2.m_glossReUse = Layer.REUSE_MODE.REUSE_LAYER1; break;
								default: throw new Exception( "Unsupported re-use mode!" );
							}
							break;
						case "layer2_specularreuselayer":
							switch ( value ) {
								case 0:	Layer2.m_specularReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer2.m_specularReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
								case 2:	Layer2.m_specularReUse = Layer.REUSE_MODE.REUSE_LAYER1; break;
								default: throw new Exception( "Unsupported re-use mode!" );
							}
							break;
						case "layer2_metallicreuselayer":
							switch ( value ) {
								case 0:	Layer2.m_metalReUse = Layer.REUSE_MODE.DONT_REUSE; break;
								case 1:	Layer2.m_metalReUse = Layer.REUSE_MODE.REUSE_LAYER0; break;
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
							RecordUnknownOption( token, value, P );
							break;
					}
				}
			} catch ( Exception _e ) {
				throw new Exception( "Failed parsing options block!", _e );
			}
		}

		void	RecordUnknownOption( string _token, Parser P ) {
			string	record = _token + "\t" + P.ReadToEOL();
			m_options.m_unknownOptions.Add( record );
		}

		void	RecordUnknownOption( string _token, int value, Parser P ) {
			string	record = _token + "\t" + value + P.ReadToEOL();
			m_options.m_unknownOptions.Add( record );
		}

		void	RecordSingleLineCommentOption( string _token, Parser P ) {
			string	record = _token + P.ReadToEOL();
			m_options.m_unknownOptions.Add( record );
		}

		void	RecordCommentOption( string _token, Parser P ) {
			string	comment = _token + P.SkipComment();
			m_options.m_unknownOptions.Add( comment );
		}

		void	RecordUnknownVariable( string _token, Parser P ) {
			string	record = _token + "\t" + P.ReadToEOL();
			m_unknownVariables.Add( record );
		}

		void	RecordForbiddenVariable( string _token, Parser P ) {
			string	record = _token + "\t" + P.ReadToEOL();
			m_forbiddenParms.Add( record );
		}

		void	RecordSingleLineCommentVariable( string _token, Parser P ) {
			RecordSingleLineCommentVariable( m_unknownVariables, _token, P );
		}
		static void	RecordSingleLineCommentVariable( List< string > _variables, string _token, Parser P ) {
			string	record = _token + P.ReadToEOL();
			_variables.Add( record );
		}
	
		void	RecordCommentVariable( string _token, Parser P ) {
			RecordCommentVariable( m_unknownVariables, _token, P );
		}
		static void	RecordCommentVariable( List< string > _variables, string _token, Parser P ) {
			string	comment = _token + P.SkipComment();
			_variables.Add( comment );
		}
	
		#region Tokens Checking

		bool	CheckSafeTokens( string token ) {
			if ( m_programs.m_type != Programs.KNOWN_TYPES.DEFAULT )
				return true;	// Don't care about other programs than arkDefault

			string[]	recognizedStrings = new string[] {
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
//"lightMap",
"lightMapLuminance",

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


// Obsolete stuff
"roughnessMap",
"wardDiffuseRoughness",
"IBL/intensity",
			};
			token = token.ToLower();
			if ( token.StartsWith( "//" ) )
				return true;
			if ( token.StartsWith( "materialeffects" ) )
				return true;
			if ( token.StartsWith( "decal" ) )
				return true;
			if ( token.StartsWith( "fx/" ) )
				return true;

			//////////////////////////////////////////////////////////////////////////
			// Check okay tokens
			foreach ( string recognizedString in recognizedStrings ) {
				if ( recognizedString.ToLower() == token )
					return true;	// Okay!
			}

			//////////////////////////////////////////////////////////////////////////
			// Check parameters that shouldn't be set by users
			string[]	forbiddenParms = new string[] {
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
			};
			foreach ( string forbiddenParm in forbiddenParms ) {
				if ( forbiddenParm.ToLower() == token ) {
					return false;
				}
			}

			//////////////////////////////////////////////////////////////////////////
			// Check shader-specific parms

				// VEGETATION
			string[]	vegetationRelatedParms = new string[] {
				"vegetanim/leaffreq",
				"vegetanim/branchamplitude",
				"vegetanim/mainbendingamplitude",
				"vegetanim/mainbendingvar",
				"vegetanim/mainbendingfreq",
				"vegetanim/leafamplitude",
				"vegetanim/leafMinWindStrength",
				"vegetation/transmittanceParms",
				"vegetation/transmittanceMap",
				"vegetation/transmittanceMapIntensity",
				"vegetation/indirectTranslucencyBoost",
			};
			foreach ( string vegetationParm in vegetationRelatedParms ) {
				if ( vegetationParm.ToLower() == token ) {
					m_isUsingVegetationParms = true;
					return true;
				}
			}

				// CLOUD
			string[]	cloudRelatedParms = new string[] {
				"cloudProjOnFar",
				"cloudDensityCoef",
				"cloudVerticalAlpha",
				"cloudBackLitCoef",
			};
			foreach ( string cloudParm in cloudRelatedParms ) {
				if ( cloudParm.ToLower() == token ) {
					m_isUsingCloudParms = true;
					return true;
				}
			}

				// WATER
			string[]	waterRelatedParms = new string[] {
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
			};
			foreach ( string waterParm in waterRelatedParms ) {
				if ( waterParm.ToLower() == token ) {
					m_isUsingWaterParms = true;
					return true;
				}
			}

// 			int	glou = 0;
// 			glou++;
//			throw new Exception( "Unrecognized token \"" + token + "\"!" );
			return true;
		}

		void	CheckSafeOptionsTokens( string token, int value ) {
// // 			if ( m_programs.m_type != Programs.KNOWN_TYPES.DEFAULT )
// // 				return;	// Don't care about other programs than arkDefault
// 
// 			string[]	recognizedStrings = new string[] {
// 			};
// 			token = token.ToLower();
// 			if ( token.StartsWith( "//" ) )
// 				return;
// 			if ( token.StartsWith( "materialeffects" ) )
// 				return;
// 			if ( token.StartsWith( "decal" ) )
// 				return;
// 			if ( token.StartsWith( "fx/" ) )
// 				return;
// 
// 			foreach ( string recognizedString in recognizedStrings ) {
// 				if ( recognizedString.ToLower() == token )
// 					return;	// Okay!
// 			}
// 
//  			int	glou = 0;
//  			glou++;
// //			throw new Exception( "Unrecognized token!" );
		}

		#endregion

		#endregion

		#region Serialization

		public void	Write( BinaryWriter W ) {
			W.Write( m_sourceFileName.FullName );
			W.Write( m_name );

			W.Write( m_version );

			m_programs.Write( W );
			m_parms.Write( W );
			m_states.Write( W );
			m_options.Write( W );

			W.Write( m_layers.Count );
			foreach ( Layer L in m_layers ) {
				L.Write( W );
			}

			W.Write( m_height != null );
			if ( m_height != null )
				m_height.Write( W );
			W.Write( m_lightMap != null );
			if ( m_lightMap != null )
				m_lightMap.Write( W );
			
			W.Write( m_physicsMaterial != null ? m_physicsMaterial : "" );
			W.Write( m_glossMinMax.x );
			W.Write( m_glossMinMax.y );
			W.Write( m_metallicMinMax.x );
			W.Write( m_metallicMinMax.y );

			W.Write( m_isUsingVegetationParms );
			W.Write( m_isUsingCloudParms );
			W.Write( m_isUsingWaterParms );

			// Write unknown strings that should be restored as-is
			WriteListOfStrings( W, m_unknownVariables );
			WriteListOfStrings( W, m_forbiddenParms );

			W.Write( (int) m_errorLevel );
			W.Write( m_errors != null ? m_errors : "" );
			W.Write( m_warnings != null ? m_warnings : "" );
		}

		public void	Read( BinaryReader R ) {
			m_sourceFileName = new FileInfo( R.ReadString() );
			m_name = R.ReadString();

			m_version = R.ReadInt32();

			m_programs.Read( R );
			m_parms.Read( R );
			m_states.Read( R );
			m_options.Read( R );

			m_layers.Clear();
			int	layersCount = R.ReadInt32();
			for ( int layerIndex=0; layerIndex < layersCount; layerIndex++ ) {
				m_layers.Add( new Layer( this, layerIndex, R ) );
			}

			m_height = R.ReadBoolean() ? new Layer.Texture( R ) : null;
			m_lightMap = R.ReadBoolean() ? new Layer.Texture( R ) : null;
			
			m_physicsMaterial = R.ReadString();
			if ( m_physicsMaterial == "" )
				m_physicsMaterial = null;
			m_glossMinMax.x = R.ReadSingle();
			m_glossMinMax.y = R.ReadSingle();
			m_metallicMinMax.x = R.ReadSingle();
			m_metallicMinMax.y = R.ReadSingle();

			m_isUsingVegetationParms = R.ReadBoolean();
			m_isUsingCloudParms = R.ReadBoolean();
			m_isUsingWaterParms = R.ReadBoolean();

			// Read unknown strings that should be restored as-is
			ReadListOfStrings( R, m_unknownVariables );
			ReadListOfStrings( R, m_forbiddenParms );

			// Read errors and warnings
			m_errorLevel = (ERROR_LEVEL) R.ReadInt32();
			m_errors = R.ReadString();
			if ( m_errors == string.Empty )
				m_errors = null;
			m_warnings = R.ReadString();
			if ( m_warnings == string.Empty )
				m_warnings = null;
		}

		static void	WriteListOfStrings( BinaryWriter W, List< string > _list ) {
			W.Write( _list.Count );
			foreach ( string value in _list )
				W.Write( value );
		}

		static void	ReadListOfStrings( BinaryReader R, List< string > _list ) {
			_list.Clear();
			int	stringsCount = R.ReadInt32();
			for ( int stringIndex=0; stringIndex < stringsCount; stringIndex++ )
				_list.Add( R.ReadString() );
		}

		#endregion

		#region Material Optimizer

		/// <summary>
		/// This function cleans the material from automatically recoverable errors and warnings
		/// </summary>
		public void		CleanUp( ref int _clearedOptionsCount, ref int _removedTexturesCount, ref int _blackColorConstantsCount, ref int _swappedSlotsCount, ref int _missingTexturesReplacedCount, ref int _removedHasOcclusionMapOptionsCount, ref int _reUseOptionsSetCount ) {

			// Check gloss/metal ranges to know if textures are useful
			bool	emptyGlossRange = Math.Abs( m_glossMinMax.y - m_glossMinMax.x ) < 1e-3f;
			bool	emptyMetalRange = Math.Abs( m_metallicMinMax.y - m_metallicMinMax.x ) < 1e-3f;

			// Clear options
			if ( emptyGlossRange && m_options.m_hasGloss ) {
				m_options.m_hasGloss = false;
				_clearedOptionsCount++;
			}
			if ( emptyMetalRange && m_options.m_hasMetal ) {
				m_options.m_hasMetal = false;
				_clearedOptionsCount++;
			}

			// Cleanup layers
			while ( m_layers.Count > LayersCount )
				m_layers.RemoveAt( m_layers.Count-1 );	// Remove last layer
			foreach ( Layer L in m_layers ) {
				L.CleanUp( m_layers, m_options, ref _removedTexturesCount, ref _blackColorConstantsCount, ref _swappedSlotsCount, ref _missingTexturesReplacedCount, ref _removedHasOcclusionMapOptionsCount, ref _reUseOptionsSetCount );
			}

			// Remove forbidden parms
			m_forbiddenParms.Clear();

			// Remove standard comments
			CleanUpUselessComments( m_parms.m_unknownParms );
			CleanUpUselessComments( m_states.m_unknownStates );
			CleanUpUselessComments( m_options.m_unknownOptions );
			CleanUpUselessComments( m_unknownVariables );
		}

		public void	CollectDiffuseGlossTextures( Dictionary< TextureFileInfo, List< TextureFileInfo > > _diffuse2GlossMaps ) {
			if ( IsAlpha )
				return;	// Don't collect alpha materials

			// Check the material is arkDefault
			if ( m_programs.m_type != Programs.KNOWN_TYPES.DEFAULT )
				return;	// We could but I chose not to deal with other shaders than arkDefault...

			// Check the material uses diffuse and gloss
			if ( !m_options.m_hasGloss )
				return;	// No gloss maps to compact

			foreach ( Layer L in m_layers ) {
				Layer.Texture	diffuse = L.Diffuse;
				if ( diffuse == null || diffuse.m_textureFileInfo == null || diffuse.m_textureFileInfo.m_usage != TextureFileInfo.USAGE.DIFFUSE )
					continue;	// No diffuse slot?

				if ( !_diffuse2GlossMaps.ContainsKey( diffuse.m_textureFileInfo ) )
					_diffuse2GlossMaps.Add( diffuse.m_textureFileInfo, new List< TextureFileInfo >() );

				List< TextureFileInfo >	glossMaps = _diffuse2GlossMaps[diffuse.m_textureFileInfo];
				Layer.Texture			glossMap = L.Gloss;

				if ( glossMap != null && glossMap.m_textureFileInfo != null && glossMap.m_textureFileInfo.m_usage == TextureFileInfo.USAGE.GLOSS )
					glossMaps.Add( glossMap.m_textureFileInfo );
				else
					glossMaps.Add( null );	// Add null anyway so we can know this texture is sometimes lacking a gloss map, which also means it's shared by the "no gloss" texture...
			}
		}

		/// <summary>
		/// This function optimizes the material by compacting diffuse and gloss textures into a single "_dg" texture whenever possible
		/// </summary>
		/// <param name="_totalDiffuseGlossTexturesReplaced"></param>
		public bool	Optimize( ref int _totalDiffuseGlossTexturesReplaced ) {
			if ( IsAlpha )
				return false;	// Can't optimize alpha materials

			// Check the material is arkDefault
			if ( m_programs.m_type != Programs.KNOWN_TYPES.DEFAULT )
				return false;	// We could but I chose not to deal with other shaders than arkDefault...

			// Check the material uses diffuse and gloss
			if ( !m_options.m_hasGloss )
				return false;	// No gloss maps to compact

			bool	allLayersAreUsingPairedTexture = true;
			foreach ( Layer L in m_layers ) {
				Layer.Texture	diffuse = L.Diffuse;
				if ( diffuse == null || diffuse.m_textureFileInfo == null || diffuse.m_textureFileInfo.m_associatedTexture == null ) {
					allLayersAreUsingPairedTexture = false;
					break;
				}
			}

			if ( !allLayersAreUsingPairedTexture )
				return false;

			// Set the "glossInDiffuseAlpha" option and clear the "hasGloss" one
			m_options.m_glossInDiffuseAlpha = true;
			m_options.m_hasGloss = false;

			// Replace all diffuse textures by their "_dg" equivalents and remove gloss textures
			foreach ( Layer L in m_layers ) {
				if ( L.m_diffuseReUse != Layer.REUSE_MODE.DONT_REUSE )
					continue;

				string	originalTextureName = L.m_diffuse.m_rawTextureLine;
				string	optimizedDiffuseTextureName = TextureFileInfo.GetOptimizedDiffuseGlossNameFromDiffuseName( originalTextureName );
						optimizedDiffuseTextureName = Path.ChangeExtension( optimizedDiffuseTextureName, ".png" );

				L.m_diffuseBeforeOptimization = L.m_diffuse;	// Keep track of original diffuse
				L.m_diffuse = new Layer.Texture( optimizedDiffuseTextureName );	// Replace with 

				// Check gloss is okay then remove it
				Layer.Texture	gloss = L.m_gloss;
				switch ( L.m_glossReUse ) {
					case Layer.REUSE_MODE.REUSE_LAYER0: gloss = m_layers[0].m_glossBeforeOptimization; break;
					case Layer.REUSE_MODE.REUSE_LAYER1: gloss = m_layers[1].m_glossBeforeOptimization; break;
				}
				if ( gloss == null || gloss.m_textureFileInfo == null || gloss.m_textureFileInfo.m_usage != TextureFileInfo.USAGE.GLOSS )
					throw new Exception( "Diffuse slot about to be optimized is not attached to a proper gloss texture! How could it be listed as optimizable then?" );

				L.m_glossBeforeOptimization = L.m_gloss;	// Keep track of original gloss
				L.m_gloss = null;

				_totalDiffuseGlossTexturesReplaced++;
			}

			return true;
		}

		public void		CleanUpUselessComments( List< string > _unknownStrings ) {
			string[]	annoyingComments =  new string[] {
				"// bumpmap",
				"// specularmap",
				"// //specularmap",
				"// glossmap",
				"// metallicmap",
				"//deprecated",
			};
			string[]	sourceUnknownVariables = _unknownStrings.ToArray();
			_unknownStrings.Clear();
			foreach ( string unknownVariable in sourceUnknownVariables ) {
				string	trimmed = unknownVariable.Trim().ToLower();
				bool	skip = false;
				foreach ( string annoyingComment in annoyingComments ) {
					if ( trimmed.StartsWith( annoyingComment ) ) {
						// Found a recognized comment we want to clean up
						skip = true;
						break;
					}
				}

				if ( !skip )
					_unknownStrings.Add( unknownVariable );	// Okay, we can re-add the variable...
			}
		}

		#endregion

		#region Material Exporter

		/// <summary>
		/// This attempts to re-export a valid material for a M2 file
		/// </summary>
		/// <param name="W"></param>
		public void	Export( StringWriter W ) {
			string	T = "\t";
			W.WriteLine( "material " + m_name + " {" );

			if ( m_version >= 0 )
				W.WriteLine( T + "version	" + m_version );

			if ( m_physicsMaterial != null )
				W.WriteLine( T + "m_PhysicsMaterial	" + m_physicsMaterial );

			// Write parms
			m_parms.Export( W, T );

			// Write state
			m_states.Export( W, T );

			// Write options
			W.WriteLine( T + "options {" );
			m_options.Export( W, T, m_layers );
			W.WriteLine( T + "}" );

			// Write programs
			W.WriteLine();
			m_programs.Export( W, T );

			// Write regular variables
			W.WriteLine();
			W.WriteLine( T + "wardroughness	{ " + m_glossMinMax.x + ", " + m_glossMinMax.y + " }" );
			W.WriteLine( T + "metallicminmax	{ " + m_metallicMinMax.x + ", " + m_metallicMinMax.y + " }" );

			if ( m_height != null )
				W.WriteLine( T + "heightmap	" + m_height.Export() );
			if ( m_lightMap != null )
				W.WriteLine( T + "lightmap	" + m_lightMap.Export() );

			// Write variables from layers
			for ( int layerIndex=0; layerIndex < SafeLayersCount; layerIndex++ ) {
				Layer	L = m_layers[layerIndex];

				string	layerPrefix = null;
				switch ( layerIndex ) {
					case 0: layerPrefix = "layer0_"; break;
					case 1: layerPrefix = "layer1_"; break;
					case 2: layerPrefix = "layer2_"; break;
				}

				L.Export( W, T, layerPrefix, m_options );
				if ( layerIndex < SafeLayersCount-1 )
					W.WriteLine();
			}

			// Write unknown variables
			foreach ( string unknownVariable in m_unknownVariables )
				W.WriteLine( T + unknownVariable );

			W.WriteLine( "}\n" );
		}

		#endregion
	}
}
