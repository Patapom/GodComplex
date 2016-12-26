#ifdef _DEBUG

#include "../Types.h"

using namespace BaseLib;

#include <windows.h>
#include <mmsystem.h>
#include <string.h>
#include <stdlib.h>
#include <stdio.h>
#include <sys/types.h>
#include <sys/stat.h>

namespace tweakval {
	struct Tweakable {
		enum TweakableType
		{
			Type_INT,
			Type_FLOAT,
		};

		TweakableType type;
		union {
			float	f;
			int		i;
		} val;
	};

	struct TweakableSourceFile {
		BString	filename;
		time_t	lastModificationTime;
	};

	static Dictionary< TweakableSourceFile >	g_TweakableFiles;
	static Dictionary< Tweakable >				g_TweakableValues;

	U32			HashKey( const BString& _fileName, size_t _Counter ) {
		BString	temp( "%s#%d", _fileName, _Counter );
		U32	Hash = temp.Hash();
		return Hash;
	}

	Tweakable*	LookupTweakValue( const BString& _fileName, size_t _Counter ) {
		U32	Hash = HashKey( _fileName, _Counter );
		return g_TweakableValues.Get( Hash );
	}

	time_t		GetFileModTime( const char* _pFileName ) {	
		struct _stat statInfo;
		_stat( _pFileName, &statInfo );

		return statInfo.st_mtime;
	}

	Tweakable&	AddTweakableValue( const BString& _filename, size_t _Counter ) {
		// First, see if this file is in the files list
		U32		Key = _filename.Hash();
		TweakableSourceFile*	pFileEntry = g_TweakableFiles.Get( Key );
		if ( pFileEntry == NULL ) {
			// If it's not found, add to the list of tweakable files, assume it's unmodified since the program has been built 
			TweakableSourceFile&	Value = g_TweakableFiles.Add( Key );
// 			strcpy( Value.pFilename, _pFilename );
			Value.filename = _filename;
			Value.lastModificationTime = GetFileModTime( _filename );
		}

		// Add to the tweakables
		Key = HashKey( _filename, _Counter );
		return g_TweakableValues.Add( Key );
	}

	void	ReloadTweakableFile( TweakableSourceFile& _SrcFile ) {	
		size_t	counter = 0;
		FILE*	fp;
		fopen_s( &fp, _SrcFile.filename, "rt" );
	
		char line[2048], strval[512];
		while ( !feof( fp ) )
		{
			fgets( line, 2048, fp );
			char*	ch = line;

			// chop off c++ comments. C style comments, and 
			// preprocessor directives like #if 0 are not currently
			// handled so beware if you use those too much
			char*	comment = strstr( line, "//" );
			if ( comment )
				*comment = 0;

			// Abuse compiler string concatination.. since the parser isn't smart enough to skip _TV( in quoted strings (but it should skip this comment)
			while ( (ch=strstr( ch, "_T" "V(")) )
			{
				ch += 4; // skip the _TV( value
				char*	chend = strstr( ch, ")" );
				if ( !chend)
					break;	// Unmatched parenthesis

				strncpy_s( strval, 512, ch, chend-ch );
				strval[ chend-ch ] = '\0';

				//printf("TWK: %s, %d : val '%s'\n", srcFile.filename.c_str(), counter, strval );
				ch = chend;

				// Apply the tweaked value (if found)
				Tweakable*	tv = LookupTweakValue( _SrcFile.filename, counter );
				if ( tv ) {
					if ( tv->type == Tweakable::Type_INT ) {
						tv->val.i = atoi( strval );
					} else if ( tv->type == Tweakable::Type_FLOAT ) {
						tv->val.f = float(atof( strval ));
					}
				}

				counter++;
			}
		}
		fclose( fp );
	}

} // namespace tweakval

using namespace tweakval;
float _TweakValue( const BString& _fileName, size_t _counter, float _originalValue ) {
	Tweakable*	tv = LookupTweakValue( _fileName, _counter );
	if ( !tv ) {
		tv = &AddTweakableValue( _fileName, _counter );
		tv->type = Tweakable::Type_FLOAT;
		tv->val.f = _originalValue;
	}

	return tv->val.f;
}

int _TweakValue( const BString& _fileName, size_t _counter, int _originalValue ) {
	Tweakable*	tv = LookupTweakValue( _fileName, _counter );
	if ( !tv ) {
		tv = &AddTweakableValue( _fileName, _counter );
		tv->type = Tweakable::Type_INT;
		tv->val.i = _originalValue;
	}

	return tv->val.i;
}

void	FilesVisitor( int _entryIndex, TweakableSourceFile& _File, void* _pUserData ) {
	time_t	LastModificationTime = GetFileModTime( _File.filename );
	if ( LastModificationTime <= _File.lastModificationTime )
		return;	// No change !

	_File.lastModificationTime = LastModificationTime;
	ReloadTweakableFile( _File );
}

void	ReloadChangedTweakableValues() {
	static int	LastTime = -1;
	int			CurrentTime = timeGetTime();
	if ( LastTime >= 0 && (CurrentTime - LastTime) < MATERIAL_REFRESH_CHANGES_INTERVAL )
		return;	// Too soon to check !

	// Update last check time
	LastTime = CurrentTime;
		
	// Go through the list of Tweakable Files and see if any have changed since their last modification time
	g_TweakableFiles.ForEach( FilesVisitor, NULL );
}

#endif
