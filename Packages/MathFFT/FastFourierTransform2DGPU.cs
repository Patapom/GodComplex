﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Renderer;

namespace SharpMath.FFT {
	/// <summary>
	/// Performs the Fast Fourier Transform (FFT) or Inverse-FFT of a 2-Dimensional discrete complex signal
	/// This is the fast GPU version
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
    public class FFT2D_GPU : IDisposable {

		#region FIELDS

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB {
			public float	_sign;
			public uint		_bitReversalShift;
			public float	_normalizationFirstPass;
			public float	_normalizationFinal;
			// Generic FFT only
			public float	_groupFrequency;
			public uint		_groupSize;
			public uint		_groupSizePOT;
		}

		Device					m_device;

		ConstantBuffer< CB >	m_CB;

		ComputeShader			m_CS__1to16;
		ComputeShader			m_CS__Generic;

		Texture2D				m_texBufferIn;		// Texture that will contain the input signal/spectrum
		Texture2D				m_texBufferOut;		// Texture that will contain the output spectrum/signal
		Texture2D				m_texBufferCPU;		// CPU version used for readback

		uint					m_size;
		uint					m_POT;

		#endregion

		/// <summary>
		/// Gets the input view for quick GPU access
		/// </summary>
		public Texture2D	Input {
			get { return m_texBufferIn; }
		}

		/// <summary>
		/// Gets the output view for quick GPU access
		/// </summary>
		public Texture2D	Output {
			get { return m_texBufferOut; }
		}


		/// <summary>
		/// Initializes the FFT for the required signal size
		/// </summary>
		/// <param name="_device"></param>
		/// <param name="_signalSize">The size of the signals to process</param>
		public FFT2D_GPU( Device _device, uint _signalSize ) {
			// Check Power Of Two
			m_size = _signalSize;
			float	fPOT = (float) (Math.Log( m_size ) / Math.Log( 2.0 ));
			m_POT = (uint) Math.Floor( fPOT );
			if ( fPOT != m_POT )
				throw new Exception( "Signal size is not a Power Of Two!" );
			if ( m_POT < 4 || m_POT > 12 )
				throw new Exception( "GPU FFT implementation only supports the following sizes: { 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 }!" );

			// Initialize DX stuff
			m_device = _device;
			m_CB = new ConstantBuffer<CB>( m_device, 0 );
			m_texBufferIn = new Texture2D( m_device, (uint) m_size, (uint) m_size, 1, 1, ImageUtility.PIXEL_FORMAT.RG32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, null );
			m_texBufferOut = new Texture2D( m_device, (uint) m_size, (uint) m_size, 1, 1, ImageUtility.PIXEL_FORMAT.RG32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, null );
			m_texBufferCPU = new Texture2D( m_device, (uint) m_size, (uint) m_size, 1, 1, ImageUtility.PIXEL_FORMAT.RG32F, ImageUtility.COMPONENT_FORMAT.AUTO, true, true, null );

			try {
				FileServer	server = new FileServer( Properties.Resources.ResourceManager );
//				FileServer	server = new FileServer( new System.IO.DirectoryInfo( @"../../MathFFT/Shaders/" ) );

				Shader.AssertOnSaveBinaryBlobFailed = false;

				#if DEBUG
					m_CS__1to16 = new ComputeShader( _device, new System.IO.FileInfo( @"./Shaders/FFT2D.hlsl" ), "CS__1to16", server );
					m_CS__Generic = new ComputeShader( _device, new System.IO.FileInfo( @"./Shaders/FFT2D.hlsl" ), "CS__Generic", server );
				#else
					using ( new ScopedForceShadersLoadFromBinary() ) {
						m_CS__1to16 = new ComputeShader( _device, new System.IO.FileInfo( @"./Shaders/FFT2D.hlsl" ), "CS__1to16", server );
						m_CS__Generic = new ComputeShader( _device, new System.IO.FileInfo( @"./Shaders/FFT2D.hlsl" ), "CS__Generic", server );
					}
				#endif
			} catch ( Exception _e ) {
				m_device = null;
				throw new Exception( "An error occurred while creating FFT2D shaders!", _e );
			}
		}

		#region IDisposable Members

		public void Dispose() {
			m_CS__Generic.Dispose();
			m_CS__1to16.Dispose();
			m_texBufferCPU.Dispose();
			m_texBufferOut.Dispose();
			m_texBufferIn.Dispose();
			m_CB.Dispose();
		}

