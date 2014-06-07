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
	public class CalibratedTexture : IDisposable
	{
		#region NESTED TYPES

		public class	CaptureParms
		{
			// Image shot infos
			public string		SourceImageName;
			public float		ISOSpeed;
			public float		ShutterSpeed;
			public float		Aperture;

			// Crop infos
			public bool			CropSource = false;
			public float2		CropRectangleCenter;	// In UV space (note that UVs are not in [0,1] as usual because of aspect ratio, e.g. X = UV.x * ImageHeight, instead of ImageWidth)
			public float2		CropRectangleHalfSize;	// In UV space
			public float		CropRectangleRotation;
		}

		/// <summary>
		/// Possible target formats for textures
		/// </summary>
		public enum		TARGET_FORMAT
		{
			PNG8,
			PNG16,	// Default should be 16-bits PNG
			TIFF,
		}

		public class	Swatch
		{
			public float3		xyY;						// The color used to build the swatch
			public float4		Location = new float4();	// The location (in UV space) where the swatch color was taken (XY=Top Left Corner, ZW=Bottom Right Corner)
			public Bitmap2		Texture;					// The bitmap generated from the swatch color

			public virtual void	Save( CalibratedTexture _Owner, XmlElement _SwatchElement )
			{
				float4	XYZ = new float4( Bitmap2.ColorProfile.xyY2XYZ( xyY ), 1.0f );
				float3	RGB = (float3) Texture.Profile.XYZ2RGB( XYZ );
				_Owner.SetAttribute( _SwatchElement, "xyY", xyY.ToString() ).SetAttribute( "RGB", RGB.ToString() );
			}
		}
		public class	CustomSwatch : Swatch
		{
			public override void	Save( CalibratedTexture _Owner, XmlElement _SwatchElement )
			{
				base.Save( _Owner, _SwatchElement );

				_Owner.SetAttribute( _SwatchElement, "SampleTopLeft", (new float2( Location.x, Location.y )).ToString() );
				_Owner.SetAttribute( _SwatchElement, "SampleBottomRight", (new float2( Location.z, Location.w )).ToString() );
			}
		}

		#endregion

		#region FIELDS

		// The parameters that were used to capture the texture
		private CaptureParms		m_CaptureParameters = null;

		// Main texture
		private Bitmap2				m_Texture = null;
		private float3				m_WhiteReflectanceReference = new float3( 0, 0, -1 );
		private float				m_WhiteReflectanceCorrectionFactor = 1.0f;
		private bool				m_SpatialCorrectionEnabled = false;

		// Default swatches
		private Swatch				m_SwatchMin = new Swatch();
		private Swatch				m_SwatchMax = new Swatch();
		private Swatch				m_SwatchAvg = new Swatch();

		// Custom swatches
		private int					m_SwatchWidth = 48;
		private int					m_SwatchHeight = 32;
		private CustomSwatch[]		m_CustomSwatches = new CustomSwatch[0];

		#endregion

		#region PROPERTIES

		public Bitmap2			Texture			{ get { return m_Texture; } }
		public Swatch			SwatchMin		{ get { return m_SwatchMin; } }
		public Swatch			SwatchMax		{ get { return m_SwatchMax; } }
		public Swatch			SwatchAvg		{ get { return m_SwatchAvg; } }
		public CustomSwatch[]	CustomSwatches	{ get { return m_CustomSwatches; } }

		public int				SwatchWidth		{ get { return m_SwatchWidth; } set { m_SwatchWidth = value; } }
		public int				SwatchHeight	{ get { return m_SwatchHeight; } set { m_SwatchHeight = value; } }

		public float3			WhiteReflectanceReference			{ get { return m_WhiteReflectanceReference; } }
		public float			WhiteReflectanceCorrectionFactor	{ get { return m_WhiteReflectanceCorrectionFactor; } }
		public bool				SpatialCorrectionEnabled			{ get { return m_SpatialCorrectionEnabled; } }

		#endregion

		#region METHODS

		public	CalibratedTexture()
		{
		}

		/// <summary>
		/// Captures the calibrated texture
		/// </summary>
		/// <param name="_Source">The source image to capture</param>
		/// <param name="_Database">Database to perform proper calibration</param>
		/// <param name="_Parms">Parameters for the capture</param>
		public void		Capture( Bitmap2 _Source, CameraCalibrationDatabase _Database, CaptureParms _Parms )
		{
			if ( _Source == null )
				throw new Exception( "Invalid source bitmap to build texture from!" );
			if ( _Database == null )
				throw new Exception( "Invalid calibration database found in parameters!" );
			if ( _Parms == null )
				throw new Exception( "Invalid calibration parameters!" );
			if ( m_SwatchWidth <= 0 || m_SwatchHeight <= 0 )
				throw new Exception( "Invalid swatch size! Must be > 0!" );

			// Save parameters as they're associated to this texture
			m_CaptureParameters = _Parms;
			m_WhiteReflectanceReference = _Database.WhiteReflectanceReference;
			m_WhiteReflectanceCorrectionFactor = _Database.WhiteReflectanceCorrectionFactor;
			m_SpatialCorrectionEnabled = _Database.WhiteReferenceImage != null;

			//////////////////////////////////////////////////////////////////////////
			// Setup the database to find the most appropriate calibration data for our image infos
			_Database.PrepareCalibrationFor( _Parms.ISOSpeed, _Parms.ShutterSpeed, _Parms.Aperture );


			//////////////////////////////////////////////////////////////////////////
			// Build target texture
			m_SwatchMin.xyY.z = float.MaxValue;
			m_SwatchMax.xyY.z = -float.MaxValue;
			m_SwatchAvg.xyY = new float3( 0, 0, 0 );
//DEBUG
// float	MinLuminance_Raw = float.MaxValue;
// float	MaxLuminance_Raw = -float.MaxValue;

			if ( _Parms.CropSource )
			{
				float	fImageWidth = 2.0f * _Parms.CropRectangleHalfSize.x * _Source.Height;
				float	fImageHeight = 2.0f * _Parms.CropRectangleHalfSize.y * _Source.Height;
				int		W = (int) Math.Floor( fImageWidth );
				int		H = (int) Math.Floor( fImageHeight );

				float2	AxisX = new float2( (float) Math.Cos( _Parms.CropRectangleRotation ), -(float) Math.Sin( _Parms.CropRectangleRotation ) );
				float2	AxisY = new float2( (float) Math.Sin( _Parms.CropRectangleRotation ), (float) Math.Cos( _Parms.CropRectangleRotation ) );

				float2	TopLeftCorner = new float2( 0.5f * (_Source.Width - _Source.Height) + _Parms.CropRectangleCenter.x * _Source.Height, _Source.Height * _Parms.CropRectangleCenter.y )
												  + _Source.Height * (-_Parms.CropRectangleHalfSize.x * AxisX - _Parms.CropRectangleHalfSize.y * AxisY);

				m_Texture = new Bitmap2( W, H, new Bitmap2.ColorProfile( Bitmap2.ColorProfile.STANDARD_PROFILE.sRGB ) );
				float4	XYZ;
				float3	xyY;

				float2	CurrentScanlinePixel = TopLeftCorner + 0.5f * (fImageWidth - W) * AxisX + 0.5f * (fImageHeight - H) * AxisY;
				if ( Math.Abs( _Parms.CropRectangleRotation ) < 1e-6f )
				{	// Use integer pixels to avoid attenuated values due to bilinear filtering
					CurrentScanlinePixel.x = (float) Math.Floor( CurrentScanlinePixel.x );
					CurrentScanlinePixel.y = (float) Math.Floor( CurrentScanlinePixel.y );
				}
				for ( int Y=0; Y < H; Y++ )
				{
					float2	CurrentPixel = CurrentScanlinePixel;
					for ( int X=0; X < W; X++ )
					{
						float	U = CurrentPixel.x / _Source.Width;
						float	V = CurrentPixel.y / _Source.Height;

						XYZ = _Source.BilinearSample( CurrentPixel.x, CurrentPixel.y );

//DEBUG
// float	L = XYZ.y * _Database.GetSpatialLuminanceCorrectionFactor( U, V );
// if ( L < MinLuminance_Raw )
// 	MinLuminance_Raw = L;
// if ( L > MaxLuminance_Raw )
// 	MaxLuminance_Raw = L;
//DEBUG

						xyY = Bitmap2.ColorProfile.XYZ2xyY( (float3) XYZ );
						xyY = _Database.CalibrateWithSpatialCorrection( U, V, xyY );	// Apply luminance calibration
						XYZ = new float4( Bitmap2.ColorProfile.xyY2XYZ( xyY ), XYZ.w );
						m_Texture.ContentXYZ[X,Y] = XYZ;

						// Update min/max/avg values
						if ( xyY.z < m_SwatchMin.xyY.z )
						{
							m_SwatchMin.xyY = xyY;
							m_SwatchMin.Location.x = (float) X / W;
							m_SwatchMin.Location.y = (float) Y / H;
						}
						if ( xyY.z > m_SwatchMax.xyY.z )
						{
							m_SwatchMax.xyY = xyY;
							m_SwatchMax.Location.x = (float) X / W;
							m_SwatchMax.Location.y = (float) Y / H;
						}
						m_SwatchAvg.xyY += xyY;

						CurrentPixel += AxisX;
					}
					CurrentScanlinePixel += AxisY;
				}
			}
			else
			{	// Simple texture copy, with luminance calibration
				m_Texture = new Bitmap2( _Source.Width, _Source.Height, new Bitmap2.ColorProfile( Bitmap2.ColorProfile.STANDARD_PROFILE.sRGB ) );
				float4	XYZ;
				float3	xyY;

				int	W = m_Texture.Width;
				int	H = m_Texture.Height;

				int	X0 = 0;
				int	X1 = W;
				int	Y0 = 0;
				int	Y1 = H;

//DEBUG
// X0 = 1088; Y0 = 764;
// X1 = X0 + 1100; Y1 = Y0 + 632;

				for ( int Y=Y0; Y < Y1; Y++ )
				{
					float	V = (float) Y / H;
					for ( int X=X0; X < X1; X++ )
					{
						float	U = (float) X / W;

						XYZ = _Source.ContentXYZ[X,Y];

//DEBUG
// float	L = XYZ.y * _Database.GetSpatialLuminanceCorrectionFactor( U, V );
// if ( L < MinLuminance_Raw )
// 	MinLuminance_Raw = L;
// if ( L > MaxLuminance_Raw )
// 	MaxLuminance_Raw = L;
//DEBUG

						xyY = Bitmap2.ColorProfile.XYZ2xyY( (float3) XYZ );
						xyY = _Database.CalibrateWithSpatialCorrection( U, V, xyY );	// Apply luminance calibration
						XYZ = new float4( Bitmap2.ColorProfile.xyY2XYZ( xyY ), XYZ.w );
						m_Texture.ContentXYZ[X,Y] = XYZ;

						// Update min/max/avg values
						if ( xyY.z < m_SwatchMin.xyY.z )
						{
							m_SwatchMin.xyY = xyY;
							m_SwatchMin.Location.x = (float) X / W;
							m_SwatchMin.Location.y = (float) Y / H;
						}
						if ( xyY.z > m_SwatchMax.xyY.z )
						{
							m_SwatchMax.xyY = xyY;
							m_SwatchMax.Location.x = (float) X / W;
							m_SwatchMax.Location.y = (float) Y / H;
						}
						m_SwatchAvg.xyY += xyY;
					}
				}
			}

			// Normalize average swatch color
			float	Normalizer = 1.0f / (m_Texture.Width*m_Texture.Height);
			m_SwatchAvg.xyY.x *= Normalizer;
			m_SwatchAvg.xyY.y *= Normalizer;
			m_SwatchAvg.xyY.z *= Normalizer;

			m_SwatchMin.Texture = BuildSwatch( m_SwatchWidth, m_SwatchHeight, m_SwatchMin.xyY );
			m_SwatchMax.Texture = BuildSwatch( m_SwatchWidth, m_SwatchHeight, m_SwatchMax.xyY );
			m_SwatchAvg.Texture = BuildSwatch( m_SwatchWidth, m_SwatchHeight, m_SwatchAvg.xyY );

			// Rebuild custom swatches
			foreach ( CustomSwatch CS in m_CustomSwatches )
				CS.Texture = BuildSwatch( m_SwatchWidth, m_SwatchHeight, CS.xyY );

			//////////////////////////////////////////////////////////////////////////
			// Feed some purely informational shot infos to the main texture, probably won't be saved anyway...
			m_Texture.HasValidShotInfo = true;
			m_Texture.ISOSpeed = _Parms.ISOSpeed;
			m_Texture.ShutterSpeed = _Parms.ShutterSpeed;
			m_Texture.Aperture = _Parms.Aperture;
		}

		/// <summary>
		/// Builds the custom swatches
		/// </summary>
		/// <param name="_CustomSwatchSamplingLocations">In UV space. XY=Top Left corner, ZW=Bottom Right corner</param>
		public void		BuildCustomSwatches( float4[] _CustomSwatchSamplingLocations )
		{
			if ( m_Texture == null )
				throw new Exception( "Cannot build custom swatched because no texture was captured!" );
			if ( _CustomSwatchSamplingLocations == null )
				throw new Exception( "Invalid swatch parameters!" );
			if ( m_SwatchWidth <= 0 || m_SwatchHeight <= 0 )
				throw new Exception( "Invalid swatch size! Must be > 0!" );

			m_CustomSwatches = new CustomSwatch[_CustomSwatchSamplingLocations.Length];
			for ( int CustomSwatchIndex=0; CustomSwatchIndex < m_CustomSwatches.Length; CustomSwatchIndex++ )
			{
				CustomSwatch	S = new CustomSwatch();
				m_CustomSwatches[CustomSwatchIndex] = S;

				S.Location = _CustomSwatchSamplingLocations[CustomSwatchIndex];
				S.xyY = ComputeAverageSwatchColor( new float2( S.Location.x, S.Location.y ), new float2( S.Location.z, S.Location.w ) );
				S.Texture = BuildSwatch( m_SwatchWidth, m_SwatchHeight, S.xyY );
			}
		}

		/// <summary>
		/// Saves the texture pack (texture + swatches + xml manifest)
		/// </summary>
		/// <param name="_FileName"></param>
		public void		SavePack( System.IO.FileInfo _FileName, TARGET_FORMAT _TargetFormat )
		{
			if ( m_Texture == null )
				throw new Exception( "No calibrated texture has been built! Can't save." );

			System.IO.DirectoryInfo	Dir = _FileName.Directory;
			string	RawFileName = System.IO.Path.GetFileNameWithoutExtension( _FileName.FullName );
			string	Extension = System.IO.Path.GetExtension( _FileName.FullName );

			System.IO.FileInfo		FileName_Manifest = new System.IO.FileInfo( System.IO.Path.Combine( Dir.FullName, RawFileName + ".xml" ) );
			System.IO.FileInfo		FileName_SwatchMin = new System.IO.FileInfo( System.IO.Path.Combine( Dir.FullName, RawFileName + "_Min" + Extension ) );
			System.IO.FileInfo		FileName_SwatchMax = new System.IO.FileInfo( System.IO.Path.Combine( Dir.FullName, RawFileName + "_Max" + Extension ) );
			System.IO.FileInfo		FileName_SwatchAvg = new System.IO.FileInfo( System.IO.Path.Combine( Dir.FullName, RawFileName + "_Avg" + Extension ) );
			System.IO.FileInfo[]	FileName_CustomSwatches = new System.IO.FileInfo[m_CustomSwatches.Length];
			for ( int CustomSwatchIndex=0; CustomSwatchIndex < m_CustomSwatches.Length; CustomSwatchIndex++ )
				FileName_CustomSwatches[CustomSwatchIndex] = new System.IO.FileInfo( System.IO.Path.Combine( Dir.FullName, RawFileName + "_Custom" + CustomSwatchIndex.ToString() + Extension ) );


			//////////////////////////////////////////////////////////////////////////
			// Build image type and format parameters as well as target color profile
			Bitmap2.FILE_TYPE		FileType = Bitmap2.FILE_TYPE.UNKNOWN;
			Bitmap2.FORMAT_FLAGS	Format = Bitmap2.FORMAT_FLAGS.NONE;
			switch ( _TargetFormat )
			{
				case TARGET_FORMAT.PNG8:
					FileType = Bitmap2.FILE_TYPE.PNG;
					Format = Bitmap2.FORMAT_FLAGS.SAVE_8BITS_UNORM;
					break;

				case TARGET_FORMAT.PNG16:
					FileType = Bitmap2.FILE_TYPE.PNG;
					Format = Bitmap2.FORMAT_FLAGS.SAVE_16BITS_UNORM;
					break;

				case TARGET_FORMAT.TIFF:
					FileType = Bitmap2.FILE_TYPE.TIFF;
					Format = Bitmap2.FORMAT_FLAGS.SAVE_16BITS_UNORM;
					break;
			}
			if ( FileType == Bitmap2.FILE_TYPE.UNKNOWN )
				throw new Exception( "Unknown target file format!" );

			//////////////////////////////////////////////////////////////////////////
			// Save textures

			// Save main texture
			SaveImage( m_Texture, _FileName, FileType, Format );

			// Save default swatches
			SaveImage( m_SwatchMin.Texture, FileName_SwatchMin, FileType, Format );
			SaveImage( m_SwatchMax.Texture, FileName_SwatchMax, FileType, Format );
			SaveImage( m_SwatchAvg.Texture, FileName_SwatchAvg, FileType, Format );

			// Save custom swatches
			for ( int CustomSwatchIndex=0; CustomSwatchIndex < m_CustomSwatches.Length; CustomSwatchIndex++ )
				SaveImage( m_CustomSwatches[CustomSwatchIndex].Texture, FileName_CustomSwatches[CustomSwatchIndex], FileType, Format );


			//////////////////////////////////////////////////////////////////////////
			// Prepare the XML manifest
			XmlDocument	Doc = new XmlDocument();

			XmlComment	HeaderComment = Doc.CreateComment( 
				"***Do not modify!***\r\n\r\n" +
				"This is a calibrated texture manifest file generated from the uncalibrated image \"" + m_CaptureParameters.SourceImageName + "\"\r\n" +
				"Resulting generated images have been stored using a standard sRGB profile and can be used directly as source or color-picked by artists\r\n" +
				" without any other processing. Colors in the textures will have the proper reflectance (assuming the original image has been properly captured\r\n" +
				" with specular removal using polarization filters) and after sRGB->Linear conversion will be directly useable as reflectance in the lighting equation.\r\n" +
				"The xyY values are given in device-independent xyY color space and can be used as linear-space colors directly.\r\n\r\n" +
				"***Do not modify!***" );
			Doc.AppendChild( HeaderComment );

			XmlElement	Root = Doc.CreateElement( "Manifest" );
			Doc.AppendChild( Root );

			// Save source image infos
			XmlElement	SourceInfosElement = AppendElement( Root, "SourceInfos" );
			SetAttribute( AppendElement( SourceInfosElement, "SourceImageName" ), "Value", m_CaptureParameters.SourceImageName );
			SetAttribute( AppendElement( SourceInfosElement, "ISOSpeed" ), "Value", m_CaptureParameters.ISOSpeed.ToString() );
			SetAttribute( AppendElement( SourceInfosElement, "ShutterSpeed" ), "Value", m_CaptureParameters.ShutterSpeed.ToString() );
			SetAttribute( AppendElement( SourceInfosElement, "Aperture" ), "Value", m_CaptureParameters.Aperture.ToString() );

			SetAttribute( AppendElement( SourceInfosElement, "SpatialCorrection" ), "Status", m_SpatialCorrectionEnabled ? "Enabled" : "Disabled" );
			SetAttribute( AppendElement( SourceInfosElement, "WhiteReflectanceCorrectionFactor" ), "Value", m_WhiteReflectanceCorrectionFactor.ToString() );
			if ( m_WhiteReflectanceReference.z > 0.0f )
				SetAttribute( AppendElement( SourceInfosElement, "WhiteBalance" ), "xyY", m_WhiteReflectanceReference.ToString() );

			SetAttribute( AppendElement( SourceInfosElement, "CropSource" ), "Value", m_CaptureParameters.CropSource.ToString() );
			SetAttribute( AppendElement( SourceInfosElement, "CropRectangleCenter" ), "X", m_CaptureParameters.CropRectangleCenter.x.ToString() ).SetAttribute( "Y", m_CaptureParameters.CropRectangleCenter.y.ToString() );
			SetAttribute( AppendElement( SourceInfosElement, "CropRectangleHalfSize" ), "X", m_CaptureParameters.CropRectangleHalfSize.x.ToString() ).SetAttribute( "Y", m_CaptureParameters.CropRectangleHalfSize.y.ToString() );
			SetAttribute( AppendElement( SourceInfosElement, "CropRectangleRotation" ), "Value", m_CaptureParameters.CropRectangleRotation.ToString() );

			SetAttribute( AppendElement( SourceInfosElement, "SwatchesSize" ), "Width", m_SwatchWidth.ToString() ).SetAttribute( "Height", m_SwatchHeight.ToString() );

			SetAttribute( AppendElement( SourceInfosElement, "TargetFormat" ), "Value", _TargetFormat.ToString() );

			// Save calibrated texture infos
			{
				XmlElement	CalibratedTextureElement = AppendElement( Root, "CalibratedTexture" );
				SetAttribute( CalibratedTextureElement, "Name", _FileName.Name ).SetAttribute( "Width", m_Texture.Width.ToString() ).SetAttribute( "Height", m_Texture.Height.ToString() );

				// Save default swatches
				XmlElement	DefaultSwatchesElement = AppendElement( CalibratedTextureElement, "DefaultSwatches" );
				XmlElement	MinSwatchElement = AppendElement( DefaultSwatchesElement, "Min" );
				SetAttribute( MinSwatchElement, "Name", FileName_SwatchMin.Name );
				m_SwatchMin.Save( this, MinSwatchElement );

				XmlElement	MaxSwatchElement = AppendElement( DefaultSwatchesElement, "Max" );
				SetAttribute( MaxSwatchElement, "Name", FileName_SwatchMax.Name );
				m_SwatchMax.Save( this, MaxSwatchElement );

				XmlElement	AvgSwatchElement = AppendElement( DefaultSwatchesElement, "Avg" );
				SetAttribute( AvgSwatchElement, "Name", FileName_SwatchAvg.Name );
				m_SwatchAvg.Save( this, AvgSwatchElement );
			}

			// Save custom swatches infos
			if ( m_CustomSwatches.Length > 0 )
			{
				XmlElement	CustomSwatchesElement = AppendElement( Root, "CustomSwatches" );
				SetAttribute( CustomSwatchesElement, "Count", m_CustomSwatches.Length.ToString() );
				for ( int CustomSwatchIndex=0; CustomSwatchIndex < m_CustomSwatches.Length; CustomSwatchIndex++ )
				{
					XmlElement	CustomSwatchElement = AppendElement( CustomSwatchesElement, "Custom"+CustomSwatchIndex.ToString() );
					SetAttribute( CustomSwatchElement, "Name", FileName_CustomSwatches[CustomSwatchIndex].Name );
					m_CustomSwatches[CustomSwatchIndex].Save( this, CustomSwatchElement );
				}
			}

			Doc.Save( FileName_Manifest.FullName );
		}

		/// <summary>
		/// Computes the average color within a rectangle in UV space
		/// </summary>
		/// <param name="_TopLeft">The top left corner (in UV space) of the rectangle to sample</param>
		/// <param name="_BottomRight">The bottom right corner (in UV space) of the rectangle to sample</param>
		/// <returns>The average xyY color</returns>
		public float3	ComputeAverageSwatchColor( float2 _TopLeft, float2 _BottomRight )
		{
			// Average xyY values in the specified rectangle
			int		X0 = Math.Max( 0, Math.Min( m_Texture.Width-1, (int) Math.Floor( _TopLeft.x * m_Texture.Width ) ) );
			int		Y0 = Math.Max( 0, Math.Min( m_Texture.Height-1, (int) Math.Floor( _TopLeft.y * m_Texture.Height ) ) );
			int		X1 = Math.Min( m_Texture.Width, Math.Max( X0+1, (int) Math.Floor( _BottomRight.x * m_Texture.Width ) ) );
			int		Y1 = Math.Min( m_Texture.Height, Math.Max( Y0+1, (int) Math.Floor( _BottomRight.y * m_Texture.Height ) ) );
			int		W = X1 - X0;
			int		H = Y1 - Y0;

			float3	AveragexyY = new float3( 0, 0, 0 );
			for ( int Y=Y0; Y < Y1; Y++ )
				for ( int X=X0; X < X1; X++ )
				{
					float4	XYZ = m_Texture.ContentXYZ[X,Y];
					float3	xyY = Bitmap2.ColorProfile.XYZ2xyY( (float3) XYZ );
					AveragexyY += xyY;
				}
			AveragexyY = (1.0f / (W*H)) * AveragexyY;

			return AveragexyY;
		}

		/// <summary>
		/// Saves a texture to disk
		/// </summary>
		/// <param name="_Texture"></param>
		/// <param name="_FileName"></param>
		/// <param name="_FileType"></param>
		/// <param name="_Format"></param>
		private void	SaveImage( Bitmap2 _Texture, System.IO.FileInfo _FileName, Bitmap2.FILE_TYPE _FileType, Bitmap2.FORMAT_FLAGS _Format )
		{
			using ( System.IO.FileStream S = _FileName.Create() )
				_Texture.Save( S, _FileType, _Format );
		}

		/// <summary>
		/// Builds a swatch bitmap
		/// </summary>
		/// <param name="_Width"></param>
		/// <param name="_Height"></param>
		/// <param name="_xyY"></param>
		/// <returns></returns>
		private Bitmap2	BuildSwatch( int _Width, int _Height, float3 _xyY )
		{
			Bitmap2	Result = new Bitmap2( _Width, _Height, new Bitmap2.ColorProfile( Bitmap2.ColorProfile.STANDARD_PROFILE.sRGB ) );
			float4	XYZ = new float4( Bitmap2.ColorProfile.xyY2XYZ( _xyY ), 1.0f );
			for ( int Y=0; Y < _Height; Y++ )
				for ( int X=0; X < _Width; X++ )
					Result.ContentXYZ[X,Y] = XYZ;

			return Result;
		}

		private XmlElement	AppendElement( XmlNode _ParentNode, string _ElementName )
		{
			XmlElement	E = _ParentNode.OwnerDocument.CreateElement( _ElementName );
			_ParentNode.AppendChild( E );
			return E;
		}
		private XmlElement	m_CurrentElement = null;
		private CalibratedTexture	SetAttribute( XmlElement _Element, string _Attribute, string _Value )
		{
			m_CurrentElement = _Element;
			m_CurrentElement.SetAttribute( _Attribute, _Value );
			return this;
		}
		private CalibratedTexture	SetAttribute( string _Attribute, string _Value )
		{
			m_CurrentElement.SetAttribute( _Attribute, _Value );
			return this;
		}

		#region IDisposable Members

		public void Dispose()
		{
			if ( m_Texture != null )
				m_Texture.Dispose();

			if ( m_SwatchMin.Texture != null )
				m_SwatchMin.Texture.Dispose();
			if ( m_SwatchMax.Texture != null )
				m_SwatchMax.Texture.Dispose();
			if ( m_SwatchAvg.Texture != null )
				m_SwatchAvg.Texture.Dispose();

			for ( int i=0; i < m_CustomSwatches.Length; i++ )
				if ( m_CustomSwatches[i].Texture != null )
					m_CustomSwatches[i].Texture.Dispose();
		}

		#endregion

		#endregion
	}
}
