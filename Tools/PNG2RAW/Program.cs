using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace PNG2RAW
{
	class Program
	{
		static unsafe void Main( string[] args )
		{
			if ( args.Length != 2 )
				throw new Exception( "First argument must be the path to the PNG file, second argument is the path to the RAW file." );

			using ( Bitmap B = Image.FromFile( args[0] ) as Bitmap )
			{
				BitmapData	LockedBitmap = B.LockBits( new Rectangle( 0, 0, B.Width, B.Height ), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb );

				byte[]		RAWImage = new byte[4*B.Width*B.Height];
				int			ByteIndex = 0;
				for ( int Y=0; Y < B.Height; Y++ )
				{
					byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + Y * LockedBitmap.Stride;
					for ( int X=0; X < B.Width; X++ )
					{
						RAWImage[ByteIndex+2] = *pScanline++;
						RAWImage[ByteIndex+1] = *pScanline++;
						RAWImage[ByteIndex+0] = *pScanline++;
						RAWImage[ByteIndex+3] = *pScanline++;
						ByteIndex += 4;
					}
				}

				B.UnlockBits( LockedBitmap );

				using ( FileStream S = new FileInfo( args[1] ).Create() )
					S.Write( RAWImage, 0, RAWImage.Length );
			}
		}
	}
}
