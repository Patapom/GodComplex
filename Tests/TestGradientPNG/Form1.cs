using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace TestGradientPNG
{
	public partial class Form1 : Form
	{
		public unsafe Form1()
		{
			InitializeComponent();



TestIntegral();



			byte[]	GradientValues = new byte[256];
			using ( Bitmap Gradient = Bitmap.FromFile( @"C:\Users\Patapom\Desktop\Arkane Color Profiling\MaterialsChart\Gradient.png" ) as Bitmap )
			{
				BitmapData	LockedBitmap = Gradient.LockBits( new Rectangle( 0, 0, 256, 1 ), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb );

				byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer();
				for ( int X=0; X < 256; X++ )
				{
					GradientValues[X] = *pScanline;
					pScanline += 4;
				}

				Gradient.UnlockBits( LockedBitmap );
			}

			panelGraph.Gradient = GradientValues;
		}

	
		void	TestIntegral()
		{
			double[]	Angles = new double[] {
 0,
 5,
 10,
 15,
 20,
 25,
 30,
 35,
 40,
 45,
 50,
 55,
 60,
 65,
 70,
 75,
 80,
 85,
 90,
			};
			double[]	Candelas = new double[] {
				681.879276637, 557.110079287, 490.896871945, 628.387368307, 682.069349408, 430.426577604, 286.398935592, 254.901162159, 76.9183773216, 18.9393939394, 11.0377973281, .488758553275, .00678831323993, 0, 0, 0, 0, 0, 0,
			};	// Lumen = 680.13291184476259 (sans multiplier par 2.2)

// 			double[]	Candelas = new double[] {
// 				936.4, 1015, 1045, 961.8, 719.1, 425.5, 150.9, 119.1, 114.5, 100, 40.91, 29.09, 20, 20, 16.36, 14.55, 14.55, 14.55, 14.55
// 			};	// Lumen = 889.7819568483286 (sans multiplication par 0.825)

			double	Lumen = 0.0;
			for ( int AngleIndex=0; AngleIndex < Angles.Length-1; AngleIndex++ )
			{
				double	Angle0 = Math.PI * Angles[AngleIndex] / 180.0;
				double	Angle1 = Math.PI * Angles[AngleIndex+1] / 180.0;
				double	dw = Math.Sin( Angle1 ) * (Angle1 - Angle0);		// sin(Theta).dTheta
//				double	dw = Math.Sin( 0.5 * (Angle0 + Angle1) ) * (Angle1 - Angle0);		// sin(Theta).dTheta

				double	V0 = Candelas[AngleIndex];
				double	V1 = Candelas[AngleIndex+1];
				double	dI = 0.5 * (V0+V1) * dw;

				Lumen += dI;
			}
			Lumen *= 2.0 * Math.PI;

			Lumen *= 0.825;
		}
	}
}
