//////////////////////////////////////////////////////////////////////////
// The structures declared here allow us to wrap the standard DXGI types and use standardized Read()/Write() methods to address the pixels
// The goal here is to always be capable of abstracting a pixel format as float4
// Also, a Read() immediately followed by a Write() should yield exactly the original value
//
////////////////////////////////////////////////////////////////////////////
//
#pragma once

//#include <dxgi.h>

namespace BaseLib {

	// This is the interface to access pixel format structures needed for images and textures
	class	IPixelAccessor {
	public:
		// Gives the size of the pixel format in bytes
		virtual U32		Size() const abstract;

		// Tells if the format uses sRGB input (cf. Image<PF> Gamma Correction)
		virtual bool	sRGB() const abstract;

		// Tells the equivalent DXGI pixel format used by DirectX
//		virtual DXGI_FORMAT	DirectXFormat() const abstract;

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

		// Converts a U8 component to a float component
		inline static float		U8toF32( U32 _Component ) {
			return _Component / 255.0f;
		}

		// Converts a U16 component to a float component
		inline static float		U16toF32( U32 _Component ) {
			return _Component / 65535.0f;
		}

		// Converts a U32 component to a float component
		inline static float		U32toF32( U32 _Component ) {
			return _Component / 4294967295.0f;	// / (2^32-1)
		}

		// Converts a float component to a U8 component
		inline static U8		F32toU8( float _Component ) {
			return U8( CLAMP( _Component * 255.0f, 0.0f, 255.0f ) );
		}

		// Converts a float component to a U16 component
		inline static U16		F32toU16( float _Component ) {
			return U16( CLAMP( _Component * 65535.0f, 0.0f, 65535.0f ) );
		}
	};

	
	// This is the interface to access depth format structures needed for depth stencil buffers
	// They inherit pixel format data and may define additional data
	class IDepthAccessor {
		// Gives the size of the depth format in bytes
		virtual U32		Size() const abstract;

		// Tells if the format uses stencil bits
		virtual bool	HasStencil() const abstract;

		// Tells the equivalent DXGI pixel format used by DirectX
//		virtual DXGI_FORMAT	DirectXFormat() const abstract;

		virtual void	Write( void* _pixel, float _Depth, U8 _Stencil ) abstract;
		virtual float	Depth( const void* _pixel ) const abstract;
		virtual U8		Stencil( const void* _pixel ) const abstract;
		virtual void	DepthStencil( const void* _pixel, float& _Depth, U8& _Stencil ) const abstract;
	};

