#include "stdafx.h"

#include "Everything.h"

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
