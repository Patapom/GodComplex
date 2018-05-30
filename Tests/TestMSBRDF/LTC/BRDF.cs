//////////////////////////////////////////////////////////////////////////
// Fitter class for Linearly-Transformed Cosines
// From "Real-Time Polygonal-Light Shading with Linearly Transformed Cosines" (https://eheitzresearch.wordpress.com/415-2/)
// This is a C# re-implementation of the code provided by Heitz et al.
//////////////////////////////////////////////////////////////////////////
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;

namespace TestMSBRDF.LTC
{
	public interface	IBRDF {
		// evaluation of the cosine-weighted BRDF
		// pdf is set to the PDF of sampling L
		float	Eval( ref float3 _tsView, ref float3 _tsLight, float _alpha, out float _pdf );

		// Gets a sampling direction
		void	GetSamplingDirection( ref float3 _tsView, float _alpha, float _U1, float _U2, ref float3 _direction );
	};

	/// <summary>
	/// GGX implementation of the BRDF interface
	/// </summary>
	class BRDF_GGX : IBRDF {

		public float	Eval( ref float3 _tsView, ref float3 _tsLight, float _alpha, out float _pdf ) {
			if ( _tsView.z <= 0 ) {
				_pdf = 0;
				return 0;
			}

			// masking
			float	a_V = 1.0f / _alpha / Mathf.Tan(Mathf.Acos(_tsView.z));
			float	lambdaV = _tsView. z < 1.0f ? 0.5f * (-1.0f + Mathf.Sqrt(1.0f + 1.0f / (a_V*a_V))) : 0.0f;
			float	G1 = 1.0f / (1.0f + lambdaV);

			// shadowing
			float	G2;
			if ( _tsLight.z <= 0.0f ) {
				G2 = 0;
			} else {
				float	a_L = 1.0f / _alpha / Mathf.Tan(Mathf.Acos(_tsLight.z));
				float	lambdaL = _tsLight.z < 1.0f ? 0.5f * (-1.0f + Mathf.Sqrt(1.0f + 1.0f/a_L/a_L)) : 0.0f;
				G2 = 1.0f / (1.0f + lambdaV + lambdaL);
			}

			// D
			float3	H = (_tsView + _tsLight).Normalized;
			float	slopex = H.x / H.z;
			float	slopey = H.y / H.z;
			float	D = 1.0f / (1.0f + (slopex*slopex+slopey*slopey) / (_alpha*_alpha));
			D = D*D;
			D = D / (Mathf.PI * _alpha * _alpha * H.z*H.z*H.z*H.z);

			_pdf = Mathf.Abs( D * H.z / 4.0f / _tsView.Dot(H) );
			float	res = D * G2 / (4.0f * _tsView.z);

			return res;
		}

		public void	GetSamplingDirection( ref float3 _tsView, float _alpha, float _U1, float _U2, ref float3 _direction ) {
			float	phi = Mathf.TWOPI * _U1;
			float	r = _alpha * Mathf.Sqrt( _U2 / (1.0f - _U2) );
			float3	N = new float3( r*Mathf.Cos(phi), r*Mathf.Sin(phi), 1.0f ).Normalized;
			_direction = -_tsView + 2.0f * N * N.Dot(_tsView);
		}
	}

}
