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

U32		BString::Length() const {
	if ( m_str == nullptr )
		return 0;

	U32	length = U32( strlen( m_str ) );
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

BString&	BString::ToLower() {
	_strlwr_s( m_str, Length()+1 );
	return *this;
}
BString&	BString::ToUpper() {
	_strupr_s( m_str, Length()+1 );
	return *this;
}
BString&	BString::Replace( const BString& a, const BString& b ) {
	U32		l = Length();
	U32		la = a.Length();
	if ( l == 0 || la == 0 || la > l )
		return *this;
	U32		lb = b.Length();

	// Track all occurrences and replace
	char	temp[4096];
	char*	sourcePtr = m_str;
	char*	sourcePtrEnd = sourcePtr + l;
	char*	targetPtr = temp;
	while ( sourcePtr < sourcePtrEnd ) {
		if ( !strncmp( sourcePtr, a.m_str, la ) ) {
			// Found an occurrence!
			strncpy_s( targetPtr, 4096-(targetPtr-temp), b.m_str, lb );	// Replace a with b
			targetPtr += lb;
			sourcePtr += la;
		} else {
			*targetPtr++ = *sourcePtr++;
		}
	}

	*targetPtr++ = '\0';

	// Replace string
	*this = temp;

	return *this;
}
bool		BString::StartsWith( const BString& value ) const {
	U32	l = value.Length();
	return strncmp( m_str, value.m_str, l ) == 0;
}
bool		BString::EndsWith( const BString& value ) const {
	U32	l0 = Length();
	U32	l1 = value.Length();
	if ( l1 > l0 )
		return false;

	int	cmp = strncmp( m_str+l0-l1, value.m_str, l1 );
	return cmp == 0;
}
S32			BString::IndexOf( const BString& value, int _startIndex ) const {
	U32	l = Length();
	if ( _startIndex > S32(l) )
		return -1;

	const char*	pattern = strstr( m_str + (_startIndex == -1 ? 0 : _startIndex), value.m_str );
	S32			index = pattern != NULL ? S32( pattern - m_str ) : -1;
	return index;
}
S32			BString::LastIndexOf( const BString& value, int _startIndex ) const {
	U32	l0 = Length();
	U32	l1 = value.Length();
	if ( _startIndex > S32(l0) || l1 > l0 )
		return -1;
	if ( l1 == 0 )
		return l0;	// Empty string is always at the end...

	_startIndex = _startIndex >= 0 ? MIN( l0-l1, U32(_startIndex) ) : l0 - l1;
	while ( _startIndex >= 0 ) {
		if ( !strncmp( m_str + _startIndex, value.m_str, l1 ) )
			break;
		_startIndex--;
	}

	return _startIndex;
}

void	BString::GetFileDirectory( BString& _fileDirectory ) const {
	if ( IsEmpty() ) {
		_fileDirectory = "\0";
		return;
	}

	_fileDirectory = *this;
	_fileDirectory.Replace( "\\", "/" );
	S32	indexOfLastSlash = _fileDirectory.LastIndexOf( "/" );
	if ( indexOfLastSlash != -1 ) {
		_fileDirectory[indexOfLastSlash] = '\0';
	}

// 	// Cut at last slash or anti-slash
// 	const char*	pLastSlash = strrchr( _fileDirectory, '\\' );
// 	if ( pLastSlash == NULL )
// 		pLastSlash = strrchr( _fileDirectory, '/' );
// 
// 	if ( pLastSlash != NULL ) {
// 		size_t	end = 1 + pLastSlash - _fileDirectory;
// 		_fileDirectory[U32(end)] = '\0';
// 
// 		// Make lowercase
// 		_fileDirectory.ToLower();
// 	} else {
// 		_fileDirectory[0] = '\0';
// 	}
}
void	BString::GetFileName( BString& _fileName ) const {
	if ( IsEmpty() ) {
		_fileName = "\0";
		return;
	}

	S32	lastIndexOfSlash = LastIndexOf( "\\" );
	if ( lastIndexOfSlash == -1 )
		lastIndexOfSlash = LastIndexOf( "/" );

	const char*	fileNameStart = m_str + lastIndexOfSlash + 1;	// Skip the last (anti-) slash
	_fileName = fileNameStart;

// 	const char*	pLastSlash = strrchr( _filePath, '\\' );
// 	if ( pLastSlash == NULL )
// 		pLastSlash = strrchr( _filePath, '/' );
// 	if ( pLastSlash == NULL )
// 		pLastSlash = _filePath - 1;
// 
// 	const char*	fileNameStart = pLastSlash + 1;
// 	_fileName = fileNameStart;
}
void	BString::Combine( const BString& _fileDirectory, const BString& _fileName ) {
	bool	endsWithSlash = _fileDirectory.IsEmpty() || _fileDirectory.EndsWith( "\\" ) || _fileDirectory.EndsWith( "/" );
	Format( "%s%s%s", _fileDirectory.m_str, endsWithSlash ? "" : "/", _fileName.m_str );
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