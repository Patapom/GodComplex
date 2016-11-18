#include "stdafx.h"

#include "ColorProfile.h"

using namespace ImageUtility;
//using namespace SharpMath;

ColorProfile::Chromaticities^	ColorProfile::Chromaticities::Empty::get()			{ return gcnew Chromaticities( float2(), float2(), float2(), float2() ); }
ColorProfile::Chromaticities^	ColorProfile::Chromaticities::sRGB::get()			{ return gcnew Chromaticities(	float2( ImageUtilityLib::ColorProfile::Chromaticities::sRGB.R.x, ImageUtilityLib::ColorProfile::Chromaticities::sRGB.R.y ),
																													float2( ImageUtilityLib::ColorProfile::Chromaticities::sRGB.G.x, ImageUtilityLib::ColorProfile::Chromaticities::sRGB.G.y ),
																													float2( ImageUtilityLib::ColorProfile::Chromaticities::sRGB.B.x, ImageUtilityLib::ColorProfile::Chromaticities::sRGB.B.y ),
																													ILLUMINANT_D65 ); }
ColorProfile::Chromaticities^	ColorProfile::Chromaticities::AdobeRGB_D50::get()	{ return gcnew Chromaticities(	float2( ImageUtilityLib::ColorProfile::Chromaticities::AdobeRGB_D50.R.x, ImageUtilityLib::ColorProfile::Chromaticities::AdobeRGB_D50.R.y ),
																													float2( ImageUtilityLib::ColorProfile::Chromaticities::AdobeRGB_D50.G.x, ImageUtilityLib::ColorProfile::Chromaticities::AdobeRGB_D50.G.y ),
																													float2( ImageUtilityLib::ColorProfile::Chromaticities::AdobeRGB_D50.B.x, ImageUtilityLib::ColorProfile::Chromaticities::AdobeRGB_D50.B.y ),
																													ILLUMINANT_D50 ); }
ColorProfile::Chromaticities^	ColorProfile::Chromaticities::AdobeRGB_D65::get()	{ return gcnew Chromaticities(	float2( ImageUtilityLib::ColorProfile::Chromaticities::AdobeRGB_D65.R.x, ImageUtilityLib::ColorProfile::Chromaticities::AdobeRGB_D65.R.y ),
																													float2( ImageUtilityLib::ColorProfile::Chromaticities::AdobeRGB_D65.G.x, ImageUtilityLib::ColorProfile::Chromaticities::AdobeRGB_D65.G.y ),
																													float2( ImageUtilityLib::ColorProfile::Chromaticities::AdobeRGB_D65.B.x, ImageUtilityLib::ColorProfile::Chromaticities::AdobeRGB_D65.B.y ),
																													ILLUMINANT_D65 ); }
ColorProfile::Chromaticities^	ColorProfile::Chromaticities::ProPhoto::get()		{ return gcnew Chromaticities(	float2( ImageUtilityLib::ColorProfile::Chromaticities::ProPhoto.R.x, ImageUtilityLib::ColorProfile::Chromaticities::ProPhoto.R.y ),
																													float2( ImageUtilityLib::ColorProfile::Chromaticities::ProPhoto.G.x, ImageUtilityLib::ColorProfile::Chromaticities::ProPhoto.G.y ),
																													float2( ImageUtilityLib::ColorProfile::Chromaticities::ProPhoto.B.x, ImageUtilityLib::ColorProfile::Chromaticities::ProPhoto.B.y ),
																													ILLUMINANT_D50 ); }
ColorProfile::Chromaticities^	ColorProfile::Chromaticities::Radiance::get()		{ return gcnew Chromaticities(	float2( ImageUtilityLib::ColorProfile::Chromaticities::Radiance.R.x, ImageUtilityLib::ColorProfile::Chromaticities::Radiance.R.y ),
																													float2( ImageUtilityLib::ColorProfile::Chromaticities::Radiance.G.x, ImageUtilityLib::ColorProfile::Chromaticities::Radiance.G.y ),
																													float2( ImageUtilityLib::ColorProfile::Chromaticities::Radiance.B.x, ImageUtilityLib::ColorProfile::Chromaticities::Radiance.B.y ),
																													ILLUMINANT_E ); }

