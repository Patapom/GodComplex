//////////////////////////////////////////////////////////////////////////
// Stream Abstraction
//
#pragma once

#include "../Types.h"

// Abstract Stream Class
class	Stream {
public:
	virtual U64		Position() const abstract;
	virtual void	SetPosition( U64 _position ) abstract;
	virtual U64		Length() const abstract;
	virtual void	Read( U32 _count, void* _container ) const abstract;
	virtual void	Write( U32 _count, const void* _container ) const abstract;
};

// Binary Reader from an abstract stream
class	BinaryReader {
	Stream&		m_stream;
public:
	BinaryReader( Stream& _stream ) : m_stream( _stream ) {}
	Stream&		BaseStream() const	{ return m_stream; }

	U8			ReadByte() const;
	U16			ReadUInt16() const;
	U32			ReadUInt32() const;
	float		ReadSingle() const;
	double		ReadDouble() const;
};

// Binary Writer to an abstract stream
class	BinaryWriter {
	Stream&		m_stream;
public:
	BinaryWriter( Stream& _stream ) : m_stream( _stream ) {}
	Stream&		BaseStream() const	{ return m_stream; }

	void		Write( const U8& _value ) const;
	void		Write( const U16& _value ) const;
	void		Write( const U32& _value ) const;
	void		Write( const float& _value ) const;
	void		Write( const double& _value ) const;
};
