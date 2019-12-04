//////////////////////////////////////////////////////////////////////////
/// This is an adaptation of the iOS LaTeX library by Kostub Deshmukh
/// Initial GitHub project: https://github.com/kostub/iosMath
///
//////////////////////////////////////////////////////////////////////////
///
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpLaTeX
{
	/// <summary>
	/// Converts a LaTeX-formatted string into a list of atoms used internally
	/// Equivalent of the original MTMathListBuilder class in the original library
	/// </summary>
    public class LaTeXStringToAtomsList {

		#region NESTED TYPES

		/// <summary>
		/// The error encountered when parsing a LaTeX string.
		/// The `code` in the `NSError` is one of the following indiciating why the LaTeX string could not be parsed.
		/// </summary>
		public enum ParseError {
			NONE,

			/// The braces { } do not match.
			MTParseErrorMismatchBraces = 1,
			/// A command in the string is not recognized.
			MTParseErrorInvalidCommand,
			/// An expected character such as ] was not found.
			MTParseErrorCharacterNotFound,
			/// The \left or \right command was not followed by a delimiter.
			MTParseErrorMissingDelimiter,
			/// The delimiter following \left or \right was not a valid delimiter.
			MTParseErrorInvalidDelimiter,
			/// There is no \right corresponding to the \left command.
			MTParseErrorMissingRight,
			/// There is no \left corresponding to the \right command.
			MTParseErrorMissingLeft,
			/// The environment given to the \begin command is not recognized
			MTParseErrorInvalidEnv,
			/// A command is used which is only valid inside a \begin,\end environment
			MTParseErrorMissingEnv,
			/// There is no \begin corresponding to the \end command.
			MTParseErrorMissingBegin,
			/// There is no \end corresponding to the \begin command.
			MTParseErrorMissingEnd,
			/// The number of columns do not match the environment
			MTParseErrorInvalidNumColumns,
			/// Internal error, due to a programming mistake.
			MTParseErrorInternalError,
			/// Limit control applied incorrectly
			MTParseErrorInvalidLimits,
		}

		#endregion

		#region FIELDS

		ParseError	m_error = ParseError.NONE;

		#endregion

		#region PROPERTIES

		#endregion

		#region METHODS

		// /** Contains any error that occurred during parsing. */
		// @property (nonatomic, readonly, nullable) NSError* error;
		// 
		// /** Create a `MTMathListBuilder` for the given string. After instantiating the
		//     `MTMathListBuilder, use `build` to build the mathlist. Create a new `MTMathListBuilder`
		//     for each string that needs to be parsed. Do not reuse the object.
		//     @param str The LaTeX string to be used to build the `MTMathList`
		//  */
		// - (nonnull instancetype) initWithString:(nonnull NSString*) str NS_DESIGNATED_INITIALIZER;
		// - (nonnull instancetype) init NS_UNAVAILABLE;
		// 
		// /// Builds a mathlist from the given string. Returns nil if there is an error.
		// - (nullable MTMathList*) build;
		// 
		// /** Construct a math list from a given string. If there is parse error, returns
		//  nil. To retrieve the error use the function `[MTMathListBuilder buildFromString:error:]`.
		//  */
		// + (nullable MTMathList*) buildFromString:(nonnull NSString*) str;
		// 
		// /** Construct a math list from a given string. If there is an error while
		//  constructing the string, this returns nil. The error is returned in the
		//  `error` parameter.
		//  */
		// + (nullable MTMathList*) buildFromString:(nonnull NSString*) str error:( NSError* _Nullable * _Nullable) error;
		// 
		// /// This converts the MTMathList to LaTeX.
		// + (nonnull NSString*) mathListToString:(nonnull MTMathList*) ml;

		#endregion
    }
}
