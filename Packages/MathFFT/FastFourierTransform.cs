using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMath.FFT {
	/// <summary>
	/// Performs the Fast Fourier Transform (FFT) or Inverse-FFT of a 1-Dimensional discrete complex signal
	/// This is a slow CPU version purely designed to test the Fourier transform and for debugging purpose
	/// 
	/// The idea behind the "Fast" Discrete Fourier Transform is that we can recursively subdivide the analyze
	///  of a domain into sub-domains that are easier to compute, and re-use the results each stage.
	/// It turns a O(N²) computation into log2(N) stages of O(N) computations.
	///  
	/// Basically, a DFT is:
	///			  N-1
	///		X_k = Sum[ x_n * e^(-2PI * n/N * k) ]
	///			  n=0
	/// 
	/// For k€[0,N[
	/// 
	/// Assuming a power of two length N, we can separate the even and odd powers and write:
	/// 
	///			  N/2-1
	///		X_k = Sum[ x_(2n) * e^(-i * 2PI * (2n)/N * k) ]
	///			  n=0
	///			  
	///			  N/2-1
	///			+ Sum[ x_(2n+1) * e^(-i * 2PI * (2n+1)/N * k) ]
	///			  n=0
	/// 
	/// That we rewrite:
	/// 
	///		X_k = E_k + e^(-i * 2PI * k / N) * O_k
	/// 
	/// With:
	///			  N/2-1
	///		E_k = Sum[ x_(2n) * e^(-i * 2PI * (2n)/N * k) ]
	///			  n=0
	///			  
	///			  N/2-1
	///		O_k	= Sum[ x_(2n+1) * e^(-i * 2PI * (2n)/N * k) ]
	///			  n=0
	/// 
	/// Noticing that:
	/// 
	///		e^(-i * 2PI * (2n)/N * (k+N/2)) = e^(-i * 2PI * (2n)/N * k) * e^(-i * 2PI * (2n/N)*(N/2) )
	///										= e^(-i * 2PI * (2n)/N * k) * e^(-i * 2PI * n)
	///										= e^(-i * 2PI * (2n)/N * k)
	/// 
	///		E_(k+N/2) = E_k
	///	and	O_(k+N/2) = O_k
	/// 
	/// Therefore we can rewrite:
	/// 
	///		X_k = E_k + e^(-i * 2PI * k / N) * O_k					if 0 <= k < N/2
	/// 
	///		X_k = E_(k-N/2) + e^(-i * 2PI * k / N) * O_(k-N/2)		if N/2 <= k < N (that is the part where we reuse existing results)
	/// 
	/// Noticing the "twiddle factor" also simplifies:
	/// 
	///		e^(-i * 2PI * (k+N/2) / N) = e^(-i * 2PI * k / N) * e^(-i * PI) = -e^(-i * 2PI * k / N)
	/// 
	/// Thus we get the final result for 0 <= k < N/2 :
	/// 
	///		X_k = E_k + e^(-i * 2PI * k / N) * O_k
	///		X_(k+N/2) = E_k - e^(-i * 2PI * k / N) * O_k
	/// 
	/// Expressing the DFT of length N recursively in terms of two DFTs of length N/2 allows to reach N*log(N) speeds instead
	///	 of the traditional N² form.
	/// </summary>
    public static class FFT1D {

		#region Forward FFT

		//////////////////////////////////////////////////////////////////////////
		// The forward FFT:
		//	• Takes a 2^N length complex-valued signal as input
		//	• Outputs a complex-valued 2^N spectrum as output
		//
		// The resulting spectrum:
		//	• Is normalized, meaning each of the complex values in the spectrum are multiplied by 1/N
		//	• Stores complex amplitudes for frequencies in [0,N[ as a contiguous array
		//
		//////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Applies the Discrete Fourier Transform to an input signal
		/// </summary>
		/// <param name="_inputSignal">The input signal of complex-valued amplitudes for each time sample</param>
		/// <returns>The output complex-valued spectrum of amplitudes for each frequency.
		/// The spectrum of frequencies goes from [-N/2,N/2[ hertz, where N is the length of the input signal</returns>
		/// <remarks>Throws an exception if signal size is not a power of two!</remarks>
		public static Complex[]	FFT_Forward( Complex[] _inputSignal ) {
			Complex[] outputSpectrum = new Complex[_inputSignal.Length];
			FFT_Forward( _inputSignal, outputSpectrum );
			return outputSpectrum;
		}

		/// <summary>
		/// Applies the Discrete Fourier Transform to an input signal
		/// </summary>
		/// <param name="_inputSignal">The input signal of complex-valued amplitudes for each time sample</param>
		/// <param name="_outputSpectrum">The output complex-valued spectrum of amplitudes for each frequency.
		/// The spectrum of frequencies goes from [-N/2,N/2[ hertz, where N is the length of the input signal</param>
		/// <remarks>Throws an exception if signal and spectrum sizes mismatch and if their size are not a power of two!</remarks>
		public static void	FFT_Forward( Complex[] _inputSignal, Complex[] _outputSpectrum ) {
			int	size = _inputSignal.Length;
			if ( _outputSpectrum.Length != size )
				throw new Exception( "Input signal and output spectrum sizes mismatch!" );
			float	fPOT = (float) (Math.Log( size ) / Math.Log( 2.0 ));
			int		POT = (int) Math.Floor( fPOT );
			if ( fPOT != POT )
				throw new Exception( "Input signal length is not a Power Of Two!" );

			FFT_BreadthFirst( _inputSignal, _outputSpectrum, size, POT, -2.0 * Math.PI );

			// Normalize
			for ( int i=0; i < size; i++ )
				_outputSpectrum[i] /= size;
		}

		#endregion

		#region Inverse FFT

		//////////////////////////////////////////////////////////////////////////
		// The inverse FFT:
		//	• Takes a 2^N length complex-valued frequency spectrum as input
		//	• Outputs a complex-valued 2^N temporal signal as output
		//
		// The input spectrum:
		//	• Must be normalized, meaning each of the complex values in the spectrum are already multiplied by 1/N
		//	• Must store complex amplitudes for frequencies in [0,N[ as a contiguous array
		//
		//////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Applies the Inverse Discrete Fourier Transform to an input frequency spectrum
		/// </summary>
		/// <param name="_inputSpectrum">The input complex-valued spectrum of amplitudes for each frequency</param>
		/// <returns>The output complex-valued signal</returns>
		/// <remarks>Throws an exception if spectrum size is not a power of two!</remarks>
		public static Complex[]	FFT_Inverse( Complex[] _inputSpectrum ) {
			Complex[] outputSignal = new Complex[_inputSpectrum.Length];
			FFT_Inverse( _inputSpectrum, outputSignal );
			return outputSignal;
		}

		/// <summary>
		/// Applies the Inverse Discrete Fourier Transform to an input frequency spectrum
		/// </summary>
		/// <param name="_inputSpectrum">The input complex-valued spectrum of amplitudes for each frequency</param>
		/// <param name="_outputSignal">The output signal of complex-valued amplitudes for each time sample</param>
		/// <remarks>Throws an exception if signal and spectrum sizes mismatch and if their size are not a power of two!</remarks>
		public static void	FFT_Inverse( Complex[] _inputSpectrum, Complex[] _outputSignal ) {
			int	size = _inputSpectrum.Length;
			if ( _outputSignal.Length != size )
				throw new Exception( "Input spectrum and output signal sizes mismatch!" );
			float	fPOT = (float) (Math.Log( size ) / Math.Log( 2.0 ));
			int		POT = (int) Math.Floor( fPOT );
			if ( fPOT != POT )
				throw new Exception( "Input spectrum length is not a Power Of Two!" );

//Complex[]	input = _inputSpectrum;
//Complex[]	result = BROUTEFORCE_FFT( input, 0, 1, size, 2.0 * Math.PI );
//for ( int i=0; i < size; i++ )
//	_outputSignal[i] = result[i];
//return;

			FFT_BreadthFirst( _inputSpectrum, _outputSignal, size, POT, 2.0 * Math.PI );
		}

		#endregion

		#region Common Code

		private static void	FFT_BreadthFirst( Complex[] _input, Complex[] _output, int _size, int _POT, double _baseFrequency ) {
			Complex[]	temp = new Complex[_size];

			Complex[]	bufferIn = (_POT & 1) != 0 ? temp : _output;
			Complex[]	bufferOut = (_POT & 1) != 0 ? _output : temp;

			// Generate most-displacement indices then copy and displace source
// 			int[]	indices = new int[_size];
// 			GenerateIndexList( _POT-1, indices );
			int[]	indices = PermutationTables.ms_tables[_POT];
			for ( int i=0; i < _size; i++ )
				bufferIn[i] = _input[indices[i]];

			// Apply grouping and twiddling
			int		groupsCount = _size >> 1;
			int		groupSize = 1;
			double	frequency = 0.5 * _baseFrequency;
//			for ( int stageIndex=0; stageIndex < _POT; stageIndex++ ) {
for ( int stageIndex=0; stageIndex < 9; stageIndex++ ) {

				int	k_even = 0;
				int	k_odd = groupSize;
				for ( int groupIndex=0; groupIndex < groupsCount; groupIndex++ ) {
					for ( int i=0; i < groupSize; i++ ) {
						Complex	E = bufferIn[k_even];
						Complex	O = bufferIn[k_odd];

						double	omega = frequency * i;
						double	c = Math.Cos( omega );
						double	s = Math.Sin( omega );

						bufferOut[k_even].Set(	E.r + c * O.r - s * O.i, 
												E.i + s * O.r + c * O.i );

						bufferOut[k_odd].Set(	E.r - c * O.r + s * O.i, 
												E.i - s * O.r - c * O.i );

						k_even++;
						k_odd++;
					}

					k_even += groupSize;
					k_odd += groupSize;
				}

				// Double group size and halve frequency resolution
				groupsCount >>= 1;
				groupSize <<= 1;
				frequency *= 0.5;

				// Swap buffers
				Complex[]	t = bufferIn;
				bufferIn = bufferOut;
				bufferOut = t;
			}

if ( true ) {
	for ( int i=0; i < _size; i++ )
		bufferOut[i] = bufferIn[i];

	// Swap buffers
	Complex[]	t = bufferIn;
	bufferIn = bufferOut;
	bufferOut = t;
}

			if ( bufferIn != _output )
				throw new Exception( "Unexpected buffer as output!" );
		}

		#region Brute-Force Recursive Version

		private static Complex[]	BROUTEFORCE_FFT( Complex[] _inputSignal, int _inputIndex, int _inputStride, int _length, double _baseFrequency ) {
			if ( _length == 1 )
				return new Complex[] { _inputSignal[_inputIndex] };

			int			halfLength = _length >> 1;
			double		frequency = _baseFrequency / _length;

			Complex[]	even = BROUTEFORCE_FFT( _inputSignal, _inputIndex, 2*_inputStride, halfLength, _baseFrequency );
			Complex[]	odd = BROUTEFORCE_FFT( _inputSignal, _inputIndex + _inputStride, 2*_inputStride, halfLength, _baseFrequency );

			Complex[]	result = new Complex[_length];

			for ( int k=0; k < halfLength; k++ ) {
				Complex	Ek = even[k];
				Complex	Ok = odd[k];

				double	omega = frequency * k;
				double	c = Math.Cos( omega );
				double	s = Math.Sin( omega );

				result[k].Set(	Ek.r + c * Ok.r - s * Ok.i, 
								Ek.i + s * Ok.r + c * Ok.i );

				result[k+halfLength].Set(	Ek.r - c * Ok.r + s * Ok.i, 
											Ek.i - s * Ok.r - c * Ok.i );
			}
			return result;
		}

		#endregion

		#endregion
	}
}
