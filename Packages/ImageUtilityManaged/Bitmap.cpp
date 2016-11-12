#include "stdafx.h"

#include "Bitmap.h"

using namespace ImageUtility;

void	CopyHDRParms( Bitmap::HDRParms% _parmsIn, ImageUtilityLib::Bitmap::HDRParms& _parmsOut ) {
	_parmsOut._inputBitsPerComponent = _parmsIn._inputBitsPerComponent;
	_parmsOut._luminanceFactor = _parmsIn._luminanceFactor;
	_parmsOut._curveSmoothnessConstraint = _parmsIn._curveSmoothnessConstraint;
	_parmsOut._quality = _parmsIn._quality;
}

void	Bitmap::LDR2HDR( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, HDRParms% _parms ) {
	if ( _images == nullptr )
		throw gcnew Exception( "Invalid images array!" );
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
	CopyHDRParms( _parms, parms );

	m_nativeObject->LDR2HDR( imagesCount, images, imageShutterSpeedsPtr, parms );

	System::Runtime::InteropServices::Marshal::FreeHGlobal( imagesPtr );
}

void	Bitmap::LDR2HDR( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, System::Collections::Generic::List< float3 >^ _responseCurve, float _luminanceFactor ) {
	if ( _images == nullptr )
		throw gcnew Exception( "Invalid images array!" );
	if ( _imageShutterSpeeds == nullptr )
		throw gcnew Exception( "Invalid image shutter speeds array!" );
	if ( _responseCurve == nullptr || _responseCurve->Count == 0 )
		throw gcnew Exception( "Invalid or empty response curve list!" );
	if ( _images->Length != _imageShutterSpeeds->Length )
		throw gcnew Exception( "Image array and shutter speeds array sizes mismatch!" );

	int	imagesCount = _images->Length;

	IntPtr								imagesPtr = System::Runtime::InteropServices::Marshal::AllocHGlobal( imagesCount*sizeof(void*) );
	const ImageUtilityLib::ImageFile**	images = (const ImageUtilityLib::ImageFile**) imagesPtr.ToPointer();
	for ( int imageIndex=0; imageIndex < imagesCount; imageIndex++ )
		images[imageIndex] = _images[imageIndex]->m_nativeObject;

	pin_ptr<float>	imageShutterSpeedsPtr = &_imageShutterSpeeds[0];

	// Copy to native list
	BaseLib::List< bfloat3 >	responseCurve( _responseCurve->Count );
	for ( int i=0; i < _responseCurve->Count; i++ ) {
		float3%	source = _responseCurve[i];
		responseCurve.Append( bfloat3( source.x, source.y, source.z ) );
	}

	m_nativeObject->LDR2HDR( imagesCount, images, imageShutterSpeedsPtr, responseCurve, _luminanceFactor );

	System::Runtime::InteropServices::Marshal::FreeHGlobal( imagesPtr );
}

void	Bitmap::ComputeCameraResponseCurve( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, HDRParms% _parms, System::Collections::Generic::List< float3 >^ _responseCurve ) {
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

	ImageUtilityLib::Bitmap::HDRParms	parms;
	CopyHDRParms( _parms, parms );

	BaseLib::List< bfloat3 >	responseCurve;
	ImageUtilityLib::Bitmap::ComputeCameraResponseCurve( imagesCount, images, imageShutterSpeedsPtr, parms, responseCurve );

	// Copy result
	_responseCurve->Clear();
	for ( int i=0; i < responseCurve.Count(); i++ ) {
		const bfloat3&	source = responseCurve[i];
		_responseCurve->Add( float3( source.x, source.y, source.z ) );
	}

	System::Runtime::InteropServices::Marshal::FreeHGlobal( imagesPtr );
}
