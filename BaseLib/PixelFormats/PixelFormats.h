//////////////////////////////////////////////////////////////////////////
// The structures declared here allow us to wrap the standard DXGI types and use standardized Read()/Write() methods to address the pixels
// The goal here is to always be capable of abstracting a pixel format as float4
// Also, a Read() immediately followed by a Write() should yield exactly the original value
//
////////////////////////////////////////////////////////////////////////////
//
#pragma once

namespace BaseLib {

	/// <summary>
	/// This is the interface to access pixel format structures needed for images and textures
	/// </summary>
	class	IPixelFormatAccessor {
	public:
		// Gives the size of the pixel format in bytes
		virtual U32		Size() const abstract;

		// Tells if the format uses sRGB input (cf. Image<PF> Gamma Correction)
		virtual bool	sRGB() const abstract;

		// LDR pixel writer
		virtual void	Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) abstract;
		virtual void	Write( void* _pixel, U32 _A ) abstract;

		// HDR pixel writer
		virtual void	Write( void* _pixel, const bfloat4& _Color ) abstract;
		virtual void	Write( void* _pixel, float _R, float _G, float _B, float _A ) abstract;
		virtual void	Write( void* _pixel, float _A ) abstract;

		// HDR pixel readers
		virtual float	Red( const void* _pixel ) const abstract;
		virtual float	Green( const void* _pixel ) const abstract;
		virtual float	Blue( const void* _pixel ) const abstract;
		virtual float	Alpha( const void* _pixel ) const abstract;
		virtual void	RGBA( const void* _pixel, bfloat4& _Color ) const abstract;

	protected:	// HELPERS

		template< typename T > inline void Setup( void* _pixelPtr, T* _internalPtr )				{ _internalPtr = reinterpret_cast< T* >( _pixelPtr ); }
		template< typename T > inline void Setup( const void* _pixelPtr, T* _internalPtr ) const	{ _internalPtr = reinterpret_cast< T* >( const_cast< void* >( _pixelPtr ) ); }	// Don't worry, we won't write it!

		// Converts a U8 component to a float component
		inline static float		ToFloat( U32 _Component ) {
			return _Component / 255.0f;
		}

		// Converts a float component to a U8 component
		inline static U8		ToByte( float _Component ) {
			return U8( CLAMP( _Component * 255.0f, 0.0f, 255.0f ) );
		}

		// Converts a float component to a U16 component
		inline static U16	ToUShort( float _Component ) {
			return U16( CLAMP( _Component * 65535.0f, 0.0f, 65535.0f ) );
		}
	};

	/// <summary>
	/// This is the interface to access depth format structures needed for depth stencil buffers
	/// They inherit pixel format data and define additional data
	/// </summary>
	struct IDepthFormatAccessor : public IPixelFormatAccessor {
		// Actually, they don't! :)
	};

