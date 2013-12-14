using System;
using System.Runtime.InteropServices;

namespace fftwlib
{
	#region Single Precision
	/// <summary>
	/// To simplify FFTW memory management
	/// </summary>
	public abstract class fftwf_complexarray
	{
		private IntPtr handle;
		public IntPtr Handle
		{get {return handle;}}

		private int length;
		public int Length
		{get {return length;}}

		/// <summary>
		/// Creates a new array of complex numbers
		/// </summary>
		/// <param name="length">Logical length of the array</param>
		public fftwf_complexarray(int length)
		{
			this.length = length;
			this.handle = fftwf.malloc(this.length*8);
		}

		/// <summary>
		/// Creates an FFTW-compatible array from array of floats, initializes to single precision only
		/// </summary>
		/// <param name="data">Array of floats, alternating real and imaginary</param>
		public fftwf_complexarray(float[] data)
		{
			this.length = data.Length/2;
			this.handle = fftwf.malloc(this.length*8);
			Marshal.Copy(data,0,handle,this.length*2);
		}

		~fftwf_complexarray()
		{
			fftwf.free(handle);
		}
	}
	#endregion
}