#include "stdafx.h"
#include "ColorProfile.h"

using namespace ImageUtilityLib;

const bfloat2	ColorProfile::ILLUMINANT_A( 0.44757f, 0.40745f );	// Incandescent, tungsten
const bfloat2	ColorProfile::ILLUMINANT_D50( 0.34567f, 0.35850f );	// Daylight, Horizon
const bfloat2	ColorProfile::ILLUMINANT_D55( 0.33242f, 0.34743f );	// Mid-Morning, Mid-Afternoon
const bfloat2	ColorProfile::ILLUMINANT_D65( 0.31271f, 0.32902f );	// Daylight, Noon, Overcast (sRGB reference illuminant)
const bfloat2	ColorProfile::ILLUMINANT_E( 1/3.0f, 1/3.0f );		// Reference

const float		ColorProfile::GAMMA_EXPONENT_STANDARD = 2.2f;
const float		ColorProfile::GAMMA_EXPONENT_sRGB = 2.4f;
const float		ColorProfile::GAMMA_EXPONENT_ADOBE = 2.19921875f;
const float		ColorProfile::GAMMA_EXPONENT_PRO_PHOTO = 1.8f;

const float		ColorProfile::CMF_WAVELENGTH_START = 390.0f;
const float		ColorProfile::CMF_WAVELENGTH_END = 830.0f;
const float		ColorProfile::CMF_WAVELENGTH_STEP = 0.1f;
const float		ColorProfile::CMF_WAVELENGTH_RCP_STEP = 1.0f / CMF_WAVELENGTH_STEP;

// Standard chromaticities
const ColorProfile::Chromaticities	ColorProfile::Chromaticities::Empty;
const ColorProfile::Chromaticities	ColorProfile::Chromaticities::sRGB			( bfloat2( 0.6400f, 0.3300f ), bfloat2( 0.3000f, 0.6000f ), bfloat2( 0.1500f, 0.0600f ), ILLUMINANT_D65 );
const ColorProfile::Chromaticities	ColorProfile::Chromaticities::AdobeRGB_D50	( bfloat2( 0.6400f, 0.3300f ), bfloat2( 0.2100f, 0.7100f ), bfloat2( 0.1500f, 0.0600f ), ILLUMINANT_D50 );
const ColorProfile::Chromaticities	ColorProfile::Chromaticities::AdobeRGB_D65	( bfloat2( 0.6400f, 0.3300f ), bfloat2( 0.2100f, 0.7100f ), bfloat2( 0.1500f, 0.0600f ), ILLUMINANT_D65 );
const ColorProfile::Chromaticities	ColorProfile::Chromaticities::ProPhoto		( bfloat2( 0.7347f, 0.2653f ), bfloat2( 0.1596f, 0.8404f ), bfloat2( 0.0366f, 0.0001f ), ILLUMINANT_D50 );
const ColorProfile::Chromaticities	ColorProfile::Chromaticities::Radiance		( bfloat2( 0.6400f, 0.3300f ), bfloat2( 0.2900f, 0.6000f ), bfloat2( 0.1500f, 0.0600f ), ILLUMINANT_E );

//////////////////////////////////////////////////////////////////////////
// Copy constructor
ColorProfile::ColorProfile( const ColorProfile& _other ) : m_internalConverter( nullptr ) {
	m_profileFoundInFile = _other.m_profileFoundInFile;
	m_chromaticities = _other.m_chromaticities;
	m_gammaCurve = _other.m_gammaCurve;
	m_gamma = _other.m_gamma;

	// Rebuild internal converter and matrices
	BuildTransformFromChroma( true );
}

