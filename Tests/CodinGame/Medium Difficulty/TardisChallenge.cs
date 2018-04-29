// This is my solution to the coding challenge at https://www.codingame.com/ide/puzzle/the-gift
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
	// Computes the ideal target price for each participant
	static int	ComputeTargetAverage( int _remainingPrice, int _remainingParticipants ) {
		if ( _remainingParticipants == 0 )
			return 0;

// 		return (int) Math.Ceiling( (float) _remainingPrice / _remainingParticipants );
		return _remainingPrice / _remainingParticipants;
	}

    static void Main(string[] args) {
        int N = int.Parse(Console.ReadLine());
        List<int>   budgets = new List<int>( N );
        int price = int.Parse(Console.ReadLine());
Console.Error.WriteLine( "Price = " + price );

        for (int i = 0; i < N; i++) {
            int b = int.Parse(Console.ReadLine());
            budgets.Add( b );
        }
//		budgets.Sort( ( int a, int b ) => { return a < b ? 1 : a > b ? -1 : 0; } );
		budgets.Sort();
foreach ( int B in budgets ) {
	Console.Error.WriteLine( "Sorted Budget = " + B );
}

		List<int>   results = new List<int>();
// 		for ( int i=0; i < N; i++ ) {
// 			int b = budgets[i];
// 			int v = Math.Min( b, price );
// 			results.Add( v );
// Console.Error.WriteLine( "Budget #" + i + " = " + b + " => " + price + " - " + v + " = " + (price-v) );
// 			price -= v;
// 		}

		int	participantsCount = N;
		int	targetAverage = ComputeTargetAverage( price, participantsCount );
Console.Error.WriteLine( "Start average target = " + targetAverage );
		for ( int i=0; i < N; i++ ) {
			int	b = budgets[i];
			int	v = Math.Min( b, targetAverage );
			results.Add( v );
Console.Error.WriteLine( "Budget #" + i + " = " + b + " => " + price + " - " + v + " = " + (price-v) );
			price -= v;
			participantsCount--;
			targetAverage = ComputeTargetAverage( price, participantsCount );
Console.Error.WriteLine( "New average target = " + targetAverage );
		}

        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");

        if ( price != 0 )
            Console.WriteLine( "IMPOSSIBLE");
        else {
            for ( int i=0; i < N; i++ )
                Console.WriteLine( results[i] );
        }
    }
}