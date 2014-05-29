using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using WMath;

namespace StandardizedDiffuseAlbedoMaps
{
	/// <summary>
	/// This class hosts camera calibration data
	/// 
	/// </summary>
	[System.Diagnostics.DebuggerDisplay( "Ref={System.IO.Path.GetFileName( m_ReferenceImageName )} ISO={m_CameraShotInfos.m_ISOSpeed} Shutter={m_CameraShotInfos.m_ShutterSpeed} Aperture={m_CameraShotInfos.m_Aperture} Focal={m_CameraShotInfos.m_FocalLength}" )]
	public class CameraCalibration
	{
		#region NESTED TYPES

		/// <summary>
		/// Contains various luminance information about the reflectance probe
		/// </summary>
		[System.Diagnostics.DebuggerDisplay( "Reflectance={StandardReflectance} Measured={m_LuminanceMeasured} Normalized={m_LuminanceNormalized}" )]
		public class Probe
		{
			private float	m_StandardReflectance;	// Standard reflectance (read it but never change it!)
			public float	StandardReflectance	{ get { return m_StandardReflectance; } internal set { m_StandardReflectance = value; } }

			public bool		m_IsAvailable = false;
			public float	m_LuminanceMeasured;	// Luminance as measured from a reference image containing the probe
			public float	m_LuminanceNormalized;	// Fully adapted luminance, normalized by the 2% and 99% probes' values
			public float	m_LuminanceRelative;	// Luminance value relative to previous available probe

			// Defines the coordinates of the disc used for the measurement
			public bool		m_MeasurementDiscIsAvailable = false;
			public float	m_MeasurementCenterX;
			public float	m_MeasurementCenterY;
			public float	m_MeasurementRadius;

			public void		Load( XmlElement _Parent )
			{
				XmlElement	Element;
				m_IsAvailable = bool.Parse( _Parent.GetAttribute( "IsAvailable" ) );

				Element = _Parent["Luminance"];
				m_LuminanceMeasured = Element != null ? float.Parse( Element.GetAttribute( "Measured" ) ) : 0.0f;
				m_LuminanceNormalized = Element != null ? float.Parse( Element.GetAttribute( "Normalized" ) ) : 0.0f;

				Element = _Parent["MeasurementLocation"];
				m_MeasurementCenterX = Element != null ? float.Parse( Element.GetAttribute( "X" ) ) : 0.0f;
				m_MeasurementCenterY = Element != null ? float.Parse( Element.GetAttribute( "Y" ) ) : 0.0f;
				m_MeasurementRadius = Element != null ? float.Parse( Element.GetAttribute( "Radius" ) ) : 0.0f;

				if ( Element != null && Element.GetAttribute( "Available" ) != "" )
					m_MeasurementDiscIsAvailable = bool.Parse( Element.GetAttribute( "Available" ) );
				else
					m_MeasurementDiscIsAvailable = m_MeasurementCenterX != 0.0f;	// Assume it's available as soon as it has a value other than default
			}

			public void		Save( XmlElement _Parent )
			{
				_Parent.SetAttribute( "IsAvailable", m_IsAvailable.ToString() );

				XmlElement	Element;
				Element = _Parent.OwnerDocument.CreateElement( "Luminance" );			_Parent.AppendChild( Element );
				Element.SetAttribute( "Measured", m_LuminanceMeasured.ToString() );
				Element.SetAttribute( "Normalized", m_LuminanceNormalized.ToString() );

				Element = _Parent.OwnerDocument.CreateElement( "MeasurementLocation" );	_Parent.AppendChild( Element );
				Element.SetAttribute( "Available", m_MeasurementDiscIsAvailable.ToString() );
				Element.SetAttribute( "X", m_MeasurementCenterX.ToString() );
				Element.SetAttribute( "Y", m_MeasurementCenterY.ToString() );
				Element.SetAttribute( "Radius", m_MeasurementRadius.ToString() );
			}
		}

		/// <summary>
		/// Contains shot information about the image used for calibration
		/// </summary>
		[System.Diagnostics.DebuggerDisplay( "ISO={m_ISOSpeed} Shutter={m_ShutterSpeed} Aperture={m_Aperture} m_FocalLength={m_FocalLength}" )]
		public class CameraShotInfo
		{
			public float	m_ISOSpeed = -1.0f;
			public float	m_ShutterSpeed = -1.0f;
			public float	m_Aperture = -1.0f;
			public float	m_FocalLength = -1.0f;