//////////////////////////////////////////////////////////////////////////
// Builds the RGB<->XYZ transforms from chromaticities
// (refer to http://wiki.nuaj.net/index.php/Color_Transforms#XYZ_Matrices for explanations)
//
void	ColorProfile::BuildTransformFromChroma( bool _checkGammaCurveOverride ) {
	bfloat3	xyz_R( m_chromaticities.R.x, m_chromaticities.R.y, 1.0f - m_chromaticities.R.x - m_chromaticities.R.y );
	bfloat3	xyz_G( m_chromaticities.G.x, m_chromaticities.G.y, 1.0f - m_chromaticities.G.x - m_chromaticities.G.y );
	bfloat3	xyz_B( m_chromaticities.B.x, m_chromaticities.B.y, 1.0f - m_chromaticities.B.x - m_chromaticities.B.y );
	bfloat3	XYZ_W;
	xyY2XYZ( bfloat3( m_chromaticities.W.x, m_chromaticities.W.y, 1.0f ), XYZ_W );

	// Build the xyz->RGB matrix
	m_XYZ2RGB.r[0].Set( xyz_R, 0.0f );
	m_XYZ2RGB.r[1].Set( xyz_G, 0.0f );
	m_XYZ2RGB.r[2].Set( xyz_B, 0.0f );
	m_XYZ2RGB.r[3].Set( 0,0,0, 1.0f );
	m_XYZ2RGB.Invert();

	// Knowing the XYZ of the white point, we retrieve the scale factor for each axis x, y and z that will help us get X, Y and Z
	bfloat4	sum_RGB = bfloat4( XYZ_W, 0.0f ) * m_XYZ2RGB;

// Compute a 2nd white point
bfloat3	XYZ_W_in;
xyY2XYZ( bfloat3( ILLUMINANT_D50.x, ILLUMINANT_D50.y, 1.0f ), XYZ_W_in );
bfloat4	sum_RGB_in = bfloat4( XYZ_W_in, 0.0f ) * m_XYZ2RGB;

// sum_RGB.Set( 2, 1, 1, 0 );
// sum_RGB_in.Set( 1, 1, 1, 0 );

	// Finally, we can retrieve the RGB->XYZ transform
	m_RGB2XYZ.r[0].Set( sum_RGB.x * xyz_R, 0.0f );
	m_RGB2XYZ.r[1].Set( sum_RGB.y * xyz_G, 0.0f );
	m_RGB2XYZ.r[2].Set( sum_RGB.z * xyz_B, 0.0f );
	m_RGB2XYZ.r[3].Set( 0, 0, 0, 1 );

	// And the XYZ->RGB transform is simply the existing xyz->RGB matrix scaled by the reciprocal of the sum
	bfloat3	recSum_RGB( 1.0f / sum_RGB.x, 1.0f / sum_RGB.y, 1.0f / sum_RGB.z );
	(bfloat3&) m_XYZ2RGB.r[0] = (bfloat3&) m_XYZ2RGB.r[0] * recSum_RGB;
	(bfloat3&) m_XYZ2RGB.r[1] = (bfloat3&) m_XYZ2RGB.r[1] * recSum_RGB;
	(bfloat3&) m_XYZ2RGB.r[2] = (bfloat3&) m_XYZ2RGB.r[2] * recSum_RGB;

	float4x4	test = m_RGB2XYZ * m_XYZ2RGB;

// for ( int i=0; i < 3; i++ )
// 	for ( int j=0; j < 3; j++ ) {
// 		float	b = m_XYZ2RGB.r[i][j];
// 		float	a = M_xyz.r[i][j];
// 				a /= sum_RGB[j];
// 		ASSERT( fabs( a - b ) < 1e-3f, "mismatch!" );
// 	}

float4x4	XYZ2RGB_in;
XYZ2RGB_in.r[0].Set( sum_RGB_in.x * xyz_R, 0.0f );
XYZ2RGB_in.r[1].Set( sum_RGB_in.y * xyz_G, 0.0f );
XYZ2RGB_in.r[2].Set( sum_RGB_in.z * xyz_B, 0.0f );
XYZ2RGB_in.r[3].Set( 0,0,0, 1.0f );

XYZ2RGB_in.Invert();
float4x4	test2 = XYZ2RGB_in * m_RGB2XYZ;
//float4x4	test2 = XYZ2RGB_in * m_XYZ2RGB;

bfloat4		newR = bfloat4( sum_RGB_in.x * xyz_R, 0.0f ) * test2;
bfloat4		newG = bfloat4( sum_RGB_in.y * xyz_G, 0.0f ) * test2;
bfloat4		newB = bfloat4( sum_RGB_in.z * xyz_B, 0.0f ) * test2;
bfloat4		newWhite = bfloat4( XYZ_W_in, 0.0f ) * test2;

// Test outer product
float3x3	outer;
outer.r[0].Set( sum_RGB.x / sum_RGB_in.x, sum_RGB.y / sum_RGB_in.x, sum_RGB.z / sum_RGB_in.x );
outer.r[1].Set( sum_RGB.x / sum_RGB_in.y, sum_RGB.y / sum_RGB_in.y, sum_RGB.z / sum_RGB_in.y );
outer.r[2].Set( sum_RGB.x / sum_RGB_in.z, sum_RGB.y / sum_RGB_in.z, sum_RGB.z / sum_RGB_in.z );

	// ============= Attempt to recognize a standard profile ============= 
	STANDARD_PROFILE	recognizedChromaticity = m_chromaticities.FindRecognizedChromaticity();

	if ( _checkGammaCurveOverride ) {
		// Also ensure the gamma ramp is correct before assigning a standard profile
		bool	bIsGammaCorrect = true;
		switch ( recognizedChromaticity ) {
			case STANDARD_PROFILE::sRGB:			bIsGammaCorrect = EnsureGamma( GAMMA_CURVE::sRGB, GAMMA_EXPONENT_sRGB ); break;
			case STANDARD_PROFILE::ADOBE_RGB_D50:	bIsGammaCorrect = EnsureGamma( GAMMA_CURVE::STANDARD, GAMMA_EXPONENT_ADOBE ); break;
			case STANDARD_PROFILE::ADOBE_RGB_D65:	bIsGammaCorrect = EnsureGamma( GAMMA_CURVE::STANDARD, GAMMA_EXPONENT_ADOBE ); break;
			case STANDARD_PROFILE::PRO_PHOTO:		bIsGammaCorrect = EnsureGamma( GAMMA_CURVE::PRO_PHOTO, GAMMA_EXPONENT_PRO_PHOTO ); break;
			case STANDARD_PROFILE::RADIANCE:		bIsGammaCorrect = EnsureGamma( GAMMA_CURVE::STANDARD, 1.0f ); break;
		}

		if ( !bIsGammaCorrect )
			recognizedChromaticity = STANDARD_PROFILE::CUSTOM;	// A non-standard gamma curves fails our pre-defined design...
	}

	// ============= Assign the internal converter depending on the profile =============
	SAFE_DELETE( m_internalConverter );

	switch ( recognizedChromaticity ) {
		case STANDARD_PROFILE::sRGB:
			m_gammaCurve = GAMMA_CURVE::sRGB;
			m_gamma = GAMMA_EXPONENT_sRGB;
			m_internalConverter = new InternalColorConverter_sRGB();
			break;

		case STANDARD_PROFILE::ADOBE_RGB_D50:
			m_gammaCurve = GAMMA_CURVE::STANDARD;
			m_gamma = GAMMA_EXPONENT_ADOBE;
			m_internalConverter = new InternalColorConverter_AdobeRGB_D50();
			break;

		case STANDARD_PROFILE::ADOBE_RGB_D65:
			m_gammaCurve = GAMMA_CURVE::STANDARD;
			m_gamma = GAMMA_EXPONENT_ADOBE;
			m_internalConverter = new InternalColorConverter_AdobeRGB_D65();
			break;

		case STANDARD_PROFILE::PRO_PHOTO:
			m_gammaCurve = GAMMA_CURVE::PRO_PHOTO;
			m_gamma = GAMMA_EXPONENT_PRO_PHOTO;
			m_internalConverter = new InternalColorConverter_ProPhoto();
			break;

		case STANDARD_PROFILE::RADIANCE:
			m_gammaCurve = GAMMA_CURVE::STANDARD;
			m_gamma = 1.0f;
			m_internalConverter = new InternalColorConverter_Radiance();
			break;

		default:	// Switch to one of our generic converters
			switch ( m_gammaCurve ) {
				case GAMMA_CURVE::sRGB:
					m_internalConverter = new InternalColorConverter_Generic_sRGBGamma( m_RGB2XYZ, m_XYZ2RGB );
					break;
				case GAMMA_CURVE::PRO_PHOTO:
					m_internalConverter = new InternalColorConverter_Generic_ProPhoto( m_RGB2XYZ, m_XYZ2RGB );
					break;
				case GAMMA_CURVE::STANDARD:
					if ( fabs( m_gamma - 1.0f ) < 1e-3f )
						m_internalConverter = new InternalColorConverter_Generic_NoGamma( m_RGB2XYZ, m_XYZ2RGB );
					else
						m_internalConverter = new InternalColorConverter_Generic_StandardGamma( m_RGB2XYZ, m_XYZ2RGB, m_gamma );
					break;
			}
			break;
	}
}


