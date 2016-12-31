//////////////////////////////////////////////////////////////////////////
// Super basic string class
//
// WARNING: If the "GODCOMPLEX" constant is defined then only a shallow copy is made so
//	you have to make sure the const char* is not allocated on the stack!
//
#pragma once

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
	U32				Length() const;

	void			Format( const char* _format, ... );
	void			Format( const char* _format, va_list _args );

	BString&		ToLower();
	BString&		ToUpper();
	BString&		Replace( const BString& a, const BString& b );						// Replaces a with b
	bool			StartsWith( const BString& value ) const;							// True if string starts with value
	bool			EndsWith( const BString& value ) const;								// True if string end with value
	S32				IndexOf( const BString& value, int _startIndex=-1 ) const;			// Returns the index where value is found, or -1 if not found
	S32				LastIndexOf( const BString& value, int _startIndex=-1 ) const;		// Returns the index where value is found, or -1 if not found. Search is performed backward

	// Path helpers
	void			GetFileDirectory( BString& _fileDirectory )const;					// Extracts the directory from the file's full path
	void			GetFileName( BString& _fileName ) const;							// Extracts the name from the file's full path
	void			Combine( const BString& _fileDirectory, const BString& _fileName );	// Combines a file directory and file name into a full path


	const char&		operator[]( U32 _index ) const;
	char&			operator[]( U32 _index );
					operator const char*() const	{ return m_str; }
					operator char*()				{ return m_str; }

	BString&		operator=( const char* _other );
	BString&		operator=( const BString& _other );

	bool			operator==( const BString& _other ) const;
	bool			operator!=( const BString& _other ) const;

	U32				Hash() const;
	static U32		Hash( const BString& _key );
	static S32		Compare( const BString& _a, const BString& _b );
	static S32		Compare( const BString& _a, const BString& _b, int _maxLength );

private:
	void			Copy( const char* _source );
};

static U32		GetHash( const BString& _key ) { return _key.Hash(); }
static S32		Compare( const BString& _a, const BString& _b )  { return BString::Compare( _a, _b ); }
