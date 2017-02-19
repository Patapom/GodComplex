// This is my solution to the Last Crusade Challenge: https://www.codingame.com/ide/puzzle/the-last-crusade-episode-1
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
	/*
The tunnel consists of a patchwork of square rooms of different types.The rooms can be accessed and activated by computer using an ancient RS232 serial port (because Mayans aren't very technologically advanced, as is to be expected...).

There is a total of 14 room types (6 base shapes extended to 14 through rotations).

Upon entering a room, depending on the type of the room and Indy's entrance point (TOP,LEFT, or RIGHT) he will either exit the room through a specific exit point, suffer a lethal collision or lose momentum and get stuck:

Type 0	This room type is not part of the tunnel per se as Indy cannot move across it.

Type 1	The green arrows indicate Indy's possible paths through the room.

Type 2	
Type 3	The room of type 3 is similar to the room of type 2 but rotated.

Type 4	
Type 5	A red arrow indicate a path that Indy cannot use to move across the room.

Type 6	
Type 7	
Type 8	
Type 9

Type 10	
Type 11	
Type 12	
Type 13
Indy is perpetually drawn downwards: he cannot leave a room through the top.

At the start of the game, you are given the map of the tunnel in the form of a rectangular grid of rooms. Each room is represented by its type.

For this first mission, you will familiarize yourself with the tunnel system, the rooms have all been arranged in such a way that Indy will have a safe continuous route between his starting point (top of the temple) and the exit area (bottom of the temple).

Each game turn:
You receive Indy's current position
Then you specify what Indy's position will be next turn.
Indy will then move from the current room to the next according to the shape of the current room.
*/

// Game Input
// 
// The program must first read the initialization data from standard input. Then, within an infinite loop, read the data from the standard input related to the current position of Indy and provide to the standard output the expected data.
// Initialization input
// Line 1: 2 space separated integers W H specifying the width and height of the grid.
// 
// Next H lines: each line represents a line in the grid and contains W space separated integers T. T specifies the type of the room.
// 
// Last line: 1 integer EX specifying the coordinate along the X axis of the exit (this data is not useful for this first mission, it will be useful for the second level of this puzzle).
// 
// Input for one game turn
// Line 1: XI YI POS
// (XI, YI) two integers to indicate Indy's current position on the grid.
// POS a single word indicating Indy's entrance point into the current room: TOP if Indy enters from above, LEFT if Indy enters from the left and RIGHT if Indy enters from the right.
// Output for one game turn
// A single line with 2 integers: X Y representing the (X, Y) coordinates of the room in which you believe Indy will be on the next turn.
// Constraints
// 0 < W ≤ 20
// 0 < H ≤ 20
// 0 ≤ T ≤ 13
// 0 ≤ EX < W
// 0 ≤ XI, X < W
// 0 ≤ YI, Y < H
// 
// Response time for one game ≤ 150ms



