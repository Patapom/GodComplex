//////////////////////////////////////////////////////////////////////////
// Super basic string class
//
// WARNING: If the "GODCOMPLEX" constant is defined then only a shallow copy is made so
//	you have to make sure the const char* is not allocated on the stack!
//
#pragma once

//typedef char*	BString;

//*
class	BString {
private:
	char*		m_str;

public:
	BString() : m_str(nullptr) {}
	BString( const char* _str );
	BString( bool _dummy, const char* _format, ... );	// _dummy is only there to differentiate variadic constructor from regular copy constructor
	BString( BString& _str );
	~BString();

	bool			IsEmpty() const;
	S32				Length() const;

	void			Format( const char* _format, ... );

	const char&		operator[]( U32 _index ) const;
	char&			operator[]( U32 _index );
					operator const char*() const { return m_str; }

// 	String&			operator=( const char* _other );
// 	String&			operator=( const String& _other );

	bool			operator==( const BString& _other ) const;
	bool			operator!=( const BString& _other ) const;

	U32				Hash() const;
	static U32		Hash( const BString& _key );
	static S32		Compare( const BString& _a, const BString& _b );
	static S32		Compare( const BString& _a, const BString& _b, int _maxLength );

private:
	void			Copy( const char* _source );
};
//*/
