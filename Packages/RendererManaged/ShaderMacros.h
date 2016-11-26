// RendererManaged.h

#pragma once

#include "Device.h"

using namespace System;

namespace Renderer {

	// Wraps a simple [key,value] pair used to specify shader compilation macros
	//
	public ref class	ShaderMacro {
	public:
		String^		name;
		String^		value;

	public:
		ShaderMacro() : name( nullptr ), value( nullptr ) {}
		ShaderMacro( String^ _name, String^ _value ) : name( _name ), value( _value ) {}
	};
}
