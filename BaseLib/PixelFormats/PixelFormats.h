//////////////////////////////////////////////////////////////////////////
// The structures declared here allow us to wrap the standard DXGI types and use standardized Read()/Write() methods to address the pixels
// The goal here is to always be capable of abstracting a pixel format as float4
// Also, a Read() immediately followed by a Write() should yield exactly the original value
//
////////////////////////////////////////////////////////////////////////////
//
#pragma once

#include <dxgiformat.h>

namespace BaseLib {

	#pragma pack(push,1)

	// This enum matches most of the the classes available as IPixelAccessor's (which in turn match the DXGI formats)
	//
	// Some formats are not supported natively by the FreeImage library (e.g. RG8, RG16, RG32, etc.) so I made them use the format
	//	whose size is immediately larger (e.g. RG8 use RGB8, RG16 uses RGB16, etc.) and allocating larger scanlines each time.
	// Here is an example of a scanline representation for a 8x2 RG8 image, internally supported by an RGB8 image:
	//
	//	RGB8 Pixel Index	[  0  |  1  |  2  |  3  |  4  |  5  |  6  |  7  ]
	//		Scanline 0 >	|R|G|R|G|R|G|R|G|R|G|R|G|R|G|R|G| | | | | | | | |
	//		Scanline 1 >	|R|G|R|G|R|G|R|G|R|G|R|G|R|G|R|G| | | | | | | | |
	//
	// We lose some memory (scanlines are usually 33% larger) but in turn we get these advantages:
	//	• We can use images with the appropriate width (i.e. I didn't try to reduce the width to have the least possible memory overhead otherwise the user would have to deal with nightmarish width conversions and pixel size management)
	//	• We can still read and write sequentially to each scanline without having to skip some components (cf. figure above), we just need to skip the appropriate padding at the end of each scanline
	//	• Loading a DDS file into a texture, or writing a texture into a DDS file is quite painless
	//	• Creating procedural DDS files or textures with these formats is also quite easy
	//
	// The only problem with these formats is that they cannot be loaded, saved or converted by FreeImage methods otherwise you will get garbage!
	//
	enum class PIXEL_FORMAT : U32 {
		UNKNOWN = ~0U,
		NOT_NATIVELY_SUPPORTED	= 0x80000000U,	// This flag is used by formats that are not natively supported by the FreeImage library
		RAW_BUFFER				= 0x40000000U,	// This flag is used to indicate raw buffer formats that are not directly mappable to a recognized pixel format (e.g. compressed formats)
		COMPRESSED				= 0x20000000U,	// This flag is used by compressed formats that are only supported by DDS images

		// 8-bits
		R8		= 0,
		RG8		= 1		| NOT_NATIVELY_SUPPORTED,	// FreeImage thinks it's R5G6B5! Aliased as RGBA8
		RGB8	= 2,
		RGBA8	= 3,

		// 16-bits
		R16		= 4,
		RG16	= 5		| NOT_NATIVELY_SUPPORTED,	// Unsupported by FreeImage, aliased as RGBA16
		RGB16	= 6,
		RGBA16	= 7,

		// 16-bits half-precision floating points
		// WARNING: These formats are NOT natively supported by FreeImage but can be used by DDS for example so I chose
		//			 to support them as regular U16 formats but treating the raw U16 as half-floats internally...
		// NOTE: These are NOT loadable or saveable by the regular Load()/Save() routine, this won't crash but it will produce garbage
		//		 These formats should only be used for in-memory manipulations and DDS-related routines that can manipulate them
		//
		R16F	= 8		| NOT_NATIVELY_SUPPORTED,	// Unsupported by FreeImage, aliased as R16_UNORM
		RG16F	= 9		| NOT_NATIVELY_SUPPORTED,	// Unsupported by FreeImage, aliased as RGB16_UNORM
		RGB16F	= 10	| NOT_NATIVELY_SUPPORTED,	// Unsupported by FreeImage, aliased as RGB16_UNORM
		RGBA16F	= 11	| NOT_NATIVELY_SUPPORTED,	// Unsupported by FreeImage, aliased as RGBA16_UNORM

		// 32-bits
		R32F	= 12,
		RG32F	= 13	| NOT_NATIVELY_SUPPORTED,	// Unsupported by FreeImage, aliased as RGBA32F
		RGB32F	= 14,
		RGBA32F = 15,

		// This is the "raw compressed format" used to support compressed or otherwise unsupported pixel formats like DirectX BCx formats (only used by DDS images)
		BC4		= 256	| RAW_BUFFER | COMPRESSED,	// Only supported by DDS, raw buffered images
		BC5		= 257	| RAW_BUFFER | COMPRESSED,	// Only supported by DDS, raw buffered images
		BC6H	= 258	| RAW_BUFFER | COMPRESSED,	// Only supported by DDS, raw buffered images
		BC7		= 259	| RAW_BUFFER | COMPRESSED,	// Only supported by DDS, raw buffered images
	};

	// Additional information about how the individual components of a pixel structure should be treated
	enum class COMPONENT_FORMAT {
		AUTO,	// Default value, will select UNORM for integer types and FLOAT for floating-point types
		UNORM,
		UNORM_sRGB,
		SNORM,
		UINT,
		SINT,
	};

	// Additional information about how the individual components of a pixel structure should be treated (for depth stencil)
	enum class	DEPTH_COMPONENT_FORMAT {
		DEPTH_ONLY,
		DEPTH_STENCIL,
	};

