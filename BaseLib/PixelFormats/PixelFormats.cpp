#include "..\Types.h"

using namespace BaseLib;

PF_Unknown::desc_t		PF_Unknown::Descriptor;
PF_R8::desc_t			PF_R8::Descriptor;
PF_RG8::desc_t			PF_RG8::Descriptor;
PF_RGB8::desc_t			PF_RGB8::Descriptor;
PF_RGBA8::desc_t		PF_RGBA8::Descriptor;
PF_BGR8::desc_t			PF_BGR8::Descriptor;
PF_BGRA8::desc_t		PF_BGRA8::Descriptor;
PF_RGBE::desc_t			PF_RGBE::Descriptor;
PF_R16::desc_t			PF_R16::Descriptor;
PF_RG16::desc_t			PF_RG16::Descriptor;
PF_RGB16::desc_t		PF_RGB16::Descriptor;
PF_RGBA16::desc_t		PF_RGBA16::Descriptor;
PF_R16F::desc_t			PF_R16F::Descriptor;
PF_RG16F::desc_t		PF_RG16F::Descriptor;
PF_RGB16F::desc_t		PF_RGB16F::Descriptor;
PF_RGBA16F::desc_t		PF_RGBA16F::Descriptor;
PF_R32::desc_t			PF_R32::Descriptor;
PF_RG32::desc_t			PF_RG32::Descriptor;
PF_RGB32::desc_t		PF_RGB32::Descriptor;
PF_RGBA32::desc_t		PF_RGBA32::Descriptor;
PF_R32F::desc_t			PF_R32F::Descriptor;
PF_RG32F::desc_t		PF_RG32F::Descriptor;
PF_RGB32F::desc_t		PF_RGB32F::Descriptor;
PF_RGBA32F::desc_t		PF_RGBA32F::Descriptor;

PF_D16::desc_t			PF_D16::Descriptor;
PF_D24S8::desc_t		PF_D24S8::Descriptor;
PF_D32::desc_t			PF_D32::Descriptor;

