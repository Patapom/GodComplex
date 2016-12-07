//////////////////////////////////////////////////////////////////////////
// The bitmap class cannot work without a proper definition of a Color Profile
// Because all image systems sooner or later work with a device-dependent RGB color space,
//	we need to offer a robust bridge between the (device-dependent) RGB color space and the
//	reference (device-independent) XYZ color space that is used internally by the Bitmap class
//
// The color profile serves as a bridge between device-dependent color spaces like:
//	• RGB
//	• CMYK
//	• HSL / HSB / HSI
//	• RGBE
//	• YCoCg
//	• YCbCr
// 
// and device-independent color spaces like:
//	• CIE XYZ	(our reference space)
//	• CIE xyY	(a variation on XYZ)
//	• CIE Lab
//
////////////////////////////////////////////////////////////////////////////
//
#pragma once

// Bastard windows defined this in wingdi.h!!!
#undef ILLUMINANT_A   
#undef ILLUMINANT_B   
#undef ILLUMINANT_C   
#undef ILLUMINANT_D50 
#undef ILLUMINANT_D55 
#undef ILLUMINANT_D65 

namespace ImageUtilityLib {

	using namespace BaseLib;

	/// <summary>
	/// Defines a color converter that can handle transforms between XYZ and RGB
	/// Usually implemented by a ColorProfile so the RGB color is fully characterized
	/// </summary>
	class IColorConverter {
	public:
		virtual void		XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const abstract;
		virtual void		RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const abstract;
		virtual void		XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const abstract;
		virtual void		RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const abstract;

		// Converts between gamma- and linear-space RGB
		virtual void		GammaRGB2LinearRGB( const bfloat4& _gammaRGB, bfloat4& _linearRGB ) const abstract;
		virtual void		LinearRGB2GammaRGB( const bfloat4& _linearRGB, bfloat4& _gammaRGB ) const abstract;
	};

	// The source color for the bitmap
	// The color profile helps converting between the original color space and the internal CIEXYZ color space used in the Bitmap class
	// 
	// For now, only standard profiles like Linear, sRGB, Adobe RGB, ProPhoto RGB or any custom chromaticities are supported.
	// I believe it would be overkill to include a library for parsing embedded ICC profiles...
	//
	class	ColorProfile : IColorConverter {
	public:
		#pragma region CONSTANTS

		static const bfloat2		ILLUMINANT_A;	// Incandescent, tungsten
		static const bfloat2		ILLUMINANT_D50;	// Daylight, Horizon
		static const bfloat2		ILLUMINANT_D55;	// Mid-Morning, Mid-Afternoon
		static const bfloat2		ILLUMINANT_D65;	// Daylight, Noon, Overcast (sRGB reference illuminant)
		static const bfloat2		ILLUMINANT_E;	// Reference

		static const float			GAMMA_EXPONENT_STANDARD;// = 2.2f;
		static const float			GAMMA_EXPONENT_sRGB;// = 2.4f;
		static const float			GAMMA_EXPONENT_ADOBE;// = 2.19921875f;
		static const float			GAMMA_EXPONENT_PRO_PHOTO;// = 1.8f;

		// Color Matching Functions
		static const float			CMF_WAVELENGTH_START;	// 390 nm
		static const float			CMF_WAVELENGTH_END;		// 830 nm
		static const float			CMF_WAVELENGTH_STEP;	// 0.1 nm
		static const float			CMF_WAVELENGTH_RCP_STEP;

		#pragma endregion

	public:
		#pragma region NESTED TYPES

		enum class STANDARD_PROFILE {
			INVALID,		// The profile is invalid (meaning one of the chromaticities was not initialized!)
			CUSTOM,			// No recognizable standard profile (custom)
			LINEAR,			// sRGB with linear gamma
			sRGB,			// sRGB with D65 illuminant
			ADOBE_RGB_D50,	// Adobe RGB with D50 illuminant
			ADOBE_RGB_D65,	// Adobe RGB with D65 illuminant
			PRO_PHOTO,		// ProPhoto with D50 illuminant
			RADIANCE,		// Radiance HDR format with E illuminant
		};

		/// <summary>
		/// Enumerates the various supported gamma curves
		/// </summary>
		enum class GAMMA_CURVE {
			STANDARD,		// Standard gamma curve using a single exponent and no linear slope
			sRGB,			// sRGB gamma with linear slope
			PRO_PHOTO,		// ProPhoto gamma with linear slope
		};

