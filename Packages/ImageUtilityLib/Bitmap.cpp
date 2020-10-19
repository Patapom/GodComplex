#include "stdafx.h"
#include "Bitmap.h"

using namespace ImageUtilityLib;
using namespace BaseLib;

ImageFile*	Bitmap::ms_DEBUG = new ImageFile( 4, 4, PIXEL_FORMAT::R8, ColorProfile(ColorProfile::STANDARD_PROFILE::sRGB) );

void	Bitmap::Init( U32 _width, U32 _height ) {
	Exit();

	m_width = _width;
	m_height = _height;
	m_XYZ = new bfloat4[m_width * m_height];
	memset( m_XYZ, 0, m_width*m_height*sizeof(bfloat4) );
}

void	Bitmap::Exit() {
	SAFE_DELETE_ARRAY( m_XYZ );
}

// This is the core of the bitmap class
// This method converts any image file into a float4 CIE XYZ format using the provided profile or the profile associated to the file
void	Bitmap::FromImageFile( const ImageFile& _sourceFile, const ColorProfile* _profileOverride, bool _unPremultiplyAlpha ) {
	const ColorProfile*	colorProfile = _profileOverride != nullptr ? _profileOverride : &_sourceFile.GetColorProfile();
 	if ( colorProfile == nullptr )
 		throw "The provided file doesn't contain a valid color profile and you did not provide any profile override to initialize the bitmap!";

	Exit();

	// Convert for float4 format
	FIBITMAP*	float4Bitmap = FreeImage_ConvertToType( _sourceFile.m_bitmap, FIT_RGBAF );

	m_width = FreeImage_GetWidth( float4Bitmap );
	m_height = FreeImage_GetHeight( float4Bitmap );

	// Convert to XYZ in bulk using profile
	const bfloat4*	source = (const bfloat4*) FreeImage_GetBits( float4Bitmap );
	m_XYZ = new bfloat4[m_width * m_height];
	colorProfile->RGB2XYZ( source, m_XYZ, U32(m_width * m_height) );

	FreeImage_Unload( float4Bitmap );

	if ( _unPremultiplyAlpha ) {
		// Un-pre-multiply by alpha
		bfloat4*	unPreMultipliedTarget = m_XYZ;
		for ( U32 i=m_width*m_height; i > 0; i--, unPreMultipliedTarget++ ) {
			if ( unPreMultipliedTarget->w > 0.0f ) {
				float	invAlpha = 1.0f / unPreMultipliedTarget->w;
				unPreMultipliedTarget->x *= invAlpha;
				unPreMultipliedTarget->y *= invAlpha;
				unPreMultipliedTarget->z *= invAlpha;
			}
		}
	}
}

// And this method converts back the bitmap to RGBA32F format
void	Bitmap::ToImageFile( ImageFile& _targetFile, const ColorProfile& _colorProfile, bool _premultiplyAlpha ) const {
	// Convert back to float4 RGBA using color profile
	_targetFile.Init( m_width, m_height, PIXEL_FORMAT::RGBA32F, _colorProfile );
	const bfloat4*	source = m_XYZ;
	bfloat4*		target = (bfloat4*) _targetFile.GetBits();
	if ( _premultiplyAlpha ) {
		// Pre-multiply by alpha
		const bfloat4*	unPreMultipliedSource = m_XYZ;
		bfloat4*		preMultipliedTarget = target;
		for ( U32 i=m_width*m_height; i > 0; i--, unPreMultipliedSource++, preMultipliedTarget++ ) {
			preMultipliedTarget->x = unPreMultipliedSource->x * unPreMultipliedSource->w;
			preMultipliedTarget->y = unPreMultipliedSource->y * unPreMultipliedSource->w;
			preMultipliedTarget->z = unPreMultipliedSource->z * unPreMultipliedSource->w;
			preMultipliedTarget->w = unPreMultipliedSource->w;
		}
		source = target;	// In-place conversion
	}
	_colorProfile.XYZ2RGB( source, target, m_width*m_height );
}

void	Bitmap::BilinearSample( float X, float Y, bfloat4& _XYZ ) const {
	int		X0 = (int) floorf( X );
	int		Y0 = (int) floorf( Y );
	float	x = X - X0;
	float	y = Y - Y0;
	float	rx = 1.0f - x;
	float	ry = 1.0f - y;
			X0 = CLAMP( X0, 0, S32(m_width-1) );
			Y0 = CLAMP( Y0, 0, S32(m_height-1) );
	int		X1 = MIN( X0+1, S32(m_width-1) );
	int		Y1 = MIN( Y0+1, S32(m_height-1) );

	const bfloat4&	V00 = Access( X0, Y0 );
	const bfloat4&	V01 = Access( X1, Y0 );
	const bfloat4&	V10 = Access( X0, Y1 );
	const bfloat4&	V11 = Access( X1, Y1 );

	bfloat4	V0 = rx * V00 + x * V01;
	bfloat4	V1 = rx * V10 + x * V11;

	_XYZ = ry * V0 + y * V1;
}


//////////////////////////////////////////////////////////////////////////
// LDR -> HDR Conversion
//
// Computes weight using a hat function:
//	_Z must be in [0,_responseCurveSize[ range
float	ComputeWeight( U32 _Z, U32 _responseCurveSize ) {
	U32	Zmid = _responseCurveSize >> 1;
	U32	weight = _Z <= Zmid	? _Z							// Z�[0,Zmid] => Z
							: _responseCurveSize-1 - _Z;	// Z�]Zmid,Zmax] => Zmax - Z
	return float( 1 + weight );								// Add 1 so the weight is never 0!
}

void	Bitmap::LDR2HDR( U32 _imagesCount, const ImageFile** _images, const float* _imageShutterSpeeds, const HDRParms& _parms ) {
	// 1] Compute HDR response
	List< bfloat3 >	responseCurve;
	ComputeCameraResponseCurve( _imagesCount, _images, _imageShutterSpeeds, _parms._inputBitsPerComponent, _parms._curveSmoothnessConstraint, _parms._quality, _parms._luminanceOnly, responseCurve );

	// 2] Filter response
	List< bfloat3 >	responseCurve_filtered;
	FilterCameraResponseCurve( responseCurve, responseCurve_filtered, _parms._luminanceOnly ? 1 : 3, _parms._responseCurveFilterType );

	// 2] Use the response curve to convert our LDR images into an HDR image
	LDR2HDR( _imagesCount, _images, _imageShutterSpeeds, responseCurve_filtered, _parms._luminanceOnly, _parms._luminanceFactor );
}

