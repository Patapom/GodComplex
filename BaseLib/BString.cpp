#include "Types.h"
//*
#include <stdarg.h>
#include <stdio.h>
#include <string.h>

BString::BString( const char* _str ) : m_str( NULL ) {
	Copy( _str );
}

BString::BString( bool, const char* _format, ... ) : m_str( NULL ) {
	va_list args;
	va_start( args, _format );
	Format( _format, args );
	va_end( args );
}
BString::BString( BString& _str ) : m_str( NULL ) {
	Copy( _str.m_str );
}

bool	BString::IsEmpty() const {
	return m_str == nullptr || *m_str == '\0';
}

S32		BString::Length() const {
	if ( m_str == nullptr )
		return 0;

	S32	length = S32( strlen( m_str ) );
	return length;
}

void	BString::Format( const char* _format, ... ) {
	va_list args;
	va_start( args, _format );
	Format( _format, args );
	va_end( args );
}
void	BString::Format( const char* _format, va_list _args ) {
	SAFE_DELETE_ARRAY( m_str );

	m_str = new char[4096];
	vsprintf_s( m_str, 4096, _format, _args );
}

BString&	BString::operator=( const char* _other ) {
	Copy( _other );
	return *this;
}
BString&	BString::operator=( const BString& _other ) {
	Copy( _other.m_str );
	return *this;
}

const char&	BString::operator[]( U32 _index ) const {
	RELEASE_ASSERT( m_str != NULL, "Invalid string!" );
	return m_str[_index];
}

char&	BString::operator[]( U32 _index ) {
	RELEASE_ASSERT( m_str != NULL, "Invalid string!" );
	return m_str[_index];
}

bool	BString::operator==( const BString& _other ) const {
	if ( m_str == nullptr || _other.m_str == nullptr ) {
		return m_str == _other.m_str;
	}
	int	cmp = strcmp( m_str, _other.m_str );
	return cmp == 0;
}
bool	BString::operator!=( const BString& _other ) const {
	if ( m_str == nullptr || _other.m_str == nullptr ) {
		return m_str != _other.m_str;
	}
	int	cmp = strcmp( m_str, _other.m_str );
	return cmp != 0;
}

U32	BString::Hash() const {
	return BString::Hash( *this );
}

U32	BString::Hash( const BString& _key ) {
	// djb2
	const char*	ptr = _key.m_str;
	if ( ptr == nullptr )
		return 0;

	int c;
	U32 hash = 5381;
	while ( c = *ptr++ ) {
		hash = ((hash << 5) + hash) + c;
	}
	return hash;
}

S32	BString::Compare( const BString& _a, const BString& _b ) {
	int	cmp = strcmp( _a.m_str, _b.m_str );
	return cmp;
}
S32	BString::Compare( const BString& _a, const BString& _b, int _maxLength ) {
	int	cmp = strncmp( _a.m_str, _b.m_str, _maxLength );
	return cmp;
}


#ifndef GODCOMPLEX
	// Deep copy
	void	BString::Copy( const char* _source ) {
		SAFE_DELETE_ARRAY( m_str );
		if ( _source != NULL ) {
			size_t	length = strlen( _source ) + 1;
			char*	str = new char[length];
			strncpy_s( str, length, _source, length-1 );
			m_str = str;
		}
	}

	BString::~BString() {
		SAFE_DELETE_ARRAY( m_str );
	}

#else
	// Shallow copy
	void	BString::Copy( const char* _source ) {
		m_str = _source;
	}
	BString::~BString() {
	}
#endif
//*/