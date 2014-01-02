using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Nuaj.Cirrus.Utility
{
	/// <summary>
	/// This class supports loading and saving POM image files which are usually the direct result of a DirectX Map() call
	/// It has an homonym class in the GodComplex C++ project and as such should always mirror the C++ class if the format is to ever change...
	/// </summary>
	public class TextureFilePOM
	{
		#region NESTED TYPES

		public enum		TYPE
		{
			TEX_2D = 0,		// 2D
			TEX_CUBE = 1,	// CUBE
			TEX_3D = 2,		// 3D
		}

		public enum		DXGI_FORMAT
		{
			DXGI_FORMAT_UNKNOWN	                    = 0,
			DXGI_FORMAT_R32G32B32A32_TYPELESS       = 1,
			DXGI_FORMAT_R32G32B32A32_FLOAT          = 2,
			DXGI_FORMAT_R32G32B32A32_UINT           = 3,
			DXGI_FORMAT_R32G32B32A32_SINT           = 4,
			DXGI_FORMAT_R32G32B32_TYPELESS          = 5,
			DXGI_FORMAT_R32G32B32_FLOAT             = 6,
			DXGI_FORMAT_R32G32B32_UINT              = 7,
			DXGI_FORMAT_R32G32B32_SINT              = 8,
			DXGI_FORMAT_R16G16B16A16_TYPELESS       = 9,
			DXGI_FORMAT_R16G16B16A16_FLOAT          = 10,
			DXGI_FORMAT_R16G16B16A16_UNORM          = 11,
			DXGI_FORMAT_R16G16B16A16_UINT           = 12,
			DXGI_FORMAT_R16G16B16A16_SNORM          = 13,
			DXGI_FORMAT_R16G16B16A16_SINT           = 14,
			DXGI_FORMAT_R32G32_TYPELESS             = 15,
			DXGI_FORMAT_R32G32_FLOAT                = 16,
			DXGI_FORMAT_R32G32_UINT                 = 17,
			DXGI_FORMAT_R32G32_SINT                 = 18,
			DXGI_FORMAT_R32G8X24_TYPELESS           = 19,
			DXGI_FORMAT_D32_FLOAT_S8X24_UINT        = 20,
			DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS    = 21,
			DXGI_FORMAT_X32_TYPELESS_G8X24_UINT     = 22,
			DXGI_FORMAT_R10G10B10A2_TYPELESS        = 23,
			DXGI_FORMAT_R10G10B10A2_UNORM           = 24,
			DXGI_FORMAT_R10G10B10A2_UINT            = 25,
			DXGI_FORMAT_R11G11B10_FLOAT             = 26,
			DXGI_FORMAT_R8G8B8A8_TYPELESS           = 27,
			DXGI_FORMAT_R8G8B8A8_UNORM              = 28,
			DXGI_FORMAT_R8G8B8A8_UNORM_SRGB         = 29,
			DXGI_FORMAT_R8G8B8A8_UINT               = 30,
			DXGI_FORMAT_R8G8B8A8_SNORM              = 31,
			DXGI_FORMAT_R8G8B8A8_SINT               = 32,
			DXGI_FORMAT_R16G16_TYPELESS             = 33,
			DXGI_FORMAT_R16G16_FLOAT                = 34,
			DXGI_FORMAT_R16G16_UNORM                = 35,
			DXGI_FORMAT_R16G16_UINT                 = 36,
			DXGI_FORMAT_R16G16_SNORM                = 37,
			DXGI_FORMAT_R16G16_SINT                 = 38,
			DXGI_FORMAT_R32_TYPELESS                = 39,
			DXGI_FORMAT_D32_FLOAT                   = 40,
			DXGI_FORMAT_R32_FLOAT                   = 41,
			DXGI_FORMAT_R32_UINT                    = 42,
			DXGI_FORMAT_R32_SINT                    = 43,
			DXGI_FORMAT_R24G8_TYPELESS              = 44,
			DXGI_FORMAT_D24_UNORM_S8_UINT           = 45,
			DXGI_FORMAT_R24_UNORM_X8_TYPELESS       = 46,
			DXGI_FORMAT_X24_TYPELESS_G8_UINT        = 47,
			DXGI_FORMAT_R8G8_TYPELESS               = 48,
			DXGI_FORMAT_R8G8_UNORM                  = 49,
			DXGI_FORMAT_R8G8_UINT                   = 50,
			DXGI_FORMAT_R8G8_SNORM                  = 51,
			DXGI_FORMAT_R8G8_SINT                   = 52,
			DXGI_FORMAT_R16_TYPELESS                = 53,
			DXGI_FORMAT_R16_FLOAT                   = 54,
			DXGI_FORMAT_D16_UNORM                   = 55,
			DXGI_FORMAT_R16_UNORM                   = 56,
			DXGI_FORMAT_R16_UINT                    = 57,
			DXGI_FORMAT_R16_SNORM                   = 58,
			DXGI_FORMAT_R16_SINT                    = 59,
			DXGI_FORMAT_R8_TYPELESS                 = 60,
			DXGI_FORMAT_R8_UNORM                    = 61,
			DXGI_FORMAT_R8_UINT                     = 62,
			DXGI_FORMAT_R8_SNORM                    = 63,
			DXGI_FORMAT_R8_SINT                     = 64,
			DXGI_FORMAT_A8_UNORM                    = 65,
			DXGI_FORMAT_R1_UNORM                    = 66,
			DXGI_FORMAT_R9G9B9E5_SHAREDEXP          = 67,
			DXGI_FORMAT_R8G8_B8G8_UNORM             = 68,
			DXGI_FORMAT_G8R8_G8B8_UNORM             = 69,
			DXGI_FORMAT_BC1_TYPELESS                = 70,
			DXGI_FORMAT_BC1_UNORM                   = 71,
			DXGI_FORMAT_BC1_UNORM_SRGB              = 72,
			DXGI_FORMAT_BC2_TYPELESS                = 73,
			DXGI_FORMAT_BC2_UNORM                   = 74,
			DXGI_FORMAT_BC2_UNORM_SRGB              = 75,
			DXGI_FORMAT_BC3_TYPELESS                = 76,
			DXGI_FORMAT_BC3_UNORM                   = 77,
			DXGI_FORMAT_BC3_UNORM_SRGB              = 78,
			DXGI_FORMAT_BC4_TYPELESS                = 79,
			DXGI_FORMAT_BC4_UNORM                   = 80,
			DXGI_FORMAT_BC4_SNORM                   = 81,
			DXGI_FORMAT_BC5_TYPELESS                = 82,
			DXGI_FORMAT_BC5_UNORM                   = 83,
			DXGI_FORMAT_BC5_SNORM                   = 84,
			DXGI_FORMAT_B5G6R5_UNORM                = 85,
			DXGI_FORMAT_B5G5R5A1_UNORM              = 86,
			DXGI_FORMAT_B8G8R8A8_UNORM              = 87,
			DXGI_FORMAT_B8G8R8X8_UNORM              = 88,
			DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM  = 89,
			DXGI_FORMAT_B8G8R8A8_TYPELESS           = 90,
			DXGI_FORMAT_B8G8R8A8_UNORM_SRGB         = 91,
			DXGI_FORMAT_B8G8R8X8_TYPELESS           = 92,
			DXGI_FORMAT_B8G8R8X8_UNORM_SRGB         = 93,
			DXGI_FORMAT_BC6H_TYPELESS               = 94,
			DXGI_FORMAT_BC6H_UF16                   = 95,
			DXGI_FORMAT_BC6H_SF16                   = 96,
			DXGI_FORMAT_BC7_TYPELESS                = 97,
			DXGI_FORMAT_BC7_UNORM                   = 98,
			DXGI_FORMAT_BC7_UNORM_SRGB              = 99,
		}

		public struct	MipDescriptor 
		{
			public int	RowPitch;
			public int	DepthPitch;
		}

		#endregion

		#region FIELDS

		public TYPE				m_Type;
		public DXGI_FORMAT		m_Format = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN;
		public int				m_Width = 0;
		public int				m_Height = 0;
		public int				m_ArraySizeOrDepth = 0;
		public int				m_MipsCount = 0;

		public byte[][]			m_Content = null;			// Should contain as many slices as array slices and mips, for 3D textures should contain as many mips of cube infos (not sliced, entire cubes)
		public MipDescriptor[]	m_MipsDescriptors = null;	// Should contain as many descriptors as mip levels

		#endregion

		#region METHODS

		public void	Load( FileInfo _FileName )
		{
			using ( FileStream S = _FileName.OpenRead() )
				using ( BinaryReader R = new BinaryReader( S ) )
				{
					// Read the type and format
					m_Type = (TYPE) R.ReadByte();
					m_Format = (DXGI_FORMAT) R.ReadByte();

					// Read the dimensions
					m_Width = (int) R.ReadUInt32();
					m_Height = (int) R.ReadUInt32();
					m_ArraySizeOrDepth = (int) R.ReadUInt32();
					m_MipsCount = (int) R.ReadUInt32();

					int	ContentBuffersCount = m_Type == TYPE.TEX_3D ? m_MipsCount : m_MipsCount*m_ArraySizeOrDepth;
					m_Content = new byte[ContentBuffersCount][];
					m_MipsDescriptors = new MipDescriptor[m_MipsCount];

					// Read each mip
					int	Depth = m_ArraySizeOrDepth;
					for ( int MipLevelIndex=0; MipLevelIndex < m_MipsCount; MipLevelIndex++ )
					{
						m_MipsDescriptors[MipLevelIndex].RowPitch = (int) R.ReadUInt32();
						m_MipsDescriptors[MipLevelIndex].DepthPitch = (int) R.ReadUInt32();

						if ( m_Type != TYPE.TEX_3D )
						{
							for ( int SliceIndex=0; SliceIndex < m_ArraySizeOrDepth; SliceIndex++ )
								m_Content[MipLevelIndex+m_MipsCount*SliceIndex] = R.ReadBytes( m_MipsDescriptors[MipLevelIndex].DepthPitch );
						}
						else
							m_Content[MipLevelIndex] = R.ReadBytes( Depth * m_MipsDescriptors[MipLevelIndex].DepthPitch );

						Depth = Math.Max( 1, Depth >> 1 );
					}
				}
		}

		public void	Save( FileInfo _FileName )
		{
			using ( FileStream S = _FileName.Create() )
				using ( BinaryWriter W = new BinaryWriter( S ) )
				{
					// Write the type and format
					W.Write( (byte) m_Type );
					W.Write( (byte) m_Format );

					// Write the dimensions
					W.Write( (UInt32) m_Width );
					W.Write( (UInt32) m_Height );
					W.Write( (UInt32) m_ArraySizeOrDepth );
					W.Write( (UInt32) m_MipsCount );

					// Write each mip
					int	Depth = m_ArraySizeOrDepth;
					for ( int MipLevelIndex=0; MipLevelIndex < m_MipsCount; MipLevelIndex++ )
					{
						W.Write( (UInt32) m_MipsDescriptors[MipLevelIndex].RowPitch );
						W.Write( (UInt32) m_MipsDescriptors[MipLevelIndex].DepthPitch );

						if ( m_Type != TYPE.TEX_3D )
						{
							for ( int SliceIndex=0; SliceIndex < m_ArraySizeOrDepth; SliceIndex++ )
								W.Write( m_Content[MipLevelIndex+m_MipsCount*SliceIndex] );
						}
						else
							W.Write( Depth * m_MipsDescriptors[MipLevelIndex].DepthPitch );

						Depth = Math.Max( 1, Depth >> 1 );
					}
				}
		}

		#endregion
	}
}
