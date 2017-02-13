#define FILTER_EXP_SHADOW_MAP
#define USE_COMPUTE_SHADER_FOR_BRDF_INTEGRATION

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

using Renderer;
using SharpMath;
using Nuaj.Cirrus.Utility;
using Nuaj.Cirrus;

namespace AreaLightTest {
	public partial class AreaLightForm : Form {
		private Device		m_Device = new Device();


		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float3		iResolution;
			public float		iGlobalTime;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Camera {
			public float4x4		_Camera2World;
			public float4x4		_World2Camera;
			public float4x4		_Proj2World;
			public float4x4		_World2Proj;
			public float4x4		_Camera2Proj;
			public float4x4		_Proj2Camera;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Light {
			public float3		_AreaLightX;
			public float		_AreaLightScaleX;
			public float3		_AreaLightY;
			public float		_AreaLightScaleY;
			public float3		_AreaLightZ;
			public float		_AreaLightDiffusion;
			public float3		_AreaLightT;
			public float		_AreaLightIntensity;
			public float4		_AreaLightTexDimensions;	// XY=Texture size, ZW=1/XY
			public float3		_ProjectionDirectionDiff;	// Closer to portal when diffusion increases
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_ShadowMap {
			public float2		_ShadowOffsetXY;			// XY offset in local light space where to place the shadow
			public float2		_ShadowZFar;				// X=Far clip distance for the shadow, Y=1/X
			public float		_InvShadowMapSize;			// 1/Size of the shadow map
			public float		_KernelSize;				// Size of the filtering kernel
			public float2		_HardeningFactor;			// Hardening factor for the sigmoïd
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Object {
			public float4x4		_Local2World;
			public float4x4		_World2Local;
			public float3		_DiffuseAlbedo;
			public float		_Gloss;
			public float3		_SpecularTint;
			public float		_Metal;
			public UInt32		_UseTexture;
			public UInt32		_FalseColors;
			public float		_FalseColorsMaxRange;
		}

		private ConstantBuffer<CB_Main>			m_CB_Main = null;
		private ConstantBuffer<CB_Camera>		m_CB_Camera = null;
		private ConstantBuffer<CB_Light>		m_CB_Light = null;
		private ConstantBuffer<CB_ShadowMap>	m_CB_ShadowMap = null;
		private ConstantBuffer<CB_Object>		m_CB_Object = null;

		private Shader		m_Shader_RenderShadowMap = null;

#if FILTER_EXP_SHADOW_MAP
		private Shader		m_Shader_FilterShadowMapH = null;
		private Shader		m_Shader_FilterShadowMapV = null;
#else
		private Shader		m_Shader_BuildSmoothie = null;
		private Shader		m_Shader_BuildSmoothieDistanceFieldH = null;
		private Shader		m_Shader_BuildSmoothieDistanceFieldV = null;
#endif
		private Shader		m_Shader_RenderAreaLight = null;
		private Shader		m_Shader_RenderScene = null;

		private Texture2D	m_Tex_AreaLight = null;
//		private Texture3D	m_Tex_AreaLight3D = null;
// 		private Texture2D	m_Tex_AreaLightSAT = null;
// 		private Texture2D	m_Tex_AreaLightSATFade = null;
		private Texture2D	m_Tex_FalseColors = null;

// 		private Texture2D	m_Tex_GlossMap = null;
// 		private Texture2D	m_Tex_Normal = null;

		private Texture2D	m_Tex_ShadowMap = null;
#if FILTER_EXP_SHADOW_MAP
		private Texture2D[]	m_Tex_ShadowMapFiltered = new Texture2D[2];
#else
		private Texture2D	m_Tex_ShadowSmoothie = null;
		private Texture2D[]	m_Tex_ShadowSmoothiePou = new Texture2D[2];
#endif

		private Texture2D	m_Tex_BRDFIntegral = null;

		private Primitive	m_Prim_Quad = null;
		private Primitive	m_Prim_Rectangle = null;
		private Primitive	m_Prim_Sphere = null;
		private Primitive	m_Prim_Cube = null;


		private Camera				m_Camera = new Camera();
		private CameraManipulator	m_Manipulator = new CameraManipulator();

		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_StopWatch = new System.Diagnostics.Stopwatch();
		private double						m_Ticks2Seconds;
		public float						m_StartTime = 0;
		public float						m_CurrentTime = 0;
		public float						m_DeltaTime = 0;		// Delta time used for the current frame

		public AreaLightForm() {
			InitializeComponent();

//			TestBisou();
		}


		#region Test Bisou

		// Conversions sRGB RGB <=> CIE XYZ using D65 illuminant (sRGB white point)
		float3    RGB2XYZ( float3 _RGB ) {
			return new float3(  _RGB.Dot( new float3( 0.4124f, 0.3576f, 0.1805f ) ),
								_RGB.Dot( new float3( 0.2126f, 0.7152f, 0.0722f ) ),                                // <= Y component is the LUMINANCE defined above
								_RGB.Dot( new float3( 0.0193f, 0.1192f, 0.9505f ) ) );

		}

		float3    XYZ2RGB( float3 _XYZ ) {
			return new float3(  _XYZ.Dot( new float3(  3.2406f, -1.5372f, -0.4986f ) ),
								_XYZ.Dot( new float3( -0.9689f,  1.8758f,  0.0415f ) ),
								_XYZ.Dot( new float3(  0.0557f, -0.2040f,  1.0570f ) ) );
		}

		// Conversions CIE XYZ <=> CIE xyY (xy = chromaticities, Y = luminance)
		// Y = Y
		// x = X / ( X + Y + Z )
		// y = Y / ( X + Y + Z )
		float3    XYZ2xyY( float3 _XYZ ) {
			float3    xyY;
			xyY.z  = _XYZ.y;
			xyY.x = _XYZ.x / (_XYZ.x + _XYZ.y + _XYZ.z);
			xyY.y = _XYZ.y / (_XYZ.x + _XYZ.y + _XYZ.z);
			return xyY;
		}

		// X = x * ( Y / y )
		// Y = Y
		// Z = ( 1 - x - y ) * ( Y / y )
		float3    xyY2XYZ( float3 _xyY ) {
			float       Y_over_y = _xyY.z / _xyY.y;
			float3    XYZ;
			XYZ.x = _xyY.x * Y_over_y;
			XYZ.y = _xyY.z;
			XYZ.z = (1.0f - _xyY.x - _xyY.y) * Y_over_y; 
			return XYZ;
		}

		// Conversions sRGB RGB <=> CIE xyY
		// Simply re-using the above
		float3    RGB2xyY( float3 _RGB ) {
			return XYZ2xyY( RGB2XYZ( _RGB ) );
		}

		float3    xyY2RGB( float3 _xyY ) {
			return XYZ2RGB( xyY2XYZ( _xyY ) );
		}

		void		TestBisou() {
			Random	RNG = new Random( 1 );

			for ( int i=0; i < 1000; i++ ) {
				float3	color = new float3( (float) RNG.NextDouble(), (float) RNG.NextDouble(), (float) RNG.NextDouble() );
				float	amount = (float) RNG.NextDouble();
				float3	ciexyY = RGB2xyY( color );
//ciexyY.z = Math.Min( 1.0f, ciexyY.z * (1.0f + amount) );
						ciexyY.z = ciexyY.z * (1.0f + amount);
				float3	brightenedColor_xyY = xyY2RGB( ciexyY );
				float3	brightenedColor = (1.0f + amount) * color;
				bool	different = (brightenedColor - brightenedColor_xyY).Length > 1e-3f;
				Console.WriteLine( "color ({0}, {1}, {2}) => brightned_xyY ({3}, {4}, {5}), brightened ({6}, {7}, {8})" + (different ? " ####DIFFERENT####" : ""), color.x, color.y, color.z, brightenedColor_xyY.x, brightenedColor_xyY.y, brightenedColor_xyY.z, brightenedColor.x, brightenedColor.y, brightenedColor.z );
	
			}

		}

		#endregion

		#region Image Helpers

		public Texture2D	Image2Texture( System.IO.FileInfo _fileName, ImageUtility.COMPONENT_FORMAT _componentFormat ) {
			ImageUtility.ImagesMatrix	images = null;
			if ( _fileName.Extension.ToLower() == ".dds" ) {
				images = ImageUtility.ImageFile.DDSLoadFile( _fileName, _componentFormat );
				codé dans le fichier ?? Putain relou je sais plus... Le save devrait embarquer le DXGI_FORMAT!
			} else {
				ImageUtility.ImageFile	image = new ImageUtility.ImageFile( _fileName );
				if ( image.PixelFormat != ImageUtility.PIXEL_FORMAT.BGRA8 ) {
					ImageUtility.ImageFile	badImage = image;
					image = new ImageUtility.ImageFile();
					image.ConvertFrom( badImage, ImageUtility.PIXEL_FORMAT.BGRA8 );
					badImage.Dispose();
				}

				images = new ImageUtility.ImagesMatrix( new ImageUtility.ImageFile[1,1] { { image } } );
			}
			return new Texture2D( m_Device, images, _componentFormat );
		}

		public void	ComputeSAT( System.IO.FileInfo _FileName, System.IO.FileInfo _TargetFileName ) {

			ImageUtility.ImageFile	mip0_unknownFormat = new ImageUtility.ImageFile( _FileName );
			uint		W = mip0_unknownFormat.Width;
			uint		H = mip0_unknownFormat.Height;

			ImageUtility.ImageFile	mip0 = new ImageUtility.ImageFile();// W, H, ImageUtility.ImageFile.PIXEL_FORMAT.RGBA8, mip0_unknownFormat.ColorProfile );
			mip0.ConvertFrom( mip0_unknownFormat, ImageUtility.PIXEL_FORMAT.BGRA8 );

			mip0_unknownFormat.Dispose();

			// Read as linear floats
			float4[,]	mip0Content = new float4[W,H];
			mip0.ReadPixels( ( uint X, uint Y, ref float4 _color ) => {
				mip0Content[X,Y].Set( ImageUtility.ColorProfile.sRGB2Linear( _color.x ), ImageUtility.ColorProfile.sRGB2Linear( _color.y ), ImageUtility.ColorProfile.sRGB2Linear( _color.z ), 1.0f );
			} );

			ImageUtility.ImagesMatrix	matrix = new ImageUtility.ImagesMatrix();
			matrix.InitTexture2DArray( W, H, 1, 0 );
			matrix[0][0][0] = mip0;	// We provide the first mip for free
			matrix.AllocateImageFiles( mip0.PixelFormat, mip0.ColorProfile );	// Allocate the next mips

			//////////////////////////////////////////////////////////////////////////
			// Build mips
			for ( uint mipLevelIndex=1; mipLevelIndex < matrix[0].MipLevelsCount; mipLevelIndex++ ) {
				ImageUtility.ImagesMatrix.Mips.Mip	mip = matrix[0][mipLevelIndex];

				// Compute gaussian kernel size
				float	mipPixelSizeX = (float) W / mip.Width;	// Size of a mip pixel; in amount of original image pixels (i.e. mip #0)
				float	mipPixelSizeY = (float) H / mip.Height;	// Size of a mip pixel; in amount of original image pixels (i.e. mip #0)
				int		kernelSize = 2 * (int) Math.Pow( 2, mipLevelIndex );
				float	sigma = (float) Math.Sqrt( -kernelSize*kernelSize / (2.0 * Math.Log( 0.01 )) );	// So we have a weight of 0.01 at a Kernel Size distance
				float[]	kernelFactors = new float[1+kernelSize];
				float	sumWeights = 0.0f;
				for ( int i=0; i <= kernelSize; i++ ) {
					kernelFactors[i] = (float) (Math.Exp( -i*i / (2.0 * sigma * sigma)) / Math.Sqrt( 2 * Math.PI * sigma * sigma ) );
					sumWeights += kernelFactors[i];
				}

				// Build high-precision mip first by performing a gaussian blur

				// Perform a horizontal blur first
				float4[,]	mipTemp = new float4[mip.Width, H];
				for ( int Y=0; Y < H; Y++ ) {
					for ( int X=0; X < mip.Width; X++ ) {
						float	centerX = X * mipPixelSizeX + 0.5f * (mipPixelSizeX-1);
						float4	sum = kernelFactors[0] * BilinearSample( mip0Content, centerX, Y );
						for ( int i=1; i <= kernelSize; i++ ) {
							sum += kernelFactors[i] * BilinearSample( mip0Content, centerX - i, Y );
							sum += kernelFactors[i] * BilinearSample( mip0Content, centerX + i, Y );
						}
						mipTemp[X,Y] = sum;
					}
				}

				// Perform vertical blur
				float4[,]	mipFloat = new float4[mip.Width, mip.Height];
				for ( int X=0; X < mip.Width; X++ ) {
					for ( int Y=0; Y < mip.Height; Y++ ) {
						float	centerY = Y * mipPixelSizeY + 0.5f * (mipPixelSizeY-1);
						float4	sum = kernelFactors[0] * BilinearSample( mipTemp, X, centerY );
						for ( int i=1; i <= kernelSize; i++ ) {
							sum += kernelFactors[i] * BilinearSample( mipTemp, X, centerY - i );
							sum += kernelFactors[i] * BilinearSample( mipTemp, X, centerY + i );
						}
						mipFloat[X,Y] = sum;
					}
				}

				// Convert to source sRGB format
				matrix[0][mipLevelIndex][0].WritePixels( ( uint X, uint Y, ref float4 _color ) => {
					_color.x = ImageUtility.ColorProfile.Linear2sRGB( mipFloat[X,Y].x );
					_color.y = ImageUtility.ColorProfile.Linear2sRGB( mipFloat[X,Y].y );
					_color.z = ImageUtility.ColorProfile.Linear2sRGB( mipFloat[X,Y].z );
					_color.w = 1.0f;
				} );
			}

			//////////////////////////////////////////////////////////////////////////
			// Write as DDS
			ImageUtility.ImageFile.DDSSaveFile( matrix, _TargetFileName, ImageUtility.COMPONENT_FORMAT.UNORM_sRGB );
		}

		float4	BilinearSample( float4[,] _Source, float _X, float _Y ) {
			int		X = (int) Math.Floor( _X );
			float	x = _X - X;
			int		Y = (int) Math.Floor( _Y );
			float	y = _Y - Y;
			int		W = _Source.GetLength( 0 );
			int		H = _Source.GetLength( 1 );
			float4	V00 = X >= 0 && Y >= 0 && X < W && Y < H ? _Source[X,Y] : float4.Zero;
			X++;
			float4	V01 = X >= 0 && Y >= 0 && X < W && Y < H ? _Source[X,Y] : float4.Zero;
			Y++;
			float4	V11 = X >= 0 && Y >= 0 && X < W && Y < H ? _Source[X,Y] : float4.Zero;
			X--;
			float4	V10 = X >= 0 && Y >= 0 && X < W && Y < H ? _Source[X,Y] : float4.Zero;

			float4	V0 = (1.0f - x) * V00 + x * V01;
			float4	V1 = (1.0f - x) * V10 + x * V11;
			float4	Result = (1.0f - y) * V0 + y * V1;
			return Result;
		}

		#endregion

		#region Image Helpers (OLD)
/*
// 		public Texture2D	Image2Texture( System.IO.FileInfo _FileName ) {
// 			using ( System.IO.FileStream S = _FileName.OpenRead() )
// 				return Image2Texture( S );
// 		}
// 		public unsafe Texture2D	Image2Texture( System.IO.Stream _Stream ) {
// 			int		W, H;
// 			byte[]	Content = null;
// 			using ( Bitmap B = Bitmap.FromStream( _Stream ) as Bitmap ) {
// 				W = B.Width;
// 				H = B.Height;
// 				Content = new byte[W*H*4];
// 
// 				BitmapData	LockedBitmap = B.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb );
// 				for ( int Y=0; Y < H; Y++ ) {
// 					byte*	pScanline = (byte*) LockedBitmap.Scan0 + Y * LockedBitmap.Stride;
// 					int		Offset = 4*W*Y;
// 					for ( int X=0; X < W; X++, Offset+=4 ) {
// 						Content[Offset+2] = *pScanline++;	// B
// 						Content[Offset+1] = *pScanline++;	// G
// 						Content[Offset+0] = *pScanline++;	// R
// 						Content[Offset+3] = *pScanline++;	// A
// 					}
// 				}
// 				B.UnlockBits( LockedBitmap );
// 			}
// 			return Image2Texture( (uint) W, (uint) H, Content );
// 		}
// 		public Texture2D	Image2Texture( uint _Width, uint _Height, byte[] _Content ) {
// 			using ( PixelsBuffer Buff = new PixelsBuffer( (uint) _Content.Length ) ) {
// 				using ( System.IO.BinaryWriter W = Buff.OpenStreamWrite() )
// 					W.Write( _Content );
// 
// 				return Image2Texture( _Width, _Height, PIXEL_FORMAT.RGBA8_UNORM_sRGB, Buff );
// 			}
// 		}

// 		public Texture2D	Pipi2Texture( System.IO.FileInfo _FileName ) {
// 			using ( System.IO.FileStream S = _FileName.OpenRead() )
// 				using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) ) {
// 
// 					int				MipLevels = R.ReadInt32();
// 					PixelsBuffer[]	Mips = new PixelsBuffer[MipLevels];
// 					int				ImageWidth = 0, ImageHeight = 0;
// 					for ( int MipLevel=0; MipLevel < MipLevels; MipLevel++ ) {
// 						int	W, H;
// 						W = R.ReadInt32();
// 						H = R.ReadInt32();
// 						if ( MipLevel == 0 ) {
// 							ImageWidth = W;
// 							ImageHeight = H;
// 						}
// 
// 						PixelsBuffer	Buff = new PixelsBuffer( (uint) (4 * W * H * 4) );
// 						Mips[MipLevel] = Buff;
// 						using ( System.IO.BinaryWriter Wr = Buff.OpenStreamWrite() )
// 						{
// 							float4	C = new float4();
// 							for ( int Y=0; Y < H; Y++ ) {
// 								for ( int X=0; X < W; X++ ) {
// 									C.x = R.ReadSingle();
// 									C.y = R.ReadSingle();
// 									C.z = R.ReadSingle();
// 									C.w = R.ReadSingle();
// 
// 									Wr.Write( C.x );
// 									Wr.Write( C.y );
// 									Wr.Write( C.z );
// 									Wr.Write( C.w );
// 								}
// 							}
// 						}
// 					}
// 
// 					return Image2Texture( (uint) ImageWidth, (uint) ImageHeight, PIXEL_FORMAT.RGBA32_FLOAT, Mips );
// 				}
// 		}
// 
// 		public Texture3D	Pipu2Texture( System.IO.FileInfo _FileName ) {
// 			using ( System.IO.FileStream S = _FileName.OpenRead() )
// 				using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) ) {
// 
// 					int		SlicesCount = R.ReadInt32();
// 					int		W = R.ReadInt32();
// 					int		H = R.ReadInt32();
// 
// 					PixelsBuffer	Slices = new PixelsBuffer( (uint) (4 * W * H * SlicesCount * 4) );
// 					using ( System.IO.BinaryWriter Wr = Slices.OpenStreamWrite() ) {
// 						for ( int SliceIndex=0; SliceIndex < SlicesCount; SliceIndex++ ) {
// 							float4	C = new float4();
// 							for ( int Y=0; Y < H; Y++ ) {
// 								for ( int X=0; X < W; X++ ) {
// 									C.x = R.ReadSingle();
// 									C.y = R.ReadSingle();
// 									C.z = R.ReadSingle();
// 									C.w = R.ReadSingle();
// 
// 									Wr.Write( C.x );
// 									Wr.Write( C.y );
// 									Wr.Write( C.z );
// 									Wr.Write( C.w );
// 								}
// 							}
// 						}
// 					}
// 
// 					return Image2Texture3D( (uint) W, (uint) H, (uint) SlicesCount, PIXEL_FORMAT.RGBA32_FLOAT, new PixelsBuffer[] { Slices } );
// 				}
// 		}
// 
// 		public Texture2D	PipoImage2Texture( System.IO.FileInfo _FileName ) {
// 			using ( System.IO.FileStream S = _FileName.OpenRead() )
// 				using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) ) {
// 
// 					int	W, H;
// 					W = R.ReadInt32();
// 					H = R.ReadInt32();
// 
// 					PixelsBuffer	Buff = new PixelsBuffer( (uint) (4 * W * H * 4) );
// 					using ( System.IO.BinaryWriter Wr = Buff.OpenStreamWrite() )
// 					{
// 						float4	C = new float4();
// 						for ( int Y=0; Y < H; Y++ ) {
// 							for ( int X=0; X < W; X++ ) {
// 								C.x = R.ReadSingle();
// 								C.y = R.ReadSingle();
// 								C.z = R.ReadSingle();
// 								C.w = R.ReadSingle();
// 
// 								Wr.Write( C.x );
// 								Wr.Write( C.y );
// 								Wr.Write( C.z );
// 								Wr.Write( C.w );
// 							}
// 						}
// 					}
// 
// 					return Image2Texture( (uint) W, (uint) H, PIXEL_FORMAT.RGBA32_FLOAT, Buff );
// 				}
// 		}
		public Texture2D	Image2Texture( uint _Width, uint _Height, PIXEL_FORMAT _Format, PixelsBuffer _Content ) {
			return Image2Texture( _Width, _Height, _Format, new PixelsBuffer[] { _Content } );
		}
		public Texture2D	Image2Texture( uint _Width, uint _Height, PIXEL_FORMAT _Format, PixelsBuffer[] _MipsContent ) {
			return new Texture2D( m_Device, _Width, _Height, 1, (uint) _MipsContent.Length, _Format, false, false, _MipsContent );
		}
		public Texture3D	Image2Texture3D( uint _Width, uint _Height, uint _Depth, PIXEL_FORMAT _Format, PixelsBuffer[] _SlicesContent ) {
			return new Texture3D( m_Device, _Width, _Height, _Depth, 1, _Format, false, false, _SlicesContent );
		}

		/// <summary>
		/// Builds the SAT
		/// </summary>
		/// <param name="_FileName"></param>
		public unsafe void	ComputeSAT( System.IO.FileInfo _FileName, System.IO.FileInfo _TargetFileName ) {
			int		W, H;
			byte[]	Content = null;
			using ( System.IO.FileStream S = _FileName.OpenRead() )
				using ( Bitmap B = Bitmap.FromStream( S ) as Bitmap )
				{
					W = B.Width;
					H = B.Height;
					Content = new byte[W*H*4];

					BitmapData	LockedBitmap = B.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb );
					for ( int Y=0; Y < H; Y++ )
					{
						byte*	pScanline = (byte*) LockedBitmap.Scan0 + Y * LockedBitmap.Stride;
						int		Offset = 4*W*Y;
						for ( int X=0; X < W; X++, Offset+=4 )
						{
							Content[Offset+2] = *pScanline++;	// B
							Content[Offset+1] = *pScanline++;	// G
							Content[Offset+0] = *pScanline++;	// R
							Content[Offset+3] = *pScanline++;	// A
						}
					}
					B.UnlockBits( LockedBitmap );
				}

			// Build the float4 image
			float4[,]	Image = new float4[W,H];
			for ( int Y=0; Y < H; Y++ ) {
				for ( int X=0; X < W; X++ ) {
					Image[X,Y] = new float4( Content[4*(W*Y+X)+0] / 255.0f, Content[4*(W*Y+X)+1] / 255.0f, Content[4*(W*Y+X)+2] / 255.0f, 0.0f );

					// Linearize from gamma space
					Image[X,Y].x = (float) Math.Pow( Image[X,Y].x, 2.2 );
					Image[X,Y].y = (float) Math.Pow( Image[X,Y].y, 2.2 );
					Image[X,Y].z = (float) Math.Pow( Image[X,Y].z, 2.2 );
				}
			}

			//////////////////////////////////////////////////////////////////////////
			// Build mips and save as a simple format
			{
				int	MaxSize = Math.Max( W, H );
				int	MipsCount = (int) (Math.Ceiling( Math.Log( MaxSize+1 ) / Math.Log( 2 ) ));
				float4[][,]	Mips = new float4[MipsCount][,];
				Mips[0] = Image;

				int	TargetWidth = W;
				int	TargetHeight = H;
				for ( int MipLevel=1; MipLevel < Mips.Length; MipLevel++ ) {
					TargetWidth = Math.Max( 1, TargetWidth >> 1 );
					TargetHeight = Math.Max( 1, TargetHeight >> 1 );

					float	MipPixelSizeX = W / TargetWidth;	// Size of a mip pixel; in amount of original image pixels (i.e. mip #0)
					float	MipPixelSizeY = H / TargetHeight;	// Size of a mip pixel; in amount of original image pixels (i.e. mip #0)
					int		KernelSize = 2 * (int) Math.Pow( 2, MipLevel );
					float	Sigma = (float) Math.Sqrt( -KernelSize*KernelSize / (2.0 * Math.Log( 0.01 )) );	// So we have a weight of 0.01 at a Kernel Size distance
					float[]	KernelFactors = new float[1+KernelSize];
					float	SumWeights = 0.0f;
					for ( int i=0; i <= KernelSize; i++ ) {
						KernelFactors[i] = (float) (Math.Exp( -i*i / (2.0 * Sigma * Sigma)) / Math.Sqrt( 2 * Math.PI * Sigma * Sigma ) );
						SumWeights += KernelFactors[i];
					}

					// Perform a horizontal blur first
					float4[,]	Source = Image;
					float4[,]	Target = new float4[TargetWidth,H];
					for ( int Y=0; Y < H; Y++ ) {
						for ( int X=0; X < TargetWidth; X++ ) {
							float	CenterX = X * MipPixelSizeX + 0.5f * (MipPixelSizeX-1);
							float4	Sum = KernelFactors[0] * BilinearSample( Source, CenterX, Y );
							for ( int i=1; i <= KernelSize; i++ ) {
								Sum += KernelFactors[i] * BilinearSample( Image, CenterX - i, Y );
								Sum += KernelFactors[i] * BilinearSample( Image, CenterX + i, Y );
							}
							Target[X,Y] = Sum;
						}
					}

					// Perform vertical blur
					Source = Target;
					Mips[MipLevel] = new float4[TargetWidth,TargetHeight];
					Target = Mips[MipLevel];
					for ( int X=0; X < TargetWidth; X++ ) {
						for ( int Y=0; Y < TargetHeight; Y++ ) {
							float	CenterY = Y * MipPixelSizeY + 0.5f * (MipPixelSizeY-1);
							float4	Sum = KernelFactors[0] * BilinearSample( Source, X, CenterY );
							for ( int i=1; i <= KernelSize; i++ ) {
								Sum += KernelFactors[i] * BilinearSample( Source, X, CenterY - i );
								Sum += KernelFactors[i] * BilinearSample( Source, X, CenterY + i );
							}
							Target[X,Y] = Sum;
						}
					}
				}

				string	Pipi = _TargetFileName.FullName;
				Pipi = System.IO.Path.GetFileNameWithoutExtension( Pipi ) + ".pipi";
				System.IO.FileInfo	SimpleTargetFileName2 = new System.IO.FileInfo(  Pipi );
				using ( System.IO.FileStream S = SimpleTargetFileName2.OpenWrite() )
					using ( System.IO.BinaryWriter Wr = new System.IO.BinaryWriter( S ) ) {
						Wr.Write( Mips.Length );
						for ( int MipLevel=0; MipLevel < Mips.Length; MipLevel++ ) {
							float4[,]	Mip = Mips[MipLevel];

							int	MipWidth = Mip.GetLength( 0 );
							int	MipHeight = Mip.GetLength( 1 );
							Wr.Write( MipWidth );
							Wr.Write( MipHeight );

							for ( int Y=0; Y < MipHeight; Y++ ) {
								for ( int X=0; X < MipWidth; X++ ) {
									Wr.Write( Mip[X,Y].x );
									Wr.Write( Mip[X,Y].y );
									Wr.Write( Mip[X,Y].z );
									Wr.Write( Mip[X,Y].w );
								}
							}
						}
					}
			}


// 			//////////////////////////////////////////////////////////////////////////
// 			// Build "3D mips" and save as a simple format
// 			{
// 				int	MaxSize = Math.Max( W, H );
// 				int	MipsCount = (int) (Math.Ceiling( Math.Log( MaxSize+1 ) / Math.Log( 2 ) ));
// 
// 				// 1] Build vertical mips
// 				float4[][,]	VerticalMips = new float4[MipsCount][,];
// 				VerticalMips[0] = Image;
// 
// 				int	TargetHeight = H;
// 				for ( int MipLevel=1; MipLevel < MipsCount; MipLevel++ ) {
// 					int	SourceHeight = TargetHeight;
// 
// 					int	BorderSize = (int) Math.Pow( 2, MipLevel-1 );
// 					TargetHeight = Math.Max( 1, H - 2*BorderSize );
// 
// 					float4[,]	SourceMip = VerticalMips[MipLevel-1];
// 					float4[,]	TargetMip = new float4[W,TargetHeight];
// 					VerticalMips[MipLevel] = TargetMip;
// 					for ( int Y=0; Y < TargetHeight; Y++ ) {
// 						float	fY = (float) (Y+0.5f) * SourceHeight / TargetHeight;
// 						for ( int X=0; X < W; X++ ) {
// 							TargetMip[X,Y] = BilinearSample( SourceMip, X, fY );
// 						}
// 					}
// 				}
// 
// 
// //MipsCount = 6;
// 
// 				// 2] Build smoothed slices
// 				float4[][,]	Slices = new float4[MipsCount][,];
// 				Slices[0] = Image;
// 
// 				for ( int MipLevel=1; MipLevel < Slices.Length; MipLevel++ ) {
// 
// 					int		BorderSize = (int) Math.Pow( 2, MipLevel-1 );		// Each new "mip" has a border twice the size of the previous level
// 
// 					int		InsetWidth = Math.Max( 1, W - 2 * BorderSize );		// The inset image is now reduced to account for borders
// 					int		InsetHeight = Math.Max( 1, H - 2 * BorderSize );
// 
// 					int		WidthWithBorders = W + 2 * BorderSize;				// The larger image with borders that will be stored in the specific mip
// 					int		HeightWithBorders = H + 2 * BorderSize;
// 
// 					int		Y0 = BorderSize;
// 					int		Y1 = H - BorderSize;
// 
// 					// Build gaussian weights
// 					int		KernelSize = 2 * (int) BorderSize;
// 					float	Sigma = (float) Math.Sqrt( -KernelSize*KernelSize / (2.0 * Math.Log( 0.01 )) );	// So we have a weight of 0.01 at a Kernel Size distance
// 					float[]	KernelFactors = new float[1+KernelSize];
// 					float	SumWeights = 0.0f;
// 					for ( int i=0; i <= KernelSize; i++ ) {
// 						KernelFactors[i] = (float) (Math.Exp( -i*i / (2.0 * Sigma * Sigma)) / Math.Sqrt( 2 * Math.PI * Sigma * Sigma ) );
// 						SumWeights += KernelFactors[i];
// 					}
// 
// 					// Perform a horizontal blur first
// 					float4[,]	Source = VerticalMips[MipLevel];
// 					float4[,]	Target = new float4[W,H];
// 					for ( int Y=0; Y < H; Y++ ) {
// 						if ( Y < Y0 || Y >= Y1 ) {
// 							// In the borderlands
// 							for ( int X=0; X < W; X++ ) {
// 								Target[X,Y] = float4.Zero;
// 							}
// 							continue;
// 						}
// 
// 						float	fY = (float) (Y - Y0) * H / InsetHeight;
// 						for ( int X=0; X < W; X++ ) {
// 							float			CenterX = 0.5f * W + ((float) (X+0.5f) / W - 0.5f) * WidthWithBorders;
// 							float4	Sum = KernelFactors[0] * BilinearSample( Source, CenterX, fY );
// 							for ( int i=1; i <= KernelSize; i++ ) {
// 								Sum += KernelFactors[i] * BilinearSample( Image, CenterX - i, fY );
// 								Sum += KernelFactors[i] * BilinearSample( Image, CenterX + i, fY );
// 							}
// 							Target[X,Y] = Sum;
// 						}
// 					}
// 
// 					// Perform vertical blur
// 					Source = Target;
// 					Slices[MipLevel] = new float4[W,H];
// 					Target = Slices[MipLevel];
// 					for ( int X=0; X < W; X++ ) {
// 						for ( int Y=0; Y < H; Y++ ) {
// 							float			CenterY = 0.5f * H + ((float) (Y+0.5f) / H - 0.5f) * HeightWithBorders;
// 							float4	Sum = KernelFactors[0] * BilinearSample( Source, X, CenterY );
// 							for ( int i=1; i <= KernelSize; i++ ) {
// 								Sum += KernelFactors[i] * BilinearSample( Source, X, CenterY - i );
// 								Sum += KernelFactors[i] * BilinearSample( Source, X, CenterY + i );
// 							}
// 							Target[X,Y] = Sum;
// 						}
// 					}
// 				}
// 
// 
// 				string	Pipu = _TargetFileName.FullName;
// 				Pipu = System.IO.Path.GetFileNameWithoutExtension( Pipu ) + ".pipu";
// 				System.IO.FileInfo	SimpleTargetFileName2 = new System.IO.FileInfo(  Pipu );
// 				using ( System.IO.FileStream S = SimpleTargetFileName2.OpenWrite() )
// 					using ( System.IO.BinaryWriter Wr = new System.IO.BinaryWriter( S ) ) {
// 						Wr.Write( Slices.Length );
// 						Wr.Write( W );
// 						Wr.Write( H );
// 
// 						for ( int MipLevel=0; MipLevel < Slices.Length; MipLevel++ ) {
// 							float4[,]	Mip = Slices[MipLevel];
// 							for ( int Y=0; Y < H; Y++ ) {
// 								for ( int X=0; X < W; X++ ) {
// 									Wr.Write( Mip[X,Y].x );
// 									Wr.Write( Mip[X,Y].y );
// 									Wr.Write( Mip[X,Y].z );
// 									Wr.Write( Mip[X,Y].w );
// 								}
// 							}
// 						}
// 					}
// 			}

			//////////////////////////////////////////////////////////////////////////
			// Build the SAT
			for ( int Y=0; Y < H; Y++ ) {
				for ( int X=1; X < W; X++ ) {
					Image[X,Y] += Image[X-1,Y];	// Accumulate along X
				}
			}

			for ( int X=0; X < W; X++ ) {
				for ( int Y=1; Y < H; Y++ ) {
					Image[X,Y] += Image[X,Y-1];	// Accumulate along Y
				}
			}

//			DirectXTexManaged.TextureCreator.CreateRGBA16FFile( _TargetFileName.FullName, Image );
throw new Exception( "Deprecated!" );

			// Save as a simple format
			string	Pipo = _TargetFileName.FullName;
			Pipo = System.IO.Path.GetFileNameWithoutExtension( Pipo ) + ".pipo";
			System.IO.FileInfo	SimpleTargetFileName = new System.IO.FileInfo(  Pipo );
			using ( System.IO.FileStream S = SimpleTargetFileName.OpenWrite() )
				using ( System.IO.BinaryWriter Wr = new System.IO.BinaryWriter( S ) ) {

					Wr.Write( W );
					Wr.Write( H );
					for ( int Y=0; Y < H; Y++ ) {
						for ( int X=0; X < W; X++ ) {
							Wr.Write( Image[X,Y].x );
							Wr.Write( Image[X,Y].y );
							Wr.Write( Image[X,Y].z );
							Wr.Write( Image[X,Y].w );
						}
					}
				}
		} 

		float4	BilinearSample( float4[,] _Source, float _X, float _Y ) {
			int		X = (int) Math.Floor( _X );
			float	x = _X - X;
			int		Y = (int) Math.Floor( _Y );
			float	y = _Y - Y;
			int		W = _Source.GetLength( 0 );
			int		H = _Source.GetLength( 1 );
			float4	V00 = X >= 0 && Y >= 0 && X < W && Y < H ? _Source[X,Y] : float4.Zero;
			X++;
			float4	V01 = X >= 0 && Y >= 0 && X < W && Y < H ? _Source[X,Y] : float4.Zero;
			Y++;
			float4	V11 = X >= 0 && Y >= 0 && X < W && Y < H ? _Source[X,Y] : float4.Zero;
			X--;
			float4	V10 = X >= 0 && Y >= 0 && X < W && Y < H ? _Source[X,Y] : float4.Zero;

			float4	V0 = (1.0f - x) * V00 + x * V01;
			float4	V1 = (1.0f - x) * V10 + x * V11;
			float4	Result = (1.0f - y) * V0 + y * V1;
			return Result;
		}
//*/
		#endregion

		#region BRDF Integration

#if USE_COMPUTE_SHADER_FOR_BRDF_INTEGRATION

		void	AnalyticalBRDFIntegral_Order2( float _NdotV, float _roughness, out float _F0term, out float _ambientTerm ) {
			float	x = _NdotV;
			float	y = _roughness;

			float	x2 = x*x;
			float	x3 = x2*x;
			float	y2 = y*y;
			float	y3 = y2*y;

			_F0term = Math.Min( 1.0f, 3.2341316749472577f * x - 2.373272450906272f * x2 + 1.0643192106211778f * y - 4.840084966583103f * x * y + 4.174411960036787f * x2 * y - 0.38184478124695787f * y2 + 1.3304731349921781f * x * y2 - 1.779774293647103f * x2 * y2 );
			_ambientTerm = Math.Max( 0.0f, 1 - 3.10644824089575f * x + 2.2442720252458033f * x2 - 1.5631045930597178f * y + 5.286682741799993f * x * y - 4.013072795013668f * x2 * y + 0.8078765587702772f * y2 - 2.9710821881549427f * x * y2 + 2.3561138954372183f * x2 * y2 );
		}

		void	AnalyticalBRDFIntegral_Order3( float _NdotV, float _roughness, out float _F0term, out float _ambientTerm ) {
			float	x = _NdotV;
			float	y = _roughness;

			float	x2 = x*x;
			float	x3 = x2*x;
			float	y2 = y*y;
			float	y3 = y2*y;

// 			_F0term = Math.Min( 1.0f, 4.468499668510825f * x - 6.559058152749997f * x2 + 3.100924246015641f * x3
// 									+ 0.3060480025629177f * y - 3.9000236894490814f * x * y + 9.815053796171387f * x2 * y - 5.871672977702834f * x3 * y
// 									+ 1.524434529668645f * y2 - 6.411883095596227f * x * y2 + 1.8246250809966547f * x2 * y2 + 1.524434529668645f * x3 * y2
// 									- 1.179575011394126f * y3 + 5.788688558693789f * x * y3 - 5.3758421081258785f * x2 * y3 + 1.4190618001278472f * x3 * y3 );
// 			_ambientTerm = Math.Max( 0.0f, 1 - 4.204097008385143f * x + 5.7767751596221775f * x2 - 2.587651085262007f * x3
// 											- 1.0571539479331853f * y + 5.054812966013966f * x * y - 7.000129703739535f * x2 * y + 2.942312835851425f * x3 * y
// 											- 0.10991284572137273f * y2 - 0.8366676687929925f * x * y2 + 1.2399422035451517f * x2 * y2 - 0.10991284572137273f * x3 * y2
// 											+ 0.4450309398884565f * y3 - 1.296796034045326f * x * y3 + 1.9692132758430827f * x2 * y3 - 1.2457869679372504f * x3 * y3 );


			_F0term = Math.Min( 1.0f, 4.483120094766527f * x - 6.607418162702069f * x2 + 3.1369146126741314f * x3
									+ 0.31965568814498413f * y - 4.171560969733041f * x * y + 10.579256413997497f * x2 * y - 6.407181973132758f * x3 * y
									+ 1.4812238752410947f * y2 - 5.679180293057831f * x * y2 - 0.17354724317745246f * x2 * y2 + 2.9060079735435256f * x3 * y2
									- 1.1483095194551398f * y3 + 5.289774653050149f * x * y3 - 4.03337882134342f * x2 * y3 + 0.496313678299834f * x3 * y3 );
			_ambientTerm = Math.Max( 0.0f, 1 - 4.268373376557555f * x + 5.9893822170455895f * x2 -  2.745876982205202f * x3
											- 1.1169779635363377f * y + 6.2485832361009965f * x * y - 10.359824418695426f * x2 * y + 5.296592577623529f * x3 * y
											+ 0.08005589283505338f * y2 - 4.057878478465465f * x * y2 + 10.024587584890986f * x2 * y2 - 6.183779761950312f * x3 * y2
											+ 0.3075771997708405f * y3 + 0.8965992421669212f * x * y3 - 3.9327120832393003f * x2 * y3 + 2.8109277250321485f * x3 * y3 );
		}

		unsafe void	ComputeBRDFIntegral( System.IO.FileInfo _TableFileName, uint _TableSize ) {

//			ComputeShader	CS = new ComputeShader( m_Device, new System.IO.FileInfo( "Shaders/ComputeBRDFIntegral.hlsl" ), "CS", new ShaderMacro[0] );
			ComputeShader	CS = new ComputeShader( m_Device, new System.IO.FileInfo( "Shaders/ComputeBRDFIntegral.hlsl" ), "CS", new ShaderMacro[] { new ShaderMacro( "IMPORTANCE_SAMPLING", "1" ) } );

			Texture2D		TexTable = new Texture2D( m_Device, _TableSize, _TableSize, 1, 1, ImageUtility.PIXEL_FORMAT.RG32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, null );
			TexTable.SetCSUAV( 0 );
			CS.Use();
			CS.Dispatch( _TableSize >> 4, _TableSize >> 4, 1 );
			CS.Dispose();

 
			Texture2D		TexTableStaging = new Texture2D( m_Device, _TableSize, _TableSize, 1, 1, ImageUtility.PIXEL_FORMAT.RG32F, ImageUtility.COMPONENT_FORMAT.AUTO, true, false, null );
			TexTableStaging.CopyFrom( TexTable );
			TexTable.Dispose();

			// Save it to disk as a DDS file
			string	DDSFileName = System.IO.Path.GetFileNameWithoutExtension( _TableFileName.FullName ) + ".dds";
			using ( ImageUtility.ImagesMatrix M = TexTableStaging.AsImagesMatrix )
				ImageUtility.ImageFile.DDSSaveFile( M, new System.IO.FileInfo( DDSFileName ), ImageUtility.COMPONENT_FORMAT.AUTO );

			// Write tables
			float2	Temp = new float2();
			float	F0_analytical;
			float	ambient_analytical;
			float	maxAbsoluteError_F0 = 0.0f;
			float	maxRelativeError_F0 = 0.0f;
			float	avgAbsoluteError_F0 = 0.0f;
			float	avgRelativeError_F0 = 0.0f;
			float	maxAbsoluteError_ambient = 0.0f;
			float	maxRelativeError_ambient = 0.0f;
			float	avgAbsoluteError_ambient = 0.0f;
			float	avgRelativeError_ambient = 0.0f;

			float2[]	Integral_SpecularReflectance = new float2[_TableSize];

			PixelsBuffer Buff = TexTableStaging.MapRead( 0, 0 );
			using ( System.IO.BinaryReader R = Buff.OpenStreamRead() )
				using ( System.IO.FileStream S = _TableFileName.Create() )
					using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) )
						for ( int Y=0; Y < _TableSize; Y++ ) {
							float2	SumSpecularlyReflected = float2.Zero;
							for ( int X=0; X < _TableSize; X++ ) {
								Temp.x = R.ReadSingle();
								Temp.y = R.ReadSingle();
								W.Write( Temp.x );
								W.Write( Temp.y );

								SumSpecularlyReflected.x += Temp.x;
								SumSpecularlyReflected.y += Temp.y;

								// Check analytical solution
								float	NdotV = (float) X / (_TableSize-1);
								float	roughness = (float) Y / (_TableSize-1);
								AnalyticalBRDFIntegral_Order3( NdotV, roughness, out F0_analytical, out ambient_analytical );
//								AnalyticalBRDFIntegral_Order2( NdotV, roughness, out F0_analytical, out ambient_analytical );

								float	absoluteError_F0 = Math.Abs( F0_analytical - Temp.x );
								float	relativeError_F0 = F0_analytical > 1e-6f ? Temp.x / F0_analytical : 0.0f;
								maxAbsoluteError_F0 = Math.Max( maxAbsoluteError_F0, absoluteError_F0 );
								maxRelativeError_F0 = Math.Max( maxRelativeError_F0, relativeError_F0 );
								avgAbsoluteError_F0 += absoluteError_F0;
								avgRelativeError_F0 += relativeError_F0;

								float	absoluteError_ambient = Math.Abs( ambient_analytical - Temp.y );
								float	relativeError_ambient = ambient_analytical > 1e-6f ? Temp.x / ambient_analytical : 0.0f;
								maxAbsoluteError_ambient = Math.Max( maxAbsoluteError_ambient, absoluteError_ambient );
								maxRelativeError_ambient = Math.Max( maxRelativeError_ambient, relativeError_ambient );
								avgAbsoluteError_ambient += absoluteError_ambient;
								avgRelativeError_ambient += relativeError_ambient;
							}

							// Normalize and store "not specularly reflected" light
							SumSpecularlyReflected = SumSpecularlyReflected / _TableSize;

							float	sum_dielectric = 1.0f - (0.04f * SumSpecularlyReflected.x + SumSpecularlyReflected.y);
							float	sum_metallic = 1.0f - (SumSpecularlyReflected.x + SumSpecularlyReflected.y);

							Integral_SpecularReflectance[Y] = SumSpecularlyReflected;
						}
			TexTableStaging.UnMap( Buff );

			avgAbsoluteError_F0 /= _TableSize*_TableSize;
			avgRelativeError_F0 /= _TableSize*_TableSize;
			avgAbsoluteError_ambient /= _TableSize*_TableSize;
			avgRelativeError_ambient /= _TableSize*_TableSize;

			string	TotalSpecularReflectionTableFileName = System.IO.Path.GetFileNameWithoutExtension( _TableFileName.FullName ) + ".table";
			using ( System.IO.FileStream S = new System.IO.FileInfo( TotalSpecularReflectionTableFileName ).Create() )
				using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) )
					for ( int i=0; i < _TableSize; i++ ) {
						W.Write( Integral_SpecularReflectance[i].x );
						W.Write( Integral_SpecularReflectance[i].y );
					}
		}

#else

