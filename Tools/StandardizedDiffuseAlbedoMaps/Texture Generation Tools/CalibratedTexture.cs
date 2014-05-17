using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using WMath;

namespace StandardizedDiffuseAlbedoMaps
{
	/// <summary>
	/// This class hosts calibrated texture & swatches infos
	/// </summary>
	public class CalibratedTexture
	{
		#region NESTED TYPES

		public class	CalibrationParms
		{
			// Database to perform proper calibration
			public CameraCalibrationDatabase	Database;

			// Image shot infos
			public float		ISOSpeed;
			public float		ShutterSpeed;
			public float		Aperture;

			// Crop infos
			public bool			CropSource = false;
			public float2		CropRectangleCenter;
			public float2		CropRectangleHalfSize;
			public float		CropRectangleRotation;
		}

		#endregion

		#region FIELDS

		private Bitmap2		m_Texture = null;

		#endregion

		#region PROPERTIES
		#endregion

		#region METHODS

		public	CalibratedTexture()
		{

		}

		public void		Build( Bitmap2 _Source, CalibrationParms _Parms )
		{
			if ( _Source == null )
				throw new Exception( "Invalid source bitmap to build texture from!" );
			if ( _Parms == null )
				throw new Exception( "Invalid calibration parameters!" );
			if ( _Parms.Database == null )
				throw new Exception( "Invalid calibration database found in parameters!" );

			//////////////////////////////////////////////////////////////////////////
			// Setup the database to find the most appropriate calibration data for our image infos
			_Parms.Database.PrepareCalibrationFor( _Parms.ISOSpeed, _Parms.ShutterSpeed, _Parms.Aperture );

			//////////////////////////////////////////////////////////////////////////
			// Build target texture
			if ( _Parms.CropSource )
			{
// TODO!
//				m_Texture = CropSource( _Source, _Parms.Database );
			}
			else
			{

			}
		}

		#endregion
	}
}
