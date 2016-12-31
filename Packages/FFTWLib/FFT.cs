//////////////////////////////////////////////////////////////////////////
// I wrote this helper to avoid remembering how to use that lib every time!
//
//////////////////////////////////////////////////////////////////////////
//
using System;
using System.Runtime.InteropServices;

namespace fftwlib
{
	public class	FFT2D : IDisposable {
		#region NESTED TYPES

		public enum		Normalization
		{
			NONE,								// Leave result as is
			SQUARE_ROOT_OF_DIMENSIONS_PRODUCT,	// Divide result after execution by sqrt(Width*Height)
			DIMENSIONS_PRODUCT,					// Divide result after execution by Width*Height
		}

		#endregion

		#region FIELDS

		int			m_Width;
		int			m_Height;
		IntPtr		m_Input;
		IntPtr		m_Output;
		IntPtr		m_PlanForward;
		IntPtr		m_PlanBackward;

		bool		m_InputIsSpatial = true;

		float[]		m_UserInput;
		GCHandle	m_hUserInput;
		float[]		m_UserOutput;
		GCHandle	m_hUserOutput;

		#endregion

		#region PROPERTIES

		public float[]		Input	{ get { return m_UserInput; } }
		public float[]		Output	{ get { return m_UserOutput; } }
		public bool			InputIsSpatial	{ get { return m_InputIsSpatial; } set { m_InputIsSpatial = value; } }

		#endregion

		#region METHODS

		public FFT2D( int _Width, int _Height ) {
			m_Width = _Width;
			m_Height = _Height;
			m_Input = fftwlib.fftwf.malloc( m_Width*m_Height*2*sizeof(float) );
			m_Output = fftwlib.fftwf.malloc( m_Width*m_Height*2*sizeof(float) );

			m_UserInput = new float[2*m_Width*m_Height];
            m_hUserInput = GCHandle.Alloc( m_UserInput, GCHandleType.Pinned );

			m_UserOutput = new float[2*m_Width*m_Height];
            m_hUserOutput = GCHandle.Alloc( m_UserOutput, GCHandleType.Pinned );
		}

		#region IDisposable Members

		public void Dispose() {
			// Free willy
			m_hUserInput.Free();
			m_hUserOutput.Free();
     		fftwlib.fftwf.free( m_Input );
			fftwlib.fftwf.free( m_Output );
			if ( m_PlanForward != IntPtr.Zero )
				fftwlib.fftwf.destroy_plan( m_PlanForward );
			if ( m_PlanBackward != IntPtr.Zero )
				fftwlib.fftwf.destroy_plan( m_PlanBackward );
		}

		#endregion

		//////////////////////////////////////////////////////////////////////////
		// Input
		public delegate void	SetValueSpatialDelegate( int x, int y, out float r, out float i );
		public void FillInputSpatial( SetValueSpatialDelegate _SetValue ) {
			m_InputIsSpatial = true;
			for ( int Y=0; Y < m_Height; Y++ )
				for ( int X=0; X < m_Width; X++ )
					_SetValue( X, Y, out m_UserInput[2*(m_Width*Y+X)+0], out m_UserInput[2*(m_Width*Y+X)+1] );
		}

		public delegate void	SetValueFrequencyDelegate( int x, int y, out float r, out float i );
		public void FillInputFrequency_UseNegativeFrequencies( SetValueFrequencyDelegate _SetValue ) {
			m_InputIsSpatial = false;
			int	halfWidth = m_Width / 2;
			int	halfHeight = m_Height / 2;
			for ( int Y=0; Y < m_Height; Y++ ) {
				int	Fy = Y < halfHeight ? Y : Y-m_Height+1;		// Negative frequencies are stored in the second half of the array
				for ( int X=0; X < m_Width; X++ ) {
					int	Fx = X < halfWidth ? X : X-m_Width+1;	// Negative frequencies are stored in the second half of the array
					_SetValue( Fx, Fy, out m_UserInput[2*(m_Width*Y+X)+0], out m_UserInput[2*(m_Width*Y+X)+1] );
				}
			}
		}
		public void FillInputFrequency( SetValueFrequencyDelegate _SetValue ) {
			m_InputIsSpatial = false;
			for ( int Y=0; Y < m_Height; Y++ ) {
				for ( int X=0; X < m_Width; X++ ) {
					_SetValue( X, Y, out m_UserInput[2*(m_Width*Y+X)+0], out m_UserInput[2*(m_Width*Y+X)+1] );
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		// Execution
		public void	Execute( Normalization _Normalization ) {
			if ( m_Height == 1 ) {
				if ( m_InputIsSpatial && m_PlanForward == IntPtr.Zero )
					m_PlanForward = fftwlib.fftwf.dft_1d( m_Width, m_Input, m_Output, fftwlib.fftw_direction.Forward, fftwlib.fftw_flags.DestroyInput );
				else if ( !m_InputIsSpatial && m_PlanBackward == IntPtr.Zero )
					m_PlanBackward = fftwlib.fftwf.dft_1d( m_Width, m_Input, m_Output, fftwlib.fftw_direction.Backward, fftwlib.fftw_flags.DestroyInput );
			} else {
				if ( m_InputIsSpatial && m_PlanForward == IntPtr.Zero )
					m_PlanForward = fftwlib.fftwf.dft_2d( m_Width, m_Height, m_Input, m_Output, fftwlib.fftw_direction.Forward, fftwlib.fftw_flags.DestroyInput );
				else if ( !m_InputIsSpatial && m_PlanBackward == IntPtr.Zero )
					m_PlanBackward = fftwlib.fftwf.dft_2d( m_Width, m_Height, m_Input, m_Output, fftwlib.fftw_direction.Backward, fftwlib.fftw_flags.DestroyInput );
			}

			// Copy source data to FFTW memory
            Marshal.Copy( m_UserInput, 0, m_Input, m_Width*m_Height*2 );

			// FFT
			fftwlib.fftwf.execute( m_InputIsSpatial ? m_PlanForward : m_PlanBackward );

			// Retrieve results
			Marshal.Copy( m_Output, m_UserOutput, 0, m_Width*m_Height*2 );

			if ( _Normalization != Normalization.NONE ) {
				float	Normalizer = _Normalization == Normalization.DIMENSIONS_PRODUCT ? 1.0f / (m_Width * m_Height) : 1.0f / (float) Math.Sqrt(m_Width * m_Height);
				for ( int i=0; i < 2*m_Width*m_Height; i++ )
					m_UserOutput[i] *= Normalizer;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		// Output
		public delegate void	GetValueDelegate( int x, int y, float r, float i );
		public void GetOutput( GetValueDelegate _GetValue )
		{
			for ( int Y=0; Y < m_Height; Y++ )
				for ( int X=0; X < m_Width; X++ )
					_GetValue( X, Y, m_UserOutput[2*(m_Width*Y+X)+0], m_UserOutput[2*(m_Width*Y+X)+1] );
		}

		public void SwapInputOutput()
		{
			float	Temp;
			for ( int i=0; i < 2*m_Width*m_Height; i++ )
			{
				Temp = m_UserOutput[i];
				m_UserOutput[i] = m_UserInput[i];
				m_UserInput[i] = Temp;
			}
		}

		#endregion
	}
}