// RendererManaged.h

#pragma once
#include "Device.h"

using namespace System;

namespace RendererManaged {

	public enum class	VERTEX_FORMAT
	{
		Pt4,		// Transformed position (vector4)
		P3,			// Position
		P3N3,		// Position+Normal
		P3N3G3B3T2,	// Position+Normal+Tangent+BiTangent+UV
	};

	static ::IVertexFormatDescriptor*	GetDescriptor( VERTEX_FORMAT _Format )
	{
		IVertexFormatDescriptor*	pDescriptor = NULL;
		switch ( _Format )
		{
		case VERTEX_FORMAT::Pt4:		pDescriptor = &VertexFormatPt4::DESCRIPTOR; break;
		case VERTEX_FORMAT::P3:			pDescriptor = &VertexFormatP3::DESCRIPTOR; break;
		case VERTEX_FORMAT::P3N3:		pDescriptor = &VertexFormatP3N3::DESCRIPTOR; break;
		case VERTEX_FORMAT::P3N3G3B3T2:	pDescriptor = &VertexFormatP3N3G3T2::DESCRIPTOR; break;
		default:	throw gcnew Exception( "Unsupported vertex format!" );
		}

		return pDescriptor;
	}
}
