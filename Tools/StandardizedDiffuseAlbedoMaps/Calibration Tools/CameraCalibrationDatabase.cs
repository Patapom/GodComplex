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

		private float	m_PreparedForISOSpeed = 0.0f;
		private float	m_PreparedForShutterSpeed = 0.0f;
		private float	m_PreparedForAperture = 0.0f;

		#endregion

		#region PROPERTIES

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

		public float	PreparedForISOSpeed		{ get { return m_PreparedForISOSpeed; } }
		public float	PreparedForShutterSpeed	{ get { return m_PreparedForShutterSpeed; } }
		public float	PreparedForAperture		{ get { return m_PreparedForAperture; } }

		#endregion

		#region METHODS

		/// <summary>
		/// Prepares the 8 closest calibration tables to process the pixels in an image shot with the specified shot infos
		/// </summary>
		/// <param name="_ISOSpeed"></param>
		/// <param name="_ShutterSpeed"></param>
		/// <param name="_Aperture"></param>
		public void	PrepareCalibrationFor( float _ISOSpeed, float _ShutterSpeed, float _Aperture )
		{
			m_PreparedForISOSpeed = _ISOSpeed;
			m_PreparedForShutterSpeed = _ShutterSpeed;
			m_PreparedForAperture = _Aperture;
		}

		/// <summary>
		/// Tells if the database is prepared and can be used for processing colors of an image with the specified shot infos
		/// </summary>
		/// <param name="_ISOSpeed"></param>
		/// <param name="_ShutterSpeed"></param>
		/// <param name="_Aperture"></param>
		/// <returns></returns>
		public bool	IsPreparedFor( float _ISOSpeed, float _ShutterSpeed, float _Aperture )
		{
			return Math.Abs( _ISOSpeed - m_PreparedForISOSpeed ) < 1e-6f
				&& Math.Abs( _ShutterSpeed - m_PreparedForShutterSpeed ) < 1e-6f
				&& Math.Abs( _Aperture - m_PreparedForAperture ) < 1e-6f;
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
