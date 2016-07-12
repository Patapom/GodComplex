using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using RendererManaged;

namespace MaterialsOptimizer
{
	[System.Diagnostics.DebuggerDisplay( "{m_fileName} - {m_usage} - {m_fileType}" )]
	public class TextureFileInfo {

		public enum		FILE_TYPE {
			UNKNOWN,
			TGA,
			PNG,
			JPG,
			TIFF,
			DDS,
			BIMAGE,
		}

		public enum		USAGE {
			UNKNOWN,
			DIFFUSE,		// _d
			NORMAL,			// _n
			GLOSS,			// _r, _g, _g1, etc.
			METAL,			// _mt
			MASK,			// _m, _o
			MASK_BAD_SUFFIX,// _mask	<= They're using it a lot but it makes a BC7-encoded image!!
			EMISSIVE,		// _e
			AO,				// _ao
			HEIGHT,			// _h
			TRANSLUCENCY,	// _tr
			DIR_TRANSLUCENCY,	// _dt
			SPECULAR,		// _s
			SSBUMP,			// _ssbump
			COLOR_CUBE,		// _cc
			DISPLACEMENT,	// _disp
			DISTANCE_FIELD,	// _df
			WRINKLE_MASK,	// _wm
			WRINKLE_NORMAL,	// _wn
			IGGY,			// _is
			DIFFUSE_GLOSS,	// _dg
		}

		public FileInfo		m_fileName = null;
		public FILE_TYPE	m_fileType = FILE_TYPE.UNKNOWN;
		public USAGE		m_usage = USAGE.UNKNOWN;
		public bool			m_couldBeRead = false;
		public int			m_width = 0;
		public int			m_height = 0;
		public Exception	m_error = null;

		// Generated in conjunction with materials database
		public int			m_refCount = 0;

		// Used when trying to compact textures together
		// Usually, the associated texture to a diffuse texture is a gloss texture...
		public TextureFileInfo	m_associatedTexture = null;

		/// <summary>
		/// Lower-case, slashes file name that can be used in a hashtable to map engine-convention texture names to actual files
		/// </summary>
		public string	NormalizedFileName {
			get { return NormalizeFileName( m_fileName.FullName ); }
		}

		public int	ColorChannelsCount {
			get {
				switch ( m_usage ) {
					case USAGE.DIFFUSE:			return 4;
					case USAGE.NORMAL:			return 2;
					case USAGE.GLOSS:			return 1;
					case USAGE.METAL:			return 1;
					case USAGE.MASK:			return 1;
					case USAGE.MASK_BAD_SUFFIX:	return 1;
					case USAGE.EMISSIVE:		return 4;
					case USAGE.AO:				return 1;
					case USAGE.HEIGHT:			return 1;
					case USAGE.TRANSLUCENCY:	return 1;
					case USAGE.DIR_TRANSLUCENCY:return 3;
					case USAGE.SPECULAR:		return 4;
					case USAGE.SSBUMP:			return 3;
					case USAGE.COLOR_CUBE:		return 4;
					case USAGE.DISPLACEMENT:	return 1;
					case USAGE.DISTANCE_FIELD:	return 1;
					case USAGE.WRINKLE_MASK:	return 4;
					case USAGE.WRINKLE_NORMAL:	return 2;
					case USAGE.IGGY:			return 4;
					case USAGE.DIFFUSE_GLOSS:	return 4;
				}

				return 0;
			}
		}

		public TextureFileInfo( FileInfo _fileName ) {
			m_fileName = _fileName;
			m_usage = FindUsage( _fileName );

			if ( _fileName.Extension.ToLower().StartsWith( ".bimage" ) ) {
				// Can't read!
				m_fileType = FILE_TYPE.BIMAGE;
				return;
			}

			try {
				using ( ImageUtility.Bitmap B = new ImageUtility.Bitmap( _fileName ) ) {

					m_couldBeRead = true;
					m_width = B.Width;
					m_height = B.Height;
					switch ( B.Type ) {
						case ImageUtility.Bitmap.FILE_TYPE.PNG: m_fileType = FILE_TYPE.PNG; break;
						case ImageUtility.Bitmap.FILE_TYPE.TGA: m_fileType = FILE_TYPE.TGA; break;
//						case ImageUtility.Bitmap.FILE_TYPE.DDS: m_fileType = FILE_TYPE.DDS; break;	// DDS not supported?
						case ImageUtility.Bitmap.FILE_TYPE.JPEG: m_fileType = FILE_TYPE.JPG; break;
						case ImageUtility.Bitmap.FILE_TYPE.TIFF: m_fileType = FILE_TYPE.TIFF; break;
					}
				}
			} catch ( Exception _e ) {
				m_error = _e;
				throw _e;
			}
		}

