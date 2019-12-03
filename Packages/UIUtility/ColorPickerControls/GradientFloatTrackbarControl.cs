using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UIUtility
{
	public partial class GradientFloatTrackbarControl : FloatTrackbarControl
	{
		#region FIELDS

		protected Color		m_ColorMin = Color.FromArgb( 255, 200, 200, 200 );
		protected Color		m_ColorMax = Color.FromArgb( 255, 200, 200, 200 );

		#endregion

		#region PROPERTIES

		[Description( "The color at the minimum end of the slider" )]
		[Category( "Appearance" )]
		public Color		ColorMin
		{
			get { return m_ColorMin; }
			set { m_ColorMin = value; Invalidate(); }
		}

		[Description( "The color at the maximum end of the slider" )]
		[Category( "Appearance" )]
		public Color		ColorMax
		{
			get { return m_ColorMax; }
			set { m_ColorMax = value; Invalidate(); }
		}

		#endregion

		#region METHODS

		public GradientFloatTrackbarControl()
		{
			InitializeComponent();

			InitializeGraphics();
		}

		#region Graphics Creation

		protected override void DrawSlider( PaintEventArgs e )
		{
			float		fSizeToDraw = m_SliderRectangle.Width * (Value - VisibleRangeMin) / (VisibleRangeMax - VisibleRangeMin);

			if ( m_ColorMin.A == 255 && m_ColorMax.A == 255 )
				e.Graphics.FillRectangle( m_BackgroundBrush, m_SliderRectangle.X + fSizeToDraw, m_SliderRectangle.Y, m_SliderRectangle.Width - fSizeToDraw, m_SliderRectangle.Height );
			else
			{	// Draw a nice checker box background
				System.Drawing.Drawing2D.HatchBrush	Checker = new System.Drawing.Drawing2D.HatchBrush( System.Drawing.Drawing2D.HatchStyle.LargeCheckerBoard, Color.Gray, Color.DarkGray );

				e.Graphics.FillRectangle( Checker, m_SliderRectangle );

				Checker.Dispose();
			}

			// Draw a gradient box
			if ( fSizeToDraw < 1.0f )
				return;	// Crashes if empty!

			RectangleF	Rect = new RectangleF( m_SliderRectangle.X, m_SliderRectangle.Y, fSizeToDraw, m_SliderRectangle.Height );

			System.Drawing.Drawing2D.GraphicsPath		Path = new System.Drawing.Drawing2D.GraphicsPath();
														Path.AddRectangle( Rect );

			System.Drawing.Drawing2D.PathGradientBrush	Gradient = new System.Drawing.Drawing2D.PathGradientBrush( Path );
														Gradient.SurroundColors = new Color[] { m_ColorMin, m_ColorMax, m_ColorMax, m_ColorMin };
 														Gradient.CenterPoint = new PointF( 0.5f * (Rect.Left + Rect.Right), .5f * (Rect.Bottom + Rect.Top) );
 														Gradient.CenterColor = Color.FromArgb( (m_ColorMin.A + m_ColorMax.A) / 2, (m_ColorMin.R + m_ColorMax.R) / 2, (m_ColorMin.G + m_ColorMax.G) / 2, (m_ColorMin.B + m_ColorMax.B) / 2 );

			e.Graphics.FillRectangle( Gradient, Rect );

 			Gradient.Dispose();
 			Path.Dispose();
		}

		#endregion

		#endregion
	}
}
