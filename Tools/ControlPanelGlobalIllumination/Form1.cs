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

			// Dynamic objects params
			public int		DynamicObjectsCount;

			// Bounce params
			public float	BounceFactorSun;
			public float	BounceFactorSky;
			public float	BounceFactorPoint;
			public float	BounceFactorStaticLights;
			public float	BounceFactorEmissive;

			// Neighors
			public int		EnableNeighborsRedistribution;
			public float	NeighborProbesContributionBoost;

			// Probes Update
			public int		MaxProbeUpdatesPerFrame;

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

			// Dynamic objects
			integerTrackbarControlDynamicObjectsCount.Value = m_Instance.DynamicObjectsCount;

			// Bounce
			floatTrackbarControlSunBounceFactor.Value = m_Instance.BounceFactorSun;
			floatTrackbarControlSkyBounceFactor.Value = m_Instance.BounceFactorSky;
			floatTrackbarControlPointLightBounceFactor.Value = m_Instance.BounceFactorPoint;
			floatTrackbarControlStaticLightsBounceFactor.Value = m_Instance.BounceFactorStaticLights;
			floatTrackbarControlEmissiveLightsBounceFactor.Value = m_Instance.BounceFactorEmissive;

			// Neighborhood
			checkBoxEnableRedistribution.Checked = m_Instance.EnableNeighborsRedistribution != 0;
			floatTrackbarControlNeighborProbesContribution.Value = m_Instance.NeighborProbesContributionBoost;

			// Probes update
			integerTrackbarControlMaxProbeUpdatesPerFrame.Value = m_Instance.MaxProbeUpdatesPerFrame;

			// Debug
			checkBoxShowDebugProbes.Checked = m_Instance.ShowDebugProbes != 0;
			checkBoxShowNetwork.Checked = m_Instance.ShowDebugProbesNetwork != 0;

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
				else if ( Field.FieldType == typeof(Nuaj.Cirrus.Utility.IntegerTrackbarControl) )
				{
					Nuaj.Cirrus.Utility.IntegerTrackbarControl	Slider = Field.GetValue( this ) as Nuaj.Cirrus.Utility.IntegerTrackbarControl;
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

		#region DYNAMIC LIGHTS & OBJECTS

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

		private void integerTrackbarControlDynamicObjectsCount_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			m_Instance.DynamicObjectsCount = _Sender.Value;
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

		private void integerTrackbarControlMaxProbeUpdatesPerFrame_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			m_Instance.MaxProbeUpdatesPerFrame = _Sender.Value;
			UpdateMMF();
		}

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
// 	dindex = dindex->branch[getbits(1)];
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
// 
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