const IPixelAccessor&	BaseLib::PixelFormat2PixelAccessor( PIXEL_FORMAT _pixelFormat ) {
	switch ( _pixelFormat ) {
		// 8-bits
	case PIXEL_FORMAT::R8:			return PF_R8::Descriptor;
	case PIXEL_FORMAT::RG8:			return PF_RG8::Descriptor;
	case PIXEL_FORMAT::BGR8:		return PF_BGR8::Descriptor;
	case PIXEL_FORMAT::BGRA8:		return PF_BGRA8::Descriptor;
	case PIXEL_FORMAT::RGB8:		return PF_RGB8::Descriptor;
	case PIXEL_FORMAT::RGBA8:		return PF_RGBA8::Descriptor;

		// 16-bits
	case PIXEL_FORMAT::R16:			return PF_R16::Descriptor;
	case PIXEL_FORMAT::RG16:		return PF_RG16::Descriptor;
	case PIXEL_FORMAT::RGB16:		return PF_RGB16::Descriptor;
	case PIXEL_FORMAT::RGBA16:		return PF_RGBA16::Descriptor;

		// 16-bits half-precision floating points
	case PIXEL_FORMAT::R16F:		return PF_R16F::Descriptor;
	case PIXEL_FORMAT::RG16F:		return PF_RG16F::Descriptor;
	case PIXEL_FORMAT::RGB16F:		return PF_RGB16F::Descriptor;
	case PIXEL_FORMAT::RGBA16F:		return PF_RGBA16F::Descriptor;

		// 32-bits
	case PIXEL_FORMAT::R32:			return PF_R32::Descriptor;
 	case PIXEL_FORMAT::RG32:		return PF_RG32::Descriptor;
	case PIXEL_FORMAT::RGB32:		return PF_RGB32::Descriptor;
	case PIXEL_FORMAT::RGBA32:		return PF_RGBA32::Descriptor;

		// 32-bits floating points
	case PIXEL_FORMAT::R32F:		return PF_R32F::Descriptor;
 	case PIXEL_FORMAT::RG32F:		return PF_RG32F::Descriptor;
	case PIXEL_FORMAT::RGB32F:		return PF_RGB32F::Descriptor;
	case PIXEL_FORMAT::RGBA32F:		return PF_RGBA32F::Descriptor;
	}

	return PF_Unknown::Descriptor;
}
PIXEL_FORMAT	BaseLib::PixelAccessor2PixelFormat( const IPixelAccessor& _pixelAccessor ) {
	switch ( _pixelAccessor.Size() ) {
	case 1:
		if ( &_pixelAccessor == &PF_R8::Descriptor ) return PIXEL_FORMAT::R8;
		break;
	case 2:
		if ( &_pixelAccessor == &PF_RG8::Descriptor ) return PIXEL_FORMAT::RG8;
		if ( &_pixelAccessor == &PF_R16::Descriptor ) return PIXEL_FORMAT::R16;
		if ( &_pixelAccessor == &PF_R16F::Descriptor ) return PIXEL_FORMAT::R16F;
		break;
	case 3:
		if ( &_pixelAccessor == &PF_BGR8::Descriptor ) return PIXEL_FORMAT::BGR8;
		if ( &_pixelAccessor == &PF_RGB8::Descriptor ) return PIXEL_FORMAT::RGB8;
		break;
	case 4:
		if ( &_pixelAccessor == &PF_BGRA8::Descriptor ) return PIXEL_FORMAT::BGRA8;
		if ( &_pixelAccessor == &PF_RGBA8::Descriptor ) return PIXEL_FORMAT::RGBA8;
		if ( &_pixelAccessor == &PF_RG16::Descriptor ) return PIXEL_FORMAT::RG16;
		if ( &_pixelAccessor == &PF_RG16F::Descriptor ) return PIXEL_FORMAT::RG16F;
		if ( &_pixelAccessor == &PF_R32::Descriptor ) return PIXEL_FORMAT::R32;
		if ( &_pixelAccessor == &PF_R32F::Descriptor ) return PIXEL_FORMAT::R32F;
		break;
	case 6:
		if ( &_pixelAccessor == &PF_RGB16::Descriptor ) return PIXEL_FORMAT::RGB16;
		if ( &_pixelAccessor == &PF_RGB16F::Descriptor ) return PIXEL_FORMAT::RGB16F;
		break;
	case 8:
		if ( &_pixelAccessor == &PF_RGBA16::Descriptor ) return PIXEL_FORMAT::RGBA16;
		if ( &_pixelAccessor == &PF_RGBA16F::Descriptor ) return PIXEL_FORMAT::RGBA16F;
		if ( &_pixelAccessor == &PF_RG32::Descriptor ) return PIXEL_FORMAT::RG32;
		if ( &_pixelAccessor == &PF_RG32F::Descriptor ) return PIXEL_FORMAT::RG32F;
		break;
	case 12:
		if ( &_pixelAccessor == &PF_RGB32::Descriptor ) return PIXEL_FORMAT::RGB32;
		if ( &_pixelAccessor == &PF_RGB32F::Descriptor ) return PIXEL_FORMAT::RGB32F;
		break;
	case 16:
		if ( &_pixelAccessor == &PF_RGBA32::Descriptor ) return PIXEL_FORMAT::RGBA32;
		if ( &_pixelAccessor == &PF_RGBA32F::Descriptor ) return PIXEL_FORMAT::RGBA32F;
		break;
	}

	return PIXEL_FORMAT::UNKNOWN;
}