		/// <summary>
		/// Describes the Red, Green, Blue and White Point chromaticities of a simple/standard color profile
		/// </summary>
		struct	Chromaticities {
			bfloat2		R, G, B, W;

			Chromaticities() {}
			Chromaticities( const Chromaticities& _other ) {
				R = _other.R;
				G = _other.G;
				B = _other.B;
				W = _other.W;
			}
			Chromaticities( const bfloat2& r, const bfloat2& g, const bfloat2& b, const bfloat2& w ) {
				R = r;
				G = g;
				B = b;
				W = w;
			}
			Chromaticities( float xr, float yr, float xg, float yg, float xb, float yb, float xw, float yw ) {
				R.Set( xr, yr );
				G.Set( xg, yg );
				B.Set( xb, yb );
				W.Set( xw, yw );
			}

			static const Chromaticities	Empty;
			static const Chromaticities	sRGB;
			static const Chromaticities	AdobeRGB_D50;
			static const Chromaticities	AdobeRGB_D65;
			static const Chromaticities	ProPhoto;
			static const Chromaticities	Radiance;

			/// <summary>
			/// Attempts to recognize the current chromaticities as a standard profile
			/// </summary>
			/// <returns></returns>
			STANDARD_PROFILE	FindRecognizedChromaticity() const {
				if ( Equals( sRGB ) )
					return STANDARD_PROFILE::sRGB;
				if ( Equals( AdobeRGB_D65 ) )
					return STANDARD_PROFILE::ADOBE_RGB_D65;
				if ( Equals( AdobeRGB_D50 ) )
					return STANDARD_PROFILE::ADOBE_RGB_D50;
				if ( Equals( ProPhoto ) )
					return STANDARD_PROFILE::PRO_PHOTO;
				if ( Equals( Radiance ) )
					return STANDARD_PROFILE::RADIANCE;

				// Ensure the profile is valid
				return R.x != 0.0f && R.y != 0.0f && G.x != 0.0f && G.y != 0.0f && B.x != 0.0f && B.y != 0.0f && W.x != 0.0f && W.y != 0.0f ? STANDARD_PROFILE::CUSTOM : STANDARD_PROFILE::INVALID;
			}

			bool	Equals( const Chromaticities& other, float _eps=1e-3f ) const {
				return R.Almost( other.R, _eps )
					&& G.Almost( other.G, _eps )
					&& B.Almost( other.B, _eps )
					&& W.Almost( other.W, _eps );
			}
		};

	protected:

		#pragma region Internal XYZ<->RGB Converters

		class		InternalColorConverter_sRGB : public IColorConverter {
		public:
			static const float3x3 MAT_RGB2XYZ;
			static const float3x3 MAT_XYZ2RGB;
			void		XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const override;
			void		RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const override;
			void		XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const override;
			void		RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const override;
			void		GammaRGB2LinearRGB( const bfloat4& _gammaRGB, bfloat4& _linearRGB ) const override;
			void		LinearRGB2GammaRGB( const bfloat4& _linearRGB, bfloat4& _gammaRGB ) const override;
		};

		class		InternalColorConverter_AdobeRGB_D50 : public IColorConverter {
		public:
			static const float3x3 MAT_RGB2XYZ;
			static const float3x3 MAT_XYZ2RGB;
			void		XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const override;
			void		RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const override;
			void		XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const override;
			void		RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const override;
			void		GammaRGB2LinearRGB( const bfloat4& _gammaRGB, bfloat4& _linearRGB ) const override;
			void		LinearRGB2GammaRGB( const bfloat4& _linearRGB, bfloat4& _gammaRGB ) const override;
		};

		class		InternalColorConverter_AdobeRGB_D65 : public IColorConverter {
		public:
			static const float3x3 MAT_RGB2XYZ;
			static const float3x3 MAT_XYZ2RGB;
			void		XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const override;
			void		RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const override;
			void		XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const override;
			void		RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const override;
			void		GammaRGB2LinearRGB( const bfloat4& _gammaRGB, bfloat4& _linearRGB ) const override;
			void		LinearRGB2GammaRGB( const bfloat4& _linearRGB, bfloat4& _gammaRGB ) const override;
		};

		class		InternalColorConverter_ProPhoto : public IColorConverter {
		public:
			static const float3x3 MAT_RGB2XYZ;
			static const float3x3 MAT_XYZ2RGB;
			void		XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const override;
			void		RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const override;
			void		XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const override;
			void		RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const override;
			void		GammaRGB2LinearRGB( const bfloat4& _gammaRGB, bfloat4& _linearRGB ) const override;
			void		LinearRGB2GammaRGB( const bfloat4& _linearRGB, bfloat4& _gammaRGB ) const override;
		};