		public TextureFileInfo( BinaryReader R ) {
			Read( R );
		}

		public void		Read( BinaryReader R ) {
			m_fileName = new FileInfo( R.ReadString() );
			m_fileType = (FILE_TYPE) R.ReadInt32();
			m_width = R.ReadInt32();
			m_height = R.ReadInt32();
			m_couldBeRead = R.ReadBoolean();
			string	errorText = R.ReadString();
			m_error = errorText != string.Empty ? new Exception( errorText ) : null;

			m_usage = FindUsage( m_fileName );	// Attempt to find usage again
		}

		public void		Write( BinaryWriter W ) {
			W.Write( m_fileName.FullName );
			W.Write( (int) m_fileType );
			W.Write( m_width );
			W.Write( m_height );
			W.Write( m_couldBeRead );
			W.Write( m_error != null ? m_error.Message : "" );
		}

		public static USAGE	FindUsage( FileInfo _fileName ) {
			string	fileName = Path.GetFileNameWithoutExtension( _fileName.FullName );
			int		indexOfUnderscore = fileName.LastIndexOf( '_' );
			if ( indexOfUnderscore == -1 )
				return USAGE.UNKNOWN;

			string	usageTag = fileName.Substring( indexOfUnderscore ).ToLower();
			if ( usageTag.EndsWith( "k1" ) || usageTag.EndsWith( "k2" ) )
				usageTag = usageTag.Substring( 0 , usageTag.Length - 2 );
			else if ( usageTag.EndsWith( "kc1" ) || usageTag.EndsWith( "kc2" ) )
				usageTag = usageTag.Substring( 0 , usageTag.Length - 3 );

			switch ( usageTag ) {
				case "_d":
				case "_d1":
				case "_d2":
				case "_d3":
				case "_d4":
				case "_d5":
				case "_d6":
				case "_d7":
				case "_d8":
				case "_d9":
					return USAGE.DIFFUSE;

				case "_dg": return USAGE.DIFFUSE_GLOSS;
				case "_n": return USAGE.NORMAL;

				case "_g":
				case "_r":	// Old but can be encountered...
				case "_g1":
				case "_g2":
				case "_g3":
				case "_g4":
				case "_g5":
				case "_g6":
				case "_g7":
				case "_g8":
				case "_g9":
					return USAGE.GLOSS;
				case "_mt":
				case "_mt1":
				case "_mt2":
				case "_mt3":
				case "_mt4":
				case "_mt5":
				case "_mt6":
				case "_mt7":
				case "_mt8":
				case "_mt9":
					return USAGE.METAL;
				case "_ao": return USAGE.AO;
				case "_h": return USAGE.HEIGHT;
				case "_ssbump": return USAGE.SSBUMP;
				case "_m":
				case "_m1":
				case "_m2":
				case "_m3":
				case "_m4":
				case "_m5":
				case "_m6":
				case "_m7":
				case "_m8":
				case "_m9":
					return USAGE.MASK;
				case "_mask":
					return USAGE.MASK_BAD_SUFFIX;
				case "_o": return USAGE.MASK;
				case "_cc": return USAGE.COLOR_CUBE;
				case "_disp": return USAGE.DISPLACEMENT;
				case "_is": return USAGE.IGGY;
				case "_df": return USAGE.DISTANCE_FIELD;
				case "_dt": return USAGE.DIR_TRANSLUCENCY;
				case "_tr": return USAGE.TRANSLUCENCY;
				case "_e": return USAGE.EMISSIVE;
				case "_wm": return USAGE.WRINKLE_MASK;
				case "_wn": return USAGE.WRINKLE_NORMAL;
				case "_s": return USAGE.SPECULAR;
			}

			return USAGE.UNKNOWN;
		}

		public static string NormalizeFileName( string _fileName ) {
			return _fileName.ToLower().Replace( '\\', '/' );
		}

		public static string GetOptimizedDiffuseGlossNameFromDiffuseName( string _diffuseFileName ) {
			_diffuseFileName = _diffuseFileName.Trim();
			int		indexOf_d = _diffuseFileName.LastIndexOf( "_d" );
			string	optimizedDiffuseTextureName = _diffuseFileName.Substring( 0, indexOf_d ) + "_dg" + _diffuseFileName.Substring( indexOf_d+2 );
			return optimizedDiffuseTextureName;
		}
	}
}
