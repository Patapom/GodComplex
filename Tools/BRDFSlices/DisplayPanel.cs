using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace BRDFSlices
{
	public partial class DisplayPanel : Panel
	{
		protected Bitmap	m_Slice = null;
		public Bitmap	Slice
		{
			get { return m_Slice; }
			set
			{
				m_Slice = value;
				Invalidate();
			}
		}
		public bool		ShowIsoLines = false;
		public double	PhiD = -0.5 * Math.PI;

		private Pen		m_Pen = null;

		public DisplayPanel()
		{
			InitializeComponent();
		}

		public DisplayPanel( IContainer container )
		{
			container.Add( this );

			InitializeComponent();
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected void	Convert( double _Theta, double _Phi, out double _ThetaH, out double _ThetaD )
		{
 			double	HalfPhi = 0.5 * _Phi;
// //			_ThetaH = Math.Atan( Math.Cos( _Theta ) / (Math.Sin( _Theta ) * Math.Cos( HalfPhi ) ) );
// 			_ThetaH = Math.Atan( Math.Tan( _Theta ) * Math.Cos( HalfPhi ) );
// 
// 			double	SinSq = 2.0 * Math.Sin( _Theta )*Math.Sin( _Theta ) * Math.Sin( HalfPhi )*Math.Sin( HalfPhi );
// 			_ThetaD = 0.5 * Math.Acos( 1.0 - SinSq );

			_ThetaD = Math.Asin( Math.Sin( HalfPhi ) * Math.Sin( _Theta ) );
			_ThetaH = Math.Atan( Math.Cos( HalfPhi ) * Math.Tan( _Theta ) );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			if ( m_Slice != null )
				e.Graphics.DrawImage( m_Slice, 0, 0, Width, Height );

			if ( ShowIsoLines )
			{
				PointF	P0 = new PointF(), P1 = new PointF();

				double	ThetaHalf, PhiHalf, ThetaDiff, PhiDiff;
				double	ThetaHalfN, PhiHalfN, ThetaDiffN, PhiDiffN;
				for ( int IsolineIndex=0; IsolineIndex < 20; IsolineIndex++ )
				{
					double	ThetaI = (1+IsolineIndex) / 20.0 * 0.495 * Math.PI;
//					double	ThetaI = Math.Sqrt( (1+IsolineIndex) / 12.0 ) * 0.49 * Math.PI;

					for ( int i=0; i < 20; i++ )
					{
						ThetaI = (1.0+IsolineIndex) / 20.0 * 0.495 * Math.PI;
						double	HalfPhi = 0.5 * i * Math.PI / 20;
						DisplayForm.std_coords_to_half_diff_coords( ThetaI, HalfPhi, ThetaI, -HalfPhi, out ThetaHalf, out PhiHalf, out ThetaDiff, out PhiDiff );
//Convert( ThetaI, Phi, out ThetaHalf, out ThetaDiff );

						// Change Phi_d
						double	TestThetaI, TestPhiI, TestThetaO, TestPhiO;
						DisplayForm.half_diff_coords_to_std_coords( ThetaHalf, PhiHalf, ThetaDiff, PhiD, out TestThetaI, out TestPhiI, out TestThetaO, out TestPhiO );
						DisplayForm.std_coords_to_half_diff_coords( TestThetaI, TestPhiI, TestThetaO, TestPhiO, out ThetaHalf, out PhiHalf, out ThetaDiff, out PhiDiff );


						ThetaI = (1.2+IsolineIndex) / 20.0 * 0.495 * Math.PI;
						HalfPhi = 0.5 * (i+1) * Math.PI / 20;
						DisplayForm.std_coords_to_half_diff_coords( ThetaI, HalfPhi, ThetaI, -HalfPhi, out ThetaHalfN, out PhiHalfN, out ThetaDiffN, out PhiDiffN );
//Convert( ThetaI, Phi, out ThetaHalfN, out ThetaDiffN );

						// Change Phi_d
						DisplayForm.half_diff_coords_to_std_coords( ThetaHalfN, PhiHalfN, ThetaDiffN, PhiD, out TestThetaI, out TestPhiI, out TestThetaO, out TestPhiO );
						DisplayForm.std_coords_to_half_diff_coords( TestThetaI, TestPhiI, TestThetaO, TestPhiO, out ThetaHalfN, out PhiHalfN, out ThetaDiffN, out PhiDiffN );

						P0.X = (float) (Width * ThetaHalf * 0.63661977236758134307553505349006);			// divided by PI/2
						P0.Y = (float) (Height * (1.0f - ThetaDiff * 0.63661977236758134307553505349006));	// divided by PI/2
						P1.X = (float) (Width * ThetaHalfN * 0.63661977236758134307553505349006);			// divided by PI/2
						P1.Y = (float) (Height * (1.0f - ThetaDiffN * 0.63661977236758134307553505349006));	// divided by PI/2
						e.Graphics.DrawLine( m_Pen, P0, P1 );
					}

//Convert( ThetaI, 0.8 * Math.PI, out ThetaHalf, out ThetaDiff );

					{
						double	ThetaDH = Math.Acos( Math.Sqrt( Math.Cos( ThetaI ) ) );
						double	PhiI = 2.0 * Math.Atan( 1.0 / Math.Cos( ThetaDH ) );
//						DisplayForm.std_coords_to_half_diff_coords( ThetaI, 0, ThetaI, PhiI, out ThetaHalf, out PhiHalf, out ThetaDiff, out PhiDiff );
						Convert( ThetaI, PhiI, out ThetaHalf, out ThetaDiff );
					}

					P0.X = (float) (Width * ThetaHalf * 0.63661977236758134307553505349006) - 16.0f;				// divided by PI/2
					P0.Y = (float) (Height * (1.0f - ThetaDiff * 0.63661977236758134307553505349006)) - 0*16.0f;	// divided by PI/2
					e.Graphics.DrawString( "" + (int) (ThetaI * 180 / Math.PI) + "°", Font, Brushes.Black, P0 );

				}
			}

			if ( ShowIsoLines )
			{
				PointF	P0 = new PointF(), P1 = new PointF();

				double	ThetaHalf, PhiHalf, ThetaDiff, PhiDiff;
				double	ThetaHalfN, PhiHalfN, ThetaDiffN, PhiDiffN;
				for ( int IsolineIndex=0; IsolineIndex < 19; IsolineIndex++ )
				{
 					double	Phi = (1.0+IsolineIndex) / 20.0 * Math.PI;
//					double	Phi = Math.Sqrt( (0.5+IsolineIndex) / 12.0 ) * Math.PI;

					for ( int i=0; i < 40; i++ )
					{
						double	Angle = i * 0.50 * Math.PI / 40;

						DisplayForm.std_coords_to_half_diff_coords( Angle, 0, Angle, Phi, out ThetaHalf, out PhiHalf, out ThetaDiff, out PhiDiff );
Convert( Angle, Phi, out ThetaHalf, out ThetaDiff );

						Angle = (i+1) * 0.50 * Math.PI / 40;
						DisplayForm.std_coords_to_half_diff_coords( Angle, 0, Angle, Phi, out ThetaHalfN, out PhiHalfN, out ThetaDiffN, out PhiDiffN );
Convert( Angle, Phi, out ThetaHalfN, out ThetaDiffN );

						P0.X = (float) (Width * ThetaHalf * 0.63661977236758134307553505349006);			// divided by PI/2
						P0.Y = (float) (Height * (1.0f - ThetaDiff * 0.63661977236758134307553505349006));	// divided by PI/2
						P1.X = (float) (Width * ThetaHalfN * 0.63661977236758134307553505349006);			// divided by PI/2
						P1.Y = (float) (Height * (1.0f - ThetaDiffN * 0.63661977236758134307553505349006));	// divided by PI/2
						e.Graphics.DrawLine( m_Pen, P0, P1 );
					}

					DisplayForm.std_coords_to_half_diff_coords( 0.5*Math.PI, 0, 0.5*Math.PI, Phi, out ThetaHalf, out PhiHalf, out ThetaDiff, out PhiDiff );
					P0.X = (float) (Width * ThetaHalf * 0.63661977236758134307553505349006) - 30.0f;				// divided by PI/2
					P0.Y = (float) (Height * (1.0f - ThetaDiff * 0.63661977236758134307553505349006)) + 2.0f;		// divided by PI/2
					e.Graphics.DrawString( "" + (int) (Phi * 180 / Math.PI) + "°", Font, Brushes.Black, P0 );
				}
			}
		}

	}
}