 	struct	PF_Unknown {
		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			bool		sRGB() const override			{ return false; }
			U32			Size() const override			{ return 0; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {}
			void Write( void* _pixel, const bfloat4& _Color ) const override {}
			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {}
			void Write( void* _pixel, U32 _A ) const override {}
			void Write( void* _pixel, float _A ) const override {}

			float	Red( const void* _pixel ) const override					{ return 0; }
			float	Green( const void* _pixel ) const override					{ return 0; }
			float	Blue( const void* _pixel ) const override					{ return 0; }
			float	Alpha( const void* _pixel ) const override					{ return 1;}
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{}

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

			bool		sRGB() const override			{ return false; }
			U32			Size() const override			{ return 1; }
//			DXGI_FORMAT	DirectXFormat() const override	{ return DXGI_FORMAT_R8_UNORM; }

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
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( ((PF_R8*) _pixel)->R / 255.0f, 0, 0, 1 ); }

		}	Descriptor;
		#pragma endregion
	};

	// RG8 format
	struct PF_RG8 {
		U8	R, G;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			bool	sRGB() const override { return false; }
			U32		Size() const override { return 2; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RG8& P = *((PF_RG8*) _pixel);
				P.R = (U8) _R;
				P.G = (U8) _G;
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RG8& P = *((PF_RG8*) _pixel);
				P.R = F32toU8(_Color.x);
				P.G = F32toU8(_Color.y);
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RG8& P = *((PF_RG8*) _pixel);
				P.R = F32toU8( _R );
				P.G = F32toU8( _G );
			}

			void Write( void* _pixel, U32 _A ) const override {
			}

			void Write( void* _pixel, float _A ) const override {
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

			bool	sRGB() const override { return false; }
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

			bool	sRGB() const override { return false; }
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
	struct	PF_RGBA8_sRGB {
		U8	B, G, R, A;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			bool	sRGB() const override { return true; }
			U32		Size() const override { return 4; }

			void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) const override {	PF_RGBA8_sRGB& P = *((PF_RGBA8_sRGB*) _pixel);
				P.R = (U8) _R;
				P.G = (U8) _G;
				P.B = (U8) _B;
				P.A = (U8) _A;
			}

			void Write( void* _pixel, const bfloat4& _Color ) const override {	PF_RGBA8_sRGB& P = *((PF_RGBA8_sRGB*) _pixel);
				P.R = F32toU8(_Color.x);
				P.G = F32toU8(_Color.y);
				P.B = F32toU8(_Color.z);
				P.A = F32toU8(_Color.w);
			}

			void Write( void* _pixel, float _R, float _G, float _B, float _A ) const override {	PF_RGBA8_sRGB& P = *((PF_RGBA8_sRGB*) _pixel);
				P.R = F32toU8( _R );
				P.G = F32toU8( _G );
				P.B = F32toU8( _B );
				P.A = F32toU8( _A );
			}

			void Write( void* _pixel, U32 _A ) const override {	PF_RGBA8_sRGB& P = *((PF_RGBA8_sRGB*) _pixel);
				P.A = (U8) _A;
			}

			void Write( void* _pixel, float _A ) const override {	PF_RGBA8_sRGB& P = *((PF_RGBA8_sRGB*) _pixel);
				P.A = F32toU8( _A );
			}

			float	Red( const void* _pixel ) const override					{ return ((PF_RGBA8_sRGB*) _pixel)->R / 255.0f; }
			float	Green( const void* _pixel ) const override					{ return ((PF_RGBA8_sRGB*) _pixel)->G / 255.0f; }
			float	Blue( const void* _pixel ) const override					{ return ((PF_RGBA8_sRGB*) _pixel)->B / 255.0f; }
			float	Alpha( const void* _pixel ) const override					{ return ((PF_RGBA8_sRGB*) _pixel)->A / 255.0f; }
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( ((PF_RGBA8_sRGB*) _pixel)->R / 255.0f, ((PF_RGBA8_sRGB*) _pixel)->G / 255.0f, ((PF_RGBA8_sRGB*) _pixel)->B / 255.0f, ((PF_RGBA8_sRGB*) _pixel)->A / 255.0f ); }

		} Descriptor;
		#pragma endregion
	};


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

			bool	sRGB() const override { return false; }
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

			bool	sRGB() const override { return false; }
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
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( U16toF32( ((PF_R16*) _pixel)->R ), 0, 0, 1 ); }

		} Descriptor;
		#pragma endregion
	};

	// RG16 format
	struct	PF_RG16 {
		U16	R, G;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			bool	sRGB() const override { return false; }
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

			bool	sRGB() const override { return false; }
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

			bool	sRGB() const override { return false; }
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

			bool	sRGB() const override { return false; }
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
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( ((PF_R16F*) _pixel)->R, 0, 0, 1 ); }

		} Descriptor;
		#pragma endregion
	};

	// RG16F format
	struct	PF_RG16F {
		half	R, G;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			bool	sRGB() const override { return false; }
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

			bool	sRGB() const override { return false; }
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

			bool	sRGB() const override { return false; }
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

	#pragma region 32 bits floating points Formats

	// R32F format
	struct	PF_R32F {
		float	R;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			bool	sRGB() const override { return false; }
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
			void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( ((PF_R32F*) _pixel)->R, 0, 0, 1 ); }

		} Descriptor;
		#pragma endregion
	};

	// RG32F format
	struct	PF_RG32F {
		float	R, G;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			bool	sRGB() const override { return false; }
			U32		Size() const override { return 4; }

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

			bool	sRGB() const override { return false; }
			U32		Size() const override { return 4; }

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
	struct	PF_RGBA32F : IPixelAccessor {
		float	R, G, B, A;

		#pragma region IPixelAccessor
		static class desc_t : public IPixelAccessor {
		public:

			bool	sRGB() const override { return false; }
			U32		Size() const override { return 4; }

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
	struct	PF_D24S8 : IDepthAccessor {
		U8	D[3];
		U8	Stencil;

		#pragma region IDepthAccessor
		static class desc_t : public IDepthAccessor {
		public:
			U32		Size() const override { return 4; }
			bool	HasStencil() const override { return true; }

			void	Write( void* _pixel, float _Depth, U8 _Stencil ) override { PF_D24S8& P = *((PF_D24S8*) _pixel);
				U32	temp = (U32&) _Depth;
				((U8*) temp)[3] = _Stencil;
				*((U32*) P.D) = temp;
			}
			float	Depth( const void* _pixel ) const override { PF_D24S8& P = *((PF_D24S8*) _pixel);
				U32	temp = *((U32*) P.D);
				((U8*) temp)[3] = 0x3F;
				float&	tempF = (float&) temp;
				return tempF;
			}
			U8		Stencil( const void* _pixel ) const override { ((PF_D24S8*) _pixel)->Stencil; }
			void	DepthStencil( const void* _pixel, float& _Depth, U8& _Stencil ) const override { PF_D24S8& P = *((PF_D24S8*) _pixel);
				U32	temp = *((U32*) P.D);
				((U8*) temp)[3] = 0x3F;
				float&	tempF = (float&) temp;

				_Depth = tempF;
				_Stencil = P.Stencil;
			}

		} Descriptor;
		#pragma endregion
	};

	#pragma endregion

}	// namespace BaseLib