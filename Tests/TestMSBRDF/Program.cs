using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestMSBRDF
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			byte[]	entriesCountAND = new byte[256];
			byte[]	entriesCountFibonacci = new byte[256];
			Random	RNG = new Random( 1 );
			for ( uint i=0; i < 10000; i++ ) {
				uint	key = (uint) RNG.Next();
				entriesCountAND[key & 0xFF]++;
				entriesCountFibonacci[Fibonacci( key )]++;
			}

			float	avg = 10000 / 256.0f;
			float	varAnd = 0.0f, varFibo = 0.0f;
			for ( int i=0; i < 256; i++ )
			{
				varAnd += (entriesCountAND[i] - avg)*(entriesCountAND[i] - avg);
				varFibo += (entriesCountFibonacci[i] - avg)*(entriesCountFibonacci[i] - avg);
			}
			varAnd /= 256.0f;
			varAnd = (float)Math.Sqrt( varAnd );
			varFibo /= 256.0f;
			varFibo = (float)Math.Sqrt( varFibo );


			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );
			Application.Run( new TestForm() );
		}

		static uint	Fibonacci( uint _key ) {
			const int	m_POT = 8;
			_key ^= _key >> (32-m_POT);
			uint	index = (uint) ( (_key * 2654435769U) >> (32-m_POT) );	// 2654435769 = 2^32 / Phi
			return index;
		}
	}
}
