#pragma once
#include "../Renderer.h"

// The base format descriptor interface
class IFormatDescriptor
{
public:

	virtual DXGI_FORMAT	DirectXFormat() const = 0;
	virtual int			Size() const = 0;
};