void	Bitmap::LDR2HDR( U32 _imagesCount, const ImageFile** _images, const float* _imageShutterSpeeds, const List< bfloat3 >& _responseCurve, bool _luminanceOnly, float _luminanceFactor ) {
	if ( _images == nullptr )
		throw "Invalid images array!";
	if ( _imageShutterSpeeds == nullptr )
		throw "Invalid shutter speeds array!";

	U32		W = _images[0]->Width();
	U32		H = _images[0]->Height();
	Init( W, H );

	U32		responseCurveSize = U32(_responseCurve.Count());

	ColorProfile	linearProfile( ColorProfile::STANDARD_PROFILE::LINEAR );


	//////////////////////////////////////////////////////////////////////////
	// 1] Recompose HDR image into the XYZ buffer (still RGB but it will be converted into XYZ at the end)
	bfloat3*	sumWeights = new bfloat3[W*H];
	memset( sumWeights, 0, W*H*sizeof(bfloat3) );
	bfloat4*	scanline = new bfloat4[W];

	U32			Zr, Zg, Zb;
	bfloat4		weight, response;
	bfloat4*	targetHDR = nullptr;
	bfloat3*	targetWeights = nullptr;

	bfloat4		colorLDR_RGB_Linear;
	bfloat4		colorLDR_XYZ;
	bfloat4		colorLDR_xyY;

#if 1
	for ( U32 imageIndex=0; imageIndex < _imagesCount; imageIndex++ ) {
		const ImageFile&	image = *_images[imageIndex];
		const ColorProfile&	imageProfile = image.GetColorProfile();
		float				shutterSpeed = _imageShutterSpeeds[imageIndex];
		float				imageEV = log2f( shutterSpeed );

		targetHDR = m_XYZ;
		targetWeights = sumWeights;
		for ( U32 Y=0; Y < H; Y++ ) {
			image.ReadScanline( Y, scanline );
			bfloat4*	scanlinePtr = scanline;
			for ( U32 X=0; X < W; X++, scanlinePtr++, targetHDR++, targetWeights++ ) {
				imageProfile.RGB2XYZ( *scanlinePtr, colorLDR_XYZ );
				linearProfile.XYZ2RGB( colorLDR_XYZ, colorLDR_RGB_Linear );

// 				if ( colorLDR_RGB_Linear.x < -1e-3f || colorLDR_RGB_Linear.x > 1.001f ) throw "PROUT!";
// 				if ( colorLDR_RGB_Linear.y < -1e-3f || colorLDR_RGB_Linear.y > 1.001f ) throw "PROUT!";
// 				if ( colorLDR_RGB_Linear.z < -1e-3f || colorLDR_RGB_Linear.z > 1.001f ) throw "PROUT!";

				// Retrieve LDR values for RGB
// 				Zr = CLAMP( U32( (responseCurveSize-1) * scanlinePtr->x ), 0U, responseCurveSize-1 );
// 				Zg = CLAMP( U32( (responseCurveSize-1) * scanlinePtr->y ), 0U, responseCurveSize-1 );
// 				Zb = CLAMP( U32( (responseCurveSize-1) * scanlinePtr->z ), 0U, responseCurveSize-1 );
				Zr = CLAMP( U32( (responseCurveSize-1) * colorLDR_RGB_Linear.x ), 0U, responseCurveSize-1 );
				Zg = CLAMP( U32( (responseCurveSize-1) * colorLDR_RGB_Linear.y ), 0U, responseCurveSize-1 );
				Zb = CLAMP( U32( (responseCurveSize-1) * colorLDR_RGB_Linear.z ), 0U, responseCurveSize-1 );

				// Compute weights
				weight.x = ComputeWeight( Zr, responseCurveSize );
				weight.y = ComputeWeight( Zg, responseCurveSize );
				weight.z = ComputeWeight( Zb, responseCurveSize );

				// Accumulate weighted response
				response.x = _responseCurve[Zr].x - imageEV;
				response.y = _responseCurve[Zg].y - imageEV;
				response.z = _responseCurve[Zb].z - imageEV;

				targetHDR->x += weight.x * response.x;
				targetHDR->y += weight.y * response.y;
				targetHDR->z += weight.z * response.z;

				// Accumulate weight
				*targetWeights += weight;
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// 2] Divide by weights and retrieve linear radiance
	targetHDR = m_XYZ;
	targetWeights = sumWeights;
	for ( U32 Y=0; Y < H; Y++ ) {
		for ( U32 X=0; X < W; X++, targetHDR++, targetWeights++ ) {
			bfloat4&	temp = *targetHDR;

			// Retrieve log2(E)
			temp.x *= _luminanceFactor / targetWeights->x;
			temp.y *= _luminanceFactor / targetWeights->y;
			temp.z *= _luminanceFactor / targetWeights->z;

			// Retrieve linear radiance
			temp.x = powf( 2.0f, temp.x );
			temp.y = powf( 2.0f, temp.y );
			temp.z = powf( 2.0f, temp.z );

			temp.w = 1.0f;	// Force alpha to 1
		}
	}

	delete[] scanline;
	delete[] sumWeights;

	//////////////////////////////////////////////////////////////////////////
	// 3] Convert into XYZ using a linear profile
	linearProfile.RGB2XYZ( m_XYZ, m_XYZ, W*H );

#else
	for ( U32 imageIndex=0; imageIndex < _imagesCount; imageIndex++ ) {
		const ImageFile&	image = *_images[imageIndex];
		const ColorProfile&	imageProfile = image.GetColorProfile();
		float				shutterSpeed = _imageShutterSpeeds[imageIndex];
		float				imageEV = log2f( shutterSpeed );

		targetHDR = m_XYZ;
		targetWeights = sumWeights;

		if ( _luminanceOnly ) {
			// When dealing with luminance only response curve, we work in xyY
			// The response curve and log2() shifting is only applied to the Y component while the xy chromaticities are simply weighted
			for ( U32 Y=0; Y < H; Y++ ) {
				image.ReadScanline( Y, scanline );
				bfloat4*	scanlinePtr = scanline;
				for ( U32 X=0; X < W; X++, scanlinePtr++, targetHDR++, targetWeights++ ) {
					// Transform into xyY
					imageProfile.RGB2XYZ( *scanlinePtr, colorLDR_XYZ );
					if ( colorLDR_XYZ.y == 0.0f )
						continue;	// Don't account for black pixels since we don't have valid chromas for these!

					ColorProfile::XYZ2xyY( (bfloat3&) colorLDR_XYZ, (bfloat3&) colorLDR_xyY );

					// Retrieve LDR values for luminance
					Zb = CLAMP( U32( (responseCurveSize-1) * colorLDR_xyY.z ), 0U, responseCurveSize-1 );

					// Compute weights
					weight.x = weight.y = weight.z = ComputeWeight( Zb, responseCurveSize );

					// Accumulate weighted response
					response.x = colorLDR_xyY.x;
					response.y = colorLDR_xyY.y;
					response.z = _responseCurve[Zb].x - imageEV;

					targetHDR->x += weight.x * response.x;
					targetHDR->y += weight.y * response.y;
					targetHDR->z += weight.z * response.z;

					// Accumulate weight
					*targetWeights += weight;
				}
			}
		} else {
			// When dealing with a RGB response curve then we first convert into linear RGB (i.e. without gamma)
			// We weight each component independently in RGB space
			for ( U32 Y=0; Y < H; Y++ ) {
				image.ReadScanline( Y, scanline );
				bfloat4*	scanlinePtr = scanline;
				for ( U32 X=0; X < W; X++, scanlinePtr++, targetHDR++, targetWeights++ ) {
					// Transform into linear RGB
					imageProfile.RGB2XYZ( *scanlinePtr, colorLDR_XYZ );
					linearProfile.XYZ2RGB( colorLDR_XYZ, colorLDR_RGB_Linear );

					// Retrieve LDR values for RGB
					Zr = CLAMP( U32( (responseCurveSize-1) * colorLDR_RGB_Linear.x ), 0U, responseCurveSize-1 );
					Zg = CLAMP( U32( (responseCurveSize-1) * colorLDR_RGB_Linear.y ), 0U, responseCurveSize-1 );
					Zb = CLAMP( U32( (responseCurveSize-1) * colorLDR_RGB_Linear.z ), 0U, responseCurveSize-1 );

					// Compute weights
					weight.x = ComputeWeight( Zr, responseCurveSize );
					weight.y = ComputeWeight( Zg, responseCurveSize );
					weight.z = ComputeWeight( Zb, responseCurveSize );

					// Accumulate weighted response
					response.x = _responseCurve[Zr].x - imageEV;
					response.y = _responseCurve[Zg].y - imageEV;
					response.z = _responseCurve[Zb].z - imageEV;

					targetHDR->x += weight.x * response.x;
					targetHDR->y += weight.y * response.y;
					targetHDR->z += weight.z * response.z;

					// Accumulate weight
					*targetWeights += weight;
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// 2] Divide by weights and retrieve linear radiance
	if ( _luminanceOnly ) {
		// When dealing with a luminance only response curve then our target HDR is expressed as a weighted xyY value where Y is actually the log2() of the luminance
		// We simply need to renormalized the weighted xyY sum and retrieve the linear luminance, finally we convert the xyY value back into XYZ
		targetHDR = m_XYZ;
		targetWeights = sumWeights;
		for ( U32 Y=0; Y < H; Y++ ) {
			for ( U32 X=0; X < W; X++, targetHDR++, targetWeights++ ) {
				bfloat4&	temp = *targetHDR;
				if ( targetWeights->z == 0.0f )
					continue;	// All pixels were black?

				// Retrieve log2(E)
				temp.x /= targetWeights->x;
				temp.y /= targetWeights->y;
				temp.z /= targetWeights->z;

				// Retrieve linear radiance
				temp.z = _luminanceFactor * powf( 2.0f, temp.z );
				temp.w = 1.0f;	// Force alpha to 1

				// Convert back into XYZ
				ColorProfile::xyY2XYZ( (bfloat3&) temp, (bfloat3&) temp );
			}
		}
	} else {
		// When dealing with a RGB response curve then each RGB channel is dealt with separately
		// We start with a weighted log2() value for RGB so we simply renormalize it first then retrieve a linear RGB that we finally convert back into XYZ
		targetHDR = m_XYZ;
		targetWeights = sumWeights;
		for ( U32 Y=0; Y < H; Y++ ) {
			for ( U32 X=0; X < W; X++, targetHDR++, targetWeights++ ) {
				bfloat4&	temp = *targetHDR;

				// Retrieve log2(E)
				temp.x /= targetWeights->x;
				temp.y /= targetWeights->y;
				temp.z /= targetWeights->z;

				// Retrieve linear radiance
				temp.x = _luminanceFactor * powf( 2.0f, temp.x );
				temp.y = _luminanceFactor * powf( 2.0f, temp.y );
				temp.z = _luminanceFactor * powf( 2.0f, temp.z );
				temp.w = 1.0f;	// Force alpha to 1

				// Convert back into XYZ
				linearProfile.RGB2XYZ( temp, temp );
			}
		}
	}

	delete[] scanline;
	delete[] sumWeights;
#endif
}

void svdcmp( int m, int n, float** a, float w[], float** v );
void svdcmp_ORIGINAL( int m, int n, float** a, float w[], float** v );

//#define DEBUG_LINEAR_SIGNAL	// Define this to inject a linear sensor response for debugging purpose

void	Bitmap::ComputeCameraResponseCurve( U32 _imagesCount, const ImageFile** _images, const float* _imageShutterSpeeds, U32 _inputBitsPerComponent, float _curveSmoothnessConstraint, float _quality, bool _luminanceOnly, List< bfloat3 >& _responseCurve ) {
	if ( _images == nullptr )
		throw "Invalid images array!";
	if ( _imageShutterSpeeds == nullptr )
		throw "Invalid shutter speeds array!";

	U32		W = _images[0]->Width();
	U32		H = _images[0]->Height();

	ColorProfile	linearProfile( ColorProfile::STANDARD_PROFILE::LINEAR );

	//////////////////////////////////////////////////////////////////////////
	// 1] Find the best possible samples across the provided images
	// According to Debevec in �2.1:
	//	<< Finally, we need not use every available pixel site in this solution procedure.
	//		Given measurements of N pixels in P photographs, we have to solve for N values of ln(Ei) and (Zmax - Zmin) samples of g.
	//		To ensure a sufficiently overdetermined system, we want N*(P-1) > (Zmax-Zmin).
	//		For the pixel value range (Zmax - Zmin) = 255, P = 11 photographs, a choice of N on the order of 50 pixels is more than adequate.
	//		Since the size of the system of linear equations arising from Equation 3 is on the order of N * P + (Zmax - Zmin), computational
	//		complexity considerations make it impractical to use every pixel location in this algorithm. >>
	//
	// Here, Zmin and Zmax are the minimum and maximum pixel values in the images (e.g. for an 8-bits input image Zmin=0 and Zmax=255)
	//	and g is the log of the inverse of the transfer function the camera applies to the pixels to transform input irradiance into numerical values
	//
	U32		responseCurveSize = 1U << _inputBitsPerComponent;
	float	nominalPixelsCount = float(responseCurveSize) / _imagesCount;	// Use about that amount of pixels across images to have a nice over-determined system
			nominalPixelsCount *= 1.0f + _quality;							// Apply the user's quality settings to use more or less pixels

	int		pixelsCountPerImage = int( ceilf( nominalPixelsCount ) );		// And that is our amount of pixels to use per image

	int		totalPixelsCount = _imagesCount * pixelsCountPerImage;

	// Prepare the response curve array
	_responseCurve.SetCount( responseCurveSize );

	// Now, we need to carefully select the candidate pixels.
	// Still quoting Debevec in �2.1:
	//	<< Clearly, the pixel locations should be chosen so that they have a reasonably even distribution of pixel values from Zmin to Zmax,
	//		and so that they are spatially well distributed in the image.
	//	   Furthermore, the pixels are best sampled from regions of the image with low intensity variance so that radiance can be assumed to
	//		be constant across the area of the pixel, and the effect of optical blur of the imaging system is minimized. >>
	//

// Use a pseudo-random sequence of positions at the moment... :/
List< bfloat2 >	sequence;
Hammersley::BuildSequence( pixelsCountPerImage, sequence );

	const bfloat4	LUMINANCE_D65( 0.2126f, 0.7152f, 0.0722f, 0.0f );	// Y vector for observer. = 2�, Illuminant = D65

	U32		componentsCount = _luminanceOnly ? 1 : 3;

	U32*	pixels = new U32[3 *totalPixelsCount];
	for ( U32 componentIndex=0; componentIndex < componentsCount; componentIndex++ ) {	// Because R, G, B

		// 1] Select the pixels within the images that best cover the [Zmin,Zmax] range
// @TODO!
// @TODO!
// @TODO!
// @TODO! This is an important algorithm we need to find out: select the given amount of pixels that cover the most range
// @TODO!
// @TODO!
// @TODO!

		// 2] Store as integer pixel values within range [Zmin,Zmax] (which is [0,2^bitDepth[ )
		const bfloat2*	sequencePtr = sequence.Ptr();
		bfloat4	colorLDR_RGB, colorLDR_XYZ, colorLDR_RGB_Linear;

		for ( U32 pixelIndex=0; pixelIndex < sequence.Count(); pixelIndex++, sequencePtr++ ) {
			U32	X = U32( floorf( sequencePtr->x * (W-1) ) );
			U32	Y = U32( floorf( sequencePtr->y * (H-1) ) );

			for ( U32 imageIndex=0; imageIndex < _imagesCount; imageIndex++ ) {
				const ImageFile&	image = *_images[imageIndex];
				ASSERT( image.Width() == W && image.Height() == H, "All input images must have the same resolution!" );

				const ColorProfile&	imageProfile = image.GetColorProfile();

				// Get floating point value of the selected pixel for R, G or B
				#if 1
					// Use an average of neighbor pixels
					S32	X0 = MAX( 0, S32(X) - 8 );
					S32	X1 = MIN( S32(W-1), S32(X) + 8 );
					S32	Y0 = MAX( 0, S32(Y) - 8 );
					S32	Y1 = MIN( S32(H-1), S32(Y) + 8 );
					bfloat4	sum_XYZ( 0, 0, 0, 0 );
					for ( S32 CY=Y0; CY <= Y1; CY++ )
						for ( S32 CX=X0; CX <= X1; CX++ ) {
							image.Get( CX, CY, colorLDR_RGB );
							imageProfile.RGB2XYZ( colorLDR_RGB, colorLDR_XYZ );	// Transform into linear XYZ
							sum_XYZ += colorLDR_XYZ;
//sum_XYZ += colorLDR;
// colorLDR.x = powf( colorLDR.x, 2.2f );
// colorLDR.y = powf( colorLDR.y, 2.2f );
// colorLDR.z = powf( colorLDR.z, 2.2f );
// sum_XYZ.x += colorLDR.Dot( LUMINANCE_D65 );
						}
					colorLDR_XYZ = sum_XYZ / float( (1+X1-X0)*(1+Y1-Y0) );
				#else
					image.Get( X, Y, colorLDR_RGB );

					// Transform into linear XYZ
					imageProfile.RGB2XYZ( colorLDR_RGB, colorLDR_XYZ );
				#endif

				float	pixelValue;
				if ( _luminanceOnly ) {
					// Use luminance directly
					pixelValue = colorLDR_XYZ.y;
//pixelValue = colorLDR_XYZ.Dot( LUMINANCE_D65 );
//pixelValue = colorLDR_XYZ.x;
// colorLDR_XYZ.x = powf( colorLDR_XYZ.x, 2.2f );
// colorLDR_XYZ.y = powf( colorLDR_XYZ.y, 2.2f );
// colorLDR_XYZ.z = powf( colorLDR_XYZ.z, 2.2f );
// pixelValue = colorLDR_XYZ.Dot( LUMINANCE_D65 );

				} else {
					// Transform back into linear RGB
					linearProfile.XYZ2RGB( colorLDR_XYZ, colorLDR_RGB );
					pixelValue = ((float*) &colorLDR_RGB.x)[componentIndex];
				}

#ifdef DEBUG_LINEAR_SIGNAL
pixelValue = SATURATE( float(1+pixelIndex) / sequence.Count() * powf( 2.0f, -float(imageIndex) ) );
#endif

				// Convert to integer value
				U32		Z = CLAMP( U32( (responseCurveSize-1) * pixelValue ), 0U, responseCurveSize-1 );

				pixels[totalPixelsCount*componentIndex + pixelsCountPerImage*imageIndex + pixelIndex] = Z;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// 2] Apply SVD 
	const float	lambda = _curveSmoothnessConstraint;

#if 1
	U32	equationsCount  = totalPixelsCount		// Pixels
						+ responseCurveSize		// Used to enforce the smoothness of the g curve
						+ 1;					// Constraint that g(Zmid) = 0 (with Zmid = (Zmax+Zmin)/2)

	U32	unknownsCount	= responseCurveSize		// g(Z) = curve solution
						+ pixelsCountPerImage;	// log(E) for each pixel

	MathSolversLib::SVD			SVD( equationsCount, unknownsCount );
	MathSolversLib::VectorF		b( equationsCount );
	MathSolversLib::VectorF		x( unknownsCount );

	U32*		pixelPtr = pixels;
	for ( U32 componentIndex=0; componentIndex < componentsCount; componentIndex++ ) {	// Because R, G, B

		// ===================================================================
		// 2.1] Build the "A" matrix and target vector "b"
		SVD.A.Clear();
		b.Clear();

		// 2.1.1) Fill the first part of the equations containing our data
		MathSolversLib::VectorF*	A1 = &SVD.A[0];	// A1 starts at A
		for ( U32 imageIndex=0; imageIndex < _imagesCount; imageIndex++ ) {

			// Compute image EV = log2(shutterSpeed)
			// (e.g. taking 3 shots in bracket mode with shutter speeds [1/4s, 1s, 4s] will yield EV array [-2, 0, +2] respectively)
			//
			float	imageShutterSpeed = _imageShutterSpeeds[imageIndex];
			float	imageEV = log2f( imageShutterSpeed );

#ifdef DEBUG_LINEAR_SIGNAL
imageEV = -float(imageIndex);
#endif

			for ( int pixelIndex=0; pixelIndex < pixelsCountPerImage; pixelIndex++, A1++ ) {

				// Zij = pixel value for selected pixel i in image j
				U32		Z = *pixelPtr++;

				// Weight based on Z position within the range [Zmin,Zmax]
				float	Wij = ComputeWeight( Z, responseCurveSize );

				// First weight g(Zi)
				(*A1)[Z] = Wij;

				// Next, weight ln(Ei)
				(*A1)[responseCurveSize + pixelIndex] = -Wij;

				// And subtract weighted EV = log2(DeltaT)
				b[pixelsCountPerImage*imageIndex + pixelIndex] = Wij * imageEV;
			}
		}

		// 2.1.2) Fill the second part of the equations containing weighted smoothing coefficients
		// This part will ensure the g() curve's smoothness
		MathSolversLib::VectorF*	A2 = &SVD.A[totalPixelsCount];	// A2 starts at the end of A's first part, A1

		A2[0][0] = lambda * ComputeWeight( 0, responseCurveSize );	// First element can't reach neighbors
		U32		Z = 1;
		for ( ; Z < responseCurveSize-1; Z++ ) {
			float	Weight = lambda * ComputeWeight( Z, responseCurveSize );
			A2[Z][Z-1] = Weight;
			A2[Z][Z+0] = -2.0f * Weight;
			A2[Z][Z+1] = Weight;
		}
		A2[Z][Z] = lambda * ComputeWeight( Z, responseCurveSize );	// Last element can't reach neighbors either

		// 2.1.3) Fill the last equation used to ensure the Zmid value transforms into g(Zmid) = 0
		MathSolversLib::VectorF*	A3 = &SVD.A[totalPixelsCount+responseCurveSize];	// A3 starts at the end of A's second part and should actually be the last row of A

		A3[0][responseCurveSize>>1] = 1;	// Make sure g(Zmid) maps to 0

		// ===================================================================
		// 2.2] Apply SVD
		SVD.Decompose();

		// ===================================================================
		// 2.3] Solve for x
		// We know that A was decomposed into U.w.V^T where U and V are orthonormal matrices and w a diagonal matrix with (theoretically) non null components
		//	so A^-1 = V.1/w.U^T
		// Since A.x = b it ensues that x = A^-1.b
		//
		SVD.Solve( b, x );

		// ===================================================================
		// 2.4] Recover curve values
		// At this point, we recovered the g(Z) for Z�[Zmin,Zmax], followed by the log2(Ei) for the N selected pixels
		// Let's just store the g(Z) into our target array
		if ( _luminanceOnly ) {
			for ( U32 Z=0; Z < responseCurveSize; Z++ ) {
				_responseCurve[Z].Set( x[Z], x[Z], x[Z] );
			}
		} else {
			for ( U32 Z=0; Z < responseCurveSize; Z++ ) {
				((float*) &_responseCurve[Z].x)[componentIndex] = x[Z];
			}
		}
	}

#else

#if 0
	U32	equationsCount  = 3;
	U32	unknownsCount	= 3;

	float*	Aterms = new float[equationsCount*unknownsCount];	// M*N matrix with M rows = #equations and N columns = #unknowns
	float**	A = new float*[equationsCount];
	for ( U32 rowIndex=0; rowIndex < equationsCount; rowIndex++ ) A[rowIndex] = &Aterms[unknownsCount*rowIndex];	// Initialize each row pointer

	float*	Vterms = new float[unknownsCount*unknownsCount];	// N*N matrix
	float**	V = new float*[unknownsCount];
	for ( U32 rowIndex=0; rowIndex < unknownsCount; rowIndex++ ) V[rowIndex] = &Vterms[unknownsCount*rowIndex];		// Initialize each row pointer

	float*	w = new float[unknownsCount];

	float*	b = new float[equationsCount];

	float*	x = new float[unknownsCount];	// Our result vector
	float*	tempX = new float[unknownsCount];

A[0][0] = 1;
A[0][1] = 2;
A[0][2] = 3;
A[1][0] = 0;
A[1][1] = 1;
A[1][2] = 4;
A[2][0] = 5;
A[2][1] = 6;
A[2][2] = 0;

	U32*	pixelPtr = pixels;
	for ( U32 componentIndex=0; componentIndex < componentsCount; componentIndex++ ) {	// Because R, G, B


#else
	U32	equationsCount  = totalPixelsCount		// Pixels
						+ responseCurveSize		// Used to enforce the smoothness of the g curve
						+ 1;					// Constraint that g(Zmid) = 0 (with Zmid = (Zmax+Zmin)/2)
	U32	unknownsCount	= responseCurveSize		// g(Z) = curve solution
						+ pixelsCountPerImage;	// log(E) for each pixel

	float*	Aterms = new float[equationsCount*unknownsCount];	// M*N matrix with M rows = #equations and N columns = #unknowns
	float**	A = new float*[equationsCount];
	for ( U32 rowIndex=0; rowIndex < equationsCount; rowIndex++ ) A[rowIndex] = &Aterms[unknownsCount*rowIndex];	// Initialize each row pointer

	float*	Vterms = new float[unknownsCount*unknownsCount];	// N*N matrix
	float**	V = new float*[unknownsCount];
	for ( U32 rowIndex=0; rowIndex < unknownsCount; rowIndex++ ) V[rowIndex] = &Vterms[unknownsCount*rowIndex];		// Initialize each row pointer
// 
// 	float*	w = new float[unknownsCount];
// 
 	float*	b = new float[equationsCount];
// 
// 	float*	x = new float[unknownsCount];	// Our result vector
// 	float*	tempX = new float[unknownsCount];

	MathSolversLib::MatrixF	A_( equationsCount, unknownsCount );
	MathSolversLib::MatrixF	V_( unknownsCount, unknownsCount );
	MathSolversLib::VectorF	w( unknownsCount );
	MathSolversLib::VectorF	b_( equationsCount );
	MathSolversLib::VectorF	tempX( unknownsCount );
	MathSolversLib::VectorF	x( unknownsCount );


	float**	Arows = new float*[equationsCount];
	for ( U32 rowIndex=0; rowIndex < equationsCount; rowIndex++ ) Arows[rowIndex] = A_[rowIndex].m;	// Initialize each row pointer
	float**	Vrows = new float*[unknownsCount];
	for ( U32 rowIndex=0; rowIndex < unknownsCount; rowIndex++ ) Vrows[rowIndex] = V_[rowIndex].m;	// Initialize each row pointer



	U32*	pixelPtr = pixels;
	for ( U32 componentIndex=0; componentIndex < componentsCount; componentIndex++ ) {	// Because R, G, B

		// ===================================================================
		// 2.1] Build the "A" matrix and target vector "b"
		memset( Aterms, 0, equationsCount*unknownsCount*sizeof(float) );
		memset( b, 0, equationsCount*sizeof(float) );
		A_.Clear();
		b_.Clear();

		// 2.1.1) Fill the first part of the equations containing our data
		float**		A1 = &A[0];	// A1 starts at A
		MathSolversLib::VectorF*	A1_ = &A_[0];	// A1 starts at A
		for ( U32 imageIndex=0; imageIndex < _imagesCount; imageIndex++ ) {

			// Compute image EV = log2(shutterSpeed)
			// (e.g. taking 3 shots in bracket mode with shutter speeds [1/4s, 1s, 4s] will yield EV array [-2, 0, +2] respectively)
			//
			float	imageShutterSpeed = _imageShutterSpeeds[imageIndex];
			float	imageEV = log2f( imageShutterSpeed );

#ifdef DEBUG_LINEAR_SIGNAL
imageEV = -float(imageIndex);
#endif

			for ( int pixelIndex=0; pixelIndex < pixelsCountPerImage; pixelIndex++, A1_++ ) {
				float*	columns = *A1++;

				// Zij = pixel value for selected pixel i in image j
				U32		Z = *pixelPtr++;

				// Weight based on Z position within the range [Zmin,Zmax]
				float	Wij = ComputeWeight( Z, responseCurveSize );

				// First weight g(Zi)
				columns[Z] = Wij;
				(*A1_)[Z] = Wij;

				// Next, weight ln(Ei)
 				columns[responseCurveSize + pixelIndex] = -Wij;
				(*A1_)[responseCurveSize + pixelIndex] = -Wij;

				// And subtract weighted EV = log2(DeltaT)
				b[pixelsCountPerImage*imageIndex + pixelIndex] = Wij * imageEV;
				b_[pixelsCountPerImage*imageIndex + pixelIndex] = Wij * imageEV;
			}
		}

		// 2.1.2) Fill the second part of the equations containing weighted smoothing coefficients
		// This part will ensure the g() curve's smoothness
		float**	A2 = &A[totalPixelsCount];	// A2 starts at the end of A's first part, A1
		MathSolversLib::VectorF*	A2_ = &A_[totalPixelsCount];	// A2 starts at the end of A's first part, A1

		A2[0][0] = lambda * ComputeWeight( 0, responseCurveSize );	// First element can't reach neighbors
		A2_[0][0] = lambda * ComputeWeight( 0, responseCurveSize );	// First element can't reach neighbors
		U32		Z = 1;
		for ( ; Z < responseCurveSize-1; Z++ ) {
			float	Weight = lambda * ComputeWeight( Z, responseCurveSize );
			A2[Z][Z-1] = Weight;
			A2[Z][Z+0] = -2.0f * Weight;
			A2[Z][Z+1] = Weight;
			A2_[Z][Z-1] = Weight;
			A2_[Z][Z+0] = -2.0f * Weight;
			A2_[Z][Z+1] = Weight;
		}
		A2[Z][Z] = lambda * ComputeWeight( Z, responseCurveSize );	// Last element can't reach neighbors either
		A2_[Z][Z] = lambda * ComputeWeight( Z, responseCurveSize );	// Last element can't reach neighbors either

		// 2.1.3) Fill the last equation used to ensure the Zmid value transforms into g(Zmid) = 0
		float**	A3 = &A[totalPixelsCount+responseCurveSize];	// A3 starts at the end of A's second part and should actually be the last row of A
		MathSolversLib::VectorF*	A3_ = &A_[totalPixelsCount+responseCurveSize];	// A3 starts at the end of A's second part and should actually be the last row of A

		A3[0][responseCurveSize>>1] = 1;	// Make sure g(Zmid) maps to 0
		A3_[0][responseCurveSize>>1] = 1;	// Make sure g(Zmid) maps to 0

		ASSERT( memcmp( Aterms, A_.m_raw, equationsCount*unknownsCount*sizeof(float) ) == 0, "RHA!" );
		ASSERT( memcmp( b, b_.m, equationsCount*sizeof(float) ) == 0, "RHA!" );
#endif

		// ===================================================================
		// 2.2] Apply SVD
#if 0
float*	Abackup = new float[equationsCount*unknownsCount];
memcpy_s( Abackup, equationsCount*unknownsCount*sizeof(float), Aterms, equationsCount*unknownsCount*sizeof(float) );

// Fill up the debug bitmap with the matrix's coefficients
ms_DEBUG->Init( unknownsCount, equationsCount, ImageFile::PIXEL_FORMAT::R16, ColorProfile( ColorProfile::STANDARD_PROFILE::sRGB ) );
for ( U32 i=0; i < equationsCount; i++ ) {
	for ( U32 j=0; j < unknownsCount; j++ ) {
		float	value = 0.0078125f * A[i][j];
		ms_DEBUG->Set( j, i, bfloat4( value, value, value, 1.0f ) );
	}
}

#endif

		svdcmp( equationsCount, unknownsCount, Arows, w.m, Vrows );

#if 0	// Check against the original routine
float*	resultA = new float[equationsCount*unknownsCount];
float*	resultW = new float[unknownsCount];
float*	resultV = new float[unknownsCount*unknownsCount];
memcpy( resultA, Aterms, equationsCount*unknownsCount*sizeof(float) );
memcpy( resultW, w, unknownsCount*sizeof(float) );
memcpy( resultV, Vterms, unknownsCount*unknownsCount*sizeof(float) );


// Copy back and call again using original routine this time
memcpy_s( Aterms, equationsCount*unknownsCount*sizeof(float), Abackup, equationsCount*unknownsCount*sizeof(float) );
float**	A_index1 = new float*[1+equationsCount];
for ( int rowIndex=0; rowIndex < equationsCount; rowIndex++ ) A_index1[1+rowIndex] = &Aterms[unknownsCount*rowIndex] - 1;	// Initialize each row pointer
float**	V_index1 = new float*[1+unknownsCount];
for ( int rowIndex=0; rowIndex < unknownsCount; rowIndex++ ) V_index1[1+rowIndex] = &Vterms[unknownsCount*rowIndex] - 1;		// Initialize each row pointer

svdcmp_ORIGINAL( equationsCount, unknownsCount, A_index1, w-1, V_index1 );

float	sumDiffA = 0.0f;
float	sumDiffV = 0.0f;
float	sumDiffW = 0.0f;
for ( U32 j=0; j < unknownsCount; j++ ) {
	for ( U32 i=0; i < equationsCount; i++ ) {
		if ( i < unknownsCount )
			sumDiffV += fabs(resultV[unknownsCount*i+j] - V[i][j] );
		sumDiffA += fabs(resultA[unknownsCount*i+j] - A[i][j] );
	}
	sumDiffW += fabs( resultW[j] - w[j] );
}
#endif

#if 0
// Check U and V are orthonormal
float	minDot = FLT_MAX;
float	maxDot = -FLT_MAX;
float	avgDot = 0.0f;
float	sameColumn_minDot = FLT_MAX;
float	sameColumn_maxDot = -FLT_MAX;
float	sameColumn_avgDot = 0.0f;
// Check U
for ( U32 col0=0; col0 < unknownsCount-1; col0++ ) {
	for ( U32 col1=col0+1; col1 < unknownsCount; col1++ ) {
		float	dot = 0.0f;
		for ( U32 i=0; i < equationsCount; i++ ) {
			const float	c0 = A[i][col0];
			const float	c1 = A[i][col1];
			dot += c0*c1;
		}
		minDot = MIN( minDot, dot );
		maxDot = MAX( maxDot, dot );
		avgDot += dot;
	}

	// Check column norm
	float	dot = 0.0f;
	for ( U32 i=0; i < equationsCount; i++ ) {
		const float	c0 = A[i][col0];
		dot += c0*c0;
	}
	sameColumn_minDot = MIN( sameColumn_minDot, dot );
	sameColumn_maxDot = MAX( sameColumn_maxDot, dot );
	sameColumn_avgDot += dot;
}
avgDot /= unknownsCount-1;
sameColumn_avgDot /= unknownsCount;

// Check V
minDot = FLT_MAX;
maxDot = -FLT_MAX;
avgDot = 0.0f;
sameColumn_minDot = FLT_MAX;
sameColumn_maxDot = -FLT_MAX;
sameColumn_avgDot = 0.0f;
for ( U32 col0=0; col0 < unknownsCount-1; col0++ ) {
	for ( U32 col1=col0+1; col1 < unknownsCount; col1++ ) {
		float	dot = 0.0f;
		for ( U32 i=0; i < unknownsCount; i++ ) {
			const float	c0 = V[i][col0];
			const float	c1 = V[i][col1];
			dot += c0*c1;
		}
		minDot = MIN( minDot, dot );
		maxDot = MAX( maxDot, dot );
		avgDot += dot;
	}

	// Check column norm
	float	dot = 0.0f;
	for ( U32 i=0; i < unknownsCount; i++ ) {
		const float	c0 = V[i][col0];
		dot += c0*c0;
	}
	sameColumn_minDot = MIN( sameColumn_minDot, dot );
	sameColumn_maxDot = MAX( sameColumn_maxDot, dot );
	sameColumn_avgDot += dot;
}
avgDot /= unknownsCount-1;
sameColumn_avgDot /= unknownsCount;
#endif

#if 0	// Debug visually by transforming matrices into a bitmap
// Fill up the debug bitmap with the matrix's coefficients
ms_DEBUG->Init( unknownsCount, equationsCount, ImageFile::PIXEL_FORMAT::R16, ColorProfile( ColorProfile::STANDARD_PROFILE::sRGB ) );
for ( U32 i=0; i < equationsCount; i++ ) {
	for ( U32 j=0; j < unknownsCount; j++ ) {
		float	value = 1000.0f * 0.0078125f * A[i][j];
		ms_DEBUG->Set( j, i, bfloat4( value, value, value, 1.0f ) );
	}
}
// ms_DEBUG->Init( unknownsCount, unknownsCount, ImageFile::PIXEL_FORMAT::R16, ColorProfile( ColorProfile::STANDARD_PROFILE::sRGB ) );
// for ( U32 i=0; i < unknownsCount; i++ ) {
// 	for ( U32 j=0; j < unknownsCount; j++ ) {
// 		float	value = 1000.0f * 0.0078125f * V[i][j];
// 		ms_DEBUG->Set( j, i, bfloat4( value, value, value, 1.0f ) );
// 	}
// }
// ms_DEBUG->Init( unknownsCount, unknownsCount, ImageFile::PIXEL_FORMAT::R16, ColorProfile( ColorProfile::STANDARD_PROFILE::sRGB ) );
// for ( U32 i=0; i < unknownsCount; i++ ) {
// 	float	value = 0.001f * fabs(w[i]);
// 	for ( U32 j=0; j < unknownsCount; j++ ) {
// 		ms_DEBUG->Set( j, i, bfloat4( value, value, value, 1.0f ) );
// 	}
// }

//throw "PIPO!";
#endif

#if 0
// Ensure the product of A.A^-1 = (U.w.V^T) * (V.1/w.U^T) = I

// Recompose A from U, w and V
float*	Arecomposed = new float[equationsCount*unknownsCount];
for ( U32 rowIndex=0; rowIndex < equationsCount; rowIndex++ ) {
	for ( U32 columnIndex=0; columnIndex < unknownsCount; columnIndex++ ) {
		float	r = 0.0f;
		for ( U32 i=0; i < unknownsCount; i++ ) {
			const float	VTterm = V[columnIndex][i];	// V was returned, we need V^T
			const float	Wterm = w[i];
			const float	Uterm = A[rowIndex][i];
			r += Uterm * Wterm * VTterm;
		}
		Arecomposed[rowIndex*unknownsCount + columnIndex] = r;
	}
}

// Ensure recomposition yields original A
float	sumSqDiff = 0.0f;
for ( int i=equationsCount*unknownsCount-1; i >= 0; i-- ) {
	float	originalTerm = Abackup[i];
	float	recomposedTerm = Arecomposed[i];
	float	diff = originalTerm - recomposedTerm;
	sumSqDiff += diff * diff;
}
sumSqDiff /= equationsCount*unknownsCount;
ASSERT( sumSqDiff < 1e-4f, "Singular Value Decomposition is wrong!" );

// Recompose A^-1
float*	Ainverse = new float[unknownsCount*equationsCount];
for ( U32 rowIndex=0; rowIndex < unknownsCount; rowIndex++ ) {
	for ( U32 columnIndex=0; columnIndex < equationsCount; columnIndex++ ) {
		float	r = 0.0f;
		for ( U32 i=0; i < unknownsCount; i++ ) {
			const float	Vterm = V[rowIndex][i];
			const float	recWterm = w[i] != 0.0f ? 1.0f / w[i] : 0.0f;
			const float	UTterm = A[columnIndex][i];
			r += Vterm * recWterm * UTterm;
		}
		Ainverse[rowIndex*equationsCount + columnIndex] = r;
	}
}

// Apply product of matrices
float*	Aproduct = new float[equationsCount*equationsCount];
for ( U32 rowIndex=0; rowIndex < equationsCount; rowIndex++ ) {
	for ( U32 columnIndex=0; columnIndex < equationsCount; columnIndex++ ) {
		float	r = 0.0f;
		for ( U32 i=0; i < unknownsCount; i++ ) {
			const float	a = Arecomposed[unknownsCount*rowIndex+i];
			const float	aI = Ainverse[equationsCount*i+columnIndex];
			r += a * aI;
		}
		Aproduct[rowIndex*equationsCount + columnIndex] = r;
	}
}

// Ensure product yields identity
float	sumDiagonal = 0.0f;
float	sumOffDiagonal = 0.0f;
for ( U32 rowIndex=0; rowIndex < unknownsCount; rowIndex++ ) {
	for ( U32 columnIndex=0; columnIndex < unknownsCount; columnIndex++ ) {
		float	absTerm = fabs( Aproduct[unknownsCount*rowIndex+columnIndex] );
		if ( rowIndex == columnIndex )
			sumDiagonal += absTerm;
		else
			sumOffDiagonal += absTerm;
	}
}
sumDiagonal /= unknownsCount;
sumOffDiagonal /= unknownsCount * (unknownsCount-1);
ASSERT( sumOffDiagonal < 1e-4f, "Singular Value Decomposition is wrong!" );
ASSERT( fabs( sumDiagonal - 1.0f) < 1e-4f, "Singular Value Decomposition is wrong!" );

delete[] Aproduct;
delete[] Arecomposed;
delete[] Ainverse;
delete[] Abackup;
#endif


		// ===================================================================
		// 2.3] Solve for x
		// We know that A was decomposed into U.w.V^T where U and V are orthonormal matrices and w a diagonal matrix with (theoretically) non null components
		//	so A^-1 = V.1/w.U^T
		// Since A.x = b it ensues that x = A^-1.b
		//
		
		// 2.3.1) Perform 1/w * U^T * b
		for ( U32 rowIndex=0; rowIndex < unknownsCount; rowIndex++ ) {
			float	r = 0.0f;
			for ( U32 i=0; i < equationsCount; i++ ) {
				const float	Uterm = A[i][rowIndex];	// The svdcmp() routine used A to store the U matrix in place
				const float	bterm = b[i];
				r += Uterm * bterm;
			}

			// Multiply by 1/w
			const float	wterm = w[rowIndex];
			float		recW = fabs( wterm ) > 1e-6f ? 1.0f / wterm : 0.0f;	// We shouldn't ever have 0 values because of the overdetermined system of equations but let's be careful anyway!
			tempX[rowIndex] = r * recW;
		}

		// 2.3.2) Perform V * (1/w * U^T * b)
		for ( U32 rowIndex=0; rowIndex < unknownsCount; rowIndex++ ) {
			float	r = 0.0f;
			for ( U32 i=0; i < unknownsCount; i++ ) {
				const float	Vterm = V[rowIndex][i];
				const float	tempXterm = tempX[i];
				r += Vterm * tempXterm;
			}
			x[rowIndex] = r;	// This is our final results!
		}


		// ===================================================================
		// 2.4] Recover curve values
		// At this point, we recovered the g(Z) for Z�[Zmin,Zmax], followed by the log2(Ei) for the N selected pixels
		// Let's just store the g(Z) into our target array
		if ( _luminanceOnly ) {
			for ( U32 Z=0; Z < responseCurveSize; Z++ ) {
				_responseCurve[Z].Set( x[Z], x[Z], x[Z] );
			}
		} else {
			for ( U32 Z=0; Z < responseCurveSize; Z++ ) {
				((float*) &_responseCurve[Z].x)[componentIndex] = x[Z];
			}
		}
	}

// 	delete[] tempX;
// 	delete[] x;
// 	delete[] b;
// 	delete[] w;
// 	delete[] V;
// 	delete[] Vterms;
// 	delete[] A;
// 	delete[] Aterms;
#endif

	delete[] pixels;
}

#pragma region BFGS Minimization

using namespace MathSolversLib;

// This model attempts to fit a 3rd order polynomial to the curve
class BFGSModel_polynomial : public BFGS::IModel {
	List< float >		m_curve;
	double				m_curveTentCenter;
	VectorD				m_parameters;
public:
	BFGSModel_polynomial() : m_parameters( 4 ) {}

	void				SetCurve( const List< bfloat3 >& _curve, U32 _componentIndex ) {
		m_curve.SetCount( _curve.Count() );
		for ( U32 i=0; i < _curve.Count(); i++ )
			m_curve[i] = ((float*) &_curve[i].x)[_componentIndex];
		m_curveTentCenter = 0.5 * _curve.Count();
	}

	static double		EvalModel( double x, const VectorD& _parameters ) {
//		return _parameters[0] + _parameters[1] * x + _parameters[2] * x*x + _parameters[3] * x*x*x;
		return _parameters[0] + x * (_parameters[1] + x * (_parameters[2] + x * _parameters[3]));
	}

	// IModel Implementation
	virtual VectorD&		getParameters() override						{ return m_parameters; }
	virtual void			setParameters( const VectorD& value ) override	{ value.CopyTo( m_parameters ); }
	virtual double			Eval( const VectorD& _newParameters ) override {
		double	sumSqDiff = 0.0;
		for ( U32 i=0; i < m_curve.Count(); i++ ) {
			double	curveValue = m_curve[i];
			double	modelValue = EvalModel( i, _newParameters );
			double	diff = modelValue - curveValue;
					diff *= TentFilter( i );
			sumSqDiff += diff * diff;
		}
		return sumSqDiff / (m_curve.Count()*m_curve.Count());
	}
	virtual void		Constrain( VectorD& _parameters ) override {}

private:
	double TentFilter( double x ) {
		return 1.0 - abs( x - m_curveTentCenter ) / m_curveTentCenter;
	}
};

#pragma endregion

void	Bitmap::FilterCameraResponseCurve( const BaseLib::List< bfloat3 >& _rawResponseCurve, BaseLib::List< bfloat3 >& _filteredResponseCurve, U32 _componentsCount, FILTER_TYPE _filterType ) {
	_filteredResponseCurve.SetCount( _rawResponseCurve.Count() );
	memcpy_s( _filteredResponseCurve.Ptr(), _filteredResponseCurve.Count()*sizeof(bfloat3), _rawResponseCurve.Ptr(), _rawResponseCurve.Count()*sizeof(bfloat3) );

	// Compute kernel size for tent and gaussian filters
	const S32	KERNEL_SIZE = S32(_filteredResponseCurve.Count()) / 8;	// Should be [-32,+32] for 256 entries response curve

	// Apply filtering
	switch ( _filterType ) {

		//////////////////////////////////////////////////////////////////////////
		// Use a gaussian filter
		case Bitmap::FILTER_TYPE::SMOOTHING_GAUSSIAN:
		case Bitmap::FILTER_TYPE::SMOOTHING_GAUSSIAN_2_PASSES: {
			const float	SIGMA = logf( 0.01f ) / (KERNEL_SIZE*KERNEL_SIZE);
			U32	passesCount = _filterType == FILTER_TYPE::SMOOTHING_GAUSSIAN_2_PASSES ? 2 : 1;
			for ( U32 passIndex=0; passIndex < passesCount; passIndex++ ) {
				for ( U32 i=0; i < _filteredResponseCurve.Count(); i++ ) {
					S32		minJ = MAX( 0, S32(i) - KERNEL_SIZE );
					S32		maxJ = MIN( S32(_filteredResponseCurve.Count()), S32(i) + KERNEL_SIZE );
					float	sumWeights = 0.0f;
					bfloat3	sum = bfloat3::Zero;
					for ( S32 j=minJ; j < maxJ; j++ ) {
						float	weight = expf( SIGMA * (j - S32(i))*(j - S32(i)) );		// Gaussian
						sumWeights += weight;
						sum += weight * _rawResponseCurve[j];
					}
					sum /= sumWeights;
					_filteredResponseCurve[i] = sum;
				}

				// Get ready for next pass
				if ( passIndex < passesCount-1 )
					memcpy_s( (void*) _rawResponseCurve.Ptr(), _rawResponseCurve.Count()*sizeof(bfloat3), _filteredResponseCurve.Ptr(), _filteredResponseCurve.Count()*sizeof(bfloat3) );
			}
			
			for ( U32 componentIndex=_componentsCount; componentIndex < 3; componentIndex++ ) {
				for ( U32 i=0; i < _filteredResponseCurve.Count(); i++ ) {
					bfloat3&	value = _filteredResponseCurve[i];
					((float*) &value.x)[componentIndex] = ((float*) &value.x)[componentIndex-1];
				}
			}
			break;
		}

		//////////////////////////////////////////////////////////////////////////
		// Use a tent filter
		case Bitmap::FILTER_TYPE::SMOOTHING_TENT: {
			for ( U32 componentIndex=0; componentIndex < _componentsCount; componentIndex++ ) {	// Because R, G, B
				for ( U32 i=0; i < _filteredResponseCurve.Count(); i++ ) {
					S32		minJ = MAX( 0, S32(i) - KERNEL_SIZE );
					S32		maxJ = MIN( S32(_filteredResponseCurve.Count()), S32(i) + KERNEL_SIZE );
					float	sumWeights = 0.0f;
					float	sum = 0.0f;
					for ( S32 j=minJ; j < maxJ; j++ ) {
						float	weight = 1.0f - abs( j - S32(i) ) / float(KERNEL_SIZE);	// Tent filter
						sumWeights += weight;
						sum += weight * ((float*) &_rawResponseCurve[j].x)[componentIndex];
					}
					sum /= sumWeights;
					((float*) &_filteredResponseCurve[i].x)[componentIndex] = sum;
				}
			}
			for ( U32 componentIndex=_componentsCount; componentIndex < 3; componentIndex++ ) {
				for ( U32 i=0; i < _filteredResponseCurve.Count(); i++ ) {
					bfloat3&	value = _filteredResponseCurve[i];
					((float*) &value.x)[componentIndex] = ((float*) &value.x)[componentIndex-1];
				}
			}
			break;
		}

		//////////////////////////////////////////////////////////////////////////
		// Use an exponential moving average to smooth out the input curve
// 		case Bitmap::FILTER_TYPE::EXPONENTIAL_MOVING_AVERAGE: {
// //			const float	alpha = 1.0f - expf( logf( 0.001f ) / _rawResponseCurve.Count() );
// 			const float	alpha = 0.5f;
// 
// 			for ( int iterationIndex=0; iterationIndex < 1; iterationIndex++ ) {
// 				for ( U32 componentIndex=0; componentIndex < _componentsCount; componentIndex++ ) {	// Because R, G, B
// 					float	movingSum = ((float*) &_filteredResponseCurve[0].x)[componentIndex];
// 					((float*) &_filteredResponseCurve[0].x)[componentIndex] = movingSum;
// 					for ( U32 i=1; i < _filteredResponseCurve.Count(); i++ ) {
// 						float		Yi = ((float*) &_filteredResponseCurve[i].x)[componentIndex];
// 						movingSum = alpha * Yi + (1.0f - alpha) * movingSum;
// 						((float*) &_filteredResponseCurve[i].x)[componentIndex] = movingSum;
// 					}
// 				}
// 			}
// 			for ( U32 componentIndex=_componentsCount; componentIndex < 3; componentIndex++ ) {
// 				for ( U32 i=0; i < _filteredResponseCurve.Count(); i++ ) {
// 					bfloat3&	value = _filteredResponseCurve[i];
// 					((float*) &value.x)[componentIndex] = ((float*) &value.x)[componentIndex-1];
// 				}
// 			}
// 			break;
// 		}

		//////////////////////////////////////////////////////////////////////////
		// Perform BFGS minimization of a 3rd order polynomial
		case Bitmap::FILTER_TYPE::CURVE_FITTING:
		case Bitmap::FILTER_TYPE::GAUSSIAN_PLUS_CURVE_FITTING: {

			if ( _filterType == Bitmap::FILTER_TYPE::GAUSSIAN_PLUS_CURVE_FITTING ) {
				// Apply gaussian filtering first
				const float	SIGMA = logf( 0.01f ) / (KERNEL_SIZE*KERNEL_SIZE);

				for ( U32 componentIndex=0; componentIndex < _componentsCount; componentIndex++ ) {	// Because R, G, B
					for ( U32 i=0; i < _filteredResponseCurve.Count(); i++ ) {
						S32		minJ = MAX( 0, S32(i) - KERNEL_SIZE );
						S32		maxJ = MIN( S32(_filteredResponseCurve.Count()), S32(i) + KERNEL_SIZE );
						float	sumWeights = 0.0f;
						float	sum = 0.0f;
						for ( S32 j=minJ; j < maxJ; j++ ) {
							float	weight = expf( SIGMA * (j - S32(i))*(j - S32(i)) );		// Gaussian
							sumWeights += weight;
							sum += weight * ((float*) &_rawResponseCurve[j].x)[componentIndex];
						}
						sum /= sumWeights;
						((float*) &_filteredResponseCurve[i].x)[componentIndex] = sum;
					}
				}
				memcpy_s( (void*) _rawResponseCurve.Ptr(), _rawResponseCurve.Count()*sizeof(bfloat3), _filteredResponseCurve.Ptr(), _filteredResponseCurve.Count()*sizeof(bfloat3) );
			}

			BFGSModel_polynomial	model;
			BFGS					solver;
			for ( U32 componentIndex=0; componentIndex < _componentsCount; componentIndex++ ) {	// Because R, G, B
				// Setup initial values
				model.SetCurve( _rawResponseCurve, componentIndex );
				model.getParameters()[0] = 0.0f;
				model.getParameters()[1] = 1.0f;
				model.getParameters()[2] = 0.0f;
				model.getParameters()[3] = 0.0f;

				// Solve!
				solver.Minimize( model );

				// Replace curve by fit polynomial model
				for ( U32 i=0; i < _filteredResponseCurve.Count(); i++ ) {
					bfloat3&	value = _filteredResponseCurve[i];
					float		f = float( BFGSModel_polynomial::EvalModel( i, model.getParameters() ) );
					((float*) &value.x)[componentIndex] = f;
				}
			}
			for ( U32 componentIndex=_componentsCount; componentIndex < 3; componentIndex++ ) {
				for ( U32 i=0; i < _filteredResponseCurve.Count(); i++ ) {
					bfloat3&	value = _filteredResponseCurve[i];
					((float*) &value.x)[componentIndex] = ((float*) &value.x)[componentIndex-1];
				}
			}
			break;
		}
	}
}

#pragma region SVD Decomposition

// Computes (a2 + b2)^1/2 without destructive underflow or overflow.
static float pythag(float a, float b) {
	float	absa = fabs(a);
	float	absb = fabs(b);
	if ( absa > absb )
		return absa*sqrt( 1.0f + SQR(absb/absa) );
	else
		return absb == 0.0f ? 0.0f : absb*sqrt( 1.0f + SQR(absa/absb) );
}

template<class T>
inline T SIGN(const T &a, const T &b) { return b >= 0 ? (a >= 0 ? a : -a) : (a >= 0 ? -a : a); }

static float maxarg1,maxarg2;
#define FMAX(a,b) (maxarg1=(a),maxarg2=(b),(maxarg1) > (maxarg2) ?\
        (maxarg1) : (maxarg2))

static int iminarg1,iminarg2;
#define IMIN(a,b) (iminarg1=(a),iminarg2=(b),(iminarg1) < (iminarg2) ?\
        (iminarg1) : (iminarg2))

//char	debugString[4096];

// From numerical recipes, chapter 2.6 (NOTE: I rewrote the code so it uses 0-based vectors and matrices!)
// (itself stolen from http://people.duke.edu/~hpgavin/SystemID/References/Golub+Reinsch-NM-1970.pdf)
//
// Given a matrix a[1..m][1..n], this routine computes its singular value decomposition, A = U � W � V^T
//	The matrix U replaces a on output.
//	The diagonal matrix of singular values W is output as a vector w[1..n].
//	The matrix V (not the transpose V^T) is output as v[1..n][1..n].
//
void svdcmp( int m, int n, float** a, float w[], float** v ) {
	float*	residualValues = new float[n];
	memset( residualValues, 0, n*sizeof(float) );

	float	f = 0.0f, g = 0.0f, h = 0.0f;
 	float	scale = 0.0f;
 	float	anorm = 0.0f;

	// Householder reduction to bidiagonal form
	for ( int i=0; i < n; i++ ) {
		int	l = i+1;
		residualValues[i] = scale * g;

		g = 0.0f;
		float	s = 0.0f;
		scale = 0.0f;
		if ( i < m ) {
			for ( int k=i; k < m; k++ )
				scale += fabs( a[k][i] );

			if ( scale != 0.0f ) {
				for ( int k=i; k < m; k++ ) {
					a[k][i] /= scale;
					s += a[k][i] * a[k][i];
				}
				f = a[i][i];
				g = -SIGN( sqrtf(s), f );
				h = f*g - s;
				a[i][i] = f-g;
				for ( int j=l; j < n; j++ ) {
					s = 0.0f;
					for ( int k = i; k < m; k++)
						s += a[k][i] * a[k][j];

					f = s/h;
					for ( int k=i; k < m; k++ )
						a[k][j] += f*a[k][i];
				}
				for ( int k=i; k < m; k++ )
					a[k][i] *= scale;
			}
		}
		w[i] = scale * g;

		g = 0.0f;
		s = 0.0f;
		scale = 0.0f;
		if ( i < m && l < n ) {
			for ( int k=l; k < n; k++ )
				scale += fabs( a[i][k] );

			if ( scale != 0.0f ) {
				for ( int k=l; k < n; k++ ) {
					a[i][k] /= scale;
					s += a[i][k] * a[i][k];
				}
				f = a[i][l];
				g = -SIGN( sqrtf(s), f );
				h = f * g - s;
				a[i][l] = f - g;
				for ( int k=l; k < n; k++ )
					residualValues[k] = a[i][k] / h;

				for ( int j=l; j < m; j++ ) {
					s = 0.0f;
					for ( int k=l; k < n; k++ )
						s += a[j][k] * a[i][k];

					for ( int k=l; k < n; k++ )
						a[j][k] += s * residualValues[k];
				}
				for ( int k=l; k < n; k++ )
					a[i][k] *= scale;
			}
		}
		anorm = FMAX( anorm, (fabs(w[i])+fabs(residualValues[i])) );
	}

	// Accumulation of right-hand transformations
	v[n-1][n-1] = 1.0f;
	g = residualValues[n-1];

	for ( int i=n-2; i >= 0; i-- ) {
		int	l = i+1;
		if ( g != 0.0f ) {
			for ( int j=l; j < n; j++ )
				v[j][i] = (a[i][j] / a[i][l]) / g;	// Double division to avoid possible underflow

			for ( int j=l; j < n; j++) {
				float	s = 0.0f;
				for ( int k=l; k < n; k++ )
					s += a[i][k] * v[k][j];
				for ( int k=l; k < n; k++ )
					v[k][j] += s * v[k][i];
			}
		}
		for ( int j=l; j < n; j++ )
			v[i][j] = v[j][i] = 0.0f;

		v[i][i] = 1.0f;
		g = residualValues[i];
	}

	// Accumulation of left-hand transformations
	for ( int i=IMIN(m,n)-1; i >= 0; i-- ) {
		int	l = i+1;
		g = w[i];
		for ( int j=l; j < n; j++ )
			a[i][j] = 0.0f;
		if ( g != 0.0f ) {
			g = 1.0f / g;
			for ( int j=l; j < n; j++ ) {
				float	s = 0.0f;
				for ( int k=l; k < m; k++ )
					s += a[k][i] * a[k][j];

				f = (s / a[i][i]) * g;
				for ( int k=i; k < m; k++ )
					a[k][j] += f * a[k][i];
			}
		}
		for ( int j=i; j < m; j++ )
			a[j][i] *= g;

		++a[i][i];
	}

//return;

	// Diagonalization of the bidiagonal form: Loop over singular values, and over allowed iterations
	const int	MAX_ITERATIONS = 30;
//	const int	MAX_ITERATIONS = 1000;

	for ( int k=n-1; k >= 0; k-- ) {
		int	iterationsCount = 1;
		for ( ; iterationsCount <= MAX_ITERATIONS; iterationsCount++ ) {
			int	nm;
			// Test for splitting
			bool	flag = true;
			int		l = k;
			for ( ; l >= 0; l-- ) {
				nm = l-1;
				// Note that rv1[0] is always zero so we eventually break at the very end
				if ( float( fabs(residualValues[l]) + anorm ) == anorm ) {
					flag = false;
					break;
				}
				ASSERT( nm >= 0, "nm in negative range!" );
				if ( float( fabs(w[nm]) + anorm ) == anorm )
					break;
			}
			if ( flag ) {
				// Cancellation of rv1[l], if l > 0.
				float	c = 0.0f;
				float	s = 1.0f;
				for ( int i=l; i < k; i++ ) {
					f = s * residualValues[i];
					residualValues[i] = c * residualValues[i];
					if ( float( fabs(f) + anorm ) == anorm )
						break;

					g = w[i];
					h = pythag( f, g );
					w[i] = h;
					h = 1.0f / h;
					c = g * h;
					s = -f * h;
					for ( int j=0; j < m; j++ ) {
						float	y = a[j][nm];
						float	z = a[j][i];
						a[j][nm] = y*c + z*s;
						a[j][i]  = z*c - y*s;
					}
				}
			}
			float	z = w[k];
			if ( l == k ) {
				// Convergence
				if ( z < 0.0f ) {
					// Singular value is made nonnegative
					w[k] = -z;
					for ( int j=0; j < n; j++ )
						v[j][k] = -v[j][k];
				}
				break;
			}
			if ( iterationsCount >= MAX_ITERATIONS )
				throw "No convergence in svdcmp iterations";

			nm = k-1;
			ASSERT( nm >= 0, "nm in negative range!" );

			float	x = w[l];	// Shift from bottom 2-by-2 minor
			float	y = w[nm];
			g = residualValues[nm];
			h = residualValues[k];
			f = ((y-z)*(y+z)+(g-h)*(g+h)) / (2.0f*h*y);
			g = pythag( f, 1.0f );
			f = ((x-z)*(x+z)+h*((y/(f+SIGN(g,f)))-h))/x;

// sprintf( debugString, "ITERATION %d = f = %f - g = %f - h = %f\r\n", iterationsCount, f, g, h );
// OutputDebugStringA( debugString );

			// Next QR transformation
			float	c = 1.0f;
			float	s = 1.0f;
			for ( int j=l; j <= nm; j++ ) {
				int	i = j+1;
				g = residualValues[i];
				y = w[i];
				h = s*g;
				g = c*g;
				z = pythag( f, h );
				residualValues[j] = z;
				c = f/z;
				s = h/z;
				f = x*c + g*s;	// Rotation with sine/cosine?
				g = g*c - x*s;
				h = y*s;
				y *= c;
				for ( int jj=0; jj < n; jj++ ) {
					x = v[jj][j];
					z = v[jj][i];
					v[jj][j] = x*c + z*s;
					v[jj][i] = z*c - x*s;
				}
				z = pythag( f, h );
				w[j] = z;
				if ( z != 0.0f ) {
					z = 1.0f / z;
					c = f*z;
					s = h*z;
				}
				f = c*g + s*y;
				x = c*y - s*g;
				for ( int jj=0; jj < m; jj++ ) {
					y = a[jj][j];
					z = a[jj][i];
					a[jj][j] = y*c + z*s;
					a[jj][i] = z*c - y*s;
				}

// sprintf( debugString, "j = %d -> z = %f - f = %f - x = %f\r\n", j, z, f, x );
// OutputDebugStringA( debugString );
			}
			residualValues[l] = 0.0f;
			residualValues[k] = f;
			w[k] = x;
		}	// for ( ; iterationsCount <= 30; iterationsCount++ ) 
	}	// for ( int k=n-1; k >= 0; k-- ) 

	delete[] residualValues;
}


void svdcmp_ORIGINAL(int m, int n, float **a, float w[], float **v) {
	int flag,i,its,j,jj,k,l,nm;
	float anorm,c,f,g,h,s,scale,x,y,z,*rv1;

	rv1 = new float[1+n];
	memset( rv1, 0, (1+n) * sizeof(float) );

	g=scale=anorm=0.0;
	for (i=1;i<=n;i++) {
		l=i+1;
		rv1[i]=scale*g;
		g=s=scale=0.0;
		if (i <= m) {
			for (k=i;k<=m;k++) scale += fabs(a[k][i]);
			if (scale) {
				for (k=i;k<=m;k++) {
					a[k][i] /= scale;
					s += a[k][i]*a[k][i];
				}
				f=a[i][i];
				g = -SIGN(sqrtf(s),f);
				h=f*g-s;
				a[i][i]=f-g;
				for (j=l;j<=n;j++) {
					for (s=0.0,k=i;k<=m;k++) s += a[k][i]*a[k][j];
					f=s/h;
					for (k=i;k<=m;k++) a[k][j] += f*a[k][i];
				}
				for (k=i;k<=m;k++) a[k][i] *= scale;
			}
		}
		w[i]=scale *g;
		g=s=scale=0.0;
		if (i <= m && i != n) {
			for (k=l;k<=n;k++) scale += fabs(a[i][k]);
			if (scale) {
				for (k=l;k<=n;k++) {
					a[i][k] /= scale;
					s += a[i][k]*a[i][k];
				}
				f=a[i][l];
				g = -SIGN(sqrtf(s),f);
				h=f*g-s;
				a[i][l]=f-g;
				for (k=l;k<=n;k++) rv1[k]=a[i][k]/h;
				for (j=l;j<=m;j++) {
					for (s=0.0,k=l;k<=n;k++) s += a[j][k]*a[i][k];
					for (k=l;k<=n;k++) a[j][k] += s*rv1[k];
				}
				for (k=l;k<=n;k++) a[i][k] *= scale;
			}
		}
		anorm=FMAX(anorm,(fabs(w[i])+fabs(rv1[i])));
	}

	for (i=n;i>=1;i--) {
		if (i < n) {
			if (g) {
				for (j=l;j<=n;j++)
					v[j][i]=(a[i][j]/a[i][l])/g;
				for (j=l;j<=n;j++) {
					for (s=0.0,k=l;k<=n;k++) s += a[i][k]*v[k][j];
					for (k=l;k<=n;k++) v[k][j] += s*v[k][i];
				}
			}
			for (j=l;j<=n;j++) v[i][j]=v[j][i]=0.0;
		}
		v[i][i]=1.0;
		g=rv1[i];
		l=i;
	}

	for (i=IMIN(m,n);i>=1;i--) {
		l=i+1;
		g=w[i];
		for (j=l;j<=n;j++) a[i][j]=0.0;
		if (g) {
			g=1.0f/g;
			for (j=l;j<=n;j++) {
				for (s=0.0,k=l;k<=m;k++) s += a[k][i]*a[k][j];
				f=(s/a[i][i])*g;
				for (k=i;k<=m;k++) a[k][j] += f*a[k][i];
			}
			for (j=i;j<=m;j++) a[j][i] *= g;
		} else for (j=i;j<=m;j++) a[j][i]=0.0;
		++a[i][i];
	}

	for (k=n;k>=1;k--) {
		for (its=1;its<=30;its++) {
			flag=1;
			for (l=k;l>=1;l--) {
				nm=l-1;
				if ((float)(fabs(rv1[l])+anorm) == anorm) {
					flag=0;
					break;
				}
				if ((float)(fabs(w[nm])+anorm) == anorm) break;
			}
			if (flag) {
				c=0.0;
				s=1.0;
				for (i=l;i<=k;i++) {
					f=s*rv1[i];
					rv1[i]=c*rv1[i];
					if ((float)(fabs(f)+anorm) == anorm) break;
					g=w[i];
					h=pythag(f,g);
					w[i]=h;
					h=1.0f/h;
					c=g*h;
					s = -f*h;
					for (j=1;j<=m;j++) {
						y=a[j][nm];
						z=a[j][i];
						a[j][nm]=y*c+z*s;
						a[j][i]=z*c-y*s;
					}
				}
			}
			z=w[k];
			if (l == k) {
				if (z < 0.0) {
					w[k] = -z;
					for (j=1;j<=n;j++) v[j][k] = -v[j][k];
				}
				break;
			}
			if (its == 30) throw "no convergence in 30 svdcmp iterations";
			x=w[l];
			nm=k-1;
			y=w[nm];
			g=rv1[nm];
			h=rv1[k];
			f=((y-z)*(y+z)+(g-h)*(g+h))/(2.0f*h*y);
			g=pythag(f,1.0);
			f=((x-z)*(x+z)+h*((y/(f+SIGN(g,f)))-h))/x;


// sprintf( debugString, "ITERATION %d = f = %f - g = %f - h = %f\r\n", its, f, g, h );
// OutputDebugStringA( debugString );


			c=s=1.0;
			for (j=l;j<=nm;j++) {
				i=j+1;
				g=rv1[i];
				y=w[i];
				h=s*g;
				g=c*g;
				z=pythag(f,h);
				rv1[j]=z;
				c=f/z;
				s=h/z;
				f=x*c+g*s;
				g = g*c-x*s;
				h=y*s;
				y *= c;
				for (jj=1;jj<=n;jj++) {
					x=v[jj][j];
					z=v[jj][i];
					v[jj][j]=x*c+z*s;
					v[jj][i]=z*c-x*s;
				}
				z=pythag(f,h);
				w[j]=z;
				if (z) {
					z=1.0f/z;
					c=f*z;
					s=h*z;
				}
				f=c*g+s*y;
				x=c*y-s*g;
				for (jj=1;jj<=m;jj++) {
					y=a[jj][j];
					z=a[jj][i];
					a[jj][j]=y*c+z*s;
					a[jj][i]=z*c-y*s;
				}


// sprintf( debugString, "j = %d -> z = %f - f = %f - x = %f\r\n", j-1, z, f, x );
// OutputDebugStringA( debugString );
			}
			rv1[l]=0.0;
			rv1[k]=f;
			w[k]=x;
		}
	}

	delete[] rv1;
}

#pragma endregion

/* MATLAB CODE
	%
	% gsolve.m - Solve for imaging system response function
	%
	% Given a set of pixel values observed for several pixels in several
	% images with different exposure times, this function returns the
	% imaging system�s response function g as well as the log film irradiance
	% values for the observed pixels.
	%
	% Assumes:
	%
	% Zmin = 0
	% Zmax = 255
	%
	% Arguments:
	%
	% Z(i,j) is the pixel values of pixel location number i in image j
	% B(j) is the log delta t, or log shutter speed, for image j
	% l is lamdba, the constant that determines the amount of smoothness
	% w(z) is the weighting function value for pixel value z
	%
	% Returns:
	%
	% g(z) is the log exposure corresponding to pixel value z
	% lE(i) is the log film irradiance at pixel location i
	%
	function [g,lE]=gsolve(Z,B,l,w)
		n = 256;
		A = zeros(size(Z,1)*size(Z,2)+n+1,n+size(Z,1));
		b = zeros(size(A,1),1);

		%% Include the data-fitting equations
		k = 1;
		for i=1:size(Z,1)
			for j=1:size(Z,2)
				wij = w(Z(i,j)+1);
				A(k,Z(i,j)+1) = wij; A(k,n+i) = -wij; b(k,1) = wij * B(i,j);
				k=k+1;
			end
		end

		%% Fix the curve by setting its middle value to 0
		A(k,129) = 1;
		k=k+1;

		%% Include the smoothness equations
		for i=1:n-2
		A(k,i)=l*w(i+1); A(k,i+1)=-2*l*w(i+1); A(k,i+2)=l*w(i+1);
		k=k+1;
	end

	%% Solve the system using SVD
	x = A\b;
	g = x(1:n);
	lE = x(n+1:size(x,1));
*/
