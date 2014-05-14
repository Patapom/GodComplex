using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using System.IO;
using System.Reflection;

using WMath;

namespace StandardizedDiffuseAlbedoMaps
{
	/// <summary>
	/// This class loads the CRW (Canon Raw) format as specified in http://xyrion.org/ciff/CIFFspecV1R04.pdf
	/// Raw image is decompressed following the algorithms found at http://cybercom.net/~dcoffin/dcraw/
	/// Additional infos at: http://www.sno.phy.queensu.ca/~phil/exiftool/canon_raw.html
	/// </summary>
	public class	CanonRawLoader : IDisposable
	{
		[StructLayout( LayoutKind.Sequential )]
		public struct	Header
		{
			public UInt16	ByteOrder;
			public UInt32	HeaderLength;
			public UInt64	Signature;
			public UInt32	Version;
			public UInt64	Reserved;

			public void		Check()
			{
				if ( ByteOrder != 0x4949 )				throw new Exception( "Unsupported byte order" );								// "ll" (Intel littel-endian)
				if ( HeaderLength != 0x1A )				throw new Exception( "Unexpected header length" );
				if ( Signature != 0x5244434350414548 )	throw new Exception( "Unexpected header signature: not a Canon Raw file?" );	// "HEAPCCDR"
				if ( Version != 0x00010002 )			throw new Exception( "Unsuppoted version" );									// We only support version 1.2
			}
		}

		[System.Diagnostics.DebuggerDisplay( "TagID={TagID} Offset={Offset} Size={Size} DataFormat={DataFormat} Location={DataLocation}" )]
		[StructLayout( LayoutKind.Sequential )]
		public struct	RecordEntry
		{
			public enum		STORAGE_TYPE
			{
				IN_HEAP_SPACE = 0,
				IN_RECORD_ENTRY = 1,
			}
			public enum		DATA_FORMAT
			{
				BYTE = 0,
				STRINGZ = 1,
				UINT16 = 2,
				UINT32_OR_FLOAT = 3,
				STRUCTURE = 4,
				SUBDIRECTORY = 5,
				SUBDIRECTORY2 = 6,
				UNKNOWN = 7,
			}

			public UInt16	Tag;
			public uint	Size;
			public uint	Offset;

			public STORAGE_TYPE	DataLocation		{ get { return (STORAGE_TYPE) ((Tag >> 14) & 0x3); } }
			public UInt16		TagIndex			{ get { return (UInt16) (Tag & 0x07FFU); } }
			public UInt16		TagID				{ get { return (UInt16) (Tag & 0x3FFFU); } }
			public DATA_FORMAT	DataFormat			{ get { return (DATA_FORMAT) ((Tag >> 11) & 0x7); } }

			public void		Check( long _ParentSize )
			{
				if ( Offset >= _ParentSize )		throw new Exception( "Sub-block start is larger than parent block size!" );
				if ( Offset+Size >= _ParentSize )	throw new Exception( "Sub-block end is larger than parent block size!" );
			}
		}

		public class	DataRecord
		{
			public RecordEntry	m_Entry;

			public BinaryReader	m_Reader = null;
			public long			m_Offset = 0;
			public long			m_Size = 0;

			public DataRecord[]	m_DataRecords = new DataRecord[0];

			public DataRecord( RecordEntry _Entry, BinaryReader R, long _Offset, long _Size )
			{
				m_Entry = _Entry;
				m_Reader = R;
				m_Offset = _Offset;
				m_Size = _Size;
			}

			/// <summary>
			/// Setups the reader at the position where that record is starting
			/// </summary>
			protected void	SetupReader()
			{
				SetupReader( 0 );
			}

			protected void	SetupReader( int _Offset )
			{
				m_Reader.BaseStream.Position = m_Offset + _Offset;
			}
		}

		public class	Directory : DataRecord
		{
			public Directory( RecordEntry _Entry, BinaryReader R, long _Offset, long _Size ) : base( _Entry, R, _Offset, _Size )
			{
				R.BaseStream.Position = _Offset + _Size - 4;
				uint	ValueDataSize = R.ReadUInt32();

				// Read sub-directories
				R.BaseStream.Position = _Offset + ValueDataSize;
				UInt16	DataRecordsCount = R.ReadUInt16();

				m_DataRecords = new DataRecord[DataRecordsCount];

				for ( int DataRecordIndex=0; DataRecordIndex < DataRecordsCount; DataRecordIndex++ )
				{
					long	DataRecordOffset = _Offset + ValueDataSize + 2 + DataRecordIndex * 10;
					R.BaseStream.Position = DataRecordOffset;

					RecordEntry	Entry = new RecordEntry();
					SRead( R, ref Entry );
					if ( Entry.DataLocation != RecordEntry.STORAGE_TYPE.IN_RECORD_ENTRY )
						Entry.Check( _Size );

					long	DataOffset = Entry.DataLocation == RecordEntry.STORAGE_TYPE.IN_HEAP_SPACE ? _Offset + Entry.Offset : DataRecordOffset + 2;
					long	DataSize = Entry.DataLocation == RecordEntry.STORAGE_TYPE.IN_HEAP_SPACE ? Entry.Size : 8;

					DataRecord	Record = null;
					if ( Entry.DataFormat == RecordEntry.DATA_FORMAT.SUBDIRECTORY || Entry.DataFormat == RecordEntry.DATA_FORMAT.SUBDIRECTORY2 )
					{
						switch ( Entry.TagID )
						{
							case 0x0000:	// NULL directory
								break;

							default:
								Record = new Directory( Entry, R, DataOffset, DataSize );
								break;
						}
					}
					else
					{	// Handle specific records
						// Add specific tag support at convenience
						R.BaseStream.Position = DataOffset;
						switch ( Entry.TagID )
						{
							case 0x2005:	// Main image data
								Record = new DataRecordRawData( Entry, R, DataOffset, DataSize );
								break;

							case 0x1031:	// Image size infos
								Record = new DataRecordImageSize( Entry, R, DataOffset, DataSize );
								break;

							case 0x10B4:	// Color profile infos
								Record = new DataRecordColorProfile( Entry, R, DataOffset, DataSize );
								break;

							case 0x1814:
								Record = new DataRecordEV( Entry, R, DataOffset, DataSize );
								break;

							case 0x1818:
								Record = new DataRecordExposureInfo( Entry, R, DataOffset, DataSize );
								break;

							case 0x1835:	// Decoder table for RAW format decoding
								Record = new DataRecordDecoderTable( Entry, R, DataOffset, DataSize );
								break;
						}
					}

					m_DataRecords[DataRecordIndex] = Record;
				}
			}

			/// <summary>
			/// Finds the first data record of the specified tag ID
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="_TagID"></param>
			/// <returns></returns>
			public T	FindDataRecord<T>( UInt16 _TagID ) where T:DataRecord
			{
				foreach ( DataRecord Child in m_DataRecords )
					if ( Child != null )
					{
						if ( Child is Directory )
						{	// Recurse through children
							T	Result = (Child as Directory).FindDataRecord<T>( _TagID );
							if ( Result != null )
								return Result;
						}
						else if ( Child.m_Entry.TagID == _TagID )
						{	// Found it!
							T	Result = Child as T;
							if ( Result == null )
								throw new Exception( "Found specified TagID but the data record is not of the type assumed by the caller!" );
							return Result;
						}
					}

				return null;
			}
		}

