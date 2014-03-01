//#define SAVE_RAW

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

#if SAVE_RAW
			string[]	FileNames = new string[]
			{
				"Iris_Blades",

				// Material 0
				"LayeredMaterial0-Layer0",
				"LayeredMaterial0-Layer1",
				"LayeredMaterial0-Layer2",
				"LayeredMaterial0-Layer3",
				"LayeredMaterial0-Specular",
				"LayeredMaterial0-Height",

				// Material 1
				"LayeredMaterial1-Layer0",
				"LayeredMaterial1-Layer3",
				"LayeredMaterial1-Specular",
				"LayeredMaterial1-Height",

				// Material 2
				"LayeredMaterial2-Layer0",
				"LayeredMaterial2-Layer1",
				"LayeredMaterial2-Height",

				// Material 3
				"LayeredMaterial3-Layer0",
				"LayeredMaterial3-Layer1",
				"LayeredMaterial3-Layer2",
				"LayeredMaterial3-Layer3",
				"LayeredMaterial3-Specular",
				"LayeredMaterial3-Height",

			};

			foreach ( string FileName in FileNames )
			{
				string	Source = FileName + ".png";
				Convert( Source, FileName + ".raw" );
			}
#else
			// Convert to POM format (i.e. DirectX format actually)
			ConvertPOM( new FileInfo( "../../../Resources/Scenes/GITest1/pata_diff_color_small.png" ), new FileInfo( "../../../Resources/Scenes/GITest1/pata_diff_colo.pom" ), true );

			ConvertPOM( new DirectoryInfo( "../../../../Arkane/Textures" ), new DirectoryInfo( "../../../../Arkane/TexturesPOM" ), "_d;_s" );

#endif
		}

		static void			ConvertPOM( DirectoryInfo _SourceDir, DirectoryInfo _TargetDir, string _sRGBPattern )
		{
			string[]	sRGBPatterns = _sRGBPattern != null ? _sRGBPattern.Split( ';' ) : new string[0];
			FileInfo[]	Files = _SourceDir.GetFiles( "*.png" );
			int			FileIndex = 1;
			foreach ( FileInfo File in Files )
			{
				bool	issRGB = false;
				foreach ( string sRGBPattern in sRGBPatterns )
				{
					if ( Path.GetFileNameWithoutExtension( File.FullName ).EndsWith( sRGBPattern, StringComparison.CurrentCultureIgnoreCase ) )
					{
						issRGB = true;
						break;
					}
				}

				FileInfo	TargetFile = new FileInfo( Path.Combine( _TargetDir.FullName, Path.GetFileNameWithoutExtension( File.FullName ) + ".pom" ) );

				try
				{
					ConvertPOM( File, TargetFile, issRGB );
					Console.WriteLine( "Converted \"" + File.FullName + "\" => \"" + TargetFile.FullName + "\"... (" + (100 * FileIndex / Files.Length) + "%)" );
				}
				catch ( Exception _e )
				{
					Console.WriteLine( "An error occurred while converting \"" + File.FullName + "\" => \"" + TargetFile.FullName + "\": " + _e.Message );
				}

				FileIndex++;
			}
		}

		static unsafe void	ConvertRAW( string _Source, string _Target )
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

		static unsafe void	ConvertPOM( FileInfo _Source, FileInfo _Target, bool _sRGB )
		{
			using ( Bitmap B = Image.FromFile( _Source.FullName ) as Bitmap )
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

				using ( FileStream S = new FileInfo( _Target.FullName ).Create() )
					using ( BinaryWriter W = new BinaryWriter( S ) )
					{
						// Write type & format
						W.Write( (byte) 0 );	// 2D
						W.Write( (byte) (28 + (_sRGB ? 1 : 0)) );		// DXGI_FORMAT_R8G8B8A8_UNORM=28, DXGI_FORMAT_R8G8B8A8_UNORM_SRGB=29

						// Write dimensions
						W.Write( (Int32) B.Width );
						W.Write( (Int32) B.Height );
						W.Write( (Int32) 1 );			// ArraySize = 1
						W.Write( (Int32) 1 );			// MipLevel = 1

						// Write row & depth pitch
						W.Write( (Int32) (B.Width * 4) );
						W.Write( (Int32) (B.Width * B.Height * 4) );

						// Write content
						S.Write( RAWImage, 0, RAWImage.Length );
					}
			}
		}
	}
}