		#endregion

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
		public Complex[,]	FFT_Forward( Complex[,] _inputSignal ) {
			Complex[,] outputSpectrum = new Complex[_inputSignal.GetLength(0),_inputSignal.GetLength(1)];
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
		public void	FFT_Forward( Complex[,] _inputSignal, Complex[,] _outputSpectrum ) {
			if ( _inputSignal.GetLength(0) != m_size || _inputSignal.GetLength(1) != m_size )
				throw new Exception( "Input signal size doesn't match value passed to Init()!" );
			if ( _outputSpectrum.GetLength(0) != m_size || _outputSpectrum.GetLength(1) != m_size )
				throw new Exception( "Output spectrum size doesn't match value passed to Init()!" );

			FFT_CPUInOut( _inputSignal, _outputSpectrum, -1.0f );
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
		public Complex[,]	FFT_Inverse( Complex[,] _inputSpectrum ) {
			Complex[,] outputSignal = new Complex[_inputSpectrum.GetLength(0),_inputSpectrum.GetLength(1)];
			FFT_Inverse( _inputSpectrum, outputSignal );
			return outputSignal;
		}

		/// <summary>
		/// Applies the Inverse Discrete Fourier Transform to an input frequency spectrum
		/// </summary>
		/// <param name="_inputSpectrum">The input complex-valued spectrum of amplitudes for each frequency</param>
		/// <param name="_outputSignal">The output signal of complex-valued amplitudes for each time sample</param>
		/// <remarks>Throws an exception if signal and spectrum sizes mismatch and if their size are not a power of two!</remarks>
		public void	FFT_Inverse( Complex[,] _inputSpectrum, Complex[,] _outputSignal ) {
			if ( _inputSpectrum.GetLength(0) != m_size || _inputSpectrum.GetLength(1) != m_size )
				throw new Exception( "Input spectrum size doesn't match value passed to Init()!" );
			if ( _outputSignal.GetLength(0) != m_size || _outputSignal.GetLength(1) != m_size )
				throw new Exception( "Output signal size doesn't match value passed to Init()!" );

			FFT_CPUInOut( _inputSpectrum, _outputSignal, 1.0f );
		}

		#endregion

		#region Common Code

		private void	FFT_CPUInOut( Complex[,] _input, Complex[,] _output, float _sign ) {
			try {
				//////////////////////////////////////////////////////////////////////////
				// Load initial content
				PixelsBuffer			loadingBuffer = m_texBufferCPU.MapWrite( 0, 0 );
				System.IO.BinaryWriter	W = loadingBuffer.OpenStreamWrite();
				for ( int Y=0; Y < m_size; Y++ ) {
					for ( int X=0; X < m_size; X++ ) {
						W.Write( (float) _input[X,Y].r );
						W.Write( (float) _input[X,Y].i );
					}
				}
				loadingBuffer.CloseStream();
				m_texBufferCPU.UnMap( loadingBuffer );
				m_texBufferIn.CopyFrom( m_texBufferCPU );

				//////////////////////////////////////////////////////////////////////////
				// Apply multiple shader passes
				FFT_GPUInOut( _sign );

				//////////////////////////////////////////////////////////////////////////
				// Read back content
				m_texBufferCPU.CopyFrom( m_texBufferOut );

				PixelsBuffer			resultBuffer = m_texBufferCPU.MapRead( 0, 0 );
				System.IO.BinaryReader	R = resultBuffer.OpenStreamRead();
				for ( int Y=0; Y < m_size; Y++ ) {
					for ( int X=0; X < m_size; X++ ) {
						_output[X,Y].Set( R.ReadSingle(), R.ReadSingle() );
					}
				}
				resultBuffer.CloseStream();
				m_texBufferCPU.UnMap( resultBuffer );

			} catch ( Exception _e ) {
				throw new Exception( "An error occurred while performing the FFT!", _e );
			}
		}

		/// <summary>
		/// Directly applies the FFT to the input buffer
		/// </summary>
		/// <param name="_sign">Use -1 for forward FFT (temporal->frequential) and +1 for backward FFT (frequential->temporal)</param>
		public void	FFT_GPUInOut( float _sign ) {
			try {
				m_CB.m._sign = _sign;
				m_CB.m._bitReversalShift = (uint) (32 - m_POT);
				m_CB.m._normalizationFirstPass = _sign < 0.0f && m_POT == 4 ? 1.0f / (m_size*m_size) : 1.0f;
				m_CB.UpdateData();

				if ( !m_CS__1to16.Use() )
					throw new Exception( "Failed to use compute shader: did it compile without error?" );

				m_texBufferIn.SetCS( 0 );
				m_texBufferOut.SetCSUAV( 0 );

				// • We're using groups of 8x8 threads
				// • Each thread reads and writes 4 values
				// ==> The total amount of elements processed by a group is thus 16x16
				uint	groupsCount = (uint) (m_size >> 4);
				m_CS__1to16.Dispatch( groupsCount, groupsCount, 1 );

				m_texBufferIn.RemoveFromLastAssignedSlots();
				m_texBufferOut.RemoveFromLastAssignedSlotUAV();

				// Process remaining sizes
				if ( !m_CS__Generic.Use() )
					throw new Exception( "Failed to use compute shader: did it compile without error?" );

				uint	stagesCount = m_POT - 4;
				uint	groupSizePOT = 4U;
				float	frequency = _sign * 2.0f * (float) Math.PI / 16.0f;
				for ( int stageIndex=0; stageIndex < stagesCount; stageIndex++ ) {
					groupsCount >>= 1;
					frequency *= 0.5f;
					groupSizePOT++;

					// Swap in and out
					SwapBuffers();

					m_texBufferIn.SetCS( 0 );
					m_texBufferOut.SetCSUAV( 0 );

					m_CB.m._groupFrequency = frequency;
					m_CB.m._groupSize = 1U << (int) groupSizePOT;
					m_CB.m._groupSizePOT = groupSizePOT;
					m_CB.m._normalizationFinal = _sign < 0.0f && groupsCount == 1 ? 1.0f / (m_size*m_size) : 1.0f;
					m_CB.UpdateData();

					m_CS__Generic.Dispatch( groupsCount, groupsCount, 1 );

					m_texBufferIn.RemoveFromLastAssignedSlots();
					m_texBufferOut.RemoveFromLastAssignedSlotUAV();
				}

			} catch ( Exception _e ) {
				throw new Exception( "An error occurred while performing the FFT!", _e );
			}
		}

		/// <summary>
		/// Swaps in & out buffers
		/// </summary>
		public void		SwapBuffers() {
			Texture2D	temp = m_texBufferIn;
			m_texBufferIn = m_texBufferOut;
			m_texBufferOut = temp;
		}

		#endregion
	}
}