//////////////////////////////////////////////////////////////////////////
// Spectral Power Conversions and Chromaticity Helpers

// Computes the XYZ matrix to perform white balancing between 2 white points assuming the R,G,B chromaticities are the same for the input and output profiles
// Re-using the equations from http://wiki.nuaj.net/index.php/Color_Transforms#XYZ_Matrices we know that to compute the RGB->XYZ matrix for the input profile
//	we need to find { Sigma_R, Sigma_G, Sigma_B } in order to scale the xyz_R, xyz_G, xyz_B vectors into XYZ_R, XYZ_G and XYZ_B respectively.
//
// We thus obtain the (RGB_in -> XYZ_in) matrix:
//	| XYZ_R_in | = | Sigma_R_in * xyz_R |
//	| XYZ_G_in | = | Sigma_G_in * xyz_G |
//	| XYZ_B_in | = | Sigma_B_in * xyz_B |
//
// In the same maner, we can obtain the (RGB_out -> XYZ_out) matrix:
//	| XYZ_R_out | = | Sigma_R_out * xyz_R |
//	| XYZ_G_out | = | Sigma_G_out * xyz_G |
//	| XYZ_B_out | = | Sigma_B_out * xyz_B |
//
// The matrix we're after is simply the (XYZ_in -> RGB_in) * (RGB_out -> XYZ_out) matrix...
//
void	ColorProfile::ComputeWhiteBalanceXYZMatrix( const Chromaticities& _profileIn, const bfloat2& _whitePointOut, float3x3& _whiteBalanceMatrix ) {
	bfloat3	xyz_R( _profileIn.R.x, _profileIn.R.y, 1.0f - _profileIn.R.x - _profileIn.R.y );
	bfloat3	xyz_G( _profileIn.G.x, _profileIn.G.y, 1.0f - _profileIn.G.x - _profileIn.G.y );
	bfloat3	xyz_B( _profileIn.B.x, _profileIn.B.y, 1.0f - _profileIn.B.x - _profileIn.B.y );

	bfloat3	XYZ_W_in;
	xyY2XYZ( bfloat3( _profileIn.W.x, _profileIn.W.y, 1.0f ), XYZ_W_in );

	bfloat3	XYZ_W_out;
	xyY2XYZ( bfloat3( _whitePointOut.x, _whitePointOut.y, 1.0f ), XYZ_W_out );

	// Build xyz matrix (common to both input and output)
	float3x3	M_xyz;
	M_xyz.r[0].Set( xyz_R.x, xyz_R.y, xyz_R.z );
	M_xyz.r[1].Set( xyz_G.x, xyz_G.y, xyz_G.z );
	M_xyz.r[2].Set( xyz_B.x, xyz_B.y, xyz_B.z );
	M_xyz.Invert();

	// Retrieve the sigmas for in and out white points
	bfloat3	Sum_RGB_in = XYZ_W_in * M_xyz;
	bfloat3	Sum_RGB_out = XYZ_W_out * M_xyz;



// 	// Finally, we can retrieve the RGB->XYZ transform
// 	m_RGB2XYZ.r[0].Set( Sum_RGB.x * xyz_R, 0.0f );
// 	m_RGB2XYZ.r[1].Set( Sum_RGB.y * xyz_G, 0.0f );
// 	m_RGB2XYZ.r[2].Set( Sum_RGB.z * xyz_B, 0.0f );
// 	m_RGB2XYZ.r[3].Set( 0, 0, 0, 1 );
// 
// 	// And the XYZ->RGB transform
// 	m_XYZ2RGB = m_RGB2XYZ;
// 	m_XYZ2RGB.Invert();
// 
// 	float	determinant = Sb Sg Sr Xr Yg Zb - Sb Sg Sr Xg Yr Zb - Sb Sg Sr Xr Yb Zg + 
//  Sb Sg Sr Xb Yr Zg + Sb Sg Sr Xg Yb Zr - Sb Sg Sr Xb Yg Zr;
}

