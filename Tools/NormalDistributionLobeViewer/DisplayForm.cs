using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LobeViewer
{
	public partial class DisplayForm : Form
	{
		const int	INTERSECTIONS_TABLE_SIZE = 256;

		protected Bitmap		m_Slice = null;
		protected Pen			m_Pen = null;

		protected double[,]		m_Intersections = null;

		public DisplayForm()
		{
			InitializeComponent();


			m_Pen = new Pen( Color.Black, 1.0f );
			m_Pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

			integerTrackbarControlPhiD_ValueChanged( integerTrackbarControlPhiD, 0 );
		}

		protected unsafe void	Redraw()
		{
			int		PhiD = integerTrackbarControlPhiD.Value;
			double	Gamma = 1.0 / floatTrackbarControlGamma.Value;
			double	Exposure = Math.Pow( 2.0, floatTrackbarControlExposure.Value );
			bool	bShowDifferences = checkBoxDifferences.Checked;

			bool	bUseWarping = checkBoxUseWarping.Checked;
			double	Warp = floatTrackbarControlWarpFactor.Value;

			byte	R, G, B;
			Vector3	Temp = new Vector3(), Temp2;
			BitmapData	LockedBitmap = m_Slice.LockBits( new Rectangle( 0, 0, m_Slice.Width, m_Slice.Height ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
			for ( int Y=0; Y < 90; Y++ )
			{
				int		ThetaD = 89 - Y;
				byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + Y * LockedBitmap.Stride;
				for ( int X=0; X < 90; X++ )
				{
					if ( !bUseWarping )
					{
						int	ThetaH = (int) (89 * Math.Sqrt( X/89.0 ));	// Square ThetaH

						Temp = m_BRDF[ThetaH,ThetaD,PhiD];

						if ( !bShowDifferences )
						{
							Temp.x = Math.Pow( Exposure * Temp.x, Gamma );
							Temp.y = Math.Pow( Exposure * Temp.y, Gamma );
							Temp.z = Math.Pow( Exposure * Temp.z, Gamma );
						}
						else
						{
							Temp2 = m_BRDF[ThetaH,ThetaD,Math.Min( 179, 179-PhiD )];	// Read from symetrical slice
							Temp.x = 64.0 * Exposure * Math.Abs( Temp.x - Temp2.x );
							Temp.y = 64.0 * Exposure * Math.Abs( Temp.y - Temp2.y );
							Temp.z = 64.0 * Exposure * Math.Abs( Temp.z - Temp2.z );
						}
					}
					else
					{	// Use slice warping !
						int	ThetaH = X;	// Don't square ThetaH just yet!

//						WarpSlice( ThetaH, ThetaD, Warp, ref Temp );
						WarpSlice( ThetaH, ThetaD, PhiD, ref Temp );

						if ( !bShowDifferences )
						{
							Temp.x = Math.Pow( Exposure * Temp.x, Gamma );
							Temp.y = Math.Pow( Exposure * Temp.y, Gamma );
							Temp.z = Math.Pow( Exposure * Temp.z, Gamma );
						}
						else
						{
							ThetaH = (int) (89 * Math.Sqrt( X/89.0 ));	// Square ThetaH
							Temp2 = m_BRDF[ThetaH,ThetaD,PhiD];
							Temp2.x = Math.Max( 0.0, Temp2.x );
							Temp2.y = Math.Max( 0.0, Temp2.y );
							Temp2.z = Math.Max( 0.0, Temp2.z );
							Temp.x = 10.0 * Exposure * Math.Abs( Temp.x - Temp2.x );
							Temp.y = 10.0 * Exposure * Math.Abs( Temp.y - Temp2.y );
							Temp.z = 10.0 * Exposure * Math.Abs( Temp.z - Temp2.z );
						}
					}

					R = (byte) Math.Max( 0, Math.Min( 255, 255.0 * Temp.x ) );
					G = (byte) Math.Max( 0, Math.Min( 255, 255.0 * Temp.y ) );
					B = (byte) Math.Max( 0, Math.Min( 255, 255.0 * Temp.z ) );

					*pScanline++ = B;
					*pScanline++ = G;
					*pScanline++ = R;
					*pScanline++ = 0xFF;
				}
			}
			m_Slice.UnlockBits( LockedBitmap );

// 			if ( checkBoxShowIsolines.Checked )
// 			{
// 				PointF	P0 = new PointF(), P1 = new PointF();
// 
// 				using ( Graphics Graph = Graphics.FromImage( m_Slice ) )
// 				{
// 					double	ThetaHalf, PhiHalf, ThetaDiff, PhiDiff;
// 					double	ThetaHalfN, PhiHalfN, ThetaDiffN, PhiDiffN;
// 					for ( int IsolineIndex=0; IsolineIndex < 8; IsolineIndex++ )
// 					{
// 						double	Angle = (1+IsolineIndex) * 0.49 * Math.PI / 8;
// //						double	Angle = 0.25 * Math.PI;
// 
// 						for ( int i=0; i < 40; i++ )
// 						{
// 							double	Phi = i * Math.PI / 40;
// 							std_coords_to_half_diff_coords( Angle, 0, Angle, Phi, out ThetaHalf, out PhiHalf, out ThetaDiff, out PhiDiff );
// 
// 							Phi = (i+1) * Math.PI / 40;
// 							std_coords_to_half_diff_coords( Angle, 0, Angle, Phi, out ThetaHalfN, out PhiHalfN, out ThetaDiffN, out PhiDiffN );
// 
// 							P0.X = (float) (m_Slice.Width * ThetaHalf * 0.63661977236758134307553505349006);			// divided by PI/2
// 							P0.Y = (float) (m_Slice.Height * (1.0f - ThetaDiff * 0.63661977236758134307553505349006));	// divided by PI/2
// 							P1.X = (float) (m_Slice.Width * ThetaHalfN * 0.63661977236758134307553505349006);			// divided by PI/2
// 							P1.Y = (float) (m_Slice.Height * (1.0f - ThetaDiffN * 0.63661977236758134307553505349006));	// divided by PI/2
// 							Graph.DrawLine( m_Pen, P0, P1 );
// 						}
// 					}
// 				}
// 			}

			panelDisplay.ShowIsoLines = checkBoxShowIsolines.Checked;
			panelDisplay.PhiD = Math.PI * PhiD / 180.0;
			panelDisplay.Slice = m_Slice;	// Will trigger update
		}

		private void integerTrackbarControlPhiD_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			Redraw();
		}

		private void floatTrackbarControlGamma_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			Redraw();
		}

		private void floatTrackbarControlExposure_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			Redraw();
		}

		private void checkBoxDifferences_CheckedChanged( object sender, EventArgs e )
		{
			Redraw();
		}

		private void checkBoxUseWarping_CheckedChanged( object sender, EventArgs e )
		{
			Redraw();
			floatTrackbarControlWarpFactor.Enabled = checkBoxUseWarping.Checked;
		}

		private void floatTrackbarControlWarpFactor_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			Redraw();
		}

		private void checkBoxShowIsolines_CheckedChanged( object sender, EventArgs e )
		{
			Redraw();
		}

		private void floatTrackbarControlScaleFactor_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{

		}

		private void buttonRebuild_Click( object sender, EventArgs e )
		{
			BuildScaleTable();
			Redraw();
		}
	}
}
