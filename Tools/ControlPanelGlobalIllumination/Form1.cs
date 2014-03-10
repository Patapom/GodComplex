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

using System.IO;
using System.Xml;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Reflection;

namespace ControlPanelGlobalIllumination
{
	public partial class Form1 : Form
	{
		[StructLayout( LayoutKind.Sequential, Pack=4 )]
		public struct ParametersBlock
		{
			int	Checksum;

			// Atmosphere Params
			public int		EnableSun;
			public float	SunTheta;
			public float	SunPhi;
			public float	SunIntensity;

			public int		EnableSky;
			public float	SkyIntensity;
			public float	SkyColorR;
			public float	SkyColorG;
			public float	SkyColorB;

			// Dynamic lights params
			public int		EnablePointLight;
			public int		AnimatePointLight;
			public float	PointLightIntensity;
			public float	PointLightColorR;
			public float	PointLightColorG;
			public float	PointLightColorB;

			// Static lighting params
			public int		EnableStaticLighting;

			// Emissive params
			public int		EnableEmissiveMaterials;
			public float	EmissiveIntensity;
			public float	EmissiveColorR;
			public float	EmissiveColorG;
			public float	EmissiveColorB;

			// Bounce params
			public float	BounceFactorSun;
			public float	BounceFactorSky;
			public float	BounceFactorPoint;
			public float	BounceFactorStaticLights;
			public float	BounceFactorEmissive;

			// Neighors
			public int		EnableNeighborsRedistribution;
			public float	NeighborProbesContributionBoost;

			// Debug
			public int		ShowDebugProbes;
			public int		ShowDebugProbesNetwork;
			public float	DebugProbesIntensity;

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
// 				SunIntensity = 100.0f,				// Should never change!
// 				AverageGroundReflectance = 0.1f,	// Should never change!
			};
			m_StructSize = System.Runtime.InteropServices.Marshal.SizeOf(m_Instance);
			InitFromUI();

			m_MMF = MemoryMappedFile.CreateOrOpen( @"GlobalIllumination", m_StructSize, MemoryMappedFileAccess.ReadWrite );
			m_View = m_MMF.CreateViewAccessor( 0, m_StructSize, MemoryMappedFileAccess.ReadWrite );
			UpdateMMF();
		}

		protected override void OnFormClosing( FormClosingEventArgs e )
		{
			base.OnFormClosing( e );

			m_View.Dispose();
			m_MMF.Dispose();
		}

