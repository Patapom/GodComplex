using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

using ShaderInterpreter.ShaderMath;

namespace ShaderInterpreter
{
	public class	Converter
	{
		public class	ConverterException : Exception
		{
			public string	m_Source = null;
			public int		m_PositionStart = 0;
			public int		m_PositionEnd = 0;
			public ConverterException( string _Message, string _Source ) : this( _Message, _Source, 0 )			{}
			public ConverterException( string _Message, string _Source, int _PositionStart ) : this( _Message, _Source, _PositionStart, _PositionStart )			{}
			public ConverterException( string _Message, string _Source, int _PositionStart, int _PositionEnd ) : base( _Message )
			{
				m_Source = _Source;
				m_PositionStart = _PositionStart;
				m_PositionEnd = _PositionEnd;
			}

			/// <summary>
			/// Translates a source index into a line & character position
			/// </summary>
			/// <returns></returns>
			public string		TranslatePosition()
			{
				int	LinesCount = 1;
				int	CharactersCount = 1;
				int	CurrentIndex = 0;
				while ( CurrentIndex < m_PositionStart )
				{
					char	CharAtIndex = m_Source[CurrentIndex++];
					if ( CharAtIndex == '\n' )
					{
						LinesCount++;
						CharactersCount = 1;
					}
					else
						CharactersCount++;
				}

				return "line #" + LinesCount + " char #" + CharactersCount;
			}

			public int			FindBOL( int _Position )
			{
				int	Result = m_Source.LastIndexOf( '\n', _Position );
				return Result != -1 ? Result+1 : 0;	// BOF?
			}

			public int			FindEOL( int _Position )
			{
				int	Result = m_Source.IndexOf( '\n', _Position );
				return Result != -1 ? Result : m_Source.Length;	// EOF?
			}
		}

		/// <summary>
		/// Converts a source shader into a C#-runnable class
		/// </summary>
		/// <param name="_ShaderPath">The path of the shader to convert (for includes resolution)</param>
		/// <param name="_ShaderSource">The shader code</param>
		/// <param name="_EntryPointVS">The name of the Vertex Shader entry point function</param>
		/// <param name="_EntryPointPS">The name of the Vertex Shader entry point function</param>
		/// <returns>A shader converted into a directly interpretable C# class</returns>
		public static string	ConvertShader( FileInfo _ShaderPath, string _ShaderSource, string _EntryPointVS, string _EntryPointPS )
		{
			// First resolve includes so we have the complete source code
			string	FullSource = ResolveIncludes( _ShaderPath, _ShaderSource );

			// Next perform various conversions
			string	FullSource2 = RemovePreprocessorDirectives( FullSource );
			string	FullSource3 = ConvertRegisters( FullSource2 );
			string	FullSource4 = ConvertSemantics( FullSource3 );
			string	FullSource5 = ConvertConstantBuffers( FullSource4 );
			string	FullSource6 = ConvertVectorConstructors( FullSource5 );

			string	FinalSource = FullSource6;

			return FinalSource;
		}

