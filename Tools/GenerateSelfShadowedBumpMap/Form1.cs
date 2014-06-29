//////////////////////////////////////////////////////////////////////////
// Implements the Valve technique for self-shadowed bump maps
// Source: http://www.valvesoftware.com/publications/2007/SIGGRAPH2007_EfficientSelfShadowedRadiosityNormalMapping.pdf
// More: http://n00body.squarespace.com/journal/2010/2/7/self-shadowed-bump-maps.html
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

namespace GenerateSelfShadowedBumpMap
{
	public partial class Form1 : Form
	{
		#region CONSTANTS

		private const int		MAX_THREADS = 1024;	// Maximum threads run by the compute shader

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBInput
		{
			public UInt32	y;					// Index of the texture line we're processing
			public UInt32	RaysCount;			// Amount of rays in the structured buffer
			public UInt32	MaxStepsCount;		// Maximum amount of steps to take before stopping
			public float	TexelSize_mm;		// Size of a texel (in millimeters)
			public float	Displacement_mm;	// Max displacement value encoded by the height map (in millimeters)
			public float	AOFactor;			// Darkening factor for AO when ray goes below height map
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBFilter
		{
			public float	Radius;				// Radius of the bilateral filter
			public float	Tolerance;			// Range tolerance of the bilateral filter
		}

		#endregion

		#region FIELDS

		private int								W, H;
		private ImageUtility.Bitmap				m_BitmapSource = null;

		private RendererManaged.Device			m_Device = new RendererManaged.Device();
		private RendererManaged.Texture2D		m_TextureSource = null;
		private RendererManaged.Texture2D		m_TextureTarget0 = null;
		private RendererManaged.Texture2D		m_TextureTarget1 = null;
		private RendererManaged.Texture2D		m_TextureTarget_CPU = null;

		// SSBump Generation
		private RendererManaged.ConstantBuffer<CBInput>	m_CB_Input;
		private RendererManaged.StructuredBuffer<RendererManaged.float3>	m_SB_Rays = null;
		private RendererManaged.ComputeShader	m_CS_GenerateSSBumpMap = null;

		// Bilateral filtering pre-processing
		private RendererManaged.ConstantBuffer<CBFilter>	m_CB_Filter;
		private RendererManaged.ComputeShader	m_CS_BilateralFilter = null;

		private ImageUtility.ColorProfile		m_LinearProfile = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.Chromaticities.sRGB, ImageUtility.ColorProfile.GAMMA_CURVE.STANDARD, 1.0f );
		private ImageUtility.Bitmap				m_BitmapResult = null;

		#endregion

		#region METHODS

		public unsafe Form1()
		{
			InitializeComponent();
		}

		protected override void  OnLoad(EventArgs e)
		{
 			base.OnLoad(e);

			m_Device.Init( viewportPanelResult.Handle, viewportPanelResult.Width, viewportPanelResult.Height, false, true );

			// Create our compute shaders
			m_CS_GenerateSSBumpMap = new RendererManaged.ComputeShader( m_Device, new RendererManaged.ShaderFile( new System.IO.FileInfo( "./Shaders/GenerateSSBumpMap.hlsl" ) ), "CS", null );
//			m_CS_GenerateSSBumpMap = RendererManaged.ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Binary/GenerateSSBumpMap.fxbin" ), "CS" );

			m_CS_BilateralFilter = new RendererManaged.ComputeShader( m_Device, new RendererManaged.ShaderFile( new System.IO.FileInfo( "./Shaders/BilateralFiltering.hlsl" ) ), "CS", null );
//			m_CS_BilateralFilter = RendererManaged.ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Binary/BilateralFiltering.fxbin" ), "CS" );

			// Create our constant buffers
			m_CB_Input = new RendererManaged.ConstantBuffer<CBInput>( m_Device, 0 );
			m_CB_Filter = new RendererManaged.ConstantBuffer<CBFilter>( m_Device, 0 );

			// Create our structured buffer containing the rays
			m_SB_Rays = new RendererManaged.StructuredBuffer<RendererManaged.float3>( m_Device, 3 * MAX_THREADS, true );
			integerTrackbarControlRaysCount_SliderDragStop( integerTrackbarControlRaysCount, 0 );

//			LoadHeightMap( new System.IO.FileInfo( "eye_generic_01_disp.png" ) );
			LoadHeightMap( new System.IO.FileInfo( "10 - Smooth.jpg" ) );
		}

