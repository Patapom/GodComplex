#include "stdafx.h"
#include "Bitmap.h"

using namespace ImageUtilityLib;

// This is the core of the bitmap class
// This method converts any image file into a float4 CIE XYZ format using the provided profile or the profile associated to the file
void	Bitmap::FromImageFile( const ImageFile& _sourceFile, ColorProfile* _profileOverride, bool _unPremultiplyAlpha ) {
	m_colorProfile = _profileOverride != nullptr ? _profileOverride : _sourceFile.GetColorProfile();
 	if ( m_colorProfile == nullptr )
 		throw "The provided file doesn't contain a valid color profile and you did not provide any profile override to initialize the bitmap!";

	// Convert for float4 format
	FIBITMAP*	float4Bitmap = FreeImage_ConvertToType( _sourceFile.m_bitmap, FIT_RGBAF );

	m_width = FreeImage_GetWidth( float4Bitmap );
	m_height = FreeImage_GetHeight( float4Bitmap );

	// Convert to XYZ in bulk using profile
	const bfloat4*	source = (const bfloat4*) FreeImage_GetBits( float4Bitmap );
	m_XYZ = new bfloat4[m_width * m_height];
	m_colorProfile->RGB2XYZ( source, m_XYZ, U32(m_width * m_height) );

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

// And this method converts back the bitmap to any format
void	Bitmap::ToImageFile( ImageFile& _targetFile, ImageFile::PIXEL_FORMAT _targetFormat, bool _premultiplyAlpha ) const {
 	if ( m_colorProfile == nullptr )
 		throw "The bitmap doesn't contain a valid color profile to initialize the image file!";

	FREE_IMAGE_TYPE	targetType = ImageFile::PixelFormat2FIT( _targetFormat );
	if ( targetType == FIT_UNKNOWN )
		throw "Unsupported target type!";

	// Convert back to float4 RGB using color profile
	ImageFile		float4Image( m_width, m_height, ImageFile::PIXEL_FORMAT::RGBA32F, *m_colorProfile );
	const bfloat4*	source = m_XYZ;
	bfloat4*			target = (bfloat4*) float4Image.GetBits();
	if ( _premultiplyAlpha ) {
		// Pre-multiply by alpha
		const bfloat4*	unPreMultipliedSource = m_XYZ;
		bfloat4*			preMultipliedTarget = target;
		for ( U32 i=m_width*m_height; i > 0; i--, unPreMultipliedSource++, preMultipliedTarget++ ) {
			preMultipliedTarget->x = unPreMultipliedSource->x * unPreMultipliedSource->w;
			preMultipliedTarget->y = unPreMultipliedSource->y * unPreMultipliedSource->w;
			preMultipliedTarget->z = unPreMultipliedSource->z * unPreMultipliedSource->w;
			preMultipliedTarget->w = unPreMultipliedSource->w;
		}
		source = target;	// In-place conversion
	}
	m_colorProfile->XYZ2RGB( source, target, m_width*m_height );

	// Convert to target bitmap
	FIBITMAP*	targetBitmap = FreeImage_ConvertToType( float4Image.m_bitmap, targetType );

	// Substitute bitmap pointer into target file
	_targetFile.Exit();
	_targetFile.m_bitmap = targetBitmap;
}


//////////////////////////////////////////////////////////////////////////
// LDR -> HDR Conversion
//
void	Bitmap::LDR2HDR( U32 _imagesCount, ImageFile* _images, float* _imageEVs, const HDRParms& _parms ) {

}

void	Bitmap::LDR2HDR( U32 _imagesCount, ImageFile* _images, float* _imageEVs, const List< bfloat3 >& _responseCurve, const HDRParms& _parms ) {

}

void	Bitmap::ComputeHDRResponseCurve( U32 _imagesCount, ImageFile* _images, float* _imageEVs, const HDRParms& _parms, List< bfloat3 >& _responseCurve ) {

	//////////////////////////////////////////////////////////////////////////
	// 1] Find the best possible samples across the provided images
	// According to Debevec in §2.1:
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
	int		responseCurveSize = 1 << _parms._inputBitsPerComponent;
	float	nominalPixelsCount = float(responseCurveSize) / _imagesCount;				// Use about that amount of pixels across images to have a nice over-determined system
			nominalPixelsCount *= 1.0f + _parms._quality;								// Apply the user's quality settings to use more or less pixels
	int		pixelsCountPerImage = int ( ceilf( nominalPixelsCount / _imagesCount ) );	// And that is our amount of pixels to use per image
	int		totalPixelsCount = _imagesCount * pixelsCountPerImage;

	// Now, we need to carefully select the candidate pixels.
	// Still quoting Debevec in §2.1:
	//	<< Clearly, the pixel locations should be chosen so that they have a reasonably even distribution of pixel values from Zmin to Zmax,
	//		and so that they are spatially well distributed in the image.
	//	   Furthermore, the pixels are best sampled from regions of the image with low intensity variance so that radiance can be assumed to
	//		be constant across the area of the pixel, and the effect of optical blur of the imaging system is minimized. >>
	//
	List< bfloat3 >	pixels( totalPixelsCount );
	for ( int i=0; i < 3; i++ ) {
		// Do for R, G and B
	}

	//////////////////////////////////////////////////////////////////////////
	// 2] Apply SVD 
	int	equationsCount  = totalPixelsCount		// Pixels
						+ responseCurveSize		// Used to define the smoothness of the g curve
						+ 1;					// Constraint that g(Zmid) = 0 (with Zmid = (Zmax+Zmin)/2)
	int	unknownsCount  = totalPixelsCount		// Pixels solution
						+ responseCurveSize;	// g curve solution

	int	Zmid = responseCurveSize / 2;

	float*	Aterms = new float[equationsCount*unknownsCount];
	float**	A = new float*[equationsCount];
	for ( int rowIndex=0; rowIndex < equationsCount; rowIndex++ ) A[rowIndex] = &Aterms[equationsCount*rowIndex];
	float*	b = new float[unknownsCount];
	for ( int i=0; i < 3; i++ ) {	// Because R, G, B

		// 2.1] Build the A matrix
		memset( A, 0, equationsCount*unknownsCount*sizeof(float) );
		for ( int pixelIndex=0; pixelIndex < totalPixelsCount; pixelIndex++ ) {
			int		Z = CLAMP( int( (responseCurveSize-1) * ((float*) &pixels[totalPixelsCount].x)[i] ), 0, responseCurveSize-1 );
			int		wZ = 1 + (Z < Zmid ? Z : responseCurveSize-Z);	// Z weighted by a hat function
			A[pixelIndex][Z] = float(wZ);
			A[pixelIndex][responseCurveSize+Z] = float(wZ);
			b[pixelIndex][Z] = wZ;
		}


		// 2.2] Build the target vector
		memset( b, 0, equationsCount*sizeof(float) );

		// 2.3] Apply

		// 2.4] Profit
	}
	delete[] b;
	delete[] A;
	delete[] Aterms;
}

#pragma region SVD Decomposition

float pythag(float a, float b);
template<class T>
inline T SIGN(const T &a, const T &b) { return b >= 0 ? (a >= 0 ? a : -a) : (a >= 0 ? -a : a); }

static float maxarg1,maxarg2;
#define FMAX(a,b) (maxarg1=(a),maxarg2=(b),(maxarg1) > (maxarg2) ?\
        (maxarg1) : (maxarg2))