// According to https://en.wikipedia.org/wiki/Black-body_radiation#Planck.27s_law_of_black-body_radiation
// Planck's law states that[30]
// 
//	I(nu,T) = [(2.h.nu^3) / c²] * 1 / [e^(h.nu/(k.T)) - 1]
//
// Where:
//	I(nu,T) is the power (the energy per unit time) radiated per unit area of emitting surface in the normal direction per unit solid angle per unit frequency by a black body at temperature T, also known as spectral radiance;
//	h is the Planck constant;
//	c is the speed of light in a vacuum;
//	k is the Boltzmann constant;
//	nu is the frequency of the electromagnetic radiation;
//	T is the absolute temperature of the body.
//
double	ColorProfile::ComputeBlackBodyRadiationPower( float _blackBodyTemperature, float _wavelength ) {
	const double	h = 6.62607004081e-34;	// Planck's constant (J.s)
	const double	c = 299792458.0;		// Light speed (m/s)
	const double	k = 1.3806485279e-23;	// Boltzmann constant (J/K)

	double	lambda = 1e-9 * _wavelength;	// in m
	double	T = _blackBodyTemperature;		// in K
	double	num = 2.0 * h * c * c;			// in J.m²/s
	double	den = pow( lambda, 5.0 ) * (exp( (h * c) / (lambda * k * T) ) - 1.0);	// in m^5

	double	I = num / den;					// in J/m^3/s/sr/lambda or W/m^3/sr/lambda

	return I;
}

void	ColorProfile::IntegrateSpectralPowerDistributionIntoXYZ( U32 _wavelengthsCount, float _wavelengthStart, float _wavelengthStep, double* _spectralPowerDistibution, bfloat3& _XYZ ) {
	float		wavelengthStart = MAX( CMF_WAVELENGTH_START, _wavelengthStart );

	float		wavelengthEnd = _wavelengthStart + _wavelengthsCount * _wavelengthStep;
				wavelengthEnd = MIN( CMF_WAVELENGTH_END, wavelengthEnd );

	U32			iStart = U32( floorf( (wavelengthStart - _wavelengthStart) / _wavelengthStep ) );
	U32			iEnd = U32( floorf( (wavelengthEnd - _wavelengthStart) / _wavelengthStep ) );
	U32			wavelengthsCount = iEnd - iStart;

	double		X = 0.0;
	double		Y = 0.0;
	double		Z = 0.0;

	const double*	SPD = _spectralPowerDistibution + iStart;

	// Read first CMF & power values
	float	wavelength = _wavelengthStart + iStart * _wavelengthStep;
	U32		CMFindex = U32( floorf( CMF_WAVELENGTH_RCP_STEP * (wavelength - CMF_WAVELENGTH_START) ) );
	double	x = ms_colorMatchingFunctions[3*CMFindex+0];
	double	y = ms_colorMatchingFunctions[3*CMFindex+1];
	double	z = ms_colorMatchingFunctions[3*CMFindex+2];
	double	I = *SPD;
	SPD++;

	double	Dt = _wavelengthStep * 1e-9;	// Integration interval, in meters

	for ( U32 i=iStart+1; i < iEnd; i++, SPD++ ) {
		// Recompute wavelength instead of adding the increment to avoid imprecision on the long run
		wavelength = _wavelengthStart + i * _wavelengthStep;

		// Retrieve the CMF values for the current wavelength
		CMFindex = U32( floorf( CMF_WAVELENGTH_RCP_STEP * (wavelength - CMF_WAVELENGTH_START) ) );
		double	nextx = ms_colorMatchingFunctions[3*CMFindex+0];
		double	nexty = ms_colorMatchingFunctions[3*CMFindex+1];
		double	nextz = ms_colorMatchingFunctions[3*CMFindex+2];

		// Read the spectral power intensity for the current wavelength
		double	nextI = *SPD;

		#if 0
			// Perform simple square integration
			X += Dt * I * x;
			Y += Dt * I * y;
			Z += Dt * I * z;
		#elif 1
			// Perform trapezoidal integration
			X += Dt * 0.5f * (I * x + nextI * nextx);
			Y += Dt * 0.5f * (I * y + nextI * nexty);
			Z += Dt * 0.5f * (I * z + nextI * nextz);
		#else
			// Perform integration assuming linear interpolation of {x,y,z} and I across the interval
			double	Dx = nextx - x;
			double	Dy = nexty - y;
			double	Dz = nextz - z;
			double	DI = nextI - I;
			X += Dt * (x * I + Dt * (0.5 * (x * DI + I * Dx) + Dt * (Dx * DI) / 3.0f));
			Y += Dt * (y * I + Dt * (0.5 * (y * DI + I * Dy) + Dt * (Dy * DI) / 3.0f));
			Z += Dt * (z * I + Dt * (0.5 * (z * DI + I * Dz) + Dt * (Dz * DI) / 3.0f));
		#endif

		// Scroll values
		x = nextx;
		y = nexty;
		z = nextz;
		I = nextI;
	}

	_XYZ.Set( float(X), float(Y), float(Z) );
}