	// This is the interface to access pixel format structures needed for images and textures
	class	IPixelAccessor {
	public:
		// Gives the size of the pixel format in bytes
		virtual U32		Size() const abstract;

		// LDR pixel writer
		virtual void	Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const abstract;
		virtual void	Write( void* _pixel, U32 _A ) const abstract;

		// HDR pixel writer
		virtual void	Write( void* _pixel, const bfloat4& _Color ) const abstract;
		virtual void	Write( void* _pixel, float _R, float _G, float _B, float _A ) const abstract;
		virtual void	Write( void* _pixel, float _A ) const abstract;

		// HDR pixel readers
		virtual float	Red( const void* _pixel ) const abstract;
		virtual float	Green( const void* _pixel ) const abstract;
		virtual float	Blue( const void* _pixel ) const abstract;
		virtual float	Alpha( const void* _pixel ) const abstract;
		virtual void	RGBA( const void* _pixel, bfloat4& _Color ) const abstract;

	protected:	// HELPERS

		// Converts a U8 component to a [0,1] float component
		inline static float		U8toF32( U32 _Component ) {
			return _Component / 255.0f;
		}

		// Converts a U16 component to a [0,1] float component
		inline static float		U16toF32( U32 _Component ) {
			return _Component / 65535.0f;
		}

		// Converts a U32 component to a [0,1] float component
		inline static float		U32toF32( U32 _Component ) {
			return _Component / 4294967295.0f;	// / (2^32-1)
		}

		// Converts a [0,1] float component to a U8 component
		inline static U8		F32toU8( float _Component ) {
			return U8( CLAMP( _Component * 255.0f, 0.0f, 255.0f ) );
		}

		// Converts a [0,1] float component to a U16 component
		inline static U16		F32toU16( float _Component ) {
			return U16( CLAMP( _Component * 65535.0f, 0.0f, 65535.0f ) );
		}

		// Converts a [0,1] float component to a U32 component
		inline static U32		F32toU32( float _Component ) {
			return U32( _Component * 4294967295.0f );	// / (2^32-1)
		}
	};

	
	// This is the interface to access depth format structures needed for depth stencil buffers
	class	IDepthAccessor {
		// Gives the size of the depth format in bytes
		virtual U32		Size() const abstract;

		// Tells if the format uses stencil bits
		virtual bool	HasStencil() const abstract;

		virtual void	Write( void* _pixel, float _Depth, U8 _Stencil ) abstract;
		virtual float	Depth( const void* _pixel ) const abstract;
		virtual U8		Stencil( const void* _pixel ) const abstract;
		virtual void	DepthStencil( const void* _pixel, float& _Depth, U8& _Stencil ) const abstract;
	};

	//////////////////////////////////////////////////////////////////////////
	// Helpers
	//

	// Easily converts an image's PIXEL_FORMAT into a generic pixel accessor/descriptor
	extern const IPixelAccessor&	PixelFormat2PixelAccessor( PIXEL_FORMAT _pixelFormat );
	extern PIXEL_FORMAT		PixelAccessor2PixelFormat( const IPixelAccessor& _pixelAccessor );

	// Conversion to and from DXGI pixel formats and standard image pixel formats
 	extern PIXEL_FORMAT		DXGIFormat2PixelFormat( DXGI_FORMAT _sourceFormat, COMPONENT_FORMAT& _componentFormat, U32& _pixelSize );
 	extern DXGI_FORMAT		PixelFormat2DXGIFormat( PIXEL_FORMAT _sourceFormat, COMPONENT_FORMAT _componentFormat );
 	extern DXGI_FORMAT		DepthFormat2DXGIFormat( PIXEL_FORMAT _sourceFormat, DEPTH_COMPONENT_FORMAT _depthComponentFormat );


	//////////////////////////////////////////////////////////////////////////
	// Actual IPixelAccessor implementations
	//
 	struct	PF_Unknown {
		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32			Size() const override			{ return 0; }

			void		Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {}
			void		Write( void* _pixel, const bfloat4& _Color ) const override {}
			void		Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {}
			void		Write( void* _pixel, U32 _A ) const override {}
			void		Write( void* _pixel, float _A ) const override {}