// 	struct	PF_Empty : IPixelFormatAccessor {
// 		#pragma region IPixelFormat Members
// 
// 		bool	sRGB() override	{ return false; }
// 
// 		void	Write( U32 _R, U32 _G, U32 _B, U32 _A ) override		{}
// 		void	Write( const bfloat4& _Color ) override	{}
// 		void	Write( float _R, float _G, float _B, float _A ) override	{}
// 		void	Write( U32 _A ) override		{}
// 		void	Write( float _A ) override		{}
// 
// 		float	Red()	{ return 0.0f; }
// 		float	Green()	{ return 0.0f; }
// 		float	Blue()	{ return 0.0f; }
// 		float	Alpha()	{ return 1.0f; }
// 		void	RGBA( bfloat4& _Color ) override { _Color.Set( 0, 0, 0, 0 ); }
// 
// 		#pragma endregion
// 
// 	};

	#pragma region 8-Bits Formats

	// R8 format
	class	PF_R8 : public IPixelFormatAccessor {
	private:
		U8*	R;
// 		void	Setup( void* _pixel ) { R = reinterpret_cast<U8*>( _pixel ); }
// 		void	Setup( const void* _pixel ) { R = reinterpret_cast<U8*>( const_cast<void*>( _pixel ) ); }

	public:
		#pragma region IPixelFormat Members

		bool	sRGB() const override { return false; }
		U32		Size() const override { return 1; }

		void Write( void* _pixel, U32 _R, U32 _G, U32 _B, U32 _A ) override {
			Setup( _pixel, R );
			*R = (U8) _R;
		}

		void Write( void* _pixel, const bfloat4& _Color ) override {
			Setup( _pixel, R );
			*R = ToByte(_Color.x);
		}

		void Write( void* _pixel, float _R, float _G, float _B, float _A ) override {
			Setup( _pixel, R );
			*R = ToByte( _R );
		}

		void Write( void* _pixel, U32 _A ) override {
		}

		void Write( void* _pixel, float _A ) override {
		}

		float	Red( const void* _pixel ) const override		{ Setup( _pixel, R ); return *R / 255.0f; }
		float	Green( const void* _pixel ) const override		{ Setup( _pixel, R ); return 0.0f; }
		float	Blue( const void* _pixel ) const override		{ Setup( _pixel, R ); return 0.0f; }
		float	Alpha( const void* _pixel ) const override		{ Setup( _pixel, R ); return 1.0f; }
		void	RGBA( const void* _pixel, bfloat4& _Color ) const override	{ _Color.Set( *R / 255.0f, 0, 0, 1 ); }

		#pragma endregion
	};

	/// <summary>
	/// RG8 format
	/// </summary>
	struct	PF_RG8 : IPixelFormatAccessor {
		U8	G, R;

		#pragma region IPixelFormat Members

		bool	sRGB()	{ return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			R = (U8) _R;
			G = (U8) _G;
		}

		void Write( const bfloat4& _Color ) {
			R = PF_Empty::ToByte(_Color.x);
			G = PF_Empty::ToByte(_Color.y);
		}

		void Write( float _R, float _G, float _B, float _A ) {
			R = PF_Empty::ToByte( _R );
			G = PF_Empty::ToByte( _G );
		}

		void Write( U32 _A ) override {
		}

		void Write( float _A ) override {
		}

		float	Red()	{ return R / 255.0f; }
		float	Green()	{ return G / 255.0f; }
		float	Blue()	{ return 0.0f; }
		float	Alpha()	{ return 1.0f; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( R / 255.0f, G / 255.0f, 0, 1 ); }

		#pragma endregion
	};

	/// <summary>
	/// RGB8 format
	/// </summary>
	struct	PF_RGB8 : IPixelFormatAccessor {
		U8	B, G, R;

		#pragma region IPixelFormat Members

		bool sRGB() override { return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			R = (U8) _R;
			G = (U8) _G;
			B = (U8) _B;
		}

		void Write( const bfloat4& _Color ) {
			R = PF_Empty::ToByte(_Color.x);
			G = PF_Empty::ToByte(_Color.y);
			B = PF_Empty::ToByte(_Color.z);
		}

		void Write( float _R, float _G, float _B, float _A ) {
			R = PF_Empty::ToByte( _R);
			G = PF_Empty::ToByte( _G );
			B = PF_Empty::ToByte( _B );
		}

		void Write( U32 _A ) {
		}

		void Write( float _A ) {
		}

		float	Red()	{ return R / 255.0f; }
		float	Green()	{ return G / 255.0f; }
		float	Blue()	{ return B / 255.0f; }
		float	Alpha()	{ return 1.0; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( R / 255.0f, G / 255.0f, B / 255.0f, 1.0 ); }

		#pragma endregion
	};

	/// <summary>
	/// RGBA8 format
	/// </summary>
	struct	PF_RGBA8 : IPixelFormatAccessor {
		U8	R, G, B, A;

		#pragma region IPixelFormat Members

		bool sRGB() override { return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			R = (U8) _R;
			G = (U8) _G;
			B = (U8) _B;
			A = (U8) _A;
		}

		void Write( const bfloat4& _Color ) {
			R = PF_Empty::ToByte(_Color.x);
			G = PF_Empty::ToByte(_Color.y);
			B = PF_Empty::ToByte(_Color.z);
			A = PF_Empty::ToByte(_Color.w);
		}

		void Write( float _R, float _G, float _B, float _A ) {
			R = PF_Empty::ToByte( _R);
			G = PF_Empty::ToByte( _G );
			B = PF_Empty::ToByte( _B );
			A = PF_Empty::ToByte( _A );
		}

		void Write( U32 _A ) {
			A = (U8) _A;
		}

		void Write( float _A ) {
			A = PF_Empty::ToByte( _A );
		}

		float	Red()	{ return R / 255.0f; }
		float	Green()	{ return G / 255.0f; }
		float	Blue()	{ return B / 255.0f; }
		float	Alpha()	{ return A / 255.0f; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f ); }

		#pragma endregion
	};

	/// <summary>
	/// RGBA8 sRGB format
	/// </summary>
	struct	PF_RGBA8_sRGB : public IPixelFormatAccessor {
		U8	R, G, B, A;

		#pragma region IPixelFormat Members

		bool sRGB() override { return true; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			R = (U8) _R;
			G = (U8) _G;
			B = (U8) _B;
			A = (U8) _A;
		}

		void Write( const bfloat4& _Color ) {
			R = PF_Empty::ToByte(_Color.x);
			G = PF_Empty::ToByte(_Color.y);
			B = PF_Empty::ToByte(_Color.z);
			A = PF_Empty::ToByte(_Color.w);
		}

		void Write( float _R, float _G, float _B, float _A ) {
			R = PF_Empty::ToByte( _R );
			G = PF_Empty::ToByte( _G );
			B = PF_Empty::ToByte( _B );
			A = PF_Empty::ToByte( _A );
		}

		void Write( U32 _A ) {
			A = (U8) _A;
		}

		void Write( float _A ) {
			A = PF_Empty::ToByte( _A );
		}

		float	Red()	{ return R / 255.0f; }
		float	Green()	{ return G / 255.0f; }
		float	Blue()	{ return B / 255.0f; }
		float	Alpha()	{ return A / 255.0f; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f ); }

		#pragma endregion
	};


	/// <summary>
	/// This format is a special encoding of 3 floating point values into 4 U8 values, aka "Real Pixels"
	/// The RGB encode the mantissa of each RGB float component while A encodes the exponent by which multiply these 3 mantissae
	/// In fact, we only use a single common exponent that we factor out to 3 different mantissae.
	/// This format was first created by Gregory Ward for his Radiance software (http://www.graphics.cornell.edu/~bjw/rgbe.html)
	///  and allows to store HDR values using standard 8-bits formats.
	/// It's also quite useful to pack some data as we divide the size by 3, from 3 floats (12 bytes) down to only 4 bytes.
	/// </summary>
	/// <remarks>This format only allows storage of POSITIVE floats !</remarks>
	struct	PF_RGBE : public IPixelFormatAccessor {
		U8	B, G, R, E;

		#pragma region IPixelFormat Members

		bool sRGB() override { return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _E ) {
			R = (U8) _R;
			G = (U8) _G;
			B = (U8) _B;
			E = (U8) _E;
		}

		// NOTE: Alpha is ignored, RGB is encoded in RGBE
		void Write( const bfloat4& _Color ) {
			float	maxComponent = MAX( _Color.x, MAX( _Color.y, _Color.z ) );
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

			R = (U8) (_Color.x * maxComponent);
			G = (U8) (_Color.y * maxComponent);
			B = (U8) (_Color.z * maxComponent);
			E = (U8) (exponent + 128 );
		}

		void Write( float _R, float _G, float _B, float _E ) {
			Write( bfloat4( _R, _G, _B, _E ) );
		}

		void Write( U32 _E ) {
			E = (U8) _E;
		}

		void Write( float _E ) {
			E = PF_Empty::ToByte( _E );
		}

		float	Red()	{ return DecodedColor().x; }
		float	Green()	{ return DecodedColor().y; }
		float	Blue()	{ return DecodedColor().z; }
		float	Alpha()	{ return 1.0f; }
		void	RGBA( bfloat4& _Color ) override { _Color = DecodedColor(); }

		#pragma endregion

		bfloat4	DecodedColor() {
			float exponent = powf( 2.0f, E - (128.0f + 8.0f) );
			return bfloat4(	(float) ((R + .5) * exponent),
							(float) ((G + .5) * exponent),
							(float) ((B + .5) * exponent),
							1.0f
						);
		}
	};

	#pragma endregion

	#pragma region 16-Bits Formats

	/// <summary>
	/// R16 format
	/// </summary>
	struct	PF_R16 : IPixelFormatAccessor {
		U16	R;

		#pragma region IPixelFormat Members

		bool sRGB() override { return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			R = (U16) _R;
		}

		void Write( const bfloat4& _Color ) {
			R = PF_Empty::ToUShort(_Color.x);
		}

		void Write( float _R, float _G, float _B, float _A ) {
			R = PF_Empty::ToUShort( _R );
		}

		void Write( U32 _A ) {
		}

		void Write( float _A ) {
		}

		float	Red()	{ return R / 65535.0f; }
		float	Green()	{ return 0.0f; }
		float	Blue()	{ return 0.0f; }
		float	Alpha()	{ return 1.0f; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( R / 65535.0f, 0, 0, 1 ); }

		#pragma endregion
	};

	/// <summary>
	/// RG16 format
	/// </summary>
	struct	PF_RG16 : IPixelFormatAccessor {
		U16	R, G;

		#pragma region IPixelFormat Members

		bool sRGB() override { return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			R = (U16) _R;
			G = (U16) _G;
		}

		void Write( const bfloat4& _Color ) {
			R = PF_Empty::ToUShort(_Color.x);
			G = PF_Empty::ToUShort(_Color.y);
		}

		void Write( float _R, float _G, float _B, float _A ) {
			R = PF_Empty::ToUShort( _R );
			G = PF_Empty::ToUShort( _G );
		}

		void Write( U32 _A ) {
		}

		void Write( float _A ) {
		}

		float	Red()	{ return R / 65535.0f; }
		float	Green()	{ return G / 65535.0f; }
		float	Blue()	{ return 0.0f; }
		float	Alpha()	{ return 1.0f; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( R / 65535.0f, G / 65535.0f, 0, 1 ); }

		#pragma endregion
	};

	/// <summary>
	/// RGBA16 format
	/// </summary>
	struct	PF_RGBA16 : IPixelFormatAccessor {
		U16	R, G, B, A;

		#pragma region IPixelFormat Members

		bool sRGB() override { return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			R = (U16) _R;
			G = (U16) _G;
			B = (U16) _B;
			A = (U16) _A;
		}

		void Write( const bfloat4& _Color ) {
			R = PF_Empty::ToUShort(_Color.x);
			G = PF_Empty::ToUShort(_Color.y);
			B = PF_Empty::ToUShort(_Color.z);
			A = PF_Empty::ToUShort(_Color.w);
		}

		void Write( float _R, float _G, float _B, float _A ) {
			R = PF_Empty::ToUShort( _R );
			G = PF_Empty::ToUShort( _G );
			B = PF_Empty::ToUShort( _B );
			A = PF_Empty::ToUShort( _A );
		}

		void Write( U32 _A ) {
			A = (U16) _A;
		}

		void Write( float _A ) {
			A = PF_Empty::ToUShort( _A );
		}

		float	Red()	{ return R / 65535.0f; }
		float	Green()	{ return G / 65535.0f; }
		float	Blue()	{ return B / 65535.0f; }
		float	Alpha()	{ return A / 65535.0f; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( R / 65535.0f, G / 65535.0f, B / 65535.0f, A / 65535.0f ); }

		#pragma endregion
	};

	#pragma endregion

	#pragma region 16 bits floating-point formats

	/// <summary>
	/// R16F format
	/// </summary>
	struct	PF_R16F : IPixelFormatAccessor {
		half	R;

		#pragma region IPixelFormat Members

		bool sRGB() override { return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			R = half( PF_Empty::ToFloat( _R ) );
		}

		void Write( const bfloat4& _Color ) {
			R = half( _Color.x );
		}

		void Write( float _R, float _G, float _B, float _A ) {
			R = half( _R );
		}

		void Write( U32 _A ) {
		}

		void Write( float _A ) {
		}

		float	Red()	{ return R; }
		float	Green()	{ return 0.0f; }
		float	Blue()	{ return 0.0f; }
		float	Alpha()	{ return 1.0f; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( R, 0, 0, 1 ); }

		#pragma endregion
	};

	/// <summary>
	/// RG16F format
	/// </summary>
	struct	PF_RG16F : IPixelFormatAccessor {
		half	R, G;

		#pragma region IPixelFormat Members

		bool sRGB() override { return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			R = half( PF_Empty::ToFloat( _R ) );
			G = half( PF_Empty::ToFloat( _G ) );
		}

		void Write( const bfloat4& _Color ) {
			R = half( _Color.x );
			G = half( _Color.y );
		}

		void Write( float _R, float _G, float _B, float _A ) {
			R = half( _R );
			G = half( _G );
		}

		void Write( U32 _A ) {
		}

		void Write( float _A ) {
		}

		float	Red()	{ return R; }
		float	Green()	{ return G; }
		float	Blue()	{ return 0.0f; }
		float	Alpha()	{ return 1.0f; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( R, G, 0, 1 ); }

		#pragma endregion
	};

	/// <summary>
	/// RGBA16F format
	/// </summary>
	struct	PF_RGBA16F : IPixelFormatAccessor {
		half	R, G, B, A;

		#pragma region IPixelFormat Members

		bool sRGB() override { return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			R = half( PF_Empty::ToFloat( _R ) );
			G = half( PF_Empty::ToFloat( _G ) );
			B = half( PF_Empty::ToFloat( _B ) );
			A = half( PF_Empty::ToFloat( _A ) );
		}

		void Write( const bfloat4& _Color ) {
			R = half( _Color.x );
			G = half( _Color.y );
			B = half( _Color.z );
			A = half( _Color.w );
		}

		void Write( float _R, float _G, float _B, float _A ) {
			R = half( _R );
			G = half( _G );
			B = half( _B );
			A = half( _A );
		}

		void Write( U32 _A ) {
			A = half( PF_Empty::ToFloat( _A ) );
		}

		void Write( float _A ) {
			A = _A;
		}

		float	Red()	{ return R; }
		float	Green()	{ return G; }
		float	Blue()	{ return B; }
		float	Alpha()	{ return A; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( R, G, 0, 1 ); }

		#pragma endregion
	};

	#pragma endregion

	#pragma region 32 bits floating points Formats

	/// <summary>
	/// R32F format
	/// </summary>
	struct	PF_R32F : IPixelFormatAccessor {
		float	R;

		#pragma region IPixelFormat Members

		bool sRGB() override { return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			R = PF_Empty::ToFloat(_R);
		}

		void Write( const bfloat4& _Color ) {
			R = _Color.x;
		}

		void Write( float _R, float _G, float _B, float _A ) {
			R = _R;
		}

		void Write( U32 _A ) {
		}

		void Write( float _A ) {
		}

		float	Red()	{ return R; }
		float	Green()	{ return 0.0f; }
		float	Blue()	{ return 0.0f; }
		float	Alpha()	{ return 1.0f; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( R, 0, 0, 1 ); }

		#pragma endregion
	};

	/// <summary>
	/// RG32F format
	/// </summary>
	struct	PF_RG32F : IPixelFormatAccessor {
		float	R, G;

		#pragma region IPixelFormat Members

		bool sRGB() override { return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			R = PF_Empty::ToFloat(_R);
			G = PF_Empty::ToFloat(_G);
		}

		void Write( const bfloat4& _Color ) {
			R = _Color.x;
			G = _Color.y;
		}

		void Write( float _R, float _G, float _B, float _A ) {
			R = _R;
			G = _G;
		}

		void Write( U32 _A ) {
		}

		void Write( float _A ) {
		}

		float	Red()	{ return R; }
		float	Green()	{ return G; }
		float	Blue()	{ return 0.0f; }
		float	Alpha()	{ return 1.0f; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( R, G, 0, 1 ); }

		#pragma endregion
	};

	/// <summary>
	/// RGB32F format
	/// </summary>
	struct	PF_RGB32F : IPixelFormatAccessor {
		float	R, G, B;

		#pragma region IPixelFormat Members

		bool sRGB() override { return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			R = PF_Empty::ToFloat(_R);
			G = PF_Empty::ToFloat(_G);
			B = PF_Empty::ToFloat(_B);
		}

		void Write( const bfloat4& _Color ) {
			R = _Color.x;
			G = _Color.y;
			B = _Color.z;
		}

		void Write( float _R, float _G, float _B, float _A ) {
			R = _R;
			G = _G;
			B = _B;
		}

		void Write( U32 _A ) {
		}

		void Write( float _A ) {
		}

		float	Red()	{ return R; }
		float	Green()	{ return G; }
		float	Blue()	{ return B; }
		float	Alpha()	{ return 1.0f; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( R, G, B, 1 ); }

		#pragma endregion
	};

	/// <summary>
	/// RGBA32F format
	/// </summary>
	struct	PF_RGBA32F : IPixelFormatAccessor {
		float	R, G, B, A;

		#pragma region IPixelFormat Members

		bool sRGB() override { return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			R = PF_Empty::ToFloat(_R);
			G = PF_Empty::ToFloat(_G);
			B = PF_Empty::ToFloat(_B);
			A = PF_Empty::ToFloat(_A);
		}

		void Write( const bfloat4& _Color ) {
			R = _Color.x;
			G = _Color.y;
			B = _Color.z;
			A = _Color.w;
		}

		void Write( float _R, float _G, float _B, float _A ) {
			R = _R;
			G = _G;
			B = _B;
			A = _A;
		}

		void Write( U32 _A ) {
			A = PF_Empty::ToFloat(_A);
		}

		void Write( float _A ) {
			A = _A;
		}

		float	Red()	{ return R; }
		float	Green()	{ return G; }
		float	Blue()	{ return B; }
		float	Alpha()	{ return A; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( R, G, B, A ); }

		#pragma endregion
	};

	#pragma endregion

	#pragma region Depth Formats

