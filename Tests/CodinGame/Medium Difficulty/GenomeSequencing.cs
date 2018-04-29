// This is my solution to the challenge https://www.codingame.com/ide/puzzle/genome-sequencing
//
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {

	// Sorts strings by length
	class Comparer : IComparer<string> {

		#region IComparer<string> Members

		public int Compare( string x, string y ) {
 			return Comparer<int>.Default.Compare( x.Length, y.Length );
		}

		#endregion

	}

    static void Main(string[] args) {
		List< string >	sequences = new List<string>();
        int N = int.Parse(Console.ReadLine());
        for (int i = 0; i < N; i++) {
            string subSequence = Console.ReadLine();
Console.Error.WriteLine( subSequence );
			sequences.Add( subSequence );
        }

		Comparer	comp = new Comparer();
		while ( sequences.Count > 1 ) {
			// Sort from shortest to longest sequence
			sequences.Sort( comp );

			// Try and insert the shortest sequence into any other sequence and count the amount of characters we gain each time
			string	shortestSequence = sequences[0];
Console.Error.Write( "Merging " + shortestSequence );

			string	bestSequence = null;
			int		bestSequenceIndex = -1;
			int		bestSavedCharactersCount = -1;
			for ( int otherSequenceIndex=1; otherSequenceIndex < sequences.Count; otherSequenceIndex++ ) {
				string	otherSequence = sequences[otherSequenceIndex];
				int		savedCharactersCount;
				string	mergedSequence = Insert( shortestSequence, otherSequence, out savedCharactersCount );
				if ( savedCharactersCount > bestSavedCharactersCount ) {
					// We found an even better insertion!
					bestSequence = mergedSequence;
					bestSequenceIndex = otherSequenceIndex;
					bestSavedCharactersCount = savedCharactersCount;
				}
			}

			if ( bestSequence != null ) {
				// Replace shortest sequence by its merged version
Console.Error.WriteLine( " with " + sequences[bestSequenceIndex] + " => " + bestSequence );
				sequences[bestSequenceIndex] = bestSequence;
				sequences.RemoveAt( 0 );
			}
		}

        Console.WriteLine( sequences[0].Length );
    }

	/// <summary>
	/// Attempts to insert the first sequence into the second one and returns the merged sequence and the amount of characters the merge saves
	/// </summary>
	/// <param name="_sequence0"></param>
	/// <param name="_sequence1"></param>
	/// <param name="_savedCharactersCount"></param>
	/// <returns></returns>
	static string	Insert( string _sequence0, string _sequence1, out int _savedCharactersCount ) {
		int	L0 = _sequence0.Length;
		int	L1 = _sequence1.Length;

		// Make sequence0 "scroll" into sequence1
		int	bestCommonCharactersCount = 0;
		int	bestInsertionPosition = -L0;
		for ( int insertionPosition = 1-L0; insertionPosition < L1; insertionPosition++ ) {
			// Mark common characters in both sequences
			int	commonCharactersCount = 0;
			for ( int i=0; i < L0; i++ ) {
				int	j = insertionPosition + i;
				if ( j < 0 || j >= L1 )
					continue;	// Outside of sequence1, no need to compare

				char	C0 = _sequence0[i];
				char	C1 = _sequence1[j];
				if ( C0 != C1 ) {
					// No match!
					commonCharactersCount = -1;	// Cancel any previous matches
					break;
				}

				commonCharactersCount++;
			}

			if ( commonCharactersCount > bestCommonCharactersCount ) {
				bestCommonCharactersCount = commonCharactersCount;
				bestInsertionPosition = insertionPosition;
			}
		}

		// Insert at best place
		_savedCharactersCount = bestCommonCharactersCount;
		string	result = "";
		if ( bestInsertionPosition < 0 )
			result += _sequence0.Substring( 0, L0 - _savedCharactersCount );						// Insert a part at the beginning
		result += _sequence1;																		// Insert the entire 2nd sequence
		if ( bestInsertionPosition > L1 - L0 )
			result += _sequence0.Substring( _savedCharactersCount, L0 - _savedCharactersCount );	// Insert a part at the end

		return result;
	}
}