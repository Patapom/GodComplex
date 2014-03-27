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
//			ConvertPOM( new FileInfo( "Resources/Scenes/GITest1/pata_diff_color_small.png" ), new FileInfo( "Resources/Scenes/GITest1/pata_diff_colo.pom" ), true, true );
//			ConvertPOM( new DirectoryInfo( "../Arkane/Textures" ), new DirectoryInfo( "../Arkane/TexturesPOM" ), "_d;_s", true );
			ConvertPOM( new DirectoryInfo( "./Resources/Scenes/Sponza/TexturesPNG" ), new DirectoryInfo( "./Resources/Scenes/Sponza/TexturesPOM" ), "_dif;_diff;_spec", true );

#endif
		}

		static void			ConvertPOM( DirectoryInfo _SourceDir, DirectoryInfo _TargetDir, string _sRGBPattern, bool _GenerateMipMaps )
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
					ConvertPOM( File, TargetFile, issRGB, _GenerateMipMaps );
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

		static unsafe void	ConvertPOM( FileInfo _Source, FileInfo _Target, bool _sRGB, bool _GenerateMipMaps )
		{
			int	Width, Height;
			int	MipLevelsCount = 1;
			byte[][]	RAWImages = null;
			using ( Bitmap B = Image.FromFile( _Source.FullName ) as Bitmap )
			{
				if ( _GenerateMipMaps )
				{
					double	Mips = Math.Log( Math.Max( B.Width, B.Height ) ) / Math.Log( 2.0 );
					MipLevelsCount = 1 + (int) Math.Ceiling( Mips );
				}

				RAWImages = new byte[MipLevelsCount][];

				Width = B.Width;
				Height = B.Height;

				// Build mip #0
				byte[]	RAWImage = new byte[4*Width*Height];
				RAWImages[0] = RAWImage;
				
				BitmapData	LockedBitmap = B.LockBits( new Rectangle( 0, 0, Width, Height ), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb );

				int			ByteIndex = 0;
				for ( int Y=0; Y < Height; Y++ )
				{
					byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + Y * LockedBitmap.Stride;
					for ( int X=0; X < Width; X++ )
					{
						RAWImage[ByteIndex+2] = *pScanline++;
						RAWImage[ByteIndex+1] = *pScanline++;
						RAWImage[ByteIndex+0] = *pScanline++;
						RAWImage[ByteIndex+3] = *pScanline++;
						ByteIndex += 4;
					}
				}

				B.UnlockBits( LockedBitmap );
			}

			// Generate other mips
			int	W = Width;
			int	H = Height;

			for ( int MipLevelIndex=1; MipLevelIndex < MipLevelsCount; MipLevelIndex++ )
			{
				int	PW = W;
				int	PH = W;
				W = Math.Max( 1, W >> 1 );
				H = Math.Max( 1, H >> 1 );

				byte[]	PreviousMip = RAWImages[MipLevelIndex-1];
				byte[]	CurrentMip = new byte[4*W*H];
				RAWImages[MipLevelIndex] = CurrentMip;

				byte	R, G, B, A;
				for ( int Y=0; Y < H; Y++ )
				{
					int	PY0 = PH * Y / H;
					int	PY1 = Math.Min( PY0+1, PH-1 );
					for ( int X=0; X < W; X++ )
					{
						int	PX0 = PW * X / W;
						int	PX1 = Math.Min( PX0+1, PW-1 );
						
						if ( _sRGB )
						{
							R = Lin2sRGB( 0.25f * (sRGB2Lin( PreviousMip[4*(PW*PY0+PX0)+0] ) + sRGB2Lin( PreviousMip[4*(PW*PY0+PX1)+0] ) + sRGB2Lin( PreviousMip[4*(PW*PY1+PX0)+0] ) + sRGB2Lin( PreviousMip[4*(PW*PY1+PX1)+0]) ) );
							G = Lin2sRGB( 0.25f * (sRGB2Lin( PreviousMip[4*(PW*PY0+PX0)+1] ) + sRGB2Lin( PreviousMip[4*(PW*PY0+PX1)+1] ) + sRGB2Lin( PreviousMip[4*(PW*PY1+PX0)+1] ) + sRGB2Lin( PreviousMip[4*(PW*PY1+PX1)+1]) ) );
							B = Lin2sRGB( 0.25f * (sRGB2Lin( PreviousMip[4*(PW*PY0+PX0)+2] ) + sRGB2Lin( PreviousMip[4*(PW*PY0+PX1)+2] ) + sRGB2Lin( PreviousMip[4*(PW*PY1+PX0)+2] ) + sRGB2Lin( PreviousMip[4*(PW*PY1+PX1)+2]) ) );
						}
						else
						{	// Simple average will do. I should handle normal maps mip-mapping more seriously but I just don't care...
							R = (byte) ((PreviousMip[4*(PW*PY0+PX0)+0] + PreviousMip[4*(PW*PY0+PX1)+0] + PreviousMip[4*(PW*PY1+PX0)+0] + PreviousMip[4*(PW*PY1+PX1)+0]) >> 2);
							G = (byte) ((PreviousMip[4*(PW*PY0+PX0)+1] + PreviousMip[4*(PW*PY0+PX1)+1] + PreviousMip[4*(PW*PY1+PX0)+1] + PreviousMip[4*(PW*PY1+PX1)+1]) >> 2);
							B = (byte) ((PreviousMip[4*(PW*PY0+PX0)+2] + PreviousMip[4*(PW*PY0+PX1)+2] + PreviousMip[4*(PW*PY1+PX0)+2] + PreviousMip[4*(PW*PY1+PX1)+2]) >> 2);
						}

						A = (byte) ((PreviousMip[4*(PW*PY0+PX0)+3] + PreviousMip[4*(PW*PY0+PX1)+3] + PreviousMip[4*(PW*PY1+PX0)+3] + PreviousMip[4*(PW*PY1+PX1)+3]) >> 2);

						CurrentMip[4*(W*Y+X)+0] = R;
						CurrentMip[4*(W*Y+X)+1] = G;
						CurrentMip[4*(W*Y+X)+2] = B;
						CurrentMip[4*(W*Y+X)+3] = A;
					}
				}
			}

			// Write the file
			using ( FileStream S = new FileInfo( _Target.FullName ).Create() )
				using ( BinaryWriter BW = new BinaryWriter( S ) )
				{
					// Write type & format
					BW.Write( (byte) 0 );	// 2D
					BW.Write( (byte) (28 + (_sRGB ? 1 : 0)) );		// DXGI_FORMAT_R8G8B8A8_UNORM=28, DXGI_FORMAT_R8G8B8A8_UNORM_SRGB=29

					// Write dimensions
					BW.Write( (Int32) Width );
					BW.Write( (Int32) Height );
					BW.Write( (Int32) 1 );				// ArraySize = 1
					BW.Write( (Int32) MipLevelsCount );

					W = Width;
					H = Height;

					for ( int MipLevelIndex=0; MipLevelIndex < MipLevelsCount; MipLevelIndex++ )
					{
						// Write row & depth pitch
						BW.Write( (Int32) (W * 4) );
						BW.Write( (Int32) RAWImages[MipLevelIndex].Length );

						// Write content
						S.Write( RAWImages[MipLevelIndex], 0, RAWImages[MipLevelIndex].Length );

						W = Math.Max( 1, W >> 1 );
						H = Math.Max( 1, H >> 1 );
					}
				}
		}

		private static float	sRGB2Lin( byte _sRGB )
		{
			float	sRGB = _sRGB / 255.0f;
			return sRGB > 0.04045f ? (float) Math.Pow( (sRGB + 0.055f) / 1.055f, 2.4f ) : sRGB / 12.92f;
		}

		private static byte		Lin2sRGB( float _Linear )
		{
			float	sRGB = _Linear > 0.0031308f ? 1.055f * (float) Math.Pow( _Linear, 1.0f / 2.4f ) - 0.055f : 12.92f * _Linear;
			return (byte) Math.Max( 0, Math.Min( 255, (255.0f * sRGB) ) );
		}
	}
}
