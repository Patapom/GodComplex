#define ABS_NORMAL

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

using RendererManaged;

namespace ProbeSHEncoder
{
	public partial class OutputPanel : Panel
	{
		private const double			FOV = 0.7 * Math.PI;

		public enum		VIZ_TYPE
		{
			ALBEDO,
			DISTANCE,
			NORMAL,
			STATIC_LIT,
			FACE_INDEX,
			EMISSIVE_MAT_ID,
			NEIGHBOR_PROBE_ID,
			SET_INDEX,
			SET_ALBEDO,
			SET_DISTANCE,
			SET_NORMAL,
			SET_SAMPLES,
			SH,
		}
		private VIZ_TYPE		m_Viz = VIZ_TYPE.ALBEDO;
		public VIZ_TYPE			Viz
		{
			get { return m_Viz; }
			set
			{
				if ( value == m_Viz )
					return;

				m_Viz = value;
				UpdateBitmap();
			}
		}
		private int				m_IsolatedSetIndex = 0;
		public int				IsolatedSetIndex
		{
			get { return m_IsolatedSetIndex; }
			set
			{
				if ( value == m_IsolatedSetIndex )
					return;

				m_IsolatedSetIndex = value;
				if ( m_Viz < VIZ_TYPE.SET_INDEX )
					return;
				UpdateBitmap();
				Refresh();
			}
		}
		private bool			m_IsolateSet = false;
		public bool				IsolateSet
		{
			get { return m_IsolateSet; }
			set
			{
				if ( value == m_IsolateSet )
					return;

				m_IsolateSet = value;
				if ( m_Viz < VIZ_TYPE.SET_INDEX )
					return;
				UpdateBitmap();
				Refresh();
			}
		}
		private bool			m_ShowSetAverage = false;
		public bool				ShowSetAverage
		{
			get { return m_ShowSetAverage; }
			set
			{
				if ( value == m_ShowSetAverage )
					return;

				m_ShowSetAverage = value;
				if ( m_Viz < VIZ_TYPE.SET_INDEX )
					return;
				UpdateBitmap();
				Refresh();
			}
		}

		// Static SH
		private bool			m_bShowSHStatic = false;
		public bool				ShowSHStatic
		{
			get { return m_bShowSHStatic; }
			set
			{
				if ( value == m_bShowSHStatic )
					return;

				m_bShowSHStatic = value;
				if ( m_Viz == VIZ_TYPE.SH )
					UpdateBitmap();
			}
		}
		private float3[]	m_SHStatic = new float3[9];
		public float3[]	SHStatic
		{
			get { return m_SHStatic; }
			set
			{
				if ( value == null || value == m_SHStatic )
					return;

				m_SHStatic = value;
				if ( m_Viz != VIZ_TYPE.SH )
					return;
				UpdateBitmap();
				Refresh();
			}
		}

		// Dynamic SH
		private bool			m_bShowSHDynamic = true;
		public bool				ShowSHDynamic
		{
			get { return m_bShowSHDynamic; }
			set
			{
				if ( value == m_bShowSHDynamic )
					return;

				m_bShowSHDynamic = value;
				if ( m_Viz == VIZ_TYPE.SH )
					UpdateBitmap();
			}
		}
		private float3[]	m_SHDynamic = new float3[9];
		public float3[]	SHDynamic
		{
			get { return m_SHDynamic; }
			set
			{
				if ( value == null || value == m_SHDynamic )
					return;

				m_SHDynamic = value;
				if ( m_Viz != VIZ_TYPE.SH )
					return;
				UpdateBitmap();
				Refresh();
			}
		}

		// Emissive SH
		private bool			m_bShowSHEmissive = false;
		public bool				ShowSHEmissive
		{
			get { return m_bShowSHEmissive; }
			set
			{
				if ( value == m_bShowSHEmissive )
					return;

				m_bShowSHEmissive = value;
				if ( m_Viz == VIZ_TYPE.SH )
					UpdateBitmap();
			}
		}
		private float3[]	m_SHEmissive = new float3[9];
		public float3[]	SHEmissive
		{
			get { return m_SHEmissive; }
			set
			{
				if ( value == null || value == m_SHEmissive )
					return;

				m_SHEmissive = value;
				if ( m_Viz != VIZ_TYPE.SH )
					return;
				UpdateBitmap();
				Refresh();
			}
		}