void	ColorProfile::BuildSpectralPowerDistributionForBlackBody( float _blackBodyTemperature, U32 _wavelengthsCount, float _wavelengthStart, float _wavelengthStep, List< double >& _spectralPowerDistribution ) {
	_spectralPowerDistribution.Resize( _wavelengthsCount );
	float	wavelength = _wavelengthStart;
	for ( U32 i=0; i < _wavelengthsCount; i++, wavelength += _wavelengthStep ) {
		double	I = ComputeBlackBodyRadiationPower( _blackBodyTemperature, wavelength );
		_spectralPowerDistribution.Append( I );
	}
}

void	ColorProfile::ComputeWhitePointChromaticities( float _blackBodyTemperature, bfloat2& _whitePointChromaticities ) {
	// 1] Build the SPD for the black body radiator (use 1nm steps)
	List< double >	SPD;
	BuildSpectralPowerDistributionForBlackBody( _blackBodyTemperature, U32( CMF_WAVELENGTH_END - CMF_WAVELENGTH_START ), CMF_WAVELENGTH_START, 1.0f, SPD );

	// 2] Integrate into XYZ
	bfloat3	XYZ;
	IntegrateSpectralPowerDistributionIntoXYZ( SPD.Count(), CMF_WAVELENGTH_START, 1.0f, SPD.Ptr(), XYZ );

	// 3] Convert into xyY
	float	rcpSum = XYZ.x + XYZ.y + XYZ.z;
	rcpSum = rcpSum > 0.0f ? 1.0f / rcpSum : 0.0f;
	_whitePointChromaticities.x = XYZ.x * rcpSum;
	_whitePointChromaticities.y = XYZ.y * rcpSum;
}


//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_sRGB
//
const float4x4	ColorProfile::InternalColorConverter_sRGB::MAT_RGB2XYZ(
	0.4124f, 0.2126f, 0.0193f, 0.0f,
	0.3576f, 0.7152f, 0.1192f, 0.0f,
	0.1805f, 0.0722f, 0.9505f, 0.0f,
	0.0000f, 0.0000f, 0.0000f, 1.0f		// Alpha stays the same
);

const float4x4	ColorProfile::InternalColorConverter_sRGB::MAT_XYZ2RGB(
	3.2406f, -0.9689f,  0.0557f, 0.0f,
	-1.5372f,  1.8758f, -0.2040f, 0.0f,
	-0.4986f,  0.0415f,  1.0570f, 0.0f,
	0.0000f,  0.0000f,  0.0000f, 1.0f	// Alpha stays the same
);

void ColorProfile::InternalColorConverter_sRGB::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * MAT_XYZ2RGB;

	// Gamma correct
	_RGB.x = Linear2sRGB( _RGB.x );
	_RGB.y = Linear2sRGB( _RGB.y );
	_RGB.z = Linear2sRGB( _RGB.z );
}

void ColorProfile::InternalColorConverter_sRGB::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Gamma un-correct
	_XYZ.x = sRGB2Linear( _RGB.x );
	_XYZ.y = sRGB2Linear( _RGB.y );
	_XYZ.z = sRGB2Linear( _RGB.z );
	_XYZ.w = _RGB.w;

	// Transform into XYZ
	_XYZ = _XYZ * MAT_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_sRGB::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Transform into RGB
		*_RGB = *_XYZ * MAT_XYZ2RGB;

		// Gamma correct
		_RGB->x = Linear2sRGB( _RGB->x );
		_RGB->y = Linear2sRGB( _RGB->y );
		_RGB->z = Linear2sRGB( _RGB->z );
	}
}

void ColorProfile::InternalColorConverter_sRGB::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Gamma un-correct
		_XYZ->x = sRGB2Linear( _RGB->x );
		_XYZ->y = sRGB2Linear( _RGB->y );
		_XYZ->z = sRGB2Linear( _RGB->z );
		_XYZ->w = _RGB->w;

		// Transform into XYZ
		*_XYZ = *_XYZ * MAT_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_AdobeRGB_D50
//
const float4x4	ColorProfile::InternalColorConverter_AdobeRGB_D50::MAT_RGB2XYZ(
	0.60974f, 0.31111f, 0.01947f, 0.0f,
	0.20528f, 0.62567f, 0.06087f, 0.0f,
	0.14919f, 0.06322f, 0.74457f, 0.0f,
	0.00000f, 0.00000f, 0.00000f, 1.0f		// Alpha stays the same
);