		class		InternalColorConverter_Radiance : public IColorConverter {
		public:
			static const float3x3 MAT_RGB2XYZ;
			static const float3x3 MAT_XYZ2RGB;
			void		XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const override;
			void		RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const override;
			void		XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const override;
			void		RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const override;
			void		GammaRGB2LinearRGB( const bfloat4& _gammaRGB, bfloat4& _linearRGB ) const override;
			void		LinearRGB2GammaRGB( const bfloat4& _linearRGB, bfloat4& _gammaRGB ) const override;
		};

		class		InternalColorConverter_Generic_NoGamma : public IColorConverter {
			float3x3	m_RGB2XYZ;
			float3x3	m_XYZ2RGB;

		public:
			InternalColorConverter_Generic_NoGamma( const float3x3& _RGB2XYZ, const float3x3& _XYZ2RGB ) {
				m_RGB2XYZ = _RGB2XYZ;
				m_XYZ2RGB = _XYZ2RGB;
			}
			void		XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const override;
			void		RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const override;
			void		XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const override;
			void		RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const override;
			void		GammaRGB2LinearRGB( const bfloat4& _gammaRGB, bfloat4& _linearRGB ) const override;
			void		LinearRGB2GammaRGB( const bfloat4& _linearRGB, bfloat4& _gammaRGB ) const override;
		};

		class		InternalColorConverter_Generic_StandardGamma : public IColorConverter {
			float3x3	m_RGB2XYZ;
			float3x3	m_XYZ2RGB;
			float		m_Gamma;
			float		m_InvGamma;

		public:
			InternalColorConverter_Generic_StandardGamma( const float3x3& _RGB2XYZ, const float3x3& _XYZ2RGB, float _Gamma ) {
				m_RGB2XYZ = _RGB2XYZ;
				m_XYZ2RGB = _XYZ2RGB;
				m_Gamma = _Gamma;
				m_InvGamma = 1.0f / _Gamma;
			}
			void		XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const override;
			void		RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const override;
			void		XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const override;
			void		RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const override;
			void		GammaRGB2LinearRGB( const bfloat4& _gammaRGB, bfloat4& _linearRGB ) const override;
			void		LinearRGB2GammaRGB( const bfloat4& _linearRGB, bfloat4& _gammaRGB ) const override;
		};

		class		InternalColorConverter_Generic_sRGBGamma : public IColorConverter {
			float3x3	m_RGB2XYZ;
			float3x3	m_XYZ2RGB;

		public:
			InternalColorConverter_Generic_sRGBGamma( const float3x3& _RGB2XYZ, const float3x3& _XYZ2RGB ) {
				m_RGB2XYZ = _RGB2XYZ;
				m_XYZ2RGB = _XYZ2RGB;
			}
			void		XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const override;
			void		RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const override;
			void		XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const override;
			void		RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const override;
			void		GammaRGB2LinearRGB( const bfloat4& _gammaRGB, bfloat4& _linearRGB ) const override;
			void		LinearRGB2GammaRGB( const bfloat4& _linearRGB, bfloat4& _gammaRGB ) const override;
		};

		class		InternalColorConverter_Generic_ProPhoto : public IColorConverter {
			float3x3	m_RGB2XYZ;
			float3x3	m_XYZ2RGB;

		public:
			InternalColorConverter_Generic_ProPhoto( const float3x3& _RGB2XYZ, const float3x3& _XYZ2RGB ) {
				m_RGB2XYZ = _RGB2XYZ;
				m_XYZ2RGB = _XYZ2RGB;
			}
			void		XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const override;
			void		RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const override;
			void		XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const override;
			void		RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const override;
			void		GammaRGB2LinearRGB( const bfloat4& _gammaRGB, bfloat4& _linearRGB ) const override;
			void		LinearRGB2GammaRGB( const bfloat4& _linearRGB, bfloat4& _gammaRGB ) const override;
		};

		#pragma endregion

		#pragma endregion

	protected:
		#pragma region FIELDS

		bool				m_profileFoundInFile;
		Chromaticities		m_chromaticities;
		GAMMA_CURVE			m_gammaCurve;
		float				m_gamma;

		float3x3			m_RGB2XYZ;
		float3x3			m_XYZ2RGB;

