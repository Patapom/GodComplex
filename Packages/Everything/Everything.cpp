#include "stdafx.h"

#include "Everything.h"


Everything::Search::Query::Query( String^ _searchExpression, bool _matchCase, bool _matchWholeWord, bool _matchPath, bool _isRegEx ) {
	try {
		System::Threading::Monitor::Enter( Search::ms_lock );

		MatchCase = _matchCase;
		MatchPath = _matchPath;
		MatchWholeWord = _matchWholeWord;
		IsRegEx = _isRegEx;

		SearchExpression = _searchExpression;

		// Execute
		ExecuteQuery();

		// Copy results immediately
		cli::array<Search::Result^>^	results = Results;

		m_results = gcnew cli::array<Result^>( results->Length );
		for ( int i=0; i < results->Length; i++ ) {
			Search::Result^	source = results[i];
			Result^			target = gcnew Result();
			m_results[i] = target;

			// Copy source result into target result
			target->m_isVolume = source->IsVolume;
			target->m_isFolder = source->IsFolder;
			target->m_isFile = source->IsFile;
			target->m_pathName = source->PathName;
			target->m_fileName = source->FileName;
			target->m_fullName = source->FullName;
		}

	} finally {
		System::Threading::Monitor::Exit( Search::ms_lock );
	}
}


String^	Everything::Search::SearchExpression::get() {
	LPCSTR	strValue = Everything_GetSearchA();
	return	System::Runtime::InteropServices::Marshal::PtrToStringAnsi( System::IntPtr( (void*) strValue ) );
}

void	Everything::Search::SearchExpression::set( String^ value ) {
	const char*	strValue = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( value ).ToPointer();
	Everything_SetSearchA( strValue );
}

cli::array< Everything::Search::Result^ >^	Everything::Search::Results::get() {

	DWORD	resultsCount = Everything_GetNumResults();
	cli::array< Everything::Search::Result^ >^	results = gcnew cli::array<Everything::Search::Result ^>( resultsCount );

	for ( DWORD i=0; i < resultsCount; i++ ) {
		results[i] = gcnew Result( i );
	}

	return results;
}

String^	Everything::Search::Result::PathName::get() {
	LPCSTR	strValue = Everything_GetResultPathA( m_resultIndex );
	return	System::Runtime::InteropServices::Marshal::PtrToStringAnsi( System::IntPtr( (void*) strValue ) );
}

String^	Everything::Search::Result::FileName::get() {
	LPCSTR	strValue = Everything_GetResultFileNameA( m_resultIndex );
	return	System::Runtime::InteropServices::Marshal::PtrToStringAnsi( System::IntPtr( (void*) strValue ) );
}

String^	Everything::Search::Result::FullName::get() {
	char	temp[MAX_PATH];
	DWORD	count = Everything_GetResultFullPathNameA(m_resultIndex, temp, MAX_PATH);
	temp[count] = '\0';
	return	System::Runtime::InteropServices::Marshal::PtrToStringAnsi(System::IntPtr((void*)temp));
//	return System::IO::Path::Combine( PathName, FileName );
}
