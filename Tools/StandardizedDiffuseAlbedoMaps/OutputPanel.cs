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

namespace StandardizedDiffuseAlbedoMaps
{
	public partial class OutputPanel : Panel
	{
		private Bitmap		m_Bitmap = null;
		private Pen			m_PenTarget = new Pen( Color.Gold, 2.0f );

		private float3[,]	m_Image = null;
		public float3[,]	Image
		{
			get { return m_Image; }
			set {
				m_Image = value;
				UpdateBitmap();
			}
		}

		public OutputPanel( IContainer container )
		{
			container.Add( this );
			InitializeComponent();
			OnSizeChanged( EventArgs.Empty );
		}

		public delegate void	CalibrationDone( PointF _Center, float _Radius );
		private CalibrationDone	m_CalibrationDelegate = null;
		private enum	CALIBRATION_STAGE
		{
			STOPPED,
			PICK_CENTER,
			SET_RADIUS,
		}
		private CALIBRATION_STAGE	m_CalibrationStage = CALIBRATION_STAGE.STOPPED;
		private PointF				m_CalibrationCenter = PointF.Empty;
		private float				m_CalibrationRadius = 0.0f;
		public void				StartCalibrationTargetPicking( CalibrationDone _Notify )
		{
			m_CalibrationDelegate = _Notify;
			m_CalibrationStage = CALIBRATION_STAGE.PICK_CENTER;
			m_CalibrationRadius = 0.0f;
		}

		public unsafe void		UpdateBitmap()
		{
			if ( m_Bitmap == null )
				return;

			int		W = m_Bitmap.Width;
			int		H = m_Bitmap.Height;

			// Fill pixel per pixel
			if ( m_Image != null )
			{
				int		SizeX = m_Image.GetLength( 0 );
				int		SizeY = m_Image.GetLength( 1 );

				RectangleF	ImageRect = ImageClientRect();

				BitmapData	LockedBitmap = m_Bitmap.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
				byte		R, G, B, A = 0xFF;
				for ( int Y=0; Y < H; Y++ )
				{
					byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + LockedBitmap.Stride * Y;
					for ( int X=0; X < W; X++ )
					{
						if ( X >= ImageRect.X && X < ImageRect.Right && Y >= ImageRect.Y && Y < ImageRect.Bottom )
						{
							float3	RGB = m_Image[(int) (SizeX*(X-ImageRect.X)/ImageRect.Width), (int) (SizeY*(Y-ImageRect.Y)/ImageRect.Height)];
							R = (byte) Math.Min( 255, 255.0f * RGB.x );
							G = (byte) Math.Min( 255, 255.0f * RGB.y );
							B = (byte) Math.Min( 255, 255.0f * RGB.z );
						}
						else
						{
							R = G = B = 0;
						}
						*pScanline++ = B;
						*pScanline++ = G;
						*pScanline++ = R;
						*pScanline++ = A;
					}
				}
				m_Bitmap.UnlockBits( LockedBitmap );
			}
			else
			{
				using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
					G.FillRectangle( Brushes.Black, 0, 0, W, H );
			}

			Invalidate();
		}

		private RectangleF		ImageClientRect()
		{
			int		SizeX = m_Image.GetLength( 0 );
			int		SizeY = m_Image.GetLength( 1 );

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
//				e.Graphics.DrawImage( m_Bitmap, new Rectangle( 0, 0, Width, Height ), 0, 0, m_Bitmap.Width, m_Bitmap.Height, GraphicsUnit.Pixel );

			if ( m_CalibrationStage == CALIBRATION_STAGE.STOPPED )
				return;

			// Paint the calibration target
			PointF	Center = ImageUV2Client( m_CalibrationCenter );
			e.Graphics.DrawLine( m_PenTarget, Center.X, Center.Y-20, Center.X, Center.Y+20 );
			e.Graphics.DrawLine( m_PenTarget, Center.X-20, Center.Y, Center.X+20, Center.Y );

			PointF	Temp = ImageUV2Client( new PointF( m_CalibrationCenter.X + m_CalibrationRadius, m_CalibrationCenter.Y ) );
			float	ClientRadius = Temp.X - Center.X;
			e.Graphics.DrawEllipse( m_PenTarget, Center.X-ClientRadius, Center.Y-ClientRadius, 2.0f*ClientRadius, 2.0f*ClientRadius );
		}

		protected override void OnMouseDown( MouseEventArgs e )
		{
			base.OnMouseDown( e );

			if ( m_CalibrationStage == CALIBRATION_STAGE.PICK_CENTER )
				m_CalibrationStage = CALIBRATION_STAGE.SET_RADIUS;
			else if ( m_CalibrationStage == CALIBRATION_STAGE.SET_RADIUS )
			{	// We're done! Notify!
				m_CalibrationStage = CALIBRATION_STAGE.STOPPED;
				if ( m_CalibrationDelegate != null )
					m_CalibrationDelegate( m_CalibrationCenter, m_CalibrationRadius );
				Invalidate();
			}
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			switch ( m_CalibrationStage )
			{
				case CALIBRATION_STAGE.PICK_CENTER:
					m_CalibrationCenter = Client2ImageUV( e.Location );
					Invalidate();
					break;

				case CALIBRATION_STAGE.SET_RADIUS:
					{
						PointF	Temp = Client2ImageUV( e.Location );
						m_CalibrationRadius = (float) Math.Sqrt( (Temp.X - m_CalibrationCenter.X)*(Temp.X - m_CalibrationCenter.X) + (Temp.Y - m_CalibrationCenter.Y)*(Temp.Y - m_CalibrationCenter.Y) );
						Invalidate();
					}
					break;
			}
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			base.OnMouseUp( e );
		}
	}
}
