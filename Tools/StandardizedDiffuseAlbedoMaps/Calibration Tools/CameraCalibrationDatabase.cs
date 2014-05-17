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

		public void	PrepareCalibrationFor( float _ISOSpeed, float _ShutterSpeed, float _Aperture )
		{

		}

		#endregion
	}
}
