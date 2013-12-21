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

			
			using ( Bitmap B = new Bitmap( 512, 512, PixelFormat.Format32bppArgb ) )
			{
				BitmapData	LockedBitmap = B.LockBits( new Rectangle( 0, 0, 512, 512 ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );

				for ( int Y=0; Y < 512; Y++ )
				{
					byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + Y * LockedBitmap.Stride;
					for ( int X=0; X < 512; X++ )
					{
						double	Angle = Math.Atan2( Y - 255, X - 255 );
//						byte	C = (byte) (255.0 * ((Angle + Math.PI) % (0.5 * Math.PI)) / (0.5 * Math.PI));
						byte	C = (byte) (255.0 * ((Angle + Math.PI) % Math.PI) / Math.PI);
						*pScanline++ = C;
						*pScanline++ = C;
						*pScanline++ = C;
						*pScanline++ = (byte) ((X-255)*(X-255) + (Y-255)*(Y-255) < 255*255 ? 255 : 0);
					}
				}

				B.UnlockBits( LockedBitmap );

				B.Save( "\\Anisotropy.png", ImageFormat.Png ); 
			}
		}

		private void floatTrackbarControlScaleX_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.ScaleX = _Sender.Value;
		}

		private void floatTrackbarControlScaleY_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.ScaleY = _Sender.Value;
		}

		private void floatTrackbarControlWhitePoint_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.WhitePoint = _Sender.Value;
		}

		private void floatTrackbarControlA_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.A = _Sender.Value;
		}

		private void floatTrackbarControlB_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.B = _Sender.Value;
		}

		private void floatTrackbarControlC_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.C = _Sender.Value;
		}

		private void floatTrackbarControlD_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.D = _Sender.Value;
		}

		private void floatTrackbarControlE_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.E = _Sender.Value;
		}

		private void floatTrackbarControlF_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.F = _Sender.Value;
		}
	}
}