SharpMath::float4x4^	Native2ManagedFloat4x4( const ::float4x4& _native ) {
	const ::float4x4&		S = _native;
	SharpMath::float4x4^	R = gcnew SharpMath::float4x4(	float4( S.r[0].x, S.r[0].y, S.r[0].z, S.r[0].w ),
															float4( S.r[1].x, S.r[1].y, S.r[1].z, S.r[1].w ),
															float4( S.r[2].x, S.r[2].y, S.r[2].z, S.r[2].w ),
															float4( S.r[3].x, S.r[3].y, S.r[3].z, S.r[3].w )
														);
	return R;
}

SharpMath::float3x3^	Native2ManagedFloat3x3( const ::float3x3& _native ) {
	const ::float3x3&		S = _native;
	SharpMath::float3x3^	R = gcnew SharpMath::float3x3(	float3( S.r[0].x, S.r[0].y, S.r[0].z ),
															float3( S.r[1].x, S.r[1].y, S.r[1].z ),
															float3( S.r[2].x, S.r[2].y, S.r[2].z )
														);
	return R;
}

SharpMath::float3x3^	ColorProfile::MatrixRGB2XYZ::get() {
	return Native2ManagedFloat3x3( m_nativeObject->GetMatrixRGB2XYZ() );
}

SharpMath::float3x3^	ColorProfile::MatrixXYZ2RGB::get() {
	return Native2ManagedFloat3x3( m_nativeObject->GetMatrixXYZ2RGB() );
}

cli::array<float4>^	ColorProfile::XYZ2RGB( cli::array<float4>^ _XYZ ) {
	// Copy to unmanaged array
	IntPtr		nativeBufferSrc = System::Runtime::InteropServices::Marshal::AllocHGlobal( _XYZ->Length * sizeof(bfloat4) );
	bfloat4*	target = (bfloat4*) nativeBufferSrc.ToPointer();
	for ( int i=0; i < _XYZ->Length; i++, target++ ) {
		target->Set( _XYZ[i].x, _XYZ[i].y, _XYZ[i].z, _XYZ[i].w );
	}

	// Call native method
	IntPtr	nativeBufferDst = System::Runtime::InteropServices::Marshal::AllocHGlobal( _XYZ->Length * sizeof(bfloat4) );
	m_nativeObject->XYZ2RGB( (bfloat3*) nativeBufferSrc.ToPointer(), (bfloat3*) nativeBufferDst.ToPointer(), _XYZ->Length, sizeof(bfloat4) );

	// Copy result back to managed array
	cli::array<float4>^	RGB = gcnew cli::array<float4>( _XYZ->Length );
	const bfloat4*	source = (bfloat4*) nativeBufferDst.ToPointer();
	for ( int i=0; i < _XYZ->Length; i++, source++ ) {
		RGB[i].Set( source->x, source->y, source->z, source->w );
	}

	System::Runtime::InteropServices::Marshal::FreeHGlobal( nativeBufferSrc );
	System::Runtime::InteropServices::Marshal::FreeHGlobal( nativeBufferDst );

	return RGB;
}

cli::array<float4>^	ColorProfile::RGB2XYZ( cli::array<float4>^ _RGB ) {
	// Copy to unmanaged array
	IntPtr		nativeBufferSrc = System::Runtime::InteropServices::Marshal::AllocHGlobal( _RGB->Length * sizeof(bfloat4) );
	bfloat4*	target = (bfloat4*) nativeBufferSrc.ToPointer();
	for ( int i=0; i < _RGB->Length; i++, target++ ) {
		target->Set( _RGB[i].x, _RGB[i].y, _RGB[i].z, _RGB[i].w );
	}

	// Call native method
	IntPtr	nativeBufferDst = System::Runtime::InteropServices::Marshal::AllocHGlobal( _RGB->Length * sizeof(bfloat4) );
	m_nativeObject->RGB2XYZ( (bfloat3*) nativeBufferSrc.ToPointer(), (bfloat3*) nativeBufferDst.ToPointer(), _RGB->Length, sizeof(bfloat4) );

	// Copy result back to managed array
	cli::array<float4>^	XYZ = gcnew cli::array<float4>( _RGB->Length );
	const bfloat4*	source = (bfloat4*) nativeBufferDst.ToPointer();
	for ( int i=0; i < _RGB->Length; i++, source++ ) {
		XYZ[i].Set( source->x, source->y, source->z, source->w );
	}

	System::Runtime::InteropServices::Marshal::FreeHGlobal( nativeBufferSrc );
	System::Runtime::InteropServices::Marshal::FreeHGlobal( nativeBufferDst );

	return XYZ;
}

