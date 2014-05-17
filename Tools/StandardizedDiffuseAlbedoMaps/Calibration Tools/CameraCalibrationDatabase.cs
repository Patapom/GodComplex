using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using WMath;

namespace StandardizedDiffuseAlbedoMaps
{
	/// <summary>
	/// This class hosts the camera calibration database
	/// </summary>
	public class CameraCalibrationDatabase
	{
		#region FIELDS

		private System.IO.DirectoryInfo		m_DatabasePath = null;

		#endregion

		#region FIELDS

		public System.IO.DirectoryInfo		DatabasePath
		{
			get { return m_DatabasePath; }
			set
			{
				if ( m_DatabasePath != null )
				{	// Clean up existing database

				}

				m_DatabasePath = value;

				if ( m_DatabasePath != null )
				{	// Setup new database
					if ( !m_DatabasePath.Exists )
						throw new Exception( "Provided database path doesn't exist!" );


				}
			}
		}

		#endregion

		#region METHODS

		/// <summary>
		/// Prepares the 8 closest calibration tables to process the pixels in an image shot with the specified parameters
		/// </summary>
		/// <param name="_ISOSpeed"></param>
		/// <param name="_ShutterSpeed"></param>
		/// <param name="_Aperture"></param>
		public void	PrepareCalibrationFor( float _ISOSpeed, float _ShutterSpeed, float _Aperture )
		{

		}

		/// <summary>
		/// Calibrates a raw luminance value
		/// </summary>
		/// <param name="_Luminance">The uncalibrated luminance value</param>
		/// <returns>The calibrated luminance value</returns>
		/// <remarks>Typically, you start from a RAW XYZ value that you convert to xyY, pass the Y to this method
		/// and replace it into your orignal xyY, convert back to XYZ and voilà!</remarks>
		public float	Calibrate( float _Luminance )
		{
			return _Luminance;
		}

		#endregion
	}
}