		/// <summary>
		/// Contains the bulk RAW data and decoder
		/// </summary>
		public class	DataRecordRawData : DataRecord
		{
			public UInt16[,]	m_DecodedImage = null;

			public DataRecordRawData( RecordEntry _Entry, BinaryReader R, long _Offset, long _Size ) : base( _Entry, R, _Offset, _Size )
			{
			}

			public UInt16[,]		DecodeRAW( DataRecordImageSize _ImageSize, DataRecordDecoderTable _Table )
			{
				if ( _ImageSize == null )
					throw new Exception( "You must provide a valid image size data record to Decode the RAW image!" );
				if ( _Table == null )
					throw new Exception( "You must provide a valid decoder table data record to Decode the RAW image!" );

				int	W = _ImageSize.m_Width;
				int	H = _ImageSize.m_Height;

				// If the image is uncompressed then skip the uncompressed part
				bool	LowBits = HasLowBits();
				if ( LowBits )
				{
					SetupReader( W*H/4 );
					throw new Exception( "RAW image is uncompressed 8-bits data so there's no real point to shoot RAW..." );	// I'm too lazy to handle that case, I'm only interested in 16 bits raw for my project!
				}
				else
					SetupReader();	// Reset stream position to the beginning of the record

				GetBits( ~0U );

				int		OutputBufferLength = ((W*H+63) & ~63) * sizeof(UInt16);
				byte[]	Outbuf = new byte[OutputBufferLength];
				using ( MemoryStream S = new MemoryStream( Outbuf ) )
				{
					uint	Carry=0, Column=0;
					uint[]	Base = new uint[2];

					DataRecordDecoderTable.DecodeTreeNode	Decode;
					DataRecordDecoderTable.DecodeTreeNode	DIndex;
					int		leaf, len;
					uint	diff;
					uint[]	diffbuf = new uint[64];

					while ( Column < W * H )
					{
						Array.Clear( diffbuf, 0, diffbuf.Length );
						Decode = _Table.m_FirstDecodeTree[0];

						for ( int i=0; i < 64; i++ )
						{
							for ( DIndex=Decode; DIndex.branch[0] != null; )
								DIndex = DIndex.branch[GetBits(1)];

							leaf = DIndex.leaf;
							Decode = _Table.m_SecondDecodeTree[0];

							if ( leaf == 0 && i != 0 )
								break;
							if ( leaf == 0xff )
								continue;

							i  += leaf >> 4;
							len = leaf & 15;
							if ( len == 0 )
								continue;
							diff = GetBits( (uint) len );

							if ( (diff & (1 << (len-1))) == 0 )
								diff -= (uint) ((1 << len) - 1);
							if ( i < 64 )
								diffbuf[i] = diff;
						}

						diffbuf[0] += Carry;
						Carry = diffbuf[0];
						for ( int i=0; i < 64; i++ )
						{
							if ( (Column++ % W) == 0 )
								Base[0] = Base[1] = 512;

							Base[i&1] += diffbuf[i];
//							Outbuf[i] = Base[i&1];

							S.WriteByte( (byte) (Base[i&1] & 0xFF) );
							S.WriteByte( (byte) ((Base[i&1] >> 8) & 0xFF) );
						}

// 						if ( LowBits )
// 						{
// 							save = ftell(ifp);
// 							fseek (ifp, (Column-64)/4 + 26, SEEK_SET);
// 							for (i=j=0; j < 64/4; j++ )
// 							{
// 								c = fgetc(ifp);
// 								for (r = 0; r < 8; r += 2)
// 									outbuf[i++] = (outbuf[i] << 2) + ((c >> r) & 3);
// 							}
// 							fseek(ifp, save, SEEK_SET);
// 						}
// 
// 						fwrite( Outbuf, 2, 64, stdout );
					}
				}



				return m_DecodedImage;
			}

			/// <summary>
			/// Return false if the image starts with compressed data, true if it starts with uncompressed low-order bits.
			/// 
			/// In Canon compressed data, 0xff is always followed by 0x00.
			/// </summary>
			/// <returns></returns>
			private bool	HasLowBits()
			{
				SetupReader();
				byte[]	test = m_Reader.ReadBytes( 0x4000 );
				for ( int i=540; i < test.Length-1; i++ )
					if ( test[i] == 0xff && test[i+1] != 0 )
						 return true;

				return false;
			}

