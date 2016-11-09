#pragma once

using namespace System;

#include "ColorProfile.h"

namespace ImageUtility {

	ref class ImageFile;

	// Holds the image's color profile as well as important shot information pulled from EXIF data
	// You may want to have a look at APEX format to understand Tv and Av settings (https://en.wikipedia.org/wiki/APEX_system)
	//
	public ref class MetaData {
	private:
		const ImageUtilityLib::MetaData*	m_nativeObject;

	public:	// PROPERTIES

		property ImageUtility::ColorProfile^		ColorProfile {
			ImageUtility::ColorProfile^	get() {
				// Here we can only wrap a valid non null profile!
				ImageUtilityLib::ColorProfile*	nativeProfile = m_nativeObject->m_colorProfile;
				return nativeProfile != nullptr ? gcnew ImageUtility::ColorProfile( *nativeProfile ) : nullptr;
			}
		}

		// True if the gamma exponent was found in the file
		property bool		GammaSpecifiedInFile {
			bool	get() { return m_nativeObject->m_gammaSpecifiedInFile; }
		}
		// Gamma exponent or 2.2 if not found in the file
		property float		GammaExponent {
			float	get() { return m_nativeObject->m_gammaExponent; }
		}

		// True if the following information was found in the file (sometimes not available from older file formats like GIF or BMP)
		property bool		IsValid {
			bool	get() { return m_nativeObject->m_valid; }
		}
		// ISO speed (min = 50)
		property UInt32		ISOSpeed {
			UInt32	get() { return m_nativeObject->m_ISOSpeed; }
		}
		// Exposure time (in seconds)
		property float		ExposureTime {
			float	get() { return m_nativeObject->m_exposureTime; }
		}
		// Shutter Speed Value, in EV (Tv = log2( 1/ShutterSpeed))
		property float		Tv {
			float	get() { return m_nativeObject->m_Tv; }
		}
		// Aperture Value, in EV (Av = log2( Aperture² ))
		property float		Av {
			float	get() { return m_nativeObject->m_Av; }
		}
		// In F-stops
		property float		FNumber {
			float	get() { return m_nativeObject->m_FNumber; }
		}
		// In mm
		property float		FocalLength {
			float	get() { return m_nativeObject->m_focalLength; }
		}


	internal:	// METHODS
		MetaData() : m_nativeObject( nullptr ) {
		}

		MetaData( ImageFile^ _owner );
	};
}
