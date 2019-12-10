#pragma once

using namespace System;

namespace Everything {

	public ref class Search {
	public:
		//////////////////////////////////////////////////////////////////////////
		// Options
		//
		property static bool	MatchPath {
			bool	get()				{ return Everything_GetMatchPath(); }
			void	set( bool value )	{ Everything_SetMatchPath( value ); }
		}

		property static bool	MatchCase {
			bool	get()				{ return Everything_GetMatchCase(); }
			void	set( bool value )	{ Everything_SetMatchCase( value ); }
		}

		property static bool	MatchWholeWord {
			bool	get()				{ return Everything_GetMatchWholeWord(); }
			void	set( bool value )	{ Everything_SetMatchWholeWord( value ); }
		}

		property static bool	IsRegEx {
			bool	get()				{ return Everything_GetRegex(); }
			void	set( bool value )	{ Everything_SetRegex( value ); }
		}

		//////////////////////////////////////////////////////////////////////////
		// Search query
		//
		property static String^	SearchExpression {
			String^	get();
			void	set( String^ value );
		}

		static void	ExecuteQuery() {
			Everything_QueryA( TRUE );
		}

		//////////////////////////////////////////////////////////////////////////
		// Results
		// 
		ref class	Result {
			UInt32		m_resultIndex;
		internal:
			Result( UInt32 _resultIndex ) : m_resultIndex( _resultIndex ) {}
		public:

			property bool		IsVolume { bool get() { return Everything_IsVolumeResult( m_resultIndex ); } }
			property bool		IsFolder { bool get() { return Everything_IsFolderResult( m_resultIndex ); } }
			property bool		IsFile { bool get() { return Everything_IsFileResult( m_resultIndex ); } }
			property String^	PathName { String^ get(); }
		};

		property static cli::array< Result^ >^	Results {
			cli::array< Result^ >^	get();
		}
	};
}