			/// <summary>
			/// getbits(-1) initializes the buffer
			/// getbits(n) where n &lt;= 26 returns an n-bit integer
			/// </summary>
			/// <param name="_BitsCount"></param>
			uint	m_bitbuf = 0;
			uint	m_vbits = 0;
			private uint	GetBits( uint _BitsCount )
			{
				if ( _BitsCount == 0 )
					return 0;

				uint	result = 0;
				if ( _BitsCount == ~0U )
				{
					m_bitbuf = result = m_vbits = 0;
				}
				else
				{
					result = m_bitbuf << (int) (32U - m_vbits);
					result >>= (int) (32U - _BitsCount);
					m_vbits -= _BitsCount;
				}

				while ( m_vbits < 25 )
				{
					byte	c = m_Reader.ReadByte();
					m_bitbuf = (m_bitbuf << 8) + c;
					if ( c == 0xff )
						m_Reader.ReadByte();	// always extra 0x00 after 0xff
					m_vbits += 8;
				}

				return result;
			}

#region Source code from http://cybercom.net/~dcoffin/dcraw/decompress.c
// 	/*
//    Simple reference decompresser for Canon digital cameras.
//    Outputs raw 16-bit CCD data, no header, native byte order.
// 
//    $Revision: 1.12 $
//    $Date: 2004/08/06 00:08:01 $
// */
// 
// #include <stdio.h>
// #include <stdlib.h>
// #include <string.h>
// 
// typedef unsigned char uchar;
// 
// /* Global Variables */
// 
// FILE *ifp;
// short order;
// int height, width, table, lowbits;
// char name[64];
// 
// struct Decode {
//   struct Decode *branch[2];
//   int leaf;
// } first_decode[32], second_decode[512];
// 
// /*
//    Get a 2-byte integer, making no assumptions about CPU byte order.
//    Nor should we assume that the compiler evaluates left-to-right.
//  */
// short fget2 (FILE *f)
// {
//   register uchar a, b;
// 
//   a = fgetc(f);
//   b = fgetc(f);
//   if (order == 0x4d4d)		/* "MM" means big-endian */
//     return (a << 8) + b;
//   else				/* "II" means little-endian */
//     return a + (b << 8);
// }
// 
// /*
//    Same for a 4-byte integer.
//  */
// int fget4 (FILE *f)
// {
//   register uchar a, b, c, d;
// 
//   a = fgetc(f);
//   b = fgetc(f);
//   c = fgetc(f);
//   d = fgetc(f);
//   if (order == 0x4d4d)
//     return (a << 24) + (b << 16) + (c << 8) + d;
//   else
//     return a + (b << 8) + (c << 16) + (d << 24);
// }
// 
// /*
//    Parse the CIFF structure
//  */
// void parse (int offset, int length)
// {
//   int tboff, nrecs, i, type, len, roff, aoff, save;
// 
//   fseek (ifp, offset+length-4, SEEK_SET);
//   tboff = fget4(ifp) + offset;
//   fseek (ifp, tboff, SEEK_SET);
//   nrecs = fget2(ifp);
//   for (i = 0; i < nrecs; i++) {
//     type = fget2(ifp);
//     len  = fget4(ifp);
//     roff = fget4(ifp);
//     aoff = offset + roff;
//     save = ftell(ifp);
//     if (type == 0x080a) {		/* Get the camera name */
//       fseek (ifp, aoff, SEEK_SET);
//       while (fgetc(ifp));
//       fread (name, 64, 1, ifp);
//     }
//     if (type == 0x1031) {		/* Get the width and height */
//       fseek (ifp, aoff+2, SEEK_SET);
//       width  = fget2(ifp);
//       height = fget2(ifp);
//     }
//     if (type == 0x1835) {		/* Get the decoder table */
//       fseek (ifp, aoff, SEEK_SET);
//       table = fget4(ifp);
//     }
//     if (type >> 8 == 0x28 || type >> 8 == 0x30)	/* Get sub-tables */
//       parse (aoff, len);
//     fseek (ifp, save, SEEK_SET);
//   }
// }
// 
// /*
//    Return 0 if the image starts with compressed data,
//    1 if it starts with uncompressed low-order bits.
// 
//    In Canon compressed data, 0xff is always followed by 0x00.
//  */
// int canon_has_lowbits()
// {
//   uchar test[0x4000];
//   int ret=1, i;
// 
//   fseek (ifp, 0, SEEK_SET);
//   fread (test, 1, sizeof test, ifp);
//   for (i=540; i < sizeof test - 1; i++)
//     if (test[i] == 0xff) {
//       if (test[i+1]) return 1;
//       ret=0;
//     }
//   return ret;
// }
// 
// /*
//    Open a CRW file, identify which camera created it, and set
//    global variables accordingly.  Returns nonzero if an error occurs.
//  */
// int open_and_id(char *fname)
// {
//   char head[8];
//   int hlen;
// 
//   ifp = fopen(fname,"rb");
//   if (!ifp) {
//     perror(fname);
//     return 1;
//   }
//   order = fget2(ifp);
//   hlen  = fget4(ifp);
// 
//   fread (head, 1, 8, ifp);
//   if (memcmp(head,"HEAPCCDR",8) || (order != 0x4949 && order != 0x4d4d)) {
//     fprintf(stderr,"%s is not a Canon CRW file.\n",fname);
//     return 1;
//   }
// 
//   name[0] = 0;
//   table = -1;
//   fseek (ifp, 0, SEEK_END);
//   parse (hlen, ftell(ifp) - hlen);
//   lowbits = canon_has_lowbits();
// 
//   fprintf(stderr,"name = %s, width = %d, height = %d, table = %d, bpp = %d\n",
// 	name, width, height, table, 10+lowbits*2);
//   if (table < 0) {
//     fprintf(stderr,"Cannot decompress %s!!\n",fname);
//     return 1;
//   }
//   return 0;
// }
// 
// /*
//    A rough description of Canon's compression algorithm:
// 
// +  Each pixel outputs a 10-bit sample, from 0 to 1023.
// +  Split the data into blocks of 64 samples each.
// +  Subtract from each sample the value of the sample two positions
//    to the left, which has the same color filter.  From the two
//    leftmost samples in each row, subtract 512.
// +  For each nonzero sample, make a token consisting of two four-bit
//    numbers.  The low nibble is the number of bits required to
//    represent the sample, and the high nibble is the number of
//    zero samples preceding this sample.
// +  Output this token as a variable-length bitstring using
//    one of three tablesets.  Follow it with a fixed-length
//    bitstring containing the sample.
// 
//    The "first_decode" table is used for the first sample in each
//    block, and the "second_decode" table is used for the others.
//  */
// 
// /*
//    Construct a Decode tree according the specification in *source.
//    The first 16 bytes specify how many codes should be 1-bit, 2-bit
//    3-bit, etc.  Bytes after that are the leaf values.
// 
//    For example, if the source is
// 
//     { 0,1,4,2,3,1,2,0,0,0,0,0,0,0,0,0,
//       0x04,0x03,0x05,0x06,0x02,0x07,0x01,0x08,0x09,0x00,0x0a,0x0b,0xff  },
// 
//    then the code is
// 
// 	00		0x04
// 	010		0x03
// 	011		0x05
// 	100		0x06
// 	101		0x02
// 	1100		0x07
// 	1101		0x01
// 	11100		0x08
// 	11101		0x09
// 	11110		0x00
// 	111110		0x0a
// 	1111110		0x0b
// 	1111111		0xff
//  */
// void make_decoder(struct Decode *dest, const uchar *source, int level)
// {
//   static struct Decode *free;	/* Next unused node */
//   static int leaf;		/* no. of leaves already added */
//   int i, next;
// 
//   if (level==0) {
//     free = dest;
//     leaf = 0;
//   }
//   free++;
// /*
//    At what level should the next leaf appear?
//  */
//   for (i=next=0; i <= leaf && next < 16; )
//     i += source[next++];
// 
//   if (i > leaf)
//     if (level < next) {		/* Are we there yet? */
//       dest->branch[0] = free;
//       make_decoder(free,source,level+1);
//       dest->branch[1] = free;
//       make_decoder(free,source,level+1);
//     } else
//       dest->leaf = source[16 + leaf++];
// }
// 
// void init_tables(unsigned table)
// {
//   static const uchar first_tree[3][29] = {
//     { 0,1,4,2,3,1,2,0,0,0,0,0,0,0,0,0,
//       0x04,0x03,0x05,0x06,0x02,0x07,0x01,0x08,0x09,0x00,0x0a,0x0b,0xff  },
// 
//     { 0,2,2,3,1,1,1,1,2,0,0,0,0,0,0,0,
//       0x03,0x02,0x04,0x01,0x05,0x00,0x06,0x07,0x09,0x08,0x0a,0x0b,0xff  },
// 
//     { 0,0,6,3,1,1,2,0,0,0,0,0,0,0,0,0,
//       0x06,0x05,0x07,0x04,0x08,0x03,0x09,0x02,0x00,0x0a,0x01,0x0b,0xff  },
//   };
// 
//   static const uchar second_tree[3][180] = {
//     { 0,2,2,2,1,4,2,1,2,5,1,1,0,0,0,139,
//       0x03,0x04,0x02,0x05,0x01,0x06,0x07,0x08,
//       0x12,0x13,0x11,0x14,0x09,0x15,0x22,0x00,0x21,0x16,0x0a,0xf0,
//       0x23,0x17,0x24,0x31,0x32,0x18,0x19,0x33,0x25,0x41,0x34,0x42,
//       0x35,0x51,0x36,0x37,0x38,0x29,0x79,0x26,0x1a,0x39,0x56,0x57,
//       0x28,0x27,0x52,0x55,0x58,0x43,0x76,0x59,0x77,0x54,0x61,0xf9,
//       0x71,0x78,0x75,0x96,0x97,0x49,0xb7,0x53,0xd7,0x74,0xb6,0x98,
//       0x47,0x48,0x95,0x69,0x99,0x91,0xfa,0xb8,0x68,0xb5,0xb9,0xd6,
//       0xf7,0xd8,0x67,0x46,0x45,0x94,0x89,0xf8,0x81,0xd5,0xf6,0xb4,
//       0x88,0xb1,0x2a,0x44,0x72,0xd9,0x87,0x66,0xd4,0xf5,0x3a,0xa7,
//       0x73,0xa9,0xa8,0x86,0x62,0xc7,0x65,0xc8,0xc9,0xa1,0xf4,0xd1,
//       0xe9,0x5a,0x92,0x85,0xa6,0xe7,0x93,0xe8,0xc1,0xc6,0x7a,0x64,
//       0xe1,0x4a,0x6a,0xe6,0xb3,0xf1,0xd3,0xa5,0x8a,0xb2,0x9a,0xba,
//       0x84,0xa4,0x63,0xe5,0xc5,0xf3,0xd2,0xc4,0x82,0xaa,0xda,0xe4,
//       0xf2,0xca,0x83,0xa3,0xa2,0xc3,0xea,0xc2,0xe2,0xe3,0xff,0xff  },
// 
//     { 0,2,2,1,4,1,4,1,3,3,1,0,0,0,0,140,
//       0x02,0x03,0x01,0x04,0x05,0x12,0x11,0x06,
//       0x13,0x07,0x08,0x14,0x22,0x09,0x21,0x00,0x23,0x15,0x31,0x32,
//       0x0a,0x16,0xf0,0x24,0x33,0x41,0x42,0x19,0x17,0x25,0x18,0x51,
//       0x34,0x43,0x52,0x29,0x35,0x61,0x39,0x71,0x62,0x36,0x53,0x26,
//       0x38,0x1a,0x37,0x81,0x27,0x91,0x79,0x55,0x45,0x28,0x72,0x59,
//       0xa1,0xb1,0x44,0x69,0x54,0x58,0xd1,0xfa,0x57,0xe1,0xf1,0xb9,
//       0x49,0x47,0x63,0x6a,0xf9,0x56,0x46,0xa8,0x2a,0x4a,0x78,0x99,
//       0x3a,0x75,0x74,0x86,0x65,0xc1,0x76,0xb6,0x96,0xd6,0x89,0x85,
//       0xc9,0xf5,0x95,0xb4,0xc7,0xf7,0x8a,0x97,0xb8,0x73,0xb7,0xd8,
//       0xd9,0x87,0xa7,0x7a,0x48,0x82,0x84,0xea,0xf4,0xa6,0xc5,0x5a,
//       0x94,0xa4,0xc6,0x92,0xc3,0x68,0xb5,0xc8,0xe4,0xe5,0xe6,0xe9,
//       0xa2,0xa3,0xe3,0xc2,0x66,0x67,0x93,0xaa,0xd4,0xd5,0xe7,0xf8,
//       0x88,0x9a,0xd7,0x77,0xc4,0x64,0xe2,0x98,0xa5,0xca,0xda,0xe8,
//       0xf3,0xf6,0xa9,0xb2,0xb3,0xf2,0xd2,0x83,0xba,0xd3,0xff,0xff  },
// 
//     { 0,0,6,2,1,3,3,2,5,1,2,2,8,10,0,117,
//       0x04,0x05,0x03,0x06,0x02,0x07,0x01,0x08,
//       0x09,0x12,0x13,0x14,0x11,0x15,0x0a,0x16,0x17,0xf0,0x00,0x22,
//       0x21,0x18,0x23,0x19,0x24,0x32,0x31,0x25,0x33,0x38,0x37,0x34,
//       0x35,0x36,0x39,0x79,0x57,0x58,0x59,0x28,0x56,0x78,0x27,0x41,
//       0x29,0x77,0x26,0x42,0x76,0x99,0x1a,0x55,0x98,0x97,0xf9,0x48,
//       0x54,0x96,0x89,0x47,0xb7,0x49,0xfa,0x75,0x68,0xb6,0x67,0x69,
//       0xb9,0xb8,0xd8,0x52,0xd7,0x88,0xb5,0x74,0x51,0x46,0xd9,0xf8,
//       0x3a,0xd6,0x87,0x45,0x7a,0x95,0xd5,0xf6,0x86,0xb4,0xa9,0x94,
//       0x53,0x2a,0xa8,0x43,0xf5,0xf7,0xd4,0x66,0xa7,0x5a,0x44,0x8a,
//       0xc9,0xe8,0xc8,0xe7,0x9a,0x6a,0x73,0x4a,0x61,0xc7,0xf4,0xc6,
//       0x65,0xe9,0x72,0xe6,0x71,0x91,0x93,0xa6,0xda,0x92,0x85,0x62,
//       0xf3,0xc5,0xb2,0xa4,0x84,0xba,0x64,0xa5,0xb3,0xd2,0x81,0xe5,
//       0xd3,0xaa,0xc4,0xca,0xf2,0xb1,0xe4,0xd1,0x83,0x63,0xea,0xc3,
//       0xe2,0x82,0xf1,0xa3,0xc2,0xa1,0xc1,0xe3,0xa2,0xe1,0xff,0xff  }
//   };
// 
//   if (table > 2) table = 2;
//   memset( first_decode, 0, sizeof first_decode);
//   memset(second_decode, 0, sizeof second_decode);
//   make_decoder( first_decode,  first_tree[table], 0);
//   make_decoder(second_decode, second_tree[table], 0);
// }
// 
// #if 0
// writebits (int val, int nbits)
// {
//   val <<= 32 - nbits;
//   while (nbits--) {
//     putchar(val & 0x80000000 ? '1':'0');
//     val <<= 1;
//   }
// }
// #endif
// 
// /*
//    getbits(-1) initializes the buffer
//    getbits(n) where 0 <= n <= 25 returns an n-bit integer
// */
// unsigned long getbits(int nbits)
// {
//   static unsigned long bitbuf=0, ret=0;
//   static int vbits=0;
//   unsigned char c;
// 
//   if (nbits == 0) return 0;
//   if (nbits == -1)
//     ret = bitbuf = vbits = 0;
//   else {
//     ret = bitbuf << (32 - vbits) >> (32 - nbits);
//     vbits -= nbits;
//   }
//   while (vbits < 25) {
//     c=fgetc(ifp);
//     bitbuf = (bitbuf << 8) + c;
//     if (c == 0xff) fgetc(ifp);	/* always extra 00 after ff */
//     vbits += 8;
//   }
//   return ret;
// }
// 
// int main(int argc, char **argv)
// {
//   struct Decode *Decode, *DIndex;
//   int i, j, leaf, len, diff, diffbuf[64], r, save;
//   int Carry=0, Column=0, base[2];
//   unsigned short outbuf[64];
//   uchar c;
// 
//   if (argc < 2) {
//     fprintf(stderr,"Usage:  %s file.crw\n",argv[0]);
//     exit(1);
//   }
//   if (open_and_id(argv[1]))
//     exit(1);
// 
//   init_tables(table);
// 
//   fseek (ifp, 540 + lowbits*height*width/4, SEEK_SET);
//   getbits(-1);			/* Prime the bit buffer */
// 
//   while (Column < width * height) {
//     memset(diffbuf,0,sizeof diffbuf);
//     Decode = first_decode;
//     for (i=0; i < 64; i++ ) {
// 
//       for (DIndex=Decode; DIndex.branch[0]; )
// 	DIndex = DIndex.branch[getbits(1)];
//       leaf = DIndex.leaf;
//       Decode = second_decode;
// 
//       if (leaf == 0 && i) break;
//       if (leaf == 0xff) continue;
//       i  += leaf >> 4;
//       len = leaf & 15;
//       if (len == 0) continue;
//       diff = getbits(len);
//       if ((diff & (1 << (len-1))) == 0)
// 	diff -= (1 << len) - 1;
//       if (i < 64) diffbuf[i] = diff;
//     }
//     diffbuf[0] += Carry;
//     Carry = diffbuf[0];
//     for (i=0; i < 64; i++ ) {
//       if (Column++ % width == 0)
// 	base[0] = base[1] = 512;
//       outbuf[i] = ( base[i & 1] += diffbuf[i] );
//     }
//     if (lowbits) {
//       save = ftell(ifp);
//       fseek (ifp, (Column-64)/4 + 26, SEEK_SET);
//       for (i=j=0; j < 64/4; j++ ) {
// 	c = fgetc(ifp);
// 	for (r = 0; r < 8; r += 2)
// 	  outbuf[i++] = (outbuf[i] << 2) + ((c >> r) & 3);
//       }
//       fseek (ifp, save, SEEK_SET);
//     }
//     fwrite(outbuf,2,64,stdout);
//   }
//   return 0;
// }

#endregion
		}

