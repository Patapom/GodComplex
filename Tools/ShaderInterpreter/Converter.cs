using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

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
		public static string	ConvertShader( FileInfo _ShaderPath, string _ShaderSource, string _ClassName, string _EntryPointVS, string _EntryPointPS )
		{
			// First resolve includes so we have the complete source code
			string	FullSource = ResolveIncludes( _ShaderPath, _ShaderSource );

			// Next perform various conversions
			FullSource = RemovePreprocessorDirectives( FullSource );
			FullSource = ReplaceStaticConsts( FullSource );
			FullSource = ConvertConstantBuffers( FullSource );
			FullSource = MakeStructFieldsPublic( FullSource );
			FullSource = ConvertRegisters( FullSource );
			FullSource = ConvertSemantics( FullSource );
			FullSource = ConvertFloatToDouble( FullSource );
			FullSource = ConvertVectorConstructors( FullSource );
			FullSource = ConvertOutFunctions( FullSource );

			// Finish by including the source in the middle of our header and footer
			string	Header = "using System;\n"
						   + "using ShaderInterpreter.ShaderMath;\n"
						   + "using ShaderInterpreter.Textures;\n"
						   + "\n"
						   + "namespace ShaderInterpreter\n"
						   + "{\n"
						   + "	public class	" + _ClassName + " : Shader\n"
						   + "	{\n"
						   + "";

			string	Footer = "\n	}\n"	// End the class
						   + "\n}\n";		// End the namespace

			string	FinalSource = Header
								+ FullSource
								+ Footer;

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
		public static string		RemovePreprocessorDirectives( string _Source )
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
		/// Replaces "static consts" by simple "static"
		/// </summary>
		/// <param name="_Source"></param>
		/// <returns></returns>
		public static string		ReplaceStaticConsts( string _Source )
		{
//			_Source = _Source.Replace( "static const", "const" );	// Don't!
			_Source = _Source.Replace( "static const", "static" );

			return _Source;
		}

		/// <summary>
		/// Makes all the fields in structs public
		/// </summary>
		/// <param name="_Source"></param>
		/// <returns></returns>
		private static string		MakeStructFieldsPublic( string _Source )
		{
			int	CurrentPosition = 0;
			while ( true )
			{
				int	StructIndex = _Source.IndexOf( "struct", CurrentPosition, StringComparison.InvariantCultureIgnoreCase );
				if ( StructIndex == -1 )
					break;

				int	StructBeginIndex = _Source.IndexOf( "{", StructIndex, StringComparison.InvariantCultureIgnoreCase );
				if ( StructBeginIndex == -1 )
					throw new ConverterException( "Failed to retrieve opening { for struct!", _Source, StructIndex );

				int	StructEndIndex = _Source.IndexOf( "}", StructBeginIndex, StringComparison.InvariantCultureIgnoreCase );
				if ( StructEndIndex == -1 )
					throw new ConverterException( "Failed to retrieve closing } for struct!", _Source, StructIndex );

				CurrentPosition = FindEOL( _Source, StructBeginIndex );
				if ( CurrentPosition == -1 )
					throw new ConverterException( "Failed to retrieve end of line after struct { !", _Source, StructIndex );
				CurrentPosition++;	// Start at new line

				while ( CurrentPosition < StructEndIndex )
				{
					int	EOLIndex = FindEOL( _Source, CurrentPosition );

					// Retrieve and check for valid field
					int		FieldIndex = CurrentPosition;
					string	Field = _Source.Substring( FieldIndex, EOLIndex - FieldIndex );
					Field = Field.Trim();

					int		SemicolonIndex = Field.IndexOf( ";" );
					SemicolonIndex = SemicolonIndex == -1 ? Field.Length : SemicolonIndex;
					bool	IsCommented = IsCommentedLine( Field, SemicolonIndex );	// The line is commented if we find a // before any ; terminator

					if ( Field == "" || IsCommented )
					{	// This line is either empty or is a comment...
						CurrentPosition = EOLIndex+1;
						continue;
					}

					// Insert the public keyword at the beginning of this field declaration
					_Source = _Source.Insert( CurrentPosition, "public " );

					// Go to next line...
					CurrentPosition = FindEOL( _Source, CurrentPosition );
					if ( CurrentPosition == -1 )
						throw new ConverterException( "Failed to retrieve end of line after struct field \"" + Field + "\"!", _Source, FieldIndex, EOLIndex );

					CurrentPosition++;	// Start at new line

					// Update position of end of structure
					StructEndIndex = _Source.IndexOf( "}", StructBeginIndex, StringComparison.InvariantCultureIgnoreCase );
				}
			}

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

				int	BOLIndex = FindBOL( _Source, MatchIndex );
				int	EOLIndex = FindEOL( _Source, MatchIndex );

				if ( IsCommentedLine( _Source, MatchIndex ) )
				{	// Skip this line if it's a comment
					CurrentPosition = MatchIndex+1;
					continue;
				}

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
				string	RegisterAttrib = "[Register( \"" + Register + "\" )]\n";
				_Source = _Source.Insert( BOLIndex, RegisterAttrib );

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
// Regex	REX = new Regex( @"^[^?]*:([ \t]*[a-zA-Z0-9_]*)[ \t]*;.*$", RegexOptions.IgnoreCase | RegexOptions.Multiline );
// string	Captures = "";
// MatchCollection	Koll = REX.Matches( Test );
// foreach ( Match K in Koll )
// {
// 	Captures += K.Value + "(" + K.Index + ", " + K.Length + ")\n";
// 	foreach ( Capture C in K.Captures )
// 	{
// 		Captures += "\t Capture = " + C.Value;
// 	}
// 	foreach ( Group G in K.Groups )
// 	{
// 		Captures += "\t Group = " + G.Value;
// 	}
// }

// _Source =	"Hey ? Dont : GetMe ;  // Don't get him!\n"
// 			+	"float	Bisou : SEMANTIC;\n"
// 			+	"truite  Aglamou : MOISI2;	// Hey!\n"
// 			+	"truite  Aglamou : MOISI1 sqdqsd;	// Hey!\n"
// 			+	"End of file...";

			int	CurrentPosition = 0;
			while ( true )
			{
				int	SemanticStart = _Source.IndexOf( ":", CurrentPosition, StringComparison.InvariantCultureIgnoreCase );
				if ( SemanticStart == -1 )
					break;

				// Ensure there's no "?" on the same line to avoid grabbing ?: ternary operators
				int	BOLIndex = FindBOL( _Source, SemanticStart );
				int	EOLIndex = FindEOL( _Source, SemanticStart );

				if ( IsCommentedLine( _Source, SemanticStart ) )
				{	// Skip this line if it's a comment
					CurrentPosition = SemanticStart+1;
					continue;
				}

				// Ensure we're not mistaking this as part of a ternary operator...
				if ( IndexOfBetween( _Source, "?", BOLIndex, SemanticStart ) != -1 )
				{	// Skip that match...
					CurrentPosition = SemanticStart+1;
					continue;
				}

				// Find the ; end marker
				int	SemanticEnd = IndexOfBetween( _Source, ";", SemanticStart, EOLIndex );
				if ( SemanticEnd == -1 )
					throw new ConverterException( "Failed to retrieve closing ; for semantic specification!", _Source, SemanticStart, EOLIndex );

				// Isolate & remove unsupported semantic syntax
				string	Semantic = _Source.Substring( SemanticStart+1, SemanticEnd-SemanticStart-1 );
						Semantic = Semantic.Trim();

				// Ensure it's alphanumeric!
				if ( !EnsureAlphanumeric( Semantic ) )
					throw new ConverterException( "Retrieved semantic \"" + Semantic + "\" is not recognized as an alpha numeric string!", _Source, SemanticStart, SemanticEnd );

				_Source = _Source.Remove( SemanticStart, SemanticEnd-SemanticStart );

				// Replace by inserting a semantic attribute
				string	SemanticAttrib = "[Semantic( \"" + Semantic + "\" )]\n";
				_Source = _Source.Insert( BOLIndex, SemanticAttrib );

				// New position for starting our search...
				CurrentPosition = SemanticStart;
			}

			return _Source;
		}

		/// <summary>
		/// Converts all constant buffers syntax (cbuffer) into a struct with a cbuffer attribute
		/// Builds a dictionary of constant names so we can replace their simple use by a composition cbufferName.ConstantName instead
		/// </summary>
		/// <param name="_Source"></param>
		/// <returns></returns>
		private static string		ConvertConstantBuffers( string _Source )
		{
			int	CurrentPosition = 0;
			while ( true )
			{
				int	CBufferStart = _Source.IndexOf( "cbuffer", CurrentPosition, StringComparison.InvariantCultureIgnoreCase );
				if ( CBufferStart == -1 )
					break;

				int	BOLIndex = FindBOL( _Source, CBufferStart );
				int	EOLIndex = FindEOL( _Source, CBufferStart );

				if ( IsCommentedLine( _Source, CBufferStart ) )
				{	// Skip this line if it's a comment
					CurrentPosition = CBufferStart+1;
					continue;
				}

TODO: get cbuffer name and fields and store into a map to append cbuffer name fo field usage
				
				// Replace by a struct with an attribute
				_Source = _Source.Remove( CBufferStart, "cbuffer".Length );

				string	CBufferAttributePlusStruct = "[cbuffer]\nstruct";
				_Source = _Source.Insert( CBufferStart, CBufferAttributePlusStruct );

				// New position for starting our search...
				CurrentPosition = CBufferStart + CBufferAttributePlusStruct.Length;
			}

			return _Source;
		}

		/// <summary>
		/// Converts the float type to double type
		/// </summary>
		/// <param name="_Source"></param>
		/// <returns></returns>
		private static string		ConvertFloatToDouble( string _Source )
		{
			int	CurrentPosition = 0;
			while( true )
			{
				int	FloatIndex = _Source.IndexOf( "float ", CurrentPosition, StringComparison.InvariantCultureIgnoreCase );
				if ( FloatIndex == -1 )
					FloatIndex = _Source.IndexOf( "float	", CurrentPosition, StringComparison.InvariantCultureIgnoreCase );
				if ( FloatIndex == -1 )
					FloatIndex = _Source.IndexOf( "float(", CurrentPosition, StringComparison.InvariantCultureIgnoreCase );
				if ( FloatIndex == -1 )
					break;

				_Source = _Source.Remove( FloatIndex, "float".Length );
				_Source = _Source.Insert( FloatIndex, "double" );
			}

			return _Source;
		}

		/// <summary>
		/// Converts vector constructors (float2, float3, float4, uint2, uint3, uint4, etc.) by a function that calls actual C# constructors
		/// </summary>
		/// <param name="_Source"></param>
		/// <returns></returns>
		private static string		ConvertVectorConstructors( string _Source )
		{
			_Source = _Source.Replace( "float2(", "_float2(" );
			_Source = _Source.Replace( "float3(", "_float3(" );
			_Source = _Source.Replace( "float4(", "_float4(" );
			_Source = _Source.Replace( "int2(", "_int2(" );
			_Source = _Source.Replace( "int3(", "_int3(" );
			_Source = _Source.Replace( "int4(", "_int4(" );
			_Source = _Source.Replace( "uint2(", "_uint2(" );
			_Source = _Source.Replace( "uint3(", "_uint3(" );
			_Source = _Source.Replace( "uint4(", "_uint4(" );

			return _Source;
		}

		/// <summary>
		/// Converts all functions with an "inout" syntax as "ref" and fix callers of such functions to issue either a "ref" or an "out" prefix
		/// </summary>
		/// <param name="_Source"></param>
		/// <returns></returns>
		private static string		ConvertOutFunctions( string _Source )
		{
			return _Source;	// TODO!
		}

		#region Helpers

		/// <summary>
		/// Comments any line containing the specified pattern
		/// </summary>
		/// <param name="_Source"></param>
		/// <param name="_Pattern"></param>
		/// <returns></returns>
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
				_Source = _Source.Insert( BOLIndex, "// " );

				CurrentPosition = MatchIndex + 3 + 1;
			}

			return _Source;
		}

		/// <summary>
		/// Ensures the provided name is alphanumeric
		/// </summary>
		/// <param name="_Name"></param>
		/// <returns></returns>
		private static bool			EnsureAlphanumeric( string _Name )
		{
			for ( int CharIndex=0; CharIndex < _Name.Length; CharIndex++ )
			{
				char	C = _Name[CharIndex];
				bool	IsAlpha = false;
				IsAlpha |= C >= 'a' && C <= 'z';
				IsAlpha |= C >= 'A' && C <= 'Z';
				IsAlpha |= C >= '0' && C <= '9';
				IsAlpha |= C == '_';
				if ( !IsAlpha )
					return false;
			}

			return true;
		}

		/// <summary>
		/// Checks if the current position is part of a comment (warning: only // coments are supported!)
		/// </summary>
		/// <param name="_Source"></param>
		/// <param name="_Index"></param>
		/// <returns></returns>
		private static bool			IsCommentedLine( string _Source, int _Index )
		{
			int	BOLIndex = FindBOL( _Source, _Index );
			return IndexOfBetween( _Source, "//", BOLIndex, _Index ) != -1;
		}

		/// <summary>
		/// Throws an exception if there is a line with the #if syntax, which is not supported in C# (except very annoying #define at the very beginning of the file)!
		/// </summary>
		/// <param name="_Source"></param>
		/// <param name="_Pattern"></param>
		private static void			ErrorIfLinesWith( string _Source, string _Pattern )
		{
			int	MatchIndex = _Source.IndexOf( _Pattern, StringComparison.InvariantCultureIgnoreCase );
			if ( MatchIndex == -1 )
				return;	// We're safe!

			throw new ConverterException( "Found an unsupported preprocess #if! Please remove the concerned block...", _Source, MatchIndex );
		}

