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
	public partial class WhiteImageReferencePanel : Panel
	{
		#region NESTED TYPES

//		public delegate void	ColorPickingUpdate( float2 _TopLeft, float2 _BottomRight );	// Sends the UV coordinates of the rectangle to average as a single color and create a swatch

		private enum	MANIPULATION_STATE
		{
			STOPPED,				// No manipulation currently taking place
			PICK_COLOR,				// User is picking a swatch color
		}

		#endregion

		#region FIELDS

		private Bitmap					m_Bitmap = null;
		private Bitmap					m_TextureBitmap = null;
		private Bitmap2.ColorProfile	m_sRGBProfile = new Bitmap2.ColorProfile( Bitmap2.ColorProfile.STANDARD_PROFILE.sRGB );

		private Bitmap2					m_WhiteReferenceImage = null;

		// === Manipulation ===
		private MANIPULATION_STATE		m_ManipulationState = MANIPULATION_STATE.STOPPED;

// 		// Color picking manipulation
// 		private ColorPickingUpdate		m_ColorPickingUpdateDelegate = null;
// 		private ColorPickingUpdate		m_ColorPickingEndDelegate = null;

		private MouseButtons			m_MouseButtonsDown = MouseButtons.None;
		private PointF					m_ButtonDownMousePosition;
		private PointF					m_MousePositionCurrent;

		#endregion

		#region PROPERTIES

		public unsafe Bitmap2	WhiteReferenceImage
		{
			get { return m_WhiteReferenceImage; }
			set {
				m_WhiteReferenceImage = value;
 
				if ( m_WhiteReferenceImage != null )
				{
					int		W = m_WhiteReferenceImage.Width;
					int		H = m_WhiteReferenceImage.Height;
					if ( m_TextureBitmap == null || m_TextureBitmap.Width != W || m_TextureBitmap.Height != H )
					{
						if ( m_TextureBitmap != null )
							m_TextureBitmap.Dispose();

						m_TextureBitmap = new Bitmap( W, H, PixelFormat.Format32bppArgb );
					}

					// Convert to RGB first
					float4[,]	ContentXYZ = m_WhiteReferenceImage.ContentXYZ;
					float4[,]	ContentRGB = new float4[ContentXYZ.GetLength(0),ContentXYZ.GetLength(1)];
					m_sRGBProfile.XYZ2RGB( ContentXYZ, ContentRGB );

					// Fill pixels
					BitmapData	LockedBitmap = m_TextureBitmap.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
					byte	R, G, B;
					for ( int Y=0; Y < H; Y++ )
					{
						byte*	pScanline = (byte*) LockedBitmap.Scan0 + LockedBitmap.Stride * Y;
						for ( int X=0; X < W; X++ )
						{
							R = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].x ) );
							G = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].y ) );
							B = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].z ) );
							*pScanline++ = B;
							*pScanline++ = G;
							*pScanline++ = R;
							*pScanline++ = 0xFF;
						}
					}
					m_TextureBitmap.UnlockBits( LockedBitmap );
				}
				else
				{
					if ( m_TextureBitmap != null )
						m_TextureBitmap.Dispose();
					m_TextureBitmap = null;
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

		public WhiteImageReferencePanel( IContainer container )
		{
			container.Add( this );
			InitializeComponent();
			OnSizeChanged( EventArgs.Empty );
		}

// 		/// <summary>
// 		/// Starts color picking with the mouse, provided delegate is called on mouse move with new informations
// 		/// </summary>
// 		/// <param name="_Notify"></param>
// 		public void				StartSwatchColorPicking( ColorPickingUpdate _Update, ColorPickingUpdate _PickingEnd )
// 		{
// 			m_ColorPickingUpdateDelegate = _Update;
// 			m_ColorPickingEndDelegate = _PickingEnd;
// 			ManipulationState = MANIPULATION_STATE.PICK_COLOR;
// 		}

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

		private float2	Client2ImageUV( PointF _Position )
		{
			RectangleF	ImageRect = ImageClientRect();
			return new float2( (_Position.X - ImageRect.X) / ImageRect.Width, (_Position.Y - ImageRect.Y) / ImageRect.Height );
		}

		private PointF	ImageUV2Client( float2 _Position )
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
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			m_MousePositionCurrent = e.Location;
			if ( m_MouseButtonsDown == MouseButtons.None )
				m_ButtonDownMousePosition = e.Location;

			switch ( m_ManipulationState )
			{
// 				case MANIPULATION_STATE.PICK_COLOR:
// 					Cursor = Cursors.Cross;
// 					float2	UV0 = Client2ImageUV( m_ButtonDownMousePosition );
// 					float2	UV1 = Client2ImageUV( e.Location );
// 					m_ColorPickingUpdateDelegate( UV0, UV1 );
// 					Invalidate();
// 					break;

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
// 			switch ( m_ManipulationState )
// 			{
// 				case MANIPULATION_STATE.PICK_COLOR:
// 					ManipulationState = MANIPULATION_STATE.STOPPED;
// 
// 					// Notify end
// 					float2	UV0 = Client2ImageUV( m_ButtonDownMousePosition );
// 					float2	UV1 = Client2ImageUV( e.Location );
// 					m_ColorPickingEndDelegate( UV0, UV1 );
// 					break;
//			}

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

// 			if ( m_WhiteReferenceImage == null )
// 				return;
// 
// 			// Show custom swatches' location
// 			RectangleF		R = ImageClientRect();
// 			for ( int SwatchIndex=0; SwatchIndex < m_CalibratedTexture.CustomSwatches.Length; SwatchIndex++ )
// 			{
// 				CalibratedTexture.CustomSwatch	S = m_CalibratedTexture.CustomSwatches[SwatchIndex];
// 
// 				PointF	TopLeft = new PointF( R.Left + S.Location.x * R.Width, R.Top + S.Location.y * R.Height );
// 				PointF	BottomRight = new PointF( R.Left + S.Location.z * R.Width, R.Top + S.Location.w * R.Height );
// 
// 				e.Graphics.DrawRectangle( Pens.Red, TopLeft.X, TopLeft.Y, 1+BottomRight.X-TopLeft.X, 1+BottomRight.Y-TopLeft.Y );
// 				e.Graphics.DrawString( SwatchIndex.ToString(), Font, Brushes.Red, 0.5f * (TopLeft.X + BottomRight.X - Font.Height), 0.5f * (TopLeft.Y + BottomRight.Y + Font.Height) );
// 			}
// 
// 			// Paint active tools
// 			switch ( ManipulationState )
// 			{
// 				case MANIPULATION_STATE.PICK_COLOR:
// 				{	// Paint a small red rectangle where the color should be averaged
// 					e.Graphics.DrawRectangle( Pens.Red, m_ButtonDownMousePosition.X, m_ButtonDownMousePosition.Y, m_MousePositionCurrent.X - m_ButtonDownMousePosition.X, m_MousePositionCurrent.Y - m_ButtonDownMousePosition.Y );
// 					break;
// 				}
// 			}
		}

		#endregion
	}
}
