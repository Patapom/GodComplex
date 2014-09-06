using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace StandardizedDiffuseAlbedoMaps
{
	public partial class ReferenceImagePanel : Panel
	{
		private Bitmap		m_Bitmap = null;
		private Pen			m_PenProbeShadow = new Pen( Color.FromArgb( 0, 0, 0 ), 3.0f );
		private Pen			m_PenProbe = new Pen( Color.Gold, 2.0f );
		private Pen			m_PenProbeInvalid = new Pen( Color.Red, 2.0f );

		private Bitmap		m_Thumbnail = null;

		private CameraCalibration	m_CameraCalibration = null;
		public unsafe CameraCalibration	Calibration
		{
			get { return m_CameraCalibration; }
			set {
				m_CameraCalibration = value;

				if ( m_CameraCalibration != null && m_CameraCalibration.m_Thumbnail != null )
				{
					int		W = m_CameraCalibration.m_Thumbnail.GetLength(0);
					int		H = m_CameraCalibration.m_Thumbnail.GetLength(1);
					if ( m_Thumbnail == null || m_Thumbnail.Width != W || m_Thumbnail.Height != H )
					{
						if ( m_Thumbnail != null )
							m_Thumbnail.Dispose();

						m_Thumbnail = new Bitmap( W, H, PixelFormat.Format32bppArgb );
					}

					// Fill pixels
					BitmapData	LockedBitmap = m_Thumbnail.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
					for ( int Y=0; Y < H; Y++ )
					{
						byte*	pScanline = (byte*) LockedBitmap.Scan0 + LockedBitmap.Stride * Y;
						for ( int X=0; X < W; X++ )
						{
							byte	L = (byte) (255.0f * ImageUtility.ColorProfile.Linear2sRGB( m_CameraCalibration.m_Thumbnail[X,Y] / 255.0f ));
							*pScanline++ = L;
							*pScanline++ = L;
							*pScanline++ = L;
							*pScanline++ = 0xFF;
						}
					}
					m_Thumbnail.UnlockBits( LockedBitmap );
				}
				else
				{
					if ( m_Thumbnail != null )
						m_Thumbnail.Dispose();
					m_Thumbnail = null;
				}

				UpdateBitmap();
			}
		}

		public ReferenceImagePanel( IContainer container )
		{
			container.Add( this );
			InitializeComponent();
			OnSizeChanged( EventArgs.Empty );
		}

		public void		UpdateBitmap()
		{
			if ( m_Bitmap == null )
				return;

			int		W = m_Bitmap.Width;
			int		H = m_Bitmap.Height;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				using ( SolidBrush B = new SolidBrush( Color.Black ) )
					G.FillRectangle( B, 0, 0, W, H );

				if ( m_CameraCalibration != null && m_Thumbnail != null )
				{
					// Draw thumbnail
					RectangleF	ClientRect = ImageClientRect();
					G.DrawImage( m_Thumbnail, ClientRect, new RectangleF( 0, 0, m_Thumbnail.Width, m_Thumbnail.Height ), GraphicsUnit.Pixel );

					// Draw probe measurement circles if available
					for ( int ProbeIndex=0; ProbeIndex < 6; ProbeIndex++ )
					{
						CameraCalibration.Probe	P = m_CameraCalibration.m_Reflectances[ProbeIndex];
						if ( !P.m_MeasurementDiscIsAvailable )
							continue;

						PointF	ClientPos = ImageUV2Client( new PointF( P.m_MeasurementCenterX, P.m_MeasurementCenterY ) );
						float	ClientRadius = ClientRect.Width * P.m_MeasurementRadius;

//						G.DrawEllipse( m_PenProbeShadow, ClientPos.X - ClientRadius, ClientPos.Y - ClientRadius, 2*ClientRadius, 2*ClientRadius );
						G.DrawEllipse( P.m_IsAvailable ? m_PenProbe : m_PenProbeInvalid, ClientPos.X - ClientRadius, ClientPos.Y - ClientRadius, 2*ClientRadius, 2*ClientRadius );
					}
				}
			}

			Invalidate();
		}

		private RectangleF		ImageClientRect()
		{
			int		SizeX = m_Thumbnail.Width;
			int		SizeY = m_Thumbnail.Height;

			int		WidthIfVertical = SizeX * Height / SizeY;	// Client width of the image if fitting vertically
			int		HeightIfHorizontal = SizeY * Width / SizeX;	// Client height of the image if fitting horizontally

			if ( WidthIfVertical > Width )
			{	// Fit horizontally
				return new RectangleF( 0, 0.5f * (Height-HeightIfHorizontal), Width, HeightIfHorizontal );
			}
			else
			{	// Fit vertically
				return new RectangleF( 0.5f * (Width-WidthIfVertical), 0, WidthIfVertical, Height );
			}
		}

		private PointF	Client2ImageUV( PointF _Position )
		{
			RectangleF	ImageRect = ImageClientRect();
			return new PointF( (_Position.X - ImageRect.X) / ImageRect.Width, (_Position.Y - ImageRect.Y) / ImageRect.Height );
		}

		private PointF	ImageUV2Client( PointF _Position )
		{
			RectangleF	ImageRect = ImageClientRect();
			return new PointF( _Position.X * ImageRect.Width + ImageRect.X, _Position.Y * ImageRect.Height + ImageRect.Y );
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			if ( m_Bitmap != null )
				m_Bitmap.Dispose();

			m_Bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

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

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );
		}
	}
}
