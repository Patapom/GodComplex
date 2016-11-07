// ==========================================================
// FreeImage 3 Test Script
//
// Design and implementation by
// - Hervé Drolon (drolon@infonie.fr)
//
// This file is part of FreeImage 3
//
// COVERED CODE IS PROVIDED UNDER THIS LICENSE ON AN "AS IS" BASIS, WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING, WITHOUT LIMITATION, WARRANTIES
// THAT THE COVERED CODE IS FREE OF DEFECTS, MERCHANTABLE, FIT FOR A PARTICULAR PURPOSE
// OR NON-INFRINGING. THE ENTIRE RISK AS TO THE QUALITY AND PERFORMANCE OF THE COVERED
// CODE IS WITH YOU. SHOULD ANY COVERED CODE PROVE DEFECTIVE IN ANY RESPECT, YOU (NOT
// THE INITIAL DEVELOPER OR ANY OTHER CONTRIBUTOR) ASSUME THE COST OF ANY NECESSARY
// SERVICING, REPAIR OR CORRECTION. THIS DISCLAIMER OF WARRANTY CONSTITUTES AN ESSENTIAL
// PART OF THIS LICENSE. NO USE OF ANY COVERED CODE IS AUTHORIZED HEREUNDER EXCEPT UNDER
// THIS DISCLAIMER.
//
// Use at your own risk!
// ==========================================================


#include "TestSuite.h"

// Local test functions
// ----------------------------------------------------------

static void testBasicWrapper(BOOL copySource, BYTE *bits, FREE_IMAGE_TYPE type, int width, int height, int pitch, unsigned bpp) {
	FIBITMAP *src = NULL;
	FIBITMAP *clone = NULL;
	FIBITMAP *dst = NULL;

	// allocate a wrapper
	src = FreeImage_ConvertFromRawBitsEx(copySource, bits, type, width, height, pitch, bpp, FI_RGBA_RED_MASK, FI_RGBA_GREEN_MASK, FI_RGBA_BLUE_MASK, FALSE);
	assert(src != NULL);
	
	// test clone
	clone = FreeImage_Clone(src);
	assert(clone != NULL);

	// test in-place processing
	FreeImage_Invert(src);

	// test processing
	dst = FreeImage_ConvertToFloat(src);
	assert(dst != NULL);

	FreeImage_Unload(dst);
	FreeImage_Unload(clone);

	// unload the wrapper
	FreeImage_Unload(src);
}

static void testViewport(FIBITMAP *dib) {
	FIBITMAP *src = NULL;
	
	// define a viewport as [vp_x, vp_y, vp_width, vp_height]
	// (assume the image is larger than the viewport)
	int vp_width = 300;
	int vp_height = 200;
	int vp_x = FreeImage_GetWidth(dib) / 2 - vp_width / 2;
	int vp_y = FreeImage_GetHeight(dib) / 2 - vp_height / 2;
	
	// point the viewport data
	unsigned bytes_per_pixel = FreeImage_GetLine(dib) / FreeImage_GetWidth(dib);
	BYTE *data = FreeImage_GetBits(dib) + vp_y * FreeImage_GetPitch(dib) + vp_x * bytes_per_pixel;

	// wrap a section (no copy)
	src = FreeImage_ConvertFromRawBitsEx(FALSE/*copySource*/, data, FIT_BITMAP, vp_width, vp_height, FreeImage_GetPitch(dib), FreeImage_GetBPP(dib), FI_RGBA_RED_MASK, FI_RGBA_GREEN_MASK, FI_RGBA_BLUE_MASK);

	// save the section (note that the image is inverted due to previous processing)
	FreeImage_Save(FIF_PNG, src, "viewport.png");
	
	// unload the wrapper
	FreeImage_Unload(src);
}

// Main test functions
// ----------------------------------------------------------

void testWrappedBuffer(const char *lpszPathName, int flags) {
	FIBITMAP *dib = NULL;

	// simulate a user provided buffer
	// -------------------------------
	
	// load the dib
	FREE_IMAGE_FORMAT fif = FreeImage_GetFileType(lpszPathName);
	dib = FreeImage_Load(fif, lpszPathName, flags); 
	assert(dib != NULL);

	// get data info
	FREE_IMAGE_TYPE type = FreeImage_GetImageType(dib);
	unsigned width = FreeImage_GetWidth(dib);
	unsigned height = FreeImage_GetHeight(dib);
	unsigned pitch = FreeImage_GetPitch(dib);
	unsigned bpp = FreeImage_GetBPP(dib);
	BYTE *bits = FreeImage_GetBits(dib);

	// test wrapped buffer manipulations
	// -------------------------------

	testBasicWrapper(TRUE /*copySource*/, bits, type, width, height, pitch, bpp);

	testBasicWrapper(FALSE /*copySource*/, bits, type, width, height, pitch, bpp);

	// test another use-case : viewport
	testViewport(dib);

	// unload the user provided buffer
	// -------------------------------
	FreeImage_Unload(dib);
}