// 3 possible entrance points: LEFT, RIGHT or TOP

	class CrashException : Exception {}			// Crashes into a wall or falls hard onto the floor
	class LoseMomentumException : Exception {}	// Stops course as Indy loses momentum
	enum LINK_POINT {
		TOP,
		LEFT,
		RIGHT,
		BOTTOM,
	}

	class Room {
		public int		X, Y;
		public int		m_type;
		public LINK_POINT		Follow( LINK_POINT _entryPoint ) {
			switch ( m_type ) {
	//				case 0:								// Full block: can't use
				case 1: return LINK_POINT.BOTTOM;	// Cross shape: fall down anyway

				// Tube shapes
				case 2:								// Horizontal tube: pass through
					switch ( _entryPoint ) {
						case LINK_POINT.LEFT: return LINK_POINT.RIGHT;		// Pass from left to right
						case LINK_POINT.RIGHT: return LINK_POINT.LEFT;		// Pass from right to left
					}
					break;
				case 3:								// Vertical tube: fall through
					if ( _entryPoint == LINK_POINT.TOP )
						return LINK_POINT.BOTTOM;
					break;

				// 2 bent tubes-shapes
				case 4:								// 2 bent tubes: top=>left, right=>down
					switch ( _entryPoint ) {
						case LINK_POINT.TOP: return LINK_POINT.LEFT;		// Fall from top to left
						case LINK_POINT.RIGHT: return LINK_POINT.BOTTOM;	// Fall from right to bottom
						case LINK_POINT.LEFT: throw new LoseMomentumException();
					}
					break;
				case 5:								// 2 bent tubes: top=>right, left=>down
					switch ( _entryPoint ) {
						case LINK_POINT.TOP: return LINK_POINT.RIGHT;		// Fall from top to right
						case LINK_POINT.LEFT: return LINK_POINT.BOTTOM;		// Fall from left to bottom
						case LINK_POINT.RIGHT: throw new LoseMomentumException();
					}
					break;

				// T-shapes
				case 6:								// T opening to top
					switch ( _entryPoint ) {
						case LINK_POINT.TOP: throw new CrashException();	// Crashes from top
						case LINK_POINT.LEFT: return LINK_POINT.RIGHT;		// Pass from left to right
						case LINK_POINT.RIGHT: return LINK_POINT.LEFT;		// Pass from right to left
					}
					break;
				case 7:								// T opening to right
					switch ( _entryPoint ) {
						case LINK_POINT.TOP: return LINK_POINT.BOTTOM;		// Fall through
						case LINK_POINT.RIGHT: return LINK_POINT.BOTTOM;	// Fall from right to bottom
					}
					break;
				case 8:								// T opening to bottom
					switch ( _entryPoint ) {
						case LINK_POINT.LEFT: return LINK_POINT.BOTTOM;		// Fall from left to bottom
						case LINK_POINT.RIGHT: return LINK_POINT.BOTTOM;	// Fall from right to bottom
					}
					break;
				case 9:								// T opening to left
					switch ( _entryPoint ) {
						case LINK_POINT.TOP: return LINK_POINT.BOTTOM;		// Fall through
						case LINK_POINT.LEFT: return LINK_POINT.BOTTOM;		// Fall from left to bottom
					}
					break;

				// Curve shapes
				case 10:
					switch ( _entryPoint ) {
						case LINK_POINT.TOP: return LINK_POINT.LEFT;
						case LINK_POINT.LEFT: throw new LoseMomentumException();
					}
					break;
				case 11:
					switch ( _entryPoint ) {
						case LINK_POINT.TOP: return LINK_POINT.RIGHT;
						case LINK_POINT.RIGHT: throw new LoseMomentumException();
					}
					break;
				case 12:
					if ( _entryPoint == LINK_POINT.RIGHT ) return LINK_POINT.BOTTOM;
					break;
				case 13:
					if ( _entryPoint == LINK_POINT.LEFT ) return LINK_POINT.BOTTOM;
					break;
			}
			throw new Exception( "Unhandled!" );
		}
	}

	public static void Main(string[] args) {
		string		line = Console.ReadLine(); // represents a line in the grid and contains W integers. Each integer represents one room of a given type.
Console.Error.WriteLine( line );
		string[]	inputs = line.Split(' ');
		int W = int.Parse(inputs[0]); // number of columns.
		int H = int.Parse(inputs[1]); // number of rows.

		Room[,] rooms = new Room[W,H];
		for (int Y=0; Y < H; Y++) {
			line = Console.ReadLine(); // represents a line in the grid and contains W integers. Each integer represents one room of a given type.
Console.Error.WriteLine( line );

			string[]    RoomTypes = line.Split( ' ' );
			for ( int X=0; X < W; X++ )
				rooms[X,Y] = new Room() { X = X, Y = Y, m_type = int.Parse( RoomTypes[X] ) };
		}

		line = Console.ReadLine(); // the coordinate along the X axis of the exit (not useful for this first mission, but must be read).
Console.Error.WriteLine( line );
		int EX = int.Parse(line);

		// game loop
		while (true) {
			line = Console.ReadLine(); // the coordinate along the X axis of the exit (not useful for this first mission, but must be read).
Console.Error.WriteLine( line );
			inputs = line.Split(' ');
			int		XI = int.Parse(inputs[0]);
			int		YI = int.Parse(inputs[1]);
			LINK_POINT	entryPoint;
			switch ( inputs[2] ) {
				case "TOP": entryPoint = LINK_POINT.TOP; break;
				case "LEFT": entryPoint = LINK_POINT.LEFT; break;
				case "RIGHT": entryPoint = LINK_POINT.RIGHT; break;
				default: throw new Exception( "Unexpected entry point type \"" + inputs[2] + "\"!" );
			}

			Room		currentRoom = rooms[XI,YI];
			LINK_POINT	exitPoint = currentRoom.Follow( entryPoint );
Console.Error.WriteLine( "Exiting room through " + exitPoint );

			int		XO = XI;
			int		YO = YI;
			switch ( exitPoint ) {
				case LINK_POINT.LEFT: XO--; break;
				case LINK_POINT.RIGHT: XO++; break;
				case LINK_POINT.BOTTOM: YO++; break;
				default: throw new Exception( "Unsupported exit point!" );	// Can't ever go up!
			}
Console.Error.WriteLine( "New room is (" + XO + ", " + YO + ")" );

			// One line containing the X Y coordinates of the room in which you believe Indy will be on the next turn.
			Console.WriteLine( XO.ToString() + " " + YO.ToString() );
		}
	}
}