// TestDevice.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include "d3d11.h"
#include "dxgi.h"


int _tmain(int argc, _TCHAR* argv[])
{
	HRESULT	hr;
	D3D_FEATURE_LEVEL	FeatureLevel = D3D_FEATURE_LEVEL_11_0;	// Support D3D11...
	D3D_FEATURE_LEVEL	ObtainedFeatureLevel;

	ID3D11Device*			pDevice;
	ID3D11DeviceContext*	pContext;
 	hr = D3D11CreateDevice(
			NULL, D3D_DRIVER_TYPE_HARDWARE, NULL,
			D3D11_CREATE_DEVICE_DEBUG,
			&FeatureLevel, 1,
			D3D11_SDK_VERSION,
			&pDevice, &ObtainedFeatureLevel, &pContext
		);

 
D3D11_DEPTH_STENCIL_DESC	DepthStencilDescDepthWriteStencilTest = {
TRUE,                           // BOOL DepthEnable;
D3D11_DEPTH_WRITE_MASK_ALL,     // D3D11_DEPTH_WRITE_MASK DepthWriteMask;
D3D11_COMPARISON_LESS_EQUAL,    // D3D11_COMPARISON_FUNC DepthFunc;
TRUE,                           // BOOL StencilEnable;
0xFF,							 // UINT8 StencilReadMask;
0,                              // UINT8 StencilWriteMask;
{                               // D3D11_DEPTH_STENCILOP_DESC FrontFace;
    D3D11_STENCIL_OP_KEEP,      // D3D11_STENCIL_OP StencilFailOp;
    D3D11_STENCIL_OP_KEEP,      // D3D11_STENCIL_OP StencilDepthFailOp;
    D3D11_STENCIL_OP_KEEP,      // D3D11_STENCIL_OP StencilPassOp;
    D3D11_COMPARISON_EQUAL,     // D3D11_COMPARISON_FUNC StencilFunc;
}, 
{                               // D3D11_DEPTH_STENCILOP_DESC BackFace;
    D3D11_STENCIL_OP_KEEP,      // D3D11_STENCIL_OP StencilFailOp;
    D3D11_STENCIL_OP_KEEP,      // D3D11_STENCIL_OP StencilDepthFailOp;
    D3D11_STENCIL_OP_KEEP,      // D3D11_STENCIL_OP StencilPassOp;
    D3D11_COMPARISON_EQUAL,     // D3D11_COMPARISON_FUNC StencilFunc;
}, 
};

// Default alignment:
// 0x0018FDBC  00000001 00000001 00000004 00000001 001800ff 00000001  ................ÿ.......
// 0x0018FDD4  00000001 00000001 00000003 00000001 00000001 00000001  ........................
// 0x0018FDEC  00000003 

// 1 byte alignment
// 0x0042FAF8  00000001 00000001 00000004 00000001 000100ff 00010000  ................ÿ.......
// 0x0042FB10  00010000 00030000 00010000 00010000 00010000 00030000  ........................
// 0x0042FB28  00000000 

ID3D11DepthStencilState*	pState;
hr = pDevice->CreateDepthStencilState( &DepthStencilDescDepthWriteStencilTest, &pState );

	pState->Release();

	pDevice->Release();

	return 0;
}

