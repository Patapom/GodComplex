#include "stdafx.h"

#include "ColorProfile.h"

using namespace ImageUtility;
using namespace SharpMath;

SharpMath::float4x4^	Native2ManagedFloat4x4( const ::float4x4& _native ) {
	const ::float4x4&		S = _native;
	SharpMath::float4x4^	R = gcnew SharpMath::float4x4(	float4( S.r[0].x, S.r[0].y, S.r[0].z, S.r[0].w ),
															float4( S.r[1].x, S.r[1].y, S.r[1].z, S.r[1].w ),
															float4( S.r[2].x, S.r[2].y, S.r[2].z, S.r[2].w ),
															float4( S.r[3].x, S.r[3].y, S.r[3].z, S.r[3].w )
														);
	return R;
}

SharpMath::float4x4^	ColorProfile::MatrixRGB2XYZ::get() {
	return Native2ManagedFloat4x4( m_nativeObject->GetMatrixRGB2XYZ() );
}

SharpMath::float4x4^	ColorProfile::MatrixXYZ2RGB::get() {
	return Native2ManagedFloat4x4( m_nativeObject->GetMatrixXYZ2RGB() );
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
	m_nativeObject->XYZ2RGB( (bfloat4*) nativeBufferSrc.ToPointer(), (bfloat4*) nativeBufferDst.ToPointer(), _XYZ->Length );

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
	m_nativeObject->RGB2XYZ( (bfloat4*) nativeBufferSrc.ToPointer(), (bfloat4*) nativeBufferDst.ToPointer(), _RGB->Length );

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
