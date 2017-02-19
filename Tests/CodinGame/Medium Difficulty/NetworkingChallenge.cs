// This is my solution to the coding challenge at https://www.codingame.com/ide/puzzle/network-cabling
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
	struct Pos {
		public long x, y;
	}
	public static void Main(string[] args) {
		string	line = Console.ReadLine();
//Console.Error.WriteLine( line );
		int     N = int.Parse(line);
		Pos[]   P = new Pos[N];
		long    sumY = 0;
		long    minX = long.MaxValue;
		long    maxX = long.MinValue;
		for ( int i = 0; i < N; i++ ) {
			line = Console.ReadLine();
//Console.Error.WriteLine( line );
			string[] inputs = line.Split(' ');
			P[i].x = long.Parse(inputs[0]);
			P[i].y = long.Parse(inputs[1]);
			sumY += P[i].y;
			minX = Math.Min( minX, P[i].x );
			maxX = Math.Max( maxX, P[i].x );
Console.Error.WriteLine( "(" + P[i].x + ", " + P[i].y + ")" );
		}
		float	avgY = (float) sumY / N;
//		long	cableY = (long) Math.Round( avgY );
//		long	cableY = sumY / N;
		long	cableY = (long) Math.Floor( avgY );
// 		long	cableY = (long) Math.Ceiling( avgY );
// 		long	cableY = (long) Math.Round( avgY );

//cableY = -3;

Console.Error.WriteLine( "Average Y = " + avgY + " - Cable Y = " + cableY );

		// Compute total horizontal cable length
		long	cableLength_Horizontal = maxX - minX;
Console.Error.WriteLine( "MinX = " + minX + " MaxX = " + maxX + " - Horizontal Length = " + cableLength_Horizontal );

		// Compute total vertical cable length
		long	cableLength0 = 0;
		long	cableLength1 = 0;
		long	cableLength2 = 0;
		for ( int i=0; i < N; i++ ) {
			cableLength0 += Math.Abs( P[i].y - cableY );
			cableLength1 += Math.Abs( P[i].y - cableY - 1 );	// Cable 1 up from average
			cableLength2 += Math.Abs( P[i].y - cableY + 1 );	// Cable 1 down from average
Console.Error.WriteLine( "Vertical cable length #" + i + " = " + Math.Abs( P[i].y - cableY ) + ", " + Math.Abs( P[i].y - cableY - 1 ) + ", " + Math.Abs( P[i].y - cableY + 1 ) );
		}
//		long	cableLength_Vertical = cableLength0;
		long	cableLength_Vertical = Math.Min( cableLength0, Math.Min( cableLength1, cableLength2 ) );
Console.Error.WriteLine( "Sum minimum vertical cable lengths = " + cableLength_Vertical );

		long    cableLength = cableLength_Horizontal + cableLength_Vertical;
Console.Error.WriteLine( "Cable length = " + cableLength );

		Console.WriteLine( cableLength.ToString() );
	}
}