const float4x4	ColorProfile::InternalColorConverter_AdobeRGB_D50::MAT_XYZ2RGB(
	 1.96253f, -0.97876f,  0.02869f, 0.0f,
	-0.61068f,  1.91615f, -0.14067f, 0.0f,
	-0.34137f,  0.03342f,  1.34926f, 0.0f,
	 0.00000f,  0.00000f,  0.00000f, 1.0f	// Alpha stays the same
);

void ColorProfile::InternalColorConverter_AdobeRGB_D50::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * MAT_XYZ2RGB;

	// Gamma correct
	_RGB.x = powf( _RGB.x, 1.0f / GAMMA_EXPONENT_ADOBE );
	_RGB.y = powf( _RGB.y, 1.0f / GAMMA_EXPONENT_ADOBE );
	_RGB.z = powf( _RGB.z, 1.0f / GAMMA_EXPONENT_ADOBE );
}

void ColorProfile::InternalColorConverter_AdobeRGB_D50::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Gamma un-correct
	_XYZ.x = powf( _RGB.x, GAMMA_EXPONENT_ADOBE );
	_XYZ.y = powf( _RGB.y, GAMMA_EXPONENT_ADOBE );
	_XYZ.z = powf( _RGB.z, GAMMA_EXPONENT_ADOBE );
	_XYZ.w = _RGB.w;

	// Transform into XYZ
	_XYZ = _XYZ * MAT_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_AdobeRGB_D50::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Transform into RGB
		*_RGB = *_XYZ * MAT_XYZ2RGB;

		// Gamma correct
		_RGB->x = powf( _RGB->x, 1.0f / GAMMA_EXPONENT_ADOBE );
		_RGB->y = powf( _RGB->y, 1.0f / GAMMA_EXPONENT_ADOBE );
		_RGB->z = powf( _RGB->z, 1.0f / GAMMA_EXPONENT_ADOBE );
	}
}

void ColorProfile::InternalColorConverter_AdobeRGB_D50::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Gamma un-correct
		_XYZ->x = powf( _RGB->x, GAMMA_EXPONENT_ADOBE );
		_XYZ->y = powf( _RGB->y, GAMMA_EXPONENT_ADOBE );
		_XYZ->z = powf( _RGB->z, GAMMA_EXPONENT_ADOBE );
		_XYZ->w = _RGB->w;

		// Transform into XYZ
		*_XYZ = *_XYZ * MAT_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_AdobeRGB_D65
//
const float4x4	ColorProfile::InternalColorConverter_AdobeRGB_D65::MAT_RGB2XYZ(
	0.57667f, 0.29734f, 0.02703f, 0.0f,
	0.18556f, 0.62736f, 0.07069f, 0.0f,
	0.18823f, 0.07529f, 0.99134f, 0.0f,
	0.00000f, 0.00000f, 0.00000f, 1.0f		// Alpha stays the same
);

const float4x4	ColorProfile::InternalColorConverter_AdobeRGB_D65::MAT_XYZ2RGB(
	 2.04159f, -0.96924f,  0.01344f, 0.0f,
	-0.56501f,  1.87597f, -0.11836f, 0.0f,
	-0.34473f,  0.04156f,  1.01517f, 0.0f,
	 0.00000f,  0.00000f,  0.00000f, 1.0f	// Alpha stays the same
);

void ColorProfile::InternalColorConverter_AdobeRGB_D65::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * MAT_XYZ2RGB;

	// Gamma correct
	_RGB.x = powf( _XYZ.x, 1.0f / GAMMA_EXPONENT_ADOBE );
	_RGB.y = powf( _XYZ.y, 1.0f / GAMMA_EXPONENT_ADOBE );
	_RGB.z = powf( _XYZ.z, 1.0f / GAMMA_EXPONENT_ADOBE );
}

void ColorProfile::InternalColorConverter_AdobeRGB_D65::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Gamma un-correct
	_XYZ.x = powf( _RGB.x, GAMMA_EXPONENT_ADOBE );
	_XYZ.y = powf( _RGB.y, GAMMA_EXPONENT_ADOBE );
	_XYZ.z = powf( _RGB.z, GAMMA_EXPONENT_ADOBE );
	_XYZ.w = _RGB.w;

	// Transform into XYZ
	_XYZ = _XYZ * MAT_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_AdobeRGB_D65::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Transform into RGB
		*_RGB = *_XYZ * MAT_XYZ2RGB;

		// Gamma correct
		_RGB->x = powf( _RGB->x, 1.0f / GAMMA_EXPONENT_ADOBE );
		_RGB->y = powf( _RGB->y, 1.0f / GAMMA_EXPONENT_ADOBE );
		_RGB->z = powf( _RGB->z, 1.0f / GAMMA_EXPONENT_ADOBE );
	}
}

void ColorProfile::InternalColorConverter_AdobeRGB_D65::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Gamma un-correct
		_XYZ->x = powf( _RGB->x, GAMMA_EXPONENT_ADOBE );
		_XYZ->y = powf( _RGB->y, GAMMA_EXPONENT_ADOBE );
		_XYZ->z = powf( _RGB->z, GAMMA_EXPONENT_ADOBE );
		_XYZ->w = _RGB->w;

		// Transform into XYZ
		*_XYZ = *_XYZ * MAT_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_ProPhoto
