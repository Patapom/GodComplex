#pragma once

using namespace System;

namespace Everything {

	//////////////////////////////////////////////////////////////////////////
	// Wrapper for the Everything API (https://www.voidtools.com/support/everything/sdk/)
	// Everything is an amazing search tool that returns file search results extremely quickly
	//////////////////////////////////////////////////////////////////////////
	//
	public ref class Search {
	private:
		static Object^		ms_lock = gcnew Object();

	public:

		//////////////////////////////////////////////////////////////////////////
		// Advanced thread-safe wrapper
		[System::Diagnostics::DebuggerDisplayAttribute( "{m_results.Length,d}" )]
		ref class Query {
		public:
			[System::Diagnostics::DebuggerDisplayAttribute( "{m_fullName}" )]
			ref class	Result {
			public:
				bool		m_isVolume;
				bool		m_isFolder;
				bool		m_isFile;
				String^		m_pathName;
				String^		m_fileName;
				String^		m_fullName;
			};

			cli::array<Result^>^	m_results;

			Query( String^ _searchExpression, bool _matchCase, bool _matchWholeWord, bool _matchPath, bool _isRegEx );
		};


		#pragma region Basic Wrapper (not thread-safe!)

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
		[System::Diagnostics::DebuggerDisplayAttribute( "{FullName}" )]
		ref class	Result {
			UInt32		m_resultIndex;
		internal:
			Result( UInt32 _resultIndex ) : m_resultIndex( _resultIndex ) {}
		public:

			property bool		IsVolume { bool get() { return Everything_IsVolumeResult( m_resultIndex ); } }
			property bool		IsFolder { bool get() { return Everything_IsFolderResult( m_resultIndex ); } }
			property bool		IsFile { bool get() { return Everything_IsFileResult( m_resultIndex ); } }
			property String^	PathName { String^ get(); }
			property String^	FileName { String^ get(); }
			property String^	FullName { String^ get(); }
		};

		property static cli::array< Result^ >^	Results {
			cli::array< Result^ >^	get();
		}

		#pragma endregion
	};
}