		private bool	m_bInternalUpdate = false;
		private void	UpdateMMF()
		{
			if ( m_bInternalUpdate )
				return;

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

		public void		UpdateFromParams()
		{
			m_bInternalUpdate = true;

			// Atmosphere params
			checkBoxEnableSun.Checked = m_Instance.EnableSun != 0;
			floatTrackbarControlSunTheta.Value = m_Instance.SunTheta / 0.01745329251994329576923690768489f;
			floatTrackbarControlSunAzimuth.Value = m_Instance.SunPhi / 0.01745329251994329576923690768489f;
			floatTrackbarControlSunIntensity.Value = m_Instance.SunIntensity;
			floatTrackbarControlSkyIntensity.Value = m_Instance.SkyIntensity;
			checkBoxEnableSky.Checked = m_Instance.EnableSky != 0;
			panelSkyColor.BackColor = Color.FromArgb( (int) (255 * m_Instance.SkyColorR), (int) (255 * m_Instance.SkyColorG), (int) (255 * m_Instance.SkyColorB) );

			// Dynamic
			checkBoxEnableDynamicPointLight.Checked = m_Instance.EnablePointLight != 0;
			checkBoxAnimatePointLight.Checked = m_Instance.AnimatePointLight != 0;
			floatTrackbarControlPointLightIntensity.Value = m_Instance.PointLightIntensity;
			panelLightColor.BackColor = Color.FromArgb( (int) (255 * m_Instance.PointLightColorR), (int) (255 * m_Instance.PointLightColorG), (int) (255 * m_Instance.PointLightColorB) );

			// Emissive
			checkBoxEnableStaticLighting.Checked = m_Instance.EnableStaticLighting != 0;
			checkBoxEnableEmissive.Checked = m_Instance.EnableEmissiveMaterials != 0;
			floatTrackbarControlEmissiveIntensity.Value = m_Instance.EmissiveIntensity;
			panelEmissiveColor.BackColor = Color.FromArgb( (int) (255 * m_Instance.EmissiveColorR), (int) (255 * m_Instance.EmissiveColorG), (int) (255 * m_Instance.EmissiveColorB) );

			// Bounce
			floatTrackbarControlSunBounceFactor.Value = m_Instance.BounceFactorSun;
			floatTrackbarControlSkyBounceFactor.Value = m_Instance.BounceFactorSky;
			floatTrackbarControlPointLightBounceFactor.Value = m_Instance.BounceFactorPoint;
			floatTrackbarControlStaticLightsBounceFactor.Value = m_Instance.BounceFactorStaticLights;
			floatTrackbarControlEmissiveLightsBounceFactor.Value = m_Instance.BounceFactorEmissive;

			// Neighborhood
			checkBoxEnableRedistribution.Checked = m_Instance.EnableNeighborsRedistribution != 0;
			floatTrackbarControlNeighborProbesContribution.Value = m_Instance.NeighborProbesContributionBoost;

			// Refresh block
			m_bInternalUpdate = false;
			UpdateMMF();
		}

		/// <summary>
		/// Initializes the struct from the sliders' current values
		/// </summary>
		private void	InitFromUI()
		{
			FieldInfo[]	Fields = GetType().GetFields( BindingFlags.Instance | BindingFlags.NonPublic );
			foreach ( FieldInfo Field in Fields )
				if ( Field.FieldType == typeof(Nuaj.Cirrus.Utility.FloatTrackbarControl) )
				{
					Nuaj.Cirrus.Utility.FloatTrackbarControl	Slider = Field.GetValue( this ) as Nuaj.Cirrus.Utility.FloatTrackbarControl;
					Slider.SimulateValueChange();	// This should trigger a change which will in turn write the slider's value into the struct's field...
				}
				else if ( Field.FieldType == typeof(CheckBox) )
				{
					CheckBox	checkBox = Field.GetValue( this ) as CheckBox;
					checkBox.Checked = !checkBox.Checked;
					checkBox.Checked = !checkBox.Checked;
				}

			// Update colors
			m_Instance.SkyColorR = panelSkyColor.BackColor.R / 255.0f;
			m_Instance.SkyColorG = panelSkyColor.BackColor.G / 255.0f;
			m_Instance.SkyColorB = panelSkyColor.BackColor.B / 255.0f;

			m_Instance.PointLightColorR = panelLightColor.BackColor.R / 255.0f;
			m_Instance.PointLightColorG = panelLightColor.BackColor.G / 255.0f;
			m_Instance.PointLightColorB = panelLightColor.BackColor.B / 255.0f;

			m_Instance.EmissiveColorR = panelEmissiveColor.BackColor.R / 255.0f;
			m_Instance.EmissiveColorG = panelEmissiveColor.BackColor.G / 255.0f;
			m_Instance.EmissiveColorB = panelEmissiveColor.BackColor.B / 255.0f;
		}

		#region EVENT HANDLERS

		private void buttonLoadPreset_Click( object sender, EventArgs e )
		{
			if ( openFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;

			try
			{
				XmlDocument	Doc = new XmlDocument();
				Doc.Load( openFileDialog.FileName );

				XmlElement	Root = Doc["Root"];
				if ( Root == null )
					throw new Exception( "Failed to find root element!" );

				FieldInfo[]	Fields = typeof(ParametersBlock).GetFields( BindingFlags.Instance | BindingFlags.Public );
				Dictionary<string,FieldInfo>	Name2Field = new Dictionary<string,FieldInfo>();
				foreach ( FieldInfo Field in Fields )
					Name2Field.Add( Field.Name, Field );

				object	BoxedInstance = (object) m_Instance;	// Struct needs to be boxed to be referenced and not passed by value...

				string	Warnings = "";
				foreach ( XmlNode ChildNode in Root.ChildNodes )
					if ( ChildNode is XmlElement )
					{
						XmlElement	FieldElement = ChildNode as XmlElement;
						if ( !Name2Field.ContainsKey( FieldElement.Name ) )
						{	// Unknown field...
							Warnings += "	Unrecognized field \"" + FieldElement.Name + "\".\r\n";
							continue;
						}

						FieldInfo	Field = Name2Field[FieldElement.Name];
						string		Value = FieldElement.GetAttribute( "Value" );

						if ( Field.FieldType == typeof(float) )
							Field.SetValue( BoxedInstance, float.Parse( Value ) );
						else if ( Field.FieldType == typeof(int) )
							Field.SetValue( BoxedInstance, int.Parse( Value ) );
						else if ( Field.FieldType == typeof(bool) )
							Field.SetValue( BoxedInstance, bool.Parse( Value ) );
						else
						{
							Warnings += "	Unsupported field type \"" + Field.FieldType + "\" for field \"" + Field.Name + "\" (Value = " + Value + ")\r\n";
							continue;
						}
					}

				// Unbox
				m_Instance = (ParametersBlock) BoxedInstance;

				// Update the sliders
				UpdateFromParams();

				if ( Warnings == "" )
					MessageBox.Show( this, "Success!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information );
				else
					MessageBox.Show( this, "Success with warnings:\r\n\r\n" + Warnings, "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning );
			}
			catch ( Exception _e )
			{
				MessageBox.Show( this, "An error occurred while loading preset file: " + _e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		private void buttonSavePreset_Click( object sender, EventArgs e )
		{
			if ( saveFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;

			XmlDocument	Doc = new XmlDocument();
			XmlElement	Root = Doc.CreateElement( "Root" );
			Doc.AppendChild( Root );

			FieldInfo[]	Fields = typeof(ParametersBlock).GetFields( BindingFlags.Instance | BindingFlags.Public );
			foreach ( FieldInfo Field in Fields )
			{
				XmlElement	FieldElement = Doc.CreateElement( Field.Name );
				Root.AppendChild( FieldElement );
				FieldElement.SetAttribute( "Value", Field.GetValue( m_Instance ).ToString() );
			}

			Doc.Save( saveFileDialog.FileName );

			MessageBox.Show( this, "Success!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information );
		}

		#region ATMOSPHERE

		private void checkBoxEnableSun_CheckedChanged( object sender, EventArgs e )
		{
			m_Instance.EnableSun = (sender as CheckBox).Checked ? 1 : 0;
			UpdateMMF();
		}

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

		private void floatTrackbarControlSunIntensity_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.SunIntensity = _Sender.Value;
			UpdateMMF();
		}

		private void checkBoxEnableSky_CheckedChanged( object sender, EventArgs e )
		{
			m_Instance.EnableSky = (sender as CheckBox).Checked ? 1 : 0;
			UpdateMMF();
		}

		private void floatTrackbarControlSkyIntensity_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.SkyIntensity = _Sender.Value;
			UpdateMMF();
		}

		private void panelSkyColor_Click( object sender, EventArgs e )
		{
			colorDialog.Color = panelSkyColor.BackColor;
			if ( colorDialog.ShowDialog( this ) != DialogResult.OK )
				return;

			panelSkyColor.BackColor = colorDialog.Color;
			m_Instance.SkyColorR = colorDialog.Color.R / 255.0f;
			m_Instance.SkyColorG = colorDialog.Color.G / 255.0f;
			m_Instance.SkyColorB = colorDialog.Color.B / 255.0f;
			UpdateMMF();
		}

		#endregion

		#region DYNAMIC LIGHTS

		private void checkBoxEnableDynamicPointLight_CheckedChanged( object sender, EventArgs e )
		{
			m_Instance.EnablePointLight = (sender as CheckBox).Checked ? 1 : 0;
			UpdateMMF();
		}

		private void checkBoxAnimatePointLight_CheckedChanged( object sender, EventArgs e )
		{
			m_Instance.AnimatePointLight = (sender as CheckBox).Checked ? 1 : 0;
			UpdateMMF();
		}

		private void floatTrackbarControlPointLightIntensity_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.PointLightIntensity = _Sender.Value;
			UpdateMMF();
		}

		private void panelLightColor_Click( object sender, EventArgs e )
		{
			colorDialog.Color = panelLightColor.BackColor;
			if ( colorDialog.ShowDialog( this ) != DialogResult.OK )
				return;

			panelLightColor.BackColor = colorDialog.Color;
			m_Instance.PointLightColorR = colorDialog.Color.R / 255.0f;
			m_Instance.PointLightColorG = colorDialog.Color.G / 255.0f;
			m_Instance.PointLightColorB = colorDialog.Color.B / 255.0f;
			UpdateMMF();
		}

		#endregion

		#region STATIC & EMISSIVE LIGHTS

		private void checkBoxEnableStaticLighting_CheckedChanged( object sender, EventArgs e )
		{
			m_Instance.EnableStaticLighting = (sender as CheckBox).Checked ? 1 : 0;
			UpdateMMF();
		}

		private void checkBoxEnableEmissive_CheckedChanged( object sender, EventArgs e )
		{
			m_Instance.EnableEmissiveMaterials = (sender as CheckBox).Checked ? 1 : 0;
			UpdateMMF();
		}

		private void checkBoxEmissiveRandomAnimation_CheckedChanged( object sender, EventArgs e )
		{
			//TODO!
		}

		private void floatTrackbarControlEmissiveIntensity_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.EmissiveIntensity = _Sender.Value;
			UpdateMMF();
		}

		private void panelEmissiveColor_Click( object sender, EventArgs e )
		{
			colorDialog.Color = panelEmissiveColor.BackColor;
			if ( colorDialog.ShowDialog( this ) != DialogResult.OK )
				return;

			panelEmissiveColor.BackColor = colorDialog.Color;
			m_Instance.EmissiveColorR = colorDialog.Color.R / 255.0f;
			m_Instance.EmissiveColorG = colorDialog.Color.G / 255.0f;
			m_Instance.EmissiveColorB = colorDialog.Color.B / 255.0f;
			UpdateMMF();
		}

		#endregion

		#region BOUNCES

		private void floatTrackbarControlSunBounceFactor_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.BounceFactorSun = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlSkyBounceFactor_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.BounceFactorSky = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlPointLightBounceFactor_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.BounceFactorPoint = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlStaticLightsBounceFactor_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.BounceFactorStaticLights = _Sender.Value;
			UpdateMMF();
		}

		private void floatTrackbarControlEmissiveLightsBounceFactor_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.BounceFactorEmissive = _Sender.Value;
			UpdateMMF();
		}

		#endregion

		private void checkBoxEnableRedistribution_CheckedChanged( object sender, EventArgs e )
		{
			m_Instance.EnableNeighborsRedistribution = (sender as CheckBox).Checked ? 1 : 0;
			UpdateMMF();
		}

		private void floatTrackbarControlNeighborProbesContribution_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.NeighborProbesContributionBoost = _Sender.Value;
			UpdateMMF();
		}

		private void checkBoxShowDebugProbes_CheckedChanged( object sender, EventArgs e )
		{
			m_Instance.ShowDebugProbes = (sender as CheckBox).Checked ? 1 : 0;
			UpdateMMF();
		}

		private void floatTrackbarControlDebugProbeIntensity_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_Instance.DebugProbesIntensity = _Sender.Value;
			UpdateMMF();
		}

		private void checkBoxShowNetwork_CheckedChanged( object sender, EventArgs e )
		{
			m_Instance.ShowDebugProbesNetwork = (sender as CheckBox).Checked ? 1 : 0;
			UpdateMMF();
		}

		#endregion
	}
}
