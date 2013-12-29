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

namespace ProbeSHEncoder
{
	public partial class OutputPanel : Panel
	{
		private const double			FOV = 0.5 * Math.PI;

		public enum		VIZ_TYPE
		{
			ALBEDO,
			DISTANCE,
			NORMAL,
			SET_INDEX,
			SET_ALBEDO,
			SET_DISTANCE,
			SET_NORMAL,
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
				if ( m_Viz > VIZ_TYPE.NORMAL )
					UpdateBitmap();
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
				if ( m_Viz > VIZ_TYPE.NORMAL )
					UpdateBitmap();
			}
		}

		protected Bitmap		m_Bitmap = null;
		public EncoderForm		m_Owner = null;

		private WMath.Vector	m_RefUp = new WMath.Vector( 0, 1, 0 );
		private WMath.Vector	m_At = new WMath.Vector( 0, 0, 1 );
		public WMath.Vector		At
		{
			get { return m_At; }
			set
			{
				m_At = value;

				m_Right = (m_At ^ m_RefUp).Normalized;
				m_Up = m_Right ^ m_At;

				// Scale by FOV
				float	ScaleY = (float) Math.Tan( 0.5 * FOV );
				m_Up *= ScaleY;
				float	ScaleX = ScaleY * Width / Height;
				m_Right *= ScaleX;

				UpdateBitmap();
			}
		}

		private WMath.Vector	m_Right = new WMath.Vector();
		private WMath.Vector	m_Up = new WMath.Vector();

		public OutputPanel( IContainer container )
		{
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
			if ( m_Owner != null && m_Owner.m_CubeMap != null )
			{
				BitmapData	LockedBitmap = m_Bitmap.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );

				CubeMapSampler	S = CubeMapSamplerAlbedo;
				switch ( m_Viz )
				{
					case VIZ_TYPE.ALBEDO:			S = CubeMapSamplerAlbedo; break;
					case VIZ_TYPE.DISTANCE:			S = CubeMapSamplerDistance; break;
					case VIZ_TYPE.NORMAL:			S = CubeMapSamplerNormal; break;
					case VIZ_TYPE.SET_INDEX:		S = CubeMapSamplerSetIndex; break;
					case VIZ_TYPE.SET_ALBEDO:		S = CubeMapSamplerSetAlbedo; break;
					case VIZ_TYPE.SET_DISTANCE:		S = CubeMapSamplerSetDistance; break;
					case VIZ_TYPE.SET_NORMAL:		S = CubeMapSamplerSetNormal; break;
				}

				WMath.Vector	View;
				byte			R, G, B, A = 0xFF;
				for ( int Y=0; Y < H; Y++ )
				{
					float	y = 1.0f - 2.0f * (0.5f + Y) / H;
					byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + LockedBitmap.Stride * Y;
					for ( int X=0; X < W; X++ )
					{
						float	x = 2.0f * (0.5f + X) / W - 1.0f;
						View = x * m_Right + y * m_Up + m_At;
						View.Normalize();

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

		private void	CubeMapSamplerAlbedo( EncoderForm.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			_R = (byte) Math.Min( 255, 255 * _Pixel.Albedo.x );
			_G = (byte) Math.Min( 255, 255 * _Pixel.Albedo.y );
			_B = (byte) Math.Min( 255, 255 * _Pixel.Albedo.z );
		}

		private void	CubeMapSamplerDistance( EncoderForm.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			byte	C = (byte) Math.Min( 255, 255 * 0.1f * _Pixel.Distance );
			_R = _G = _B = C;
		}

		private void	CubeMapSamplerNormal( EncoderForm.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
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

		private void	CubeMapSamplerSetIndex( EncoderForm.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			EncoderForm.Set	S = _Pixel.ParentSet;
			byte	C = 0;
			if ( S != null && (!m_IsolateSet || S.SetIndex == m_IsolatedSetIndex) )
			{
				C = (byte) (255 * (1 + S.SetIndex) / m_Owner.m_Sets.Length);
			}
			_R = _G = _B = C;
		}

		private void	CubeMapSamplerSetAlbedo( EncoderForm.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			EncoderForm.Set	S = _Pixel.ParentSet;
			if ( S == null || (m_IsolateSet && S.SetIndex != m_IsolatedSetIndex) )
			{
				_R = _G = _B = 0;
				return;
			}

			_R = (byte) Math.Min( 255, 255 * S.Albedo.x );
			_G = (byte) Math.Min( 255, 255 * S.Albedo.y );
			_B = (byte) Math.Min( 255, 255 * S.Albedo.z );
		}

		private void	CubeMapSamplerSetDistance( EncoderForm.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			EncoderForm.Set	S = _Pixel.ParentSet;
			if ( S == null || (m_IsolateSet && S.SetIndex != m_IsolatedSetIndex) )
			{
				_R = 63;
				_G = 0;
				_B = 63;
				return;
			}

			float	Distance2SetCenter = (_Pixel.Position - S.Position).Length();

			byte	C = (byte) Math.Min( 255, 255 * 0.1f * Distance2SetCenter );
			_R = _G = _B = C;
		}

		private void	CubeMapSamplerSetNormal( EncoderForm.Pixel _Pixel, out byte _R, out byte _G, out byte _B )
		{
			EncoderForm.Set	S = _Pixel.ParentSet;
			if ( S == null || (m_IsolateSet && S.SetIndex != m_IsolatedSetIndex) )
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

		public delegate void	CubeMapSampler( EncoderForm.Pixel _Pixel, out byte _R, out byte _G, out byte _B );
		private WMath.Vector	AbsView = new WMath.Vector();
		private WMath.Vector	fXYZ = new WMath.Vector();
		private WMath.Vector2D	fXY = new WMath.Vector2D();
		public void		SampleCubeMap( WMath.Vector _View, CubeMapSampler _Sampler, out byte _R, out byte _G, out byte _B )
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

			int		X = (int) (EncoderForm.CUBE_MAP_SIZE * 0.5 * (1.0 + fXY.x));
			int		Y = (int) (EncoderForm.CUBE_MAP_SIZE * 0.5 * (1.0 + fXY.y));

// 			if ( X < 0 || X > EncoderForm.CUBE_MAP_SIZE-1 )
// 				throw new Exception();
// 			if ( Y < 0 || Y > EncoderForm.CUBE_MAP_SIZE-1 )
// 				throw new Exception();

			X = Math.Min( EncoderForm.CUBE_MAP_SIZE-1, X );
			Y = Math.Min( EncoderForm.CUBE_MAP_SIZE-1, Y );

			EncoderForm.Pixel[,]	CubeMapFace = m_Owner.m_CubeMap[FaceIndex];

			_Sampler( CubeMapFace[X,Y], out _R, out _G, out _B );
		}

		#endregion

		#region Manipulation

		private bool			m_LeftButtonDown = false;
		private Point			m_ButtonDownPosition;
		private WMath.Vector	m_ButtonDownAt = null;
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

			WMath.Vector	NewAt = m_ButtonDownAt * Rot;

			this.At = NewAt;

			// Force refresh for faster update...
			Refresh();
		}

		#endregion
	}
}