// 	/// <summary>
// 	/// D16 format
// 	/// </summary>
// 	[StructLayout( LayoutKind.Sequential )]
// 	struct	PF_D16 : IDepthFormat
// 	{
// 		Half	D;
// 
// 		#pragma region IPixelFormat Members
// 
// 		Format DirectXFormat
// 		{
// 			get { return Format.D16_UNorm; }
// 		}
// 
// 		bool sRGB() override { return false; }
// 
// 		void Write( U32 _R, U32 _G, U32 _B, U32 _A )
// 		{
// 			D = (Half) PF_Empty::ToFloat(_R);
// 		}
// 
// 		void Write( float4& _Color )
// 		{
// 			D = (Half) _Color.X;
// 		}
// 
// 		void Write( float _R, float _G, float _B, float _A )
// 		{
// 			D = (Half) _R;
// 		}
// 
// 		void Write( U32 _A )
// 		{
// 		}
// 
// 		void Write( float _A )
// 		{
// 		}
// 
// 		float	Red()  { return D; }
// 		float	Green()  { return 0.0f; }
// 		float	Blue()  { return 0.0f; }
// 		float	Alpha()  { return 1.0f; }
// 
// 		#pragma endregion
// 	}

	/// <summary>
	/// D32 format
	/// </summary>
	struct	PF_D32 : IDepthFormatAccessor {
		float	D;

		#pragma region IPixelFormat Members

		bool sRGB() override { return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			D = PF_Empty::ToFloat(_R);
		}

		void Write( const bfloat4& _Color ) {
			D = _Color.x;
		}

		void Write( float _R, float _G, float _B, float _A ) {
			D = _R;
		}

		void Write( U32 _A ) {
		}

		void Write( float _A ) {
		}

		float	Red()	{ return D; }
		float	Green()	{ return 0.0f; }
		float	Blue()	{ return 0.0f; }
		float	Alpha()	{ return 1.0f; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( D, 0, 0, 1 ); }

		#pragma endregion
	};
	
	/// <summary>
	/// D24S8 format (24 bits depth + 8 bits stencil)
	/// NOTE: This format is NOT readable and will throw an exception if used for a readable depth stencil !
	/// </summary>
	struct	PF_D24S8 : IDepthFormatAccessor {
		U8	R, G, B, A;

		#pragma region IPixelFormat Members

		bool sRGB() override { return false; }

		void Write( U32 _R, U32 _G, U32 _B, U32 _A ) {
			R = (U8) _R;
			G = (U8) _G;
			B = (U8) _B;
			A = (U8) _A;
		}

		void Write( const bfloat4& _Color ) {
			R = PF_Empty::ToByte(_Color.x);
			G = PF_Empty::ToByte(_Color.y);
			B = PF_Empty::ToByte(_Color.z);
			A = PF_Empty::ToByte(_Color.w);
		}

		void Write( U32 _A ) {
			A = (U8) _A;
		}

		void Write( float _A ) {
			A = PF_Empty::ToByte( _A );
		}

		void Write( float _R, float _G, float _B, float _A ) {
			A = PF_Empty::ToByte( _A );
		}

		float	Red()	{ return R / 255.0f; }
		float	Green()	{ return G / 255.0f; }
		float	Blue()	{ return B / 255.0f; }
		float	Alpha()	{ return A / 255.0f; }
		void	RGBA( bfloat4& _Color ) override { _Color.Set( R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f ); }

		#pragma endregion
	};

	#pragma endregion

}	// namespace BaseLib