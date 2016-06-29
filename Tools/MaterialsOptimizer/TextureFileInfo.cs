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
			GLOSS,			// _g, _g1
			METAL,			// _mt
			MASK,			// _m, _o
			EMISSIVE,		// _e
			AO,				// _ao
			HEIGHT,			// _h
			TRANSLUCENCY,	// _dt
			SPECULAR,		// _s
			SSBUMP,			// _ssbump
			COLOR_CUBE,		// _cc
			DISPLACEMENT,	// _disp
			DISTANCE_FIELD,	// _df
			WRINKLE_MASK,	// _wm
			WRINKLE_NORMAL,	// _wn
			IGGY,			// _is
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
			m_usage = (USAGE) R.ReadInt32();
			m_width = R.ReadInt32();
			m_height = R.ReadInt32();
			m_couldBeRead = R.ReadBoolean();
			string	errorText = R.ReadString();
			m_error = errorText != string.Empty ? new Exception( errorText ) : null;
		}

		public void		Write( BinaryWriter W ) {
			W.Write( m_fileName.FullName );
			W.Write( (int) m_fileType );
			W.Write( (int) m_usage );
			W.Write( m_width );
			W.Write( m_height );
			W.Write( m_couldBeRead );
			W.Write( m_error != null ? m_error.Message : "" );
		}

		public static USAGE	FindUsage( FileInfo _fileName ) {
			string	fileName = Path.GetFileNameWithoutExtension( _fileName.FullName );
			int		indexOfUnderscore = fileName.LastIndexOf( '_' );
			if ( indexOfUnderscore != -1 ) {
				string	usageTag = fileName.Substring( indexOfUnderscore ).ToLower();
				switch ( usageTag ) {
					case "_d": return USAGE.DIFFUSE;
					case "_n": return USAGE.NORMAL;
					case "_g": return USAGE.GLOSS;
					case "_mt": return USAGE.METAL;
					case "_ao": return USAGE.AO;
					case "_h": return USAGE.HEIGHT;
					case "_ssbump": return USAGE.SSBUMP;
					case "_m": return USAGE.MASK;
					case "_o": return USAGE.MASK;
					case "_cc": return USAGE.COLOR_CUBE;
					case "_disp": return USAGE.DISPLACEMENT;
					case "_is": return USAGE.IGGY;
					case "_df": return USAGE.DISTANCE_FIELD;
					case "_dt": return USAGE.TRANSLUCENCY;
					case "_e": return USAGE.EMISSIVE;
					case "_wm": return USAGE.WRINKLE_MASK;
					case "_wn": return USAGE.WRINKLE_NORMAL;
					case "_s": return USAGE.SPECULAR;
				}
			}

			return USAGE.UNKNOWN;
		}
	}
}