		void	ComputeBRDFIntegral( System.IO.FileInfo _TableFileName0, System.IO.FileInfo _TableFileName1, int _TableSize ) {

			const int		SAMPLES_COUNT_THETA = 128;
			const int		SAMPLES_COUNT_PHI = 2*SAMPLES_COUNT_THETA;

			const double	dTheta = 0.5 * Math.PI / SAMPLES_COUNT_THETA;
			const double	dPhi = 2.0 * Math.PI / SAMPLES_COUNT_PHI;

			double[,]		Table0 = new double[_TableSize,_TableSize];
			double[,]		Table1 = new double[_TableSize,_TableSize];


// float	Theta = 0.5 * _UV.x * PI;
// float3	ToLight = float3( sin( Theta ), 0, cos( Theta ) );
// float3	ToView = float3( -sin( Theta ), 0, cos( Theta ) );
// 
// float	Albedo = 0.0;
// const int	THETA_COUNT = 64; // * $alwaysOne; // warning X4008: floating point division by zero
// const float	dTheta = HALFPI / THETA_COUNT;
// const float	dPhi = TWOPI / THETA_COUNT;
// for ( int i=0; i < THETA_COUNT; i++ )
// {
// 	Theta = HALFPI * (0.5 + i) / THETA_COUNT;
// 	for ( int j=0; j < THETA_COUNT; j++ )
// 	{
// 		float	Phi = PI * j / THETA_COUNT;
// 
// 		ToView = float3( sin( Theta ) * cos( Phi ), sin( Theta ) * sin( Phi ), cos( Theta ) );
// 
// 		float3	Half = normalize( ToLight + ToView );
// 
// 		// "True" and expensive evaluation of the Ward BRDF
// 		float	alpha = Roughness;
// 
// 		float	CosDelta = Half.z;	// dot( Half, _wsNormal );
// 		float	delta = acos( CosDelta );
// 		float	CosThetaL = ToLight.z;
// 		float	SinThetaL = sqrt( 1.0 - CosThetaL*CosThetaL );
// 		float	PhiL = atan2( ToLight.y, ToLight.x );
// 		float	CosThetaV = ToView.z;
// 		float	SinThetaV = sqrt( 1.0 - CosThetaV*CosThetaV );
// 		float	PhiV = atan2( ToView.y, ToView.x );
// 
// 		float	BRDF = 1.0 / square(alpha) * exp( -square( tan( delta ) / alpha ) ) * 2.0 * (1.0 + CosThetaL*CosThetaV + SinThetaL*SinThetaV*cos( PhiV - PhiL )) / pow4( CosThetaL + CosThetaV );
// 
// 		Albedo += BRDF * cos( Theta ) * sin( Theta ) * dTheta * dPhi;
// 	}
// }
// 
// Albedo *= INVPI;	// Since we forgot that in the main loop



			float3	View = new float3();
			float3	Light = new float3();
			for ( int Y=0; Y < _TableSize; Y++ ) {
				float	Roughness = Math.Max( 0.01f, (float) Y / (_TableSize-1) );
				float	r2 = Roughness*Roughness;

				for ( int X=0; X < _TableSize; X++ ) {
//					double	CosThetaV = (double) (1+X) / _TableSize;
//					double	SinThetaV = Math.Sqrt( 1.0 - CosThetaV*CosThetaV );

					double	ThetaV = 0.5 * Math.PI * X / _TableSize;
					double	CosThetaV = Math.Cos( ThetaV );
					double	SinThetaV = Math.Sin( ThetaV );
					View.x = (float) SinThetaV;
					View.y = (float) CosThetaV;
					View.z = 0.0f;

					double	SumA = 0.0;
					double	SumB = 0.0;
					for ( int Theta=0; Theta < SAMPLES_COUNT_THETA; Theta++ ) {
						double	fTheta = 0.5 * Math.PI * (0.5 + Theta) / SAMPLES_COUNT_THETA;
						double	CosThetaL = Math.Cos( fTheta );
						double	SinThetaL = Math.Sin( fTheta );

						// Compute solid angle
						double	SolidAngle = SinThetaL * dTheta * dPhi;
						double	ProjectedSolidAngle = CosThetaL * SolidAngle;	// (N.L) sin(Theta).dTheta.dPhi

						for ( int Phi=0; Phi < SAMPLES_COUNT_PHI; Phi++ ) {
							double	fPhi = Math.PI * Phi / SAMPLES_COUNT_PHI;
							double	CosPhiLight = Math.Cos( fPhi );
							double	SinPhiLight = Math.Sin( fPhi );

							Light.x = (float) (SinPhiLight * SinThetaL);
							Light.y = (float) CosThetaL;
							Light.z = (float) (CosPhiLight * SinThetaL);

// 							// Transform into "reflected-view-centered space"
// 							Light = Light.x * OrthoX + Light.y * ReflectedView + Light.z * OrthoZ;
// 							if ( Light.y < 0.0f )
// 								continue;
// 							ProjectedSolidAngle = dPhi * dTheta * Light.y * Math.Sqrt( 1.0 - Light.y*Light.y );


 							float3	H_unorm = View + Light;
 							float3	H = H_unorm.Normalized;

// 							// Compute normal distribution function
// 							float	H_unorm_dot_N = H_unorm.y;
// 							float	H_unorm_dot_N2 = H_unorm_dot_N * H_unorm_dot_N;
// 							float	H_unorm_dot_N4 = H_unorm_dot_N2 * H_unorm_dot_N2;
// 
// 							double	BRDF = Math.Exp( -(H_unorm.x*H_unorm.x + H_unorm.z*H_unorm.z) / (r2 * H_unorm_dot_N2) ) * H_unorm.Dot( H_unorm ) / H_unorm_dot_N4;

							// Expensive Ward
							double	PhiL = Math.Atan2( Light.z, Light.x );
							double	PhiV = Math.Atan2( View.z, View.x );
							double	tanDelta = Math.Tan( Math.Acos( H.y ) );
							double	BRDF = Math.Exp( -tanDelta*tanDelta / r2 ) * 2.0 * (1.0 + CosThetaL*CosThetaV + SinThetaL*SinThetaV*Math.Cos( PhiV - PhiL )) / Math.Pow( CosThetaL + CosThetaV, 4.0 );


// Try with Unreal's GGX & Smith G term to see if we get the same thing
// double	alpha = r2;
// double	alpha2 = alpha*alpha;
// double	D = alpha2 / (Math.PI * Math.Pow( HoN2*(alpha2 - 1.0) + 1.0, 2.0 ));
// 
// double	k = (Roughness + 1)*(Roughness + 1) / 8.0;
// 
// 		HoN = H_norm.y;
// double	HoL = H_norm.Dot( Light );
// double	HoV = H_norm.Dot( View );
// double	Gl = HoL / (HoL * (1-k) + k);
// double	Gv = HoV / (HoV * (1-k) + k);
// double	G = Gl * Gv;
// 
// double	BRDF = D * G / (4.0 * Light.y * View.y);
// //double	BRDF = D * G * H_norm.Dot( View ) / Math.Max( 1e-6, HoN * View.y);


// Expensive Ward with angles
// double	PhiL = Math.Atan2( Light.z, Light.x );
// double	PhiV = 0.0;//Math.Atan2( View.z, View.x );
// double	tanDelta = Math.Tan( Math.Acos( H_norm.y ) );
// double	BRDF = Math.Exp( -tanDelta*tanDelta / r2 ) * 2.0 * (1.0 + CosThetaLight*CosThetaView + SinThetaLight*SinThetaView*Math.Cos( PhiV - PhiL )) / Math.Pow( CosThetaLight + CosThetaView, 4.0 );
// 
// SumA += BRDF * ProjectedSolidAngle;
// SumB += BRDF * ProjectedSolidAngle;

							// Compute Fresnel terms
							double	VoH = View.x * H.x + View.y * H.y;
							double	Schlick = 1.0 - VoH;
							double	Schlick5 = Schlick * Schlick;
									Schlick5 *= Schlick5 * Schlick;

							double	FresnelA = 1.0 - Schlick5;
							double	FresnelB = Schlick5;

//FresnelA = FresnelB = 1.0;

							SumA += FresnelA * BRDF * ProjectedSolidAngle;
							SumB += FresnelB * BRDF * ProjectedSolidAngle;
						}
					}

					SumA /= Math.PI * r2;
					SumB /= Math.PI * r2;

// 					// For few samples, the sum goes over 1 because we have poor solid angle sampling resolution...
// 					SumA = Math.Min( 1.0, SumA );
// 					SumB = Math.Min( 1.0, SumB );

					Table0[X,Y] = SumA;
					Table1[X,Y] = SumB;
				}
			}

			// Write table 0
			using ( System.IO.FileStream S = _TableFileName0.Create() )
				using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) )
					for ( int Y=0; Y < _TableSize; Y++ ) {
						for ( int X=0; X < _TableSize; X++ )
							W.Write( Table0[X,Y] );
						}

			// Write table 1
			using ( System.IO.FileStream S = _TableFileName1.Create() )
				using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) )
					for ( int Y=0; Y < _TableSize; Y++ ) {
						for ( int X=0; X < _TableSize; X++ )
							W.Write( Table1[X,Y] );
						}
		}

		double	GSmith( double Roughness, double ndotv, double ndotl )
		{
//			double	m2   = Roughness * Roughness;
			double	m2   = (Roughness + 1)*(Roughness + 1) / 8.0;
			double	visV = ndotv + Math.Sqrt( ndotv * ( ndotv - ndotv * m2 ) + m2 );
			double	visL = ndotl + Math.Sqrt( ndotl * ( ndotl - ndotl * m2 ) + m2 );

			return 1.0f / ( visV * visL );
		}

		void	ComputeBRDFIntegralImportanceSampling( System.IO.FileInfo _TableFileName0, System.IO.FileInfo _TableFileName1, int _TableSize ) {

			const int		SAMPLES_COUNT = 1024;

			double[,]		Table0 = new double[_TableSize,_TableSize];
			double[,]		Table1 = new double[_TableSize,_TableSize];

			WMath.Hammersley	QRNG = new WMath.Hammersley();
			double[,]			Sequence = QRNG.BuildSequence( SAMPLES_COUNT, 2 );

			float3	View = new float3();
			float3	Light = new float3();
			float3	Half = new float3();
			for ( int Y=0; Y < _TableSize; Y++ ) {
				double	Roughness = Math.Max( 0.01f, (float) Y / (_TableSize-1) );


//Roughness = Math.Pow( 1.0 - Roughness, 4.0 );


				double	r2 = Roughness*Roughness;

				for ( int X=0; X < _TableSize; X++ ) {
					float	CosThetaView = (float) (1+X) / _TableSize;
					float	SinThetaView = (float) Math.Sqrt( 1.0f - CosThetaView*CosThetaView );
					View.x = SinThetaView;
					View.y = CosThetaView;
					View.z = 0.0f;

					double	SumA = 0.0;
					double	SumB = 0.0;
					for ( int SampleIndex=0; SampleIndex < SAMPLES_COUNT; SampleIndex++ ) {

						double	X0 = Sequence[SampleIndex,0];
						double	X1 = Sequence[SampleIndex,1];

						double	PhiH = 2.0 * Math.PI * X0;

						// WARD
						double	ThetaH = Math.Atan( -r2 * Math.Log( 1.0 - X1 ) );
						double	CosThetaH = Math.Cos( ThetaH );
						double	SinThetaH = Math.Sin( ThetaH );

// 						// GGX
// 						double	a = r2;
// 						double	CosThetaH = Math.Sqrt( (1.0 - X1) / (1.0 + (a*a - 1.0) * X1 ) );
// 						double	SinThetaH = Math.Sqrt( 1.0f - CosThetaH * CosThetaH );


						double	CosPhiH = Math.Cos( PhiH );
						double	SinPhiH = Math.Sin( PhiH );

						Half.x = (float) (SinPhiH * SinThetaH);
						Half.y = (float) CosThetaH;
						Half.z = (float) (CosPhiH * SinThetaH);

 						Light = 2.0f * View.Dot( Half ) * Half - View;	// Light is on the other size of the Half vector...


// Intuitively, we should have the same result if we sampled around the reflected view direction
// 						float3	ReflectedView = 2.0f * View.Dot( float3.UnitY ) * float3.UnitY - View;
// 						float3	OrthoY = ReflectedView.Cross( float3.UnitZ ).Normalized;
// 						float3	OrthoX = float3.UnitZ;
//  						Light = Half.x * OrthoX + Half.y * ReflectedView + Half.z * OrthoY;
// 
// 						Half = (View + Light).Normalized;


						if ( Light.y <= 0 )
							continue;

						double	HoN = Half.y;
						double	HoN2 = HoN*HoN;
						double	HoV = Half.Dot( View );
//						float	HoV = Half.x * View.x + Half.y * View.y;	// We know that Z=0 here...
						double	HoL = Half.Dot( Light );
						double	NoL = Light.y;
						double	NoV = View.y;

 						// Apply sampling weight for correct distribution
 						double	SampleWeight = 2.0 / (1.0 + View.y / Light.y);
 						double	BRDF = SampleWeight;





// Try with Unreal's GGX & Smith G term to see if we get the same thing
// 
// 	// GGX NDF
// // double	alpha = r2;
// // double	alpha2 = alpha*alpha;
// // double	D = alpha2 / (Math.PI * Math.Pow( HoN2*(alpha2 - 1.0) + 1.0, 2.0 ));
// 
// 	// Smith masking/shadowing
// double	k = (Roughness + 1)*(Roughness + 1) / 8.0;
// double	Gl = NoL / (NoL * (1-k) + k);
// double	Gv = NoV / (NoV * (1-k) + k);
// double	G = Gl * Gv;
// 
// //double	BRDF = G / (4.0 * View.y);
// //double	BRDF = G * HoV / (HoN * NoV);
// double	BRDF = NoL * GSmith( Roughness, NoV, NoL ) * 4.0f * HoV / HoN;




						// Compute Fresnel terms
						double	Schlick = 1.0 - HoV;
						double	Schlick5 = Schlick * Schlick;
								Schlick5 *= Schlick5 * Schlick;

						double	FresnelA = 1.0f - Schlick5;
						double	FresnelB = Schlick5;

//FresnelA = FresnelB = 1.0;

						SumA += FresnelA * BRDF;
						SumB += FresnelB * BRDF;
					}

// 					SumA *= 1.0 / (SAMPLES_COUNT * Math.PI * r2);
// 					SumB *= 1.0 / (SAMPLES_COUNT * Math.PI * r2);

					SumA /= SAMPLES_COUNT;
					SumB /= SAMPLES_COUNT;

					// For few samples, the sum goes over 1 because we have poor solid angle sampling resolution...
// 					SumA = Math.Min( 1.0, SumA );
// 					SumB = Math.Min( 1.0, SumB );

					Table0[X,Y] = SumA;
					Table1[X,Y] = SumB;
				}
			}

			// Write table 0
			using ( System.IO.FileStream S = _TableFileName0.Create() )
				using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) )
					for ( int Y=0; Y < _TableSize; Y++ ) {
						for ( int X=0; X < _TableSize; X++ )
							W.Write( Table0[X,Y] );
						}

			// Write table 1
			using ( System.IO.FileStream S = _TableFileName1.Create() )
				using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) )
					for ( int Y=0; Y < _TableSize; Y++ ) {
						for ( int X=0; X < _TableSize; X++ )
							W.Write( Table1[X,Y] );
						}
		}