		/// <summary>
		/// Contains the size of the image
		/// </summary>
		public class	DataRecordImageSize : DataRecord
		{
			public int	m_Width;
			public int	m_Height;

			public DataRecordImageSize( RecordEntry _Entry, BinaryReader R, long _Offset, long _Size ) : base( _Entry, R, _Offset, _Size )
			{
				m_Width = R.ReadUInt16();
				m_Height = R.ReadUInt16();
			}
		}

		/// <summary>
		/// Contains the color profile of the image
		/// </summary>
		public class	DataRecordColorProfile : DataRecord
		{
			public enum		COLOR_PROFILE
			{
				sRGB,
				ADOBE_RGB,
				UNCALIBRATED
			}
			public COLOR_PROFILE	m_ColorProfile = COLOR_PROFILE.UNCALIBRATED;

			public DataRecordColorProfile( RecordEntry _Entry, BinaryReader R, long _Offset, long _Size ) : base( _Entry, R, _Offset, _Size )
			{
				UInt16	ColorProfileRaw = R.ReadUInt16();
				switch ( ColorProfileRaw )
				{
					case 1: m_ColorProfile = COLOR_PROFILE.sRGB; break;
					case 2: m_ColorProfile = COLOR_PROFILE.ADOBE_RGB; break;
				}
			}
		}

