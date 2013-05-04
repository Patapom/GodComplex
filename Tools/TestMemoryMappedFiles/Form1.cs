//////////////////////////////////////////////////////////////////////////
// This is a test to drive the variables of my Volumetric effect in GodComplex
// This technique of MemoryMappedFiles could easily be used to plug my old Sequencor back in action, driving a C++ program
//////////////////////////////////////////////////////////////////////////
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Reflection;

namespace TestMemoryMappedFiles
{
	public partial class Form1 : Form
	{
		[StructLayout( LayoutKind.Sequential, Pack=4)]
		public struct ParametersBlock
		{
			int	Checksum;

			// Atmosphere Params
			public float	SunTheta;
			public float	SunPhi;
			public float	SunIntensity;
			public float	AirAmount;
			public float	FogScattering;
			public float	FogExtinction;
			public float	AirReferenceAltitudeKm;
			public float	FogReferenceAltitudeKm;
			public float	FogAnisotropy;
			public float	AverageGroundReflectance;
			public float	GodraysStrength;
			// TODO: Add scattering/extinction ratio?

			// Volumetrics Params
			public float	CloudBaseAltitude;
			public float	CloudThickness;
			public float	CloudExtinction;
			public float	CloudScattering;
			public float	CloudAnisotropyIso;
			public float	CloudAnisotropyForward;
			public float	CloudShadowStrength;

			public float	CloudIsotropicScattering;	// Sigma_s for isotropic lighting
			public float	CloudIsoSkyRadianceFactor;
			public float	CloudIsoSunRadianceFactor;
			public float	CloudIsoTerrainReflectanceFactor;

			// Noise Params
				// Low frequency noise
			public float	NoiseLoFrequency;			// Horizontal frequency
			public float	NoiseLoVerticalLooping;		// Vertical frequency in amount of noise pixels
			public float	NoiseLoAnimSpeed;			// Animation speed
				// High frequency noise
			public float	NoiseHiFrequency;
			public float	NoiseHiOffset;				// Second noise is added to first noise using NoiseHiStrength * (HiFreqNoise + NoiseHiOffset)
			public float	NoiseHiStrength;
			public float	NoiseHiAnimSpeed;
				// Combined noise params
			public float	NoiseOffsetBottom;			// The noise offset to add when at the bottom altitude in the cloud
			public float	NoiseOffsetMiddle;			// The noise offset to add when at the middle altitude in the cloud
			public float	NoiseOffsetTop;				// The noise offset to add when at the top altitude in the cloud
			public float	NoiseContrast;				// Final noise value is Noise' = pow( Contrast*(Noise+Offset), Gamma )
			public float	NoiseGamma;
				// Final shaping params
			public float	NoiseShapingPower;			// Final noise value is shaped (multiplied) by pow( 1-abs(2*y-1), NoiseShapingPower ) to avoid flat plateaus at top or bottom
				
			// Terrain Params
			public float	TerrainHeight;
			public float	TerrainAlbedoMultiplier;
			public float	TerrainCloudShadowStrength;

			public void	SetChecksum( int _Checksum )	{ Checksum = _Checksum; }
		}

		protected MemoryMappedFile	m_MMF = null;
		protected MemoryMappedViewAccessor	m_View = null;

		protected int				m_StructSize = 0;
		protected ParametersBlock	m_Instance;

		public Form1()
		{
			InitializeComponent();

			m_Instance = new ParametersBlock() {
				SunIntensity = 100.0f,				// Should never change!
				AverageGroundReflectance = 0.1f,	// Should never change!
			};
			m_StructSize = System.Runtime.InteropServices.Marshal.SizeOf(m_Instance);
			InitFromSliders();

			m_MMF = MemoryMappedFile.CreateOrOpen( @"BisouTest", m_StructSize, MemoryMappedFileAccess.ReadWrite );
			m_View = m_MMF.CreateViewAccessor( 0, m_StructSize, MemoryMappedFileAccess.ReadWrite );

			UpdateMMF();
		}

		protected override void OnFormClosing( FormClosingEventArgs e )
		{
			base.OnFormClosing( e );

			m_View.Dispose();
			m_MMF.Dispose();
		}