			public void		Load( XmlElement _Parent )
			{
				XmlElement	Element;
				Element = _Parent["ISOSpeed"];
				m_ISOSpeed = Element != null ? float.Parse( Element.GetAttribute( "Value" ) ) : -1.0f;

				Element = _Parent["ShutterSpeed"];
				m_ShutterSpeed = Element != null ? float.Parse( Element.GetAttribute( "Value" ) ) : -1.0f;

				Element = _Parent["Aperture"];
				m_Aperture = Element != null ? float.Parse( Element.GetAttribute( "Value" ) ) : -1.0f;

				Element = _Parent["FocalLength"];
				m_FocalLength = Element != null ? float.Parse( Element.GetAttribute( "Value" ) ) : -1.0f;
			}

			public void		Save( XmlElement _Parent )
			{
				XmlElement	Element;
				Element = _Parent.OwnerDocument.CreateElement( "ISOSpeed" );		_Parent.AppendChild( Element );
				Element.SetAttribute( "Value", m_ISOSpeed.ToString() );

				Element = _Parent.OwnerDocument.CreateElement( "ShutterSpeed" );	_Parent.AppendChild( Element );
				Element.SetAttribute( "Value", m_ShutterSpeed.ToString() );

				Element = _Parent.OwnerDocument.CreateElement( "Aperture" );		_Parent.AppendChild( Element );
				Element.SetAttribute( "Value", m_Aperture.ToString() );

				Element = _Parent.OwnerDocument.CreateElement( "FocalLength" );		_Parent.AppendChild( Element );
				Element.SetAttribute( "Value", m_FocalLength.ToString() );
			}
		}

		#endregion

		#region FIELDS

		public string			m_ReferenceImageName = null;
		public int				m_ReferenceImageWidth = 0;
		public int				m_ReferenceImageHeight = 0;

		public Probe			m_Reflectance02 = new Probe() { StandardReflectance = 0.02f };	//  2% reflectance probe
		public Probe			m_Reflectance10 = new Probe() { StandardReflectance = 0.10f };	// 10% reflectance probe
		public Probe			m_Reflectance20 = new Probe() { StandardReflectance = 0.20f };	// 20% reflectance probe
		public Probe			m_Reflectance50 = new Probe() { StandardReflectance = 0.50f };	// 50% reflectance probe
		public Probe			m_Reflectance75 = new Probe() { StandardReflectance = 0.75f };	// 75% reflectance probe
		public Probe			m_Reflectance99 = new Probe() { StandardReflectance = 0.99f };	// 99% reflectance probe
		public Probe[]			m_Reflectances = null;

		public CameraShotInfo	m_CameraShotInfos = new CameraShotInfo();

		public byte[,]			m_Thumbnail = null;

		#endregion

		public CameraCalibration()
		{
			m_Reflectances = new Probe[] {
				m_Reflectance02,
				m_Reflectance10,
				m_Reflectance20,
				m_Reflectance50,
				m_Reflectance75,
				m_Reflectance99,
			};
		}

