//////////////////////////////////////////////////////////////////////////
// Multiple-Scattering BRDF class implementation
//////////////////////////////////////////////////////////////////////////
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;

namespace LTCTableGenerator
{
	/// <summary>
	/// GGX implementation of the BRDF interface
	/// </summary>
	class MSBRDF : IBRDF {

		int			W, H;
		float[,]	m_Eo;

		/// <summary>
		/// Builds the MSBRDF table from an irradiance table
		/// </summary>
		/// <param name="_Eo">The irradiance table</param>
		public MSBRDF( float[,] _Eo ) {
			m_Eo = _Eo;
			W = m_Eo.GetLength( 0 );
			H = m_Eo.GetLength( 1 );
		}

		public double	Eval( ref float3 _tsView, ref float3 _tsLight, float _alpha, out double _pdf ) {
			if ( _tsView.z <= 0 ) {
				_pdf = 0;
				return 0;
			}

 			float	NdotL = Math.Max( 0, _tsLight.z );

			// Cosine-weighted hemisphere sampling
			_pdf = NdotL / Math.PI;

			float	Eo = SampleEo( NdotL, _alpha );
			float	res = (1.0f - Eo) * NdotL;

			return res;
		}

		public void	GetSamplingDirection( ref float3 _tsView, float _alpha, float _U1, float _U2, ref float3 _direction ) {
			// Performs uniform sampling of the unit disk.
			// Ref: PBRT v3, p. 777.
			float	r = Mathf.Sqrt( _U1 );
			float	phi = Mathf.TWOPI * _U2;

			// Performs cosine-weighted sampling of the hemisphere.
			// Ref: PBRT v3, p. 780.
			_direction.x = r * Mathf.Cos( phi );
			_direction.y = r * Mathf.Sin( phi );

//			_direction.z = Mathf.Sqrt( 1 - r*r );	// Project the point from the disk onto the hemisphere.
			_direction.z = Mathf.Sqrt( 1 - _U1 );	// Project the point from the disk onto the hemisphere.
		}

		float	SampleEo( float _cosTheta, float _alpha ) {
			float	X = Mathf.Saturate( _cosTheta ) * W;
			int		X0 = (int) Mathf.Floor( X );
			float	x = X - X0;
					X0 = Math.Min( W-1, X0 );
			int		X1 = Math.Min( W-1, X0 + 1 );

			float	Y = Mathf.Saturate( _alpha ) * H;
			int		Y0 = (int) Mathf.Floor( Y );
			float	y = Y - Y0;
					Y0 = Math.Min( H-1, Y0 );
			int		Y1 = Math.Min( H-1, Y0 + 1 );

			float	V00 = m_Eo[X0,Y0];
			float	V10 = m_Eo[X1,Y0];
			float	V01 = m_Eo[X0,Y1];
			float	V11 = m_Eo[X1,Y1];
			float	V0 = (1-x) * V00 + x * V10;
			float	V1 = (1-x) * V01 + x * V11;
			float	V = (1-y) * V0 + y * V1;
			return V;
		}
	}
}