/*	https://knarkowicz.wordpress.com/2014/12/27/analytical-dfg-term-for-ibl/
uint32_t ReverseBits( uint32_t v )
{
    v = ( ( v >> 1 ) & 0x55555555 ) | ( ( v & 0x55555555 ) << 1 );
    v = ( ( v >> 2 ) & 0x33333333 ) | ( ( v & 0x33333333 ) << 2 );
    v = ( ( v >> 4 ) & 0x0F0F0F0F ) | ( ( v & 0x0F0F0F0F ) << 4 );
    v = ( ( v >> 8 ) & 0x00FF00FF ) | ( ( v & 0x00FF00FF ) << 8 );
    v = (   v >> 16               ) | (   v                << 16 );
    return v;
}

float GSmith( float roughness, float ndotv, float ndotl )
{
    float const m2   = roughness * roughness;
    float const visV = ndotv + sqrt( ndotv * ( ndotv - ndotv * m2 ) + m2 );
    float const visL = ndotl + sqrt( ndotl * ( ndotl - ndotl * m2 ) + m2 );

    return 1.0f / ( visV * visL );
}

int main()
{
    float const MATH_PI         = 3.14159f;
    unsigned const LUT_WIDTH    = 128;
    unsigned const LUT_HEIGHT   = 128;
    unsigned const sampleNum    = 128;

    float lutData[ LUT_WIDTH * LUT_HEIGHT * 4 ];

    for ( unsigned y = 0; y < LUT_HEIGHT; ++y )
    {
        float const ndotv = ( y + 0.5f ) / LUT_WIDTH;

        for ( unsigned x = 0; x < LUT_WIDTH; ++x )
        {
            float const gloss       = ( x + 0.5f ) / LUT_HEIGHT;
            float const roughness   = powf( 1.0f - gloss, 4.0f );

            float const vx = sqrtf( 1.0f - ndotv * ndotv );
            float const vy = 0.0f;
            float const vz = ndotv;

            float scale = 0.0f;
            float bias  = 0.0f;

            
            for ( unsigned i = 0; i < sampleNum; ++i )
            {
                float const e1 = (float) i / sampleNum;
                float const e2 = (float) ( (double) ReverseBits( i ) / (double) 0x100000000LL );

                float const phi         = 2.0f * MATH_PI * e1;
                float const cosPhi      = cosf( phi );
                float const sinPhi      = sinf( phi );
                float const cosTheta    = sqrtf( ( 1.0f - e2 ) / ( 1.0f + ( roughness * roughness - 1.0f ) * e2 ) );
                float const sinTheta    = sqrtf( 1.0f - cosTheta * cosTheta );

                float const hx  = sinTheta * cosf( phi );
                float const hy  = sinTheta * sinf( phi );
                float const hz  = cosTheta;

                float const vdh = vx * hx + vy * hy + vz * hz;
                float const lx  = 2.0f * vdh * hx - vx;
                float const ly  = 2.0f * vdh * hy - vy;
                float const lz  = 2.0f * vdh * hz - vz;

                float const ndotl = std::max( lz,   0.0f );
                float const ndoth = std::max( hz,   0.0f );
                float const vdoth = std::max( vdh,  0.0f );

                if ( ndotl > 0.0f )
                {
                    float const gsmith      = GSmith( roughness, ndotv, ndotl );
                    float const ndotlVisPDF = ndotl * gsmith * ( 4.0f * vdoth / ndoth );
                    float const fc          = powf( 1.0f - vdoth, 5.0f );

                    scale   += ndotlVisPDF * ( 1.0f - fc );
                    bias    += ndotlVisPDF * fc;
                }
            }
            scale /= sampleNum;
            bias  /= sampleNum;

            lutData[ x * 4 + y * LUT_WIDTH * 4 + 0 ] = scale;
            lutData[ x * 4 + y * LUT_WIDTH * 4 + 1 ] = bias;
            lutData[ x * 4 + y * LUT_WIDTH * 4 + 2 ] = 0.0f;
            lutData[ x * 4 + y * LUT_WIDTH * 4 + 3 ] = 0.0f;
        }
    }   

} */