static int iminarg1,iminarg2;
#define IMIN(a,b) (iminarg1=(a),iminarg2=(b),(iminarg1) < (iminarg2) ?\
        (iminarg1) : (iminarg2))

// From numerical recipes, chapter 2.6 (NOTE: I rewrote the code to use 0-based vectors and matrices!)
//
// Given a matrix a[1..m][1..n], this routine computes its singular value decomposition, A = U · W · V^T
//	The matrix U replaces a on output.
//	The diagonal matrix of singular values W is output as a vector w[1..n].
//	The matrix V (not the transpose V^T) is output as v[1..n][1..n].
//
void svdcmp( int m, int n, float** a, float w[], float** v ) {
//	bool	flag;
//	int		flag,jj,nm;
//	float	c,f,h,s,x,y,z;

	float*	residualValues = new float[n];
 	float	g = 0.0f;
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
				float	f = a[i][i];
				g = -SIGN( sqrtf(s), f );
				float	h = f*g - s;
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
				float	f = a[i][l];
				g = -SIGN( sqrt(s), f );
				float	h = f * g - s;
				a[i][l] = f - g;
				for ( int k=l; k < n; k++)
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
	for ( int i=IMIN(m,n)-1; i >= 0; i--) {
		int	l = i+1;
		g = w[i];
		for ( int j=l; j < n; j++ )
			a[i][j] = 0.0f;
		if ( g != 0.0f ) {
			g = 1.0f / g;
			for ( int j=l; j < n; j++) {
				float	s = 0.0f;
				for ( int k=l; k < m; k++ )
					s += a[k][i] * a[k][j];

				float	f = ( s / a[i][i]) * g;
				for ( int k=i; k < m; k++ )
					a[k][j] += f * a[k][i];
			}
			for ( int j=i; j < m; j++ )
				a[j][i] *= g;
		} else {
			for ( int j=i; j < m; j++ )
				a[j][i] = 0.0f;
		}
		++a[i][i];
	}

	// Diagonalization of the bidiagonal form: Loop over singular values, and over allowed iterations
	for ( int k=n-1; k >= 0; k--) {
		int	iterationsCount = 1;
		for ( ;iterationsCount <= 30; iterationsCount++ ) {
			int	nm;
			// Test for splitting
			bool	flag = true;
			int		l = k;
			for ( ; l >= 0; l--) {
				nm = l-1;
				// Note that rv1[0] is always zero
				if ( float( fabs(residualValues[l]) + anorm ) == anorm ) {
					flag = false;
					break;
				}
				if ( float( fabs(w[nm]) + anorm ) == anorm )
					break;
			}
			if ( flag ) {
				// Cancellation of rv1[l], if l > 0.
				float	c = 0.0f;
				float	s = 1.0f;
				for ( int i=l; i < k; i++ ) {
					float	f = s * residualValues[i];
					residualValues[i] = c*residualValues[i];
					if ( float( fabs(f) + anorm ) == anorm )
						break;

					float	g = w[i];
					float	h = pythag( f, g );
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
			if ( iterationsCount == 30 )
				throw "no convergence in 30 svdcmp iterations";

			nm = k-1;
			float	x = w[l];
			float	y = w[nm];
			float	g = residualValues[nm];
			float	h = residualValues[k];
			float	f = ((y-z)*(y+z)+(g-h)*(g+h)) / (2.0f*h*y);
			g = pythag( f, 1.0f );
			f = ((x-z)*(x+z)+h*((y/(f+SIGN(g,f)))-h))/x;

			float	c = 1.0f;
			float	s = 1.0f;
			for ( int j=l; j < nm; j++ ) {
				int	i = j+1;
				g = residualValues[i];
				y = w[i];
				h = s*g;
				g = c*g;
				z = pythag( f, h );
				residualValues[j] = z;
				c = f/z;
				s = h/z;
				f = x*c+g*s;
				g = g*c-x*s;
				h = y*s;
				y *= c;
				for ( int jj=0; jj < n; jj++) {
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
			}
			residualValues[l] = 0.0f;
			residualValues[k] = f;
			w[k] = x;
		}
	}
	delete[] residualValues;
}

// Computes (a2 + b2)^1/2 without destructive underflow or overflow.
float pythag(float a, float b) {
	float	absa = fabs(a);
	float	absb = fabs(b);
	if ( absa > absb )
		return absa*sqrt( 1.0f + SQR(absb/absa) );
	else
		return absb == 0.0f ? 0.0f : absb*sqrt( 1.0f + SQR(absa/absb) );
}

#pragma endregion

/* MATLAB CODE
	%
	% gsolve.m - Solve for imaging system response function
	%
	% Given a set of pixel values observed for several pixels in several
	% images with different exposure times, this function returns the
	% imaging system’s response function g as well as the log film irradiance
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