		/// <summary>
		/// Fills unavailable probes' measured luminances by interpolating from available probes' values
		///  and computes the "normalized" luminances that are rescaled between 2% (min reflectance probe) and 99% (max reflectance probe)
		/// </summary>
		public void	UpdateAllLuminances()
		{
			//////////////////////////////////////////////////////////////////////////
			// 1] First, fill in the unavailable probe values
			for ( int i=0; i < m_Reflectances.Length; i++ )
			{
				Probe	CurrentReflectance = m_Reflectances[i];
				if ( !CurrentReflectance.m_IsAvailable )
					CurrentReflectance.m_LuminanceMeasured = 0.0f;
			}

			// Browse array from top to bottom and interpolate from last known value
			Probe	LastAvailableProbe = null;
			for ( int i=m_Reflectances.Length-1; i >= 0; i-- )
			{
				Probe	CurrentReflectance = m_Reflectances[i];
				if ( CurrentReflectance.m_IsAvailable )
					LastAvailableProbe = CurrentReflectance;
				else if ( LastAvailableProbe != null && CurrentReflectance.m_LuminanceMeasured < 1e-6f )
					CurrentReflectance.m_LuminanceMeasured = LastAvailableProbe.m_LuminanceMeasured * CurrentReflectance.StandardReflectance / LastAvailableProbe.StandardReflectance;
			}

			// 1.2] Browse array from bottom to top and interpolate from last known value
			LastAvailableProbe = null;
			for ( int i=0; i < m_Reflectances.Length; i++ )
			{
				Probe	CurrentReflectance = m_Reflectances[i];
				if ( CurrentReflectance.m_IsAvailable )
					LastAvailableProbe = CurrentReflectance;
				else if ( LastAvailableProbe != null && CurrentReflectance.m_LuminanceMeasured < 1e-6f )
					CurrentReflectance.m_LuminanceMeasured = LastAvailableProbe.m_LuminanceMeasured * CurrentReflectance.StandardReflectance / LastAvailableProbe.StandardReflectance;
			}

			//////////////////////////////////////////////////////////////////////////
			// 2] Compute normalized luminances
			// After this step, if the 2% and 99% luminances have been properly set then their normalized reflectances will be 2% and 99%
			// All probes inbetween should have their corresponding reflectance value as well if the image is truly in linear space.
			// If discrepancies exist for inbetween probes (i.e. not a nice straight line from 2% to 99%) then interpolation must be used
			//	to correctly retrieve the reflectances of intermediate values...
			//
			float	Luminance_02 = m_Reflectance02.m_LuminanceMeasured;	// This is our smallest known luminance
			float	Luminance_99 = m_Reflectance99.m_LuminanceMeasured;	// This is our largest known luminance
			for ( int i=0; i < 6; i++ )
			{
				Probe	CurrentReflectance = m_Reflectances[i];
				float	t = (CurrentReflectance.m_LuminanceMeasured - Luminance_02) / Math.Max( 1e-6f, Luminance_99 - Luminance_02);
				CurrentReflectance.m_LuminanceNormalized = m_Reflectance02.StandardReflectance + t * (m_Reflectance99.StandardReflectance - m_Reflectance02.StandardReflectance);
			}

			//////////////////////////////////////////////////////////////////////////
			// 3] Compute relative luminances (informal purpose only)
			for ( int i=1; i < 6; i++ )
			{
				Probe	CurrentReflectance = m_Reflectances[i];
				CurrentReflectance.m_LuminanceRelative = CurrentReflectance.m_LuminanceNormalized / m_Reflectances[i-1].m_LuminanceNormalized;
			}
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
			Probe	PreviousProbe = m_Reflectances[0];
			for ( int ProbeIndex=1; ProbeIndex < m_Reflectances.Length; ProbeIndex++ )
			{
				Probe	CurrentProbe = m_Reflectances[ProbeIndex];
				if ( CurrentProbe.m_LuminanceMeasured > _Luminance || ProbeIndex == m_Reflectances.Length-1 )
				{	// Found the correct interval!
					float	t = (_Luminance - PreviousProbe.m_LuminanceMeasured) / (CurrentProbe.m_LuminanceMeasured - PreviousProbe.m_LuminanceMeasured);
							t = Math.Max( 0.0f, Math.Min( 1.0f, t ) );	// Should already be in [0,1] but who knows?
					float	CalibratedLuminance = PreviousProbe.StandardReflectance + t * (CurrentProbe.StandardReflectance - PreviousProbe.StandardReflectance);
					return CalibratedLuminance;
				}
				PreviousProbe = CurrentProbe;
			}

			// Out of range? How come?
			return 1.0f;
		}

		/// <summary>
		/// Creates an embeddable thumbnail of the reference image
		/// </summary>
		/// <param name="_Image"></param>
		public void		CreateThumbnail( Bitmap2 _Image )
		{
			int		MaxDim = Math.Max( _Image.Width, _Image.Height );
			int		ThumbnailSize = 256;

			int		W = ThumbnailSize * _Image.Width / MaxDim;
			int		H = ThumbnailSize * _Image.Height / MaxDim;

			// Build the thumbnail
			m_Thumbnail = new byte[W,H];
			for ( int Y=0; Y < H; Y++ )
				for ( int X=0; X < W; X++ )
				{
					float4	XYZ = _Image.ContentXYZ[X*_Image.Width/W, Y*_Image.Height/H];
					m_Thumbnail[X,Y] = (byte) Math.Min( 255, Math.Max( 0, 255.0f * XYZ.y ) );
				}
		}