#endif

		Texture2D	BuildBRDFTexture( System.IO.FileInfo _TableFileName, uint _TableSize ) {

			float2[,]	Table = new float2[_TableSize,_TableSize];

			float	MinA = 1, MaxA = 0;
			float	MinB = 1, MaxB = 0;
			using ( System.IO.FileStream S = _TableFileName.OpenRead() )
				using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) )
					for ( int Y=0; Y < _TableSize; Y++ )
						for ( int X=0; X < _TableSize; X++ ) {
							float	A = R.ReadSingle();
							float	B = R.ReadSingle();
							Table[X,Y].x = A;
							Table[X,Y].y = B;
							MinA = Math.Min( MinA, A );
							MaxA = Math.Max( MaxA, A );
							MinB = Math.Min( MinB, B );
							MaxB = Math.Max( MaxB, B );
						}

			// MaxA = 1
			// MaxB = 0.00014996325546887346

// MaxA = 1.0;
// MaxB = 1.0;

			// Create the texture
//			PixelsBuffer	Content = new PixelsBuffer( _TableSize*_TableSize*4 );
			PixelsBuffer	Content = new PixelsBuffer( (uint) (_TableSize*_TableSize*2*4) );
			using ( System.IO.BinaryWriter W = Content.OpenStreamWrite() )
			for ( int Y=0; Y < _TableSize; Y++ )
				for ( int X=0; X < _TableSize; X++ ) {
// 					W.Write( (ushort) (65535.0 * Table[X,Y].x / MaxA) );
// 					W.Write( (ushort) (65535.0 * Table[X,Y].y / MaxB) );
					W.Write( Table[X,Y].x );
					W.Write( Table[X,Y].y );
				}


