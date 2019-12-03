#include "stdafx.h"

// Read Functions
U8			BinaryReader::ReadByte() const {
	U8	temp;
	m_stream.Read( sizeof(temp), &temp );
	return temp;
}
U16			BinaryReader::ReadUInt16() const {
	U16	temp;
	m_stream.Read( sizeof(temp), &temp );
	return temp;
}
U32			BinaryReader::ReadUInt32() const {
	U32	temp;
	m_stream.Read( sizeof(temp), &temp );
	return temp;
}
float		BinaryReader::ReadSingle() const {
	float	temp;
	m_stream.Read( sizeof(temp), &temp );
	return temp;
}
double		BinaryReader::ReadDouble() const {
	double	temp;
	m_stream.Read( sizeof(temp), &temp );
	return temp;
}

// Write Functions
void	BinaryWriter::Write( const U8& _value ) const {

}
void	BinaryWriter::Write( const U16& _value ) const {

}
void	BinaryWriter::Write( const U32& _value ) const {

}
void	BinaryWriter::Write( const float& _value ) const {

}
void	BinaryWriter::Write( const double& _value ) const {

}