PIXEL_FORMAT	BaseLib::DXGIFormat2PixelFormat( DXGI_FORMAT _sourceFormat, COMPONENT_FORMAT& _componentFormat, U32& _pixelSize ) {
	_pixelSize = 0;
	_componentFormat = COMPONENT_FORMAT::AUTO;

	switch ( _sourceFormat ) {
		case DXGI_FORMAT_R8_UINT:				_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::UINT; return PIXEL_FORMAT::R8;
		case DXGI_FORMAT_R8_SINT:				_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::SINT; return PIXEL_FORMAT::R8;
		case DXGI_FORMAT_R8_SNORM:				_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::SNORM; return PIXEL_FORMAT::R8;
		case DXGI_FORMAT_R8_UNORM:				_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::UNORM; return PIXEL_FORMAT::R8;

		case DXGI_FORMAT_R8G8_UINT:				_pixelSize = 2; _componentFormat = COMPONENT_FORMAT::UINT; return PIXEL_FORMAT::RG8;
		case DXGI_FORMAT_R8G8_SINT:				_pixelSize = 2; _componentFormat = COMPONENT_FORMAT::SINT; return PIXEL_FORMAT::RG8;
		case DXGI_FORMAT_R8G8_SNORM:			_pixelSize = 2; _componentFormat = COMPONENT_FORMAT::SNORM; return PIXEL_FORMAT::RG8;
		case DXGI_FORMAT_R8G8_UNORM:			_pixelSize = 2; _componentFormat = COMPONENT_FORMAT::UNORM; return PIXEL_FORMAT::RG8;

		case DXGI_FORMAT_R8G8B8A8_UINT:			_pixelSize = 4; _componentFormat = COMPONENT_FORMAT::UINT; return PIXEL_FORMAT::RGBA8;
		case DXGI_FORMAT_R8G8B8A8_SINT:			_pixelSize = 4; _componentFormat = COMPONENT_FORMAT::SINT; return PIXEL_FORMAT::RGBA8;
		case DXGI_FORMAT_R8G8B8A8_SNORM:		_pixelSize = 4; _componentFormat = COMPONENT_FORMAT::SNORM; return PIXEL_FORMAT::RGBA8;
		case DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:	_pixelSize = 4; _componentFormat = COMPONENT_FORMAT::UNORM_sRGB; return PIXEL_FORMAT::RGBA8;
		case DXGI_FORMAT_R8G8B8A8_UNORM:		_pixelSize = 4; _componentFormat = COMPONENT_FORMAT::UNORM; return PIXEL_FORMAT::RGBA8;
		case DXGI_FORMAT_B8G8R8A8_UNORM_SRGB:	_pixelSize = 4; _componentFormat = COMPONENT_FORMAT::UNORM_sRGB; return PIXEL_FORMAT::BGRA8;
		case DXGI_FORMAT_B8G8R8A8_UNORM:		_pixelSize = 4; _componentFormat = COMPONENT_FORMAT::UNORM; return PIXEL_FORMAT::BGRA8;

		case DXGI_FORMAT_R16_UINT:				_pixelSize = 2; _componentFormat = COMPONENT_FORMAT::UINT; return PIXEL_FORMAT::R16;
		case DXGI_FORMAT_R16_SINT:				_pixelSize = 2; _componentFormat = COMPONENT_FORMAT::SINT; return PIXEL_FORMAT::R16;
		case DXGI_FORMAT_R16_SNORM:				_pixelSize = 2; _componentFormat = COMPONENT_FORMAT::SNORM; return PIXEL_FORMAT::R16;
		case DXGI_FORMAT_R16_UNORM:				_pixelSize = 2; _componentFormat = COMPONENT_FORMAT::UNORM; return PIXEL_FORMAT::R16;
		case DXGI_FORMAT_R16_FLOAT:				_pixelSize = 2; _componentFormat = COMPONENT_FORMAT::AUTO; return PIXEL_FORMAT::R16F;

		case DXGI_FORMAT_R16G16_UINT:			_pixelSize = 4; _componentFormat = COMPONENT_FORMAT::UINT; return PIXEL_FORMAT::RG16;
		case DXGI_FORMAT_R16G16_SINT:			_pixelSize = 4; _componentFormat = COMPONENT_FORMAT::SINT; return PIXEL_FORMAT::RG16;
		case DXGI_FORMAT_R16G16_SNORM:			_pixelSize = 4; _componentFormat = COMPONENT_FORMAT::SNORM; return PIXEL_FORMAT::RG16;
		case DXGI_FORMAT_R16G16_UNORM:			_pixelSize = 4; _componentFormat = COMPONENT_FORMAT::UNORM; return PIXEL_FORMAT::RG16;
		case DXGI_FORMAT_R16G16_FLOAT:			_pixelSize = 4; _componentFormat = COMPONENT_FORMAT::AUTO; return PIXEL_FORMAT::RG16F;

		case DXGI_FORMAT_R16G16B16A16_UINT:		_pixelSize = 8; _componentFormat = COMPONENT_FORMAT::UINT; return PIXEL_FORMAT::RGBA16;
		case DXGI_FORMAT_R16G16B16A16_SINT:		_pixelSize = 8; _componentFormat = COMPONENT_FORMAT::SINT; return PIXEL_FORMAT::RGBA16;
		case DXGI_FORMAT_R16G16B16A16_SNORM:	_pixelSize = 8; _componentFormat = COMPONENT_FORMAT::SNORM; return PIXEL_FORMAT::RGBA16;
		case DXGI_FORMAT_R16G16B16A16_UNORM:	_pixelSize = 8; _componentFormat = COMPONENT_FORMAT::UNORM; return PIXEL_FORMAT::RGBA16;
		case DXGI_FORMAT_R16G16B16A16_FLOAT:	_pixelSize = 8; _componentFormat = COMPONENT_FORMAT::AUTO; return PIXEL_FORMAT::RGBA16F;

 		case DXGI_FORMAT_R32_UINT:				_pixelSize = 4; _componentFormat = COMPONENT_FORMAT::UINT; return PIXEL_FORMAT::R32;	// Unsupported!
 		case DXGI_FORMAT_R32_SINT:				_pixelSize = 4; _componentFormat = COMPONENT_FORMAT::SINT; return PIXEL_FORMAT::R32;	// Unsupported!
		case DXGI_FORMAT_R32_FLOAT:				_pixelSize = 4; _componentFormat = COMPONENT_FORMAT::AUTO; return PIXEL_FORMAT::R32F;

 		case DXGI_FORMAT_R32G32_UINT:			_pixelSize = 8; _componentFormat = COMPONENT_FORMAT::UINT; return PIXEL_FORMAT::RG32;	// Unsupported!
 		case DXGI_FORMAT_R32G32_SINT:			_pixelSize = 8; _componentFormat = COMPONENT_FORMAT::SINT; return PIXEL_FORMAT::RG32;	// Unsupported!
		case DXGI_FORMAT_R32G32_FLOAT:			_pixelSize = 8; _componentFormat = COMPONENT_FORMAT::AUTO; return PIXEL_FORMAT::RG32F;

 		case DXGI_FORMAT_R32G32B32A32_UINT:		_pixelSize = 16; _componentFormat = COMPONENT_FORMAT::UINT; return PIXEL_FORMAT::RGBA32;	// Unsupported!
 		case DXGI_FORMAT_R32G32B32A32_SINT:		_pixelSize = 16; _componentFormat = COMPONENT_FORMAT::SINT; return PIXEL_FORMAT::RGBA32;	// Unsupported!
		case DXGI_FORMAT_R32G32B32A32_FLOAT:	_pixelSize = 16; _componentFormat = COMPONENT_FORMAT::AUTO; return PIXEL_FORMAT::RGBA32F;

		// Compressed formats should be handled as raw buffers
		case DXGI_FORMAT_BC1_UNORM:				_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::UNORM; return PIXEL_FORMAT::BC1;
		case DXGI_FORMAT_BC1_UNORM_SRGB:		_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::UNORM_sRGB; return PIXEL_FORMAT::BC1_sRGB;
		case DXGI_FORMAT_BC2_UNORM:				_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::UNORM; return PIXEL_FORMAT::BC2;
		case DXGI_FORMAT_BC2_UNORM_SRGB:		_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::UNORM_sRGB; return PIXEL_FORMAT::BC2_sRGB;
		case DXGI_FORMAT_BC3_UNORM:				_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::UNORM; return PIXEL_FORMAT::BC3;
		case DXGI_FORMAT_BC3_UNORM_SRGB:		_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::UNORM_sRGB; return PIXEL_FORMAT::BC3_sRGB;
		case DXGI_FORMAT_BC4_UNORM:				_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::UNORM; return PIXEL_FORMAT::BC4;
		case DXGI_FORMAT_BC4_SNORM:				_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::SNORM; return PIXEL_FORMAT::BC4;
		case DXGI_FORMAT_BC5_UNORM:				_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::UNORM; return PIXEL_FORMAT::BC5;
		case DXGI_FORMAT_BC5_SNORM:				_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::SNORM; return PIXEL_FORMAT::BC5;
		case DXGI_FORMAT_BC6H_UF16:				_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::UNORM; return PIXEL_FORMAT::BC6H;
		case DXGI_FORMAT_BC6H_SF16:				_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::SNORM; return PIXEL_FORMAT::BC6H;
		case DXGI_FORMAT_BC7_UNORM:				_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::UNORM; return PIXEL_FORMAT::BC7;
		case DXGI_FORMAT_BC7_UNORM_SRGB:		_pixelSize = 1; _componentFormat = COMPONENT_FORMAT::UNORM_sRGB; return PIXEL_FORMAT::BC7;
	}

	return PIXEL_FORMAT::UNKNOWN;
}