//			Texture2D	Result = new Texture2D( m_Device, _TableSize, _TableSize, 1, 1, ImageUtility.PIXEL_FORMAT.RG16, ImageUtility.COMPONENT_FORMAT.UNORM, false, false, new PixelsBuffer[] { Content } );
			Texture2D	Result = new Texture2D( m_Device, _TableSize, _TableSize, 1, 1, ImageUtility.PIXEL_FORMAT.RG32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new PixelsBuffer[] { Content } );
			return Result;
		}

/*

// 				float	Theta = 0.5 * _UV.x * PI;
// 				float3	ToLight = float3( sin( Theta ), 0, cos( Theta ) );
// 				float3	ToView = float3( -sin( Theta ), 0, cos( Theta ) );
// 
// 				float	Albedo = 0.0;
// 				const int	THETA_COUNT = 64; // * $alwaysOne; // warning X4008: floating point division by zero
// 				const float	dTheta = HALFPI / THETA_COUNT;
// 				const float	dPhi = PI / THETA_COUNT;
// 				for ( int i=0; i < THETA_COUNT; i++ )
// 				{
// 					Theta = HALFPI * (0.5 + i) / THETA_COUNT;
// 					for ( int j=0; j < THETA_COUNT; j++ )
// 					{
// 						float	Phi = PI * j / THETA_COUNT;
// 
// 						ToView = float3( sin( Theta ) * cos( Phi ), sin( Theta ) * sin( Phi ), cos( Theta ) );
// 
// 						float3	Half = normalize( ToLight + ToView );
// 
// 						// "True" and expensive evaluation of the Ward BRDF
// 						float	alpha = Roughness;
// 
// 						float	CosDelta = Half.z;	// dot( Half, _wsNormal );
// 						float	delta = acos( CosDelta );
// 						float	CosThetaL = ToLight.z;
// 						float	SinThetaL = sqrt( 1.0 - CosThetaL*CosThetaL );
// 						float	PhiL = atan2( ToLight.y, ToLight.x );
// 						float	CosThetaV = ToView.z;
// 						float	SinThetaV = sqrt( 1.0 - CosThetaV*CosThetaV );
// 						float	PhiV = atan2( ToView.y, ToView.x );
// 
// 						float	BRDF = 1.0 / square(alpha) * exp( -square( tan( delta ) / alpha ) ) * 2.0 * (1.0 + CosThetaL*CosThetaV + SinThetaL*SinThetaV*cos( PhiV - PhiL )) / pow4( CosThetaL + CosThetaV );
// 
// 						Albedo += BRDF * cos( Theta ) * sin( Theta ) * dTheta * dPhi;
// 					}
// 				}
// 
// 				Albedo *= 2.0;		// Since we integrate on half a hemisphere...
// 				Albedo *= INVPI;	// Since we forgot that in the main loop
// 
 
// ==========================================================================
// arkDebugBRDFAlbedo
// 
// 	Displays the true and theoretical BRDF albedos
// 
// ==========================================================================
//
renderProg PostFX/Debug/WardBRDFAlbedo {
	newstyle
	
	hlsl_prefix {

		#include <ward>
		
		// Displays the BRDF albedo in 2 small screen insets showing how the albedo varies with roughness
		// The albedo is computed by integrating the BRDF over an entire hemisphere of view directions for a 
		//	single light direction that varies with U in [0,1] corresponding to Theta_Light in [0,PI/2].
		//
		// The goal here is to demonstrate the albedo is correctly bounded (i.e. never > 1).
		// The especially important part is when the light is at grazing angles (i.e. U -> 1)
		//
		// Just call this function in the finalizer post-process, providing the screen UVs and the final color
		//
		void	DEBUG_DisplayWardBRDFAlbedo( float2 _UV, inout float3 _Color )
		{
			float	Roughness = lerp( 0.01, 1.0, 0.5 * (1.0 + sin( $time.x )) );	// Roughness is varying with time here...

			if ( _UV.x < 0.2 && _UV.y > 0.8 )
			{	// This version integrates the "true" BRDF
				// The formula comes from eq. (15) from http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.169.9908&rep=rep1&type=pdf
				//
				_UV.x /= 0.2;
				_UV.y = (_UV.y - 0.8) / 0.2;

				float	Theta = 0.5 * _UV.x * PI;
				float3	ToLight = float3( sin( Theta ), 0, cos( Theta ) );
				float3	ToView = float3( -sin( Theta ), 0, cos( Theta ) );

				float	Albedo = 0.0;
				const int	THETA_COUNT = 64; // * $alwaysOne; // warning X4008: floating point division by zero
				const float	dTheta = HALFPI / THETA_COUNT;
				const float	dPhi = PI / THETA_COUNT;
				for ( int i=0; i < THETA_COUNT; i++ )
				{
					Theta = HALFPI * (0.5 + i) / THETA_COUNT;
					for ( int j=0; j < THETA_COUNT; j++ )
					{
						float	Phi = PI * j / THETA_COUNT;

						ToView = float3( sin( Theta ) * cos( Phi ), sin( Theta ) * sin( Phi ), cos( Theta ) );

						float3	Half = normalize( ToLight + ToView );

						// "True" and expensive evaluation of the Ward BRDF
						float	alpha = Roughness;

						float	CosDelta = Half.z;	// dot( Half, _wsNormal );
						float	delta = acos( CosDelta );
						float	CosThetaL = ToLight.z;
						float	SinThetaL = sqrt( 1.0 - CosThetaL*CosThetaL );
						float	PhiL = atan2( ToLight.y, ToLight.x );
						float	CosThetaV = ToView.z;
						float	SinThetaV = sqrt( 1.0 - CosThetaV*CosThetaV );
						float	PhiV = atan2( ToView.y, ToView.x );

						float	BRDF = 1.0 / square(alpha) * exp( -square( tan( delta ) / alpha ) ) * 2.0 * (1.0 + CosThetaL*CosThetaV + SinThetaL*SinThetaV*cos( PhiV - PhiL )) / pow4( CosThetaL + CosThetaV );

						Albedo += BRDF * cos( Theta ) * sin( Theta ) * dTheta * dPhi;
					}
				}

				Albedo *= 2.0;		// Since we integrate on half a hemisphere...
				Albedo *= INVPI;	// Since we forgot that in the main loop

				_Color = 1.0 - _UV.y < 0.8 * Albedo ? 1 : 0;

				if ( _UV.x < 0.01 )
					_Color = Roughness;
			}
			else if ( _UV.x > 0.25 && _UV.x < 0.45 && _UV.y > 0.8 )
			{	// And this version uses our implementation of the Ward BRDF
				// The formula comes from eq. (23) of the same paper.
				// Both renderings should be equal and albedo should NEVER be > 1!
				//
				_UV.x = (_UV.x - 0.25) / 0.2;
				_UV.y = (_UV.y - 0.8) / 0.2;

				WardContext	ctx;
				CreateWardContext( ctx, Roughness, 0.0, 0.0, float3( 1, 0, 0 ), float3( 0, 1, 0 ) );

				float	Theta = 0.5 * _UV.x * PI;
				float3	ToLight = float3( sin( Theta ), 0, cos( Theta ) );
				float3	ToView = float3( -sin( Theta ), 0, cos( Theta ) );

				float	Albedo = 0.0;
				const int	THETA_COUNT = 64; // * $alwaysOne;	// Allows to prevent unrolling and lenghty shader compilation!
				const float	dTheta = HALFPI / THETA_COUNT;
				const float	dPhi = PI / THETA_COUNT;
				for ( int i=0; i < THETA_COUNT; i++ )
				{
					Theta = HALFPI * (0.5 + i) / THETA_COUNT;
					for ( int j=0; j < THETA_COUNT; j++ )
					{
						float	Phi = PI * j / THETA_COUNT;

						ToView = float3( sin( Theta ) * cos( Phi ), sin( Theta ) * sin( Phi ), cos( Theta ) );
 						Albedo += ctx.ComputeWardTerm( float3( 0, 0, 1 ), ToLight, ToView ) * cos( Theta ) * sin( Theta ) * dTheta * dPhi;
					}
				}

				Albedo *= 2.0;		// Since we integrate on half a hemisphere...
				Albedo *= INVPI;	// Since we forgot that in the main loop

				_Color = (1.0 - _UV.y < 0.8 * Albedo ? 1 : 0) * float3( 1, 0, 0 );

				if ( _UV.x < 0.01 )
					_Color = Roughness;
			}
		}
	}
}

 * /


/*

		struct WardContext
		{
			float3	anisoTangentDivRoughness;
			float3	anisoBitangentDivRoughness;
			float	specularNormalization;
			float	isotropicRoughness;				// Original normalized isotropic roughness
//			float	diffuseRoughness;				// ARKANE: bmayaux (2013-10-14) Disney diffuse roughness Fresnel term
		
			// Computes the Ward normal distribution
			//	_wsNormal, surface normal
			//	_wsToLight, world space light vector
			//	_wsView, world space view vector
			//	
			float	ComputeWardTerm( float3 _wsNormal, float3 _wsToLight, float3 _wsView )
			{
				float3	Half = _wsToLight + _wsView; 
//				Half = normalize(Half); // not normalized on purpose, HdotH would be 1 if normalized, nonsense
			
				float	HdotN = dot( Half, _wsNormal );
						HdotN = max( 1e-4f, HdotN );
				float	invHdotN_2 = 1.0 / square( HdotN );
				float	invHdotN_4 = square( invHdotN_2 );
			
				float	HdotT = dot( Half, anisoTangentDivRoughness );
				float	HdotB = dot( Half, anisoBitangentDivRoughness );
				float	HdotH = dot( Half, Half );
			
				float	exponent = -invHdotN_2 * (square( HdotT ) + square( HdotB ));

 				return specularNormalization * exp( exponent ) * HdotH * invHdotN_4;
			}
		};

		void	CreateWardContext( out WardContext wardContext, float _Roughness, float _Anisotropy, float _AnisotropyAngle, float3 _Tangent, float3 _BiTangent )
		{
			// Tweak roughness so the user feels it's a linear parameter
			float	Roughness = _Roughness * _Roughness;	// Roughness squared seems to give a nice linear feel...
															// People at Disney seem to agree with me! (cf §5.4 http://blog.selfshadow.com/publications/s2012-shading-course/burley/s2012_pbs_disney_brdf_notes_v2.pdf)

			// Keep an average normalized roughness value for other people who require it (e.g. IBL)
			wardContext.isotropicRoughness = Roughness;

			// Build anisotropic roughness along tangent/bitangent
			float2	anisotropicRoughness = float2( Roughness, Roughness * saturate( 1.0 - _Anisotropy ) );

			anisotropicRoughness = max( 0.01, anisotropicRoughness );	// Make sure we don't go below 0.01 otherwise specularity is unnatural for our poor lights (only IBL with many samples would solve that!)

			// Tangent/Ax, Bitangent/Ay
			float2	sinCosAnisotropy;
			sincos( _AnisotropyAngle, sinCosAnisotropy.x, sinCosAnisotropy.y );

			float2	invRoughness = 1.0 / (1e-5 + anisotropicRoughness);
			wardContext.anisoTangentDivRoughness = (sinCosAnisotropy.y * _Tangent + sinCosAnisotropy.x * _BiTangent) * invRoughness.x;
			wardContext.anisoBitangentDivRoughness = (sinCosAnisotropy.y * _BiTangent - sinCosAnisotropy.x * _Tangent) * invRoughness.y;

			wardContext.specularNormalization = INVPI * invRoughness.x * invRoughness.y;

// ARKANE: bmayaux (2014-02-05) Sheen is not tied to roughness anymore (for better or for worse?) so it doesn't need to be tied to Ward anymore either!
// 			// ARKANE: bmayaux (2013-10-14) Disney diffuse roughness Fresnel term
// 			// Diffuse roughness starts increasing for ward roughness > 0.35 and reaches 1 for ward roughness = 0.85
// 			//	this to make sure only very diffuse and rough objects have a sheen...
// //			wardContext.diffuseRoughness = _Sheen * saturate( 2.0 * _Roughness - 0.7 );
// 			wardContext.diffuseRoughness = _Sheen;	// Use direct sheen value, at the risk of making it strange on smooth materials...
		}

*/

		#endregion

		#region Primitives

		private void	BuildPrimitives()
		{
			{
				VertexPt4[]	Vertices = new VertexPt4[4];
				Vertices[0] = new VertexPt4() { Pt = new float4( -1, +1, 0, 1 ) };	// Top-Left
				Vertices[1] = new VertexPt4() { Pt = new float4( -1, -1, 0, 1 ) };	// Bottom-Left
				Vertices[2] = new VertexPt4() { Pt = new float4( +1, +1, 0, 1 ) };	// Top-Right
				Vertices[3] = new VertexPt4() { Pt = new float4( +1, -1, 0, 1 ) };	// Bottom-Right

				ByteBuffer	VerticesBuffer = VertexPt4.FromArray( Vertices );

				m_Prim_Quad = new Primitive( m_Device, Vertices.Length, VerticesBuffer, null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.Pt4 );
			}

			{
				VertexP3N3G3B3T2[]	Vertices = new VertexP3N3G3B3T2[4];
				Vertices[0] = new VertexP3N3G3B3T2() { P = new float3( -1, +1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 0, 0 ) };	// Top-Left
				Vertices[1] = new VertexP3N3G3B3T2() { P = new float3( -1, -1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 0, 1 ) };	// Bottom-Left
				Vertices[2] = new VertexP3N3G3B3T2() { P = new float3( +1, +1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 1, 0 ) };	// Top-Right
				Vertices[3] = new VertexP3N3G3B3T2() { P = new float3( +1, -1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 1, 1 ) };	// Bottom-Right

				ByteBuffer	VerticesBuffer = VertexP3N3G3B3T2.FromArray( Vertices );

				m_Prim_Rectangle = new Primitive( m_Device, Vertices.Length, VerticesBuffer, null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3N3G3B3T2 );
			}

			{	// Build the sphere
				const int	W = 41;
				const int	H = 22;
				VertexP3N3G3B3T2[]	Vertices = new VertexP3N3G3B3T2[W*H];
				for ( int Y=0; Y < H; Y++ ) {
					double	Theta = Math.PI * Y / (H-1);
					float	CosTheta = (float) Math.Cos( Theta );
					float	SinTheta = (float) Math.Sin( Theta );
					for ( int X=0; X < W; X++ ) {
						double	Phi = 2.0 * Math.PI * X / (W-1);
						float	CosPhi = (float) Math.Cos( Phi );
						float	SinPhi = (float) Math.Sin( Phi );

						float3	N = new float3( SinTheta * SinPhi, CosTheta, SinTheta * CosPhi );
						float3	T = new float3( CosPhi, 0.0f, -SinPhi );
						float3	B = N.Cross( T );

						Vertices[W*Y+X] = new VertexP3N3G3B3T2() {
							P = N,
							N = N,
							T = T,
							B = B,
							UV = new float2( 2.0f * X / W, 1.0f * Y / H )
						};
					}
				}

				ByteBuffer	VerticesBuffer = VertexP3N3G3B3T2.FromArray( Vertices );

				uint[]		Indices = new uint[(H-1) * (2*W+2)-2];
				int			IndexCount = 0;
				for ( int Y=0; Y < H-1; Y++ ) {
					for ( int X=0; X < W; X++ ) {
						Indices[IndexCount++] = (uint) ((Y+0) * W + X);
						Indices[IndexCount++] = (uint) ((Y+1) * W + X);
					}
					if ( Y < H-2 ) {
						Indices[IndexCount++] = (uint) ((Y+1) * W - 1);
						Indices[IndexCount++] = (uint) ((Y+1) * W + 0);
					}
				}

				m_Prim_Sphere = new Primitive( m_Device, Vertices.Length, VerticesBuffer, Indices, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3N3G3B3T2 );
			}

			{	// Build the cube
				float3[]	Normals = new float3[6] {
					-float3.UnitX,
					float3.UnitX,
					-float3.UnitY,
					float3.UnitY,
					-float3.UnitZ,
					float3.UnitZ,
				};

				float3[]	Tangents = new float3[6] {
					float3.UnitZ,
					-float3.UnitZ,
					float3.UnitX,
					-float3.UnitX,
					-float3.UnitX,
					float3.UnitX,
				};

				VertexP3N3G3B3T2[]	Vertices = new VertexP3N3G3B3T2[6*4];
				uint[]		Indices = new uint[2*6*3];

				for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ ) {
					float3	N = Normals[FaceIndex];
					float3	T = Tangents[FaceIndex];
					float3	B = N.Cross( T );

					Vertices[4*FaceIndex+0] = new VertexP3N3G3B3T2() {
						P = N - T + B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 0, 0 )
					};
					Vertices[4*FaceIndex+1] = new VertexP3N3G3B3T2() {
						P = N - T - B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 0, 1 )
					};
					Vertices[4*FaceIndex+2] = new VertexP3N3G3B3T2() {
						P = N + T - B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 1, 1 )
					};
					Vertices[4*FaceIndex+3] = new VertexP3N3G3B3T2() {
						P = N + T + B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 1, 0 )
					};

					Indices[2*3*FaceIndex+0] = (uint) (4*FaceIndex+0);
					Indices[2*3*FaceIndex+1] = (uint) (4*FaceIndex+1);
					Indices[2*3*FaceIndex+2] = (uint) (4*FaceIndex+2);
					Indices[2*3*FaceIndex+3] = (uint) (4*FaceIndex+0);
					Indices[2*3*FaceIndex+4] = (uint) (4*FaceIndex+2);
					Indices[2*3*FaceIndex+5] = (uint) (4*FaceIndex+3);
				}

				ByteBuffer	VerticesBuffer = VertexP3N3G3B3T2.FromArray( Vertices );

				m_Prim_Cube = new Primitive( m_Device, Vertices.Length, VerticesBuffer, Indices, Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3G3B3T2 );
			}
		}

		#endregion

		#region Open/Close

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			try {
				m_Device.Init( panelOutput.Handle, false, true );
			}
			catch ( Exception _e ) {
				m_Device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

// Build SATs
ComputeSAT( new System.IO.FileInfo( "Dummy.png" ), new System.IO.FileInfo( "DummySAT.dds" ) );
ComputeSAT( new System.IO.FileInfo( "StainedGlass.png" ), new System.IO.FileInfo( "AreaLightSAT.dds" ) );
ComputeSAT( new System.IO.FileInfo( "StainedGlass2.jpg" ), new System.IO.FileInfo( "AreaLightSAT2.dds" ) );
ComputeSAT( new System.IO.FileInfo( "StainedGlass3.png" ), new System.IO.FileInfo( "AreaLightSAT3.dds" ) );
ComputeSAT( new System.IO.FileInfo( "StainedGlass2Fade.png" ), new System.IO.FileInfo( "AreaLightSAT2Fade.dds" ) );

			buttonRebuildBRDF_Click( null, EventArgs.Empty );


			BuildPrimitives();
//			m_Tex_AreaLight = Pipi2Texture( new System.IO.FileInfo( "AreaLight.pipi" ) );
//			m_Tex_AreaLightSAT = PipoImage2Texture( new System.IO.FileInfo( "AreaLightSAT.pipo" ) );

			m_Tex_AreaLight = Image2Texture( new System.IO.FileInfo( "AreaLightSAT2.dds" ), ImageUtility.COMPONENT_FORMAT.UNORM_sRGB );
//			m_Tex_AreaLight3D = Pipu2Texture( new System.IO.FileInfo( "AreaLightSAT2.pipu" ) );
// 			m_Tex_AreaLightSAT = PipoImage2Texture( new System.IO.FileInfo( "AreaLightSAT2.pipo" ) );
// 			m_Tex_AreaLightSATFade = PipoImage2Texture( new System.IO.FileInfo( "AreaLightSAT2Fade.pipo" ) );

//			m_Tex_AreaLight = Pipi2Texture( new System.IO.FileInfo( "AreaLightSAT3.pipi" ) );
//			m_Tex_AreaLightSAT = PipoImage2Texture( new System.IO.FileInfo( "AreaLightSAT3.pipo" ) );

			m_Tex_FalseColors = Image2Texture( new System.IO.FileInfo( "FalseColorsSpectrum2.png" ), ImageUtility.COMPONENT_FORMAT.UNORM_sRGB );

// 			m_Tex_GlossMap = Image2Texture( new System.IO.FileInfo( "wooden_dirty_floor_01_g.png" ) );
// 			m_Tex_Normal = Image2Texture( new System.IO.FileInfo( "wooden_dirty_floor_01_n.png" ) );

			uint	SHADOW_MAP_SIZE = 512;
			m_Tex_ShadowMap = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, DEPTH_STENCIL_FORMAT.D32 );
