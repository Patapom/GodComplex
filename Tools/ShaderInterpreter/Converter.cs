using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

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
			string	FullSource2 = ConvertRegisters( FullSource );
			string	FullSource3 = ConvertSemantics( FullSource2 );
			string	FullSource4 = ConvertConstantBuffers( FullSource3 );
			string	FullSource5 = ConvertVectorConstructors( FullSource4 );

			string	FinalSource = FullSource4;

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
			int	IncludeIndex = _ShaderSource.IndexOf( "#include" );
			if ( IncludeIndex == -1 )
				return _ShaderSource;	// We're done!

			int	FileNameSearchStartIndex = IncludeIndex + "#include".Length;

			int	EOLIndex = _ShaderSource.IndexOf( '\n', FileNameSearchStartIndex );
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
							+ "\r\n// ======================= START INCLUDE " + IncludeFileName + " =======================\r\n"
							+ IncludedSource
							+ "// ======================= END INCLUDE " + IncludeFileName + " =======================\r\n\r\n"
							+ SourceEnd;

			// Recurse
			return ResolveIncludes( _ShaderPath, _ShaderSource );
		}

		/// <summary>
		/// Converts all registers syntax (: register(xx)) into a semantic inserted above the line with register
		/// </summary>
		/// <param name="_Source"></param>
		/// <returns></returns>
		private static string		ConvertRegisters( string _Source )
		{


			return _Source;
		}

		private static string		ConvertSemantics( string _Source )
		{
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
	}
}