DXGI_FORMAT	BaseLib::PixelFormat2DXGIFormat( PIXEL_FORMAT _sourceFormat, COMPONENT_FORMAT _componentFormat ) {
	switch ( _sourceFormat ) {
		// 8-bits formats
		case PIXEL_FORMAT::R8:
			switch ( _componentFormat ) {
				case COMPONENT_FORMAT::AUTO:
				case COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R8_UNORM; break;
				case COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R8_SNORM; break;
				case COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R8_UINT; break;
				case COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R8_SINT; break;
			}
			break;

		case PIXEL_FORMAT::RG8:
			switch ( _componentFormat ) {
				case COMPONENT_FORMAT::AUTO:
				case COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R8G8_UNORM; break;
				case COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R8G8_SNORM; break;
				case COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R8G8_UINT; break;
				case COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R8G8_SINT; break;
			}
			break;

		case PIXEL_FORMAT::RGBA8:
			switch ( _componentFormat ) {
				case COMPONENT_FORMAT::AUTO:
				case COMPONENT_FORMAT::UNORM_sRGB:	return DXGI_FORMAT_R8G8B8A8_UNORM_SRGB; break;
				case COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R8G8B8A8_UNORM; break;
				case COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R8G8B8A8_SNORM; break;
				case COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R8G8B8A8_UINT; break;
				case COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R8G8B8A8_SINT; break;
			}
			break;
		case PIXEL_FORMAT::BGRA8:
			switch ( _componentFormat ) {
				case COMPONENT_FORMAT::UNORM_sRGB:	return DXGI_FORMAT_B8G8R8A8_UNORM_SRGB; break;
				case COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_B8G8R8A8_UNORM; break;
			}
			break;

		// 16-bits formats
		case PIXEL_FORMAT::R16:
			switch ( _componentFormat ) {
				case COMPONENT_FORMAT::AUTO:
				case COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R16_UNORM; break;
				case COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R16_SNORM; break;
				case COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R16_UINT; break;
				case COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R16_SINT; break;
			}
			break;
		case PIXEL_FORMAT::R16F:
			return DXGI_FORMAT_R16_FLOAT;
			break;

		case PIXEL_FORMAT::RG16:
			switch ( _componentFormat ) {
				case COMPONENT_FORMAT::AUTO:
				case COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R16G16_UNORM; break;
				case COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R16G16_SNORM; break;
				case COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R16G16_UINT; break;
				case COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R16G16_SINT; break;
			}
			break;
		case PIXEL_FORMAT::RG16F:
			return DXGI_FORMAT_R16G16_FLOAT;
			break;

		case PIXEL_FORMAT::RGBA16:
			switch ( _componentFormat ) {
				case COMPONENT_FORMAT::AUTO:
				case COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R16G16B16A16_UNORM; break;
				case COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R16G16B16A16_SNORM; break;
				case COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R16G16B16A16_UINT; break;
				case COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R16G16B16A16_SINT; break;
			}
			break;
		case PIXEL_FORMAT::RGBA16F:
			return DXGI_FORMAT_R16G16B16A16_FLOAT;
			break;

		// 32-bits formats
		case PIXEL_FORMAT::R32:
			switch ( _componentFormat ) {
// 				case COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R32_UNORM; break;	// Doesn't exist anyway
// 				case COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R32_SNORM; break;	// Doesn't exist anyway
				case COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R32_UINT; break;
				case COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R32_SINT; break;
			}
			break;
		case PIXEL_FORMAT::R32F:
			return DXGI_FORMAT_R32_FLOAT;
			break;

		case PIXEL_FORMAT::RG32:
			switch ( _componentFormat ) {
// 				case COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R32G32_UNORM; break;	// Doesn't exist anyway
// 				case COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R32G32_SNORM; break;	// Doesn't exist anyway
				case COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R32G32_UINT; break;
				case COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R32G32_SINT; break;
			}
			break;
		case PIXEL_FORMAT::RG32F:
			return DXGI_FORMAT_R32G32_FLOAT;
			break;

		case PIXEL_FORMAT::RGBA32:
			switch ( _componentFormat ) {
// 				case COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R32G32B32A32_UNORM; break;	// Doesn't exist anyway
// 				case COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R32G32B32A32_SNORM; break;	// Doesn't exist anyway
				case COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R32G32B32A32_UINT; break;
				case COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R32G32B32A32_SINT; break;
			}
			break;
		case PIXEL_FORMAT::RGBA32F:
			return DXGI_FORMAT_R32G32B32A32_FLOAT;
			break;


		// ========================= Compressed Formats =========================
		case PIXEL_FORMAT::BC1:
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::AUTO:
			case COMPONENT_FORMAT::UNORM:			return DXGI_FORMAT_BC1_UNORM;
			}
			break;

		case PIXEL_FORMAT::BC1_sRGB:
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::AUTO:
			case COMPONENT_FORMAT::UNORM_sRGB:		return DXGI_FORMAT_BC1_UNORM_SRGB;
			}
			break;

		case PIXEL_FORMAT::BC2:
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::AUTO:
			case COMPONENT_FORMAT::UNORM:			return DXGI_FORMAT_BC2_UNORM;
			}
			break;

		case PIXEL_FORMAT::BC2_sRGB:
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::AUTO:
			case COMPONENT_FORMAT::UNORM_sRGB:		return DXGI_FORMAT_BC2_UNORM_SRGB;
			}
			break;

		case PIXEL_FORMAT::BC3:
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::AUTO:
			case COMPONENT_FORMAT::UNORM:			return DXGI_FORMAT_BC3_UNORM;
			}
			break;

		case PIXEL_FORMAT::BC3_sRGB:
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::AUTO:
			case COMPONENT_FORMAT::UNORM_sRGB:		return DXGI_FORMAT_BC3_UNORM_SRGB;
			}
			break;

		case PIXEL_FORMAT::BC4:
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::AUTO:
			case COMPONENT_FORMAT::UNORM:			return DXGI_FORMAT_BC4_UNORM;
			case COMPONENT_FORMAT::SNORM:			return DXGI_FORMAT_BC4_SNORM;
			}
			break;

		case PIXEL_FORMAT::BC5:
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::AUTO:
			case COMPONENT_FORMAT::UNORM:			return DXGI_FORMAT_BC5_UNORM;
			case COMPONENT_FORMAT::SNORM:			return DXGI_FORMAT_BC5_SNORM;
			}
			break;

		case PIXEL_FORMAT::BC6H:
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::AUTO:
			case COMPONENT_FORMAT::UNORM:			return DXGI_FORMAT_BC6H_UF16;
			case COMPONENT_FORMAT::SNORM:			return DXGI_FORMAT_BC6H_SF16;
			}
			break;

		case PIXEL_FORMAT::BC7:
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::AUTO:
			case COMPONENT_FORMAT::UNORM:			return DXGI_FORMAT_BC7_UNORM;
			case COMPONENT_FORMAT::UNORM_sRGB:		return DXGI_FORMAT_BC7_UNORM_SRGB;
			}
			break;
	}

	return DXGI_FORMAT_UNKNOWN;
}

