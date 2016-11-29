#include "stdafx.h"

#include "Bitmap.h"

using namespace ImageUtility;

void	CopyHDRParms( Bitmap::HDRParms% _parmsIn, ImageUtilityLib::Bitmap::HDRParms& _parmsOut ) {
	_parmsOut._inputBitsPerComponent = _parmsIn._inputBitsPerComponent;
	_parmsOut._luminanceFactor = _parmsIn._luminanceFactor;
	_parmsOut._curveSmoothnessConstraint = _parmsIn._curveSmoothnessConstraint;
	_parmsOut._quality = _parmsIn._quality;
	_parmsOut._luminanceOnly = _parmsIn._luminanceOnly;
	_parmsOut._responseCurveFilterType = ImageUtilityLib::Bitmap::FILTER_TYPE( _parmsIn._responseCurveFilterType );
}

void	Bitmap::LDR2HDR( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, HDRParms^ _parms ) {
	if ( _images == nullptr )
		throw gcnew Exception( "Invalid images array!" );
	if ( _parms == nullptr )
		throw gcnew Exception( "Invalid parms for conversion!" );
	if ( _imageShutterSpeeds == nullptr )
		throw gcnew Exception( "Invalid image shutter speeds array!" );
	if ( _images->Length != _imageShutterSpeeds->Length )
		throw gcnew Exception( "Image array and shutter speeds array sizes mismatch!" );

	int	imagesCount = _images->Length;

	IntPtr								imagesPtr = System::Runtime::InteropServices::Marshal::AllocHGlobal( imagesCount*sizeof(void*) );
	const ImageUtilityLib::ImageFile**	images = (const ImageUtilityLib::ImageFile**) imagesPtr.ToPointer();
//	float*								imagesPtr = (ImageUtilityLib::ImageFile*) System::Runtime::InteropServices::Marshal::AllocHGlobal( imagesCount*sizeof(void*) );
	for ( int imageIndex=0; imageIndex < imagesCount; imageIndex++ )
		images[imageIndex] = _images[imageIndex]->m_nativeObject;

	pin_ptr<float>	imageShutterSpeedsPtr = &_imageShutterSpeeds[0];

	ImageUtilityLib::Bitmap::HDRParms	parms;
	CopyHDRParms( *_parms, parms );

	m_nativeObject->LDR2HDR( imagesCount, images, imageShutterSpeedsPtr, parms );

	System::Runtime::InteropServices::Marshal::FreeHGlobal( imagesPtr );
}

void	Bitmap::LDR2HDR( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, System::Collections::Generic::List< float3 >^ _responseCurve, float _luminanceFactor ) {
	if ( _responseCurve == nullptr || _responseCurve->Count == 0 )
		throw gcnew Exception( "Invalid or empty response curve list!" );

	// Copy to native list
	BaseLib::List< bfloat3 >	responseCurve( _responseCurve->Count );
	for ( int i=0; i < _responseCurve->Count; i++ ) {
		float3%	source = _responseCurve[i];
		responseCurve.Append( bfloat3( source.x, source.y, source.z ) );
	}

	LDR2HDR_internal( _images, _imageShutterSpeeds, responseCurve, false, _luminanceFactor );
}
void	Bitmap::LDR2HDR( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, System::Collections::Generic::List< float >^ _responseCurveLuminance, float _luminanceFactor ) {
	if ( _responseCurveLuminance == nullptr || _responseCurveLuminance->Count == 0 )
		throw gcnew Exception( "Invalid or empty response curve list!" );

	// Copy to native list
	BaseLib::List< bfloat3 >	responseCurve( _responseCurveLuminance->Count );
	for ( int i=0; i < _responseCurveLuminance->Count; i++ ) {
		float	source = _responseCurveLuminance[i];
		responseCurve.Append( bfloat3( source, source, source ) );
	}

	LDR2HDR_internal( _images, _imageShutterSpeeds, responseCurve, true, _luminanceFactor );
}
void	Bitmap::LDR2HDR_internal( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, const BaseLib::List< bfloat3 >& _responseCurve, bool _luminanceOnly, float _luminanceFactor ) {
	if ( _images == nullptr )
		throw gcnew Exception( "Invalid images array!" );
	if ( _imageShutterSpeeds == nullptr )
		throw gcnew Exception( "Invalid image shutter speeds array!" );
	if ( _responseCurve.Count() == 0 )
		throw gcnew Exception( "Invalid or empty response curve list!" );
	if ( _images->Length != _imageShutterSpeeds->Length )
		throw gcnew Exception( "Image array and shutter speeds array sizes mismatch!" );

	int	imagesCount = _images->Length;

	IntPtr								imagesPtr = System::Runtime::InteropServices::Marshal::AllocHGlobal( imagesCount*sizeof(void*) );
	const ImageUtilityLib::ImageFile**	images = (const ImageUtilityLib::ImageFile**) imagesPtr.ToPointer();
	for ( int imageIndex=0; imageIndex < imagesCount; imageIndex++ )
		images[imageIndex] = _images[imageIndex]->m_nativeObject;

	pin_ptr<float>	imageShutterSpeedsPtr = &_imageShutterSpeeds[0];

	m_nativeObject->LDR2HDR( imagesCount, images, imageShutterSpeedsPtr, _responseCurve, _luminanceOnly, _luminanceFactor );

	System::Runtime::InteropServices::Marshal::FreeHGlobal( imagesPtr );
}