			float		Red( const void* _pixel ) const override					{ return 0; }
			float		Green( const void* _pixel ) const override					{ return 0; }
			float		Blue( const void* _pixel ) const override					{ return 0; }
			float		Alpha( const void* _pixel ) const override					{ return 1;}
			void		RGBA( const void* _pixel, bfloat4& _Color ) const override	{}

		}	Descriptor;
		#pragma endregion
 	};

	#pragma region 8-Bits Formats

	// R8 format
	struct	PF_R8 {
		U8	R;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32			Size() const override			{ return 1; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_R8& P = *((PF_R8*) _pixel);
				P.R = U8(_R);
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_R8& P = *((PF_R8*) _pixel);
				P.R = F32toU8(_Color.x);
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_R8& P = *((PF_R8*) _pixel);
				P.R = F32toU8( _R );
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return ((PF_R8*) _pixel)->R / 255.0f; }
			float	Green( const void* _pixel ) const override					{ return 0.0f; }
			float	Blue( const void* _pixel ) const override					{ return 0.0f; }
			float	Alpha( const void* _pixel ) const override					{ return 1.0f; }
			// Here I'm taking the risk of returning a grayscale image... :/
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ float	v = ((PF_R8*) _pixel)->R / 255.0f; _Color.Set( v, v, v, 1 ); }

		}	Descriptor;
		#pragma endregion
	};

	// RG8 format
	struct PF_RG8 {
		U8	R, G;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 2; }

			void	Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RG8& P = *((PF_RG8*) _pixel);
				P.R = (U8) _R;
				P.G = (U8) _G;
			}

			void	Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RG8& P = *((PF_RG8*) _pixel);
				P.R = F32toU8(_Color.x);
				P.G = F32toU8(_Color.y);
			}

			void	Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RG8& P = *((PF_RG8*) _pixel);
				P.R = F32toU8( _R );
				P.G = F32toU8( _G );
			}

			void	Write( void* _pixel, U32 _A ) const override {
			}

			void	Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return ((PF_RG8*) _pixel)->R / 255.0f; }
			float	Green( const void* _pixel ) const override					{ return ((PF_RG8*) _pixel)->G / 255.0f; }
			float	Blue( const void* _pixel ) const override					{ return 0.0f; }
			float	Alpha( const void* _pixel ) const override					{ return 1.0f; }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( ((PF_RG8*) _pixel)->R / 255.0f, ((PF_RG8*) _pixel)->G / 255.0f, 0, 1 ); }

		} Descriptor;
		#pragma endregion
	};

	// RGB8 format
	struct	PF_RGB8 {
		U8	B, G, R;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 3; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RGB8& P = *((PF_RGB8*) _pixel);
				P.R = (U8) _R;
				P.G = (U8) _G;
				P.B = (U8) _B;
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RGB8& P = *((PF_RGB8*) _pixel);
				P.R = F32toU8(_Color.x);
				P.G = F32toU8(_Color.y);
				P.B = F32toU8(_Color.z);
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RGB8& P = *((PF_RGB8*) _pixel);
				P.R = F32toU8( _R );
				P.G = F32toU8( _G );
				P.B = F32toU8( _B );
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return ((PF_RGB8*) _pixel)->R / 255.0f; }
			float	Green( const void* _pixel ) const override					{ return ((PF_RGB8*) _pixel)->G / 255.0f; }
			float	Blue( const void* _pixel ) const override					{ return ((PF_RGB8*) _pixel)->B / 255.0f; }
			float	Alpha( const void* _pixel ) const override					{ return 1.0f; }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( ((PF_RGB8*) _pixel)->R / 255.0f, ((PF_RGB8*) _pixel)->G / 255.0f, ((PF_RGB8*) _pixel)->B / 255.0f, 1 ); }

		} Descriptor;
		#pragma endregion
	};

	// RGBA8 format
	struct	PF_RGBA8 {
		U8	B, G, R, A;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 4; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RGBA8& P = *((PF_RGBA8*) _pixel);
				P.R = (U8) _R;
				P.G = (U8) _G;
				P.B = (U8) _B;
				P.A = (U8) _A;
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RGBA8& P = *((PF_RGBA8*) _pixel);
				P.R = F32toU8(_Color.x);
				P.G = F32toU8(_Color.y);
				P.B = F32toU8(_Color.z);
				P.A = F32toU8(_Color.w);
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RGBA8& P = *((PF_RGBA8*) _pixel);
				P.R = F32toU8( _R );
				P.G = F32toU8( _G );
				P.B = F32toU8( _B );
				P.A = F32toU8( _A );
			}

			void Write( void* _pixel, U32 _A ) const override {	PF_RGBA8& P = *((PF_RGBA8*) _pixel);
				P.A = (U8) _A;
			}

			void Write( void* _pixel, float _A ) const override {	PF_RGBA8& P = *((PF_RGBA8*) _pixel);
				P.A = F32toU8( _A );
			}

			float	Red( const void* _pixel ) const override					{ return ((PF_RGBA8*) _pixel)->R / 255.0f; }
			float	Green( const void* _pixel ) const override					{ return ((PF_RGBA8*) _pixel)->G / 255.0f; }
			float	Blue( const void* _pixel ) const override					{ return ((PF_RGBA8*) _pixel)->B / 255.0f; }
			float	Alpha( const void* _pixel ) const override					{ return ((PF_RGBA8*) _pixel)->A / 255.0f; }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( ((PF_RGBA8*) _pixel)->R / 255.0f, ((PF_RGBA8*) _pixel)->G / 255.0f, ((PF_RGBA8*) _pixel)->B / 255.0f, ((PF_RGBA8*) _pixel)->A / 255.0f ); }

		} Descriptor;
		#pragma endregion
	};

	// RGBA8_sRGB format