		IColorConverter*	m_internalConverter;
 
		static double		ms_colorMatchingFunctions[];

		#pragma endregion

	public:
		#pragma region PROPERTIES

		/// <summary>
		/// Gets the chromaticities attached to the profile
		/// </summary>
		Chromaticities&			GetChromas()								{ return m_chromaticities; }
		const Chromaticities&	GetChromas() const							{ return m_chromaticities; }
		void					SetChromas( const Chromaticities& value )	{
			memcpy_s( &m_chromaticities, sizeof(Chromaticities), &value, sizeof(Chromaticities) );
			BuildTransformFromChroma( true );
		}

		/// <summary>
		/// Gets the transform to convert RGB to CIEXYZ
		/// </summary>
		const float3x3&			GetMatrixRGB2XYZ() const { return m_RGB2XYZ; }

		/// <summary>
		/// Gets the transform to convert CIEXYZ to RGB
		/// </summary>
		const float3x3&			GetMatrixXYZ2RGB() const { return m_XYZ2RGB; }

		/// <summary>
		/// Gets or sets the image gamma curve
		/// </summary>
		GAMMA_CURVE				GetGammaCurve() const	{ return m_gammaCurve; }
		void					SetGammaCurve( GAMMA_CURVE value ) {
			m_gammaCurve = value;
			BuildTransformFromChroma( true );
		}

		/// <summary>
		/// Gets or sets the image gamma
		/// </summary>
		float					GetGammaExponent() const { return m_gamma; }
		void					SetGammaExponent( float value ) {
			m_gamma = value;
			BuildTransformFromChroma( true );
		}

		/// <summary>
		/// True if the profile was found in the file's metadata and can be considered accurate.
		/// False if it's the default assumed profile and may NOT be the actual image's profile.
		/// </summary>
		bool					GetProfileFoundInFile() const		{ return m_profileFoundInFile; }
		void					SetProfileFoundInFile( bool value ) { m_profileFoundInFile = value; }

		#pragma endregion

	public:
		#pragma region METHODS

		ColorProfile()
			: m_profileFoundInFile( false )
			, m_chromaticities( Chromaticities::Empty )
			, m_gammaCurve( GAMMA_CURVE::STANDARD )
			, m_gamma( 1.0f )
			, m_RGB2XYZ( float3x3::Identity )
			, m_XYZ2RGB( float3x3::Identity )
			, m_internalConverter( nullptr ) {
		}
		ColorProfile( const ColorProfile& _other );

		/// <summary>
		/// Build from a standard profile
		/// </summary>
		/// <param name="_Profile"></param>
		ColorProfile( STANDARD_PROFILE _profile )
			: m_profileFoundInFile( false )
			, m_internalConverter( nullptr )
		{
			switch ( _profile ) {
				case STANDARD_PROFILE::LINEAR:
					m_chromaticities = Chromaticities::sRGB;
					m_gammaCurve = GAMMA_CURVE::STANDARD;
					m_gamma = 1.0f;
					break;
				case STANDARD_PROFILE::sRGB:
					m_chromaticities = Chromaticities::sRGB;
					m_gammaCurve = GAMMA_CURVE::sRGB;
					m_gamma = GAMMA_EXPONENT_sRGB;
					break;
				case STANDARD_PROFILE::ADOBE_RGB_D50:
					m_chromaticities = Chromaticities::AdobeRGB_D50;
					m_gammaCurve = GAMMA_CURVE::STANDARD;
					m_gamma = GAMMA_EXPONENT_ADOBE;
					break;
				case STANDARD_PROFILE::ADOBE_RGB_D65:
					m_chromaticities = Chromaticities::AdobeRGB_D65;
					m_gammaCurve = GAMMA_CURVE::STANDARD;
					m_gamma = GAMMA_EXPONENT_ADOBE;
					break;
				case STANDARD_PROFILE::PRO_PHOTO:
					m_chromaticities = Chromaticities::ProPhoto;
					m_gammaCurve = GAMMA_CURVE::PRO_PHOTO;
					m_gamma = GAMMA_EXPONENT_PRO_PHOTO;
					break;
				case STANDARD_PROFILE::RADIANCE:
					m_chromaticities = Chromaticities::Radiance;
					m_gammaCurve = GAMMA_CURVE::STANDARD;
					m_gamma = 1.0f;
					break;
				default:
					throw "Unsupported standard profile!";
			}

			BuildTransformFromChroma( true );
		}

