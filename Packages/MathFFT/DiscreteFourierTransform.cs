using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMath.FFT {
	/// <summary>
	/// Performs the Discrete Fourier Transform (DFT) or Inverse-DFT of a 1-Dimensional discrete complex signal
	/// This is a slow CPU version purely designed to test the Fourier transform and for debugging purpose
	/// </summary>
    public static class DFT1D {

		#region Forward DFT

		//////////////////////////////////////////////////////////////////////////
		// The forward DFT:
		//	• Takes a 2^N length complex-valued signal as input
		//	• Outputs a complex-valued 2^N spectrum as output
		//
		// The resulting spectrum:
		//	• Is normalized, meaning each of the complex values in the spectrum are multiplied by 1/N
		//	• Stores complex amplitudes for frequencies from -N/2 to +N/2 (excluded) as a contiguous array
		//		(so, for example, the DC term can simply be found at index N/2)
		//
		//////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Applies the Discrete Fourier Transform to an input signal
		/// </summary>
		/// <param name="_inputSignal">The input signal of complex-valued amplitudes for each time sample</param>
		/// <returns>The output complex-valued spectrum of amplitudes for each frequency.
		/// The spectrum of frequencies goes from [-N/2,N/2[ hertz, where N is the length of the input signal</returns>
		/// <remarks>Throws an exception if signal size is not a power of two!</remarks>
		public static Complex[]	DFT_Forward( Complex[] _inputSignal ) {
			Complex[] outputSpectrum = new Complex[_inputSignal.Length];
			DFT_Forward( _inputSignal, outputSpectrum );
			return outputSpectrum;
		}

		/// <summary>
		/// Applies the Discrete Fourier Transform to an input signal
		/// </summary>
		/// <param name="_inputSignal">The input signal of complex-valued amplitudes for each time sample</param>
		/// <param name="_outputSpectrum">The output complex-valued spectrum of amplitudes for each frequency.
		/// The spectrum of frequencies goes from [-N/2,N/2[ hertz, where N is the length of the input signal</param>
		/// <remarks>Throws an exception if signal and spectrum sizes mismatch and if their size are not a power of two!</remarks>
		public static void	DFT_Forward( Complex[] _inputSignal, Complex[] _outputSpectrum ) {
			int	size = _inputSignal.Length;
			if ( _outputSpectrum.Length != size )
				throw new Exception( "Input signal and output spectrum sizes mismatch!" );
			float	fPOT = (float) (Math.Log( size ) / Math.Log( 2.0 ));
			int		POT = (int) Math.Floor( fPOT );
			if ( fPOT != POT )
				throw new Exception( "Input signal length is not a Power Of Two!" );

			int		halfSize = size >> 1;
			double	normalizer = 1.0 / size;
			for ( int frequencyIndex=0; frequencyIndex < size; frequencyIndex++ ) {
				double	frequency = 2.0 * Math.PI * (frequencyIndex - halfSize);
				double	normalizedFrequency = -frequency * normalizer;	// Notice the - sign here! It's indicating the "division" by e^ix and the forward transform...

				// Accumulate (co)sine wave amplitude for specified frequency (i.e. computes how much the (co)sine wave matches to the signal at this frequency)
				double	sumR = 0.0;
				double	sumI = 0.0;
				for ( int i=0; i < size; i++ ) {
					double	omega = i * normalizedFrequency;	
					double	c = Math.Cos( omega );
					double	s = Math.Sin( omega );
					Complex	v = _inputSignal[i];
					sumR += c * v.r - s * v.i;
					sumI += s * v.r + c * v.i;
				}
				sumR *= normalizer;
				sumI *= normalizer;
				_outputSpectrum[frequencyIndex].Set( sumR, sumI );
			}
		}

		#endregion

		#region Inverse DFT

		//////////////////////////////////////////////////////////////////////////
		// The inverse DFT:
		//	• Takes a 2^N length complex-valued spectrum as input
		//	• Outputs a complex-valued 2^N temporal signal as output
		//
		// The input spectrum:
		//	• Must be normalized, meaning each of the complex values in the spectrum are already multiplied by 1/N
		//	• Must store complex amplitudes for frequencies from -N/2 to +N/2 (excluded) as a contiguous array
		//		(so, for example, the DC term can simply be found at index N/2)
		//
		//////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Applies the Inverse Discrete Fourier Transform to an input frequency spectrum
		/// </summary>
		/// <param name="_inputSpectrum">The input complex-valued spectrum of amplitudes for each frequency</param>
		/// <returns>The output complex-valued signal</returns>
		/// <remarks>Throws an exception if spectrum size is not a power of two!</remarks>
		public static Complex[]	DFT_Inverse( Complex[] _inputSpectrum ) {
			Complex[] outputSignal = new Complex[_inputSpectrum.Length];
			DFT_Inverse( _inputSpectrum, outputSignal );
			return outputSignal;
		}

		/// <summary>
		/// Applies the Inverse Discrete Fourier Transform to an input frequency spectrum
		/// </summary>
		/// <param name="_inputSpectrum">The input complex-valued spectrum of amplitudes for each frequency</param>
		/// <param name="_outputSignal">The output signal of complex-valued amplitudes for each time sample</param>
		/// <remarks>Throws an exception if signal and spectrum sizes mismatch and if their size are not a power of two!</remarks>
		public static void	DFT_Inverse( Complex[] _inputSpectrum, Complex[] _outputSignal ) {
			int	size = _inputSpectrum.Length;
			if ( _outputSignal.Length != size )
				throw new Exception( "Input spectrum and output signal sizes mismatch!" );
			float	fPOT = (float) (Math.Log( size ) / Math.Log( 2.0 ));
			int		POT = (int) Math.Floor( fPOT );
			if ( fPOT != POT )
				throw new Exception( "Input spectrum length is not a Power Of Two!" );

			int		halfSize = size >> 1;
			double	normalizer = 1.0 / size;

			Array.Clear( _outputSignal, 0, size );
			for ( int frequencyIndex=0; frequencyIndex < size; frequencyIndex++ ) {
				double	frequency = 2.0 * Math.PI * (frequencyIndex - halfSize);
				double	normalizedFrequency = frequency * normalizer;	// Notice the + sign here! It's indicating the "multiplication" by e^ix and the inverse transform...

				// Accumulate (co)sine wave at specified frequency and amplitude to the signal
				Complex	v = _inputSpectrum[frequencyIndex];
				for ( int i=0; i < size; i++ ) {
					double	omega = i * normalizedFrequency;	
					double	c = Math.Cos( omega );
					double	s = Math.Sin( omega );
					_outputSignal[i].r += c * v.r - s * v.i;
					_outputSignal[i].i += s * v.r + c * v.i;
				}
			}
		}

		#endregion
    }
}
