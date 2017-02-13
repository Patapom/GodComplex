using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace TestFilmicCurve
{
	public class BImage
	{
		#region CONSTANTS

//		const uint	SUPPORTED_VERSION = 13;
		const uint	SUPPORTED_VERSION = 17;
		const uint	MAGIC = 0x004D4942 | (SUPPORTED_VERSION << 24);
//		#define BIMAGE_MAGIC (unsigned int)( ('B'<<0)|('I'<<8)|('M'<<16)|(BIMAGE_VERSION<<24) )

		#endregion

		#region NESTED TYPES

		public class PixelFormat {
			#region ENUMS

			public enum Layout {
				LAYOUT_NONE = 0,

				LAYOUT_8,
				LAYOUT_16,
				LAYOUT_32,

				LAYOUT_8_8,
				LAYOUT_16_16,
				LAYOUT_32_32,

				LAYOUT_8_8_8,
				LAYOUT_16_16_16,
				LAYOUT_32_32_32,

				LAYOUT_8_8_8_8,
				LAYOUT_16_16_16_16,
				LAYOUT_32_32_32_32,
	
				LAYOUT_10_10_10_2,
			//	LAYOUT_4_4_4_4
			//	LAYOUT_5_6_5
			//	LAYOUT_5_5_5_1

				LAYOUT_BC1,
				LAYOUT_BC2,
				LAYOUT_BC3,
				LAYOUT_BC4,
				LAYOUT_BC5,
				LAYOUT_BC6,
				LAYOUT_BC7,
	
				LAYOUT_16_8, //Depth-stencil, NB: not supported on D3D
				LAYOUT_24_8, //Depth-stencil, NB: not supported on Mantle
				LAYOUT_32_8, //Depth-stencil, NB: some platforms have internally seperated surfaces (one 32 bits and one 8 bits) - ATI, Durango, Orbis, and others may rely on one 64 bits surfaces (the second one will be D32S8X24) - NVidia ?

				LAYOUT_R11G11B10,

				LAYOUT_MAX
			};

			public enum Type {
				NONE = 0,

				TYPELESS,
				SINT,
				UINT,
				SNORM,
				UNORM,
				UNORM_sRGB, //gamma corrected format
				FLOAT,

				MAX,
			};

			public enum Swizzle {
				NONE = 0,
	
				RGBA,
				ARGB,
				BGRA,

				DEPTH,
				DEPTH_STENCIL,

				MAX
			};

			#endregion

			public Layout		m_layout = Layout.LAYOUT_NONE;
			public Type			m_type = Type.NONE;
			public Swizzle		m_swizzle = Swizzle.NONE;

			public bool			IsCompressed	{ get { return m_layout >= Layout.LAYOUT_BC1 && m_layout <= Layout.LAYOUT_BC7; } }
			public bool			IsDepth			{ get { return m_swizzle == Swizzle.DEPTH || m_swizzle == Swizzle.DEPTH_STENCIL; } }
			public bool			HasStencil		{ get { return m_swizzle == Swizzle.DEPTH_STENCIL; } }
			public bool			Is_sRGB			{ get { return m_type == Type.UNORM_sRGB; } }

			public uint			BitsCount {
				get  {
					switch ( m_layout ) {
						case Layout.LAYOUT_8: return 8;
						case Layout.LAYOUT_16: return 16;
						case Layout.LAYOUT_32: return 32;
						case Layout.LAYOUT_8_8: return 16;
						case Layout.LAYOUT_16_16: return 32;
						case Layout.LAYOUT_32_32: return 64;
						case Layout.LAYOUT_8_8_8: return 24;
						case Layout.LAYOUT_16_16_16: return 48;
						case Layout.LAYOUT_32_32_32: return 96;
						case Layout.LAYOUT_8_8_8_8: return 32;
						case Layout.LAYOUT_16_16_16_16: return 64;
						case Layout.LAYOUT_32_32_32_32: return 128;
						case Layout.LAYOUT_10_10_10_2: return 32;
						case Layout.LAYOUT_R11G11B10: return 32;
					//	case ARK_GFX_FORMAT_LAYOUT_4_4_4_4: return 16;
					//	case ARK_GFX_FORMAT_LAYOUT_5_6_5: return 16;
					//	case ARK_GFX_FORMAT_LAYOUT_5_5_5_1: return 16;
	
						//TODO find BCn sizes
						case Layout.LAYOUT_BC1: return 4;
						case Layout.LAYOUT_BC2: return 8;
						case Layout.LAYOUT_BC3: return 8;
						case Layout.LAYOUT_BC4: return 4;
						case Layout.LAYOUT_BC5: return 8;
						case Layout.LAYOUT_BC6: return 8;
						case Layout.LAYOUT_BC7: return 8;

						/// JV: don't know what to do with depth-stencil formats, some platforms have separated surfaces. Which one do you want bitcnt ? If this is needed with depth-strencil, add a specific method...
						/*case ARK_GFX_FORMAT_LAYOUT_24_8: return 32;
						case ARK_GFX_FORMAT_LAYOUT_16_8: return 32;*/

						default:
							throw new Exception( "Invalid arkGFXFormat_t" );
					}
				}
			}

			public void	GetEquivalentRendererFormat( out ImageUtility.PIXEL_FORMAT _pixelFormat, out ImageUtility.COMPONENT_FORMAT _componentFormat ) {
				switch ( m_type ) {
					case Type.UINT:			_componentFormat = ImageUtility.COMPONENT_FORMAT.UINT; break;
					case Type.SINT:			_componentFormat = ImageUtility.COMPONENT_FORMAT.SINT; break;
					case Type.UNORM:		_componentFormat = ImageUtility.COMPONENT_FORMAT.UNORM; break;
					case Type.UNORM_sRGB:	_componentFormat = ImageUtility.COMPONENT_FORMAT.UNORM_sRGB; break;
					case Type.SNORM:		_componentFormat = ImageUtility.COMPONENT_FORMAT.SNORM; break;
					default:
						_componentFormat = ImageUtility.COMPONENT_FORMAT.AUTO;
						break;
				}

				switch ( m_layout ) {
					// 8-bits formats
					case Layout.LAYOUT_8:			_pixelFormat = ImageUtility.PIXEL_FORMAT.R8; break;
					case Layout.LAYOUT_8_8:			_pixelFormat = ImageUtility.PIXEL_FORMAT.RG8; break;
					case Layout.LAYOUT_8_8_8:		_pixelFormat = ImageUtility.PIXEL_FORMAT.RGB8; break;
					case Layout.LAYOUT_8_8_8_8:		_pixelFormat = ImageUtility.PIXEL_FORMAT.RGBA8; break;

					// 16-bit formats
					case Layout.LAYOUT_16:			_pixelFormat = m_type == Type.FLOAT ? ImageUtility.PIXEL_FORMAT.R16 : ImageUtility.PIXEL_FORMAT.R16F; break;
					case Layout.LAYOUT_16_16:		_pixelFormat = m_type == Type.FLOAT ? ImageUtility.PIXEL_FORMAT.RG16 : ImageUtility.PIXEL_FORMAT.RG16F; break;
					case Layout.LAYOUT_16_16_16:	_pixelFormat = m_type == Type.FLOAT ? ImageUtility.PIXEL_FORMAT.RGB16 : ImageUtility.PIXEL_FORMAT.RGB16F; break;
					case Layout.LAYOUT_16_16_16_16:	_pixelFormat = m_type == Type.FLOAT ? ImageUtility.PIXEL_FORMAT.RGBA16 : ImageUtility.PIXEL_FORMAT.RGBA16F; break;

					// 32-bit formats
					case Layout.LAYOUT_32:			_pixelFormat = m_type == Type.FLOAT ? ImageUtility.PIXEL_FORMAT.R32 : ImageUtility.PIXEL_FORMAT.R32F; break;
					case Layout.LAYOUT_32_32:		_pixelFormat = m_type == Type.FLOAT ? ImageUtility.PIXEL_FORMAT.RG32 : ImageUtility.PIXEL_FORMAT.RG32F; break;
					case Layout.LAYOUT_32_32_32:	_pixelFormat = m_type == Type.FLOAT ? ImageUtility.PIXEL_FORMAT.RGB32 : ImageUtility.PIXEL_FORMAT.RGB32F; break;
					case Layout.LAYOUT_32_32_32_32:	_pixelFormat = m_type == Type.FLOAT ? ImageUtility.PIXEL_FORMAT.RGBA32 : ImageUtility.PIXEL_FORMAT.RGBA32F; break;

					// Compressed formats
					case Layout.LAYOUT_BC4:			_pixelFormat = ImageUtility.PIXEL_FORMAT.BC4; break;
					case Layout.LAYOUT_BC5:			_pixelFormat = ImageUtility.PIXEL_FORMAT.BC5; break;
					case Layout.LAYOUT_BC6:			_pixelFormat = ImageUtility.PIXEL_FORMAT.BC6H; break;
					case Layout.LAYOUT_BC7:			_pixelFormat = ImageUtility.PIXEL_FORMAT.BC7; break;

					default:
						throw new Exception( "Unsupported image format " + ToString() );
				}
			}

			public void		Read( BinaryReader _R ) {
				m_layout = (Layout) _R.ReadUInt32();
				m_type = (Type) _R.ReadUInt32();
				m_swizzle = (Swizzle) _R.ReadUInt32();
			}

			public override string ToString() {
				return "Layout " + m_layout + " - Type " + m_type + " - Swizzle " + m_swizzle;
			}
		}

		public class	ImageOptions {

			const uint	SUPPORTED_IMAGEOPTS_VERSION = 18;

			public enum TYPE {
				TT_2D,
				TT_3D,
				TT_CUBIC,
			}

			[Flags]
			public enum FLAGS {
				GENERATE_MIPS			= 1 << 0,
				LINEAR					= 1 << 1,		// don't use tiled layout to allow fast CPU addressing
				PARTIALLY_RESIDENT		= 1 << 2,		// // PC, Durango, Orbis: partially resident
// 				CUBE_FILTER				= 1 << 3,		// perform power map mip level filtering on environment maps
				OVERLAY_MEMORY			= 1 << 4,		// allocate from the dedicated overlay memory block on consoles
				START_PURGED			= 1 << 5,		// don't do the Alloc() when created
				CAN_BE_UAV				= 1 << 6,		// can be bind as an UAV // ARKANE : nsilvagni () add an image view object
				DONT_SKIP_MIPS			= 1 << 7,		// don't skip higher mips when r_skipMipMaps > 0
				USE_ESRAM				= 1 << 8,		//ARKANE: gmarion (2013-09-23) - for Durango
				USE_HTILE_OR_CMASK		= 1 << 9,		// only for orbis for now
				STAGING_CPU_WRITE_ONLY	= 1 << 10,		// indicate the staging texture is cpu writable, used only for staging on PC_D3D which are read by default

				LAST_USED_BIT = STAGING_CPU_WRITE_ONLY
			}

			public TYPE			m_type;
			public PixelFormat	m_format = new PixelFormat();
			public uint			m_curWidth;
			public uint			m_curHeight;			// not needed for cube maps
			public uint			m_minWidth;
			public uint			m_minHeight;			// not needed for cube maps
			public uint			m_maxWidth;
			public uint			m_maxHeight;			// not needed for cube maps
			public uint			m_depth;				// only needed for 3D maps
			public uint			m_curNumLevels;
			public uint			m_pendingNumLevels;		// != curNumLevels if a streaming request / upload is pending
			public uint			m_minNumLevels;
			public uint			m_maxNumLevels;
			public uint			m_arraySize;
			public uint			m_numSamples;			// number of samples of a multisampled texture's image
			public ushort		m_flags;

			public void		SetFixedNumLevels( uint levels ) {
				m_curNumLevels = levels;
				m_minNumLevels = levels;
				m_maxNumLevels = levels;
				m_pendingNumLevels = levels;
			}
			public void		SetFixedWidth( uint width ) {
				m_curWidth = width;
				m_minWidth = width;
				m_maxWidth = width;
			}
			public void		SetFixedHeight( uint height ) {
				m_curHeight = height;
				m_minHeight = height;
				m_maxHeight = height;
			}

			public void		Read( BinaryReader _R ) {

				uint	version = _R.ReadUInt32();
				if ( version != SUPPORTED_IMAGEOPTS_VERSION )
					throw new Exception( "Unsupported image options version " + version + " (supported version is " + SUPPORTED_IMAGEOPTS_VERSION + ")!" );

				m_type = (TYPE) _R.ReadUInt32();
				m_format.Read( _R );

				m_minWidth = _R.ReadUInt32();
				m_minHeight = _R.ReadUInt32();
				m_maxWidth = _R.ReadUInt32();
				m_maxHeight = _R.ReadUInt32();
				m_depth = _R.ReadUInt32();
				m_minNumLevels = _R.ReadUInt32();
				m_maxNumLevels = _R.ReadUInt32();
				m_arraySize = _R.ReadUInt32();
				m_flags = _R.ReadUInt16();
			}
		}

		[System.Diagnostics.DebuggerDisplay( "Mip {m_MipLevel.ToString( \"G4\" )} {m_Width.ToString( \"G4\" )}x{m_Height.ToString( \"G4\" )} Slice {m_SliceIndex.ToString( \"G4\" )} Size={m_Content.Length}" )]
		public class	ImageSlice {

			public BImage	m_Owner = null;

			public uint		m_MipLevel;
			public uint		m_SliceIndex;
			public uint		m_Width;
			public uint		m_Height;

			public byte[]	m_Content = null;

			public ImageSlice( BImage _Owner, BinaryReader _R, uint _MipOffset ) {
				m_Owner = _Owner;
				Read( _R, _MipOffset );
			}

			public void		Read( BinaryReader _R, uint _MipOffset ) {
				m_MipLevel = _MipOffset + _R.ReadUInt32();
				m_SliceIndex = _R.ReadUInt32();
				m_Width = _R.ReadUInt32();
				m_Height = _R.ReadUInt32();

				int	ContentSize = (int) _R.ReadUInt32();
				if ( !m_Owner.m_Opts.m_format.IsDepth && ContentSize != m_Width * m_Height * (m_Owner.m_Opts.m_format.BitsCount >> 3) )
					throw new Exception( "Unexpected content size!" );

				m_Content = new byte[ContentSize];
				_R.Read( m_Content, 0, ContentSize );
			}
		}

		#endregion

		#region FIELDS

		public uint			m_sourceFileTime;
		public uint			m_Magic;
		public ImageOptions	m_Opts = new ImageOptions();
		public ImageSlice[]	m_Slices = new ImageSlice[0];

		#endregion

		#region METHODS

		public	BImage( System.IO.FileInfo _FileName ) {

			using ( FileStream S = _FileName.OpenRead() )
				using ( BinaryReader R = new BinaryReader( S ) ) {

					m_sourceFileTime = R.ReadUInt32();
					m_Magic = R.ReadUInt32();
					if ( m_Magic != MAGIC )
						throw new Exception( "Image has unsupported magic!" );

					m_Opts.Read( R );

					uint	mipsCountInFile = m_Opts.m_minNumLevels;
					uint	mipsCountTotal = m_Opts.m_maxNumLevels;

					List<ImageSlice>	Slices = new List<ImageSlice>();
					if ( mipsCountInFile < mipsCountTotal ) {
						// This means the largest mips are stored elsewhere, for streaming purpose...
						List<string>	MipFileNames = new List<string>();
						MipFileNames.AddRange( Directory.EnumerateFiles( _FileName.DirectoryName, _FileName.Name + "_mip*" ) );
						MipFileNames.Sort();
						foreach ( string MipFileName in MipFileNames ) {
							using ( FileStream MipS = new FileInfo( MipFileName ).OpenRead() )
								using ( BinaryReader MipR = new BinaryReader( MipS ) )
									Slices.Add( new ImageSlice( this, MipR, 0U ) );
						}
					}

					m_Opts.SetFixedNumLevels( mipsCountTotal );
					m_Opts.SetFixedWidth( Math.Max( 1U, m_Opts.m_maxWidth ) );
					m_Opts.SetFixedHeight( Math.Max( 1U, m_Opts.m_maxHeight ) );

					uint	totalSlicesInFile = mipsCountInFile * m_Opts.m_arraySize;
					if ( m_Opts.m_type == ImageOptions.TYPE.TT_3D ) {
						if ( mipsCountInFile != mipsCountTotal )
							throw new Exception( "Min & Max mips count are the same! Can't compute depth reduction on texture 3D!" );

						// ARKANE: bmayaux (2013-07-15) We need to account for depth reduction with each new mip level...
						totalSlicesInFile = 0;
						uint	depth = m_Opts.m_depth;
						for ( int mipLevelIndex = 0; mipLevelIndex < mipsCountInFile; mipLevelIndex++ ) {
							totalSlicesInFile += depth;
							depth = Math.Max( 1, depth >> 1 );
						}
					} else if ( m_Opts.m_type == ImageOptions.TYPE.TT_CUBIC ) {
						totalSlicesInFile *= 6;
					}

					uint	mipOffset = (uint) Slices.Count;
					for ( uint i = 0; i < totalSlicesInFile; i++ )
						Slices.Add( new ImageSlice( this, R, mipOffset ) );

					m_Slices = Slices.ToArray();
				}
		}

		/// <summary>
		/// Creates a 2D texture from the image
		/// </summary>
		/// <returns></returns>
		public Renderer.Texture2D	CreateTexture2D( Renderer.Device _Device ) {
			if ( m_Opts.m_type != ImageOptions.TYPE.TT_2D )
				throw new Exception( "The image is not a 2D texture!" );

			ImageUtility.PIXEL_FORMAT		textureFormat = ImageUtility.PIXEL_FORMAT.UNKNOWN;
			ImageUtility.COMPONENT_FORMAT	componentFormat;
			m_Opts.m_format.GetEquivalentRendererFormat( out textureFormat, out componentFormat );

			uint	ArraySize = m_Opts.m_arraySize;
			uint	MipsCount = m_Opts.m_curNumLevels;
			uint	PixelSize = m_Opts.m_format.BitsCount >> 3;

			List<Renderer.PixelsBuffer>	Content = new List<Renderer.PixelsBuffer>();
			for ( uint SliceIndex=0; SliceIndex < ArraySize; SliceIndex++ ) {
				for ( uint MipLevelIndex=0; MipLevelIndex < MipsCount; MipLevelIndex++ ) {
					ImageSlice	Slice = m_Slices[MipLevelIndex*ArraySize+SliceIndex];	// Stupidly stored in reverse order!

					Renderer.PixelsBuffer	Pixels = new Renderer.PixelsBuffer( (uint) (Slice.m_Width * Slice.m_Height * PixelSize) );
					Content.Add( Pixels );

					using ( BinaryWriter Writer = Pixels.OpenStreamWrite() )
						Writer.Write( Slice.m_Content );
				}
			}

			Renderer.Texture2D	Result = new Renderer.Texture2D( _Device, m_Opts.m_curWidth, m_Opts.m_curHeight, (int) m_Opts.m_arraySize, m_Opts.m_curNumLevels, textureFormat, componentFormat, false, false, Content.ToArray() );
			return Result;
		}

		public Renderer.Texture2D	CreateTextureCube( Renderer.Device _Device ) {
			if ( m_Opts.m_type != ImageOptions.TYPE.TT_CUBIC )
				throw new Exception( "The image is not a cube map texture!" );

			ImageUtility.PIXEL_FORMAT		textureFormat = ImageUtility.PIXEL_FORMAT.UNKNOWN;
			ImageUtility.COMPONENT_FORMAT	componentFormat;
			m_Opts.m_format.GetEquivalentRendererFormat( out textureFormat, out componentFormat );

			uint	ArraySize = 6 * m_Opts.m_arraySize;
			uint	MipsCount = m_Opts.m_curNumLevels;
			uint	PixelSize = m_Opts.m_format.BitsCount >> 3;

			List<Renderer.PixelsBuffer>	Content = new List<Renderer.PixelsBuffer>();
			for ( uint SliceIndex=0; SliceIndex < ArraySize; SliceIndex++ ) {
				for ( uint MipLevelIndex=0; MipLevelIndex < MipsCount; MipLevelIndex++ ) {
					ImageSlice	Slice = m_Slices[MipLevelIndex*ArraySize+SliceIndex];	// Stupidly stored in reverse order!

					Renderer.PixelsBuffer	Pixels = new Renderer.PixelsBuffer( (uint) (Slice.m_Width * Slice.m_Height * PixelSize) );
					Content.Add( Pixels );

					using ( BinaryWriter Writer = Pixels.OpenStreamWrite() )
						Writer.Write( Slice.m_Content );
				}
			}

			Renderer.Texture2D	Result = new Renderer.Texture2D( _Device, m_Opts.m_curWidth, m_Opts.m_curHeight, -6 * (int) m_Opts.m_arraySize, m_Opts.m_curNumLevels, textureFormat, componentFormat, false, false, Content.ToArray() );
			return Result;
		}

		public Renderer.Texture3D	CreateTexture3D( Renderer.Device _Device ) {
			if ( m_Opts.m_type != ImageOptions.TYPE.TT_3D )
				throw new Exception( "The image is not a 2D texture!" );

			throw new Exception( "Texture 3D are not supported yet!" );
		}

// 		private static ushort ReadBigUInt16( BinaryReader _R ) {
// 			ushort	value = _R.ReadUInt16();
// 			ushort	result = (ushort) (((value & 0x0000FF00U) >> 8) | ((value & 0x000000FFU) << 8));
// 			return result;
// 		}
// 
// 		private static uint	ReadBigUInt32( BinaryReader _R ) {
// 			uint	value = _R.ReadUInt32();
// 			uint	result = ((value & 0xFF000000U) >> 24) | ((value & 0x00FF0000U) >> 8) | ((value & 0x0000FF00U) << 8) | ((value & 0x000000FFU) << 24);
// 			return result;
// 		}

		#endregion
	}
}
