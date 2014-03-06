// tangent space along largest/smallest axes
// approx par simples smoothed cones si trop instable...

//////////////////////////////////////////////////////////////////////////
// The purpose of this little application is to analyze the rendering from a cube map probe to perform a grouping
//	of the pixels based on their position, normal and albedo to create a limited amount of sets that we'll be able
//	to replace by simple "disc surface elements" that can be lit with dynamic lights.
// 
// These pixels belonging to each set will be be considered having the same albedo and will light the probe with
//	precomputed spherical harmonic coefficients each pondered by the solid angle covered by the pixel in the direction
//	specific to the pixel.
// 
//////////////////////////////////////////////////////////////////////////
// 
// I'm testing several methods to create the sets:
// 
//	(1) k-means clustering (http://en.wikipedia.org/wiki/K-means_clustering), that consists in creating initial sets
//		using an educated guess and aggregating pixels to each set depending on a metric.
//		The pixel gets assigned to the set whose metric is the lowest. I'm currently using a
//		metric mixing spatial distance, hue distance (for albedo similarity) and normal discrepancies measurement.
//
//		I believe it could give interesting results with a little effort but I'm lazy and I think it's still a bit
//		dodgy because it doesn't handle pixels vicinity and tends to fragment sets a lot.
// 
//	(2) Filling method, it's an experimental method of mine that consists in browsing the pixels of the cube map and
//		perform a fill operation by joining adjacent pixels if and only if they're sufficiently close enough in terms
//		of distance, normal and color.
//		Each set created this way has its own list of pixels removed from the global list of free pixels, pixels whose
//		solid angle is too low are discarded.
//		The algorithm continues until all pixels have been discarded or added to a set, then the algorithm enters a second
//		phase of optimization where sets are merged together if sufficiently close, or discarded if not significant enough.
// 
//////////////////////////////////////////////////////////////////////////
// 
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ProbeSHEncoder
{
	public partial class EncoderForm : Form
	{
		#region FIELDS

		private RegistryKey			m_AppKey;
		private string				m_ApplicationPath;

		private Probe				m_Probe = null;

		#endregion

		#region PROPERTIES

		private Probe		SelectedProbe
		{
			get { return m_Probe; }
			set
			{
				if ( value == m_Probe )
					return;

				m_Probe = value;

				buttonCompute.Enabled = m_Probe != null;
				buttonComputeFilling.Enabled = m_Probe != null;
				saveResultsToolStripMenuItem.Enabled = m_Probe != null && m_Probe.m_Sets.Length > 0;

				// Redraw cube map...
				outputPanel1.Probe = m_Probe;
				outputPanel1.UpdateBitmap();
			}
		}

		#endregion

		#region METHODS

		public EncoderForm()
		{
			InitializeComponent();

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\ProbeSHEncoder" );
			m_ApplicationPath = Path.GetDirectoryName( Application.ExecutablePath );

			outputPanel1.At = -WMath.Vector.UnitZ;

			// Restore settings
			floatTrackbarControlPosition.Value = GetRegKeyFloat( "DistanceSeparationImportance", floatTrackbarControlPosition.Value );
			floatTrackbarControlNormal.Value = GetRegKeyFloat( "NormalSeparationImportance", floatTrackbarControlNormal.Value );
			floatTrackbarControlAlbedo.Value = GetRegKeyFloat( "AlbedoSeparationImportance", floatTrackbarControlAlbedo.Value );

			integerTrackbarControlK.Value = GetRegKeyInt( "SetsCount", integerTrackbarControlK.Value );
		}

		private void	LoadCubeMap( FileInfo _POMCubeMap )
		{
			try
			{
				Probe	NewProbe = new Probe();
				NewProbe.LoadCubeMap( _POMCubeMap );

				SelectedProbe = NewProbe;
			}
			catch ( Exception _e )
			{
				throw new Exception( "Error while loading cube map \"" + _POMCubeMap.Name + "\": " + _e.Message );
			}
			finally
			{
				buttonComputeFilling.Focus();
			}
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

		private void	MessageBox( string _Text )
		{
			MessageBox( _Text, MessageBoxButtons.OK );
		}
		private void	MessageBox( string _Text, MessageBoxButtons _Buttons )
		{
			MessageBox( _Text, _Buttons, MessageBoxIcon.Information );
		}
		private void	MessageBox( string _Text, MessageBoxIcon _Icon )
		{
			MessageBox( _Text, MessageBoxButtons.OK, _Icon );
		}
		private void	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon )
		{
			System.Windows.Forms.MessageBox.Show( this, _Text, "SH Encoder", _Buttons, _Icon );
		}

		#endregion

		#endregion

		#region EVENT HANDLERS

		private void radioButtonAlbedo_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.ALBEDO;
		}

		private void radioButtonDistance_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.DISTANCE;
		}

		private void radioButtonNormal_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.NORMAL;
		}

		private void radioButtonStaticLit_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.STATIC_LIT;
		}

		private void radioButtonFaceIndex_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.FACE_INDEX;
		}

		private void radioButtonEmissiveMatID_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.EMISSIVE_MAT_ID;
		}

		private void radioButtonNeighborProbeID_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.NEIGHBOR_PROBE_ID;
		}

		private void radioButtonSetIndex_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.SET_INDEX;
		}

		private void radioButtonSetColor_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.SET_ALBEDO;
		}

		private void radioButtonSetNormal_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.SET_NORMAL;
		}

		private void radioButtonSetDistance_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.SET_DISTANCE;
		}

		private void integerTrackbarControlSetIsolation_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			outputPanel1.IsolatedSetIndex = _Sender.Value;
		}

		private void checkBoxSetIsolation_CheckedChanged( object sender, EventArgs e )
		{
			outputPanel1.IsolateSet = checkBoxSetIsolation.Checked;
		}

		private void radioButtonSH_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.SH;
		}

		private void checkBoxSHStatic_CheckedChanged( object sender, EventArgs e )
		{
			outputPanel1.ShowSHStatic = (sender as CheckBox).Checked;
		}

		private void checkBoxSHDynamic_CheckedChanged( object sender, EventArgs e )
		{
			outputPanel1.ShowSHDynamic = (sender as CheckBox).Checked;
		}

		private void checkBoxSHEmissive_CheckedChanged( object sender, EventArgs e )
		{
			outputPanel1.ShowSHEmissive = (sender as CheckBox).Checked;
		}

		private void checkBoxSHOcclusion_CheckedChanged( object sender, EventArgs e )
		{
			outputPanel1.ShowSHOcclusion = (sender as CheckBox).Checked;
		}

		private void checkBoxSHNormalized_CheckedChanged( object sender, EventArgs e )
		{
			outputPanel1.NormalizeSH = (sender as CheckBox).Checked;
		}

		private void radioButtonSetSamples_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				outputPanel1.Viz = OutputPanel.VIZ_TYPE.SET_SAMPLES;
		}

		private void buttonReset_Click( object sender, EventArgs e )
		{
			floatTrackbarControlPosition.Value = 1.0f;
			floatTrackbarControlNormal.Value = 1.0f;
			floatTrackbarControlAlbedo.Value = 1.0f;
		}

		private void floatTrackbarControlPosition_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			SetRegKey( "DistanceSeparationImportance", _Sender.Value.ToString() );
		}

		private void floatTrackbarControlNormal_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			SetRegKey( "NormalSeparationImportance", _Sender.Value.ToString() );
		}

		private void floatTrackbarControlAlbedo_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			SetRegKey( "AlbedoSeparationImportance", _Sender.Value.ToString() );
		}

		private void integerTrackbarControlK_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			SetRegKey( "SetsCount", _Sender.Value.ToString() );
		}

		private void loadProbeToolStripMenuItem_Click( object sender, EventArgs e )
		{
			string	OldFileName = GetRegKey( "LastProbeFilename", m_ApplicationPath );
			openFileDialog.InitialDirectory = Path.GetDirectoryName( OldFileName );
			openFileDialog.FileName = Path.GetFileName( OldFileName );
			if ( openFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;
			SetRegKey( "LastProbeFilename", openFileDialog.FileName );


			// Attempt to load the probe
			try
			{
				LoadCubeMap( new FileInfo( openFileDialog.FileName ) );
			}
			catch ( Exception _e )
			{
				MessageBox( "Failed to load probe!\n\n" + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		private void saveResultsToolStripMenuItem_Click( object sender, EventArgs e )
		{
			string	OldFileName = GetRegKey( "LastProbeSetFilename", m_ApplicationPath );
			saveFileDialog.InitialDirectory = Path.GetDirectoryName( OldFileName );
			saveFileDialog.FileName = Path.GetFileName( OldFileName );
			if ( saveFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;
			SetRegKey( "LastProbeSetFilename", saveFileDialog.FileName );

			FileInfo	F = new FileInfo( saveFileDialog.FileName );
			m_Probe.SaveSets( F );
		}

		private void batchEncodeToolStripMenuItem_Click( object sender, EventArgs e )
		{
			// Ask where to load
			string	OldFolder = GetRegKey( "LastProbesFolderSource", Path.GetDirectoryName( m_ApplicationPath ) );
			folderBrowserDialog.SelectedPath = OldFolder;
			if ( folderBrowserDialog.ShowDialog( this ) != DialogResult.OK )
				return;
			SetRegKey( "LastProbesFolderSource", folderBrowserDialog.SelectedPath );

			DirectoryInfo	SourceDir = new DirectoryInfo( folderBrowserDialog.SelectedPath );

			// Ask where to save
			OldFolder = GetRegKey( "LastProbesFolderTarget", Path.GetDirectoryName( m_ApplicationPath ) );
			folderBrowserDialog.SelectedPath = OldFolder;
			if ( folderBrowserDialog.ShowDialog( this ) != DialogResult.OK )
				return;
			SetRegKey( "LastProbesFolderTarget", folderBrowserDialog.SelectedPath );

			DirectoryInfo	TargetDir = new DirectoryInfo( folderBrowserDialog.SelectedPath );

			// Batch process!
			progressBarBatchConvert.Value = 0;
			progressBarBatchConvert.Visible = true;

			string			Errors = null;
			int				ProcessedProbesCount = 0;
			List<FileInfo>	PomFiles = new List<FileInfo>( SourceDir.GetFiles( "*.pom" ) );
			int				OriginalFilesCount = PomFiles.Count;
			while ( PomFiles.Count > 0 )
			{
				try
				{
					FileInfo	F = PomFiles[0];

					PomFiles.Remove( F );

					LoadCubeMap( F );

					// Create probe sets
					buttonComputeFilling_Click( null, EventArgs.Empty );

					// Save result
					string		TargetFileName = Path.GetFileNameWithoutExtension( F.FullName ) + ".probeset";
					FileInfo	TargetFile = new FileInfo( Path.Combine( TargetDir.FullName, TargetFileName ) );
					m_Probe.SaveSets( TargetFile );

					ProcessedProbesCount++;
					progressBarBatchConvert.Value = progressBarBatchConvert.Maximum * (OriginalFilesCount - PomFiles.Count) / OriginalFilesCount;
					progressBarBatchConvert.Refresh();
				}
				catch ( Exception _e )
				{
					Errors += _e.Message + "\r\n";
				}
			}

			progressBarBatchConvert.Value = progressBarBatchConvert.Maximum;
			if ( Errors == null )
				MessageBox( "Success! " + ProcessedProbesCount + " probes were processed...", MessageBoxButtons.OK, MessageBoxIcon.Information );
			else
				MessageBox( ProcessedProbesCount + " probes were processed but errors were found:\r\n\r\n" + Errors, MessageBoxButtons.OK, MessageBoxIcon.Warning );

			progressBarBatchConvert.Visible = false;
		}

		private void buttonCompute_Click( object sender, EventArgs e )
		{
			m_Probe.ComputeKMeans(
				integerTrackbarControlK.Value,
				floatTrackbarControlPosition.Value,
				floatTrackbarControlNormal.Value,
				floatTrackbarControlAlbedo.Value,
				floatTrackbarControlLambda.Value );

			//////////////////////////////////////////////////////////////////////////
			// Refresh UI
			textBoxResults.Text = m_Probe.m_Sets.Length + " sets generated:\r\n\r\n";
			for ( int SetIndex=0; SetIndex < m_Probe.m_Sets.Length; SetIndex++ )
			{
				Probe.Set	S = m_Probe.m_Sets[SetIndex];
				textBoxResults.Text += SetIndex + ") " + S.SetPixels.Count + " pixels (" + (100.0f * S.SetPixels.Count / m_Probe.m_ScenePixels.Count).ToString( "G4" ) + "%)\r\n"
									+ "Albedo = (" + S.Albedo.x.ToString( "G4" ) + ", " + S.Albedo.y.ToString( "G4" ) + ", " + S.Albedo.z.ToString( "G4" ) + ")\r\n\r\n";
			}

			integerTrackbarControlSetIsolation.RangeMax = m_Probe.m_Sets.Length-1;
			integerTrackbarControlSetIsolation.VisibleRangeMax = integerTrackbarControlSetIsolation.RangeMax;

			saveResultsToolStripMenuItem.Enabled = m_Probe.m_Sets.Length > 0;
			outputPanel1.UpdateBitmap();
		}

		private void buttonComputeFilling_Click( object sender, EventArgs e )
		{
			m_Probe.ComputeFloodFill(
				integerTrackbarControlK.Value,
				integerTrackbarControlLightSamples.Value,
				floatTrackbarControlPosition.Value,
				floatTrackbarControlNormal.Value,
				floatTrackbarControlAlbedo.Value,
				floatTrackbarControlLambda.Value );

			// Send to output panel for visual debugging
			outputPanel1.SHStatic = m_Probe.m_StaticSH;
			outputPanel1.SHOcclusion = m_Probe.m_OcclusionSH;
			outputPanel1.SHDynamic = m_Probe.m_SHSumDynamic;
			outputPanel1.SHEmissive = m_Probe.m_SHSumEmissive;


			//////////////////////////////////////////////////////////////////////////
			// Refresh UI
			// 
			textBoxResults.Text = m_Probe.m_EmissiveSets.Length + " emissive sets generated\r\n";
			textBoxResults.Text += m_Probe.m_Sets.Length + " sets generated:\r\n\r\n";
			for ( int SetIndex=0; SetIndex < m_Probe.m_Sets.Length; SetIndex++ )
			{
				Probe.Set	S = m_Probe.m_Sets[SetIndex];
				textBoxResults.Text += SetIndex + ") " + S.SetPixels.Count + " pixels (" + (100.0f * S.SetPixels.Count / m_Probe.m_ScenePixels.Count).ToString( "G4" ) + "%)\r\n"
									+ "Albedo = (" + S.Albedo.x.ToString( "G4" ) + ", " + S.Albedo.y.ToString( "G4" ) + ", " + S.Albedo.z.ToString( "G4" ) + ")\r\n\r\n";
			}

			integerTrackbarControlSetIsolation.RangeMax = m_Probe.m_Sets.Length-1;
			integerTrackbarControlSetIsolation.VisibleRangeMax = integerTrackbarControlSetIsolation.RangeMax;

//			radioButtonSetIndex.Checked = true;
			outputPanel1.UpdateBitmap();
		}

		#endregion
	}
}