DXGI_FORMAT	BaseLib::DepthFormat2DXGIFormat( PIXEL_FORMAT _sourceFormat, DEPTH_COMPONENT_FORMAT _depthComponentFormat ) {
	switch ( _sourceFormat ) {
		// 16-bits formats
		case PIXEL_FORMAT::R16:
		case PIXEL_FORMAT::R16F:
			switch ( _depthComponentFormat ) {
				case DEPTH_COMPONENT_FORMAT::DEPTH_ONLY:	return DXGI_FORMAT_D16_UNORM;
			}
			break;

		// 32-bits formats
 		case PIXEL_FORMAT::R32F:
			switch ( _depthComponentFormat ) {
				case DEPTH_COMPONENT_FORMAT::DEPTH_ONLY:	return DXGI_FORMAT_D32_FLOAT;
				case DEPTH_COMPONENT_FORMAT::DEPTH_STENCIL:	return DXGI_FORMAT_D24_UNORM_S8_UINT;
			}
			break;
	}

	return DXGI_FORMAT_UNKNOWN;
}


#ifdef _DEBUG

// Unit testing:
//	• Test instantiation in debug to make sure all abstract methods have been implemented
//	• Test the Read()/Write() contract ensuring a read followed by a write should yield exactly the original data
//
#define ASSERT_IF_NOT_EQUAL( a, b ) ASSERT( a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w, "Read() + Write() don't yield the original value!" )

