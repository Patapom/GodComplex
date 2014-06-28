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

		#endregion

		#region FIELDS

		private int								W, H;
		private ImageUtility.Bitmap				m_BitmapSource = null;

		private RendererManaged.Device			m_Device = new RendererManaged.Device();
		private RendererManaged.Texture2D		m_TextureSource = null;
		private RendererManaged.Texture2D		m_TextureTarget = null;
		private RendererManaged.Texture2D		m_TextureTarget_CPU = null;

		private RendererManaged.ConstantBuffer<CBInput>	m_CB_Input;
		private RendererManaged.StructuredBuffer<RendererManaged.float3>	m_SB_Rays = null;
		private RendererManaged.ComputeShader	m_CS_GenerateSSBumpMap = null;

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

			// Create our compute shader
			m_CS_GenerateSSBumpMap = new RendererManaged.ComputeShader( m_Device, new RendererManaged.ShaderFile( new System.IO.FileInfo( "./Shaders/GenerateSSBumpMap.hlsl" ) ), "CS", null );
//			m_CS_GenerateSSBumpMap = RendererManaged.ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Binary/GenerateSSBumpMap.fxbin" ), "CS" );

			// Create our constant buffer
			m_CB_Input = new RendererManaged.ConstantBuffer<CBInput>( m_Device, 0 );

			// Create our structured buffer containing the rays
			m_SB_Rays = new RendererManaged.StructuredBuffer<RendererManaged.float3>( m_Device, 3 * MAX_THREADS, true );
			integerTrackbarControlRaysCount_SliderDragStop( integerTrackbarControlRaysCount, 0 );

