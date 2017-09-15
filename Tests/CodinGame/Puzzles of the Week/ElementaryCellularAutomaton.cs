// This is my solution to the puzzle of the week https://www.codingame.com/ide/8426873a18efa46247ab073ed4b61f2d8144154
//
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
	static void Main(string[] args) {
		// Decode the rule
		int		rule = int.Parse(Console.ReadLine());
Console.Error.WriteLine( "Rule = " + rule.ToString( "X02" ) );
		bool[]	transforms = new bool[8];
		for ( int caseIndex=0; caseIndex < 8; caseIndex++ ) {
			transforms[caseIndex] = (rule & 1) != 0;
			rule >>= 1;
//Console.Error.WriteLine( "Transform[{0}] = {1}", caseIndex, transforms[caseIndex] );
		}

		int		linesCount = int.Parse(Console.ReadLine());
//Console.Error.WriteLine( "Lines # = " + linesCount );

		// Initialize pattern
		char[]	currentPattern = Console.ReadLine().ToCharArray();
		int		patternSize = currentPattern.Length;
		char[]	previousPattern = new char[patternSize];

//Console.Error.WriteLine( currentPattern );

		// Run automaton
		for ( int lineIndex=0; lineIndex < linesCount; lineIndex++ ) {
			Console.WriteLine( currentPattern );
			char[]	temp = previousPattern;
			previousPattern = currentPattern;
			currentPattern = temp;

			// Apply transform
			for ( int bit=0; bit < patternSize; bit++ ) {
				int	left = previousPattern[(bit + patternSize-1) % patternSize] == '@' ? 4 : 0;
				int	current = previousPattern[bit] == '@' ? 2 : 0;
				int	right = previousPattern[(bit + 1) % patternSize] == '@' ? 1 : 0;
				int	caseIndex = left | current | right;
				currentPattern[bit] = transforms[caseIndex] ? '@' : '.';
			}
		}
	}
}