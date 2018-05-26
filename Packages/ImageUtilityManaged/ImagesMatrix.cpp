// This is the main DLL file.

#include "stdafx.h"

#include "ImagesMatrix.h"

namespace ImageUtility {

	//////////////////////////////////////////////////////////////////////////
	// DDS-related methods
	//
	COMPONENT_FORMAT	ImagesMatrix::DDSLoadFile( System::IO::FileInfo^ _fileName ) {
		if ( !_fileName->Exists )
			throw gcnew System::IO::FileNotFoundException( "File not found!", _fileName->FullName );

		pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );

		ImageUtilityLib::COMPONENT_FORMAT	loadedFormat;
		m_nativeObject->DDSLoadFile( nativeFileName, loadedFormat );

		return (ImageUtility::COMPONENT_FORMAT) loadedFormat;
	}
	COMPONENT_FORMAT	ImagesMatrix::DDSLoadMemory( NativeByteArray^ _imageContent ) {
		ImageUtilityLib::COMPONENT_FORMAT	loadedFormat;
		m_nativeObject->DDSLoadMemory( _imageContent->Length, _imageContent->AsBytePointer.ToPointer(), loadedFormat );

		return (ImageUtility::COMPONENT_FORMAT) loadedFormat;
	}
	void	ImagesMatrix::DDSSaveFile( System::IO::FileInfo^ _fileName, COMPONENT_FORMAT _componentFormat ) {
		pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );

		m_nativeObject->DDSSaveFile( nativeFileName, BaseLib::COMPONENT_FORMAT( _componentFormat ) );
	}
	NativeByteArray^	ImagesMatrix::DDSSaveMemory( COMPONENT_FORMAT _componentFormat ) {
		// Generate native byte array
		U64		fileSize = 0;
		void*	fileContent = NULL;
		m_nativeObject->DDSSaveMemory( fileSize, fileContent, BaseLib::COMPONENT_FORMAT( _componentFormat ) );

		NativeByteArray^	result = gcnew NativeByteArray( int(fileSize), fileContent );
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	// Helpers
	//////////////////////////////////////////////////////////////////////////
	//
	ImageFile^	GrabFace( cli::array<cli::array<float4>^>^ _cross, UInt32 _cubeSize, PIXEL_FORMAT _format, UInt32 _X, UInt32 _Y, int _dirX, int _dirY ) {
		ImageFile^	result = gcnew ImageFile( _cubeSize, _cubeSize, _format, gcnew ColorProfile( ColorProfile::STANDARD_PROFILE::LINEAR ) );

		cli::array<float4>^	scanline = gcnew cli::array<float4>( _cubeSize );
		UInt32	sourceY = _Y * _cubeSize + (_dirY > 0 ? 0 : _cubeSize-1);
		for ( UInt32 Y=0; Y < _cubeSize; Y++ ) {
			UInt32	sourceX = _X * _cubeSize + (_dirX > 0 ? 0 : _cubeSize-1);
			for ( UInt32 X=0; X < _cubeSize; X++ ) {
				scanline[X] = _cross[sourceY][sourceX];
				sourceX += _dirX;
			}
			result->WriteScanline( Y, scanline );
			sourceY += _dirY;
		}

		return result;
	}

	void	ImagesMatrix::ConvertCrossToCubeMap( ImageFile^ _crossMap, bool _buildMips ) {
		cli::array<cli::array<float4>^>^	tempCross = gcnew cli::array<cli::array<float4>^>( _crossMap->Height );
		for ( UInt32 Y=0; Y < _crossMap->Height; Y++ ) {
			cli::array<float4>^	tempScanline = gcnew cli::array<float4>( _crossMap->Width );
			tempCross[Y] = tempScanline;
			_crossMap->ReadScanline( Y, tempScanline );
		}

//		PIXEL_FORMAT	format = _crossMap->PixelFormat;
		PIXEL_FORMAT	format = PIXEL_FORMAT::RGBA32F;	// Force RGBA32F

		// Isolate individual faces
		// We assume the center of the cross is always +Z
		UInt32					cubeSize = 0;
		UInt32					mipsCount = 0;
		cli::array<ImageFile^>^	cubeFaces = gcnew cli::array<ImageFile ^>( 6 );
		if ( tempCross->GetLength(0) < tempCross->GetLength(1) ) {
			// Vertical cross
			cubeSize = _crossMap->Height >> 2;
			float	fMipsCount = Mathf::Log2( (float) cubeSize );
			mipsCount = 1 + (UInt32) Mathf::Floor( fMipsCount );

			cubeFaces[0] = GrabFace( tempCross, cubeSize, format, 2, 1, +1, +1 );	// +X
			cubeFaces[1] = GrabFace( tempCross, cubeSize, format, 0, 1, +1, +1 );	// -X
			cubeFaces[2] = GrabFace( tempCross, cubeSize, format, 1, 0, +1, +1 );	// +Y
			cubeFaces[3] = GrabFace( tempCross, cubeSize, format, 1, 2, +1, +1 );	// -Y
			cubeFaces[4] = GrabFace( tempCross, cubeSize, format, 1, 1, +1, +1 );	// +Z
			cubeFaces[5] = GrabFace( tempCross, cubeSize, format, 1, 3, -1, -1 );	// -Z

		} else {
			// Horizontal cross
			cubeSize = _crossMap->Width >> 2;
			float	fMipsCount = Mathf::Log2( (float) cubeSize );
			mipsCount = 1 + (UInt32) Mathf::Floor( fMipsCount );

throw gcnew Exception( "TODO! Support horizontal cross!" );
		}

		// Save as cube map
		InitCubeTextureArray( cubeSize, 1, mipsCount );
		for ( UInt32 cubeFaceIndex=0; cubeFaceIndex < 6; cubeFaceIndex++ ) {
			(*this)[cubeFaceIndex][0][0] = cubeFaces[cubeFaceIndex];	// Set mip 0 images
		}
		AllocateImageFiles( format, cubeFaces[0]->ColorProfile );		// Allocate remaining mips

		if ( _buildMips )
			BuildMips( IMAGE_TYPE::LINEAR );	// Build them

// Compress
//ImagesMatrix	compressedMatrix = m_device.DDSCompress( matrix, ImageUtility.ImagesMatrix.COMPRESSION_TYPE.BC6H, ImageUtility.COMPONENT_FORMAT.AUTO );
	}

	// We're assuming each cube map face to be 2x2 square units in area since the cube has faces spanning the [-1,+1] domain along each axis
	// We're looking for the solid angle dw spanned by a single pixel which is basically the area of a pixel dA = (2 / cubeResolution)� as
	//	perceived by the camera set at the center of the cube (0,0,0).
	//
	// The solid angle is given by dw = dA * cos(theta) / r�  where dA * cos(theta) is the projected pixel area and r is the distance 
	//	between the camera and the pixel element.
	//
	// If the pixel has coordinate (x, y, 1) with x,y�[-1,+1] then r = sqrt( 1 + x� + y� )
	// We also know cos(alpha) = 1 / r and thus we finally obtain: dw = dA / r^3
	//
	cli::array<float3>^	ImagesMatrix::EncodeSHOrder2() {
		UInt32	C = (*this)[0][0][0]->Width;	// Cube map face dimensions

		float	dA = 2.0f / C;
				dA *= dA;	// Area of a single pixel
		float	r, dw;
		float3	lsDir, wsDir, radiance;
		lsDir.z = 1.0f;		// Cube face is always 1 unit away in front of camera

		// Setup cube face orientations
		cli::array<SharpMath::float3x3>^	faces2World = gcnew cli::array<SharpMath::float3x3>( 6 );
		faces2World[0] = SharpMath::float3x3( gcnew cli::array<float>( 9 )
		{	 0,  0, +1,
			 0, +1,  0,
			+1,  0,  0 } );	// +X
		faces2World[1] = SharpMath::float3x3( gcnew cli::array<float>( 9 )
		{	 0,  0, -1,
			 0, +1,  0,
			-1,  0,  0 } );	// -X
		faces2World[2] = SharpMath::float3x3( gcnew cli::array<float>( 9 )
		{	+1,  0,  0,
			 0,  0, +1,
			 0, +1,  0 } );	// +Y
		faces2World[3] = SharpMath::float3x3( gcnew cli::array<float>( 9 )
		{	+1,  0,  0,
			 0,  0, -1,
			 0, -1,  0 } );	// -Y
		faces2World[4] = SharpMath::float3x3( gcnew cli::array<float>( 9 )
		{	+1,  0,  0,
			 0, +1,  0,
			 0,  0, -1 } );	// +Z
		faces2World[5] = SharpMath::float3x3( gcnew cli::array<float>( 9 )
		{	-1,  0,  0,
			 0, +1,  0,
			 0,  0, +1 } );	// -Z

		cli::array<float4>^	scanline = gcnew cli::array<float4>( C );

		cli::array<double>^	directionalSH = gcnew cli::array<double>( 9 );
		cli::array<float3>^	sumSH = gcnew cli::array<float3>( 9 );

		// Process each face
		for ( UInt32 faceIndex=0; faceIndex < 6; faceIndex++ ) {
			ImageFile^			cubeFace = (*this)[faceIndex][0][0];
			SharpMath::float3x3	face2World = faces2World[faceIndex];

			for ( UInt32 Y=0; Y < C; Y++ ) {
				lsDir.y = 1.0f - 2.0f * (Y+0.5f) / C;

				cubeFace->ReadScanline( Y, scanline );

				for ( UInt32 X=0; X < C; X++ ) {
					lsDir.x = 2.0f * (X+0.5f) / C - 1.0f;
					r = lsDir.Length;
					dw = dA / (r*r*r);

					radiance.x = scanline[X].x;
					radiance.y = scanline[X].y;
					radiance.z = scanline[X].z;

					// Transform from cube face space to world space
					wsDir = lsDir * face2World;
					wsDir /= r;	// Normalize

// 					// Now transform into SH basis (Z-up)
// 					wsDir.Set( wsDir.z, wsDir.x, wsDir.y );

					// Compute SH coefficients for that direction and accumulate
					SphericalHarmonics::SHFunctions::Ylm( wsDir, directionalSH );
					for ( int i=0; i < 9; i++ ) {
						sumSH[i] += dw * (float) directionalSH[i] * radiance;
					}
				}
			}
		}

		return sumSH;
	}
}