		/// <summary>
		/// Contains the exposure info of the image (exposure compensation, Tv, Av)
		/// </summary>
		public class	DataRecordExposureInfo : DataRecord
		{
			public DataRecordExposureInfo( RecordEntry _Entry, BinaryReader R, long _Offset, long _Size ) : base( _Entry, R, _Offset, _Size )
			{
			}
		}

		/// <summary>
		/// Contains the EV value of the image
		/// </summary>
		public class	DataRecordEV : DataRecord
		{
			public float	m_EV = 0.0f;
			public DataRecordEV( RecordEntry _Entry, BinaryReader R, long _Offset, long _Size ) : base( _Entry, R, _Offset, _Size )
			{
				m_EV = R.ReadSingle();
			}
		}

		/// <summary>
		/// Contains the decoder table to Decode the RAW image
		/// </summary>
		public class	DataRecordDecoderTable : DataRecord
		{
			#region Decoder Tables

			private static readonly byte[][]	FIRST_TREE = new byte[3][] {
				new byte[29] { 0,1,4,2,3,1,2,0,0,0,0,0,0,0,0,0,
					0x04,0x03,0x05,0x06,0x02,0x07,0x01,0x08,0x09,0x00,0x0a,0x0b,0xff  },

				new byte[29] { 0,2,2,3,1,1,1,1,2,0,0,0,0,0,0,0,
					0x03,0x02,0x04,0x01,0x05,0x00,0x06,0x07,0x09,0x08,0x0a,0x0b,0xff  },

				new byte[29] { 0,0,6,3,1,1,2,0,0,0,0,0,0,0,0,0,
					0x06,0x05,0x07,0x04,0x08,0x03,0x09,0x02,0x00,0x0a,0x01,0x0b,0xff  },
			};