// 		/// <summary>
// 		/// Inserts a snippet of text at a specific index in the source
// 		/// </summary>
// 		/// <param name="_Source"></param>
// 		/// <param name="_Index"></param>
// 		/// <param name="_Insert"></param>
// 		/// <returns></returns>
// 		private static string		Insert( string _Source, int _Index, string _Insert )
// 		{
// 			string	Start = _Source.Substring( 0, _Index );
// 			string	End = _Source.Substring( _Index );
// 			string	Result = Start + _Insert + End;
// 			return Result;
// 		}

		/// <summary>
		/// Finds the index of the beginning of the current line
		/// </summary>
		/// <param name="_Source"></param>
		/// <param name="_StartIndex"></param>
		/// <returns></returns>
		private static int			FindBOL( string _Source, int _StartIndex )
		{
			int	MatchIndex = _Source.LastIndexOf( '\n', Math.Max( 0, _StartIndex-1) );
			return MatchIndex != -1 ? MatchIndex+1 : 0;	// BOF?
		}

		/// <summary>
		/// Finds teh index of the end of the current line
		/// </summary>
		/// <param name="_Source"></param>
		/// <param name="_StartIndex"></param>
		/// <returns></returns>
		private static int			FindEOL( string _Source, int _StartIndex )
		{
			int	MatchIndex = _Source.IndexOf( '\n', _StartIndex );
			return MatchIndex != -1 ? MatchIndex : _Source.Length;	// EOF?
		}

		/// <summary>
		/// Same as IndexOf except it returns -1 if the match is outside of the specified [Start,End] range
		/// </summary>
		/// <param name="_Source"></param>
		/// <param name="_Pattern"></param>
		/// <param name="_StartIndex"></param>
		/// <param name="_EndIndex"></param>
		/// <returns></returns>
		private static int			IndexOfBetween( string _Source, string _Pattern, int _StartIndex, int _EndIndex )
		{
			int	MatchIndex = _Source.IndexOf( _Pattern, _StartIndex, StringComparison.InvariantCultureIgnoreCase );
			return MatchIndex < _EndIndex ? MatchIndex : -1;
		}

		#endregion
	}
}