		private void	UpdateMMF()
		{
			// Update checksum
			FieldInfo[]	Fields = m_Instance.GetType().GetFields( BindingFlags.Public | BindingFlags.Instance );

			int	Checksum = 0;
			foreach ( FieldInfo Field in Fields )
			{
				object	Value = Field.GetValue( m_Instance );
				int		Hash = Value.GetHashCode();
				Checksum ^= Hash;
			}
			m_Instance.SetChecksum( Checksum );

			if ( m_View == null )
				return;	// Not initialized yet...

			// Write the new content...
			m_View.Write( 0, ref m_Instance );
			m_View.Flush();
		}

		/// <summary>
		/// Initializes the struct from the sliders' current values
		/// </summary>
		private void	InitFromSliders()
		{
			FieldInfo[]	Fields = GetType().GetFields( BindingFlags.Instance | BindingFlags.NonPublic );
			foreach ( FieldInfo Field in Fields )
				if ( Field.FieldType == typeof(Nuaj.Cirrus.Utility.FloatTrackbarControl) )
				{
					Nuaj.Cirrus.Utility.FloatTrackbarControl	Slider = Field.GetValue( this ) as Nuaj.Cirrus.Utility.FloatTrackbarControl;
					Slider.SimulateValueChange();	// This should trigger a change which will in turn write the slider's value into the struct's field...
				}
		}

		#region EVENT HANDLERS

		#region ATMOSPHERE

		private void floatTrackbarControlSunTheta_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.SunTheta = 0.01745329251994329576923690768489f * _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlSunAzimuth_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.SunPhi = 0.01745329251994329576923690768489f * _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlAirAmount_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.AirAmount = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlFogAmount_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.FogScattering = _Sender.Value * 0.004f;
			m_Instance.FogExtinction = m_Instance.FogScattering / 0.9f;	// Hardcoded for the moment... Should we need to make it a parameter too?
			UpdateMMF();
		}

		private void floatTrackbarControlAirRefAltitude_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.AirReferenceAltitudeKm = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlFogRefAltitude_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.FogReferenceAltitudeKm = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlFogAnisotropy_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.FogAnisotropy = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlGodraysStrength_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.GodraysStrength = _Sender.Value;
			UpdateMMF();
		}

		#endregion

		#region TERRAIN

		private void floatTrackbarControlTerrainHeight_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.TerrainHeight = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlAlbedoMultiplier_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.TerrainAlbedoMultiplier = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlTerrainShadowStrength_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.TerrainCloudShadowStrength = _Sender.Value;
			UpdateMMF();
		}

		#endregion

		#region CLOUDS

		#region Location & Lighting

		private void floatTrackbarControlCloudAltitude_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.CloudBaseAltitude = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlCloudThickness_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.CloudThickness = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlCloudExtinction_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.CloudExtinction = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlCloudScatteringRatio_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.CloudScattering = floatTrackbarControlCloudExtinction.Value * _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlCloudPhaseIso_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.CloudAnisotropyIso = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlCloudPhaseForward_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.CloudAnisotropyForward = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlCloudShadowStrength_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.CloudShadowStrength = _Sender.Value;
			UpdateMMF();
		}

		#endregion

		#region Isotropic Lighting

		private void floatTrackbarControlIsotropicScattering_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.CloudIsotropicScattering = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlIsotropicScatteringSkyFactor_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.CloudIsoSkyRadianceFactor = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlIsotropicScatteringSunFactor_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.CloudIsoSunRadianceFactor = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlIsotropicScatteringTerrainFactor_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.CloudIsoTerrainReflectanceFactor = _Sender.Value;
			UpdateMMF();
		}

		#endregion

		#region Noise

		private void floatTrackbarControlCloudLowFrequency_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.NoiseLoFrequency = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlCloudVerticalLooping_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.NoiseLoVerticalLooping = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlCloudLowAnimSpeed_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.NoiseLoAnimSpeed = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlCloudHiFrequency_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.NoiseHiFrequency = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlCloudHiOffset_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.NoiseHiOffset = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlCloudHiFactor_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.NoiseHiStrength = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlCloudHiAnimSpeed_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.NoiseHiAnimSpeed = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlNoiseOffsetTop_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.NoiseOffsetTop = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlNoiseOffsetMiddle_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.NoiseOffsetMiddle = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlNoiseOffsetBottom_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.NoiseOffsetBottom = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlNoiseContrast_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.NoiseContrast = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlNoiseGamma_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.NoiseGamma = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlNoiseShapingPower_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.NoiseShapingPower = (float) Math.Pow( 10.0, _Sender.Value );
			UpdateMMF();
		}

		#endregion

		#endregion

		#endregion
	}
}
