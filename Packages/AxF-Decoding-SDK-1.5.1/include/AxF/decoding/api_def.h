#pragma once

#ifndef __cplusplus
#error This version of the AxF Decoding SDK requires C++
#endif


#if defined(_WIN32)
#	if defined(AXF_SDK_EXPORTS)
#		define AXF_API __declspec(dllexport)
#	elif !defined(AXF_SDK_STATIC_LINK)
#		define AXF_API __declspec(dllimport)
#	else
#		define AXF_API
#	endif
#elif defined(__GNUC__) && __GNUC__ >= 4 && defined(AXF_SDK_EXPORTS)
#	define AXF_API __attribute__((visibility("default")))
#else
#	define AXF_API
#endif


// name of version namespace
#define AXF_DECODING_VERSION_NAMESPACE   v1_5

#ifdef AXF_SDK_DOXYGEN
    // hide version namespace from AxF SDK documentation
#   define AXF_DECODING_OPEN_NAMESPACE   namespace axf { namespace decoding {
#   define AXF_DECODING_CLOSE_NAMESPACE  }}
#elif defined(__GNUC__) && __cplusplus >= 201103L
    // use inline namespace
#   define AXF_DECODING_OPEN_NAMESPACE   namespace axf { namespace decoding { inline namespace AXF_DECODING_VERSION_NAMESPACE {
#   define AXF_DECODING_CLOSE_NAMESPACE  }}}
#else
    // emulate inline namespace (to ensure compatibility to older compilers)
#   define AXF_DECODING_OPEN_NAMESPACE   namespace axf { namespace decoding { namespace AXF_DECODING_VERSION_NAMESPACE {
#   define AXF_DECODING_CLOSE_NAMESPACE  } using namespace AXF_DECODING_VERSION_NAMESPACE; }}
#endif