// 	struct	PF_RGBA8_sRGB {
// 		U8	B, G, R, A;
// 
// 		#pragma region IPixelAccessor
// 		static class desc_t : public IPixelAccessor {
// 		public:
// 
// //			bool	sRGB() const override { return true; }
// 			U32		Size() const override { return 4; }
// 
// 			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RGBA8_sRGB& P = *((PF_RGBA8_sRGB*) _pixel);
// 				P.R = (U8) _R;
// 				P.G = (U8) _G;
// 				P.B = (U8) _B;
// 				P.A = (U8) _A;
// 			}
// 
// 			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RGBA8_sRGB& P = *((PF_RGBA8_sRGB*) _pixel);
// 				P.R = F32toU8(_Color.x);
// 				P.G = F32toU8(_Color.y);
// 				P.B = F32toU8(_Color.z);
// 				P.A = F32toU8(_Color.w);
// 			}
// 
// 			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RGBA8_sRGB& P = *((PF_RGBA8_sRGB*) _pixel);
// 				P.R = F32toU8( _R );
// 				P.G = F32toU8( _G );
// 				P.B = F32toU8( _B );
// 				P.A = F32toU8( _A );
// 			}
// 
// 			void Write( void* _pixel, U32 _A ) const override {	PF_RGBA8_sRGB& P = *((PF_RGBA8_sRGB*) _pixel);
// 				P.A = (U8) _A;
// 			}
// 
// 			void Write( void* _pixel, float _A ) const override {	PF_RGBA8_sRGB& P = *((PF_RGBA8_sRGB*) _pixel);
// 				P.A = F32toU8( _A );
// 			}
// 
// 			float	Red( const void* _pixel ) const override					{ return ((PF_RGBA8_sRGB*) _pixel)->R / 255.0f; }
// 			float	Green( const void* _pixel ) const override					{ return ((PF_RGBA8_sRGB*) _pixel)->G / 255.0f; }
// 			float	Blue( const void* _pixel ) const override					{ return ((PF_RGBA8_sRGB*) _pixel)->B / 255.0f; }
// 			float	Alpha( const void* _pixel ) const override					{ return ((PF_RGBA8_sRGB*) _pixel)->A / 255.0f; }
// 			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( ((PF_RGBA8_sRGB*) _pixel)->R / 255.0f, ((PF_RGBA8_sRGB*) _pixel)->G / 255.0f, ((PF_RGBA8_sRGB*) _pixel)->B / 255.0f, ((PF_RGBA8_sRGB*) _pixel)->A / 255.0f ); }
// 
// 		} Descriptor;
// 		#pragma endregion
// 	};


	// This format is a special encoding of 3 floating point values into 4 U8 values, aka "Real Pixels"
	// The RGB encode the mantissa of each RGB float component while A encodes the exponent by which multiply these 3 mantissae
	// In fact, we only use a single common exponent that we factor out to 3 different mantissae.
	// This format was first created by Gregory Ward for his Radiance software (http://www.graphics.cornell.edu/~bjw/rgbe.html)
	//  and allows to store HDR values using standard 8-bits formats.
	// It's also quite useful to pack some data as we divide the size by 3, from 3 floats (12 bytes) down to only 4 bytes.
	//
	// Remarks: This format only allows storage of POSITIVE floats!
	struct	PF_RGBE {
		U8	B, G, R, E;

		#pragma region IPixelAccessor
		static class desc_t : IPixelAccessor {
		public:

			U32		Size() const override { return 4; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RGBE& P = *((PF_RGBE*) _pixel);
				P.R = (U8) _R;
				P.G = (U8) _G;
				P.B = (U8) _B;
				P.E = (U8) _A;
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override { Write( _pixel, _Color.x, _Color.y, _Color.z, _Color.w ); }
			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override { ((PF_RGBE*) _pixel)->EncodeColor( _R, _G, _B ); }
			void Write( void* _pixel, U32 _E ) const override {}
			void Write( void* _pixel, float _E ) const override {}

			float	Red( const void* _pixel ) const override					{ bfloat3 tempHDR; ((PF_RGBE*) _pixel)->DecodedColor( tempHDR ); return tempHDR.x; }
			float	Green( const void* _pixel ) const override					{ bfloat3 tempHDR; ((PF_RGBE*) _pixel)->DecodedColor( tempHDR ); return tempHDR.y; }
			float	Blue( const void* _pixel ) const override					{ bfloat3 tempHDR; ((PF_RGBE*) _pixel)->DecodedColor( tempHDR ); return tempHDR.z; }
			float	Alpha( const void* _pixel ) const override					{ return ((PF_RGBE*) _pixel)->E / 255.0f; }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ ((PF_RGBE*) _pixel)->DecodedColor( (bfloat3&) _Color ); _Color.w = 1.0f; }

		} Descriptor;
		#pragma endregion

		void	EncodeColor( float _R, float _G, float _B ) {
			float	maxComponent = MAX( _R, MAX( _G, _B ) );
			if ( maxComponent < 1e-16f ) {
				// Too low to encode...
				R = G = B = E = 0;
				return;
			}

			float	completeExponent = log2f( maxComponent );
			float	exponent = float( ceilf( completeExponent ) );
			double	mantissa = maxComponent / powf( 2.0f, exponent );
			if ( mantissa == 1.0 ) {
				// Step to next order
				mantissa = 0.5;
				exponent++;
			}

//			double	debug0 = mantissa * powf( 2.0, exponent );
			maxComponent = float( mantissa * 255.99999999f / maxComponent );

			R = (U8) (_R * maxComponent);
			G = (U8) (_G * maxComponent);
			B = (U8) (_B * maxComponent);
			E = (U8) (exponent + 128 );
		}

		void	DecodedColor( bfloat3& _HDRColor ) {
			float exponent = powf( 2.0f, E - (128.0f + 8.0f) );
			_HDRColor.Set(	(float) ((R + .5) * exponent),
							(float) ((G + .5) * exponent),
							(float) ((B + .5) * exponent)
						);
		}
	};

	#pragma endregion

	#pragma region 16-Bits Formats

	// R16 format
	struct	PF_R16 {
		U16	R;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 2; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_R16& P = *((PF_R16*) _pixel);
				P.R = (U16) _R;
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_R16& P = *((PF_R16*) _pixel);
				P.R = F32toU16(_Color.x);
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_R16& P = *((PF_R16*) _pixel);
				P.R = F32toU16(_R);
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return U16toF32( ((PF_R16*) _pixel)->R ); }
			float	Green( const void* _pixel ) const override					{ return 0.0f; }
			float	Blue( const void* _pixel ) const override					{ return 0.0f; }
			float	Alpha( const void* _pixel ) const override					{ return 1.0f; }
			// Here I'm taking the risk of returning a grayscale image... :/
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ float	v = U16toF32( ((PF_R16*) _pixel)->R ); _Color.Set( v, v, v, 1 ); }

		} Descriptor;
		#pragma endregion
	};

	// RG16 format
	struct	PF_RG16 {
		U16	R, G;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 4; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RG16& P = *((PF_RG16*) _pixel);
				P.R = (U16) _R;
				P.G = (U16) _G;
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RG16& P = *((PF_RG16*) _pixel);
				P.R = F32toU16(_Color.x);
				P.G = F32toU16(_Color.y);
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RG16& P = *((PF_RG16*) _pixel);
				P.R = F32toU16(_R);
				P.G = F32toU16(_G);
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return U16toF32( ((PF_RG16*) _pixel)->R ); }
			float	Green( const void* _pixel ) const override					{ return U16toF32( ((PF_RG16*) _pixel)->G ); }
			float	Blue( const void* _pixel ) const override					{ return 0.0f; }
			float	Alpha( const void* _pixel ) const override					{ return 1.0f; }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( U16toF32( ((PF_RG16*) _pixel)->R ), U16toF32( ((PF_RG16*) _pixel)->G ), 0, 1 ); }

		} Descriptor;
		#pragma endregion
	};

	// RGB16 format
	struct	PF_RGB16 {
		U16	R, G, B;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 6; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RGB16& P = *((PF_RGB16*) _pixel);
				P.R = (U16) _R;
				P.G = (U16) _G;
				P.B = (U16) _B;
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RGB16& P = *((PF_RGB16*) _pixel);
				P.R = F32toU16(_Color.x);
				P.G = F32toU16(_Color.y);
				P.B = F32toU16(_Color.z);
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RGB16& P = *((PF_RGB16*) _pixel);
				P.R = F32toU16(_R);
				P.G = F32toU16(_G);
				P.B = F32toU16(_B);
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return U16toF32( ((PF_RGB16*) _pixel)->R ); }
			float	Green( const void* _pixel ) const override					{ return U16toF32( ((PF_RGB16*) _pixel)->G ); }
			float	Blue( const void* _pixel ) const override					{ return U16toF32( ((PF_RGB16*) _pixel)->B ); }
			float	Alpha( const void* _pixel ) const override					{ return 1.0f; }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( U16toF32( ((PF_RGB16*) _pixel)->R ), U16toF32( ((PF_RGB16*) _pixel)->G ), U16toF32( ((PF_RGB16*) _pixel)->B ), 1 ); }

		} Descriptor;
		#pragma endregion
	};
	
	// RGBA16 format
	struct	PF_RGBA16 {
		U16	R, G, B, A;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 8; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RGBA16& P = *((PF_RGBA16*) _pixel);
				P.R = (U16) _R;
				P.G = (U16) _G;
				P.B = (U16) _B;
				P.A = (U16) _A;
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RGBA16& P = *((PF_RGBA16*) _pixel);
				P.R = F32toU16(_Color.x);
				P.G = F32toU16(_Color.y);
				P.B = F32toU16(_Color.z);
				P.A = F32toU16(_Color.w);
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RGBA16& P = *((PF_RGBA16*) _pixel);
				P.R = F32toU16(_R);
				P.G = F32toU16(_G);
				P.B = F32toU16(_B);
				P.A = F32toU16(_A);
			}

			void Write( void* _pixel, U32 _A ) const override {	PF_RGBA16& P = *((PF_RGBA16*) _pixel);
				P.A = (U16) _A;
			}

			void Write( void* _pixel, float _A ) const override {	PF_RGBA16& P = *((PF_RGBA16*) _pixel);
				P.A = F32toU16(_A);
			}

			float	Red( const void* _pixel ) const override					{ return U16toF32( ((PF_RGBA16*) _pixel)->R ); }
			float	Green( const void* _pixel ) const override					{ return U16toF32( ((PF_RGBA16*) _pixel)->G ); }
			float	Blue( const void* _pixel ) const override					{ return U16toF32( ((PF_RGBA16*) _pixel)->B ); }
			float	Alpha( const void* _pixel ) const override					{ return U16toF32( ((PF_RGBA16*) _pixel)->A ); }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( U16toF32( ((PF_RGBA16*) _pixel)->R ), U16toF32( ((PF_RGBA16*) _pixel)->G ), U16toF32( ((PF_RGBA16*) _pixel)->B ), U16toF32( ((PF_RGBA16*) _pixel)->A ) ); }

		} Descriptor;
		#pragma endregion
	};

	#pragma endregion

	#pragma region 16 bits floating-point formats
	
	// R16F format
	struct	PF_R16F {
		half	R;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 2; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_R16F& P = *((PF_R16F*) _pixel);
				P.R = U16toF32(_R);
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_R16F& P = *((PF_R16F*) _pixel);
				P.R = _Color.x;
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_R16F& P = *((PF_R16F*) _pixel);
				P.R = _R;
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return ((PF_R16F*) _pixel)->R; }
			float	Green( const void* _pixel ) const override					{ return 0; }
			float	Blue( const void* _pixel ) const override					{ return 0; }
			float	Alpha( const void* _pixel ) const override					{ return 1; }
			// Here I'm taking the risk of returning a grayscale image... :/
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ float	v = ((PF_R16F*) _pixel)->R; _Color.Set( v, v, v, 1 ); }

		} Descriptor;
		#pragma endregion
	};

	// RG16F format
	struct	PF_RG16F {
		half	R, G;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 4; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RG16F& P = *((PF_RG16F*) _pixel);
				P.R = U16toF32(_R);
				P.G = U16toF32(_G);
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RG16F& P = *((PF_RG16F*) _pixel);
				P.R = _Color.x;
				P.G = _Color.y;
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RG16F& P = *((PF_RG16F*) _pixel);
				P.R = _R;
				P.G = _G;
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return ((PF_RG16F*) _pixel)->R; }
			float	Green( const void* _pixel ) const override					{ return ((PF_RG16F*) _pixel)->G; }
			float	Blue( const void* _pixel ) const override					{ return 0; }
			float	Alpha( const void* _pixel ) const override					{ return 1; }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( ((PF_RG16F*) _pixel)->R, ((PF_RG16F*) _pixel)->G, 0, 1 ); }

		} Descriptor;
		#pragma endregion
	};

	// RGB16F Format
	struct	PF_RGB16F {
		half	R, G, B;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 6; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RGB16F& P = *((PF_RGB16F*) _pixel);
				P.R = U16toF32(_R);
				P.G = U16toF32(_G);
				P.B = U16toF32(_B);
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RGB16F& P = *((PF_RGB16F*) _pixel);
				P.R = _Color.x;
				P.G = _Color.y;
				P.B = _Color.z;
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RGB16F& P = *((PF_RGB16F*) _pixel);
				P.R = _R;
				P.G = _G;
				P.B = _B;
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return ((PF_RGB16F*) _pixel)->R; }
			float	Green( const void* _pixel ) const override					{ return ((PF_RGB16F*) _pixel)->G; }
			float	Blue( const void* _pixel ) const override					{ return ((PF_RGB16F*) _pixel)->B; }
			float	Alpha( const void* _pixel ) const override					{ return 1; }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( ((PF_RGB16F*) _pixel)->R, ((PF_RGB16F*) _pixel)->G, ((PF_RGB16F*) _pixel)->B, 1 ); }

		} Descriptor;
		#pragma endregion
	};

	// RGBA16F format
	struct	PF_RGBA16F {
		half	R, G, B, A;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 8; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RGBA16F& P = *((PF_RGBA16F*) _pixel);
				P.R = U16toF32(_R);
				P.G = U16toF32(_G);
				P.B = U16toF32(_B);
				P.A = U16toF32(_A);
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RGBA16F& P = *((PF_RGBA16F*) _pixel);
				P.R = _Color.x;
				P.G = _Color.y;
				P.B = _Color.z;
				P.A = _Color.w;
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RGBA16F& P = *((PF_RGBA16F*) _pixel);
				P.R = _R;
				P.G = _G;
				P.B = _B;
				P.A = _A;
			}

			void Write( void* _pixel, U32 _A ) const override { PF_RGBA16F& P = *((PF_RGBA16F*) _pixel);
				P.A = U16toF32(_A);
			}

			void Write( void* _pixel, float _A ) const override { PF_RGBA16F& P = *((PF_RGBA16F*) _pixel);
				P.A = _A;
			}

			float	Red( const void* _pixel ) const override					{ return ((PF_RGBA16F*) _pixel)->R; }
			float	Green( const void* _pixel ) const override					{ return ((PF_RGBA16F*) _pixel)->G; }
			float	Blue( const void* _pixel ) const override					{ return ((PF_RGBA16F*) _pixel)->B; }
			float	Alpha( const void* _pixel ) const override					{ return ((PF_RGBA16F*) _pixel)->A; }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( ((PF_RGBA16F*) _pixel)->R, ((PF_RGBA16F*) _pixel)->G, ((PF_RGBA16F*) _pixel)->B, ((PF_RGBA16F*) _pixel)->A ); }

		} Descriptor;
		#pragma endregion
	};

	#pragma endregion

	#pragma region 32-Bits Formats

	// R32 format
	struct	PF_R32 {
		U32	R;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 4; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_R32& P = *((PF_R32*) _pixel);
				P.R = _R;
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_R32& P = *((PF_R32*) _pixel);
				P.R = F32toU32(_Color.x);
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_R32& P = *((PF_R32*) _pixel);
				P.R = F32toU32(_R);
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return U32toF32( ((PF_R32*) _pixel)->R ); }
			float	Green( const void* _pixel ) const override					{ return 0.0f; }
			float	Blue( const void* _pixel ) const override					{ return 0.0f; }
			float	Alpha( const void* _pixel ) const override					{ return 1.0f; }
			// Here I'm taking the risk of returning a grayscale image... :/
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ float	v = U32toF32( ((PF_R32*) _pixel)->R ); _Color.Set( v, v, v, 1 ); }

		} Descriptor;
		#pragma endregion
	};

	// RG32 format
	struct	PF_RG32 {
		U32	R, G;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 8; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RG32& P = *((PF_RG32*) _pixel);
				P.R = _R;
				P.G = _G;
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RG32& P = *((PF_RG32*) _pixel);
				P.R = F32toU32(_Color.x);
				P.G = F32toU32(_Color.y);
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RG32& P = *((PF_RG32*) _pixel);
				P.R = F32toU32(_R);
				P.G = F32toU32(_G);
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return U32toF32( ((PF_RG32*) _pixel)->R ); }
			float	Green( const void* _pixel ) const override					{ return U32toF32( ((PF_RG32*) _pixel)->G ); }
			float	Blue( const void* _pixel ) const override					{ return 0.0f; }
			float	Alpha( const void* _pixel ) const override					{ return 1.0f; }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( U32toF32( ((PF_RG32*) _pixel)->R ), U32toF32( ((PF_RG32*) _pixel)->G ), 0, 1 ); }

		} Descriptor;
		#pragma endregion
	};

	// RGB32 format
	struct	PF_RGB32 {
		U32	R, G, B;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 12; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RGB32& P = *((PF_RGB32*) _pixel);
				P.R = _R;
				P.G = _G;
				P.B = _B;
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RGB32& P = *((PF_RGB32*) _pixel);
				P.R = F32toU32(_Color.x);
				P.G = F32toU32(_Color.y);
				P.B = F32toU32(_Color.z);
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RGB32& P = *((PF_RGB32*) _pixel);
				P.R = F32toU32(_R);
				P.G = F32toU32(_G);
				P.B = F32toU32(_B);
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return U32toF32( ((PF_RGB32*) _pixel)->R ); }
			float	Green( const void* _pixel ) const override					{ return U32toF32( ((PF_RGB32*) _pixel)->G ); }
			float	Blue( const void* _pixel ) const override					{ return U32toF32( ((PF_RGB32*) _pixel)->B ); }
			float	Alpha( const void* _pixel ) const override					{ return 1.0f; }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( U32toF32( ((PF_RGB32*) _pixel)->R ), U32toF32( ((PF_RGB32*) _pixel)->G ), U32toF32( ((PF_RGB32*) _pixel)->B ), 1 ); }

		} Descriptor;
		#pragma endregion
	};
	
	// RGBA32 format
	struct	PF_RGBA32 {
		U32	R, G, B, A;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 16; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RGBA32& P = *((PF_RGBA32*) _pixel);
				P.R = _R;
				P.G = _G;
				P.B = _B;
				P.A = _A;
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RGBA32& P = *((PF_RGBA32*) _pixel);
				P.R = F32toU32(_Color.x);
				P.G = F32toU32(_Color.y);
				P.B = F32toU32(_Color.z);
				P.A = F32toU32(_Color.w);
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RGBA32& P = *((PF_RGBA32*) _pixel);
				P.R = F32toU32(_R);
				P.G = F32toU32(_G);
				P.B = F32toU32(_B);
				P.A = F32toU32(_A);
			}

			void Write( void* _pixel, U32 _A ) const override {	PF_RGBA32& P = *((PF_RGBA32*) _pixel);
				P.A = (U16) _A;
			}

			void Write( void* _pixel, float _A ) const override {	PF_RGBA32& P = *((PF_RGBA32*) _pixel);
				P.A = F32toU32(_A);
			}

			float	Red( const void* _pixel ) const override					{ return U16toF32( ((PF_RGBA32*) _pixel)->R ); }
			float	Green( const void* _pixel ) const override					{ return U16toF32( ((PF_RGBA32*) _pixel)->G ); }
			float	Blue( const void* _pixel ) const override					{ return U16toF32( ((PF_RGBA32*) _pixel)->B ); }
			float	Alpha( const void* _pixel ) const override					{ return U16toF32( ((PF_RGBA32*) _pixel)->A ); }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( U16toF32( ((PF_RGBA32*) _pixel)->R ), U16toF32( ((PF_RGBA32*) _pixel)->G ), U16toF32( ((PF_RGBA32*) _pixel)->B ), U16toF32( ((PF_RGBA32*) _pixel)->A ) ); }

		} Descriptor;
		#pragma endregion
	};

	#pragma endregion

	#pragma region 32 bits floating points Formats

	// R32F format
	struct	PF_R32F {
		float	R;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 4; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_R32F& P = *((PF_R32F*) _pixel);
				P.R = U32toF32(_R);
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_R32F& P = *((PF_R32F*) _pixel);
				P.R = _Color.x;
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_R32F& P = *((PF_R32F*) _pixel);
				P.R = _R;
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return ((PF_R32F*) _pixel)->R; }
			float	Green( const void* _pixel ) const override					{ return 0; }
			float	Blue( const void* _pixel ) const override					{ return 0; }
			float	Alpha( const void* _pixel ) const override					{ return 1; }
			// Here I'm taking the risk of returning a grayscale image... :/
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ float	v = ((PF_R32F*) _pixel)->R; _Color.Set( v, v, v, 1 ); }

		} Descriptor;
		#pragma endregion
	};

	// RG32F format
	struct	PF_RG32F {
		float	R, G;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 8; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RG32F& P = *((PF_RG32F*) _pixel);
				P.R = U32toF32(_R);
				P.G = U32toF32(_G);
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RG32F& P = *((PF_RG32F*) _pixel);
				P.R = _Color.x;
				P.G = _Color.y;
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RG32F& P = *((PF_RG32F*) _pixel);
				P.R = _R;
				P.G = _G;
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return ((PF_RG32F*) _pixel)->R; }
			float	Green( const void* _pixel ) const override					{ return ((PF_RG32F*) _pixel)->G; }
			float	Blue( const void* _pixel ) const override					{ return 0; }
			float	Alpha( const void* _pixel ) const override					{ return 1; }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( ((PF_RG32F*) _pixel)->R, ((PF_RG32F*) _pixel)->G, 0, 1 ); }

		} Descriptor;
		#pragma endregion
	};

	// RGB32F format
	struct	PF_RGB32F {
		float	R, G, B;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 12; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RGB32F& P = *((PF_RGB32F*) _pixel);
				P.R = U32toF32(_R);
				P.G = U32toF32(_G);
				P.B = U32toF32(_B);
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RGB32F& P = *((PF_RGB32F*) _pixel);
				P.R = _Color.x;
				P.G = _Color.y;
				P.B = _Color.z;
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RGB32F& P = *((PF_RGB32F*) _pixel);
				P.R = _R;
				P.G = _G;
				P.B = _B;
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return ((PF_RGB32F*) _pixel)->R; }
			float	Green( const void* _pixel ) const override					{ return ((PF_RGB32F*) _pixel)->G; }
			float	Blue( const void* _pixel ) const override					{ return ((PF_RGB32F*) _pixel)->B; }
			float	Alpha( const void* _pixel ) const override					{ return 1; }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( ((PF_RGB32F*) _pixel)->R, ((PF_RGB32F*) _pixel)->G, ((PF_RGB32F*) _pixel)->B, 1 ); }

		} Descriptor;
		#pragma endregion
	};

	// RGBA32F format
	struct	PF_RGBA32F {
		float	R, G, B, A;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			U32		Size() const override { return 16; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RGBA32F& P = *((PF_RGBA32F*) _pixel);
				P.R = U32toF32(_R);
				P.G = U32toF32(_G);
				P.B = U32toF32(_B);
				P.A = U32toF32(_A);
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RGBA32F& P = *((PF_RGBA32F*) _pixel);
				P.R = _Color.x;
				P.G = _Color.y;
				P.B = _Color.z;
				P.A = _Color.w;
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RGBA32F& P = *((PF_RGBA32F*) _pixel);
				P.R = _R;
				P.G = _G;
				P.B = _B;
				P.A = _A;
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
			}

			float	Red( const void* _pixel ) const override					{ return ((PF_RGBA32F*) _pixel)->R; }
			float	Green( const void* _pixel ) const override					{ return ((PF_RGBA32F*) _pixel)->G; }
			float	Blue( const void* _pixel ) const override					{ return ((PF_RGBA32F*) _pixel)->B; }
			float	Alpha( const void* _pixel ) const override					{ return ((PF_RGBA32F*) _pixel)->A; }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( ((PF_RGBA32F*) _pixel)->R, ((PF_RGBA32F*) _pixel)->G, ((PF_RGBA32F*) _pixel)->B, ((PF_RGBA32F*) _pixel)->A ); }

		} Descriptor;
		#pragma endregion
	};

	#pragma endregion

	#pragma region Depth Formats

	// D16 format
	struct	PF_D16 {
		half	D;

		#pragma region IDepthAccessor
		static class desc_t : public IDepthAccessor {
		public:
			U32		Size() const override { return 2; }
			bool	HasStencil() const override { return false; }

			void	Write( void* _pixel, float _Depth, U8 _Stencil ) override { PF_D16& P = *((PF_D16*) _pixel);
				P.D = _Depth;
			}
			float	Depth( const void* _pixel ) const override { PF_D16& P = *((PF_D16*) _pixel);
				return P.D;
			}
			U8		Stencil( const void* _pixel ) const override { return 0; }
			void	DepthStencil( const void* _pixel, float& _Depth, U8& _Stencil ) const override { PF_D16& P = *((PF_D16*) _pixel);
				_Depth = P.D;
				_Stencil = 0;
			}

		} Descriptor;
		#pragma endregion
	};

	// D32 format
	struct	PF_D32 {
		float	D;

		#pragma region IDepthAccessor
		static class desc_t : public IDepthAccessor {
		public:
			U32		Size() const override { return 4; }
			bool	HasStencil() const override { return false; }

			void	Write( void* _pixel, float _Depth, U8 _Stencil ) override { PF_D32& P = *((PF_D32*) _pixel);
				P.D = _Depth;
			}
			float	Depth( const void* _pixel ) const override { PF_D32& P = *((PF_D32*) _pixel);
				return P.D;
			}
			U8		Stencil( const void* _pixel ) const override { return 0; }
			void	DepthStencil( const void* _pixel, float& _Depth, U8& _Stencil ) const override { PF_D32& P = *((PF_D32*) _pixel);
				_Depth = P.D;
				_Stencil = 0;
			}

		} Descriptor;
		#pragma endregion
	};
		
	// D24S8 format (24 bits depth + 8 bits stencil)
	struct	PF_D24S8 {
		U8	D[3];
		U8	Stencil;

		#pragma region IDepthAccessor
		static class desc_t : public IDepthAccessor {
		public:
			U32		Size() const override { return 4; }
			bool	HasStencil() const override { return true; }

			void	Write( void* _pixel, float _Depth, U8 _Stencil ) override { PF_D24S8& P = *((PF_D24S8*) _pixel);
				U32	temp = (U32&) _Depth;
				((U8*) &temp)[3] = _Stencil;
				*((U32*) P.D) = temp;
			}
			float	Depth( const void* _pixel ) const override { PF_D24S8& P = *((PF_D24S8*) _pixel);
				U32	temp = *((U32*) P.D);
				((U8*) &temp)[3] = 0x3F;
				float&	tempF = (float&) temp;
				return tempF;
			}
			U8		Stencil( const void* _pixel ) const override { return ((PF_D24S8*) _pixel)->Stencil; }
			void	DepthStencil( const void* _pixel, float& _Depth, U8& _Stencil ) const override { PF_D24S8& P = *((PF_D24S8*) _pixel);
				U32	temp = *((U32*) P.D);
				((U8*) &temp)[3] = 0x3F;
				float&	tempF = (float&) temp;

				_Depth = tempF;
				_Stencil = P.Stencil;
			}

		} Descriptor;
		#pragma endregion
	};

	#pragma endregion

	#pragma pack(pop)

}	// namespace BaseLib