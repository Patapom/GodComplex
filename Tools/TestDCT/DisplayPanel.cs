using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace TestDCT
{
	public partial class DisplayPanel : Panel
	{
		public float[][]	m_Curve = null;
		public float[]		m_DCTCoefficients = null;

		private Pen			m_Pen = null;

		public DisplayPanel( IContainer container )
		{
			container.Add( this );

			InitializeComponent();
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			e.Graphics.FillRectangle( Brushes.White, 0, 0, Width, Height );

			if ( m_Curve != null )
			{	// Paint curve
				float	PrevX, PrevY;
				float	X = 0, Y = 0;
				for ( int i=0; i < m_Curve.Length; i++ )
				{
					PrevX = X;
					PrevY = Y;
					X = Width * m_Curve[i][0] / Form1.MAX_Z;
					Y = Height * (1.0f - m_Curve[i][1]);

					if ( i > 0 )
						e.Graphics.DrawLine( Pens.Black, PrevX, PrevY, X, Y );
				}
			}
			else if ( m_DCTCoefficients != null )
			{	// Paint estimate from DCT coeffs
				float	PrevX, PrevY;
				float	X = 0, Y = 0;
				for ( int i=0; i < Width; i++ )
				{
//					float	Z = Form1.MAX_Z * i / Width;
					float	z = (float) (0.5f + i) / Width;			// Normalized Z

					float	T = 0.0f;
// 					for ( int j=0; j < m_DCTCoefficients.Length; j++ )
// 					{
// 						float	Bj = (float) Math.Cos( j * Math.PI * z );
// 						T += m_DCTCoefficients[j] * Bj;
// 					}
// 					T *= 1.0f;// / Form1.MAX_Z;

                    // Tricky inverse DCT treats coeff 0 specially!
					T = 0.5f * m_DCTCoefficients[0];
					for ( int j=1; j < m_DCTCoefficients.Length; j++ )
					{
						float	Bj = (float) Math.Cos( j * Math.PI * z );
						T += m_DCTCoefficients[j] * Bj;
					}
//					T *= 2.0f / 8.0f;

					PrevX = X;
					PrevY = Y;
					X = i;
					Y = Height * (1.0f - T);

					if ( i > 0 )
						e.Graphics.DrawLine( Pens.Black, PrevX, PrevY, X, Y );
				}
			}
		}
	}
}