//			LoadHeightMap( new System.IO.FileInfo( "eye_generic_01_disp.png" ) );
			LoadHeightMap( new System.IO.FileInfo( "10 - Smooth.jpg" ) );
		}

		protected override void OnClosing( CancelEventArgs e )
		{
			m_CS_GenerateSSBumpMap.Dispose();
			m_CB_Input.Dispose();
			m_SB_Rays.Dispose();

			m_TextureTarget_CPU.Dispose();
			m_TextureTarget.Dispose();
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
			if ( m_TextureTarget != null )
				m_TextureTarget.Dispose();
			m_TextureTarget = null;
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
			m_TextureTarget = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.RGBA32_FLOAT, false, true, null );
			m_TextureTarget_CPU = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.RGBA32_FLOAT, true, false, null );

			// Allocate target bitmap
			m_BitmapResult = new ImageUtility.Bitmap( W, H, m_LinearProfile );
		}

		private unsafe void	Generate()
		{
			// Prepare computation parameters
			m_TextureSource.SetCS( 0 );
			m_TextureTarget.SetCSUAV( 0 );
			m_SB_Rays.SetInput( 1 );

			m_CB_Input.m.RaysCount = (UInt32) Math.Min( MAX_THREADS, integerTrackbarControlRaysCount.Value );
			m_CB_Input.m.MaxStepsCount = (UInt32) integerTrackbarControlMaxStepsCount.Value;
			m_CB_Input.m.TexelSize_mm = 1000.0f / floatTrackbarControlPixelDensity.Value;
			m_CB_Input.m.Displacement_mm = 10.0f * floatTrackbarControlHeight.Value;
			m_CB_Input.m.AOFactor = floatTrackbarControlAOFactor.Value;

			//////////////////////////////////////////////////////////////////////////
			// Compute!
			m_CS_GenerateSSBumpMap.Use();

			int	h = Math.Max( 1, H / 10 );
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

			//////////////////////////////////////////////////////////////////////////
			// Copy target to staging for CPU readback and update the resulting bitmap
			m_TextureTarget_CPU.CopyFrom( m_TextureTarget );

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
				double	Phi = ((Math.PI / 3.0) * (2.0 * (RayIndex + R.NextDouble()) / _RaysCount - 1.0));
				double	Theta = (Math.Acos( Math.Sqrt( (RayIndex + R.NextDouble()) / _RaysCount ) ));

// 				// Don't give a shit version
// 				double	Phi = (Math.PI / 3.0) * (2.0 * R.NextDouble() - 1.0);
// //				double	Theta = Math.Acos( Math.Sqrt( R.NextDouble() ) );
// 				double	Theta = 0.5 * Math.PI * R.NextDouble();

//				Theta = Math.Min( 0.499f * Math.PI, Theta );


				double	CosTheta = Math.Cos( Theta );
				double	SinTheta = Math.Sin( Theta );

				double	LengthFactor = 1.0 / SinTheta;	// The ray is scaled so we ensure we always walk at least a texel in the texture
				CosTheta *= LengthFactor;
				SinTheta *= LengthFactor;	// Yeah, yields 1... :)

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

// 			try
// 			{
// 				groupBox1.Enabled = false;
// 
// 				// Half-life basis (Z points outside of the surface, as in normal maps)
// 				WMath.Vector[]	Basis = new WMath.Vector[] {
// 					new WMath.Vector( (float) (-1.0 / Math.Sqrt( 6.0 )), (float) (1.0 / Math.Sqrt( 2.0 )), (float) (1.0 / Math.Sqrt( 3.0 )) ),
// 					new WMath.Vector( (float) (-1.0 / Math.Sqrt( 6.0 )), (float) (-1.0 / Math.Sqrt( 2.0 )), (float) (1.0 / Math.Sqrt( 3.0 )) ),
// 					new WMath.Vector( (float) (Math.Sqrt( 2.0 ) / Math.Sqrt( 6.0 )), (float) 0.0, (float) (1.0 / Math.Sqrt( 3.0 )) ),
// 				};
// 
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
// 						float	Hx0 = m_HeightMap[X0,Y];
// 						float	Hx1 = m_HeightMap[X1,Y];
// 						float	Hy0 = m_HeightMap[X,Y0];
// 						float	Hy1 = m_HeightMap[X,Y1];
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
// 
// 				// 2] Compute directional occlusion
// 				float	Exponent = floatTrackbarControlLobeExponent.Value;
// 				float	Scale = floatTrackbarControlPixelSize.Value / floatTrackbarControlHeight.Value;	// Scale factor to apply to pixel distances so they're renormalized in [0,1], our "heights space"...
// 						Scale *= floatTrackbarControlZFactor.Value;	// Cheat Z velocity so AO is amplified!
// 
// 				for ( int Y=0; Y < H; Y++ )
// 				{
// 					for ( int X=0; X < W; X++ )
// 					{
// 						float	R = ComputeAO( Basis[0], X, Y, Scale, Exponent );
// 						float	G = ComputeAO( Basis[1], X, Y, Scale, Exponent );
// 						float	B = ComputeAO( Basis[2], X, Y, Scale, Exponent );
// 						N = m_Normal[X,Y];
// 
// 						m_Result[X,Y] = new WMath.Vector( R * N.x, G * N.y, B * N.z );
// 					}
// 
// 					// Update and show progress
// 					UpdateProgress( m_Result, Y, true );
// 				}
// 				UpdateProgress( m_Result, H, true );
// 
// 				m_BitmapResult.Save( "eye_generic_01_disp_hl2.png", ImageFormat.Png );
// 			}
// 			catch ( Exception _e )
// 			{
// 				MessageBox( "An error occurred during generation:\r\n" + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
// 			}
// 			finally
// 			{
// 				groupBox1.Enabled = true;
// 			}
// 		}
// 
// 		/// <summary>
// 		/// Computes the ambient occlusion of the specified coordinate by shooting N rays in a lobe oriented in the specified light direction
// 		/// </summary>
// 		/// <param name="_Light"></param>
// 		/// <param name="_X"></param>
// 		/// <param name="_Y"></param>
// 		/// <param name="_Z2HeightScale">Scale factor to apply to world Z coordinate to be remapped into the heights' [0,1] range</param>
// 		/// <param name="_LobeExponent">1 is a simple cosine lobe</param>
// 		/// <returns></returns>
// 		/// 
// 		WMath.Vector	m_X;
// 		WMath.Vector	m_Y;
// 		WMath.Vector	m_RayLocal = new WMath.Vector();
// 		WMath.Vector	m_RayWorld = new WMath.Vector();
// 		private float	ComputeAO( WMath.Vector _Light, int _X, int _Y, float _Z2HeightScale, float _LobeExponent )
// 		{
// 			double	Exponent = 1.0 / (1.0 + _LobeExponent);
// 
// 			WMath.Vector	ScaledLight = new WMath.Vector( _Light.x, _Light.y, _Z2HeightScale *_Light.y ).Normalized;
// 
// 			// Create orthonormal basis to orient the lobe
// 			m_X = ScaledLight.Cross( WMath.Vector.UnitZ ).Normalized;	// We can safely use (0,0,1) as the "up" direction since the HL2 basis doesn't have any vertical direction
// 			m_Y = m_X.Cross( ScaledLight );
// 
// 			// Start from the provided coordinates
// 			float	X = _X;
// 			float	Y = _Y;
// 			float	Z = m_HeightMap[_X,_Y];
// 
// 			double	AO = 0.0f;
// 			int		SamplesCount = 0;
// 			for ( int RayIndex=0; RayIndex < integerTrackbarControlRaysCount.Value; RayIndex++ )
// 			{
// 				double	Phi = 2.0 * Math.PI * WMath.SimpleRNG.GetUniform();
// 				double	Theta = Math.Acos( Math.Pow( WMath.SimpleRNG.GetUniform(), Exponent ) );
// 				m_RayLocal.x = (float) (Math.Cos( Phi ) * Math.Sin( Theta ));
// 				m_RayLocal.y = (float) (Math.Sin( Phi ) * Math.Sin( Theta ));
// 				m_RayLocal.z = (float) Math.Cos( Theta );
// 				m_RayWorld = m_RayLocal.x * m_X + m_RayLocal.y * m_Y + m_RayLocal.z * ScaledLight;
//  				if ( m_RayWorld.z < 0.0f )
// 				{
// // AO += 1.0;
// // SamplesCount++;
//  					continue;	// Pointing to the ground so don't account for it...
// 				}
// 
// 				// Make sure the ray has a unit step so we always travel at least one pixel
// //				m_RayWorld.z *= _Z2HeightScale;
// // 				float	Normalizer = 1.0f / Math.Max( Math.Abs( m_RayWorld.x ), Math.Abs( m_RayWorld.y ) );
// // 				float	Normalizer = 1.0f;
// // 				Normalizer = Math.Max( Normalizer, (1.0f - Z) / (128.0f * m_RayWorld.z) );	// This makes sure we can't use more than 128 steps to escape the heightfield
// 
// 				float	Normalizer = (1.0f - Z) / (128.0f * m_RayWorld.z);
// 
// 				m_RayWorld.x *= Normalizer;
// 				m_RayWorld.y *= Normalizer;
// 				m_RayWorld.z *= Normalizer;
// 
// 				// Compute intersection with the height field
// 				while ( Z < 1.0f )
// 				{
// 					X += m_RayWorld.x;
// 					Y += m_RayWorld.y;
// 					Z += m_RayWorld.z;
// 
// 					float	Height = SampleHeightField( X, Y );
// 					if ( Height >= Z )
// 					{	// Hit!
// 						AO += 1.0;
// 						break;
// 					}
// 				}
// 
// 				SamplesCount++;
// 			}
// 			AO /= SamplesCount;
// 
// 			return (float) (1.0 - AO);
// 		}
// 
// 		private float	SampleHeightField( float _X, float _Y )
// 		{
// 			_X *= W;
// 			_Y *= H;
// 			int		X0 = (int) Math.Floor( _X );
// 			int		Y0 = (int) Math.Floor( _Y );
// 			float	x = _X - X0;
// 			float	y = _Y - Y0;
// 			X0 = Math.Max( 0, Math.Min( W-1, X0 ) );
// 			Y0 = Math.Max( 0, Math.Min( H-1, Y0 ) );
// 			int		X1 = Math.Min( W-1, X0+1 );
// 			int		Y1 = Math.Min( H-1, Y0+1 );
// 
// 			float	V00 = m_HeightMap[X0,Y0];
// 			float	V01 = m_HeightMap[X1,Y0];
// 			float	V10 = m_HeightMap[X0,Y1];
// 			float	V11 = m_HeightMap[X1,Y1];
// 
// 			float	V0 = V00 + (V01-V00) * x;
// 			float	V1 = V10 + (V11-V10) * x;
// 
// 			float	V = V0 + (V1-V0) * y;
// 			return V;
// 		}
// 
// 		private unsafe void	UpdateProgress( WMath.Vector[,] _Source, int Y, bool _Bias )
// 		{
// 			const int	REFRESH_EVERY_N_SCANLINES = 4;
// 
// 			if ( Y == 0 || (Y & (REFRESH_EVERY_N_SCANLINES-1)) != 0 )
// 				return;
// 
// //			BitmapData	LockedBitmap = m_BitmapResult.LockBits( new Rectangle( 0, Y-REFRESH_EVERY_N_SCANLINES, W, REFRESH_EVERY_N_SCANLINES ), ImageLockMode.WriteOnly, PixelFormat.Format64bppRgb );
// 			BitmapData	LockedBitmap = m_BitmapResult.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb );
// 			for ( int y=Y-REFRESH_EVERY_N_SCANLINES; y < Y; y++ )
// 			{
// //						ushort*	pScanline = (ushort*) ((byte*) LockedBitmap.Scan0.ToPointer() + LockedBitmap.Stride * y);
// 				byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + LockedBitmap.Stride * y;
// 				for ( int X=0; X < W; X++ )
// 				{
// 					WMath.Vector	V = _Source[X,y];
// // 							*pScanline++ = (ushort) 65535;												// A
// // 							*pScanline++ = (ushort) (65535.0 * Math.Max( 0, Math.Min( 1.0, V.x ) ));	// R
// // 							*pScanline++ = (ushort) (65535.0 * Math.Max( 0, Math.Min( 1.0, V.y ) ));	// G
// // 							*pScanline++ = (ushort) (65535.0 * Math.Max( 0, Math.Min( 1.0, V.z ) ));	// B
// 
// 					*pScanline++ = (byte) (Math.Max( 0, Math.Min( 255, 255.0 * (_Bias ? 0.5f * (1.0f + V.z) : V.z) ) ));	// B
// 					*pScanline++ = (byte) (Math.Max( 0, Math.Min( 255, 255.0 * (_Bias ? 0.5f * (1.0f + V.y) : V.y) ) ));	// G
// 					*pScanline++ = (byte) (Math.Max( 0, Math.Min( 255, 255.0 * (_Bias ? 0.5f * (1.0f + V.x) : V.x) ) ));	// R
// 					*pScanline++ = (byte) 255;												// A
// 				}
// 			}
// 			m_BitmapResult.UnlockBits( LockedBitmap );
// 			LockedBitmap = null;
// //			outputPanelResult.Image = m_BitmapResult;
// //			Application.DoEvents();
		#endregion

		#region EVENT HANDLERS

 		private unsafe void buttonGenerate_Click( object sender, EventArgs e )
 		{
			Generate();
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
