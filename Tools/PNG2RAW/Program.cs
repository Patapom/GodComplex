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
// 			if ( args.Length != 2 )
// 				throw new Exception( "First argument must be the path to the PNG file, second argument is the path to the RAW file." );
// 
//			Convert( args[0], args[1] );


// 			// Material 0
// 			string[]	FileNames = new string[]
// 			{
// 				"LayeredMaterial0-Layer0",
// 				"LayeredMaterial0-Layer1",
// 				"LayeredMaterial0-Layer2",
// 				"LayeredMaterial0-Layer3",
// 				"LayeredMaterial0-Specular",
// 				"LayeredMaterial0-Height",
// 			};

			// Material 1
			string[]	FileNames = new string[]
			{
				"LayeredMaterial1-Layer0",
				"LayeredMaterial1-Layer3",
				"LayeredMaterial1-Specular",
				"LayeredMaterial1-Height",
			};

			foreach ( string FileName in FileNames )
			{
				string	Source = FileName + ".png";
				string	Target = FileName + ".raw";
				Convert( Source, Target );
			}
		}

		static unsafe void	Convert( string _Source, string _Target )
		{
			using ( Bitmap B = Image.FromFile( _Source ) as Bitmap )
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

				using ( FileStream S = new FileInfo( _Target ).Create() )
					S.Write( RAWImage, 0, RAWImage.Length );
			}
		}
	}
}
