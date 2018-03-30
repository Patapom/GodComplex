// This is my solution to the coding challenge at https://www.codingame.com/ide/693616389c7f6a37164da00919ef6f64252d0c9
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    public static void Main(string[] args) {
        int n = int.Parse(Console.ReadLine());

		int	sumDivisors = 1;
		for ( int i=2; i <= n; i++ ) {
//			string	divisors = "";
			for ( int j=1; j <= i; j++ )
				if ( (i % j) == 0 ) {
					sumDivisors += j;
//					divisors += ", " + j;
				}
//Console.Error.WriteLine( "d(" + i + ") = { " + divisors + " }" );
		}

        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");

        Console.WriteLine( sumDivisors );
    }
}