using namespace BaseLib;

void	unitTest_PF() {

//	PF_Empty		instance_Empty;
 	PF_R8			instance_R8;
// 	PF_RG8			instance_RG8;
// 	PF_RGB8			instance_RGB8;
// 	PF_RGBA8		instance_RGBA8;
// 	PF_RGBA8_sRGB	instance_RGBA8_sRGB;
// 	PF_RGBE			instance_RGBE;
// 	PF_R16			instance_R16;
// 	PF_RG16			instance_RG16;
// 	PF_RGBA16		instance_RGBA16;
// 	PF_R16F			instance_R16F;
// 	PF_RG16F		instance_RG16F;
// 	PF_RGBA16F		instance_RGBA16F;
// 	PF_R32F			instance_R32F;
// 	PF_RG32F		instance_RG32F;
// 	PF_RGB32F		instance_RGB32F;
// 	PF_RGBA32F		instance_RGBA32F;
// 	PF_D32			instance_D32;
// 	PF_D24S8		instance_D24S8;

	// Test 8-bit formats
	for ( U32 i=0; i <= 255; i++ ) {
		bfloat4	temp, temp2;
		PF_R8::Descriptor.Write( &instance_R8, i, i, i, i );
		PF_R8::Descriptor.RGBA( &instance_R8, temp );
		PF_R8::Descriptor.Write( &instance_R8, temp );
		PF_R8::Descriptor.RGBA( &instance_R8, temp2 );
		ASSERT_IF_NOT_EQUAL( temp, temp2 );
	}
}

#endif