		/// <summary>
		/// Loads the calibration data from an XML file
		/// </summary>
		/// <param name="_FileName"></param>
		public void		Load( System.IO.FileInfo _FileName )
		{
			XmlDocument	Doc = new XmlDocument();
			Doc.Load( _FileName.FullName );

			XmlElement	Root = Doc["CameraCalibration"];
			if ( Root == null )
				throw new Exception( "Failed to find expected root element \"CameraCalibration\"!" );

			// Get reference image data
			XmlElement	ImageRefElement = Root["ImageReference"];
			if ( ImageRefElement == null )
				throw new Exception( "Failed to find expected element \"ImageReference\"!" );

			m_ReferenceImageName = ImageRefElement.GetAttribute( "Name" );
			m_ReferenceImageWidth = int.Parse( ImageRefElement.GetAttribute( "Width" ) );
			m_ReferenceImageHeight = int.Parse( ImageRefElement.GetAttribute( "Height" ) );

			m_CameraShotInfos.Load( ImageRefElement );

			// Load thumbnail
			m_Thumbnail = null;
			XmlElement	ThumbnailElement = ImageRefElement["Thumbnail"];
			if ( ThumbnailElement != null )
			{
				int	W = int.Parse( ThumbnailElement.GetAttribute( "Width" ) );
				int	H = int.Parse( ThumbnailElement.GetAttribute( "Height" ) );
				m_Thumbnail = new byte[W,H];

				XmlCDataSection	ThumbnailCData = ThumbnailElement.FirstChild as XmlCDataSection;
				if ( ThumbnailCData != null )
				{
					byte[]	PackedThumbnail = System.Convert.FromBase64String( ThumbnailCData.Data );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
							if ( Y*W+X < PackedThumbnail.Length )
								m_Thumbnail[X,Y] = PackedThumbnail[Y*W+X];
				}
			}

			// Read reflectance infos
			XmlElement	ReflectanceProbesElement = Root["ReflectanceProbes"];
			if ( ReflectanceProbesElement == null )
				throw new Exception( "Failed to find expected element \"ReflectanceProbes\"!" );

			foreach ( XmlNode ChildNode in ReflectanceProbesElement.ChildNodes )
			{
				XmlElement	ChildElement = ChildNode as XmlElement;
				if ( ChildElement == null )
					continue;

				if ( ChildElement.Name != "Probe" || !ChildElement.HasAttribute( "StandardReflectance" ) )
					continue;

				float	StandardReflectance = 0;
				if ( !float.TryParse( ChildElement.GetAttribute( "StandardReflectance" ), out StandardReflectance ) )
					throw new Exception( "Failed to retrieve standard reflectance index from probe element!" );

				foreach ( Probe P in m_Reflectances )
					if ( Math.Abs( P.StandardReflectance - StandardReflectance ) < 1e-6f )
					{	// Found the probe to load into!
						P.Load( ChildElement );
						break;
					}
			}
		}

		/// <summary>
		/// Saves the calibration data to an XML file
		/// </summary>
		/// <param name="_FileName"></param>
		public void		Save( System.IO.FileInfo _FileName )
		{
			XmlDocument	Doc = new XmlDocument();
			XmlElement	Root = Doc.CreateElement( "CameraCalibration" );
			Doc.AppendChild( Root );

			// Set reference image data
			XmlElement	ImageRefElement = Doc.CreateElement( "ImageReference" );
			Root.AppendChild( ImageRefElement );
			ImageRefElement.SetAttribute( "Name", m_ReferenceImageName != null ? m_ReferenceImageName : "unknown" );
			ImageRefElement.SetAttribute( "Width", m_ReferenceImageWidth.ToString() );
			ImageRefElement.SetAttribute( "Height", m_ReferenceImageHeight.ToString() );
			m_CameraShotInfos.Save( ImageRefElement );

			// Set thumbnail
			XmlElement	ThumbnailElement = Doc.CreateElement( "Thumbnail" );
			ImageRefElement.AppendChild( ThumbnailElement );
			ThumbnailElement.SetAttribute( "Width", m_Thumbnail.GetLength(0).ToString() );
			ThumbnailElement.SetAttribute( "Height", m_Thumbnail.GetLength(1).ToString() );

			byte[]	PackedThumbnail = new byte[m_Thumbnail.Length];
			for ( int Y=0; Y < m_Thumbnail.GetLength(1); Y++ )
				for ( int X=0; X < m_Thumbnail.GetLength(0); X++ )
					PackedThumbnail[m_Thumbnail.GetLength(0)*Y+X] = m_Thumbnail[X,Y];
			string	PackedThumbnailString = System.Convert.ToBase64String( PackedThumbnail );
			XmlCDataSection	ThumbnailCData = Doc.CreateCDataSection( PackedThumbnailString );
			ThumbnailElement.AppendChild( ThumbnailCData );

			// Write reflectance infos
			XmlElement	ReflectanceProbesElement = Doc.CreateElement( "ReflectanceProbes" );
			Root.AppendChild( ReflectanceProbesElement );
			foreach ( Probe P in m_Reflectances )
			{
				XmlElement	ProbeElement = Doc.CreateElement( "Probe" );
				ReflectanceProbesElement.AppendChild( ProbeElement );

				ProbeElement.SetAttribute( "StandardReflectance", P.StandardReflectance.ToString() );

				P.Save( ProbeElement );
			}

			Doc.Save( _FileName.FullName );
		}
	}
}