		protected override void OnClosing( CancelEventArgs e )
		{
			m_CS_BilateralFilter.Dispose();
			m_CB_Filter.Dispose();

			m_CS_GenerateSSBumpMap.Dispose();
			m_CB_Input.Dispose();
			m_SB_Rays.Dispose();

			m_TextureTarget_CPU.Dispose();
			m_TextureTarget1.Dispose();
			m_TextureTarget0.Dispose();
			m_TextureSource.Dispose();
			m_Device.Dispose();

			base.OnClosing( e );
		}

		private void	LoadHeightMap( System.IO.FileInfo _FileName )
		{
			// Dispose of existing resources
			if ( m_BitmapSource != null )
				m_BitmapSource.Dispose();
			m_BitmapSource = null;
			if ( m_BitmapResult != null )
				m_BitmapResult.Dispose();
			m_BitmapResult = null;

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

			// Load the source image
			m_BitmapSource = new ImageUtility.Bitmap( _FileName );
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
			m_TextureTarget1 = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.RGBA32_FLOAT, false, true, null );
			m_TextureTarget_CPU = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.RGBA32_FLOAT, true, false, null );

			// Allocate target bitmap
			m_BitmapResult = new ImageUtility.Bitmap( W, H, m_LinearProfile );
		}

		private unsafe void	Generate()
		{
			groupBox1.Enabled = false;

			//////////////////////////////////////////////////////////////////////////
			// 1] Apply bilateral filtering to the input texture as a pre-process
			m_TextureSource.SetCS( 0 );
			m_TextureTarget0.SetCSUAV( 0 );

			m_CB_Filter.m.Radius = floatTrackbarControlBilateralRadius.Value;
			m_CB_Filter.m.Tolerance = floatTrackbarControlBilateralTolerance.Value;
			m_CB_Filter.UpdateData();

			m_CS_BilateralFilter.Use();
			m_CS_BilateralFilter.Dispatch( W, H, 1 );

			m_TextureTarget0.RemoveFromLastAssignedSlotUAV();	// So we can use it as input for next stage

			//////////////////////////////////////////////////////////////////////////
			// 2] Compute directional occlusion

			// Prepare computation parameters
			m_TextureTarget0.SetCS( 0 );
			m_TextureTarget1.SetCSUAV( 0 );
			m_SB_Rays.SetInput( 1 );

			m_CB_Input.m.RaysCount = (UInt32) Math.Min( MAX_THREADS, integerTrackbarControlRaysCount.Value );
			m_CB_Input.m.MaxStepsCount = (UInt32) integerTrackbarControlMaxStepsCount.Value;
			m_CB_Input.m.TexelSize_mm = 1000.0f / floatTrackbarControlPixelDensity.Value;
			m_CB_Input.m.Displacement_mm = 10.0f * floatTrackbarControlHeight.Value;
			m_CB_Input.m.AOFactor = floatTrackbarControlAOFactor.Value;

			// Start
			m_CS_GenerateSSBumpMap.Use();

			int	h = Math.Max( 1, H / 100 );
			int	CallsCount = (int) Math.Ceiling( (float) H / h );
			for ( int i=0; i < CallsCount; i++ )
			{
				m_CB_Input.m.y = (UInt32) (i * h);
				m_CB_Input.UpdateData();
				m_CS_GenerateSSBumpMap.Dispatch( W, h, 1 );

				progressBar.Value = (i+1) * progressBar.Maximum / CallsCount;
				progressBar.Refresh();
				Application.DoEvents();
			}

// 			m_CB_Input.m.y = 0;
// 			m_CB_Input.UpdateData();
// 			m_CS_GenerateSSBumpMap.Dispatch( W, H, 1 );

			//////////////////////////////////////////////////////////////////////////
			// 3] Copy target to staging for CPU readback and update the resulting bitmap
			m_TextureTarget_CPU.CopyFrom( m_TextureTarget1 );

			RendererManaged.PixelsBuffer	Pixels = m_TextureTarget_CPU.Map( 0, 0 );
			using ( System.IO.BinaryReader R = Pixels.OpenStreamRead() )
				for ( int Y=0; Y < H; Y++ )
				{
					R.BaseStream.Position = Y * Pixels.RowPitch;
					for ( int X=0; X < W; X++ )
					{
						ImageUtility.float4	Color = new ImageUtility.float4( R.ReadSingle(), R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
						Color = m_LinearProfile.RGB2XYZ( Color );
						m_BitmapResult.ContentXYZ[X,Y] = Color;
					}
				}

			Pixels.Dispose();
			m_TextureTarget_CPU.UnMap( 0, 0 );

			// Assign result
			viewportPanelResult.Image = m_BitmapResult;

			groupBox1.Enabled = true;
		}

		private void	GenerateRays( int _RaysCount )
		{
			_RaysCount = Math.Min( MAX_THREADS, _RaysCount );

			// Half-Life 2 basis
			RendererManaged.float3[]	HL2Basis = new RendererManaged.float3[] {
				new RendererManaged.float3( (float) Math.Sqrt( 2.0 / 3.0 ), 0.0f, (float) Math.Sqrt( 1.0 / 3.0 ) ),
				new RendererManaged.float3( -(float) Math.Sqrt( 1.0 / 6.0 ), (float) Math.Sqrt( 1.0 / 2.0 ), (float) Math.Sqrt( 1.0 / 3.0 ) ),
				new RendererManaged.float3( -(float) Math.Sqrt( 1.0 / 6.0 ), -(float) Math.Sqrt( 1.0 / 2.0 ), (float) Math.Sqrt( 1.0 / 3.0 ) )
			};

			float	CenterTheta = (float) Math.Acos( HL2Basis[0].z );
			float[]	CenterPhi = new float[] {
				(float) Math.Atan2( HL2Basis[0].y, HL2Basis[0].x ),
				(float) Math.Atan2( HL2Basis[1].y, HL2Basis[1].x ),
				(float) Math.Atan2( HL2Basis[2].y, HL2Basis[2].x ),
			};

			Random	R = new Random( 1 );
			for ( int RayIndex=0; RayIndex < _RaysCount; RayIndex++ )
			{
				// Stratified version
// 				double	Phi = ((Math.PI / 3.0) * (2.0 * (RayIndex + R.NextDouble()) / _RaysCount - 1.0));
// 				double	Theta = (Math.Acos( Math.Sqrt( (RayIndex + R.NextDouble()) / _RaysCount ) ));

				// Don't give a shit version
				double	Phi = (Math.PI / 3.0) * (2.0 * R.NextDouble() - 1.0);
//				double	Theta = Math.Acos( Math.Sqrt( R.NextDouble() ) );
				double	Theta = 0.5 * Math.PI * R.NextDouble();

				Theta = Math.Min( 0.499f * Math.PI, Theta );


				double	CosTheta = Math.Cos( Theta );
				double	SinTheta = Math.Sin( Theta );

// 				double	LengthFactor = 1.0 / SinTheta;	// The ray is scaled so we ensure we always walk at least a texel in the texture
// 				CosTheta *= LengthFactor;
// 				SinTheta *= LengthFactor;	// Yeah, yields 1... :)

				m_SB_Rays.m[0*MAX_THREADS+RayIndex].Set(	(float) (Math.Cos( CenterPhi[0] + Phi ) * SinTheta),
															(float) (Math.Sin( CenterPhi[0] + Phi ) * SinTheta),
															(float) CosTheta );
				m_SB_Rays.m[1*MAX_THREADS+RayIndex].Set(	(float) (Math.Cos( CenterPhi[1] + Phi ) * SinTheta),
															(float) (Math.Sin( CenterPhi[1] + Phi ) * SinTheta),
															(float) CosTheta );
				m_SB_Rays.m[2*MAX_THREADS+RayIndex].Set(	(float) (Math.Cos( CenterPhi[2] + Phi ) * SinTheta),
															(float) (Math.Sin( CenterPhi[2] + Phi ) * SinTheta),
															(float) CosTheta );
			}

			m_SB_Rays.Write();
		}

		private void MessageBox( string _Message )
		{
			MessageBox( _Message, MessageBoxButtons.OK, MessageBoxIcon.Information );
		}
		private void MessageBox( string _Message, MessageBoxButtons _Buttons, MessageBoxIcon _Icon )
		{
			System.Windows.Forms.MessageBox.Show( this, _Message, "Generator", _Buttons, _Icon );
		}

		#region Super Slow CPU Version

		private void	Generate_CPU( int _RaysCount )
		{
			try
			{
				groupBox1.Enabled = false;

				// Half-life basis (Z points outside of the surface, as in normal maps)
				WMath.Vector[]	Basis = new WMath.Vector[] {
					new WMath.Vector( (float) Math.Sqrt( 2.0 / 3.0 ), 0.0f, (float) Math.Sqrt( 1.0 / 3.0 ) ),
					new WMath.Vector( (float) -Math.Sqrt( 1.0 / 6.0 ), (float) Math.Sqrt( 1.0 / 2.0 ), (float) Math.Sqrt( 1.0 / 3.0 ) ),
					new WMath.Vector( (float) -Math.Sqrt( 1.0 / 6.0 ), (float) -Math.Sqrt( 1.0 / 2.0 ), (float) Math.Sqrt( 1.0 / 3.0 ) ),
				};

// 				// 1] Compute normal map
// 				WMath.Vector	dX = new WMath.Vector();
// 				WMath.Vector	dY = new WMath.Vector();
// 				WMath.Vector	N;
// 				float			ddX = floatTrackbarControlPixelSize.Value;
// 				float			ddH = floatTrackbarControlHeight.Value;
// 				for ( int Y=0; Y < H; Y++ )
// 				{
// 					int	Y0 = Math.Max( 0, Y-1 );
// 					int	Y1 = Math.Min( H-1, Y+1 );
// 					for ( int X=0; X < W; X++ )
// 					{
// 						int	X0 = Math.Max( 0, X-1 );
// 						int	X1 = Math.Min( W-1, X+1 );
// 
// 						float	Hx0 = m_BitmapSource.ContentXYZ[X0,Y].y;
// 						float	Hx1 = m_BitmapSource.ContentXYZ[X1,Y].y;
// 						float	Hy0 = m_BitmapSource.ContentXYZ[X,Y0].y;
// 						float	Hy1 = m_BitmapSource.ContentXYZ[X,Y1].y;
// 
// 						dX.Set( 2.0f * ddX, 0.0f, ddH * (Hx1 - Hx0) );
// 						dY.Set( 0.0f, 2.0f * ddX, ddH * (Hy1 - Hy0) );
// 
// 						N = dX.Cross( dY ).Normalized;
// 
// 						m_Normal[X,Y] = new WMath.Vector(
// 							N.Dot( Basis[0] ),
// 							N.Dot( Basis[1] ),
// 							N.Dot( Basis[2] ) );
// 					}
// 
// 					// Update and show progress
// 					UpdateProgress( m_Normal, Y, true );
// 				}
// 				UpdateProgress( m_Normal, H, true );

				float	LobeExponent = 4.0f;//floatTrackbarControlLobeExponent.Value;

				float	PixelSize_mm = 1000.0f / floatTrackbarControlPixelDensity.Value;

				float	Scale = 0.1f * PixelSize_mm / floatTrackbarControlHeight.Value;	// Scale factor to apply to pixel distances so they're renormalized in [0,1], our "heights space"...
//						Scale *= floatTrackbarControlZFactor.Value;	// Cheat Z velocity so AO is amplified!

				// 2] Build local rays only once
				int				RaysCount = integerTrackbarControlRaysCount.Value;
				WMath.Vector[,]	Rays = new WMath.Vector[3,RaysCount];

				// Create orthonormal bases to orient the lobe
				WMath.Vector	Xr = Basis[0].Cross( WMath.Vector.UnitZ ).Normalized;	// We can safely use (0,0,1) as the "up" direction since the HL2 basis doesn't have any vertical direction
				WMath.Vector	Yr = Xr.Cross( Basis[0] );
				WMath.Vector	Xg = Basis[1].Cross( WMath.Vector.UnitZ ).Normalized;	// We can safely use (0,0,1) as the "up" direction since the HL2 basis doesn't have any vertical direction
				WMath.Vector	Yg = Xg.Cross( Basis[1] );
				WMath.Vector	Xb = Basis[2].Cross( WMath.Vector.UnitZ ).Normalized;	// We can safely use (0,0,1) as the "up" direction since the HL2 basis doesn't have any vertical direction
				WMath.Vector	Yb = Xb.Cross( Basis[2] );

				double	Exponent = 1.0 / (1.0 + LobeExponent);
				for ( int RayIndex=0; RayIndex < RaysCount; RayIndex++ )
				{
					if ( false )
					{
						double	Phi = 2.0 * Math.PI * WMath.SimpleRNG.GetUniform();
//						double	Theta = Math.Acos( Math.Pow( WMath.SimpleRNG.GetUniform(), Exponent ) );
						double	Theta = Math.PI / 3.0 * WMath.SimpleRNG.GetUniform();

						WMath.Vector	RayLocal = new WMath.Vector(
							(float) (Math.Cos( Phi ) * Math.Sin( Theta )),
							(float) (Math.Sin( Phi ) * Math.Sin( Theta )),
							(float) Math.Cos( Theta ) );

						Rays[0,RayIndex] = RayLocal.x * Xr + RayLocal.y * Yr + RayLocal.z * Basis[0];
						Rays[1,RayIndex] = RayLocal.x * Xg + RayLocal.y * Yg + RayLocal.z * Basis[1];
						Rays[2,RayIndex] = RayLocal.x * Xb + RayLocal.y * Yb + RayLocal.z * Basis[2];
					}
					else
					{
						double	Phi = Math.PI / 3.0 * (2.0 * WMath.SimpleRNG.GetUniform() - 1.0);
						double	Theta = 0.49 * Math.PI * WMath.SimpleRNG.GetUniform();
						Rays[0,RayIndex] = new WMath.Vector(
							(float) (Math.Cos( Phi ) * Math.Sin( Theta )),
							(float) (Math.Sin( Phi ) * Math.Sin( Theta )),
							(float) Math.Cos( Theta ) );

						Phi = Math.PI / 3.0 * (2.0 * WMath.SimpleRNG.GetUniform() - 1.0 + 2.0);
						Theta = 0.49 * Math.PI * WMath.SimpleRNG.GetUniform();
						Rays[1,RayIndex] = new WMath.Vector(
							(float) (Math.Cos( Phi ) * Math.Sin( Theta )),
							(float) (Math.Sin( Phi ) * Math.Sin( Theta )),
							(float) Math.Cos( Theta ) );

						Phi = Math.PI / 3.0 * (2.0 * WMath.SimpleRNG.GetUniform() - 1.0 + 4.0);
						Theta = 0.49 * Math.PI * WMath.SimpleRNG.GetUniform();
						Rays[2,RayIndex] = new WMath.Vector(
							(float) (Math.Cos( Phi ) * Math.Sin( Theta )),
							(float) (Math.Sin( Phi ) * Math.Sin( Theta )),
							(float) Math.Cos( Theta ) );
					}

					Rays[0,RayIndex].z *= Scale;
					Rays[1,RayIndex].z *= Scale;
					Rays[2,RayIndex].z *= Scale;
				}

				// 3] Compute directional occlusion
				for ( int Y=0; Y < H; Y++ )
				{
					for ( int X=0; X < W; X++ )
					{
						float	R = ComputeAO( 0, X, Y, Scale, Rays );
						float	G = ComputeAO( 1, X, Y, Scale, Rays );
						float	B = ComputeAO( 2, X, Y, Scale, Rays );
//						N = m_Normal[X,Y];
// 
 						m_BitmapResult.ContentXYZ[X,Y] = m_LinearProfile.RGB2XYZ( new ImageUtility.float4( R, G, B, (R+G+B)/3.0f ) );
					}

					// Update and show progress
					UpdateProgress( m_BitmapResult, Y, true );
				}
				UpdateProgress( m_BitmapResult, H, true );

//				m_BitmapResult.Save( "eye_generic_01_disp_hl2.png", ImageFormat.Png );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred during generation:\r\n" + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
			finally
			{
				groupBox1.Enabled = true;
			}
		}

		/// <summary>
		/// Computes the ambient occlusion of the specified coordinate by shooting N rays in a lobe oriented in the specified light direction
		/// </summary>
		/// <param name="_Light"></param>
		/// <param name="_X"></param>
		/// <param name="_Y"></param>
		/// <param name="_Z2HeightScale">Scale factor to apply to world Z coordinate to be remapped into the heights' [0,1] range</param>
		/// <param name="_LobeExponent">1 is a simple cosine lobe</param>
		/// <returns></returns>
		/// 
		private float	ComputeAO( int _LightIndex, int _X, int _Y, float _Z2HeightScale, WMath.Vector[,] _Rays )
		{
			int		RaysCount = _Rays.GetLength( 1 );
			int		MaxStepsCount = integerTrackbarControlMaxStepsCount.Value;

			float	Z0 = m_BitmapSource.ContentXYZ[_X,_Y].y;
			double	AO = 0.0f;
			int		SamplesCount = 0;
			for ( int RayIndex=0; RayIndex < RaysCount; RayIndex++ )
			{
				WMath.Vector	RayWorld = _Rays[_LightIndex,RayIndex];
 				if ( RayWorld.z < 0.0f )
				{
// AO += 1.0;
// SamplesCount++;
 					continue;	// Pointing to the ground so don't account for it...
				}

				// Make sure the ray has a unit step so we always travel at least one pixel
//				m_RayWorld.z *= _Z2HeightScale;
// 				float	Normalizer = 1.0f / Math.Max( Math.Abs( m_RayWorld.x ), Math.Abs( m_RayWorld.y ) );
// 				float	Normalizer = 1.0f;
// 				Normalizer = Math.Max( Normalizer, (1.0f - Z) / (128.0f * m_RayWorld.z) );	// This makes sure we can't use more than 128 steps to escape the heightfield
// 
// 				float	Normalizer = (1.0f - Z) / (128.0f * m_RayWorld.z);
// 
// 				m_RayWorld.x *= Normalizer;
// 				m_RayWorld.y *= Normalizer;
// 				m_RayWorld.z *= Normalizer;

				// Start from the provided coordinates
				float	X = _X;
				float	Y = _Y;
				float	Z = Z0;

				// Compute intersection with the height field
				int	StepIndex = 0;
				while ( StepIndex < MaxStepsCount && Z < 1.0f && X > 0.0f && Y > 0.0f && X < W && Y < H )
				{
					X += RayWorld.x;
					Y += RayWorld.y;
					Z += RayWorld.z;

					float	Height = SampleHeightField( X, Y );
					if ( Height > Z )
					{	// Hit!
						AO += 1.0;
						break;
					}

					StepIndex++;
				}

				SamplesCount++;
			}
			AO /= SamplesCount;

			return (float) (1.0 - AO);
		}

		private float	SampleHeightField( float _X, float _Y )
		{
// 			_X *= W;
// 			_Y *= H;
			int		X0 = (int) Math.Floor( _X );
			int		Y0 = (int) Math.Floor( _Y );
			float	x = _X - X0;
			float	y = _Y - Y0;
			X0 = Math.Max( 0, Math.Min( W-1, X0 ) );
			Y0 = Math.Max( 0, Math.Min( H-1, Y0 ) );
			int		X1 = Math.Min( W-1, X0+1 );
			int		Y1 = Math.Min( H-1, Y0+1 );

			float	V00 = m_BitmapSource.ContentXYZ[X0,Y0].y;
			float	V01 = m_BitmapSource.ContentXYZ[X1,Y0].y;
			float	V10 = m_BitmapSource.ContentXYZ[X0,Y1].y;
			float	V11 = m_BitmapSource.ContentXYZ[X1,Y1].y;

			float	V0 = V00 + (V01-V00) * x;
			float	V1 = V10 + (V11-V10) * x;

			float	V = V0 + (V1-V0) * y;
			return V;
		}

		private unsafe void	UpdateProgress( ImageUtility.Bitmap _Image, int Y, bool _Bias )
		{
			const int	REFRESH_EVERY_N_SCANLINES = 4;

			if ( Y == 0 || (Y & (REFRESH_EVERY_N_SCANLINES-1)) != 0 )
				return;

			viewportPanelResult.Image = _Image;
			Application.DoEvents();
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
			GenerateRays( _Sender.Value );
		}

		private void checkBoxShowAO_CheckedChanged( object sender, EventArgs e )
		{
			viewportPanelResult.ShowAO = checkBoxShowAO.Checked;
		}
 
		#endregion
	}
}