		/// <summary>
		/// Resolves all include files an merge them into the shader source
		/// </summary>
		/// <param name="_ShaderPath"></param>
		/// <param name="_ShaderSource"></param>
		/// <returns></returns>
		private static string		ResolveIncludes( FileInfo _ShaderPath, string _ShaderSource )
		{
			// Remove double characters
			_ShaderSource = _ShaderSource.Replace( "\r\n", "\n" );

			int	IncludeIndex = _ShaderSource.IndexOf( "#include", StringComparison.InvariantCultureIgnoreCase );
			if ( IncludeIndex == -1 )
				return _ShaderSource;	// We're done!

			int	FileNameSearchStartIndex = IncludeIndex + "#include".Length;

			int	EOLIndex = FindEOL( _ShaderSource, FileNameSearchStartIndex );
			if ( EOLIndex == -1 )
				throw new ConverterException( "Failed to find end of line while attempting to replace #include", _ShaderSource, IncludeIndex );

			// Retrieve start of either "Filename" or <Filename>
			int	IncludeFileNameStart = _ShaderSource.IndexOf( '"', FileNameSearchStartIndex, EOLIndex-FileNameSearchStartIndex );
			if ( IncludeFileNameStart == -1 )
			{
				IncludeFileNameStart =_ShaderSource.IndexOf( '<', FileNameSearchStartIndex, EOLIndex-FileNameSearchStartIndex );
				if ( IncludeFileNameStart == -1 )
					throw new ConverterException( "Failed to find the start of the include path (looked for \" and <) while attempting to replace #include", _ShaderSource, IncludeIndex, EOLIndex );
			}

			// Retrieve end of either "Filename" or <Filename>
			int	IncludeFileNameEnd = _ShaderSource.IndexOf( '"', IncludeFileNameStart+1, EOLIndex-IncludeFileNameStart-1 );
			if ( IncludeFileNameEnd == -1 )
			{
				IncludeFileNameEnd =_ShaderSource.IndexOf( '>', IncludeFileNameStart+1, EOLIndex-IncludeFileNameStart-1 );
				if ( IncludeFileNameEnd == -1 )
					throw new ConverterException( "Failed to find the end of the include path (looked for \" and >) while attempting to replace #include", _ShaderSource, IncludeIndex, EOLIndex );
			}

			// Isolate filename & resolve file
			string		IncludeFileName = _ShaderSource.Substring( IncludeFileNameStart+1, IncludeFileNameEnd-IncludeFileNameStart-1 );
			string		IncludeFullPath = Path.Combine( _ShaderPath.Directory.FullName, IncludeFileName );
			FileInfo	IncludeFile = new FileInfo( IncludeFullPath );
			if ( !IncludeFile.Exists )
				throw new ConverterException( "Failed to find include file \"" + IncludeFile.FullName + "\" while attempting to replace #include", _ShaderSource, IncludeIndex, EOLIndex );

			// Replace
			string		IncludedSource;
			using ( StreamReader S = IncludeFile.OpenText() )
				IncludedSource = S.ReadToEnd();

			string		SourceStart = _ShaderSource.Substring( 0, IncludeIndex );
			string		SourceEnd = _ShaderSource.Substring( EOLIndex+1, _ShaderSource.Length-EOLIndex-1 );

			_ShaderSource	= SourceStart
							+ "\r\n	#region ======================= START INCLUDE " + IncludeFileName + " =======================\r\n"
							+ IncludedSource
							+ "	#endregion // ======================= END INCLUDE " + IncludeFileName + " =======================\r\n\r\n"
							+ SourceEnd;

			// Recurse
			return ResolveIncludes( _ShaderPath, _ShaderSource );
		}

		/// <summary>
		/// Removes all recognizable preprocessor directives like #ifdef, #define, #ifndef, #endif, etc.
		/// </summary>
		/// <param name="_Source"></param>
		/// <returns></returns>
		public static string	RemovePreprocessorDirectives( string _Source )
		{
			// First, check if there are #if blocks 'cause we can't support them at the moment!
			ErrorIfLinesWith( _Source, "#if " );
			ErrorIfLinesWith( _Source, "#if\t" );

			// Comment out other directives
			_Source = CommentLinesWith( _Source, "#ifdef" );
			_Source = CommentLinesWith( _Source, "#ifndef" );
			_Source = CommentLinesWith( _Source, "#endif" );
			_Source = CommentLinesWith( _Source, "#define" );

			return _Source;
		}

		/// <summary>
		/// Converts all registers syntax (: register(xx)) into an attribute inserted above the line with register
		/// </summary>
		/// <param name="_Source"></param>
		/// <returns></returns>
		private static string		ConvertRegisters( string _Source )
		{
			int	CurrentPosition = 0;
			int	MatchIndex = 0;
			while ( true )
			{
				MatchIndex = _Source.IndexOf( ": register", CurrentPosition, StringComparison.InvariantCultureIgnoreCase );
				if ( MatchIndex == -1 )
					MatchIndex = _Source.IndexOf( ":register", CurrentPosition, StringComparison.InvariantCultureIgnoreCase );
				if ( MatchIndex == -1 )
					break;

				int	EOLIndex = FindEOL( _Source, MatchIndex );

				// Find beginning of register specification
				int	RegisterStart = IndexOfBetween( _Source, "register", MatchIndex, EOLIndex );
				RegisterStart += "register".Length;
				RegisterStart = IndexOfBetween( _Source, "(", RegisterStart, EOLIndex );
				if ( RegisterStart == -1 )
					throw new ConverterException( "Failed to retrieve opening parenthesis for register specification!", _Source, MatchIndex, EOLIndex );

				// Find end of register specification
				int	RegisterEnd = IndexOfBetween( _Source, ")", RegisterStart, EOLIndex );
				if ( RegisterStart == -1 )
					throw new ConverterException( "Failed to retrieve closing parenthesis for register specification!", _Source, MatchIndex, EOLIndex );

				// Retrieve register spec
				string	Register = _Source.Substring( RegisterStart+1, RegisterEnd-RegisterStart-1 );
				Register = Register.Trim();

				// Remove unsupported syntax
				_Source = _Source.Remove( MatchIndex, RegisterEnd+1-MatchIndex );

				// Replace by inserting a register attribute
				int		BOLIndex = FindBOL( _Source, MatchIndex );
				string	RegisterAttrib = "[Register( \"" + Register + "\" )]\n";
				_Source = Insert( _Source, BOLIndex, RegisterAttrib );

				// New position for starting our search...
				CurrentPosition = MatchIndex;
			}

			return _Source;
		}

