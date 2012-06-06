using System;
using System.Collections.Generic;
using System.Text;

using WMath;

namespace SphericalHarmonics
{
	/// <summary>
	/// This class hosts a collection of SH coefficients from directions homogeneously spaced on a sphere
	/// </summary>
	public class SHSamplesCollection : IEnumerable<SHSamplesCollection.SHSample>
	{
		#region NESTED TYPES

		public class	SHSample
		{
			#region	FIELDS

			public Vector		m_Direction = new Vector();
			public float		m_Phi = 0.0f;
			public float		m_Theta = 0.0f;
			public double[]		m_SHFactors = null;

			#endregion

			#region	METHODS

			public	SHSample( float _fPhi, float _fTheta )
			{
				m_Direction = SHFunctions.SphericalToCartesian( _fTheta, _fPhi );
				m_Phi = _fPhi;
				m_Theta = _fTheta;
			}

			#endregion
		};

		public class	Enumerator : IEnumerator<SHSample>
		{
			#region FIELDS

			protected SHSample[]	m_Collection = null;
			protected int			m_Index = -1;

			#endregion

			#region PROPERTIES

			#region IEnumerator<SHSample> Members

			public SHSample Current
			{
				get { return m_Collection[m_Index]; }
			}

			#endregion

			#region IEnumerator Members

			object System.Collections.IEnumerator.Current
			{
				get { return m_Collection[m_Index]; }
			}

			public bool MoveNext()
			{
				m_Index++;

				return	m_Index < m_Collection.Length;
			}

			public void Reset()
			{
				m_Index = -1;
			}

			#endregion

			#endregion

			#region METHODS

			public	Enumerator( SHSample[] _Collection )
			{
				m_Collection = _Collection;
			}

			#region IDisposable Members

			public void Dispose()
			{
			}

			#endregion

			#endregion
		}

		/// <summary>
		/// The function delegate used by the Project() method to encode a radial function into SH basis using this set of samples
		/// </summary>
		/// <param name="_Direction">The normalized sampling direction</param>
		/// <param name="_Phi">The Phi angle of the sampling direction</param>
		/// <param name="_Theta">The Theta angle of the sampling direction</param>
		/// <returns></returns>
		public delegate double	FunctionDelegate( WMath.Vector _Direction, double _Phi, double _Theta );

		#endregion

		#region FIELDS

		private int			m_Order = 0;
		private int			m_ThetaSamplesCount = 0;
		private int			m_SamplesCount = 0;

		private int			m_RandomSeed = 1;
		private Random		m_Random = null;

		private SHSample[]	m_SHSamples = null;

		#endregion

		#region PROPERTIES

		public int			Order
		{
			get { return m_Order; }
		}

		public int			CoefficientsCount
		{
			get { return m_Order * m_Order; }
		}

		public int			ThetaSamplesCount
		{
			get { return m_ThetaSamplesCount; }
		}

		public int			SamplesCount
		{
			get { return m_SamplesCount; }
		}

		public SHSample[]	Samples
		{
			get { return m_SHSamples; }
		}

		#endregion

		#region METHODS

		public				SHSamplesCollection()
		{
		}

		public				SHSamplesCollection( int _RandomSeed )
		{
			m_RandomSeed = _RandomSeed;
		}

		#region IEnumerable<SHSample> Members

		public IEnumerator<SHSample> GetEnumerator()
		{
			return new Enumerator( m_SHSamples );
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new Enumerator( m_SHSamples );
		}

		#endregion

		public void			Initialize( int _Order, int _ThetaSamplesCount )
		{
			Initialize( _Order, _ThetaSamplesCount, new Vector( 0, 1, 0 ) );
		}

		/// <summary>
		/// Initializes the collection of samples
		/// </summary>
		/// <param name="_Order">The order of the SH</param>
		/// <param name="_ThetaSamplesCount">The amount of samples on Theta (total samples count will be 2*N*N)</param>
		/// <param name="_Up">The vector to use as the Up direction (use [0,1,0] if not sure)</param>
		public void			Initialize( int _Order, int _ThetaSamplesCount, WMath.Vector _Up )
		{
			m_Order = _Order;
			m_ThetaSamplesCount = _ThetaSamplesCount;
			m_SamplesCount = 2 * m_ThetaSamplesCount * m_ThetaSamplesCount;
			m_Random = new Random( m_RandomSeed );

			// Compute the rotation matrix
			Matrix3x3	Rotation = new Matrix3x3();
			Vector		Ortho = new Vector( 0, 1, 0 ) ^ _Up;
			float		fNorm = Ortho.SquareMagnitude();
			if ( fNorm < 1e-6f )
				Rotation.MakeIdentity();
			else
			{
				Ortho /= (float) Math.Sqrt( fNorm );
				Rotation.SetRow0( Ortho );
				Rotation.SetRow1( _Up );
				Rotation.SetRow2( Ortho ^ _Up );
			}

			// Initialize the SH samples
			m_SHSamples = new SHSample[m_SamplesCount];

			// Build the samples using stratified sampling
			int	SampleIndex = 0;
			for ( int ThetaIndex=0; ThetaIndex < m_ThetaSamplesCount; ThetaIndex++ )
			{
				for ( int PhiIndex=0; PhiIndex < 2 * m_ThetaSamplesCount; PhiIndex++ )
				{
					double	fTheta = 2.0 * System.Math.Acos( System.Math.Sqrt( 1.0 - (ThetaIndex + m_Random.NextDouble()) / m_ThetaSamplesCount ) );
					double	fPhi = System.Math.PI * (PhiIndex + m_Random.NextDouble()) / m_ThetaSamplesCount;

					// Compute direction, rotate it then cast it back to sphercial coordinates
					Vector	Direction = SphericalHarmonics.SHFunctions.SphericalToCartesian( fTheta, fPhi );
							Direction *= Rotation;

					SphericalHarmonics.SHFunctions.CartesianToSpherical( Direction, out fTheta, out fPhi );

					// Fill up the new sample
					m_SHSamples[SampleIndex] = new SHSample( (float) fPhi, (float) fTheta );
					m_SHSamples[SampleIndex].m_SHFactors = new double[m_Order * m_Order];

					// Build the SH Factors
					SHFunctions.InitializeSHCoefficients( m_Order, fTheta, fPhi, m_SHSamples[SampleIndex].m_SHFactors );

					SampleIndex++;
				}
			}
		}

		/// <summary>
		/// Projects a function into SH basis
		/// </summary>
		/// <param name="_Function">The function delegate used to evaluate the function for each sampling direction</param>
		/// <returns>The SH coefficients best encoding the provided function</returns>
		public double[]	Project( FunctionDelegate _Function )
		{
			int			SqOrder = CoefficientsCount;
			double[]	Result = new double[SqOrder];

			// Integrate function
			double		NormalizationFactor = 4.0 * Math.PI / m_SamplesCount;
			foreach ( SHSample Sample in m_SHSamples )
				for ( int l=0;l < SqOrder; l++ )
					Result[l] += NormalizationFactor * _Function( Sample.m_Direction, Sample.m_Phi, Sample.m_Theta ) * Sample.m_SHFactors[l];

			return Result;
		}

		#endregion
	}
}
