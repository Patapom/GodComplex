// tangent space along largest/smallest axes
// approx par simples smoothed cones si trop instable...

//////////////////////////////////////////////////////////////////////////
// The purpose of this little application is to analyze the rendering from a cube map probe to perform a grouping
//	of the pixels based on their position, normal and albedo to create a limited amount of sets that we'll be able
//	to replace by simple "disc surface elements" that can be lit with dynamic lights.
// 
// The pixels belonging to each set will be considered having the same albedo and will light the probe with
//	precomputed spherical harmonic coefficients, each pondered by the solid angle covered by the pixel from the direction
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
		#region NESTED TYPES
		
		[System.Diagnostics.DebuggerDisplay( "ProbeID={ProbeID} Importance={Importance}" )]
		class ProbeInfluence
		{
			public const UInt32	INVALID_PROBEID = 0xFFFFFFFF;

			public UInt32	ProbeID = INVALID_PROBEID;
			public double	Importance = 0.0;
		};

		#endregion

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
			integerTrackbarControlLightSamples.Value = GetRegKeyInt( "LightSamplesCount", integerTrackbarControlLightSamples.Value );
			floatTrackbarControlLambda.Value = GetRegKeyFloat( "Lambda", floatTrackbarControlLambda.Value );
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

		private void floatTrackbarControlLambda_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			SetRegKey( "Lambda", _Sender.Value.ToString() );
		}

		private void integerTrackbarControlLightSamples_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			SetRegKey( "LightSamplesCount", _Sender.Value.ToString() );
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

			uint	MaxFaceIndex = 0;
			Dictionary<UInt32,ProbeInfluence>	BestProbeInfluencePerFace = new Dictionary<UInt32,ProbeInfluence>();

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

					// Process probe influences and keep the best ones for each face
					MaxFaceIndex = Math.Max( MaxFaceIndex, m_Probe.m_MaxFaceIndex );
					foreach ( uint FaceIndex in m_Probe.m_ProbeInfluencePerFace.Keys )
					{
						double	ProbeImportance = m_Probe.m_ProbeInfluencePerFace[FaceIndex];
						if ( !BestProbeInfluencePerFace.ContainsKey( FaceIndex ) )
						{	// Use this probe
							BestProbeInfluencePerFace[FaceIndex] = new ProbeInfluence() { Importance=ProbeImportance, ProbeID=(UInt32) m_Probe.m_ProbeID };
							continue;
						}

						double	ExistingImportance = BestProbeInfluencePerFace[FaceIndex].Importance;
						if ( ExistingImportance >= ProbeImportance )
							continue;

						// We have a better probe
						BestProbeInfluencePerFace[FaceIndex].ProbeID = (UInt32) m_Probe.m_ProbeID;
						BestProbeInfluencePerFace[FaceIndex].Importance = ProbeImportance;
					}

					// Update UI
					ProcessedProbesCount++;
					progressBarBatchConvert.Value = progressBarBatchConvert.Maximum * (OriginalFilesCount - PomFiles.Count) / OriginalFilesCount;
					progressBarBatchConvert.Refresh();
					outputPanel1.Refresh();
					textBoxResults.Refresh();
				}
				catch ( Exception _e )
				{
					Errors += _e.Message + "\r\n";
				}
			}

			// Save the final probe influences
			FileInfo	TargetInfluenceFile = new FileInfo( Path.Combine( TargetDir.FullName, "ProbeInfluence.pim" ) );
			using ( FileStream S = TargetInfluenceFile.Create() )
				using( BinaryWriter Writer = new BinaryWriter( S ) )
					for ( uint FaceIndex=0; FaceIndex < MaxFaceIndex; FaceIndex++ )
					{
						if ( BestProbeInfluencePerFace.ContainsKey( FaceIndex ) )
						{
							ProbeInfluence	Influence = BestProbeInfluencePerFace[FaceIndex];
							Writer.Write( Influence.ProbeID );
							Writer.Write( Influence.Importance );
						}
						else
						{
							Writer.Write( (int) -1 );
							Writer.Write( 0.0 );
						}
					}

			// Notify
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
			try
			{
				m_Probe.ComputeFloodFill(
					integerTrackbarControlK.Value,
					integerTrackbarControlLightSamples.Value,
					floatTrackbarControlPosition.Value,
					floatTrackbarControlNormal.Value,
					floatTrackbarControlAlbedo.Value,
					floatTrackbarControlLambda.Value );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while encoding probe: " + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}


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

			// Send to output panel for visual debugging
			outputPanel1.SHStatic = m_Probe.m_StaticSH;
			outputPanel1.SHOcclusion = m_Probe.m_OcclusionSH;
			outputPanel1.SHDynamic = m_Probe.m_SHSumDynamic;
			outputPanel1.SHEmissive = m_Probe.m_SHSumEmissive;

//			radioButtonSetIndex.Checked = true;
			outputPanel1.UpdateBitmap();

			// We can now save the results
			saveResultsToolStripMenuItem.Enabled = true;
		}

		/// <summary>
		/// This tool builds the additional vertex stream for each primitive that will attach the best probe ID (as a U16)
		///  to a vertex so the vertex shader has a single entry point into the probes network
		/// Most face indices have already been rendered in the probe cube map and the batch processing tool took care of
		///  selecting the most important probe for these faces, but some faces don't have probe information.
		///  Maybe they have been occluded by other faces (shadowing) or are too small to be rendered in the cube map.
		/// For these special faces, we need to propagate probe IDs from valid faces until the entire mesh is filled with
		///  valid probe IDs.
		/// 
		/// We proceed primitive per primitive since they are all disconnected so no propagation would be possible cross
		///  primitives unless we could reconnect split vertices, but I don't want to do that. Maybe it will become necessary
		///  at some point but for the moment I don't care.
		/// Some primitives may lack probe ID completely (!!), for these I choose to assign the ID of the nearest probe, which
		///  may be a bit dangerous, especially if we're dealing with probes that are standing right behind the primitive for example.
		/// In that case, perhaps using a scoring scheme that depends on both distance and orientation would be better? Let's see that when the problem arises...
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void encodeFaceProbesToolStripMenuItem_Click( object sender, EventArgs e )
		{
			// Ask for the influence file to load from
			string	OldFileName = GetRegKey( "LastProbeInfluenceFileName", GetRegKey( "LastProbesFolderTarget", Path.GetDirectoryName( m_ApplicationPath ) ) );
			openFileDialogProbeInfluence.FileName = OldFileName;
			if ( openFileDialogProbeInfluence.ShowDialog( this ) != DialogResult.OK )
				return;
			SetRegKey( "LastProbeInfluenceFileName", openFileDialogProbeInfluence.FileName );

			FileInfo	SourceInfluenceFile = new FileInfo( openFileDialogProbeInfluence.FileName );

			// Ask for the scene file to apply to
			string	OldSceneFile = GetRegKey( "LastSceneFileName", GetRegKey( "LastProbesFolderSource", Path.GetDirectoryName( m_ApplicationPath ) ) );
			openFileDialogScene.FileName = OldSceneFile;
			if ( openFileDialogScene.ShowDialog( this ) != DialogResult.OK )
				return;
			SetRegKey( "LastSceneFileName", openFileDialogScene.FileName );

			FileInfo	SceneFile = new FileInfo( openFileDialogScene.FileName );

			try
			{
				// Read the scene file
				GCXFormat.Scene	GCX = null;
				using ( FileStream S = SceneFile.OpenRead() )
					using( BinaryReader R = new BinaryReader( S ) )
						GCX = new GCXFormat.Scene( R );

				// Read the probe influences
				ProbeInfluence[]	FaceInfluences = new ProbeInfluence[GCX.m_TotalFacesCount];
				using ( FileStream S = SourceInfluenceFile.OpenRead() )
					using( BinaryReader R = new BinaryReader( S ) )
					{
						int	SizeofProbeInfluence = System.Runtime.InteropServices.Marshal.SizeOf(typeof(UInt32)) + System.Runtime.InteropServices.Marshal.SizeOf(typeof(double));
						int	FacesCount = (int) S.Length / SizeofProbeInfluence;
						if ( (S.Length % SizeofProbeInfluence) != 0 )
							throw new Exception( "Probe influence file is larger than an integer number of ProbeInfluence structures!" );

						for ( int FaceIndex=0; FaceIndex < FacesCount; FaceIndex++ )
						{
							UInt32	ProbeID = R.ReadUInt32();
							double	Importance = R.ReadDouble();
							FaceInfluences[FaceIndex] = new ProbeInfluence() { ProbeID = ProbeID, Importance = Importance };
						}
						for ( int FaceIndex=FacesCount; FaceIndex < GCX.m_TotalFacesCount; FaceIndex++ )
							FaceInfluences[FaceIndex] = new ProbeInfluence() { ProbeID = ProbeInfluence.INVALID_PROBEID, Importance = 0.0 };
					}

				UInt16[]	VertexStreamProbeID = new UInt16[GCX.m_TotalVerticesCount];	// The final vertex stream we're building


				//////////////////////////////////////////////////////////////////////////
				// The algorithm is quite simple:
				//	For each primitive
				//		Create a winged edge structure for each face
				//		Assign known probe influence to each face
				//		Build a list of faces without probe influence
				//
				//		While list is not empty and its size keeps decreasing
				//			For each face without influence
				//				If neighbor faces have influence information
				//					Copy best influence from neighbor
				//					Remove face from the list
				//		
				//		If list is not empty			// We have a bunch of disconnected faces that can't see any probe
				//			For each face of the list
				//				Assign [NO PROBE|NEAREST PROBE|WORLD PROBE] ? (not clear yet which is best)
				//		
				//		Redistribute best probe's ID to each face vertex
				//
				//	Save stream of vertex probe IDs to be merged into an additional vertex buffer stream at runtime
				//		
				int	FacesWithoutProbeCount = 0;
				int	PrimitivesWithoutProbeCount = 0;

				foreach ( GCXFormat.Scene.Node N in GCX.m_Nodes )
				{
					GCXFormat.Scene.Mesh	M = N as GCXFormat.Scene.Mesh;
					if ( M == null )
						continue;

					foreach ( GCXFormat.Scene.Mesh.Primitive P in M.m_Primitives )
					{
						int	FacesCount = P.m_Faces.Length;

						P.BuildWingedEdgesMesh();

						// Tag winged edge faces with probe influence & list faces without valid probe ID
						List<GCXFormat.Scene.Mesh.Primitive.WingedEdgeTriangle>	MissingProbeIDFaces = new List<GCXFormat.Scene.Mesh.Primitive.WingedEdgeTriangle>( FacesCount );
						for ( int FaceIndex=0; FaceIndex < FacesCount; FaceIndex++ )
						{
							ProbeInfluence	Influence = FaceInfluences[P.m_FaceOffset+FaceIndex];
							GCXFormat.Scene.Mesh.Primitive.WingedEdgeTriangle	Face = P.m_WingedEdgeFaces[FaceIndex];
							Face.m_Tag = Influence;
							if ( Influence.ProbeID == ProbeInfluence.INVALID_PROBEID )
								MissingProbeIDFaces.Add( Face );
						}

						// Propagate probe influence
						int	LastListSize = MissingProbeIDFaces.Count+1;
						while ( MissingProbeIDFaces.Count > 0 && MissingProbeIDFaces.Count < LastListSize )
						{
							LastListSize = MissingProbeIDFaces.Count;	// Keep track of list's size so we know if anything moved
							for ( int FaceIndex=MissingProbeIDFaces.Count-1; FaceIndex >= 0; FaceIndex-- )	// Browse backward so we can safely remove elements from the list
							{
								GCXFormat.Scene.Mesh.Primitive.WingedEdgeTriangle	Face = MissingProbeIDFaces[FaceIndex];

								// Examine neighbors' probe influence and propagate best probe
								ProbeInfluence	BestProbe = null;
								for ( int EdgeIndex=0; EdgeIndex < 3; EdgeIndex++ )
								{
									int	NeighborFaceIndex = Face.m_Edges[EdgeIndex].GetOtherFaceIndex( Face.m_Index );
									if ( NeighborFaceIndex != -1 )
									{
										GCXFormat.Scene.Mesh.Primitive.WingedEdgeTriangle	NeighborFace = P.m_WingedEdgeFaces[NeighborFaceIndex];
										ProbeInfluence	NeighborProbe = NeighborFace.m_Tag as ProbeInfluence;
										if ( NeighborProbe.ProbeID != ProbeInfluence.INVALID_PROBEID && (BestProbe == null || NeighborProbe.Importance > BestProbe.Importance) )
											BestProbe = NeighborProbe;	// Found a better probe for that face!
									}
								}

								if ( BestProbe == null )
									continue;	// Still no valid probe found...

								Face.m_Tag = BestProbe;	// Share the probe, don't care...
								MissingProbeIDFaces.RemoveAt( FaceIndex );	// We can safely remove the face by index since we're browsing the list backward
							}
						}

						// At this point, either the list of faces missing probe IDs is empty in which case the job is done
						//	or we need to assign an arbitrary probe ID to those faces
						if ( MissingProbeIDFaces.Count > 0 )
						{
							FacesWithoutProbeCount += MissingProbeIDFaces.Count;
							PrimitivesWithoutProbeCount++;

							// TODO: Assign nearest probe!
						}

						// Now that we have valid probe IDs for each face, propagate best probe influence to each vertex
						ProbeInfluence[]	BestProbePerVertex = new ProbeInfluence[P.m_Vertices.Length];
						for ( int FaceIndex=0; FaceIndex < FacesCount; FaceIndex++ )
						{
							GCXFormat.Scene.Mesh.Primitive.Face	Face = P.m_Faces[FaceIndex];
							ProbeInfluence	FaceProbe = P.m_WingedEdgeFaces[FaceIndex].m_Tag as ProbeInfluence;

							if ( BestProbePerVertex[Face.V0] == null || BestProbePerVertex[Face.V0].Importance < FaceProbe.Importance )	BestProbePerVertex[Face.V0] = FaceProbe;	// Better probe for that vertex
							if ( BestProbePerVertex[Face.V1] == null || BestProbePerVertex[Face.V1].Importance < FaceProbe.Importance )	BestProbePerVertex[Face.V1] = FaceProbe;	// Better probe for that vertex
							if ( BestProbePerVertex[Face.V2] == null || BestProbePerVertex[Face.V2].Importance < FaceProbe.Importance )	BestProbePerVertex[Face.V2] = FaceProbe;	// Better probe for that vertex
						}

						// Finally, we can splat probe IDs into the giant vertex stream
						for ( int VertexIndex=0; VertexIndex < P.m_Vertices.Length; VertexIndex++ )
							VertexStreamProbeID[P.m_VertexOffset+VertexIndex] = (UInt16) (BestProbePerVertex[VertexIndex] != null ? BestProbePerVertex[VertexIndex].ProbeID : 0xFFFF);	// TODO <= warn if we still have invalid vertices!
					}
				}

				//////////////////////////////////////////////////////////////////////////
				// 2] 

			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while probe influences in scene file: " + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}
		}

		#endregion
	}
}
