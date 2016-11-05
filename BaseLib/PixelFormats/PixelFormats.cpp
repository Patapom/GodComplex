#include "..\Types.h"

using namespace BaseLib;

#ifdef _DEBUG

// Unit testing:
//	• Test instantiation in debug to make sure all abstract methods have been implemented
//	• Test the Read()/Write() contract ensuring a read followed by a write should yield exactly the original data
//
#define ASSERT_IF_NOT_EQUAL( a, b ) ASSERT( a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w, "Read() + Write() don't yield the original value!" )

using namespace BaseLib;

void	unitTest_PF() {

	PF_Empty		instance_Empty;
	PF_R8			instance_R8;
	PF_RG8			instance_RG8;
	PF_RGB8			instance_RGB8;
	PF_RGBA8		instance_RGBA8;
	PF_RGBA8_sRGB	instance_RGBA8_sRGB;
	PF_RGBE			instance_RGBE;
	PF_R16			instance_R16;
	PF_RG16			instance_RG16;
	PF_RGBA16		instance_RGBA16;
	PF_R16F			instance_R16F;
	PF_RG16F		instance_RG16F;
	PF_RGBA16F		instance_RGBA16F;
	PF_R32F			instance_R32F;
	PF_RG32F		instance_RG32F;
	PF_RGB32F		instance_RGB32F;
	PF_RGBA32F		instance_RGBA32F;
	PF_D32			instance_D32;
	PF_D24S8		instance_D24S8;

	// Test 8-bit formats
	for ( U32 i=0; i <= 255; i++ ) {
		bfloat4	temp, temp2;
		instance_R8.Write( i, i, i, i );
		instance_R8.RGBA( temp );
		instance_R8.Write( temp );
		instance_R8.RGBA( temp2 );
		ASSERT_IF_NOT_EQUAL( temp, temp2 );
	}
}

#endif