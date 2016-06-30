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
		}

		[System.Diagnostics.DebuggerDisplay( "Alpha={m_isAlpha}" )]
		public class	Options {
			public bool				m_isAlpha = false;
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
				get { return m_isAlpha || m_isAlphaTest || m_isMasking; }
			}

			#region Serialization

			public void	Write( BinaryWriter W ) {
				W.Write( m_isAlpha );
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
			}

			public void	Read( BinaryReader R ) {
				m_isAlpha = R.ReadBoolean();
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
			}

			#endregion

			public void		Export( StringWriter W, string T, List< Layer > _layers ) {
				T += "\t";	// Indent

				// Write known options
				W.WriteLine( T + "isalpha	" + (m_isAlpha ? 1 : 0) );
				W.WriteLine( T + "alphatest	" + (m_isAlphaTest ? 1 : 0) );
				W.WriteLine( T + "ismasking	" + (m_isMasking ? 1 : 0) );
				W.WriteLine( T + "hasbumpmap	" + (m_hasNormal ? 1 : 0) );
				W.WriteLine( T + "hasspecularmap	" + (m_hasSpecular ? 1 : 0) );
				W.WriteLine( T + "hasOcclusionMap	" + (m_hasOcclusionMap ? 1 : 0) );
				W.WriteLine( T + "hasglossmap	" + (m_hasGloss ? 1 : 0) );
				W.WriteLine( T + "hasmetallicmap	" + (m_hasMetal ? 1 : 0) );
				W.WriteLine( T + "hasemissivemap	" + (m_hasEmissive ? 1 : 0) );
				W.WriteLine( T + "translucency/enable	" + (m_translucencyEnabled ? 1 : 0) );
				W.WriteLine( T + "translucencyusevertexcolor	" + (m_translucencyUseVertexColor ? 1 : 0) );
				W.WriteLine( T + "extralayer	" + m_extraLayers );

					// LAYER 0
				Layer	L = _layers[0];
				WriteLayerUVSet( W, T + "layer0_uvset", L.m_UVSet );
				WriteLayerUVSet( W, T + "layer0_mask_uvset", L.m_maskUVSet );
				WriteLayerMaskingMode( W, T + "layer0_maskmode", L.m_maskingMode );

				if ( m_extraLayers > 0 ) {
					// LAYER 1
					L = _layers[1];
					WriteLayerUVSet( W, T + "layer1_uvset", L.m_UVSet );
					WriteLayerReUseMode( W, T + "layer1_diffusereuselayer", L.m_diffuseReUse );
					if ( m_hasGloss )
						WriteLayerReUseMode( W, T + "layer1_glossreuselayer", L.m_glossReUse );
					if ( m_hasMetal )
						WriteLayerReUseMode( W, T + "layer1_metallicreuselayer", L.m_metalReUse );
					if ( L.m_maskingMode != Layer.MASKING_MODE.NONE && L.m_maskingMode != Layer.MASKING_MODE.VERTEX_COLOR ) {
						WriteLayerUVSet( W, T + "layer1_mask_uvset", L.m_maskUVSet );
						WriteLayerMaskingMode( W, T + "layer1_maskmode", L.m_maskingMode );
						WriteLayerReUseMode( W, T + "layer1_maskreuselayer", L.m_maskReUse );
					}
					if ( m_hasSpecular )
						WriteLayerReUseMode( W, T + "layer1_specularreuselayer", L.m_specularReUse );

					if ( m_extraLayers > 1 ) {
						// LAYER 2
						L = _layers[2];
						WriteLayerUVSet( W, T + "layer2_uvset", L.m_UVSet );
						WriteLayerReUseMode( W, T + "layer2_diffusereuselayer", L.m_diffuseReUse );
						if ( m_hasGloss )
							WriteLayerReUseMode( W, T + "layer2_glossreuselayer", L.m_glossReUse );
						if ( m_hasMetal )
							WriteLayerReUseMode( W, T + "layer2_metallicreuselayer", L.m_metalReUse );
						if ( L.m_maskingMode != Layer.MASKING_MODE.NONE && L.m_maskingMode != Layer.MASKING_MODE.VERTEX_COLOR ) {
							WriteLayerUVSet( W, T + "layer2_mask_uvset", L.m_maskUVSet );
							WriteLayerMaskingMode( W, T + "layer2_maskmode", L.m_maskingMode );
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
					case Layer.MASKING_MODE.NONE: maskingModeValue = 0; break;
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

			[System.Diagnostics.DebuggerDisplay( "{m_name} CstColorType={m_constantColorType}" )]
			public class	Texture {
				public enum	 CONSTANT_COLOR_TYPE {
					TEXTURE,
					DEFAULT,
					BLACK,
					BLACK_ALPHA_WHITE,
					WHITE,
					CUSTOM,
				}

				public string				m_name;
				public FileInfo				m_fileName = null;
				public Exception			m_error = null;		// Any error that occurred during texture creation
				public CONSTANT_COLOR_TYPE	m_constantColorType = CONSTANT_COLOR_TYPE.TEXTURE;
				public float4				m_customConstantColor = float4.Zero;

				// Resolve by analysis
				public TextureFileInfo		m_textureFileInfo = null;

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

				public	Texture( BinaryReader R ) {
					Read( R );
				}

				#region Serialization

				public void	Write( BinaryWriter W ) {
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

			public REUSE_MODE	m_specularReUse = REUSE_MODE.DONT_REUSE;
			public Texture		m_specular = null;	// Special specular map!

			public MASKING_MODE	m_maskingMode = MASKING_MODE.NONE;
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

			private ERROR_LEVEL	m_errorLevel = ERROR_LEVEL.NONE;
			public string		m_errors = null;
			public string		m_warnings = null;

			public ERROR_LEVEL		ErrorLevel {
				get { return m_errorLevel; }
			}

			public bool				HasErrors {
				get { return m_errors != null && m_errors != ""; }
			}

			public bool				HasWarnings {
				get { return m_warnings != null && m_warnings != ""; }
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

			public Layer() {
			}

			public Layer( BinaryReader R ) {
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

				m_errorLevel = (ERROR_LEVEL) R.ReadInt32();
				m_errors = R.ReadString();
				if ( m_errors == string.Empty )
					m_errors = null;
				m_warnings = R.ReadString();
				if ( m_warnings == string.Empty )
					m_warnings = null;
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
						m_errors += T + "• " + _textureName0 + " is not provided wheras " + _textureName1 + " is specified and re-use mode is " + _reUseMode0 + "\n";
					return;
				} else if ( _texture1 == null ) {
					if ( _reUseMode1 == REUSE_MODE.DONT_REUSE  )
						_layer1.m_errors += T + "• " + _textureName1 + " is not provided wheras " + _textureName0 + " is specified and re-use mode is " + _reUseMode0 + "\n";
					return;
				}

				// At this point we know both textures are set
				if ( _reUseMode0 != REUSE_MODE.DONT_REUSE )
					m_warnings += T + "• " + _textureName0 + " is provided (\"" + _texture0.m_name + "\") wheras re-use mode is specified. It's not optimal, you should remove the texture...\n";
				if ( _reUseMode1 != REUSE_MODE.DONT_REUSE )
					_layer1.m_warnings += T + "• " + _textureName1 + " is provided (\"" + _texture1.m_name + "\")  wheras re-use mode is specified. It's not optimal, you should remove the texture...\n";

				if ( _reUseMode0 == REUSE_MODE.DONT_REUSE && _reUseMode1 == REUSE_MODE.DONT_REUSE ) {
					// Check if both textures are the same
					if ( _texture0.m_name.ToLower() == _texture1.m_name.ToLower() ) {
						// Check if UV sets and tiling is the same
						if ( SameUVs( _layer1 ) ) 
							m_errors += T + "• " + _textureName0 + " and " + _textureName1 + " are identical and use the same UV sets, tiling and offsets! Consider re-using other layer texture instead!\n";
					}
				}
			}

			#endregion
		}

		#endregion

		public FileInfo			m_sourceFileName = null;
		public string			m_name = null;

		public int				m_version = -1;

		public Programs			m_programs = new Programs();

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
		public List< string >	m_unknownStates = new List< string >();
		public List< string >	m_unknownParms = new List< string >();
		public List< string >	m_unknownVariables = new List< string >();

		// Forbidden parms
		public List< string >	m_forbiddenParms = new List< string >();


		// Filled by analyzer
		public string			m_isCandidateForOptimization = null;
		private ERROR_LEVEL		m_errorLevel = ERROR_LEVEL.NONE;
		public string			m_errors = null;
		public string			m_warnings = null;

		#region PROPERTIES

		/// <summary>
		/// Returns the ACTUAL amount of layers used by the shader
		/// </summary>
		public int				LayersCount {
			get { return 1+m_options.m_extraLayers; }
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
				int	layersCount = Math.Min( LayersCount, m_layers.Count );
				for ( int layerIndex=0; layerIndex < layersCount; layerIndex++ ) {
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
				for ( int layerIndex=0; layerIndex < LayersCount; layerIndex++ ) {
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

		#endregion

		public Material( FileInfo _sourceFileName, string _name, string _content ) {
			m_sourceFileName = _sourceFileName;
			m_name = _name;
			m_layers.Add( new Layer() );	// We always have at least 1 layer
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
				R += "\nErrors:\n-----------\n" + m_errors;
			}
			if ( m_warnings != null && m_warnings != "" ) {
				R += "\nWarnings:\n-----------\n" + m_warnings;
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
			m_unknownStates.Clear();
			m_unknownParms.Clear();
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
					case "lightmap":			m_lightMap = new Layer.Texture( P.ReadToEOL() ); break;
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
						CheckSafeTokens( token );
						RecordUnknownVariable( token, P );
						break;
				}
			}
		}

		private void	ParseState( string _state ) {
			m_unknownStates.Add( _state );	// Add entire state!
// 
// 			try {
// 			} catch ( Exception _e ) {
// 				throw new Exception( "Failed parsing state block!", _e );
// 			}
		}

		private void	ParseParms( string _parms ) {
			m_unknownParms.Add( _parms );	// Add entire parms!
// 
// 			try {
// 			} catch ( Exception _e ) {
// 				throw new Exception( "Failed parsing parms block!", _e );
// 			}
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

						case "isalpha":						m_options.m_isAlpha = value != 0; break;
						case "alphatest":					m_options.m_isAlphaTest = value != 0; break;
						case "ismasking":					m_options.m_isMasking = value != 0; break;
						case "hasbumpmap":					m_options.m_hasNormal = value != 0; break;
						case "hasspecularmap":				m_options.m_hasSpecular = value != 0; break;
						case "hasOcclusionMap":				m_options.m_hasOcclusionMap = value != 0; break;
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
	
		void	RecordSingleLineCommentVariable( string _token, Parser P ) {
			string	record = _token + P.ReadToEOL();
			m_unknownVariables.Add( record );
		}
	
		void	RecordCommentVariable( string _token, Parser P ) {
			string	comment = _token + P.SkipComment();
			m_unknownVariables.Add( comment );
		}
	
		#region Tokens Checking

		void	CheckSafeTokens( string token ) {
			if ( m_programs.m_type != Programs.KNOWN_TYPES.DEFAULT )
				return;	// Don't care about other programs than arkDefault

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
				return;
			if ( token.StartsWith( "materialeffects" ) )
				return;
			if ( token.StartsWith( "decal" ) )
				return;
			if ( token.StartsWith( "fx/" ) )
				return;

			//////////////////////////////////////////////////////////////////////////
			// Check okay tokens
			foreach ( string recognizedString in recognizedStrings ) {
				if ( recognizedString.ToLower() == token )
					return;	// Okay!
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
					m_forbiddenParms.Add( token );
					return;
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
					return;
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
					return;
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
					return;
				}
			}

// 			int	glou = 0;
// 			glou++;
			throw new Exception( "Unrecognized token \"" + token + "\"!" );
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
			WriteListOfStrings( W, m_unknownStates );
			WriteListOfStrings( W, m_unknownParms );
			WriteListOfStrings( W, m_options.m_unknownOptions );
			WriteListOfStrings( W, m_unknownVariables );
			WriteListOfStrings( W, m_forbiddenParms );

			W.Write( m_isCandidateForOptimization != null ? m_isCandidateForOptimization : "" );
			W.Write( (int) m_errorLevel );
			W.Write( m_errors != null ? m_errors : "" );
			W.Write( m_warnings != null ? m_warnings : "" );
		}

		public void	Read( BinaryReader R ) {
			m_sourceFileName = new FileInfo( R.ReadString() );
			m_name = R.ReadString();

			m_version = R.ReadInt32();

			m_programs.Read( R );
			m_options.Read( R );

			m_layers.Clear();
			int	layersCount = R.ReadInt32();
			for ( int layerIndex=0; layerIndex < layersCount; layerIndex++ ) {
				m_layers.Add( new Layer( R ) );
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
			ReadListOfStrings( R, m_unknownStates );
			ReadListOfStrings( R, m_unknownParms );
			ReadListOfStrings( R, m_options.m_unknownOptions );
			ReadListOfStrings( R, m_unknownVariables );
			ReadListOfStrings( R, m_forbiddenParms );


			m_isCandidateForOptimization = R.ReadString();
			if ( m_isCandidateForOptimization == string.Empty )
				m_isCandidateForOptimization = null;
			m_errorLevel = (ERROR_LEVEL) R.ReadInt32();
			m_errors = R.ReadString();
			if ( m_errors == string.Empty )
				m_errors = null;
			m_warnings = R.ReadString();
			if ( m_warnings == string.Empty )
				m_warnings = null;
		}

		void	WriteListOfStrings( BinaryWriter W, List< string > _list ) {
			W.Write( _list.Count );
			foreach ( string value in _list )
				W.Write( value );
		}

		void	ReadListOfStrings( BinaryReader R, List< string > _list ) {
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
		public void		CleanUp( ) {

		}

		#endregion

		#region Material Exporter

		/// <summary>
		/// This attempts to re-export a valid material for a M2 file
		/// </summary>
		/// <param name="W"></param>
		public void	Export( StringWriter W ) {
			string	T = "\t";
			W.WriteLine( "material " + m_name + "{" );

			if ( m_version >= 0 )
				W.WriteLine( T + "version	" + m_version );

			if ( m_physicsMaterial != null )
				W.WriteLine( T + "m_PhysicsMaterial	" + m_physicsMaterial );

			// Write parms
			W.WriteLine( T + "parms {" );
			WriteParms( W, T, m_unknownParms );
			W.WriteLine( T + "}" );

			// Write state
			W.WriteLine( T + "state {" );
			WriteStates( W, T, m_unknownStates );
			W.WriteLine( T + "}" );

			// Write options
			W.WriteLine( T + "state {" );
			m_options.Export( W, T, m_layers );
			W.WriteLine( T + "}" );

			// Write regular variables
//			int	layersCount = 
// 		// Textures
// 		public List< Layer >	m_layers = new List< Layer >();
// 		public Layer.Texture	m_height = null;	// Special height map!
// 		public Layer.Texture	m_lightMap = null;	// Special light map for vista!
// 
// 		// Main variables
// 		public string			m_physicsMaterial = null;
// 		public float2			m_glossMinMax = new float2( 0.0f, 0.5f );
// 		public float2			m_metallicMinMax = new float2( 0.0f, 0.5f );

			// Write unknown variables
			foreach ( string unknownVariable in m_unknownVariables )
				W.WriteLine( T + unknownVariable );

			W.WriteLine( "}\n" );
		}

		void	WriteParms( StringWriter W, string T, List< string > _unknownParms ) {
			foreach ( string unknownParm in _unknownParms )
				W.WriteLine( T + unknownParm );
		}

		void	WriteStates( StringWriter W, string T, List< string > _unknownStates ) {
			foreach ( string unknownState in _unknownStates )
				W.WriteLine( T + unknownState );
		}

		#endregion
	}
}
