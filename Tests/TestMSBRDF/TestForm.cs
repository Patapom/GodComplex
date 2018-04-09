//#define PRECOMPUTE_BRDF	// Define this to precompute the BRDF, comment to only load it

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;
using Renderer;
using System.IO;
using Nuaj.Cirrus.Utility;

namespace TestMSBRDF
{
	public partial class TestForm : Form
	{
		#region CONSTANTS

		const int	VOLUME_SIZE = 128;
		const int	HEIGHTMAP_SIZE = 128;
		const int	NOISE_SIZE = 64;
		const int	PHOTONS_COUNT = 128 * 1024;

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Global {
			public float4		_ScreenSize;
			public float		_Time;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Camera {
			public float4x4		_Camera2World;
			public float4x4		_World2Camera;
			public float4x4		_Proj2World;
			public float4x4		_World2Proj;
			public float4x4		_Camera2Proj;
			public float4x4		_Proj2Camera;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Render {
			public float		_roughness;
			public float		_albedo;
			public float		_lightElevation;
		}

		#endregion

		#region FIELDS

		private Device						m_device = new Device();

		Texture3D							m_Tex_Noise;
		Texture3D							m_Tex_Noise4D;

		Texture2D							m_tex_IrradianceComplement;
		Texture2D							m_tex_IrradianceAverage;

		Shader								m_shader_Render;

		Camera								m_Camera = new Camera();
		CameraManipulator					m_Manipulator = new CameraManipulator();

		ConstantBuffer<CB_Global>			m_CB_Global;
		ConstantBuffer<CB_Camera>			m_CB_Camera;
		ConstantBuffer<CB_Render>			m_CB_Render;


		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_StopWatch = new System.Diagnostics.Stopwatch();
		private double						m_Ticks2Seconds;
		public float						m_StartGameTime = 0;
		public float						m_CurrentGameTime = 0;
		public float						m_StartFPSTime = 0;
		public int							m_SumFrames = 0;
		public float						m_AverageFrameTime = 0.0f;

		#endregion

		public TestForm()
		{
			InitializeComponent();

//TestCross5D();
//TestCross7D();
TestCrossGeneric();

// 			// Build 8 random rotation matrices
// 			string[]	randomRotations = new string[8];
// 			Random	RNG = new Random( 1 );
// 			for ( int i=0; i < 8; i++ ) {
// 				WMath.Matrix3x3	rot = new WMath.Matrix3x3();
// 				rot.FromEuler( new WMath.Vector( (float) RNG.NextDouble(), (float) RNG.NextDouble(), (float) RNG.NextDouble() ) );
// 				randomRotations[i] = rot.ToString();
// 			}

			Application.Idle += new EventHandler( Application_Idle );
		}

#region 5D Cross Product

void	TestCross5D() {
	int[][]	derangements = new int[3][] {
		new int[4] { 1, 0, 3, 2 },		// Couple {x,y} and {z,w}
		new int[4] { 2, 3, 0, 1 },		// Couple {x,z} and {y,w}
		new int[4] { 3, 2, 1, 0 },		// Couple {x,w} and {y,z}
	};
	int[][]	coupless = new int[3][] {
		new int[4] { 1, -1, 2, -2 },	// Couple {x,y} and {z,w}
		new int[4] { 1, 2, -1, -2 },	// Couple {x,z} and {y,w}
		new int[4] { 1, 2, -2, -1 },	// Couple {x,w} and {y,z}
	};

	int[,]	permutations = new int[5,4] {
		{ 1, 2, 3, 4 },					// Row x uses yzwt
		{ 2, 3, 4, 0 },					// Row y uses zwtx
		{ 3, 4, 0, 1 },					// Row z uses wtxy
		{ 4, 0, 1, 2 },					// Row w uses txyz
		{ 0, 1, 2, 3 },					// Row t uses xyzw
	};

	int[]	signs = new int[4];

	List< Tuple<int,int> >	solutionsU = new List<Tuple<int, int>>();
	List< Tuple<int,int> >	solutionsV = new List<Tuple<int, int>>();
	List< Tuple<int,int> >	solutions = new List<Tuple<int, int>>();

	// Try all possible derangements
	for ( int i=0; i < 3; i++ ) {
		int[]	derangement = derangements[i];
		int[]	couples = coupless[i];

		// Try all possible sign combinations
		for ( int s=0; s < 4; s++ ) {
			int	s0 = (s & 1) != 0 ? 1 : -1;
			int	s1 = (s & 2) != 0 ? 1 : -1;
			signs[0] = (couples[0] < 0 ? -1 : 1) * (Math.Abs(couples[0]) == 1 ? s0 : s1);
			signs[1] = (couples[1] < 0 ? -1 : 1) * (Math.Abs(couples[1]) == 1 ? s0 : s1);
			signs[2] = (couples[2] < 0 ? -1 : 1) * (Math.Abs(couples[2]) == 1 ? s0 : s1);
			signs[3] = (couples[3] < 0 ? -1 : 1) * (Math.Abs(couples[3]) == 1 ? s0 : s1);

			// Estimate each line of w.(u x v)
			Array.Clear( m_sumTripletsU, 0, 5*5*5 );
			Array.Clear( m_sumTripletsV, 0, 5*5*5 );
			for ( int d=0; d < 5; d++ ) {
				int	index_w_0 = d;
				int	index_u_0 = permutations[d,0];
				int	index_v_0 = permutations[d,derangement[0]];
				int	sign_0 = signs[0];
				AddTripletU( index_w_0, index_u_0, index_v_0, sign_0 );
				AddTripletV( index_w_0, index_u_0, index_v_0, sign_0 );

				int	index_w_1 = d;
				int	index_u_1 = permutations[d,1];
				int	index_v_1 = permutations[d,derangement[1]];
				int	sign_1 = signs[1];
				AddTripletU( index_w_1, index_u_1, index_v_1, sign_1 );
				AddTripletV( index_w_1, index_u_1, index_v_1, sign_1 );

				int	index_w_2 = d;
				int	index_u_2 = permutations[d,2];
				int	index_v_2 = permutations[d,derangement[2]];
				int	sign_2 = signs[2];
				AddTripletU( index_w_2, index_u_2, index_v_2, sign_2 );
				AddTripletV( index_w_2, index_u_2, index_v_2, sign_2 );

				int	index_w_3 = d;
				int	index_u_3 = permutations[d,3];
				int	index_v_3 = permutations[d,derangement[3]];
				int	sign_3 = signs[3];
				AddTripletU( index_w_3, index_u_3, index_v_3, sign_3 );
				AddTripletV( index_w_3, index_u_3, index_v_3, sign_3 );
			}

			// Check if the sum only contains zeroes
			bool	allZeroesU = true;
			bool	allZeroesV = true;
			for ( int j=0; j < 5*5*5; j++ ) {
				int	z = j;
				int	x = z / (5*5);
				z -= 5*5 * x;
				int	y = z / 5;
				z -= 5* y;

				int	sumU = m_sumTripletsU[x,y,z];
				int	sumV = m_sumTripletsV[x,y,z];
				if ( sumU != 0 ) {
//					if ( Math.Abs( sumU ) > 1 ) throw new Exception( "Multiple adds with same sign!" );
					allZeroesU = false;
//					break;
				}
				if ( sumV != 0 ) {
//					if ( Math.Abs( sumU ) > 1 ) throw new Exception( "Multiple adds with same sign!" );
					allZeroesV = false;
//					break;
				}
			}
			if ( allZeroesU && allZeroesV ) {
				solutions.Add( new Tuple<int,int>( i, s ) );
			} else if ( allZeroesU ) {
				solutionsU.Add( new Tuple<int,int>( i, s ) );
			} else if ( allZeroesV ) {
				solutionsV.Add( new Tuple<int,int>( i, s ) );
			}
		}
	}

	if ( solutions.Count == 0 )
		throw new Exception( "No solution!" );
}

int[,,]	m_sumTripletsU = new int[5,5,5];
void	AddTripletU( int _u0, int _u1, int _v, int _sign ) {
	if ( _u0 == _u1 || _u1 == _v ) throw new Exception(  "Can't have identical indices!" );
	m_sumTripletsU[_u0, _u1, _v] += _sign;
	m_sumTripletsU[_u1, _u0, _v] += _sign;
}
int[,,]	m_sumTripletsV = new int[5,5,5];
void	AddTripletV( int _v0, int _u, int _v1, int _sign ) {
	if ( _v0 == _u || _u == _v1 ) throw new Exception(  "Can't have identical indices!" );
	m_sumTripletsV[_v0, _u, _v1] += _sign;
	m_sumTripletsV[_v1, _u, _v0] += _sign;
}

#endregion

#region 7D Cross Product

void	TestCross7D() {
	// Manual creation of the 5*3 possible valid derangements
	const int	x=0, y=1, z=2, w=3, s=4, t=5;
	int[][]	derangements = new int[5*3][] {
		new int[6] { y, x, w, z, t, s },
		new int[6] { y, x, s, t, z, w },
		new int[6] { y, x, t, s, w, z },

		new int[6] { z, w, x, y, t, s },
		new int[6] { z, s, x, t, y, w },
		new int[6] { z, t, x, s, w, y },	// SOLUTION #1!

		new int[6] { w, z, y, x, t, s },
		new int[6] { w, s, t, x, y, z },
		new int[6] { w, t, s, x, z, y },

		new int[6] { s, z, y, t, x, w },	// SOLUTION #2!
		new int[6] { s, w, t, y, x, z },
		new int[6] { s, t, w, z, x, y },

		new int[6] { t, z, y, s, w, x },
		new int[6] { t, w, s, y, z, x },
		new int[6] { t, s, w, z, y, x },
	};
	int[][]	coupless = new int[5*3][];
	for ( int i=0; i < derangements.Length; i++ ) {		// Automate couples creation to avoid mistakes...
		int[]	couples = new int[6];
		coupless[i] = couples;
		int	coupleIndex = 1;
		for ( int j=0; j < 6; j++ ) {
			if ( couples[j] == 0 ) {
				couples[j] = coupleIndex;
				couples[derangements[i][j]] = -coupleIndex;
				coupleIndex++;
			}
		}
	}

	int[,]	permutations = new int[7,6] {
		{ 1, 2, 3, 4, 5, 6 },			// Row i uses jklmno
		{ 2, 3, 4, 5, 6, 0 },			// Row j uses klmnoi
		{ 3, 4, 5, 6, 0, 1 },			// Row k uses lmnoij
		{ 4, 5, 6, 0, 1, 2 },			// Row l uses mnoijk
		{ 5, 6, 0, 1, 2, 3 },			// Row m uses noijkl
		{ 6, 0, 1, 2, 3, 4 },			// Row n uses oijklm
		{ 0, 1, 2, 3, 4, 5 },			// Row o uses ijklmn
	};

	int[]	signs = new int[6];

	List< Tuple<int,int> >	solutionsU = new List<Tuple<int, int>>();
	List< Tuple<int,int> >	solutionsV = new List<Tuple<int, int>>();
	List< Tuple<int,int> >	solutions = new List<Tuple<int, int>>();

	// Try all possible derangements
	for ( int i=0; i < derangements.Length;i++ ) {
		int[]	derangement = derangements[i];
		int[]	couples = coupless[i];

		// Try all possible sign combinations
		for ( int sign=0; sign < 8; sign++ ) {
			int[]	signBits = new int[] {
				(sign & 1) != 0 ? -1 : 1,
				(sign & 2) != 0 ? -1 : 1,
				(sign & 4) != 0 ? -1 : 1
			};
			signs[0] = (couples[0] < 0 ? -1 : 1) * signBits[Math.Abs(couples[0]) - 1];
			signs[1] = (couples[1] < 0 ? -1 : 1) * signBits[Math.Abs(couples[1]) - 1];
			signs[2] = (couples[2] < 0 ? -1 : 1) * signBits[Math.Abs(couples[2]) - 1];
			signs[3] = (couples[3] < 0 ? -1 : 1) * signBits[Math.Abs(couples[3]) - 1];
			signs[4] = (couples[4] < 0 ? -1 : 1) * signBits[Math.Abs(couples[4]) - 1];
			signs[5] = (couples[5] < 0 ? -1 : 1) * signBits[Math.Abs(couples[5]) - 1];

			// Estimate each line of w.(u x v)
			Array.Clear( m_sumTripletsU7D, 0, 7*7*7 );
			Array.Clear( m_sumTripletsV7D, 0, 7*7*7 );
			for ( int d=0; d < 7; d++ ) {
				int	index_w_0 = d;
				int	index_u_0 = permutations[d,0];
				int	index_v_0 = permutations[d,derangement[0]];
				int	sign_0 = signs[0];
				AddTripletU7D( index_w_0, index_u_0, index_v_0, sign_0 );
				AddTripletV7D( index_w_0, index_u_0, index_v_0, sign_0 );

				int	index_w_1 = d;
				int	index_u_1 = permutations[d,1];
				int	index_v_1 = permutations[d,derangement[1]];
				int	sign_1 = signs[1];
				AddTripletU7D( index_w_1, index_u_1, index_v_1, sign_1 );
				AddTripletV7D( index_w_1, index_u_1, index_v_1, sign_1 );

				int	index_w_2 = d;
				int	index_u_2 = permutations[d,2];
				int	index_v_2 = permutations[d,derangement[2]];
				int	sign_2 = signs[2];
				AddTripletU7D( index_w_2, index_u_2, index_v_2, sign_2 );
				AddTripletV7D( index_w_2, index_u_2, index_v_2, sign_2 );

				int	index_w_3 = d;
				int	index_u_3 = permutations[d,3];
				int	index_v_3 = permutations[d,derangement[3]];
				int	sign_3 = signs[3];
				AddTripletU7D( index_w_3, index_u_3, index_v_3, sign_3 );
				AddTripletV7D( index_w_3, index_u_3, index_v_3, sign_3 );

				int	index_w_4 = d;
				int	index_u_4 = permutations[d,4];
				int	index_v_4 = permutations[d,derangement[4]];
				int	sign_4 = signs[4];
				AddTripletU7D( index_w_4, index_u_4, index_v_4, sign_4 );
				AddTripletV7D( index_w_4, index_u_4, index_v_4, sign_4 );

				int	index_w_5 = d;
				int	index_u_5 = permutations[d,5];
				int	index_v_5 = permutations[d,derangement[5]];
				int	sign_5 = signs[5];
				AddTripletU7D( index_w_5, index_u_5, index_v_5, sign_5 );
				AddTripletV7D( index_w_5, index_u_5, index_v_5, sign_5 );
			}

			// Check if the sum only contains zeroes
			bool	allZeroesU = true;
			bool	allZeroesV = true;
			for ( int j=0; j < 7*7*7; j++ ) {
				int	Z = j;
				int	X = Z / (7*7);
				Z -= 7*7 * X;
				int	Y = Z / 7;
				Z -= 7 * Y;

				int	sumU = m_sumTripletsU7D[X,Y,Z];
				int	sumV = m_sumTripletsV7D[X,Y,Z];
				if ( sumU != 0 ) {
//					if ( Math.Abs( sumU ) > 1 ) throw new Exception( "Multiple adds with same sign!" );
					allZeroesU = false;
//					break;
				}
				if ( sumV != 0 ) {
//					if ( Math.Abs( sumU ) > 1 ) throw new Exception( "Multiple adds with same sign!" );
					allZeroesV = false;
//					break;
				}
			}
			if ( allZeroesU && allZeroesV ) {
				solutions.Add( new Tuple<int,int>( i, sign ) );
			} else if ( allZeroesU ) {
				solutionsU.Add( new Tuple<int,int>( i, sign ) );
			} else if ( allZeroesV ) {
				solutionsV.Add( new Tuple<int,int>( i, sign ) );
			}
		}
	}

	if ( solutions.Count == 0 )
		throw new Exception( "No solution!" );

// Write latex code expanding the 1st solution
	{
		int		i = solutions[0].Item1;
		int		sign = solutions[0].Item2;
		int[]	derangement = derangements[i];
		int[]	couples = coupless[i];
		int[]	signBits = new int[] {
			(sign & 1) != 0 ? -1 : 1,
			(sign & 2) != 0 ? -1 : 1,
			(sign & 4) != 0 ? -1 : 1
		};
		signs[0] = (couples[0] < 0 ? -1 : 1) * signBits[Math.Abs(couples[0]) - 1];
		signs[1] = (couples[1] < 0 ? -1 : 1) * signBits[Math.Abs(couples[1]) - 1];
		signs[2] = (couples[2] < 0 ? -1 : 1) * signBits[Math.Abs(couples[2]) - 1];
		signs[3] = (couples[3] < 0 ? -1 : 1) * signBits[Math.Abs(couples[3]) - 1];
		signs[4] = (couples[4] < 0 ? -1 : 1) * signBits[Math.Abs(couples[4]) - 1];
		signs[5] = (couples[5] < 0 ? -1 : 1) * signBits[Math.Abs(couples[5]) - 1];

		string[]	indices = new string[] { "0", "1", "2", "3", "4", "5", "6",  };

		string	latex = "";
		for ( int d=0; d < 7; d++ ) {
			latex += d == 0 ? "&= " : "&+ ";
			for ( int e=0; e < 6; e++ ) {
				if ( e > 0 )
					latex += signs[e] > 0 ? " + " : " - ";

				int	index_w = d;
				int	index_u = permutations[d,e];
				int	index_v = permutations[d,derangement[e]];

				latex += "u_" + indices[index_w] + ".u_" + indices[index_u] + ".v_" + indices[index_v];
			}
			latex += " \\\\\\\\\r\n";
		}
	}
}

int[,,]	m_sumTripletsU7D = new int[7,7,7];
void	AddTripletU7D( int _u0, int _u1, int _v, int _sign ) {
	if ( _u0 == _u1 || _u1 == _v ) throw new Exception(  "Can't have identical indices!" );
	m_sumTripletsU7D[_u0, _u1, _v] += _sign;
	m_sumTripletsU7D[_u1, _u0, _v] += _sign;
}
int[,,]	m_sumTripletsV7D = new int[7,7,7];
void	AddTripletV7D( int _v0, int _u, int _v1, int _sign ) {
	if ( _v0 == _u || _u == _v1 ) throw new Exception(  "Can't have identical indices!" );
	m_sumTripletsV7D[_v0, _u, _v1] += _sign;
	m_sumTripletsV7D[_v1, _u, _v0] += _sign;
}

#endregion

#region Generic Cross Product

void	TestCrossGeneric() {

	List< Tuple< int, int > >[]	allSolutions = new List<Tuple<int, int>>[(21-3) / 2];
	for ( int N=3; N < 21; N+=2 ) {

		int[][]	derangements = null;
		int[][]	coupless = null;
		GenerateDerangments( N, ref derangements, ref coupless );

		int[,]	permutations = new int[N,N-1];
		for ( int d=0; d < N; d++ ) {
			for ( int x=0; x < N-1; x++ ) {
				permutations[d,x] = (d+1 + x) % N;
			}
		}

		int		signBitsCount = (N-1) / 2;
		int		totalSignsCombinations = 1 << signBitsCount;
		int[]	signs = new int[N-1];

		List< Tuple<int,int> >	solutionsU = new List<Tuple<int, int>>();
		List< Tuple<int,int> >	solutionsV = new List<Tuple<int, int>>();
		List< Tuple<int,int> >	solutions = new List<Tuple<int, int>>();

		// Try all possible derangements
		for ( int derangementIndex=0; derangementIndex < derangements.Length; derangementIndex++ ) {
			int[]	derangement = derangements[derangementIndex];
			int[]	couples = coupless[derangementIndex];

			// Try all possible sign combinations
			for ( int signCombination=0; signCombination < totalSignsCombinations; signCombination++ ) {

				// Build signs for each component
				for ( int componentIndex=0; componentIndex < N-1; componentIndex++ ) {
					int	bitIndex = Math.Abs( couples[componentIndex] ) - 1;
					int	signValue = (signCombination & (1 << bitIndex)) != 0 ? -1 : 1;
						signValue *= couples[componentIndex] > 0 ? 1 : -1;
					signs[componentIndex] = signValue;
				}

				// Estimate each line of w.(u x v)
				int[,,]	sumTripletsU = new int[N,N,N];
				int[,,]	sumTripletsV = new int[N,N,N];
				for ( int d=0; d < N; d++ ) {
					for ( int x=0; x < N-1; x++ ) {
						int	index_w = d;
						int	index_u = permutations[d,x];
						int	index_v = permutations[d,derangement[x]];
						int	sign = signs[x];
						AddTripletU( sumTripletsU, index_w, index_u, index_v, sign );
						AddTripletV( sumTripletsV, index_w, index_u, index_v, sign );
					}
				}

				// Check if the sum only contains zeroes
				bool	allZeroesU = true;
				bool	allZeroesV = true;
				int		totalTriplets = N*N*N;
				for ( int j=0; j < totalTriplets; j++ ) {
					int	Z = j;
					int	X = Z / (N*N);
						Z -= N*N * X;
					int	Y = Z / N;
						Z -= N * Y;

					int	sumU = sumTripletsU[X,Y,Z];
					int	sumV = sumTripletsV[X,Y,Z];
					if ( sumU != 0 ) {
						allZeroesU = false;
					}
					if ( sumV != 0 ) {
						allZeroesV = false;
					}
				}
				if ( allZeroesU && allZeroesV ) {
					solutions.Add( new Tuple<int,int>( derangementIndex, signCombination ) );
				} else if ( allZeroesU ) {
					solutionsU.Add( new Tuple<int,int>( derangementIndex, signCombination ) );
				} else if ( allZeroesV ) {
					solutionsV.Add( new Tuple<int,int>( derangementIndex, signCombination ) );
				}
			}
		}

// 		if ( solutions.Count == 0 )
//			throw new Exception( "No solution!" );
// 		}
		allSolutions[(N-3) / 2] = solutions;
	}
}

void	AddTripletU( int[,,] _sumTriplets, int _u0, int _u1, int _v, int _sign ) {
	if ( _u0 == _u1 || _u1 == _v ) throw new Exception(  "Can't have identical indices!" );
	_sumTriplets[_u0, _u1, _v] += _sign;
	_sumTriplets[_u1, _u0, _v] += _sign;
}
void	AddTripletV( int[,,] _sumTriplets, int _v0, int _u, int _v1, int _sign ) {
	if ( _v0 == _u || _u == _v1 ) throw new Exception(  "Can't have identical indices!" );
	_sumTriplets[_v0, _u, _v1] += _sign;
	_sumTriplets[_v1, _u, _v0] += _sign;
}

void	GenerateDerangments( int _dimensionsCount, ref int[][] _derangements, ref int[][] _coupless ) {

// 	// Compute total amount of derangements (for array allocation)
// 	int	totalDerangementsCount = 1;
// 	for ( int i=_dimensionsCount-2; i > 1; i-=2 )
// 		totalDerangementsCount *= i;
// 
// 	_derangements = new int[totalDerangementsCount][];
// 	_coupless = new int[totalDerangementsCount][];

	// Recursively generate derangements
	int[]	originalOrder = new int[_dimensionsCount-1];
	for ( int i=0; i < _dimensionsCount-1; i++ )
		originalOrder[i] = i;
	RecurseGenerateDerangements( originalOrder, out _derangements, out _coupless );

// 	// Associate sign couples
// 	for ( int i=0; i < totalDerangementsCount; i++ ) {
// 		int[]	couples = new int[_dimensionsCount-1];
// 		_coupless[i] = couples;
// 		int	coupleIndex = 1;
// 		for ( int j=0; j < _dimensionsCount-1; j++ ) {
// 			if ( couples[j] == 0 ) {
// 				couples[j] = coupleIndex;
// 				couples[_derangements[i][j]] = -coupleIndex;
// 				coupleIndex++;
// 			}
// 		}
// 	}
}

void	RecurseGenerateDerangements( int[] _sequence, out int[][] _derangements, out int[][] _coupless ) {
	if ( _sequence.Length == 2 ) {
		// The only sensible choice...
		_derangements = new int[][] { new int[2] { _sequence[1], _sequence[0] } };
		_coupless = new int[][] { new int[2] { 1, -1 } };
		return;
	}

	int				N = _sequence.Length;
	List< int[] >	derangements = new List<int[]>();
	List< int[] >	coupless = new List<int[]>();
	for ( int i=1; i < N; i++ ) {

		// Build the original sub-sequence containing the remaining terms (i.e. all terms except the terms at index 0 and i)
		int[]	originalSubSequence = new int[N-2];
		for ( int j=1; j <= N-2; j++ ) {
			originalSubSequence[j-1] = j < i ? _sequence[j] : _sequence[j+1];	// Skip i^th term
		}
		int[][]	subSequences, subCoupless;
		RecurseGenerateDerangements( originalSubSequence, out subSequences, out subCoupless );

		// Insert the sub-sequences back
		for ( int subSequenceIndex=0; subSequenceIndex < subSequences.Length; subSequenceIndex++ ) {
			int[]	subSequence = subSequences[subSequenceIndex];
			int[]	subCouples = subCoupless[subSequenceIndex];

			int[]	derangement = new int[N];
			int[]	couples = new int[N];

			// Initial derangement is always between the first term of the sequence and any other term
			derangement[0] = _sequence[i];
			derangement[i] = _sequence[0];
			couples[0] = 1;		// First, positive couple
			couples[i] = -1;	// First, negative couple

			// Insert remaining terms
			for ( int j=1; j <= N-2; j++ ) {
				derangement[j < i ? j : j+1] = subSequence[j-1];
				couples[j < i ? j : j+1] = subCouples[j-1] < 0 ? subCouples[j-1] - 1 : 1 + subCouples[j-1];
			}

			// Store new combination
			derangements.Add( derangement );
			coupless.Add( couples );
		}
	}

	_derangements = derangements.ToArray();
	_coupless = coupless.ToArray();
}

#endregion

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			try {
				m_device.Init( panelOutput.Handle, false, true );
			} catch ( Exception _e ) {
				m_device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "MSBRDF Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			try {
				m_shader_Render = new Shader( m_device, new System.IO.FileInfo( "Shaders/Render.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "MSBRDF Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}

			uint	W = (uint) panelOutput.Width;
			uint	H = (uint) panelOutput.Height;

			m_CB_Global = new ConstantBuffer<CB_Global>( m_device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_device, 1 );
			m_CB_Render = new ConstantBuffer<CB_Render>( m_device, 2 );

			BuildNoiseTextures();

BuildMSBRDF( new DirectoryInfo( @".\Tables\" ) );

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 1, -2.5f ), new float3( 0, 1, 0 ), float3.UnitY );

			// Start game time
			m_Ticks2Seconds = 1.0 / System.Diagnostics.Stopwatch.Frequency;
			m_StopWatch.Start();
			m_StartGameTime = GetGameTime();
		}

		protected override void OnFormClosed( FormClosedEventArgs e ) {
			if ( m_device == null )
				return;

			m_Tex_Noise4D.Dispose();
			m_Tex_Noise.Dispose();

			m_CB_Render.Dispose();
			m_CB_Camera.Dispose();
			m_CB_Global.Dispose();

			m_shader_Render.Dispose();

			m_device.Exit();

			base.OnFormClosed( e );
		}

		#region  Noise Generation

		void	BuildNoiseTextures() {

			PixelsBuffer	Content = new PixelsBuffer( NOISE_SIZE*NOISE_SIZE*NOISE_SIZE*4 );
			PixelsBuffer	Content4D = new PixelsBuffer( NOISE_SIZE*NOISE_SIZE*NOISE_SIZE*16 );

			SimpleRNG.SetSeed( 521288629, 362436069 );

			float4	V = float4.Zero;
			using ( BinaryWriter W = Content.OpenStreamWrite() ) {
				using ( BinaryWriter W2 = Content4D.OpenStreamWrite() ) {
					for ( int Z=0; Z < NOISE_SIZE; Z++ )
						for ( int Y=0; Y < NOISE_SIZE; Y++ )
							for ( int X=0; X < NOISE_SIZE; X++ ) {
								V.Set( (float) SimpleRNG.GetUniform(), (float) SimpleRNG.GetUniform(), (float) SimpleRNG.GetUniform(), (float) SimpleRNG.GetUniform() );
								W.Write( V.x );
								W2.Write( V.x );
								W2.Write( V.y );
								W2.Write( V.z );
								W2.Write( V.w );
							}
				}
			}

			m_Tex_Noise = new Texture3D( m_device, NOISE_SIZE, NOISE_SIZE, NOISE_SIZE, 1, ImageUtility.PIXEL_FORMAT.R8, ImageUtility.COMPONENT_FORMAT.UNORM, false, false, new PixelsBuffer[] { Content } );
			m_Tex_Noise4D = new Texture3D( m_device, NOISE_SIZE, NOISE_SIZE, NOISE_SIZE, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.UNORM, false, false, new PixelsBuffer[] { Content4D } );
		}

		#endregion

		#region Multiple-Scattering BRDF

		const uint		COS_THETA_SUBDIVS_COUNT = 32;
		const uint		ROUGHNESS_SUBDIVS_COUNT = 32;

		float[,]	m_E = new float[COS_THETA_SUBDIVS_COUNT,ROUGHNESS_SUBDIVS_COUNT];
		float[]		m_Eavg = new float[ROUGHNESS_SUBDIVS_COUNT];
		
		void	BuildMSBRDF( DirectoryInfo _targetDirectory ) {

			FileInfo	MSBRDFFileName = new FileInfo( Path.Combine( _targetDirectory.FullName, "MSBRDF_E" + COS_THETA_SUBDIVS_COUNT + "x" + ROUGHNESS_SUBDIVS_COUNT + ".float" ) );
			FileInfo	MSBRDFFileName2 = new FileInfo( Path.Combine( _targetDirectory.FullName, "MSBRDF_Eavg" + ROUGHNESS_SUBDIVS_COUNT + ".float" ) );

			#if PRECOMPUTE_BRDF

{
			const uint		PHI_SUBDIVS_COUNT = 2*512;
			const uint		THETA_SUBDIVS_COUNT = 64;

			const float		dPhi = Mathf.TWOPI / PHI_SUBDIVS_COUNT;
			const float		dTheta = Mathf.HALFPI / THETA_SUBDIVS_COUNT;
			const float		dMu = 1.0f / THETA_SUBDIVS_COUNT;

			string	dumpMathematica = "{";
			for ( uint Y=0; Y < ROUGHNESS_SUBDIVS_COUNT; Y++ ) {
				float	m = (float) Y / (ROUGHNESS_SUBDIVS_COUNT-1);
				float	m2 = Math.Max( 0.01f, m*m );
// 				float	m2 = Math.Max( 0.01f, (float) Y / (ROUGHNESS_SUBDIVS_COUNT-1) );
// 				float	m = Mathf.Sqrt( m2 );

//				dumpMathematica += "{ ";	// Start a new roughness line
				for ( uint X=0; X < COS_THETA_SUBDIVS_COUNT; X++ ) {
					float	cosThetaO = (float) X / (COS_THETA_SUBDIVS_COUNT-1);
					float	sinThetaO = Mathf.Sqrt( 1 - cosThetaO*cosThetaO );

					float	NdotV = cosThetaO;

					float	integral = 0.0f;
//					float	integralNDF = 0.0f;
					for ( uint THETA=0; THETA < THETA_SUBDIVS_COUNT; THETA++ ) {
// 						float	thetaI = Mathf.HALFPI * (0.5f+THETA) / THETA_SUBDIVS_COUNT;
// 						float	cosThetaI = Mathf.Cos( thetaI );
// 						float	sinThetaI = Mathf.Sin( thetaI );

						// Use cosine-weighted sampling
						float	sqCosThetaI = (0.5f+THETA) / THETA_SUBDIVS_COUNT;
						float	cosThetaI = Mathf.Sqrt( sqCosThetaI );
						float	sinThetaI = Mathf.Sqrt( 1 - sqCosThetaI );

						float	NdotL = cosThetaI;

						for ( uint PHI=0; PHI < PHI_SUBDIVS_COUNT; PHI++ ) {
							float	phi = Mathf.TWOPI * PHI / PHI_SUBDIVS_COUNT;

							// Compute cos(theta_h) = Omega_h.N where Omega_h = (Omega_i + Omega_o) / ||Omega_i + Omega_o|| is the half vector and N the surface normal
							float	cosThetaH = (cosThetaI + cosThetaO) / Mathf.Sqrt( 2 * (1 + cosThetaO * cosThetaI + sinThetaO * sinThetaI * Mathf.Sin( phi )) );
// 							float3	omega_i = new float3( sinThetaI * Mathf.Cos( phi ), sinThetaI * Mathf.Sin( phi ), cosThetaI );
// 							float3	omega_o = new float3( sinThetaO, 0, cosThetaO );
// 							float3	omega_h = (omega_i + omega_o).Normalized;
// 							float	cosThetaH = omega_h.z;

							// Compute GGX NDF
							float	den = 1 - cosThetaH*cosThetaH * (1 - m2);
							float	NDF = m2 / (Mathf.PI * den*den);

							// Compute Smith shadowing/masking
							float	Smith_i_den = NdotL + Mathf.Sqrt( m2 + (1-m2) * NdotL*NdotL );
							float	Smith_o_den = NdotV + Mathf.Sqrt( m2 + (1-m2) * NdotV*NdotV );

							// Full BRDF is thus...
							float	GGX = NDF / (Smith_i_den * Smith_o_den);

//							integral += GGX * cosThetaI * sinThetaI;
							integral += GGX;
						}

//						integralNDF += Mathf.TWOPI * m2 * cosThetaI * sinThetaI / (Mathf.PI * Mathf.Pow( cosThetaI*cosThetaI * (m2 - 1) + 1, 2.0f ));
					}

					// Finalize
//					integral *= dTheta * dPhi;
					integral *= 0.5f * dMu * dPhi;	// Cosine-weighted sampling has a 0.5 factor!
//					integralNDF *= dTheta;

					m_E[X,Y] = integral;
					dumpMathematica += "{ " + cosThetaO + ", " + m + ", "  + integral + "}, ";
				}
			}

			dumpMathematica = dumpMathematica.Remove( dumpMathematica.Length-2 );	// Remove last comma
			dumpMathematica += " };";

			// Dump as binary
			using ( FileStream S = MSBRDFFileName.Create() )
				using ( BinaryWriter W = new BinaryWriter( S ) ) {
					for ( uint Y=0; Y < ROUGHNESS_SUBDIVS_COUNT; Y++ )
						for ( uint X=0; X < COS_THETA_SUBDIVS_COUNT; X++ )
							W.Write( m_E[X,Y] );
				}

			//////////////////////////////////////////////////////////////////////////
			// Compute average irradiance based on roughness, re-using the previously computed results
			const uint		THETA_SUBDIVS_COUNT2 = 512;

			float	dTheta2 = Mathf.HALFPI / THETA_SUBDIVS_COUNT2;

			for ( uint X=0; X < ROUGHNESS_SUBDIVS_COUNT; X++ ) {

				float	integral = 0.0f;
				for ( uint THETA=0; THETA < THETA_SUBDIVS_COUNT2; THETA++ ) {
					float	thetaO = Mathf.HALFPI * (0.5f+THETA) / THETA_SUBDIVS_COUNT2;
					float	cosThetaO = Mathf.Cos( thetaO );
					float	sinThetaO = Mathf.Sin( thetaO );

					// Sample previously computed table
					float	i = cosThetaO * COS_THETA_SUBDIVS_COUNT;
					uint	i0 = Math.Min( COS_THETA_SUBDIVS_COUNT-1, (uint) Mathf.Floor( i ) );
					uint	i1 = Math.Min( COS_THETA_SUBDIVS_COUNT-1, i0 + 1 );
					float	E = (1-i) * m_E[i0,X] + i * m_E[i1,X];

					integral += E * cosThetaO * sinThetaO;
				}

				// Finalize
				integral *= Mathf.TWOPI * dTheta2;

				m_Eavg[X] = integral;
			}

			// Dump as binary
			using ( FileStream S = MSBRDFFileName2.Create() )
				using ( BinaryWriter W = new BinaryWriter( S ) ) {
					for ( uint X=0; X < ROUGHNESS_SUBDIVS_COUNT; X++ )
						W.Write( m_Eavg[X] );
				}
}

			#endif

			// Build irradiance complement texture
			using ( PixelsBuffer content = new PixelsBuffer( COS_THETA_SUBDIVS_COUNT * ROUGHNESS_SUBDIVS_COUNT * 4 ) ) {
				using ( FileStream S = MSBRDFFileName.OpenRead() )
					using ( BinaryReader R = new BinaryReader( S ) )
						using ( BinaryWriter W = content.OpenStreamWrite() ) {
							for ( uint Y=0; Y < ROUGHNESS_SUBDIVS_COUNT; Y++ ) {
								for ( uint X=0; X < COS_THETA_SUBDIVS_COUNT; X++ ) {
									float	V = R.ReadSingle();
									m_E[X,Y] = V;
									W.Write( V );
								}
							}
						}

				m_tex_IrradianceComplement = new Texture2D( m_device, COS_THETA_SUBDIVS_COUNT, ROUGHNESS_SUBDIVS_COUNT, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new PixelsBuffer[] { content } );
			}

			// Build average irradiance texture
			using ( PixelsBuffer content = new PixelsBuffer( ROUGHNESS_SUBDIVS_COUNT * 4 ) ) {
				using ( FileStream S = MSBRDFFileName2.OpenRead() )
					using ( BinaryReader R = new BinaryReader( S ) )
						using ( BinaryWriter W = content.OpenStreamWrite() ) {
							for ( uint X=0; X < ROUGHNESS_SUBDIVS_COUNT; X++ ) {
								float	V = R.ReadSingle();
								m_Eavg[X] = V;
								W.Write( V );
							}
						}

				m_tex_IrradianceAverage = new Texture2D( m_device, ROUGHNESS_SUBDIVS_COUNT, 1, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new PixelsBuffer[] { content } );
			}


//////////////////////////////////////////////////////////////////////////
// Check single-scattering and multiple-scattering BRDFs are actual complements
//
float3[,]	integralChecks = new float3[COS_THETA_SUBDIVS_COUNT,ROUGHNESS_SUBDIVS_COUNT];
for ( uint Y=0; Y < ROUGHNESS_SUBDIVS_COUNT; Y++ ) {
	float	m = (float) Y / (ROUGHNESS_SUBDIVS_COUNT-1);
	float	m2 = Math.Max( 0.01f, m*m );

	float	Eavg = SampleEavg( m );

	for ( uint X=0; X < COS_THETA_SUBDIVS_COUNT; X++ ) {
		float	cosThetaO = (float) X / (COS_THETA_SUBDIVS_COUNT-1);
		float	sinThetaO = Mathf.Sqrt( 1 - cosThetaO*cosThetaO );

		float	NdotV = cosThetaO;

		float	Eo = SampleE( cosThetaO, m );

		const uint		CHECK_THETA_SUBDIVS_COUNT = 128;
		const uint		CHECK_PHI_SUBDIVS_COUNT = 2*128;

		const float		dPhi = Mathf.TWOPI / CHECK_PHI_SUBDIVS_COUNT;
		const float		dTheta = Mathf.HALFPI / CHECK_THETA_SUBDIVS_COUNT;

		float	integralSS = 0.0f;
		float	integralMS = 0.0f;
		for ( uint THETA=0; THETA < CHECK_THETA_SUBDIVS_COUNT; THETA++ ) {

			// Use regular sampling
			float	thetaI = Mathf.HALFPI * (0.5f+THETA) / CHECK_THETA_SUBDIVS_COUNT;
			float	cosThetaI = Mathf.Cos( thetaI );
			float	sinThetaI = Mathf.Sin( thetaI );

// 			// Use cosine-weighted sampling
// 			float	sqCosThetaI = (0.5f+THETA) / CHECK_THETA_SUBDIVS_COUNT;
// 			float	cosThetaI = Mathf.Sqrt( sqCosThetaI );
// 			float	sinThetaI = Mathf.Sqrt( 1 - sqCosThetaI );

 			float	NdotL = cosThetaI;

			for ( uint PHI=0; PHI < CHECK_PHI_SUBDIVS_COUNT; PHI++ ) {
				float	phi = Mathf.TWOPI * PHI / CHECK_PHI_SUBDIVS_COUNT;

				//////////////////////////////////////////////////////////////////////////
				// Single-scattering part

				// Compute cos(theta_h) = Omega_h.N where Omega_h = (Omega_i + Omega_o) / ||Omega_i + Omega_o|| is the half vector and N the surface normal
				float	cosThetaH = (cosThetaI + cosThetaO) / Mathf.Sqrt( 2 * (1 + cosThetaO * cosThetaI + sinThetaO * sinThetaI * Mathf.Sin( phi )) );
// 				float3	omega_i = new float3( sinThetaI * Mathf.Cos( phi ), sinThetaI * Mathf.Sin( phi ), cosThetaI );
// 				float3	omega_o = new float3( sinThetaO, 0, cosThetaO );
// 				float3	omega_h = (omega_i + omega_o).Normalized;
// 				float	cosThetaH = omega_h.z;

				// Compute GGX NDF
				float	den = 1 - cosThetaH*cosThetaH * (1 - m2);
				float	NDF = m2 / (Mathf.PI * den*den);

				// Compute Smith shadowing/masking
				float	Smith_i_den = NdotL + Mathf.Sqrt( m2 + (1-m2) * NdotL*NdotL );
				float	Smith_o_den = NdotV + Mathf.Sqrt( m2 + (1-m2) * NdotV*NdotV );

				// Full BRDF is thus...
				float	GGX = NDF / (Smith_i_den * Smith_o_den);

				integralSS += GGX * cosThetaI * sinThetaI;
//				integralSS += GGX;

				//////////////////////////////////////////////////////////////////////////
				// Multiple-scattering part
				float	Ei = SampleE( cosThetaI, m );

				float	GGX_ms = Eo * Ei / Eavg;

				integralMS += GGX_ms * cosThetaI * sinThetaI;
			}
		}

		// Finalize
		integralSS *= dTheta * dPhi;
		integralMS *= dTheta * dPhi;

		integralChecks[X,Y] = new float3( integralSS, integralMS, integralSS + integralMS );
	}
}
//
//////////////////////////////////////////////////////////////////////////


// verify BRDF + BRDFms integration = 1
//cube map + integration
		}

		float	SampleE( float _cosTheta, float _roughness ) {
			_cosTheta *= COS_THETA_SUBDIVS_COUNT;
			_roughness *= ROUGHNESS_SUBDIVS_COUNT;

			float	X = Mathf.Floor( _cosTheta );
			float	x = _cosTheta - X;
			uint	X0 = Mathf.Min( COS_THETA_SUBDIVS_COUNT-1, (uint) X );
			uint	X1 = Mathf.Min( COS_THETA_SUBDIVS_COUNT-1, X0+1 );

			float	Y = Mathf.Floor( _roughness );
			float	y = _roughness - Y;
			uint	Y0 = Mathf.Min( ROUGHNESS_SUBDIVS_COUNT-1, (uint) Y );
			uint	Y1 = Mathf.Min( ROUGHNESS_SUBDIVS_COUNT-1, Y0+1 );

			float	V00 = m_E[X0,Y0];
			float	V10 = m_E[X1,Y0];
			float	V01 = m_E[X0,Y1];
			float	V11 = m_E[X1,Y1];

			float	V0 = (1.0f - x) * V00 + x * V10;
			float	V1 = (1.0f - x) * V01 + x * V11;
			float	V = (1.0f - y) * V0 + y * V1;
			return V;
		}
		float	SampleEavg( float _roughness ) {
			_roughness *= ROUGHNESS_SUBDIVS_COUNT;
			float	X = Mathf.Floor( _roughness );
			float	x = _roughness - X;
			uint	X0 = Mathf.Min( ROUGHNESS_SUBDIVS_COUNT-1, (uint) X );
			uint	X1 = Mathf.Min( ROUGHNESS_SUBDIVS_COUNT-1, X0+1 );

			float	V0 = m_Eavg[X0];
			float	V1 = m_Eavg[X1];
			float	V = (1.0f - x) * V0 + x * V1;
			return V;
		}

		#endregion

		void Camera_CameraTransformChanged( object sender, EventArgs e ) {

			m_CB_Camera.m._Camera2World = m_Camera.Camera2World;
			m_CB_Camera.m._World2Camera = m_Camera.World2Camera;

			m_CB_Camera.m._Camera2Proj = m_Camera.Camera2Proj;
			m_CB_Camera.m._Proj2Camera = m_CB_Camera.m._Camera2Proj.Inverse;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Proj2Camera * m_CB_Camera.m._Camera2World;

			m_CB_Camera.UpdateData();
		}

		/// <summary>
		/// Gets the current game time in seconds
		/// </summary>
		/// <returns></returns>
		public float	GetGameTime() {
			long	Ticks = m_StopWatch.ElapsedTicks;
			float	Time = (float) (Ticks * m_Ticks2Seconds);
			return Time;
		}

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null )
				return;

			uint	W = (uint) panelOutput.Width;
			uint	H = (uint) panelOutput.Height;

			// Timer
			float	lastGameTime = m_CurrentGameTime;
			m_CurrentGameTime = GetGameTime();
			
			if ( m_CurrentGameTime - m_StartFPSTime > 1.0f ) {
				m_AverageFrameTime = (m_CurrentGameTime - m_StartFPSTime) / Math.Max( 1, m_SumFrames );
				m_SumFrames = 0;
				m_StartFPSTime = m_CurrentGameTime;
			}
			m_SumFrames++;

			m_CB_Global.m._ScreenSize.Set( W, H, 1.0f / W, 1.0f / H );
			m_CB_Global.m._Time = m_CurrentGameTime;
			m_CB_Global.UpdateData();

			Camera_CameraTransformChanged( m_Camera, EventArgs.Empty );

			m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, 0, true, false );

			m_Tex_Noise.Set( 8 );
			m_Tex_Noise4D.Set( 9 );


			//////////////////////////////////////////////////////////////////////////
			// Fullscreen rendering
			if ( m_shader_Render.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
				m_device.SetRenderTarget( m_device.DefaultTarget, null );

				m_CB_Render.m._roughness = floatTrackbarControlRoughness.Value;
				m_CB_Render.m._albedo = floatTrackbarControlAlbedo.Value;
				m_CB_Render.m._lightElevation = floatTrackbarControlLightElevation.Value * Mathf.HALFPI;
				m_CB_Render.UpdateData();


				m_tex_IrradianceComplement.SetPS( 2 );
				m_tex_IrradianceAverage.SetPS( 3 );


				m_device.RenderFullscreenQuad( m_shader_Render );
			}

			// Show!
			m_device.Present( false );

			// Update window text
			Text = "GloubiBoule - Avg. Frame Time " + (1000.0f * m_AverageFrameTime).ToString( "G5" ) + " ms (" + (1.0f / m_AverageFrameTime).ToString( "G5" ) + " FPS)";
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			m_device.ReloadModifiedShaders();
		}
	}
}
