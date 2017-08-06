using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpMath;
using ImageUtility;

namespace TestArduinoSerialPacketDecoder
{
	class Program
	{
		static void Main(string[] args) {
			DecodeArduinoSerialPacket();
		}

		static void	DecodeArduinoSerialPacket() {
			bool[]	values;
			using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "ArduinoSerialPacket.png" ) ) ) {
				values = new bool[I.Width];
				I.ReadPixels( ( uint _X, uint _Y, ref float4 _color ) => {
					if ( _Y != I.Height/2 )
						return;

					// Detect blue color
					bool	isWhite = _color.x > 0.9f && _color.y > 0.9f && _color.z > 0.9f;
					values[_X] = !isWhite && _color.z > 0.9f;
				} );
			}

			// Compute smallest interval between 2 fronts
			int	size = values.Length;
			int	clockInterval = 1000;
			int	firstChange = -1;
			int	lastChange = -10000;
			for ( int i=0; i < size; i++ ) {
				if ( !values[i] )
					continue;	// Not a front!

				if ( firstChange == -1 )
					firstChange = i;	// Keep first front
				int	interval = i - lastChange;
				if ( interval < clockInterval )
					clockInterval = interval;
				lastChange = i;
			}

			// Start measuring fronts as bits following the detected clock interval (with a tolerance)
			List< bool >	messageFronts = new List< bool >();
			int	X = firstChange;
			while ( X < size ) {
				if ( X+1 >= size )
					break;	// End of message

				bool	value = false;
				if ( values[X-1] ) {
					value = true;
					X = X-1;	// Found a front one pixel before, re-adjust
				}
				if ( values[X] ) {
					value = true;	// Found a front exactly where we expected
				}
				if ( values[X+1] ) {
					value = true;
					X = X+1;	// Found a front one pixel after, re-adjust
				}
				messageFronts.Add( value );

				// Jump ahead of an entire clock interval
				X += clockInterval;
			}

			// Rebuild actual bits from fronts
			List< bool >	messageBits = new List< bool >();
			bool			currentState = true;
			for ( int i=0; i < messageFronts.Count; i++ ) {
				bool	front = messageFronts[i];
				currentState ^= front;
				messageBits.Add( currentState );
			}
			int	messageSize = messageBits.Count;

			// Try various offsets using 10 bits per character
			string[,]	decodes = new string[3,10];
			for ( int initialBitOffset=0; initialBitOffset < 3; initialBitOffset++ ) {
				for ( int messageBitOffset=0; messageBitOffset < 10; messageBitOffset++ ) {

					List< char >	stringAsChars = new List< char >();
					int				charBitIndex = 0;
					int				charValue = 0;
					for ( int i=messageBitOffset+initialBitOffset; i < messageSize; i++ ) {
						bool	bit = messageBits[i];
						charValue |= bit ? (1 << charBitIndex) : 0;
						charBitIndex++;
						if ( charBitIndex == 8 ) {
							// Dump a new complete character
							char	C = (char) charValue;
							stringAsChars.Add( C );
							charBitIndex = 0;
							charValue = 0;
							i += 2;	// Skip what we assume to be parity bits
						}
					}
					decodes[initialBitOffset,messageBitOffset] = new string( stringAsChars.ToArray() );
				}
			}
		}
	}
}