//
const float4x4	ColorProfile::InternalColorConverter_ProPhoto::MAT_RGB2XYZ(
	0.7977f, 0.2880f, 0.0000f, 0.0f,
	0.1352f, 0.7119f, 0.0000f, 0.0f,
	0.0313f, 0.0001f, 0.8249f, 0.0f,
	0.0000f, 0.0000f, 0.0000f, 1.0f		// Alpha stays the same
);

const float4x4	ColorProfile::InternalColorConverter_ProPhoto::MAT_XYZ2RGB(
	 1.3460f, -0.5446f,  0.0000f, 0.0f,
	-0.2556f,  1.5082f,  0.0000f, 0.0f,
	-0.0511f,  0.0205f,  1.2123f, 0.0f,
	 0.0000f,  0.0000f,  0.0000f, 1.0f	// Alpha stays the same
);

void ColorProfile::InternalColorConverter_ProPhoto::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * MAT_XYZ2RGB;

	// Gamma correct
	_RGB.x = _RGB.x > 0.001953f ? powf( _RGB.x, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB.x;
	_RGB.y = _RGB.y > 0.001953f ? powf( _RGB.y, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB.y;
	_RGB.z = _RGB.z > 0.001953f ? powf( _RGB.z, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB.z;
}

void ColorProfile::InternalColorConverter_ProPhoto::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Gamma un-correct
	_XYZ.x = _RGB.x > 0.031248f ? powf( _RGB.x, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.x / 16.0f;
	_XYZ.y = _RGB.y > 0.031248f ? powf( _RGB.y, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.y / 16.0f;
	_XYZ.z = _RGB.z > 0.031248f ? powf( _RGB.z, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.z / 16.0f;
	_XYZ.w = _RGB.w;

	// Transform into XYZ
	_XYZ = _XYZ * MAT_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_ProPhoto::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Transform into RGB
		*_RGB = *_XYZ * MAT_XYZ2RGB;

		// Gamma correct
		_RGB->x = _RGB->x > 0.001953f ? powf( _RGB->x, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB->x;
		_RGB->y = _RGB->y > 0.001953f ? powf( _RGB->y, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB->y;
		_RGB->z = _RGB->z > 0.001953f ? powf( _RGB->z, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB->z;
	}
}

void ColorProfile::InternalColorConverter_ProPhoto::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Gamma un-correct
		_XYZ->x = _RGB->x > 0.031248f ? powf( _RGB->x, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB->x / 16.0f;
		_XYZ->y = _RGB->y > 0.031248f ? powf( _RGB->y, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB->y / 16.0f;
		_XYZ->z = _RGB->z > 0.031248f ? powf( _RGB->z, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB->z / 16.0f;
		_XYZ->w = _RGB->w;

		// Transform into XYZ
		*_XYZ = *_XYZ * MAT_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_Radiance
//
const float4x4	ColorProfile::InternalColorConverter_Radiance::MAT_RGB2XYZ(
	0.5141447f, 0.2651059f, 0.0241005f, 0.0f,
	0.3238845f, 0.6701059f, 0.1228527f, 0.0f,
	0.1619709f, 0.0647883f, 0.8530467f, 0.0f,
	0.0000000f, 0.0000000f, 0.0000000f, 1.0f		// Alpha stays the same
);

const float4x4	ColorProfile::InternalColorConverter_Radiance::MAT_XYZ2RGB(
	 2.5653124f, -1.02210832f,  0.07472437f, 0.0f,
	-1.1668493f,  1.97828662f, -0.25193953f, 0.0f,
	-0.3984632f,  0.04382159f,  1.17721522f, 0.0f,
	 0.0000000f,  0.00000000f,  0.00000000f, 1.0f	// Alpha stays the same
);

void ColorProfile::InternalColorConverter_Radiance::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * MAT_XYZ2RGB;
}

void ColorProfile::InternalColorConverter_Radiance::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Transform into XYZ
	_XYZ = _RGB * MAT_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_Radiance::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		*_RGB = *_XYZ * MAT_XYZ2RGB;
	}
}

void ColorProfile::InternalColorConverter_Radiance::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		*_XYZ = *_RGB * MAT_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_Generic_NoGamma
//
void ColorProfile::InternalColorConverter_Generic_NoGamma::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * m_XYZ2RGB;
}

void ColorProfile::InternalColorConverter_Generic_NoGamma::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Transform into XYZ
	_XYZ = _RGB * m_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_Generic_NoGamma::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		*_RGB = *_XYZ * m_XYZ2RGB;
	}
}

void ColorProfile::InternalColorConverter_Generic_NoGamma::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		*_XYZ = *_RGB * m_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_Generic_StandardGamma
//
void ColorProfile::InternalColorConverter_Generic_StandardGamma::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * m_XYZ2RGB;

	// Gamma correct
	_RGB.x = powf( _RGB.x, m_InvGamma );
	_RGB.y = powf( _RGB.y, m_InvGamma );
	_RGB.z = powf( _RGB.z, m_InvGamma );
}

void ColorProfile::InternalColorConverter_Generic_StandardGamma::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Gamma un-correct
	_XYZ.x = powf( _RGB.x, m_Gamma );
	_XYZ.y = powf( _RGB.y, m_Gamma );
	_XYZ.z = powf( _RGB.z, m_Gamma );
	_XYZ.w = _RGB.w;

	// Transform into XYZ
	_XYZ = _XYZ * m_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_Generic_StandardGamma::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Transform into RGB
		*_RGB = *_XYZ * m_XYZ2RGB;

		// Gamma correct
		_RGB->x = powf( _RGB->x, m_InvGamma );
		_RGB->y = powf( _RGB->y, m_InvGamma );
		_RGB->z = powf( _RGB->z, m_InvGamma );
	}
}