void	Bitmap::ComputeCameraResponseCurve( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, UInt32 _inputBitsPerComponent, float _curveSmoothnessConstraint, float _quality, System::Collections::Generic::List< float3 >^ _responseCurve ) {

 	BaseLib::List< bfloat3 >	responseCurve;
	ComputeCameraResponseCurve_internal( _images, _imageShutterSpeeds, _inputBitsPerComponent, _curveSmoothnessConstraint, _quality, false, responseCurve );

	// Copy result
	_responseCurve->Clear();
	for ( U32 i=0; i < responseCurve.Count(); i++ ) {
		const bfloat3&	source = responseCurve[i];
		_responseCurve->Add( float3( source.x, source.y, source.z ) );
	}
}
void	Bitmap::ComputeCameraResponseCurve( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, UInt32 _inputBitsPerComponent, float _curveSmoothnessConstraint, float _quality, System::Collections::Generic::List< float >^ _responseCurveLuminance ) {

 	BaseLib::List< bfloat3 >	responseCurve;
	ComputeCameraResponseCurve_internal( _images, _imageShutterSpeeds, _inputBitsPerComponent, _curveSmoothnessConstraint, _quality, true, responseCurve );

	// Copy result
	_responseCurveLuminance->Clear();
	for ( U32 i=0; i < responseCurve.Count(); i++ ) {
		const bfloat3&	source = responseCurve[i];
		_responseCurveLuminance->Add( source.x );
	}
}

void	Bitmap::ComputeCameraResponseCurve_internal( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, UInt32 _inputBitsPerComponent, float _curveSmoothnessConstraint, float _quality, bool _luminanceOnly, BaseLib::List< bfloat3 >& _responseCurve ) {
	if ( _images == nullptr )
		throw gcnew Exception( "Invalid images array!" );
	if ( _imageShutterSpeeds == nullptr )
		throw gcnew Exception( "Invalid image shutter speeds array!" );
	if ( _images->Length != _imageShutterSpeeds->Length )
		throw gcnew Exception( "Image array and shutter speeds array sizes mismatch!" );

	int	imagesCount = _images->Length;

	IntPtr								imagesPtr = System::Runtime::InteropServices::Marshal::AllocHGlobal( imagesCount*sizeof(void*) );
	const ImageUtilityLib::ImageFile**	images = (const ImageUtilityLib::ImageFile**) imagesPtr.ToPointer();
	for ( int imageIndex=0; imageIndex < imagesCount; imageIndex++ )
		images[imageIndex] = _images[imageIndex]->m_nativeObject;

	pin_ptr<float>	imageShutterSpeedsPtr = &_imageShutterSpeeds[0];

	ImageUtilityLib::Bitmap::ComputeCameraResponseCurve( imagesCount, images, imageShutterSpeedsPtr, _inputBitsPerComponent, _curveSmoothnessConstraint, _quality, _luminanceOnly, _responseCurve );

	System::Runtime::InteropServices::Marshal::FreeHGlobal( imagesPtr );
}

void	Bitmap::FilterCameraResponseCurve( System::Collections::Generic::List< float3 >^ _rawResponseCurve, System::Collections::Generic::List< float3 >^ _filteredResponseCurve, FILTER_TYPE _filterType ) {
	BaseLib::List< bfloat3 >	rawResponseCurve( _rawResponseCurve->Count );
	for ( int i=0; i < _rawResponseCurve->Count; i++ )
		rawResponseCurve.Append( bfloat3( _rawResponseCurve[i].x, _rawResponseCurve[i].y, _rawResponseCurve[i].z ) );

	BaseLib::List< bfloat3 >	filteredResponseCurve;
	ImageUtilityLib::Bitmap::FilterCameraResponseCurve( rawResponseCurve, filteredResponseCurve, 3, ImageUtilityLib::Bitmap::FILTER_TYPE( _filterType ) );

	_filteredResponseCurve->Clear();
	for ( U32 i=0; i < filteredResponseCurve.Count(); i++ )
		_filteredResponseCurve->Add( float3( filteredResponseCurve[i].x, filteredResponseCurve[i].y, filteredResponseCurve[i].z ) );
}
void	Bitmap::FilterCameraResponseCurve( System::Collections::Generic::List< float >^ _rawResponseCurve, System::Collections::Generic::List< float >^ _filteredResponseCurve, FILTER_TYPE _filterType ) {
	BaseLib::List< bfloat3 >	rawResponseCurve( _rawResponseCurve->Count );
	for ( int i=0; i < _rawResponseCurve->Count; i++ )
		rawResponseCurve.Append( bfloat3( _rawResponseCurve[i], _rawResponseCurve[i], _rawResponseCurve[i] ) );

	BaseLib::List< bfloat3 >	filteredResponseCurve;
	ImageUtilityLib::Bitmap::FilterCameraResponseCurve( rawResponseCurve, filteredResponseCurve, 1, ImageUtilityLib::Bitmap::FILTER_TYPE( _filterType ) );

	_filteredResponseCurve->Clear();
	for ( U32 i=0; i < filteredResponseCurve.Count(); i++ )
		_filteredResponseCurve->Add( filteredResponseCurve[i].x );
}