			public static readonly byte[][]	SECOND_TREE = new byte[3][] {
				new byte[180] { 0,2,2,2,1,4,2,1,2,5,1,1,0,0,0,139,
					0x03,0x04,0x02,0x05,0x01,0x06,0x07,0x08,
					0x12,0x13,0x11,0x14,0x09,0x15,0x22,0x00,0x21,0x16,0x0a,0xf0,
					0x23,0x17,0x24,0x31,0x32,0x18,0x19,0x33,0x25,0x41,0x34,0x42,
					0x35,0x51,0x36,0x37,0x38,0x29,0x79,0x26,0x1a,0x39,0x56,0x57,
					0x28,0x27,0x52,0x55,0x58,0x43,0x76,0x59,0x77,0x54,0x61,0xf9,
					0x71,0x78,0x75,0x96,0x97,0x49,0xb7,0x53,0xd7,0x74,0xb6,0x98,
					0x47,0x48,0x95,0x69,0x99,0x91,0xfa,0xb8,0x68,0xb5,0xb9,0xd6,
					0xf7,0xd8,0x67,0x46,0x45,0x94,0x89,0xf8,0x81,0xd5,0xf6,0xb4,
					0x88,0xb1,0x2a,0x44,0x72,0xd9,0x87,0x66,0xd4,0xf5,0x3a,0xa7,
					0x73,0xa9,0xa8,0x86,0x62,0xc7,0x65,0xc8,0xc9,0xa1,0xf4,0xd1,
					0xe9,0x5a,0x92,0x85,0xa6,0xe7,0x93,0xe8,0xc1,0xc6,0x7a,0x64,
					0xe1,0x4a,0x6a,0xe6,0xb3,0xf1,0xd3,0xa5,0x8a,0xb2,0x9a,0xba,
					0x84,0xa4,0x63,0xe5,0xc5,0xf3,0xd2,0xc4,0x82,0xaa,0xda,0xe4,
					0xf2,0xca,0x83,0xa3,0xa2,0xc3,0xea,0xc2,0xe2,0xe3,0xff,0xff  },

				new byte[180] { 0,2,2,1,4,1,4,1,3,3,1,0,0,0,0,140,
					0x02,0x03,0x01,0x04,0x05,0x12,0x11,0x06,
					0x13,0x07,0x08,0x14,0x22,0x09,0x21,0x00,0x23,0x15,0x31,0x32,
					0x0a,0x16,0xf0,0x24,0x33,0x41,0x42,0x19,0x17,0x25,0x18,0x51,
					0x34,0x43,0x52,0x29,0x35,0x61,0x39,0x71,0x62,0x36,0x53,0x26,
					0x38,0x1a,0x37,0x81,0x27,0x91,0x79,0x55,0x45,0x28,0x72,0x59,
					0xa1,0xb1,0x44,0x69,0x54,0x58,0xd1,0xfa,0x57,0xe1,0xf1,0xb9,
					0x49,0x47,0x63,0x6a,0xf9,0x56,0x46,0xa8,0x2a,0x4a,0x78,0x99,
					0x3a,0x75,0x74,0x86,0x65,0xc1,0x76,0xb6,0x96,0xd6,0x89,0x85,
					0xc9,0xf5,0x95,0xb4,0xc7,0xf7,0x8a,0x97,0xb8,0x73,0xb7,0xd8,
					0xd9,0x87,0xa7,0x7a,0x48,0x82,0x84,0xea,0xf4,0xa6,0xc5,0x5a,
					0x94,0xa4,0xc6,0x92,0xc3,0x68,0xb5,0xc8,0xe4,0xe5,0xe6,0xe9,
					0xa2,0xa3,0xe3,0xc2,0x66,0x67,0x93,0xaa,0xd4,0xd5,0xe7,0xf8,
					0x88,0x9a,0xd7,0x77,0xc4,0x64,0xe2,0x98,0xa5,0xca,0xda,0xe8,
					0xf3,0xf6,0xa9,0xb2,0xb3,0xf2,0xd2,0x83,0xba,0xd3,0xff,0xff  },

				new byte[180] { 0,0,6,2,1,3,3,2,5,1,2,2,8,10,0,117,
					0x04,0x05,0x03,0x06,0x02,0x07,0x01,0x08,
					0x09,0x12,0x13,0x14,0x11,0x15,0x0a,0x16,0x17,0xf0,0x00,0x22,
					0x21,0x18,0x23,0x19,0x24,0x32,0x31,0x25,0x33,0x38,0x37,0x34,
					0x35,0x36,0x39,0x79,0x57,0x58,0x59,0x28,0x56,0x78,0x27,0x41,
					0x29,0x77,0x26,0x42,0x76,0x99,0x1a,0x55,0x98,0x97,0xf9,0x48,
					0x54,0x96,0x89,0x47,0xb7,0x49,0xfa,0x75,0x68,0xb6,0x67,0x69,
					0xb9,0xb8,0xd8,0x52,0xd7,0x88,0xb5,0x74,0x51,0x46,0xd9,0xf8,
					0x3a,0xd6,0x87,0x45,0x7a,0x95,0xd5,0xf6,0x86,0xb4,0xa9,0x94,
					0x53,0x2a,0xa8,0x43,0xf5,0xf7,0xd4,0x66,0xa7,0x5a,0x44,0x8a,
					0xc9,0xe8,0xc8,0xe7,0x9a,0x6a,0x73,0x4a,0x61,0xc7,0xf4,0xc6,
					0x65,0xe9,0x72,0xe6,0x71,0x91,0x93,0xa6,0xda,0x92,0x85,0x62,
					0xf3,0xc5,0xb2,0xa4,0x84,0xba,0x64,0xa5,0xb3,0xd2,0x81,0xe5,
					0xd3,0xaa,0xc4,0xca,0xf2,0xb1,0xe4,0xd1,0x83,0x63,0xea,0xc3,
					0xe2,0x82,0xf1,0xa3,0xc2,0xa1,0xc1,0xe3,0xa2,0xe1,0xff,0xff  }
			  };

			#endregion

			public class	DecodeTreeNode
			{
				public DecodeTreeNode[]	branch = new DecodeTreeNode[2];
				public int				leaf;
			};
			public DecodeTreeNode[]	m_FirstDecodeTree = new DecodeTreeNode[32];
			public DecodeTreeNode[]	m_SecondDecodeTree = new DecodeTreeNode[512];

			public DataRecordDecoderTable( RecordEntry _Entry, BinaryReader R, long _Offset, long _Size ) : base( _Entry, R, _Offset, _Size )
			{
				int	TableIndex = R.ReadInt32();
				if ( TableIndex < 0 ) throw new Exception( "Invalid decoder table! Can't Decode RAW image" );
				TableIndex = Math.Min( 2, TableIndex );

				m_FirstDecodeTree[0] = new DecodeTreeNode();
				BuildDecodeTree( m_FirstDecodeTree, 0, FIRST_TREE[TableIndex], 0 );

				m_SecondDecodeTree[0] = new DecodeTreeNode();
				BuildDecodeTree( m_SecondDecodeTree, 0, SECOND_TREE[TableIndex], 0 );
			}

			/// <summary>
			///    A rough description of Canon's compression algorithm:
			/// 
			/// +  Each pixel outputs a 10-bit sample, from 0 to 1023.
			/// +  Split the data into blocks of 64 samples each.
			/// +  Subtract from each sample the value of the sample two positions
			///    to the left, which has the same color filter.  From the two
			///    leftmost samples in each row, subtract 512.
			/// +  For each nonzero sample, make a token consisting of two four-bit
			///    numbers.  The low nibble is the number of bits required to
			///    represent the sample, and the high nibble is the number of
			///    zero samples preceding this sample.
			/// +  Output this token as a variable-length bitstring using
			///    one of three tablesets.  Follow it with a fixed-length
			///    bitstring containing the sample.
			/// 
			///    The "first_decode" table is used for the first sample in each
			///    block, and the "second_decode" table is used for the others.
			/// 
			/// 
			///    Construct a Decode tree according the specification in *source.
			///    The first 16 bytes specify how many codes should be 1-bit, 2-bit
			///    3-bit, etc.  Bytes after that are the leaf values.
			/// 
			///    For example, if the source is
			/// 
			///     { 0,1,4,2,3,1,2,0,0,0,0,0,0,0,0,0,
			///       0x04,0x03,0x05,0x06,0x02,0x07,0x01,0x08,0x09,0x00,0x0a,0x0b,0xff  },
			/// 
			///    then the code is
			/// 
			/// 	00			0x04
			/// 	010			0x03
			/// 	011			0x05
			/// 	100			0x06
			/// 	101			0x02
			/// 	1100		0x07
			/// 	1101		0x01
			/// 	11100		0x08
			/// 	11101		0x09
			/// 	11110		0x00
			/// 	111110		0x0a
			/// 	1111110		0x0b
			/// 	1111111		0xff
			/// </summary>
			public void		Decode()
			{

			}

