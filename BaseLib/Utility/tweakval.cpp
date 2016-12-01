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
//		char		pFilename[1024];
		const char*	pFilename;
		time_t		LastModificationTime;
	};

	static Dictionary< TweakableSourceFile >	g_TweakableFiles;
	static Dictionary< Tweakable >				g_TweakableValues;

	U32			HashKey( const char* _pFileName, size_t _Counter ) {
		static char	pStringKey[1024];
		U32	counter = 0;
		sprintf_s( pStringKey, 1024, "%s#%d", _pFileName, counter );
		_Counter = counter;

		U32	Hash = DictionaryString<int>::Hash( pStringKey );
		return Hash;
	}

	Tweakable*	LookupTweakValue( const char* _pFileName, size_t _Counter )
	{
		U32	Hash = HashKey( _pFileName, _Counter );
		return g_TweakableValues.Get( Hash );
	}

	time_t		GetFileModTime( const char* _pFileName )
	{	
		struct _stat statInfo;
		_stat( _pFileName, &statInfo );

		return statInfo.st_mtime;
	}

	Tweakable&	AddTweakableValue( const char* _pFilename, size_t _Counter )
	{
		// First, see if this file is in the files list
		U32		Key = DictionaryString<int>::Hash( _pFilename );
		TweakableSourceFile*	pFileEntry = g_TweakableFiles.Get( Key );
		if ( pFileEntry == NULL )
		{	// if it's not found, add to the list of tweakable files, assume it's unmodified since the program has been built 
			TweakableSourceFile&	Value = g_TweakableFiles.Add( Key );
// 			strcpy( Value.pFilename, _pFilename );
			Value.pFilename = _pFilename;
			Value.LastModificationTime = GetFileModTime( _pFilename );
		}

		// Add to the tweakables
		Key = HashKey( _pFilename, _Counter );
		return g_TweakableValues.Add( Key );
	}

	void	ReloadTweakableFile( TweakableSourceFile& _SrcFile )
	{	
		size_t	counter = 0;
		FILE*	fp;
		fopen_s( &fp, _SrcFile.pFilename, "rt" );
	
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
				Tweakable*	tv = LookupTweakValue( _SrcFile.pFilename, counter );
				if ( tv )
				{
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
float _TweakValue( const char* file, size_t counter, float origVal ) {
	Tweakable*	tv = LookupTweakValue( file, counter );
	if ( !tv )
	{
		tv = &AddTweakableValue( file, counter );
		tv->type = Tweakable::Type_FLOAT;
		tv->val.f = origVal;
	}

	return tv->val.f;
}

int _TweakValue( const char *file, size_t counter, int origVal ) {
	Tweakable*	tv = LookupTweakValue( file, counter );
	if ( !tv )
	{
		tv = &AddTweakableValue( file, counter );
		tv->type = Tweakable::Type_INT;
		tv->val.i = origVal;
	}

	return tv->val.i;
}

void	FilesVisitor( int _EntryIndex, TweakableSourceFile& _File, void* _pUserData ) {
	time_t	LastModificationTime = GetFileModTime( _File.pFilename );
	if ( LastModificationTime <= _File.LastModificationTime )
		return;	// No change !

	_File.LastModificationTime = LastModificationTime;
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
