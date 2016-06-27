using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using RendererManaged;

namespace MaterialsOptimizer
{
	[System.Diagnostics.DebuggerDisplay( "{m_name} - {m_options} - {m_layers.Count} Layers" )]
	public class TextureFile {

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
			AO,				// _ao
			HEIGHT,			// _h
		}

		public FileInfo		m_fileName = null;
		public FILE_TYPE	m_fileType = FILE_TYPE.UNKNOWN;
		public USAGE		m_usage = USAGE.UNKNOWN;
		public bool			m_couldBeRead = false;
		public int			m_width = 0;
		public int			m_height = 0;

		public TextureFile( FileInfo _fileName ) {
			m_fileName = _fileName;
			m_usage = FindUsage( _fileName );

			if ( _fileName.Extension.ToLower().StartsWith( ".bimage" ) ) {
				// Can't read!
				m_fileType = FILE_TYPE.BIMAGE;
				return;
			}

			try {
				ImageUtility.Bitmap	B = new ImageUtility.Bitmap( _fileName );

				m_couldBeRead = true;
				m_width = B.Width;
				m_height = B.Height;
				switch ( B.Type ) {
					case ImageUtility.Bitmap.FILE_TYPE.PNG: m_fileType = FILE_TYPE.PNG; break;
					case ImageUtility.Bitmap.FILE_TYPE.TGA: m_fileType = FILE_TYPE.TGA; break;
//					case ImageUtility.Bitmap.FILE_TYPE.DDS: m_fileType = FILE_TYPE.DDS; break;	// DDS not supported?
					case ImageUtility.Bitmap.FILE_TYPE.JPEG: m_fileType = FILE_TYPE.JPG; break;
					case ImageUtility.Bitmap.FILE_TYPE.TIFF: m_fileType = FILE_TYPE.TIFF; break;
				}

			} catch ( Exception _e ) {
				throw _e;
			}
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
				}
			}

			return USAGE.UNKNOWN;
		}
	}
}