		/// <summary>
		/// Converts all semantics syntax (: NAME) into an attribute inserted above the line with semantic
		/// </summary>
		/// <param name="_Source"></param>
		/// <returns></returns>
		private static string		ConvertSemantics( string _Source )
		{
			Regex	REX = new Regex( @":\s*(\w)\s*;.*$", RegexOptions.IgnoreCase | RegexOptions.Multiline );

			int	CurrentPosition = 0;
			int	MatchIndex = 0;
			while ( true )
			{
				Match	M = REX.Match( _Source, CurrentPosition );
// 				MatchIndex = _Source.IndexOf( ": ", CurrentPosition, StringComparison.InvariantCultureIgnoreCase );
// 				if ( MatchIndex == -1 )
// 					MatchIndex = _Source.IndexOf( ":", CurrentPosition, StringComparison.InvariantCultureIgnoreCase );
// 				if ( MatchIndex == -1 )
// 					break;

				int	EOLIndex = FindEOL( _Source, MatchIndex );

				// Find beginning of register specification
				int	RegisterStart = IndexOfBetween( _Source, "register", MatchIndex, EOLIndex );
				RegisterStart += "register".Length;
				RegisterStart = IndexOfBetween( _Source, "(", RegisterStart, EOLIndex );
				if ( RegisterStart == -1 )
					throw new ConverterException( "Failed to retrieve opening parenthesis for register specification!", _Source, MatchIndex, EOLIndex );

				// Find end of register specification
				int	RegisterEnd = IndexOfBetween( _Source, ")", RegisterStart, EOLIndex );
				if ( RegisterStart == -1 )
					throw new ConverterException( "Failed to retrieve closing parenthesis for register specification!", _Source, MatchIndex, EOLIndex );

				// Retrieve register spec
				string	Register = _Source.Substring( RegisterStart+1, RegisterEnd-RegisterStart-1 );
				Register = Register.Trim();

				// Remove unsupported syntax
				_Source = _Source.Remove( MatchIndex, RegisterEnd+1-MatchIndex );

				// Replace by inserting a register attribute
				int		BOLIndex = FindBOL( _Source, MatchIndex );
				string	RegisterAttrib = "[Register( \"" + Register + "\" )]\n";
				_Source = Insert( _Source, BOLIndex, RegisterAttrib );

				// New position for starting our search...
				CurrentPosition = MatchIndex;
			}

			return _Source;
		}

		private static string		ConvertConstantBuffers( string _Source )
		{
			return _Source;
		}

		private static string		ConvertVectorConstructors( string _Source )
		{
			return _Source;
		}

		#region Helpers

		private static string	CommentLinesWith( string _Source, string _Pattern )
		{
			int	CurrentPosition = 0;
			int	MatchIndex = 0;
			while( true )
			{
				MatchIndex = _Source.IndexOf( _Pattern, CurrentPosition, StringComparison.InvariantCultureIgnoreCase );
				if ( MatchIndex == -1 )
					break;

				// Insert comment at beginning of line
				int	BOLIndex = FindBOL( _Source, MatchIndex );
				_Source = Insert( _Source, BOLIndex, "// " );

				CurrentPosition = MatchIndex + 3 + 1;
			}

			return _Source;
		}

		private static void			ErrorIfLinesWith( string _Source, string _Pattern )
		{
			int	MatchIndex = _Source.IndexOf( _Pattern, StringComparison.InvariantCultureIgnoreCase );
			if ( MatchIndex == -1 )
				return;	// We're safe!

			throw new ConverterException( "Found an unsupported preprocess #if! Please remove the concerned block...", _Source, MatchIndex );
		}

		private static string		Insert( string _Source, int _Index, string _Insert )
		{
			string	Start = _Source.Substring( 0, _Index );
			string	End = _Source.Substring( _Index );
			string	Result = Start + _Insert + End;
			return Result;
		}

		private static int			FindBOL( string _Source, int _StartIndex )
		{
			int	MatchIndex = _Source.LastIndexOf( '\n', Math.Max( 0, _StartIndex-1) );
			return MatchIndex != -1 ? MatchIndex+1 : 0;	// BOF?
		}

		private static int			FindEOL( string _Source, int _StartIndex )
		{
			int	MatchIndex = _Source.IndexOf( '\n', _StartIndex );
			return MatchIndex != -1 ? MatchIndex : _Source.Length;	// EOF?
		}

		private static int			IndexOfBetween( string _Source, string _Pattern, int _StartIndex, int _EndIndex )
		{
			int	MatchIndex = _Source.IndexOf( _Pattern, _StartIndex, StringComparison.InvariantCultureIgnoreCase );
			return MatchIndex < _EndIndex ? MatchIndex : -1;
		}

		#endregion
	}
}