		/// <summary>
		/// Creates a color profile from chromaticities
		/// </summary>
		/// <param name="_Chromaticities">The chromaticities for this profile</param>
		/// <param name="_GammaCurve">The type of gamma curve to use</param>
		/// <param name="_Gamma">The gamma power</param>
		ColorProfile( const Chromaticities& _chromaticities, GAMMA_CURVE _gammaCurve, float _gamma )
			: m_profileFoundInFile( false )
			, m_internalConverter( nullptr )
		{
			m_chromaticities = _chromaticities;
			m_gammaCurve = _gammaCurve;
			m_gamma = _gamma;

			BuildTransformFromChroma( true );
		}

		virtual ~ColorProfile() {
			SAFE_DELETE( m_internalConverter );
		}

		#pragma region IColorConverter Members

		/// <summary>
		/// Converts a CIEXYZ color to a RGB color
		/// </summary>
		/// <param name="_XYZ"></param>
		/// <returns></returns>
		void	XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
			m_internalConverter->XYZ2RGB( _XYZ, _RGB );
		}

		/// <summary>
		/// Converts a RGB color to a CIEXYZ color
		/// </summary>
		/// <param name="_RGB"></param>
		/// <returns></returns>
		void	RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
			m_internalConverter->RGB2XYZ( _RGB, _XYZ );
		}

		/// <summary>
		/// Converts a CIEXYZ color to a RGB color
		/// </summary>
		/// <param name="_XYZ"></param>
		void	XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
			m_internalConverter->XYZ2RGB( _XYZ, _RGB, _length );
		}

		/// <summary>
		/// Converts a RGB color to a CIEXYZ color
		/// </summary>
		/// <param name="_RGB"></param>
		void	RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
			m_internalConverter->RGB2XYZ( _RGB, _XYZ, _length );
		}

		void		GammaRGB2LinearRGB( const bfloat4& _gammaRGB, bfloat4& _linearRGB ) const {
			m_internalConverter->GammaRGB2LinearRGB( _gammaRGB, _linearRGB );
		}
		void		LinearRGB2GammaRGB( const bfloat4& _linearRGB, bfloat4& _gammaRGB ) const {
			m_internalConverter->LinearRGB2GammaRGB( _linearRGB, _gammaRGB );
		}

		#pragma endregion

	public:

		#pragma region Helpers

		/// <summary>
		/// Converts from XYZ to xyY
		/// </summary>
		/// <param name="_XYZ"></param>
		/// <returns></returns>
		static void	XYZ2xyY( const bfloat3& _XYZ, bfloat3& _xyY ) {
			float	InvSum = _XYZ.x + _XYZ.y + _XYZ.z;
					InvSum = InvSum > 1e-8f ? 1.0f / InvSum : 0.0f;
			_xyY.Set( _XYZ.x * InvSum, _XYZ.y * InvSum, _XYZ.y );
		}

		/// <summary>
		/// Converts from xyY to XYZ
		/// </summary>
		/// <param name="_xyY"></param>
		/// <returns></returns>
		static void	xyY2XYZ( const bfloat3& _xyY, bfloat3& _XYZ ) {
			float	Y_y = _xyY.y > 1e-8f ? _xyY.z / _xyY.y : 0.0f;
			_XYZ.Set( _xyY.x * Y_y, _xyY.z, (1.0f - _xyY.x - _xyY.y) * Y_y );
		}

		/// <summary>
		/// Applies gamma correction to the provided color
		/// </summary>
		/// <param name="c">The color to gamma-correct</param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		/// <returns></returns>
		inline static float	GammaCorrect( float c, float _imageGamma ) {
			return powf( c, 1.0f / _imageGamma );
		}

		/// <summary>
		/// Un-aplies gamma correction to the provided color
		/// </summary>
		/// <param name="c">The color to gamma-uncorrect</param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		/// <returns></returns>
		inline static float	GammaUnCorrect( float c, float _imageGamma ) {
			return powf( c, _imageGamma );
		}

		/// <summary>
		/// Converts from linear space to sRGB
		/// Code borrowed from D3DX_DXGIFormatConvert.inl from the DX10 SDK
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		inline static float		Linear2sRGB( float c ) {
			if ( c < 0.0031308f )
				return c * 12.92f;
			return 1.055f * powf( c, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f;
		}

		/// <summary>
		/// Converts from sRGB to linear space
		/// Code borrowed from D3DX_DXGIFormatConvert.inl from the DX10 SDK
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		inline static float		sRGB2Linear( float c ) {
			if ( c < 0.04045f )
				return c / 12.92f;
			return powf( (c + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
		}

		//////////////////////////////////////////////////////////////////////////
		// Spectral Power Conversions and Chromaticity Helpers

		// Computes the XYZ matrix to perform white balancing between 2 white points assuming the R,G,B chromaticities are the same for the input and output profiles
		// The resulting matrix is used like this:
		//	XYZ_out = XYZ_in * M
		// The XYZ_in value comes from a device-dependent profile where the white point is _whitePointIn
		// The XYZ_out value is ready to be used with the profile _profileOut
		//
		static void				ComputeWhiteBalanceXYZMatrix( const bfloat2& _whitePointIn, const Chromaticities& _profileOut, float3x3& _whiteBalanceMatrix );
		static void				ComputeWhiteBalanceXYZMatrix( const Chromaticities& _profileIn, const bfloat2& _whitePointOut, float3x3& _whiteBalanceMatrix );
		static void				ComputeWhiteBalanceXYZMatrix( const bfloat2& _xyR, const bfloat2& _xyG, const bfloat2& _xyB, const bfloat2& _whitePointIn, const bfloat2& _whitePointOut, float3x3& _whiteBalanceMatrix );

		// Computes the power of a black body radiator 
		//	_blackBodyTemperature, the temperature of the black body (in Kelvin)
		//	_wavelength, the wavelength at which to compute the power (in nm)
		static double			ComputeBlackBodyRadiationPower( float _blackBodyTemperature, float _wavelength );

		// Integrates the provided Spectral Power Distribution into CIE XYZ tristimulus value
		//	_wavelengthsCount, the amount of wavelengths present in the distribution
		//	_wavelengthStart, the start wavelength (in nm)
		//	_wavelengthStep, the step in wavelength (in nm)
		//	_spectralPowerDistibution, the intensities for each wavelength
		//	_XYZ, the resulting CIE XYZ tristimulus value resulting from the integrtiton
		static void				IntegrateSpectralPowerDistributionIntoXYZ( U32 _wavelengthsCount, float _wavelengthStart, float _wavelengthStep, double* _spectralPowerDistibution, bfloat3& _XYZ );

		// Generates the Spectral Power Distribution for a black body radiator given its temperature
		//	_blackBodyTemperature, the temperature of the black body (in Kelvin)
		//	_wavelengthsCount, the amount of wavelengths to generate
		//	_wavelengthStart, the start wavelength (in nm)
		//	_wavelengthStep, the step in wavelength (in nm)
		//	_spectralPowerDistibution, the intensities for each wavelength
		static void				BuildSpectralPowerDistributionForBlackBody( float _blackBodyTemperature, U32 _wavelengthsCount, float _wavelengthStart, float _wavelengthStep, List< double >& _spectralPowerDistribution );

		// Computes the xy chromaticities of the white point given by a black body at specified temperature
		//	_blackBodyTemperature, the temperature of the black body (in Kelvin)
		//	_whitePointChromaticities, the resulting white point chromaticities in xyY space (Y=1)
		static void				ComputeWhitePointChromaticities( float _blackBodyTemperature, bfloat2& _whitePointChromaticities );

		// Computes the xy chromaticities of the white point given by a black body at specified temperature
		// This is the analytical solution by Judd, MacAdam, and Wyszecki described in http://wiki.nuaj.net/index.php?title=Illuminant_Computation
		//	_blackBodyTemperature, the temperature of the black body (in Kelvin)
		//							(NOTE: valid temperature range is from 4000K to 25000K!)
		//	_whitePointChromaticities, the resulting white point chromaticities in xyY space (Y=1)
		static void				ComputeWhitePointChromaticitiesAnalytical( float _blackBodyTemperature, bfloat2& _whitePointChromaticities );

		#pragma endregion

	protected:

		#pragma region Color Space Transforms

		// Builds the RGB<->XYZ transforms from chromaticities
		// (refer to http://wiki.nuaj.net/index.php/Color_Transforms#XYZ_Matrices for explanations)
		void	BuildTransformFromChroma( bool _checkGammaCurveOverride );

		// Ensures the current gamma curve type and value are the ones we want
		bool	EnsureGamma( GAMMA_CURVE _Curve, float _Gamma ) const {
			return m_gammaCurve == _Curve && fabs( _Gamma - m_gamma ) < 1e-3f;
		}

		#pragma endregion

		#pragma endregion
	};
};