// The structures present in this file allow to quickly write a stream of predefined vertex structures
// Only basic and mostly used structures are defined and you will need to add your own if you want to 
//	support additional vertex formats
//
#pragma once

#include "Device.h"
#include "ByteBuffer.h"

using namespace System;
using namespace SharpMath;

namespace Renderer {

	public enum class	VERTEX_FORMAT
	{
		Pt4,		// Transformed position (vector4)
		P3,			// Position
		P3N3,		// Position+Normal
		P3N3G3T2,	// Position+Normal+Tangent+UV
		P3N3G3B3T2,	// Position+Normal+Tangent+Bitangent+UV
		T2,			// UV
	};

	static ::IVertexFormatDescriptor*	GetDescriptor( VERTEX_FORMAT _format ) {
		IVertexFormatDescriptor*	pDescriptor = NULL;
		switch ( _format ) {
		case VERTEX_FORMAT::Pt4:		pDescriptor = &VertexFormatPt4::DESCRIPTOR; break;
		case VERTEX_FORMAT::P3:			pDescriptor = &VertexFormatP3::DESCRIPTOR; break;
		case VERTEX_FORMAT::P3N3:		pDescriptor = &VertexFormatP3N3::DESCRIPTOR; break;
		case VERTEX_FORMAT::P3N3G3T2:	pDescriptor = &VertexFormatP3N3G3T2::DESCRIPTOR; break;
		case VERTEX_FORMAT::P3N3G3B3T2:	pDescriptor = &VertexFormatP3N3G3B3T2::DESCRIPTOR; break;
		case VERTEX_FORMAT::T2:			pDescriptor = &VertexFormatT2::DESCRIPTOR; break;

		default:	throw gcnew Exception( "Unsupported vertex format!" );
		}

		return pDescriptor;
	}

	//////////////////////////////////////////////////////////////////////////
	// Pt4
	[System::Runtime::InteropServices::StructLayoutAttribute( System::Runtime::InteropServices::LayoutKind::Sequential )]
	public value struct VertexPt4 {
		float4	Pt;		// Transformed Position

		static property int	SizeOf	{ int get() { return System::Runtime::InteropServices::Marshal::SizeOf(VertexPt4::typeid); } }
		static ByteBuffer^	FromArray( cli::array<VertexPt4>^ _vertices ) {
			ByteBuffer^	Buffer = gcnew ByteBuffer( _vertices->Length * VertexPt4::SizeOf );
			{
				System::IO::BinaryWriter^ W = Buffer->OpenStreamWrite();
				for ( int VertexIndex=0; VertexIndex < _vertices->Length; VertexIndex++ ) {
					W->Write( _vertices[VertexIndex].Pt.x );
					W->Write( _vertices[VertexIndex].Pt.y );
					W->Write( _vertices[VertexIndex].Pt.z );
					W->Write( _vertices[VertexIndex].Pt.w );
				}
				Buffer->CloseStream();
			}
			return Buffer;
		}
	};

	//////////////////////////////////////////////////////////////////////////
	// P3
	[System::Runtime::InteropServices::StructLayoutAttribute( System::Runtime::InteropServices::LayoutKind::Sequential )]
	public value struct VertexP3
	{
		float3	P;		// Position

		static property int	SizeOf	{ int get() { return System::Runtime::InteropServices::Marshal::SizeOf(VertexP3::typeid); } }
		static ByteBuffer^	FromArray( cli::array<VertexP3>^ _vertices )
		{
			ByteBuffer^	Buffer = gcnew ByteBuffer( _vertices->Length * VertexP3::SizeOf );
			{
				System::IO::BinaryWriter^ W = Buffer->OpenStreamWrite();
				for ( int VertexIndex=0; VertexIndex < _vertices->Length; VertexIndex++ ) {
					W->Write( _vertices[VertexIndex].P.x );
					W->Write( _vertices[VertexIndex].P.y );
					W->Write( _vertices[VertexIndex].P.z );
				}
				Buffer->CloseStream();
			}
			return Buffer;
		}
	};

	//////////////////////////////////////////////////////////////////////////
	// P3N3
	[System::Runtime::InteropServices::StructLayoutAttribute( System::Runtime::InteropServices::LayoutKind::Sequential )]
	public value struct VertexP3N3
	{
		float3	P;		// Position
		float3	N;		// Normal

		static property int	SizeOf	{ int get() { return System::Runtime::InteropServices::Marshal::SizeOf(VertexP3N3::typeid); } }
		static ByteBuffer^	FromArray( cli::array<VertexP3N3>^ _vertices )
		{
			ByteBuffer^	Buffer = gcnew ByteBuffer( _vertices->Length * VertexP3N3::SizeOf );
			{
				System::IO::BinaryWriter^ W = Buffer->OpenStreamWrite();
				for ( int VertexIndex=0; VertexIndex < _vertices->Length; VertexIndex++ ) {
					W->Write( _vertices[VertexIndex].P.x );
					W->Write( _vertices[VertexIndex].P.y );
					W->Write( _vertices[VertexIndex].P.z );
					W->Write( _vertices[VertexIndex].N.x );
					W->Write( _vertices[VertexIndex].N.y );
					W->Write( _vertices[VertexIndex].N.z );
				}
				Buffer->CloseStream();
			}
			return Buffer;
		}
	};

