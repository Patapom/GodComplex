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
		#region NESTED TYPES

		public delegate void	CalibrationDone( PointF _Center, float _Radius );
		public delegate void	ColorPickingUpdate( PointF _Position );

		private enum	MANIPULATION_STATE
		{
			STOPPED,			// No manipulation currently taking place
			CALIBRATION_TARGET,	// User is selecting the calibration target
			CROP_RECTANGLE,		// User is modifying the crop rectangle
			PICK_COLOR,			// User is picking a color
		}

		private enum	CALIBRATION_STAGE
		{
			STOPPED,
			PICK_CENTER,
			SET_RADIUS,
		}

		private enum	CROP_RECTANGLE_SPOT
		{
			NONE,

			CORNER_TOP_LEFT,
			CORNER_TOP_RIGHT,
			CORNER_BOTTOM_LEFT,
			CORNER_BOTTOM_RIGHT,

			TOP,
			BOTTOM,
			LEFT,
			RIGHT,

			ROTATE_TOP_LEFT,
			ROTATE_TOP_RIGHT,
			ROTATE_BOTTOM_LEFT,
			ROTATE_BOTTOM_RIGHT,
		}

		#endregion

		#region FIELDS

		private Bitmap				m_Bitmap = null;

		// Source image in RGB space
		private float3[,]			m_Image = null;

		private MANIPULATION_STATE	m_ManipulationState = MANIPULATION_STATE.STOPPED;

		// Calibration luminance target manipulation
		private Pen					m_PenTarget = new Pen( Color.Gold, 2.0f );

		private CalibrationDone		m_CalibrationDelegate = null;
		private CALIBRATION_STAGE	m_CalibrationStage;
		private PointF				m_CalibrationCenter = PointF.Empty;
		private float				m_CalibrationRadius = 0.0f;

		// Crop rectangle manipulation
		private Pen					m_PenCropRectangle = new Pen( Color.Red, 1.0f );
		private SolidBrush			m_BrushCroppedZone = new SolidBrush( Color.FromArgb( 128, Color.White ) );

		private PointF				m_CropRectangleCenter;
		private PointF				m_CropRectangleHalfSize;
		private float				m_CropRectangleRotation;

		private bool				m_CropRectangleManipulationStarted = false;
		private CROP_RECTANGLE_SPOT	m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.NONE;
		private PointF				m_CropRectangeManipulationMousePositionButtonDown;

		// Color picking manipulation
		private ColorPickingUpdate	m_ColorPickingDelegate = null;


		#endregion

		#region PROPERTIES

		public float3[,]	Image
		{
			get { return m_Image; }
			set {
				m_Image = value;
				UpdateBitmap();
			}
		}

		public bool			CropRectangleEnabled
		{
			get { return m_ManipulationState == MANIPULATION_STATE.CROP_RECTANGLE; }
			set { ManipulationState = value ? MANIPULATION_STATE.CROP_RECTANGLE : MANIPULATION_STATE.STOPPED; }
		}

		private MANIPULATION_STATE	ManipulationState
		{
			get { return m_ManipulationState; }
			set
			{
				if ( value == m_ManipulationState )
					return;

				m_ManipulationState = value;
				Invalidate();
			}
		}

		/// <summary>
		/// If true then don't crop the source image
		/// </summary>
		public bool			IsDefaultCropRectangle
		{
			get
			{
				return Math.Abs( m_CropRectangleCenter.X - 0.5f ) < 1e-6f
					&& Math.Abs( m_CropRectangleCenter.Y - 0.5f ) < 1e-6f
					&& Math.Abs( m_CropRectangleHalfSize.X - 0.5f ) < 1e-6f
					&& Math.Abs( m_CropRectangleHalfSize.Y - 0.5f ) < 1e-6f
					&& Math.Abs( m_CropRectangleRotation - 0.0f ) < 1e-6f;
			}
		}

		public PointF		CropRectangeCenter		{ get { return m_CropRectangleCenter; } }
		public PointF		CropRectangeHalfSize	{ get { return m_CropRectangleHalfSize; } }
		public float		CropRectangeRotation	{ get { return m_CropRectangleRotation; } }

		#endregion

		public OutputPanel( IContainer container )
		{
			container.Add( this );
			InitializeComponent();
			OnSizeChanged( EventArgs.Empty );
		}

		/// <summary>
		/// Starts calibration target picking mode (user is expected to place the target with the mouse, then change the radius so the calibration can take place)
		/// </summary>
		/// <param name="_Notify"></param>
		public void				StartCalibrationTargetPicking( CalibrationDone _Notify )
		{
			m_CalibrationDelegate = _Notify;
			m_CalibrationRadius = 0.0f;
			ManipulationState = MANIPULATION_STATE.CALIBRATION_TARGET;
		}

		/// <summary>
		/// Starts color picking with the mouse, provided delegate is called on mouse move with new informations
		/// </summary>
		/// <param name="_Notify"></param>
		public void				StartSwatchColorPicking( ColorPickingUpdate _Notify )
		{
			m_ColorPickingDelegate = _Notify;
			ManipulationState = MANIPULATION_STATE.PICK_COLOR;
		}

		/// <summary>
		/// Resets the crop rectangle to the entire image
		/// </summary>
		public void				ResetCropRectangle()
		{
			if ( m_Image == null )
				return;

			RectangleF	ImageRect = ImageClientRect();

			m_CropRectangleCenter = new PointF( 0.5f, 0.5f );
			m_CropRectangleHalfSize = new PointF( 0.5f, 0.5f );
			m_CropRectangleRotation = 0.0f;
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

		private PointF		m_CropRectangleAxisX;
		private PointF		m_CropRectangleAxisY;
		private PointF[]	m_CropRectangleVertices = new PointF[4];	// Top-left, top-right, bottom-left, bottom-right
		private void		UpdateCropRectangleVertices()
		{
			m_CropRectangleAxisX = new PointF( (float) Math.Cos( m_CropRectangleRotation ), -(float) Math.Sin( m_CropRectangleRotation ) );
			m_CropRectangleAxisY = new PointF( -(float) Math.Sin( m_CropRectangleRotation ), -(float) Math.Cos( m_CropRectangleRotation ) );

			RectangleF	ImageRect = ImageClientRect();

			PointF	Center = ImageUV2Client( m_CropRectangleCenter );
//			PointF	HalfSize = new PointF( m_Image.GetLength( 0 ) * m_CropRectangleHalfSize.X, m_Image.GetLength( 1 ) * m_CropRectangleHalfSize.Y );
			PointF	HalfSize = new PointF( ImageRect.Width * m_CropRectangleHalfSize.X, ImageRect.Height * m_CropRectangleHalfSize.Y );

			m_CropRectangleVertices[0] = new PointF(	Center.X - HalfSize.X * m_CropRectangleAxisX.X + HalfSize.Y * m_CropRectangleAxisY.X,
														Center.Y - HalfSize.X * m_CropRectangleAxisX.Y + HalfSize.Y * m_CropRectangleAxisY.Y );
			m_CropRectangleVertices[1] = new PointF(	Center.X + HalfSize.X * m_CropRectangleAxisX.X + HalfSize.Y * m_CropRectangleAxisY.X,
														Center.Y + HalfSize.X * m_CropRectangleAxisX.Y + HalfSize.Y * m_CropRectangleAxisY.Y );
			m_CropRectangleVertices[2] = new PointF(	Center.X - HalfSize.X * m_CropRectangleAxisX.X - HalfSize.Y * m_CropRectangleAxisY.X,
														Center.Y - HalfSize.X * m_CropRectangleAxisX.Y - HalfSize.Y * m_CropRectangleAxisY.Y );
			m_CropRectangleVertices[3] = new PointF(	Center.X + HalfSize.X * m_CropRectangleAxisX.X - HalfSize.Y * m_CropRectangleAxisY.X,
														Center.Y + HalfSize.X * m_CropRectangleAxisX.Y - HalfSize.Y * m_CropRectangleAxisY.Y );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			if ( m_Bitmap != null )
				e.Graphics.DrawImage( m_Bitmap, 0, 0 );
//				e.Graphics.DrawImage( m_Bitmap, new Rectangle( 0, 0, Width, Height ), 0, 0, m_Bitmap.Width, m_Bitmap.Height, GraphicsUnit.Pixel );

			if ( m_Image == null )
				return;

			switch ( ManipulationState )
			{
				case MANIPULATION_STATE.CROP_RECTANGLE:
				{
					// Paint the crop rectangle
					UpdateCropRectangleVertices();

					// Draw off-screen area
					const float	F = 20.0f;
//					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] { m_CropRectangleVertices[0], m_CropRectangleVertices[1], m_CropRectangleVertices[3], m_CropRectangleVertices[2] } );
					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_CropRectangleVertices[0],
						new PointF( m_CropRectangleVertices[0].X + F*(+0*m_CropRectangleAxisX.X+1*m_CropRectangleAxisY.X), m_CropRectangleVertices[0].Y + F*(+0*m_CropRectangleAxisX.Y+1*m_CropRectangleAxisY.Y) ),
						new PointF( m_CropRectangleVertices[0].X + F*(-1*m_CropRectangleAxisX.X+1*m_CropRectangleAxisY.X), m_CropRectangleVertices[0].Y + F*(-1*m_CropRectangleAxisX.Y+1*m_CropRectangleAxisY.Y) ),
						new PointF( m_CropRectangleVertices[0].X + F*(-1*m_CropRectangleAxisX.X+0*m_CropRectangleAxisY.X), m_CropRectangleVertices[0].Y + F*(-1*m_CropRectangleAxisX.Y+0*m_CropRectangleAxisY.Y) ),
					} );
					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_CropRectangleVertices[1],
						new PointF( m_CropRectangleVertices[1].X + F*(+1*m_CropRectangleAxisX.X+0*m_CropRectangleAxisY.X), m_CropRectangleVertices[1].Y + F*(+1*m_CropRectangleAxisX.Y+0*m_CropRectangleAxisY.Y) ),
						new PointF( m_CropRectangleVertices[1].X + F*(+1*m_CropRectangleAxisX.X+1*m_CropRectangleAxisY.X), m_CropRectangleVertices[1].Y + F*(+1*m_CropRectangleAxisX.Y+1*m_CropRectangleAxisY.Y) ),
						new PointF( m_CropRectangleVertices[1].X + F*(+0*m_CropRectangleAxisX.X+1*m_CropRectangleAxisY.X), m_CropRectangleVertices[1].Y + F*(+0*m_CropRectangleAxisX.Y+1*m_CropRectangleAxisY.Y) ),
					} );
					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_CropRectangleVertices[2],
						new PointF( m_CropRectangleVertices[2].X + F*(-1*m_CropRectangleAxisX.X+0*m_CropRectangleAxisY.X), m_CropRectangleVertices[2].Y + F*(-1*m_CropRectangleAxisX.Y+0*m_CropRectangleAxisY.Y) ),
						new PointF( m_CropRectangleVertices[2].X + F*(-1*m_CropRectangleAxisX.X-1*m_CropRectangleAxisY.X), m_CropRectangleVertices[2].Y + F*(-1*m_CropRectangleAxisX.Y-1*m_CropRectangleAxisY.Y) ),
						new PointF( m_CropRectangleVertices[2].X + F*(+0*m_CropRectangleAxisX.X-1*m_CropRectangleAxisY.X), m_CropRectangleVertices[2].Y + F*(+0*m_CropRectangleAxisX.Y-1*m_CropRectangleAxisY.Y) ),
					} );
					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_CropRectangleVertices[3],
						new PointF( m_CropRectangleVertices[3].X + F*(+0*m_CropRectangleAxisX.X-1*m_CropRectangleAxisY.X), m_CropRectangleVertices[3].Y + F*(+0*m_CropRectangleAxisX.Y-1*m_CropRectangleAxisY.Y) ),
						new PointF( m_CropRectangleVertices[3].X + F*(+1*m_CropRectangleAxisX.X-1*m_CropRectangleAxisY.X), m_CropRectangleVertices[3].Y + F*(+1*m_CropRectangleAxisX.Y-1*m_CropRectangleAxisY.Y) ),
						new PointF( m_CropRectangleVertices[3].X + F*(+1*m_CropRectangleAxisX.X+0*m_CropRectangleAxisY.X), m_CropRectangleVertices[3].Y + F*(+0*m_CropRectangleAxisX.Y+0*m_CropRectangleAxisY.Y) ),
					} );

					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_CropRectangleVertices[1],
						m_CropRectangleVertices[0],
						new PointF( m_CropRectangleVertices[0].X + F*(+0*m_CropRectangleAxisX.X+1*m_CropRectangleAxisY.X), m_CropRectangleVertices[0].Y + F*(+0*m_CropRectangleAxisX.Y+1*m_CropRectangleAxisY.Y) ),
						new PointF( m_CropRectangleVertices[1].X + F*(+0*m_CropRectangleAxisX.X+1*m_CropRectangleAxisY.X), m_CropRectangleVertices[1].Y + F*(+0*m_CropRectangleAxisX.Y+1*m_CropRectangleAxisY.Y) ),
					} );
					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_CropRectangleVertices[3],
						m_CropRectangleVertices[2],
						new PointF( m_CropRectangleVertices[2].X + F*(+0*m_CropRectangleAxisX.X-1*m_CropRectangleAxisY.X), m_CropRectangleVertices[2].Y + F*(+0*m_CropRectangleAxisX.Y-1*m_CropRectangleAxisY.Y) ),
						new PointF( m_CropRectangleVertices[3].X + F*(+0*m_CropRectangleAxisX.X-1*m_CropRectangleAxisY.X), m_CropRectangleVertices[3].Y + F*(+0*m_CropRectangleAxisX.Y-1*m_CropRectangleAxisY.Y) ),
					} );
					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_CropRectangleVertices[2],
						m_CropRectangleVertices[0],
						new PointF( m_CropRectangleVertices[0].X + F*(-1*m_CropRectangleAxisX.X+0*m_CropRectangleAxisY.X), m_CropRectangleVertices[0].Y + F*(-1*m_CropRectangleAxisX.Y+0*m_CropRectangleAxisY.Y) ),
						new PointF( m_CropRectangleVertices[2].X + F*(-1*m_CropRectangleAxisX.X+0*m_CropRectangleAxisY.X), m_CropRectangleVertices[2].Y + F*(-1*m_CropRectangleAxisX.Y+0*m_CropRectangleAxisY.Y) ),
					} );
					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_CropRectangleVertices[1],
						m_CropRectangleVertices[3],
						new PointF( m_CropRectangleVertices[3].X + F*(+1*m_CropRectangleAxisX.X+0*m_CropRectangleAxisY.X), m_CropRectangleVertices[3].Y + F*(+1*m_CropRectangleAxisX.Y+0*m_CropRectangleAxisY.Y) ),
						new PointF( m_CropRectangleVertices[1].X + F*(+1*m_CropRectangleAxisX.X+0*m_CropRectangleAxisY.X), m_CropRectangleVertices[1].Y + F*(+1*m_CropRectangleAxisX.Y+0*m_CropRectangleAxisY.Y) ),
					} );

					// Draw actual crop rectange
					e.Graphics.DrawPolygon( m_PenCropRectangle, new PointF[] { m_CropRectangleVertices[0], m_CropRectangleVertices[1], m_CropRectangleVertices[3], m_CropRectangleVertices[2] } );
					break;
				}

				case MANIPULATION_STATE.CALIBRATION_TARGET:
				{	// Paint the calibration target
					PointF	Center = ImageUV2Client( m_CalibrationCenter );
					e.Graphics.DrawLine( m_PenTarget, Center.X, Center.Y-20, Center.X, Center.Y+20 );
					e.Graphics.DrawLine( m_PenTarget, Center.X-20, Center.Y, Center.X+20, Center.Y );

					PointF	Temp = ImageUV2Client( new PointF( m_CalibrationCenter.X + m_CalibrationRadius, m_CalibrationCenter.Y ) );
					float	ClientRadius = Temp.X - Center.X;
					e.Graphics.DrawEllipse( m_PenTarget, Center.X-ClientRadius, Center.Y-ClientRadius, 2.0f*ClientRadius, 2.0f*ClientRadius );
					break;
				}
			}
		}

		protected override void OnMouseDown( MouseEventArgs e )
		{
			base.OnMouseDown( e );

			switch ( m_ManipulationState )
			{
				case MANIPULATION_STATE.CROP_RECTANGLE:
				{	// Take care of crop rectangle manipulation
					if ( m_CropRectangleManipulatedSpot == CROP_RECTANGLE_SPOT.NONE )
						return;	// Nothing to manipulate...

					Capture = true;
					m_CropRectangleManipulationStarted = true;
					m_CropRectangeManipulationMousePositionButtonDown = e.Location;
					break;
				}

				case MANIPULATION_STATE.CALIBRATION_TARGET:
				{	// Take care of calibration manipulation
					if ( m_CalibrationStage == CALIBRATION_STAGE.PICK_CENTER )
						m_CalibrationStage = CALIBRATION_STAGE.SET_RADIUS;
					else if ( m_CalibrationStage == CALIBRATION_STAGE.SET_RADIUS )
					{	// We're done! Notify!
						m_CalibrationStage = CALIBRATION_STAGE.STOPPED;
						if ( m_CalibrationDelegate != null )
							m_CalibrationDelegate( m_CalibrationCenter, m_CalibrationRadius );

						// End manipulation
						ManipulationState = MANIPULATION_STATE.STOPPED;
					}
					break;
				}

				case MANIPULATION_STATE.PICK_COLOR:

					// End manipulation
					ManipulationState = MANIPULATION_STATE.STOPPED;

					break;
			}
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			switch ( m_ManipulationState )
			{
				case MANIPULATION_STATE.CROP_RECTANGLE:
				{	// Take care of crop rectangle manipulation
					if ( !m_CropRectangleManipulationStarted )
					{	// Update the cursor based on the hovered manipulation spot
						m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.NONE;

						const float	TOLERANCE = 8.0f;

						PointF	P = e.Location;

						PointF	TopLeftCorner2UV = new PointF( P.X - m_CropRectangleVertices[0].X, P.Y - m_CropRectangleVertices[0].Y );
						PointF	BottomRightCorner2UV = new PointF( P.X - m_CropRectangleVertices[3].X, P.Y - m_CropRectangleVertices[3].Y );

						float	Distance2Left = -TopLeftCorner2UV.X * m_CropRectangleAxisX.X - TopLeftCorner2UV.Y * m_CropRectangleAxisX.Y;
						float	Distance2Top = TopLeftCorner2UV.X * m_CropRectangleAxisY.X + TopLeftCorner2UV.Y * m_CropRectangleAxisY.Y;
						float	Distance2Right = BottomRightCorner2UV.X * m_CropRectangleAxisX.X + BottomRightCorner2UV.Y * m_CropRectangleAxisX.Y;
						float	Distance2Bottom = -BottomRightCorner2UV.X * m_CropRectangleAxisY.X - BottomRightCorner2UV.Y * m_CropRectangleAxisY.Y;

						// Stretch
						if ( Math.Abs( Distance2Left ) < TOLERANCE )
						{
							if ( Math.Abs( Distance2Top ) < TOLERANCE )
								m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.CORNER_TOP_LEFT;
							else if ( Math.Abs( Distance2Bottom ) < TOLERANCE )
								m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.CORNER_BOTTOM_LEFT;
							else
								m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.LEFT;
						}
						else if ( Math.Abs( Distance2Right ) < TOLERANCE )
						{
							if ( Math.Abs( Distance2Top ) < TOLERANCE )
								m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.CORNER_TOP_RIGHT;
							else if ( Math.Abs( Distance2Bottom ) < TOLERANCE )
								m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.CORNER_BOTTOM_RIGHT;
							else
								m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.RIGHT;
						}
						else if ( Math.Abs( Distance2Top ) < TOLERANCE )
							m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.TOP;
						else if ( Math.Abs( Distance2Bottom ) < TOLERANCE )
							m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.BOTTOM;
						// Rotate
						else if ( Distance2Top > TOLERANCE && Distance2Right > TOLERANCE )
							m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.ROTATE_TOP_RIGHT;
						else if ( Distance2Top > TOLERANCE && Distance2Left > TOLERANCE )
							m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.ROTATE_TOP_LEFT;
						else if ( Distance2Bottom > TOLERANCE && Distance2Right > TOLERANCE )
							m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.ROTATE_BOTTOM_RIGHT;
						else if ( Distance2Bottom > TOLERANCE && Distance2Left > TOLERANCE )
							m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.ROTATE_BOTTOM_LEFT;

						// Update cursor accordingly
						switch ( m_CropRectangleManipulatedSpot  )
						{
							case CROP_RECTANGLE_SPOT.NONE: Cursor = Cursors.Default; break;
							case CROP_RECTANGLE_SPOT.LEFT: Cursor = Cursors.SizeWE; break;
							case CROP_RECTANGLE_SPOT.RIGHT: Cursor = Cursors.SizeWE; break;
							case CROP_RECTANGLE_SPOT.TOP: Cursor = Cursors.SizeNS; break;
							case CROP_RECTANGLE_SPOT.BOTTOM: Cursor = Cursors.SizeNS; break;
							case CROP_RECTANGLE_SPOT.CORNER_TOP_LEFT: Cursor = Cursors.SizeNWSE; break;
							case CROP_RECTANGLE_SPOT.CORNER_BOTTOM_RIGHT: Cursor = Cursors.SizeNWSE; break;
							case CROP_RECTANGLE_SPOT.CORNER_TOP_RIGHT: Cursor = Cursors.SizeNESW; break;
							case CROP_RECTANGLE_SPOT.CORNER_BOTTOM_LEFT: Cursor = Cursors.SizeNESW; break;
							case CROP_RECTANGLE_SPOT.ROTATE_TOP_LEFT:
							case CROP_RECTANGLE_SPOT.ROTATE_TOP_RIGHT:
							case CROP_RECTANGLE_SPOT.ROTATE_BOTTOM_LEFT:
							case CROP_RECTANGLE_SPOT.ROTATE_BOTTOM_RIGHT:
								Cursor = Cursors.Hand;
								break;
						}
					}
					else
					{	// Handle actual manipulation
						// TODO!
					}
					break;
				}

				case MANIPULATION_STATE.CALIBRATION_TARGET:
				{	// Take care of calibration manipulation
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
					break;
				}

				case MANIPULATION_STATE.PICK_COLOR:
				{
					PointF	UV = Client2ImageUV( e.Location );
					Cursor = Cursors.Cross;
					m_ColorPickingDelegate( UV );
					break;
				}

				default:
					Cursor = Cursors.Default;
					break;
			}
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			base.OnMouseUp( e );

			// Stop any manipulation
			Capture = false;
			m_CropRectangleManipulationStarted = false;
		}
	}
}
