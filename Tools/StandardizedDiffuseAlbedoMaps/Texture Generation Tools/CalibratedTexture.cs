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
			public float2		CropRectangleCenter;	// In UV space
			public float2		CropRectangleHalfSize;	// In UV space
			public float		CropRectangleRotation;

			// Custom swatches
			public int			CustomSwatchesCount = 0;
			public float2[]	CustomSwatches = new float2[16];	// In UV space
		}

		#endregion

		#region FIELDS

		private Bitmap2		m_Texture = null;
		private Bitmap2[]	m_Swatches = null;

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
			{	// Simple texture copy, with luminance calibration
				m_Texture = new Bitmap2( _Source.Width, _Source.Height );
				float4	XYZ;
				float3	xyY;
				for ( int Y=0; Y < m_Texture.Height; Y++ )
					for ( int X=0; X < m_Texture.Width; X++ )
					{
						XYZ = _Source.ContentXYZ[X,Y];
						xyY = Bitmap2.ColorProfile.XYZ2xyY( (float3) XYZ );
						xyY.z = _Parms.Database.Calibrate( xyY.z );
						XYZ = new float4( Bitmap2.ColorProfile.XYZ2xyY( xyY ), XYZ.w );
						m_Texture.ContentXYZ[X,Y] = XYZ;
					}
			}

			//////////////////////////////////////////////////////////////////////////
			// Build swatches


			//////////////////////////////////////////////////////////////////////////
			// Feed some purely informational shot infos
			m_Texture.HasValidShotInfo = true;
			m_Texture.ISOSpeed = _Parms.ISOSpeed;
			m_Texture.ShutterSpeed = _Parms.ShutterSpeed;
			m_Texture.Aperture = _Parms.Aperture;
		}

		#endregion
	}
}
