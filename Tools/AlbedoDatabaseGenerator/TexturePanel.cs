using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace AlbedoDatabaseGenerator
{
	public partial class TexturePanel : Panel
	{
		#region FIELDS

		private WMath.Vector4D[]	m_CustomSwatches = new WMath.Vector4D[9];
		private Bitmap				m_SourceImage = null;

		#endregion

		#region PROPERTIES

		public WMath.Vector4D[]	CustomSwatches
		{
			get { return m_CustomSwatches; }
		}

		public unsafe Bitmap	SourceImage
		{
			get { return m_SourceImage; }
			set {
				m_SourceImage = value;
				Invalidate();
			}
		}

		public RectangleF	ImageClientRectangle
		{
			get
			{
				if ( m_SourceImage == null )
					return RectangleF.Empty;

				int		SizeX = m_SourceImage.Width;
				int		SizeY = m_SourceImage.Height;

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
		}

		#endregion

		#region METHODS

		public TexturePanel( IContainer container )
		{
			container.Add( this );
			InitializeComponent();
			OnSizeChanged( EventArgs.Empty );
		}

		private PointF	Client2ImageUV( PointF _Position )
		{
			RectangleF	ImageRect = ImageClientRectangle;
			return new PointF( (_Position.X - ImageRect.X) / ImageRect.Width, (_Position.Y - ImageRect.Y) / ImageRect.Height );
		}

		private PointF	ImageUV2Client( PointF _Position )
		{
			RectangleF	ImageRect = ImageClientRectangle;
			return new PointF( _Position.X * ImageRect.Width + ImageRect.X, _Position.Y * ImageRect.Height + ImageRect.Y );
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			Invalidate();
			base.OnSizeChanged( e );
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			e.Graphics.FillRectangle( Brushes.Black, 0, 0, Width, Height );
			if ( m_SourceImage != null )
				e.Graphics.DrawImage( m_SourceImage, ImageClientRectangle, new RectangleF( 0, 0, m_SourceImage.Width, m_SourceImage.Height ), GraphicsUnit.Pixel );

			
			// Show custom swatches' location
			RectangleF		R = ImageClientRectangle;
			for ( int SwatchIndex=0; SwatchIndex < m_CustomSwatches.Length; SwatchIndex++ )
				if ( m_CustomSwatches[SwatchIndex] != null )
				{
					WMath.Vector4D	Location = m_CustomSwatches[SwatchIndex];

					PointF	TopLeft = new PointF( R.Left + Location.x * R.Width, R.Top + Location.y * R.Height );
					PointF	BottomRight = new PointF( R.Left + Location.z * R.Width, R.Top + Location.w * R.Height );

					e.Graphics.DrawRectangle( Pens.Red, TopLeft.X, TopLeft.Y, 1+BottomRight.X-TopLeft.X, 1+BottomRight.Y-TopLeft.Y );
					e.Graphics.DrawString( SwatchIndex.ToString(), Font, Brushes.Red, 0.5f * (TopLeft.X + BottomRight.X - Font.Height), 0.5f * (TopLeft.Y + BottomRight.Y - Font.Height) );
				}
		}

		#endregion
	}
}
