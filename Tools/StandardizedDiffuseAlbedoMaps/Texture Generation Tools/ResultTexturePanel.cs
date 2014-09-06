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
	public partial class ResultTexturePanel : Panel
	{
		#region NESTED TYPES

		public delegate void	ColorPickingUpdate( ImageUtility.float2 _TopLeft, ImageUtility.float2 _BottomRight );	// Sends the UV coordinates of the rectangle to average as a single color and create a swatch

		private enum	MANIPULATION_STATE
		{
			STOPPED,				// No manipulation currently taking place
			PICK_COLOR,				// User is picking a swatch color
		}

		#endregion

		#region FIELDS

		private Bitmap					m_Bitmap = null;
		private Bitmap					m_TextureBitmap = null;
		private Bitmap					m_TextureOutOfRangeBitmap = null;
		private ImageUtility.ColorProfile	m_sRGBProfile = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB );

		private CalibratedTexture		m_CalibratedTexture = null;

		// === Manipulation ===
		private MANIPULATION_STATE		m_ManipulationState = MANIPULATION_STATE.STOPPED;

		// Color picking manipulation
		private ColorPickingUpdate		m_ColorPickingUpdateDelegate = null;
		private ColorPickingUpdate		m_ColorPickingEndDelegate = null;

		private MouseButtons			m_MouseButtonsDown = MouseButtons.None;
		private PointF					m_ButtonDownMousePosition;
		private PointF					m_MousePositionCurrent;

		#endregion

		#region PROPERTIES

		public unsafe CalibratedTexture	CalibratedTexture
		{
			get { return m_CalibratedTexture; }
			set {
				m_CalibratedTexture = value;
 
				if ( m_CalibratedTexture != null && m_CalibratedTexture.Texture != null )
				{
					int		W = m_CalibratedTexture.Texture.Width;
					int		H = m_CalibratedTexture.Texture.Height;
					if ( m_TextureBitmap == null || m_TextureBitmap.Width != W || m_TextureBitmap.Height != H )
					{
						if ( m_TextureBitmap != null )
						{
							m_TextureBitmap.Dispose();
							m_TextureOutOfRangeBitmap.Dispose();
						}

						m_TextureBitmap = new Bitmap( W, H, PixelFormat.Format32bppArgb );
						m_TextureOutOfRangeBitmap = new Bitmap( W, H, PixelFormat.Format32bppArgb );
					}

					// Convert to RGB first
					ImageUtility.float4[,]	ContentXYZ = m_CalibratedTexture.Texture.ContentXYZ;
					ImageUtility.float4[,]	ContentRGB = new ImageUtility.float4[ContentXYZ.GetLength(0),ContentXYZ.GetLength(1)];
					m_sRGBProfile.XYZ2RGB( ContentXYZ, ContentRGB );

					// Find significant areas to draw min/max location by isolating the 5% darkest/brightest pixels
					float		Min = m_CalibratedTexture.SwatchMin.xyY.z;
					float		Max = m_CalibratedTexture.SwatchMax.xyY.z;
					float		Range = Math.Max( 1e-6f, Max - Min );
					float		DarkestPixels = Min + 0.05f * Range;
					float		BrightestPixels = Max - 0.05f * Range;

					// Fill pixels
					BitmapData	LockedBitmap = m_TextureBitmap.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
					BitmapData	LockedBitmap2 = m_TextureOutOfRangeBitmap.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
					byte	R, G, B, A;
					for ( int Y=0; Y < H; Y++ )
					{
						byte*	pScanline = (byte*) LockedBitmap.Scan0 + LockedBitmap.Stride * Y;
						byte*	pScanline2 = (byte*) LockedBitmap2.Scan0 + LockedBitmap2.Stride * Y;
						float	V = (float) Y / H;
						for ( int X=0; X < W; X++ )
						{
							float	U = (float) X / W;

							R = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].x ) );
							G = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].y ) );
							B = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].z ) );
							*pScanline++ = B;
							*pScanline++ = G;
							*pScanline++ = R;
							*pScanline++ = 0xFF;

							float	Reflectance = ContentXYZ[X,Y].y;

							R = G = B = 0;
							A = 0x7F;

							if ( Reflectance < 0.02f || Reflectance > 0.99f )
								R = 0xFF;	// Show out of range pixels in red
							else if ( Reflectance < DarkestPixels )
							{	// Show min spot as a dark yellow
								R = 0x40;
								G = 0x40;
								B = 0;
								A = 0xFF;
							}
							else if ( Reflectance > BrightestPixels )
							{	// Show max spot as a bright yellow
								R = 0xFF;
								G = 0xFF;
								B = 0;
								A = 0xFF;
							}

							*pScanline2++ = B;
							*pScanline2++ = G;
							*pScanline2++ = R;
							*pScanline2++ = A;
						}
					}
					m_TextureOutOfRangeBitmap.UnlockBits( LockedBitmap2 );
					m_TextureBitmap.UnlockBits( LockedBitmap );
				}
				else
				{
					if ( m_TextureBitmap != null )
					{
						m_TextureBitmap.Dispose();
						m_TextureOutOfRangeBitmap.Dispose();
					}
					m_TextureBitmap = null;
					m_TextureOutOfRangeBitmap = null;
				}

				UpdateBitmap();
			}
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

		public RectangleF	ImageClientRectangle		{ get { return ImageClientRect(); } }

		#endregion

		#region METHODS

		public ResultTexturePanel( IContainer container )
		{
			container.Add( this );
			InitializeComponent();
			OnSizeChanged( EventArgs.Empty );
		}

		/// <summary>
		/// Starts color picking with the mouse, provided delegate is called on mouse move with new informations
		/// </summary>
		/// <param name="_Notify"></param>
		public void				StartSwatchColorPicking( ColorPickingUpdate _Update, ColorPickingUpdate _PickingEnd )
		{
			m_ColorPickingUpdateDelegate = _Update;
			m_ColorPickingEndDelegate = _PickingEnd;
			ManipulationState = MANIPULATION_STATE.PICK_COLOR;
		}

		private void		UpdateBitmap()
		{
			if ( m_Bitmap == null )
				return;

			int		W = m_Bitmap.Width;
			int		H = m_Bitmap.Height;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				using ( SolidBrush B = new SolidBrush( Color.Black ) )
					G.FillRectangle( B, 0, 0, W, H );

				if ( m_TextureBitmap != null )
				{
					// Draw thumbnail
					RectangleF	ClientRect = ImageClientRect();
					G.DrawImage( m_TextureBitmap, ClientRect, new RectangleF( 0, 0, m_TextureBitmap.Width, m_TextureBitmap.Height ), GraphicsUnit.Pixel );
				}
			}

			Invalidate();
		}

		private RectangleF		ImageClientRect()
		{
			int		SizeX = m_TextureBitmap.Width;
			int		SizeY = m_TextureBitmap.Height;

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

		private ImageUtility.float2	Client2ImageUV( PointF _Position )
		{
			RectangleF	ImageRect = ImageClientRect();
			return new ImageUtility.float2( (_Position.X - ImageRect.X) / ImageRect.Width, (_Position.Y - ImageRect.Y) / ImageRect.Height );
		}

		private PointF	ImageUV2Client( ImageUtility.float2 _Position )
		{
			RectangleF	ImageRect = ImageClientRect();
			return new PointF( _Position.x * ImageRect.Width + ImageRect.X, _Position.y * ImageRect.Height + ImageRect.Y );
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			if ( m_Bitmap != null )
				m_Bitmap.Dispose();
			m_Bitmap = null;

			if ( Width > 0 && Height > 0 )
				m_Bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			UpdateBitmap();

			base.OnSizeChanged( e );
		}

		protected override void OnMouseDown( MouseEventArgs e )
		{
			base.OnMouseDown( e );

			m_MouseButtonsDown |= e.Button;
			Capture = true;
			Invalidate();
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			m_MousePositionCurrent = e.Location;
			if ( m_MouseButtonsDown == MouseButtons.None )
				m_ButtonDownMousePosition = e.Location;

			switch ( m_ManipulationState )
			{
				case MANIPULATION_STATE.PICK_COLOR:
					Cursor = Cursors.Cross;
					ImageUtility.float2	UV0 = Client2ImageUV( m_ButtonDownMousePosition );
					ImageUtility.float2	UV1 = Client2ImageUV( e.Location );
					m_ColorPickingUpdateDelegate( UV0, UV1 );
					Invalidate();
					break;

				default:
					Cursor = Cursors.Default;
					break;
			}
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			base.OnMouseUp( e );

			m_MouseButtonsDown &= ~e.Button;

			// End manipulation
			switch ( m_ManipulationState )
			{
				case MANIPULATION_STATE.PICK_COLOR:
					ManipulationState = MANIPULATION_STATE.STOPPED;

					// Notify end
					ImageUtility.float2	UV0 = Client2ImageUV( m_ButtonDownMousePosition );
					ImageUtility.float2	UV1 = Client2ImageUV( e.Location );
					m_ColorPickingEndDelegate( UV0, UV1 );
					break;
			}

			Invalidate();
			Capture = false;
			Cursor = Cursors.Default;
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

			if ( m_CalibratedTexture == null )
				return;

			// Show custom swatches' location
			RectangleF		R = ImageClientRect();
			for ( int SwatchIndex=0; SwatchIndex < m_CalibratedTexture.CustomSwatches.Length; SwatchIndex++ )
			{
				CalibratedTexture.CustomSwatch	S = m_CalibratedTexture.CustomSwatches[SwatchIndex];

				PointF	TopLeft = new PointF( R.Left + S.Location.x * R.Width, R.Top + S.Location.y * R.Height );
				PointF	BottomRight = new PointF( R.Left + S.Location.z * R.Width, R.Top + S.Location.w * R.Height );

				e.Graphics.DrawRectangle( Pens.Red, TopLeft.X, TopLeft.Y, 1+BottomRight.X-TopLeft.X, 1+BottomRight.Y-TopLeft.Y );
				e.Graphics.DrawString( SwatchIndex.ToString(), Font, Brushes.Red, 0.5f * (TopLeft.X + BottomRight.X - Font.Height), 0.5f * (TopLeft.Y + BottomRight.Y - Font.Height) );
			}

			// Paint active tools
			switch ( ManipulationState )
			{
				case MANIPULATION_STATE.PICK_COLOR:
				{	// Paint a small red rectangle where the color should be averaged
					e.Graphics.DrawRectangle( Pens.Red, m_ButtonDownMousePosition.X, m_ButtonDownMousePosition.Y, m_MousePositionCurrent.X - m_ButtonDownMousePosition.X, m_MousePositionCurrent.Y - m_ButtonDownMousePosition.Y );
					break;
				}

				default:
					if ( m_TextureOutOfRangeBitmap != null && m_MouseButtonsDown != MouseButtons.None )
					{
						RectangleF	ClientRect = ImageClientRect();
						e.Graphics.DrawImage( m_TextureOutOfRangeBitmap, ClientRect, new RectangleF( 0, 0, m_TextureOutOfRangeBitmap.Width, m_TextureOutOfRangeBitmap.Height ), GraphicsUnit.Pixel );
					}
					break;
			}
		}

		#endregion
	}
}
