//////////////////////////////////////////////////////////////////////////
// Builds an AO map from a height maps
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace GenerateSelfShadowedBumpMap
{
	public partial class GeneratorForm : Form
	{
		#region CONSTANTS

		private const int		MAX_THREADS = 1024;			// Maximum threads run by the compute shader

		private const int		BILATERAL_PROGRESS = 50;	// Bilateral filtering is considered as this % of the total task (bilateral is quite long so I decided it was equivalent to 50% of the complete computation task)
		private const int		MAX_LINES = 16;				// Process at most that amount of lines of a 4096x4096 image for a single dispatch

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBInput {
			public UInt32	Y0;					// Index of the texture line we're processing
			public UInt32	RaysCount;			// Amount of rays in the structured buffer
			public UInt32	MaxStepsCount;		// Maximum amount of steps to take before stopping
			public UInt32	Tile;				// Tiling flag
			public float	TexelSize_mm;		// Size of a texel (in millimeters)
			public float	Displacement_mm;	// Max displacement value encoded by the height map (in millimeters)
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBFilter {
			public UInt32	Y0;					// Index of the texture line we're processing
			public float	Radius;				// Radius of the bilateral filter
			public float	Tolerance;			// Range tolerance of the bilateral filter
			public UInt32	Tile;				// Tiling flag
		}

		#endregion

		#region FIELDS

		private RegistryKey						m_AppKey;
		private string							m_ApplicationPath;

		private System.IO.FileInfo				m_SourceFileName = null;
		private int								W, H;
		private ImageUtility.Bitmap				m_BitmapSource = null;

		internal RendererManaged.Device			m_Device = new RendererManaged.Device();
		internal RendererManaged.Texture2D		m_TextureSource = null;
		internal RendererManaged.Texture2D		m_TextureTarget0 = null;
		internal RendererManaged.Texture2D		m_TextureTarget1 = null;
		internal RendererManaged.Texture2D		m_TextureTarget_CPU = null;

		// SSBump Generation
		private RendererManaged.ConstantBuffer<CBInput>						m_CB_Input;
		private RendererManaged.StructuredBuffer<RendererManaged.float3>	m_SB_Rays = null;
		private RendererManaged.ComputeShader								m_CS_GenerateSSBumpMap = null;

		// Bilateral filtering pre-processing
		private RendererManaged.ConstantBuffer<CBFilter>	m_CB_Filter;
		private RendererManaged.ComputeShader	m_CS_BilateralFilter = null;

		private ImageUtility.ColorProfile		m_LinearProfile = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.Chromaticities.sRGB, ImageUtility.ColorProfile.GAMMA_CURVE.STANDARD, 1.0f );
		private ImageUtility.Bitmap				m_BitmapResult = null;

		#endregion

		#region PROPERTIES

		internal float	TextureHeight_mm {
			get { return 10.0f * floatTrackbarControlHeight.Value; }
		}

		internal float	TextureSize_mm {
			get { return 10.0f * floatTrackbarControlPixelDensity.Value; }
		}

		#endregion

		#region METHODS

		public unsafe GeneratorForm()
		{
			InitializeComponent();

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\AOMapGenerator" );
			m_ApplicationPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );

			#if DEBUG
				buttonReload.Visible = true;
			#endif
		}

		protected override void  OnLoad(EventArgs e)
		{
 			base.OnLoad(e);

			try {
				m_Device.Init( viewportPanelResult.Handle, false, true );

				// Create our compute shaders
				#if DEBUG
					m_CS_BilateralFilter = new RendererManaged.ComputeShader( m_Device, new RendererManaged.ShaderFile( new System.IO.FileInfo( "./Shaders/BilateralFiltering.hlsl" ) ), "CS", null );
					m_CS_GenerateSSBumpMap = new RendererManaged.ComputeShader( m_Device, new RendererManaged.ShaderFile( new System.IO.FileInfo( "./Shaders/GenerateAOMap.hlsl" ) ), "CS", null );
				#else
					m_CS_BilateralFilter = RendererManaged.ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Binary/BilateralFiltering.fxbin" ), "CS" );
					m_CS_GenerateSSBumpMap = RendererManaged.ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Binary/GenerateAOMap.fxbin" ), "CS" );
				#endif

				// Create our constant buffers
				m_CB_Input = new RendererManaged.ConstantBuffer<CBInput>( m_Device, 0 );
				m_CB_Filter = new RendererManaged.ConstantBuffer<CBFilter>( m_Device, 0 );

				// Create our structured buffer containing the rays
				m_SB_Rays = new RendererManaged.StructuredBuffer<RendererManaged.float3>( m_Device, MAX_THREADS, true );
				integerTrackbarControlRaysCount_SliderDragStop( integerTrackbarControlRaysCount, 0 );

			} catch ( Exception _e ) {
				MessageBox( "Failed to create DX11 device and default shaders:\r\n", _e );
				Close();
			}
		}

		protected override void OnClosing( CancelEventArgs e )
		{
			try {
				m_CS_GenerateSSBumpMap.Dispose();
				m_CS_BilateralFilter.Dispose();

				m_SB_Rays.Dispose();
				m_CB_Filter.Dispose();
				m_CB_Input.Dispose();

				if ( m_TextureTarget_CPU != null )
					m_TextureTarget_CPU.Dispose();
				if ( m_TextureTarget1 != null )
					m_TextureTarget1.Dispose();
				if ( m_TextureTarget0 != null )
					m_TextureTarget0.Dispose();
				if ( m_TextureSource != null )
					m_TextureSource.Dispose();

				m_Device.Dispose();
			} catch ( Exception ) {
			}

			e.Cancel = false;
			base.OnClosing( e );
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && (components != null) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		private void	LoadHeightMap( System.IO.FileInfo _FileName ) {
			try {
				panelParameters.Enabled = false;

				// Dispose of existing resources
				if ( m_BitmapSource != null )
					m_BitmapSource.Dispose();
				m_BitmapSource = null;

				if ( m_TextureTarget_CPU != null )
					m_TextureTarget_CPU.Dispose();
				m_TextureTarget_CPU = null;
				if ( m_TextureTarget0 != null )
					m_TextureTarget0.Dispose();
				m_TextureTarget0 = null;
				if ( m_TextureTarget1 != null )
					m_TextureTarget1.Dispose();
				m_TextureTarget1 = null;
				if ( m_TextureSource != null )
					m_TextureSource.Dispose();
				m_TextureSource = null;

				// Load the source image assuming it's in linear space
				m_SourceFileName = _FileName;
				m_BitmapSource = new ImageUtility.Bitmap( _FileName, m_LinearProfile );
				outputPanelInputHeightMap.Image = m_BitmapSource;

				W = m_BitmapSource.Width;
				H = m_BitmapSource.Height;

				// Build the source texture
				RendererManaged.PixelsBuffer	SourceHeightMap = new RendererManaged.PixelsBuffer( W*H*4 );
				using ( System.IO.BinaryWriter Wr = SourceHeightMap.OpenStreamWrite() )
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
							Wr.Write( m_BitmapSource.ContentXYZ[X,Y].y );

				m_TextureSource = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.R32_FLOAT, false, false, new RendererManaged.PixelsBuffer[] { SourceHeightMap } );

				// Build the target UAV & staging texture for readback
				m_TextureTarget0 = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.R32_FLOAT, false, true, null );
				m_TextureTarget1 = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.R32_FLOAT, false, true, null );
				m_TextureTarget_CPU = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.R32_FLOAT, true, false, null );

				panelParameters.Enabled = true;
				buttonGenerate.Focus();

			} catch ( Exception _e ) {
				MessageBox( "An error occurred while opening the image:\n\n", _e );
			}
		}

		private void	Generate() {
			try {
				panelParameters.Enabled = false;

				//////////////////////////////////////////////////////////////////////////
				// 1] Apply bilateral filtering to the input texture as a pre-process
				ApplyBilateralFiltering( m_TextureSource, m_TextureTarget0, floatTrackbarControlBilateralRadius.Value, floatTrackbarControlBilateralTolerance.Value, checkBoxWrap.Checked );


				//////////////////////////////////////////////////////////////////////////
				// 2] Compute directional occlusion
				m_TextureTarget1.RemoveFromLastAssignedSlots();

				// Prepare computation parameters
				m_TextureTarget0.SetCS( 0 );
				m_TextureTarget1.SetCSUAV( 0 );
				m_SB_Rays.SetInput( 1 );

				m_CB_Input.m.RaysCount = (UInt32) Math.Min( MAX_THREADS, integerTrackbarControlRaysCount.Value );
				m_CB_Input.m.MaxStepsCount = (UInt32) integerTrackbarControlMaxStepsCount.Value;
				m_CB_Input.m.Tile = (uint) (checkBoxWrap.Checked ? 1 : 0);
				m_CB_Input.m.TexelSize_mm = TextureSize_mm / Math.Max( W, H );
				m_CB_Input.m.Displacement_mm = TextureHeight_mm;

				// Start
				if ( !m_CS_GenerateSSBumpMap.Use() )
					throw new Exception( "Can't generate self-shadowed bump map as compute shader failed to compile!" );

				int	h = Math.Max( 1, MAX_LINES*1024 / W );
				int	CallsCount = (int) Math.Ceiling( (float) H / h );
				for ( int i=0; i < CallsCount; i++ )
				{
					m_CB_Input.m.Y0 = (UInt32) (i * h);
					m_CB_Input.UpdateData();

					m_CS_GenerateSSBumpMap.Dispatch( W, h, 1 );

					m_Device.Present( true );

					progressBar.Value = (int) (0.01f * (BILATERAL_PROGRESS + (100-BILATERAL_PROGRESS) * (i+1) / (CallsCount)) * progressBar.Maximum);
//					for ( int a=0; a < 10; a++ )
						Application.DoEvents();
				}

				m_TextureTarget1.RemoveFromLastAssignedSlotUAV();	// So we can use it as input for next stage

				progressBar.Value = progressBar.Maximum;

				// Compute in a single shot (this is madness!)
// 				m_CB_Input.m.y = 0;
// 				m_CB_Input.UpdateData();
// 				m_CS_GenerateSSBumpMap.Dispatch( W, H, 1 );


				//////////////////////////////////////////////////////////////////////////
				// 3] Copy target to staging for CPU readback and update the resulting bitmap
				m_TextureTarget_CPU.CopyFrom( m_TextureTarget1 );

				if ( m_BitmapResult != null )
					m_BitmapResult.Dispose();
				m_BitmapResult = null;
				m_BitmapResult = new ImageUtility.Bitmap( W, H, m_LinearProfile );
				m_BitmapResult.HasAlpha = true;

				RendererManaged.PixelsBuffer	Pixels = m_TextureTarget_CPU.Map( 0, 0 );
				using ( System.IO.BinaryReader R = Pixels.OpenStreamRead() )
					for ( int Y=0; Y < H; Y++ )
					{
						R.BaseStream.Position = Y * Pixels.RowPitch;
						for ( int X=0; X < W; X++ ) {
							float	AO = R.ReadSingle();
							ImageUtility.float4	Color = new ImageUtility.float4( AO, AO, AO, AO );
							Color = m_LinearProfile.RGB2XYZ( Color );
							m_BitmapResult.ContentXYZ[X,Y] = Color;
						}
					}

				Pixels.Dispose();
				m_TextureTarget_CPU.UnMap( 0, 0 );

				// Assign result
				viewportPanelResult.Image = m_BitmapResult;

			} catch ( Exception _e ) {
				MessageBox( "An error occurred during generation!\r\n\r\nDetails: ", _e );
			} finally {
				panelParameters.Enabled = true;
			}
		}

		private void	ApplyBilateralFiltering( RendererManaged.Texture2D _Source, RendererManaged.Texture2D _Target, float _BilateralRadius, float _BilateralTolerance, bool _Wrap ) {
			_Source.SetCS( 0 );
			_Target.SetCSUAV( 0 );

			m_CB_Filter.m.Radius = _BilateralRadius;
			m_CB_Filter.m.Tolerance = _BilateralTolerance;
			m_CB_Filter.m.Tile = (uint) (_Wrap ? 1 : 0);

			m_CS_BilateralFilter.Use();

			int	h = Math.Max( 1, MAX_LINES*1024 / W );
			int	CallsCount = (int) Math.Ceiling( (float) H / h );
			for ( int i=0; i < CallsCount; i++ )
			{
				m_CB_Filter.m.Y0 = (UInt32) (i * h);
				m_CB_Filter.UpdateData();

				m_CS_BilateralFilter.Dispatch( W, h, 1 );

				m_Device.Present( true );

				progressBar.Value = (int) (0.01f * (0 + BILATERAL_PROGRESS * (i+1) / CallsCount) * progressBar.Maximum);
//				for ( int a=0; a < 10; a++ )
					Application.DoEvents();
			}

			// Single gulp (crashes the driver on large images!)
//			m_CS_BilateralFilter.Dispatch( W, H, 1 );

			_Target.RemoveFromLastAssignedSlotUAV();	// So we can use it as input for next stage
		}

		private void	GenerateRays( int _RaysCount, float _MaxConeAngle, RendererManaged.StructuredBuffer<RendererManaged.float3> _Target ) {
			_RaysCount = Math.Min( MAX_THREADS, _RaysCount );

			WMath.Hammersley	hammersley = new WMath.Hammersley();
			double[,]			sequence = hammersley.BuildSequence( _RaysCount, 2 );
			WMath.Vector[]		rays = hammersley.MapSequenceToSphere( sequence, 0.5f * _MaxConeAngle );
			for ( int RayIndex=0; RayIndex < _RaysCount; RayIndex++ ) {
				WMath.Vector	ray = rays[RayIndex];

				// Scale the ray so we ensure to always walk at least a texel in the texture
				float	SinTheta = (float) Math.Sqrt( 1.0 - ray.y * ray.y );
				float	LengthFactor = 1.0f / SinTheta;	// The ray is scaled so we ensure we always walk at least a texel in the texture
				ray *= LengthFactor;

				_Target.m[RayIndex].Set( ray.x, -ray.z, ray.y );
			}

			_Target.Write();
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
			return System.Windows.Forms.MessageBox.Show( this, _Text, "Ambient Occlusion Map Generator", _Buttons, _Icon );
		}

		#endregion 

		#endregion

		#region EVENT HANDLERS

 		private unsafe void buttonGenerate_Click( object sender, EventArgs e )
 		{
 			Generate();
//			Generate_CPU( integerTrackbarControlRaysCount.Value );
		}

		private void integerTrackbarControlRaysCount_SliderDragStop( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _StartValue )
		{
			GenerateRays( _Sender.Value, floatTrackbarControlMaxConeAngle.Value * (float) (Math.PI / 180.0), m_SB_Rays );
		}

		private void radioButtonShowDirOccRGB_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				viewportPanelResult.ViewMode = ImagePanel.VIEW_MODE.RGB;
		}

		private void radioButtonDirOccRGBtimeAO_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				viewportPanelResult.ViewMode = ImagePanel.VIEW_MODE.RGB_AO;
		}

		private void radioButtonDirOccR_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				viewportPanelResult.ViewMode = ImagePanel.VIEW_MODE.R;
		}

		private void radioButtonDirOccG_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				viewportPanelResult.ViewMode = ImagePanel.VIEW_MODE.G;
		}

		private void radioButtonDirOccB_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				viewportPanelResult.ViewMode = ImagePanel.VIEW_MODE.B;
		}

		private void radioButton1_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				viewportPanelResult.ViewMode = ImagePanel.VIEW_MODE.AO;
		}

		private void radioButtonAOfromRGB_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				viewportPanelResult.ViewMode = ImagePanel.VIEW_MODE.AO_FROM_RGB;
		}

		private unsafe void viewportPanelResult_Click( object sender, EventArgs e )
		{
			if ( m_BitmapResult == null )
			{
				MessageBox( "There is no result image to save!" );
				return;
			}

			string	SourceFileName = m_SourceFileName.FullName;
			string	TargetFileName = System.IO.Path.Combine( System.IO.Path.GetDirectoryName( SourceFileName ), System.IO.Path.GetFileNameWithoutExtension( SourceFileName ) + "_ssbump.png" );

			saveFileDialogImage.InitialDirectory = System.IO.Path.GetFullPath( TargetFileName );
			saveFileDialogImage.FileName = System.IO.Path.GetFileName( TargetFileName );
			if ( saveFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			try
			{
				m_BitmapResult.Save( new System.IO.FileInfo( saveFileDialogImage.FileName ) );

				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while saving the image:\n\n", _e );
			}
		}

		private void outputPanelInputHeightMap_Click( object sender, EventArgs e )
		{
			string	OldFileName = GetRegKey( "DatabaseFileName", System.IO.Path.Combine( m_ApplicationPath, "Example.jpg" ) );
			openFileDialogImage.InitialDirectory = System.IO.Path.GetFullPath( OldFileName );
			openFileDialogImage.FileName = System.IO.Path.GetFileName( OldFileName );
			if ( openFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "DatabaseFileName", openFileDialogImage.FileName );

			LoadHeightMap( new System.IO.FileInfo( openFileDialogImage.FileName ) );
		}

		private string	m_DraggedFileName = null;
		private void outputPanelInputHeightMap_DragEnter( object sender, DragEventArgs e )
		{
			m_DraggedFileName = null;
			if ( (e.AllowedEffect & DragDropEffects.Copy) != DragDropEffects.Copy )
				return;

			Array	data = ((IDataObject) e.Data).GetData( "FileNameW" ) as Array;
			if ( data == null || data.Length != 1 )
				return;
			if ( !(data.GetValue(0) is String) )
				return;

			string	DraggedFileName = (data as string[])[0];

			string	Extension = System.IO.Path.GetExtension( DraggedFileName ).ToLower();
			if (	Extension == ".jpg"
				||	Extension == ".jpeg"
				||	Extension == ".png"
				||	Extension == ".tga"
				||	Extension == ".bmp"
				||	Extension == ".tif"
				||	Extension == ".tiff"
				||	Extension == ".hdr"
				||	Extension == ".crw"
				||	Extension == ".dng"
				)
			{
				m_DraggedFileName = DraggedFileName;	// Supported!
				e.Effect = DragDropEffects.Copy;
			}
		}

		private void outputPanelInputHeightMap_DragDrop( object sender, DragEventArgs e )
		{
			if ( m_DraggedFileName != null )
				LoadHeightMap( new System.IO.FileInfo( m_DraggedFileName ) );
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			m_Device.ReloadModifiedShaders();
		}

		#endregion
	}
}
