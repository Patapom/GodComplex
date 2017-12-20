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

using SharpMath;

namespace GenerateSelfShadowedBumpMap
{
	public partial class GeneratorForm : Form {
		#region CONSTANTS

		private const uint		MAX_THREADS = 1024;			// Maximum threads run by the compute shader

		private const int		BILATERAL_PROGRESS = 50;	// Bilateral filtering is considered as this % of the total task (bilateral is quite long so I decided it was equivalent to 50% of the complete computation task)
		private const uint		MAX_LINES = 16;				// Process at most that amount of lines of a 4096x4096 image for a single dispatch

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
// 			public float	Radius;				// Radius of the bilateral filter
// 			public float	Tolerance;			// Range tolerance of the bilateral filter
			public float	Sigma_Radius;		// Radius of the bilateral filter
			public float	Sigma_Tolerance;	// Range tolerance of the bilateral filter
			public UInt32	Tile;				// Tiling flag
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		[System.Diagnostics.DebuggerDisplay( "({sourcePixelIndex&0xFFFF},{sourcePixelIndex>>16}) - ({targetPixelIndex&0xFFFF},{targetPixelIndex>>16})" )]
		private struct	SBPixel {
			public uint		sourcePixelIndex;	// Index of the pixel that issued this entry
			public uint		targetPixelIndex;	// Index of the pixel that is perceived by the source pixel
		}

		#endregion

		#region FIELDS

		private RegistryKey							m_AppKey;
		private string								m_ApplicationPath;

		private System.IO.FileInfo					m_sourceFileName = null;
		private uint								W, H;
		private ImageUtility.ImageFile				m_imageSourceHeight = null;
		private ImageUtility.ImageFile				m_imageSourceNormal = null;

		internal Renderer.Device					m_device = new Renderer.Device();
		internal Renderer.Texture2D					m_textureSourceHeightMap = null;
		internal Renderer.Texture2D					m_TextureSourceNormal = null;
		internal Renderer.Texture2D					m_textureTarget0 = null;
		internal Renderer.Texture2D					m_textureTarget1 = null;
		internal Renderer.Texture2D					m_textureTarget_CPU = null;

		// AO Generation
		private Renderer.ConstantBuffer<CBInput>	m_CB_Input;
		private Renderer.StructuredBuffer<float3>	m_SB_Rays = null;
		private Renderer.ComputeShader				m_CS_GenerateAOMap = null;

		// Bilateral filtering pre-processing
		private Renderer.ConstantBuffer<CBFilter>	m_CB_Filter;
		private Renderer.ComputeShader				m_CS_BilateralFilter = null;

 		private ImageUtility.ColorProfile			m_profilesRGB = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB );
		private ImageUtility.ColorProfile			m_profileLinear = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.LINEAR );
		private ImageUtility.ImageFile				m_imageResult = null;

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

		public unsafe GeneratorForm() {
			InitializeComponent();

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\TestGroundTruthAOFitting" );
			m_ApplicationPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );

// N.Silvagni test with Adobe RGB
//			ImageUtility.Bitmap	Test = new ImageUtility.Bitmap( 1, 1, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
//			Test.ContentXYZ[0,0] = Test.Profile.RGB2XYZ( new ImageUtility.float4( 0, 1, 0, 1 ) );
//			ImageUtility.ColorProfile	AdobeProfile = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.ADOBE_RGB_D65 );
//			ImageUtility.ColorProfile	AdobeProfile2 = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.ADOBE_RGB_D50 );
//
//
//			ImageUtility.float4	Glou0 = AdobeProfile.XYZ2RGB( Test.ContentXYZ[0,0] );
//			ImageUtility.float4	Glou1 = AdobeProfile2.XYZ2RGB( Test.ContentXYZ[0,0] );
//			ImageUtility.float4	Glou2 = Test.Profile.XYZ2RGB( Test.ContentXYZ[0,0] );

			#if DEBUG
				buttonReload.Visible = true;
			#endif
		}

		protected override void  OnLoad(EventArgs e) {
 			base.OnLoad(e);

			try {
				m_device.Init( viewportPanelResult.Handle, false, true );

				// Create our compute shaders
				#if !DEBUG
					using ( Renderer.ScopedForceShadersLoadFromBinary scope = new Renderer.ScopedForceShadersLoadFromBinary() )
				#endif
				{
					m_CS_BilateralFilter = new Renderer.ComputeShader( m_device, new System.IO.FileInfo( "./Shaders/BilateralFiltering.hlsl" ), "CS", null );
					m_CS_GenerateAOMap = new Renderer.ComputeShader( m_device, new System.IO.FileInfo( "./Shaders/GenerateAOMap.hlsl" ), "CS", null );
				}

				// Create our constant buffers
				m_CB_Input = new Renderer.ConstantBuffer<CBInput>( m_device, 0 );
				m_CB_Filter = new Renderer.ConstantBuffer<CBFilter>( m_device, 0 );

				// Create our structured buffer containing the rays
				m_SB_Rays = new Renderer.StructuredBuffer<float3>( m_device, MAX_THREADS, true, false );
				integerTrackbarControlRaysCount_SliderDragStop( integerTrackbarControlRaysCount, 0 );

				// Create the default, planar normal map
				clearNormalToolStripMenuItem_Click( null, EventArgs.Empty );

			} catch ( Exception _e ) {
				MessageBox( "Failed to create DX11 device and default shaders:\r\n", _e );
				Close();
			}



LoadHeightMap( new System.IO.FileInfo( GetRegKey( "HeightMapFileName", System.IO.Path.Combine( m_ApplicationPath, "Example.jpg" ) ) ) );
LoadNormalMap( new System.IO.FileInfo( GetRegKey( "NormalMapFileName", System.IO.Path.Combine( m_ApplicationPath, "Example.jpg" ) ) ) );
Generate();

		}

		protected override void OnClosing( CancelEventArgs e )
		{
			try {
				m_CS_GenerateAOMap.Dispose();
				m_CS_BilateralFilter.Dispose();

				m_SB_Rays.Dispose();

				m_CB_Filter.Dispose();
				m_CB_Input.Dispose();

				if ( m_textureTarget_CPU != null )
					m_textureTarget_CPU.Dispose();
				if ( m_textureTarget1 != null )
					m_textureTarget1.Dispose();
				if ( m_textureTarget0 != null )
					m_textureTarget0.Dispose();
				if ( m_TextureSourceNormal != null )
					m_TextureSourceNormal.Dispose();
				if ( m_textureSourceHeightMap != null )
					m_textureSourceHeightMap.Dispose();

				m_device.Dispose();
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

		/// <summary>
		/// Arguments-driven generation
		/// </summary>
		/// 
		public class	BuildArguments {
			public string	heightMapFileName = null;
			public string	normalMapFileName = null;
			public string	AOMapFileName = null;
			public float	textureSize_cm = 100.0f;
			public float	displacementSize_cm = 45.0f;
			public int		raysCount = 1024;
			public int		searchRange = 200;
			public float	coneAngle = 160.0f;
			public bool		tile = true;
			public float	bilateralRadius = 1.0f;
			public float	bilateralTolerance = 0.2f;

		}

		bool	m_silentMode = false;
		public void		Build( BuildArguments _args ) {

			// Setup arguments
			floatTrackbarControlHeight.Value = _args.displacementSize_cm;
			floatTrackbarControlPixelDensity.Value = _args.textureSize_cm;

			integerTrackbarControlRaysCount.Value = _args.raysCount;
			integerTrackbarControlMaxStepsCount.Value = _args.searchRange;
			floatTrackbarControlMaxConeAngle.Value = _args.coneAngle;

			floatTrackbarControlBilateralRadius.Value = _args.bilateralRadius;
			floatTrackbarControlBilateralTolerance.Value = _args.bilateralTolerance;

			checkBoxWrap.Checked = _args.tile;

			m_silentMode = true;

			// Create device, shaders and structures
			OnLoad( EventArgs.Empty );

			// Load height map
			System.IO.FileInfo	HeightMapFileName = new System.IO.FileInfo( _args.heightMapFileName );
			LoadHeightMap( HeightMapFileName );

			// Load normal map
			if ( _args.normalMapFileName != null ) {
				System.IO.FileInfo	NormalMapFileName = new System.IO.FileInfo( _args.normalMapFileName );
				LoadNormalMap( NormalMapFileName );
			}

			// Generate
 			Generate();

			// Save results
			m_imageResult.Save( new System.IO.FileInfo( _args.AOMapFileName ) );

			// Dispose
			CancelEventArgs	onsenfout = new CancelEventArgs();
			OnClosing( (CancelEventArgs) onsenfout );
		}

		private void	LoadHeightMap( System.IO.FileInfo _FileName ) {
			try {
				panelParameters.Enabled = false;

				// Dispose of existing resources
				if ( m_imageSourceHeight != null )
					m_imageSourceHeight.Dispose();
				m_imageSourceHeight = null;

				if ( m_textureTarget_CPU != null )
					m_textureTarget_CPU.Dispose();
				m_textureTarget_CPU = null;
				if ( m_textureTarget0 != null )
					m_textureTarget0.Dispose();
				m_textureTarget0 = null;
				if ( m_textureTarget1 != null )
					m_textureTarget1.Dispose();
				m_textureTarget1 = null;
				if ( m_textureSourceHeightMap != null )
					m_textureSourceHeightMap.Dispose();
				m_textureSourceHeightMap = null;

				// Load the source image
				// Assume it's in linear space
				m_sourceFileName = _FileName;
				m_imageSourceHeight = new ImageUtility.ImageFile( _FileName );
				outputPanelInputHeightMap.Bitmap = m_imageSourceHeight.AsBitmap;

				W = m_imageSourceHeight.Width;
				H = m_imageSourceHeight.Height;

				// Build the source texture
				float4[]	scanline = new float4[W];

				Renderer.PixelsBuffer	SourceHeightMap = new Renderer.PixelsBuffer( W*H*4 );
				using ( System.IO.BinaryWriter Wr = SourceHeightMap.OpenStreamWrite() )
					for ( uint Y=0; Y < H; Y++ ) {
						m_imageSourceHeight.ReadScanline( Y, scanline );
						for ( uint X=0; X < W; X++ ) {
							Wr.Write( scanline[X].y );
						}
					}

				// Build the target UAV & staging texture for readback
				m_textureTarget0 = new Renderer.Texture2D( m_device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, null );
				m_textureTarget1 = new Renderer.Texture2D( m_device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, null );
				m_textureTarget_CPU = new Renderer.Texture2D( m_device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, true, false, null );

				m_textureSourceHeightMap = new Renderer.Texture2D( m_device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new Renderer.PixelsBuffer[] { SourceHeightMap } );

				panelParameters.Enabled = true;
				buttonGenerate.Focus();

			} catch ( Exception _e ) {
				MessageBox( "An error occurred while opening the image:\n\n", _e );
			}
		}

		private void	LoadNormalMap( System.IO.FileInfo _FileName ) {
			try {
				// Dispose of existing resources
				if ( m_imageSourceNormal != null )
					m_imageSourceNormal.Dispose();
				m_imageSourceNormal = null;

				if ( m_TextureSourceNormal != null )
					m_TextureSourceNormal.Dispose();
				m_TextureSourceNormal = null;

				// Load the source image
				// Assume it's in linear space (all normal maps should be in linear space, with the default value being (0.5, 0.5, 1))
				m_imageSourceNormal = new ImageUtility.ImageFile( _FileName );
				imagePanelNormalMap.Bitmap = m_imageSourceNormal.AsBitmap;

				uint	W = m_imageSourceNormal.Width;
				uint	H = m_imageSourceNormal.Height;

				// Build the source texture
				float4[]	scanline = new float4[W];

				Renderer.PixelsBuffer	SourceNormalMap = new Renderer.PixelsBuffer( W*H*4*4 );
				using ( System.IO.BinaryWriter Wr = SourceNormalMap.OpenStreamWrite() )
					for ( int Y=0; Y < H; Y++ ) {
						m_imageSourceNormal.ReadScanline( (uint) Y, scanline );
						for ( int X=0; X < W; X++ ) {
							float	Nx = 2.0f * scanline[X].x - 1.0f;
							float	Ny = 1.0f - 2.0f * scanline[X].y;
							float	Nz = 2.0f * scanline[X].z - 1.0f;
							Wr.Write( Nx );
							Wr.Write( Ny );
							Wr.Write( Nz );
							Wr.Write( 1.0f );
						}
					}

				m_TextureSourceNormal = new Renderer.Texture2D( m_device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new Renderer.PixelsBuffer[] { SourceNormalMap } );

			} catch ( Exception _e ) {
				MessageBox( "An error occurred while opening the image:\n\n", _e );
			}
		}

		private void	Generate() {
			try {
				panelParameters.Enabled = false;

				//////////////////////////////////////////////////////////////////////////
				// 1] Apply bilateral filtering to the input texture as a pre-process
				ApplyBilateralFiltering( m_textureSourceHeightMap, m_textureTarget0, floatTrackbarControlBilateralRadius.Value, floatTrackbarControlBilateralTolerance.Value, checkBoxWrap.Checked, BILATERAL_PROGRESS );


				//////////////////////////////////////////////////////////////////////////
				// 2] Compute directional occlusion
				m_textureTarget1.RemoveFromLastAssignedSlots();

				// Prepare computation parameters
				m_textureTarget0.SetCS( 0 );
				m_textureTarget1.SetCSUAV( 0 );
				m_SB_Rays.SetInput( 1 );
				m_TextureSourceNormal.SetCS( 2 );

				// Create the counter & indirect pixel buffers
				Renderer.StructuredBuffer<SBPixel>	SB_IndirectPixelsStack = new Renderer.StructuredBuffer<SBPixel>( m_device, MAX_LINES*1024*1024, true );	// A lot!! :'(
				Renderer.StructuredBuffer<uint>		SB_IndirectPixelsCounter = new Renderer.StructuredBuffer<uint>( m_device, 1, true );

				SB_IndirectPixelsCounter.SetOutput( 1 );
				SB_IndirectPixelsStack.SetOutput( 2 );

				m_CB_Input.m.RaysCount = (UInt32) Math.Min( MAX_THREADS, integerTrackbarControlRaysCount.Value );
				m_CB_Input.m.MaxStepsCount = (UInt32) integerTrackbarControlMaxStepsCount.Value;
				m_CB_Input.m.Tile = (uint) (checkBoxWrap.Checked ? 1 : 0);
				m_CB_Input.m.TexelSize_mm = TextureSize_mm / Math.Max( W, H );
				m_CB_Input.m.Displacement_mm = TextureHeight_mm;

				// Start
				if ( !m_CS_GenerateAOMap.Use() )
					throw new Exception( "Can't generate self-shadowed bump map as compute shader failed to compile!" );

				uint	h = Math.Max( 1, MAX_LINES*1024 / W );
				uint	callsCount = (uint) Math.Ceiling( (float) H / h );

				uint	totalIndirectPixelsCount = 0;
				uint[][]listOfListsOfIndirectPixels = new uint[callsCount][];
				uint[,]	offsetsInListOfIndirectPixels = new uint[W,H];
				uint[,]	countersInListOfIndirectPixels = new uint[W,H];

				for ( uint callIndex=0; callIndex < callsCount; callIndex++ ) {
					uint	Y0 = callIndex * h;
					m_CB_Input.m.Y0 = Y0;
					m_CB_Input.UpdateData();

					SB_IndirectPixelsCounter.m[0] = 0;	// Reset stack counter
					SB_IndirectPixelsCounter.Write();

					m_CS_GenerateAOMap.Dispatch( W, h, 1 );

//*					// Read back and accumulate indirect pixels
					SB_IndirectPixelsCounter.Read();
					SB_IndirectPixelsStack.Read();
					uint	indirectPixelsStackSize = SB_IndirectPixelsCounter.m[0];
					totalIndirectPixelsCount += indirectPixelsStackSize;

					uint[]	newList = new uint[indirectPixelsStackSize];	// Grow the list of indirect pixels
					listOfListsOfIndirectPixels[callIndex] = newList;

					// Browse once to count how many indirect pixels affect our source pixels (establishes the size of source pixel lists)
					for ( uint i=0; i < indirectPixelsStackSize; i++ ) {
						SBPixel	packedIndirectPixel = SB_IndirectPixelsStack.m[i];
						uint	sourcePixelPositionX = packedIndirectPixel.sourcePixelIndex & 0xFFFFU;
						uint	sourcePixelPositionY = packedIndirectPixel.sourcePixelIndex >> 16;

						countersInListOfIndirectPixels[sourcePixelPositionX,sourcePixelPositionY]++;
					}

					// Accumulate size of source pixel lists to establish final list offsets
					uint	Y1 = Math.Min( H, Y0+h );
					uint	currentOffset = 0;
					for ( uint Y=Y0; Y < Y1; Y++ ) {
						for ( uint X=0; X < W; X++ ) {
							offsetsInListOfIndirectPixels[X,Y] = currentOffset;
							currentOffset += countersInListOfIndirectPixels[X,Y];	// Leave room for that pixel's list of indirect pixel entries
							countersInListOfIndirectPixels[X,Y] = 0;				// Reset counter so we can use it to start counting again
						}
					}

					// Browse a second time to accumulate
					for ( uint i=0; i < indirectPixelsStackSize; i++ ) {
						SBPixel	packedIndirectPixel = SB_IndirectPixelsStack.m[i];
						uint	sourcePixelPositionX = packedIndirectPixel.sourcePixelIndex & 0xFFFFU;
						uint	sourcePixelPositionY = packedIndirectPixel.sourcePixelIndex >> 16;

						uint	sourcePixelOffset = offsetsInListOfIndirectPixels[sourcePixelPositionX,sourcePixelPositionY];	// Finally indicates where to start storing target pixels
						uint	sourcePixelCounter = countersInListOfIndirectPixels[sourcePixelPositionX,sourcePixelPositionY];	// Current size of source pixel list
						newList[sourcePixelOffset+sourcePixelCounter] = packedIndirectPixel.targetPixelIndex;					// Store target pixel link into source pixel list
						countersInListOfIndirectPixels[sourcePixelPositionX,sourcePixelPositionY]++;							// Increment size of source pixel list
					}
//*/
//					m_device.Present( true );

					progressBar.Value = (int) (0.01f * (BILATERAL_PROGRESS + (100-BILATERAL_PROGRESS) * (callIndex+1) / callsCount) * progressBar.Maximum);
//					for ( int a=0; a < 10; a++ )
						Application.DoEvents();
				}

				m_textureTarget1.RemoveFromLastAssignedSlotUAV();	// So we can use it as input for next stage

				progressBar.Value = progressBar.Maximum;

				// Compute in a single shot (this is madness!)
// 				m_CB_Input.m.y = 0;
// 				m_CB_Input.UpdateData();
// 				m_CS_GenerateSSBumpMap.Dispatch( W, H, 1 );


				SB_IndirectPixelsCounter.Dispose();
				SB_IndirectPixelsStack.Dispose();


				//////////////////////////////////////////////////////////////////////////
				// 3] Copy target to staging for CPU readback and update the resulting bitmap
				m_textureTarget_CPU.CopyFrom( m_textureTarget1 );

//				ImageUtility.ColorProfile	profile = m_profilesRGB;	// AO maps are sRGB! (although strange, that's certainly to have more range in dark values)
				ImageUtility.ColorProfile	profile = m_profileLinear;

				float3	whitePoint_xyY = new float3( profile.Chromas.White, 0 );
				float3	whitePoint_XYZ = new float3();

				ImageUtility.Bitmap		tempBitmap = new ImageUtility.Bitmap( W, H );
				Renderer.PixelsBuffer	Pixels = m_textureTarget_CPU.MapRead( 0, 0 );
				using ( System.IO.BinaryReader R = Pixels.OpenStreamRead() )
					for ( uint Y=0; Y < H; Y++ ) {
						R.BaseStream.Position = Y * Pixels.RowPitch;
						for ( uint X=0; X < W; X++ ) {
							whitePoint_xyY.z = R.ReadSingle();		// Linear value
							ImageUtility.ColorProfile.xyY2XYZ( whitePoint_xyY, ref whitePoint_XYZ );
							tempBitmap[X,Y] = new float4( whitePoint_XYZ, 1 );
						}
					}

				m_textureTarget_CPU.UnMap( Pixels );

				// Convert to RGB
				ImageUtility.ImageFile	temmpImageRGBA32F = new ImageUtility.ImageFile();
				tempBitmap.ToImageFile( temmpImageRGBA32F, profile );

				if ( m_imageResult == null )
					m_imageResult = new ImageUtility.ImageFile();
				m_imageResult.ToneMapFrom( temmpImageRGBA32F, ( float3 _HDR, ref float3 _LDR ) => {
					_LDR = _HDR;	// Return as-is..
				} );

				// Assign result
				viewportPanelResult.Bitmap = m_imageResult.AsBitmap;


//*				//////////////////////////////////////////////////////////////////////////
				// 4] Store raw binary data
				System.IO.FileInfo	binaryDataFileName = new System.IO.FileInfo( System.IO.Path.GetFileNameWithoutExtension( m_sourceFileName.FullName ) + ".indirectMap" );
				using ( System.IO.FileStream S = binaryDataFileName.Create() )
					using ( System.IO.BinaryWriter Wr = new System.IO.BinaryWriter( S ) ) {

						// 4.1) We start by writing a WxH array of (AO,offset,count) triplets
						Wr.Write( W );
						Wr.Write( H );

						uint	currentIndirectPixelsCount = 0;
						for ( uint callIndex=0; callIndex < callsCount; callIndex++ ) {
							uint	Y0 = callIndex * h;
							uint	Y1 = Math.Min( H, Y0+h );

							uint[]	listOfIndirectPixels = listOfListsOfIndirectPixels[callIndex];
							for ( uint Y=Y0; Y < Y1; Y++ ) {
								for ( uint X=0; X < W; X++ ) {
									Wr.Write( tempBitmap[X,Y].z );

									uint	localListOffset = offsetsInListOfIndirectPixels[X,Y];
									uint	globalListOffset = currentIndirectPixelsCount + localListOffset;
									Wr.Write( globalListOffset );
									uint	localListCounter = countersInListOfIndirectPixels[X,Y];
									Wr.Write( globalListOffset );
								}
							}

							currentIndirectPixelsCount += (uint) listOfIndirectPixels.Length;
						}

						// 4.2) Then we write the full list of indirect pixels
						for ( uint callIndex=0; callIndex < callsCount; callIndex++ ) {
							uint[]	listOfIndirectPixels = listOfListsOfIndirectPixels[callIndex];
							uint	length = (uint) listOfIndirectPixels.Length;
							for ( uint i=0; i < length; i++ )
								Wr.Write( listOfIndirectPixels[i] );
						}
					}
//*/
			} catch ( Exception _e ) {
				MessageBox( "An error occurred during generation!\r\n\r\nDetails: ", _e );
			} finally {
				panelParameters.Enabled = true;
			}
		}

		private void	ApplyBilateralFiltering( Renderer.Texture2D _Source, Renderer.Texture2D _Target, float _BilateralRadius, float _BilateralTolerance, bool _Wrap, int _ProgressBarMax ) {
			_Source.SetCS( 0 );
			_Target.SetCSUAV( 0 );

// 			m_CB_Filter.m.Radius = _BilateralRadius;
// 			m_CB_Filter.m.Tolerance = _BilateralTolerance;
			m_CB_Filter.m.Sigma_Radius = (float) (-0.5 * Math.Pow( _BilateralRadius / 3.0f, -2.0 ));
			m_CB_Filter.m.Sigma_Tolerance = _BilateralTolerance > 0.0f ? (float) (-0.5 * Math.Pow( _BilateralTolerance, -2.0 )) : -1e6f;
			m_CB_Filter.m.Tile = (uint) (_Wrap ? 1 : 0);

			m_CS_BilateralFilter.Use();

			uint	h = Math.Max( 1, MAX_LINES*1024 / W );
			uint	CallsCount = (uint) Math.Ceiling( (float) H / h );
			for ( uint i=0; i < CallsCount; i++ ) {
				m_CB_Filter.m.Y0 = (UInt32) (i * h);
				m_CB_Filter.UpdateData();

				m_CS_BilateralFilter.Dispatch( W, h, 1 );

				m_device.Present( true );

				progressBar.Value = (int) (0.01f * (0 + _ProgressBarMax * (i+1) / CallsCount) * progressBar.Maximum);
//				for ( int a=0; a < 10; a++ )
					Application.DoEvents();
			}

			// Single gulp (crashes the driver on large images!)
//			m_CS_BilateralFilter.Dispatch( W, H, 1 );

			_Target.RemoveFromLastAssignedSlotUAV();	// So we can use it as input for next stage
		}

		private void	GenerateRays( int _RaysCount, float _MaxConeAngle, Renderer.StructuredBuffer<float3> _Target ) {
			_RaysCount = Math.Min( (int) MAX_THREADS, _RaysCount );

			Hammersley	hammersley = new Hammersley();
			double[,]	sequence = hammersley.BuildSequence( _RaysCount, 2 );
			float3[]	rays = hammersley.MapSequenceToSphere( sequence, 0.5f * _MaxConeAngle );
			for ( int RayIndex=0; RayIndex < _RaysCount; RayIndex++ ) {
				float3	ray = rays[RayIndex];

// 				// Scale the ray so we ensure to always walk at least a texel in the texture
// 				float	SinTheta = (float) Math.Sqrt( 1.0 - ray.y * ray.y );
// 				float	LengthFactor = 1.0f / SinTheta;
// 				ray *= LengthFactor;

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
			if ( m_silentMode )
				throw new Exception( _Text );

			return System.Windows.Forms.MessageBox.Show( this, _Text, "Ambient Occlusion Map Generator", _Buttons, _Icon );
		}

		#endregion 

		#endregion

		#region EVENT HANDLERS

 		private unsafe void buttonGenerate_Click( object sender, EventArgs e ) {
 			Generate();
//			Generate_CPU( integerTrackbarControlRaysCount.Value );
		}

		private void buttonTestBilateral_Click( object sender, EventArgs e ) {
			try {
				panelParameters.Enabled = false;

				//////////////////////////////////////////////////////////////////////////
				// 1] Apply bilateral filtering to the input texture as a pre-process
				ApplyBilateralFiltering( m_textureSourceHeightMap, m_textureTarget0, floatTrackbarControlBilateralRadius.Value, floatTrackbarControlBilateralTolerance.Value, checkBoxWrap.Checked, 100 );

				progressBar.Value = progressBar.Maximum;

				//////////////////////////////////////////////////////////////////////////
				// 2] Copy target to staging for CPU readback and update the resulting bitmap
				m_textureTarget_CPU.CopyFrom( m_textureTarget0 );

				ImageUtility.Bitmap		tempBitmap = new ImageUtility.Bitmap( W, H );
				Renderer.PixelsBuffer	pixels = m_textureTarget_CPU.MapRead( 0, 0 );
				using ( System.IO.BinaryReader R = pixels.OpenStreamRead() )
					for ( uint Y=0; Y < H; Y++ ) {
						R.BaseStream.Position = Y * pixels.RowPitch;
						for ( uint X=0; X < W; X++ ) {
							float	AO = R.ReadSingle();
							tempBitmap[X,Y] = new float4( AO, AO, AO, 1 );
						}
					}

				m_textureTarget_CPU.UnMap( pixels );

				// Convert to RGB
//				ImageUtility.ColorProfile	Profile = m_ProfilesRGB;	// AO maps are sRGB! (although strange, that's certainly to have more range in dark values)
				ImageUtility.ColorProfile	Profile = m_profilesRGB;	// AO maps are sRGB! (although strange, that's certainly to have more range in dark values)

				if ( m_imageResult != null )
					m_imageResult.Dispose();
				m_imageResult = new ImageUtility.ImageFile();
				tempBitmap.ToImageFile( m_imageResult, Profile );

				// Assign result
				viewportPanelResult.Bitmap = m_imageResult.AsCustomBitmap( ( ref float4 _color ) => {} );

			} catch ( Exception _e ) {
				MessageBox( "An error occurred during generation!\r\n\r\nDetails: ", _e );
			} finally {
				panelParameters.Enabled = true;
			}
		}

		private void integerTrackbarControlRaysCount_SliderDragStop( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _StartValue )
		{
			GenerateRays( _Sender.Value, floatTrackbarControlMaxConeAngle.Value * (float) (Math.PI / 180.0), m_SB_Rays );
		}

		private void floatTrackbarControlMaxConeAngle_SliderDragStop( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fStartValue )
		{
			GenerateRays( integerTrackbarControlRaysCount.Value, floatTrackbarControlMaxConeAngle.Value * (float) (Math.PI / 180.0), m_SB_Rays );
		}

		private void checkBoxViewsRGB_CheckedChanged( object sender, EventArgs e )
		{
			viewportPanelResult.ViewLinear = !checkBoxViewsRGB.Checked;
		}

		private void floatTrackbarControlBrightness_SliderDragStop( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fStartValue )
		{
			viewportPanelResult.Brightness = _Sender.Value;
		}

		private void floatTrackbarControlContrast_SliderDragStop( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fStartValue )
		{
			viewportPanelResult.Contrast = _Sender.Value;
		}

		private void floatTrackbarControlGamma_SliderDragStop( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fStartValue )
		{
			viewportPanelResult.Gamma = _Sender.Value;
		}

		private unsafe void viewportPanelResult_Click( object sender, EventArgs e ) {
			if ( m_imageResult == null ) {
				MessageBox( "There is no result image to save!" );
				return;
			}

			string	SourceFileName = m_sourceFileName.FullName;
			string	TargetFileName = System.IO.Path.Combine( System.IO.Path.GetDirectoryName( SourceFileName ), System.IO.Path.GetFileNameWithoutExtension( SourceFileName ) + "_ao.png" );

			saveFileDialogImage.InitialDirectory = System.IO.Path.GetDirectoryName( TargetFileName );
			saveFileDialogImage.FileName = System.IO.Path.GetFileName( TargetFileName );
			if ( saveFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			try {
				m_imageResult.Save( new System.IO.FileInfo( saveFileDialogImage.FileName ), ImageUtility.ImageFile.FILE_FORMAT.PNG );

				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while saving the image:\n\n", _e );
			}
		}

		#region Height Map

		private void outputPanelInputHeightMap_Click( object sender, EventArgs e ) {
			string	OldFileName = GetRegKey( "HeightMapFileName", System.IO.Path.Combine( m_ApplicationPath, "Example.jpg" ) );
			openFileDialogImage.InitialDirectory = System.IO.Path.GetDirectoryName( OldFileName );
			openFileDialogImage.FileName = System.IO.Path.GetFileName( OldFileName );
			if ( openFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "HeightMapFileName", openFileDialogImage.FileName );

			LoadHeightMap( new System.IO.FileInfo( openFileDialogImage.FileName ) );
		}

		private string	m_DraggedFileName = null;
		private void outputPanelInputHeightMap_DragEnter( object sender, DragEventArgs e ) {
			m_DraggedFileName = null;
			if ( (e.AllowedEffect & DragDropEffects.Copy) != DragDropEffects.Copy )
				return;

			Array	data = ((IDataObject) e.Data).GetData( "FileNameW" ) as Array;
			if ( data == null || data.Length != 1 )
				return;
			if ( !(data.GetValue(0) is String) )
				return;

			string	DraggedFileName = (data as string[])[0];

			if ( ImageUtility.ImageFile.GetFileTypeFromFileNameOnly( new System.IO.FileInfo( DraggedFileName ) ) != ImageUtility.ImageFile.FILE_FORMAT.UNKNOWN ) {
				m_DraggedFileName = DraggedFileName;	// Supported!
				e.Effect = DragDropEffects.Copy;
			}
		}

		private void outputPanelInputHeightMap_DragDrop( object sender, DragEventArgs e ) {
			if ( m_DraggedFileName != null )
				LoadHeightMap( new System.IO.FileInfo( m_DraggedFileName ) );
		}

		#endregion

		#region Normal Map

		private void outputPanelInputNormalMap_Click( object sender, EventArgs e ) {
			string	OldFileName = GetRegKey( "NormalMapFileName", System.IO.Path.Combine( m_ApplicationPath, "Example.jpg" ) );
			openFileDialogImage.InitialDirectory = System.IO.Path.GetDirectoryName( OldFileName );
			openFileDialogImage.FileName = System.IO.Path.GetFileName( OldFileName );
			if ( openFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "NormalMapFileName", openFileDialogImage.FileName );

			LoadNormalMap( new System.IO.FileInfo( openFileDialogImage.FileName ) );
		}

		private void outputPanelInputNormalMap_DragEnter( object sender, DragEventArgs e ) {
			m_DraggedFileName = null;
			if ( (e.AllowedEffect & DragDropEffects.Copy) != DragDropEffects.Copy )
				return;

			Array	data = ((IDataObject) e.Data).GetData( "FileNameW" ) as Array;
			if ( data == null || data.Length != 1 )
				return;
			if ( !(data.GetValue(0) is String) )
				return;

			string	DraggedFileName = (data as string[])[0];

			if ( ImageUtility.ImageFile.GetFileTypeFromFileNameOnly( new System.IO.FileInfo( DraggedFileName ) ) != ImageUtility.ImageFile.FILE_FORMAT.UNKNOWN ) {
				m_DraggedFileName = DraggedFileName;	// Supported!
				e.Effect = DragDropEffects.Copy;
			}
		}

		private void outputPanelInputNormalMap_DragDrop( object sender, DragEventArgs e ) {
			if ( m_DraggedFileName != null )
				LoadNormalMap( new System.IO.FileInfo( m_DraggedFileName ) );
		}

		private void clearNormalToolStripMenuItem_Click( object sender, EventArgs e ) {
			if ( m_TextureSourceNormal != null )
				m_TextureSourceNormal.Dispose();
			m_TextureSourceNormal = null;
			imagePanelNormalMap.Bitmap = null;

			// Create the default, planar normal map
			Renderer.PixelsBuffer	SourceNormalMap = new Renderer.PixelsBuffer( 4*4 );
			using ( System.IO.BinaryWriter Wr = SourceNormalMap.OpenStreamWrite() ) {
				Wr.Write( 0.0f );
				Wr.Write( 0.0f );
				Wr.Write( 1.0f );
				Wr.Write( 1.0f );
			}

			m_TextureSourceNormal = new Renderer.Texture2D( m_device, 1, 1, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new Renderer.PixelsBuffer[] { SourceNormalMap } );
		}

		#endregion

		private void buttonReload_Click( object sender, EventArgs e ) {
			m_device.ReloadModifiedShaders();
		}

		#endregion
	}
}
