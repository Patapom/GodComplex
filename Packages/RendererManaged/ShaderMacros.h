// RendererManaged.h

#pragma once

#include "Device.h"

using namespace System;

namespace RendererManaged {

	public ref class	ShaderMacro
	{
	public:
		String^		Name;
		String^		Value;

	public:
		ShaderMacro() : Name( nullptr ), Value( nullptr ) {}
		ShaderMacro( String^ _Name, String^ _Value ) : Name( _Name ), Value( _Value ) {}
	};
}