void ColorProfile::InternalColorConverter_Generic_StandardGamma::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Gamma un-correct
		_XYZ->x = powf( _RGB->x, m_Gamma );
		_XYZ->y = powf( _RGB->y, m_Gamma );
		_XYZ->z = powf( _RGB->z, m_Gamma );
		_XYZ->w = _RGB->w;

		// Transform into XYZ
		*_XYZ = *_XYZ * m_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_Generic_sRGBGamma
//
void ColorProfile::InternalColorConverter_Generic_sRGBGamma::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * m_XYZ2RGB;

	// Gamma correct
	_RGB.x = _RGB.x > 0.0031308f ? 1.055f * powf( _RGB.x, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _RGB.x;
	_RGB.y = _RGB.y > 0.0031308f ? 1.055f * powf( _RGB.y, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _RGB.y;
	_RGB.z = _RGB.z > 0.0031308f ? 1.055f * powf( _RGB.z, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _RGB.z;
}

void ColorProfile::InternalColorConverter_Generic_sRGBGamma::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Gamma un-correct
	_XYZ.x = _RGB.x < 0.04045f ? _RGB.x / 12.92f : powf( (_RGB.x + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
	_XYZ.y = _RGB.y < 0.04045f ? _RGB.y / 12.92f : powf( (_RGB.y + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
	_XYZ.z = _RGB.z < 0.04045f ? _RGB.z / 12.92f : powf( (_RGB.z + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
	_XYZ.w = _RGB.w;

	// Transform into XYZ
	_XYZ = _XYZ * m_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_Generic_sRGBGamma::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Transform into RGB
		*_RGB = *_XYZ * m_XYZ2RGB;

		// Gamma correct
		_RGB->x = _RGB->x > 0.0031308f ? 1.055f * powf( _RGB->x, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _RGB->x;
		_RGB->y = _RGB->y > 0.0031308f ? 1.055f * powf( _RGB->y, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _RGB->y;
		_RGB->z = _RGB->z > 0.0031308f ? 1.055f * powf( _RGB->z, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _RGB->z;
	}
}

void ColorProfile::InternalColorConverter_Generic_sRGBGamma::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Gamma un-correct
		_XYZ->x = _RGB->x < 0.04045f ? _RGB->x / 12.92f : powf( (_RGB->x + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
		_XYZ->y = _RGB->y < 0.04045f ? _RGB->y / 12.92f : powf( (_RGB->y + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
		_XYZ->z = _RGB->z < 0.04045f ? _RGB->z / 12.92f : powf( (_RGB->z + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
		_XYZ->w = _RGB->w;

		// Transform into XYZ
		*_XYZ = *_XYZ * m_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_Generic_ProPhoto
//
void ColorProfile::InternalColorConverter_Generic_ProPhoto::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * m_XYZ2RGB;

	// Gamma correct
	_RGB.x = _RGB.x > 0.001953f ? powf( _RGB.x, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB.x;
	_RGB.y = _RGB.y > 0.001953f ? powf( _RGB.y, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB.y;
	_RGB.z = _RGB.z > 0.001953f ? powf( _RGB.z, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB.z;
}

void ColorProfile::InternalColorConverter_Generic_ProPhoto::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Gamma un-correct
	_XYZ.x = _RGB.x > 0.031248f ? powf( _RGB.x, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.x / 16.0f;
	_XYZ.y = _RGB.y > 0.031248f ? powf( _RGB.y, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.y / 16.0f;
	_XYZ.z = _RGB.z > 0.031248f ? powf( _RGB.z, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.z / 16.0f;
	_XYZ.w = _RGB.w;

	// Transform into XYZ
	_XYZ = _XYZ * m_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_Generic_ProPhoto::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Transform into RGB
		*_RGB = *_XYZ * m_XYZ2RGB;

		// Gamma correct
		_RGB->x = _RGB->x > 0.001953f ? powf( _RGB->x, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB->x;
		_RGB->y = _RGB->y > 0.001953f ? powf( _RGB->y, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB->y;
		_RGB->z = _RGB->z > 0.001953f ? powf( _RGB->z, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB->z;
	}
}

void ColorProfile::InternalColorConverter_Generic_ProPhoto::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Gamma un-correct
		_XYZ->x = _RGB->x > 0.031248f ? powf( _RGB->x, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB->x / 16.0f;
		_XYZ->y = _RGB->y > 0.031248f ? powf( _RGB->y, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB->y / 16.0f;
		_XYZ->z = _RGB->z > 0.031248f ? powf( _RGB->z, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB->z / 16.0f;
		_XYZ->w = _RGB->w;

		// Transform into XYZ
		*_XYZ = *_XYZ * m_RGB2XYZ;
	}
}
