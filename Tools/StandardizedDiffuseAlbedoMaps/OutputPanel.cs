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

		public delegate void	CalibrationDone( ImageUtility.float2 _Center, float _Radius );			// Sends the center and radius of the circle to average as a single luminance
		public delegate void	ColorPickingUpdate( ImageUtility.float2 _TopLeft, ImageUtility.float2 _BottomRight );	// Sends the UV coordinates of the rectangle to average as a single color

		private enum	MANIPULATION_STATE
		{
			STOPPED,				// No manipulation currently taking place
			CALIBRATION_TARGET,		// User is selecting the calibration target
			CROP_RECTANGLE,			// User is modifying the crop rectangle
		}

		private enum	CALIBRATION_STAGE
		{
			PICK_CENTER,
			SET_RADIUS,
		}

		private enum	CROP_RECTANGLE_SPOT
		{
			NONE,

			CENTER,

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
		private ImageUtility.float4[,]			m_Image = null;

		private MANIPULATION_STATE	m_ManipulationState = MANIPULATION_STATE.STOPPED;

		// Calibration luminance target manipulation
		private Pen					m_PenTarget = new Pen( Color.Gold, 2.0f );

		private CalibrationDone		m_CalibrationDelegate = null;
		private CALIBRATION_STAGE	m_CalibrationStage;
		private ImageUtility.float2				m_CalibrationCenter = new ImageUtility.float2();
		private float				m_CalibrationRadius = 0.0f;

		// Crop rectangle manipulation
		private Pen					m_PenCropRectangle = new Pen( Color.Red, 1.0f );
		private SolidBrush			m_BrushCroppedZone = new SolidBrush( Color.FromArgb( 128, Color.White ) );

		private bool				m_CropRectangleIsDefault = true;
		private ImageUtility.float2				m_CropRectangleCenter = new ImageUtility.float2( 0.5f, 0.5f );
		private ImageUtility.float2				m_CropRectangleHalfSize = new ImageUtility.float2( 0.5f, 0.5f );
		private float				m_CropRectangleRotation = 0.0f;
		private ImageUtility.float2				m_CropRectangleAxisX;	// Updated by UpdateCropRectangleVertices
		private ImageUtility.float2				m_CropRectangleAxisY;	// Updated by UpdateCropRectangleVertices

		private bool				m_CropRectangleManipulationStarted = false;
		private CROP_RECTANGLE_SPOT	m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.NONE;
		private ImageUtility.float2				m_ButtonDownCropRectangleCenter;
		private ImageUtility.float2				m_ButtonDownCropRectangleHalfSize;
		private float				m_ButtonDownCropRectangleRotation;
		private ImageUtility.float2				m_ButtonDownCropRectangleAxisX;
		private ImageUtility.float2				m_ButtonDownCropRectangleAxisY;
			// Client space data
		private PointF				m_ButtonDownClientCropRectangleCenter;
		private PointF[]			m_ButtonDownClientCropRectangleVertices = new PointF[4];	// Top-left, top-right, bottom-left, bottom-right


		private MouseButtons		m_MouseButtonsDown = MouseButtons.None;
		private PointF				m_ButtonDownMousePosition;
		private PointF				m_MousePositionCurrent;

		#endregion

		#region PROPERTIES

		public ImageUtility.float4[,]	Image
		{
			get { return m_Image; }
			set {
				bool	FirstTime = m_Image == null;
				m_Image = value;
				if ( FirstTime )
					ResetCropRectangle();

				UpdateBitmap();
			}
		}

		public bool			CropRectangleEnabled
		{
			get { return m_ManipulationState == MANIPULATION_STATE.CROP_RECTANGLE; }
			set { ManipulationState = value ? MANIPULATION_STATE.CROP_RECTANGLE : MANIPULATION_STATE.STOPPED; }
		}

		public RectangleF	ImageClientRectangle		{ get { return ImageClientRect(); } }

		private float				ImageWidth			{ get { return (float) m_Image.GetLength(0); } }
		private float				ImageHeight			{ get { return (float) m_Image.GetLength(1); } }
		private float				ImageAspectRatio	{ get { return ImageWidth / ImageHeight; } }

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
		public bool			IsDefaultCropRectangle	{ get { return m_CropRectangleIsDefault; } }
		public ImageUtility.float2		CropRectangeCenter		{ get { return m_CropRectangleCenter; } }
		public ImageUtility.float2		CropRectangeHalfSize	{ get { return m_CropRectangleHalfSize; } }
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
			m_CalibrationStage = CALIBRATION_STAGE.PICK_CENTER;
		}

		/// <summary>
		/// Resets the crop rectangle to the entire image
		/// </summary>
		public void				ResetCropRectangle()
		{
			m_CropRectangleIsDefault = true;
			if ( m_Image == null )
				return;

			m_CropRectangleCenter = new ImageUtility.float2( 0.5f, 0.5f );
			m_CropRectangleHalfSize = new ImageUtility.float2( 0.5f * ImageAspectRatio, 0.5f );
			m_CropRectangleRotation = 0.0f;
			Invalidate();
		}

		/// <summary>
		/// Sets the crop rectangle to a specific value
		/// </summary>
		public void				SetCropRectangle( ImageUtility.float2 _Center, ImageUtility.float2 _HalfSize, float _Rotation )
		{
			m_CropRectangleIsDefault = false;
			m_CropRectangleCenter = _Center;
			m_CropRectangleHalfSize = _HalfSize;
			m_CropRectangleRotation = _Rotation;
			Invalidate();
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
							ImageUtility.float4	RGB = m_Image[(int) (SizeX*(X-ImageRect.X)/ImageRect.Width), (int) (SizeY*(Y-ImageRect.Y)/ImageRect.Height)];
							R = (byte) Math.Max( 0, Math.Min( 255, 255.0f * RGB.x ) );
							G = (byte) Math.Max( 0, Math.Min( 255, 255.0f * RGB.y ) );
							B = (byte) Math.Max( 0, Math.Min( 255, 255.0f * RGB.z ) );
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

		/// <summary>
		/// This is a simple UV as used in 3D engines: (0,0) is top left corner of the image, (1,1) is bottom right corner
		/// </summary>
		/// <param name="_Position"></param>
		/// <returns></returns>
		private ImageUtility.float2	Client2ImageUV_NoSquareAspectRatio( PointF _Position )
		{
			RectangleF	ImageRect = ImageClientRect();
			return new ImageUtility.float2( (_Position.X - ImageRect.X) / ImageRect.Width, (_Position.Y - ImageRect.Y) / ImageRect.Height );
		}
	
		private PointF	ImageUV2Client_NoSquareAspectRatio( ImageUtility.float2 _Position )
		{
			RectangleF	ImageRect = ImageClientRect();
			return new PointF( ImageRect.X + _Position.x * ImageRect.Width, ImageRect.Y + _Position.y * ImageRect.Height );
		}
	
		/// <summary>
		/// This is the UVs used for crop rectangle computation
		/// The V is in [0,1] mapping from [0,ImageHeight] as usual but U takes into account aspect ratio
		///	 to ensure pixels are always squares so for example if you image has an aspect ratio of 2 (Width = 2 * Height)
		///	 then U will go from [-0.5,1.5] assuming 0.5 is always the center of the image.
		///	U span is then 1.5+0.5 = 2, which is twice the span of V [0,1] and we keep the correct aspect ratio
		/// </summary>
		/// <param name="_Position"></param>
		/// <returns></returns>
		private ImageUtility.float2	Client2ImageUV( PointF _Position )
		{
			RectangleF	ImageRect = ImageClientRect();
//			return new ImageUtility.float2( (_Position.X - ImageRect.X) / ImageRect.Width, (_Position.Y - ImageRect.Y) / ImageRect.Height );
//			return new ImageUtility.float2( (_Position.X - ImageRect.X) / ImageRect.Height, (_Position.Y - ImageRect.Y) / ImageRect.Height );
			return new ImageUtility.float2( (_Position.X - 0.5f * (Width - ImageRect.Height)) / ImageRect.Height, (_Position.Y - ImageRect.Y) / ImageRect.Height );
		}

		private PointF	ImageUV2Client( ImageUtility.float2 _Position )
		{
			RectangleF	ImageRect = ImageClientRect();
//			return new PointF( ImageRect.X + _Position.x * ImageRect.Width, ImageRect.Y + _Position.y * ImageRect.Height );
//			return new PointF( ImageRect.X + _Position.x * ImageRect.Height, ImageRect.Y + _Position.y * ImageRect.Height );
			return new PointF( 0.5f * (Width - ImageRect.Height) + _Position.x * ImageRect.Height, ImageRect.Y + _Position.y * ImageRect.Height );
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

		private PointF		m_ClientCropRectangleCenter;
		private PointF[]	m_ClientCropRectangleVertices = new PointF[4];	// Top-left, top-right, bottom-left, bottom-right
		private void		UpdateCropRectangleVertices()
		{
			m_CropRectangleAxisX = new ImageUtility.float2( (float) Math.Cos( m_CropRectangleRotation ), -(float) Math.Sin( m_CropRectangleRotation ) );
			m_CropRectangleAxisY = new ImageUtility.float2( -(float) Math.Sin( m_CropRectangleRotation ), -(float) Math.Cos( m_CropRectangleRotation ) );

			RectangleF	ImageRect = ImageClientRect();

			m_ClientCropRectangleCenter = ImageUV2Client( m_CropRectangleCenter );

			ImageUtility.float2[]	Vertices = new ImageUtility.float2[4] {
				m_CropRectangleCenter - m_CropRectangleHalfSize.x * m_CropRectangleAxisX + m_CropRectangleHalfSize.y * m_CropRectangleAxisY,
				m_CropRectangleCenter + m_CropRectangleHalfSize.x * m_CropRectangleAxisX + m_CropRectangleHalfSize.y * m_CropRectangleAxisY,
				m_CropRectangleCenter - m_CropRectangleHalfSize.x * m_CropRectangleAxisX - m_CropRectangleHalfSize.y * m_CropRectangleAxisY,
				m_CropRectangleCenter + m_CropRectangleHalfSize.x * m_CropRectangleAxisX - m_CropRectangleHalfSize.y * m_CropRectangleAxisY,
			};

			for ( int i=0; i < 4; i++ )
				m_ClientCropRectangleVertices[i] = ImageUV2Client( Vertices[i] );
		}

		protected override void OnMouseDown( MouseEventArgs e )
		{
			base.OnMouseDown( e );

			m_MouseButtonsDown |= e.Button;
			Capture = true;

			switch ( m_ManipulationState )
			{
				case MANIPULATION_STATE.CROP_RECTANGLE:
				{	// Take care of crop rectangle manipulation
					if ( m_CropRectangleManipulatedSpot == CROP_RECTANGLE_SPOT.NONE )
						return;	// Nothing to manipulate...

					m_CropRectangleManipulationStarted = true;
					break;
				}

				case MANIPULATION_STATE.CALIBRATION_TARGET:
				{	// Take care of calibration manipulation
					if ( m_CalibrationStage == CALIBRATION_STAGE.PICK_CENTER )
						m_CalibrationStage = CALIBRATION_STAGE.SET_RADIUS;
					else if ( m_CalibrationStage == CALIBRATION_STAGE.SET_RADIUS )
					{	// We're done! Notify!
						m_CalibrationDelegate( m_CalibrationCenter, m_CalibrationRadius );
						ManipulationState = MANIPULATION_STATE.STOPPED;		// End manipulation
					}
					break;
				}
			}
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			m_MousePositionCurrent = e.Location;
			if ( m_MouseButtonsDown == MouseButtons.None )
			{
				m_ButtonDownMousePosition = e.Location;
				m_ButtonDownCropRectangleCenter = m_CropRectangleCenter;
				m_ButtonDownCropRectangleHalfSize = m_CropRectangleHalfSize;
				m_ButtonDownCropRectangleRotation = m_CropRectangleRotation;
				m_ButtonDownCropRectangleAxisX = m_CropRectangleAxisX;
				m_ButtonDownCropRectangleAxisY = m_CropRectangleAxisY;

				m_ButtonDownClientCropRectangleCenter = m_ClientCropRectangleCenter;
				m_ButtonDownClientCropRectangleVertices = new PointF[4] {
					m_ClientCropRectangleVertices[0],
					m_ClientCropRectangleVertices[1],
					m_ClientCropRectangleVertices[2],
					m_ClientCropRectangleVertices[3],
				};
			}

			switch ( m_ManipulationState )
			{
				case MANIPULATION_STATE.CROP_RECTANGLE:
				{	// Take care of crop rectangle manipulation
					if ( !m_CropRectangleManipulationStarted )
					{	// Update the cursor based on the hovered manipulation spot
						m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.NONE;

						const float	TOLERANCE = 8.0f;

						PointF	P = e.Location;

						PointF	TopLeftCorner2UV = new PointF( P.X - m_ClientCropRectangleVertices[0].X, P.Y - m_ClientCropRectangleVertices[0].Y );
						PointF	BottomRightCorner2UV = new PointF( P.X - m_ClientCropRectangleVertices[3].X, P.Y - m_ClientCropRectangleVertices[3].Y );

						float	Distance2Left = -TopLeftCorner2UV.X * m_CropRectangleAxisX.x - TopLeftCorner2UV.Y * m_CropRectangleAxisX.y;
						float	Distance2Top = TopLeftCorner2UV.X * m_CropRectangleAxisY.x + TopLeftCorner2UV.Y * m_CropRectangleAxisY.y;
						float	Distance2Right = BottomRightCorner2UV.X * m_CropRectangleAxisX.x + BottomRightCorner2UV.Y * m_CropRectangleAxisX.y;
						float	Distance2Bottom = -BottomRightCorner2UV.X * m_CropRectangleAxisY.x - BottomRightCorner2UV.Y * m_CropRectangleAxisY.y;

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
						else if ( Distance2Left < 0.0f && Distance2Right < 0.0f && Distance2Bottom < 0.0f && Distance2Top < 0.0f )
							m_CropRectangleManipulatedSpot = CROP_RECTANGLE_SPOT.CENTER;

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
							case CROP_RECTANGLE_SPOT.CENTER:
								Cursor = Cursors.Hand;
								break;
						}
					}
					else
					{	// Handle actual manipulation
						ImageUtility.float2		CurClientPos = new ImageUtility.float2( e.X, e.Y );
						ImageUtility.float2		OldClientPos = new ImageUtility.float2( m_ButtonDownMousePosition.X, m_ButtonDownMousePosition.Y );
						ImageUtility.float2		OldClientCenter = new ImageUtility.float2( m_ButtonDownClientCropRectangleCenter.X, m_ButtonDownClientCropRectangleCenter.Y );

						ImageUtility.float2		OldCenter = m_ButtonDownCropRectangleCenter;
						ImageUtility.float2		OldHalfSize = m_ButtonDownCropRectangleHalfSize;
						ImageUtility.float2		OldAxisX = m_ButtonDownCropRectangleAxisX;
						ImageUtility.float2		OldAxisY = m_ButtonDownCropRectangleAxisY;

						ImageUtility.float2[]	OldVertices = new ImageUtility.float2[4] {
							OldCenter - OldHalfSize.x * OldAxisX + OldHalfSize.y * OldAxisY,
							OldCenter + OldHalfSize.x * OldAxisX + OldHalfSize.y * OldAxisY,
							OldCenter - OldHalfSize.x * OldAxisX - OldHalfSize.y * OldAxisY,
							OldCenter + OldHalfSize.x * OldAxisX - OldHalfSize.y * OldAxisY,
						};

						ImageUtility.float2		CurCenter = OldCenter;
						ImageUtility.float2		CurHalfSize = OldHalfSize;
						ImageUtility.float2		CurAxisX = OldAxisX;
						ImageUtility.float2		CurAxisY = OldAxisY;

						RectangleF	ImageRect = ImageClientRect();	// The image's rectangle in client space

						switch ( m_CropRectangleManipulatedSpot )
						{
							case CROP_RECTANGLE_SPOT.LEFT:
							case CROP_RECTANGLE_SPOT.RIGHT:
							{
								ImageUtility.float2	Center2OldPos = OldClientPos - OldClientCenter;
								float	OldDistance2Edge = Center2OldPos.Dot( OldAxisX );
								ImageUtility.float2	Center2CurPos = CurClientPos - OldClientCenter;
								float	CurDistance2Edge = Center2CurPos.Dot( OldAxisX );

								float	Delta = CurDistance2Edge - OldDistance2Edge;	// This is the amount (in client space) we need to move the left/right border
								float	DeltaUV = Delta / ImageRect.Height;				// This is the same amount in UV space
								DeltaUV *= 0.5f;	// We're dealing with halves all along

								// Increase width of that amount
								CurHalfSize.x = OldHalfSize.x + (m_CropRectangleManipulatedSpot == CROP_RECTANGLE_SPOT.LEFT ? -1 : 1) * DeltaUV;

								// Move center along the X axis
								if ( m_CropRectangleManipulatedSpot == CROP_RECTANGLE_SPOT.LEFT )
								{	// Keep right fixed, move to the left
									ImageUtility.float2	Right = OldCenter + OldHalfSize.x * OldAxisX;
									ImageUtility.float2	Left = Right - 2.0f * CurHalfSize.x * OldAxisX;
									CurCenter = 0.5f * (Right + Left);
								}
								else
								{	// Keep left fixed, move to the right
									ImageUtility.float2	Left = OldCenter - OldHalfSize.x * OldAxisX;
									ImageUtility.float2	Right = Left + 2.0f * CurHalfSize.x * OldAxisX;
									CurCenter = 0.5f * (Right + Left);
								}
								break;
							}

							case CROP_RECTANGLE_SPOT.TOP:
							case CROP_RECTANGLE_SPOT.BOTTOM:
							{
								ImageUtility.float2	Center2OldPos = OldClientPos - OldClientCenter;
								float	OldDistance2Edge = Center2OldPos.Dot( OldAxisY );
								ImageUtility.float2	Center2CurPos = CurClientPos - OldClientCenter;
								float	CurDistance2Edge = Center2CurPos.Dot( OldAxisY );

								float	Delta = CurDistance2Edge - OldDistance2Edge;	// This is the amount (in client space) we need to move the left/right border
								float	DeltaUV = Delta / ImageRect.Height;				// This is the same amount in UV space
								DeltaUV *= 0.5f;	// We're dealing with halves all along

								// Increase height of that amount
								CurHalfSize.y = OldHalfSize.y + (m_CropRectangleManipulatedSpot == CROP_RECTANGLE_SPOT.TOP ? 1 : -1) * DeltaUV;

								// Move center along the X axis
								if ( m_CropRectangleManipulatedSpot == CROP_RECTANGLE_SPOT.TOP )
								{	// Keep bottom fixed, move up
									ImageUtility.float2	Bottom = OldCenter - OldHalfSize.y * OldAxisY;
									ImageUtility.float2	Top = Bottom + 2.0f * CurHalfSize.y * OldAxisY;
									CurCenter = 0.5f * (Bottom + Top);
								}
								else
								{	// Keep top fixed, move down
									ImageUtility.float2	Top = OldCenter + OldHalfSize.y * OldAxisY;
									ImageUtility.float2	Bottom = Top - 2.0f * CurHalfSize.y * OldAxisY;
									CurCenter = 0.5f * (Bottom + Top);
								}
								break;
							}

							case CROP_RECTANGLE_SPOT.CORNER_TOP_LEFT:
							case CROP_RECTANGLE_SPOT.CORNER_TOP_RIGHT:
							case CROP_RECTANGLE_SPOT.CORNER_BOTTOM_LEFT:
							case CROP_RECTANGLE_SPOT.CORNER_BOTTOM_RIGHT:
							{
								ImageUtility.float2	Delta = CurClientPos - OldClientPos;
								ImageUtility.float2	DeltaUV = (1.0f / ImageRect.Height) * Delta;

								ImageUtility.float2	Corner = new ImageUtility.float2(), OppositeCorner = new ImageUtility.float2();
								switch ( m_CropRectangleManipulatedSpot )
								{
									case CROP_RECTANGLE_SPOT.CORNER_TOP_LEFT: Corner = OldVertices[0]; OppositeCorner = OldVertices[3]; break;		// Keep bottom right fixed
									case CROP_RECTANGLE_SPOT.CORNER_TOP_RIGHT: Corner = OldVertices[1]; OppositeCorner = OldVertices[2]; break;		// Keep bottom left fixed
									case CROP_RECTANGLE_SPOT.CORNER_BOTTOM_LEFT: Corner = OldVertices[2]; OppositeCorner = OldVertices[1]; break;		// Keep top right fixed
									case CROP_RECTANGLE_SPOT.CORNER_BOTTOM_RIGHT: Corner = OldVertices[3]; OppositeCorner = OldVertices[0]; break;	// Keep top left fixed
								}

								// Move corner
								Corner += DeltaUV;

								// Compute new center
								CurCenter = new ImageUtility.float2( 0.5f * (Corner.x + OppositeCorner.x), 0.5f * (Corner.y + OppositeCorner.y) );

								// Compute new size
								CurHalfSize = new ImageUtility.float2( Math.Abs( (Corner - CurCenter).Dot( OldAxisX ) ), Math.Abs( (Corner - CurCenter).Dot( OldAxisY ) ) );

								break;
							}

							case CROP_RECTANGLE_SPOT.ROTATE_TOP_LEFT:
							case CROP_RECTANGLE_SPOT.ROTATE_TOP_RIGHT:
							case CROP_RECTANGLE_SPOT.ROTATE_BOTTOM_LEFT:
							case CROP_RECTANGLE_SPOT.ROTATE_BOTTOM_RIGHT:
							{
								ImageUtility.float2	Center2OldPos = OldClientPos - OldClientCenter;
								ImageUtility.float2	Center2CurPos = CurClientPos - OldClientCenter;

								float	OldAngle = (float) Math.Atan2( -Center2OldPos.y, Center2OldPos.x );
								float	CurAngle = (float) Math.Atan2( -Center2CurPos.y, Center2CurPos.x );

								m_CropRectangleRotation = m_ButtonDownCropRectangleRotation + CurAngle - OldAngle;

								CurAxisX = new ImageUtility.float2( (float) Math.Cos( m_CropRectangleRotation ), -(float) Math.Sin( m_CropRectangleRotation ) );
								CurAxisY = new ImageUtility.float2( -(float) Math.Sin( m_CropRectangleRotation ), -(float) Math.Cos( m_CropRectangleRotation ) );
								break;
							}

							case CROP_RECTANGLE_SPOT.CENTER:
							{
								ImageUtility.float2	Delta = CurClientPos - OldClientPos;
								ImageUtility.float2	DeltaUV = (1.0f / ImageRect.Height) * Delta;

								CurCenter = OldCenter + DeltaUV;
								break;
							}
						}

						CurHalfSize.x = Math.Max( 1e-3f, CurHalfSize.x );
						CurHalfSize.y = Math.Max( 1e-3f, CurHalfSize.y );

						// Constrain vertices to the image
						ImageUtility.float2[]	Vertices = BuildClipVertices( CurCenter, CurHalfSize, CurAxisX, CurAxisY );

						float	MinX = 0.5f * (1.0f - ImageAspectRatio);
						float	MaxX = 0.5f * (1.0f + ImageAspectRatio);

						for ( int i=0; i < 4; i++ )
						{
							bool	Rebuild = false;
							if ( Vertices[i].x < MinX )
							{
								Vertices[i].x = MinX;
								Rebuild = true;
							}
							if ( Vertices[i].x > MaxX )
							{
								Vertices[i].x = MaxX;
								Rebuild = true;
							}
							if ( Vertices[i].y < 0.0f )
							{
								Vertices[i].y = 0.0f;
								Rebuild = true;
							}
							if ( Vertices[i].y > 1.0f )
							{
								Vertices[i].y = 1.0f;
								Rebuild = true;
							}
							if ( !Rebuild )
								continue;

							ImageUtility.float2	OppositeVertex = Vertices[3-i];	// This one is fixed

							// Recompute center & half size
							CurCenter = 0.5f * (OppositeVertex + Vertices[i]);
							ImageUtility.float2	Delta = Vertices[i] - OppositeVertex;
							CurHalfSize = 0.5f * new ImageUtility.float2( Math.Abs( Delta.Dot( CurAxisX ) ), Math.Abs( Delta.Dot( CurAxisY ) ) );

							// Rebuild new vertices
							Vertices = BuildClipVertices( CurCenter, CurHalfSize, CurAxisX, CurAxisY );
						}

						m_CropRectangleCenter = CurCenter;
						m_CropRectangleHalfSize = CurHalfSize;

						// The crop rectangle has changed!
						m_CropRectangleIsDefault = false;

						// Repaint to update the crop rectangle
						Invalidate();
					}
					break;
				}

				case MANIPULATION_STATE.CALIBRATION_TARGET:
				{	// Take care of calibration manipulation
					Cursor = Cursors.Cross;
					switch ( m_CalibrationStage )
					{
						case CALIBRATION_STAGE.PICK_CENTER:
							m_CalibrationCenter = Client2ImageUV_NoSquareAspectRatio( e.Location );
							Invalidate();
							break;

						case CALIBRATION_STAGE.SET_RADIUS:
							{
								ImageUtility.float2	Temp = Client2ImageUV_NoSquareAspectRatio( e.Location );
								m_CalibrationRadius = (float) Math.Sqrt( (Temp.x - m_CalibrationCenter.x)*(Temp.x - m_CalibrationCenter.x) + (Temp.y - m_CalibrationCenter.y)*(Temp.y - m_CalibrationCenter.y) );
								Invalidate();
							}
							break;
					}
					break;
				}

				default:
					Cursor = Cursors.Default;
					break;
			}
		}

		protected ImageUtility.float2[]	BuildClipVertices( ImageUtility.float2 _Center, ImageUtility.float2 _HalfSize, ImageUtility.float2 _AxisX, ImageUtility.float2 _AxisY )
		{
			return new ImageUtility.float2[4] {
							_Center - _HalfSize.x * _AxisX + _HalfSize.y * _AxisY,
							_Center + _HalfSize.x * _AxisX + _HalfSize.y * _AxisY,
							_Center - _HalfSize.x * _AxisX - _HalfSize.y * _AxisY,
							_Center + _HalfSize.x * _AxisX - _HalfSize.y * _AxisY,
						};
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			base.OnMouseUp( e );

			m_MouseButtonsDown &= ~e.Button;

			// End manipulation
			switch ( m_ManipulationState )
			{
				case MANIPULATION_STATE.CROP_RECTANGLE:
					m_CropRectangleManipulationStarted = false;
					break;
			}

			Capture = false;
			Cursor = Cursors.Default;
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
					const float	F = 1000.0f;
//					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] { m_ClientCropRectangleVertices[0], m_ClientCropRectangleVertices[1], m_ClientCropRectangleVertices[3], m_ClientCropRectangleVertices[2] } );
					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_ClientCropRectangleVertices[0],
						new PointF( m_ClientCropRectangleVertices[0].X + F*(+0*m_CropRectangleAxisX.x+1*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[0].Y + F*(+0*m_CropRectangleAxisX.y+1*m_CropRectangleAxisY.y) ),
						new PointF( m_ClientCropRectangleVertices[0].X + F*(-1*m_CropRectangleAxisX.x+1*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[0].Y + F*(-1*m_CropRectangleAxisX.y+1*m_CropRectangleAxisY.y) ),
						new PointF( m_ClientCropRectangleVertices[0].X + F*(-1*m_CropRectangleAxisX.x+0*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[0].Y + F*(-1*m_CropRectangleAxisX.y+0*m_CropRectangleAxisY.y) ),
					} );
					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_ClientCropRectangleVertices[1],
						new PointF( m_ClientCropRectangleVertices[1].X + F*(+1*m_CropRectangleAxisX.x+0*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[1].Y + F*(+1*m_CropRectangleAxisX.y+0*m_CropRectangleAxisY.y) ),
						new PointF( m_ClientCropRectangleVertices[1].X + F*(+1*m_CropRectangleAxisX.x+1*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[1].Y + F*(+1*m_CropRectangleAxisX.y+1*m_CropRectangleAxisY.y) ),
						new PointF( m_ClientCropRectangleVertices[1].X + F*(+0*m_CropRectangleAxisX.x+1*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[1].Y + F*(+0*m_CropRectangleAxisX.y+1*m_CropRectangleAxisY.y) ),
					} );
					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_ClientCropRectangleVertices[2],
						new PointF( m_ClientCropRectangleVertices[2].X + F*(-1*m_CropRectangleAxisX.x+0*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[2].Y + F*(-1*m_CropRectangleAxisX.y+0*m_CropRectangleAxisY.y) ),
						new PointF( m_ClientCropRectangleVertices[2].X + F*(-1*m_CropRectangleAxisX.x-1*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[2].Y + F*(-1*m_CropRectangleAxisX.y-1*m_CropRectangleAxisY.y) ),
						new PointF( m_ClientCropRectangleVertices[2].X + F*(+0*m_CropRectangleAxisX.x-1*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[2].Y + F*(+0*m_CropRectangleAxisX.y-1*m_CropRectangleAxisY.y) ),
					} );
					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_ClientCropRectangleVertices[3],
						new PointF( m_ClientCropRectangleVertices[3].X + F*(+0*m_CropRectangleAxisX.x-1*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[3].Y + F*(+0*m_CropRectangleAxisX.y-1*m_CropRectangleAxisY.y) ),
						new PointF( m_ClientCropRectangleVertices[3].X + F*(+1*m_CropRectangleAxisX.x-1*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[3].Y + F*(+1*m_CropRectangleAxisX.y-1*m_CropRectangleAxisY.y) ),
						new PointF( m_ClientCropRectangleVertices[3].X + F*(+1*m_CropRectangleAxisX.x+0*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[3].Y + F*(+1*m_CropRectangleAxisX.y+0*m_CropRectangleAxisY.y) ),
					} );

					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_ClientCropRectangleVertices[1],
						m_ClientCropRectangleVertices[0],
						new PointF( m_ClientCropRectangleVertices[0].X + F*(+0*m_CropRectangleAxisX.x+1*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[0].Y + F*(+0*m_CropRectangleAxisX.y+1*m_CropRectangleAxisY.y) ),
						new PointF( m_ClientCropRectangleVertices[1].X + F*(+0*m_CropRectangleAxisX.x+1*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[1].Y + F*(+0*m_CropRectangleAxisX.y+1*m_CropRectangleAxisY.y) ),
					} );
					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_ClientCropRectangleVertices[3],
						m_ClientCropRectangleVertices[2],
						new PointF( m_ClientCropRectangleVertices[2].X + F*(+0*m_CropRectangleAxisX.x-1*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[2].Y + F*(+0*m_CropRectangleAxisX.y-1*m_CropRectangleAxisY.y) ),
						new PointF( m_ClientCropRectangleVertices[3].X + F*(+0*m_CropRectangleAxisX.x-1*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[3].Y + F*(+0*m_CropRectangleAxisX.y-1*m_CropRectangleAxisY.y) ),
					} );
					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_ClientCropRectangleVertices[2],
						m_ClientCropRectangleVertices[0],
						new PointF( m_ClientCropRectangleVertices[0].X + F*(-1*m_CropRectangleAxisX.x+0*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[0].Y + F*(-1*m_CropRectangleAxisX.y+0*m_CropRectangleAxisY.y) ),
						new PointF( m_ClientCropRectangleVertices[2].X + F*(-1*m_CropRectangleAxisX.x+0*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[2].Y + F*(-1*m_CropRectangleAxisX.y+0*m_CropRectangleAxisY.y) ),
					} );
					e.Graphics.FillPolygon( m_BrushCroppedZone, new PointF[] {
						m_ClientCropRectangleVertices[1],
						m_ClientCropRectangleVertices[3],
						new PointF( m_ClientCropRectangleVertices[3].X + F*(+1*m_CropRectangleAxisX.x+0*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[3].Y + F*(+1*m_CropRectangleAxisX.y+0*m_CropRectangleAxisY.y) ),
						new PointF( m_ClientCropRectangleVertices[1].X + F*(+1*m_CropRectangleAxisX.x+0*m_CropRectangleAxisY.x), m_ClientCropRectangleVertices[1].Y + F*(+1*m_CropRectangleAxisX.y+0*m_CropRectangleAxisY.y) ),
					} );

					// Draw actual crop rectange
					e.Graphics.DrawPolygon( m_PenCropRectangle, new PointF[] { m_ClientCropRectangleVertices[0], m_ClientCropRectangleVertices[1], m_ClientCropRectangleVertices[3], m_ClientCropRectangleVertices[2] } );
					break;
				}

				case MANIPULATION_STATE.CALIBRATION_TARGET:
				{	// Paint the calibration target
					PointF	Center = ImageUV2Client_NoSquareAspectRatio( m_CalibrationCenter );
					e.Graphics.DrawLine( m_PenTarget, Center.X, Center.Y-20, Center.X, Center.Y+20 );
					e.Graphics.DrawLine( m_PenTarget, Center.X-20, Center.Y, Center.X+20, Center.Y );

					PointF	Temp = ImageUV2Client_NoSquareAspectRatio( new ImageUtility.float2( m_CalibrationCenter.x + m_CalibrationRadius, m_CalibrationCenter.y ) );
					float	ClientRadius = Temp.X - Center.X;
					e.Graphics.DrawEllipse( m_PenTarget, Center.X-ClientRadius, Center.Y-ClientRadius, 2.0f*ClientRadius, 2.0f*ClientRadius );
					break;
				}
			}
		}
	}
}