		// Occlusion SH
		private bool			m_bShowSHOcclusion = false;
		public bool				ShowSHOcclusion
		{
			get { return m_bShowSHOcclusion; }
			set
			{
				if ( value == m_bShowSHOcclusion )
					return;

				m_bShowSHOcclusion = value;
				if ( m_Viz == VIZ_TYPE.SH )
					UpdateBitmap();
			}
		}
		private float[]	m_SHOcclusion = new float[9];
		public float[]	SHOcclusion
		{
			get { return m_SHOcclusion; }
			set
			{
				if ( value == null || value == m_SHOcclusion )
					return;

				m_SHOcclusion = value;
				if ( m_Viz != VIZ_TYPE.SH )
					return;
				UpdateBitmap();
				Refresh();
			}
		}

		private bool			m_bNormalizeSH = false;
		public bool				NormalizeSH
		{
			get { return m_bNormalizeSH; }
			set
			{
				if ( value == m_bNormalizeSH )
					return;

				m_bNormalizeSH = value;
				if ( m_Viz != VIZ_TYPE.SH )
					return;
				UpdateBitmap();
				Refresh();
			}
		}

		protected Bitmap		m_Bitmap = null;
		protected Probe			m_Probe = null;
		public Probe			Probe
		{
			get { return m_Probe; }
			set
			{
				m_Probe = value;
				if ( m_Probe != null )
					UpdateBitmap();
			}
		}

		private float3	m_RefUp = new float3( 0, 1, 0 );
		private float3	m_At = new float3( 0, 0, 1 );
		public float3		At
		{
			get { return m_At; }
			set
			{
				m_At = value;

				m_Right = m_At.Cross( m_RefUp ).Normalized;
				m_Up = m_Right.Cross( m_At );

				// Scale by FOV
				float	ScaleY = (float) Math.Tan( 0.5 * FOV );
				m_Up *= ScaleY;
				float	ScaleX = ScaleY * Width / Height;
				m_Right *= ScaleX;

				UpdateBitmap();
			}
		}

		private float3	m_Right = new float3();
		private float3	m_Up = new float3();

		public OutputPanel( IContainer container )
		{
			for ( int i=0; i < 9; i++ )
			{
				m_SHDynamic[i] = float3.Zero;
				m_SHStatic[i] = float3.Zero;
				m_SHEmissive[i] = float3.Zero;
			}

			container.Add( this );

			InitializeComponent();
		}

		public unsafe void		UpdateBitmap()
		{
			if ( m_Bitmap == null )
				return;

			int		W = m_Bitmap.Width;
			int		H = m_Bitmap.Height;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				G.FillRectangle( Brushes.White, 0, 0, W, H );
			}

			// Fill pixel per pixel
			if ( m_Probe != null && m_Probe.m_CubeMap != null )
			{
				BitmapData	LockedBitmap = m_Bitmap.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );

				CubeMapSampler	S = CubeMapSamplerAlbedo;
				switch ( m_Viz )
				{
					case VIZ_TYPE.ALBEDO:				S = CubeMapSamplerAlbedo; break;
					case VIZ_TYPE.DISTANCE:				S = CubeMapSamplerDistance; break;
					case VIZ_TYPE.NORMAL:				S = CubeMapSamplerNormal; break;
					case VIZ_TYPE.STATIC_LIT:			S = CubeMapSamplerStaticLit; break;
					case VIZ_TYPE.FACE_INDEX:			S = CubeMapSamplerFaceIndex; break;
					case VIZ_TYPE.EMISSIVE_MAT_ID:		S = CubeMapSamplerEmissiveMatID; break;
					case VIZ_TYPE.NEIGHBOR_PROBE_ID:	S = CubeMapSamplerNeighborProbeID; break;
					case VIZ_TYPE.SET_INDEX:			S = CubeMapSamplerSetIndex; break;
					case VIZ_TYPE.SET_ALBEDO:			S = CubeMapSamplerSetAlbedo; break;
					case VIZ_TYPE.SET_DISTANCE:			S = CubeMapSamplerSetDistance; break;
					case VIZ_TYPE.SET_NORMAL:			S = CubeMapSamplerSetNormal; break;
					case VIZ_TYPE.SET_SAMPLES:			S = CubeMapSamplerSetSamples; break;
					case VIZ_TYPE.SH:					S = CubeMapSamplerSH; break;
				}

				float3	View;
				byte			R, G, B, A = 0xFF;
				for ( int Y=0; Y < H; Y++ )
				{
					float	y = 1.0f - 2.0f * (0.5f + Y) / H;
					byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + LockedBitmap.Stride * Y;
					for ( int X=0; X < W; X++ )
					{
						float	x = 2.0f * (0.5f + X) / W - 1.0f;
						View = x * m_Right + y * m_Up + m_At;
						View = View.Normalized;

						SampleCubeMap( View, S, out R, out G, out B );

						*pScanline++ = B;
						*pScanline++ = G;
						*pScanline++ = R;
						*pScanline++ = A;
					}
				}