#if FILTER_EXP_SHADOW_MAP
			m_Tex_ShadowMapFiltered[0] = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.R16, ImageUtility.COMPONENT_FORMAT.UNORM, false, false, null );
			m_Tex_ShadowMapFiltered[1] = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.R16, ImageUtility.COMPONENT_FORMAT.UNORM, false, false, null );
// 			m_Tex_ShadowMapFiltered[0] = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, null );
// 			m_Tex_ShadowMapFiltered[1] = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, null );
#else
			m_Tex_ShadowSmoothie = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, 1, PIXEL_FORMAT.RG16_FLOAT, false, false, null );
			m_Tex_ShadowSmoothiePou[0] = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, 1, PIXEL_FORMAT.RG16_FLOAT, false, false, null );
			m_Tex_ShadowSmoothiePou[1] = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, 1, PIXEL_FORMAT.RG16_FLOAT, false, false, null );
#endif


			m_CB_Main = new ConstantBuffer<CB_Main>( m_Device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 1 );
			m_CB_Light = new ConstantBuffer<CB_Light>( m_Device, 2 );
			m_CB_ShadowMap = new ConstantBuffer<CB_ShadowMap>( m_Device, 3 );
			m_CB_Object = new ConstantBuffer<CB_Object>( m_Device, 4 );

			try {
				m_Shader_RenderAreaLight = new Shader( m_Device, new System.IO.FileInfo( "Shaders/RenderAreaLight.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );;
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader \"RenderAreaLight\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_RenderAreaLight = null;
			}

			try {
				m_Shader_RenderShadowMap = new Shader( m_Device, new System.IO.FileInfo( "Shaders/RenderShadowMap.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );;
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader \"RenderShadow\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_RenderShadowMap = null;
			}

#if FILTER_EXP_SHADOW_MAP
			try {
				m_Shader_FilterShadowMapH = new Shader( m_Device, new System.IO.FileInfo( "Shaders/FilterShadowMap.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS_FilterH", null );;
				m_Shader_FilterShadowMapV = new Shader( m_Device, new System.IO.FileInfo( "Shaders/FilterShadowMap.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS_FilterV", null );;
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader \"BuildSmoothie\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_FilterShadowMapH = null;
				m_Shader_FilterShadowMapV = null;
			}
#else
			try
			{
				m_Shader_BuildSmoothie = new Shader( m_Device, new System.IO.FileInfo( "Shaders/BuildSmoothie.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS_Edge", null );;
				m_Shader_BuildSmoothieDistanceFieldH = new Shader( m_Device, new System.IO.FileInfo( "Shaders/BuildSmoothie.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS_DistanceFieldH", null );;
				m_Shader_BuildSmoothieDistanceFieldV = new Shader( m_Device, new System.IO.FileInfo( "Shaders/BuildSmoothie.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS_DistanceFieldV", null );;
			}
			catch ( Exception _e )
			{
				MessageBox.Show( "Shader \"BuildSmoothie\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_BuildSmoothie = null;
				m_Shader_BuildSmoothieDistanceFieldH = null;
				m_Shader_BuildSmoothieDistanceFieldV = null;
			}
#endif

			try {
				m_Shader_RenderScene = new Shader( m_Device, new System.IO.FileInfo( "Shaders/RenderScene.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );;
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader \"RenderScene\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_RenderScene = null;
			}

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 1, 4 ), new float3( 0, 1, 0 ), float3.UnitY );

			// Start game time
			m_Ticks2Seconds = 1.0 / System.Diagnostics.Stopwatch.Frequency;
			m_StopWatch.Start();
			m_StartTime = GetGameTime();

			m_Camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );
			Camera_CameraTransformChanged( m_Camera, EventArgs.Empty );

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnFormClosed( FormClosedEventArgs e ) {
			if ( m_Device == null )
				return;

			if ( m_Shader_RenderShadowMap != null ) {
				m_Shader_RenderShadowMap.Dispose();
			}
#if FILTER_EXP_SHADOW_MAP
			if ( m_Shader_FilterShadowMapH != null ) {
				m_Shader_FilterShadowMapH.Dispose();
				m_Shader_FilterShadowMapV.Dispose();
			}
#else
			if ( m_Shader_BuildSmoothie != null ) {
				m_Shader_BuildSmoothie.Dispose();
				m_Shader_BuildSmoothieDistanceFieldH.Dispose();
				m_Shader_BuildSmoothieDistanceFieldV.Dispose();
			}
#endif
			if ( m_Shader_RenderAreaLight != null ) {
				m_Shader_RenderAreaLight.Dispose();
			}
			if ( m_Shader_RenderScene != null ) {
				m_Shader_RenderScene.Dispose();
			}

			m_CB_Main.Dispose();
			m_CB_Camera.Dispose();
			m_CB_Light.Dispose();
			m_CB_ShadowMap.Dispose();
			m_CB_Object.Dispose();

			m_Prim_Quad.Dispose();
			m_Prim_Rectangle.Dispose();
			m_Prim_Sphere.Dispose();
			m_Prim_Cube.Dispose();

			m_Tex_BRDFIntegral.Dispose();

			m_Tex_ShadowMap.Dispose();
#if FILTER_EXP_SHADOW_MAP
			m_Tex_ShadowMapFiltered[0].Dispose();
			m_Tex_ShadowMapFiltered[1].Dispose();
#else
			m_Tex_ShadowSmoothie.Dispose();
			m_Tex_ShadowSmoothiePou[0].Dispose();
			m_Tex_ShadowSmoothiePou[1].Dispose();
#endif
			m_Tex_AreaLight.Dispose();
//			m_Tex_AreaLight3D.Dispose();
// 			m_Tex_AreaLightSAT.Dispose();
// 			m_Tex_AreaLightSATFade.Dispose();

// 			m_Tex_Normal.Dispose();
// 			m_Tex_GlossMap.Dispose();
			m_Tex_FalseColors.Dispose();

			m_Device.Exit();

			base.OnFormClosed( e );
		}

		#endregion

		/// <summary>
		/// Gets the current game time in seconds
		/// </summary>
		/// <returns></returns>
		public float	GetGameTime() {
			long	Ticks = m_StopWatch.ElapsedTicks;
			float	Time = (float) (Ticks * m_Ticks2Seconds);
			return Time;
		}

		void Camera_CameraTransformChanged( object sender, EventArgs e ) {
			m_CB_Camera.m._Camera2World = m_Camera.Camera2World;
			m_CB_Camera.m._World2Camera = m_Camera.World2Camera;

			m_CB_Camera.m._Camera2Proj = m_Camera.Camera2Proj;
			m_CB_Camera.m._Proj2Camera = m_CB_Camera.m._Camera2Proj.Inverse;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Proj2Camera * m_CB_Camera.m._Camera2World;

			m_CB_Camera.UpdateData();
		}

		void RenderScene( Shader _Shader ) {

			// Render a floor plane
			if ( _Shader == m_Shader_RenderScene ) {
				m_CB_Object.m._Local2World.BuildRotRightHanded( float3.Zero, float3.UnitY, float3.UnitX );
				m_CB_Object.m._Local2World.Scale( new float3( 16.0f, 16.0f, 1.0f ) );
				m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
				m_CB_Object.m._DiffuseAlbedo = 0.5f * new float3( 1, 1, 1 );
				m_CB_Object.m._SpecularTint = new float3( 0.95f, 0.94f, 0.93f );
				m_CB_Object.m._Gloss = floatTrackbarControlGloss.Value;
				m_CB_Object.m._Metal = floatTrackbarControlMetal.Value;
				m_CB_Object.m._UseTexture = checkBoxUseTexture.Checked ? 1U : 0U;
				m_CB_Object.m._FalseColors = checkBoxFalseColors.Checked ? 1U : 0U;
				m_CB_Object.m._FalseColorsMaxRange = floatTrackbarControlFalseColorsRange.Value;
				m_CB_Object.UpdateData();

				m_Prim_Rectangle.Render( _Shader );
			}

			// Render the sphere
			m_CB_Object.m._Local2World.BuildRotRightHanded( new float3( 0, 0.5f, 1.0f ), new float3( 0, 0.5f, 2 ), float3.UnitY );
//			m_CB_Object.m._Local2World.MakeLookAt( new float3( 0, 0.3f, 1.0f ), new float3( 0, 0.3f, 2 ), float3.UnitY );
			m_CB_Object.m._Local2World.Scale( new float3( 0.5f, 0.5f, 0.5f ) );
			m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
			m_CB_Object.m._DiffuseAlbedo = 0.5f * new float3( 1, 0.8f, 0.5f );
			m_CB_Object.m._SpecularTint = new float3( 0.95f, 0.94f, 0.93f );
 			m_CB_Object.m._Gloss = floatTrackbarControlGloss.Value;
 			m_CB_Object.m._Metal = floatTrackbarControlMetal.Value;
			m_CB_Object.m._UseTexture = checkBoxUseTexture.Checked ? 1U : 0U;
			m_CB_Object.m._FalseColors = checkBoxFalseColors.Checked ? 1U : 0U;
			m_CB_Object.m._FalseColorsMaxRange = floatTrackbarControlFalseColorsRange.Value;
			m_CB_Object.UpdateData();

			m_Prim_Sphere.Render( _Shader );

			// Render the tiny cubes
			for ( int CubeIndex=0; CubeIndex < 4; CubeIndex++ ) {

				float	X = -1.0f + 2.0f * CubeIndex / 3;
//				float	Y = 0.1f + 1.0f * (float) Math.Abs( Math.Sin( m_CB_Main.m.iGlobalTime + CubeIndex ));
				float	Y = 0.1f + (float) Math.Max( 0.0, Math.Sin( m_CB_Main.m.iGlobalTime + CubeIndex ));

//				m_CB_Object.m._Local2World.MakeLookAt( new float3( 1.0f, 0.1f, 0.0f ), new float3( 1.0f, 0.1f, 1 ), float3.UnitY );
				m_CB_Object.m._Local2World.BuildRotRightHanded( new float3( X, Y, 0.0f ), new float3( X, Y, 1 ), float3.UnitY );
				m_CB_Object.m._Local2World.Scale( new float3( 0.1f, 0.1f, 0.1f ) );
				m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
				m_CB_Object.m._DiffuseAlbedo = 0.5f * new float3( 1, 1, 1 );
				m_CB_Object.m._SpecularTint = new float3( 0.95f, 0.94f, 0.92f );
 				m_CB_Object.m._Gloss = floatTrackbarControlGloss.Value;
 				m_CB_Object.m._Metal = floatTrackbarControlMetal.Value;
				m_CB_Object.m._UseTexture = checkBoxUseTexture.Checked ? 1U : 0U;
				m_CB_Object.m._FalseColors = checkBoxFalseColors.Checked ? 1U : 0U;
				m_CB_Object.m._FalseColorsMaxRange = floatTrackbarControlFalseColorsRange.Value;
				m_CB_Object.UpdateData();

				m_Prim_Cube.Render( _Shader );
			}
		}

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_Device == null )
				return;

			// Setup global data
			m_CB_Main.m.iResolution = new float3( panelOutput.Width, panelOutput.Height, 0 );
			if ( checkBoxAnimate.Checked )
				m_CB_Main.m.iGlobalTime = GetGameTime() - m_StartTime;
			m_CB_Main.UpdateData();

			// Setup area light buffer
			float		LighOffsetX = 1.2f;
			float		SizeX = floatTrackbarControlLightScaleX.Value;
			float		SizeY = 1.0f;
			float		RollAngle = (float) (Math.PI * floatTrackbarControlLightRoll.Value / 180.0);
			float3		LightPosition = new float3( LighOffsetX + floatTrackbarControlLightPosX.Value, 1.0f + floatTrackbarControlLightPosY.Value, -1.0f + floatTrackbarControlLightPosZ.Value );
			float3		LightTarget = new float3( LightPosition.x + floatTrackbarControlLightTargetX.Value, LightPosition.y + floatTrackbarControlLightTargetY.Value, LightPosition.z + 2.0f + floatTrackbarControlLightTargetZ.Value );
			float3		LightUp = new float3( (float) Math.Sin( -RollAngle ), (float) Math.Cos( RollAngle ), 0.0f );
			float4x4	AreaLight2World = new float4x4(); 
						AreaLight2World.BuildRotRightHanded( LightPosition, LightTarget, LightUp );

			float4x4	World2AreaLight = AreaLight2World.Inverse;

			double		Phi = Math.PI * floatTrackbarControlProjectionPhi.Value / 180.0;
			double		Theta = Math.PI * floatTrackbarControlProjectionTheta.Value / 180.0;
			float3		Direction = new float3( (float) (Math.Sin(Theta) * Math.Sin(Phi)), (float) (Math.Sin(Theta) * Math.Cos(Phi)), (float) Math.Cos( Theta ) );

			const float	DiffusionMin = 1e-2f;
			const float	DiffusionMax = 1000.0f;
//			float		Diffusion_Diffuse = DiffusionMin / (DiffusionMin / DiffusionMax + floatTrackbarControlProjectionDiffusion.Value);
			float		Diffusion_Diffuse = DiffusionMax + (DiffusionMin - DiffusionMax) * (float) Math.Pow( floatTrackbarControlProjectionDiffusion.Value, 0.01f );

//			float3		LocalDirection_Diffuse = (float3) (new float4( Diffusion_Diffuse * Direction, 0 ) * World2AreaLight);
			float3		LocalDirection_Diffuse = Diffusion_Diffuse * Direction;

			m_CB_Light.m._AreaLightX = (float3) AreaLight2World[0];
			m_CB_Light.m._AreaLightY = (float3) AreaLight2World[1];
			m_CB_Light.m._AreaLightZ = (float3) AreaLight2World[2];
			m_CB_Light.m._AreaLightT = (float3) AreaLight2World[3];
			m_CB_Light.m._AreaLightScaleX = SizeX;
			m_CB_Light.m._AreaLightScaleY = SizeY;
			m_CB_Light.m._AreaLightDiffusion = floatTrackbarControlProjectionDiffusion.Value;
			m_CB_Light.m._AreaLightIntensity = floatTrackbarControlLightIntensity.Value;
			m_CB_Light.m._AreaLightTexDimensions = new float4( m_Tex_AreaLight.Width, m_Tex_AreaLight.Height, 1.0f / m_Tex_AreaLight.Width, 1.0f / m_Tex_AreaLight.Height );
			m_CB_Light.m._ProjectionDirectionDiff = LocalDirection_Diffuse;
			m_CB_Light.UpdateData();


			// =========== Render shadow map ===========
//			float	KernelSize = 16.0f * floatTrackbarControlProjectionDiffusion.Value;
			float	KernelSize = floatTrackbarControlKernelSize.Value;

//			float	ShadowZFar = (float) Math.Sqrt( 2.0 ) * m_Camera.Far;
			float	ShadowZFar = 10.0f;
			m_CB_ShadowMap.m._ShadowOffsetXY = (float2) Direction;
			m_CB_ShadowMap.m._ShadowZFar = new float2( ShadowZFar, 1.0f / ShadowZFar );
			m_CB_ShadowMap.m._KernelSize = KernelSize;
			m_CB_ShadowMap.m._InvShadowMapSize = 1.0f / m_Tex_ShadowMap.Width;
			m_CB_ShadowMap.m._HardeningFactor = new float2( floatTrackbarControlHardeningFactor.Value, floatTrackbarControlHardeningFactor2.Value );
			m_CB_ShadowMap.UpdateData();

			if ( m_Shader_RenderShadowMap != null && m_Shader_RenderShadowMap.Use() ) {
				m_Tex_ShadowMap.RemoveFromLastAssignedSlots();

				m_Device.SetRenderTargets( m_Tex_ShadowMap.Width, m_Tex_ShadowMap.Height, new View2D[0], m_Tex_ShadowMap );
#if FILTER_EXP_SHADOW_MAP
// 				m_Device.ClearDepthStencil( m_Tex_ShadowMap, 0.0f, 0, true, false );
// 				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_GREATER, BLEND_STATE.DISABLED );	// For exp shadow map, the Z order is reversed
				m_Device.ClearDepthStencil( m_Tex_ShadowMap, 1.0f, 0, true, false );
				m_Device.SetRenderStates( checkBoxCullFront.Checked ? RASTERIZER_STATE.CULL_BACK : RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );	// For exp shadow map, the Z order is reversed
#else
				m_Device.ClearDepthStencil( m_Tex_ShadowMap, 1.0f, 0, true, false );
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
#endif
				RenderScene( m_Shader_RenderShadowMap );

				m_Device.RemoveRenderTargets();
				m_Tex_ShadowMap.SetPS( 2 );
			}

#if FILTER_EXP_SHADOW_MAP
			if ( m_Shader_FilterShadowMapH != null ) {
//				m_Tex_ShadowMapFiltered[1].RemoveFromLastAssignedSlots();

				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

				// Filter horizontally
				m_Device.SetRenderTarget( m_Tex_ShadowMapFiltered[0], null );
				m_Shader_FilterShadowMapH.Use();
				m_Prim_Quad.Render( m_Shader_FilterShadowMapH );

				// Filter vertically
				m_Device.SetRenderTarget( m_Tex_ShadowMapFiltered[1], null );
				m_Tex_ShadowMapFiltered[0].SetPS( 2 );

				m_Shader_FilterShadowMapV.Use();
				m_Prim_Quad.Render( m_Shader_FilterShadowMapV );

				m_Device.RemoveRenderTargets();
				m_Tex_ShadowMapFiltered[1].SetPS( 2 );
			}
#else
			if ( m_Shader_BuildSmoothie != null && m_Shader_BuildSmoothie.Use() ) {
				m_Tex_ShadowSmoothie.RemoveFromLastAssignedSlots();

				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

				// Render the (silhouette + Z) RG16 buffer
				m_Device.SetRenderTarget( m_Tex_ShadowSmoothie, null );

				m_Prim_Quad.Render( m_Shader_BuildSmoothie );

				m_Device.RemoveRenderTargets();
				m_Tex_ShadowSmoothie.SetPS( 3 );

				// Build distance field
				m_Device.SetRenderTarget( m_Tex_ShadowSmoothiePou[0], null );
				m_Shader_BuildSmoothieDistanceFieldH.Use();
				m_Prim_Quad.Render( m_Shader_BuildSmoothieDistanceFieldH );

// m_Device.RemoveRenderTargets();
// m_Tex_ShadowSmoothiePou[0].SetPS( 3 );

				m_Device.SetRenderTarget( m_Tex_ShadowSmoothiePou[1], null );
				m_Tex_ShadowSmoothiePou[0].SetPS( 0 );
				m_Shader_BuildSmoothieDistanceFieldV.Use();
				m_Prim_Quad.Render( m_Shader_BuildSmoothieDistanceFieldV );

				m_Device.RemoveRenderTargets();
				m_Tex_ShadowSmoothiePou[1].SetPS( 3 );
			}
#endif

			// =========== Render scene ===========
			m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );

			m_Device.Clear( m_Device.DefaultTarget, float4.Zero );
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );

//			m_Tex_AreaLightSAT.SetPS( 0 );
//			m_Tex_AreaLight3D.SetPS( 0 );
//			m_Tex_AreaLightSATFade.SetPS( 1 );
			m_Tex_AreaLight.SetPS( 4 );
			m_Tex_FalseColors.SetPS( 6 );
// 			m_Tex_GlossMap.SetPS( 7 );
// 			m_Tex_Normal.SetPS( 8 );

			// Render the area light itself
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
			if ( m_Shader_RenderAreaLight != null && m_Shader_RenderAreaLight.Use() ) {

				m_CB_Object.m._Local2World = AreaLight2World;
				m_CB_Object.m._Local2World.Scale( new float3( SizeX, SizeY, 1.0f ) );
				m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
				m_CB_Object.m._UseTexture = checkBoxUseTexture.Checked ? 1U : 0U;
				m_CB_Object.m._FalseColors = checkBoxFalseColors.Checked ? 1U : 0U;
				m_CB_Object.m._FalseColorsMaxRange = floatTrackbarControlFalseColorsRange.Value;
				m_CB_Object.UpdateData();

				m_Prim_Rectangle.Render( m_Shader_RenderAreaLight );
			} else {
				m_Device.Clear( new float4( 1, 0, 0, 0 ) );
			}


			// Render the scene
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_BACK, DEPTHSTENCIL_STATE.NOCHANGE, BLEND_STATE.NOCHANGE );
			if ( m_Shader_RenderScene != null && m_Shader_RenderScene.Use() ) {
				RenderScene( m_Shader_RenderScene );
			} else {
				m_Device.Clear( new float4( 1, 1, 0, 0 ) );
			}


			// Show!
			m_Device.Present( false );


			// Update window text
//			Text = "Zombizous Prototype - " + m_Game.m_CurrentGameTime.ToString( "G5" ) + "s";
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			if ( m_Device != null )
				m_Device.ReloadModifiedShaders();
		}

		private void buttonRebuildBRDF_Click( object sender, EventArgs e ) {
			ComputeBRDFIntegral( new System.IO.FileInfo( "BRDF0_64x64.bin" ), 64 );
//			ComputeBRDFIntegralImportanceSampling( new System.IO.FileInfo( "BRDF1_64x64.bin" ), 64 );

			if ( m_Tex_BRDFIntegral != null )
				m_Tex_BRDFIntegral.Dispose();
			m_Tex_BRDFIntegral = BuildBRDFTexture( new System.IO.FileInfo( "BRDF0_64x64.bin" ), 64 );
			m_Tex_BRDFIntegral.SetPS( 5 );
		}

		private void checkBoxUseTexture_CheckedChanged( object sender, EventArgs e ) {
		}
	}
}