			/// <summary>
			/// Construct a decode tree according the specification in *source.
			/// The first 16 bytes specify how many codes should be 1-bit, 2-bit, 3-bit, etc.  Bytes after that are the leaf values.
			/// 
			///    For example, if the source is
			/// 
			///     { 0,1,4,2,3,1,2,0,0,0,0,0,0,0,0,0,
			///       0x04,0x03,0x05,0x06,0x02,0x07,0x01,0x08,0x09,0x00,0x0a,0x0b,0xff  },
			/// 
			///    then the code is
			/// 
			/// 	00			0x04
			/// 	010			0x03
			/// 	011			0x05
			/// 	100			0x06
			/// 	101			0x02
			/// 	1100		0x07
			/// 	1101		0x01
			/// 	11100		0x08
			/// 	11101		0x09
			/// 	11110		0x00
			/// 	111110		0x0a
			/// 	1111110		0x0b
			/// 	1111111		0xff
			/// </summary>
			private int		m_FreeNodeIndex = 0;
			private int		m_LeafNodeIndex = 0;
			private void	BuildDecodeTree( DecodeTreeNode[] _TreeNodes, int _CurrentNodeIndex, byte[] _Source, int _Level )
			{
				if ( _Level == 0 )
				{	// Initialize global variables for this tree
					m_FreeNodeIndex = 0;
					m_LeafNodeIndex = 0;
				}

				DecodeTreeNode	CurrentNode = _TreeNodes[_CurrentNodeIndex];

				// Use a new free node
				m_FreeNodeIndex++;
				_TreeNodes[m_FreeNodeIndex] = new DecodeTreeNode();

				// At what level should the next leaf appear?
				int	i, next;
				for ( i=next=0; i <= m_LeafNodeIndex && next < 16; )
					i += _Source[next++];

				if ( i > m_LeafNodeIndex )
				{
					if ( _Level < next )
					{	// Are we there yet?
						CurrentNode.branch[0] = _TreeNodes[m_FreeNodeIndex];
						BuildDecodeTree( _TreeNodes, m_FreeNodeIndex, _Source, _Level+1 );

						CurrentNode.branch[1] = _TreeNodes[m_FreeNodeIndex];
						BuildDecodeTree( _TreeNodes, m_FreeNodeIndex, _Source, _Level+1 );
					} else
						CurrentNode.leaf = _Source[16 + m_LeafNodeIndex++];
				}

#region Original Code
//    Construct a decode tree according the specification in *source.
//    The first 16 bytes specify how many codes should be 1-bit, 2-bit
//    3-bit, etc.  Bytes after that are the leaf values.
// 
//    For example, if the source is
// 
//     { 0,1,4,2,3,1,2,0,0,0,0,0,0,0,0,0,
//       0x04,0x03,0x05,0x06,0x02,0x07,0x01,0x08,0x09,0x00,0x0a,0x0b,0xff  },
// 
//    then the code is
// 
// 	00		0x04
// 	010		0x03
// 	011		0x05
// 	100		0x06
// 	101		0x02
// 	1100		0x07
// 	1101		0x01
// 	11100		0x08
// 	11101		0x09
// 	11110		0x00
// 	111110		0x0a
// 	1111110		0x0b
// 	1111111		0xff
//  */
// ushort * CLASS make_decoder_ref (const uchar **source)
// {
//   int max, len, h, i, j;
//   const uchar *count;
//   ushort *huff;
// 
//   count = (*source += 16) - 17;
//   for (max=16; max && !count[max]; max--);
//   huff = (ushort *) calloc (1 + (1 << max), sizeof *huff);
//   merror (huff, "make_decoder()");
//   huff[0] = max;
//   for (h=len=1; len <= max; len++)
//     for (i=0; i < count[len]; i++, ++*source)
//       for (j=0; j < 1 << (max-len); j++)
// 	if (h <= 1 << max)
// 	  huff[h++] = len << 8 | **source;
//   return huff;
// }
// 
// ushort * CLASS make_decoder (const uchar *source)
// {
//   return make_decoder_ref (&source);
// }
// 
// void CLASS crw_init_tables (unsigned table, ushort *huff[2])
// {
//   static const uchar first_tree[3][29] = {
//     { 0,1,4,2,3,1,2,0,0,0,0,0,0,0,0,0,
//       0x04,0x03,0x05,0x06,0x02,0x07,0x01,0x08,0x09,0x00,0x0a,0x0b,0xff  },
//     { 0,2,2,3,1,1,1,1,2,0,0,0,0,0,0,0,
//       0x03,0x02,0x04,0x01,0x05,0x00,0x06,0x07,0x09,0x08,0x0a,0x0b,0xff  },
//     { 0,0,6,3,1,1,2,0,0,0,0,0,0,0,0,0,
//       0x06,0x05,0x07,0x04,0x08,0x03,0x09,0x02,0x00,0x0a,0x01,0x0b,0xff  },
//   };
//   static const uchar second_tree[3][180] = {
//     { 0,2,2,2,1,4,2,1,2,5,1,1,0,0,0,139,
//       0x03,0x04,0x02,0x05,0x01,0x06,0x07,0x08,
//       0x12,0x13,0x11,0x14,0x09,0x15,0x22,0x00,0x21,0x16,0x0a,0xf0,
//       0x23,0x17,0x24,0x31,0x32,0x18,0x19,0x33,0x25,0x41,0x34,0x42,
//       0x35,0x51,0x36,0x37,0x38,0x29,0x79,0x26,0x1a,0x39,0x56,0x57,
//       0x28,0x27,0x52,0x55,0x58,0x43,0x76,0x59,0x77,0x54,0x61,0xf9,
//       0x71,0x78,0x75,0x96,0x97,0x49,0xb7,0x53,0xd7,0x74,0xb6,0x98,
//       0x47,0x48,0x95,0x69,0x99,0x91,0xfa,0xb8,0x68,0xb5,0xb9,0xd6,
//       0xf7,0xd8,0x67,0x46,0x45,0x94,0x89,0xf8,0x81,0xd5,0xf6,0xb4,
//       0x88,0xb1,0x2a,0x44,0x72,0xd9,0x87,0x66,0xd4,0xf5,0x3a,0xa7,
//       0x73,0xa9,0xa8,0x86,0x62,0xc7,0x65,0xc8,0xc9,0xa1,0xf4,0xd1,
//       0xe9,0x5a,0x92,0x85,0xa6,0xe7,0x93,0xe8,0xc1,0xc6,0x7a,0x64,
//       0xe1,0x4a,0x6a,0xe6,0xb3,0xf1,0xd3,0xa5,0x8a,0xb2,0x9a,0xba,
//       0x84,0xa4,0x63,0xe5,0xc5,0xf3,0xd2,0xc4,0x82,0xaa,0xda,0xe4,
//       0xf2,0xca,0x83,0xa3,0xa2,0xc3,0xea,0xc2,0xe2,0xe3,0xff,0xff  },
//     { 0,2,2,1,4,1,4,1,3,3,1,0,0,0,0,140,
//       0x02,0x03,0x01,0x04,0x05,0x12,0x11,0x06,
//       0x13,0x07,0x08,0x14,0x22,0x09,0x21,0x00,0x23,0x15,0x31,0x32,
//       0x0a,0x16,0xf0,0x24,0x33,0x41,0x42,0x19,0x17,0x25,0x18,0x51,
//       0x34,0x43,0x52,0x29,0x35,0x61,0x39,0x71,0x62,0x36,0x53,0x26,
//       0x38,0x1a,0x37,0x81,0x27,0x91,0x79,0x55,0x45,0x28,0x72,0x59,
//       0xa1,0xb1,0x44,0x69,0x54,0x58,0xd1,0xfa,0x57,0xe1,0xf1,0xb9,
//       0x49,0x47,0x63,0x6a,0xf9,0x56,0x46,0xa8,0x2a,0x4a,0x78,0x99,
//       0x3a,0x75,0x74,0x86,0x65,0xc1,0x76,0xb6,0x96,0xd6,0x89,0x85,
//       0xc9,0xf5,0x95,0xb4,0xc7,0xf7,0x8a,0x97,0xb8,0x73,0xb7,0xd8,
//       0xd9,0x87,0xa7,0x7a,0x48,0x82,0x84,0xea,0xf4,0xa6,0xc5,0x5a,
//       0x94,0xa4,0xc6,0x92,0xc3,0x68,0xb5,0xc8,0xe4,0xe5,0xe6,0xe9,
//       0xa2,0xa3,0xe3,0xc2,0x66,0x67,0x93,0xaa,0xd4,0xd5,0xe7,0xf8,
//       0x88,0x9a,0xd7,0x77,0xc4,0x64,0xe2,0x98,0xa5,0xca,0xda,0xe8,
//       0xf3,0xf6,0xa9,0xb2,0xb3,0xf2,0xd2,0x83,0xba,0xd3,0xff,0xff  },
//     { 0,0,6,2,1,3,3,2,5,1,2,2,8,10,0,117,
//       0x04,0x05,0x03,0x06,0x02,0x07,0x01,0x08,
//       0x09,0x12,0x13,0x14,0x11,0x15,0x0a,0x16,0x17,0xf0,0x00,0x22,
//       0x21,0x18,0x23,0x19,0x24,0x32,0x31,0x25,0x33,0x38,0x37,0x34,
//       0x35,0x36,0x39,0x79,0x57,0x58,0x59,0x28,0x56,0x78,0x27,0x41,
//       0x29,0x77,0x26,0x42,0x76,0x99,0x1a,0x55,0x98,0x97,0xf9,0x48,
//       0x54,0x96,0x89,0x47,0xb7,0x49,0xfa,0x75,0x68,0xb6,0x67,0x69,
//       0xb9,0xb8,0xd8,0x52,0xd7,0x88,0xb5,0x74,0x51,0x46,0xd9,0xf8,
//       0x3a,0xd6,0x87,0x45,0x7a,0x95,0xd5,0xf6,0x86,0xb4,0xa9,0x94,
//       0x53,0x2a,0xa8,0x43,0xf5,0xf7,0xd4,0x66,0xa7,0x5a,0x44,0x8a,
//       0xc9,0xe8,0xc8,0xe7,0x9a,0x6a,0x73,0x4a,0x61,0xc7,0xf4,0xc6,
//       0x65,0xe9,0x72,0xe6,0x71,0x91,0x93,0xa6,0xda,0x92,0x85,0x62,
//       0xf3,0xc5,0xb2,0xa4,0x84,0xba,0x64,0xa5,0xb3,0xd2,0x81,0xe5,
//       0xd3,0xaa,0xc4,0xca,0xf2,0xb1,0xe4,0xd1,0x83,0x63,0xea,0xc3,
//       0xe2,0x82,0xf1,0xa3,0xc2,0xa1,0xc1,0xe3,0xa2,0xe1,0xff,0xff  }
//   };
//   if (table > 2) table = 2;
//   huff[0] = make_decoder ( first_tree[table]);
//   huff[1] = make_decoder (second_tree[table]);
// }
#endregion
			}
		}


