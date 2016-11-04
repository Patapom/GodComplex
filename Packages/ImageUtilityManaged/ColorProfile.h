//////////////////////////////////////////////////////////////////////////
//
//////////////////////////////////////////////////////////////////////////
//
#pragma once

#pragma unmanaged
#include "..\ImageUtilityLib\ColorProfile.h"
#pragma managed

//#include "..\MathSimple\Math.h"

using namespace System;

namespace ImageUtility {

	public ref class ColorProfile {
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

		// Enumerates the various supported gamma curves
		enum class GAMMA_CURVE {
			STANDARD,		// Standard gamma curve using a single exponent and no linear slope
			sRGB,			// sRGB gamma with linear slope
			PRO_PHOTO,		// ProPhoto gamma with linear slope
		};

		// Describes the Red, Green, Blue and White Point chromaticities of a simple/standard color profile
		[System::Diagnostics::DebuggerDisplay( "R=({R.x},{R.y}) G=({G.x},{G.y}) B=({B.x},{B.y}) W=({W.x},{W.y}) Prof={RecognizedChromaticity}" )]
		ref struct	Chromaticities {
		internal:
			ImageUtilityLib::ColorProfile::Chromaticities*	m_nativeObject;

		public:

			Chromaticities( float xr, float yr, float xg, float yg, float xb, float yb, float xw, float yw ) {
				m_nativeObject = new ImageUtilityLib::ColorProfile::Chromaticities( xr, yr, xg, yg, xb, yb, xw, yw );
			}
			Chromaticities( SharpMath::float2 r, SharpMath::float2 g, SharpMath::float2 b, SharpMath::float2 w ) {
				m_nativeObject = new ImageUtilityLib::ColorProfile::Chromaticities( ::float2( r.x, r.y ), ::float2( g.x, g.y ), ::float2( b.x, b.y ), ::float2( w.x, w.y ) );
			}
			~Chromaticities() {
				SAFE_DELETE( m_nativeObject );
			}

// 			public static Chromaticities	Empty			{ get { return new Chromaticities() { R = new float2(), G = new float2(), B = new float2(), W = new float2() }; } }
// 			public static Chromaticities	sRGB			{ get { return new Chromaticities() { R = new float2( 0.6400f, 0.3300f ), G = new float2( 0.3000f, 0.6000f ), B = new float2( 0.1500f, 0.0600f ), W = ILLUMINANT_D65 }; } }
// 			public static Chromaticities	AdobeRGB_D50	{ get { return new Chromaticities() { R = new float2( 0.6400f, 0.3300f ), G = new float2( 0.2100f, 0.7100f ), B = new float2( 0.1500f, 0.0600f ), W = ILLUMINANT_D50 }; } }
// 			public static Chromaticities	AdobeRGB_D65	{ get { return new Chromaticities() { R = new float2( 0.6400f, 0.3300f ), G = new float2( 0.2100f, 0.7100f ), B = new float2( 0.1500f, 0.0600f ), W = ILLUMINANT_D65 }; } }
// 			public static Chromaticities	ProPhoto		{ get { return new Chromaticities() { R = new float2( 0.7347f, 0.2653f ), G = new float2( 0.1596f, 0.8404f ), B = new float2( 0.0366f, 0.0001f ), W = ILLUMINANT_D50 }; } }
// 			public static Chromaticities	Radiance		{ get { return new Chromaticities() { R = new float2( 0.6400f, 0.3300f ), G = new float2( 0.2900f, 0.6000f ), B = new float2( 0.1500f, 0.0600f ), W = ILLUMINANT_E }; } }

			/// <summary>
			/// Attempts to recognize the current chromaticities as a standard profile
			/// </summary>
			/// <returns></returns>
			property STANDARD_PROFILE	RecognizedChromaticity {
				STANDARD_PROFILE	get() {
					ImageUtilityLib::ColorProfile::STANDARD_PROFILE	nativeStandardProfile = m_nativeObject->FindRecognizedChromaticity();
					return STANDARD_PROFILE( nativeStandardProfile );
				}
			}
		};

		#pragma endregion

	internal:
		const ImageUtilityLib::ColorProfile*	m_nativeObject;

		// Special wrapper constructor
		ColorProfile( const ImageUtilityLib::ColorProfile* _nativeObject ) {
			m_nativeObject = _nativeObject;
		}

	public:
		ColorProfile() {
			m_nativeObject = new ImageUtilityLib::ColorProfile();
		}
		ColorProfile( STANDARD_PROFILE _profile ) {
			m_nativeObject = new ImageUtilityLib::ColorProfile();
		}
		ColorProfile( const Chromaticities^ _chromaticities, GAMMA_CURVE _gammaCurve, float _gammaExponent ) {
			m_nativeObject = new ImageUtilityLib::ColorProfile( *_chromaticities->m_nativeObject, ImageUtilityLib::ColorProfile::GAMMA_CURVE( _gammaCurve ), _gammaExponent );
		}
		~ColorProfile() {
			SAFE_DELETE( m_nativeObject );
		}
	};
}