				m_Bitmap.UnlockBits( LockedBitmap );
			}

			Invalidate();
		}

		#region Samplers

		private void	CubeMapSamplerAlbedo( Probe.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			_R = (byte) Math.Min( 255, 255 * _Pixel.Albedo.x );
			_G = (byte) Math.Min( 255, 255 * _Pixel.Albedo.y );
			_B = (byte) Math.Min( 255, 255 * _Pixel.Albedo.z );
		}

		private void	CubeMapSamplerDistance( Probe.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			byte	C = (byte) Math.Min( 255, 255 * 0.1f * _Pixel.Distance );
			_R = _G = _B = C;
		}

		private void	CubeMapSamplerNormal( Probe.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
#if ABS_NORMAL
			_R = (byte) Math.Min( 255, 255 * Math.Abs( _Pixel.Normal.x) );
			_G = (byte) Math.Min( 255, 255 * Math.Abs( _Pixel.Normal.y) );
			_B = (byte) Math.Min( 255, 255 * Math.Abs( _Pixel.Normal.z) );
#else
			_R = (byte) Math.Min( 255, 127 * (1.0f + _Pixel.Normal.x) );
			_G = (byte) Math.Min( 255, 127 * (1.0f + _Pixel.Normal.y) );
			_B = (byte) Math.Min( 255, 127 * (1.0f + _Pixel.Normal.z) );
#endif
		}

		private void	CubeMapSamplerStaticLit( Probe.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			_R = (byte) Math.Min( 255, 255 * _Pixel.StaticLitColor.x );
			_G = (byte) Math.Min( 255, 255 * _Pixel.StaticLitColor.y );
			_B = (byte) Math.Min( 255, 255 * _Pixel.StaticLitColor.z );
		}

		private void	CubeMapSamplerFaceIndex( Probe.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			byte	C = (byte) (_Pixel.FaceIndex & 0xFF);
			_R = _G = _B = C;
		}

		private void	CubeMapSamplerEmissiveMatID( Probe.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			byte	C = (byte) Math.Min( 255, 255 * ((1+_Pixel.EmissiveMatID) % 4) / 4 );
			_R = _G = _B = C;
		}

		private void	CubeMapSamplerNeighborProbeID( Probe.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			byte	C = (byte) Math.Min( 255, 255 * ((1+_Pixel.NeighborProbeID) % 4) / 4 );
			_R = _G = _B = C;
		}

		private void	CubeMapSamplerSetIndex( Probe.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			Probe.Set	S = _Pixel.ParentSet;
			byte	C = 0;
			if ( S != null && S.SetIndex != -1 && (!m_IsolateSet || S.SetIndex == m_IsolatedSetIndex) )
			{
				C = m_IsolateSet ? (byte) 255 : (byte) (255 * (1 + S.SetIndex) / m_Probe.m_Sets.Length);
			}
			_R = _G = _B = C;
		}

		private void	CubeMapSamplerSetAlbedo( Probe.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			Probe.Set	S = _Pixel.ParentSet;
			if ( S == null || S.SetIndex == -1 || (m_IsolateSet && S.SetIndex != m_IsolatedSetIndex) )
			{
				_R = _G = _B = 0;
				return;
			}

			_R = (byte) Math.Min( 255, 255 * S.Albedo.x );
			_G = (byte) Math.Min( 255, 255 * S.Albedo.y );
			_B = (byte) Math.Min( 255, 255 * S.Albedo.z );
		}

		private void	CubeMapSamplerSetDistance( Probe.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			Probe.Set	S = _Pixel.ParentSet;
			if ( S == null || S.SetIndex == -1 || (m_IsolateSet && S.SetIndex != m_IsolatedSetIndex) )
			{
				_R = 63;
				_G = 0;
				_B = 63;
				return;
			}

			float	Distance2SetCenter = 0.2f * (_Pixel.Position - S.Position).Length;

			byte	C = (byte) Math.Min( 255, 255 * Distance2SetCenter );
			_R = _G = _B = C;
		}

		private void	CubeMapSamplerSetNormal( Probe.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			Probe.Set	S = _Pixel.ParentSet;
			if ( S == null || S.SetIndex == -1 || (m_IsolateSet && S.SetIndex != m_IsolatedSetIndex) )
			{
				_R = _G = _B = 0;
				return;
			}

#if ABS_NORMAL
			_R = (byte) Math.Min( 255, 255 * Math.Abs( S.Normal.x) );
			_G = (byte) Math.Min( 255, 255 * Math.Abs( S.Normal.y) );
			_B = (byte) Math.Min( 255, 255 * Math.Abs( S.Normal.z) );
#else
			_R = (byte) Math.Min( 255, 127 * (1.0f + S.Normal.x) );
			_G = (byte) Math.Min( 255, 127 * (1.0f + S.Normal.y) );
			_B = (byte) Math.Min( 255, 127 * (1.0f + S.Normal.z) );
#endif
		}

		private void	CubeMapSamplerSH( Probe.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			float3	Dir = _Pixel.View;

			// Dot the SH together
			float3	Color = float3.Zero;
			if ( m_IsolateSet )
			{
				float	Factor = 1.0f;
				if ( m_bShowSHDynamic )
				{
					for ( int i=0; i < 9; i++ )
						Color += (float) _Pixel.SHCoeffs[i] * m_Probe.m_Sets[m_IsolatedSetIndex].SH[i];

					Factor = m_bNormalizeSH ? 2.0f * m_Probe.m_Sets[m_IsolatedSetIndex].SH[0].Max() : 1.0f;
				}

				if ( m_bShowSHEmissive )
				{
					int		EmissiveSetIndex = Math.Min( m_IsolatedSetIndex, m_Probe.m_EmissiveSets.Length-1 );
					if ( EmissiveSetIndex >= 0 )
						for ( int i=0; i < 9; i++ )
							Color += (float) _Pixel.SHCoeffs[i] * m_Probe.m_EmissiveSets[EmissiveSetIndex].SH[i];

					Factor = m_bNormalizeSH ? 2.0f * m_Probe.m_EmissiveSets[EmissiveSetIndex].SH[0].Max() : 1.0f;
				}

//				Color *= 100.0f;
				Color *= 1.0f / Factor;
			}
			else
			{
				float	Factor = 0.0f;
				if ( m_bShowSHStatic )
				{
					for ( int i=0; i < 9; i++ )
						Color += (float) _Pixel.SHCoeffs[i] * m_SHStatic[i];
					Factor = Math.Max( Factor, m_SHStatic[0].Max() );
				}
				if ( m_bShowSHDynamic )
				{
					for ( int i=0; i < 9; i++ )
						Color += (float) _Pixel.SHCoeffs[i] * m_SHDynamic[i];
					Factor = Math.Max( Factor, m_SHDynamic[0].Max() );
				}
				if ( m_bShowSHEmissive )
				{
					for ( int i=0; i < 9; i++ )
						Color += (float) _Pixel.SHCoeffs[i] * m_SHEmissive[i];
					Factor = Math.Max( Factor, m_SHEmissive[0].Max() );
				}
				if ( m_bShowSHOcclusion )
				{
					for ( int i=0; i < 9; i++ )
						Color += (float) _Pixel.SHCoeffs[i] * m_SHOcclusion[i] * float3.One;
					Factor = Math.Max( Factor, m_SHOcclusion[0] );
				}

//				Color *= 50.0f;

				Color *= m_bNormalizeSH ? 1.0f / Factor : 1.0f;
			}

			if ( Color.x < 0.0f || Color.y < 0.0f || Color.z < 0.0f )
				Color.Set( 1, 0, 1 );

			_R = (byte) Math.Min( 255, 255 * Color.x );
			_G = (byte) Math.Min( 255, 255 * Color.y );
			_B = (byte) Math.Min( 255, 255 * Color.z );
		}

		private void	CubeMapSamplerSetSamples( Probe.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			Probe.Set	S = _Pixel.ParentSet;
			if ( S == null || S.SetIndex == -1 || S.EmissiveMatID != -1 || (m_IsolateSet && S.SetIndex != m_IsolatedSetIndex) )
			{
				_R = 0;
				_G = 0;
				_B = 0;
				return;
			}

// 			float	Distance2SetCenter = 0.2f * (_Pixel.Position - S.Position).Length;
// 			byte	C = (byte) Math.Min( 255, 255 * Distance2SetCenter );

			byte	C = (byte) (255 * (1+_Pixel.ParentSetSampleIndex) / S.Samples.Length);

			_R = _G = _B = C;
		}

		#endregion

		protected override void OnSizeChanged( EventArgs e )
		{
			if ( m_Bitmap != null )
				m_Bitmap.Dispose();

			m_Bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			At = m_At;		// Update view transform

			UpdateBitmap();

			base.OnSizeChanged( e );
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			if ( m_Bitmap != null )
				e.Graphics.DrawImage( m_Bitmap, 0, 0 );
		}

		#region Cube Map Sampling

		public delegate void	CubeMapSampler( Probe.Pixel _Pixel, out byte _R, out byte _G, out byte _B );
		private float3	AbsView = new float3();
		private float3	fXYZ = new float3();
		private float2	fXY = new float2();
		public void		SampleCubeMap( float3 _View, CubeMapSampler _Sampler, out byte _R, out byte _G, out byte _B )
		{
			AbsView.Set( Math.Abs( _View.x ), Math.Abs( _View.y ), Math.Abs( _View.z ) );
			float	MaxComponent = Math.Max( Math.Max( AbsView.x, AbsView.y ), AbsView.z );

			fXYZ.Set( _View.x / MaxComponent, _View.y / MaxComponent, _View.z / MaxComponent );
			int		FaceIndex = 0;
			if ( Math.Abs( fXYZ.x ) > 1.0-1e-6 )
			{	// +X or -X
				if ( _View.x > 0.0 )
				{
					FaceIndex = 0;
					fXY.Set( -fXYZ.z, fXYZ.y );
				}
				else
				{
					FaceIndex = 1;
					fXY.Set( fXYZ.z, fXYZ.y );
				}
			}
			else if ( Math.Abs( fXYZ.y ) > 1.0-1e-6 )
			{	// +Y or -Y
				if ( _View.y > 0.0 )
				{
					FaceIndex = 2;
					fXY.Set( fXYZ.x, -fXYZ.z );
				}
				else
				{
					FaceIndex = 3;
					fXY.Set( fXYZ.x, fXYZ.z );
				}
			}
			else // if ( Math.Abs( fXYZ.z ) > 1.0-1e-6 )
			{	// +Z or -Z
				if ( _View.z > 0.0 )
				{
					FaceIndex = 4;
					fXY.Set( fXYZ.x, fXYZ.y );
				}
				else
				{
					FaceIndex = 5;
					fXY.Set( -fXYZ.x, fXYZ.y );
				}
			}

			fXY.y = -fXY.y;

			int		X = (int) (Probe.CUBE_MAP_SIZE * 0.5 * (1.0 + fXY.x));
			int		Y = (int) (Probe.CUBE_MAP_SIZE * 0.5 * (1.0 + fXY.y));

// 			if ( X < 0 || X > Probe.CUBE_MAP_SIZE-1 )
// 				throw new Exception();
// 			if ( Y < 0 || Y > Probe.CUBE_MAP_SIZE-1 )
// 				throw new Exception();

			X = Math.Min( Probe.CUBE_MAP_SIZE-1, X );
			Y = Math.Min( Probe.CUBE_MAP_SIZE-1, Y );

			Probe.Pixel[,]	CubeMapFace = m_Probe.m_CubeMap[FaceIndex];

			_Sampler( CubeMapFace[X,Y], out _R, out _G, out _B );
		}

		#endregion

		#region Manipulation

		private bool			m_LeftButtonDown = false;
		private Point			m_ButtonDownPosition;
		private float3			m_ButtonDownAt;
		protected override void OnMouseDown( MouseEventArgs e )
		{
			base.OnMouseDown( e );

			m_LeftButtonDown |= (e.Button & System.Windows.Forms.MouseButtons.Left) != 0;
			m_ButtonDownPosition = e.Location;
			m_ButtonDownAt = m_At;
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			base.OnMouseUp( e );

			m_LeftButtonDown &= (e.Button & System.Windows.Forms.MouseButtons.Left) == 0;
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			if ( !m_LeftButtonDown )
				return;

			int		MotionX = e.Location.X - m_ButtonDownPosition.X;
			int		MotionY = e.Location.Y - m_ButtonDownPosition.Y;
			float	AngleX = 1.5f * (float) Math.PI * MotionX / Width;
			float	AngleY = -1.2f * (float) Math.PI * MotionY / Height;

			WMath.Matrix3x3	RotX = new WMath.Matrix3x3( WMath.Matrix3x3.INIT_TYPES.ROT_Y, AngleX );
			WMath.Matrix3x3	RotY = new WMath.Matrix3x3( WMath.Matrix3x3.INIT_TYPES.ROT_X, AngleY );
			WMath.Matrix3x3	Rot = RotY * RotX;

			float3	NewAt = m_ButtonDownAt * Rot;

			this.At = NewAt;

			// Force refresh for faster update...
			Refresh();
		}

		#endregion
	}
}