	//////////////////////////////////////////////////////////////////////////
	// P3N3G3T2
	[System::Runtime::InteropServices::StructLayoutAttribute( System::Runtime::InteropServices::LayoutKind::Sequential )]
	public value struct VertexP3N3G3T2
	{
		float3	P;		// Position
		float3	N;		// Normal
		float3	T;		// Tangent
		float2	UV;		// TexCoords

		static property int	SizeOf	{ int get() { return System::Runtime::InteropServices::Marshal::SizeOf(VertexP3N3G3T2::typeid); } }
		static ByteBuffer^	FromArray( cli::array<VertexP3N3G3T2>^ _vertices )
		{
			ByteBuffer^	Buffer = gcnew ByteBuffer( _vertices->Length * VertexP3N3G3T2::SizeOf );
			{
				System::IO::BinaryWriter^ W = Buffer->OpenStreamWrite();
				for ( int VertexIndex=0; VertexIndex < _vertices->Length; VertexIndex++ ) {
					W->Write( _vertices[VertexIndex].P.x );
					W->Write( _vertices[VertexIndex].P.y );
					W->Write( _vertices[VertexIndex].P.z );
					W->Write( _vertices[VertexIndex].N.x );
					W->Write( _vertices[VertexIndex].N.y );
					W->Write( _vertices[VertexIndex].N.z );
					W->Write( _vertices[VertexIndex].T.x );
					W->Write( _vertices[VertexIndex].T.y );
					W->Write( _vertices[VertexIndex].T.z );
					W->Write( _vertices[VertexIndex].UV.x );
					W->Write( _vertices[VertexIndex].UV.y );
				}
				Buffer->CloseStream();
			}
			return Buffer;
		}
	};

	//////////////////////////////////////////////////////////////////////////
	// P3N3G3B3T2
	[System::Runtime::InteropServices::StructLayoutAttribute( System::Runtime::InteropServices::LayoutKind::Sequential )]
	public value struct VertexP3N3G3B3T2
	{
		float3	P;		// Position
		float3	N;		// Normal
		float3	T;		// Tangent
		float3	B;		// Bi-Tangent
		float2	UV;		// TexCoords

		static property int	SizeOf	{ int get() { return System::Runtime::InteropServices::Marshal::SizeOf(VertexP3N3G3B3T2::typeid); } }
		static ByteBuffer^	FromArray( cli::array<VertexP3N3G3B3T2>^ _vertices )
		{
			ByteBuffer^	Buffer = gcnew ByteBuffer( _vertices->Length * VertexP3N3G3B3T2::SizeOf );
			{
				System::IO::BinaryWriter^ W = Buffer->OpenStreamWrite();
				for ( int VertexIndex=0; VertexIndex < _vertices->Length; VertexIndex++ ) {
					W->Write( _vertices[VertexIndex].P.x );
					W->Write( _vertices[VertexIndex].P.y );
					W->Write( _vertices[VertexIndex].P.z );
					W->Write( _vertices[VertexIndex].N.x );
					W->Write( _vertices[VertexIndex].N.y );
					W->Write( _vertices[VertexIndex].N.z );
					W->Write( _vertices[VertexIndex].T.x );
					W->Write( _vertices[VertexIndex].T.y );
					W->Write( _vertices[VertexIndex].T.z );
					W->Write( _vertices[VertexIndex].B.x );
					W->Write( _vertices[VertexIndex].B.y );
					W->Write( _vertices[VertexIndex].B.z );
					W->Write( _vertices[VertexIndex].UV.x );
					W->Write( _vertices[VertexIndex].UV.y );
				}
				Buffer->CloseStream();
			}
			return Buffer;
		}
	};

	//////////////////////////////////////////////////////////////////////////
	// T2
	[System::Runtime::InteropServices::StructLayoutAttribute( System::Runtime::InteropServices::LayoutKind::Sequential )]
	public value struct VertexT2
	{
		float2	UV;		// UV

		static property int	SizeOf	{ int get() { return System::Runtime::InteropServices::Marshal::SizeOf(VertexT2::typeid); } }
		static ByteBuffer^	FromArray( cli::array<VertexT2>^ _vertices )
		{
			ByteBuffer^	Buffer = gcnew ByteBuffer( _vertices->Length * VertexT2::SizeOf );
			{
				System::IO::BinaryWriter^ W = Buffer->OpenStreamWrite();
				for ( int VertexIndex=0; VertexIndex < _vertices->Length; VertexIndex++ ) {
					W->Write( _vertices[VertexIndex].UV.x );
					W->Write( _vertices[VertexIndex].UV.y );
				}
				Buffer->CloseStream();
			}
			return Buffer;
		}
	};
}
