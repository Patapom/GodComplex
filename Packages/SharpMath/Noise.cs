using System;
using System.Collections.Generic;
using System.Text;

namespace WMath
{
	/// <summary>
	/// A simple implementation of gradient noise from Ken Perlin
	/// </summary>
	public class	Noise
	{
		#region CONSTANT

		protected const int			NOISE_BIAS = 1024;

		#endregion

		#region FIELDS

		protected uint					m_NoiseSize = 0;
		protected WMath.Vector4D[]		m_NoiseTable = null;
		protected uint[]				m_PermutationTable = null;

		#endregion

		#region PROPERTIES

		public uint		NoiseSize		{ get { return m_NoiseSize; } }

		#endregion

		#region METHODS

		public	Noise()
		{
		}

		/// <summary>
		/// Initializes the noise table
		/// </summary>
		/// <param name="_TableSize"></param>
		/// <param name="_GradientSeed"></param>
		/// <param name="_PermutationSeed"></param>
		public void		Init( int _TableSize, int _GradientSeed, int _PermutationSeed )
		{
			m_NoiseSize = (uint) _TableSize;
			Random	GradientRNG = new Random( _GradientSeed );
			Random	PermutationRNG = new Random( _PermutationSeed );

			// Build the noise & permutation tables
			m_NoiseTable = new WMath.Vector4D[2*m_NoiseSize];
			m_PermutationTable = new uint[2*m_NoiseSize];

			for ( uint SlotIndex=0; SlotIndex < m_NoiseSize; SlotIndex++ )
			{
				m_NoiseTable[SlotIndex] = m_NoiseTable[m_NoiseSize+SlotIndex] = new WMath.Vector4D(	2.0f * (float) GradientRNG.NextDouble() - 1.0f,
																									2.0f * (float) GradientRNG.NextDouble() - 1.0f,
																									2.0f * (float) GradientRNG.NextDouble() - 1.0f,
																									2.0f * (float) GradientRNG.NextDouble() - 1.0f );
				m_PermutationTable[SlotIndex] = SlotIndex;
			}

			// Mix the permutation table by exchanging indices at random
			for ( uint SlotIndex=0; SlotIndex < m_NoiseSize; SlotIndex++ )
			{
				uint	RandomIndex = (uint) PermutationRNG.Next( (int) m_NoiseSize );

				uint	Temp = m_PermutationTable[RandomIndex];
				m_PermutationTable[RandomIndex] = m_PermutationTable[SlotIndex];
				m_PermutationTable[SlotIndex] = Temp;
			}

			// Finalize the permutation table by doubling its size
			for ( uint SlotIndex=0; SlotIndex < m_NoiseSize; SlotIndex++ )
				m_PermutationTable[m_NoiseSize + SlotIndex] = m_PermutationTable[SlotIndex];
		}

