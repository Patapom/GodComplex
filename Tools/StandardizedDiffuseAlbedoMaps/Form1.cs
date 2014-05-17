using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace StandardizedDiffuseAlbedoMaps
{
	public partial class Form1 : Form
	{
		#region CONSTANTS

		private const string	APP_TITLE = "Standardized Diffuse Albedo Maps Creator";
		private const float		BLACK_VALUES_TOLERANCE = 0.05f;		// We can tolerate up to 5% black values for a probe to be valid
		private const float		SATURATED_VALUES_TOLERANCE = 0.05f;	// We can tolerate up to 5% saturated values for a probe to be valid

		#endregion

		#region FIELDS

		private RegistryKey			m_AppKey;
		private string				m_ApplicationPath;

		private System.IO.FileInfo	m_ImageFileName = null;
		private Bitmap2				m_BitmapXYZ = null;

		// Calibration database
		private CameraCalibrationDatabase	m_CalibrationDatabase = new CameraCalibrationDatabase();
		private CameraCalibration			m_Calibration = new CameraCalibration();	// Current calibration

		#endregion

		#region METHODS

		public Form1()
		{
 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\StandardizedDiffuseAlbedoMaps" );
			m_ApplicationPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );

			InitializeComponent();
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			try
			{
				m_CalibrationDatabase.DatabasePath = new System.IO.DirectoryInfo( System.IO.Path.GetDirectoryName( GetRegKey( "LastCalibrationDatabasePath", m_ApplicationPath ) ) );
			}
			catch ( Exception _e )
			{
				MessageBox( "Failed to parse the calibration database:\r\n\r\n", _e );
			}

			UpdateUIFromCalibration();
		}

		#region Helpers

		private string	GetRegKey( string _Key, string _Default )
		{
			string	Result = m_AppKey.GetValue( _Key ) as string;
			return Result != null ? Result : _Default;
		}
		private void	SetRegKey( string _Key, string _Value )
		{
			m_AppKey.SetValue( _Key, _Value );
		}

		private float	GetRegKeyFloat( string _Key, float _Default )
		{
			string	Value = GetRegKey( _Key, _Default.ToString() );
			float	Result;
			float.TryParse( Value, out Result );
			return Result;
		}

		private int		GetRegKeyInt( string _Key, float _Default )
		{
			string	Value = GetRegKey( _Key, _Default.ToString() );
			int		Result;
			int.TryParse( Value, out Result );
			return Result;
		}

		private DialogResult	MessageBox( string _Text )
		{
			return MessageBox( _Text, MessageBoxButtons.OK );
		}
		private DialogResult	MessageBox( string _Text, Exception _e )
		{
			return MessageBox( _Text + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons )
		{
			return MessageBox( _Text, _Buttons, MessageBoxIcon.Information );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxIcon _Icon )
		{
			return MessageBox( _Text, MessageBoxButtons.OK, _Icon );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon )
		{
			return System.Windows.Forms.MessageBox.Show( this, _Text, "Texture Reflectances Generator", _Buttons, _Icon );
		}

		#endregion

		private void RebuildImage()
		{
			if ( m_BitmapXYZ == null )
				return;

			bool		sRGB = checkBoxsRGB.Checked;
			float3[,]	Image = new float3[m_BitmapXYZ.Width,m_BitmapXYZ.Height];

			if ( checkBoxLuminance.Checked )
			{
				for ( int Y = 0; Y < m_BitmapXYZ.Height; Y++ )
					for ( int X = 0; X < m_BitmapXYZ.Width; X++ )
					{
						float L = m_BitmapXYZ.ContentXYZ[X, Y].y;
						if ( sRGB )
							L = Bitmap2.ColorProfile.Linear2sRGB( L );

						Image[X, Y].x = L;
						Image[X, Y].y = L;
						Image[X, Y].z = L;
					}
			}
			else
			{
				for ( int Y = 0; Y < m_BitmapXYZ.Height; Y++ )
					for ( int X = 0; X < m_BitmapXYZ.Width; X++ )
					{
						float4	XYZ = m_BitmapXYZ.ContentXYZ[X, Y];
						float4	RGB = m_BitmapXYZ.Profile.XYZ2RGB( XYZ );
						if ( sRGB )
						{
							RGB.x = Bitmap2.ColorProfile.Linear2sRGB( RGB.x );
							RGB.y = Bitmap2.ColorProfile.Linear2sRGB( RGB.y );
							RGB.z = Bitmap2.ColorProfile.Linear2sRGB( RGB.z );
						}

						Image[X, Y].x = RGB.x;
						Image[X, Y].y = RGB.y;
						Image[X, Y].z = RGB.z;
					}
			}

			outputPanel.Image = Image;
		}

		#endregion

		#region EVENT HANDLERS

		private void buttonLoadImage_Click( object sender, EventArgs e )
		{
 			string	OldFileName = GetRegKey( "LastImageFilename", m_ApplicationPath );
			openFileDialogSourceImage.InitialDirectory = System.IO.Path.GetDirectoryName( OldFileName );
			openFileDialogSourceImage.FileName = System.IO.Path.GetFileName( OldFileName );

			if ( openFileDialogSourceImage.ShowDialog( this ) != DialogResult.OK )
 				return;

			SetRegKey( "LastImageFilename", openFileDialogSourceImage.FileName );

			try
			{
				// Load
				System.IO.FileInfo	ImageFileName = new System.IO.FileInfo( openFileDialogSourceImage.FileName );
				Bitmap2	NewBitmap = new Bitmap2( ImageFileName );

				// Safely assign once loaded
				m_ImageFileName = ImageFileName;
				m_BitmapXYZ = NewBitmap;

				this.Text = APP_TITLE + " (" + ImageFileName.Name + ")";

				// Setup camera shot info if it exists
				if ( m_BitmapXYZ.HasValidShotInfo )
				{
					groupBoxCameraShotInfos.Enabled = false;
					floatTrackbarControlISOSpeed.Value = m_BitmapXYZ.ISOSpeed;			floatTrackbarControlISOSpeed.SimulateValueChange();	// So we get notified even if value is the same as default slider value
					floatTrackbarControlShutterSpeed.Value = m_BitmapXYZ.ShutterSpeed;	floatTrackbarControlShutterSpeed.SimulateValueChange();	// So we get notified even if value is the same as default slider value
					floatTrackbarControlAperture.Value = m_BitmapXYZ.Aperture;			floatTrackbarControlAperture.SimulateValueChange();	// So we get notified even if value is the same as default slider value
					floatTrackbarControlFocalLength.Value = m_BitmapXYZ.FocalLength;	floatTrackbarControlFocalLength.SimulateValueChange();	// So we get notified even if value is the same as default slider value
				}
				else
					groupBoxCameraShotInfos.Enabled = true;

				RebuildImage();
				UpdateUIFromCalibration();
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while loading the image:\r\n\r\n", _e );
			}
		}

		private void outputPanel_MouseMove( object sender, MouseEventArgs e )
		{
			if ( m_BitmapXYZ == null )
				return;

			float	Lum = m_BitmapXYZ.ContentXYZ[e.X*m_BitmapXYZ.Width/outputPanel.Width,e.Y*m_BitmapXYZ.Height/outputPanel.Height].y;
			if ( checkBoxsRGB.Checked )
				Lum = Bitmap2.ColorProfile.Linear2sRGB( Lum );

			labelLuminance.Text = "L=" + Lum.ToString( "G4" ) + " (" + (int) (Lum*255) + ")";
		}

		private void checkBoxsRGB_CheckedChanged( object sender, EventArgs e )
		{
			RebuildImage();
		}

		private void checkBoxLuminance_CheckedChanged( object sender, EventArgs e )
		{
			RebuildImage();
		}

		#region Calibration

		/// <summary>
		/// Integrates the luminance of the pixels in a given circle selected by the user
		/// </summary>
		/// <param name="_X"></param>
		/// <param name="_Y"></param>
		/// <param name="_Radius"></param>
		/// <param name="_PercentOfBlackValues"></param>
		/// <param name="_HasSaturatedValues">Returns the percentage of encountered saturated values. 0 is okay, more means the probe shouldn't be used</param>
		/// <returns></returns>
		private float	IntegrateLuminance( float _X, float _Y, float _Radius, out float _PercentOfBlackValues, out float _PercentOfSaturatedValues )
		{
			float	Radius = _Radius * m_BitmapXYZ.Width;
			float	CenterX = _X * m_BitmapXYZ.Width;
			float	CenterY = _Y * m_BitmapXYZ.Height;
			float	SqRadius = Radius*Radius;

			int	X0 = Math.Max( 0, Math.Min( m_BitmapXYZ.Width-1, (int) Math.Floor( CenterX - Radius ) ) );
			int	X1 = Math.Max( 0, Math.Min( m_BitmapXYZ.Width-1, (int) Math.Ceiling( CenterX + Radius ) ) );
			int	Y0 = Math.Max( 0, Math.Min( m_BitmapXYZ.Height-1, (int) Math.Floor( CenterY - Radius ) ) );
			int	Y1 = Math.Max( 0, Math.Min( m_BitmapXYZ.Height-1, (int) Math.Ceiling( CenterY + Radius ) ) );

			const float	SMOOTHSTEP_MAX_RADIUS = 0.2f;	// We reach max weight 1 at 20% of the border of the circle

			int		TotalBlackValuesCount = 0;
			int		TotalSaturatedValuesCount = 0;
			int		TotalValuesCount = 0;
			float	SumLuminance = 0.0f;
			float	SumWeights = 0.0f;
			for ( int Y=Y0; Y < Y1; Y++ )
				for ( int X=X0; X < X1; X++ )
				{
					float	SqR = (X-CenterX)*(X-CenterX) + (Y-CenterY)*(Y-CenterY);
					if ( SqR > SqRadius )
						continue;

					float	r = (float) Math.Sqrt( SqR ) / Radius;	// Nomalized radius
//					float	Weight = Math.Min( 1.0f, floatTrackbarControl1.Value * (float) Math.Exp( -floatTrackbarControl2.Value * r ) );

					float	x = Math.Max( 0.0f, Math.Min( 1.0f, (1.0f - r) / SMOOTHSTEP_MAX_RADIUS ) );
					float	Weight = x*x*(3.0f - 2.0f*x);

//DEBUG					m_BitmapXYZ.ContentXYZ[X,Y].y = Weight;
					float	Luminance = m_BitmapXYZ.ContentXYZ[X,Y].y;
					if ( Luminance < 0.001f )
						TotalBlackValuesCount++;		// Warning!
					if ( Luminance > 0.999f )
						TotalSaturatedValuesCount++;	// Warning!
					TotalValuesCount++;

					SumLuminance += Weight * Luminance;
					SumWeights += Weight;
				}

//DEBUG			RebuildImage();

			_PercentOfBlackValues = (float) TotalBlackValuesCount / TotalValuesCount;
			_PercentOfSaturatedValues = (float) TotalSaturatedValuesCount / TotalValuesCount;

			SumLuminance /= SumWeights;
			return SumLuminance;
		}

		/// <summary>
		/// Integrates luminance for the provided probe by sampling luminances in the provided disc
		/// </summary>
		/// <param name="_Probe"></param>
		/// <param name="_Center"></param>
		/// <param name="_Radius"></param>
		private void	IntegrateLuminance( CameraCalibration.Probe _Probe, PointF _Center, float _Radius )
		{
			float	BlackValues, SaturatedValues;
			float	MeasuredLuminance = IntegrateLuminance( _Center.X, _Center.Y, _Radius, out BlackValues, out SaturatedValues );

			bool	DisableProbe = false;
			if ( BlackValues > BLACK_VALUES_TOLERANCE &&
				MessageBox( "This probe has more than 5% luminance values that are too dark, it's advised you don't use it to calibrate the camera as its values will not be useful.\r\n" +
							"\r\nDo you wish to disable this probe?",
							MessageBoxButtons.YesNo, MessageBoxIcon.Warning ) == DialogResult.Yes )
			{
				DisableProbe = true;
			}
			else if ( SaturatedValues > SATURATED_VALUES_TOLERANCE &&
				MessageBox( "This probe has more than 5% luminance values that are saturated, it's advised you don't use it to calibrate the camera as its values will not be useful.\r\n" +
							"\r\nDo you wish to disable this probe?",
							MessageBoxButtons.YesNo, MessageBoxIcon.Warning ) == DialogResult.Yes )
			{
				DisableProbe = true;
			}

			if ( DisableProbe )
			{	// Disable probe
				_Probe.m_IsAvailable = false;
				_Probe.m_LuminanceMeasured = 0.0f;
				UpdateUIFromCalibration();
				return;
			}

			_Probe.m_LuminanceMeasured = MeasuredLuminance;

			// We now have valid measurement disc infos
			_Probe.m_MeasurementDiscIsAvailable = true;
			_Probe.m_MeasurementCenterX = _Center.X;
			_Probe.m_MeasurementCenterY = _Center.Y;
			_Probe.m_MeasurementRadius = _Radius;

			CommitImageToCurrentCalibration();	// We now used the current image as reference for this calibration so commit its data
			UpdateUIFromCalibration();
		}

		/// <summary>
		/// Commit image data to camera calibration
		/// Must be called as soon as the current image is used to feed data to the camera calibration
		/// </summary>
		private void CommitImageToCurrentCalibration()
		{
			m_Calibration.m_ReferenceImageName = m_ImageFileName.FullName;
			m_Calibration.m_ReferenceImageWidth = m_BitmapXYZ.Width;
			m_Calibration.m_ReferenceImageHeight = m_BitmapXYZ.Height;

			m_Calibration.m_CameraShotInfos.m_ISOSpeed = floatTrackbarControlISOSpeed.Value;
			m_Calibration.m_CameraShotInfos.m_ShutterSpeed = floatTrackbarControlShutterSpeed.Value;
			m_Calibration.m_CameraShotInfos.m_Aperture = floatTrackbarControlAperture.Value;
			m_Calibration.m_CameraShotInfos.m_FocalLength = floatTrackbarControlFocalLength.Value;

			m_Calibration.CreateThumbnail( m_BitmapXYZ );
		}

		private void UpdateUIFromCalibration()
		{
			m_Calibration.UpdateAllLuminances();

			string	Format = "G4";

			labelCalbrationImageName.Text = m_Calibration.m_ReferenceImageName != null ? System.IO.Path.GetFileName( m_Calibration.m_ReferenceImageName ) : "<NO IMAGE>";

			checkBoxCalibrate02.Checked = m_Calibration.m_Reflectance02.m_IsAvailable;
			buttonCalibrate02.Enabled = m_Calibration.m_Reflectance02.m_IsAvailable;
			labelProbeNormalized02.Enabled = m_Calibration.m_Reflectance02.m_IsAvailable;
			labelProbeValue02.Text = m_Calibration.m_Reflectance02.m_IsAvailable ? m_Calibration.m_Reflectance02.m_LuminanceMeasured.ToString( Format ) : "";
			labelProbeNormalized02.Text = m_Calibration.m_Reflectance02.m_IsAvailable ? m_Calibration.m_Reflectance02.m_LuminanceNormalized.ToString( Format ) : "";
			labelProbeRelative02.Text = m_Calibration.m_Reflectance02.m_IsAvailable ? m_Calibration.m_Reflectance02.m_LuminanceRelative.ToString( Format ) : "";

			checkBoxCalibrate10.Checked = m_Calibration.m_Reflectance10.m_IsAvailable;
			buttonCalibrate10.Enabled = m_Calibration.m_Reflectance10.m_IsAvailable;
			labelProbeNormalized10.Enabled = m_Calibration.m_Reflectance10.m_IsAvailable;
			labelProbeValue10.Text = m_Calibration.m_Reflectance10.m_IsAvailable ? m_Calibration.m_Reflectance10.m_LuminanceMeasured.ToString( Format ) : "";
			labelProbeNormalized10.Text = m_Calibration.m_Reflectance10.m_IsAvailable ? m_Calibration.m_Reflectance10.m_LuminanceNormalized.ToString( Format ) : "";
			labelProbeRelative10.Text = m_Calibration.m_Reflectance10.m_IsAvailable ? m_Calibration.m_Reflectance10.m_LuminanceRelative.ToString( Format ) : "";

			checkBoxCalibrate20.Checked = m_Calibration.m_Reflectance20.m_IsAvailable;
			buttonCalibrate20.Enabled = m_Calibration.m_Reflectance20.m_IsAvailable;
			labelProbeNormalized20.Enabled = m_Calibration.m_Reflectance20.m_IsAvailable;
			labelProbeValue20.Text = m_Calibration.m_Reflectance20.m_IsAvailable ? m_Calibration.m_Reflectance20.m_LuminanceMeasured.ToString( Format ) : "";
			labelProbeNormalized20.Text = m_Calibration.m_Reflectance20.m_IsAvailable ? m_Calibration.m_Reflectance20.m_LuminanceNormalized.ToString( Format ) : "";
			labelProbeRelative20.Text = m_Calibration.m_Reflectance20.m_IsAvailable ? m_Calibration.m_Reflectance20.m_LuminanceRelative.ToString( Format ) : "";

			checkBoxCalibrate50.Checked = m_Calibration.m_Reflectance50.m_IsAvailable;
			buttonCalibrate50.Enabled = m_Calibration.m_Reflectance50.m_IsAvailable;
			labelProbeNormalized50.Enabled = m_Calibration.m_Reflectance50.m_IsAvailable;
			labelProbeValue50.Text = m_Calibration.m_Reflectance50.m_IsAvailable ? m_Calibration.m_Reflectance50.m_LuminanceMeasured.ToString( Format ) : "";
			labelProbeNormalized50.Text = m_Calibration.m_Reflectance50.m_IsAvailable ? m_Calibration.m_Reflectance50.m_LuminanceNormalized.ToString( Format ) : "";
			labelProbeRelative50.Text = m_Calibration.m_Reflectance50.m_IsAvailable ? m_Calibration.m_Reflectance50.m_LuminanceRelative.ToString( Format ) : "";

			checkBoxCalibrate75.Checked = m_Calibration.m_Reflectance75.m_IsAvailable;
			buttonCalibrate75.Enabled = m_Calibration.m_Reflectance75.m_IsAvailable;
			labelProbeNormalized75.Enabled = m_Calibration.m_Reflectance75.m_IsAvailable;
			labelProbeValue75.Text = m_Calibration.m_Reflectance75.m_IsAvailable ? m_Calibration.m_Reflectance75.m_LuminanceMeasured.ToString( Format ) : "";
			labelProbeNormalized75.Text = m_Calibration.m_Reflectance75.m_IsAvailable ? m_Calibration.m_Reflectance75.m_LuminanceNormalized.ToString( Format ) : "";
			labelProbeRelative75.Text = m_Calibration.m_Reflectance75.m_IsAvailable ? m_Calibration.m_Reflectance75.m_LuminanceRelative.ToString( Format ) : "";

			checkBoxCalibrate99.Checked = m_Calibration.m_Reflectance99.m_IsAvailable;
			buttonCalibrate99.Enabled = m_Calibration.m_Reflectance99.m_IsAvailable;
			labelProbeNormalized99.Enabled = m_Calibration.m_Reflectance99.m_IsAvailable;
			labelProbeValue99.Text = m_Calibration.m_Reflectance99.m_IsAvailable ? m_Calibration.m_Reflectance99.m_LuminanceMeasured.ToString( Format ) : "";
			labelProbeNormalized99.Text = m_Calibration.m_Reflectance99.m_IsAvailable ? m_Calibration.m_Reflectance99.m_LuminanceNormalized.ToString( Format ) : "";
			labelProbeRelative99.Text = m_Calibration.m_Reflectance99.m_IsAvailable ? m_Calibration.m_Reflectance99.m_LuminanceRelative.ToString( Format ) : "";

			graphPanel.Calibration = m_Calibration;				// Update the graph
			referenceImagePanel.Calibration = m_Calibration;	// Update the image thumbnail

			bool	CanReCalibrate = false;
			if ( m_BitmapXYZ != null )
			{
				foreach ( CameraCalibration.Probe P in m_Calibration.m_Reflectances )
					if ( P.m_IsAvailable )
					{	// We have at least one available probe location to use
						CanReCalibrate = true;
						break;
					}
			}
			buttonReCalibrate.Enabled = CanReCalibrate;
		}

		private void checkBoxCalibrate02_CheckedChanged( object sender, EventArgs e )
		{
			m_Calibration.m_Reflectance02.m_IsAvailable = (sender as CheckBox).Checked;
			UpdateUIFromCalibration();
		}

		private void checkBoxCalibrate10_CheckedChanged( object sender, EventArgs e )
		{
			m_Calibration.m_Reflectance10.m_IsAvailable = (sender as CheckBox).Checked;
			UpdateUIFromCalibration();
		}

		private void checkBoxCalibrate20_CheckedChanged( object sender, EventArgs e )
		{
			m_Calibration.m_Reflectance20.m_IsAvailable = (sender as CheckBox).Checked;
			UpdateUIFromCalibration();
		}

		private void checkBoxCalibrate50_CheckedChanged( object sender, EventArgs e )
		{
			m_Calibration.m_Reflectance50.m_IsAvailable = (sender as CheckBox).Checked;
			UpdateUIFromCalibration();
		}

		private void checkBoxCalibrate75_CheckedChanged( object sender, EventArgs e )
		{
			m_Calibration.m_Reflectance75.m_IsAvailable = (sender as CheckBox).Checked;
			UpdateUIFromCalibration();
		}

		private void checkBoxCalibrate99_CheckedChanged( object sender, EventArgs e )
		{
			m_Calibration.m_Reflectance99.m_IsAvailable = (sender as CheckBox).Checked;
			UpdateUIFromCalibration();
		}

		private void buttonCalibrate02_Click( object sender, EventArgs e )
		{
			outputPanel.StartCalibrationTargetPicking( ( PointF _Center, float _Radius ) => {
				IntegrateLuminance( m_Calibration.m_Reflectance02, _Center, _Radius );
			} );
		}

		private void buttonCalibrate10_Click( object sender, EventArgs e )
		{
			outputPanel.StartCalibrationTargetPicking( ( PointF _Center, float _Radius ) => {
				IntegrateLuminance( m_Calibration.m_Reflectance10, _Center, _Radius );
			} );
		}

		private void buttonCalibrate20_Click( object sender, EventArgs e )
		{
			outputPanel.StartCalibrationTargetPicking( ( PointF _Center, float _Radius ) => {
				IntegrateLuminance( m_Calibration.m_Reflectance20, _Center, _Radius );
			} );
		}

		private void buttonCalibrate50_Click( object sender, EventArgs e )
		{
			outputPanel.StartCalibrationTargetPicking( ( PointF _Center, float _Radius ) => {
				IntegrateLuminance( m_Calibration.m_Reflectance50, _Center, _Radius );
			} );
		}

		private void buttonCalibrate75_Click( object sender, EventArgs e )
		{
			outputPanel.StartCalibrationTargetPicking( ( PointF _Center, float _Radius ) => {
				IntegrateLuminance( m_Calibration.m_Reflectance75, _Center, _Radius );
			} );
		}

		private void buttonCalibrate99_Click( object sender, EventArgs e )
		{
			outputPanel.StartCalibrationTargetPicking( ( PointF _Center, float _Radius ) => {
				IntegrateLuminance( m_Calibration.m_Reflectance99, _Center, _Radius );
			} );
		}

		private void checkBoxGraphLagrange_CheckedChanged( object sender, EventArgs e )
		{
			graphPanel.UseLagrange = checkBoxGraphLagrange.Checked;
		}

		private void buttonLoadCalibration_Click( object sender, EventArgs e )
		{
 			string	OldFileName = GetRegKey( "LastCalibrationFilename", m_ApplicationPath );
			openFileDialogCalibration.InitialDirectory = System.IO.Path.GetDirectoryName( OldFileName );
			openFileDialogCalibration.FileName = System.IO.Path.GetFileName( OldFileName );

			if ( openFileDialogCalibration.ShowDialog( this ) != DialogResult.OK )
 				return;

			SetRegKey( "LastCalibrationFilename", openFileDialogCalibration.FileName );

			try
			{
				CameraCalibration	NewCalibration = new CameraCalibration();
				NewCalibration.Load( new System.IO.FileInfo( openFileDialogCalibration.FileName ) );
				m_Calibration = NewCalibration;
				UpdateUIFromCalibration();
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while loading calibration file:\r\n\r\n", _e );
			}
		}

		private void buttonSaveCalibration_Click( object sender, EventArgs e )
		{
			if ( m_BitmapXYZ == null )
			{	// No iage loaded you moron!
				MessageBox( "Can't save calibration as no image is currently loaded!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
				return;
			}

			string	OldFileName = GetRegKey( "LastCalibrationFilename", m_ApplicationPath );
			saveFileDialogCalibration.InitialDirectory = System.IO.Path.GetDirectoryName( OldFileName );
			saveFileDialogCalibration.FileName = System.IO.Path.GetFileNameWithoutExtension( m_Calibration.m_ReferenceImageName ) + ".xml";

			if ( saveFileDialogCalibration.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "LastCalibrationFilename", saveFileDialogCalibration.FileName );

			try
			{
				// Save & update UI
				m_Calibration.Save( new System.IO.FileInfo( saveFileDialogCalibration.FileName ) );
				UpdateUIFromCalibration();
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while loading calibration file:", _e );
			}
		}

		private void buttonSetupDatabaseFolder_Click( object sender, EventArgs e )
		{
			string	OldPath = System.IO.Path.GetDirectoryName( GetRegKey( "LastCalibrationDatabasePath", m_ApplicationPath ) );
			folderBrowserDialogDatabaseLocation.SelectedPath = OldPath;
			if ( folderBrowserDialogDatabaseLocation.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "LastCalibrationDatabasePath", folderBrowserDialogDatabaseLocation.SelectedPath );

			// Setup the path again, this will rebuild the database...
			m_CalibrationDatabase.DatabasePath = new System.IO.DirectoryInfo( folderBrowserDialogDatabaseLocation.SelectedPath );
		}

		private void buttonReCalibrate_Click( object sender, EventArgs e )
		{
			// Re-Calibrate every available probe
			bool	ProbesHaveSaturatedValues = false;
			bool	ProbesMissMeasurementDisc = false;
			foreach ( CameraCalibration.Probe P in m_Calibration.m_Reflectances )
				if ( P.m_MeasurementDiscIsAvailable )
				{
					float	BlackValues, SaturatedValues;
					float	MeasuredValue = IntegrateLuminance( P.m_MeasurementCenterX, P.m_MeasurementCenterY, P.m_MeasurementRadius, out BlackValues, out SaturatedValues );
					if ( BlackValues > BLACK_VALUES_TOLERANCE || SaturatedValues > SATURATED_VALUES_TOLERANCE )
					{	// Disable that probe as too many values are black or saturated
						P.m_IsAvailable = false;
						P.m_LuminanceMeasured = 0.0f;
						ProbesHaveSaturatedValues = true;
						continue;
					}

					// We have a valid measurement!
					P.m_IsAvailable = true;
					P.m_LuminanceMeasured = MeasuredValue;
				}
				else
				{	// Disable probe as it has no measurement info
					P.m_IsAvailable = false;
					P.m_LuminanceMeasured = 0.0f;
					ProbesMissMeasurementDisc = true;
				}

			// We now used the current image as reference for this calibration so commit its data
			CommitImageToCurrentCalibration();
			UpdateUIFromCalibration();

			if ( ProbesHaveSaturatedValues )
				MessageBox( "Some probes have been disabled because the luminance measurement returned too many saturated or black values!", MessageBoxButtons.OK, MessageBoxIcon.Warning );
			if ( ProbesMissMeasurementDisc )
				MessageBox( "Some probes can't be measured because they're missing the sampling disc information!\r\nClick the \"Calibrate\" button to place the disk and calibrate manually.", MessageBoxButtons.OK, MessageBoxIcon.Warning );
		}

		#endregion

		#endregion
	}
}


#region Source code from http://cybercom.net/~dcoffin/dcraw/decompress.c
// 	/*
//    Simple reference decompresser for Canon digital cameras.
//    Outputs raw 16-bit CCD data, no header, native byte order.
// 
//    $Revision: 1.12 $
//    $Date: 2004/08/06 00:08:01 $
// */
// 
// #include <stdio.h>
// #include <stdlib.h>
// #include <string.h>
// 
// typedef unsigned char uchar;
// 
// /* Global Variables */
// 
// FILE *ifp;
// short order;
// int height, width, table, lowbits;
// char name[64];
// 
// struct decode {
//   struct decode *branch[2];
//   int leaf;
// } first_decode[32], second_decode[512];
// 
// /*
//    Get a 2-byte integer, making no assumptions about CPU byte order.
//    Nor should we assume that the compiler evaluates left-to-right.
//  */
// short fget2 (FILE *f)
// {
//   register uchar a, b;
// 
//   a = fgetc(f);
//   b = fgetc(f);
//   if (order == 0x4d4d)		/* "MM" means big-endian */
//     return (a << 8) + b;
//   else				/* "II" means little-endian */
//     return a + (b << 8);
// }
// 
// /*
//    Same for a 4-byte integer.
//  */
// int fget4 (FILE *f)
// {
//   register uchar a, b, c, d;
// 
//   a = fgetc(f);
//   b = fgetc(f);
//   c = fgetc(f);
//   d = fgetc(f);
//   if (order == 0x4d4d)
//     return (a << 24) + (b << 16) + (c << 8) + d;
//   else
//     return a + (b << 8) + (c << 16) + (d << 24);
// }
// 
// /*
//    Parse the CIFF structure
//  */
// void parse (int offset, int length)
// {
//   int tboff, nrecs, i, type, len, roff, aoff, save;
// 
//   fseek (ifp, offset+length-4, SEEK_SET);
//   tboff = fget4(ifp) + offset;
//   fseek (ifp, tboff, SEEK_SET);
//   nrecs = fget2(ifp);
//   for (i = 0; i < nrecs; i++) {
//     type = fget2(ifp);
//     len  = fget4(ifp);
//     roff = fget4(ifp);
//     aoff = offset + roff;
//     save = ftell(ifp);
//     if (type == 0x080a) {		/* Get the camera name */
//       fseek (ifp, aoff, SEEK_SET);
//       while (fgetc(ifp));
//       fread (name, 64, 1, ifp);
//     }
//     if (type == 0x1031) {		/* Get the width and height */
//       fseek (ifp, aoff+2, SEEK_SET);
//       width  = fget2(ifp);
//       height = fget2(ifp);
//     }
//     if (type == 0x1835) {		/* Get the decoder table */
//       fseek (ifp, aoff, SEEK_SET);
//       table = fget4(ifp);
//     }
//     if (type >> 8 == 0x28 || type >> 8 == 0x30)	/* Get sub-tables */
//       parse (aoff, len);
//     fseek (ifp, save, SEEK_SET);
//   }
// }
// 
// /*
//    Return 0 if the image starts with compressed data,
//    1 if it starts with uncompressed low-order bits.
// 
//    In Canon compressed data, 0xff is always followed by 0x00.
//  */
// int canon_has_lowbits()
// {
//   uchar test[0x4000];
//   int ret=1, i;
// 
//   fseek (ifp, 0, SEEK_SET);
//   fread (test, 1, sizeof test, ifp);
//   for (i=540; i < sizeof test - 1; i++)
//     if (test[i] == 0xff) {
//       if (test[i+1]) return 1;
//       ret=0;
//     }
//   return ret;
// }
// 
// /*
//    Open a CRW file, identify which camera created it, and set
//    global variables accordingly.  Returns nonzero if an error occurs.
//  */
// int open_and_id(char *fname)
// {
//   char head[8];
//   int hlen;
// 
//   ifp = fopen(fname,"rb");
//   if (!ifp) {
//     perror(fname);
//     return 1;
//   }
//   order = fget2(ifp);
//   hlen  = fget4(ifp);
// 
//   fread (head, 1, 8, ifp);
//   if (memcmp(head,"HEAPCCDR",8) || (order != 0x4949 && order != 0x4d4d)) {
//     fprintf(stderr,"%s is not a Canon CRW file.\n",fname);
//     return 1;
//   }
// 
//   name[0] = 0;
//   table = -1;
//   fseek (ifp, 0, SEEK_END);
//   parse (hlen, ftell(ifp) - hlen);
//   lowbits = canon_has_lowbits();
// 
//   fprintf(stderr,"name = %s, width = %d, height = %d, table = %d, bpp = %d\n",
// 	name, width, height, table, 10+lowbits*2);
//   if (table < 0) {
//     fprintf(stderr,"Cannot decompress %s!!\n",fname);
//     return 1;
//   }
//   return 0;
// }
// 
// /*
//    A rough description of Canon's compression algorithm:
// 
// +  Each pixel outputs a 10-bit sample, from 0 to 1023.
// +  Split the data into blocks of 64 samples each.
// +  Subtract from each sample the value of the sample two positions
//    to the left, which has the same color filter.  From the two
//    leftmost samples in each row, subtract 512.
// +  For each nonzero sample, make a token consisting of two four-bit
//    numbers.  The low nibble is the number of bits required to
//    represent the sample, and the high nibble is the number of
//    zero samples preceding this sample.
// +  Output this token as a variable-length bitstring using
//    one of three tablesets.  Follow it with a fixed-length
//    bitstring containing the sample.
// 
//    The "first_decode" table is used for the first sample in each
//    block, and the "second_decode" table is used for the others.
//  */
// 
// /*
//    Construct a decode tree according the specification in *source.
//    The first 16 bytes specify how many codes should be 1-bit, 2-bit
//    3-bit, etc.  Bytes after that are the leaf values.
// 
//    For example, if the source is
// 
//     { 0,1,4,2,3,1,2,0,0,0,0,0,0,0,0,0,
//       0x04,0x03,0x05,0x06,0x02,0x07,0x01,0x08,0x09,0x00,0x0a,0x0b,0xff  },
// 
//    then the code is
// 
// 	00		0x04
// 	010		0x03
// 	011		0x05
// 	100		0x06
// 	101		0x02
// 	1100		0x07
// 	1101		0x01
// 	11100		0x08
// 	11101		0x09
// 	11110		0x00
// 	111110		0x0a
// 	1111110		0x0b
// 	1111111		0xff
//  */
// void make_decoder(struct decode *dest, const uchar *source, int level)
// {
//   static struct decode *free;	/* Next unused node */
//   static int leaf;		/* no. of leaves already added */
//   int i, next;
// 
//   if (level==0) {
//     free = dest;
//     leaf = 0;
//   }
//   free++;
// /*
//    At what level should the next leaf appear?
//  */
//   for (i=next=0; i <= leaf && next < 16; )
//     i += source[next++];
// 
//   if (i > leaf)
//     if (level < next) {		/* Are we there yet? */
//       dest->branch[0] = free;
//       make_decoder(free,source,level+1);
//       dest->branch[1] = free;
//       make_decoder(free,source,level+1);
//     } else
//       dest->leaf = source[16 + leaf++];
// }
// 
// void init_tables(unsigned table)
// {
//   static const uchar first_tree[3][29] = {
//     { 0,1,4,2,3,1,2,0,0,0,0,0,0,0,0,0,
//       0x04,0x03,0x05,0x06,0x02,0x07,0x01,0x08,0x09,0x00,0x0a,0x0b,0xff  },
// 
//     { 0,2,2,3,1,1,1,1,2,0,0,0,0,0,0,0,
//       0x03,0x02,0x04,0x01,0x05,0x00,0x06,0x07,0x09,0x08,0x0a,0x0b,0xff  },
// 
//     { 0,0,6,3,1,1,2,0,0,0,0,0,0,0,0,0,
//       0x06,0x05,0x07,0x04,0x08,0x03,0x09,0x02,0x00,0x0a,0x01,0x0b,0xff  },
//   };
// 
//   static const uchar second_tree[3][180] = {
//     { 0,2,2,2,1,4,2,1,2,5,1,1,0,0,0,139,
//       0x03,0x04,0x02,0x05,0x01,0x06,0x07,0x08,
//       0x12,0x13,0x11,0x14,0x09,0x15,0x22,0x00,0x21,0x16,0x0a,0xf0,
//       0x23,0x17,0x24,0x31,0x32,0x18,0x19,0x33,0x25,0x41,0x34,0x42,
//       0x35,0x51,0x36,0x37,0x38,0x29,0x79,0x26,0x1a,0x39,0x56,0x57,
//       0x28,0x27,0x52,0x55,0x58,0x43,0x76,0x59,0x77,0x54,0x61,0xf9,
//       0x71,0x78,0x75,0x96,0x97,0x49,0xb7,0x53,0xd7,0x74,0xb6,0x98,
//       0x47,0x48,0x95,0x69,0x99,0x91,0xfa,0xb8,0x68,0xb5,0xb9,0xd6,
//       0xf7,0xd8,0x67,0x46,0x45,0x94,0x89,0xf8,0x81,0xd5,0xf6,0xb4,
//       0x88,0xb1,0x2a,0x44,0x72,0xd9,0x87,0x66,0xd4,0xf5,0x3a,0xa7,
//       0x73,0xa9,0xa8,0x86,0x62,0xc7,0x65,0xc8,0xc9,0xa1,0xf4,0xd1,
//       0xe9,0x5a,0x92,0x85,0xa6,0xe7,0x93,0xe8,0xc1,0xc6,0x7a,0x64,
//       0xe1,0x4a,0x6a,0xe6,0xb3,0xf1,0xd3,0xa5,0x8a,0xb2,0x9a,0xba,
//       0x84,0xa4,0x63,0xe5,0xc5,0xf3,0xd2,0xc4,0x82,0xaa,0xda,0xe4,
//       0xf2,0xca,0x83,0xa3,0xa2,0xc3,0xea,0xc2,0xe2,0xe3,0xff,0xff  },
// 
//     { 0,2,2,1,4,1,4,1,3,3,1,0,0,0,0,140,
//       0x02,0x03,0x01,0x04,0x05,0x12,0x11,0x06,
//       0x13,0x07,0x08,0x14,0x22,0x09,0x21,0x00,0x23,0x15,0x31,0x32,
//       0x0a,0x16,0xf0,0x24,0x33,0x41,0x42,0x19,0x17,0x25,0x18,0x51,
//       0x34,0x43,0x52,0x29,0x35,0x61,0x39,0x71,0x62,0x36,0x53,0x26,
//       0x38,0x1a,0x37,0x81,0x27,0x91,0x79,0x55,0x45,0x28,0x72,0x59,
//       0xa1,0xb1,0x44,0x69,0x54,0x58,0xd1,0xfa,0x57,0xe1,0xf1,0xb9,
//       0x49,0x47,0x63,0x6a,0xf9,0x56,0x46,0xa8,0x2a,0x4a,0x78,0x99,
//       0x3a,0x75,0x74,0x86,0x65,0xc1,0x76,0xb6,0x96,0xd6,0x89,0x85,
//       0xc9,0xf5,0x95,0xb4,0xc7,0xf7,0x8a,0x97,0xb8,0x73,0xb7,0xd8,
//       0xd9,0x87,0xa7,0x7a,0x48,0x82,0x84,0xea,0xf4,0xa6,0xc5,0x5a,
//       0x94,0xa4,0xc6,0x92,0xc3,0x68,0xb5,0xc8,0xe4,0xe5,0xe6,0xe9,
//       0xa2,0xa3,0xe3,0xc2,0x66,0x67,0x93,0xaa,0xd4,0xd5,0xe7,0xf8,
//       0x88,0x9a,0xd7,0x77,0xc4,0x64,0xe2,0x98,0xa5,0xca,0xda,0xe8,
//       0xf3,0xf6,0xa9,0xb2,0xb3,0xf2,0xd2,0x83,0xba,0xd3,0xff,0xff  },
// 
//     { 0,0,6,2,1,3,3,2,5,1,2,2,8,10,0,117,
//       0x04,0x05,0x03,0x06,0x02,0x07,0x01,0x08,
//       0x09,0x12,0x13,0x14,0x11,0x15,0x0a,0x16,0x17,0xf0,0x00,0x22,
//       0x21,0x18,0x23,0x19,0x24,0x32,0x31,0x25,0x33,0x38,0x37,0x34,
//       0x35,0x36,0x39,0x79,0x57,0x58,0x59,0x28,0x56,0x78,0x27,0x41,
//       0x29,0x77,0x26,0x42,0x76,0x99,0x1a,0x55,0x98,0x97,0xf9,0x48,
//       0x54,0x96,0x89,0x47,0xb7,0x49,0xfa,0x75,0x68,0xb6,0x67,0x69,
//       0xb9,0xb8,0xd8,0x52,0xd7,0x88,0xb5,0x74,0x51,0x46,0xd9,0xf8,
//       0x3a,0xd6,0x87,0x45,0x7a,0x95,0xd5,0xf6,0x86,0xb4,0xa9,0x94,
//       0x53,0x2a,0xa8,0x43,0xf5,0xf7,0xd4,0x66,0xa7,0x5a,0x44,0x8a,
//       0xc9,0xe8,0xc8,0xe7,0x9a,0x6a,0x73,0x4a,0x61,0xc7,0xf4,0xc6,
//       0x65,0xe9,0x72,0xe6,0x71,0x91,0x93,0xa6,0xda,0x92,0x85,0x62,
//       0xf3,0xc5,0xb2,0xa4,0x84,0xba,0x64,0xa5,0xb3,0xd2,0x81,0xe5,
//       0xd3,0xaa,0xc4,0xca,0xf2,0xb1,0xe4,0xd1,0x83,0x63,0xea,0xc3,
//       0xe2,0x82,0xf1,0xa3,0xc2,0xa1,0xc1,0xe3,0xa2,0xe1,0xff,0xff  }
//   };
// 
//   if (table > 2) table = 2;
//   memset( first_decode, 0, sizeof first_decode);
//   memset(second_decode, 0, sizeof second_decode);
//   make_decoder( first_decode,  first_tree[table], 0);
//   make_decoder(second_decode, second_tree[table], 0);
// }
// 
// #if 0
// writebits (int val, int nbits)
// {
//   val <<= 32 - nbits;
//   while (nbits--) {
//     putchar(val & 0x80000000 ? '1':'0');
//     val <<= 1;
//   }
// }
// #endif
// 
// /*
//    getbits(-1) initializes the buffer
//    getbits(n) where 0 <= n <= 25 returns an n-bit integer
// */
// unsigned long getbits(int nbits)
// {
//   static unsigned long bitbuf=0, ret=0;
//   static int vbits=0;
//   unsigned char c;
// 
//   if (nbits == 0) return 0;
//   if (nbits == -1)
//     ret = bitbuf = vbits = 0;
//   else {
//     ret = bitbuf << (32 - vbits) >> (32 - nbits);
//     vbits -= nbits;
//   }
//   while (vbits < 25) {
//     c=fgetc(ifp);
//     bitbuf = (bitbuf << 8) + c;
//     if (c == 0xff) fgetc(ifp);	/* always extra 00 after ff */
//     vbits += 8;
//   }
//   return ret;
// }
// 
// int main(int argc, char **argv)
// {
//   struct decode *decode, *dindex;
//   int i, j, leaf, len, diff, diffbuf[64], r, save;
//   int carry=0, column=0, base[2];
//   unsigned short outbuf[64];
//   uchar c;
// 
//   if (argc < 2) {
//     fprintf(stderr,"Usage:  %s file.crw\n",argv[0]);
//     exit(1);
//   }
//   if (open_and_id(argv[1]))
//     exit(1);
// 
//   init_tables(table);
// 
//   fseek (ifp, 540 + lowbits*height*width/4, SEEK_SET);
//   getbits(-1);			/* Prime the bit buffer */
// 
//   while (column < width * height) {
//     memset(diffbuf,0,sizeof diffbuf);
//     decode = first_decode;
//     for (i=0; i < 64; i++ ) {
// 
//       for (dindex=decode; dindex->branch[0]; )
// 			dindex = dindex->branch[getbits(1)];
//       leaf = dindex->leaf;
//       decode = second_decode;
// 
//       if (leaf == 0 && i) break;
//       if (leaf == 0xff) continue;
//       i  += leaf >> 4;
//       len = leaf & 15;
//       if (len == 0) continue;
//       diff = getbits(len);
//       if ((diff & (1 << (len-1))) == 0)
// 	diff -= (1 << len) - 1;
//       if (i < 64) diffbuf[i] = diff;
//     }
//     diffbuf[0] += carry;
//     carry = diffbuf[0];
//     for (i=0; i < 64; i++ ) {
//       if (column++ % width == 0)
// 	base[0] = base[1] = 512;
//       outbuf[i] = ( base[i & 1] += diffbuf[i] );
//     }
//     if (lowbits) {
//       save = ftell(ifp);
//       fseek (ifp, (column-64)/4 + 26, SEEK_SET);
//       for (i=j=0; j < 64/4; j++ ) {
// 	c = fgetc(ifp);
// 	for (r = 0; r < 8; r += 2)
// 	  outbuf[i++] = (outbuf[i] << 2) + ((c >> r) & 3);
//       }
//       fseek (ifp, save, SEEK_SET);
//     }
//     fwrite(outbuf,2,64,stdout);
//   }
//   return 0;
// }

#endregion

#region Source code from http://cybercom.net/~dcoffin/dcraw/dcraw.c

// unsigned CLASS getbithuff (int nbits, ushort *huff)
// {
//   static unsigned bitbuf=0;
//   static int vbits=0, reset=0;
//   unsigned c;
// 
//   if (nbits > 25) return 0;
//   if (nbits < 0)
//     return bitbuf = vbits = reset = 0;
//   if (nbits == 0 || vbits < 0) return 0;
//   while (!reset && vbits < nbits && (c = fgetc(ifp)) != EOF &&
//     !(reset = zero_after_ff && c == 0xff && fgetc(ifp))) {
//     bitbuf = (bitbuf << 8) + (uchar) c;
//     vbits += 8;
//   }
//   c = bitbuf << (32-vbits) >> (32-nbits);
//   if (huff) {
//     vbits -= huff[c] >> 8;
//     c = (uchar) huff[c];
//   } else
//     vbits -= nbits;
//   if (vbits < 0) derror();
//   return c;
// }
// 
// #define getbits(n) getbithuff(n,0)
// #define gethuff(h) getbithuff(*h,h+1)
// 
// /*
//    Construct a decode tree according the specification in *source.
//    The first 16 bytes specify how many codes should be 1-bit, 2-bit
//    3-bit, etc.  Bytes after that are the leaf values.
// 
//    For example, if the source is
// 
//     { 0,1,4,2,3,1,2,0,0,0,0,0,0,0,0,0,
//       0x04,0x03,0x05,0x06,0x02,0x07,0x01,0x08,0x09,0x00,0x0a,0x0b,0xff  },
// 
//    then the code is
// 
// 	00		0x04
// 	010		0x03
// 	011		0x05
// 	100		0x06
// 	101		0x02
// 	1100		0x07
// 	1101		0x01
// 	11100		0x08
// 	11101		0x09
// 	11110		0x00
// 	111110		0x0a
// 	1111110		0x0b
// 	1111111		0xff
//  */
// ushort * CLASS make_decoder_ref (const uchar **source)
// {
//   int max, len, h, i, j;
//   const uchar *count;
//   ushort *huff;
// 
//   count = (*source += 16) - 17;
//   for (max=16; max && !count[max]; max--);
//   huff = (ushort *) calloc (1 + (1 << max), sizeof *huff);
//   merror (huff, "make_decoder()");
//   huff[0] = max;
//   for (h=len=1; len <= max; len++)
//     for (i=0; i < count[len]; i++, ++*source)
//       for (j=0; j < 1 << (max-len); j++)
// 	if (h <= 1 << max)
// 	  huff[h++] = len << 8 | **source;
//   return huff;
// }
// 
// ushort * CLASS make_decoder (const uchar *source)
// {
//   return make_decoder_ref (&source);
// }
// 
// void CLASS crw_init_tables (unsigned table, ushort *huff[2])
// {
//   static const uchar first_tree[3][29] = {
//     { 0,1,4,2,3,1,2,0,0,0,0,0,0,0,0,0,
//       0x04,0x03,0x05,0x06,0x02,0x07,0x01,0x08,0x09,0x00,0x0a,0x0b,0xff  },
//     { 0,2,2,3,1,1,1,1,2,0,0,0,0,0,0,0,
//       0x03,0x02,0x04,0x01,0x05,0x00,0x06,0x07,0x09,0x08,0x0a,0x0b,0xff  },
//     { 0,0,6,3,1,1,2,0,0,0,0,0,0,0,0,0,
//       0x06,0x05,0x07,0x04,0x08,0x03,0x09,0x02,0x00,0x0a,0x01,0x0b,0xff  },
//   };
//   static const uchar second_tree[3][180] = {
//     { 0,2,2,2,1,4,2,1,2,5,1,1,0,0,0,139,
//       0x03,0x04,0x02,0x05,0x01,0x06,0x07,0x08,
//       0x12,0x13,0x11,0x14,0x09,0x15,0x22,0x00,0x21,0x16,0x0a,0xf0,
//       0x23,0x17,0x24,0x31,0x32,0x18,0x19,0x33,0x25,0x41,0x34,0x42,
//       0x35,0x51,0x36,0x37,0x38,0x29,0x79,0x26,0x1a,0x39,0x56,0x57,
//       0x28,0x27,0x52,0x55,0x58,0x43,0x76,0x59,0x77,0x54,0x61,0xf9,
//       0x71,0x78,0x75,0x96,0x97,0x49,0xb7,0x53,0xd7,0x74,0xb6,0x98,
//       0x47,0x48,0x95,0x69,0x99,0x91,0xfa,0xb8,0x68,0xb5,0xb9,0xd6,
//       0xf7,0xd8,0x67,0x46,0x45,0x94,0x89,0xf8,0x81,0xd5,0xf6,0xb4,
//       0x88,0xb1,0x2a,0x44,0x72,0xd9,0x87,0x66,0xd4,0xf5,0x3a,0xa7,
//       0x73,0xa9,0xa8,0x86,0x62,0xc7,0x65,0xc8,0xc9,0xa1,0xf4,0xd1,
//       0xe9,0x5a,0x92,0x85,0xa6,0xe7,0x93,0xe8,0xc1,0xc6,0x7a,0x64,
//       0xe1,0x4a,0x6a,0xe6,0xb3,0xf1,0xd3,0xa5,0x8a,0xb2,0x9a,0xba,
//       0x84,0xa4,0x63,0xe5,0xc5,0xf3,0xd2,0xc4,0x82,0xaa,0xda,0xe4,
//       0xf2,0xca,0x83,0xa3,0xa2,0xc3,0xea,0xc2,0xe2,0xe3,0xff,0xff  },
//     { 0,2,2,1,4,1,4,1,3,3,1,0,0,0,0,140,
//       0x02,0x03,0x01,0x04,0x05,0x12,0x11,0x06,
//       0x13,0x07,0x08,0x14,0x22,0x09,0x21,0x00,0x23,0x15,0x31,0x32,
//       0x0a,0x16,0xf0,0x24,0x33,0x41,0x42,0x19,0x17,0x25,0x18,0x51,
//       0x34,0x43,0x52,0x29,0x35,0x61,0x39,0x71,0x62,0x36,0x53,0x26,
//       0x38,0x1a,0x37,0x81,0x27,0x91,0x79,0x55,0x45,0x28,0x72,0x59,
//       0xa1,0xb1,0x44,0x69,0x54,0x58,0xd1,0xfa,0x57,0xe1,0xf1,0xb9,
//       0x49,0x47,0x63,0x6a,0xf9,0x56,0x46,0xa8,0x2a,0x4a,0x78,0x99,
//       0x3a,0x75,0x74,0x86,0x65,0xc1,0x76,0xb6,0x96,0xd6,0x89,0x85,
//       0xc9,0xf5,0x95,0xb4,0xc7,0xf7,0x8a,0x97,0xb8,0x73,0xb7,0xd8,
//       0xd9,0x87,0xa7,0x7a,0x48,0x82,0x84,0xea,0xf4,0xa6,0xc5,0x5a,
//       0x94,0xa4,0xc6,0x92,0xc3,0x68,0xb5,0xc8,0xe4,0xe5,0xe6,0xe9,
//       0xa2,0xa3,0xe3,0xc2,0x66,0x67,0x93,0xaa,0xd4,0xd5,0xe7,0xf8,
//       0x88,0x9a,0xd7,0x77,0xc4,0x64,0xe2,0x98,0xa5,0xca,0xda,0xe8,
//       0xf3,0xf6,0xa9,0xb2,0xb3,0xf2,0xd2,0x83,0xba,0xd3,0xff,0xff  },
//     { 0,0,6,2,1,3,3,2,5,1,2,2,8,10,0,117,
//       0x04,0x05,0x03,0x06,0x02,0x07,0x01,0x08,
//       0x09,0x12,0x13,0x14,0x11,0x15,0x0a,0x16,0x17,0xf0,0x00,0x22,
//       0x21,0x18,0x23,0x19,0x24,0x32,0x31,0x25,0x33,0x38,0x37,0x34,
//       0x35,0x36,0x39,0x79,0x57,0x58,0x59,0x28,0x56,0x78,0x27,0x41,
//       0x29,0x77,0x26,0x42,0x76,0x99,0x1a,0x55,0x98,0x97,0xf9,0x48,
//       0x54,0x96,0x89,0x47,0xb7,0x49,0xfa,0x75,0x68,0xb6,0x67,0x69,
//       0xb9,0xb8,0xd8,0x52,0xd7,0x88,0xb5,0x74,0x51,0x46,0xd9,0xf8,
//       0x3a,0xd6,0x87,0x45,0x7a,0x95,0xd5,0xf6,0x86,0xb4,0xa9,0x94,
//       0x53,0x2a,0xa8,0x43,0xf5,0xf7,0xd4,0x66,0xa7,0x5a,0x44,0x8a,
//       0xc9,0xe8,0xc8,0xe7,0x9a,0x6a,0x73,0x4a,0x61,0xc7,0xf4,0xc6,
//       0x65,0xe9,0x72,0xe6,0x71,0x91,0x93,0xa6,0xda,0x92,0x85,0x62,
//       0xf3,0xc5,0xb2,0xa4,0x84,0xba,0x64,0xa5,0xb3,0xd2,0x81,0xe5,
//       0xd3,0xaa,0xc4,0xca,0xf2,0xb1,0xe4,0xd1,0x83,0x63,0xea,0xc3,
//       0xe2,0x82,0xf1,0xa3,0xc2,0xa1,0xc1,0xe3,0xa2,0xe1,0xff,0xff  }
//   };
//   if (table > 2) table = 2;
//   huff[0] = make_decoder ( first_tree[table]);
//   huff[1] = make_decoder (second_tree[table]);
// }
// 
// /*
//    Return 0 if the image starts with compressed data,
//    1 if it starts with uncompressed low-order bits.
// 
//    In Canon compressed data, 0xff is always followed by 0x00.
//  */
// int CLASS canon_has_lowbits()
// {
//   uchar test[0x4000];
//   int ret=1, i;
// 
//   fseek (ifp, 0, SEEK_SET);
//   fread (test, 1, sizeof test, ifp);
//   for (i=540; i < sizeof test - 1; i++)
//     if (test[i] == 0xff) {
//       if (test[i+1]) return 1;
//       ret=0;
//     }
//   return ret;
// }
// 
// void CLASS canon_load_raw()
// {
//   ushort *pixel, *prow, *huff[2];
//   int nblocks, lowbits, i, c, row, r, save, val;
//   int block, diffbuf[64], leaf, len, diff, carry=0, pnum=0, base[2];
// 
//   crw_init_tables (tiff_compress, huff);
//   lowbits = canon_has_lowbits();
//   if (!lowbits) maximum = 0x3ff;
//   fseek (ifp, 540 + lowbits*raw_height*raw_width/4, SEEK_SET);
//   zero_after_ff = 1;
//   getbits(-1);
//   for (row=0; row < raw_height; row+=8) {
//     pixel = raw_image + row*raw_width;
//     nblocks = MIN (8, raw_height-row) * raw_width >> 6;
//     for (block=0; block < nblocks; block++) {
//       memset (diffbuf, 0, sizeof diffbuf);
//       for (i=0; i < 64; i++ ) {
// 	leaf = gethuff(huff[i > 0]);
// 	if (leaf == 0 && i) break;
// 	if (leaf == 0xff) continue;
// 	i  += leaf >> 4;
// 	len = leaf & 15;
// 	if (len == 0) continue;
// 	diff = getbits(len);
// 	if ((diff & (1 << (len-1))) == 0)
// 	  diff -= (1 << len) - 1;
// 	if (i < 64) diffbuf[i] = diff;
//       }
//       diffbuf[0] += carry;
//       carry = diffbuf[0];
//       for (i=0; i < 64; i++ ) {
// 	if (pnum++ % raw_width == 0)
// 	  base[0] = base[1] = 512;
// 	if ((pixel[(block << 6) + i] = base[i & 1] += diffbuf[i]) >> 10)
// 	  derror();
//       }
//     }
//     if (lowbits) {
//       save = ftell(ifp);
//       fseek (ifp, 26 + row*raw_width/4, SEEK_SET);
//       for (prow=pixel, i=0; i < raw_width*2; i++) {
// 	c = fgetc(ifp);
// 	for (r=0; r < 8; r+=2, prow++) {
// 	  val = (*prow << 2) + ((c >> r) & 3);
// 	  if (raw_width == 2672 && val < 512) val += 2;
// 	  *prow = val;
// 	}
//       }
//       fseek (ifp, save, SEEK_SET);
//     }
//   }
//   FORC(2) free (huff[c]);
// }

#endregion