SharpMath::float3x3^	ColorProfile::ComputeWhiteBalanceXYZMatrix( Chromaticities^ _profileIn, SharpMath::float2^ _whitePointOut ) {
	::float3x3	result;
	ImageUtilityLib::ColorProfile::ComputeWhiteBalanceXYZMatrix( *_profileIn->m_nativeObject, bfloat2( _whitePointOut->x, _whitePointOut->y ), result );

	return gcnew SharpMath::float3x3(	result.r[0].x, result.r[0].y, result.r[0].z,
										result.r[1].x, result.r[1].y, result.r[1].z,
										result.r[2].x, result.r[2].y, result.r[2].z );
}

SharpMath::float3x3^	ColorProfile::ComputeWhiteBalanceXYZMatrix( SharpMath::float2^ _whitePointIn, Chromaticities^ _profileOut ) {
	::float3x3	result;
	ImageUtilityLib::ColorProfile::ComputeWhiteBalanceXYZMatrix( bfloat2( _whitePointIn->x, _whitePointIn->y ), *_profileOut->m_nativeObject, result );

	return gcnew SharpMath::float3x3(	result.r[0].x, result.r[0].y, result.r[0].z,
										result.r[1].x, result.r[1].y, result.r[1].z,
										result.r[2].x, result.r[2].y, result.r[2].z );
}

SharpMath::float3x3^	ColorProfile::ComputeWhiteBalanceXYZMatrix( SharpMath::float2^ _xyR, float2^ _xyG, SharpMath::float2^ _xyB, SharpMath::float2^ _whitePointIn, SharpMath::float2^ _whitePointOut ) {
	::float3x3	result;
	ImageUtilityLib::ColorProfile::ComputeWhiteBalanceXYZMatrix( bfloat2( _xyR->x, _xyR->y ), bfloat2( _xyG->x, _xyG->y ), bfloat2( _xyB->x, _xyB->y ), bfloat2( _whitePointIn->x, _whitePointIn->y ), bfloat2( _whitePointOut->x, _whitePointOut->y ), result );

	return gcnew SharpMath::float3x3(	result.r[0].x, result.r[0].y, result.r[0].z,
										result.r[1].x, result.r[1].y, result.r[1].z,
										result.r[2].x, result.r[2].y, result.r[2].z );
}


void	ColorProfile::IntegrateSpectralPowerDistributionIntoXYZ( float _wavelengthStart, float _wavelengthStep, cli::array< double >^ _spectralPowerDistibution, float3% _XYZ ) {
	bfloat3	XYZ;
	pin_ptr<double>	SPD = &_spectralPowerDistibution[0];

	ImageUtilityLib::ColorProfile::IntegrateSpectralPowerDistributionIntoXYZ( _spectralPowerDistibution->Length, _wavelengthStart, _wavelengthStep, SPD, XYZ );
	_XYZ.x = XYZ.x;
	_XYZ.y = XYZ.y;
	_XYZ.z = XYZ.z;
}
void	ColorProfile::BuildSpectralPowerDistributionForBlackBody( float _blackBodyTemperature, UInt32 _wavelengthsCount, float _wavelengthStart, float _wavelengthStep, System::Collections::Generic::List< double >^ _spectralPowerDistribution ) {
	BaseLib::List< double >	SPD;
	ImageUtilityLib::ColorProfile::BuildSpectralPowerDistributionForBlackBody( _blackBodyTemperature, _wavelengthsCount, _wavelengthStart, _wavelengthStep, SPD );

	_spectralPowerDistribution->Clear();
	for ( int i=0; i < SPD.Count(); i++ ) {
		_spectralPowerDistribution->Add( SPD[i] );
	}
}