		/// <summary>
		/// Gets a 4D noise
		/// </summary>
		/// <param name="_Position"></param>
		/// <returns></returns>
		public float	GetNoise( WMath.Point4D _Position )
		{
			float	x = _Position.x + NOISE_BIAS;
			float	y = _Position.y + NOISE_BIAS;
			float	z = _Position.z + NOISE_BIAS;
			float	w = _Position.w + NOISE_BIAS;

			uint	FloorX = (uint) Math.Floor( x );
			float	rx = x - FloorX;
			float	rxc = rx - 1.0f;

			uint	FloorY = (uint) Math.Floor( y );
			float	ry = y - FloorY;
			float	ryc = ry - 1.0f;

			uint	FloorZ = (uint) Math.Floor( z );
			float	rz = z - FloorZ;
			float	rzc = rz - 1.0f;

			uint	FloorW = (uint) Math.Floor( w );
			float	rw = w - FloorW;
			float	rwc = rw - 1.0f;

			uint	dwBoundX0 = FloorX % m_NoiseSize;
			uint	dwBoundX1 = (dwBoundX0 + 1) % m_NoiseSize;
			uint	dwBoundY0 = FloorY % m_NoiseSize;
			uint	dwBoundY1 = (dwBoundY0 + 1) % m_NoiseSize;
			uint	dwBoundZ0 = FloorZ % m_NoiseSize;
			uint	dwBoundZ1 = (dwBoundZ0 + 1) % m_NoiseSize;
			uint	dwBoundW0 = FloorW % m_NoiseSize;
			uint	dwBoundW1 = (dwBoundW0 + 1) % m_NoiseSize;

			float	sx = SCurve( rx );
			float	sy = SCurve( ry );
			float	sz = SCurve( rz );
			float	sw = SCurve( rw );

			float	f0000 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX0]+dwBoundY0]+dwBoundZ0]+dwBoundW0], rx,  ry,  rz,  rw  );
			float	f1000 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX1]+dwBoundY0]+dwBoundZ0]+dwBoundW0], rxc, ry,  rz,  rw  );
			float	f1100 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX1]+dwBoundY1]+dwBoundZ0]+dwBoundW0], rxc, ryc, rz,  rw  );
			float	f0100 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX0]+dwBoundY1]+dwBoundZ0]+dwBoundW0], rx,  ryc, rz,  rw  );
			float	f0010 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX0]+dwBoundY0]+dwBoundZ1]+dwBoundW0], rx,  ry,  rzc, rw  );
			float	f1010 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX1]+dwBoundY0]+dwBoundZ1]+dwBoundW0], rxc, ry,  rzc, rw  );
			float	f1110 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX1]+dwBoundY1]+dwBoundZ1]+dwBoundW0], rxc, ryc, rzc, rw  );
			float	f0110 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX0]+dwBoundY1]+dwBoundZ1]+dwBoundW0], rx,  ryc, rzc, rw  );
			float	f0001 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX0]+dwBoundY0]+dwBoundZ0]+dwBoundW1], rx,  ry,  rz,  rwc );
			float	f1001 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX1]+dwBoundY0]+dwBoundZ0]+dwBoundW1], rxc, ry,  rz,  rwc );
			float	f1101 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX1]+dwBoundY1]+dwBoundZ0]+dwBoundW1], rxc, ryc, rz,  rwc );
			float	f0101 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX0]+dwBoundY1]+dwBoundZ0]+dwBoundW1], rx,  ryc, rz,  rwc );
			float	f0011 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX0]+dwBoundY0]+dwBoundZ1]+dwBoundW1], rx,  ry,  rzc, rwc );
			float	f1011 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX1]+dwBoundY0]+dwBoundZ1]+dwBoundW1], rxc, ry,  rzc, rwc );
			float	f1111 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX1]+dwBoundY1]+dwBoundZ1]+dwBoundW1], rxc, ryc, rzc, rwc );
			float	f0111 = Dot( m_NoiseTable[m_PermutationTable[m_PermutationTable[m_PermutationTable[dwBoundX0]+dwBoundY1]+dwBoundZ1]+dwBoundW1], rx,  ryc, rzc, rwc );

			float	a = TriLerp( f0000, f1000, f1100, f0100, f0010, f1010, f1110, f0110, sx, sy, sz );
			float	b = TriLerp( f0001, f1001, f1101, f0101, f0011, f1011, f1111, f0111, sx, sy, sz );
			return	Lerp( a, b, sw );
		}

		protected float	SCurve_OLD( float _t )
		{
			return	_t * _t * (3.0f - 2.0f * _t);		// 3 t^2 - 2 t^3  ==> Gives some sort of S-Shaped curve with 0 first derivatives at t=0 & t=1
		}

		protected float	SCurve( float _t )
		{
			return	_t * _t * _t * (10.0f + _t * (-15.0f + _t *  6.0f));	// 6 t^5 - 15 t^4 + 10 t^3  ==> Gives some sort of S-Shaped curve with 0 first and second derivatives at t=0 & t=1
		}

		protected float	Dot( WMath.Vector4D _Op0, float _Op1x, float _Op1y, float _Op1z, float _Op1w )
		{
			return	_Op0.x * _Op1x + _Op0.y * _Op1y + _Op0.z * _Op1z + _Op0.w * _Op1w;
		}

		// Linear, Bilinear and Trilinear interpolation functions.
		// The cube is designed like this:
		//                                                                  
		//        p3         p2                                             
		//         o--------o                                               
		//        /:       /|          Y
		//     p7/ :    p6/ |          |                                    
		//      o--------o  |          |                                    
		//      |  :p0   |  |p1        |                                    
		//      |  o.....|..o          o------X
		//      | '      | /          /                                     
		//      |'       |/          /                                      
		//      o--------o          Z
		//     p4        p5                                                 
		//                                                                  
		protected float	TriLerp( float _p0, float _p1, float _p2, float _p3, float _p4, float _p5, float _p6, float _p7, float _x, float _y, float _z )
		{
			return	Lerp( BiLerp( _p0, _p1, _p2, _p3, _x, _y ), BiLerp( _p4, _p5, _p6, _p7, _x, _y ), _z );
		}

		protected float	BiLerp( float _p0, float _p1, float _p2, float _p3, float _x, float _y )
		{
			return	Lerp( Lerp( _p0, _p1, _x ), Lerp( _p3, _p2, _x ), _y );
		}

		protected float	Lerp( float _p0, float _p1, float _x )
		{
			return	_p0 + (_p1 - _p0) * _x;
		}

		#endregion
	}
}
