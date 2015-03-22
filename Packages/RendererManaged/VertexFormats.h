// RendererManaged.h

#pragma once
#include "Device.h"
#include "ByteBuffer.h"

using namespace System;

namespace RendererManaged {

	public enum class	VERTEX_FORMAT
	{
		Pt4,		// Transformed position (vector4)
		P3,			// Position
		P3N3,		// Position+Normal
		P3N3G3T2,	// Position+Normal+Tangent+UV
		P3N3G3B3T2,	// Position+Normal+Tangent+Bitangent+UV
		T2,			// UV
	};

	static ::IVertexFormatDescriptor*	GetDescriptor( VERTEX_FORMAT _Format )
	{
		IVertexFormatDescriptor*	pDescriptor = NULL;
		switch ( _Format )
		{
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
	public value struct VertexPt4
	{
		float4	Pt;		// Transformed Position

		static property int	SizeOf	{ int get() { return System::Runtime::InteropServices::Marshal::SizeOf(VertexPt4::typeid); } }
		static ByteBuffer^	FromArray( cli::array<VertexPt4>^ _Vertices )
		{
			ByteBuffer^	Buffer = gcnew ByteBuffer( _Vertices->Length * VertexPt4::SizeOf );
			{
				System::IO::BinaryWriter^ W = Buffer->OpenStreamWrite();
				for ( int VertexIndex=0; VertexIndex < _Vertices->Length; VertexIndex++ )
				{
					W->Write( _Vertices[VertexIndex].Pt.x );
					W->Write( _Vertices[VertexIndex].Pt.y );
					W->Write( _Vertices[VertexIndex].Pt.z );
					W->Write( _Vertices[VertexIndex].Pt.w );
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
		static ByteBuffer^	FromArray( cli::array<VertexP3>^ _Vertices )
		{
			ByteBuffer^	Buffer = gcnew ByteBuffer( _Vertices->Length * VertexP3::SizeOf );
			{
				System::IO::BinaryWriter^ W = Buffer->OpenStreamWrite();
				for ( int VertexIndex=0; VertexIndex < _Vertices->Length; VertexIndex++ )
				{
					W->Write( _Vertices[VertexIndex].P.x );
					W->Write( _Vertices[VertexIndex].P.y );
					W->Write( _Vertices[VertexIndex].P.z );
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
		static ByteBuffer^	FromArray( cli::array<VertexP3N3>^ _Vertices )
		{
			ByteBuffer^	Buffer = gcnew ByteBuffer( _Vertices->Length * VertexP3N3::SizeOf );
			{
				System::IO::BinaryWriter^ W = Buffer->OpenStreamWrite();
				for ( int VertexIndex=0; VertexIndex < _Vertices->Length; VertexIndex++ )
				{
					W->Write( _Vertices[VertexIndex].P.x );
					W->Write( _Vertices[VertexIndex].P.y );
					W->Write( _Vertices[VertexIndex].P.z );
					W->Write( _Vertices[VertexIndex].N.x );
					W->Write( _Vertices[VertexIndex].N.y );
					W->Write( _Vertices[VertexIndex].N.z );
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
		static ByteBuffer^	FromArray( cli::array<VertexP3N3G3T2>^ _Vertices )
		{
			ByteBuffer^	Buffer = gcnew ByteBuffer( _Vertices->Length * VertexP3N3G3T2::SizeOf );
			{
				System::IO::BinaryWriter^ W = Buffer->OpenStreamWrite();
				for ( int VertexIndex=0; VertexIndex < _Vertices->Length; VertexIndex++ )
				{
					W->Write( _Vertices[VertexIndex].P.x );
					W->Write( _Vertices[VertexIndex].P.y );
					W->Write( _Vertices[VertexIndex].P.z );
					W->Write( _Vertices[VertexIndex].N.x );
					W->Write( _Vertices[VertexIndex].N.y );
					W->Write( _Vertices[VertexIndex].N.z );
					W->Write( _Vertices[VertexIndex].T.x );
					W->Write( _Vertices[VertexIndex].T.y );
					W->Write( _Vertices[VertexIndex].T.z );
					W->Write( _Vertices[VertexIndex].UV.x );
					W->Write( _Vertices[VertexIndex].UV.y );
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
		static ByteBuffer^	FromArray( cli::array<VertexP3N3G3B3T2>^ _Vertices )
		{
			ByteBuffer^	Buffer = gcnew ByteBuffer( _Vertices->Length * VertexP3N3G3B3T2::SizeOf );
			{
				System::IO::BinaryWriter^ W = Buffer->OpenStreamWrite();
				for ( int VertexIndex=0; VertexIndex < _Vertices->Length; VertexIndex++ )
				{
					W->Write( _Vertices[VertexIndex].P.x );
					W->Write( _Vertices[VertexIndex].P.y );
					W->Write( _Vertices[VertexIndex].P.z );
					W->Write( _Vertices[VertexIndex].N.x );
					W->Write( _Vertices[VertexIndex].N.y );
					W->Write( _Vertices[VertexIndex].N.z );
					W->Write( _Vertices[VertexIndex].T.x );
					W->Write( _Vertices[VertexIndex].T.y );
					W->Write( _Vertices[VertexIndex].T.z );
					W->Write( _Vertices[VertexIndex].B.x );
					W->Write( _Vertices[VertexIndex].B.y );
					W->Write( _Vertices[VertexIndex].B.z );
					W->Write( _Vertices[VertexIndex].UV.x );
					W->Write( _Vertices[VertexIndex].UV.y );
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
		static ByteBuffer^	FromArray( cli::array<VertexT2>^ _Vertices )
		{
			ByteBuffer^	Buffer = gcnew ByteBuffer( _Vertices->Length * VertexT2::SizeOf );
			{
				System::IO::BinaryWriter^ W = Buffer->OpenStreamWrite();
				for ( int VertexIndex=0; VertexIndex < _Vertices->Length; VertexIndex++ )
				{
					W->Write( _Vertices[VertexIndex].UV.x );
					W->Write( _Vertices[VertexIndex].UV.y );
				}
				Buffer->CloseStream();
			}
			return Buffer;
		}
	};
}