		public Header		m_Header;
		public Directory	m_RootDir;

		public DataRecordRawData	m_RAWImage = null;

		#region METHODS 

		public	CanonRawLoader( Stream _Stream )
		{
			using ( BinaryReader R = new BinaryReader( _Stream ) )
			{
				SRead( R, ref m_Header );
				m_Header.Check();

				m_RootDir = new Directory( new RecordEntry(), R, _Stream.Position, _Stream.Length - _Stream.Position );

				// Retrieve the essential bits of information
				DataRecordImageSize		ImageSize = m_RootDir.FindDataRecord<DataRecordImageSize>( 0x1031 );
				if ( ImageSize == null )
					throw new Exception( "Can't retrieve image size" );

				DataRecordDecoderTable	DecoderTable = m_RootDir.FindDataRecord<DataRecordDecoderTable>( 0x1835 );
				if ( DecoderTable == null )
					throw new Exception( "Can't retrieve decoder table" );

				m_RAWImage  = m_RootDir.FindDataRecord<DataRecordRawData>( 0x2005 );
				if ( m_RAWImage == null )
					throw new Exception( "Can't retrieve RAW image data" );

				// Proceed with decoding
				m_RAWImage.DecodeRAW( ImageSize, DecoderTable );
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IO Helpers

		private static void	SRead( BinaryReader R, out byte value )		{ value = R.ReadByte(); }
		private static void	SRead( BinaryReader R, out UInt16 value )	{ value = R.ReadUInt16(); }
		private static void	SRead( BinaryReader R, out uint value )	{ value = R.ReadUInt32(); }
		private static void	SRead( BinaryReader R, out UInt64 value )	{ value = R.ReadUInt64(); }

		private static void	SRead<T>( BinaryReader R, ref T value ) where T:struct
		{
			object	BoxedInstance = (object) value;	// Struct needs to be boxed to be referenced and not passed by value...

			FieldInfo[]	Fields = typeof(T).GetFields( BindingFlags.Instance | BindingFlags.Public );
			foreach ( FieldInfo Field in Fields )
			{
				Type	FieldType = Field.FieldType;
				string	FieldTypeName = FieldType.Name;
				switch ( FieldTypeName )
				{
					case "bool":
						Field.SetValue( BoxedInstance, R.ReadBoolean() );
						break;
					case "byte":
						Field.SetValue( BoxedInstance, R.ReadByte() );
						break;
					case "UInt16":
						Field.SetValue( BoxedInstance, R.ReadUInt16() );
						break;
					case "UInt32":
						Field.SetValue( BoxedInstance, R.ReadUInt32() );
						break;
					case "UInt64":
						Field.SetValue( BoxedInstance, R.ReadUInt64() );
						break;
					case "char":
						Field.SetValue( BoxedInstance, R.ReadChar() );
						break;
					case "Int16":
						Field.SetValue( BoxedInstance, R.ReadInt16() );
						break;
					case "Int32":
						Field.SetValue( BoxedInstance, R.ReadInt32() );
						break;
					case "Int64":
						Field.SetValue( BoxedInstance, R.ReadInt64() );
						break;
					default:
						throw new Exception( "Unsupported field type!" );
				}
			}

			value = (T) BoxedInstance;
		}

		#endregion

		#endregion
	}
}
