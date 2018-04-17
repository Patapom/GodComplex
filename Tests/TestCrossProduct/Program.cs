using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCrossProduct
{
	class Program
	{
		static void Main( string[] args ) {

//TestCross5D();
//TestCross7D();
TestCrossGeneric();

		}

		#region 5D Cross Product

		static int[,,]	m_sumTripletsU5D = new int[5,5,5];
		static int[,,]	m_sumTripletsV5D = new int[5,5,5];

		static void	TestCross5D() {
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
					Array.Clear( m_sumTripletsU5D, 0, 5*5*5 );
					Array.Clear( m_sumTripletsV5D, 0, 5*5*5 );
					for ( int d=0; d < 5; d++ ) {
						int	index_w_0 = d;
						int	index_u_0 = permutations[d,0];
						int	index_v_0 = permutations[d,derangement[0]];
						int	sign_0 = signs[0];
						AddTripletU( m_sumTripletsU5D, index_w_0, index_u_0, index_v_0, sign_0 );
						AddTripletV( m_sumTripletsV5D, index_w_0, index_u_0, index_v_0, sign_0 );

						int	index_w_1 = d;
						int	index_u_1 = permutations[d,1];
						int	index_v_1 = permutations[d,derangement[1]];
						int	sign_1 = signs[1];
						AddTripletU( m_sumTripletsU5D, index_w_1, index_u_1, index_v_1, sign_1 );
						AddTripletV( m_sumTripletsV5D, index_w_1, index_u_1, index_v_1, sign_1 );

						int	index_w_2 = d;
						int	index_u_2 = permutations[d,2];
						int	index_v_2 = permutations[d,derangement[2]];
						int	sign_2 = signs[2];
						AddTripletU( m_sumTripletsU5D, index_w_2, index_u_2, index_v_2, sign_2 );
						AddTripletV( m_sumTripletsV5D, index_w_2, index_u_2, index_v_2, sign_2 );

						int	index_w_3 = d;
						int	index_u_3 = permutations[d,3];
						int	index_v_3 = permutations[d,derangement[3]];
						int	sign_3 = signs[3];
						AddTripletU( m_sumTripletsU5D, index_w_3, index_u_3, index_v_3, sign_3 );
						AddTripletV( m_sumTripletsV5D, index_w_3, index_u_3, index_v_3, sign_3 );
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

						int	sumU = m_sumTripletsU5D[x,y,z];
						int	sumV = m_sumTripletsV5D[x,y,z];
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

		#endregion

		#region 7D Cross Product

		static int[,,]	m_sumTripletsU7D = new int[7,7,7];
		static int[,,]	m_sumTripletsV7D = new int[7,7,7];

		static void	TestCross7D() {
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
						AddTripletU( m_sumTripletsU7D, index_w_0, index_u_0, index_v_0, sign_0 );
						AddTripletV( m_sumTripletsV7D, index_w_0, index_u_0, index_v_0, sign_0 );

						int	index_w_1 = d;
						int	index_u_1 = permutations[d,1];
						int	index_v_1 = permutations[d,derangement[1]];
						int	sign_1 = signs[1];
						AddTripletU( m_sumTripletsU7D, index_w_1, index_u_1, index_v_1, sign_1 );
						AddTripletV( m_sumTripletsV7D, index_w_1, index_u_1, index_v_1, sign_1 );

						int	index_w_2 = d;
						int	index_u_2 = permutations[d,2];
						int	index_v_2 = permutations[d,derangement[2]];
						int	sign_2 = signs[2];
						AddTripletU( m_sumTripletsU7D, index_w_2, index_u_2, index_v_2, sign_2 );
						AddTripletV( m_sumTripletsV7D, index_w_2, index_u_2, index_v_2, sign_2 );

						int	index_w_3 = d;
						int	index_u_3 = permutations[d,3];
						int	index_v_3 = permutations[d,derangement[3]];
						int	sign_3 = signs[3];
						AddTripletU( m_sumTripletsU7D, index_w_3, index_u_3, index_v_3, sign_3 );
						AddTripletV( m_sumTripletsV7D, index_w_3, index_u_3, index_v_3, sign_3 );

						int	index_w_4 = d;
						int	index_u_4 = permutations[d,4];
						int	index_v_4 = permutations[d,derangement[4]];
						int	sign_4 = signs[4];
						AddTripletU( m_sumTripletsU7D, index_w_4, index_u_4, index_v_4, sign_4 );
						AddTripletV( m_sumTripletsV7D, index_w_4, index_u_4, index_v_4, sign_4 );

						int	index_w_5 = d;
						int	index_u_5 = permutations[d,5];
						int	index_v_5 = permutations[d,derangement[5]];
						int	sign_5 = signs[5];
						AddTripletU( m_sumTripletsU7D, index_w_5, index_u_5, index_v_5, sign_5 );
						AddTripletV( m_sumTripletsV7D, index_w_5, index_u_5, index_v_5, sign_5 );
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

		#endregion

		#region Generic Cross Product

		static void	TestCrossGeneric() {

			List< Tuple< int, int > >[]	allSolutions = new List<Tuple<int, int>>[(21-3) / 2];
			int[]						totalValidDerangementsCounts = new int[(21-3) / 2];
			List< Tuple< int, int > >[]	allCommutativeSolutions = new List<Tuple<int, int>>[(21-3) / 2];	// Solutions that yield a commutative cross-product

			// Try all possible odd dimensions
			for ( int N=3; N < 21; N+=2 ) {

				// Recursively generate all possible derangements
				int[][]	derangements = null;
				int[][]	coupless = null;
				int[]	originalOrder = new int[N-1];
				for ( int i=0; i < N-1; i++ )
					originalOrder[i] = i;
				RecurseGenerateDerangements( originalOrder, out derangements, out coupless );

				totalValidDerangementsCounts[(N-3) / 2] = derangements.Length;

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

				// Store solutions
				allSolutions[(N-3) / 2] = solutions;

				if ( solutions.Count == 0 )
					continue;

				//////////////////////////////////////////////////////////////////////////
				// Check anti-commutativity
				// We simply add couples for the u x v and v x u products
				//
				List< Tuple< int, int > >	commutativeSolutions = new List<Tuple<int, int>>();
				foreach ( Tuple< int, int > solution in solutions ) {
					// Retrieve derangement
					int		derangementIndex = solution.Item1;
					int[]	derangement = derangements[derangementIndex];
					int[]	couples = coupless[derangementIndex];

					// Build signs
					int		signCombination = solution.Item2;
					for ( int componentIndex=0; componentIndex < N-1; componentIndex++ ) {
						int	bitIndex = Math.Abs( couples[componentIndex] ) - 1;
						int	signValue = (signCombination & (1 << bitIndex)) != 0 ? -1 : 1;
							signValue *= couples[componentIndex] > 0 ? 1 : -1;
						signs[componentIndex] = signValue;
					}

					// Estimate cross products (u x v) and (v x u)
					int[,]	sumCouplesUV = new int[N,N];
					int[,]	sumCouplesVU = new int[N,N];
					int[,]	sumCouples = new int[N,N];
					for ( int d=0; d < N; d++ ) {
						for ( int x=0; x < N-1; x++ ) {
							int	index_u = permutations[d,x];
							int	index_v = permutations[d,derangement[x]];
							int	sign = signs[x];
							sumCouplesUV[index_u,index_v] += sign;
							sumCouplesVU[index_v,index_u] += sign;
							// Add 
							sumCouples[index_u,index_v] += sign;
							sumCouples[index_v,index_u] += sign;
						}
					}

					// Check couples got canceled
					bool	allZeroes = true;
					for ( int j=0; j < N*N; j++ ) {
						int	Y = j;
						int	X = Y / N;
							Y -= N * X;

						int	sum = sumCouples[X,Y];
						if ( sum != 0 ) {
							allZeroes = false;
							break;
						}
					}
					if ( !allZeroes ) {
						commutativeSolutions.Add( solution );
					}
				}
				allCommutativeSolutions[(N-3) / 2] = commutativeSolutions;
			}
		}

		static void	RecurseGenerateDerangements( int[] _sequence, out int[][] _derangements, out int[][] _coupless ) {
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

		static void	AddTripletU( int[,,] _sumTripletsU, int _u0, int _u1, int _v, int _sign ) {
			if ( _u0 == _u1 || _u1 == _v ) throw new Exception(  "Can't have identical indices!" );
			_sumTripletsU[_u0, _u1, _v] += _sign;
			_sumTripletsU[_u1, _u0, _v] += _sign;
		}
		static void	AddTripletV( int[,,] _sumTripletsV, int _v0, int _u, int _v1, int _sign ) {
			if ( _v0 == _u || _u == _v1 ) throw new Exception(  "Can't have identical indices!" );
			_sumTripletsV[_v0, _u, _v1] += _sign;
			_sumTripletsV[_v1, _u, _v0] += _sign;
		}

// 		void	AddTripletU( int[,,] _sumTripletsU, int _u0, int _u1, int _v, int _sign ) {
// 			if ( _u0 == _u1 || _u1 == _v ) throw new Exception(  "Can't have identical indices!" );
// 			_sumTripletsU[_u0, _u1, _v] += _sign;
// 			_sumTripletsU[_u1, _u0, _v] += _sign;
// 		}
// 		void	AddTripletV( int[,,] _sumTripletsV, int _v0, int _u, int _v1, int _sign ) {
// 			if ( _v0 == _u || _u == _v1 ) throw new Exception(  "Can't have identical indices!" );
// 			_sumTripletsV[_v0, _u, _v1] += _sign;
// 			_sumTripletsV[_v1, _u, _v0] += _sign;
// 		}
// 
// 		void	AddTripletU( int[,,] _sumTriplets, int _u0, int _u1, int _v, int _sign ) {
// 			if ( _u0 == _u1 || _u1 == _v ) throw new Exception(  "Can't have identical indices!" );
// 			_sumTriplets[_u0, _u1, _v] += _sign;
// 			_sumTriplets[_u1, _u0, _v] += _sign;
// 		}
// 		void	AddTripletV( int[,,] _sumTriplets, int _v0, int _u, int _v1, int _sign ) {
// 			if ( _v0 == _u || _u == _v1 ) throw new Exception(  "Can't have identical indices!" );
// 			_sumTriplets[_v0, _u, _v1] += _sign;
// 			_sumTriplets[_v1, _u, _v0] += _sign;
// 		}
	}
}
